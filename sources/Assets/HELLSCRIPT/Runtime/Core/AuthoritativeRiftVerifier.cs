#if UNITY_EDITOR || UNITY_SERVER
using System;
using System.Linq;
using UnityEngine;

namespace Hellscript
{
    [Serializable] public sealed class HuntAuthorityCheckpoint
    {
        public int version=1;
        public string accountId,leaseId,runId;
        public string previousLeaseId;
        public long revision,sequence,ticks,issuedMs,heartbeatMs;
        // Replay wall time starts at the lease, independently of earlier run attendance.
        public double replayMs;
        public AccountSave account;
        public RunRecord combatTelemetry;
    }
    // The host authenticates the principal, loads its account and commits by compare-and-swap.
    // Commit must atomically store BOTH the account and replay cursor, and reject stale leases.
    public interface IHuntAuthorityRepository
    {
        bool CompareExchange(string accountId,string leaseId,long expectedRevision,HuntAuthorityCheckpoint next);
    }
    public sealed class HuntTickRequest
    {
        public string leaseId,runId;
        public long sequence,totalTicks;
        // Deliberately no client rewards, damage, hero stats, seed, clear result or clock.
    }
    // Runs only in Editor tests or a Unity server build. It is not an in-client anti-cheat claim.
    // The snapshot must come from a server-owned admission transaction, never a request body.
    // Each accepted request replays the SAME CombatSimulation used by the foreground client.
    public sealed class AuthoritativeRiftVerifier
    {
        public const int MaxTicksPerRequest=200,LeaseTimeoutMs=30000;
        readonly GameCatalog catalog;
        readonly Func<long> serverClock;
        readonly IHuntAuthorityRepository repository;
        readonly object gate=new object();
        HuntAuthorityCheckpoint checkpoint;
        public string Error {get;private set;}="";
        static HuntAuthorityCheckpoint Copy(HuntAuthorityCheckpoint value)
        {
            var copy=JsonUtility.FromJson<HuntAuthorityCheckpoint>(JsonUtility.ToJson(value));
            if(copy.account.suspendedRun!=null&&string.IsNullOrEmpty(copy.account.suspendedRun.id))copy.account.suspendedRun=null;
            if(copy.account.repeatHunt?.pendingResult!=null&&string.IsNullOrEmpty(copy.account.repeatHunt.pendingResult.id))copy.account.repeatHunt.pendingResult=null;
            if(string.IsNullOrEmpty(copy.account.repeatHunt?.id))copy.account.repeatHunt=null;
            foreach(var hero in copy.account.heroes)if(string.IsNullOrEmpty(hero.interruptedRiftResult?.id))hero.interruptedRiftResult=null;
            return copy;
        }
        public AuthoritativeRiftVerifier(HuntAuthorityCheckpoint trusted,GameCatalog catalog,Func<long> serverClock,IHuntAuthorityRepository repository)
        {
            var ownedRun=string.IsNullOrEmpty(trusted?.account?.suspendedRun?.id)?trusted?.account?.repeatHunt?.pendingResult:trusted.account.suspendedRun;
            if(string.IsNullOrEmpty(ownedRun?.id))ownedRun=trusted?.account?.heroes?.Select(h=>h.interruptedRiftResult?.pendingResult).FirstOrDefault(r=>r?.id==trusted.runId);
            if(trusted?.version!=1||ownedRun==null||ownedRun.training>=0||
                ownedRun.id!=trusted.runId||!RepeatHunt.Terminal(ownedRun)&&trusted.account.Hero.id!=ownedRun.heroId||
                string.IsNullOrEmpty(trusted.accountId)||string.IsNullOrEmpty(trusted.leaseId)||trusted.issuedMs<0||trusted.heartbeatMs<trusted.issuedMs||
                trusted.sequence<0||trusted.ticks<0||trusted.revision<0||ownedRun.riftAttendance?.version!=1||
                !double.IsFinite(trusted.replayMs)||trusted.replayMs<0)
                throw new ArgumentException("A server-owned, admitted rift checkpoint is required.");
            this.catalog=catalog??throw new ArgumentNullException(nameof(catalog));
            this.serverClock=serverClock??throw new ArgumentNullException(nameof(serverClock));this.repository=repository??throw new ArgumentNullException(nameof(repository));
            checkpoint=Copy(trusted);
            // Checkpoints predating segment timing used the admitted speed for every tick.
            if(checkpoint.replayMs==0&&checkpoint.ticks>0)checkpoint.replayMs=checkpoint.ticks*50d/ownedRun.riftAttendance.speed;
        }
        public HuntAuthorityCheckpoint Snapshot(){lock(gate)return Copy(checkpoint);}
        // Host-only reentry: authenticate the account and issue a fresh lease before calling.
        // The expired 30-second tick lease never removes the 180-second recovery window.
        public bool EnterHero(string authenticatedAccountId,string previousLeaseId,string renewedLeaseId,int index)
        {
            lock(gate)
            {
                Error="";var current=checkpoint;
                bool Reject(string reason){Error=reason;return false;}
                if(authenticatedAccountId!=current.accountId||string.IsNullOrWhiteSpace(renewedLeaseId)||renewedLeaseId==previousLeaseId||!HeroSlots.IsCreated(current.account,index))return Reject("OWNER_OR_LEASE");
                if(current.leaseId==renewedLeaseId)return current.previousLeaseId==previousLeaseId&&current.account.selectedHero==index||Reject("REPLAY_PAYLOAD");
                if(current.leaseId!=previousLeaseId)return Reject("OWNER_OR_LEASE");
                long now=serverClock();if(now<current.heartbeatMs)return Reject("SERVER_CLOCK_ROLLBACK");
                var next=Copy(current);var account=next.account;
                try
                {
                    if(account.suspendedRun is RunState saved&&(account.riftResumeRunId!=saved.id||account.riftResumeUntilUtc==0))
                        RiftInterruption.Checkpoint(account,current.heartbeatMs/1000);
                    RepeatHunt.Normalize(account);RiftInterruption.NormalizeDeadline(account);
                    RiftInterruption.Enter(account,index,catalog,now/1000);
                    if(account.suspendedRun!=null)
                    {
                        // Preserve consumed segment time; offline time supplies no new tick budget.
                        next.issuedMs=now-(long)Math.Ceiling(next.replayMs);
                    }
                    CaptureTelemetry(next,now);
                    account.lastSeenUtc=Math.Max(account.lastSeenUtc,now/1000);
                    next.previousLeaseId=previousLeaseId;next.leaseId=renewedLeaseId;next.heartbeatMs=now;next.revision++;
                    if(!repository.CompareExchange(current.accountId,current.leaseId,current.revision,Copy(next)))return Reject("STALE_OR_SAVE_FAILED");
                    checkpoint=next;return true;
                }
                catch(Exception error){return Reject("REENTRY_FAILED:"+error.GetType().Name);}
            }
        }
        static void CaptureTelemetry(HuntAuthorityCheckpoint next,long now)
        {
            if(next.combatTelemetry?.id==next.runId&&next.combatTelemetry.journal?.trust=="server_replay")return;
            var record=next.account.records.Find(r=>r.id==next.runId);
            if(record?.journal==null)return;
            next.combatTelemetry=CombatJournal.Copy(record);
            next.combatTelemetry.journal.trust="server_replay";next.combatTelemetry.journal.completedUtcMs=now;
        }
        public bool Advance(string authenticatedAccountId,HuntTickRequest request)
        {
            lock(gate)
            {
                Error="";var current=checkpoint;
                bool Reject(string reason){Error=reason;return false;}
                if(authenticatedAccountId!=current.accountId||request==null||request.leaseId!=current.leaseId||request.runId!=current.runId)return Reject("OWNER_OR_LEASE");
                long now=serverClock();
                if(now<current.heartbeatMs)return Reject("SERVER_CLOCK_ROLLBACK");
                if(now-current.heartbeatMs>LeaseTimeoutMs)return Reject("LEASE_EXPIRED");
                if(request.sequence==current.sequence)return request.totalTicks==current.ticks||Reject("REPLAY_PAYLOAD");
                if(request.sequence!=current.sequence+1)return Reject("SEQUENCE");
                long count=request.totalTicks-current.ticks;
                if(count<1||count>MaxTicksPerRequest)return Reject("TICK_BUDGET");
                var run=current.account.suspendedRun;
                if(run==null||run.phase==RunPhase.Cleared||run.phase==RunPhase.Failed||run.paused||run.portal||!string.IsNullOrEmpty(run.navigationError))return Reject("RUN_NOT_ACTIVE");
                if(request.totalTicks>(long)Math.Floor((now-current.issuedMs)*run.riftAttendance.speed/50d))return Reject("SPEED_HACK");
                var next=Copy(current);var account=next.account;run=account.suspendedRun;
                try
                {
                    var sim=new CombatSimulation(account,catalog,run.stage,restore:run,recordResume:false);
                    for(int n=0;n<count;n++)
                    {
                        if(run.phase==RunPhase.Cleared||run.phase==RunPhase.Failed||run.paused||run.portal||!string.IsNullOrEmpty(run.navigationError))break;
                        double charge=50d/sim.EffectiveSpeed;
                        if(next.replayMs+charge>now-current.issuedMs+.001)return Reject("SPEED_HACK");
                        next.replayMs+=charge;
                        double paid=RiftEntryRules.Attend(account.riftFatigue,run.riftAttendance,now,charge);
                        run.realTime=(float)(run.riftAttendance.elapsedMs/1000d);
                        if(paid+.001<charge||account.riftFatigue.Total<=0){sim.ExhaustFatigue();break;}
                        sim.Tick(CombatSimulation.Step);
                    }
                    if(run.phase==RunPhase.Cleared||run.phase==RunPhase.Failed)
                    {
                        account.repeatHunt=RepeatHunt.Start(run,sim.RepeatPolicy,account.repeatHunt);
                        RepeatHunt.Complete(account.repeatHunt,run,sim.RepeatPolicy,account.Hero.highestClear,account.Hero,sim.ActivePotionSlots);
                        MonsterCodex.Commit(account,run);
                        account.suspendedRun=null;
                    }
                    if(run.phase==RunPhase.Cleared||run.phase==RunPhase.Failed)
                    {
                        CaptureTelemetry(next,now);
                    }
                    RiftInterruption.Checkpoint(account,now/1000);
                    account.lastSeenUtc=Math.Max(account.lastSeenUtc,now/1000);
                    next.sequence=request.sequence;next.ticks=request.totalTicks;next.revision++;next.heartbeatMs=now;
                    if(!repository.CompareExchange(current.accountId,current.leaseId,current.revision,Copy(next)))return Reject("STALE_OR_SAVE_FAILED");
                    checkpoint=next;return true;
                }
                catch(Exception error){return Reject("REPLAY_FAILED:"+error.GetType().Name);}
            }
        }
    }
}
#endif
