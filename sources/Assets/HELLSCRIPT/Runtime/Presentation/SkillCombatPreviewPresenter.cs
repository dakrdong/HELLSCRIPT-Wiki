using System;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    // Owned by the window, outside its rebuilt child UI. Reflow only rebinds the image and labels.
    public sealed class SkillCombatPreviewPresenter : MonoBehaviour
    {
        public CombatPreviewSession Session {get;private set;}
        public RenderTexture Texture=>texture;
        public Camera PreviewCamera=>cameraView;
        public Func<bool> Visible;
        public RawImage Image;
        public Action RefreshLabels;
        GameObject root;
        WorldView world;
        Camera cameraView;
        CombatSimulation attached;
        RenderTexture texture;
        float frameClock,presentationSeconds;
        bool background;
        const int Layer=31;
        static readonly Vector3 Origin=new Vector3(10000,0,10000);
        public void Show(GameCatalog catalog,SkillPresetScenario scenario,AccountSave starter=null,int puzzleLevel=0)
        {
            if(Session?.Scenario.Key==scenario.Key&&Session.IsPuzzle==(puzzleLevel>0)&&(starter==null?!Session.IsStarter:Session.SourceSignature==(puzzleLevel>0?JsonUtility.ToJson(starter.Hero):TutorialProgress.Signature(starter.Hero))))return;
            Clear();Session=puzzleLevel>0?new CombatPreviewSession(catalog,starter,scenario,puzzleLevel):starter==null?new CombatPreviewSession(catalog,scenario):new CombatPreviewSession(catalog,starter,scenario.preset);
            root=new GameObject("Skill combat preview presentation");
            cameraView=new GameObject("Skill combat preview camera").AddComponent<Camera>();cameraView.transform.SetParent(root.transform,false);
            cameraView.cullingMask=1<<Layer;cameraView.enabled=false;cameraView.depth=-100;cameraView.rect=new Rect(0,0,1,1);
            cameraView.transform.position=Origin+new Vector3(0,22,-15);
            world=root.AddComponent<WorldView>();world.Initialize(new CombatPresentationContext(()=>Session.Combat,cameraView,()=>1,()=>Session.Combat.Hero,true,Origin,Layer));
            Session.Reset+=ResetWorld;ResetWorld();
        }
        void ResetWorld()
        {
            if(attached!=null)attached.Visual-=world.Effect;attached=Session.Combat;attached.Visual+=world.Effect;
            world.BuildDungeon(attached.State);frameClock=0;presentationSeconds=0;
        }
        public void Clear()
        {
            if(Session!=null){Session.Reset-=ResetWorld;Session.Dispose();Session=null;}
            if(attached!=null&&world!=null)attached.Visual-=world.Effect;attached=null;
            if(cameraView!=null){cameraView.enabled=false;cameraView.targetTexture=null;}
            if(root!=null){root.SetActive(false);Destroy(root);}root=null;world=null;cameraView=null;
            if(Image!=null)Image.texture=null;ReleaseTexture();Image=null;RefreshLabels=null;
        }
        void Resize()
        {
            Vector2 size=Image.rectTransform.rect.size;
            var canvas=Image.canvas;float scale=canvas!=null?canvas.scaleFactor:1;
            int width=Mathf.Clamp(Mathf.RoundToInt(size.x*scale),128,960),height=Mathf.Clamp(Mathf.RoundToInt(size.y*scale),96,640);
            if(texture!=null&&texture.width==width&&texture.height==height){Image.texture=texture;Image.color=Color.white;return;}
            cameraView.targetTexture=null;ReleaseTexture();
            texture=new RenderTexture(width,height,24,RenderTextureFormat.ARGB32){name="Skill preview texture",antiAliasing=1};texture.Create();
            cameraView.targetTexture=texture;cameraView.aspect=(float)width/height;Image.texture=texture;Image.color=Color.white;
        }
        void ReleaseTexture(){if(texture==null)return;texture.Release();Destroy(texture);texture=null;}
        void Update()
        {
            if(Session==null)return;
            bool visible=!background&&Image!=null&&Image.gameObject.activeInHierarchy&&(Visible?.Invoke()??true);
            cameraView.enabled=false;
            if(!visible)
            {
                // Bind a frozen first frame even when focus is elsewhere. Playback remains stopped.
                if(Image!=null&&Image.gameObject.activeInHierarchy&&(texture==null||Image.texture!=texture))
                {Resize();world.Present(Session.Combat.State,0);cameraView.enabled=true;RefreshLabels?.Invoke();}
                return;
            }
            presentationSeconds+=Session.Advance(Time.unscaledDeltaTime);frameClock+=Time.unscaledDeltaTime;
            if(frameClock<1f/30)return;
            frameClock%=1f/30;Resize();world.Present(Session.Combat.State,presentationSeconds);presentationSeconds=0;
            cameraView.enabled=true;RefreshLabels?.Invoke();
        }
        void OnApplicationPause(bool value)=>background=value;
        void OnApplicationFocus(bool value)=>background=!value;
        void OnDisable(){if(cameraView!=null)cameraView.enabled=false;}
        void OnDestroy()=>Clear();
    }
}
