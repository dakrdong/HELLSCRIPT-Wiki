using UnityEditor;
using UnityEngine;

namespace Hellscript.Editor
{
    // Title vista paintings: opaque far and close-up backdrops, cut-out hero cliffs and floating rocks with native alpha.
    public sealed class TitleVistaArtImporter:AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if(!assetPath.StartsWith("Assets/HELLSCRIPT/Resources/Art/Title/Vista/",System.StringComparison.Ordinal))return;
            var importer=(TextureImporter)assetImporter;bool cutout=assetPath.Contains("_heroes_")||assetPath.Contains("_mid_");
            importer.textureType=TextureImporterType.Default;importer.alphaSource=cutout?TextureImporterAlphaSource.FromInput:TextureImporterAlphaSource.None;
            importer.alphaIsTransparency=cutout;importer.mipmapEnabled=false;importer.sRGBTexture=true;importer.isReadable=false;
            importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Bilinear;importer.npotScale=TextureImporterNPOTScale.None;
        }
    }
}
