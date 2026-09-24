using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    [Serializable] public sealed class CombatJournalEvent
    {
        public int sequence, target=-1, actionId;
        public float seconds, hp, resource;
        public string kind, source, trigger, message;
        public Vector2 position, destination;
        // JsonUtility materializes null inline classes on reload. Zero-or-one arrays preserve absence.
        public DamageEvent[] damageEvents=Array.Empty<DamageEvent>();
        public Rule[] rules=Array.Empty<Rule>();
        public BuildConfig[] builds=Array.Empty<BuildConfig>();
        public HuntEdictV2Document[] edicts=Array.Empty<HuntEdictV2Document>();
    }
    [Serializable] public sealed class CombatJournalData
    {
        public int version=1, sequence, omittedEvents, resumes;
        public long startedUtcMs, completedUtcMs, attempt;
        public string localAccountId, heroId, buildVersion, buildGuid, platform, outcome, finish, trust="client_observed";
        public int saveSchema, itemCatalogVersion, layoutVersion;
        public HeroClass heroClass;
        public int initialLevel, finalLevel, stage, kills, bossesKilled, equipmentCollected, runesAwarded;
        public uint seed;
        public float observedFrom, simulationSeconds, attendanceSeconds, walkingDistance, maxHealth;
        public long earnedGold,priorUnexportedLossCount;
        public bool archived, bossDefeated;
        public BuildConfig initialBuild;
        public HuntEdictV2Document initialEdict;
        public List<Item> initialEquipment=new List<Item>();
        public string runeConfigurationJson;
        public int[] slotLevels;
        public PotionRuntimeState initialPotions;
        public List<DropState> equipmentDrops=new List<DropState>();
        public List<RiftResourceDrop> resourceDrops=new List<RiftResourceDrop>();
        public List<CombatJournalEvent> events=new List<CombatJournalEvent>();
        public List<string> integritySignals=new List<string>();
    }
    public enum CombatRecordFilter { All, Victory, Defeat }
    public static class CombatJournal
    {
        // Detailed events are bounded independently of the exact aggregate counters. Any gap is explicit.
        public const int EventLimit=20000;
        public static T Copy<T>(T value) => JsonUtility.FromJson<T>(JsonUtility.ToJson(value));
        public static string Outcome(RunRecord record)=>record.journal?.outcome??
            (record.review?.phase==RunPhase.Cleared?"victory":record.review?.phase==RunPhase.Failed?"defeat":"unknown");
        public static IEnumerable<RunRecord> Query(IEnumerable<RunRecord> records,CombatRecordFilter filter)
            =>(records??Enumerable.Empty<RunRecord>()).Where(r=>r!=null&&
                (filter==CombatRecordFilter.All||Outcome(r)==(filter==CombatRecordFilter.Victory?"victory":"defeat")))
                .OrderByDescending(r=>r.journal?.completedUtcMs??0).ThenByDescending(r=>r.journal?.attempt??0);
        public static void Append(RunState run,string kind,string message,string source="",string trigger="",int target=-1,int actionId=0,DamageEvent damage=null)
        {
            var journal=run.journal;if(journal==null||run.training>=0||journal.archived)return;
            journal.events.Add(new CombatJournalEvent{sequence=++journal.sequence,seconds=run.time,kind=kind,message=message,
                source=source,trigger=trigger,target=target,actionId=actionId,hp=run.health,resource=run.resource,
                position=run.position,destination=run.destination,damageEvents=damage==null?Array.Empty<DamageEvent>():new[]{Copy(damage)},
                builds=kind=="BUILD_CHANGED"||kind=="HUNT_EDICT_CHANGED"?new[]{run.build.Copy()}:Array.Empty<BuildConfig>()});
            if(journal.events.Count>EventLimit){journal.events.RemoveAt(1);journal.omittedEvents++;}
        }
        public static string PolicyReason(string code)=>code switch
        {
            "READY"=>"조건 충족", "COOLDOWN"=>"재사용 대기중", "RESOURCE"=>"자원 부족", "BUSY"=>"행동 잠김",
            "AUTO_CONDITION"=>"설정한 발동 조건 대기", "POSITIONING"=>"설정한 위치로 이동", "POSITION_FAILED"=>"이동 경로를 확보하지 못함",
            "RANGE_OR_WALL"=>"사거리 또는 시야 제한", "TARGET"=>"유효한 대상 없음", "NOT_FIGHTING"=>"전투 중이 아님", _=>code
        };
        static Dictionary<string,string> skillTerms;
        static Dictionary<string,string> SkillTerms
        {
            get
            {
                if(skillTerms!=null)return skillTerms;
                skillTerms=new Dictionary<string,string>(StringComparer.Ordinal);
                void Add(string source,string english){if(!string.IsNullOrEmpty(source)&&!string.IsNullOrEmpty(english))skillTerms[source]=english;}
                foreach(var skill in ClassSkills.Data.skills){Add(skill.name,skill.nameEn);Add(skill.automatic,skill.automaticEn);}
                foreach(var option in ClassSkills.Data.options)foreach(var choice in option.choices)Add(choice.name,choice.nameEn);
                return skillTerms;
            }
        }
        public static string Line(CombatJournalEvent e)=>$"{e.seconds:000.0}s  {Message(e.message)}";
        public static string Message(string message)=>string.IsNullOrEmpty(message)?message:string.Join("\n",message.Split('\n')
            .Select(line=>Loc.Language=="en"?Loc.StoredText(line,SkillTerms):Loc.StoredText(line)));
        public static string TimeLabel(RunRecord r)=>r.journal?.completedUtcMs>0?
            DateTimeOffset.FromUnixTimeMilliseconds(r.journal.completedUtcMs).ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss zzz"):Loc.T("시간 미기록");
    }

    public sealed partial class CombatSimulation
    {
        readonly Dictionary<string,float> journalDecisionTimes=new Dictionary<string,float>();
        string journalMovementKey="";
        string journalMovementReason,journalMovementTrigger;
        float journalMovementAt=-10;
        Vector2 journalPosition;
        void InitializeJournal(bool restoring,uint seed,bool recordResume)
        {
            if(State.training>=0){State.journal=null;return;}
            if(State.journal==null||State.journal.version==0)
            {
                if(string.IsNullOrEmpty(account.telemetryAccountId))account.telemetryAccountId=Guid.NewGuid().ToString("N");
                State.journal=new CombatJournalData{localAccountId=account.telemetryAccountId,heroId=Hero.id,heroClass=Hero.heroClass,
                    stage=State.stage,attempt=++account.combatSequence,startedUtcMs=restoring?0:DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    observedFrom=State.time,seed=restoring?0:seed,initialLevel=Hero.level,buildVersion=Application.version,buildGuid=Application.buildGUID,saveSchema=GameStore.MaximumSchemaVersion,itemCatalogVersion=ItemCatalog.Version,layoutVersion=RiftLayout.CurrentVersion,platform=Application.platform.ToString(),
                    initialBuild=State.build.Copy(),initialEdict=edictSource?.Copy(),initialEquipment=Hero.inventory.Where(i=>i.equipped).Select(CombatJournal.Copy).ToList(),
                    runeConfigurationJson=JsonUtility.ToJson(account.runes),slotLevels=(int[])State.slotLevels.Clone(),initialPotions=CombatJournal.Copy(State.potions),maxHealth=Stats.hp};
                if(restoring)State.journal.integritySignals.Add("PARTIAL_LEGACY_RUN");
            }
            journalPosition=State.position;
            if(restoring&&recordResume)
            {
                State.journal.resumes++;
                CombatJournal.Append(State,"RESUME",Loc.Source("저장된 균열을 {0:0.0}초 지점부터 이어갑니다.",State.time));
            }
        }
        string JournalTarget(int id)
        {
            var e=State.enemies.FirstOrDefault(x=>x.id==id);
            return e==null?Loc.Source("대상 #{0}",id):Loc.Source("{0} #{1}",e.goblin?GoldenGoblin.Name:e.boss?GameCatalog.BossNames[e.pattern]:GameCatalog.EnemyNames[e.kind],id);
        }
        void JournalDecision(Rule rule,string code,string detail,EnemyState target)
        {
            if(State.training>=0||State.journal==null||code!="SELECTED"&&code!="RESOURCE"&&code!="COOLDOWN"&&code!="RANGE"&&code!="NO_TARGET")return;
            string key=(rule.id??"")+":"+code+":"+(target?.id??-1);
            if(journalDecisionTimes.TryGetValue(key,out float last)&&State.time-last<4)return;
            if(journalDecisionTimes.Count>512)journalDecisionTimes.Clear();journalDecisionTimes[key]=State.time;
            string action=rule.action==RuleAction.Basic?"기본 공격":rule.action==RuleAction.Skill?catalog.skills[rule.skill].name:
                rule.action==RuleAction.Loot?"전리품 회수":rule.action==RuleAction.Chest?"상자 개봉":"탐색·성소";
            CombatJournal.Append(State,"DECISION",Loc.Source(code=="SELECTED"?"{0} 선택: {1} · {2}":"{0} 대기: {1} · {2}",action,detail,target==null?"대상 없음":JournalTarget(target.id)),
                rule.id,code,target?.id??-1);
            State.journal.events.Last().rules=new[]{rule.Copy()};
        }
        void JournalMovement()
        {
            if(State.journal==null)return;
            float distance=Vector2.Distance(journalPosition,State.position);
            bool moving=distance>.001f;string key=(moving?"moving:":"stopped:")+State.movementAction+":"+State.targetId+":"+State.targetRuleId+":"+journalMovementTrigger;
            if((key!=journalMovementKey||moving&&State.time-journalMovementAt>=2)&&State.time-journalMovementAt>=.4f)
            {
                var decision=journalMovementTrigger==null?State.decisions.LastOrDefault(d=>(d.code=="SELECTED"||d.code=="RANGE")&&
                    d.ruleId==State.targetRuleId&&d.targetId==State.targetId&&State.time-d.lastTime<=.5f):null;
                string reason=journalMovementReason??State.action;
                CombatJournal.Append(State,moving?"MOVEMENT":"MOVEMENT_STOP",Loc.Source(moving?"이동: {0} → ({1:0.0}, {2:0.0}) · {3}":"이동 중지: {0} · 위치 ({1:0.0}, {2:0.0}) · {3}",
                    reason,moving?State.destination.x:State.position.x,moving?State.destination.y:State.position.y,decision?.detail??State.action),
                    journalMovementTrigger==null?State.targetRuleId:"",journalMovementTrigger??decision?.code??"NAVIGATION",State.targetId,State.heroAction.id);
                journalMovementKey=key;journalMovementAt=State.time;
            }
            journalPosition=State.position;
        }
        void JournalDamage(DamageEvent damage)
        {
            if(State.journal==null)return;
            string name=JournalAttackName(damage.definitionId);
            string message=damage.incoming?Loc.Source("{0} · {1} 피격: HP {2:0.##} 감소, 보호막 {3:0.##} 흡수 → HP {4:0.##}/{5:0.##}",
                int.TryParse(damage.casterId,out int attackerId)?JournalTarget(attackerId):"적 공격",name,damage.hpLoss,damage.absorbed,State.health,Stats.hp):
                Loc.Source("{0} 적중 → {1}: HP 피해 {2:0.##}{3}",name,JournalTarget(damage.targetId),damage.hpLoss,damage.critical?" · 치명타":"");
            CombatJournal.Append(State,damage.incoming?"HIT_TAKEN":"HIT_DEALT",message,damage.definitionId,damage.kind.ToString(),damage.targetId,damage.rootCastId,damage);
        }
        static string JournalAttackName(string id)
        {
            string name=ClassSkills.Find(id)?.name;if(name!=null)return name;
            foreach(BossAttack attack in Enum.GetValues(typeof(BossAttack)))if(BossCombat.Definition((int)attack)==id)return BossCombat.Name((int)attack);
            if(id?.Length==3&&int.TryParse(id.Substring(1),out int index))
            {
                if(id[0]=='N'&&index>=1&&index<=GameCatalog.EnemyNames.Length)return Loc.Source("{0} 공격",GameCatalog.EnemyNames[index-1]);
                if(id[0]=='E'&&index>=1&&index<=EnemyCombat.TraitNames.Length)return EnemyCombat.TraitNames[index-1];
            }
            if(id!=null&&id.StartsWith("N",StringComparison.Ordinal)&&id.EndsWith("_DEATH",StringComparison.Ordinal)&&
                int.TryParse(id.Substring(1,2),out int dead)&&dead>=1&&dead<=GameCatalog.EnemyNames.Length)
                return GameCatalog.EnemyNames[dead-1]+" · 사망 효과";
            return id=="BASIC"?"기본 공격":id=="ENEMY_ATTACK"?"일반 공격":id=="ENEMY_GROUND"?"적 장판 피해":id??"적 공격";
        }
        void JournalEnemy(EnemyState enemy,string kind,string definition,int action,float value,Vector2 aim)
        {
            if(State.journal==null||State.training>=0)return;
            string message=null;
            if(kind=="PREPARE"||kind=="FOLLOWUP_PREPARE"||kind=="HAZARD_CREATED")
                message=Loc.Source("{0} · {1} 예고: {2:0.0}초 후 · 목표 위치 ({3:0.0}, {4:0.0})",JournalTarget(enemy.id),JournalAttackName(definition),value,aim.x,aim.y);
            else if(kind=="ENRAGED")message=Loc.Source("{0} 격노: 강화된 공격에 주의하세요.",JournalTarget(enemy.id));
            else if(kind.StartsWith("INTERRUPTED:",StringComparison.Ordinal))message=Loc.Source("{0} 공격 중단: {1}",JournalTarget(enemy.id),kind.Substring(12));
            if(message==null)return;
            CombatJournal.Append(State,"ENEMY_"+kind.Split(':')[0],message,definition,kind,enemy.id,action);
            State.journal.events.Last().destination=aim;
        }
        void CompleteJournal(RunRecord record)
        {
            var j=State.journal;if(j==null)return;
            if(j.completedUtcMs==0)j.completedUtcMs=DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            j.outcome=State.phase==RunPhase.Cleared?"victory":"defeat";j.finish=State.statistics.finish.ToString();j.finalLevel=Hero.level;
            j.simulationSeconds=State.time;j.attendanceSeconds=State.realTime;j.earnedGold=State.earnedGold;j.kills=State.kills;
            j.bossDefeated=State.bossRewarded;j.bossesKilled=State.enemies.Count(e=>e.boss&&e.dead);j.equipmentCollected=State.lootCount;
            j.priorUnexportedLossCount=account.combatTelemetryLossCount;
            if(j.priorUnexportedLossCount>0&&!j.integritySignals.Contains("LOCAL_EXPORT_LOSS"))j.integritySignals.Add("LOCAL_EXPORT_LOSS");
            j.runesAwarded=State.runesAwarded;j.walkingDistance=State.moveDistance;
            j.equipmentDrops=State.drops.Select(CombatJournal.Copy).ToList();j.resourceDrops=State.resources.Select(CombatJournal.Copy).ToList();
            var window=record.review?.finalWindow;
            string message=Loc.Source("전투 종료: {0} · {1:0.0}초 · 처치 {2} · 획득 골드 {3}",State.action,State.time,State.kills,State.earnedGold);
            if(window?.fatalSources.Count>0&&State.statistics.finish==CombatFinish.HeroDeath)
            {
                var source=window.fatalSources.OrderByDescending(x=>x.totalHpLoss).First();
                message+="\n"+Loc.Source("마지막 {0:0.0}초 HP 손실 {1:0.##}, 주요 피격원 {2} ({3:0.##})",window.preDeathWindowSeconds,window.hpLost5s,source.sourceName,source.totalHpLoss);
                var blocked=window.blockedSummary.OrderByDescending(x=>x.count).FirstOrDefault();
                if(blocked!=null)message+="\n"+Loc.Source("{0} 대기 사유: {1}",blocked.skillName,CombatStatistics.ReasonName(blocked.reason));
            }
            var summary=j.events.LastOrDefault(e=>e.kind=="RESULT_SUMMARY");
            if(summary==null)CombatJournal.Append(State,"RESULT_SUMMARY",message);else summary.message=message;
            if(!j.integritySignals.Contains("CLIENT_CLOCK_UNVERIFIED"))j.integritySignals.Add("CLIENT_CLOCK_UNVERIFIED");
            if(j.omittedEvents>0&&!j.integritySignals.Contains("EVENT_GAP"))j.integritySignals.Add("EVENT_GAP");
            record.journal=CombatJournal.Copy(j);
        }
    }
}
