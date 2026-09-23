using System;
using System.Collections.Generic;
using System.Linq;

namespace Hellscript
{
    public enum PotionFallback { HigherGrade, LowerGrade, Newest, Oldest }
    [Serializable] public sealed class PotionSlot
    {
        public string id="";
        public PotionFallback fallback; // Legacy per-slot value, read only when migrating old saves.
    }
    [Serializable] public sealed class PotionBatch
    {
        public string id;
        public int count,sequence;
    }
    public static class PotionLoadout
    {
        public const int SlotCount=3;
        public static readonly string[] FallbackLabels={"높은 등급 물약 사용","낮은 등급 물약 사용","가장 최근에 얻은 물약 순서로 사용","가장 오래된 물약 순서로 사용"};
        public static PotionSlot[] Defaults(string utility="PU04")=>new[]{new PotionSlot{id="PH01"},new PotionSlot{id="PM01"},new PotionSlot{id=utility}};
        public static PotionSlot[] Copy(PotionSlot[] slots)=>slots.Select(s=>new PotionSlot{id=s.id,fallback=s.fallback}).ToArray();
        public static void Validate(PotionSlot[] slots)
        {
            if(slots==null||slots.Length==0)return; // Saves made before configurable slots retain their edict selection.
            if(slots.Length!=SlotCount||slots.Any(s=>s==null||!Enum.IsDefined(typeof(PotionFallback),s.fallback)))throw new ArgumentException("Invalid potion slots.");
            var ids=slots.Where(s=>!string.IsNullOrEmpty(s.id)).Select(s=>s.id).ToArray();
            if(ids.Distinct().Count()!=ids.Length)throw new ArgumentException("같은 물약은 여러 슬롯에 지정할 수 없습니다.");
            foreach(string id in ids)PotionCatalog.Get(id);
            if(ids.Select(PotionCatalog.Family).Distinct().Count()!=ids.Length)throw new ArgumentException(Loc.T("같은 종류의 물약은 여러 슬롯에 지정할 수 없습니다."));
        }
        public static string[] Resolve(PotionInventory stock,PotionSlot[] slots,IReadOnlyList<PotionDefinition> catalog=null)
        {
            catalog??=PotionCatalog.All;var fallback=stock.SharedFallback;
            var result=new string[SlotCount];var reserved=new HashSet<string>(slots.Where(s=>!string.IsNullOrEmpty(s.id)).Select(s=>s.id));
            for(int i=0;i<SlotCount;i++)
            {
                var slot=slots[i];if(string.IsNullOrEmpty(slot.id))continue;
                var assigned=catalog.Single(p=>p.id==slot.id);
                if(stock.Count(slot.id)>0){result[i]=slot.id;continue;}
                var candidates=catalog.Where(p=>p.effect==assigned.effect&&stock.Count(p.id)>0&&!reserved.Contains(p.id));
                IOrderedEnumerable<PotionDefinition> ordered=fallback==PotionFallback.HigherGrade?candidates.OrderByDescending(p=>p.grade):
                    fallback==PotionFallback.LowerGrade?candidates.OrderBy(p=>p.grade):
                    fallback==PotionFallback.Newest?candidates.OrderByDescending(p=>stock.Acquired(p.id,true)):
                    candidates.OrderBy(p=>stock.Acquired(p.id,false));
                var next=ordered.ThenBy(p=>stock.Acquired(p.id,false)).ThenBy(p=>p.id,StringComparer.Ordinal).FirstOrDefault();
                if(next!=null){result[i]=next.id;reserved.Add(next.id);}
            }
            return result;
        }
    }
    public sealed partial class PotionInventory
    {
        public PotionSlot[] slots;
        public PotionFallback fallback;
        public int fallbackVersion,familyVersion;
        // Resolving a preview is read-only. Older saves inherit the first assigned slot's rule once.
        public PotionFallback SharedFallback=>fallbackVersion>0?fallback:
            slots?.FirstOrDefault(s=>!string.IsNullOrEmpty(s?.id))?.fallback??PotionFallback.HigherGrade;
        public int acquisitionVersion,acquisitionSequence;
        public List<PotionBatch> batches=new List<PotionBatch>();
        void EnsureAcquisitions()
        {
            if(acquisitionVersion!=0)return;
            batches=new List<PotionBatch>();acquisitionSequence=0;
            // Old saves have no timestamps; retain their stored order once, then record each future receipt.
            foreach(var stack in stacks.Where(s=>s.count>0))batches.Add(new PotionBatch{id=stack.id,count=stack.count,sequence=++acquisitionSequence});
            acquisitionVersion=1;
        }
        void RecordStockChange(string id,int count)
        {
            EnsureAcquisitions();int delta=count-Count(id);
            if(delta>0)batches.Add(new PotionBatch{id=id,count=delta,sequence=checked(++acquisitionSequence)});
            else if(delta<0)RemoveBatches(id,-delta,false);
        }
        void RemoveBatches(string id,int count,bool newest)
        {
            var ordered=newest?batches.Where(b=>b.id==id).OrderByDescending(b=>b.sequence):batches.Where(b=>b.id==id).OrderBy(b=>b.sequence);
            foreach(var batch in ordered){int take=Math.Min(count,batch.count);batch.count-=take;count-=take;if(count==0)break;}
            if(count!=0)throw new InvalidOperationException("Potion acquisition ledger does not match stock.");
            batches.RemoveAll(b=>b.count==0);
        }
        public int Acquired(string id,bool newest)
        {
            // Rendering and previews must not migrate or write the inventory.
            if(acquisitionVersion==0)return stacks.FindIndex(s=>s.id==id)+1;
            var entries=batches.Where(b=>b.id==id&&b.count>0).Select(b=>b.sequence);
            return newest?entries.DefaultIfEmpty(0).Max():entries.DefaultIfEmpty(int.MaxValue).Min();
        }
        public void Consume(string id,bool newest=false)
        {
            if(Count(id)<=0)throw new InvalidOperationException("No potion stock.");
            EnsureAcquisitions();RemoveBatches(id,1,newest);stacks.Single(s=>s.id==id).count--;
        }
        void ValidateLoadout()
        {
            if(familyVersion<0||familyVersion>1)throw new NotSupportedException("Invalid potion family version.");
            if(familyVersion==0)
            {
                // Older saves could equip different grades of one gem. Preserve stock and the first slot.
                var used=new Dictionary<string,string>();
                if(slots?.Length==PotionLoadout.SlotCount&&slots.All(s=>s!=null))foreach(var slot in slots)
                {
                    if(string.IsNullOrEmpty(slot.id))continue;string family=PotionCatalog.Family(slot.id);
                    if(used.TryGetValue(family,out var first)&&first!=slot.id)slot.id="";
                    else used[family]=slot.id;
                }
                familyVersion=1;
            }
            PotionLoadout.Validate(slots);
            if(fallbackVersion<0||fallbackVersion>1)throw new NotSupportedException("Invalid potion fallback version.");
            if(fallbackVersion==0){fallback=SharedFallback;fallbackVersion=1;}
            if(!Enum.IsDefined(typeof(PotionFallback),fallback))throw new ArgumentException("Invalid potion fallback.");
            if(slots?.Length==0)slots=null;
            if(acquisitionVersion<0||acquisitionVersion>1||acquisitionSequence<0)throw new NotSupportedException("Invalid potion acquisition version.");
            EnsureAcquisitions();
            if(batches==null||batches.Any(b=>b==null||b.count<=0||b.sequence<=0||b.sequence>acquisitionSequence)||batches.Select(b=>b.sequence).Distinct().Count()!=batches.Count)
                throw new ArgumentException("Invalid potion acquisitions.");
            foreach(var batch in batches)PotionCatalog.Get(batch.id);
            if(stacks.Any(s=>batches.Where(b=>b.id==s.id).Sum(b=>b.count)!=s.count)||batches.Any(b=>!stacks.Any(s=>s.id==b.id)))throw new ArgumentException("Potion acquisitions do not match stock.");
        }
    }
}
