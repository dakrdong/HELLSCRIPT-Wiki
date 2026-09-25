using System;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    public sealed partial class GameUI
    {
        UnityEngine.UI.Text guideBuildDifference;
        void AddTownGuide()
        {
            var next=Tutorials.Next(game.Store.Data);if(next==null)return;
            BigButton(content,Loc.F("모험 안내 · {0}",next.Title),ShowOnboarding,true);
        }
        public void ShowOnboarding()=>ShowTutorialJournal();
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
