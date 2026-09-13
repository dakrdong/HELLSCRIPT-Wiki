using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed partial class GameUI
    {
        void QualityFrame(Transform target,Item item)
        {
            if(!item.awakened||target.GetComponent<Graphic>()==null)return;
            var outline=target.gameObject.AddComponent<Outline>();outline.effectColor=new Color(.62f,.78f,.87f);
            outline.effectDistance=new Vector2(2,-2);outline.useGraphicAlpha=false;
        }
        string QualityLine(Item item)=>ItemQuality.HasQuality(item)?"\n"+ItemQuality.Summary(item):"";
        void DescribeQualityMain(Transform parent,Item item,ItemBaseDefinition basis,string mainName)
        {
            InventoryNote(parent,ItemQuality.Summary(item),21,gold);
            InventoryNote(parent,Loc.F("{0}\n{1} {2:0.0}\n기본 {3:0.0} × 강화 {4:0.###} × 각성 {5:0.##} × 걸작 {6:0.###}",basis.name,mainName,ItemCatalog.MainValue(item),basis.main*(1+.08f*(item.level-1)),1+.05f*item.enhancement,item.awakened?1.25f:1,Mathf.Pow(1.02f,item.masterwork)));
            if(basis.resistance>0)InventoryNote(parent,Loc.F("고정 부가 저항 {0:0.0}",basis.resistance*(1+.08f*(item.level-1))),18,muted);
            if(basis.attackSpeed!=0)InventoryNote(parent,Loc.F("고정 공격속도 {0:+0%;-0%}",basis.attackSpeed),18,muted);
        }
        void DescribeQualityAffix(Transform parent,Item item,AffixRoll roll)
        {
            var def=ItemCatalog.Affix(roll.affixId);float factor=ItemQuality.LineMultiplier(item,roll);
            int minimum=roll.greater?10000:item.awakened?4000:0;
            InventoryNote(parent,Loc.F("{0}{1} · {2} [{3}]\n{4} +{5:0.##} · 가능 범위 {6:0.##}~{7:0.##}",roll.greater?Loc.T("◆ 상위 ·")+" ":"",roll.side==AffixSide.Prefix?"접두":"접미",def.phrase,roll.tierId,StatCatalog.Name(def.stat),ItemQuality.AffixValue(item,roll),def.Value(item.level,minimum)*factor,def.Value(item.level,10000)*factor),20,roll.greater?gold:pale);
            if(roll.greater)InventoryNote(parent,"최고치 고정 · 상위 접사 ×1.5",18,gold);
            int hits=ItemQuality.LineHits(item,roll.slotId);
            if(hits>0)InventoryNote(parent,Loc.F("걸작 접사 강화 {0}회 · ×{1:0.##}",hits,1+.25f*hits),18,setGreen);
            if(roll.legacyRoll)InventoryNote(parent,"기존 수치를 보존한 옵션입니다.",18,muted);
            if(item.rerollSlotId==roll.slotId)InventoryNote(parent,"재설정 대상으로 선택한 줄입니다.",18,muted);
        }
        void ShowInventoryMasterwork(string id)
        {
            var a=game.Store.Data;var item=FindOwned(a,id);if(item==null){RefreshInventory();return;}
            OpenInventoryTool("masterwork");var body=inventoryTool.content;
            InventoryNote(body,Loc.F("대장장이 · 걸작\n{0}",item.DisplayName),24,gold);
            InventoryNote(body,Loc.F("현재 {0}단계 / 상한 {1}단계\n계정 최고 실클리어 {2}단계 · 보유 재료 {3:N0} / 골드 {4:N0}",item.masterwork,ItemQuality.MasterworkCap(a),ContentUnlocks.AccountClear(a),a.materials,a.gold),22);
            InventoryNote(body,"단계마다 주 기본값이 2%씩 증가합니다. 4·8·12단계에는 무작위 접사 한 줄이 25%씩 강화되며, 13단계부터는 주 기본값만 오릅니다.");
            int milestone=ItemQuality.NextMilestone(item.masterwork);
            if(item.rolls.Count==0)InventoryNote(body,"이 장비에는 접사가 없어 주 기본값만 오릅니다.",20,gold);
            else if(milestone>0)InventoryNote(body,Loc.F("다음 접사 강화까지 {0}단계 · 어느 줄이 선택될지는 미리 알 수 없습니다.",milestone-item.masterwork),20,gold);
            foreach(var group in item.masterworkLines.GroupBy(slot=>slot))
            {
                var roll=item.rolls.Find(r=>r.slotId==group.Key);if(roll==null)continue;
                InventoryNote(body,Loc.F("{0} · 걸작 {1}회 · ×{2:0.##}",StatCatalog.Name(ItemCatalog.Affix(roll.affixId).stat),group.Count(),1+.25f*group.Count()),20,setGreen);
            }
            string error=ItemQuality.MasterworkError(a,item);int next=item.masterwork+1;
            if(item.masterwork<ItemQuality.MasterworkCap(a))
            {
                uint seed=(uint)DateTime.UtcNow.Ticks;
                string quote=Loc.F("걸작 {0}단계로 올립니다.\n재료 {1:N0}개와 골드 {2:N0}가 필요합니다.",next,ItemQuality.Materials(next),ItemQuality.Gold(next));
                if(next<=12&&next%4==0&&item.rolls.Count>0)quote+="\n"+Loc.T("이번 단계에는 무작위 접사 한 줄이 25% 강화됩니다.");
                var commit=InventoryTransaction("masterwork:"+id+":"+item.masterwork,staged=>
                {uint random=seed;return ItemQuality.Advance(staged,FindOwned(staged,id),ref random);},"걸작 단계를 올렸습니다.",item,()=>ShowInventoryMasterwork(id));
                var button=InventoryButton(body,Loc.F("걸작 {0}단계 · 재료 {1:N0} / 골드 {2:N0}",next,ItemQuality.Materials(next),ItemQuality.Gold(next)),()=>ShowInventoryConfirm(quote,commit),true);
                button.interactable=error==""&&!portalBag&&GearServiceAvailable;
            }
            if(error!="")InventoryNote(body,error,20,gold);
            if(item.masterwork>0||item.masterworkInvestedMaterials>0)
            {
                InventoryNote(body,Loc.F("걸작에 투자한 재료 {0:N0}개 · 초기화 시 {1:N0}개 회수",item.masterworkInvestedMaterials,ItemQuality.ResetRefund(item)),20,muted);
                string quote=Loc.F("걸작을 0단계로 초기화합니다.\n골드 {0:N0}를 내고 걸작 재료 {1:N0}개를 회수합니다.\n걸작 접사 강화는 사라집니다. 강화 +{2}, 각성, 상위 접사와 보석은 유지됩니다.",ItemQuality.ResetGold(item),ItemQuality.ResetRefund(item),item.enhancement);
                var reset=InventoryButton(body,Loc.F("걸작 초기화 · 골드 {0:N0}",ItemQuality.ResetGold(item)),()=>ShowInventoryConfirm(quote,InventoryTransaction("masterwork-reset:"+id+":"+item.masterwork,staged=>ItemQuality.Reset(staged,FindOwned(staged,id)),"걸작을 초기화하고 재료를 회수했습니다.",item,()=>ShowInventoryMasterwork(id))));
                string resetError=ItemQuality.ResetError(a,item);reset.interactable=resetError==""&&!portalBag&&GearServiceAvailable;
                if(resetError!="")InventoryNote(body,resetError,18,gold);
            }
            FooterButton(0,1,"장비 상세로",CloseInventoryTool);ReflowInventory();
        }
    }
}
