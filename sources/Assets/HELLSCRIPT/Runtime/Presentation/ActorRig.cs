using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hellscript
{
    public enum ActorArchetype{Biped,Quadruped,Hover,Blob,Worm,Spider,Serpent}
    public enum ActorAction{Idle,Windup,Release,Recover,Channel,Charge,Travel,Cast,Stagger,Hit,Dead,Despair,Offer}
    // Presentation state the caller derives each frame. speed in m/s (model units), progress 0..1 through the action,
    // time = presentation clock (add a per-actor offset to desync packs), aimLocal = target direction in the actor's space.
    public struct ActorPose{public float speed;public ActorAction action;public float progress;public float time;public Vector3 aimLocal;}

    // Procedural animation of the Pivot_* transforms an imported world-art model carries (see WorldArtImporter).
    // Bone names (character faces +Z, right hand at +X):
    //   Biped     Pivot_Hips, _Spine, _Chest, _Head, _Jaw, _ArmUpper_L/R, _ArmLower_L/R, _Hand_L/R, _LegUpper_L/R,
    //             _LegLower_L/R, _Foot_L/R, _Cape, _Tail*, _Weapon
    //   Quadruped Pivot_Hips, _Spine, _Chest, _Neck, _Head, _Jaw, _LegFront_L/R, _LegFrontLower_L/R, _LegBack_L/R,
    //             _LegBackLower_L/R, _Tail*
    //   Hover     Pivot_Hips (floats), _Chest, _Head, arms as Biped, _Shard*        Blob  Pivot_Hips, _Belly, _Head, arms
    //   Worm/Serpent  Pivot_Seg0..n (base to head), _Head, _Jaw, _Tail*               Spider Pivot_Body, _Leg<N>_L/R, _Leg<N>Lower_L/R
    // Missing bones are skipped. Tick only writes pivot local transforms: no allocation, no simulation access.
    public sealed class ActorRig:MonoBehaviour
    {
        public const float DeathSeconds=1;
        struct Bone{public Transform t;public Quaternion rest,q,qi,parentInv;public Vector3 pos,scale,at;}
        Bone[] bones=Array.Empty<Bone>();
        Vector3[] rot=Array.Empty<Vector3>(),move=Array.Empty<Vector3>(),grow=Array.Empty<Vector3>();
        int hips,spine,chest,neck,head,jaw,cape,belly,body;
        readonly int[] armU={-1,-1},armL={-1,-1},legU={-1,-1},legL={-1,-1},foot={-1,-1},legF={-1,-1},legFL={-1,-1},legB={-1,-1},legBL={-1,-1};
        int[] tail=Array.Empty<int>(),shard=Array.Empty<int>(),seg=Array.Empty<int>(),spiderLeg=Array.Empty<int>(),spiderLower=Array.Empty<int>(),spiderIndex=Array.Empty<int>();
        float size=1,phase,crawl,gait,flinch,flinchVelocity,deathTime;
        ActorAction last;
        const int Lean=0,Twist=1,ArmRP=2,ArmRR=3,ElbowR=4,ArmLP=5,ArmLR=6,ElbowL=7,Crouch=8,HeadP=9,Jaw=10,Tuck=11,Squash=12,Tilt=13,Wave=14,Channels=15;
        readonly float[] target=new float[Channels],ch=new float[Channels];
        public ActorArchetype Archetype{get;private set;}
        public int BoneCount=>bones.Length;
        public float DeathProgress=>Mathf.Clamp01(deathTime/DeathSeconds);

        public void Bind(Transform root,ActorArchetype archetype)
        {
            Archetype=archetype;var found=new List<Transform>();
            foreach(var t in root.GetComponentsInChildren<Transform>(true))if(t!=root&&t.name.StartsWith("Pivot_",StringComparison.Ordinal))found.Add(t);
            bones=new Bone[found.Count];rot=new Vector3[found.Count];move=new Vector3[found.Count];grow=new Vector3[found.Count];
            var inverse=Quaternion.Inverse(root.rotation);
            for(int i=0;i<found.Count;i++)
            {
                var t=found[i];var q=inverse*t.rotation;var parent=t.parent==root?Quaternion.identity:inverse*t.parent.rotation;
                bones[i]=new Bone{t=t,rest=t.localRotation,pos=t.localPosition,scale=t.localScale,q=q,qi=Quaternion.Inverse(q),parentInv=Quaternion.Inverse(parent),at=root.InverseTransformPoint(t.position)};
            }
            hips=Find("Hips");spine=Find("Spine");chest=Find("Chest");neck=Find("Neck");head=Find("Head");jaw=Find("Jaw");cape=Find("Cape");belly=Find("Belly");body=Find("Body");
            Pair(armU,"ArmUpper");Pair(armL,"ArmLower");Pair(legU,"LegUpper");Pair(legL,"LegLower");Pair(foot,"Foot");
            Pair(legF,"LegFront");Pair(legFL,"LegFrontLower");Pair(legB,"LegBack");Pair(legBL,"LegBackLower");
            tail=Series("Tail");shard=Series("Shard");seg=Series("Seg");
            var legs=new List<int>();var lowers=new List<int>();var index=new List<int>();
            for(int n=0;n<8;n++)for(int side=0;side<2;side++){int leg=Find("Leg"+n+(side==0?"_L":"_R"));if(leg<0)continue;legs.Add(leg);lowers.Add(Find("Leg"+n+"Lower"+(side==0?"_L":"_R")));index.Add(n*2+side);}
            spiderLeg=legs.ToArray();spiderLower=lowers.ToArray();spiderIndex=index.ToArray();
            int based=Base();size=based>=0?Mathf.Clamp(bones[based].at.y,.25f,10):1;if(size<.3f&&seg.Length>0)size=Mathf.Max(.3f,bones[seg[seg.Length-1]].at.magnitude*.5f);
            phase=crawl=gait=flinch=flinchVelocity=deathTime=0;last=ActorAction.Idle;Array.Clear(ch,0,Channels);
            Write();
        }
        int Find(string name){name="Pivot_"+name;for(int i=0;i<bones.Length;i++)if(bones[i].t.name==name)return i;return -1;}
        void Pair(int[] pair,string name){pair[0]=Find(name+"_L");pair[1]=Find(name+"_R");}
        // Chains sort base to tip: by depth, then by trailing number (Seg2 before Seg10).
        int[] Series(string prefix)
        {
            prefix="Pivot_"+prefix;var list=new List<int>();
            for(int i=0;i<bones.Length;i++){var n=bones[i].t.name;if(n.StartsWith(prefix,StringComparison.Ordinal)&&(n.Length==prefix.Length||!char.IsLetter(n[prefix.Length])||char.IsUpper(n[prefix.Length])))list.Add(i);}
            list.Sort((a,b)=>{int d=Depth(bones[a].t)-Depth(bones[b].t);return d!=0?d:Number(bones[a].t.name)!=Number(bones[b].t.name)?Number(bones[a].t.name)-Number(bones[b].t.name):string.CompareOrdinal(bones[a].t.name,bones[b].t.name);});
            return list.ToArray();
        }
        static int Depth(Transform t){int d=0;for(;t!=null;t=t.parent)d++;return d;}
        static int Number(string name){int end=name.Length,start=end;while(start>0&&char.IsDigit(name[start-1]))start--;return start<end&&end-start<9?int.Parse(name.Substring(start)):-1;}
        int Base()=>hips>=0?hips:body>=0?body:belly>=0?belly:seg.Length>0?seg[0]:-1;

        public void Flinch(float strength=1)=>flinchVelocity+=12*Mathf.Clamp(strength,0,2);

        public void Tick(float dt,ActorPose pose)
        {
            if(bones.Length==0)return;
            dt=Mathf.Clamp(dt,0,.1f);float time=pose.time;
            if(pose.action==ActorAction.Dead)deathTime+=dt;else deathTime=0;
            if(pose.action==ActorAction.Hit&&last!=ActorAction.Hit)Flinch();
            last=pose.action;
            // Short underdamped spring. Semi-implicit Euler stops damping it near the .1 s clamp (one mode reaches
            // eigenvalue -1 and flips sign forever), so it steps in equal substeps of at most 20 ms.
            int steps=Mathf.CeilToInt(dt/.02f);float h=steps>0?dt/steps:0;
            for(int i=0;i<steps;i++){flinchVelocity+=(-220*flinch-9*flinchVelocity)*h;flinch+=flinchVelocity*h;}
            bool dead=pose.action==ActorAction.Dead;
            float speed=dead||pose.action==ActorAction.Stagger?0:Mathf.Max(0,pose.speed);
            if(pose.action==ActorAction.Charge)speed=Mathf.Max(speed,6*size);
            float pace=speed/size;
            gait=Mathf.MoveTowards(gait,Mathf.Clamp01(pace/.8f),dt*4);
            phase=Mathf.Repeat(phase+dt*speed/(size*(1.4f+.25f*Mathf.Min(pace,6))),1);
            crawl=Mathf.Repeat(crawl+dt*(.35f+.45f*Mathf.Min(pace,6)),1);
            Targets(pose);
            float rate=pose.action==ActorAction.Release?30:dead?6:12,k=1-Mathf.Exp(-rate*dt);
            for(int i=0;i<Channels;i++)ch[i]+=(target[i]-ch[i])*k;
            for(int i=0;i<bones.Length;i++){rot[i]=Vector3.zero;move[i]=Vector3.zero;grow[i]=Vector3.zero;}
            float run=Mathf.Clamp01((pace-1.5f)/2.5f),a=phase*Mathf.PI*2,idle=1-gait;
            switch(Archetype)
            {
                case ActorArchetype.Biped:Biped(a,run,time,idle);break;
                case ActorArchetype.Quadruped:Quadruped(a,run,time,idle);break;
                case ActorArchetype.Hover:Hover(time);break;
                case ActorArchetype.Blob:Blob(a,time,idle);break;
                case ActorArchetype.Worm:Chain(time,false);break;
                case ActorArchetype.Serpent:Chain(time,true);break;
                case ActorArchetype.Spider:Spider(a,time,idle);break;
            }
            Aim(pose.aimLocal,idle);Tails(time);Death();
            Write();
        }

        // Shared intent channels; each archetype maps them onto its own bones. Angles in degrees, root axes
        // (+X pitch tips forward/down, +Y yaw turns right, +Z roll leans left).
        void Targets(ActorPose pose)
        {
            Array.Clear(target,0,Channels);float p=Mathf.Clamp01(pose.progress),s=Smooth(p),time=pose.time;
            switch(pose.action)
            {
                case ActorAction.Windup:Set(-10*s,20*s,-155*s,20*s,-55*s,-25*s,-15*s,-10*s,.35f*s,-6*s,25*s,0,.12f*s);break;
                case ActorAction.Release:{float r=1-(1-p)*(1-p)*(1-p);Released(r);break;}
                case ActorAction.Recover:{Released(1);float r=1-s;for(int i=0;i<Channels;i++)target[i]*=r;break;}
                case ActorAction.Channel:Set(6,Mathf.Sin(time*20)*4,-15,80,-5,-15,-80,-5,.3f,0,10,0,0);target[Wave]=1;break;
                case ActorAction.Charge:Set(22,0,30,8,-25,30,-8,-25,.25f,-12,12,0,-.06f);break;
                case ActorAction.Travel:{float t=Mathf.Sin(p*Mathf.PI);Set(12*t,0,-70*t,20*t,-30*t,-70*t,-20*t,-30*t,0,0,0,t,.1f*t);break;}
                case ActorAction.Cast:{float w=Mathf.Sin(time*6)*6;Set(-6,0,-85+w,15,-20,-85-w,-15,-20,.1f,-8,15,0,.06f);target[Wave]=.6f;break;}
                case ActorAction.Stagger:Set(18,Mathf.Sin(time*2.3f)*8,-8,10,-10,-8,-10,-10,.45f,22,18,0,-.08f);target[Tilt]=Mathf.Sin(time*3.1f)*7;target[Wave]=.5f;break;
                case ActorAction.Dead:Set(-10,0,-20,50,-10,-20,-50,-10,.6f,-20,22,0,0);break;
                // Staged prologue poses. Despair: sunk to one knee, head bowed, arms hanging, a faint tremble.
                case ActorAction.Despair:Set(24*s,0,12*s,14*s,-18*s,12*s,-14*s,-18*s,.62f*s,(32+Mathf.Sin(time*9)*1.5f)*s,0,0,-.04f*s);break;
                // Offer: rising up, both arms stretched to the sky holding the scroll, face turned up to the light.
                case ActorAction.Offer:Set(-12*s,0,-172*s,-6*s,-6*s,-168*s,6*s,-6*s,0,-30*s,8*s,0,.06f*s);break;
            }
        }
        void Released(float r)=>Set(Mathf.Lerp(-10,16,r),Mathf.Lerp(20,-35,r),Mathf.Lerp(-155,-30,r),Mathf.Lerp(20,-10,r),Mathf.Lerp(-55,-10,r),Mathf.Lerp(-25,15,r),-15,-10,Mathf.Lerp(.35f,.55f,r),Mathf.Lerp(-6,6,r),25*(1-r),0,Mathf.Lerp(.12f,-.18f,r));
        void Set(float lean,float twist,float armRP,float armRR,float elbowR,float armLP,float armLR,float elbowL,float crouch,float headP,float jawOpen,float tuck,float squash)
        {target[Lean]=lean;target[Twist]=twist;target[ArmRP]=armRP;target[ArmRR]=armRR;target[ElbowR]=elbowR;target[ArmLP]=armLP;target[ArmLR]=armLR;target[ElbowL]=elbowL;target[Crouch]=crouch;target[HeadP]=headP;target[Jaw]=jawOpen;target[Tuck]=tuck;target[Squash]=squash;}
        static float Smooth(float x)=>x*x*(3-2*x);

        void R(int i,float x,float y,float z){if(i>=0)rot[i]+=new Vector3(x,y,z);}
        void M(int i,Vector3 d){if(i>=0)move[i]+=d;}
        void S(int i,Vector3 d){if(i>=0)grow[i]+=d;}

        // Upper body shared by bipeds, hover and blob casters: torso lean/twist, arms, head stabilised against the torso.
        void Upper(float breathe,float sway,float flinchLean)
        {
            float lean=ch[Lean]+flinchLean;
            R(spine,lean*.4f+breathe*.5f,ch[Twist]*.4f,ch[Tilt]*.5f);R(chest,lean*.6f-breathe,ch[Twist]*.6f,0);
            R(neck,-lean*.2f,0,0);R(head,ch[HeadP]-lean*.5f+sway*.3f-flinch*6,-ch[Twist]*.6f+sway*6,-ch[Tilt]*.4f);R(jaw,ch[Jaw],0,0);
            R(armU[1],ch[ArmRP]-flinch*10,0,ch[ArmRR]+flinch*8);R(armL[1],ch[ElbowR],0,0);
            R(armU[0],ch[ArmLP]-flinch*10,0,ch[ArmLR]-flinch*8);R(armL[0],ch[ElbowL],0,0);
        }
        void Biped(float a,float run,float time,float idle)
        {
            float sw=Mathf.Sin(a),cw=Mathf.Cos(a),w=gait,amp=w*(22+14*run),breathe=Mathf.Sin(time*1.76f)*1.2f;
            for(int side=0;side<2;side++)
            {
                float dir=side==0?1:-1,swing=-sw*amp*dir,knee=w*(5+(35+30*run)*Mathf.Max(0,cw*dir));
                R(legU[side],swing,0,0);R(legL[side],knee,0,0);R(foot[side],-(swing+knee)*.5f,0,0);
                R(armU[side],sw*amp*.8f*dir,0,0);R(armL[side],-w*(8+22*run),0,0);
            }
            float c=ch[Crouch],t=ch[Tuck];
            for(int side=0;side<2;side++){R(legU[side],-40*c-60*t,0,0);R(legL[side],80*c+95*t,0,0);R(foot[side],-40*c-20*t,0,0);}
            M(hips,Vector3.up*size*(w*(.025f+.03f*run)*Mathf.Cos(2*a)-.22f*c+.004f*breathe));
            R(hips,0,sw*6*w,sw*3*w+Mathf.Sin(time*.9f)*1.2f*idle+ch[Tilt]);
            R(chest,w*(4+10*run),-sw*9*w,0);R(head,-w*(4+10*run)*.6f,sw*3*w,0);
            Upper(breathe,Mathf.Sin(time*.43f)*idle,-flinch*14);
            R(cape,w*(10+25*run)+Mathf.Sin(time*3)*3+ch[Wave]*25+flinch*10,0,0);
        }
        void Quadruped(float a,float run,float time,float idle)
        {
            float sw=Mathf.Sin(a),cw=Mathf.Cos(a),w=gait,amp=w*(24+12*run),c=ch[Crouch];
            // Diagonal pairs (front-left with back-right) move together; each lower leg bends while its own leg swings forward.
            for(int side=0;side<2;side++)
            {
                float dir=side==0?1:-1,swing=-sw*amp*dir,lift=w*(5+40*Mathf.Max(0,cw*dir)),liftB=w*(5+40*Mathf.Max(0,-cw*dir));
                R(legF[side],swing-35*c,0,0);R(legFL[side],lift+50*c,0,0);
                R(legB[side],-swing-35*c,0,0);R(legBL[side],-liftB*1.2f-40*c,0,0);
            }
            R(legF[1],ch[ArmRP]*.45f,0,ch[ArmRR]*.3f);R(legF[0],ch[ArmLP]*.2f,0,0);
            float lean=ch[Lean]-flinch*10,lunge=Mathf.Max(0,ch[Lean])/16*.2f*size;
            M(hips,Vector3.up*size*(w*.03f*Mathf.Cos(2*a)-.2f*c)+Vector3.forward*lunge);
            R(hips,0,sw*4*w,sw*2*w+ch[Tilt]);R(spine,lean*.4f+Mathf.Sin(2*a)*3*w,ch[Twist]*.4f,0);R(chest,lean*.6f-Mathf.Sin(2*a)*2*w+Mathf.Sin(time*1.6f)*idle,ch[Twist]*.6f,0);
            R(neck,-lean*.5f+ch[HeadP]*.5f,-ch[Twist]*.5f+Mathf.Sin(time*.5f)*6*idle,0);R(head,ch[HeadP]*.5f+Mathf.Sin(2*a)*3*w-flinch*8,Mathf.Sin(time*.37f)*5*idle,0);
            R(jaw,ch[Jaw]+Mathf.Max(0,Mathf.Sin(time*.8f))*4*idle,0,0);
        }
        void Hover(float time)
        {
            float w=gait,bob=Mathf.Sin(time*1.7f)*.08f*size,drive=1+2*Mathf.Abs(ch[Wave]+ch[Squash]*4);
            M(hips,Vector3.up*(bob-size*.1f*ch[Crouch]));R(hips,w*14+ch[Lean]*.3f,Mathf.Sin(time*.6f)*5,Mathf.Sin(time*1.1f)*4+ch[Tilt]);
            for(int side=0;side<2;side++){float dir=side==0?1:-1;R(armU[side],w*25+Mathf.Sin(time*2+side*1.3f)*5,0,-dir*Mathf.Sin(time*1.4f+side)*4);R(armL[side],-w*10+Mathf.Sin(time*2.4f+side)*5,0,0);}
            Upper(Mathf.Sin(time*1.3f)*1.5f,Mathf.Sin(time*.5f),-flinch*16);
            for(int i=0;i<shard.Length;i++){R(shard[i],0,Mathf.Repeat(time*40*drive+i*137.5f,360),Mathf.Sin(time*1.9f+i)*10);M(shard[i],Vector3.up*Mathf.Sin(time*2.3f+i*1.7f)*.05f*size);}
        }
        void Blob(float a,float time,float idle)
        {
            float sw=Mathf.Sin(a),w=gait,breathe=Mathf.Sin(time*1.5f);
            M(hips,Vector3.up*size*(Mathf.Abs(sw)*.05f*w-.12f*ch[Crouch]));R(hips,ch[Lean]*.3f,ch[Twist]*.3f,sw*7*w+ch[Tilt]);
            float s=ch[Squash]+breathe*.03f-.06f*w*Mathf.Abs(Mathf.Cos(a))-flinch*.1f;S(belly>=0?belly:hips,new Vector3(-s*.5f,s,-s*.5f));
            for(int side=0;side<2;side++){float dir=side==0?1:-1;R(legU[side],-sw*18*w*dir,0,0);R(legL[side],w*10*Mathf.Max(0,Mathf.Cos(a)*dir),0,0);}
            Upper(breathe,Mathf.Sin(time*.43f)*idle,-flinch*12);R(head,Mathf.Sin(time*1.3f)*2,0,Mathf.Sin(time*.9f)*3);
        }
        // Worm: vertical undulation, rears on windup and strikes on release. Serpent: lateral slither, head held forward.
        void Chain(float time,bool lateral)
        {
            int n=seg.Length;if(n==0){R(head,ch[HeadP],0,0);R(jaw,ch[Jaw]*1.2f,0,0);return;}
            float alive=1-Smooth(DeathProgress),amp=(6+10*gait+8*ch[Wave])*alive,wave=crawl*Mathf.PI*2,sink=size*1.1f*ch[Tuck];
            for(int i=0;i<n;i++)
            {
                float tip=(i+1f)/n,rear=ch[Lean]*3*2*tip/n+ch[Crouch]*6*(1-tip)-flinch*6*tip;
                if(lateral)R(seg[i],rear-DeathProgress*6,Mathf.Sin(wave-i*.8f)*amp,0);
                else R(seg[i],Mathf.Sin(wave-i*.9f)*amp+rear-DeathProgress*6,Mathf.Sin(wave*.5f-i*.7f)*amp*.35f,0);
            }
            M(seg[0],Vector3.down*sink);
            R(head,ch[HeadP]-ch[Lean]*.5f,lateral?-Mathf.Sin(wave-n*.8f)*amp*.6f:0,0);R(jaw,ch[Jaw]*1.2f,0,0);
        }
        // Alternating tetrapod gait: legs (0L,1R,2L,3R) step together, the other four half a cycle later.
        void Spider(float a,float time,float idle)
        {
            float w=gait,d=Smooth(DeathProgress);
            for(int j=0;j<spiderLeg.Length;j++)
            {
                int n=spiderIndex[j]>>1,side=spiderIndex[j]&1;float s=side==0?-1:1,la=a+((n+side)&1)*Mathf.PI;
                float lift=Mathf.Max(0,Mathf.Cos(la))*20*w+(n==0?-ch[ArmRP]*.3f:0)+Mathf.Sin(time*1.3f+j)*2*idle;
                R(spiderLeg[j],0,-s*Mathf.Sin(la)*18*w,s*(lift+40*d));R(spiderLower[j],0,0,-s*(lift*.5f+110*d));
            }
            M(body,Vector3.up*size*(.02f*Mathf.Cos(2*a)*w-.25f*ch[Crouch]));R(body,ch[Lean]*.5f-flinch*8,ch[Twist]*.3f,ch[Tilt]+Mathf.Sin(time*.7f)*1.5f*idle);
            R(head,ch[HeadP]*.3f,0,0);R(jaw,ch[Jaw],0,0);
        }
        void Aim(Vector3 aim,float idle)
        {
            if(aim.x*aim.x+aim.z*aim.z<1e-4f||deathTime>0)return;
            float yaw=Mathf.Clamp(Mathf.Atan2(aim.x,aim.z)*Mathf.Rad2Deg,-45,45)*(.5f+.5f*idle);
            if(Archetype==ActorArchetype.Quadruped){R(neck,0,yaw*.5f,0);R(head,0,yaw*.4f,0);}else{R(chest,0,yaw*.4f,0);R(head,0,yaw*.5f,0);}
        }
        void Tails(float time)
        {
            float alive=1-Smooth(DeathProgress);
            for(int i=0;i<tail.Length;i++)R(tail[i],Mathf.Sin(time*1.7f-i*.6f)*4*alive+ch[Lean]*.2f,Mathf.Sin(time*2-i*.7f)*(8+10*gait)*alive,0);
        }
        // Collapse over DeathSeconds: knees buckle (channels), then the base bone topples about its ground point and sinks.
        void Death()
        {
            float d=DeathProgress;if(d<=0)return;
            int based=Base();if(based<0)return;
            float fall=Smooth(Mathf.Clamp01((d-.15f)/.75f));Vector3 euler;float sink=.12f;
            switch(Archetype)
            {
                case ActorArchetype.Quadruped:euler=new Vector3(0,0,80*fall);break;
                case ActorArchetype.Hover:euler=new Vector3(40*fall,0,15*fall);sink=.6f;break;
                case ActorArchetype.Blob:{float s=d<.3f?.25f*d/.3f:Mathf.Lerp(.25f,-.55f,(d-.3f)/.7f);S(belly>=0?belly:based,new Vector3(-s*.6f,s,-s*.6f));euler=new Vector3(0,0,10*fall);break;}
                case ActorArchetype.Worm:case ActorArchetype.Serpent:euler=Vector3.zero;sink=.3f;break;
                case ActorArchetype.Spider:euler=Vector3.zero;sink=.5f;break;
                default:euler=new Vector3(-75*fall,0,20*fall);break;
            }
            var at=bones[based].at;var ground=new Vector3(at.x,0,at.z);
            R(based,euler.x,euler.y,euler.z);M(based,ground+Quaternion.Euler(euler)*(at-ground)-at+Vector3.down*size*sink*fall);
        }
        void Write()
        {
            for(int i=0;i<bones.Length;i++)
            {
                var b=bones[i];if(b.t==null)continue;
                b.t.localRotation=b.rest*(b.qi*Quaternion.Euler(rot[i])*b.q);
                b.t.localPosition=b.pos+b.parentInv*move[i];
                b.t.localScale=Vector3.Scale(b.scale,Vector3.one+grow[i]);
            }
        }
    }
}
