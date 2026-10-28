using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed partial class GameUI
    {
        bool sheetCatalog;
        bool CatalogPanel=>sheetCatalog;
        void BuildCatalogPanel(AccountSave data,HeroSave hero)
        {
            var groups=HuntEdictHudRows.Groups(sheetTab,data,hero);sheetGroup=Mathf.Clamp(sheetGroup,0,groups.Count-1);
            bool submenu=sheetTab=="combat"||sheetTab=="survival";
            if(submenu)
            {
                float h=BuildCompactEdictSurface("edict-catalog-nameplate",groups[sheetGroup].title,"전투 중 · 바로 적용",CasualSheetHeight(560));
                BuildSurvivalFrame(h,"edict-catalog-back");
            }
            else
            {
                BuildCasualSheetFrame("edict-catalog-nameplate",EdictTabTitle(sheetTab),560,136);
                BuildCasualGroupChips(sheetCard.rect.height-102*positionUnit,groups,2);
            }
            BuildCatalogBody();
        }
        void AddSelectedCatalogRows(HuntEdictV2Document edict,params string[] except)
        {
            var data=game.Store.Data;var hero=data.Hero;var groups=HuntEdictHudRows.Groups(sheetTab,data,hero);
            if(sheetGroup>=groups.Count)return;
            foreach(var row in HuntEdictHudRows.Rows(groups[sheetGroup],edict,data,hero).Where(r=>!except.Contains(r.definition.id)))AddCasualRow(row,edict);
        }
        void BuildCatalogBody()
        {
            float keep=sheetContent.anchoredPosition.y;
            foreach(Transform child in sheetContent){child.gameObject.SetActive(false);Destroy(child.gameObject);}
            var data=game.Store.Data;var hero=data.Hero;var groups=HuntEdictHudRows.Groups(sheetTab,data,hero);
            SurvivalNote(sheetContent,"edict-sheet-description",groups[sheetGroup].Description,UiTheme.Caption,UiSkin.Casual.Muted);
            AddSelectedCatalogRows(HuntEdictLoadout.FromHero(hero).edict);
            FinishSurvivalBody(keep);
        }
    }
}
