using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hellscript
{
    public enum HeroActionPhase { Idle, Preparing, Travelling, Channeling, Recovering }
    [Serializable] public sealed class HeroActionPolicy
    {
        public string buildVersion;
        public Rule rule;
        public TargetMode target;
        public EdictTargetPolicy edictTarget;
        public MovementMode movement;
        public float distance;
        public bool clockwise,retreatTrapEquipped;
        public HuntEdictV2Document whirlwindEdict;
        public string leapFollowUp;
        public string retreatFollowUp;
        public float retreatDistance;
        public bool keepRangerTraps;
    }
    [Serializable] public sealed class HeroActionState
    {
        public int id,skill=-1,rule=-1,targetId=-1;
        public HeroActionPhase phase;
        public Vector2 origin,aim,destination;
        public float startedAt,elapsed,prepare,travel,recovery,cost,emptyTime;
        public bool released,escape,travelStarted,whirlwindReserved,crushBonus,pierceBonus,legacyRetreatTrapOnLanding;
        public int chainBonus,retreatTrapId;
        public BlizzardMode blizzardMode;
        public DamageSnapshot snapshot;
        public HeroActionPolicy policy;
        public bool exitChannelForBuild,exitChannelForSurvival;
    }
    [Serializable] public sealed class CombatActionEvent
    {
        public int actionId,skill,targetId;
        public float time,resource;
        public string kind,reason,buildVersion;
        public Vector2 position;
    }
    [Serializable] public sealed class ProjectileVictim {public int id,hits;}
    [Serializable] public sealed class ProjectileGroup
    {
        public int actionId,maxPerVictim=1;
        public List<ProjectileVictim> victims=new List<ProjectileVictim>();
    }
    [Serializable] public sealed class CombatProjectile
    {
        public string casterId,definitionId;
        public int id,actionId,skill=-1,element,maxHits=1;
        public Vector2 origin,position,direction;
        public float createdAt,delay,speed,remaining,radius,coefficient,explosionRadius,damage,pullDistance;
        public bool hostile,shadow,extra;
        public DamageSnapshot snapshot;
        public List<int> hitIds=new List<int>();
    }
    [Serializable] public sealed class CombatTrap
    {
        public string definitionId,casterId;
        public int id,actionId;
        public Vector2 position;
        public float createdAt,triggeredAt,arm=.5f,wait=8,duration=5,remaining,tick,radius=3;
        public bool triggered;
        public DamageSnapshot snapshot;
    }
    public readonly struct ActionTiming
    {
        public readonly float prepare,travel,recovery;
        public ActionTiming(float prepare,float travel=0,float recovery=0){this.prepare=prepare;this.travel=travel;this.recovery=recovery;}
    }
    public static class CombatActions
    {
        public const int Version=5;
        public static HeroActionPolicy CapturePolicy(HeroActionState action,BuildConfig build,Rule explicitRule=null)
        {
            var rule=explicitRule??(build?.rules!=null&&action.rule>=0&&action.rule<build.rules.Count?build.rules[action.rule]:null);
            if(rule!=null&&(action.skill<0?rule.action!=RuleAction.Basic:rule.action!=RuleAction.Skill||rule.skill!=action.skill))rule=null;
            return new HeroActionPolicy{buildVersion=build?.version??"",rule=rule?.Copy(),target=build?.target??TargetMode.Nearest,
                movement=rule!=null&&rule.overrideMovement?rule.movement:build?.movement??MovementMode.Stand,
                distance=rule!=null&&rule.overrideMovement?rule.distance:build?.distance??2,clockwise=build?.clockwise??true,
                retreatTrapEquipped=build?.activeSkills!=null&&build.activeSkills.Contains(8)};
        }
        public static ActionTiming Timing(int skill,HeroClass hero,float speed)
        {
            if(skill<0)
            {float total=new[]{1f,.9f,.8f}[(int)hero]/speed;return new ActionTiming(total*.4f,0,total*.6f);}
            switch(skill)
            {
                case 0:return new ActionTiming(0);
                case 1:return new ActionTiming(.15f,.45f);
                case 2:case 6:case 7:return new ActionTiming(.65f/speed);
                case 3:case 5:case 17:return new ActionTiming(.35f);
                case 4:case 10:case 11:case 15:case 16:return new ActionTiming(.2f);
                case 8:return new ActionTiming(.3f);
                case 9:return new ActionTiming(0,.5f);
                case 12:return new ActionTiming(.7f/speed);
                case 13:return new ActionTiming(.4f);
                case 14:return new ActionTiming(.45f/speed);
                default:throw new ArgumentOutOfRangeException(nameof(skill));
            }
        }
        public static void Normalize(RunState run)
        {
            if(run.combatVersion>Version)throw new NotSupportedException("더 새로운 전투 상태 버전이 필요합니다. 저장 파일은 보존했습니다.");
            run.heroAction??=new HeroActionState();run.actionEvents??=new List<CombatActionEvent>();
            run.projectiles??=new List<CombatProjectile>();run.projectileGroups??=new List<ProjectileGroup>();run.traps??=new List<CombatTrap>();
            if(run.lastSkillStarts==null||run.lastSkillStarts.Length!=18){run.lastSkillStarts=new float[18];for(int i=0;i<18;i++)run.lastSkillStarts[i]=-1000;}
            if(run.lastSkillCompletions==null||run.lastSkillCompletions.Length!=18){run.lastSkillCompletions=new float[18];for(int i=0;i<18;i++)run.lastSkillCompletions[i]=-1000;}
            if(run.combatVersion==0)
            {
                // Old saves already applied their non-channel effect; retain only their recovery lock.
                if(run.activeSkill==0)run.heroAction=new HeroActionState{id=run.nextId++,skill=0,phase=HeroActionPhase.Channeling,released=true,startedAt=run.time-run.channelTime};
                else if(run.actionCd>0)run.heroAction=new HeroActionState{id=run.nextId++,skill=run.activeSkill,phase=HeroActionPhase.Recovering,recovery=run.actionCd,released=true,startedAt=run.time};
            }
            // Older saves cannot contain a post-edit in-flight action: their edits cancelled it.
            // The saved build is therefore the best available source; never replay a release.
            if(run.combatVersion<4&&run.heroAction.phase!=HeroActionPhase.Idle)
                run.heroAction.policy=CapturePolicy(run.heroAction,run.build);
            EnemyCombat.Normalize(run);EdictResponseState.Normalize(run);EdictEngagementState.Normalize(run);EdictRangerState.Normalize(run);run.combatVersion=Version;
            if(run.heroAction.policy!=null)run.heroAction.policy.whirlwindEdict=HuntEdictV2Storage.IsAbsent(run.heroAction.policy.whirlwindEdict)?null:HuntEdictV2.Canonical(run.heroAction.policy.whirlwindEdict);
            if(!string.IsNullOrEmpty(run.heroAction.policy?.leapFollowUp)&&run.heroAction.policy.leapFollowUp!="GLOBAL"&&run.heroAction.policy.leapFollowUp!="W01"&&run.heroAction.policy.leapFollowUp!="W03")throw new NotSupportedException("지원하지 않는 도약 후 공격 선호입니다. 저장 파일은 보존했습니다.");
            EdictLandingPreference.Normalize(run);
            if(!string.IsNullOrEmpty(run.heroAction.policy?.retreatFollowUp)&&run.heroAction.policy.retreatFollowUp!="GLOBAL"&&run.heroAction.policy.retreatFollowUp!="RETREAT")throw new NotSupportedException("지원하지 않는 착지 후 보행 방침입니다. 저장 파일은 보존했습니다.");
        }
    }
}
