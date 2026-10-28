
> **2026-10-28 A2 correction:** References to the large edict window, ≡ menu and their dedicated components or smokes are historical. Use the [current entry points](../../../../Assets/HELLSCRIPT/Runtime/Presentation/GameUI.HuntEdict.cs) and [replacement screens and source owners](../../../Implementation/Hunt_Edict_Window_Removal_20261028.en.md). Earlier captures and test counts describe the earlier implementation.

Read-only pass done; nothing was changed. No `SkillTreeD4*` code exists yet. Paths are abbreviated:

- `R` = `/Users/t8g-2410-pn-005/Documents/MyProject/HELLSCRIPT/wt-d4-skill-plan`
- `P` = `R/Assets/HELLSCRIPT/Runtime/Presentation`
- `T` = `R/Assets/HELLSCRIPT/Tests/Editor`
- `E` = `R/Assets/HELLSCRIPT/Editor`
- `D` = `R/Docs/Design/D4SkillTree`

The worktree already has someone else's uncommitted edits: `D/extract.js` and `D/skill_tree_d4.json` are modified, and `D/golden/` is untracked (layout, viewmodel, anatomy and interaction JSON, `styles.json`, `fx.json`).

## 1. DRAWING PRIMITIVES

**API (Graphic local space, y up, unless noted)**

- **`UiMesh`** (`P/UiButtonFace.cs:156-256`) is a static class writing into a `VertexHelper`, so it needs a `MaskableGraphic` host.
  - `Inset(Rect,n)`, `Box(m,Rect,upper,lower)`, `Frame(m,Rect,n,c)`
  - `RoundedBox(m,Rect,radius,upper,lower)`, `RoundedRing(m,Rect,radius,width,ink)`
  - `Gradient(m,Rect,cut,Func<float,Color>,bands=6)` fills a chamfered rect with colour = f(y).
  - `Ring(m,Rect,cut,width,c)` is a chamfered outline.
  - `Diamond(m,Vector2,radius,c)`, `Brackets(m,Rect,n,arm,c)`
  - Rounded shapes use a fixed 24-point contour (`RoundedContour` :174) with no AA. `RoundedContour`, `Gradient` and `Brackets` allocate arrays or Lists on every call.
- **`UiButtonFace : MaskableGraphic`** (:10) is added by `UiButton.Configure(role,chosen,locked)` (`P/UiButton.cs:28-42`).
  - `Show(role,chosen,locked,available,hovered,pressed,focused,instant)` (:27) is a no-op when nothing changed. `SetChrome(Gilt|Minimal)`.
  - It draws a chamfered gradient face, hairline ring, top light, rising glow, and for Primary a double line plus lozenges.
  - The Tab role is a gradient strip, bottom bar, gold bar and diamond (:84-98). Minimal chrome uses `RoundedBox/RoundedRing` with radius <= 5 (:127-142).
  - The fade is `Time.unscaledDeltaTime/UiTheme.ButtonFade` (.085 s) (:43-49).
- **`UiTheme`** (`P/UiTheme.cs:8-130`):
  - Hex consts at :12-21 (Void, Background, Inset, Panel, Raised, Hover, Selected, Border, Edge, GoldDeep, Gold, GoldBright, Text, Muted, Faint, Success, Danger, Primary). Matching `Color` properties come from `StorageSurface.Hex`.
  - `Tint(hex,alpha)` :44, `Title/Heading/Body/Caption = 20/15/12/10` :50, `Scale(Rect)` :54.
  - `ButtonPalette(role,chosen,locked,available,hover,pressed)` returns `UiButtonPalette{top,bottom,edge,light,accent,glow}` (:80; struct at `P/UiButtonFace.cs:149`). `MinimalButtonPalette` :115. `Button/Choice/Skin/Backdrop`.
  - There are no steel, ember, line-state or rim-gradient tokens.
- **`UiLayout`** (:148-163):
  - `Rect(name,parent)`.
  - `Place(r,x,y,w,h)`: top-left anchor and pivot, y down, size >= 1.
  - `Stretch(r)`.
  - `Text(parent,name,value,x,y,w,h,int size,Color,Font font=null)`.
  - `Clear(parent)`: SetActive(false) plus Destroy.
- **`UiWindowFrame : Image`** (`P/UiWindowFrame.cs:9`; fields `header, footer, corners`) draws a gradient panel, header band, border and gilt `Brackets` (no dots). `Divider(m,x0,x1,y,n,c,strength,jewel,hole)` (:39) fades its ends and has a centre diamond. `UiHeaderRule.Attach(Transform header)` :61.
- **`UiIconButton`** (`P/UiIconButton.cs:11`) has `Create(parent,name,icon,x,y,w,h,Action click,framed=true,Color? tint=null,bool backdrop=false,string folder=null,Rect? uv=null)` (:15) and `CreateGlyph(...)` (:23). Both return a `UiButton`.
- **`SkillIconView`** (`P/SkillIconView.cs:10`):
  - `Create(parent,passive)` :18, `SetKind`, `SetSkill(id,squareFrame=false)` :48, `SetSprite`.
  - Children are Plate (an `Image` with a stencil `Mask`), Icon, Cover (Radial360 cooldown) and Frame.
  - `SkillIconAssets.TryResolve(id,out passive,out sprite)` (:57) only knows ids that `ClassSkills.Find` returns, plus legacy atlas ids.
- **`SkillTreeGraphic`** (`P/SkillTreeGraphic.cs:25`):
  - Builder API: `Create(parent,name,x,y,w,h)` :32, `Bind(UiButton)`, `Line(a,b,width,c,hot)`, `Disc`, `Ring`, `Glow(center,inner,outer,c)` (radial halo), `Diamond`, `DiamondRing`, `Box(x,y,w,h,top,bottom)`.
  - `Link(a,b,crossing,width,c)` is orthogonal, three segments only.
  - Coordinates are top-left px, and the whole shape list is re-emitted on every dirty.
  - A private `Polygon(...,sides,start)` (:98) exists. `SkillTreeLayout` :13 is the legacy 37-node placement.
