using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    // The five pages Diablo IV's character sheet groups its stats under. A stat is listed on the
    // page it appears on there, so a player who knows that sheet finds the same value in the same
    // place here.
    public enum StatPage { Core, Offense, Defense, Resource, Utility }

    // How a stored bonus reads and how it combines. Flat adds an absolute amount; Percent stores
    // percentage points and is divided by a hundred wherever it is applied; Meters and PerSecond
    // are flat values that carry a unit on screen.
    public enum StatForm { Flat, Percent, Meters, PerSecond }

    // Every stat an item can carry. The first twenty-four numbers are the affix indices already
    // written into existing save files, so they are fixed and must never be renumbered. The stats
    // Diablo IV has that this project did not are appended from twenty-four on.
    public enum StatId
    {
        MaximumLife = 0,
        MaximumLifePercent = 1,
        Armor = 2,
        AllResistance = 3,
        PhysicalDamage = 4,
        FireDamage = 5,
        ColdDamage = 6,
        LightningDamage = 7,
        PoisonDamage = 8,
        ShadowDamage = 9,
        Strength = 10,
        Dexterity = 11,
        Intelligence = 12,
        Willpower = 13,
        AllStats = 14,
        CriticalStrikeChance = 15,
        CriticalStrikeDamage = 16,
        AttackSpeed = 17,
        CooldownReduction = 18,
        ResourceCostReduction = 19,
        ResourceGeneration = 20,
        MovementSpeed = 21,
        PickupRadius = 22,
        HealingReceived = 23,
        VulnerableDamage = 24,
        OverpowerDamage = 25,
        LuckyHitChance = 26,
        DodgeChance = 27,
        BlockChance = 28,
        BlockedDamageReduction = 29,
        Thorns = 30,
        LifeSteal = 31,
        DamageReduction = 32,
        DamageReductionFromClose = 33,
        DamageReductionFromDistant = 34,
        DamageReductionWhileInjured = 35,
        FireResistance = 36,
        ColdResistance = 37,
        LightningResistance = 38,
        PoisonResistance = 39,
        ShadowResistance = 40,
        PhysicalDamageReduction = 41,
        MaximumResource = 42,
        LifeRegeneration = 43,
        PotionHealing = 44,
        BarrierGeneration = 45,
        CrowdControlDuration = 46,
        ExperienceGain = 47,
        GoldFind = 48,
        CloseDamage = 49,
        DistantDamage = 50,
        DamageOverTime = 51,
        DamageToCrowdControlled = 52,
        DamageToInjured = 53,
        DamageToHealthy = 54,
        MaximumStamina = 55,
        StaminaRegeneration = 56,
        AllResistancePercent = 57,
    }

    // The conditions Diablo IV lets a damage bonus depend on. The order is the index of the
    // conditional array on the character sheet.
    public enum DamageCondition { Close = 0, Distant = 1, OverTime = 2, CrowdControlled = 3, Injured = 4, Healthy = 5 }

    // Damage elements in the order the simulation has always numbered them. Physical is mitigated
    // by armour and the other five by their own resistance, exactly as in Diablo IV.
    public static class Element
    {
        public const int Physical = 0, Fire = 1, Cold = 2, Lightning = 3, Poison = 4, Shadow = 5;
        public const int Count = 6;
        // The resistance stat that guards this element. Physical answers with armour instead.
        public static StatId Resistance(int element)
        {
            switch (element)
            {
                case Fire: return StatId.FireResistance;
                case Cold: return StatId.ColdResistance;
                case Lightning: return StatId.LightningResistance;
                case Poison: return StatId.PoisonResistance;
                case Shadow: return StatId.ShadowResistance;
                default: return StatId.Armor;
            }
        }
        // The damage stat this element scales with, in the same order bonuses 4 to 9 have always used.
        public static StatId Damage(int element) => (StatId)((int)StatId.PhysicalDamage + Mathf.Clamp(element, 0, Count - 1));
    }

    public sealed class StatDefinition
    {
        public readonly StatId id;
        // Written in the source language like every other literal, and read through Loc when shown.
        public readonly string name;
        public readonly StatPage page;
        public readonly StatForm form;
        // The ceiling the derived value is clamped to, in the stat's own unit. Zero means the stat
        // has no ceiling of its own; a few are instead bounded by the curve that consumes them.
        public readonly float cap;
        public StatDefinition(StatId id, string name, StatPage page, StatForm form, float cap = 0)
        { this.id = id; this.name = name; this.page = page; this.form = form; this.cap = cap; }
        public int Index => (int)id;
        public string Name => Loc.T(name);
        public string Format(float value)
        {
            switch (form)
            {
                case StatForm.Percent: return Loc.F("{0:0.#}%", value);
                case StatForm.Meters: return Loc.F("{0:0.0}m", value);
                case StatForm.PerSecond: return Loc.F("초당 {0:0.#}", value);
                default: return Loc.F("{0:0.#}", value);
            }
        }
        // Gear stores a percentage movement bonus; the sheet reports actual metres per second.
        public string FormatSheet(float value)
            =>id==StatId.MovementSpeed?Loc.F("{0:0.0}m/s",value):Format(value);
        public string Signed(float value)
        {
            string body = Format(Mathf.Abs(value));
            return value < 0 ? "-" + body : "+" + body;
        }
    }

    // The catalogue of stats the game knows. Diablo IV's sheet is the source of the list, the
    // grouping and the units; the numbers each stat produces are this project's own and live in
    // HeroStats, not here.
    public static class StatCatalog
    {
        const StatPage Core = StatPage.Core, Off = StatPage.Offense, Def = StatPage.Defense, Res = StatPage.Resource, Util = StatPage.Utility;
        const StatForm Flat = StatForm.Flat, Pct = StatForm.Percent, Met = StatForm.Meters, Sec = StatForm.PerSecond;
        static readonly StatDefinition[] all =
        {
            new StatDefinition(StatId.Strength, "힘", Core, Flat),
            new StatDefinition(StatId.Dexterity, "민첩성", Core, Flat),
            new StatDefinition(StatId.Intelligence, "지능", Core, Flat),
            new StatDefinition(StatId.Willpower, "의지력", Core, Flat),
            new StatDefinition(StatId.AllStats, "모든 능력치", Core, Flat),

            new StatDefinition(StatId.CriticalStrikeChance, "치명타 확률", Off, Pct, 75),
            new StatDefinition(StatId.CriticalStrikeDamage, "치명타 피해", Off, Pct),
            new StatDefinition(StatId.VulnerableDamage, "취약 피해", Off, Pct),
            new StatDefinition(StatId.OverpowerDamage, "압도 피해", Off, Pct),
            new StatDefinition(StatId.AttackSpeed, "공격 속도", Off, Pct, 50),
            new StatDefinition(StatId.LuckyHitChance, "행운의 적중 확률", Off, Pct, 100),
            new StatDefinition(StatId.PhysicalDamage, "물리 피해", Off, Pct),
            new StatDefinition(StatId.FireDamage, "화염 피해", Off, Pct),
            new StatDefinition(StatId.ColdDamage, "냉기 피해", Off, Pct),
            new StatDefinition(StatId.LightningDamage, "번개 피해", Off, Pct),
            new StatDefinition(StatId.PoisonDamage, "독 피해", Off, Pct),
            new StatDefinition(StatId.ShadowDamage, "암흑 피해", Off, Pct),
            new StatDefinition(StatId.CloseDamage, "근접 피해", Off, Pct),
            new StatDefinition(StatId.DistantDamage, "원거리 피해", Off, Pct),
            new StatDefinition(StatId.DamageOverTime, "지속 피해", Off, Pct),
            new StatDefinition(StatId.DamageToCrowdControlled, "군중 제어된 적에게 주는 피해", Off, Pct),
            new StatDefinition(StatId.DamageToInjured, "부상당한 적에게 주는 피해", Off, Pct),
            new StatDefinition(StatId.DamageToHealthy, "생명력이 높은 적에게 주는 피해", Off, Pct),
            new StatDefinition(StatId.LifeSteal, "생명력 훔치기", Off, Pct),
            new StatDefinition(StatId.Thorns, "가시", Off, Flat),

            new StatDefinition(StatId.MaximumLife, "최대 생명력", Def, Flat),
            new StatDefinition(StatId.MaximumLifePercent, "최대 생명력 증가", Def, Pct),
            new StatDefinition(StatId.Armor, "방어도", Def, Flat),
            new StatDefinition(StatId.AllResistance, "모든 원소 저항", Def, Flat),
            new StatDefinition(StatId.AllResistancePercent, "모든 원소 저항 증가", Def, Pct, 70),
            new StatDefinition(StatId.FireResistance, "화염 저항", Def, Flat),
            new StatDefinition(StatId.ColdResistance, "냉기 저항", Def, Flat),
            new StatDefinition(StatId.LightningResistance, "번개 저항", Def, Flat),
            new StatDefinition(StatId.PoisonResistance, "독 저항", Def, Flat),
            new StatDefinition(StatId.ShadowResistance, "암흑 저항", Def, Flat),
            new StatDefinition(StatId.PhysicalDamageReduction, "물리 피해 감소", Def, Pct, 85),
            new StatDefinition(StatId.DamageReduction, "피해 감소", Def, Pct, 85),
            new StatDefinition(StatId.DamageReductionFromClose, "근접한 적에게 받는 피해 감소", Def, Pct, 85),
            new StatDefinition(StatId.DamageReductionFromDistant, "먼 적에게 받는 피해 감소", Def, Pct, 85),
            new StatDefinition(StatId.DamageReductionWhileInjured, "부상 시 피해 감소", Def, Pct, 85),
            new StatDefinition(StatId.DodgeChance, "회피 확률", Def, Pct, 60),
            new StatDefinition(StatId.BlockChance, "막기 확률", Def, Pct, 75),
            new StatDefinition(StatId.BlockedDamageReduction, "막은 피해 감소", Def, Pct, 90),
            new StatDefinition(StatId.LifeRegeneration, "생명력 재생", Def, Sec),
            new StatDefinition(StatId.HealingReceived, "받는 치유 효과", Def, Pct),
            new StatDefinition(StatId.PotionHealing, "물약 회복량", Def, Pct),
            new StatDefinition(StatId.BarrierGeneration, "보호막 생성량", Def, Pct),

            new StatDefinition(StatId.MaximumResource, "최대 자원", Res, Flat),
            new StatDefinition(StatId.ResourceGeneration, "자원 생성", Res, Sec),
            new StatDefinition(StatId.ResourceCostReduction, "자원 소모 감소", Res, Pct, 50),
            new StatDefinition(StatId.CooldownReduction, "재사용 대기시간 감소", Res, Pct, 40),

            new StatDefinition(StatId.MovementSpeed, "이동 속도", Util, Pct, 50),
            new StatDefinition(StatId.CrowdControlDuration, "군중 제어 지속시간 감소", Util, Pct, 80),
            new StatDefinition(StatId.PickupRadius, "획득 반경", Util, Met, 6),
            new StatDefinition(StatId.ExperienceGain, "경험치 획득량", Util, Pct),
            new StatDefinition(StatId.GoldFind, "금화 획득량", Util, Pct),
            new StatDefinition(StatId.MaximumStamina, "최대 기력", Util, Flat),
            new StatDefinition(StatId.StaminaRegeneration, "기력 회복", Util, Sec),
        };
        static readonly StatDefinition[] byIndex = BuildIndex();
        public static readonly IReadOnlyList<StatDefinition> All = Array.AsReadOnly(all);
        // The length every bonus array has. Adding a stat lengthens the array; the numbers already
        // in a save keep their meaning because no existing index moves.
        public static int Count => byIndex.Length;
        static StatDefinition[] BuildIndex()
        {
            var names = Enum.GetValues(typeof(StatId)).Cast<StatId>().ToArray();
            if (all.Length != names.Length) throw new InvalidOperationException("스탯 정의와 StatId 개수가 다릅니다.");
            var slots = new StatDefinition[names.Max(x => (int)x) + 1];
            foreach (var definition in all)
            {
                if (slots[definition.Index] != null) throw new InvalidOperationException("스탯 번호가 겹칩니다: " + definition.id);
                slots[definition.Index] = definition;
            }
            for (int i = 0; i < slots.Length; i++) if (slots[i] == null) throw new InvalidOperationException("스탯 번호에 빈 자리가 있습니다: " + i);
            return slots;
        }
        public static bool Known(int index) => index >= 0 && index < byIndex.Length;
        public static StatDefinition Get(StatId id) => Get((int)id);
        public static StatDefinition Get(int index)
            => Known(index) ? byIndex[index] : throw new ArgumentOutOfRangeException(nameof(index), Loc.Source("알 수 없는 스탯 번호: {0}", index));
        public static string Name(StatId id) => Get(id).Name;
        public static string Name(int index) => Get(index).Name;
        public static IEnumerable<StatDefinition> Page(StatPage page) => all.Where(x => x.page == page);
        // A ceiling of zero means the stat is bounded by whatever consumes it rather than here.
        public static float Cap(StatId id, float value)
        {
            float cap = Get(id).cap;
            return cap > 0 ? Mathf.Min(cap, value) : value;
        }
        // Diablo IV stacks separate sources of damage reduction multiplicatively: two thirty
        // percent sources leave 0.7 * 0.7 of the damage, not forty percent of it.
        public static float Combine(params float[] reductions)
        {
            float remaining = 1;
            foreach (float reduction in reductions) remaining *= 1 - Mathf.Clamp(reduction, 0, 1);
            return 1 - remaining;
        }
        // Armor and resistance both read off the same curve in Diablo IV: the rating is measured
        // against a value that grows with the attacker's level, so a rating that was strong at a
        // low level is ordinary later.
        public static float Mitigation(float rating, int attackerLevel, float ceiling)
        {
            float value = Mathf.Max(0, rating);
            return Mathf.Min(ceiling, value / (value + 100 + 10 * Mathf.Max(1, attackerLevel)));
        }
    }
}
