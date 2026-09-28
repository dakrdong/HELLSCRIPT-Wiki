#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Hellscript
{
    // Regression acceptance: render the real HUD, wait through its periodic writers, and resize again.
    public sealed class RuntimeHudScaleSmoke : MonoBehaviour
    {
        GameController game;string output;int samples;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Boot()
        {
            if(!Debug.isDebugBuild||!Environment.GetCommandLineArgs().Contains("-hellscriptHudScaleSmoke"))return;
            Application.runInBackground=true;
            Application.logMessageReceived+=(m,s,t)=>{if(t==LogType.Exception)Application.Quit(1);};
            new GameObject("HUD proportional text acceptance").AddComponent<RuntimeHudScaleSmoke>();
        }
        static void Require(bool value,string message){if(!value)throw new InvalidOperationException(message);}
        static Rect Pixels(RectTransform rect)
        {var corners=new Vector3[4];rect.GetWorldCorners(corners);return Rect.MinMaxRect(corners[0].x,corners[0].y,corners[2].x,corners[2].y);}
        string Check(string phase)
        {
            var page=game.UI.GetComponentsInChildren<Canvas>().Single(c=>c.name=="HELLSCRIPT UI");
            float expected=UiTheme.Scale(UiSafeArea.Current)*.5f*game.UI.InterfaceFactor;
            Require(Mathf.Abs(page.scaleFactor-expected)<.001f,"Page controls/text kept a fixed pixel scale: "+phase);
            var hud=game.UI.GlobalHud;var layout=hud.Layout;var style=GlobalHudStyle.Load();
            Require(!layout.wrapped&&layout.actives.Append(layout.ultimate).All(r=>Mathf.Abs(r.y-layout.actives[0].y)<.001f),"Skills split into two rows: "+phase);
            var captions=hud.GetComponentsInChildren<Text>().Where(t=>t.name=="Caption").ToArray();
            var state=hud.Snapshot;
            int expectedCaptions=state.actives.Count(s=>s!=null)+state.potions.Count(s=>s!=null)+(state.ultimate==null?0:1);
            Require(captions.Length==expectedCaptions,"Missing actual HUD captions.");
            foreach(var t in captions)
                Require(Mathf.Abs(t.fontSize-style.captionFont*layout.scale)<=.501f,"HUD text rebounded: "+phase);
            var parts=new[]{("Class seal",layout.seal),("HP",layout.hp),("Resource",layout.resource),("Experience",layout.xp),("Potion 0",layout.potions[0])};
            foreach(var part in parts)
            {
                var rect=(RectTransform)hud.transform.Find("HUD safe area/"+part.Item1);
                Require(Vector2.Distance(Pixels(rect).size,part.Item2.size*layout.scale)<.01f,"A HUD part scaled independently: "+part.Item1);
            }
            if(state.ultimate!=null)
            {
                var ultimate=(RectTransform)hud.transform.Find("HUD safe area/Ultimate");
                var active=(RectTransform)hud.transform.Find("HUD safe area/Active 0");
                Require(Mathf.Abs(Pixels(ultimate).y-Pixels(active).y)<.01f,"Ultimate moved above the four active skills");
            }
            var card=game.UI.GetComponentsInChildren<RectTransform>(true).Single(r=>r.name=="Town interaction card");
            Require(Mathf.Abs(Pixels(card).width-250*layout.scale)<1,"NPC interaction text and card no longer proportional: "+phase);
            foreach(var bubble in game.UI.GetComponentsInChildren<RectTransform>(true).Where(r=>r.name.StartsWith("NPC bubble ")||r.name.StartsWith("Resident bubble ")))
            {
                Require(Mathf.Abs(Pixels(bubble).width-208*layout.scale)<1,"Nameplate did not scale with the HUD: "+phase);
                foreach(var t in bubble.GetComponentsInChildren<Text>(true))
                {
                    Require(t.preferredHeight<=t.rectTransform.rect.height+1,"Nameplate line clipped: "+phase);
                    Require(Mathf.Abs(t.rectTransform.lossyScale.y-layout.scale)<.001f,"Nameplate font scale changed independently: "+phase);
                    if(Loc.Language=="en")Require(t.text.All(c=>c<128),"Untranslated English NPC label: "+t.text);
                }
            }
            string key=Screen.width+"x"+Screen.height+" scale="+layout.scale.ToString("0.000")+" caption="+captions[0].fontSize;
            File.AppendAllText(Path.Combine(output,"geometry.txt"),Loc.Language+" "+phase+" "+key+" PASS\n");samples++;return key;
        }
        IEnumerator Capture(string name)
        {
            yield return new WaitForEndOfFrame();var image=ScreenCapture.CaptureScreenshotAsTexture();
            File.WriteAllBytes(Path.Combine(output,name+".png"),image.EncodeToPNG());Destroy(image);
        }
        IEnumerator Start()
        {
            var args=Environment.GetCommandLineArgs();int save=Array.IndexOf(args,"-hellscriptSavePath"),shots=Array.IndexOf(args,"-hellscriptScreenshots");
            Require(save>=0&&shots>=0,"Use isolated save and evidence directories.");output=args[shots+1];Directory.CreateDirectory(output);File.WriteAllText(Path.Combine(output,"geometry.txt"),"");
            yield return new WaitForSecondsRealtime(1);game=FindAnyObjectByType<GameController>();Require(game?.Store!=null,"Game failed to initialize.");
            // This isolated layout fixture starts after the mandatory tutorial, not inside it.
            game.Store.Data.guide.legacyExempt=true;
            game.AttendancePopups.SetHidden(AttendanceKind.Weekly,true,game.Store.AttendanceClock());
            game.AttendancePopups.SetHidden(AttendanceKind.Monthly,true,game.Store.AttendanceClock());game.UI.CloseAttendance();
            game.EnterPlaza(true);UiSafeArea.AspectRatio=0;game.InterfaceScale.Apply(100);game.UI.ApplyInterfaceScale();
            game.RequestStation(TownStation.Merchant);for(int n=0;n<1000&&game.Town.Walking;n++)game.Town.Tick(.05f);
            foreach(string language in new[]{"ko","en"})
            {
                game.ApplyLanguage(language);
                if(language=="en")
                {
                    string[] expected={"Marc Kus","Jake Bokun","Chador Samaf","Anton Jindark","Injel Mir","Pyonya Nermwen","Jean Jorin","Darc Alvi"};
                    string[] source={"마르크 쿠스","제이크 보쿤","차도르 사마프","안톤 진다크","인젤 미르","표냐 내르뭰","쟝 죠린","달크 알뷔"};
                    for(int n=0;n<source.Length;n++)Require(Loc.T(source[n])==expected[n],"NPC English name missing: "+source[n]);
                }
                foreach(var size in new[]{new Vector2Int(1600,900),new Vector2Int(800,450),new Vector2Int(640,360),new Vector2Int(480,270),new Vector2Int(1600,900),new Vector2Int(900,1600),new Vector2Int(450,800),new Vector2Int(360,640)})
                {
                    Screen.SetResolution(size.x,size.y,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.5f);
                    game.UI.RefreshHud();game.UI.RefreshPlaza();Canvas.ForceUpdateCanvases();string first=Check("settled");
                    yield return new WaitForSecondsRealtime(1.5f);Canvas.ForceUpdateCanvases();Require(Check("after-refresh")==first,"Text grew after periodic refresh.");
                    if(size.x==800||size.x==450||size.x==1600)yield return Capture(language+"-"+size.x+"x"+size.y);
                }
            }
            foreach(int percent in new[]{50,150})
            {
                game.InterfaceScale.Apply(percent);game.UI.ApplyInterfaceScale();yield return new WaitForSecondsRealtime(.5f);game.UI.RefreshPlaza();Check("preference-"+percent);
            }
            game.InterfaceScale.Apply(100);game.UI.ApplyInterfaceScale();Screen.SetResolution(800,450,FullScreenMode.Windowed);yield return new WaitForSecondsRealtime(.6f);
            foreach(var npc in TownLayout.Stations.Where(s=>!string.IsNullOrEmpty(s.npcName)).Select(s=>(id:"NPC bubble "+s.id,name:s.npcName,position:s.position))
                .Concat(TownLayout.Residents.Select(r=>(id:"Resident bubble "+r.id,name:r.name,position:r.position))))
            {
                game.Town.RequestPoint(npc.position);for(int n=0;n<1000&&game.Town.Walking;n++)game.Town.Tick(.05f);
                yield return new WaitForSecondsRealtime(.6f);game.UI.RefreshPlaza();Canvas.ForceUpdateCanvases();
                var bubble=game.UI.GetComponentsInChildren<RectTransform>().Single(r=>r.name==npc.id);
                Require(bubble.gameObject.activeInHierarchy,"Nearby English NPC name is hidden: "+npc.name);
                var name=bubble.Find("NPC name").GetComponent<Text>();Require(name.text==Loc.T(npc.name)&&name.text.All(ch=>ch<128),"Wrong visible English NPC name.");
                var bounds=Pixels(bubble);Require(UiSafeArea.Current.Contains(bounds.min)&&UiSafeArea.Current.Contains(bounds.max),"NPC name escaped the safe area.");
                File.AppendAllText(Path.Combine(output,"geometry.txt"),"VISIBLE English NPC: "+name.text+" PASS\n");
            }
            File.WriteAllText(Path.Combine(output,"runtime.txt"),"PASS: "+samples+" actual HUD/nameplate/card samples, Korean and English, shrink/restore/portrait transitions, unchanged fonts after 1.5s of periodic refresh, 50/150% preference, all eight English NPCs visited with visible names inside the safe area. macOS development player; no physical mobile proof.\n");
            Debug.Log("HELLSCRIPT_HUD_SCALE_SMOKE_OK");Application.Quit(0);
        }
    }
}

#endif