- **`HuntEdictSurface : Image`** (`P/HuntEdictSurface.cs:9`): `Paint(skin)` :16 (named skins :20-62); public `fill, edge, radius, edgeWidth, underline`. It draws a rounded fill, ring and 1 px AA feather (:80-84). `radius=100` gives a pill.
- **`StorageSurface : Image`** (`P/StorageSurface.cs:8`): `top,bottom,edge,bottomOnly,sidesOnly,dashed,inset`, `Paint(upper,lower,border)`, `Hex(string)`. Square corners and a 1 px edge. Dashes are a fixed 3/3 px on straight edges (:37-46).
- **Others:**
  - `StorageGlyph`: 24-unit line icons by `symbol`.
  - `UIRectBorder` (BaseMeshEffect), `UiLockGraphic`.
  - `AttendanceClaimGlow`: static radial glow plus 6 rays.
  - `TutorialAnchorRing`.
  - `EquipmentRevealGraphic`: Frame = L corners plus diamond dots (:16-29).
  - `TitleFrameGraphic/TitleOrnament`.
  - `RuneGemMesh` (`mainTexture` override :12, rune atlas): public `Hex/Circle/Quad/Line/Outline(points,width,color,dashed)/Polygon(points,light,dark)`.
  - `TownCircleGraphic` (`P/TownJoystick.cs:52`), `ForgeMilestoneRing`.
  - `SelectionRune` and `SelectionGroundShadow` (`P/CharacterSelectionStage.cs:95,106`).
  - `VignetteGraphic` (`P/TutorialCinematic.cs:181`).
  - `TrainingDpsChart`: polyline plus private dashed/dot helpers (:49,:54).
  - `DialogueReveal : BaseMeshEffect` (`P/StoryDialogueWindow.cs:243`).

**Feature support**

| Feature | Closest existing | Verdict |
|---|---|---|
| Vertical gradient fill | `UiMesh.Box/Gradient/RoundedBox`, `StorageSurface.top/bottom` | yes |
| Gradient (gilt) border | every ring is single colour; `Divider` fades alpha only | no |
| Rounded rect, per-widget radius | `RoundedBox/RoundedRing(radius)`, `HuntEdictSurface.radius` | yes (one radius for all corners; AA only in HuntEdictSurface) |
| Dashed outline | `StorageSurface.dashed` | partly (straight edges, 1 px, fixed 3/3) |
| Dashed line | `RuneGemMesh.Outline(dashed)` (4.5/7.3), `TrainingDpsChart.Dashed` (private) | partly |
| Hexagon medallion | `SkillTreeGraphic.Polygon` and `RuneBoardGraphic.Hex` are private; `RuneGemMesh.Hex` is atlas-bound | no (no usable public one) |
| Segmented arc / conic ring | none | no |
| Soft outer glow / box-shadow | `SkillTreeGraphic.Glow` (circular), `AttendanceClaimGlow`, 1 px feather | partly |
| Inner shadow | `StorageSurface.inset` (hard 1 px), `UiButtonFace` top light | partly |
| Blurred backdrop art | no blur code or shader | no |
| Polyline with glow under-stroke | `Line/Link`; the two-pass wide-translucent plus narrow stroke is at `P/HuntEdictWindow.Skills.cs:52` | partly (orthogonal only, no joins or caps) |
| Corner ornaments (L plus dots) | `UiMesh.Brackets`, `EquipmentRevealGraphic.Frame` | partly |
| Gradient text / text shadow | Unity `Shadow`/`Outline` | no / yes (hard offset) |
| Circle / square crops | `SkillIconView` | yes (fixed radius, hard stencil edge) |
| Scroll plate and panel body | see section 3 | yes, single axis only |
| Tab buttons | `UiButtonRole.Tab` | partly (gold, not ember) |
| One-shot FX | see section 5 | partly |

**GAPS and the idiomatic way to fill each**

1. **Gradient rims** (`GILT_RIM` and `STEEL_RIM` are 5-stop vertical gradients): add a `RoundedRing(..., Func<float,Color> byY)` overload to `UiMesh`, mirroring `Gradient`. Put the stop arrays in `UiTheme`, which is exempt from the colour lint.
2. **Hex, conic arc, angular gradient**:
   - The design's HEX is a regular pointy-top hexagon, so `Polygon(…,6,90)` already draws it.
   - Add `UiMesh.Hex`/`HexRing` and `UiMesh.Arc(m,center,rIn,rOut,startDeg,sweepDeg,Func<float,Color> byAngle,segments)`.
   - Expose them as new `Kind`s in `SkillTreeGraphic` (builder style).
3. **Polyline with joins, round caps, dash, glow under-stroke, partial range** (ignite and glint): add a new `PolylineGraphic : MaskableGraphic` per state colour, as plan 9.7 says. Give it `SetRange(t0,t1)` and preallocated buffers; reuse the dash maths of `RuneGemMesh.Outline`.
4. **Rounded dashed outline**: add `UiMesh.DashedRing` walking the `RoundedContour` perimeter.
5. **Box-shadow, inner shadow, radial plate gradients**:
   - Add `UiMesh.Glow/Inner(m,Rect,radius,spread,Color)`: a ring from the inflated contour to the rect with an alpha ramp, generalising the feather at `P/HuntEdictSurface.cs:80-84`.
   - For radial fills, use ellipse fans like `SelectionGroundShadow`.
   - `RectMask2D` clips glows at the plate edge.
