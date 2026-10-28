# Per-skill hunt edict B1 — development validation window (2026-10-24)

> **2026-10-28 A2 correction:** References to the large edict window, ≡ menu and their dedicated components or smokes are historical. Use the [current entry points](../../Assets/HELLSCRIPT/Runtime/Presentation/GameUI.HuntEdict.cs) and [replacement screens and source owners](Hunt_Edict_Window_Removal_20261028.en.md). Earlier captures and test counts describe the earlier implementation.

[한국어](Skill_Tactics_Window_B1_20261024.md). **Player entry stays OFF; combat integration is not implemented.** This implements original U-E §9.4/9.5's B1 UI and connection to the existing B0/B2 save contract. The remaining 27 parents, 81 modifiers and per-class combat work remain the existing B3 follow-ups.

## Reference and entry

The parent reported verification of the original Part A/B artifact and approval context through supported Claude UI. This follows the user's removal of in-game class tabs and request for a title icon that adds no scrolling. No new aesthetic decision or repeated approval request is introduced. The older bottom entry row is superseded by the approved **44px title medallion** contract. The original HTML prototype remains untouched at its E worktree path and original branches.

`SkillTreeD4Window.OpenDevelopmentTactics(parent, store, catalog, previewOrient, closed, initialSlot)` is the explicit development/test entry. Ordinary `SkillTreeD4Window.Open` receives no such capability, and no GameUI caller was changed. The new screen is available only under `UNITY_EDITOR || DEVELOPMENT_BUILD`; `SkillTacticsCatalog.PlayerEntryEnabled=false` remains unchanged. Pass an isolated validation `GameStore`; protected user-test projects and saves are not validation inputs.

There are no class tabs. Node selection retains the original detail behavior; points and modifier equipment remain owned by the tree. Passives, missing cards and missing modifiers do not get an empty editor or entry. Unlearned skills, unselected modifiers and unsupported saved values show the existing session's read-only reason. Off still allows entry and editing of other values. The entry badge represents saved On/Off or preview `i`, and its accessible name includes the saved summary.

## Shared window and data

The entry was generated with `new_content_ui.py SkillTactics` and uses `ContentWindowView.Open(fit: ...)` plus `Adopt`. The shared shell owns canvases, safe areas, input order, Back and nested pause leases. The subpage matches the existing D4 frame's position and scale: portrait stacks information and options; landscape/PC places information left and options right. Header, Back/Close, dirty indication and save/undo/status remain fixed.

Colors, fonts, buttons and panels reuse `UiSkin.Casual`, `UiFonts`, `UiButton`, `StorageSurface` and `UiWindowFrame`. Skills use `SkillIconView`. The approved 512px medallion was copied byte-for-byte to `Resources/Art/GlobalHUD/Casual/skill-edict-medallion.png`, with the existing single-sprite 512px importer settings and fresh GUIDs. Original art and `ResourceTextureBudget` are unchanged.

| Control | Contract |
| --- | --- |
| Toggle | One parent `auto` owner; Off retains and permits other edits |
| Short choice | Two/three buttons with a non-color `✓` indication |
| Long/many choices | Current-value row opens a shared picker; text heights are measured |
| Number | Existing decimal normalization, steps and disabled endpoints; unit/range/default hint |
| Skill reference | Actual icon/name and learned/equipped/automatic/modifier restrictions |
| Conditional/replacement/read-only | Current `Visible` rows and modifier only; unused reasons, hidden-value preservation and global restrictions |

No functional choices, numbers or defaults were duplicated into UI switches. The same twenty catalog definitions feed the template.

| Representative | Visible rows |
| --- | --- |
| W02 Leap Slam / W02-3 Chain Leap | Parent 5 / modifier 7 |
| A03 Venom Trap / A03-3 Crossbow Turret | Parent 4 / replace three trap rows with two turret rows, yielding 3 |
| M04 Teleport / M04-3 Elemental Shield | Parent 5 / modifier 7 |

