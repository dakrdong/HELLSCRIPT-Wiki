using System;
using System.Linq;

namespace Hellscript
{
    public enum WarehouseAdmission { Keep, LowestPrice, LowestScore, Oldest }

    // Automatic actions only. Manual warehouse moves never authorize a sale.
    public static class EquipmentAutomation
    {
        public const string Displaced="autoEquip.displaced",Admission="bag.warehouseAdmission";
        public static bool NewOption(string id)=>id==Displaced||id==Admission;
        public static bool HasWarehouseRoom(AccountSave account)=>Enumerable.Range(0,StorageRules.Tabs).Any(t=>
            Storage.Unlocked(account,t)&&!Storage.Overfull(account,StorageSide.Warehouse,t)&&Storage.FirstFree(account,StorageSide.Warehouse,t)>=0);

        // Run on a staged account: disposal and admission are part of the same saved transaction.
        public static bool Deposit(AccountSave account,Item item,EdictCleanupPolicy policy,EdictCleanupReport report=null)
        {
            if(item==null||item.equipped||!account.Hero.inventory.Contains(item)||Tutorials.RequiredArmor(account,item.id))return false;
            if(Storage.Deposit(account,item)){RiftResult.Disposed(account,item.id,RiftLootOutcome.Stored);return true;}
            if(policy.warehouseAdmission==WarehouseAdmission.Keep)return false;
            // Legacy overflow tabs remain withdrawal-only; never sell several items just to repair one.
            var eligible=account.warehouse.Where(i=>Storage.Unlocked(account,i.storageTab)&&
                !Storage.Overfull(account,StorageSide.Warehouse,i.storageTab)&&!policy.Protects(account,i,EdictCleanupAction.Sell)&&Economy.CanSellStored(account,i));
            var ordered=policy.warehouseAdmission==WarehouseAdmission.LowestPrice?eligible.OrderBy(i=>(double)i.Price):
                policy.warehouseAdmission==WarehouseAdmission.LowestScore?eligible.OrderBy(i=>(double)EquipmentScore.Value(i)):eligible.OrderBy(i=>0d);
            var victim=ordered.ThenBy(i=>i.warehouseOrder).ThenBy(i=>i.id,StringComparer.Ordinal).FirstOrDefault();
            if(victim==null)return false;
            if(!Economy.SellStored(account,victim)||!Storage.Deposit(account,item))
                throw new InvalidOperationException("창고 자동 보관을 완료하지 못했습니다. 장비와 재화는 유지했습니다.");
            RiftResult.Disposed(account,item.id,RiftLootOutcome.Stored);
            if(report!=null)report.sold++;
            return true;
        }
        public static void Replaced(AccountSave account,Item[] outgoing)
        {
            var hero=account.Hero;
            var policy=EdictCleanupPolicy.Compile(HuntEdictV2.Canonical(HuntEdictDefaults.Resolve(hero)).global.ToDictionary(o=>o.id,o=>o.value));
            string mode=EquipmentRecommendation.Get(HuntEdictDefaults.Resolve(hero),Displaced);
            var action=mode=="SELL"?EdictCleanupAction.Sell:mode=="SALVAGE"?EdictCleanupAction.Salvage:EdictCleanupAction.Warehouse;
            foreach(var item in outgoing)
            {
                if(policy.Protects(account,item,action))continue;
                if(action==EdictCleanupAction.Warehouse)
                    item.pendingAutoStorage=!Deposit(account,item,policy);
                else if(action==EdictCleanupAction.Sell)Economy.Sell(account,hero,item);
                else Economy.Dismantle(account,hero,item);
                // Rejected disposal (protection or a currency/material cap) keeps the item in the bag.
            }
            Storage.SettleAll(account);
        }
        public static bool HasPending(AccountSave account)=>EquipmentRecommendation.Enabled(account.Hero)&&
            account.Hero.inventory.Any(i=>i.pendingAutoStorage&&!i.equipped);
        public static void StorePending(AccountSave account,EdictCleanupPolicy policy)
        {
            if(!HasPending(account))return;
            foreach(var item in account.Hero.inventory.Where(i=>i.pendingAutoStorage&&!i.equipped).ToArray())
                if(!policy.Protects(account,item,EdictCleanupAction.Warehouse))Deposit(account,item,policy);
        }
    }
    public sealed partial class GameStore
    {
        public bool StorePendingEquipment(EdictCleanupPolicy policy)
        {
            if(policy==null||!EquipmentAutomation.HasPending(Data))return true;
            return Transact(Guid.NewGuid().ToString("N"),"recommended-storage",a=>{EquipmentAutomation.StorePending(a,policy);return true;});
        }
    }
}
