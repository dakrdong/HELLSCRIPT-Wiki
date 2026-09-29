using System;
using System.Collections.Generic;
using System.Linq;

namespace Hellscript
{
    public sealed partial class CombatSimulation
    {
        // The v0.2 document only drives combat when the hero opts in. Existing saves and the legacy
        // rule editor keep their behaviour untouched until someone turns this on.
        HuntEdictV2Document edictSource;
        ClassSkillLoadout recommendedSkills;
        BuildConfig edictPolicy;
        EdictLootPolicy edictLoot;
        readonly Dictionary<string,Rule> edictMovementRules=new Dictionary<string,Rule>();
        string edictInactiveReason="";

        // What the simulation reads for its global policies. With the edict off this is the live
        // BuildConfig itself, so every existing read keeps its exact behaviour.
        public BuildConfig Policy=>edictPolicy??State.build;
        public RepeatHuntPolicy RepeatPolicy
        {
            get
            {
                var result=RepeatHuntPolicy.Compile(Policy,edictSource);
                // Keep the intended owner visible even if a runtime error deactivated its edict.
                // The controller then blocks repetition instead of silently using legacy settings.
                result.edict|=Hero.useEdict;return result;
            }
        }
        // Navigation failures switch repeat off; the overlay must agree or the controller would restart anyway.
        void DisableAutoRepeat(){State.build.autoRepeat=false;if(edictPolicy!=null)edictPolicy.autoRepeat=false;}

        public bool EdictActive=>edictSource!=null;
        // The editor calls this after saving so an in-progress run reads the new document instead of
        // the copy it prepared at start.
        public void RefreshEdict()
        {
            var before=edictSource;PrepareEdict();
            if(State.heroAction.phase==HeroActionPhase.Channeling&&State.heroAction.policy?.whirlwindEdict!=null&&
                UnityEngine.JsonUtility.ToJson(before)!=UnityEngine.JsonUtility.ToJson(edictSource))State.heroAction.exitChannelForBuild=true;
        }
        public string EdictInactiveReason=>edictInactiveReason;
        public EdictResponsePlan LastEdictPlan {get;private set;}
        public EdictAimPlan LastEdictAim {get;private set;}
        public int EdictAimedCasts {get;private set;}
        // W03, A01, A02, M01, M02, M03 and the Mage basic attack have fixed aiming policies.
        static readonly int[] AimSkills={2,6,7,12,13,14};

        // Called once at construction and again whenever the live build changes, because the document
        // is only allowed to act while its slots match the loadout actually fighting.
        // A reopened run keeps the reason it already reported; every later call is a real change
        // of the document or the loadout, so the next block is worth reporting again.
        void PrepareEdict(bool reopening=false)
        {
            edictSource=null;recommendedSkills=null;edictPolicy=null;edictLoot=null;edictTarget=null;edictField=null;edictMovementRules.Clear();LastEdictPlan=null;
            if(!reopening)State.edictBlockLogged="";
            bool classEdict=ClassSkills.Enabled(Hero)&&!ClassSkillLoadout.IsAbsent(State.build.classSkills);
            if(!classEdict&&(Hero?.edict==null||HuntEdictV2Storage.IsAbsent(Hero.edict))){edictInactiveReason="이 캐릭터에 저장된 사냥 칙령 원본이 없습니다.";return;}
            if((!classEdict||ClassSkillTree.Uses(State.build.classSkills))&&!Hero.useEdict){State.limitedLoot=false;edictInactiveReason="사냥 칙령 v0.2 자동 판단이 꺼져 있습니다.";return;}
            try
            {
                if(Hero.useRecommendedEdict&&classEdict)recommendedSkills=HuntEdictDefaults.Skills(State.build.classSkills);
                var document=classEdict?(recommendedSkills??State.build.classSkills).ProjectEdict():HuntEdictV2.Canonical(HuntEdictDefaults.Resolve(Hero,State.build));
                HuntEdictV2.ValidateReceiver(document,Hero,catalog);
                if(document.global.Single(o=>o.id=="bag.stillFull").value=="STOP")State.limitedLoot=false;
                edictSource=document;edictPolicy=EdictPolicyBridge.Apply(State.build,document,Hero.heroClass);edictLoot=new EdictLootPolicy(document);edictTarget=EdictTargetPolicy.Compile(document);edictField=EdictFieldPolicy.Compile(document);edictInactiveReason="";
                foreach(var compiled in EdictRuleOrder.ForSimulation(Array.Empty<Rule>(),document))
                    if(EdictRuleOrder.IsCompiledRule(compiled.rule.id))edictMovementRules.Add(compiled.rule.id,compiled.rule);
            }
            catch(ArgumentException e){edictInactiveReason=Loc.F("사냥 칙령 원본을 사용할 수 없습니다: {0}", e.Message);}
        }

