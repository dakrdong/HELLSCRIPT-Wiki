using System.Linq;
using UnityEngine;

namespace Hellscript
{
    public sealed partial class CombatSimulation
    {
        static string SkillId(int skill)=>ClassSkills.LegacyId(skill);
        void RecordDamage(DamageEvent damage)
        {
            CombatTelemetry.Damage(State.statistics,damage,damage.incoming?Attacker(damage.casterId):null);
            JournalDamage(damage);
            State.damageEvents.Add(damage);
            if(State.damageEvents.Count>2000)State.damageEvents.RemoveAt(0);
        }
        float TargetReduction(EnemyState enemy,int element,bool projectile=false,Vector2? origin=null)
        {
            float multiplier=1;
            if(EnemyCombat.Trait(enemy,3)&&projectile&&Vector2.Angle(enemy.brain.facing,(origin??State.position)-enemy.position)<=60)multiplier*=.5f;
            if(EnemyCombat.Trait(enemy,2)&&State.enemies.Any(e=>e.id==enemy.elitePartner&&!e.dead&&e!=enemy&&e.elite>=0&&Vector2.Distance(e.position,enemy.position)<=5))multiplier*=.8f;
            if(enemy.boss&&enemy.pattern==1&&State.enemies.Any(e=>BossCombat.OwnAdd(e,enemy)&&!e.dead))multiplier*=.8f;
            return Mathf.Min(.5f,1-multiplier);
        }
        // The mark is this project's vulnerable: Diablo IV adds a fixed share against a vulnerable
        // target and lets gear raise it, which is exactly what Stats.vulnerable holds.
        float AttackBonus(EnemyState enemy,int element,DamageSnapshot snap,float extra,DamageKind kind=DamageKind.Direct)
        {
            var mark=D4Combat&&Hero.heroClass==HeroClass.Ranger?enemy.statuses.FirstOrDefault(s=>s.definitionId=="A05"&&s.casterId==State.heroId&&s.remaining>0):null;
            float bonus=snap.elements[element]+snap.bonus+(HasAmplifyingMark(enemy)?(mark!=null?mark.strength+RuneSnapshotBonus(StatId.VulnerableDamage,snap)/100:(Stats.vulnerable-Stats.Bonus(StatId.VulnerableDamage)+RuneSnapshotBonus(StatId.VulnerableDamage,snap))/100):0)+extra+ConditionalBonus(enemy,kind,snap)+RuneDamageBonus(enemy,kind,snap);
            // Round the threshold to stored HP precision; Mono can otherwise retain extra intermediate precision.
            if(Hero.heroClass==HeroClass.Warrior)
            {if(snap.passives[0]&&(snap.crowdCaptured?snap.crowdQualified:CountNear(State.position,3)>=3))bonus+=SkillEffects.Passive(snap.ranks,HeroClass.Warrior,0);if(snap.passives[5]&&enemy.health<=(float)(enemy.maxHealth*.3f))bonus+=SkillEffects.Passive(snap.ranks,HeroClass.Warrior,5);}
            if(Hero.heroClass==HeroClass.Ranger&&snap.passives[0]&&Vector2.Distance(State.position,enemy.position)>=6)bonus+=SkillEffects.Passive(snap.ranks,HeroClass.Ranger,0);
            return bonus;
        }
        DamageEvent ApplyOutgoing(EnemyState enemy,float baseAttack,float additive,float independent,float critMultiplier,bool critical,int element,DamageSnapshot snapshot,string definition,int root,int instance,DamageKind kind,int triggerTarget=-1,bool projectile=false,Vector2? origin=null,bool attackResolved=false,int extraClassFlags=0)
        {
            // The phase-change roar is the boss's only invulnerable window.
            if(enemy==null||enemy.dead||BossCombat.Roaring(enemy,State.time)||DataIgnoresHit(enemy,definition,root,projectile,origin))return null;
            float defense=element==0?10+2*(State.stage-1):5+State.stage-1;
            var legendary=PrepareLegendaryHit(snapshot,definition,enemy);
            int classFlags=ClassHitFlags(enemy)|extraClassFlags;
            if(D4Combat&&(classFlags&2)!=0&&Passive("AP21")&&definition!="EXTRA:A18:clone")independent*=1+PV("AP21");
            if(!attackResolved){additive+=ClassDamageBonus(definition,enemy,kind,root,instance,element);independent*=1+Mathf.Min(1.5f,LegendaryDamageBonus(snapshot,definition,enemy)+ClassLegendaryBonus(definition,root));}
            // Data monsters' frontal guard, rear weakness and protective auras (exactly 1 for everything else).
            float taken=DataDamageTaken(enemy,origin??State.position,kind,projectile);if(taken!=1)independent*=taken;
            var numbers=DamageMath.Calculate(baseAttack,additive,independent,critMultiplier,defense,snapshot.level,element,TargetReduction(enemy,element,projectile,origin));
            var damage=new DamageEvent{id=State.nextDamageId++,rootCastId=root,effectInstanceId=instance,definitionId=definition,casterId=State.heroId,targetId=enemy.id,targetIsBoss=enemy.boss,triggerTargetId=triggerTarget,kind=kind,element=element,tick=EffectTick,time=State.time,
                baseAttack=baseAttack,additive=additive,independent=independent,critical=critical,criticalMultiplier=critMultiplier,attackBeforeDefense=numbers.beforeDefense,projectile=projectile,attackOrigin=origin??State.position,
                defenseReduction=numbers.defenseReduction,buffReduction=numbers.buffReduction,finalDamage=numbers.final,hpLoss=Mathf.Min(Mathf.Max(0,enemy.health),numbers.final)};
            RecordDamage(damage);Deal(enemy,numbers.final,critical);
            if(D4Combat&&Hero.heroClass==HeroClass.Mage)RecordD4MageDamage(enemy,damage,snapshot);
            if(numbers.final>0&&(!ClassSkillsActive||Native(definition)))RecordElementHit(element,snapshot,root);
            FinishLegendaryHit(legendary,enemy,damage,snapshot);
            AfterClassHit(enemy,damage,snapshot,classFlags);
            if(ClassSkillsActive)
            {
                var eventOrigin=damage.kind==DamageKind.Periodic?ClassSkillEventOrigin.Periodic:damage.kind==DamageKind.Set||damage.kind==DamageKind.Legendary||damage.kind==DamageKind.Shadow?
                    definition=="EXTRA:W17"?ClassSkillEventOrigin.Summon:
                    definition=="A06"||definition=="EXTRA:A18"?ClassSkillEventOrigin.Echo:definition.StartsWith("EXTRA:",System.StringComparison.Ordinal)&&ClassSkills.Find(definition.Substring(6))!=null?ClassSkillEventOrigin.Secondary:ClassSkillEventOrigin.Equipment:ClassSkillEventOrigin.Direct;
                QueueClassSkillEvent(new ClassSkillEvent(Cast(root)?.id??definition,"DAMAGE",definition,root,enemy.id,State.time,damage.hpLoss,enemy.position,eventOrigin));
            }
            return damage;
        }
        DamageEvent Hit(EnemyState enemy,float coefficient,int element,bool procs=true,float extraBonus=0,DamageSnapshot snapshot=null,bool canCrit=true,bool? shadowShot=null,string definition=null,int root=0,int instance=0,DamageKind kind=DamageKind.Direct,bool projectile=false,Vector2? origin=null,bool forceCritical=false,float critDamageBonus=0,bool modifierValue=false,int imbueRoot=0)
        {
            if(enemy==null||enemy.dead)return null;
            var snap=snapshot??CaptureDamage();definition??=SkillId(State.heroAction.skill);if(root==0)root=State.heroAction.id;
            int masterySkill=ClassSkills.LegacyIndex(definition);
            if(masterySkill>=0&&masterySkill<18&&snap.runeSkillPower!=null&&snap.runeSkillPower.Length==18)coefficient*=1+(snap.runeSkillPower[masterySkill]-(D4Combat&&modifierValue?snap.d4BasePower?.Length==18?snap.d4BasePower[masterySkill]:D4LegacyRankPower(masterySkill,Cast(root)):0))/100;
            extraBonus+=RuneAreaBonus(snap,definition);
            float crit=snap.crit+ClassCritical(definition,enemy,kind)+LegendaryCriticalBonus(snap,definition,enemy)+(Hero.heroClass==HeroClass.Ranger&&snap.passives[5]&&enemy.health<=(float)(enemy.maxHealth*.3f)?SkillEffects.Passive(snap.ranks,HeroClass.Ranger,5):0);
            if(D4Combat&&Hero.heroClass==HeroClass.Ranger&&DirectHit(kind)&&Fx("A04:critical") is ClassSkillEffect step&&!step.consumed){step.consumed=true;forceCritical=true;critDamageBonus+=step.value;}
            bool critical=canCrit&&(RandomStream.Unit(ref State.rng)<Mathf.Min(.75f,crit)||forceCritical||Cast(root)?.forceCrit==true);
            if(!enemy.boss&&enemy.kind==6&&enemy.brain.rearWindow>0&&(kind==DamageKind.Direct||kind==DamageKind.Basic)&&Vector2.Angle(-enemy.brain.facing,(origin??State.position)-enemy.position)<=60)extraBonus+=.25f;
            bool overpowered=OverpowerRoll(kind);
            float bonus=AttackBonus(enemy,element,snap,extraBonus,kind)+(overpowered?Stats.overpower/100:0),criticalMultiplier=critical?snap.critDamage+critDamageBonus+(Cast(root)?.critDamageBonus??0):1;
            bool ownPoison=CombatEffects.OwnTrapPoison(enemy,State.heroId);
            var damage=ApplyOutgoing(enemy,snap.damage*coefficient,bonus,1,criticalMultiplier,critical,element,snap,definition,root,instance,kind,projectile:projectile,origin:origin);
            if(damage!=null)
            {
                if(overpowered)EffectEvent(definition,"OVERPOWER",instance,root,value:damage.finalDamage);
                StealLife(damage.hpLoss);StealMana(damage.hpLoss);LuckyHit(damage.hpLoss);
                EliteDirectHit(enemy,damage,critical,element,snap,root,instance,kind);
            }
            if(D4Combat&&procs&&Hero.heroClass==HeroClass.Ranger&&(shadowShot??State.shadowCharges>0)&&element==0)D4Imbuement(enemy,coefficient,snap,root,instance,critical,criticalMultiplier,imbueRoot);
            if(!D4Combat&&procs&&Hero.heroClass==HeroClass.Ranger&&(shadowShot??State.shadowCharges>0)&&element==0&&!enemy.dead)
            {
                // A06 inherits the direct shot's critical result but uses shadow bonuses and resistance.
                var shadow=ApplyOutgoing(enemy,snap.damage*coefficient*ShadowFraction,AttackBonus(enemy,5,snap,extraBonus,DamageKind.Shadow),1,criticalMultiplier,critical,5,snap,"A06",root,instance,DamageKind.Shadow,projectile:projectile,origin:origin);
                SpreadImbuement(enemy,shadow,snap,root,instance,ownPoison);
            }
            return damage;
        }
        void SpreadImbuement(EnemyState enemy,DamageEvent imbued,DamageSnapshot snapshot,int root,int instance,bool ownPoison)
        {
            if(imbued==null||!Stats.specials.Contains("LA04")||!ownPoison||State.procHits.Any(p=>p.rootCastId==root&&p.definitionId=="LA04"&&(!p.originScoped||p.targetId==enemy.id)))return;
            // Old receipts retain their root-wide restriction; current receipts identify the original target.
            State.procHits.Add(new ProcReceipt{rootCastId=root,targetId=enemy.id,definitionId="LA04",originScoped=true});
            foreach(var other in State.enemies.Where(e=>e!=enemy&&!e.dead&&Vector2.Distance(e.position,enemy.position)<=3&&Map.LineClear(enemy.position,e.position)).OrderBy(e=>(e.position-enemy.position).sqrMagnitude).ThenBy(e=>e.id).Take(2).ToArray())
                ApplyOutgoing(other,imbued.attackBeforeDefense,0,AspectGrowth.SnapshotValue(snapshot,"LA04",.5f),1,false,imbued.element,snapshot,"LA04",root,instance,DamageKind.Spread,enemy.id);
        }
        // Exposed weakness is limited by how much of the fight it covers, not by its size: a direct
        // critical on one target for a few seconds. Both knobs are named here because tuning moves them.
        const float ExposedSeconds=4,ExposedBonus=.1f;
        // v13 specifies 30% of the attack basis, without a second additive damage package.
        const float CascadeFraction=.3f;
        // The elite centre runes, resolved from the hit that just landed. A hit never grants the stack or
        // the exposure it was itself multiplied by, and the burst is raised outside Hit so that a kill it
        // causes cannot start another one.
        void EliteDirectHit(EnemyState enemy,DamageEvent damage,bool critical,int element,DamageSnapshot snap,int root,int instance,DamageKind kind)
        {
            if(!DirectHit(kind)||damage.finalDamage<=0)return;
            if(Stats.specials.Contains("ELITE_HEAT"))
            {
                ItemEffects.eliteHeatLast=State.time;
                if(State.time-ItemEffects.eliteHeatGained>=.5f-.00001f&&ItemEffects.eliteHeatStacks<5)
                {ItemEffects.eliteHeatStacks++;ItemEffects.eliteHeatGained=State.time;EffectEvent("ELITE_HEAT","STACK",instance,root,enemy.id,ItemEffects.eliteHeatStacks);}
            }
            if(critical&&Stats.specials.Contains("ELITE_WEAKNESS"))
            {enemy.eliteExposed=State.time+ExposedSeconds;EffectEvent("ELITE_WEAKNESS","EXPOSED",instance,root,enemy.id,ExposedSeconds);}
            if(enemy.dead&&Stats.specials.Contains("ELITE_CASCADE")&&ItemEffects.eliteCascadeCooldown<=.00001f)EliteCascade(enemy,element,snap,root,instance);
        }
        void EliteCascade(EnemyState killed,int element,DamageSnapshot snap,int root,int instance)
        {
            ItemEffects.eliteCascadeCooldown=1;
            EffectEvent("ELITE_CASCADE","BURST",instance,root,killed.id,2.5f);
            // ApplyOutgoing rather than Hit: the burst rolls no critical, no overpower and no lucky hit,
            // and cannot reach this hook again from a kill of its own.
            foreach(var other in State.enemies.Where(e=>e!=killed&&!e.dead&&Vector2.Distance(e.position,killed.position)<=2.5f).OrderBy(e=>e.id).ToArray())
                ApplyOutgoing(other,snap.damage*CascadeFraction,0,1,1,false,element,snap,"ELITE_CASCADE",root,instance,DamageKind.Legendary,killed.id);
        }
        void Hurt(float damage,int element=0,string caster="ENEMY",string definition="ENEMY_ATTACK",int root=0,int instance=0,DamageKind kind=DamageKind.Direct)
        {
            // A data monster's curse raises everything the hero takes while it lasts.
            if(State.heroCurseTime>0)damage*=1+State.heroCurse;
            var attacker=Attacker(caster);
            // Diablo IV settles dodge before anything else. A dodged hit lands nothing, so none of
            // the things a hit does — interrupting a chest, arming a proc, marking the hero as
            // recently damaged — happens either.
            if(DodgeIncoming())
            {
                CombatTelemetry.Support(State.statistics,"DODGE",SkillResultMetric.Protection,Mathf.Max(0,damage));
                EffectEvent(definition,"DODGED",instance,root);Visual?.Invoke(State.position,State.position,32,0);
                Log("DODGE","공격을 회피했습니다.");return;
            }
            if(kind!=DamageKind.Periodic&&Stats.perfectBlock>0&&SheetRoll()<Stats.perfectBlock)
            {CombatTelemetry.Support(State.statistics,"PERFECT_BLOCK",SkillResultMetric.Protection,Mathf.Max(0,damage));EffectEvent(definition,"PERFECT_BLOCK",instance,root);Log("BLOCK","완벽하게 방어했습니다.");return;}
            bool blocked=BlockIncoming(out bool skillBlock);
            var numbers=IncomingDamageNumbers(damage,element,IncomingReduction(attacker,element,blocked),kind==DamageKind.Periodic);
            RecordProtection(damage,element,attacker,skillBlock,numbers,kind==DamageKind.Periodic);
            if(blocked)EffectEvent(definition,"BLOCKED",instance,root,value:numbers.final);
            float before=State.health,absorbed=AbsorbDamage(numbers.final),floor=GuideHealthFloor(before);
            State.health=Mathf.Max(floor,State.health-(numbers.final-absorbed));
            float hpLoss=floor>0?Mathf.Max(0,before-State.health):Mathf.Min(Mathf.Max(0,before),Mathf.Max(0,numbers.final-absorbed));
            if(D4Combat&&Hero.heroClass==HeroClass.Ranger&&numbers.final>0&&CSStationary&&CS.action?.id=="A05")CancelClassSkill("HURT");
            State.receivedDamage.Add(new DamageReceived{time=State.time,hp=hpLoss,absorbed=absorbed});
            RecordDamage(new DamageEvent{id=State.nextDamageId++,incoming=true,kind=kind,rootCastId=root,effectInstanceId=instance,definitionId=definition,casterId=caster,element=element,tick=EffectTick,time=State.time,
                baseAttack=damage,independent=1,criticalMultiplier=1,attackBeforeDefense=numbers.beforeDefense,defenseReduction=numbers.defenseReduction,buffReduction=numbers.buffReduction,finalDamage=numbers.final,absorbed=absorbed,hpLoss=hpLoss});
            RecordTickIncomingDamage(numbers.final,hpLoss,caster,definition);
            if(before>Stats.hp*.3f&&State.health>0&&State.health<=Stats.hp*.3f&&hpLoss>0&&Stats.specials.Contains("LW04")&&State.build.activeSkills.Contains(1)&&(State.cooldowns[1]>0||D4LeapRecharging)&&ItemEffects.lw04Cooldown<=.00001f)
            {State.cooldowns[1]=0;if(D4LeapRecharging)ReduceD4LeapCooldown(float.MaxValue);ItemEffects.lw04Cooldown=AspectGrowth.Reduced(30,Stats.AspectLevel("LW04"));EffectEvent("LW04","COOLDOWN_RESET",root:root,value:ItemEffects.lw04Cooldown);}
            if(before>Stats.hp*.35f&&State.health>0&&State.health<=Stats.hp*.35f&&hpLoss>0&&Stats.specials.Contains("ELITE_RESOLVE")&&ItemEffects.eliteResolveCooldown<=.00001f)
            {AddShield("ELITE_RESOLVE",Stats.hp*.2f,4,root);ItemEffects.eliteResolveCooldown=30;}
            if(numbers.final>0&&State.health>0)
            {TriggerLegendary(LegendaryTrigger.Hurt,"*",attacker,root);if(blocked)TriggerLegendary(LegendaryTrigger.Block,"*",attacker,root);}
            AfterClassHurt(attacker,blocked,root);ApplyThorns(attacker);
            State.lastDamageTime=State.time;CancelChest("피격으로 개봉 중단");CancelShrine("피격으로 사용 중단");
            State.portalCast=0;Visual?.Invoke(State.position,State.position,32,numbers.final-absorbed);
            if(State.time-lastLog>=1){Log("DAMAGE",Loc.F("받은 피해 {0:0} / HP {1:0}", numbers.final, Mathf.Max(0,State.health)));lastLog=State.time;}
        }
    }
}
