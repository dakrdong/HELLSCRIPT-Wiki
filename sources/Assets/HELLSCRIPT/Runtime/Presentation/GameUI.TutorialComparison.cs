using System.Linq;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed partial class GameUI
    {
        (string,string) ProgressiveSkillLessonStep()
        {
            var run=game.Combat.State;var hero=PrologueHero;var (skill,start,pick)=Tutorials.StarterComparison(hero.heroClass);
            var w=huntEdictWindow;bool equipped=hero.build.classSkills?.actives[0]==skill;
            if(w==null)return equipped&&run.tutorialLessonStep>=4?(null,null):("menu-hunt-edict","사냥 칙령을 열어 첫 스킬을 설정하세요.");
            if(!equipped)
            {
                if(w.SelectedTab!="skills")return ("edict-tab-skills","스킬을 여세요.");
                if(!w.ManagingSkills)return ("edict-policy-back","스킬 트리로 돌아가세요.");
                var draft=w.Session.Draft.classSkills;
                if(w.SelectedSkill!=skill)return ("edict-skill-"+skill,"첫 스킬을 눌러 효과를 확인하세요.");
                if(draft.Rank(skill)<=0)return ("edict-skill-rank-up","스킬 포인트 1점을 사용해 첫 스킬을 배우세요.");
                if(draft.actives[0]!=skill)return ("edict-skill-equip","첫 스킬을 1번 슬롯에 장착하세요.");
                return ("edict-save","저장하면 이 스킬을 실제 전투에서 사용합니다.");
            }
            if(run.tutorialLessonStep>=4)return ("edict-close","선택한 방식이 저장되었습니다. 창을 닫고 사냥을 이어가세요.");
            if(w.ManagingSkills)return !w.HasDialog?("edict-active-slot-0","장착한 첫 스킬을 누르세요."):("edict-slot-policy","사용 방식을 열어 전투를 비교하세요.");
            if(w.PolicySkill!=skill)return ("edict-policy-back","첫 스킬의 사용 방식을 여세요.");
            string needed=run.tutorialLessonStep<=0?start:pick;
            if(run.tutorialLessonStep==3)return ("starter-selection-area","두 방식을 확인했습니다. 원하는 방식을 활성화한 뒤 '이 방식으로 사냥하기'를 누르세요.");
            if(w.VisibleSkillPreset!=needed)return ("edict-preset-tab-"+needed,run.tutorialLessonStep==1?"이제 방식 B를 눌러 같은 조건에서 비교하세요.":"현재 구간의 사용 방식을 여세요.");
            if(w.ActiveSkillPreset!=needed)return ("edict-preset-activate","이 사용 방식을 활성화해 저장하세요.");
            if(run.tutorialLessonStep==1)return ("edict-starter-next","방식 B 저장을 확인하고 비교를 시작하세요.");
            return ("starter-observation-area",run.tutorialLessonStep==0?"방식 A의 실제 동작을 보세요. 재생·일시정지·재실행을 사용할 수 있습니다. 동작을 확인한 뒤 버튼을 누르세요.":"같은 시작 조건에서 방식 B를 관찰하세요. 움직임과 조건 대기를 확인한 뒤 버튼을 누르세요.");
        }
        (string,string) ProgressiveSurvivalLessonStep()
        {
            bool saved=HuntEdictSummary.Value(PrologueHero.edict,"survival.potionHpPercent")=="60";
            var w=huntEdictWindow;if(w==null)return saved?(null,null):("menu-hunt-edict","사냥 칙령을 열어 물약 기준을 바꾸세요.");
            if(saved)return ("edict-close","HP 60% 이하에서 물약을 사용하도록 저장했습니다. 창을 닫고 실제 회복을 확인하세요.");
            if(w.SelectedTab!="survival")return ("edict-tab-survival","생존을 여세요.");
            if(w.HasDialog)
            {
                var input=w.GetComponentsInChildren<InputField>().FirstOrDefault(f=>f.name=="edict-number-input");
                return input?.text=="60"?("edict-number-apply","60%를 적용하세요."):("edict-number-input","물약 HP 기준에 60을 입력하세요.");
            }
            if(HuntEdictSummary.Value(w.Session.Draft.edict,"survival.potionHpPercent")=="60")return ("edict-save","저장하면 현재 전투에도 새 기준을 적용합니다.");
            return ("edict-option-survival.potionHpPercent","물약 HP 기준을 눌러 40%를 60%로 바꾸세요.");
        }
    }
}
