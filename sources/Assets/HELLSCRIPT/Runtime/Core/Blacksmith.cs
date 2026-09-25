using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    public sealed class GearUpgradeQuote
    {
        public string itemId, fingerprint;
        public int from, to, amount;
        public long gold;
        public bool Affordable(int balance)=>to>from&&gold<=balance;
    }
    public static class GearEnhancement
    {
        public const int Maximum=100, MaxAffordable=-1;
        public static long Cost(int itemLevel,int target)
        {
            if(itemLevel<1||target<1||target>Maximum)throw new ArgumentOutOfRangeException();
            try{checked{long l=itemLevel,k=target-1,b=100*l+10*l*l;return (b*(400+20*k+k*k)+399)/400;}}
            catch(OverflowException){return long.MaxValue;}
        }
        public static GearUpgradeQuote Quote(Item item,int gold,int amount=1)
        {
            if(item==null||item.enhancement<0||item.enhancement>Maximum||gold<0||amount!=1&&amount!=10&&amount!=MaxAffordable)throw new ArgumentException("강화 정보를 확인해 주세요.");
            var q=new GearUpgradeQuote{itemId=item.id,fingerprint=JsonUtility.ToJson(item),from=item.enhancement,to=item.enhancement,amount=amount};
            int limit=amount==MaxAffordable?Maximum:Math.Min(Maximum,q.from+amount);
            while(q.to<limit)
            {
                long cost=Cost(item.level,q.to+1);
                if(amount==MaxAffordable&&cost>gold-q.gold)break;
                q.gold=cost>long.MaxValue-q.gold?long.MaxValue:q.gold+cost;q.to++;
            }
            return q;
        }
        public static string Stat(Item item)=>EquipmentSlots.Kind(item)==WeaponKind.Shield?"armor":new[]{"attack","armor","armor","attackSpeed","moveSpeed","life","allResistance","crit"}[item.slot];
        public static float Step(Item item)=>EquipmentSlots.Kind(item)==WeaponKind.Shield?2:new[]{2f,2,3,.08f,.05f,8,.04f,.02f}[item.slot];
        public static float Base(Item item)
        {
            float l=item.level-1,v=ItemCatalog.Base(item).main*(1+.08f*l);
            switch(item.slot){case 3:v=2+.04f*l;break;case 4:v=3+.05f*l;break;case 5:v*=10;break;case 6:v=2+.04f*l;break;case 7:v=1+.02f*l;break;}
            return (float)Math.Round(v,2,MidpointRounding.AwayFromZero)*ItemQuality.MainMultiplier(item);
        }
        public static float Value(Item item,int level=-1)=>Base(item)+Step(item)*(level<0?item.enhancement:Math.Clamp(level,0,Maximum));
        public static string Name(Item item)=>BlacksmithCatalog.StatName(Stat(item));
        public static string Format(Item item,float value)=>BlacksmithCatalog.Format(Stat(item),value);
        public static bool Apply(AccountSave a,Item item,GearUpgradeQuote q)
        {
            if(item==null||q==null||a.suspendedRun!=null||!ContentUnlocks.Has(a,ContentUnlocks.Enhance)||!a.Hero.inventory.Contains(item)||JsonUtility.ToJson(item)!=q.fingerprint)return false;
            var current=Quote(item,a.gold,q.amount);
            if(current.to!=q.to||current.gold!=q.gold||!current.Affordable(a.gold))return false;
            a.gold-=(int)q.gold;item.enhancement=q.to;return true;
        }
    }
    [Serializable] public sealed class SlotProgress
    {
        public int version=1;
        public int[] levels=Enumerable.Repeat(1,10).ToArray();
    }
    [Serializable] public sealed class ForgeJob
    {
        public string id,heroId,slot;
        public int station,from,target,paidStones;
        public long startedUtc,finishUtc;
    }
    [Serializable] public sealed class ForgeAccount
    {
        public int version=1,stations=2;
        public List<ForgeJob> jobs=new List<ForgeJob>();
    }
    [Serializable] public sealed class ForgeLabel { public string ko,en; }
    [Serializable] public sealed class ForgeGrowth { public string stat;public int unlock;public float rate;public bool once; }
    [Serializable] public sealed class ForgeSlot { public string id,art;public ForgeLabel name;public List<ForgeGrowth> growth; }
    [Serializable] public sealed class ForgeCatalogData { public List<ForgeSlot> slots; }
    public static class BlacksmithCatalog
    {
        public static bool Start(AccountSave a,string request,string hero,string slot,int expectedLevel,long now)
        {
                if(a.Hero.id!=hero||a.suspendedRun!=null||!ContentUnlocks.Has(a,ContentUnlocks.SlotEnhance))return false;
                BlacksmithCatalog.Settle(a,now);int index=BlacksmithCatalog.Index(slot),level=a.Hero.slotProgress.levels[index];
                if(level!=expectedLevel||level>=100||a.forge.jobs.Count>=a.forge.stations||a.forge.jobs.Any(j=>j.heroId==hero&&j.slot==slot))return false;
                int cost=BlacksmithCatalog.Stones(level+1);if(a.enhancementStones<cost)return false;
                int station=Enumerable.Range(0,a.forge.stations).First(n=>a.forge.jobs.All(j=>j.station!=n));
                a.enhancementStones-=cost;a.forge.jobs.Add(new ForgeJob{id=request,heroId=hero,slot=slot,station=station,from=level,target=level+1,paidStones=cost,startedUtc=now,finishUtc=checked(now+BlacksmithCatalog.Seconds(level+1))});return true;
        }
        static ForgeCatalogData data;
        public static IReadOnlyList<ForgeSlot> Slots=>(data??=JsonUtility.FromJson<ForgeCatalogData>(Resources.Load<TextAsset>("BlacksmithSlots").text)).slots;
        public static int Index(string id){for(int n=0;n<Slots.Count;n++)if(Slots[n].id==id)return n;throw new ArgumentException("장착 부위를 확인해 주세요.");}
        public static string StatName(string id)
        {
            switch(id)
            {
                case "attack":return "공격력";case "armor":return "방어도";case "life":return "최대 생명력";case "crit":return "치명타 확률";
                case "critDamage":return "치명타 피해";case "attackSpeed":return "공격 속도";case "allSkills":return "모든 스킬 레벨";
                case "allResistance":return "모든 원소 저항";case "damageReduction":return "받는 피해 감소";case "lifePercent":return "최대 생명력 증가";
                case "lifeRegen":return "생명력 재생";case "resource":return "최대 자원";case "cooldown":return "재사용 대기시간 감소";
                case "controlReduction":return "군중 제어 지속시간 감소";case "allStats":return "모든 능력치";case "luckyHit":return "행운의 적중 확률";
                case "moveSpeed":return "이동 속도";case "dodge":return "회피 확률";case "stamina":return "최대 기력";case "staminaRegen":return "기력 회복";
                case "healing":return "받는 치유량";case "resourceCost":return "자원 소모량 감소";case "potionHealing":return "물약 회복량";case "resourceRegen":return "자원 생성";
                default:throw new ArgumentException(id);
            }
        }
        public static string Unit(string id)=>new[]{"lifeRegen","staminaRegen","resourceRegen"}.Contains(id)?Loc.T("/초"):new[]{"attack","armor","life","allSkills","resource","allStats","stamina"}.Contains(id)?"":"%";
        public static string Format(string id,float value)=>"+"+value.ToString("0.##",System.Globalization.CultureInfo.InvariantCulture)+Unit(id);
        public static float Value(ForgeGrowth g,int level)=>level<g.unlock?0:g.rate*(g.once?1:level-g.unlock+1);
        public static int Stones(int target)
        {
            if(target<2||target>100)throw new ArgumentOutOfRangeException(nameof(target));
            int cost=10;for(int n=3;n<=target;n++)cost+=n==100?500:n>=75?50:n>=50?20:n>=25?10:5;return cost;
        }
        public static long Seconds(int target)
        {
            if(target<2||target>100)throw new ArgumentOutOfRangeException(nameof(target));
            long minutes=5;for(int n=3;n<=target;n++)minutes+=n==100?1440:n>=75?60:n>=50?30:n>=25?10:5;return minutes*60;
        }
        public static int StationCost(int stations)=>stations==2?10:stations==3?50:stations==4?250:0;
        public static int SalvageStones(Item item)
        {
            var unique=ItemCatalog.Unique(item.special);
            if(!string.IsNullOrEmpty(unique?.setId))return 10;
            if(unique!=null)return 15;
            return new[]{0,1,5,10}[Math.Clamp(item.rarity,0,3)];
        }
        public static int RewardStones(int stage)=>checked(10+2*Math.Max(1,stage));
        public static long FinishCost(ForgeJob job,long now)=>Math.Max(0,job.finishUtc-now)/60;
        public static void Normalize(AccountSave a)
        {
            a.forge??=new ForgeAccount();a.forge.jobs??=new List<ForgeJob>();
            foreach(var h in a.heroes)h.slotProgress??=new SlotProgress();
            Validate(a);
        }
        public static void Validate(AccountSave a)
        {
            if(a.forge.version>1||a.heroes.Any(h=>h.slotProgress.version>1))throw new NotSupportedException("더 새로운 대장간 저장 버전이 필요합니다.");
            if(a.enhancementStones<0||a.forge.stations<2||a.forge.stations>5||a.heroes.Any(h=>h.slotProgress.levels==null||h.slotProgress.levels.Length!=10||h.slotProgress.levels.Any(l=>l<1||l>100)))throw new InvalidOperationException("대장간 성장 기록을 확인해 주세요.");
            var jobs=a.forge.jobs;
            if(jobs.Count>a.forge.stations||jobs.Any(j=>j==null)||jobs.Select(j=>j.id).Distinct().Count()!=jobs.Count||jobs.Select(j=>j.station).Distinct().Count()!=jobs.Count||jobs.Select(j=>j.heroId+":"+j.slot).Distinct().Count()!=jobs.Count)throw new InvalidOperationException("강화 작업 기록이 중복되었습니다.");
            foreach(var j in jobs)
            {
                var h=a.heroes.Find(x=>x.id==j.heroId);
                if(string.IsNullOrEmpty(j.id)||h==null||j.station<0||j.station>=a.forge.stations||j.from<1||j.target!=j.from+1||j.target>100||h.slotProgress.levels[Index(j.slot)]!=j.from||j.paidStones!=Stones(j.target)||j.startedUtc<0||j.finishUtc-j.startedUtc!=Seconds(j.target))throw new InvalidOperationException("강화 작업 기록을 확인해 주세요.");
            }
        }
        public static int Settle(AccountSave a,long now)
        {
            var done=a.forge.jobs.Where(j=>j.finishUtc<=now).ToArray();
            foreach(var j in done){a.heroes.First(h=>h.id==j.heroId).slotProgress.levels[Index(j.slot)]=j.target;a.forge.jobs.Remove(j);}return done.Length;
        }
    }
}
