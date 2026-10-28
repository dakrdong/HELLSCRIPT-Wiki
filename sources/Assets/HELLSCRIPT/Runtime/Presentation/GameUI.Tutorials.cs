using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed partial class GameUI
    {
        ContentWindowView tutorialJournal;
        string selectedTutorial="",pendingTutorial="";
        int tutorialPromptPhase=-1;
        int tutorialTab;
        float tutorialPoll;
        float nextEncounterTip;readonly System.Collections.Generic.Queue<string> encounterTips=new System.Collections.Generic.Queue<string>();
        string queuedTutorialContext="";readonly System.Collections.Generic.HashSet<string> shownTutorialContexts=new System.Collections.Generic.HashSet<string>();
        string tutorialFocus="";TutorialAnchorRing tutorialRing;
        public bool SuppressTutorialPopups=>game.Active||game.TutorialActive||cinematic?.Busy==true||RuneIntroductionActive||RiftEntryTourActive||tutorialJournal!=null||Tutorials.Mandatory(game.Store.Data);
        public void CloseTutorialJournal(){tutorialJournal?.Close();tutorialJournal=null;}
        public void ShowTutorialJournal(string id="")
        {
            if(id==ContentUnlocks.Rune&&!ContentUnlocks.Has(game.Store.Data,id)){ShowToast(ContentUnlocks.LockedToast(id),4,true);return;}
            if(game.TutorialActive){ShowTutorialPrompt();return;}
            if(game.Active){var hint=Tutorials.Definition(id)??Tutorials.Next(game.Store.Data);ShowToast(hint?.Body??Loc.T("자세한 안내는 마을에서 확인하세요."));return;}
            CloseTutorialJournal();selectedTutorial=id;
            tutorialJournal=TutorialJournalWindow.Open(transform,EquipmentViewSource.Owned,()=>InterfaceFactor,
                "모험 안내",RenderTutorialJournal);
        }
        void TutorialText(ContentWindowView view,string text,int size=18)
        {
            float width=view.Width-30;
            var row=UiLayout.Rect("Guide text",view.Body);var label=UiLayout.Text(row,"Text",text,0,0,width,100,Mathf.RoundToInt(size*InterfaceFactor),UiTheme.Text);
            float height=Mathf.Max(42,label.preferredHeight+16);row.gameObject.AddComponent<LayoutElement>().preferredHeight=height;
            UiLayout.Place(label.rectTransform,0,0,width,height);label.supportRichText=false;
        }
        Button TutorialRow(ContentWindowView view,string text,Action action)
        {
            var row=UiLayout.Rect("Guide row",view.Body);row.gameObject.AddComponent<LayoutElement>().preferredHeight=58*InterfaceFactor;
            return view.Button(row,text,0,0,view.Width-28,54*InterfaceFactor,action);
        }
        void RenderTutorialJournal(ContentWindowView view)
        {
            var a=game.Store.Data;var d=Tutorials.Definition(selectedTutorial);
            if(d!=null&&(!HuntEdictProgression.GuideVisible(a,d.id)||d.id==ContentUnlocks.Rune&&!Tutorials.Available(a,d))){d=null;selectedTutorial="";}
            if(d==null)
            {
                string[] tabs={"진행 중","완료","나중에 보기"};float tabWidth=(view.Width-32)/3;
                for(int i=0;i<tabs.Length;i++){int tab=i;view.Tab(view.Navigation,tabs[i],i*(tabWidth+4),0,tabWidth,34,()=>{tutorialTab=tab;view.Scroll.verticalNormalizedPosition=1;view.Repaint();},tutorialTab==i).name="tutorial-tab-"+i;}
                if(tutorialTab==0)
                {
                    var next=FirstPlayRecommendation.Choose(a);
                    if(next!=null){TutorialText(view,Loc.T("지금 할 일")+"\n"+next.text,20);TutorialRow(view,Loc.T("관련 화면 열기"),()=>{CloseTutorialJournal();OpenNextStep(next);}).name="tutorial-next-action";}
                    TutorialText(view,Loc.T("설명 확인과 실제 수행을 별도로 기록합니다. 나중에 보기와 숨김은 언제든 되돌릴 수 있습니다."));
                }
                foreach(var def in Tutorials.All.Where(def=>HuntEdictProgression.GuideVisible(a,def.id)&&Tutorials.Available(a,def)))
                {
                    var r=Tutorials.Record(a,def.id);string status=r.practiced?Loc.T("수행 완료"):r.hidden?Loc.T("숨김"):r.deferred?Loc.T("나중에"):r.read?Loc.T("읽음"):Tutorials.Available(a,def)?Loc.T("진행 가능"):Loc.T("조건 대기");
                    int tab=r.hidden||r.deferred?2:r.practiced?1:0;if(tab!=tutorialTab)continue;
                    TutorialRow(view,def.Title+" · "+status,()=>{selectedTutorial=def.id;view.Repaint();}).name="tutorial-row-"+def.id;
                }
                return;
            }
            view.Button(view.Navigation,Loc.T("목록"),0,0,130,34,()=>{selectedTutorial="";view.Repaint();});
            TutorialText(view,d.Title,22);TutorialText(view,d.Body);
            if(d.id.StartsWith("UL"))TutorialText(view,ContentUnlocks.Condition(d.id));
            foreach(var variant in a.guide.tutorials.Where(r=>r.id==d.id&&r.variant!=""))
            {
                var current=variant;TutorialRow(view,Tutorials.VariantTitle(current.variant)+" · "+Loc.T(current.read?"읽음":"새 안내"),()=>{game.Store.SetTutorialPreference(d.id,"read",current.variant);view.Repaint();});
                TutorialText(view,Tutorials.VariantBody(current.variant));
            }
            var state=Tutorials.Record(a,d.id);
            TutorialText(view,Loc.F("설명: {0} · 실습: {1}",state.read?"읽음":"미확인",state.practiced?"수행 완료":"아직 수행하지 않음"));
            if(d.supported)TutorialText(view,a.guide.supportUsed.Contains(d.id)?Loc.T("계정의 최초 실습 지원을 사용했습니다."):Loc.T("최초 실습 지원 1회가 남아 있습니다. 실행과 저장이 성공할 때만 사용됩니다."));
            TutorialRow(view,state.hidden||state.deferred?Loc.T("안내 다시 표시"):Loc.T("이 안내 숨기기"),()=>{if(game.Store.SetTutorialPreference(d.id,state.hidden||state.deferred?"resume":"hide")){tutorialFocus="";pendingTutorial="";}view.Repaint();}).name="tutorial-visibility";
            float w=(view.Width-40)/3;
            view.Button(view.Actions,Loc.T("읽음"),0,0,w,40,()=>{game.Store.SetTutorialPreference(d.id,"read");view.Repaint();}).name="tutorial-read";
            view.Button(view.Actions,Loc.T("나중에"),w+8,0,w,40,()=>{if(!game.Store.SetTutorialPreference(d.id,"defer"))return;tutorialFocus="";pendingTutorial="";CloseTutorialJournal();}).name="tutorial-defer";
            view.Button(view.Actions,Loc.T("실습"),2*(w+8),0,w,40,()=>StartTutorialAction(d),true).name="tutorial-practice";
            // The tutorial stage itself (P00) is played once and has nothing to practice afterwards.
            var practice=view.Actions.GetComponentsInChildren<Button>().Last();
            practice.interactable=d.id!="P00"&&Tutorials.Available(a,d);
            if(d.target=="edict"&&!EdictPracticeAvailable(d.id))practice.gameObject.SetActive(false);
        }
        bool EdictPracticeAvailable(string id)
        {
            if(id=="H10"||id=="H11"||id=="F05"||id=="E02")return true;
            string focus=EdictGuidance.FocusOption(id);
            var group=HuntEdictUiCatalog.Data.groups.FirstOrDefault(g=>g.ids.Contains(focus));
            return group!=null&&HuntEdictHudRows.Tabs.Contains(group.tab)&&focus!="autoEquip.preserveEffects";
        }
        void ClearEdictTutorialFocus()
        {
            string target=Tutorials.Definition(tutorialFocus)?.target;
            if(target!="skills"&&target!="edict")return;
            tutorialFocus="";
            if(tutorialRing!=null){Destroy(tutorialRing.gameObject);tutorialRing=null;}
        }
        void StartTutorialAction(TutorialDefinition d)
        {
            if(d.id==ContentUnlocks.Rune&&!ContentUnlocks.Has(game.Store.Data,d.id)){ShowToast(ContentUnlocks.LockedToast(d.id),4,true);return;}
            CloseTutorialJournal();tutorialFocus=d.id;
            if(d.id=="F07"&&game.Store.Data.Hero.guide.trainedSetup!=""&&game.Store.Data.Hero.guide.trainedSetup==TutorialProgress.Signature(game.Store.Data.Hero))
            {game.RequestStation(TownStation.RiftKeeper);return;}
            if(d.supported&&!game.Store.Data.guide.supportUsed.Contains(d.id))
            {
                var station=TutorialPractice.Station(d.id);
                if(game.Town?.Nearby==station){OpenTutorialPractice(d.id);return;}
                pendingTutorial=d.id;game.RequestStation(station);ShowToast(Loc.T("해당 주민에게 이동합니다. 가까이 도착하면 실습을 선택하세요."));return;
            }
            if(d.id=="H11"){ShowRepeatSettings();return;}
            switch(d.target)
            {
                case "inventory":ShowPlayInventory();break;
                case "skills":ShowEdictSkillTree();break;
                case "edict":
                    if(d.id=="E02"){ShowEdictShare();PresetsWindow?.ShowQuickPresets();}
                    else if(d.id=="H10")ShowEdictShare();
                    else {string option=EdictGuidance.FocusOption(d.id);if(option=="survival.potionHpPercent"||option=="survival.lowHp"||option=="survival.lowHpPercent"||option=="survival.returnHpPercent"||option=="potion.resourcePercent"){TogglePotionQuick();break;}var group=HuntEdictUiCatalog.Data.groups.FirstOrDefault(g=>g.ids.Contains(option));if(group!=null&&HuntEdictHudRows.Tabs.Contains(group.tab))ShowEdictTab(group.tab);}
                    break;
                case "offline":ShowOfflineSupplies();break;
                case "attendance":ShowAttendance(AttendanceKind.Weekly);break;
                case "class-practice":ShowClassPractice();break;
                case "battle":if(game.Combat!=null)ShowBattle();else game.RequestStation(TownStation.RiftKeeper);break;
                default:game.RequestStation(d.target switch {"training"=>TownStation.Training,"blacksmith"=>TownStation.Blacksmith,"jeweler"=>TownStation.GemMerchant,"runes"=>TownStation.RuneMaster,"gambler"=>TownStation.Gambler,"merchant"=>TownStation.Merchant,"storage"=>TownStation.Warehouse,"aspect"=>TownStation.AspectStone,_=>TownStation.RiftKeeper});break;
            }
        }
        void TickTutorialUI()
        {
            if(game.Store==null)return;
            if(TickRiftEntryTour())return;
            TickPrologueLesson();
            if(TickForgeIntroduction())return;
            if(TickRuneBoardTutorial())return;
            tutorialPoll-=Time.unscaledDeltaTime;if(tutorialPoll>0)return;tutorialPoll=.25f;RefreshTutorialAnchor();
            if(Page=="title"||Page=="characters"){if(cinematic.Busy)cinematic.Clear();return;}
            if(TickGuideTutorial())return;
            if(game.TutorialActive)
            {
                var run=game.Combat.State;ResetStaging(run);
                int phase=run.tutorialPhase;int promptKey=phase+(run.health<=0?10:0);
                bool beat=phase==ProloguePhase.Surrounded||phase==ProloguePhase.Gatekeeper&&game.Combat.TutorialBoss==null||
                    phase==ProloguePhase.SurvivalLesson&&!interventionTold||phase==ProloguePhase.Cleared||PitRun(run)&&phase==ProloguePhase.StageTwoIntro||run.health<=0;
                if(promptKey!=tutorialPromptPhase&&beat&&!CommonPanelOpen&&!PlayInventoryOpen&&tutorialJournal==null&&!cinematic.Busy)
                    PresentTutorialStep();
                StageTutorialCombat(run);
            }
            else
            {
                if(cinematic.Busy)return;
                tutorialPromptPhase=-1;
                if(TickRiftGraphLesson())return;
                if(pendingTutorial!=""&&game.Town?.Nearby==TutorialPractice.Station(pendingTutorial)&&!CommonPanelOpen)
                {string id=pendingTutorial;pendingTutorial="";OpenTutorialPractice(id);}
                // A hero who quit between graduation and the end of the chief's welcome is welcomed on the next town entry.
                var guide=game.Store.Data.guide;
                if(guide.chiefPending&&guide.townQuest==TownQuest.None&&Page=="plaza"&&tutorialJournal==null&&!CommonPanelOpen&&!game.Active)ShowChiefArrival();
                ObserveTutorialContext();
                TryEdictNotice();
                if(tutorialFocus!=TownEnhanceFocus&&tutorialFocus!=""&&Tutorials.Record(game.Store.Data,tutorialFocus).practiced)tutorialFocus="";
                TickTownErrands();
                if(blacksmith?.ActiveFeature!=null)QueueTutorialContext(blacksmith.ActiveFeature);
                if(equipmentShopWindow?.Gamble==true)QueueTutorialContext(ContentUnlocks.Shop);
                if(jewelerWindow!=null)QueueTutorialContext(ContentUnlocks.Gem);
                if(queuedTutorialContext!=""&&tutorialJournal==null&&!game.Active)
                {string id=queuedTutorialContext;queuedTutorialContext="";shownTutorialContexts.Add(id);ShowTutorialBriefing(id);}
            }
        }
        void QueueTutorialContext(string id)
        {
            var a=game.Store.Data;
            if(id==ContentUnlocks.Rune&&RuneBoardTutorial.Active(a))return;
            Tutorials.Discover(a,id);var state=Tutorials.Record(a,id);
            if(a.guide.hintsHidden||tutorialFocus!=""||tutorialJournal!=null||game.TutorialActive||game.Active||shownTutorialContexts.Contains(id)||state.read||state.deferred||state.hidden||state.practiced||!ContentUnlocks.Has(a,id))return;
            if(queuedTutorialContext=="")queuedTutorialContext=id;
        }
        void RefreshTutorialAnchor()
        {
            var found=FindRingTarget(tutorialFocus=="F02"?new[]{"rift-enter-normal"}:Array.Empty<string>());
            var target=found?.transform;
            if(tutorialRing!=null&&tutorialRing.transform.parent==target)return;
            if(tutorialRing!=null)Destroy(tutorialRing.gameObject);
            if(target==null)return;var rect=UiLayout.Rect("Tutorial focus",target);UiLayout.Stretch(rect);tutorialRing=rect.gameObject.AddComponent<TutorialAnchorRing>();
        }
        Button FindRingTarget(string[] names)
        {
            var buttons=GetComponentsInChildren<Button>();
            foreach(string name in names){var b=buttons.FirstOrDefault(x=>x.name==name&&x.interactable&&x.gameObject.activeInHierarchy);if(b!=null)return b;}
            return null;
        }
        void ObserveTutorialContext()
        {
            var a=game.Store.Data;
            if(!Tutorials.Mandatory(a)&&Economy.FreeSlots(a.Hero)<=5)Tutorials.Discover(a,"H13");
            if(ContentUnlocks.Has(a,ContentUnlocks.Train))Tutorials.Discover(a,"H17");
            if(Page=="bag"){Tutorials.Discover(a,"H01");Tutorials.Discover(a,"H03");Tutorials.Discover(a,"H05");Tutorials.Discover(a,"H13");}
            if(Page=="plaza"&&a.Hero.level>=2)Tutorials.Discover(a,"H07");
            var run=game.Combat?.State;
            if(run?.training<0)
            {
                Tutorials.Performed(a,"F02");if(run.kills>0){Tutorials.Performed(a,"F03");Tutorials.Discover(a,"H09");}
                if(run.portal)Tutorials.Discover(a,"H13");
                if(run.phase==RunPhase.Failed)Tutorials.Discover(a,"H12");
                if(run.layout.objective!=RiftObjectiveKind.None)DiscoverEncounter(run.layout.objective.ToString());
                if(run.layout.chests.Any(c=>c.discovered))DiscoverEncounter("chest");
                if(run.layout.shrines.Any(c=>c.discovered))DiscoverEncounter("shrine");
                foreach(var e in run.enemies.Where(e=>Vector2.Distance(e.position,run.position)<12))
                {if(e.goblin)DiscoverEncounter("goblin");if(e.elite>=0)DiscoverEncounter("elite");if(e.boss)DiscoverEncounter("boss");}
                if(game.Active&&encounterTips.Count>0&&Time.unscaledTime>=nextEncounterTip)
                {ShowToast(Tutorials.VariantBody(encounterTips.Dequeue()));nextEncounterTip=Time.unscaledTime+8;}
            }
            foreach(var item in a.Hero.inventory)
            {
                if(item.rarity==3)Tutorials.Discover(a,"H14",string.IsNullOrEmpty(ItemCatalog.Unique(item.special)?.setId)?"legendary":"set");
                if(item.awakened)Tutorials.Discover(a,"H14","awakened");
            }
            if(a.guide.tutorials.Any(r=>r.id=="H14"&&r.variant!=""))Tutorials.Discover(a,"H14");
            if(a.guide.tutorials.Any(r=>r.id=="H15"&&r.variant!=""))Tutorials.Discover(a,"H15");
            // Changed facts save with the normal checkpoint cadence, never one write per frame.
        }
        void DiscoverEncounter(string variant)
        {
            var a=game.Store.Data;var record=Tutorials.Record(a,"H15",variant);
            var group=Tutorials.Record(a,"H15");
            if(!record.available&&!record.read&&!a.guide.hintsHidden&&!group.hidden&&!group.deferred)encounterTips.Enqueue(variant);
            Tutorials.Discover(a,"H15",variant);
        }
    }
}
