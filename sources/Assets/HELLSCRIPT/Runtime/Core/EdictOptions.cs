using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Hellscript
{
    public enum EdictOptionKind { Toggle,Number,Choice,Set,Order,SkillReference }
    public sealed class EdictOptionDefinition
    {
        public readonly string id,group,label,initial;
        public readonly EdictOptionKind kind;
        public readonly string[] choices;
        public readonly float min,max,step;
        public EdictOptionDefinition(string id,string group,string label,EdictOptionKind kind,string initial,string[] choices=null,float min=0,float max=0,float step=1)
        {this.id=id;this.group=group;this.label=label;this.kind=kind;this.initial=initial;this.choices=choices??Array.Empty<string>();this.min=min;this.max=max;this.step=step;}
        public string CanonicalValue(string value,string heroClass)
        {
            if(value==null||value.Length>512)throw new ArgumentException(Loc.F("{0}: 값이 없거나 너무 깁니다.", id));
            if(kind==EdictOptionKind.Toggle){if(value!="ON"&&value!="OFF")throw new ArgumentException(Loc.F("{0}: 켜짐 또는 꺼짐을 선택하세요.", id));return value;}
            if(kind==EdictOptionKind.Number)
            {
                if(!decimal.TryParse(value,NumberStyles.Float,CultureInfo.InvariantCulture,out decimal n)||n<(decimal)min||n>(decimal)max||(n-(decimal)min)%(decimal)step!=0)throw new ArgumentException(Loc.F("{0}: 숫자 범위와 단위를 확인하세요.", id));
                return n==0?"0":n.ToString("0.###",CultureInfo.InvariantCulture);
            }
            if(kind==EdictOptionKind.SkillReference)
            {if(value!=""&&(!HuntEdict.SkillIds(heroClass).Contains(value)||choices.Length>0&&!choices.Contains(value)))throw new ArgumentException(Loc.F("{0}: 지정할 수 없는 스킬입니다.", id));return value;}
            if(kind==EdictOptionKind.Choice){if(!choices.Contains(value))throw new ArgumentException(Loc.F("{0}: 알 수 없는 선택값입니다.", id));return value;}
            var selected=value==""?Array.Empty<string>():value.Split(',');
            if(selected.Distinct(StringComparer.Ordinal).Count()!=selected.Length||selected.Any(s=>!choices.Contains(s))||kind==EdictOptionKind.Order&&selected.Length!=choices.Length)throw new ArgumentException(Loc.F("{0}: 선택 목록이 중복되거나 누락됐습니다.", id));
            return kind==EdictOptionKind.Order?value:string.Join(",",choices.Where(selected.Contains));
        }
    }
    public static class EdictOptions
    {
        static readonly string[] Legendary={"LW01","LW02","LW03","LW04","LA01","LA02","LA03","LA04","LM01","LM02","LM03","LM04","LC01","LC02","LC03"};
        static readonly string[] Sets={"SW","SWB","SA","SAB","SM","SMB"};
        static readonly string[] Rarities={"NORMAL","MAGIC","RARE","LEGENDARY","SET"};
        public static readonly EdictOptionDefinition[] Version6Global=CreateVersion6Global();
        public static readonly EdictOptionDefinition[] Version7Global=CreateVersion7Global();
        public static readonly EdictOptionDefinition[] Global=Version7Global.Select(d=>d.id=="common.order"?O(d.id,d.group,d.label,EdictRuleOrder.DefaultCategories):d).ToArray();
        // Positional share envelopes retain the original definitions as well as their order.
        public static readonly EdictOptionDefinition[] Version5Global=Version6Global.Where(d=>!PositionV6(d.id)).Select(d=>d.id=="position.mode"?C(d.id,d.group,d.label,"ORBIT","CENTER","EDGE","DISTANCE","ORBIT","STAND"):
            d.id=="position.distance"?N(d.id,d.group,d.label,.5f,10,.5f,2):d).ToArray();
        public static bool PositionV6(string id)=>id=="position.engageApproach"||id=="position.engageDistanceSource"||id=="position.engageDistance"||id=="position.lureMode";
        public static readonly EdictOptionDefinition[] WarehouseGlobal=Version5Global.Where(d=>d.id!=EquipmentRecommendation.ExcludedSlots).ToArray();
        public static readonly EdictOptionDefinition[] RecommendationGlobal=WarehouseGlobal.Where(d=>!EquipmentAutomation.NewOption(d.id)).ToArray();
        public static readonly EdictOptionDefinition[] PotionGlobal=RecommendationGlobal.Where(d=>!d.id.StartsWith(EquipmentRecommendation.Prefix,StringComparison.Ordinal)).ToArray();
        public static readonly EdictOptionDefinition[] LegacyGlobal=PotionGlobal.Where(d=>!d.id.StartsWith("potion.",StringComparison.Ordinal)).ToArray();
        public static EdictOptionDefinition[] GlobalsForVersion(int version)=>version==1?LegacyGlobal:version==2?PotionGlobal:version==3?RecommendationGlobal:version==4?WarehouseGlobal:version==5?Version5Global:version==6?Version6Global:version==7?Version7Global:Global;
        static EdictOptionDefinition T(string id,string group,string label,bool initial=false)=>new EdictOptionDefinition(id,group,label,EdictOptionKind.Toggle,initial?"ON":"OFF");
        static EdictOptionDefinition N(string id,string group,string label,float min,float max,float step,float initial)=>new EdictOptionDefinition(id,group,label,EdictOptionKind.Number,initial.ToString("0.###",CultureInfo.InvariantCulture),min:min,max:max,step:step);
        static EdictOptionDefinition C(string id,string group,string label,string initial,params string[] choices)=>new EdictOptionDefinition(id,group,label,EdictOptionKind.Choice,initial,choices);
        static EdictOptionDefinition S(string id,string group,string label,params string[] choices)=>new EdictOptionDefinition(id,group,label,EdictOptionKind.Set,"",choices);
        static EdictOptionDefinition O(string id,string group,string label,params string[] choices)=>new EdictOptionDefinition(id,group,label,EdictOptionKind.Order,string.Join(",",choices),choices);
        static EdictOptionDefinition R(string id,string group,string label,params string[] choices)=>new EdictOptionDefinition(id,group,label,EdictOptionKind.SkillReference,"",choices);
        static EdictOptionDefinition[] CreateVersion6Global()
        {
            var d=new List<EdictOptionDefinition>();
            foreach(string shape in new[]{"ground","area","direct"})
            {d.Add(C("dodge."+shape+".policy","dodge",shape=="ground"?"지속 피해 장판":shape=="area"?"순간 범위 공격":"직접 타격·투사체","OFF","OFF","ALWAYS","DAMAGE"));d.Add(N("dodge."+shape+".hpPercent","dodge","예상 HP 손실 기준",1,100,1,10));}
            d.AddRange(new[]{S("dodge.control","dodge","특수 위험","STUN","FREEZE","ROOT","PULL"),T("dodge.overlap","dodge","겹치는 공격 합산"),N("dodge.window","dodge","앞으로 합산할 시간",.1f,5,.1f,1),N("dodge.totalHpPercent","dodge","합산 HP 손실 기준",1,100,1,20),C("dodge.method","dodge","회피 수단","WALK_FIRST","WALK_FIRST","SKILL_FIRST","SKILL_IF_NEEDED"),C("dodge.interrupt","dodge","공격 중 회피","FINISH","CANCEL_ALLOWED","FINISH")});
            d.AddRange(new[]{T("survival.lowHp","survival","체력 위기 대응"),N("survival.lowHpPercent","survival","후퇴 시작 HP",1,99,1,30),T("survival.lethal","survival","치명적 피해 대응"),T("survival.dot","survival","부착 지속 피해 대응"),N("survival.dotHpPercent","survival","부착 피해의 초당 HP 비율",1,100,1,10),R("survival.defenseSkill","survival","지정 방어기","W04","W05","A03","M04-3","M06"),R("survival.escapeSkill","survival","지정 회피기","W02","A04","M04"),R("survival.fallbackSkill","survival","다른 지정 생존기","W02","W04","W05","A03","A04","M04","M04-3","M06"),O("survival.order","survival","긴급 행동 순서","DEFENSE","ESCAPE","WALK"),C("survival.fallback","survival","회피기 사용 불가 시","KEEP_FIGHTING","WALK","OTHER_SKILL","KEEP_FIGHTING"),T("survival.reserveSkill","survival","긴급 생존용 회피기 보존"),T("survival.reserveResource","survival","회피기 1회분 자원 보존"),T("survival.potion","survival","자동 물약",true),N("survival.potionHpPercent","survival","물약 HP 기준",1,100,1,40),N("survival.returnHpPercent","survival","전투 복귀 HP",2,100,1,50)});
            d.AddRange(new[]{C("position.engage","position","첫 교전 위치","APPROACH","APPROACH","STAND","DISTANCE","LURE"),C("position.mode","position","전투 중 위치","DISTANCE","CENTER","EDGE","DISTANCE","STAND"),C("position.distanceSource","position","유지 거리 기준","EXPLICIT","BASIC_RANGE","EXPLICIT"),N("position.distance","position","유지 거리",0,10,.5f,2),T("position.clockwise","position","시계 방향 선회",true),T("position.ownArea","position","내 영역 유지"),C("position.surrounded","position","포위 대응","FIGHT_CENTER","FIGHT_CENTER","MOVE_EDGE","ESCAPE"),T("position.gather","position","여러 무리 몰이"),N("position.gatherCount","position","목표 인원",1,20,1,3),T("position.addEnemies","position","추가 적 유인"),N("position.gatherTime","position","최대 몰이 시간",1,30,1,5)});
            d.AddRange(new[]{C("target.default","target","기본 대상","NEAREST","NEAREST","LOW_HP","HIGH_HP","DENSE"),S("target.specialEnabled","target","우선할 특수 대상","BOSS","ELITE","SUPPORT","RANGED"),O("target.specialOrder","target","특수 대상 순서","BOSS","ELITE","SUPPORT","RANGED"),T("target.interruptCaster","target","위험 시전 방해 우선"),C("target.switch","target","대상 유지","PRIORITY","UNTIL_DEATH","PRIORITY"),N("target.chaseDistance","target","최대 추적 거리",1,100,1,30),N("target.chaseTime","target","최대 추적 시간",1,120,1,20),C("target.lostSight","target","시야 밖 대상","LAST_SEEN","STOP","LAST_SEEN"),T("target.goblin","target","황금 고블린 우선"),N("target.goblinDistance","target","고블린 추적 거리",1,100,1,30),N("target.goblinTime","target","고블린 추적 시간",1,120,1,20),C("target.bossFight","target","보스전 대상","BOSS","BOSS","ADDS","DANGEROUS_SUPPORT")});
            foreach(string rarity in Rarities)d.Add(C("loot."+rarity,"loot",rarity+" 회수","AFTER_COMBAT","IGNORE","PASSING","AFTER_COMBAT","PRIORITY"));
            d.AddRange(new[]{C("loot.class","loot","장비 직업","ALL","ALL","OWN"),new EdictOptionDefinition("loot.slots","loot","장비 부위",EdictOptionKind.Set,"WEAPON,HEAD,CHEST,HANDS,FEET,WAIST,AMULET,RING",new[]{"WEAPON","HEAD","CHEST","HANDS","FEET","WAIST","AMULET","RING"}),N("loot.itemLevel","loot","최소 아이템 레벨",1,100,1,1),T("loot.exceptLegendary","loot","전설 필터 예외"),T("loot.exceptSet","loot","세트 필터 예외"),S("loot.exceptEffects","loot","효과별 필터 예외",Legendary),C("loot.gold","loot","금화 회수","PASSING","PASSING","AFTER_COMBAT","PRIORITY"),N("loot.minimumGold","loot","추적할 최소 금액",0,1000000,1,0),C("loot.material","loot","재료 회수","AFTER_COMBAT","IGNORE","PASSING","AFTER_COMBAT","PRIORITY"),C("loot.gem","loot","보석 회수","AFTER_COMBAT","IGNORE","PASSING","AFTER_COMBAT","PRIORITY"),C("loot.core","loot","코어 회수","AFTER_COMBAT","IGNORE","PASSING","AFTER_COMBAT","PRIORITY"),N("loot.distance","loot","최대 회수 경로 거리",1,100,1,20),C("loot.danger","loot","위험 지역 회수","WAIT","WAIT","SAFE_PATH","BELOW_DODGE_THRESHOLD"),C("loot.finish","loot","종료 전 회수","COLLECT","COLLECT","LEAVE")});
            d.AddRange(new[]{C("bag.trigger","bag","정리 시작","FULL","FULL","FREE_SLOTS"),N("bag.freeSlots","bag","빈칸 기준",0,100,1,0),C("bag.policy","bag","가방 부족 대응","PORTAL","PORTAL","REPLACE","IGNORE"),C("bag.replacementRank","bag","가치 비교","RARITY","RARITY","RARITY_LEVEL_PRICE"),C("bag.replacementFail","bag","교체 불가 시","LEAVE","LEAVE","PORTAL"),C("bag.cleanupAt","bag","자동 정리 시점","OFF","OFF","PORTAL","AFTER_RIFT"),T("bag.allowLegendaryDisposal","bag","전설·세트 자동 처리 별도 허용"),T("bag.protectEnhanced","bag","강화 장비 추가 보호"),T("bag.protectSocketed","bag","보석 장비 추가 보호"),S("bag.protectEffects","bag","전설 효과 보호",Legendary),S("bag.protectSets","bag","지정 세트 보호",Sets),C(EquipmentAutomation.Admission,"bag","창고가 가득 찼을 때 입고","KEEP","PRICE","SCORE","OLDEST","KEEP"),C("bag.warehouseFull","bag","창고가 가득 찼을 때 반복 사냥","KEEP","STOP","KEEP"),C("bag.stillFull","bag","정리 후 공간 부족","STOP","STOP","LIMIT_LOOT")});
            foreach(string rarity in Rarities)d.Add(C("bag.cleanup."+rarity,"bag",rarity+" 자동 정리","KEEP","KEEP","SELL","SALVAGE","WAREHOUSE"));
            d.AddRange(new[]{C("explore.mode","explore","탐색 방식","RIGHT_WALL","NEAREST_UNSEEN","LEFT_WALL","RIGHT_WALL","DETECTED_ENEMIES"),O("explore.order","explore","발견물 순서","ENEMIES","LOOT","CHESTS","SHRINES"),T("explore.chests","explore","상자 이용",true),C("explore.normalChest","explore","일반 상자","ON_PATH","IGNORE","ON_PATH","PRIORITY"),C("explore.sealedChest","explore","봉인 상자","ON_PATH","IGNORE","ON_PATH","PRIORITY"),N("explore.chestDetour","explore","상자 우회 거리",0,30,1,12),T("explore.chestsAfterBoss","explore","보스전 중에도 상자 탐색"),C("explore.cursedChest","explore","저주 상자","OFF","OFF","AFTER_COMBAT","CONDITIONAL"),N("explore.cursedHpPercent","explore","저주 상자 최소 HP",1,100,1,70),R("explore.cursedDefense","explore","저주 상자 지정 생존기","W02","W04","W05","A03","A04","M04","M04-3","M06"),N("explore.cursedMinimumTime","explore","저주 상자 최소 남은 시간",0,300,5,90),C("explore.shrine","explore","성소 이용","FOUND","OFF","FOUND","NEEDED"),T("explore.guideShrine","explore","길잡이 성소",true),T("explore.resolveShrine","explore","결의 성소",true),N("explore.shrineDetour","explore","성소 우회 거리",0,30,1,12),C("explore.bossUnlocked","explore","보스 해금 시","FINISH_CURRENT","BOSS","FINISH_CURRENT","EXPLORE"),T("explore.lowTime","explore","시간 부족 시 클리어 우선"),N("explore.lowTimeSeconds","explore","시간 부족 기준",5,300,5,30),S("explore.lowTimeLoot","explore","시간 부족 시 회수 예외",Rarities)});
            d.AddRange(new[]{T("repeat.enabled","repeat","자동 반복"),C("repeat.success","repeat","성공 후","SAME","SAME","NEXT","STOP"),C("repeat.failure","repeat","실패 후","SAME","SAME","LOWER","STOP"),N("repeat.failures","repeat","연속 실패 상한",1,20,1,3),N("repeat.count","repeat","완료 횟수 상한·0은 제한 없음",0,10000,1,0),N("repeat.minutes","repeat","실제 세션 분·0은 제한 없음",0,1440,1,0),N("repeat.gold","repeat","목표 금화·0은 사용 안 함",0,100000000,1,0),S("repeat.effects","repeat","목표 전설 효과",Legendary),S("repeat.sets","repeat","목표 세트",Sets),N("repeat.stage","repeat","목표 단계·0은 사용 안 함",0,1000,1,0),N("repeat.freeSlots","repeat","시작 전 가방 여유",0,100,1,0),N("repeat.resultSeconds","repeat","결과 대기 시간",0,30,.5f,3),O("common.order","common","공통 행동 우선순위","EMERGENCY","DODGE","PRIORITY_LOOT","ATTACK","AFTER_COMBAT_LOOT","EXPLORE")});
            d.AddRange(PotionEdict.Options);d.AddRange(EquipmentRecommendation.Options());
            d.AddRange(new[]{C("position.engageApproach","position","접근 방식","SKILL","SKILL","WALK"),C("position.engageDistanceSource","position","첫 교전 유지 거리 기준","BASIC_RANGE","BASIC_RANGE","EXPLICIT"),
                N("position.engageDistance","position","첫 교전 유지 거리",0,10,.5f,2),C("position.lureMode","position","길목 찾는 방법","NEARBY","KNOWN","SCOUT","NEARBY")});return d.ToArray();
        }
        public static bool RemovedTarget(string id)=>id=="target.default"||id=="target.specialEnabled"||id=="target.specialOrder"||id=="target.interruptCaster"||id=="target.goblinDistance"||id=="target.goblinTime";
        static EdictOptionDefinition[] CreateVersion7Global()
        {
            var d=Version6Global.Where(o=>!RemovedTarget(o.id)).Select(o=>o.id=="target.goblin"?T(o.id,o.group,o.label,true):o.id=="target.chaseDistance"?N(o.id,o.group,o.label,5,50,1,30):
                o.id=="target.chaseTime"?N(o.id,o.group,o.label,5,30,1,20):o).ToList();
            d.AddRange(new[]{O("target.priorityGrade","target","몬스터 등급",EdictTargetPolicy.DefaultGrade),O("target.priorityType","target","몬스터 타입",EdictTargetPolicy.DefaultType),
                O("target.priorityStatus","target","상태 타입",EdictTargetPolicy.DefaultStatus),O("target.priorityCategories","target","판정 순서",EdictTargetPolicy.DefaultCategories),
                T("target.goblinChase","target","고블린 추적",true),T("target.goblinAllOut","target","고블린 필살")});return d.ToArray();
        }
        public static EdictOptionDefinition[] ForAction(string id,string heroClass)
        {
            const string group="skill";
            switch(id)
            {
                case "W01":return new[]{T("channel.resourceStop",group,"자원 기준으로 유지 종료"),N("channel.resourcePercent",group,"종료 자원",0,100,1,0),N("channel.emptySeconds",group,"범위 안 적이 없을 때 종료",0,5,.1f,.5f)};
                case "W03":case "A01":return new[]{C("aim.direction",group,"공격 방향","TARGET","TARGET","MOST_HITS")};
                case "W04":return new[]{C("control.reuse",group,"중복 제어","ALLOW","ALLOW","NEW_TARGET_REQUIRED")};
                case "M06":return new[]{C("control.reuse",group,"중복 제어","ALLOW","ALLOW","NEW_TARGET_REQUIRED"),T("charge.check",group,"연쇄 충전 횟수 검사"),N("charge.count",group,"남은 연쇄 충전",0,3,1,0)};
                case "A02":return new[]{T("aim.focus",group,"단일 대상 집중"),N("aim.arrows",group,"예상 동일 대상 화살 수",1,3,1,1)};
                case "A03":return new[]{C("area.preserve",group,"기존 덫 보존","REPLACE","REPLACE","WAIT_AT_CAP"),C("area.overlap",group,"영역 중복","ALLOW","ALLOW","NEW_AREA_REQUIRED")};
                case "A05":return new[]{C("mark.transfer",group,"표식 이전","PRIORITY","KEEP_UNTIL_EXPIRED","PRIORITY"),T("mark.remaining",group,"표식 남은 시간 검사"),N("mark.seconds",group,"표식 갱신 시간",0,10,.5f,2)};
                case "A06":return new[]{T("buff.chargeCheck",group,"남은 사격 횟수 검사"),N("buff.charges",group,"남은 사격 횟수",0,3,1,0),T("followup.ready",group,"후속 사격 실행 가능 검사"),N("followup.resourceShots",group,"남길 후속 사격 자원 횟수·0은 사용 안 함",0,3,1,0)};
                case "M02":return new[]{C("area.preserve",group,"기존 눈보라 보존","REPLACE","REPLACE","WAIT_AT_CAP","WAIT_IF_TARGET_INSIDE")};
                case "M03":return new[]{T("charge.check",group,"연쇄 충전 횟수 검사"),N("charge.count",group,"남은 연쇄 충전",0,3,1,0)};
                case "M04":return new[]{C("escape.improvement",group,"이동의 최소 개선","VALID_DESTINATION","VALID_DESTINATION","LESS_DANGER_OR_DISTANCE")};
                case "BASIC":return heroClass=="WARRIOR"?new[]{T("basic.keepDiscountTarget",group,"절제 연계 대상 유지")}:heroClass=="RANGER"?new[]{T("basic.preferEnhancedSkill",group,"강화된 액티브 사격 먼저 검토")}:Array.Empty<EdictOptionDefinition>();
                default:return Array.Empty<EdictOptionDefinition>();
            }
        }
        public static List<EdictOption> Defaults(IEnumerable<EdictOptionDefinition> definitions)=>definitions.Select(d=>new EdictOption(d.id,d.initial)).ToList();
        public static List<EdictOption> Canonical(IEnumerable<EdictOptionDefinition> definitions,List<EdictOption> values,string heroClass)
        {
            if(values==null||values.Any(v=>v==null||v.id==null)||values.Select(v=>v.id).Distinct(StringComparer.Ordinal).Count()!=values.Count)throw new ArgumentException("설정 목록이 없거나 ID가 중복되었습니다.");
            var expected=definitions.ToArray();var known=new HashSet<string>(expected.Select(d=>d.id),StringComparer.Ordinal);
            if(values.Count!=expected.Length||values.Any(v=>!known.Contains(v.id)))throw new ArgumentException("설정 항목이 누락됐거나 현재 버전에 없는 항목이 있습니다.");
            var byId=values.ToDictionary(v=>v.id,StringComparer.Ordinal);
            return expected.Select(d=>new EdictOption(d.id,d.CanonicalValue(byId[d.id].value,heroClass))).ToList();
        }
    }
}
