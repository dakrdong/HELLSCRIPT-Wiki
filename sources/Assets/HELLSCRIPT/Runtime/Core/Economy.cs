using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    public static class RandomStream
    {
        public static uint Next(ref uint state) { if(state==0)state=0xA341316Cu; state^=state<<13;state^=state>>17;state^=state<<5;return state; }
        public static float Unit(ref uint state) => (Next(ref state)&0xFFFFFF)/16777216f;
        public static int Range(ref uint state,int min,int max) => min+(int)(Unit(ref state)*(max-min));
    }

    // The character sheet. Diablo IV derives every line of that sheet from the four core
    // attributes and whatever the equipment rolled, and this does the same: bonuses[] holds the
    // rolls and each field below is the finished number the simulation reads.
    public sealed class HeroStats
    {
        public float hp, damage, armor, resistance, regen, speed, pickup, crit, critDamage, attackSpeed, cdr, costReduction, healing,shieldMultiplier;
        // The four core attributes, kept as fields because the rest of the sheet is derived from them.
        public float strength, dexterity, intelligence, willpower, attackPower;
        // The remaining Diablo IV lines. Each one is neutral until gear or an attribute raises it.
        public float vulnerable, overpower, luckyHit, dodge, blockChance, blockedReduction, thorns, lifeSteal;
        public float damageReduction, closeReduction, distantReduction, injuredReduction, physicalReduction;
        public float lifeRegen, potionHealing, ccReduction, experienceGain, goldFind;
        public float maxResource, maxStamina, staminaRegen;
        public float gemBuffReduction,gemPeriodicReduction,gemArmorPercent;
        public float[] gemBonuses=new float[StatCatalog.Count];
        public float[] runeBonuses=new float[StatCatalog.Count];
        public float runeAttack;
        public float[] runeSkillPower=new float[18],runeSkillCost=new float[18];
        public int[] runeSkillLevels=new int[18];
        public int SkillLevel(int index,bool learned)=>learned?1+runeSkillLevels[index]:0;
        public float[] bonuses=new float[StatCatalog.Count];
        public HashSet<string> specials=new HashSet<string>();
        readonly Dictionary<string,HashSet<int>> sets=new Dictionary<string,HashSet<int>>();
        public int SetPieces(string id)=>sets.TryGetValue(id,out var slots)?slots.Count:0;
        public bool[] passives=new bool[6];
        float baseSpeed,potionMoveBonus;
        public float Bonus(StatId id)=>bonuses[(int)id];
        public float SpeedWithBonus(float bonus)=>baseSpeed*(1+Mathf.Min(.5f,bonuses[21]/100+bonus+potionMoveBonus));
        public HeroStats WithPotion(PotionDefinition potion,int level)
        {
            var result=(HeroStats)MemberwiseClone();
            switch(potion.id)
            {
                case "PU01":result.potionMoveBonus=potion.magnitude;result.speed=result.SpeedWithBonus(0);break;
                case "PU02":result.damage*=1+potion.magnitude;result.attackPower*=1+potion.magnitude;break;
                case "PU03":result.resistance+=potion.magnitude+level*3;break;
                case "PU04":result.armor*=1+potion.magnitude;break;
                case "PU05":result.attackSpeed=Mathf.Min(1.5f,result.attackSpeed+potion.magnitude);break;
                case "PU06":result.crit=Mathf.Min(.75f,result.crit+potion.magnitude);break;
            }
            return result;
        }
        public bool PotionChanges(HeroStats other)=>speed!=other.speed||damage!=other.damage||resistance!=other.resistance||armor!=other.armor||attackSpeed!=other.attackSpeed||crit!=other.crit;
        public float BuffReduction(float other)=>Mathf.Min(.5f,gemBuffReduction+Mathf.Max(0,other));
        // The rating that mitigates one element: armour for physical, and the element's own
        // resistance beside the all-resistance roll for the other five. Read rather than stored,
        // so that overwriting armour or resistance moves what depends on them.
        public float Resistance(int element)
            =>element==Element.Physical?armor:Mathf.Max(0,resistance+Bonus(Element.Resistance(element)));
        public float Conditional(DamageCondition condition)
            =>Bonus((StatId)((int)StatId.CloseDamage+(int)condition))/100;
        // The finished number for one line of the sheet — what Diablo IV's character screen shows,
        // not the sum of what the gear rolled. A line the game derives answers with the derived
        // value; every other line is the roll itself, which is all there is to show for it.
        public float Sheet(StatId id)
        {
            switch(id)
            {
                case StatId.Strength: return strength;
                case StatId.Dexterity: return dexterity;
                case StatId.Intelligence: return intelligence;
                case StatId.Willpower: return willpower;
                case StatId.CriticalStrikeChance: return crit*100;
                case StatId.CriticalStrikeDamage: return (critDamage-1)*100;
                case StatId.VulnerableDamage: return vulnerable;
                case StatId.OverpowerDamage: return overpower;
                case StatId.AttackSpeed: return (attackSpeed-1)*100;
                case StatId.LuckyHitChance: return luckyHit;
                case StatId.LifeSteal: return lifeSteal*100;
                case StatId.Thorns: return thorns;
                case StatId.MaximumLife: return hp;
                case StatId.Armor: return armor;
                case StatId.AllResistance: return resistance;
                case StatId.FireResistance: return Resistance(Element.Fire);
                case StatId.ColdResistance: return Resistance(Element.Cold);
                case StatId.LightningResistance: return Resistance(Element.Lightning);
                case StatId.PoisonResistance: return Resistance(Element.Poison);
                case StatId.ShadowResistance: return Resistance(Element.Shadow);
                case StatId.PhysicalDamageReduction: return physicalReduction*100;
                case StatId.DamageReduction: return damageReduction*100;
                case StatId.DamageReductionFromClose: return closeReduction*100;
                case StatId.DamageReductionFromDistant: return distantReduction*100;
                case StatId.DamageReductionWhileInjured: return injuredReduction*100;
                case StatId.DodgeChance: return dodge*100;
                case StatId.BlockChance: return blockChance*100;
                case StatId.BlockedDamageReduction: return blockedReduction*100;
                case StatId.LifeRegeneration: return lifeRegen;
                case StatId.HealingReceived: return (healing-1)*100;
                case StatId.PotionHealing: return (potionHealing-1)*100;
                case StatId.BarrierGeneration: return (shieldMultiplier-1)*100;
                case StatId.MaximumResource: return maxResource;
                case StatId.ResourceGeneration: return regen;
                case StatId.ResourceCostReduction: return costReduction*100;
                case StatId.CooldownReduction: return cdr*100;
                case StatId.MovementSpeed: return speed;
                case StatId.CrowdControlDuration: return ccReduction*100;
                case StatId.PickupRadius: return pickup;
                case StatId.ExperienceGain: return (experienceGain-1)*100;
                case StatId.GoldFind: return (goldFind-1)*100;
                case StatId.MaximumStamina: return maxStamina;
                case StatId.StaminaRegeneration: return staminaRegen;
                default: return Bonus(id);
            }
        }
        public HeroStats(HeroSave hero, bool training=false,RuneGrowthState runes=null)
        {
            int level=training?30:hero.level; int c=(int)hero.heroClass;
            foreach(int p in hero.build.passives) if(p>=0&&p<6)passives[p]=true;
            var equipped=hero.inventory.Where(x=>x.equipped).ToList();
            foreach(var item in equipped)
            {
                for(int i=0;i<bonuses.Length;i++)bonuses[i]+=item.Value(i);
                if(GemCatalog.TryEffect(item,out var gem,out float value))
                {
                    switch(gem.kind)
                    {
                        case GemEffectKind.Stat:bonuses[(int)gem.stat]+=value;gemBonuses[(int)gem.stat]+=value;break;
                        case GemEffectKind.BuffReduction:gemBuffReduction+=value/100;break;
                        case GemEffectKind.PeriodicReduction:gemPeriodicReduction+=value/100;break;
                        case GemEffectKind.ArmorPercent:gemArmorPercent+=value/100;break;
                    }
                }
                if(!string.IsNullOrEmpty(item.special))specials.Add(item.special);
                var unique=ItemCatalog.Unique(item.special);
                if(unique!=null&&!string.IsNullOrEmpty(unique.setId))
                {if(!sets.TryGetValue(unique.setId,out var slots))sets[unique.setId]=slots=new HashSet<int>();slots.Add(item.slot);}
            }
            foreach(var effect in RuneGrowth.Contributions(runes,Hellscript.Runes.RuneMasteryCatalog.EquippedWeapon(hero)))
            {if(effect.Meaning=="AttackPower")runeAttack+=effect.Value;else if(Hellscript.Runes.RuneMasteryCatalog.IsSkill(effect.Meaning)){int skill=Hellscript.Runes.RuneMasteryCatalog.SkillIndex(effect.Meaning);if(effect.Meaning.StartsWith("SkillLevel:"))runeSkillLevels[skill]=Mathf.Min(5,runeSkillLevels[skill]+(int)effect.Value);else if(effect.Meaning.StartsWith("SkillCost:"))runeSkillCost[skill]=Mathf.Min(30,runeSkillCost[skill]+effect.Value);else runeSkillPower[skill]=Mathf.Min(60,runeSkillPower[skill]+effect.Value);}else {int stat=(int)Enum.Parse<StatId>(effect.Meaning);bonuses[stat]+=effect.Value;runeBonuses[stat]+=effect.Value;}}
            float primary=30+2*(level-1)+bonuses[10+c]+bonuses[14];
            float str=(c==0?30+2*(level-1):10+level-1)+bonuses[10]+bonuses[14];
            float dex=(c==1?30+2*(level-1):10+level-1)+bonuses[11]+bonuses[14];
            float intel=(c==2?30+2*(level-1):10+level-1)+bonuses[12]+bonuses[14];
            float will=10+level-1+bonuses[13]+bonuses[14];
            strength=str;dexterity=dex;intelligence=intel;willpower=will;
            float flatHp=bonuses[0]; armor=str*2+bonuses[2];resistance=intel+bonuses[3];
            float weapon=20, weaponSpeed=0;
            foreach(var item in equipped)
            {
                var basis=ItemCatalog.Base(item);float upgraded=ItemCatalog.MainValue(item);
                if(item.slot==0){weapon=upgraded;weaponSpeed=basis.attackSpeed;}
                else if(item.slot<=5)armor+=upgraded;
                else if(item.slot==6)flatHp+=upgraded;
                else resistance+=upgraded;
                resistance+=basis.resistance*(1+.08f*(item.level-1));
            }
            armor*=1+gemArmorPercent;
            hp=(new[]{300,240,210}[c]+new[]{35,28,25}[c]*(level-1)+flatHp)*(1+bonuses[1]/100);
            damage=(weapon+runeAttack)*(1+.002f*primary);attackPower=damage;
            regen=new[]{8,10,12}[c]+bonuses[20]; if(c==2&&passives[4])regen*=1.2f;
            baseSpeed=new[]{4f,4.4f,4f}[c];speed=SpeedWithBonus(0);
            pickup=Mathf.Min(6,1.5f+bonuses[22]+(specials.Contains("LC01")?2.5f:0));
            crit=Mathf.Min(.75f,.05f+dex*.0003f+bonuses[15]/100);critDamage=1.5f+bonuses[16]/100;
            attackSpeed=1+Mathf.Clamp(weaponSpeed+bonuses[17]/100,-.5f,.5f);
            cdr=Mathf.Min(.4f,bonuses[18]/100); costReduction=Mathf.Min(.5f,bonuses[19]/100);
            healing=1+bonuses[23]/100+will*.001f;
            // Diablo IV calls this barrier generation. Willpower, the two passives and the gear
            // roll all raise the same number, so they are summed here rather than kept apart.
            shieldMultiplier=1+will*.001f+(c==0&&passives[4]||c==2&&passives[3]?.2f:0)+bonuses[(int)StatId.BarrierGeneration]/100;
            DeriveExtendedStats(will,dex);
        }
        // Diablo IV's own derivations: strength feeds armour, dexterity feeds dodge, intelligence
        // feeds resistance and willpower feeds healing, overpower damage and resource generation.
        // The four lines above this call already spend strength, intelligence and part of
        // willpower, so only the parts this project did not have are added here.
        void DeriveExtendedStats(float will,float dex)
        {
            // Twenty-five is what a marked target already added here before the sheet existed;
            // Diablo IV's own baseline for a vulnerable target sits beside it in the gear roll.
            vulnerable=25+Bonus(StatId.VulnerableDamage);
            overpower=50+will*.25f+Bonus(StatId.OverpowerDamage);
            luckyHit=StatCatalog.Cap(StatId.LuckyHitChance,Bonus(StatId.LuckyHitChance));
            dodge=StatCatalog.Cap(StatId.DodgeChance,dex*.025f+Bonus(StatId.DodgeChance))/100;
            blockChance=StatCatalog.Cap(StatId.BlockChance,Bonus(StatId.BlockChance))/100;
            blockedReduction=StatCatalog.Cap(StatId.BlockedDamageReduction,Bonus(StatId.BlockedDamageReduction))/100;
            thorns=Mathf.Max(0,Bonus(StatId.Thorns));
            lifeSteal=Mathf.Max(0,Bonus(StatId.LifeSteal))/100;
            damageReduction=StatCatalog.Cap(StatId.DamageReduction,Bonus(StatId.DamageReduction))/100;
            closeReduction=StatCatalog.Cap(StatId.DamageReductionFromClose,Bonus(StatId.DamageReductionFromClose))/100;
            distantReduction=StatCatalog.Cap(StatId.DamageReductionFromDistant,Bonus(StatId.DamageReductionFromDistant))/100;
            injuredReduction=StatCatalog.Cap(StatId.DamageReductionWhileInjured,Bonus(StatId.DamageReductionWhileInjured))/100;
            physicalReduction=StatCatalog.Cap(StatId.PhysicalDamageReduction,Bonus(StatId.PhysicalDamageReduction))/100;
            // Only what the gear rolled. Diablo IV's baseline regeneration would heal the hero
            // through a fight this project deliberately settles with potions and passives alone.
            lifeRegen=Mathf.Max(0,Bonus(StatId.LifeRegeneration));
            potionHealing=1+Bonus(StatId.PotionHealing)/100;
            ccReduction=StatCatalog.Cap(StatId.CrowdControlDuration,Bonus(StatId.CrowdControlDuration))/100;
            experienceGain=1+Bonus(StatId.ExperienceGain)/100;
            goldFind=1+Bonus(StatId.GoldFind)/100;
            maxResource=100+Bonus(StatId.MaximumResource);
            maxStamina=100+Bonus(StatId.MaximumStamina);
            staminaRegen=10+Bonus(StatId.StaminaRegeneration);
        }
    }

    public static class Economy
    {
        public static readonly string[] LegendIds={"LW01","LW02","LW03","LW04","LA01","LA02","LA03","LA04","LM01","LM02","LM03","LM04","LC01","LC02","LC03"};
        public static readonly string[] LegendNames={"소용돌이의 송곳니","낙성의 발걸음","고독한 처형","마지막 명령","끝없는 궤적","좁혀진 사선","독사의 탈피","검은 전염","겨울의 발자취","되울림의 잿불","환류의 매듭","종말의 회로","파수꾼의 고리","절제의 서약","꺼지지 않는 심장"};
        public static int FreeSlots(HeroSave hero) => hero.capacity-hero.inventory.Count(x=>!x.equipped);
        public static int XpRequired(int level) => 100+60*(level-1)+15*(level-1)*(level-1);
        public static void AddXp(HeroSave hero,int xp)
        {
            if(hero.level>=30)return;hero.xp+=xp;
            while(hero.level<30&&hero.xp>=XpRequired(hero.level)){hero.xp-=XpRequired(hero.level);hero.level++;}
            if(hero.level==30)hero.xp=0;
        }
        public static bool AllowedAffix(int a,int s)=>ItemCatalog.Affixes.Any(d=>d.stat==a&&d.Allows(s));
        public static Item CreateItem(HeroClass c,int slot,int rarity,int level,ref uint rng,string id=null)
            =>ItemGenerator.Create(c,slot,rarity,level,ref rng,id);
        public static Item CreateRiftItem(HeroClass c,int slot,int rarity,int level,int stage,ref uint rng,string id=null)
            =>ItemGenerator.Create(c,slot,rarity,level,ref rng,id,riftStage:stage);
        public static bool Referenced(HeroSave hero,Item item)=>
            (hero.build.equipmentIds?.Contains(item.id)??false)||hero.presets.Any(p=>p?.equipmentIds?.Contains(item.id)??false);
        public static bool Protected(HeroSave hero,Item item)=>item.locked||item.equipped||Referenced(hero,item)||GemCatalog.HasGem(item);
        static bool Owned(AccountSave a,Item item)=>item!=null&&a.heroes.Any(h=>h.inventory.Contains(item));
        static bool TownService(AccountSave a)=>a.suspendedRun==null||a.suspendedRun.phase==RunPhase.Cleared||a.suspendedRun.phase==RunPhase.Failed;
        public static string EquipError(HeroSave hero,Item item)
        {
            if(item==null||!hero.inventory.Contains(item))return "이 캐릭터가 보유한 장비만 장착할 수 있습니다.";
            if(item.equipped)return "이미 장착한 장비입니다.";
            if(hero.level<item.RequiredLevel)return Loc.F("캐릭터 레벨 {0}이 필요합니다.", item.RequiredLevel);
            if(!ItemCatalog.Base(item).Fits(hero.heroClass,item.slot)||ItemCatalog.Unique(item.special) is UniqueItemDefinition u&&!u.Fits(hero.heroClass,item.slot))return "다른 직업의 전용 장비입니다.";
            return "";
        }
        public static bool AddItem(HeroSave hero,Item item,BagPolicy policy,AccountSave account=null)
        {
            if(item==null||hero.inventory.Any(i=>i.id==item.id))return false;
            if(FreeSlots(hero)>0){ItemAcquisition.Stamp(account,item);hero.inventory.Add(item);return true;}
            if(policy!=BagPolicy.Replace)return false;
            var worst=hero.inventory.Where(x=>!Protected(hero,x)&&!(account?.heroes.Any(h=>Referenced(h,x))??false)&&x.rarity<3).OrderBy(x=>x.rarity).ThenBy(x=>x.level).ThenBy(x=>x.Price).FirstOrDefault();
            if(worst==null||Compare(item,worst)<=0)return false;
            ItemAcquisition.Stamp(account,item);hero.inventory.Remove(worst);hero.inventory.Add(item);return true;
        }
        static int Compare(Item a,Item b) {int c=a.rarity.CompareTo(b.rarity);if(c==0)c=a.level.CompareTo(b.level);return c==0?a.Price.CompareTo(b.Price):c;}
        public static bool Equip(HeroSave hero,Item item)
        {
            if(!string.IsNullOrEmpty(EquipError(hero,item)))return false;
            foreach(var i in hero.inventory)if(i.slot==item.slot)i.equipped=false;
            item.equipped=true;return true;
        }
        public static bool Dismantle(AccountSave account,HeroSave hero,Item item)
        {
            if(!aOwns(account,hero,item)||(Protected(hero,item)||account.heroes.Any(h=>Referenced(h,item))))return false;
            if(item.rarity==3)account.cores[item.slot]++;else account.materials+=new[]{1,2,5}[item.rarity];
            int invested=item.contentVersion>0?item.investedMaterials:20*((1<<item.enhancement)-1);account.materials+=(int)(invested*4L/5);hero.inventory.Remove(item);ContentUnlocks.Reconcile(account);return true;
        }
        static bool aOwns(AccountSave a,HeroSave h,Item i)=>i!=null&&a.heroes.Contains(h)&&h.inventory.Contains(i);
        public static bool Sell(AccountSave a,HeroSave h,Item item)
        {
            if(!aOwns(a,h,item)||(Protected(h,item)||a.heroes.Any(owner=>Referenced(owner,item))))return false;
            if((long)a.gold+item.Price>int.MaxValue)return false;
            a.gold+=item.Price;h.inventory.Remove(item);return true;
        }
        public static int EnhancementMaterials(Item item)=>item.enhancement>=5?0:20*(1<<item.enhancement);
        public static long EnhancementGold(Item item)=>item.enhancement>=5?0:200L*(item.enhancement+1)*item.level;
        public static long RerollGold(Item item)
        {
            // Test coefficients: 500*L + 50*L*(L-1), independent of previous rerolls.
            long level=Math.Max(1,item.level),factor=level*(level+9);
            return factor>long.MaxValue/50?long.MaxValue:50*factor;
        }
        public static bool Enhance(AccountSave a,Item item)
        {
            if(!ContentUnlocks.Has(a,ContentUnlocks.Enhance)||!Owned(a,item)||!TownService(a)||item.enhancement>=5)return false;
            int mats=EnhancementMaterials(item);long gold=EnhancementGold(item);
            if(a.materials<mats||a.gold<gold)return false;
            a.materials-=mats;a.gold-=(int)gold;item.enhancement++;item.investedMaterials+=mats;return true;
        }
        public static bool Reroll(AccountSave a,Item item,int index,ref uint rng)
            =>item!=null&&index>=0&&index<item.rolls.Count&&Reroll(a,item,item.rolls[index].slotId,ref rng);
        public static bool Reroll(AccountSave a,Item item,string slotId,ref uint rng)
        {
            if(!ContentUnlocks.Has(a,ContentUnlocks.Reroll)||!Owned(a,item)||!TownService(a)||!string.IsNullOrEmpty(item.rerollSlotId)&&item.rerollSlotId!=slotId)return false;
            int index=item.rolls.FindIndex(r=>r.slotId==slotId);if(index<0||ItemGenerator.RerollPool(item,slotId).Count==0)return false;
            long cost=RerollGold(item);if(a.gold<cost)return false;
            uint next=rng;var roll=ItemGenerator.Reroll(item,slotId,ref next);
            a.gold-=(int)cost;item.rerollSlotId=slotId;item.rerolls++;item.rolls[index]=roll;item.name=item.DisplayName;rng=next;return true;
        }
        public static bool Sweep(AccountSave account,string requestId,ref uint rng)
        {
            if(!ContentUnlocks.Has(account,ContentUnlocks.Sweep)||!TownService(account)||string.IsNullOrWhiteSpace(requestId)||account.receipts.Contains(requestId))return false;
            string day=DateTime.UtcNow.ToString("yyyy-MM-dd");int used=account.sweepDay==day?account.sweepCount:0;
            var h=account.Hero;if(h.highestClear<1||used>=3||FreeSlots(h)<3)return false;
            GemInventory.Normalize(account);
            var gems=account.gems.Select(g=>new GemStack{gemId=g.gemId,tier=g.tier,count=g.count}).ToList();uint gemRandom=rng^0xA63149C7u;
            for(int i=0;i<GemCatalog.DropCount(RiftRewardSource.Boss);i++)if(!GemStacks.TryAdd(gems,account.gemCapacity,GemCatalog.Roll(h.highestClear,ref gemRandom)))return false;
            for(int i=0;i<3;i++)AddItem(h,CreateRiftItem(h.heroClass,RandomStream.Range(ref rng,0,8),RiftRarity.Roll(RiftRewardSource.Boss,h.highestClear,ref rng),RandomStream.Range(ref rng,Mathf.Max(1,h.highestClear-2),h.highestClear+3),h.highestClear,ref rng),BagPolicy.Ignore,account);
            account.gems=gems;ContentUnlocks.RecordGemAcquisition(account);
            account.gold+=800+50*h.highestClear;account.materials+=5+h.highestClear/5;account.sweepDay=day;account.sweepCount=used+1;account.receipts.Add(requestId);return true;
        }
    }
}
