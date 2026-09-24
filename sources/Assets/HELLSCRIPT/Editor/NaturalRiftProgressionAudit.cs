using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Hellscript.Editor
{
    // A continuous, isolated new-account playthrough using the production simulation and save
    // transactions. The only accelerated input is the foreground clock; no progression is granted.
    public static class NaturalRiftProgressionAudit
    {
        [Serializable] public sealed class Wallet
        {
            public int gold,materials,stones,premium,gems,runes;
            public int[] cores;
            public List<GemStack> gemStacks;
        }
        [Serializable] public sealed class Snapshot
        {
            public int level,xp,highestClear,equippedLegendary;
            public float attack,hp,offense,survival;
            public Wallet wallet;
            public int[] slots;
            public ClassSkillLoadout skills;
            public List<Item> equipment;
            public List<PotionStack> potions;
            public List<ForgeJob> forge;
        }
        [Serializable] public sealed class Acquisition
        {
            public int id;
            public string source;
            public float firstSeenSeconds;
            public Item item;
            public bool claimed,ignored;
        }
        [Serializable] public sealed class ActionRow
        {
            public int attempt;
            public double riftSeconds;
            public string action,detail;
            public Snapshot after;
        }
        [Serializable] public sealed class Attempt
        {
            public int number,stage,kills,normalKills,eliteKills,addKills,goblinKills,chests,levelUps,potionUses;
            public uint seed;
            public string outcome,finish,map,objective,navigationError;
            public bool farming,bossRewarded,normalClearRecorded;
            public double riftSeconds,cumulativeRiftSeconds,failedRiftSeconds,wallSeconds;
            public float combatSeconds,damage;
            public Snapshot before,afterCombat,afterTown;
            public List<Acquisition> equipment=new List<Acquisition>();
            public List<RiftResourceDrop> resources;
            public List<GrowthEvent> growth;
            public List<string> lastLogs;
        }
        [Serializable] public sealed class Header
        {
            public int version=1,target=10,heroIndex,attempts,clears,failures,deaths,timeouts,farms,days=1;
            public uint seed=20260924;
            public string status="running",startedUtc,completedUtc,policy;
            public double riftSeconds,failedRiftSeconds,wallSeconds,calendarRestSeconds;
            public bool reloadVerified;
            public Snapshot initial,final;
        }
        sealed class Session
        {
            public GameCatalog catalog;
            public GameStore store;
            public string directory;
            public Header header=new Header();
            public long clock;
            public int attempt;
            public uint craftRandom=0x715517u;
            public System.Diagnostics.Stopwatch wall=System.Diagnostics.Stopwatch.StartNew();
            public string Request()=>"natural-audit-"+Guid.NewGuid().ToString("N");
            public void Require(bool success){if(!success)throw new InvalidOperationException(store.Error);}
            public void Event(string action,string detail)
            {
                File.AppendAllText(Path.Combine(directory,"actions.jsonl"),JsonUtility.ToJson(new ActionRow
                {attempt=attempt,riftSeconds=header.riftSeconds,action=action,detail=detail,after=Capture(store.Data)})+"\n");
            }
            public void SaveHeader()=>File.WriteAllText(Path.Combine(directory,"summary.json"),JsonUtility.ToJson(header,true));
        }
        static T Copy<T>(T value)=>JsonUtility.FromJson<T>(JsonUtility.ToJson(value));
        static Snapshot Capture(AccountSave a)
        {
            var h=a.Hero;var stats=new HeroStats(h,false,a.runes);var power=new ShopPower(stats,h.level);
            return new Snapshot{level=h.level,xp=h.xp,highestClear=h.highestClear,attack=stats.attackPower,hp=stats.hp,
                offense=power.offense,survival=power.survival,equippedLegendary=h.inventory.Count(i=>i.equipped&&i.rarity==3),
                wallet=new Wallet{gold=a.gold,materials=a.materials,stones=a.enhancementStones,premium=a.premium,
                    gems=a.gems.Sum(g=>g.count),runes=a.runes.owned.Count,cores=(int[])a.cores.Clone(),gemStacks=a.gems.Select(Copy).ToList()},
                slots=(int[])h.slotProgress.levels.Clone(),skills=Copy(h.build.classSkills),equipment=h.inventory.Where(i=>i.equipped).Select(Copy).ToList(),
                potions=h.potions.stacks.Select(Copy).ToList(),forge=a.forge.jobs.Select(Copy).ToList()};
        }
        static double Score(HeroStats stats,int level)
        {var p=new ShopPower(stats,level);return p.offense*Math.Sqrt(p.survival);}
        static void EquipUpgrades(Session s)
        {
            // A fixed, greedy sheet comparison; no knowledge of future drops or encounter RNG.
            for(int pass=0;pass<30;pass++)
            {
                var a=s.store.Data;var h=a.Hero;string best=null;int target=0;
                double bestScore=Score(new HeroStats(h,false,a.runes),h.level)*1.005;
                foreach(var item in h.inventory.Where(i=>!i.equipped).ToArray())
                    foreach(int index in EquipmentSlots.Targets(h,item))
                    {
                        var preview=ItemComparison.Preview(h,item,index,a.runes);
                        if(preview.after==null)continue;double score=Score(preview.after,h.level);
                        if(score>bestScore){best=item.id;target=index;bestScore=score;}
                    }
                if(best==null)return;
                s.Require(s.store.EquipInventoryItem(s.Request(),best,target));s.Event("equip",best+":"+target);
            }
            throw new InvalidOperationException("Equipment selection did not converge.");
        }
        static void Cleanup(Session s)
        {
            // Keep all legendaries and items whose required character level has not been reached.
            var ids=s.store.Data.Hero.inventory.Where(i=>!i.equipped&&i.rarity<3&&i.RequiredLevel<=s.store.Data.Hero.level).Select(i=>i.id).ToArray();
            foreach(string id in ids)
            {
                bool sell=s.store.Data.gold<700;
                bool ok=s.store.Transact(s.Request(),"audit-"+(sell?"sell:":"salvage:")+id,a=>
                {var item=a.Hero.inventory.Find(i=>i.id==id);return sell?Economy.Sell(a,a.Hero,item):Economy.Dismantle(a,a.Hero,item);});
                if(ok)s.Event(sell?"sell":"salvage",id);
                // The production protection checks retain preset/favourite items.
            }
        }
        static void Skills(Session s)
        {
            var hero=s.store.Data.Hero;var load=ClassSkillTree.FromHero(hero);string before=JsonUtility.ToJson(load);
            foreach(var skill in ClassSkills.For(hero.heroClass).Where(d=>!d.Passive&&!d.Ultimate))
            {
                if(load.actives.Contains(skill.id)||!ClassSkillTree.Unlocked(load,hero.level,skill.id))continue;
                int empty=Array.IndexOf(load.actives,"");if(empty<0)break;
                load=ClassSkillTree.Equip(load,hero,skill.id,empty);
            }
            while(ClassSkillTree.Remaining(load,hero.level)>0)
            {
                var rank=load.ranks.Where(r=>load.actives.Contains(r.id)&&r.rank<ClassSkills.Find(r.id).maxRank).OrderBy(r=>r.rank).ThenBy(r=>r.id,StringComparer.Ordinal).FirstOrDefault();
                if(rank==null)break;load=ClassSkillTree.ChangeRank(load,hero,rank.id,1);
            }
            if(before==JsonUtility.ToJson(load))return;
            var edit=new HuntEdictEditSession(hero);edit.ReplaceDraft(new HuntEdictLoadout{version=2,classSkills=load,edict=load.ProjectEdict()});
            s.Require(s.store.CommitHuntEdict(edit,s.catalog));s.Event("skills",string.Join(",",load.actives));
        }
        static void ClaimAndOpen(Session s)
        {
            for(int stage=1;stage<=10;stage++)if(RewardBoxes.Status(s.store.Data,stage)=="ready")
            {s.Require(s.store.ClaimRiftFirstRewards(s.Request(),stage));s.Event("first-clear-claim",stage.ToString());}
            int choiceIndex=0;
            foreach(var box in s.store.Data.rewardBoxes.owned.Select(Copy).ToArray())
            {
                var def=RewardBoxCatalog.Find(box.boxId);string choice=def.kind=="gem"&&string.IsNullOrEmpty(def.gemId)?(choiceIndex++%2==0?"G01":"G03"):"";
                for(int n=0;n<box.count;n++)
                {
                    if(Economy.FreeSlots(s.store.Data.Hero)<=0){EquipUpgrades(s);Cleanup(s);}
                    s.Require(s.store.OpenRewardBox(s.Request(),box.id,1,choice));
                    s.Event("open-box",box.boxId+"; stage="+box.sourceStage+"; choice="+choice+"; items="+string.Join(",",s.store.LastBoxEquipment.Select(i=>i.id)));
                    foreach(var item in s.store.LastBoxEquipment)
                        File.AppendAllText(Path.Combine(s.directory,"box-equipment.jsonl"),JsonUtility.ToJson(item)+"\n");
                }
            }
        }
        static void Town(Session s,string visit)
        {
            s.Require(s.store.SettleForgeJobs());EquipUpgrades(s);Cleanup(s);ClaimAndOpen(s);EquipUpgrades(s);Cleanup(s);Skills(s);
            s.Require(s.store.PreparePotions(s.store.Data.Hero.id,visit));s.Event("prepare-potions",visit);
            var a=s.store.Data;
            if(ContentUnlocks.Has(a,ContentUnlocks.RareCraft)&&a.materials>=50&&a.gold>=500*Math.Max(1,a.Hero.highestClear)+700)
            {
                int slot=Enumerable.Range(1,7).FirstOrDefault(n=>!a.Hero.inventory.Any(i=>i.equipped&&i.slot==n));
                if(slot>0)
                {
                    s.Require(s.store.Transact(s.Request(),"audit-rare-craft:"+slot,x=>ContentServices.Purchase(x,slot,1,ref s.craftRandom,out _)));
                    s.Event("rare-craft",slot.ToString());EquipUpgrades(s);
                }
            }
            if(ContentUnlocks.Has(s.store.Data,ContentUnlocks.Enhance))
            {
                int cap=s.store.Data.Hero.highestClear<5?1:2;
                foreach(string id in s.store.Data.Hero.inventory.Where(i=>i.equipped).OrderBy(i=>i.slot).Select(i=>i.id).ToArray())
                    while(true)
                    {
                        var item=s.store.Data.Hero.inventory.Find(i=>i.id==id);if(item.enhancement>=cap)break;
                        var quote=GearEnhancement.Quote(item,s.store.Data.gold);
                        if(quote.gold>s.store.Data.gold-700)break;
                        s.Require(s.store.EnhanceEquipment(s.Request(),s.store.Data.Hero.id,quote));s.Event("enhance",id+"; to="+quote.to+"; gold="+quote.gold);
                    }
            }
            if(ContentUnlocks.Has(s.store.Data,ContentUnlocks.SlotEnhance))
            {
                foreach(string slot in new[]{"mainhand","chest","head","hands","feet","neck","waist","leftRing","rightRing","offhand"})
                {
                    a=s.store.Data;int level=a.Hero.slotProgress.levels[BlacksmithCatalog.Index(slot)];
                    if(level>=2||a.forge.jobs.Count>=a.forge.stations||a.forge.jobs.Any(j=>j.slot==slot)||a.enhancementStones<BlacksmithCatalog.Stones(level+1))continue;
                    s.Require(s.store.StartSlotUpgrade(s.Request(),a.Hero.id,slot,level));s.Event("slot-start",slot+"; target="+(level+1));
                }
            }
            s.Require(s.store.Save());
        }
        static string Source(RunState run,DropState drop,List<EnemyState> newlyDead)
        {
            if(run.layout.chests.Any(c=>c.dropId==drop.id&&c.phase==ChestPhase.Opened))return "chest";
            if(newlyDead.Any(e=>e.boss)&&Enumerable.Range(0,3).Any(i=>Vector2.Distance(drop.position,run.position+new Vector2(i-1,1))<.01f))return "boss";
            var at=newlyDead.Where(e=>Vector2.Distance(e.position,drop.position)<.01f).Select(e=>e.boss?"boss":e.goblin?"goblin":e.add?"add":e.elite>=0?"elite":"normal").Distinct().ToArray();
            return at.Length==1?at[0]:"unknown";
        }
        static Attempt Play(Session s,int stage,bool farming)
        {
            var row=new Attempt{number=++s.attempt,stage=stage,farming=farming,before=Capture(s.store.Data),seed=unchecked(s.header.seed+(uint)s.attempt*1337u)};
            var sw=System.Diagnostics.Stopwatch.StartNew();
            string departure=PotionPolicy.Resolve(s.store.Data.Hero).DepartureReason(s.store.Data.Hero.potions);
            if(departure!="")throw new InvalidOperationException(departure);
            s.Require(s.store.RefreshRiftDay());var sim=new CombatSimulation(s.store.Data,s.catalog,stage,seed:row.seed);var run=sim.State;
            s.store.Data.suspendedRun=run;
            sim.CommitChest=c=>s.store.CommitChest(run,c);
            sim.CommitRunChange=(request,operation,change)=>s.store.CommitRunMutation(run,request,operation,change);
            s.Require(s.store.CommitRiftEntry(run,1,false));
            var seenDead=new HashSet<int>();var seenDrops=new HashSet<int>();int ticks=0;
            while(run.phase!=RunPhase.Cleared&&run.phase!=RunPhase.Failed)
            {
                if(!string.IsNullOrEmpty(run.navigationError)||run.paused)throw new InvalidOperationException("Simulation blocked: "+run.navigationError);
                if(run.portal)
                {
                    Cleanup(s);
                    if(Economy.FreeSlots(s.store.Data.Hero)<=0||sim.GemBagBlocked)throw new InvalidOperationException("Loot portal cannot be cleared legally.");
                    run.portal=false;run.portalCast=0;run.paused=false;s.Require(s.store.Save());s.Event("portal-return","inventory cleanup");
                }
                if(++ticks>18000)throw new InvalidOperationException("Run exceeded the 900-second diagnostic limit.");
                s.clock+=50;double elapsed=s.store.AttendRift(run,50);run.realTime=(float)(run.riftAttendance.elapsedMs/1000);
                s.header.riftSeconds+=elapsed/1000;
                if(elapsed<50){sim.ExhaustFatigue();break;}
                sim.Tick(CombatSimulation.Step);
                var dead=run.enemies.Where(e=>e.dead&&seenDead.Add(e.id)).ToList();
                row.normalKills+=dead.Count(e=>!e.boss&&!e.goblin&&!e.add&&e.elite<0);row.eliteKills+=dead.Count(e=>!e.boss&&!e.goblin&&!e.add&&e.elite>=0);
                row.addKills+=dead.Count(e=>e.add);row.goblinKills+=dead.Count(e=>e.goblin&&run.goblin?.escaped!=true);
                foreach(var drop in run.drops)if(seenDrops.Add(drop.id))row.equipment.Add(new Acquisition{id=drop.id,source=Source(run,drop,dead),firstSeenSeconds=run.realTime,item=Copy(drop.item)});
                if(ticks%20==0)s.Require(s.store.SettleForgeJobs());
            }
            foreach(var item in row.equipment){var drop=run.drops.Single(d=>d.id==item.id);item.claimed=drop.claimed;item.ignored=drop.ignored;}
            row.outcome=BuildIntegrationValidation.AuditOutcome(run,sim.TimeLimit,ticks,18000);row.finish=run.statistics.finish.ToString();
            row.riftSeconds=run.riftAttendance.elapsedMs/1000;row.combatSeconds=run.time;row.cumulativeRiftSeconds=s.header.riftSeconds;
            row.kills=run.kills;row.damage=run.dealt;row.map=run.layout.fingerprint;row.objective=run.layout.objective.ToString();
            row.bossRewarded=run.bossRewarded;row.normalClearRecorded=s.store.Data.Hero.riftProgress.Best(stage)>0;
            row.chests=run.layout.chests.Count(c=>c.phase==ChestPhase.Opened);row.potionUses=run.potions.uses;
            row.growth=run.growthEvents.Select(Copy).ToList();
            row.resources=run.resources.Select(Copy).ToList();row.navigationError=run.navigationError;
            row.lastLogs=run.logs.TakeLast(12).ToList();
            row.afterCombat=Capture(s.store.Data);row.levelUps=row.afterCombat.level-row.before.level;
            File.WriteAllText(Path.Combine(s.directory,"run-"+row.number.ToString("D3")+".json"),JsonUtility.ToJson(run));
            s.store.Data.suspendedRun=null;s.store.Data.repeatHunt=null;s.Require(s.store.Save());
            if(row.outcome=="Cleared"){s.header.clears++;if(!row.normalClearRecorded)throw new InvalidOperationException("Clear lacked a normal entry record.");}
            else{s.header.failures++;s.header.failedRiftSeconds+=row.riftSeconds;if(row.outcome=="HeroDeath")s.header.deaths++;if(row.outcome=="TimeLimit")s.header.timeouts++;}
            if(farming)s.header.farms++;
            row.failedRiftSeconds=s.header.failedRiftSeconds;
            Town(s,"return:"+run.id);row.afterTown=Capture(s.store.Data);row.wallSeconds=sw.Elapsed.TotalSeconds;
            s.header.attempts=s.attempt;s.header.final=row.afterTown;s.header.wallSeconds=s.wall.Elapsed.TotalSeconds;
            File.AppendAllText(Path.Combine(s.directory,"attempts.jsonl"),JsonUtility.ToJson(row)+"\n");s.SaveHeader();
            Debug.Log($"HELLSCRIPT_NATURAL_PROGRESS attempt={row.number} R{stage} {row.outcome} level={row.afterTown.level} seconds={row.riftSeconds:F2} cumulative={s.header.riftSeconds:F2} failures={s.header.failures}");
            return row;
        }
        public static void Run()=>RunSession(Environment.GetCommandLineArgs());
        public static void RunEarlyClasses()
        {
            var args=Environment.GetCommandLineArgs();int at=Array.IndexOf(args,"-hellscriptNaturalOutput");
            if(at<0)throw new ArgumentException("An isolated output root is required.");
            foreach(int hero in new[]{0,1,2})RunSession(new[]{"-hellscriptNaturalOutput",Path.Combine(args[at+1],((HeroClass)hero).ToString()),
                "-hellscriptNaturalHero",hero.ToString(),"-hellscriptNaturalTarget",hero==0?"10":"5"});
        }
        static void RunSession(string[] args)
        {
            int at=Array.IndexOf(args,"-hellscriptNaturalOutput");
            string directory=Path.GetFullPath(at>=0?args[at+1]:"Artifacts/NaturalRift10/"+DateTime.UtcNow.ToString("yyyyMMdd-HHmmss"));
            if(Directory.Exists(Path.Combine(directory,"save"))||File.Exists(Path.Combine(directory,"summary.json")))throw new IOException("Use a fresh output directory; never replace a playthrough.");
            Directory.CreateDirectory(directory);var s=new Session{directory=directory,clock=DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()};
            s.catalog=ScriptableObject.CreateInstance<GameCatalog>();s.catalog.Populate();
            s.store=new GameStore(Path.Combine(directory,"save"),s.catalog,()=>s.clock/1000);s.store.RiftClock=()=>s.clock;
            s.header.startedUtc=DateTime.UtcNow.ToString("O");
            int Arg(string key,int fallback){int index=Array.IndexOf(args,key);return index<0?fallback:int.Parse(args[index+1]);}
            s.header.target=Arg("-hellscriptNaturalTarget",10);s.header.heroIndex=Arg("-hellscriptNaturalHero",0);
            s.header.seed=(uint)Arg("-hellscriptNaturalSeed",20260924);
            if(s.header.target<1||s.header.target>30||s.header.heroIndex<0||s.header.heroIndex>2)throw new ArgumentOutOfRangeException("Audit target or hero");
            s.store.Data.selectedHero=s.header.heroIndex;
            s.header.policy="Default new Warrior; continuous save; production CombatSimulation at 0.05 s and 1x attendance; seeds=20260924+1337*attempt fixed in advance; no grants, rerolls or retries discarded. Town has zero modeled time. Use earned gear by offense*sqrt(mixed survival) greedy comparison (>0.5%); preserve legendaries and future-level items; sell spare nonlegendaries below 700 gold, otherwise salvage. Fill empty skill slots in catalog order and rank equipped skills evenly. Claim/open all eligible first-clear boxes; choose G01/G03 gems. Craft one missing nonweapon slot when affordable; maintain 700 gold reserve; gear enhancement cap +1 before R5 and +2 thereafter. Two free forge stations, level-2 slot target, real durations. Retry failures; after two consecutive failures at R>1 farm previous rift once. Free daily 120 minutes, no paid recovery, no offline rewards requested. Stop at normal R10 clear or 100 attempts; all attempts persist. This is accelerated production-core play, not manual UI play or a population win-rate estimate.";
            try
            {
                s.Require(s.store.ActivateSkillTrees(s.catalog));s.Require(s.store.Save());s.header.policy=s.header.policy.Replace("Default new Warrior","Default new "+s.store.Data.Hero.heroClass).Replace("20260924+",s.header.seed+"+").Replace("normal R10 clear","normal R"+s.header.target+" clear");
                s.header.initial=Capture(s.store.Data);
                if(s.header.initial.level!=1||s.header.initial.xp!=0||s.header.initial.highestClear!=0||s.header.initial.equipment.Count!=1)throw new InvalidOperationException("Not a fresh level-one account.");
                File.WriteAllText(Path.Combine(directory,"initial-account.json"),JsonUtility.ToJson(s.store.Data,true));s.SaveHeader();Town(s,"arrival:"+s.store.Data.Hero.id);
                int failures=0;
                while(s.store.Data.Hero.riftProgress.Best(s.header.target)<=0&&s.attempt<100)
                {
                    if(s.store.Data.riftFatigue.Total<=0&&s.store.Data.riftFatigue.version>0)
                    {
                        long next=RiftEntryRules.Midnight(s.clock)+1;s.header.calendarRestSeconds+=(next-s.clock)/1000.0;s.clock=next;s.header.days++;
                        s.Require(s.store.RefreshRiftDay());s.Event("next-day","Free daily allowance; calendar gap recorded separately");
                    }
                    int stage=Math.Min(s.header.target,s.store.Data.Hero.highestClear+1);bool farm=failures>=2&&stage>1;
                    var row=Play(s,farm?stage-1:stage,farm);
                    failures=farm||row.outcome=="Cleared"?0:failures+1;
                }
                s.header.status=s.store.Data.Hero.riftProgress.Best(s.header.target)>0?"complete":"attempt-limit";
                s.Require(s.store.Save());File.WriteAllText(Path.Combine(directory,"final-account.json"),JsonUtility.ToJson(s.store.Data,true));
                var before=JsonUtility.ToJson(Capture(s.store.Data));var reloaded=new GameStore(Path.Combine(directory,"save"),s.catalog,()=>s.clock/1000);
                s.header.reloadVerified=before==JsonUtility.ToJson(Capture(reloaded.Data));
                if(!s.header.reloadVerified)throw new InvalidOperationException("Final progression snapshot changed after save/reload.");
                s.header.completedUtc=DateTime.UtcNow.ToString("O");s.header.wallSeconds=s.wall.Elapsed.TotalSeconds;s.SaveHeader();
                Debug.Log("HELLSCRIPT_NATURAL_COMPLETE "+JsonUtility.ToJson(s.header));
            }
            catch(Exception e){s.header.status="blocked: "+e.Message;s.header.wallSeconds=s.wall.Elapsed.TotalSeconds;s.SaveHeader();throw;}
            finally{UnityEngine.Object.DestroyImmediate(s.catalog);}
        }
    }
}
