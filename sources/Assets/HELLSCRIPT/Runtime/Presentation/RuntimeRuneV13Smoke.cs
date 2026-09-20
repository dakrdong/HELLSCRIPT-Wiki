using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Hellscript.Runes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

namespace Hellscript
{
    // Explicit opt-in development harness; requires an isolated save and user-supplied fixture paths.
    public sealed class RuntimeRuneV13Smoke:MonoBehaviour
    {
        GameController game;string output;int captures;readonly HashSet<string> missing=new HashSet<string>();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)] static void Install()
        {if(Debug.isDebugBuild&&Environment.GetCommandLineArgs().Any(a=>a=="-hellscriptRuneV13Smoke"||a=="-hellscriptRuneDragSmoke")){Application.runInBackground=true;new GameObject("Rune v13 verification").AddComponent<RuntimeRuneV13Smoke>();}}
        static string Arg(string name){var a=Environment.GetCommandLineArgs();int i=Array.IndexOf(a,name);if(i<0||i+1>=a.Length)throw new ArgumentException(name);return a[i+1];}
        static void Require(bool check,string message){if(!check)throw new InvalidOperationException(message);}
        Button Find(string name)=>game.UI.GetComponentsInChildren<Button>().Single(b=>b.name==name);
        void Click(string name){var button=Find(name);Require(button.IsInteractable(),"Disabled "+name);button.onClick.Invoke();}
        RuneBoardGraphic Board=>game.UI.GetComponentsInChildren<RuneBoardGraphic>().Single(b=>b.name=="rune-board");
        void Tap(HexCell cell)=>Board.OnPointerClick(new PointerEventData(EventSystem.current){position=Board.ScreenCell(cell)});
        IEnumerator Resize(int width,int height)
        {
            Screen.SetResolution(width,height,FullScreenMode.Windowed);float until=Time.realtimeSinceStartup+8;while((Screen.width!=width||Screen.height!=height)&&Time.realtimeSinceStartup<until)yield return null;
            Require(Screen.width==width&&Screen.height==height,"Resolution failed");yield return null;Canvas.ForceUpdateCanvases();yield return new WaitForSecondsRealtime(.25f);
        }
        IEnumerator Capture(string name,int width,int height)
        {
            yield return Resize(width,height);
            Require(Board.editor.Board.Cells.Count==259,"Wrong board geometry");foreach(var key in Loc.Missing)missing.Add(key);
            ScreenCapture.CaptureScreenshot(Path.Combine(output,(++captures).ToString("00")+"-"+name+".png"));yield return new WaitForSecondsRealtime(.2f);
        }
        RuneBoardGraphic DragVisual=>game.UI.GetComponentsInChildren<RuneBoardGraphic>().SingleOrDefault(b=>b.name=="Rune drag visual");
        static Vector2 ScreenCenter(RectTransform rect)=>RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(rect.rect.center));
        PointerEventData BeginBoardDrag(RunePlacement placement,out Vector2 grabOffset,out HexCell grabbedCell)
        {
            grabbedCell=placement.OccupiedCells.Last()-placement.Anchor;grabOffset=new Vector2(3,-2);
            var press=Board.ScreenCell(placement.OccupiedCells.Last())+grabOffset;
            var e=new PointerEventData(EventSystem.current){pointerId=-1,button=PointerEventData.InputButton.Left,pressPosition=press,position=press};
            Board.OnPointerDown(e);Board.OnInitializePotentialDrag(e);Require(!e.useDragThreshold,"Mouse pickup should be immediate");
            e.position+=Vector2.right*.5f;Board.OnBeginDrag(e);e.dragging=true;
            Require(Board.Dragging,"Fast mouse pickup panned the board instead of lifting the piece");return e;
        }
        IEnumerator VerifyInputModuleDrag()
        {
            yield return Resize(1280,720);
            var placed=Board.editor.DraftPlacements.First();int count=Board.editor.DraftPlacements.Count;
            var start=Board.ScreenCell(placed.OccupiedCells.Last());var end=ScreenCenter(Board.storageDropTarget);
            string before=JsonUtility.ToJson(game.Store.Data.runes);var mouse=InputSystem.AddDevice<Mouse>("Rune verification mouse");
            try
            {
                InputSystem.QueueStateEvent(mouse,new MouseState{position=start});yield return null;
                InputSystem.QueueStateEvent(mouse,new MouseState{position=start}.WithButton(MouseButton.Left));yield return null;
                foreach(float amount in new[]{.02f,.3f,.7f,1f})
                {
                    var point=Vector2.Lerp(start,end,amount);InputSystem.QueueStateEvent(mouse,new MouseState{position=point}.WithButton(MouseButton.Left));yield return null;
                    Require(Board.Dragging,"Input module did not route a held mouse to rune dragging");
                    Require(Vector2.Distance(DragVisual.ScreenCell(placed.OccupiedCells.Last()-placed.Anchor),point)<1,"Input module drag lost pointer alignment");
                }
                InputSystem.QueueStateEvent(mouse,new MouseState{position=end});yield return null;
                Require(DragVisual==null&&!Board.editor.IsInDraft(placed.InstanceId)&&Board.editor.DraftPlacements.Count==count-1,"Input module did not route release to storage recovery");
                Require(before==JsonUtility.ToJson(game.Store.Data.runes),"Input module release saved the draft prematurely");
                yield return Capture("input-module-recovered",1280,720);Click("rune-undo");Require(Board.editor.IsInDraft(placed.InstanceId),"Input module recovery did not preserve undo");
            }
            finally{InputSystem.RemoveDevice(mouse);}
        }
        IEnumerator VerifyDragging()
        {
            game.Store.Data.runes=JsonUtility.FromJson<RuneGrowthState>(File.ReadAllText(Arg("-hellscriptRuneFixture")));RuneGrowth.Normalize(game.Store.Data);game.Store.Save();
            game.UI.ShowRunes();Click("rune-weapon-sword");
            foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(720,1280)})
            {
                yield return Resize(size.x,size.y);var board=Board;var placement=board.editor.DraftPlacements.First(p=>p.Piece.Shape.Size>1);
                string id=placement.InstanceId,before=JsonUtility.ToJson(game.Store.Data.runes);int owned=game.Store.Data.runes.owned.Count,count=board.editor.DraftPlacements.Count;var pan=board.pan;
                var e=BeginBoardDrag(placement,out var offset,out var cell);e.position+=new Vector2(17,9);board.OnDrag(e);Canvas.ForceUpdateCanvases();
                Require(Vector2.Distance(DragVisual.ScreenCell(cell),e.position-offset)<1,"Grabbed point no longer follows the mouse");
                Require(board.pan==pan,"Dragging a placed rune moved the board");
                e.position=ScreenCenter(board.storageDropTarget);board.OnDrag(e);yield return null;Canvas.ForceUpdateCanvases();
                var ghost=DragVisual;Require(ghost!=null&&!ghost.canvasRenderer.cull,"Rune disappears over storage");
                Require(ghost.transform.parent==board.canvas.rootCanvas.transform&&ghost.transform.GetSiblingIndex()==ghost.transform.parent.childCount-1,"Rune is behind the storage panel");
                Require(Vector2.Distance(ghost.ScreenCell(cell),e.position-offset)<1,"Rune stopped following outside the board");
                var hits=new List<RaycastResult>();EventSystem.current.RaycastAll(e,hits);Require(hits.Count>0&&hits.All(h=>h.gameObject!=ghost.gameObject),"Drag visual intercepted the drop target");
                yield return Capture("drag-over-storage-"+(size.x>size.y?"landscape":"portrait"),size.x,size.y);
                board.OnEndDrag(e);Require(!board.editor.IsInDraft(id)&&board.editor.DraftPlacements.Count==count-1,"Storage drop did not recover the placed rune");
                Require(DragVisual==null&&game.Store.Data.runes.owned.Count==owned,"Drop left a ghost or changed ownership");
                Require(before==JsonUtility.ToJson(game.Store.Data.runes),"Storage drop leaked into the account before Save");
                Click("rune-undo");Require(Board.editor.IsInDraft(id),"Storage recovery cannot be undone");
                placement=Board.editor.DraftPlacements.Single(p=>p.InstanceId==id);e=BeginBoardDrag(placement,out _,out _);e.position=ScreenCenter(Board.storageDropTarget);Board.OnDrag(e);Board.OnEndDrag(e);Click("rune-save");
                var disk=new GameStore(Arg("-hellscriptSavePath"),game.catalog);Require(disk.Data.runes.placements.All(p=>p.runeId!=id)&&disk.Data.runes.owned.Count==owned,"Recovered rune did not survive disk reload");
                yield return null;Canvas.ForceUpdateCanvases();
                var card=Find("rune-card-"+id);var drag=card.GetComponent<RuneStorageDrag>();var start=ScreenCenter((RectTransform)card.transform);
                e=new PointerEventData(EventSystem.current){pointerId=-1,pressPosition=start,position=start};drag.OnPointerDown(e);drag.OnInitializePotentialDrag(e);Require(!e.useDragThreshold,"Storage mouse drag is delayed");drag.OnBeginDrag(e);e.dragging=true;
                Require(DragVisual!=null&&Vector2.Distance(DragVisual.ScreenCell(new HexCell(0,0)),e.position)<1,"Stored rune is not attached to the mouse");
                drag.OnEndDrag(e);Require(DragVisual==null&&!Board.editor.IsInDraft(id),"Dropping a stored rune back into storage changed its state");
                drag.OnBeginDrag(e);var anchor=Board.editor.ValidPlacementAnchors(id,drag.rotation).First(c=>Board.Contains(Board.ScreenCell(c)));
                e.position=Board.ScreenCell(anchor);drag.OnDrag(e);Require(Board.landingValid,"Storage-to-board preview is invalid");drag.OnEndDrag(e);Require(Board.editor.IsInDraft(id),"Storage-to-board drop failed");Click("rune-save");
                placement=Board.editor.DraftPlacements.Single(p=>p.InstanceId==id);var savedAnchor=placement.Anchor;
                e=BeginBoardDrag(placement,out _,out _);e.position=new Vector2(-40,-40);Board.OnDrag(e);Board.OnEndDrag(e);
                Require(DragVisual==null&&Board.editor.DraftPlacements.Single(p=>p.InstanceId==id).Anchor.Equals(savedAnchor),"Invalid drop changed the original placement");
                e=BeginBoardDrag(placement,out _,out _);Board.OnCancel(e);e.position=ScreenCenter(Board.storageDropTarget);Board.OnDrag(e);Board.OnEndDrag(e);
                Require(DragVisual==null&&Board.editor.IsInDraft(id),"Cancelled drag recovered a rune");
                e=BeginBoardDrag(placement,out _,out _);Click("rune-weapon-bow");Require(DragVisual==null,"Changing weapon left the drag visual on screen");Click("rune-weapon-sword");
            }
            yield return VerifyInputModuleDrag();
            File.WriteAllText(Path.Combine(output,"result.txt"),"PASS: landscape and portrait; immediate mouse pickup; exact grabbed-point tracking; unclipped topmost visual over storage; transparent raycasts; storage recovery; draft-only change; undo; save/disk reload without ownership loss; storage pickup and return; storage-to-board placement; invalid drop; cancellation; weapon-switch cleanup. Also queued mouse press/held movement/release through the actual InputSystem UI input module and verified recovery and undo. Native automated input workflow, not physical mobile input.\n");
            Application.Quit(0);
        }
        IEnumerator Start()
        {
            output=Arg("-hellscriptScreenshots");Arg("-hellscriptSavePath");Directory.CreateDirectory(output);
            Application.logMessageReceived+=(m,s,t)=>{if(t==LogType.Exception){File.WriteAllText(Path.Combine(output,"failure.txt"),m+"\n"+s);Application.Quit(1);}};
            yield return new WaitForSecondsRealtime(1);game=FindAnyObjectByType<GameController>();Require(game?.Store!=null,"Store unavailable");game.enabled=false;game.ApplyLanguage("ko");game.ApplyInterfaceScale(100);
            if(Environment.GetCommandLineArgs().Contains("-hellscriptRuneDragSmoke")){yield return VerifyDragging();yield break;}
            game.UI.ShowRunes();yield return Capture("fresh-portrait-ko",720,1280);
            Click("rune-weapon-sword");yield return null;
            string id=game.Store.Data.runes.owned[0].id;Click("rune-card-"+id);Tap(new HexCell(1,0));Require(Board.editor.DraftPlacements.Count==1,"Tap placement failed");Click("rune-save");yield return null;
            Require(game.Store.Data.runes.placements.Single().runeId==id,"Save failed");
            Tap(new HexCell(1,0));Click("rune-remove");Click("rune-weapon-bow");yield return null;
            Click("rune-card-"+id);Tap(new HexCell(1,0));Click("rune-save");Require(game.Store.Data.runes.placements.Single().weapon=="bow","Cross-weapon transfer failed");
            Click("닫기");yield return null;
            RuneMasteryProgress.AddExperience(RuneMasteryProgress.Get(game.Store.Data.runes,"sword"),100);game.Store.Save();game.UI.ShowRunes();Click("rune-weapon-sword");
            Tap(new HexCell(3,0));Click("rune-unlock");Require(RuneMasteryProgress.Get(game.Store.Data.runes,"sword").unlocked.Count==19,"Slot draft leaked before Save");
            Click("rune-save");Require(RuneMasteryProgress.Get(game.Store.Data.runes,"sword").unlocked.Count==20,"Slot unlock failed");Click("닫기");yield return null;
            game.Store.Data.runes=JsonUtility.FromJson<RuneGrowthState>(File.ReadAllText(Arg("-hellscriptRuneFixture")));RuneGrowth.Normalize(game.Store.Data);game.Store.Save();
            game.UI.ShowRunes();Click("rune-weapon-sword");yield return Capture("reference-landscape-ko",1280,720);
            Click("rune-presets");
            for(int i=0;i<5;i++){Click("rune-preset-save-"+i);yield return null;Click("rune-preset-confirm");yield return null;}
            Require(game.Store.Data.runes.presets.All(p=>!p.occupied),"Preset draft leaked before Save");Click("rune-dialog-close");Click("rune-save");yield return null;
            var reopened=new GameStore(Arg("-hellscriptSavePath"),game.catalog);Require(reopened.Data.runes.presets.All(p=>p.occupied&&p.placements.Count==5),"Global presets failed disk reload");
            Tap(new HexCell(1,0));Click("rune-remove");Require(Board.editor.DraftPlacements.Count==4,"Retrieve sample failed");Click("rune-undo");Require(Board.editor.DraftPlacements.Count==5,"Undo did not restore sample");
            Tap(new HexCell(1,0));Click("rune-remove");Click("rune-presets");Click("rune-preset-load-0");Require(Board.editor.DraftPlacements.Count==5,"Preset load did not restore all placements");
            Click("rune-presets");yield return Capture("global-presets-ko",1280,720);Click("rune-dialog-close");
            yield return Capture("reference-portrait-ko",720,1280);
            Click("rune-weapon-sword");Click("rune-region-1");yield return Capture("region-preview-ko",1280,720);
            Click("rune-region-lock-1");yield return Capture("region-dialog-ko",1280,720);Click("rune-dialog-close");
            Click("rune-region-0");Click("rune-overview");yield return Capture("full-map-ko",1280,720);
            Click("rune-fit");game.ApplyLanguage("en");yield return Capture("reference-landscape-en",1280,720);yield return Capture("reference-portrait-en",720,1280);
            game.ApplyLanguage("ko");game.ApplyInterfaceScale(140);yield return Capture("large-type-portrait-ko",720,1280);game.ApplyInterfaceScale(100);
            yield return Resize(1280,720);Click("rune-effects-type-0");yield return Capture("attack-effects-ko",1280,720);Click("rune-dialog-close");
            Click("rune-codex");Click("rune-codex-weapons");Click("지팡이");
            var search=game.UI.GetComponentsInChildren<InputField>().Single();search.text="연쇄";yield return Capture("codex-search-ko",1280,720);
            Click("rune-codex-regions");Click("기교");yield return Capture("codex-region-ko",1280,720);Click("rune-dialog-close");
            yield return Resize(720,1280);Click("rune-codex");yield return Capture("codex-portrait-ko",720,1280);Click("rune-dialog-close");
            File.WriteAllText(Path.Combine(output,"result.txt"),"PASS: exact 259-cell boards; tap placement, account save, cross-weapon recovery, mastery slot unlock draft/save, five global preset drafts/save/disk reload/load, undo; supplied v13 sample; landscape/portrait, map, region preview, per-color effects, codex weapon/region selection/search and English.\nMissing translations: "+string.Join(" | ",missing));
            Require(missing.Count==0,"Missing translations: "+string.Join(" | ",missing));Application.Quit(0);
        }
    }
}
