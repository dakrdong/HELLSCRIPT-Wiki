using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object=UnityEngine.Object;

namespace Hellscript
{
    // Scene mood owner for the single world camera: the one shadowed directional key light, trilight ambient and
    // fog from FieldLook, a warm light that follows the hero (a separate object, so the hero's child 0 stays the
    // body), a small pool of real point lights handed to the nearest registered emitters, the global post volume
    // (a runtime copy of the Resources profile, so field tweaks never touch the asset) and a camera shake that is
    // added only while the camera renders, so every gameplay read of the camera transform stays exact.
    public sealed class WorldLighting : IDisposable
    {
        // Mobile_RPAsset is Forward with 4 additional lights per object: the hero light plus 3 pool lights stay inside it,
        // so URP never drops a light (a flickering intensity would otherwise change which one it drops every frame).
        // PC renders Forward+, which has no per-object cap.
        public const int PcPool=8,MobilePool=3;
        public const float HeroLightHeight=3,HeroLightBack=1.3f,MaxEmitterDistance=24,FadeBand=2,ShakeScale=.4f,MaxShake=.3f;
        public static bool MobileQuality=>Application.isMobilePlatform||QualitySettings.names[QualitySettings.GetQualityLevel()]=="Mobile";
        sealed class Emitter {public Transform t;public Color color;public float range,intensity,seed;}
        // Keep the cached Comparison: in Unity's Mono corlib List.Sort(Comparison) sorts without allocating, while
        // List.Sort(IComparer) allocates on every call (measured in the Editor, whose corlib matches the player's).
        static readonly Comparison<(float d,Emitter e)> ByDistance=(a,b)=>a.d.CompareTo(b.d);
        readonly Camera camera;
        readonly GameObject root;
        readonly VolumeProfile profile;
        readonly float baseBloom;
        readonly List<Light> pool=new List<Light>();
        readonly List<Emitter> emitters=new List<Emitter>();
        readonly List<(float d,Emitter e)> nearest=new List<(float d,Emitter e)>();
        float clock,shake,shakeTotal,shakeLeft;
        Vector3 applied;
        public Light Moonlight {get;}
        public Light HeroLight {get;}
        public Volume Volume {get;}
        public IReadOnlyList<Light> Pool=>pool;
        public int EmitterCount=>emitters.Count;
        public FieldLook Look {get;private set;}
        // Device-local reduce-motion preference, the same key BlacksmithWindow already honours; set, it zeroes shakes.
        // ponytail: no settings UI writes this key yet (CHK-P04 wants a shake/flash toggle), so shakes stay subtle
        // (ShakeScale, MaxShake) until the settings screen gains that switch.
        public const string ReduceMotionKey="hellscript.reduce-motion";
        public bool ShakeEnabled=true;
        // Set by WorldView while the settings world is open.
        public bool ShakeHeld;
        bool CanShake=>ShakeEnabled&&PlayerPrefs.GetInt(ReduceMotionKey,0)==0;

