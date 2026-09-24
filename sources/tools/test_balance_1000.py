#!/usr/bin/env python3
"""Independent boundary, budget and source-fixture checks for the planning model."""
import json
import math
import unittest
from pathlib import Path
import balance_1000 as model


class BalancePlanningTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.data=model.read(model.OUT/"projection.json")
        cls.config=cls.data["assumptions"]
        cls.rows=cls.data["rows"]
        cls.slots=model.read(model.RES/"BlacksmithSlots.json")["slots"]

    def test_fresh_stats_match_unity_for_all_classes(self):
        reference=model.read(model.OUT/"production_skill_tree_audit.json")["rows"]
        for name in ("Warrior","Ranger","Mage"):
            actual=next(r for r in reference if r["hero"]==name and r["level"]==1)
            target=self.rows[0]["class_targets"][name]
            for measured,key in [("damage","attack_power"),("hp","hp"),("armor","armor"),("resistance","resistance")]:
                self.assertAlmostEqual(actual[measured],target[key],places=3,msg=name+" "+key)

    def test_contiguous_stages_finite_stats_and_time_budget(self):
        self.assertEqual([r["stage"] for r in self.rows],list(range(1,1001)))
        seconds=sum(r["expected_attempts"]*r["duration_seconds"] for r in self.rows)
        self.assertAlmostEqual(seconds,180*120*60,places=6)
        self.assertTrue(all(r["expected_successes"]>=1 for r in self.rows))
        self.assertAlmostEqual(sum(r["farming_successes"]+1 for r in self.rows),self.rows[-1]["cumulative_successes"],places=6)
        for r in self.rows:
            for key in ("attack_power","boss_dps","mixed_ehp","normal_hp","boss_hp","normal_attack","boss_attack"):
                self.assertTrue(math.isfinite(r[key]) and r[key]>0)

    def test_contributions_do_not_double_count(self):
        for r in self.rows:
            for key in ("attack_share","dps_share","ehp_share","power_share"):
                self.assertAlmostEqual(sum(r[key].values()),1,places=8)
                self.assertGreaterEqual(min(r[key].values()),-1e-8)
        self.assertEqual(self.rows[0]["dps_share"]["baseline"],1)
        self.assertEqual(self.rows[-1]["attack_share"]["gems"],0)
        self.assertGreater(self.rows[-1]["dps_share"]["gems"],0)

    def test_no_preunlock_model_power(self):
        for r in self.rows:
            if r["stage"]<10:
                self.assertEqual(r["dps_share"]["gems"],0)
            if r["stage"]<30:self.assertEqual(r["dps_share"]["legendary_set"],0)
            if r["stage"]<15:self.assertEqual(r["dps_share"]["runes"],0)
            if r["stage"]<80:self.assertEqual(r["dps_share"]["quality"],0)

    def test_legendary_duplicate_slots_and_guarantee(self):
        self.assertEqual(model.slot_distribution(0)["mean"],0)
        self.assertEqual(model.slot_distribution(0,True)["mean"],1)
        self.assertAlmostEqual(model.slot_distribution(1e4)["mean"],9)
        self.assertLess(model.slot_distribution(9)["mean"],9)
        self.assertEqual(self.rows[8]["proposal_equipped_coverage"]["p10"],0)
        self.assertGreaterEqual(self.rows[29]["proposal_equipped_coverage"]["p10"],1)
        for r in self.rows:
            p=r["proposal_equipped_coverage"]
            self.assertTrue(0<=p["p10"]<=p["p50"]<=p["p90"]<=9)

    def test_budgets_identify_failed_calendar_scenario(self):
        scenarios={s["days"]:s for s in self.data["scenarios"]}
        self.assertTrue(all(not stages for stages in scenarios[180]["validation"]["resource_or_timer_shortages"].values()))
        self.assertLess(scenarios[90]["gold_earned"],scenarios[90]["gold_required"])
        self.assertTrue(scenarios[90]["validation"]["resource_or_timer_shortages"]["forge_time"])
        self.assertLess(scenarios[90]["mastery_level"],41)

    def test_enemy_hit_budget_uses_weakest_class(self):
        for r in self.rows:
            weakest=min(c["mixed_ehp"] for c in r["class_targets"].values())
            scale=.3+.2*(r["stage"]-1)/4 if r["stage"]<=5 else 1
            self.assertAlmostEqual(r["boss_attack"]/weakest,.16*scale,places=9)
            self.assertAlmostEqual(r["elite_hp"]/r["normal_hp"],3,places=9)

    def test_growth_parameters_recalculate_results(self):
        row=model.interpolate(self.config,1000)
        baseline=model.analytical_metrics(row,self.slots,self.config,attribute=False)
        changed=model.analytical_metrics(row,self.slots,{**self.config,"masterwork_step":.005},attribute=False)
        self.assertLess(changed["attack_power"],baseline["attack_power"])
        self.assertLess(changed["mixed_ehp"],baseline["mixed_ehp"])

    def test_current_forge_formula_matches_unity_output(self):
        for row in model.read(model.OUT/"current_runtime.json")["slots"]:
            stones,minutes=model.forge_costs(row["level"])
            self.assertEqual(stones,row["stonesPerSlot"])
            self.assertEqual(minutes*60,row["secondsPerSlot"])

    def test_source_fingerprints_are_current(self):
        self.assertEqual(self.data["source_hashes"],model.source_manifest())


if __name__=="__main__":unittest.main(verbosity=2)
