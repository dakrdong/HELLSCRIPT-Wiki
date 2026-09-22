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
            var item=EquipmentSlots.At(hero,0,0)??hero.inventory.FirstOrDefault(i=>i.equipped&&i.slot==0&&!EquipmentSlots.Offhand(i));if(item==null)return null;
            switch(ItemCatalog.Base(item).id)
            {case "B01":case "B30":return "sword";case "B02":return "greatsword";case "B03":return "axe";
             case "B04":case "B05":return "bow";case "B06":return "crossbow";
             case "B07":case "B08":case "B09":case "B25":return "staff";default:return null;}
        }
        public static int UnlockedGrade(AccountSave account)=>Math.Min(6,account.heroes.Max(h=>h.highestClear)/5);
        // Retained only for legacy save migration and historical verification tools.
        public static int ClearForGrade(int grade){RuneColors.CheckGrade(grade);return grade*5;}
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
                case "weakness":return Loc.T("직접 치명타로 적중한 대상에게 4초간 자신이 주는 직접 피해 +10%. 같은 대상 재적용은 시간만 갱신. 일반 취약 표식과 별도의 신규 효과이며 이 효과는 취약 상태를 부여하지 않습니다.");
                case "cascade":return Loc.T("직접 타격으로 적 처치 시 2.5m 폭발: 공격 기준 D의 30%, 처치 타격과 같은 속성. 내부 재사용 1초. 폭발은 치명타·압도·행운의 적중·처치 재귀 발동을 만들지 않습니다.");
                case "inherit":return Loc.T("이미 해금했고 장착 중인 액티브 스킬 레벨 +1. 스킬별 개별 레벨 룬과 합산해 최대 +5. 기존 레벨 피해 계수만 강화하며 스킬·장착 슬롯을 해금하지 않습니다.");
                case "cycle":return Loc.T("실제로 소비한 자원 50마다 남은 재사용 시간이 가장 긴 장착 액티브의 시간을 1초 감소. 내부 재사용 3초. 감소 대상이 없거나 내부 재사용 중이면 해당 50 소비는 소모하고 보류하지 않습니다. 무료 발동·회복량은 누적하지 않습니다.");
                case "resolve":return Loc.T("생명력이 35% 위에서 35% 이하로 내려갈 때 최대 생명력의 20% 보호막을 4초 생성. 내부 재사용 30초. 장착·편집 저장으로 즉시 발동하지 않으며 발동 대기시간을 초기화하지 않습니다.");
                default:throw new ArgumentOutOfRangeException(nameof(id));
            }
        }
        public static string SkillName(int index)
        {
            switch(index){case 0:return Loc.T("회오리");case 1:return Loc.T("도약 내려찍기");case 2:return Loc.T("분쇄 일격");case 6:return Loc.T("관통 사격");case 7:return Loc.T("다중 사격");case 12:return Loc.T("화염구");case 13:return Loc.T("눈보라");case 14:return Loc.T("연쇄 번개");default:throw new ArgumentOutOfRangeException(nameof(index));}
        }
        public static bool IsSkill(string meaning)=>meaning.StartsWith("Skill",StringComparison.Ordinal);
        public static int SkillIndex(string meaning)=>int.Parse(meaning.Split(':')[1]);
        public static string AbilityName(string meaning)
        {
            var reference=RuneV13Catalog.ForMeaning(meaning);if(reference!=null)return Loc.T(reference.name);
            if(IsElite(meaning))return EliteName(EliteId(meaning));
            if(IsSkill(meaning)){int skill=SkillIndex(meaning);return Loc.F(meaning.StartsWith("SkillLevel:")?"{0} 스킬 레벨":meaning.StartsWith("SkillCost:")?"{0} 자원 소모 감소":"{0} 추가 피해",SkillName(skill));}
            return meaning=="AttackPower"?Loc.T("공격력"):Loc.T(StatCatalog.All[(int)Enum.Parse<StatId>(meaning)].name);
        }
        public static string Unit(string meaning)=>RuneV13Catalog.ForMeaning(meaning)?.unit??"";

        public static RuneBoardDefinition Board(string weapon)
        {
            if(!IsWeapon(weapon))throw new ArgumentException("Unknown mastery weapon: "+weapon);
            if(!boards.TryGetValue(weapon,out var board))boards[weapon]=board=RuneV13Catalog.Build(weapon);
            return board;
        }
    }
}
