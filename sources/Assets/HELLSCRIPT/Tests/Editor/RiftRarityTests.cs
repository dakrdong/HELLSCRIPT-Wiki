using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace Hellscript.Tests
{
    public sealed class RiftRarityTests
    {
        static readonly RiftRewardSource[] Sources = { RiftRewardSource.Normal, RiftRewardSource.Elite, RiftRewardSource.Boss };
        static readonly int[] Stages = { 1, 2, 9, 10, 19, 20, 29, 30, 49, 50, 51, 99, 100, 299, 300, 999, 1000, 1000000, int.MaxValue - 1, int.MaxValue };
        static readonly double[][] Baselines = { new[] { .55, .25, .19, .01 }, new[] { 0d, .60, .35, .05 }, new[] { 0d, 0d, .85, .15 } };
        static readonly double[] Caps = { .10, .25, .45 };
        GameCatalog catalog;
        string directory;

        [SetUp]
        public void Setup()
        {
            catalog = ScriptableObject.CreateInstance<GameCatalog>();
            catalog.Populate();
            directory = Path.Combine(Path.GetTempPath(), "HellscriptRiftRarity-" + Guid.NewGuid().ToString("N"));
        }

        [TearDown]
        public void Teardown()
        {
            UnityEngine.Object.DestroyImmediate(catalog);
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
        }

        [Test]
        public void FirstStageAndNonpositiveStagesKeepEveryExistingBaseline()
        {
            foreach (var source in Sources)
            foreach (int stage in new[] { int.MinValue, -1, 0, 1 })
            {
                var probabilities = RiftRarity.Get(source, stage);
                for (int grade = 0; grade < 4; grade++)
                    Assert.AreEqual(Baselines[(int)source][grade], probabilities[grade], 1e-12, source + " / " + stage + " / " + grade);
            }
        }

        [Test]
        public void RepresentativeStagesMatchTheDraftCurveAndPreserveLowerGradeRatios()
        {
            foreach (var source in Sources)
            foreach (int stage in Stages)
            {
                var probabilities = RiftRarity.Get(source, stage);
                double[] expected = ExpectedProbabilities(source, stage);
                for (int grade = 0; grade < 4; grade++)
                    Assert.AreEqual(expected[grade], probabilities[grade], 1e-12, source + " / " + stage + " / " + grade);
                Assert.AreEqual(probabilities.Rare + probabilities.Legendary, probabilities.RareOrBetter, 1e-12);
                var halfway = RiftRarity.Get(source, 50);
                Assert.AreEqual((Baselines[(int)source][3] + Caps[(int)source]) / 2, halfway.Legendary, 1e-12);
            }
        }

        [Test]
        public void EveryProbabilityRemainsFiniteNormalizedAndBelowItsCapAtIntegerLimits()
        {
            foreach (int stage in Stages)
            {
                double previousSource = 0;
                foreach (var source in Sources)
                {
                    var p = RiftRarity.Get(source, stage);
                    Assert.AreEqual(1d, p.Total, 1e-12, source + " / " + stage);
                    for (int grade = 0; grade < 4; grade++)
                    {
                        Assert.IsFalse(double.IsNaN(p[grade]) || double.IsInfinity(p[grade]));
                        Assert.That(p[grade], Is.InRange(0d, 1d));
                    }
                    Assert.GreaterOrEqual(p.Legendary, previousSource);
                    Assert.LessOrEqual(p.Legendary, Caps[(int)source]);
                    Assert.Less(p.Legendary, 1d);
                    if (source != RiftRewardSource.Normal) Assert.AreEqual(0d, p.Common);
                    if (source == RiftRewardSource.Boss) Assert.AreEqual(0d, p.Magic);
                    previousSource = p.Legendary;
                }
            }
        }

        [Test]
        public void LegendaryAndEveryUpperGradeCumulativeProbabilityNeverRegress()
        {
            foreach (var source in Sources)
            {
                var previous = RiftRarity.Get(source, 1);
                foreach (int stage in Enumerable.Range(2, 9999).Concat(new[] { 1000000, int.MaxValue - 1, int.MaxValue }))
                {
                    var next = RiftRarity.Get(source, stage);
                    Assert.GreaterOrEqual(next.Legendary + 1e-12, previous.Legendary);
                    Assert.GreaterOrEqual(next.RareOrBetter + 1e-12, previous.RareOrBetter);
                    Assert.GreaterOrEqual(1 - next.Common + 1e-12, 1 - previous.Common);
                    previous = next;
                }
            }
        }

        [Test]
        public void ExactDrawBoundariesSelectOnlyTheirAllowedGrades()
        {
            foreach (var source in Sources)
            foreach (int stage in new[] { 1, 10, 50, 1000, int.MaxValue })
            {
                var p = RiftRarity.Get(source, stage);
                Assert.AreEqual(3, p.Roll(0));
                Assert.AreEqual(3, p.Roll(p.Legendary - 1e-12));
                Assert.AreEqual(2, p.Roll(p.Legendary));
                if (p.Magic > 0) Assert.AreEqual(1, p.Roll(p.Legendary + p.Rare));
                if (p.Common > 0) Assert.AreEqual(0, p.Roll(p.Legendary + p.Rare + p.Magic));
                Assert.AreEqual(source == RiftRewardSource.Normal ? 0 : source == RiftRewardSource.Elite ? 1 : 2, p.Roll(.9999999999999999));
                foreach (double invalid in new[] { -.001, 1d, double.NaN, double.PositiveInfinity })
                    Assert.That(() => p.Roll(invalid), Throws.InstanceOf<ArgumentException>());
            }
        }

        [Test]
        public void DataParametersChangeTheCurveWithoutChangingProductionDefaults()
        {
            var normal = new RiftRarityProfile(.55, .25, .19, .01, .12);
            var elite = new RiftRarityProfile(0, .60, .35, .05, .30);
            var boss = new RiftRarityProfile(0, 0, .85, .15, .50);
            var parameters = new RiftRarityParameters(9, normal, elite, boss);
            double[] caps = { .12, .30, .50 };
            foreach (var source in Sources)
            {
                Assert.AreEqual((Baselines[(int)source][3] + caps[(int)source]) / 2, RiftRarity.Get(source, 10, parameters).Legendary, 1e-12);
                Assert.AreEqual(ExpectedProbabilities(source, 10)[3], RiftRarity.Get(source, 10).Legendary, 1e-12);
            }
            Assert.AreEqual(49d, RiftRarityBalance.Current.GrowthHalfSpan);
        }

        [Test]
        public void InvalidDataCannotIntroduceNegativeProbabilitiesForbiddenGradesOrInvertedSources()
        {
            var current = RiftRarityBalance.Current;
            foreach (double invalid in new[] { -1d, 0d, double.NaN, double.PositiveInfinity })
                Assert.That(() => new RiftRarityParameters(invalid, current.Normal, current.Elite, current.Boss), Throws.InstanceOf<ArgumentException>());
            Assert.That(() => new RiftRarityProfile(-.1, .4, .6, .1, .2), Throws.InstanceOf<ArgumentException>());
            Assert.That(() => new RiftRarityProfile(.55, .25, .19, .02, .1), Throws.InstanceOf<ArgumentException>());
            Assert.That(() => new RiftRarityProfile(.55, .25, .19, .01, .005), Throws.InstanceOf<ArgumentException>());
            Assert.That(() => new RiftRarityProfile(.55, .25, .19, .01, 1.1), Throws.InstanceOf<ArgumentException>());
            Assert.That(() => new RiftRarityProfile(double.NaN, .25, .19, .01, .1), Throws.InstanceOf<ArgumentException>());
            Assert.That(() => new RiftRarityParameters(49, current.Normal, new RiftRarityProfile(.1, .5, .35, .05, .25), current.Boss), Throws.InstanceOf<ArgumentException>());
            Assert.That(() => new RiftRarityParameters(49, current.Normal, current.Elite, new RiftRarityProfile(0, .1, .75, .15, .45)), Throws.InstanceOf<ArgumentException>());
            Assert.That(() => new RiftRarityParameters(49, new RiftRarityProfile(.55, .25, .19, .01, .4), current.Elite, current.Boss), Throws.InstanceOf<ArgumentException>());
        }

        [TestCase(RiftRewardSource.Normal)]
        [TestCase(RiftRewardSource.Elite)]
        [TestCase(RiftRewardSource.Boss)]
        public void SeededGradeDrawsMatchExpectedFrequenciesAndConsumeOneDraw(RiftRewardSource source)
        {
            const int samples = 100000;
            const uint seed = 20260912;
            foreach (int stage in new[] { 1, 10, 50, 1000 })
            {
                uint rng = seed, control = seed;
                int[] counts = new int[4];
                for (int n = 0; n < samples; n++)
                {
                    counts[RiftRarity.Roll(source, stage, ref rng)]++;
                    RandomStream.Unit(ref control);
                }
                Assert.AreEqual(control, rng, "Rarity selection must consume exactly one reward draw.");
                double[] expected = ExpectedProbabilities(source, stage);
                for (int grade = 0; grade < 4; grade++) AssertFrequency(counts[grade], samples, expected[grade], source + " / " + stage + " / " + grade);
                TestContext.WriteLine("Grade simulation: seed=" + seed + ", source=" + source + ", stage=" + stage + ", samples=" + samples + ", counts=" + string.Join(",", counts) + ", tolerance=max(0.001,6*sqrt(p*(1-p)/N)).");
            }
        }

        [TestCase(RiftRewardSource.Normal)]
        [TestCase(RiftRewardSource.Elite)]
        [TestCase(RiftRewardSource.Boss)]
        public void RealDeathSettlementUsesCurrentTenNotHighestFiftyAndKeepsDrawCount(RiftRewardSource source)
        {
            var sim = Fixture(10, out var account);
            account.Hero.highestClear = 50;
            uint seed = DistinguishingSeed(source);
            sim.State.rewardRng = seed;
            uint expectedRng = seed;
            int expectedCount = source == RiftRewardSource.Boss ? 3 : 1;
            var expected = new Item[expectedCount];
            int firstDropId = sim.State.nextId + (source == RiftRewardSource.Boss ? 0 : 1);
            if (source == RiftRewardSource.Normal) Assert.Less(RandomStream.Unit(ref expectedRng), .02f);
            for (int i = 0; i < expected.Length; i++) expected[i] = ExpectedDrop(sim.Hero.heroClass, source, 10, ref expectedRng, sim.State.id + "-" + (firstDropId + i));
            Assert.AreNotEqual(OracleGrade(source, 50, FirstGradeRoll(source, seed)), expected[0].rarity, "The fixture must distinguish current stage from highest clear.");
            int gold = account.gold, materials = account.materials;
            AddPendingDeath(sim, source);
            // The public tick resolves real death rewards before movement and collection.
            sim.Tick(CombatSimulation.Step);
            Assert.AreEqual(expectedCount, sim.State.drops.Count);
            for (int i = 0; i < expected.Length; i++) Assert.AreEqual(ItemFingerprint(expected[i]), ItemFingerprint(sim.State.drops[i].item));
            Assert.AreEqual(expectedRng, sim.State.rewardRng);
            Assert.IsTrue(sim.State.drops.All(d => d.item.level >= 8 && d.item.level <= 12));
            Assert.AreEqual(50, account.Hero.highestClear);
            Assert.AreEqual(gold + (source == RiftRewardSource.Boss ? 3300 : source == RiftRewardSource.Elite ? 75 : 15), account.gold);
            Assert.AreEqual(materials + (source == RiftRewardSource.Boss ? 19 : 0), account.materials);
            if (source == RiftRewardSource.Boss) Assert.AreEqual(RunPhase.Looting, sim.State.phase);
        }

        [TestCase(1)]
        [TestCase(1000)]
        public void NormalEnemyEquipmentGateRemainsTwoPercentAtLowAndHighStages(int stage)
        {
            const int samples = 50000;
            const uint seed = 918273;
            var sim = Fixture(stage, out _);
            sim.State.rewardRng = seed;
            var enemy = new EnemyState { id = 900, elite = -1, position = sim.State.position + Vector2.up * 8 };
            int drops = 0;
            var reward = typeof(CombatSimulation).GetMethod("RewardEnemyDeath", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(reward);
            for (int n = 0; n < samples; n++)
            {
                reward.Invoke(sim, new object[] { enemy });
                Assert.LessOrEqual(sim.State.drops.Count, 1);
                drops += sim.State.drops.Count;
                sim.State.drops.Clear();
            }
            AssertFrequency(drops, samples, .02, "Normal equipment gate");
            TestContext.WriteLine("Enemy reward simulation: seed=" + seed + ", stage=" + stage + ", deaths=" + samples + ", drops=" + drops + ", expected=2%, tolerance=max(0.001,6*sqrt(p*(1-p)/N)); calls actual death-reward handler, not real-time combat play.");
        }

        [Test]
        public void SweepUsesSelectedHerosHighestActualClearWithThreeBossDrawsAndNoClearRecord()
        {
            var account = GameStore.NewAccount();
            account.Hero.highestClear = 50;
            account.heroes[1].highestClear = 300;
            account.Hero.firstClears.Add(50);
            account.suspendedRun = new RunState { stage = 10, heroId = account.Hero.id, phase = RunPhase.Cleared };
            uint rng = 124799, expectedRng = rng;
            // Sweep retains its existing slot-before-grade draw order.
            var expected = Enumerable.Range(0, 3).Select(_ => ExpectedSweepItem(account.Hero.heroClass, 50, ref expectedRng)).ToArray();
            int count = account.Hero.inventory.Count;
            Assert.IsTrue(Economy.Sweep(account, "rarity-sweep", ref rng));
            var actual = account.Hero.inventory.Skip(count).ToArray();
            Assert.AreEqual(3, actual.Length);
            for (int i = 0; i < 3; i++) Assert.AreEqual(ItemFingerprint(expected[i], false), ItemFingerprint(actual[i], false));
            Assert.AreEqual(expectedRng, rng);
            Assert.AreEqual(3300, account.gold);
            Assert.AreEqual(15, account.materials);
            Assert.AreEqual(1, account.sweepCount);
            Assert.AreEqual(50, account.Hero.highestClear);
            CollectionAssert.AreEqual(new[] { 50 }, account.Hero.firstClears);
            Assert.IsEmpty(account.records);
            string before = JsonUtility.ToJson(account);
            uint settled = rng;
            Assert.IsFalse(Economy.Sweep(account, "rarity-sweep", ref rng));
            Assert.AreEqual(before, JsonUtility.ToJson(account));
            Assert.AreEqual(settled, rng);
        }

        [Test]
        public void SavedUnclaimedBossItemsStayFixedAfterRecordChangesRestoreAndRepeatedCollection()
        {
            var store = new GameStore(directory, catalog);
            var sim = Fixture(10, out _, store.Data);
            sim.Hero.highestClear = 50;
            sim.State.rewardRng = DistinguishingSeed(RiftRewardSource.Boss);
            AddPendingDeath(sim, RiftRewardSource.Boss);
            sim.Tick(CombatSimulation.Step);
            string[] items = sim.State.drops.Select(d => ItemFingerprint(d.item)).ToArray();
            Assert.AreEqual(3, items.Length);
            Assert.IsTrue(sim.State.drops.All(d => !d.claimed));
            uint settledRng = sim.State.rewardRng;
            store.Data.suspendedRun = sim.State;
            sim.Hero.highestClear = 500;
            Assert.IsTrue(store.Save());
            var loaded = new GameStore(directory, catalog);
            CollectionAssert.AreEqual(items, loaded.Data.suspendedRun.drops.Select(d => ItemFingerprint(d.item)));
            var resumed = new CombatSimulation(loaded.Data, catalog, 999, restore: loaded.Data.suspendedRun);
            Assert.AreEqual(10, resumed.State.stage);
            int before = resumed.Hero.inventory.Count, gold = loaded.Data.gold, materials = loaded.Data.materials;
            resumed.Tick(CombatSimulation.Step);
            resumed.Tick(CombatSimulation.Step);
            Assert.AreEqual(RunPhase.Cleared, resumed.State.phase);
            Assert.AreEqual(before + 3, resumed.Hero.inventory.Count);
            Assert.AreEqual(settledRng, resumed.State.rewardRng);
            CollectionAssert.AreEqual(items, resumed.Hero.inventory.Skip(before).Select(i => ItemFingerprint(i)));
            Assert.AreEqual(gold, loaded.Data.gold);
            Assert.AreEqual(materials, loaded.Data.materials);
            Assert.AreEqual(1, loaded.Data.records.Count);
            Assert.IsTrue(loaded.Save());
            var reloaded = new GameStore(directory, catalog);
            var repeat = new CombatSimulation(reloaded.Data, catalog, 1, restore: reloaded.Data.suspendedRun);
            repeat.Tick(CombatSimulation.Step);
            Assert.AreEqual(before + 3, repeat.Hero.inventory.Count);
            Assert.AreEqual(settledRng, repeat.State.rewardRng);
        }

        [Test]
        public void SweepReceiptSurvivesReconnectionAndDoesNotRerollAtANewHighestStage()
        {
            var store = new GameStore(directory, catalog);
            store.Data.Hero.highestClear = 50;
            uint rng = 56199;
            Assert.IsTrue(Economy.Sweep(store.Data, "saved-rarity-sweep", ref rng));
            Assert.IsTrue(store.Save());
            var loaded = new GameStore(directory, catalog);
            loaded.Data.Hero.highestClear = 300;
            string before = JsonUtility.ToJson(loaded.Data);
            uint settledRng = rng;
            Assert.IsFalse(Economy.Sweep(loaded.Data, "saved-rarity-sweep", ref rng));
            Assert.AreEqual(before, JsonUtility.ToJson(loaded.Data));
            Assert.AreEqual(settledRng, rng);
        }

        [Test]
        public void OfflineSettlementStillAwardsOnlyGoldAndCommonMaterials()
        {
            Directory.CreateDirectory(directory);
            var account = GameStore.NewAccount();
            account.Hero.highestClear = 1000;
            account.lastSeenUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - 3600;
            ContentUnlocks.Reconcile(account);
            account.contentUnlocks.offlineActivatedUtc = account.lastSeenUtc;
            string[] inventories = account.heroes.SelectMany(h => h.inventory).Select(i => ItemFingerprint(i)).ToArray();
            int[] cores = (int[])account.cores.Clone();
            File.WriteAllText(Path.Combine(directory, "hellscript-local-v1.json"), JsonUtility.ToJson(account));
            var loaded = new GameStore(directory);
            Assert.GreaterOrEqual(loaded.LocalIdleGoldAwarded, 40200);
            Assert.GreaterOrEqual(loaded.LocalIdleMaterialsAwarded, 205);
            CollectionAssert.AreEqual(inventories, loaded.Data.heroes.SelectMany(h => h.inventory).Select(i => ItemFingerprint(i)));
            CollectionAssert.AreEqual(cores, loaded.Data.cores);
            Assert.IsEmpty(loaded.Data.warehouse);
            Assert.IsNull(loaded.Data.suspendedRun);
            Assert.IsEmpty(loaded.Data.records);
        }

        [Test]
        public void ExistingLegendaryMemberWeightsIncludingSetsRemainUnchanged()
        {
            string[] weight20 = { "LW03", "LA02", "LM03" };
            string[] weight10 = { "LW04", "LA04", "LM04" };
            Assert.AreEqual(39, ItemCatalog.Uniques.Count);
            foreach (var item in ItemCatalog.Uniques)
                Assert.AreEqual(weight20.Contains(item.id) ? 20 : weight10.Contains(item.id) ? 10 : 100, item.weight, item.id);
            const int samples = 20000;
            const uint seed = 112681;
            uint rng = seed;
            int rareMember = 0;
            for (int n = 0; n < samples; n++)
                if (ItemGenerator.Create(HeroClass.Warrior, 6, 3, 30, ref rng).special == "LW04") rareMember++;
            AssertFrequency(rareMember, samples, 10d / 110, "Legendary member LW04");
            TestContext.WriteLine("Legendary composition simulation: seed=" + seed + ", samples=" + samples + ", LW04=" + rareMember + ", expected=10/110, tolerance=max(0.001,6*sqrt(p*(1-p)/N)).");
        }

        [Test]
        public void RarityContextDoesNotChangeSubsequentLegendaryCompositionOrFixedCraftGrades()
        {
            string expected = null;
            foreach (var source in Sources)
            foreach (int stage in new[] { 1, 50, 1000 })
            {
                uint rng = 5531;
                RiftRarity.Roll(source, stage, ref rng);
                var legendary = Economy.CreateItem(HeroClass.Warrior, 3, 3, 30, ref rng, "composition-fixture");
                string actual = JsonUtility.ToJson(legendary);
                if (expected != null) Assert.AreEqual(expected, actual);
                expected = actual;
                var rare = Economy.CreateItem(HeroClass.Warrior, 3, 2, 30, ref rng);
                Assert.AreEqual(2, rare.rarity);
                Assert.AreEqual(3, rare.rolls.Count);
                Assert.AreEqual(3, legendary.rarity);
                Assert.AreEqual(4, legendary.rolls.Count);
            }
        }

        [TestCase(-1, 500L)]
        [TestCase(0, 500L)]
        [TestCase(1, 500L)]
        [TestCase(10, 9500L)]
        [TestCase(30, 58500L)]
        [TestCase(100, 545000L)]
        public void RerollCostUsesOnlyClampedItemLevelAndNeverRerollCount(int level, long expected)
        {
            foreach (int rerolls in new[] { 0, 1, 5, 12, 100, int.MaxValue })
                Assert.AreEqual(expected, Economy.RerollGold(new Item { level = level, rerolls = rerolls }));
        }

        [Test]
        public void RepeatedRerollTransactionsDebitTheSameCost()
        {
            var account = GameStore.NewAccount();
            account.Hero.highestClear = 8;
            account.gold = 1000000;
            uint rng = 12983;
            var item = ItemGenerator.Create(HeroClass.Warrior, 6, 2, 30, ref rng);
            account.Hero.inventory.Add(item);
            for (int n = 0; n < 12; n++)
            {
                int gold = account.gold;
                Assert.IsTrue(Economy.Reroll(account, item, item.rolls[0].slotId, ref rng));
                Assert.AreEqual(58500, gold - account.gold);
            }
        }

        CombatSimulation Fixture(int stage, out AccountSave account, AccountSave existing = null)
        {
            account = existing ?? GameStore.NewAccount();
            account.Hero.level = 30;
            account.Hero.useEdict = false;
            account.Hero.build.passives = Array.Empty<int>();
            account.Hero.build.minimumRarity = 0;
            account.Hero.build.bagPolicy = BagPolicy.Ignore;
            account.Hero.capacity = 100;
            // A training map supplies a small, known room; rewards are then exercised as a real run.
            var sim = new CombatSimulation(account, catalog, stage, 0, seed: 917);
            sim.State.training = -1;
            sim.State.id = "rift-rarity-fixture";
            sim.State.enemies.Clear();
            sim.State.build.rules.Clear();
            sim.State.build.movement = MovementMode.Stand;
            sim.State.decisionTime = 100;
            sim.State.position = RiftMap.Rooms[0];
            sim.State.destination = sim.State.position;
            sim.Stats.regen = 0;
            return sim;
        }

        static void AddPendingDeath(CombatSimulation sim, RiftRewardSource source)
        {
            var enemy = new EnemyState { id = 900, elite = source == RiftRewardSource.Elite ? 0 : -1, boss = source == RiftRewardSource.Boss,
                position = sim.State.position + Vector2.up * 8, health = 0, maxHealth = 1, dead = true, pendingDeath = true };
            sim.State.enemies.Add(enemy);
            if (enemy.boss) { sim.State.bossId = enemy.id; sim.State.phase = RunPhase.Boss; }
        }

        static double[] ExpectedProbabilities(RiftRewardSource source, int stage)
        {
            double t = Math.Max(0d, (double)stage - 1), q = t / (t + 49d);
            double[] baseline = Baselines[(int)source];
            double p = baseline[3] + (Caps[(int)source] - baseline[3]) * q;
            return new[] { baseline[0] * (1 - p) / (1 - baseline[3]), baseline[1] * (1 - p) / (1 - baseline[3]), baseline[2] * (1 - p) / (1 - baseline[3]), p };
        }

        static int OracleGrade(RiftRewardSource source, int stage, float roll)
        {
            double[] p = ExpectedProbabilities(source, stage);
            if (roll < p[3]) return 3;
            if (roll < p[3] + p[2]) return 2;
            if (roll < p[3] + p[2] + p[1]) return 1;
            return 0;
        }

        static Item ExpectedDrop(HeroClass hero, RiftRewardSource source, int stage, ref uint rng, string id)
        {
            int rarity = OracleGrade(source, stage, RandomStream.Unit(ref rng));
            int slot = RandomStream.Range(ref rng, 0, 8);
            int level = RandomStream.Range(ref rng, Math.Max(1, stage - 2), stage + 3);
            return Economy.CreateItem(hero, slot, rarity, level, ref rng, id);
        }

        static Item ExpectedSweepItem(HeroClass hero, int stage, ref uint rng)
        {
            int slot = RandomStream.Range(ref rng, 0, 8);
            int rarity = OracleGrade(RiftRewardSource.Boss, stage, RandomStream.Unit(ref rng));
            int level = RandomStream.Range(ref rng, Math.Max(1, stage - 2), stage + 3);
            return Economy.CreateItem(hero, slot, rarity, level, ref rng);
        }

        static uint DistinguishingSeed(RiftRewardSource source)
        {
            for (uint seed = 1; seed < 1000000; seed++)
            {
                uint rng = seed;
                if (source == RiftRewardSource.Normal && RandomStream.Unit(ref rng) >= .02f) continue;
                float roll = RandomStream.Unit(ref rng);
                if (OracleGrade(source, 10, roll) != OracleGrade(source, 50, roll)) return seed;
            }
            throw new InvalidOperationException("No stage-discriminating seed found.");
        }

        static float FirstGradeRoll(RiftRewardSource source, uint rng)
        {
            if (source == RiftRewardSource.Normal) RandomStream.Unit(ref rng);
            return RandomStream.Unit(ref rng);
        }

        static string ItemFingerprint(Item item, bool includeIdentity = true)
        {
            var copy = JsonUtility.FromJson<Item>(JsonUtility.ToJson(item));
            copy.acquiredOrder = 0;
            if (!includeIdentity)
            {
                copy.id = "";
                foreach (var roll in copy.rolls) roll.slotId = "";
            }
            return JsonUtility.ToJson(copy);
        }

        static void AssertFrequency(int count, int samples, double expected, string label)
        {
            if (expected == 0) { Assert.AreEqual(0, count, label); return; }
            double tolerance = Math.Max(.001, 6 * Math.Sqrt(expected * (1 - expected) / samples));
            Assert.AreEqual(expected, count / (double)samples, tolerance, label);
        }
    }
}
