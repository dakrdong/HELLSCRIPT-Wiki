using System;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    [Serializable] public sealed class LiveOpsRarity
    {
        public double common,magic,rare,legendary,legendaryCap;
        public RiftRarityProfile Profile()=>new RiftRarityProfile(common,magic,rare,legendary,legendaryCap);
    }
    [Serializable] public sealed class LiveOpsRiftSettings
    {
        public float monsterHealth=1,monsterAttack=1,monsterSpeed=1,eliteHealth=1,eliteAttack=1,eliteSpeed=1,bossHealth=1,bossAttack=1,bossSpeed=1;
        public float mapScale=1,packSpread=1,normalDensity=1;
        public float timeLimitSeconds=300;
        public float normalEquipmentChance=.02f,eliteEquipmentChance=1,bossEquipmentChance=1;
        public int bossEquipmentCount=3;
        public float goldMultiplier=1,experienceMultiplier=1;
        public int clearGoldBase=800,clearGoldPerStage=50,clearMaterialsBase=5,clearMaterialsStageDivisor=5;
        public int firstClearGoldBase=1000,firstClearGoldPerStage=100,firstClearMaterialsBase=10,firstClearMaterialsStageDivisor=5;
        public float bossStoneMultiplier=1,normalGemChance=.015f,eliteGemChance=.08f,bossGemChance=1;
        public int bossGemCount=2;
        public float normalRuneChance=.02f,eliteRuneChance=.20f,bossRuneChance=1;
        public int bossRuneCount=4;
        public double rarityGrowthHalfSpan=49;
        public LiveOpsRarity normalRarity=new LiveOpsRarity{common=.55,magic=.25,rare=.19,legendary=.01,legendaryCap=.10};
        public LiveOpsRarity eliteRarity=new LiveOpsRarity{common=0,magic=.60,rare=.35,legendary=.05,legendaryCap=.25};
        public LiveOpsRarity bossRarity=new LiveOpsRarity{common=0,magic=0,rare=.85,legendary=.15,legendaryCap=.45};
        [NonSerialized] RiftRarityParameters rarity;
        public RiftRarityParameters Rarity=>rarity??=new RiftRarityParameters(rarityGrowthHalfSpan,normalRarity.Profile(),eliteRarity.Profile(),bossRarity.Profile());
        public float GemChance(RiftRewardSource source)=>source==RiftRewardSource.Boss?bossGemChance:source==RiftRewardSource.Elite?eliteGemChance:normalGemChance;
        public int GemCount(RiftRewardSource source)=>source==RiftRewardSource.Boss?bossGemCount:1;
        public int ClearGold(int stage)=>checked(clearGoldBase+clearGoldPerStage*stage);
        public int FirstGold(int stage)=>checked(firstClearGoldBase+firstClearGoldPerStage*stage);
        public int ClearMaterials(int stage)=>clearMaterialsBase+stage/clearMaterialsStageDivisor;
        public int FirstMaterials(int stage)=>firstClearMaterialsBase+stage/firstClearMaterialsStageDivisor;
    }
    [Serializable] public sealed class LiveOpsStageOverride
    {public int fromStage,toStage;public LiveOpsRiftSettings rift;}
    [Serializable] public sealed class LiveOpsFirstClear
    {public int stage;public RewardBoxGrant[] grants;}
    [Serializable] public sealed class LiveOpsConfiguration
    {
        public LiveOpsRiftSettings rift=new LiveOpsRiftSettings();
        public LiveOpsStageOverride[] stageOverrides=Array.Empty<LiveOpsStageOverride>();
        public LiveOpsFirstClear[] firstClearRewards=Array.Empty<LiveOpsFirstClear>();
    }
    [Serializable] public sealed class LiveOpsRelease
    {
        public int schemaVersion,version;
        public long publishedUtcMs;
        public string configHash,configJson;
        [NonSerialized] public LiveOpsConfiguration Configuration;
        [NonSerialized] public string source;
    }
    // Only the resolved stage settings travel with a run. The release hash identifies the
    // immutable complete server document without duplicating every stage in every save.
    [Serializable] public sealed class LiveOpsRunSnapshot
    {
        public int schemaVersion,version,stage;
        public string configHash,source;
        public LiveOpsRiftSettings rift;
        public RewardBoxGrant[] firstClearRewards;
    }
    public static class LiveOpsConfig
    {
        public const int SchemaVersion=1,MaximumResponseBytes=4*1024*1024;
        public static LiveOpsRiftSettings For(RunState run)=>run?.liveOps?.rift??new LiveOpsRiftSettings();
        public static LiveOpsRelease BuiltIn()
        {
            string json=JsonUtility.ToJson(new LiveOpsConfiguration());
            return new LiveOpsRelease{schemaVersion=SchemaVersion,version=0,source="builtin",configJson=json,configHash=CombatJournalArchive.Hash(json),Configuration=JsonUtility.FromJson<LiveOpsConfiguration>(json)};
        }
        public static LiveOpsRelease Parse(string json)
        {
            if(string.IsNullOrEmpty(json)||System.Text.Encoding.UTF8.GetByteCount(json)>MaximumResponseBytes)throw new ArgumentException("Live operations response exceeds its limit.");
            LiveOpsJson.Syntax(json);
            var release=JsonUtility.FromJson<LiveOpsRelease>(json);
            if(release==null||release.schemaVersion!=SchemaVersion||release.version<0||release.publishedUtcMs<0||string.IsNullOrEmpty(release.configJson)||!Hash(release.configHash)||CombatJournalArchive.Hash(release.configJson)!=release.configHash)
                throw new NotSupportedException("Invalid live operations release or hash.");
            LiveOpsJson.Shape<LiveOpsConfiguration>(release.configJson);
            release.Configuration=JsonUtility.FromJson<LiveOpsConfiguration>(release.configJson);Validate(release.Configuration);release.source="published";return release;
        }
        public static LiveOpsRunSnapshot Capture(LiveOpsRelease release,int stage)
        {
            if(stage<1||stage>1000)throw new ArgumentOutOfRangeException(nameof(stage));
            release??=BuiltIn();
            var config=release.Configuration??JsonUtility.FromJson<LiveOpsConfiguration>(release.configJson);Validate(config);
            var settings=config.stageOverrides.FirstOrDefault(x=>stage>=x.fromStage&&stage<=x.toStage)?.rift??config.rift;
            var grants=config.firstClearRewards.FirstOrDefault(x=>x.stage==stage)?.grants??RewardBoxCatalog.FirstClear(stage);
            return new LiveOpsRunSnapshot{schemaVersion=SchemaVersion,version=release.version,stage=stage,configHash=release.configHash,source=release.source,
                rift=CombatJournal.Copy(settings),firstClearRewards=grants.Select(CombatJournal.Copy).ToArray()};
        }
        public static void NormalizeRun(RunState run)
        {
            if(run.training>=0){run.liveOps=null;return;}
            // A legacy suspended run keeps the shipped rules, even if a newer release is cached.
            if(run.liveOps==null||run.liveOps.schemaVersion==0)run.liveOps=Capture(null,run.stage);
            Validate(run.liveOps);
        }
        public static void Validate(LiveOpsRunSnapshot snapshot)
        {
            if(snapshot==null||snapshot.schemaVersion!=SchemaVersion||snapshot.version<0||snapshot.stage<1||snapshot.stage>1000||!Hash(snapshot.configHash)||snapshot.source!="builtin"&&snapshot.source!="published")
                throw new NotSupportedException("Unsupported saved live operations snapshot.");
            Validate(snapshot.rift);ValidateGrants(snapshot.firstClearRewards,snapshot.stage);
        }
        static bool Hash(string value)=>value!=null&&value.Length==64&&value.All(c=>c>='0'&&c<='9'||c>='a'&&c<='f');
        static void Range(double value,double min,double max,string name)
        {if(double.IsNaN(value)||double.IsInfinity(value)||value<min||value>max)throw new ArgumentOutOfRangeException(name);}
        public static void Validate(LiveOpsConfiguration config)
        {
            if(config==null||config.stageOverrides==null||config.firstClearRewards==null||config.stageOverrides.Length>100||config.firstClearRewards.Length>1000)
                throw new ArgumentException("Invalid live operations configuration.");
            Validate(config.rift);int previous=0;
            foreach(var row in config.stageOverrides.OrderBy(x=>x?.fromStage??0))
            {
                if(row==null||row.fromStage<=previous||row.fromStage<1||row.toStage<row.fromStage||row.toStage>1000)throw new ArgumentException("Overlapping or invalid live operations stages.");
                Validate(row.rift);previous=row.toStage;
            }
            if(config.firstClearRewards.Any(r=>r==null||r.stage<1||r.stage>1000)||config.firstClearRewards.Select(r=>r.stage).Distinct().Count()!=config.firstClearRewards.Length)
                throw new ArgumentException("Invalid first-clear stages.");
            foreach(var row in config.firstClearRewards)ValidateGrants(row.grants,row.stage);
        }
        public static void ValidateGrants(RewardBoxGrant[] grants,int stage)
        {
            if(grants==null||grants.Length>20)throw new ArgumentException("Invalid first-clear rewards.");
            foreach(var grant in grants)
            {
                var box=grant==null?null:RewardBoxCatalog.Find(grant.boxId);
                if(box==null||grant.count<1||grant.count>100||grant.minimumQuality<0||grant.minimumQuality>10000||box.kind=="equipment"&&box.rarity!=RiftRarityBalance.AllowedRarity(box.rarity,stage))
                    throw new ArgumentException("Invalid first-clear reward or rarity gate.");
            }
        }
        public static void Validate(LiveOpsRiftSettings r)
        {
            if(r==null||r.normalRarity==null||r.eliteRarity==null||r.bossRarity==null)throw new ArgumentException("Missing rift settings.");
            foreach(float n in new[]{r.monsterHealth,r.monsterAttack,r.eliteHealth,r.eliteAttack,r.bossHealth,r.bossAttack})Range(n,.1,5,"difficulty");
            foreach(float n in new[]{r.monsterSpeed,r.eliteSpeed,r.bossSpeed})Range(n,.5,2,"speed");
            Range(r.mapScale,.95f,1.05f,"mapScale");Range(r.packSpread,.8,1.5,"packSpread");Range(r.normalDensity,.75,1.25,"normalDensity");Range(r.timeLimitSeconds,60,1800,"timeLimitSeconds");
            foreach(float n in new[]{r.normalEquipmentChance,r.eliteEquipmentChance,r.bossEquipmentChance,r.normalGemChance,r.eliteGemChance,r.bossGemChance,r.normalRuneChance,r.eliteRuneChance,r.bossRuneChance})Range(n,0,1,"chance");
            foreach(float n in new[]{r.goldMultiplier,r.experienceMultiplier,r.bossStoneMultiplier})Range(n,0,10,"multiplier");
            foreach(int n in new[]{r.bossEquipmentCount,r.bossGemCount,r.bossRuneCount})Range(n,0,20,"count");
            foreach(int n in new[]{r.clearGoldBase,r.clearMaterialsBase,r.firstClearGoldBase,r.firstClearMaterialsBase})Range(n,0,1000000,"rewardBase");
            foreach(int n in new[]{r.clearGoldPerStage,r.firstClearGoldPerStage})Range(n,0,10000,"rewardPerStage");
            foreach(int n in new[]{r.clearMaterialsStageDivisor,r.firstClearMaterialsStageDivisor})Range(n,1,1000,"rewardDivisor");
            Range(r.rarityGrowthHalfSpan,1,1000,"rarityGrowthHalfSpan");
            foreach(var rarity in new[]{r.normalRarity,r.eliteRarity,r.bossRarity})Range(rarity.legendaryCap,0,.9999,"legendaryCap");
            _=new RiftRarityParameters(r.rarityGrowthHalfSpan,r.normalRarity.Profile(),r.eliteRarity.Profile(),r.bossRarity.Profile());
        }
        // Avoid integer overflow in all multiplier-driven reward sources.
        public static int Scale(int amount,float multiplier)=>checked((int)Math.Floor((double)amount*multiplier));
    }
}
