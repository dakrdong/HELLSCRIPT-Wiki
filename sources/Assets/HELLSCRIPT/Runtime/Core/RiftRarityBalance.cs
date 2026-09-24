using System;

namespace Hellscript
{
    // These definitions follow ItemCatalog's immutable, code-authored content data pattern.
    public sealed class RiftRarityProfile
    {
        public readonly double Common, Magic, Rare, Legendary, LegendaryCap;

        public RiftRarityProfile(double common,double magic,double rare,double legendary,double legendaryCap)
        {
            if(!Probability(common)||!Probability(magic)||!Probability(rare)||!Probability(legendary)||
               !Probability(legendaryCap)||legendaryCap>=1||legendaryCap<legendary)
                throw new ArgumentException("Rarity probabilities must be finite and in range, with base legendary <= cap < 1.");
            if(Math.Abs(common+magic+rare+legendary-1)>1e-12)
                throw new ArgumentException("Base rarity probabilities must sum to one.");
            Common=common;Magic=magic;Rare=rare;Legendary=legendary;LegendaryCap=legendaryCap;
        }

        static bool Probability(double value)=>!double.IsNaN(value)&&!double.IsInfinity(value)&&value>=0&&value<=1;
    }

    public sealed class RiftRarityParameters
    {
        public readonly double GrowthHalfSpan;
        public readonly RiftRarityProfile Normal, Elite, Boss;

        public RiftRarityParameters(double growthHalfSpan,RiftRarityProfile normal,RiftRarityProfile elite,RiftRarityProfile boss)
        {
            if(double.IsNaN(growthHalfSpan)||double.IsInfinity(growthHalfSpan)||growthHalfSpan<=0)
                throw new ArgumentOutOfRangeException(nameof(growthHalfSpan));
            if(normal==null||elite==null||boss==null)throw new ArgumentNullException("Rarity profiles cannot be null.");
            if(elite.Common!=0||boss.Common!=0||boss.Magic!=0)
                throw new ArgumentException("Elite and boss rewards cannot introduce forbidden lower rarities.");
            // A shared growth factor makes ordered bases and caps sufficient for ordering at every stage.
            if(normal.Legendary>elite.Legendary||elite.Legendary>boss.Legendary||
               normal.LegendaryCap>elite.LegendaryCap||elite.LegendaryCap>boss.LegendaryCap)
                throw new ArgumentException("Legendary bases and caps must be ordered Normal <= Elite <= Boss.");
            GrowthHalfSpan=growthHalfSpan;Normal=normal;Elite=elite;Boss=boss;
        }

        public RiftRarityProfile Profile(RiftRewardSource source)
        {
            switch(source)
            {
                case RiftRewardSource.Normal:return Normal;
                case RiftRewardSource.Elite:return Elite;
                case RiftRewardSource.Boss:return Boss;
                default:throw new ArgumentOutOfRangeException(nameof(source));
            }
        }
    }

    public static class RiftRarityBalance
    {
        // This is a reward-source gate, not an account unlock or an equip requirement.
        // Unidentified purchases retain their independent Legendary/Set pool at R25.
        public const int MinimumLegendaryStage=30;
        public static int AllowedRarity(int rarity,int stage)=>stage<MinimumLegendaryStage&&rarity==3?2:rarity;

        // Playtest draft: R=50 earns half the increase from the R=1 baseline to each route's cap.
        // Tune the common span and each route here; construction rejects invalid balance data.
        public static readonly RiftRarityParameters Current=new RiftRarityParameters(
            growthHalfSpan:49,
            normal:new RiftRarityProfile(.55,.25,.19,.01,.10),
            elite:new RiftRarityProfile(0,.60,.35,.05,.25),
            boss:new RiftRarityProfile(0,0,.85,.15,.45));
    }
}
