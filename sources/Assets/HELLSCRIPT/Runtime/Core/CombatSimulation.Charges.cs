using System.Linq;
using UnityEngine;

namespace Hellscript
{
    public sealed partial class CombatSimulation
    {
        static float RemainingCharge(float value,float dt)=>value-dt<=.00001f?0:value-dt;
        void TickCharges(float dt)
        {
            ItemEffects.whirlwindCharge=RemainingCharge(ItemEffects.whirlwindCharge,dt);ItemEffects.crushCharge=RemainingCharge(ItemEffects.crushCharge,dt);
            ItemEffects.pierceCharge=RemainingCharge(ItemEffects.pierceCharge,dt);ItemEffects.chainCharge=RemainingCharge(ItemEffects.chainCharge,dt);
            if(ItemEffects.chainCharge==0)ItemEffects.chainCharges=0;
            ItemEffects.lc02Charge=RemainingCharge(ItemEffects.lc02Charge,dt);reducedNext=ItemEffects.lc02Charge>0;
            ItemEffects.ap05Cooldown=RemainingCharge(ItemEffects.ap05Cooldown,dt);
            if(lastBasic>=0&&!State.enemies.Any(e=>e.id==lastBasic&&!e.dead)){lastBasic=-1;basicCount=0;}
        }
        void TrackWalking(float moved,float dt,float eligiblePassiveTime=-1)
        {
            State.statistics.walkingDistance+=Mathf.Max(0,moved);
            var a=State.heroAction;
            if(a.phase==HeroActionPhase.Channeling&&a.skill==0&&a.cost>0&&Stats.SetPieces("SW")>=4)
            {
                ItemEffects.whirlwindDistance+=moved;
                if(ItemEffects.whirlwindCharge>0)ItemEffects.whirlwindDistance=Mathf.Min(3.999f,ItemEffects.whirlwindDistance);
                else if(moved>0&&ItemEffects.whirlwindDistance>=4-.00001f)
                {ItemEffects.whirlwindDistance=Mathf.Max(0,ItemEffects.whirlwindDistance-4);ItemEffects.whirlwindCharge=6;EffectEvent("SW4","CHARGE",root:a.id,value:6);}
            }
            else ItemEffects.whirlwindDistance=0;
            if(Hero.heroClass!=HeroClass.Ranger||!Stats.passives[4]){ItemEffects.ap05Ready=false;ItemEffects.ap05Movement=0;return;}
            if(ItemEffects.ap05Ready||ItemEffects.ap05Cooldown>0){ItemEffects.ap05Movement=0;return;}
            // Called around walking only: action travel, teleport and external position restoration are excluded.
            float elapsed=eligiblePassiveTime<0?dt:Mathf.Min(dt,eligiblePassiveTime);
            ItemEffects.ap05Movement=moved>.0001f?ItemEffects.ap05Movement+elapsed:0;
            if(ItemEffects.ap05Movement>=1-.00001f)
            {ItemEffects.ap05Ready=true;ItemEffects.ap05Movement=0;EffectEvent("AP05","CHARGE",value:.25f);}
        }
        void ConsumeCostEffects(float paid,HeroActionState action)
        {
            if(paid<=0)return;
            if(reducedNext)
            {reducedNext=false;ItemEffects.lc02Charge=0;EffectEvent("LC02","CONSUMED",root:action.id,value:paid);}
            if(ItemEffects.ap05Ready)
            {ItemEffects.ap05Ready=false;ItemEffects.ap05Movement=0;ItemEffects.ap05Cooldown=4;EffectEvent("AP05","CONSUMED",root:action.id,value:paid);}
            EliteCycle(paid,action);
        }
        // Every 50 resource actually paid trims the longest remaining cooldown. A 50 that lands during the
        // internal cooldown, or with nothing to trim, is still spent: it is never banked for later.
        void EliteCycle(float paid,HeroActionState action)
        {
            if(!Stats.specials.Contains("ELITE_CYCLE"))return;
            ItemEffects.eliteCycleSpent+=paid;
            while(ItemEffects.eliteCycleSpent>=50)
            {
                ItemEffects.eliteCycleSpent-=50;
                if(ItemEffects.eliteCycleCooldown>.00001f)continue;
                int longest=-1;
                for(int i=0;i<State.cooldowns.Length;i++)
                    if(State.build.activeSkills.Contains(i)&&State.cooldowns[i]>0&&(longest<0||State.cooldowns[i]>State.cooldowns[longest]))longest=i;
                if(longest<0)continue;
                State.cooldowns[longest]=Mathf.Max(0,State.cooldowns[longest]-1);
                ItemEffects.eliteCycleCooldown=3;
                EffectEvent("ELITE_CYCLE","COOLDOWN",root:action.id,target:longest,value:1);
            }
        }
        void ReserveCastCharges(HeroActionState a)
        {
            if(a.skill==2&&Stats.SetPieces("SWB")>=4&&ItemEffects.crushCharge>0)
            {a.crushBonus=true;ItemEffects.crushCharge=0;EffectEvent("SWB4","RESERVED",root:a.id);}
            if(a.skill==6&&Stats.SetPieces("SAB")>=4&&ItemEffects.pierceCharge>0)
            {a.pierceBonus=true;ItemEffects.pierceCharge=0;EffectEvent("SAB4","RESERVED",root:a.id);}
            if(a.skill==14&&Stats.SetPieces("SMB")>=4&&ItemEffects.chainCharge>0&&ItemEffects.chainCharges>0)
            {a.chainBonus=2;ItemEffects.chainCharges--;if(ItemEffects.chainCharges==0)ItemEffects.chainCharge=0;EffectEvent("SMB4","RESERVED",root:a.id,value:2);}
        }
        bool BeginTravel(HeroActionState a)
        {
            if(a.travelStarted)return true;
            if(!Map.CanLand(a.destination)||a.skill==1&&!string.IsNullOrEmpty(a.policy?.leapFollowUp)&&!EdictLeapLandingValid(a.origin,a.destination)||a.skill==9&&!string.IsNullOrEmpty(a.policy?.retreatFollowUp)&&!RangerLandingValid(a.origin,a.destination)){InterruptHeroAction("이동 시작 전 착지 지점이 유효하지 않습니다");return false;}
            a.travelStarted=true;a.phase=HeroActionPhase.Travelling;
            if(a.skill==1&&Stats.SetPieces("SW")>=4&&ItemEffects.whirlwindCharge>0)
            {a.whirlwindReserved=true;ItemEffects.whirlwindCharge=0;EffectEvent("SW4","RESERVED",root:a.id);}
            if(a.skill==9&&Stats.specials.Contains("LA03")&&(a.policy?.retreatTrapEquipped??State.build.activeSkills.Contains(8)))
                a.retreatTrapId=CreateTrap(new HeroActionState{id=a.id,skill=9,snapshot=CaptureDamage()},a.origin,3)?.id??0;
            ActionEvent(a,"ACTION_TRAVEL","이동 시작");return true;
        }
        void ResetUnavailableCharges()
        {
            if(Hero.heroClass!=HeroClass.Ranger||!Stats.passives[4]){ItemEffects.ap05Ready=false;ItemEffects.ap05Movement=0;ItemEffects.ap05Cooldown=0;}
            if(!Stats.specials.Contains("LC02")){reducedNext=false;ItemEffects.lc02Charge=0;basicCount=0;lastBasic=-1;procCooldown=0;}
            if(Stats.SetPieces("SW")<4){ItemEffects.whirlwindDistance=0;ItemEffects.whirlwindCharge=0;}
            if(Stats.SetPieces("SWB")<4)ItemEffects.crushCharge=0;
            if(Stats.SetPieces("SAB")<4)ItemEffects.pierceCharge=0;
            if(Stats.SetPieces("SMB")<4){ItemEffects.chainCharge=0;ItemEffects.chainCharges=0;}
        }
    }
}
