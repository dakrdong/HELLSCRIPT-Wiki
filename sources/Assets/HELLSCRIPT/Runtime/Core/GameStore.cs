using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    // Development adapter. Production account ownership and server-time settlement are a separate boundary.
    public sealed partial class GameStore
    {
        public const int MaximumSchemaVersion=4;
        public AccountSave Data {get;private set;}
        public string Error {get;private set;}="";
        public string OfflineMessage {get;private set;}="";
        public string GemRecoveryMessage {get;private set;}="";
        public string GemRecoveryArchive {get;private set;}="";
        public string QualityRecoveryMessage {get;private set;}="";
        public string QualityRecoveryArchive {get;private set;}="";
        public int LocalIdleGoldAwarded {get;private set;}
        public int LocalIdleMaterialsAwarded {get;private set;}
        long? pendingLocalIdleThrough;
        bool settlingLocalIdle;
        readonly string path;
        public GameStore(string directory,GameCatalog catalog=null)
        {
            Directory.CreateDirectory(directory);path=Path.Combine(directory,"hellscript-local-v1.json");
            string source=path;
            Data=Read(path);
            if(Data==null){source=path+".bak";Data=Read(source);}
            if(Data==null&&(File.Exists(path)||File.Exists(path+".bak")))throw new InvalidDataException(Loc.F("저장 파일과 백업을 복구할 수 없습니다. 원본은 그대로 보존했습니다.\n{0}", path));
            if(Data==null)Data=NewAccount(catalog);
            else if(Data.schema==1)
            {
                File.Copy(source,path+".schema1-"+DateTime.UtcNow.Ticks+".json",false);
                Migrate(Data);
            }
            ContentUnlocks.Normalize(Data);
            SettleLocalIdle();
        }
        AccountSave Read(string file)
        {
            try
            {
                if(!File.Exists(file))return null;var a=JsonUtility.FromJson<AccountSave>(File.ReadAllText(file));
                if(a!=null&&a.schema>MaximumSchemaVersion)throw new NotSupportedException("이 저장 파일은 더 새로운 게임 버전이 필요합니다. 원본을 보존하고 불러오기를 중단했습니다.");
                if(a==null||a.schema<1||a.heroes==null||a.heroes.Count!=3||a.cores==null||a.cores.Length!=8)return null;
                if(a.heroes.Any(h=>h==null||h.build==null||h.inventory==null||h.level<1||h.level>30))return null;
                Normalize(a);
                if(a.schema>=2)
                {
                    var allItems=PersistedItems(a);
                    if(allItems.Any(i=>i.contentVersion>ItemCatalog.Version))throw new NotSupportedException("더 새로운 장비 데이터가 있어 불러오기를 중단했습니다. 저장 파일은 보존했습니다.");
                    var socketItems=allItems.ToArray();
                    if(socketItems.Any(i=>i.sockets!=null&&i.sockets.Count>0&&(!GemCatalog.AllowsSocket(i)||i.sockets.Count>1||i.sockets[0]==null||i.sockets[0].index!=0)))
                        throw new NotSupportedException(Loc.T("소켓 구조를 안전하게 읽을 수 없어 불러오기를 중단했습니다. 저장 파일은 보존했습니다."));
                    bool repaired=false,qualityRepaired=false;foreach(var item in socketItems)
                    {repaired|=GemCatalog.RepairGemValues(item);qualityRepaired|=ItemQuality.RepairValues(item);}
                    ValidateItems(a);
                    if(repaired||qualityRepaired)
                    {
                        string archive=file+(repaired?".gem-recovery-":".quality-recovery-")+Guid.NewGuid().ToString("N")+".json";
                        try{File.Copy(file,archive,false);}
                        catch(Exception error){throw new NotSupportedException(Loc.T(repaired?"보석 복구 원본을 보관하지 못해 불러오기를 중단했습니다. 저장 파일은 보존했습니다.":"품질 복구 원본을 보관하지 못해 불러오기를 중단했습니다. 저장 파일은 보존했습니다."),error);}
                        if(repaired){GemRecoveryArchive=archive;GemRecoveryMessage=Loc.T("잘못된 보석 값을 빈 소켓으로 복구했습니다. 장비와 원본 저장 파일은 보존했습니다.");}
                        if(qualityRepaired){QualityRecoveryArchive=archive;QualityRecoveryMessage=Loc.T("잘못된 장비 품질 기록을 복구했습니다. 장비와 투자 원장, 원본 저장 파일은 보존했습니다.");}
                    }
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
            try{GemInventory.Normalize(a);}catch(Exception error){throw new NotSupportedException(Loc.T("보석 보관함을 안전하게 읽을 수 없어 불러오기를 중단했습니다. 원본 저장 파일은 보존했습니다."),error);}
            RuneGrowth.Normalize(a);
            a.speed=CombatSpeedAccess.Resolve(a.speed);
            ItemAcquisition.NormalizeCounter(a);
            FirstPlayGuide.Normalize(a);
            ContentUnlocks.Normalize(a,legacy:true);
            if(a.gems.Count>0)ContentUnlocks.RecordGemAcquisition(a);
            // JsonUtility materializes a null plain serializable class as an empty object.
            if(a.suspendedRun!=null&&string.IsNullOrEmpty(a.suspendedRun.id))a.suspendedRun=null;
            if(a.suspendedRun!=null)NormalizeRun(a.suspendedRun);
            if(a.records!=null)foreach(var record in a.records)
                if(record?.review!=null&&record.review.version==0)record.review=null;
            a.transactions??=new System.Collections.Generic.List<EconomyReceipt>();
            foreach(var h in a.heroes)
            {
                if(h.trainingComparison!=null&&string.IsNullOrEmpty(h.trainingComparison.id))h.trainingComparison=null;
                BehaviorRules.Normalize(h.build);
                HuntEdictV2Storage.Normalize(h);
                NormalizePresetSlots(h);
            }
            RepeatHunt.Normalize(a);
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
                else BehaviorRules.Normalize(preset);
            }
            while(hero.presets.Count<HuntEdict.PresetSlots)hero.presets.Add(new BuildConfig{emptySlot=true,name="",version=""});
        }
        public static void NormalizeRun(RunState run)
        {
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
            return items;
        }
        static void ValidateItems(AccountSave a)
        {
            if(a.gold<0||a.materials<0||a.cores.Any(c=>c<0))throw new InvalidDataException("재화 값이 음수입니다.");
            if(a.heroes.Any(h=>h.inventory.Where(i=>i.equipped).GroupBy(i=>i.slot).Any(g=>g.Count()>1)))throw new InvalidDataException("같은 부위에 여러 장비가 장착되어 있습니다.");
            var owned=a.heroes.SelectMany(h=>h.inventory).Concat(a.warehouse).ToArray();
            if(owned.Select(i=>i.id).Distinct().Count()!=owned.Length)throw new InvalidDataException("중복 장비 인스턴스 ID");
            foreach(var i in owned)ItemCatalog.Validate(i);
            foreach(var i in RecordedItems(a))if(i.contentVersion>=ItemQuality.ItemVersion||ItemQuality.HasQuality(i))ItemCatalog.Validate(i);
            if(a.suspendedRun!=null)foreach(var d in a.suspendedRun.drops)ItemCatalog.Validate(d.item);
            if(a.suspendedRun?.layout!=null)foreach(var c in a.suspendedRun.layout.chests)if(c.reward!=null)ItemCatalog.Validate(c.reward);
            if(a.repeatHunt?.pendingResult!=null)
            {foreach(var d in a.repeatHunt.pendingResult.drops)ItemCatalog.Validate(d.item);foreach(var c in a.repeatHunt.pendingResult.layout.chests)if(c.reward!=null)ItemCatalog.Validate(c.reward);}
        }
        public static AccountSave NewAccount(GameCatalog catalog=null)
        {
            var a=new AccountSave{contentUnlocks=new ContentUnlockState{version=1},lastSeenUtc=DateTimeOffset.UtcNow.ToUnixTimeSeconds()};
            uint rng=112358;
            for(int i=0;i<3;i++)
            {
                var h=new HeroSave{id=Guid.NewGuid().ToString("N"),heroClass=(HeroClass)i,build=GameCatalog.Preset((HeroClass)i,0)};
                if(catalog!=null)h.build=BehaviorPresets.ForLevel(h.heroClass,0,h.level,catalog);
                h.build.passives=Array.Empty<int>();
                h.edict=HuntEdictV2Storage.CreateForHero(h);
                var w=Economy.CreateItem(h.heroClass,0,0,1,ref rng);w.baseId=new[]{"B02","B05","B08"}[i];w.baseIndex=i*3+1;w.name=w.DisplayName;w.equipped=true;ItemAcquisition.Stamp(a,w);h.inventory.Add(w);a.heroes.Add(h);
            }
            RuneGrowth.Normalize(a);return a;
        }
        public bool SettleLocalIdle()
        {
            pendingLocalIdleThrough??=DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long through=pendingLocalIdleThrough.Value;int best=Data.heroes.Max(h=>h.highestClear),gold=0,materials=0;
            LocalIdleGoldAwarded=LocalIdleMaterialsAwarded=0;OfflineMessage="";
            if(ContentUnlocks.Has(Data,ContentUnlocks.Offline)&&best>0&&Data.lastSeenUtc>0)
            {
                double hours=Math.Min(12,Math.Max(0,through-Math.Max(Data.lastSeenUtc,Data.contentUnlocks.offlineActivatedUtc))/3600d);
                gold=(int)(hours*(200+40*best));materials=(int)(hours*(5+best/5));
            }
            // Keep the failed interval fixed. Neither another save nor an inventory transaction
            // can move its cursor forward before its rewards commit successfully.
            settlingLocalIdle=true;bool success;
            try
            {
                success=gold>0||materials>0?Transact("local-idle:"+Data.lastSeenUtc+":"+through,"local-idle",staged=>
                {staged.gold=checked(staged.gold+gold);staged.materials=checked(staged.materials+materials);return true;}):Save();
            }
            finally{settlingLocalIdle=false;}
            if(!success)return false;
            pendingLocalIdleThrough=null;LocalIdleGoldAwarded=gold;LocalIdleMaterialsAwarded=materials;
            if(gold>0||materials>0)OfflineMessage=Loc.F("미실행 보상 · 골드 {0:N0} / 재료 {1}",gold,materials);
            return true;
        }
        public bool Save()
        {
            if(pendingLocalIdleThrough.HasValue&&!settlingLocalIdle&&!SettleLocalIdle())return false;
            Data.lastSeenUtc=DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return Write(Data);
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
                ContentUnlocks.Reconcile(staged);ValidateItems(staged);staged.lastSeenUtc=DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }
            catch(Exception e){Error=Loc.F("거래를 적용하지 않았습니다: {0}", e.Message);return false;}
            if(!Write(staged))return false;
            // The simulation holds the account and hero objects; retain their identities during portal cleanup.
            for(int i=0;i<Data.heroes.Count;i++)
            {
                var target=Data.heroes[i];var source=staged.heroes[i];
                target.legacyPassiveSlots=source.legacyPassiveSlots;target.level=source.level;target.xp=source.xp;target.highestClear=source.highestClear;target.capacity=source.capacity;
                target.lastRiftFingerprint=source.lastRiftFingerprint;target.lastRiftBoss=source.lastRiftBoss;
                target.build=source.build;target.presets=source.presets;target.inventory=source.inventory;target.firstClears=source.firstClears;
            }
            Data.schema=staged.schema;Data.contentUnlocks=staged.contentUnlocks;Data.gold=staged.gold;Data.materials=staged.materials;Data.cores=staged.cores;Data.warehouse=staged.warehouse;
            Data.sweepDay=staged.sweepDay;Data.sweepCount=staged.sweepCount;Data.receipts=staged.receipts;Data.transactions=staged.transactions;
            Data.repeatHunt=staged.repeatHunt;
            Data.gems=staged.gems;Data.gemCapacity=staged.gemCapacity;Data.runes=staged.runes;
            Data.lastSeenUtc=staged.lastSeenUtc;Data.itemSequence=staged.itemSequence;Error="";return true;
        }
        public bool CommitChest(RunState run,RiftChest chest)
        {
            if(Data.suspendedRun!=run||run.layout==null||!run.layout.chests.Contains(chest)){Error="진행 중인 균열의 상자가 아닙니다.";return false;}
            if(chest.phase==ChestPhase.Opened)return true;
            RunState committed=null;
            bool success=Transact(chest.requestId,"chest:"+run.id+":"+chest.id,staged=>
            {committed=staged.suspendedRun;return ChestRewards.Apply(staged,committed,chest.id,false);});
            if(!success)return false;
            if(committed==null){Error="상자 지급 기록과 진행 상태가 다릅니다. 저장된 균열을 다시 불러와 주세요.";return false;}
            var result=committed.layout.chests.Single(c=>c.id==chest.id);
            chest.phase=result.phase;chest.progress=result.progress;chest.dropId=result.dropId;
            run.drops=committed.drops;run.nextId=committed.nextId;run.earnedGold=committed.earnedGold;return true;
        }
        bool Write(AccountSave data)
        {
            try
            {
                GemInventory.Normalize(data);data.schema=MaximumSchemaVersion;
                ContentUnlocks.Reconcile(data);
                data.speed=CombatSpeedAccess.Resolve(data.speed);
                foreach(var hero in data.heroes)NormalizePresetSlots(hero);
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
