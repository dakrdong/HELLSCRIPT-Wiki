# HELLSCRIPT UI refresh stability audit

Verified: 2026-10-03

## Findings and evidence level

The investigation starts from `main` **14735105**. The three-second `GameController.Update` autosave calls `Save`, which publishes `GameStore.Committed("save")` and reaches `StoreViewBinding.LateUpdate`. Of 11 bindings, only the forge and rune board had local state guards; the other 9 recreated controls even when displayed data had not changed. Expanding the four shop entries and two attendance tracks gives 13 affected windows/tabs. This Unity code is shared by web and native players.

Settings runs `UpdateCommonPanel → RefreshScreenSettings → UiTheme.Choice → UiButton.Configure` every frame with identical role/selection values. Previously `UiButtonFace.Show` also reset transition progress for unchanged state. A newly created control whose availability changed before its first mesh could begin with an obsolete palette. Identical configuration now preserves progress, and the first mesh starts in the final palette. Real hover, press, selection and availability transitions remain.

These are **control replacement and transition causes established through source and EditMode object/state checks**. This report does not claim that the user's original web pixels, actual before/after players, or full CPU/GC cost were verified. The new opt-in runtime harness is authored and compiled against both modified source and actual 14735105 source, but has not been executed.

## Changes

- `StoreViewBinding` compares the displayed dependency key to the last rendered key. Unchanged commits leave controls intact; a plain Save that changes displayed data still refreshes. It does not discard every `save` operation.
- Meaningful refresh waits while a mouse/touch/button is held, an InputField is editing, a dropdown is expanded, or a background CanvasGroup blocks input. Commits coalesce. Existing modal gates and inventory/storage drag gates remain.
- When meaningful refresh replaces a control, its named path and keyboard/pointer focus modality are restored if the replacement exists and is interactable. Focus acquired by another window is preserved. Path segments support control names containing `/`.
- Explicit action/tab/layout/locale repaint records the rendered baseline, preventing a second LateUpdate repaint. Existing scroll restoration remains.
- Domain transactions, rewards, save implementation, receipts and final protection checks are unchanged. Display keys never authorize a transaction or replace live game state.

## Page, tab and popup checklist

Every row below received **static refresh-path investigation**. **Actual screen runtime has not run**. [UI_Refresh_Audit.json](UI_Refresh_Audit.json) records locations for 153 UI source files, 222 creation/tab/dialog candidates and 11 bindings, with refresh/activation/destruction markers. These are candidates, not a count of unique reachable screens: declarations, dynamic titles and compatibility paths are included. This is not an all-screen runtime pass table.

Paths below are relative to `Runtime/Presentation/`; a wildcard includes the owner's partial files.

