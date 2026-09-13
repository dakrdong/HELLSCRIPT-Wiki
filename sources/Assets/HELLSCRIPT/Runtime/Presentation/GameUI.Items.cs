using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    public sealed partial class GameUI
    {
        readonly Color setGreen=new Color(.43f,.83f,.64f);
        bool GearServiceAvailable=>!game.Active&&game.Store.Data.suspendedRun==null;
        void EquipmentIcon(Transform parent,Item item,float x,float y,float size)
        {
            if(equipmentAtlas==null){Icon(parent,item.slot==0?21:item.slot>=6?23:22,x,y,size);return;}
            var r=Rect("Equipment "+item.baseId,parent);Place(r,x,y,size,size);var image=r.gameObject.AddComponent<UnityEngine.UI.RawImage>();image.texture=equipmentAtlas;
            int index=int.Parse(ItemCatalog.Base(item).id.Substring(1))-1;image.uvRect=new Rect(index%6/6f,1-(index/6+1)/4f,1/6f,1/4f);image.raycastTarget=false;
        }
        string Grade(Item item)=>!string.IsNullOrEmpty(ItemCatalog.Unique(item.special)?.setId)?"세트":GameCatalog.Rarities[item.rarity];
        Color ItemColor(Item item)=>Grade(item)=="세트"?setGreen:RarityColor(item.rarity);
        string Protection(Item item)
        {
            var labels=new List<string>();if(item.equipped)labels.Add("장착 중");if(item.locked)labels.Add("잠금");
            if(GemCatalog.HasGem(item))labels.Add("보석이 장착됨");
            foreach(var h in game.Store.Data.heroes)
            {
                if(h.build.equipmentIds?.Contains(item.id)??false)labels.Add("현재 설정 참조");
                for(int n=0;n<h.presets.Count;n++)if(h.presets[n]?.equipmentIds?.Contains(item.id)??false)labels.Add(Loc.F("{0} 프리셋 {1}", game.catalog.classNames[(int)h.heroClass], n+1));
            }
            return string.Join(" · ",labels.Select(Loc.T));
        }
        void ItemTransaction(string request,string operation,Func<AccountSave,bool> mutation,Action refresh,string success)
        {
            bool ok=game.Store.Transact(request,operation,mutation);string message=ok?success:game.Store.Error;
            refresh();ShowToast(message);
        }
        Action TransactionAction(string operation,Func<AccountSave,bool> mutation,Action refresh,string success)
        {
            string request=Guid.NewGuid().ToString("N");return ()=>ItemTransaction(request,operation,mutation,refresh,success);
        }
        static Item FindOwned(AccountSave a,string id)=>a.Hero.inventory.Find(i=>i.id==id);
        void RenderEquipment(bool portal) => OpenInventory(false,portal);
        public void ShowItemDetail(string id,bool portal=false)
        {
            if(inventoryList==null||inventoryWarehouse||portalBag!=portal)OpenInventory(false,portal);
            SelectInventoryItem(id);
        }
        void ShowEquipmentComparison(HeroSave hero,Item item)
        {
            if(item.equipped)return;
            string error=Economy.EquipError(hero,item);if(error!=""){Note(content,Loc.F("장착 조건: {0}", error),20,70,gold);return;}
            var copy=JsonUtility.FromJson<HeroSave>(JsonUtility.ToJson(hero));var candidate=copy.inventory.Find(i=>i.id==item.id);var old=hero.inventory.Find(i=>i.equipped&&i.slot==item.slot);
            var before=new HeroStats(hero,false,game.Store.Data.runes);Economy.Equip(copy,candidate);var after=new HeroStats(copy,false,game.Store.Data.runes);
            Note(content,Loc.F("교체 후 전체 능력치\n현재: {0}", (old?.DisplayName??"빈 부위")),22,94,gold);
            CompareStat("최대 HP",before.hp,after.hp);CompareStat("공격 기준",before.damage,after.damage);CompareStat("방어도",before.armor,after.armor);CompareStat("비물리 저항",before.resistance,after.resistance);
            CompareStat("치명 확률 %",before.crit*100,after.crit*100);CompareStat("치명 피해 %",before.critDamage*100,after.critDamage*100);
            CompareStat("자원 회복 / 초",before.regen,after.regen);CompareStat("자원 소모 감소 %",before.costReduction*100,after.costReduction*100);CompareStat("쿨타임 감소 %",before.cdr*100,after.cdr*100);CompareStat("이동속도 m/s",before.speed,after.speed);
            for(int i=4;i<=9;i++)if(before.bonuses[i]!=after.bonuses[i])CompareStat(StatCatalog.Get(i),before.bonuses[i],after.bonuses[i]);
            foreach(var stat in new[]{StatId.FireResistance,StatId.ColdResistance,StatId.LightningResistance,StatId.PoisonResistance,StatId.ShadowResistance,StatId.BarrierGeneration,StatId.PotionHealing})
                if(Mathf.Abs(before.Sheet(stat)-after.Sheet(stat))>.0001f)CompareStat(StatCatalog.Get(stat),before.Sheet(stat),after.Sheet(stat));
            if(before.gemBuffReduction!=after.gemBuffReduction)CompareStat("보석의 받는 피해 감소 %",before.gemBuffReduction*100,after.gemBuffReduction*100);
            if(before.gemPeriodicReduction!=after.gemPeriodicReduction)CompareStat("보석의 지속 피해 감소 %",before.gemPeriodicReduction*100,after.gemPeriodicReduction*100);
            if(GemCatalog.HasGem(old))Note(content,Loc.F("교체로 빠지는 보석 효과\n{0}\n보석은 기존 장비에 남습니다.",GemCatalog.SocketSummary(old)),19,118,gold);
            foreach(var set in ItemCatalog.Sets)
            {
                int from=before.SetPieces(set.id),to=after.SetPieces(set.id);if(from==0&&to==0)continue;
                string change=from>=4&&to<4?" · 4세트 해제":from>=2&&to<2?" · 2세트 해제":from<4&&to>=4?" · 4세트 활성":from<2&&to>=2?" · 2세트 활성":"";
                Note(content,Loc.F("{0}  {1} → {2}부위{3}", set.name, from, to, change),20,76,change.Contains("해제")?gold:setGreen);
                if(from>=4&&to<4)Note(content,Loc.F("해제되는 4세트 효과\n{0}", set.four),19,105,gold);
                if(from>=2&&to<2)Note(content,Loc.F("해제되는 2세트 효과\n{0}", set.two),19,95,gold);
            }
            if(old!=null&&!string.IsNullOrEmpty(old.special)&&old.special!=item.special&&string.IsNullOrEmpty(ItemCatalog.Unique(old.special).setId))Note(content,Loc.F("교체로 빠지는 전설 효과: {0}\n{1}", old.DisplayName, ItemCatalog.Unique(old.special).description),19,130,gold);
        }
        void CompareStat(string label,float before,float after)
        {float delta=after-before;Note(content,Loc.F("{0}   {1:0.#} → {2:0.#}   ({3:+0.#;-0.#;0})",label,before,after,delta),20,44,Mathf.Abs(delta)<.001f?muted:delta>0?setGreen:gold);}
        // A catalogued stat prints its own unit, so a percentage reads as one on both sides of the arrow.
        void CompareStat(StatDefinition stat,float before,float after)
        {float delta=after-before;Note(content,Loc.F("{0}   {1} → {2}   ({3})",stat.Name,stat.Format(before),stat.Format(after),stat.Signed(delta)),20,44,Mathf.Abs(delta)<.001f?muted:delta>0?setGreen:gold);}
        void RenderWarehouse(bool portal) => OpenInventory(true,portal);
        void RenderItemShop()
        {
            var a=game.Store.Data;var h=a.Hero;pageRepaint=()=>RenderItemShop();Base("shop","수수께끼 상인","모든 제작 경로에서 전설·세트를 발견할 수 있습니다");
            int level=Mathf.Max(1,h.highestClear),slot=selectedSlot;Note(content,Loc.F("골드 {0:N0} · 재료 {1:N0}\n생성 아이템 레벨 {2} · 최고 실클리어 기준", a.gold, a.materials, level),22,90,pale);
            Cycle(content,"선택 부위",GameCatalog.Slots,slot,i=>{selectedSlot=i;ShowShop();});
            int gambleCost=500+50*h.highestClear;
            ContentButton(ContentUnlocks.Shop,Loc.F("미확인 {0} · {1:N0} 골드", GameCatalog.Slots[slot], gambleCost),PurchaseAction(slot,level,0,gambleCost,0));
            ContentButton(ContentUnlocks.RareCraft,Loc.F("희귀 제작 · 재료 50 / {0:N0} 골드", 500L*level),PurchaseAction(slot,level,1,500*level,50));
            ContentButton(ContentUnlocks.CoreCraft,Loc.F("전설·세트 제작 · {0} 코어 {1}/10", GameCatalog.Slots[slot], a.cores[slot]),PurchaseAction(slot,level,2,0,0),true);
            Note(content,"베이스와 옵션은 확정 시 생성됩니다. 직업·부위에 맞는 접두/접미와 수치 티어를 적용합니다.",20,95);
            BigButton(content,"전설·세트 도감",ShowItemCollection);
            FooterButton(0,2,"장비 보기",()=>ShowBag());FooterButton(1,2,"성소로",ShowTown);
        }
        Action PurchaseAction(int slot,int level,int mode,int goldCost,int materials)
        {
            string request=Guid.NewGuid().ToString("N"),operation=$"purchase:{game.Store.Data.Hero.id}:{slot}:{level}:{mode}";
            return ()=>
            {
                uint rng=(uint)DateTime.UtcNow.Ticks;string acquired="";
                ItemTransaction(request,operation,staged=>
                {
                    bool ok=ContentServices.Purchase(staged,slot,mode,ref rng,out var item);
                    if(ok)acquired=item.DisplayName;return ok;
                },ShowShop,"장비를 획득했습니다.");
                if(acquired!=""&&game.Store.Error=="")ShowToast(Loc.F("{0} 획득", acquired));
            };
        }
        void ShowItemCollection()
        {
            var a=game.Store.Data;var h=a.Hero;pageRepaint=()=>ShowItemCollection();Base("collection","전설·세트 도감",Loc.F("{0} 전용 장비와 공용 장비", game.catalog.classNames[(int)h.heroClass]));
            var all=a.heroes.SelectMany(hero=>hero.inventory).Concat(a.warehouse).ToArray();var stats=new HeroStats(h,false,game.Store.Data.runes);
            foreach(var set in ItemCatalog.Sets.Where(s=>s.heroClass==h.heroClass))
            {
                var owned=all.Where(i=>ItemCatalog.Unique(i.special)?.setId==set.id).Select(i=>i.slot).Distinct().Count();
                Note(content,Loc.F("{0} · 보유 {1}/4 · 장착 {2}/4", set.name, owned, stats.SetPieces(set.id)),23,78,setGreen);
                Note(content,Loc.F("2세트 · {0}\n4세트 · {1}", set.two, set.four),20,168,pale);
            }
            foreach(var definition in ItemCatalog.Uniques.Where(d=>string.IsNullOrEmpty(d.setId)&&(d.heroClass<0||d.heroClass==(int)h.heroClass)))
            {
                Note(content,Loc.F("{0}{1} / {2}", (all.Any(i=>i.special==definition.id)?"보유 · ":"미보유 · "), definition.name, GameCatalog.Slots[definition.slot]),23,68,gold);
                Note(content,definition.Description,20,116,pale);
                Note(content,Loc.F("획득: 균열 전리품 · 소탕 · 상점 · 해당 부위 코어 제작\n호환 베이스: {0}", string.Join(", ",ItemCatalog.Bases.Where(b=>b.Fits(h.heroClass,definition.slot)).Select(b=>b.name))),18,90);
            }
            FooterButton(0,1,"장비 목록으로",()=>ShowBag());
        }
    }
}
