# Daily quests

Updated on: 2026-10-01

Following the user's 2026-10-01 decision, every account receives the same five missions with the same goals every day. Open the daily quest tab in the attendance/event window and manually claim each completed reward.

| Activity | Fixed daily goal | Abyssal Coins |
|---|---:|---:|
| Sell equipment | 30 items | 20 |
| Clear rifts | 20 victories | 40 |
| Enhance equipment | 2 completed steps | 15 |
| Spend gold at shops | 500G | 15 |
| Consume potions | 10 items | 10 |
| **Daily quest reward total** | | **100** |

Progression, hero switching, speed and recent victory counts/durations do not change these goals. Tier selection and duration-based adjustments were removed. Claims remain manual with no extra all-complete bonus. Attendance and first-clear rewards remain separate. The page shows progress, claimed state, today's claimed total and the time until 00:00 KST.

`Assets/HELLSCRIPT/Resources/Data/DailyQuests.json` version 2 contains one positive `target` and `reward` for each of five distinct activities, with rewards totaling exactly 100. Any future data tuning applies equally to every account.

A previously prepared version-1 day is converted to the fixed goals through the existing save transaction. The date, wallet, reward amounts and claims are preserved. Partial progress is retained up to the new target; already completed or claimed activities stay complete. The preparation transaction ID includes the catalog version so an old receipt cannot skip conversion. Opening the page, committing a real activity or claiming a reward applies the conversion. Write failures preserve the original state and permit retry. Conversion never grants currency.

Every day uses the same 30/20/2/500/10 goals. Play time, speed, equipment and history add neither a goal adjustment nor a waiting condition. Existing fatigue, sale/enhancement/shop prices, supply/potion consumption and unlock rules remain in use.

Successful events count at their domain completion:

- Equipment removed and credited through protected inventory, warehouse or automatic sale. Salvage and discard do not count.
- A normal winning rift after loot cleanup. Failed, abandoned, duplicate, tutorial and training runs do not count. Loading or resuming adds nothing.
- Actual completed equipment enhancement steps. A completed +10 operation counts ten steps. Stale quotes, retries, rerolls and slot enhancement do not count.
- Exact gold paid for equipment, buyback, town gear/gems/runes, unidentified gear or paid potion restock. Refunds from automatic sales do not reduce the counted price. Enhancement, reroll, crafting and free starter supplies do not count.
- Actual potion stock consumed in normal rifts. Failed inputs, cooldowns, missing stock, training and tutorials do not count.

Financial events update progress in the same existing `GameStore.Transact` write as their resources. Refusal or write failure rolls everything back. Combat consumption and victories follow existing account/run persistence. Progress stops at the target. Read-only account copies have no attached quest clock.

The day and reset reuse attendance's `AttendanceClock`, `Attendance.Day` and `RiftEntryRules.Midnight`. Current attendance uses the device UTC clock mapped to KST in the local development save adapter; this feature adds no new credentials or server-time authority. Server enforcement against device-clock manipulation remains the same separate operational boundary as attendance.

The next actual event, login or open-page refresh prepares the new day. Offline days grant no retroactive progress or currency. Moving the clock backward refuses progress/claims. Claims use a stable day+quest transaction ID, persisted claimed flag and 100-coin cap; old windows cannot claim tomorrow's rewards. Removing old receipts cannot bypass the claimed flag. Reload, restart and hero switching preserve wallet and claims.

Save schema20 adds an empty daily state to older saves without changing their wallet or equipment. Unsupported future or malformed states stop loading while preserving the source. Older clients reject schema20, preventing a downgrade from silently discarding claims and re-granting rewards. Reverting code alone does not downgrade new saves.

`DailyQuests` and `GameStore.DailyQuests` own state and transactions. The generated `DailyQuestWindow` uses the shared `ContentWindowView`, theme, fonts, tabs/buttons and window host, with the fixed body adapter described below. Korean and English are shipped together. `DailyQuestTests` covers catalog, identical goals across progression/speed/history, legacy-day conversion and preservation of completion/claims, success-only hooks, failures/retry, calendar, offline, persistence and duplicate protection. Development-only `RuntimeDailyQuestSmoke` checks actual domain transactions, claims/reset, KO/EN portrait/landscape/small screens, clipping, fixed controls, raycast clicks, unchanged body position after dragging and fresh-process persistence. Synthetic input is not physical mobile proof, and synthetic combat is not a human timing study. Final execution evidence is recorded below and in [optimization and integration validation](Optimization_20260930.en.md).

