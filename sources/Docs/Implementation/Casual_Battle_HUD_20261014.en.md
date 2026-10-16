# Casual base battle HUD — source handoff (2026-10-14)

[한국어](Casual_Battle_HUD_20261014.md) · [Reference](../Design/GlobalHUD/Casual/README.md) · [Shared handoff](Casual_UI_Parallel_Handoff_20261014.en.md)

This is a source/static-validation delivery of the base HUD and four independent normal-rift graph windows. No Unity, EditMode execution, player build, actual screen pass, or real save access is claimed. The integration owner will run the single final combined validation after all 15 screens are assembled.

## Base and ownership

Worktree: `task-22/battle-hud`, branch `codex/casual-battle-hud`, local shared Git objects and sparse sources. No concurrent original-checkout edits or Library/player copies.

Dependency order: existing 13 UI `8d356b3c` → documentation `529effd6`/`4148241a` → six hero slots `e3809624`/`93219e3f` → character creation/selection `b87b581b249c5e682cd7832fa9190b24c0dd9c4e` → shared HUD API **b0d4da6be02d9069bff04c4e81c4029caa677c13** → this HUD commit. Do not integrate earlier WIP snapshots. Recovery integration owner: `01a13a01-b651-7457-b0e9-aa4f6325a0e9`.

Dedicated additions: `CasualBattleHudLayout/Style`, `GameUI.CasualBattleHud/CasualDamageGraphs/CasualEdictMenu`, `GameController.BattleHud`, `CasualBattleHudTests`. Base HUD owners: `GlobalHudLayout/View/Snapshot`, `StatusStripView`, layout JSON. Adapter edits: `GameUI.BattleLayout/DamageGraphs/Rift/RiftActivity/TrainingGround/EdictSheet/EdictQuick/Enemies`; Enemies adds one dispatch line. Agreed shared hooks: Display calls `InitializeBattleHudPositions(directory)`; IdleDashboard passes class and gender to Portrait.

Shared SkillIconView/PotionArt/TrainingDpsChart/RiftActivityPanel/budget/localization files remain exactly at b0d4da6b. Protected `GameUI.EdictPosition/Target/Order`, `CasualEdictSkinScope`, `GameUI.PotionQuick` remain byte-identical. HeroSlots, migration, creation/selection, combat, rewards and currency were not reimplemented.

## Behavior and compatibility

The top-right edict opens downward; the minimap sits bottom-right; effects begin above the first potion. Portrait stacks effects, slots, seal/vitals and XP. The 44 reference-unit targets are scaled to fit, not certified as 44 physical pixels on every device. Saved class/gender chooses six approved seals. HP below 30% pulses a coral ring every 1.1 seconds; dead portraits turn gray.

Skill roles use `SetCasualHudFrame`; potions use `PotionArt.Bottle(...,casualHud:true)` with the complete 512px canvas rendered at 66% of the slot. No alpha recrop. Crafted-elixir art and tap/hold/swap/settings paths remain. DPS/activity retain their preference files; new files are `hellscript-monster-damage-position-v1.json` and `hellscript-boss-damage-position-v1.json`. Existing atomic `DpsHudPosition` preserves old files on failure. Folds remain session state, not account schema.

All four windows fold/drag independently. Existing DPS/incoming/activity calculations, 20/40% risk thresholds, 50% bar saturation, boss-alive gate and 60 cast-marker cap remain. The folded activity ratio uses the same percentages. Training/tutorial graph layout and input adapters stay intact. A scoped `BuildCasualCompatibleEdictPanel` adapter supplies a lower-screen layout anchor to existing portrait editors and restores the real top-right anchor in `finally`.

Jua display/numbers and Noto body use a single existing pooled `SkillTreeD4TextShadow` effect. No stacked Outline/Shadow components. Existing one-second DPS and 0.15-second activity refresh cadence remains; marker lookup uses a reused list. No CPU/GPU/GC measurement was performed. Shared b0d4da6b updates result-chart series colors while keeping result geometry separate from HUD-specific strokes/endpoints.

