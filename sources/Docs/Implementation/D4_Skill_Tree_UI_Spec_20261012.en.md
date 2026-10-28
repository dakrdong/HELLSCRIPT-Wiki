# Skill Window Screen Implementation Spec (for Codex)

> **2026-10-28 A2 correction:** References to the large edict window, ≡ menu and their dedicated components or smokes are historical. Use the [current entry points](../../Assets/HELLSCRIPT/Runtime/Presentation/GameUI.HuntEdict.cs) and [replacement screens and source owners](Hunt_Edict_Window_Removal_20261028.en.md). Earlier captures and test counts describe the earlier implementation.

Updated: 2026-10-12 · For: Codex implementation work · [한국어](D4_Skill_Tree_UI_Spec_20261012.md)

Author: Claude (spec and verification material). Codex implements, verifies and commits.

This document spells out sections 9, 10, 11 and 13 of the plan document [`D4_Skill_Tree_Plan_20261012.md`](D4_Skill_Tree_Plan_20261012.en.md) **to a level you can implement from**. There is one goal: **the in-game skill window has the same look and the same behaviour as the design.** For rules and decisions the plan document takes priority; **for numbers, the golden files in `Docs/Design/D4SkillTree/golden/` take priority**. When documents conflict, follow that order.

## 0. Material map and reading order

| Material | Contents | Priority |
|---|---|---|
| Plan `D4_Skill_Tree_Plan_20261012.md` | Rules, decisions (section 17), steps (section 14) | Rules, decisions |
| `golden/*.json` | Numbers obtained by actually running the design: rules, layout, wording, interaction, internal dimensions, look | **Numbers** |
| `golden/csharp/` | A C# port that passes all of the numbers above (rules, layout, view model, session) | Starting code |
| This document | How to move the screen to uGUI | Method |
| `golden/anatomy/OUTLINE.md` `TEXT_STYLES.md` | Measured rectangles inside the detail panel and widgets, 129 text styles | Reading material |
| `research/ui_infrastructure.md` | Facts and gaps of today's drawing tools, window structure, input and tests | Evidence |
| `reference/*.jpg` | 24 reference renders (2×) | Visual check |
| `SkillWindow.dc.html` | The design source | Last resort |

Reading order: `golden/README.md` → sections 1 to 3 of this document → your work package (section 13) → other sections as needed.

## 1. Fidelity contract and verification layers

Do not leave "identical" to the human eye alone. Split it into six layers and put **the layers a machine checks first and the layer checked by eye last**.

| Layer | What | Reference | How | Pass criterion |
|---|---|---|---|---|
| L0 Rules | Invest and refund verdicts, blocked reasons, allocation hash | `rules_golden.json` | `SkillTreeD4RulesTests` | 32,208 checks match |
| L1 Layout | Rectangle, state and text of every element, and the draw order | `layout/*.json`, `layout_scenarios/*.json` | `SkillTreeD4LayoutTests` (pure function) | 2,975 + 15,034 checks match |
| L2 Wording | Text of the panel, tabs, header numbers and accessibility text | `viewmodel/*.json` | `SkillTreeD4ViewModelTests` | 600 checks match |
| L3 Interaction | State and effect signals at every step of 60 sessions (select, invest, refund, tab) | `interaction/*.json` | `SkillTreeD4InteractionTests`; the smoke test replays the same 60 sessions with **real clicks** | 4,986 checks match, click replay matches |
| L4 Internal dimensions | Inner rectangles and font sizes of the 6 detail-panel states and of widgets (cards, badges, passives, tabs, legend …) | `anatomy/*.json` | `SkillTreeD4AnatomyTests`: measure the real uGUI rectangles and compare | Rectangles ±1pt, text boxes ±2pt, font sizes as values snapped to the 0.5pt grid |
| L5 Look | Colour, gradient, border, glow | `styles.json`, `reference/*.jpg` | Compare smoke test screenshots with the reference renders (section 14) | Within the pass limit |

L0 to L3 **already pass** if you copy the port (`golden/csharp`) (55,803 checks, `cd Docs/Design/D4SkillTree/golden/csharp && dotnet run`). Implementation work starts by moving these into Unity EditMode tests. L4 and L5 are the job of the UI implementation.

**What must match**
- Rule verdicts and blocked reasons (down to the hash), element counts and draw order, every rectangle (±0.01pt, ±1px on screen), state names, card font sizes, wording (Korean and English), interaction results.
- The composition and element positions of the detail panel (L4), and the reserved height of the description slot (2 lines in portrait and landscape, 3 lines on PC).
- Colours exactly as the values in `styles.json` (the hex must be the same after you move them into tokens).

**What may differ — allowed approximations (a difference outside this list is a defect)**

| No. | Item | Allowed range | Reason |
|---|---|---|---|
| A1 | Font shape | Follows the decision of D6 in section 17. Different glyph widths can move the line-break positions. | The design uses Noto Serif/Sans KR and Cinzel. The game uses `UiFonts`. |
| A2 | Blur of the detail-panel backdrop picture | Shader blur (recommended), or an unblurred picture with the same position, size and opacity | Icons are non-readable textures, so a CPU blur is impossible (`research/ui_infrastructure.md` gap 6). |
| A3 | Gradient text (title, names) | 2-colour gradient from top → bottom. The middle colour stop may be omitted | A glyph rectangle has only 2 vertex colours. |
| A4 | Edge softness | Hand-drawn meshes may be rough by ±0.5px. A 1px feather is recommended for rounded-corner shapes | `UiMesh` has no antialiasing. |
| A5 | Outer glow, inner shadow | Approximate the same colour and spread with rings of 3 to 6 steps | The engine has no CSS `box-shadow`/`inset`. |
| A6 | Effects | Follow the durations and curves of `fx.json`, but frame-exact matching is not required (±10%). | |
| A7 | Alpha compositing | The project uses the Linear colour space (`ProjectSettings` `m_ActiveColorSpace: 1`), so glows can look brighter or darker than in the design. Match it with 1 global correction constant. | CSS composites in gamma space. |
| A8 | Antialiasing of small shapes | Dots and diamonds under 6px may be snapped to whole pixels. | |

## 2. Code structure and data flow

### 2.1 Files

| Location | File | Responsibility | Source |
|---|---|---|---|
| `Runtime/Core/SkillTreeD4/` | `SkillTreeD4.Model.cs`, `SkillTreeD4.Rules.cs` | Data model, allocation string and hash, rules engine | Copy from `golden/csharp` (already in place from **step 1**) |
| `Runtime/Core/SkillTreeD4/` | `SkillTreeD4.Data.cs` | `Resources/Data/SkillTreeD4.json` → model (`JsonUtility` DTO) | New (step 1) |
| `Runtime/Presentation/SkillTreeD4/` | `SkillTreeD4.Layout.cs`, `.ViewModel.cs`, `.Session.cs`, `.Fx.cs` | Pure layout, view model, state machine, effect signals (no UnityEngine) | Copy from `golden/csharp` (**UI-1**) |
| Same folder | `SkillTreeD4Units.cs` | Design points ↔ screen units (section 3) | New |
| Same folder | `SkillTreeD4Window.cs` + `.Scene.cs` `.Panel.cs` `.Header.cs` `.Input.cs` `.Fx.cs` `.Look.cs` | uGUI window | New |
| Same folder | `SkillTreeD4Graphics.cs` | New drawing elements (section 5) | New |
| `Runtime/Presentation/` | `UiButtonFace.cs`(`UiMesh`), `UiTheme.cs` | Add drawing functions, add colour tokens | Edit |
| `Tests/Editor/` | `SkillTreeD4{Rules,Layout,ViewModel,Interaction,Anatomy,Graphics}Tests.cs`, `GoldenJson.cs`, `GoldenShapes.cs` | Tests | Copy from `golden/csharp/Tests`, then move to EditMode |
| `Presentation/` | `RuntimeSkillTreeD4Smoke*.cs` | Smoke test `-hellscriptSkillTreeD4Smoke` | New |

The files in `golden/csharp` use only `namespace Hellscript`, C# 9 and `System`. You may rename them and move folders to follow the game's conventions, but **the behaviour must keep passing the L0 to L3 tests.** Coordinates are `double`, but the tolerance is 0.01pt, so you may change them to `float`.

### 2.2 Flow

```
GameStore (levels per hero, allocation)
   │  read                         ▲ commit (save after validation)
   ▼                               │
ISkillTreeD4Host ── the only storage boundary the window uses (see 2.3 below)
   │
SkillTreeD4Session  (tab, selection per hero, allocation copy per hero)  ← user input: Select / Invest / Refund / Tab
   │ Context()
   ├─► SkillTreeD4Layout.Build   → SkillTreeD4Scene   (rectangles, states, text, draw order)
   └─► SkillTreeD4ViewBuilder    → SkillTreeD4View    (panel, tab and header text, accessibility text)
                │
          SkillTreeD4Window (uGUI)  ← updates **state only** by comparing scene keys; never clears and rebuilds
```

- The scene and the view model are pure functions: the same input always gives the same result. The window recomputes them whenever the selection, allocation, language or level changes and **applies only the differences by node key** (4.4).
- The store is the source of truth. The allocation in `Session` is a copy that lives while the window is open, and **when an invest or refund is accepted it is saved immediately with `host.Commit`**. If the commit fails or the `visibleState` key changes (another window or the battle quick edit changed it), resync to the host's values and **keep the selection**.

### 2.3 Host boundary