Acceptance before the fixed-goal correction (2026-09-30): focused170/170 (including27 daily cases), UI contract11/11, daily native initial28.7s and fresh restart4.8s passed. Twelve KO/EN viewport combinations, real raycast/drag, midnight (including preservation of both automatic attendance popup tracks), claims, empty write-failure state and persisted100 coins were verified. [Portrait KO](OptimizationEvidence20260930/daily-portrait-ko.png), [landscape EN](OptimizationEvidence20260930/daily-landscape-en.png), [small](OptimizationEvidence20260930/daily-small-en.png), [claimed](OptimizationEvidence20260930/daily-claimed.png), [empty state](OptimizationEvidence20260930/daily-empty.png).

## Final fixed-goal validation — 2026-10-01

Final game code and actual main integration are `a1edd320`. Its macOS development build succeeded with zero errors. [Daily focused 33/33](DailyQuestFixedEvidence20261001/fixed-daily-20261001-focused-editmode.xml) and [localization 33/33](DailyQuestFixedEvidence20261001/fixed-daily-20261001-localization-editmode.xml) passed.

The final full EditMode run completed **4,937 total / 4,890 passed / 47 existing failures / 0 skipped**. Failure names exactly match September 30's fresh 47-failure baseline, with zero new failures. UTC end time and individual failures are preserved in the [summary](DailyQuestFixedEvidence20261001/validation.json) and [raw XML](DailyQuestFixedEvidence20261001/fixed-daily-20261001-final-editmode.xml). The host-interrupted run left no result XML and was not counted as complete; a new run completed it.

All **12 smokes / 16 launches** passed on the same final player. Daily initial/restart took 31.9s/7.4s; attendance took 59.7s/7.4s. Only the unfinished attendance stage was resumed separately. Reading the isolated save after restart confirmed catalog 2, targets `30/20/2/500/10`, rewards `20/40/15/15/10`, all five claims and wallet 100; replay added zero coins. Coverage includes actual transactions, raycast input, body drag, midnight, rollback, write failure, twelve default-text KO/EN viewports and additional states. [Full native results](DailyQuestFixedEvidence20261001/native-results.json).

[Portrait KO](DailyQuestFixedEvidence20261001/daily-portrait-ko.png) · [landscape EN](DailyQuestFixedEvidence20261001/daily-landscape-en.png) · [small EN](DailyQuestFixedEvidence20261001/daily-small-en.png) · [claimed](DailyQuestFixedEvidence20261001/daily-claimed.png). The footer also describes identical daily goals. This is macOS synthetic-input validation, without physical-mobile or natural-play timing measurements. Later documentation/wiki changes leave this game source unchanged. Concurrent uncommitted forge edits in the original checkout are outside this run's scope.

## Fixed layout without scrolling — 2026-10-01

The approved [HTML preview](../../Prototypes/DailyQuests/HELLSCRIPT-DailyQuests.html) is adapted inside the existing `DailyQuestWindow`. Preview state/device controls are excluded. `ContentWindowView` and `ContentWindowHost` retain the title, close, attendance navigation, fixed actions, safe area and Back ownership. The maximum frame increases to 1200×1600 to use portrait height.

As in the Rift result adapter, only this body's `VerticalLayoutGroup`, `ContentSizeFitter` and `ScrollRect` are disabled. Portrait shows summary, five cards and rules; landscape shows summary followed by a 3×2 grid (sales/rifts/enhancement, gold/potions/rules). Actual frame aspect determines direction. One proportional body unit fits the content on top of the shared screen scale, including 640×360. No scroll exception, text-size preference or shared window API is added.

The summary reads saved claimed coins, completed goals and the current day's ready reward total. Preparation failure shows an empty retry explanation; success/errors occupy a fixed notice area. Incomplete buttons are disabled, ready rewards use shared green emphasis, and claimed rewards show a check with a disabled button. `ClaimDailyQuest` remains the only grant transaction and updates claims after successful saving. Save failures, midnight, clock rollback, duplicate protections, targets, 100-coin total, unlocks and save schema are unchanged. Orientation, language and dragging do not mutate account state.

The adapter uses `UiTheme`, `UiFonts` and shared buttons. `CurrencyIconView` supplies the existing Abyssal Coin artwork. Five `daily-*` paths in `StorageGlyph` trace the approved SVG activity marks; no raster is generated. Compact descriptions and summary strings are registered in the existing Korean/English table.

`RuntimeDailyQuestSmoke` removes pre-click auto-scrolling and the successful-scroll expectation. It checks every card/text/button inside the frame and viewport, actual raycast hits, non-overlap, unchanged drag position and state-derived summary. At default text size it covers six sizes (440×956, 956×440, 1440×810, 1440×900, 1680×720, 640×360) × KO/EN × safe insets on/off × start/mixed/all-ready/all-claimed/empty states. Display fixtures are isolated geometry samples; grants are separately checked through successful activity transactions, individual claims, failed saves and retries, midnight, clock rollback, both attendance round trips, close/Back and fresh-process persistence.

