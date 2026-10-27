using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    [Serializable] public sealed class ContentUnlockState
    {
        public int version;
        public bool firstRunEnded,gemAcquired,coreReady;
        public bool legendaryAcquired,setAcquired;
        public long offlineActivatedUtc;
        public List<string> unlocked=new List<string>();
        public List<string> guidesCompleted=new List<string>();
        // Unlock shows already played. Saves written before this field read as "not seeded": the first Reconcile counts every feature
        // that was already open as introduced, so only content opened afterwards is pending.
        public bool introSeeded,forgeIntroSeeded;
        public List<string> introduced=new List<string>();
    }
    [Serializable] public sealed class ContentUnlockDefinition
    {
        public string id,name,early,guide;
        public int stage;
    }
    [Serializable] public sealed class ContentUnlockData
    {
        public string status;
        public ContentUnlockDefinition[] features;
        public int[] passiveLevels;
        public int expandedEnemiesStage,secondThemeStage,secondBossStage,thirdBossStage,fourthBossStage,fifthBossStage,doubleEliteStage,coreCount;
    }
    public static class ContentUnlocks
    {
        public const string Train="UL01_TRAIN", Enhance="UL02_ENHANCE", Offline="UL03_OFFLINE",
            Gem="UL05_GEM", Reroll="UL06_REROLL", Shop="UL07_UNIDENTIFIED_SHOP", Sweep="UL08_SWEEP", CoreCraft="UL09_CORE_CRAFT",
            SlotEnhance="UL10_SLOT_ENHANCE", Rune="UL11_RUNE", Aspect="UL12_ASPECT", Elixir="UL13_ELIXIR", Masterwork="UL14_MASTERWORK";
        public const int Version=2;
        // Features that get an unlock show on the first town arrival after they open. A new show registers its id here and in GameUI.StartContentIntro.
        public static string[] IntroIds={Rune,Enhance,SlotEnhance,Reroll,CoreCraft};
        static ContentUnlockData data;
        public static ContentUnlockData Rules=>data??=JsonUtility.FromJson<ContentUnlockData>(Resources.Load<TextAsset>("ContentUnlocks").text);
        public static int AccountClear(AccountSave a)=>HeroSlots.Created(a).Select(h=>Math.Max(0,h.highestClear)).DefaultIfEmpty(0).Max();
        public static ContentUnlockDefinition[] AtStage(int stage)=>Rules.features.Where(f=>f.stage==stage).ToArray();
        public static ContentUnlockDefinition NextAfter(int stage)=>Rules.features.Where(f=>f.stage>stage).OrderBy(f=>f.stage).FirstOrDefault();
        public static ContentUnlockDefinition[] NextLocked(AccountSave account)
        {
            int best=AccountClear(account);
            var pending=Rules.features.Where(f=>f.stage>best&&account.contentUnlocks?.unlocked?.Contains(f.id)!=true).OrderBy(f=>f.stage).ToArray();
            return pending.Length==0?Array.Empty<ContentUnlockDefinition>():pending.Where(f=>f.stage==pending[0].stage).ToArray();
        }
        public static string StageSummary(int stage)=>string.Join(" · ",AtStage(stage).Select(f=>Loc.T(f.name)));
        public static int PassiveSlots(HeroSave h)=>Math.Max(Mathf.Clamp(h.legacyPassiveSlots,0,3),Rules.passiveLevels.Count(p=>h.level>=p));
        public static string PassiveError(HeroSave h,BuildConfig b)=>b.passives.Length<=PassiveSlots(h)?"":Loc.F("현재 영웅의 패시브 슬롯은 {0}개입니다.",PassiveSlots(h));
        public static void Normalize(AccountSave a,bool legacy=false)
        {
            if(a.schema>=13&&(a.contentUnlocks==null||a.contentUnlocks.version!=Version))
                throw new NotSupportedException("Unsupported content unlock save version.");
            a.contentUnlocks??=new ContentUnlockState();var s=a.contentUnlocks;
            s.unlocked??=new List<string>();s.guidesCompleted??=new List<string>();s.introduced??=new List<string>();
            if(s.version<0||s.version>Version)throw new NotSupportedException("Unsupported content unlock save version.");
            if(legacy&&s.version==0)
            {
                // These services and three passive slots were available to every schema-1/2 user.
                foreach(string id in new[]{Train,Enhance,Reroll,Shop,Sweep,CoreCraft})Grant(s,id);
                foreach(var h in HeroSlots.Created(a))h.legacyPassiveSlots=3;
                if(AccountClear(a)>0){Grant(s,Offline);s.offlineActivatedUtc=a.lastSeenUtc;}
                s.firstRunEnded=a.records.Count>0||AccountClear(a)>0;
            }
            if(s.version==1||legacy&&s.version==0)PreservePreviousAccess(a);
            s.version=Version;
            Reconcile(a);
        }
        static void PreservePreviousAccess(AccountSave a)
        {
            var s=a.contentUnlocks;int best=AccountClear(a);
            // Freeze the released v1 policy here: future balance edits must not rewrite migration.
            string[] ids={Enhance,Offline,Gem,Reroll,Shop,Sweep,CoreCraft};
            int[] stages={1,1,5,8,10,12,15};
            for(int i=0;i<ids.Length;i++)if(best>=stages[i])Grant(s,ids[i]);
            if(s.gemAcquired||(a.gems?.Count??0)>0)Grant(s,Gem);
            if(s.coreReady||a.cores.Any(c=>c>=10))Grant(s,CoreCraft);
            Grant(s,Rune);Grant(s,Aspect); // Both editors were available from town arrival.
            if(s.unlocked.Contains(Enhance)){Grant(s,SlotEnhance);Grant(s,Masterwork);}
            if(s.unlocked.Contains(Gem))Grant(s,Elixir);
            if(s.unlocked.Contains(Offline)&&s.offlineActivatedUtc==0)s.offlineActivatedUtc=DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }
        static void Grant(ContentUnlockState s,string id){if(!s.unlocked.Contains(id))s.unlocked.Add(id);}
        public static void GrantAll(AccountSave a)
        {
            Reconcile(a);var s=a.contentUnlocks;
            foreach(var f in Rules.features)
            {
                if(s.unlocked.Contains(f.id))continue;
                Grant(s,f.id);if(f.id==Offline)s.offlineActivatedUtc=DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }
            // Opening everything at once is the guide's graduation, which carries no unlock show.
            foreach(var f in Rules.features)if(!ForgeIntroductions.Feature(f.id)||ForgeAccess(a,f.id))Introduce(s,f.id);
        }
        public static void Reconcile(AccountSave a)
        {
            if(a.contentUnlocks?.version!=Version){Normalize(a);return;}
            a.contentUnlocks??=new ContentUnlockState();var s=a.contentUnlocks;
            s.unlocked??=new List<string>();s.guidesCompleted??=new List<string>();s.introduced??=new List<string>();
            if(!s.introSeeded){foreach(string id in s.unlocked)Introduce(s,id);s.introSeeded=true;}
            ForgeIntroductions.Reconcile(a);
            s.coreReady|=a.cores.Any(c=>c>=Rules.coreCount);
            foreach(var item in HeroSlots.Created(a).SelectMany(h=>h.inventory).Concat(a.warehouse))RecordEquipment(a,item);
            int best=AccountClear(a);
            s.firstRunEnded|=best>0;
            foreach(var f in Rules.features)
            {
                bool eligible=best>=f.stage;
                if(!eligible||s.unlocked.Contains(f.id))continue;
                Grant(s,f.id);
                // A feature without a registered show never becomes pending later, when someone registers it.
                if(Array.IndexOf(IntroIds,f.id)<0)Introduce(s,f.id);
                if(f.id==Offline)s.offlineActivatedUtc=DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }
        }
        public static bool Has(AccountSave a,string id){Reconcile(a);return a.contentUnlocks.unlocked.Contains(id);}
        static void Introduce(ContentUnlockState s,string id){if(!s.introduced.Contains(id))s.introduced.Add(id);}
        // The locked-shortcut toast speaks of "Rift Level" like the player does; the number comes from the unlock data.
        public static string LockedToast(string id)=>Loc.F("균열 레벨 {0} 달성 시 오픈됩니다",Rules.features.Single(f=>f.id==id).stage);
        // The unlock show of a registered feature is pending from the moment Reconcile opens it until MarkIntroduced.
        public static bool IntroPending(AccountSave a,string id){Reconcile(a);return Pending(a,id);}
        static bool Pending(AccountSave a,string id)=>Array.IndexOf(IntroIds,id)>=0&&a.contentUnlocks.unlocked.Contains(id)&&
            !a.contentUnlocks.introduced.Contains(id)&&(!ForgeIntroductions.Feature(id)||ForgeAccess(a,id));
        // Pending shows run one at a time in unlock order; null when nothing waits.
        public static string NextIntro(AccountSave a)=>NextIntro(a,null);
        // Town and forge own separate queues, so a forge visit can never block a pending rune introduction.
        public static string NextIntro(AccountSave a,bool? atForge)
        {Reconcile(a);return Rules.features.OrderBy(f=>f.stage).Where(f=>(atForge==null||ForgeIntroductions.Feature(f.id)==atForge.Value)&&Pending(a,f.id)).Select(f=>f.id).FirstOrDefault();}
        public static void MarkIntroduced(AccountSave a,string id){Reconcile(a);Introduce(a.contentUnlocks,id);}
        // The forge keeps each service's rift level even where the guide stage has opened everything: such an account uses a forge service once it has
        // cleared the service's stage. The one exception is the town errand, which teaches the first enhancement before any rift is cleared.
        public static bool ForgeHas(AccountSave a,string id){Reconcile(a);return ForgeAccess(a,id);}
        // Shared by access, pending shows and the one-time forge migration; it never recursively reconciles.
        internal static bool ForgeAccess(AccountSave a,string id)
        {
            if(a.contentUnlocks?.unlocked?.Contains(id)!=true)return false;
            if(a.guide==null||!a.guide.everythingOpen)return true;
            var f=Rules.features.FirstOrDefault(x=>x.id==id);
            return f==null||AccountClear(a)>=f.stage||id==Enhance&&a.guide.townQuest==TownQuest.EnhanceWeapon;
        }
        public static string Condition(string id)
        {
            var f=Rules.features.Single(x=>x.id==id);
            string text=f.stage>0?Loc.F("계정 내 영웅으로 균열 {0}단계 클리어 시 개방",f.stage):Loc.T("마을 도착부터 언제든 이용 가능");
            return string.IsNullOrEmpty(f.early)?text:text+" · "+Loc.T(f.early);
        }
        public static void RecordRunEnd(AccountSave a){a.contentUnlocks.firstRunEnded=true;Reconcile(a);}
        // Call only after the real gem acquisition transaction succeeds; no gem grants are invented here.
        public static void RecordGemAcquisition(AccountSave a){a.contentUnlocks.gemAcquired=true;Reconcile(a);}
        public static void RecordEquipment(AccountSave a,Item item)
        {
            if(a==null||item==null||item.rarity!=3)return;
            a.contentUnlocks??=new ContentUnlockState();
            if(!string.IsNullOrEmpty(ItemCatalog.Unique(item.special)?.setId))a.contentUnlocks.setAcquired=true;
            else a.contentUnlocks.legendaryAcquired=true;
        }
        public static void CompleteGuide(AccountSave a,string id)
        {if((Rules.features.Any(f=>f.id==id)||id=="GUIDE_LEGENDARY"||id=="GUIDE_SET")&&!a.contentUnlocks.guidesCompleted.Contains(id))a.contentUnlocks.guidesCompleted.Add(id);}
        // Adapter boundary for an authenticated account sync, not a server or cloud implementation.
        public static void MergeVerified(AccountSave a,ContentUnlockState incoming)
        {
            Normalize(a);if(incoming==null)return;
            if(incoming.version<0||incoming.version>Version)throw new NotSupportedException("Unsupported content unlock save version.");
            if(incoming.version<Version)
            {
                var previous=new AccountSave{contentUnlocks=JsonUtility.FromJson<ContentUnlockState>(JsonUtility.ToJson(incoming))};
                Normalize(previous,legacy:true);incoming=previous.contentUnlocks;
            }
            foreach(var f in Rules.features)if(incoming.unlocked?.Contains(f.id)==true)Grant(a.contentUnlocks,f.id);
            foreach(var f in Rules.features)if(incoming.guidesCompleted?.Contains(f.id)==true)CompleteGuide(a,f.id);
            foreach(var f in Rules.features)if(incoming.introduced?.Contains(f.id)==true&&(!ForgeIntroductions.Feature(f.id)||incoming.forgeIntroSeeded))Introduce(a.contentUnlocks,f.id);
            a.contentUnlocks.firstRunEnded|=incoming.firstRunEnded;a.contentUnlocks.gemAcquired|=incoming.gemAcquired;a.contentUnlocks.coreReady|=incoming.coreReady;
            a.contentUnlocks.legendaryAcquired|=incoming.legendaryAcquired;a.contentUnlocks.setAcquired|=incoming.setAcquired;
            foreach(string id in new[]{"GUIDE_LEGENDARY","GUIDE_SET"})if(incoming.guidesCompleted?.Contains(id)==true)CompleteGuide(a,id);
            // Never backdate idle eligibility from a client sync timestamp.
            if(a.contentUnlocks.unlocked.Contains(Offline)&&a.contentUnlocks.offlineActivatedUtc==0)a.contentUnlocks.offlineActivatedUtc=DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            Reconcile(a);
        }
        public static int ThemeCount(int stage)=>stage>=Rules.secondThemeStage?2:1;
        public static int BossCount(int stage)=>Unlocked(Rules.fifthBossStage,stage)?5:Unlocked(Rules.fourthBossStage,stage)?4:stage>=Rules.thirdBossStage?3:stage>=Rules.secondBossStage?2:1;
        // A stage missing from older data (0) keeps that boss locked.
        static bool Unlocked(int unlockStage,int stage)=>unlockStage>0&&stage>=unlockStage;
        public static int EliteTraits(int stage)=>stage>=Rules.doubleEliteStage?2:1;
    }
}
