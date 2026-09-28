using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace Hellscript
{
    // Facts tools/art3d records per model in Resources/World/<Family>/manifest*.json ("models": id -> object with
    // height, glow "#rrggbb", archetype, pivots[], tris). found is false when no manifest lists the id.
    public struct WorldArtInfo{public bool found;public string id;public float height;public Color glow;public ActorArchetype archetype;public string[] pivots;public int tris;}

    // Generated world art lives under Resources/World/<Family>/<Id> (model), <Id>_A (albedo, alpha = smoothness) and
    // <Id>_N (tangent normal). Paths passed here are "<Family>/<Id>", e.g. "Characters/N01" or "Fields/F2/F2_Pillar".
    // Every lookup returns null/default for a missing asset so callers keep their primitive fallback.
    public static class WorldArt
    {
        public const string Root="World/";
        static readonly Dictionary<string,Dictionary<string,WorldArtInfo>> manifests=new Dictionary<string,Dictionary<string,WorldArtInfo>>();
        // Enter Play Mode keeps statics (domain reload is off): forget manifests read before the art was reinstalled.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] static void Reset()=>manifests.Clear();
        static GameObject Model(string path)=>string.IsNullOrEmpty(path)?null:Resources.Load<GameObject>(Root+path);
        public static string Id(string path)=>path.Substring(path.LastIndexOf('/')+1);
        public static bool Has(string path)=>Model(path)!=null;
        // The meshes are not CPU-readable in players (WorldArtImporter drops the copy), so Mesh.CombineMeshes fails on
        // them there: keep spawned art out of WorldView.BatchTownGeometry. The Editor reads them anyway and hides this.
        public static GameObject Spawn(string path,Transform parent,WorldArtMaterials materials,Func<Material,Material> resolve=null)
        {
            var model=Model(path);if(model==null)return null;
            var go=UnityEngine.Object.Instantiate(model,parent,false);go.name=Id(path);
            var material=materials==null?null:materials.Get(path,resolve);
            if(material!=null)foreach(var renderer in go.GetComponentsInChildren<Renderer>(true))renderer.sharedMaterial=material;
            return go;
        }
        public static WorldArtInfo Info(string path)
        {
            if(string.IsNullOrEmpty(path))return default;
            int slash=path.LastIndexOf('/');string family=slash<0?"":path.Substring(0,slash);
            if(!manifests.TryGetValue(family,out var models)){models=MergeManifests(Resources.LoadAll<TextAsset>(Root+family));manifests.Add(family,models);}
            return models.TryGetValue(Id(path),out var info)?info:default;
        }
        // Each workstream ships its own manifest_<group>.json; other TextAssets are ignored. Ordinal name order makes a
        // duplicate id deterministic (the last manifest wins); sorts texts in place.
        public static Dictionary<string,WorldArtInfo> MergeManifests(TextAsset[] texts)
        {
            var models=new Dictionary<string,WorldArtInfo>(StringComparer.Ordinal);Array.Sort(texts,(a,b)=>string.CompareOrdinal(a.name,b.name));
            foreach(var text in texts)if(text.name.StartsWith("manifest",StringComparison.Ordinal))foreach(var entry in ParseManifest(text.text))models[entry.Key]=entry.Value;
            return models;
        }
        public static Dictionary<string,WorldArtInfo> ParseManifest(string json)
        {
            var result=new Dictionary<string,WorldArtInfo>(StringComparer.Ordinal);
            try
            {
                if(!(Json.Parse(json) is Dictionary<string,object> root)||!(root.TryGetValue("models",out var list)&&list is Dictionary<string,object> models))return result;
                foreach(var entry in models)
                {
                    if(!(entry.Value is Dictionary<string,object> fields))continue;
                    var info=new WorldArtInfo{found=true,id=entry.Key,glow=WorldArtMaterials.DefaultGlow,pivots=Array.Empty<string>()};
                    if(fields.TryGetValue("height",out var height)&&height is double h)info.height=(float)h;
                    if(fields.TryGetValue("tris",out var tris)&&tris is double t)info.tris=(int)t;
                    if(fields.TryGetValue("glow",out var glow)&&glow is string hex&&ColorUtility.TryParseHtmlString(hex.StartsWith("#",StringComparison.Ordinal)?hex:"#"+hex,out var color))info.glow=color;
                    if(fields.TryGetValue("archetype",out var archetype)&&archetype is string name&&Enum.TryParse(name,true,out ActorArchetype type))info.archetype=type;
                    if(fields.TryGetValue("pivots",out var pivots)&&pivots is List<object> names)
                    {var array=new List<string>();foreach(var p in names)if(p is string s)array.Add(s);info.pivots=array.ToArray();}
                    result[entry.Key]=info;
                }
            }
            catch(Exception e){Debug.LogWarning("World art manifest ignored: "+e.Message);result.Clear();}
            return result;
        }
        // tools/art3d/hs3d.export_fbx flattens nested objects to "Name@Parent" so Blender's axis baking stays correct.
        // Two passes: every name is restored first, so chains (A@B, B@C) resolve in any order. World poses are kept.
        public static void RestoreHierarchy(Transform root)
        {
            var all=root.GetComponentsInChildren<Transform>(true);List<(Transform child,string parent)> moved=null;
            foreach(var t in all)
            {
                int at=t.name.IndexOf('@');if(at<=0||t==root)continue;
                (moved??=new List<(Transform,string)>()).Add((t,t.name.Substring(at+1)));t.name=t.name.Substring(0,at);
            }
            if(moved==null)return;
            var byName=new Dictionary<string,Transform>(StringComparer.Ordinal);foreach(var t in all)if(!byName.ContainsKey(t.name))byName.Add(t.name,t);
            foreach(var (child,parent) in moved)if(byName.TryGetValue(parent,out var p)&&p!=child&&!p.IsChildOf(child))child.SetParent(p,true);
        }
        // Minimal JSON reader for the manifests (objects, arrays, strings, numbers as double, true/false/null).
        sealed class Json
        {
            readonly string s;int i;
            Json(string s){this.s=s;}
            public static object Parse(string s){var json=new Json(s??"");var value=json.Value();json.Space();if(json.i!=json.s.Length)throw new FormatException("trailing data");return value;}
            void Space(){while(i<s.Length&&char.IsWhiteSpace(s[i]))i++;}
            char Peek(){Space();if(i>=s.Length)throw new FormatException("unexpected end");return s[i];}
            bool More(char close){char c=Peek();i++;if(c==',')return true;if(c==close)return false;throw new FormatException("expected , or "+close);}
            object Value()
            {
                char c=Peek();
                if(c=='{'){i++;var o=new Dictionary<string,object>(StringComparer.Ordinal);if(Peek()=='}'){i++;return o;}do{string key=Text();if(Peek()!=':')throw new FormatException("expected :");i++;o[key]=Value();}while(More('}'));return o;}
                if(c=='['){i++;var a=new List<object>();if(Peek()==']'){i++;return a;}do a.Add(Value());while(More(']'));return a;}
                if(c=='"')return Text();
                foreach(var (word,value) in Literals)if(string.CompareOrdinal(s,i,word,0,word.Length)==0){i+=word.Length;return value;}
                int start=i;while(i<s.Length&&"+-.0123456789eE".IndexOf(s[i])>=0)i++;
                return double.Parse(s.Substring(start,i-start),NumberStyles.Float,CultureInfo.InvariantCulture);
            }
            static readonly (string,object)[] Literals={("true",true),("false",false),("null",null)};
            string Text()
            {
                if(Peek()!='"')throw new FormatException("expected string");i++;var b=new StringBuilder();
                while(true)
                {
                    char c=s[i++];if(c=='"')return b.ToString();if(c!='\\'){b.Append(c);continue;}
                    char e=s[i++];
                    switch(e){case 'n':b.Append('\n');break;case 't':b.Append('\t');break;case 'r':b.Append('\r');break;case 'b':b.Append('\b');break;case 'f':b.Append('\f');break;
                        case 'u':b.Append((char)Convert.ToInt32(s.Substring(i,4),16));i+=4;break;default:b.Append(e);break;}
                }
            }
        }
    }

    // One world material per model id on the world shader (Resources/RiftTerrain). WorldView owns an instance and
    // disposes it; materials returned through a resolve function (RiftFogView.Resolve in rifts) belong to that resolver.
    public sealed class WorldArtMaterials:IDisposable
    {
        public static readonly Color DefaultGlow=new Color(1,.46f,.1f);
        public const float GlowIntensity=3;
        // The cool rim every world-art material starts with; WorldView keeps it under hit flashes and windup rims.
        public static readonly Color Rim=new Color(.3f,.38f,.5f);
        readonly Dictionary<string,Material> cache=new Dictionary<string,Material>(StringComparer.Ordinal);
        Shader shader;
        public int Count=>cache.Count;
        public Material Get(string path,Func<Material,Material> resolve=null)
        {
            if(string.IsNullOrEmpty(path))return null;
            if(!cache.TryGetValue(path,out var material)){material=Build(path);cache.Add(path,material);}
            return material!=null&&resolve!=null?resolve(material):material;
        }
        Material Build(string path)
        {
            if(shader==null)shader=Resources.Load<Shader>("RiftTerrain");
            if(shader==null)return null;
            var material=new Material(shader){name="World "+path};var info=WorldArt.Info(path);
            var albedo=Resources.Load<Texture2D>(WorldArt.Root+path+"_A");if(albedo!=null)material.SetTexture("_BaseMap",albedo);
            var normal=Resources.Load<Texture2D>(WorldArt.Root+path+"_N");if(normal!=null)material.SetTexture("_BumpMap",normal);
            material.SetColor("_BaseColor",Color.white);material.SetFloat("_BumpScale",1);material.SetFloat("_Smoothness",1);
            // HDR emission for the vertex-colour glow mask. World-shader colours are gamma values that Unity linearises on
            // upload (SetVector included, pow 2.2 above 1), so store the gamma of the wanted linear colour x GlowIntensity.
            var glow=((info.found?info.glow:DefaultGlow).linear*GlowIntensity).gamma;glow.a=1;material.SetColor("_GlowColor",glow);
            material.SetColor("_RimColor",Rim);material.SetFloat("_RimPower",3.5f);material.SetFloat("_FogEnabled",0);
            return material;
        }
        public void Dispose()
        {
            foreach(var material in cache.Values)if(material!=null){if(Application.isPlaying)UnityEngine.Object.Destroy(material);else UnityEngine.Object.DestroyImmediate(material);}
            cache.Clear();
        }
    }
}
