using System;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    public sealed partial class SkillTacticsWindow
    {
        public bool SetUsage(string id,string value)=>EditLegacy(()=>Session.SetUsage(id,value));
        public bool SetLegacy(string id,string value)=>EditLegacy(()=>Session.SetLegacy(id,value));
        bool EditLegacy(Action change)
        {bool ok=Edit(change);if(ok)View.Repaint();return ok;}
        float RenderLegacyRows(RectTransform body,float width,float y)
        {
            var edict=Session.Draft.ProjectEdict();
            bool direct=HuntEdictProgression.Legacy(store.Data)||HuntEdictProgression.Has(store.Data,HuntEdictProgression.Details);
            void Heading(string title)
            {Text(body,"Tactics policy section",Loc.T(title),4,y,width-8,28,17,true);y+=36;}
            void Note(string name,string text)
            {float h=Height(text,width-24,13);Text(body,name,text,12,y,width-24,h,13,ink:skin.InfoInk);y+=h+12;}
            void Row(HuntEdictHudRows.Row row,Action<string,string> apply,Func<string,string> label=null)
            {
                var slot=UiLayout.Rect("Tactics policy slot",body);
                float height=EdictCasualRows.Row(View,slot,width,row,edict,apply,label);UiLayout.Place(slot,0,y,width,height);y+=height+12;
            }
            if(!direct)Note("Combat detail disclosure",Loc.T("세부 스킬 설정은 균열 6단계부터 공개됩니다."));
            foreach(var option in Session.UsageOptions)
            {
                Heading("전투에 적용되는 사용 조건");
                var definition=new EdictOptionDefinition(option.id,"skills",Loc.Language=="en"?option.nameEn:option.name,EdictOptionKind.Choice,option.initial,option.choices.Select(c=>c.id).ToArray());
                Row(new HuntEdictHudRows.Row{definition=definition,value=Session.UsageValue(option.id),editable=!Session.Preview&&direct},(id,value)=>SetUsage(id,value),
                    code=>{var choice=option.choices.Single(c=>c.id==code);return Loc.Language=="en"?choice.nameEn:choice.name;});
                var chosen=option.choices.Single(c=>c.id==Session.UsageValue(option.id));
                Note("Use condition benefit",ClassSkillTree.Display(Loc.Language=="en"?chosen.benefitEn:chosen.benefit));
                Note("Use condition cost",ClassSkillTree.Display(Loc.Language=="en"?chosen.costEn:chosen.cost));
            }
            bool basic=false;
            foreach(var option in Session.LegacyOptions)
            {
                bool next=option.id.StartsWith("HEC-"+edict.heroClass+"-",StringComparison.Ordinal);
                if(next!=basic||option==Session.LegacyOptions.First()){Heading(next?"기본 공격 세부 설정":"전투에 적용되는 세부 설정");basic=next;}
                var definition=new EdictOptionDefinition(option.id,"skills",option.label,option.Numeric?EdictOptionKind.Number:EdictOptionKind.Choice,option.initial,option.choices.ToArray(),(float)option.min,(float)option.max,(float)option.step);
                bool overridden=Session.LegacyOverridden(option.id);
                Row(new HuntEdictHudRows.Row{definition=definition,value=Session.LegacyValue(option.id),editable=(!Session.Preview||next&&Session.CanEditBasic)&&direct&&!overridden},(id,value)=>SetLegacy(id,value),
                    code=>Loc.T(option.choiceLabels[option.choices.ToList().IndexOf(code)]));
                if(overridden)Note("Use condition override",Loc.T("이 값은 위에서 고른 사용 조건을 따릅니다. 기본 사용 조건을 고르면 직접 바꿀 수 있습니다."));
            }
            return y;
        }
    }
}
