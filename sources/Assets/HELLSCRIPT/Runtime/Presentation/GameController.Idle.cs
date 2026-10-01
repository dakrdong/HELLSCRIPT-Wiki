using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace Hellscript
{
    public sealed partial class GameController
    {
        readonly ForegroundCombatClock combatClock = new ForegroundCombatClock();
        readonly IdleDisplaySession idle = new IdleDisplaySession();
        IdlePreferenceStore idlePreferences;
        public IdleHuntJournal IdleJournal { get; } = new IdleHuntJournal();
        double idleInteractiveUntil;
        public bool IdleUnlocking => idle.UnlockActive;
        public bool IdleUnlockHeld => idle.UnlockHeld;
        public float IdleUnlockProgress => idle.UnlockProgress;
        public void BeginIdleUnlock()
        { if (DisplayDimmed && !UI.CommonPanelOpen) { idle.SetUnlockHeld(true, Time.realtimeSinceStartupAsDouble); ForceIdleFrame(); } }
        public void EndIdleUnlock()
        {
            if (!DisplayDimmed) return;
            if (UI.CommonPanelOpen) { CancelIdleUnlock(); return; }
            if (idle.SetUnlockHeld(false, Time.realtimeSinceStartupAsDouble)) KeepWatching();
            else ForceIdleFrame();
        }
        public void CancelIdleUnlock()
        { idle.CancelUnlock(); if (IdleHunting) ForceIdleFrame(); }
        public void WakeIdleInteraction()
        { idleInteractiveUntil = Time.realtimeSinceStartupAsDouble + .7; ForceIdleFrame(); }
        void ObserveIdleCommit(StoreChange change)
        { if (IdleHunting) IdleJournal.Observe(Store.Data, Combat?.State, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()); }
        bool idleIntroduced, peekRequested, foregroundResumeRequired, foregroundSaveBlocked;
        int savedFrameRate, savedVSync, savedRenderInterval, savedSleepTimeout, forceIdleFrame, idleSummaryState;
        public IdleDisplayMode DisplayMode => idle.Mode;
        public bool IdleHunting => idle.Mode != IdleDisplayMode.Normal;
        public bool DisplayDimmed => idle.Mode == IdleDisplayMode.Dimmed;
        public bool ForegroundResumeRequired => foregroundResumeRequired;
        public int IdleCompleted => Mathf.Max(0, (RepeatSession?.completed ?? 0) - idle.CompletedAtEntry);
        public string ForegroundPauseReason { get; private set; } = "";
        public double PendingCombatTime => combatClock.Pending;
        public bool CanEnterIdle => Active && Combat.State.training < 0 && UI.Page == "battle" && !UI.BlocksRepeat &&
            !Combat.State.paused && !Combat.State.portal && !foregroundResumeRequired && !backgroundPaused &&
            string.IsNullOrEmpty(Combat.State.navigationError);

        public string CommonIdleStatus()
            => IdleHuntStatus.Read(Combat?.State, RepeatSession, UI.CommonPanelOpen,
                !string.IsNullOrEmpty(ForegroundPauseReason), liveOpsAdmissionPending, repeatRestored);

        void InitializeIdle(string directory)
        { idlePreferences = new IdlePreferenceStore(directory); idleIntroduced = idlePreferences.Read(); combatClock.Reset(Time.realtimeSinceStartupAsDouble); }
        public void RequestIdle()
        {
            if (!CanEnterIdle) return;
            EnterIdle();
        }
        public void EnterIdle()
        {
            if (!CanEnterIdle || IdleHunting) return;
            idleIntroduced = true; idlePreferences.Write();
            savedFrameRate = Application.targetFrameRate; savedVSync = QualitySettings.vSyncCount;
            savedRenderInterval = OnDemandRendering.renderFrameInterval; savedSleepTimeout = Screen.sleepTimeout;
            IdleJournal.Begin(Store.Data, Combat.State, Time.realtimeSinceStartupAsDouble, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            Store.Committed += ObserveIdleCommit;
            idleInteractiveUntil = 0;
            idle.Enter(RepeatSession?.completed ?? 0); SetDimPresentation();
        }
        void SetDimPresentation()
        {
            peekRequested = false; World.SuspendPresentation(); Audio?.SetPowerSaving(true); UI.ShowIdleDimmed();
            Application.targetFrameRate = 20; QualitySettings.vSyncCount = 0;
            Screen.sleepTimeout = SleepTimeout.NeverSleep; ForceIdleFrame();
        }
        public void RequestIdlePeek() { if (DisplayDimmed) { peekRequested = true; ForceIdleFrame(); } }
        public void DimIdle()
        { if (!IdleHunting) return; idle.Dim(); SetDimPresentation(); }
        public void KeepWatching()
        {
            ExitIdle(false);
            if (Combat == null) return;
            if (Combat.State.portal) UI.ShowBag(true); else if (Active) UI.ShowBattle(); else UI.ShowResult();
            if (!string.IsNullOrEmpty(ForegroundPauseReason)) UI.ShowToast(ForegroundPauseReason);
        }
        public void ExitIdle(bool openingMenu = true)
        {
            if (!IdleHunting) return;
            Store.Committed -= ObserveIdleCommit;
            idle.Exit(); peekRequested = false; Audio?.SetPowerSaving(false);
            Application.targetFrameRate = savedFrameRate; QualitySettings.vSyncCount = savedVSync;
            OnDemandRendering.renderFrameInterval = savedRenderInterval; Screen.sleepTimeout = savedSleepTimeout;
            UI.HideIdleOverlay();
            if (Combat != null) World.RevealPresentation(Combat.State);
            else World.ResumeCamera();
            if (openingMenu && UI.Page == "battle") UI.ShowBattle();
        }
        void ForceIdleFrame() { forceIdleFrame = Time.frameCount + 1; OnDemandRendering.renderFrameInterval = 1; }
        void UpdateIdlePresentation()
        {
            if (!IdleHunting) return;
            double now = Time.realtimeSinceStartupAsDouble;
            if (UI.CommonPanelOpen) idle.CancelUnlock();
            if (idle.UpdateUnlock(now)) { KeepWatching(); return; }
            if (DisplayDimmed && UI.CommonPanelOpen)
            { Application.targetFrameRate = 30; OnDemandRendering.renderFrameInterval = 1; return; }
            if (DisplayDimmed)
            {
                var input = Pointer.current;
                if (input?.press.isPressed == true || Mouse.current != null && Mouse.current.scroll.ReadValue().sqrMagnitude > 0)
                    idleInteractiveUntil = now + .7;
                bool interactive = IdleUnlocking || now < idleInteractiveUntil;
                Application.targetFrameRate = interactive ? 30 : 20;
                if (interactive) ForceIdleFrame();
            }
            if (DisplayMode == IdleDisplayMode.Peek)
            {
                var pointer = Pointer.current;
                if (pointer != null && pointer.press.isPressed || Mouse.current != null && Mouse.current.scroll.ReadValue().sqrMagnitude > 0 || Keyboard.current?.anyKey.isPressed == true) idle.Input(now);
                if (idle.Update(now)) SetDimPresentation();
            }
            if (!DisplayDimmed) return;
            if (peekRequested)
            {
                // Run after this frame's fixed ticks and rebuild from the current in-memory run.
                idle.Peek(now); peekRequested = false;
                Application.targetFrameRate = 30; OnDemandRendering.renderFrameInterval = 1;
                UI.PrepareIdlePeek(); World.RevealPresentation(Combat.State); UI.HideIdleOverlay();
                return;
            }
            int state = (Combat.State.paused ? 1 : 0) + (Combat.State.portal ? 2 : 0) + (foregroundResumeRequired ? 4 : 0) +
                8 * (int)(RepeatSession?.blocked ?? RepeatBlock.None) + 64 * (int)(RepeatSession?.stop ?? RepeatStop.None) + 1024 * (int)Combat.State.phase;
            if (state != idleSummaryState) { idleSummaryState = state; UI.RefreshIdleSummary(true); ForceIdleFrame(); }
            OnDemandRendering.renderFrameInterval = Time.frameCount <= forceIdleFrame ? 1 : 20;
        }
        void PauseForForeground(string reason)
        {
            if (Combat == null) return;
            foregroundResumeRequired = true; ForegroundPauseReason = reason;
            if (Active) Combat.State.paused = true;
            else repeatRestored = true;
            combatClock.Reset(Time.realtimeSinceStartupAsDouble); Save();
            if (DisplayDimmed) { UI.RefreshIdleSummary(true); ForceIdleFrame(); } else Notify(reason);
        }
        void RestoreForegroundClock()
        {
            foregroundResumeRequired = foregroundSaveBlocked = false; ForegroundPauseReason = "";
            combatClock.Reset(Time.realtimeSinceStartupAsDouble); repeatClock = Time.realtimeSinceStartupAsDouble;
        }
        void PreserveIdleSaveFailure()
        {
            if (!IdleHunting || !Active) return;
            foregroundResumeRequired=foregroundSaveBlocked=true;Combat.State.paused=true;
            ForegroundPauseReason="사냥 상태를 저장하지 못해 멈췄습니다. 저장 공간을 확인한 뒤 재개를 눌러 주세요.";
            combatClock.Reset(Time.realtimeSinceStartupAsDouble);
        }
        void OnDestroy()
        {
            if (Store != null) Store.Committed -= ObserveIdleCommit;
            if (!IdleHunting) return;
            Audio?.SetPowerSaving(false);
            Application.targetFrameRate=savedFrameRate;QualitySettings.vSyncCount=savedVSync;
            OnDemandRendering.renderFrameInterval=savedRenderInterval;Screen.sleepTimeout=savedSleepTimeout;
        }
    }
}
