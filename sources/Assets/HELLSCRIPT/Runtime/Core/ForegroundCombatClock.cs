using System;

namespace Hellscript
{
    // Display frequency never changes combat's fixed step or silently discards foreground time.
    public sealed class ForegroundCombatClock
    {
        public const int TickBudget = 20;
        public const double StepSeconds = 1d / 20;
        public const double BacklogLimit = .5, OverloadSeconds = 3;
        const double Epsilon = 1e-10;
        double last, backlogDuration;
        bool initialized;
        public double Pending { get; private set; }
        public bool Overloaded { get; private set; }

        public double Sample(double now)
        {
            if (double.IsNaN(now) || double.IsInfinity(now)) throw new ArgumentOutOfRangeException(nameof(now));
            if (!initialized) { initialized = true; last = now; return 0; }
            double elapsed = Math.Max(0, now - last); last = Math.Max(last, now); return elapsed;
        }
        public void Accumulate(double elapsed, double speed)
        {
            if (double.IsNaN(elapsed) || double.IsInfinity(elapsed) || elapsed < 0) throw new ArgumentOutOfRangeException(nameof(elapsed));
            if (double.IsNaN(speed) || double.IsInfinity(speed) || speed <= 0) throw new ArgumentOutOfRangeException(nameof(speed));
            if (!Overloaded) Pending += elapsed * speed;
        }
        public bool TakeTick()
        {
            if (Overloaded || Pending + Epsilon < StepSeconds) return false;
            Pending = Math.Max(0, Pending - StepSeconds); return true;
        }
        public bool CheckBacklog(double elapsed, double speed)
        {
            if (Pending / speed > BacklogLimit) backlogDuration += elapsed; else backlogDuration = 0;
            if (backlogDuration < OverloadSeconds) return false;
            Overloaded = true; Pending = 0; return true;
        }
        // Editing and explicit pauses retain only an unfinished tick, never deferred active work.
        public void Pause()
        { Pending %= StepSeconds; backlogDuration = 0; }
        // An application suspension or a new run starts at its last completed tick.
        public void Reset(double now)
        { initialized = false; Pending = backlogDuration = 0; Overloaded = false; Sample(now); }
    }
}
