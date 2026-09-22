# Blacksmith implementation validation

Validation date: 2026-09-22

[한국어](Blacksmith_Validation.md) · [Implementation, resources and migration](Blacksmith_Unity_Integration.en.md)

## Environment and evidence

Validation uses Unity 6000.6.0f1 Edit Mode tests and a macOS development player. Branch `codex/blacksmith-runtime` is an isolated checkout of the committed inventory foundation with `main` merged. The original running editor, uncommitted inventory work and existing saves are untouched.

The player uses a fixture account in a separate save directory. It raycasts actual UGUI button coordinates before dispatching pointer events, and also sends the E keyboard input. It checks real `GameStore` balances, upgrade levels, jobs and persisted state before and after interaction. Screenshots alone do not establish functional success.

The acceptance-only development flag temporarily keeps synthetic keyboard input enabled across macOS focus changes. Normal game launches retain their existing focus behavior.

## Results

- HTML behavior checks: **80 passed, 0 failed**.
- Focused Edit Mode suite covering blacksmith, resources, quality, attributes and localization: **178 passed, 0 failed/skipped**. Preserve the [XML](BlacksmithEvidence/blacksmith-focused-editmode.xml) and [summary/hash](BlacksmithEvidence/focused-summary.json). This overlaps the full suite and must not be added to its count.
- Integration Edit Mode suite after merging inventory `8a5c88d` and `main` `12e0a88`: **312 passed, 0 failed/skipped**, 99.25 seconds. Covers the forge, shared comparison/selection, inventory, shops, runes, graphics and town. [XML](BlacksmithEvidence/blacksmith-integration-editmode.xml) · [summary/hash](BlacksmithEvidence/integration-summary.json). Counts overlap earlier runs.
- macOS development build succeeded with 0 build errors. Native acceptance exited with `HELLSCRIPT_BLACKSMITH_SMOKE_OK`; see [results](BlacksmithEvidence/runtime-result.txt) and the [69-capture hash manifest](BlacksmithEvidence/runtime-manifest.json).
- Final native inventory integration acceptance also exited with `HELLSCRIPT_INVENTORY_SMOKE_OK`. It covers shared inventory/shop/storage comparisons, ring baselines, shared persisted salvage auto-selection and battle input gating in Korean/English portrait/landscape. [Results](BlacksmithEvidence/inventory-runtime-result.txt) · [retained capture hashes](BlacksmithEvidence/inventory-runtime-manifest.json) · [comparison](BlacksmithEvidence/inventory-20-shared-inventory-landscape-ko.png).
- All **30 transparent PNGs** passed alpha-channel, transparent-pixel, content and hash checks.

Full Edit Mode regression: **2,880 passed, 0 failed/skipped** in 1,518.18 seconds. Preserve the [full XML](BlacksmithEvidence/blacksmith-full-editmode.xml) and [summary](BlacksmithEvidence/full-summary.json). The full suite ran on the forge snapshot before the latest inventory merge. The subsequent boss-stage check and shared-comparison integration are covered by the latest 312-test run. Native macOS acceptance verifies language-switch retention and the final integrated UI.

The existing URP build configuration logs stripped Lens Flare/Panini post-processing shaders. Forge UI, input and persistence checks passed; this does not claim validation of those post-processing effects.

## Screen evidence

| Reference | Representative captures |
| --- | --- |
| 440×956 | [Affix list](BlacksmithEvidence/portrait-ko-tab0-list.png), [details](BlacksmithEvidence/portrait-ko-tab0.png), [slot growth](BlacksmithEvidence/portrait-ko-tab1.png), [equipment](BlacksmithEvidence/portrait-ko-tab2.png), [English slots](BlacksmithEvidence/portrait-en-tab1.png), [English equipment](BlacksmithEvidence/portrait-en-tab2.png) |
| 956×440 | [Enchanting](BlacksmithEvidence/landscape-ko-tab0.png), [slots](BlacksmithEvidence/landscape-ko-tab1.png), [equipment](BlacksmithEvidence/landscape-ko-tab2.png), [English roadmap](BlacksmithEvidence/landscape-en-growth.png) |
| PC 16:9 | [Slot growth](BlacksmithEvidence/pc-16x9-ko-tab1.png) |
| PC 16:10 | [English enchanting](BlacksmithEvidence/pc-16x10-en-tab0.png) |
| PC 21:9 | [Equipment enhancement](BlacksmithEvidence/pc-21x9-ko-tab2.png) |
| States | [Auto target match](BlacksmithEvidence/interaction-auto-match.png), [Lv.75 preview](BlacksmithEvidence/interaction-preview-75.png), [unlock tooltip](BlacksmithEvidence/interaction-unlock-tooltip.png), [pulse 1](BlacksmithEvidence/interaction-working-bright.png), [pulse 2](BlacksmithEvidence/interaction-working-pulse.png) |
| Fixed dialogs | [Affix candidates](BlacksmithEvidence/portrait-ko-candidates.png), [growth plan](BlacksmithEvidence/portrait-ko-growth.png) |

