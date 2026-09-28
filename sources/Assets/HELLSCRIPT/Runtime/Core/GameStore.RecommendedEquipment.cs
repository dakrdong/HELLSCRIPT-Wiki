using System;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    public enum EquipmentLootResult { Rejected, Acquired, SaveFailed }
    public sealed partial class GameStore
    {
        // Pickup, displacement, and the claimed flag share one atomic save and idempotent receipt.
        public EquipmentLootResult CommitRecommendedLoot(RunState run,string itemId,BagPolicy policy,bool rarityOnly)
        {
            if(Data.suspendedRun!=run||run.training>=0||run.heroId!=Data.Hero.id||run.portal)return EquipmentLootResult.Rejected;
            var drop=run.drops.SingleOrDefault(d=>d.item.id==itemId);
            if(drop==null||drop.ignored)return EquipmentLootResult.Rejected;
            if(drop.claimed)return EquipmentLootResult.Acquired;
            using var notifications=DeferNotifications();
            bool eligible=false;float health=run.health,resource=run.resource;
            bool saved=Transact("recommended-loot:"+run.id+":"+itemId,"recommended-loot:"+run.id+":"+itemId,a=>
            {
                var staged=a.suspendedRun;var candidate=staged.drops.Single(d=>d.item.id==itemId);
                if(candidate.claimed||candidate.ignored)return false;
                var rewardSnapshot=CombatJournal.Copy(candidate.item);
                if(!Economy.AddItem(a.Hero,candidate.item,policy,a,rarityOnly))return false;
                eligible=true;candidate.claimed=true;candidate.outcome=RiftLootOutcome.Kept;staged.lootCount++;
                if(candidate.item.equipped)
                {
                    var hero=RuneGrowth.Copy(a.Hero);hero.build=staged.build;
                    hero.slotProgress=new SlotProgress{levels=(int[])staged.slotLevels.Clone()};var stats=new HeroStats(hero,false,a.runes).WithActivePotion(staged.potions,hero.level);
                    health=staged.health=Mathf.Min(staged.health,stats.hp);resource=staged.resource=Mathf.Min(staged.resource,stats.maxResource);
                }
                candidate.item=rewardSnapshot;
                return true;
            });
            if(!saved)return eligible?EquipmentLootResult.SaveFailed:EquipmentLootResult.Rejected;
            // Keep the live run and drop objects: the loot loop and presentation own their identities.
            drop.item=CombatJournal.Copy(drop.item);drop.claimed=true;drop.outcome=RiftLootOutcome.Kept;run.lootCount++;run.health=health;run.resource=resource;
            return EquipmentLootResult.Acquired;
        }
    }
}
