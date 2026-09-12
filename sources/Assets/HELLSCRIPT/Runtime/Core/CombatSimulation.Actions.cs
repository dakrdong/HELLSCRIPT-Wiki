using System;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    public sealed partial class CombatSimulation
    {
        // Describe the live phase in the currently selected language. Stored display text may
        // have been formatted in another language before a pause or an app restart.
        public string CurrentActionText
        {
            get
            {
                var a=State.heroAction;string name=a.skill<0?"기본 공격":catalog.skills[a.skill].name;
                switch(a.phase)
                {
                    case HeroActionPhase.Preparing:return Loc.F("{0} · 준비 {1:0.00}초",name,Mathf.Max(0,a.prepare-a.elapsed));
                    case HeroActionPhase.Travelling:return Loc.F("{0} · 이동 중",name);
                    case HeroActionPhase.Recovering:return Loc.F("{0} · 동작 마무리",name);
                    case HeroActionPhase.Channeling:return Loc.F("회오리 · {0:0.0}초 유지",State.channelTime);
                    default:return Loc.StoredText(State.action);
                }
            }
        }
        public bool HeroActionBusy=>State.heroAction.phase!=HeroActionPhase.Idle;
        public bool HeroTravelling=>State.heroAction.phase==HeroActionPhase.Travelling;
        public float HeroAirHeight=>HeroTravelling?Mathf.Sin(Mathf.Clamp01(State.heroAction.elapsed/Mathf.Max(.001f,State.heroAction.travel))*Mathf.PI)*2.5f:0;
        void ActionEvent(HeroActionState action,string kind,string reason)
        {
            CombatTelemetry.Action(State.statistics,action.skill,kind);
            if(kind=="ACTION_TRAVEL"||action.skill==15&&kind=="ACTION_RELEASE")ObserveMovementStart(action);
            State.actionEvents.Add(new CombatActionEvent{actionId=action.id,skill=action.skill,targetId=action.targetId,time=State.time,resource=State.resource,kind=kind,reason=reason,buildVersion=action.policy?.buildVersion??State.build.version,position=State.position});
            if(State.actionEvents.Count>600)State.actionEvents.RemoveAt(0);
            Log(kind,Loc.F("{0} · {1}", (action.skill<0?"기본 공격":catalog.skills[action.skill].name), reason));
        }
        bool CanInterruptWith(int rule,SkillDefinition skill,Rule config)
        {
            if(!HeroActionBusy)return true;
            var a=State.heroAction;if(a.phase==HeroActionPhase.Travelling||a.phase==HeroActionPhase.Recovering)return false;
            if(a.exitChannelForBuild||a.exitChannelForSurvival)return false;
            var source=a.policy?.rule;
            int priority=source==null?a.rule:State.build.rules.FindIndex(r=>r.id==source.id&&r.action==source.action&&r.skill==source.skill);
            if(priority<0&&source!=null&&EdictRuleOrder.IsCompiledRule(source.id))priority=a.rule;
            return priority>=0&&rule<priority&&(config.escape||skill.kind==SkillKind.Shield);
        }
        void StartHeroAction(int index,EnemyState target,Vector2 destination,float cost,int rule,bool escape=false,Rule explicitRule=null,Vector2? aim=null)
        {
            StopEdictWalk();
            if(HeroActionBusy)InterruptHeroAction("상위 생존 행동으로 전환");CancelChest("스킬 실행");CancelShrine("스킬 실행");
            var timing=CombatActions.Timing(index,Hero.heroClass,Stats.attackSpeed);
            var config=explicitRule??(rule>=0&&rule<State.build.rules.Count?State.build.rules[rule]:null);
            var a=new HeroActionState{id=State.nextId++,skill=index,rule=rule,targetId=target?.id??-1,origin=State.position,aim=aim??(GroundSkill(index)?destination:target?.position??State.position),destination=destination,blizzardMode=config?.blizzardMode??BlizzardMode.Follow,
                startedAt=State.time,prepare=timing.prepare,travel=timing.travel,recovery=timing.recovery,cost=index==0?0:cost,escape=escape,
                phase=index==0?HeroActionPhase.Channeling:timing.prepare>0?HeroActionPhase.Preparing:timing.travel>0?HeroActionPhase.Travelling:HeroActionPhase.Recovering};
            a.policy=CombatActions.CapturePolicy(a,State.build,explicitRule);
            CaptureWarriorEdictAction(a,edictSource);
            CaptureRangerEdictAction(a,edictSource);
            if(edictTarget!=null)a.policy.edictTarget=JsonUtility.FromJson<EdictTargetPolicy>(JsonUtility.ToJson(edictTarget));
            State.heroAction=a;State.activeSkill=index;State.resource-=a.cost;ConsumeCostEffects(a.cost,a);ReserveCastCharges(a);
            ObserveShieldOpeningAction(index);
            if(index<0&&target!=null&&lastBasic!=target.id){lastBasic=target.id;basicCount=0;}
            if(index>=0)
            {
                var skill=catalog.skills[index];State.lastSkillStarts[index]=State.time;
                State.cooldowns[index]=skill.cooldown*(1-Mathf.Min(.4f,Stats.cdr+(index==1&&Stats.SetPieces("SWB")>=2?.15f:0)));
                if(index!=4&&index!=5&&index!=11&&index!=16)State.lastAttackTime=State.time;
            }
            else State.lastAttackTime=State.time;
            State.actionCd=timing.prepare+timing.travel+timing.recovery;State.action=Loc.F("{0} · 준비", (index<0?"기본 공격":catalog.skills[index].name));
            ActionEvent(a,"ACTION_START",Loc.F("실행 확정 / 자원 {0:0.##} 사용", a.cost));
            if(index==0){State.channelTime=0;State.channelTick=.25f;State.action="회오리 · 유지";}
            if(a.phase==HeroActionPhase.Travelling)BeginTravel(a);
        }
        void TickHeroAction(float dt)
        {
            var a=State.heroAction;if(a.phase==HeroActionPhase.Idle||a.phase==HeroActionPhase.Channeling)return;
            a.elapsed+=dt;
            if(a.phase==HeroActionPhase.Preparing)
            {
                State.action=Loc.F("{0} · 준비 {1:0.00}초", (a.skill<0?"기본 공격":catalog.skills[a.skill].name), Mathf.Max(0,a.prepare-a.elapsed));
                if(a.elapsed+.00001f<a.prepare)return;a.elapsed=Mathf.Max(0,a.elapsed-a.prepare);
                if(a.travel>0){if(!BeginTravel(a))return;}
                else ReleaseHeroAction(a);
            }
            if(State.heroAction!=a)return;
            if(a.phase==HeroActionPhase.Travelling)
            {
                State.position=Vector2.Lerp(a.origin,a.destination,Mathf.Clamp01(a.elapsed/a.travel));State.destination=a.destination;
                State.action=Loc.F("{0} · 이동 중", catalog.skills[a.skill].name);
                if(a.elapsed+.00001f<a.travel)return;a.elapsed=Mathf.Max(0,a.elapsed-a.travel);
                if(!Map.CanLand(a.destination)||a.skill==1&&!string.IsNullOrEmpty(a.policy?.leapFollowUp)&&!EdictLeapLandingValid(a.origin,a.destination)||a.skill==9&&!string.IsNullOrEmpty(a.policy?.retreatFollowUp)&&!RangerLandingValid(a.origin,a.destination)){State.position=a.origin;InterruptHeroAction("착지 지점이 유효하지 않습니다");return;}
                State.position=a.destination;ReleaseHeroAction(a);
            }
            if(State.heroAction!=a)return;
            if(a.phase==HeroActionPhase.Recovering)
            {State.action=Loc.F("{0} · 동작 마무리", (a.skill<0?"기본 공격":catalog.skills[a.skill].name));if(a.elapsed+.00001f>=a.recovery)CompleteHeroAction(a);}
        }
        void ReleaseHeroAction(HeroActionState a)
        {
            if(a.released)return;
            if(a.skill==8&&a.policy?.keepRangerTraps==true&&LiveRangerTraps().Length>=2)
            {InterruptHeroAction("덫 설치 직전 기존 덫 두 개 유지 방침을 확인했습니다.");return;}
            if(a.skill==10&&a.policy?.rule?.id=="edict:A05"&&OwnRangerMark(State.enemies.Find(e=>e.id==a.targetId)))
            {InterruptHeroAction("같은 대상의 자기 표식을 조기 갱신하지 않습니다.");return;}
            // Teleport has no travelling phase, so validate its paid destination here.
            if(a.skill==15&&!TeleportLandingValid(a.origin,a.destination))
            {InterruptHeroAction("순간이동 직전 이동 거리·시야·착지 조건을 충족하지 못했습니다.");return;}
            if(a.skill==16&&(OwnElementalShield||ElementalShieldAmount<=.00001f)||a.skill==4&&(OwnIronWall||IronWallAmount<=.00001f))
            {InterruptHeroAction("보호막 생성 직전 같은 보호막 또는 합계 상한 때문에 추가 생성할 수 없습니다.");return;}
            a.released=true;a.snapshot=CaptureDamage();a.phase=HeroActionPhase.Recovering;
            ActionEvent(a,"ACTION_RELEASE",a.travel>0?"착지 완료":"효과 실행");
            if(a.skill<0)ReleaseBasic(a);else ResolveSkill(a);
        }
        void CompleteHeroAction(HeroActionState a,string reason="동작 완료")
        {
            if(State.heroAction!=a)return;
            if(a.skill>=0)State.lastSkillCompletions[a.skill]=State.time;
            ActionEvent(a,"ACTION_END",reason);State.heroAction=new HeroActionState();State.activeSkill=-1;State.actionCd=0;State.action="동작 완료 · 다음 행동 판단 대기";
            if(a.phase==HeroActionPhase.Channeling){State.channelTime=0;State.channelTick=0;ItemEffects.whirlwindDistance=0;State.action=a.exitChannelForSurvival?"전역 회피 · 다음 행동 판단 대기":a.exitChannelForBuild?"설정 변경 · 다음 행동 판단 대기":"회오리 종료 · 다음 행동 판단 대기";State.decisionTime=0;}
        }
        void InterruptHeroAction(string reason)
        {
            if(!HeroActionBusy)return;var a=State.heroAction;ActionEvent(a,"ACTION_INTERRUPTED",reason);
            if(a.whirlwindReserved&&!a.released)EffectEvent("SW4","RESERVATION_LOST",root:a.id,reason:reason);
            if(a.retreatTrapId>0&&!a.released&&string.IsNullOrEmpty(a.policy?.retreatFollowUp))State.traps.RemoveAll(t=>t.id==a.retreatTrapId);
            ItemEffects.whirlwindDistance=0;
            State.heroAction=new HeroActionState();State.activeSkill=-1;State.actionCd=0;State.channelTime=0;State.channelTick=0;
        }
        void ReleaseBasic(HeroActionState a)
        {
            if(Hero.heroClass!=HeroClass.Warrior){LaunchBasicProjectile(a);return;}
            var target=State.enemies.Find(e=>e.id==a.targetId&&!e.dead);
            if(target==null||Vector2.Distance(State.position,target.position)>2||!Map.LineClear(State.position,target.position))
            {ActionEvent(a,"ACTION_MISS","기본 공격의 대상이 사라졌거나 실제 사거리·시야를 벗어났습니다.");return;}
            Hit(target,1,0,true,0,a.snapshot);BasicResource(target.id);Visual?.Invoke(a.origin,target.position,18,1);
        }
        void BasicResource(int target)
        {
            State.resource=Mathf.Min(Stats.maxResource,State.resource+new[]{10,6,5}[(int)Hero.heroClass]);
            if(!Stats.specials.Contains("LC02")||reducedNext||procCooldown>.00001f)return;
            if(lastBasic==target)basicCount++;else{lastBasic=target;basicCount=1;}
            if(basicCount>=3){reducedNext=true;ItemEffects.lc02Charge=5;basicCount=0;procCooldown=4;EffectEvent("LC02","CHARGE",target:target,value:5);}
        }
    }
}
