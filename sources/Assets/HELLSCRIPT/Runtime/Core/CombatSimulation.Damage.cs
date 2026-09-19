using System.Linq;
using UnityEngine;

namespace Hellscript
{
    public sealed partial class CombatSimulation
    {
        static string SkillId(int skill)=>skill<0?"BASIC":new[]{"W","A","M"}[skill/6]+(skill%6+1).ToString("00");
        void RecordDamage(DamageEvent damage)
        {
            CombatTelemetry.Damage(State.statistics,damage);
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
            float bonus=snap.elements[element]+snap.bonus+(CombatEffects.Has(enemy,StatusKind.Mark)?(Stats.vulnerable-Stats.Bonus(StatId.VulnerableDamage)+RuneSnapshotBonus(StatId.VulnerableDamage,snap))/100:0)+extra+ConditionalBonus(enemy,kind,snap)+RuneDamageBonus(enemy,kind,snap);
            // Round the threshold to stored HP precision; Mono can otherwise retain extra intermediate precision.
            if(Hero.heroClass==HeroClass.Warrior)
            {if(snap.passives[0]&&(snap.crowdCaptured?snap.crowdQualified:CountNear(State.position,3)>=3))bonus+=SkillEffects.Passive(snap.ranks,HeroClass.Warrior,0);if(snap.passives[5]&&enemy.health<=(float)(enemy.maxHealth*.3f))bonus+=SkillEffects.Passive(snap.ranks,HeroClass.Warrior,5);}
            if(Hero.heroClass==HeroClass.Ranger&&snap.passives[0]&&Vector2.Distance(State.position,enemy.position)>=7)bonus+=SkillEffects.Passive(snap.ranks,HeroClass.Ranger,0);
            return bonus;
        }
        DamageEvent ApplyOutgoing(EnemyState enemy,float baseAttack,float additive,float independent,float critMultiplier,bool critical,int element,DamageSnapshot snapshot,string definition,int root,int instance,DamageKind kind,int triggerTarget=-1,bool projectile=false,Vector2? origin=null)
        {
            if(enemy==null||enemy.dead)return null;
            float defense=element==0?10+2*(State.stage-1):5+State.stage-1;
            var numbers=DamageMath.Calculate(baseAttack,additive,independent,critMultiplier,defense,snapshot.level,element,TargetReduction(enemy,element,projectile,origin));
            var damage=new DamageEvent{id=State.nextDamageId++,rootCastId=root,effectInstanceId=instance,definitionId=definition,casterId=State.heroId,targetId=enemy.id,triggerTargetId=triggerTarget,kind=kind,element=element,tick=EffectTick,time=State.time,
                baseAttack=baseAttack,additive=additive,independent=independent,critical=critical,criticalMultiplier=critMultiplier,attackBeforeDefense=numbers.beforeDefense,projectile=projectile,attackOrigin=origin??State.position,
                defenseReduction=numbers.defenseReduction,buffReduction=numbers.buffReduction,finalDamage=numbers.final,hpLoss=Mathf.Min(Mathf.Max(0,enemy.health),numbers.final)};
            RecordDamage(damage);Deal(enemy,numbers.final,critical);
            if(numbers.final>0)RecordElementHit(element,snapshot,root);
            return damage;
        }
        DamageEvent Hit(EnemyState enemy,float coefficient,int element,bool procs=true,float extraBonus=0,DamageSnapshot snapshot=null,bool canCrit=true,bool? shadowShot=null,string definition=null,int root=0,int instance=0,DamageKind kind=DamageKind.Direct,bool projectile=false,Vector2? origin=null)
        {
            if(enemy==null||enemy.dead)return null;
            var snap=snapshot??CaptureDamage();definition??=SkillId(State.heroAction.skill);if(root==0)root=State.heroAction.id;
            int masterySkill=definition.Length==3&&definition[0]=='W'?int.Parse(definition.Substring(1))-1:definition.Length==3&&definition[0]=='A'?int.Parse(definition.Substring(1))+5:definition.Length==3&&definition[0]=='M'?int.Parse(definition.Substring(1))+11:-1;
            if(masterySkill>=0&&masterySkill<18&&snap.runeSkillPower!=null&&snap.runeSkillPower.Length==18)coefficient*=1+snap.runeSkillPower[masterySkill]/100;
            extraBonus+=RuneAreaBonus(snap,definition);
            float crit=snap.crit+(Hero.heroClass==HeroClass.Ranger&&snap.passives[5]&&enemy.health<=(float)(enemy.maxHealth*.3f)?SkillEffects.Passive(snap.ranks,HeroClass.Ranger,5):0);
            bool critical=canCrit&&RandomStream.Unit(ref State.rng)<Mathf.Min(.75f,crit);
            if(!enemy.boss&&enemy.kind==6&&enemy.brain.rearWindow>0&&(kind==DamageKind.Direct||kind==DamageKind.Basic)&&Vector2.Angle(-enemy.brain.facing,(origin??State.position)-enemy.position)<=60)extraBonus+=.25f;
            bool overpowered=OverpowerRoll(kind);
            float bonus=AttackBonus(enemy,element,snap,extraBonus,kind)+(overpowered?Stats.overpower/100:0),criticalMultiplier=critical?snap.critDamage:1;
            bool ownPoison=CombatEffects.OwnTrapPoison(enemy,State.heroId);
            var damage=ApplyOutgoing(enemy,snap.damage*coefficient,bonus,1,criticalMultiplier,critical,element,snap,definition,root,instance,kind,projectile:projectile,origin:origin);
            if(damage!=null)
            {
                if(overpowered)EffectEvent(definition,"OVERPOWER",instance,root,value:damage.finalDamage);
                StealLife(damage.hpLoss);LuckyHit(damage.hpLoss);
                EliteDirectHit(enemy,damage,critical,element,snap,root,instance,kind);
            }
            if(procs&&Hero.heroClass==HeroClass.Ranger&&(shadowShot??State.shadowCharges>0)&&element==0&&!enemy.dead)
            {
                // A06 inherits the direct shot's critical result but uses shadow bonuses and resistance.
                var shadow=ApplyOutgoing(enemy,snap.damage*coefficient*ShadowFraction,AttackBonus(enemy,5,snap,extraBonus,DamageKind.Shadow),1,criticalMultiplier,critical,5,snap,"A06",root,instance,DamageKind.Shadow,projectile:projectile,origin:origin);
                if(shadow!=null&&Stats.specials.Contains("LA04")&&ownPoison&&!State.procHits.Any(p=>p.rootCastId==root&&p.definitionId=="LA04"&&(!p.originScoped||p.targetId==enemy.id)))
                {
                    // Old receipts lack the original target: retain their root-wide restriction instead of inventing an origin.
                    State.procHits.Add(new ProcReceipt{rootCastId=root,targetId=enemy.id,definitionId="LA04",originScoped=true});
                    var others=State.enemies.Where(e=>e!=enemy&&!e.dead&&Vector2.Distance(e.position,enemy.position)<=3&&Map.LineClear(enemy.position,e.position))
                        .OrderBy(e=>(e.position-enemy.position).sqrMagnitude).ThenBy(e=>e.id).Take(2).ToArray();
                    foreach(var other in others)
                    {
                        ApplyOutgoing(other,shadow.attackBeforeDefense,0,.5f,1,false,5,snap,"LA04",root,instance,DamageKind.Spread,enemy.id);
                    }
                }
            }
            return damage;
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
            var attacker=Attacker(caster);
            // Diablo IV settles dodge before anything else. A dodged hit lands nothing, so none of
            // the things a hit does — interrupting a chest, arming a proc, marking the hero as
            // recently damaged — happens either.
            if(DodgeIncoming())
            {
                EffectEvent(definition,"DODGED",instance,root);Visual?.Invoke(State.position,State.position,32,0);
                Log("DODGE","공격을 회피했습니다.");return;
            }
            bool blocked=BlockIncoming();
            var numbers=IncomingDamageNumbers(damage,element,IncomingReduction(attacker,element,blocked),kind==DamageKind.Periodic);
            if(blocked)EffectEvent(definition,"BLOCKED",instance,root,value:numbers.final);
            float before=State.health,absorbed=AbsorbDamage(numbers.final);State.health=Mathf.Max(0,State.health-(numbers.final-absorbed));
            float hpLoss=Mathf.Min(Mathf.Max(0,before),Mathf.Max(0,numbers.final-absorbed));
            State.receivedDamage.Add(new DamageReceived{time=State.time,hp=hpLoss,absorbed=absorbed});
            RecordDamage(new DamageEvent{id=State.nextDamageId++,incoming=true,kind=kind,rootCastId=root,effectInstanceId=instance,definitionId=definition,casterId=caster,element=element,tick=EffectTick,time=State.time,
                baseAttack=damage,independent=1,criticalMultiplier=1,attackBeforeDefense=numbers.beforeDefense,defenseReduction=numbers.defenseReduction,buffReduction=numbers.buffReduction,finalDamage=numbers.final,absorbed=absorbed,hpLoss=hpLoss});
            RecordTickIncomingDamage(numbers.final,hpLoss,caster,definition);
            if(before>Stats.hp*.3f&&State.health>0&&State.health<=Stats.hp*.3f&&hpLoss>0&&Stats.specials.Contains("LW04")&&State.build.activeSkills.Contains(1)&&State.cooldowns[1]>0&&ItemEffects.lw04Cooldown<=.00001f)
            {State.cooldowns[1]=0;ItemEffects.lw04Cooldown=30;EffectEvent("LW04","COOLDOWN_RESET",root:root,value:30);}
            if(before>Stats.hp*.35f&&State.health>0&&State.health<=Stats.hp*.35f&&hpLoss>0&&Stats.specials.Contains("ELITE_RESOLVE")&&ItemEffects.eliteResolveCooldown<=.00001f)
            {AddShield("ELITE_RESOLVE",Stats.hp*.2f,4,root);ItemEffects.eliteResolveCooldown=30;}
            ApplyThorns(attacker);
            State.lastDamageTime=State.time;CancelChest("피격으로 개봉 중단");CancelShrine("피격으로 사용 중단");
            State.portalCast=0;Visual?.Invoke(State.position,State.position,32,numbers.final-absorbed);
            if(State.time-lastLog>=1){Log("DAMAGE",Loc.F("받은 피해 {0:0} / HP {1:0}", numbers.final, Mathf.Max(0,State.health)));lastLog=State.time;}
        }
    }
}
