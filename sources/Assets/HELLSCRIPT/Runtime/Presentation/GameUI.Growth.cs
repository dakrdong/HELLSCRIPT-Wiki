using System;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    public sealed partial class GameUI
    {
        string growthRunId="";
        int growthNotifiedLevel;
        RunState growthPauseRun;
        bool growthWasPaused;
        void RefreshGrowthHud()
        {
            var combat=game.Combat;var run=combat.State;int level=combat.EffectiveLevel;
            headerTitle.text=BattleHeading(run)+$" · Lv.{level}";
            if(run.training<0)
            {
                var next=game.catalog.skills.Where(s=>s.heroClass==combat.Hero.heroClass&&s.unlock>level).OrderBy(s=>s.unlock).FirstOrDefault();
                headerSubtitle.text=Loc.F("{0}{1} · 성장 보기", (level>=30?"최대 레벨":$"XP {combat.Hero.xp:N0} / {Economy.XpRequired(level):N0}"), (next!=null?Loc.F(" · 다음 스킬 Lv.{0}", next.unlock):""));
            }
            var last=run.growthEvents.LastOrDefault();if(last==null)return;
            if(growthRunId==run.id&&growthNotifiedLevel==last.toLevel)return;growthRunId=run.id;growthNotifiedLevel=last.toLevel;
            ShowToast(Loc.F("Lv.{0} 달성{1}", last.toLevel, (last.unlockedSkills.Count>0?Loc.F(" · {0} 해금 · 성장에서 설정 확인", string.Join(", ",last.unlockedSkills.Select(i=>game.catalog.skills[i].name))):" · HP 비율을 유지했습니다.")));
        }
        public void ShowGrowth()
        {
            var combat=game.Combat;var run=combat?.State;
            if(Page!="growth"||growthPauseRun!=run){growthPauseRun=run;growthWasPaused=run?.paused??false;}
            bool paused=growthWasPaused;if(run!=null)run.paused=true;
            var hero=combat?.Hero??game.Store.Data.Hero;var build=run?.build??hero.build;int level=combat?.EffectiveLevel??hero.level;
            if(run==null||run.training<0)game.RecordGuide(()=>FirstPlayGuide.ReadBuild(hero));
            pageRepaint=()=>ShowGrowth();Base("growth","성장과 스킬",Loc.F("{0} Lv.{1} · 현재 장비와 행동을 확인합니다", game.catalog.classNames[(int)hero.heroClass], level));
            BigButton(content,"룬 성장",()=>{if(run!=null)run.paused=paused;ShowRunes();});
            Note(content,run?.training>=0?"훈련 복사본입니다. 실제 XP와 성장에는 반영하지 않습니다.":level>=30?"최대 레벨에 도달했습니다.":Loc.F("다음 레벨까지 XP {0:N0} / {1:N0}", hero.xp, Economy.XpRequired(level)),22,82,gold);
            Note(content,"스킬을 배워도 장착과 꺼 둔 규칙은 자동으로 바뀌지 않습니다. 사용할 설정은 행동 설계에서 직접 확인해 주세요.",20,106,pale);
            for(int n=(int)hero.heroClass*6;n<(int)hero.heroClass*6+6;n++)
            {
                var skill=game.catalog.skills[n];bool learned=skill.unlock<=level;int enabled=build.rules.Count(r=>r.action==RuleAction.Skill&&r.skill==n&&r.enabled);
                string state=!learned?Loc.F("Lv.{0} 해금 예정", skill.unlock):!build.activeSkills.Contains(n)?"배움 · 장착하지 않음":enabled==0?"장착됨 · 켜진 규칙 없음":Loc.F("장착됨 · 켜진 규칙 {0}개", enabled);
                var runeStats=combat?.Stats??new HeroStats(hero,false,game.Store.Data.runes);
                Note(content,Loc.F("{0}\n{1}",skill.name,state),21,78,learned?pale:muted);
                if(runeStats.runeSkillLevels[n]>0||runeStats.runeSkillPower[n]>0||runeStats.runeSkillCost[n]>0)Note(content,Loc.F("룬 마스터리 · 스킬 Lv.{0} · 추가 피해 {1:0.#}% · 자원 소모 감소 {2:0.#}%",runeStats.SkillLevel(n,learned),runeStats.runeSkillPower[n]+runeStats.runeSkillLevels[n]*10,runeStats.runeSkillCost[n]),19,76,gold);
            }
            if(run!=null)foreach(var change in run.growthEvents.AsEnumerable().Reverse().Take(3))
                Note(content,Loc.F("{0:0.0}초 · Lv.{1} → {2}\nHP {3:0.0}/{4:0.0} → {5:0.0}/{6:0.0}{7}", change.time, change.fromLevel, change.toLevel, change.healthBefore, change.oldMaxHp, change.healthAfter, change.newMaxHp, (change.unlockedSkills.Count>0?Loc.F("\n해금: {0}", string.Join(", ",change.unlockedSkills.Select(i=>game.catalog.skills[i].name))):"")),20,112,gold);
            for(int v=0;v<2;v++){int variant=v;BigButton(content,Loc.F("{0} · 현재 레벨 추천", GameCatalog.Preset(hero.heroClass,v).name),()=>{if(run!=null)run.paused=paused;ShowBuild();ShowRecommendation(variant);});}
            FooterButton(0,2,"능력치",()=>{if(run!=null)run.paused=paused;ShowAttributes();});
            FooterButton(1,2,run==null?"성소로":game.Active?"전투로 돌아가기":"결과로 돌아가기",()=>{if(run!=null)run.paused=paused;if(run==null)ShowTown();else if(game.Active)ShowBattle();else ShowResult();});
        }
        public void ShowRecommendation(int variant)
        {
            if(game.ComparisonRun&&game.Active){ShowToast("비교가 끝나면 B 설정에서 추천안을 선택할 수 있습니다.");return;}
            if(editing==null)ShowBuild();variant=Mathf.Clamp(variant,0,1);var hero=EditingHero;
            int level=EditingLevel;var proposed=BehaviorPresets.ForLevel(hero.heroClass,variant,level,game.catalog);
            var preview=RiftLoadoutLocked?BuildEditing.DuringRift(game.Combat.State.build,proposed,true):proposed;
            pageRepaint=()=>ShowRecommendation(variant);Base("recommendation","추천 행동 미리보기",Loc.F("{0} · 불러온 뒤 설정 적용으로 확정합니다", proposed.name));
            Note(content,BehaviorPresets.Concepts[(int)hero.heroClass*2+variant],23,84,gold);
            if(hero.heroClass==HeroClass.Mage&&variant==0&&level<game.catalog.skills[13].unlock)
                Note(content,"눈보라를 배우기 전에는 화염구로 예상 명중 2명 이상 또는 보스를 공격합니다. Lv.3부터 장판 연계 추천안을 확인하세요.",20,115,pale);
            if(hero.heroClass==HeroClass.Warrior&&variant==1&&level<3||hero.heroClass==HeroClass.Mage&&variant==1&&level<6)
                Note(content,"핵심 공격을 아직 배우지 않아 기본 공격 중심으로 진행합니다. 지금 사용할 공격이 더 많은 다른 추천안도 비교해 보세요.",20,106,gold);
            Note(content,RiftLoadoutLocked?"균열에서는 현재 장착을 유지한 후보입니다. 기존 사용자 설정은 아직 바뀌지 않았습니다.":"현재 작성 중인 편집안과 비교합니다. 실제 장비는 교체하지 않습니다.",20,94,pale);
            string[] targets={"가까운 적","정예·보스 우선","지원형 우선","낮은 HP","밀집 중심"};
            Note(content,Loc.F("행동 {0}행 변경\n목표: {1} → {2}\n이동: {3} {4:0.#}m → {5} {6:0.#}m\n물약 HP: {7:0}% → {8:0}%", BuildEditing.ChangedRows(editing,preview), targets[(int)editing.target], targets[(int)preview.target], BehaviorRules.Movements[(int)editing.movement], editing.distance, BehaviorRules.Movements[(int)preview.movement], preview.distance, editing.potionThreshold, preview.potionThreshold),20,158,pale);
            foreach(int skill in preview.activeSkills)
            {
                var s=game.catalog.skills[skill];int enabled=preview.rules.Count(r=>r.action==RuleAction.Skill&&r.skill==skill&&r.enabled);
                Note(content,Loc.F("{0} · {1}", s.name, (s.unlock>level?Loc.F("Lv.{0} 해금 전 · 규칙 꺼짐", s.unlock):enabled>0?Loc.F("사용 가능 · 규칙 {0}개", enabled):"켜진 규칙 없음")),20,65,s.unlock>level?muted:pale);
            }
            string PassiveNames(BuildConfig b)=>string.Join(", ",b.passives.Select(i=>GameCatalog.Passives[(int)hero.heroClass*6+i]));
            Note(content,Loc.F("패시브\n현재: {0}\n후보: {1}", PassiveNames(editing), PassiveNames(preview)),19,123,pale);
            Note(content,"불러오기는 편집안만 교체합니다. 돌아가면 작성 중인 설정을 유지합니다.",20,84,muted);
            FooterButton(0,2,"돌아가기",RenderBuild);FooterButton(1,2,"편집안에 불러오기",()=>LoadEditing(proposed),true);
        }
    }
}
