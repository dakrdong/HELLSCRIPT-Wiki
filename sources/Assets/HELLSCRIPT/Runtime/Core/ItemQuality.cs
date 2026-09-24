using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    public static class ItemQuality
    {
        public const int Version=1,ItemVersion=4,MaximumItemLevel=60,MaximumMasterwork=200;
        public static float AwakeningChance(int stage)=>Mathf.Clamp(.02f*((long)stage-19),0,.5f);
        public static bool RollAwakening(int rarity,int stage,ref uint rng)
            =>rarity>=2&&stage>=20&&RandomStream.Unit(ref rng)<AwakeningChance(stage);
        public static void RollGreater(Item item,AffixRoll roll,ref uint rng)
        {
            if(!item.awakened||RandomStream.Unit(ref rng)>=.1f)return;
            roll.greater=true;roll.rollBasisPoints=10000;roll.tierId="T1";
            roll.value=ItemCatalog.Affix(roll.affixId).Value(item.level,10000);
        }
        public static int MasterworkCap(int highestClear)=>(int)Math.Min(MaximumMasterwork,12+3L*Math.Max(0L,(long)highestClear-30));
        public static int MasterworkCap(AccountSave account)=>MasterworkCap(ContentUnlocks.AccountClear(account));
        public static float MainMultiplier(Item item)=>(item.awakened?1.25f:1)*Mathf.Pow(1.02f,item.masterwork);
        public static int LineHits(Item item,string slotId)=>item.masterworkLines?.Count(id=>id==slotId)??0;
        public static float LineMultiplier(Item item,AffixRoll roll)=>(roll.greater?1.5f:1)*(1+.25f*LineHits(item,roll.slotId));
        // The stored roll remains the pre-quality value. Both layers are derived exactly once.
        public static float AffixValue(Item item,AffixRoll roll)=>roll.value*LineMultiplier(item,roll);
        public static int GreaterCount(Item item)=>item.rolls?.Count(r=>r!=null&&r.greater)??0;
        public static bool HasQuality(Item item)=>item.awakened||item.masterwork!=0||item.masterworkInvestedMaterials!=0||
            (item.masterworkLines?.Count??0)>0||GreaterCount(item)>0;
        public static string Name(Item item,string name)=>item.awakened?Loc.F("[각성] {0}",name):name;
        public static string Summary(Item item)=>HasQuality(item)?Loc.F("상위 접사 {0}개 · 걸작 {1}단계",GreaterCount(item),item.masterwork):"";
        public static int NextMilestone(int level)=>level<4?4:level<8?8:level<12?12:0;
        public static int Materials(int nextLevel)=>20+2*nextLevel;
        public static int Gold(int nextLevel)=>200*nextLevel;
        public static long ResetGold(Item item)=>2000L*Math.Max(1,item.level);
        public static int ResetRefund(Item item)=>(int)(item.masterworkInvestedMaterials*4L/5);
        public static string MasterworkError(AccountSave account,Item item)
        {
            if(item==null||!account.heroes.Any(h=>h.inventory.Contains(item)))return "보유한 장비만 걸작을 진행할 수 있습니다.";
            if(account.suspendedRun!=null)return "균열을 완료하고 성소에서 걸작을 진행해 주세요.";
            if(!ContentUnlocks.Has(account,ContentUnlocks.Masterwork))return ContentUnlocks.Condition(ContentUnlocks.Masterwork);
            if(item.enhancement<5)return "강화 +5 이상 장비만 걸작을 진행할 수 있습니다.";
            if(item.masterwork<0||item.masterwork>MaximumMasterwork)return "장비 품질 기록을 확인해 주세요.";
            if(item.masterwork>=MasterworkCap(account))return "현재 걸작 상한에 도달했습니다. 더 높은 균열을 실제로 클리어하면 상한이 열립니다.";
            int next=item.masterwork+1;
            if(account.materials<Materials(next)||account.gold<Gold(next))return "걸작에 필요한 재료 또는 골드가 부족합니다.";
            if(item.investedMaterials>int.MaxValue-Materials(next)||item.masterworkInvestedMaterials>int.MaxValue-Materials(next))return "장비의 투자 기록 한도를 넘을 수 없습니다.";
            return "";
        }
        public static string ResetError(AccountSave account,Item item)
        {
            if(item==null||!account.heroes.Any(h=>h.inventory.Contains(item)))return "보유한 장비만 걸작을 초기화할 수 있습니다.";
            if(account.suspendedRun!=null)return "균열을 완료하고 성소에서 걸작을 진행해 주세요.";
            if(item.masterwork==0&&item.masterworkInvestedMaterials==0)return "초기화할 걸작 기록이 없습니다.";
            if(item.masterworkInvestedMaterials<0||item.masterworkInvestedMaterials>item.investedMaterials)return "장비 품질 기록을 확인해 주세요.";
            if(account.gold<ResetGold(item))return "걸작 초기화에 필요한 골드가 부족합니다.";
            if((long)account.materials+ResetRefund(item)>int.MaxValue)return "회수할 재료가 보유 한도를 넘습니다.";
            return "";
        }
        public static bool Advance(AccountSave account,Item item,ref uint rng)
        {
            if(MasterworkError(account,item)!="")return false;
            int next=item.masterwork+1,mats=Materials(next);uint random=rng;
            string hit=null;if(next<=12&&next%4==0&&item.rolls.Count>0)hit=item.rolls[RandomStream.Range(ref random,0,item.rolls.Count)].slotId;
            account.materials-=mats;account.gold-=Gold(next);item.investedMaterials+=mats;item.masterworkInvestedMaterials+=mats;
            item.masterwork=next;item.masterworkLines??=new List<string>();if(hit!=null)item.masterworkLines.Add(hit);
            item.contentVersion=Math.Max(ItemVersion,item.contentVersion);rng=random;return true;
        }
        public static bool Reset(AccountSave account,Item item)
        {
            if(ResetError(account,item)!="")return false;
            account.gold-=(int)ResetGold(item);account.materials+=ResetRefund(item);
            item.investedMaterials-=item.masterworkInvestedMaterials;item.masterworkInvestedMaterials=0;item.masterwork=0;
            item.masterworkLines??=new List<string>();item.masterworkLines.Clear();return true;
        }
        public static void Validate(Item item)
        {
            if(item.masterwork<0||item.masterwork>MaximumMasterwork||item.masterwork>0&&item.enhancement<5||
                item.masterworkInvestedMaterials<0||item.masterworkInvestedMaterials>item.investedMaterials)
                throw new InvalidOperationException("장비 품질 기록을 확인해 주세요.");
            if(item.awakened&&(item.rarity<2||item.rolls.Any(r=>r!=null&&r.rollBasisPoints<4000)))
                throw new InvalidOperationException("각성 장비의 등급 또는 접사 품질이 올바르지 않습니다.");
            if((item.masterworkLines?.Count??0)>Math.Min(3,item.masterwork/4)||
                (item.masterworkLines?.Any(id=>!item.rolls.Any(r=>r!=null&&r.slotId==id))??false))
                throw new InvalidOperationException("걸작 접사 강화 기록이 올바르지 않습니다.");
            if(item.rolls.Any(r=>r!=null&&r.greater&&(!item.awakened||r.rollBasisPoints!=10000||r.legacyRoll)))
                throw new InvalidOperationException("상위 접사 기록이 올바르지 않습니다.");
        }
        public static bool RepairValues(Item item)
        {
            item.masterworkLines??=new List<string>();bool changed=false;
            if(item.masterwork<0||item.masterwork>MaximumMasterwork||item.masterwork>0&&item.enhancement<5)
            {item.masterwork=0;changed=true;}
            if(item.masterworkInvestedMaterials<0||item.masterworkInvestedMaterials>item.investedMaterials)
            {item.masterworkInvestedMaterials=0;changed=true;}
            // Preserve raw affix values; an invalid awakening loses its extra quality instead of granting a free reroll.
            if(item.awakened&&(item.rarity<2||(item.rolls?.Any(r=>r!=null&&r.rollBasisPoints<4000)??false)))
            {item.awakened=false;changed=true;}
            foreach(var roll in item.rolls??new List<AffixRoll>())
                if(roll!=null&&roll.greater&&(!item.awakened||roll.rollBasisPoints!=10000||roll.legacyRoll))
                {roll.greater=false;roll.rollBasisPoints=Mathf.Clamp(roll.rollBasisPoints,0,10000);roll.tierId=ItemGenerator.Tier(roll.rollBasisPoints);changed=true;}
            int allowed=Math.Min(3,item.masterwork/4);
            var valid=item.masterworkLines.Where(id=>!string.IsNullOrEmpty(id)&&(item.rolls?.Any(r=>r!=null&&r.slotId==id)??false)).Take(allowed).ToList();
            if(!item.masterworkLines.SequenceEqual(valid)){item.masterworkLines=valid;changed=true;}
            if(HasQuality(item))item.contentVersion=Math.Max(ItemVersion,item.contentVersion);
            return changed;
        }
    }
}