6. **Blur and CSS filters** (blur 14-18 px art, grayscale/brightness on locked icons):
   - Add one UI shader under `Resources/`, modelled on `Resources/GlobalHudFade.shader`. It has Stencil and `_ClipRect`, so it works under `Mask`/`RectMask2D`.
   - Fold in `_Tone` (brightness, saturation) from `Resources/Art/PuzzleHubTone.shader`. That shader has no stencil or clip, so it must not be used inside a scroll plate.
   - Own the `new Material(shader)` per window and destroy it (`P/PuzzleHubWindow.cs:37,223`).
   - Icons are 1254², `isReadable:0`, no mips (`W01.png.meta`), so CPU or baked blur is out, and plan §16 forbids resizing icons.
7. **Gradient text**: add a `TextGradient : BaseMeshEffect`. `DialogueReveal.Conceal` (`P/StoryDialogueWindow.cs:243-261`) already rewrites per-glyph vertex colours. Add it before `Shadow` so the shadow copies stay black. Soft text glow is only possible by approximation (`Outline` or several `Shadow`s).
8. **Dotted corner ornaments and centre diamond rule**: compose `Brackets` + `Diamond` + a new `UiMesh.Disc`. Draw them on a layer above the scroll mask, because the design draws them outside the clip.
9. **Icon crops with exact radius and dim/desaturate**:
   - `mask-square` has a fixed radius of about 13%, decoded from its alpha.
   - Add a textured rounded-rect `MaskableGraphic` with UVs, like the `RuneGemMesh.mainTexture` pattern. This avoids a stencil `Mask` per icon.
10. **FX**: build them from items 2 and 3 plus `CanvasGroup`/`canvasRenderer.SetAlpha`, `localScale`, a skewed quad inside `RectMask2D` for the shine, and a small pool of `Text` for "+1".

## 2. CUSTOM-UNIT ADAPTER WINDOWS

- **Unit.** `BlacksmithWindow.Reflow` (`P/BlacksmithWindow.cs:61-72`):
  - `safe=UiSafeArea.Current`, `landscape=safe.width>safe.height`, `scale=UiTheme.Scale(safe)`. That formula is min(w/800,h/450) landscape and min(w/405,h/720) portrait (`P/UiTheme.cs:54`).
  - Canvas units are `width=safe.width/scale` and `height=safe.height/scale`.
  - :67 sets `U=Clamp(landscape?Min(width/75.8f,height/35.7f):Min(width/31.7f,height/64.5f),8,17)`. Those constants are the HTML's em extents.
  - `E(em)=em*U` and `Z(em)=Max(7,Round(em*U))` (`P/BlacksmithWindow.Chrome.cs:14,16`).
  - `RuneBoardWindow.Render` uses the same constants on `shell.Frame.rect` (`P/RuneBoardWindow.cs:62-63`).
  - Neither fits a fixed design rect or centres margins. The plan §9.2 fit-and-centre 1:1 unit is new code.
- **Root and canvas.**
  - Blacksmith creates `new GameObject("Blacksmith",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster))` under the GameUI transform, `ScreenSpaceOverlay`, `sortingOrder=320` (:30-31). Storage is 320, HuntEdict 310, RuneMaster 330.
  - The scaler is `ConstantPixelSize` with `scaleFactor=scale` (:68).
  - Reflow does `Clear(canvasRoot)` (destroys all children), adds a backdrop, then a `frame` anchored bottom-left at `safe.position/scale` with `RectMask2D` (:69-71).
  - On the host stack the host rewrites sorting to `400+i*10` and restores it on removal (`P/ContentWindowHost.cs:87,62`).
  - RuneBoard owns no canvas. `ContentWindowView.Open` (`P/ContentWindowView.cs:23-29`) builds it, and `Repaint` sets scaler and `Canvas.scaleFactor` (:38-40). `Render` hides the shell title, close, frame and scroll (`P/RuneBoardWindow.cs:64`).
- **Reflow** is polling, not events: `if(safe!=UiSafeArea.Current||language!=Loc.Language||Mathf.Abs(textScale-readingScale())>.001f)Reflow();` (`P/BlacksmithWindow.cs:40`; same in `P/StorageWindow.cs:72-73`, `P/HuntEdictWindow.cs:56`). Shell windows do it in `ContentWindowView.Update` (:30). `GameUI.ApplyLanguage` also repaints a fixed list of windows (`P/GameUI.cs:109`). `readingScale` is always 1 (`P/GameUI.cs:71`).
- **Attach.** `ContentWindowHost.Attach(Component owner,Action back,bool pause=true,bool blocksGameplay=true)` (:28) silently returns if the window is not under GameUI (:30). `StoreViewBinding.Attach(Component owner,GameStore store,Action refresh,Func<bool> ready=null,Func<string> visibleState=null)` is at `P/StoreViewBinding.cs:13`. Full real example, `P/StorageWindow.cs:67` and `:111-114`:
```
ContentWindowHost.Attach(view,view.Escape);StoreViewBinding.Attach(view,store,view.Repaint,()=>view.modal==null&&!view.Dragging,view.DisplayKey);
string DisplayKey()=>string.Join("|",StoreViewBinding.EquipmentKey(Account),Account.gold,Account.premium,Account.materials,
  Account.gemCapacity,StoreViewBinding.Key(Account.warehouseCapacity),...,StoreViewBinding.Key(Account.contentUnlocks),badge?.Invoke());
```
  The Blacksmith call has no key (`P/BlacksmithWindow.cs:33`) and is grandfathered at `R/tools/check_ui_refresh.py:60-63`.
