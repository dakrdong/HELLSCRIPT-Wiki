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
        public int expandedEnemiesStage,secondThemeStage,secondBossStage,thirdBossStage,doubleEliteStage,coreCount;
    }
    public static class ContentUnlocks
    {
        public const string Train="UL01_TRAIN", Enhance="UL02_ENHANCE", Offline="UL03_OFFLINE", RareCraft="UL04_RARE_CRAFT",
            Gem="UL05_GEM", Reroll="UL06_REROLL", Shop="UL07_UNIDENTIFIED_SHOP", Sweep="UL08_SWEEP", CoreCraft="UL09_CORE_CRAFT";
        static ContentUnlockData data;
        public static ContentUnlockData Rules=>data??=JsonUtility.FromJson<ContentUnlockData>(Resources.Load<TextAsset>("ContentUnlocks").text);
        public static int AccountClear(AccountSave a)=>a.heroes.Count==0?0:a.heroes.Max(h=>Math.Max(0,h.highestClear));
        public static int PassiveSlots(HeroSave h)=>Math.Max(Mathf.Clamp(h.legacyPassiveSlots,0,3),Rules.passiveLevels.Count(p=>h.level>=p));
        public static string PassiveError(HeroSave h,BuildConfig b)=>b.passives.Length<=PassiveSlots(h)?"":Loc.F("현재 영웅의 패시브 슬롯은 {0}개입니다.",PassiveSlots(h));
        public static void Normalize(AccountSave a,bool legacy=false)
        {
            a.contentUnlocks??=new ContentUnlockState();var s=a.contentUnlocks;
            s.unlocked??=new List<string>();s.guidesCompleted??=new List<string>();
            if(s.version>1)throw new NotSupportedException("Unsupported content unlock save version.");
            if(legacy&&s.version==0)
            {
                // These services and three passive slots were available to every schema-1/2 user.
                foreach(string id in new[]{Train,Enhance,RareCraft,Reroll,Shop,Sweep,CoreCraft})Grant(s,id);
                foreach(var h in a.heroes)h.legacyPassiveSlots=3;
                if(AccountClear(a)>0){Grant(s,Offline);s.offlineActivatedUtc=a.lastSeenUtc;}
                s.firstRunEnded=a.records.Count>0||AccountClear(a)>0;
            }
            s.version=1;
            Reconcile(a);
        }
        static void Grant(ContentUnlockState s,string id){if(!s.unlocked.Contains(id))s.unlocked.Add(id);}
        public static void Reconcile(AccountSave a)
        {
            a.contentUnlocks??=new ContentUnlockState();var s=a.contentUnlocks;
            s.unlocked??=new List<string>();s.guidesCompleted??=new List<string>();
            s.coreReady|=a.cores.Any(c=>c>=Rules.coreCount);
            foreach(var item in a.heroes.SelectMany(h=>h.inventory).Concat(a.warehouse))RecordEquipment(a,item);
            int best=AccountClear(a);
            s.firstRunEnded|=best>0;
            foreach(var f in Rules.features)
            {
                bool eligible=f.stage>0&&best>=f.stage||f.id==Train&&s.firstRunEnded||f.id==Gem&&s.gemAcquired||f.id==CoreCraft&&s.coreReady;
                if(!eligible||s.unlocked.Contains(f.id))continue;
                Grant(s,f.id);
                if(f.id==Offline)s.offlineActivatedUtc=DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }
        }
        public static bool Has(AccountSave a,string id){Reconcile(a);return a.contentUnlocks.unlocked.Contains(id);}
        public static string Condition(string id)
        {
            var f=Rules.features.Single(x=>x.id==id);
            string text=f.stage>0?Loc.F("계정 내 영웅으로 균열 {0}단계 클리어 시 개방",f.stage):Loc.T("첫 균열 시도 종료 후 개방 · 실패해도 이용 가능");
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
            if(incoming.version>1)throw new NotSupportedException("Unsupported content unlock save version.");
            foreach(var f in Rules.features)if(incoming.unlocked?.Contains(f.id)==true)Grant(a.contentUnlocks,f.id);
            foreach(var f in Rules.features)if(incoming.guidesCompleted?.Contains(f.id)==true)CompleteGuide(a,f.id);
            a.contentUnlocks.firstRunEnded|=incoming.firstRunEnded;a.contentUnlocks.gemAcquired|=incoming.gemAcquired;a.contentUnlocks.coreReady|=incoming.coreReady;
            a.contentUnlocks.legendaryAcquired|=incoming.legendaryAcquired;a.contentUnlocks.setAcquired|=incoming.setAcquired;
            foreach(string id in new[]{"GUIDE_LEGENDARY","GUIDE_SET"})if(incoming.guidesCompleted?.Contains(id)==true)CompleteGuide(a,id);
            // Never backdate idle eligibility from a client sync timestamp.
            if(a.contentUnlocks.unlocked.Contains(Offline)&&a.contentUnlocks.offlineActivatedUtc==0)a.contentUnlocks.offlineActivatedUtc=DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            Reconcile(a);
        }
        public static int ThemeCount(int stage)=>stage>=Rules.secondThemeStage?2:1;
        public static int BossCount(int stage)=>stage>=Rules.thirdBossStage?3:stage>=Rules.secondBossStage?2:1;
        public static int EliteTraits(int stage)=>stage>=Rules.doubleEliteStage?2:1;
    }
}
