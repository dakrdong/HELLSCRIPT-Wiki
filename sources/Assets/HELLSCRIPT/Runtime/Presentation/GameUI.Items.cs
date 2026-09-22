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
            if(ItemCatalog.Base(item).legacyIndex>=24)
            {var symbol=Rect("Equipment "+item.baseId,parent);Place(symbol,x+size*.12f,y+size*.12f,size*.76f,size*.76f);var icon=symbol.gameObject.AddComponent<StorageGlyph>();icon.symbol=EquipmentSlots.Kind(item).ToString().ToLowerInvariant();icon.color=gold;icon.raycastTarget=false;return;}
            if(equipmentAtlas==null){Icon(parent,item.slot==0?21:item.slot>=6?23:22,x,y,size);return;}
            var r=Rect("Equipment "+item.baseId,parent);Place(r,x,y,size,size);var image=r.gameObject.AddComponent<UnityEngine.UI.RawImage>();image.texture=equipmentAtlas;
            int index=ItemCatalog.AtlasIndex(item);image.uvRect=new Rect(index%6/6f,1-(index/6+1)/4f,1/6f,1/4f);image.raycastTarget=false;
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
        void RenderEquipment(bool portal) => OpenNativeInventory(portal);
        public void ShowItemDetail(string id,bool portal=false)
        {
            OpenNativeInventory(portal);nativeInventory?.ShowDetail(id);
        }
        void ShowEquipmentComparison(HeroSave hero,Item item,RuneGrowthState frozenRunes=null,int targetIndex=-1)
        {
            if(item==null||item.equipped)return;
            var host=Row(content,520);host.name="Legacy equipment comparison";
            EquipmentComparisonView.Create(host,hero,item,frozenRunes??game.Store.Data.runes,font,equipmentAtlas,1.25f,targetIndex);
        }
        void CompareStat(string label,float before,float after)
        {float delta=after-before;Note(content,Loc.F("{0}   {1:0.#} → {2:0.#}   ({3:+0.#;-0.#;0})",label,before,after,delta),20,44,Mathf.Abs(delta)<.001f?muted:delta>0?setGreen:gold);}
        // A catalogued stat prints its own unit, so a percentage reads as one on both sides of the arrow.
        void CompareStat(StatDefinition stat,float before,float after)
        {float delta=after-before;Note(content,Loc.F("{0}   {1} → {2}   ({3})",stat.Name,stat.Format(before),stat.Format(after),stat.Signed(delta)),20,44,Mathf.Abs(delta)<.001f?muted:delta>0?setGreen:gold);}
        void RenderWarehouse(bool portal) => OpenInventory(true,portal);
        void RenderItemShop()
        {
            var a=game.Store.Data;var h=a.Hero;pageRepaint=()=>RenderItemShop();Base("shop","장비 제작","모든 제작 경로에서 전설·세트를 발견할 수 있습니다");
            int level=Mathf.Max(1,h.highestClear),slot=selectedSlot;Note(content,Loc.F("골드 {0:N0} · 재료 {1:N0}\n생성 아이템 레벨 {2} · 최고 실클리어 기준", a.gold, a.materials, Mathf.Min(level,ItemQuality.MaximumItemLevel)),22,90,pale);
            Cycle(content,"선택 부위",GameCatalog.Slots,slot,i=>{selectedSlot=i;ShowShop();});
            ContentButton(ContentUnlocks.Shop,"갬블 상인 찾아가기",()=>{ShowTown();game.RequestStation(TownStation.Gambler);});
            content.GetComponentsInChildren<UnityEngine.UI.Button>().Last().name="gamble-route";
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
                Note(content,Loc.F("{0}{1} / {2}", (all.Any(i=>i.special==definition.id)?"보유 · ":"미보유 · "), definition.Name, GameCatalog.Slots[definition.slot]),23,68,gold);
                Note(content,definition.Description,20,116,pale);
                Note(content,Loc.F("획득: 균열 전리품 · 소탕 · 상점 · 해당 부위 코어 제작\n호환 베이스: {0}", string.Join(", ",ItemCatalog.Bases.Where(b=>b.Fits(h.heroClass,definition.slot)).Select(b=>b.name))),18,90);
            }
            FooterButton(0,1,"장비 목록으로",()=>ShowBag());
        }
    }
}