The window does not know `GameStore` directly. This boundary is needed so that tests and smoke tests can open the window without a store.

```csharp
public interface ISkillTreeD4Host
{
    int Level(string hero);                                   // hero level (1..60)
    SkillTreeD4Alloc Alloc(string hero);                      // saved allocation (read-only copy)
    bool Commit(string hero, SkillTreeD4Alloc alloc);         // save after rule validation. Returns false if rejected
    string VisibleState();                                    // StoreViewBinding's visibleState key (a value that depends on allocation, level and language)
    bool PlayerCanEdit(string hero);                          // false when editing is blocked inside compare, training or tutorials
}
```

- The production implementation is built on top of `GameStore` in step 6. Step 4 (UI) is developed and tested with an in-memory implementation (`InMemorySkillTreeD4Host`).
- Each hero has its own level. The design fixed it at 24, but **the three values and the passive unlocks use the values of the hero on the tab** (plan 9.3).

### 2.4 Naming rules (names that tests, smoke tests and tutorials look up)

The old window used `edict-…` names. The new window uses the names below as **GameObject names**. Tutorial pointing (step 6) and the smoke tests use these names.

| Element | Name |
|---|---|
| Window root / detail panel | `d4-window`, `d4-panel` |
| Header | `d4-title`, `d4-tab-<0..2>`, `d4-stat-<level\|spent\|left>`, `d4-close` |
| Tree plate / scroll content / legend | `d4-plate`, `d4-content`, `d4-legend` |
| Tier badge | `d4-badge-<0..4>` |
| Active card | `d4-card-<id>` (example: `d4-card-W09`) |
| Modifier socket | `d4-mod-<id>` (example: `d4-mod-W09-2`) |
| Passive | `d4-passive-<id>` |
| Tag (rank chip and so on) | `d4-tag-<for>-<id>` |
| Connector line groups | `d4-lines`, `d4-arrows` |
| Inside the panel | `d4-panel-name`, `d4-panel-desc`, `d4-panel-mod-<0..2>`, `d4-panel-stat-<n>`, `d4-panel-invest`, `d4-panel-refund`, `d4-panel-reason`, `d4-panel-link-<n>` |

Accessibility and test text comes from `SkillTreeD4View` (`CardLabel.Aria` and so on). Use that text for the `UiButton` name of a button and for its test description.

## 3. Window shell, units, entry points

### 3.1 Units: fit design points to the screen (fit-and-centre)

Design coordinates are design points. There are three frames.

| Layout | Frame | Safe area (inside the frame, points) | When the frame is used |
|---|---|---|---|
| Portrait | 440×956 | (0, 62) – 440×860 | Mobile, and safe-area height > width |
| Landscape | 956×440 | (62, 0) – 832×419 | Mobile, otherwise |
| PC | 1152×648 | The whole frame | Desktop (including web desktop) |

For the mobile/desktop decision, use the platform check the project already has (find `Application.isMobilePlatform` with `grep` and do it the same way). Do not write a new one.

```
S  = UiSafeArea.Current (pixel rectangle)
P  = min(S.width / safe.w, S.height / safe.h)            // 1 design point = P screen pixels
frame top-left (pixels) = S.center − safe.center × P     // the frame's safe area lands at the centre of S
```

- **Canvas unit = half a design point.** `CanvasScaler` is `ConstantPixelSize` with `scaleFactor = P / 2`. Write every coordinate, size and font size as design points × 2. The design has many half-point font sizes (10.5, 12.5, 14.5pt), so taking half a point as the unit makes every font size an integer (`Text.fontSize` is an integer).
- Font sizes multiplied by the PC zoom (`plate.inner.scale`, 0.99 to 1.18) are rounded to integers, with an error of at most 0.25 points. **Apply the zoom by multiplying coordinates, sizes and font sizes, not through `localScale`.** Scaling with `localScale` blurs the text.
- The background (dark base and radial gradients) paints the whole screen, beyond the frame. The area outside the frame's safe area (for example the top 62pt in portrait) is under the notch, so only the background shows there.
- When only the size changes (window resize, safe-area change), change only `scaleFactor` and the frame position. **Do not rebuild.** Rebuild the scene only when the layout kind or the language changes.
- Test vectors (EditMode, `SkillTreeD4UnitsTests`):

| Screen (px) / safe area | Layout | P | Frame top-left (px) |
|---|---|---|---|
| 956×440, safe (62,0)–832×419 | Landscape | 1.0 | (0, 0) |
| 440×956, safe (0,62)–440×860 | Portrait | 1.0 | (0, 0) |
| 1080×2400, no safe area | Portrait | 2.4545 | (0, −7.6) |
| 2400×1080, no safe area (mobile) | Landscape | min(2400/832, 1080/419) = 2.5776 | (−32.1, 0) |
| 1920×1080 desktop | PC | 1.6667 | (0, 0) |
| 2560×1080 desktop | PC | 1.6667 | (320, 0) |

### 3.2 Window shell

- It is an adapter window. Because it is a special layout that uses the design's coordinates as they are, it is an adapter instead of a `ContentWindowView`. The forge window (`BlacksmithWindow`) is the precedent: it creates its own `Canvas` (`ScreenSpaceOverlay`, `CanvasScaler`, `GraphicRaycaster`) under GameUI and joins the window stack, Back handling and pause through `ContentWindowHost.Attach(this, Escape)`. The host rewrites the sorting order.
- **Always** give `StoreViewBinding.Attach(this, store, Repaint, ready, visibleState)` a visibleState key (`tools/check_ui_refresh.py` requires it). What goes in the key: the levels and allocations of the 3 heroes, the language, and values related to account points. While the user is pressing or scrolling, `ready` returns false to defer repainting.
- Reflowing (`Reflow`) is done by polling, as in the forge: check in `Update` whether `UiSafeArea.Current`, `Loc.Language` or the layout kind has changed.
- **Register it in `ADAPTERS` in `tools/check_ui_contract.py`.** Choose the required tokens from what the window really uses: `ContentWindowHost.Attach`, `SkillTreeD4Units`, `UiSafeArea.Current`. `UiTheme.Scale` in plan 9.1 is a value this window does not use, so **do not call it just to satisfy the token** (`research/ui_infrastructure.md` risk 7). Write the reason and the verification method as one adapter section in `Docs/Implementation/Shared_UI_Contract(.en).md` (`AgentRules/Shared_UI.md:12`). The existing "skill tree adapter" section describes the old tree, so add a new adapter section in UI-3 and replace the old section with a description of the new window in step 7.
- Colour literals live only in `UiTheme` (section 6). A Korean text literal must have an `en.txt` entry (`LocalizationTests`), and `GetComponent … ?.` is forbidden (`EditorNullPatternTests`): use `TryGetComponent`.

### 3.3 Entry points

- Switch to the new window only the calls that open the old skill tree page directly: `GameUI.ShowEdictSkillTree()` and `HuntEdictWindow.SelectSkillPage(true)`. Callers (re-check with `grep` before implementing): `RiftEntryWindow.cs` (empty skill slot), `GameUI.FirstPlay.cs`, `HuntEdictWindow.SkillPresets.cs` (the "back to skill tree" button), `HuntEdictWindow.cs` (Back handling), `HuntEdictWindow.Presets.cs` (moves to the skills tab when a saved preset fails validation). For the full list and the delete/keep split, see section A of `research/dependents.md`.
- Leave the entry points that open the whole edict window as they are. The new window is reached from the "Skill tree" button on the edict window's "skills" tab (plan D1).
- **Parent window refresh risk (not in the plan):** The edict window's `HuntEdictEditSession` is created once when the window opens and is refreshed only after its own commit. If the skill window opened on top commits, the parent's save button can revert to the old values and the loadout bar shows old ranks. Raise a `Committed` event when the skill window commits, and have the parent call `AcceptSaved` or rebuild at that point.

## 4. Moving the scene to uGUI

### 4.1 Hierarchy (draw order)

The scene's `Primitives` are listed in **draw order** (later is on top). Carry the same order into the hierarchy.

```
d4-window (Canvas)
 ├ Backdrop                     Whole screen. Base colour + 3 radial gradients + vignette (styles.json frame.root.*)
 └ Frame                        Frame coordinate system (3.1)
    ├ Header                    Title, 3 tabs, 3 header numbers, close
    ├ Plate                     Tree plate: border and background (plate.*) ← laid underneath
    │   ├ Viewport (RectMask2D) Scroll viewing area
    │   │   └ Content           Size = scene plate.inner (PC: zoom and offset applied)
    │   │       ├ Bands         Passive shelves and headings
    │   │       ├ Trays         Modifier trays (portrait)
    │   │       ├ Lines         All connector lines in one mesh, in scene order
    │   │       ├ Arrowheads    Arrowheads in one mesh
    │   │       ├ Badges        5 tier badges
    │   │       ├ Cards         10 active cards
    │   │       ├ Modifiers     28 modifier sockets (2 sockets for an ultimate)
    │   │       ├ Passives      10 passives
    │   │       ├ Tags          Rank chips, level chips, unlock levels, passive ranks
    │   │       └ FxContent     Line effects (ignite, glint) sit above Lines, node effects above each node
    │   ├ Legend                Legend strip (24pt at the bottom, inside the plate, takes no input)
    │   └ CornerCaps            Corner ornaments: an overlay **outside the mask** (sticks out beyond the plate edge)
    └ Panel                     Detail panel (section 8). Its corner ornaments are also outside the mask
```