HET numbers and development state pickers stay absent. Modifier entry scrolls to its rows and distinguishes a previewed modifier. Global navigation in [SkillTreeD4Window.Tactics](../../Assets/HELLSCRIPT/Runtime/Presentation/SkillTreeD4Window.Tactics.cs) closes the skill window and calls `GameUI.ShowEdictTab("combat")` to open the Combat sheet. `SkillTacticsEditSession.RefreshEligibility` and the save-time recheck own the recommended-defaults lock.

## Save, return and lifetime

The independent `SkillTacticsDevelopmentHost`/`SkillTacticsEditSession` draft is reused. Save calls existing atomic `CommitSkillTactics`; saved summaries change after success. Failure preserves draft, last save and disk. Undo restores the last save. Unsupported preserved values remain opaque and read-only.

Back, whole-window Close and global navigation share Save and leave / Discard and leave / Keep editing. Failure or continued editing does not close the parent, commit slots or invoke its closed callback. A resolved whole-window close invokes the original parent `Close` once, committing the independent Part A slot draft separately. Direct parent destruction cancels the draft and releases owned subpages, pickers, the global editor and its tree without a write or close callback.

The current D4 adapter has fixed detail geometry, with no detail ScrollRect or user-adjustable splitter. This remains unchanged. Return retains class, selected/detail node, actual tree scroll and the existing frame split. `SkillTacticsReturnState` records actual detail scroll 0 and the current fixed split ratio. No new split control was invented.

Saving, undo, steps and choices update existing controls' text/state. No new save subscriber was added, so unchanged autosaves cannot rebuild the subpage. Locale/layout rebuilds retain draft, option offset and available existing input focus. Background shell rebuilds are deferred while a picker/global child is open. Concurrent changes to owned values are rejected by the existing stale check while retaining the draft; no automatic rebase occurs.

## Ownership and evidence status

Existing-file edits are limited to title space/icon calls in `SkillTreeD4Window.Panel.cs`, the approved first-line guard in `SkillTreeD4Window.cs.Close()`, and appended `en.txt` entries. Korean uses common `Loc.T` source keys; 28 English entries were added and seven existing keys reused. Core, catalog, combat, GameUI, store host, J/K, original art, packages and project settings are unchanged.

`SkillTacticsWindowTests` authors 22 cases: representative rows/replacement, 44px and unchanged panel/scroll geometry, ordinary-host entry OFF, Off/Undo/numeric/reference restrictions, preview/unsupported preservation, failed save/continued editing, independent slot save and single close, no-write parent destruction, existing global navigation, and logical KO/EN layouts at 440×956, 956×440, 1600×900, 1280×800 and 1680×720 with draft/scroll retention on rebuild.

This agent **did not run Unity, compilation, Edit/Play Mode, actual CLI review or a build**. Only shared Python UI checks and source/localization/meta/hash checks are performed here. The parent's single Unity owner must validate the final candidate's focused scenarios, actual rendering, long text, safe areas and input. No measured performance or final visual pass is claimed.

Base: `e1c1434a843a552c2c0d72a1100234ac9045c998`, containing main `1f1887d1148d99e5b5f42f9a899d8945c6209259`. Local evidence: `Builds/Validation/SkillTacticsB120261024/`. First focused filter: `Hellscript.Tests.SkillTacticsWindowTests`; related regressions: `SkillTacticsTests` and `SkillTreeD4SlotWindowTests`.

## Executed integration checks (2026-10-25)

After the original handoff, the single Unity owner compiled and ran the focused cases. TextGenerator uses Unity 6000.6's explicit IDisposable implementation. Owner lifetime now also unregisters the common window and cancels owned children when a development preview is destroyed outside Play Mode. The callback-count fixture resets per test. B1 passed22/22 in takeover/e-npc-combined/unit3/results.xml. Related tactics/tree/input/HUD/combat cases also passed within the394 ordinary integration cases in integrated-streaming-impact/run1. Two separately selected measurement cases stopped for missing output environment; that failure is retained and the configured measurement paths are recorded separately.

Development-only RuntimeSkillTacticsB1Acceptance requires fresh empty save/evidence siblings under /tmp/hellscript-tactics-b1-*, explicit offline QA and a disabled server override. It labels real-window/Store/raycast synthetic flows separately from the final external-pointer Save. Rendered/pointer acceptance is still pending; successful driver compilation is not visual acceptance.
