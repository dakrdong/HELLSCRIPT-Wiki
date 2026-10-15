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
