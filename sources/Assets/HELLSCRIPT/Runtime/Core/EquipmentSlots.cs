using System;
using System.Collections.Generic;
using System.Linq;

namespace Hellscript
{
    public enum WeaponKind { None, Melee, Greatsword, Blade, Bow, Crossbow, Wand, Staff, Shield, Orb, Scroll, Arrows }

    // A two-handed item is one owned instance. equipIndex only selects a hand or ring position.
    public static class EquipmentSlots
    {
        public const int Count=10;
        public static int Slot(int position)=>position<8?position:position==8?0:7;
        public static int Index(int position)=>position>=8?1:0;
        public static int Position(Item item)=>item.equipIndex==1?(item.slot==0?8:9):item.slot;
        public static string Label(int slot,int index)=>slot==0?(index==0?"주무기":"보조무기"):slot==7?(index==0?"왼쪽 반지":"오른쪽 반지"):GameCatalog.Slots[slot];
        public static WeaponKind Kind(Item item)=>item==null?WeaponKind.None:Kind(ItemCatalog.Base(item).id);
        public static WeaponKind Kind(string id)
        {
            switch(id)
            {
                case "B01":case "B03":return WeaponKind.Melee;
                case "B02":return WeaponKind.Greatsword;
                case "B04":case "B05":return WeaponKind.Bow;
                case "B06":return WeaponKind.Crossbow;
                case "B07":case "B08":case "B09":return WeaponKind.Staff;
                case "B25":return WeaponKind.Wand;case "B26":return WeaponKind.Orb;
                case "B27":return WeaponKind.Scroll;case "B28":return WeaponKind.Shield;
                case "B29":return WeaponKind.Arrows;case "B30":return WeaponKind.Blade;
                default:return WeaponKind.None;
            }
        }
        public static bool TwoHanded(Item item)=>Kind(item)==WeaponKind.Greatsword||Kind(item)==WeaponKind.Staff||Kind(item)==WeaponKind.Crossbow;
        public static bool Offhand(Item item)=>IsOffhand(Kind(item));
        public static bool IsOffhand(WeaponKind kind)=>kind==WeaponKind.Shield||kind==WeaponKind.Orb||kind==WeaponKind.Scroll||kind==WeaponKind.Arrows;
        public static bool Occupies(Item item,int slot,int index)=>item.equipped&&item.slot==slot&&(item.equipIndex==index||slot==0&&TwoHanded(item));
        public static Item At(HeroSave hero,int slot,int index=0)=>hero.inventory.FirstOrDefault(i=>Occupies(i,slot,index));
        public static int[] Targets(HeroSave hero,Item item)
            =>item.slot==7||item.slot==0&&hero.heroClass==HeroClass.Warrior&&Kind(item)==WeaponKind.Melee?new[]{0,1}:new[]{Offhand(item)?1:0};
        public static bool Compatible(Item main,Item off,HeroClass heroClass)
        {
            if(off==null)return true;
            if(TwoHanded(main))return main==off;
            switch(Kind(off))
            {
                case WeaponKind.Orb:case WeaponKind.Scroll:return heroClass==HeroClass.Mage&&Kind(main)==WeaponKind.Wand;
                case WeaponKind.Arrows:return heroClass==HeroClass.Ranger&&Kind(main)==WeaponKind.Bow;
                case WeaponKind.Shield:return Kind(main)!=WeaponKind.Bow;
                case WeaponKind.Melee:return heroClass==HeroClass.Warrior;
                default:return false;
            }
        }
        public static EquipmentPlan Plan(HeroSave hero,Item item,int index=-1)
        {
            var plan=new EquipmentPlan();
            if(item==null||!hero.inventory.Contains(item)){plan.error="이 캐릭터가 보유한 장비만 장착할 수 있습니다.";return plan;}
            if(item.equipped){plan.error="이미 장착한 장비입니다.";return plan;}
            plan.error=Storage.WearError(hero,item);if(plan.error!="")return plan;
            var choices=Targets(hero,item);
            if(index<0)index=choices.Where(n=>At(hero,item.slot,n)==null).DefaultIfEmpty(choices[0]).First();
            if(!choices.Contains(index)){plan.error="이 장비를 해당 장착 위치에 넣을 수 없습니다.";return plan;}
            plan.index=index;
            foreach(var old in hero.inventory.Where(i=>i.equipped&&i.slot==item.slot))
                if(old.equipIndex==index||item.slot==0&&(TwoHanded(item)||TwoHanded(old)))plan.outgoing.Add(old.id);
            if(item.slot==0&&!TwoHanded(item))
            {
                var main=index==0?item:At(hero,0,0);var off=index==1?item:At(hero,0,1);
                if(main!=item&&main!=null&&plan.outgoing.Contains(main.id))main=null;
                if(off!=item&&off!=null&&plan.outgoing.Contains(off.id))off=null;
                if(!Compatible(main,off,hero.heroClass))
                {
                    if(index==1){plan.error=Kind(item)==WeaponKind.Arrows?"화살은 활과 함께 장착할 수 있습니다.":Kind(item)==WeaponKind.Orb||Kind(item)==WeaponKind.Scroll?"오브와 마법 스크롤은 한손 지팡이와 함께 장착할 수 있습니다.":"현재 주무기와 함께 장착할 수 없습니다.";return plan;}
                    if(off!=null)plan.outgoing.Add(off.id);
                }
            }
            if(hero.inventory.Count(i=>!i.equipped)-1+plan.outgoing.Count>hero.capacity)plan.error="교체할 장비를 돌려받을 소지품 공간이 부족합니다.";
            return plan;
        }
        // A two-handed weapon may land on either physical hand; the stored anchor remains main hand.
        // The hint and the eventual drop share all class, level, pairing and bag-capacity checks.
        public static EquipmentPlan PlanDrop(HeroSave hero,Item item,int slot,int index)
        {
            if(item==null||slot!=item.slot||index<0||index>1||slot!=0&&slot!=7&&index!=0)
                return new EquipmentPlan{error="이 장비를 해당 장착 위치에 넣을 수 없습니다."};
            return Plan(hero,item,slot==0&&TwoHanded(item)?0:index);
        }
        public static bool Equip(HeroSave hero,Item item,int index=-1)
        {
            var plan=Plan(hero,item,index);if(!plan.Valid)return false;
            var outgoing=hero.inventory.Where(i=>plan.outgoing.Contains(i.id)).ToArray();
            foreach(var old in outgoing)old.equipped=false;
            item.equipped=true;item.equipIndex=plan.index;Storage.HandOff(item,outgoing);
            foreach(var old in outgoing)if(old.storageSlot<0)Storage.PlaceInBag(hero,old);
            return true;
        }
        public static string UnequipError(HeroSave hero,Item item)
        {
            if(item==null||!item.equipped||!hero.inventory.Contains(item))return "장착한 장비를 선택해 주세요.";
            return Economy.FreeSlots(hero)<Removing(hero,item).Count?"장비를 돌려받을 소지품 공간이 부족합니다.":"";
        }
        static List<Item> Removing(HeroSave hero,Item item)
        {
            var items=new List<Item>{item};var off=At(hero,0,1);
            if(item.slot==0&&item.equipIndex==0&&off!=null&&off!=item&&!Compatible(null,off,hero.heroClass))items.Add(off);
            return items;
        }
        public static bool Unequip(HeroSave hero,Item item)
        {
            if(UnequipError(hero,item)!="")return false;
            foreach(var old in Removing(hero,item)){old.equipped=false;old.origin="";Storage.PlaceInBag(hero,old);}return true;
        }
        public static bool Valid(IEnumerable<Item> equipment,HeroClass? heroClass=null)
        {
            var items=equipment.ToArray();var positions=new HashSet<int>();
            if(items.Length>Count||items.Any(i=>i==null||i.slot<0||i.slot>7||i.equipIndex<0||i.equipIndex>1||i.slot!=0&&i.slot!=7&&i.equipIndex!=0))return false;
            foreach(var item in items)
            {
                if(!positions.Add(Position(item)))return false;
                if(TwoHanded(item)&&(item.equipIndex!=0||!positions.Add(8)))return false;
                if(Offhand(item)&&item.equipIndex!=1)return false;
                if(heroClass.HasValue&&(!ItemCatalog.Base(item).Fits(heroClass.Value,item.slot)||!Targets(new HeroSave{heroClass=heroClass.Value},item).Contains(item.equipIndex)))return false;
            }
            var main=items.FirstOrDefault(i=>i.slot==0&&i.equipIndex==0);var off=items.FirstOrDefault(i=>i.slot==0&&i.equipIndex==1);
            return !heroClass.HasValue||Compatible(main,off,heroClass.Value);
        }
        public static bool Assign(HeroSave hero,IReadOnlyList<Item> items,IReadOnlyList<int> positions=null)
        {
            if(items.Count>Count)return false;
            var trial=RuneGrowth.Copy(hero);trial.inventory=items.Select(RuneGrowth.Copy).ToList();trial.capacity=int.MaxValue;
            foreach(var item in trial.inventory)item.equipped=false;
            foreach(var item in trial.inventory.OrderBy(i=>Offhand(i)?1:0))
            {
                int n=trial.inventory.IndexOf(item),index=positions!=null&&positions.Count==items.Count?positions[n]:item.equipIndex;
                if(positions==null||positions.Count!=items.Count)
                {
                    var choices=Targets(trial,item);
                    if(!choices.Contains(index)||At(trial,item.slot,index)!=null)index=choices.Where(v=>At(trial,item.slot,v)==null).DefaultIfEmpty(-1).First();
                    if(index<0)return false;
                }
                var plan=Plan(trial,item,index);if(!plan.Valid||plan.outgoing.Count>0)return false;
                if(!Equip(trial,item,index))return false;
            }
            if(!Valid(trial.inventory,hero.heroClass))return false;
            for(int n=0;n<items.Count;n++)items[n].equipIndex=trial.inventory[n].equipIndex;
            return true;
        }
        public static string MainLabel(Item item)=>GearEnhancement.Name(item);
        public static string HandLabel(Item item)=>TwoHanded(item)?"양손 무기 · 두 칸 점유":Offhand(item)?"보조 장비":"한손 무기";
    }
    public sealed class EquipmentPlan
    {
        public string error="";public int index;public readonly HashSet<string> outgoing=new HashSet<string>();
        public bool Valid=>error=="";
    }
}