- **Back, pause, input.**
  - `Push` adds a `CanvasGroup`, takes a ref-counted pause lease on the active `RunState`, and disables interaction on every other canvas under the host. Only the top window is interactable (`P/ContentWindowHost.cs:33-89`).
  - Escape calls the top entry's `back` (:92-96). `ContentWindowHost.BackLocked` can veto (:22; set at `P/GameUI.TutorialStaging.cs:35`). Close with `Detach(this)`.
  - Inside the window, repaints are deferred while input is busy. `StoreViewBinding.InputBusy` (:43-50) covers press, `UiButton.Pressed`, a focused InputField and an open dropdown. RuneBoard defers repaint to `LateUpdate` while dragging (`P/RuneBoardWindow.cs:39-43`).
- **ADAPTERS entries** (`R/tools/check_ui_contract.py:16-23`):
```
'BlacksmithWindow': ('ContentWindowHost.Attach','UiTheme.Scale','EquipmentSlotView','CharacterEquipmentView','ItemDetailView'),
'HuntEdictWindow': ('ContentWindowHost.Attach','UiTheme.Scale','HuntEdictSummary','Session.Draft'),
'RuneMasterWindow': ('ContentWindowHost.Attach','UiFonts.Body'),
```
  Tokens are searched in comment-stripped source of `Owner.cs` plus `Owner.*.cs` (:51-58).
- **Alternative route.** `RuneBoardWindow` (not in ADAPTERS) uses `ContentWindowView.Open` as its shell, so it needs no canvas exemption. The cost is hiding shell chrome (:64).

## 3. SCROLLING AND INPUT

- **Scroll views.**
  - All are `ScrollRect` + clear `Image` + `RectMask2D` viewport, content anchored top, `Clamped`. Copies are at `P/ContentWindowView.cs:48-53`, `P/BlacksmithWindow.Controls.cs:32-42` (with `Scrollbar`), `P/HuntEdictWindow.cs:265-277`, `P/RuneBoardWindow.cs:101-106`.
  - All set `horizontal=false`, except `HudStatusScroll` (horizontal only, no inertia, `P/StatusStripView.cs:10-17,41`) and the codex tab strip (`P/MonsterCodexWindow.cs:100`).
  - No ScrollRect with both axes exists.
  - Nested scroll is avoided by locking the outer one (`P/MonsterCodexWindow.Detail.cs:14`, `P/HuntEdictWindow.cs:186`).
- **Tap vs drag.**
  - `UiButton` (`P/UiButton.cs:12-97`) has no drag logic. It relies on EventSystem, so a ScrollRect drag cancels the press. `Pressed` (:24) lets `StoreViewBinding` postpone repaints.
  - Custom cells forward gestures to the parent ScrollRect: `StorageCell` (`P/StorageWindow.Drag.cs:13-31`, with `!e.dragging` on click), `RuneMasterCell` (`P/RuneMasterWindow.Drag.cs:7-17`).
  - Direction arbitration: `TrainingChartView` hands off when `|dy|>|dx|` (`P/TrainingChartView.cs:73-76`). `AttendanceSwipeTarget` decides after 10 px with a 1.25 ratio (`P/AttendanceSwipe.cs:32-53`).
  - `HoldPress` separates a tap (< 0.3 s) from a 0.7 s hold gauge (`P/HoldPress.cs:8-39`).
  - `useDragThreshold` is set per control; `pixelDragThreshold` is never changed.
- **Scroll preserved across Repaint** is the owner's job. Save `content.anchoredPosition.y`, rebuild, `Canvas.ForceUpdateCanvases()`, clamp, restore. See `P/ContentWindowView.cs:33,62`, `P/HuntEdictWindow.cs:188-195` (dictionary by key), `P/StorageWindow.cs:440-449`, `P/InventoryWindow.cs:77-79`.
  - Only Y is ever stored. X is never persisted anywhere.
  - `StoreViewBinding` restores focus, not scroll (:25-39).
  - Scroll-into-view: `P/HuntEdictWindow.Skills.cs:298-306`, and the smoke `Tap` (`P/RuntimeSkillTreeSmoke.cs:54-65`).
  - `UiVisibility.Visible/Fraction` (`P/UiVisibility.cs:13-28`) computes the reachable part of a control.

## 4. TEXT

- **`UiLayout.Text`** (`P/UiTheme.cs:156-160`) sets `font??UiFonts.Body`, int `fontSize`, `supportRichText=true`, `raycastTarget=false`, `horizontalOverflow=Wrap` and `verticalOverflow=Truncate`. It leaves the default UpperLeft alignment, no `lineSpacing`, and does not call `Loc.T`.
  - `HuntEdictWindow.Text` also runs `ClassSkillTree.Display` (`P/HuntEdictWindow.cs:70`), which swaps skill codes like W01 for names (`Runtime/Core/ClassSkillTree.cs:120`).
  - Adapters build their own Text, mostly with `supportRichText=false` (`P/BlacksmithWindow.Controls.cs:16-20`).
- **Fonts** (`P/UiTheme.cs:131-147`):
  - `Body` is an OS dynamic font from {Apple SD Gothic Neo, Malgun Gothic, Noto Sans CJK KR, Noto Sans CJK, Droid Sans Fallback, Arial}. WebGL uses the preloaded `ThirdParty/Fonts/NanumGothic-Regular.ttf`.
  - `Display` is {Baskerville, Georgia, Times New Roman, Noto Serif}, and WebGL uses Body.
  - Noto Serif KR and Cinzel are not bundled (plan D6).
  - `check_ui_contract.py:61-62` rejects creating fonts outside `UiTheme.cs`.
