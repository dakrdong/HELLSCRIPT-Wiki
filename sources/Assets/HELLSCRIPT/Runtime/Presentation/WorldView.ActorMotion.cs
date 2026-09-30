using System.Collections.Generic;
using UnityEngine;

namespace Hellscript
{
    // Enemy and boss bodies act out their attacks, and struck bodies react. A windup leans back and raises the weapon, crouches,
    // aims or levitates according to the attack, glows along the rim as the gauge fills and trembles near the end; the release
    // swings, lunges, recoils or thrusts. A struck enemy flashes white, a killed one dissolves where it fell, the hero flashes red
    // and the camera shakes with the share of health a hit took. Poses offset the parts of the placeholder figure around its
    // feet from their rest pose, so the actor hierarchy and every lookup by name or child index stay as they are.
    // Presentation only: reads RunState and never writes it; the step it is given already honours pause and game speed.
    public sealed partial class WorldView
    {
        public enum AttackMotion{Swing,Charge,Shoot,Cast}
        sealed class Figure
        {
            public Transform[] parts;public Vector3[] restPosition;public Quaternion[] restRotation;public int weapon=-1;
            public Renderer[] renderers;
            public float flash,flashTotal=.12f,rim,dissolve;public Color flashColor,rimColor,tierRim,baseRim;public bool shaded;
            public int action=-1;public float windup,strikeAge=-1;public AttackMotion motion,strikeMotion;
        }
        // Parts that make up the figure; rings, the health bar and marks keep their own placement.
        static readonly HashSet<string> FigureParts=new HashSet<string>{"Body","Head","Face light","Shoulders","Weapon","Warrior shield","Support lantern"};
        const float StrikeTime=.3f,DissolveTime=.65f,HeroFlashTime=.16f,EnemyFlashTime=.11f;
        static readonly Vector3 Shoulder=new Vector3(.35f,1.6f,0);
        readonly Dictionary<GameObject,Figure> figures=new Dictionary<GameObject,Figure>();
        readonly HashSet<GameObject> enragedAuras=new HashSet<GameObject>();
        readonly List<GameObject> staleFigures=new List<GameObject>();
        sealed class Dying{public GameObject go;public float age;}
        readonly List<Dying> dying=new List<Dying>();
        static MaterialPropertyBlock shadeBlockInstance;
        static MaterialPropertyBlock ShadeBlock=>shadeBlockInstance??=new MaterialPropertyBlock();
        static readonly int FlashId=Shader.PropertyToID("_Flash"),FlashColorId=Shader.PropertyToID("_FlashColor"),RimColorId=Shader.PropertyToID("_RimColor"),DissolveId=Shader.PropertyToID("_Dissolve");
        DamageEvent lastDamageSeen;
        float motionStep;