## Art and evidence

Actually inspected: all 14 local reference JPGs and pixel contact sheets of imported art. The designated `dakrdong@gmail.com` Chrome profile was verified, but the private online artifact timed out after its title loaded. **The online artboards were not seen.** No other profile was operated.

Runtime: `Assets/HELLSCRIPT/Resources/Art/GlobalHUD/Casual/`, 60 PNGs: six approved seals, three approved potion canvases, 51 prepared vector derivatives. Existing prepared files were reused; no follow-up regeneration/imagegen. Sources/recipe are in `Docs/Design/GlobalHUD/Casual/` and `Resources/v2-casual/` beneath GlobalHUD design docs. Do not regenerate to finish this handoff.

[Runtime hashes/GUIDs/borders](CasualBattleHudEvidence/runtime-art.json), [approved source hashes](CasualBattleHudEvidence/approved-art.json), [audit](CasualBattleHudEvidence/art-audit.json). PNG bytes: **2,846,922**. RGBA8 raw upper bound: **20,702,464 bytes**, not measured GPU usage. Shared texture budget: maximum 512, mip/readable off. All 91 original root PNGs and their 91 metadata files are unchanged. Old GUIDs, fallback resource strings and shared assets remain; zero deletions.

## Checks and integration entry points

- Roslyn with actual Unity references: Runtime 861, Editor 53, Tests 271 sources; zero errors. Existing warnings 90/28/43; changed-file warnings zero. Test assembly compilation is not EditMode execution.
- Production layout C# executed with pure value-type/math shims: five requested sizes, boss/menu combinations and 16 fold combinations; **320 cases / 38,480 boundary and overlap checks passed**. No font/input/rendering claim.
- UI contract 15/15, refresh guard 16 bindings, design-source smoke 38 variants, diff whitespace pass. [Compile](CasualBattleHudEvidence/compile-result.json), [geometry](CasualBattleHudEvidence/geometry-result.json), [receipt](CasualBattleHudEvidence/receipt.json).
- EditMode entry points: GlobalHudTests/GlobalHudResourceTests/CasualBattleHudTests/CasualHudSharedTests/CasualChartSkinTests/BuildResourceTests and existing DamageGraph/DpsHud/CombatActivity/HuntEdict/Potion tests.
- Reuse one final integrated player with existing `-hellscriptGlobalHudSmoke`, `-hellscriptHudScaleSmoke`, `-hellscriptBattleLayoutSmoke`, `-hellscriptDamageGraphSmoke`, `-hellscriptRiftDpsSmoke`, `-hellscriptDpsDragSmoke`, `-hellscriptRiftActivitySmoke`, `-hellscriptBattleQuickEditSmoke`; use each smoke's disposable-save/evidence arguments. Never use the user's real save.
- Remaining checks: ko/en, long boss labels/large DPS, six seals, insufficient ◆/locked/empty/dead/low-HP, full effects scrolling, four independent folds/drags/relaunch/failure, edict/potion editing/rotation, three map modes, observation/settings/idle/return and the other 13 screens. Physical input, mobile devices, screen readers and performance remain untested.

## Declared reference adjustments

Portrait slot faces are 44 rather than 42 design units; actual face bottoms including caption reservation are portrait147/compact37/desktop54. Compact boss damage uses x306/y96 in the middle lane because five rows cannot fit under both right-hand windows. Boss status and header shortcuts are separated; full compact action text stays in the observation menu.

Open edict tabs temporarily hide the content dock, narrow the portrait right graph lane and move the landscape/desktop map clear of the menu. Closing restores the dock; reflow does not overwrite saved anchors. New art uses a scoped Casual path; old assets stay until actual rendering/reference verification. CSS/Unity font pixel parity, existing boss/skill illustration consistency and actual readability remain final-review items.

