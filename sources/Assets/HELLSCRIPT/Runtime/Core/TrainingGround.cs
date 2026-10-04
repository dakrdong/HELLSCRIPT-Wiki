using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    // One enemy row of a training ground setup. A boss row uses kind 0-4 (B01-B05) and is always a single boss;
    // a normal row uses kind 0-19 (N01-N20) and 1-20 monsters of one tier. Elite is a real rift elite; Magic and
    // Legendary are visual tiers, as they are in rifts, so they keep normal stats.
    [Serializable] public sealed class TrainingGroundEnemy
    {
        public bool boss;
        public int kind,count=1;
        public MonsterTier tier=MonsterTier.Normal;
    }
    [Serializable] public sealed class TrainingGroundSetup
    {
        public int stage=1;
        public List<TrainingGroundEnemy> enemies=new List<TrainingGroundEnemy>();
        // Equipped skills unchecked for training. They stay equipped; the run turns their automatic use off.
        public List<string> excluded=new List<string>();
        public TrainingGroundSetup Copy()=>JsonUtility.FromJson<TrainingGroundSetup>(JsonUtility.ToJson(this));
    }
    // A successful run. The previous run of a setup keeps its damage per second and loadout for the next comparison;
    // the best run keeps only its numbers.
    [Serializable] public sealed class TrainingGroundResult
    {
        public string runId="";
        public float seconds,averageDps,peakDps;
        public double damage;
        public List<int> damagePerSecond=new List<int>();
        public ClassSkillLoadout loadout;
        // The recommended defaults were switched on; the loadout is then the defaults combat used.
        public bool recommended;
        public long completedUtc;
        public bool HasLoadout=>ClassSkillTree.Uses(loadout);
        public bool Recorded=>!string.IsNullOrEmpty(runId)&&seconds>0;
    }
    [Serializable] public sealed class TrainingGroundRecord
    {
        public string key="";
        public TrainingGroundResult previous=new TrainingGroundResult(),best=new TrainingGroundResult();
    }
    [Serializable] public sealed class TrainingGroundProgress
    {
        public TrainingGroundSetup setup=new TrainingGroundSetup();
        public List<TrainingGroundRecord> records=new List<TrainingGroundRecord>();
    }
    // One release of an equipped skill, at the combat time it happened.
    [Serializable] public sealed class TrainingGroundCast
    {
        public string id="";
        public float t;
    }
    // The training meter's samples are also used by rift results. They survive checkpoints and own every release time.
    [Serializable] public class CombatDpsTimeline
    {
        public const int Version=1;
        public int version;
        public List<float> damage=new List<float>();
        public List<TrainingGroundCast> casts=new List<TrainingGroundCast>();
        // JsonUtility materializes a serialized null as an empty version-zero object.
        // Existing samples prove an older graph existed; empty placeholders do not.
        public static CombatDpsTimeline Normalize(CombatDpsTimeline timeline)
        {
            if(timeline==null)return null;
            if(timeline.version==0)
            {
                if((timeline.damage?.Count??0)==0&&(timeline.casts?.Count??0)==0)return null;
                timeline.version=Version;
            }
            if(timeline.version!=Version)throw new NotSupportedException("Unsupported combat graph version.");
            timeline.damage??=new List<float>();timeline.casts??=new List<TrainingGroundCast>();
            return timeline;
        }
    }
    [Serializable] public sealed class TrainingGroundRunState:CombatDpsTimeline
    {
        public TrainingGroundSetup setup=new TrainingGroundSetup();
        public string key="";
    }

    public static class TrainingGround
    {
        public const int Training=4,MaxKinds=5,MaxNormal=20,NormalKinds=20,BossKinds=5,RecordLimit=12;
        public const float DpsWindow=3;
        // Pack centres in the start room, measured from its middle. The hero enters at (0,-4).
        static readonly Vector2[] Anchors={new Vector2(0,4.2f),new Vector2(-4.6f,3),new Vector2(4.6f,3),new Vector2(-4.8f,-1.8f),new Vector2(4.8f,-1.8f)};

        public static int MaxStage(HeroSave hero)=>Mathf.Clamp(hero.highestClear+1,1,1000);
        // The approved mockup's starting roster, at the hero's highest cleared tier.
        public static TrainingGroundSetup Default(HeroSave hero)=>new TrainingGroundSetup{stage=Mathf.Clamp(hero.highestClear,1,MaxStage(hero)),
            enemies=new List<TrainingGroundEnemy>{new TrainingGroundEnemy{boss=true,kind=0},new TrainingGroundEnemy{kind=0,count=12},
                new TrainingGroundEnemy{kind=2,count=6,tier=MonsterTier.Magic},new TrainingGroundEnemy{kind=7,count=4,tier=MonsterTier.Elite}}};
        public static string Validate(TrainingGroundSetup setup,HeroSave hero)
        {
            if(setup?.enemies==null||setup.excluded==null||hero==null)return Loc.T("훈련장 설정을 읽을 수 없습니다.");
            if(setup.stage<1||setup.stage>MaxStage(hero))return Loc.F("균열 단계는 1~{0}단계에서 고를 수 있습니다.",MaxStage(hero));
            if(setup.enemies.Count==0)return Loc.T("훈련할 적을 한 종류 이상 추가해 주세요.");
            if(setup.enemies.Count>MaxKinds)return Loc.F("적은 최대 {0}종까지 추가할 수 있습니다.",MaxKinds);
            if(setup.enemies.Count(e=>e?.boss==true)>1)return Loc.T("보스는 한 종류만 추가할 수 있습니다.");
            foreach(var e in setup.enemies)
            {
                bool valid=e!=null&&(e.boss?e.kind>=0&&e.kind<BossKinds&&e.count==1&&e.tier==MonsterTier.Normal:
                    e.kind>=0&&e.kind<NormalKinds&&e.count>=1&&e.count<=MaxNormal&&(e.tier==MonsterTier.Normal||e.tier==MonsterTier.Magic||e.tier==MonsterTier.Elite||e.tier==MonsterTier.Legendary));
                if(!valid)return Loc.T("적 구성을 확인해 주세요.");
            }
            if(setup.enemies.Where(e=>!e.boss).GroupBy(e=>e.kind).Any(g=>g.Count()>1))return Loc.T("같은 몬스터는 한 줄에 모아 주세요.");
            return "";
        }
        // The equipped actives and the ultimate, in slot order.
        public static string[] Equipped(ClassSkillLoadout load)=>load.actives.Concat(new[]{load.ultimate}).Where(id=>!string.IsNullOrEmpty(id)).ToArray();
        public static string[] UsedSkills(TrainingGroundSetup setup,ClassSkillLoadout load)=>Equipped(load).Where(id=>!setup.excluded.Contains(id)).ToArray();
        // Normal rows in spawn order. Sorting by kind makes the arena depend on the set of rows, not the order they were added in.
        public static List<TrainingGroundEnemy> Packs(TrainingGroundSetup setup)=>setup.enemies.Where(e=>!e.boss).OrderBy(e=>e.kind).ToList();
        public static TrainingGroundEnemy Boss(TrainingGroundSetup setup)=>setup.enemies.FirstOrDefault(e=>e.boss);
        public static string ArenaKey(TrainingGroundSetup setup)=>"R"+setup.stage+"|"+string.Join(",",setup.enemies.OrderBy(e=>e.boss?0:1).ThenBy(e=>e.kind)
            .Select(e=>e.boss?"B"+(e.kind+1).ToString("00"):"N"+(e.kind+1).ToString("00")+":"+e.tier+"x"+e.count));
        // The same setup: hero level, equipped items, rift tier, enemies and checked skills. The Hunt Edict is what gets tested, so it is left out.
        public static string Key(TrainingGroundSetup setup,HeroSave hero)=>ArenaKey(setup)+"|L"+hero.level+"|E"+EquipmentHash(hero).ToString("x8")+"|S"+
            string.Join(",",UsedSkills(setup,ClassSkillTree.FromHero(hero)).OrderBy(id=>id,StringComparer.Ordinal));
        public static uint Seed(TrainingGroundSetup setup)=>RiftGenerator.Derive(731014u,ArenaKey(setup));
        static uint EquipmentHash(HeroSave hero)
        {
            // Bag bookkeeping (review marks, locks, storage places) never changes a fight.
            var items=TrainingEquipmentSnapshot.Capture(hero);
            foreach(var i in items){i.reviewed=i.locked=i.pendingAutoStorage=false;i.storageTab=i.storageSlot=-1;i.origin="";i.acquiredOrder=i.warehouseOrder=0;}
            return RiftGenerator.Derive(0,string.Join("\n",items.Select(i=>JsonUtility.ToJson(i))));
        }
        // The traits each member of an elite row carries, by the rift's own rule. They are drawn from the row alone, so
        // the lobby can show them and changing another row never rerolls them.
        public static List<List<int>> EliteTraits(int stage,TrainingGroundEnemy entry,out bool linked)
        {
            var traits=new List<List<int>>();linked=false;bool elite=!entry.boss&&entry.tier==MonsterTier.Elite;
            uint rng=RiftGenerator.Derive(731014u,"elite|"+stage+"|"+entry.kind+"|"+entry.count);
            for(int n=0;n<entry.count;n++)traits.Add(elite?RiftGenerator.RollEliteTraits(stage,entry.count,ref rng):new List<int>());
            linked=elite&&entry.count==2&&RiftGenerator.RollElitePair(ref rng);
            if(linked)foreach(var t in traits){t.Clear();t.Add(2);}
            return traits;
        }
        // Member n of pack k: a sunflower spiral about the pack centre, pulled inward if a point falls off the floor.
        public static Vector2 PackPoint(int pack,int member,RiftNavigation map)
        {
            Vector2 centre=RiftMap.Rooms[0]+Anchors[pack];float angle=member*2.3999632f,radius=.47f*Mathf.Sqrt(member+.5f);
            var offset=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius;
            for(float scale=1;scale>0;scale-=.25f)if(map.CanLand(centre+offset*scale,.4f))return centre+offset*scale;
            return centre;
        }

        // Skills on a real cooldown get an icon on the graph; rotation fillers without one (Whirlwind, Crushing Strike) would bury
        // the line, so they only appear by name when the graph is inspected. Ultimates always get an icon.
        public const float MarkCooldown=3;
        public static bool MarkedOnGraph(string id){var skill=ClassSkills.Find(id);return skill!=null&&(skill.Ultimate||skill.Number("cooldownSeconds")>=MarkCooldown);}
        // The graph point a cast belongs to: the whole second it happened in, the same index as the damage series.
        public static int CastSecond(TrainingGroundCast cast)=>Mathf.Max(0,Mathf.FloorToInt(cast.t));
        public static List<TrainingGroundCast> CastsIn(IReadOnlyList<TrainingGroundCast> casts,int second)=>casts.Where(c=>CastSecond(c)==second).ToList();

        // Damage per second over the last whole seconds before `second`; the live meter and the peak both read it.
        public static float WindowDps(IReadOnlyList<float> series,int second)
        {
            int end=Mathf.Min(second,series.Count),start=Mathf.Max(0,second-(int)DpsWindow);float sum=0;
            for(int i=start;i<end;i++)sum+=series[i];
            return second<=0?0:sum/Mathf.Min(DpsWindow,second);
        }
        public static float WindowDps(IReadOnlyList<int> series,int second)=>WindowDps(series.Select(v=>(float)v).ToList(),second);
        public static TrainingGroundResult Capture(RunState run,HeroSave hero)
        {
            var result=CaptureDps(run.id,run.time,run.trainingGround.damage);
            result.loadout=Effective(hero);result.recommended=hero.useRecommendedEdict;result.completedUtc=DateTimeOffset.UtcNow.ToUnixTimeSeconds();return result;
        }
        public static TrainingGroundResult CaptureDps(string id,float seconds,IReadOnlyList<float> series)
        {
            double total=0;foreach(float value in series)total+=value;
            float peak=0;for(int second=1;second<=series.Count;second++)peak=Mathf.Max(peak,WindowDps(series,second));
            return new TrainingGroundResult{runId=id,seconds=seconds,damage=total,averageDps=seconds>0?(float)(total/seconds):0,peakDps=peak,
                damagePerSecond=series.Select(Mathf.RoundToInt).ToList()};
        }
        // The loadout combat uses: the recommended defaults while they are switched on, otherwise the player's own.
        public static ClassSkillLoadout Effective(HeroSave hero){var load=ClassSkillTree.FromHero(hero);return hero.useRecommendedEdict?HuntEdictDefaults.Skills(load):load;}
        public static bool Automatic(HeroSave hero,string id)=>Effective(hero).automatic.Contains(id);
        // A skill's edict as the lobby and the result show it.
        public static string SkillEdictLine(HeroSave hero,string id)=>hero.useRecommendedEdict?Loc.T("기본 추천 세팅"):EdictLine(id,HuntEdictLoadout.FromHero(hero));
        // The change list between two recorded runs, led by a switch of the recommended defaults.
        public static List<string> DescribeChanges(TrainingGroundResult before,ClassSkillLoadout after,bool recommended)
        {
            var changes=DescribeChanges(before.loadout,after);
            if(before.recommended!=recommended)changes.Insert(0,Loc.F("기본 세팅 적용: {0} → {1}",before.recommended?"ON":"OFF",recommended?"ON":"OFF"));
            return changes;
        }
        // A stored loadout as the edict window reads it. Throws for a loadout this build cannot read.
        public static HuntEdictLoadout Edict(ClassSkillLoadout load)=>HuntEdictLoadout.Canonical(new HuntEdictLoadout{version=2,edict=load.ProjectEdict(),classSkills=load.Copy()});
        // A skill's edict in the edict window's words: its active preset tab (an explicit Custom stays Custom), or custom.
        public static string EdictLine(string id,HuntEdictLoadout loadout)
        {
            try
            {
                string scope=HuntEdictQuickPresets.SkillScope(id,loadout.edict.heroClass),selected=HuntEdictSkillPresets.Active(loadout,scope);
                return selected==HuntEdictQuickPresets.Custom?Loc.T("직접 설정"):Loc.F("간편 설정 · {0}",HuntEdictQuickPresets.For(scope).Single(p=>p.id==selected).Name);
            }
            catch(ArgumentException){return Loc.T("기본 칙령");}
        }
        // What changed in the Hunt Edict between two runs: equipped skills, each shared skill's automatic use, preset or
        // detailed options and investment, the attack order and every global group, summarised as the edict window does.
        public static List<string> DescribeChanges(ClassSkillLoadout before,ClassSkillLoadout after)
        {
            var changes=new List<string>();HuntEdictLoadout a,b;
            if(!ClassSkillTree.Uses(before)||!ClassSkillTree.Uses(after))return changes;
            try{a=Edict(before);b=Edict(after);}catch(ArgumentException){return changes;}
            string Name(string id)=>ClassSkillTree.Name(id);string OnOff(bool on)=>Loc.T(on?"켜짐":"꺼짐");
            string Options(HuntEdictLoadout l,string id)=>string.Join("|",l.edict.skills.Where(s=>s.id==id).SelectMany(s=>s.options.Skip(1)).Select(o=>o.id+"="+o.value));
            var was=Equipped(a.classSkills);var now=Equipped(b.classSkills);
            if(!was.SequenceEqual(now))changes.Add(Loc.F("장착 스킬: {0} → {1}",string.Join(", ",was.Select(Name)),string.Join(", ",now.Select(Name))));
            foreach(string id in now.Intersect(was).Append("BASIC"))
            {
                bool autoA=a.classSkills.automatic.Contains(id),autoB=b.classSkills.automatic.Contains(id);
                if(autoA!=autoB)changes.Add(Loc.F("{0} · 자동 사용: {1} → {2}",Name(id),OnOff(autoA),OnOff(autoB)));
                string lineA=EdictLine(id,a),lineB=EdictLine(id,b);
                if(lineA!=lineB)changes.Add(Loc.F("{0} · {1} → {2}",Name(id),lineA,lineB));
                else if(Options(a,id)!=Options(b,id))changes.Add(Loc.F("{0} · 세부 옵션 변경",Name(id)));
                if(id!="BASIC"&&a.classSkills.Rank(id)!=b.classSkills.Rank(id))changes.Add(Loc.F("{0} · 투자 {1} → {2}",Name(id),a.classSkills.Rank(id),b.classSkills.Rank(id)));
            }
            if(!a.classSkills.order.SequenceEqual(b.classSkills.order))
                changes.Add(Loc.F("공격 순서: {0} → {1}",string.Join(" → ",a.classSkills.order.Select(Name)),string.Join(" → ",b.classSkills.order.Select(Name))));
            foreach(var group in HuntEdictUiCatalog.Data.groups)
                if(group.ids.Any(id=>HuntEdictSummary.Value(a.edict,id)!=HuntEdictSummary.Value(b.edict,id)))
                    changes.Add(Loc.F("{0} · {1}",Loc.T(group.title),HuntEdictSummary.ForGroup(group,b.edict,Name)));
            return changes;
        }
        public static bool Recordable(RunState run)=>run.training==Training&&run.phase==RunPhase.Cleared&&run.statistics?.finish==CombatFinish.TargetsDefeated&&
            run.statistics.fullRun&&!run.statistics.buildChanged&&!string.IsNullOrEmpty(run.trainingGround.key);
        public static TrainingGroundRecord Find(HeroSave hero,string key)=>hero.trainingGround?.records?.Find(r=>r.key==key);
        // A success becomes the setup's previous run and, when faster, its best. The least recently used setups drop past the limit.
        public static void Record(HeroSave hero,string key,TrainingGroundResult result)
        {
            var records=hero.trainingGround.records;var record=records.Find(r=>r.key==key)??new TrainingGroundRecord{key=key};
            records.Remove(record);records.Insert(0,record);record.previous=result;
            if(!record.best.Recorded||result.seconds<record.best.seconds)
                record.best=new TrainingGroundResult{runId=result.runId,seconds=result.seconds,averageDps=result.averageDps,peakDps=result.peakDps,damage=result.damage,completedUtc=result.completedUtc};
            while(records.Count>RecordLimit)records.RemoveAt(records.Count-1);
        }
    }
}
