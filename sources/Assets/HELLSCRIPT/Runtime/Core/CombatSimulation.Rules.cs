using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    public sealed partial class CombatSimulation
    {
        bool Perceived(EnemyState e)=>!e.dead&&Vector2.Distance(State.position,e.position)<=12&&Map.LineClear(State.position,e.position);
        bool OwnArea(EnemyState e,bool trap)=>trap?State.traps.Any(t=>t.triggered&&t.remaining>0&&Vector2.Distance(t.position,e.position)<=t.radius&&Map.LineClear(t.position,e.position)):
            State.effects.Any(f=>OwnBlizzard(f)&&f.delay<=0&&f.duration>0&&Vector2.Distance(f.position,e.position)<=f.radius&&Map.LineClear(f.position,e.position));
        bool VisibleGround(GroundEffect f)=>f.hostile&&f.duration>0&&Vector2.Distance(State.position,f.position)<=12+f.radius&&Map.LineClear(State.position,f.position);
        bool DangerAt(Vector2 p,float warning,bool activeOnly=false)
            =>EnemyThreatAt(p,warning,activeOnly)||State.effects.Any(f=>VisibleGround(f)&&Vector2.Distance(p,f.position)<f.radius&&(f.kind!=6||Vector2.Distance(p,f.position)>2)&&f.delay<=(activeOnly?0:warning))||
              !activeOnly&&State.enemies.Any(e=>Perceived(e)&&e.brain.action.phase==EnemyActionPhase.Idle&&e.windup>0&&e.windup<=warning&&Vector2.Distance(p,e.aim)<(e.boss?3:2.5f));
        float BuffRemaining(int kind)
        {
            switch(kind)
            {
                case 0:return State.shield>0?State.shieldTime:0;case 1:return State.shoutTime;case 2:return State.shadowCharges>0?State.shadowTime:0;
                case 3:return leapDefense;case 4:return moveBuff;case 5:return elementBuff;case 6:return ItemEffects.whirlwindCharge;case 7:return ItemEffects.crushCharge;
                case 8:return ItemEffects.pierceCharge;case 9:return ItemEffects.chainCharges>0?ItemEffects.chainCharge:0;case 10:return ItemEffects.lc02Charge;
                case 11:return State.guideShrineTime;case 12:return State.resolveShrineTime;case 13:return ItemEffects.ap05Ready?float.PositiveInfinity:0;default:return 0;
            }
        }
        static bool Compare(float actual,RuleCondition c)
        {
            switch(c.comparison)
            {
                case Comparison.AtMost:return actual<=c.value+.0001f;case Comparison.AtLeast:return actual+.0001f>=c.value;
                case Comparison.Between:return actual+.0001f>=c.value&&actual<=c.upper+.0001f;
                case Comparison.Absent:return actual<=0;default:return actual>0;
            }
        }
        public bool EvaluateCondition(RuleCondition c,Rule rule,EnemyState target,out string detail)
        {
            float actual=0;bool valid=true,result=false;var seen=State.enemies.Where(Perceived).ToArray();
            if(BehaviorRules.ValidateCondition(c)!=""){detail="조건 입력 오류";return false;}
            switch(c.id)
            {
                case "SC01":actual=100*State.health/Stats.hp;result=Compare(actual,c);break;
                case "SC02":actual=State.resource;result=Compare(actual,c);break;
                case "SC03":actual=100*State.shield/Stats.hp;result=Compare(actual,c);break;
                case "SC04":actual=Mathf.Max(0,BuffRemaining(c.choice));result=c.comparison==Comparison.AtMost?actual>0&&Compare(actual,c):Compare(actual,c);break;
                case "SC05":actual=100*State.receivedDamage.Where(d=>d.time>=State.time-c.window-.00001f&&d.time<=State.time).Sum(d=>d.hp)/Stats.hp;result=Compare(actual,c);break;
                case "SC06":actual=c.predicted?PredictedHits(rule,target,seen):seen.Count(e=>Vector2.Distance(State.position,e.position)<=c.radius);result=Compare(actual,c);break;
                case "SC07":actual=seen.Any(e=>c.choice==2?e.boss:c.choice==1?e.elite>=0: e.elite>=0||e.boss)?1:0;result=Compare(actual,c);break;
                case "SC08":actual=DangerAt(State.position,c.comparison==Comparison.Soon?c.value:1.5f,c.comparison!=Comparison.Soon)?1:0;result=c.comparison==Comparison.Outside?actual==0:actual>0;break;
                case "SC09":valid=target!=null;if(valid){actual=Vector2.Distance(State.position,target.position);result=Compare(actual,c);}break;
                case "SC10":valid=target!=null;if(valid){actual=100*target.health/target.maxHealth;result=Compare(actual,c);}break;
                case "SC11":valid=target!=null;if(valid){actual=target.kind%6;result=(int)actual==c.choice;}break;
                case "SC12":valid=target!=null;if(valid){actual=target.boss?2:target.elite>=0?1:0;result=c.comparison==Comparison.AtLeast?actual>=c.choice:actual==c.choice;}break;
                case "SC13":
                    valid=target!=null;if(valid)
                    {actual=c.choice==0?CombatEffects.Remaining(target,StatusKind.Poisoned):c.choice==1?CombatEffects.Remaining(target,StatusKind.Slow):c.choice==2?CombatEffects.Remaining(target,StatusKind.Stun):c.choice==3?CombatEffects.Remaining(target,StatusKind.Freeze):c.choice==4?CombatEffects.Remaining(target,StatusKind.Mark):c.choice==5?target.frostMarkTime:OwnArea(target,c.choice==7)?1:0;result=Compare(actual,c);}break;
                case "SC14":valid=target!=null&&seen.Contains(target);if(valid){actual=Mathf.Max(0,target.windup);result=actual>0&&(c.comparison!=Comparison.Soon||actual<=c.value+.0001f);}break;
                case "SC15":
                    valid=State.build.activeSkills.Contains(c.skill)&&catalog.skills[c.skill].unlock<=EffectiveLevel;
                    if(valid){actual=State.cooldowns[c.skill];result=c.comparison==Comparison.Present?actual<=0:Compare(actual,c);}break;
                case "SC16":valid=State.heroAction.phase==HeroActionPhase.Channeling;actual=State.channelTime;result=valid&&Compare(actual,c);break;
                case "SC17":valid=seen.Length==0;actual=valid?Mathf.Max(0,State.time-State.noEnemySince):0;result=valid&&Compare(actual,c);break;
                case "SC18":actual=FindRuleLoot()!=null?1:0;result=Compare(actual,c);break;
                case "SC19":actual=Economy.FreeSlots(Hero);result=Compare(actual,c);break;
                case "SC20":actual=State.bossRewarded?2:State.bossId<0?0:seen.Any(e=>e.boss)?1:-1;result=(int)actual==c.choice;break;
                case "SC21":actual=Mathf.Max(0,TimeLimit-State.time);result=Compare(actual,c);break;
                case "SC22":valid=State.build.activeSkills.Contains(c.skill);actual=State.time-State.lastSkillStarts[c.skill];result=valid&&(c.comparison==Comparison.Present?actual<=c.value+.0001f:actual>c.value+.0001f);break;
            }
            detail=Loc.F("{0} → {1}", BehaviorRules.Summary(c), (valid?Loc.F("현재 {0:0.##} / {1}", actual, (result?"충족":"미충족")):"유효한 대상·참조 없음"));return result;
        }
        bool RuleMatches(Rule rule,EnemyState target,out string details)
        {
            if(rule.always){details="항상 검사";return true;}
            var notes=new List<string>();bool matches=false;
            foreach(var group in rule.groups)
            {
                bool all=group.conditions.Count>0;var parts=new List<string>();
                foreach(var c in group.conditions){bool pass=EvaluateCondition(c,rule,target,out string detail);all&=pass;parts.Add(detail);}
                matches|=all;notes.Add(string.Join(" / ",parts));
            }
            details=string.Join(Loc.T(" 또는 "),notes);return matches;
        }
        LootTarget FindRuleLoot()=>SelectResourceLoot(edictLoot!=null?FindEdictLoot():State.drops.Where(d=>!d.claimed&&!d.ignored&&d.item.rarity>=Policy.minimumRarity&&d.item.rarity>=Policy.pursueRarity&&Vector2.Distance(d.position,State.position)<=12&&Map.LineClear(State.position,d.position)).OrderBy(d=>Vector2.Distance(State.position,d.position)).ThenBy(d=>d.id).FirstOrDefault());
        public int PredictedHits(Rule rule,EnemyState target,EnemyState[] seen=null)
            =>rule!=null&&rule.action==RuleAction.Skill?ForecastAttack(rule,target,observed:seen).uniqueTargets:0;
        RuleTarget EffectiveTarget(Rule r,TargetMode? global=null)
        {
            if(r.target!=RuleTarget.Default)return r.target;
            switch(global??Policy.target){case TargetMode.Elite:return RuleTarget.Elite;case TargetMode.Support:return RuleTarget.Support;case TargetMode.LowHealth:return RuleTarget.LowHealth;case TargetMode.Dense:return RuleTarget.Dense;default:return RuleTarget.Nearest;}
        }
        EnemyState SelectRuleTarget(Rule r,TargetMode? global=null,EdictTargetPolicy exact=null)
        {
            var policy=exact?.version==EdictTargetPolicy.CurrentVersion?exact:global.HasValue?null:TargetPolicy;
            var candidates=policy!=null?sensed.Where(EdictTargetEligible).ToList():sensed;
            if(policy!=null&&r.target==RuleTarget.Default)
                return policy.Select(candidates,State,State.edictTarget?.enemyId??-1,r.targetRadius,CanEdictInterrupt,(a,b)=>Map.LineClear(a,b));
            var mode=EffectiveTarget(r,global);if(mode==RuleTarget.Current)return candidates.Find(e=>e.id==State.targetId);
            float Score(EnemyState e)
            {
                float score=Vector2.Distance(State.position,e.position);
                if(mode==RuleTarget.Elite&&(e.elite>=0||e.boss)||mode==RuleTarget.Support&&e.kind%6==4||mode==RuleTarget.Ranged&&e.kind%6==2||mode==RuleTarget.OwnBlizzard&&OwnArea(e,false)||mode==RuleTarget.OwnTrap&&OwnArea(e,true))score-=30;
                if(mode==RuleTarget.LowHealth)score+=e.health/e.maxHealth*15;
                if(mode==RuleTarget.Dense)score-=candidates.Count(x=>Vector2.Distance(e.position,x.position)<=r.targetRadius&&Map.LineClear(e.position,x.position))*3;
                return score;
            }
            var chosen=candidates.OrderBy(Score).ThenBy(e=>e.id).FirstOrDefault();var old=candidates.Find(e=>e.id==State.targetId);
            if(old!=null&&chosen!=null&&!r.escape&&State.targetRuleId==(r.id??"")&&(State.time-State.targetSelectedAt<.6f||mode==RuleTarget.Nearest&&Mathf.Abs(Score(old)-Score(chosen))<.75f))return old;
            return chosen;
        }
        void SelectTarget(EnemyState target,string reason,string ruleId="")
        {
            int id=target?.id??-1;State.targetRuleId=ruleId;TrackEdictTarget(target);if(id==State.targetId)return;
            State.targetId=id;State.targetSelectedAt=State.time;
            Log("TARGET_SELECTED",target==null?reason:Loc.F("{0} · {1}",(target.goblin?GoldenGoblin.Name:target.boss?GameCatalog.BossNames[target.pattern]:GameCatalog.EnemyNames[target.kind]),reason));
        }
        void RecordDecision(Rule r,int row,string code,string detail,EnemyState target)
        {
            if(code=="RESOURCE")CombatTelemetry.ResourceBlocked(State.statistics,r.skill);
            RecordBlockedDecision(r,row,code,detail,target);
            var old=State.decisions.LastOrDefault(d=>d.ruleId==r.id);
            if(old!=null&&old.actionKnown&&old.action==r.action&&old.code==code&&old.targetId==(target?.id??-1)&&old.version==State.build.version)
            {old.count++;old.lastTime=State.time;old.detail=detail;old.row=row;return;}
            State.decisions.Add(new RuleDecision{ruleId=r.id,row=row,skill=r.skill,action=r.action,actionKnown=true,code=code,detail=detail,targetId=target?.id??-1,version=State.build.version,firstTime=State.time,lastTime=State.time});
            if(State.decisions.Count>240)State.decisions.RemoveAt(0);
        }
        Vector2 MovementDestination(Rule r,SkillDefinition skill,EnemyState target,Vector2? origin=null,EnemyState[] observed=null)
        {
            Vector2 start=origin??State.position;var enemies=observed??sensed.ToArray();
            if(target==null)return start;
            if(r.positionPurpose==PositionPurpose.Default||r.positionPurpose==PositionPurpose.Target)
                return r.escape?Map.Escape(start,target.position,SkillRange(r.skill),enemies.ToList(),State.effects.Where(VisibleGround).ToList(),p=>DangerAt(p,1.5f)):Vector2.MoveTowards(start,target.position,skill.range);
            Vector2 best=start;float bestScore=float.PositiveInfinity;float desired=r.overrideMovement?r.distance:Policy.distance;
            var trap=State.traps.OrderBy(t=>Vector2.Distance(start,t.position)).FirstOrDefault();
            for(float distance=2;distance<=SkillRange(r.skill)+.001f;distance+=2)for(int n=0;n<16;n++)
            {
                float angle=n*Mathf.PI/8;Vector2 p=start+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*distance;
                if(!Map.CanLand(p)||!Map.LineClear(start,p))continue;
                int count=enemies.Count(e=>Vector2.Distance(e.position,p)<=3);float risk=DangerAt(p,1.5f)?100:0;float targetDistance=Vector2.Distance(p,target.position);
                float score=r.positionPurpose==PositionPurpose.Dense?-count*100+risk:r.positionPurpose==PositionPurpose.Sparse?count*100+risk:r.positionPurpose==PositionPurpose.Safe?risk*100+count*10:r.positionPurpose==PositionPurpose.OwnTrap&&trap!=null?Vector2.Distance(p,trap.position)*100+risk:Mathf.Abs(targetDistance-desired)*100+risk;
                score+=r.escape?-targetDistance*.1f:distance*.1f;if(score<bestScore){bestScore=score;best=p;}
            }
            return best;
        }
        sealed class RuleCandidate
        {public Rule rule;public int row;public EnemyState target;public Vector2 destination;public Vector2? aim;public float cost;public string code,detail;public bool ready;public EdictAimPlan edictAim;}
        string DecisionSource(RuleCandidate candidate)=>EdictRuleOrder.IsCompiledRule(candidate.rule.id)
            ?Loc.F("{0} 사냥 칙령",candidate.rule.action==RuleAction.Basic?"기본 공격":catalog.skills[candidate.rule.skill].name):Loc.F("{0}번 규칙",candidate.row+1);
        RuleCandidate InspectRule(Rule r,int row)
        {
            BehaviorRules.NormalizeRule(r);var c=new RuleCandidate{rule=r,row=row,target=SelectRuleTarget(r),destination=State.position,code="CONDITION"};
            if(!r.enabled){c.code="DISABLED";c.detail="사용자가 끈 규칙";return c;}
            if(r.action==RuleAction.Skill&&(r.skill<0||r.skill>=catalog.skills.Count||!State.build.activeSkills.Contains(r.skill)))
            {c.code="NOT_EQUIPPED";c.detail="장착하지 않은 스킬";return c;}
            if(edictSource==null&&EdictRuleOrder.IsCompiledRule(r.id)){c.code="EDICT_OFF";c.detail=edictInactiveReason;return c;}
            // The edict owns collection timing; the legacy preset's "no enemies" condition must not
            // cancel an explicit during-combat choice. Other rule types retain their own conditions.
            if(edictLoot!=null&&r.action==RuleAction.Loot)c.detail="전리품 회수";
            if(!(edictLoot!=null&&r.action==RuleAction.Loot)&&!RuleMatches(r,c.target,out c.detail))return c;
            if(r.action!=RuleAction.Skill)
            {
                if(HeroActionBusy){c.code="ACTION_LOCK";c.detail="진행 중인 주 행동이 있습니다.";return c;}
                if(r.action==RuleAction.Basic)
                {
                    if(r.id=="edict:WARRIOR:BASIC")
                    {var basic=PlanEdictWarriorBasic(edictSource,c.target);c.code=basic.code;c.detail=basic.detail;c.ready=basic.ready;c.target=State.enemies.Find(e=>e.id==basic.targetId&&!e.dead);return c;}
                    if((r.id=="edict:MAGE:BASIC"||r.id=="edict:RANGER:BASIC")&&!EdictAimGate(c,false))return c;
                    if(c.target==null){c.code="NO_TARGET";c.detail="감지한 대상이 없습니다.";return c;}
                    float basicRange=Hero.heroClass==HeroClass.Warrior?2:10;
                    if(Vector2.Distance(State.position,c.target.position)>basicRange){c.code="RANGE";c.detail=Loc.F("기본 공격 사거리 {0}m 밖", basicRange);return c;}
                    if(Hero.heroClass!=HeroClass.Warrior&&!Map.ProjectileClear(State.position,c.target.position,.2f)){c.code="LINE_OF_FIRE";c.detail="투사체가 통과하지 못하는 사선";return c;}
                }
                if(r.action==RuleAction.Loot&&FindRuleLoot()==null){c.code="NO_LOOT";c.detail="필터에 맞는 인지한 회수 대상이 없습니다.";return c;}
                if(r.action==RuleAction.Chest&&!HasRuleChest())
                {c.code="NO_CHEST";c.detail="안전하게 열 수 있는 상자가 없습니다.";return c;}
                c.ready=true;c.code="READY";return c;
            }
            var skill=catalog.skills[r.skill];
            if(skill.heroClass!=Hero.heroClass||skill.unlock>EffectiveLevel){c.code="LEVEL_LOCK";c.detail=Loc.F("해금 레벨 {0}", skill.unlock);return c;}
            if(r.id=="edict:A03"||r.id=="edict:A04"||r.id=="edict:A05"||r.id=="edict:A06")
            {
                var plan=r.skill==8?PlanEdictTrap(edictSource,c.target):r.skill==9?PlanEdictRetreat(edictSource,c.target):r.skill==10?PlanEdictMark(edictSource,c.target):PlanEdictShadow(edictSource,c.target);
                c.code=plan.code;c.detail=plan.detail;c.ready=plan.ready;c.cost=plan.cost;c.destination=plan.destination;c.target=State.enemies.Find(e=>e.id==plan.targetId&&!e.dead);
                if(r.skill==8)c.aim=plan.destination;return c;
            }
            if(r.id=="edict:A01"||r.id=="edict:A02")
            {
                string block=RangerGate(edictSource,r.skill,true,false,out string reason);
                if(block!=""){c.code=block;c.detail=reason;return c;}
                if(r.skill==7)
                {
                    var aim=PlanEdictAim(edictSource,"A02",c.target);var target=State.enemies.FirstOrDefault(e=>e.id==aim.targetId&&!e.dead)??c.target;
                    var point=RangerShotPosition(edictSource,target,aim);
                    if(point.HasValue){c.ready=true;c.code="APPROACH";c.detail="다중 사격의 선호 위치로 이동합니다.";c.destination=point.Value;c.target=target;return c;}
                    c.edictAim=aim;
                }
                // Position and purpose remain observable even when the shot is unaffordable.
                // Otherwise a resource-only basic and an out-of-range shot can both wait forever.
                if(!EdictAimGate(c,false))return c;
                block=RangerGate(edictSource,r.skill,false,false,out reason);
                if(block!=""){c.code=block;c.detail=reason;return c;}
            }
            if(r.id=="edict:W01"||r.id=="edict:W02")
            {
                var plan=r.skill==0?PlanEdictWhirlwind(edictSource,c.target):PlanEdictLeap(edictSource,c.target);
                c.code=plan.code;c.detail=plan.detail;c.ready=plan.ready;c.cost=plan.cost;c.destination=plan.destination;c.target=State.enemies.Find(e=>e.id==plan.targetId&&!e.dead);return c;
            }
            if(r.id=="edict:W05"||r.id=="edict:W06")
            {
                var plan=r.skill==4?PlanEdictIronWall(edictSource,c.target):PlanEdictShout(edictSource,c.target);
                c.code=plan.code;c.detail=plan.detail;c.ready=plan.ready;c.cost=plan.cost;c.destination=plan.destination;return c;
            }
            if(r.id=="edict:W04")
            {
                var plan=PlanEdictSlam(edictSource,c.target);c.code=plan.code;c.detail=plan.detail;c.ready=plan.ready;c.cost=plan.cost;c.destination=plan.destination;
                c.target=State.enemies.Find(e=>e.id==plan.targetId&&!e.dead);return c;
            }
            if(r.id=="edict:M04")
            {
                var plan=PlanEdictTeleport(edictSource,c.target);c.code=plan.code;c.detail=plan.detail;c.ready=plan.ready;c.cost=plan.cost;c.destination=plan.destination;return c;
            }
            if(r.id=="edict:M05")
            {
                var plan=PlanEdictShield(edictSource,c.target);c.code=plan.code;c.detail=plan.detail;c.ready=plan.ready;c.cost=plan.cost;c.destination=plan.destination;return c;
            }
            if(r.id=="edict:M06")
            {
                var plan=PlanEdictNova(edictSource,c.target);c.code=plan.code;c.detail=plan.detail;c.ready=plan.ready;c.cost=plan.cost;c.destination=plan.destination;
                c.target=State.enemies.Find(e=>e.id==plan.targetId&&!e.dead);return c;
            }
            if(State.activeSkill==r.skill&&State.heroAction.phase==HeroActionPhase.Channeling){c.code="CHANNELING";c.detail="이미 유지 중";return c;}
            if(!CanInterruptWith(row,skill,r)){c.code="ACTION_LOCK";c.detail="현재 행동 구간에서 중단할 수 없습니다.";return c;}
            if(State.cooldowns[r.skill]>0){c.code="COOLDOWN";c.detail=Loc.F("남은 CD {0:0.00}초", State.cooldowns[r.skill]);return c;}
            c.cost=Cost(skill);if(State.resource<c.cost){c.code="RESOURCE";c.detail=Loc.F("필요 {0:0.##} / 현재 {1:0.##}", c.cost, State.resource);return c;}
            if(r.id=="edict:W03")
            {var risk=AssessEdictSurvival(edictSource,State.edictResponse?.memory);if(!risk.emergency&&State.resource-c.cost<risk.escapeResource-.00001f){c.code="RESERVED";c.detail="일반 공격을 위해 생존용 자원을 사용하지 않습니다.";return c;}}
            // A fully connected core policy chooses its target before range/line checks. Its
            // derived rule has no hidden legacy conditions, and the final aim still passes
            // the same resource, action, perception and projectile safety gates.
            if(EdictRuleOrder.IsCompiledRule(r.id)&&!EdictAimGate(c,false))return c;
            bool ground=GroundSkill(r.skill);
            if(c.target==null&&skill.kind!=SkillKind.Shield&&skill.kind!=SkillKind.Shout&&skill.kind!=SkillKind.Shadow&&!(ground&&r.areaAim==AreaAim.Self)){c.code="NO_TARGET";c.detail="감지한 대상이 없습니다.";return c;}
            if(skill.kind==SkillKind.Shield&&State.shields.Any(s=>s.definitionId==SkillId(r.skill)&&s.amount>0)||skill.kind==SkillKind.Mark&&CombatEffects.Has(c.target,StatusKind.Mark)||skill.kind==SkillKind.Shadow&&State.shadowCharges>0)
            {c.code="EFFECT_ACTIVE";c.detail="현재 효과가 남아 있습니다.";return c;}
            bool movement=skill.kind==SkillKind.Leap||skill.kind==SkillKind.Retreat||skill.kind==SkillKind.Teleport;
            float range=skill.kind==SkillKind.Whirlwind?3:skill.range;
            if(ground)c.destination=c.aim??GroundAim(r,c.target);
            if(!movement&&range>0&&Vector2.Distance(State.position,ground?c.destination:c.target.position)>range+.0001f){c.code="RANGE";c.detail=Loc.F("스킬 사거리 {0}m 밖", range);return c;}
            float radius=r.skill==6?.3f:r.skill==7?.2f:r.skill==12?.25f:0;
            if(radius>0&&!Map.ProjectileClear(State.position,c.target.position,radius)){c.code="LINE_OF_FIRE";c.detail="투사체가 통과하지 못하는 사선";return c;}
            if(movement)
            {c.destination=MovementDestination(r,skill,c.target);if(r.skill==15)c.destination=LegalLegacyTeleportDestination(c.destination);
                bool invalid=r.skill==15?!TeleportLandingValid(State.position,c.destination):Vector2.Distance(State.position,c.destination)<2||!Map.CanLand(c.destination)||!Map.LineClear(State.position,c.destination);
                if(invalid){c.code="LANDING";c.detail="이동 범위 안에 유효한 착지점이 없습니다.";return c;}}
            if(ground&&(!Map.CanLand(c.destination,.1f)||!Map.LineClear(State.position,c.destination))){c.code="LANDING";c.detail="장판을 설치할 수 없는 위치";return c;}
            c.code="READY";c.ready=true;return c;
        }
        bool HasRuleChest()
        {
            if(State.layout.legacy||!Policy.openChests||State.phase==RunPhase.Boss&&!Policy.chestsAfterBoss||LowTime)return false;
            return State.layout.chests.Any(c=>c.discovered&&!c.abandoned&&(c.phase==ChestPhase.Available||c.phase==ChestPhase.Approaching||c.phase==ChestPhase.Opening)&&c.retryAfter<=State.time&&
                (c.definitionId!="CH01"||Policy.commonChests)&&(c.definitionId!="CH02"||Policy.sealedChests)&&CursedAllowed(c)&&SafeForChest(c)&&c.accessPoints.Any(p=>OpeningPointFree(p)&&Map.Length(State.position,p)<=Policy.chestDetour));
        }
        void DecideRules()
        {
            BehaviorRules.Normalize(State.build);
            // With a document driving the hero the rules keep their rows but are inspected in the
            // document's category and attack order; a skill whose automatic use is off is not inspected.
            var candidates=EdictRuleOrder.ForSimulation(State.build.rules,edictSource)
                .Select(o=>o.skipReason==""?InspectRule(o.rule,o.row):new RuleCandidate{rule=o.rule,row=o.row,destination=State.position,code="EDICT_OFF",detail=o.skipReason}).ToArray();
            // Gathering walks toward remembered enemies before fighting; attacks wait until it ends.
            if(Gathering)foreach(var c in candidates)if(c.ready&&(c.rule.action==RuleAction.Skill||c.rule.action==RuleAction.Basic)){c.ready=false;c.code="GATHERING";c.detail="몰이 중에는 공격을 보류합니다.";}
            RuleCandidate selected=null;foreach(var c in LandingPreferenceOrder(candidates)){if(!c.ready)continue;if(EdictAimGate(c)){selected=c;break;}}
            selected=ApplyDiscoveryOrder(candidates,ReviewShieldOpening(candidates,selected));
            foreach(var c in candidates)RecordDecision(c.rule,c.row,c==selected?"SELECTED":c.ready?"PRIORITY":c.code,c.ready&&c!=selected?
                EdictRuleOrder.IsCompiledRule(selected.rule.id)?Loc.F("{0}이 먼저 실행됩니다.",DecisionSource(selected)):Loc.F("{0}번 규칙이 먼저 실행됩니다.",selected.row+1):c.detail,c.target);
            if(selected==null)
            {
                if(HeroActionBusy)
                {
                    var policy=State.heroAction.policy;
                    if(State.heroAction.phase==HeroActionPhase.Channeling&&policy?.rule!=null)
                        SelectTarget(SelectRuleTarget(policy.rule,policy.target,policy.edictTarget),"유지 행동의 목표 재평가",policy.rule.id);
                    return;
                }
                var approach=candidates.FirstOrDefault(c=>c.target!=null&&(c.code=="RANGE"||c.code=="LINE_OF_FIRE"));
                State.movementRule=approach?.row??-1;State.movementAction=approach?.rule.action??RuleAction.Skill;
                if(approach!=null)SelectTarget(approach.target,"공격 위치 확보",approach.rule.id);
                State.action=approach!=null?"공격 위치로 이동":"실행 가능한 규칙 없음";return;
            }
            State.movementRule=selected.row;State.movementAction=selected.rule.action;
            if((selected.rule.id=="edict:M06"||selected.rule.id=="edict:A02")&&selected.code=="APPROACH")
            {SelectTarget(selected.target,selected.detail,selected.rule.id);State.destination=selected.destination;State.action=selected.detail;return;}
            if(selected.rule.action==RuleAction.Skill||selected.rule.action==RuleAction.Basic)
            {SelectTarget(selected.target,DecisionSource(selected),selected.rule.id);StartHeroAction(selected.rule.action==RuleAction.Basic?-1:selected.rule.skill,selected.target,selected.destination,selected.cost,selected.row,selected.rule.escape,explicitRule:EdictRuleOrder.IsCompiledRule(selected.rule.id)?selected.rule:null,aim:selected.aim);}
            else if(selected.rule.action==RuleAction.Loot)State.movementDrop=FindRuleLoot()?.id??-1;
        }
    }
}
