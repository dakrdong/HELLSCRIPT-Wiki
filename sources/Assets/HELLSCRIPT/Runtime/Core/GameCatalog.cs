using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hellscript
{
    public enum HeroClass { Warrior, Ranger, Mage }
    public enum RunPhase { Exploring, Boss, Looting, Cleared, Failed }
    public enum TargetMode { Nearest, Elite, Support, LowHealth, Dense }
    public enum MovementMode { Approach, Orbit, KeepDistance, Stand, Retreat }
    public enum BagPolicy { Portal, Replace, Ignore }
    public enum SkillKind { Whirlwind, Leap, Crush, Slam, Shield, Shout, Pierce, Multi, Trap, Retreat, Mark, Shadow, Fireball, Blizzard, Chain, Teleport, Nova }
    public enum ConditionKind { Always, HealthBelow, ResourceBelow, EnemiesNear, TargetFar, TargetNear, ElitePresent, Danger, NoShield, TargetPoisoned }

    [Serializable]
    public sealed class SkillDefinition
    {
        public string id, name, description;
        public HeroClass heroClass;
        public SkillKind kind;
        public int unlock, icon;
        public float cooldown, cost, coefficient, range, radius, duration;
        public SkillDefinition(string id, string name, HeroClass heroClass, SkillKind kind, int unlock,
            float cd, float cost, float damage, float range, float radius, float duration, int icon, string description)
        { this.id=id; this.name=name; this.heroClass=heroClass; this.kind=kind; this.unlock=unlock; cooldown=cd; this.cost=cost;
          coefficient=damage; this.range=range; this.radius=radius; this.duration=duration; this.icon=icon; this.description=description; }
    }

    [Serializable]
    public sealed class Rule
    {
        public int schema;
        public string id="";
        public RuleAction action;
        public bool always,overrideMovement;
        public RuleTarget target;
        public PositionPurpose positionPurpose;
        public AreaAim areaAim;
        public BlizzardMode blizzardMode;
        public MovementMode movement;
        public float distance=2,targetRadius=3;
        public List<ConditionGroup> groups=new List<ConditionGroup>();
        public int skill;
        public bool enabled=true;
        public ConditionKind condition;
        public float threshold;
        public bool escape;
        public Rule(int skill, ConditionKind condition=ConditionKind.Always, float threshold=0, bool escape=false)
        { this.skill=skill; this.condition=condition; this.threshold=threshold; this.escape=escape; }
        public Rule Copy() => JsonUtility.FromJson<Rule>(JsonUtility.ToJson(this));
    }

    [Serializable]
    public sealed class BuildConfig
    {
        public bool emptySlot;
        public int ruleSchema;
        public List<int> activeSkills=new List<int>();
        public string name="생존 우선", version="1";
        public TargetMode target=TargetMode.Nearest;
        public MovementMode movement=MovementMode.Orbit;
        public float distance=2, potionThreshold=40;
        public bool clockwise=true, autoRepeat, advanceOnWin;
        public int stopAfterFailures=3;
        public BagPolicy bagPolicy=BagPolicy.Portal;
        public int minimumRarity, pursueRarity=3;
        public ExplorationPolicy exploration=ExplorationPolicy.RightWall;
        public bool openChests=true, commonChests=true, sealedChests=true, chestsAfterBoss;
        public float chestDetour=12;
        public bool useShrines=true, guideShrines=true, resolveShrines=true, allowCursedChests;
        public float shrineDetour=12, cursedMinimumTime=90;
        public List<Rule> rules=new List<Rule>();
        public List<string> equipmentIds=new List<string>();
        public int[] passives={0,1,2};
        public BuildConfig Copy() => JsonUtility.FromJson<BuildConfig>(JsonUtility.ToJson(this));
    }

    [Serializable]
    public sealed class Item
    {
        public string id, name, special="";
        public string baseId="", rerollSlotId="";
        public int contentVersion, investedMaterials;
        public bool awakened;
        public int masterwork,masterworkInvestedMaterials;
        public List<string> masterworkLines=new List<string>();
        public long acquiredOrder;
        public bool reviewed;
        public HeroClass lootClass;
        public List<AffixRoll> rolls=new List<AffixRoll>();
        public List<SocketState> sockets=new List<SocketState>();
        public int baseIndex, slot, rarity, level=1, enhancement;
        public bool locked, equipped;
        public int rerollIndex=-1, rerolls;
        public List<int> affixes=new List<int>();
        public List<float> values=new List<float>();
        public int Price => new[]{20,40,80,200}[Mathf.Clamp(rarity,0,3)]*level;
        // The index arrays are read only by the schema-1 migration; new items store stable IDs.
        public float Value(int index)
        {
            float v=0;
            if(contentVersion>0){foreach(var roll in rolls)if(ItemCatalog.Affix(roll.affixId).stat==index)v+=ItemQuality.AffixValue(this,roll);}
            else for(int i=0;i<affixes.Count;i++)if(affixes[i]==index)v+=values[i];
            return v;
        }
        public int RequiredLevel => Mathf.Min(30,(level+1)/2);
        public string DisplayName => contentVersion>0?ItemCatalog.Name(this):name;
    }

    [Serializable]
    public sealed class HeroSave
    {
        public int legacyPassiveSlots;
        public PotionInventory potions=new PotionInventory();
        public TrainingComparisonRecord trainingComparison;
        public string id;
        public HeroClass heroClass;
        public int level=1, xp, highestClear, capacity=50;
        public string lastRiftFingerprint="";
        public int lastRiftBoss=-1;
        public BuildConfig build;
        public HuntEdictV2Document edict;
        // Opt-in. Off keeps the legacy rule engine in sole control, which is what every existing save expects.
        public bool useEdict;
        public HeroGuide guide=new HeroGuide();
        public List<BuildConfig> presets=new List<BuildConfig>();
        public List<Item> inventory=new List<Item>();
        public List<int> firstClears=new List<int>();
    }

    [Serializable]
    public sealed class AccountSave
    {
        public int gemCapacity=GemInventory.DefaultCapacity;
        public List<GemStack> gems=new List<GemStack>();
        public RuneGrowthState runes=new RuneGrowthState();
        public ContentUnlockState contentUnlocks=new ContentUnlockState();
        public AccountGuide guide=new AccountGuide();
        public int schema=2, selectedHero, gold, materials, sweepCount;
        public int[] cores=new int[8];
        public string sweepDay="";
        public long lastSeenUtc;
        public long itemSequence;
        public float speed=1;
        public List<HeroSave> heroes=new List<HeroSave>();
        public List<Item> warehouse=new List<Item>();
        public List<RunRecord> records=new List<RunRecord>();
        public List<string> receipts=new List<string>();
        public List<EconomyReceipt> transactions=new List<EconomyReceipt>();
        public RunState suspendedRun;
        public RepeatHuntSession repeatHunt;
        public HeroSave Hero => heroes[Mathf.Clamp(selectedHero,0,heroes.Count-1)];
    }

    [Serializable]
    public sealed class EconomyReceipt
    {
        public string requestId, operation;
        public long committedUtc;
    }

    [Serializable]
    public sealed class RunRecord
    {
        public CombatReview review;
        public string id, hero, result;
        public int stage, kills, loot;
        public int chestsOpened, chestsTotal;
        public string mapFingerprint;
        public RiftObjectiveRecord objective;
        public float simulationSeconds, realSeconds, damageDealt;
        public List<string> logs=new List<string>();
    }

    [Serializable]
    public sealed class EnemyState
    {
        public EnemyBrain brain=new EnemyBrain();
        public List<StatusEffect> statuses=new List<StatusEffect>();
        public BossControlState bossControl=new BossControlState();
        public int id, kind, room, elite=-1;
        public int group=-1, elitePartner=-1,summonerId=-1;
        public string eventId="";
        public List<int> eliteTraits=new List<int>();
        public float lastSeenTime=-20;
        public Vector2 lastSeenPosition;
        public Vector2 position, aim;
        public float health, maxHealth, attack, speed, cooldown, windup, stun, slow, poison, poisonDamage, mark, exposure;
        public bool boss, add, dead,pendingDeath;
        public bool runeRewardRolled;
        public int pattern;
        public float setPoisonTime, setPoisonTick, frostMarkTime, frostCooldown;
        public float trapTick=.5f,blizzardTick=.5f,freeze,root;
        public int setPoisonRoot,setPoisonInstance;
        public int frostMarkId;
        public string setPoisonCasterId,frostCasterId;
        public DamageSnapshot setPoisonSnapshot;
    }
    [Serializable]
    public sealed class GroundEffect
    {
        public int rootCastId,element;
        public string definitionId,casterId;
        public float moved,exposureTick,createdAt;
        public bool followsTarget;
        public int id, kind;
        public Vector2 position, end;
        public float radius, delay, duration, tick, damage;
        public bool hostile;
        public bool periodic;
        // Legacy hostile ground kind 4 is a repeating zone even on its final saved tick.
        public bool PeriodicIncoming=>hostile&&(periodic||duration>.11f||kind==4);
        public DamageSnapshot snapshot;
    }
    [Serializable]
    public sealed class DamageSnapshot
    {
        public float[] runeSkillPower;
        public float[] runeBonuses;
        public float damage, bonus, crit, critDamage;
        public int level;
        public float[] elements;
        public bool[] passives;
        public bool crowdCaptured,crowdQualified;
    }
    [Serializable]
    public sealed class CombatEffectState
    {
        public float lc02Charge,ap05Cooldown,ap05Movement;
        public bool ap05Ready;
        public float lm03Cooldown,lw04Cooldown,lc03Cooldown;
        public float lastLog=-10,lastPull=-1000,lastHeal=-1000,leapDefense,moveBuff,elementBuff,lastElementTime,procCooldown;
        public int lastElement=-1,basicCount,lastBasic=-1;
        public int[] elementHitTicks={-1000000,-1000000,-1000000,-1000000,-1000000,-1000000};
        public bool reducedNext;
        public float whirlwindDistance,whirlwindCharge,crushCharge,pierceCharge,chainCharge;
        public int chainCharges;
    }
    [Serializable]
    public sealed class DropState { public int id; public Vector2 position; public Item item; public bool ignored, claimed, discovered; }
    [Serializable]
    public sealed class RunState
    {
        public PotionRuntimeState potions=new PotionRuntimeState();
        public float[] cooldownTotals=new float[18];
        public uint gemRng;
        public int gemsCollected;
        public List<RiftResourceDrop> resources=new List<RiftResourceDrop>();
        public long earnedGold;
        public bool limitedLoot;
        public EdictResponseState edictResponse;
        public EdictLandingPreference edictLandingPreference;
        public EdictTargetState edictTarget;
        public CombatStatistics statistics;
        public int runesAwarded;
        public int pendingExperience;
        public List<GrowthEvent> growthEvents=new List<GrowthEvent>();
        public float enemySlowTime;
        public List<EnemyHazard> enemyHazards=new List<EnemyHazard>();
        public List<EnemyCorpse> enemyCorpses=new List<EnemyCorpse>();
        public List<EnemyCombatEvent> enemyEvents=new List<EnemyCombatEvent>();
        public int effectVersion,nextDamageId=1;
        public List<ShieldEffect> shields=new List<ShieldEffect>();
        public List<EffectEvent> effectEvents=new List<EffectEvent>();
        public List<DamageEvent> damageEvents=new List<DamageEvent>();
        public List<ProcReceipt> procHits=new List<ProcReceipt>();
        public float noEnemySince,targetSelectedAt;
        public string targetRuleId="";
        public List<DamageReceived> receivedDamage=new List<DamageReceived>();
        public List<RuleDecision> decisions=new List<RuleDecision>();
        public int movementRule=-1;
        public RuleAction movementAction=RuleAction.Explore;
        public int movementDrop=-1;
        public int combatVersion;
        public HeroActionState heroAction=new HeroActionState();
        public EdictEngagementState edictEngagement;
        public EdictRangerState edictRanger;
        public int shotApproachTarget=-1;
        public Vector2 shotApproachPosition,shotApproachAim;
        public float shotApproachReview;
        public List<CombatActionEvent> actionEvents=new List<CombatActionEvent>();
        public List<CombatProjectile> projectiles=new List<CombatProjectile>();
        public List<ProjectileGroup> projectileGroups=new List<ProjectileGroup>();
        public List<CombatTrap> traps=new List<CombatTrap>();
        public float[] lastSkillStarts,lastSkillCompletions;
        public string id, heroId;
        public uint rng, rewardRng;
        // The stream the character sheet's own rolls draw from. Kept apart from rng so that
        // dodge, block, overpower and lucky hit do not move the sequence saved runs replay from.
        public uint sheetRng;
        public RiftLayout layout;
        public RiftDiscovery discovery;
        [NonSerialized] public RiftVisibility visibility;
        public RiftExplorationState exploration=new RiftExplorationState();
        public float lastDamageTime=-20, bossWait;
        public float lastAttackTime=-20,lastOutgoingDamageTime=-20,guideShrineTime,resolveShrineTime;
        public string navigationError="";
        public int stage, theme, training=-1, kills, meter, lootCount, nextId=1, bossId=-1;
        public bool trainingUsesOwnedHero;
        public RunPhase phase;
        public float time, realTime, health, resource=100, shield, shieldTime, potionCd, actionCd, channelTime, channelTick,
            shoutTime, shadowTime, dealt, moveDistance, decisionTime, saveTime, portalCast;
        public int targetId=-1, exploreRoom, shadowCharges;
        public bool paused, portal, bossRewarded,heroDeathRecorded;
        public Vector2 position, destination;
        public float[] cooldowns=new float[18];
        public List<EnemyState> enemies=new List<EnemyState>();
        public List<GroundEffect> effects=new List<GroundEffect>();
        public List<DropState> drops=new List<DropState>();
        public List<string> logs=new List<string>();
        public List<int> visited=new List<int>();
        public BuildConfig build;
        public string action="탐색 중";
        public int activeSkill=-1;
        public CombatEffectState itemEffects=new CombatEffectState();
    }

    public sealed class GameCatalog : ScriptableObject
    {
        public List<SkillDefinition> skills=new List<SkillDefinition>();
        public string[] classNames={"전사","궁수","마법사"};
        public static readonly string[] Slots={"무기","머리","몸통","손","발","허리","목걸이","반지"};
        public static readonly string[] Rarities={"일반","마법","희귀","전설"};
        public static readonly string[] EnemyNames={"쇠사슬 시체","무덤 사냥개","해골 궁병","역병 시종","장송 사제","부푼 순례자","철갑 망령","광신 돌격병","흑철 석궁병","잿불 사술사","종지기","균열 파편체"};
        public static readonly string[] BossNames={"묘지의 집행자","종말의 합창자","균열의 포식자"};
        public static readonly string[] BaseNames={"녹슨 도검","강철 대검","묵철 도끼","사냥 활","전쟁 활","중형 쇠뇌","재의 지팡이","봉인 지팡이","흑요석 지팡이","천 두건","철 투구","가죽 외투","철 갑옷","천 장갑","철 장갑","가죽 장화","철 장화","직물 허리띠","철 버클 허리띠","뼈 목걸이","은 목걸이","봉인 목걸이","철 반지","은 반지"};
        public static readonly float[] BaseValues={17,20,23,17,20,23,17,20,23,12,20,24,40,8,14,8,14,10,16,20,25,30,5,5};
        public static readonly int[] BaseSlots={0,0,0,0,0,0,0,0,0,1,1,2,2,3,3,4,4,5,5,6,6,6,7,7};
        public static readonly string[] Passives={"군중 속으로","끝나지 않는 회전","착지 자세","피의 회복","단단한 의지","처형자의 눈","긴 사거리","탈출의 발걸음","꿰뚫는 시선","덫 사냥꾼","절약된 집중","마무리 사격","밀집 연소","깊은 한기","번개의 사슬","안정된 결계","마나 순환","원소 교차"};
        public static readonly string[] PassiveDescriptions={
            "3m 안에 살아 있는 적이 3명 이상이면 가산 피해 +15%를 적용합니다.",
            "회오리를 2초 유지하면 비용 감소 +20%를 적용합니다. 중단하면 해제됩니다.",
            "도약 착지 후 2초 동안 받는 피해가 15% 감소합니다.",
            "처치 시 최대 HP의 2%를 회복합니다. 한 번 발동한 뒤 1초 동안 기다립니다.",
            "보호막 생성량 증가에 +20%p를 더합니다. 의지의 증가량과 합산합니다.",
            "HP가 30% 이하인 적에게 가산 피해 +20%를 적용합니다.",
            "적중할 때 대상과 7m 이상 떨어져 있으면 가산 피해 +15%를 적용합니다.",
            "후퇴 도약 착지 후 2초 동안 이동속도 증가에 +20%p를 더합니다.",
            "관통 사격의 최대 대상이 5명에서 7명으로 늘어납니다.",
            "직접 설치한 덫과 후퇴 덫의 속박 시간이 25% 늘어납니다.",
            "1초 보행 후 다음 유료 스킬의 비용이 25% 감소합니다. 사용 후 대기시간은 4초입니다.",
            "HP가 30% 이하인 적에게 치명 확률 +10%p를 적용합니다. 최종 상한은 75%입니다.",
            "화염구 폭발에 적이 3명 이상 포함되면 직접 폭발의 가산 피해 +15%를 적용합니다.",
            "눈보라의 둔화가 35%에서 55%로 강해집니다. 다른 둔화와는 가장 강한 값만 적용합니다.",
            "연쇄 번개의 총 타격 수가 4회에서 5회로 늘어납니다.",
            "보호막 생성량 증가에 +20%p를 더합니다. 의지의 증가량과 합산합니다.",
            "초당 자원 회복이 20% 증가합니다. 평면 자원 회복 옵션에도 적용됩니다.",
            "최근 4초 안에 서로 다른 두 속성으로 피해를 주면 4초간 가산 피해 +10%를 얻습니다. 중첩되지 않습니다."};
        public void Populate()
        {
            skills=new List<SkillDefinition>{
                new SkillDefinition("W01","회오리",HeroClass.Warrior,SkillKind.Whirlwind,1,0,6,.5f,2.5f,2.5f,0,0,"주변 적에게 0.25초마다 피해. 이동하며 유지합니다."),
                new SkillDefinition("W02","도약 내려찍기",HeroClass.Warrior,SkillKind.Leap,3,8,0,1.8f,8,2.5f,.6f,1,"유효한 위치로 도약해 착지 충격을 줍니다."),
                new SkillDefinition("W03","분쇄 일격",HeroClass.Warrior,SkillKind.Crush,6,0,25,2.2f,3,3,.65f,2,"전방의 적에게 강한 물리 피해를 줍니다."),
                new SkillDefinition("W04","지면 강타",HeroClass.Warrior,SkillKind.Slam,10,10,0,1.2f,3,3,.35f,3,"주변 적에게 피해와 1.5초 기절을 줍니다."),
                new SkillDefinition("W05","철벽",HeroClass.Warrior,SkillKind.Shield,15,14,0,0,0,0,4,4,"최대 HP 30% 보호막을 4초 유지합니다."),
                new SkillDefinition("W06","전투 함성",HeroClass.Warrior,SkillKind.Shout,20,16,0,0,0,0,6,5,"자원 40 회복, 6초간 피해 +20%."),
                new SkillDefinition("A01","관통 사격",HeroClass.Ranger,SkillKind.Pierce,1,0,20,1.8f,10,.6f,.65f,6,"일직선으로 최대 5명을 관통합니다."),
                new SkillDefinition("A02","다중 사격",HeroClass.Ranger,SkillKind.Multi,3,3,30,.8f,8,8,.65f,7,"60도 부채꼴로 화살 3발을 발사합니다."),
                new SkillDefinition("A03","맹독 덫",HeroClass.Ranger,SkillKind.Trap,6,8,20,.6f,6,3,5,8,"5초 독 장판을 설치합니다. 최대 2개."),
                new SkillDefinition("A04","후퇴 도약",HeroClass.Ranger,SkillKind.Retreat,10,8,0,0,6,0,.5f,9,"적과 거리를 벌리는 위치로 도약합니다."),
                new SkillDefinition("A05","사냥꾼의 표식",HeroClass.Ranger,SkillKind.Mark,15,10,0,0,10,0,10,10,"표식 대상에게 주는 피해 +25%."),
                new SkillDefinition("A06","그림자 화살",HeroClass.Ranger,SkillKind.Shadow,20,12,10,.25f,0,0,8,11,"다음 사격 3회에 암흑 추가 피해."),
                new SkillDefinition("M01","화염구",HeroClass.Mage,SkillKind.Fireball,1,0,25,2.1f,10,2.5f,.7f,12,"착탄 지점에 폭발 피해를 줍니다."),
                new SkillDefinition("M02","눈보라",HeroClass.Mage,SkillKind.Blizzard,3,6,30,.65f,10,3,6,13,"6초간 냉기 피해와 35% 둔화. 최대 2개."),
                new SkillDefinition("M03","연쇄 번개",HeroClass.Mage,SkillKind.Chain,6,2,25,1.1f,10,4,.45f,14,"4명에게 번개를 연결합니다. 다음 타격마다 ×0.8."),
                new SkillDefinition("M04","순간이동",HeroClass.Mage,SkillKind.Teleport,10,9,0,0,8,0,.2f,15,"위험이 낮은 유효한 지점으로 이동합니다."),
                new SkillDefinition("M05","원소 보호막",HeroClass.Mage,SkillKind.Shield,15,14,0,0,0,0,4,16,"최대 HP 35% 보호막을 4초 유지합니다."),
                new SkillDefinition("M06","서리 폭발",HeroClass.Mage,SkillKind.Nova,20,10,20,1,3,3,.35f,17,"주변 적에게 냉기 피해와 1.5초 빙결."),
            };
        }
        public static BuildConfig Preset(HeroClass c,int variant)=>BehaviorPresets.Create(c,variant);
    }
}
