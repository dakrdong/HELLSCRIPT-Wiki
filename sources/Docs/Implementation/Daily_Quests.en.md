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

`DailyQuests` and `GameStore.DailyQuests` own state and transactions. The generated `DailyQuestWindow` uses the shared `ContentWindowView`, theme, fonts, tabs/buttons, window host and independent scroll body. Korean and English are shipped together. `DailyQuestTests` covers catalog, identical goals across progression/speed/history, legacy-day conversion and preservation of completion/claims, success-only hooks, failures/retry, calendar, offline, persistence and duplicate protection. Development-only `RuntimeDailyQuestSmoke` checks actual domain transactions, claims/reset, KO/EN portrait/landscape/small screens, clipping, fixed controls, raycast clicks, drag and fresh-process persistence. Synthetic input is not physical mobile proof, and synthetic combat is not a human timing study. Final execution evidence is recorded in [optimization and integration validation](Optimization_20260930.en.md).

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

Final validation combines the full Edit Mode run and necessary daily/attendance macOS smokes once after the final code and main integration. Failures receive only directly affected reruns. Physical mobile testing and a WebGL player redeployment are separate scope. Actual results and captures will be recorded after final validation.
