#!/usr/bin/env python3
"""Reproducible, source-anchored planning model. Never writes runtime data or saves.

The loot model uses Poisson thinning, not a combat simulator. Shapley attribution
is exact for the explicitly defined analytical model, not for gameplay traces.
"""
from __future__ import annotations
import argparse
import hashlib
import json
import math
from pathlib import Path
import balance_rewards as rewards

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "Docs/Design/Balance1000"
CORE = ROOT / "Assets/HELLSCRIPT/Runtime/Core"
RES = ROOT / "Assets/HELLSCRIPT/Resources"
GROUPS = ["character", "equipment", "enhancement", "slots", "quality", "runes", "skills", "legendary_set", "gems"]
NAMES_KO = ["캐릭터 레벨", "장비·접사·재설정", "장비 강화", "슬롯 강화", "각성·걸작", "룬·숙련", "스킬·패시브", "전설·세트·위상", "보석"]
NAMES_EN = ["Character level", "Gear, affixes, reroll", "Gear enhancement", "Slot growth", "Awakening, masterwork", "Runes, mastery", "Skills, passives", "Legendary, set, aspect", "Gems"]


def read(path):
    return json.loads(path.read_text())


def interpolate(config, stage):
    keys = config["milestone_columns"]
    points = config["milestones"]
    left, right = points[0], points[-1]
    for a, b in zip(points, points[1:]):
        if a[0] <= stage <= b[0]:
            left, right = a, b
            break
    t = (stage-left[0]) / (right[0]-left[0]) if right[0] != left[0] else 0
    row = dict(zip(keys, [a+(b-a)*t for a,b in zip(left,right)]))
    row["stage"] = stage
    # Keep the first seven days identical between calendar scenarios.
    if row["day"] > 7:
        row["day"] = 7+(row["day"]-7)*(config["days_to_1000"]-7)/173
    for key, gate in [("masterwork",80),("rune_fraction",15),("gem_tier",10)]:
        if stage < gate:
            row[key] = 0
    if stage <= 5:row["slot_level"] = 1
    if stage < config["rift_legendary_start_stage"]:
        for key in ("normal_legendary","elite_legendary","boss_legendary","legendary_bonus"):
            row[key] = 0
    if stage < config["reroll_unlock"]:row["rerolls_total"] = 0
    return row


def forge_costs(level):
    stones, minutes, all_stones, all_minutes = 10,5,0,0
    for t in range(2, int(level)+1):
        if t >= 3:
            stones += 500 if t == 100 else 50 if t >= 75 else 20 if t >= 50 else 10 if t >= 25 else 5
            minutes += 1440 if t == 100 else 60 if t >= 75 else 30 if t >= 50 else 10 if t >= 25 else 5
        all_stones += stones
        all_minutes += minutes
    return all_stones, all_minutes


def enhancement_cost(ilvl, target):
    k = target-1
    return math.ceil((100*ilvl+10*ilvl*ilvl)*(400+20*k+k*k)/400)


def slot_stats(level, definitions):
    total = {}
    for slot in definitions:
        for g in slot["growth"]:
            v = 0 if level < g["unlock"] else g["rate"]*(1 if g["once"] else level-g["unlock"]+1)
            total[g["stat"]] = total.get(g["stat"],0)+v
    # Proposal: keep the first ten levels, then flatten attack growth.
    total["attack"] = 8.5*level if level <= 10 else 85+3*(level-10)
    return total


def rune_bounds():
    data = read(RES / "Runes/V13/catalog.json")
    abilities = {a["id"]:a for a in data["abilities"]}
    result = {}
    for board in data["boards"]:
        sums = {}
        for node in board["nodes"]:
            a = abilities.get(node["ability"],{})
            symbol = a.get("symbol",node["ability"])
            sums[symbol] = sums.get(symbol,0)+node["value"]
        result[board["weapon"]] = {k:round(v,6) for k,v in sums.items()}
    return result


