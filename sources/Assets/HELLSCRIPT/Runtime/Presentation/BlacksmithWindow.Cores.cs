using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    // 코어 제작 state and helpers. The screen is drawn in BlacksmithWindow.CoresScreen.cs and BlacksmithWindow.CoresEquip.cs.
    public sealed partial class BlacksmithWindow
    {
        int coreSlot,corePage,coreInvestment,coreGrade,coreClass=-1;
        string coreRecipeId,coreInspectId,coreResultId,coreQuery="",coreRequest;
        bool coreCommitting;Action coreRefresh;
        readonly Dictionary<string,float> coreScroll=new Dictionary<string,float>();
        public int CorePage=>corePage;public int CoreInvestment=>coreInvestment;public string CoreRecipeId=>coreRecipeId;
        Item CoreDefinition=>CoreCrafting.Definition(coreRecipeId,CoreCrafting.Level(Hero));
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

        // "장비 제련": the catalogue starts unfiltered (HTML openCoreCatalog(true)).
        void OpenCoreCatalogue(){coreGrade=0;coreClass=-1;coreQuery="";ResetCoreCatalogueScroll();NavigateCore(1);}
        IEnumerable<UniqueItemDefinition> CoreRecipes()=>CoreCrafting.Recipes.Where(r=>r.slot==coreSlot&&(coreGrade==0||coreGrade==1&&string.IsNullOrEmpty(r.setId)||coreGrade==2&&!string.IsNullOrEmpty(r.setId))&&(coreClass<0||r.heroClass<0||r.heroClass==coreClass)&&(string.IsNullOrWhiteSpace(coreQuery)||r.Name.IndexOf(coreQuery,StringComparison.OrdinalIgnoreCase)>=0||r.name.IndexOf(coreQuery,StringComparison.OrdinalIgnoreCase)>=0));
        // A button with no face of its own: a transparent hit area with the shared focus and click feedback.
        UiButton CoreGhost(Transform parent,string name,float x,float y,float w,float h,Action click,UiButtonRole role=UiButtonRole.Icon)
        {
            var r=Rect(name,parent);Place(r,x,y,w,h);var hit=r.gameObject.AddComponent<Image>();hit.color=Color.clear;
            var b=r.gameObject.AddComponent<UiButton>();b.targetGraphic=hit;b.Configure(role);skin.ApplyChrome(b);b.onClick.AddListener(()=>click());return b;
        }
        static Text CoreFit(Text t){t.resizeTextForBestFit=true;t.resizeTextMinSize=7;t.resizeTextMaxSize=t.fontSize;return t;}
    }
}