After freeze, changes require coordination of ownership with integration. No push, main merge, web build, Unity/player execution or real save access. Task-owned source and static outputs remain through final review under task-22/artifact-lifecycle.json; no permanent cleanup was performed.

Accounting: total work/wait/retry duration and usage tokens were not measured across interruption/reconnection. The final 15-test static UI contract log reports 43.491 seconds. Unity full tests, player builds and wiki runs: zero.


## 2026-10-15 follow-up — portrait slot groups and Training header

The 2026-10-14 body above is preserved as its historical source handoff. This addendum records the user-requested **selected local HUD changes and narrowly scoped validation**. The working tree also contains a separate uncommitted art/provider integration unit; these observations are not a clean-HEAD-only integrated acceptance claim.

### Change scope

- Portrait slots use a maximum of **50 design units**, **4** within-group spacing and **16** between potion/skill groups. Remaining width is split between both outer margins; small viewports scale slots and spacing together. This does not guarantee 50 physical pixels. The actual 602×930 observation measured **48.64px** slots, **3.89px** inner spacing and **15.56px** between groups.
- The portrait content dock retains its existing **44-unit basis**. Separating that basis in `GameUI.ContentDock` prevents larger slots from enlarging dock buttons. Compact and ordinary landscape slot layout remain outside this portrait-slot change.
- The selected slot patch SHA256 is `656eb19d5d432166d7d0c92225705a8cf541fe68f5880fe6b23f4f3e1a12de60`. It includes `GlobalHudLayout`, only the first two layout/comment hunks in `GameUI.ContentDock`, and one expected-value line in `GlobalHudTests`. The ContentDock `Emblem`→`GlobalHudView.LoadArt` art/provider hunk stays with the separate integration unit. Application checking against 3 HEAD `b5b86c51` text snapshots passed; this is not compilation or runtime evidence.
- A separate one-file `GameUI.BattleLayout` change applies only to the **legacy noncompact landscape Training header**. It bounds the settings button above the fixed Edict and moves the dock top below Edict. Shared `dockIcon` is unchanged, preserving dock icon/rail, field-event and medallion/minimap size inputs. The ordinary CasualCombatHud early return, portrait and compact branches remain. Source SHA256: `623060ef503b4bf14940aeb4a04f5fccd8fa64b2a80b9fd5bbb16bcd1e13664e` → `78dd091d775caeaff1a52858c9b40071c1ab8f8b91677b1c50db55672b639334`; metadata was preserved.

### Actual slot observations

- **10 geometry conditions** covered Korean/English × 5 viewports: 602×930, 390×844, 320×568, 956×440 and 1600×900. EventSystem center hits on visible targets are distinct from physical-input testing.
- Actual callback paths covered **16** taps followed by detail close, **16** timed partial-hold exit/cancels, and **8** nonempty full holds followed by detail close. The remaining **8** empty/ultimate full-hold cases are **NOT_RUN**; no grant or equip action was added for testing.
- Root directly reviewed **9** earlier bottom-row frames and **1** new Korean 602×930 frame after the title flight ended naturally, and accepted the slot row. The earlier Korean 602×930 frame obscured by flight VFX remains preserved and excluded from visual acceptance. Existing art and the complete potion canvas at 66% remain; no all 144-art review is claimed.
- Original matrix and single-Korean-capture reports retain **FAIL** unchanged. The temporary GameView helper had two cleanup defects: custom-size removal index conversion and a strict full-token label check encountering Unity's automatic truncation. Root separately restored the original `Options review` **FixedResolution 2100×900**, **8** total presets and selected index **0**, with preference-file byte identity recorded. This does not relabel the original reports or classify those helper failures as product-row failures.

### Training header, shutdown and remaining validation

The Training header passed actual `GetWorldCorners` bounds, EventSystem raycasts and non-overlap for three controls in Korean at 1600×900. Opening the content dock verified **4** actual shortcuts for safe bounds, raycasts and no overlap with the eight bottom slots before closing it. The Edict button opened its quick root and a backdrop click closed it. Settings was raycast-tested only; the options panel was not opened. Root directly reviewed **2** closed/open-rail frames and accepted this header scope. Five shortcuts remain an arithmetic-only case; the actual capture had four.