def analytical_metrics(row, definitions, config, hero_class=0, attribute=True):
    """Return total metrics and exact Shapley shares over 512 coalitions.

    Expected affixes: a fixed legal four-line endpoint on each of nine items.
    Coverage ramps by gear_fraction. Quality is an expectation (not one item).
    Conditional skills and legendary uptime are declared planning inputs.
    """
    base_slot = slot_stats(1, definitions)
    slots = slot_stats(row["slot_level"], definitions)
    # Columns: primary, HP, armor, resist, crit pp, crit damage pp,
    # attack speed pp, elemental damage pp, HP pp, CDR pp, dodge pp.
    # Counts arise from the explicit nine-item loadout in the report.
    q = row["affix_quality"]
    flat_scale = (1+.03*(row["item_level"]-1))/1.87
    coverage = row["gear_fraction"]
    aff = [7*(10+15*q)*flat_scale, 3*(40+60*q)*flat_scale,
           2*(25+35*q)*flat_scale, 2*(15+20*q)*flat_scale,
           4*(2+2*q), 3*(5+7*q), 3+5*q, 4*(3+5*q),
           3*(3+5*q), 2*(3+3*q), 2+3*q]
    aff = [v*coverage for v in aff]
    rune = row["rune_fraction"]
    values = []
    for mask in (range(1 << len(GROUPS)) if attribute else [(1 << len(GROUPS))-1]):
        b = [(mask>>i)&1 for i in range(len(GROUPS))]
        character,gear,enh,sl,quality,ru,sk,leg,gem = b
        lvl = 1+(row["hero_level"]-1)*character
        ilvl = 1+(row["item_level"]-1)*gear
        ss = {k:base_slot.get(k,0)+(slots.get(k,0)-base_slot.get(k,0))*sl for k in slots}
        aw = row["awakened_fraction"]*quality
        mw = row["masterwork"]*quality
        main_mult = (1+.25*aw)*(1+config["masterwork_step"]*mw)
        # Three line hits at 4/8/12, distributed over four lines in expectation.
        aff_mult = (1+.05*aw)*(1+.0625*min(3,math.floor(mw/4)))
        a = [v*gear*aff_mult for v in aff]
        e = row["enhance"]*enh
        primary = 30+2*(lvl-1)+ss.get("allStats",0)+a[0]+[5.5,8.5,7.5][hero_class]*rune*ru
        dex = primary if hero_class==1 else 10+lvl-1+ss.get("allStats",0)+[3,0,2][hero_class]*rune*ru
        intel = primary if hero_class==2 else 10+lvl-1+ss.get("allStats",0)+2*rune*ru
        strength = primary if hero_class==0 else 10+lvl-1+ss.get("allStats",0)
        attack = (20*(1+.08*(ilvl-1))*main_mult+2*e+ss["attack"]+1.6*rune*ru)*(1+.002*primary)
        gear_main = coverage*gear
        life_base = 160*(1+.08*(ilvl-1))*gear_main*main_mult+8*e*gear_main
        armor = strength*2+(60*(1+.08*(ilvl-1))*main_mult+5*e)*gear_main+ss.get("armor",0)+a[2]+[13.5,10.5,10.5][hero_class]*rune*ru
        resist = intel+a[3]
        tier = min(6,int(row["gem_tier"]))*gem
        gem_damage = [0,3,5,8,12,17,23][tier]/100
        gem_hp = 2*[0,2,3.5,5,7,9.5,12][tier]/100
        resist += 3*[0,4,7,11,16,22,30][tier]
        hp = ([300,240,210][hero_class]+[35,28,25][hero_class]*(lvl-1)+life_base+ss.get("life",0)+a[1]+[22,22,26][hero_class]*rune*ru)*(1+(a[8]+ss.get("lifePercent",0)+[1.8,1.8,2.4][hero_class]*rune*ru)/100+gem_hp)
        crit = min(.75,.05+dex*.0003+(a[4]+ss.get("crit",0)+2*(1+.02*(ilvl-1))*gear_main*main_mult+.04*e*gear_main+1.8*rune*ru)/100)
        cd = 1.5+(a[5]+ss.get("critDamage",0)+11.5*rune*ru)/100
        speed = min(1.5,1+(a[6]+ss.get("attackSpeed",0)+(2+.04*(ilvl-1))*gear_main*main_mult+.08*e*gear_main+2*rune*ru)/100)
        # No potion and no physical-mitigation advantage are required to pass.
        additive = a[7]/100+gem_damage+.30*rune*ru+.20*min(1,(row["hero_level"]-1)/9)*sk
        coefficient = 1+.10*(row["skill_rank"]-1)*sk+.40*rune*ru
        raw_dps = attack*speed*(1+crit*(cd-1))*(1+additive)*coefficient*(1+row["legendary_bonus"]*leg)
        # Proposed common reference defense: capped at 1000, 20% initial DR.
        defense = 27.5+972.5*(row["stage"]-1)/999
        dr_enemy = min(.75,defense/(defense+100+10*lvl))
        dps = raw_dps*(1-dr_enemy)
        rating = 100+10*min(30,row["stage"])
        physical_dr = min(.75,armor/(armor+rating))
        element_dr = min(.7,resist/(resist+rating)+ss.get("allResistance",0)/100)
        reduction = min(.85,(ss.get("damageReduction",0)+.7*rune*ru)/100)
        dodge = min(.6,(dex*.025+a[10]+ss.get("dodge",0))/100)
        ehp = hp / ((1-.5*(physical_dr+element_dr))*(1-reduction)*(1-dodge))
        values.append((attack,dps,ehp,math.sqrt(dps*ehp)))
    totals = values[-1]
    if not attribute:
        return dict(attack_power=totals[0],boss_dps=totals[1],mixed_ehp=totals[2],hp=hp,armor=armor,resistance=resist)
    shares = []
    fact = [math.factorial(i) for i in range(len(GROUPS)+1)]
    for metric in range(4):
        parts = {"baseline":values[0][metric]/totals[metric]}
        for i,key in enumerate(GROUPS):
            contribution = 0
            for mask in range(1 << len(GROUPS)):
                if mask & (1<<i):
                    continue
                count = bin(mask).count("1")
                weight = fact[count]*fact[len(GROUPS)-count-1]/fact[len(GROUPS)]
                contribution += weight*(values[mask|(1<<i)][metric]-values[mask][metric])
            parts[key] = contribution/totals[metric]
        shares.append(parts)
    # A named design index, not an existing game combat-power field.
    power = math.sqrt(totals[1]*totals[2])
    return dict(attack_power=totals[0],boss_dps=totals[1],mixed_ehp=totals[2],hp=hp,
                crit=crit,attack_speed=speed,damage_reduction_enemy=dr_enemy,
                armor=armor,resistance=resist,power_index=power,
                attack_share=shares[0],dps_share=shares[1],ehp_share=shares[2],
                power_share=shares[3])


