using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    // Settings for the Rift gear offer cards: shown rarities, equipment positions (each with optional required options),
    // the offer rule and how long a card stays. Edits go to a draft; Save is one atomic hero-save transaction.
    // Exposes the open window's draft (what is chosen, not yet saved) to the guide tutorial.
    // Drawn in the mockup's own CSS pixels (board "4 · 추천 장비 설정"): one cream card with a dashed inner ring, the gilt title and a round close button,
    // a body that scrolls and a pinned Save / Close row. The card is 12 px in from the sides on a portrait screen and 560 wide in the middle otherwise.
    public sealed class RiftGearSettingsProbe:MonoBehaviour{public RiftGearOfferSettings Draft;public bool BetterChosen,Saved;}
    public static partial class RiftGearSettingsWindow
    {
        public static ContentWindowView Open(Transform parent,GameStore store,GameCatalog catalog,Func<float> readingScale,Action<string> notify,Action closed,bool chooseBetter=false,CombatSimulation simulation=null)
        {
            var draft=RuneGrowth.Copy(store.Data.Hero.riftGearOffer);draft.Normalize();
            if(chooseBetter)draft.allWearable=true; // Only the detached lesson draft changes before Save.
            var memory=new Memory();var edict=new EdictSettingsSection(store,catalog,"autoEquip",simulation);
            ContentWindowView window=null;
            window=ContentWindowView.Open(parent,"추천 장비 설정",EquipmentViewSource.Owned,readingScale,v=>
            {
                // The guide stage reads the unsaved choice from here to lead the next step.
                var probe=v.GetComponent<RiftGearSettingsProbe>();if(probe==null)probe=v.gameObject.AddComponent<RiftGearSettingsProbe>();probe.Draft=draft;
                new Sheet(v,readingScale,draft,memory,store,notify,probe,chooseBetter,edict).Render();
            },closed,back:()=>{if(!chooseBetter)window?.Close();},skin:RiftGearSkin.Current,fit:Fit);
            return window;
        }
        // The window is the whole safe area; the plate it draws is placed inside it.
        static (float scale,Rect plate) Fit(Rect safe){float s=UiMock.WindowScale(safe);return (s,new Rect(safe.x/s,safe.y/s,safe.width/s,safe.height/s));}
        static string StatName(int position,string stat)
            =>stat=="main"?position==1?"기본 수치":EquipmentShopWindow.MainName(RiftGearOfferSettings.Slot(position)):StatCatalog.Name(ItemCatalog.Affix(stat).stat);

        // What a repaint keeps: the expanded range, the open option list and the body's scroll offset.
        sealed class Memory{public string expanded;public bool adding;public float offset;}

        sealed partial class Sheet
        {
            readonly ContentWindowView view;readonly MockKit kit;readonly UiSkin skin;readonly RiftGearOfferSettings draft;readonly Memory state;readonly GameStore store;readonly Action<string> notify;readonly RiftGearSettingsProbe probe;readonly bool lesson;
            readonly EdictSettingsSection edict;
            public Sheet(ContentWindowView view,Func<float> reading,RiftGearOfferSettings draft,Memory state,GameStore store,Action<string> notify,RiftGearSettingsProbe probe,bool lesson,EdictSettingsSection edict)
            {this.view=view;skin=view.Skin;kit=new MockKit(view.Frame,skin,reading);this.draft=draft;this.state=state;this.store=store;this.notify=notify;this.probe=probe;this.lesson=lesson;this.edict=edict;}
            bool Allowed(string name)=>!lesson||name=="rift-gear-rule-better"||name=="rift-gear-settings-save"&&probe.BetterChosen&&!draft.allWearable;
            UiButton Press(RectTransform rect,Action action)
            {var button=kit.Press(rect,()=>{if(Allowed(rect.name))action();});button.interactable=Allowed(rect.name);return button;}
            void CloseFromInput(){if(!lesson)view.Close();}

            public void Render()
            {
                var frame=view.Frame;float W=frame.rect.width,H=frame.rect.height;
                var backdrop=view.transform.Find("Backdrop");if(backdrop!=null)backdrop.GetComponent<Image>().color=Veil;
                bool portrait=H>W;
                float pw=portrait?W-24:Mathf.Min(560,W-16),ph=portrait?H-60:Mathf.Min(780,H-16),px=portrait?12:(W-pw)/2,py=portrait?30:(H-ph)/2;
                var card=kit.Box(frame,"Rift gear settings",px,py,pw,ph,skin.CreamShade,Navy,20,3,5,hit:true);
                var clip=kit.Clip(card,"Panel glow",3,3,pw-6,ph-6);kit.Glow(clip,(pw-6)/2,0,.9f*(pw-6),.6f*(ph-6),Color.white,.7f);
                kit.Dashed(card,9,9,pw-18,ph-18,14,2,Trim,6,6);
                // the gilt knob on the top edge: a navy ring with a 2 px drop around a gold disc with a white glint
                kit.Box(card,"Knob ring",pw/2-11.5f,-10.5f,23,23,Navy,Navy,11.5f,0,2);
                var knob=kit.Box(card,"Knob",pw/2-9,-8,18,18,skin.Gold,Color.clear,9,0,0,UiTheme.MockCoin);kit.Box(knob,"Knob glint",3.3f,2.4f,6,6,Color.white,Color.clear,3,0,0);
                // header: gilt title, round close button, the gilt rule under them
                var title=Jua(card,Loc.T("추천 장비 설정"),21,21.2f,pw-90,22,skin.Gold,TextAnchor.MiddleLeft,true);title.name="Window title";Gilt(title);
                var close=kit.Box(card,"content-close",pw-57,11,44,44,Color.white,Navy,22,3,3);var face=close.GetComponent<StorageSurface>();
                var closer=close.gameObject.AddComponent<UiButton>();closer.targetGraphic=face;face.raycastTarget=true;closer.Configure(UiButtonRole.Action);skin.ApplyChrome(closer,true);closer.SetCasualAppearance(22,3,3,Navy);
                if(closer.Face!=null)closer.Face.rectTransform.offsetMin=new Vector2(0,-3);
                kit.Gly(close,"close",13,13,18,skin.White);closer.onClick.AddListener(CloseFromInput);closer.interactable=!lesson;
                var rule=UiLayout.Rect("Header rule",card);UiLayout.Place(rule,17,63,pw-34,1);var fade=rule.gameObject.AddComponent<GearSettingsBar>();fade.raycastTarget=false;
                var gild=UiTheme.MockGildLine;var clear=new Color(gild.r,gild.g,gild.b,0);fade.Set(new[]{clear,gild,gild,clear},new[]{0f,.2f,.8f,1f});
                // body: scrolls between the rule and the pinned buttons (64 px below the card top, 66 above its bottom)
                float cw=pw-34,viewH=Mathf.Max(60,ph-133),contentH=Probe(t=>Body(t,cw));
                var scroll=kit.Scroll(card,"Content scroll",3,64,pw-6,viewH,contentH,out var content);view.Adopt(scroll,content);
                Body(content,cw);
                if(state.offset>0)content.anchoredPosition=new Vector2(0,Mathf.Clamp(state.offset,0,Mathf.Max(0,contentH-viewH)));else scroll.verticalNormalizedPosition=1; // a ScrollRect opens at its end
                scroll.onValueChanged.AddListener(_=>state.offset=content.anchoredPosition.y);
                // Save (green, 3 px ring) and Close (white) share the row; flex:1 leaves each its own border on top of an equal share
                float fy=ph-61,free=cw-8-10,saveW=6+free/2,closeW=4+free/2;
                var save=kit.Box(card,"rift-gear-settings-save",17,fy,saveW,44,skin.Green,Navy,12,3,3,skin.GreenShade);Press(save,Save);
                CasualBattleHudStyle.Outline(Jua(save,Loc.T("저장"),0,15,saveW,14,Color.white,TextAnchor.MiddleCenter,true),1);
                var cancel=kit.Box(card,"rift-gear-settings-close",17+saveW+8,fy,closeW,44,Color.white,Navy,12,2,3);Press(cancel,CloseFromInput);
                Jua(cancel,Loc.T("닫기"),0,15,closeW,14,Navy,TextAnchor.MiddleCenter,true);
            }
            void Save()
            {
                if(!Allowed("rift-gear-settings-save"))return;
                if(store.SaveRiftGearOfferSettings(Guid.NewGuid().ToString("N"),draft)){probe.Saved=true;notify(Loc.T("추천 장비 설정을 저장했습니다."));view.Close();}
                else notify(Loc.T(store.Error));
            }
            // The body is measured once off screen, then drawn into the scroll content; both passes are the same drawing.
            float Probe(Func<Transform,float> draw)
            {
                var host=UiLayout.Rect("Measure",kit.Root);UiLayout.Place(host,-4000,-4000,10,10);float h=draw(host);MockKit.Discard(host.gameObject);return h;
            }

            // Draws the four numbered sections at x 14 (the card's 14 px side padding) and returns the content height, 16 px above and 18 px below included.
            float Body(Transform p,float cw)
            {
                const float X=14;float y=16;
                // I. rarities: a 3-column grid, 6 px apart
                Pill(p,X,y,cw,"I","추천 표시 등급");y+=36;
                float third=(cw-12)/3;int grades=RiftGearOfferSettings.GradeNames.Length;
                for(int i=0;i<grades;i++)
                {
                    int grade=i;bool on=(draft.gradeMask&(1<<grade))!=0;
                    Choice(p,"rift-gear-grade-"+grade,X+i%3*(third+6),y+i/3*46,third,on,false,RiftGearOfferSettings.GradeNames[grade],GradeInk[Mathf.Min(grade,GradeInk.Length-1)],true,()=>{draft.gradeMask^=1<<grade;view.Repaint();});
                }
                y+=(grades+2)/3*46-6+8+12;
                // II. positions, each with its own details button; the expanded one shows its filters right under the row
                Pill(p,X,y,cw,"II","추천 범위");y+=36;
                for(int i=0;i<RiftGearOfferSettings.Ranges.Length;i++)
                {
                    int position=i;var range=draft.Range(position);string id=RiftGearOfferSettings.Ranges[position];
                    Choice(p,"rift-gear-range-"+id,X,y,cw-118,(draft.rangeMask&(1<<position))!=0,false,RiftGearOfferSettings.RangeNames[position],null,false,()=>{draft.rangeMask^=1<<position;view.Repaint();});
                    Details(p,"rift-gear-details-"+id,X+cw-112,y,range.details,()=>{state.expanded=state.expanded==id?null:id;state.adding=false;view.Repaint();});
                    y+=48;
                    if(state.expanded==id)y+=Filters(p,X+10,y,cw-20,position,range)+8;
                }
                y+=kit.Para(p,Loc.T("조건에 맞지 않거나 꺼 둔 장비는 추천하지 않습니다. 설정은 저장 이후 새로 획득한 장비부터 적용합니다."),X,y,cw,11,1.5f,UiTheme.MockMutedInk,400,TextAnchor.UpperLeft,true)+8+12;
                // III. offer rule
                Pill(p,X,y,cw,"III","추천 여부");y+=36;
                Choice(p,"rift-gear-rule-all",X,y,cw,draft.allWearable,true,"착용 가능한 모든 장비",null,false,()=>{draft.allWearable=true;probe.BetterChosen=false;view.Repaint();});y+=48;
                Choice(p,"rift-gear-rule-better",X,y,cw,!draft.allWearable,true,"현재 착용중인 장비보다 점수가 높은 장비",null,false,()=>{draft.allWearable=false;probe.BetterChosen=true;view.Repaint();});y+=48+12;
                // IV. how long a card stays: two radios, the slider with its seconds, the caption and the closing note
                Pill(p,X,y,cw,"IV","팝업 시간");y+=36;
                Choice(p,"rift-gear-time-manual",X,y,cw,!draft.timed,true,"취소 버튼을 누를 때까지",null,false,()=>{draft.timed=false;view.Repaint();});y+=48;
                Choice(p,"rift-gear-time-limited",X,y,cw,draft.timed,true,"지정 시간 후 닫힘",null,false,()=>{draft.timed=true;view.Repaint();});y+=48;
                var seconds=kit.Txt(p,Loc.F("{0}초",draft.ClampedSeconds),X+4,y+11.5f,40,Noto*13,13,draft.timed?Navy:UiTheme.MockMutedInk,400,false,TextAnchor.MiddleLeft,true);seconds.name="Seconds value";
                TimeSlider(p,X+56,y+6,cw-60,RiftGearOfferSettings.MinSeconds,RiftGearOfferSettings.MaxSeconds,draft.ClampedSeconds,draft.timed,
                    amount=>{draft.seconds=amount;seconds.text=Loc.F("{0}초",amount);},"rift-gear-seconds");
                y+=36+8-4;
                y+=kit.Para(p,Loc.T("1초 ~ 1분 (지정 시간 선택 시에만 사용)"),X+56,y,cw-56,11,16f/11,skin.Edge,400,TextAnchor.UpperLeft,true)+8;
                y+=kit.Para(p,Loc.T("시간이 지나면 그 추천 장비는 사라지고 다음 순서의 장비가 표시됩니다. 조건에 맞는 장비를 한 번씩 모두 보여 준 뒤 팝업이 닫히며, 그 사이에 새로 얻은 추천 장비는 맨 뒤에 추가됩니다."),X,y,cw,11,1.5f,UiTheme.MockMutedInk,400,TextAnchor.UpperLeft,true);
                if(!lesson)y+=18+edict.Draw(view,p,cw,X,y+18);
                return y+18;
            }

            // The details panel under an expanded position (.filters): 10 px padding, 6 px between its rows; the height follows what it holds.
            float Filters(Transform p,float x,float y,float w,int at,RiftGearRange range)
            {
                var ring=kit.Box(p,"Filters ring",x-1,y-1,w+2,10,skin.CardEdge,skin.CardEdge,15,0,0);
                var panel=kit.Box(p,"Filters "+RiftGearOfferSettings.Ranges[at],x,y,w,10,UiTheme.MockCellEnd,skin.CardEdge,14,1,0);Inset(panel,14,1,w-28,.06f);
                float ix=11,iw=w-22,cy=11;
                Choice(panel,"rift-gear-details-enabled",ix,cy,iw,range.details,false,"세부 설정 사용",null,false,()=>{range.details=!range.details;state.adding=false;view.Repaint();});cy+=46;
                cy+=kit.Para(panel,Loc.T("선택한 옵션을 모두 기준값 이상으로 만족하는 장비만 추천합니다. 선택한 옵션이 없으면 조건을 적용하지 않습니다."),ix,cy,iw,11,1.5f,UiTheme.MockMutedInk,400,TextAnchor.UpperLeft,true)+6;
                Jua(panel,Loc.T("대표 옵션"),ix,cy+1,iw,12.5f,Navy,TextAnchor.MiddleLeft,true);cy+=21;
                var common=RiftGearOfferSettings.RepresentativeStatKeys(at).ToArray();
                foreach(string key in common){Threshold(panel,ix,cy,iw,at,range,range.thresholds.Single(t=>t.stat==key));cy+=46;}
                var extra=range.thresholds.Where(t=>t.enabled&&!common.Contains(t.stat)).ToArray();
                if(extra.Length>0)
                {
                    Jua(panel,Loc.T("추가 옵션"),ix,cy+1,iw,12.5f,Navy,TextAnchor.MiddleLeft,true);cy+=21;
                    foreach(var t in extra){Threshold(panel,ix,cy,iw,at,range,t);cy+=46;}
                }
                var remaining=range.thresholds.Where(t=>!t.enabled&&!common.Contains(t.stat)).ToArray();
                AddButton(panel,ix,cy,iw,range.details&&remaining.Length>0);cy+=46;
                if(state.adding&&range.details)
                    foreach(var t in remaining)
                    {
                        var option=t;Choice(panel,"rift-gear-add-"+t.stat,ix,cy,iw,false,false,StatName(at,t.stat),null,false,()=>{option.enabled=true;state.adding=false;view.Repaint();});cy+=46;
                    }
                float h=cy-6+11;UiLayout.Place(ring,x-1,y-1,w+2,h+2);UiLayout.Place(panel,x,y,w,h);return h;
            }
            // One required option: its checkbox row, the value field and the unit.
            void Threshold(Transform p,float x,float y,float iw,int at,RiftGearRange range,ShopThreshold t)
            {
                float labelW=iw-70-26-12;
                Choice(p,"rift-gear-threshold-"+t.stat,x,y,labelW,t.enabled,false,StatName(at,t.stat),null,false,()=>{t.enabled=!t.enabled;view.Repaint();},range.details);
                Number(p,x+labelW+6,y,70,t.value,range.details,t.enabled,value=>t.value=value,"rift-gear-value-"+t.stat);
                Unit(p,x+labelW+6+70+6,y);
            }
            // "＋ 조건 옵션 추가": white; yellow while its list is open; flat grey when there is nothing to add or the details are off.
            void AddButton(Transform p,float x,float y,float w,bool enabled)
            {
                bool open=state.adding&&enabled;
                var r=!enabled?kit.Box(p,"rift-gear-add-option",x,y,w,40,UiTheme.MockDisabledFill,UiTheme.MockDisabledEdge,12,2,0)
                    :open?kit.Box(p,"rift-gear-add-option",x,y,w,40,UiTheme.MockIconTop,Navy,12,2,3,UiTheme.MockIconBottom):kit.Box(p,"rift-gear-add-option",x,y,w,40,Color.white,Navy,12,2,3);
                var button=Press(r,()=>{state.adding=!state.adding;view.Repaint();});button.interactable=enabled;
                Jua(r,Loc.T(open?"옵션 목록 닫기":"＋ 조건 옵션 추가"),0,14,w,13,enabled?Navy:UiTheme.MockDisabledInk,TextAnchor.MiddleCenter,true);
            }
        }
    }
}
