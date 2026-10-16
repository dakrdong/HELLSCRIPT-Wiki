using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hellscript
{
    public static class RuneV13Art
    {
        [Serializable] sealed class Index {public string[] ids;public int columns,tile;}
        static Index index;static Texture2D atlas;static Dictionary<string,int> positions;
        public static Texture2D Atlas=>atlas??=Resources.Load<Texture2D>("Runes/V13/glyph-atlas");
        static void Ensure()
        {if(index!=null)return;index=JsonUtility.FromJson<Index>(Resources.Load<TextAsset>("Runes/V13/glyphs").text);positions=new Dictionary<string,int>();for(int i=0;i<index.ids.Length;i++)positions[index.ids[i]]=i;}
        static Rect Uv(int i)
        {int rows=Mathf.CeilToInt((index.ids.Length+1f)/index.columns);return new Rect((i%index.columns)/(float)index.columns,1-(i/index.columns+1f)/rows,1f/index.columns,1f/rows);}
        public static Rect Glyph(string id){Ensure();return Uv(positions.TryGetValue(id,out int i)?i:positions["b-root"]);}
        public static Vector2 White{get{Ensure();return Uv(index.ids.Length).center;}}
        public static Color Color(string hex){ColorUtility.TryParseHtmlString(hex,out var c);return c;}
        // Match the reference's inventory pose without changing owned shapes or saved rotations.
        public static int StorageRotation(string shapeId)=>shapeId switch
        {"rune-2-01"=>2,"rune-3-01"=>2,"rune-3-02"=>1,"rune-4-06"=>4,"rune-5-05"=>1,_=>0};
    }
}
