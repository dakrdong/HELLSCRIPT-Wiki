using System.Collections.Generic;
using UnityEngine;

namespace Hellscript
{
    // World effects for the class skills that emit no Visual kind (W07-W18, A07-A18, M07-M18). A released cast plays its
    // skill's burst once (classSkills.casts), and the lasting effects in classSkills.effects keep a view while they exist:
    // arrows and globes fly, traps, wards and vortices mark the ground, the firewall burns, rain and the triune collapse
    // pulse on each tick, the orb and the ballista zap, ancestors and the decoy stand as spectres. Bursts are WorldFx
    // (pooled, fog-aware); the few lasting shapes are owned by the world root and dropped with their effect. Sizes come from
    // the skill's own numbers. Presentation only: reads the run, never writes it.
    public sealed partial class WorldView
    {
        readonly HashSet<int> playedClassCasts=new HashSet<int>();
        readonly List<int> finishedClassCasts=new List<int>();
        sealed class ClassEffectView{public GameObject go;public int count;public bool placed;}
        readonly Dictionary<string,ClassEffectView> classEffectViews=new Dictionary<string,ClassEffectView>();
        readonly List<string> endedClassEffects=new List<string>();
        static readonly Color PhysicalTint=new Color(.92f,.82f,.66f,1),FireTint=new Color(1,.52f,.18f,1),ColdTint=new Color(.62f,.86f,1,1),
            LightningTint=new Color(.66f,.72f,1,1),PoisonTint=new Color(.55f,1,.35f,1),ShadowTint=new Color(.7f,.45f,1,1),BloodTint=new Color(.85f,.14f,.1f,1),
            SpiritTint=new Color(.6f,.8f,1,1);
        static Color ElementTint(int element)=>element==1?FireTint:element==2?ColdTint:element==3?LightningTint:element==4?PoisonTint:element==5?ShadowTint:PhysicalTint;
        static float SkillNumber(string id,string key,float fallback){var d=ClassSkills.Find(id);return d!=null?d.Number(key,fallback):fallback;}
        static string ClassEffectKind(ClassSkillEffect effect)=>effect.kind switch
        {"d4arrow"=>"arrow","d4fireball"=>"globe","d4split"=>"globe","d4frost"=>"returnGlobe","d4conduit"=>"conduit","d4afterimage"=>"afterimage","d4reaction"=>"reactionShield","d4firewall"=>"firewall","d4orb"=>"orb","d4ward"=>"ward","d4rain"=>"rain","d4collapse"=>"collapse","d4ancestor"=>"ancestor","d4decoy"=>"decoy","clone"=>"decoy","turret"=>"ballista","d4trail"=>"trail","d4meteor"=>"meteor","d4nova"=>"nova","d4burst"=>"burst",_=>effect.kind};

        // A restored run keeps its released casts silent; only casts released from now on play.
        void SeedClassSkillFx(RunState run){playedClassCasts.Clear();if(run.classSkills!=null)foreach(var c in run.classSkills.casts)if(c.released)playedClassCasts.Add(c.root);}
        void ClearClassSkillFx(){ClearHookViews();ClearRakingWoundArea();playedClassCasts.Clear();foreach(var v in classEffectViews.Values)if(v.go!=null)Destroy(v.go);classEffectViews.Clear();}

        void PresentClassSkillFx(RunState run)
        {
            var cs=run.classSkills;if(cs==null||presentationSuspended||world==null)return;var fx=Fx;if(fx==null)return;
            foreach(var c in cs.casts)if(c.released&&playedClassCasts.Add(c.root))PlayClassRelease(run,fx,c);
            PresentHookViews(run);PresentRakingWoundArea(run,fx);
            finishedClassCasts.Clear();
            foreach(int root in playedClassCasts){bool alive=false;foreach(var c in cs.casts)if(c.root==root){alive=true;break;}if(!alive)finishedClassCasts.Add(root);}
            foreach(int root in finishedClassCasts)playedClassCasts.Remove(root);
            foreach(var e in cs.effects)PresentClassEffect(run,fx,e);
            endedClassEffects.Clear();
            foreach(var pair in classEffectViews){bool alive=false;foreach(var e in cs.effects)if(e.id==pair.Key){alive=true;break;}if(!alive)endedClassEffects.Add(pair.Key);}
            foreach(var id in endedClassEffects){var v=classEffectViews[id];if(v.go!=null)Destroy(v.go);classEffectViews.Remove(id);}
        }