        // Returns true when the edict took the main action this decision cycle, so the ordinary rule
        // pass is skipped and does not immediately override the survival response it just started.
        // Applies the document's aiming stage to a ready skill candidate. "Not available" means the
        // skill's chosen purpose is not met right now, so the candidate is passed over and reads EDICT_AIM.
        bool EdictAimGate(RuleCandidate c,bool countCast=true)
        {
            bool compiled=EdictRuleOrder.IsCompiledRule(c.rule.id);
            if(edictSource==null)
            {if(compiled){c.ready=false;c.code="EDICT_OFF";c.detail=edictInactiveReason;return false;}return true;}
            if(c.rule.id=="edict:A02"&&c.code=="APPROACH")return true;
            bool coreBasic=c.rule.id=="edict:MAGE:BASIC"||c.rule.id=="edict:RANGER:BASIC";
            if(!coreBasic&&(c.rule.action!=RuleAction.Skill||Array.IndexOf(AimSkills,c.rule.skill)<0))return true;
            EdictAimPlan plan;
            try{plan=c.edictAim??PlanEdictAim(edictSource,coreBasic?"BASIC":CombatTelemetry.SkillId(c.rule.skill),c.target);}
            catch(Exception e)
            {
                edictSource=null;edictPolicy=null;edictLoot=null;edictTarget=null;edictField=null;edictInactiveReason=Loc.F("사냥 칙령 조준 판단을 중단했습니다: {0}", e.Message);
                Log("EDICT_STOPPED",edictInactiveReason);
                if(compiled){c.ready=false;c.code="EDICT_OFF";c.detail=edictInactiveReason;return false;}return true;
            }
            LastEdictAim=plan;c.edictAim=plan;
            if(!plan.supported)return true;
            var target=State.enemies.Find(e=>e.id==plan.targetId&&!e.dead);
            if(target!=null)c.target=target;
            if(!plan.available){c.ready=false;c.code=plan.blockCode;c.detail=plan.reason;return false;}
            if(plan.forecast!=null)c.aim=plan.forecast.aim;
            if(compiled)c.detail=plan.reason;
            if(countCast)EdictAimedCasts++;
            return true;
        }

        bool TickEdictSurvival()
        {
            if(edictSource==null)return false;
            EdictResponseExecution execution;
            try{execution=ExecuteEdictSurvival(edictSource);}
            catch(Exception e)
            {
                // A bad document must never end a run. Stand down for the rest of it and say why.
                edictSource=null;edictPolicy=null;edictLoot=null;edictTarget=null;edictField=null;edictInactiveReason=Loc.F("사냥 칙령 판단을 중단했습니다: {0}", e.Message);
                Log("EDICT_STOPPED",edictInactiveReason);
                return false;
            }
            LastEdictPlan=execution.plan;
            if(execution.plan.assessment.emergency&&execution.plan.ControlsMainAction&&State.edictTarget!=null)
            {State.edictTarget.enemyId=-1;State.edictTarget.searching=false;State.edictTarget.pursuitSeconds=0;}
            string blocked=execution.plan.blockedReason;
            if(blocked!=State.edictBlockLogged)
            {
                // Logged only on change; the decision cycle runs five times a second.
                if(blocked!="")Log("EDICT_BLOCKED",blocked);
                else if(State.edictBlockLogged!="")Log("EDICT_RESUMED","사냥 칙령 판단을 다시 사용합니다.");
                State.edictBlockLogged=blocked;
            }
            return blocked==""&&execution.plan.ControlsMainAction;
        }
    }
}
