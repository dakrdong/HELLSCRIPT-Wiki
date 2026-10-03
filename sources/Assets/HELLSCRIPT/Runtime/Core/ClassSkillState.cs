using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    [Serializable] public sealed class ClassSkillTimer {public string id;public float remaining,total;}
    [Serializable] public sealed class ClassSkillEffect
    {
        public string id,kind,source,previous="";
        public int root,target=-1,count,limit,element;
        public float until,next,period,value,additive,extra,radius,width,health,created;
        public Vector2 position,direction,origin;
        public bool consumed,persistent;
        public DamageSnapshot snapshot;
        public List<string> history=new List<string>();
        public List<int> targets=new List<int>();
    }
    [Serializable] public sealed class ClassSkillDot
    {
        public string id;public int root,target,element;
        public float remaining,perSecond,next,expires;
        public DamageSnapshot snapshot;
    }
    [Serializable] public sealed class ClassCastModifier
    {public string source;public float additive,area=1;}
    [Serializable] public sealed class ClassSkillCast
    {
        public string id;
        public int root,target=-1,element;
        public float started,releaseAt,finishAt,cost,resourceRatio,additive,setAdditive,setRoll,legendary,area=1,rankScale=1;
        public bool released,automatic,highResource,stationary,combo,lightningCharge,ancestralCharge,setElementCharge,elementCharge,compassLightning,vengeance,overheat,firstHit;
        public Vector2 origin,aim,destination;
        public DamageSnapshot snapshot;
        public List<string> receipts=new List<string>();
        public List<ClassCastModifier> setModifiers=new List<ClassCastModifier>();
    }
    [Serializable] public sealed class ClassSkillRuntimeState
    {
        public string format;
        public int version=1,lastElement;
        public float ultimateRemaining,ultimateTotal,stationarySeconds;
        public Vector2 previousPosition;
        public ClassSkillCast action;
        public ClassSkillIntent intent;
        public List<ClassSkillPolicyRecord> policies=new List<ClassSkillPolicyRecord>();
        public List<ClassSkillCast> casts=new List<ClassSkillCast>();
        public List<ClassSkillTimer> cooldowns=new List<ClassSkillTimer>();
        public List<ClassSkillEffect> effects=new List<ClassSkillEffect>();
        public List<ClassSkillDot> dots=new List<ClassSkillDot>();
        public List<ClassSkillTimer> intervals=new List<ClassSkillTimer>();
    }
    public sealed class ClassSkillReadiness
    {
        public readonly string id,reason,detail,detailEn;
        public readonly bool ready;
        public readonly float cost,cooldown;
        public ClassSkillReadiness(string id,string reason,float cost=0,float cooldown=0,string detailOverride=null,string englishOverride=null)
        {this.id=id;this.reason=reason;this.cost=cost;this.cooldown=cooldown;ready=reason=="";
            var message=ClassSkills.Data.reasons.FirstOrDefault(m=>m.id==reason);detail=message?.ko??reason;detailEn=message?.en??reason;
            if(reason=="AUTO_CONDITION"){detail=ClassSkills.Find(id)?.automatic??detail;detailEn=ClassSkills.Find(id)?.automaticEn??detailEn;}
            if(detailOverride!=null)detail=detailOverride;if(englishOverride!=null)detailEn=englishOverride;}
    }
    public enum ClassSkillEventOrigin { Cast, Direct, Periodic, Summon, Split, Echo, Equipment, Secondary }
    public sealed class ClassSkillEvent
    {
        public readonly string skillId,kind,sourceId;
        public readonly int castId,targetId;
        public readonly ClassSkillEventOrigin origin;
        public readonly float time,value;
        public readonly Vector2 position;
        public ClassSkillEvent(string skill,string kind,string source,int cast,int target,float time,float value,Vector2 position,ClassSkillEventOrigin origin=ClassSkillEventOrigin.Cast)
        {this.origin=origin;skillId=skill;this.kind=kind;sourceId=source;castId=cast;targetId=target;this.time=time;this.value=value;this.position=position;}
    }
}