Final validation combines the full Edit Mode run and necessary daily/attendance macOS smokes once. Failures receive only directly affected reruns. Physical mobile testing and a WebGL player redeployment are separate scope.

### Actual fixed-layout validation

The actual owners and data are linked here: [window layout](../../Assets/HELLSCRIPT/Runtime/Presentation/DailyQuestWindow.cs), [vector marks](../../Assets/HELLSCRIPT/Runtime/Presentation/StorageGlyph.cs), [native check](../../Assets/HELLSCRIPT/Runtime/Presentation/RuntimeDailyQuestSmoke.cs), [existing save/claim transactions](../../Assets/HELLSCRIPT/Runtime/Core/GameStore.DailyQuests.cs) and [fixed-goal catalog](../../Assets/HELLSCRIPT/Resources/Data/DailyQuests.json).

Game integration is `fc5995e9`, the reward/button label spacing repair is `744be568`, and its main integration is `427ac49d`. The [macOS development build](DailyQuestResponsiveEvidence20261001/build.json) succeeded with zero errors. The final player's [source hashes](DailyQuestResponsiveEvidence20261001/source-manifest.json) match the later documentation integration. Compilation errors were zero; existing Unity AI subscription errors were classified separately.

The final-stage full Edit Mode run finished **4,996 total / 4,868 passed / 128 failed / 0 skipped**. It is not an all-pass result. Forty-seven failures match the previous full baseline; the other 81 all reproduce in failed-case-only checks on task-start main `e3b83c94`. These are 80 combat-restore DPS serialization comparisons and one existing Rift window Unity-null-operator check, with zero new Daily Quest UI failures. The [full XML](DailyQuestResponsiveEvidence20261001/responsive-daily-20261001-final-editmode.xml), [baseline comparison](DailyQuestResponsiveEvidence20261001/baseline-comparison.json), [11 pre-change cases](DailyQuestResponsiveEvidence20261001/prechange-daily-ui-failed-only-editmode.xml), [remaining 70 pre-change cases](DailyQuestResponsiveEvidence20261001/prechange-daily-ui-parameters-editmode.xml) and [validation summary](DailyQuestResponsiveEvidence20261001/validation.json) preserve the evidence. The first CLI filter selected only the 11 unparameterized cases, so the remaining 70 were checked separately using method filters.

The full run started on integrated `733b5ca3`. Native validation found the spacing defect during that run; after repair, only the focused checks below and affected smoke were repeated. The full suite was not repeated. Its 33 daily domain and 94 shared UI/button/localization cases also passed. Pre-change failure reproduction was a focused comparison, not another full run.

[Shared UI/localization focused checks passed 94/94](DailyQuestResponsiveEvidence20261001/focused-ui.json), as did the shared UI contract check and its 11/11 tests. The [daily native check](DailyQuestResponsiveEvidence20261001/daily-runtime.txt) passed **120 viewport/language/safe-area/state combinations**, 124 geometry checks, 14 raycast clicks and 120 drags that left the body in place. The first run found touching reward/button text rectangles; after correcting spacing, only the affected daily check was repeated. The [final 120 combinations](DailyQuestResponsiveEvidence20261001/layouts-final.txt) and [pre-repair failure log](DailyQuestResponsiveEvidence20261001/daily-overlap-failed.log) are preserved separately.

Successful activity transactions and 100 individually claimed coins, failed-save rollback/retry, midnight, clock rollback, attendance round trips, Back and close were verified. A [fresh daily process](DailyQuestResponsiveEvidence20261001/daily-restart.txt) restored all five claims and exactly 100 coins; replay granted zero. The [save readback](DailyQuestResponsiveEvidence20261001/save-readback.json) confirms goals, rewards and claims. [Attendance](DailyQuestResponsiveEvidence20261001/attendance-runtime.txt) and [attendance restart](DailyQuestResponsiveEvidence20261001/attendance-restart.txt) also passed. See the [native launch records](DailyQuestResponsiveEvidence20261001/native-results.json).

[Portrait KO](DailyQuestResponsiveEvidence20261001/portrait-ko.png) · [portrait EN](DailyQuestResponsiveEvidence20261001/portrait-en.png) · [landscape KO](DailyQuestResponsiveEvidence20261001/landscape-ko.png) · [landscape EN](DailyQuestResponsiveEvidence20261001/landscape-en.png) · [640×360 EN](DailyQuestResponsiveEvidence20261001/small-en.png) · [all claimed](DailyQuestResponsiveEvidence20261001/claimed-all.png) · [preparation failure](DailyQuestResponsiveEvidence20261001/empty-save-failure.png) · [claim save failure](DailyQuestResponsiveEvidence20261001/claim-save-failure.png). These are macOS synthetic-input results. That native validation stage did not include physical mobile verification or WebGL redeployment; the Web follow-up is recorded below.