- Elements of the same kind can be in any order (they do not overlap). Keep the order between kinds as in the hierarchy above. **Lines are below cards, sockets and passives, and tags are all above.**
- Do not split lines by state colour; put them **into one mesh in scene order**. For each line, draw the base glow (the under-stroke) and then the main line in that order so that overlaps match the design (SVG paint order). Do not create a GameObject per segment.
- The corner ornaments (`.gx-cap-l`, `.gx-cap-t`, `.gx-pan-*`) are in `rules` of `fx.json`, as CSS verbatim. Some ornaments stick out 6pt beyond the plate edge, so they are clipped if placed inside the `RectMask2D`.

### 4.2 Implementation per element kind

| Scene element | View | How to draw | When the state changes |
|---|---|---|---|
| `Band` passive-shelf / passive-heading | `BandView` | Rounded-rectangle gradient + border (`band.*`) / text | Colour only |
| `Tray` | `TrayView` | Rounded rectangle, inner shadow (`tray.learned`, `tray.unlearned`) | Border solid line ↔ dashed line |
| `Line` + `Arrowhead` | `LinesGraphic`, `ArrowsGraphic` | `PolylineGraphic` (section 5): horizontal and vertical segments, rounded ends, dashed line (`k`), base glow (`line.*.glow`) | Line state colour (change only the vertex colours) |
| `Badge` | `BadgeView` | Hexagon medallion (Roman numeral, `Cinzel`), name, condition text, lock icon (portrait), **progress bar of the next locked tier** (`Bar`, `bar.tier`, width = `Bar`%) | Open / next locked / locked |
| `Card` | `CardView` | Rounded rectangle (`card.box.*`), icon socket + icon, name (fitted font size), **rank bar** (`Bars`: `rank`, `ghost`; 2pt along the bottom edge), `n/max` pill in portrait | State l/o/k, selected, modifier focus, hot (has a ghost bar) |
| `ModifierSocket` | `ModifierView` | Hit area (`Hit`) and face (`FaceW×FaceH`), icon (`modifier.icon.*`) | chosen/available/idle/unlearned/tier-locked, focused |
| `PassiveNode` | `PassiveView` | Silver coin medallion + **rank arc** (`Arc`) + icon / key passive is a gold hexagon / portrait uses a capsule (name, "Unlocked Lv2 · 3 / 5") | l/o/k, selected, **avail** (investable glow) |
| `Tag` | `TagView` | Pill or round chip + text (`tag.*`) | Text, lit |

- **State → look** is determined by the names in `styles.json`. The rule that builds a name from a scene element's fields (state, selected, hot, avail …) is in the `S(label, …)` calls of `golden/lib/scene.js`. One name determines a look (the generator checks this). The names inside the detail panel (buttons, modifier rows, passive block) are in `golden/panel_parts.json` for each node (section 6). The `{pct}` and `{arc …}` placeholders are values that differ per element (card and badge bar width, number of lit segments of the rank arc).
- **Rank arc**: segment count `Total` (5; 3 for an ultimate), lit segments `Lit`. The angle of segment k, clockwise from 12 o'clock, is `[g/2 + k·seg, g/2 + k·seg + seg − g]` with `seg = 360/Total` and gap `g = Total > 3 ? 8° : 10°`. Lit segments are `#eaf6ff` (coin) or `#f2d9a4` (detail-panel active), unlit segments are `#1a2126` or `#2a241c`. The arc thickness is 2pt for the coin and 3pt for the detail-panel passive.
- **Hexagon**: vertices `(50% 0, 93% 25%, 93% 75%, 50% 100%, 7% 75%, 7% 25%)`, pointed at the top. Badge medallion 27×30, key passive 38×38.
- The inner rectangles of an element (icon position, text boxes) follow **L4** (`widgets.*` in `anatomy/OUTLINE.md`). The scene gives only the outer rectangle.

### 4.3 PC, scrolling, clipping

- Portrait: the tree **scrolls vertically** (content 420×1460, plate interior about 422×404). When opened with "passive view", it starts at `views.passives.initialScrollTop` (`passTop − 200`). Changing the tab keeps the scroll position (clamped into range).
- Landscape: **scrolls horizontally** (content 718 (Mage 854)×332, plate 548×357). The content height 332 fits in the viewport height 355, so there is no vertical scroll. The legend strip is drawn over the bottom 24pt. The right edge of the plate has a 28pt fade (`.gx-cap-l::after`).
- PC: **no scrolling.** Scale the whole content by `plate.inner.scale` (at most 1.5) and place it at `plate.inner.offsetX/offsetY`. Multiply coordinates, sizes and font sizes (3.1).
- Scroll on one axis only. Two-axis scrolling has no precedent in the project (section 3 of `research/ui_infrastructure.md`). When redrawing, save the scroll position and restore it, corrected after `Canvas.ForceUpdateCanvases()` (precedent: `ContentWindowView`, `StorageWindow`).
- Pressing a card, socket or passive follows the `UiButton` rules. Use the same `EventSystem` path as `ScrollRect` so that dragging (scrolling) cancels the press.

### 4.4 Updating state only (never clear and rebuild)

- Get the scene by calling `Build` again (it is small). The window matches each element to its previous view by a **stable key**:

| Kind | Key |
|---|---|
| Card / modifier socket / passive | `card:<id>` / `mod:<id>` / `passive:<id>` |
| Badge / tray / band | `badge:<tier>` / `tray:<u>` / `band:<for>:<tier>` |
| Tag | `tag:<for>:<u or n>:<j>` |
| Line, arrowhead | Order in the list (if the structure is the same, the count and order do not change) |

- A new key creates a view, and a vanished key hides its view (returns it to the pool). For the same key, apply **only the changed fields** (state, bars, text, selection). Update line vertex colours only for the changed segments.
- With these rules, one invest causes updates to 1 to 2 cards, 0 to 1 sockets, a few tags, a few lines, the header numbers and the panel. This update happens once, in the click transaction frame; the frames that follow neither allocate nor rebuild the UI (4.5, plan 9.7).
- When (hero, layout kind, language) changes, rebuild the set of views (on a tab switch the tree rises from 6pt below, an effect of 0.26s, section 11).

### 4.5 Allocation standard: where 0 allocation applies
"0 allocation" is **not a standard for the whole click transaction frame.** One invest, refund or modifier selection goes through `host.Commit` (the save still serialises and copies with `JsonUtility`), `SkillTreeD4Layout.Build` (new lists) and the view model recomputation once in one frame. That work may allocate. The standard has three layers.

| Layer | Target | Standard | Check |
|---|---|---|---|
| **0 allocation** | ① the effect update `FxPlayer.Tick` (whether or not an effect is playing) ② the window update in a frame where the state did not change (when the key comparison ends with "no change", the layout, the view model and the UI are not rebuilt) ③ `OnPopulateMesh` of every new `Graphic` (5.8) | 0 bytes of GC allocation | Wrap the calls in these three places in a delegate and apply `Is.Not.AllocatingGCMemory()` (precedent: `ForgePresentationAllocationTests`) |
| **Allowed once** | The click transaction frame: `host.Commit` → rule check → layout → view model → key-comparison update | May allocate. But call each of them only once per transaction (do not run `Commit` or `Build` twice in the same frame), and the frames that follow are in the 0-allocation layer. Do not rebuild the hierarchy: a new key borrows a view from the pool, and a new one is created only when the pool is empty | The click replay smoke test writes the `GC.Alloc` bytes and count of the click frame, and the 0 of the next frame, into its report (for example `ProfilerRecorder` with `GC Allocated In Frame`). The click frame has no pass limit. Set one with the user after the first measurement |
| **Performance claims** | Statements such as "faster" or "reduced" | At least 3 runs of each real before and after binary, with raw CPU and GC data | Project rule |

- If rapid clicking makes N transactions, there are just N click frames. The frames between transactions are in the 0-allocation layer.
- Passing means "the three 0-allocation places are 0, the click frame does not rebuild the hierarchy, and the allocation of that frame is written in the report", not "the click frame is 0".

## 5. Extending the drawing tools

What today's drawing tools (`UiMesh`, `UiButtonFace`, `SkillTreeGraphic`, `HuntEdictSurface`, `StorageSurface`, `RuneGemMesh` and so on) can do, and **what they cannot**, is in section 1 of `research/ui_infrastructure.md`. Counting the CSS features the design uses gives the following (`node golden/tools/css_features.js`, 428 looks and 56 common rules).

| Feature | Number of looks that use it | Today | Needed |
|---|---|---|---|
| Vertical and diagonal gradient fill | 142 | Exists (`UiMesh.Gradient/RoundedBox`) | |
| **Gradient border** | 143 | **Missing** (all borders are single-colour) | 5.1 |
| Radial gradient | 93 | Partial | 5.5 |
| **Angular (conic) gradient** — rank arc, coin border | 46 | **Missing** | 5.2 |
| Outer glow / inner shadow | 160 / 100 | Partial (1px feather) | 5.5 |
| Dashed border and line | 32 | Straight lines only | 5.3, 5.4 |
| Hexagon | 39 | No usable public function | 5.2 |
| Text shadow | 86 | Exists (`Shadow`, fixed offset) | |
| Brightness and saturation filter (locked icons) | 23 | Missing | 5.6 |
| Blur | 9 | Missing | 5.6 |
| Gradient text | 2 (title, name) | Missing | 5.7 |

Write new functions in the existing owner (`UiMesh` lives in `UiButtonFace.cs`). Put colour-stop arrays in `UiTheme` (they are excluded from the colour-literal check).

