using System;
using System.Collections.Generic;
using System.Linq;

namespace Hellscript
{
    public enum AttendanceKind { Weekly, Monthly }
    public enum AttendanceRewardKind { Coins, Gems, Gold, Cores, LegendaryChest }
    [Serializable] public sealed class AttendanceTrack
    {
        public long period;
        public int earned,claimed;
        public bool Claimed(int day)=>(claimed&(1<<(day-1)))!=0;
    }
    [Serializable] public sealed class AttendanceChest
    {
        public string id;
        public uint seed;
        public int level;
        public HeroClass heroClass;
    }
    [Serializable] public sealed class AttendanceState
    {
        public int version=1;
        public long lastDay;
        public AttendanceTrack weekly=new AttendanceTrack(),monthly=new AttendanceTrack();
        public List<AttendanceChest> chests=new List<AttendanceChest>();
    }
    public readonly struct AttendanceReward
    {
        public readonly AttendanceRewardKind kind;
        public readonly int amount,tier;
        public AttendanceReward(AttendanceRewardKind kind,int amount,int tier=0){this.kind=kind;this.amount=amount;this.tier=tier;}
        public string Name=>kind switch
        {
            AttendanceRewardKind.Coins=>Loc.T("심연 주화"),
            AttendanceRewardKind.Gems=>Loc.F("{0}단계 무작위 보석",tier),
            AttendanceRewardKind.Gold=>Loc.T("골드"),
            AttendanceRewardKind.Cores=>Loc.T("무작위 부위 코어"),
            _=>Loc.T("무작위 부위 전설 상자")
        };
    }
    // Account progress, calendar boundaries and rewards have one owner. The UI only issues commands.
    public static class Attendance
    {
        public const int Schema=12;
        static readonly int[] monthlyMultipliers={1,2,3,4},monthlyChests={1,1,1,2};
        static readonly AttendanceReward[] weekly={
            new AttendanceReward(AttendanceRewardKind.Coins,10),new AttendanceReward(AttendanceRewardKind.Gems,10,3),
            new AttendanceReward(AttendanceRewardKind.Coins,30),new AttendanceReward(AttendanceRewardKind.Gold,10000),
            new AttendanceReward(AttendanceRewardKind.Coins,50),new AttendanceReward(AttendanceRewardKind.Cores,5),
            new AttendanceReward(AttendanceRewardKind.LegendaryChest,1)};
        public static bool Valid(AttendanceKind kind)=>kind==AttendanceKind.Weekly||kind==AttendanceKind.Monthly;
        public static int Length(AttendanceKind kind)=>kind==AttendanceKind.Weekly?7:28;
        public static long Day(long utcMs)=>RiftEntryRules.Day(utcMs);
        public static DateTime Date(long day)=>DateTime.UnixEpoch.AddDays(day);
        public static long Period(AttendanceKind kind,long day)
        {
            var date=Date(day);
            return kind==AttendanceKind.Weekly?day-((int)date.DayOfWeek+6)%7:day-date.Day+1;
        }
        public static long ResetDay(AttendanceKind kind,long day)=>kind==AttendanceKind.Weekly?Period(kind,day)+7:Period(kind,day)+DateTime.DaysInMonth(Date(day).Year,Date(day).Month);
        public static AttendanceTrack Track(AttendanceState state,AttendanceKind kind)=>kind==AttendanceKind.Weekly?state.weekly:state.monthly;
        public static AttendanceReward Reward(AttendanceKind kind,int day)
        {
            if(!Valid(kind)||day<1||day>Length(kind))throw new ArgumentOutOfRangeException(nameof(day));
            var reward=weekly[(day-1)%7];if(kind==AttendanceKind.Weekly)return reward;
            int week=(day-1)/7;
            // Monthly cadence mirrors the weekly track; the fourth milestone awards two boxes.
            int amount=reward.kind==AttendanceRewardKind.LegendaryChest?monthlyChests[week]:reward.amount*monthlyMultipliers[week];
            return new AttendanceReward(reward.kind,amount,reward.tier);
        }
        public static void Normalize(AccountSave account)
        {
            if(account.attendance==null)account.attendance=new AttendanceState();
            Validate(account.attendance);
        }
        public static void Validate(AttendanceState state)
        {
            if(state==null||state.version!=1||state.lastDay<0||state.lastDay>2932896||state.chests==null)
                throw new NotSupportedException("Unsupported attendance save; original preserved.");
            foreach(var kind in new[]{AttendanceKind.Weekly,AttendanceKind.Monthly})
            {
                var t=Track(state,kind);int max=Length(kind);
                if(t==null||t.period<0||t.earned<0||t.earned>max||t.claimed<0||(t.claimed>>t.earned)!=0||
                   state.lastDay==0&&(t.period!=0||t.earned!=0)||state.lastDay>0&&(t.period!=Period(kind,state.lastDay)||t.earned<1||t.earned>state.lastDay-t.period+1))
                    throw new NotSupportedException("Invalid attendance progress; original preserved.");
            }
            if(state.chests.Any(c=>c==null||string.IsNullOrEmpty(c.id)||c.seed==0||c.level<1||c.level>ItemQuality.MaximumItemLevel||(int)c.heroClass<0||(int)c.heroClass>2)||state.chests.Select(c=>c.id).Distinct().Count()!=state.chests.Count)
                throw new NotSupportedException("Invalid attendance chest; original preserved.");
        }
        public static bool Visit(AccountSave account,long day)
        {
            var state=account.attendance;if(day<state.lastDay)return false;if(day==state.lastDay)return true;
            foreach(var kind in new[]{AttendanceKind.Weekly,AttendanceKind.Monthly})
            {
                var t=Track(state,kind);long period=Period(kind,day);
                if(t.period!=period){t.period=period;t.earned=t.claimed=0;}
                t.earned=Math.Min(Length(kind),t.earned+1);
            }
            state.lastDay=day;return true;
        }
        public static int Pending(AttendanceTrack track)=>Enumerable.Range(1,track.earned).Count(d=>!track.Claimed(d));
        public static uint Seed(string key)
        {uint seed=2166136261;foreach(char c in key){seed^=c;seed=unchecked(seed*16777619);}return seed==0?1:seed;}
        public static bool Grant(AccountSave account,AttendanceKind kind,int day,string id)
        {
            var reward=Reward(kind,day);uint seed=Seed(account.heroes[0].id+":"+id);
            switch(reward.kind)
            {
                case AttendanceRewardKind.Coins: account.premium=checked(account.premium+reward.amount);break;
                case AttendanceRewardKind.Gold: account.gold=checked(account.gold+reward.amount);break;
                case AttendanceRewardKind.Gems:
                    for(int n=0;n<reward.amount;n++)
                    {string gem=GemElixirs.Families[(int)(RandomStream.Next(ref seed)%6)].gemId;if(!GemInventory.Add(account,new GemStack{gemId=gem,tier=reward.tier,count=1}))return false;}
                    break;
                case AttendanceRewardKind.Cores:
                    for(int n=0;n<reward.amount;n++){int slot=(int)(RandomStream.Next(ref seed)%8);account.cores[slot]=checked(account.cores[slot]+1);}break;
                case AttendanceRewardKind.LegendaryChest:
                    for(int n=0;n<reward.amount;n++)account.attendance.chests.Add(new AttendanceChest{id=id+":"+n,seed=RandomStream.Next(ref seed),heroClass=account.Hero.heroClass,level=Math.Max(1,Math.Min(ItemQuality.MaximumItemLevel,account.Hero.highestClear))});break;
            }
            return true;
        }
    }
}
