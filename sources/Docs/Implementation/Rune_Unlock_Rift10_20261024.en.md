# Rune Board Opens at Rift 10, With an Unlock Show

2026-10-24 · [한국어](Rune_Unlock_Rift10_20261024.md) · user play-test fix U-J (item 16-a)

## What changed

- **Unlock stage 15 → 10.** `UL11_RUNE` in `ContentUnlocks.json` opens at Rift stage 10. `RuneEconomy.MinimumDropStage` reads the same value, so rune drops start at stage 10 too (intended). The game's own word is "stage" while the player says "Rift Level"; only the locked toast uses the player's sentence and every other screen keeps "stage".
- **Locked Rune Board button.** The Rune Board slot of the upper-right content dock (town, plaza and battle HUD, not the prologue) always holds a button. Before unlock it shows `menu-rune-board-locked`, a muted medallion with a padlock in front, and a tap says "Opens when you reach Rift Level 10" (the number comes from `UL11_RUNE.stage`; Korean: "균열 레벨 10 달성 시 오픈됩니다"). After unlock but before the introduction starts, a tap says "The Rune Board opens after its introduction in town." The slot keeps its position and order when it opens (the dock anchors checked by `CasualBattleHudTests` are unchanged).
- **When the show plays.** Reaching Rift 10 or higher → the result is processed → the **first arrival in the town plaza**, once. Nothing starts in battle or on the result window. The saved pending state survives restarts.
- **Show sequence.** Injel Mir's introduction (one added page: what runes are and where they come from) → claim the gift (Rune Block Set) → the dock unfolds and the **padlock breaks** while light spreads (new-feature notice, `flow.skill_unlock`) → the rune emblem flies in and **lands on that slot** → the existing five-step playable guide. Reduce Motion uses a short version. The effect, `DockUnlockBurst`, is visual only; `GameUI` keeps the lesson's own clock.
- **The lesson pays the gift itself.** `ClaimRuneBoardTutorial` creates the fixed-ID box `rune-lesson-starter` and opens it in the same `Transact`. The Rift 15 first-clear package no longer carries this set (the `RewardBoxes.json` milestone was removed) and the server contract (`server/testdata/liveops-contract.json`, its site copy and the generated schema) was updated with it. Accounts that already claimed the first clear get the same single gift, and result-window reward claiming follows the same rules as every other result.

## The shared "content unlock show" mechanism (reused by the blacksmith unit U-K)

| Part | Role |
| --- | --- |
| `ContentUnlockState.introduced`, `introSeeded` | Content whose introduction is done, plus a first-read marker. Saves without them read as defaults, so the `Version=2` check is intact. |
| `ContentUnlocks.IntroIds` | Content ids with a registered show. A new show adds its id here and a start call in `GameUI.StartContentIntro`. Unregistered content is recorded as introduced the moment it opens, so registering it later never replays it. |
| `IntroPending(a,id)`, `NextIntro(a)`, `MarkIntroduced(a,id)` | Pending test, next show in unlock-stage order, completion record. `GrantAll` (guide graduation) marks everything done and `MergeVerified` merges the list. |
| `GameUI.StartContentIntro` | Starts only on a quiet plaza (no other popup, tutorial, result window, suspended Rift or chief welcome), one at a time, checked every 0.5 s. The next show waits until the running one finishes. |
| `DockUnlockBurst` | Padlock-break and light-ring effect on a dock slot. Without a padlock sprite it plays the ring alone (used for landing). |

Gems (`UL05_GEM`) also open at stage 10 and therefore on the same first arrival. Only runes get a show now; gems are not registered (registering them orders same-stage shows by data order, so gems would come before runes).

## How earlier saves behave

| Account | Result |
| --- | --- |
| New account clearing 9 → 10 | The result window is unchanged. One show and lesson on the first town arrival. |
| Rift 10–14 account whose runes were closed | The board opens on read and the show plays once on the first town arrival. |
| 15+, lesson complete or in progress, everything-open (`everythingOpen`) | Recorded as introduced. Never locked, never replayed. A lesson in progress resumes at its saved step in town. |
| 15+, old-style `Reward` step | Consumes the set of its saved stage-15 promise and gets no second set. |
| Highest clear only, no exact clear record | Open icon with no show (same access as before). |

## Validation and limits

Validation used only throwaway saves in the development worktree (on top of the U-G branch); no user-test project or save was touched.

- **Focused Edit Mode**: 336 of 341 tests passed across 14 classes and 5 failed. All five were authoring errors in the newly written tests (four JSON-edit regex cases, one mastery-XP case that lands exactly on a level boundary). After fixing them, `ContentIntroTests` and `RiftContentUnlockTests` passed 30 of 30. The other 12 classes (`ContentUnlockTests`, `RuneEconomyTests`, `RewardBoxTests`, `LiveOpsTests`, `LocalizationTests`, `TutorialProgressionTests`, `CasualBattleHudTests`, `GlobalHudResourceTests`, `GuideTutorialTests`, `HuntEdictProgressionTests`, `HuntEdictSentenceTests`, `RepeatHuntTests`) passed on the first run. Full Edit Mode was not run.
- Server contract: `test_liveops` in `server` passed 22 tests. `build.py --generate-only` changed only the generated schema and the contract copy.
- `tools/check_ui_contract.py --verify` passed 15 and `tools/check_ui_refresh.py` passed 14 bindings.
- **Real GameUI stills** (Editor GUI, Korean and English, 440×956 and 956×440): the locked slot sits in the same sixth place; the padlock break, landing light and open icon are visible. Tap toasts read "균열 레벨 10 달성 시 오픈됩니다" / "마을에서 룬 보드 안내를 마치면 열립니다." and in English "Opens when you reach Rift Level 10" / "The Rune Board opens after its introduction in town." [Portrait Korean](RuneUnlockRift10Evidence20261024/portrait-ko-dock-stages.png) | [Landscape Korean](RuneUnlockRift10Evidence20261024/landscape-ko-dock-stages.png) | [Portrait English](RuneUnlockRift10Evidence20261024/portrait-en-dock-stages.png) | [Report](RuneUnlockRift10Evidence20261024/runtime-report.txt).
- **Real GameUI tick check**: pending → start (`Waiting`→`Reward`, introduction recorded) → story window → claim (five runes, no first-clear claim, lesson box spent) → padlock break → flight → landing (`OpenBoard`, icon open). It found and fixed an Editor-only `??` component check bug. [Result](RuneUnlockRift10Evidence20261024/flow-report.txt).
- **Not done**: the runtime smokes (assertions in `RuntimeRuneBoardSmoke.Unlock`, `RuntimeNaturalRiftProof` and `RuntimeButtonUxSmoke` were updated but not run in a macOS player), real pointer or touch input, the animation's elapsed time (still frames and logic steps only), PC 16:9, 16:10 and 21:9 screens, physical mobile devices and performance.

- Deliberately not done: full Edit Mode, physical devices, performance measurement, the balance model (`tools/balance_rewards.py` still assumes runes open at 15) and its dashboard text, and the operations bundle `rune-apprentice-v1` (minimum stage 15).
- `MergeVerified` merges the content list only and cannot prove tutorial or reward ownership on another device.
- This work appended only its own entries to the end of `en.txt`, so merging with other units can cause a tail overlap (not a content conflict).
