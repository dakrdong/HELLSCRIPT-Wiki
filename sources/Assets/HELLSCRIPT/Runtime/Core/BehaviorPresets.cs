using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Hellscript.BehaviorRules;

namespace Hellscript
{
    public static class BehaviorPresets
    {
        public static readonly string[] Concepts={"회오리를 유지하며 자원을 관리합니다.","도약으로 진입하고 분쇄로 마무리합니다.","거리를 유지하며 관통 사선을 만듭니다.","덫과 후퇴로 중독 대상을 유도합니다.","눈보라 안의 적에게 화염구를 연결합니다.","군집에 연쇄 번개와 제어를 연결합니다."};
        public static BuildConfig ForLevel(HeroClass hero,int variant,int level,GameCatalog catalog)
        {
            level=Mathf.Clamp(level,1,30);var build=Create(hero,variant);
            if(hero==HeroClass.Mage&&variant==0&&level<catalog.skills[13].unlock)
            {
                var original=build.rules.Single(r=>r.skill==12);
                var fire=Or(Make(12,C("SC06",Comparison.AtLeast,2,predicted:true)),C("SC12",Comparison.Equal,0,2));fire.id=original.id;fire.target=RuleTarget.Dense;
                build.rules[build.rules.IndexOf(original)]=fire;
            }
            foreach(var rule in build.rules)if(rule.action==RuleAction.Skill&&catalog.skills[rule.skill].unlock>level)rule.enabled=false;
            build.passives=build.passives.Take(ContentUnlocks.Rules.passiveLevels.Count(p=>level>=p)).ToArray();
            build.name=Loc.Source("{0} · Lv.{1} 추천", build.name, level);build.version=$"growth-1-{(int)hero}-{variant}-{level}";return build;
        }
        static Rule Escape(int skill,RuleCondition condition,PositionPurpose purpose=PositionPurpose.Safe)
        {var r=Make(skill,condition);r.escape=true;r.positionPurpose=purpose;return r;}
        static Rule Or(Rule rule,params RuleCondition[] conditions)
        {rule.groups.Add(new ConditionGroup{conditions=new List<RuleCondition>(conditions)});return rule;}
        public static BuildConfig Create(HeroClass hero,int variant)
        {
            var b=new BuildConfig{target=TargetMode.Nearest,movement=hero==HeroClass.Warrior?MovementMode.Orbit:MovementMode.KeepDistance,distance=hero==HeroClass.Warrior?2:7};
            if(hero==HeroClass.Warrior)
            {
                b.name=variant==0?"회오리 생존":"도약 분쇄";b.passives=variant==0?new[]{0,1,2}:new[]{0,2,5};
                b.rules.Add(Escape(1,C("SC01",Comparison.AtMost,30),PositionPurpose.Sparse));
                b.rules.Add(Make(4,C("SC01",Comparison.AtMost,55)));
                if(variant==0)
                {b.rules.Add(Make(5,C("SC02",Comparison.AtMost,40)));b.rules.Add(Or(Make(0,C("SC06",Comparison.AtLeast,2)),C("SC07",Comparison.Present,0,2)));}
                else
                {
                    b.movement=MovementMode.Approach;b.distance=1.5f;
                    var leap=Make(1,C("SC09",Comparison.AtLeast,5),C("SC06",Comparison.AtLeast,3,predicted:true),C("SC01",Comparison.AtLeast,70));leap.target=RuleTarget.Dense;leap.positionPurpose=PositionPurpose.Target;b.rules.Add(leap);
                    b.rules.Add(Or(Make(3,C("SC06",Comparison.AtLeast,2)),C("SC12",Comparison.AtLeast,0,1)));
                    b.rules.Add(Make(2));
                }
            }
            else if(hero==HeroClass.Ranger)
            {
                b.name=variant==0?"관통 거리유지":"맹독 매복";b.passives=variant==0?new[]{0,1,2}:new[]{1,3,4};b.target=variant==0?TargetMode.Support:TargetMode.Nearest;b.distance=variant==0?7:6;
                var escape=Escape(9,C("SC09",Comparison.AtMost,3.5f),variant==0?PositionPurpose.Safe:PositionPurpose.OwnTrap);b.rules.Add(escape);
                if(variant==0)
                {
                    var mark=Make(10,C("SC12",Comparison.AtLeast,0,1));mark.target=RuleTarget.Elite;b.rules.Add(mark);
                    b.rules.Add(Or(Make(6,C("SC06",Comparison.AtLeast,2,predicted:true)),C("SC12",Comparison.Equal,0,2)));
                    b.rules.Add(Make(7,C("SC06",Comparison.AtLeast,3,predicted:true)));
                }
                else
                {
                    Or(escape,C("SC01",Comparison.AtMost,35));
                    b.rules.Add(Escape(9,C("SC08",Comparison.Soon,1),PositionPurpose.OwnTrap));
                    b.rules.Add(Make(8,C("SC09",Comparison.AtMost,6)));
                    var shadow=Make(11,C("SC13",Comparison.Present));shadow.target=RuleTarget.OwnTrap;b.rules.Add(shadow);b.rules.Add(Make(6));
                }
            }
            else
            {
                b.name=variant==0?"서리 장판":"연쇄 제어";b.passives=variant==0?new[]{0,1,3}:new[]{2,3,4};b.distance=variant==0?7:6;b.target=variant==0?TargetMode.Nearest:TargetMode.Support;
                b.rules.Add(Escape(15,variant==0?C("SC01",Comparison.AtMost,35):C("SC08",Comparison.Soon,1)));
                b.rules.Add(Make(16,C("SC01",Comparison.AtMost,55)));
                if(variant==0)
                {
                    var blizzard=Or(Make(13,C("SC06",Comparison.AtLeast,3,predicted:true)),C("SC12",Comparison.Equal,0,2));blizzard.target=RuleTarget.Dense;b.rules.Add(blizzard);
                    var fire=Make(12,C("SC13",Comparison.Present,0,6));fire.target=RuleTarget.OwnBlizzard;b.rules.Add(fire);
                }
                else {b.rules.Add(Make(17,C("SC06",Comparison.AtLeast,2)));b.rules.Add(Make(14));}
            }
            Normalize(b);for(int i=0;i<b.rules.Count;i++)b.rules[i].id=$"preset-{(int)hero}-{variant}-{i}";return b;
        }
    }
}
