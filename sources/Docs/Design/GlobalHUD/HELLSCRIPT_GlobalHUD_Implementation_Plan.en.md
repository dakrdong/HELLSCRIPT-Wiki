# Observation HUD implementation plan

Date: 2026-09-14

Current stage: deliver resources and planning, then wait. Every implementation step below is future work.

## Project truth and ownership

The local project uses Unity 6000.6.0f1, uGUI 2.6.0 and Input System 1.20.0. Extend the existing uGUI, localization and display-settings structure without adding a framework or package. `GameController.Awake()` creates `GameUI`; its `Initialize()` owns the main Canvas at sorting order 100. `Base()` deletes children of `root` and `shell`, so the persistent HUD must live outside those deletion scopes under existing ownership, not a new global singleton.

`GameUI.BattleLayout.cs` owns the current battle HUD and `BattleViewport`; replace duplicate HUD elements and remove HUD-based camera-height reservation while retaining selected aspect and safe-area behavior. `UiSafeArea` and display settings remain authoritative. `GameUI.Effects.cs` supplies effect inspection and its pause policy; `GameUI.Growth.cs` and `Economy` supply growth context. `RunState`, `HeroSave` and `BuildConfig` remain state owners; the HUD model only reads.

Existing preset/settings/idle canvases use orders such as 110/120/140. Choose HUD order after examining the whole hierarchy rather than putting it above every modal. A parallel project change is introducing a central overlay map: coordinate world → map → lower-side HUD → detail/modal layering. The HUD does not change the map's visibility setting. Open details have input priority; read-only HUD graphics do not intercept map input.

## Prerequisites

| Decision | Recommended direction | Restriction until resolved |
|---|---|---|
| Three passives to one | Migrate unlocks, presets, recommendations and saves together; require explicit reselection or a documented migration rather than silently discarding equipment. | Do not enable the new HUD on live character state. |
| One unlimited potion to three stocked types | Define inventory, automatic triggers, effects, supply and legacy migration first. | Do not invent finite stock for the current unlimited HP potion. |
| Resource names | Confirm actual class-specific names in both languages. | Do not rename every resource MP. |
| Cooldown totals and effect IDs | Provide cast-time effective totals and stable identifiers from authoritative state. | Do not substitute UI guesses for simulation facts. |

These decisions affect combat, growth and saved state. Review concrete migration policies and trial values after the user authorizes implementation. This preparation changes no economy or saves.

## Proposed structure and data flow

Keep an independent `GlobalHudRoot` under existing controller/UI ownership, outside `Base()` clearing. Its safe-area root contains seal/level, vitals/shield, status `ScrollRect` with `RectMask2D` and pooled entries, an unmasked expand control, passive/four actives, three potions and XP ticks. Details retain the existing modal owner.

Proposed `GlobalHudSnapshot` carries character/session identity, vitals, skills/potions, level/XP, effects and validity. Proposed `GameUI.GlobalHud` manages lifetime and refresh, with pure conversion logic separate from rendering. These are planned names, not existing symbols. Use one simulation clock for speed and pause. Refresh numbers/lists only when needed, interpolate visual fills between snapshots, and recompute layout on actual size/safe-area/expansion changes. Disable graphic raycasts except for inspection controls.

## Delivery stages

| Stage | Work | Exit evidence |
|---|---|---|
| A. Data contracts | Resolve passive/potion/save migration and timing/effect IDs. | Documented old-save, preset and automatic-use compatibility examples. |
| B. Import | Import selected PNGs into proposed `Assets/HELLSCRIPT/Resources/Art/GlobalHUD/v1`; reuse the existing atlas. | Editor checks of alpha, masks, PPU, slicing, borders and filtering. Editor-generated `.meta` and stable GUIDs. |
| C. Layout | Build both orientations with read-only fixture data. | All eight functions, 120%/75% landscape ratios and unobstructed scenery. |
| D. Binding | Adapt hero/combat/town/training/result data. | Correct values, session swaps, event cleanup and page persistence. |
| E. Effects | Implement 4.5/8 slots, scrolling, +/−, descriptions, expiration and text-compatible alpha fading. | 0/4/5/8/12/40 entries and all boundary/input states without loss. |
| F. Existing UI | Remove duplicated battle HUD and reserved lower viewport; coordinate map/modals. | Selected aspect preserved, full scenery, readable hazards/map/HUD. |
| G. Validate | Run bilingual, Editor/runtime/device checks and publish records. | Matrix below, with untested devices explicitly identified. |

Create any required prefab or scene references through the Editor rather than rewriting scene serialization by hand. Commit only this task's changes per stage and synchronize the public wiki.

## Validation matrix

- Edit Mode: ID mapping, zero cooldown, rounding, XP denominator/level transitions, exact viewport widths, scroll anchoring/clamping and fade widths.
- Play Mode: town/combat/bag/settings/effects/results, hero switching/restart, pause/speed, training copies, repeated entry without duplicate HUD or subscriptions.
- Visual: 16:9, 20:9, 9:16, tablet, small windows, notches, bright/dark terrain, text scaling, central map on/off and modal input.
- Alpha: artwork/frame/timer/stacks fade together; controls stay clear. Do not default to full-HUD RenderTexture compositing every frame.
- Performance: compare frame time, UI rebuilds, allocations and memory on the same scene and actual target device. No improvement claims without measurements.
- Build: existing macOS development-build runtime smoke plus mobile readability and drag behavior.
- Documentation: wiki build/check, Python/Node checks, full read-only public mirror deployment and logged-out verification.

Stop after resource files, bilingual specifications, manifests and the plan. Do not start C#, shaders, prefabs, scenes or save migration. Resume at stage A only on the user's next implementation instruction.

한국어: [구현 계획](HELLSCRIPT_GlobalHUD_Implementation_Plan.md).