### 5.1 Gradient border
`UiMesh.RoundedRing(m, rect, radius, width, Func<float, Color> byY)` — an overload with the same shape as `Gradient`. The design's `GILT_RIM` (5 colours) and `STEEL_RIM` use it. The inner fill is the existing `RoundedBox`.

### 5.2 Hexagon, arc, angular gradient
- `UiMesh.Hex(m, rect, Func<float,Color> fill)`, `HexRing(...)`: vertices `(50% 0, 93% 25%, 93% 75%, 50% 100%, 7% 75%, 7% 25%)`. Badge medallion (27×30), key passive (38×38 + inner 24).
- `UiMesh.Arc(m, center, rIn, rOut, startDeg, sweepDeg, Func<float,Color> byAngle, segments)`: the rank arc (4.2) and the medallion's **angular gradient border** (`conic-gradient(from 210deg, …)`, colour stops in `styles.json`). For a smooth look, use at least 12 subdivisions per 1 arc segment.
- Wrap the rank arc in `RankArcGraphic` (segment count, lit segments, two colours). In the invest and refund effect, at the moment lit segments increase, it goes to 1.18× size and 2.2× brightness and then settles (`gxArc`, 0.55s).

### 5.3 Lines: `PolylineGraphic`
- Emit the scene's segment list (horizontal and vertical segments, width 2pt, **rounded ends**) as **one mesh**. For each line, the order is `base glow` → `main line` (the design uses 3 SVG `<path>`s: glow, main line, effect).
- Look per state (width 2pt): `g` gold `#e9c886` + glow 7pt `rgba(242,180,96,.30)`, `o` brown `#8a7556` + **black groove** 5pt `rgba(0,0,0,.8)`, `k` dashed line `#4a4238` (dash 4·4, no glow), `s1` lit silver `#b9c6d0` + glow 7pt `rgba(150,200,235,.28)`, `s2` silver `#6f8291` + black groove 5pt, `s3` dark silver dashed line `#3e4852` (dash 4·4, no glow). Arrowheads are filled with the colour of the same state (`s2` `#6f8291`, `s3` `#3e4852`). The values are `line.*` and `arrowhead.*` in `styles.json`.
- Effects (ignite, glint) draw a white line over the same segment on a **partial range** with `SetRange(t0, t1)` (section 11). The values are the segment length normalised to 0 to 1. Use allocation-free buffers.
- An arrowhead is an 8×8 triangle (`0,0 8,4 0,8` and so on) in the state colour.
- Skip a segment shorter than 1pt (the scene already skips them).

### 5.4 Rounded dashed border
`UiMesh.DashedRing(m, rect, radius, width, ink, dash, gap)` — walks along the perimeter of `RoundedContour`. Locked cards, sockets, trays and tier badges use it (`border: 1px dashed`, dash about 3·3).

### 5.5 Glow and shadow
- `UiMesh.Glow(m, rect, radius, spread, color)`: a ring whose alpha falls off outward. 3 to 6 steps. For a shadow that spreads downward (`0 3px 6px`), move the centre down.
- `UiMesh.Inner(m, rect, radius, spread, color)`: inner glow. A 1px light on top (`inset 0 1px 0`) needs just 1 line.
- Fill a radial gradient with an ellipse fan (the way `SelectionGroundShadow` does it).
- Outer glows are clipped by `RectMask2D`. For elements inside the plate, glow clipped at the plate edge matches the design (the design also has `overflow:hidden`). Only ornaments that stick out of the plate go in the layer outside the mask (4.1).

### 5.6 Blur and filters
- Detail-panel backdrop picture: a 220×220 icon, `opacity .4`, `blur(18px) saturate(1.15) brightness(.6)`, radial mask (centre 30% opaque → 62% semi-transparent → transparent). Do it with **one UI shader**: base it on `Resources/GlobalHudFade.shader` (it supports stencil and `_ClipRect`) and merge in the `_Tone` (brightness and saturation) formula of `PuzzleHubTone.shader`. `PuzzleHubTone` has no stencil or clip, so it must not be used inside the scroll plate. The window creates the `Material` and destroys it on close (precedent: `PuzzleHubWindow`).
- `grayscale/brightness/opacity` of locked icons: use `_Tone` of the same shader, and opacity through `CanvasRenderer.SetAlpha` (no mesh rebuild). The values are `modifier.icon.*` and `card.icon.*` from 4.2.
- Icons are 1254² non-readable textures with no mipmaps. CPU blur and baked blur are impossible, and plan section 16 forbids touching the icons again. If blur cannot be made to work in the end, go with allowed approximation A2 (the unblurred picture).

### 5.7 Gradient text
`TextGradient : BaseMeshEffect` — gives different colours to the top vertices and the bottom vertices of each glyph rectangle (precedent: `DialogueReveal.Conceal` rewrites per-glyph vertex colours). Attach it before `Shadow` so the shadow stays black. The title uses `#fff3d6 → #9b743c` and the silver name uses `#ffffff → #9fb0bd` (allowed approximation A3).

### 5.8 Quality rules
- A new `Graphic` carries `[RequireComponent(typeof(CanvasRenderer))]` and must not throw when added with `AddComponent` without any setup (`GraphicComponentTests` checks every `Graphic` subclass).
- `OnPopulateMesh` does not allocate on any call (the 0-allocation layer of 4.5). Add a test that calls it through a delegate and checks `Is.Not.AllocatingGCMemory()` (precedent: `ForgePresentationAllocationTests`). The existing `UiMesh` helpers (`RoundedContour`, `Gradient`, `Brackets`) allocate arrays on every call. New functions reuse buffers.
- Compare deterministic shapes (hexagon, arc, ring) against a **mesh golden**, the `OnPopulateMesh` result saved as JSON (precedent: `forge-ring-baseline`).
- Snap a 1px hairline to `1 / canvas.scaleFactor` (precedent: `UiButtonFace`).
- Draw pictures in code. Do not make frames, masks or selection effects as images (`Docs/Implementation/Generated_Game_Art.md`). Only the 92 icons are images.

## 6. Look catalogue and tokens

`golden/styles.json` is **the reference for looks**. It has 428 looks, each with its list of CSS declarations and "where it was used".

- **One name determines a look.** The fields of a scene element (state, selected, hot, avail …) build the name, and the name determines the look. The generator fails if the same name yields different looks. So the implementation comes down to one "field → name → look" table (`SkillTreeD4Look`).
- Naming rules (`og` = `land` | `port`):

