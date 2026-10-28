using System;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed partial class SkillTreeD4Window
    {
        GameStore tacticsStore;GameCatalog tacticsCatalog;
        CombatSimulation tacticsSimulation;
        SkillTacticsEditSession tacticsPreview;
        string tacticsNode="";long tacticsRevision=-1;
        RectTransform tacticsEntry;UiButton tacticsButton;Text tacticsBadge;StorageSurface tacticsBadgeFace;
        SkillTacticsReturnState tacticsReturn;SkillTreeD4Selection tacticsSelection;
        public SkillTacticsWindow TacticsWindow {get;private set;}
        public bool TacticsEntryVisible=>tacticsEntry!=null&&tacticsEntry.gameObject.activeInHierarchy;

        // Retained for existing development captures; players use the same store-backed tree.
        public static SkillTreeD4Window OpenDevelopmentTactics(Transform parent,GameStore store,GameCatalog catalog,
            string previewOrient=null,Action closed=null,int initialSlot=-1)
        {
            if(store==null||catalog==null)throw new ArgumentNullException(store==null?nameof(store):nameof(catalog));
            return Open(parent,new StoreSkillTreeD4Host(store,catalog),hero:store.Data.Hero.heroClass.ToString(),
                previewOrient:previewOrient,store:store,closed:closed,initialSlot:initialSlot);
        }
        string SelectedTacticsNode()
        {
            var selected=Session.Selection;if(selected.Passive)return "";
            var active=model.Classes[Session.Hero].Actives[selected.Index];
            return selected.Modifier<0?active.Id:active.Modifiers[selected.Modifier].Id;
        }
        SkillTacticsEditSession TacticsPreview()
        {
            if(tacticsStore==null||tacticsCatalog==null||!host.PlayerCanEdit(Session.Hero)||tacticsStore.Data.Hero.heroClass.ToString()!=Session.Hero||tacticsStore.Data.Hero.build.classSkills?.version!=5)return null;
            string node=SelectedTacticsNode();if(!SkillTacticsCatalog.CanEnter(SkillTacticsHostKind.Player,node))return null;
            if(tacticsPreview!=null&&tacticsNode==node&&tacticsRevision==tacticsStore.Revision)return tacticsPreview;
            tacticsNode=node;tacticsRevision=tacticsStore.Revision;
            try{return tacticsPreview=new SkillTacticsEditSession(tacticsStore.Data.Hero,node,SkillTacticsHostKind.Player);}
            catch(ArgumentException){return tacticsPreview=null;}
        }
        double TacticsTitleInset()=>TacticsPreview()==null?0:26;
        void RenderTacticsEntry(RectTransform head,double x,double y)
        {
            var preview=TacticsPreview();if(preview==null){if(tacticsEntry!=null)tacticsEntry.gameObject.SetActive(false);return;}
            if(tacticsEntry==null)
            {
                tacticsEntry=UiLayout.Rect("d4-tactics-entry",head);
                var target=UiLayout.Rect("Tactics hit",tacticsEntry);UiLayout.Stretch(target);var hit=target.gameObject.AddComponent<Image>();hit.color=Color.clear;
                tacticsButton=target.gameObject.AddComponent<UiButton>();tacticsButton.Configure(UiButtonRole.Icon);tacticsButton.targetGraphic=hit;tacticsButton.Face.gameObject.SetActive(false);
                tacticsButton.onClick.AddListener(()=>OpenTactics());
                var art=UiLayout.Rect("Approved edict medallion",tacticsEntry);UiLayout.Stretch(art);
                var image=art.gameObject.AddComponent<Image>();image.sprite=Resources.Load<Sprite>("Art/GlobalHUD/Casual/skill-edict-medallion");image.preserveAspect=true;image.raycastTarget=false;
                var badge=UiLayout.Rect("Saved automatic badge",tacticsEntry);UiLayout.Place(badge,28,28,18,18);
                tacticsBadgeFace=badge.gameObject.AddComponent<StorageSurface>();tacticsBadgeFace.raycastTarget=false;
                tacticsBadge=UiLayout.Text(badge,"Saved automatic state","",0,0,18,18,12,UiSkin.Casual.Navy,UiSkin.Casual.BodyFont);tacticsBadge.alignment=TextAnchor.MiddleCenter;
            }
            tacticsEntry.SetParent(head,false);tacticsEntry.gameObject.SetActive(true);Place(tacticsEntry,x,y,22,22);
            tacticsBadge.text=preview.Preview?"i":preview.SavedValue("auto")=="1"?"✓":"−";
            tacticsBadgeFace.SetSkin(UiSkin.Casual,radius:9,borderWidth:2,shadow:0,fill:preview.Preview?UiSkin.Casual.InfoFill:UiSkin.Casual.Gold);
            tacticsButton.gameObject.name=Loc.T("사냥칙령 설정")+" · "+(preview.Preview?Loc.T("미리 보기"):preview.SavedValue("auto")=="1"?Loc.T("켜짐"):Loc.T("꺼짐"))+" · "+preview.Summary(true);
        }
        public bool OpenTactics()
        {
            if(!ready||TacticsWindow!=null||TacticsPreview()==null)return false;
            string node=SelectedTacticsNode();tacticsSelection=Session.Selection;
            tacticsReturn=new SkillTacticsReturnState{classTab=Array.IndexOf(SkillTreeD4Model.Heroes,Session.Hero),selectedNode=node,detailNode=node,
                treeScroll=content.anchoredPosition,detailScroll=Vector2.zero,split=frame.rect.width>0?plate.rect.width/frame.rect.width:0};
            TacticsWindow=SkillTacticsWindow.Open(transform.parent,tacticsStore,tacticsCatalog,node,TacticsFit,TacticsClosed,tacticsReturn,simulation:tacticsSimulation);
            return true;
        }
        (float scale,Rect plate) TacticsFit(Rect area)
        {
            if(forcedOrient!=null)
            {
                // A preview plate belongs to its actual Canvas, which can differ from Screen.height in the Editor.
                Canvas.ForceUpdateCanvases();float previewScale=uiCanvas.scaleFactor;var original=UiVisibility.ScreenRect(frame);
                return (previewScale,new Rect(original.position/previewScale,original.size/previewScale));
            }
            string next=forcedOrient??SkillTreeD4Units.Orient(area.width,area.height,Application.isMobilePlatform);
            var board=SkillTreeD4Frame.For(next);var placed=forcedOrient!=null?new SkillTreeD4UnitFit(next,1,0,0):SkillTreeD4Units.Fit(area.x,Screen.height-area.yMax,area.width,area.height,Application.isMobilePlatform);
            float scale=(float)placed.CanvasScale;
            return (scale,new Rect((float)(placed.FrameX/scale),Screen.height/scale-(float)(placed.FrameY/scale)-(float)board.H*2,(float)board.W*2,(float)board.H*2));
        }
        void TacticsClosed(SkillTacticsLeaveTarget target)
        {
            TacticsWindow=null;tacticsPreview=null;
            if(!ready)return;
            if(target==SkillTacticsLeaveTarget.GlobalSettings){var ui=GetComponentInParent<GameUI>();Close();ui?.ShowEdictTab("combat");return;}
            if(target==SkillTacticsLeaveTarget.CloseWindow){Close();return;}
            if(tacticsReturn!=null)
            {
                Session.Hero=SkillTreeD4Model.Heroes[tacticsReturn.classTab];Session.Select(tacticsSelection);
                Reflow();Repaint();content.anchoredPosition=tacticsReturn.treeScroll;scroll.velocity=Vector2.zero;
            }
            StoreViewBinding.Rendered(this);
        }
        bool RequestTacticsOwnerClose()
        {if(TacticsWindow==null)return false;TacticsWindow.RequestLeave(SkillTacticsLeaveTarget.CloseWindow);return true;}
    }
}
