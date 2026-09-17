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
            if(IsSkill(meaning)){int skill=SkillIndex(meaning);return Loc.F(meaning.StartsWith("SkillLevel:")?"{0} 스킬 레벨":meaning.StartsWith("SkillCost:")?"{0} 자원 소모 감소":"{0} 추가 피해",SkillName(skill));}
            return meaning=="AttackPower"?Loc.T("공격력"):Loc.T(StatCatalog.All[(int)Enum.Parse<StatId>(meaning)].name);
        }
        public static string Unit(string meaning)=>IsSkill(meaning)?(meaning.StartsWith("SkillLevel:")?"":"%"):meaning!="AttackPower"&&StatCatalog.All[(int)Enum.Parse<StatId>(meaning)].form==StatForm.Percent?"%":"";
        static float Value(string meaning,int grade)
        {
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
                if(g.isStart){meaning="AttackPower";numeric=0;}
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
