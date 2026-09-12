using System.Linq;
using UnityEngine;

namespace Hellscript
{
    public sealed partial class GameUI
    {
        RunState effectsPauseRun;
        bool effectsWasPaused;
        string EffectName(string id)
        {
            var skill=game.catalog.skills.Find(s=>s.id==id);if(skill!=null)return skill.name;
            if(id!=null&&id.Length==4&&id[1]=='P'&&int.TryParse(id.Substring(2),out int passive)&&passive>=1&&passive<=6)
            {int hero=id[0]=='W'?0:id[0]=='A'?1:id[0]=='M'?2:-1;if(hero>=0)return GameCatalog.Passives[hero*6+passive-1];}
            for(int kind=(int)BossAttack.Basic;kind<=(int)BossAttack.Legacy;kind++)if(BossCombat.Definition(kind)==id)return BossCombat.Name(kind);
            if(id!=null&&id.Length==6&&id.StartsWith("BOSS0")&&int.TryParse(id.Substring(5),out int boss)&&boss>=1&&boss<=3)return GameCatalog.BossNames[boss-1];
            if(id!=null&&id.StartsWith("N")&&id.Length>=3&&int.TryParse(id.Substring(1,2),out int number)&&number>=1&&number<=12)return Loc.F("{0}{1}", GameCatalog.EnemyNames[number-1], (id.EndsWith("DEATH")?" · 사망 효과":" · 공격"));
            if(id!=null&&id.StartsWith("E")&&id.Length==3&&int.TryParse(id.Substring(1),out int trait)&&trait>=1&&trait<=6)return new[]{"추적 화염","얼음 고리","생명 연결","사격 방벽","시체 폭발","분노 축적"}[trait-1];
            switch(id)
            {
                case "BASIC":return "기본 공격";case "SW4":return "소용돌이 세트 추가타";case "SA4":return "맹독 세트 추가 독";case "SM4":return "서리 세트 추가타";
                case "SWB4":return "도약 세트 추가타";case "SAB4":return "관통 잔영";case "LEGACY_SHIELD":return "이전 버전에서 유지한 보호막";
                default:var item=ItemCatalog.Unique(id);return item!=null?item.Name:id!=null&&id.StartsWith("ENEMY")?"적 공격":"기타 효과";
            }
        }
        public void ShowCombatEffects()
        {
            if(game.Combat==null)return;var run=game.Combat.State;
            if(Page!="effects"||effectsPauseRun!=run){effectsPauseRun=run;effectsWasPaused=run.paused;}
            bool paused=effectsWasPaused;run.paused=true;
            pageRepaint=()=>ShowCombatEffects();Base("effects","전투 효과","확인하는 동안 전투 시간이 멈춥니다.");
            BigButton(content,"적의 행동과 위험 확인",()=>{run.paused=paused;ShowEnemyCombat();});
            if(game.Combat.Stats.specials.Contains("LW01"))
            {
                var last=run.effectEvents.LastOrDefault(e=>e.definitionId=="LW01"&&e.kind=="PULLED");
                if(last==null)Note(content,"소용돌이의 송곳니\n회오리 1초 유지 후 유료 틱에서 적을 끌어당깁니다.",20,94,gold);
                else
                {
                    var pulls=run.effectEvents.Where(e=>e.definitionId=="LW01"&&e.kind=="PULLED"&&e.tick==last.tick).ToArray();
                    Note(content,Loc.F("소용돌이의 송곳니\n최근 {0:0.00}초 · {1}명 끌어당김\n실제 이동 {2:0.00}~{3:0.00}m", last.time, pulls.Length, pulls.Min(e=>e.value), pulls.Max(e=>e.value)),20,120,gold);
                }
            }
            if(run.heroDeathRecorded)Note(content,run.bossRewarded?"보스와 영웅이 동시에 사망했습니다.\n보스 처치를 우선하여 클리어 보상을 지급했습니다.":"영웅의 사망을 기록했습니다.",20,88,gold);
            Note(content,Loc.F("현재 보호막 {0:0} · 최대 HP {1:0}", run.shield, game.Combat.Stats.hp),23,68,gold);
            foreach(var shield in run.shields.OrderBy(s=>s.remaining).ThenBy(s=>s.id))
                Note(content,Loc.F("{0}\n남은 양 {1:0} · {2:0.0}초\n누적 흡수 {3:0}{4}", EffectName(shield.definitionId), shield.amount, shield.remaining, shield.absorbed, (shield.definitionId=="M05"&&game.Combat.Stats.specials.Contains("LM03")?Loc.F(" / 환류 기준 {0:0}{1}", shield.createdMaxHp*.15f, (shield.manaPaid?" · 회복 완료":shield.absorbed>=shield.createdMaxHp*.15f?Loc.F(" · 대기 {0:0.0}초", run.itemEffects.lm03Cooldown):"")):"")),20,112,pale);
            if(run.shields.Count==0)Note(content,"유지 중인 보호막이 없습니다.",20,60,pale);
            foreach(var field in run.effects.Where(f=>!f.hostile&&f.kind==13&&(string.IsNullOrEmpty(f.casterId)||f.casterId==run.heroId)).OrderBy(f=>f.id))
                Note(content,Loc.F("눈보라 · {0} · {1:0.0}초\n{2}", (field.followsTarget?"목표 추적":"위치 고정"), field.duration, (field.followsTarget?Loc.F("이동 {0:0.0} / 4.0m", field.moved):"생성 위치를 유지합니다.")),20,88,gold);
            foreach(var enemy in run.enemies.Where(e=>!e.dead&&(e.exposure>0||e.frostMarkTime>0||e.frostCooldown>0||e.setPoisonTime>0)).OrderBy(e=>e.id))
            {
                string name=enemy.boss?GameCatalog.BossNames[enemy.pattern]:GameCatalog.EnemyNames[enemy.kind];
                string frost=enemy.frostMarkTime>0?Loc.F("서리 노출 · {0:0.0}초 남음", enemy.frostMarkTime):enemy.frostCooldown>0?Loc.F("서리 노출 · 재생성 대기 {0:0.0}초", enemy.frostCooldown):enemy.exposure>0?Loc.F("서리 노출 누적 · {0:0.0} / 2.0초", enemy.exposure):"";
                string poison=enemy.setPoisonTime>0?Loc.F("세트 맹독 · {0:0.0}초 / 다음 피해 {1:0.0}초", enemy.setPoisonTime, enemy.setPoisonTick):"";
                Note(content,Loc.T(name)+"\n"+frost+(frost!=""&&poison!=""?"\n":"")+poison,20,poison!=""&&frost!=""?112:88,pale);
            }
            var charges=run.itemEffects;var action=run.heroAction;
            if(charges.leapDefense>0)Note(content,Loc.F("착지 자세 · {0:0.00}초\n받는 피해 감소 +15%p", charges.leapDefense),20,88,gold);
            if(charges.moveBuff>0&&game.Combat.Hero.heroClass==HeroClass.Ranger&&game.Combat.Stats.passives[1])Note(content,Loc.F("탈출의 발걸음 · {0:0.00}초\n이동속도 증가 +20%p", charges.moveBuff),20,88,gold);
            if(charges.elementBuff>0)Note(content,Loc.F("원소 교차 · {0:0.00}초\n가산 피해 +10% · 중첩 없음", charges.elementBuff),20,88,gold);
            if(action.phase==HeroActionPhase.Channeling&&game.Combat.Hero.heroClass==HeroClass.Warrior&&game.Combat.Stats.passives[1])
                Note(content,run.channelTime>=2-.00001f?"끝나지 않는 회전 · 비용 감소 +20%\n다른 비용 감소와 합산하며 상한은 50%입니다.":Loc.F("끝나지 않는 회전 · {0:0.00} / 2.00초 유지", run.channelTime),20,88,gold);
            if(charges.whirlwindCharge>0)Note(content,Loc.F("회오리 충격 · {0:0.0}초\n다음 도약의 이동 시작에 예약합니다.", charges.whirlwindCharge),20,88,gold);
            if(charges.crushCharge>0)Note(content,Loc.F("분쇄 준비 · {0:0.0}초", charges.crushCharge),20,64,gold);
            if(charges.pierceCharge>0)Note(content,Loc.F("긴 사선 · {0:0.0}초", charges.pierceCharge),20,64,gold);
            if(charges.chainCharges>0)Note(content,Loc.F("과충전 {0}회 · {1:0.0}초", charges.chainCharges, charges.chainCharge),20,64,gold);
            if(charges.lc02Charge>0)Note(content,Loc.F("절제 · {0:0.0}초\n다음 자원 소모 스킬의 비용 감소 +50%", charges.lc02Charge),20,88,gold);
            else if(charges.basicCount>0)Note(content,Loc.F("절제 준비 · 같은 대상 기본공격 {0} / 3회", charges.basicCount),20,64,pale);
            else if(game.Combat.Stats.specials.Contains("LC02")&&charges.procCooldown>0)Note(content,Loc.F("절제 · 재충전 대기 {0:0.0}초", charges.procCooldown),20,64,pale);
            if(charges.ap05Ready)Note(content,"절약된 집중 · 준비 완료\n다음 자원 소모 스킬의 비용 감소 +25%",20,88,gold);
            else if(charges.ap05Cooldown>0)Note(content,Loc.F("절약된 집중 · 대기 {0:0.0}초", charges.ap05Cooldown),20,64,pale);
            if(charges.lc02Charge>0||charges.ap05Ready)Note(content,"비용 감소는 합산 최대 50%입니다.\n유료 시전 시 준비된 할인 효과를 함께 사용합니다.",18,78,pale);
            if(!action.released)
            {
                string reserved=action.whirlwindReserved?"회오리 충격 · 현재 도약에 예약됨":action.crushBonus?"분쇄 추가타 · 현재 분쇄에 예약됨":action.pierceBonus?"관통 잔영 · 현재 사격에 예약됨":action.chainBonus>0?"연쇄 +2회 · 현재 시전에 예약됨":"";
                if(reserved!="")Note(content,reserved,20,68,gold);
            }
            foreach(var boss in run.enemies.Where(e=>e.boss&&!e.dead))
            {var c=boss.bossControl;Note(content,c.staggered>0?Loc.F("보스 제압 · {0:0.0}초 남음", c.staggered):c.immunity>0?Loc.F("보스 제압 면역 · {0:0.0}초 남음", c.immunity):Loc.F("보스 제압 게이지 {0:0.0} / 100", c.meter),22,68,gold);}
            Note(content,"최근 피해 20건",23,64,gold);
            foreach(var hit in run.damageEvents.AsEnumerable().Reverse().Take(20))
            {
                string element=new[]{"물리","화염","냉기","번개","독","암흑"}[Mathf.Clamp(hit.element,0,5)];
                string detail=hit.incoming?Loc.F("보호막 흡수 {0:0.##} · HP 감소 {1:0.##}", hit.absorbed, hit.hpLoss):Loc.F("최종 피해 {0:0.##} · HP 감소 {1:0.##}", hit.finalDamage, hit.hpLoss);
                Note(content,Loc.F("{0:0.00}초 · {1}\n{2} {3} 피해{4}\n방어 전 {5:0.##} · 방어 감소 {6:P0}\n추가 피해 감소 {7:P0}\n{8}", hit.time, EffectName(hit.definitionId), (hit.incoming?"받은":"가한"), element, (hit.critical?" · 극대화":""), hit.attackBeforeDefense, hit.defenseReduction, hit.buffReduction, detail),19,155,pale);
            }
            if(run.damageEvents.Count==0)Note(content,"다음 피해부터 기록됩니다.",20,64,pale);
            FooterButton(0,1,"전투로 돌아가기",()=>{run.paused=paused;ShowBattle();});
        }
    }
}
