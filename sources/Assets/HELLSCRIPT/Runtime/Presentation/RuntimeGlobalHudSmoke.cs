#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Hellscript
{
    public sealed class RuntimeGlobalHudSmoke:MonoBehaviour
    {
        GameController game;string output;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Boot()
        {
            if(!Debug.isDebugBuild||!Environment.GetCommandLineArgs().Contains("-hellscriptGlobalHudSmoke"))return;
            Application.runInBackground=true;
            Application.logMessageReceived+=(m,s,t)=>{if(t==LogType.Exception)Application.Quit(1);};
            new GameObject("Global HUD acceptance").AddComponent<RuntimeGlobalHudSmoke>();
        }
        static void Require(bool ok,string message){if(!ok)throw new InvalidOperationException(message);}
        IEnumerator Resize(int w,int h)
        {Screen.SetResolution(w,h,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.5f);game.UI.RefreshHud();yield return null;}
        IEnumerator Capture(string name)
        {
            yield return new WaitForSecondsRealtime(.22f);Canvas.ForceUpdateCanvases();yield return new WaitForEndOfFrame();
            var image=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes(Path.Combine(output,name+".png"),image.EncodeToPNG());Destroy(image);
        }
        void Click(string name)=>game.UI.GetComponentsInChildren<Button>().Single(b=>b.name==name).onClick.Invoke();
        void CheckRects(string name)
        {
            var hud=game.UI.GlobalHud;var layout=hud.Layout;
            foreach(var group in new[]{("Active",layout.actives),("Potion",layout.potions)})
                for(int i=0;i<group.Item2.Length;i++)
                {
                    var rect=(RectTransform)hud.transform.Find("HUD safe area/"+group.Item1+" "+i);var wanted=layout.Pixels(group.Item2[i]);
                    Require(Vector2.Distance(rect.anchoredPosition,wanted.position)<=2*layout.scale&&Vector2.Distance(rect.sizeDelta,wanted.size)<=2*layout.scale,"Layout mismatch: "+name);
                    if(group.Item1!="Potion")
                    {
                        var icon=rect.GetComponentInChildren<SkillIconView>();bool passive=false;
                        Require(icon!=null&&icon.Passive==passive,"Skill frame kind mismatch: "+name);
                        Require(icon.Plate.sprite.name==(passive?"mask-square":"mask-circle"),"Skill clipping shape mismatch: "+name);
                        Require(icon.Frame.enabled&&icon.Frame.sprite.name=="frame-skill","Casual skill frame missing: "+name);
                    }
                }
            foreach(var label in hud.GetComponentsInChildren<Text>().Where(t=>t.name=="Caption"&&t.transform.parent.name.StartsWith("Passive")))
                Require(label.preferredWidth<=label.rectTransform.rect.width+1,"Passive captions overlap: "+name);
            foreach(var label in hud.StatusStrip.Viewport.GetComponentsInChildren<Text>().Where(t=>(t.name=="Remaining time"||t.name=="Stacks")&&!string.IsNullOrEmpty(t.text)))
                Require(label.preferredHeight<=label.rectTransform.rect.height+1,"Status line clips: "+name+" "+label.name+" "+label.text+" preferred="+label.preferredHeight+" actual="+label.rectTransform.rect.height);
            File.AppendAllText(Path.Combine(output,"geometry.txt"),name+" "+Screen.width+"x"+Screen.height+" scale="+layout.scale+" skills="+layout.actives[0]+" potions="+layout.potions[0]+" XP="+layout.xp+" PASS <=2 logical px\n");
        }
        // Presentation fixtures only: do not create/select heroes or mutate the account.
        IEnumerator VerifyCasualHudStates()
        {
            string accountBefore=JsonUtility.ToJson(game.Store.Data);var hud=game.UI.GlobalHud;
            var face=hud.transform.Find("HUD safe area/Class seal/Circle/Selected class").GetComponent<Image>();
            var ring=hud.transform.Find("HUD safe area/Class seal/Seal health ring").GetComponent<Image>();
            var hpBorder=hud.transform.Find("HUD safe area/HP/Border").GetComponent<Image>();
            var seen=new HashSet<Sprite>();
            foreach(var heroClass in new[]{HeroClass.Warrior,HeroClass.Ranger,HeroClass.Mage})
            foreach(var gender in new[]{HeroGender.Male,HeroGender.Female})
            {
                var d=GlobalHudSnapshot.Sample();d.heroClass=heroClass;d.gender=gender;game.UI.SetHudPreview(d);yield return null;
                string id="seal-"+heroClass.ToString().ToLowerInvariant()+"-"+(gender==HeroGender.Female?"f":"m");
                var expectedFace=Resources.Load<Sprite>("Art/GlobalHUD/Casual/"+id);
                Require(expectedFace!=null&&face.sprite==expectedFace&&seen.Add(face.sprite),"HUD did not bind a distinct class/gender seal: "+id);
                yield return Capture("casual-hud-"+id);
            }
            var sample=GlobalHudSnapshot.Sample(40);sample.health=sample.maxHealth*.31f;game.UI.SetHudPreview(sample);yield return null;
            Require(!ring.enabled,"Healthy HUD seal has a danger ring");
            sample.health=sample.maxHealth*.29f;game.UI.SetHudPreview(sample);yield return null;
            Require(ring.enabled&&hpBorder.color==ring.color&&ring.color!=hud.Style.deadTint,"Low HP seal/HP ring missing");yield return Capture("casual-hud-low-hp");
            sample.dead=true;sample.health=0;game.UI.SetHudPreview(sample);yield return null;
            Require(ring.enabled&&ring.color==hud.Style.deadTint&&face.material.HasProperty("_Tone")&&face.material.GetVector("_Tone").x>.99f,"Dead HUD seal is not grey");
            Require(hud.transform.Find("HUD safe area/HP/HP value").GetComponent<Text>().text.Contains(Loc.T("사망")),"Dead HP label missing");yield return Capture("casual-hud-dead");
            sample.dead=false;sample.health=sample.maxHealth;sample.actives[0].insufficient=true;sample.actives[0].remaining=0;
            sample.actives[1].locked=true;sample.actives[1].remaining=0;sample.actives[2].empty=true;sample.actives[2].id="";sample.actives[2].atlas=-1;
            sample.ultimate=new HudSlotState{id="M17",name=Loc.T("궁극기"),caption=Loc.T("궁극기")};game.UI.SetHudPreview(sample);yield return null;
            Require(hud.SkillSlot(0).Find("Cooldown time").GetComponent<Text>().text=="◆","Resource shortage diamond missing");
            Require(hud.SkillSlot(1).Find("Cooldown time").GetComponent<Text>().text=="—","Locked skill marker missing");
            Require(!hud.SkillSlot(2).GetComponentInChildren<SkillIconView>().Icon.enabled,"Empty skill retained an icon");
            var ultimate=hud.SkillSlot(4).GetComponentInChildren<SkillIconView>();Require(ultimate.Frame.sprite.name=="frame-seal"&&ultimate.Plate.sprite.name=="mask-circle","Ultimate frame/mask changed");
            var strip=hud.StatusStrip;Require(strip.VisibleEntries==43,"The complete 40-effect plus 3-passive list was truncated");
            foreach(var entry in strip.Viewport.Find("Effects").GetComponentsInChildren<Button>())
            {
                var skill=entry.GetComponentInChildren<SkillIconView>();
                if(skill!=null&&skill.gameObject.activeSelf)Require(skill.Frame.sprite.name=="frame-status"&&skill.Plate.sprite.name=="mask-square","Status skill frame/mask changed");
                else Require(entry.transform.Find("Effect frame").GetComponent<Image>().sprite.name=="frame-status","Status effect frame missing");
            }
            strip.ScrollTo(1);Require(strip.Offset>0&&strip.FadePixels.y==0,"Full status list cannot reach its last entry");yield return Capture("casual-hud-shortage-locked-empty-full-status");
            Require(accountBefore==JsonUtility.ToJson(game.Store.Data),"HUD presentation fixtures changed the account");
            game.UI.SetHudPreview(GlobalHudSnapshot.Sample());hud.StatusStrip.ScrollTo(0);
            File.WriteAllText(Path.Combine(output,"casual-hud-states.txt"),"PASS six distinct class/gender seals, healthy/low/dead HP presentation, shortage diamond, locked/empty skills, ultimate frame, complete 40-effect plus 3-passive list and square status frames; presentation fixtures preserve the account.\n");
            Debug.Log("HELLSCRIPT_CASUAL_HUD_STATES_OK");
        }
        void CheckDeterministicState(CombatSimulation control,string phase)
        {
            string actualRun=JsonUtility.ToJson(game.Combat.State),expectedRun=JsonUtility.ToJson(control.State);
            string actualHero=JsonUtility.ToJson(game.Combat.Hero),expectedHero=JsonUtility.ToJson(control.Hero);
            // Use the existing RestoreFidelityTests save-form contract for JSON's damage-total rounding.
            string Saved(string json)=>JsonUtility.ToJson(JsonUtility.FromJson<RunState>(json));
            var a=game.Combat.State;var b=control.State;
            bool critical=a.rng==b.rng&&a.rewardRng==b.rewardRng&&a.position.Equals(b.position)&&a.health==b.health&&
                JsonUtility.ToJson(a.enemies)==JsonUtility.ToJson(b.enemies)&&JsonUtility.ToJson(a.drops)==JsonUtility.ToJson(b.drops);
            if(Saved(actualRun)==Saved(expectedRun)&&actualHero==expectedHero&&critical)return;
            File.WriteAllText(Path.Combine(output,phase+"-actual-run.json"),actualRun);
            File.WriteAllText(Path.Combine(output,phase+"-control-run.json"),expectedRun);
            File.WriteAllText(Path.Combine(output,phase+"-actual-hero.json"),actualHero);
            File.WriteAllText(Path.Combine(output,phase+"-control-hero.json"),expectedHero);
            Require(false,phase=="baseline"?"Restored deterministic fixture differs before HUD ticks.":"HUD affected deterministic combat.");
        }
        IEnumerator VerifyPortraitDpsComparison()
        {
            const System.Reflection.BindingFlags access=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
            game.ApplyLanguage("en");yield return Resize(440,956);game.UI.SetHudPreview(null);
            var previous=typeof(GameUI).GetField("trainingPreviousDamage",access);var second=typeof(GameUI).GetField("trainingHudSecond",access);
            var saved=previous.GetValue(game.UI);var run=game.Combat.State;float time=run.time;
            try
            {
                previous.SetValue(game.UI,new[]{100f,100f,100f});run.time=3;second.SetValue(game.UI,-1);
                typeof(GameUI).GetMethod("RefreshTrainingHud",access).Invoke(game.UI,new object[]{run});game.UI.RefreshCanvasLayout();Canvas.ForceUpdateCanvases();
                var delta=(Text)typeof(GameUI).GetField("trainingDelta",access).GetValue(game.UI);var panel=(RectTransform)delta.transform.parent;
                Require(delta.text==Loc.F("직전 판 같은 시점 대비 {0}",TrainingGroundResultWindow.Percent(-1)),"Previous-run comparison fixture did not reach the real HUD: "+delta.text);
                Require(delta.horizontalOverflow==HorizontalWrapMode.Wrap,"Portrait DPS comparison lost wrapping");
                using var generator=new TextGenerator();Require(generator.Populate(delta.text,delta.GetGenerationSettings(delta.rectTransform.rect.size)),"Cannot generate portrait DPS comparison");
                Require(generator.lineCount>=2&&generator.characterCountVisible>=delta.text.Length,"Portrait comparison did not show both complete lines");
                foreach(var vertex in generator.verts)
                {
                    var point=panel.InverseTransformPoint(delta.transform.TransformPoint(vertex.position/delta.pixelsPerUnit));var bounds=panel.rect;
                    Require(point.x>=bounds.xMin-.5f&&point.x<=bounds.xMax+.5f&&point.y>=bounds.yMin-.5f&&point.y<=bounds.yMax+.5f,"Portrait DPS comparison glyph escaped the panel");
                }
                yield return Capture("21-portrait-dps-comparison-en");File.WriteAllText(Path.Combine(output,"dps-portrait-comparison.txt"),"PASS real previous-run -100% comparison wraps completely inside the English portrait DPS panel.\n");
            }
            finally{previous.SetValue(game.UI,saved);run.time=time;second.SetValue(game.UI,-1);game.UI.RefreshHud();}
        }
        IEnumerator Start()
        {
            var args=Environment.GetCommandLineArgs();int save=Array.IndexOf(args,"-hellscriptSavePath"),shots=Array.IndexOf(args,"-hellscriptScreenshots");Require(save>=0&&shots>=0,"Use isolated save and evidence directories.");output=args[shots+1];Directory.CreateDirectory(output);
            yield return new WaitForSecondsRealtime(1);game=FindAnyObjectByType<GameController>();Require(game?.Store!=null,"Game failed to initialize.");
            game.Store.Data.guide.legacyExempt=true;
            if(!HeroSlots.IsCreated(game.Store.Data,2))Require(game.Store.CreateCharacter(2),game.Store.Error);
            Require(HeroSlots.IsCreated(game.Store.Data,2),"The returning HUD fixture must own its mage before selection.");
            Tutorials.Switch(game.Store.Data,game.Store.Data.selectedHero,2);D4DevelopmentFixture.OpenEdict(game);game.Store.Data.guide.hintsHidden=true;game.Store.Data.guide.mapComplete=true;
            Require(game.AttendancePopups.SetHidden(AttendanceKind.Weekly,true,game.Store.AttendanceClock())&&game.AttendancePopups.SetHidden(AttendanceKind.Monthly,true,game.Store.AttendanceClock()),game.AttendancePopups.Error);game.UI.CloseAttendance();
            var hero=game.Store.Data.Hero;hero.level=24;hero.build=GameCatalog.Preset(HeroClass.Mage,0);hero.build.passives=new[]{0,3,4};hero.build.activeSkills=new List<int>{12,13,14,15};hero.build.autoRepeat=false;
            game.Begin(seed:7421);Require(game.Combat!=null,"No combat.");game.Combat.State.paused=true;game.enabled=false;UiSafeArea.AspectRatio=0;
            game.UI.SetHudPreview(GlobalHudSnapshot.Sample());
            yield return Resize(1600,900);game.World.Present(game.Combat.State,.1f);
            Require(game.UI.BattleViewport==UiSafeArea.FrameNormalized,"HUD cropped the world camera.");
            Require(game.UI.GlobalHud.Layout.potions.Length==3,"Exactly three potion slots are required.");
            if(args.Contains("-hellscriptCasualHudAcceptance"))yield return VerifyCasualHudStates();
            yield return Capture("01-landscape-collapsed");
            game.UI.GlobalHud.StatusStrip.Toggle();yield return Capture("02-landscape-expanded");
            game.UI.GlobalHud.StatusStrip.ScrollTo(.5f);yield return Capture("03-landscape-middle");
            game.UI.GlobalHud.StatusStrip.ScrollTo(1);yield return Capture("04-landscape-end");
            foreach(int n in new[]{0,4,5,8,12,40})
            {game.UI.SetHudPreview(GlobalHudSnapshot.Sample(n));Require(game.UI.GlobalHud.StatusStrip.VisibleEntries==n+3,"Effects were dropped.");}
            game.UI.SetHudPreview(GlobalHudSnapshot.Sample());
            yield return Resize(900,1600);game.World.Present(game.Combat.State,.1f);yield return Capture("05-portrait");
            yield return Resize(640,360);game.World.Present(game.Combat.State,.1f);yield return Capture("06-small-landscape");
            yield return Resize(360,640);game.World.Present(game.Combat.State,.1f);yield return Capture("07-small-portrait");
            game.UI.RefreshCanvasLayout();yield return Resize(1600,900);
            UiSafeArea.Simulate(90,30,12,24);game.UI.RefreshHud();yield return Capture("09-safe-area");UiSafeArea.StopSimulating();
            game.UI.SetHudPreview(null);game.UI.RefreshHud();yield return Capture("10-live-data");
            foreach(string language in new[]{"ko","en"})
            { const int percent=100;
                game.ApplyLanguage(language);game.UI.RefreshCanvasLayout();
                foreach(var size in new[]{new Vector2Int(1600,900),new Vector2Int(900,1600),new Vector2Int(2000,900),new Vector2Int(1200,900),new Vector2Int(360,640)})
                {
                    yield return Resize(size.x,size.y);game.UI.SetHudPreview(GlobalHudSnapshot.Sample());CheckRects(language+"-"+percent);
                    if(language=="en"&&size.x==900)yield return Capture("11-portrait-english");
                    if(language=="en"&&size.x==360)yield return Capture("12-small-english-large");
                    if(language=="ko"&&size.x==360)yield return Capture("13-small-minimum");
                }
            }
            yield return VerifyPortraitDpsComparison();
            game.ApplyLanguage("ko");game.UI.RefreshCanvasLayout();yield return Resize(1600,900);game.UI.SetHudPreview(GlobalHudSnapshot.Sample(40));
            var strip=game.UI.GlobalHud.StatusStrip;strip.ScrollTo(1);Require(strip.Offset>0&&strip.FadePixels.y==0,"Last effect inaccessible.");
            var scroller=strip.Viewport.GetComponent<HudStatusScroll>();var pointer=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left,position=new Vector2(300,200)};
            scroller.OnBeginDrag(pointer);strip.Viewport.Find("Effects/Effect 0").GetComponent<Button>().onClick.Invoke();Require(!game.UI.CommonPanelOpen,"Drag triggered inspection.");
            game.UI.SetHudPreview(GlobalHudSnapshot.Sample(12));Require(strip.VisibleEntries==43,"Dragging did not retain expiry placeholders.");scroller.OnEndDrag(pointer);yield return null;yield return null;Require(strip.VisibleEntries==15,"Deferred effect changes were lost.");
            strip.ScrollTo(0);strip.Viewport.Find("Effects/Effect 0").GetComponent<Button>().onClick.Invoke();Require(!game.UI.CommonPanelOpen&&game.UI.GlobalHud.transform.Find("Status speech bubble").gameObject.activeSelf,"Effect bubble unavailable.");yield return Capture("14-effect-bubble");
            yield return Resize(900,1600);yield return Capture("15-bubble-rotated");Require(game.Combat.State.paused,"Bubble changed original pause state.");
            game.UI.SetHudPreview(null);var run=game.Combat.State;run.health=game.Combat.Stats.hp*.1f;run.resource=0;run.potionCd=0;run.potions.resourceCooldown=run.potions.utilityCooldown=0;run.paused=false;
            int hpStock=hero.potions.Count("PH01"),mpStock=hero.potions.Count("PM01"),utilityStock=hero.potions.Count("PU04");
            game.Combat.Tick(.05f);run.paused=true;game.UI.RefreshHud();yield return Capture("17-real-potion-use");
            Require(hero.potions.Count("PH01")==hpStock-1&&hero.potions.Count("PM01")==mpStock-1&&hero.potions.Count("PU04")==utilityStock-1,"Live automatic potion use failed.");
            Require(game.UI.GlobalHud.Snapshot.potions[0].count==hero.potions.Count("PH01")&&game.UI.GlobalHud.Snapshot.effects.Any(e=>e.id=="PU04"),"HUD did not read real supplies and utility buff.");
            string stateBefore=JsonUtility.ToJson(run),stockBefore=JsonUtility.ToJson(hero.potions);yield return Capture("18-paused-timers");Require(stateBefore==JsonUtility.ToJson(run)&&stockBefore==JsonUtility.ToJson(hero.potions),"HUD advanced the simulation.");
            var controlAccount=JsonUtility.FromJson<AccountSave>(JsonUtility.ToJson(game.Store.Data));run.paused=false;
            // A comparison clone must not add the resume journal event of a real relaunch.
            var control=new CombatSimulation(controlAccount,game.catalog,run.stage,restore:JsonUtility.FromJson<RunState>(JsonUtility.ToJson(run)),recordResume:false);
            CheckDeterministicState(control,"baseline");
            for(int i=0;i<100;i++){game.Combat.Tick(.05f);control.Tick(.05f);game.UI.RefreshHud();}
            CheckDeterministicState(control,"after-hud");run.paused=true;
            game.ReturnTown();hero=game.Store.Data.Hero;hero.useEdict=true;hero.edict=HuntEdictV2Storage.CreateForHero(hero);hero.edict=HuntEdictV2Editing.WithGlobal(hero.edict,"potion.autoBuy","OFF");hero.potions.Set("PH01",0);game.Store.Data.gold=0;game.Begin(seed:7422);
            Require(string.IsNullOrEmpty(game.Store.Error),game.Store.Error);Require(game.Combat==null,"HP_EMPTY allowed departure without HP potions.");yield return Capture("19-departure-wait");
            Require(!game.UI.CommonPanelOpen,"A content overlay intercepted the potion wait fixture.");hero.edict=HuntEdictV2Editing.WithGlobal(hero.edict,"potion.shortage","CONTINUE");Require(game.Store.Save(),game.Store.Error);game.enabled=true;yield return new WaitForSecondsRealtime(.4f);game.enabled=false;Require(game.Combat!=null,"Saved shortage change did not resume preparation.");game.Combat.State.paused=true;
            hero.edict=HuntEdictV2Editing.WithGlobal(hero.edict,"potion.autoBuy","ON");hero.potions.Set("PH01",0);hero.potions.Set("PM01",0);hero.potions.Set("PU04",0);game.Store.Data.gold=1000;game.ReturnTown();
            Require(game.Store.Data.gold==650&&hero.potions.Count("PH01")==20&&hero.potions.Count("PM01")==20&&hero.potions.Count("PU04")==10,"Sanctuary purchase priority or prices failed.");
            int gold=game.Store.Data.gold;for(int i=0;i<5;i++)game.UI.ShowTown();Require(game.Store.Data.gold==gold,"Redraw purchased potions.");yield return Capture("20-sanctuary-supplies");
            var joystick=game.UI.GetComponentInChildren<TownJoystick>();var corners=new Vector3[4];((RectTransform)joystick.transform).GetWorldCorners(corners);
            var hudLayout=game.UI.GlobalHud.Layout;Require(corners[0].y>=UiSafeArea.Current.yMin+hudLayout.status.yMax*hudLayout.scale,"Town joystick overlaps the HUD.");
            File.AppendAllText(Path.Combine(output,"geometry.txt"),"PASS real automatic use, separate cooldowns, utility buff, paused rendering purity, 100-tick deterministic comparison, HP_EMPTY wait, saved CONTINUE reevaluation, sanctuary 350-gold purchase, no redraw purchase.\n");
            File.WriteAllText(Path.Combine(output,"runtime.txt"),"PASS: shared production HUD, three square potions/four circular active skills; equipped legacy passives in status effects, full camera frame, effect counts 0/4/5/8/12/40, both orientations and safe area.\n");
            Debug.Log("HELLSCRIPT_GLOBAL_HUD_SMOKE_OK");Application.Quit(0);
        }
    }
}

#endif
