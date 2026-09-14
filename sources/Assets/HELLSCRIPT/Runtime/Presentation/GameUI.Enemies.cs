using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed partial class GameUI
    {
        RectTransform bossHud;
        Image bossHealthFill;
        Text bossTitle,bossActionLabel;
        void AddBossHud(RectTransform parent)
        {
            bossHud=Box("Boss status",parent,new Color(.025f,.035f,.05f,.94f));Place(bossHud,24,148,648,64);
            bossTitle=Label(bossHud,"",19,gold);Place(bossTitle.rectTransform,8,2,632,27);
            bossActionLabel=Label(bossHud,"",17,pale);Place(bossActionLabel.rectTransform,8,29,632,24);
            bossHealthFill=Bar(bossHud,new Vector2(8,56),new Vector2(632,5),new Color(.7f,.16f,.16f));
        }
        void RefreshBossHud(EnemyState boss)
        {
            if(bossHud==null)return;bossHud.gameObject.SetActive(boss!=null);if(boss==null)return;
            bool visible=Vector2.Distance(game.Combat.State.position,boss.position)<=12&&game.Combat.Map.LineClear(game.Combat.State.position,boss.position);
            bossTitle.text=visible?Loc.F("{0} · HP {1:0.#}% · {2}", GameCatalog.BossNames[boss.pattern], 100*boss.health/boss.maxHealth, (boss.brain.boss.enraged?"후반":"전반")):Loc.F("{0} · 위치 확인 중", GameCatalog.BossNames[boss.pattern]);
            var a=boss.brain.action;bossActionLabel.text=Loc.T(!visible?"표시된 방향으로 접근하세요.":boss.bossControl.staggered>0?"제압되어 행동을 멈췄습니다.":Loc.T(boss.brain.state)+(a.phase==EnemyActionPhase.Preparing?Loc.F(" · {0:0.00}초 뒤 실행", a.remaining):""));
            bossHealthFill.gameObject.SetActive(visible);Fill(bossHealthFill,boss.health/boss.maxHealth);ReflowBossText();
        }
        void ReflowBossText()
        {
            if(Page!="battle"||bossHud==null)return;
            float width=bossHud.rect.width;
            bossTitle.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,width);
            bossActionLabel.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,width);
            float titleHeight=Mathf.Max(25,bossTitle.preferredHeight),actionHeight=Mathf.Max(30,bossActionLabel.preferredHeight);
            Place(bossTitle.rectTransform,0,0,width,titleHeight);
            Place(bossActionLabel.rectTransform,0,titleHeight+2,width,actionHeight);
            Place((RectTransform)bossHealthFill.transform.parent,0,titleHeight+actionHeight+7,width,4);
            bossHud.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,titleHeight+actionHeight+11);
        }
        public void ShowEnemyCombat()
        {
            if(game.Combat==null)return;var run=game.Combat.State;bool paused=run.paused;run.paused=true;pageRepaint=()=>ShowEnemyCombat();Base("enemy-combat","적의 행동과 위험","확인하는 동안 전투 시간이 멈춥니다.");
            if(run.enemySlowTime>0)Note(content,Loc.F("얼음 고리 감속 · 이동속도 -35%\n{0:0.0}초 남음", run.enemySlowTime),21,90,pale);
            foreach(var e in run.enemies.Where(e=>!e.dead&&Vector2.Distance(run.position,e.position)<=12&&game.Combat.Map.LineClear(run.position,e.position)).OrderBy(e=>(e.position-run.position).sqrMagnitude).Take(12))
            {
                var b=e.brain;var a=b.action;string name=e.boss?GameCatalog.BossNames[e.pattern]:GameCatalog.EnemyNames[e.kind];
                Note(content,Loc.F("{0}\n{1} · HP {2:0} / {3:0}", name, (e.boss&&e.bossControl.staggered>0?"제압됨":b.state), e.health, e.maxHealth),22,94,gold);
                if(a.phase!=EnemyActionPhase.Idle)Note(content,Loc.F("{0}\n조준 방향 고정{1}", (a.phase==EnemyActionPhase.Preparing?Loc.F("예고 {0:0.00}초", a.remaining):a.phase==EnemyActionPhase.Charging?"돌진 중":Loc.F("효과 진행 {0:0.00}초", a.remaining)), (a.kind==7||a.kind==(int)BossAttack.Charge?Loc.F(" · 남은 돌진 {0}회", a.remainingCharges):a.kind==(int)BossAttack.Slam?Loc.F(" · 남은 타격 {0}회", a.remainingCharges):"")),19,88,pale);
                if(e.boss)
                {
                    Note(content,Loc.F("{0} · 제압 {1:0}/100\n{2}", (b.boss.enraged?"후반 패턴":"전반 패턴"), e.bossControl.meter, (e.bossControl.staggered>0?Loc.F("무력화 {0:0.0}초", e.bossControl.staggered):e.bossControl.immunity>0?Loc.F("제압 면역 {0:0.0}초", e.bossControl.immunity):"제압 가능")),19,88,gold);
                    if(e.pattern==1)Note(content,Loc.F("살아 있는 소환 부하 {0}/8\n부하가 있으면 받는 피해 20% 감소", run.enemies.Count(x=>!x.dead&&BossCombat.OwnAdd(x,e))),19,88,pale);
                    if(b.boss.refuges.Count>0)Note(content,Loc.F("폭발을 피할 지점 {0}곳\n파란 윤곽은 이번 연속 폭발의 범위 밖입니다.", b.boss.refuges.Count),19,88,pale);
                }
                string traits=string.Join(" / ",Enumerable.Range(0,6).Where(i=>EnemyCombat.Trait(e,i)).Select(i=>new[]{"추적 화염","얼음 고리","생명 연결","사격 방벽","시체 폭발","분노 축적"}[i]));
                if(traits!="")Note(content,traits,19,64,pale);
                if(b.auraSource>=0)Note(content,"종지기 오라 · 공격 +20%",19,56,gold);
                if(b.rageStacks>0)Note(content,Loc.F("분노 {0}/5 · 공격 +{1}%", b.rageStacks, b.rageStacks*10),19,56,gold);
                if(b.rearWindow>0)Note(content,Loc.F("후방 취약 · {0:0.00}초", b.rearWindow),19,56,gold);
            }
            Note(content,Loc.F("진행 중인 장판·사망 예고 {0}개", run.enemyHazards.Count),21,70,gold);
            foreach(var e in run.enemyEvents.AsEnumerable().Reverse().Take(24))Note(content,Loc.F("{0:0.00}초 · {1}\n{2}", e.time, EffectName(e.definitionId), EnemyEventName(e.kind)),18,84,pale);
            FooterButton(0,1,"전투로 돌아가기",()=>{run.paused=paused;ShowBattle();});
        }
        static string EnemyEventName(string kind)
        {
            if(kind.StartsWith("STATE:"))return kind.Substring(6);if(kind.StartsWith("INTERRUPTED:"))return Loc.F("중단 · {0}", kind.Substring(12));
            switch(kind){case "PREPARE":return "공격 예고 시작";case "RELEASE":return "공격 실행";case "ACTION_END":return "동작 완료";case "FOLLOWUP_PREPARE":return "후속 공격 예고";case "HAZARD_CREATED":return "장판·사망 효과 생성";case "CORPSE_CONSUMED":return "사체 사용 확정";case "RAGE":return "분노 누적";case "HEAL":return "아군 회복";case "AURA_APPLIED":return "공격 강화 적용";case "AURA_REMOVED":return "공격 강화 해제";case "ENRAGED":return "후반 패턴으로 전환";case "SUMMONED":return "부하 소환 완료";case "PLAN_DEFERRED":return "안전한 폭발 배치 재검토";default:return kind;}
        }
    }
}
