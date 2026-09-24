#!/usr/bin/env python3
"""Analytical reward budgets for balance_1000; never mutates runtime balance.

Rarity probabilities are conditional on an equipment roll. Sets and awakening
are subsets/overlays, never additional rarity buckets. Count distributions use
finite independent Bernoulli trials; progression coverage uses the main model's
separately labelled Poisson approximation.
"""
from __future__ import annotations
import math

RARITIES = ('common', 'magic', 'rare', 'legendary')


def lerp(points, stage):
    if stage <= points[0][0]: return points[0][1:]
    if stage >= points[-1][0]: return points[-1][1:]
    for left, right in zip(points, points[1:]):
        if left[0] <= stage <= right[0]:
            t = (stage-left[0])/(right[0]-left[0])
            return [a+(b-a)*t for a,b in zip(left[1:],right[1:])]
    raise ValueError(stage)


def add(target, source, scale=1):
    for key, value in source.items(): target[key] = target.get(key, 0)+value*scale
    return target


def total(bundles):
    result = {}
    for bundle in bundles: add(result, bundle)
    return result


def rarity_profile(row, config):
    common, magic, elite_magic, chest_magic = lerp(config['nonlegendary_anchors'], row['stage'])
    pn, pe, pb = (row[k] for k in ('normal_legendary','elite_legendary','boss_legendary'))
    return {
        'normal': [common*(1-pn), magic*(1-pn), (1-common-magic)*(1-pn), pn],
        'elite': [0, elite_magic*(1-pe), (1-elite_magic)*(1-pe), pe],
        'boss': [0, 0, 1-pb, pb],
        'chest': [0, chest_magic*(1-pe/2), (1-chest_magic)*(1-pe/2), pe/2],
        'goblin': [0, 0, 1-pe, pe]
    }


def set_fraction(uniques, hero_class=0):
    per_slot=[]
    for slot in range(8):
        pool=[u for u in uniques if u['heroClass'] in (-1,hero_class) and u['slot']==slot]
        per_slot.append(sum(u['weight'] for u in pool if u['setId'])/sum(u['weight'] for u in pool))
    return sum(per_slot)/8


def count_distribution(trials):
    """Exact Poisson-binomial count quantiles, including deterministic trials."""
    dist=[1.]
    for n, probability in trials:
        for _ in range(n):
            next_dist=[0.]*(len(dist)+1)
            for k,p in enumerate(dist):
                next_dist[k]+=p*(1-probability)
                next_dist[k+1]+=p*probability
            dist=next_dist
    def quantile(q):
        cumulative=0.
        for n,p in enumerate(dist):
            cumulative+=p
            if cumulative>=q-1e-12:return n
        return len(dist)-1
    return dict(mean=sum(n*p for n,p in enumerate(dist)),p10=quantile(.1),p50=quantile(.5),p90=quantile(.9),
                at_least_one=1-dist[0],probability_mass=sum(dist))


def item_levels(center, offsets):
    result={}
    for offset, probability in offsets:
        level=max(1,min(60,int(math.floor(center+.5))+offset))
        result[level]=result.get(level,0)+probability
    return [{'level':k,'probability':v} for k,v in sorted(result.items())]


def equipment_bundle(count, rarity, sets, awakening):
    result=dict(zip(RARITIES,[count*p for p in rarity]))
    result['set_inside_legendary']=result['legendary']*sets
    result['awakened_rare_or_legendary']=(result['rare']+result['legendary'])*awakening
    return result


def salvage(bundle, stage, config):
    ordinary=config['ordinary_fraction']
    legendary=lerp(config['legendary_fraction_anchors'],stage)[0]
    c,m,r,l=(bundle.get(k,0) for k in RARITIES)
    sets=bundle.get('set_inside_legendary',0)
    return {'materials':ordinary*(c+2*m+5*r),
            'stones':ordinary*(m+5*r)+legendary*(15*l-5*sets),
            'cores':legendary*l}


def guaranteed_single_slots(config,stage):
    slots={gear['slot'] for milestone in config['milestones'] if milestone['stage']<=stage
           for gear in milestone.get('equipment',[]) if gear['rarity']==3}
    if any(slot not in range(7) for slot in slots):
        raise ValueError('Ring guarantees need a separate two-position coverage model')
    return len(slots)


