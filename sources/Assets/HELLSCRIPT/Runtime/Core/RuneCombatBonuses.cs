using System;
using System.Collections.Generic;
using System.Linq;
using Hellscript.Runes;
using UnityEngine;

namespace Hellscript
{
    public enum RuneBonus { DirectDamage,BasicDamage,EliteDamage,AreaDamage,MultiDamage,ControlOutgoing,ShieldedDamage,BasicResource,PotionCooldown,MobilityGuard }
    public sealed partial class HeroStats
    {
        public float[] runeV13=new float[10],runeSkillRadius=new float[18],runeSkillCooldown=new float[18],runeSkillDuration=new float[18],runeSkillTargets=new float[18];
        public float Rune(RuneBonus bonus)=>runeV13[(int)bonus];
        void AddRune(RuneEffectContribution effect)
        {
            string meaning=effect.Meaning;
            if(meaning=="AttackPower"){runeAttack+=effect.Value;return;}
            if(RuneMasteryCatalog.IsElite(meaning)){specials.Add(RuneMasteryCatalog.EliteSpecial(meaning));return;}
            if(RuneMasteryCatalog.IsSkill(meaning))
            {
                int skill=RuneMasteryCatalog.SkillIndex(meaning);
                if(meaning.StartsWith("SkillLevel:",StringComparison.Ordinal)){runeSkillLevels[skill]=Mathf.Min(5,runeSkillLevels[skill]+(int)effect.Value);return;}
                var values=meaning.StartsWith("SkillPower:",StringComparison.Ordinal)?runeSkillPower:meaning.StartsWith("SkillCost:",StringComparison.Ordinal)?runeSkillCost:
                    meaning.StartsWith("SkillRadius:",StringComparison.Ordinal)?runeSkillRadius:meaning.StartsWith("SkillCooldown:",StringComparison.Ordinal)?runeSkillCooldown:
                    meaning.StartsWith("SkillDuration:",StringComparison.Ordinal)?runeSkillDuration:runeSkillTargets;
                values[skill]=Mathf.Min(RuneV13Catalog.ForMeaning(meaning).cap,values[skill]+effect.Value);return;
            }
            if(Enum.TryParse(meaning,out RuneBonus bonus)){runeV13[(int)bonus]=Mathf.Min(RuneV13Catalog.ForMeaning(meaning).cap,runeV13[(int)bonus]+effect.Value);return;}
            int stat=(int)Enum.Parse<StatId>(meaning);bonuses[stat]+=effect.Value;runeBonuses[stat]+=effect.Value;
        }
    }
    public sealed partial class CombatSimulation
    {
        static float SnapshotRune(DamageSnapshot snapshot,RuneBonus bonus)=>snapshot?.runeV13?.Length==10?snapshot.runeV13[(int)bonus]:0;
        float RuneDamageBonus(EnemyState enemy,DamageKind kind,DamageSnapshot snap)
        {
            float value=(DirectHit(kind)?SnapshotRune(snap,RuneBonus.DirectDamage):0)+(kind==DamageKind.Basic?SnapshotRune(snap,RuneBonus.BasicDamage):0);
            if(enemy.boss||enemy.elite>=0)value+=SnapshotRune(snap,RuneBonus.EliteDamage);
            if(State.shield>0)value+=SnapshotRune(snap,RuneBonus.ShieldedDamage);
            return value/100;
        }
        static readonly HashSet<string> runeAreaSkills=new HashSet<string>{"W01","W02","W03","W04","A02","A03","M01","M02","M06"};
        static float RuneAreaBonus(DamageSnapshot snap,string definition)=>runeAreaSkills.Contains(definition)?SnapshotRune(snap,RuneBonus.AreaDamage)/100:0;
        static float RuneMultiBonus(DamageSnapshot snap,int distinctTargets)=>distinctTargets>=3?SnapshotRune(snap,RuneBonus.MultiDamage)/100:0;
        public float RuneSkillRadius(int skill,float basis)=>basis*(1+Stats.runeSkillRadius[skill]/100);
        float RunePotionCooldown(float basis)=>basis*(1-Stats.Rune(RuneBonus.PotionCooldown)/100);
        // Membership is evaluated before damage for all projectiles of this cast crossing targets in this tick.
        int RuneProjectileTargets(CombatProjectile source,float dt)
        {
            if(SnapshotRune(source.snapshot,RuneBonus.MultiDamage)<=0)return 1;
            var ids=new HashSet<int>();
            foreach(var p in State.projectiles.Where(p=>!p.hostile&&!p.extra&&p.actionId==source.actionId&&p.createdAt<State.time-.00001f))
            {
                float time=Mathf.Max(0,dt-p.delay);if(time<=0)continue;
                var end=ClipProjectile(p.position,p.position+p.direction*Mathf.Min(p.remaining,p.speed*time),p.radius,out _);
                var group=State.projectileGroups.Find(g=>g.actionId==p.actionId);
                foreach(var e in State.enemies.Where(e=>!e.dead&&!p.hitIds.Contains(e.id)&&!float.IsInfinity(Entry(p.position,end,e.position,p.radius+(e.boss?1.2f:.4f))))
                    .OrderBy(e=>Entry(p.position,end,e.position,p.radius+(e.boss?1.2f:.4f))).ThenBy(e=>e.id).Take(Mathf.Max(0,p.maxHits-p.hitIds.Count)))
                    if(group==null||!group.victims.Any(v=>v.id==e.id&&v.hits>=group.maxPerVictim))ids.Add(e.id);
            }
            return ids.Count;
        }
    }
}
