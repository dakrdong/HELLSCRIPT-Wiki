using System;
using System.Collections.Generic;
using System.Linq;
using Hellscript.Runes;
using UnityEngine;

namespace Hellscript
{
    [Serializable] public sealed class RewardBoxDefinition
    {
        public string id,nameKo,nameEn,kind,gemId,formula,icon;
        public int amount=1,slot=-1,rarity,tier,grade,size;
        public bool awakened,setOnly,eachType;
        public string Name=>Loc.Language=="en"?nameEn:nameKo;
    }
    [Serializable] public sealed class RewardBoxGrant
    {public string boxId;public int count=1,minimumQuality;}
    [Serializable] public sealed class RewardBoxMilestone
    {public int stage;public RewardBoxGrant[] grants;}
    [Serializable] public sealed class RewardBoxRule
    {public int every;public RewardBoxGrant grant;}
    [Serializable] public sealed class RewardBoxItemLevel
    {public int stage,level;}
    [Serializable] public sealed class RewardBoxDatabase
    {
        public int version;public RewardBoxDefinition[] boxes;public RewardBoxMilestone[] milestones;
        public RewardBoxRule[] firstClearRules;public RewardBoxItemLevel[] itemLevels;
    }
    [Serializable] public sealed class OwnedRewardBox
    {
        public string id,boxId;public int count,sourceStage,itemLevel,minimumQuality,opened;
        public uint seed;
    }
    [Serializable] public sealed class RewardBoxState
    {
        public int version;
        public List<OwnedRewardBox> owned=new List<OwnedRewardBox>();
        public List<int> claimedStages=new List<int>();
        public List<RewardBoxPromise> firstClearPromises=new List<RewardBoxPromise>();
        // Only exact normal-clear records are eligible, including existing RiftBestTime entries.
        // Historical boss-defeat flags/highestClear do not prove a completed run.
    }
    [Serializable] public sealed class RewardBoxPromise
    {public int stage,itemLevel,liveOpsVersion;public string configHash;public RewardBoxGrant[] grants;}
    public static class RewardBoxCatalog
    {
        static RewardBoxDatabase database;
        public static RewardBoxDatabase Data
        {
            get
            {
                if(database!=null)return database;
                var source=Resources.Load<TextAsset>("Data/RewardBoxes");
                if(source==null)throw new InvalidOperationException("Reward box catalog is missing.");
                var next=JsonUtility.FromJson<RewardBoxDatabase>(source.text);Validate(next);database=next;return database;
            }
        }
        public static IReadOnlyList<RewardBoxDefinition> All=>Data.boxes;
        public static RewardBoxDefinition Find(string id)=>Data.boxes.FirstOrDefault(b=>b.id==id);
        public static void Validate(RewardBoxDatabase d)
        {
            if(d==null||d.version!=1||d.boxes==null||d.milestones==null||d.firstClearRules==null||d.itemLevels==null)
                throw new NotSupportedException("Unsupported reward box catalog.");
            var kinds=new[]{"equipment","gem","stones","materials","gold","premium","cores","rune"};
            if(d.boxes.Any(b=>b==null||string.IsNullOrWhiteSpace(b.id)||string.IsNullOrWhiteSpace(b.nameKo)||string.IsNullOrWhiteSpace(b.nameEn)||b.icon!=b.id||!kinds.Contains(b.kind)||b.amount<1||b.slot< -1||b.slot>7)||d.boxes.Select(b=>b.id).Distinct().Count()!=d.boxes.Length)
                throw new InvalidOperationException("Invalid reward box definitions.");
            foreach(var b in d.boxes)
            {
                if(b.kind=="equipment"&&(b.rarity<2||b.rarity>3||b.amount!=1||b.setOnly&&b.rarity!=3))throw new InvalidOperationException("Invalid equipment box.");
                if(b.kind=="gem"&&(b.amount!=10||b.tier<1||b.tier>6||!string.IsNullOrEmpty(b.gemId)&&GemCatalog.Find(b.gemId)==null))throw new InvalidOperationException("Invalid gem box.");
                if(b.kind=="rune"&&(b.grade<0||b.grade>6||b.size<1||b.size>5||b.eachType&&b.amount!=5))throw new InvalidOperationException("Invalid rune box.");
                if(!string.IsNullOrEmpty(b.formula)&&!(b.formula=="rift-stones"&&b.kind=="stones"||b.formula=="ten-materials"&&b.kind=="materials"||b.formula=="fifty-stones"&&b.kind=="stones"))throw new InvalidOperationException("Unknown box amount formula.");
            }
            Action<RewardBoxGrant> check=g=>{if(g==null||g.count<1||g.count>100||g.minimumQuality<0||g.minimumQuality>10000||!d.boxes.Any(b=>b.id==g.boxId))throw new InvalidOperationException("Invalid first-clear box grant.");};
            Action<RewardBoxGrant,int> checkStage=(g,stage)=>
            {
                check(g);var box=d.boxes.Single(b=>b.id==g.boxId);
                if(box.kind=="equipment"&&box.rarity!=RiftRarityBalance.AllowedRarity(box.rarity,stage))
                    throw new InvalidOperationException("First-clear equipment precedes its rift rarity gate.");
            };
            foreach(var r in d.firstClearRules){if(r.every<1||r.every>1000)throw new InvalidOperationException("Invalid first-clear interval.");checkStage(r.grant,r.every);}
            foreach(var m in d.milestones){if(m.stage<1||m.stage>1000||m.grants==null)throw new InvalidOperationException("Invalid first-clear milestone.");foreach(var g in m.grants)checkStage(g,m.stage);}
            if(d.milestones.Select(m=>m.stage).Distinct().Count()!=d.milestones.Length||d.itemLevels.Length<2||d.itemLevels[0].stage!=1||d.itemLevels.Last().stage!=1000||d.itemLevels.Any(p=>p.level<1||p.level>60)||!d.itemLevels.Select(p=>p.stage).SequenceEqual(d.itemLevels.Select(p=>p.stage).Distinct().OrderBy(s=>s)))throw new InvalidOperationException("Invalid reward box progression.");
        }
        public static int ItemLevel(int stage)
        {
            if(stage<1||stage>1000)throw new ArgumentOutOfRangeException(nameof(stage));
            var points=Data.itemLevels;for(int i=1;i<points.Length;i++)if(stage<=points[i].stage)
            {var a=points[i-1];var b=points[i];return Mathf.RoundToInt(Mathf.Lerp(a.level,b.level,(stage-a.stage)/(float)(b.stage-a.stage)));}
            return points.Last().level;
        }
        public static int Amount(RewardBoxDefinition d,int stage)
        {
            switch(d.formula)
            {
                case "rift-stones":return 2+stage/50;
                case "ten-materials":return 20+stage/20;
                case "fifty-stones":return 50+stage/2;
                default:return d.amount;
            }
        }
        public static RewardBoxGrant[] FirstClear(int stage)
        {
            if(stage<1||stage>1000)return Array.Empty<RewardBoxGrant>();
            return Data.firstClearRules.Where(r=>stage%r.every==0).Select(r=>r.grant)
                .Concat(Data.milestones.FirstOrDefault(m=>m.stage==stage)?.grants??Array.Empty<RewardBoxGrant>()).Select(RuneGrowth.Copy).ToArray();
        }
    }
    public static class RewardBoxes
    {
        public const int Version=1,MaximumOpen=10;
        public static void Normalize(AccountSave a)
        {
            if(a.rewardBoxes==null||a.rewardBoxes.version==0)
            {
                if(a.schema>=12)throw new NotSupportedException("Missing reward box state in a current save.");
                if(a.rewardBoxes!=null&&((a.rewardBoxes.owned?.Count??0)>0||(a.rewardBoxes.claimedStages?.Count??0)>0))throw new NotSupportedException("Unversioned reward box state.");
                a.rewardBoxes=new RewardBoxState{version=Version};
            }
            a.rewardBoxes.firstClearPromises??=new List<RewardBoxPromise>();
            // Existing exact clears keep their shipped package even when first opened after deployment.
            if(a.schema<16)foreach(int stage in a.heroes.SelectMany(h=>h.riftProgress?.best??new List<RiftBestTime>()).Where(b=>b.milliseconds>0).Select(b=>b.stage).Distinct())
                CapturePromise(a,stage,null);
            Validate(a);
        }
        public static void Validate(AccountSave a)
        {
            var r=a.rewardBoxes;
            if(r==null||r.version!=Version||r.owned==null||r.claimedStages==null||a.premium<0||a.enhancementStones<0)throw new NotSupportedException("Unsupported reward box state; original save preserved.");
            if(r.claimedStages.Any(s=>s<1||s>1000)||r.claimedStages.Distinct().Count()!=r.claimedStages.Count)throw new NotSupportedException("Invalid first-clear claims.");
            if(r.firstClearPromises==null||r.firstClearPromises.Count>1000||r.firstClearPromises.Any(p=>p==null||p.stage<1||p.stage>1000||p.itemLevel<1||p.itemLevel>60||p.liveOpsVersion<0)||r.firstClearPromises.Select(p=>p.stage).Distinct().Count()!=r.firstClearPromises.Count)
                throw new NotSupportedException("Invalid first-clear promises.");
            foreach(var promise in r.firstClearPromises)LiveOpsConfig.ValidateGrants(promise.grants,promise.stage);
            var ids=new HashSet<string>();
            foreach(var box in r.owned)
            {
                if(box==null||string.IsNullOrWhiteSpace(box.id)||!ids.Add(box.id)||RewardBoxCatalog.Find(box.boxId)==null||box.count<1||box.count>10000||box.opened<0||box.sourceStage<1||box.sourceStage>1000||box.itemLevel<1||box.itemLevel>60||box.minimumQuality<0||box.minimumQuality>10000||box.seed==0)
                    throw new NotSupportedException("Invalid owned reward box; original save preserved.");
            }
        }
        public static bool Eligible(AccountSave a,int stage)=>stage>=1&&stage<=1000&&a.heroes.Any(h=>(h.riftProgress?.Best(stage)??0)>0);
        public static string Status(AccountSave a,int stage)=>a.rewardBoxes.claimedStages.Contains(stage)?"claimed":Eligible(a,stage)?"ready":"locked";
        public static RewardBoxGrant[] Preview(AccountSave a,int stage,LiveOpsRunSnapshot next=null)
        {
            var promise=a.rewardBoxes.firstClearPromises?.FirstOrDefault(p=>p.stage==stage);
            return (promise?.grants??(a.rewardBoxes.claimedStages.Contains(stage)?RewardBoxCatalog.FirstClear(stage):next?.firstClearRewards)??RewardBoxCatalog.FirstClear(stage)).Select(CombatJournal.Copy).ToArray();
        }
        public static void CaptureClear(AccountSave a,RunState run)
        {
            if(run.training>=0||run.phase!=RunPhase.Cleared||!Eligible(a,run.stage))return;
            CapturePromise(a,run.stage,run.liveOps);
        }
        static void CapturePromise(AccountSave a,int stage,LiveOpsRunSnapshot snapshot)
        {
            if(a.rewardBoxes.claimedStages.Contains(stage)||a.rewardBoxes.firstClearPromises.Any(p=>p.stage==stage))return;
            var grants=snapshot?.firstClearRewards??RewardBoxCatalog.FirstClear(stage);LiveOpsConfig.ValidateGrants(grants,stage);
            a.rewardBoxes.firstClearPromises.Add(new RewardBoxPromise{stage=stage,itemLevel=RewardBoxCatalog.ItemLevel(stage),liveOpsVersion=snapshot?.version??0,
                configHash=snapshot?.configHash??"",grants=grants.Select(CombatJournal.Copy).ToArray()});
        }
        // The caller must be a staged GameStore transaction; this is not a public inventory write path.
        internal static bool Claim(AccountSave a,int stage)
        {
            if(a.suspendedRun!=null||!Eligible(a,stage)||a.rewardBoxes.claimedStages.Contains(stage))return false;
            // Legacy clear fixtures and imported pre-live-ops records have no remote promise.
            CapturePromise(a,stage,null);
            var promise=a.rewardBoxes.firstClearPromises.Single(p=>p.stage==stage);
            int n=0;
            foreach(var grant in promise.grants)
            {
                string id="rift-first-"+stage+"-"+n++;
                a.rewardBoxes.owned.Add(new OwnedRewardBox{id=id,boxId=grant.boxId,count=grant.count,sourceStage=stage,
                    itemLevel=promise.itemLevel,minimumQuality=grant.minimumQuality,seed=RuneEconomy.Seed(Guid.NewGuid().ToString("N"))|1u});
            }
            a.rewardBoxes.claimedStages.Add(stage);return true;
        }
        static string EquipmentUnique(RewardBoxDefinition d,HeroClass hero,int slot,ref uint random)
        {
            if(!d.setOnly)return null;
            var pool=ItemCatalog.Uniques.Where(u=>u.Fits(hero,slot)&&!string.IsNullOrEmpty(u.setId)).ToArray();
            int total=pool.Sum(u=>u.weight);if(total<1)throw new InvalidOperationException("No released class set in this slot.");
            int pick=RandomStream.Range(ref random,0,total);foreach(var u in pool){pick-=u.weight;if(pick<0)return u.id;}
            throw new InvalidOperationException("Invalid class set weights.");
        }
        internal static bool Open(AccountSave a,string id,int count,string choice,List<Item> results)
        {
            if(a.suspendedRun!=null||count<1||count>MaximumOpen)return false;
            var box=a.rewardBoxes.owned.SingleOrDefault(b=>b.id==id);if(box==null||box.count<count)return false;
            var d=RewardBoxCatalog.Find(box.boxId);int amount=checked(RewardBoxCatalog.Amount(d,box.sourceStage)*count);
            bool choosingGem=d.kind=="gem"&&string.IsNullOrEmpty(d.gemId),choosingCore=d.kind=="cores"&&d.slot<0;
            if(!choosingGem&&!choosingCore&&!string.IsNullOrEmpty(choice))return false;
            string gem=choosingGem?choice:d.gemId;
            int core=d.slot;
            if(choosingGem&&!GemCatalog.Valid(gem,d.tier)||choosingCore&&(!int.TryParse(choice,out core)||core<0||core>7))return false;
            if(d.kind=="equipment"&&Economy.FreeSlots(a.Hero)<count)return false;
            uint random=box.seed;
            switch(d.kind)
            {
                case "equipment":
                    for(int i=0;i<count;i++)
                    {
                        int slot=d.slot<0?RandomStream.Range(ref random,0,8):d.slot;
                        string unique=EquipmentUnique(d,a.Hero.heroClass,slot,ref random);
                        // The catalog gates new first-clear grants. Previously owned guarantee boxes
                        // keep their promise and deterministic seed; never downgrade an owned reward.
                        var item=ItemGenerator.Create(a.Hero.heroClass,slot,d.rarity,box.itemLevel,ref random,id+"-item-"+(box.opened+i),unique);
                        if(d.slot==0&&EquipmentSlots.Offhand(item))
                        {
                            var bases=ItemCatalog.Bases.Where(b=>b.Fits(a.Hero.heroClass,0)&&!EquipmentSlots.IsOffhand(EquipmentSlots.Kind(b.id))).ToArray();
                            var basis=bases[RandomStream.Range(ref random,0,bases.Length)];item.baseId=basis.id;item.baseIndex=basis.legacyIndex;
                        }
                        item.awakened=d.awakened;int minimum=Math.Max(box.minimumQuality,d.awakened?4000:0);
                        if(minimum>0)foreach(var roll in item.rolls)
                        {roll.rollBasisPoints=ItemGenerator.UniformQuality(ref random,minimum);roll.tierId=ItemGenerator.Tier(roll.rollBasisPoints);roll.value=ItemCatalog.Affix(roll.affixId).Value(item.level,roll.rollBasisPoints);}
                        item.name=item.DisplayName;item.acquisitionKind="reward-box";ItemCatalog.Validate(item);
                        if(!Economy.AddItem(a.Hero,item,BagPolicy.Ignore,a))return false;
                        results.Add(RuneGrowth.Copy(item));
                    }
                    break;
                case "gem":
                    if(!GemStacks.TryExchange(a.gems,a.gemCapacity,Array.Empty<GemStack>(),new[]{new GemStack{gemId=gem,tier=d.tier,count=amount}}))return false;
                    ContentUnlocks.RecordGemAcquisition(a);break;
                case "gold":a.gold=checked(a.gold+amount);break;
                case "materials":a.materials=checked(a.materials+amount);break;
                case "stones":a.enhancementStones=checked(a.enhancementStones+amount);break;
                case "premium":a.premium=checked(a.premium+amount);break;
                case "cores":a.cores[core]=checked(a.cores[core]+amount);break;
                case "rune":
                    var shapes=RuneMasteryCatalog.Shapes.Where(s=>s.Size==d.size).ToArray();
                    for(int i=0;i<amount;i++)a.runes.owned.Add(new OwnedRune{id=id+"-rune-"+(box.opened*d.amount+i),grade=d.grade,
                        shapeId=shapes[RandomStream.Range(ref random,0,shapes.Length)].Id,type=d.eachType?i%5:RandomStream.Range(ref random,0,5)});
                    a.runes.revision=checked(a.runes.revision+1);break;
                default:return false;
            }
            box.count-=count;box.opened=checked(box.opened+count);box.seed=random;
            if(box.count==0)a.rewardBoxes.owned.Remove(box);
            return true;
        }
    }
}
