using System;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    public sealed partial class GameUI
    {
        int guideStep;
        RunState guideRun;
        bool guideWasPaused;
        UnityEngine.UI.Text guideBuildDifference;
        static readonly string[] GuideDescriptions={
            "이동·공격·장비 수집은 자동입니다. 플레이어는 행동 설계에서 위에서부터 검사할 규칙과 조건을 정합니다. 1배·1.5배·2배는 공격뿐 아니라 균열 제한시간도 함께 흐르게 합니다.",
            "직업마다 두 가지 추천 빌드가 있습니다. 각 빌드가 어떤 스킬을 연결하는지 살펴보세요. 현재 레벨에서 쓸 수 있는 추천안을 미리 보고 적용할 수 있습니다. 성장하면 새로운 스킬이 열립니다.",
            "현재 캐릭터로 균열을 탐험합니다. 처치 게이지 100을 채우면 보스가 등장합니다. 제한시간은 300전투초이며, 배속에 따라 실제 소요 시간이 달라집니다.",
            "전투 화면 아래의 현재 행동을 누르면 판단 기록이 열립니다. 실행 선택·조건 미충족·사거리·자원 부족을 확인하세요. 선택된 규칙이 남아 있으면 바로 편집기로 이동할 수 있습니다.",
            "균열의 성공 또는 실패 결과를 확인하세요. 이미 얻은 XP·장비·재화는 유지됩니다. 개봉하지 않은 상자는 지급하지 않습니다. 결과의 마지막 기록과 성장 화면에서 다음 변경을 정합니다.",
            "현재 장비와 후보의 요구 레벨·접두사·접미사·전설·세트 효과를 비교하세요. 공격 기준 하나만으로 우열을 정하지 않습니다. 지금 바꿀 후보가 없다면 비교할 항목부터 확인합니다.",
            "행동 설계에서 목표 거리나 물약을 마실 HP 조건을 하나 바꿔 보세요. 적용 전후 설명을 확인한 뒤 설정 적용을 누릅니다. 마음에 들지 않으면 취소하고 다른 조건을 골라도 됩니다.",
            "바꾼 설정을 고정 훈련장에서 시험하세요. 현재 레벨과 장비로 60전투초 또는 사망까지 진행합니다. 피해와 생존 시간, 행동의 이유를 확인하세요. 훈련에서는 경험치나 전리품을 얻지 않습니다.",
            "수정한 설정을 훈련했다면 새 균열에 도전하세요. 아래에서 반복 여부와 성공·실패 후의 행동, 가방이 찼을 때의 처리 방법을 확인할 수 있습니다. 필요하면 반복 정책을 바꾸고 출발하세요."
        };
        void AddTownGuide()
        {
            var a=game.Store.Data;if(a.guide.hintsHidden||FirstPlayGuide.Complete(a,a.Hero))return;
            BigButton(content,Loc.F("첫 플레이 안내 {0}/9 · {1}", FirstPlayGuide.Count(a,a.Hero), FirstPlayGuide.Titles[(int)FirstPlayGuide.Next(a,a.Hero)]),ShowOnboarding,true);
        }
        public void ShowOnboarding()
        {
            var a=game.Store.Data;guideRun=game.Combat?.State;guideWasPaused=guideRun?.paused??false;if(guideRun!=null)guideRun.paused=true;
            guideStep=FirstPlayGuide.Complete(a,a.Hero)?8:(int)FirstPlayGuide.Next(a,a.Hero);RenderGuide();
        }
        void GuideGo(Action next,bool town=false)
        {
            if(town&&(game.Active||game.Store.Data.suspendedRun!=null)){ShowToast("진행 중인 균열을 먼저 완료한 뒤 이용할 수 있습니다.");return;}
            if(guideRun!=null&&game.Combat?.State==guideRun)guideRun.paused=guideWasPaused;
            if(town&&game.Combat!=null)game.ReturnTown();next();
        }
        void CloseGuide()=>GuideGo(()=>{if(game.Active)ShowBattle();else if(game.Combat!=null)ShowResult();else ShowTown();});
        void GuideButton(string title,Action next,bool town=false)
        {var b=BigButton(content,title,()=>GuideGo(next,town),true);b.interactable=!town||!game.Active&&game.Store.Data.suspendedRun==null;}
        void RenderGuide()
        {
            var a=game.Store.Data;var h=a.Hero;var step=(GuideStep)guideStep;bool done=FirstPlayGuide.Done(a,h,step);
            pageRepaint=()=>RenderGuide();Base("onboarding","첫 플레이 안내",Loc.F("{0} · 안내 {1}/9 완료", game.catalog.classNames[(int)h.heroClass], FirstPlayGuide.Count(a,h)));
            Cycle(content,"단계",FirstPlayGuide.Titles,guideStep,i=>{guideStep=i;RenderGuide();});
            Note(content,Loc.F("{0:00} · {1} · {2}", guideStep+1, FirstPlayGuide.Titles[guideStep], (done?"확인됨":"진행할 내용")),25,70,gold);
            Note(content,GuideDescriptions[guideStep],22,185,pale);
            if(step==GuideStep.Controls)BigButton(content,"자동 전투 안내 확인",()=>{game.RecordGuide(()=>FirstPlayGuide.ReadControls(a));guideStep=1;RenderGuide();},true);
            if(step==GuideStep.Build)
            {
                for(int v=0;v<2;v++)
                {var build=GameCatalog.Preset(h.heroClass,v);int unlock=build.activeSkills.Max(i=>game.catalog.skills[i].unlock);Note(content,Loc.F("{0} · 주요 스킬 완성 Lv.{1}\n{2}", build.name, unlock, BehaviorPresets.Concepts[(int)h.heroClass*2+v]),21,115,gold);}
                Note(content,Loc.F("현재 무기: {0}", (h.inventory.FirstOrDefault(i=>i.equipped&&i.slot==0)?.DisplayName??"없음")),20,74,pale);
                GuideButton("현재 스킬과 성장 확인",ShowGrowth);
            }
            if(step==GuideStep.Rift)
            {
                if(game.Active)GuideButton("진행 중인 전투로",ShowBattle);
                else if(a.suspendedRun!=null)GuideButton("저장된 균열 이어하기",()=>game.Begin(resume:true));
                else GuideButton("첫 균열에 진입",()=>game.Begin(),true);
            }
            if(step==GuideStep.Decision)
            {
                if(game.Combat!=null)GuideButton("현재 행동 판단 기록",ShowRuleDecisions);
                else Note(content,"균열에 입장한 뒤 현재 행동을 눌러 기록을 확인하세요.",21,90,gold);
            }
            if(step==GuideStep.Result)
            {
                if(game.Combat!=null&&!game.Active&&game.Combat.State.training<0)GuideButton("현재 균열 결과",ShowResult);
                else Note(content,"현재 균열을 마치면 결과가 열립니다. 기존 기록은 성소의 전투 기록에서도 확인할 수 있습니다.",21,110,gold);
                GuideButton("성장과 다음 해금",ShowGrowth);
            }
            if(step==GuideStep.Equipment)
            {
                var candidate=FirstPlayGuide.ComparisonCandidate(h);
                if(candidate!=null){Note(content,Loc.F("소유 후보: {0}", candidate.DisplayName),21,86,gold);GuideButton("소유 장비 상세와 비교",()=>ShowItemDetail(candidate.id),true);}
                else
                {
                    Note(content,"지금 장착할 수 있는 미장착 후보가 없습니다. 비교할 때는 요구 레벨을 먼저 보고, 무기 피해·방어·HP와 자원 회복, 접사와 세트가 켜지거나 꺼지는지를 확인하세요. 거리 유지 설정과 스킬 사거리는 장비 공격력과 별개입니다.",21,190,gold);
                    BigButton(content,"장비 비교 설명 확인",()=>{game.RecordGuide(()=>FirstPlayGuide.ReadEquipmentExplanation(h));RenderGuide();},true);
                    var weapon=h.inventory.FirstOrDefault(i=>i.equipped&&i.slot==0);if(weapon!=null)GuideButton("현재 무기 상세",()=>ShowItemDetail(weapon.id),true);
                }
                if(done)Note(content,h.guide.equipmentExplanation?"기록: 소유 후보가 없어 비교 설명을 확인했습니다.":"기록: 실제 소유 장비의 비교 화면을 확인했습니다.",20,88,pale);
            }
            if(step==GuideStep.Edit)
            {
                GuideButton("행동 설계에서 직접 수정",ShowBuild);
                if(!string.IsNullOrEmpty(h.guide.changeSummary))Note(content,Loc.F("최근 적용한 변경\n{0}", NamedConditions(h.guide.changeSummary)),20,Mathf.Max(150,h.guide.changeSummary.Length/32f*25+75),gold);
            }
            if(step==GuideStep.Training)
            {
                if(!FirstPlayGuide.Done(a,h,GuideStep.Edit))Note(content,"먼저 실제 행동 설정의 조건을 수정하고 적용해 주세요.",21,90,gold);
                GuideButton("고정 훈련장으로",ShowTraining,true);
                Note(content,"이번에는 적용한 설정을 그대로 훈련하고 결과를 확인하세요. 피해가 늘지 않아도 원하는 조건에서 행동했는지, 더 오래 생존했는지 살펴보면 좋습니다.",20,150,pale);
            }
            if(step==GuideStep.Retry)
            {
                var b=h.build;
                Note(content,Loc.F("자동 반복: {0}\n성공 후: {1}\n연속 실패 {2}회에 중단\n가방 정책: {3}", (b.autoRepeat?"켜짐":"꺼짐"), (b.advanceOnWin?"다음 단계":"같은 단계"), b.stopAfterFailures, BagPolicyDescription(b)),22,165,gold);
                GuideButton("현재 설정으로 새 균열",()=>game.Begin(),true);GuideButton("반복 정책 직접 확인",ShowBuild);
            }
            if(game.Active)Note(content,"안내를 보는 동안 전투가 멈춥니다. 장비·훈련·새 판은 현재 균열을 마친 뒤 이용하세요.",19,90,muted);
            if(guideStep<8)BigButton(content,"다음 안내",()=>{guideStep++;RenderGuide();});
            Note(content,"원하는 단계부터 둘러보거나 안내를 숨기고 바로 플레이해도 됩니다. 게임 안내에서 언제든 다시 열어 이어서 확인할 수 있습니다.",19,115,muted);
            FooterButton(0,2,a.guide.hintsHidden?"안내 다시 표시":"안내 숨기기",()=>{game.RecordGuide(()=>FirstPlayGuide.HideHints(a,!a.guide.hintsHidden));CloseGuide();});
            FooterButton(1,2,"닫기",CloseGuide,true);
        }
        string BagPolicyDescription(BuildConfig b)
        {
            switch(b.bagPolicy){case BagPolicy.Portal:return "포탈로 성소에서 정리";case BagPolicy.Replace:return "미보호 장비와 비교해 자동 교체";default:return "남은 장비 획득 중단";}
        }
        void AddGuideBuildDifference()
        {
            guideBuildDifference=null;
            var a=game.Store.Data;if(a.guide.hintsHidden||FirstPlayGuide.Complete(a,a.Hero))return;
            guideBuildDifference=Note(content,"",19,70,gold);RefreshGuideBuildDifference();
        }
        void RefreshGuideBuildDifference()
        {
            if(comparisonEditing){RefreshComparisonDifference();return;}
            if(Page!="build"||guideBuildDifference==null)return;
            var before=game.Active?game.Combat.State.build:game.Store.Data.Hero.build;string change=FirstPlayGuide.DescribeChange(before,editing,game.catalog);
            guideBuildDifference.text=Loc.T(change==""?"아직 전투 행동을 바꾸지 않았습니다. 조건을 바꾸면 적용 전후의 설명이 여기에 표시됩니다.":Loc.F("적용 전 확인\n{0}", NamedConditions(change)));
            var layout=guideBuildDifference.transform.parent.GetComponent<UnityEngine.UI.LayoutElement>();layout.minHeight=layout.preferredHeight=change==""?80:Mathf.Max(110,change.Length/34f*25+60);
        }
        void RefreshGuideHint()
        {
            if(Page!="battle"||toastTime>0||!game.Active||game.Combat.State.training>=0)return;
            var a=game.Store.Data;var h=a.Hero;if(a.guide.hintsHidden||h.guide.firstSkillHintShown||FirstPlayGuide.Done(a,h,GuideStep.Decision))return;
            var first=game.Combat.State.actionEvents.FirstOrDefault(e=>e.kind=="ACTION_START"&&e.skill>=0);if(first==null)return;
            ShowToast(Loc.F("{0} 사용 · 아래 현재 행동을 눌러 판단 이유를 확인하세요.", game.catalog.skills[first.skill].name));
            h.guide.firstSkillHintShown=true;game.Save();
        }
    }
}