The first header attempt ended before Training because the existing attendance `hide_until` expired at 15:00 UTC (Korean midnight), revealing a new-day popup; it produced 0 PNGs. That FAIL report remains preserved and counts as neither product-header failure nor pass. The follow-up helper used two public `SetHidden` calls on the exact owned disposable fixture to adjust display preferences only and asserted unchanged account JSON around that setup. No clock, attendance entitlement or reward was changed. Its raw `HEADER_SCOPED_PASS_VISUAL_REVIEW_PENDING` result remains unchanged, with root visual acceptance recorded separately.

The owned Editor **PID 53002 exited gracefully with exit 0 and confirmed PID absence**. Original GameView state (**8 presets / selected 0 / Options review 2100×900**) and preference-file byte identity were confirmed, and all **7 disposable fixture files were restored exactly**. The scoped helper source and metadata were moved to recoverable evidence storage after confirming no references. Real user-save access and permanent deletion were 0.

Full integrated **NUnit runs: 0; native builds: 0**. Ordinary quick-list/equip, ultimate grants/unlocks, potion consumption/auto-buy, other art galleries, physical mouse/touch and full-screen regression were not exercised by this scoped check. Slot callback results do not establish those behaviors. One final combined suite and one reused native build remain after approved scope freeze.

Scoped repository evidence: [receipt](CasualBattleHudEvidence/SlotGroups20261015/receipt.json), [Korean portrait row](CasualBattleHudEvidence/SlotGroups20261015/portrait-ko.png), and [Training header](CasualBattleHudEvidence/SlotGroups20261015/training-header-ko.png). The receipt pins raw reports, root acceptance and shutdown/restoration evidence by SHA and Task paths. Current-source UI contracts passed **15/15**, and refresh passed **14 bindings / 195 UI sources**; these do not replace full NUnit/native validation. Options PR 138 remains unadopted and unmerged pending user approval.


## 2026-10-15 final integration follow-up — assertion scope and portrait Training header

The preceding sections remain historical records of their execution scopes. The original integrated native `hud-battle-layout` job is **FAIL** because Settings, content and event sizes differed in Training at640×360. Its receipt and24 preceding PNGs remain unchanged. The documented legacy noncompact landscape layout deliberately reduced Settings alone while preserving content/event sizes, so the unconditional equal-size assertion had the wrong scope.

The post-build QA correction in `RuntimeBattleLayoutSmoke.CheckBattle` checks positive square targets, the Settings floor of44 page units and content-size ceiling, and equal content/event sizes in that legacy noncompact landscape branch. Other branches retain equal-size checks; the Casual44 reference, camera, text, safe-area and visibility checks remain. This QA correction changes no product layout and does not relabel the original native FAIL.

A subsequent focused Editor attempt passed40 deterministic ticks and one actual `CheckBattle` call at Training720×1280, then **FAILED** on overlapping content-toggle and fixed-Edict touch rectangles. Actual screen coordinates placed the content bottom at`1132.05762` and Edict top at`1132.71961`, an overlap of about **0.662px**. Both overlay Canvases were converted into the same screen-pixel coordinates. Separate successful center raycasts do not eliminate the shared edge area. This attempt and the earlier helper recovery failures remain preserved.

The product correction in `GameUI.BattleLayout` extends the existing landscape rule to portrait in the **nonCasual, noncompact** path: the content dock begins at least **6 page units** below the fixed Edict bottom. Only landscape retains the Settings-size clamp. Dock/event/Settings sizes, slot groups and art, `ContentDockView` fold/unfold spacing and rotation, combat and save calculations remain unchanged. Training-ground/tutorial screens using the same legacy portrait branch are also affected; the ordinary Casual early return and compact path remain unchanged.

