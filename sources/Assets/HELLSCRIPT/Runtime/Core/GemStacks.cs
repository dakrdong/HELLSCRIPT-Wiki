using System;
using System.Collections.Generic;
using System.Linq;

namespace Hellscript
{
    // Storage arithmetic is independent of which account or character owns the container.
    // The eventual service must call this on GameStore.Transact's staged container.
    public static class GemStacks
    {
        public static void Validate(IReadOnlyList<GemStack> stacks,int capacity)
        {
            if(stacks==null||capacity<0||stacks.Count>capacity||stacks.Any(s=>s==null||!GemCatalog.Valid(s.gemId,s.tier)||s.count<1||s.count>GemCatalog.StackLimit))
                throw new ArgumentException(Loc.T("보석 묶음의 종류·단계·개수와 보관 공간을 확인하세요."));
        }
        public static long Count(IEnumerable<GemStack> stacks,string id,int tier)
            =>stacks.Where(s=>s.gemId==id&&s.tier==tier).Sum(s=>(long)s.count);
        static List<GemStack> Copy(IEnumerable<GemStack> stacks)
            =>stacks.Select(s=>new GemStack{gemId=s.gemId,tier=s.tier,count=s.count}).ToList();
        static bool ValidChange(GemStack s)=>s!=null&&GemCatalog.Valid(s.gemId,s.tier)&&s.count>0;
        static bool Take(List<GemStack> stacks,GemStack change)
        {
            if(Count(stacks,change.gemId,change.tier)<change.count)return false;
            int left=change.count;
            // Consume smaller overflow stacks first so fusion can reuse a freed slot.
            foreach(var stack in stacks.Where(s=>s.gemId==change.gemId&&s.tier==change.tier).OrderBy(s=>s.count).ToArray())
            {
                int taken=Math.Min(left,stack.count);stack.count-=taken;left-=taken;
                if(stack.count==0)stacks.Remove(stack);if(left==0)break;
            }
            return true;
        }
        static bool Add(List<GemStack> stacks,int capacity,GemStack change)
        {
            long room=(long)(capacity-stacks.Count)*GemCatalog.StackLimit+
                stacks.Where(s=>s.gemId==change.gemId&&s.tier==change.tier).Sum(s=>(long)GemCatalog.StackLimit-s.count);
            if(room<change.count)return false;
            int left=change.count;
            foreach(var stack in stacks.Where(s=>s.gemId==change.gemId&&s.tier==change.tier))
            {int added=Math.Min(left,GemCatalog.StackLimit-stack.count);stack.count+=added;left-=added;if(left==0)return true;}
            while(left>0){int added=Math.Min(left,GemCatalog.StackLimit);stacks.Add(new GemStack{gemId=change.gemId,tier=change.tier,count=added});left-=added;}
            return true;
        }
        public static bool TryExchange(List<GemStack> stacks,int capacity,IEnumerable<GemStack> consume,IEnumerable<GemStack> receive)
        {
            Validate(stacks,capacity);
            if(consume==null||receive==null)return false;
            var inputs=consume.ToArray();var outputs=receive.ToArray();
            if(inputs.Any(s=>!ValidChange(s))||outputs.Any(s=>!ValidChange(s)))return false;
            var staged=Copy(stacks);
            foreach(var input in inputs)if(!Take(staged,input))return false;
            foreach(var output in outputs)if(!Add(staged,capacity,output))return false;
            stacks.Clear();stacks.AddRange(staged);return true;
        }
        public static bool TryAdd(List<GemStack> stacks,int capacity,GemStack reward)
            =>TryExchange(stacks,capacity,Array.Empty<GemStack>(),new[]{reward});
        // Capacity is not monotonic: consuming the last source stack may make a larger batch fit.
        // Evaluate freed-stack boundaries instead of looping once for every gem.
        public static int ConversionCount(IReadOnlyList<GemStack> stacks,int capacity,string id,int tier,bool upgrade,GemBatch batch)
        {
            if(!Jeweler.ValidBatch(batch)||!GemCatalog.Valid(id,tier)||upgrade&&tier==6||!upgrade&&tier==1)return 0;
            int target=tier+(upgrade?1:-1),input=upgrade?5:1,output=upgrade?1:5;
            var sources=stacks.Where(s=>s.gemId==id&&s.tier==tier).OrderBy(s=>s.count).ToArray();
            long maximum=sources.Sum(s=>(long)s.count)/input;
            long room=(long)(capacity-stacks.Count)*GemCatalog.StackLimit+stacks.Where(s=>s.gemId==id&&s.tier==target).Sum(s=>(long)GemCatalog.StackLimit-s.count);
            if(batch!=GemBatch.All)
            {
                int count=(int)batch;if(count>maximum)return 0;
                long left=(long)count*input;foreach(var s in sources){if(left<s.count)break;left-=s.count;room+=GemCatalog.StackLimit;}
                return (long)count*output<=room?count:0;
            }
            long consumed=0,best=0;
            for(int freed=0;freed<=sources.Length;freed++)
            {
                long candidate=Math.Min(maximum,(room+(long)freed*GemCatalog.StackLimit)/output);
                if(candidate*input>=consumed)best=Math.Max(best,candidate);
                if(freed<sources.Length)consumed+=sources[freed].count;
            }
            return checked((int)best);
        }
        public static bool TryConvert(List<GemStack> stacks,int capacity,string id,int tier,bool upgrade,GemBatch batch)
        {
            Validate(stacks,capacity);int count=ConversionCount(stacks,capacity,id,tier,upgrade,batch);if(count==0)return false;
            return TryExchange(stacks,capacity,new[]{new GemStack{gemId=id,tier=tier,count=checked(count*(upgrade?5:1))}},
                new[]{new GemStack{gemId=id,tier=tier+(upgrade?1:-1),count=checked(count*(upgrade?1:5))}});
        }
        public static bool TryFuse(List<GemStack> stacks,int capacity,string id,int tier,ref int gold)
            =>TryConvert(stacks,capacity,id,tier,true,GemBatch.One);
    }
}
