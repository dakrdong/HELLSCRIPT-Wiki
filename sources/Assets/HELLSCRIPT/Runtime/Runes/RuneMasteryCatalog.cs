using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hellscript.Runes
{
    public static class RuneMasteryCatalog
    {
        public static readonly string[] Weapons={"sword","greatsword","axe","bow","crossbow","staff"};
        public static readonly string[] Names={"도검","대검","도끼","활","쇠뇌","지팡이"};
        public const int PresetCount=5;
        [Serializable] sealed class Source { public Shape[] shapes; public Cell[] geometry; }
        [Serializable] sealed class Shape { public string id; public int shapeNumber; public Cell[] cells; }
        [Serializable] sealed class Cell { public int q,r,regionGrade; public bool isStart; }
        static Source source;
        static List<RuneShapeDefinition> shapes;
        static readonly Dictionary<string,RuneBoardDefinition> boards=new Dictionary<string,RuneBoardDefinition>();
        static Source Data=>source??=JsonUtility.FromJson<Source>(Resources.Load<TextAsset>("Runes/shapes").text);
        public static IReadOnlyList<RuneShapeDefinition> Shapes=>shapes??=Data.shapes.Select(s=>new RuneShapeDefinition(s.id,s.shapeNumber,s.cells.Select(c=>new HexCell(c.q,c.r)))).ToList();
        public static RuneShapeDefinition ShapeById(string id)=>Shapes.FirstOrDefault(s=>s.Id==id)??throw new ArgumentException("Unknown rune shape: "+id);
        public static bool IsWeapon(string id)=>Array.IndexOf(Weapons,id)>=0;
        public static string Name(string id)=>Loc.T(Names[Array.IndexOf(Weapons,id)]);
        public static string EquippedWeapon(HeroSave hero)
        {
            var item=hero.inventory.FirstOrDefault(i=>i.equipped&&i.slot==0);if(item==null)return null;
            switch(ItemCatalog.Base(item).id)
            {case "B01":return "sword";case "B02":return "greatsword";case "B03":return "axe";
             case "B04":case "B05":return "bow";case "B06":return "crossbow";
             case "B07":case "B08":case "B09":return "staff";default:return null;}
        }
        public static int UnlockedGrade(AccountSave account)=>Math.Min(6,account.heroes.Max(h=>h.highestClear)/5);
        // The inverse of the line above, so a locked region can name the clear it is still waiting for.
        public static int ClearForGrade(int grade){RuneColors.CheckGrade(grade);return grade*5;}
        // Each outer region's centre carries the elite designed for the direction that region points in;
        // the board's own centre keeps the attack power it has always granted.
        static readonly string[] eliteByRegion={null,"cascade","resolve","heat","weakness","inherit","cycle"};
        public const string ElitePrefix="Elite:";
        public static bool IsElite(string meaning)=>meaning.StartsWith(ElitePrefix,StringComparison.Ordinal);
        public static string EliteId(string meaning)=>meaning.Substring(ElitePrefix.Length);
        // Combat reads elites the same way it reads a unique item's effect, by name in HeroStats.specials.
        public static string EliteSpecial(string meaning)=>"ELITE_"+EliteId(meaning).ToUpperInvariant();
        public static string EliteName(string id)
        {
            switch(id){case "heat":return Loc.T("전투 열기");case "weakness":return Loc.T("급소 노출");case "cascade":return Loc.T("연쇄 붕괴");
                case "inherit":return Loc.T("기술 전승");case "cycle":return Loc.T("순환의 핵");case "resolve":return Loc.T("불굴의 결계");
                default:throw new ArgumentOutOfRangeException(nameof(id));}
        }
        public static string EliteDescription(string id)
        {
            switch(id)
            {
                case "heat":return Loc.T("직접 적중마다 공세 +1: 중첩당 직접 피해 +2%, 최대 5중첩. 획득 간격 0.5초. 마지막 직접 적중 후 4초에 모두 해제. 지속 피해·가시·추가 발동으로 중첩을 얻지 않습니다.");
                case "weakness":return Loc.T("직접 치명타로 적중한 대상에게 8초간 자신이 주는 직접 피해 +20%. 같은 대상 재적용은 시간만 갱신. 일반 취약 표식과 별도의 신규 효과이며 이 효과는 취약 상태를 부여하지 않습니다.");
                case "cascade":return Loc.T("직접 타격으로 적 처치 시 2.5m 폭발: 공격 기준 D의 30%, 처치 타격과 같은 속성. 내부 재사용 1초. 폭발은 치명타·압도·행운의 적중·처치 재귀 발동을 만들지 않습니다.");
                case "inherit":return Loc.T("이미 해금했고 장착 중인 액티브 스킬 레벨 +1. 스킬별 개별 레벨 룬과 합산해 최대 +5. 기존 레벨 피해 계수만 강화하며 스킬·장착 슬롯을 해금하지 않습니다.");
                case "cycle":return Loc.T("실제로 소비한 자원 50마다 남은 재사용 시간이 가장 긴 장착 액티브의 시간을 1초 감소. 내부 재사용 3초. 감소 대상이 없거나 내부 재사용 중이면 해당 50 소비는 소모하고 보류하지 않습니다. 무료 발동·회복량은 누적하지 않습니다.");
                case "resolve":return Loc.T("생명력이 35% 위에서 35% 이하로 내려갈 때 최대 생명력의 20% 보호막을 4초 생성. 내부 재사용 30초. 장착·편집 저장으로 즉시 발동하지 않으며 발동 대기시간을 초기화하지 않습니다.");
                default:throw new ArgumentOutOfRangeException(nameof(id));
            }
        }
        // Cell value tier is independent of region unlock and rune color.
        static readonly string[] common={"AttackPower","Strength","Dexterity","Intelligence","Willpower","MaximumLife","AllResistance","Armor"};
        static readonly string[][] specialty={
            new[]{"AttackSpeed","CriticalStrikeChance","PhysicalDamage"},
            new[]{"CloseDamage","CriticalStrikeDamage","DamageToCrowdControlled"},
            new[]{"DamageToInjured","CriticalStrikeDamage","PhysicalDamage"},
            new[]{"AttackSpeed","DamageToHealthy","VulnerableDamage"},
            new[]{"DistantDamage","CriticalStrikeChance","VulnerableDamage"},
            new[]{"FireDamage","ColdDamage","LightningDamage","CooldownReduction"}
        };
        static readonly int[][] skills={new[]{1},new[]{0},new[]{2},new[]{7},new[]{6},new[]{12,13,14}};
        public static string SkillName(int index)
        {
            switch(index){case 0:return Loc.T("회오리");case 1:return Loc.T("도약 내려찍기");case 2:return Loc.T("분쇄 일격");case 6:return Loc.T("관통 사격");case 7:return Loc.T("다중 사격");case 12:return Loc.T("화염구");case 13:return Loc.T("눈보라");case 14:return Loc.T("연쇄 번개");default:throw new ArgumentOutOfRangeException(nameof(index));}
        }
        public static bool IsSkill(string meaning)=>meaning.StartsWith("Skill",StringComparison.Ordinal);
        public static int SkillIndex(string meaning)=>int.Parse(meaning.Split(':')[1]);
        public static string AbilityName(string meaning)
        {
            if(IsElite(meaning))return EliteName(EliteId(meaning));
            if(IsSkill(meaning)){int skill=SkillIndex(meaning);return Loc.F(meaning.StartsWith("SkillLevel:")?"{0} 스킬 레벨":meaning.StartsWith("SkillCost:")?"{0} 자원 소모 감소":"{0} 추가 피해",SkillName(skill));}
            return meaning=="AttackPower"?Loc.T("공격력"):Loc.T(StatCatalog.All[(int)Enum.Parse<StatId>(meaning)].name);
        }
        public static string Unit(string meaning)=>IsElite(meaning)?"":IsSkill(meaning)?(meaning.StartsWith("SkillLevel:")?"":"%"):meaning!="AttackPower"&&StatCatalog.All[(int)Enum.Parse<StatId>(meaning)].form==StatForm.Percent?"%":"";
        static float Value(string meaning,int grade)
        {
            // An elite is one whole effect rather than an amount, so it carries the single unit the board counts.
            if(IsElite(meaning))return 1;
            if(meaning.StartsWith("SkillLevel:"))return 1;
            if(meaning.StartsWith("SkillCost:"))return 2;
            if(meaning.StartsWith("SkillPower:"))return 3;
            float scale=1+grade*.25f;
            if(meaning=="AttackPower")return (float)Math.Round(.4f*scale,2);
            if(meaning=="MaximumLife")return 2*scale;
            if(meaning=="CriticalStrikeChance"||meaning=="CooldownReduction"||meaning=="AttackSpeed")return (float)Math.Round(.12f*scale,2);
            return (float)Math.Round((Unit(meaning)=="%"?.5f:1f)*scale,2);
        }
        public static RuneBoardDefinition Board(string weapon)
        {
            if(!IsWeapon(weapon))throw new ArgumentException("Unknown mastery weapon: "+weapon);
            if(boards.TryGetValue(weapon,out var cached))return cached;
            int wi=Array.IndexOf(Weapons,weapon);var cells=new List<RuneBoardCell>();
            for(int i=0;i<Data.geometry.Length;i++)
            {
                var g=Data.geometry[i];int local=i%61;bool special=local>=37;int numeric;string meaning;
                if(local<37){numeric=local%3;meaning=common[(local+g.regionGrade+wi)%common.Length];}
                else if(local<49){numeric=3+local%2;meaning=specialty[wi][(local+g.regionGrade)%specialty[wi].Length];}
                else
                {
                    int skill=skills[wi][(local+g.regionGrade)%skills[wi].Length];numeric=local<58?5:6;
                    // Skills without a resource cost receive damage nodes, never a inert cost reduction.
                    meaning=local>=58?"SkillLevel:"+skill:local%2==0&&skill!=1?"SkillCost:"+skill:"SkillPower:"+skill;
                }
                if(g.isStart){meaning=eliteByRegion[g.regionGrade]==null?"AttackPower":ElitePrefix+eliteByRegion[g.regionGrade];numeric=0;}
                int required=g.isStart||local%5!=4?g.regionGrade:(g.regionGrade+1)%7;
                float value=Value(meaning,numeric);string unit=Unit(meaning);
                var ability=new RuneAbilityDefinition(weapon,meaning,meaning,RuneEffectTarget.Wearer,"","","","",RuneCombatStatus.Applied,weapon+"/"+meaning);
                cells.Add(new RuneBoardCell(new HexCell(g.q,g.r),g.regionGrade,g.isStart,required,numeric,special?RuneCellKind.StructuralRare:RuneCellKind.Normal,
                    meaning,value.ToString(System.Globalization.CultureInfo.InvariantCulture),value,true,unit,"",RuneCombatStatus.Applied,ability));
            }
            return boards[weapon]=new RuneBoardDefinition(weapon,weapon,1,cells,"hellscript-rune-mastery-v1");
        }
    }
}