| Element | Name | State axes |
|---|---|---|
| Card | `card.box.<og>.<l\|o\|k>[.selected][.mod-focused][.hot]`, `card.icon.<og>.<state>`, `card.text.<og>`, `card.pill.port.<state>[.maxed]` | Invested/available/locked, selected, modifier focus, next rank possible (`hot`) |
| Rank chip (landscape) | `tag.card-rank.<state>[.maxed]` | |
| Modifier socket | `modifier.hit.<og>`, `modifier.face.<og>.<chosen\|available\|idle\|unlearned\|tier-locked>[.focus]`, `modifier.icon.<og>.<…>`, `tag.mod-level.any` | |
| Passive | `passive.node.<og>.<coin\|key>.<l\|o\|k>[.avail][.sel]`, `passive.medal.<og>.<coin\|key>.<state>[.lit][.avail][.sel]`, `passive.ring.<coin\|key>.<state>`, `passive.icon.<coin\|key>.<state>`, `passive.text.name.<state>`, `passive.text.sub.<state>[.key]`, `tag.passive-unlock.<state>[.key]`, `tag.passive-rank.<state>[.key]` | |
| Badge | `badge.box.<og>.<open\|next-locked\|locked>`, `badge.medal.<open\|closed>.<og>`, `badge.medal.inner.<open\|closed>.<og>.<large-numeral\|small-numeral>`, `badge.name.<og>.<state>`, `badge.gate.<og>.<state>`, `badge.wrap.<og>` | |
| Line | `line.<g\|o\|k\|s1\|s2\|s3>` + `.glow`, `arrowhead.<s1\|s2\|s3>` | |
| Band, tray | `band.passive-shelf.<plain\|key>`, `band.passive-heading.<plain\|key>`, `tray.<learned\|unlearned>` | Band `key` = the row whose `tier` is 4 (the key passive row). Tray `learned` = that active is Lv1 or higher (the scene's tray does not carry this value, so read it from the allocation) |
| Header | `header.title.<layout>`, `header.stats.<layout>`, `header.stat.<level\|spent\|left.zero\|left.hot>`, `header.close`, `tab.<idle\|selected>.<og>` | |
| Plate | `plate.<layout>`, `plate.scroll`, `plate.legend` | |
| Detail panel | `panel.<layout>.<active\|passive\|key>`, `panel.medal.<kind>.<layout>.<state>[.lit][.max3]`, `panel.medal.ring.<kind>.<state>`, `panel.medal.icon.<kind>.<layout>.<state>`, `panel.art.wrap`, `panel.art.<kind>.<layout>`, `panel.name/sub/level/stats/heading/pick/desc.*` | |
| Panel buttons and reason | `panel.btn.<sq\|bar>.refund.<on\|off>`, `panel.btn.<sq\|bar>.invest.<gold\|off>[.reason]`, `panel.why.<red\|grey>` | `sq` = the square buttons at the right of the header in the portrait layout (44pt), `bar` = the wide bottom buttons in landscape and PC. Invest is possible (`gold`) or not (`off`). `.reason` = the bar invest button is blocked and the blocked reason is its label. `panel.why` = the line under the `sq` buttons (`red` = because of the tier or the level, `grey` = any other reason) |
| Panel modifier rows | `panel.mod.row.<land\|port\|pc>.<chosen\|idle>[.focus]`, `panel.mod.socket.<land\|port\|pc>.<l\|o>`, `panel.mod.name.<og>.<chosen\|idle>`, `panel.mod.eff.<og>.<chosen\|idle>`, `panel.mod.effTo.<chosen\|idle>`, `panel.mod.right.<og>.<chosen\|idle>` | Chosen row, other row; `focus` = the modifier row that is selected now (a bright 2pt ring) |
| Panel passive block | `panel.unlock.<ok\|lock>`, `panel.unlock.bar`, `panel.effect.<og>`, `panel.effect.to`, `panel.linked.<og>`, `panel.link.<og>`, `panel.link.lv.<on\|off>`, `panel.link.icon` | `lock` = the hero level is too low (red text) |
| Bar | `bar.rank`, `bar.ghost`, `bar.tier` | The width comes from the element's `Bars`/`Bar` |

- **The looks inside the panel** are pointed to by name from `scene.panel.parts` (`buttons`, `reason`, `modifiers[]`, `passive`). The text and the on/off state come from the view model (`panel.buttons`, `panel.modifiers`, `panel.unlock` and so on). The fixed styles written into the template (the text colours of the stat rows, the divider, the headings and so on) are defined by lines 135 to 177 of the design `SkillWindow.dc.html` and by `anatomy/TEXT_STYLES.md`.
- **Name test.** `SkillTreeD4LookTests` checks that the `style` name of every element in the golden `layout/*.json` (and the names in `panel.parts`, `panel.styles` and `panel.medal`) equals the name that `SkillTreeD4Look` builds from the fields, and that the name exists in `styles.json`. The formulas that build the names are in the generator `golden/lib/scene.js`, in readable form. The names inside the detail panel are in `golden/panel_parts.json` for every selected node (3 classes × 48 nodes × 3 layouts, with two more states at level 60 so that an investable state is shown). The generator audit found that the name of every element is determined by the element's state fields and the layout kind alone; the only exceptions are the band and the tray in the table above (both come from data).
- **How to read one look.** Example: `card.box.land.l`.
  - `background: <bar layer>, linear-gradient(180deg,#27211a,#17130f 55%,#120f0c) padding-box, linear-gradient(180deg,#e2c07e,#8a6b3c 40%,#4e3e26 70%,#a5844e) border-box` + `border: 1px solid transparent` = **gradient border** (the `border-box` layer shows through the transparent border). Draw the fill and the border separately.
  - `box-shadow: 0 0 0 1px #000, 0 3px 6px rgba(0,0,0,.6), inset 0 1px 0 rgba(255,230,180,.14)` = outer black 1px, glow below, inner light on top.
- CSS → drawing mapping:

| CSS | How to draw |
|---|---|
| `linear-gradient(180deg, A, B 55%, C)` | `UiMesh.Gradient`/`RoundedBox` colour stops |
| `… padding-box, … border-box` + transparent border | Fill + `RoundedRing(byY)` (5.1) |
| `radial-gradient(circle at 50% 60%, c1, c2 70%)` | Ellipse fan (5.5) |
| `box-shadow: 0 0 0 1px #000` | A ring one layer larger |
| `box-shadow: 0 3px 6px …` / `inset …` | `Glow` / `Inner` (5.5) |
| `border: 1px dashed` | `DashedRing` (5.4) |
| `conic-gradient(…)` | `Arc` (5.2) |
| `clip-path: polygon(…)` | `Hex` (5.2) |
| `filter: grayscale/brightness`, `opacity` | `_Tone` shader, `SetAlpha` (5.6) |
| `text-shadow: 0 1px 0 #000` | One `Shadow`, offset `(0, −2)` (canvas units, half points) |
| `background-clip: text` | `TextGradient` (5.7) |
| `word-break: keep-all` | 7.4 |

- **Colour tokens.** There are 239 colour literals (`node golden/tools/css_features.js --colors`). If you write a hex in screen code, it is caught by the colour-literal check of `check_ui_contract` (it catches warm, dark and ivory hex values). **Put all of them in `UiTheme` as tokens.** Where the existing Obsidian & Gilt tokens (gold, obsidian) already cover a colour, use those, and add only the families that are newly needed: passive silver (`#b9c6d0 #6f8291 #3e4852 #4a5862 #24303a #1a2126`, text `#c9d5de #9fb0bd #66727c`, border `STEEL_RIM`), line colours (`g o k s1 s2 s3`), ember (the `#e07a32` family), blocked-reason red `#df927b`. Name the tokens by family (`D4Gold…`, `D4Steel…`, `D4Ember…`), but the hex values must be the same as in the design.
- **Shortening RGBA:** Keep colours with alpha (`rgba(0,0,0,.65)` and so on) as hex + alpha tokens. Do not multiply by the alpha of `Color`; include it in the token.

## 7. Text

### 7.1 Fonts (user decision D6)
The design uses Noto Serif KR (title, names, headings), Noto Sans KR (body text) and Cinzel (Roman numerals). The game uses the OS font (`UiFonts.Body`) and `UiFonts.Display` (Baskerville, Georgia, Times New Roman, Noto Serif; WebGL uses the body font). Bundling a new font increases the size and makes the look differ from other windows.
- **Default (recommended):** use the existing `UiFonts`. This is allowed approximation A1. Line breaks can change because of glyph width differences, so always do the measured check in 7.3.
- **Alternative:** bundle Noto Serif KR and Cinzel (a subset). The user must decide on size and consistency with other windows (plan section 17 D6).
- Code that creates fonts is forbidden outside `UiTheme.cs` (`check_ui_contract.py`).

### 7.2 Text styles
- The 129 text styles measured in the design are in `golden/anatomy/TEXT_STYLES.md` (layout, font, size, weight, line height, colour, sample text). Key rules:
  1. **Sizes are in design points** and are 2× on the canvas (3.1). Sizes in 0.5pt steps (10.5, 12.5, 14.5 …) become integers.
  2. Where the line height is fixed, such as `13px`, set the line spacing to that value (adjust `Text.lineSpacing` and measure the gap between two lines). `normal` is the font's default.
  3. The colour is the value in that row. `rgba(0, 0, 0, 0)` is a **gradient-filled text** (the title and the panel name), so the colour is recorded as transparent (5.7). The subheading "Modifiers" (`#e2c07e`) is a single colour.
  4. Letter spacing (`letter-spacing: .03em`) exists only on the passive headings. Ignore it (allowed approximation).
- Sizes by role (points): title 27 (portrait) / 24 (landscape) / 26 (PC), tabs 14/13/13, header numbers 12 (PC 12.5) and values 12 to 13 bold, card name 12.5→10.5 (portrait) / 11→9.5 (landscape, fitted), rank chip 10, unlock level 9, panel name 19 (portrait) / 16 (landscape, PC), panel subtitle 11.5/10.5, panel level 12.5/11, description 12/10.5, stat row 12.5/11, modifier name 13/11.5, effect 11/10, button 13, legend 10.5. Exact values are in TEXT_STYLES.md.

### 7.3 Card name fitting and measured check
- The scene's card `FontSize` (`FitFontSize`) was decided from the design's **estimate** of glyph widths. The port outputs it unchanged (verified by L1).
- The real widths of the game font differ. After drawing, measure the **longest word** with `Text.preferredWidth`, and if it exceeds the box width, shrink the font by 0.5pt at a time (down to `min − 1` at the lowest). If it still does not fit, break the line inside the word. **Do not clip the text.** This check runs in EditMode for Korean and English and for every card name (10 per class), verifying that every card stays inside its box.
- Card names break at words, take at most 2 lines and are vertically centred.

### 7.4 Line breaking (`keep-all`)
- Card names (`card.text.*`) and the description slot (`panel.desc.*`) are `word-break: keep-all`: **Korean breaks lines only at spaces**. The project has no keep-all precedent (section 4 of `research/ui_infrastructure.md`), and this document could not confirm whether the Unity legacy `Text` breaks Korean text only at spaces.
- During implementation, **the smoke test checks with the longest description sentence.** If a line breaks in the middle of a word, switch to `SkillTreeD4Wrap` (it wraps by word itself, using the glyph-width estimate `TextWidth`, and inserts `\n`).
- **Reserved height** of the description slot: portrait 32pt (2 lines × 16), landscape 28pt (2 lines × 14), PC 42pt (3 lines × 14). The panel does not move when the selection changes. The longest description is 43 Korean and 70 English characters, and the longest shape sentence is 43 Korean and 64 English characters (including the modifier name and " — "), and both fit inside this. The smoke test picks every active and modifier in turn (3 classes × 2 languages × 3 layouts) and checks for **no overflow**.
- In the description slot, the modifier name is **bold gold** (`#f2d9a4`) and runs on in the same paragraph: `<b><color=#f2d9a4>name — </color></b>sentence`. Check that partial bold works with `<b>` of the legacy `Text` (the project has no `<b>` precedent). If it does not, lay out two `Text`s as one line flow.

## 8. Detail panel

### 8.1 Structure
The detail panel is outside the scene (the scene gives rectangles only). The **view model** (`SkillTreeD4View.Panel`) gives the text and the enabled flags, and **`anatomy` gives the position and size**.

| Selection | `anatomy.panel` state | Contents |
|---|---|---|
| Active | `skillSelected`, `lockedUltimate` | Medallion (rank arc), name, subtitle "T2 Core · Active", level "Lv 5 / 5 · values Lv1 → Lv5", **description slot**, n stat rows, subheading "Modifiers" + hint, 3 modifier rows (2 for an ultimate), buttons |
| Modifier | `modifierSelected`, `skillWithInvestedModifierRows` | Same as above, but the description slot reads "modifier name — shape sentence", and the chosen modifier row is highlighted |
| Passive | `passiveWithoutLinks`, `passiveWithLinks` | Medallion (silver arc), name (silver), subtitle, level, unlock line (+ progress bar), effect box, **linked-skill chips** (with no link there is neither a chip area nor a note), buttons |
| Key passive | (same structure) | Gold hexagon medallion, gold name, subtitle "Key passive · pick one" |

### 8.2 How to lay it out
- It is a vertical flow (flex column). The inner padding and gaps come from `SkillTreeD4Frame` (`PanelPad`, `PanelGap`) and the `padding`/`gap` of the `panel.*` looks: portrait 16/5, landscape 12/4, PC 14/10.
- Child heights: header block (medallion size + 3 lines of name), description slot (reserved height), stat area (divider 1px + top margin 4 + rows × line height + row gap 3), subheading line, modifier rows (in portrait they share the remaining height with a minimum of 40; in landscape the fixed minimum is 36/40), button row 44.
- The **golden values for position and size are the matching state in `anatomy/OUTLINE.md`.** The rectangles you compute must land within ±1pt of the golden values (L4). Name the GameObjects of each state by the rules in 2.4 so that the tests can find them by name.
- The corner ornaments (`gx-pan-p`, `gx-pan-l`) stick out 6pt beyond the panel's top and left edges (outside the mask). The backdrop picture is clipped by the panel's corner radius (`overflow:hidden; border-radius:inherit`).

### 8.3 Text and rules
- Stat row: label on the left, "from → **to**" on the right. The arrow is `#8a6b3c`, from is dim, to is bright and bold (`SplitEffect` splits it). The effect sentence of a modifier row works the same way.
- Right side of a modifier row: not chosen `0/5`, chosen `3/5` (landscape) or `Selected 3/5` (portrait, pill). A chosen row has a gold border and an ember background.
- Passive unlock line: if unlocked, `Lv15 ✓` (silver); otherwise `Lv25 · you are Lv24` (red `#df927b`) + a red progress bar (`UnlockBar`%, 4pt high, rounded ends). The bar is a `#1c1512` track with a `#8a3e2a → #df927b` fill and a red glow. The bar is present when the level is insufficient and absent when it is sufficient.
- **Buttons:** if `Buttons.Layout == "header"` (portrait), put two 44pt square − / + buttons on the right of the panel header, with the blocked reason below them (red text `#df927b` when the cause is the tier or the level, grey `#8f877b` otherwise); if `"bottom"` (landscape, PC), put two buttons at the bottom of the panel: "Refund" (iron) / "Invest" (gold). When blocked, the button text changes to `InvestLabel` (the reason). A block caused by having no points has no reason text (the header number is already 0). A shine passes over an enabled invest button on a 7.8s cycle (section 11).
- The view model provides the blocked-reason text, order and `aria`. The window uses only those strings.

## 9. Header, tabs, legend, close

| Element | Spec (design points) |
|---|---|
| Title | Portrait (16,62) 27pt / landscape (62,10) 24pt / PC (24,12) 26pt, height 44, serif bold, gold gradient |
| Tabs | Height 44, corner radius 6. Portrait: (16,112), width 408 divided into 3 with gap 8, 14pt. Landscape and PC: width 76, gap 6, 13pt. **Selected tab**: ember radial gradient + gold border + bold `#fbe9c0` text + gold diamond 9×9 below (`bottom:-5`). **Class with investable points**: an 8×8 ember dot at 6,6 from the top right |
| Header numbers | Landscape (400,10, 442×44) gap 16, PC (560,12, 510×44), portrait (100,62, 270×44) gap 10. "Level 24", "Spent 24 / 60", "Points left ⓪". The points-left value is a pill (height 26, min width 26, radius 13): an orb that is dark at 0 (`header.stat.left.zero`) and an ember orb above 0 (`left.hot`), with the border glow pulsing on a 6.4s cycle |
| Close | 44 circle, gold border `#8a6b3c`, inner X 16×16 (line 2pt, rounded ends) |
| Legend | 24pt strip at the bottom of the plate (transparent → `#090807` at 62%), inner padding 10, item gap 12, text 10.5pt. Items: gold line 22×8 (`#e9c886` + glow) "Invested", brown line "Available", dashed line "Locked", silver arrow "Passive link" |
| Plate | Landscape (62,62) 548×357 r8 / portrait (8,164) 424×406 r10 / PC (24,64) 856×564 r8. Obsidian base, gold gradient border, inner darkness |

- Tab switch: when you change class, **the selection and allocation stay per class**. The scroll position is kept (clamped into range). The tree appears rising from 6pt below (0.26s).
- Close and Back: leave them to the Back handling of `ContentWindowHost`.
- The numbers come from `SkillTreeD4Scene.Header`, `Plate` and `SkillTreeD4Frame`. Do not write the numbers in the table above into code again (use the port's values).

## 10. Input and scrolling

- **Select**: `Session.Select` is called when a card, modifier socket, passive or a modifier row in the panel is **pressed and released** (a click). Press effects while held: cards and passives `scale(.97)`, modifier sockets `scale(.92)`, tabs and close `translateY(1px)` (in points). Keep the press effects regardless of the reduced-motion setting (the design's `prefers-reduced-motion` turns off only the `gx-*` effects).
- **Invest and refund buttons**: `Session.Invest()` / `Refund()`. When accepted, call `host.Commit` at once (2.3). Ignore clicks on a button whose `Buttons.InvestEnabled`/`RefundEnabled` is false (no sound, no shake). The blocked reason is always visible (8.3).
- **Class tabs**: `Session.Tab(i)`. Pressing the tab of the same hero does nothing.
- **Close and Back**: the host's Back handling (`ContentWindowHost`). Esc is the same.
- **Scrolling**: one axis only: horizontal for the landscape layout, vertical for the portrait layout. PC has no scrolling. Dragging on a card, socket or passive scrolls and **cancels the press** (`UiButton` has no drag logic, and the press is cancelled when `ScrollRect` takes over the drag). The drag threshold is the default.
- Long press, double tap, right click and mouse-over (hover) effects are **not** in the design. Keyboard and gamepad navigation are also out of scope (a non-goal).
- First selection: the design uses `designSelection` (Warrior Whirlwind, Ranger Piercing Shot, Mage Fireball). The real game uses the last chosen target, or else the first investable active (plan 9.5). When opened with "passive view", the first passive of that class that has links is selected and the vertical scroll goes to `initialScrollTop`.
- If the invest button is tapped rapidly, commit as many times as it was pressed. The last effect replaces the earlier ones (an effect on the same node restarts).
- The hit areas are exactly the scene's rectangles (card `Rect`, socket `Hit`, passive `Rect`). Whether a touch area falls below 44px is checked with the screen-size table in `SkillTreeD4UnitsTests` (precedent: "tap target ≥ 44px" in `MonsterCodexWindowTests`).

## 11. Effects

**When something moves** is decided by the port (`SkillTreeD4.Fx.cs`, verified by `events` in the golden files). The window only plays back the result.

- `SkillTreeD4Session.Fx` — the effect right after an invest or refund is accepted: `Action` (invest/refund), `Id` (target node `a:<active>:<modifier>` or `p:<passive>:-1`), `Max` (max rank), `Gate` (the tier newly opened by this invest, −1 if none), `IgnU/IgnM/IgnP` (what to ignite: the first rank of an active `IgnM = −2`, the first point of a modifier `IgnM = modifier number`, the first point of a passive `IgnP`).
- `SkillTreeD4Session.Overlays()` — effects on connector lines: `Ignite` (list of line indices, the lines the invest just lit), `Glint` (line index, delay in ms, reversed or not). On the same line, ignite comes before glint. **A glint is started by selection**; invest and refund do not restart it. A tab change and a selection clear `Fx`.

**How it looks** (keyframes from `fx.json`, times in seconds):

| Trigger | Target | Effect | Time · curve |
|---|---|---|---|
| Successful invest | Target node (card, socket face, passive) | Punch: size 0.94 → 1.06 (55%) → 1 (`gxPunch`) | 0.52 · cubic-bezier(.2,1.6,.4,1) |
| " | Overlay on the node | Light ring: size 0.3 → 1.55, opacity 1 → .55 (70%) → 0 (`gxBloom`) | 0.56 · cubic-bezier(.15,.8,.25,1) |
| " | " | Starburst with 12 rays: size 0.35 → 1.9, rotates 18°, opacity 1 → 0 (`gxStar`) | 0.6 · same as above |
| " | " | "+1": opacity 0 → 1 (25%) → 0, upward 4 → −4 (25%) → −16pt, size 0.6 → 1.15 → 1 (`gxNumUp`) | 0.75 · cubic-bezier(.2,.8,.3,1) |
| Max rank (`Max`) | " | Larger starburst (0.35 → 2.6, rotates 30°, `gxStarMax`), the text is "MAX" (11pt) | 0.8 |
| Invest, refund | The node's rank chip, pill and level chip | 1 → 1.32 (35%, brightness 1.7) → 0.95 (70%) → 1 (`gxPop`) | 0.38 · cubic-bezier(.3,1.6,.5,1) |
| Invest, refund | Header "Spent n/60", "Points left" | 1 → 1.12 (35%, brightness 1.6) → 1 (`gxTick`) | 0.38 · same as above |
| Invest, refund | Detail panel medallion (rank arc) | 1.18×, brightness 2.2 → 1 (`gxArc`) | 0.55 · cubic-bezier(.2,.9,.3,1.2) |
| A new tier opens through an invest (`Gate ≥ 0`) | That tier's badge | A border ring spreads 0 → 14pt and fades (opacity .95 → 0) and a 26pt glow fades out (`gxGate`); a sweep of light (width 40%) passes from the left (−120%) to the right (320%) (`gxSweep`, slant −20°) | 0.8 · ease-out |
| An invest lights a new line | `Ignite` lines | A white line (gold `#fff3d6`, silver `#f4f8fb`, width 4) is drawn along the line length from 0 → 1 (70%) and disappears (100%) (`gxIgnite`) | 0.65 · cubic-bezier(.3,.7,.3,1) |
| Successful refund | Target node | Saturation and brightness drain and come back (saturation .2, brightness .8 → original) while the node shakes left and right −2 → +2 → 0pt (`gxDrain`) | 0.34 · ease-out |
| " | Overlay | An ash ring is sucked inward: 1.7 → 0.35, opacity .9 → 0 (`gxImplode`) | 0.4 · ease-in |
| " | " | "−1" (`#c8bfae`): opacity 0 → 1 (25%) → 0, downward −6 → +10pt (`gxNumDown`) | 0.7 · ease-out |
| User selects | `Glint` lines | A white `#fffaf0` piece of width 4 and 14% of the line length passes along the line (the other way if reversed), with a per-line delay (below), **3 times** (`gxGlint`) | 1.3 · linear |
| " | Selected node | The selection border glow breathes between .35 ↔ 1, and after **3 round trips** it stays lit (`gxBreath`). Gold (active, key) `rgba(242,180,96,.55)` 14pt, silver (coin) `rgba(170,210,240,.5)` | 2.4 · ease-in-out |
| " | Detail panel header | Opacity .35 → 1, from 4 → 0pt below (`gxRise`) | 0.2 · ease-out |
| Class tab change | Tree content | Opacity 0 → 1, from 6 → 0pt below (`gxTree`) | 0.26 · ease-out |
| Repeat | "Points left" orb (only when points remain) | The border glow pulses between .3 ↔ 1 **4 times** (every 1.6s) (`gxOrb`) | 6.4 · repeating |
| Repeat | Invest button (only when investable) | A white light bar (width 38%, slant −20°) passes from the left (−160%) to the right (360%) **3 times per cycle** (every 2.6s, about 0.9s each) (`gxShine`) | 7.8 · repeating |

- **Glint delays** (per line piece, in ms). `T` = the tier of the selected active (0 if a passive was selected): tier connector `tc` → (that tier number − 1) × 90, tier trunk `s` → tier number × 90 + 50, line to the card `c` → T × 90 + 200, trunk → socket `f` → T × 90 + 300, socket branch `m` → T × 90 + 360, passive link `l` and others → T × 90 + 300 (0 if a passive was selected). `LineOverlays` already does the computation (`SkillTreeD4Glint.DelayMs`).
- **Size and colour of node overlays** (in the design): card 64 (landscape) / 76 (portrait) pt, centred on the icon. Modifier socket 52 (landscape) / 58 (portrait). Passive node 58, medallion of the portrait capsule 60. Colours are gold `255,214,150` (active, key passive), silver `214,232,245` (coin passive), ash `160,151,138` (refund). The shapes of the light ring and the starburst are in the `.gx-fx*` rules of `fx.json`, as CSS verbatim.
- **Reduced motion**: when `StoryDialogueWindow.ReduceMotion` (the device setting `hellscript.reduce-motion` or the OS setting) is on, **turn off all the effects above** (the final state is shown at once). Keep the press effects. Also turn off the repeating effects (orb pulse, invest button shine). The fact that no settings screen uses this key (risk 9 in `research/ui_infrastructure.md`) is out of scope for this work.
- **Implementation frame**: one updater per window (`Update`, `Time.unscaledTime`) runs a pool of effect structs. There is no tween library. One effect is a state struct (target, start time, kind) and **must not allocate** (4.5). Do not restart an effect when redrawing with the same state (the design's A/B class alternation is a device for making CSS restart an animation, so in the game you simply replay the effect).
- Line effects use `PolylineGraphic.SetRange(t0, t1)` (5.3): ignite goes 0 → 1 → disappears, and for a glint the piece passes from −0.14 → 1 (the length of each segment normalised to 0 to 1).

## 12. Text and accessibility

- **All on-screen text comes from data**: the names, effects and descriptions of skills, modifiers and passives come from `SkillTreeD4.json` (Korean/English pairs), and screen text goes from `strings.ko/en` into a new section of `en.txt` (`# D4 skill tree`) (plan section 10). Do not write Korean literals in code (`LocalizationTests` requires an `en.txt` entry for every Korean literal inside `Runtime`).
- Text keys the port uses: title, level, spent, points left, 3 class names, condition ("{n}+ pts spent"), "Unlocked", "Locked", "Selected", invest and refund (long and short forms), active, passive, key passive, passive heading and key heading, "Requires Lv{n}", "Unlocked Lv{n}", "you are Lv{level}", 4 legend items, "values Lv1 → Lv{n}", blocked reasons (`max tier base swap cap key`; the English `tier` has singular and plural forms), and **`KeyPrefix`** (the `"핵심 · "` / "Key · " of the portrait key passive row), which exists only in the port and must be added as new.
- Convert **composed text** into the placeholders of `Loc.F` (`{0}`). The design's `{n}` and `{level}` names are what the port's `Fill` uses. A template that starts with a hole (`"{n}pt 더 필요"`) can swallow stored log sentences (`Loc.StoredText`). If `StoredLocalizationTests` fails, attach fixed text at the start and end. Tell the user about changed wording (plan section 10).
- If English is missing, Korean is shown instead and the miss is recorded in `Loc.Missing`. Never put an empty string or a key name on screen.
- **Accessibility text** (`CardLabel.Aria` and so on, for example "Whirlwind · 5/5", "Rapid Whirl · Selected 3/5", "Blood Recovery · Unlocked Lv15 · 0/5") is used as the element's name and description, and the tests read it. It is the same text used as `aria-label` in the design.
- The English `open` string is kept as in the design, "Unlocked Lv15" (in the accessibility text of a locked passive it reads as if it were already unlocked). Carry it over as it is until the design text changes, and ask the user first if you want to change it.
- Id substitution next to skill names: the regex in `ClassSkillTree.Display` catches `A06` of `A06-2` first. The new window uses the names in the data directly, so it does not go through this function. Fix it before using modifier ids elsewhere (quick edit) (section B of `research/dependents.md`).

## 13. Work packages (UI-1 to UI-6)

Split **step 4 (skill window)** of plan section 14 into six small PRs. Go in order and run only the relevant checks for each PR.

| Package | Contents | Main files | Done when |
|---|---|---|---|
| **UI-1** Screen calculation core and golden tests | On top of the model and rules that step 1 has already moved in, move the layout, view model, session and effect signals of `golden/csharp` (`Layout`, `ViewModel`, `Session`, `Fx`) to `Presentation/SkillTreeD4/`. Move the test helpers (`GoldenJson`, `GoldenShapes`) to `Tests/Editor/` and create **4 EditMode tests** (layout, view model, interaction, units) (the rules test is in step 1). `SkillTreeD4Units` and the test vectors of 3.1. | `SkillTreeD4.*.cs`, `SkillTreeD4Units.cs`, `SkillTreeD4*Tests.cs` | **55,803 checks pass in EditMode** (together with the 32,208 rule checks of step 1). Read the golden files from `Application.dataPath/../Docs/Design/D4SkillTree/golden/…` and do not copy them. If the data loading of step 1 (`SkillTreeD4.Data.cs`) fills the same model as the design export does, it must pass the same golden checks. |
| **UI-2** Drawing tools | Section 5: `UiMesh` extensions, `PolylineGraphic`, `RankArcGraphic`, `TextGradient`, blur and tone shader, `UiTheme` colour tokens. | `UiButtonFace.cs` (`UiMesh`), `SkillTreeD4Graphics.cs`, `Resources/*.shader`, `UiTheme.cs` | `GraphicComponentTests`, mesh golden tests and allocation tests pass. For each tool, a screenshot with a **sample plate** (hexagon, arcs of 5 and 3 segments, gradient border, dashed line, glow, 6 line states) drawn on one sheet. |
| **UI-3** Window shell and static scene | Sections 3 and 4: adapter window, units, hierarchy, per-element views, scrolling, `InMemorySkillTreeD4Host` (design sample allocation: every hero at level 24), `ADAPTERS` registration and docs, naming rules (2.4). Only a placeholder where the panel goes. | `SkillTreeD4Window*.cs`, `check_ui_contract.py`, `Shared_UI_Contract(.en).md` | 24 screenshots of the screens shown statically in **the same states as the 24 reference renders** (3 classes × landscape, portrait, portrait passive view, PC × 2 languages). The per-element scores of `compare_screens.py --scene` are within the pass limit. The widget part of `SkillTreeD4AnatomyTests` (cards, sockets, passives, badges, tabs, legend) within ±1pt. **Name test** (section 6): `SkillTreeD4LookTests` matches the golden `style` name of every element. |
| **UI-4** Detail panel and header | Sections 8 and 9: 6 panel states + key and locked, buttons, blocked reasons, description slot reservation, header, tabs, legend, close, text (section 7: size, fitting, line breaking, gradient). | `SkillTreeD4Window.Panel.cs`, `.Header.cs`, `SkillTreeD4Wrap.cs` (if needed) | The 6 panel states of `SkillTreeD4AnatomyTests` within ±1pt. The name test also checks `panel.parts` and the panel names. **Overflow smoke test**: pick every active and modifier (10 + 28 per class) in turn across 3 classes × 2 languages × 5 screen sizes, with 0 overflow outside the panel and description lines ≤ 2 (3 on PC). Every card name is inside its box (7.3). |
| **UI-5** Input, commit, effects | Sections 10 and 11: select, invest, refund, tab, `host.Commit`, key-comparison updates (4.4), scroll retention, an `FxPlayer` that plays `SkillTreeD4Fx`, reduced motion. | `SkillTreeD4Window.Input.cs`, `.Fx.cs` | **Click replay smoke test**: replay the 60 sessions of `interaction/*.json` with **real clicks**, and the screen state at every step equals the golden state (Korean and English). The effect signals equal the golden `events`. **Allocation test** (4.5): the effect update, frames without a change and mesh drawing allocate 0, and the click transaction frame does not rebuild the hierarchy and writes its allocation into the report. The state persists after saving and relaunching (`-hellscriptSkillTreeResume`). |
| **UI-6** Wiring and finishing | The 4 entry points of plan 9.1, `visibleState` of `StoreViewBinding.Attach`, parent window refresh (3.3), opening behind the `SkillTreeD4.Enabled` flag, the final L5 comparison table. | `GameUI.*`, `HuntEdictWindow*.cs`, `RiftEntryWindow.cs` | The PR has a **table comparing all captures** with the 24 reference renders (section 14). The old window still works as before (before step 7). `check_ui_contract.py --verify` and `check_ui_refresh.py` pass. |

- Each package starts on a `codex/d4-skill-tree-ui-<n>` branch, not a `claude/` one (plan section 14), and uses English commit messages and English code comments.
- **Do not**: rewrite the rules in window code. Do not recompute scene rectangles in window code (L1 breaks). Do not do the PC zoom with `localScale`. Do not write the design's hex in code. Do not use a stencil `Mask` per icon.

## 14. Visual comparison procedure and pass limits (L5)

The 24 reference renders are `reference/<Class>-<Layout>-<KO|EN>[-Passive].jpg` (portrait 880×1912, landscape 1912×880, PC 2880×1620, all 2× the design frame).

**1. Build the same state.** The renders are in the design sample state (level 24, sample allocation, default selection per class; "passive view" selects a linked passive). The smoke test builds the same state with `InMemorySkillTreeD4Host.DesignSample()` (`SkillTreeD4Alloc.Sample`, every hero at level 24).

**2. Capture exactly at the frame.** Set the window size to an integer multiple of the design frame: portrait 880×1912, landscape 1912×880, PC 2880×1620 (all 2×). Give the safe area with `UiSafeArea.Simulate(left, bottom, right, top)` using the design's values at the same scale: portrait (0, 68, 0, 124), landscape (124, 42, 124, 0), none for PC. Then `P = 2` and the capture is 1:1 with the design frame. Write it to `Temp/D4Shots/<Class>-<layout>-<language>.png` with `Capture(name)` (`ScreenCapture.CaptureScreenshotAsTexture` → PNG). On a Mac development build a window larger than the screen can be clipped (`resizableWindow: 0`), so use the largest integer multiple that fits the screen (including 1×) and match `--scale`.

**3. Compare.**
```bash
python3 Docs/Design/D4SkillTree/golden/tools/compare_screens.py \
  --ref Docs/Design/D4SkillTree/reference/Warrior-Landscape-KO.jpg --game Temp/D4Shots/Warrior-landscape-ko.png \
  --scene Docs/Design/D4SkillTree/golden/layout/Warrior-landscape.json --scale 2 --out Temp/D4Compare/Warrior-landscape-ko
```
For PC use `--scale 2.5` (frame 1.25× × 2×). The tool uses only PIL and numpy. Output: `report.json`, `diff.png` (heat map), `side_by_side.png` (reference | capture | difference), exit code (0 for a pass).

| Measure | Meaning | Pass limit | Where to look when it fails |
|---|---|---|---|
| `colour` | Mean colour difference of the picture blurred at 4pt (0 to 255) | ≤ 6.0 | Overall tone, strength of glow and light |
| `edges` | Whether borders and shapes are in place (correlation) | ≥ 0.70 | Position and size, roundness |
| `worst` | Mean of the worst-mismatched 2% of cells in the picture blurred at 8pt | ≤ 32 | Areas that are wrong as a whole |
| `element` | The worst difference inside the named rectangles of cards, sockets, passives, badges, trays and shelves | ≤ 20 | **Which element's look is wrong** (the name and position are printed) |

The pass limits were calibrated by comparing the reference renders with each other (`python3 tools/compare_screens.py --selftest`): Korean ↔ English (only the text differs) passes, a 1pt shift and noise pass, and **a 3pt shift, a 12% brightness change, a different class or one missing card fail**. Because the fonts differ (allowed approximation A1), the scores of text-heavy elements (cards, panel) can be a little higher. In that case, check by eye and write it in the table with the reason, such as "A1". **Catching a missing small element is not this tool's job** (L1 and L4 catch it). This tool is for the **look** (colour, gradient, border, glow).

**4. Look by eye.** Open `side_by_side.png` and check the following one by one: ① position and size ② tint ③ border gradient (the light-to-dark variation of the gold border) ④ glow and shadow ⑤ darkened icons (opacity .78 / .45, saturation) ⑥ text line breaking and clipping.

**5. Attach the table to the PR.** For each of the 24: the four measured values, pass or fail, and for a fail the reason (allowed-approximation number or what to fix). Do not merge while anything to fix remains.

## 15. Known traps

1. **Unit confusion**: design points (golden values, tests) ≠ canvas units (points × 2) ≠ screen pixels (points × P). Tests always speak in points.
2. **Legacy `Text`**: the font size is an integer, the line height is a multiple of the font's (`lineSpacing`), there is no letter spacing, and there is no precedent for Korean `keep-all` (section 7).
3. **Masks**: `RectMask2D` clips glows and ornaments that stick out of the plate (4.1). Put the ornaments in the layer outside the mask.
4. **PC zoom**: scaling with `localScale` blurs the text. Multiply coordinates, sizes and font sizes.
5. **Draw order**: lines below cards, tags above everything. Lines in one mesh in scene order (base glow → main line).
6. **Colour space of glows**: the project uses the Linear colour space, so alpha glows can look different from the design. Match it with one global correction constant (A7).
7. **Drag and click**: a card must not be pressed while scrolling. Decide the click on release.
8. **Icons with new ids**: `SkillIconAssets.TryResolve` does not know modifier ids (`W09-1` and so on). The new window either reads the icon path (`Art/ClassSkillIcons/<Class>/<iconId>`) directly or widens `TryResolve` to the `<base>-<n>` form.
9. **Overwriting the A06 and M07 icons**: they have the same file names as the existing icons. Check that the guid in `.meta` is unchanged and commit it together (this is step 3 work, but the UI tests assume the new pictures).
10. **Smoke test file names**: the UI contract check, the refresh check and `LocalizationTests` skip `Runtime*Smoke*.cs`. The new smoke test uses this name and is enabled with the `-hellscriptSkillTreeD4Smoke` flag. Move `ExistingTrees`/`OpenEdict` (v3 harness) of the old smoke tests to a shared fixture, then delete them (section F of `research/dependents.md`).
11. **Parent window**: the edict window's `HuntEdictEditSession` goes stale (3.3).
12. **`GetComponent … ?.` is forbidden**: use `TryGetComponent`.
13. **CI**: `shared-ui.yml` runs `check_ui_contract.py --verify`. It turns red if the adapter registration or the colour tokens are missing.
14. **Performance**: using a stencil `Mask` for each of the 48 icons breaks batching. Clip with a rounded-rectangle graphic that has UVs (section 5, item 9). Moving the scroll content re-lays out the whole canvas — defer updates while scrolling.
15. **Screen refresh key**: `visibleState` must include the levels, allocations and language of the 3 heroes. If they are missing, values changed in another window are not reflected.

## 16. Items that need user confirmation (screen)

Work can proceed on the defaults. If the user decides differently, change it before that package.

| # | Question | Default |
|---|---|---|
| D6 | Fonts (7.1) | The existing `UiFonts`. Bundling Noto Serif KR and Cinzel needs the user to decide on size and consistency. |
| DU1 | Whether to introduce the detail-panel backdrop blur (A2) as a shader | Introduce it (one shader based on `GlobalHudFade.shader`). If rejected, go with the unblurred picture. |
| DU2 | Mouse-over effects and keyboard/gamepad navigation | Not done (not in the design). If needed, make them a separate task, starting with the design. |
| DU3 | When Korean line breaks fall in the middle of a word | Wrap by word manually with `SkillTreeD4Wrap` (7.4). |
| DU4 | The GC allocation limit of the click transaction frame (4.5) | Set no limit; the smoke test only writes the size. After the first measurement, decide with the user whether a limit is needed. |
