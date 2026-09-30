using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    // Production path: no training entry transaction, developer build, rewards service or real account is used.
    public sealed partial class CombatSimulation
    {
        readonly SkillPresetScenario previewScenario;
        readonly HashSet<int> previewSpawned=new HashSet<int>();
        float previewStartTime;
        bool previewRunning;
        public bool IsSkillPreview=>previewScenario!=null;
        public static CombatSimulation CreateSkillPreview(GameCatalog catalog,SkillPresetScenario scenario)
        {
            var definition=ClassSkills.Find(scenario.skill);
            var fixture=GameStore.NewAccount(catalog);fixture.selectedHero=definition.heroClass;
            var h=fixture.Hero;h.id="0000000000000000000000000000000"+definition.heroClass;h.level=40;h.inventory.Clear();h.build.classSkills=null;
            h.build.activeSkills.Clear();h.build.passives=Array.Empty<int>();h.useEdict=true;h.useRecommendedEdict=false;
            h.classUltimateRemaining=h.classUltimateTotal=0;h.potions=new PotionInventory{version=1};
            var load=ClassSkillTree.ResetAllocation(ClassSkillTree.FromHero(h));
            foreach(string id in scenario.Skills)load.ranks.Single(r=>r.id==id).rank=1;
            var ordinary=scenario.Skills.Where(id=>!ClassSkills.Find(id).Ultimate).ToArray();
            load.actives=ordinary.Concat(Enumerable.Repeat("",4-ordinary.Length)).ToArray();load.ultimate=scenario.Skills.FirstOrDefault(id=>ClassSkills.Find(id).Ultimate)??"";
            load.order=new[]{scenario.skill}.Concat(scenario.companions).Concat(new[]{"BASIC"}).Distinct().ToArray();load.automatic=load.order.ToArray();
            load.legacyEdict=HuntEdictV2.Create(h.heroClass);
            // Explicit preview-only global conditions. They are displayed along with the companion skills.
            void Global(string id,string value)=>load.legacyEdict.global.Single(o=>o.id==id).value=value;
            Global("survival.potion","OFF");Global("survival.lowHp","OFF");Global("survival.lethal","OFF");Global("survival.dot","OFF");
            bool escape=scenario.skill=="W02"&&scenario.preset=="escape"||scenario.skill=="A04"&&scenario.preset=="escape"||scenario.skill=="M04"&&scenario.preset!="distance";
            if(escape)
            {Global("survival.lowHp","ON");Global("survival.lowHpPercent","30");Global("survival.escapeSkill",scenario.skill);Global("survival.order","ESCAPE,DEFENSE,WALK");}
            foreach(var option in scenario.globals)Global(option.id,option.value);
            load.ProjectLegacy(h.build,catalog);h.edict=load.ProjectEdict();
            var configured=HuntEdictQuickPresets.Apply(HuntEdictLoadout.FromHero(h),"skill/"+scenario.skill,scenario.preset);
            // Companion recipes are ordinary policies too; no hidden manual casts occur during the example.
            foreach(string id in scenario.companions)
                configured=HuntEdictQuickPresets.Apply(configured,"skill/"+id,scenario.companionPresets.FirstOrDefault(p=>p.skill==id)?.preset??HuntEdictQuickPresets.For("skill/"+id)[0].id);
            configured.classSkills.ProjectLegacy(h.build,catalog);h.edict=configured.edict.Copy();h.build.classSkills.legacyEdict=h.edict.Copy();
            uint gearSeed=9137;
            for(int slot=0;slot<8;slot++)
            {
                var item=ItemGenerator.Create(h.heroClass,slot,0,30,ref gearSeed);h.inventory.Add(item);
                if(!Economy.Equip(h,item))throw new InvalidOperationException("Invalid ordinary preview equipment.");
            }
            var sim=new CombatSimulation(fixture,catalog,1,0,seed:scenario.seed,recordResume:false,combatPreview:scenario);
            sim.State.health=sim.Stats.hp*scenario.health;sim.State.resource=sim.Stats.maxResource*scenario.resource;
            // Generate dependent statuses through the real cast/release path before taking the initial snapshot.
            var automatic=sim.Loadout.automatic;sim.Loadout.automatic=Array.Empty<string>();
            foreach(var preparation in scenario.preparation)
            {
                var target=sim.State.enemies.First(e=>!e.dead);
                Vector2 aim=preparation.aim=="self"?sim.State.position:preparation.aim=="behind"?sim.State.position+Vector2.left*2:target.position;
                if(!sim.TryCastClassSkill(preparation.skill,target.id,aim))throw new InvalidOperationException("Preview preparation could not cast: "+scenario.Key+" / "+preparation.skill);
                for(int tick=0;tick<30&&(sim.HeroActionBusy||sim.CSBusy);tick++)sim.Tick(Step);
                if(sim.HeroActionBusy||sim.CSBusy)throw new InvalidOperationException("Preview preparation did not finish: "+scenario.Key);
                for(int tick=0;tick<Mathf.CeilToInt(preparation.settleSeconds/Step);tick++)sim.Tick(Step);
            }
            sim.Loadout.automatic=automatic;
            foreach(var delay in scenario.cooldowns)
            {
                int index=ClassSkills.LegacyIndex(delay.skill);
                if(index>=0)sim.State.cooldowns[index]=delay.seconds;
                else sim.CS.cooldowns.Add(new ClassSkillTimer{id=delay.skill,remaining=delay.seconds,total=delay.seconds});
            }
            sim.previewStartTime=sim.State.time;sim.previewRunning=true;sim.State.decisionTime=0;sim.Hero.build=sim.State.build.Copy();
            return sim;
        }
        void SpawnPreviewEnemies()
        {
            State.position=previewScenario.hero;State.destination=State.position;State.classSkills.previousPosition=State.position;
            State.enemies.Clear();State.visited=new List<int>{0};TickPreviewSpawns();
        }
        void TickPreviewSpawns()
        {
            if(!IsSkillPreview)return;
            for(int i=0;i<previewScenario.enemies.Length;i++)
            {
                var entry=previewScenario.enemies[i];
                if(previewSpawned.Contains(i)||!previewRunning&&entry.spawnAt>0||State.time-previewStartTime+.0001f<entry.spawnAt)continue;
                if(!Map.CanLand(entry.position)||!Map.LineClear(previewScenario.hero,entry.position))throw new InvalidOperationException("Invalid preview placement: "+previewScenario.Key);
                SpawnEnemy(0,entry.kind,entry.position,entry.elite);
                var enemy=State.enemies.Last();enemy.health=enemy.maxHealth=entry.health;previewSpawned.Add(i);
            }
        }
        bool PreviewOutcome()
        {
            if(State.health<=0){State.phase=RunPhase.Failed;return true;}
            if(State.enemies.All(e=>e.dead)&&previewSpawned.Count==previewScenario.enemies.Length){State.phase=RunPhase.Cleared;return true;}
            return false;
        }
        internal (string account,string run) PreviewSnapshot()=> (JsonUtility.ToJson(account),JsonUtility.ToJson(State));
        internal static CombatSimulation RestoreSkillPreview(GameCatalog catalog,SkillPresetScenario scenario,string account,string run)
        {
            var state=JsonUtility.FromJson<RunState>(run);
            var sim=new CombatSimulation(JsonUtility.FromJson<AccountSave>(account),catalog,1,0,state,recordResume:false,combatPreview:scenario);
            sim.previewStartTime=state.time;
            sim.previewRunning=true;
            for(int i=0;i<scenario.enemies.Length;i++)if(scenario.enemies[i].spawnAt==0)sim.previewSpawned.Add(i);
            return sim;
        }
    }
}
