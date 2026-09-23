using System;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    public sealed partial class CombatSimulation
    {
        HeroStats baseStats,potionStats;
        string potionStatsId;
        int potionStatsLevel;
        PotionPolicy potionPolicy;
        HuntEdictV2Document potionPolicySource;
        float potionPolicyThreshold=-1;
        public HeroStats Stats
        {
            get
            {
                string id=State?.potions?.utilityRemaining>0?State.potions.utilityId:"";
                if(string.IsNullOrEmpty(id))return baseStats;
                if(potionStats==null||potionStatsId!=id||potionStatsLevel!=EffectiveLevel)
                {potionStats=baseStats.WithPotion(PotionCatalog.Get(id),EffectiveLevel);potionStatsId=id;potionStatsLevel=EffectiveLevel;}
                return potionStats;
            }
            private set {baseStats=value;potionStats=null;}
        }
        public PotionPolicy CurrentPotionPolicy
        {
            get
            {
                if(potionPolicy==null||potionPolicySource!=edictSource||potionPolicyThreshold!=Policy.potionThreshold||potionPolicy.configuredSlots!=Hero.potions.slots)
                {
                    potionPolicy=PotionPolicy.Resolve(Hero,edictSource,false);potionPolicySource=edictSource;potionPolicyThreshold=Policy.potionThreshold;
                    if(edictSource==null){potionPolicy.hp=Policy.potionThreshold>0;potionPolicy.hpThreshold=Policy.potionThreshold;}
                }
                return potionPolicy;
            }
        }
        public PotionSlot[] ActivePotionSlots=>PotionLoadout.Copy(Hero.potions.slots?.Length>0?Hero.potions.slots:PotionLoadout.Defaults(State.potions.equippedUtility));
        public string[] ResolvedPotionSlots=>PotionLoadout.Resolve(Hero.potions,ActivePotionSlots);
        PotionDefinition ReadyPotion(string effect)=>ResolvedPotionSlots.Where(id=>id!=null).Select(PotionCatalog.Get).FirstOrDefault(p=>p.effect==effect);
        bool HasHpPotion=>State.potions.version==0||ReadyPotion("hp")!=null;
        void InitializePotions(bool fresh)
        {
            State.potions??=new PotionRuntimeState();Hero.potions??=new PotionInventory();
            if(State.cooldownTotals==null||State.cooldownTotals.Length!=18)State.cooldownTotals=new float[18];
            if(fresh)
            {
                // Training owns a copy, including supplies. Resumed legacy runs keep version zero.
                State.potions.version=Hero.potions.version;
                State.potions.equippedUtility=CurrentPotionPolicy.utility;
            }
        }
        void TickPotionTimers(float dt)
        {
            var p=State.potions;if(p.version==0)return;
            p.resourceCooldown=Mathf.Max(0,p.resourceCooldown-dt);p.utilityCooldown=Mathf.Max(0,p.utilityCooldown-dt);
            p.utilityRemaining=Mathf.Max(0,p.utilityRemaining-dt);
            if(p.utilityRemaining==0)p.utilityId="";
        }
        void UsePotion()
        {
            if(State.potions.version==0){TryUsePotion(Policy.potionThreshold);return;}
            var p=CurrentPotionPolicy;
            if(p.hp)TryUsePotion(p.hpThreshold);
            if(p.resource&&State.resource<=Stats.maxResource*p.resourceThreshold/100)TryUseResourcePotion();
            TryUseUtilityPotion(false);
        }
        public bool TryUseResourcePotion()
        {
            var state=State.potions;var def=ReadyPotion("resource");
            if(state.version==0||State.health<=0||state.resourceCooldown>0||def==null)return false;
            float next=Mathf.Min(Stats.maxResource,State.resource+Stats.maxResource*def.magnitude);
            if(next<=State.resource)return false;
            State.resource=next;ConsumePotion(def);state.resourceCooldown=state.resourceTotal=RunePotionCooldown(def.cooldown);return true;
        }
        void TryUseUtilityPotion(bool moved)
        {
            var p=CurrentPotionPolicy;
            bool combat=Target!=null||sensed.Any(e=>!e.dead);
            foreach(var id in ResolvedPotionSlots.Where(id=>id!=null&&PotionCatalog.IsUtility(id)))
            {
                string condition=p.condition=="DEFAULT"?PotionCatalog.DefaultCondition(id):p.condition;
                bool ready=condition=="MOVING"?moved:condition=="COMBAT"?combat:condition=="ELITE"?sensed.Any(e=>!e.dead&&(e.boss||e.elite>=0)):
                    condition=="DANGER"&&(State.health<=Stats.hp*.6f||InDanger);
                if(ready&&TryUseUtility(id))break;
            }
        }
        public bool TryUseEquippedUtility()
        {
            string id=ResolvedPotionSlots.FirstOrDefault(id=>id!=null&&PotionCatalog.IsUtility(id));
            return id!=null&&TryUseUtility(id);
        }
        bool TryUseUtility(string id)
        {
            var state=State.potions;
            if(state.version==0||State.health<=0||state.utilityCooldown>0||state.utilityRemaining>0||Hero.potions.Count(id)<=0)return false;
            var def=PotionCatalog.Get(id);var changed=baseStats.WithPotion(def,EffectiveLevel);
            if(!baseStats.PotionChanges(changed))return false;
            state.utilityId=def.id;state.utilityRemaining=state.utilityDuration=def.duration;
            state.utilityCooldown=state.utilityTotal=RunePotionCooldown(def.cooldown);ConsumePotion(def);return true;
        }
        void ConsumePotion(PotionDefinition def)
        {
            var slots=ActivePotionSlots;int index=Array.IndexOf(ResolvedPotionSlots,def.id);
            bool newest=index>=0&&slots[index].id!=def.id&&Hero.potions.SharedFallback==PotionFallback.Newest;
            Hero.potions.Consume(def.id,newest);Hero.potions.revision++;State.potions.uses++;
            Log("POTION",Loc.F("{0} 사용 · 남은 수량 {1}",Loc.T(def.name),Hero.potions.Count(def.id)));
        }
        public bool EquipTrainingPotion(string id)
        {
            if(State.training<0||!PotionCatalog.IsUtility(id)||State.potions.utilityRemaining>0)return false;
            if(Hero.potions.slots?.Length>0)
            {
                var slots=ActivePotionSlots;int index=Array.FindIndex(slots,s=>PotionCatalog.IsUtility(s.id));
                if(index<0||slots.Where((s,n)=>n!=index).Any(s=>s.id==id))return false;
                slots[index].id=id;Hero.potions.slots=slots;
            }
            State.potions.equippedUtility=id;return true;
        }
    }
}