- **Shadow and outline** are Unity `Shadow`/`Outline` with hard offsets only: `P/ItemDetailView.Style.cs:140`, `P/GlobalHudView.cs:207`, `P/AttendanceWindow.cs:182`, `P/TitleScreenView.cs:39`, `Outline` at `P/GameUI.Plaza.cs:106`. Rich text is `<color=#hex>` in 6 places (e.g. `P/EquipmentComparisonView.cs:19`); there is no `<size>` or `<b>`.
- **Gradient-filled text** does not exist anywhere.
- **Line height and wrapping.**
  - `Text.lineSpacing` is a multiplier of the font's own line height (set to 1 or 1.12: `P/ItemDetailView.Style.cs:30`, `P/StorageWindow.cs:469`). There is no CSS px line-height.
  - No keep-all control exists. A grep for NBSP, ZWSP and word-joiner characters across `Runtime` finds nothing.
  - Wrapping is only checked via `cachedTextGenerator.lineCount` and `preferredHeight` in smokes (`P/RuntimeJewelerSmoke.cs:57-58`).
  - `resizeTextForBestFit` is the usual overflow fix (`P/BlacksmithWindow.Controls.cs:25`).
- **Measurement helpers** that could replace the HTML's `tw()`/`fitFs()` (`D/SkillWindow.dc.html:486-497`):
  - `Text.preferredWidth/Height` (`P/HuntEdictWindow.cs:111`, `P/HuntEdictWindow.Skills.cs:105`).
  - Throwaway-Text `TextWidth(value,size,bold,serif)` and `TextHeight(value,w,size,bold)` (`P/BlacksmithWindow.Chrome.cs:41-51`).
  - Static, no GameObject: `t.cachedTextGeneratorForLayout.GetPreferredWidth(value,t.GetGenerationSettings(Vector2.zero))/t.pixelsPerUnit` (`P/GameUI.Rift.cs:70-71`).
  - `new TextGenerator().GetPreferredWidth(value,new TextGenerationSettings{font=UiFonts.Body,fontSize=size,fontStyle=FontStyle.Normal,scaleFactor=1,richText=true})` (`P/MonsterCodexWindow.cs:129`).
  - Set `Canvas.scaleFactor` before measuring on a first render (`P/ContentWindowView.cs:39-40`).

## 5. ANIMATION

There is no tween library. Everything is `Update` or a coroutine on `Time.unscaledTime` or `unscaledDeltaTime`, because pause is `RunState.paused`, not `timeScale`.

- **`UiButtonFace`** (`P/UiButtonFace.cs:21,39,43-49`): a state change sets progress to 0, and `Update` lerps the palette and calls `SetVerticesDirty` each step. `Show` early-outs when nothing changed (:30-31), so a repeated Repaint does not restart it.
- **`ForgeWorkingPulse`** (`P/BlacksmithWindow.Slots.cs:245-250`): a CanvasGroup alpha of `.65+.35*cos(unscaledTime*2π/1.5)`, with no mesh rebuild. It reads PlayerPrefs each frame.
- **`TutorialAnchorRing`** (`P/TutorialAnchorRing.cs:17-36`): `Update()=>SetVerticesDirty()` every frame. It has a breathing border, ripple and bobbing pointer, and is still under reduce-motion (:20).
- **`AttendanceClaimGlow`** is a static mesh.
- **`ContentDockView`** (`P/GameUI.ContentDock.cs:73-112`): `Seconds=.22f`, step = `Time.unscaledDeltaTime/Seconds`, with a `Settled` early-out and `Snap(bool)`.
- **Window tickers:**
  - `RuneMasterWindow.TickFusion` (`P/RuneMasterWindow.cs:173-184`): start time, `SmoothStep`, `Lerp`.
  - `RiftRewardRevealMotion` (`P/RiftRewardRevealWindow.cs:42-47`): scale .86 to 1 plus alpha.
  - `DialogueEntrance` (`P/StoryDialogueWindow.cs:44-51`).
  - Blacksmith's 0.25 s ticker (`P/BlacksmithWindow.cs:43-49`).
- **Cheap alpha:** `canvasRenderer.SetAlpha` (`P/UiButton.cs:63,68`) and CanvasGroup do not rebuild meshes.
- **Reduce motion:** `WorldLighting.ReduceMotionKey="hellscript.reduce-motion"` (`P/WorldLighting.cs:46`) is a PlayerPrefs int. `StoryDialogueWindow.ReduceMotion` is that flag OR `SystemReduceMotion` (`P/StoryDialogueWindow.cs:55`).
  - The OS flag is read only on macOS via objc, cached 1 s (:57-70), and is `false` elsewhere (:72).
  - No settings UI writes the key (comment `P/WorldLighting.cs:44-45`), and `ForgeWorkingPulse` ignores the OS flag.
- **Pooled floating numbers:** no UI one exists. The nearest pools are the `StatusStripView` entries (`P/StatusStripView.cs:21,75-77`), `PrologueGate` `DimPool=16` (`P/PrologueGate.cs:16,31`), and the capped, oldest-recycled 3D `WorldFx` transients (`P/WorldFx.cs:13-31`).

## 6. SMOKE AND SCREENSHOT FRAMEWORK

- **Structure.**
  - A smoke is a `MonoBehaviour` in `P/Runtime*Smoke*.cs`, wrapped in `#if UNITY_EDITOR || DEVELOPMENT_BUILD`.
  - A `[RuntimeInitializeOnLoadMethod(AfterSceneLoad)] static void Boot()` returns unless `Debug.isDebugBuild` and the flag is in `GetCommandLineArgs()`. It sets `Application.runInBackground=true` and creates the GameObject (`P/RuntimeSkillTreeSmoke.cs:42-47`, `P/RuntimeBattleQuickEditSmoke.cs:210-217`).
  - `P/RuntimeBlacksmithSmoke.cs:20-26` also sets `InputSystem.settings.backgroundBehavior=IgnoreFocus`.
  - `Start()` is a coroutine. It reads `-hellscriptSavePath` (required) and `-hellscriptScreenshots`, `-hellscriptEvidence` or `-hellscriptEvidencePath` (`P/RuntimeSkillTreeSmoke.cs:106-109`; the three names vary per smoke). It then waits 1 s and calls `FindAnyObjectByType<GameController>()`.
  - Failures: an exception writes `failure.txt` and calls `Quit(1)` (:48-51). Success writes `runtime.txt` or an `..._OK` marker such as `HELLSCRIPT_BATTLEQUICK_SMOKE_OK` (`P/RuntimeBattleQuickEditSmoke.cs:343-344`).
  - Files starting `Runtime` and containing `Smoke` are skipped by both UI check tools (`check_ui_contract.py:52`, `check_ui_refresh.py:51`) and by `LocalizationTests` (`T/LocalizationTests.cs:259`).
