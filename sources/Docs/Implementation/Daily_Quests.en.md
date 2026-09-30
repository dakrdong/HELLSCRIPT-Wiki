# Daily quests

Updated on: 2026-10-01

Account-wide daily quests are available in **Events → Daily Quests**, alongside the existing 7-day and 28-day attendance pages. Automatic attendance popups retain their two original tracks.

| Activity | R0–5 | R6–24 | R25+ | Abyssal Coins |
|---|---:|---:|---:|---:|
| Sell equipment | 30 items | 40 items | 40 items | 20 |
| Clear rifts | Base 20 | Base 12 | Base 12 | 40 |
| Enhance equipment | 2 steps | 2 steps | 1 step | 15 |
| Pay gold in shops | 500G | 2,000G | 2,000G | 15 |
| Consume potions | 10 | 15 | 15 | 10 |
| **Daily quest total** | | | | **100** |

There is one goal per activity, manual claims, and no additional all-complete reward. Existing attendance and first-clear rewards remain separate. The page shows progress, claimed state, today's claimed total and time until 00:00 KST.

`Resources/Data/DailyQuests.json` owns goals, rewards, tiers, timing parameters and caps. It validates five distinct activities, positive goals and a reward sum of exactly 100. Goals and rewards are frozen on the first preparation each day using the highest normal clear across all account heroes. Switching heroes, advancing a tier or editing the catalog does not reset today's claims.

Rift goals start at `ceil(base clears × max(1, current speed))`, The current `CombatSpeedAccess` permits only 1× and normalizes other requests before transactions. If the existing speed policy later permits 1.5×, the formula gives 30/18 clears; this feature does not unlock that speed. With at least three valid recent victories among the last ten, goals can rise to `ceil(1800 / median real clear seconds)`, capped at 120. Valid duration samples are 15 seconds through two hours. This determines a count; it adds no waiting timer and does not change an already prepared day.

The preserved production-simulation onboarding evidence observed five R1–5 victories in 481.35s for Warrior, 516.85s for Ranger and 545.50s for Mage, and five Warrior R6–10 victories in 822.50s. Arithmetic extrapolation at the same per-run duration gives about 32.09–36.37 minutes for 20 early clears and 32.90 minutes for 12 middle clears. Increasing counts for speed targets similar durations. These are calculations from the [early-rift evidence](../Design/HELLSCRIPT_Early_Rift_Balance.en.md), not new human play-time measurements.

Sales, gold income and potion use overlap with rift play; their durations are not added again. Strong gear, lower-stage repetition or same-day speed changes can finish faster. Recent durations cannot guarantee later performance. Tutorial, failures and town actions can take longer. **Thirty minutes of actual play is a design target, not a guarantee.** Later progression and physical mobile timing remain unmeasured.

Existing rift fatigue grants 120 free minutes per day (`RiftEntryRules.DailyMinutes`). The ordinary 32–36-minute design target fits this budget without requiring paid recovery/admission or a new waiting condition. If other play has consumed the allowance, or failures/long fights dominate, full completion that day is not guaranteed. Existing fatigue rules are unchanged.

Existing first-five-rift records collected 32–33 items and consumed 28–41 potions. Selling 30/40 and consuming 10/15 therefore uses part of normal play. Early gold spending is 500G. Enhancement follows existing unlocks, which can be reached while working on rift goals. First two enhancement steps cost 226G on level-one equipment but 12,315G at level20 and 86,205G at level60; later tiers require one step. Low-level spare equipment belonging to the current hero can be used. If every item is at its enhancement cap, new equipment is required. Existing prices, drops and purchase policies are preserved.

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

`DailyQuests` and `GameStore.DailyQuests` own state and transactions. The generated `DailyQuestWindow` uses the shared `ContentWindowView`, theme, fonts, tabs/buttons, window host and independent scroll body. Korean and English are shipped together. `DailyQuestTests` covers catalog, tiers, speed/history, success-only hooks, failures/retry, calendar, offline, persistence and duplicate protection. Development-only `RuntimeDailyQuestSmoke` checks actual domain transactions, claims/reset, KO/EN portrait/landscape/small screens, clipping, fixed controls, raycast clicks, drag and fresh-process persistence. Synthetic input is not physical mobile proof, and synthetic combat is not a human timing study. Final execution evidence is recorded in [optimization and integration validation](Optimization_20260930.en.md).

Executed acceptance: focused170/170 (including27 daily cases), UI contract11/11, daily native initial28.7s and fresh restart4.8s passed. Twelve KO/EN viewport combinations, real raycast/drag, midnight (including preservation of both automatic attendance popup tracks), claims, empty write-failure state and persisted100 coins were verified. [Portrait KO](OptimizationEvidence20260930/daily-portrait-ko.png), [landscape EN](OptimizationEvidence20260930/daily-landscape-en.png), [small](OptimizationEvidence20260930/daily-small-en.png), [claimed](OptimizationEvidence20260930/daily-claimed.png), [empty state](OptimizationEvidence20260930/daily-empty.png).
