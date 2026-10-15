# Potion / Retreat casual skin — 2026-10-14

[한국어](README.md)

**Source frozen and handed to the single integration owner.** The user requested all screens be integrated before one coordinated validation pass, ending this screen's independent Unity/build plan. The [final integration handoff](FINAL_INTEGRATION_HANDOFF.md) supersedes earlier queue wording and contains commit order, assets, test filters and smoke entry points. Unity compilation, EditMode execution and player validation remain pending in the shared integration workspace. This is not visual acceptance or merge readiness.

## Source and scope

Reference: https://claude.ai/artifact/Pejw6sDmiP48Aa3ewfneYx. The user reported three artboards; **3/3 were actually read in supported Chrome**: Korean three-state board, English three-state board, and developer specification. Both language boards contain Potion, Retreat OFF and Retreat ON. The specification was read as text/source; no separate specification screenshot was captured. Tabs, toggle and paired slider clamping were exercised. Prototype Apply/Cancel have no save handlers and are not persistence evidence.

Initial baseline: `2e193c336c6176d80b9ddbdff7269246bf2590fb`. Current approved common followup baseline: `0ccba80eef0da4703cb7ffcd1fb8e22848002b3d`, imported with merge `1cd07c5d` while preserving initial screen `8ad1bc64` and first API connection `35f89372`. The [shared API handoff](../Casual_UI_Parallel_Handoff_20261014.md) was read and followed. Branch: `codex/casual-potion-quick-skin`. Owned changes are `GameUI.PotionQuick.cs`, two `PotionQuick*` skin classes, dedicated resources, checks and this evidence folder. `Resources/UI.meta` creates a previously absent parent folder; the integration owner must select one parent GUID if another screen also creates it. The approved common commit was imported; no shared file was directly edited.

The original repository and skill workspace were not written. This is a source-only sparse local clone, sharing Git objects with the skill workspace. **Do not launch Unity in this incomplete sparse checkout.** Integrate into the parent's complete workspace for sequential validation. No Library/Builds/Temp cache was copied. If the reference must be reopened, first confirm Chrome profile `dakrdong@gmail.com` using supported UI. No new browser tab was opened after that instruction.

## Before / after (source inspection only)

| Area | Before | Implemented |
|---|---|---|
| Panels | Existing dark theme and straight borders | Cream panel, navy rounded borders/hard shadows/stitching, white potion/retreat cards |
| Buttons | Existing UiButton chrome | Same UiButton input with green Apply, white Cancel, gold selected tab; disabled, hover, pressed and keyboard focus states |
| Fonts | Existing font and synthetic bold | Noto Sans KR 400 body, real 700 emphasis; Jua headings/buttons/percentages; existing sizes and text rectangles |
| Art | Shared previous HUD potion PNGs | Dedicated approved potion-hp / potion-mp mapping |
| Vitals | HUD sprites and rectangular threshold zone | Image-compatible current-value gradient under the same rounded stencil as threshold shade/stripes |
| Persistence | Detached draft and atomic transaction | Unchanged clamping, save/failure, unlock, pause restoration and safe-area methods |

Panels use `StorageSurface.SetSkin(UiSkin.Casual)`; buttons use `UiSkin.Casual.ApplyChrome` and `UiButtonChrome.Casual`; gauge masks use `UiSkin.Casual.Clip`. The existing shared `UiButton.Face` remains active and owns input, selection and disabled feedback. The dedicated `PotionQuickButtonSkin` and its per-frame observer were removed. Explicit button labels refresh at construction/tab selection only, with the shared `LabelFont(text)` character fallback. Screen-specific rows, toggles and markers use the shared palette/fonts/meshes: `UiTheme.D4*`, unchanged 430-look catalog, `UiFonts.Casual*`, `SkillTreeD4ShapeGraphic`, `SkillTreeD4TextShadow` and `UiMesh`. No new material, shader or Canvas. `PotionQuickFill : Image` preserves the tutorial's `Fill`/`fillAmount` contract. The common clip under `Vital` preserves the original world rectangle and clips fill, shade and stripes together. GC and device performance are not measured yet.

The followup keeps every tab label Navy through selection changes and takes the shared Muted contrast adjustment. The existing tab test now checks that Choice alone preserves label contrast. Common Noto symbol improvements are inherited without local font copies or text substitutions. This popup does not construct edict sheet number controls, world previews, combat HUDs, charts or equipment details, so ConfigureEdictSkin, ConfigureCombatSkin, chart skin arguments and equipment providers do not apply. Calling ConfigureEdictSkin here would affect world previews outside this popup. Shared equipment art is in the merged history; no 359-PNG copy was materialized in this sparse checkout.

The orange Retreat heading and percentage on the white card use the common OnLight adjustment. Marker/gauge colours, gold title and white values remain unchanged. Darker orange text is an intentional visual difference from the prototype under the common contrast rule.

Potions are not equipment catalog entries, so `WithEquipmentArt(...)` does not apply; the approved dedicated mapping remains. Approved source PNG bytes are unchanged, totaling **1,261,081 bytes**. Import settings are single full-rect sprite, centered pivot, source alpha, no mipmaps, max 256. [Complete provenance](approved-art.json):

- `potion-hp` → `UI/PotionQuickCasual/potion-hp`, SHA `29086f1948be1c607e75634f6980383fa9ad5e6c1522fef880565bba26269264`.
- `potion-mp` → `UI/PotionQuickCasual/potion-mp`, SHA `6b6ce69a1b34dc309afe32d3c1d622746a7dec68f2e5b4524c4f8fccb2600fcc`.