- **Helpers** (copied per smoke, not shared):
  - `Require(bool,string)`.
  - `Find(name)` (:52).
  - `Bounds(RectTransform)`.
  - `Tap(name)` scrolls the button's ScrollRect to centre, then calls `TapAt` (:54-65).
  - `TapAt(Vector2,name)` calls `EventSystem.current.RaycastAll`, requires the first hit to be the button, then runs `ExecuteEvents.Execute` for pointerDown, pointerUp and pointerClick (:71-81).
  - `Hold(target,seconds)` (BattleQuick :29-33).
  - Waits: `yield return null` twice, or `WaitForSecondsRealtime(.25f)` plus `Canvas.ForceUpdateCanvases()`.
  - `Resize(w,h,lang)` (:82-87): `game.ApplyLanguage(lang); game.UI.RefreshCanvasLayout(); Screen.SetResolution(w,h,Windowed)`, poll up to 8 s, then 0.4 s.
  - `Capture(name)` (:88-89): `Debug.developerConsoleVisible=false; Canvas.ForceUpdateCanvases(); WaitForEndOfFrame; ScreenCapture.CaptureScreenshotAsTexture()`, then `EncodeToPNG` and `File.WriteAllBytes`. RuneBoard uses `CaptureScreenshot(path)` (`P/RuntimeRuneBoardSmoke.cs:40`).
  - Real mouse device: `InputSystem.AddDevice<Mouse>` (`P/RuntimeRuneBoardSmoke.cs:94`).
  - Notch: `UiSafeArea.Simulate(left,bottom,right,top)` (debug builds only, `P/UiSafeArea.cs:59-61`) and `StopSimulating()`. The plan's iPhone insets are already used at `P/RuntimeScreenSettingsSmoke.RiftTitle.cs:69` (956 wide: 62/21/62/0; 440 wide: 0/34/0/62).
  - `UiSafeArea.AspectRatio=0` resets the aspect frame (`P/RuntimeRuneBoardSmoke.cs:45`).
  - Resume: a second launch with the same save path and `-hellscriptSkillTreeResume` asserts persisted state, writes `restart.txt` and quits (:118-125).
- **Exact commands** (clone with `cp -Rc Assets Packages ProjectSettings Library <work>/proj; rm -f <work>/proj/Temp/UnityLockfile`; Unity 6000.6.0f1). `-nographics` and `-logFile` are from `CPU_Optimization_20261002.md:120`; the Unity binary path is from `R/tools/hunyuan_monster_install.py:31`.
```
/Applications/Unity/Hub/Editor/6000.6.0f1/Unity.app/Contents/MacOS/Unity -batchmode -nographics -quit -projectPath <work>/proj -buildTarget StandaloneOSX -executeMethod Hellscript.Editor.ProjectBuilder.BuildMac -hellscriptBuildOutput <out>/HELLSCRIPT.app -logFile <out>/build.log
ARGS=(-hellscriptSkillTreeD4Smoke -hellscriptOfflineQa -hellscriptSavePath "$SAVE" -hellscriptScreenshots "$SHOTS" -screen-fullscreen 0 -screen-width 440 -screen-height 956); <out>/HELLSCRIPT.app/Contents/MacOS/HELLSCRIPT "${ARGS[@]}"
```
  `BuildMac` is a development build (`E/ProjectBuilder.cs:68-71,78-95`) and the output must end in `.app` (:110-121). Use bash arrays; zsh does not split words. `-hellscriptOfflineQa` is only needed for flows that start at the title.
- **What the docs say.**
  - No "Smoke build recipe" heading exists. `R/Docs/Design/Hunt_Edict_UI_Overhaul.en.md:577-582` (§12.6) says AGENTS.md has none and lists the steps used locally.
  - Command lines are in `Docs/Implementation/Map_Display_Option.md:134-138`, `Town_Hunyuan_Models.md:109` and `Blacksmith_Validation.md:149`. Plan §15 is `D4_Skill_Tree_Plan_20261012.md:506` and the smoke spec is :460-471.
  - No script automates smokes. The only runner in the repo is `R/tools/profile_native_cpu.py` (CPU profile). The `run_smokes.py` named in that doc is not in the repo.

## 7. EDITMODE UI TESTS

- **Building uGUI in EditMode.**
  - Create `new GameObject("x",typeof(RectTransform),typeof(Canvas))`, add an `EventSystem` child for pointer tests (`T/UiButtonTests.cs:13-20`, `T/StoreViewBindingTests.cs:19-31`), or make a `ContentWindowHost` root, then call the real `Open(...)` (`T/SharedUiTests.cs:63`, `T/MonsterCodexWindowTests.cs:14-23`).
  - Measure `RectTransform.rect`, `Text.preferredHeight` and `Canvas.ForceUpdateCanvases()`.
  - Vary layout by `UiSafeArea.AspectRatio=w/(float)h` (the frame is fitted inside the Game view, `P/UiSafeArea.cs:25-26`).
  - `T/MonsterCodexWindowTests.cs:43-62` is the pattern: five sizes by ko/en, asserting row counts, tap targets >= 44 px, `preferredHeight<=rect.height+.5` and `Loc.MissingCount==0`. English comes from `Loc.Use("en",LocalizationTable.Parse(Resources.Load<TextAsset>("Localization/en").text).Entries)`. TearDown resets `AspectRatio=0`, `StopSimulating()` and `Loc.UseSource()`.
  - `TouchLayoutTests` is pure maths on `Runtime/Core/TouchLayout.cs`, with no GameObjects.
  - Pure-layout precedent: `BattleHudLayout`/`InventoryLayout` with `[TestCaseSource]` (`T/BattleHudLayoutTests.cs:9-26`), a good template for the plan's layout function.
  - `T/GraphicComponentTests.cs:13-24` reflects over every `Graphic` subclass: each needs `[RequireComponent(typeof(CanvasRenderer))]`, and `AddComponent(type)` with no setup must not throw.
