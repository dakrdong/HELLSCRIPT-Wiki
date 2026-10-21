using System;
using System.Linq;

namespace Hellscript
{
    public enum GemBatch { One=1, Five=5, All=0 }
    public static class Jeweler
    {
        public static bool Available(AccountSave a)=>a!=null&&a.suspendedRun==null&&ContentUnlocks.Has(a,ContentUnlocks.Gem);
        public static bool ValidBatch(GemBatch batch)=>batch==GemBatch.One||batch==GemBatch.Five||batch==GemBatch.All;
        // The most one craft can make: the gems in stock, bounded by the potion stack limit; zero while the service or the recipe is locked.
        public static int CraftLimit(AccountSave a,string gemId,int tier)
        {
            if(!Available(a)||!ContentUnlocks.Has(a,ContentUnlocks.Elixir)||GemElixirs.Family(gemId)==null||!GemCatalog.Valid(gemId,tier))return 0;
            return (int)Math.Max(0,Math.Min(GemStacks.Count(a.gems,gemId,tier),GemElixirs.PotionLimit-a.Hero.potions.Count(GemElixirs.Id(gemId,tier))));
        }
        public static int CraftCount(AccountSave a,string gemId,int tier,GemBatch batch)
        {
            if(!ValidBatch(batch))return 0;
            long maximum=CraftLimit(a,gemId,tier);int count=batch==GemBatch.All?(int)maximum:(int)batch;return count>0&&count<=maximum?count:0;
        }
        public static bool Craft(AccountSave a,string heroId,string gemId,int tier,GemBatch batch)=>a?.Hero.id==heroId&&Craft(a,heroId,gemId,tier,CraftCount(a,gemId,tier,batch));
        public static bool Craft(AccountSave a,string heroId,string gemId,int tier,int count)
        {
            if(a?.Hero.id!=heroId||count<=0||count>CraftLimit(a,gemId,tier))return false;
            if(!GemStacks.TryExchange(a.gems,a.gemCapacity,new[]{new GemStack{gemId=gemId,tier=tier,count=count}},Array.Empty<GemStack>()))return false;
            var stock=a.Hero.potions;stock.Activate();string id=GemElixirs.Id(gemId,tier);stock.Set(id,stock.Count(id)+count);stock.revision++;return true;
        }
        public static bool Convert(AccountSave a,string gemId,int tier,bool upgrade,GemBatch batch)
            =>Available(a)&&GemElixirs.Family(gemId)!=null&&GemStacks.TryConvert(a.gems,a.gemCapacity,gemId,tier,upgrade,batch);
    }
    public sealed partial class GameStore
    {
        // Town position is session-owned, so inspect the live owner again at execution.
        bool AtJeweler(TownWalk town)=>town?.Nearby==TownStation.GemMerchant;
        public bool ConvertJewelerGems(string request,string gemId,int tier,bool upgrade,GemBatch batch,TownWalk town)
            =>AtJeweler(town)&&Transact(request,"jeweler-convert:"+gemId+":"+tier+":"+upgrade+":"+batch,a=>Jeweler.Convert(a,gemId,tier,upgrade,batch));
        public bool CraftGemPotion(string request,string heroId,string gemId,int tier,GemBatch batch,TownWalk town)
            =>AtJeweler(town)&&Transact(request,"jeweler-craft:"+heroId+":"+gemId+":"+tier+":"+batch,a=>Jeweler.Craft(a,heroId,gemId,tier,batch));
        public bool CraftGemPotion(string request,string heroId,string gemId,int tier,int count,TownWalk town)
            =>AtJeweler(town)&&Transact(request,"jeweler-craft:"+heroId+":"+gemId+":"+tier+":"+count,a=>Jeweler.Craft(a,heroId,gemId,tier,count));
    }
}