| Page/tabs and nested popups | Owning source | Refresh assessment |
| --- | --- | --- |
| Title; login, Google account, progress choice, server selection | `TitleScreenView*`, `GameUI.Title`, `.Accounts` | Size reflow and login-state polling; integrate separately with account owner's changes |
| Character selection and save recovery | `GameUI.CharacterSelection`, `GameUI.cs` | Explicit entry/state changes; no unconditional save repaint subscriber |
| Town/plaza/menu, NPC dialogue, services, content unlocks | `GameUI.Plaza`, `.TownServices`, `.RiftKeeper`, `.ContentUnlocks`, `NpcDialogueWindow`, `StoryDialogueWindow` | 0.15-second/Revision HUD refresh and service activation belong to the prior CPU work; untouched here |
| Battle, portal/escape confirmation, Rift map, enemy actions/effects, HUD popup | `GameUI.cs`, `.BattleLayout`, `.BattleEscape`, `.Rift`, `.Enemies`, `.Effects`, `.GlobalHud`, `GlobalHudView` | Intentional battle/map/value updates; slots change activation when actual data presence changes |
| Content dock, idle introduction/summary, equipment detail | `GameUI.ContentDock`, `.Idle`, `IdleEquipmentDetailWindow` | Intentional dock interpolation and idle updates; no save-driven full window recreation |
| Inventory; detail/comparison/stats, bulk/selected salvage, potion settings/picker, full wallet, filter/range help | `InventoryWindow*`, `GameUI.PlayInventory` | **Fixed**: equipment/build/protection references/slot levels and balances; drag/modal gate; wallet values retain existing in-place updates |
| Storage equipment/gems/runes; five warehouse tabs, unlock/expand/rename, detail, salvage/sale, presets, help/history | `StorageWindow*`, `GameUI.Storage` | **Fixed**: storage/capacity/name/balances/equipment/rune key; drag/modal gate, existing panel scroll restoration |
| Merchant Buy/Sell/Buyback; sale confirmation, automatic selection settings, reward comparison | `EquipmentShopWindow*`, `GameUI.EquipmentShop` | **Fixed**: equipment/protection/gold/shop state key; actual stock changes from the one-second check remain |
| Gamble Buy/Sell/Buyback, reveal and reward comparison | Shop `Gamble` adapter and `.Gamble` | **Fixed**: same dependency gate; normal reveal and post-transaction repaint remain |
| Forge option reroll/slot enhancement/equipment enhancement/core craft; auto reroll/result, equipment choice, line lock, help, core result | `BlacksmithWindow*` | Existing store signature and job-ID guards; 0.25-second timer text updates in place; real completion/notice/reroll repaint remains |
| Jeweler gem storage, potion craft, socket entry | `JewelerWindow`, `GameUI.Jeweler` | **Fixed**: gold/gems/capacity/potions/work availability/unlocks |
| Aspect stone equipment/library/confirmation, search/filter/help/equipment picker | `AspectStoneWindow`, `GameUI.AspectStone` | **Fixed**: equipment/aspects/unlocks/suspended-run presence; editing search defers replacement |
| Weekly/monthly attendance, daily quest entry, swipe and hide today | `AttendanceWindow`, `AttendanceSwipe`, `GameUI.Attendance` | **Fixed**: attendance/date/pending daily rewards/hero/free bag slots; real claim glow and midnight refresh remain |
| Five daily quests, claim and refresh | `DailyQuestWindow` | **Fixed**: quest state key; one-second countdown changes text, midnight refresh remains once per day |
| Reward boxes All/Equipment/Gems/Currency/Runes/Potions, paging/choice; receipt/equipment comparison/recent openings | `RewardBoxesWindow` | **Fixed**: owned boxes/receipts/suspended-run presence; unrelated equipment/gold changes excluded |
| Per-stage first-clear rewards, claim and box storage entry | `RewardBoxesWindow.ShowFirstClear` | **Fixed**: actual stage status/unlocks/reward preview/suspended-run presence; final domain validation remains |
| Offline supplies and reward reveal | `OfflineSuppliesWindow`, `RiftRewardRevealWindow` | Fixed reward snapshots; intentional appearance/reveal animation |
| Rift entry/stage, 1.5× confirmation, fatigue restore, potion/skill/ultimate swap, first rewards | `RiftEntryWindow*`, `GameUI.RiftKeeper` | Existing DisplayKey, popup and CanvasGroup gates; one-second fatigue/date text update |
| Rift result/loot/detail/comparison, repeat settings, retry countdown | `RiftVictoryWindow*`, `GameUI.RiftVictory`, `.Repeat` | Existing save exclusion, actual rewardStatus/repeat change and popup gate; intentional countdown |
| Training lobby hero/skills/enemies, add-enemy picker, live skill editing | `TrainingGroundWindow*`, `GameUI.TrainingGround` | Existing save exclusion; next-frame redraw for dropdown actions; non-save/nested runtime interaction remains unverified |
| Training result, previous result/DPS, skill/edict detail | `TrainingGroundResultWindow*` | Existing save exclusion and snapshots; actual non-save repaint runtime unverified |
| Hunt edict Overview/Skills/Combat/Survival/Loot/Bag/Explore/Repeat/Auto-equip/Presets-share | `HuntEdictWindow*`, `GameUI.HuntEdict` | Owned draft; no unconditional save subscription; size/locale reflow restores input/scroll |
| Edict numeric/set/order choices, skill actions/policy, preset names/import, save/revert/close confirmation | `.Summary`, `.Skills`, `.Presets`, `GameUI.Presets` | Explicit draft/size changes; intentional preview animation |
| Rune board weapon/effects/owned filters; information/help/legend/catalog, presets/store, regions/info/lock/slot, practice/close/revert | `RuneBoardWindow*`, `GameUI.RuneBoard` | Existing rune revision and dirty-draft guards; defer drag refresh; intentional camera overlays |
| Rune master reshape/upgrade, materials/drag, fusion/upgrade reveal | `RuneMasterWindow*`, `GameUI.RuneMaster` | Actual rune revision drives repaint; fusion/reveal animation remains |
| Combat records/log/DPS contribution graph, defeat analysis, window/tick/rule/archived edict | `CombatRecordsWindow`, `CombatLogWindow`, `RiftCombatGraphWindow`, `GameUI.History`, `.CombatJournal` | Fixed records/snapshots, explicit paging/filtering; no unconditional save repaint |
| A/B comparison conditions/B gear/originals/results/records/changes/apply/preset save | `GameUI.Comparison`, `.TrainingEquipment`, `.Presets` | Snapshot/draft and size reflow guards; transactions unchanged |
| Growth/stats/recommended behavior, first practice A/B, tutorial journal/support/completion | `GameUI.Growth`, `.Attributes`, `.FirstPlay`, `.TutorialPractice`, `TutorialJournalWindow`, `RiftRecommendationWindow` | Explicit actions/snapshots/progress; cue animations remain |
| Settings Screen/Sound/Combat/Help, aspect/language/hero/volume/display choices | `GameUI.ScreenSettings`, `.Audio`, `.SettingsFrame`, `SettingsControls` | **Shared button fix**: identical Configure/Show does not restart transition; value refresh/safe-area reflow remain |
| Remaining compatibility Base pages: build/rule-condition picker/decisions, item craft/collection, gem menu/detail/sockets, legacy bag/warehouse, rune fusion/drop rates/practice, edict sharing | `GameUI.cs`, `.Rules`, `.Items`, `.Gems`, `.InventoryLayout`, `.Runes`, `.EdictEditor`, `.EdictShare` | Indexed separately from wrappers routing into replacement windows; explicit repaint, runtime reachability remains to verify |