- **Mesh golden and allocation test:** `T/ForgePresentationAllocationTests.cs:13-24,54-68` reflects `OnPopulateMesh` into a delegate and compares `vertices`, `colors32` and `triangles` with `T/Fixtures/forge-ring-baseline-N.json`. It then asserts `Is.Not.AllocatingGCMemory()` after warm-up.
- **Loading repo files.**
  - `File.ReadAllText(Path.Combine(Application.dataPath,"..","Docs/Design/MonsterRoster/roster.json"))` (`T/MonsterSoakTests.cs:16`).
  - CWD-relative `File.ReadAllText("Assets/HELLSCRIPT/Tests/Fixtures/RuneNativeParity.json")` with `JsonUtility` and `[Serializable]` classes (`T/RuneBoardNativeTests.cs:18`). This is a JS-to-C# parity fixture.
  - `Path.Combine(Application.dataPath,"HELLSCRIPT/Tests/Fixtures/RuneV13Layout.json")` (`T/RuneV13Tests.cs:67`).
  - There is no `InternalsVisibleTo`, so tests see only public members or use `BindingFlags.NonPublic` reflection (`T/StoreViewBindingTests.cs:18`).

## 8. ICONS AND MASKS

- **Crop.** A circular crop is a `Plate` `Image` with sprite `Art/GlobalHUD/mask-circle` and a stencil `Mask` (`showMaskGraphic=true`). The child `Icon` is an `Image` with `preserveAspect=true` (`P/SkillIconView.cs:28-35,41-46`). Passives use `mask-square` and `frame-active`. The HUD seal uses the same approach with `showMaskGraphic=false` (`P/GlobalHudView.cs:178-183`).
- **Masks.** They are 256² white RGBA sprites, bilinear. `mask-square` is a rounded square and `mask-circle` is a circle. A frame image (`frame-seal`, `frame-active`) covers the hard stencil edge.
- **Skill icons.** `Resources/Art/ClassSkillIcons/{Warrior,Ranger,Mage}/<id>.png`: 37 each, 1254², no mipmaps, not readable.
- **Budget rows.** `E/ResourceTextureBudget.cs:24` is Android 512 ASTC_6x6. The desktop table (`:63-82`, `Art/ClassSkillIcons/` 512) gives Standalone BC7 and WebGL ETC2_RGBA8. `Art/SkillTree/` has its own rows (`tree-backdrop` 1024, rest 512). Masks and frames have no row.
- **Tests.** `T/BuildResourceTests.cs:11-32` checks one icon sample for overrides plus GUID/meta stability; `P/RuntimeBuildResourceSmoke.cs:20-27` samples at runtime.
- **Old tree art** (`Art/SkillTree/node-frame-*`, `tree-backdrop`, `skill-point-gem`) is used at `P/HuntEdictWindow.Skills.cs:25,77,86,232-241`.
- **New ids.** The 84 modifier ids are unknown to `TryResolve`. Extend it, or `Resources.Load` the sprite and call `SetSprite`.

## 9. FILE/ASSEMBLY CONVENTIONS

- **Layout.**
  - `P/` is flat (493 `.cs` files, 157 of them smokes). Namespace is `Hellscript`.
  - There are three asmdefs: `Runtime/Hellscript.Runtime.asmdef` (all of Core, Presentation and Runes), `Editor/Hellscript.Editor.asmdef`, and `Tests/Editor/Hellscript.Tests.asmdef` (Editor only).
  - Partial naming is `X.cs` (Open, Update, Reflow) plus `X.<Topic>.cs`, as in `BlacksmithWindow.Chrome/Controls/Slots.cs` and `HuntEdictWindow.Skills.cs`. Use `SkillTreeD4Window*.cs` and `RuntimeSkillTreeD4Smoke*.cs`.
- **Registration in GameUI.** Add a partial `GameUI.<Name>.cs`:
  - Follow `P/GameUI.Blacksmith.cs:5-21`, `GameUI.RuneBoard.cs` and `GameUI.AspectStone.cs`: field, public accessor for smokes, `Show…()` and `Close…()`.
  - `Show…()` gates with `RequireContent(...)`, closes sibling windows (`CloseRuneMaster();CloseBlacksmith();CloseHudPanel();CloseStorage();CloseEquipmentShop();ResetTownInput();game.Town.Cancel()`), then calls `Open(transform,…)` with a `closed` callback that nulls the field and runs `RefreshHud();RefreshPlaza();`.
  - The host is created at `P/GameUI.cs:38`.
  - `CommonPanelOpen` (`P/GameUI.ScreenSettings.cs:22`) picks up `windowHost.BlocksGameplay` automatically.
  - Language: add to the `P/GameUI.cs:109` chain, or poll in `Update`.
  - Entry points to repoint: `P/GameUI.HuntEdict.cs:69`, `P/GameUI.FirstPlay.cs:22`, `P/HuntEdictWindow.SkillPresets.cs:59`, `P/HuntEdictWindow.cs:60`, `P/RiftEntryWindow.cs:355`.
