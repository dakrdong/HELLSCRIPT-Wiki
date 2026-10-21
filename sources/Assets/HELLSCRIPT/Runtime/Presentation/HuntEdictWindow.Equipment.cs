using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed partial class HuntEdictWindow
    {
        bool addingEquipmentOption;
        void DrawRecommendedEquipment(RectTransform list,float w,HuntEdictUiGroup group)
        {
            string id=group.ids[0];
            if(id=="autoEquip.enabled")
            {
                bool enabled=EquipmentRecommendation.Get(Session.Draft.edict,id)=="ON";
                DrawEquipmentToggle(list,w,enabled,()=>SetGlobal(id,enabled?"OFF":"ON"));
                list=EquipmentSettings(list,enabled);
                Paragraph(list,"새로 획득한 장비가 부위별 기준을 만족하면 바로 착용하고, 교체한 장비는 아래 설정에 따라 처리합니다.",w,Look.BodyText);
                Paragraph(list,"추천 설정: 같은 계열의 더 높은 점수 무기와 더 높은 점수의 방어구·장신구를 착용합니다.",w,Look.SmallText,muted);
                Paragraph(list,"교체한 장비 처리",w,Look.ChoiceTitle,textColor);
                DrawDisplacedEquipmentDropdown(list,w);
                Paragraph(list,"판매·분해에도 가방·자동 정리의 보호 설정을 적용합니다. 보호된 장비나 처리하지 못한 장비는 가방에 남깁니다.",w,Look.SmallText,muted);
                if(EquipmentRecommendation.Get(Session.Draft.edict,EquipmentAutomation.Displaced)=="WAREHOUSE")
                    EquipmentChoice(list,w,"창고가 가득 찼을 때 설정 ›","edict-auto-storage-settings",()=>FocusGlobalOption("bag.warehouseFull"));
                Paragraph(list,"추천 착용 예외",w,Look.ChoiceTitle,textColor);
                Paragraph(list,"체크한 부위는 추천 장비가 있어도 자동으로 교체하지 않습니다.",w,Look.SmallText,muted);
                DrawEquipmentExclusions(list,w);
                if(!hero.useEdict)Paragraph(list,"사냥 칙령이 꺼져 있습니다. 사냥 칙령을 켜야 추천 착용이 작동합니다.",w,Look.SmallText,gold);
                return;
            }
            list=EquipmentSettings(list,EquipmentRecommendation.Get(Session.Draft.edict,"autoEquip.enabled")=="ON");
            bool weapon=id=="autoEquip.weapon";int position=weapon?0:int.Parse(id.Split('.')[1]);
            var current=EquipmentSlots.At(hero,EquipmentSlots.Slot(position),EquipmentSlots.Index(position));
            Paragraph(list,current==null?Loc.T("장착 장비 없음"):Loc.F("현재 착용: {0} · 점수 {1:0.0}",current.DisplayName,EquipmentScore.Value(current)),w,Look.SmallText,muted);
            string mode=EquipmentRecommendation.Get(Session.Draft.edict,id);
            foreach(string choice in weapon?new[]{"SAME","SCORE","OFF"}:new[]{"SCORE","STATS","BOTH","OFF"})
            {
                string next=choice;
                EquipmentChoice(list,w,EquipmentRecommendation.ModeName(choice,weapon),"edict-auto-mode-"+position+"-"+choice,()=>SetGlobal(id,next),mode==choice);
            }
            if(weapon)
            {
                Paragraph(list,Loc.F("현재 무기 구성 점수 {0:0.0}",EquipmentScore.Weapons(hero.inventory.Where(i=>i.equipped))),w,Look.BodyText,gold);
                Paragraph(list,"무기는 주무기와 보조 장비를 합친 교체 전후 점수로 판단합니다. 양손 무기는 한 번만 계산합니다.",w,Look.SmallText,muted);
                Paragraph(list,"같은 계열은 한손검·도끼·양손검·활·쇠뇌처럼 무기 종류가 같은 경우입니다. 직업·레벨·양손 및 보조 장비 조건은 항상 지킵니다.",w,Look.SmallText,muted);
                return;
            }
            if(mode!="STATS"&&mode!="BOTH")return;
            Paragraph(list,mode=="STATS"?"선택한 옵션이 모두 기존 장비보다 높아야 합니다. 장비 점수는 낮아도 착용합니다.":"장비 점수가 더 높고, 선택한 옵션이 모두 기존 장비 이상이어야 합니다.",w,Look.BodyText,gold);
            string statsId=EquipmentRecommendation.StatsId(position);var selected=EquipmentRecommendation.Get(Session.Draft.edict,statsId).Split(',').Where(v=>v!="").ToHashSet();
            if(selected.Count==0)Paragraph(list,"옵션을 하나 이상 추가해 주세요. 선택하기 전에는 이 부위를 자동으로 바꾸지 않습니다.",w,Look.SmallText,gold);
            foreach(string key in ShopAutoSelect.StatKeys(EquipmentRecommendation.Type(position)).Where(selected.Contains))
            {
                string stat=key;float before=0;if(current!=null)ShopAutoSelect.Value(current,key,out before);
                string formatted=key=="main"?BlacksmithCatalog.Format(new[]{"attack","armor","armor","attackSpeed","moveSpeed","life","allResistance","crit"}[EquipmentSlots.Slot(position)],before):StatCatalog.Get(ItemCatalog.Affix(key).stat).FormatSheet(before);
                EquipmentChoice(list,w,Loc.F("✓ {0} · 현재 {1} · 제거",EquipmentRecommendation.StatName(position,key),formatted),"edict-auto-stat-"+key,()=>{selected.Remove(stat);SetGlobal(statsId,string.Join(",",selected));},true);
            }
            EquipmentChoice(list,w,addingEquipmentOption?"옵션 목록 닫기":"＋ 비교 옵션 추가","edict-auto-add-stat",()=>{addingEquipmentOption=!addingEquipmentOption;Repaint();});
            if(!addingEquipmentOption)return;
            Paragraph(list,"장비 분해와 같은 옵션 목록에서 비교할 항목을 고르세요. 없는 옵션의 기존 값은 0입니다.",w,Look.SmallText,muted);
            foreach(string key in ShopAutoSelect.StatKeys(EquipmentRecommendation.Type(position)).Where(k=>!selected.Contains(k)))
            {
                string stat=key;
                EquipmentChoice(list,w,EquipmentRecommendation.StatName(position,key),"edict-auto-add-"+key,()=>{selected.Add(stat);addingEquipmentOption=false;SetGlobal(statsId,string.Join(",",selected));});
            }
        }
        RectTransform EquipmentSettings(RectTransform parent,bool enabled)
        {
            var body=new GameObject("edict-auto-settings",typeof(RectTransform),typeof(VerticalLayoutGroup),typeof(CanvasGroup)).GetComponent<RectTransform>();
            body.SetParent(parent,false);
            var layout=body.GetComponent<VerticalLayoutGroup>();layout.spacing=Look.Gap/2;layout.childControlWidth=layout.childControlHeight=true;
            layout.childForceExpandWidth=true;layout.childForceExpandHeight=false;
            var availability=body.GetComponent<CanvasGroup>();availability.alpha=enabled?1:.4f;
            availability.interactable=enabled;availability.blocksRaycasts=enabled;
            return body;
        }
        void DrawEquipmentExclusions(RectTransform list,float w)
        {
            int[] positions={0,8,1,2,3,4,5,6,7,9};const int columns=2;float cell=Mathf.Max(1,(w-12)/columns);
            for(int first=0;first<positions.Length;first+=columns)
            {
                var row=Row(list,"Equipment exclusion positions",40);float rowHeight=40;
                for(int column=0;column<columns&&first+column<positions.Length;column++)
                {
                    int position=positions[first+column];bool selected=EquipmentRecommendation.Excluded(Session.Draft.edict,position);
                    var area=Panel(row,"edict-auto-exclude-"+position,null);Place(area,column*(cell+12),0,cell,40);
                    var background=area.GetComponent<Image>();background.color=Color.clear;
                    var toggle=area.gameObject.AddComponent<Toggle>();toggle.targetGraphic=background;toggle.transition=Selectable.Transition.None;
                    var label=Text(area,EquipmentRecommendation.PositionName(position),0,0,Mathf.Max(1,cell-34),1000,Look.BodyText);skin.ApplyLabel(label,false,textColor);rowHeight=Mathf.Max(rowHeight,label.preferredHeight+6);
                    var box=Panel(area,"Checkbox",selected?"checkbox-on":"checkbox-off");Place(box,cell-26,7,26,26);box.GetComponent<Image>().raycastTarget=false;
                    var check=HuntEdictDecor.Create(box,"Checkbox tick",0,0,26,26,"tick",skin).GetComponent<HuntEdictDecor>();
                    toggle.graphic=check;toggle.SetIsOnWithoutNotify(selected);
                    toggle.onValueChanged.AddListener(on=>{
                        // The editor re-sends the current value when a toggle rebuilds; a no-change must not rewrite the draft and repaint (that loops).
                        if(on==EquipmentRecommendation.Excluded(Session.Draft.edict,position))return;
                        var chosen=EquipmentRecommendation.Get(Session.Draft.edict,EquipmentRecommendation.ExcludedSlots).Split(',').Where(v=>v!="").ToHashSet();
                        if(on)chosen.Add(position.ToString());else chosen.Remove(position.ToString());
                        SetGlobal(EquipmentRecommendation.ExcludedSlots,string.Join(",",chosen));
                    });
                }
                row.GetComponent<LayoutElement>().minHeight=row.GetComponent<LayoutElement>().preferredHeight=rowHeight;
                foreach(Transform child in row)
                {
                    var rect=(RectTransform)child;Place(rect,rect.anchoredPosition.x,0,cell,rowHeight);
                    var label=child.GetComponentInChildren<Text>();Place(label.rectTransform,0,0,Mathf.Max(1,cell-34),rowHeight);
                    var box=(RectTransform)child.Find("Checkbox");Place(box,cell-26,(rowHeight-26)/2,26,26);
                }
            }
        }
        void DrawEquipmentToggle(RectTransform list,float w,bool enabled,Action changed)
        {
            const float switchWidth=84,switchHeight=40;
            var row=Row(list,"Recommended equipment switch",switchHeight);
            var label=Text(row,"추천 장비 자동 장착",0,0,Mathf.Max(1,w-switchWidth-12),1000,Look.BodyText);label.name="edict-auto-enable-label";skin.ApplyLabel(label,false,textColor);
            float h=Mathf.Max(switchHeight,label.preferredHeight+6);row.GetComponent<LayoutElement>().minHeight=row.GetComponent<LayoutElement>().preferredHeight=h;
            Place(label.rectTransform,0,0,Mathf.Max(1,w-switchWidth-12),h);
            var button=Button(row,enabled?"ON":"OFF",w-switchWidth,(h-switchHeight)/2,switchWidth,switchHeight,changed,enabled?"switch-on":"switch-off",Look.SmallText);
            button.name="edict-auto-enable";SurfaceChoice(button,enabled,"switch-on","switch-off");
            var caption=button.GetComponentInChildren<Text>();caption.color=enabled?skin.White:muted;
            Place(caption.rectTransform,enabled?5:39,0,40,switchHeight);
            var thumb=Panel(button.transform,"Switch thumb","switch-thumb");Place(thumb,enabled?switchWidth-35:5,5,30,30);thumb.GetComponent<Image>().raycastTarget=false;
        }
        void DrawDisplacedEquipmentDropdown(RectTransform list,float w)
        {
            var definition=EdictOptions.Global.Single(d=>d.id==EquipmentAutomation.Displaced);
            string selected=EquipmentRecommendation.Get(Session.Draft.edict,definition.id);
            float h=Look.Button-12;var row=Row(list,"Replaced equipment choice",h+2);
            var dropdown=UiDropdown.Create(row,"edict-auto-displaced",0,0,Mathf.Min(w,340),h,1,
                definition.choices.Select(code=>Loc.T(EdictOptionLabels.Item(definition,code))).ToArray(),Array.IndexOf(definition.choices,selected),
                index=>SetGlobal(definition.id,definition.choices[index]),visibleItems:3);
            // Keep the shared dropdown's input/list ownership, including its inactive template.
            dropdown.colors=CasualControlColors();
            foreach(var surface in dropdown.GetComponentsInChildren<StorageSurface>(true))
            {
                surface.SetSkin(skin,skin.Navy,12,2,2);
                // Dropdown clones serialized fields, while StorageSurface's opt-in skin is instance state.
                surface.top=surface.bottom=skin.White;surface.edge=skin.Navy;surface.radius=12;
            }
            foreach(var label in dropdown.GetComponentsInChildren<Text>(true)){skin.ApplyLabel(label,true,textColor);label.fontSize=Mathf.RoundToInt(Look.BodyText+1);}
            foreach(var glyph in dropdown.GetComponentsInChildren<StorageGlyph>(true))glyph.color=skin.Navy;
            foreach(var toggle in dropdown.GetComponentsInChildren<Toggle>(true))
            {toggle.colors=CasualControlColors();toggle.targetGraphic.color=skin.White;toggle.graphic.color=skin.GoldShade;}
        }
        ColorBlock CasualControlColors()
        {
            var colors=ColorBlock.defaultColorBlock;colors.normalColor=skin.White;colors.highlightedColor=skin.ChoiceTint;
            colors.pressedColor=skin.GoldShade;colors.selectedColor=skin.ChoiceTint;colors.disabledColor=skin.Disabled;return colors;
        }
        void DrawEquipmentActions(RectTransform list,float w,string id,string prefix)
        {
            var definition=EdictOptions.Global.Single(d=>d.id==id);string selected=EquipmentRecommendation.Get(Session.Draft.edict,id);
            foreach(string code in definition.choices)
            {
                string choice=code;
                EquipmentChoice(list,w,Loc.T(EdictOptionLabels.Item(definition,code)),prefix+code,()=>SetGlobal(id,choice),code==selected);
            }
        }
        void DrawWarehouseHandling(RectTransform list,float w)
        {
            Paragraph(list,"새 장비를 넣는 방법",w,Look.ChoiceTitle,textColor);
            DrawEquipmentActions(list,w,EquipmentAutomation.Admission,"edict-storage-admission-");
            Paragraph(list,"추천 착용과 자동 정리의 창고 보관에 함께 적용합니다. 판매할 수 있는 장비만 비교하며, 가격·점수가 같으면 먼저 보관한 장비를 판매합니다.",w,Look.SmallText,muted);
            Paragraph(list,"보호된 장비는 판매하지 않습니다. 빈칸을 만들 수 없으면 새 장비를 가방에 남깁니다. 수동 창고 이동에는 자동 판매를 적용하지 않습니다.",w,Look.SmallText,muted);
            Paragraph(list,"창고가 가득 찼을 때 반복 사냥",w,Look.ChoiceTitle,textColor);
            DrawEquipmentActions(list,w,"bag.warehouseFull","edict-storage-repeat-");
            Paragraph(list,"중단을 선택하면 현재 전투를 마친 뒤 다음 전투를 기다립니다. 창고 정리 후 결과 화면에서 다시 시도하세요. 계속 사냥해도 가방 부족·목표 달성 등 다른 중단 조건은 적용합니다.",w,Look.SmallText,muted);
            Paragraph(list,"가방도 가득 찼을 때",w,Look.ChoiceTitle,textColor);
            DrawEquipmentActions(list,w,"bag.stillFull","edict-storage-bag-");
        }
        void EquipmentChoice(RectTransform list,float w,string caption,string name,Action action,bool selected=false)
        {
            caption=Loc.T(caption).TrimStart('✓',' ');bool link=caption.TrimEnd().EndsWith("›",StringComparison.Ordinal);
            if(link)caption=caption.TrimEnd().TrimEnd('›').TrimEnd();
            var row=Row(list,name,Look.Button);var button=Button(row,caption,0,0,w,Look.Button,action,"button-idle",Look.BodyText+1);button.name=name;SurfaceChoice(button,selected);
            var label=button.GetComponentInChildren<Text>();label.alignment=TextAnchor.MiddleLeft;Place(label.rectTransform,14,0,Mathf.Max(1,w-54),1000);
            float h=Mathf.Max(Look.Button,label.preferredHeight+14);row.GetComponent<LayoutElement>().minHeight=row.GetComponent<LayoutElement>().preferredHeight=h+2;
            Place((RectTransform)button.transform,0,0,w,h);Place(label.rectTransform,14,0,Mathf.Max(1,w-54),h);ChoiceCheck(button.transform,w,h,selected);
            if(link&&!selected)HuntEdictDecor.Create(button.transform,"Choice chevron",w-30,(h-18)/2,18,18,"chevron",skin);
        }
    }
}
