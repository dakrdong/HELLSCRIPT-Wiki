using System.Collections.Generic;
using UnityEngine;

namespace Hellscript
{
    // Enemy telegraph views WorldView.Enemies rents per threat key: the WorldTelegraph fill decal (its fill grows with the
    // windup, Transparent+10 so it draws above friendly ground effects), the calm blue refuge circle and the Bellringer aura.
    // Fill meshes follow the shader's contract: UV.x = normalised radius (circle, sector; inner 0 to outer 1 for rings) or
    // distance along a line, UV.y = angle 0..1 around (or across a sector) or 0..1 across a line.
    public sealed partial class WorldFx
    {
        public enum TelegraphKind{Fill,Refuge,Aura}
        public sealed class TelegraphView{public TelegraphKind Kind{get;internal set;}public GameObject go;internal Effect fx;internal MeshFilter filter;internal MeshRenderer renderer;}
        public const float FillHeight=.12f;
        static readonly int ProgressId=Shader.PropertyToID("_Progress"),EdgesId=Shader.PropertyToID("_Edges"),FlashId=Shader.PropertyToID("_Flash");
        // Release flash: a completed warning's footprint burns white-hot, swells a little and fades over FlashLife seconds.
        public const float FlashLife=.3f;
        struct ReleaseFlashState{public TelegraphView view;public TelegraphGauge.Footprint footprint;public float age;}
        readonly List<ReleaseFlashState> flashes=new List<ReleaseFlashState>(16);
        public int ActiveFlashes=>flashes.Count;
        // Gamma tints (linearised by Unity): the shader default for enemies, brighter and hotter for bosses.
        // Kept low in G and B: brighter values leave ACES's red shoulder and bleach to salmon with post-processing on.
        public static readonly Color EnemyTelegraph=new Color(1,.2f,.13f,.95f),BossTelegraph=new Color(1.15f,.3f,.12f,1);
        readonly List<TelegraphView> freeViews=new List<TelegraphView>();
        Material telegraph,edge,bossEdge,refugeEdge,auraEdge,guardEdge,rearEdge,tether;
        Mesh disc,band;
        readonly Dictionary<int,Mesh> rings=new Dictionary<int,Mesh>(),sectors=new Dictionary<int,Mesh>();

