using System;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine;

namespace Hellscript
{
    public sealed partial class GameController : MonoBehaviour
    {
        public GameCatalog catalog;
        public GameStore Store {get;private set;}
        public AttendancePopupSettings AttendancePopups {get;private set;}
        CombatSimulation combat;
        public CombatSimulation Combat {get=>combat;private set {combat=value;Audio?.Bind(value);}}
        public GameAudio Audio {get;private set;}
        // Session-only plaza walk; nothing here is saved or affects a run.
        public TownWalk Town {get;private set;}
        public GameUI UI {get;private set;}
        public WorldView World {get;private set;}
        public string Notice {get;private set;}="";
        public int SelectedStage=1;
        public bool Running => Combat!=null;
        public bool Active => Combat!=null&&Combat.State.phase!=RunPhase.Cleared&&Combat.State.phase!=RunPhase.Failed;
        public float EffectiveSpeed => Combat?.State.training<0&&Combat.State.riftAttendance?.version==1?Combat.State.riftAttendance.speed:1f;
        float saveClock,resultDelay;
        bool resultShown,backgroundPaused;
        void Awake()
        {
            Application.targetFrameRate=60;Screen.sleepTimeout=SleepTimeout.NeverSleep;
            if(catalog==null){catalog=ScriptableObject.CreateInstance<GameCatalog>();catalog.Populate();}
            string saveDirectory=Application.persistentDataPath;
            string[] args=Environment.GetCommandLineArgs();for(int i=0;i<args.Length-1;i++)if(args[i]=="-hellscriptSavePath")saveDirectory=args[i+1];
            AttendancePopups=new AttendancePopupSettings(saveDirectory);
            InitializeLanguage(saveDirectory);
            ItemRangeDisplay.Initialize(saveDirectory);
            InitializeDisplaySettings(saveDirectory);
            InitializeInterfaceScale(saveDirectory);
            InitializeIdle(saveDirectory);
            try{Store=new GameStore(saveDirectory,catalog);if(!Store.ActivateSkillTrees(catalog))throw new InvalidOperationException(Store.Error);}
            catch(Exception e){Notice=e.Message;UI=gameObject.AddComponent<GameUI>();UI.Initialize(this);enabled=false;return;}
            SelectedStage=Mathf.Max(1,Store.Data.Hero.highestClear+1);Notice=Store.OfflineMessage;
            if(Store.GemRecoveryMessage!="")Notice+=(Notice!=""?"\n":"")+Store.GemRecoveryMessage;
            if(Store.QualityRecoveryMessage!="")Notice+=(Notice!=""?"\n":"")+Store.QualityRecoveryMessage;
            World=gameObject.AddComponent<WorldView>();World.Initialize(this);
            Audio=gameObject.AddComponent<GameAudio>();Audio.Initialize(saveDirectory);
            UI=gameObject.AddComponent<GameUI>();UI.Initialize(this);
        }
        public void SelectHero(int index)
        {
            if(Running)return;CancelPotionDeparture();Store.Data.selectedHero=Mathf.Clamp(index,0,2);SelectedStage=Store.Data.Hero.highestClear+1;Save();if(UI.Page=="title")UI.ShowTitle();else UI.ShowTown();
        }
        public bool ChangeCharacterFromSettings(int index)
        {
            if(UI.Page=="title"||UI.Page=="characters")
            {
                if(!SaveEntryCharacter(index))return false;
                UI.CloseCommonPanel();if(UI.EntrySession.SignedIn)UI.OpenCharacterSelection();else UI.ShowTitle();return true;
            }
            if(!Store.SwitchCharacter(index,catalog,Combat?.State)){Notify(Store.Error);return false;}
            CancelPotionDeparture();
            UI.CloseCommonPanel();ExitIdle(false);RestoreForegroundClock();
            Comparison=null;ComparisonError="";repeatRestored=false;
            if(Combat!=null){Combat.Visual-=World.Effect;Combat=null;}
            SelectedStage=Store.Data.Hero.highestClear+1;EnterPlaza(true);return true;
        }
        public void Begin(int training=-1,bool resume=false,uint? seed=null)
        {BeginRun(training,resume,seed,false);}
        public void BeginDeveloperTraining(int training)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            BeginRun(training,false,null,true);
#endif
        }
        void BeginRun(int training,bool resume,uint? seed,bool fullSkillTraining,bool continueRepeat=false,float riftSpeed=1)
        {
            if(Active)return;
            if(training<0&&(!Store.RefreshRiftDay()||Store.Data.riftFatigue.Total<=0)){BlockRepeat(RepeatBlock.Configuration,Loc.T("남은 피로도가 없습니다."));Notify(Store.Error!=""?Store.Error:Loc.T("남은 피로도가 없습니다."));return;}
            if(!continueRepeat)ExitIdle(false);
            if(training>=0&&!fullSkillTraining&&!ContentUnlocks.Has(Store.Data,ContentUnlocks.Train)){Notify(ContentUnlocks.Condition(ContentUnlocks.Train));return;}
            if(!resume&&Store.Data.suspendedRun!=null){Notice="진행 중인 균열을 먼저 이어서 완료해 주세요.";UI.ShowTown();return;}
            Comparison=null;
            RunState snapshot=resume?Store.Data.suspendedRun:null;
            if(snapshot!=null){int hero=Store.Data.heroes.FindIndex(h=>h.id==snapshot.heroId);if(hero<0){Notify("저장된 영웅을 찾을 수 없습니다.");return;}Store.Data.selectedHero=hero;}
            if(training<0&&snapshot==null)
            {
                // A paid confirmation must not become a delayed free launch after changing supplies.
                if(riftSpeed!=1)CancelPotionDeparture();
                string visit=PotionVisit();
                if(!PrepareDeparturePotions(visit,continueRepeat))
                {if(!continueRepeat&&riftSpeed==1&&string.IsNullOrEmpty(Store.Error)){waitingHero=Store.Data.Hero.id;waitingVisit=visit;waitingSeed=seed;}return;}
            }
            CancelPotionDeparture();
            var previous=Combat;var previousSession=Store.Data.repeatHunt;var previousSuspended=Store.Data.suspendedRun;
            string fingerprint=Store.Data.Hero.lastRiftFingerprint;int boss=Store.Data.Hero.lastRiftBoss;
            try{Combat=new CombatSimulation(Store.Data,catalog,Mathf.Clamp(SelectedStage,1,Store.Data.Hero.highestClear+1),training,snapshot,seed,ownedTraining:!fullSkillTraining);}
            catch(Exception e){Combat=previous;BlockRepeat(RepeatBlock.Configuration,e.Message);Notify(e.Message);return;}
            if(training<0){Store.Data.suspendedRun=Combat.State;Combat.CommitChest=c=>Store.CommitChest(Combat.State,c);Combat.CommitRunChange=(request,operation,change)=>Store.CommitRunMutation(Combat.State,request,operation,change);}
            if(training<0)
            {
                var continuing=(continueRepeat||resume)&&previousSession?.heroId==Combat.Hero.id&&(!resume||previousSession.runId==Combat.State.id)?
                    JsonUtility.FromJson<RepeatHuntSession>(JsonUtility.ToJson(previousSession)):null;
                Store.Data.repeatHunt=RepeatHunt.Start(Combat.State,Combat.RepeatPolicy,continuing);
                if(!resume&&Combat.EdictActive&&!Store.Data.repeatHunt.policy.stopWhenFull&&Economy.FreeSlots(Store.Data.Hero)<=0)Combat.State.limitedLoot=true;
            }
            else Store.Data.repeatHunt=null;
            if(!(training<0?Store.CommitRiftEntry(Combat.State,riftSpeed,resume):Store.Save()))
            {
                Combat=previous;Store.Data.repeatHunt=previousSession;Store.Data.suspendedRun=previousSuspended;
                Store.Data.Hero.lastRiftFingerprint=fingerprint;Store.Data.Hero.lastRiftBoss=boss;
                BlockRepeat(RepeatBlock.Save,Store.Error);Notify(Store.Error);return;
            }
            UI.CloseRiftEntry();
            if(previous!=null)previous.Visual-=World.Effect;
            repeatRestored=false;portalCleanupTried=false;repeatClock=Time.realtimeSinceStartupAsDouble;
            FirstPlayGuide.Enter(Store.Data,Combat.State,resume);
            Combat.Visual+=World.Effect;Combat.GateOpened+=UI.ShowToast;
            if(DisplayDimmed)World.DeferDungeon();else{World.BuildDungeon(Combat.State);UI.ShowBattle();}
            RestoreForegroundClock();resultDelay=0;resultShown=false;Save();
        }
        public void BeginRiftEntry(bool accelerated)=>BeginRun(-1,false,null,false,false,accelerated?1.5f:1);
        public void TogglePause()
        {if(!Active)return;if(foregroundSaveBlocked&&!Store.Save()){Notify(ForegroundPauseReason);return;}if(foregroundResumeRequired){RestoreForegroundClock();Combat.State.paused=false;}else Combat.State.paused=!Combat.State.paused;Combat.Log("PAUSE",Combat.State.paused?"일시정지":"전투 재개");UI.RefreshHud();}
        public void SetSpeed(float speed)
        {
            if(!CombatSpeedAccess.CanSelect(speed)){Notify(CombatSpeedAccess.LockedMessage);return;}
            if(Store.Data.speed!=speed){Store.Data.speed=speed;Save();}
            UI.RefreshHud();
        }
        public void ContinuePortal()
        {if(Combat==null)return;if(Combat.GemBagBlocked){Notify("보석을 합성해 공간을 만들거나 남은 보상을 두고 종료해 주세요.");return;}if(Economy.FreeSlots(Store.Data.Hero)<=0&&!Combat.State.limitedLoot){Notify("판매·분해·창고 이동으로 가방을 비워 주세요.");return;}Combat.State.portal=false;Combat.State.portalCast=0;Combat.State.paused=false;if(!DisplayDimmed)UI.ShowBattle();Save();}
        public void LeaveUncollectedLoot(string request)
        {
            if(Combat==null)return;
            if(!Store.CommitRunMutation(Combat.State,request,"leave-loot:"+Combat.State.id,run=>
            {
                if(!run.portal||run.phase!=RunPhase.Looting)return false;
                foreach(var drop in run.drops)if(!drop.claimed)drop.ignored=true;
                foreach(var drop in run.resources)if(!drop.claimed)drop.ignored=true;
                run.portal=false;run.paused=false;return true;
            })){Notify(Store.Error);return;}
            UI.ShowBattle();
        }
        public void EnterPlaza(bool fresh=false)
        {
            if(Active)return;
            if(!Store.ActivateSkillTrees(catalog)){Notify(Store.Error);return;}
            if(fresh||Town==null)VisitSanctuary(PotionVisit());
            if(Combat!=null){Combat.Visual-=World.Effect;Combat=null;}
            if(fresh||Town==null)Town=new TownWalk();
            World.BuildTown(Town);UI.ShowPlaza();
        }
        public void RequestStation(TownStation station)
        {if(Town==null||Active)return;Town.Request(station);UI.RefreshPlaza();}
        public void InteractTown()
        {
            if(Town==null||Active||UI.Page!="plaza"||UI.CommonPanelOpen||UI.TownNavigationOpen)return;
            if(Town.Nearby.HasValue)UI.OpenStation(Town.Nearby.Value);
        }
        public void InteractEquipmentMerchant(EquipmentShopTab tab)
        {
            if(Town==null||Active||UI.Page!="plaza"||UI.CommonPanelOpen||UI.TownNavigationOpen||Town.Nearby!=TownStation.Merchant&&Town.Nearby!=TownStation.Gambler)return;
            UI.ShowEquipmentShop(tab,Town.Nearby==TownStation.Gambler);
        }
        public void TapPlaza(Vector2 screen)
        {
            if(Town==null||UI==null||UI.Page!="plaza"||Active||UI.CommonPanelOpen||UI.TownNavigationOpen||!UiSafeArea.Frame.Contains(screen))return;
            if(World.TryPickStation(screen,out var station))RequestStation(station);
            else if(World.TryPickTownGround(screen,out var point))Town.RequestPoint(point);
        }
        readonly System.Collections.Generic.List<RaycastResult> townUIHits=new System.Collections.Generic.List<RaycastResult>();
        bool TownPointerOverUI(Vector2 point)
        {
            if(EventSystem.current==null)return false;townUIHits.Clear();
            EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current){position=point},townUIHits);return townUIHits.Count>0;
        }
        void TickPlaza(float real)
        {
            if(Town==null||UI==null||UI.Page!="plaza")return;
            if(UI.CommonPanelOpen||UI.TownNavigationOpen||backgroundPaused){UI.ResetTownInput();Town.Cancel();return;}
            Vector2 input=UI.TownMovement;var keyboard=Keyboard.current;
            if(keyboard!=null)
            {
                input+=new Vector2((keyboard.dKey.isPressed||keyboard.rightArrowKey.isPressed?1:0)-(keyboard.aKey.isPressed||keyboard.leftArrowKey.isPressed?1:0),
                    (keyboard.wKey.isPressed||keyboard.upArrowKey.isPressed?1:0)-(keyboard.sKey.isPressed||keyboard.downArrowKey.isPressed?1:0));
                if(keyboard.eKey.wasPressedThisFrame){InteractTown();return;}
            }
            var pointer=Pointer.current;
            if(pointer!=null&&pointer.press.wasPressedThisFrame&&!TownPointerOverUI(pointer.position.ReadValue()))TapPlaza(pointer.position.ReadValue());
            if(input.sqrMagnitude>.01f)Town.Move(input,real);else Town.Tick(real);
            World.PresentTown(Town,real);UI.RefreshPlaza();
        }
        public void ReturnTown()
        {
            SettleRiftAttendance(combatClock.Sample(Time.realtimeSinceStartupAsDouble));
            ExitIdle(false);RestoreForegroundClock();
            CancelPotionDeparture();string visit=Combat?.State.training<0?"return:"+Combat.State.id:PotionVisit();
            Comparison=null;ComparisonError="";
            if(Combat!=null){if(Active)Combat.Abandon();Combat.Visual-=World.Effect;Combat=null;}
            Store.Data.suspendedRun=null;Store.Data.repeatHunt=null;repeatRestored=false;World.ClearDungeon();Save();UI.ShowTown();
            VisitSanctuary(visit);
        }
        public void EditBuild()
        {UI.ShowBuild();}
        public bool SaveTrainingPreset(int slot,string name=null)
        {
            if(Combat==null||!Combat.OwnedTraining||slot<0||slot>=HuntEdict.PresetSlots||Combat.Hero.id!=Store.Data.Hero.id)return false;
            var build=Combat.State.build.Copy();if(BehaviorRules.Validate(build,Combat.Hero.heroClass).Count>0)return false;
            build.equipmentIds=Combat.Hero.inventory.Where(item=>item.equipped).Select(item=>item.id).ToList();
            build.equipmentPositions=Combat.Hero.inventory.Where(item=>item.equipped).Select(item=>item.equipIndex).ToList();
            if(Store.SaveBuildPreset(Store.Data.Hero.id,slot,build,name??build.name))return true;
            Notify(Store.Error);return false;
        }
        public void CommitBuild(BuildConfig build,bool remainPaused=false)
        {
            if(ComparisonRun){Notify("A/B 비교에서는 설정을 고정합니다. 결과에서 B 설정을 작성해 주세요.");return;}
            if(Active&&Combat.State.training>=0)
            {
                if(!Combat.ApplyBuild(build)){Notify(Loc.F("설정 오류를 수정해 주세요. {0}", Combat.State.logs[Combat.State.logs.Count-1]));return;}
                Notice="";Combat.State.paused=remainPaused;UI.ShowBattle();return;
            }
            if(!Store.CommitCurrentBuild(build,catalog,Active?Combat:null,remainPaused))
            {Notify(Store.Error);return;}
            Notice="";
            if(Active)UI.ShowBattle();else UI.ShowTown();
        }

        public void RecordGuide(Func<bool> change){if(change())Save();}
        public void Notify(string message){Notice=message;UI.ShowToast(message);}
        public void Save()
        {
            if(Combat!=null)Store.Data.suspendedRun=Combat.State.training<0&&Active?Combat.State:null;
            if(Combat!=null&&!Active&&Store.Data.repeatHunt?.runId==Combat.State.id)Store.Data.repeatHunt.pendingResult=Combat.State;
            if(!Store.Save()){Notice=Store.Error;BlockRepeat(RepeatBlock.Save,Store.Error);PreserveIdleSaveFailure();}
        }
        float forgeSettlementTick;
        void Update()
        {
            if(Store!=null&&Time.unscaledTime>=forgeSettlementTick){forgeSettlementTick=Time.unscaledTime+1;Store.SettleForgeJobs();}
            double elapsed=combatClock.Sample(Time.realtimeSinceStartupAsDouble);
            UpdateDisplaySettings();
            float repeatReal=UpdateRepeatClock();
            TickPotionWait();
            if(Combat==null){TickPlaza(Mathf.Min(Time.unscaledDeltaTime,.25f));return;}
            // This display-only pause is not serialized into the run. Existing pause reasons
            // and the partial simulation tick remain exactly as they were on entry.
            SettleRiftAttendance(elapsed);
            if(UI.CommonPanelOpen&&Active){saveClock+=(float)elapsed;if(saveClock>=3){saveClock=0;Save();}return;}
            var run=Combat.State;float real=(float)elapsed;bool wasActive=Active;
            if(Active&&!backgroundPaused){if(run.riftAttendance?.version==1)run.realTime=(float)(run.riftAttendance.elapsedMs/1000);else run.realTime+=real;}
            if(Active&&!run.paused&&!run.portal&&!backgroundPaused&&!foregroundResumeRequired&&string.IsNullOrEmpty(run.navigationError))
            {
                combatClock.Accumulate(elapsed,EffectiveSpeed);int guard=0;
                while(guard++<ForegroundCombatClock.TickBudget&&Active&&!run.paused&&!run.portal&&string.IsNullOrEmpty(run.navigationError)&&combatClock.TakeTick())
                {using var sample=PresentationMetrics.Combat.Auto();PresentationMetrics.CombatTicks++;Combat.Tick(CombatSimulation.Step);}
                if(Active&&combatClock.CheckBacklog(elapsed,EffectiveSpeed))PauseForForeground("기기 상태로 사냥이 일시정지되었습니다. 계속하려면 재개를 눌러 주세요.");
            }
            else combatClock.Pause();
            if(!DisplayDimmed&&!backgroundPaused)World.Present(run,Mathf.Min(real,.25f));
            if(run.portal&&!portalCleanupTried)TryPortalCleanup();else if(!run.portal)portalCleanupTried=false;
            if(run.portal&&!IdleHunting&&UI.Page!="bag"&&UI.Page!="warehouse"&&UI.Page!="gem-menu")UI.ShowBag(true);
            if(!Active&&!resultShown){resultShown=true;if(run.training<0)CompleteHuntResult();else Save();if(ComparisonRun)CompleteComparison();if(!IdleHunting)UI.ShowResult();}
            if(!Active&&!wasActive&&!backgroundPaused&&!foregroundResumeRequired&&!UI.BlocksRepeat&&(IdleHunting||UI.Page=="result")&&run.training<0)TickRepeat(repeatReal);
            if(!backgroundPaused){saveClock+=real;if(saveClock>=3){saveClock=0;Save();}}
            UpdateIdlePresentation();
        }
        void SettleRiftAttendance(double elapsed)
        {
            if(!Active||Combat.State.training>=0||backgroundPaused||foregroundResumeRequired)return;
            double charged=Store.AttendRift(Combat.State,elapsed*1000);
            Combat.State.realTime=(float)(Combat.State.riftAttendance.elapsedMs/1000);
            if(charged+0.001<elapsed*1000||Store.Data.riftFatigue.Total<=0)Combat.ExhaustFatigue();
        }
        void OnApplicationPause(bool paused)
        {
            if(paused)SettleRiftAttendance(combatClock.Sample(Time.realtimeSinceStartupAsDouble));
            bool wasPaused=backgroundPaused;backgroundPaused=paused;repeatClock=Time.realtimeSinceStartupAsDouble;combatClock.Reset(repeatClock);
            if(paused&&wasPaused)return;
            if(paused&&Combat?.State.training<0){PauseForForeground("앱을 나가 사냥을 보존했습니다. 계속하려면 재개를 눌러 주세요.");ExitIdle(false);}
            if(!paused&&DisplaySettings?.Pending!=0){displayRequestStarted=Time.realtimeSinceStartup;displayRequestFrame=Time.frameCount;}
            if(!paused&&wasPaused&&Store!=null&&!Store.SettleLocalIdle())
            {
                foregroundSaveBlocked=foregroundResumeRequired=true;
                ForegroundPauseReason="미실행 보상을 저장하지 못했습니다. 저장 공간을 확인한 뒤 재개를 눌러 주세요.";
                if(Active)Combat.State.paused=true;Notify(ForegroundPauseReason);return;
            }
            if(!paused&&foregroundResumeRequired)Notify(ForegroundPauseReason);
            if(!paused&&wasPaused&&!string.IsNullOrEmpty(Store?.OfflineMessage))Notify(ForegroundPauseReason+"\n"+Store.OfflineMessage);
            if(Store!=null)Save();
        }
        // Desktop players can suspend updates on focus loss without a mobile pause callback.
        void OnApplicationFocus(bool focused){if(!focused){Town?.Cancel();UI?.ResetTownInput();}repeatClock=Time.realtimeSinceStartupAsDouble;combatClock.Reset(repeatClock);}
        void OnApplicationQuit(){if(Store!=null){SettleRiftAttendance(combatClock.Sample(Time.realtimeSinceStartupAsDouble));Save();}}
    }
}
