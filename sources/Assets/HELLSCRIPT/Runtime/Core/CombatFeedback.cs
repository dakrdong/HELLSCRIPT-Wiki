using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    public enum CombatActivity { Movement, Attack, Evasion, Farming }
    public sealed class CombatActivitySample
    {
        public float time;
        public readonly float[] seconds=new float[4];
    }
    [Serializable] public sealed class CombatReasonCount { public string code; public int count; }
    [Serializable] public sealed class SkillFeedback
    {
        public string id;
        public int channelTicks,lastChannelRoot=-1;
        public float activeSeconds,healing,shieldAbsorbed,resourceRestored;
        public bool targetsTruncated;
        public List<int> targets=new List<int>();
        public List<CombatReasonCount> checks=new List<CombatReasonCount>(),interruptions=new List<CombatReasonCount>();
        public void Applied(int target)
        {if(target<0||targets.Contains(target))return;if(targets.Count<256)targets.Add(target);else targetsTruncated=true;}
    }
    // Optional aggregate on the existing archive. No decision is re-evaluated to populate it.
    [Serializable] public sealed class CombatFeedback
    {
        public const int Version=2;
        public const int ActivityHistoryLimit=257;
        public int version;
        public float observedFrom;
        public float attackSeconds,evasionSeconds,movementSeconds,lootSeconds,waitingSeconds;
        public bool truncated;
        public List<SkillFeedback> skills=new List<SkillFeedback>();
        [NonSerialized] public List<CombatActivitySample> activityHistory=new List<CombatActivitySample>();
        [NonSerialized] float activityHistoryInterval=1;
        [NonSerialized] CombatActivity? historyActivity;
        public double ObservedSeconds=>attackSeconds+evasionSeconds+movementSeconds+lootSeconds+waitingSeconds;
        public double ActivitySeconds=>movementSeconds+(double)attackSeconds+evasionSeconds+lootSeconds;
        public float[] ActivityDurations()=>new[]{movementSeconds,attackSeconds,evasionSeconds,lootSeconds};
        // Largest remainders keep the four displayed integers at exactly 100, including ties.
        public int[] ActivityPercentages()
        {
            var values=ActivityDurations();double total=values.Sum(v=>float.IsFinite(v)?Math.Max(0,(double)v):0);var result=new int[4];
            if(total<=0)return result;
            var remainder=new double[4];int assigned=0;
            for(int i=0;i<4;i++)
            {double exact=(float.IsFinite(values[i])?Math.Max(0,(double)values[i]):0)/total*100;result[i]=(int)Math.Floor(exact);remainder[i]=exact-result[i];assigned+=result[i];}
            foreach(int i in Enumerable.Range(0,4).OrderByDescending(i=>remainder[i]).ThenBy(i=>i).Take(100-assigned))result[i]++;
            return result;
        }
        public void RecordActivity(CombatActivity? activity,float seconds,float endTime)
        {
            if(!float.IsFinite(seconds)||!float.IsFinite(endTime)||seconds<=0||endTime<seconds)return;
            activityHistory??=new List<CombatActivitySample>();
            if(activityHistory.Count==0)
            {
                activityHistoryInterval=1;
                var start=new CombatActivitySample{time=endTime-seconds};
                start.seconds[0]=movementSeconds;start.seconds[1]=attackSeconds;start.seconds[2]=evasionSeconds;start.seconds[3]=lootSeconds;
                activityHistory.Add(start);
            }
            switch(activity)
            {
                case CombatActivity.Movement:movementSeconds+=seconds;break;
                case CombatActivity.Attack:attackSeconds+=seconds;break;
                case CombatActivity.Evasion:evasionSeconds+=seconds;break;
                case CombatActivity.Farming:lootSeconds+=seconds;break;
                default:waitingSeconds+=seconds;break;
            }
            // Cumulative endpoints keep inactive periods flat, including sub-second activity changes.
            for(float cursor=Mathf.Max(0,endTime-seconds);cursor<endTime;)
            {
                float next=Mathf.Min(endTime,(Mathf.Floor(cursor/activityHistoryInterval)+1)*activityHistoryInterval);
                if(next<=cursor)break;
                var row=activityHistory.LastOrDefault();
                if(activityHistory.Count==1||activityHistoryInterval==1&&historyActivity!=activity||cursor%activityHistoryInterval==0)
                {row=new CombatActivitySample();activityHistory.Add(row);}
                row.time=next;row.seconds[0]=movementSeconds;row.seconds[1]=attackSeconds;row.seconds[2]=evasionSeconds;row.seconds[3]=lootSeconds;
                if(activity.HasValue)row.seconds[(int)activity.Value]-=endTime-next;
                if(activityHistory.Count>ActivityHistoryLimit)
                {
                    // ponytail: coarsen long-run plot samples; totals and the first/latest endpoints stay exact.
                    for(int i=activityHistory.Count-2;i>0;i-=2)activityHistory.RemoveAt(i);
                    activityHistoryInterval*=2;
                }
                historyActivity=activity;
                cursor=next;
            }
        }
        public SkillFeedback For(string id)
        {
            var row=skills.FirstOrDefault(s=>s.id==id);if(row!=null)return row;
            if(skills.Count>=160){truncated=true;return null;}
            row=new SkillFeedback{id=id};skills.Add(row);return row;
        }
        static void Count(List<CombatReasonCount> rows,string code)
        {
            if(string.IsNullOrEmpty(code))code="UNKNOWN";
            var row=rows.FirstOrDefault(r=>r.code==code);
            if(row==null){if(rows.Count>=31)code="OTHER";row=rows.FirstOrDefault(r=>r.code==code);if(row==null){row=new CombatReasonCount{code=code};rows.Add(row);}}
            row.count++;
        }
        public void Check(string id,string code){var row=For(id);if(row!=null)Count(row.checks,code);}
        public void Interrupted(string id,string reason){var row=For(id);if(row!=null)Count(row.interruptions,reason);}
        public static string Reason(string code)=>Loc.T(code switch
        {
            "DISABLED"=>"자동 사용 해제", "EDICT_DISABLED"=>"사냥 칙령 해제", "HIGHER_PRIORITY"=>"상위 행동으로 검사 기회 없음",
            "BUSY" or "ACTION_LOCK" or "CHANNELING"=>"다른 행동 진행 중", "GATHERING"=>"적 모으기 진행 중",
            "AUTO_CONDITION" or "CONDITION"=>"사용 조건 대기", "RESOURCE"=>"자원 부족", "COOLDOWN"=>"재사용 대기중",
            "TARGET" or "NO_TARGET"=>"대상 없음", "RANGE"=>"사거리 밖", "LOS" or "SIGHT"=>"시야 막힘",
            "SELECTED" or "READY"=>"실행 대상으로 선택됨", "APPROACH" or "POSITIONING"=>"사용 위치로 이동 중",
            _=>CombatJournal.PolicyReason(code)
        });
        public static string Summary(SkillFeedback row)
        {
            if(row==null)return Loc.T("원인 집계 기록 없음");
            var checks=row.checks.Where(c=>c.code!="READY"&&c.code!="SELECTED").OrderByDescending(c=>c.count).Take(3);
            string reasons=string.Join(" · ",checks.Select(c=>Loc.F("{0} {1}회",Reason(c.code),c.count)));
            return reasons==""?Loc.T("기록된 사용 방해 없음"):reasons;
        }
    }
    public sealed partial class CombatSimulation
    {
        CombatActivity? activityThisTick;
        static int ActivityPriority(CombatActivity activity)=>activity==CombatActivity.Evasion?3:activity==CombatActivity.Attack?2:activity==CombatActivity.Farming?1:0;
        void ObserveActivity(CombatActivity activity)
        {if(!activityThisTick.HasValue||ActivityPriority(activity)>ActivityPriority(activityThisTick.Value))activityThisTick=activity;}
        void BeginActivityTick()
        {
            activityThisTick=null;
            if(State.heroAction.phase!=HeroActionPhase.Idle&&State.heroAction.escape)ObserveActivity(CombatActivity.Evasion);
            else if(HeroActionBusy)ObserveActivity(CombatActivity.Attack);
            else if(ChestBusy)ObserveActivity(CombatActivity.Farming);
        }
        readonly HashSet<string> feedbackActiveEffects=new HashSet<string>();
        void FeedbackCheck(string id,string reason)=>State.statistics.feedback?.Check(id,reason);
        bool FeedbackSelected(string id,IEnumerable<string> order)
        {
            bool after=false;foreach(string next in order){if(after&&Loadout.automatic.Contains(next))FeedbackCheck(next,"HIGHER_PRIORITY");if(next==id)after=true;}return true;
        }
        void FeedbackGuard(string reason)
        {foreach(string id in Loadout.order)FeedbackCheck(id,Loadout.automatic.Contains(id)?reason:"DISABLED");}
        void RecordFeedbackTick(float seconds)
        {
            var f=State.statistics.feedback;if(f==null||seconds<=0)return;
            f.RecordActivity(activityThisTick,seconds,State.time);
            var active=feedbackActiveEffects;active.Clear();
            foreach(var enemy in State.enemies)
                if(!enemy.dead)foreach(var s in enemy.statuses)if(s.remaining>0&&s.casterId==State.heroId)active.Add(s.definitionId);
            foreach(var s in State.shields)if(s.remaining>0&&s.amount>0)active.Add(s.definitionId);
            if(CS!=null)foreach(var e in CS.effects)if(e.until>State.time&&e.kind=="buff")active.Add(e.id);
            foreach(string id in active){var row=f.For(id);if(row!=null)row.activeSeconds+=seconds;}
        }
        void RecordFeedbackChannelTick(HeroActionState action)
        {
            var row=State.statistics.feedback?.For("W01");if(row==null)return;
            row.channelTicks++;
            // This is an observation only. Do not emit a cast/proc event or change action.released.
            if(row.lastChannelRoot==action.id)return;
            row.lastChannelRoot=action.id;State.statistics.For("W01").releases++;
        }
        void FeedbackEffect(string id,string kind,int target,float value)
        {
            var f=State.statistics.feedback;if(f==null)return;
            if(kind=="STATUS"||kind=="BOSS_STAGGER")f.For(id)?.Applied(target);
            if(kind=="HEAL"||kind=="ABSORB"||kind=="RESOURCE")
            {var row=f.For(id);if(row!=null){if(kind=="HEAL")row.healing+=value;else if(kind=="ABSORB")row.shieldAbsorbed+=value;else row.resourceRestored+=value;}}
            // Legacy actions already pass through CombatTelemetry.Action.
            if(id=="BASIC"||ClassSkills.LegacyIndex(id)>=0||ClassSkills.Find(id)==null)return;
            if(kind=="CAST_START")State.statistics.For(id).starts++;
            if(kind=="CAST_RELEASE")State.statistics.For(id).releases++;
            if(kind=="CAST_END")State.statistics.For(id).completions++;
        }
    }
}