def slot_distribution(mu, guaranteed_weapon=False):
    lam = mu/8
    zero = math.exp(-lam)
    guaranteed = int(guaranteed_weapon)
    if not 0 <= guaranteed <= 7: raise ValueError("Guaranteed single slots must be 0..7")
    dist = [0.]*guaranteed+[1.]
    for i in range(7-guaranteed):
        next_dist = [0.]*(len(dist)+1)
        for n,p in enumerate(dist):
            next_dist[n] += p*zero
            next_dist[n+1] += p*(1-zero)
        dist = next_dist
    ring = [zero,lam*zero,1-zero*(1+lam)]
    final = [0.]*10
    for n,p in enumerate(dist):
        for k,q in enumerate(ring):
            final[n+k] += p*q
    def quantile(q):
        total=0
        for n,p in enumerate(final):
            total+=p
            if total>=q-1e-12:return n
        return 9
    return {"mean":sum(n*p for n,p in enumerate(final)),"p10":quantile(.1),"p50":quantile(.5),"p90":quantile(.9),"full_probability":final[9]}


def complete_set_probability(mu, uniques, set_id="SW"):
    relevant = [u for u in uniques if u["heroClass"] in (-1,0)]
    probs=[]
    for slot in range(1,5):
        pool=[u for u in relevant if u["slot"]==slot]
        selected=next(u for u in pool if u["id"]==set_id+str(slot))
        lam=mu/8*selected["weight"]/sum(u["weight"] for u in pool)
        probs.append(1-math.exp(-lam))
    return math.prod(probs)


