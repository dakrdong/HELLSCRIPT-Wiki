using System;
using System.Collections.Generic;
using System.Linq;

namespace Hellscript
{
    public sealed class EdictCoreOptionDefinition
    {
        public readonly string id,label,initial;
        public readonly IReadOnlyList<string> choices,choiceLabels;
        public readonly decimal min,max,step;
        public bool Numeric=>choices.Count==0;
        internal EdictCoreOptionDefinition(string id,string label,string initial,string values,string labels)
        {this.id=id;this.label=label;this.initial=initial;choices=Array.AsReadOnly(values.Split('|'));choiceLabels=Array.AsReadOnly(labels.Split('|'));}
        internal EdictCoreOptionDefinition(string id,string label,decimal min,decimal max,decimal step,decimal initial)
        {this.id=id;this.label=label;this.min=min;this.max=max;this.step=step;this.initial=initial.ToString(System.Globalization.CultureInfo.InvariantCulture);choices=Array.AsReadOnly(Array.Empty<string>());choiceLabels=Array.AsReadOnly(Array.Empty<string>());}
        public string CanonicalValue(string value)
        {
            if(value==null||value.Length>64)throw new ArgumentException(Loc.F("{0}: 값이 없거나 너무 깁니다.", id));
            if(!Numeric){if(!choices.Contains(value))throw new ArgumentException(Loc.F("{0}: 지원하지 않는 선택입니다.", id));return value;}
            if(!decimal.TryParse(value,System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.InvariantCulture,out decimal n)||n<min||n>max||(n-min)%step!=0)
                throw new ArgumentException(Loc.F("{0}: 숫자 범위와 입력 단위를 확인하세요.", id));
            return n==0?"0":n.ToString("0.###",System.Globalization.CultureInfo.InvariantCulture);
        }
        public string Describe(string value)
        {
            value=CanonicalValue(value);if(Numeric)return value+"m";
            return choiceLabels[choices.ToList().IndexOf(value)];
        }
    }
    public static class HuntEdictV2Options
    {
        static EdictCoreOptionDefinition C(string owner,int n,string label,string initial,string values,string labels)=>new EdictCoreOptionDefinition($"HEC-{owner}-{n:00}",label,initial,values,labels);
        static EdictCoreOptionDefinition On(string owner,string label="자동 사용")=>C(owner,1,label,"ON","ON|OFF","켜기|끄기");
        static readonly IReadOnlyDictionary<string,IReadOnlyList<EdictCoreOptionDefinition>> Catalog=CreateCatalog();
        static IReadOnlyDictionary<string,IReadOnlyList<EdictCoreOptionDefinition>> CreateCatalog()
        {
            var d=new Dictionary<string,IReadOnlyList<EdictCoreOptionDefinition>>(StringComparer.Ordinal);
            void Add(string id,params EdictCoreOptionDefinition[] options)=>d.Add(id,Array.AsReadOnly(options));
            Add("W01",On("W01"),
                C("W01",2,"시작할 교전","ANY","ANY|GROUP|ELITE","적이 있으면|무리 상대|정예·보스 상대"),
                C("W01",3,"회전 중 이동","GLOBAL","GLOBAL|FOLLOW|EDGE|STAND|CENTER","전역 따르기|대상 추적|무리 가장자리|제자리|무리 중심 진입"),
                C("W01",4,"회전 종료 방식","CONTINUE","CONTINUE|THREE_SECONDS|LEAP_CHARGE","연속 유지|3초씩 끊기|도약 충전 연계"));
            Add("W02",On("W02"),
                C("W02",2,"도약 역할","BOTH","ATTACK|ESCAPE|BOTH","공격 진입|생존 이탈|둘 다"),
                C("W02",3,"공격 착지 위치","TARGET","TARGET|DENSE|EDGE","대상 주변|밀집 중심|무리 가장자리"),
                new EdictCoreOptionDefinition("HEC-W02-04","공격 도약 최소 거리",2,8,.5m,4),
                C("W02",5,"착지 후 공격 선호","GLOBAL","GLOBAL|W01|W03","전역 따르기|회오리 우선|분쇄 일격 우선"));
            Add("W03",On("W03"),
                C("W03",2,"우선 대상","GLOBAL","GLOBAL|CURRENT|ELITE|LOW_HP","전역 따르기|현재 대상|정예·보스|낮은 HP"),
                C("W03",3,"공격 방향","TARGET","TARGET|MOST_HITS|ISOLATED","우선 대상 중심|많은 적 포함|한 명만 포함"),
                C("W03",4,"사용할 기회","READY","READY|GROUP|CRUSH_CHARGE","사용 가능할 때|무리 처리|분쇄 충전이 있을 때"));
            Add("W04",On("W04"),
                C("W04",2,"일반 적에게 쓸 때","GROUP","GROUP|INTERRUPT|RETREAT","무리 처리|위험 시전 방해|후퇴 보조"),
                C("W04",3,"이미 제어된 적","NEW_CONTROL","DAMAGE|NEW_CONTROL|EXPIRING","피해를 위해 사용|제어가 없는 적이 있을 때|제어가 끝나갈 때"),
                C("W04",4,"보스 사용 방식","STAGGER","DAMAGE|STAGGER|COMPLETE","피해용|제압 누적|제압 완성 시점"));
            Add("W05",On("W05"),
                C("W05",2,"보호막 사용 시점","PREVENT","EMERGENCY|PREVENT|ELITE_ENGAGE","긴급 생존|위험 예방|정예 교전 진입"),
                C("W05",3,"다른 보호막과 사용","TOGETHER","TOGETHER|AFTER","함께 사용|다른 보호막 소진 후"),
                C("W05",4,"도약 방어와 사용","TOGETHER","TOGETHER|AFTER","함께 사용|도약 방어 종료 후"));
            Add("W06",On("W06"),
                C("W06",2,"함성 운영 목적","BOTH","RESOURCE|DAMAGE|BOTH","자원 보충|공격 강화|둘 다"),
                C("W06",3,"강화할 교전","ELITE","ALL|ELITE|BOSS","모든 교전|정예·보스|보스만"),
                C("W06",4,"공격 강화 준비 시점","IN_RANGE","ENGAGE|IN_RANGE","교전이 시작되면|다음 공격이 닿을 때"),
                C("W06",5,"비전투 자원 보충","OFF","ON|OFF","허용|사용 안 함"));
            Add("WARRIOR",On("WARRIOR"),
                C("WARRIOR",2,"기본 공격 역할","FILL","FILL|RESOURCE|FINISH","일반 공격 보완|자원 보충|마무리"),
                C("WARRIOR",3,"우선 대상","GLOBAL","GLOBAL|CURRENT|NEAREST|LOW_HP","전역 따르기|현재 대상|가까운 적|낮은 HP"),
                C("WARRIOR",4,"절제 충전 대상 유지","NORMAL","NORMAL|UNTIL_DISCOUNT","일반 선택 유지|충전이 생길 때까지 같은 적"));
            Add("A01",On("A01"),
                C("A01",2,"사용할 상황","ELITE_OR_GROUP","ONE|TWO|ELITE_OR_GROUP","한 명부터|두 명 이상|정예·보스 또는 두 명 이상"),
                C("A01",3,"우선할 대상","GLOBAL","GLOBAL|CURRENT|ELITE|OWN_POISON","전역 대상|현재 대상|정예·보스|자기 덫에 중독된 적"),
                C("A01",4,"사선 선택","MOST_HITS","TARGET|MOST_HITS","대상 방향|대상을 포함해 많이 관통"));
            Add("A02",On("A02"),
                C("A02",2,"사격 목적","GROUP","IMMEDIATE|GROUP|FOCUS","즉시 공격|무리 정리|단일 집중"),
                C("A02",3,"우선할 대상","GLOBAL","GLOBAL|CURRENT|ELITE","전역 대상|현재 대상|정예·보스"),
                C("A02",4,"사격 위치","GLOBAL","GLOBAL|FAR|FOCUS","전역 위치|먼 사거리|집중 명중 위치"));
            Add("A03",On("A03","자동 설치"),
                C("A03",2,"설치 위치","DENSE","TARGET|DENSE|PATH|SELF","대상 발밑|무리 중심|추격 경로|자신 아래"),
                C("A03",3,"설치할 상황","ELITE_OR_GROUP","ONE|TWO|ELITE_OR_GROUP|AMBUSH","적 한 명부터|적 두 명 이상|정예·보스 또는 두 명 이상|교전 전 매복"),
                C("A03",4,"덫 두 개가 남았을 때","KEEP","KEEP|REPLACE","기존 덫 유지|오래된 덫 교체"),
                C("A03",5,"기존 덫과 겹침","NEW_TARGET","ALLOW|NEW_TARGET","겹쳐도 설치|새 적이 있을 때만"));
            Add("A04",On("A04"),
                C("A04",2,"일반 전투에서 사용","DISTANCE","OFF|DISTANCE|TRAP_LINK|PIERCE_LINK","사용 안 함|거리 복구|덫 연계|관통 연계"),
                C("A04",3,"착지 위치","SAFE","SAFE|SPARSE|DISTANCE|OWN_TRAP","위험이 적은 곳|적이 적은 곳|전역 교전 거리|자기 덫 방향"),
                C("A04",4,"착지 후 보행","GLOBAL","GLOBAL|RETREAT","전역 따르기|희망 거리까지 후퇴"));
            Add("A05",On("A05"),
                C("A05",2,"표식할 대상","ELITE","GLOBAL|CURRENT|ELITE|HIGH_HP","전역 대상|현재 대상|정예·보스|남은 HP가 가장 많은 적"),
                C("A05",3,"부여 시점","LONG_FIGHT","IMMEDIATE|LONG_FIGHT","대상이 생기면|오래 싸울 적에게"),
                C("A05",4,"다른 적으로 이전","HIGHER_PRIORITY","KEEP|HIGHER_PRIORITY|NEW_TARGET","기존 표식 유지|더 높은 우선순위면 이전|새 대상에 이전"));
            Add("A06",On("A06"),
                C("A06",2,"강화할 교전","ANY","ANY|ELITE|POISON_SPREAD","적이 있으면|정예·보스 교전|독 전염 교전"),
                C("A06",3,"준비를 확인할 사격","BASIC","BASIC|A01|A02","기본 사격|관통 사격|다중 사격"),
                C("A06",4,"교전의 기준 대상","GLOBAL","GLOBAL|CURRENT|ELITE|OWN_POISON","전역 대상|현재 대상|정예·보스 우선|자기 덫 중독 우선"));
            Add("RANGER",On("RANGER"),
                C("RANGER",2,"사용할 상황","FILL","FILL|RESOURCE|DISCOUNT","기회가 있을 때|자원 보충|절제 할인 준비"),
                C("RANGER",3,"우선할 대상","GLOBAL","GLOBAL|CURRENT|LOW_HP|ELITE","전역 대상|현재 대상|낮은 HP|정예·보스"),
                C("RANGER",4,"같은 대상 유지","GLOBAL","GLOBAL|UNTIL_DEATH|UNTIL_DISCOUNT","전역 방침|처치할 때까지|절제 할인 완성까지"),
                C("RANGER",5,"그림자 강화 중 기본 사격","ALLOW","ALLOW|HOLD","그대로 사용|기본 사격 보류"));
            Add("M01",On("M01"),
                C("M01",2,"사용 방식","STEADY","STEADY|GROUP|EXPOSURE","꾸준히 공격|무리 위주|세트 노출 우선"),
                C("M01",3,"우선 대상","GLOBAL","GLOBAL|ELITE|SUPPORT|DENSE","전역 대상|정예·보스|지원형|밀집한 적"),
                C("M01",4,"조준 방식","PREDICTED","CURRENT|PREDICTED","현재 위치|예상 착탄 위치"));
            Add("M02",On("M02"),
                C("M02",2,"사용 방식","NEW_AREA","NEW_AREA|KEEP_TARGET|GROUP","새 영역 확보|교전 대상 유지|무리 위주"),
                C("M02",3,"설치 위치","DENSE","TARGET|DENSE|SELF|PATH","대상 위치|무리 중심|자신 아래|이동 경로"),
                C("M02",4,"두 장판이 남아 있을 때","REPLACE","KEEP|REPLACE","기존 장판 보존|오래된 장판 교체"),
                C("M02",5,"장판 이동","FIXED","FIXED|FOLLOW","위치 고정|대상 추적"));
            Add("M03",On("M03"),
                C("M03",2,"사용 방식","SINGLE","LINKED|SINGLE|SAVE_CHARGE","연결되는 적 위주|단독 적도 공격|충전 낭비 줄이기"),
                C("M03",3,"첫 대상","ALL","ALL|ELITE|SUPPORT","전체 적|정예·보스 우선|지원형 우선"),
                C("M03",4,"연결 선택 기준","UNIQUE","UNIQUE|TOTAL_HITS","여러 적에게 명중|총 타격 수 확보"));
            Add("M04",On("M04"),
                C("M04",2,"사용 목적","SURVIVAL","SURVIVAL|DISTANCE","생존용|거리 조정 겸용"),
                C("M04",3,"도착 위치","SAFE","SAFE|SPARSE|DISTANCE","위험이 적은 곳|적이 적은 곳|교전 거리 복구"),
                C("M04",4,"도착지 허용 기준","GLOBAL","GLOBAL|NO_DAMAGE","전역 위험 기준 충족|감지 피해 위험 없음"));
            Add("M04-3",On("M04-3"),
                C("M04-3",2,"방어 시점","PREVENT","EMERGENCY|PREVENT|ENGAGE","긴급 상황|피격 대비|교전 시작"),
                C("M04-3",3,"다른 보호막이 있을 때","TOGETHER","AFTER|TOGETHER","소진·만료 후 사용|필요하면 함께 사용"),
                C("M04-3",4,"일반 사용을 허용할 교전","ALL","ALL|ELITE|BOSS","모든 교전|정예·보스|보스만"));
            Add("M06",On("M06"),
                C("M06",2,"사용 목적","CONTROL","CONTROL|GROUP|CHAIN_CHARGE","위기 제어|군중 공격|연쇄 충전 준비"),
                C("M06",3,"시전 위치","HERE","HERE|APPROACH","현재 위치에서만|현재 교전 안에서 접근"),
                C("M06",4,"보스에게 사용","PURPOSE","PURPOSE|STAGGER_ONLY|HOLD_ALONE","선택한 목적대로|제압 기여가 가능할 때만|보스 단독이면 보류"));
            Add("MAGE",On("MAGE"),
                C("MAGE",2,"사용 방식","FILL","FILL|RESOURCE|DISCOUNT","빈틈마다 공격|자원이 부족할 때|절제 충전 준비"),
                C("MAGE",3,"우선 대상","GLOBAL","GLOBAL|CURRENT|LOW_HP|OWN_BLIZZARD","전역 대상|현재 대상|남은 HP가 낮은 적|자기 눈보라 안의 적"),
                C("MAGE",4,"조준 방식","PREDICTED","CURRENT|PREDICTED","현재 위치|예상 적중 위치"));
            return new System.Collections.ObjectModel.ReadOnlyDictionary<string,IReadOnlyList<EdictCoreOptionDefinition>>(d);
        }
        public static IReadOnlyList<EdictCoreOptionDefinition> ForSkill(string skillId,string heroClass)
        {
            if(HuntEdict.ClassIndex(heroClass)<0||skillId!="BASIC"&&!HuntEdict.SkillIds(heroClass).Contains(skillId))throw new ArgumentException("현재 직업의 기본 공격 또는 알려진 스킬을 선택하세요.");
            return Catalog[skillId=="BASIC"?heroClass:skillId];
        }
        public static IEnumerable<EdictCoreOptionDefinition> All=>Catalog.Values.SelectMany(x=>x);
    }
}
