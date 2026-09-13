using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Hellscript.Editor
{
    public static partial class BuildIntegrationValidation
    {
        [Serializable] public sealed class QualityCurveRow
        {
            public int stage,heroClass,variant,seedIndex,masterwork,ticks,kills,chests,greaterAffixes,resourceChecks;
            public uint seed;
            public bool quality;
            public string outcome,map,encounter,equipmentHash;
            public float seconds,hp,maximumHp,attack,armor,resistance;
            public double dealt,received;
        }
        [Serializable] public sealed class QualityCurveSummary
        {
            public int stage,heroClass,variant,runs,clears,defeats,incomplete;
            public bool quality;
            public float clearRate,meanClearSeconds;
        }
        [Serializable] sealed class QualityCurveReport
        {
            public int version=1,seeds,completedRuns,expectedRuns;
            public string status="running",createdUtc=DateTime.UtcNow.ToString("O");
            public string scope="Paired fixed-seed generated rifts for six preset builds at stages 30/45/60/75/90. Level 30 heroes; eight legal item-level min(stage,60) pieces; enhancement +5. Quality loadouts use actual awakened item rolls, their naturally rolled greater affixes, and service-generated masterwork at the account cap. Baselines retain identical bases, unique identities and raw affix rolls, with quality layers removed. No gems, rune boards, combat overrides or forced outcomes. Account progress and crafting currency are seeded fixtures; this is not a natural progression, farming-cost or balance guarantee. Loot is ignored only when the bag is full.";
            public List<QualityCurveSummary> summaries=new List<QualityCurveSummary>();
        }
        [Serializable] sealed class QualityLoadout {public List<Item> equipment;}
        static string QualityEquipmentJson(AccountSave account)=>JsonUtility.ToJson(new QualityLoadout{equipment=account.Hero.inventory});
        static string QualityHash(string source)
        {using(var sha=SHA256.Create())return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(source))).Replace("-","").ToLowerInvariant();}

        public static AccountSave QualityCurveAccount(HeroClass heroClass,int variant,int stage,uint seed,bool quality)
        {
            if((int)heroClass<0||(int)heroClass>2||variant<0||variant>1||stage<20)throw new ArgumentOutOfRangeException();
            var account=GameStore.NewAccount();account.selectedHero=(int)heroClass;
            var hero=account.Hero;hero.level=30;hero.highestClear=stage;hero.capacity=100;hero.inventory.Clear();
            hero.build=GameCatalog.Preset(heroClass,variant);hero.build.autoRepeat=false;hero.build.bagPolicy=BagPolicy.Ignore;hero.useEdict=false;
            account.gold=100000000;account.materials=10000000;ContentUnlocks.Reconcile(account);
            for(int slot=0;slot<8;slot++)
            {
                uint random=seed^unchecked((uint)(slot+1)*0x9E3779B9u);string unique=Sets[(int)heroClass*2+variant][slot];Item item=null;
                for(int attempt=0;attempt<10000;attempt++)
                {
                    item=ItemGenerator.Create(heroClass,slot,unique==null?2:3,stage,ref random,
                        id:$"quality-{(int)heroClass}-{variant}-{stage}-{seed}-{slot}",uniqueId:unique,riftStage:stage);
                    if(item.awakened)break;
                }
                if(item==null||!item.awakened)throw new InvalidOperationException("Cannot obtain the seeded awakened fixture.");
                hero.inventory.Add(item);if(!Economy.Equip(hero,item))throw new InvalidOperationException("Illegal quality fixture equipment.");
                for(int level=0;level<5;level++)if(!Economy.Enhance(account,item))throw new InvalidOperationException("Fixture enhancement failed.");
                for(int level=0;level<ItemQuality.MasterworkCap(account);level++)
                    if(!ItemQuality.Advance(account,item,ref random))throw new InvalidOperationException("Fixture masterwork failed.");
                if(!quality)
                {
                    item.awakened=false;item.masterwork=0;item.masterworkLines.Clear();
                    item.investedMaterials-=item.masterworkInvestedMaterials;item.masterworkInvestedMaterials=0;
                    foreach(var roll in item.rolls)roll.greater=false;
                }
                ItemCatalog.Validate(item);
            }
            return account;
        }
        static QualityCurveRow RunQualityCurveSample(GameCatalog catalog,AccountSave account,int stage,int variant,int index,uint seed,bool quality)
        {
            var sim=new CombatSimulation(account,catalog,stage,seed:seed);var run=sim.State;
            var row=new QualityCurveRow{stage=stage,heroClass=(int)account.Hero.heroClass,variant=variant,seedIndex=index,seed=seed,quality=quality,
                masterwork=account.Hero.inventory[0].masterwork,greaterAffixes=account.Hero.inventory.Sum(ItemQuality.GreaterCount),
                map=run.layout.fingerprint,encounter=EncounterFingerprint(run),equipmentHash=QualityHash(QualityEquipmentJson(account)),
                attack=sim.Stats.damage,maximumHp=sim.Stats.hp,armor=sim.Stats.armor,resistance=sim.Stats.resistance};
            while(run.phase!=RunPhase.Cleared&&run.phase!=RunPhase.Failed&&row.ticks<10000)
            {
                if(run.portal||run.paused||!string.IsNullOrEmpty(run.navigationError))break;
                sim.Tick(CombatSimulation.Step);row.ticks++;
            }
            row.outcome=AuditOutcome(run,sim.TimeLimit,row.ticks,10000);row.seconds=run.time;row.hp=run.health;row.kills=run.kills;
            row.chests=run.layout.chests.Count(c=>c.phase==ChestPhase.Opened);row.resourceChecks=run.statistics.resourceChecks;
            row.dealt=run.statistics.damage;row.received=run.statistics.incomingDamage;return row;
        }
        public static void RunItemQualityCurve()
        {
            GameCatalog catalog=null;
            try
            {
                int seeds=AuditInteger("-hellscriptQualitySeeds",30);if(seeds>1000)throw new ArgumentOutOfRangeException("seeds");
                string path=AuditArgument("-hellscriptQualityOutput","");
                if(!Path.IsPathFullyQualified(path)||Directory.Exists(path)||File.Exists(path))throw new ArgumentException("Use a new absolute output directory.");
                string assets=Path.GetFullPath(Application.dataPath)+Path.DirectorySeparatorChar;
                if(Path.GetFullPath(path).StartsWith(assets,StringComparison.Ordinal))throw new ArgumentException("Quality evidence belongs outside Assets.");
                Directory.CreateDirectory(path);catalog=ScriptableObject.CreateInstance<GameCatalog>();catalog.Populate();
                var report=new QualityCurveReport{seeds=seeds,expectedRuns=5*6*seeds*2};var rows=new List<QualityCurveRow>();
                void WriteReport()=>File.WriteAllText(Path.Combine(path,"summary.json"),JsonUtility.ToJson(report,true));
                WriteReport();
                using(var output=new StreamWriter(Path.Combine(path,"runs.jsonl")))
                foreach(int stage in new[]{30,45,60,75,90})for(int c=0;c<3;c++)for(int variant=0;variant<2;variant++)
                {
                    for(int index=0;index<seeds;index++)
                    {
                        uint seed=unchecked((uint)(54321+index*1337+c*97+variant*31));QualityCurveRow previous=null;
                        foreach(bool quality in new[]{false,true})
                        {
                            var account=QualityCurveAccount((HeroClass)c,variant,stage,seed,quality);
                            if(index==0)File.WriteAllText(Path.Combine(path,$"equipment-{stage}-{c}-{variant}-{quality}.json"),QualityEquipmentJson(account));
                            var row=RunQualityCurveSample(catalog,account,stage,variant,index,seed,quality);
                            if(previous!=null&&(row.map!=previous.map||row.encounter!=previous.encounter))throw new InvalidOperationException("Paired encounters differ.");
                            previous=row;rows.Add(row);output.WriteLine(JsonUtility.ToJson(row));report.completedRuns++;
                        }
                        output.Flush();
                    }
                    foreach(bool quality in new[]{false,true})
                    {
                        var subset=rows.Where(r=>r.stage==stage&&r.heroClass==c&&r.variant==variant&&r.quality==quality).ToArray();
                        var wins=subset.Where(r=>r.outcome=="Cleared").ToArray();int defeats=subset.Count(r=>r.outcome=="HeroDeath"||r.outcome=="TimeLimit"||r.outcome=="OtherFailure");
                        report.summaries.Add(new QualityCurveSummary{stage=stage,heroClass=c,variant=variant,quality=quality,runs=subset.Length,
                            clears=wins.Length,defeats=defeats,incomplete=subset.Length-wins.Length-defeats,clearRate=(float)wins.Length/subset.Length,
                            meanClearSeconds=wins.Length==0?0:wins.Average(r=>r.seconds)});
                    }
                    WriteReport();Debug.Log($"QUALITY_CURVE stage={stage} hero={c} variant={variant} completed={report.completedRuns}/{report.expectedRuns}");
                }
                report.status="complete";WriteReport();
                if(Application.isBatchMode)EditorApplication.Exit(0);
            }
            catch(Exception exception)
            {Debug.LogException(exception);if(Application.isBatchMode)EditorApplication.Exit(1);else throw;}
            finally{if(catalog!=null)UnityEngine.Object.DestroyImmediate(catalog);}
        }
    }
}