def source_manifest():
    paths=[CORE/n for n in ["Economy.cs","Blacksmith.cs","HeroStats.Blacksmith.cs","ItemQuality.cs","Itemization.cs","AspectStone.cs","AspectGrowth.cs","CombatSimulation.cs","CombatSimulation.Damage.cs","CombatSimulation.Incoming.cs","CombatEffects.cs","RiftRarity.cs","RiftRarityBalance.cs","RiftEntry.cs","RiftGenerator.cs","RuneEconomy.cs","RuneMasteryProgress.cs","RuneCombatBonuses.cs","GemCatalog.cs","GemStacks.cs","GemElixirs.cs","GameStore.cs","ClassSkills.cs","EquipmentSlots.cs"]]
    paths += [CORE/n for n in ["GemInventory.cs","GameStore.SkillTree.cs","ClassSkillTree.cs",
                              "CombatSimulation.Goblin.cs","RiftExploration.cs","EdictFieldPolicy.cs",
                              "CoreCrafting.cs","Economy.ContentServices.cs","RuneGrowth.cs","IntroductoryRift.cs","RiftSurface.cs","RewardBoxes.cs","ContentUnlocks.cs","GambleShop.cs"]]
    paths += [CORE.parent/"Presentation/GameController.cs"]
    paths += [RES/n for n in ["ContentUnlocks.json","BlacksmithSlots.json","Data/ClassSkills.json","Data/ClassSkillTree.json","Runes/V13/catalog.json","Data/RewardBoxes.json"]]
    return {str(p.relative_to(ROOT)):hashlib.sha256(p.read_bytes()).hexdigest() for p in paths}


