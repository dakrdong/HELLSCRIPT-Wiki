using System;
using System.Globalization;
using UnityEngine;

namespace Hellscript
{
    // Level 1 is the existing combat rule. Only its named magnitude grows; proc conditions stay fixed.
    public static class AspectGrowth
    {
        public static float Factor(int level)=>1+.05f*(Mathf.Clamp(level,1,5)-1);
        public static float Value(float basis,int level)=>basis*Factor(level);
        public static float Reduced(float basis,int level)=>basis*(2-Factor(level));
        public static bool DurationGrows(LegendaryEffect effect)=>effect==LegendaryEffect.Stun||effect==LegendaryEffect.Root||effect==LegendaryEffect.Freeze;
        public static int SnapshotLevel(DamageSnapshot snapshot,string id)
        {
            int index=Array.IndexOf(snapshot?.aspectIds??Array.Empty<string>(),id);
            return index>=0&&index<(snapshot?.aspectLevels?.Length??0)?Mathf.Clamp(snapshot.aspectLevels[index],1,5):1;
        }
        public static float SnapshotValue(DamageSnapshot snapshot,string id,float basis)=>Value(basis,SnapshotLevel(snapshot,id));
        public static string Number(float n)=>n.ToString("0.###",CultureInfo.InvariantCulture);
        static string Change(string text,string from,string to)
        {
            if(!text.Contains(from))throw new InvalidOperationException("Missing aspect description token: "+from);
            return text.Replace(from,to);
        }
        public static string Description(string id,int level)
        {
            var d=AspectStone.Definition(id);if(d==null)return "";
            string text=d.Description;if(level<=1)return text;
            float f=Factor(level);
            string N(float basis)=>Number(basis*f);
            string R(float basis)=>Number(basis*(2-f));
            string Replace(string template,object before,object after)=>Change(text,Loc.F(template,before),Loc.F(template,after));
            switch(id)
            {
                case "LW01":return Change(text,"1.5m",N(1.5f)+"m");
                case "LW02":return Change(text,"D90%","D"+N(90)+"%");
                case "LW03":return Change(text,"+50%","+"+N(50)+"%");
                case "LW04":return Change(text,Loc.T("30초"),Loc.F("{0}초",R(30)));
                case "LA01":return Change(Change(text,"+15","+"+N(15)),"+60","+"+N(60));
                case "LA02":return Replace("{0}도",20,R(20));
                case "LA03":return text+Loc.F(" 후퇴 덫은 0.5초마다 D{0}%의 독 피해를 줍니다.",N(30));
                case "LA04":return text+Loc.F(" 원래 추가 피해의 {0}%를 전염시킵니다.",N(50));
                case "LM01":return Replace("초당 {0}m",1,N(1));
                case "LM02":return Change(text,"D70%","D"+N(70)+"%");
                case "LM03":return Replace("자원 {0}",20,N(20));
                case "LM04":return Change(text,Loc.T("절반입니다"),Loc.F("{0}%입니다",N(50)));
                case "LC01":return Change(Change(text,"+2.5m","+"+N(2.5f)+"m"),"+15%","+"+N(15)+"%");
                case "LC02":return Change(text,Loc.T("5초"),Loc.F("{0}초",N(5)));
                case "LC03":return Change(text,"25%",N(25)+"%");
            }
            var p=LegendaryPowers.Find(id);if(p==null)throw new InvalidOperationException("Unplanned aspect: "+id);
            string a=Number(p.Amount),b=N(p.Amount),percent=Number(p.Amount*100),grown=N(p.Amount*100);
            switch(p.Effect)
            {
                case LegendaryEffect.Stun:case LegendaryEffect.Root:case LegendaryEffect.Freeze:
                    return Replace("{0}초 동안",Number(p.Duration),N(p.Duration));
                case LegendaryEffect.Resource:return Replace("자원을 {0} 회복",a,b);
                case LegendaryEffect.ReduceCooldown:return Replace("쿨타임을 {0}초",a,b);
                case LegendaryEffect.Damage:case LegendaryEffect.Empower:return Replace("가산으로 {0}%",percent,grown);
                case LegendaryEffect.Heal:return Replace("최대 HP의 {0}%를 회복합니다",percent,grown);
                case LegendaryEffect.Shield:return Replace("최대 HP의 {0}%에 해당하는 보호막",percent,grown);
                case LegendaryEffect.Guard:return Replace("피해가 {0}% 감소합니다",percent,grown);
                case LegendaryEffect.Move:return Replace("이동속도가 {0}% 증가합니다",percent,grown);
                case LegendaryEffect.CriticalChance:case LegendaryEffect.Cost:case LegendaryEffect.Economize:case LegendaryEffect.Haste:
                    string unit=Loc.Language=="en"?" percentage points":"%p";
                    return Change(text,percent+unit,grown+unit);
                case LegendaryEffect.Slow:return Replace("{0}% 감속",percent,grown);
                default:return Change(text,"D"+percent+"%","D"+grown+"%");
            }
        }
    }
}
