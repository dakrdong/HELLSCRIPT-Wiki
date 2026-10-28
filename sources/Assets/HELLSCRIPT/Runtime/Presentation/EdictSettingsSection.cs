using System;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    // The enclosing window keeps its own draft. These catalog rows use the HUD's immediate edict transaction.
    public sealed class EdictSettingsSection
    {
        readonly GameStore store;readonly GameCatalog catalog;readonly string tab,heroId;readonly CombatSimulation simulation;
        int group;string error="";
        public EdictSettingsSection(GameStore store,GameCatalog catalog,string tab,CombatSimulation simulation=null)
        {this.store=store;this.catalog=catalog;this.tab=tab;this.simulation=simulation;heroId=store.Data.Hero.id;}
        public bool Apply(string id,string value)
        {
            try
            {
                if(store.Data.Hero.id!=heroId)throw new InvalidOperationException(Loc.T("설정을 편집한 캐릭터를 먼저 선택하세요."));
                if(!HuntEdictHudRows.Groups(tab,store.Data,store.Data.Hero).Any(g=>g.ids.Contains(id)))throw new ArgumentException("Option belongs to a different settings page.");
                var session=new HuntEdictEditSession(store.Data.Hero);session.ReplaceDraft(HuntEdictQuickMenu.With(session.Draft,id,value));
                var live=simulation!=null&&!RepeatHunt.Terminal(simulation.State)&&!simulation.State.portal?simulation:null;
                bool ok=store.CommitHuntEdict(session,catalog,live,live?.State.paused??false);error=ok?"":store.Error;return ok;
            }
            catch(Exception failure){error=failure.Message;return false;}
        }
        public float Draw(ContentWindowView view,Transform parent,float width,float x=0,float y=0)
        {
            var root=UiLayout.Rect("edict-settings-section-"+tab,parent);UiLayout.Place(root,x,y,width,1);
            var skin=UiSkin.Casual;float cy=0;
            float Note(string name,string text,int size)
            {
                var label=UiLayout.Text(root,name,Loc.T(text),0,cy,width,1,size,skin.Navy,skin.BodyFont);
                float h=Mathf.Max(size+8,Mathf.Ceil(label.preferredHeight)+4);label.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,h);cy+=h+8;return h;
            }
            Note("Edict section title","사냥 칙령 · 바로 적용",18);
            if(error!="")Note("Edict section error",error,13);
            var data=store.Data;var hero=data.Hero;
            if(hero.id!=heroId){Note("Edict owner changed","설정을 편집한 캐릭터를 먼저 선택하세요.",13);return cy;}
            var groups=HuntEdictHudRows.Groups(tab,data,hero);
            if(groups.Count==0){Note("Edict section locked","진행 후 공개됩니다.",13);return cy;}
            group=Mathf.Clamp(group,0,groups.Count-1);const float gap=6;float cell=(width-gap)/2,top=cy,rowHeight=44;
            var buttons=new System.Collections.Generic.List<UnityEngine.UI.Button>();
            for(int i=0;i<groups.Count;i++)
            {
                int at=i;var button=view.Button(root,groups[i].title,i%2*(cell+gap),top+i/2*(rowHeight+gap),cell,rowHeight,()=>{group=at;view.Repaint();});
                button.name="edict-settings-group-"+tab+"-"+i;UiTheme.Choice(button,i==group,tab:false);
                var label=button.GetComponentInChildren<UnityEngine.UI.Text>();label.font=skin.BodyFont;
                rowHeight=Mathf.Max(rowHeight,Mathf.Ceil(label.preferredHeight)+12);buttons.Add(button);
            }
            for(int i=0;i<buttons.Count;i++)
            {
                UiLayout.Place((RectTransform)buttons[i].transform,i%2*(cell+gap),top+i/2*(rowHeight+gap),cell,rowHeight);
                var label=buttons[i].GetComponentInChildren<UnityEngine.UI.Text>();UiLayout.Stretch(label.rectTransform);label.rectTransform.offsetMin=new Vector2(5,2);label.rectTransform.offsetMax=new Vector2(-5,-2);
            }
            cy+=Mathf.Ceil(groups.Count/2f)*(rowHeight+gap);
            Note("Edict group description",groups[group].Description,13);
            var edict=HuntEdictLoadout.FromHero(hero).edict;
            foreach(var row in HuntEdictHudRows.Rows(groups[group],edict,data,hero))
            {
                var slot=UiLayout.Rect("Edict row slot",root);
                float height=EdictCasualRows.Row(view,slot,width,row,edict,(id,value)=>{Apply(id,value);view.Repaint();});
                UiLayout.Place(slot,0,cy,width,height);cy+=height+8;
            }
            root.sizeDelta=new Vector2(width,cy);return cy;
        }
    }
}
