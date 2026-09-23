using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed partial class BlacksmithWindow
    {
        void CloseCoreDialog()
        {
            if(dialogKind=="core-result"&&store.Data.coreCraft.pendingId==coreResultId)
            {
                if(!store.AcknowledgeCoreCraft(coreResultId)){notice=store.Error;noticeUntil=Time.unscaledTime+3;Repaint();return;}
                corePage=0;coreRecipeId=null;coreRequest=null;coreInvestment=0;
            }
            Dismiss();Repaint();
        }
        void RenderCoreDialog()
        {
            var backdrop=Panel(overlays,"Core dialog backdrop","080b08e8","080b08e8");Stretch(backdrop);
            var outside=backdrop.gameObject.AddComponent<Button>();outside.targetGraphic=backdrop.GetComponent<Image>();outside.transition=Selectable.Transition.None;outside.onClick.AddListener(CloseCoreDialog);outside.name="core-dialog-outside";
            float w=Mathf.Min(width-32,620),h=Mathf.Min(height-28,820);
            dialog=Panel(backdrop,"Core equipment dialog",UiTheme.ItemTopHex,UiTheme.ItemBottomHex,UiTheme.ItemRuleHex);Place(dialog,(width-w)/2,(height-h)/2,w,h);
            // Stop pointer clicks on blank card space from reaching the outside close target.
            var blocker=dialog.gameObject.AddComponent<Button>();blocker.transition=Selectable.Transition.None;blocker.targetGraphic=dialog.GetComponent<Image>();
            string title=dialogKind=="core-result"?"제작 완료":dialogKind=="core-history"?"제작 기록":dialogKind=="core-classes"?"직업":"장비 상세";
            Txt(dialog,title,16,10,w-78,35,20,gold);Btn(dialog,"×",w-53,10,37,35,CloseCoreDialog,false,22).name="core-dialog-close";Rule(dialog,57,w);
            bool result=dialogKind=="core-result";float top=result?94:68;
            if(dialogKind=="core-classes")
            {
                var classes=new[]{"모든 직업","전사","궁수","마법사"};
                for(int n=0;n<classes.Length;n++)
                {int selected=n-1;Btn(dialog,classes[n],16,top+n*48,w-32,41,()=>{coreClass=selected;ResetCoreCatalogueScroll();Dismiss();Repaint();},coreClass==selected,14).name="core-class-"+n;}
            }
            else if(dialogKind=="core-history")
            {
                Scroll(dialog,"Craft history",8,top,w-16,h-top-61,out var list);float y=0;
                foreach(var record in store.Data.coreCraft.history.AsEnumerable().Reverse())
                {
                    string id=record.id;var row=Btn(list,"",5,y,w-38,72,()=>{coreResultId=id;dialogKind="core-result";Repaint();});row.name="core-record-"+id;
                    EquipmentSlotView.Create(row.transform,record.item,5,7,UiTheme.SlotSize(landscape),font);
                    Txt(row.transform,record.item.DisplayName,71,4,w-118,31,13,EquipmentGradePalette.For(record.item));
                    Txt(row.transform,Loc.F("최소 품질 {0:0.##}% · 심연 주화 {1:N0}개",record.investment/100d,record.investment),71,36,w-118,29,10,muted);y+=79;
                }
                if(y==0){Txt(list,"아직 제작한 장비가 없습니다.",12,20,w-48,60,13,muted);y=90;}list.sizeDelta=new Vector2(0,y);
            }
            else if(result)
            {
                var record=store.Data.coreCraft.history.FirstOrDefault(r=>r.id==coreResultId);
                if(record!=null)
                {
                    Txt(dialog,Loc.F("최소 품질 {0:0.##}% · 심연 주화 {1:N0}개",record.investment/100d,record.investment),16,61,w-32,26,11,pale);
                    var host=Rect("Crafted item detail",dialog);Place(host,10,top,w-20,h-top-61);
                    ItemDetailView.Create(host,record.item,EquipmentViewSource.RewardSnapshot,font,textScale);
                }
            }
            else
            {
                var definition=CoreCrafting.Definition(coreInspectId,CoreCrafting.Level(Hero));
                if(definition!=null)
                {
                    Scroll(dialog,"Catalogue item detail",8,top,w-16,h-top-61,out var content);
                    var shared=ItemDetailView.AppendCatalog(content,w-26,()=>ItemTooltip.CatalogRanges(definition),font,textScale,definition);
                    content.sizeDelta=new Vector2(0,shared.rect.height+8);
                }
            }
            Btn(dialog,result?"확인":"닫기",16,h-49,w-32,37,CloseCoreDialog,result,13).name="core-dialog-confirm";
        }
    }
}
