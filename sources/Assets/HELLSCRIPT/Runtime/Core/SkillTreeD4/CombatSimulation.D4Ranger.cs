using System;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    public sealed partial class CombatSimulation
    {
        float D4RangerNumber(string id,string key,float original,ClassSkillCast cast)
        {
            if(id=="A07"&&key=="resourceCost")return 0;
            return original;
        }
        float D4RangerCost(string id,float original)
            =>id=="A01"||id=="A02"?D4BaseValue(id,1):original;
        ClassSkillEffect D4RangerEffect(ClassSkillCast cast,string suffix,string kind,float duration,float value=0)
        {
            var effect=Buff(cast.id+suffix,duration,value,cast.root,kind);effect.source=cast.id;effect.mod=cast.mod;effect.modRank=cast.modRank;effect.snapshot=cast.snapshot;return effect;
        }
        void CaptureD4RangerPassives(ClassSkillCast cast)
        {
            if((cast.id=="A01"||cast.id=="A02")&&Fx("AP20:charge") is ClassSkillEffect precision&&!precision.consumed)
            {precision.consumed=true;cast.forceCrit=true;cast.critDamageBonus+=precision.value;}
            if(cast.cost>0&&Fx("A05:preparation") is ClassSkillEffect preparation&&!preparation.consumed)preparation.consumed=true;
            if(cast.cost>0&&ItemEffects.ap05Ready)
            {ItemEffects.ap05Ready=false;ItemEffects.ap05Movement=0;ItemEffects.ap05Cooldown=4;}
        }
        void PrepareD4RangerAction(HeroActionState action)
        {
            var cast=Cast(action.id);if(cast?.id!="A04"||cast.mod!=3)return;
            var target=Enemy(cast.target);if(target==null)return;
            var facing=target.brain.facing.sqrMagnitude>.0001f?target.brain.facing.normalized:(cast.origin-target.position).normalized;
            var point=target.position-facing*.95f;
            if(Map.CanLand(point)&&Map.LineClear(cast.origin,point)&&Vector2.Distance(cast.origin,point)<=SkillRange(9))action.destination=cast.destination=point;
            action.prepare=action.travel;action.travel=0;action.phase=HeroActionPhase.Preparing;
        }
        bool ResolveD4RangerLegacy(HeroActionState action)
        {
            var cast=Cast(action.id);if(cast==null)return false;cast.snapshot=action.snapshot;var target=Enemy(action.targetId);
            switch(cast.id)
            {
                case "A01":LaunchD4Pierce(action,cast);return true;
                case "A02":LaunchD4Multi(action,cast);return true;
                case "A03":
                    if(cast.mod==3)
                    {
                        float duration=D4BaseValue("A03",1,cast);var turret=D4RangerEffect(cast,":turret","turret",duration,.35f);turret.position=action.aim;turret.radius=N("A03-3","radius",9);turret.limit=(int)D4ModValue("A03",cast);turret.period=duration/turret.limit;turret.next=State.time+turret.period;
                    }
                    else CreateTrap(action,action.aim,D4BaseValue("A03",1,cast));
                    return true;
                case "A04":
                    State.position=State.destination=action.destination;LandingPassives(action);
                    if(cast.mod==1)
                    {var decoy=D4RangerEffect(cast,":decoy","d4decoy",N("A04-1","duration",6));decoy.position=action.origin;decoy.health=Stats.hp*N("A04-1","summonHealthFraction",.1f);decoy.next=State.time+D4ModValue("A04",cast)+(Gear("DES_LA43")?2:0);decoy.radius=N("A04-1","radius",2.5f);decoy.value=N("A04-1","coefficient",.8f);}
                    if(cast.mod==2)D4RangerEffect(cast,":smoke","buff",3,-D4ModValue("A04",cast)/100);
                    if(cast.mod==3)D4RangerEffect(cast,":critical","charge",6,D4ModValue("A04",cast)/100);
                    else RememberRetreatLanding(action);
                    if(Gear("LA03")&&Loadout.Equipped("A03")&&Loadout.Modifier("A03")==0)CreateTrap(new HeroActionState{id=cast.root,skill=9,snapshot=cast.snapshot},cast.origin,3);
                    if(Stats.SetPieces("SAB")>=4){ItemEffects.pierceCharge=6;EffectEvent("SAB4","CHARGE",root:cast.root,value:6);}
                    if(Stats.Rune(RuneBonus.MobilityGuard)>0)ItemEffects.runeMobilityGuardUntil=State.time+2;
                    return true;
                case "A05":
                    if(!Visible(target,cast.origin,10))return true;
                    ApplyStatus(target,StatusKind.Mark,"A05",10,cast.root,D4BaseValue("A05",0,cast)/100);
                    var mark=target.statuses.Single(s=>s.definitionId=="A05"&&s.rootCastId==cast.root);mark.mod=cast.mod;mark.modRank=cast.modRank;mark.chainRemaining=cast.mod==3?(int)D4ModValue("A05",cast):0;
                    if(cast.mod==1)
                    {
                        var aim=D4RangerEffect(cast,":aim","d4aim",N("A05-1","prepare",.9f),D4ModValue("A05",cast)/100);aim.target=target.id;aim.next=aim.until;cast.released=true;cast.finishAt=aim.until;CS.action=cast;
                    }
                    return true;
                case "A06":
                    State.shadowCharges=3;State.shadowTime=8;State.shadowFraction=D4BaseValue("A06",0,cast)/100;CS.imbueCastRoot=cast.root;D4RangerEffect(cast,":imbue","buff",8,State.shadowFraction);return true;
            }
            return false;
        }
        bool ResolveD4RangerNative(ClassSkillCast cast)
        {
            if(cast.id=="A07")
            {
                int arrows=cast.mod==1?5:cast.mod==2?(int)D4ModValue("A07",cast):3;var targets=cast.mod==2?Nearby(cast.origin,10,(int)D4ModValue("A07",cast)):new[]{Enemy(cast.target)};
                for(int n=0;n<arrows;n++)
                {
                    var target=targets.Length>0?targets[n%targets.Length]:null;if(target==null)continue;
                    float power=cast.mod==1?D4ModValue("A07",cast)/100:D4BaseValue("A07",0,cast)/100;
                    var arrow=D4Arrow(cast,cast.origin,target.position,power,0,false,n*N("A07","period",.15f));
                    if(cast.mod==3&&n==2){arrow.forceCrit=true;arrow.critDamageBonus=D4ModValue("A07",cast)/100;arrow.limit=5;}
                }
                return true;
            }
            if(cast.id=="A14"){D4Arrow(cast,cast.origin,cast.aim,D4BaseValue("A14",0,cast)/100,0);return true;}
            if(cast.id=="A17")
            {
                var rain=D4RangerEffect(cast,"","d4rain",cast.mod==1?4:N("A17","duration",2.5f),D4BaseValue("A17",0,cast)/100*(cast.mod==2?D4ModValue("A17",cast):1));
                rain.position=cast.aim;rain.radius=N("A17","radius",4)*(cast.mod==2?.5f:1);rain.limit=cast.mod==1?(int)D4ModValue("A17",cast):cast.mod==2?10:6;rain.period=cast.mod==2?.25f:.5f;rain.next=State.time;return true;
            }
            if(cast.id=="A18")
            {
                var march=D4RangerEffect(cast,cast.mod==1?":clone":"",cast.mod==1?"clone":"d4march",N("A18","duration",8),cast.mod==1?D4ModValue("A18",cast)/100:D4BaseValue("A18",0,cast)/100);
                march.position=State.position;march.limit=(int)N("A18","procLimit",8);return true;
            }
            return false;
        }
        ClassSkillEffect D4Arrow(ClassSkillCast cast,Vector2 origin,Vector2 target,float coefficient,int element,bool secondary=false,float delay=0,int generation=0)
        {
            Vector2 direction=(target-origin).normalized;if(direction==Vector2.zero)direction=Vector2.up;
            var arrow=new ClassSkillEffect{id=cast.id+":arrow:"+State.nextId++,source=cast.id,mod=cast.mod,modRank=cast.modRank,kind="d4arrow",root=cast.root,position=origin,origin=origin,direction=direction,
                value=coefficient,element=element,next=State.time+delay,until=State.time+delay+2,extra=Mathf.Max(.1f,Vector2.Distance(origin,target)),limit=1,previous=secondary?"secondary":"direct",snapshot=cast.snapshot,generation=generation};
            CS.effects.Add(arrow);return arrow;
        }
        void LaunchD4Pierce(HeroActionState action,ClassSkillCast cast)
        {
            bool imbued=ConsumeShadow();var directions=cast.mod==3?new[]{-30f,0,30f}:new[]{0f};
            foreach(float degrees in directions)
            {
                var arrow=Shot(action,RotateShot((action.aim-action.origin).normalized,degrees),10,18,.3f,1.8f*(cast.mod==3?D4ModValue("A01",cast)/100:1),0,5+(int)Stats.runeSkillTargets[6]);
                arrow.shadow=imbued;arrow.imbueRoot=CS.imbueCastRoot;if(cast.mod==1)arrow.bounces=(int)D4ModValue("A01",cast);
            }
            if(action.pierceBonus){var echo=Shot(action,action.aim-action.origin,10,18,.3f,1.2f,0,5);echo.extra=true;echo.delay=.3f;}
        }
        float[] D4MultiAngles(ClassSkillCast cast=null)
        {
            int mod=cast?.mod??Loadout.Modifier("A02");bool focused=mod==3||Stats.specials.Contains("LA02");float half=mod==3?7.5f:MultiShotHalfAngle(focused);
            float barrageHalf=focused?half:60;return mod==1?Enumerable.Range(0,7).Select(n=>-barrageHalf+barrageHalf*n/3).ToArray():new[]{-half,0,half};
        }
        void LaunchD4Multi(HeroActionState action,ClassSkillCast cast)
        {
            bool imbued=ConsumeShadow();State.projectileGroups.Add(new ProjectileGroup{actionId=action.id,maxPerVictim=cast.mod==3||Stats.specials.Contains("LA02")?3:1});
            foreach(float degrees in D4MultiAngles(cast))
            {
                var arrow=Shot(action,RotateShot((action.aim-action.origin).normalized,degrees),RuneSkillRadius(7,8),18,.2f,cast.mod==1?D4ModValue("A02",cast)/100:.8f,0);arrow.modifierValue=cast.mod==1;arrow.shadow=imbued;arrow.imbueRoot=CS.imbueCastRoot;
            }
        }
        bool TickD4RangerEffect(ClassSkillEffect effect,ClassSkillCast cast)
        {
            if(effect.kind=="d4arrow"){TickD4Arrow(effect,cast);return true;}
            if(effect.kind=="d4decoy")
            {if(State.time+.00001f>=effect.until&&!effect.consumed){effect.consumed=true;foreach(var enemy in Nearby(effect.position,effect.radius))SkillHit(cast,enemy,effect.value,0,true,"A04",modifierValue:true);}return true;}
            if(effect.kind=="d4afterimage")
            {if(State.time+.00001f>=effect.until)FadeD4Afterimage(effect);return true;}
            if(effect.kind=="clone"){effect.position=State.position;return true;}
            if(effect.kind=="d4aim")
            {
                if(CS.action!=cast){CS.effects.Remove(effect);return true;}
                if(effect.next<=State.time+.00001f&&!effect.consumed)
                {effect.consumed=true;SkillHit(cast,Enemy(effect.target),effect.value,0,modifierValue:true);}
                return true;
            }
            if(effect.kind!="turret"&&effect.kind!="d4rain"&&effect.kind!="d4march")return false;
            if(effect.kind=="d4march"){LeaveD4Afterimage(effect);return true;}
            if(effect.kind=="d4rain"&&effect.mod==1)effect.position=State.position;
            while(effect.next<=State.time+.00001f&&effect.next<=effect.until+.00001f&&effect.count<effect.limit)
            {
                effect.next+=effect.period;effect.count++;
                if(effect.kind=="turret")
                {var enemy=Nearby(effect.position,effect.radius).Where(Perceived).FirstOrDefault();if(enemy!=null)D4Arrow(cast,effect.position,enemy.position,effect.value,0,true);}
                else foreach(var enemy in Nearby(effect.position,effect.radius))Hit(enemy,effect.value,0,false,snapshot:effect.snapshot,canCrit:false,definition:"A17",root:effect.root,kind:DamageKind.Periodic);
            }
            return true;
        }
        void TickD4Arrow(ClassSkillEffect arrow,ClassSkillCast cast)
        {
            if(cast==null||arrow.next>State.time+.00001f)return;var from=arrow.position;float distance=Mathf.Min(arrow.extra,20*classDelta);var to=ClipProjectile(from,from+arrow.direction*distance,.25f,out bool wall);
            var victims=State.enemies.Where(e=>!e.dead&&!arrow.targets.Contains(e.id)).Select(e=>(enemy:e,entry:Entry(from,to,e.position,.5f)))
                .Where(h=>!float.IsInfinity(h.entry)).OrderBy(h=>h.entry).ThenBy(h=>h.enemy.id).Take(Math.Max(0,arrow.limit-arrow.count)).ToArray();
            bool returnNow=false;
            foreach(var victim in victims)
            {
                var enemy=victim.enemy;bool direct=arrow.previous=="direct";arrow.position=Vector2.Lerp(from,to,victim.entry);
                DamageEvent hit=direct?Hit(enemy,arrow.value,arrow.element,false,snapshot:arrow.snapshot,definition:cast.id,root:cast.root,projectile:true,origin:from,forceCritical:arrow.forceCrit,critDamageBonus:arrow.critDamageBonus):SkillHit(cast,enemy,arrow.value,arrow.element,true,cast.id,attackOrigin:from,modifierValue:true);
                if(cast.id=="A18")D4EquipmentUltimateHit(hit,cast.root);
                arrow.targets.Add(enemy.id);arrow.count++;
                if(hit?.hpLoss>0&&cast.id=="A07"&&Once("A07:resource",cast.root))Resource(D4BaseValue("A07",1,cast),"A07",cast.root);
                if(cast.id=="A14")
                {
                    if(direct)cast.receipts.Add("fork:"+enemy.id);
                    if(direct&&cast.mod==2)
                    {foreach(var other in Nearby(enemy.position,D4ModValue("A14",cast)).Where(e=>e.id!=enemy.id))SkillHit(cast,other,D4BaseValue("A14",1,cast)/100,0,true,"A14",modifierValue:true);}
                    else if(direct||cast.mod==1&&arrow.generation==1)
                    {
                        float power=D4BaseValue("A14",1,cast)/100*(direct?1:D4ModValue("A14",cast)/100);
                        foreach(var other in Nearby(enemy.position,N("A14","radius",3)).Where(e=>e.id!=enemy.id&&!cast.receipts.Contains("fork:"+e.id)).Take(direct&&Gear("DES_LA44")&&HasDot("A06:poison",enemy)?3:2))
                        {cast.receipts.Add("fork:"+other.id);var split=D4Arrow(cast,enemy.position,other.position,power,0,true,generation:arrow.generation+1);split.targets.Add(enemy.id);}
                    }
                    if(!direct&&cast.mod==3&&arrow.leg==0){returnNow=true;arrow.leg=1;arrow.targets.Clear();arrow.targets.Add(enemy.id);arrow.count=0;arrow.value=D4ModValue("A14",cast)/100;arrow.direction=(arrow.origin-arrow.position).normalized;arrow.extra=Vector2.Distance(arrow.origin,arrow.position);break;}
                }
                if(arrow.count>=arrow.limit){CS.effects.Remove(arrow);return;}
            }
            if(returnNow)return;
            arrow.extra-=Vector2.Distance(from,to);arrow.position=to;if(wall||arrow.extra<=.001f)CS.effects.Remove(arrow);
        }
        void LeaveD4Afterimage(ClassSkillEffect march)
        {
            if(march.count>=march.limit||Vector2.Distance(State.position,march.position)<N("A18","trailMeters",1.5f))return;
            var echoes=CS.effects.Where(e=>e.kind=="d4afterimage").OrderBy(e=>e.created).ThenBy(e=>e.id).ToArray();
            if(echoes.Length>=2){FadeD4Afterimage(echoes[0]);CS.effects.Remove(echoes[0]);}
            CS.effects.Add(new ClassSkillEffect{id="A18:echo:"+State.nextId++,source="A18",mod=march.mod,modRank=march.modRank,root=march.root,kind="d4afterimage",position=march.position,created=State.time,until=march.until,value=march.value,snapshot=march.snapshot});
            march.position=State.position;march.count++;
        }
        void FadeD4Afterimage(ClassSkillEffect echo)
        {
            if(echo.consumed||echo.mod!=2)return;echo.consumed=true;var cast=Cast(echo.root);if(cast==null)return;
            foreach(var enemy in Nearby(echo.position,2.5f))SkillHit(cast,enemy,D4ModValue("A18",cast)/100,5,true,"A18",modifierValue:true);
        }
        void AfterD4RangerHit(EnemyState enemy,DamageEvent hit,ClassSkillCast cast,int flags)
        {
            if(!DirectHit(hit.kind))return;
            if(hit.critical&&Passive("AP20"))
            {var precision=Counter("AP20");if(++precision.count>=3){precision.count=0;Buff("AP20:charge",30,PV("AP20"),hit.rootCastId,"charge");}}
            if(Fx("A18:clone") is ClassSkillEffect clone&&!enemy.dead)
                D4EquipmentUltimateHit(ApplyOutgoing(enemy,hit.attackBeforeDefense*clone.value,0,1,1,false,hit.element,clone.snapshot,"EXTRA:A18:clone",clone.root,0,DamageKind.Set,attackResolved:true),clone.root);
            var echo=CS.effects.Where(e=>e.kind=="d4afterimage"&&e.until>State.time&&Visible(enemy,e.position,10)).OrderBy(e=>e.created).FirstOrDefault();
            if(echo!=null&&Proc("A18",1,hit.rootCastId))
            {FadeD4Afterimage(echo);CS.effects.Remove(echo);var ultimate=Cast(echo.root);if(ultimate!=null)D4Arrow(ultimate,echo.position,enemy.position,echo.value,5,true);}
        }
        void D4RangerDeath(EnemyState enemy)
        {
            var mark=enemy.statuses.FirstOrDefault(s=>s.definitionId=="A05"&&s.casterId==State.heroId&&s.remaining>0);if(mark==null)return;
            var cast=Cast(mark.rootCastId);if(cast==null)return;
            if(mark.mod==2)
            {Resource(N("A05-2","resourceGain",25),"A05",cast.root);if(Gear("DES_LA45"))Buff("DES_LA45",6,root:cast.root,kind:"charge");D4RangerEffect(cast,":preparation","charge",N("A05-2","duration",6),-D4ModValue("A05",cast)/100);}
            if(mark.mod==3&&mark.chainRemaining>0)
            {
                var next=Nearby(enemy.position,10).Where(e=>e.id!=enemy.id).FirstOrDefault();if(next==null)return;
                ApplyStatus(next,StatusKind.Mark,"A05",mark.remaining,mark.rootCastId,mark.strength);var moved=next.statuses.Single(s=>s.definitionId=="A05"&&s.rootCastId==mark.rootCastId);moved.mod=mark.mod;moved.modRank=mark.modRank;moved.chainRemaining=mark.chainRemaining-1;
            }
        }
        void D4Imbuement(EnemyState enemy,float coefficient,DamageSnapshot snapshot,int root,int instance,bool critical,float critMultiplier,int imbueRoot)
        {
            var cast=Cast(imbueRoot>0?imbueRoot:CS.imbueCastRoot);if(cast?.id!="A06")return;
            int element=cast.mod==1?5:cast.mod==2?4:cast.mod==3?2:0;
            bool ownPoison=CombatEffects.OwnTrapPoison(enemy,State.heroId)||HasDot("A06:poison",enemy);
            if(!enemy.dead)SpreadImbuement(enemy,ApplyOutgoing(enemy,snapshot.damage*coefficient*D4BaseValue("A06",0,cast)/100,AttackBonus(enemy,element,snapshot,0,DamageKind.Shadow),1,critMultiplier,critical,element,snapshot,"A06",root,instance,DamageKind.Shadow),snapshot,root,instance,ownPoison);
            if(cast.mod==1&&enemy.dead&&Once("A06:blast",root,enemy.id))foreach(var other in Nearby(enemy.position,3))SkillHit(cast,other,D4ModValue("A06",cast)/100,5,true,"A06",modifierValue:true);
            if(cast.mod==2&&!enemy.dead)Dot("A06:poison",enemy,D4ModValue("A06",cast)/100,4,4,cast,modifierValue:true);
            if(cast.mod==3&&!enemy.dead)
            {ApplyStatus(enemy,StatusKind.Slow,"A06",3,cast.root,D4ModValue("A06",cast)/100);if(++enemy.d4ColdStacks>=3){enemy.d4ColdStacks=0;ApplyStatus(enemy,StatusKind.Freeze,"A06",N("A03-2","freezeSeconds",.75f),cast.root);}}
        }
        float D4ProjectileBonus(CombatProjectile projectile,ProjectileVictim victim)
        {
            var cast=Cast(projectile.actionId);if(cast==null||projectile.extra)return 0;
            return cast.id=="A01"&&cast.mod==2?projectile.hitIds.Count*D4ModValue("A01",cast)/100:
                cast.id=="A02"&&cast.mod==3?Math.Max(0,(victim?.hits??1)-1)*D4ModValue("A02",cast)/100:0;
        }
        void BounceD4Multi(CombatProjectile projectile,EnemyState victim)
        {
            var cast=Cast(projectile.actionId);if(projectile.extra||projectile.bounced||cast?.id!="A02"||cast.mod!=2)return;
            projectile.bounced=true;var target=Nearby(victim.position,3).FirstOrDefault(e=>e.id!=victim.id&&!projectile.hitIds.Contains(e.id));if(target==null)return;
            var action=new HeroActionState{id=cast.root,skill=7,origin=victim.position,snapshot=projectile.snapshot};
            var bounce=Shot(action,target.position-victim.position,Vector2.Distance(target.position,victim.position),18,.2f,D4BaseValue("A02",0,cast)/100*D4ModValue("A02",cast)/100,0);
            bounce.extra=true;bounce.definitionId="EXTRA:A02";bounce.hitIds.Add(victim.id);
        }
        bool ReflectD4Projectile(CombatProjectile projectile,Vector2 position)
        {
            if(projectile.bounces<=0)return false;Vector2 normal=Vector2.zero;
            foreach(var direction in new[]{Vector2.up,Vector2.down,Vector2.left,Vector2.right})
                if(Map.ProjectileClear(position,position+direction*.12f,projectile.radius))normal+=direction;
            if(normal.sqrMagnitude<.00001f)normal=-projectile.direction;else normal.Normalize();
            projectile.direction=Vector2.Reflect(projectile.direction,normal).normalized;projectile.bounces--;projectile.position=position+projectile.direction*.02f;
            return Map.ProjectileClear(position,projectile.position,projectile.radius);
        }
    }
}