        void PlayClassRelease(RunState run,WorldFx fx,ClassSkillCast c)
        {
            Vector2 o=c.origin,aim=c.aim,dir=(aim-o).sqrMagnitude>.0001f?(aim-o).normalized:Vector2.up;
            Vector3 O=Position(o),A=Position(aim),D=new Vector3(dir.x,0,dir.y),up=Vector3.up;
            EnemyState target=null;if(c.target>=0)foreach(var e in run.enemies)if(e.id==c.target){target=e;break;}
            Vector3 T=target!=null?Position(target.position):A;
            float radius=SkillNumber(c.id,"radius",3);
            switch(c.id)
            {
                case "W08":
                    var hookAt=hookViews.TryGetValue(c.root,out var hook)?Position(hook.before):A;
                    if(c.mod==1)fx.Beam(O+up*1.2f,T+up*1.1f,PhysicalTint,.12f,.3f);
                    fx.Flash(O+up*1.3f,.65f,PhysicalTint);fx.Burst("impact",hookAt+up*1.1f,.9f,PhysicalTint);fx.Burst("sparks",hookAt+up*1.1f,.65f,PhysicalTint);break;
                case "W09":
                    if(c.mod==2){fx.Burst("blood",T+up,.8f);break;}
                    var rendArea=Combat.RakingWoundArea(c);
                    fx.Slash(O+up*.9f,D,rendArea.radius,rendArea.angle,BloodTint,.3f);
                    fx.Burst("blood",O+D*Mathf.Min(rendArea.radius,1.8f)+up,.8f);break;
                case "W17":fx.Burst("arcane",O+up*1.2f,1.5f,SpiritTint);fx.Shockwave(O+up*.1f,2.5f,SpiritTint,.5f);break;
                case "W18":
                    fx.Shockwave(O+up*.1f,Mathf.Max(3,radius),new Color(1,.8f,.5f,1),.6f);fx.Burst("explosion",O+up*1.1f,1.3f,new Color(1,.85f,.6f,1));
                    fx.Burst("dust",O,2.2f);fx.Decal("crack",O,Mathf.Max(3,radius)*.8f);lighting.Shake(.6f,.35f);break;
                case "A18":fx.Burst("shadow",O+up,1.2f);break;
                case "M07":{float range=run.build.classSkills.version==5&&c.mod==3?12:SkillNumber(c.id,"range",10);var tint=run.build.classSkills.version==5?ElementTint(c.element):FireTint;fx.Beam(O+up*1.3f,O+D*range+up*1.1f,tint,.35f,.3f);fx.Burst(c.element==2?"frost":c.element==3?"lightning":"fire",O+D*range+up,1.1f);break;}
                case "M18":fx.Flash(O+up*1.2f,2,new Color(.85f,.75f,1,1));fx.Burst("arcane",O+up*1.2f,1.5f);break;
                // Projectiles and placed current effects render from their own state.
            }
        }

