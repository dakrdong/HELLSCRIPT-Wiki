using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Hellscript
{
    public sealed partial class WorldView
    {
        bool presentationSuspended, cameraWasEnabled, snapPresentation;
        Camera idleBackgroundCamera;
        string presentedRunId;
        public bool PresentationSuspended => presentationSuspended;
        public bool BattleCameraEnabled => viewCamera != null && viewCamera.enabled;
        public bool IdleBackgroundCameraEnabled => idleBackgroundCamera != null && idleBackgroundCamera.isActiveAndEnabled;
        public string PresentedRunId => presentedRunId;
        public Vector3 PresentedHeroPosition => hero == null ? Vector3.zero : hero.transform.position;
        public int TransientEffectCount => effects.Count;
        public void SuspendPresentation()
        {
            if (presentationSuspended) return;
            presentationSuspended = true; cameraWasEnabled = viewCamera != null && viewCamera.enabled;
            lighting?.StopShake();
            if (viewCamera != null) viewCamera.enabled = false;
            ShowIdleBackground();
            if (world != null) world.SetActive(false);
            foreach (var effect in effects) if (effect.go != null) Destroy(effect.go);
            effects.Clear();
        }
        void ShowIdleBackground()
        {
            // Keep one empty display camera so the Editor does not draw "No cameras rendering" over the HUD.
            if (idleBackgroundCamera == null)
            {
                var obj = new GameObject("Power saving background camera", typeof(Camera));
                obj.transform.SetParent(transform, false);
                idleBackgroundCamera = obj.GetComponent<Camera>();
                idleBackgroundCamera.clearFlags = CameraClearFlags.SolidColor;
                idleBackgroundCamera.backgroundColor = Color.black;
                idleBackgroundCamera.cullingMask = 0;
                idleBackgroundCamera.orthographic = true;
                idleBackgroundCamera.allowHDR = idleBackgroundCamera.allowMSAA = idleBackgroundCamera.useOcclusionCulling = false;
                var data = idleBackgroundCamera.GetUniversalAdditionalCameraData();
                data.renderPostProcessing = data.renderShadows = false;
                data.requiresColorTexture = data.requiresDepthTexture = false;
                data.volumeLayerMask = 0;
                data.antialiasing = AntialiasingMode.None;
            }
            idleBackgroundCamera.targetDisplay = viewCamera != null ? viewCamera.targetDisplay : 0;
            idleBackgroundCamera.enabled = true;
        }
        public void DeferDungeon() { ClearDungeon(); presentedRunId = null; }
        public void ResumeCamera()
        {
            if (idleBackgroundCamera != null) idleBackgroundCamera.enabled = false;
            if (!presentationSuspended) return;
            presentationSuspended = false;
            if (viewCamera != null) viewCamera.enabled = cameraWasEnabled;
        }
        public void RevealPresentation(RunState run)
        {
            if (run == null) return;
            if (idleBackgroundCamera != null) idleBackgroundCamera.enabled = false;
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
