using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Hellscript.Runes;

namespace Hellscript.Tests
{
    public sealed class RewardBoxTests
    {
        string directory,path;GameStore store;GameCatalog catalog;
        AccountSave A=>store.Data;
        static string Json(object o)=>JsonUtility.ToJson(o);
        [SetUp] public void Setup()
        {
            Loc.UseSource();directory=Path.Combine(Path.GetTempPath(),"hellscript-reward-boxes-"+Guid.NewGuid());path=Path.Combine(directory,"hellscript-local-v1.json");
            catalog=ScriptableObject.CreateInstance<GameCatalog>();catalog.Populate();store=new GameStore(directory,catalog);
        }
        [TearDown] public void Cleanup(){Loc.UseSource();UnityEngine.Object.DestroyImmediate(catalog);if(Directory.Exists(directory))Directory.Delete(directory,true);}
        void Cleared(int stage)=>A.Hero.riftProgress.best.Add(new RiftBestTime{stage=stage,milliseconds=1000});
        OwnedRewardBox Add(string definition,int count=1,int stage=100)
        {
            var box=new OwnedRewardBox{id=Guid.NewGuid().ToString("N"),boxId=definition,count=count,sourceStage=stage,itemLevel=RewardBoxCatalog.ItemLevel(stage),seed=712349,minimumQuality=6000};
            A.rewardBoxes.owned.Add(box);return box;
        }
        [Test] public void CatalogCoversEveryRequestedRaritySlotGemAndIcon()
        {
            RewardBoxCatalog.Validate(RewardBoxCatalog.Data);Assert.AreEqual(148,RewardBoxCatalog.All.Count);
            foreach(int rarity in new[]{2,3})foreach(int slot in Enumerable.Range(-1,9))Assert.AreEqual(1,RewardBoxCatalog.All.Count(d=>d.kind=="equipment"&&d.rarity==rarity&&d.slot==slot&&!d.setOnly&&!d.awakened&&!d.chooseSlot));
            foreach(var gem in GemCatalog.Gems)foreach(int tier in Enumerable.Range(1,6))Assert.AreEqual(10,RewardBoxCatalog.All.Single(d=>d.kind=="gem"&&d.gemId==gem.id&&d.tier==tier).amount);
            foreach(var d in RewardBoxCatalog.All)Assert.IsNotNull(Resources.Load<Sprite>("Art/RewardBoxes/"+d.icon),d.id);
        }
        [Test] public void EveryStageHasAValidPackageAndTotalsMatchBudget()
        {
            long premium=0,stones=0,materials=0,gemCount=0,boxes=0;
            foreach(int stage in Enumerable.Range(1,1000))foreach(var grant in RewardBoxCatalog.FirstClear(stage))
            {
                var d=RewardBoxCatalog.Find(grant.boxId);Assert.IsNotNull(d);int amount=RewardBoxCatalog.Amount(d,stage)*grant.count;boxes+=grant.count;
                if(d.kind=="premium")premium+=amount;if(d.kind=="stones")stones+=amount;if(d.kind=="materials")materials+=amount;if(d.kind=="gem")gemCount+=amount;
                Assert.That(RewardBoxCatalog.ItemLevel(stage),Is.InRange(1,60));
            }
            Assert.AreEqual(5600,premium);Assert.AreEqual(20800,stones);Assert.AreEqual(6250,materials);Assert.AreEqual(90,gemCount);Assert.Greater(boxes,1120);
        }
        [Test] public void NewFirstClearLegendaryGrantsBeginAtThirtyAndInvalidCatalogsAreRejected()
        {
            foreach(int stage in Enumerable.Range(1,29))Assert.IsFalse(RewardBoxCatalog.FirstClear(stage).Any(g=>RewardBoxCatalog.Find(g.boxId).rarity==3));
            Assert.IsTrue(RewardBoxCatalog.FirstClear(30).Any(g=>g.boxId=="legendary-weapon"));
            Assert.IsFalse(RewardBoxCatalog.FirstClear(25).Any(g=>g.boxId=="gold-20400"));
            Assert.IsTrue(RewardBoxCatalog.FirstClear(35).Any(g=>g.boxId=="gold-20400"));
            var copy=JsonUtility.FromJson<RewardBoxDatabase>(Json(RewardBoxCatalog.Data));
            copy.milestones.Single(m=>m.stage==30).stage=29;
            Assert.Throws<InvalidOperationException>(()=>RewardBoxCatalog.Validate(copy));
            copy=JsonUtility.FromJson<RewardBoxDatabase>(Json(RewardBoxCatalog.Data));
            copy.firstClearRules[0].grant.boxId="set-body";
            Assert.Throws<InvalidOperationException>(()=>RewardBoxCatalog.Validate(copy));
        }
        [Test] public void PreviouslyOwnedLowStageGuaranteeBoxesKeepTheirPromise()
        {
            var box=Add("legendary-weapon",stage:10);Assert.IsTrue(store.Save());
            var loaded=new GameStore(directory,catalog);Assert.IsTrue(loaded.OpenRewardBox("legacy-guarantee",box.id));
            Assert.AreEqual(3,loaded.LastBoxEquipment.Single().rarity);
        }
        [TestCase(0)] [TestCase(1)] [TestCase(2)] public void AllGearBoxesProduceOnlyLegalReleasedClassEquipment(int hero)
        {
            A.selectedHero=hero;A.Hero.capacity=500;
            foreach(var definition in RewardBoxCatalog.All.Where(d=>d.kind=="equipment"&&!d.chooseSlot))
            {
                var box=Add(definition.id,3);Assert.IsTrue(store.OpenRewardBox(Guid.NewGuid().ToString(),box.id,3),store.Error);
                foreach(var item in store.LastBoxEquipment)
                {
                    ItemCatalog.Validate(item);Assert.AreEqual(definition.rarity,item.rarity);Assert.AreEqual((HeroClass)hero,item.lootClass);
                    if(definition.slot>=0)Assert.AreEqual(definition.slot,item.slot);if(definition.slot==0)Assert.IsFalse(EquipmentSlots.Offhand(item));
                    Assert.AreEqual(definition.awakened,item.awakened);Assert.IsTrue(item.rolls.All(r=>r.rollBasisPoints>=6000));
                    if(item.rarity==3)Assert.IsTrue(ItemCatalog.Uniques.Any(u=>u.id==item.special&&u.Fits((HeroClass)hero,item.slot)));
                    if(definition.setOnly)Assert.IsNotEmpty(ItemCatalog.Unique(item.special).setId);
                }
            }
        }
        [Test] public void ClaimRequiresExactNormalClearAndIsAccountOnceWithoutRepeatingDirectCurrencies()
        {
            A.Hero.highestClear=100;A.Hero.firstClears.Add(10);Assert.IsFalse(store.ClaimRiftFirstRewards("not-normal",10));Assert.IsFalse(store.ClaimRiftFirstRewards("skip",9));
            Cleared(10);int gold=A.gold,materials=A.materials;Assert.IsTrue(store.ClaimRiftFirstRewards("claim",10),store.Error);
            Assert.AreEqual(gold,A.gold);Assert.AreEqual(materials,A.materials);Assert.AreEqual(5,A.rewardBoxes.owned.Sum(b=>b.count));
            string before=Json(A.rewardBoxes);Assert.IsTrue(store.ClaimRiftFirstRewards("claim",10));Assert.AreEqual(before,Json(A.rewardBoxes));
            Assert.IsFalse(store.ClaimRiftFirstRewards("claim",1));A.selectedHero=1;Cleared(10);Assert.IsFalse(store.ClaimRiftFirstRewards("another-hero",10));
            Assert.AreEqual(before,Json(A.rewardBoxes));
        }
        [Test] public void GemChoiceIsExplicitTenPerBoxAndRequestBindsChoice()
        {
            var box=Add("gem-choice-t3",2);string before=Json(A);
            Assert.IsFalse(store.OpenRewardBox("choose",box.id));Assert.AreEqual(before,Json(A));
            Assert.IsTrue(store.OpenRewardBox("choose",box.id,1,"G04"),store.Error);Assert.AreEqual(10,GemStacks.Count(A.gems,"G04",3));
            Assert.IsTrue(store.OpenRewardBox("choose",box.id,1,"G04"));Assert.IsFalse(store.OpenRewardBox("choose",box.id,1,"G06"));
            Assert.AreEqual(1,A.rewardBoxes.owned.Single().count);
            Assert.IsTrue(store.OpenRewardBox("next",box.id,1,"G06"));Assert.AreEqual(10,GemStacks.Count(A.gems,"G06",3));
        }
        [Test] public void FixedColorAndCoreChoicePayExactBalances()
        {
            var gem=Add("gem-g02-t6",2);Assert.IsTrue(store.OpenRewardBox("fixed",gem.id,2));Assert.AreEqual(20,GemStacks.Count(A.gems,"G02",6));
            var core=Add("core-choice");Assert.IsFalse(store.OpenRewardBox("invalid",core.id,1,"8"));Assert.IsTrue(store.OpenRewardBox("core",core.id,1,"7"));Assert.AreEqual(10,A.cores[7]);
        }
        [Test] public void FullBagsAndGemStorageRollBackTheEntireOpen()
        {
            var gear=Add("rare-weapon",2);A.Hero.capacity=1;string before=Json(A);Assert.IsFalse(store.OpenRewardBox("full",gear.id,2));Assert.AreEqual(before,Json(A));
            A.gemCapacity=1;Assert.IsTrue(GemInventory.Add(A,new GemStack{gemId="G01",tier=1,count=999}));var gems=Add("gem-g02-t1");before=Json(A);
            Assert.IsFalse(store.OpenRewardBox("gemfull",gems.id));Assert.AreEqual(before,Json(A));
        }
        [Test] public void CurrencyOverflowAndInvalidQuantityNeverConsume()
        {
            var box=Add("premium-100",11);A.premium=int.MaxValue;string before=Json(A);
            Assert.IsFalse(store.OpenRewardBox("overflow",box.id));Assert.IsFalse(store.OpenRewardBox("zero",box.id,0));Assert.IsFalse(store.OpenRewardBox("eleven",box.id,11));Assert.AreEqual(before,Json(A));
        }
        [Test] public void RuneBoxesGrantExactlyFiveTypesOrOneFiveCellRuneWithoutMasteryXp()
        {
            int original=A.runes.owned.Count;string mastery=Json(A.runes.mastery[0]);var starter=Add("rune-starter");Assert.IsTrue(store.OpenRewardBox("starter",starter.id));
            var added=A.runes.owned.Skip(original).ToArray();Assert.AreEqual(5,added.Length);Assert.AreEqual(5,added.Select(r=>r.type).Distinct().Count());Assert.IsTrue(added.All(r=>r.grade==0));
            var high=Add("rune-g6");Assert.IsTrue(store.OpenRewardBox("g6",high.id));Assert.AreEqual(6,A.runes.owned.Last().grade);Assert.AreEqual(mastery,Json(A.runes.mastery[0]));
        }
        [TestCase(false)] [TestCase(true)] public void FailedSavePreservesBoxRandomStateAndRetryPaysOnce(bool claim)
        {
            Cleared(10);var box=Add("legendary-weapon",2);Assert.IsTrue(store.Save());string before=Json(A),disk=File.ReadAllText(path);
            Func<bool> apply=()=>claim?store.ClaimRiftFirstRewards("save-failure",10):store.OpenRewardBox("save-failure",box.id);
            Directory.CreateDirectory(path+".tmp");LogAssert.Expect(LogType.Error,new Regex("저장하지 못했습니다:"));Assert.IsFalse(apply());
            Assert.AreEqual(before,Json(A));Assert.AreEqual(disk,File.ReadAllText(path));Directory.Delete(path+".tmp");Assert.IsTrue(apply(),store.Error);
            string committed=Json(A);Assert.IsTrue(apply());Assert.AreEqual(committed,Json(A));var reopened=new GameStore(directory,catalog);Assert.AreEqual(Json(A.rewardBoxes),Json(reopened.Data.rewardBoxes));
        }
        [Test] public void MigrationIsIdempotentAndUsesNormalClearEvidenceOnly()
        {
            Cleared(10);A.Hero.firstClears.Add(9);A.schema=11;A.rewardBoxes=null;File.WriteAllText(path,Json(A));
            var loaded=new GameStore(directory,catalog);Assert.AreEqual(GameStore.MaximumSchemaVersion,loaded.Data.schema);Assert.IsTrue(loaded.ClaimRiftFirstRewards("legacy",10));Assert.IsFalse(loaded.ClaimRiftFirstRewards("legacy-skip",9));
            var again=new GameStore(directory,catalog);Assert.AreEqual(Json(loaded.Data.rewardBoxes),Json(again.Data.rewardBoxes));Assert.IsFalse(again.ClaimRiftFirstRewards("legacy-again",10));
        }
        [Test] public void RuneIntroductionPaysOnceAndRequiresEveryPlayableGuideStep()
        {
            Assert.IsFalse(RuneBoardTutorial.IconVisible(A));Assert.IsFalse(store.ClaimRuneBoardTutorial());
            var operational=Add("rune-starter",stage:15);
            A.Hero.highestClear=15;ContentUnlocks.Reconcile(A);
            RewardBoxes.CaptureClear(A,new RunState{stage=15,training=-1,phase=RunPhase.Cleared});
            Assert.AreEqual(RuneBoardLesson.Waiting,A.guide.runeBoard.step,"Highest stage alone is not an exact clear");
            Cleared(15);RewardBoxes.CaptureClear(A,new RunState{stage=15,training=-1,phase=RunPhase.Cleared});Assert.IsTrue(store.Save());
            Assert.AreEqual(RuneBoardLesson.Reward,A.guide.runeBoard.step);Assert.IsFalse(RuneBoardTutorial.IconVisible(A));
            Assert.IsFalse(store.LandRuneBoardEmblem());Assert.IsFalse(store.FinishRuneBoardLesson());
            string before=Json(A),disk=File.ReadAllText(path),mastery=Json(A.runes.mastery);int runes=A.runes.owned.Count;
            Directory.CreateDirectory(path+".tmp");LogAssert.Expect(LogType.Error,new Regex("저장하지 못했습니다:"));Assert.IsFalse(store.ClaimRuneBoardTutorial());
            Assert.AreEqual(before,Json(A));Assert.AreEqual(disk,File.ReadAllText(path));Directory.Delete(path+".tmp");
            Assert.IsTrue(store.ClaimRuneBoardTutorial(),store.Error);Assert.IsTrue(store.ClaimRuneBoardTutorial());
            Assert.IsTrue(A.rewardBoxes.owned.Any(b=>b.id==operational.id),"An unrelated grant must stay unopened");
            Assert.AreEqual(runes+5,A.runes.owned.Count);Assert.AreEqual(mastery,Json(A.runes.mastery));
            Assert.AreEqual(5,A.guide.runeBoard.rewards.Count);Assert.AreEqual(5,A.runes.owned.Where(r=>A.guide.runeBoard.rewards.Contains(r.id)).Select(r=>r.type).Distinct().Count());
            Assert.AreEqual(RuneBoardLesson.Emblem,A.guide.runeBoard.step);Assert.IsFalse(RuneBoardTutorial.IconVisible(A));
            store=new GameStore(directory,catalog);Assert.AreEqual(RuneBoardLesson.Emblem,A.guide.runeBoard.step);
            Assert.IsTrue(store.LandRuneBoardEmblem(),store.Error);Assert.IsTrue(RuneBoardTutorial.IconVisible(A));
            Assert.IsTrue(store.BeginRuneBoardLesson(),store.Error);Assert.IsFalse(store.FinishRuneBoardLesson());
            string ownership=Json(A.runes);
            Assert.IsFalse(store.AdvanceRuneBoardPractice(new RunePracticeModel(4)),"Cannot jump ahead");
            for(int page=0;page<5;page++)
            {
                var model=new RunePracticeModel(page);Assert.IsFalse(store.AdvanceRuneBoardPractice(model),"An unfinished puzzle cannot advance");
                bool Solve(int index)
                {
                    if(index==model.Pieces.Count)return model.LessonComplete;
                    var piece=model.Pieces[index];model.Select(piece.InstanceId);
                    for(int r=0;r<(model.CanRotate?6:1);r++)
                    {
                        foreach(var c in model.BoardCells)if(model.TryPlace(piece.InstanceId,c)&&Solve(index+1))return true;
                        model.Select(piece.InstanceId);model.RecallSelected();model.RotateSelected();
                    }
                    return false;
                }
                Assert.IsTrue(Solve(0));Assert.IsTrue(store.AdvanceRuneBoardPractice(model),store.Error);
                Assert.IsTrue(store.AdvanceRuneBoardPractice(model),"Retries do not advance twice");
                store=new GameStore(directory,catalog);Assert.AreEqual(page+1,A.guide.runeBoard.practicePage);
                if(page<4)Assert.IsFalse(store.FinishRuneBoardLesson());
            }
            Assert.AreEqual(ownership,Json(A.runes),"Practice must not change actual owned runes or layouts");
            Assert.IsTrue(store.FinishRuneBoardLesson(),store.Error);
            store=new GameStore(directory,catalog);Assert.AreEqual(RuneBoardLesson.Complete,A.guide.runeBoard.step);Assert.IsFalse(RuneBoardTutorial.Active(A));
            Assert.IsTrue(Tutorials.Record(A,ContentUnlocks.Rune).practiced);Assert.AreEqual(runes+5,A.runes.owned.Count);
        }
        [TestCase(false)] [TestCase(true)] [TestCase(true,true)] public void RuneIntroductionMigratesWithoutReplayingForPreviouslyUnlockedAccounts(bool unlocked,bool previousPolicy=false)
        {
            A.Hero.highestClear=unlocked?15:14;ContentUnlocks.Reconcile(A);
            if(previousPolicy){A.schema=12;A.Hero.highestClear=0;A.contentUnlocks.version=1;A.contentUnlocks.unlocked.Clear();}
            // Read a real older JSON document with the newly added field absent, not a normalized write.
            File.WriteAllText(path,Regex.Replace(Json(A),"\\\"runeBoard\\\":(?:null|\\{[^{}]*\\}),?",""));
            store=new GameStore(directory,catalog);Assert.AreEqual(unlocked?RuneBoardLesson.Complete:RuneBoardLesson.Waiting,A.guide.runeBoard.step);
            Assert.AreEqual(unlocked,RuneBoardTutorial.IconVisible(A));Assert.IsFalse(RuneBoardTutorial.Active(A));
        }
        [Test] public void RuneIntroductionRejectsUnversionedProgressWithoutRewritingTheSave()
        {
            A.guide.runeBoard.version=0;A.guide.runeBoard.step=RuneBoardLesson.Emblem;
            string json=Json(A);File.WriteAllText(path,json);
            Assert.Throws<NotSupportedException>(()=>new GameStore(directory,catalog));Assert.AreEqual(json,File.ReadAllText(path));
        }
        [TestCase("version")] [TestCase("missing")] [TestCase("unknown")] public void UnsupportedBoxStateStopsLoadWithoutFallingBackToBackup(string kind)
        {
            Add("rare-weapon");Assert.IsTrue(store.Save());Assert.IsTrue(store.Save());
            if(kind=="version")A.rewardBoxes.version=99;else if(kind=="missing")A.rewardBoxes=null;else A.rewardBoxes.owned[0].boxId="unreleased-box";
            File.WriteAllText(path,Json(A));string bytes=File.ReadAllText(path);Assert.Throws<NotSupportedException>(()=>new GameStore(directory,catalog));Assert.AreEqual(bytes,File.ReadAllText(path));
        }
    }
}
