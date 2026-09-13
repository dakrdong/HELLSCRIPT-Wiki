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
        public static bool TryFuse(List<GemStack> stacks,int capacity,string id,int tier,ref int gold)
        {
            if(!GemCatalog.Valid(id,tier)||tier>=GemCatalog.MaximumTier)return false;
            int cost=GemCatalog.FusionGold(tier+1);if(gold<cost)return false;
            if(!TryExchange(stacks,capacity,new[]{new GemStack{gemId=id,tier=tier,count=3}},new[]{new GemStack{gemId=id,tier=tier+1,count=1}}))return false;
            gold-=cost;return true;
        }
    }
}