All 69 captures are included under `Artifacts/Blacksmith/RuntimeEvidence/` in the resource package. Representative captures above are also versioned in Git.

## Validation matrix

| Area | Checks | Evidence |
| --- | --- | --- |
| Quotes | Prices at levels 1–100; +10 sum; affordable maximum and remainder; +97→100; insufficient/stale quotes | `BlacksmithTests` |
| Transactions | Repeated request IDs, failed writes, currency overflow, unchanged rejected state | `BlacksmithTests` |
| Quality/migration | Legacy +5, constant gains after awakening/masterwork, affix lock and investment records, masterwork at +5 or higher | Blacksmith and quality tests |
| Enchanting | First paid slot lock, constant repeated price, actual candidates/ranges, auto match/pause/continue/stop/close | Core and player checks |
| Workstations | Per-character levels, two account-shared stations, sequential 10/50/250-diamond unlocks, duplicate rejection | `BlacksmithTests` |
| Time | Destination-level costs/durations, exactly-once offline settlement, 60 seconds costs 1; 59/1 seconds free | Core and player checks |
| Combat | Frozen run growth, next-run application, 70% elemental resistance cap, effective learned-active/equipped-passive levels | Core, skill and combat regression tests |
| Salvage | Ordinary legendary 10, unique-effect 15, set 10 in single/bulk/automatic paths; cores and persistence retained | Nine route/reward combinations |
| Rift/sweep | Stage-based stones, one-time pickup, duplicate sweep rejection | Blacksmith and resource tests |
| Access/layout | NPC range guard, interaction button and E key, three equal columns, portrait navigation, full-frame dialogs | Player checks |
| Details | Live preview values, three-second unlock tooltip, working pulse | Player state checks and captures |
| Locale/ratios | KO/EN, 440×956, 956×440, PC 16:9/16:10/21:9, shared inventory grade colors | Localization tests and captures |

## Reproduction

HTML: `node --test Prototypes/Blacksmith/*.test.cjs`.

Unity: use `-batchmode -nographics -runTests -testPlatform EditMode -testResults <xml>`. Add `-testFilter Hellscript.Tests.BlacksmithTests` for the focused suite.

Build the macOS development player through `Hellscript.Editor.ProjectBuilder.BuildMac` with `-hellscriptBuildOutput <app path>`. Run it with `-hellscriptBlacksmithSmoke -hellscriptSavePath <new fixture directory> -hellscriptScreenshots <evidence directory>`. It exits after acceptance; success requires `HELLSCRIPT_BLACKSMITH_SMOKE_OK` and `result.txt`. Omit these arguments for normal gameplay.

`Artifacts/Blacksmith/Play-Blacksmith.command` in the current worktree launches manual play using the separate fixture account, without sharing the original account's save directory.

Resources: run `python3 tools/blacksmith/package.py` to create `Artifacts/Blacksmith/HELLSCRIPT-Blacksmith-Resources.zip`, including per-file SHA-256 hashes. This package contains resources and integration references. Integrate the complete Git branch for the runnable game.

## Scope and publication

Mobile proportions are macOS windows. Physical iPhone/Android touch, keyboards, rotation, cutouts and performance require device validation. The project retains its local save adapter; no server-authoritative time or economy implementation is claimed.

Do not publish this feature branch. The `main` merger regenerates and validates the wiki from merged `main`, publishes the mirror and verifies GitHub Pages. Public destination: [HELLSCRIPT Wiki](https://dakrdong.github.io/HELLSCRIPT-Wiki/#/tree).
