using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    // Dialogs of the core screen: equipment detail (a centred card, a sheet in portrait), the crafting result and the history
    // (full window, as in the HTML). The result must be confirmed; confirming it is what acknowledges the pending craft.
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
            if(dialogKind=="core-result")CoreResultDialog();else if(dialogKind=="core-history")CoreHistoryDialog();else CoreDetailDialog();
        }

        // ---- equipment detail --------------------------------------------------------------------------------------------------
        void CoreDetailDialog()
        {
            var definition=CoreCrafting.Definition(coreInspectId,CoreCrafting.Level(Hero));if(definition==null){Dismiss();return;}
            var card=ShowModal("CORE CRAFTING / EQUIPMENT","장비 상세",coreLook==CoreLook.Compact?48:46,(parent,w)=>
            {
                var shared=ItemDetailView.AppendCatalog(parent,w,()=>ItemTooltip.CatalogRanges(definition),font,textScale,definition);
                float y=shared.rect.height+E(.8f),h=TextHeight(Loc.T("각 옵션 수치는 제작할 때 따로 결정됩니다."),w,Z(.82f));
                Txt(parent,"각 옵션 수치는 제작할 때 따로 결정됩니다.",0,y,w,h,Z(.82f),muted,TextAnchor.UpperLeft);return y+h;
            },E(2.8f),(footer,fw)=>FooterButtons(footer,fw,null,null,"닫기",CloseCoreDialog,"","core-dialog-confirm"),sheet:true);
            var outside=card.parent.GetComponent<Button>();outside.name="core-dialog-outside";outside.onClick.RemoveAllListeners();outside.onClick.AddListener(CloseCoreDialog);
            var close=card.GetComponentsInChildren<Button>().First(b=>b.name=="forge-dialog-close");close.name="core-dialog-close";close.onClick.RemoveAllListeners();close.onClick.AddListener(CloseCoreDialog);
        }

        // ---- full-window dialogs ------------------------------------------------------------------------------------------------
        // Header with an optional close, a scrolling body and an optional confirm button (HTML .modal.pool-fullscreen).
        void CoreFullDialog(string eyebrow,string title,bool closable,Func<Transform,float,float> drawBody,bool confirm)
        {
            var scrim=Panel(overlays,"Core dialog backdrop",UiTheme.VoidHex,UiTheme.VoidHex);Stretch(scrim);
            var outside=scrim.gameObject.AddComponent<Button>();outside.transition=Selectable.Transition.None;outside.targetGraphic=scrim.GetComponent<Image>();outside.name="core-dialog-outside";
            var card=Panel(scrim,"Core full screen dialog",UiTheme.RaisedHex,UiTheme.PanelHex);Stretch(card);dialog=card;
            float pad=E(1.1f),headerH=E(3.6f),footerH=confirm?E(2.8f)+pad*1.4f:0;
            Txt(card,eyebrow,pad,E(.75f),width-pad*2-E(3),E(1),Z(.66f),gold);Bold(Txt(card,title,pad,E(1.6f),width-pad*2-E(3),E(1.7f),Z(1.2f),UiTheme.GoldBright));
            if(closable)UiIconButton.CreateGlyph(card,"core-dialog-close","close",width-E(.7f)-E(2.4f),E(.6f),E(2.4f),E(2.4f),CloseCoreDialog,false,muted);
            var rule=Panel(card,"Dialog rule",UiTheme.BorderHex,UiTheme.BorderHex);rule.GetComponent<Image>().raycastTarget=false;Place(rule,0,headerH,width,1);
            float bodyH=height-headerH-footerH-pad;List(card,"Dialog body",pad,headerH+pad*.5f,width-pad*2,bodyH,out var content);
            float used=drawBody(content,width-pad*2-6);content.sizeDelta=new Vector2(0,used+pad);
            if(!confirm)return;
            var line=Panel(card,"Footer rule",UiTheme.BorderHex,UiTheme.BorderHex);line.GetComponent<Image>().raycastTarget=false;Place(line,0,height-footerH,width,1);
            float bw=landscape?Mathf.Min(E(26),width-pad*2):width-pad*2;var ok=Btn(card,"확인",width-pad-bw,height-footerH+pad*.7f,bw,E(2.8f),CloseCoreDialog,true,Z(.94f));ok.name="core-dialog-confirm";
        }
        void CoreResultDialog()
        {
            var record=store.Data.coreCraft.history.FirstOrDefault(r=>r.id==coreResultId);if(record==null){Dismiss();return;}
            bool pending=store.Data.coreCraft.pendingId==coreResultId;
            CoreFullDialog("CORE CRAFTING / RESULT",pending?"제작 완료":"제작 기록",false,(parent,w)=>CoreResultBody(parent,w,record),true);
        }
        float CoreResultBody(Transform parent,float w,CoreCraftRecord record)
        {
            bool roomy=coreLook==CoreLook.Roomy,portrait=coreLook==CoreLook.Portrait;
            float total=portrait?w:Mathf.Min(w,E(60)),x0=(w-total)/2,g=E(CoreV(1f,1.6f,1f)),left=portrait?total:(total-g)*1.3f/2.3f,right=portrait?total:total-g-left;
            var probe=Rect("Body probe",canvasRoot);Place(probe,0,0,left,10);float leftH=CoreEquipmentBlock(probe,record.item,0,0,left,true);probe.gameObject.SetActive(false);Destroy(probe.gameObject);
            // aside: what the craft did (HTML .core-result-aside)
            int headSize=Z(CoreV(.95f,1.1f,1f)),textSize=Z(.84f),labelSize=Z(.82f),valueSize=Z(CoreV(1.3f,1.7f,1.5f));float pad=E(CoreV(.6f,1.2f,.8f)),inner=right-pad*2,gap=E(CoreV(.3f,.7f,.6f));
            string stored=Loc.T("제작한 장비가 제작 기록에 저장되었습니다."),used=Loc.F("코어 10개 · 심연 주화 {0:N0}개 사용",record.investment),independent=Loc.T("모든 옵션이 표시된 구간에서 각각 추첨됩니다.");
            float seal=roomy?E(4.6f):0,storedH=TextHeight(stored,inner,headSize,true),usedH=TextHeight(used,inner,textSize),intervalH=labelSize*1.5f+valueSize*1.4f+E(.8f),independentH=coreLook==CoreLook.Compact?0:TextHeight(independent,inner,textSize);
            float asideH=pad*2+seal+storedH+gap+usedH+gap+intervalH+(independentH>0?gap+independentH:0);
            float rowH=portrait?leftH+E(1):Mathf.Max(leftH,asideH);float ly=portrait?0:(rowH-leftH)/2,ay=portrait?leftH+E(1):(rowH-asideH)/2,ax=portrait?x0:x0+left+g;
            CoreEquipmentBlock(parent,record.item,x0,ly,left,true);
            var box=Panel(parent,"Core result aside",UiTheme.InsetHex,UiTheme.InsetHex,UiTheme.GoldDeepHex);Place(box,ax,ay,right,asideH);box.GetComponent<Image>().raycastTarget=false;
            float y=pad;
            if(roomy)
            {
                float s=E(2.8f);var ring=Panel(box,"Success seal",UiTheme.InsetHex,UiTheme.InsetHex,UiTheme.GoldHex);ring.pivot=new Vector2(.5f,.5f);ring.anchorMin=ring.anchorMax=new Vector2(0,1);ring.sizeDelta=new Vector2(s,s);
                ring.anchoredPosition=new Vector2(right/2,-(y+seal/2-E(.2f)));ring.localRotation=Quaternion.Euler(0,0,45);ring.GetComponent<Image>().raycastTarget=false;
                Icon(box,"check",right/2-s*.3f,y+seal/2-E(.2f)-s*.3f,s*.6f,gold);y+=seal;
            }
            Bold(Txt(box,stored,pad,y,inner,storedH,headSize,green,TextAnchor.UpperCenter));y+=storedH+gap;
            Txt(box,used,pad,y,inner,usedH,textSize,muted,TextAnchor.UpperCenter);y+=usedH+gap;
            var top=Panel(box,"Interval rule",UiTheme.BorderHex,UiTheme.BorderHex);top.GetComponent<Image>().raycastTarget=false;Place(top,pad,y,inner,1);
            Txt(box,"옵션별 추첨 구간",pad,y+E(.3f),inner,labelSize*1.5f,labelSize,muted,TextAnchor.MiddleCenter);
            Bold(Serif(Txt(box,(record.investment/100d).ToString("0.##")+"% ~ 100%",pad,y+E(.3f)+labelSize*1.5f,inner,valueSize*1.4f,valueSize,UiTheme.GoldBright,TextAnchor.MiddleCenter)));
            var bottom=Panel(box,"Interval rule",UiTheme.BorderHex,UiTheme.BorderHex);bottom.GetComponent<Image>().raycastTarget=false;Place(bottom,pad,y+intervalH-1,inner,1);y+=intervalH;
            if(independentH>0)Txt(box,independent,pad,y+gap,inner,independentH,textSize,muted,TextAnchor.UpperCenter);
            return (portrait?ay+asideH:rowH)+E(.5f);
        }
        void CoreHistoryDialog()
        {
            CoreFullDialog("CORE CRAFTING / HISTORY","제작 기록",true,(parent,w)=>
            {
                var records=store.Data.coreCraft.history.AsEnumerable().Reverse().ToArray();
                if(records.Length==0){Txt(parent,"아직 제작한 장비가 없습니다.",0,E(1.5f),w,E(3),Z(.9f),muted,TextAnchor.UpperCenter);return E(5);}
                int columns=(int)CoreV(2,3,1);float g=E(.5f),cell=(w-g*(columns-1))/columns,row=E(4.4f),art=E(3.4f);
                for(int n=0;n<records.Length;n++)
                {
                    var record=records[n];string id=record.id;float x=n%columns*(cell+g),y=n/columns*(row+g);
                    var b=Btn(parent,"",x,y,cell,row,()=>{coreResultId=id;dialogKind="core-result";Repaint();});b.name="core-record-"+id;UiTheme.Choice(b,false,false);
                    EquipmentSlotView.Create(b.transform,record.item,E(.5f),(row-art)/2,art,font);float tx=E(.5f)+art+E(.7f),tw=cell-tx-E(2f);
                    TwoLines(b.transform,record.item.DisplayName,Z(.92f),EquipmentGradePalette.For(record.item),Loc.F("코어 10개 · 심연 주화 {0:N0}개 사용",record.investment),Z(.76f),muted,tx,0,tw,row);
                    CoreChevron(b.transform,cell-E(.7f),row/2,E(1.1f));
                }
                return (records.Length+columns-1)/columns*(row+g);
            },false);
        }
    }
}