def first_clear(stage, row, config):
    f=config['first_clear']
    basic={'gold':f['base_gold']+f['gold_per_stage']*stage,
           'materials':f['base_materials']+stage//f['material_divisor'],
           'stones':f['base_stones']+stage//f['stone_divisor']}
    if stage%10==0:basic['materials']+=f['ten_stage_material_base']+stage//f['ten_stage_material_divisor']
    if stage%50==0:basic['stones']+=f['fifty_stage_stone_base']+stage//f['fifty_stage_stone_divisor']
    milestone=next((m for m in config['milestones'] if m['stage']==stage),None)
    bonus={}
    if milestone:
        for key in ('gold','materials','stones','selected_slot_cores','premium'):
            bonus[key]=milestone.get(key,0)
        level=int(row['item_level'])
        bonus['gold']+=milestone.get('reroll_attempts_gold',0)*50*level*(level+9)
        for gear in milestone.get('equipment',[]):
            key='guaranteed_'+RARITIES[gear['rarity']]
            bonus[key]=bonus.get(key,0)+1
        for gem in milestone.get('gems',[]):
            key='choice_'+gem.get('reference_family','diamond')+'_gem_t'+str(gem['tier'])
            bonus[key]=bonus.get(key,0)+gem['count']
        for rune in milestone.get('runes',[]):
            key='gift_rune_g'+str(rune['grade'])
            bonus[key]=bonus.get(key,0)+rune['count']
    return basic,bonus,milestone


def reward_profile(row, config, main, uniques, cycle_seconds):
    s=row['stage'];eq=config['equipment'];currency=config['currency']
    rarities=rarity_profile(row,config)
    sets=set_fraction(uniques,main.get('reference_class_index',0))
    awake=lerp(config['awakening_chance_anchors'],s)[0]
    goblin=eq['goblin_spawn_chance']*eq['goblin_kill_fraction'] if s>=eq['goblin_from_stage'] else 0
    trials={'normal':(main['normal_kills_per_success'],eq['normal_drop_chance']),
            'elite':(main['elite_kills_per_success'],eq['elite_drop_chance']),
            'boss':(eq['boss_rolls'],1),'chest':(eq['chest_equipment_rolls'],eq['chest_open_fraction']),
            'goblin':(1,goblin)}
    loot={name:equipment_bundle(n*p,rarities[name],sets,awake) for name,(n,p) in trials.items()}
    n,e=main['normal_kills_per_success'],main['elite_kills_per_success']
    loot['normal']['gold']=n*(5+s);loot['elite']['gold']=e*(25+5*s)
    old_boss_gold=800+50*s
    stone_total=10+2*s if s<=currency['stone_anchor_stage'] else currency['stone_anchor_count']+currency['stone_late_slope']*(s-currency['stone_anchor_stage'])
    boss_stones=math.floor(stone_total*currency['boss_stone_fraction'])
    clear_stones=math.floor(stone_total)-boss_stones
    loot['boss'].update(gold=math.floor(old_boss_gold*currency['boss_gold_fraction']),
        materials=currency['boss_material_base']+s//currency['boss_material_divisor'],stones=boss_stones)
    loot['clear']={'gold':old_boss_gold-loot['boss']['gold'],
        'materials':currency['clear_material_base']+s//currency['clear_material_divisor'],'stones':clear_stones}
    loot['chest'].update(gold=math.floor(.15*old_boss_gold)*eq['chest_open_fraction'],
                         materials=max(1,math.floor(.2*(5+s//5)))*eq['chest_open_fraction'])
    loot['goblin']['gold']=8*(25+5*s)*goblin
    gem=config['gems'];tier=sum(s>=gate for gate in gem['tier_starts']) if s>=gem['unlock'] else 0
    for name,count in [('normal',n*gem['normal_chance']),('elite',e*gem['elite_chance']),('boss',gem['boss_count'])]:
        if tier:loot[name]['gem_t'+str(tier)]=count
    rune=config['runes'];grade=sum(s>=gate for gate in rune['grade_starts'])-1
    if s>=rune['unlock']:
        for name,count in [('normal',n*rune['normal_chance']),('elite',e*rune['elite_chance']),('boss',rune['boss_count'])]:
            loot[name]['rune_g'+str(grade)]=count
    repeat=total(loot.values())
    low=total(loot[k] for k in ('normal','elite','boss','clear'))
    salvage_budget=salvage(repeat,s,config['salvage'])
    # Sweep substitutes the boss + clear bundles, excluding mastery and runes.
    sweep=total(loot[k] for k in ('boss','clear'))
    sweep={k:v for k,v in sweep.items() if not k.startswith('rune_')}
    basic,bonus,milestone=first_clear(s,row,config)
    sources={name:{'trials':n,'equipment_chance_per_trial':p,'expected_equipment':n*p,
                  'rarity_probabilities':rarities[name], 'expected_by_rarity':{k:loot[name][k] for k in RARITIES}}
             for name,(n,p) in trials.items()}
    # Forecast the same 5% policy from proposed recurring field income. Runtime uses
    # the player's last actual clear; this model uses the proposed successful cycle.
    offline_factor=3600/(max(5,cycle_seconds)*currency['offline_divisor'])
    field=total(loot[k] for k in ('normal','elite','boss','clear','goblin'))
    offline={key:field.get(key,0)*offline_factor for key in ('gold','stones')}
    return {'stage':s,'sources':sources,'repeat_by_source':loot,'repeat_total':repeat,
            'repeat_salvage':salvage_budget,'low_optional_total':low,
            'sweep_total':sweep,'sweep_salvage':salvage(sweep,s,config['salvage']),
            'offline_per_hour':offline,
            'first_clear_basic':basic,'first_clear_milestone':bonus,'milestone':milestone,
            'equipment_count_distribution':count_distribution(list(trials.values())),
            'legendary_count_distribution':count_distribution([(n,p*rarities[name][3]) for name,(n,p) in trials.items()]),
            'boss_at_least_one_legendary':1-(1-row['boss_legendary'])**eq['boss_rolls'],
            'legendary_set_fraction':sets,'awakening_chance':awake,
            'item_level_distribution':item_levels(row['item_level'],config['item_level_offsets']),
            'gem_drop_tier':tier,'rune_drop_grade':grade,'rune_size_weights':rune['size_weights'][grade] if grade>=0 else [0]*5}


class RewardLedger:
    def __init__(self,config):
        self.config=config;self.by_source={};self.low={}
    def record(self,profile,wins,sweeps,offline_hours):
        stage=profile['stage'];cfg=self.config
        increments={name:{k:v*wins for k,v in bundle.items()} for name,bundle in profile['repeat_by_source'].items()}
        increments.update(salvage_run={k:v*wins for k,v in profile['repeat_salvage'].items()},
            sweep={k:v*sweeps for k,v in profile['sweep_total'].items()},
            salvage_sweep={k:v*sweeps for k,v in profile['sweep_salvage'].items()},
            offline={k:v*offline_hours for k,v in profile['offline_per_hour'].items()},
            first_basic=profile['first_clear_basic'],first_milestone=profile['first_clear_milestone'])
        for name,bundle in increments.items():add(self.by_source.setdefault(name,{}),bundle)
        low_add=total(b for k,b in increments.items() if k not in ('chest','goblin','salvage_run'))
        add(low_add,salvage(profile['low_optional_total'],stage,cfg['salvage']),wins)
        add(self.low,low_add)
        cumulative=total(self.by_source.values())
        random_t1=sum(cumulative.get('gem_t'+str(t),0)*5**(t-1) for t in range(1,7))
        choice_t1=sum(cumulative.get('choice_diamond_gem_t'+str(t),0)*5**(t-1) for t in range(1,7))
        ruby_choice=sum(cumulative.get('choice_ruby_gem_t'+str(t),0)*5**(t-1) for t in range(1,7))
        # Reference allocation: ten diamonds and ten rubies at R10; later ten-gem packs to diamonds.
        return {'cumulative':cumulative,'cumulative_currency_by_source':{name:{k:b.get(k,0) for k in ('gold','materials','stones','cores','premium')} for name,b in self.by_source.items()},
                'low_optional_cumulative':dict(self.low),'diamond_t1_supply':random_t1/6+choice_t1,
                'ruby_t1_supply':random_t1/6+ruby_choice,'choice_gem_t1_equivalent':choice_t1+ruby_choice}


def daily_budget(profile,duration,main):
    successes=main['rift_minutes_per_day']*60/duration*main['success_rate']
    sweeps=main['sweeps_per_day'] if profile['stage']>=main['sweep_unlock'] else 0
    result={}
    for bundle,scale in [(profile['repeat_total'],successes),(profile['repeat_salvage'],successes),
                         (profile['sweep_total'],sweeps),(profile['sweep_salvage'],sweeps),
                         (profile['offline_per_hour'],main['offline_hours_per_day'])]:add(result,bundle,scale)
    return {'successes':successes,'sweeps':sweeps,'rift_only':{k:v*successes for k,v in profile['repeat_total'].items()},
            'all_repeat_sources':result,'first_clears_included':False}


def tables(rows, english=False):
    levels=[1,10,15,30,50,100,200,300,500,750,1000]
    title='Rift reward calculations' if english else '균열 보상 계산표'
    lines=['# '+title,'','Updated: 2026-09-24' if english else '갱신일: 2026-09-24','',
      'Planning only. Counts are means per successful full clear, including optional chest/goblin assumptions. Rarity is conditional on a gear roll. Sets are inside legendary.' if english else
      '게임 미반영 기획안입니다. 획득량은 성공 판에서의 평균이며 상자·고블린 가정을 포함합니다. 등급 확률은 장비가 나온 경우의 조건부 확률입니다. 세트는 전설 안에 포함합니다.','',
      '|'+('Rift|Normal C/M/R/L %|Elite M/R/L %|Boss R/L %|Chest M/R/L %|Goblin R/L %|' if english else '균열|일반 일반/마법/희귀/전설 %|정예 마법/희귀/전설 %|보스 희귀/전설 %|상자 마법/희귀/전설 %|고블린 희귀/전설 %|'),
      '|---|---|---|---|---|---|']
    for s in levels:
        p=rows[s-1]['rewards'];values=[]
        for name,start in [('normal',0),('elite',1),('boss',2),('chest',1),('goblin',2)]:
            values.append('/'.join(f'{v*100:.3f}' for v in p['sources'][name]['rarity_probabilities'][start:]))
        lines.append('|'+str(s)+'|'+'|'.join(values)+'|')
    lines+=['','|'+('Rift|C/M/R/L per clear|Gear P10/50/90|≥1 legendary|Boss ≥1 legendary|Gem count/tier|Rune count/grade|' if english else '균열|판당 일반/마법/희귀/전설|장비 P10/50/90|전설 ≥1개|보스 전설 ≥1개|보석 개수/단계|룬 개수/등급|'),'|---|---|---|---:|---:|---|---|']
    for s in levels:
        p=rows[s-1]['rewards'];b=p['repeat_total'];d=p['equipment_count_distribution'];g=p['gem_drop_tier'];r=p['rune_drop_grade']
        lines.append(f"|{s}|"+'/'.join(f'{b[k]:.3f}' for k in RARITIES)+f"|{d['p10']}/{d['p50']}/{d['p90']}|{p['legendary_count_distribution']['at_least_one']*100:.2f}%|{p['boss_at_least_one_legendary']*100:.2f}%|{b.get('gem_t'+str(g),0):.2f}/{'T'+str(g) if g else '—'}|{b.get('rune_g'+str(r),0):.2f}/"+(f'G{r}' if r>=0 else '—')+'|')
    lines+=['','|'+('Rift|Boss gold/materials/stones|Repeat bonus gold/materials/stones|First-clear basic gold/materials/stones|Salvage materials/stones/cores|' if english else '균열|보스 골드/재료/강화석|반복 보너스 골드/재료/강화석|첫 클리어 기본 골드/재료/강화석|분해 재료/강화석/코어|'),'|---|---|---|---|---|']
    for s in levels:
        p=rows[s-1]['rewards'];bundles=[p['repeat_by_source']['boss'],p['repeat_by_source']['clear'],p['first_clear_basic'],p['repeat_salvage']]
        lines.append('|'+str(s)+'|'+'|'.join('/'.join(f'{b.get(k,0):,.1f}' for k in (('materials','stones','cores') if i==3 else ('gold','materials','stones'))) for i,b in enumerate(bundles))+'|')
    lines+=['','|'+('Rift|Successful runs/day|Equipment/day|Legendary/day|Gems/day|Runes/day|Gold/day|Materials/day|Stones/day|' if english else '균열|일일 성공 판수|일일 장비|일일 전설|일일 보석|일일 룬|일일 골드|일일 재료|일일 강화석|'),'|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|']
    for s in levels:
        d=rows[s-1]['rewards']['daily'];b=d['all_repeat_sources']
        values=[d['successes'],sum(b.get(k,0) for k in RARITIES),b.get('legendary',0),sum(v for k,v in b.items() if k.startswith('gem_t')),sum(v for k,v in b.items() if k.startswith('rune_g')),b['gold'],b['materials'],b['stones']]
        lines.append('|'+str(s)+'|'+'|'.join(f'{v:,.2f}' for v in values)+'|')
    lines+=['','Daily totals include 2h rifts, unlocked sweeps, 12h Offline Supplies at 5% of recurring field gold/stones, and salvage. First-clear gifts and offline materials are excluded. All 1,000 rows are in [the full dataset](projection.json).' if english else '일일 합계에는 균열 2시간·해금 후 소탕·미접속 보급 12시간·분해가 포함됩니다. 보급은 반복 필드 골드·강화석 수급 속도의 5%이며 일반 재료와 최초 보상은 제외합니다. 1,000개 단계 전체 값은 [계산 데이터](projection.json)에 있습니다.','']
    return '\n'.join(lines)
