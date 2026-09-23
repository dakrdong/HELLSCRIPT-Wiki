using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    [Serializable] public sealed class CoreCraftRecord
    {
        public string id,heroId,recipeId;
        public int investment;
        public long createdUtc;
        public Item item;
    }
    [Serializable] public sealed class CoreCraftState
    {
        public int version=1;
        public string pendingId="";
        public List<CoreCraftRecord> history=new List<CoreCraftRecord>();
    }
    public sealed class CoreCraftQuote
    {
        public string heroId,recipeId,fingerprint,error;
        public int level,investment;
        public bool CanCraft=>string.IsNullOrEmpty(error);
    }
    // Recipes are definitions, not owned items. Stable affix identities make the preview binding.
    public static class CoreCrafting
    {
        public const int CoreCost=10,MaximumInvestment=8000;
        public static IReadOnlyList<UniqueItemDefinition> Recipes=>ItemCatalog.Uniques;
        public static int Level(HeroSave hero)=>Math.Clamp(hero.highestClear,1,ItemQuality.MaximumItemLevel);
        public static int InvestmentLimit(AccountSave account)=>Math.Clamp(account.premium,0,MaximumInvestment);
        public static void Normalize(AccountSave account)
        {
            account.coreCraft??=new CoreCraftState();var state=account.coreCraft;
            if(state.version>1)throw new NotSupportedException("Unsupported core crafting save version.");
            state.version=1;state.history??=new List<CoreCraftRecord>();state.pendingId??="";
        }
        public static void Validate(AccountSave account)
        {
            var s=account.coreCraft;
            if(s==null||s.version!=1||s.history==null||s.history.Any(r=>r==null||string.IsNullOrEmpty(r.id)||r.item==null||r.investment<0||r.investment>MaximumInvestment||!account.heroes.Any(h=>h.id==r.heroId))||s.history.Select(r=>r.id).Distinct().Count()!=s.history.Count||s.pendingId!=""&&!s.history.Any(r=>r.id==s.pendingId))
                throw new InvalidOperationException("Invalid core crafting history.");
            foreach(var record in s.history)ItemCatalog.Validate(record.item);
        }
        static uint Seed(string id)
        {uint seed=2166136261;foreach(char c in id)seed=unchecked(seed*31+c);return seed;}
        public static Item Definition(string recipeId,int level)
        {
            var recipe=ItemCatalog.Unique(recipeId);if(recipe==null)return null;
            var c=(HeroClass)Math.Max(0,recipe.heroClass);uint seed=Seed(recipeId);
            var bases=ItemCatalog.Bases.Where(b=>b.Fits(c,recipe.slot)&&!EquipmentSlots.IsOffhand(EquipmentSlots.Kind(b.id))).ToArray();
            var basis=bases[seed%(uint)bases.Length];
            var item=new Item{id="core-definition:"+recipeId,baseId=basis.id,baseIndex=basis.legacyIndex,special=recipeId,slot=recipe.slot,rarity=3,level=Math.Clamp(level,1,ItemQuality.MaximumItemLevel),lootClass=c,contentVersion=ItemCatalog.Version};
            var used=new HashSet<string>();
            foreach(var side in new[]{AffixSide.Prefix,AffixSide.Prefix,AffixSide.Suffix,AffixSide.Suffix})
            {
                var pool=ItemCatalog.Affixes.Where(a=>a.Allows(recipe.slot)&&a.side==side&&!used.Contains(a.group)).ToArray();
                seed=unchecked(seed*1664525+1013904223);double draw=seed/4294967296d*pool.Sum(a=>a.Weight(c));var chosen=pool.Last();
                foreach(var candidate in pool){draw-=candidate.Weight(c);if(draw<0){chosen=candidate;break;}}
                used.Add(chosen.group);item.rolls.Add(new AffixRoll{slotId=item.id+":a"+item.rolls.Count,affixId=chosen.id,side=side,tierId=ItemGenerator.Tier(0),rollBasisPoints=0,value=chosen.Value(item.level,0)});
            }
            item.name=item.DisplayName;return item;
        }
        static string Fingerprint(Item item)=>item==null?"":item.special+":"+item.baseId+":"+item.level+":"+ItemCatalog.Version+":"+string.Join(",",item.rolls.Select(r=>r.affixId));
        public static CoreCraftQuote Quote(AccountSave account,string recipeId,int investment)
        {
            var item=Definition(recipeId,Level(account.Hero));
            return new CoreCraftQuote{heroId=account.Hero.id,recipeId=recipeId,level=Level(account.Hero),investment=investment,fingerprint=Fingerprint(item),error=Error(account,item,investment)};
        }
        static string Error(AccountSave a,Item definition,int investment)
        {
            if(definition==null)return "제작할 장비를 선택해 주세요.";
            if(a.suspendedRun!=null)return "균열을 완료하고 대장간에서 제작해 주세요.";
            if(!ContentUnlocks.Has(a,ContentUnlocks.CoreCraft))return ContentUnlocks.Condition(ContentUnlocks.CoreCraft);
            if(!string.IsNullOrEmpty(a.coreCraft?.pendingId))return "이전 제작 결과를 먼저 확인해 주세요.";
            if(investment<0||investment>MaximumInvestment)return "심연 주화는 0~8,000개까지 사용할 수 있습니다.";
            if(a.cores[definition.slot]<CoreCost)return "해당 부위의 코어가 10개 필요합니다.";
            if(a.premium<investment)return "심연 주화가 부족합니다.";
            if(Economy.FreeSlots(a.Hero)<=0)return "가방에 장비를 받을 공간이 필요합니다.";
            return "";
        }
        public static string Check(AccountSave a,CoreCraftQuote quote)
        {
            if(quote==null||a.Hero.id!=quote.heroId)return "제작 정보가 바뀌었습니다. 다시 선택해 주세요.";
            var current=Quote(a,quote.recipeId,quote.investment);
            return current.level!=quote.level||current.fingerprint!=quote.fingerprint?"제작 정보가 바뀌었습니다. 다시 선택해 주세요.":current.error;
        }
        public static bool Apply(AccountSave account,CoreCraftQuote quote,string request,ref uint random)
        {
            if(Check(account,quote)!="")return false;
            var item=Definition(quote.recipeId,quote.level);item.id=Guid.NewGuid().ToString("N");item.acquisitionKind="craft";
            for(int n=0;n<item.rolls.Count;n++)
            {
                var roll=item.rolls[n];roll.slotId=item.id+":a"+n;
                roll.rollBasisPoints=ItemGenerator.UniformQuality(ref random,quote.investment);
                roll.tierId=ItemGenerator.Tier(roll.rollBasisPoints);roll.value=ItemCatalog.Affix(roll.affixId).Value(item.level,roll.rollBasisPoints);
            }
            ItemCatalog.Validate(item);
            if(!Economy.AddItem(account.Hero,item,BagPolicy.Ignore,account))return false;
            account.cores[item.slot]-=CoreCost;account.premium-=quote.investment;
            account.coreCraft.history.Add(new CoreCraftRecord{id=request,heroId=account.Hero.id,recipeId=quote.recipeId,investment=quote.investment,createdUtc=DateTimeOffset.UtcNow.ToUnixTimeSeconds(),item=RuneGrowth.Copy(item)});
            account.coreCraft.pendingId=request;return true;
        }
    }
}
