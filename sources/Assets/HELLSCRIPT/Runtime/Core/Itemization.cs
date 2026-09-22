using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    public enum AffixSide { Prefix, Suffix }

    [Serializable]
    public sealed class AffixRoll
    {
        public string slotId, affixId, tierId;
        public AffixSide side;
        public int rollBasisPoints;
        public float value;
        public bool legacyRoll,greater;
    }

    public sealed class ItemBaseDefinition
    {
        public readonly string id, name;
        public readonly int slot, heroClass, legacyIndex;
        public readonly float main, resistance, attackSpeed;
        public ItemBaseDefinition(string id,string name,int index,int slot,int heroClass,float main,float resistance=0,float speed=0)
        {this.id=id;this.name=name;legacyIndex=index;this.slot=slot;this.heroClass=heroClass;this.main=main;this.resistance=resistance;attackSpeed=speed;}
        public bool Fits(HeroClass c,int s)=>slot==s&&(heroClass<0||heroClass==(int)c);
    }

    public sealed class AffixDefinition
    {
        public readonly string id, phrase, group;
        public readonly int stat, weight, slotMask;
        public readonly float min, max;
        public readonly bool flat;
        public readonly AffixSide side;
        public AffixDefinition(string id,int stat,AffixSide side,string phrase,string group,int weight,float min,float max,bool flat,params int[] slots)
        {this.id=id;this.stat=stat;this.side=side;this.phrase=phrase;this.group=group;this.weight=weight;this.min=min;this.max=max;this.flat=flat;foreach(int s in slots)slotMask|=1<<s;}
        public bool Allows(int slot)=>slot>=0&&slot<8&&(slotMask&(1<<slot))!=0;
        public int Weight(HeroClass c)
        {
            bool preferred=stat==10+(int)c || c==HeroClass.Warrior&&stat==4 || c==HeroClass.Ranger&&(stat==4||stat==8||stat==9) || c==HeroClass.Mage&&stat>=5&&stat<=7;
            return weight*(preferred?3:1);
        }
        public float Scale(int level)=>flat?(1+.03f*(level-1))/1.87f:1;
        public float Value(int level,int q)
        {
            if(level<1||q<0||q>10000)throw new ArgumentOutOfRangeException();
            double scale=Scale(level),value=Math.Round((min+(max-min)*q/10000d)*scale,2,MidpointRounding.AwayFromZero);
            return (float)Math.Max(Math.Ceiling(min*scale*100)/100,Math.Min(Math.Floor(max*scale*100)/100,value));
        }
    }

    public sealed class UniqueItemDefinition
    {
        public readonly string id, name, setId, description, requiredSkill;
        public readonly int heroClass, slot, weight;
        // A set piece writes its name and its two lines from the set it belongs to. The catalogue is
        // built once when the type loads, so those parts are kept and joined when the text is read;
        // joining them at load time would freeze whichever language happened to be active then.
        readonly SetDefinition set;
        readonly string slotWord,nameEn,descriptionEn;
        public UniqueItemDefinition(string id,string name,int heroClass,int slot,int weight,string description,string skill="",string setId="",SetDefinition set=null,string slotWord="",string nameEn="",string descriptionEn="")
        {this.id=id;this.name=name;this.heroClass=heroClass;this.slot=slot;this.weight=weight;this.description=description;requiredSkill=skill;this.setId=setId;this.set=set;this.slotWord=slotWord;this.nameEn=nameEn;this.descriptionEn=descriptionEn;}
        public string Name=>set==null?(Loc.Language=="en"&&!string.IsNullOrEmpty(nameEn)?nameEn:Loc.T(name)):Loc.F("{0}의 {1}",set.name,slotWord);
        public string Description=>set==null?(Loc.Language=="en"&&!string.IsNullOrEmpty(descriptionEn)?descriptionEn:Loc.T(description)):Loc.F("2세트: {0}\n4세트: {1}",set.two,set.four);
        public bool Fits(HeroClass c,int s)=>slot==s&&(heroClass<0||heroClass==(int)c);
    }

    public sealed class SetDefinition
    {
        public readonly string id, name, two, four;
        public readonly HeroClass heroClass;
        public SetDefinition(string id,string name,HeroClass c,string two,string four)
        {this.id=id;this.name=name;heroClass=c;this.two=two;this.four=four;}
    }

    // Stable content IDs are authoritative. Legacy indices are only an import adapter.
    public static class ItemCatalog
    {
        public const int Version=5;
        const AffixSide P=AffixSide.Prefix,S=AffixSide.Suffix;
        public static readonly IReadOnlyList<ItemBaseDefinition> Bases=Array.AsReadOnly(new[]{
            new ItemBaseDefinition("B01","녹슨 도검",0,0,0,17,0,.15f),new ItemBaseDefinition("B02","강철 대검",1,0,0,20),new ItemBaseDefinition("B03","묵철 도끼",2,0,0,23,0,-.15f),
            new ItemBaseDefinition("B04","사냥 활",3,0,1,17,0,.15f),new ItemBaseDefinition("B05","전쟁 활",4,0,1,20),new ItemBaseDefinition("B06","중형 쇠뇌",5,0,1,23,0,-.15f),
            new ItemBaseDefinition("B07","재의 지팡이",6,0,2,17,0,.15f),new ItemBaseDefinition("B08","봉인 지팡이",7,0,2,20),new ItemBaseDefinition("B09","흑요석 지팡이",8,0,2,23,0,-.15f),
            new ItemBaseDefinition("B10","천 두건",9,1,-1,12,8),new ItemBaseDefinition("B11","철 투구",10,1,-1,20),
            new ItemBaseDefinition("B12","가죽 외투",11,2,-1,24,8),new ItemBaseDefinition("B13","철 갑옷",12,2,-1,40),
            new ItemBaseDefinition("B14","천 장갑",13,3,-1,8,8),new ItemBaseDefinition("B15","철 장갑",14,3,-1,14),
            new ItemBaseDefinition("B16","가죽 장화",15,4,-1,8,8),new ItemBaseDefinition("B17","철 장화",16,4,-1,14),
            new ItemBaseDefinition("B18","직물 허리띠",17,5,-1,10,8),new ItemBaseDefinition("B19","철 버클 허리띠",18,5,-1,16),
            new ItemBaseDefinition("B20","뼈 목걸이",19,6,-1,20),new ItemBaseDefinition("B21","은 목걸이",20,6,-1,25),new ItemBaseDefinition("B22","봉인 목걸이",21,6,-1,30),
            new ItemBaseDefinition("B23","철 반지",22,7,-1,5),new ItemBaseDefinition("B24","은 반지",23,7,-1,5),
            new ItemBaseDefinition("B25","재의 한손 지팡이",24,0,2,17,0,.1f),new ItemBaseDefinition("B26","봉인된 오브",25,0,2,5),
            new ItemBaseDefinition("B27","마법 스크롤",26,0,2,5),new ItemBaseDefinition("B28","철 방패",27,0,-1,24),
            new ItemBaseDefinition("B29","사냥 화살",28,0,1,5),new ItemBaseDefinition("B30","사냥꾼 단검",29,0,1,17,0,.15f)
        });
        public static readonly IReadOnlyList<AffixDefinition> Affixes=Array.AsReadOnly(new[]{
            new AffixDefinition("AF01",0,P,"생명 어린","FlatHealth",100,40,100,true,1,2,5,6,7),
            new AffixDefinition("AF02",1,S,"활력","PercentHealth",100,3,8,false,1,2,5),
            new AffixDefinition("AF03",2,P,"견고한","Armor",100,25,60,true,1,2,3,4,5),
            new AffixDefinition("AF04",3,P,"봉인된","Resistance",100,15,35,true,1,2,5,6,7),
            new AffixDefinition("AF05",4,P,"분쇄하는","DamageElement",60,3,8,false,0,3,6,7),
            new AffixDefinition("AF06",5,P,"타오르는","DamageElement",60,3,8,false,0,3,6,7),
            new AffixDefinition("AF07",6,P,"서리 맺힌","DamageElement",60,3,8,false,0,3,6,7),
            new AffixDefinition("AF08",7,P,"번개 두른","DamageElement",60,3,8,false,0,3,6,7),
            new AffixDefinition("AF09",8,P,"독을 머금은","DamageElement",60,3,8,false,0,3,6,7),
            new AffixDefinition("AF10",9,P,"그늘진","DamageElement",60,3,8,false,0,3,6,7),
            new AffixDefinition("AF11",10,P,"강인한","PrimaryAttribute",100,10,25,true,0,1,2,3,4,5,6,7),
            new AffixDefinition("AF12",11,P,"날렵한","PrimaryAttribute",100,10,25,true,0,1,2,3,4,5,6,7),
            new AffixDefinition("AF13",12,P,"명석한","PrimaryAttribute",100,10,25,true,0,1,2,3,4,5,6,7),
            new AffixDefinition("AF14",13,S,"의지","Willpower",80,10,25,true,0,1,2,3,4,5,6,7),
            new AffixDefinition("AF15",14,P,"완전한","PrimaryAttribute",40,3,8,true,6,7),
            new AffixDefinition("AF16",15,S,"정밀함","CritChance",50,2,4,false,0,3,6,7),
            new AffixDefinition("AF17",16,S,"처형","CritDamage",50,5,12,false,0,3,6,7),
            new AffixDefinition("AF18",17,S,"공세","AttackSpeed",60,3,8,false,0,3,7),
            new AffixDefinition("AF19",18,S,"집중","Cooldown",45,3,6,false,1,3,6),
            new AffixDefinition("AF20",19,S,"절약","ResourceCost",60,3,6,false,1,5,6),
            new AffixDefinition("AF21",20,S,"순환","ResourceRegen",80,1,3,true,1,5,7),
            new AffixDefinition("AF22",21,S,"바람","MoveSpeed",80,3,6,false,4,6),
            new AffixDefinition("AF23",22,S,"회수","PickupRadius",80,.5f,1.5f,false,4,5,7),
            new AffixDefinition("AF24",23,S,"치유","HealingReceived",80,5,10,false,2,5,6,7),
            // The rest of Diablo IV's sheet. Every one of these carries a lighter weight than the
            // affixes that came before it, so widening the pool does not push the older lines out.
            new AffixDefinition("AF25",24,S,"간파","VulnerableDamage",55,5,12,false,0,3,6,7),
            new AffixDefinition("AF26",25,S,"압도","OverpowerDamage",55,8,20,false,0,3,6,7),
            new AffixDefinition("AF27",26,S,"행운","LuckyHit",50,3,8,false,0,3,7),
            new AffixDefinition("AF28",27,S,"회피","Dodge",55,2,5,false,2,4),
            new AffixDefinition("AF29",28,S,"방벽","BlockChance",45,5,12,false,2,3),
            new AffixDefinition("AF30",29,S,"요새","BlockReduction",45,4,10,false,2,5),
            new AffixDefinition("AF31",30,P,"가시 돋친","Thorns",50,8,20,true,2,3,7),
            new AffixDefinition("AF32",31,S,"흡혈","LifeSteal",40,1,3,false,0,7),
            new AffixDefinition("AF33",32,P,"불굴의","DamageReduction",50,2,5,false,1,2,6),
            new AffixDefinition("AF34",33,P,"맞서는","DamageReduction",50,3,7,false,1,2),
            new AffixDefinition("AF35",34,P,"가로막는","DamageReduction",50,3,7,false,1,2),
            new AffixDefinition("AF36",35,P,"끈질긴","DamageReduction",50,4,10,false,2,5),
            new AffixDefinition("AF37",41,P,"철갑의","DamageReduction",50,2,5,false,2,4),
            new AffixDefinition("AF38",36,P,"화염 견디는","ElementResistance",55,10,25,true,1,2,5,6,7),
            new AffixDefinition("AF39",37,P,"한기 견디는","ElementResistance",55,10,25,true,1,2,5,6,7),
            new AffixDefinition("AF40",38,P,"뇌전 견디는","ElementResistance",55,10,25,true,1,2,5,6,7),
            new AffixDefinition("AF41",39,P,"독 견디는","ElementResistance",55,10,25,true,1,2,5,6,7),
            new AffixDefinition("AF42",40,P,"어둠 견디는","ElementResistance",55,10,25,true,1,2,5,6,7),
            new AffixDefinition("AF43",42,S,"심원","MaximumResource",55,5,12,true,1,5,7),
            new AffixDefinition("AF44",43,S,"재생","LifeRegeneration",55,1,4,true,2,5,6),
            new AffixDefinition("AF45",44,S,"영약","PotionHealing",50,5,15,false,5,6),
            new AffixDefinition("AF46",45,S,"결계","BarrierGeneration",50,5,12,false,1,6),
            new AffixDefinition("AF47",47,S,"각성","ExperienceGain",35,3,8,false,6,7),
            new AffixDefinition("AF48",48,S,"탐욕","GoldFind",35,5,15,false,5,7),
            new AffixDefinition("AF49",49,P,"육박하는","ConditionalDamage",50,4,10,false,0,3,7),
            new AffixDefinition("AF50",50,P,"겨냥하는","ConditionalDamage",50,4,10,false,0,3,7),
            new AffixDefinition("AF51",51,P,"잠식하는","ConditionalDamage",50,4,10,false,0,6),
            new AffixDefinition("AF52",52,P,"옭아매는","ConditionalDamage",50,4,10,false,0,3),
            new AffixDefinition("AF53",53,P,"마무리하는","ConditionalDamage",50,4,10,false,0,3),
            new AffixDefinition("AF54",54,P,"선제하는","ConditionalDamage",50,4,10,false,0,3)
        });
        public static readonly IReadOnlyList<SetDefinition> Sets=Array.AsReadOnly(new[]{
            new SetDefinition("SW","회오리 감시자",HeroClass.Warrior,"회오리 자원 소모 감소 +15% (합산 상한 50%).","회오리 중 4m 이동하면 다음 도약 착지에 D180% 물리 추가 피해."),
            new SetDefinition("SA","독무덤지기",HeroClass.Ranger,"맹독 덫 반경 +0.5m.","중독된 적에게 관통 사격 적중 시 3초 동안 총 D240% 추가 독 피해."),
            new SetDefinition("SM","겨울의 서약",HeroClass.Mage,"눈보라 자원 소모 감소 +20% (합산 상한 50%).","눈보라에 2초 노출된 적에게 화염구 적중 시 D100% 냉기 폭발. 대상별 3초 간격."),
            new SetDefinition("SWB","낙성의 집행자",HeroClass.Warrior,"도약 쿨타임 감소 +15% (합산 상한 40%).","도약 적중 후 4초 안에 다음 분쇄 일격에 D160% 물리 추가 피해."),
            new SetDefinition("SAB","긴 그림자의 추적자",HeroClass.Ranger,"관통 사격 자원 소모 감소 +15% (합산 상한 50%).","후퇴 후 6초 안의 다음 관통 사격에 0.3초 뒤 D120% 잔영 사격. 최대 5명."),
            new SetDefinition("SMB","공명하는 폭풍",HeroClass.Mage,"연쇄 번개 자원 소모 감소 +15% (합산 상한 50%).","서리 폭발 적중 후 6초간 다음 연쇄 번개 2회가 각각 2번 더 연결됩니다.")
        });
        static readonly UniqueItemDefinition[] legends={
            new UniqueItemDefinition("LW01","소용돌이의 송곳니",0,0,100,"회오리 1초 유지 후 유료 틱에서 1초마다, 3.5m 내 적을 최대 1.5m 끌어옵니다. 벽·몸체 앞에서 멈추며 보스·이동 불가 적은 제외합니다.","W01"),
            new UniqueItemDefinition("LW02","낙성의 발걸음",0,4,100,"도약 착지에 반경 4m, D90% 물리 충격파를 추가합니다.","W02"),
            new UniqueItemDefinition("LW03","고독한 처형",0,3,20,"분쇄 일격이 정확히 한 명을 맞히면 직접 피해 가산 +50%.","W03"),
            new UniqueItemDefinition("LW04","마지막 명령",0,6,10,"피격으로 HP가 30% 초과에서 30% 이하가 되면 장착한 도약의 남은 쿨타임을 초기화합니다. 생존한 경우만 적용하며 내부 대기시간은 30초입니다.","W02"),
            new UniqueItemDefinition("LA01","끝없는 궤적",1,0,100,"관통 사격의 다음 대상마다 피해 +15%p. 최대 +60%p.","A01"),
            new UniqueItemDefinition("LA02","좁혀진 사선",1,3,20,"다중 사격이 20도 안으로 좁아집니다. 같은 적에게 최대 3발 적중.","A02"),
            new UniqueItemDefinition("LA03","독사의 탈피",1,4,100,"후퇴 도약 출발점에 덫을 남깁니다. 0.5초 무장, 최대 8초 대기, 발동 후 3초 피해. 맹독 덫 장착이 필요하며 직접 덫과 합쳐 최대 2개입니다.","A03"),
            new UniqueItemDefinition("LA04","검은 전염",1,6,10,"중독된 대상의 그림자 화살 추가 피해가 주변으로 전염됩니다. 연쇄 재발동 없음.","A06"),
            new UniqueItemDefinition("LM01","겨울의 발자취",2,0,100,"눈보라를 위치 고정 또는 목표 추적으로 설정합니다. 추적은 초당 1m, 총 4m까지 이동하며 겹친 피해는 강한 값만 적용합니다.","M02"),
            new UniqueItemDefinition("LM02","되울림의 잿불",2,3,100,"화염구 착탄 0.4초 뒤 반경 3.5m에 D70% 추가 화염 폭발.","M01"),
            new UniqueItemDefinition("LM03","환류의 매듭",2,5,20,"원소 보호막이 생성 당시 최대 HP의 15%만큼 피해를 흡수하면 자원 20을 회복합니다. 보호막마다 1회, 내부 대기시간 4초입니다.","M05"),
            new UniqueItemDefinition("LM04","종말의 회로",2,6,10,"연쇄 번개가 같은 적을 최대 2회 방문하며 두 번째 피해는 절반입니다. 총 타격 수는 유지하고 직전 대상은 다시 방문하지 않습니다.","M03"),
            new UniqueItemDefinition("LC01","파수꾼의 고리",-1,7,100,"획득 반경 +2.5m (최대 6m). 비전투 상태에서 전리품·상자를 회수하러 이동하는 동안 이동속도 +15%를 얻습니다."),
            new UniqueItemDefinition("LC02","절제의 서약",-1,6,100,"같은 적에게 기본공격 3회 적중 후, 5초 안의 다음 유료 스킬 비용 감소 +50%를 준비합니다. 1충전이며 내부 대기시간은 4초입니다."),
            new UniqueItemDefinition("LC03","꺼지지 않는 심장",-1,5,100,"HP 20% 이하에서 물약 사용 시 최대 HP 25% 보호막을 3초 얻습니다. 20초 간격.")
        };
        public static readonly IReadOnlyList<UniqueItemDefinition> Uniques=Array.AsReadOnly(BuildUniques());
        static readonly Dictionary<string,ItemBaseDefinition> baseById=Bases.ToDictionary(x=>x.id,StringComparer.Ordinal);
        static readonly Dictionary<string,AffixDefinition> affixById=Affixes.ToDictionary(x=>x.id,StringComparer.Ordinal);
        static readonly Dictionary<string,UniqueItemDefinition> uniqueById=Uniques.ToDictionary(x=>x.id,StringComparer.Ordinal);
        static UniqueItemDefinition[] BuildUniques()
        {
            var result=legends.ToList();
            result.AddRange(LegendaryPowers.All.Select(p=>p.Item));
            foreach(var set in Sets)for(int slot=1;slot<=4;slot++)
            {
                string part=new[]{"","관","갑옷","손아귀","걸음"}[slot];
                result.Add(new UniqueItemDefinition(set.id+slot,set.name+"의 "+part,(int)set.heroClass,slot,100,"2세트: "+set.two+"\n4세트: "+set.four,"",set.id,set,part));
            }
            return result.ToArray();
        }
        public static ItemBaseDefinition Base(string id)=>baseById.TryGetValue(id??"",out var d)?d:throw new InvalidOperationException(Loc.F("알 수 없는 베이스 ID: {0}", id));
        public static ItemBaseDefinition Base(Item item)=>item.contentVersion>0?Base(item.baseId):Bases.Single(b=>b.legacyIndex==item.baseIndex);
        public static AffixDefinition Affix(string id)=>affixById.TryGetValue(id??"",out var d)?d:throw new InvalidOperationException(Loc.F("알 수 없는 접사 ID: {0}", id));
        public static UniqueItemDefinition Unique(string id)=>uniqueById.TryGetValue(id??"",out var d)?d:null;
        public static int AtlasIndex(Item item)=>Math.Min(23,Base(item).legacyIndex);
        public static float MainValue(Item item)=>GearEnhancement.Value(item);
        public static string Name(Item item)
        {
            var unique=Unique(item.special);if(unique!=null)return ItemQuality.Name(item,unique.Name);
            string result=Loc.T(Base(item).name);
            var p=item.rolls.Where(r=>r.side==P).OrderByDescending(r=>r.rollBasisPoints).ThenBy(r=>r.affixId,StringComparer.Ordinal).FirstOrDefault();
            var s=item.rolls.Where(r=>r.side==S).OrderByDescending(r=>r.rollBasisPoints).ThenBy(r=>r.affixId,StringComparer.Ordinal).FirstOrDefault();
            if(p!=null)result=Loc.T(Affix(p.affixId).phrase)+" "+result;
            if(s!=null)result+=" · "+Loc.T(Affix(s.affixId).phrase);
            return ItemQuality.Name(item,result);
        }
        public static void Upgrade(Item item,HeroClass ownerClass)
        {
            if(item==null)throw new InvalidOperationException("빈 장비 데이터");
            if(item.contentVersion>0){Validate(item);return;}
            if(item.affixes==null||item.values==null||item.affixes.Count!=item.values.Count)throw new InvalidOperationException("기존 접사와 수치 개수가 다릅니다.");
            item.baseId=Base(item).id;item.lootClass=Base(item).heroClass>=0?(HeroClass)Base(item).heroClass:Unique(item.special)?.heroClass>=0?(HeroClass)Unique(item.special).heroClass:ownerClass;
            item.rolls=new List<AffixRoll>();
            for(int n=0;n<item.affixes.Count;n++)
            {
                var def=Affixes.Single(a=>a.stat==item.affixes[n]);float value=item.values[n];
                int q=Mathf.Clamp(Mathf.RoundToInt((value/def.Scale(item.level)-def.min)/(def.max-def.min)*10000),0,10000);
                item.rolls.Add(new AffixRoll{slotId=item.id+":a"+n,affixId=def.id,side=def.side,tierId=ItemGenerator.Tier(q),rollBasisPoints=q,value=value,legacyRoll=true});
            }
            item.rerollSlotId=item.rerollIndex>=0&&item.rerollIndex<item.rolls.Count?item.rolls[item.rerollIndex].slotId:"";
            item.investedMaterials=20*((1<<Mathf.Clamp(item.enhancement,0,5))-1);
            item.contentVersion=Version;Validate(item);
            item.affixes.Clear();item.values.Clear();item.rerollIndex=-1;
        }
        public static void Validate(Item item)
        {
            GemCatalog.ValidateSockets(item);
            var b=Base(item.baseId);
            if(string.IsNullOrWhiteSpace(item.id)||item.slot!=b.slot||item.level<1||item.rarity<0||item.rarity>3||item.enhancement<0||item.enhancement>GearEnhancement.Maximum||item.investedMaterials<0||item.rerolls<0||item.contentVersion<1||item.contentVersion>Version)
                throw new InvalidOperationException(Loc.F("장비 기본 데이터가 올바르지 않습니다: {0}", item.id));
            if(!string.IsNullOrEmpty(item.special)&&(Unique(item.special)==null||Unique(item.special).slot!=item.slot||item.rarity!=3))throw new InvalidOperationException(Loc.F("고유 장비 참조 오류: {0}", item.special));
            if(item.rolls==null||item.rolls.Count>4||item.rolls.Any(r=>r==null)||item.rolls.Select(r=>r.slotId).Distinct().Count()!=item.rolls.Count)throw new InvalidOperationException(Loc.F("접사 슬롯 오류: {0}", item.id));
            ItemQuality.Validate(item);
            if(!item.rolls.Any(r=>r.legacyRoll)&&(item.rarity==0&&item.rolls.Count!=0||item.rarity==1&&(item.rolls.Count<1||item.rolls.Count>2)||item.rarity==2&&item.rolls.Count!=3||item.rarity==3&&item.rolls.Count!=4))throw new InvalidOperationException(Loc.F("등급별 접사 개수 오류: {0}", item.id));
            var groups=new HashSet<string>();
            foreach(var r in item.rolls)
            {
                var d=Affix(r.affixId);
                if(string.IsNullOrEmpty(r.slotId)||r.side!=d.side||!d.Allows(item.slot)||!float.IsFinite(r.value)||r.value<0||r.rollBasisPoints<0||r.rollBasisPoints>10000||r.tierId!=ItemGenerator.Tier(r.rollBasisPoints))throw new InvalidOperationException(Loc.F("접사 데이터 오류: {0}", r.affixId));
                if(!groups.Add(d.group)&&!r.legacyRoll&&!item.rolls.Any(x=>x.legacyRoll))throw new InvalidOperationException(Loc.F("중복 접사 그룹: {0}", d.group));
            }
            if(!string.IsNullOrEmpty(item.rerollSlotId)&&!item.rolls.Any(r=>r.slotId==item.rerollSlotId))throw new InvalidOperationException("재설정 슬롯 참조 오류");
        }
    }

    public static class ItemGenerator
    {
        public static string Tier(int q)=>q<5000?"T3":q<8500?"T2":"T1";
        static int Integer(ref uint rng,int min,int max)
        {
            uint range=(uint)(max-min),limit=uint.MaxValue-uint.MaxValue%range,n;
            do{n=RandomStream.Next(ref rng)-1;}while(n>=limit);
            return min+(int)(n%range);
        }
        struct Allocation { public int prefixes,suffixes,weight;public Allocation(int p,int s,int w){prefixes=p;suffixes=s;weight=w;} }
        static readonly Allocation[][] allocations={new[]{new Allocation(0,0,1)},new[]{new Allocation(1,0,1),new Allocation(0,1,1)},new[]{new Allocation(1,1,1)},new[]{new Allocation(2,1,50),new Allocation(1,2,50)},new[]{new Allocation(2,2,60),new Allocation(3,1,20),new Allocation(1,3,20)}};
        static readonly List<AffixDefinition>[] affixPools=Enumerable.Range(0,8).Select(slot=>ItemCatalog.Affixes.Where(a=>a.Allows(slot)).ToList()).ToArray();
        static ItemGenerator()
        {
            if(ItemCatalog.Affixes.GroupBy(a=>a.group).Any(g=>g.Select(a=>a.side).Distinct().Count()!=1))throw new InvalidOperationException("중복 그룹은 접두·접미 양쪽에 걸칠 수 없습니다.");
        }
        static bool CanFill(List<AffixDefinition> pool,HashSet<string> used,int p,int s)
        {
            // Groups belong to one side, so group counts prove a complete assignment exists.
            return pool.Where(a=>a.side==AffixSide.Prefix&&!used.Contains(a.group)).Select(a=>a.group).Distinct().Count()>=p
                &&pool.Where(a=>a.side==AffixSide.Suffix&&!used.Contains(a.group)).Select(a=>a.group).Distinct().Count()>=s;
        }
        static T Pick<T>(IList<T> pool,Func<T,int> weight,ref uint rng)
        {
            int total=pool.Sum(weight);if(total<=0)throw new InvalidOperationException("장비 생성 후보가 없습니다.");
            int n=Integer(ref rng,0,total);foreach(var x in pool){n-=weight(x);if(n<0)return x;}throw new InvalidOperationException("가중치 추첨 오류");
        }
        public static AffixRoll Roll(AffixDefinition def,int level,string slotId,ref uint rng,bool awakened=false)
        {
            int tier=Integer(ref rng,0,100),low=level<10?70:level<30?55:45,mid=level<10?95:level<30?90:85;
            int q=tier<low?Integer(ref rng,awakened?4000:0,5000):tier<mid?Integer(ref rng,5000,8500):Integer(ref rng,8500,10001);
            return new AffixRoll{slotId=slotId,affixId=def.id,side=def.side,tierId=Tier(q),rollBasisPoints=q,value=def.Value(level,q)};
        }
        public static Item Create(HeroClass c,int slot,int rarity,int level,ref uint rng,string id=null,string uniqueId=null,int riftStage=0)
        {
            if((int)c<0||(int)c>2||slot<0||slot>7||rarity<0||rarity>3||level<1)throw new ArgumentOutOfRangeException();
            level=Math.Min(ItemQuality.MaximumItemLevel,level);
            uint next=rng;
            var bases=ItemCatalog.Bases.Where(b=>b.Fits(c,slot)&&(rarity<3||!EquipmentSlots.IsOffhand(EquipmentSlots.Kind(b.id)))).ToList();var b=Pick(bases,_=>1,ref next);
            var item=new Item{id=id??Guid.NewGuid().ToString("N"),baseId=b.id,baseIndex=b.legacyIndex,slot=slot,rarity=rarity,level=level,lootClass=c,contentVersion=ItemCatalog.Version};
            if(rarity==3)
            {
                var pool=ItemCatalog.Uniques.Where(d=>d.Fits(c,slot)).ToList();
                var d=uniqueId==null?Pick(pool,x=>x.weight,ref next):pool.SingleOrDefault(x=>x.id==uniqueId);
                if(d==null)throw new InvalidOperationException("부위·직업에 맞지 않는 고유 장비입니다.");item.special=d.id;
            }
            else if(uniqueId!=null)throw new InvalidOperationException("고유 장비는 전설 등급이어야 합니다.");
            item.awakened=ItemQuality.RollAwakening(rarity,riftStage,ref next);
            int count=rarity==0?0:rarity==1?Integer(ref next,1,3):rarity==2?3:4;
            var available=affixPools[slot];var used=new HashSet<string>();
            var possible=allocations[count].Where(a=>CanFill(available,used,a.prefixes,a.suffixes)).ToList();var allocation=Pick(possible,x=>x.weight,ref next);
            int p=allocation.prefixes,s=allocation.suffixes;
            for(int n=0;n<count;n++)
            {
                var side=p>0?AffixSide.Prefix:AffixSide.Suffix;if(p>0)p--;else s--;
                // Every group belongs to exactly one side. Any legal choice removes exactly one
                // group from that side, so the prevalidated allocation remains fillable.
                var pool=available.Where(a=>a.side==side&&!used.Contains(a.group)).ToList();
                var def=Pick(pool,a=>a.Weight(c),ref next);used.Add(def.group);var roll=Roll(def,level,item.id+":a"+n,ref next,item.awakened);
                ItemQuality.RollGreater(item,roll,ref next);item.rolls.Add(roll);
            }
            item.name=item.DisplayName;ItemCatalog.Validate(item);rng=next;return item;
        }
        public static List<AffixDefinition> RerollPool(Item item,string slotId)
        {
            var current=item.rolls.FirstOrDefault(r=>r.slotId==slotId);if(current==null)return new List<AffixDefinition>();
            var groups=new HashSet<string>(item.rolls.Where(r=>r.slotId!=slotId).Select(r=>ItemCatalog.Affix(r.affixId).group));
            return ItemCatalog.Affixes.Where(a=>a.side==current.side&&a.Allows(item.slot)&&!groups.Contains(a.group)).ToList();
        }
        public static AffixRoll Reroll(Item item,string slotId,ref uint rng)
        {var pool=RerollPool(item,slotId);var def=Pick(pool,a=>a.Weight(item.lootClass),ref rng);return Roll(def,item.level,slotId,ref rng,item.awakened);}
    }
}