def generate(config, cached_metrics=None):
    definitions=read(RES/"BlacksmithSlots.json")["slots"]
    uniques=read(OUT/"current_runtime.json")["uniques"]
    reward_config=read(OUT/"reward_parameters.json")
    ledger=rewards.RewardLedger(reward_config)
    rows=[]
    days=wins=attempts=gold=materials=stones=gems=legends=current_legends=0.
    cost_gold=cost_materials=0.
    enhancement_gold=masterwork_gold=reroll_gold=consumable_gold=socket_gold=0.
    previous_socket_ilvl=0
    prev_enh=prev_mw=prev_rerolls=0
    proposed_mastery_xp=0.
    for stage in range(1,1001):
        row=interpolate(config,stage)
        metrics=analytical_metrics(row,definitions,config) if cached_metrics is None else cached_metrics[stage-1]
        class_targets={"Warrior":{k:metrics[k] for k in ("attack_power","boss_dps","mixed_ehp","hp","armor","resistance")}}
        for index,name in [(1,"Ranger"),(2,"Mage")]:class_targets[name]=analytical_metrics(row,definitions,config,index,False)
        weakest_ehp=min(target["mixed_ehp"] for target in class_targets.values())
        progress=(stage-1)/999
        normal_ttk=1.8+.4*progress
        boss_seconds=35+25*progress
        intro=config["introductory_rift"];early=stage<=intro["last_stage"]
        normal,elite=(intro["normal_count"],intro["elite_count"]) if early else (config["normal_kills_per_success"],config["elite_kills_per_success"])
        t=(stage-1)/(intro["last_stage"]-1)
        hp_scale=intro["health_start"]+(intro["health_end"]-intro["health_start"])*t if early else 1
        boss_scale=intro["boss_health_start"]+(intro["boss_health_end"]-intro["boss_health_start"])*t if early else 1
        attack_scale=intro["attack_start"]+(intro["attack_end"]-intro["attack_start"])*t if early else 1
        normal_ttk*=hp_scale;boss_seconds*=boss_scale
        duration=75*(intro["geometry_scale"] if early else 1)+normal_ttk*(normal+elite*3)/3.5+boss_seconds+reward_config["extra_seconds_per_attempt"]
        delta=row["day"]-days
        new_attempts=delta*config["rift_minutes_per_day"]*60/duration
        new_wins=new_attempts*config["success_rate"]
        if new_wins < 1-1e-7:
            raise ValueError(f"R{stage}: fewer than one expected first clear in allocated time")
        sweep_count=delta*config["sweeps_per_day"] if stage>=config["sweep_unlock"] else 0
        profile=rewards.reward_profile(row,reward_config,{**config,"normal_kills_per_success":normal,"elite_kills_per_success":elite},uniques,duration)
        account=ledger.record(profile,new_wins,sweep_count,delta*config["offline_hours_per_day"] if stage>1 else 0)
        cumulative=account["cumulative"]
        gold=cumulative["gold"];materials=cumulative["materials"];stones=cumulative["stones"]
        legends=cumulative.get("legendary",0)
        gems=sum(v for k,v in cumulative.items() if k.startswith("gem_t") or k.startswith("choice_") and "_gem_t" in k)
        profile["daily"]=rewards.daily_budget(profile,duration,config)
        profile["ledger"]=account
        stone_reward=profile["repeat_by_source"]["boss"]["stones"]+profile["repeat_by_source"]["clear"]["stones"]
        g=(stage-1)/(stage+48)
        current_probs=[.01+.09*g,.05+.20*g,.15+.30*g] if stage>=config["rift_legendary_start_stage"] else [0,0,0]
        # Same modeled successful attempts; current rewards use current drop chances.
        current_per_run=normal*.02*current_probs[0]+elite*current_probs[1]+3*current_probs[2]
        current_per_run+=(.5*(.02+.01) if stage>=config["rift_legendary_start_stage"] else 0)+(.08*current_probs[1] if stage>=5 else 0)
        current_legends+=new_wins*current_per_run+sweep_count*3*current_probs[2]
        random_equipped=slot_distribution(legends)
        guaranteed_slots=rewards.guaranteed_single_slots(reward_config,stage)
        proposal_equipped=slot_distribution(legends,guaranteed_slots)
        current_equipped=slot_distribution(current_legends)
        level=int(row["item_level"])
        # A concrete socket replacement budget; old gems are recovered, not lost.
        if stage in config["socket_replacement_stages"]:
            paid=6*(2000*level+500*previous_socket_ilvl)
            socket_gold+=paid;cost_gold+=paid;cost_materials+=6*30
            previous_socket_ilvl=level
        e=int(row["enhance"])
        for t in range(prev_enh+1,e+1):
            paid=9*enhancement_cost(level,t)*1.35*config.get("enhancement_cost_multiplier",.8)
            cost_gold+=paid;enhancement_gold+=paid
        m=int(row["masterwork"])
        for t in range(prev_mw+1,m+1):
            paid=9*200*t*1.15;cost_gold+=paid;masterwork_gold+=paid
            cost_materials+=9*(20+2*t)*1.15
        rerolls=int(row["rerolls_total"])
        paid=(rerolls-prev_rerolls)*50*level*(level+9);cost_gold+=paid;reroll_gold+=paid
        # HP/resource/basic utility budget; no premium currency or elixir upkeep.
        cost_gold+=delta*7200;consumable_gold+=delta*7200
        slot_stones,slot_minutes=forge_costs(row["slot_level"])
        current_ideal_days=10*slot_minutes/1440/config["forge_stations"]
        proposed_queue_days=current_ideal_days*config["forge_time_multiplier"]/config["forge_queue_utilization"]
        proposed_mastery_xp+=new_wins*(normal+5*elite+30) if stage>=15 else 0
        mastery_level=min(41,int((1+math.sqrt(1+8*proposed_mastery_xp/1000))/2))
        # Target experience payout needed for the chosen character-level schedule.
        def total_xp(lvl):
            l=int(lvl);return sum(100+60*n+15*n*n for n in range(l-1))+(lvl-l)*(100+60*(l-1)+15*(l-1)**2)
        prev_xp=total_xp(rows[-1]["hero_level"]) if rows else 0
        xp_per_success=max(0,total_xp(row["hero_level"])-prev_xp)/new_wins
        row.update(metrics)
        row.update(duration_seconds=duration,expected_attempts=new_attempts,expected_successes=new_wins,
                   cumulative_successes=wins+new_wins,cumulative_attempts=attempts+new_attempts,
                   farming_successes=new_wins-1,normal_hp=metrics["boss_dps"]*normal_ttk,
                   elite_hp=metrics["boss_dps"]*normal_ttk*3,boss_hp=metrics["boss_dps"]*boss_seconds,
                   normal_attack=weakest_ehp*.045*attack_scale,elite_attack=weakest_ehp*.09*attack_scale,
                   boss_attack=weakest_ehp*.16*attack_scale,class_targets=class_targets,
                   current_normal_hp=70*1.08**(stage-1)*hp_scale,current_normal_attack=18*1.055**(stage-1)*attack_scale,
                   cumulative_legendary_drops=legends,natural_legendary_coverage=random_equipped,
                   proposal_equipped_coverage=proposal_equipped,
                   current_rates_coverage=current_equipped,first_guarantee_earned=guaranteed_slots>0,
                   target_set4_probability=complete_set_probability(legends,uniques),
                   current_rates_set4_probability=complete_set_probability(current_legends,uniques),
                   rewards=profile,
                   gold_earned=gold,gold_required=cost_gold,gold_reserve=gold-cost_gold,
                   gold_cost_by_content={"enhancement":enhancement_gold,"masterwork":masterwork_gold,"reroll":reroll_gold,"consumables":consumable_gold,"sockets":socket_gold},
                   materials_earned=materials,materials_required=cost_materials,materials_reserve=materials-cost_materials,
                   stones_earned=stones,stones_required=10*slot_stones,
                   stones_per_clear_target=stone_reward,
                   gems_earned=gems,gem_same_color_tier1_equivalent=account["diamond_t1_supply"],
                   gem_ruby_tier1_equivalent=account["ruby_t1_supply"],
                   gem_ruby_tier1_required=2*5**(int(row["gem_tier"])-1) if stage>=10 else 0,
                   gem_diamond_tier1_required=4*5**(int(row["gem_tier"])-1) if stage>=10 else 0,
                   rune_cell_capacity_upper_bound=min(259,19+6*(mastery_level-1)+6),
                   rune_active_cell_target=259*row["rune_fraction"],
                   current_ideal_forge_days=current_ideal_days,
                   proposed_forge_days=proposed_queue_days,proposed_mastery_level=mastery_level,
                   hero_xp_per_success_target=xp_per_success,
                   optional_attack_potion_average_dps=metrics["boss_dps"]*1.04,
                   optional_attack_potion_average_share=.04/1.04,
                   optional_attack_potion_gold_per_day=3600)
        rows.append(row)
        days=row["day"];wins+=new_wins;attempts+=new_attempts
        prev_enh=e;prev_mw=m;prev_rerolls=rerolls
    return rows


