using UnityEngine;

namespace Hellscript
{
    public sealed partial class HeroStats
    {
        HeroSave potionSource;bool potionTraining;RuneGrowthState potionRunes;
        public float magicFind,manaSteal,perfectBlock;
        float potionMoveMultiplier=1;
        internal HeroStats WithActivePotion(PotionRuntimeState state,int level)
            =>state?.utilityRemaining>0&&!string.IsNullOrEmpty(state.utilityId)?WithPotion(PotionCatalog.Get(state.utilityId),level):this;
        void ElixirAttributes(PotionDefinition potion)
        {
            if(potion==null||!potion.Crafted)return;
            var f=GemElixirs.Family(potion.gemId);float value=f.first[potion.grade-1];
            if(potion.gemId=="G05")bonuses[(int)StatId.Strength]+=value;
            if(potion.gemId=="G02")bonuses[(int)StatId.Intelligence]+=value;
            if(potion.gemId=="G04")bonuses[(int)StatId.Dexterity]+=value;
        }
        void ElixirEffects(PotionDefinition potion)
        {
            if(potion==null||!potion.Crafted)return;
            var f=GemElixirs.Family(potion.gemId);int tier=potion.grade;
            float a=f.Value(0,tier),b=f.Value(1,tier),c=f.Value(2,tier);
            switch(potion.gemId)
            {
                case "G03":magicFind=a/100;potionMoveMultiplier=1+b/100;speed=SpeedWithBonus(0);pickup=Mathf.Min(6,pickup*(1+c/100));break;
                case "G05":hp*=1+b/100;lifeSteal+=c/100;break;
                case "G02":maxResource*=1+b/100;manaSteal=c/100;break;
                case "G04":dodge=StatCatalog.Cap(StatId.DodgeChance,dodge*100+b)/100;attackSpeed=Mathf.Min(1.5f,attackSpeed*(1+c/100));break;
                case "G01":armor*=1+a/100;lifeRegen+=hp*b/100;perfectBlock=c/100;break;
                case "G06":resistance+=a;ccReduction=StatCatalog.Cap(StatId.CrowdControlDuration,ccReduction*100+b)/100;break;
            }
        }
    }
}
