using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    // One independently owned simulation. Real seconds are converted to the existing fixed 20 Hz clock.
    // There is deliberately no duration limit, automatic repeat, GameStore or controller dependency.
    public sealed class CombatPreviewSession : IDisposable
    {
        readonly GameCatalog catalog;
        readonly string initialAccount,initialRun;
        readonly List<ClassSkillEvent> observed=new List<ClassSkillEvent>();
        float pending,startTime;
        bool disposed;
        readonly bool starter,puzzle;
        bool waited,moved;
        public string SourceSignature {get;}="";
        public bool IsStarter=>starter;
        public bool IsPuzzle=>puzzle;
        public bool StarterObservationComplete=>starter&&Seconds>=10-.001f&&observed.Any(e=>e.skillId==Scenario.skill&&e.kind=="CAST_START")&&
            (Scenario.preset!="pack"||waited)&&(Scenario.preset!="edge"||moved);

        public SkillPresetScenario Scenario {get;}
        public CombatSimulation Combat {get;private set;}
        public float Speed {get;private set;}=1;
        public bool Paused {get;private set;}
        public bool Ended=>starter&&Seconds>=10-.001f||Combat.State.phase==RunPhase.Cleared||Combat.State.phase==RunPhase.Failed||Combat.State.health<=0;
        public float Seconds=>Combat.State.time-startTime;
        public IReadOnlyList<ClassSkillEvent> Observed=>observed;
        public event Action Reset;
        public CombatPreviewSession(GameCatalog catalog,SkillPresetScenario scenario)
        {
            this.catalog=catalog;Scenario=scenario;
            var prepared=CombatSimulation.CreateSkillPreview(catalog,scenario);
            (initialAccount,initialRun)=prepared.PreviewSnapshot();Restart();
        }
        public CombatPreviewSession(GameCatalog catalog,AccountSave source,string mode)
        {
            this.catalog=catalog;starter=true;SourceSignature=TutorialProgress.Signature(source.Hero);
            var (skill,start,pick)=Tutorials.StarterComparison(source.Hero.heroClass);
            if(mode!=start&&mode!=pick)throw new ArgumentException("Unknown starter comparison mode.");
            Scenario=JsonUtility.FromJson<SkillPresetScenario>(JsonUtility.ToJson(SkillPresetScenario.Find(skill,start)));
            Scenario.preset=mode;Scenario.attackLimit=1;
            var description=SkillPresetScenario.Find(skill,mode);Scenario.conditions=description.conditions;Scenario.conditionsEn=description.conditionsEn;
            var prepared=CombatSimulation.CreateStarterPreview(catalog,source,Scenario);
            (initialAccount,initialRun)=prepared.PreviewSnapshot();Restart();
        }
        public CombatPreviewSession(GameCatalog catalog,AccountSave source,SkillPresetScenario scenario,int puzzleLevel)
        {
            this.catalog=catalog;puzzle=true;Scenario=scenario;SourceSignature=JsonUtility.ToJson(source.Hero);
            var copy=JsonUtility.FromJson<AccountSave>(JsonUtility.ToJson(source));
            if(!copy.Hero.build.classSkills.Equipped(scenario.skill))throw new ArgumentException("Equip this skill before comparing.");
            GameStore.ApplyHuntEdict(copy.Hero,HuntEdictQuickPresets.Apply(HuntEdictLoadout.FromHero(copy.Hero),"skill/"+scenario.skill,scenario.preset));
            var prepared=new CombatSimulation(copy,catalog,1,3,seed:PuzzleTutorial.Seed,recordResume:false,tutorial:true,puzzleLevel:puzzleLevel);
            prepared.State.tutorialReplay=true;if(!prepared.ContinueTutorial())throw new ArgumentException("Learn the level's skills before comparing.");
            (initialAccount,initialRun)=prepared.PreviewSnapshot();Restart();
        }
        public void SetSpeed(float value)
        {if(value!=.5f&&value!=1&&value!=2)throw new ArgumentOutOfRangeException(nameof(value));Speed=starter?1:value;}
        public void SetPaused(bool value)=>Paused=value;
        public void Restart()
        {
            if(disposed)throw new ObjectDisposedException(nameof(CombatPreviewSession));
            if(Combat!=null)Combat.ClassSkillChanged-=Observe;
            Combat=puzzle?new CombatSimulation(JsonUtility.FromJson<AccountSave>(initialAccount),catalog,1,3,JsonUtility.FromJson<RunState>(initialRun),recordResume:false,tutorial:true):CombatSimulation.RestoreSkillPreview(catalog,Scenario,initialAccount,initialRun);startTime=Combat.State.time;
            Combat.ClassSkillChanged+=Observe;pending=0;observed.Clear();waited=moved=false;Paused=false;Reset?.Invoke();
        }
        void Observe(ClassSkillEvent e){observed.Add(e);if(observed.Count>160)observed.RemoveAt(0);}
        public float Advance(float realSeconds,bool visible=true)
        {
            if(disposed||Paused||!visible||Ended)return 0;
            if(!float.IsFinite(realSeconds)||realSeconds<0)throw new ArgumentOutOfRangeException(nameof(realSeconds));
            pending+=Mathf.Min(realSeconds,.25f)*Speed;float before=Combat.State.time;
            while(pending+0.000001f>=CombatSimulation.Step&&!Ended)
            {
                if(starter)
                {string reason=Combat.InspectClassSkillAutomatic(Scenario.skill).reason;if(reason=="AUTO_CONDITION"||reason=="LEGACY_EDICT")waited=true;}
                var position=Combat.State.position;bool casting=Combat.HeroActionBusy;
                Combat.Tick(CombatSimulation.Step);
                if(casting&&Vector2.Distance(position,Combat.State.position)>.0001f)moved=true;
                pending=Mathf.Max(0,pending-CombatSimulation.Step);
            }
            return Combat.State.time-before;
        }
        public string Caption
        {
            get
            {
                if(Ended)return Loc.T(Combat.State.health>0?"예시 전투 완료 · 재실행으로 다시 볼 수 있습니다.":"예시 캐릭터가 쓰러졌습니다. 재실행으로 다시 볼 수 있습니다.");
                var e=observed.LastOrDefault(x=>x.skillId==Scenario.skill&&!x.kind.StartsWith("POLICY_",StringComparison.Ordinal)&&Combat.State.time-x.time<2);
                if(e!=null)return Loc.F("{0} · {1}",ClassSkillTree.Name(e.skillId),Loc.T(EventLabel(e.kind)));
                var readiness=Combat.InspectClassSkillAutomatic(Scenario.skill);
                if(readiness.reason=="AUTO_CONDITION"||readiness.reason=="LEGACY_EDICT")return Loc.F("조건 대기 · {0}",Scenario.Conditions);
                return ClassSkillTree.Display(Loc.Language=="en"?readiness.detailEn:readiness.detail);
            }
        }
        static string EventLabel(string kind)=>kind switch
        {"CAST_START"=>"사용 시작","DAMAGE"=>"피해 적용","RESOURCE"=>"자원 회복","HEAL"=>"체력 회복","PULL"=>"끌어당김","DOT_CREATED"=>"지속 피해 적용","CASHOUT"=>"출혈 회수",
            "PUSH"=>"전선 밀기","BORROW"=>"자원 차입","REPAY"=>"빚 상환","CROSSFIRE"=>"교차 사격",
            "AFTERIMAGE"=>"이동 잔상 생성","ELEMENT_REACTION"=>"속성 흡수 반응","RETURN"=>"서리 귀환",
            "MISSED_PICKUP"=>"서리 회수 실패","RELAY"=>"접지 연쇄 중계",_=>"스킬 효과 진행"};
        public void Dispose()
        {if(disposed)return;disposed=true;Combat.ClassSkillChanged-=Observe;Reset=null;observed.Clear();}
    }
}
