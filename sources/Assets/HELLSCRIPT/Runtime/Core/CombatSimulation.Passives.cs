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
            TickPassiveBuff(ref ItemEffects.leapDefense,dt,"W02");
            TickPassiveBuff(ref ItemEffects.elementBuff,dt,"MP06");
        }
        void LandingPassives(HeroActionState action)
        {
            ClassLanding(ClassSkills.LegacyId(action.skill),action.id);
        }
        void ApplyKillHealing(int deaths)
        {
            if(deaths==0||State.health<=0||Hero.heroClass!=HeroClass.Warrior||!Stats.passives[3]
                ||EffectTick-Mathf.RoundToInt(lastHeal/Step)<20)return;
            lastHeal=State.time;float before=State.health;
            State.health=Mathf.Min(Stats.hp,State.health+Stats.hp*SkillEffects.Passive(Ranks,HeroClass.Warrior,3)*Stats.healing);
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
            if(D4Combat)foreach(var enemy in State.enemies){if(!Passive("MP20"))enemy.d4Shatter=0;if(!Passive("MP21")){enemy.d4Elements=0;enemy.d4ElementUntil=0;}}
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
