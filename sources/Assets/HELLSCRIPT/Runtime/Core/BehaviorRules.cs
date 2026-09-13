using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    public enum RuleAction { Skill, Basic, Loot, Chest, Explore }
    public enum RuleTarget { Default, Current, Nearest, Elite, Support, Ranged, LowHealth, Dense, OwnBlizzard, OwnTrap }
    public enum PositionPurpose { Default, Target, Dense, Sparse, Safe, Distance, OwnTrap }
    public enum AreaAim { Target, Dense, Self, ObservedPath }
    public enum BlizzardMode { Follow, Fixed }
    public enum Comparison { AtMost, AtLeast, Between, Present, Absent, Soon, Inside, Outside, Equal }
    [Serializable] public sealed class RuleCondition
    {
        public string id="SC01";
        public Comparison comparison;
        public float value=30,upper=8,radius=3,window=2;
        public int choice,skill=-1;
        public bool predicted;
    }
    [Serializable] public sealed class ConditionGroup {public List<RuleCondition> conditions=new List<RuleCondition>();}
    [Serializable] public sealed class DamageReceived {public float time,hp,absorbed;}
    [Serializable] public sealed class RuleDecision
    {
        public string ruleId,code,detail,version;
        public int row,skill,targetId=-1,count=1;
        public float firstTime,lastTime;
        public RuleAction action;
        public bool actionKnown;
    }
    public sealed class ConditionDefinition
    {
        public readonly string id,name,unit;
        public readonly Comparison[] comparisons;
        public readonly float min,max,step,initial;
        public ConditionDefinition(int id,string name,string unit,float min,float max,float step,float initial,params Comparison[] comparisons)
        {this.id=$"SC{id:00}";this.name=name;this.unit=unit;this.min=min;this.max=max;this.step=step;this.initial=initial;this.comparisons=comparisons;}
        public RuleCondition Create()=>new RuleCondition{id=id,value=initial,comparison=comparisons[0]};
    }
    public static class BehaviorRules
    {
        public const int Version=2;
        public static readonly float[] Radii={2,3,5,8},Windows={1,2,3},Warnings={.5f,1,1.5f};
        public static readonly string[] Buffs={"보호막","전투 함성","그림자 화살","도약 방어","이동 가속","원소 교차","회오리 충전","분쇄 충전","관통 충전","연쇄 충전","절제 비용 감소","길잡이 성소","결의 성소","절약된 집중"};
        public static readonly string[] States={"중독","둔화","기절","빙결","표식","세트 노출","자신의 눈보라 안","자신의 활성 덫 안"};
        public static readonly string[] Targets={"전체 설정","현재 대상","가까운 적","정예·보스","지원형","원거리형","낮은 HP","밀집 중심","눈보라 안 우선","활성 덫 안 우선"};
        public static readonly string[] Movements={"접근","가장자리 선회","거리 유지","제자리","후퇴"};
        public static readonly string[] Purposes={"기본 위치","목표 위치","적이 많은 위치","적이 적은 위치","위험이 적은 위치","지정 거리","자신의 덫 방향"};
        public static readonly ConditionDefinition[] All={
            new ConditionDefinition(1,"현재 HP","%",5,95,5,30,Comparison.AtMost,Comparison.AtLeast),
            new ConditionDefinition(2,"현재 자원","%",0,100,5,40,Comparison.AtMost,Comparison.AtLeast),
            new ConditionDefinition(3,"보호막 / 최대 HP","%",0,100,5,0,Comparison.AtMost,Comparison.AtLeast),
            new ConditionDefinition(4,"지정 버프","초",0,10,.5f,2,Comparison.Present,Comparison.Absent,Comparison.AtMost),
            new ConditionDefinition(5,"최근 HP 피해","%",5,80,5,30,Comparison.AtLeast),
            new ConditionDefinition(6,"감지한 적 수","명",1,20,1,2,Comparison.AtLeast,Comparison.AtMost),
            new ConditionDefinition(7,"감지한 정예·보스","",0,0,1,0,Comparison.Present,Comparison.Absent),
            new ConditionDefinition(8,"현재 위치의 위험","초",.5f,1.5f,.5f,1,Comparison.Inside,Comparison.Outside,Comparison.Soon),
            new ConditionDefinition(9,"대상과 거리","m",0,12,.5f,5,Comparison.AtMost,Comparison.AtLeast,Comparison.Between),
            new ConditionDefinition(10,"대상 HP","%",5,95,5,30,Comparison.AtMost,Comparison.AtLeast),
            new ConditionDefinition(11,"대상 행동 유형","",0,0,1,0,Comparison.Equal),
            new ConditionDefinition(12,"대상 등급","",0,0,1,0,Comparison.Equal,Comparison.AtLeast),
            new ConditionDefinition(13,"대상 상태·지속 영역","",0,0,1,0,Comparison.Present,Comparison.Absent),
            new ConditionDefinition(14,"대상의 위험 시전","초",.5f,1.5f,.5f,1,Comparison.Present,Comparison.Soon),
            new ConditionDefinition(15,"지정 스킬 CD","초",0,20,.5f,2,Comparison.Present,Comparison.AtMost),
            new ConditionDefinition(16,"연속 유지 시간","초",.5f,10,.5f,2,Comparison.AtLeast),
            new ConditionDefinition(17,"인지한 적 없음","초",0,5,.5f,1,Comparison.AtLeast),
            new ConditionDefinition(18,"회수할 장비 존재","",0,0,1,0,Comparison.Present,Comparison.Absent),
            new ConditionDefinition(19,"가방 빈칸","칸",0,30,1,0,Comparison.AtMost),
            new ConditionDefinition(20,"보스 상태","",0,0,1,0,Comparison.Equal),
            new ConditionDefinition(21,"균열 남은 시간","초",5,300,5,30,Comparison.AtMost),
            new ConditionDefinition(22,"지정 스킬 최근 실행","초",.5f,10,.5f,2,Comparison.Present,Comparison.Absent)
        };
        public static ConditionDefinition Definition(string id)=>All.FirstOrDefault(d=>d.id==id);
        public static RuleCondition C(string id,Comparison comparison,float value=0,int choice=0,int skill=-1,float radius=3,bool predicted=false)
            =>new RuleCondition{id=id,comparison=comparison,value=value,choice=choice,skill=skill,radius=radius,predicted=predicted};
        public static Rule Make(int skill,params RuleCondition[] conditions)
            =>new Rule(skill){schema=Version,always=conditions.Length==0,groups=conditions.Length==0?new List<ConditionGroup>():new List<ConditionGroup>{new ConditionGroup{conditions=conditions.ToList()}}};
        public static Rule Utility(RuleAction action,params RuleCondition[] conditions)
        {var rule=Make(-1,conditions);rule.action=action;return rule;}
        public static void NormalizeRule(Rule r)
        {
            if(r.schema>Version)throw new NotSupportedException("더 새로운 행동 규칙 버전이 필요합니다. 저장은 보존했습니다.");
            if(r.schema>=1){r.groups??=new List<ConditionGroup>();r.schema=Version;return;}
            r.groups=new List<ConditionGroup>();r.always=r.condition==ConditionKind.Always;
            RuleCondition c=null;
            switch(r.condition)
            {
                case ConditionKind.HealthBelow:c=C("SC01",Comparison.AtMost,r.threshold);break;
                case ConditionKind.ResourceBelow:c=C("SC02",Comparison.AtMost,r.threshold);break;
                case ConditionKind.EnemiesNear:c=C("SC06",Comparison.AtLeast,r.threshold);break;
                case ConditionKind.TargetFar:c=C("SC09",Comparison.AtLeast,r.threshold);break;
                case ConditionKind.TargetNear:c=C("SC09",Comparison.AtMost,r.threshold);break;
                case ConditionKind.ElitePresent:c=C("SC12",Comparison.AtLeast,0,1);break;
                case ConditionKind.Danger:c=C("SC08",Comparison.Soon,1.5f);break;
                case ConditionKind.NoShield:c=C("SC03",Comparison.AtMost,0);break;
                case ConditionKind.TargetPoisoned:c=C("SC13",Comparison.Present,0);break;
            }
            if(c!=null)r.groups.Add(new ConditionGroup{conditions=new List<RuleCondition>{c}});
            // Preserve the old implicit boss exception as a visible OR branch in migrated rules only.
            if(r.condition==ConditionKind.EnemiesNear)r.groups.Add(new ConditionGroup{conditions=new List<RuleCondition>{C("SC12",Comparison.Equal,0,2)}});
            if(r.escape)r.positionPurpose=PositionPurpose.Safe;r.schema=Version;
        }
        public static void Normalize(BuildConfig build)
        {
            if(build==null)throw new InvalidOperationException("행동 설정이 없습니다.");
            if(build.ruleSchema>Version)throw new NotSupportedException("더 새로운 행동 설정 버전이 필요합니다. 저장은 보존했습니다.");
            build.rules??=new List<Rule>();
            if(build.ruleSchema==0)
            {
                build.activeSkills=build.rules.Where(r=>r.action==RuleAction.Skill&&r.skill>=0).Select(r=>r.skill).Distinct().ToList();
                build.rules.Add(Utility(RuleAction.Basic));
                build.rules.Add(Utility(RuleAction.Loot,C("SC17",Comparison.AtLeast,0),C("SC18",Comparison.Present)));
                build.rules.Add(Utility(RuleAction.Chest,C("SC17",Comparison.AtLeast,0)));
                build.rules.Add(Utility(RuleAction.Explore,C("SC17",Comparison.AtLeast,0)));
                build.ruleSchema=Version;
            }
            build.activeSkills??=new List<int>();build.passives??=Array.Empty<int>();
            for(int i=0;i<build.rules.Count;i++)
            {var r=build.rules[i];NormalizeRule(r);if(string.IsNullOrEmpty(r.id)){int candidate=i;do{r.id="rule-"+(candidate++).ToString("00");}while(build.rules.Any(other=>other!=r&&other.id==r.id));}}
            build.ruleSchema=Version;
        }
        public static string ValidateCondition(RuleCondition c)
        {
            var d=Definition(c.id);if(d==null)return "알 수 없는 조건입니다.";
            if(!d.comparisons.Contains(c.comparison))return "비교 방법이 맞지 않습니다.";
            if(float.IsNaN(c.value)||float.IsInfinity(c.value)||c.value<d.min||c.value>d.max||Mathf.Abs((c.value-d.min)/d.step-Mathf.Round((c.value-d.min)/d.step))>.001f)return Loc.F("{0}–{1}{2}, {3} 단위로 입력하세요.", d.min, d.max, d.unit, d.step);
            if(c.comparison==Comparison.Between&&(c.upper<c.value||c.upper>d.max||Mathf.Abs(c.upper*2-Mathf.Round(c.upper*2))>.001f))return "거리 범위의 끝값을 확인하세요.";
            if(c.id=="SC06"&&!Radii.Contains(c.radius))return "감지 반경은 2/3/5/8m입니다.";
            if(c.id=="SC05"&&!Windows.Contains(c.window))return "피해 기록은 최근 1/2/3초입니다.";
            int choices=c.id=="SC04"?Buffs.Length:c.id=="SC07"?3:c.id=="SC11"?6:c.id=="SC12"||c.id=="SC20"?3:c.id=="SC13"?States.Length:1;
            if(c.choice<0||c.choice>=choices)return "선택값이 유효하지 않습니다.";
            if((c.id=="SC15"||c.id=="SC22")&&(c.skill<0||c.skill>=18))return "참조할 장착 스킬을 선택하세요.";
            return "";
        }
        // Read without normalization or JSON copying: non-finite values must not be
        // lost or converted before the save boundary validates the player's draft.
        public static string InputValueIssue(BuildConfig b)
        {
            if(b==null)return "사냥 설정이 없습니다.";
            bool Finite(params float[] values)=>values.All(v=>!float.IsNaN(v)&&!float.IsInfinity(v));
            if(!Finite(b.distance,b.potionThreshold,b.chestDetour,b.shrineDetour,b.cursedMinimumTime))return "거리·HP·남은 시간에는 유효한 숫자를 입력하세요.";
            foreach(var r in b.rules??new List<Rule>())
            {
                if(r==null)return "비어 있는 행동 규칙을 확인하세요.";
                if(!Finite(r.distance,r.targetRadius,r.threshold))return "행동 규칙의 거리·반경·기준값을 확인하세요.";
                foreach(var g in r.groups??new List<ConditionGroup>())
                {
                    if(g==null)return "비어 있는 조건 묶음을 확인하세요.";
                    foreach(var c in g.conditions??new List<RuleCondition>())
                        if(c==null||!Finite(c.value,c.upper,c.radius,c.window))return "조건의 숫자와 범위를 확인하세요.";
                }
            }
            return "";
        }
        public static List<string> Validate(BuildConfig b,HeroClass hero)
        {
            string valueIssue=InputValueIssue(b);if(valueIssue!="")return new List<string>{valueIssue};
            if(b!=null&&b.emptySlot)return new List<string>{"저장된 설정이 없는 슬롯입니다."};
            var errors=new List<string>();Normalize(b);
            if(b.activeSkills.Count>4||b.activeSkills.Distinct().Count()!=b.activeSkills.Count||b.activeSkills.Any(s=>s<0||s>=18||s/6!=(int)hero))errors.Add("장착 액티브는 같은 직업의 서로 다른 스킬 최대 4개입니다.");
            if(b.passives.Length>3||b.passives.Distinct().Count()!=b.passives.Length||b.passives.Any(p=>p<0||p>=6))errors.Add("패시브는 서로 다른 항목 최대 3개입니다.");
            if(b.rules.Count(r=>r.enabled)>24)errors.Add("활성 규칙은 최대 24개입니다.");
            foreach(var set in b.rules.Where(r=>r.action==RuleAction.Skill).GroupBy(r=>r.skill))if(set.Count()>3)errors.Add(Loc.F("스킬 {0}의 규칙은 최대 3개입니다.", set.Key+1));
            if(b.rules.GroupBy(r=>r.id).Any(g=>g.Count()>1))errors.Add("규칙 식별자가 중복되었습니다. 복제 버튼으로 다시 추가하세요.");
            for(int i=0;i<b.rules.Count;i++)
            {
                var r=b.rules[i];string prefix=Loc.F("{0}번 규칙: ", i+1);
                if(!Enum.IsDefined(typeof(RuleAction),r.action)||!Enum.IsDefined(typeof(RuleTarget),r.target)||!Enum.IsDefined(typeof(PositionPurpose),r.positionPurpose)||!Enum.IsDefined(typeof(MovementMode),r.movement))errors.Add(Loc.F("{0}행동·목표·이동 선택이 유효하지 않습니다.", prefix));
                if(!Enum.IsDefined(typeof(AreaAim),r.areaAim)||!Enum.IsDefined(typeof(BlizzardMode),r.blizzardMode))errors.Add(Loc.F("{0}장판 조준·이동 선택이 유효하지 않습니다.", prefix));
                if(r.areaAim!=AreaAim.Target&&(r.action!=RuleAction.Skill||r.skill!=8&&r.skill!=13))errors.Add(Loc.F("{0}설치 위치 선택은 맹독 덫과 눈보라에서 사용합니다.", prefix));
                if(r.action==RuleAction.Skill&&!b.activeSkills.Contains(r.skill))errors.Add(Loc.F("{0}장착하지 않은 스킬입니다.", prefix));
                if(r.always&&r.groups.Count>0||!r.always&&(r.groups.Count<1||r.groups.Count>2))errors.Add(Loc.F("{0}항상 검사 또는 조건 OR 묶음 1–2개를 선택하세요.", prefix));
                for(int g=0;g<r.groups.Count;g++)
                {
                    var group=r.groups[g];if(group.conditions.Count<1||group.conditions.Count>3)errors.Add(Loc.F("{0}OR {1} 묶음의 AND 조건은 1–3개입니다.", prefix, g+1));
                    foreach(var c in group.conditions){string issue=ValidateCondition(c);if(issue!="")errors.Add(prefix+issue);if((c.id=="SC15"||c.id=="SC22")&&!b.activeSkills.Contains(c.skill))errors.Add(Loc.F("{0}참조 스킬을 장착해야 합니다.", prefix));if(c.id=="SC06"&&c.predicted&&r.action!=RuleAction.Skill)errors.Add(Loc.F("{0}예상 명중 영역은 스킬 행동에서만 선택할 수 있습니다.", prefix));}
                }
                if(r.distance<.5f||r.distance>10||float.IsNaN(r.distance)||!Radii.Contains(r.targetRadius))errors.Add(Loc.F("{0}이동 거리 또는 목표 반경이 유효하지 않습니다.", prefix));
            }
            return errors;
        }
        public static string CompareText(Comparison op)=>new[]{"이하","이상","범위","있음 / 가능","없음","완료 임박","안","밖","일치"}[(int)op];
        public static bool UsesValue(RuleCondition c)=>Definition(c.id)?.max>Definition(c.id)?.min&&
            (c.id!="SC04"&&c.id!="SC15"||c.comparison==Comparison.AtMost)&&(c.id!="SC08"&&c.id!="SC14"||c.comparison==Comparison.Soon);
        public static string Summary(RuleCondition c)
        {
            var d=Definition(c.id);if(d==null)return c.id;
            string selected=c.id=="SC04"?Buffs[Mathf.Clamp(c.choice,0,Buffs.Length-1)]:c.id=="SC13"?States[Mathf.Clamp(c.choice,0,States.Length-1)]:c.id=="SC11"?new[]{"근접","돌진","원거리","장판","지원","폭발"}[Mathf.Clamp(c.choice,0,5)]:c.id=="SC12"?new[]{"일반","정예","보스"}[Mathf.Clamp(c.choice,0,2)]:c.id=="SC20"?new[]{"미등장","교전","처치"}[Mathf.Clamp(c.choice,0,2)]:c.id=="SC07"?new[]{"정예 또는 보스","정예","보스"}[Mathf.Clamp(c.choice,0,2)]:"";
            string context=c.id=="SC05"?Loc.F("최근 {0:0}초 / ", c.window):c.id=="SC06"?(c.predicted?Loc.T("선택 스킬 예상 영역 / "):Loc.F("자신 중심 {0:0}m / ", c.radius)):c.id=="SC15"||c.id=="SC22"?Loc.F("스킬 {0} / ", c.skill+1):"";
            string number=UsesValue(c)?$" {c.value:0.#}"+(c.comparison==Comparison.Between?$"–{c.upper:0.#}":"")+Loc.T(d.unit):"";
            return context+Loc.T(d.name)+" "+Loc.T(selected)+number+" "+Loc.T(CompareText(c.comparison));
        }
        public static string Summary(Rule r)=>r.always?Loc.T("항상 검사"):string.Join(Loc.T(" 또는 "),r.groups.Select(g=>"("+string.Join(Loc.T(" 그리고 "),g.conditions.Select(Summary))+")"));
    }
}
