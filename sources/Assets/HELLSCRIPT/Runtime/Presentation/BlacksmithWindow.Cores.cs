using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    // 코어 제작 (HTML: .cores). Landscape: 01 cores | 02 equipment (prompt, catalogue, preview) | 03 quality, with the craft dock once a
    // recipe is chosen. Portrait: a three-page wizard (cores, catalogue, preview + quality) with a slide between pages.
    // Crafting is one GameStore transaction; drawing code only reads the save.
    public sealed partial class BlacksmithWindow
    {
        // How much room the window has: phone landscape (compact), taller landscape such as PC (roomy), portrait (a page at a time).
        enum CoreLook{Compact,Roomy,Portrait}
        int coreSlot,corePage,coreInvestment,coreGrade,coreClass=-1;
        string coreRecipeId,coreInspectId,coreResultId,coreQuery="",coreRequest;
        bool coreCommitting;Action coreRefresh;CoreLook coreLook;
        readonly Dictionary<string,float> coreScroll=new Dictionary<string,float>();
        public int CorePage=>corePage;public int CoreInvestment=>coreInvestment;public string CoreRecipeId=>coreRecipeId;
        Item CoreDefinition=>CoreCrafting.Definition(coreRecipeId,CoreCrafting.Level(Hero));
        float CoreV(float compact,float roomy,float portrait)=>coreLook==CoreLook.Compact?compact:coreLook==CoreLook.Roomy?roomy:portrait;
        string CoreName(int slot)=>Loc.F("{0} 코어",GameCatalog.Slots[slot]);
        void CoreHook(Action refresh){coreRefresh+=refresh;refresh();}
        void RememberCoreScroll()
        {
            if(tab!=3||body==null)return;
            foreach(var scroll in body.GetComponentsInChildren<ScrollRect>())
                if(scroll.name.StartsWith("Core ")&&scroll.content!=null)coreScroll[scroll.name]=scroll.content.anchoredPosition.y;
        }
        void RestoreCoreScroll()
        {
            if(tab!=3)return;Canvas.ForceUpdateCanvases();
            foreach(var scroll in body.GetComponentsInChildren<ScrollRect>())
                if(coreScroll.TryGetValue(scroll.name,out float offset))scroll.content.anchoredPosition=new Vector2(0,Mathf.Clamp(offset,0,Mathf.Max(0,scroll.content.rect.height-scroll.viewport.rect.height)));
        }
        void NavigateCore(int page)
        {int old=corePage;corePage=Math.Clamp(page,0,2);Dismiss();Repaint();AnimatePage(corePage<old?1:-1);}
        void ResetCoreCatalogueScroll()
        {
            coreScroll.Remove("Core recipe list");
            foreach(var scroll in body.GetComponentsInChildren<ScrollRect>())
                if(scroll.name=="Core recipe list")scroll.content.anchoredPosition=Vector2.zero;
        }
        void SelectCore(int slot)
        {coreSlot=slot;coreRecipeId=null;coreRequest=null;coreInvestment=0;corePage=0;coreQuery="";ResetCoreCatalogueScroll();Repaint();}
        void SelectCoreRecipe(string id)
        {coreRecipeId=id;coreRequest=null;NavigateCore(2);}

        // ---- screen -----------------------------------------------------------------------------------------------------------
        // Service screen entry point (below DrawChrome).
        void DrawCoresScreen(float y,float h)
        {
            coreLook=!landscape?CoreLook.Portrait:height/U>39f?CoreLook.Roomy:CoreLook.Compact;
            coreInvestment=Math.Clamp(coreInvestment,0,CoreCrafting.InvestmentLimit(store.Data));
            if(corePage==2&&CoreDefinition==null)corePage=1;
            bool chosen=corePage==2;float dockH=chosen?E(landscape?4.3f:5.2f):0;
            if(chosen)DrawCoreDock(dockH);
            if(landscape)
            {
                float pad=E(.55f),gap=E(.55f),sy=y+pad,sh=height-dockH-y-pad*2,inner=width-pad*2-gap*2,a=inner*.98f/3.02f,b=inner*1.14f/3.02f,c=inner-a-b;
                var stock=PanelBox(body,"Core stock",pad,sy,a,sh);Columns.Add(stock);DrawCoreStock(stock,a,sh);
                var middle=PanelBox(body,"Core equipment",pad+a+gap,sy,b,sh);Columns.Add(middle);
                if(chosen)DrawCorePreview(middle,b,sh);else if(corePage==1)DrawCoreCatalog(middle,b,sh);else DrawCoreEmpty(middle,b,sh);
                var quality=PanelBox(body,"Core quality",pad+a+b+gap*2,sy,c,sh);Columns.Add(quality);DrawCoreQuality(quality,c,sh,chosen);
                return;
            }
            {
                float pad=E(.7f),gap=E(.6f),navH=E(2.4f),top=y+pad;
                if(corePage>0){DrawCoreNav(pad,top,width-pad*2,navH);top+=navH+gap;}
                float availH=height-dockH-top-pad,pw=width-pad*2;
                var page=Rect("Core page "+corePage,body);Place(page,pad,top,pw,availH);Columns.Add(page);
                if(corePage==0){var stock=PanelBox(page,"Core stock",0,0,pw,availH);DrawCoreStock(stock,pw,availH);}
                else if(corePage==1){var catalogue=PanelBox(page,"Core equipment",0,0,pw,availH);DrawCoreCatalog(catalogue,pw,availH);}
                else
                {
                    var equipment=PanelBox(page,"Core equipment",0,0,pw,availH);float px=E(.7f),used=CoreEquipmentBlock(equipment,CoreDefinition,px,px,pw-px*2,false)+px*2;Place(equipment,0,0,pw,used);
                    float qh=availH-used-gap;var quality=PanelBox(page,"Core quality",0,used+gap,pw,qh);DrawCoreQuality(quality,pw,qh,true);
                }
            }
        }
        // Portrait pages B and C: back one page, and the crafting history.
        void DrawCoreNav(float x,float y,float w,float h)
        {
            string label=corePage==2?"장비 목록으로":"코어 목록으로";int size=Z(.9f);
            float icon=E(1.3f),tw=TextWidth(Loc.T(label),size,true)+E(.4f),bw=E(.4f)+icon+E(.5f)+tw+E(.5f);
            var back=CoreGhost(body,"core-back",x,y,bw,h,()=>NavigateCore(corePage-1));
            Icon(back.transform,"back",E(.4f),(h-icon)/2,icon,UiTheme.GoldBright);Bold(Txt(back.transform,label,E(.4f)+icon+E(.5f),0,tw,h,size,UiTheme.GoldBright));
            CoreHistoryButton(body,x+w,y+h/2);
        }
        // A button with no face of its own: a transparent hit area with the shared focus and click feedback.
        UiButton CoreGhost(Transform parent,string name,float x,float y,float w,float h,Action click,UiButtonRole role=UiButtonRole.Icon)
        {
            var r=Rect(name,parent);Place(r,x,y,w,h);var hit=r.gameObject.AddComponent<Image>();hit.color=Color.clear;
            var b=r.gameObject.AddComponent<UiButton>();b.targetGraphic=hit;b.Configure(role);b.onClick.AddListener(()=>click());return b;
        }
        // Button with a small icon in front of the label.
        Button CoreIconButton(Transform parent,string label,string symbol,float x,float y,float w,float h,Action click,bool primary,int size,Color iconColor)
        {
            var b=Btn(parent,label,x,y,w,h,click,primary,size);float tw=Mathf.Min(TextWidth(Loc.T(label),size,true),w-E(3)),icon=Mathf.Min(h*.5f,E(1.3f));
            Icon(b.transform,symbol,Mathf.Max(E(.4f),(w-tw)/2-icon-E(.4f)),(h-icon)/2,icon,iconColor);return b;
        }
        void CoreHistoryButton(Transform parent,float right,float centerY)
        {
            int size=Z(coreLook==CoreLook.Compact?.74f:.82f);string label=Loc.F("제작 기록 ({0})",store.Data.coreCraft.history.Count);
            float h=E(1.9f),icon=E(1.1f),tw=TextWidth(label,size)+E(.3f),bw=E(.6f)+icon+E(.4f)+tw+E(.6f);
            var b=Btn(parent,"",right-bw,centerY-h/2,bw,h,()=>OpenDialog("core-history"));b.name="core-history";UiTheme.Choice(b,false,true);
            Icon(b.transform,"history",E(.6f),(h-icon)/2,icon,muted);Txt(b.transform,label,E(.6f)+icon+E(.4f),0,tw,h,size,muted);
        }
        // A serif number with a small unit after it, right-aligned at `right` inside the band y..y+h (centred, or at the top of the band).
        Text CoreCount(Transform parent,string number,int size,Color color,string unit,int unitSize,float right,float y,float h,bool top=false,string name=null)
        {
            float uw=unit==null?0:TextWidth(unit,unitSize)+E(.25f),nw=TextWidth(number,size,true,true)+E(.15f),drop=(size-unitSize)*(top?.85f:.3f);
            var n=Bold(Serif(Txt(parent,number,right-uw-nw,y,nw,h,size,color,top?TextAnchor.UpperRight:TextAnchor.MiddleRight)));n.horizontalOverflow=HorizontalWrapMode.Overflow;if(name!=null)n.name=name;
            if(unit!=null){var u=Txt(parent,unit,right-uw+E(.1f),y+drop,uw,h,unitSize,UiTheme.Faint,top?TextAnchor.UpperLeft:TextAnchor.MiddleLeft);u.horizontalOverflow=HorizontalWrapMode.Overflow;}
            return n;
        }
        // A right-pointing chevron centred on (right - size/2, centerY).
        void CoreChevron(Transform parent,float right,float centerY,float size)
        {
            var r=Icon(parent,"chevron",0,0,size,ColorOf(UiTheme.GoldDeepHex));r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=new Vector2(right-size/2,-centerY);r.localRotation=Quaternion.Euler(0,0,90);
        }
        // "장비 제련": the catalogue starts unfiltered (HTML openCoreCatalog(true)).
        void OpenCoreCatalogue(){coreGrade=0;coreClass=-1;coreQuery="";ResetCoreCatalogueScroll();NavigateCore(1);}
        static Text CoreFit(Text t){t.resizeTextForBestFit=true;t.resizeTextMinSize=7;t.resizeTextMaxSize=t.fontSize;return t;}
        // The crystal of one slot in a small framed box.
        void CoreGlyph(Transform parent,int slot,float x,float y,float size)
        {
            var frame=Panel(parent,"Core glyph",UiTheme.SelectedHex,UiTheme.VoidHex,UiTheme.EdgeHex);Place(frame,x,y,size,size);frame.GetComponent<Image>().raycastTarget=false;
            float pad=size*.08f;CurrencyIconView.Create(frame,CurrencyIconView.CoreIcon(slot),pad,pad,size-pad*2);
        }

        // ---- 01 · cores ------------------------------------------------------------------------------------------------------
        void DrawCoreStock(RectTransform panel,float w,float h)
        {
            bool compact=coreLook==CoreLook.Compact,portrait=coreLook==CoreLook.Portrait;
            float px=E(landscape?.55f:.7f),cw=w-px*2,cy=px,gap=E(CoreV(.4f,.5f,.6f)),tg=E(CoreV(.3f,.4f,.45f));
            float hh=Head(panel,"01","코어 선택",px,cy,cw);CoreHistoryButton(panel,px+cw,cy+hh/2);cy+=hh+gap;
            float summary=CoreSummaryHeight(),noteH=0;string note=Loc.T("전설·세트 장비 1개 분해 → 같은 부위 코어 1개");
            if(!compact)noteH=TextHeight(note,cw-E(1.7f),Z(.76f))+E(.6f);
            float room=h-px-cy-gap-summary-(noteH>0?gap+noteH:0),tile=Mathf.Clamp((room-tg*3)/4,E(CoreV(2.9f,3.3f,6f)),E(CoreV(3.5f,4.4f,8.4f))),tw=(cw-tg)/2;
            for(int n=0;n<8;n++)CoreTile(panel,n,px+n%2*(tw+tg),cy+n/2*(tile+tg),tw,tile);
            cy+=tile*4+tg*3+gap;DrawCoreSummary(panel,px,cy,cw,summary);
            if(noteH>0)
            {
                float ny=h-px-noteH;var rule=Panel(panel,"Foot rule",UiTheme.BorderHex,UiTheme.BorderHex);rule.GetComponent<Image>().raycastTarget=false;Place(rule,px,ny,cw,1);
                Icon(panel,"info",px,ny+E(.6f),E(1.1f),muted);Txt(panel,note,px+E(1.6f),ny+E(.5f),cw-E(1.7f),noteH-E(.5f),Z(.76f),muted,TextAnchor.UpperLeft);
            }
        }
        float CoreSummaryHeight()
        {
            if(coreLook==CoreLook.Compact)return E(3.9f);
            return E(.9f)*2+Z(1.7f)*1.3f+Z(.78f)*1.5f+E(.6f)+E(coreLook==CoreLook.Portrait?2.9f:2.6f);
        }
        void CoreTile(Transform parent,int slot,float x,float y,float w,float h)
        {
            int count=store.Data.cores[slot];bool selected=coreSlot==slot,ready=count>=CoreCrafting.CoreCost,compact=coreLook==CoreLook.Compact,portrait=coreLook==CoreLook.Portrait;
            var b=Btn(parent,"",x,y,w,h,()=>SelectCore(slot));b.name="core-slot-"+slot;UiTheme.Choice(b,selected,false);var t=b.transform;
            float glyph=E(compact?2.1f:portrait?3.3f:2.6f),pad=E(compact?.4f:portrait?.75f:.5f);string name=CoreName(slot),status=ready?Loc.T("제작 가능"):Loc.F("{0}개 더 필요",CoreCrafting.CoreCost-count);
            Color countColor=ready?UiTheme.GoldBright:muted;string number=count.ToString("N0");
            if(portrait)
            {
                float top=E(.7f);CoreGlyph(t,slot,pad,top,glyph);CoreCount(t,number,Z(1.7f),countColor,"/10",Z(.58f),w-pad,top,glyph,true);
                float ty=top+glyph+E(.5f);CoreFit(Bold(Txt(t,name,pad,ty,w-pad*2,Z(.98f)*1.4f,Z(.98f))));Txt(t,status,pad,ty+Z(.98f)*1.35f,w-pad*2,Z(.72f)*1.5f,Z(.72f),ready?green:muted);
            }
            else
            {
                CoreGlyph(t,slot,pad,(h-glyph)/2,glyph);float tx=pad+glyph+E(.5f);int countSize=Z(compact?.95f:1.05f),unitSize=Z(.55f);string unit=compact?null:"/10";
                float countW=TextWidth(number,countSize,true,true)+(unit==null?0:TextWidth(unit,unitSize)+E(.25f))+E(.4f),nameW=w-tx-pad-countW;
                CoreCount(t,number,countSize,countColor,unit,unitSize,w-pad,0,h);
                if(compact)
                {
                    CoreFit(Bold(Txt(t,name,tx,0,nameW,h,Z(.78f))));
                    if(ready){var mark=Panel(t,"Ready mark",UiTheme.SuccessHex,UiTheme.SuccessHex);float m=E(.4f);mark.pivot=new Vector2(.5f,.5f);mark.anchorMin=mark.anchorMax=new Vector2(0,1);mark.sizeDelta=new Vector2(m,m);mark.anchoredPosition=new Vector2(w-E(.55f),-E(.55f));mark.localRotation=Quaternion.Euler(0,0,45);mark.GetComponent<Image>().raycastTarget=false;}
                }
                else
                {
                    float nameH=Z(.9f)*1.4f,statusH=Z(.7f)*1.4f,y0=(h-nameH-statusH)/2;
                    CoreFit(Bold(Txt(t,name,tx,y0,nameW,nameH,Z(.9f))));Txt(t,status,tx,y0+nameH,nameW,statusH,Z(.7f),ready?green:muted);
                }
            }
            float meterH=E(compact?.15f:.2f);var meter=Panel(t,"Core meter",ready?UiTheme.SuccessHex:UiTheme.GoldDeepHex,ready?UiTheme.SuccessHex:UiTheme.GoldDeepHex);
            Place(meter,0,h-meterH,Mathf.Max(1,w*Mathf.Min(1f,count/(float)CoreCrafting.CoreCost)),meterH);meter.GetComponent<Image>().raycastTarget=false;
        }
        void DrawCoreSummary(Transform panel,float x,float y,float w,float h)
        {
            int count=store.Data.cores[coreSlot];bool ready=count>=CoreCrafting.CoreCost,compact=coreLook==CoreLook.Compact;
            var box=Panel(panel,"Core summary",UiTheme.InsetHex,UiTheme.InsetHex,UiTheme.GoldDeepHex);Place(box,x,y,w,h);box.GetComponent<Image>().raycastTarget=false;
            float pad=E(compact?.55f:.9f);string status=ready?Loc.T("제작 가능"):Loc.F("{0}개 더 필요",CoreCrafting.CoreCost-count),label=Loc.T("장비 제련");
            int nameSize=Z(compact?.86f:1.02f),subSize=Z(compact?.66f:.78f),countSize=Z(compact?1.2f:1.7f);
            float buttonH=compact?E(2.4f):E(coreLook==CoreLook.Portrait?2.9f:2.6f),buttonW=compact?TextWidth(label,Z(.86f),true)+E(1.8f):w-pad*2,copyW=compact?w-pad*2-buttonW-E(.5f):w-pad*2;
            float r1=compact?E(.35f):pad,r1h=countSize*1.3f,r2=r1+r1h,r2h=subSize*1.5f;
            CoreFit(Bold(Txt(box,CoreName(coreSlot),pad,r1,copyW-E(4),r1h,nameSize,UiTheme.GoldBright)));
            CoreCount(box,count.ToString("N0"),countSize,UiTheme.GoldBright,"/ 10",Z(compact?.6f:.8f),pad+copyW,r1,r1h);
            float sw=TextWidth(status,subSize)+E(.3f);
            CoreFit(Txt(box,"인벤토리·창고 공용",pad,r2,copyW-sw-E(.4f),r2h,subSize,muted));Txt(box,status,pad+copyW-sw,r2,sw,r2h,subSize,ready?green:muted,TextAnchor.MiddleRight);
            var refine=compact?Btn(box,"장비 제련",w-pad-buttonW,(h-buttonH)/2,buttonW,buttonH,()=>OpenCoreCatalogue(),true,Z(.86f))
                :CoreIconButton(box,"장비 제련","craft",pad,r2+r2h+E(.6f),buttonW,buttonH,()=>OpenCoreCatalogue(),true,Z(.94f),UiTheme.GoldBright);
            refine.name="core-refine";refine.interactable=ready;
        }

        // ---- 02 · equipment ---------------------------------------------------------------------------------------------------
        // Nothing chosen yet: the prompt (HTML .core-empty).
        void DrawCoreEmpty(RectTransform panel,float w,float h)
        {
            bool roomy=coreLook==CoreLook.Roomy;float px=E(.55f),cw=w-px*2,cx=w/2;
            float hh=Head(panel,"02","장비 선택",px,px,cw);
            float seal=E(roomy?4.6f:3.4f),over=seal*.21f;int titleSize=Z(roomy?1.4f:1.15f),amountSize=Z(roomy?1.9f:1.5f),lineSize=Z(.86f);
            string hint=roomy?Loc.T("부위 코어 10개로 원하는 전설·세트 장비를 제작합니다."):null;float hintW=Mathf.Min(cw-E(1),E(24)),hintH=hint==null?0:TextHeight(hint,hintW,lineSize);
            float total=over*2+seal+E(.9f)+titleSize*1.4f+(hint==null?0:E(.4f)+hintH)+E(.5f)+amountSize*1.4f+E(.3f)+lineSize*1.5f;
            float y=px+hh+Mathf.Max(0,(h-px*2-hh-total)/2)+over;
            var ring=Panel(panel,"Core seal",UiTheme.InsetHex,UiTheme.InsetHex,UiTheme.GoldDeepHex);ring.pivot=new Vector2(.5f,.5f);ring.anchorMin=ring.anchorMax=new Vector2(0,1);ring.sizeDelta=new Vector2(seal,seal);
            ring.anchoredPosition=new Vector2(cx,-(y+seal/2));ring.localRotation=Quaternion.Euler(0,0,45);ring.GetComponent<Image>().raycastTarget=false;
            float icon=seal*.8f;CurrencyIconView.Create(panel,CurrencyIconView.CoreIcon(coreSlot),cx-icon/2,y+(seal-icon)/2,icon);y+=seal+over+E(.9f);
            Bold(Serif(Txt(panel,"어떤 전설을 벼릴까요?",px,y,cw,titleSize*1.4f,titleSize,UiTheme.GoldBright,TextAnchor.MiddleCenter)));y+=titleSize*1.4f;
            if(hint!=null){y+=E(.4f);Txt(panel,hint,cx-hintW/2,y,hintW,hintH,lineSize,muted,TextAnchor.UpperCenter);y+=hintH;}
            y+=E(.5f);string one="10",two="1";float w1=TextWidth(one,amountSize,true,true),w2=TextWidth(two,amountSize,true,true),arrow=amountSize*.7f,gapA=E(.8f),row=w1+gapA+arrow+gapA+w2,x0=cx-row/2;
            Bold(Serif(Txt(panel,one,x0,y,w1+2,amountSize*1.4f,amountSize,gold,TextAnchor.MiddleCenter)));Icon(panel,"arrow",x0+w1+gapA,y+(amountSize*1.4f-arrow)/2,arrow,UiTheme.Faint);
            Bold(Serif(Txt(panel,two,x0+w1+gapA*2+arrow,y,w2+2,amountSize*1.4f,amountSize,gold,TextAnchor.MiddleCenter)));y+=amountSize*1.4f+E(.3f);
            string a=CoreName(coreSlot),b=" · "+Loc.F("보유 {0:N0}개",store.Data.cores[coreSlot]);float aw=TextWidth(a,lineSize),bw=TextWidth(b,lineSize),lx=cx-(aw+bw)/2;
            Txt(panel,a,lx,y,aw+2,lineSize*1.5f,lineSize,pale);Txt(panel,b,lx+aw,y,bw+2,lineSize*1.5f,lineSize,muted);
        }
        IEnumerable<UniqueItemDefinition> CoreRecipes()=>CoreCrafting.Recipes.Where(r=>r.slot==coreSlot&&(coreGrade==0||coreGrade==1&&string.IsNullOrEmpty(r.setId)||coreGrade==2&&!string.IsNullOrEmpty(r.setId))&&(coreClass<0||r.heroClass<0||r.heroClass==coreClass)&&(string.IsNullOrWhiteSpace(coreQuery)||r.Name.IndexOf(coreQuery,StringComparison.OrdinalIgnoreCase)>=0||r.name.IndexOf(coreQuery,StringComparison.OrdinalIgnoreCase)>=0));
        // The catalogue of the chosen slot: search, class, grade and the recipe list (HTML .is-catalog).
        void DrawCoreCatalog(RectTransform panel,float w,float h)
        {
            bool compact=coreLook==CoreLook.Compact;
            float px=E(landscape?.55f:.7f),cw=w-px*2,cy=px,gap=E(compact?.3f:.45f);
            float hh=Head(panel,"02","제작할 장비 선택",px,cy,cw);
            if(landscape)
            {
                float close=E(2.2f);UiIconButton.CreateGlyph(panel,"core-catalog-back","close",px+cw-close,cy+(hh-close)/2,close,close,()=>NavigateCore(coreRecipeId!=null?2:0),false,muted);
            }
            cy+=hh+E(.4f);Text count=null;
            if(!compact)
            {
                float lh=E(1.4f),wa=TextWidth(CoreName(coreSlot),Z(.82f))+E(.3f);
                Txt(panel,CoreName(coreSlot),px,cy,wa,lh,Z(.82f),muted);Txt(panel,"× 10",px+wa,cy,E(4),lh,Z(.82f),gold);
                count=Txt(panel,"",px+cw-E(8),cy,E(8),lh,Z(.82f),muted,TextAnchor.MiddleRight);cy+=lh+gap;
            }
            string[] classes={Loc.T("모든 직업"),Loc.T(catalog.classNames[0]),Loc.T(catalog.classNames[1]),Loc.T(catalog.classNames[2])};
            float fh=E(compact?2.2f:2.4f),classW=classes.Max(c=>TextWidth(c,Z(.82f)))+E(3f),searchW=cw-classW-E(.4f);
            var inputRoot=Panel(panel,"Core search",UiTheme.InsetHex,UiTheme.InsetHex,UiTheme.BorderHex);Place(inputRoot,px,cy,searchW,fh);
            var input=inputRoot.gameObject.AddComponent<InputField>();input.name="core-search";
            input.textComponent=Txt(inputRoot,"",E(.6f),0,searchW-E(1.2f),fh,Z(.86f),pale);input.placeholder=Txt(inputRoot,"장비 이름 검색",E(.6f),0,searchW-E(1.2f),fh,Z(.86f),UiTheme.Faint);input.SetTextWithoutNotify(coreQuery);
            UiDropdown.Create(panel,"core-class-filter",px+searchW+E(.4f),cy,classW,fh,Z(.82f)/10f,classes,coreClass+1,index=>{coreClass=index-1;ResetCoreCatalogueScroll();Repaint();},null,4);
            cy+=fh+gap;
            Seg(panel,new[]{Loc.T("전체"),Loc.T("전설"),Loc.T("세트")},coreGrade,px,cy,compact?searchW:cw,fh,grade=>{coreGrade=grade;ResetCoreCatalogueScroll();Repaint();},"core-grade-");cy+=fh+gap;
            var scroll=List(panel,"Core recipe list",px,cy,cw,h-cy-px,out var content);
            float rw=cw-8,art=E(compact?2.8f:3.7f),rowMin=E(compact?3f:3.9f),rowGap=E(compact?.3f:.4f);int nameSize=Z(compact?.82f:.92f),kindSize=Z(compact?.66f:.74f);
            void Fill()
            {
                Clear(content);float ry=0;int shown=0;
                foreach(var recipe in CoreRecipes())
                {
                    shown++;string id=recipe.id;var item=CoreCrafting.Definition(id,CoreCrafting.Level(Hero));if(item==null)continue;
                    string kind=Loc.T(string.IsNullOrEmpty(recipe.setId)?"전설":"세트")+" · "+Loc.T(recipe.heroClass<0?"공용":catalog.classNames[recipe.heroClass])+" · Lv."+item.level;
                    float tx=art+E(.7f),tw=rw-tx-E(2.2f),nameH=Mathf.Max(nameSize*1.35f,TextHeight(item.DisplayName,tw,nameSize,true)),row=Mathf.Max(rowMin,nameH+kindSize*1.5f+E(.8f)),ty=(row-nameH-kindSize*1.5f)/2;
                    var select=Btn(content,"",0,ry,rw,row,()=>SelectCoreRecipe(id));select.name="core-recipe-"+id;UiTheme.Choice(select,false,false);
                    Bold(Txt(select.transform,item.DisplayName,tx,ty,tw,nameH,nameSize,EquipmentGradePalette.For(item),TextAnchor.UpperLeft));Txt(select.transform,kind,tx,ty+nameH,tw,kindSize*1.5f,kindSize,muted);
                    CoreChevron(select.transform,rw-E(.8f),row/2,E(1.1f));
                    var inspect=CoreGhost(select.transform,"core-inspect-"+id,0,0,art,row,()=>{coreInspectId=id;OpenDialog("core-preview");},UiButtonRole.Item);
                    var back=Panel(inspect.transform,"Art backdrop",UiTheme.BorderHex,UiTheme.VoidHex,UiTheme.BorderHex);Stretch(back);back.GetComponent<Image>().raycastTarget=false;
                    float inset=art*.1f;EquipmentSlotView.Icon(inspect.transform,item,inset,(row-(art-inset*2))/2-E(.1f),art-inset*2);
                    var line=Panel(inspect.transform,"Rarity line",EquipmentGradePalette.Hex[Storage.Grade(item)],EquipmentGradePalette.Hex[Storage.Grade(item)]);Place(line,1,row-E(.2f),art-1,E(.2f));line.GetComponent<Image>().raycastTarget=false;
                    Icon(inspect.transform,"info",art-E(1.15f),E(.15f),E(1f),gold);
                    ry+=row+rowGap;
                }
                if(shown==0){Txt(content,"조건에 맞는 장비가 없습니다.",E(.6f),E(1),rw-E(1.2f),E(3),Z(.9f),muted,TextAnchor.UpperCenter);ry=E(4);}
                content.sizeDelta=new Vector2(0,ry);if(count!=null)count.text=Loc.F("{0}종",shown);
            }
            Fill();input.onValueChanged.AddListener(value=>{coreQuery=value;Fill();scroll.verticalNormalizedPosition=1;});
        }
        // The chosen equipment: what it will roll, and its power (HTML .col-equip).
        void DrawCorePreview(RectTransform panel,float w,float h)
        {
            var item=CoreDefinition;float px=E(.55f),cw=w-px*2,cy=px;
            float hh=Head(panel,"02","장비 선택",px,cy,cw);
            int size=Z(coreLook==CoreLook.Compact?.74f:.82f);string label=Loc.T("장비 다시 선택");float bh=E(1.9f),icon=E(1.1f),tw=TextWidth(label,size)+E(.3f),bw=E(.6f)+icon+E(.4f)+tw+E(.6f);
            var change=Btn(panel,"",px+cw-bw,cy+hh/2-bh/2,bw,bh,()=>NavigateCore(1));change.name="core-change-recipe";UiTheme.Choice(change,false,true);
            Icon(change.transform,"reroll",E(.6f),(bh-icon)/2,icon,muted);Txt(change.transform,label,E(.6f)+icon+E(.4f),0,tw,bh,size,muted);
            cy+=hh+E(.5f);List(panel,"Core forge details",px,cy,cw,h-cy-px,out var content);
            float used=CoreEquipmentBlock(content,item,0,0,cw-8,false);content.sizeDelta=new Vector2(0,used+E(.3f));
        }
    }
}
