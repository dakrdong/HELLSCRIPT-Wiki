using System;
using UnityEngine;

namespace Hellscript
{
    public sealed partial class GameUI
    {
        // The rift keeper gathers stage choice, sweep and training in one place. Every action here is
        // the same call the sanctuary menu makes, with the same eligibility, so no route is cheaper.
        RiftEntryWindow riftEntryWindow;
        public void ShowRiftKeeper()
        {
            if(game.Active)return;
            if(riftEntryWindow!=null)return;
            if(!game.Store.ActivateSkillTrees(game.catalog)||!game.Store.RefreshRiftDay()){ShowToast(game.Store.Error);return;}
            riftEntryWindow=RiftEntryWindow.Open(transform,game,()=>InterfaceFactor,()=>{riftEntryWindow=null;RefreshHud();RefreshPlaza();});
        }
        public void CloseRiftEntry(){if(riftEntryWindow!=null)riftEntryWindow.Close();}
        public void ShowRiftServices()
        {
            var a=game.Store.Data;var h=a.Hero;
            pageRepaint=()=>ShowRiftServices();Base("rift-keeper","균열 관리자","단계 선택 · 소탕 · 훈련은 성소 메뉴와 같은 조건으로 열립니다");
            string today=DateTime.UtcNow.ToString("yyyy-MM-dd");int sweeps=a.sweepDay==today?a.sweepCount:0;
            Note(content,Loc.F("{0}   Lv.{1}   최고 실클리어 {2}단계\n오늘 소탕 {3}/3 회 사용   가방 빈칸 {4}칸", game.catalog.classNames[(int)h.heroClass], h.level, h.highestClear, sweeps, Economy.FreeSlots(h)),22,84,pale);
            Note(content,"입장은 최고 실클리어 다음 단계까지 허용합니다. 진행 중인 균열이 있으면 먼저 이어서 마쳐야 새 균열에 들어갑니다.",19,66,muted);
            var stage=Row(content,100);
            var minus=Button(stage,"−",()=>{game.SelectedStage=Mathf.Max(1,game.SelectedStage-1);ShowRiftKeeper();});Place((RectTransform)minus.transform,12,10,TouchHeight,TouchHeight);
            var st=Label(stage,Loc.F("균열 {0:00}단계", game.SelectedStage),28,gold,TextAnchor.MiddleCenter);Span(st.rectTransform,102,19,102,62);
            var plus=Button(stage,"+",()=>{game.SelectedStage=Mathf.Min(h.highestClear+1,game.SelectedStage+1);ShowRiftKeeper();});Right((RectTransform)plus.transform,12,10,TouchHeight,TouchHeight);
            BigButton(content,"단계별 등급 확률",ShowRiftRewards);
            AddRepeatPreparation();
            if(!game.Running&&a.repeatHunt?.pendingResult!=null)BigButton(content,"저장된 반복 결과 확인",game.ResumeRepeatResult,true);
            if(a.suspendedRun!=null)BigButton(content,"진행 중인 균열 이어하기",()=>game.Begin(resume:true),true);
            else BigButton(content,"균열에 진입",()=>game.Begin(),true);
            bool sweepable=h.highestClear>=1&&sweeps<3&&Economy.FreeSlots(h)>=3;
            Note(content,sweepable?"소탕은 최고 실클리어 단계의 보상을 즉시 정산합니다. 하루 3회, 가방 3칸이 필요합니다.":"소탕에는 실클리어 기록, 남은 소탕 횟수, 가방 3칸이 필요합니다.",19,66,sweepable?pale:muted);
            ContentButton(ContentUnlocks.Sweep,"최고 단계 소탕",SweepAction(ShowRiftKeeper));
            ContentButton(ContentUnlocks.Train,"고정 훈련장",ShowTraining);
            ContentButton(ContentUnlocks.Train,"같은 조건으로 A/B 비교",ShowComparisonPicker);
            FooterButton(0,2,"성소 메뉴",ShowTownMenu);FooterButton(1,2,"광장으로",()=>game.EnterPlaza());
        }
        Action SweepAction(Action refresh)
        {
            string request=Guid.NewGuid().ToString("N");uint seed=(uint)DateTime.UtcNow.Ticks;
            return ()=>{bool ok=game.Store.SweepRift(request,seed);string error=game.Store.Error;refresh();ShowToast(ok?"소탕 보상을 받았습니다.":Loc.F("소탕하지 못했습니다. 실클리어·남은 횟수·장비 3칸·보석 공간을 확인하세요.\n{0}",error));};
        }
    }
}
