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
        static readonly Color PhysicalTint=new Color(1,.5f,0,1),FireTint=new Color(1,.22f,0,1),ColdTint=new Color(0,.58f,1,1),
            LightningTint=new Color(.38f,.3f,1,1),PoisonTint=new Color(.28f,.86f,0,1),ShadowTint=new Color(.55f,.1f,.95f,1),BloodTint=new Color(.92f,.04f,.1f,1),
            SpiritTint=new Color(0,.72f,.9f,1);
        static Color ElementTint(int element)=>element==1?FireTint:element==2?ColdTint:element==3?LightningTint:element==4?PoisonTint:element==5?ShadowTint:PhysicalTint;
        static float SkillNumber(string id,string key,float fallback){var d=ClassSkills.Find(id);return d!=null?d.Number(key,fallback):fallback;}
        static string ClassEffectKind(ClassSkillEffect effect)=>effect.kind switch
        {"d4arrow"=>"arrow","d4fireball"=>"globe","d4split"=>"globe","d4frost"=>"returnGlobe","d4conduit"=>"conduit","d4afterimage"=>"afterimage","d4reaction"=>"reactionShield","d4firewall"=>"firewall","d4orb"=>"orb","d4ward"=>"ward","d4rain"=>"rain","d4collapse"=>"collapse","d4ancestor"=>"ancestor","d4decoy"=>"decoy","clone"=>"decoy","turret"=>"ballista","d4trail"=>"trail","d4meteor"=>"meteor","d4nova"=>"nova","d4burst"=>"burst",_=>effect.kind};

        // A restored run keeps its released casts silent; only casts released from now on play.
        void SeedClassSkillFx(RunState run){playedClassCasts.Clear();SeedSkillJuice(run);if(run.classSkills!=null)foreach(var c in run.classSkills.casts)if(c.released)playedClassCasts.Add(c.root);}
        void ClearClassSkillFx(){ClearSkillJuice();ClearHookViews();ClearRakingWoundArea();playedClassCasts.Clear();foreach(var v in classEffectViews.Values)if(v.go!=null)Destroy(v.go);classEffectViews.Clear();}

        void PresentClassSkillFx(RunState run)
        {
            var cs=run.classSkills;if(cs==null||presentationSuspended||world==null)return;var fx=Fx;if(fx==null)return;
            TickJuice(run,fx,motionStep);EffectJuice(run,fx);
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
                    if(c.mod==1)fx.Beam(O+up*1.2f,T+up*1.1f,PhysicalTint,.12f,.3f,true);
                    fx.Burst("flare",O+up*1.3f,1.3f,PhysicalTint);fx.Burst("impact",hookAt+up*1.1f,1.1f,PhysicalTint);fx.Burst("strike",hookAt+up*1.1f,1.8f,PhysicalTint);break;
                case "W09":
                    if(c.mod==2){fx.Slash(O+up*.9f,D,2.4f,40,BloodTint,.28f,true);fx.Burst("blood",T+up,1.1f);fx.Burst("lines",T+up*1.2f,1.4f,BloodTint);break;}
                    var rendArea=Combat.RakingWoundArea(c);
                    fx.Slash(O+up*.9f,D,rendArea.radius,rendArea.angle,BloodTint,.3f,true);
                    // Two follow-up rakes, the same arc turned either way, read as claw marks.
                    var rakeLeft=Quaternion.Euler(0,-14,0)*D;var rakeRight=Quaternion.Euler(0,14,0)*D;
                    After(.05f,f=>f.Slash(O+up*1f,rakeLeft,rendArea.radius*.95f,rendArea.angle*.9f,new Color(1,.35f,.25f,1),.24f,true));
                    After(.1f,f=>f.Slash(O+up*.8f,rakeRight,rendArea.radius*.9f,rendArea.angle*.85f,BloodTint,.22f,true));
                    fx.Burst("blood",O+D*Mathf.Min(rendArea.radius,1.8f)+up,1.1f);break;
                case "W17":fx.Burst("flare",O+up*1.3f,2.6f,SpiritTint);fx.Burst("lines",O+up*1.3f,2.6f,SpiritTint);break;
                case "W18":
                    // The shake comes from the style table (JuiceRelease); three rings roll out and dust jets up around the rim.
                    float titan=Mathf.Max(3.5f,radius);
                    fx.Burst("ring",O+up*.1f,titan,GoldTint);After(.12f,f=>f.Burst("ring",O+up*.1f,titan*1.4f,WarCryTint));After(.26f,f=>f.Burst("ring",O+up*.1f,titan*1.8f,GoldTint));
                    fx.Burst("explosion",O+up*1.1f,1.5f,new Color(1,.85f,.6f,1));fx.Burst("lines.ground",O+up*.08f,titan*1.2f,GoldTint);fx.Burst("star",O+up*1.4f,2.6f,GoldTint);fx.Flash(O+up*1.5f,5,Color.white);
                    fx.Burst("dust",O,2.2f);fx.Decal("crack",O,titan*.9f);
                    for(int jet=0;jet<8;jet++){float rim=jet*Mathf.PI/4;var at=O+new Vector3(Mathf.Cos(rim),0,Mathf.Sin(rim))*titan*.85f;After(.04f*jet,f=>f.Burst("dust",at,1.8f));}
                    break;
                case "A18":fx.Burst("shadow",O+up,1.2f);break;
                case "M07":{float range=run.build.classSkills.version==5&&c.mod==3?12:SkillNumber(c.id,"range",10);var tint=run.build.classSkills.version==5?ElementTint(c.element):FireTint;fx.Beam(O+up*1.3f,O+D*range+up*1.1f,tint,2f,.36f,true);fx.Beam(O+up*1.3f,O+D*range+up*1.1f,Color.white,.6f,.3f);fx.Burst(c.element==2?"frost":c.element==3?"lightning":"fire",O+D*range+up,1.1f);
                    fx.Streak(O+D*1.2f+up*1.2f,O+D*range+up*1.15f,tint,2.4f,.34f,solid:true);fx.Burst("strike",O+D*range+up,1.9f,tint);
                    for(int mid=1;mid<4;mid++)fx.Burst("pop",O+D*(range*mid/4f)+up*1.2f,.5f,tint);break;}
                case "M18":fx.Flash(O+up*1.2f,2,new Color(.85f,.75f,1,1));fx.Burst("arcane",O+up*1.2f,1.5f);break;
                // Projectiles and placed current effects render from their own state.
            }
            JuiceRelease(run,fx,c,O,D,T,radius);
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
                // A meteor drops out of the sky over its delay and lands as the simulation's blast goes off.
                else if(kind=="meteor"){float fall=e.until>e.created+.01f?Mathf.Clamp01((run.time-e.created)/(e.until-e.created)):1;v.go.transform.position=at+Vector3.Lerp(new Vector3(-5,18,-4),Vector3.up*.5f,fall*fall);}
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
                    var tint=e.element==4?green:e.element==5?purple:Mat("Ranger arrow",RangerTint,true);
                    var arrow=SkillLine("Class arrow",world.transform,new[]{Vector3.back*1.4f,Vector3.zero},tint,.15f);
                    // A glowing head and a trail of light; a split arrow announces itself with a star where it was born.
                    fx.Aura(arrow.transform,e.element==4?4:e.element==5?5:0,1.3f,e.element==4||e.element==5?default:RangerTint);
                    if(e.generation>0)fx.Burst("star",at+up*1.25f,.8f,e.element==4?PoisonTint:e.element==5?ShadowTint:RangerTint);
                    return arrow;
                }
                case "globe":case "returnGlobe":
                {
                    bool fire=e.kind=="d4fireball"||e.kind=="d4split";var globe=Shape(fire?"Fireball":"Frost globe",PrimitiveType.Sphere,world.transform,at+up*1.2f,Vector3.one*(fire?.85f:.8f),fire?ember:blue);
                    fx.Aura(globe.transform,fire?1:2,fire?1.7f:1.8f);fx.Burst(fire?"fire":"frost",at+up*1.2f,.9f);fx.Burst("flare",at+up*1.2f,1.7f,fire?FireTint:ColdTint);return globe;
                }
                case "conduit":
                {
                    var spear=Shape("Grounding spear",PrimitiveType.Cylinder,world.transform,at+up,new Vector3(.14f,1,.14f),blue);
                    fx.Aura(spear.transform,3,1.7f);fx.Burst("flare",at+up*1.2f,1.7f,LightningTint);
                    fx.Decal("rune",at,1.2f,LightningTint,Mathf.Max(.1f,e.until-Combat.State.time));return spear;
                }
                case "afterimage":
                {
                    var image=Shape("Archer afterimage",PrimitiveType.Capsule,world.transform,at+up,new Vector3(.55f,.9f,.55f),purple);
                    fx.Aura(image.transform,5,.9f);fx.Burst("shadow",at+up,.65f);return image;
                }
                case "trap":
                {
                    var trap=Ring(world.transform,at+up*.05f,Mathf.Max(.8f,radius),e.source=="A03"&&e.mod==2?blue:green,.07f);trap.name="Class trap";
                    bool cold=e.source=="A03"&&e.mod==2;
                    fx.Decal(cold?"ice":"rune",at,Mathf.Max(.8f,radius),cold?ColdTint:PoisonTint,Mathf.Max(.5f,e.until-Combat.State.time));
                    fx.Zone(trap.transform,cold?"snow":"mist",Mathf.Max(.8f,radius)*.8f);fx.Burst("ring",at+up*.1f,Mathf.Max(1,radius),cold?ColdTint:PoisonTint);
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
                    fx.Decal("scorch",at,length*.5f,default,Mathf.Max(.5f,e.until-Combat.State.time));
                    fx.Burst("ring",at+up*.1f,Mathf.Max(2.5f,length*.5f),FireTint);fx.Burst("embers",at+up*.6f,1.6f);return root;
                }
                case "orb":
                {
                    var orb=Shape("Capacitor orb",PrimitiveType.Sphere,world.transform,at+up*1.4f,Vector3.one*.7f,blue);
                    fx.Aura(orb.transform,3,1.8f);fx.Burst("lightning",at+up*1.4f,1.1f);fx.Burst("flare",at+up*1.4f,1.8f,LightningTint);return orb;
                }
                case "vortex":
                {
                    fx.Decal("rune",at,Mathf.Max(2,radius),LightningTint,Mathf.Max(.5f,e.until-Combat.State.time));
                    var ring=Ring(world.transform,at+up*.08f,Mathf.Max(2,radius),blue,.08f);ring.name="Magnetic vortex";fx.Zone(ring.transform,"vortex",Mathf.Max(2,radius));return ring;
                }
                case "ward":
                {
                    fx.Decal("glow",at,Mathf.Max(2,radius),SpiritTint,Mathf.Max(.5f,e.until-Combat.State.time));
                    var ring=Ring(world.transform,at+up*.06f,Mathf.Max(2,radius),purple,.08f);ring.name="Rift ward";fx.Zone(ring.transform,"ward",Mathf.Max(2,radius));fx.Burst("ring",at+up*.1f,Mathf.Max(2,radius),SpiritTint);return ring;
                }
                case "rain":case "collapse":
                {
                    bool rainfall=ClassEffectKind(e)=="rain";float area=Mathf.Max(e.kind.StartsWith("d4",System.StringComparison.Ordinal)?0:3,radius);
                    var ring=Ring(world.transform,at+up*.05f,area,rainfall?Mat("Volley ring",RangerTint,true):ember,.07f);ring.name=rainfall?"Killing rain":"Triune collapse";
                    if(rainfall)fx.Zone(ring.transform,"rain",area);else fx.Decal("rune",at,Mathf.Max(3,area),ArcaneTint,Mathf.Max(.5f,e.until-Combat.State.time));
                    return ring;
                }
                case "ancestor":case "decoy":
                {
                    var spectre=Shape(ClassEffectKind(e)=="ancestor"?"Ancestor spirit":"Decoy",PrimitiveType.Capsule,world.transform,at+up,new Vector3(.8f,1,.8f),ClassEffectKind(e)=="ancestor"?blue:purple);
                    bool spirit=ClassEffectKind(e)=="ancestor";fx.Aura(spectre.transform,spirit?6:5,1.1f);fx.Decal("rune",at,1.6f,spirit?SpiritTint:ShadowTint,1.2f);
                    fx.Burst("arcane",at+up,1,SpiritTint);fx.Burst("star",at+up*1.2f,1.1f,spirit?SpiritTint:ShadowTint);return spectre;
                }
                case "ballista":
                {
                    var root=new GameObject("Watch ballista");root.transform.SetParent(world.transform,false);root.transform.position=at;
                    Shape("Ballista frame",PrimitiveType.Cube,root.transform,up*.5f,new Vector3(.7f,.5f,1.1f),darkStone);
                    Shape("Ballista bow",PrimitiveType.Cube,root.transform,up*.9f+Vector3.forward*.3f,new Vector3(1.4f,.08f,.12f),trim);
                    fx.Burst("dust",at,.8f);return root;
                }
                case "meteor":
                {
                    fx.Decal("rune",at,Mathf.Max(1,radius),FireTint,Mathf.Max(.1f,e.until-Combat.State.time));
                    var rock=Shape("Meteor",PrimitiveType.Sphere,world.transform,at+new Vector3(-5,18,-4),Vector3.one*1.3f,ember);fx.Fire(rock.transform,2.2f,FireTint);return rock;
                }
                case "nova":case "burst":return null;
                case "trail":fx.Beam(Position(e.origin)+up*.08f,at+up*.08f,FireTint,.45f,Mathf.Max(.1f,e.until-Combat.State.time),true);return null;
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
                // Meteors and novas are shown by the simulation's own blast (LayerSkillFx); an aftershock or quake pulse is shown here.
                case "burst":
                    fx.Burst("ring",at+up*.1f,Mathf.Max(1.5f,radius),ElementTint(e.element));fx.Burst("impact",at+up*.4f,1.2f);fx.Burst("dust",at,1.4f);
                    fx.Decal("crack",at,Mathf.Max(1.5f,radius)*.7f);lighting.Shake(.3f,.25f);break;
                case "meteor":case "nova":break;
                case "reactionShield":
                    fx.Burst(e.element==1?"fire":e.element==2?"frost":"lightning",at+up,1.2f,ElementTint(e.element));
                    fx.Shockwave(at+up*.1f,3,ElementTint(e.element),.4f);break;
                case "rain":
                    for(int i=0;i<5;i++)
                    {
                        float a=(tick*2.39996f+i*1.2566f)%6.2832f,r=radius*Mathf.Sqrt(((tick*7+i*3)%10+.5f)/10f);
                        var p=at+new Vector3(Mathf.Cos(a)*r,0,Mathf.Sin(a)*r);fx.Burst("impact",p+up*.2f,.55f);if(i==0){fx.Burst("dust",p,.7f);fx.Burst("star",p+up*.5f,.7f,GoldTint);}
                    }
                    break;
                case "collapse":
                {
                    // One stage per tick: fire blast, frost spikes, then a bolt from the sky.
                    int element=e.kind=="d4collapse"&&e.mod==1?e.element:tick;var tint=ElementTint(element);float stage=Mathf.Max(3,radius);
                    fx.Burst("ring",at+up*.1f,stage,tint);fx.Burst("lines.ground",at+up*.08f,stage*.9f,tint);lighting.Shake(.4f,.3f);
                    if(element==1){fx.Burst("explosion",at+up*1.1f,1.5f,tint);fx.Decal("scorch",at,stage*.8f);}
                    else if(element==2){fx.Burst("frost",at+up,1.8f);fx.Decal("ice",at,stage*.8f);for(int spike=0;spike<8;spike++){float rim=spike*Mathf.PI/4;var tip=at+new Vector3(Mathf.Cos(rim),0,Mathf.Sin(rim))*stage*.8f;After(.025f*spike,f=>f.Burst("frost",tip+up*.3f,.7f));}}
                    else{fx.Beam(at+up*15+Vector3.right*1.5f,at+up*.2f,tint,.5f,.28f,true);fx.Flash(at+up*1.2f,4,Color.white);fx.Burst("lightning",at+up,1.8f);}
                    fx.Burst("star",at+up*1.3f,1.7f,tint);break;
                }
                case "orb":case "vortex":
                {
                    EnemyState near=null;float best=float.MaxValue;
                    foreach(var enemy in run.enemies){if(enemy.dead)continue;float d=Vector2.Distance(enemy.position,e.position);if(d<=Mathf.Max(3,radius)&&d<best){best=d;near=enemy;}}
                    if(near!=null){fx.Beam(at+up*(ClassEffectKind(e)=="orb"?1.4f:.6f),Position(near.position)+up*1.1f,LightningTint,.12f,.16f,true);fx.Burst("star",Position(near.position)+up*1.2f,.8f,LightningTint);}
                    else if(e.kind=="vortex")fx.Burst("lightning",at+up*.6f,.8f);
                    break;
                }
                case "ancestor":fx.Slash(at+up*1.1f,Vector3.forward,1.8f,140,SpiritTint,.22f,true);fx.Burst("star",at+Vector3.forward*1.2f+up*1.1f,.9f,SpiritTint);break;
            }
        }
    }
}
