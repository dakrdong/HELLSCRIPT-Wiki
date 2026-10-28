# Training ground casual skin candidate · 2026-10-14

> **2026-10-28 A2 correction:** References to the large edict window, ≡ menu and their dedicated components or smokes are historical. Use the [current entry points](../../Assets/HELLSCRIPT/Runtime/Presentation/GameUI.HuntEdict.cs) and [replacement screens and source owners](Hunt_Edict_Window_Removal_20261028.en.md). Earlier captures and test counts describe the earlier implementation.

[한국어](Casual_Training_Ground_Skin_20261014.md)

Current status: **source frozen; per-screen Unity/build plans closed; handed to combined integration validation**. 0ccba80e was inspected only and remains unapplied. The [final handoff](Casual_Training_Ground_Final_Handoff_20261014.md) supersedes previous queue wording below; these implementation/check records describe the cdb9 input.

Status: **common-API source candidate ready; chart/battle APIs, shared art integration and the parent-managed Unity slot are pending**. No native after capture or runtime pass is claimed.

## Baseline and scope

Reference: https://claude.ai/artifact/KZpgGkzK1YJ9yNzLex2san. User-reported32 boards; actual32 AX contents/frame dimensions inspected; only2 prototypes individually viewed and exercised (PC KO lobby and Portrait EN lobby). Their paths cover skill exclusion, boss filter/swap, start/pause/resume/first result, portrait scrolling and empty-roster disabled start. One empty-state capture failed blank while its AX proof succeeded; it remains labeled as failed. The artboard JSONs record counts and states.

Verified common game baseline is `4139a77e2df495cee009c07c6094b2eea9b1aeaa`; the source checkout branches from `2e193c336c6176d80b9ddbdff7269246bf2590fb` (handoff documents only) as `codex/casual-training-skin`. `task-12/training-skin` is a shared-object sparse source checkout, not a complete Unity validation project. The original checkout/index, skill branch, active monsters checkout, attendance pilot6378320b and other uncommitted art were preserved. No Library/Builds/Temp/Obj copies, Unity execution, remote push or merge occurred. Future browser work must first confirm the dakrdong@gmail.com Chrome profile; this implementation stage opened no new tab.

Approved common-owner commit `cdb9c140fbf8b778bd3c0bbc0fbcc68a5d9e3fa8` was cherry-picked without adding training-specific edits to shared files. Exact imported blobs are pinned in `common-api-inputs.json`.

## Before / candidate after

| Surface | Existing game | Candidate | Verification limit |
|---|---|---|---|
| Shell/panels | Dark angular Obsidian/Gilt | Three Open calls use `skin:UiSkin.Casual`; panels use `StorageSurface.SetSkin` | Original frame/safe-area/navigation/actions dimensions retained; native pixels pending |
| Typography | OS body and serif times | Noto400 body, explicit700/900 emphasis; Jua only window/hero titles | Size arguments retained; shrink calls removed; long EN layout remains pending |
| Buttons | Gilt face | Shared `UiButtonChrome.Casual` for normal/primary/chosen/hover/press/keyboard-focus/disabled states | Original UiButton, click/submit path and targetGraphic retained; no polling/input replacement |
| Checkbox/dropdowns | Gold and dark | Blue check and light menu template | Options, callbacks, value and template sizes retained; shared rounded clip wraps the existing viewport |
| Yard | Moonlit ruined practice yard | Sunlit castle courtyard with three dummies | Asset pixels reviewed; native crop/contrast not yet reviewed |
| Portraits | Registry-only paths leave N23/B06 empty | Explicit fallback for those2 IDs; full-UV contain and rounded stencil | Parent must include all35 shared resources with matching hashes |
| DPS/cards/battle HUD | Shared renderers | Existing shared calls retained | Shared owner must provide casual opt-in; not completed here |

`TrainingGroundSkin` now consumes `UiSkin` for screen-specific color/font/portrait/dropdown adaptation. The provisional shell painter and `TrainingGroundButtonSkin` observer/meta were removed. Shared window chrome and the original UiButtonFace directly own rendering and state. Only the constructor-owned button label receives Noto700; nested state/rarity labels retain meaning. The common closer remains Quiet + CasualClose + Noto ×. Start note keeps Navy and its initial disabled alpha.

The backdrop, translucent veil, portrait and dropdown use the supplied `UiSkin.Clip`. Hero and portrait image panels use shadow0 to align their visible inner border with the clip. The veil and checkbox keep their meaningful fills instead of receiving a blanket opaque SetSkin. This screen contains no equipment view, so `WithEquipmentArt(...)` is not required. No missing art-path/chart/battle APIs were invented. Shared files are exact approved commit blobs; training edits remain within owned files. Core, transaction, save and localization contracts are unchanged.

## Preserved behavior and reference differences

Stage/count ranges, five-kind limit, one boss/one unit, default ten normal enemies, skills/edicts, seed/time/record comparison, rewards/prices and save schema are unchanged. Result pause-lease behavior remains unchanged.

