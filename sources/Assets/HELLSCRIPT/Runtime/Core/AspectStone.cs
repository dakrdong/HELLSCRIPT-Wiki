using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    [Serializable] public sealed class AspectProgress
    {
        public string id;
        public int count,level;
    }

    // Account collection and item imprint are separate: upgrading the library never rewrites gear.
    public static class AspectStone
    {
        public const int MaximumLevel=5;
        public static IEnumerable<UniqueItemDefinition> Catalog=>ItemCatalog.Uniques.Where(d=>d.id.StartsWith("L",StringComparison.Ordinal)&&string.IsNullOrEmpty(d.setId));
        public static UniqueItemDefinition Definition(string id)
        {var d=ItemCatalog.Unique(id);return d!=null&&d.id.StartsWith("L",StringComparison.Ordinal)&&string.IsNullOrEmpty(d.setId)?d:null;}
        public static int Threshold(int level)=>level>=1&&level<=MaximumLevel?1<<(level-1):throw new ArgumentOutOfRangeException(nameof(level));
        public static int Attainable(int count)
        {int result=0;for(int level=1;level<=MaximumLevel&&count>=Threshold(level);level++)result=level;return result;}
        public static AspectProgress Progress(AccountSave account,string id)=>account?.aspects?.Find(p=>p.id==id);
        public static int Count(AccountSave account,string id)=>Progress(account,id)?.count??0;
        public static int Level(AccountSave account,string id)=>Progress(account,id)?.level??0;
        public static bool CanUpgrade(AccountSave account,string id)=>ContentUnlocks.Has(account,ContentUnlocks.Aspect)&&Level(account,id)>0&&Level(account,id)<Attainable(Count(account,id));
        public static int UpgradableCount(AccountSave account)=>account.aspects.Count(p=>CanUpgrade(account,p.id));
        public static void Normalize(AccountSave account)
        {
            account.aspects??=new List<AspectProgress>();
            var ids=new HashSet<string>(StringComparer.Ordinal);
            foreach(var p in account.aspects)
                if(p==null||Definition(p.id)==null||!ids.Add(p.id)||p.count<1||p.level<1||p.level>Attainable(p.count))
                    throw new NotSupportedException(Loc.T("위상 수집 기록을 안전하게 읽을 수 없어 불러오기를 중단했습니다. 원본 저장 파일은 보존했습니다."));
        }
        public static string PowerId(Item item)=>item==null?"":!string.IsNullOrEmpty(item.aspectId)?item.aspectId:item.special;
        public static int PowerLevel(Item item)=>Definition(PowerId(item))==null?0:!string.IsNullOrEmpty(item.aspectId)?item.aspectLevel:1;
        public static bool Eligible(Item item)=>item!=null&&(item.rarity==2||item.rarity==3)&&string.IsNullOrEmpty(ItemCatalog.Unique(item.special)?.setId);
        // Only the originally generated power can be collected. Free imprints cannot mint copies.
        public static string SalvagedId(Item item)=>item?.rarity==3&&Definition(item.special)!=null?item.special:null;
        public static bool CanCollect(AccountSave account,Item item)
        {string id=SalvagedId(item);return id==null||Count(account,id)<int.MaxValue;}
        internal static void Collect(AccountSave account,Item item)
        {
            string id=SalvagedId(item);if(id==null)return;
            var p=Progress(account,id);
            if(p==null)account.aspects.Add(new AspectProgress{id=id,count=1,level=1});
            else p.count=checked(p.count+1);
        }
        public static void Validate(Item item)
        {
            if(string.IsNullOrEmpty(item.aspectId))
            {if(item.aspectLevel!=0)throw new InvalidOperationException("Aspect level without an imprint.");return;}
            if(!Eligible(item)||item.rarity!=3||Definition(item.aspectId)==null||item.aspectLevel<1||item.aspectLevel>MaximumLevel)
                throw new InvalidOperationException("Invalid equipment aspect imprint.");
        }
        public static string Effect(Item item)=>Definition(PowerId(item))!=null?AspectGrowth.Description(PowerId(item),PowerLevel(item)):ItemCatalog.Unique(item?.special)?.Description??"";
    }

    // The confirmation freezes the exact equipped item and library level, then rechecks at commit.
    public sealed class AspectImprintPlan
    {
        public readonly string requestId=Guid.NewGuid().ToString("N"),heroId,itemId,aspectId,fingerprint;
        public readonly int level;
        public AspectImprintPlan(AccountSave account,Item item,string aspect)
        {heroId=account.Hero.id;itemId=item?.id;aspectId=aspect;level=AspectStone.Level(account,aspect);fingerprint=item==null?"":JsonUtility.ToJson(item);}
        public bool Apply(AccountSave account)
        {
            if(!ContentUnlocks.Has(account,ContentUnlocks.Aspect)||account.suspendedRun!=null||account.Hero.id!=heroId||AspectStone.Definition(aspectId)==null||level<1||level!=AspectStone.Level(account,aspectId))return false;
            var item=account.Hero.inventory.Find(i=>i.id==itemId);
            if(!AspectStone.Eligible(item)||!item.equipped||JsonUtility.ToJson(item)!=fingerprint||AspectStone.PowerId(item)==aspectId&&AspectStone.PowerLevel(item)>=level)return false;
            item.rarity=3;item.aspectId=aspectId;item.aspectLevel=level;
            ItemCatalog.Validate(item);return true;
        }
    }
    public sealed partial class GameStore
    {
        public bool UpgradeAspect(string request,string id,int expectedLevel)
            =>Transact(request,"aspect-upgrade:"+id+":"+expectedLevel,a=>
            {
                if(a.suspendedRun!=null||!AspectStone.CanUpgrade(a,id)||AspectStone.Level(a,id)!=expectedLevel)return false;
                AspectStone.Progress(a,id).level++;return true;
            });
        public bool ImprintAspect(AspectImprintPlan plan)=>plan!=null&&Transact(plan.requestId,"aspect-imprint:"+plan.heroId+":"+plan.itemId+":"+plan.aspectId+":"+plan.level+":"+plan.fingerprint,plan.Apply);
    }
}
