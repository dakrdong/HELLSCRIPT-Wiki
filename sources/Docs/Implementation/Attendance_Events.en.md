# Weekly and monthly attendance events

Updated: 2026-09-24
Korean: [7일·28일 출석 이벤트](Attendance_Events.md)

Attendance uses **00:00 Korea Standard Time (UTC+9)**. Weekly attendance resets every Monday; monthly attendance resets on the first day of each calendar month. Both tracks grant independently and share account progress across characters.

## Rules and rewards

Each distinct day played advances each active track once. A first login on Thursday starts at weekly day 1; the next Monday starts a new weekly period. Missed days do not advance progress. Earned, unclaimed rewards remain available until the period resets. Monthly progress stops at day 28 until the next month, including in 29-, 30- and 31-day months.

`Attendance.Reward` owns the reward definitions. The wiki Attendance database extracts all 35 rows from the same runtime source.

| Weekly day | Reward |
| --- | --- |
| 1 | 10 Abyssal Coins |
| 2 | 10 random-color tier 3 gems |
| 3 | 30 Abyssal Coins |
| 4 | 10,000 gold |
| 5 | 50 Abyssal Coins |
| 6 | 5 random-slot cores |
| 7 | 1 random-slot legendary chest |

| Monthly reward | Days 1–7 | Days 8–14 | Days 15–21 | Days 22–28 |
| --- | --- | --- | --- | --- |
| First day: Coins | D1: 10 | D8: 20 | D15: 30 | D22: 40 |
| Second day: Tier 3 gems | D2: 10 | D9: 20 | D16: 30 | D23: 40 |
| Third day: Coins | D3: 30 | D10: 60 | D17: 90 | D24: 120 |
| Fourth day: Gold | D4: 10,000 | D11: 20,000 | D18: 30,000 | D25: 40,000 |
| Fifth day: Coins | D5: 50 | D12: 100 | D19: 150 | D26: 200 |
| Sixth day: Cores | D6: 5 | D13: 10 | D20: 15 | D27: 20 |
| Seventh day: Legendary chests | D7: 1 | D14: 1 | D21: 1 | D28: 2 |

Monthly totals: 900 Coins, 100 tier 3 gems, 100,000 gold, 50 cores and 5 legendary chests. Each gem/core independently rolls among the six existing gem families/eight existing slots.

Chests survive period resets. Each stores its reward seed, the claiming character's class and highest cleared stage, bounded by the existing item-level rules with a minimum of 1. Opening from the event window creates one legendary item in a random slot, using those stored conditions, in the current character's bag. Full bags retain the unopened chest. No automatic equipment, salvage or sale runs.

## Interface

Entering the game opens weekly attendance first, then monthly attendance as separate popups. Visiting the monthly page from the first popup prevents a duplicate automatic monthly popup during that visit. Each page has an independent **Do not show again today** preference, persisted on the device until the next KST midnight. Unchecking re-enables it for the next entry.

The **Events** button near the upper left of town and rift fields always opens page 1, weekly attendance, even when automatic popups are hidden. Page 2 is monthly attendance. Horizontal swipes, tabs and arrows switch pages; vertical drags scroll rewards. Claimed, claimable and upcoming days have explicit labels. A badge shows outstanding rewards.

The shared `ContentWindowView` provides safe area, title, navigation, scrolling body and fixed actions. Reward tiles describe attendance days, not owned equipment. Existing theme, fonts, coin/gem art and chest graphics are reused. `ContentWindowHost` pauses combat and restores its previous state when closed.

## Persistence and verification

`AccountSave.attendance` owns progress, claims and unopened chests. GameStore transactions atomically commit rewards and entitlement, then notify views. Gem capacity or disk failures cannot grant partial rewards. Account/period/day seeds keep random results stable on retry. Popup preferences use a separate device file.

Save schema 12 migrates old saves to empty attendance. Future versions and invalid state preserve the original and stop loading. Stale-period claims and dates before the last recorded attendance are rejected. This remains the existing local development save/device-clock adapter; authoritative server time and account authentication are separate product boundaries.

The [official Black Desert Mobile weekly login event](https://www.world.blackdesertm.com/Ocean/News/Detail?boardNo=4064) informed event access, daily rewards, manual claiming and midnight refresh. HELLSCRIPT uses its own shared theme and account-wide weekly/monthly rules.

- Unity 6000.6.0f1: **108 related Edit Mode tests passed, 0 failed or skipped**. [Results](AttendanceEvidence/editmode.xml) cover resets, midnight, leap years and month/year boundaries, retries, character switches, storage capacity and disk failure.
- The macOS development build succeeded with zero errors. Native acceptance verifies actual uGUI raycast targets before synthetic clicks and drags, and checks rewards, popup ordering, independent suppression, chest opening, paging, scrolling, midnight behavior and combat pause restoration.
- Five resolutions (440×956, 956×440, 1440×810, 1440×900, 1680×720), two languages and 100%/150% text produce **40 page-layout checks**. Safe area, text height and fixed action access are asserted. Narrow field layouts also check the event button against boss status bounds.
- A fresh process verifies progress, balances, chest/item state and device preferences, including manual reopening while suppressed. [Native results](AttendanceEvidence/initial.txt), [restart results](AttendanceEvidence/resume.txt) and [scope/source hashes](AttendanceEvidence/validation.json) record the evidence.
- **Physical mobile devices were not tested.** Existing URP post-processing shader warnings are distinct from attendance feature failures.

[Weekly portrait](AttendanceEvidence/weekly-440x956-en-100.png) · [Monthly at 150% text](AttendanceEvidence/monthly-440x956-en-150.png) · [Field event button](AttendanceEvidence/field-button-440x956-150.png)
