using System;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    public sealed partial class CombatSimulation
    {
        // Preview construction deliberately does not resume/normalize a run, spawn actors,
        // reset pause state, or attach live callbacks. It only rehearses the build transition.
        CombatSimulation(CombatSimulation source)
        {
            account=JsonUtility.FromJson<AccountSave>(JsonUtility.ToJson(source.account));catalog=source.catalog;
            Hero=account.heroes.Single(h=>h.id==source.Hero.id);
            State=JsonUtility.FromJson<RunState>(JsonUtility.ToJson(source.State));
            Stats=source.baseStats;Map=new RiftNavigation(State.layout);
        }
        BuildConfig ValidateBuildChange(BuildConfig requested)
        {
            if(!BuildEditing.HasPreset(requested))throw new ArgumentException("저장된 설정이 없는 슬롯입니다.");
            string issue=BehaviorRules.InputValueIssue(requested);if(issue!="")throw new ArgumentException(issue);
            var candidate=State.training<0?BuildEditing.DuringRift(State.build,requested):requested.Copy();
            var errors=BehaviorRules.Validate(candidate,Hero.heroClass);
            if(!FullSkillTraining){string passiveError=ContentUnlocks.PassiveError(Hero,candidate);if(passiveError!="")errors.Add(passiveError);}
            if(errors.Count>0)throw new ArgumentException(errors[0]);return candidate;
        }
        HeroStats BuildChangeStats(BuildConfig candidate)
        {
            if(State.training<0)return baseStats;
            var hero=JsonUtility.FromJson<HeroSave>(JsonUtility.ToJson(Hero));hero.build=candidate;hero.slotProgress=new SlotProgress{levels=(int[])State.slotLevels.Clone()};
            return new HeroStats(hero,FullSkillTraining,account.runes);
        }
        // This transition has no external callbacks, I/O, time reads, or random draws.
        // Both preflight and adoption use it, preserving existing object/navigation ownership.
        void ApplyValidatedBuild(BuildConfig candidate,HeroStats stats,BuildConfig persistentHeroBuild)
        {
            var previous=State.build;bool changed=BuildEditing.CombatSignature(previous)!=BuildEditing.CombatSignature(candidate);
            State.build=candidate;Stats=stats;if(State.training<0)Hero.build=persistentHeroBuild;
            if(!changed)return;
            StopEdictWalk();
            // The document may no longer match the equipped skills, so its eligibility is decided again.
            PrepareEdict();
            State.statistics.buildChanged=true;
            // Finite actions retain their start policy. A channel exits at its existing
            // quarter-second boundary without buying another tick; repeated saves cannot defer it.
            if(State.heroAction.phase==HeroActionPhase.Channeling)State.heroAction.exitChannelForBuild=true;
            State.movementRule=-1;State.shotApproachReview=0;Map.Repath();
            ResetUnavailableCharges();ResetUnavailableAreaEffects();ResetUnavailablePassives();ResetUnavailableLegendaryBuffs();
            Log("BUILD_CHANGED",Loc.F("{0} → {1} · 행동 {2}행 변경", previous.version, State.build.version, BuildEditing.ChangedRows(previous,State.build)));
        }
        internal bool IsOwnedBy(AccountSave owner)=>ReferenceEquals(account,owner)&&ReferenceEquals(Hero,owner.Hero)&&State.heroId==Hero.id;
        internal sealed class PreparedBuildChange
        {
            readonly CombatSimulation owner;
            readonly BuildConfig candidate,heroBuild;
            readonly HeroStats stats;
            readonly bool paused;
            internal readonly RunState snapshot;
            internal PreparedBuildChange(CombatSimulation owner,BuildConfig requested,bool paused)
            {
                this.owner=owner;this.paused=paused;
                candidate=owner.ValidateBuildChange(requested);stats=owner.BuildChangeStats(candidate);heroBuild=candidate.Copy();
                var preview=new CombatSimulation(owner);
                preview.ApplyValidatedBuild(candidate.Copy(),stats,heroBuild.Copy());preview.State.paused=paused;snapshot=preview.State;
            }
            // Called synchronously by GameStore only after the rehearsed snapshot reaches disk.
            internal void Adopt(){owner.ApplyValidatedBuild(candidate,stats,heroBuild);owner.State.paused=paused;}
        }
        internal PreparedBuildChange PrepareBuildChange(BuildConfig requested,bool paused)=>new PreparedBuildChange(this,requested,paused);
    }
}
