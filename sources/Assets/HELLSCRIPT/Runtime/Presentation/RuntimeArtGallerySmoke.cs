using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Hellscript
{
    // Art review gallery for the world overhaul (development builds only):
    //   HELLSCRIPT -hellscriptArtGallerySmoke -hellscriptSavePath <dir> -hellscriptScreenshots <dir>
    // Writes NN-<shot>.png plus runtime.txt (one claim per shot) and logs HELLSCRIPT_ART_GALLERY_SMOKE_OK.
    // Every rift shot is a synthetic run resumed while paused; the smoke only presents it, and checks that the
    // run's JSON is unchanged afterwards. Later phases append to Shots(); enemy kinds and bosses follow the name tables.
    public sealed class RuntimeArtGallerySmoke:MonoBehaviour
    {
        enum ShotKind{Room,Enemies,Boss,Elites,Effect,Town}
        sealed class Shot
        {
            public ShotKind kind;public string name;public int field,first,count,pattern,effect,width=1600,height=900;public float age;public TownStation station;
        }
        const int Stage=40,EnemyBase=900;
        // Every Visual kind Core emits (VFX.md coverage checklist), sent through WorldView.Effect, with the presentation
        // age (seconds) to capture it mid-life. A kind that spawns no transient yet is recorded as SKIP.
        static readonly (int kind,float age)[] Effects={(0,.1f),(1,.15f),(2,.1f),(3,.15f),(4,.15f),(5,.2f),(6,.1f),(8,.15f),(9,.1f),(10,.15f),(11,.15f),(12,.15f),(13,.15f),(14,.06f),(15,.1f),(16,.15f),(17,.15f),(18,.06f),(20,.2f),(30,.06f),(31,.06f),(32,.06f)};
        // (enemy kind, elite trait); the two trait-2 elites stand side by side (<= 5 m) so their life link is drawn.
        static readonly (int kind,int trait)[] EliteExamples={(6,2),(8,2),(0,0),(2,1),(1,3),(10,5)};

        static List<Shot> Shots()
        {
            var shots=new List<Shot>();
            for(int f=0;f<6;f++)shots.Add(new Shot{kind=ShotKind.Room,name="room-field"+f,field=f});
            shots.Add(new Shot{kind=ShotKind.Room,name="room-field0-956x440",field=0,width=956,height=440});
            shots.Add(new Shot{kind=ShotKind.Room,name="room-field1-440x956",field=1,width=440,height=956});
            int kinds=GameCatalog.EnemyNames.Length;
            for(int k=0;k<kinds;k+=6)shots.Add(new Shot{kind=ShotKind.Enemies,name=$"enemies-{k:00}-{Mathf.Min(kinds,k+6)-1:00}",first=k,count=Mathf.Min(6,kinds-k)});
            for(int p=0;p<GameCatalog.BossNames.Length;p++)shots.Add(new Shot{kind=ShotKind.Boss,name="boss-"+p,pattern=p});
            shots.Add(new Shot{kind=ShotKind.Elites,name="elites"});
            foreach(var (kind,age) in Effects)shots.Add(new Shot{kind=ShotKind.Effect,name=$"fx-{kind:00}",effect=kind,age=age});
            shots.Add(new Shot{kind=ShotKind.Town,name="town-portal",station=TownStation.RiftKeeper});
            shots.Add(new Shot{kind=ShotKind.Town,name="town-blacksmith",station=TownStation.Blacksmith});
            return shots;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if(!Debug.isDebugBuild||!Environment.GetCommandLineArgs().Contains("-hellscriptArtGallerySmoke"))return;
            Application.runInBackground=true;Application.logMessageReceived+=(m,s,t)=>{if(t==LogType.Exception)Application.Quit(1);};
            new GameObject("Art gallery smoke").AddComponent<RuntimeArtGallerySmoke>();
        }
        static void Require(bool pass,string why){if(!pass)throw new InvalidOperationException(why);}
        static string Arg(string name){var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,name);return i>=0&&i+1<args.Length?args[i+1]:null;}
        // The game itself advances these clocks every frame, paused or not; everything else must stay identical.
        static string Json(RunState run)=>Regex.Replace(JsonUtility.ToJson(run),"\"(realTime|elapsedMs)\":[^,}]*","");
        static readonly FieldInfo FieldMember=typeof(RiftLayout).GetField("field");
        static int FieldOf(RiftLayout layout){int f=FieldMember==null?-1:(int)FieldMember.GetValue(layout);return f>=0?f:layout.theme;}
        static int ThemeOf(int field)=>field==0||field==3||field==4?0:1;

        GameController game;string output;int number;RunState run,effectRun;string frozen;Color32[] lastFrame;
        readonly List<string> claims=new List<string>();

        IEnumerator Start()
        {
            output=Arg("-hellscriptScreenshots");Require(Arg("-hellscriptSavePath")!=null&&output!=null,"Art gallery needs -hellscriptSavePath and -hellscriptScreenshots.");
            Directory.CreateDirectory(output);yield return new WaitForSecondsRealtime(1);game=FindAnyObjectByType<GameController>();Require(game!=null&&game.Store!=null,"No game store.");
            var a=game.Store.Data;a.guide.mapComplete=true;a.guide.hintsHidden=true;a.Hero.level=30;a.Hero.highestClear=Stage;
            ContentUnlocks.Reconcile(a);Require(game.Store.Save(),game.Store.Error);
            // Attendance popups would cover the town shots (RuntimeLiveOpsSmoke precedent).
            game.AttendancePopups.SetHidden(AttendanceKind.Weekly,true,game.Store.AttendanceClock());game.AttendancePopups.SetHidden(AttendanceKind.Monthly,true,game.Store.AttendanceClock());game.UI.CloseAttendance();
            yield return Resize(1600,900);
            foreach(var shot in Shots())
            {
                if(shot.kind==ShotKind.Town)yield return TownShot(shot);
                else if(shot.kind==ShotKind.Effect)yield return EffectShot(shot);
                else yield return RiftShot(shot);
            }
            if(game.Running)game.ReturnTown();
            File.WriteAllLines(Path.Combine(output,"runtime.txt"),claims);
            Debug.Log("HELLSCRIPT_ART_GALLERY_SMOKE_OK "+number+" screenshots");Application.Quit(0);
        }

        IEnumerator RiftShot(Shot shot)
        {
            if(shot.kind==ShotKind.Room&&shot.field>1&&FieldMember==null){claims.Add($"SKIP {shot.name}: RiftLayout.field does not exist on this branch yet.");yield break;}
            RunState fixture;string what;
            if(shot.kind==ShotKind.Room)
            {
                fixture=null;
                for(uint seed=7100;seed<7500&&fixture==null;seed++){var candidate=new CombatSimulation(game.Store.Data,game.catalog,Stage,seed:seed).State;if(FieldOf(candidate.layout)==shot.field)fixture=candidate;}
                Require(fixture!=null,"No seed produced field "+shot.field+".");what=$"generated rift {fixture.layout.fingerprint} field {shot.field} theme {fixture.layout.theme} from its start";
            }
            else
            {
                fixture=Arena(0);var enemies=new List<EnemyState>();
                if(shot.kind==ShotKind.Enemies)
                    for(int i=0;i<shot.count;i++){var e=Enemy(fixture,EnemyBase+i,shot.first+i,new Vector2((i%3-1)*3.2f,3.2f+i/3*3));if(i%2==1)Windup(e,fixture.position,e.kind,1,.45f);enemies.Add(e);}
                else if(shot.kind==ShotKind.Boss)
                {
                    var boss=Enemy(fixture,EnemyBase,0,new Vector2(0,5));boss.boss=true;boss.pattern=shot.pattern;boss.brain.boss.initialized=true;
                    Windup(boss,fixture.position,(int)BossAttack.Basic,.6f,.3f);fixture.phase=RunPhase.Boss;fixture.bossId=boss.id;enemies.Add(boss);
                }
                else for(int i=0;i<EliteExamples.Length;i++)
                {
                    var (kind,trait)=EliteExamples[i];var e=Enemy(fixture,EnemyBase+i,kind,new Vector2((i%3-1)*3.4f,3.4f+i/3*3.4f));e.elite=trait;e.eliteTraits.Add(trait);enemies.Add(e);
                }
                if(shot.kind==ShotKind.Elites)foreach(var e in enemies.Where(x=>x.elite==2))e.elitePartner=enemies.First(x=>x!=e&&x.elite==2).id;
                fixture.enemies.AddRange(enemies);
                what=shot.kind==ShotKind.Boss?$"boss pattern {shot.pattern} ({GameCatalog.BossNames[shot.pattern]}) winding up a basic attack":
                    shot.kind==ShotKind.Elites?"elite traits "+string.Join(",",EliteExamples.Select(x=>x.kind+":"+x.trait)):
                    $"enemy kinds {shot.first}..{shot.first+shot.count-1}, odd slots winding up";
            }
            yield return Enter(fixture);
            yield return Resize(shot.width,shot.height);
            string facts=ActorFacts();
            yield return Capture(shot.name);
            if(shot.width!=1600||shot.height!=900)yield return Resize(1600,900);
            claims.Add($"PASS {number:00}-{shot.name} {shot.width}x{shot.height}: {what}; {facts}; RunState unchanged by presentation.");
            Leave();
        }

        IEnumerator EffectShot(Shot shot)
        {
            if(run==null||run!=effectRun)
            {var fixture=Arena(0);fixture.enemies.Add(Enemy(fixture,EnemyBase,0,new Vector2(0,3.5f)));yield return Enter(fixture);effectRun=run;}
            Vector2 hero=run.position,aim=run.enemies[0].position;
            for(int i=0;i<40&&game.World.TransientEffectCount>0;i++)Advance(.25f);Require(game.World.TransientEffectCount==0,"Previous effects are still alive after 10 s.");
            yield return Capture(null);var before=lastFrame;
            // origin, target and amount mirror the simulation's emitters (understand/vfx.md kind table).
            var (from,to,amount)=shot.effect switch
            {
                0=>(hero,hero,2.5f),1=>(hero+new Vector2(0,-2),hero,2.5f),9=>(hero,hero+new Vector2(2,-2),0f),15=>(hero+new Vector2(-2,-1),hero,0f),
                12=>(aim,aim,2.5f),14=>(hero,aim,4f),6=>(hero,aim,1f),18=>(hero,aim,1f),20=>(hero,hero,1.5f),30=>(aim,aim,20f),31=>(aim,aim,40f),32=>(hero,hero,10f),
                2=>(hero,aim,3f),3=>(hero,aim,3f),8=>(hero,aim,3f),13=>(hero,aim,3f),17=>(hero,aim,3f),_=>(hero,aim,0f)
            };
            string call=$"Effect({from},{to},{shot.effect},{amount})";
            game.World.Effect(from,to,shot.effect,amount);int spawned=game.World.TransientEffectCount;
            if(spawned==0){claims.Add($"SKIP {shot.name}: {call} spawns no transient on this branch; RunState unchanged.");Require(Json(run)==frozen,"Presentation changed the run.");yield break;}
            Advance(shot.age);int alive=game.World.TransientEffectCount;
            Require(alive>0,$"Effect kind {shot.effect} left nothing alive at {shot.age}s ({spawned} spawned).");
            yield return Capture(shot.name);int changed=Changed(before,lastFrame);
            // GAP: the transient is alive but the frame equals the arena before the effect (hidden or too small to draw).
            claims.Add($"{(changed>0?"PASS":"GAP")} {number:00}-{shot.name}: {call} spawned {spawned} transient(s), {alive} alive after {shot.age}s of presentation time; {changed} pixel(s) differ from the arena before the effect{(changed>0?"":", so it is not visible")}; RunState unchanged.");
            Require(Json(run)==frozen,"Presentation changed the run.");
        }

        IEnumerator TownShot(Shot shot)
        {
            if(game.Running){game.ReturnTown();run=null;}
            if(game.Town==null||game.UI.Page!="plaza")game.EnterPlaza(true);
            Require(game.Town!=null&&game.UI.Page=="plaza","Town did not open.");
            game.RequestStation(shot.station);game.Town.Tick(40);yield return new WaitForSecondsRealtime(.8f);
            Require(game.Town.Nearby==shot.station,"Could not reach "+shot.station+".");Require(GameObject.Find("Golden Rift Portal")!=null,"Town portal missing.");
            Require(game.UI.AttendancePanel==null&&!game.UI.CommonPanelOpen,"A window covers the town shot.");
            yield return Capture(shot.name);
            claims.Add($"PASS {number:00}-{shot.name}: town hero arrived at {shot.station} (position {game.Town.Position}), portal present, {game.World.FadedTownBuildings} faded building(s).");
        }

        // One large empty room; the view maps the field onto it once RiftLayout.field exists.
        RunState Arena(int field)
        {
            var fixture=new CombatSimulation(game.Store.Data,game.catalog,Stage,seed:7001).State;
            var map=new RiftLayout{version=RiftLayout.CurrentVersion,fingerprint="art-gallery-arena",start=Vector2.zero,gateOpen=true,roamingBoss=true,theme=ThemeOf(field)};
            map.rooms.Add(new RiftRoom{index=0,templateId="Art gallery",position=Vector2.zero,size=new Vector2(30,30)});
            FieldMember?.SetValue(map,field);
            fixture.layout=map;fixture.theme=map.theme;fixture.position=map.start;fixture.discovery=new RiftDiscovery{fingerprint=map.fingerprint};fixture.visibility=null;fixture.enemies.Clear();
            return fixture;
        }
        static EnemyState Enemy(RunState fixture,int id,int kind,Vector2 offset)
        {
            var e=new EnemyState{id=id,kind=kind,position=fixture.position+offset,health=1000,maxHealth=1000,attack=0,speed=0,cooldown=1000};
            e.brain.initialized=true;e.brain.facing=(fixture.position-e.position).normalized;return e;
        }
        static void Windup(EnemyState e,Vector2 hero,int kind,float preparation,float remaining)
        {
            var a=e.brain.action;var d=(hero-e.position).normalized;a.id=e.id;a.kind=kind;a.phase=EnemyActionPhase.Preparing;a.origin=e.position;a.direction=d;
            a.aim=kind==1||kind==7?e.position+d*8:hero;a.preparation=preparation;a.remaining=remaining;e.windup=remaining;
        }

        IEnumerator Enter(RunState fixture)
        {
            if(game.Running)game.ReturnTown();
            game.Store.Data.suspendedRun=fixture;game.Begin(resume:true);
            Require(game.Running&&game.Combat.State==fixture,"Resume did not start the gallery run: "+game.Store.Error);
            run=game.Combat.State;run.paused=true;frozen=Json(run);game.World.RevealPresentation(run);game.UI.ShowBattle();
            yield return new WaitForSecondsRealtime(.4f);
        }
        void Leave(){Require(Json(run)==frozen,"Presentation changed the run.");}
        // Presentation-only time: WorldView advances its own clocks while the run reads as unpaused, and the run is
        // paused again before the next simulation tick could see it.
        void Advance(float seconds)
        {
            run.paused=false;
            try{for(float t=0;t<seconds-.0001f;t+=1/60f)game.World.Present(run,Mathf.Min(1/60f,seconds-t));}
            finally{run.paused=true;}
        }
        string ActorFacts()
        {
            var actors=Private<Dictionary<int,GameObject>>("actors");var threats=Private<Dictionary<string,GameObject>>("enemyThreatViews");
            int shown=0;
            // Same sight rule as WorldView.Present: only enemies the hero can see are presented.
            foreach(var e in run.enemies.Where(x=>!x.dead&&Vector2.Distance(x.position,run.position)<=12&&game.Combat.Map.LineClear(run.position,x.position)))
            {
                Require(actors.TryGetValue(e.id,out var actor)&&actor.activeSelf,$"Enemy {e.id} (kind {e.kind}) is not presented.");
                Require(Mathf.Abs(actor.transform.position.y)<.001f,$"Enemy {e.id} root left the ground.");shown++;
            }
            int planned=EnemyCombat.Threats(run,game.Combat.Map).Count(),drawn=threats.Values.Count(v=>v!=null&&v.activeSelf);
            Require(planned==0||drawn>0,"Wind-up telegraphs were not drawn.");
            return $"{shown} actor root(s) active on the ground, {planned} planned telegraph(s), {drawn} threat view(s)";
        }
        T Private<T>(string name)=>(T)typeof(WorldView).GetField(name,BindingFlags.NonPublic|BindingFlags.Instance).GetValue(game.World);

        IEnumerator Resize(int w,int h)
        {
            if(Screen.width==w&&Screen.height==h)yield break;
            Screen.SetResolution(w,h,FullScreenMode.Windowed);float end=Time.realtimeSinceStartup+8;
            while((Screen.width!=w||Screen.height!=h)&&Time.realtimeSinceStartup<end)yield return null;
            Require(Screen.width==w&&Screen.height==h,$"Window stayed {Screen.width}x{Screen.height}, wanted {w}x{h}.");yield return new WaitForSecondsRealtime(.4f);
        }
        // Grabs the frame into lastFrame; with a name it is also saved as NN-<name>.png.
        IEnumerator Capture(string name)
        {
            Canvas.ForceUpdateCanvases();yield return new WaitForEndOfFrame();
            var image=ScreenCapture.CaptureScreenshotAsTexture();lastFrame=image.GetPixels32();
            if(name!=null){number++;File.WriteAllBytes(Path.Combine(output,$"{number:00}-{name}.png"),image.EncodeToPNG());}
            Destroy(image);
        }
        static int Changed(Color32[] a,Color32[] b)
        {
            if(a.Length!=b.Length)return Mathf.Max(a.Length,b.Length);
            int n=0;for(int i=0;i<a.Length;i++)if(a[i].r!=b[i].r||a[i].g!=b[i].g||a[i].b!=b[i].b)n++;return n;
        }
    }
}
