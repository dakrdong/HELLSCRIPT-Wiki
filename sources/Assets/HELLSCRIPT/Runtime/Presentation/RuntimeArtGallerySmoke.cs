#if UNITY_EDITOR || DEVELOPMENT_BUILD
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
        enum ShotKind{Room,Enemies,Boss,Elites,Effect,Fx,Town}
        sealed class Shot
        {
            public ShotKind kind;public string name;public int field,first,count,pattern,effect,width=1600,height=900;public float age;public TownStation station;public bool reveal;
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
            for(int f=0;f<3;f++)shots.Add(new Shot{kind=ShotKind.Room,name="room-field"+f+"-revealed",field=f,reveal=true});
            int kinds=GameCatalog.EnemyNames.Length;
            for(int k=0;k<kinds;k+=6)shots.Add(new Shot{kind=ShotKind.Enemies,name=$"enemies-{k:00}-{Mathf.Min(kinds,k+6)-1:00}",first=k,count=Mathf.Min(6,kinds-k)});
            for(int p=0;p<GameCatalog.BossNames.Length;p++)shots.Add(new Shot{kind=ShotKind.Boss,name="boss-"+p,pattern=p});
            shots.Add(new Shot{kind=ShotKind.Elites,name="elites"});
            foreach(var (kind,age) in Effects)shots.Add(new Shot{kind=ShotKind.Effect,name=$"fx-{kind:00}",effect=kind,age=age});
            foreach(var name in FxShots)shots.Add(new Shot{kind=ShotKind.Fx,name=name});
            for(int f=0;f<6;f++)shots.Add(new Shot{kind=ShotKind.Fx,name="fx-ambience-field"+f,field=f});
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
                else if(shot.kind==ShotKind.Fx)yield return FxShot(shot);
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
            if(shot.reveal)
            {
                // Evidence only: paint the fog texture explored and in sight so the dressed floors, walls and decor around the
                // start show past the hero's sight; the fog re-uploads only on a visibility revision, which a paused run skips.
                var tex=game.World.RiftFog.Texture;var px=tex.GetPixels32();for(int i=0;i<px.Length;i++)px[i]=new Color32(255,255,0,255);tex.SetPixels32(px);tex.Apply(false);
                what+=", fog painted open for the capture";yield return new WaitForSecondsRealtime(.2f);
            }
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

        // VFX core (WorldFx) review: monster tiers, telegraph fills, hits and deaths by surface, status overlays, loot beams,
        // fires, generic bursts and each field's ambience. The gallery drives the library directly; the actor, skill and
        // environment presenters make the same calls in later phases.
        static readonly string[] FxShots={"fx-tiers","fx-telegraphs","fx-telegraphs-boss","fx-hits","fx-hits-crit","fx-deaths","fx-status","fx-loot","fx-fire","fx-bursts","fx-hero-behind-wall","fx-class-lasting","fx-class-releases"};
        static readonly int[] SurfaceKinds={0,2,6,11,18,3,0};
        IEnumerator FxShot(Shot shot)
        {
            string name=shot.name;var fixture=Arena(shot.field);var hero=fixture.position;var enemies=new List<EnemyState>();
            MonsterTier[] tiers={MonsterTier.Normal,MonsterTier.Magic,MonsterTier.Elite,MonsterTier.Legendary,MonsterTier.Unique};
            FxStatus[] statuses={FxStatus.Burning,FxStatus.Frozen,FxStatus.Poisoned,FxStatus.Slowed,FxStatus.Stunned};
            if(name=="fx-tiers")for(int i=0;i<5;i++){var e=Enemy(fixture,EnemyBase+i,new[]{0,2,6,8,11}[i],new Vector2((i-2)*3.2f,3.5f));if(tiers[i]==MonsterTier.Elite){e.elite=0;e.eliteTraits.Add(0);}enemies.Add(e);}
            else if(name=="fx-hits"||name=="fx-hits-crit")for(int i=0;i<7;i++){var e=Enemy(fixture,EnemyBase+i,SurfaceKinds[i],new Vector2((i-3)*2.6f,3.5f));if(i==6)e.eventId="gallery-echo";enemies.Add(e);}
            else if(name=="fx-status")for(int i=0;i<5;i++)enemies.Add(Enemy(fixture,EnemyBase+i,0,new Vector2((i-2)*3.2f,3.5f)));
            else if(name=="fx-telegraphs")
            {
                var early=Enemy(fixture,EnemyBase,0,new Vector2(-6,3));Windup(early,hero,0,.65f,.5f);
                var late=Enemy(fixture,EnemyBase+1,0,new Vector2(-2.5f,5.5f));Windup(late,hero,0,.65f,.12f);
                var caster=Enemy(fixture,EnemyBase+2,3,new Vector2(3,8));Windup(caster,hero,3,1.2f,.6f);caster.brain.action.aim=hero+new Vector2(4.5f,2.5f);
                var charger=Enemy(fixture,EnemyBase+3,1,new Vector2(-8,-2.5f));Windup(charger,hero,1,.8f,.32f);
                var archer=Enemy(fixture,EnemyBase+4,8,new Vector2(8,1.5f));Windup(archer,hero,8,1.2f,.7f);
                var ringer=Enemy(fixture,EnemyBase+5,10,new Vector2(6.5f,-5.5f));
                var linkA=Enemy(fixture,EnemyBase+6,6,new Vector2(-7,7.5f));var linkB=Enemy(fixture,EnemyBase+7,8,new Vector2(-4,9.5f));
                foreach(var e in new[]{linkA,linkB}){e.elite=2;e.eliteTraits.Add(2);}linkA.elitePartner=linkB.id;linkB.elitePartner=linkA.id;
                enemies.AddRange(new[]{early,late,caster,charger,archer,ringer,linkA,linkB});
                fixture.enemyHazards.Add(new EnemyHazard{id=EnemyBase+50,actionId=EnemyBase+50,enemyId=early.id,definitionId="E02",shape=AttackShape.Ring,position=hero+new Vector2(2.5f,-4.5f),
                    end=hero+new Vector2(2.5f,-4.5f),direction=Vector2.up,createdAt=fixture.time-.6f,delay=.9f,radius=4,innerRadius=2});
            }
            else if(name=="fx-telegraphs-boss")
            {
                var slam=Enemy(fixture,EnemyBase,0,new Vector2(-2.5f,6));slam.boss=true;slam.pattern=0;slam.brain.boss.initialized=true;Windup(slam,hero,(int)BossAttack.Slam,1.2f,.45f);
                slam.brain.boss.refuges.Add(new BossRefuge{position=hero+new Vector2(-4.5f,-2.5f)});slam.brain.boss.refuges.Add(new BossRefuge{position=hero+new Vector2(4,-3)});
                var blasts=Enemy(fixture,EnemyBase+1,0,new Vector2(8,6));blasts.boss=true;blasts.pattern=2;blasts.brain.boss.initialized=true;Windup(blasts,hero,(int)BossAttack.Blasts,1.3f,.9f);
                blasts.brain.action.points.AddRange(new[]{hero+new Vector2(-1,-6),hero+new Vector2(3.5f,-6.5f),hero+new Vector2(7.5f,-4)});
                fixture.phase=RunPhase.Boss;fixture.bossId=slam.id;enemies.Add(slam);enemies.Add(blasts);
            }
            fixture.enemies.AddRange(enemies);
            yield return Enter(fixture);
            var fx=game.World.Fx;Require(fx!=null,"The presented rift has no WorldFx.");
            var actors=Private<Dictionary<int,GameObject>>("actors");var root=GameObject.Find("Rift Runtime").transform;string what;
            Vector3 At(Vector2 offset,float y=0)=>new Vector3(hero.x+offset.x,y,hero.y+offset.y);
            var propMaterial=new Material(Resources.Load<Shader>("RiftTerrain")){name="Gallery prop"};propMaterial.SetColor("_BaseColor",new Color(.24f,.22f,.21f));
            var resolvedProp=game.World.RiftFog!=null?game.World.RiftFog.Resolve(propMaterial):propMaterial;
            // A prop below the anchor (a brazier bowl under its flame) or standing on the ground under it (a drop).
            GameObject Anchor(string label,Vector3 at,PrimitiveType? prop,Vector3 size,bool standing=false)
            {
                var anchor=new GameObject(label);anchor.transform.SetParent(root,false);anchor.transform.position=at;
                if(prop.HasValue)
                {
                    var p=GameObject.CreatePrimitive(prop.Value);p.transform.SetParent(anchor.transform,false);p.transform.localPosition=new Vector3(0,standing?size.y*.5f:-size.y*.5f,0);
                    p.transform.localScale=size;Destroy(p.GetComponent<Collider>());p.GetComponent<Renderer>().sharedMaterial=resolvedProp;
                }
                return anchor;
            }
            switch(name)
            {
                case "fx-tiers":
                {
                    for(int i=0;i<5;i++)if(tiers[i]!=MonsterTier.Normal&&tiers[i]!=MonsterTier.Elite)game.World.SetMonsterTierOverride(enemies[i].id,tiers[i]);
                    for(int i=0;i<5;i++){Require(game.World.MonsterTierOf(enemies[i])==tiers[i],"Tier of "+i+" is "+game.World.MonsterTierOf(enemies[i]));fx.Tier(actors[enemies[i].id],tiers[i]);}
                    Advance(1.2f);int views=enemies.Count(e=>actors[e.id].transform.Find("Monster tier")!=null);Require(views==4,views+" tier views, wanted 4.");
                    what="tiers Normal, Magic, Elite (elite >= 0), Legendary and Unique (overrides) left to right, "+views+" tier views";break;
                }
                case "fx-telegraphs":case "fx-telegraphs-boss":
                {
                    Advance(.5f);
                    var gauge=new TelegraphGauge();var parts=EnemyCombat.Threats(run,game.Combat.Map).Select(t=>t.key+" "+t.shape+" "+gauge.Progress(run,t).ToString("0.00")).ToList();
                    int fills=root.GetComponentsInChildren<MeshRenderer>().Count(r=>r.name=="Telegraph fill"&&r.gameObject.activeInHierarchy);
                    Require(fills>=parts.Count,fills+" fills for "+parts.Count+" threats.");
                    what=$"{fills} fill view(s) for {parts.Count} threat(s) [{string.Join(", ",parts)}] plus refuges/aura/link views";break;
                }
                case "fx-hits":case "fx-hits-crit":
                {
                    bool crit=name=="fx-hits-crit";
                    for(int i=0;i<7;i++)fx.Hit(enemies[i],crit?1+i%5:0,crit,actors[enemies[i].id].transform.position+Vector3.up*1.3f);
                    Advance(.07f);what=(crit?"critical elemental":"physical")+" hits on Flesh, Bone, Metal, Crystal, Ice, Ichor and Spirit (echo) left to right";break;
                }
                case "fx-deaths":
                {
                    for(int i=0;i<7;i++)fx.Death(At(new Vector2((i-3)*2.8f,3.5f)),1,(FxSurface)i);
                    Advance(.2f);yield return Capture(name+"-burst");claims.Add($"PASS {number:00}-{name}-burst: death bursts Flesh..Spirit left to right after 0.2 s; {fx.ActiveTransients} live transient(s).");
                    Advance(1.8f);what="the ground marks 2 s after the deaths (blood, dust, scorch, crystal glow, ice, ichor)";name+="-marks";break;
                }
                case "fx-status":
                {
                    for(int i=0;i<5;i++)fx.Status(actors[enemies[i].id],statuses[i]);
                    Advance(1);what="status overlays Burning, Frozen, Poisoned, Slowed and Stunned left to right";break;
                }
                case "fx-loot":
                {
                    for(int g=0;g<5;g++)fx.LootBeam(Anchor("Gallery drop "+g,At(new Vector2((g-2)*2.6f,3)),PrimitiveType.Cube,new Vector3(.35f,.6f,.2f),true).transform,g);
                    fx.GoldGlint(Anchor("Gallery gold",At(new Vector2(0,.8f)),null,Vector3.one).transform);
                    Advance(1);what="loot beams for grades common, magic, rare, legendary and set left to right, gold glint in front";break;
                }
                case "fx-fire":
                {
                    Color[] colors={default,new Color(.7f,.4f,1),new Color(.45f,.75f,1),new Color(.45f,1,.6f)};
                    for(int i=0;i<4;i++)
                    {
                        fx.Fire(Anchor("Gallery brazier "+i,At(new Vector2((i-1.5f)*3.4f,4),1.1f),PrimitiveType.Cylinder,new Vector3(.8f,.55f,.8f)).transform,1,colors[i]);
                        fx.Fire(Anchor("Gallery torch "+i,At(new Vector2((i-1.5f)*3.4f,.5f),.9f),null,Vector3.one).transform,.5f,colors[i]);
                    }
                    fx.Smoke(Anchor("Gallery chimney",At(new Vector2(7,8),2),null,Vector3.one).transform);
                    Advance(1.2f);what="braziers (scale 1) and torches (0.5) in ember, violet, frost and fungal colours plus chimney smoke";break;
                }
                case "fx-bursts":
                {
                    string[] ids={"fire","frost","lightning","shadow","poison","physical","holy","arcane","blood","explosion","heal","impact"};
                    for(int i=0;i<ids.Length;i++)fx.Burst(ids[i],At(new Vector2((i%4-1.5f)*3.6f,(1-i/4)*3.4f+1.5f),1),1);
                    fx.Shockwave(At(new Vector2(0,-4.5f)),2.2f);fx.Beam(At(new Vector2(-6,-4),1.2f),At(new Vector2(-2,-5),1.2f));fx.Slash(At(new Vector2(5,-4.5f),1),Vector3.forward,2.2f,110);
                    Advance(.18f);what="bursts "+string.Join(", ",ids)+" (rows from the back), shockwave, beam and slash in front";break;
                }
                case "fx-class-lasting":case "fx-class-releases":
                {
                    // Class skills (WorldView.SkillFx) drawn from their state: lasting effects laid around the hero with one pulse
                    // each, or casts released after the entry so every one plays its burst. The run is re-frozen after these edits.
                    var cs=run.classSkills??(run.classSkills=new ClassSkillRuntimeState());float now=run.time;
                    ClassSkillEffect E(string id,string kind,Vector2 offset,Vector2 dir,float radius=0)=>
                        new ClassSkillEffect{id=id+":gallery",source=id,kind=kind,created=now,until=now+30,position=hero+offset,direction=dir,radius=radius,width=1};
                    if(name=="fx-class-lasting")
                    {
                        cs.effects.AddRange(new[]{E("M08","firewall",new Vector2(0,3.5f),Vector2.right),E("A10","trap",new Vector2(-4,1),Vector2.up,1.5f),
                            E("M14","ward",Vector2.zero,Vector2.up,3),E("M16","vortex",new Vector2(4,2.5f),Vector2.up,3),E("M11","orb",new Vector2(-2.5f,4.5f),Vector2.up,3),
                            E("A17","rain",new Vector2(0,7.5f),Vector2.up,4),E("W17","ancestor",new Vector2(-1.8f,.6f),Vector2.up),E("A11","decoy",new Vector2(2.2f,-1),Vector2.up),
                            E("A15","ballista",new Vector2(-3.5f,-2.5f),Vector2.up),E("A07","arrow",new Vector2(1.2f,0),Vector2.right),E("M10","globe",new Vector2(-1.2f,-1.4f),new Vector2(-1,-.3f).normalized)});
                        run.enemies.Add(Enemy(run,EnemyBase+40,0,new Vector2(-4,5.5f)));frozen=Json(run);Advance(.3f);
                        foreach(var e in cs.effects)if(e.kind=="rain"||e.kind=="orb"||e.kind=="vortex"||e.kind=="ancestor")e.count=1;
                        frozen=Json(run);Advance(.12f);
                        int views=Private<System.Collections.IDictionary>("classEffectViews")?.Count??-1;
                        what=$"class skill lasting effects (firewall, frost snare, rift ward, vortex, orb, killing rain, ancestor, decoy, ballista, arrow, frost globe) with one pulse each; {views} view(s)";
                    }
                    else
                    {
                        int castRoot=9000;ClassSkillCast C(string id,Vector2 aim)=>new ClassSkillCast{id=id,root=castRoot++,released=true,origin=hero,aim=hero+aim,destination=hero+aim};
                        cs.casts.AddRange(new[]{C("W18",Vector2.up),C("M07",Vector2.right*10),C("M12",new Vector2(-.6f,.8f)*12),C("W09",Vector2.down*3)});
                        frozen=Json(run);Advance(.14f);
                        what="class skill releases: Titan's Judgment, Ember Lance (east), Storm Spear (north-west) and Raking Wound (south) 0.14 s after release";
                    }
                    frozen=Json(run);break;
                }
                case "fx-hero-behind-wall":
                {
                    // A wall between the camera and the model hero: the golden silhouette shows on the wall where it hides the
                    // hero, and nowhere on the hero's own overlapping parts (the stencil bit on the hero's materials).
                    var skin=Private<GameObject>("hero").GetComponentInChildren<SkinnedMeshRenderer>();
                    Require(skin!=null&&skin.sharedMaterials.Any(m=>m!=null&&m.shader.name=="HELLSCRIPT/HeroOcclusion"),"The model hero has no occlusion pass.");
                    Require(skin.sharedMaterials.Where(m=>m!=null&&m.HasProperty("_HeroStencil")).All(m=>m.GetFloat("_HeroStencil")==64),"The hero's materials do not mark the stencil.");
                    var toCamera=new Vector2(12,-18).normalized;var wall=Anchor("Gallery wall",At(toCamera*1.7f),PrimitiveType.Cube,new Vector3(3.4f,4.6f,.6f),true);
                    wall.transform.rotation=Quaternion.LookRotation(new Vector3(toCamera.x,0,toCamera.y));
                    Advance(.2f);what="a 4.6 m wall 1.7 m in front of the model hero toward the camera, the hero's silhouette showing through it";break;
                }
                default:
                {
                    fx.Ambience(shot.field);Advance(.6f);var amb=root.Find("Ambience "+fx.AmbienceId);Require(amb!=null,"No ambience for field "+shot.field);
                    what=$"field {shot.field} ambience '{fx.AmbienceId}' with {amb.GetComponentsInChildren<ParticleSystem>().Sum(p=>p.particleCount)} particle(s) around the hero "+
                        "(the rift lighting still follows the theme until the environment workstream applies layout.Field)";break;
                }
            }
            yield return Capture(name);
            claims.Add($"PASS {number:00}-{name}: {what}; {fx.ActiveTransients} live transient(s); RunState unchanged by presentation.");
            Leave();Destroy(propMaterial);
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
            // Field dressing (WorldView.Rift/Props): wall columns, torches, decor and obstacle models in the whole rift.
            var world=GameObject.Find("Rift Runtime");int Named(string n)=>world==null?0:world.GetComponentsInChildren<Transform>(true).Count(t=>t.name==n);
            // Chests, shrines and the offering altar are field content with views of their own, not obstacle props.
            var obstacles=run.layout.obstacles.Where(o=>o.kind!="Chest"&&o.kind!="Shrine"&&o.kind!="OfferingAltar").ToList();
            int props=world==null?0:obstacles.Count(o=>WorldView.ObstacleArt(run.layout.Field,o) is string art&&WorldArt.Has(art));
            return $"{shown} actor root(s) active on the ground, {planned} planned telegraph(s), {drawn} threat view(s); "+
                $"{Named("Wall column")} wall column(s), {Named("Wall torch")} torch(es), {Named("Decor")} decor, {props}/{obstacles.Count} obstacle(s) with a field model";
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

#endif
