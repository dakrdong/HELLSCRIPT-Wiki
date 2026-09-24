#!/usr/bin/env python3
"""Reward-probability, source conservation and resource-feasibility checks."""
import math
import unittest
import balance_1000 as model
import balance_rewards as rewards


class RewardPlanningTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.data=model.read(model.OUT/'projection.json')
        cls.rows=cls.data['rows'];cls.cfg=cls.data['reward_assumptions']

    def test_rarity_mass_and_overlay_boundaries(self):
        for row in self.rows:
            p=row['rewards'];b=p['repeat_total']
            for source in p['sources'].values():
                probs=source['rarity_probabilities']
                self.assertAlmostEqual(sum(probs),1,places=12)
                self.assertTrue(all(0<=v<=1 for v in probs))
                self.assertAlmostEqual(sum(source['expected_by_rarity'].values()),source['expected_equipment'])
            self.assertLessEqual(b['set_inside_legendary'],b['legendary'])
            self.assertLessEqual(b['awakened_rare_or_legendary'],b['rare']+b['legendary'])

    def test_finite_count_distribution_against_hand_calculation(self):
        q=rewards.count_distribution([(2,.5),(1,1)])
        self.assertEqual(q['mean'],2)
        self.assertEqual([q[k] for k in ('p10','p50','p90')],[1,2,3])
        self.assertEqual(q['at_least_one'],1)
        p=self.rows[999]['rewards']
        # 110 * .01 + 8 * .35 + 3 + 2 * .5 + .1 * .8
        self.assertAlmostEqual(p['equipment_count_distribution']['mean'],7.98)
        self.assertAlmostEqual(p['boss_at_least_one_legendary'],.784)
        for row in self.rows:
            p=row['rewards']
            for key in ('equipment_count_distribution','legendary_count_distribution'):
                self.assertAlmostEqual(p[key]['probability_mass'],1,places=11)
            self.assertAlmostEqual(p['legendary_count_distribution']['mean'],p['repeat_total']['legendary'])

    def test_unlocks_apply_to_all_reward_sources(self):
        for row in self.rows[:100]:
            s=row['stage'];p=row['rewards']
            if s<30:self.assertEqual(p['repeat_total']['legendary'],0)
            if s<10:
                self.assertFalse(any(k.startswith('gem_t') for k in p['repeat_total']))
            if s<15:self.assertFalse(any(k.startswith('rune_g') for k in p['repeat_total']))
            if s<100:self.assertEqual(p['awakening_chance'],0)
        self.assertEqual(self.rows[9]['rewards']['gem_drop_tier'],1)
        self.assertEqual(self.rows[14]['rewards']['rune_drop_grade'],0)
        self.assertEqual(self.rows[99]['rewards']['gem_drop_tier'],2)
        self.assertEqual(self.rows[299]['rewards']['gem_drop_tier'],3)
        self.assertEqual(self.rows[649]['rewards']['rune_drop_grade'],6)

    def test_boss_clear_split_does_not_duplicate_rewards(self):
        for row in self.rows:
            s=row['stage'];p=row['rewards'];b=p['repeat_by_source']['boss'];c=p['repeat_by_source']['clear']
            self.assertEqual(b['gold']+c['gold'],800+50*s)
            self.assertAlmostEqual(sum(b[k] for k in rewards.RARITIES),3)
            self.assertFalse(set(rewards.RARITIES)&set(c))
            self.assertFalse(any(k.startswith(('gem_','rune_')) for k in c))
            expected=math.floor(10+2*s if s<=20 else 50+.075*(s-20))
            self.assertEqual(b['stones']+c['stones'],expected)

    def test_first_clear_milestones_stack_only_once_in_ledger(self):
        end=self.rows[-1]['rewards']['ledger'];sources=end['cumulative_currency_by_source']
        self.assertEqual(sources['first_basic']['gold'],51_050_000)
        self.assertEqual(sources['first_basic']['materials'],64_100)
        self.assertEqual(sources['first_basic']['stones'],17_770)
        self.assertEqual(end['cumulative']['guaranteed_legendary'],4)
        self.assertEqual(end['cumulative']['guaranteed_rare'],1)
        self.assertEqual(end['cumulative']['selected_slot_cores'],10)
        self.assertEqual(end['cumulative']['premium'],5600)
        self.assertEqual(sum(v for k,v in end['cumulative'].items() if k.startswith('choice_') and '_gem_t' in k),90)
        self.assertEqual(self.rows[999]['rewards']['first_clear_basic'],dict(gold=101000,materials=180,stones=572))
        # Choice packs are allocated once, not duplicated across all six colors.
        self.assertEqual(end['cumulative']['choice_ruby_gem_t1'],10)
        self.assertEqual(end['cumulative']['choice_diamond_gem_t1'],10)
        for row in self.rows:
            self.assertFalse(row['rewards']['daily']['first_clears_included'])

    def test_offline_supplies_use_five_percent_and_exclude_materials_and_chests(self):
        for row in self.rows:
            profile=row['rewards'];field=rewards.total(profile['repeat_by_source'][k] for k in ('normal','elite','boss','clear','goblin'))
            self.assertEqual(set(profile['offline_per_hour']),{'gold','stones'})
            for key in ('gold','stones'):
                self.assertAlmostEqual(profile['offline_per_hour'][key]*20,field.get(key,0)*3600/row['duration_seconds'])

    def test_sweep_has_no_first_clear_or_rune_rewards(self):
        for row in self.rows:
            p=row['rewards'];sweep=p['sweep_total']
            self.assertFalse(any(k.startswith(('rune_','gift_','choice_','guaranteed_')) for k in sweep))
            self.assertAlmostEqual(sum(sweep[k] for k in rewards.RARITIES),3)
            self.assertEqual(sweep['gold'],800+50*row['stage'])
        self.assertEqual(self.rows[58]['rewards']['daily']['sweeps'],0)
        self.assertEqual(self.rows[59]['rewards']['daily']['sweeps'],3)

    def test_salvage_counts_set_cores_and_stones_once(self):
        bag=dict(common=1,magic=1,rare=1,legendary=2,set_inside_legendary=1)
        cfg=dict(ordinary_fraction=1,legendary_fraction_anchors=[[1,1],[1000,1]])
        self.assertEqual(rewards.salvage(bag,100,cfg),dict(materials=8,stones=31,cores=2))
        self.assertFalse(self.cfg['salvage']['include_investment_refunds'])

    def test_currency_sources_conserve_totals(self):
        for row in self.rows:
            ledger=row['rewards']['ledger']
            for key,out in [('gold','gold_earned'),('materials','materials_earned'),('stones','stones_earned')]:
                source_total=sum(b[key] for b in ledger['cumulative_currency_by_source'].values())
                self.assertAlmostEqual(source_total,row[out],places=5)
                self.assertGreaterEqual(source_total+1e-5,ledger['low_optional_cumulative'][key])
        self.assertTrue(all(not v for v in self.data['validation']['no_chests_or_goblins_shortages'].values()))

    def test_gem_allocation_and_item_level_clamping(self):
        r=self.rows[9]
        self.assertGreaterEqual(r['gem_same_color_tier1_equivalent'],4)
        self.assertGreaterEqual(r['gem_ruby_tier1_equivalent'],2)
        self.assertEqual(rewards.item_levels(60,self.cfg['item_level_offsets']),[
            dict(level=58,probability=.1),dict(level=59,probability=.15),dict(level=60,probability=.75)])
        for row in self.rows:
            dist=row['rewards']['item_level_distribution']
            self.assertAlmostEqual(sum(v['probability'] for v in dist),1)
            self.assertTrue(all(1<=v['level']<=60 for v in dist))
            self.assertGreaterEqual(row['gem_ruby_tier1_equivalent'],row['gem_ruby_tier1_required'])

    def test_schedule_aliases_and_class_set_pools(self):
        main=self.data['assumptions']
        self.assertEqual(main['rune_grade_stages'],self.cfg['runes']['grade_starts'])
        self.assertEqual(main['gem_drop_tier_stages'],self.cfg['gems']['tier_starts'])
        uniques=model.read(model.OUT/'current_runtime.json')['uniques']
        fractions=[rewards.set_fraction(uniques,c) for c in range(3)]
        self.assertTrue(all(0<f<1 for f in fractions))
        self.assertAlmostEqual(fractions[0],self.rows[999]['rewards']['legendary_set_fraction'])
        self.assertEqual(rewards.guaranteed_single_slots(self.cfg,9),0)
        self.assertEqual(rewards.guaranteed_single_slots(self.cfg,30),1)
        self.assertEqual(rewards.guaranteed_single_slots(self.cfg,1000),3)
        self.assertEqual(model.slot_distribution(0,3)['mean'],3)
        with self.assertRaises(ValueError):model.slot_distribution(0,8)

    def test_two_hour_daily_counts_exclude_first_clear(self):
        for row in self.rows:
            p=row['rewards'];d=p['daily']
            self.assertAlmostEqual(d['successes']*row['duration_seconds']/0.9,7200)
            self.assertAlmostEqual(d['rift_only']['legendary'],p['repeat_total']['legendary']*d['successes'])
            self.assertEqual(d['all_repeat_sources'].get('guaranteed_legendary',0),0)

    def test_runtime_box_packages_match_the_planned_first_clear_budget(self):
        from wiki_reward_boxes import amount
        catalog=model.read(model.RES/'Data/RewardBoxes.json')
        definitions={b['id']:b for b in catalog['boxes']}
        for row in self.rows:
            stage=row['stage'];profile=row['rewards'];actual={}
            grants=[r['grant'] for r in catalog['firstClearRules'] if stage%r['every']==0]
            grants+=next((m['grants'] for m in catalog['milestones'] if m['stage']==stage),[])
            for g in grants:
                b=definitions[g['boxId']];key=b['kind']
                actual[key]=actual.get(key,0)+amount(b,stage)*g['count']
            basic=profile['first_clear_basic'];extra=profile['first_clear_milestone']
            self.assertEqual(actual.get('stones',0),basic['stones']+extra.get('stones',0),stage)
            self.assertEqual(actual.get('materials',0),basic['materials']-(10+stage//10)+extra.get('materials',0),stage)
            self.assertEqual(actual.get('gold',0),extra.get('gold',0),stage)
            self.assertEqual(actual.get('premium',0),extra.get('premium',0),stage)
            self.assertEqual(actual.get('gem',0),sum(v for k,v in extra.items() if k.startswith('choice_')),stage)
            self.assertEqual(actual.get('equipment',0),sum(v for k,v in extra.items() if k.startswith('guaranteed_')),stage)
            self.assertEqual(actual.get('rune',0),sum(v for k,v in extra.items() if k.startswith('gift_rune_')),stage)
            self.assertEqual(actual.get('cores',0),extra.get('selected_slot_cores',0),stage)


if __name__=='__main__':unittest.main(verbosity=2)
