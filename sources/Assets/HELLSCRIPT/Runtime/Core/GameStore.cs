using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    // Development adapter. Production account ownership and server-time settlement are a separate boundary.
    public sealed partial class GameStore
    {
        public const int MaximumSchemaVersion=15;
        public AccountSave Data {get;private set;}
        public string Error {get;private set;}="";
        public string OfflineMessage {get;private set;}="";
        public string GemRecoveryMessage {get;private set;}="";
        public string GemRecoveryArchive {get;private set;}="";
        public string QualityRecoveryMessage {get;private set;}="";
        public string QualityRecoveryArchive {get;private set;}="";
        public int LocalIdleGoldAwarded {get;private set;}
        public int LocalIdleMaterialsAwarded => 0; // Kept for existing development evidence readers.
        public int LocalIdleStonesAwarded {get;private set;}
        readonly Func<long> offlineClock;
        long SeenNow()=>Math.Max(Data.lastSeenUtc,offlineClock());
        long? pendingLocalIdleThrough;
        bool settlingLocalIdle;
        readonly string path;
        public GameStore(string directory,GameCatalog catalog=null,Func<long> forgeClock=null,Func<long> offlineClock=null)
        {
            this.offlineClock=offlineClock??(()=>DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            if(forgeClock!=null)ForgeClock=forgeClock;
            CombatArchive=new CombatJournalArchive(directory);
            Directory.CreateDirectory(directory);path=Path.Combine(directory,"hellscript-local-v1.json");
            string source=path;
            Data=Read(path);
            if(Data==null){source=path+".bak";Data=Read(source);}
            if(Data==null&&(File.Exists(path)||File.Exists(path+".bak")))throw new InvalidDataException(Loc.F("저장 파일과 백업을 복구할 수 없습니다. 원본은 그대로 보존했습니다.\n{0}", path));
            if(Data==null){Data=NewAccount(catalog);Data.lastSeenUtc=this.offlineClock();}
            else if(Data.schema==1)
            {
                File.Copy(source,path+".schema1-"+DateTime.UtcNow.Ticks+".json",false);
                Migrate(Data);
            }
            ContentUnlocks.Normalize(Data);
            EquipmentShop.Normalize(Data);
            SettleLocalIdle();
            BlacksmithCatalog.Normalize(Data);SettleForgeJobs();
        }
        AccountSave Read(string file)
        {
            try
            {
                if(!File.Exists(file))return null;var a=JsonUtility.FromJson<AccountSave>(File.ReadAllText(file));
                if(a!=null&&a.schema>MaximumSchemaVersion)throw new NotSupportedException("이 저장 파일은 더 새로운 게임 버전이 필요합니다. 원본을 보존하고 불러오기를 중단했습니다.");
                if(a==null||a.schema<1||a.heroes==null||a.heroes.Count!=3||a.cores==null||a.cores.Length!=8)return null;
                if(a.heroes.Any(h=>h==null||h.build==null||h.inventory==null||h.level<1||h.level>40))return null;
                if(a.heroes.Any(h=>h.level>ClassSkills.LevelCap(h)||!ClassSkillLoadout.IsAbsent(h.build.classSkills)&&!ClassSkills.Enabled(h)))
                    throw new NotSupportedException("This save requires the class-skill release. The original file is preserved.");
                bool unlocksMigrated=a.schema<13&&(a.contentUnlocks?.version??0)<ContentUnlocks.Version;
                bool gemsMigrated=MigrateRetiredGemStacks(a);
                Normalize(a);
                if(unlocksMigrated)
                {
                    string archive=file+".before-content-unlocks-v2.json";
                    if(!File.Exists(archive))File.Copy(file,archive,false);
                }
                if(a.schema>=2)
                {
                    var allItems=PersistedItems(a);
                    if(allItems.Any(i=>i.contentVersion>ItemCatalog.Version))throw new NotSupportedException("더 새로운 장비 데이터가 있어 불러오기를 중단했습니다. 저장 파일은 보존했습니다.");
                    var socketItems=allItems.ToArray();
                    if(socketItems.Any(i=>i.sockets!=null&&i.sockets.Count>0&&(!GemCatalog.AllowsSocket(i)||i.sockets.Count>1||i.sockets[0]==null||i.sockets[0].index!=0)))
                        throw new NotSupportedException(Loc.T("소켓 구조를 안전하게 읽을 수 없어 불러오기를 중단했습니다. 저장 파일은 보존했습니다."));
                    bool repaired=false,qualityRepaired=false;foreach(var item in socketItems)
                    {gemsMigrated|=MigrateRetiredGemSockets(item);repaired|=GemCatalog.RepairGemValues(item);qualityRepaired|=ItemQuality.RepairValues(item);}
                    ValidateItems(a);
                    if(repaired||qualityRepaired||gemsMigrated)
                    {
                        string archive=file+(repaired||gemsMigrated?".gem-recovery-":".quality-recovery-")+Guid.NewGuid().ToString("N")+".json";
                        try{File.Copy(file,archive,false);}
                        catch(Exception error){throw new NotSupportedException(Loc.T(repaired||gemsMigrated?"보석 복구 원본을 보관하지 못해 불러오기를 중단했습니다. 저장 파일은 보존했습니다.":"품질 복구 원본을 보관하지 못해 불러오기를 중단했습니다. 저장 파일은 보존했습니다."),error);}
                        if(repaired){GemRecoveryArchive=archive;GemRecoveryMessage=Loc.T("잘못된 보석 값을 빈 소켓으로 복구했습니다. 장비와 원본 저장 파일은 보존했습니다.");}
                        if(gemsMigrated){GemRecoveryArchive=archive;GemRecoveryMessage=(repaired?GemRecoveryMessage+"\n":"")+Loc.T("해골 보석을 같은 단계와 수량의 금강석으로 바꿨습니다. 원본 저장 파일은 보존했습니다.");}
                        if(qualityRepaired){QualityRecoveryArchive=archive;QualityRecoveryMessage=Loc.T("잘못된 장비 품질 기록을 복구했습니다. 장비와 투자 원장, 원본 저장 파일은 보존했습니다.");}
                    }
                }
                if(a.schema<6)
                {
                    string archive=file+".schema"+a.schema+"-before-hunt-edict.json";
                    if(!File.Exists(archive))File.Copy(file,archive,false);
                }
                return a;
            }
            catch(NotSupportedException){throw;}
            catch(Exception){return null;}
        }
        public static void Migrate(AccountSave a)
        {
            Normalize(a);
            foreach(var h in a.heroes)foreach(var i in h.inventory)ItemCatalog.Upgrade(i,h.heroClass);
            foreach(var i in a.warehouse)ItemCatalog.Upgrade(i,a.Hero.heroClass);
            if(a.suspendedRun!=null)
            {
                var owner=a.heroes.Single(h=>h.id==a.suspendedRun.heroId);
                foreach(var drop in a.suspendedRun.drops)ItemCatalog.Upgrade(drop.item,owner.heroClass);
            }
            a.transactions??=new System.Collections.Generic.List<EconomyReceipt>();
            a.schema=2;ValidateItems(a);
        }
        static void Normalize(AccountSave a)
        {
            if(string.IsNullOrEmpty(a.telemetryAccountId))a.telemetryAccountId=Guid.NewGuid().ToString("N");
            a.records??=new System.Collections.Generic.List<RunRecord>();
            a.records=a.records.Where(r=>r!=null).OrderByDescending(r=>r.journal?.attempt??0).Take(CombatHistory.RecordLimit).ToList();
            RewardBoxes.Normalize(a);
            Attendance.Normalize(a);OfflineSupplies.Normalize(a);
            try{GemInventory.Normalize(a);}catch(Exception error){throw new NotSupportedException(Loc.T("보석 보관함을 안전하게 읽을 수 없어 불러오기를 중단했습니다. 원본 저장 파일은 보존했습니다."),error);}
            a.riftFatigue??=new RiftFatigue();RiftEntryRules.Validate(a.riftFatigue);
            AspectStone.Normalize(a);
            CoreCrafting.Normalize(a);
            RuneGrowth.Normalize(a);
            BlacksmithCatalog.Normalize(a);
            Storage.Normalize(a);
            a.salvage??=new SalvagePreferences();a.salvage.Normalize();
            a.speed=CombatSpeedAccess.Resolve(a.speed);
            ItemAcquisition.NormalizeCounter(a);
            FirstPlayGuide.Normalize(a);
            ContentUnlocks.Normalize(a,legacy:true);
            if(a.gems.Count>0)ContentUnlocks.RecordGemAcquisition(a);
            // JsonUtility materializes a null plain serializable class as an empty object.
            if(a.suspendedRun!=null&&string.IsNullOrEmpty(a.suspendedRun.id))a.suspendedRun=null;
            if(a.suspendedRun!=null)NormalizeRun(a.suspendedRun);
            if(a.records!=null)foreach(var record in a.records)
                {if(record?.review!=null&&record.review.version==0)record.review=null;
                 if(record?.journal!=null&&record.journal.version==0)record.journal=null;}
            a.transactions??=new System.Collections.Generic.List<EconomyReceipt>();
            foreach(var h in a.heroes)
            {
                h.riftProgress??=new RiftEntryProgress();h.riftProgress.best??=new System.Collections.Generic.List<RiftBestTime>();h.riftProgress.claimed??=new System.Collections.Generic.List<int>();
                h.potions??=new PotionInventory();h.potions.Validate();
                if(h.trainingComparison!=null&&string.IsNullOrEmpty(h.trainingComparison.id))h.trainingComparison=null;
                BehaviorRules.Normalize(h.build);
                HuntEdictV2Storage.Normalize(h);
                NormalizePresetSlots(h);
                HuntEdictStorage.Normalize(h);
                if(ClassSkillLoadout.IsAbsent(h.build.classSkills))h.build.classSkills=null;
                else
                {
                    try{ClassSkillLoadout.Validate(h.build.classSkills,h);h.build.classSkills.ProjectLegacy(h.build,null);a.schema=Math.Max(a.schema,7);}
                    catch(Exception error){throw new NotSupportedException("Unsupported class-skill save; the original file is preserved.",error);}
                }
            }
            if(a.suspendedRun!=null)ClassSkillPersistence.ValidateRun(a.suspendedRun,a.heroes.Single(h=>h.id==a.suspendedRun.heroId));
            RepeatHunt.Normalize(a);
            EquipmentShop.Normalize(a);
        }
        static void NormalizePresetSlots(HeroSave hero)
        {
            hero.presets??=new System.Collections.Generic.List<BuildConfig>();
            if(hero.presets.Count>HuntEdict.PresetSlots)throw new NotSupportedException("지원하는 5개보다 많은 프리셋이 있습니다. 원본을 보존하고 불러오기를 중단했습니다.");
            for(int n=0;n<hero.presets.Count;n++)
            {
                var preset=hero.presets[n];
                bool emptyLegacy=preset!=null&&preset.ruleSchema==0&&(preset.rules==null||preset.rules.Count==0)&&
                    (preset.activeSkills==null||preset.activeSkills.Count==0)&&(preset.equipmentIds==null||preset.equipmentIds.Count==0);
                // JsonUtility can materialize null as a default BuildConfig. Persist absence explicitly instead.
                if(!BuildEditing.HasPreset(preset)||emptyLegacy||preset.rules==null||string.IsNullOrEmpty(preset.name)&&string.IsNullOrEmpty(preset.version))
                    hero.presets[n]=new BuildConfig{emptySlot=true,name="",version=""};
                else
                {
                    BehaviorRules.Normalize(preset);
                    if(ClassSkillLoadout.IsAbsent(preset.classSkills))preset.classSkills=null;
                    else try{preset.classSkills=ClassSkillLoadout.Canonical(preset.classSkills);}
                    catch(Exception error){throw new NotSupportedException("Unsupported skill preset; original preserved.",error);}
                }
            }
            while(hero.presets.Count<HuntEdict.PresetSlots)hero.presets.Add(new BuildConfig{emptySlot=true,name="",version=""});
        }
        public static void NormalizeRun(RunState run)
        {
            run.potions??=new PotionRuntimeState();run.potions.Validate();
            if(run.cooldownTotals==null||run.cooldownTotals.Length!=18)run.cooldownTotals=new float[18];
            try{RiftResources.Normalize(run);}catch(Exception error){throw new NotSupportedException(Loc.T("균열의 재화·보석 기록을 안전하게 읽을 수 없어 불러오기를 중단했습니다. 원본 저장 파일은 보존했습니다."),error);}
            if(run.heroAction?.policy?.edictTarget?.version>EdictTargetPolicy.CurrentVersion)
                throw new NotSupportedException("더 새로운 대상 판단 버전이 필요합니다. 저장 파일은 보존했습니다.");
            CombatTelemetry.Normalize(run);
            run.growthEvents??=new System.Collections.Generic.List<GrowthEvent>();
            CombatActions.Normalize(run);
            CombatEffects.Normalize(run);
            if(run.build!=null&&run.build.ruleSchema==0){run.noEnemySince=run.time;run.targetSelectedAt=run.time;}
            BehaviorRules.Normalize(run.build);run.receivedDamage??=new System.Collections.Generic.List<DamageReceived>();run.decisions??=new System.Collections.Generic.List<RuleDecision>();
            if(run.layout!=null&&run.layout.version>RiftLayout.CurrentVersion)throw new NotSupportedException("더 새로운 균열 지도 버전이 필요합니다. 저장 파일은 보존했습니다.");
            if(run.layout==null||run.layout.rooms==null||run.layout.rooms.Count==0)run.layout=RiftLayout.Legacy(run.theme);
            run.exploration??=new RiftExplorationState();run.exploration.enemies??=new System.Collections.Generic.List<EnemyObservation>();
            run.layout.shrines??=new System.Collections.Generic.List<RiftShrine>();run.layout.events??=new System.Collections.Generic.List<RiftEvent>();
            run.layout.gates??=new System.Collections.Generic.List<RiftGate>();
            run.layout.carriers??=new System.Collections.Generic.List<RiftEssenceCarrier>();
            run.layout.offerings??=new System.Collections.Generic.List<RiftOffering>();
            foreach(var chest in run.layout.chests)if(chest.reward!=null&&string.IsNullOrEmpty(chest.reward.id))chest.reward=null;
        }
        public bool SaveTrainingComparison(TrainingComparisonRecord record)
        {
            if(record==null||!record.Complete||record.heroId!=Data.Hero.id||Data.suspendedRun!=null)
            {Error="현재 캐릭터가 완료한 훈련 비교를 성소에서 저장해 주세요.";return false;}
            var hero=Data.Hero;var previous=hero.trainingComparison;
            hero.trainingComparison=JsonUtility.FromJson<TrainingComparisonRecord>(JsonUtility.ToJson(record));
            if(Save())return true;hero.trainingComparison=previous;return false;
        }
        public bool CommitRunMutation(RunState run,string requestId,string operation,Func<RunState,bool> mutation)
        {
            if(Data.suspendedRun!=run){Error="진행 중인 균열이 아닙니다.";return false;}
            using var notifications=DeferNotifications();
            RunState committed=null;
            bool success=Transact(requestId,operation,staged=>{committed=staged.suspendedRun;return mutation(committed);});
            if(!success||committed==null)return success;
            // Keep the root RunState object used by the controller and UI; adopt the committed snapshot.
            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(committed),run);NormalizeRun(run);return true;
        }
        static System.Collections.Generic.IEnumerable<Item> RecordedItems(AccountSave a)
            =>a.records?.Where(r=>r?.review?.equipment!=null).SelectMany(r=>r.review.equipment).Where(i=>i!=null)??Enumerable.Empty<Item>();
        static System.Collections.Generic.IEnumerable<Item> PersistedItems(AccountSave a)
        {
            var items=a.heroes.SelectMany(h=>h.inventory).Concat(a.warehouse).Concat(RecordedItems(a));
            foreach(var run in new[]{a.suspendedRun,a.repeatHunt?.pendingResult})
                if(run!=null)items=items.Concat(run.drops.Select(d=>d.item)).Concat(run.layout.chests.Where(c=>c.reward!=null).Select(c=>c.reward));
            return items.Concat(EquipmentShop.PersistedItems(a)).Concat(a.coreCraft.history.Select(r=>r.item));
        }
        static void ValidateItems(AccountSave a)
        {
            RewardBoxes.Validate(a);
            EquipmentShop.Validate(a);
            CoreCrafting.Validate(a);Attendance.Validate(a.attendance);OfflineSupplies.Validate(a);
            BlacksmithCatalog.Validate(a);
            foreach(var h in a.heroes)h.potions.Validate();
            if(a.gold<0||a.materials<0||a.cores.Any(c=>c<0))throw new InvalidDataException("재화 값이 음수입니다.");
            if(a.heroes.Any(h=>!EquipmentSlots.Valid(h.inventory.Where(i=>i.equipped),h.heroClass)))throw new InvalidDataException("같은 부위에 여러 장비가 장착되어 있습니다.");
            var owned=a.heroes.SelectMany(h=>h.inventory).Concat(a.warehouse).ToArray();
            if(owned.Select(i=>i.id).Distinct().Count()!=owned.Length)throw new InvalidDataException("중복 장비 인스턴스 ID");
            Storage.Validate(a);
            foreach(var i in owned)ItemCatalog.Validate(i);
            foreach(var i in RecordedItems(a))if(i.contentVersion>=ItemQuality.ItemVersion||ItemQuality.HasQuality(i))ItemCatalog.Validate(i);
            if(a.suspendedRun!=null)foreach(var d in a.suspendedRun.drops)ItemCatalog.Validate(d.item);
            if(a.suspendedRun?.layout!=null)foreach(var c in a.suspendedRun.layout.chests)if(c.reward!=null)ItemCatalog.Validate(c.reward);
            if(a.repeatHunt?.pendingResult!=null)
            {foreach(var d in a.repeatHunt.pendingResult.drops)ItemCatalog.Validate(d.item);foreach(var c in a.repeatHunt.pendingResult.layout.chests)if(c.reward!=null)ItemCatalog.Validate(c.reward);}
        }
        public static AccountSave NewAccount(GameCatalog catalog=null)
        {
            var a=new AccountSave{contentUnlocks=new ContentUnlockState{version=ContentUnlocks.Version},lastSeenUtc=DateTimeOffset.UtcNow.ToUnixTimeSeconds()};
            uint rng=112358;
            for(int i=0;i<3;i++)
            {
                var h=new HeroSave{id=Guid.NewGuid().ToString("N"),heroClass=(HeroClass)i,build=GameCatalog.Preset((HeroClass)i,0)};
                if(catalog!=null)h.build=BehaviorPresets.ForLevel(h.heroClass,0,h.level,catalog);
                h.build.passives=Array.Empty<int>();
                h.edict=HuntEdictV2Storage.CreateForHero(h);
                HuntEdictStorage.InitializeNewHero(h,catalog);
                h.potions.Validate();
                var w=Economy.CreateItem(h.heroClass,0,0,1,ref rng);w.baseId=new[]{"B02","B05","B08"}[i];w.baseIndex=i*3+1;w.name=w.DisplayName;w.equipped=true;ItemAcquisition.Stamp(a,w);h.inventory.Add(w);a.heroes.Add(h);
            }
            a.riftFatigue??=new RiftFatigue();RiftEntryRules.Validate(a.riftFatigue);
            AspectStone.Normalize(a);
            RuneGrowth.Normalize(a);RewardBoxes.Normalize(a);Attendance.Normalize(a);OfflineSupplies.Normalize(a);return a;
        }
        public bool SettleLocalIdle()
        {
            pendingLocalIdleThrough??=offlineClock();
            long through=pendingLocalIdleThrough.Value;OfflineSupplies.Normalize(Data);
            var quote=OfflineSupplies.Quote(Data,through);string id="offline-supplies:"+Data.lastSeenUtc+":"+quote.through;
            LocalIdleGoldAwarded=LocalIdleStonesAwarded=0;OfflineMessage="";
            // Retain a failed interval, including fractional income. No other transaction may
            // advance the cursor until this exact interval commits.
            settlingLocalIdle=true;bool success;
            try
            {
                bool accrued=quote.gold>0||quote.stones>0||quote.goldFraction!=Data.offlineSupplies.goldFraction||quote.stoneFraction!=Data.offlineSupplies.stoneFraction;
                if(accrued)success=Transact(id,"offline-supplies",staged=>{OfflineSupplies.Apply(staged,quote,id);return true;});
                else
                {
                    // Persist migrations and backup recovery even at the same timestamp. An empty
                    // financial receipt would suppress that write when its request ID is reused.
                    long previous=Data.lastSeenUtc;Data.lastSeenUtc=quote.through;
                    success=Write(Data);if(!success)Data.lastSeenUtc=previous;else NotifyCommitted("save");
                }
            }
            finally{settlingLocalIdle=false;}
            if(!success)return false;
            pendingLocalIdleThrough=null;LocalIdleGoldAwarded=quote.gold;LocalIdleStonesAwarded=quote.stones;
            if(quote.gold>0||quote.stones>0)OfflineMessage=Loc.F("미접속 보급 · 골드 {0:N0} / 강화석 {1:N0}",quote.gold,quote.stones);
            return true;
        }
        public bool AcknowledgeOfflineSupplies(string id)
            =>Transact("offline-seen:"+id,"offline-seen",a=>
            {if(a.offlineSupplies.receipt?.id!=id)return false;a.offlineSupplies.receipt.acknowledged=true;return true;});
        public bool Save()
        {
            if(pendingLocalIdleThrough.HasValue&&!settlingLocalIdle&&!SettleLocalIdle())return false;
            long previous=Data.lastSeenUtc;Data.lastSeenUtc=SeenNow();
            if(!Write(Data)){Data.lastSeenUtc=previous;return false;}NotifyCommitted("save");return true;
        }
        // Stage the whole account before disk commit. Failed saves never consume the player's inputs.
        public bool Transact(string requestId,string operation,Func<AccountSave,bool> mutation)
        {
            if(pendingLocalIdleThrough.HasValue&&!settlingLocalIdle&&!SettleLocalIdle())return false;
            if(string.IsNullOrWhiteSpace(requestId)||string.IsNullOrWhiteSpace(operation)){Error="거래 식별자가 없습니다.";return false;}
            var receipt=Data.transactions.Find(r=>r.requestId==requestId);
            if(receipt!=null){Error=receipt.operation==operation?"":"같은 거래 요청에 다른 내용이 들어왔습니다.";return receipt.operation==operation;}
            var staged=JsonUtility.FromJson<AccountSave>(JsonUtility.ToJson(Data));
            Normalize(staged);
            try
            {
                if(!mutation(staged)){Error="소유권·보호 상태·재화·가방 공간을 확인해 주세요.";return false;}
                staged.transactions.Add(new EconomyReceipt{requestId=requestId,operation=operation,committedUtc=DateTimeOffset.UtcNow.ToUnixTimeSeconds()});
                ContentUnlocks.Reconcile(staged);ValidateItems(staged);staged.lastSeenUtc=settlingLocalIdle?Math.Max(staged.lastSeenUtc,pendingLocalIdleThrough.Value):SeenNow();
            }
            catch(Exception e){Error=Loc.F("거래를 적용하지 않았습니다: {0}", e.Message);return false;}
            if(!Write(staged))return false;
            // The simulation holds the account and hero objects; retain their identities during portal cleanup.
            for(int i=0;i<Data.heroes.Count;i++)
            {
                var target=Data.heroes[i];var source=staged.heroes[i];
                target.legacyPassiveSlots=source.legacyPassiveSlots;target.level=source.level;target.xp=source.xp;target.highestClear=source.highestClear;target.capacity=source.capacity;
                target.lastRiftFingerprint=source.lastRiftFingerprint;target.lastRiftBoss=source.lastRiftBoss;
                target.riftProgress=source.riftProgress;target.potions=source.potions;target.guide=source.guide;target.slotProgress=source.slotProgress;
                target.equipmentShop=source.equipmentShop;
                target.build=source.build;target.presets=source.presets;target.inventory=source.inventory;target.firstClears=source.firstClears;
            }
            Data.offlineSupplies=staged.offlineSupplies;Data.attendance=staged.attendance;Data.aspects=staged.aspects;Data.enhancementStones=staged.enhancementStones;Data.forge=staged.forge;Data.coreCraft=staged.coreCraft;
            Data.salvage=staged.salvage;Data.schema=staged.schema;Data.contentUnlocks=staged.contentUnlocks;Data.gold=staged.gold;Data.materials=staged.materials;Data.cores=staged.cores;Data.warehouse=staged.warehouse;
            Data.premium=staged.premium;Data.riftFatigue=staged.riftFatigue;Data.warehouseCapacity=staged.warehouseCapacity;Data.warehouseNames=staged.warehouseNames;
            Data.sweepDay=staged.sweepDay;Data.sweepCount=staged.sweepCount;Data.receipts=staged.receipts;Data.transactions=staged.transactions;
            Data.repeatHunt=staged.repeatHunt;Data.records=staged.records;
            Data.telemetryAccountId=staged.telemetryAccountId;Data.combatSequence=staged.combatSequence;Data.combatTelemetryLossCount=staged.combatTelemetryLossCount;
            Data.gems=staged.gems;Data.gemCapacity=staged.gemCapacity;Data.runes=staged.runes;
            Data.rewardBoxes=staged.rewardBoxes;
            Data.lastSeenUtc=staged.lastSeenUtc;Data.itemSequence=staged.itemSequence;Error="";NotifyCommitted(operation);return true;
        }
        public bool CommitChest(RunState run,RiftChest chest)
        {
            if(Data.suspendedRun!=run||run.layout==null||!run.layout.chests.Contains(chest)){Error="진행 중인 균열의 상자가 아닙니다.";return false;}
            if(chest.phase==ChestPhase.Opened)return true;
            using var notifications=DeferNotifications();
            RunState committed=null;
            bool success=Transact(chest.requestId,"chest:"+run.id+":"+chest.id,staged=>
            {committed=staged.suspendedRun;return ChestRewards.Apply(staged,committed,chest.id,false);});
            if(!success)return false;
            if(committed==null){Error="상자 지급 기록과 진행 상태가 다릅니다. 저장된 균열을 다시 불러와 주세요.";return false;}
            var result=committed.layout.chests.Single(c=>c.id==chest.id);
            chest.phase=result.phase;chest.progress=result.progress;chest.dropId=result.dropId;
            run.drops=committed.drops;run.nextId=committed.nextId;run.earnedGold=committed.earnedGold;run.journal=committed.journal;return true;
        }
        bool Write(AccountSave data)
        {
            // Direct build/preset/character writers must not bypass a failed offline interval.
            if(pendingLocalIdleThrough.HasValue&&!settlingLocalIdle)
            {Error=Loc.T("미접속 보급 정산을 먼저 완료해 주세요.");return false;}
            try
            {
                ContentUnlocks.Normalize(data);RewardBoxes.Normalize(data);GemInventory.Normalize(data);OfflineSupplies.Normalize(data);data.schema=MaximumSchemaVersion;
                ContentUnlocks.Reconcile(data);
                data.speed=CombatSpeedAccess.Resolve(data.speed);
                foreach(var hero in data.heroes)
                {
                    NormalizePresetSlots(hero);
                    if(!ClassSkillLoadout.IsAbsent(hero.build.classSkills))ClassSkillLoadout.Validate(hero.build.classSkills,hero);
                }
                // JsonUtility can materialize an absent optional run as an empty object.
                // Only the new ID-bearing runtime requires this additional ownership check.
                if(data.suspendedRun?.build!=null&&!ClassSkillLoadout.IsAbsent(data.suspendedRun.build.classSkills))
                    ClassSkillPersistence.ValidateRun(data.suspendedRun,data.heroes.Single(h=>h.id==data.suspendedRun.heroId));
                // A schema boundary also protects historical quality when every owned bag is empty.
                foreach(var item in PersistedItems(data))
                {
                    if(item.sockets?.Count>0)item.contentVersion=Math.Max(GemCatalog.SocketItemVersion,item.contentVersion);
                    if(ItemQuality.HasQuality(item))item.contentVersion=Math.Max(ItemQuality.ItemVersion,item.contentVersion);
                }
                if(PersistedItems(data).Any(ItemQuality.HasQuality)||RecordedItems(data).Any(i=>i.contentVersion>=ItemQuality.ItemVersion))
                    data.schema=Math.Max(data.schema,MaximumSchemaVersion);
                File.WriteAllText(path+".tmp",JsonUtility.ToJson(data,true));
                if(File.Exists(path))File.Replace(path+".tmp",path,path+".bak");else File.Move(path+".tmp",path);
                CombatArchive.Synchronize(data);
                Error="";return true;
            }
            catch(Exception e)
            {
                Error=Loc.T("저장하지 못했습니다. 저장 공간과 파일 접근 권한을 확인한 뒤 다시 시도해 주세요.");
                Debug.LogError(Loc.F("저장하지 못했습니다: {0}",e.Message));return false;
            }
        }
    }
}
