using System;
using System.Linq;

namespace Hellscript
{
    [Serializable] public sealed class OfflineSupplyReceipt
    {
        public string id="";
        public int stage,gold,stones;
        public long seconds;
        public bool acknowledged;
    }
    [Serializable] public sealed class OfflineSupplyState
    {
        public int version;
        public string heroId="",runId="";
        public int stage,goldPerRun,stonesPerRun;
        public long cycleMs;
        public bool estimated;
        public double goldFraction,stoneFraction;
        public OfflineSupplyReceipt receipt;
    }
    public sealed class OfflineSupplyQuote
    {
        public long through,seconds;
        public int stage,gold,stones;
        public double goldFraction,stoneFraction;
    }
    // Shared by the development store and the future account authority. Time and account data
    // are inputs from the authority; client-reported time, stages or amounts are never evidence.
    public static class OfflineSupplies
    {
        public const int Version=1,Divisor=20,CapHours=12;
        public const long CapSeconds=CapHours*3600;
        public static double GoldPerHour(OfflineSupplyState s)=>s?.cycleMs>0?s.goldPerRun*3600000d/(s.cycleMs*Divisor):0;
        public static double StonesPerHour(OfflineSupplyState s)=>s?.cycleMs>0?s.stonesPerRun*3600000d/(s.cycleMs*Divisor):0;
        public static void Normalize(AccountSave a)
        {
            var s=a.offlineSupplies;
            if(a.schema>=14&&(s==null||s.version!=Version))throw new NotSupportedException("Unsupported offline supply save version.");
            if(s==null||s.version==0)
            {
                s=a.offlineSupplies=new OfflineSupplyState{version=Version};
                // Old saves have no resource ledger in their completed reviews. Prefer the last
                // completed run's owner/stage; never substitute another hero's account record.
                var record=a.records?.FirstOrDefault(r=>r.review?.phase==RunPhase.Cleared&&r.review.totals?.finish==CombatFinish.Other);
                var hero=record==null?a.Hero:a.heroes.FirstOrDefault(h=>h.id==record.review.heroId)??a.Hero;
                int stage=record?.stage??hero.highestClear;
                if(stage>0&&stage<=hero.highestClear)
                {
                    SetEstimate(s,hero.id,Math.Min(1000,stage));
                    if(record!=null)s.cycleMs=Math.Max(s.cycleMs,(long)Math.Ceiling(Math.Max(record.realSeconds,record.simulationSeconds)*1000)+5000);
                }
            }
            Validate(a);
        }
        public static void SetEstimate(OfflineSupplyState s,string heroId,int stage)
        {
            s.heroId=heroId;s.stage=stage;s.estimated=true;s.runId="legacy";
            // Gate-meter-equivalent enemy gold plus the boss, with no gold-find bonus,
            // chests, first-clear grants or sales. Only a migration fallback, never a live run grant.
            s.goldPerRun=(stage<=5?50:100)*(5+stage)+800+50*stage;
            s.stonesPerRun=BlacksmithCatalog.RewardStones(stage);s.cycleMs=stage<=5?105000:185000;
        }
        public static void Validate(AccountSave a)
        {
            var s=a.offlineSupplies;
            if(s==null||s.version!=Version||s.stage<0||s.stage>1000||s.goldPerRun<0||s.stonesPerRun<0||s.cycleMs<0||s.cycleMs>86400000||
                !Fraction(s.goldFraction)||!Fraction(s.stoneFraction)||
                s.stage>0&&(s.cycleMs<5000||string.IsNullOrEmpty(s.runId)||!a.heroes.Any(h=>h.id==s.heroId&&h.highestClear>=s.stage)))
                throw new NotSupportedException("Invalid offline supply basis; original save preserved.");
            var r=s.receipt;
            if(r!=null&&(r.gold<0||r.stones<0||r.seconds<0||r.seconds>CapSeconds||r.stage<0||r.stage>1000))throw new NotSupportedException("Invalid offline supply receipt.");
        }
        static bool Fraction(double value)=>!double.IsNaN(value)&&!double.IsInfinity(value)&&value>=0&&value<1;
        public static void RecordClear(AccountSave a,RunState run,float resultSeconds)
        {
            if(run.training>=0||run.phase!=RunPhase.Cleared||!run.bossRewarded||run.statistics?.finish!=CombatFinish.Other||run.stage<1||run.stage>1000)return;
            Normalize(a);var s=a.offlineSupplies;if(s.runId==run.id)return;
            double seconds=Math.Max(run.time,run.realTime);
            if(double.IsNaN(seconds)||double.IsInfinity(seconds)||seconds<=0)return;
            s.heroId=run.heroId;s.runId=run.id;s.stage=run.stage;s.estimated=false;
            // Claimed field currency excludes first-clear gold and chest/box/shop grants.
            s.goldPerRun=(int)Math.Min(int.MaxValue,run.resources.Where(r=>r.claimed&&r.kind==RiftResourceKind.Gold).Sum(r=>(long)r.amount));
            s.stonesPerRun=(int)Math.Min(int.MaxValue,run.resources.Where(r=>r.claimed&&r.kind==RiftResourceKind.EnhancementStone).Sum(r=>(long)r.amount));
            s.cycleMs=(long)Math.Ceiling(seconds*1000)+Math.Max(5000,(long)(resultSeconds*1000));
        }
        public static OfflineSupplyQuote Quote(AccountSave a,long now)
        {
            Validate(a);var s=a.offlineSupplies;
            var q=new OfflineSupplyQuote{through=Math.Max(a.lastSeenUtc,now),stage=s.stage,goldFraction=s.goldFraction,stoneFraction=s.stoneFraction};
            if(s.stage==0||a.lastSeenUtc<=0||!ContentUnlocks.Has(a,ContentUnlocks.Offline))return q;
            long since=Math.Max(a.lastSeenUtc,a.contentUnlocks.offlineActivatedUtc);
            q.seconds=now<=since?0:Math.Min(CapSeconds,now-since);
            double gold=s.goldFraction+q.seconds*1000d*s.goldPerRun/(s.cycleMs*Divisor);
            double stones=s.stoneFraction+q.seconds*1000d*s.stonesPerRun/(s.cycleMs*Divisor);
            double wholeGold=Math.Floor(gold+1e-9),wholeStones=Math.Floor(stones+1e-9);
            q.gold=(int)Math.Min(int.MaxValue-(long)a.gold,wholeGold);
            q.stones=(int)Math.Min(int.MaxValue-(long)a.enhancementStones,wholeStones);
            q.goldFraction=Math.Max(0,gold-wholeGold);q.stoneFraction=Math.Max(0,stones-wholeStones);
            return q;
        }
        public static void Apply(AccountSave a,OfflineSupplyQuote q,string id)
        {
            a.gold=checked(a.gold+q.gold);a.enhancementStones=checked(a.enhancementStones+q.stones);
            a.offlineSupplies.goldFraction=q.goldFraction;a.offlineSupplies.stoneFraction=q.stoneFraction;
            a.lastSeenUtc=Math.Max(a.lastSeenUtc,q.through);
            if(q.gold>0||q.stones>0)a.offlineSupplies.receipt=new OfflineSupplyReceipt{id=id,stage=q.stage,seconds=q.seconds,gold=q.gold,stones=q.stones};
        }
    }
}
