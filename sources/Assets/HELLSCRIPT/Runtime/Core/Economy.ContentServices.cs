using System;
using UnityEngine;

namespace Hellscript
{
    public static class ContentServices
    {
        public static string PurchaseFeature(int mode)=>mode==0?ContentUnlocks.Shop:mode==1?ContentUnlocks.RareCraft:ContentUnlocks.CoreCraft;
        public static bool Purchase(AccountSave a,int slot,int mode,ref uint rng,out Item item)
        {
            item=null;
            if(mode<0||mode>2||slot<0||slot>=a.cores.Length||a.suspendedRun!=null||!ContentUnlocks.Has(a,PurchaseFeature(mode)))return false;
            int level=Math.Max(1,a.Hero.highestClear);
            long gold=mode==0?500L+50L*a.Hero.highestClear:mode==1?500L*level:0;
            int materials=mode==1?50:0;
            if(a.gold<gold||a.materials<materials||Economy.FreeSlots(a.Hero)<=0||mode==2&&a.cores[slot]<ContentUnlocks.Rules.coreCount)return false;
            uint next=rng;float roll=RandomStream.Unit(ref next);int rarity=mode==2?3:mode==1?2:roll<.02f?3:roll<.4f?2:1;
            var created=Economy.CreateItem(a.Hero.heroClass,slot,rarity,level,ref next);
            if(!Economy.AddItem(a.Hero,created,BagPolicy.Ignore,a))return false;
            a.gold-=(int)gold;a.materials-=materials;if(mode==2)a.cores[slot]-=ContentUnlocks.Rules.coreCount;
            item=created;rng=next;return true;
        }
    }
}
