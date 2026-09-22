using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    // One full-screen draw: original artwork, two moving mist layers, cloud shadows and gate light.
    // Every animated value uses the same unscaled clock, so motion can stop without pausing the game.
    public sealed class TitleAtmosphere:MonoBehaviour
    {
        TitleSession session;RawImage image;Material atmosphere;
        public float Clock {get;private set;}
        public bool Ready=>image!=null&&image.texture!=null&&atmosphere!=null&&atmosphere.shader.isSupported;
        static readonly int ClockId=Shader.PropertyToID("_SceneTime"),CropId=Shader.PropertyToID("_Crop"),FadeId=Shader.PropertyToID("_EntryFade");
        public void Initialize(TitleSession state,string artwork="Art/Title/TitleSanctuary")
        {
            session=state;image=gameObject.AddComponent<RawImage>();image.raycastTarget=false;
            image.texture=Resources.Load<Texture2D>(artwork);
            var shader=Resources.Load<Shader>("Art/Title/TitleAtmosphere");
            if(shader!=null){atmosphere=new Material(shader){name="Title atmosphere instance"};image.material=atmosphere;}
            Update();
        }
        public static Rect Cover(float viewAspect,float imageAspect)
        {
            float w=Mathf.Min(1,viewAspect/imageAspect),h=Mathf.Min(1,imageAspect/viewAspect);
            return new Rect((1-w)*.5f,(1-h)*.5f,w,h);
        }
        public void SetEntryFade(float amount){if(atmosphere!=null)atmosphere.SetFloat(FadeId,amount);}
        void Update()
        {
            if(session==null||image==null||image.texture==null)return;
            if(session.MotionEnabled)Clock+=Mathf.Min(Time.unscaledDeltaTime,.1f);
            var r=((RectTransform)transform).rect;
            var crop=Cover(Mathf.Max(1,r.width)/Mathf.Max(1,r.height),(float)image.texture.width/image.texture.height);
            if(atmosphere==null){image.uvRect=crop;return;}
            atmosphere.SetVector(CropId,new Vector4(crop.x,crop.y,crop.width,crop.height));
            atmosphere.SetFloat(ClockId,Clock);
        }
        void OnDestroy(){if(atmosphere!=null)Destroy(atmosphere);}
    }
}