        Figure FigureOf(GameObject actor)
        {
            if(figures.TryGetValue(actor,out var f))return f;
            f=new Figure();var parts=new List<Transform>();
            for(int i=0;i<actor.transform.childCount;i++){var c=actor.transform.GetChild(i);if(FigureParts.Contains(c.name))parts.Add(c);}
            f.parts=parts.ToArray();f.restPosition=new Vector3[f.parts.Length];f.restRotation=new Quaternion[f.parts.Length];
            for(int i=0;i<f.parts.Length;i++){f.restPosition[i]=f.parts[i].localPosition;f.restRotation[i]=f.parts[i].localRotation;if(f.parts[i].name=="Weapon")f.weapon=i;}
            var renderers=new List<Renderer>();
            foreach(var r in actor.GetComponentsInChildren<Renderer>(true))
                if(!(r is LineRenderer)&&r.gameObject.name!="Health"&&r.gameObject.name!="Hero occlusion silhouette")renderers.Add(r);
            f.renderers=renderers.ToArray();if(HasModelBody(actor))f.baseRim=WorldArtMaterials.Rim;figures[actor]=f;return f;
        }
        // How a body moves for an attack: regular enemies by kind, bosses by attack (item-specific bodies come with the kits).
        public static AttackMotion MotionOf(EnemyState enemy,int kind)
        {
            if(enemy.boss)
            {
                switch((BossAttack)kind)
                {
                    case BossAttack.Charge:case BossAttack.ExecutionLeap:case BossAttack.BurrowStrike:return AttackMotion.Charge;
                    case BossAttack.Hook:case BossAttack.IceLance:case BossAttack.VenomSpray:case BossAttack.ShatterFan:case BossAttack.ShardStorm:case BossAttack.LamentOrbs:case BossAttack.TailLash:return AttackMotion.Shoot;
                    case BossAttack.Basic:case BossAttack.Slam:case BossAttack.ReapingSweep:case BossAttack.MawSnap:case BossAttack.ChainWhirl:case BossAttack.DevouringPull:case BossAttack.Legacy:return AttackMotion.Swing;
                    default:return AttackMotion.Cast;
                }
            }
            switch(EnemyCombat.Role(enemy.kind))
            {
                case 1:return EnemyCombat.Attack(enemy.kind).range>=8?AttackMotion.Charge:AttackMotion.Swing;
                case 2:return AttackMotion.Shoot;case 3:case 4:return AttackMotion.Cast;default:return AttackMotion.Swing;
            }
        }
        static bool ReleasedRecently(RunState run,int action)
        {
            for(int i=run.enemyEvents.Count-1;i>=0;i--){var ev=run.enemyEvents[i];if(ev.time<run.time-1)break;if(ev.actionId==action&&ev.kind=="RELEASE")return true;}
            return false;
        }
        // Called for every presented enemy after its root has been placed and turned.
        void PoseEnemy(GameObject actor,EnemyState enemy,RunState run)
        {
            var f=FigureOf(actor);var a=enemy.brain.action;bool preparing=a.phase==EnemyActionPhase.Preparing;
            float windup=preparing?Mathf.Clamp01(1-a.remaining/Mathf.Max(.01f,a.preparation)):0;
            // A windup that ended in a release (or a followup rewinding it) strikes; an interrupted one just relaxes.
            if(f.action>=0&&f.windup>=.6f&&(f.action!=a.id||!preparing||windup<f.windup-.4f)&&ReleasedRecently(run,f.action)){f.strikeAge=0;f.strikeMotion=f.motion;}
            f.action=a.phase==EnemyActionPhase.Idle?-1:a.id;f.windup=windup;if(a.phase!=EnemyActionPhase.Idle)f.motion=!enemy.boss&&a.kind==14&&a.variant==1?AttackMotion.Charge:MotionOf(enemy,a.kind);
            if(f.strikeAge>=0){f.strikeAge+=motionStep;if(f.strikeAge>StrikeTime)f.strikeAge=-1;}
            float strike=f.strikeAge<0?0:f.strikeAge<.06f?f.strikeAge/.06f:1-(f.strikeAge-.06f)/(StrikeTime-.06f);
            float lean=0,crouch=0,lift=0,lunge=0,weapon=0,tremble=preparing&&windup>.8f?(windup-.8f)*5:0;
            switch(preparing?f.motion:f.strikeMotion)
            {
                case AttackMotion.Swing:lean=-14*windup+24*strike;weapon=-105*windup+160*strike;crouch=.07f*windup;lunge=.5f*strike;break;
                case AttackMotion.Charge:lean=16*windup;crouch=.15f*windup;break;
                case AttackMotion.Shoot:lean=-8*windup-8*strike;weapon=-40*windup+15*strike;lunge=-.22f*strike;break;
                case AttackMotion.Cast:lift=.2f*windup;lean=-7*windup+12*strike;weapon=-145*windup+70*strike;lunge=.18f*strike;break;
            }
            bool blinking=false;
            if(a.phase==EnemyActionPhase.Charging)
            {
                if(enemy.boss&&BossCombat.Moves((BossAttack)a.kind,out var travel))
                {
                    // A travelling boss: a leap arcs (root scaled 1.7 high, so 2.2 local is ~3.7 m at the top), a burrow sinks
                    // under the floor and bursts up at the end, glides and dashes lean into the run, a whirl spins, a blink fades.
                    float total=travel.LegTime(BossCombat.LegStart(a,a.leg),BossCombat.Waypoint(a,a.leg)),t=total>1e-4f?Mathf.Clamp01(a.legElapsed/total):1;
                    switch(travel.style)
                    {
                        case BossLegStyle.Leap:lift=4*2.2f*t*(1-t);lean=14-28*t;break;
                        case BossLegStyle.Burrow:lift=t<.8f?-2.6f:-2.6f*(1-(t-.8f)/.2f);break;
                        case BossLegStyle.Glide:lean=18;lift=.12f;break;
                        case BossLegStyle.Blink:blinking=true;f.dissolve=Mathf.Max(f.dissolve,t);break;
                        default:if(travel.carriesWhirl){weapon=-70;actor.transform.rotation=Quaternion.Euler(0,elapsed*900,0);}else{lean=26;crouch=.06f;}break;
                    }
                }
                else if(EnemyCombat.Travels(a))
                {
                    // The sandworm sinks under the floor and bursts up at the end; the ghoul arcs onto its landing.
                    float t=Mathf.Clamp01(a.moved/Mathf.Max(.01f,EnemyCombat.Travel(a)));
                    if(a.kind==13)lift=t<.8f?-1.9f:-1.9f*(1-(t-.8f)/.2f);else{lift=4*1.3f*t*(1-t);lean=14-28*t;}
                }
                else{lean=28;crouch=.08f;lift=Mathf.Abs(Mathf.Sin(elapsed*18))*.06f;}
            }
            // After a blink the body fades back in where it appeared.
            if(!blinking&&f.dissolve>0)f.dissolve=Mathf.Max(0,f.dissolve-motionStep/.25f);
            // A model boss has no 2 x 1.7 root scale, so its travel offsets are scaled here to leap as high and sink as deep.
            if(enemy.boss&&HasModelBody(actor)){lift*=1.7f;lunge*=2;}
            ApplyPose(f,lean,crouch,lift,lunge,weapon,tremble*.035f*Mathf.Sin(elapsed*57));
            // The rim heats up with the gauge: bosses burn orange, regular enemies red.
            f.rim=preparing?windup*windup*.95f:strike*.6f;f.rimColor=enemy.boss?new Color(1,.45f,.12f):new Color(1,.2f,.14f);
            // The monster tier (elites today; gameplay sets the others later) wears its sigil effect and colours the rim.
            var tier=MonsterTierOf(enemy);if(Fx!=null)Fx.Tier(actor,tier);f.tierRim=tier==MonsterTier.Normal?default:WorldFx.TierColor(tier)*.8f;
            TickEnemyRig(actor,enemy,f);
            // A boss in its second phase smoulders: a steady red rim and short-lived flames at its feet that stay with it.
            if(enemy.boss&&enemy.brain.boss.enraged)
            {
                f.rim=Mathf.Max(f.rim,.3f+.08f*Mathf.Sin(elapsed*6));if(!preparing)f.rimColor=new Color(1,.25f,.1f);
                if(enragedAuras.Add(actor)&&Fx!=null)Fx.Fire(actor.transform,.75f,new Color(1,.28f,.1f));
            }
        }
        void ApplyPose(Figure f,float lean,float crouch,float lift,float lunge,float weapon,float jitter)
        {
            var tilt=Quaternion.Euler(lean,0,0);var offset=new Vector3(jitter,lift,lunge);
            for(int i=0;i<f.parts.Length;i++)
            {
                var t=f.parts[i];if(t==null)continue;var rest=f.restPosition[i];rest.y*=1-crouch;var rotation=f.restRotation[i];
                if(i==f.weapon&&weapon!=0){var swing=Quaternion.AngleAxis(weapon,Vector3.right);rest=Shoulder+swing*(rest-Shoulder);rotation=swing*rotation;}
                t.localPosition=tilt*rest+offset;t.localRotation=tilt*rotation;
            }
        }
        // Keeps a killed enemy's body for a dissolve instead of destroying it at once; it leaves `actors` immediately.
        void StartDying(GameObject actor,EnemyState enemy,RunState run)
        {
            var f=FigureOf(actor);f.flash=0;f.rim=0;
            foreach(Transform child in actor.transform)if(!FigureParts.Contains(child.name))child.gameObject.SetActive(false);
            dying.Add(new Dying{go=actor});
            var library=Fx;if(library!=null&&actor.activeInHierarchy&&CanDisplayEnemyMarker(run,enemy.position))library.Death(actor.transform.position,enemy.boss?2.2f:1,WorldFx.SurfaceOf(enemy));
        }
        // New damage since the last frame: enemy hits flash and throw debris, hero hits flash and shake the camera.
        void PresentCombatFeedback(RunState run)
        {
            var library=Fx;float maxHealth=Mathf.Max(1,Combat.Stats.hp);float strongest=0;
            GameAudio.ReadNew(run.damageEvents,ref lastDamageSeen,d=>
            {
                if(d.time<run.time-.5f)return;
                if(d.incoming)
                {
                    if(d.hpLoss<=0&&d.absorbed<=0)return;
                    var f=FigureOf(hero);f.flash=f.flashTotal=HeroFlashTime;f.flashColor=new Color(1,.28f,.2f);FlinchRig(hero,false);
                    strongest=Mathf.Max(strongest,Mathf.Max(d.hpLoss,d.absorbed*.5f)/maxHealth);
                    if(library!=null)library.Burst(d.element>0&&d.element<6?ElementBurst[d.element]:"blood",hero.transform.position+Vector3.up*1.3f,.8f);
                    return;
                }
                if(!actors.TryGetValue(d.targetId,out var actor)||!actor.activeInHierarchy)return;
                var enemy=run.enemies.Find(e=>e.id==d.targetId);if(enemy==null||!CanDisplayEnemyMarker(run,enemy.position))return;
                var fig=FigureOf(actor);fig.flash=fig.flashTotal=EnemyFlashTime;fig.flashColor=d.critical?new Color(1,.9f,.55f):Color.white;if(d.kind!=DamageKind.Periodic)FlinchRig(actor,d.critical);
                if(library!=null&&d.kind!=DamageKind.Periodic)library.Hit(enemy,d.element,d.critical,actor.transform.position+Vector3.up*(enemy.boss?2.4f:1.4f));
            },(a,b)=>a.id==b.id&&a.time==b.time);
            // Share of max health: a scratch nudges the camera, a quarter of the bar or more hits hard.
            if(strongest>0)lighting.Shake(Mathf.Clamp(.18f+strongest*2.6f,.18f,.75f),Mathf.Clamp(.16f+strongest*.8f,.16f,.42f));
        }
        static readonly string[] ElementBurst={"blood","fire","frost","lightning","poison","shadow"};
        // Flash, rim and dissolve reach the renderers through one property block per renderer; a figure with nothing left to
        // show is cleared once so the renderers return to their shared material values.
        void ShadeFigures(float step)
        {
            staleFigures.Clear();
            foreach(var pair in figures)
            {
                var f=pair.Value;if(pair.Key==null){staleFigures.Add(pair.Key);continue;}
                f.flash=Mathf.Max(0,f.flash-step);
                bool any=f.flash>0||f.rim>.001f||f.dissolve>0||f.tierRim.maxColorComponent>0||f.baseRim.maxColorComponent>0;if(!any&&!f.shaded)continue;
                var block=ShadeBlock;
                foreach(var r in f.renderers)
                {
                    if(r==null)continue;
                    if(!any){r.SetPropertyBlock(null);continue;}
                    r.GetPropertyBlock(block);
                    block.SetFloat(FlashId,f.flashTotal>0?f.flash/f.flashTotal:0);block.SetColor(FlashColorId,f.flashColor);
                    var rim=f.rimColor*f.rim;rim=new Color(Mathf.Max(rim.r,Mathf.Max(f.tierRim.r,f.baseRim.r)),Mathf.Max(rim.g,Mathf.Max(f.tierRim.g,f.baseRim.g)),Mathf.Max(rim.b,Mathf.Max(f.tierRim.b,f.baseRim.b)),1);
                    block.SetColor(RimColorId,rim);block.SetFloat(DissolveId,f.dissolve);r.SetPropertyBlock(block);
                }
                f.shaded=any;
            }
            foreach(var key in staleFigures)figures.Remove(key);
            for(int i=dying.Count-1;i>=0;i--)
            {
                var d=dying[i];if(d.go==null){dying.RemoveAt(i);continue;}
                // A model collapses with its rig first and dissolves after; a primitive figure just dissolves.
                d.age+=step;var f=FigureOf(d.go);bool rigged=rigs.TryGetValue(d.go,out var rig)&&rig!=null;float total=rigged?ActorRig.DeathSeconds+DissolveTime*.6f:DissolveTime;
                if(rigged)rig.Tick(step,new ActorPose{action=ActorAction.Dead,progress=Mathf.Clamp01(d.age/ActorRig.DeathSeconds),time=elapsed});
                f.dissolve=Mathf.Clamp01((d.age-(rigged?ActorRig.DeathSeconds*.55f:0))/(total-(rigged?ActorRig.DeathSeconds*.55f:0)));
                if(d.age>=total){figures.Remove(d.go);rigs.Remove(d.go);rigPositions.Remove(d.go);Destroy(d.go);dying.RemoveAt(i);}
            }
        }
        void ClearActorMotion(){figures.Clear();dying.Clear();enragedAuras.Clear();lastDamageSeen=null;}
        void SeedCombatFeedback(RunState run){lastDamageSeen=run.damageEvents.Count>0?run.damageEvents[run.damageEvents.Count-1]:null;}
    }
}
