using System;
using System.Collections.Generic;
using System.Linq;

namespace Hellscript
{
    // Versioned, intrinsic equipment rating, not a prediction of a build's DPS.
    // All values are current values from the existing item/quality owners.
    public static class EquipmentScore
    {
        public const int Version=2;
        static readonly double[] MainReference={20,20,40,2,3,160,2,1};
        static double Extras(Item item)
        {
            double value=25*ItemCatalog.Base(item).resistance*(1+.08*(item.level-1))/35;
            foreach(var bonus in ItemCatalog.Base(item).bonuses)
            {
                var reference=ItemCatalog.Affixes.FirstOrDefault(a=>a.stat==bonus.stat);
                if(reference!=null&&reference.max>0)value+=25*bonus.Value(item)/reference.max;
            }
            foreach(var roll in item.rolls)
            {
                var affix=ItemCatalog.Affix(roll.affixId);
                if(affix!=null&&affix.max>0)value+=25*ItemQuality.AffixValue(item,roll)/affix.max;
            }
            return value;
        }
        static double Raw(Item item)
        {
            if(item==null)return 0;
            double speed=item.slot==0&&!EquipmentSlots.Offhand(item)?1+ItemCatalog.Base(item).attackSpeed:1;
            return 100*ItemCatalog.MainValue(item)*speed/MainReference[item.slot]+Extras(item);
        }
        static double Rounded(double value)=>Math.Round(value,1,MidpointRounding.AwayFromZero);
        public static double Value(Item item)=>Rounded(Raw(item));
        // Dual-wield weapon damage is averaged in HeroStats; it must not be counted twice.
        public static double Weapons(IEnumerable<Item> source)
        {
            var items=source.Where(i=>i.slot==0).Distinct().ToArray();
            var attacks=items.Where(i=>!EquipmentSlots.Offhand(i)).ToArray();
            double damage=attacks.Length==0?0:attacks.Average(i=>(double)ItemCatalog.MainValue(i));
            double speed=attacks.Length==0?1:1+attacks.Average(i=>(double)ItemCatalog.Base(i).attackSpeed);
            damage+=items.Where(i=>EquipmentSlots.Offhand(i)&&EquipmentSlots.Kind(i)!=WeaponKind.Shield).Sum(i=>(double)ItemCatalog.MainValue(i));
            double shields=items.Where(i=>EquipmentSlots.Kind(i)==WeaponKind.Shield).Sum(i=>100*ItemCatalog.MainValue(i)/24d);
            return Rounded(100*damage*speed/20+shields+items.Sum(Extras));
        }
        public static (double before,double after) Replacement(HeroSave hero,Item candidate,EquipmentPlan plan)
        {
            if(candidate.slot!=0)return (Value(EquipmentSlots.At(hero,candidate.slot,plan.index)),Value(candidate));
            var old=hero.inventory.Where(i=>i.equipped&&i.slot==0).ToArray();
            return (Weapons(old),Weapons(old.Where(i=>!plan.outgoing.Contains(i.id)).Concat(new[]{candidate})));
        }
    }
}
