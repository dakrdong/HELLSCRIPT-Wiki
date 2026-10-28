using System;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    public sealed partial class GameStore
    {
        public bool CommitHuntEdict(HuntEdictEditSession session,GameCatalog catalog,CombatSimulation simulation=null,bool remainPaused=false)
            =>CommitHuntEdictForHero(session,catalog,Data.Hero.id,simulation,remainPaused);
        internal bool CommitHuntEdictForHero(HuntEdictEditSession session,GameCatalog catalog,string heroId,CombatSimulation simulation=null,bool remainPaused=false,bool guidePotionLesson=false)
        {
            try
            {
                var owner=Data.heroes.Find(h=>h.id==heroId);
                if(owner==null||!owner.created||session==null||session.HeroId!=owner.id)throw new ArgumentException(Loc.T("설정을 편집한 캐릭터를 먼저 선택하세요."));
                if(guidePotionLesson&&(simulation==null||!GuideTutorial.PotionLesson(simulation.State)))throw new ArgumentException(Loc.T("물약 안내를 완료할 수 없습니다. 설정과 보유 수량을 확인해 주세요."));
                if(!ClassSkillLoadout.IsAbsent(owner.build.classSkills)&&!session.Draft.UsesTree)throw new ArgumentException("Use the ID-based skill loadout.");
                var loadout=HuntEdictLoadout.Canonical(session.Draft);
                HuntEdictLoadout.ValidateForHero(loadout,owner,catalog);ValidateUtilityChange(owner,loadout.edict);
                if(guidePotionLesson&&!GuideTutorial.PotionLessonThreshold(PotionPolicy.Resolve(owner,loadout.edict,false).hpThreshold))throw new ArgumentException(Loc.T("안전한 운영을 위해 HP 물약 사용 기준을 30% 이상으로 설정해 보세요."));
                HuntEdictProgression.ValidateChange(Data,owner,loadout,session.Recipes);
                CombatSimulation.PreparedHuntEdictChange prepared=null;RunState suspended=null;
                if(simulation!=null)
                {
                    if(simulation.Hero.id!=owner.id)throw new ArgumentException(Loc.T("현재 캐릭터가 진행 중인 균열의 편집 화면에서 저장하세요."));
                    // The prologue keeps its checkpoint in the guide instead of the suspended-run slot.
                    bool prologue=simulation.IsTutorial&&Data.guide.tutorialRun==simulation.State;
                    if(!simulation.IsOwnedBy(Data)||simulation.State.training>=0&&!prologue||Data.suspendedRun!=simulation.State&&!prologue||
                        simulation.State.phase==RunPhase.Cleared||simulation.State.phase==RunPhase.Failed||simulation.State.portal)
                        throw new ArgumentException(Loc.T("현재 캐릭터가 진행 중인 균열의 편집 화면에서 저장하세요."));
                    prepared=simulation.PrepareHuntEdictChange(loadout,remainPaused,guidePotionLesson);
                }
                else if(Data.suspendedRun?.heroId==owner.id&&!RepeatHunt.Terminal(Data.suspendedRun))
                {
                    if(Data.suspendedRun.training>=0)
                        throw new ArgumentException(Loc.T("현재 캐릭터가 진행 중인 균열의 편집 화면에서 저장하세요."));
                    suspended=CombatSimulation.PrepareSuspendedHuntEdictChange(Data,catalog,loadout);
                }
                var staged=JsonUtility.FromJson<AccountSave>(JsonUtility.ToJson(Data));var hero=staged.heroes.Single(h=>h.id==heroId);
                var before=hero.build;var previous=RuneGrowth.Copy(hero);ApplyHuntEdict(hero,loadout);
                if(prepared?.guidePotionStock!=null)hero.potions=prepared.guidePotionStock;
                TutorialProgress.EdictSaved(staged,previous,hero);
                FirstPlayGuide.Applied(staged,hero,before,hero.build,catalog);
                int selected=session.SelectedSlot;
                if(selected< -1||selected>=5)throw new ArgumentException(Loc.T("프리셋 슬롯은 1~5입니다."));
                if(selected>=0)
                {
                    var slot=hero.edictPresets[selected];
                    if(!HuntEdictPreset.HasValue(slot))throw new ArgumentException(Loc.T("저장된 프리셋이 없는 슬롯입니다."));
                    slot.loadout=loadout.Copy();slot.needsReview=false;hero.build.name=slot.name;
                    UpdateLegacyPresetMirror(hero,selected);
                }
                hero.selectedEdictPreset=selected;if(selected>=0)Tutorials.Performed(staged,"H10");
                if(prepared!=null&&simulation.IsTutorial)staged.guide.tutorialRun=prepared.snapshot;
                else if(prepared!=null)staged.suspendedRun=prepared.snapshot;
                else if(suspended!=null)staged.suspendedRun=suspended;
                staged.lastSeenUtc=SeenNow();
                if(!Write(staged,session))return false;
                // There are no callbacks between disk replacement and adoption.
                prepared?.Adopt();AdoptHuntEdict(owner,hero);
                if(suspended!=null)Data.suspendedRun=suspended;
                Data.guide=staged.guide;owner.guide=hero.guide;Data.lastSeenUtc=staged.lastSeenUtc;Error="";
                if(prepared!=null&&simulation.IsTutorial)Data.guide.tutorialRun=simulation.State;
                session.AcceptSaved(owner);NotifyCommitted("hunt-edict");
                if(guidePotionLesson)simulation.PresentGuidePotionLesson();return true;
            }
            catch(Exception error){Error=Loc.F("사냥 칙령을 저장하지 못했습니다: {0}",error.Message);return false;}
        }
        internal static void ApplyHuntEdict(HeroSave hero,HuntEdictLoadout loadout)
        {
            hero.build=loadout.ToBuild(hero.build);hero.edict=loadout.edict.Copy();hero.edictPassiveSlots=loadout.passives.ToArray();
            hero.useEdict=true;hero.huntEdictVersion=1;hero.edictMigrationNeedsReview=false;
        }
        static void AdoptHuntEdict(HeroSave target,HeroSave source)
        {
            target.build=source.build;target.edict=source.edict;target.edictPassiveSlots=source.edictPassiveSlots;
            target.huntEdictVersion=source.huntEdictVersion;target.useEdict=source.useEdict;
            target.useRecommendedEdict=source.useRecommendedEdict;
            target.edictMigrationNeedsReview=source.edictMigrationNeedsReview;
            target.edictPresets=source.edictPresets;target.presets=source.presets;target.selectedEdictPreset=source.selectedEdictPreset;
        }
        static void UpdateLegacyPresetMirror(HeroSave hero,int slot)
        {
            NormalizePresetSlots(hero);var preset=hero.edictPresets[slot];
            var mirror=preset.loadout.ToBuild(hero.build);mirror.name=preset.name;
            mirror.equipmentIds=preset.equipmentIds.ToList();mirror.equipmentPositions=preset.equipmentPositions?.ToList()??new System.Collections.Generic.List<int>();hero.presets[slot]=mirror;
        }
        // Store a received or edited snapshot without applying it to the hero.
        public bool StoreHuntEdictPreset(string heroId,int slot,HuntEdictPreset source,bool overwrite=false,bool reviewedLegacy=false)
        {
            try
            {
                if(Data.Hero.id!=heroId)throw new ArgumentException(Loc.T("설정을 편집한 캐릭터를 먼저 선택하세요."));
                if(slot<0||slot>=5)throw new ArgumentException(Loc.T("프리셋 슬롯은 1~5입니다."));
                if(HuntEdictPreset.HasValue(Data.Hero.edictPresets[slot])&&!overwrite)
                    throw new ArgumentException(Loc.T("이미 해당 슬롯에 프리셋이 존재합니다. 덮어쓰시겠습니까?"));
                if(!HuntEdictPreset.HasValue(source))throw new ArgumentException(Loc.T("보관할 설정이 없습니다."));
                if(source.needsReview&&!reviewedLegacy)throw new ArgumentException(Loc.T("이전 설정의 변환 내역을 먼저 확인해 주세요."));
                var candidate=source.Copy();candidate.name=PresetNames.Validate(source.name);candidate.loadout=HuntEdictLoadout.Canonical(source.loadout);
                if(!string.IsNullOrEmpty(candidate.loadout.classSkills?.kitKind))throw new ArgumentException("Authored skill kits cannot be stored as player presets.");
                if(!HuntEdictProgression.Has(Data,HuntEdictProgression.Presets))throw new ArgumentException(Loc.T("프리셋 관리는 균열 6단계부터 공개됩니다."));
                if(!HuntEdictProgression.Has(Data,HuntEdictProgression.Sharing)&&!string.IsNullOrEmpty(candidate.originalCode))throw new ArgumentException(Loc.T("공유와 가져오기는 균열 20단계부터 공개됩니다."));
                HuntEdictProgression.ValidateChange(Data,Data.Hero,candidate.loadout);
                if(!HuntEdictProgression.Legacy(Data)&&candidate.loadout.UsesTree)ClassSkillLoadout.Validate(candidate.loadout.classSkills,Data.Hero);
                if(candidate.loadout.edict.heroClass!=HuntEdict.ClassId(Data.Hero.heroClass))throw new ArgumentException(Loc.T("다른 직업의 사냥 칙령입니다."));
                if((candidate.originalCode?.Length??0)>HuntEdictPresetCodec.MaxCodeCharacters)throw new ArgumentException(Loc.T("공유 코드가 없거나 너무 깁니다."));
                candidate.needsReview=false;
                var staged=JsonUtility.FromJson<AccountSave>(JsonUtility.ToJson(Data));
                Tutorials.Performed(staged,"H10");staged.Hero.edictPresets[slot]=candidate;UpdateLegacyPresetMirror(staged.Hero,slot);
                // Importing over the active slot changes storage only; the old live configuration stays active.
                if(staged.Hero.selectedEdictPreset==slot)staged.Hero.selectedEdictPreset=-1;
                staged.lastSeenUtc=SeenNow();
                if(!Write(staged))return false;
                Data.guide=staged.guide;Data.Hero.edictPresets=staged.Hero.edictPresets;Data.Hero.presets=staged.Hero.presets;
                Data.Hero.selectedEdictPreset=staged.Hero.selectedEdictPreset;
                Data.lastSeenUtc=staged.lastSeenUtc;Error="";NotifyCommitted("hunt-edict");return true;
            }
            catch(Exception error){Error=Loc.F("프리셋을 저장하지 못했습니다: {0}",error.Message);return false;}
        }
        public bool RenameHuntEdictPreset(string heroId,int slot,string name)
        {
            try
            {
                if(Data.Hero.id!=heroId||slot<0||slot>=5||!HuntEdictPreset.HasValue(Data.Hero.edictPresets[slot]))
                    throw new ArgumentException(Loc.T("저장된 프리셋이 없는 슬롯입니다."));
                var validated=PresetNames.Validate(name);var staged=JsonUtility.FromJson<AccountSave>(JsonUtility.ToJson(Data));
                staged.Hero.edictPresets[slot].name=validated;staged.Hero.presets[slot].name=validated;
                if(staged.Hero.selectedEdictPreset==slot)staged.Hero.build.name=validated;
                staged.lastSeenUtc=SeenNow();if(!Write(staged))return false;
                AdoptHuntEdict(Data.Hero,staged.Hero);Data.lastSeenUtc=staged.lastSeenUtc;Error="";NotifyCommitted("hunt-edict");return true;
            }
            catch(Exception error){Error=Loc.F("이름을 저장하지 못했습니다: {0}",error.Message);return false;}
        }
    }
}
