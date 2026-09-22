using System;
using UnityEngine;

namespace Hellscript
{
    public sealed partial class GameStore
    {
        public bool CommitCurrentBuild(BuildConfig requested,GameCatalog catalog,CombatSimulation simulation=null,bool remainPaused=false)
        {
            AccountSave staged;CombatSimulation.PreparedBuildChange prepared=null;
            try
            {
                if(catalog==null||!BuildEditing.HasPreset(requested))throw new ArgumentException("적용할 사냥 설정을 확인하세요.");
                string issue=BehaviorRules.InputValueIssue(requested);if(issue!="")throw new ArgumentException(issue);
                if(simulation!=null)
                {
                    if(!simulation.IsOwnedBy(Data)||simulation.State.training>=0||Data.suspendedRun!=simulation.State||
                        simulation.State.phase==RunPhase.Cleared||simulation.State.phase==RunPhase.Failed||simulation.State.portal)
                        throw new ArgumentException("현재 캐릭터가 진행 중인 균열의 편집 화면에서 저장하세요.");
                    prepared=simulation.PrepareBuildChange(requested,remainPaused);
                }
                else if(Data.suspendedRun!=null&&Data.suspendedRun.heroId==Data.Hero.id)
                    throw new ArgumentException("진행 중인 균열을 이어서 연 뒤 행동 수정에서 저장하세요.");
                staged=JsonUtility.FromJson<AccountSave>(JsonUtility.ToJson(Data));
                var candidate=prepared==null?requested.Copy():prepared.snapshot.build.Copy();
                var errors=BehaviorRules.Validate(candidate,staged.Hero.heroClass);
                string passiveError=ContentUnlocks.PassiveError(staged.Hero,candidate);if(passiveError!="")errors.Add(passiveError);
                if(errors.Count>0)throw new ArgumentException(errors[0]);
                var before=prepared==null?Data.Hero.build:simulation.State.build;
                staged.Hero.build=candidate;
                FirstPlayGuide.Applied(staged,staged.Hero,before,candidate,catalog);
                if(prepared!=null)staged.suspendedRun=prepared.snapshot;
                staged.lastSeenUtc=DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }
            catch(Exception e){Error=Loc.F("사냥 설정을 적용하지 않았습니다: {0}", e.Message);return false;}
            if(!Write(staged)){Error="사냥 설정을 저장하지 못했습니다. 기존 설정과 편집 내용은 유지됩니다. 저장 상태를 확인한 뒤 다시 시도하세요.";return false;}
            prepared?.Adopt();
            Data.Hero.build=staged.Hero.build;Data.Hero.guide=staged.Hero.guide;Data.lastSeenUtc=staged.lastSeenUtc;Error="";NotifyCommitted("build");
            return true;
        }
    }
}
