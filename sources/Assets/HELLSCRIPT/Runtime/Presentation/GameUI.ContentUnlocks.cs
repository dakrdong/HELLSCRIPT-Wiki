using System;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    public sealed partial class GameUI
    {
        bool RequireContent(string id)
        {if(ContentUnlocks.Has(game.Store.Data,id))return true;ShowToast(ContentUnlocks.Condition(id));return false;}
        void ContentButton(string id,string title,Action action,bool accent=false)
        {
            bool open=ContentUnlocks.Has(game.Store.Data,id);
            BigButton(content,title,()=>{if(RequireContent(id))action();},accent).interactable=open;
            if(!open)Note(content,ContentUnlocks.Condition(id),18,100,muted);
        }
        public void ShowContentUnlocks()
        {
            var a=game.Store.Data;ContentUnlocks.Reconcile(a);
            pageRepaint=ShowContentUnlocks;Base("content-unlocks","콘텐츠 해금 · 안내","공개 단계와 영웅 레벨은 테스트 초안입니다");
            Note(content,Loc.F("계정 최고 실제 클리어 {0}단계 · 선택 영웅 {1}단계",ContentUnlocks.AccountClear(a),a.Hero.highestClear),21,65,pale);
            var next=ContentUnlocks.Rules.features.FirstOrDefault(f=>!ContentUnlocks.Has(a,f.id));
            if(next!=null)Note(content,Loc.F("다음 목표: {0}\n{1}",Loc.T(next.name),ContentUnlocks.Condition(next.id)),20,95,gold);
            Note(content,"직업·상세 스크립트·프리셋·분해·판매·보호·기존 창고는 처음부터 이용할 수 있습니다. 구매 권한은 그대로 유지됩니다.",19,90,pale);
            foreach(var f in ContentUnlocks.Rules.features)
            {
                string id=f.id;bool open=ContentUnlocks.Has(a,id);
                BigButton(content,Loc.F("{0} · {1}",Loc.T(f.name),Loc.T(open?"개방됨":"잠김")),()=>ShowContentGuide(id));
                Note(content,ContentUnlocks.Condition(id),18,string.IsNullOrEmpty(f.early)?55:85,muted);
            }
            if(a.contentUnlocks.legendaryAcquired)EquipmentUnlockGuide("GUIDE_LEGENDARY","첫 전설 장비의 고유 효과를 도감에서 확인하세요. 기존 착용 조건만 충족하면 지금 사용할 수 있습니다.");
            if(a.contentUnlocks.setAcquired)EquipmentUnlockGuide("GUIDE_SET","세트 장비는 서로 다른 부위의 2·4세트 효과를 도감에서 확인하세요. 별도의 콘텐츠 단계 제한은 없습니다.");
            FooterButton(0,1,"성소로",ShowTown);
        }
        public void ShowContentGuide(string id)
        {
            var f=ContentUnlocks.Rules.features.Single(x=>x.id==id);
            pageRepaint=()=>ShowContentGuide(id);Base("content-guide",f.name,"콘텐츠 해금 · 안내");
            Note(content,ContentUnlocks.Condition(id),21,125,gold);Note(content,f.guide,21,180,pale);
            if(ContentUnlocks.Has(game.Store.Data,id)&&!game.Store.Data.contentUnlocks.guidesCompleted.Contains(id))
                BigButton(content,"안내 확인 / 건너뛰기",()=>{game.Store.Transact(Guid.NewGuid().ToString("N"),"content-guide:"+id,staged=>{ContentUnlocks.CompleteGuide(staged,id);return true;});ShowContentGuide(id);});
            FooterButton(0,1,"콘텐츠 해금 · 안내",ShowContentUnlocks);
        }
        void EquipmentUnlockGuide(string id,string text)
        {
            Note(content,text,19,150,pale);BigButton(content,"전설·세트 도감",ShowItemCollection);
            if(!game.Store.Data.contentUnlocks.guidesCompleted.Contains(id))BigButton(content,"안내 확인 / 건너뛰기",()=>
            {game.Store.Transact(Guid.NewGuid().ToString("N"),"content-guide:"+id,staged=>{ContentUnlocks.CompleteGuide(staged,id);return true;});ShowContentUnlocks();});
        }
    }
}
