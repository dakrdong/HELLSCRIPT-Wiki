using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    // Staging of the guide tutorial stage (GuideTutorial): Velina's words on each waiting beat and the HUD skill lesson.
    // Every beat ends in game.ContinueTutorial or game.FinishTutorial; the lesson reads the hero's saved skills each frame.
    public sealed partial class GameUI
    {
        const string G=NpcProfiles.Guide;
        bool guideOpened,guideRoadBark,guideGoblinBark,guideTreasureRevealed,guideDotBark,guideShotBark,guideBossStaged,guideFrontBark,guideSwarmBark;string guideRun="",guideLesson="";
        bool GuideActive=>game.TutorialActive&&game.Combat.IsGuide;
        // The surrounded beat after her words: the HUD skill slot and its list lead to Whirlwind.
        bool GuideSkillLesson=>guideLesson=="hook"||guideLesson=="refund"||guideLesson=="spin"||guideLesson=="whirl";
        public bool GuideLessonActive=>GuideActive&&(GuideSkillLesson||guideLesson=="skill"&&game.Combat.State.tutorialPhase==GuideTutorial.Surrounded);
        NpcProfile GuideGoddess=>NpcProfiles.Find(G);
        void ResetGuide(RunState run){if(guideRun==run.id)return;guideRun=run.id;guideLesson="";guideOpened=guideRoadBark=guideGoblinBark=guideTreasureRevealed=guideDotBark=guideShotBark=guideBossStaged=guideFrontBark=guideSwarmBark=false;guideGearFlashPhase=-1;guideMessages=null;ResetGuideModifierReview();}
        // From TickTutorialUI (four times a second). True while the run is the guide stage, so the older flows stay out of it.
        bool TickGuideTutorial()
        {
            if(!GuideActive)return false;
            var run=game.Combat.State;ResetGuide(run);
            // A new account gets its chapter card from the awakening beat, as the hero sets off.
            if(!guideOpened){guideOpened=true;if(run.tutorialPhase!=GuideTutorial.Awakening)cinematic.Card(CinematicCard.Chapter,"튜토리얼","칙령의 길","벨리나가 당신의 첫걸음을 안내합니다",2.4f);}
            if(run.tutorialPhase==GuideTutorial.Road&&!guideRoadBark){guideRoadBark=true;cinematic.Bark(GuideGoddess,GuideTutorial.SkillLessons(run)?"저 앞쪽에서 또 다른 적들의 기척이 느껴져요. 천천히 다가가 볼까요?":"보셨나요? 영웅이 당신이 가르친 그대로 움직였습니다.");}
            if(run.tutorialPhase==GuideTutorial.GoblinAppear&&!guideGoblinBark){guideGoblinBark=true;cinematic.Bark(GuideGoddess,"저기 보세요! 보물 고블린이 나타났어요!");}
            TickGuideHazardStaging(run);
            // Time stands still on every beat she speaks on: the world takes a pale cold tint. Not while an edict sheet is open, which shrinks
            // the world into a window: the edge wash then covers the black around it and reads as a navy (or dark red) frame.
            bool still=GuideTutorial.Waiting(run.tutorialPhase)&&run.tutorialPhase!=GuideTutorial.Cleared&&run.tutorialPhase!=GuideTutorial.Awakening&&quickRoot==null&&!(game.World!=null&&game.World.SettingsWorldOpen);
            bool danger=run.tutorialPhase==GuideTutorial.DotTalk||run.tutorialPhase==GuideTutorial.ShotTalk;
            cinematic.Vignette(danger?new Color(.95f,.3f,.25f):new Color(.45f,.6f,.9f),still?(danger?.2f:.16f):0);
            if(GuideTutorial.Waiting(run.tutorialPhase)&&guideLesson==""&&1000+run.tutorialPhase!=tutorialPromptPhase&&!CommonPanelOpen&&!PlayInventoryOpen&&tutorialJournal==null&&!cinematic.Busy&&!GuideGearBeatWaits(run))
                ShowGuideBeat();
            return true;
        }
        // Opens the current beat's words; the HUD prompt button and a return request also land here.
        void ShowGuideBeat()
        {
            var run=game.Combat.State;ResetGuide(run);
            if(guideLesson!=""){ShowToast(GuideLessonReminder());return;}
            if(GuideTutorial.LessonOptions(run.tutorialPhase).Length>0&&!game.Store.UnlockGuideEdict(run)){ShowToast(game.Store.Error);return;}
            tutorialPromptPhase=1000+run.tutorialPhase;
            var lines=new List<StoryLine>();StoryChoice[] choices;
            void Say(string text)=>lines.Add(new StoryLine(G,text));
            switch(run.tutorialPhase)
            {
                case GuideTutorial.Awakening:ShowGuideAwakening();return;
                case GuideTutorial.Intro:
                    Say("어서 오세요. 이 세계에 오신 것을 환영합니다.");
                    Say("저는 벨리나, 이 세계의 칙령을 관장하는 여신입니다. 당신이 이곳에서 길을 잃지 않도록, 이 세계의 규칙을 하나씩 알려 드릴게요.");
                    Say("당신이 지켜보는 영웅은 적을 만나면 스스로 싸워요. 다만 어떻게 싸울지는 당신이 '사냥 칙령'에 적은 대로, 한 글자도 어김없이 따라요.");
                    Say("길이 짧지는 않지만 서두르지 않겠습니다. 천천히 따라와 주세요.");
                    choices=GuideGo("따르겠습니다",()=>game.ContinueTutorial());break;
                case GuideTutorial.FirstTalk:
                    if(GuideTutorial.SkillLessons(run))
                    {
                        Say("원거리 적이 나타났어요. 가까이 다가가기 전에 먼저 공격해 오네요.");
                        Say("꿰뚫는 갈고리를 사용하면 멀리 있는 적을 끌어와 쉽게 처치할 수 있어요.");
                        Say("스킬창에서 꿰뚫는 갈고리에 1포인트를 투자하고 장착해 볼까요?");
                        choices=GuideGo("스킬을 배운다",()=>StartGuideSkillLesson("hook"));break;
                    }
                    Say("보셨나요? 영웅이 적을 만나자 스스로 검을 휘둘렀습니다.");
                    Say("당신이 지켜보는 이 영웅은 적을 만나면 알아서 싸웁니다. 검을 뽑으라고 일일이 말해 줄 필요는 없어요.");
                    Say("다만 어떻게 싸울지는 당신이 정해야 합니다. 영웅은 당신이 사냥 칙령에 적은 대로, 한 글자도 어김없이 따를 거예요.");
                    Say("지금은 평범한 공격뿐이에요. 우선 이 싸움을 끝까지 지켜봐 주세요.");
                    choices=GuideGo("싸움을 지켜본다",()=>game.ContinueTutorial());break;
                case GuideTutorial.FrontTalk:
                    Say("작은 몬스터들이 전방에서 몰려왔어요. 하나씩 상대하니 체력이 많이 줄었네요.");
                    Say("이번에는 갈고리에 쓴 1포인트를 회수하고, 찢어낸 상처에 다시 투자해 보세요.");
                    Say("찢어낸 상처는 전방의 여러 적에게 출혈을 입혀요. 몰려오는 적들을 한 번에 상대하기 좋죠.");
                    choices=GuideGo("스킬을 바꾼다",()=>StartGuideSkillLesson("refund"));break;
                case GuideTutorial.SpinTalk:
                    Say("이번에는 사방에서 둘러쌌어요. 전방만 공격하는 찢어낸 상처만으로는 역부족이에요.");
                    Say("아까 레벨이 올랐으니, 새로 얻은 포인트로 스킬을 강화해 볼까요?");
                    Say("보조 스킬은 연결된 주스킬을 먼저 배워야 투자할 수 있어요.");
                    Say("각 주스킬에는 보조 스킬을 한 가지만 선택해 투자할 수 있어요.");
                    Say("보조 스킬마다 주스킬의 효과나 공격 형태가 달라져요. 상황에 맞는 하나를 골라 보세요.");
                    Say("먼저 세 보조 스킬을 하나씩 눌러 효과를 비교해 볼까요?");
                    choices=GuideGo("보조 스킬을 배운다",()=>StartGuideSkillLesson("spin"));break;
                case GuideTutorial.PotionTalk:
                    Say("주변의 적은 모두 물리쳤지만 상처가 났으니 어서 치료해야 해요. 처음이니까 제가 회복 물약 열 개를 드릴게요.");
                    Say("물약은 직접 마시지 않아도 돼요. 물약 설정에서 정한 HP 아래로 체력이 떨어졌을 때 물약이 남아 있으면 자동으로 마신답니다.");
                    Say("그런데 지금은 HP가 1% 이하가 되어야만 마시도록 되어 있어서 위험해요. 왼쪽 아래 물약 설정에서 HP 사용 기준을 30% 이상으로 정한 뒤 저장해 주세요.");
                    choices=GuideGo("물약 사용 시점을 바꾼다",StartGuidePotionLesson);break;
                case GuideTutorial.SkillSummary:
                    if(GuideTutorial.PotionLesson(run))goto case GuideTutorial.PotionTalk;
                    Say("이렇게 적의 약점을 파악하고, 가장 효율적인 스킬을 골라 장착하는 것이 중요해요.");
                    Say("똑같은 장비를 입더라도 적의 약점을 파악하느냐에 따라 승패가 갈릴 수 있답니다.");
                    Say("이제 길을 따라가며, 적을 상대할 위치와 공격 순서도 정해 볼까요?");
                    choices=GuideGo("계속 나아간다",()=>game.ContinueTutorial());break;
                case GuideTutorial.DotTalk:
                    Say("조심하세요! 바닥에 깔린 붉은 원이 지속 피해 장판이에요. 그 안에 서 있는 동안 피해가 계속 쌓여요.");
                    Say("가만히 서서 맞고만 있으면 위험해요. 맞지 않도록 피해 봐요!");
                    Say(GuideDodgeStanding("ground"));
                    Say("사냥 칙령의 '피해 유형별 회피'에서 지속 피해 장판 회피 기준을 7% 이하로 정해 주세요. 그러면 영웅이 장판을 알아보고 스스로 비켜서요.");
                    choices=GuideGo("회피 기준을 정한다",()=>StartGuideDodgeLesson("dot"));break;
                case GuideTutorial.ShotTalk:
                    Say("이번에는 활을 든 적들이에요! 날아오는 화살에 맞지 않도록 피해 봐요!");
                    Say(GuideDodgeStanding("direct"));
                    Say("'피해 유형별 회피'에서 투사체 회피 기준을 5% 이하로 정해 주세요. 그러면 영웅이 화살이 날아오는 길목에서 벗어나요.");
                    choices=GuideGo("회피 기준을 정한다",()=>StartGuideDodgeLesson("shot"));break;
                case GuideTutorial.LevelTalk:
                    Say("해냈어요! 레벨이 올랐어요. HP와 MP도 가득 채워졌답니다.");
                    Say("새 스킬 포인트도 하나 생겼어요. 아껴 두었다가, 곧 만날 보스를 위해 쓸 거예요.");
                    Say("이 길 끝에서 강한 기운이 느껴져요. 이제 보스를 만나러 가요.");
                    choices=GuideGo("보스를 만나러 간다",()=>game.ContinueTutorial());break;
                case GuideTutorial.BossTalk:
                    Say("저기 보스가 있어요! 호위하는 마물들까지 함께예요. 지금 가진 기술만으로는 쉽지 않아요.");
                    Say("그래서 영웅에게 전사의 메인 스킬, 회오리를 가르쳐 줄 거예요.");
                    Say("회오리는 몸을 돌리며 주변의 모든 적을 계속 베는 기술이에요. 가장 효율적으로, 가장 빠르게 몬스터를 정리할 수 있죠.");
                    Say("방금 얻은 포인트를 회오리에 쓰고 장착해 주세요. 이 기술로 보스를 잡아 봐요!");
                    choices=GuideGo("회오리를 배운다",()=>StartGuideSkillLesson("whirl"));break;
                case GuideTutorial.Surrounded:
                    Say("보세요, 사방이 막혔습니다. 평범한 공격만으로는 이 무리를 감당하기 어려워요.");
                    Say("이제 영웅에게 새로운 기술 쓰는 법을 가르쳐 주세요.");
                    Say("당신이 정해 준 기술로, 영웅은 저 무리와 맞서 싸울 거예요.");
                    choices=GuideGo("기술을 가르친다",()=>{guideLesson="skill";game.Audio?.Play("flow.skill_unlock");});break;
                case GuideTutorial.OpeningTalk:
                    Say("적이 나타났어요. 적과 처음 마주쳤을 때 어떻게 대응할지 정해 볼까요?");
                    Say("전사는 공격 거리가 짧기 때문에, 적에게 붙어서 싸우는 것이 중요합니다.");
                    choices=GuideGo("첫 교전 위치를 정한다",()=>guideLesson="opening");break;
                case GuideTutorial.CombatTalk:
                    if(GuideTutorial.CombatDone(HuntEdictDefaults.Resolve(game.Store.Data.Hero)))
                    {
                        Say(Loc.F("이렇게 하면 {0}이(가) 적을 좀 더 효율적으로 공격할 수 있습니다.",ClassSkillTree.NameForLoadout(GuideTutorial.Skill,game.Store.Data.Hero.build.classSkills)));
                        choices=GuideGo("확인",()=>game.ContinueTutorial());
                    }
                    else
                    {
                        Say("적과 마주친 뒤에도, 전투 중에 어디에서 싸울지 정할 수 있습니다.");
                        choices=GuideGo("전투 중 위치를 정한다",()=>guideLesson="combat");
                    }
                    break;
                case GuideTutorial.TargetTalk:
                    Say("적들의 협동 공격이 시작됐어요! 이럴 때 체력이 많은 적부터 공격하면, 다른 적들에게 더 많은 공격 기회를 주게 돼요.");
                    Say("공격 우선순위를 바꿔서 적을 더 효율적으로 물리쳐 봐요.");
                    choices=GuideGo("공격 대상을 정한다",()=>guideLesson="target-grade");break;
                case GuideTutorial.GoblinTalk:
                    Say("보물 고블린은 처치하면 엄청난 보상을 떨어뜨려요! 하지만 시간이 지나면 도망가 버리니까, 빠르게 쫓아가는 게 중요해요.");
                    choices=GuideGo("보물 고블린을 노린다",()=>guideLesson="treasure");break;
                case GuideTutorial.LootTalk:
                    Say("앗! 잠깐만요, 아직 바닥의 보상을 줍지 않았어요!");
                    Say("적을 처치하는 것도 중요하지만, 보상을 챙기는 건 그 이상으로 중요하다고요!");
                    choices=GuideGo("행동 우선순위를 바꾼다",()=>guideLesson="loot");break;
                case GuideTutorial.LootDoneTalk:
                    Say("이제 떨어진 아이템을 모두 주웠으니, 다시 보스 몬스터를 잡으러 가 볼까요?");
                    choices=GuideGo("보스에게 간다",()=>game.ContinueTutorial());break;
                case GuideTutorial.HelmetTalk:
                    Say("보물 고블린에게서 새로운 장비가 떨어졌어요!");
                    Say("입을 수 있는 장비를 얻으면, 이렇게 바로 입을 수 있도록 추천 팝업이 나타난답니다.");
                    choices=GuideGo("살펴본다",()=>StartGuideGearLesson("helmet"));break;
                case GuideTutorial.RareTalk:
                    Say("이것 보세요! 희귀 등급 장비를 얻었어요!");
                    Say("이번에도 추천 장비로 빠르게 장비를 바꿔 볼까요?");
                    choices=GuideGo("바꿔 본다",()=>StartGuideGearLesson("rare"));break;
                case GuideTutorial.MagicTalk:
                    Say("어? 이번엔 더 낮은 등급의 검이 표시됐네요. 카드의 점수 옆 숫자가 마이너스예요.");
                    Say("이렇게 기존 장비보다 낮은 등급의 아이템은 굳이 확인할 필요가 없죠.");
                    choices=GuideGo("설정을 바꾼다",()=>StartGuideGearLesson("magic"));break;
                case GuideTutorial.Cleared:
                    Say("보셨나요? 영웅이 당신이 가르친 그대로 움직였습니다.");
                    Say("이것이 사냥 칙령이에요. 앞으로도 길이 막힐 때마다 칙령을 고쳐 써서 길을 열어 주세요.");
                    Say("길은 저 위로 이어집니다. 마을에 닿을 때까지 제가 계속 지켜볼게요.");
                    choices=GuideGo("마을로 향한다",()=>cinematic.StartCoroutine(ToTown(true)));break;
                default:return;
            }
            CloseTutorialJournal();
            tutorialJournal=StoryDialogueWindow.Open(transform,"칙령의 길",EquipmentViewSource.Owned,()=>InterfaceFactor,new StoryDialogueState(lines,PrologueHero.gender),()=>choices,()=>tutorialJournal=null,"",blocksGameplay:false,listener:HeroPortrait);
        }
        string GuideObjective(RunState r)=>r.tutorialPhase switch
        {
            GuideTutorial.Awakening=>Loc.T("목표 · 눈을 뜨고 주변 살피기"),
            GuideTutorial.Entrance or GuideTutorial.Intro=>Loc.T("목표 · 벨리나의 말에 귀 기울이기"),
            GuideTutorial.FirstTalk when GuideTutorial.SkillLessons(r)=>Loc.T("목표 · 꿰뚫는 갈고리 배우고 장착하기"),
            GuideTutorial.FrontFight or GuideTutorial.FrontClear=>Loc.T("목표 · 전방의 작은 몬스터 처치"),
            GuideTutorial.FrontTalk=>Loc.T("목표 · 갈고리 포인트를 회수하고 찢어낸 상처 배우기"),
            GuideTutorial.SwarmRoad=>Loc.T("목표 · 길을 따라 전진"),
            GuideTutorial.SwarmFight or GuideTutorial.SpinClear=>Loc.T("목표 · 포위한 작은 몬스터 처치"),
            GuideTutorial.SpinTalk=>Loc.T("목표 · 회전 베기에 1포인트 투자하기"),
            GuideTutorial.PotionTalk=>Loc.T("목표 · 물약 사용 기준을 HP 30% 이상으로 저장하기"),
            GuideTutorial.SkillSummary when GuideTutorial.PotionLesson(r)=>Loc.T("목표 · 물약 사용 기준을 HP 30% 이상으로 저장하기"),
            GuideTutorial.SkillSummary=>Loc.T("목표 · 상황에 맞는 스킬 선택하기"),
            GuideTutorial.Surrounded=>Loc.F("목표 · 영웅에게 {0} 가르치기",ClassSkillTree.NameForLoadout(GuideTutorial.Skill,game.Store.Data.Hero.build.classSkills)),
            GuideTutorial.Whirlwind=>Loc.F("목표 · 포위한 마물 {0}/{1}마리 처치",r.enemies.Count(e=>e.group==GuideTutorial.RingGroup&&e.dead),r.enemies.Count(e=>e.group==GuideTutorial.RingGroup)),
            GuideTutorial.Road=>Loc.T("목표 · 길을 따라 전진"),
            GuideTutorial.OpeningTalk=>Loc.T("목표 · 첫 교전 위치 정하기"),
            GuideTutorial.Approach=>Loc.T("목표 · 원거리 적에게 접근"),
            GuideTutorial.CombatTalk=>Loc.T("목표 · 전투 중 위치 정하기"),
            GuideTutorial.FinalFight=>Loc.T("목표 · 남은 적 처치"),
            GuideTutorial.Road3=>Loc.T("목표 · 광장으로 전진"),
            GuideTutorial.DotFight=>Loc.T("목표 · 장판 마법사와 싸우기"),
            GuideTutorial.DotTalk=>Loc.T("목표 · 지속 피해 장판 회피 기준을 7% 이하로 정하기"),
            GuideTutorial.DotDodge=>Loc.T("목표 · 장판을 피하며 마법사 처치"),
            GuideTutorial.ShotFight=>Loc.T("목표 · 궁수들과 싸우기"),
            GuideTutorial.ShotTalk=>Loc.T("목표 · 투사체 회피 기준을 5% 이하로 정하기"),
            GuideTutorial.ShotDodge=>Loc.F("목표 · 화살을 피하며 궁수 처치 {0}/{1}",r.enemies.Count(e=>e.group==GuideTutorial.ShotGroup&&e.dead),r.enemies.Count(e=>e.group==GuideTutorial.ShotGroup)),
            GuideTutorial.LevelUp or GuideTutorial.LevelTalk=>Loc.T("목표 · 레벨 업!"),
            GuideTutorial.Road4=>Loc.T("목표 · 보스를 만나러 전진"),
            GuideTutorial.BossTalk=>Loc.T("목표 · 회오리를 배우고 장착하기"),
            GuideTutorial.MixedFight or GuideTutorial.TargetFight=>Loc.T("목표 · 협동 공격에 맞서 싸우기"),
            GuideTutorial.TargetTalk=>Loc.T("목표 · 공격 대상 우선순위 정하기"),
            GuideTutorial.BossFight or GuideTutorial.FinalBoss=>Loc.T("목표 · 보스 처치"),
            GuideTutorial.GoblinAppear or GuideTutorial.GoblinChase=>Loc.T("목표 · 보물 고블린 처치"),
            GuideTutorial.GoblinTalk=>Loc.T("목표 · 보물 몬스터를 최우선으로"),
            GuideTutorial.ReturnToBoss or GuideTutorial.LootTalk=>Loc.T("목표 · 회수를 최우선으로"),
            GuideTutorial.Pickup=>Loc.T("목표 · 바닥의 보상 회수"),
            GuideTutorial.LootDoneTalk=>Loc.T("목표 · 보스에게 돌아가기"),
            GuideTutorial.HelmetTalk=>Loc.T("목표 · 추천 장비 살펴보기"),
            GuideTutorial.RareTalk=>Loc.T("목표 · 희귀 장비로 교체하기"),
            GuideTutorial.MagicTalk=>Loc.T("목표 · 추천 설정 바꾸기"),
            GuideTutorial.BossLoot=>Loc.T("목표 · 보스가 남긴 장비 회수"),
            GuideTutorial.Cleared=>Loc.T("포위를 뚫었다"),
            _=>Loc.T("목표 · 길을 따라 첫 적 처치")
        };
        StoryChoice[] GuideGo(string label,System.Action act)=>new[]{new StoryChoice("tutorial-continue",label,()=>{CloseTutorialJournal();act();},true)};
        // Each frame: names the one HUD control that moves the lesson on. Once Whirlwind is learned and slotted, time flows again.
        void TickGuideLesson()
        {
            if(!GuideActive||guideLesson==""||tutorialJournal!=null){if(gate!=null&&gate.Active&&!RuneIntroductionActive)gate.Hide();ClearPrologueRing();return;}
            if(TickGuideTargetLesson())return;
            if(TickGuideModifierLesson())return;
            if(TickGuideGearLesson())return;
            var load=game.Store.Data.Hero.build.classSkills;
            var edict=HuntEdictDefaults.Resolve(game.Store.Data.Hero);
            bool done=guideLesson=="potion"?GuideTutorial.PotionLessonDone(game.Combat.State):GuideSkillLesson?GuideTutorial.SkillLessonDone(load,game.Combat.State.tutorialPhase):guideLesson=="skill"?GuideTutorial.LessonDone(load):guideLesson=="opening"?GuideTutorial.OpeningDone(edict):
                guideLesson=="dot"?GuideTutorial.GroundDone(edict):guideLesson=="shot"?GuideTutorial.DirectDone(edict):GuideTutorial.CombatDone(edict);
            if(done&&guideLesson=="potion"&&GuidePotionRecoveryWaiting())return;
            if(done)
            {
                string lesson=guideLesson;GuideLessonNotice(lesson);if(GuideSkillLesson)CloseGuideSkillTree();guideLesson="";gate?.Hide();ClearPrologueRing();CloseHudPanel();CloseEdictQuick(false);
                if(lesson=="potion"){ShowGuideBeat();return;}
                if(lesson=="combat")ShowGuideBeat();
                else if(game.ContinueTutorial())GuideLessonFinished(lesson);
                return;
            }
            EnsureGate();
            var (target,line)=GuideLessonStep(load);
            if(target!=null&&UiVisibility.Fraction(target,UiSafeArea.Current)<.99f)UiVisibility.EnsureVisible(target);
            gate.Show(target,G,line,pointerAboveDim:true,strongGuidance:true);
            if(target==null){ClearPrologueRing();return;}
            if(prologueRing!=null&&prologueRing.transform.parent==target)return;
            ClearPrologueRing();var rect=UiLayout.Rect("Prologue focus",target);UiLayout.Stretch(rect);prologueRing=rect.gameObject.AddComponent<TutorialAnchorRing>();prologueRing.FillInterior=false;prologueRing.ShowPointer=false;
        }
        (RectTransform,string) GuideLessonStep(ClassSkillLoadout load)
        {
            if(guideLesson=="potion")return GuidePotionLessonStep();
            if(guideLesson=="dot"||guideLesson=="shot")return GuideDodgeLessonStep(guideLesson=="dot"?"ground":"direct");
            if(GuideSkillLesson)return GuideTreeLessonStep(load);
            if(guideLesson!="skill")return GuidePositionLessonStep(guideLesson=="opening");
            bool learned=load.Rank(GuideTutorial.Skill)>0;
            var popup=skillPopupOpen?Named(hudPanel,learned?"edict-quick-skill-equip":"edict-quick-skill-rank-up"):null;
            if(popup!=null)return learned?(popup,Loc.T("이제 '장착'을 눌러 첫 번째 칸에 올려 주세요.")):(popup,Loc.F("스킬 포인트가 1점 있어요. '레벨 올리기'를 눌러 영웅이 {0}을(를) 익히게 해 주세요.",ClassSkillTree.NameForLoadout(GuideTutorial.Skill,load)));
            var item=quickRoot!=null&&quickKind=="skill"&&!skillPopupOpen?Named(quickRoot,"edict-quick-skill-"+GuideTutorial.Skill):null;
            if(item!=null)return learned?(item,Loc.F("{0}을(를) 눌러 첫 번째 칸에 장착해 주세요.",ClassSkillTree.NameForLoadout(GuideTutorial.Skill,load))):(item,SkillTreeD4.Enabled?Loc.T("찢어낸 상처예요. 앞의 적에게 짧은 출혈을 남기고 자원을 얻는 기술이죠. 눌러서 살펴보세요."):Loc.T("회오리예요. 몸을 돌려 주변의 적을 한꺼번에 베는 기술이죠. 눌러서 살펴보세요."));
            if(skillPopupOpen||quickRoot!=null){CloseHudPanel();CloseEdictQuick(false);}
            return (globalHud?.SkillSlot(0),Loc.T(learned?"첫 번째 기술 칸을 다시 눌러 주세요.":"영웅의 첫 번째 기술 칸을 눌러 주세요."));
        }
        void StartGuideSkillLesson(string lesson)
        {
            ResetGuideModifierReview();guideLesson=lesson;CloseHudPanel();CloseEdictQuick(false);ShowEdictSkillTree();
        }
        void ReleaseGuideSkillTreeInput()
        {UnbindGuideModifierTree();gate?.Hide();ClearPrologueRing();}
        void CloseGuideSkillTree()
        {ReleaseGuideSkillTreeInput();CurrentSkillTree?.Close();}
        (RectTransform,string) GuideTreeLessonStep(ClassSkillLoadout load)
        {
            string skill=guideLesson=="hook"?GuideTutorial.Hook:guideLesson=="whirl"?GuideTutorial.Whirl:GuideTutorial.Rend;
            bool refund=guideLesson=="refund"&&load.Rank(GuideTutorial.Hook)>0;
            bool allocating=guideLesson=="spin"||refund||load.Rank(skill)==0;
            if(allocating)
            {
                if(CurrentSkillTree==null)ShowEdictSkillTree();
                var tree=CurrentSkillTree;if(tree==null)return(null,Loc.T("스킬창을 열어 주세요."));
                int index=System.Array.FindIndex(SkillTreeD4.For(HeroClass.Warrior).actives,a=>a.id==(refund?GuideTutorial.Hook:skill));
                var wanted=guideLesson=="spin"?SkillTreeD4Selection.ModifierOf(index,2):SkillTreeD4Selection.Active(index);
                bool selected=tree.Session.Selection.Equals(wanted);
                var target=tree.GuideTarget(wanted,selected?(bool?)!refund:null);
                string instruction=refund?Loc.T("꿰뚫는 갈고리의 포인트 1점을 회수해 주세요."):guideLesson=="spin"?Loc.T("적들이 사방을 둘러싸고 있으니 회전 베기가 알맞아요. 1포인트를 투자해 주변의 적을 물리쳐 보세요."):
                    Loc.F("{0}을(를) 선택하고 1포인트를 투자해 주세요.",ClassSkillTree.NameForLoadout(skill,load));
                return(target,instruction);
            }
            if(CurrentSkillTree!=null)CloseGuideSkillTree();
            var item=quickRoot!=null&&quickKind=="skill"?Named(quickRoot,"edict-quick-skill-"+skill):null;
            // Whirlwind joins the slot beside the bleed, so the first skill stays equipped.
            int slot=guideLesson=="whirl"?1:0;
            return item!=null?(item,Loc.F(slot==0?"{0}을(를) 눌러 첫 번째 칸에 장착해 주세요.":"{0}을(를) 눌러 두 번째 칸에 장착해 주세요.",ClassSkillTree.NameForLoadout(skill,load))):
                (globalHud?.SkillSlot(slot),Loc.T(slot==0?"영웅의 첫 번째 기술 칸을 눌러 주세요.":"영웅의 두 번째 기술 칸을 눌러 주세요."));
        }
        (RectTransform,string) GuidePositionLessonStep(bool opening)
        {
            if(quickRoot==null||quickKind!="edict")
            {
                if(quickRoot!=null){CloseHudPanel();CloseEdictQuick(false);}
                return (globalHud?.EdictButton,Loc.T("사냥 칙령 버튼을 눌러 주세요."));
            }
            if(sheetCard==null||sheetTab!="combat")return (Named(quickRoot,"edict-sheet-tab-combat"),Loc.T("전투 아이콘을 눌러 주세요."));
            if(positionList)return (Named(sheetCard,opening?"edict-entry-opening":"edict-entry-combat"),Loc.T(opening?"'첫 교전 위치'를 눌러 주세요.":"'전투 중 위치'를 눌러 주세요."));
            if(!PositionPanel)
            {
                var groups=HuntEdictHudRows.Groups(sheetTab,game.Store.Data,game.Store.Data.Hero);
                int index=0;while(index<groups.Count&&!groups[index].ids.Contains("position.engage"))index++;
                return (Named(sheetCard,"edict-sheet-group-"+index),Loc.T("'전투 위치 거리'를 눌러 주세요."));
            }
            if(positionOpening!=opening)return (Named(sheetCard,opening?"edict-position-tab-opening":"edict-position-tab-combat"),Loc.T(opening?"'첫 교전 위치'를 눌러 주세요.":"'전투 중 위치'를 눌러 주세요."));
            return (Named(sheetCard,opening?"edict-sheet-choice-position.engage-APPROACH":"edict-sheet-choice-position.mode-CENTER"),Loc.T(opening?"'즉시 접근'을 눌러 주세요.":"'무리 중심'을 눌러 주세요."));
        }
        static RectTransform Named(Transform root,string name)
        {
            if(root==null)return null;
            var found=root.GetComponentsInChildren<RectTransform>().FirstOrDefault(r=>r.name==name&&r.gameObject.activeInHierarchy);
            var button=found!=null?found.GetComponent<Button>():null;
            return button!=null&&!button.interactable?null:found;
        }
    }
}
