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

        void InitializeIdle(string directory)
        { idlePreferences = new IdlePreferenceStore(directory); idleIntroduced = idlePreferences.Read(); combatClock.Reset(Time.realtimeSinceStartupAsDouble); }
        public void RequestIdle()
        {
            if (!CanEnterIdle) return;
            if (!idleIntroduced) UI.ShowIdleIntroduction(); else EnterIdle();
        }
        public void EnterIdle()
        {
            if (!CanEnterIdle || IdleHunting) return;
            idleIntroduced = true; idlePreferences.Write();
            savedFrameRate = Application.targetFrameRate; savedVSync = QualitySettings.vSyncCount;
            savedRenderInterval = OnDemandRendering.renderFrameInterval; savedSleepTimeout = Screen.sleepTimeout;
            idle.Enter(RepeatSession?.completed ?? 0); SetDimPresentation();
        }
        void SetDimPresentation()
        {
            peekRequested = false; World.SuspendPresentation(); UI.ShowIdleDimmed();
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
        }
        public void ExitIdle(bool openingMenu = true)
        {
            if (!IdleHunting) return;
            idle.Exit(); peekRequested = false;
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
            if (!IdleHunting) return;
            Application.targetFrameRate=savedFrameRate;QualitySettings.vSyncCount=savedVSync;
            OnDemandRendering.renderFrameInterval=savedRenderInterval;Screen.sleepTimeout=savedSleepTimeout;
        }
    }
}
