using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    public sealed partial class CombatSimulation
    {
        public const float Step=.05f;
        public readonly RunState State;
        public readonly HeroSave Hero;
        public bool OwnedTraining=>State.training>=0&&State.trainingUsesOwnedHero;
        bool FullSkillTraining=>State.training>=0&&!State.trainingUsesOwnedHero;
        public int EffectiveLevel=>FullSkillTraining?ClassSkills.LevelCap(Hero):Hero.level;
        public float TimeLimit=>OwnedTraining?60:300;
        readonly AccountSave account;
        readonly GameCatalog catalog;
        readonly List<EnemyState> sensed=new List<EnemyState>();
        public event Action<Vector2,Vector2,int,float> Visual;
        CombatEffectState ItemEffects=>State.itemEffects;
        float lastLog {get=>ItemEffects.lastLog;set=>ItemEffects.lastLog=value;}
        float lastPull {get=>ItemEffects.lastPull;set=>ItemEffects.lastPull=value;}
        float lastHeal {get=>ItemEffects.lastHeal;set=>ItemEffects.lastHeal=value;}
        float leapDefense {get=>ItemEffects.leapDefense;set=>ItemEffects.leapDefense=value;}
        float moveBuff {get=>ItemEffects.moveBuff;set=>ItemEffects.moveBuff=value;}
        float elementBuff {get=>ItemEffects.elementBuff;set=>ItemEffects.elementBuff=value;}
        float lastElementTime {get=>ItemEffects.lastElementTime;set=>ItemEffects.lastElementTime=value;}
        float procCooldown {get=>ItemEffects.procCooldown;set=>ItemEffects.procCooldown=value;}
        int lastElement {get=>ItemEffects.lastElement;set=>ItemEffects.lastElement=value;}
        int basicCount {get=>ItemEffects.basicCount;set=>ItemEffects.basicCount=value;}
        int lastBasic {get=>ItemEffects.lastBasic;set=>ItemEffects.lastBasic=value;}
        bool reducedNext {get=>ItemEffects.reducedNext;set=>ItemEffects.reducedNext=value;}
        public CombatSimulation(AccountSave account,GameCatalog catalog,int stage,int training=-1,RunState restore=null,uint? seed=null,bool ownedTraining=false,RiftObjectiveKind? forcedObjective=null)
        {
            if(training>=0&&ownedTraining&&!ContentUnlocks.Has(account,ContentUnlocks.Train))throw new InvalidOperationException(ContentUnlocks.Condition(ContentUnlocks.Train));
            bool owned=restore!=null?restore.training>=0&&restore.trainingUsesOwnedHero:ownedTraining&&training>=0;
            if(owned&&(restore?.training??training)>2)throw new ArgumentOutOfRangeException(nameof(training));
            // Owned training and every stocked training session own their account copy. Keep the
            // pre-inventory developer fixture contract until its hero adopts the new inventory.
            bool copyAccount=owned||(restore?.training??training)>=0&&account.Hero.potions?.version>0;
            this.account=copyAccount?JsonUtility.FromJson<AccountSave>(JsonUtility.ToJson(account)):account;this.catalog=catalog;Hero=this.account.Hero;
            State=restore??new RunState{id=Guid.NewGuid().ToString("N"),heroId=Hero.id,stage=Mathf.Max(1,stage),training=training,
                rng=seed??(uint)(DateTime.UtcNow.Ticks&0xFFFFFFFF),position=RiftMap.Rooms[0]+new Vector2(0,-4),build=Hero.build.Copy()};
            if(restore==null&&owned){State.trainingUsesOwnedHero=true;State.stage=1;State.rng=seed??(731010u+(uint)training);}
            CombatTelemetry.Normalize(State);
            RiftResources.Normalize(State);GemInventory.Normalize(this.account);
            State.itemEffects??=new CombatEffectState();InitializeLegendaryState();
            State.growthEvents??=new List<GrowthEvent>();
            BehaviorRules.Normalize(State.build);
            if(ClassSkillLoadout.IsAbsent(State.build.classSkills))State.build.classSkills=null;
            else State.build.classSkills.ProjectLegacy(State.build,catalog);
            if(State.slotLevels==null)State.slotLevels=(int[])(Hero.slotProgress??new SlotProgress()).levels.Clone();
            var statsHero=JsonUtility.FromJson<HeroSave>(JsonUtility.ToJson(Hero));statsHero.build=State.build;statsHero.slotProgress=new SlotProgress{levels=(int[])State.slotLevels.Clone()};
            Stats=new HeroStats(statsHero,FullSkillTraining,this.account.runes);
            PrepareEdict(restore!=null);
            InitializeClassSkills(restore!=null);
            InitializePotions(restore==null);
            InitializeRift(restore==null,forcedObjective);
            RiftVisibility.Initialize(State,Map,restore!=null);
            if(Hero.heroClass==HeroClass.Mage||Hero.heroClass==HeroClass.Warrior)EnsureShieldEngagement();
            if(restore==null)
            {
                State.health=Stats.hp;State.visited.Add(0);State.exploreRoom=0;
                SpawnDungeon();Log("RUN_START",training>=0?"훈련 시작 · 실제 재화와 성장에 반영하지 않습니다.":Loc.F("균열 {0}단계 진입", stage));
            }
            State.paused=false;
        }
        void SpawnDungeon()
        {
            if(OwnedTraining){SpawnOwnedTraining();return;}
            if(State.training>=0)
            {
                int count=State.training==0?1:8;
                for(int i=0;i<count;i++)SpawnEnemy(0,i%6,RiftMap.Rooms[0]+new Vector2((i%3-1)*3,(i/3)*2),-1);
                if(State.training==2)foreach(var e in State.enemies){e.kind=3;e.cooldown=2;}
                return;
            }
            if(!State.layout.legacy)
            {
                foreach(var spawn in State.layout.spawns)
                {SpawnEnemy(spawn.room,spawn.kind,spawn.position,spawn.elite);var e=State.enemies[State.enemies.Count-1];e.group=spawn.group;e.eliteTraits=new List<int>(spawn.traits);}
                foreach(var spawn in State.layout.spawns)if(spawn.elitePartner>=0)State.enemies[spawn.index].elitePartner=State.enemies[spawn.elitePartner].id;
                foreach(var carrier in State.layout.carriers)carrier.enemyId=State.enemies[carrier.spawn].id;
                SpawnGoldenGoblin();return;
            }
            for(int room=0;room<8;room++)for(int i=0;i<18;i++)
            {
                float x=RandomStream.Unit(ref State.rng)*12-6,z=RandomStream.Unit(ref State.rng)*12-6;
                if(room==0&&z<0)z+=5;
                SpawnEnemy(room,State.theme*6+RandomStream.Range(ref State.rng,0,6),RiftMap.Rooms[room]+new Vector2(x,z),i==17?RandomStream.Range(ref State.rng,0,6):-1);
            }
        }
        void SpawnOwnedTraining()
        {
            int count=State.training==0?1:State.training==1?8:3;
            for(int i=0;i<count;i++)
            {
                Vector2 point=State.training==0?new Vector2(0,2):State.training==1?new Vector2((i%3-1)*2,(i/3)*1.5f):new Vector2((i-1)*4,i==1?3:2);
                SpawnEnemy(0,State.training==2?9:0,RiftMap.Rooms[0]+point,-1);
                var enemy=State.enemies[State.enemies.Count-1];enemy.health=enemy.maxHealth=100000;enemy.speed=0;
                enemy.cooldown=State.training==2?2+i*.5f:1000;if(State.training!=2)enemy.attack=0;
            }
        }
        void SpawnEnemy(int room,int kind,Vector2 pos,int elite,bool boss=false,bool add=false,RunState into=null)
        {
            var spawnRun=into??State;
            float[] hp={1,.7f,.7f,.85f,.9f,1.2f,1.4f,.9f,.8f,.8f,1.1f,.65f};
            float[] atk={1,1.1f,.8f,.8f,.3f,.6f,1.1f,1.3f,1,1,.4f,.7f};
            float[] speed={2.7f,3.6f,2.4f,2.3f,2.2f,2,2.2f,3.2f,2.2f,2.3f,2,3};
            float health=70*Mathf.Pow(1.08f,spawnRun.stage-1)*(boss?45:hp[kind])*(elite>=0?3:1);
            var e=new EnemyState{id=spawnRun.nextId++,room=room,kind=kind,position=pos,health=health,maxHealth=health,
                attack=18*Mathf.Pow(1.055f,spawnRun.stage-1)*(boss?3:atk[kind])*(elite>=0?1.5f:1),speed=boss?2:speed[kind],elite=elite,boss=boss,add=add,cooldown=1+RandomStream.Unit(ref spawnRun.rng)};
            if(boss){spawnRun.bossId=e.id;e.pattern=spawnRun.layout.legacy?(spawnRun.stage-1)%3:spawnRun.layout.bossKind;if(e.pattern==1){e.health*=.85f;e.attack*=.9f;}if(e.pattern==2){e.health*=1.1f;e.attack*=1.1f;}e.maxHealth=e.health;}
            InitializeEnemyBrain(e,spawnRun);spawnRun.enemies.Add(e);
        }
        public bool ApplyBuild(BuildConfig build)
        {
            try
            {
                var candidate=ValidateBuildChange(build);var stats=BuildChangeStats(candidate);
                ApplyValidatedBuild(candidate,stats,State.training<0?candidate.Copy():null);return true;
            }
            catch(ArgumentException e){Log("BUILD_REJECTED",e.Message);return false;}
        }
        public void Log(string kind,string text)
        {
            State.logs.Add($"{State.time:000.0}s [{kind}] {text}");
            if(State.logs.Count>600)State.logs.RemoveAt(0);
        }
        EnemyState Target => State.enemies.Find(e=>e.id==State.targetId&&!e.dead);
        public void Tick(float dt)
        {
            if(State.paused||State.portal||!string.IsNullOrEmpty(State.navigationError)||State.phase==RunPhase.Cleared||State.phase==RunPhase.Failed)return;
            if(State.phase==RunPhase.Looting){CommitExperience();Loot(dt);RiftVisibility.Get(State,Map)?.Update();return;}
            // The pre-death window has to include the tick that kills the hero, and this body has many
            // early returns, so the snapshot is flushed in a finally rather than at the last statement.
            float observedTime=State.time,observedHealth=State.health;
            tickingClassEvents=true;
            try{TickCombat(dt);}finally{RecordTickTelemetry(State.time-observedTime,observedHealth);CaptureCompletedReview();RiftVisibility.Get(State,Map)?.Update();tickingClassEvents=false;FlushClassSkillEvents();}
        }
        void TickCombat(float dt)
        {
            ResolveCombatDeaths();if(SettleCombatOutcome())return;
            State.guideShrineTime=Mathf.Max(0,State.guideShrineTime-dt);State.resolveShrineTime=Mathf.Max(0,State.resolveShrineTime-dt);
            State.time+=dt;State.statistics.ticks++;State.potionCd=Mathf.Max(0,State.potionCd-dt);State.actionCd=Mathf.Max(0,State.actionCd-dt);
            TickPotionTimers(dt);
            TickEnemyTimers(dt);
            float passiveWalkingTime=Mathf.Clamp(dt-ItemEffects.ap05Cooldown,0,dt);TickCharges(dt);
            State.receivedDamage.RemoveAll(d=>d.time<State.time-3-Step);
            TickShields(dt);TickStatuses(dt);State.shoutTime-=dt;State.shadowTime-=dt;if(State.shadowTime<=0)State.shadowCharges=0;
            TickPassiveBuffs(dt);procCooldown=Mathf.Max(0,procCooldown-dt);
            for(int i=0;i<18;i++)State.cooldowns[i]=Mathf.Max(0,State.cooldowns[i]-dt);
            RegenerateClassResource(dt);RegenerateLife(dt);
            UsePotion();
            RiftExploration.Discover(State,Map);UpdateChestAvailability();UpdateSealAvailability();TrySpawnBoss(dt);
            TickHeroAction(dt);TickClassSkills(dt);
            AdvanceChestOpening(dt);AdvanceSealBreaking(dt);AdvanceOffering(dt);AdvanceShrine(dt);
            State.decisionTime-=dt;
            if(State.decisionTime<=.00001f)
            {
                Sense();
                if(!TryClassPolicySurvival()&&!TryAutomaticClassSkill()&&(!(ChestBusy||ShrineBusy||ObjectiveBusy)||Target!=null||InDanger))DecideRules();
                State.decisionTime=.2f;
            }
            var walkingOrigin=State.position;MoveHero(dt);TrackWalking(Vector2.Distance(walkingOrigin,State.position),dt,passiveWalkingTime);TickChannel(dt);
            if(State.potions.version>0&&Vector2.Distance(walkingOrigin,State.position)>.0001f)TryUseUtilityPotion(true);
            TickItemAilments(dt);
            TickEnemies(dt);TickProjectiles(dt);TickTraps(dt);TickEffects(dt);TickEnemyHazards(dt);TickLegendaryPulses();
            RemoveCancelledEvasions();
            ResolveCombatDeaths();
            TickFieldEvents(dt);if(!string.IsNullOrEmpty(State.navigationError)){CommitExperience();return;}
            if(SettleCombatOutcome())return;
            TickChests(dt);TickSeals(dt);TickOfferings();if(!string.IsNullOrEmpty(State.navigationError))return;
            TickShrine(dt);if(!string.IsNullOrEmpty(State.navigationError))return;Loot(dt);
            if(State.training>=0&&State.enemies.All(e=>e.dead))Finish(true,"훈련 완료",CombatFinish.TargetsDefeated);
        }
        void Sense()
        {
            sensed.Clear();foreach(var e in State.enemies)if(!e.dead&&Vector2.Distance(State.position,e.position)<=12&&Map.LineClear(State.position,e.position))sensed.Add(e);
            RiftExploration.ObserveEnemies(State,sensed);
            ObserveEdictTarget();ObserveEngagement();
            if(sensed.Count>0)State.noEnemySince=State.time;
            if(Target==null||!sensed.Contains(Target))SelectTarget(SelectRuleTarget(new Rule(-1)),"사망 또는 시야 변경");
            ObserveShieldEngagement();
            ObserveRangerEdictState();
        }
        public int CountNear(Vector2 pos,float radius) => State.enemies.Count(e=>!e.dead&&Vector2.Distance(pos,e.position)<=radius);
        public bool InDanger => DangerAt(State.position,1.5f);
        public float DisplaySkillCost(SkillDefinition skill)=>Cost(skill);
        // Invested ranks travel with the build the run fights with; a run restored from before
        // ranks existed has none and reads as rank one everywhere.
        int[] Ranks=>Stats.effectiveSkillRanks;
        public float SkillRange(int index)=>SkillEffects.Range(catalog.skills[index],SkillEffects.ActiveRank(Ranks,index));
        float ShoutBonus=>State.shoutBonus>0?State.shoutBonus:SkillEffects.ShoutBonus(SkillEffects.ActiveRank(Ranks,5));
        float ShadowFraction=>State.shadowFraction>0?State.shadowFraction:SkillEffects.ShadowFraction(SkillEffects.ActiveRank(Ranks,11));
        float LeapDefenseReduction=>SkillEffects.Passive(Ranks,HeroClass.Warrior,2);
        float SlowStrength(DamageSnapshot snapshot)=>.35f+(snapshot.passives[1]?SkillEffects.Passive(snapshot.ranks,HeroClass.Mage,1):0);
        float Cost(SkillDefinition skill,bool withoutConsumables=false)
        {
            float reduction=ClassCostReduction(skill.id)+LegendaryCostReduction(skill.id)+Stats.costReduction+Stats.runeSkillCost[catalog.skills.IndexOf(skill)]/100+(!withoutConsumables&&reducedNext?.5f:0)+(!withoutConsumables&&Hero.heroClass==HeroClass.Ranger&&Stats.passives[4]&&ItemEffects.ap05Ready?SkillEffects.Passive(Ranks,HeroClass.Ranger,4):0);
            if(skill.kind==SkillKind.Whirlwind){if(Stats.passives[1]&&State.channelTime>=2-.00001f)reduction+=SkillEffects.Passive(Ranks,HeroClass.Warrior,1);if(Stats.SetPieces("SW")>=2)reduction+=.15f;}
            if(skill.kind==SkillKind.Blizzard&&Stats.SetPieces("SM")>=2)reduction+=.2f;
            if(skill.kind==SkillKind.Pierce&&Stats.SetPieces("SAB")>=2||skill.kind==SkillKind.Chain&&Stats.SetPieces("SMB")>=2)reduction+=.15f;
            return skill.cost*(1-Mathf.Min(.5f,reduction));
        }
        void ResolveSkill(HeroActionState action)
        {
            int index=action.skill;var skill=catalog.skills[index];var target=State.enemies.Find(e=>e.id==action.targetId&&!e.dead);
            Vector2 origin=action.origin,aim=action.aim,destination=action.destination;
            switch(skill.kind)
            {
                case SkillKind.Whirlwind:break;
                case SkillKind.Leap:case SkillKind.Retreat:case SkillKind.Teleport:
                    State.position=destination;LandingPassives(action);
                    if(Stats.Rune(RuneBonus.MobilityGuard)>0)ItemEffects.runeMobilityGuardUntil=State.time+2;
                    if(skill.kind==SkillKind.Leap)
                    {
                        RememberLeapLanding(action);
                        int hits=AreaHit(destination,RuneSkillRadius(1,2.5f),1.8f,0);
                        if(hits>0&&Stats.SetPieces("SWB")>=4){ItemEffects.crushCharge=4;EffectEvent("SWB4","CHARGE",root:action.id,value:4);}
                        if(Stats.specials.Contains("LW02"))AreaHit(destination,4,Stats.AspectValue("LW02",.9f),0,default,360,false,"LW02",action.id,DamageKind.Legendary);
                        if(action.whirlwindReserved){AreaHit(destination,3,1.8f,0,default,360,false,"SW4",action.id,DamageKind.Set);action.whirlwindReserved=false;EffectEvent("SW4","CONSUMED",root:action.id);}
                    }
                    if(skill.kind==SkillKind.Retreat){RememberRetreatLanding(action);if(Stats.SetPieces("SAB")>=4){ItemEffects.pierceCharge=6;EffectEvent("SAB4","CHARGE",root:action.id,value:6);}}
                    if(skill.kind==SkillKind.Retreat&&action.legacyRetreatTrapOnLanding&&Stats.specials.Contains("LA03")&&(action.policy?.retreatTrapEquipped??State.build.activeSkills.Contains(8))){CreateTrap(action,origin,3);action.legacyRetreatTrapOnLanding=false;}
                    break;
                case SkillKind.Crush:
                    var crushTargets=AreaTargets(origin,RuneSkillRadius(2,3)*ClassArea(action.id),aim-origin,100);bool crushBonus=action.crushBonus;
                    if(crushTargets.Length==0)ActionEvent(action,"ACTION_MISS","분쇄 일격의 실제 부채꼴 안에 적이 없습니다.");
                    float single=Stats.specials.Contains("LW03")&&crushTargets.Length==1?Stats.AspectValue("LW03",.5f):0;
                    var crushSnapshot=CaptureDamage();
                    foreach(var e in crushTargets){Hit(e,skill.coefficient,0,true,single+RuneMultiBonus(crushSnapshot,crushTargets.Length),crushSnapshot,definition:"W03",root:action.id);if(crushBonus)Hit(e,1.6f,0,false,definition:"SWB4",root:action.id,kind:DamageKind.Set);}
                    break;
                case SkillKind.Slam:case SkillKind.Nova:
                    int novaHits=AreaHit(origin,3*ClassArea(action.id),skill.coefficient,skill.kind==SkillKind.Nova?2:0);
                    if(novaHits==0)ActionEvent(action,"ACTION_MISS","폭발 순간 자기 범위에 적이 없습니다.");
                    if(skill.kind==SkillKind.Nova&&novaHits>0&&Stats.SetPieces("SMB")>=4){ItemEffects.chainCharge=6;ItemEffects.chainCharges=2;EffectEvent("SMB4","CHARGE",root:action.id,value:2);}
                    foreach(var e in AreaTargets(origin,3*ClassArea(action.id),default,360)){ApplyStatus(e,skill.kind==SkillKind.Nova?StatusKind.Freeze:StatusKind.Stun,SkillId(index),1.5f,action.id);if(skill.kind==SkillKind.Nova)ClassFreeze("M06",e,action.id);}break;
                case SkillKind.Shield:AddShield(SkillId(index),Stats.hp*SkillEffects.ShieldFraction(Hero.heroClass,SkillEffects.ActiveRank(Ranks,index)),index==4&&Set("REF_SW02",3)?5:4,action.id);break;
                case SkillKind.Shout:{int rank=SkillEffects.ActiveRank(Ranks,index);State.resource=Mathf.Min(Stats.maxResource,State.resource+SkillEffects.ShoutResource(rank));State.shoutTime=6;State.shoutBonus=SkillEffects.ShoutBonus(rank);break;}
                case SkillKind.Pierce:LaunchPierce(action);break;
                case SkillKind.Multi:LaunchMulti(action);break;
                case SkillKind.Trap:CreateTrap(action,aim,5);break;
                case SkillKind.Mark:
                    if(target!=null&&Vector2.Distance(origin,target.position)<=10&&Map.LineClear(origin,target.position))ApplyStatus(target,StatusKind.Mark,"A05",10,action.id);
                    else ActionEvent(action,"ACTION_MISS","표식 대상이 사라졌거나 실제 사거리·시야를 벗어났습니다.");break;
                case SkillKind.Shadow:State.shadowCharges=3;State.shadowTime=8;State.shadowFraction=SkillEffects.ShadowFraction(SkillEffects.ActiveRank(Ranks,index));break;
                case SkillKind.Fireball:LaunchFireball(action);break;
                case SkillKind.Blizzard:AddGround(aim,3,0,6*(1+Stats.runeSkillDuration[13]/100),Stats.damage*.65f,false,13,root:action.id,followsTarget:Stats.specials.Contains("LM01")&&action.blizzardMode==BlizzardMode.Follow);break;
                case SkillKind.Chain:
                    // A paid preparation can lose its first target. Do not hit through a wall
                    // or outside the actual initial range, and do not transfer it to a new target.
                    if(!ChainFirstTargetValid(origin,target))
                    {ActionEvent(action,"ACTION_MISS","첫 대상이 사라졌거나 사거리·시야를 벗어나 연쇄 번개가 빗나갔습니다.");break;}
                    var visits=new Dictionary<int,int>();var current=target;Vector2 last=origin;
                    int hops=ChainHitLimit(action.chainBonus);
                    for(int i=0;i<hops&&current!=null;i++)
                    {
                        int previous=current.id;visits[previous]=visits.TryGetValue(previous,out int count)?count+1:1;
                        Hit(current,1.1f*Mathf.Pow(.8f,i)*(visits[previous]>1?AspectGrowth.SnapshotValue(action.snapshot,"LM04",.5f):1),3,true,0,action.snapshot,definition:"M03",root:action.id);Visual?.Invoke(last,current.position,14,1);last=current.position;
                        current=NextChainTarget(last,previous,visits,State.enemies.Where(Perceived));
                    }
                    break;
            }
            if(index!=6&&index!=7&&index!=12)Visual?.Invoke(origin,skill.kind==SkillKind.Leap||skill.kind==SkillKind.Retreat||skill.kind==SkillKind.Teleport?destination:aim,index,skill.radius);
        }
        void MoveHero(float dt)
        {
            if(CSStationary)return;
            if(HeroActionBusy&&State.heroAction.phase!=HeroActionPhase.Channeling)return;
            if(MoveEdictResponse(dt))return;
            if(MoveClassSkillIntent(dt))return;
            if(MoveEdictGather(dt))return;
            if(MoveEdictRanger(dt))return;
            if(MoveEdictNova(dt))return;
            if(MoveEdictWhirlwind(dt))return;
            var policy=HeroActionBusy?State.heroAction.policy:null;
            var movementRule=State.movementRule>=0&&State.movementRule<State.build.rules.Count?State.build.rules[State.movementRule]:null;
            if(edictSource!=null&&State.movementRule>=0&&(State.movementAction==RuleAction.Skill||State.movementAction==RuleAction.Basic)&&
                edictMovementRules.TryGetValue(State.targetRuleId??"",out var compiledMovement))movementRule=compiledMovement;
            bool searching=EdictLastSeenGoal(out var lastSeenGoal);
            if(movementRule==null&&!HeroActionBusy&&!searching)return;
            var target=Target;Vector2 goal=State.position;bool recovering=false;
            if(TargetPolicy!=null&&target!=null&&!EdictTargetEligible(target))target=null;
            if(!HeroActionBusy&&State.movementAction!=RuleAction.Skill&&State.movementAction!=RuleAction.Basic)target=null;
            if(State.portalCast>0&&target==null){State.exploration.probeTime=State.time;return;}
            if((ChestBusy||ShrineBusy||ObjectiveBusy)&&target==null&&!InDanger){State.exploration.probeTime=State.time;return;}
            if(!AdvanceEdictPursuit(dt,searching,target,movementRule))return;
            if(searching&&!DiscoveryBeforeEnemies(ref goal,ref recovering)){goal=lastSeenGoal;State.action="마지막으로 발견한 적의 위치 확인";}
            else if(target!=null)
            {
                Vector2 delta=State.position-target.position;float d=delta.magnitude;float range=policy?.distance??(movementRule!=null&&movementRule.overrideMovement?movementRule.distance:Policy.distance);
                var movement=policy?.movement??(movementRule!=null&&movementRule.overrideMovement?movementRule.movement:Policy.movement);
                float standRange=Hero.heroClass==HeroClass.Warrior?(movementRule?.id=="edict:WARRIOR:BASIC"?2:2.5f):movementRule?.id=="edict:A02"?8:movementRule?.id=="edict:A03"?6:movementRule?.id=="edict:A06"&&HuntEdictV2.Value(edictSource,"A06",3)=="A02"?8:10;
                if(policy==null&&EdictFieldGoal(target,range,ref goal)){}
                else
                {
                if(movement==MovementMode.Stand&&d<=standRange)return;
                if(movement==MovementMode.Retreat)goal=State.position+delta.normalized*2;
                else if(movement==MovementMode.KeepDistance)
                {if(d<range-.75f)goal=State.position+delta.normalized*2;else if(d>range+.75f)goal=target.position+delta.normalized*range;}
                else if(movement==MovementMode.Orbit&&d<=range+1)
                {Vector2 tangent=new Vector2(-delta.y,delta.x).normalized*((policy?.clockwise??Policy.clockwise)?1:-1);goal=target.position+(delta.normalized+tangent*.4f).normalized*range;}
                else goal=target.position+delta.normalized*Mathf.Min(range,Hero.heroClass==HeroClass.Warrior?1.6f:8);
                if(Hero.heroClass!=HeroClass.Warrior&&!Map.ProjectileClear(State.position,target.position,.3f))goal=ShotApproach(target,goal);
                }
            }
            else
            {
                if(HeadToBossNow())
                {if(TargetPolicy!=null&&!State.layout.roamingBoss)goal=State.layout.rooms[State.layout.bossRoom].position;else{var boss=State.enemies.Find(e=>e.id==State.bossId);if(boss!=null)goal=boss.position;}}
                else
                {
                    if(!State.layout.legacy)
                    {var remembered=TargetPolicy==null?RiftExploration.RememberedTarget(State,Map):null;goal=remembered??RiftExploration.Goal(State,Map,Policy.exploration);State.action=remembered.HasValue?"마지막으로 발견한 적의 위치 확인":"미탐색 지점으로 이동";}
                    else
                    {
                    if(Vector2.Distance(State.position,RiftMap.Rooms[State.exploreRoom])<2)
                    {if(!State.visited.Contains(State.exploreRoom))State.visited.Add(State.exploreRoom);State.exploreRoom=(State.exploreRoom+((policy?.clockwise??Policy.clockwise)?1:7))%8;}
                    goal=RiftMap.Rooms[State.exploreRoom];State.action="미탐색 영역으로 이동";
                    }
                }
                var loot=State.movementAction==RuleAction.Loot?FindRuleLoot():null;
                if(loot!=null){goal=loot.position;recovering=true;State.action="전리품 회수";}
                else
                {
                    if(State.movementAction==RuleAction.Chest)ChooseChestGoal(ref goal);
                    else if(State.movementAction==RuleAction.Explore)ChooseShrineGoal(ref goal);
                    recovering=ActiveChest!=null;
                    // The objective takes the goal only when no chest or shrine wants it. Both are
                    // finite, so waiting for them cannot stall the run.
                    if(ActiveChest==null&&ActiveShrine==null&&ChooseObjectiveGoal(ref goal))recovering=true;
                }
            }
            if(!Map.Walkable(goal))goal=target!=null?CombatApproach(target,goal):Map.MoveDirect(State.position,goal,Vector2.Distance(State.position,goal));
            float speed=MovementSpeed(recovering)*(State.enemySlowTime>0?.65f:1);
            Vector2 p=Map.Move(State.position,goal,speed*dt,0,State.time);if(!EdictPursuitStepAllowed(p,searching,target,movementRule))return;
            float moved=Vector2.Distance(p,State.position);State.moveDistance+=moved;
            State.position=p;State.destination=goal;
            if(!State.layout.legacy&&!RiftExploration.TrackMovement(State,Map,goal,State.activeSkill>=0||State.actionCd>0))
            {CancelChest("경로 재탐색");CancelShrine("경로 재탐색");CancelSeal("경로 재탐색");CancelOffering("경로 재탐색");Log("PATH_RETRY","같은 목표 접근 실패 · 다른 관찰 지점 탐색");}
        }
        void TickChannel(float dt)
        {
            if(State.activeSkill!=0||Hero.heroClass!=HeroClass.Warrior)return;
            var action=State.heroAction;if(action.phase!=HeroActionPhase.Channeling)return;
            if(action.startedAt>=State.time-.00001f)return;
            action.emptyTime=AreaTargets(State.position,RuneSkillRadius(0,2.5f),default,360).Length==0?action.emptyTime+dt:0;
            if(action.emptyTime>=.5f-.00001f){InterruptHeroAction("0.5초 동안 공격 범위에 적 없음");return;}
            State.channelTime+=dt;State.channelTick-=dt;if(State.channelTick>.00001f)return;
            if(action.exitChannelForBuild){CompleteHeroAction(action,"설정 변경 · 유지 구간 종료");return;}
            if(action.exitChannelForSurvival){CompleteHeroAction(action,"전역 회피 · 유지 구간 종료");return;}
            float cost=Cost(catalog.skills[0]);if(State.resource<cost){InterruptHeroAction("회오리 자원 부족");return;}
            if(EndEdictWhirlwindBeforeTick(action,cost))return;
            State.channelTick+=.25f;State.resource-=cost;ConsumeCostEffects(cost,action);action.cost+=cost;State.lastAttackTime=State.time;AreaHit(State.position,RuneSkillRadius(0,2.5f),.5f,0);Visual?.Invoke(State.position,State.position,0,RuneSkillRadius(0,2.5f));
            State.action=Loc.F("회오리 · {0:0.0}초 유지", State.channelTime);
            PullWhirlwindTargets();
            EndTimedEdictWhirlwind(action);
        }
        void PullWhirlwindTargets()
        {
            if(!Stats.specials.Contains("LW01")||State.channelTime<1-.00001f||EffectTick-Mathf.RoundToInt(lastPull/Step)<20)return;
            lastPull=State.time;
            // Move the inner ring first so outer enemies can follow into the space it vacated.
            var targets=AreaTargets(State.position,3.5f,default,360).OrderBy(e=>(e.position-State.position).sqrMagnitude).ThenBy(e=>e.id).ToArray();
            foreach(var enemy in targets)
            {
                if(enemy.boss||Immobilized(enemy)||!Map.Walkable(enemy.position,.4f))continue;
                Vector2 from=enemy.position;float distance=Mathf.Min(Stats.AspectValue("LW01",1.5f),Mathf.Max(0,Vector2.Distance(from,State.position)-.85f));
                Vector2 to=Map.MoveDirect(from,State.position,distance,.4f);
                foreach(var body in State.enemies.Where(e=>e!=enemy&&!e.dead).OrderBy(e=>e.id))
                {
                    float radius=.4f+(body.boss?1.2f:.4f);Vector2 offset=from-body.position;
                    // An old crowd may already overlap. Allow separation, never deeper penetration.
                    if(offset.sqrMagnitude<=radius*radius&&Vector2.Dot(to-from,offset)>=0)continue;
                    float entry=Entry(from,to,body.position,radius);
                    if(!float.IsInfinity(entry))to=Vector2.Lerp(from,to,Mathf.Max(0,entry-.001f));
                }
                float moved=Vector2.Distance(from,to);if(moved<.001f||!Map.Walkable(to,.4f))continue;
                InterruptEnemyAction(enemy,"회오리에 끌림");enemy.position=to;Map.Repath(enemy.id+1);
                EffectEvent("LW01","PULLED",root:State.heroAction.id,target:enemy.id,value:moved);
            }
        }
        EnemyState[] AreaTargets(Vector2 pos,float radius,Vector2 direction,float arc)
            =>State.enemies.Where(e=>!e.dead&&Vector2.Distance(pos,e.position)<=radius&&Map.LineClear(pos,e.position)&&(arc>=360||Vector2.Angle(direction,e.position-pos)<=arc*.5f)).OrderBy(e=>e.id).ToArray();
        int AreaHit(Vector2 pos,float radius,float coefficient,int element,Vector2 direction=default,float arc=360,bool procs=true,string definition=null,int root=0,DamageKind kind=DamageKind.Direct)
        {
            var targets=AreaTargets(pos,radius,direction,arc);var snapshot=CaptureDamage();definition??=SkillId(State.heroAction.skill);if(root==0)root=State.heroAction.id;
            foreach(var e in targets)Hit(e,coefficient,element,procs,RuneMultiBonus(snapshot,targets.Length),snapshot,definition:definition,root:root,kind:kind);return targets.Length;
        }
        DamageSnapshot CaptureDamage()=>new DamageSnapshot{classWildcardEmpower=CaptureClassLegendaryEmpower(),aspectIds=Stats.aspectLevels.Keys.OrderBy(id=>id,System.StringComparer.Ordinal).ToArray(),aspectLevels=Stats.aspectLevels.OrderBy(pair=>pair.Key,System.StringComparer.Ordinal).Select(pair=>pair.Value).ToArray(),legendaryPowers=EquippedLegendaryPowers().Select(p=>p.Id).ToArray(),legendaryDamage=CaptureLegendaryDamage(),runeV13=(float[])Stats.runeV13.Clone(),runeSkillPower=Enumerable.Range(0,18).Select(i=>Stats.runeSkillPower[i]+Stats.runeSkillLevels[i]*10+SkillEffects.Power(Ranks,i)).ToArray(),runeBonuses=(float[])Stats.runeBonuses.Clone(),damage=Stats.damage,bonus=(State.shoutTime>0?ShoutBonus:0)+(elementBuff>0?SkillEffects.Passive(Ranks,HeroClass.Mage,5):0),crit=Stats.crit,critDamage=Stats.critDamage,
            level=EffectiveLevel,elements=Stats.bonuses.Skip(4).Take(6).Select(v=>v/100).ToArray(),passives=(bool[])Stats.passives.Clone(),ranks=Ranks==null?null:(int[])Ranks.Clone(),
            crowdCaptured=Hero.heroClass==HeroClass.Warrior,crowdQualified=Hero.heroClass==HeroClass.Warrior&&CountNear(State.position,3)>=3};
        void Deal(EnemyState e,float damage,bool critical)
        {
            if(damage>0&&!e.dead)State.lastOutgoingDamageTime=State.time;
            if(e.dead)return;State.dealt+=Mathf.Min(e.health,damage);e.health-=damage;Visual?.Invoke(e.position,e.position,critical?31:30,damage);
            if(e.boss&&!e.brain.boss.enraged&&e.health<=e.maxHealth*.5f)e.brain.boss.enragePending=true;
            if(e.health>0)return;e.health=0;e.dead=true;e.pendingDeath=true;State.kills++;
        }
        void RewardEnemyDeath(EnemyState e)
        {
            if(e.boss)return;
            if(e.goblin){RewardGoldenGoblin(e);return;}
            if(!e.add)
            {
                State.meter+=e.elite>=0?5:1;
                if(State.training<0)
                {
                    var rune=RuneGrowth.GrantMonster(account,State,e);
                    if(rune!=null)Log("RUNE_DROP",Loc.Source("G{0} 룬 · {1}칸 · 모양 {2} 획득",rune.grade,Runes.RuneMasteryCatalog.ShapeById(rune.shapeId).Size,Runes.RuneMasteryCatalog.ShapeById(rune.shapeId).ShapeNumber));
                    RiftResources.Add(State,RiftResourceKind.Gold,e.position,KillGold(e.elite>=0?25+5*State.stage:5+State.stage));
                    RiftResources.RollGems(State,e.elite>=0?RiftRewardSource.Elite:RiftRewardSource.Normal,e.position);
                    QueueExperience(Mathf.FloorToInt((e.elite>=0?50:10)*(1+.05f*(State.stage-1))));
                    bool drop=e.elite>=0||RandomStream.Unit(ref State.rewardRng)<.02f;
                    if(drop)Drop(e.position,RiftRarity.Roll(e.elite>=0?RiftRewardSource.Elite:RiftRewardSource.Normal,State.stage,ref State.rewardRng));
                }
            }

        }
        void Drop(Vector2 pos,int rarity)=>DropFrom(pos,rarity,ref State.rewardRng);
        // Named apart from Drop so reflection by name in the tests still finds one method.
        void DropFrom(Vector2 pos,int rarity,ref uint rng)
        {
            int id=State.nextId++;State.drops.Add(new DropState{id=id,position=pos,item=Economy.CreateRiftItem(Hero.heroClass,RandomStream.Range(ref rng,0,8),rarity,RandomStream.Range(ref rng,Mathf.Max(1,State.stage-2),State.stage+3),State.stage,ref rng,State.id+"-"+id)});
        }
        void AddGround(Vector2 pos,float radius,float delay,float duration,float damage,bool hostile,int kind,int element=0,string caster=null,string definition=null,int root=0,bool? followsTarget=null)
        {
            if(!hostile&&kind==13)State.effects.RemoveAll(f=>OwnBlizzard(f)&&f.duration<=.00001f);
            if(!hostile&&(kind==8||kind==13))
            {var same=State.effects.Where(e=>!e.hostile&&e.kind==kind&&(string.IsNullOrEmpty(e.casterId)||e.casterId==State.heroId)).OrderBy(e=>e.id).ToList();if(same.Count>=2)State.effects.Remove(same[0]);}
            State.effects.Add(new GroundEffect{id=State.nextId++,position=pos,radius=radius,delay=delay,duration=duration,damage=damage,hostile=hostile,periodic=hostile&&duration>.11f,kind=kind,createdAt=State.time,
                snapshot=hostile?null:CaptureDamage(),element=hostile?element:kind==13?2:kind==12?1:kind==8?4:element,
                definitionId=definition??(hostile?"ENEMY_GROUND":kind==13?"M02":kind==12?"LM02":kind==8?"LEGACY_TRAP":"SAB4"),casterId=caster??(hostile?"ENEMY":State.heroId),
                rootCastId=root!=0?root:State.heroAction.id,followsTarget=!hostile&&kind==13&&(followsTarget??Stats.specials.Contains("LM01"))});
        }
        void TickItemAilments(float dt)
        {
            foreach(var e in State.enemies.ToArray())
            {
                if(e.dead)continue;
                TickSetPoison(e,dt);if(!e.dead)TickFrostExposure(e,dt);
            }
        }
        DamageSnapshot GroundSnapshot(GroundEffect fx)
        {
            // Old fields recorded base damage only; retain it without inventing historical bonuses.
            return fx.snapshot??(fx.snapshot=new DamageSnapshot{damage=1,level=EffectiveLevel,elements=new float[6],passives=new bool[6],critDamage=1});
        }
        void TickEffects(float dt)
        {
            foreach(var fx in State.effects.ToArray())
            {
                if(fx.kind==13&&!fx.hostile)continue;
                if(fx.createdAt>=State.time-.00001f)continue;
                fx.delay-=dt;if(fx.delay>.0001f)continue;
                if(fx.kind==40)
                {
                    Vector2 direction=(fx.end-fx.position).normalized;int hits=0;
                    foreach(var e in State.enemies.Where(e=>!e.dead).OrderBy(e=>Vector2.Dot(e.position-fx.position,direction)).ThenBy(e=>e.id).ToArray())
                    {var v=e.position-fx.position;float dot=Vector2.Dot(v,direction);if(dot<0||dot>10||Mathf.Abs(v.x*direction.y-v.y*direction.x)>.3f||!Map.LineClear(fx.position,e.position))continue;
                     Hit(e,1.2f,0,false,0,GroundSnapshot(fx),definition:"SAB4",root:fx.rootCastId,instance:fx.id,kind:DamageKind.Set);if(++hits>=5)break;}
                    Visual?.Invoke(fx.position,fx.end,6,1);State.effects.Remove(fx);continue;
                }
                fx.periodic=fx.PeriodicIncoming;fx.duration-=dt;fx.tick-=dt;
                if(fx.tick<=.00001f)
                {
                    fx.tick+=.5f;float amount=fx.duration<.11f&&fx.kind!=8?fx.damage:fx.damage*.5f;
                    if(fx.hostile)
                    {float d=Vector2.Distance(fx.position,State.position);if(d<fx.radius&&(fx.kind!=6||d>2))Hurt(amount,fx.element,fx.casterId??"ENEMY",fx.definitionId??"ENEMY_GROUND",fx.rootCastId,fx.id,fx.periodic?DamageKind.Periodic:DamageKind.Direct);}
                    else foreach(var e in AreaTargets(fx.position,fx.radius,default,360))
                    {
                        var snapshot=GroundSnapshot(fx);
                        Hit(e,amount/Mathf.Max(.0001f,snapshot.damage),fx.kind==12?1:fx.kind==8?4:fx.element,false,0,snapshot,fx.kind==12,
                            definition:fx.definitionId??(fx.kind==12?"LM02":"LEGACY_GROUND"),root:fx.rootCastId,instance:fx.id,kind:fx.kind==12?DamageKind.Legendary:DamageKind.Periodic);
                    }
                    if(!fx.hostile&&fx.kind==12){Visual?.Invoke(fx.position,fx.position,12,fx.radius);State.effects.Remove(fx);continue;}
                }
                if(fx.duration<=.00001f)State.effects.Remove(fx);
            }
            TickBlizzards(dt);
        }
        void Loot(float dt)
        {
            if(State.training>=0)return;
            CollectResources(dt);if(State.portal)return;
            if(State.limitedLoot&&Economy.FreeSlots(Hero)>0)State.limitedLoot=false;
            var pending=State.drops.Where(d=>!d.claimed&&!d.ignored).ToArray();
            bool combatClear=edictLoot==null||State.phase==RunPhase.Looting||EdictLootCombatClear;
            foreach(var drop in pending)
            {
                // The configured continue-with-limited-loot policy leaves new equipment on the
                // floor. Already owned items are never removed to make this transition possible.
                if(State.limitedLoot&&Policy.bagPolicy==BagPolicy.Portal){drop.ignored=true;continue;}
                if(edictLoot!=null)
                {
                    var mode=edictLoot.Mode(drop.item,Hero.heroClass);
                    if(mode==EdictLootMode.Ignore||State.phase==RunPhase.Looting&&!edictLoot.collectAtEnd){drop.ignored=true;continue;}
                    if(mode==EdictLootMode.AfterCombat&&!combatClear)continue;
                    if(State.phase!=RunPhase.Looting&&!Map.LineClear(State.position,drop.position))continue;
                }
                else if(drop.item.rarity<Policy.minimumRarity){drop.ignored=true;continue;}
                if(Vector2.Distance(State.position,drop.position)>Stats.pickup&&State.phase!=RunPhase.Looting)continue;
                // The shortage trigger can fire before the bag is completely full: keep the configured
                // slots free by ignoring further equipment or by cleaning up in town early.
                bool shortage=BagShort;
                if(shortage&&Policy.bagPolicy==BagPolicy.Ignore){drop.ignored=true;continue;}
                if(shortage&&Policy.bagPolicy==BagPolicy.Portal&&Economy.FreeSlots(Hero)>0){PortalForBag(dt);return;}
                if(Economy.AddItem(Hero,drop.item,Policy.bagPolicy,account,edictField?.replacementRank=="RARITY")){drop.claimed=true;State.lootCount++;Log("LOOT",drop.item.name);}
                else if(Policy.bagPolicy==BagPolicy.Portal||Policy.bagPolicy==BagPolicy.Replace&&edictField?.replacementFail=="PORTAL"){PortalForBag(dt);return;}
                else drop.ignored=true;
            }
            if(State.phase==RunPhase.Looting&&State.drops.All(d=>d.claimed||d.ignored)&&State.resources.All(d=>d.claimed||d.ignored))Finish(true,"균열 클리어");
        }
        void BossClear()
        {
            if(State.bossRewarded)return;CompleteReadyChest();if(!string.IsNullOrEmpty(State.navigationError))return;CloseUnopenedChests();State.bossRewarded=true;State.phase=RunPhase.Looting;InterruptHeroAction("보스 처치 후 전리품 정리");State.activeSkill=-1;
            RiftResources.Add(State,RiftResourceKind.Gold,State.position,KillGold(800+50*State.stage));
            RiftResources.Add(State,RiftResourceKind.Material,State.position,5+State.stage/5);
            RiftResources.Add(State,RiftResourceKind.EnhancementStone,State.position+Vector2.left*.4f,BlacksmithCatalog.RewardStones(State.stage));
            RiftResources.RollGems(State,RiftRewardSource.Boss,State.position);
            QueueExperience(Mathf.FloorToInt(300*(1+.05f*(State.stage-1))));Hero.highestClear=Mathf.Max(Hero.highestClear,State.stage);
            if(!Hero.firstClears.Contains(State.stage)){Hero.firstClears.Add(State.stage);RiftEarnings.GrantGold(account,State,Gold(1000+100*State.stage));account.materials+=10+State.stage/5;Log("FIRST_CLEAR","캐릭터 초회 보상 지급");}
            for(int i=0;i<3;i++)Drop(State.position+new Vector2(i-1,1),RiftRarity.Roll(RiftRewardSource.Boss,State.stage,ref State.rewardRng));
            RuneGrowth.GrantVictory(account,State);
            ContentUnlocks.Reconcile(account);
            Log("BOSS_CLEAR","보스 처치 · 전리품 정리");
        }
        public void Abandon() { if(State.phase==RunPhase.Looting){foreach(var d in State.drops)if(!d.claimed)d.ignored=true;foreach(var d in State.resources)if(!d.claimed)d.ignored=true;Finish(true,"전리품 정리 종료",CombatFinish.Abandoned);}else Finish(false,"마을로 귀환",CombatFinish.Abandoned); }
        void Finish(bool won,string reason,CombatFinish completion=CombatFinish.Other)
        {
            if(State.phase==RunPhase.Cleared||State.phase==RunPhase.Failed)return;
            CommitExperience();
            InterruptHeroAction(reason);
            CombatTelemetry.Finish(State.statistics,completion);
            CloseUnopenedChests();State.phase=won?RunPhase.Cleared:RunPhase.Failed;State.action=reason;Log("RUN_END",reason);
            if(State.training>=0)return;
            ContentUnlocks.RecordRunEnd(account);
            if(account.records.Any(r=>r.id==State.id))return;
            account.records.Insert(0,new RunRecord{id=State.id,hero=catalog.classNames[(int)Hero.heroClass],result=reason,stage=State.stage,kills=State.kills,loot=State.lootCount,chestsOpened=State.layout.chests.Count(c=>c.phase==ChestPhase.Opened),chestsTotal=State.layout.chests.Count,mapFingerprint=State.layout.fingerprint,objective=State.training<0?RiftObjectives.Capture(State):null,simulationSeconds=State.time,realSeconds=State.realTime,damageDealt=State.dealt,logs=new List<string>(State.logs)});
            while(account.records.Count>CombatHistory.RecordLimit)account.records.RemoveAt(account.records.Count-1);
            CaptureCompletedReview();
        }
    }
}