## Completed verification

- Isolated Unity **6000.6.0f1** focused EditMode fixture `Hellscript.Tests.StoreViewBindingTests`: **12 passed, 0 failed, 0 skipped**, fixture duration 0.804 seconds. Original user Editors, apps and worktrees were preserved.
- Covered unchanged Save/unrelated transaction control identity; meaningful gold-change Save and disk reload; held press/deferred update and exactly one click; coalesced commits while not ready; child CanvasGroup deferred update; unsaved InputField text; no duplicate manual repaint; keyboard/pointer focus restoration; failed save/duplicate receipt preserving state and grants; equipment key list/protection sensitivity; identical button configuration preserving transition progress.
- All Runtime C# compiled through Roslyn with existing Unity 6000.6/package references, zero errors and existing obsolete API warnings. This does not establish Editor import, IL2CPP, native or WebGL build success.
- UI contract check passed; UI contract tests 11/11 passed; refresh guard check passed for 11 bindings; whitespace diff check passed.
- [Focused result XML](UIRefreshEvidence20261003/focused-editmode.xml) and [scope/source hashes](UIRefreshEvidence20261003/validation-scope.json) are preserved with this change; detailed logs are in the handoff `evidence/` directory. These common regressions do not mean all 13 windows or the complete game were executed.

## Actual screen and cost handoff

`RuntimeUiRefreshSmoke` only activates by explicit flag in a development native player. It requires a new save directory and creates a synthetic account. Optional reflection permits adding **only this probe file to actual old source 14735105** and running the baseline observation route without the new binding API.

```sh
PLAYER -hellscriptSavePath ISOLATED_SAVE -hellscriptScreenshots EVIDENCE -hellscriptUiRefreshBaselineProbe -hellscriptUiRefreshSource 14735105 -screen-width 440 -screen-height 956
PLAYER -hellscriptSavePath OTHER_ISOLATED_SAVE -hellscriptScreenshots AFTER_EVIDENCE -hellscriptUiRefreshSmoke -hellscriptUiRefreshSource INTEGRATED_COMMIT -screen-width 440 -screen-height 956
```

Record actual commit/build GUID and matched viewport/language/save fixture for old, CPU-only and modified players. The old route observes actual replacement counts without asserting stability. The modified route asserts 13 windows/tabs × KO/EN: controller autosave after waiting three seconds, three trials of five unchanged saves, stable button identity, real displayed-value changes, held-pointer deferral/release, child-close deferred update and close/reentry; screenshots and JSON are produced.

Cost samples cover **synchronous Save + binding flush + Canvas update wall time, current-thread GC bytes and whole-process CPU time**. Frame waits/screenshots are outside that region; process CPU includes other threads. Do not interpret this as full-frame CPU/GC or browser cost. Optional `-hellscriptUiRefreshLegacy` only disables the key in the modified binary and is a synthetic control, not actual old-player evidence. No estimated improvement percentage is reported.

Remaining: actual KO/EN pixels at 440×956, 956×440, 1440×810, 1440×900 and 1680×720; hover/press/focus/scroll; repeated tabs/rapid clicks/close-reentry/external focus return; real transactions/rewards/save failure; settings/forge/runes/entry/results/training/tutorial flows; **actual browser idle refresh plus native comparison**; at least three matched actual-player CPU/GC runs; physical touch devices. The harness invokes mouse handlers, so actual OS/browser input also requires separate verification. The integrator runs the final full EditMode/native/web/wiki checks sequentially after all merges. Local full wiki build stops at an existing omitted art manifest link (`Docs/Art/ClassAspectIcons/manifest.json`) in this sparse source checkout; the four new pages render and their history is preserved. Full wiki check/test_wiki/test_wiki_ui were not run.

## Integration basis

Branch `codex/ui-refresh-stability` starts at **14735105**. Integrate with CPU branch `codex/cpu-ponytail-lite`: actual implementation **91aceb5f**, evidence **2873aa41**, latest guidance **ff300129**. Its implementation files do not overlap this change's Runtime files. Prior HUD duplication, service-button false/true toggles and covered-world rendering fixes were not copied into this branch.

The whole-tree 14735105..ff300129 comparison also differs in AttendanceWindow, ContentWindowView, EquipmentShopWindow and UiButton because other main UI work arrived after common ancestor **94e11019**. Do not overwrite current files with the older branch's layouts/attendance/unavailable-click policy. Combine account **86e4dd87** and other active attendance work in final main. This branch performs no main merge, push or public deployment.

Only newly found causes are added to [UI refresh recurrence rules](UI_Refresh_Stability_Rules.en.md); existing CPU guidance is not duplicated.
