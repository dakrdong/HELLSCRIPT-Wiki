using System;
using System.Globalization;
using System.Linq;

namespace Hellscript
{
    public enum RepeatOutcome { Same, Next, Lower, Stop }
    public enum RepeatStop { None, Disabled, Cancelled, Success, Failure, FailureLimit, CountLimit, TimeLimit, GoldGoal, EquipmentGoal, StageGoal }
    public enum RepeatBlock { None, Save, Bag, Warehouse, Configuration, Navigation, Potions }

    [Serializable]
    public sealed class RepeatHuntPolicy
    {
        public bool enabled,edict,stopWhenFull;
        public RepeatOutcome success,failure;
        public int failures=3,count,minutes,stage,freeSlots;
        public long gold;
        public float resultSeconds=5;
        public string[] effects=Array.Empty<string>(),sets=Array.Empty<string>();
        public EdictCleanupPolicy cleanup;

        public static RepeatHuntPolicy Compile(BuildConfig build,HuntEdictV2Document document=null)
        {
            if(document==null)return new RepeatHuntPolicy{enabled=build.autoRepeat,success=build.advanceOnWin?RepeatOutcome.Next:RepeatOutcome.Same,
                failure=RepeatOutcome.Same,failures=Math.Max(1,build.stopAfterFailures),stopWhenFull=build.bagPolicy==BagPolicy.Portal};
            var values=HuntEdictV2.Canonical(document).global.ToDictionary(o=>o.id,o=>o.value,StringComparer.Ordinal);
            string V(string id)=>values["repeat."+id];
            int N(string id)=>int.Parse(V(id),CultureInfo.InvariantCulture);
            RepeatOutcome Outcome(string id)=>V(id)=="STOP"?RepeatOutcome.Stop:V(id)=="NEXT"?RepeatOutcome.Next:V(id)=="LOWER"?RepeatOutcome.Lower:RepeatOutcome.Same;
            return new RepeatHuntPolicy{enabled=V("enabled")=="ON",edict=true,success=Outcome("success"),failure=Outcome("failure"),
                failures=N("failures"),count=N("count"),minutes=N("minutes"),gold=N("gold"),stage=N("stage"),freeSlots=N("freeSlots"),
                resultSeconds=float.Parse(V("resultSeconds"),CultureInfo.InvariantCulture),effects=V("effects").Split(',',StringSplitOptions.RemoveEmptyEntries),
                sets=V("sets").Split(',',StringSplitOptions.RemoveEmptyEntries),stopWhenFull=values["bag.stillFull"]=="STOP",cleanup=EdictCleanupPolicy.Compile(values)};
        }
    }

    [Serializable]
    public sealed class RepeatHuntSession
    {
        public int version=1;
        public string id,heroId,runId,completedRunId;
        public int completed,consecutiveFailures,nextStage,cleanupAttempt;
        public double seconds;
        public long gold;
        public float delayElapsed;
        public bool cancelled,cleanupDone;
        public RepeatStop stop;
        public RepeatBlock blocked;
        public string error="";
        public RepeatHuntPolicy policy;
        public EdictCleanupReport cleanup;
        // Exactly one completed result is kept until the next run or an explicit return to town.
        // It is not an active suspended battle and never starts itself when the app opens.
        public RunState pendingResult;
    }

