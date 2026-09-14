using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hellscript
{
    public sealed class RuntimeVisibilitySmoke : MonoBehaviour
    {
        GameController game; string output, save;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Install()
        {
            if (!Debug.isDebugBuild || !Environment.GetCommandLineArgs().Contains("-hellscriptVisibilitySmoke")) return;
            Application.runInBackground = true;
            Application.logMessageReceived += (message, stack, type) => { if (type == LogType.Exception) Application.Quit(1); };
            new GameObject("Visibility acceptance").AddComponent<RuntimeVisibilitySmoke>();
        }
        static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
        static Rect Pixels(RectTransform rect) { var corners = new Vector3[4]; rect.GetWorldCorners(corners); return new Rect(corners[0], corners[2] - corners[0]); }
        IEnumerator Capture(string name)
        {
            yield return new WaitForSecondsRealtime(.2f); Canvas.ForceUpdateCanvases(); yield return new WaitForEndOfFrame();
            var image = ScreenCapture.CaptureScreenshotAsTexture(); File.WriteAllBytes(Path.Combine(output, name + ".png"), image.EncodeToPNG()); Destroy(image);
        }
        void Move(Vector2 p)
        { game.Combat.State.position = p; game.Combat.State.visibility.Update(); game.World.RevealPresentation(game.Combat.State); }
        void Click(string name)
        { game.UI.GetComponentsInChildren<Button>().Single(b => b.name == name).onClick.Invoke(); }
        void CheckOverlay()
        {
            Canvas.ForceUpdateCanvases(); var overlay = game.UI.GetComponentsInChildren<RiftAutomap>().Single();
            var marker = game.UI.GetComponentsInChildren<RiftAutomapHero>().Single();
            Vector2 center = new Vector2(game.UI.BattleViewport.center.x * Screen.width, game.UI.BattleViewport.center.y * Screen.height);
            Require(Vector2.Distance(Pixels(marker.rectTransform).center, center) < 1, "Overlay player is not centered in the battle viewport.");
            Require(!overlay.raycastTarget && !marker.raycastTarget, "Map consumes pointer input.");
            var hits = new List<RaycastResult>(); EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = center }, hits);
            Require(hits.All(h => h.gameObject.GetComponent<RiftAutomap>() == null && h.gameObject.GetComponent<RiftAutomapHero>() == null), "Overlay intercepts clicks.");
            var mesh = overlay.canvasRenderer.GetMesh(); Require(mesh != null && mesh.vertexCount > 0 && mesh.vertexCount < 65000, "Map exceeded the UI vertex limit or is empty.");
        }
        IEnumerator Start()
        {
            var args = Environment.GetCommandLineArgs(); int pi = Array.IndexOf(args, "-hellscriptSavePath"), oi = Array.IndexOf(args, "-hellscriptScreenshots");
            Require(pi >= 0 && oi >= 0, "Isolated save and evidence paths are required."); save = args[pi + 1]; output = args[oi + 1]; Directory.CreateDirectory(output);
            yield return new WaitForSecondsRealtime(1); game = FindAnyObjectByType<GameController>();
            if (args.Contains("-hellscriptVisibilityResume"))
            {
                Require(!game.OverlayMap.Enabled, "Overlay preference did not survive native restart.");
                int remembered = game.Store.Data.suspendedRun.discovery.cells.Count;
                game.Begin(resume: true); game.Combat.State.paused = true;
                Require(game.Combat.State.discovery.cells.Count >= remembered, "Native restart lost explored terrain.");
                Require(game.UI.GetComponentsInChildren<RiftAutomap>().Length == 0, "Disabled overlay returned after restart.");
                yield return Capture("09-restart"); File.WriteAllText(Path.Combine(output, "restart.txt"), "PASS: explored cells and disabled overlay survived a separate process.\n");
                Debug.Log("HELLSCRIPT_VISIBILITY_RESTART_OK"); Application.Quit(0); yield break;
            }
            game.ApplyAspect("16:9"); yield return new WaitForSecondsRealtime(.4f);
            var fixture = new CombatSimulation(game.Store.Data, game.catalog, 1, seed: 7421).State;
            var enemy = fixture.enemies[0]; enemy.position = new Vector2(4, .25f); enemy.speed = 0; fixture.enemies.Clear(); fixture.enemies.Add(enemy);
            var map = new RiftLayout { version = RiftLayout.CurrentVersion, fingerprint = "visibility-acceptance", start = new Vector2(-2.5f,0), gateOpen = true, roamingBoss = true };
            map.rooms.Add(new RiftRoom { index = 0, templateId = "Room A", position = new Vector2(-8, 0), size = new Vector2(12, 12) });
            map.rooms.Add(new RiftRoom { index = 1, templateId = "Room B", position = new Vector2(8, 0), size = new Vector2(12, 12) });
            map.rooms[0].doors.Add(new RiftDoor { corridor = 0, position = new Vector2(-2,0), direction = Vector2.right, width = 2 });
            map.rooms[1].doors.Add(new RiftDoor { corridor = 0, position = new Vector2(2,0), direction = Vector2.left, width = 2 });
            map.corridors.Add(new RiftCorridor { index = 0, roomA = 0, roomB = 1, width = 2, points = new List<Vector2> { new Vector2(-2, 0), new Vector2(2, 0) } });
            map.obstacles.Add(new RiftObstacle { id = "pillar", kind = "Pillar", position = new Vector2(7,0), radius = .9f, height = 3 });
            map.chests.Add(new RiftChest { id = "visibility-chest", definitionId = "CH01", room = 1, position = new Vector2(5,4), phase = ChestPhase.Available, discovered = true });
            fixture.layout = map; fixture.position = map.start; fixture.discovery = new RiftDiscovery { fingerprint = map.fingerprint }; fixture.visibility = null;
            game.Store.Data.suspendedRun = fixture; game.Begin(resume: true); game.Combat.State.paused = true;
            var run = game.Combat.State; Move(map.start); yield return Capture("01-doorway"); CheckOverlay();
            Require(run.visibility.Explored(new Vector2(4.25f,.25f)) && !run.visibility.Explored(new Vector2(4.25f,4.25f)), "Doorway revealed the whole room or failed to reveal the visible part.");
            Require(!GameObject.Find("Rift Runtime").transform.Find("visibility-chest").gameObject.activeSelf, "Chest behind the doorway is visible.");
            Move(new Vector2(5,3)); yield return Capture("02-room");
            Require(GameObject.Find("Rift Runtime").transform.Find("visibility-chest").gameObject.activeSelf, "Visible chest remains hidden.");
            Move(new Vector2(-4,4)); yield return Capture("03-remembered");
            Require(run.visibility.Explored(new Vector2(5.25f,4.25f)) && !run.visibility.Visible(new Vector2(5,4)), "Remembered terrain lost its separate state.");
            Require(!GameObject.Find("Rift Runtime").transform.Find("visibility-chest").gameObject.activeSelf, "Remembered terrain retained the chest.");
            game.UI.ShowScreenSettings(); Canvas.ForceUpdateCanvases(); DialogReadingAnchor.Show((RectTransform)game.UI.GetComponentsInChildren<Toggle>().Single(t => t.name == "settings-overlay-map").transform); yield return Capture("04-settings-ko");
            var toggle = game.UI.GetComponentsInChildren<Toggle>().Single(t => t.name == "settings-overlay-map");
            var center = Pixels((RectTransform)toggle.transform.Find("Toggle track")).center;var pointer = new PointerEventData(EventSystem.current) { button = PointerEventData.InputButton.Left, position = center };
            var hits = new List<RaycastResult>(); EventSystem.current.RaycastAll(pointer, hits);Require(hits.Count > 0 && hits[0].gameObject.GetComponentInParent<Toggle>() == toggle, "Pointer cannot reach overlay toggle.");
            toggle.OnPointerClick(pointer); Require(!game.OverlayMap.Enabled && !new OverlayMapSettings(save).Enabled, "Toggle did not apply and persist.");
            game.UI.CloseCommonPanel(); yield return Capture("05-overlay-off"); Require(game.UI.GetComponentsInChildren<RiftAutomap>().Length == 0, "Overlay remains visible when disabled.");
            Move(new Vector2(10,4)); int explored = run.discovery.cells.Count; Move(new Vector2(-4,4)); Require(run.discovery.cells.Count >= explored, "Hidden map stopped exploration.");
            game.ApplyLanguage("en"); game.ApplyAspect("9:16"); yield return new WaitForSecondsRealtime(.4f); game.UI.ShowScreenSettings(); Canvas.ForceUpdateCanvases(); DialogReadingAnchor.Show((RectTransform)game.UI.GetComponentsInChildren<Toggle>().Single(t => t.name == "settings-overlay-map").transform); yield return Capture("06-settings-en-portrait");
            Require(game.UI.GetComponentsInChildren<Text>().Any(t => t.text == "Show overlay map"), "English toggle label is missing.");
            toggle = game.UI.GetComponentsInChildren<Toggle>().Single(t => t.name == "settings-overlay-map"); toggle.isOn = true; game.UI.CloseCommonPanel(); yield return Capture("07-overlay-portrait"); CheckOverlay();
            game.ApplyAspect("16:9"); yield return new WaitForSecondsRealtime(.4f); game.ReturnTown(); game.Begin(seed: 7421); game.Combat.State.paused = false;
            double start = Time.realtimeSinceStartupAsDouble; int frames = Time.frameCount; yield return new WaitForSecondsRealtime(4);
            Require(game.Combat.State.time > 1 && string.IsNullOrEmpty(game.Combat.State.navigationError), "Generated dungeon did not advance.");
            game.Combat.State.paused = true; yield return Capture("08-generated"); CheckOverlay();
            File.WriteAllText(Path.Combine(output, "runtime.txt"), "PASS: doorway, remembered terrain, hidden props, centered noninteractive overlay, pointer toggle, two languages, two orientations, generated dungeon.\nFrames per second including gameplay: " + (Time.frameCount-frames)/(Time.realtimeSinceStartupAsDouble-start) + "\n");
            game.OverlayMap.Apply(false); game.Save(); Debug.Log("HELLSCRIPT_VISIBILITY_SMOKE_OK"); Application.Quit(0);
        }
    }
}
