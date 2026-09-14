using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Hellscript
{
    // One fade value per building, shared across its walls, roof and props. Materials are owned here.
    public sealed class TownBuildingFade
    {
        readonly Renderer[] renderers;
        readonly Material[] opaque,transparent;
        readonly ShadowCastingMode[] shadows;
        readonly MaterialPropertyBlock properties=new MaterialPropertyBlock();
        readonly Bounds bounds;
        readonly Transform frame;
        bool fading;
        public float Alpha {get;private set;}=1;
        public TownBuildingFade(Renderer[] renderers,Shader shader)
        {
            this.renderers=renderers;opaque=new Material[renderers.Length];transparent=new Material[renderers.Length];shadows=new ShadowCastingMode[renderers.Length];
            frame=renderers[0].transform.parent;
            var total=new Bounds();bool first=true;
            for(int i=0;i<renderers.Length;i++)
            {
                var r=renderers[i];opaque[i]=r.sharedMaterial;shadows[i]=r.shadowCastingMode;
                transparent[i]=new Material(shader){name=opaque[i].name+" faded"};transparent[i].SetColor("_BaseColor",opaque[i].color);
                // Measure in the building's own axes. A rotated world AABB includes empty front-yard space.
                var local=r.localBounds;var matrix=frame.worldToLocalMatrix*r.localToWorldMatrix;
                for(int corner=0;corner<8;corner++)
                {
                    var p=matrix.MultiplyPoint3x4(local.center+Vector3.Scale(local.extents,new Vector3((corner&1)==0?-1:1,(corner&2)==0?-1:1,(corner&4)==0?-1:1)));
                    if(first){total=new Bounds(p,Vector3.zero);first=false;}else total.Encapsulate(p);
                }
            }
            bounds=total;
        }
        public static bool Occludes(Bounds bounds,Vector3 camera,Vector3 subject)
        {
            var delta=subject-camera;float distance=delta.magnitude;if(distance<=.001f)return false;
            return bounds.IntersectRay(new Ray(camera,delta/distance),out float entry)&&entry<distance-.2f;
        }
        public void Update(Camera camera,Vector3 hero,float dt)
        {
            // Orthographic rays are parallel; their origins must be projected for the hero, not camera.position.
            bool blocked=false;
            for(int sample=0;sample<2;sample++)
            {
                var subject=hero+Vector3.up*(sample==0?.9f:1.9f);var ray=camera.ScreenPointToRay(camera.WorldToScreenPoint(subject));
                if(Occludes(bounds,frame.InverseTransformPoint(ray.origin),frame.InverseTransformPoint(subject))){blocked=true;break;}
            }
            Alpha=Mathf.MoveTowards(Alpha,blocked?.18f:1,dt*4);
            bool next=Alpha<.999f;
            for(int i=0;i<renderers.Length;i++)
            {
                var r=renderers[i];if(r==null)continue;
                if(next!=fading){r.sharedMaterial=next?transparent[i]:opaque[i];r.shadowCastingMode=next?ShadowCastingMode.Off:shadows[i];if(!next)r.SetPropertyBlock(null);}
                if(next){Color c=opaque[i].color;c.a=Alpha;properties.SetColor("_BaseColor",c);r.SetPropertyBlock(properties);}
            }
            fading=next;
        }
        public void Dispose(){foreach(var m in transparent)if(m!=null)Object.Destroy(m);}
    }
}
