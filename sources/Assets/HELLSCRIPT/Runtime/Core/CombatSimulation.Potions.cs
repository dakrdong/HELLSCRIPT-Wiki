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
        bool guidePotionCommit;
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
            if(p.utilityRemaining==0&&!string.IsNullOrEmpty(p.utilityId))
            {
                p.utilityId="";State.shields.RemoveAll(s=>s.definitionId=="GEM_ELIXIR_SHIELD");MirrorShields();
                State.health=Mathf.Min(State.health,Stats.hp);State.resource=Mathf.Min(State.resource,Stats.maxResource);
            }
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
            CombatTelemetry.Support(State.statistics,def.id,SkillResultMetric.Resource,next-State.resource);
            State.resource=next;ConsumePotion(def);state.resourceCooldown=state.resourceTotal=RunePotionCooldown(def.cooldown);return true;
        }
        void TryUseUtilityPotion(bool moved)
        {
            if(EdictActive)return;
            var p=CurrentPotionPolicy;
            bool combat=Target!=null||sensed.Any(e=>!e.dead);
            foreach(var id in ResolvedPotionSlots.Where(id=>id!=null&&PotionCatalog.IsUtility(id)))
            {
                string condition=p.condition=="DEFAULT"?PotionCatalog.DefaultCondition(id):p.condition;
                bool ready=condition=="MOVING"?moved:condition=="COMBAT"?combat:condition=="ELITE"?sensed.Any(e=>!e.dead&&(e.boss||e.elite>=0)):
                    condition=="DANGER"&&(State.health<=Stats.hp*.6f||InDanger);
                if(ready&&State.potions.utilityRemaining<=0&&TryUseUtility(id))break;
            }
        }
        public bool TryUseEquippedUtility()
        {
            string id=ResolvedPotionSlots.FirstOrDefault(id=>id!=null&&PotionCatalog.IsUtility(id));
            return id!=null&&TryUseUtility(id);
        }
        public bool TryUseUtilitySlot(string id)=>ResolvedPotionSlots.Contains(id)&&PotionCatalog.IsUtility(id)&&TryUseUtility(id);
        bool TryUseUtility(string id,bool upkeep=false)
        {
            var state=State.potions;
            if(state.version==0||State.health<=0||state.utilityCooldown>0||Hero.potions.Count(id)<=0)return false;
            var def=PotionCatalog.Get(id);if(state.utilityRemaining>0&&(!def.Crafted&&!upkeep||upkeep&&state.utilityId!=id))return false;var changed=baseStats.WithPotion(def,EffectiveLevel);
            if(!def.Crafted&&!baseStats.PotionChanges(changed))return false;
            state.utilityId=def.id;state.utilityRemaining=state.utilityDuration=def.duration;
            state.utilityCooldown=state.utilityTotal=RunePotionCooldown(def.cooldown);
            State.shields.RemoveAll(s=>s.definitionId=="GEM_ELIXIR_SHIELD");MirrorShields();
            State.health=Mathf.Min(State.health,Stats.hp);State.resource=Mathf.Min(State.resource,Stats.maxResource);
            if(def.gemId=="G06")
            {
                float amount=Mathf.Min(Stats.hp*GemElixirs.Family("G06").third[def.grade-1]/100,Mathf.Max(0,Stats.hp-State.shield));
                if(amount>0)State.shields.Add(new ShieldEffect{id=State.nextId++,definitionId="GEM_ELIXIR_SHIELD",remaining=def.duration,amount=amount,createdMaxHp=Stats.hp,createdTick=EffectTick});
                MirrorShields();EffectEvent(def.id,"ELIXIR_SHIELD",value:amount);
            }
            ConsumePotion(def);return true;
        }
        // Invoked only by a prepared guide-policy commit: the preview is detached until Write succeeds.
        bool CompleteGuidePotionLesson()
        {
            if(!GuideTutorial.PotionLesson(State)||!State.paused||State.potions.version==0||State.potions.uses!=0||
                !CurrentPotionPolicy.hp||CurrentPotionPolicy.hpThreshold!=50||ReadyPotion("hp")?.id!=GuideTutorial.HealingPotion)return false;
            var visual=Visual;Visual=null;guidePotionCommit=true;
            // An old SkillSummary checkpoint may already have received the retired free heal; keep its HP intact.
            float trigger=State.tutorialPhase==GuideTutorial.SkillSummary?100:CurrentPotionPolicy.hpThreshold;
            try{if(!TryUsePotion(trigger))return false;}
            finally{guidePotionCommit=false;Visual=visual;}
            State.tutorialPhase=GuideTutorial.SkillSummary;State.paused=true;return true;
        }
        internal void PresentGuidePotionLesson()
        {
            var listeners=Visual;if(listeners==null)return;
            // Presentation cannot turn an already saved potion use into a failed transaction.
            foreach(Action<Vector2,Vector2,int,float> listener in listeners.GetInvocationList())
                try{listener(State.position,State.position,20,1);}
                catch(Exception error){Debug.LogWarning("Guide potion presentation listener failed: "+error.Message);}
        }
        void ConsumePotion(PotionDefinition def)
        {
            if(IsTutorial&&!(GuideTutorial.PotionTeaching(State)&&def.id==GuideTutorial.HealingPotion))return;
            var slots=ActivePotionSlots;int index=Array.IndexOf(ResolvedPotionSlots,def.id);
            bool newest=index>=0&&slots[index].id!=def.id&&Hero.potions.SharedFallback==PotionFallback.Newest;
            Hero.potions.Consume(def.id,newest);Hero.potions.revision++;State.potions.uses++;
            if(State.training<0)DailyQuests.Record(account,DailyQuestActivity.UsePotion,1);
            Log("POTION",Loc.F("{0} 사용 · 남은 수량 {1}",GemElixirs.Name(def),Hero.potions.Count(def.id)));
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