Both are pass-approved in the original asset index and director status. No new generation or image transformation was needed. Old PNG/meta remain because shared HUD/PotionArt still reference them.

## Evidence

- Shared UI source/regression suite: **15/15 passed**, [receipt](ui-contract.json), [log](ui-contract.log).
- Save refresh ownership: **16 bindings guarded; 192 UI source files inventoried**, [inventory](ui-refresh-inventory.json). This is not runtime reachability evidence.
- Screen source/art preflight: **7 checks passed**, [receipt](source-art-preflight.json). Existing control rectangles, label strings/sizes, clamping/reflow/commit methods, palette tokens and approved SHA are preserved.
- Updated bundled-font cmap inspection: **0 missing characters**, 113 screen-owned KO/EN body characters for Noto 400/700 and 46 heading/tab/button/percentage characters for Jua. The receipt records font hashes and exact strings. Checkmark uses Noto 700; dynamic resource names still need runtime verification. [Receipt](font-coverage.json). This does not prove Unity wrapping or clipping.
- `git diff --check` passed.
- [Calculated contrast](contrast-review.json): Retreat text/white card 2.5179 to 4.8368:1; Muted/cream 4.5004:1; selected Navy/Gold tab 8.7943:1. Opaque solid-colour calculation only, not rendered accessibility certification.
- **Roslyn source compilation passed:** Runtime 835 sources including all three new common files, 0 errors/47 warnings; dedicated test assembly 0 errors/0 warnings. Used the installed .NET 10.0.401 compiler and read-only existing Unity reference DLLs. Runtime warnings are in existing sources outside this screen change. No Unity or player process was launched. [Input/output hashes and commands](static-compilation.json), [runtime log](runtime-compile.log), [test compile log](tests-compile.log). This is not Unity import/compilation or test execution.
- **Authored and source-compiled, execution pending:** `Hellscript.Tests.PotionQuickCasualSkinTests`, 14 cases covering 0/1/30/99/100% fill, rounded mask, input/keyboard/disabled state, geometry/font roles and approved imports.
- **Not run:** Unity compilation, `HuntEdictQuickMenuTests`, `HuntEdictProgressionTests`, `StoreViewBindingTests`, player smoke. No existing test was removed or weakened.

Reproduce static checks from the branch with:

```sh
python3 tools/check_ui_contract.py --verify --receipt Docs/Implementation/PotionQuickCasual/ui-contract.json
python3 tools/check_ui_refresh.py --inventory Docs/Implementation/PotionQuickCasual/ui-refresh-inventory.json
python3 tools/check_potion_quick_casual.py --receipt Docs/Implementation/PotionQuickCasual/source-art-preflight.json
```

## Remaining differences and validation queue

The reference is 560×560 with 46-high tabs, 56-high buttons and 24-point title. The game's maximum width 420, both-tab height 410, tabs 40, buttons 52, title 19 and HP-only height 232 remain unchanged. Existing text sizes were not reduced. The new socket paint is 38×46 around the unchanged 28×36 icon, versus the reference's 46×46 socket. The protruding selected-tab diamond is omitted because the retained tab/hint geometry lacks its gap. Repeating shine, blur, radial highlights and exact three-stop gradients are not pixel-identical. Common button chrome uses a maximum radius of 10 and keeps face/shadow inside the hit rectangle, differing from the initial dedicated 12/14-radius skin and the reference. Original control geometry and font sizes remain unchanged.

Gameplay contracts take precedence over conflicting prototype samples: potion HP 1–100/resource 0–100 instead of 5–95; retreat 1–99/rejoin 2–100 instead of 5–90/10–100. OFF retains “low HP alone does not trigger retreat” because separate lethal-damage reactions still exist. Town shows full bars and resource names without fabricated combat values. Locked features do not create a new empty menu. The card has no internal scroll.

The integration owner should include this scope in the combined validation pass; this screen no longer has an independent execution plan:

1. Run the 14 dedicated EditMode cases plus the three existing related classes above. Preserve and resolve failures.
2. Run existing `-hellscriptPotionActionsSmoke` silently with isolated save/evidence paths. It covers KO/EN pointer drag, toggle, fixed OFF/ON card, cancellation, failed save/retry, atomic save/reload, autosave draft stability, rotation and unlock combinations. Include the tutorial's town Fill contract.
3. Existing smoke sizes are 440×956, 956×440, 1600×900, 1600×1000 and 2100×900. Capture **2868×1320** KO/EN Potion/OFF/ON separately; the shared smoke size list was not edited.
4. Inspect 0/1/99/100% gauge ends, threshold stripes, rounded corners, potion edges, English clipping, pointer targets and save errors. Record the existing safe-area simulation assumption (left 24, bottom 18, right 32, top 20). Mac resolution simulation is not physical iOS testing.

There are **no game before/after captures yet**. The actual EN reference screenshot is saved in Library as **`libfile_cc29b785e56c819186d44c670a72af7b`**, file `file_00000000ae4881f5ba38b5477dbf6ca8`, version 1, 141,277 bytes. Record new Library IDs when real game captures are available.

Suggested shared-rule follow-ups: inventory legacy smoke contracts such as `Fill : Image`; use a common rounded clip for current vital and threshold shade; include active shared Face, explicit label and clip contracts in each screen's checks; clarify the existing Shared_UI_Contract distinction between real HUD values/input contracts and dedicated screen art. No shared document was edited here.

No remote push, main merge, web build, public share, install or permission expansion. Retain source, Git object dependency and evidence through parent integration/validation; review retention on 2026-10-28. This is not an automatic deletion date.
