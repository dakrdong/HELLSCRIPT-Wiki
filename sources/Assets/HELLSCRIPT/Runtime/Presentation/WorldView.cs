using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Hellscript
{
    public sealed partial class WorldView : MonoBehaviour
    {
        GameController game;
        Camera viewCamera;
        GameObject world,hero;
        readonly Dictionary<int,GameObject> actors=new Dictionary<int,GameObject>();
        readonly Dictionary<int,GameObject> hazards=new Dictionary<int,GameObject>();
        readonly Dictionary<int,GameObject> drops=new Dictionary<int,GameObject>();
        readonly Dictionary<string,Material> materials=new Dictionary<string,Material>();
        readonly List<VisualFx> effects=new List<VisualFx>();
        Material stone,darkStone,trim,ember,blue,red,green,purple;
        float elapsed;
        sealed class VisualFx {public GameObject go;public float life,total,growth=.35f,spin;public Vector3 scale,velocity;}
        public void Initialize(GameController controller)
        {
            game=controller;
            viewCamera=Camera.main;
            if(viewCamera==null){var obj=new GameObject("Hellscript Camera");viewCamera=obj.AddComponent<Camera>();obj.tag="MainCamera";obj.AddComponent<AudioListener>();}
            viewCamera.orthographic=true;viewCamera.orthographicSize=12.5f;viewCamera.nearClipPlane=.1f;viewCamera.farClipPlane=180;
            viewCamera.clearFlags=CameraClearFlags.SolidColor;viewCamera.backgroundColor=new Color(.022f,.033f,.05f);
            viewCamera.transform.rotation=Quaternion.LookRotation(new Vector3(-12,-25,18));
            RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.29f,.34f,.43f);
            RenderSettings.fog=true;RenderSettings.fogColor=new Color(.035f,.05f,.075f);RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogStartDistance=35;RenderSettings.fogEndDistance=95;
            if(FindAnyObjectByType<Light>()==null){var l=new GameObject("Moonlight").AddComponent<Light>();l.type=LightType.Directional;l.intensity=1.3f;l.color=new Color(.68f,.78f,1);l.transform.rotation=Quaternion.Euler(45,-25,0);l.shadows=LightShadows.Soft;}
            stone=Mat("Basalt",new Color(.17f,.21f,.25f));darkStone=Mat("Obsidian",new Color(.07f,.1f,.14f));trim=Mat("Aged Brass",new Color(.51f,.34f,.17f));
            ember=Mat("Amber",new Color(1,.46f,.1f),true);blue=Mat("Frost",new Color(.2f,.7f,1),true);red=Mat("Danger",new Color(.9f,.1f,.14f),true);
            green=Mat("Poison",new Color(.3f,.9f,.38f),true);purple=Mat("Arcane",new Color(.6f,.3f,1),true);
        }
        Material Mat(string name,Color color,bool emission=false)
        {
            if(materials.TryGetValue(name,out var found))return found;
            Shader shader=Shader.Find("Universal Render Pipeline/Lit")??Shader.Find("Standard");var material=new Material(shader){name="HELLSCRIPT "+name,color=color};
            if(material.HasProperty("_BaseColor"))material.SetColor("_BaseColor",color);
            if(emission){material.EnableKeyword("_EMISSION");material.SetColor("_EmissionColor",color*1.8f);}
            if(material.HasProperty("_Smoothness"))material.SetFloat("_Smoothness",.15f);materials.Add(name,material);return material;
        }
        GameObject Shape(string name,PrimitiveType type,Transform parent,Vector3 pos,Vector3 scale,Material mat)
        {
            var go=GameObject.CreatePrimitive(type);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=pos;go.transform.localScale=scale;
            go.GetComponent<Renderer>().sharedMaterial=mat;var collider=go.GetComponent<Collider>();if(collider!=null)Destroy(collider);return go;
        }
        public void ClearDungeon()
        {
            presentedRunId=null;
            shieldView=shadowView=shoutView=null;
            if(world!=null)Destroy(world);world=null;hero=null;actors.Clear();hazards.Clear();drops.Clear();effects.Clear();chestViews.Clear();shrineViews.Clear();projectileViews.Clear();trapViews.Clear();enemyThreatViews.Clear();
            roomGeometry.Clear();passageGeometry.Clear();sealViews.Clear();gateViews.Clear();resourceViews.Clear();ClearObjectiveChains();
        }
        public void BuildDungeon(RunState run)
        {
            ClearDungeon();presentedRunId=run.id;world=new GameObject("Rift Runtime");
            if(!run.layout.legacy){BuildGeneratedGeometry(run);BuildObjectives(run);BuildGates(run);BuildObjectiveChains(run);}
            else
            {
            for(int i=0;i<8;i++)
            {
                Vector2 room=RiftMap.Rooms[i];var floor=Shape("Room "+i,PrimitiveType.Cube,world.transform,new Vector3(room.x,-.45f,room.y),new Vector3(16,.8f,16),stone);
                for(int n=0;n<9;n++)
                {float x=room.x+(n%3-1)*5;float z=room.y+(n/3-1)*5;Shape("Inlaid tile",PrimitiveType.Cube,floor.transform,Vector3.zero,Vector3.one,stone).transform.SetParent(world.transform);var tile=world.transform.GetChild(world.transform.childCount-1);tile.position=new Vector3(x,-.025f,z);tile.localScale=new Vector3(4.8f,.05f,4.8f);}
                for(int edge=0;edge<4;edge++)for(int part=-1;part<=1;part+=2)
                {
                    float a=part*5.4f;Vector3 pos=edge<2?new Vector3(room.x+a,.8f,room.y+(edge==0?-8:8)):new Vector3(room.x+(edge==2?-8:8),.8f,room.y+a);
                    Shape("Broken parapet",PrimitiveType.Cube,world.transform,pos,edge<2?new Vector3(5,1.6f,.7f):new Vector3(.7f,1.6f,5),darkStone);
                }
                for(int corner=0;corner<4;corner++)
                {
                    Vector3 pos=new Vector3(room.x+(corner%2==0?-7:7),0,room.y+(corner<2?-7:7));
                    Shape("Pillar",PrimitiveType.Cylinder,world.transform,pos+Vector3.up*1.8f,new Vector3(1.1f,1.8f,1.1f),darkStone);
                    Shape("Brazier",PrimitiveType.Cylinder,world.transform,pos+Vector3.up*3.6f,new Vector3(1.4f,.15f,1.4f),trim);
                    Shape("Ember",PrimitiveType.Sphere,world.transform,pos+Vector3.up*3.9f,new Vector3(.5f,.65f,.5f),run.theme==0?ember:purple);
                }
                Vector2 next=RiftMap.Rooms[(i+1)%8],mid=(room+next)/2;bool horizontal=room.y==next.y;
                Shape("Connected passage",PrimitiveType.Cube,world.transform,new Vector3(mid.x,-.4f,mid.y),horizontal?new Vector3(7,.7f,6):new Vector3(6,.7f,7),darkStone);
            }
            }
            hero=CreateBody("Hero",(int)game.Store.Data.Hero.heroClass,false,false);
            hero.transform.position=Position(run.position);viewCamera.transform.position=CameraPosition(run.position);
        }
        GameObject CreateBody(string name,int type,bool enemy,bool boss)
        {
            var root=new GameObject(name);root.transform.SetParent(world.transform,false);
            Material body=enemy?Mat("Enemy "+type,Color.Lerp(new Color(.35f,.3f,.34f),type%3==0?new Color(.44f,.2f,.16f):new Color(.2f,.33f,.37f),.6f)):type==0?Mat("Hero Iron",new Color(.37f,.44f,.53f)):type==1?Mat("Hunter Cloak",new Color(.19f,.34f,.28f)):Mat("Mage Cloak",new Color(.28f,.19f,.44f));
            // Rigid one-piece placeholder. The view adds bob, lean, and facing without a skeleton.
            int role=type%6;Vector3 bodyScale=enemy&&role==1?new Vector3(.65f,.65f,1.1f):enemy&&role==5?new Vector3(1.25f,1,1.25f):enemy&&role==4?new Vector3(.55f,1.25f,.55f):new Vector3(.8f,1,.65f);
            Shape("Body",enemy&&role==5?PrimitiveType.Sphere:enemy&&type==6?PrimitiveType.Cube:PrimitiveType.Capsule,root.transform,new Vector3(0,1,0),bodyScale,body);
            Shape("Head",PrimitiveType.Sphere,root.transform,new Vector3(0,2.05f,0),new Vector3(.66f,.65f,.6f),body);
            Shape("Face light",PrimitiveType.Cube,root.transform,new Vector3(0,2.07f,.28f),new Vector3(.4f,.08f,.06f),enemy?red:ember);
            var shoulders=Shape("Shoulders",PrimitiveType.Cube,root.transform,new Vector3(0,1.6f,0),new Vector3(1.2f,.32f,.7f),body);shoulders.transform.localRotation=Quaternion.Euler(0,0,-4);
            var weapon=Shape("Weapon",PrimitiveType.Cube,root.transform,new Vector3(.68f,1.3f,.35f),new Vector3(.17f,1.6f,.25f),enemy?darkStone:trim);weapon.transform.localRotation=Quaternion.Euler(25,0,-25);
            if(!enemy&&type==1){weapon.transform.localScale=new Vector3(.12f,1.3f,.12f);SkillLine("Simple bow",weapon.transform,new[]{new Vector3(0,-.5f,0),new Vector3(0,-.25f,2),new Vector3(0,.25f,2),new Vector3(0,.5f,0),new Vector3(0,-.5f,0)},trim,.55f);}
            if(!enemy&&type==2)Shape("Staff tip",PrimitiveType.Sphere,weapon.transform,new Vector3(0,.55f,0),new Vector3(2,.2f,1.5f),blue);
            if(enemy&&role==2)weapon.transform.localScale=new Vector3(1.2f,.15f,.4f);
            if(enemy&&role==4)Shape("Support lantern",PrimitiveType.Sphere,root.transform,new Vector3(.68f,2.3f,.35f),Vector3.one*.4f,purple);
            if(!enemy)Ring(root.transform,Vector3.up*.08f,1.05f,ember,.055f);
            if(boss)root.transform.localScale=Vector3.one*2;
            return root;
        }
        GameObject Ring(Transform parent,Vector3 pos,float radius,Material mat,float width)
        {
            var go=new GameObject("Runic ring");go.transform.SetParent(parent,false);go.transform.localPosition=pos;
            var line=go.AddComponent<LineRenderer>();line.sharedMaterial=mat;line.useWorldSpace=false;line.positionCount=49;line.loop=false;line.widthMultiplier=width;
            for(int i=0;i<49;i++){float a=i*Mathf.PI/24;line.SetPosition(i,new Vector3(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius));}return go;
        }
        static Vector3 Position(Vector2 p) => new Vector3(p.x,0,p.y);
        Vector3 CameraPosition(Vector2 p) => Position(p)+new Vector3(12,25,-18);
        public void Present(RunState run,float dt)
        {
            if(presentationSuspended||world==null)return;
            using var sample=PresentationMetrics.World.Auto();
            PresentationMetrics.WorldCalls++;
            if(world==null)return;bool frozen=run.paused||run.portal||!string.IsNullOrEmpty(run.navigationError);elapsed+=frozen?0:dt*game.EffectiveSpeed;
            PresentExploredGeometry(run);
            Vector3 hp=Position(run.position)+Vector3.up*game.Combat.HeroAirHeight;Vector3 movement=hp-hero.transform.position;movement.y=0;
            hero.transform.position=snapPresentation?hp:Vector3.Lerp(hero.transform.position,hp,Mathf.Min(1,dt*22));
            var currentFacing=CurrentHeroFacing(run);
            if(snapPresentation&&currentFacing.sqrMagnitude>.005f)hero.transform.rotation=Quaternion.LookRotation(currentFacing);
            else if(movement.sqrMagnitude>.005f)hero.transform.rotation=Quaternion.Slerp(hero.transform.rotation,Quaternion.LookRotation(movement),dt*14);
            var body=hero.transform.GetChild(0);body.localPosition=new Vector3(0,1+(movement.sqrMagnitude>.001f?Mathf.Sin(elapsed*13)*.08f:0),0);
            viewCamera.transform.position=snapPresentation?CameraPosition(run.position):Vector3.Lerp(viewCamera.transform.position,CameraPosition(run.position),dt*5);
            ApplyBattleViewport();
            foreach(var enemy in run.enemies)
            {
                if(enemy.dead){if(actors.TryGetValue(enemy.id,out var dead)){Destroy(dead);actors.Remove(enemy.id);}continue;}
                if(Vector2.Distance(enemy.position,run.position)>(run.layout.legacy?22:12)||!run.layout.legacy&&!game.Combat.Map.LineClear(run.position,enemy.position))
                {if(actors.TryGetValue(enemy.id,out var hidden))hidden.SetActive(false);continue;}
                if(!actors.TryGetValue(enemy.id,out var actor))
                {actor=CreateBody(!string.IsNullOrEmpty(enemy.eventId)?"균열 잔향":enemy.boss?GameCatalog.BossNames[enemy.pattern]:GameCatalog.EnemyNames[enemy.kind],enemy.boss?new[]{6,10,7}[enemy.pattern]:enemy.kind,true,enemy.boss);actors[enemy.id]=actor;actor.transform.position=Position(enemy.position);
                 if(enemy.elite>=0)Ring(actor.transform,Vector3.up*.1f,1.1f,purple,.1f);
                 if(!string.IsNullOrEmpty(enemy.eventId))Ring(actor.transform,Vector3.up*.15f,.9f,red,.1f);
                 Shape("Health",PrimitiveType.Cube,actor.transform,new Vector3(0,2.65f,0),new Vector3(1,.1f,.1f),red);}
                if(enemy.boss&&actor.transform.Find("Stagger")==null){var ring=Ring(actor.transform,Vector3.up*.12f,1.1f,blue,.09f);ring.name="Stagger";}
                if(enemy.boss)actor.transform.Find("Stagger").gameObject.SetActive(enemy.bossControl.staggered>0);
                actor.SetActive(true);actor.transform.position=snapPresentation?Position(enemy.position):Vector3.Lerp(actor.transform.position,Position(enemy.position),Mathf.Min(1,dt*20));
                Vector3 facing=Position(enemy.brain.facing);facing.y=0;if(facing.sqrMagnitude>.01f)actor.transform.rotation=Quaternion.LookRotation(facing);
                actor.transform.GetChild(0).localRotation=Quaternion.Euler(enemy.brain.action.phase==EnemyActionPhase.Charging?25:enemy.brain.action.phase==EnemyActionPhase.Preparing?-12:0,0,0);
                var bar=actor.transform.Find("Health");bar.localScale=new Vector3(Mathf.Max(.01f,enemy.health/enemy.maxHealth),.1f,.1f);
            }
            foreach(var drop in run.drops)
            {
                if(drop.claimed||drop.ignored){if(drops.TryGetValue(drop.id,out var old)){Destroy(old);drops.Remove(drop.id);}continue;}
                if(!run.layout.legacy&&!drop.discovered)continue;
                if(!drops.TryGetValue(drop.id,out var visual))
                {var color=drop.item.rarity==3?ember:drop.item.rarity==2?purple:blue;visual=Shape(drop.item.name,PrimitiveType.Cube,world.transform,Position(drop.position)+Vector3.up*.4f,new Vector3(.35f,.65f,.2f),color);drops.Add(drop.id,visual);Ring(visual.transform,Vector3.zero,1,color,.04f);}
                visual.transform.rotation=Quaternion.Euler(15,elapsed*55,25);
            }
            PresentResources(run);PresentChests(run,dt);PresentObjectives(run);PresentGates(run);PresentObjectiveChains(run);PresentActions(run);PresentEnemyCombat(run);PresentSkillStates(run);
            var active=new HashSet<int>();
            foreach(var fx in run.effects)
            {
                active.Add(fx.id);
                if(fx.hostile&&!CanDisplayEnemyMarker(run,fx.position,fx.radius))
                {if(hazards.TryGetValue(fx.id,out var hidden))hidden.SetActive(false);continue;}
                if(!hazards.TryGetValue(fx.id,out var v)){v=Ring(world.transform,Position(fx.position)+Vector3.up*.09f,fx.radius,fx.hostile?red:fx.kind==8?green:blue,.13f);hazards[fx.id]=v;DecorateGround(v,fx);}
                v.SetActive(true);
                v.transform.position=Position(fx.position)+Vector3.up*.1f;v.transform.localScale=Vector3.one*(fx.delay>0?.85f+Mathf.Sin(elapsed*8)*.08f:1);
                var snow=v.transform.Find("Snow swirl");if(snow!=null)snow.localRotation=Quaternion.Euler(0,elapsed*80,0);
            }
            foreach(var id in new List<int>(hazards.Keys))if(!active.Contains(id)){Destroy(hazards[id]);hazards.Remove(id);}
            for(int i=effects.Count-1;i>=0;i--)
            {var fx=effects[i];float step=frozen?0:dt*game.EffectiveSpeed;fx.life-=step;if(fx.life<=0){Destroy(fx.go);effects.RemoveAt(i);}else{fx.go.transform.localScale=fx.scale*(1+(1-fx.life/fx.total)*fx.growth);fx.go.transform.position+=fx.velocity*step;fx.go.transform.Rotate(0,fx.spin*step,0);}}
        }
        public void Effect(Vector2 origin,Vector2 target,int kind,float amount)
        {
            if(presentationSuspended||world==null)return;
            using var sample=PresentationMetrics.Effect.Auto();
            PresentationMetrics.EffectCalls++;
            if(world==null)return;
            var run=game.Combat.State;
            if(kind==30||kind==31||kind==32){if(!CanDisplayEnemyMarker(run,origin))return;}
            else if(kind==6||kind==14||kind==18||kind==25)
            {if(!CanDisplayEnemyMarker(run,origin)||!CanDisplayEnemyMarker(run,target))return;}
            else if(!CanDisplayEnemyMarker(run,kind==0||kind==3||kind==4||kind==5||kind==11||kind==16||kind==17||kind==20?origin:target,kind==24?2:Mathf.Clamp(amount,.8f,4)))return;
            if(kind==30||kind==31||kind==32){if(kind==30&&amount<8)return;var spark=Shape("Hit",PrimitiveType.Sphere,world.transform,Position(origin)+Vector3.up*1.5f,Vector3.one*(kind==31?.35f:.15f),kind==32?red:ember);effects.Add(new VisualFx{go=spark,life=.15f,total=.15f,scale=spark.transform.localScale});return;}
            if(SkillEffect(origin,target,kind,amount))return;
            Material mat=kind>=12&&kind<=17?blue:kind==8?green:kind==24||kind==25?red:ember;
            GameObject go;
            if(kind==6||kind==14||kind==18||kind==25)
            {go=new GameObject("Attack trail");go.transform.SetParent(world.transform);var line=go.AddComponent<LineRenderer>();line.sharedMaterial=mat;line.positionCount=2;line.SetPosition(0,Position(origin)+Vector3.up);line.SetPosition(1,Position(target)+Vector3.up);line.widthMultiplier=kind==14?.12f:.055f;}
            else go=Ring(world.transform,Position(kind==0||kind==4||kind==16||kind==20?origin:target)+Vector3.up*.15f,kind==24?2:Mathf.Clamp(amount,.8f,4),mat,kind==0?.16f:.09f);
            float life=kind==24?amount:kind==0?.22f:.45f;effects.Add(new VisualFx{go=go,life=life,total=life,scale=go.transform.localScale});
        }
        void LateUpdate()=>ApplyBattleViewport();
        void ApplyBattleViewport()
        {
            if(viewCamera==null||presentationSuspended)return;
            var viewport=game.UI!=null&&game.UI.Page=="battle"?game.UI.BattleViewport:new Rect(0,0,1,1);
            viewCamera.rect=viewport;viewCamera.ResetAspect();
            viewCamera.orthographicSize=BattleHudLayout.CameraHalfHeight(viewCamera.aspect,game.UI!=null?game.UI.BattleViewHeight:Screen.height);
        }
        void OnDestroy(){foreach(var mat in materials.Values)if(mat!=null)Destroy(mat);}
    }
}
