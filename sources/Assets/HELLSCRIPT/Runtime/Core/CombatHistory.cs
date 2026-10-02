using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    [Serializable]
    public sealed class CombatReview
    {
        public int version;
        public string heroId;
        public HeroClass heroClass;
        public int heroLevel;
        public RunPhase phase;
        public float health, maxHealth, resource;
        public bool edictActive, timelineTruncated, decisionsTruncated, contextTruncated;
        public BuildConfig finalBuild;
        public HuntEdictV2Document finalEdict;
        public List<Item> equipment=new List<Item>();
        public CombatStatistics totals;
        public CombatDpsTimeline dps;
        public List<CombatActivitySample> activityHistory=new List<CombatActivitySample>();
        public DefeatAnalysis finalWindow;
        public List<RuleDecision> decisions=new List<RuleDecision>();
        public List<HuntEdictV2Document> edictVersions=new List<HuntEdictV2Document>();
    }

    public static class CombatHistory
    {
        public const int Version=1, RecordLimit=100, TimelineEventLimit=512, TimelineSnapshotLimit=101, DecisionLimit=80;
        public const int EdictVersionLimit=8;
        public const float WindowSeconds=5;
        static T Copy<T>(T value) where T:class => value==null?null:JsonUtility.FromJson<T>(JsonUtility.ToJson(value));

        // Completed records own their data. Reading a record must never refer back to an
        // equipped item, editable build, telemetry buffer or another running simulation.
        public static CombatReview Capture(RunState run,HeroSave hero,HeroStats stats,HuntEdictV2Document activeEdict)
        {
            var observed=run.statistics.BuildDefeatAnalysis(null,WindowSeconds);
            var result=new CombatReview
            {
                version=Version, heroId=hero.id, heroClass=hero.heroClass, heroLevel=hero.level,
                phase=run.phase, health=Mathf.Max(0,run.health), maxHealth=stats.hp, resource=run.resource,
                edictActive=activeEdict!=null, finalBuild=run.build.Copy(), finalEdict=activeEdict?.Copy(),
                equipment=hero.inventory.Where(i=>i.equipped).Select(Copy).ToList(),
                totals=Copy(run.statistics), dps=Copy(run.dps), finalWindow=Copy(observed),
                activityHistory=(run.statistics.feedback?.activityHistory??new List<CombatActivitySample>()).Select(Copy).ToList(),
                decisions=run.decisions.Skip(Math.Max(0,run.decisions.Count-DecisionLimit)).Select(Copy).ToList(),
                decisionsTruncated=run.decisions.Count>DecisionLimit
            };
            // Keep the complete aggregate, but bound detailed samples and events.
            // The newest events win so a fatal hit is not dropped by a busy earlier tick.
            result.timelineTruncated=result.finalWindow.timeline.Count>TimelineSnapshotLimit;
            result.finalWindow.timeline=result.finalWindow.timeline.Skip(Math.Max(0,result.finalWindow.timeline.Count-TimelineSnapshotLimit)).ToList();
            int remaining=TimelineEventLimit;
            var contexts=new Dictionary<HuntEdictV2Document,int>();
            int firstObserved=Math.Max(0,observed.timeline.Count-TimelineSnapshotLimit);
            for(int i=result.finalWindow.timeline.Count-1;i>=0;i--)
            {
                var tick=result.finalWindow.timeline[i];
                int damage=Math.Min(remaining,tick.damageSources.Count);remaining-=damage;
                int blocked=Math.Min(remaining,tick.blockedAttempts.Count);remaining-=blocked;
                result.timelineTruncated|=damage<tick.damageSources.Count||blocked<tick.blockedAttempts.Count;
                tick.damageSources=tick.damageSources.Skip(tick.damageSources.Count-damage).ToList();
                tick.blockedAttempts=tick.blockedAttempts.Skip(tick.blockedAttempts.Count-blocked).ToList();
                var original=observed.timeline[firstObserved+i].blockedAttempts;
                for(int j=0;j<blocked;j++)
                {
                    var document=original[original.Count-blocked+j].edictContext;
                    if(document==null)continue;
                    if(!contexts.TryGetValue(document,out int index))
                    {
                        if(result.edictVersions.Count>=EdictVersionLimit){result.contextTruncated=true;continue;}
                        index=result.edictVersions.Count;contexts.Add(document,index);result.edictVersions.Add(document.Copy());
                    }
                    tick.blockedAttempts[j].edictIndex=index;
                }
            }
            return result;
        }
    }

    public sealed partial class CombatSimulation
    {
        void CaptureCompletedReview()
        {
            if(State.training>=0||State.phase!=RunPhase.Cleared&&State.phase!=RunPhase.Failed)return;
            var record=account.records.FirstOrDefault(r=>r.id==State.id);
            if(record!=null){record.review=CombatHistory.Capture(State,Hero,Stats,edictSource);CompleteJournal(record);}
        }
    }
}
