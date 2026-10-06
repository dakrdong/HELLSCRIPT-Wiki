using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Hellscript.Editor
{
    // Import rules for the generated world art (tools/art3d) and its test fixture; other folders are untouched.
    // Each model becomes ONE renderer with one submesh on its root: a SkinnedMeshRenderer when it has Pivot_* transforms
    // (every part weighted 100% to its nearest ancestor pivot, bones = root + pivots), otherwise a MeshFilter. Parts named
    // *_Glow get vertex colour alpha 1, the emission mask the world shader multiplies with _GlowColor.
    // Textures: <Id>_A albedo (sRGB, alpha = smoothness), <Id>_N tangent normal. Fields/ and Fx/ repeat and keep 1024 like Bosses/.
    public sealed class WorldArtImporter:AssetPostprocessor
    {
        public const string Root="Assets/HELLSCRIPT/Resources/World/",Fixtures="Assets/HELLSCRIPT/Tests/Editor/Fixtures/WorldArt/";
        public override uint GetVersion()=>4;
        // Tiling field sets, boss atlases and the town models keep 1024 (World/Town buildings are authored at 1024; its NPC and prop files are 512,
        // which a larger cap never enlarges); the town ground layers are capped at 512 (tile = 2.6-4 m, so 512 texels still give about
        // 1 texel per pixel or better on a 1080p screen at the closest zoom); everything else 512.
        public static int MaxTextureSize(string rel)=>rel.StartsWith("Fields/Town/",StringComparison.Ordinal)?512:
            rel.StartsWith("Fx/",StringComparison.Ordinal)||rel.StartsWith("Fields/",StringComparison.Ordinal)||rel.StartsWith("Bosses/",StringComparison.Ordinal)||
            rel.StartsWith("Town/",StringComparison.Ordinal)?1024:512;
        static string Relative(string path)=>path.StartsWith(Root,StringComparison.Ordinal)?path.Substring(Root.Length):path.StartsWith(Fixtures,StringComparison.Ordinal)?path.Substring(Fixtures.Length):null;
        static bool Keep(Transform t)=>t.name.StartsWith("Pivot_",StringComparison.Ordinal)||t.name.StartsWith("Socket_",StringComparison.Ordinal);
        void OnPreprocessModel()
        {
            if(Relative(assetPath)==null)return;
            var m=(ModelImporter)assetImporter;
            m.materialImportMode=ModelImporterMaterialImportMode.None;m.importCameras=false;m.importLights=false;m.importVisibility=false;m.importBlendShapes=false;
            m.importAnimation=false;m.animationType=ModelImporterAnimationType.None;m.isReadable=false;
            m.importNormals=ModelImporterNormals.Import;m.importTangents=ModelImporterTangents.CalculateMikk;
            m.globalScale=1;m.useFileScale=true;m.bakeAxisConversion=false;m.meshCompression=ModelImporterMeshCompression.Off;
        }
        void OnPostprocessModel(GameObject model)
        {
            if(Relative(assetPath)==null)return;
            var mesh=Build(model);if(mesh==null)return;
            // The importer only applies isReadable to its own meshes; drop the CPU copy of the merged one the same way.
            if(!((ModelImporter)assetImporter).isReadable)mesh.UploadMeshData(true);
            context.AddObjectToAsset("world-art-mesh",mesh);
        }
        // Restores the "Name@Parent" hierarchy, merges every part into one mesh and replaces the part renderers.
        public static Mesh Build(GameObject model)
        {
            var root=model.transform;WorldArt.RestoreHierarchy(root);
            var parts=model.GetComponentsInChildren<MeshFilter>(true);if(parts.Length==0)return null;
            var bones=new List<Transform>{root};foreach(var t in model.GetComponentsInChildren<Transform>(true))if(t!=root&&t.name.StartsWith("Pivot_",StringComparison.Ordinal))bones.Add(t);
            var rig=bones.Count>1?bones.ToArray():null;var mesh=Merge(root,parts,rig,model.name);
            foreach(var part in parts)
            {
                var go=part.gameObject;var source=part.sharedMesh;
                if(go.TryGetComponent<MeshRenderer>(out var renderer))UnityEngine.Object.DestroyImmediate(renderer);
                UnityEngine.Object.DestroyImmediate(part);if(source!=null)UnityEngine.Object.DestroyImmediate(source);
                if(go.transform==root||Keep(go.transform))continue;
                for(int i=go.transform.childCount-1;i>=0;i--)go.transform.GetChild(i).SetParent(go.transform.parent,true);
                UnityEngine.Object.DestroyImmediate(go);
            }
            if(rig!=null)
            {
                var skin=model.AddComponent<SkinnedMeshRenderer>();skin.sharedMesh=mesh;skin.bones=rig;skin.rootBone=root;skin.quality=SkinQuality.Bone1;
                // Room for procedural poses (raised arms, a body lying on the ground after death).
                var bounds=mesh.bounds;float reach=Mathf.Max(bounds.size.x,bounds.size.y,bounds.size.z);bounds.Expand(new Vector3(reach,reach*.5f,reach));skin.localBounds=bounds;
            }
            else{model.AddComponent<MeshFilter>().sharedMesh=mesh;model.AddComponent<MeshRenderer>();}
            return mesh;
        }
        // One submesh in root space. bones == null builds a static mesh; otherwise bones[0] must be root and each part's
        // vertices go to the nearest ancestor in bones (root if none). Colour = (1,1,1,glow mask).
        public static Mesh Merge(Transform root,IList<MeshFilter> parts,Transform[] bones,string name)
        {
            var vertices=new List<Vector3>();var normals=new List<Vector3>();var tangents=new List<Vector4>();var uv=new List<Vector2>();
            var colors=new List<Color32>();var weights=new List<BoneWeight>();var triangles=new List<int>();
            foreach(var part in parts)
            {
                var source=part.sharedMesh;if(source==null)continue;
                var m=root.worldToLocalMatrix*part.transform.localToWorldMatrix;var n=m.inverse.transpose;bool flip=m.determinant<0;
                int start=vertices.Count,count=source.vertexCount;var pv=source.vertices;var pn=source.normals;var pt=source.tangents;var pu=source.uv;
                byte glow=(byte)(part.name.EndsWith("_Glow",StringComparison.Ordinal)?255:0);int bone=bones==null?0:Bone(part.transform,bones);
                for(int i=0;i<count;i++)
                {
                    vertices.Add(m.MultiplyPoint3x4(pv[i]));normals.Add(pn.Length==count?n.MultiplyVector(pn[i]).normalized:Vector3.up);uv.Add(pu.Length==count?pu[i]:Vector2.zero);
                    if(pt.Length==count){var t=m.MultiplyVector(pt[i]).normalized;tangents.Add(new Vector4(t.x,t.y,t.z,flip?-pt[i].w:pt[i].w));}else tangents.Add(new Vector4(1,0,0,1));
                    colors.Add(new Color32(255,255,255,glow));if(bones!=null)weights.Add(new BoneWeight{boneIndex0=bone,weight0=1});
                }
                for(int sub=0;sub<source.subMeshCount;sub++)
                {
                    var tris=source.GetTriangles(sub);
                    for(int k=0;k<tris.Length;k+=3){triangles.Add(start+tris[k]);triangles.Add(start+tris[flip?k+2:k+1]);triangles.Add(start+tris[flip?k+1:k+2]);}
                }
            }
            var mesh=new Mesh{name=name,indexFormat=vertices.Count>65535?IndexFormat.UInt32:IndexFormat.UInt16};
            mesh.SetVertices(vertices);mesh.SetNormals(normals);mesh.SetTangents(tangents);mesh.SetUVs(0,uv);mesh.SetColors(colors);mesh.SetTriangles(triangles,0);
            if(bones!=null)
            {
                mesh.boneWeights=weights.ToArray();var poses=new Matrix4x4[bones.Length];
                for(int b=0;b<bones.Length;b++)poses[b]=bones[b].worldToLocalMatrix*root.localToWorldMatrix;
                mesh.bindposes=poses;
            }
            mesh.RecalculateBounds();return mesh;
        }
        static int Bone(Transform t,Transform[] bones){for(;t!=null;t=t.parent){int i=Array.IndexOf(bones,t);if(i>=0)return i;}return 0;}
        void OnPreprocessTexture()
        {
            string rel=Relative(assetPath);if(rel==null)return;
            var t=(TextureImporter)assetImporter;string file=System.IO.Path.GetFileNameWithoutExtension(assetPath);
            // Old minimal .meta files can migrate to Cubemap. These are sampled as 2D maps.
            t.textureShape=TextureImporterShape.Texture2D;
            bool normal=file.EndsWith("_N",StringComparison.Ordinal),albedo=file.EndsWith("_A",StringComparison.Ordinal),fx=rel.StartsWith("Fx/",StringComparison.Ordinal);
            t.textureType=normal?TextureImporterType.NormalMap:TextureImporterType.Default;t.sRGBTexture=!normal;
            t.alphaSource=TextureImporterAlphaSource.FromInput;t.alphaIsTransparency=fx&&!albedo&&!normal;
            t.mipmapEnabled=true;t.filterMode=FilterMode.Trilinear;t.anisoLevel=4;
            bool tiled=fx||rel.StartsWith("Fields/",StringComparison.Ordinal);
            t.wrapMode=tiled?TextureWrapMode.Repeat:TextureWrapMode.Clamp;
            t.textureCompression=TextureImporterCompression.CompressedHQ;
            t.maxTextureSize=MaxTextureSize(rel);
        }
    }
}
