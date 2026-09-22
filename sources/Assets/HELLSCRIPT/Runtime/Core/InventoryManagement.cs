using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Hellscript
{
    public enum InventoryGrade { All, Normal, Magic, Rare, Legendary, Set }
    public enum InventoryOrder { Equipment, Newest, Oldest, Level, Grade }
    public sealed class InventoryQuery
    {
        public int slot=-1, heroClass=-2;
        public InventoryGrade grade;
        public InventoryOrder order;
        public string setId="",affixId="",uniqueId="";
        public bool unreadOnly,awakenedOnly;
        public IEnumerable<Item> Apply(IEnumerable<Item> source)
        {
            var indexed=source.Select((item,index)=>new{item,index});
            var filtered=indexed.Where(x=>Matches(x.item));
            var ordered=order==InventoryOrder.Newest?filtered.OrderByDescending(x=>x.item.acquiredOrder>0).ThenByDescending(x=>x.item.acquiredOrder):
                order==InventoryOrder.Oldest?filtered.OrderByDescending(x=>x.item.acquiredOrder>0).ThenBy(x=>x.item.acquiredOrder):
                order==InventoryOrder.Level?filtered.OrderByDescending(x=>x.item.level).ThenByDescending(x=>x.item.rarity):
                order==InventoryOrder.Grade?filtered.OrderByDescending(x=>x.item.rarity).ThenByDescending(x=>x.item.level):
                filtered.OrderByDescending(x=>x.item.equipped).ThenBy(x=>x.item.slot).ThenByDescending(x=>x.item.rarity).ThenByDescending(x=>x.item.level);
            return ordered.ThenBy(x=>x.index).Select(x=>x.item);
        }
        public bool Matches(Item item)
        {
            if(item==null||slot>=0&&item.slot!=slot)return false;
            if(awakenedOnly&&!item.awakened)return false;
            var unique=ItemCatalog.Unique(item.special);string set=unique?.setId??"";
            if(grade==InventoryGrade.Set&&set==""||grade==InventoryGrade.Legendary&&(item.rarity!=3||set!="")||grade>InventoryGrade.All&&grade<InventoryGrade.Legendary&&item.rarity!=(int)grade-1)return false;
            int restriction=unique!=null&&unique.heroClass>=0?unique.heroClass:ItemCatalog.Base(item).heroClass;
            if(heroClass==-1&&restriction!=-1||heroClass>=0&&restriction>=0&&restriction!=heroClass)return false;
            if(setId!=""&&set!=setId||affixId!=""&&!item.rolls.Any(r=>r.affixId==affixId)||uniqueId!=""&&item.special!=uniqueId)return false;
            return !unreadOnly||item.acquiredOrder>0&&!item.reviewed;
        }
    }

    public static class ItemAcquisition
    {
        public static void Stamp(AccountSave account,Item item)
        {
            if(account==null||item.acquiredOrder>0)return;
            ContentUnlocks.RecordEquipment(account,item);
            account.itemSequence=checked(account.itemSequence+1);item.acquiredOrder=account.itemSequence;item.reviewed=false;
        }
        // Unknown legacy acquisition dates stay unknown. Moving or reviewing an item never re-stamps it.
        public static void NormalizeCounter(AccountSave account)
        {account.itemSequence=Math.Max(0,Math.Max(account.itemSequence,account.heroes.SelectMany(h=>h.inventory).Concat(account.warehouse).Select(i=>i.acquiredOrder).DefaultIfEmpty(0).Max()));}
    }

    public enum InventoryBulkOperation { Sell, Dismantle }
    public sealed class InventoryBulkEntry
    {
        public readonly string id,name,excludedReason,snapshot;
        public readonly long gold,materials,stones;
        internal InventoryBulkEntry(AccountSave account,Item item,InventoryBulkOperation operation)
        {
            id=item.id;name=item.DisplayName;excludedReason=InventoryBulkPlan.Exclusion(account,item);snapshot=InventoryBulkPlan.Fingerprint(account,item);
            if(excludedReason!="")return;
            if(operation==InventoryBulkOperation.Sell)gold=item.Price;
            else{materials=new[]{1,2,5}[item.rarity]+(item.contentVersion>0?item.investedMaterials:20L*((1<<Math.Clamp(item.enhancement,0,5))-1))*4/5;stones=BlacksmithCatalog.SalvageStones(item);}
        }
    }
    public sealed class InventoryBulkPlan
    {
        public readonly string heroId,requestId,operationKey;
        public readonly InventoryBulkOperation operation;
        public readonly IReadOnlyList<InventoryBulkEntry> entries;
        public int Count=>entries.Count(e=>e.excludedReason=="");
        public long Gold=>entries.Sum(e=>e.gold);
        public long Materials=>entries.Sum(e=>e.materials);
        public long Stones=>entries.Sum(e=>e.stones);
        public InventoryBulkPlan(AccountSave account,IEnumerable<string> ids,InventoryBulkOperation operation)
        {
            this.operation=operation;heroId=account.Hero.id;requestId=Guid.NewGuid().ToString("N");
            var owned=account.Hero.inventory.ToDictionary(i=>i.id,StringComparer.Ordinal);
            var values=ids.Distinct(StringComparer.Ordinal).Select(id=>owned.TryGetValue(id,out var item)?new InventoryBulkEntry(account,item,operation):throw new ArgumentException("이 캐릭터의 가방에 없는 장비입니다.")).ToArray();
            entries=Array.AsReadOnly(values);
            operationKey="inventory-bulk:"+heroId+":"+operation+":"+Hash(string.Join("|",values.Select(e=>e.id+":"+e.snapshot)));
        }
        internal static string Exclusion(AccountSave account,Item item)
        {
            var reasons=new List<string>();if(item.equipped)reasons.Add("장착 중");if(item.locked)reasons.Add("잠금");
            if(account.heroes.Any(h=>Economy.Referenced(h,item)))reasons.Add("현재 설정·프리셋 참조");
            if(GemCatalog.HasGem(item))reasons.Add("보석이 장착됨");
            if(item.rarity>=3)reasons.Add("전설·세트는 개별 처리");return string.Join(" · ",reasons);
        }
        internal static string Fingerprint(AccountSave account,Item item)=>Hash(JsonUtility.ToJson(item)+"|"+Exclusion(account,item));
        static string Hash(string text){using(var hash=SHA256.Create())return Convert.ToBase64String(hash.ComputeHash(Encoding.UTF8.GetBytes(text)));}
        // Apply only to GameStore.Transact's staged account. Revalidate every previewed instance before mutation.
        public bool Apply(AccountSave staged)
        {
            if(staged.Hero.id!=heroId||Count==0||Gold>int.MaxValue-staged.gold||Materials>int.MaxValue-staged.materials||Stones>int.MaxValue-staged.enhancementStones)return false;
            var items=new List<Item>();
            foreach(var entry in entries)
            {
                var item=staged.Hero.inventory.Find(i=>i.id==entry.id);
                if(item==null||Fingerprint(staged,item)!=entry.snapshot)return false;
                if(entry.excludedReason=="")items.Add(item);
            }
            foreach(var item in items)if(!(operation==InventoryBulkOperation.Sell?Economy.Sell(staged,staged.Hero,item):Economy.Dismantle(staged,staged.Hero,item)))return false;
            return true;
        }
    }
}
