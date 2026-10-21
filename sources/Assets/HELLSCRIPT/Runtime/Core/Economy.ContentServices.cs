using System;
using UnityEngine;

namespace Hellscript
{
    public static class ContentServices
    {
        // Mode 0 is the unidentified purchase and mode 2 the core craft. Mode 1 was the rare craft and stays unused so the numbers keep their meaning.
        public static string PurchaseFeature(int mode)=>mode==0?ContentUnlocks.Shop:ContentUnlocks.CoreCraft;
        public static long PurchaseGold(HeroSave hero,int mode)=>mode==0?500L+50L*hero.highestClear:0;
        public static bool Purchase(AccountSave a,int slot,int mode,ref uint rng,out Item item)
        {
            item=null;
            if(mode<0||mode>2||mode==1||slot<0||slot>=a.cores.Length||(mode==0&&a.suspendedRun!=null)||!(mode==0?ContentUnlocks.Has(a,PurchaseFeature(mode)):ContentUnlocks.ForgeHas(a,PurchaseFeature(mode))))return false;
            int level=Math.Max(1,a.Hero.highestClear);
            long gold=PurchaseGold(a.Hero,mode);
            if(a.gold<gold||Economy.FreeSlots(a.Hero)<=0||mode==2&&a.cores[slot]<ContentUnlocks.Rules.coreCount)return false;
            uint next=rng;float roll=RandomStream.Unit(ref next);int rarity=mode==2?3:roll<.02f?3:roll<.4f?2:1;
            var created=Economy.CreateItem(a.Hero.heroClass,slot,rarity,level,ref next);
            created.acquisitionKind="craft";
            if(!Economy.AddItem(a.Hero,created,BagPolicy.Ignore,a))return false;
            a.gold-=(int)gold;if(mode==2)a.cores[slot]-=ContentUnlocks.Rules.coreCount;
            item=created;rng=next;return true;
        }
    }
}
