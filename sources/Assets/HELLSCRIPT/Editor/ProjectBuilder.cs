using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace Hellscript.Editor
{
    public static class ProjectBuilder
    {
        public const string ScenePath="Assets/HELLSCRIPT/Scenes/Hellscript.unity";
        const string CatalogPath="Assets/HELLSCRIPT/Resources/GameCatalog.asset";
        [MenuItem("HELLSCRIPT/프로젝트 구성")]
        public static void Setup()
        {
            EnsureFolder("Assets/HELLSCRIPT/Scenes");EnsureFolder("Assets/HELLSCRIPT/Resources/Materials");
            var catalog=AssetDatabase.LoadAssetAtPath<GameCatalog>(CatalogPath);
            if(catalog==null){catalog=ScriptableObject.CreateInstance<GameCatalog>();catalog.Populate();AssetDatabase.CreateAsset(catalog,CatalogPath);}
            var shader=Shader.Find("Universal Render Pipeline/Lit");if(shader==null)throw new InvalidOperationException("URP Lit shader is unavailable.");
            const string matPath="Assets/HELLSCRIPT/Resources/Materials/RuntimeShader.mat";
            if(AssetDatabase.LoadAssetAtPath<Material>(matPath)==null)AssetDatabase.CreateAsset(new Material(shader){name="Runtime Shader Reference"},matPath);
            foreach(string image in new[]{"Sanctuary","SkillAtlas","EquipmentAtlas"})
            {
                var importer=(TextureImporter)AssetImporter.GetAtPath("Assets/HELLSCRIPT/Resources/Art/"+image+".png");
                if(importer!=null&&(importer.mipmapEnabled||importer.maxTextureSize!=2048))
                {importer.textureType=TextureImporterType.Default;importer.mipmapEnabled=false;importer.maxTextureSize=2048;importer.textureCompression=TextureImporterCompression.Compressed;importer.wrapMode=TextureWrapMode.Clamp;importer.SaveAndReimport();}
            }
            var paving=(TextureImporter)AssetImporter.GetAtPath("Assets/HELLSCRIPT/Resources/Art/RiftStone.png");
            if(paving!=null&&(!paving.mipmapEnabled||paving.wrapMode!=TextureWrapMode.Repeat||paving.maxTextureSize!=1024||paving.filterMode!=FilterMode.Trilinear))
            {paving.textureType=TextureImporterType.Default;paving.mipmapEnabled=true;paving.wrapMode=TextureWrapMode.Repeat;paving.maxTextureSize=1024;paving.npotScale=TextureImporterNPOTScale.ToNearest;paving.filterMode=FilterMode.Trilinear;paving.anisoLevel=2;paving.textureCompression=TextureImporterCompression.Compressed;paving.SaveAndReimport();}
            if(!File.Exists(ScenePath))
            {
                var active=EditorSceneManager.GetActiveScene();
                var mode=Application.isBatchMode||string.IsNullOrEmpty(active.path)&&!active.isDirty?NewSceneMode.Single:NewSceneMode.Additive;
                var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,mode);
                UnityEngine.SceneManagement.SceneManager.SetActiveScene(scene);
                var root=new GameObject("HELLSCRIPT");Undo.RegisterCreatedObjectUndo(root,"Create HELLSCRIPT scene");
                var game=root.AddComponent<GameController>();game.catalog=catalog;
                EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene,ScenePath);
            }
            var existing=EditorBuildSettings.scenes.Where(s=>s.path!=ScenePath).ToList();existing.Insert(0,new EditorBuildSettingsScene(ScenePath,true));EditorBuildSettings.scenes=existing.ToArray();
            PlayerSettings.productName="HELLSCRIPT";PlayerSettings.defaultScreenWidth=720;PlayerSettings.defaultScreenHeight=1280;
            PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.defaultInterfaceOrientation=UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait=false;PlayerSettings.allowedAutorotateToPortraitUpsideDown=false;PlayerSettings.allowedAutorotateToLandscapeLeft=false;PlayerSettings.allowedAutorotateToLandscapeRight=false;
            AssetDatabase.SaveAssets();Debug.Log("HELLSCRIPT_SETUP_OK scene="+ScenePath+" skills="+catalog.skills.Count);
        }
        static void EnsureFolder(string path)
        {
            if(AssetDatabase.IsValidFolder(path))return;string parent=Path.GetDirectoryName(path).Replace('\\','/');EnsureFolder(parent);AssetDatabase.CreateFolder(parent,Path.GetFileName(path));
        }
        [MenuItem("HELLSCRIPT/게임 시작")]
        public static void Play()
        {
            Setup();if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
            EditorSceneManager.OpenScene(ScenePath);EditorApplication.isPlaying=true;
        }
        public static void BuildMac()
        {
            string output="Builds/macOS/HELLSCRIPT.app";var args=Environment.GetCommandLineArgs();
            int outputIndex=Array.IndexOf(args,"-hellscriptBuildOutput");
            if(outputIndex>=0)
            {
                if(outputIndex+1>=args.Length||args[outputIndex+1].StartsWith("-",StringComparison.Ordinal))throw new ArgumentException("-hellscriptBuildOutput requires an .app path.");
                output=args[outputIndex+1];
            }
            output=Path.GetFullPath(output);
            if(!output.EndsWith(".app",StringComparison.OrdinalIgnoreCase))throw new ArgumentException("macOS build output must end in .app.");
            Setup();Directory.CreateDirectory(Path.GetDirectoryName(output));
            BuildReport report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{ScenePath},locationPathName=output,target=BuildTarget.StandaloneOSX,options=BuildOptions.Development});
            Debug.Log("HELLSCRIPT_BUILD "+report.summary.result+" errors="+report.summary.totalErrors+" output="+output);
            if(report.summary.result!=BuildResult.Succeeded)throw new InvalidOperationException("HELLSCRIPT build failed");
        }
        public static void ValidateSimulation()
        {
            var catalog=ScriptableObject.CreateInstance<GameCatalog>();catalog.Populate();
            string report="HELLSCRIPT simulation validation\n";
            for(int c=0;c<3;c++)
            {
                var account=GameStore.NewAccount();account.selectedHero=c;var sim=new CombatSimulation(account,catalog,1,1);
                int ticks=0;while(sim.State.phase!=RunPhase.Cleared&&sim.State.phase!=RunPhase.Failed&&ticks++<6001)sim.Tick(.05f);
                report+=$"Class {c}: {sim.State.phase}, kills={sim.State.kills}, HP={sim.State.health:0}, time={sim.State.time:0.0}, damage={sim.State.dealt:0}\n";
                if(float.IsNaN(sim.State.health)||sim.State.kills==0)throw new InvalidOperationException("Simulation did not progress: "+c);
            }
            for(int c=0;c<3;c++)
            {
                var account=GameStore.NewAccount();account.selectedHero=c;
                for(int attempt=0;attempt<8;attempt++)
                {
                    var sim=new CombatSimulation(account,catalog,1);
                    for(int tick=0;tick<6100&&sim.State.phase!=RunPhase.Cleared&&sim.State.phase!=RunPhase.Failed;tick++)sim.Tick(.05f);
                    report+=$"Rift class={c} attempt={attempt+1}: {sim.State.phase}, level={account.Hero.level}, kills={sim.State.kills}, meter={sim.State.meter}, time={sim.State.time:0.0}\n";
                    if(sim.State.phase==RunPhase.Looting&&sim.State.portal)sim.Abandon();
                    foreach(var item in account.Hero.inventory.Where(x=>!x.equipped).OrderBy(x=>x.level).ThenBy(x=>x.rarity).ToArray())Economy.Equip(account.Hero,item);
                    if(account.Hero.highestClear>0)break;
                }
            }
            Directory.CreateDirectory("Artifacts/Validation");File.WriteAllText("Artifacts/Validation/simulation.txt",report);Debug.Log(report);
        }
    }
}