The reference16:9 hero/records column differs from the game's portrait-or-wide-21:9 branch (width900 and ratio2.2); the existing branch is retained. Tier radio buttons and count steppers in the reference remain existing game dropdowns. The game opens the shared HuntEdictWindow, not the reference's dedicated dialog. Existing summary+DPS and two-column damage cards remain instead of the reference's three-column/list composition. Text size was not reduced: enemy-role wrapping can increase the row's measured preferred height within its original width. Other fixed single-line EN slots still need native clipping measurements; this is not blanket width-expansion approval. The pre-existing plain-save notification exclusion remains and is recorded separately from the skin.

The common Casual Quiet role draws an opaque idle face. Keeping the existing skill-row click rectangle covers portions of the parent auto-off warning border (left/top/bottom); the warning text and right edge remain. This is an unresolved visual difference requiring the common owner’s Quiet decision and native captures. No replacement local renderer or smaller hit rectangle was added.

## Art provenance

The650-ID approved index and DIRECTOR_STATUS contained no matching yard backdrop. The assigned yard was generated once using builtin image_gen. The tool did not expose a model ID (`unknown`); pixel review and production approval remain distinct.

- Target: `Assets/HELLSCRIPT/Resources/Art/TrainingGround/training-yard.png`.
- New SHA256 `d1055549d233e1a451b8b3177b9582372e117d273f1b238017ce46899d4e4a4b`, 2,453,192B.
- Previous SHA256 `2d236b311f215543d560a9f6526087bddee753463327d9152b1fec32201332b4`.
- Unchanged meta SHA256 `1cfe71bef209557ac6cd9892a5dc61dfa5468a10df12fffc52ead239e1612221`; GUID preserved. Original bytes remain in the read-only repository/Git. No deletion.
- Mandatory reference warrior sheet, Style D title and existing yard hashes plus the exact prompt are in `training-yard-manifest.json` and `training-yard-prompt.txt`.
- Shared35 monsters are pinned to `57bba034dad1659e8a36dc9645b1291d2554e497`, canonical `Resources/Art/TrainingGround/Portraits/{ID}.png`. All35 blob hashes match the generation manifest, all1254×1254, total63,338,362B. Existing33 meta files were preserved and N23/B06 had new meta files. They were neither regenerated nor copied into this source checkout. Recorded status remains pixel-review-accepted / productionApproved=false. The active b732010a monsters checkout predates the corrections and must not be copied wholesale; see the conflict audit.

## Validation and handoff

The two earlier UI-contract/15-regression and UI-refresh (16 bindings /191 sources) runs remain historical evidence. The same focused static checks ran twice during the migration; the final round followed the Hero shadow/clip repair, with results and exact inputs in `common-api-static-checks.json` and `common-api-inputs.json`. These are not Unity or C# compilation results.

Updated `TrainingGroundSkinTests` supplies five cases for shared chrome and unchanged input/semantic children; dropdown callback/options/viewport/content/bounds and no duplicate clip; two full-UV contain cases; and registry immutability. **None has run yet.** Common-owner CasualUiSkinTests and existing TrainingGroundUiTests, TrainingGroundTests and StoreViewBindingTests have also not run on this input.

The precise queue is `validation-queue.json`: KO/EN,440×956,2868×1320, PC16:9/16:10/21:9, chosen/disabled/empty/failure, touch/click/slider/scroll/nested return and disposable-save failure/reopen. Use the parent-designated project/output, muted audio and an isolated task save. If used, DPR3/102px horizontal/54px bottom insets are explicit Mac simulation assumptions, not physical iOS validation.

Library proof: reference sheet `libfile_aa6df6c145ac819195d450952cfb7724` contains five successful reference captures and scope labels, not a Unity after. Generated backdrop `libfile_4854daf17da88191918cb7e001eb3015`, version0, underlying file `file_00000000161c81f5a7ef35ce61c4c623`.

Shell/panel/button common API migration is complete. Remaining work is the common-owner Quiet warning-border decision and missing shared DPS/cards/battle HUD APIs; integrating identical shared35 PNG/meta blobs; and parent-scheduled compilation, focused tests and native before/after. These actions are pending, not denied or uncertain executions. The partial source checkout alone lacks N23/B06 texture bytes. Recommend documenting Quiet transparency and warning-border priority. Also recommend limiting Jua to tested titles and using Noto for numbers/punctuation; shared HUD resume ▶ requires a separate common font/icon decision. Font audit records the exact covered corpus.

Preserve code/art/evidence until parent integration; review retention2026-10-21. No builds, cache cleanup, permanent deletion, public/web deployment or main merge was performed.

The original `828fd7bb2be5b2fe1a2db8305e8742212cc97c4e` [source-review.md](TrainingGroundSkinEvidence20261014/source-review.md), `candidate-inputs.json` and `static-checks.json` describe the pre-migration candidate. Use `validation-queue.json` candidateCodeCommit and new `common-api-*` evidence for current inputs. Native verification remains pending.

Current source commit: `2dee3038016169b7ad7110cfec524d351c9d02dc`. Shared import: `d5b8c8898cb2bc140c4c736426b961704f57ac09` (exact owner commit above). [Current source review](TrainingGroundSkinEvidence20261014/common-api-diff-review.md). Both final static checks passed; native verification remains pending.
