using System;
using UnityEditor;
using UnityEngine;

namespace Hellscript.Editor
{
    public sealed class AttendanceArtImporter:AssetPostprocessor
    {
        public override uint GetVersion()=>3;
        void OnPreprocessTexture()
        {
            if(!assetPath.StartsWith("Assets/HELLSCRIPT/Resources/Art/Attendance/",StringComparison.Ordinal))return;
            var importer=(TextureImporter)assetImporter;
            importer.textureShape=TextureImporterShape.Texture2D;
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
            // The popup backdrop fills a panel up to ~1000 px wide on phones; icons stay small.
            importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.maxTextureSize=assetPath.EndsWith("/popup-altar.png",StringComparison.Ordinal)?1024:512;
            importer.textureCompression=TextureImporterCompression.Uncompressed;importer.wrapMode=TextureWrapMode.Clamp;
            importer.filterMode=FilterMode.Bilinear;importer.npotScale=TextureImporterNPOTScale.None;
        }
    }
}
