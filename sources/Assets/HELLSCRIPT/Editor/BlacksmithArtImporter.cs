using UnityEditor;
using UnityEngine;
namespace Hellscript.Editor
{
    public sealed class BlacksmithArtImporter:AssetPostprocessor
    {
        public override uint GetVersion()=>1;
        void OnPreprocessTexture()
        {
            if(!assetPath.StartsWith("Assets/HELLSCRIPT/Resources/Art/Blacksmith/"))return;
            var importer=(TextureImporter)assetImporter;importer.textureType=TextureImporterType.Default;importer.mipmapEnabled=false;
            importer.alphaIsTransparency=true;importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Bilinear;
            importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=512;importer.npotScale=TextureImporterNPOTScale.None;
        }
    }
}