def verify(rows, config):
    errors=[]
    for row in rows:
        for key in ("attack_share","dps_share","ehp_share","power_share"):
            if abs(sum(row[key].values())-1)>1e-8 or min(row[key].values())<-1e-8:
                errors.append(f"R{row['stage']}: invalid {key}")
        if any(not math.isfinite(row[k]) or row[k]<=0 for k in ("attack_power","boss_dps","hp","mixed_ehp","normal_hp","boss_hp")):
            errors.append(f"R{row['stage']}: nonfinite or nonpositive metric")
        profile=row["rewards"]
        for source,definition in profile["sources"].items():
            probs=definition["rarity_probabilities"]
            if abs(sum(probs)-1)>1e-9 or not all(0<=v<=1 for v in probs):
                errors.append(f"R{row['stage']}: invalid {source} rarity distribution")
        if abs(profile["equipment_count_distribution"]["probability_mass"]-1)>1e-9:
            errors.append(f"R{row['stage']}: invalid equipment count probability mass")
    # These are findings, not silently corrected away by the generator.
    shortages={name:[r["stage"] for r in rows if predicate(r)] for name,predicate in {
        "gold":lambda r:r["gold_reserve"]<0,
        "materials":lambda r:r["materials_reserve"]<0,
        "stones":lambda r:r["stones_required"]>r["stones_earned"],
        "gem_mean_supply":lambda r:r["gem_diamond_tier1_required"]>r["gem_same_color_tier1_equivalent"] or r["gem_ruby_tier1_required"]>r["gem_ruby_tier1_equivalent"],
        "rune_cell_capacity":lambda r:r["rune_active_cell_target"]>r["rune_cell_capacity_upper_bound"],
        "forge_time":lambda r:r["proposed_forge_days"]>max(0,r["day"]-interpolate(config,5)["day"])
    }.items()}
    low_shortages={key:[r["stage"] for r in rows if r["rewards"]["ledger"]["low_optional_cumulative"][key]+1e-7<r[required]]
                   for key,required in [("gold","gold_required"),("materials","materials_required"),("stones","stones_required")]}
    return {"rows":len(rows),"errors":errors,"resource_or_timer_shortages":shortages,
            "no_chests_or_goblins_shortages":low_shortages,
            "attribution":"Exact Shapley over 512 coalitions per stage for four model metrics; growth includes baseline",
            "power_share_note":"power_share attributes sqrt(DPS * mixed EHP), a design-only index, using exact Shapley values.",
            "not_verified":["Real combat clear rates","Natural save progression","Mobile performance","Optimal rune placement and skill rotations","User behavior and retention"]}


