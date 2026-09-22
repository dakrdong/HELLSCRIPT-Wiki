using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed partial class GameUI
    {
        readonly List<Button> presetSaveButtons=new List<Button>();
        Text presetSaveHint;
        Action presetClose;
        RectTransform presetModal,presetSafeRoot,presetCard,presetSummary,presetEntry,presetMain,presetExit;
        DialogScrollRect presetReadingPane;
        RectTransform presetSummaryBody,presetEntryBody;
        readonly List<Transform> presetSummaryRows=new List<Transform>();
        Vector2 presetLayoutSize;
        float presetKeyboardHeight;
        Rect presetSafeArea;
        Vector2 presetScreenSize;
        InputField presetNameField;
        int presetEscapeFrame=-1;
        bool PresetSourceSaved=>editing!=null&&!comparisonEditing&&BuildEditing.SettingsEqual(editing,game.Active?game.Combat.State.build:game.Store.Data.Hero.build);
        void RefreshPresetSaveButtons()
        {
            if(Page!="build")return;bool saved=PresetSourceSaved;
            foreach(var button in presetSaveButtons)if(button!=null)button.interactable=saved;
            if(presetSaveHint!=null)presetSaveHint.text=Loc.T(comparisonEditing?"B 훈련을 마치면 결과에서 저장할 수 있습니다.":saved?"현재 저장된 설정을 이름과 함께 보관합니다. 불러온 설정은 적용 전까지 편집본입니다.":"먼저 ‘설정 적용’을 완료한 뒤 프리셋으로 저장하세요. 기존 슬롯의 불러오기와 이름 변경은 가능합니다.");
        }
        void RenderPresetRows()
        {
            presetSaveButtons.Clear();Note(content,"프리셋 · 캐릭터별 5개",24,50,gold);
            presetSaveHint=Note(content,"",20,105,pale);
            var hero=game.Store.Data.Hero;
            for(int i=0;i<HuntEdict.PresetSlots;i++)
            {
                int slot=i;var existing=hero.presets.ElementAtOrDefault(slot);bool occupied=BuildEditing.HasPreset(existing);
                var row=Row(content,142);var title=Label(row,Loc.F("슬롯 {0} · {1}", i+1, (occupied?existing.name:"비어 있음")),22,occupied?pale:muted);
                title.supportRichText=false;title.rectTransform.anchorMin=new Vector2(0,.54f);title.rectTransform.anchorMax=Vector2.one;title.rectTransform.offsetMin=new Vector2(16,4);title.rectTransform.offsetMax=new Vector2(-16,-4);
                var save=Button(row,Loc.F("슬롯 {0} 저장", i+1),()=>
                {
                    if(!PresetSourceSaved)return;var source=(game.Active?game.Combat.State.build:hero.build).Copy();
                    source.equipmentIds=(game.Active?game.Combat.Hero:hero).inventory.Where(item=>item.equipped).Select(item=>item.id).ToList();
                    source.equipmentPositions=(game.Active?game.Combat.Hero:hero).inventory.Where(item=>item.equipped).Select(item=>item.equipIndex).ToList();
                    ShowPresetNameDialog(slot,source,name=>game.Store.SaveBuildPreset(hero.id,slot,source,name),RenderBuild);
                });presetSaveButtons.Add(save);
                var load=Button(row,Loc.F("슬롯 {0} 불러오기", i+1),()=>{if(occupied)LoadEditing(existing);});load.interactable=occupied;
                var rename=Button(row,"이름 변경",()=>ShowPresetNameDialog(slot,existing,name=>game.Store.RenameBuildPresets(hero.id,new Dictionary<int,string>{{slot,name}}),RenderBuild,true));rename.interactable=occupied&&!comparisonEditing;
                var buttons=new[]{save,load,rename};for(int n=0;n<3;n++)
                {var rect=(RectTransform)buttons[n].transform;rect.anchorMin=new Vector2(n/3f,0);rect.anchorMax=new Vector2((n+1)/3f,.52f);rect.offsetMin=new Vector2(6,8);rect.offsetMax=new Vector2(-6,-4);}
            }
            RefreshPresetSaveButtons();
        }
        RectTransform PresetScroll(Transform parent,string name,out RectTransform body)
        {
            var frame=Box(name,parent,panel);var scroll=frame.gameObject.AddComponent<DialogScrollRect>();scroll.readingStarted=()=>presetReadingPane=scroll;scroll.horizontal=false;scroll.movementType=ScrollRect.MovementType.Clamped;
            var viewport=Rect("Viewport",frame);Stretch(viewport);viewport.gameObject.AddComponent<RectMask2D>();scroll.viewport=viewport;
            body=Rect("Content",viewport);body.anchorMin=new Vector2(0,1);body.anchorMax=Vector2.one;body.pivot=new Vector2(.5f,1);body.offsetMin=body.offsetMax=Vector2.zero;
            var layout=body.gameObject.AddComponent<VerticalLayoutGroup>();layout.childControlWidth=true;layout.childControlHeight=true;layout.childForceExpandWidth=true;layout.childForceExpandHeight=false;layout.spacing=12;layout.padding=new RectOffset(16,16,16,16);
            var fit=body.gameObject.AddComponent<ContentSizeFitter>();fit.verticalFit=ContentSizeFitter.FitMode.PreferredSize;scroll.content=body;return frame;
        }
        Text PresetNote(Transform parent,string text,int size=22)
        {
            var label=Label(parent,text,size,pale);label.supportRichText=false;label.gameObject.AddComponent<LayoutElement>().minHeight=32;
            return label;
        }
        void ShowPresetNameDialog(int slot,BuildConfig source,Func<string,bool> save,Action completed,bool rename=false)
        {
            if(presetModal!=null||source==null)return;
            var module=EventSystem.current?.currentInputModule;
            if(module!=null&&module.inputOverride==null){if(!module.TryGetComponent(out HellscriptTextInput input))input=module.gameObject.AddComponent<HellscriptTextInput>();module.inputOverride=input;}
            var previous=game.Store.Data.Hero.presets.ElementAtOrDefault(slot);bool occupied=BuildEditing.HasPreset(previous);
            string initial=rename?previous.name:source.name;
            // A root canvas gives the text generator the actual display scale. Scaling a child
            // of the legacy portrait canvas magnifies a low-resolution glyph atlas in small windows.
            presetModal=Rect("Preset modal",transform);
            var canvas=presetModal.gameObject.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=110;canvas.pixelPerfect=true;
            ApplyScaler(presetModal.gameObject.AddComponent<CanvasScaler>(),true);
            presetModal.gameObject.AddComponent<GraphicRaycaster>();presetModal.gameObject.AddComponent<Image>().color=new Color(0,0,0,.88f);
            presetSafeRoot=Rect("Preset safe area",presetModal);Stretch(presetSafeRoot);
            presetReadingPane=null;
            presetCard=Box("Preset dialog",presetSafeRoot,ink);presetCard.anchorMin=presetCard.anchorMax=new Vector2(.5f,.5f);presetCard.pivot=new Vector2(.5f,.5f);
            var heading=Label(presetCard,rename?Loc.F("슬롯 {0} 이름 변경", slot+1):Loc.F("슬롯 {0} 프리셋 저장", slot+1),26,gold);Place(heading.rectTransform,18,12,600,46);heading.rectTransform.anchorMax=new Vector2(1,1);heading.rectTransform.sizeDelta=new Vector2(-36,46);
            presetSummary=PresetScroll(presetCard,"설정 요약",out var summary);presetSummaryBody=summary;
            string Skills(BuildConfig b)=>string.Join(" · ",b.activeSkills.Select(id=>game.catalog.skills[id].name));
            PresetNote(summary,occupied?Loc.F("기존 슬롯\n{0}", previous.name):"비어 있는 슬롯입니다.");
            if(rename)PresetNote(summary,"이름만 바꿉니다. 스킬·행동·패시브·장비 보호 참조는 유지됩니다.");
            else
            {
                PresetNote(summary,Loc.F("보관할 스킬\n{0}", Skills(source)));
                PresetNote(summary,Loc.F("이동 거리 {0:0.#}m · 행동 규칙 {1}개\n패시브 {2}개 · 장비 참조 {3}개", source.distance, source.rules.Count, source.passives.Length, source.equipmentIds.Count));
                if(occupied){var changes=BuildEditing.DescribeCombatChanges(previous,source,game.catalog,game.Store.Data.Hero.heroClass);PresetNote(summary,Loc.F("덮어쓸 내용\n{0}", (changes.Count==0?"전투 행동은 같습니다.":string.Join("\n\n",changes))));}
                PresetNote(summary,"현재 사냥 설정과 장비 장착은 바뀌지 않습니다.");
            }
            presetSummaryRows.Clear();foreach(Transform child in summary)presetSummaryRows.Add(child);
            presetEntry=PresetScroll(presetCard,"이름 입력",out var entry);presetEntryBody=entry;PresetNote(entry,"프리셋 이름 · 표시 문자 1~24자");
            var frame=Box("Preset name field",entry,new Color(.12f,.15f,.19f));var fieldSize=frame.gameObject.AddComponent<LayoutElement>();fieldSize.preferredHeight=72;fieldSize.minHeight=72;
            var field=frame.gameObject.AddComponent<InputField>();presetNameField=field;field.lineType=InputField.LineType.SingleLine;field.characterLimit=512;
            var value=Label(frame,"",24,pale);Inset(value.rectTransform,14,14,8,8);value.supportRichText=false;field.textComponent=value;
            var placeholder=Label(frame,"이름을 입력하세요",24,muted);Inset(placeholder.rectTransform,14,14,8,8);field.placeholder=placeholder;field.text=initial;
            var status=PresetNote(entry,"저장을 눌러야 슬롯에 반영됩니다.",20);
            presetMain=Rect("Preset actions",presetCard);presetExit=Box("Preset exit choices",presetCard,ink);presetExit.gameObject.SetActive(false);
            void Close()=>ClosePresetDialog();
            void Save()
            {
                string valid;try{valid=PresetNames.Validate(field.text);}catch(Exception e){status.text=Loc.T(e.Message);Main();return;}
                if(!save(valid)){status.text=Loc.T("프리셋을 저장하지 못했습니다. 입력한 이름은 유지됩니다. 현재 캐릭터와 저장 상태를 확인한 뒤 다시 시도하세요.");Main();return;}
                Close();completed();ShowToast(Loc.F("슬롯 {0}{1}", slot+1, (rename?" 이름을 저장했습니다.":"에 프리셋을 저장했습니다.")));
            }
            void Main(){presetExit.gameObject.SetActive(false);presetMain.gameObject.SetActive(true);}
            void Exit()
            {
                if(presetExit.gameObject.activeSelf){Main();return;}
                if(field.text==initial){Close();return;}
                presetMain.gameObject.SetActive(false);presetExit.gameObject.SetActive(true);
            }
            presetClose=Exit;
            var cancel=Button(presetMain,"닫기",Exit);var commit=Button(presetMain,rename?"이름 저장":occupied?"이 슬롯에 덮어쓰기":"프리셋 저장",Save,gold*.55f);
            commit.interactable=!rename;
            AnchorButton(cancel,0,2);AnchorButton(commit,1,2);
            var choices=new[]{Button(presetExit,"저장하고 닫기",Save,gold*.55f),Button(presetExit,"저장하지 않고 닫기",Close),Button(presetExit,"계속 편집하기",Main)};
            for(int n=0;n<choices.Length;n++)AnchorButton(choices[n],n,choices.Length);
            string typed=initial;
            void Changed(){status.text=Loc.T(field.text==initial?"저장을 눌러야 슬롯에 반영됩니다.":Loc.F("슬롯 {0} 이름에 저장하지 않은 변경이 있습니다.", slot+1));commit.interactable=!rename||field.text!=initial;}
            field.onValueChanged.AddListener(value=>{if(!field.wasCanceled)typed=value;Changed();});
            // uGUI normally restores its focus-time text on Escape. Preserve the draft for the three-way exit choice.
            field.onEndEdit.AddListener(_=>{if(field.wasCanceled){field.SetTextWithoutNotify(typed);Changed();HandlePresetEscape();}});
            presetLayoutSize=Vector2.zero;ReflowPresetDialog();
        }
        static void AnchorButton(Button b,int index,int count)
        {var r=(RectTransform)b.transform;r.anchorMin=new Vector2(index/(float)count,0);r.anchorMax=new Vector2((index+1f)/count,1);r.offsetMin=new Vector2(5,5);r.offsetMax=new Vector2(-5,-5);}
        void ReflowPresetDialog()
        {
            if(presetCard==null)return;
            var screenSize=new Vector2(Screen.width,Screen.height);var safe=UiSafeArea.Current;
            Rect keyboardArea=TouchScreenKeyboard.visible?TouchScreenKeyboard.area:new Rect();
            float keyboardTop=keyboardArea.width>0&&keyboardArea.xMin<safe.xMax&&keyboardArea.xMax>safe.xMin?Mathf.Clamp(keyboardArea.yMax,safe.yMin,safe.yMax):safe.yMin;
            if(presetLayoutSize!=Vector2.zero&&presetScreenSize==screenSize&&presetSafeArea==safe&&Mathf.Abs(presetKeyboardHeight-keyboardTop)<1)return;
            Canvas.ForceUpdateCanvases();
            var entryAnchor=DialogReadingAnchor.Capture(presetEntry.GetComponent<ScrollRect>());
            var summaryAnchor=presetSummary.gameObject.activeSelf?DialogReadingAnchor.Capture(presetSummary.GetComponent<ScrollRect>()):null;
            bool readingSummary=presetReadingPane!=null&&presetReadingPane.transform==presetSummary;
            bool hadLayout=presetLayoutSize!=Vector2.zero;
            presetScreenSize=screenSize;presetSafeArea=safe;presetKeyboardHeight=keyboardTop;
            presetSafeRoot.anchorMin=new Vector2(safe.xMin/Screen.width,keyboardTop/Screen.height);
            presetSafeRoot.anchorMax=new Vector2(safe.xMax/Screen.width,safe.yMax/Screen.height);presetSafeRoot.offsetMin=presetSafeRoot.offsetMax=Vector2.zero;
            Canvas.ForceUpdateCanvases();Vector2 available=presetSafeRoot.rect.size;presetLayoutSize=available;
            float w=Mathf.Min(1000,Mathf.Max(1,available.x-24)),h=Mathf.Min(720,Mathf.Max(1,available.y-24));
            presetCard.sizeDelta=new Vector2(w,h);presetCard.anchoredPosition=Vector2.zero;
            bool wide=w>=840;float top=70,bottom=82,body=Mathf.Max(1,h-top-bottom);
            foreach(var row in presetSummaryRows)if(row.parent!=(wide?presetSummaryBody:presetEntryBody))row.SetParent(wide?presetSummaryBody:presetEntryBody,false);
            presetSummary.gameObject.SetActive(wide);
            if(wide){Place(presetSummary,12,top,(w-36)*.52f,body);Place(presetEntry,24+(w-36)*.52f,top,(w-36)*.48f,body);}
            else Place(presetEntry,12,top,w-24,body);
            Place(presetMain,12,h-76,w-24,64);Place(presetExit,12,h-76,w-24,64);
            Canvas.ForceUpdateCanvases();
            if(hadLayout)
            {
                // If two panes merge, keep the pane the player was reading. If they split,
                // each anchor follows its existing content object into its new scroll area.
                if(wide){summaryAnchor?.Restore();entryAnchor?.Restore();}
                else if(readingSummary&&summaryAnchor!=null)summaryAnchor.Restore();
                else entryAnchor?.Restore();
                if(presetNameField!=null&&presetNameField.isFocused)DialogReadingAnchor.Show(presetNameField.transform as RectTransform);
            }
        }
        void ClosePresetDialog()
        {
            presetClose=null;presetNameField=null;presetReadingPane=null;var old=presetModal;
            presetModal=presetSafeRoot=presetCard=presetSummary=presetEntry=presetMain=presetExit=null;
            if(old!=null){old.gameObject.SetActive(false);Destroy(old.gameObject);}
        }
        void UpdatePresetDialog()
        {if(presetModal==null)return;ReflowPresetDialog();if(!CommonPanelOpen&&Keyboard.current?.escapeKey.wasPressedThisFrame==true&&presetNameField?.isFocused!=true)HandlePresetEscape();}
        void HandlePresetEscape(){if(presetEscapeFrame==Time.frameCount)return;presetEscapeFrame=Time.frameCount;presetClose?.Invoke();}
    }
}
