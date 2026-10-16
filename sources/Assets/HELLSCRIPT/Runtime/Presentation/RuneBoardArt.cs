using System.Collections.Generic;
using Hellscript.Runes;
using UnityEngine;
using UnityEngine.UI;
namespace Hellscript
{
    public static partial class RuneBoardArt
    {
        static readonly Dictionary<string,Texture2D> textures=new Dictionary<string,Texture2D>();
        public static readonly string[] Types={"attack","magic","support","critical","skill"};
        public static Texture2D Texture(string name)
        {if(!textures.TryGetValue(name,out var texture)){texture=Resources.Load<Texture2D>(ResourceName(name));textures[name]=texture;}return texture;}
        static readonly Dictionary<string,Rect> paintedTiles=new Dictionary<string,Rect>
        {
            {"Tiles/bed-socket-a",new Rect(0f,0.083984375f,1f,0.8671875f)},
            {"Tiles/gem-attack-a",new Rect(0f,0.078125f,1f,0.876953125f)},
            {"Tiles/bed-slab-a",new Rect(0.000000000f,0.078125000f,1.000000000f,0.878906250f)},
            {"Tiles/bed-slab-b",new Rect(0.000000000f,0.076171875f,1.000000000f,0.882812500f)},
            {"Tiles/gem-magic-a",new Rect(0.000000000f,0.076171875f,1.000000000f,0.880859375f)},
            {"Tiles/gem-support-a",new Rect(0.000000000f,0.076171875f,1.000000000f,0.882812500f)},
            {"Tiles/gem-critical-a",new Rect(0.000000000f,0.076171875f,1.000000000f,0.880859375f)},
            {"Tiles/gem-skill-a",new Rect(0.000000000f,0.080078125f,1.000000000f,0.876953125f)},
            {"Tiles/gem-root-a",new Rect(0.000000000f,0.076171875f,1.000000000f,0.882812500f)},
            {"Tiles/gem-elite-a",new Rect(0.000000000f,0.076171875f,1.000000000f,0.882812500f)},
        };
        static readonly Dictionary<string,string> paintedTileResources=new Dictionary<string,string>
        {
            {"Tiles/bed-socket-a","Art/RuneBoard/Casual/Tiles/bed-socket-a"},
            {"Tiles/gem-attack-a","Art/RuneBoard/Casual/Tiles/gem-attack-a"},
            {"Tiles/bed-slab-a","Art/RuneBoard/Casual/Tiles/bed-locked-a"},
            {"Tiles/bed-slab-b","Art/RuneBoard/Casual/Tiles/bed-raised-a"},
            {"Tiles/gem-magic-a","Art/RuneBoard/Casual/Tiles/gem-magic-a"},
            {"Tiles/gem-support-a","Art/RuneBoard/Casual/Tiles/gem-support-a"},
            {"Tiles/gem-critical-a","Art/RuneBoard/Casual/Tiles/gem-critical-a"},
            {"Tiles/gem-skill-a","Art/RuneBoard/Casual/Tiles/gem-skill-a"},
            {"Tiles/gem-root-a","Art/RuneBoard/Casual/Tiles/gem-root-a"},
            {"Tiles/gem-elite-a","Art/RuneBoard/Casual/Tiles/gem-elite-a"},
        };
        static string TileKey(string name)=>name.StartsWith("Tiles/Dim/",System.StringComparison.Ordinal)?"Tiles/"+name.Substring(10):name;
        static readonly Dictionary<string,string> paintedIcons=new Dictionary<string,string>
        {
            {"Icons/icon-eye","Art/GlobalHUD/Casual/status-eye"},
            {"Icons/icon-focus","Art/Options/Casual/button-fit"},
            {"Icons/icon-help","Art/Options/Casual/section-guide"},
            {"Icons/icon-lock","Art/Options/Casual/notice-lock"},
            {"Icons/icon-presets","Art/GlobalHUD/Casual/menu-edict-presets"},
            {"Icons/icon-revert","Art/Options/Casual/button-restore"},
            {"Icons/icon-overview","Art/RuneBoard/Casual/Icons/icon-overview"},
            {"Icons/icon-recall","Art/RuneBoard/Casual/Icons/icon-recall"},
            {"Icons/icon-rotate","Art/RuneBoard/Casual/Icons/icon-rotate"},
            {"Icons/icon-save","Art/RuneBoard/Casual/Icons/icon-save"},
            {"Icons/icon-undo","Art/RuneBoard/Casual/Icons/icon-undo"},
        };
        static readonly Dictionary<string,Rect> paintedIconBounds=new Dictionary<string,Rect>
        {
            {"Icons/icon-eye",new Rect(0.019531250f,0.109375000f,0.962890625f,0.783203125f)},
            {"Icons/icon-focus",new Rect(0.000000000f,0.054687500f,1.000000000f,0.882812500f)},
            {"Icons/icon-help",new Rect(0.000000000f,0.101562500f,1.000000000f,0.804687500f)},
            {"Icons/icon-lock",new Rect(0.085937500f,0.000000000f,0.828125000f,0.992187500f)},
            {"Icons/icon-presets",new Rect(0.042968750f,0.109375000f,0.906250000f,0.781250000f)},
            {"Icons/icon-revert",new Rect(0.023437500f,0.031250000f,0.976562500f,0.937500000f)},
            {"Icons/icon-overview",new Rect(0.017543860f,0.125199362f,0.967304625f,0.775119617f)},
            {"Icons/icon-recall",new Rect(0.046251994f,0.090111643f,0.909888357f,0.823763955f)},
            {"Icons/icon-rotate",new Rect(0.071770335f,0.063795853f,0.853269537f,0.886762360f)},
            {"Icons/icon-save",new Rect(0.126794258f,0.017543860f,0.740829346f,0.940191388f)},
            {"Icons/icon-undo",new Rect(0.071770335f,0.047846890f,0.858054226f,0.907496013f)},
        };
        public static bool IsPaintedIcon(string name)=>paintedIcons.ContainsKey(name);
        static string ResourceName(string name)=>paintedTileResources.TryGetValue(TileKey(name),out var path)||paintedIcons.TryGetValue(name,out path)?path:"Art/RuneBoard/"+name;
        public static bool IsPaintedDim(string name)=>name.StartsWith("Tiles/Dim/",System.StringComparison.Ordinal)&&paintedTiles.ContainsKey(TileKey(name));
        // Authored alpha >= 8 bounds with four pixels of padding on each registered canvas.
        static readonly Dictionary<string,Rect> categoryBounds=new Dictionary<string,Rect>
        {
            {"Icons/Casual/rune-attack",new Rect(0.106858054f,0.037480064f,0.786283892f,0.924242424f)},
            {"Icons/Casual/rune-magic",new Rect(0.106858054f,0.030303030f,0.786283892f,0.932216906f)},
            {"Icons/Casual/rune-support",new Rect(0.084529506f,0.027113238f,0.830940989f,0.949760766f)},
            {"Icons/Casual/rune-critical",new Rect(0.106060606f,0.039872408f,0.787878788f,0.914673046f)},
            {"Icons/Casual/rune-skill",new Rect(0.096491228f,0.030303030f,0.806220096f,0.937799043f)},
        };
        public static Rect UV(string name)=>paintedTiles.TryGetValue(TileKey(name),out var uv)||paintedIconBounds.TryGetValue(name,out uv)||categoryBounds.TryGetValue(name,out uv)||Bounds.TryGetValue(name,out uv)?uv:new Rect(0,0,1,1);
        public static string Gem(int type,bool elite=false,bool dim=false)=>"Tiles/"+(dim?"Dim/":"")+"gem-"+(elite?"elite":type<0?"root":Types[type])+"-a";
        public static Color Color(string hex)=>RuneV13Art.Color(hex);
        public static RuneV13Type Palette(int type,bool elite=false)=>elite?new RuneV13Type{ink="#e8d7ae",bright="#fff4d6",dark="#4d3f22",face="#9b8650"}:type<0?new RuneV13Type{ink="#e2d6bb",bright="#fff6dc",dark="#4a4531",face="#8a8160"}:RuneV13Catalog.Data.types[type];
        public static string Emblem(int type)=>"Icons/Casual/rune-"+Types[type];
        // Accepted category paintings also identify the small piece-type emblem; per-cell glyphs remain unchanged.
        public static RawImage CategoryImage(Transform parent,int type,float x,float y,float size)
        {
            return Image(parent,"Icons/Casual/rune-"+Types[type],x,y,size,UnityEngine.Color.white);
        }
        public static RawImage Image(Transform parent,string name,float x,float y,float size,Color color)
        {
            var r=UiLayout.Rect(name,parent);var uv=UV(name);float ratio=uv.width/uv.height;
            float w=size*.875f*Mathf.Min(1,ratio),h=size*.875f*Mathf.Min(1,1/ratio);
            UiLayout.Place(r,x+(size-w)/2,y+(size-h)/2,w,h);var raw=r.gameObject.AddComponent<RawImage>();raw.texture=Texture(name);raw.uvRect=uv;raw.raycastTarget=false;raw.color=IsPaintedIcon(name)?new UnityEngine.Color(1,1,1,color.a):color;return raw;
        }
    }
}