        public WorldLighting(Camera camera,bool mobile)
        {
            this.camera=camera;root=new GameObject("World lighting");
            foreach(var light in Object.FindObjectsByType<Light>())if(light.type==LightType.Directional){Moonlight=light;break;}
            if(Moonlight==null){Moonlight=new GameObject("Moonlight").AddComponent<Light>();Moonlight.type=LightType.Directional;}
            // The shadows-off main light variant is stripped from builds, so the key light always casts shadows.
            Moonlight.shadows=LightShadows.Soft;
            HeroLight=PointLight("Hero light");
            for(int i=0;i<(mobile?MobilePool:PcPool);i++)pool.Add(PointLight("Emitter light "+i));
            var data=camera.GetUniversalAdditionalCameraData();data.renderPostProcessing=true;data.dithering=true;
            data.antialiasing=mobile?AntialiasingMode.FastApproximateAntialiasing:AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            var source=Resources.Load<VolumeProfile>(mobile?"Rendering/WorldPost_Mobile":"Rendering/WorldPost_PC");
            profile=ScriptableObject.CreateInstance<VolumeProfile>();profile.name="World post (runtime)";
            if(source!=null)foreach(var component in source.components){var copy=Object.Instantiate(component);copy.name=component.name;profile.components.Add(copy);}
            if(profile.TryGet<Bloom>(out var bloom))baseBloom=bloom.intensity.value;
            Volume=new GameObject("World post").AddComponent<Volume>();Volume.transform.SetParent(root.transform,false);Volume.isGlobal=true;Volume.priority=1;Volume.sharedProfile=profile;
            RenderPipelineManager.beginCameraRendering+=BeginCamera;RenderPipelineManager.endCameraRendering+=EndCamera;
        }
        Light PointLight(string name)
        {
            var light=new GameObject(name).AddComponent<Light>();light.transform.SetParent(root.transform,false);
            light.type=LightType.Point;light.shadows=LightShadows.None;light.enabled=false;return light;
        }
        public void Apply(int field)=>Apply(FieldLook.Field(field));
        public void ApplyTown()=>Apply(FieldLook.Town);
        public void ApplyLegacy(int theme)=>Apply(FieldLook.LegacyTheme(theme));
        public void Apply(FieldLook look)
        {
            Look=look;
            RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=look.sky;RenderSettings.ambientEquatorColor=look.equator;RenderSettings.ambientGroundColor=look.ground;
            RenderSettings.fog=true;RenderSettings.fogMode=FogMode.Linear;RenderSettings.fogColor=look.fog;RenderSettings.fogStartDistance=look.fogStart;RenderSettings.fogEndDistance=look.fogEnd;
            Moonlight.color=look.moon;Moonlight.intensity=look.moonIntensity;Moonlight.transform.rotation=Quaternion.Euler(look.moonPitch,look.moonYaw,0);
            camera.backgroundColor=look.background;
            HeroLight.color=look.hero;HeroLight.intensity=look.heroIntensity;HeroLight.range=look.heroRange;
            if(profile.TryGet<ColorAdjustments>(out var grade)){grade.saturation.value=look.saturation;grade.contrast.value=look.contrast;grade.postExposure.value=look.exposure;grade.colorFilter.value=look.filter;}
            if(profile.TryGet<SplitToning>(out var split)){split.shadows.value=look.shadows;split.highlights.value=look.highlights;}
            if(profile.TryGet<Vignette>(out var vignette)){vignette.intensity.value=look.vignette;vignette.color.value=look.vignetteColor;}
            if(profile.TryGet<Bloom>(out var bloom)){bloom.intensity.value=baseBloom*look.bloom;bloom.tint.value=look.bloomTint;}
        }
        // Braziers, torches and glowing props register here; only the nearest few get a real light.
        public void RegisterEmitter(Transform t,Color color,float range,float intensity)
        {if(t!=null)emitters.Add(new Emitter{t=t,color=color,range=range,intensity=intensity,seed=emitters.Count*7.31f+.5f});}
        // Drops every emitter and hides the hero light until the next Tick (the stage that owned them is gone).
        public void UnregisterAll(){emitters.Clear();HeroLight.enabled=false;foreach(var light in pool)light.enabled=false;}
        public void Tick(Vector3 heroPosition,float dt)
        {
            clock+=dt;shakeLeft-=dt;
            // Straight above, a 1/d² light 0.4 m over the head blows it out white. Set back from the camera it rims the
            // head and shoulders instead and still pools on the ground around the feet.
            var away=camera.transform.forward;away.y=0;
            HeroLight.enabled=true;HeroLight.transform.position=heroPosition+Vector3.up*HeroLightHeight+away.normalized*HeroLightBack;
            emitters.RemoveAll(e=>e.t==null);nearest.Clear();
            foreach(var e in emitters)
            {if(!e.t.gameObject.activeInHierarchy)continue;float d=Vector3.Distance(e.t.position,heroPosition);if(d<MaxEmitterDistance)nearest.Add((d,e));}
            nearest.Sort(ByDistance);
            // The first emitter left out marks the fade edge, so a light dims to zero before it moves to a nearer emitter.
            float edge=nearest.Count>pool.Count?nearest[pool.Count].d:MaxEmitterDistance;
            for(int i=0;i<pool.Count;i++)
            {
                var light=pool[i];if(i>=nearest.Count){light.enabled=false;continue;}
                var (d,e)=nearest[i];float flicker=1+(Mathf.PerlinNoise(clock*2.1f,e.seed)-.5f)*.3f;
                light.transform.position=e.t.position;light.color=e.color;light.range=e.range;light.intensity=e.intensity*flicker*Mathf.Clamp01((edge-d)/FadeBand);
                light.enabled=light.intensity>.01f;
            }
        }
        float CurrentShake=>shakeLeft>0?shake*(shakeLeft/shakeTotal)*(shakeLeft/shakeTotal):0;
        // Starts a shake (the stronger one wins) and returns the offset the camera renders with right now.
        public Vector3 Shake(float amplitude,float duration)
        {
            float a=Mathf.Min(MaxShake,amplitude*ShakeScale);
            if(CanShake&&duration>0&&a>CurrentShake){shake=a;shakeTotal=shakeLeft=duration;}
            return Offset;
        }
        // A presentation snap (reveal after idle, restores) must land on the exact framing.
        public void StopShake()=>shakeLeft=0;
        public Vector3 Offset
        {
            get
            {
                float a=CurrentShake;if(a<=0||ShakeHeld||!CanShake)return Vector3.zero;
                var c=camera.transform;float t=clock*23;
                return (c.right*(Mathf.PerlinNoise(t,.3f)-.5f)+c.up*(Mathf.PerlinNoise(.7f,t)-.5f))*(2*a);
            }
        }
        void BeginCamera(ScriptableRenderContext context,Camera rendering){if(rendering!=camera)return;applied=Offset;rendering.transform.position+=applied;}
        void EndCamera(ScriptableRenderContext context,Camera rendering){if(rendering!=camera)return;rendering.transform.position-=applied;applied=Vector3.zero;}
        public void Dispose()
        {
            RenderPipelineManager.beginCameraRendering-=BeginCamera;RenderPipelineManager.endCameraRendering-=EndCamera;
            foreach(var component in profile.components)Release(component);Release(profile);Release(root);
        }
        static void Release(Object value){if(value==null)return;if(Application.isPlaying)Object.Destroy(value);else Object.DestroyImmediate(value);}
    }
}
