using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    // Dialogs of the core screen in the casual mockup: equipment detail (a centred card, a sheet in portrait), the crafting result and the history (full window).
    // The result must be confirmed; confirming it is what acknowledges the pending craft.
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
        // The shared dialog frame with the names and close behaviour the core dialogs always had.
        void WireCoreDialog(RectTransform card)
        {
            var outside=card.parent.GetComponent<Button>();outside.name="core-dialog-outside";outside.onClick.RemoveAllListeners();outside.onClick.AddListener(CloseCoreDialog);
            var close=card.GetComponentsInChildren<UiButton>().FirstOrDefault(b=>b.name=="forge-dialog-close");if(close!=null){close.name="core-dialog-close";close.onClick.RemoveAllListeners();close.onClick.AddListener(CloseCoreDialog);}
        }

        // ---- equipment detail
        void CoreDetailDialog()
        {
            var definition=CoreCrafting.Definition(coreInspectId,CoreCrafting.Level(Hero));if(definition==null){Dismiss();return;}
            var card=ShowModal("CORE CRAFTING / EQUIPMENT","장비 상세 정보",landscape&&!Wide?48:46,(parent,w)=>
            {
                float y=AtBodyUnit(()=>CoreDetailBody(parent,w,definition));
                y+=BE(.8f);y+=Para(parent,"각 옵션 수치는 제작할 때 따로 결정됩니다.",0,y,w,.82f*U*.92f,1.6f,UiTheme.MockMutedInk);return y;
            },1,(footer,fw)=>FooterButtons(footer,fw,null,null,"닫기",CloseCoreDialog,"","core-dialog-confirm"),sheet:true);
            WireCoreDialog(card);
        }
        // Two columns (picture and power | table) on a wide window, one column in portrait. Runs at the body unit.
        float CoreDetailBody(Transform parent,float w,Item item)
        {
            if(!landscape)return CoreEquipmentBlock(parent,item,0,0,w,false,5f,false);
            float gap=E(1.4f),left=(w-gap)*.9f/2.15f,right=w-gap-left,top=E(.1f);
            float showcase=CoreShowcase(parent,item,5.5f,top+5.5f,left-11,Wide?8f:5f,true)+5.5f;float power=CorePower(parent,item,0,top+showcase+E(.9f),left);
            float table=CoreTable(parent,item,left+gap,top,right,false,false);return Mathf.Max(top+showcase+E(.9f)+power,top+table);
        }

        // ---- the crafting result (full window)
        void CoreResultDialog()
        {
            var record=store.Data.coreCraft.history.FirstOrDefault(r=>r.id==coreResultId);if(record==null){Dismiss();return;}
            bool pending=store.Data.coreCraft.pendingId==coreResultId;
            var card=ShowModal("CORE CRAFTING / RESULT",pending?"제작 완료":"제작 기록",0,(parent,w)=>AtBodyUnit(()=>CoreResultBody(parent,w,record)),1,
                (footer,fw)=>FooterButtons(footer,fw,null,null,"확인",CloseCoreDialog,"","core-dialog-confirm",true,false,0,null,null,null,landscape?26f:100f),dismissible:false);
            WireCoreDialog(card);
        }
        float CoreResultBody(Transform parent,float w,CoreCraftRecord record)
        {
            bool phone=landscape&&!Wide,portrait=!landscape;float total=portrait?w:Mathf.Min(w,E(60f)),x0=(w-total)/2,g=E(K(1.6f,1f,1f)),left=portrait?total:(total-g)*1.3f/2.3f,right=portrait?total:total-g-left;
            var probe=Rect("Body probe",canvasRoot);Place(probe,0,0,left,10);float leftH=CoreEquipmentBlock(probe,record.item,0,0,left,true,K(8f,4.4f,4f),false);probe.gameObject.SetActive(false);Destroy(probe.gameObject);
            // the aside: what the craft did (.core-result-aside)
            float headS=K(1.1f,.95f,1.1f)*U,textS=.84f*U,labelS=.82f*U,valS=K(1.7f,1.3f,1.7f)*U,pad=E(K(1.2f,.6f,.8f)),padX=E(K(1f,.8f,.8f)),inner=right-padX*2-6,gap=E(K(.7f,.3f,.7f));
            string stored=Loc.T("제작한 장비가 제작 기록에 저장되었습니다."),used=Loc.F("코어 10개 · 심연 주화 {0:N0}개 사용",record.investment),independent=Loc.T("모든 옵션이 표시된 구간에서 각각 추첨됩니다.");
            float seal=Wide||portrait&&false?E(4f)*1.414f:0,storedH=ParaHeight(stored,inner,headS,1.5f),usedH=ParaHeight(used,inner,textS,1.6f),intervalH=E(K(.8f,.4f,.8f))*2+labelS*1.45f+valS*1.15f+E(.3f)*2;
            float indepH=phone?0:ParaHeight(independent,inner,textS,1.6f);
            float asideH=6+pad*2+(seal>0?seal+gap+E(.8f):0)+storedH+gap+usedH+gap+intervalH+(indepH>0?gap+indepH:0);
            float rowH=portrait?leftH+E(1):Mathf.Max(leftH,asideH),ly=portrait?0:(rowH-leftH)/2,ay=portrait?leftH+E(1):(rowH-asideH)/2,ax=portrait?x0:x0+left+g;
            CoreEquipmentBlock(parent,record.item,x0,ly,left,true,K(8f,4.4f,4f),false);
            var box=Card(parent,"Core result aside",ax,ay,right,asideH,Color.white,skin.Navy,16,3,4,UiTheme.MockCheckOn);float y=3+pad;
            if(seal>0)
            {
                float s=E(4f);Diamond(box,"Success seal",right/2,y+seal/2,s,UiTheme.MockBarClaimed,UiTheme.MockBarReady,skin.Navy,14,3,3);Icon(box,"check",right/2-E(.9f),y+seal/2-E(.9f),E(1.8f),Color.white);y+=seal+E(.8f);
            }
            T(box,stored,3+padX,y,inner,storedH,headS,Hex("2a8a36"),400,true,TextAnchor.MiddleCenter);
            y+=storedH+gap;Para(box,used,3+padX,y,inner,textS,1.6f,UiTheme.MockMutedInk,700,TextAnchor.UpperCenter);y+=usedH+gap;
            Card(box,"Interval rule",3+padX,y,inner,2,DashRule,Color.clear,0,0,0);
            T(box,"옵션별 추첨 구간",3+padX,y+E(K(.8f,.4f,.8f)),inner,labelS*1.45f,labelS,UiTheme.MockMutedInk,700,false,TextAnchor.MiddleCenter);
            T(box,(record.investment/100d).ToString("0.##")+"% ~ 100%",3+padX,y+E(K(.8f,.4f,.8f))+labelS*1.45f+E(.3f),inner,valS*1.15f,valS,UiTheme.MockOrange,400,true,TextAnchor.MiddleCenter,true);
            Card(box,"Interval rule",3+padX,y+intervalH-2,inner,2,DashRule,Color.clear,0,0,0);y+=intervalH;
            if(indepH>0){y+=gap;Para(box,independent,3+padX,y,inner,textS,1.6f,UiTheme.MockMutedInk,700,TextAnchor.UpperCenter);}
            return (portrait?ay+asideH:rowH)+E(.5f);
        }

        // ---- the crafting history (full window)
        void CoreHistoryDialog()
        {
            var card=ShowModal("CORE CRAFTING / HISTORY","제작 기록",0,(parent,w)=>
            {
                var records=store.Data.coreCraft.history.AsEnumerable().Reverse().ToArray();
                if(records.Length==0){T(parent,"아직 제작한 장비가 없습니다.",0,BE(1.2f),w,BE(3),.9f*U*.92f,UiTheme.MockMutedInk,700,false,TextAnchor.MiddleCenter);return BE(5f);}
                return AtBodyUnit(()=>
                {
                    int columns=K(3,2,1)>2.5f?3:K(3,2,1)>1.5f?2:1;float g=E(.5f),cell=(w-g*(columns-1))/columns,row=E(4.5f),art=E(3.4f);
                    for(int n=0;n<records.Length;n++)
                    {
                        var record=records[n];string id=record.id;float x=n%columns*(cell+g),y=n/columns*(row+g);
                        var card2=Card(parent,"core-record-"+id,x,y,cell,row,Color.white,skin.Edge,12,2,3);Press(card2,()=>{coreResultId=id;dialogKind="core-result";Repaint();});
                        Color rarity=EquipmentGradePalette.For(record.item);float ax=2+E(.5f);Disc(card2,"Art ring",ax-2,(row-art)/2-2,art+4,rarity);var plate=Card(card2,"Art plate",ax,(row-art)/2,art,art,UiTheme.MockStrip,Color.clear,10,0,0);
                        EquipmentSlotView.Icon(plate,record.item,art*.09f,art*.09f,art*.82f,skin:skin);float tx=ax+art+E(.7f),tw=cell-tx-E(2.4f),l1=E(.92f)*1.4f,l2=E(.76f)*1.45f,ty=(row-l1-l2)/2;
                        T(card2,record.item.DisplayName,tx,ty,tw,l1,.92f*U,RarityInk(record.item),900);T(card2,Loc.F("코어 10개 · 심연 주화 {0:N0}개 사용",record.investment),tx,ty+l1,tw,l2,.76f*U,UiTheme.MockMutedInk,700,false,TextAnchor.MiddleLeft,true);
                        SlotIconRot(card2,"chevron",cell-E(.7f)-E(1.1f)-2,(row-E(1.1f))/2,E(1.1f),skin.Edge,90);
                    }
                    return (records.Length+columns-1)/columns*(row+g)-g;
                });
            });
            WireCoreDialog(card);
        }
    }
}
