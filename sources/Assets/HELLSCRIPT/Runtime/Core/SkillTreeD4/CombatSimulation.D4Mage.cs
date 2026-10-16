using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    public sealed partial class CombatSimulation
    {
        float D4MageCost(string id,float original)=>id=="M01"||id=="M03"?D4BaseValue(id,1):original;
        int D4MageElement(string id,int mod)=>id=="M07"?(mod==2?2:mod==3?3:1):ClassSkills.ElementOf(id);
        float D4MageNumber(string id,string key,float original,ClassSkillCast cast)
        {
            int mod=cast?.mod??Loadout.Modifier(id);
            if(id=="M07"&&mod==3&&key=="range")return N("M07-3","range",12);
            if(id=="M08"&&key=="duration")return mod==3?D4ModValue(id,cast):D4BaseValue(id,1,cast);
            return original;
        }
        ClassSkillEffect D4MageEffect(ClassSkillCast cast,string suffix,string kind,float duration,float value=0,float period=0)
        {
            var owner=cast.secondary?Cast(cast.root):cast;
            var effect=new ClassSkillEffect{id=cast.id+suffix+":"+State.nextId++,source=owner.id,mod=owner.mod,modRank=owner.modRank,root=cast.root,kind=kind,
                created=State.time,until=State.time+duration,next=State.time+period,period=period,value=value,position=cast.aim,origin=cast.origin,element=cast.element,direction=(cast.aim-cast.origin).normalized,snapshot=cast.snapshot};
            if(effect.direction==Vector2.zero)effect.direction=Vector2.up;
            if(cast.secondary)effect.copiedCast=JsonUtility.FromJson<ClassSkillCast>(JsonUtility.ToJson(cast));CS.effects.Add(effect);return effect;
        }
        DamageEvent D4MageHit(ClassSkillCast cast,EnemyState enemy,float power,int element,bool secondary=false,bool periodic=false,Vector2? origin=null)
        {
            if(cast.secondary||secondary)return SkillHit(cast,enemy,power*cast.damageScale,element,true,cast.id,attackOrigin:origin,modifierValue:true);
            return Hit(enemy,power,element,false,snapshot:cast.snapshot,canCrit:!periodic,definition:cast.id,root:cast.root,kind:periodic?DamageKind.Periodic:DamageKind.Direct,origin:origin,modifierValue:true);
        }
        void CaptureD4MagePassives(ClassSkillCast cast)
        {
            cast.element=D4MageElement(cast.id,cast.mod);
            if(cast.id=="M17"&&cast.mod==1)cast.element=CS.lastElement>=1&&CS.lastElement<=3?CS.lastElement:1;
        }
        bool ResolveD4MageNative(ClassSkillCast cast)
        {
            if(cast.id=="M07")
            {
                int hits=0;foreach(var enemy in Line(cast.origin,cast.aim,N(cast.id,"range",cast:cast),N(cast.id,"width",cast:cast),(int)N(cast.id,"targets",3,cast)))
                {
                    bool slowed=CombatEffects.Has(enemy,StatusKind.Slow);if(D4MageHit(cast,enemy,D4BaseValue(cast.id,0,cast)/100,cast.element)==null)continue;hits++;
                    if(cast.mod==1)Dot("M07",enemy,D4ModValue(cast.id,cast)/100*(Gear("DES_LM41")&&InFirewall(enemy.position)?1.3f:1),1,N("M07","duration",3),cast,modifierValue:true);
                    if(cast.mod==2)
                    {ApplyStatus(enemy,slowed?StatusKind.Freeze:StatusKind.Slow,"M07",slowed?D4ModValue(cast.id,cast):N("M07-2","duration",3),cast.root,N("M07-2","slowFraction",.35f));if(slowed&&Gear("DES_LM42")&&CombatEffects.Has(enemy,StatusKind.Freeze))Buff("DES_LM42:freeze:"+enemy.id,3,root:cast.root);}
                }
                if(hits>0)Resource(D4BaseValue(cast.id,1,cast),cast.id,cast.root);
                if(cast.mod==3){CS.effects.RemoveAll(e=>e.kind=="d4conduit");var rod=D4MageEffect(cast,":conduit","d4conduit",D4ModValue(cast.id,cast));rod.radius=4;}
                return true;
            }
            if(cast.id=="M08")
            {
                CS.effects.RemoveAll(e=>e.kind=="d4firewall");var wall=D4MageEffect(cast,":wall","d4firewall",N(cast.id,"duration",cast:cast),D4BaseValue(cast.id,0,cast)/100,1);
                wall.position=cast.mod==1?cast.origin:cast.aim;wall.radius=cast.mod==1?D4ModValue(cast.id,cast):N("M08","length",5)*.5f;wall.width=N("M08","width",1)*cast.area;wall.radius*=cast.area;return true;
            }
            if(cast.id=="M11")
            {
                int count=cast.mod==2?3:1,cap=cast.mod==2?3:1;float duration=cast.mod==2?D4ModValue(cast.id,cast):N("M11","duration",6);
                for(int n=0;n<count;n++)
                {
                    var old=CS.effects.Where(e=>e.kind=="d4orb").OrderBy(e=>e.created).ThenBy(e=>e.id).ToArray();if(old.Length>=cap)CS.effects.Remove(old[0]);
                    var orb=D4MageEffect(cast,":orb","d4orb",duration,cast.mod==1?D4ModValue(cast.id,cast)/100:D4BaseValue(cast.id,0,cast)/100,1);orb.position=cast.aim+(count==1?Vector2.zero:new Vector2(n-1,0));orb.radius=N("M11","radius",3);
                }
                return true;
            }
            if(cast.id=="M17")
            {var collapse=D4MageEffect(cast,":collapse","d4collapse",N(cast.id,"duration",3),period:N(cast.id,"period",1));collapse.next=State.time;collapse.limit=3;collapse.radius=N(cast.id,"radius",4);return true;}
            if(cast.id=="M18")
            {
                Barrier("M18",D4BaseValue(cast.id,0,cast)/100,N(cast.id,"duration",8),cast.root);
                var sage=Buff("M18",N(cast.id,"duration",8),D4BaseValue(cast.id,1,cast)/100,cast.root);sage.source=cast.id;sage.mod=cast.mod;sage.modRank=cast.modRank;sage.snapshot=cast.snapshot;sage.limit=(int)N(cast.id,"cycleLimit",2);return true;
            }
            return false;
        }
        bool ResolveD4MageLegacy(HeroActionState action)
        {
            var cast=Cast(action.id);if(cast==null)return false;cast.snapshot=action.snapshot;
            switch(cast.id)
            {
                case "M01":SpawnD4Fireball(cast);return true;
                case "M02":SpawnD4Blizzard(cast,action.blizzardMode==BlizzardMode.Follow);return true;
                case "M03":ResolveD4Chain(cast,action.chainBonus);return true;
                case "M06":
                    var enemies=AreaTargets(cast.origin,3*cast.area,default,360);int hits=0;
                    foreach(var enemy in enemies){if(D4MageHit(cast,enemy,D4BaseValue(cast.id,0,cast)/100,2)!=null)hits++;if(cast.mod!=2){ApplyStatus(enemy,StatusKind.Freeze,cast.id,D4BaseValue(cast.id,1,cast),cast.root);}}
                    if(hits>0&&Stats.SetPieces("SMB")>=4){ItemEffects.chainCharge=6;ItemEffects.chainCharges=2;EffectEvent("SMB4","CHARGE",root:cast.root,value:2);}
                    if(cast.mod==1){var wave=D4MageEffect(cast,":wave","d4nova",1,D4BaseValue(cast.id,0,cast)/100*D4ModValue(cast.id,cast)/100);wave.position=cast.origin;wave.radius=3*cast.area;wave.next=wave.until;}
                    if(cast.mod==2){CS.effects.RemoveAll(e=>e.kind=="d4ward");var ward=D4MageEffect(cast,":ward","d4ward",N("M06-2","duration",6),-D4ModValue(cast.id,cast)/100);ward.position=cast.origin;ward.radius=N("M06-2","radius",3);Barrier("M06:ward",N("M06-2","barrierFraction",.1f),N("M06-2","barrierSeconds",4),cast.root);}
                    if(cast.mod==3)Barrier("M06:armor",Mathf.Min(.2f,hits*D4ModValue(cast.id,cast)/100),4,cast.root);
                    return true;
                case "M04":
                    if(cast.mod==1)foreach(var enemy in Nearby(cast.origin,3*cast.area,5))D4MageHit(cast,enemy,D4ModValue(cast.id,cast)/100,1,true,origin:cast.origin);
                    State.position=State.destination=action.destination;LandingPassives(action);if(Stats.Rune(RuneBonus.MobilityGuard)>0)ItemEffects.runeMobilityGuardUntil=State.time+2;
                    if(cast.mod==2)foreach(var enemy in Nearby(action.destination,3*cast.area,3))ApplyStatus(enemy,StatusKind.Stun,cast.id,D4ModValue(cast.id,cast),cast.root);
                    if(cast.mod==3){AddShield("M04:shield",Stats.hp*D4ModValue(cast.id,cast)/100,4,cast.root);var reaction=D4MageEffect(cast,":reaction","d4reaction",4);reaction.element=CS.lastElement;}
                    return true;
            }
            return false;
        }
        void SpawnD4Fireball(ClassSkillCast cast)
        {
            var fireball=D4MageEffect(cast,":fireball",cast.mod==1?"d4meteor":"d4fireball",cast.mod==1?1:2,D4BaseValue("M01",0,cast)/100);
            fireball.radius=RuneSkillRadius(12,2.5f)*cast.area*(cast.mod==1?1.5f:1);fireball.extra=cast.mod==3?10:Mathf.Min(10,Vector2.Distance(cast.origin,cast.aim));
            if(cast.mod==1){fireball.next=fireball.until;fireball.value*=D4ModValue("M01",cast);}else fireball.position=cast.origin;
        }
        void SpawnD4Blizzard(ClassSkillCast cast,bool followsTarget=false)
        {
            if(cast.mod==3)
            {var globe=D4MageEffect(cast,":return","d4frost",N("M02-3","flightSeconds",3),D4ModValue("M02",cast)/100);globe.position=globe.origin=cast.origin;globe.extra=8;globe.radius=N("M02-3","radius",.5f);return;}
            AddGround(cast.mod==1?State.position:cast.aim,cast.mod==1?D4ModValue("M02",cast):3,0,D4BaseValue("M02",1,cast)*(1+Stats.runeSkillDuration[13]/100),cast.snapshot.damage*.65f*cast.damageScale*(cast.secondary?cast.rankScale:1),false,13,definition:cast.secondary?"EXTRA:M02":"M02",root:cast.root,followsTarget:cast.mod!=1&&followsTarget&&Stats.specials.Contains("LM01"));
            var ground=State.effects.Last();ground.snapshot=cast.snapshot;ground.d4Modifier=cast.mod;ground.d4ModifierRank=cast.modRank;ground.d4Source=cast.secondary?"M18":"M02";
        }
        EnemyState[] D4ChainRoute(ClassSkillCast cast,EnemyState target,int bonus,Vector2 origin,EnemyState[] observed)
        {
            if(target==null||!Map.LineClear(origin,target.position))return Array.Empty<EnemyState>();
            int limit=cast.mod==1?(int)D4ModValue("M03",cast):4;limit+=bonus+(int)Stats.runeSkillTargets[14];
            if(cast.mod==2&&observed.Count(e=>!e.dead&&e.id!=target.id&&Vector2.Distance(target.position,e.position)<=4&&Map.LineClear(target.position,e.position))==0)return Enumerable.Repeat(target,(int)D4ModValue("M03",cast)+bonus+(int)Stats.runeSkillTargets[14]).ToArray();
            var hits=new List<EnemyState>();var visits=new Dictionary<int,int>();var current=target;bool usedConduit=false;
            for(int n=0;n<limit&&current!=null;n++)
            {hits.Add(current);visits[current.id]=visits.TryGetValue(current.id,out int count)?count+1:1;if(n+1>=limit)break;current=ChainRouteTarget(current.position,current.id,visits,observed,ref usedConduit,out bool relayed);if(relayed){if(!cast.secondary&&Gear("DES_LM43"))ChargeD4EquipmentOrb(CS.effects.First(e=>e.kind=="d4conduit"&&e.until>State.time).position,cast.root);if(++n+1>=limit)break;}}
            return hits.ToArray();
        }
        void ResolveD4Chain(ClassSkillCast cast,int bonus=0)
        {
            var target=Enemy(cast.target);if(!ChainFirstTargetValid(cast.origin,target))return;var hits=D4ChainRoute(cast,target,bonus,cast.origin,State.enemies.Where(Perceived).ToArray());var positions=hits.Select(e=>e.position).ToArray();
            var visits=new Dictionary<int,int>();for(int n=0;n<hits.Length;n++)
            {var enemy=hits[n];visits[enemy.id]=visits.TryGetValue(enemy.id,out int count)?count+1:1;float revisit=cast.mod==2?1:visits[enemy.id]>1?AspectGrowth.SnapshotValue(cast.snapshot,"LM04",.5f):1;D4MageHit(cast,enemy,D4BaseValue("M03",0,cast)/100*Mathf.Pow(.8f,n)*revisit,3);}
            if(cast.mod==3&&positions.Length>0)foreach(var enemy in hits.Distinct().Skip(1))Pull(enemy,positions[0],D4ModValue("M03",cast)+(Gear("DES_LM45")&&CS.effects.Any(e=>e.kind=="d4conduit"&&e.until>State.time)?1:0),cast.id,cast.root);
        }
        bool D4FirewallContains(ClassSkillEffect wall,Vector2 point)
        {
            var cast=wall.copiedCast?.root>0?wall.copiedCast:Cast(wall.root);if(cast==null)return false;var v=point-wall.position;
            if(cast.mod==1)return v.sqrMagnitude<=wall.radius*wall.radius;
            float along=Mathf.Abs(Vector2.Dot(v,wall.direction)),side=Mathf.Abs(wall.direction.x*v.y-wall.direction.y*v.x);
            return along<=wall.radius&&side<=wall.width*.5f||cast.mod==3&&side<=wall.radius&&along<=wall.width*.5f;
        }
        bool TickD4MageEffect(ClassSkillEffect effect,ClassSkillCast original)
        {
            var cast=effect.copiedCast?.root>0?effect.copiedCast:original;if(cast==null)return false;
            if(effect.kind=="d4conduit"||effect.kind=="d4ward"||effect.kind=="d4reaction")return true;
            if(effect.kind=="d4fireball"||effect.kind=="d4frost"||effect.kind=="d4split"){TickD4MageProjectile(effect,cast);return true;}
            if(effect.kind=="d4meteor"||effect.kind=="d4nova")
            {if(!effect.consumed&&State.time+.00001f>=effect.next){effect.consumed=true;effect.count++;foreach(var enemy in Nearby(effect.position,effect.radius,5))D4MageHit(cast,enemy,effect.value,effect.kind=="d4meteor"?1:2,effect.kind=="d4nova");Visual?.Invoke(effect.position,effect.position,effect.kind=="d4meteor"?12:17,effect.radius);}return true;}
            if(effect.kind=="d4trail")
            {while(effect.next<=State.time+.00001f&&effect.next<=effect.until+.00001f){effect.next+=effect.period;foreach(var enemy in Line(effect.origin,effect.position,Vector2.Distance(effect.origin,effect.position),.5f,5))D4MageHit(cast,enemy,effect.value,1,true,true,effect.origin);}return true;}
            if(effect.kind=="d4orb"&&cast.mod==1)effect.position=Map.MoveDirect(effect.position,effect.position+effect.direction*classDelta,classDelta,.1f);
            if(effect.kind=="d4firewall"&&cast.mod==2){float distance=D4ModValue("M08",cast)*classDelta;effect.position=Map.MoveDirect(effect.position,effect.position+effect.direction*distance,distance,.1f);}
            if(effect.kind!="d4orb"&&effect.kind!="d4firewall"&&effect.kind!="d4collapse")return false;
            while(effect.next<=State.time+.00001f&&effect.next<=effect.until+.00001f&&(effect.limit==0||effect.count<effect.limit))
            {
                effect.next+=effect.period;effect.count++;
                if(effect.kind=="d4orb"&&cast.mod==3)continue;
                if(effect.kind=="d4firewall")foreach(var enemy in State.enemies.Where(e=>!e.dead&&D4FirewallContains(effect,e.position)&&Map.LineClear(effect.position,e.position)).OrderBy(e=>e.id).Take(5))D4MageHit(cast,enemy,effect.value,1,periodic:true);
                else if(effect.kind=="d4orb")foreach(var enemy in Nearby(effect.position,effect.radius,3))D4MageHit(cast,enemy,effect.value,3,periodic:true);
                else
                {
                    if(cast.mod==2)
                    {float range=D4ModValue("M17",cast);var candidates=State.enemies.Where(e=>Perceived(e)&&Visible(e,State.position,range)).Select(e=>e.position).ToArray();effect.position=candidates.OrderByDescending(p=>State.enemies.Count(e=>Perceived(e)&&Visible(e,p,effect.radius))).ThenBy(p=>(p-effect.position).sqrMagnitude).DefaultIfEmpty(effect.position).First();}
                    int element=cast.mod==1?cast.element:effect.count;float power=effect.count==1?D4BaseValue("M17",0,cast)/100:N("M17","secondary",1.5f)*cast.rankScale;
                    if(cast.mod==1)power*=1+D4ModValue("M17",cast)/100;
                    foreach(var enemy in Nearby(effect.position,effect.radius,8)){D4MageHit(cast,enemy,power,element);if(element==2)ApplyStatus(enemy,StatusKind.Freeze,"M17",N("M17","freezeSeconds",2),cast.root);}
                }
            }
            if(effect.kind=="d4orb"&&State.time+.00001f>=effect.until&&!effect.consumed)
            {effect.consumed=true;float power=(cast.mod==3?D4ModValue("M11",cast):D4BaseValue("M11",1,cast))/100+N("M11","chargeCoefficient",.3f)*cast.rankScale*effect.extra;foreach(var enemy in Nearby(effect.position,effect.radius,5))D4MageHit(cast,enemy,power,3,true);}
            return true;
        }
        void TickD4MageProjectile(ClassSkillEffect effect,ClassSkillCast cast)
        {
            float speed=effect.kind=="d4frost"?N("M02-3","speed",8):14;var from=effect.position;float distance=Mathf.Min(effect.extra,speed*classDelta);var to=ClipProjectile(from,from+effect.direction*distance,.25f,out bool wall);
            if(effect.kind=="d4fireball"&&cast.mod==3)
            {
                foreach(var enemy in Line(from,to,Vector2.Distance(from,to),.5f,5).Where(e=>!effect.targets.Contains(e.id))){effect.targets.Add(enemy.id);D4MageHit(cast,enemy,effect.value,1);}
                if(Vector2.Distance(from,to)>.01f){var trail=D4MageEffect(cast,":trail","d4trail",D4ModValue("M01",cast),N("M08","coefficient",.6f),.5f);trail.origin=from;trail.position=to;trail.value*=.5f;}
            }
            else if(effect.kind=="d4frost"||effect.kind=="d4split")
            {
                foreach(var enemy in Line(from,to,Vector2.Distance(from,to),effect.kind=="d4frost"?N("M02-3","width",1):.5f,5).Where(e=>!effect.targets.Contains(e.id)).Take(Math.Max(0,5-effect.targets.Count)))
                {bool slowed=CombatEffects.Has(enemy,StatusKind.Slow);effect.targets.Add(enemy.id);D4MageHit(cast,enemy,effect.value,effect.kind=="d4frost"?2:1,effect.kind=="d4split");if(effect.kind=="d4frost")ApplyStatus(enemy,effect.leg==1&&slowed?StatusKind.Freeze:StatusKind.Slow,cast.secondary?"M18":"M02",effect.leg==1&&slowed?N("M02-3","freezeSeconds",.75f):N("M02-3","slowSeconds",3),cast.root,N("M02-3","slowFraction",.35f));}
            }
            else
            {
                var first=State.enemies.Where(e=>!e.dead).Select(e=>(enemy:e,entry:Entry(from,to,e.position,.65f))).Where(e=>!float.IsInfinity(e.entry)).OrderBy(e=>e.entry).ThenBy(e=>e.enemy.id).FirstOrDefault();
                if(first.enemy!=null){to=Vector2.Lerp(from,to,first.entry);wall=true;}
            }
            effect.extra-=Vector2.Distance(from,to);effect.position=to;if(!wall&&effect.extra>.001f)return;
            if(effect.kind=="d4frost"&&effect.leg==0){effect.leg=1;effect.targets.Clear();effect.direction=(cast.origin-to).normalized;effect.extra=Vector2.Distance(to,cast.origin);return;}
            if(effect.kind=="d4frost"&&!wall&&Vector2.Distance(State.position,cast.origin)<=N("M02-3","pickupRadius",1))Resource(N("M02-3","resourceGain",10)*cast.rankScale,cast.id,cast.root);
            if(effect.kind=="d4fireball"&&cast.mod!=3)
            {
                foreach(var enemy in Nearby(to,effect.radius,5))D4MageHit(cast,enemy,effect.value,1,origin:to);
                if(cast.mod==2)for(int n=0;n<3;n++){var split=D4MageEffect(cast,":split","d4split",1,D4ModValue("M01",cast)/100);split.position=split.origin=to;split.direction=RotateShot(Vector2.up,n*120);split.extra=3;}
            }
            CS.effects.Remove(effect);
        }
        void AfterD4MageCast(ClassSkillCast cast)
        {
            if(Normal(cast.id)&&cast.element>=1&&cast.element<=3)CS.lastElement=cast.element;
            if(cast.id=="M03")
            {var orb=CS.effects.Where(e=>e.kind=="d4orb").OrderBy(e=>e.created).ThenBy(e=>e.id).FirstOrDefault();if(orb!=null)orb.extra=Mathf.Min(3,orb.extra+1);}
            var sage=Fx("M18");var parent=sage==null?null:Cast(sage.root);if(parent?.mod!=1||!new[]{"M01","M02","M03"}.Contains(cast.id)||cast.secondary)return;
            string id=Loadout.order.FirstOrDefault(s=>new[]{"M01","M02","M03"}.Contains(s)&&Loadout.Equipped(s)&&ClassSkills.ElementOf(s)!=cast.element);if(id==null)return;
            var copy=new ClassSkillCast{id=id,root=parent.root,mod=Loadout.Modifier(id),modRank=Loadout.ModifierRank(id),baseRank=D4BaseRank(id),rankScale=RankScale(id),element=ClassSkills.ElementOf(id),origin=State.position,aim=cast.aim,target=cast.target,snapshot=cast.snapshot,secondary=true,damageScale=D4ModValue("M18",parent)/100,area=cast.area};
            if(id=="M01")SpawnD4Fireball(copy);else if(id=="M02")SpawnD4Blizzard(copy);else ResolveD4Chain(copy);
        }
        float D4MageAvatar(int element)
        {var sage=Fx("M18");var cast=sage==null?null:Cast(sage.root);return cast?.mod==2&&CS.lastElement==element?D4ModValue("M18",cast)/100:0;}
        bool ReactD4MageShield(ShieldEffect shield,float absorbed)
        {
            if(shield.definitionId!="M04:shield"||absorbed<=0)return false;var reaction=CS.effects.FirstOrDefault(e=>e.kind=="d4reaction"&&e.root==shield.rootCastId);if(reaction==null||reaction.consumed)return true;
            reaction.consumed=true;var cast=Cast(reaction.root);if(cast==null)return true;
            if(reaction.element==3)Resource(N("M04-3","reactionResource",8)*cast.rankScale,"M04",cast.root);
            else foreach(var enemy in Nearby(State.position,N("M04-3","reactionRadius",3),5))if(reaction.element==1)Dot("M04:burn",enemy,N("M04-3","reactionBurn",.6f),1,3,cast,modifierValue:true);else if(reaction.element==2)ApplyStatus(enemy,StatusKind.Root,"M04",N("M04-3","reactionRoot",1),cast.root);
            return true;
        }
        bool D4MageFrozen(EnemyState enemy)=>CombatEffects.Has(enemy,StatusKind.Freeze)||enemy.boss&&enemy.bossControl.staggered>0;
        void RecordD4MageDamage(EnemyState enemy,DamageEvent damage,DamageSnapshot snapshot)
        {
            if(damage.definitionId.StartsWith("EXTRA:MP2",StringComparison.Ordinal))return;
            if(Passive("MP20")&&D4MageFrozen(enemy))enemy.d4Shatter+=damage.hpLoss;
            if(enemy.dead){enemy.d4Shatter=0;enemy.d4Elements=0;enemy.d4ElementUntil=0;return;}
            if(!Passive("MP21")||enemy.dead||damage.element<1||damage.element>3)return;
            if(enemy.d4ElementUntil<=State.time)enemy.d4Elements=0;enemy.d4Elements|=1<<(damage.element-1);enemy.d4ElementUntil=State.time+4;
            if(enemy.d4Elements!=7)return;enemy.d4Elements=0;enemy.d4ElementUntil=0;
            foreach(var other in Nearby(enemy.position,2.5f,5))ApplyOutgoing(other,snapshot.damage*PV("MP21"),0,1,1,false,0,snapshot,"EXTRA:MP21",damage.rootCastId,0,DamageKind.Set,attackResolved:true);
        }
        void ReleaseD4Shatter(EnemyState enemy)
        {
            if(enemy.d4Shatter<=0||D4MageFrozen(enemy))return;float amount=enemy.d4Shatter;enemy.d4Shatter=0;if(!Passive("MP20")||enemy.dead)return;
            foreach(var other in Nearby(enemy.position,2.5f,5))ApplyOutgoing(other,amount*PV("MP20"),0,1,1,false,2,CaptureDamage(),"EXTRA:MP20",0,0,DamageKind.Set,attackResolved:true);
        }
    }
}
