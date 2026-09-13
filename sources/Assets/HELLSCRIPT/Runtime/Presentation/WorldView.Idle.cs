using UnityEngine;

namespace Hellscript
{
    public sealed partial class WorldView
    {
        bool presentationSuspended, cameraWasEnabled, snapPresentation;
        string presentedRunId;
        public bool PresentationSuspended => presentationSuspended;
        public bool BattleCameraEnabled => viewCamera != null && viewCamera.enabled;
        public string PresentedRunId => presentedRunId;
        public Vector3 PresentedHeroPosition => hero == null ? Vector3.zero : hero.transform.position;
        public int TransientEffectCount => effects.Count;
        public void SuspendPresentation()
        {
            if (presentationSuspended) return;
            presentationSuspended = true; cameraWasEnabled = viewCamera != null && viewCamera.enabled;
            if (viewCamera != null) viewCamera.enabled = false;
            if (world != null) world.SetActive(false);
            foreach (var effect in effects) if (effect.go != null) Destroy(effect.go);
            effects.Clear();
        }
        public void DeferDungeon() { ClearDungeon(); presentedRunId = null; }
        public void ResumeCamera()
        {
            if (!presentationSuspended) return;
            presentationSuspended = false;
            if (viewCamera != null) viewCamera.enabled = cameraWasEnabled;
        }
        public void RevealPresentation(RunState run)
        {
            if (run == null) return;
            bool wasSuspended = presentationSuspended;
            if (world == null || presentedRunId != run.id) BuildDungeon(run);
            presentationSuspended = false; world.SetActive(true); elapsed = run.time;
            snapPresentation = true; Present(run, 0); snapPresentation = false;
            if (wasSuspended && viewCamera != null) viewCamera.enabled = cameraWasEnabled;
        }
        Vector3 CurrentHeroFacing(RunState run)
        {
            Vector2 direction = run.heroAction.phase == HeroActionPhase.Idle ? run.destination - run.position : run.heroAction.aim - run.heroAction.origin;
            return new Vector3(direction.x, 0, direction.y);
        }
    }
}
