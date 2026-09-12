using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Hellscript.Tests
{
    public sealed class EdictRuleOrderTests
    {
        GameCatalog catalog;
        [SetUp] public void Setup(){catalog=ScriptableObject.CreateInstance<GameCatalog>();catalog.Populate();}
        [TearDown] public void Teardown()=>UnityEngine.Object.DestroyImmediate(catalog);

        static List<Rule> Rules(params Rule[] rules)=>rules.ToList();
        static Rule Skill(int index)=>new Rule(index){action=RuleAction.Skill,id="skill-"+index};
        static Rule Of(RuleAction action)=>new Rule(-1){action=action,id=action.ToString()};
        static HuntEdictV2Document With(HuntEdictV2Document d,params (string id,string value)[] edits)
        {foreach(var e in edits)d=HuntEdictV2Editing.WithGlobal(d,e.id,e.value);return d;}
        AccountSave Account(bool useEdict)
        {
            var a=ContentTestAccounts.Training(catalog);a.selectedHero=2;a.Hero.level=30;
            a.Hero.build=BehaviorPresets.ForLevel(HeroClass.Mage,0,30,catalog);
            a.Hero.edict=HuntEdictV2Storage.CreateForHero(a.Hero);a.Hero.useEdict=useEdict;return a;
        }
        static void Advance(CombatSimulation sim,int ticks){for(int i=0;i<ticks;i++)sim.Tick(CombatSimulation.Step);}
        static int Starts(CombatSimulation sim,string id)=>sim.State.statistics.skills.SingleOrDefault(s=>s.definitionId==id)?.starts??0;

        [Test]
        public void WithoutADocumentTheRulesKeepTheirWrittenOrderAndNothingIsSkipped()
        {
            var rules=Rules(Skill(13),Of(RuleAction.Loot),Skill(12),Of(RuleAction.Explore),Of(RuleAction.Basic));
            var ordered=EdictRuleOrder.Order(rules,null);
            CollectionAssert.AreEqual(new[]{0,1,2,3,4},ordered.Select(o=>o.row));
            Assert.IsTrue(ordered.All(o=>o.skipReason==""));
        }

        [Test]
        public void AttackRulesFollowTheDocumentsSkillOrderAndKeepTheirRows()
        {
            var d=HuntEdictV2.Create(HeroClass.Mage);
            // Move 연쇄 번개 (M03) to the front of the shared order.
            d=HuntEdictV2Editing.WithSkillOrderMoved(d,"M03",-1);d=HuntEdictV2Editing.WithSkillOrderMoved(d,"M03",-1);
            Assert.AreEqual("M03",d.skillOrder[0]);
            var rules=Rules(Skill(12),Skill(13),Skill(14),Of(RuleAction.Basic));   // M01, M02, M03, basic as written
            var ordered=EdictRuleOrder.Order(rules,d).Where(o=>o.rule.action==RuleAction.Skill||o.rule.action==RuleAction.Basic).ToList();
            CollectionAssert.AreEqual(new[]{14,12,13,-1},ordered.Select(o=>o.rule.skill),"M03 first, then M01, M02, then basic which is last in the shared order");
            CollectionAssert.AreEqual(new[]{2,0,1,3},ordered.Select(o=>o.row),"rows still point at the written rule");
        }

        [Test]
        public void ASkillWithAutomaticUseOffIsSkippedWithAReasonButKeepsItsPlace()
        {
            var d=HuntEdictV2.WithOption(HuntEdictV2.Create(HeroClass.Mage),"M01",1,"OFF");
            var ordered=EdictRuleOrder.Order(Rules(Skill(12),Skill(13)),d);
            var fire=ordered.Single(o=>o.rule.skill==12);
            StringAssert.Contains("자동 사용",fire.skipReason);
            Assert.AreEqual("",ordered.Single(o=>o.rule.skill==13).skipReason);
            Assert.AreEqual(2,ordered.Count,"a skipped rule is still reported, not dropped");
        }

        [Test]
        public void BasicAttackFollowsItsOwnAutomaticSwitch()
        {
            var d=HuntEdictV2.WithOption(HuntEdictV2.Create(HeroClass.Mage),"BASIC",1,"OFF");
            var ordered=EdictRuleOrder.Order(Rules(Of(RuleAction.Basic),Skill(12)),d);
            Assert.AreNotEqual("",ordered.Single(o=>o.rule.action==RuleAction.Basic).skipReason);
        }

        [Test]
        public void LootMovesBeforeAttackOnlyWhenSomeRarityIsCollectedDuringCombat()
        {
            var after=HuntEdictV2.Create(HeroClass.Mage);   // defaults: AFTER_COMBAT, and common.order puts AFTER_COMBAT_LOOT after ATTACK
            var rules=Rules(Of(RuleAction.Loot),Skill(12),Of(RuleAction.Explore),Of(RuleAction.Chest));
            var ordered=EdictRuleOrder.Order(rules,after).Select(o=>o.rule.action).ToList();
            CollectionAssert.AreEqual(new[]{RuleAction.Skill,RuleAction.Loot,RuleAction.Explore,RuleAction.Chest},ordered);
            var during=With(after,("loot.RARE","PRIORITY"));
            ordered=EdictRuleOrder.Order(rules,during).Select(o=>o.rule.action).ToList();
            CollectionAssert.AreEqual(new[]{RuleAction.Loot,RuleAction.Skill,RuleAction.Explore,RuleAction.Chest},ordered);
        }

        [Test]
        public void TheCommonOrderDecidesWhereExplorationSits()
        {
            var d=With(HuntEdictV2.Create(HeroClass.Mage),("common.order","EMERGENCY,DODGE,EXPLORE,PRIORITY_LOOT,ATTACK,AFTER_COMBAT_LOOT"));
            var ordered=EdictRuleOrder.Order(Rules(Skill(12),Of(RuleAction.Explore),Of(RuleAction.Loot)),d).Select(o=>o.rule.action).ToList();
            CollectionAssert.AreEqual(new[]{RuleAction.Explore,RuleAction.Skill,RuleAction.Loot},ordered);
        }

        [Test]
        public void OrderingNeverMutatesTheRules()
        {
            var rules=Rules(Skill(12),Skill(13),Of(RuleAction.Loot));string before=string.Join("|",rules.Select(JsonUtility.ToJson));
            EdictRuleOrder.Order(rules,HuntEdictV2.WithOption(HuntEdictV2.Create(HeroClass.Mage),"M02",1,"OFF"));
            Assert.AreEqual(before,string.Join("|",rules.Select(JsonUtility.ToJson)));
        }

        [Test]
        public void SwitchingASkillOffInTheDocumentStopsTheHeroCastingIt()
        {
            // Two always-on rules so both skills would fire; the document then switches 화염구 (12) off.
            AccountSave Make(bool useEdict)
            {
                var a=Account(useEdict);
                a.Hero.build.rules=new List<Rule>{BehaviorRules.Make(12),BehaviorRules.Make(14)};a.Hero.build.activeSkills=new List<int>{12,14};
                a.Hero.edict=HuntEdictV2Storage.CreateForHero(a.Hero);return a;
            }
            var off=new CombatSimulation(Make(false),catalog,1,0,ownedTraining:true);Advance(off,400);
            Assert.Greater(Starts(off,"M01"),0,"as written, the always-on 화염구 rule fires");
            var a=Make(true);a.Hero.edict=HuntEdictV2.WithOption(a.Hero.edict,"M01",1,"OFF");
            var on=new CombatSimulation(a,catalog,1,0,ownedTraining:true);Assert.IsTrue(on.EdictActive);Advance(on,400);
            Assert.AreEqual(0,Starts(on,"M01"),"a skill switched off in the document is never inspected");
            Assert.Greater(Starts(on,"M03"),0,"the hero still fights with the remaining skill");
            Assert.IsTrue(on.State.decisions.Any(x=>x.code=="EDICT_OFF"),"the skip is recorded where the player can read it");
        }

        [Test]
        public void TheEdictsOrderDecidesWhichOfTwoEquallyReadySkillsIsCastFirst()
        {
            // Two always-on rules with the same range and cost: 화염구 (12) written first, 연쇄 번개 (14) second.
            AccountSave Make(bool useEdict)
            {
                var a=Account(useEdict);
                a.Hero.build.rules=new List<Rule>{BehaviorRules.Make(12),BehaviorRules.Make(14)};a.Hero.build.activeSkills=new List<int>{12,14};
                a.Hero.edict=HuntEdictV2Storage.CreateForHero(a.Hero);return a;
            }
            var legacy=new CombatSimulation(Make(false),catalog,1,0,ownedTraining:true);Advance(legacy,200);
            Assert.AreEqual(12,legacy.State.actionEvents.First(e=>e.skill>=0).skill,"as written, the first rule casts first");
            var a=Make(true);var d=a.Hero.edict;
            while(d.skillOrder[0]!="M03")d=HuntEdictV2Editing.WithSkillOrderMoved(d,"M03",-1);
            a.Hero.edict=d;
            var driven=new CombatSimulation(a,catalog,1,0,ownedTraining:true);Assert.IsTrue(driven.EdictActive);Advance(driven,200);
            Assert.AreEqual(14,driven.State.actionEvents.First(e=>e.skill>=0).skill,"the promoted skill is inspected first and therefore cast first");
        }

        [Test]
        public void TwoDrivenRunsWithTheSameOrderStayIdentical()
        {
            string Trace(CombatSimulation s)=>$"{s.State.time:F3}|{s.State.health:F4}|{s.State.kills}|{s.State.dealt:F3}|{s.State.rng}";
            AccountSave Make(){var a=Account(true);a.Hero.edict=HuntEdictV2Editing.WithSkillOrderMoved(a.Hero.edict,"BASIC",-1);return a;}
            var x=new CombatSimulation(Make(),catalog,1,0,ownedTraining:true);var y=new CombatSimulation(Make(),catalog,1,0,ownedTraining:true);
            Advance(x,300);Advance(y,300);Assert.AreEqual(Trace(x),Trace(y));
        }
    }
}