    public static class RepeatHunt
    {
        public static bool Terminal(RunState run)=>run!=null&&(run.phase==RunPhase.Cleared||run.phase==RunPhase.Failed);
        public static RepeatHuntSession Start(RunState run,RepeatHuntPolicy policy,RepeatHuntSession previous=null)
        {
            if(run==null||run.training>=0)return null;
            var s=previous??new RepeatHuntSession{id=Guid.NewGuid().ToString("N"),heroId=run.heroId};
            if(s.heroId!=run.heroId)throw new InvalidOperationException("반복 사냥의 캐릭터가 일치하지 않습니다.");
            s.runId=run.id;s.policy=policy;s.pendingResult=null;s.delayElapsed=0;s.stop=RepeatStop.None;s.blocked=RepeatBlock.None;
            s.cleanupDone=false;s.cleanup=null;s.error="";return s;
        }
        public static void TickClock(RepeatHuntSession s,float seconds)
        {
            if(s==null||s.cancelled||s.stop!=RepeatStop.None||float.IsNaN(seconds)||float.IsInfinity(seconds)||seconds<=0)return;
            s.seconds+=seconds;
            // Time limits never terminate a live battle or alter its rewards.
            if(s.pendingResult!=null&&s.policy.enabled&&s.policy.minutes>0&&s.seconds>=60d*s.policy.minutes)s.stop=RepeatStop.TimeLimit;
        }
        public static bool Complete(RepeatHuntSession s,RunState run,RepeatHuntPolicy policy,int highestClear)
        {
            if(s==null||!Terminal(run)||run.training>=0||s.heroId!=run.heroId||s.runId!=run.id||s.completedRunId==run.id)return false;
            s.policy=policy;s.pendingResult=run;s.completedRunId=run.id;s.completed++;
            s.gold+=Math.Max(0,run.earnedGold);bool won=run.phase==RunPhase.Cleared;
            s.consecutiveFailures=won?0:s.consecutiveFailures+1;
            var outcome=won?policy.success:policy.failure;
            s.nextStage=Math.Max(1,Math.Min(highestClear+1,run.stage+(outcome==RepeatOutcome.Next?1:outcome==RepeatOutcome.Lower?-1:0)));
            s.stop=s.cancelled?RepeatStop.Cancelled:!policy.enabled?RepeatStop.Disabled:
                outcome==RepeatOutcome.Stop?(won?RepeatStop.Success:RepeatStop.Failure):
                s.consecutiveFailures>=policy.failures?RepeatStop.FailureLimit:
                policy.count>0&&s.completed>=policy.count?RepeatStop.CountLimit:
                policy.minutes>0&&s.seconds>=60d*policy.minutes?RepeatStop.TimeLimit:
                policy.gold>0&&s.gold>=policy.gold?RepeatStop.GoldGoal:
                AcquiredGoal(run,policy)?RepeatStop.EquipmentGoal:
                won&&policy.stage>0&&run.stage>=policy.stage?RepeatStop.StageGoal:RepeatStop.None;
            return true;
        }
        static bool AcquiredGoal(RunState run,RepeatHuntPolicy policy)=>run.drops.Any(d=>d.claimed&&d.item!=null&&d.item.acquiredOrder>0&&
            (policy.effects.Contains(d.item.special)||policy.sets.Contains(ItemCatalog.Unique(d.item.special)?.setId??"")));
        public static RepeatBlock SpaceBlock(RepeatHuntSession s,HeroSave hero)
        {
            int free=Economy.FreeSlots(hero);
            return free<s.policy.freeSlots||s.policy.stopWhenFull&&free<=0?RepeatBlock.Bag:RepeatBlock.None;
        }
        public static bool Ready(RepeatHuntSession s)=>s?.pendingResult!=null&&s.stop==RepeatStop.None&&s.blocked==RepeatBlock.None&&!s.cancelled;
        public static void Normalize(AccountSave account)
        {
            var s=account.repeatHunt;
            if(s==null||string.IsNullOrEmpty(s.id)){account.repeatHunt=null;return;}
            if(s.version>1)throw new NotSupportedException("더 새로운 반복 사냥 버전이 필요합니다. 저장 파일은 보존했습니다.");
            if(s.pendingResult!=null&&string.IsNullOrEmpty(s.pendingResult.id))s.pendingResult=null;
            // JsonUtility materializes absent nested classes. Legacy repetition never owns an
            // edict cleanup policy, and no cleanup report exists before a completed transaction.
            if(s.policy!=null&&!s.policy.edict)s.policy.cleanup=null;
            if(!s.cleanupDone)s.cleanup=null;
            if(s.version!=1||!Valid(s.policy)||!account.heroes.Any(h=>h.id==s.heroId)||double.IsNaN(s.seconds)||double.IsInfinity(s.seconds)||s.seconds<0||s.completed<0||s.gold<0||
                float.IsNaN(s.delayElapsed)||float.IsInfinity(s.delayElapsed)||s.delayElapsed<0||s.consecutiveFailures<0||s.cleanupAttempt<0||
                !Enum.IsDefined(typeof(RepeatStop),s.stop)||!Enum.IsDefined(typeof(RepeatBlock),s.blocked)||
                s.pendingResult!=null&&(!Terminal(s.pendingResult)||s.pendingResult.training>=0||s.pendingResult.heroId!=s.heroId||s.pendingResult.id!=s.runId)||
                account.suspendedRun!=null&&(s.pendingResult!=null||account.suspendedRun.id!=s.runId||account.suspendedRun.heroId!=s.heroId))
                throw new System.IO.InvalidDataException("반복 사냥의 저장 상태가 일치하지 않습니다.");
            s.policy.effects??=Array.Empty<string>();s.policy.sets??=Array.Empty<string>();
            if(s.pendingResult!=null)GameStore.NormalizeRun(s.pendingResult);
        }
        static bool Valid(RepeatHuntPolicy p)=>p!=null&&p.failures>0&&p.count>=0&&p.count<=10000&&p.minutes>=0&&p.minutes<=1440&&p.gold>=0&&p.gold<=100000000&&
            p.stage>=0&&p.stage<=1000&&p.freeSlots>=0&&p.freeSlots<=100&&!float.IsNaN(p.resultSeconds)&&p.resultSeconds>=0&&p.resultSeconds<=30&&
            (p.success==RepeatOutcome.Same||p.success==RepeatOutcome.Next||p.success==RepeatOutcome.Stop)&&
            (p.failure==RepeatOutcome.Same||p.failure==RepeatOutcome.Lower||p.failure==RepeatOutcome.Stop)&&
            (p.cleanup==null||p.cleanup.actions?.Length==5&&p.cleanup.actions.All(a=>Enum.IsDefined(typeof(EdictCleanupAction),a))&&Enum.IsDefined(typeof(EdictCleanupTime),p.cleanup.when)&&p.cleanup.effects!=null&&p.cleanup.sets!=null);
    }

    public static class RiftEarnings
    {
        // Count awarded rift gold, not a balance delta: spending, shop sales and offline rewards
        // must not cancel or complete a hunting goal. Chests use the same staged receipt as their reward.
        public static void GrantGold(AccountSave account,RunState run,int amount)
        {
            if(amount<=0||run.training>=0)return;
            int awarded=(int)Math.Min(amount,(long)int.MaxValue-account.gold);
            account.gold+=awarded;run.earnedGold+=awarded;
            CombatJournal.Append(run,"GOLD_GRANTED",Loc.Source("골드 {0} 획득 · 이번 균열 누적 {1}",awarded,run.earnedGold),trigger:"RIFT_REWARD");
        }
    }
}