### Public WebGL follow-up

A separate build copy used merged `main` revision `74a2477d6b454875020cc788982a218a9c7847fb`, frozen at build start, for a release WebGL player. It succeeded with zero errors and exit code 0. Daily layout, vector, transaction and catalog owners [match the native validation hashes](DailyQuestWebEvidence20261001/daily-ui-source.json). Independent artwork/wiki changes merged during the build are outside this player. Public player commit `17035f7d14723b933b6cc43c776d151772358500` completed its [Pages run](https://github.com/dakrdong/HELLSCRIPT-Web/actions/runs/36858217920). [Play](https://dakrdong.github.io/HELLSCRIPT-Web/) · [Validation summary](DailyQuestWebEvidence20261001/validation.json) · [Web build record](Web_Build.en.md).

The real public player was exercised in the macOS in-app browser while preserving existing guest storage. At default text size, Korean and English each covered 440×956, 956×440, 1440×810, 1440×900, 1680×720 and 640×360. The [12 captures and viewport readbacks](DailyQuestWebEvidence20261001/browser-matrix.json) showed all cards, descriptions, progress, rewards, buttons, rules, reset time and refresh without scrolling; the page and canvas fit each viewport. Dragging in Korean portrait and small English landscape left the body in place. Actual incomplete-button and refresh inputs added no progress or coins. Both attendance round trips, close and Back were checked.

After switching to English, a page reload and guest re-entry restored English, the level-1 Warrior and town, existing potion stocks, zero daily progress and 0/100 claimed coins. Weekly attendance was 2/7 and monthly attendance was 1/28. The original Korean preference was saved again after validation. No completed reward was claimed in the public guest during this follow-up. The native results above are reused for individual claims totaling 100, save failure, midnight, mixed/ready/claimed/failure states and simulated safe areas. There were zero browser runtime errors and two existing URP FSR shader warnings, preserved in the [console record](DailyQuestWebEvidence20261001/browser-console.json). Full Edit Mode and native smokes were not repeated.

The [device availability check](DailyQuestWebEvidence20261001/mobile-availability.json) found no connected Android device and no available iOS `xctrace` tool. Physical-mobile validation remains unrun; no new APK was generated. Browser safe insets were zero, so this evidence does not cover physical notches, touch or device performance.

| Viewport | Korean | English |
| --- | --- | --- |
| Portrait 440×956 | [KO](DailyQuestWebEvidence20261001/ko-440x956.png) | [EN](DailyQuestWebEvidence20261001/en-440x956.png) |
| Landscape 956×440 | [KO](DailyQuestWebEvidence20261001/ko-956x440.png) | [EN](DailyQuestWebEvidence20261001/en-956x440.png) |
| PC 16:9 1440×810 | [KO](DailyQuestWebEvidence20261001/ko-1440x810.png) | [EN](DailyQuestWebEvidence20261001/en-1440x810.png) |
| PC 16:10 1440×900 | [KO](DailyQuestWebEvidence20261001/ko-1440x900.png) | [EN](DailyQuestWebEvidence20261001/en-1440x900.png) |
| PC 21:9 1680×720 | [KO](DailyQuestWebEvidence20261001/ko-1680x720.png) | [EN](DailyQuestWebEvidence20261001/en-1680x720.png) |
| Small landscape 640×360 | [KO](DailyQuestWebEvidence20261001/ko-640x360.png) | [EN](DailyQuestWebEvidence20261001/en-640x360.png) |

[After portrait drag](DailyQuestWebEvidence20261001/ko-portrait-after-drag.png) · [After small landscape drag](DailyQuestWebEvidence20261001/en-small-after-drag.png) · [English preference saved](DailyQuestWebEvidence20261001/language-saved-en.png) · [English title after reload](DailyQuestWebEvidence20261001/restart-title-en.png) · [Daily after reload](DailyQuestWebEvidence20261001/restart-daily-en.png) · [Weekly attendance](DailyQuestWebEvidence20261001/attendance-weekly-en.png) · [Monthly attendance](DailyQuestWebEvidence20261001/attendance-monthly-en.png) · [Korean restored](DailyQuestWebEvidence20261001/language-restored-ko.png).

[Build completion receipt](DailyQuestWebEvidence20261001/build-receipt.txt) · [Raw Pages receipt](DailyQuestWebEvidence20261001/pages.json) · [Evidence SHA-256](DailyQuestWebEvidence20261001/hashes.json).
