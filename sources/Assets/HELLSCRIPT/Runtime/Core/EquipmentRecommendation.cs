using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Hellscript
{
    public static class EquipmentRecommendation
    {
        public const string Prefix="autoEquip.";
        public static readonly int[] ArmorPositions={1,2,3,4,5,6,7,9};
        public static string ModeId(int position)=>Prefix+position+".mode";
        public static string StatsId(int position)=>Prefix+position+".stats";
        public static string Type(int position)=>ShopAutoSelect.Types[EquipmentSlots.Slot(position)+5];
        public static string PositionName(int position)=>EquipmentSlots.Label(EquipmentSlots.Slot(position),EquipmentSlots.Index(position));
        public static EdictOptionDefinition[] Options()
        {
            var result=new List<EdictOptionDefinition>{
                new EdictOptionDefinition(Prefix+"enabled","autoEquip","장비 추천 착용 사용",EdictOptionKind.Toggle,"OFF"),
                new EdictOptionDefinition(EquipmentAutomation.Displaced,"autoEquip","교체한 장비 처리",EdictOptionKind.Choice,"WAREHOUSE",new[]{"WAREHOUSE","SALVAGE","SELL"}),
                new EdictOptionDefinition(Prefix+"preserveEffects","autoEquip","전설·세트 효과 유지",EdictOptionKind.Toggle,"ON"),
                new EdictOptionDefinition(Prefix+"weapon","autoEquip","무기 교체 기준",EdictOptionKind.Choice,"SAME",new[]{"SAME","SCORE","OFF"})};
            foreach(int position in ArmorPositions)
            {
                result.Add(new EdictOptionDefinition(ModeId(position),"autoEquip",PositionName(position),EdictOptionKind.Choice,"SCORE",new[]{"SCORE","STATS","BOTH","OFF"}));
                result.Add(new EdictOptionDefinition(StatsId(position),"autoEquip","비교할 옵션",EdictOptionKind.Set,"",ShopAutoSelect.StatKeys(Type(position)).ToArray()));
            }
            return result.ToArray();
        }
        public static string Get(HuntEdictV2Document doc,string id)=>doc?.global?.FirstOrDefault(o=>o.id==id)?.value??"";
        public static bool Enabled(HeroSave hero)=>hero.useEdict&&Get(HuntEdictDefaults.Resolve(hero),Prefix+"enabled")=="ON";
        public static HuntEdictV2Document Upgrade(HuntEdictV2Document source)
        {
            source=PotionEdict.Upgrade(source);
            if(source==null||source.globalVersion<2||source.globalVersion>3)return source;
            var copy=source.Copy();
            if(copy.globalVersion==2)
            {
                copy.global=EdictOptions.Canonical(EdictOptions.PotionGlobal,copy.global,copy.heroClass)
                    .Concat(EdictOptions.Defaults(Options().Where(d=>!EquipmentAutomation.NewOption(d.id)))).ToList();
                copy.globalVersion=3;
            }
            copy.global=EdictOptions.Canonical(EdictOptions.RecommendationGlobal,copy.global,copy.heroClass)
                .Concat(EdictOptions.Defaults(EdictOptions.Global.Where(d=>EquipmentAutomation.NewOption(d.id)))).ToList();
            copy.globalVersion=4;return copy;
        }
        public static string ModeName(string mode,bool weapon=false)
        {
            switch(mode)
            {
                case "SAME":return Loc.T("같은 무기 계열 · 높은 점수");
                case "SCORE":return Loc.T(weapon?"모든 무기 계열 · 높은 점수":"점수가 높으면 착용");
                case "STATS":return Loc.T("선택한 옵션이 더 높으면 착용");
                case "BOTH":return Loc.T("점수 상승 + 선택한 옵션 유지");
                default:return Loc.T("변경하지 않음");
            }
        }
        public static string StatName(int position,string key)=>key=="main"?Loc.F("기본 수치 · {0}",Loc.T(BlacksmithCatalog.StatName(new[]{"attack","armor","armor","attackSpeed","moveSpeed","life","allResistance","crit"}[EquipmentSlots.Slot(position)]))):StatCatalog.Get((StatId)ItemCatalog.Affix(key).stat).Name;
        public static EquipmentPlan Plan(HeroSave hero,Item item)
        {
            if(!Enabled(hero)||item==null||item.equipped||item.locked||!hero.inventory.Contains(item))return null;
            var policy=HuntEdictDefaults.Resolve(hero);EquipmentPlan best=null;double gain=double.NegativeInfinity;
            foreach(int index in EquipmentSlots.Targets(hero,item))
            {
                var plan=EquipmentSlots.Plan(hero,item,index);if(!plan.Valid)continue;
                var old=hero.inventory.Where(i=>plan.outgoing.Contains(i.id)).ToArray();
                if(old.Any(i=>i.locked))continue;
                // A defensive/offensive offhand must not automatically remove the only attacking weapon.
                if(EquipmentSlots.Offhand(item)&&old.Any(i=>!EquipmentSlots.Offhand(i)))continue;
                int position=item.slot==7&&index==1?9:item.slot;
                string mode=Get(policy,item.slot==0?Prefix+"weapon":ModeId(position));
                if(mode=="OFF"||mode=="")continue;
                if(item.slot==0&&mode=="SAME")
                {
                    var current=EquipmentSlots.At(hero,0,index)??hero.inventory.FirstOrDefault(i=>i.equipped&&i.slot==0);
                    if(current!=null&&ShopAutoSelect.Type(current)!=ShopAutoSelect.Type(item))continue;
                    // Filling a free hand may not evict an unrelated weapon family.
                    if(old.Any(i=>ShopAutoSelect.Type(i)!=ShopAutoSelect.Type(item)))continue;
                }
                var score=EquipmentScore.Replacement(hero,item,plan);
                if(mode!="STATS"&&score.after<=score.before)continue;
                if(item.slot!=0&&(mode=="STATS"||mode=="BOTH"))
                {
                    string[] keys=Get(policy,StatsId(position)).Split(',').Where(s=>s!="").ToArray();
                    var current=EquipmentSlots.At(hero,item.slot,index);
                    if(keys.Length==0||keys.Any(key=>!Meets(item,current,key,mode=="STATS")))continue;
                }
                if(Get(policy,Prefix+"preserveEffects")=="ON"&&old.Length>0)
                {
                    var comparison=ItemComparison.Preview(hero,item,index);
                    if(comparison.lostEffects.Count>0)continue;
                }
                double difference=score.after-score.before;
                if(best==null||difference>gain){best=plan;gain=difference;}
            }
            return best;
        }
        static bool Meets(Item candidate,Item current,string key,bool strictlyHigher)
        {
            if(!ShopAutoSelect.Value(candidate,key,out float after))return false;
            float before=0;if(current!=null)ShopAutoSelect.Value(current,key,out before);
            // Compare the same two-decimal values shown in item option rows.
            decimal a=decimal.Parse(after.ToString("0.##",CultureInfo.InvariantCulture),CultureInfo.InvariantCulture),b=decimal.Parse(before.ToString("0.##",CultureInfo.InvariantCulture),CultureInfo.InvariantCulture);
            return strictlyHigher?a>b:a>=b;
        }
        public static bool Apply(HeroSave hero,Item item,AccountSave account=null)
        {
            var plan=Plan(hero,item);if(plan==null)return false;
            var outgoing=hero.inventory.Where(i=>plan.outgoing.Contains(i.id)).ToArray();
            if(!EquipmentSlots.Equip(hero,item,plan.index))return false;
            if(account!=null&&account.Hero==hero)EquipmentAutomation.Replaced(account,outgoing);
            return true;
        }
    }
}