- **Template.** `R/tools/new_content_ui.py` plus `R/tools/templates/ContentWindow.cs.txt` emits a static `<Name>Window` that calls `ContentWindowView.Open`. That is the route that needs no ADAPTERS entry.
- **`check_ui_contract.py` failures** (`R/tools/check_ui_contract.py`):
  - `{owner}: missing shared owner {token}` (:58).
  - `{name}: use UiFonts instead of creating a font` (:62).
  - `{name}: new content must open ContentWindowView; specialized layouts require a reviewed adapter` (:67): any `class \w+Window` whose owner is not in ADAPTERS and has no `ContentWindowView.Open`.
  - `{name}: canvas ownership belongs to ContentWindowView/ContentWindowHost` (:70): `AddComponent<Canvas>` or `typeof(Canvas)` outside ADAPTERS or `CANVAS_OWNERS` (:25-26).
  - `{name}: chrome colour {hex} must use a UiTheme token` (:77): any `"rrggbb"` or `"rrggbbaa"` string literal that `chrome_colour()` (:33-42) judges warm, dark or ivory. By that function, `b9c6d0`, `fff3d6` and `d3af72` fail, while `6f8291` and `e07a32` pass. The rule is heuristic, so put every design colour in `UiTheme.cs` (exempt, :31).
  - `reusable views cannot mutate or read a live account` for `PURE_VIEWS` (:78-79).
  - `test_ui_contract.py:22` runs the same check on a copy of the whole Presentation folder.
- **`check_ui_refresh.py:55-66`:** `{path}:{line}: define visibleState or an audited owner guard`. Every `StoreViewBinding.Attach` in a top-level `P/*.cs` needs >= 5 arguments or `visibleState:`. It scans only top-level files and skips `Runtime*`.
- **Source-scanning EditMode tests.**
  - `T/LocalizationTests.cs:239-255`: every non-interpolated Korean literal in Runtime (smokes excluded) needs an exact `en.txt` entry, and interpolated Korean fails.
  - `T/EditorNullPatternTests.cs:41-49`: no `GetComponent…() ??` or `?.`; use `TryGetComponent`.
  - `T/GraphicComponentTests.cs` (see section 7).
- **Docs.** `Shared_UI_Contract(.en).md` has a section per adapter; :108 is the old skill-tree adapter. `AgentRules/Shared_UI.md:12` requires the exception's reason and verification to be written down.

## RISKS

1. **Font.** Legacy `Text` has an int `fontSize`, so the design's 10.5, 12.5 and 14.5 pt sizes round. Noto Serif KR and Cinzel are not available, and `UiFonts.Display` varies by OS. Line-height is a font-metric multiplier, and there is no letter-spacing or keep-all. Gradient title text and glow need new code.
2. **No anti-aliasing on custom meshes.** Only `HuntEdictSurface` has a feather fringe. Hex, arc and round shapes will look jagged without one. `UiMesh` also lacks hairline snapping except where `px=1/canvas.scaleFactor` is copied (`P/UiButtonFace.cs:55-56`).
3. **Colour space.** `ProjectSettings.asset:49` is Linear (`m_ActiveColorSpace: 1`). Alpha compositing and gradients can differ from CSS gamma blending, and nothing in the repo verifies this.
4. **Filters.**
   - Blur, grayscale, brightness and soft drop-shadow have no engine path. A new shader needs Stencil and `_ClipRect` support, which `PuzzleHubTone` lacks.
   - Skill icons are non-readable and have no mips, so baked blur is impossible, and plan §16 forbids touching icons.
5. **Clipping.** `RectMask2D` plates clip outer glows and the design's out-of-box ornaments (`gx-pan-*` at `top:-12px`). These need an unclipped layer above the scroll.
6. **Performance.**
   - `UiMesh` helpers allocate, so any per-frame rebuild (ignite, glint, ring bloom) needs allocation-free dedicated Graphics.
   - Per-icon stencil `Mask` (`SkillIconView`) breaks batching across about 48 icons.
   - Moving scroll content re-batches the whole canvas.
   - `AGENTS.md` and `CPU_Performance.md` forbid steady per-frame work.
7. **Scale mismatch.** `UiTheme.Scale` bases are 800x450 and 405x720, and the Blacksmith U is clamped 8-17. The design is 1:1 px (440x860, 832x419, 1152x648 stages), so a new fit-and-centre scale is needed. `UiTheme.Scale` must still be called somewhere real (not in a comment) to satisfy the ADAPTERS token.
8. **Scroll.** No two-axis or X-persisting scroll exists. Nested scroll is avoided by locking the outer one. The landscape plate's right-edge fade has a precedent only in `GlobalHudFade.shader` (`P/StatusStripView.cs:46`).
9. **Reduce motion.** No toggle writes the key, the OS flag is macOS only, and `ForgeWorkingPulse` ignores it. Use `StoryDialogueWindow.ReduceMotion`.
10. **Screenshot comparison.** References are 2x (880x1912, 1912x880, 2880x1620). `macRetinaSupport:1` and `resizableWindow:0` (`ProjectSettings.asset:89,104`) mean oversized windows may be clamped. A capture strategy is needed; nothing in the repo automates smokes or the side-by-side comparison.
11. **Mask radius.** `mask-square` radius is fixed (about 13%) while the design's radii differ per widget. The stencil edge is hard. A UV-mesh crop (gap 9) is the alternative.
12. **Texture weight.** 92 new 512² icons are about 24 MB each for BC7 (desktop) and ETC2 (WebGL), not the 11 MB APK-only figure in plan §11.
13. **Gating and lint.**
    - `SkillIconAssets` does not resolve new ids.
    - A localization entry is needed for every Korean literal.
    - New colours must be `UiTheme` tokens (the lint is heuristic).
    - `GetComponent … ?.` is banned.
    - Pre-existing worktree edits and the uncommitted `D/golden/` mean tests reading goldens depend on commit state.