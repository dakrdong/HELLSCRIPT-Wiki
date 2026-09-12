using UnityEngine;

namespace Hellscript
{
    public sealed partial class CombatSimulation
    {
        void TickPassiveBuff(ref float remaining,float dt,string id)
        {
            if(remaining<=0)return;
            remaining=Mathf.Max(0,remaining-dt);
            if(remaining>.00001f)return;
            remaining=0;EffectEvent(id,"EXPIRED");
        }
        void TickPassiveBuffs(float dt)
        {
            TickPassiveBuff(ref ItemEffects.leapDefense,dt,"WP03");
            TickPassiveBuff(ref ItemEffects.moveBuff,dt,"AP02");
            TickPassiveBuff(ref ItemEffects.elementBuff,dt,"MP06");
        }
        void LandingPassives(HeroActionState action)
        {
            if(action.skill==1&&Hero.heroClass==HeroClass.Warrior&&Stats.passives[2])
            {leapDefense=2;EffectEvent("WP03","BUFF",root:action.id,value:2);}
            if(action.skill==9&&Hero.heroClass==HeroClass.Ranger&&Stats.passives[1])
            {moveBuff=2;EffectEvent("AP02","BUFF",root:action.id,value:2);}
        }
        void ApplyKillHealing(int deaths)
        {
            if(deaths==0||State.health<=0||Hero.heroClass!=HeroClass.Warrior||!Stats.passives[3]
                ||EffectTick-Mathf.RoundToInt(lastHeal/Step)<20)return;
            lastHeal=State.time;float before=State.health;
            State.health=Mathf.Min(Stats.hp,State.health+Stats.hp*.02f*Stats.healing);
            EffectEvent("WP04","HEAL",value:State.health-before);
        }
        void RecordElementHit(int element,DamageSnapshot snapshot,int root)
        {
            if(Hero.heroClass!=HeroClass.Mage||!Stats.passives[5]||!snapshot.passives[5]||element<0||element>=6)return;
            int tick=EffectTick;var hits=ItemEffects.elementHitTicks;bool cross=false;
            for(int i=0;i<hits.Length;i++)if(i!=element&&tick>=hits[i]&&tick-hits[i]<=80){cross=true;break;}
            hits[element]=tick;lastElement=element;lastElementTime=State.time;
            if(!cross)return;
            bool active=elementBuff>0;elementBuff=4;
            EffectEvent("MP06",active?"REFRESHED":"BUFF",root:root,value:4);
        }
        void ResetUnavailablePassives()
        {
            if(Hero.heroClass!=HeroClass.Warrior||!Stats.passives[2])RemovePassiveBuff(ref ItemEffects.leapDefense,"WP03");
            if(Hero.heroClass!=HeroClass.Ranger||!Stats.passives[1])RemovePassiveBuff(ref ItemEffects.moveBuff,"AP02");
            if(Hero.heroClass!=HeroClass.Mage||!Stats.passives[5])
            {
                RemovePassiveBuff(ref ItemEffects.elementBuff,"MP06");
                for(int i=0;i<ItemEffects.elementHitTicks.Length;i++)ItemEffects.elementHitTicks[i]=-1000000;
                lastElement=-1;lastElementTime=0;
            }
        }
        void RemovePassiveBuff(ref float remaining,string id)
        {if(remaining>0)EffectEvent(id,"REMOVED",reason:"패시브 장착 해제");remaining=0;}
    }
}
