using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Hellscript.Tests
{
    public sealed class EdictResponseTests
    {
        GameCatalog catalog;AccountSave account;
        [SetUp]public void Setup(){catalog=ScriptableObject.CreateInstance<GameCatalog>();catalog.Populate();}
        [TearDown]public void Teardown()=>UnityEngine.Object.DestroyImmediate(catalog);
        CombatSimulation Fixture(HeroClass hero=HeroClass.Mage,int level=30)
        {
            account=ContentTestAccounts.Training();account.selectedHero=(int)hero;account.Hero.level=level;account.Hero.inventory.Clear();account.Hero.build.passives=Array.Empty<int>();account.Hero.build.rules.Clear();account.Hero.build.activeSkills.Clear();account.Hero.build.potionThreshold=0;
            var sim=new CombatSimulation(account,catalog,1,1,seed:246812,ownedTraining:true);sim.State.position=RiftMap.Rooms[0];sim.State.enemies.Clear();sim.Stats.armor=sim.Stats.resistance=sim.Stats.regen=sim.Stats.crit=0;sim.Stats.hp=100;sim.State.health=20;sim.State.resource=100;
            sim.State.enemies.Add(new EnemyState{id=900,position=sim.State.position+Vector2.up*2,health=1000000,maxHealth=1000000,speed=0,attack=0,cooldown=1000,brain=new EnemyBrain{initialized=true}});return sim;
        }
        HuntEdictV2Document Configure(CombatSimulation sim,params string[] skills)
        {
            var d=HuntEdictV2.Create(sim.Hero.heroClass);for(int i=0;i<skills.Length;i++)d.slots[i]=skills[i];sim.State.build.activeSkills=skills.Select(id=>catalog.skills.FindIndex(s=>s.id==id)).ToList();
            Set(d,"survival.potion","OFF");Set(d,"survival.lowHp","ON");return d;
        }
        static void Set(HuntEdictV2Document d,string id,string value)=>d.global.Single(o=>o.id==id).value=value;
        static void Advance(CombatSimulation sim,float time){for(int n=0;n<Mathf.RoundToInt(time/CombatSimulation.Step);n++)sim.Tick(CombatSimulation.Step);}
        static EnemyHazard Danger(CombatSimulation sim,float damage=20,float delay=1)
        {
            var h=new EnemyHazard{id=800,actionId=800,enemyId=900,definitionId="TEST_DANGER",position=sim.State.position,shape=AttackShape.Circle,radius=2,damage=damage,delay=delay,createdAt=-1};sim.State.enemyHazards.Add(h);return h;
        }
        static void CurrentAttack(CombatSimulation sim,HeroActionPhase phase)
        {
            bool channel=phase==HeroActionPhase.Channeling;
            sim.State.heroAction=new HeroActionState{id=700,skill=channel?0:-1,phase=phase,prepare=.5f,travel=.5f,recovery=.5f,origin=sim.State.position,destination=sim.State.position+Vector2.right*2,policy=new HeroActionPolicy{rule=new Rule(channel?0:-1){id="old-attack"},movement=MovementMode.Stand},startedAt=-1};
            sim.State.activeSkill=channel?0:-1;sim.State.channelTick=.1f;sim.State.targetId=900;
        }
        [TestCase(HeroClass.Warrior,"W05")][TestCase(HeroClass.Mage,"M05")]
        public void EmergencyShieldRunsTheRealPreparationAndCostsOnlyOnce(HeroClass hero,string id)
        {
            var sim=Fixture(hero);var d=Configure(sim,id);Set(d,"survival.defenseSkill",id);string original=HuntEdictV2Codec.Encode(d),owner=JsonUtility.ToJson(account);
            sim.State.shields.Add(new ShieldEffect{id=99,definitionId="OTHER",amount=80,remaining=5});
            var p=sim.PlanEdictSurvival(d);Assert.AreEqual(id,p.selected.skillId);float resource=sim.State.resource;
            var e=sim.ExecuteEdictSurvival(d);Assert.IsTrue(e.mainActionStarted);Assert.AreEqual(resource-p.selected.cost,sim.State.resource,.0001f);Assert.AreEqual(1,sim.State.shields.Count);
            Assert.IsFalse(sim.ExecuteEdictSurvival(d).mainActionStarted);Advance(sim,.2f);
            Assert.AreEqual(20,sim.State.shields.Single(s=>s.definitionId==id).amount,.001f);Assert.AreEqual(1,sim.State.actionEvents.Count(a=>a.kind=="ACTION_START"));Assert.AreEqual(1,sim.State.actionEvents.Count(a=>a.kind=="ACTION_RELEASE"));
            Assert.AreEqual(original,HuntEdictV2Codec.Encode(d));Assert.AreEqual(owner,JsonUtility.ToJson(account));Assert.IsTrue(sim.State.heroAction.phase==HeroActionPhase.Idle);
        }
        [TestCase("OWN")][TestCase("CAP")][TestCase("COOLDOWN")][TestCase("OFF")]
        public void ShieldQualificationRejectsAnUnusableDefenseWithoutSpendingAnything(string cause)
        {
            var sim=Fixture();var d=Configure(sim,"M05");Set(d,"survival.defenseSkill","M05");
            if(cause=="OWN"||cause=="CAP")sim.State.shields.Add(new ShieldEffect{id=99,definitionId=cause=="OWN"?"M05":"OTHER",amount=cause=="CAP"?100:5,remaining=4});
            if(cause=="COOLDOWN")sim.State.cooldowns[16]=1;if(cause=="OFF")d=HuntEdictV2.WithOption(d,"M05",1,"OFF");
            var p=sim.PlanEdictSurvival(d);Assert.IsFalse(p.attempts.First().ready);float resource=sim.State.resource;sim.ExecuteEdictSurvival(d);
            Assert.AreEqual(resource,sim.State.resource);Assert.IsFalse(sim.HeroActionBusy);
        }
        [TestCase(HeroActionPhase.Preparing,true)][TestCase(HeroActionPhase.Channeling,true)][TestCase(HeroActionPhase.Travelling,false)][TestCase(HeroActionPhase.Recovering,false)]
        public void EmergencyUsesExistingInterruptiblePhasesOnly(HeroActionPhase phase,bool allowed)
        {
            var sim=Fixture(HeroClass.Warrior);var d=Configure(sim,"W05");Set(d,"survival.defenseSkill","W05");CurrentAttack(sim,phase);var old=sim.State.heroAction;
            var e=sim.ExecuteEdictSurvival(d);Assert.AreEqual(allowed,e.mainActionStarted);
            if(allowed){Assert.AreEqual(4,sim.State.heroAction.skill);Assert.IsTrue(sim.State.actionEvents.Any(a=>a.actionId==old.id&&a.kind=="ACTION_INTERRUPTED"));}
            else {Assert.AreSame(old,sim.State.heroAction);Assert.IsFalse(sim.State.actionEvents.Any(a=>a.kind=="ACTION_INTERRUPTED"));}
        }
        [Test]
        public void FinishCurrentChannelRequestsItsNextBoundaryAndDoesNotBuyAnotherDamageTick()
        {
            var sim=Fixture(HeroClass.Warrior);sim.State.health=100;var d=Configure(sim,"W02");Set(d,"survival.lowHp","OFF");Set(d,"survival.escapeSkill","W02");Set(d,"dodge.area.policy","ALWAYS");Set(d,"dodge.method","SKILL_FIRST");Danger(sim);CurrentAttack(sim,HeroActionPhase.Channeling);
            var e=sim.ExecuteEdictSurvival(d);Assert.IsTrue(e.channelFinishRequested);Assert.IsFalse(e.mainActionStarted);Assert.IsTrue(sim.State.heroAction.exitChannelForSurvival);
            float resource=sim.State.resource;Advance(sim,.1f);Assert.IsFalse(sim.HeroActionBusy);Assert.AreEqual(resource,sim.State.resource,.0001f);Assert.IsEmpty(sim.State.damageEvents.Where(h=>!h.incoming));
            Assert.IsTrue(sim.ExecuteEdictSurvival(d).mainActionStarted);Assert.AreEqual(1,sim.State.heroAction.skill);
        }
        [TestCase(HeroClass.Warrior,"W02",.6f)][TestCase(HeroClass.Ranger,"A04",.5f)][TestCase(HeroClass.Mage,"M04",.2f)]
        public void EscapeActionsReachTheirChosenDestinationThroughNormalTravel(HeroClass hero,string id,float duration)
        {
            var sim=Fixture(hero);var d=Configure(sim,id);Set(d,"survival.escapeSkill",id);var before=sim.State.position;var e=sim.ExecuteEdictSurvival(d);
            Assert.IsTrue(e.mainActionStarted);Assert.AreEqual(id,e.plan.selected.skillId);Vector2 destination=e.plan.selected.destination;
            Assert.GreaterOrEqual(Vector2.Distance(before,destination),2-.001f);Assert.LessOrEqual(Vector2.Distance(before,destination),catalog.skills[e.plan.selected.skill].range+.001f);
            if(id!="A04")Assert.AreEqual(before,sim.State.position);Advance(sim,duration);
            Assert.Less(Vector2.Distance(destination,sim.State.position),.001f);Assert.IsFalse(sim.HeroActionBusy);Assert.AreEqual(0,sim.State.moveDistance,.0001f,"Skill travel is not walking.");
            Assert.IsTrue(sim.State.cooldowns[e.plan.selected.skill]>0);Assert.AreEqual(1,sim.State.actionEvents.Count(a=>a.kind=="ACTION_RELEASE"));
        }
        [Test]
        public void TeleportCanEscapeAVisibleEnvironmentHazardWithoutAnEnemyTarget()
        {
            var sim=Fixture();sim.State.enemies[0].position+=Vector2.up*50;sim.State.health=100;var d=Configure(sim,"M04");Set(d,"survival.lowHp","OFF");Set(d,"survival.escapeSkill","M04");Set(d,"dodge.area.policy","ALWAYS");Set(d,"dodge.method","SKILL_FIRST");Danger(sim);
            var e=sim.ExecuteEdictSurvival(d);Assert.IsTrue(e.mainActionStarted);Assert.AreEqual(-1,sim.State.heroAction.targetId);Advance(sim,.2f);Assert.Less(Vector2.Distance(e.plan.selected.destination,sim.State.position),.001f);
        }
        [Test]
        public void UnavailableEscapeHonorsKeepFightingAndDoesNotInventWalking()
        {
            var sim=Fixture();var d=Configure(sim,"M04");Set(d,"survival.escapeSkill","M04");Set(d,"survival.order","ESCAPE,WALK,DEFENSE");sim.State.cooldowns[15]=5;
            var e=sim.ExecuteEdictSurvival(d);Assert.AreEqual("KEEP_FIGHTING",e.plan.selected.role);Assert.IsFalse(e.mainActionStarted);Assert.IsFalse(sim.State.edictResponse.walking);
            Set(d,"survival.fallback","WALK");e=sim.ExecuteEdictSurvival(d);Assert.AreEqual("WALK",e.plan.selected.role);Assert.IsTrue(sim.State.edictResponse.walking);var position=sim.State.position;Advance(sim,.1f);Assert.Greater(Vector2.Distance(position,sim.State.position),0);
        }
        [Test]
        public void AnotherDesignatedSurvivalSkillIsUsedOnlyThroughTheExplicitFallback()
        {
            var sim=Fixture();var d=Configure(sim,"M04","M05");Set(d,"survival.escapeSkill","M04");Set(d,"survival.order","ESCAPE,WALK,DEFENSE");Set(d,"survival.fallback","OTHER_SKILL");Set(d,"survival.fallbackSkill","M05");sim.State.cooldowns[15]=4;
            var e=sim.ExecuteEdictSurvival(d);Assert.AreEqual("OTHER_SKILL",e.plan.selected.role);Assert.AreEqual(16,sim.State.heroAction.skill);Assert.IsFalse(sim.State.edictResponse.walking);
        }
        [Test]
        public void PotionRunsIndependentlyAndReevaluatesEmergencyAfterItsActualHealing()
        {
            var sim=Fixture();var d=Configure(sim,"M05");Set(d,"survival.defenseSkill","M05");Set(d,"survival.potion","ON");sim.Stats.healing=1;CurrentAttack(sim,HeroActionPhase.Travelling);var action=sim.State.heroAction;
            var e=sim.ExecuteEdictSurvival(d);Assert.IsTrue(e.potionUsed);Assert.IsFalse(e.mainActionStarted);Assert.AreEqual(55,sim.State.health,.001f);Assert.AreEqual(20,sim.State.potionCd);Assert.AreSame(action,sim.State.heroAction);Assert.IsFalse(e.plan.assessment.emergency);
            Assert.IsFalse(sim.ExecuteEdictSurvival(d).potionUsed);Assert.AreEqual(1,sim.State.logs.Count(l=>l.Contains("[POTION]")));
        }
        [TestCase("PAUSED")][TestCase("PORTAL")][TestCase("FAILED")][TestCase("DEAD")][TestCase("UNAPPLIED")]
        public void StoppedRunsAndUnappliedSkillDraftsCannotExecuteOrConsumePotion(string cause)
        {
            var sim=Fixture();var d=Configure(sim,"M05");Set(d,"survival.defenseSkill","M05");Set(d,"survival.potion","ON");
            if(cause=="PAUSED")sim.State.paused=true;if(cause=="PORTAL")sim.State.portal=true;if(cause=="FAILED")sim.State.phase=RunPhase.Failed;if(cause=="DEAD")sim.State.health=0;if(cause=="UNAPPLIED")sim.State.build.activeSkills.Clear();
            string before=JsonUtility.ToJson(sim.State);var e=sim.ExecuteEdictSurvival(d);Assert.IsNotEmpty(e.plan.blockedReason);Assert.IsFalse(e.potionUsed);Assert.IsFalse(e.mainActionStarted);Assert.AreEqual(before,JsonUtility.ToJson(sim.State));
        }
        [TestCase(HeroClass.Warrior,"W04")][TestCase(HeroClass.Mage,"M06")]
        public void DesignatedControlUsesActualAreaAndOnlyCompletesBossStaggerWhenPossible(HeroClass hero,string id)
        {
            var sim=Fixture(hero);var d=Configure(sim,id);Set(d,"survival.defenseSkill",id);var boss=sim.State.enemies[0];boss.boss=true;boss.brain.boss.initialized=true;boss.brain.boss.cooldowns=new[]{1000f,1000f,1000f};boss.bossControl.meter=84.9f;
            Assert.AreEqual("NO_CONTROL",sim.PlanEdictSurvival(d).attempts.First().code);boss.bossControl.meter=85;
            Assert.IsTrue(sim.ExecuteEdictSurvival(d).mainActionStarted);Advance(sim,.35f);Assert.Greater(boss.bossControl.staggered,0);Assert.AreEqual(0,boss.bossControl.meter);Assert.AreEqual(1,sim.State.effectEvents.Count(e=>e.kind=="BOSS_STAGGER"));
        }
        [Test]
        public void DesignatedTrapRespectsItsPlacementPolicyAndArmsBeforeRooting()
        {
            var sim=Fixture(HeroClass.Ranger);var d=Configure(sim,"A03");Set(d,"survival.defenseSkill","A03");d=HuntEdictV2.WithOption(d,"A03",2,"TARGET");d=HuntEdictV2.WithOption(d,"A03",3,"ONE");
            var e=sim.ExecuteEdictSurvival(d);Assert.IsTrue(e.mainActionStarted);Assert.IsEmpty(sim.State.traps);Advance(sim,.3f);Assert.AreEqual(1,sim.State.traps.Count);Assert.IsFalse(CombatEffects.Has(sim.State.enemies[0],StatusKind.Root));Advance(sim,.5f);Assert.IsTrue(CombatEffects.Has(sim.State.enemies[0],StatusKind.Root));
        }
        [Test]
        public void AFlightAndItsCapturedPolicySurviveSerializationWithoutReplayingTheCost()
        {
            var sim=Fixture(HeroClass.Ranger);var d=Configure(sim,"A04");Set(d,"survival.escapeSkill","A04");var e=sim.ExecuteEdictSurvival(d);Advance(sim,.15f);int id=sim.State.heroAction.id;float resource=sim.State.resource;
            var restored=JsonUtility.FromJson<RunState>(JsonUtility.ToJson(sim.State));GameStore.NormalizeRun(restored);var resumed=new CombatSimulation(account,catalog,1,restore:restored);Advance(resumed,.35f);
            Assert.Less(Vector2.Distance(e.plan.selected.destination,resumed.State.position),.001f);Assert.AreEqual(resource,resumed.State.resource,.001f);Assert.AreEqual(1,resumed.State.actionEvents.Count(a=>a.actionId==id&&a.kind=="ACTION_START"));Assert.AreEqual(1,resumed.State.actionEvents.Count(a=>a.actionId==id&&a.kind=="ACTION_RELEASE"));Assert.IsTrue(resumed.State.edictResponse.memory.active);
        }
        [Test]
        public void AWalkingCommandResumesItsDestinationAndKeepsPreviewReadOnly()
        {
            var sim=Fixture();var d=Configure(sim);Set(d,"survival.order","WALK,DEFENSE,ESCAPE");string before=JsonUtility.ToJson(sim.State),code=HuntEdictV2Codec.Encode(d);var p=sim.PlanEdictSurvival(d);
            Assert.AreEqual(before,JsonUtility.ToJson(sim.State));Assert.AreEqual("WALK",p.selected.role);Assert.Throws<NotSupportedException>(()=>((IList<EdictResponseCandidate>)p.attempts).Clear());
            sim.ExecuteEdictSurvival(d);Advance(sim,.1f);var restored=JsonUtility.FromJson<RunState>(JsonUtility.ToJson(sim.State));var resumed=new CombatSimulation(account,catalog,1,restore:restored);var start=resumed.State.position;Advance(resumed,.1f);
            Assert.Greater(Vector2.Distance(start,resumed.State.position),0);Assert.AreEqual(p.selected.destination,resumed.State.edictResponse.destination);Assert.AreEqual(code,HuntEdictV2Codec.Encode(d));
            sim.State.edictResponse.version=2;Assert.Throws<NotSupportedException>(()=>GameStore.NormalizeRun(sim.State));
        }
        [Test]
        public void AQueuedFlightCannotBeStartedFromAStalePreviewAfterCooldownChanges()
        {
            var sim=Fixture();var d=Configure(sim,"M04");Set(d,"survival.escapeSkill","M04");Assert.AreEqual("M04",sim.PlanEdictSurvival(d).selected.skillId);sim.State.cooldowns[15]=1;var e=sim.ExecuteEdictSurvival(d);Assert.IsFalse(e.mainActionStarted);Assert.IsFalse(sim.HeroActionBusy);
        }
        [TestCase(19f,false)][TestCase(20f,true)]
        public void PaidControlUsesTheActualCostBoundary(float resource,bool allowed)
        {
            var sim=Fixture();var d=Configure(sim,"M06");Set(d,"survival.defenseSkill","M06");sim.State.resource=resource;
            var e=sim.ExecuteEdictSurvival(d);Assert.AreEqual(allowed,e.mainActionStarted);Assert.AreEqual(allowed?0:resource,sim.State.resource,.0001f);
            if(!allowed)Assert.AreEqual("RESOURCE",e.plan.attempts.First().code);
        }
        [TestCase("WALK_FIRST",.25f,"WALK")][TestCase("SKILL_FIRST",1f,"ESCAPE")][TestCase("SKILL_IF_NEEDED",1f,"WALK")][TestCase("SKILL_IF_NEEDED",.25f,"ESCAPE")]
        public void DodgeMethodDistinguishesWalkingPreferenceAndWhetherWalkingCanArriveInTime(string method,float delay,string role)
        {
            var sim=Fixture();sim.State.health=100;var d=Configure(sim,"M04");Set(d,"survival.lowHp","OFF");Set(d,"survival.escapeSkill","M04");Set(d,"dodge.area.policy","ALWAYS");Set(d,"dodge.method",method);Danger(sim,20,delay);
            var p=sim.PlanEdictSurvival(d);Assert.AreEqual(role,p.selected.role);if(method=="SKILL_IF_NEEDED")Assert.AreEqual(delay==1,p.attempts.First().timely);
        }
        [TestCase("GLOBAL",true)][TestCase("NO_DAMAGE",false)]
        public void TeleportNoDamagePolicyRejectsEvenAnOtherwiseAllowedSmallHit(string option,bool allowed)
        {
            var sim=Fixture();var d=Configure(sim,"M04");Set(d,"survival.escapeSkill","M04");d=HuntEdictV2.WithOption(d,"M04",4,option);Danger(sim,1).radius=20;
            var p=sim.PlanEdictSurvival(d);Assert.AreEqual(allowed,p.selected?.skillId=="M04");
        }
        [Test]
        public void ReservedEscapeCannotBeReenteredThroughTheOtherSkillFallback()
        {
            var sim=Fixture();sim.State.health=100;var d=Configure(sim,"M04");Set(d,"survival.lowHp","OFF");Set(d,"survival.escapeSkill","M04");Set(d,"survival.reserveSkill","ON");Set(d,"survival.fallback","OTHER_SKILL");Set(d,"survival.fallbackSkill","M04");Set(d,"dodge.area.policy","ALWAYS");Set(d,"dodge.method","SKILL_FIRST");Danger(sim);
            sim.State.layout.legacy=false;
            foreach(Vector2 direction in new[]{Vector2.up,Vector2.down,Vector2.left,Vector2.right})sim.State.layout.obstacles.Add(new RiftObstacle{id="walk-fence",position=sim.State.position+direction*.9f,halfSize=direction.x==0?new Vector2(2,.1f):new Vector2(.1f,2),blocksSight=false,blocksProjectile=false,blocksLanding=false,blocksWalk=true});
            Assert.IsTrue(sim.Map.CanLand(sim.State.position+Vector2.down*4));var p=sim.PlanEdictSurvival(d);Assert.IsNull(p.selected);Assert.AreEqual(2,p.attempts.Count(c=>c.code=="RESERVED"));Assert.AreEqual("NO_PATH",p.attempts.Single(c=>c.role=="WALK").code);
        }
        [Test]
        public void NoLandingDoesNotSpendTheEscapeCooldownOrResource()
        {
            var sim=Fixture();var d=Configure(sim,"M04");Set(d,"survival.escapeSkill","M04");
            sim.State.layout.obstacles.Add(new RiftObstacle{id="unlandable",position=sim.State.position,halfSize=Vector2.one*20,blocksLanding=true,blocksSight=false,blocksProjectile=false,blocksWalk=false});
            var e=sim.ExecuteEdictSurvival(d);Assert.IsFalse(e.mainActionStarted);Assert.IsTrue(e.plan.attempts.Any(c=>c.code=="LANDING"));Assert.AreEqual(0,sim.State.cooldowns[15]);Assert.AreEqual(100,sim.State.resource);
        }
        [Test]
        public void ChannelFinishRequestSurvivesSaveAndBlocksLegacyInterruptsBeforeTheBoundary()
        {
            var sim=Fixture(HeroClass.Warrior);sim.State.health=100;var d=Configure(sim,"W02","W05");Set(d,"survival.lowHp","OFF");Set(d,"survival.escapeSkill","W02");Set(d,"dodge.area.policy","ALWAYS");Danger(sim);CurrentAttack(sim,HeroActionPhase.Channeling);
            sim.State.build.rules.Add(new Rule(4){id="priority-shield"});sim.State.build.rules.Add(new Rule(0){id="old-attack"});sim.State.heroAction.rule=1;
            sim.ExecuteEdictSurvival(d);var copy=JsonUtility.FromJson<RunState>(JsonUtility.ToJson(sim.State));GameStore.NormalizeRun(copy);Assert.AreEqual(5,copy.combatVersion);Assert.IsTrue(copy.heroAction.exitChannelForSurvival);
            var resumed=new CombatSimulation(account,catalog,1,restore:copy);resumed.State.resource=100;Advance(resumed,.05f);Assert.AreEqual(HeroActionPhase.Channeling,resumed.State.heroAction.phase);Advance(resumed,.05f);
            Assert.IsFalse(resumed.HeroActionBusy);Assert.AreEqual(100,resumed.State.resource,.0001f);Assert.IsFalse(resumed.State.actionEvents.Any(a=>a.skill==4&&a.kind=="ACTION_START"));
        }
        [TestCase(HeroClass.Warrior,"W05")][TestCase(HeroClass.Mage,"M05")]
        public void NormalDodgeFallbackCannotIgnoreAnEmergencyOnlyShieldPolicy(HeroClass hero,string id)
        {
            var sim=Fixture(hero);sim.State.health=100;var d=Configure(sim,id);Set(d,"survival.lowHp","OFF");Set(d,"survival.fallback","OTHER_SKILL");Set(d,"survival.fallbackSkill",id);Set(d,"dodge.area.policy","ALWAYS");d=HuntEdictV2.WithOption(d,id,2,"EMERGENCY");Danger(sim).radius=20;
            var p=sim.PlanEdictSurvival(d);Assert.IsNull(p.selected);Assert.AreEqual("PURPOSE",p.attempts.Last().code);
            sim.State.health=20;Set(d,"survival.lowHp","ON");Assert.AreEqual(id,sim.PlanEdictSurvival(d).selected.skillId);
        }
        [TestCase("OFF","PURPOSE")][TestCase("DISTANCE","LANDING")][TestCase("TRAP_LINK","LINK")][TestCase("PIERCE_LINK","LINK")]
        public void NormalRetreatRestrictionsAreNotSilentlyBypassedWhileEmergencyCanStillUseIt(string purpose,string failure)
        {
            var sim=Fixture(HeroClass.Ranger);sim.State.health=100;var d=Configure(sim,"A04");Set(d,"survival.lowHp","OFF");Set(d,"survival.escapeSkill","A04");Set(d,"dodge.area.policy","ALWAYS");Set(d,"dodge.method","SKILL_FIRST");d=HuntEdictV2.WithOption(d,"A04",2,purpose);Danger(sim);
            var p=sim.PlanEdictSurvival(d);Assert.AreEqual(failure,p.attempts.First().code);Assert.AreEqual("WALK",p.selected.role);
            sim.State.health=20;Set(d,"survival.lowHp","ON");Assert.AreEqual("A04",sim.PlanEdictSurvival(d).selected.skillId);
        }
    }
}
