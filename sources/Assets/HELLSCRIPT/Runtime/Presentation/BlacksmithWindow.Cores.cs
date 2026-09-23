using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed partial class BlacksmithWindow
    {
        int coreSlot,corePage,coreInvestment,coreGrade,coreClass=-1;
        string coreRecipeId,coreInspectId,coreResultId,coreQuery="",coreRequest;
        bool coreCommitting;Action coreRefresh;
        readonly Dictionary<string,float> coreScroll=new Dictionary<string,float>();
        public int CorePage=>corePage;public int CoreInvestment=>coreInvestment;public string CoreRecipeId=>coreRecipeId;
        Item CoreDefinition=>CoreCrafting.Definition(coreRecipeId,CoreCrafting.Level(Hero));
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
        {coreSlot=slot;coreRecipeId=null;coreRequest=null;corePage=0;coreQuery="";ResetCoreCatalogueScroll();Repaint();}
        void SelectCoreRecipe(string id)
        {coreRecipeId=id;coreRequest=null;NavigateCore(2);}
        void Coin(Transform parent,float x,float y,float size)
        {
            var r=Rect("Abyssal Coin",parent);Place(r,x,y,size,size);var image=r.gameObject.AddComponent<Image>();
            image.sprite=Resources.Load<Sprite>("Art/GlobalHUD/currency-abyssal-coin");image.preserveAspect=true;image.raycastTarget=false;
        }
        void DrawCoreWallet(Transform parent)
        {
            float x=width-(landscape?232:187);Txt(parent,"심연 주화",x,7,140,18,9,muted,TextAnchor.MiddleRight);
            Coin(parent,x+8,27,24);Txt(parent,store.Data.premium.ToString("N0"),x+36,24,104,29,17,gold,TextAnchor.MiddleRight);
        }
        void DrawCoreCraft(float y,float h)
        {
            coreInvestment=Math.Clamp(coreInvestment,0,CoreCrafting.InvestmentLimit(store.Data));
            if(landscape)
            {
                float w=width/3;DrawCoreList(Column("Core stock",0,y,w,h),w,h);
                var middle=Column("Core equipment",w,y,w,h);
                if(corePage==2&&CoreDefinition!=null)DrawCorePreview(middle,w,h,false);else DrawCoreCatalogue(middle,w,h,false);
                DrawCoreQuality(Column("Core quality",w*2,y,w,h),w,h);return;
            }
            var page=Column("Core page "+corePage,0,y,width,h);
            if(corePage==0)DrawCoreList(page,width,h);
            else if(corePage==1)DrawCoreCatalogue(page,width,h,true);
            else DrawCorePreview(page,width,h,true);
        }
        void DrawCoreList(Transform parent,float w,float h)
        {
            Txt(parent,"01 / 코어 선택",16,8,w-32,20,9,gold);Txt(parent,"보유 코어",16,29,w-32,28,16,pale);
            Txt(parent,"인벤토리·창고 공용",16,59,w-32,19,10,muted);
            Scroll(parent,"Core stock list",10,85,w-20,h-122,out var list);float y=0,row=landscape?76:78;
            for(int n=0;n<8;n++)
            {
                int slot=n,count=store.Data.cores[n];bool selected=coreSlot==n;float rw=w-28;
                var b=Btn(list,"",0,y,rw,row-5,()=>SelectCore(slot));b.name="core-slot-"+slot;
                UiTheme.Choice(b,selected,false);
                Glyph(b.transform,"orb",10,16,34,gold);
                Txt(b.transform,Loc.F("{0} 코어",GameCatalog.Slots[n]),53,8,rw-155,25,12,pale);
                Txt(b.transform,selected?Loc.F("보유 {0:N0}개",count):"계정 공용",53,36,rw-155,23,10,muted);
                if(selected)
                {
                    var refine=Btn(b.transform,"장비 제련",rw-95,6,87,35,()=>NavigateCore(1),true,11);refine.name="core-refine";refine.interactable=count>=CoreCrafting.CoreCost;
                    Txt(b.transform,count.ToString("N0")+" / 10",rw-95,43,87,23,12,gold,TextAnchor.MiddleCenter);
                }
                else
                {
                    Txt(b.transform,count.ToString("N0")+" / 10",rw-96,7,84,27,14,gold,TextAnchor.MiddleRight);
                    Txt(b.transform,count>=10?"제작 가능":Loc.F("{0}개 더 필요",10-count),rw-105,36,93,23,9,count>=10?green:muted,TextAnchor.MiddleRight);
                }
                y+=row;
            }
            list.sizeDelta=new Vector2(0,y);Txt(parent,"전설·세트 장비 1개 분해 → 같은 부위 코어 1개",14,h-33,w-28,29,9,muted);
        }
        IEnumerable<UniqueItemDefinition> CoreRecipes()=>CoreCrafting.Recipes.Where(r=>r.slot==coreSlot&&(coreGrade==0||coreGrade==1&&string.IsNullOrEmpty(r.setId)||coreGrade==2&&!string.IsNullOrEmpty(r.setId))&&(coreClass<0||r.heroClass<0||r.heroClass==coreClass)&&(string.IsNullOrWhiteSpace(coreQuery)||r.Name.IndexOf(coreQuery,StringComparison.OrdinalIgnoreCase)>=0||r.name.IndexOf(coreQuery,StringComparison.OrdinalIgnoreCase)>=0));
        void DrawCoreCatalogue(Transform parent,float w,float h,bool back)
        {
            float top=back?43:0;
            if(back)Btn(parent,"‹ 코어 목록으로",12,4,w-24,35,()=>NavigateCore(0),false,12).name="core-back";
            Txt(parent,"제작할 장비 선택",14,top+8,w-28,27,15,pale);
            Txt(parent,Loc.F("{0} 코어 × 10 · Lv.{1}",GameCatalog.Slots[coreSlot],CoreCrafting.Level(Hero)),14,top+36,w-28,24,10,gold);
            float searchWidth=(w-34)*.62f;
            var inputRoot=Panel(parent,"Core search","11170d","11170d","53563b");Place(inputRoot,12,top+66,searchWidth,34);
            var input=inputRoot.gameObject.AddComponent<InputField>();input.name="core-search";input.textComponent=Txt(inputRoot,"",8,0,searchWidth-16,34,11,pale);
            input.placeholder=Txt(inputRoot,"장비 이름 검색",8,0,searchWidth-16,34,10,muted);input.SetTextWithoutNotify(coreQuery);
            var classes=new[]{"모든 직업","전사","궁수","마법사"};
            Btn(parent,Loc.T(classes[coreClass+1])+" ▾",18+searchWidth,top+66,w-searchWidth-30,34,()=>OpenDialog("core-classes"),false,10).name="core-class-filter";
            string[] grades={"전체","전설","세트"};
            for(int n=0;n<3;n++){int grade=n;var b=Btn(parent,grades[n],12+n*(w-24)/3,top+108,(w-24)/3,32,()=>{coreGrade=grade;ResetCoreCatalogueScroll();Repaint();},false,11);b.name="core-grade-"+n;UiTheme.Choice(b,coreGrade==n);}
            var scroll=Scroll(parent,"Core recipe list",10,top+150,w-20,h-top-160,out var list);
            void Fill()
            {
                Clear(list);float y=0,size=UiTheme.SlotSize(landscape);
                foreach(var recipe in CoreRecipes())
                {
                    string id=recipe.id;var item=CoreCrafting.Definition(id,CoreCrafting.Level(Hero));float rw=w-28;
                    var r=Panel(list,"Core recipe "+id,"24271d","191c16","363b2b");
                    var icon=EquipmentSlotView.Create(r,item,5,5,size,font);var inspect=icon.gameObject.AddComponent<UiButton>();inspect.targetGraphic=icon.GetComponent<Image>();inspect.Configure(UiButtonRole.Item);
                    inspect.name="core-inspect-"+id;inspect.onClick.AddListener(()=>{coreInspectId=id;OpenDialog("core-preview");});
                    var select=Btn(r,"",size+12,1,rw-size-13,67,()=>SelectCoreRecipe(id));select.name="core-recipe-"+id;
                    var surface=select.GetComponent<StorageSurface>();surface.Paint("24271d","191c16","00000000");
                    var name=Txt(select.transform,item.DisplayName,5,2,rw-size-33,1000,11,EquipmentGradePalette.For(item));float nameHeight=Mathf.Max(32,name.preferredHeight+4);
                    Place(name.rectTransform,5,2,rw-size-33,nameHeight);float row=Mathf.Max(size+14,nameHeight+42);
                    Place(r,0,y,rw,row-5);Place((RectTransform)select.transform,size+12,1,rw-size-13,row-7);
                    Txt(select.transform,Loc.F("{0} · {1}",string.IsNullOrEmpty(recipe.setId)?"전설":"세트",recipe.heroClass<0?"공용":classes[recipe.heroClass+1]),5,nameHeight+4,rw-size-33,28,9,muted);
                    y+=row;
                }
                if(y==0){Txt(list,"조건에 맞는 장비가 없습니다.",8,20,w-48,60,12,muted);y=90;}
                list.sizeDelta=new Vector2(0,y);
            }
            Fill();input.onValueChanged.AddListener(value=>{coreQuery=value;Fill();scroll.verticalNormalizedPosition=1;});
        }
        void DrawCorePreview(Transform parent,float w,float h,bool portrait)
        {
            float top=portrait?43:0,footer=portrait?103:0;
            if(portrait)Btn(parent,"‹ 장비 목록으로",12,4,w-24,35,()=>NavigateCore(1),false,12).name="core-back";
            Scroll(parent,"Core forge details",0,top,w,h-top-footer,out var content);float y=6;
            Btn(content,"장비 다시 선택",w-130,y,115,31,()=>NavigateCore(1),false,10).name="core-change-recipe";y+=43;
            var item=CoreDefinition;if(item==null)return;
            float left=w*.39f,right=w-left-22,slotSize=UiTheme.SlotSize(landscape);
            var icon=EquipmentSlotView.Create(content,item,(left-slotSize)/2,y+7,slotSize,font);var inspect=icon.gameObject.AddComponent<UiButton>();inspect.targetGraphic=icon.GetComponent<Image>();inspect.Configure(UiButtonRole.Item);inspect.onClick.AddListener(()=>{coreInspectId=coreRecipeId;OpenDialog("core-preview");});inspect.name="core-forge-inspect";
            var name=Txt(content,item.DisplayName,10,y+slotSize+20,left-20,1000,16,EquipmentGradePalette.For(item),TextAnchor.MiddleCenter);float nameHeight=Mathf.Max(59,name.preferredHeight+4);
            Place(name.rectTransform,10,y+slotSize+20,left-20,nameHeight);float leftEnd=y+slotSize+nameHeight+50;
            Txt(content,"Lv."+item.level,10,leftEnd-26,left-20,26,11,muted,TextAnchor.MiddleCenter);
            Txt(content,"제작 옵션 범위",left+7,y,right,26,12,pale);float rowY=y+33;
            foreach(var roll in item.rolls)
            {
                var property=ItemComparison.Affix(item,roll);
                var label=Txt(content,property.label,left+7,rowY,right*.53f,1000,10,pale);
                // Reserve one line at enlarged sizes for Canvas pixel rounding near wrap boundaries.
                float labelHeight=Mathf.Max(25,label.preferredHeight+4+(textScale>1.2f?label.fontSize:0)),rowHeight=Mathf.Max(42,labelHeight+18);
                Place(label.rectTransform,left+7,rowY,right*.53f,labelHeight);
                Txt(content,property.Format(property.minimum.Value)+" ~ "+property.Format(property.maximum.Value),left+7+right*.53f,rowY,right*.47f,rowHeight-15,11,gold,TextAnchor.MiddleRight);
                var guaranteed=Txt(content,"",left+7,rowY+rowHeight-17,right,17,8,muted,TextAnchor.MiddleRight);
                void RefreshMinimum(){guaranteed.text=coreInvestment>0?Loc.F("최소 보장 {0}",property.Format(ItemCatalog.Affix(roll.affixId).Value(item.level,coreInvestment))):"";}
                coreRefresh+=RefreshMinimum;RefreshMinimum();
                rowY+=rowHeight;
            }
            y=Mathf.Max(rowY,leftEnd)+9;Rule(content,y,w);y+=10;
            Txt(content,string.IsNullOrEmpty(ItemCatalog.Unique(item.special).setId)?"고유 효과":"세트 효과",14,y,w-28,23,11,EquipmentGradePalette.For(item));y+=25;
            Paragraph(content,ItemCatalog.Unique(item.special).Description,14,ref y,w-28,10,muted);y+=10;
            if(portrait)y=CoreQualityContent(content,w,y);
            content.sizeDelta=new Vector2(0,y+14);
            if(portrait)DrawCoreActions(parent,h-footer,w,footer);
        }
        void DrawCoreQuality(Transform parent,float w,float h)
        {Scroll(parent,"Core quality controls",0,0,w,h,out var content);float end=CoreQualityContent(content,w,12);content.sizeDelta=new Vector2(0,end+12);}
        float CoreQualityContent(Transform parent,float w,float y)
        {
            Txt(parent,"최소 보장 품질",14,y,w*.62f,33,14,pale);var percent=Txt(parent,"",w*.59f,y,w*.41f-14,33,24,gold,TextAnchor.MiddleRight);percent.name="core-quality-floor";y+=38;
            var gaugeRoot=Rect("Core quality gauge",parent);Place(gaugeRoot,14,y,w-28,32);var gauge=gaugeRoot.gameObject.AddComponent<CoreQualityGauge>();gauge.color=gold;gauge.Set(coreInvestment,CoreCrafting.InvestmentLimit(store.Data));gauge.Changed=SetCoreInvestment;y+=32;
            Txt(parent,"0%",14,y,70,18,9,muted);Txt(parent,"100%",w-84,y,70,18,9,muted,TextAnchor.MiddleRight);y+=20;
            Txt(parent,"옵션별 추첨 구간",14,y,w*.48f,28,11,muted);var range=Txt(parent,"",w*.45f,y,w*.55f-14,28,17,gold,TextAnchor.MiddleRight);range.name="core-roll-interval";y+=30;
            Rule(parent,y,w);y+=4;Txt(parent,"투입 심연 주화",14,y,w*.5f,28,12,pale);Coin(parent,w-117,y+2,23);var amount=Txt(parent,"",w-94,y,80,29,20,gold,TextAnchor.MiddleRight);amount.name="core-investment";y+=31;
            var limit=Txt(parent,"",14,y,w-28,20,9,muted,TextAnchor.MiddleRight);y+=22;
            var steps=new[]{-500,-100,-50,50,100,500};var buttons=new List<Button>();float bw=(w-43)/6;
            foreach(int step in steps)
            {int delta=step;var b=Btn(parent,(step>0?"+":"−")+Math.Abs(step),14+buttons.Count*(bw+3),y,bw,37,()=>SetCoreInvestment(coreInvestment+delta),step>0,10);b.name="core-adjust-"+step;buttons.Add(b);}y+=41;
            Txt(parent,"1개당 +0.01% · 최대 8,000개 (80%)",14,y,w-28,20,9,muted);y+=22;
            var remaining=Txt(parent,"",14,y,w-28,25,11,pale,TextAnchor.MiddleRight);y+=28;
            void Refresh()
            {
                percent.text=(coreInvestment/100d).ToString("0.##")+"%";range.text=percent.text+" ~ 100%";
                amount.text=coreInvestment.ToString("N0");limit.text=Loc.F("보유 한도 {0:N0}개 · 최대 {1:0.##}%",CoreCrafting.InvestmentLimit(store.Data),CoreCrafting.InvestmentLimit(store.Data)/100d);
                remaining.text=Loc.F("제작 후 심연 주화 {0:N0}개",store.Data.premium-coreInvestment);gauge.Set(coreInvestment,CoreCrafting.InvestmentLimit(store.Data));
                for(int n=0;n<buttons.Count;n++)buttons[n].interactable=steps[n]<0?coreInvestment>0:coreInvestment<CoreCrafting.InvestmentLimit(store.Data);
            }
            coreRefresh+=Refresh;Refresh();return y;
        }
        public void SetCoreInvestment(int value)
        {coreInvestment=Math.Clamp(value,0,CoreCrafting.InvestmentLimit(store.Data));coreRequest=null;coreRefresh?.Invoke();}
        void DrawCoreActions(Transform parent,float y,float w,float h)
        {
            var rail=Panel(parent,"Core craft actions","292d1e","202419","585337");Place(rail,0,y,w,h);
            float x=landscape?w/3:14,bw=landscape?w-x-16:w-28;
            var summary=Txt(rail,"",14,6,landscape?w/3-28:w-28,25,11,gold);
            var error=Txt(rail,"",14,landscape?34:32,landscape?w/3-28:w-28,landscape?38:19,9,muted);
            CoreCraftQuote quote=null;var b=Btn(rail,"제작",x,landscape?13:54,bw,landscape?53:42,()=>CommitCore(quote),true,15);b.name="core-craft";
            void Refresh()
            {
                quote=CoreCrafting.Quote(store.Data,coreRecipeId,coreInvestment);summary.text=Loc.F("코어 10개 · 심연 주화 {0:N0}개",coreInvestment);
                error.text=quote.CanCraft?Loc.T("각 옵션의 수치는 개별 추첨됩니다."):Loc.T(quote.error);b.interactable=quote.CanCraft&&!coreCommitting;
            }
            coreRefresh+=Refresh;Refresh();
        }
        void CommitCore(CoreCraftQuote quote)
        {
            if(coreCommitting||!access()||quote==null)return;coreCommitting=true;
            coreRequest??=Guid.NewGuid().ToString("N");string request=coreRequest;
            bool ok=store.CraftCoreEquipment(request,quote,(uint)DateTime.UtcNow.Ticks);coreCommitting=false;
            if(!ok){Toast(store.Error);return;}
            coreResultId=request;coreRequest=null;dialogKind="core-result";Repaint();
        }
    }
}