Post-correction focused Editor validation is **NOT_RUN** in this preparation record. The plan uses disposable-save Training2 at720×1280,1280×720 and640×360, Korean/English actual target bounds and top EventSystem hits, dock/Edict open-close paths, and **120 identical ticks** against an unrendered control copy. Portrait coverage must also check every actual unfolded shortcut against the eight lower potion/skill slots. This actual path supplies regression coverage; no unit test merely copying the same formula is added. Completion, executed conditions and visual acceptance require a separate actual follow-up receipt under`task-23/validation/cleanup-20261015/main-canonical-resources/new-art/final-two-native-followup/`.

Neither the whole NUnit suite nor the38 native jobs will be repeated, and no additional native build is made. The one existing app and its hash remain evidence of **inputs before this product correction**. They cannot establish that the corrected product layout is present in that same app; acceptance of this source change is limited to the subsequent focused Editor checks. No real user-save, remote, physical-input, mobile-device or performance claim is added.


### Actual focused Editor result for the final integration follow-up

The **NOT_RUN** paragraph above records the preparation stage. The subsequent `training-evidence-4/result.json` completed with `accepted=true`, no error, **120 ticks / 5 actual CheckBattle calls / 20 top-hit header raycasts / 8 pointer actions / 9 PNGs**. The five executed conditions were **720×1280 Korean and English, 1280×720 Korean, and 640×360 Korean and English**. Each of the three sizes ran 40 identical explicit ticks, comparing the rendered simulation's full RunState and Hero JSON against the unrendered control. This is not a six-condition matrix including 1280×720 English.

Actual EventSystem input opened and closed the dock in portrait Korean/English and 640×360 English. The fixture had **four** real shortcuts; all passed safe-area, top-raycast, mutual-overlap and **active lower-slot** clearance checks. The 640×360 English condition also opened Edict's quick root and closed it through an unobscured backdrop input. This legacy fixture has an inactive ultimate slot, so the result does not establish input coverage for that inactive slot or a real five-shortcut rail. No positive-area overlap remained among the four header targets in any of the five executed conditions.

The helper removed its three temporary presets and restored the initial **nine presets, selected index 8, all labels/identities**, and language. Those nine include the parent's separately owned 1440×810 preset. Final restoration to the original eight presets, graceful Editor shutdown, disposable disk-fixture restoration and final visual acceptance require the parent's separate shutdown evidence; the coroutine result alone does not establish them.

The actual helper SHA256 is `9567c6db4955892be0b2b631c8680d37c6f44c19652eca5266a01379a62e2471`; result SHA256 is `61625ea065779db24dbb885969db91889490f85e784fc13dd806ab3f467e5510`. All nine actual PNGs match the result's exact names, dimensions and SHA values. The original native FAIL and prior failed helper/product attempts remain preserved. The actual status is `FOCUSED_EDITOR_TRAINING_PASS_VISUAL_REVIEW_PENDING_ORIGINAL_NATIVE_FAIL_PRESERVED`; it is not a repeated whole NUnit/native 38 pass or an additional build. The existing app still represents inputs before this product correction.

Root final acceptance is recorded in `final-two-native-followup/acceptance.json` (SHA256 `e321f6717ad600036ce41b0a2342b65ab80f76cd6b0538133e1b3b09e034fe65`). Root directly reviewed five representative PNGs. Portrait clearance changed from approximately0.662px overlap to5.333px separation. Button screen rectangles at both landscape sizes matched the preceding layout within0.001px. `restoration.json` confirms the original eight presets, selection0 and scene, all28 ProjectSettings hashes, the three disposable device seeds, and owned Editor PID33106 graceful exit0. The three invalid-preset-index errors from the Task-only diagnostic remain in the raw console and were resolved by exact owned-preset recovery; no console error followed product adoption. Real user-save access, permanent deletion and remote operations remained zero. This follow-up adds no performance or physical-mobile-input evidence. The earlier Options intake hold is historical: PR138 was subsequently integrated into local main after user approval.
