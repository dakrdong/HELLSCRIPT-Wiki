using System;
using UnityEditor;
using UnityEngine;

namespace Hellscript.Editor
{
    public sealed class AttendanceArtImporter:AssetPostprocessor
    {
        public override uint GetVersion()=>1;
        void OnPreprocessTexture()
        {
            if(!assetPath.StartsWith("Assets/HELLSCRIPT/Resources/Art/Attendance/",StringComparison.Ordinal))return;
            var importer=(TextureImporter)assetImporter;
            importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
            importer.alphaIsTransparency=true;importer.mipmapEnabled=false;importer.maxTextureSize=512;
            importer.textureCompression=TextureImporterCompression.Uncompressed;importer.wrapMode=TextureWrapMode.Clamp;
            importer.filterMode=FilterMode.Bilinear;importer.npotScale=TextureImporterNPOTScale.None;
        }
    }
}
