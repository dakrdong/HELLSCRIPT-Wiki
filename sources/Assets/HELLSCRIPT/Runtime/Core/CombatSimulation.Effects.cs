using System;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    public sealed partial class CombatSimulation
    {
        int EffectTick=>Mathf.RoundToInt(State.time/Step);
        void EffectEvent(string definition,string kind,int instance=0,int root=0,int target=-1,float value=0,string reason="")
        {
            State.effectEvents.Add(new EffectEvent{definitionId=definition,kind=kind,instanceId=instance,rootCastId=root,targetId=target,tick=EffectTick,time=State.time,value=value,reason=reason});
            if(State.effectEvents.Count>600)State.effectEvents.RemoveAt(0);
        }
        void MirrorShields()
        {
            State.shield=State.shields.Sum(s=>Mathf.Max(0,s.amount));
            State.shieldTime=State.shields.Count==0?0:State.shields.Max(s=>s.remaining);
        }
        void AddShield(string definition,float baseAmount,float duration,int root)
        {
            var old=State.shields.Find(s=>s.definitionId==definition);
            float amount=baseAmount*Stats.shieldMultiplier;
            if(old!=null)
            {
                amount=Mathf.Max(amount,old.amount);
                EffectEvent(definition,"REPLACED",old.id,old.rootCastId,value:old.amount);
                State.shields.Remove(old);
            }
            amount=Mathf.Min(amount,Mathf.Max(0,Stats.hp-State.shields.Sum(s=>s.amount)));
            if(amount<=0||duration<=0){MirrorShields();return;}
            var shield=new ShieldEffect{id=State.nextId++,rootCastId=root,createdTick=EffectTick,definitionId=definition,remaining=duration,amount=amount,createdMaxHp=Stats.hp};
            State.shields.Add(shield);MirrorShields();EffectEvent(definition,"CREATED",shield.id,root,value:amount);
        }
        void TryShieldMana(ShieldEffect shield)
        {
            if(shield.definitionId!="M05"||shield.manaPaid||!Stats.specials.Contains("LM03")||ItemEffects.lm03Cooldown>.00001f||shield.absorbed+.00001f<shield.createdMaxHp*.15f)return;
            shield.manaPaid=true;ItemEffects.lm03Cooldown=4;
            float before=State.resource;State.resource=Mathf.Min(Stats.maxResource,State.resource+20);
            EffectEvent("LM03","RESOURCE",shield.id,shield.rootCastId,value:State.resource-before);
        }
        void TickShields(float dt)
        {
            ItemEffects.lm03Cooldown=Mathf.Max(0,ItemEffects.lm03Cooldown-dt);
            ItemEffects.lw04Cooldown=Mathf.Max(0,ItemEffects.lw04Cooldown-dt);
            ItemEffects.lc03Cooldown=Mathf.Max(0,ItemEffects.lc03Cooldown-dt);
            ItemEffects.eliteCascadeCooldown=Mathf.Max(0,ItemEffects.eliteCascadeCooldown-dt);
            ItemEffects.eliteResolveCooldown=Mathf.Max(0,ItemEffects.eliteResolveCooldown-dt);
            ItemEffects.eliteCycleCooldown=Mathf.Max(0,ItemEffects.eliteCycleCooldown-dt);
            // Heat leaves all at once four seconds after the last direct hit, never one stack at a time.
            if(ItemEffects.eliteHeatStacks>0&&State.time-ItemEffects.eliteHeatLast>=4)
            {EffectEvent("ELITE_HEAT","EXPIRED",value:ItemEffects.eliteHeatStacks);ItemEffects.eliteHeatStacks=0;}
            foreach(var shield in State.shields.ToArray())
            {
                shield.remaining=Mathf.Max(0,shield.remaining-dt);
                if(shield.remaining<=.00001f||shield.amount<=0)
                {State.shields.Remove(shield);EffectEvent(shield.definitionId,"EXPIRED",shield.id,shield.rootCastId);}
                else TryShieldMana(shield);
            }
            MirrorShields();
        }
        float AbsorbDamage(float damage)
        {
            float remaining=damage;
            foreach(var shield in State.shields.OrderBy(s=>s.remaining).ThenBy(s=>s.id).ToArray())
            {
                if(remaining<=0)break;
                float absorbed=Mathf.Min(remaining,shield.amount);remaining-=absorbed;shield.amount-=absorbed;shield.absorbed+=absorbed;
                EffectEvent(shield.definitionId,"ABSORB",shield.id,shield.rootCastId,value:absorbed);
                // The final absorption can earn the reward before this instance is depleted.
                TryShieldMana(shield);
                if(shield.amount<=.00001f){State.shields.Remove(shield);EffectEvent(shield.definitionId,"DEPLETED",shield.id,shield.rootCastId);}
            }
            MirrorShields();return damage-remaining;
        }
        bool TryUsePotion(float threshold)
        {
            if(threshold<=0||State.health<=0||State.health/Stats.hp*100>threshold||State.potionCd>0||!HasHpPotion)return false;
            var def=PotionCatalog.Get("PH01");
            float before=State.health;State.health=Mathf.Min(Stats.hp,State.health+Stats.hp*def.magnitude*Stats.healing*Stats.potionHealing);
            if(State.health<=before)return false;
            State.potionCd=State.potions.hpTotal=RunePotionCooldown(def.cooldown);
            if(State.potions.version>0)ConsumePotion(def);
            if(before<=Stats.hp*.2f&&Stats.specials.Contains("LC03")&&ItemEffects.lc03Cooldown<=.00001f)
            {AddShield("LC03",Stats.hp*.25f,3,0);ItemEffects.lc03Cooldown=20;}
            Visual?.Invoke(State.position,State.position,20,1);if(State.potions.version==0)Log("POTION","자동 물약 사용");return true;
        }
        void ApplyStatus(EnemyState enemy,StatusKind kind,string definition,float duration,int root,float strength=1,int area=0)
        {
            if(enemy==null||enemy.dead||duration<=0)return;
            if(enemy.boss&&(kind==StatusKind.Stun||kind==StatusKind.Freeze||kind==StatusKind.Root))
            {
                string key=root+":"+definition;
                if(!enemy.bossControl.credited.Contains(key))
                {enemy.bossControl.credited.Add(key);AddBossControl(enemy,duration*10,definition,root);}
                return;
            }
            if(!enemy.boss&&(kind==StatusKind.Stun||kind==StatusKind.Freeze||kind==StatusKind.Root||kind==StatusKind.Slow))duration*=1+Stats.Rune(RuneBonus.ControlOutgoing)/100;
            if(kind==StatusKind.Mark)foreach(var other in State.enemies)
            {other.mark=0;other.statuses.RemoveAll(s=>s.kind==StatusKind.Mark&&s.casterId==State.heroId&&(other!=enemy||s.rootCastId!=root));}
            var status=enemy.statuses.Find(s=>s.kind==kind&&s.definitionId==definition&&s.casterId==State.heroId&&s.rootCastId==root&&s.areaId==area);
            if(status==null)
            {
                status=new StatusEffect{id=State.nextId++,targetId=enemy.id,casterId=State.heroId,definitionId=definition,kind=kind,createdTick=EffectTick,areaId=area};enemy.statuses.Add(status);
                EffectEvent(definition,"STATUS",status.id,root,enemy.id,duration);
            }
            status.remaining=Mathf.Max(status.remaining,duration);status.strength=Mathf.Max(status.strength,strength);status.rootCastId=root;
        }
        void AddBossControl(EnemyState enemy,float value,string definition,int root)
        {
            var control=enemy.bossControl;if(control.staggered>0||control.immunity>0||value<=0)return;
            control.meter=Mathf.Min(100,control.meter+value);
            if(control.meter<100-.0001f)return;
            control.meter=0;control.staggered=3;
            InterruptBossAction(enemy,"보스 제압");
            EffectEvent(definition,"BOSS_STAGGER",root:root,target:enemy.id,value:3);Log("BOSS_STAGGER","보스 제압 · 3초 동안 행동 중단");
        }
        float SlowRatio(EnemyState enemy)
        {
            if(enemy.boss)return 0;
            return Mathf.Min(.8f,Mathf.Max(enemy.slow>0?.35f:0,enemy.statuses.Where(s=>s.kind==StatusKind.Slow&&s.remaining>0).Select(s=>s.strength).DefaultIfEmpty(0).Max()));
        }
        bool Immobilized(EnemyState enemy)=>CombatEffects.Has(enemy,StatusKind.Root)||CombatEffects.Has(enemy,StatusKind.Stun)||CombatEffects.Has(enemy,StatusKind.Freeze)||enemy.bossControl.staggered>0;
        float SlowControlCredit(EnemyState enemy,float start,float end)
        {
            // Bound field slows use the live area window, so entry, departure and partial expiry
            // neither lose the first step nor credit a stale status for one additional step.
            var slows=enemy.statuses.Where(s=>s.kind==StatusKind.Slow&&s.areaId==0&&s.remaining>start&&s.strength>0).ToArray();
            var areas=State.effects.Where(f=>OwnBlizzard(f)&&Vector2.Distance(f.position,enemy.position)<=f.radius&&Map.LineClear(f.position,enemy.position))
                .Select(f=>BlizzardWindow(f,end)).Where(w=>w.end>start&&w.start<end).ToArray();
            var boundaries=slows.Select(s=>Mathf.Min(end,s.remaining)).Concat(areas.SelectMany(w=>new[]{Mathf.Max(start,w.start),Mathf.Min(end,w.end)})).Append(end).Distinct().OrderBy(t=>t);
            float credit=0,cursor=start;
            foreach(float boundary in boundaries)
            {
                float strongest=slows.Where(s=>s.remaining>cursor).Select(s=>Mathf.Clamp(s.strength,0,.8f))
                    .Concat(areas.Where(w=>w.start<=cursor&&w.end>cursor).Select(w=>SlowStrength(GroundSnapshot(w.effect)))).DefaultIfEmpty(0).Max();
                credit+=strongest*Mathf.Max(0,boundary-cursor)*10;cursor=boundary;
            }
            return credit;
        }
        void TickStatuses(float dt)
        {
            foreach(var enemy in State.enemies.Where(e=>!e.dead))
            {
                var control=enemy.bossControl;
                if(enemy.boss)
                {
                    if(control.staggered>0)
                    {control.staggered=Mathf.Max(0,control.staggered-dt);if(control.staggered<=.00001f){control.staggered=0;control.immunity=10;EffectEvent("BOSS_CONTROL","IMMUNITY",target:enemy.id,value:10);}}
                    else
                    {
                        float immuneTime=Mathf.Min(dt,control.immunity);control.immunity=Mathf.Max(0,control.immunity-dt);
                        if(immuneTime<dt)AddBossControl(enemy,SlowControlCredit(enemy,immuneTime,dt),"SLOW",0);
                    }
                }
                foreach(var status in enemy.statuses)status.remaining=Mathf.Max(0,status.remaining-dt);
                enemy.statuses.RemoveAll(s=>s.remaining<=.00001f);
            }
        }
    }
}
