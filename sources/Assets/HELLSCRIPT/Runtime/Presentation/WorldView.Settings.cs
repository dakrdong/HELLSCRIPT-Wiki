using UnityEngine;

namespace Hellscript
{
    public sealed partial class WorldView
    {
        bool settingsWorldOpen;Rect settingsWorldViewport;Vector3 settingsCameraPosition;
        public bool HasPresentedPlayer=>viewCamera!=null&&hero!=null&&world!=null&&world.activeInHierarchy&&!presentationSuspended;
        public bool SettingsWorldOpen=>settingsWorldOpen;
        Rect GameplayViewport=>game.UI!=null&&game.UI.Page=="battle"?game.UI.BattleViewport:UiSafeArea.FrameNormalized;
        float GameplayHalfHeight(Rect viewport)
        {
            float aspect=Screen.width*viewport.width/Mathf.Max(1,Screen.height*viewport.height);
            float normal=game.UI!=null&&game.UI.Page=="plaza"?Mathf.Max(15,14/Mathf.Max(.3f,aspect)):BattleHudLayout.CameraHalfHeight(aspect,game.UI!=null?game.UI.BattleViewHeight:Screen.height);
            return normal*(game.ViewDistance?.Factor??1);
        }
        // Reframe the existing world camera; the actual hero keeps its position, pose and identity.
        public void ShowSettingsWorld(Rect viewport)
        {
            if(!HasPresentedPlayer)return;
            if(!settingsWorldOpen)settingsCameraPosition=viewCamera.transform.position;
            settingsWorldOpen=true;settingsWorldViewport=viewport;ApplySettingsWorld();
        }
        void ApplySettingsWorld()
        {
            var normal=GameplayViewport;viewCamera.rect=settingsWorldViewport;viewCamera.ResetAspect();
            // Preserve the world-to-screen scale of normal gameplay, including the chosen zoom.
            viewCamera.orthographicSize=GameplayHalfHeight(normal)*settingsWorldViewport.height/Mathf.Max(.001f,normal.height);
            var camera=viewCamera.transform;var offset=hero.transform.position+Vector3.up*1.1f-settingsCameraPosition;
            camera.position=settingsCameraPosition+camera.right*Vector3.Dot(offset,camera.right)+camera.up*Vector3.Dot(offset,camera.up);
        }
        public void RestoreSettingsWorld()
        {
            if(!settingsWorldOpen)return;settingsWorldOpen=false;
            if(viewCamera!=null)viewCamera.transform.position=settingsCameraPosition;
            ApplyBattleViewport();
        }
    }
}
