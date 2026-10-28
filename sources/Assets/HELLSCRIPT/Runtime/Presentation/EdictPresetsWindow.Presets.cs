using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed partial class EdictPresetsWindow
    {
        int renamingPreset=-1;string renamingText="";
        void DrawPresets()
        {
            if(!HasFeature(HuntEdictProgression.Presets))return;
            bool sharing=HasFeature(HuntEdictProgression.Sharing);float w=bodyWidth,y=0,gap=Look.Gap;
            var heading=Text(body,"프리셋·공유",0,y,w,1000,Look.CardTitle,textColor);float titleHeight=heading.preferredHeight+4;Place(heading.rectTransform,0,y,w,titleHeight);y+=titleHeight+gap/2;
            var hint=Text(body,"이름을 누르면 선택합니다. 선택한 이름을 다시 누르면 바꿀 수 있습니다.",0,y,w,1000,Look.SmallText,muted);skin.ApplyLabel(hint,false,muted);
            float helpHeight=hint.preferredHeight+5;Place(hint.rectTransform,0,y,w,helpHeight);y+=helpHeight+gap/2;
            if(!persistentPresets)
            {
                var note=Text(body,"훈련 편집안은 실제 캐릭터에 저장되지 않습니다.",0,y,w,1000,Look.SmallText,gold);
                float h=note.preferredHeight+4;Place(note.rectTransform,0,y,w,h);y+=h+gap/2;
            }
            float importHeight=sharing?Look.Button:0,importY=bodyHeight-importHeight;
            mainScroll=Scroll(body,"Preset rows",0,y,w,Mathf.Max(1,importY-y-(sharing?gap:0)),out var list);float rowWidth=Mathf.Max(1,w-12);
            float rowHeight=Look.Button+6,saveSize=Look.Button-12,shareWidth=sharing?Mathf.Min(Look.ActionMin,rowWidth*.34f):0;
            for(int i=0;i<5;i++)
            {
                int slot=i;var preset=hero.edictPresets[i];bool exists=HuntEdictPreset.HasValue(preset),current=i==Session.SelectedSlot;
                var holder=Row(list,"Preset holder "+(i+1),rowHeight+2);
                var row=Panel(holder,"Preset "+(i+1),current?"preset-current":exists?"preset-idle":"preset-empty");Place(row,0,0,rowWidth,rowHeight);
                var number=Panel(row,"Preset number",current?"preset-number-current":"number-round");number.GetComponent<Image>().raycastTarget=false;Place(number,8,(rowHeight-34)/2,34,34);
                Text(number,(i+1).ToString(),0,0,34,34,17,current?skin.White:muted,TextAnchor.MiddleCenter);
                float shareX=rowWidth-shareWidth-8,saveX=(sharing?shareX-gap:rowWidth-8)-saveSize,nameWidth=Mathf.Max(1,Mathf.Min(320,rowWidth*.42f,saveX-50-gap)),nameHeight=Look.Button-12;
                if(exists&&renamingPreset==slot)
                {
                    var input=Input(row,renamingText,50,(rowHeight-nameHeight)/2,nameWidth,nameHeight);input.name="edict-preset-name-input";input.characterLimit=40;
                    input.textComponent.alignment=TextAnchor.MiddleLeft;input.textComponent.fontSize=Mathf.RoundToInt(Look.BodyText+2);skin.ApplyLabel(input.textComponent,true,textColor);
                    input.onValueChanged.AddListener(value=>renamingText=value);
                    input.onEndEdit.AddListener(value=>{if(input.gameObject.activeInHierarchy)FinishPresetName(slot,value);});
                    input.ActivateInputField();
                }
                else if(exists)
                {
                    var name=Button(row,preset.name,50,(rowHeight-nameHeight)/2,nameWidth,nameHeight,()=>
                    {
                        if(Session.SelectedSlot==slot&&persistentPresets){renamingPreset=slot;renamingText=hero.edictPresets[slot].name;Repaint();}
                        else ChoosePreset(slot);
                    },"input-well",Look.BodyText+2);name.name="edict-preset-name-"+slot;SurfaceChoice(name,false,idleSkin:"input-well");
                    var label=name.GetComponentInChildren<Text>();label.alignment=TextAnchor.MiddleLeft;
                    label.horizontalOverflow=HorizontalWrapMode.Overflow;name.gameObject.AddComponent<RectMask2D>();Place(label.rectTransform,7,0,Mathf.Max(1,nameWidth-14),nameHeight);
                    if(preset.needsReview)Text(row,"!",saveX-12,0,10,rowHeight,Look.SmallText,UiTheme.Danger);
                }
                else {var empty=Text(row,"비어 있음",50,0,nameWidth,rowHeight,Look.BodyText+2,skin.Edge);skin.ApplyLabel(empty,true,skin.Edge);}
                var saveButton=Button(row,"프리셋 저장",saveX,(rowHeight-saveSize)/2,saveSize,saveSize,()=>RequestLeave(()=>StoreCurrentPreset(slot)),"button-idle");
                saveButton.name="edict-preset-store-"+slot;saveButton.GetComponentInChildren<Text>().enabled=false;
                float saveGlyph=Mathf.Min(24,saveSize-8);GroupGlyph(saveButton.transform,"save",(saveSize-saveGlyph)/2,(saveSize-saveGlyph)/2,saveGlyph,skin.OrangeInk);saveButton.interactable=persistentPresets;
                var share=Button(row,"공유 코드 보기",shareX,(rowHeight-nameHeight)/2,shareWidth,nameHeight,()=>SharePreset(slot),"button-idle",Look.BodyText+1);
                share.name="edict-preset-share-"+slot;share.gameObject.SetActive(exists&&sharing);
            }
            EndList(list,gap/2);
            var importButton=Button(body,"코드 가져오기",0,importY,w,Mathf.Max(1,importHeight),()=>RequestLeave(ImportCode),"button-idle",Look.BodyText+3);importButton.name="edict-preset-import";importButton.interactable=persistentPresets;importButton.gameObject.SetActive(sharing);
        }
        void FinishPresetName(int slot,string value)
        {
            if(renamingPreset!=slot)return;renamingPreset=-1;
            if(value==hero.edictPresets[slot].name){Repaint();return;}
            if(!store.RenameHuntEdictPreset(hero.id,slot,value)){Repaint();Error(store.Error);return;}
            Repaint();
        }
        // Keep the existing mode chip above both pages, outside the locked quick-preset list.
        float DrawEdictSwitch(Transform parent,float w)
        {
            if(!HasFeature(HuntEdictProgression.Sharing))
            {
                var note=Body(parent,"추천 모드 전환은 균열 20단계부터 공개됩니다.",0,0,w,Look.SmallText,1.5f,muted,out float h);
                note.name="edict-default-locked";return h+Look.Gap;
            }
            bool enabled=hero.useRecommendedEdict;float fs=Look.BodyText+2,chip=Look.Chip,border=enabled?3:2;
            var button=Button(parent,Loc.T("기본 세팅 적용"),0,0,w,chip,ToggleEdictActive,enabled?"chip-on":"button-idle",fs);
            button.name="edict-default-settings";button.interactable=persistentPresets;
            var label=button.GetComponentInChildren<Text>();label.alignment=TextAnchor.MiddleLeft;label.horizontalOverflow=HorizontalWrapMode.Overflow;float labelWidth=label.preferredWidth;
            float pillHeight=Look.SmallText*JuaLine+8;var value=Panel(button.transform,"Default settings state",enabled?"switch-on":"default-off");value.GetComponent<Image>().raycastTarget=false;
            var state=Line(value,enabled?"ON":"OFF",0,0,200,Look.SmallText,pillHeight,enabled?skin.White:muted,true,TextAnchor.MiddleCenter);float pillWidth=state.preferredWidth+24;
            float buttonWidth=Mathf.Min(w,border+16+labelWidth+10+pillWidth+8+border);
            Place((RectTransform)button.transform,0,0,buttonWidth,chip);Place(label.rectTransform,border+16,0,labelWidth+4,chip);
            Place(value,border+16+labelWidth+10,(chip-pillHeight)/2,pillWidth,pillHeight);Place(state.rectTransform,0,0,pillWidth,pillHeight);
            float beside=w-buttonWidth-Look.Gap;bool inline=beside>=240;float messageWidth=inline?beside:w;
            var message=Body(parent,enabled?"기본 추천 세팅이 적용 중입니다. 사용자 설정이 적용되지 않습니다":"사용자 설정이 적용되고 있습니다.",inline?buttonWidth+Look.Gap:0,inline?0:chip+Look.Gap,messageWidth,Look.SmallText,1.5f,enabled?skin.GreenInk:muted,out float messageHeight);
            message.name="edict-default-status";if(enabled)message.fontStyle=FontStyle.Bold;
            if(inline)Place(message.rectTransform,buttonWidth+Look.Gap,(chip-messageHeight)/2,messageWidth,messageHeight);
            return (inline?chip:chip+Look.Gap+messageHeight)+Look.Gap;
        }
        void ToggleEdictActive()
        {
            if(!persistentPresets||!HasFeature(HuntEdictProgression.Sharing))return;
            if(!store.SetRecommendedEdict(hero.id,!hero.useRecommendedEdict,catalog)){Error(store.Error);return;}
            simulation?.RefreshEdict();Repaint();
        }
        void ChoosePreset(int slot)=>RequestLeave(()=>
        {
            var preset=hero.edictPresets[slot];
            void Select()
            {
                try
                {
                    var copy=preset.Copy();copy.needsReview=false;Session.SelectPreset(copy,slot);
                    if(!Session.Draft.UsesTree)
                    {var staged=JsonUtility.FromJson<HeroSave>(JsonUtility.ToJson(hero));GameStore.ApplyHuntEdict(staged,Session.Draft);staged.build.classSkills=null;Session.UseSkillTree(staged);}
                    Repaint();
                    try{HuntEdictLoadout.ValidateForHero(Session.Draft,hero,catalog);}
                    catch(Exception e){Error(e.Message);}
                }
                catch(Exception e){Error(e.Message);}
            }
            if(preset.needsReview)Confirm("이전 설정은 원본으로 보관됩니다. 항상 자동 사용하는 새 설정으로 변환해 편집하시겠습니까?",Select);
            else Select();
        });
        void StoreCurrentPreset(int slot)
        {
            var existing=hero.edictPresets[slot];bool exists=HuntEdictPreset.HasValue(existing);
            void Store()
            {
                var preset=new HuntEdictPreset{emptySlot=false,name=exists?existing.name:Loc.F("프리셋 {0}",slot+1),loadout=Session.Draft.Copy(),equipmentIds=hero.inventory.Where(i=>i.equipped).Select(i=>i.id).ToList(),equipmentPositions=hero.inventory.Where(i=>i.equipped).Select(i=>i.equipIndex).ToList()};
                if(!store.StoreHuntEdictPreset(hero.id,slot,preset,exists)){Error(store.Error);return;}
                Session.AcceptSaved(hero);Repaint();
            }
            if(exists)Confirm("이미 해당 슬롯에 프리셋이 존재합니다. 덮어쓰시겠습니까?",Store);else Store();
        }
        void SharePreset(int slot)
        {
            try
            {
                var preset=hero.edictPresets[slot];string code=HuntEdictPresetCodec.Encode(preset);
                float top=Look.Close+Look.Gap,inputHeight=Mathf.Max(100,Look.Button*2),buttonY=top+inputHeight+Look.Gap;
                Dialog(Loc.F("{0} · 공유 코드",preset.name),p=>
                {
                    float shift=DialogTop-top;top=DialogTop;buttonY+=shift;
                    var input=Input(p,code,12,top,DialogWidth-24,inputHeight,true);input.readOnly=true;
                    Button(p,"코드 복사",DialogWidth-12-Look.ActionMin,buttonY,Look.ActionMin,Look.Button,()=>{GUIUtility.systemCopyBuffer=code;DismissDialog();},"button-primary",Look.BodyText+3);
                },buttonY+Look.Button+Look.Gap);
            }
            catch(Exception e){Error(e.Message);}
        }
        void ImportCode()
        {
            float top=Look.Close+Look.Gap,inputHeight=Mathf.Max(90,Look.Button*2),buttonY=top+inputHeight+Look.Gap;
            Dialog("코드 가져오기",p=>
            {
                float shift=DialogTop-top;top=DialogTop;buttonY+=shift;
                var input=Input(p,"",12,top,DialogWidth-24,inputHeight,true);
                Button(p,"붙여넣기",12,buttonY,Look.ActionMin,Look.Button,()=>input.text=GUIUtility.systemCopyBuffer,"button-idle",Look.BodyText+3);
                Button(p,"확인",DialogWidth-12-Look.ActionMin,buttonY,Look.ActionMin,Look.Button,()=>
                {
                    HuntEdictPresetPreview preview;try{preview=HuntEdictPresetImport.Prepare(input.text,hero,catalog);}catch(Exception e){Error(e.Message);return;}
                    ImportPreview(preview);
                },"button-primary",Look.BodyText+3);
            },buttonY+Look.Button+Look.Gap);
        }
        void ImportPreview(HuntEdictPresetPreview preview)
        {
            float h=Mathf.Min(height-20,Mathf.Max(285,Look.Close+Look.Gap*3+Look.Button+150)),top=Look.Close+Look.Gap;
            Dialog("가져온 프리셋 확인",p=>
            {
                top=DialogTop;
                Scroll(p,"Import details",12,top,DialogWidth-24,Mathf.Max(1,h-top-Look.Button-Look.Gap*2),out var list);
                Paragraph(list,preview.Draft.name,DialogWidth-38,Look.ChoiceTitle,textColor);
                Paragraph(list,Loc.F("사용 포인트 {0} P",preview.Draft.loadout.Spent),DialogWidth-38,Look.BodyText);
                foreach(string change in preview.Changes)Paragraph(list,change,DialogWidth-38,Look.SmallText,muted);
                if(preview.Changes.Count==0)Paragraph(list,"코드를 확인했습니다. 저장할 프리셋 위치를 선택하세요.",DialogWidth-38,Look.BodyText);
                EndList(list,Look.Gap);
                Button(p,preview.RequiresLegacyConsent?"변환 내용에 동의하고 계속":"저장 위치 선택",12,h-Look.Button-Look.Gap,DialogWidth-24,Look.Button,()=>ImportDestinations(preview),"button-primary",Look.BodyText+3);
            },h);
        }
        void ImportDestinations(HuntEdictPresetPreview preview)
        {
            float h=Mathf.Min(height-16,Look.Close+Look.Gap*2+5*(Look.Button+8)),top=Look.Close+Look.Gap;
            Dialog("저장할 프리셋을 선택하세요",p=>
            {
                top=DialogTop;
                Scroll(p,"Preset destinations",12,top,DialogWidth-24,Mathf.Max(1,h-top-Look.Gap),out var list);
                for(int i=0;i<5;i++)
                {
                    int slot=i;var old=hero.edictPresets[i];bool exists=HuntEdictPreset.HasValue(old);
                    var row=Row(list,"Import destination "+i,Look.Button+6);
                    Button(row,Loc.F("{0}. {1}",i+1,exists?old.name:Loc.T("비어 있음")),0,0,DialogWidth-40,Look.Button+6,()=>
                    {
                        void Store()
                        {
                            if(!store.StoreHuntEdictPreset(hero.id,slot,preview.Draft,exists,preview.RequiresLegacyConsent)){Error(store.Error);return;}
                            Session.AcceptSaved(hero);Repaint();
                        }
                        if(exists)Confirm("이미 해당 슬롯에 프리셋이 존재합니다. 덮어쓰시겠습니까?",Store);else Store();
                    },i==hero.selectedEdictPreset?"preset-current":exists?"preset-idle":"preset-empty",Look.BodyText+1);
                }
                EndList(list,2);
            },h);
        }
    }
}
