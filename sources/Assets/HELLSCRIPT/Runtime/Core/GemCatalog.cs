using System;
using System.Collections.Generic;
using System.Linq;

namespace Hellscript
{
    [Serializable] public sealed class SocketState
    {
        public int index,tier;
        public string gemId="";
    }
    [Serializable] public sealed class GemStack
    {
        public string gemId="";
        public int tier,count;
    }
    public enum GemEffectKind { Stat, BuffReduction, PeriodicReduction, ArmorPercent }
    public sealed class GemEffectRow
    {
        public readonly string name;
        public readonly GemEffectKind kind;
        public readonly StatId stat;
        public readonly bool percent;
        public readonly IReadOnlyList<float> values;
        public GemEffectRow(string name,StatId stat,bool percent,params float[] values)
            :this(name,GemEffectKind.Stat,stat,percent,values){}
        public GemEffectRow(string name,GemEffectKind kind,StatId stat,bool percent,params float[] values)
        {
            if(values.Length!=6||values.Any(v=>float.IsNaN(v)||float.IsInfinity(v)||v<=0))throw new ArgumentException("Six positive gem tier values are required.");
            this.name=name;this.kind=kind;this.stat=stat;this.percent=percent;this.values=Array.AsReadOnly((float[])values.Clone());
        }
        public float Value(int tier)
        {if(tier<1||tier>6)throw new ArgumentOutOfRangeException(nameof(tier));return values[tier-1];}
    }
    public sealed class GemDefinition
    {
        public readonly string id,name;
        public readonly int element;
        public readonly GemEffectRow weapon,armor,jewelry;
        public GemDefinition(string id,string name,int element,GemEffectRow weapon,GemEffectRow armor,GemEffectRow jewelry)
        {this.id=id;this.name=name;this.element=element;this.weapon=weapon;this.armor=armor;this.jewelry=jewelry;}
        public GemEffectRow ForSlot(int slot)=>slot==0?weapon:slot==1||slot==2?armor:slot==6||slot==7?jewelry:throw new ArgumentOutOfRangeException(nameof(slot));
    }
    // HELLSCRIPT's own six-tier table. The reference game's values are not imported.
    public static class GemCatalog
    {
        public const int Version=1,SocketItemVersion=3,MaximumTier=6,StackLimit=999;
        public static readonly IReadOnlyList<string> TierNames=Array.AsReadOnly(new[]{"부서진","흐린","맑은","벼려진","완전한","왕관의"});
        static GemEffectRow Weapon(string name,StatId stat)=>new GemEffectRow(name,stat,true,3,5,8,12,17,23);
        static GemEffectRow Resist(string name,StatId stat)=>new GemEffectRow(name,stat,false,8,14,22,32,45,60);
        public static readonly IReadOnlyList<GemDefinition> Gems=Array.AsReadOnly(new[]{
            new GemDefinition("G01","홍옥",Element.Fire,Weapon("화염 피해 증가",StatId.FireDamage),
                new GemEffectRow("최대 HP 증가",StatId.MaximumLifePercent,true,2,3.5f,5,7,9.5f,12),Resist("화염 저항",StatId.FireResistance)),
            new GemDefinition("G02","청옥",Element.Cold,Weapon("냉기 피해 증가",StatId.ColdDamage),
                new GemEffectRow("받는 피해 감소",GemEffectKind.BuffReduction,StatId.DamageReduction,true,1,1.5f,2.5f,3.5f,5,6.5f),Resist("냉기 저항",StatId.ColdResistance)),
            new GemDefinition("G03","황옥",Element.Lightning,Weapon("번개 피해 증가",StatId.LightningDamage),
                new GemEffectRow("초당 자원 회복",StatId.ResourceGeneration,false,.4f,.7f,1.1f,1.6f,2.2f,3),Resist("번개 저항",StatId.LightningResistance)),
            new GemDefinition("G04","취옥",Element.Poison,Weapon("독 피해 증가",StatId.PoisonDamage),
                new GemEffectRow("지속 피해 감소",GemEffectKind.PeriodicReduction,StatId.DamageReduction,true,3,5,8,11,15,20),Resist("독 저항",StatId.PoisonResistance)),
            new GemDefinition("G05","자수정",Element.Shadow,Weapon("암흑 피해 증가",StatId.ShadowDamage),
                new GemEffectRow("이동 속도 증가",StatId.MovementSpeed,true,1,1.5f,2.5f,3.5f,4.5f,6),Resist("암흑 저항",StatId.ShadowResistance)),
            new GemDefinition("G06","금강석",Element.Physical,Weapon("물리 피해 증가",StatId.PhysicalDamage),
                new GemEffectRow("보호막 흡수량 증가",StatId.BarrierGeneration,true,3,5,8,12,16,21),new GemEffectRow("모든 저항",StatId.AllResistance,false,4,7,11,16,22,30)),
            new GemDefinition("G07","해골",-1,new GemEffectRow("극대화 피해 증가",StatId.CriticalStrikeDamage,true,4,7,11,16,22,30),
                new GemEffectRow("방어도 증가",GemEffectKind.ArmorPercent,StatId.Armor,true,3,5,8,12,16,21),new GemEffectRow("물약 회복량 증가",StatId.PotionHealing,true,4,7,11,16,22,30))});
        public static GemDefinition Find(string id)=>Gems.FirstOrDefault(g=>g.id==id);
        public static bool Valid(string id,int tier)=>Find(id)!=null&&tier>=1&&tier<=MaximumTier;
        public static bool AllowsSocket(Item item)=>item!=null&&item.rarity>=2&&item.rarity<=3&&(item.slot==0||item.slot==1||item.slot==2||item.slot==6||item.slot==7);
        public static bool HasGem(Item item)=>item?.sockets?.Any(s=>s!=null&&!string.IsNullOrEmpty(s.gemId))??false;
        public static bool TryEffect(Item item,out GemEffectRow effect,out float value)
        {
            effect=null;value=0;
            if(!AllowsSocket(item)||item.sockets==null||item.sockets.Count!=1)return false;
            var socket=item.sockets[0];if(socket==null||socket.index!=0||!Valid(socket.gemId,socket.tier))return false;
            effect=Find(socket.gemId).ForSlot(item.slot);value=effect.Value(socket.tier);return true;
        }
        public static string SocketSummary(Item item)
        {
            if(TryEffect(item,out var effect,out float value))
            {
                var socket=item.sockets[0];
                return Loc.F("소켓 · {0} {1} ({2}단계)\n이 부위의 효과: {3} +{4:0.##}{5}",TierNames[socket.tier-1],Find(socket.gemId).name,socket.tier,effect.name,value,effect.percent?"%":"");
            }
            return Loc.T(item?.sockets?.Count>0?"소켓 · 비어 있음":AllowsSocket(item)?"소켓 · 아직 내지 않음":"소켓을 낼 수 없는 장비입니다.");
        }
        public static void ValidateSockets(Item item)
        {
            if(item.sockets==null||item.sockets.Count==0)return;
            if(!AllowsSocket(item)||item.sockets.Count>1)throw new ArgumentException(Loc.T("소켓을 지원하지 않는 부위·등급 또는 개수입니다."));
            var socket=item.sockets[0];
            if(socket==null||socket.index!=0||!((string.IsNullOrEmpty(socket.gemId)&&socket.tier==0)||Valid(socket.gemId,socket.tier)))
                throw new ArgumentException(Loc.T("소켓의 보석 종류와 단계를 확인하세요."));
        }
        // Only invalid gem payloads become empty. Structural socket errors stay visible to validation.
        // The caller must archive the original file before saving a repaired account.
        public static bool RepairGemValues(Item item)
        {
            if(item.sockets==null){item.sockets=new List<SocketState>();return false;}
            if(item.sockets.Count>0&&item.contentVersion>0)item.contentVersion=Math.Max(SocketItemVersion,item.contentVersion);
            bool repaired=false;
            foreach(var socket in item.sockets)
                if(socket!=null&&socket.index==0&&!((string.IsNullOrEmpty(socket.gemId)&&socket.tier==0)||Valid(socket.gemId,socket.tier)))
                {socket.gemId="";socket.tier=0;repaired=true;}
            return repaired;
        }
        public static long SocketGold(Item item)=>checked(2000L*Math.Max(1,item.level));
        public static long RemovalGold(Item item)=>checked(500L*Math.Max(1,item.level));
        public static int FusionGold(int targetTier)
        {
            if(targetTier<2||targetTier>MaximumTier)throw new ArgumentOutOfRangeException(nameof(targetTier));
            int cost=300;for(int i=1;i<targetTier;i++)cost=checked(cost*3);return cost;
        }
        public static int TierOneMaterials(int tier)
        {if(tier<1||tier>MaximumTier)throw new ArgumentOutOfRangeException(nameof(tier));int count=1;for(int i=1;i<tier;i++)count*=3;return count;}
        public static long TotalFusionGold(int tier)
        {TierOneMaterials(tier);long total=0;for(int t=2;t<=tier;t++)total=checked(total*3+FusionGold(t));return total;}
        public static int DropBasisPoints(RiftRewardSource source)=>source switch
        {RiftRewardSource.Normal=>150,RiftRewardSource.Elite=>800,RiftRewardSource.Boss=>10000,_=>throw new ArgumentOutOfRangeException(nameof(source))};
        public static int DropCount(RiftRewardSource source)=>source switch
        {RiftRewardSource.Normal=>1,RiftRewardSource.Elite=>1,RiftRewardSource.Boss=>2,_=>throw new ArgumentOutOfRangeException(nameof(source))};
        public static GemStack Roll(int stage,ref uint rng)
        {
            if(stage<1)throw new ArgumentOutOfRangeException(nameof(stage));
            string id=Gems[RandomStream.Range(ref rng,0,Gems.Count)].id;
            int tier=stage<10?1:stage<20?(RandomStream.Range(ref rng,0,100)<70?1:2):stage<30?(RandomStream.Range(ref rng,0,100)<70?2:3):3;
            return new GemStack{gemId=id,tier=tier,count=1};
        }
    }
}