def tables(rows, english=False):
    levels=[1,10,30,50,100,200,300,500,750,1000]
    header=("|Rift|Day|Attack|Boss DPS|HP|Mixed EHP|Normal HP|Boss HP|Boss attack|Natural legendaries P10/P50/P90|" if english else
            "|균열|누적 일수|공격력|표준 보스 DPS|생명력|혼합 EHP|일반 적 HP|보스 HP|보스 공격|자연 전설 P10/P50/P90|")
    title="Rift 1–1000 milestone calculations" if english else "균열 1~1000 주요 구간 계산표"
    note="Planning proposal, not applied to the game. Combat DPS and dates are model targets, not measured outcomes. Natural legendary counts exclude all first-clear guarantees." if english else "게임 미반영 기획안입니다. DPS와 날짜는 계산 모델의 목표이며 실측 결과가 아닙니다. 자연 전설 개수에는 최초 클리어 확정 보상을 포함하지 않습니다."
    lines=["# "+title,"","Updated: 2026-09-24" if english else "갱신일: 2026-09-24","",note,"",header,"|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---|"]
    for s in levels:
        r=rows[s-1];p=r["natural_legendary_coverage"]
        lines.append("|"+"|".join([str(s),f"{r['day']:.2f}"]+[f"{r[k]:,.0f}" for k in ("attack_power","boss_dps","hp","mixed_ehp","normal_hp","boss_hp","boss_attack")]+[f"{p['p10']}/{p['p50']}/{p['p90']}"])+"|")
    lines += ["","|"+("Rift" if english else "균열")+"|"+"|".join(["Baseline" if english else "기초"]+(NAMES_EN if english else NAMES_KO))+"|","|---|"+"---:|"*10]
    for s in levels:
        r=rows[s-1]
        lines.append("|"+str(s)+"|"+"|".join(f"{r['dps_share'][k]*100:.1f}%" for k in ["baseline"]+GROUPS)+"|")
    return "\n".join(lines)+"\n"