        void PresentClassEffect(RunState run,WorldFx fx,ClassSkillEffect e)
        {
            string kind=ClassEffectKind(e);switch(kind)
            {
                case "arrow":case "globe":case "returnGlobe":case "conduit":case "afterimage":case "reactionShield":case "trap":case "firewall":case "orb":case "vortex":case "ward":case "rain":case "collapse":case "ancestor":case "decoy":case "ballista":case "trail":case "meteor":case "nova":case "burst":break;
                default:return;
            }
            // Arrows fired with a delay exist before they fly.
            if(e.created>run.time+.0001f)return;
            if(!classEffectViews.TryGetValue(e.id,out var v)){v=new ClassEffectView{count=e.kind=="reactionShield"?0:e.count};v.go=BuildClassEffectView(fx,e);classEffectViews[e.id]=v;}
            var at=Position(e.position);
            if(v.go!=null)
            {
                if(kind=="arrow"||kind=="globe"||kind=="returnGlobe")
                {
                    var flying=at+Vector3.up*(kind=="arrow"?1.25f:1.2f);
                    v.go.transform.position=!v.placed||snapPresentation?flying:Vector3.Lerp(v.go.transform.position,flying,Mathf.Min(1,motionStep*30));
                    if(e.direction.sqrMagnitude>.0001f)v.go.transform.rotation=Quaternion.LookRotation(new Vector3(e.direction.x,0,e.direction.y));
                }
                // Spectres walk with their effect; grounded marks stay where they were laid.
                else if(kind=="ancestor"||kind=="decoy")v.go.transform.position=at+Vector3.up;
                else if(e.kind.StartsWith("d4",System.StringComparison.Ordinal)||e.kind=="turret")v.go.transform.position=at+Vector3.up*(kind=="orb"?1.4f:kind=="ward"?.06f:kind=="rain"||kind=="collapse"?.05f:0);
                v.placed=true;
            }
            // One pulse per simulated tick the view has not shown yet.
            for(;v.count<e.count;v.count++)ClassEffectTick(run,fx,e,v.count+1);
        }
        GameObject BuildClassEffectView(WorldFx fx,ClassSkillEffect e)
        {
            var at=Position(e.position);var up=Vector3.up;float radius=e.radius>0?e.radius:SkillNumber(e.source,"radius",3);
            switch(ClassEffectKind(e))
            {
                case "arrow":
                {
                    var tint=e.element==4?green:e.element==5?purple:trim;
                    return SkillLine("Class arrow",world.transform,new[]{Vector3.back*.9f,Vector3.zero},tint,.07f);
                }
                case "globe":case "returnGlobe":
                {
                    bool fire=e.kind=="d4fireball"||e.kind=="d4split";var globe=Shape(fire?"Fireball":"Frost globe",PrimitiveType.Sphere,world.transform,at+up*1.2f,Vector3.one*.55f,fire?ember:blue);
                    fx.Burst(fire?"fire":"frost",at+up*1.2f,.6f);return globe;
                }
                case "conduit":
                {
                    var spear=Shape("Grounding spear",PrimitiveType.Cylinder,world.transform,at+up,new Vector3(.14f,1,.14f),blue);
                    fx.Decal("rune",at,1.2f,LightningTint,Mathf.Max(.1f,e.until-Combat.State.time));return spear;
                }
                case "afterimage":
                {
                    var image=Shape("Archer afterimage",PrimitiveType.Capsule,world.transform,at+up,new Vector3(.55f,.9f,.55f),purple);
                    fx.Burst("shadow",at+up,.65f);return image;
                }
                case "trap":
                {
                    var trap=Ring(world.transform,at+up*.05f,Mathf.Max(.8f,radius),e.source=="A03"&&e.mod==2?blue:green,.07f);trap.name="Class trap";
                    fx.Decal(e.source=="A03"&&e.mod==2?"ice":"rune",at,Mathf.Max(.8f,radius),e.source=="A03"&&e.mod==2?ColdTint:PoisonTint,Mathf.Max(.5f,e.until-Combat.State.time));
                    return trap;
                }
                case "firewall":
                {
                    var root=new GameObject("Firewall");root.transform.SetParent(world.transform,false);root.transform.position=at;
                    var along=new Vector3(e.direction.x,0,e.direction.y);if(along.sqrMagnitude<.0001f)along=Vector3.forward;
                    float length=e.kind=="d4firewall"?e.radius*2:SkillNumber("M08","length",5);int flames=e.kind=="d4firewall"&&e.mod==1?16:Mathf.Max(3,Mathf.RoundToInt(length));
                    for(int i=0;i<flames;i++)
                    {
                        var anchor=new GameObject("Flame "+i);anchor.transform.SetParent(root.transform,false);
                        anchor.transform.position=e.kind=="d4firewall"&&e.mod==1?at+new Vector3(Mathf.Cos(i*6.2832f/flames),0,Mathf.Sin(i*6.2832f/flames))*e.radius:at+along*Mathf.Lerp(-length*.5f,length*.5f,(i+.5f)/flames);fx.Fire(anchor.transform,.8f,FireTint);
                        if(e.kind=="d4firewall"&&e.mod==3){var crossing=new GameObject("Cross flame "+i);crossing.transform.SetParent(root.transform,false);crossing.transform.position=at+Vector3.Cross(along,up)*Mathf.Lerp(-length*.5f,length*.5f,(i+.5f)/flames);fx.Fire(crossing.transform,.8f,FireTint);}
                    }
                    fx.Decal("scorch",at,length*.5f,default,Mathf.Max(.5f,e.until-Combat.State.time));return root;
                }
                case "orb":
                {
                    var orb=Shape("Capacitor orb",PrimitiveType.Sphere,world.transform,at+up*1.4f,Vector3.one*.5f,blue);
                    fx.Burst("lightning",at+up*1.4f,.7f);return orb;
                }
                case "vortex":
                {
                    fx.Decal("rune",at,Mathf.Max(2,radius),LightningTint,Mathf.Max(.5f,e.until-Combat.State.time));
                    var ring=Ring(world.transform,at+up*.08f,Mathf.Max(2,radius),blue,.08f);ring.name="Magnetic vortex";return ring;
                }
                case "ward":
                {
                    fx.Decal("glow",at,Mathf.Max(2,radius),SpiritTint,Mathf.Max(.5f,e.until-Combat.State.time));
                    var ring=Ring(world.transform,at+up*.06f,Mathf.Max(2,radius),purple,.08f);ring.name="Rift ward";return ring;
                }
                case "rain":case "collapse":
                {
                    var ring=Ring(world.transform,at+up*.05f,Mathf.Max(e.kind.StartsWith("d4",System.StringComparison.Ordinal)?0:3,radius),ClassEffectKind(e)=="rain"?trim:ember,.07f);ring.name=ClassEffectKind(e)=="rain"?"Killing rain":"Triune collapse";return ring;
                }
                case "ancestor":case "decoy":
                {
                    var spectre=Shape(ClassEffectKind(e)=="ancestor"?"Ancestor spirit":"Decoy",PrimitiveType.Capsule,world.transform,at+up,new Vector3(.8f,1,.8f),ClassEffectKind(e)=="ancestor"?blue:purple);
                    fx.Burst("arcane",at+up,1,SpiritTint);return spectre;
                }
                case "ballista":
                {
                    var root=new GameObject("Watch ballista");root.transform.SetParent(world.transform,false);root.transform.position=at;
                    Shape("Ballista frame",PrimitiveType.Cube,root.transform,up*.5f,new Vector3(.7f,.5f,1.1f),darkStone);
                    Shape("Ballista bow",PrimitiveType.Cube,root.transform,up*.9f+Vector3.forward*.3f,new Vector3(1.4f,.08f,.12f),trim);
                    fx.Burst("dust",at,.8f);return root;
                }
                case "meteor":fx.Decal("rune",at,Mathf.Max(1,radius),FireTint,Mathf.Max(.1f,e.until-Combat.State.time));return null;
                case "nova":case "burst":return null;
                case "trail":fx.Beam(Position(e.origin)+up*.08f,at+up*.08f,FireTint,.45f,Mathf.Max(.1f,e.until-Combat.State.time));return null;
            }
            return null;
        }
        // One simulated tick of a pulsing effect: rain strikes, the triune collapse's fire, cold then lightning wave, the orb's
        // and the vortex's zaps, an ancestor's swing.
        void ClassEffectTick(RunState run,WorldFx fx,ClassSkillEffect e,int tick)
        {
            var at=Position(e.position);var up=Vector3.up;float radius=e.radius>0?e.radius:SkillNumber(e.source,"radius",3);
            if(!CanDisplayEnemyMarker(run,e.position,radius))return;
            switch(ClassEffectKind(e))
            {
                case "meteor":case "nova":case "burst":fx.Shockwave(at+up*.1f,Mathf.Max(1,radius),ElementTint(e.element),.4f);fx.Burst(e.element==2?"frost":"explosion",at+up,1.2f);break;
                case "reactionShield":
                    fx.Burst(e.element==1?"fire":e.element==2?"frost":"lightning",at+up,1.2f,ElementTint(e.element));
                    fx.Shockwave(at+up*.1f,3,ElementTint(e.element),.4f);break;
                case "rain":
                    for(int i=0;i<5;i++)
                    {
                        float a=(tick*2.39996f+i*1.2566f)%6.2832f,r=radius*Mathf.Sqrt(((tick*7+i*3)%10+.5f)/10f);
                        var p=at+new Vector3(Mathf.Cos(a)*r,0,Mathf.Sin(a)*r);fx.Burst("impact",p+up*.2f,.55f);if(i==0)fx.Burst("dust",p,.7f);
                    }
                    break;
                case "collapse":
                {
                    int element=e.kind=="d4collapse"&&e.mod==1?e.element:tick;var tint=ElementTint(element);fx.Shockwave(at+up*.1f,Mathf.Max(3,radius),tint,.45f);
                    fx.Burst(element==1?"fire":element==2?"frost":"lightning",at+up,1.6f);if(element==2)fx.Decal("ice",at,Mathf.Max(3,radius)*.8f);break;
                }
                case "orb":case "vortex":
                {
                    EnemyState near=null;float best=float.MaxValue;
                    foreach(var enemy in run.enemies){if(enemy.dead)continue;float d=Vector2.Distance(enemy.position,e.position);if(d<=Mathf.Max(3,radius)&&d<best){best=d;near=enemy;}}
                    if(near!=null)fx.Beam(at+up*(e.kind=="orb"?1.4f:.6f),Position(near.position)+up*1.1f,LightningTint,.12f,.16f);
                    else if(e.kind=="vortex")fx.Burst("lightning",at+up*.6f,.8f);
                    break;
                }
                case "ancestor":fx.Slash(at+up*1.1f,Vector3.forward,1.8f,140,SpiritTint,.22f);break;
            }
        }
    }
}