        // Outline materials for the kept LineRenderer outlines: unlit, fog-aware, readable with post-processing on.
        public Material TelegraphEdge=>edge!=null?edge:edge=Tint("Telegraph edge",Blend.Alpha,new Color(1.1f,.2f,.14f,.95f));
        public Material BossTelegraphEdge=>bossEdge!=null?bossEdge:bossEdge=Tint("Boss telegraph edge",Blend.Alpha,new Color(1.3f,.34f,.14f,1));
        public Material RefugeEdge=>refugeEdge!=null?refugeEdge:refugeEdge=Tint("Refuge edge",Blend.Alpha,new Color(.45f,.8f,1.2f,.9f));
        public Material AuraEdge=>auraEdge!=null?auraEdge:auraEdge=Tint("Aura edge",Blend.Alpha,new Color(.72f,.42f,1.1f,.55f));
        public Material GuardEdge=>guardEdge!=null?guardEdge:guardEdge=Tint("Guard edge",Blend.Alpha,new Color(.45f,.75f,1.15f,.85f));
        public Material RearEdge=>rearEdge!=null?rearEdge:rearEdge=Tint("Rear window edge",Blend.Alpha,new Color(1.25f,.62f,.2f,.9f));
        // The elite life link: a violet lightning strip the Tick scrolls along the line (LineTextureMode.Tile).
        public Material Tether=>tether!=null?tether:tether=Tint("Life link",Blend.Add,new Color(.85f,.5f,1.25f,1),"lightning_strip");
        Material Tint(string name,Blend blend,Color tint,string texture=null)
        {
            var material=new Material(Resources.Load<Shader>("WorldFx")){name="HELLSCRIPT FX "+name};owned.Add(material);
            material.SetTexture("_MainTex",Texture(texture));material.SetColor("_TintColor",tint);
            material.SetFloat("_SrcBlend",(float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend",(float)(blend==Blend.Add?UnityEngine.Rendering.BlendMode.One:UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha));
            material.SetFloat("_ZWrite",0);return Resolve(material);
        }
        Material TelegraphMaterial()
        {
            if(telegraph!=null)return telegraph;
            var material=new Material(Resources.Load<Shader>("WorldTelegraph")){name="HELLSCRIPT FX telegraph fill"};owned.Add(material);
            return telegraph=Resolve(material);
        }
        static readonly int MainTexId=Shader.PropertyToID("_MainTex");
        void TickTether(){if(tether!=null)tether.SetTextureOffset(MainTexId,new Vector2(-clock*1.8f,0));}

        public TelegraphView RentTelegraph(TelegraphKind kind)
        {
            EnsureConfigured();
            for(int i=freeViews.Count-1;i>=0;i--)
            {
                var v=freeViews[i];
                if(v.go==null){freeViews.RemoveAt(i);continue;}
                if(v.Kind!=kind)continue;
                freeViews.RemoveAt(i);v.go.SetActive(true);return v;
            }
            return CreateView(kind);
        }
        public void ReturnTelegraph(TelegraphView view)
        {
            // A rented view is active; an inactive one is already back in the pool.
            if(view==null||view.go==null||!view.go.activeSelf)return;
            view.go.SetActive(false);freeViews.Add(view);
        }
        TelegraphView CreateView(TelegraphKind kind)
        {
            var view=new TelegraphView{Kind=kind};
            if(kind==TelegraphKind.Fill)
            {
                view.go=new GameObject("Telegraph fill");view.go.transform.SetParent(transform,false);
                view.filter=view.go.AddComponent<MeshFilter>();view.renderer=view.go.AddComponent<MeshRenderer>();view.renderer.sharedMaterial=TelegraphMaterial();
                view.renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;view.renderer.receiveShadows=false;
                view.renderer.lightProbeUsage=UnityEngine.Rendering.LightProbeUsage.Off;view.renderer.reflectionProbeUsage=UnityEngine.Rendering.ReflectionProbeUsage.Off;
                return view;
            }
            if(kind==TelegraphKind.Refuge)
            {
                view.fx=Loop("Refuge circle",transform,Vector3.zero,1,System.Array.Empty<Layer>(),0);view.go=view.fx.go;
                AddSprite(view.fx,"glow_soft",Blend.Add,new Color(.35f,.65f,1,.42f),new Vector3(0,.07f,0),2.2f,0,.35f,2.2f).ratio=1.1f;
                AddSprite(view.fx,"rune_circle_B",Blend.Add,new Color(.5f,.8f,1,.85f),new Vector3(0,.08f,0),2,12,.25f,1.6f);
                return view;
            }
            var motes=new Layer{texture="ember",blend=Blend.Add,heat=1.3f,loop=true,duration=1,rate=10,life=new Vector2(1.2f,2),speed=new Vector2(.05f,.2f),size=new Vector2(.05f,.1f),
                colorA=new Color(.72f,.45f,1,.8f),colorB=new Color(.55f,.3f,.9f,.7f),gravity=-.12f,shape=Emit.Circle,radius=4,noise=.25f,offset=new Vector3(0,.2f,0),fadeIn=.2f,fadeOut=.5f,max=28};
            view.fx=Loop("Bellringer aura",transform,Vector3.zero,1,new[]{motes},1);view.go=view.fx.go;
            AddSprite(view.fx,"shockwave_ring",Blend.Add,new Color(.7f,.42f,1,.3f),new Vector3(0,.06f,0),8.2f,0,.5f,1.4f);
            AddSprite(view.fx,"glow_soft",Blend.Add,new Color(.45f,.25f,.75f,.09f),new Vector3(0,.05f,0),8.4f,0,.3f,1.4f).ratio=1.05f;
            return view;
        }

        public void ReleaseFlash(in TelegraphGauge.Footprint footprint)
        {
            if(!CanSpawn)return;EnsureConfigured();
            var state=new ReleaseFlashState{view=RentTelegraph(TelegraphKind.Fill),footprint=footprint};flashes.Add(state);ShowFlash(state);
        }
        void ShowFlash(in ReleaseFlashState s)
        {
            var f=s.footprint;float k=1-Mathf.Clamp01(s.age/FlashLife),swell=1+.08f*(1-k);
            ShowFill(s.view,f.shape,f.origin,f.end,f.direction,f.radius*swell,f.inner,f.angle,1,f.boss,k*k*(3-2*k));
        }
        void TickFlashes(float dt)
        {
            for(int i=flashes.Count-1;i>=0;i--)
            {
                var s=flashes[i];s.age+=dt;
                if(s.age>=FlashLife||s.view.go==null){ReturnTelegraph(s.view);flashes.RemoveAt(i);continue;}
                flashes[i]=s;ShowFlash(s);
            }
        }
        void ClearFlashes(){foreach(var s in flashes)ReturnTelegraph(s.view);flashes.Clear();}

        // progress = windup fraction (0 just started .. 1 lands/active); boss telegraphs are brighter; flash 1..0 = release flash.
        // tint (alpha > 0) replaces the danger colour: a zone that already landed burns in its element's colour, faint enough
        // for its own effects to show through.
        public void ShowFill(TelegraphView view,AttackShape shape,Vector2 origin,Vector2 end,Vector2 direction,float radius,float inner,float angle,float progress,bool boss,float flash=0,Color tint=default)
        {
            var t=view.go.transform;Vector3 at=new Vector3(origin.x,FillHeight,origin.y);Vector4 edges=Vector4.zero;
            var forward=new Vector3(direction.x,0,direction.y);if(forward.sqrMagnitude<1e-6f)forward=Vector3.forward;
            switch(shape)
            {
                case AttackShape.Line:
                {
                    var along=new Vector3(end.x-origin.x,0,end.y-origin.y);float length=along.magnitude;if(length>1e-4f)forward=along/length;
                    view.filter.sharedMesh=Band();t.SetPositionAndRotation(at,Quaternion.LookRotation(forward));t.localScale=new Vector3(Mathf.Max(.05f,radius*2),1,Mathf.Max(.05f,length));
                    edges=new Vector4(1,1,0,0);break;
                }
                case AttackShape.Sector:
                    view.filter.sharedMesh=Sector(angle);t.SetPositionAndRotation(at,Quaternion.LookRotation(forward));t.localScale=new Vector3(radius,1,radius);edges=new Vector4(0,1,0,0);break;
                case AttackShape.Ring:
                    view.filter.sharedMesh=Ring(radius>0?inner/radius:0);t.SetPositionAndRotation(at,Quaternion.identity);t.localScale=new Vector3(radius,1,radius);edges=new Vector4(1,0,0,0);break;
                default:
                    view.filter.sharedMesh=Disc();t.SetPositionAndRotation(at,Quaternion.identity);t.localScale=new Vector3(radius,1,radius);break;
            }
            block.Clear();block.SetFloat(ProgressId,Mathf.Clamp01(progress));block.SetFloat(FlashId,Mathf.Clamp01(flash));block.SetVector(EdgesId,edges);block.SetColor(TintId,tint.a>0?tint:boss?BossTelegraph:EnemyTelegraph);
            view.renderer.SetPropertyBlock(block);
        }
        public void ShowRefuge(TelegraphView view,Vector2 position,float radius)=>Place(view,position,radius*2);
        public void ShowAura(TelegraphView view,Vector2 position,float radius)
        {
            Place(view,position,radius*2.05f);
            if(view.fx.ps!=null){var shape=view.fx.ps.shape;if(!Mathf.Approximately(shape.radius,radius))shape.radius=radius;}
        }
        void Place(TelegraphView view,Vector2 position,float diameter)
        {
            view.go.transform.position=new Vector3(position.x,0,position.y);
            foreach(var s in view.fx.sprites){float d=diameter*s.ratio;s.scale=new Vector3(d,1,d);}
        }

        // ------------------------------------------------------------ fill meshes (shared, unit size)
        const int Around=64;
        Mesh Disc()
        {
            if(disc!=null)return disc;
            var v=new List<Vector3>();var uv=new List<Vector2>();var tris=new List<int>();
            for(int i=0;i<Around;i++)
            {
                float a0=i*Mathf.PI*2/Around,a1=(i+1)*Mathf.PI*2/Around;int b=v.Count;
                v.Add(Vector3.zero);uv.Add(new Vector2(0,(i+.5f)/Around));
                v.Add(new Vector3(Mathf.Sin(a0),0,Mathf.Cos(a0)));uv.Add(new Vector2(1,i/(float)Around));
                v.Add(new Vector3(Mathf.Sin(a1),0,Mathf.Cos(a1)));uv.Add(new Vector2(1,(i+1)/(float)Around));
                tris.Add(b);tris.Add(b+1);tris.Add(b+2);
            }
            return disc=Build("disc",v,uv,tris);
        }
        Mesh Ring(float innerRatio)
        {
            int key=Mathf.Clamp(Mathf.RoundToInt(innerRatio*100),0,98);
            if(rings.TryGetValue(key,out var mesh))return mesh;
            float k=key/100f;var v=new List<Vector3>();var uv=new List<Vector2>();var tris=new List<int>();
            for(int i=0;i<=Around;i++)
            {
                float a=i*Mathf.PI*2/Around;var d=new Vector3(Mathf.Sin(a),0,Mathf.Cos(a));
                v.Add(d*k);uv.Add(new Vector2(0,i/(float)Around));v.Add(d);uv.Add(new Vector2(1,i/(float)Around));
                if(i<Around){int b=i*2;tris.Add(b);tris.Add(b+1);tris.Add(b+2);tris.Add(b+2);tris.Add(b+1);tris.Add(b+3);}
            }
            mesh=Build("ring "+key,v,uv,tris);rings.Add(key,mesh);return mesh;
        }
        Mesh Sector(float degrees)
        {
            int key=Mathf.Clamp(Mathf.RoundToInt(degrees),1,360);
            if(sectors.TryGetValue(key,out var mesh))return mesh;
            int steps=Mathf.Max(4,key/5);var v=new List<Vector3>();var uv=new List<Vector2>();var tris=new List<int>();
            for(int i=0;i<steps;i++)
            {
                float t0=i/(float)steps,t1=(i+1)/(float)steps,a0=Mathf.Deg2Rad*(-key*.5f+key*t0),a1=Mathf.Deg2Rad*(-key*.5f+key*t1);int b=v.Count;
                v.Add(Vector3.zero);uv.Add(new Vector2(0,(t0+t1)*.5f));
                v.Add(new Vector3(Mathf.Sin(a0),0,Mathf.Cos(a0)));uv.Add(new Vector2(1,t0));
                v.Add(new Vector3(Mathf.Sin(a1),0,Mathf.Cos(a1)));uv.Add(new Vector2(1,t1));
                tris.Add(b);tris.Add(b+1);tris.Add(b+2);
            }
            mesh=Build("sector "+key,v,uv,tris);sectors.Add(key,mesh);return mesh;
        }
        // Line: x across (-.5..+.5 = UV.y 0..1), z along (0..1 = UV.x 0..1); the view scales it to width x length.
        Mesh Band()
        {
            if(band!=null)return band;
            var v=new List<Vector3>();var uv=new List<Vector2>();var tris=new List<int>();
            const int steps=8;
            for(int i=0;i<=steps;i++)
            {
                float z=i/(float)steps;v.Add(new Vector3(-.5f,0,z));uv.Add(new Vector2(z,0));v.Add(new Vector3(.5f,0,z));uv.Add(new Vector2(z,1));
                if(i<steps){int b=i*2;tris.Add(b);tris.Add(b+2);tris.Add(b+1);tris.Add(b+1);tris.Add(b+2);tris.Add(b+3);}
            }
            return band=Build("line",v,uv,tris);
        }
        Mesh Build(string name,List<Vector3> v,List<Vector2> uv,List<int> tris)
        {
            var mesh=Own(new Mesh{name="HELLSCRIPT telegraph "+name});mesh.SetVertices(v);mesh.SetUVs(0,uv);mesh.SetTriangles(tris,0);
            var normals=new Vector3[v.Count];for(int i=0;i<normals.Length;i++)normals[i]=Vector3.up;mesh.normals=normals;
            mesh.RecalculateBounds();mesh.UploadMeshData(false);return mesh;
        }
        void ClearTelegraphCaches()
        {
            freeViews.Clear();flashes.Clear();rings.Clear();sectors.Clear();arcs.Clear();disc=band=null;
            telegraph=edge=bossEdge=refugeEdge=auraEdge=guardEdge=rearEdge=tether=null;
        }
    }
}
