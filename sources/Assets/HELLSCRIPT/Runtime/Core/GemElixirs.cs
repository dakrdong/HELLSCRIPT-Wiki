using System;
using System.Collections.Generic;
using System.Linq;

namespace Hellscript
{
    // The approved jeweler table is shared by manufacturing, combat and every potion preview.
    public sealed class GemElixirFamily
    {
        public readonly string gemId,name,color;
        public readonly IReadOnlyList<float> first,second,third;
        public readonly IReadOnlyList<string> labels;
        internal GemElixirFamily(string gemId,string name,string color,string[] labels,float[] first,float[] second,float[] third)
        {this.gemId=gemId;this.name=name;this.color=color;this.labels=Array.AsReadOnly(labels);this.first=Array.AsReadOnly(first);this.second=Array.AsReadOnly(second);this.third=Array.AsReadOnly(third);}
        public float Value(int effect,int tier)=>(effect==0?first:effect==1?second:third)[tier-1];
        public string Description(int tier)=>string.Join("\n",Enumerable.Range(0,3).Select(n=>Loc.F(labels[n],Value(n,tier))));
    }
    public static class GemElixirs
    {
        public const float Duration=30;
        public const int PotionLimit=9999;
        public static readonly IReadOnlyList<GemElixirFamily> Families=Array.AsReadOnly(new[]{
            new GemElixirFamily("G03","토파즈","E6B754",new[]{"매직찬스 +{0:0.##}%p","이동 속도 +{0:0.##}%","획득 반경 +{0:0.##}%"},new float[]{5,10,15,22,30,40},new float[]{5,7,10,13,17,22},new float[]{10,15,25,35,50,70}),
            new GemElixirFamily("G05","자수정","B777DA",new[]{"힘 +{0:0.##}","최대 생명력 +{0:0.##}%","생명력 훔침 +{0:0.##}%p"},new float[]{10,20,35,55,80,120},new float[]{5,8,12,17,23,30},new float[]{.5f,1,1.5f,2,3,4}),
            new GemElixirFamily("G02","사파이어","639AFF",new[]{"마법력 +{0:0.##}","최대 마나 +{0:0.##}%","마나 훔침 +{0:0.##}%p"},new float[]{10,20,35,55,80,120},new float[]{5,8,12,17,23,30},new float[]{.5f,1,1.5f,2,3,4}),
            new GemElixirFamily("G04","에메랄드","60CD91",new[]{"민첩력 +{0:0.##}","회피 확률 +{0:0.##}%p","공격 속도 +{0:0.##}%"},new float[]{10,20,35,55,80,120},new float[]{2,3,5,7,10,14},new float[]{5,8,12,17,23,30}),
            new GemElixirFamily("G01","루비","E56D74",new[]{"방어력 +{0:0.##}%","HP 회복 +{0:0.##}%/초","완벽한 방어 확률 +{0:0.##}%p"},new float[]{10,16,24,34,46,60},new float[]{.5f,.8f,1.2f,1.8f,2.5f,3.5f},new float[]{2,3,5,7,10,14}),
            new GemElixirFamily("G06","다이아몬드","D5E8EB",new[]{"모든 저항력 +{0:0.##}","CC 지속시간 -{0:0.##}%","최대 HP의 {0:0.##}% 보호막"},new float[]{10,18,28,40,55,75},new float[]{10,15,22,30,40,50},new float[]{5,8,12,17,23,30})
        });
        public static GemElixirFamily Family(string gemId)=>Families.FirstOrDefault(f=>f.gemId==gemId);
        public static string Id(string gemId,int tier)=>"PE-"+gemId+"-"+tier;
        public static string GemName(string gemId,int tier)=>Loc.F("{0} {1}",Loc.T(GemCatalog.TierNames[tier-1]),Loc.T(Family(gemId)?.name??GemCatalog.Find(gemId).name));
        public static string Name(PotionDefinition p)=>p.Crafted?Loc.F("{0} 물약",GemName(p.gemId,p.grade)):Loc.T(p.name);
        public static string Description(PotionDefinition p)=>p.Crafted?Family(p.gemId).Description(p.grade):Loc.T(p.description);
        public static IEnumerable<PotionDefinition> Potions
        {
            get
            {
                foreach(var f in Families)for(int tier=1;tier<=6;tier++)
                    yield return new PotionDefinition{id=Id(f.gemId,tier),gemId=f.gemId,grade=tier,name="보석 물약",description="보석 물약",effect="elixir-"+f.gemId,icon="status-elements",duration=Duration,cooldown=Duration};
            }
        }
    }
}
