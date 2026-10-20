using UnityEditor;
using UnityEngine;

namespace Hellscript.Editor
{
    // Title vista paintings: opaque far and close-up backdrops, cut-out hero cliffs and floating rocks with native alpha.
    // Only the hero cliffs keep trilinear mips: the flight's defocus taps read the mip level that matches their spacing.
    public sealed class TitleVistaArtImporter:AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if(!assetPath.StartsWith("Assets/HELLSCRIPT/Resources/Art/Title/Vista/",System.StringComparison.Ordinal))return;
            var importer=(TextureImporter)assetImporter;bool hero=assetPath.Contains("_heroes_"),cutout=hero||assetPath.Contains("_mid_");
            importer.textureType=TextureImporterType.Default;importer.alphaSource=cutout?TextureImporterAlphaSource.FromInput:TextureImporterAlphaSource.None;
            importer.alphaIsTransparency=cutout;importer.mipmapEnabled=hero;importer.sRGBTexture=true;importer.isReadable=false;
            importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=hero?FilterMode.Trilinear:FilterMode.Bilinear;importer.npotScale=TextureImporterNPOTScale.None;
        }
    }
}
