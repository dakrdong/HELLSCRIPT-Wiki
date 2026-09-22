using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    public sealed partial class CombatSimulation
    {internal bool FixedTrainingConditions {get;set;}}
    [Serializable] public sealed class TrainingAttempt
    {
        public string runId;
        public BuildConfig build;
        public CombatStatistics statistics;
        public float seconds,realSeconds,health,maxHealth;
        public bool equipmentRecorded;
        public List<Item> equipment;
        public HuntEdictV2Document edict;
    }
    [Serializable] public sealed class TrainingComparisonRecord
    {
        public int version,training;
        public string id,heroId,baselineHeroJson,environmentJson;
        public uint seed,startRng;
        public long completedUtc;
        public TrainingAttempt a,b;
        public RuneGrowthState runes;
        public bool useEdict;
        public List<string> changes=new List<string>();
        public bool Supported=>ValidBaseline()&&(version==1||version==2&&runes!=null&&a?.equipmentRecorded==true&&b?.equipmentRecorded==true&&
            TrainingEquipmentSnapshot.Valid(a.equipment)&&TrainingEquipmentSnapshot.Valid(b.equipment)&&(!useEdict||a.edict!=null&&b.edict!=null))&&
            !string.IsNullOrEmpty(id)&&a?.statistics?.version==CombatTelemetry.Version&&b?.statistics?.version==CombatTelemetry.Version;
        bool ValidBaseline()
        {
            if(a?.build==null||b?.build==null||training<0||training>2||string.IsNullOrEmpty(baselineHeroJson))return false;
            try
            {
                var hero=JsonUtility.FromJson<HeroSave>(baselineHeroJson);
                return hero!=null&&hero.id==heroId&&hero.inventory!=null&&(int)hero.heroClass>=0&&(int)hero.heroClass<3;
            }
            catch(ArgumentException){return false;}
        }
        public bool Complete=>Supported&&Finished(a)&&Finished(b);
        public List<Item> Equipment(bool second)=>version==1?TrainingEquipmentSnapshot.Capture(JsonUtility.FromJson<HeroSave>(baselineHeroJson)):
            (second?b:a).equipment.Select(RuneGrowth.Copy).ToList();
        public bool EquipmentChanged=>Supported&&TrainingEquipmentSnapshot.Signature(Equipment(false))!=TrainingEquipmentSnapshot.Signature(Equipment(true));
        public HeroSave AttemptHero(bool second)
        {
            var hero=TrainingEquipmentSnapshot.WithSnapshot(JsonUtility.FromJson<HeroSave>(baselineHeroJson),Equipment(second));
            hero.build=(second?b:a).build.Copy();
            if(version>=2){hero.useEdict=useEdict;hero.edict=(second?b:a).edict?.Copy();}
            return hero;
        }
        public List<string> DescribeChanges(GameCatalog catalog)
        {
            if(version==1)return new List<string>(changes);
            return TrainingComparisonChanges.Describe(AttemptHero(false),AttemptHero(true),catalog);
        }
        static bool Finished(TrainingAttempt attempt)=>!string.IsNullOrEmpty(attempt.runId)&&attempt.seconds>0&&attempt.statistics.fullRun&&!attempt.statistics.buildChanged&&attempt.statistics.ticks>0&&
            (attempt.statistics.finish==CombatFinish.Duration||attempt.statistics.finish==CombatFinish.HeroDeath||attempt.statistics.finish==CombatFinish.TargetsDefeated);
    }
    public sealed class TrainingComparisonSession
    {
        public const string RuneLockedMessage="A/B 비교 중에는 룬 배치를 고정합니다. 비교를 마친 뒤 룬을 변경해 주세요.";
        [Serializable] sealed class Conditions
        {public string heroId;public HeroClass heroClass;public int level,xp;public List<Item> inventory;public bool useEdict;public HuntEdictV2Document edict;}
        [Serializable] sealed class EnvironmentSnapshot
        {public int training,stage;public uint rng,rewardRng;public Vector2 position;public RiftLayout layout;public List<EnemyState> enemies;}
        readonly GameCatalog catalog;
        readonly string baselineHeroJson;
        readonly TrainingEquipmentBaseline equipment;
        string conditions;
        readonly int training;
        readonly uint seed;
        string environmentJson,currentRunId;
        uint startRng;
        bool currentIsB;
        TrainingAttempt resultA,resultB;
        TrainingComparisonRecord completed;
        public CombatSimulation Current {get;private set;}
        public bool IsB=>currentIsB;
        public int Training=>training;
        public bool HasA=>resultA!=null;
        public bool HasB=>resultB!=null;
        public string HeroId=>Hero.id;
        public HeroSave Hero=>Copy<HeroSave>(baselineHeroJson);
        public RuneGrowthState Runes=>equipment.Runes;
        public IReadOnlyList<TrainingEquipmentCandidate> EquipmentCandidates=>equipment.Candidates;
        public HeroSave PreviewEquipment(IEnumerable<string> ids,IReadOnlyList<int> positions=null)=>equipment.WithEquipment(ids,positions);
        public TrainingAttempt A=>Copy(resultA);
        public TrainingAttempt B=>Copy(resultB);
        static T Copy<T>(T value) where T:class=>value==null?null:JsonUtility.FromJson<T>(JsonUtility.ToJson(value));
        static T Copy<T>(string json) where T:class=>JsonUtility.FromJson<T>(json);
        static string ConditionKey(HeroSave hero)=>JsonUtility.ToJson(new Conditions{heroId=hero.id,heroClass=hero.heroClass,level=hero.level,xp=hero.xp,inventory=hero.inventory.OrderBy(i=>i.id,StringComparer.Ordinal).ToList(),useEdict=hero.useEdict,edict=hero.edict});
        static string EnvironmentKey(RunState r)=>JsonUtility.ToJson(new EnvironmentSnapshot{training=r.training,stage=r.stage,rng=r.rng,rewardRng=r.rewardRng,position=r.position,layout=r.layout,enemies=r.enemies});
        public TrainingComparisonSession(AccountSave account,GameCatalog catalog,int training)
        {
            if(!ContentUnlocks.Has(account,ContentUnlocks.Train))throw new InvalidOperationException(ContentUnlocks.Condition(ContentUnlocks.Train));
            if(training<0||training>2)throw new ArgumentOutOfRangeException(nameof(training));
            if(account.suspendedRun!=null&&!string.IsNullOrEmpty(account.suspendedRun.id))throw new InvalidOperationException("진행 중인 균열을 먼저 완료해 주세요.");
            this.catalog=catalog;this.training=training;seed=731010u+(uint)training;
            equipment=new TrainingEquipmentBaseline(account);baselineHeroJson=JsonUtility.ToJson(equipment.Hero);
        }
        void RequireIdle()
        {if(Current!=null&&Current.State.phase!=RunPhase.Cleared&&Current.State.phase!=RunPhase.Failed)throw new InvalidOperationException("현재 훈련을 먼저 마쳐 주세요.");}
        public CombatSimulation StartA()
        {
            RequireIdle();resultA=resultB=null;completed=null;environmentJson=null;currentIsB=false;return Start(Hero);
        }
        public string ValidateB(BuildConfig build,IEnumerable<string> ids=null,HuntEdictV2Document edict=null,IReadOnlyList<int> positions=null)
        {
            if(!BuildEditing.HasPreset(build))return "비교할 행동 설정이 없습니다.";
            HeroSave hero;
            try{hero=ids==null?Hero:equipment.WithEquipment(ids,positions);}
            catch(Exception e){return e.Message;}
            string passiveError=ContentUnlocks.PassiveError(hero,build);if(passiveError!="")return passiveError;
            if(hero.useEdict)
            {
                try
                {
                    var document=HuntEdictV2.Canonical(edict??hero.edict);HuntEdictV2.ValidateReceiver(document,hero,catalog);
                    for(int slot=0;slot<document.slots.Length;slot++)
                    {
                        string id=document.slots[slot];if(id=="")continue;var skill=catalog.skills.Single(s=>s.id==id);
                        // Existing default slots may contain future skills. They remain inactive at
                        // this level; only a newly assigned locked skill is an invalid B edit.
                        if(skill.unlock>hero.level&&id!=hero.edict.slots[slot])return Loc.F("{0}은 Lv.{1}에 해금됩니다. 해당 규칙을 끄고 시험해 주세요.",skill.name,skill.unlock);
                    }
                }
                catch(Exception e){return e.Message;}
                return "";
            }
            var errors=BehaviorRules.Validate(build.Copy(),hero.heroClass);if(errors.Count>0)return errors[0];
            foreach(var rule in build.rules.Where(r=>r.enabled&&r.action==RuleAction.Skill))
                if(catalog.skills[rule.skill].unlock>hero.level)return Loc.F("{0}은 Lv.{1}에 해금됩니다. 해당 규칙을 끄고 시험해 주세요.", catalog.skills[rule.skill].name, catalog.skills[rule.skill].unlock);
            return "";
        }
        public CombatSimulation StartB(BuildConfig build,IEnumerable<string> ids=null,HuntEdictV2Document edict=null,IReadOnlyList<int> positions=null)
        {
            RequireIdle();if(resultA==null)throw new InvalidOperationException("A 훈련 결과를 먼저 확인해 주세요.");
            var selected=ids?.ToArray();string error=ValidateB(build,selected,edict,positions);if(error!="")throw new InvalidOperationException(error);
            var hero=selected==null?Hero:equipment.WithEquipment(selected,positions);hero.build=build.Copy();
            if(hero.useEdict)hero.edict=HuntEdictV2.Canonical(edict??hero.edict);
            currentIsB=true;resultB=null;completed=null;return Start(hero);
        }
        CombatSimulation Start(HeroSave hero)
        {
            var account=new AccountSave{runes=equipment.Runes};account.heroes.Add(hero);account.contentUnlocks.unlocked.Add(ContentUnlocks.Train);
            var sim=new CombatSimulation(account,catalog,1,training,seed:seed,ownedTraining:true){FixedTrainingConditions=true};string environment=EnvironmentKey(sim.State);
            if(environmentJson!=null&&environmentJson!=environment)throw new InvalidOperationException("훈련 배치가 기준과 다릅니다. 새 비교를 시작해 주세요.");
            environmentJson=environment;startRng=sim.State.rng;conditions=ConditionKey(sim.Hero);Current=sim;currentRunId=sim.State.id;return sim;
        }
        public bool AcceptCompleted(CombatSimulation sim,out string error)
        {
            error="";
            if(sim!=Current||sim?.State.id!=currentRunId){error="현재 비교의 훈련 결과가 아닙니다.";return false;}
            if((currentIsB?resultB:resultA)?.runId==currentRunId)return true;
            var run=sim.State;var stats=run.statistics;
            if(run.phase!=RunPhase.Cleared&&run.phase!=RunPhase.Failed){error="훈련이 아직 끝나지 않았습니다.";return false;}
            if(!sim.OwnedTraining||run.training!=training||ConditionKey(sim.Hero)!=conditions){error="기준 캐릭터의 레벨이나 장비가 달라졌습니다. 새 비교를 시작해 주세요.";return false;}
            if(stats==null||stats.version!=CombatTelemetry.Version||!stats.fullRun||stats.ticks==0||stats.buildChanged||!string.IsNullOrEmpty(run.navigationError))
            {error="시작부터 같은 설정으로 기록한 훈련이 아닙니다. 해당 시도를 다시 실행해 주세요.";return false;}
            if(stats.finish!=CombatFinish.Duration&&stats.finish!=CombatFinish.HeroDeath&&stats.finish!=CombatFinish.TargetsDefeated)
            {error="중도 종료한 훈련입니다. 해당 시도를 다시 실행해 주세요.";return false;}
            var result=new TrainingAttempt{runId=run.id,build=run.build.Copy(),statistics=Copy(stats),seconds=run.time,realSeconds=run.realTime,health=run.health,maxHealth=sim.Stats.hp,
                equipmentRecorded=true,equipment=TrainingEquipmentSnapshot.Capture(sim.Hero),edict=sim.Hero.edict?.Copy()};
            if(currentIsB)resultB=result;else resultA=result;
            if(resultA!=null&&resultB!=null)completed=new TrainingComparisonRecord{version=2,id=Guid.NewGuid().ToString("N"),heroId=sim.Hero.id,baselineHeroJson=baselineHeroJson,environmentJson=environmentJson,runes=equipment.Runes,useEdict=sim.Hero.useEdict,
                training=training,seed=seed,startRng=startRng,completedUtc=DateTimeOffset.UtcNow.ToUnixTimeSeconds(),a=Copy(resultA),b=Copy(resultB),changes=BuildEditing.DescribeCombatChanges(resultA.build,resultB.build,catalog,sim.Hero.heroClass)};
            if(completed!=null)completed.changes=completed.DescribeChanges(catalog);
            return true;
        }
        public TrainingComparisonRecord Record()=>Copy(completed);
    }
}