def main():
    parser=argparse.ArgumentParser();parser.add_argument("--check",action="store_true");args=parser.parse_args()
    config=read(OUT/"parameters.json")
    rows=generate(config);validation=verify(rows,config)
    metric_keys=["attack_power","boss_dps","mixed_ehp","hp","crit","attack_speed","damage_reduction_enemy","armor","resistance","power_index","attack_share","dps_share","ehp_share","power_share"]
    cached=[{k:r[k] for k in metric_keys} for r in rows]
    scenarios=[]
    for horizon in (90,180,365):
        alternative={**config,"days_to_1000":horizon}
        alternative_rows=rows if horizon==config["days_to_1000"] else generate(alternative,cached)
        last=alternative_rows[-1]
        scenarios.append({"days":horizon,"rift_hours":horizon*2,"successful_runs":last["cumulative_successes"],
                          "gold_earned":last["gold_earned"],"gold_required":last["gold_required"],
                          "materials_earned":last["materials_earned"],"materials_required":last["materials_required"],
                          "stones_earned":last["stones_earned"],"stones_required":last["stones_required"],
                          "mastery_level":last["proposed_mastery_level"],"forge_days":last["proposed_forge_days"],
                          "validation":verify(alternative_rows,alternative)})
    reward_config=read(OUT/"reward_parameters.json")
    data={"reward_assumptions":reward_config,"status":config["status"],"assumptions":config,"source_hashes":source_manifest(),"scenarios":scenarios,
          "rune_node_sum_upper_bounds":rune_bounds(),"rows":rows,"validation":validation}
    outputs={"projection.json":json.dumps(data,ensure_ascii=False,indent=None,separators=(",",":"))+"\n",
             "Balance_1000_Tables.md":tables(rows),"Balance_1000_Tables.en.md":tables(rows,True),
             "validation.json":json.dumps(validation,ensure_ascii=False,indent=2)+"\n",
             "Rift_Reward_Tables.md":rewards.tables(rows),"Rift_Reward_Tables.en.md":rewards.tables(rows,True)}
    dashboard_data={"rows":rows,"groups":GROUPS,"names_ko":NAMES_KO,"names_en":NAMES_EN,
                    "scenarios":scenarios,"unlocks":config["unlocks"],"reward_assumptions":reward_config}
    outputs["dashboard.html"]=(ROOT/"tools/templates/balance_dashboard.html").read_text().replace("__DATA__",json.dumps(dashboard_data,ensure_ascii=False,separators=(",",":")).replace("</","<\\/"))
    if args.check:
        for name,content in outputs.items():
            if (OUT/name).read_text()!=content:raise SystemExit(f"Stale generated file: {name}")
    else:
        for name,content in outputs.items():(OUT/name).write_text(content)
    if validation["errors"]:raise SystemExit(validation["errors"])
    print(json.dumps({"rows":len(rows),"errors":len(validation["errors"]),"shortages":{k:len(v) for k,v in validation["resource_or_timer_shortages"].items()},
                      "r1000":{k:rows[-1][k] for k in ("day","attack_power","boss_dps","hp","normal_hp","boss_hp","gold_earned","gold_required","materials_earned","materials_required","stones_earned","stones_required","proposed_forge_days","proposed_mastery_level")}},ensure_ascii=False,indent=2))


if __name__=="__main__":main()
