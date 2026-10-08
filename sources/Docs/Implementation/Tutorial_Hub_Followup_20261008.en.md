# Tutorial hub follow-up and per-hero tutorials

Updated: 2026-10-08 · [한국어](Tutorial_Hub_Followup_20261008.md)

Six user requests were handled in one PR. The hub's implementation record is [Puzzle hub native implementation](Puzzle_Hub_Native.en.md), the shared battle and town shortcuts are in [Play content dock](Play_Content_Dock.en.md), and the Hunt Edict window is owned by [Hunt Edict overview](Hunt_Edict_Overview.en.md). This page collects what changed (positions, sizes, rules) and what was verified.

## 1. The hub shows only the level to play

The left and right buttons beside the level plate and the keyboard arrow keys are gone. There is no reason to replay an earlier level, so the hub always shows the level that has to be played (in review, the level being reviewed). The plate stays centred within the room left by the status area (on the left in landscape). The `hub-left` and `hub-right` buttons, `PuzzleHubWindow.MoveLevel` and the hero-preparing `prepare` callback no longer exist. The native smoke checks that both buttons are absent and that the keyboard arrows change neither the level nor the economy.

## 2. The HP, resource and experience area moves with the page

The status area is a fixed overlay (sorting order 600), so when another menu slid in it did not slide and vanished after the transition. During a slide its position is now `status offset = page position − home position`, so it leaves and returns in the same direction and at the same speed as the page; the offset returns to 0 when the transition ends. The smoke `CheckStatusSlides` checks at 0.2 s into the slide, leaving and arriving, that the offset equals the page offset.

## 3. Bottom menu: Skills becomes Blacksmith, skills move into the Hunt Edict

The hub's bottom menu is now `Inventory · Hunt Edict · Waiting room · Blacksmith · Settings`. The button names `hub-menu-0…4` are unchanged; number 3 changed from skills to the blacksmith.

- **Blacksmith**: opens the existing `BlacksmithWindow` as is (on `PuzzleStore`, so a review uses its throwaway account). The four services follow the same rift unlock conditions as in town; which service the tutorial opens, and in what order, is in [Tutorial forge](Tutorial_Forge_20261008.en.md) (at the time of this PR they were all sealed). The icon is the new picture `Resources/Art/PuzzleHub/ui/icon-blacksmith.png`; its prompt and provenance are in `Docs/Art/PuzzleHub/ui-kit-4-*`.
- **Hunt Edict tabs**: the tutorial has no `Summary` tab (this PR kept it from L4 on; a follow-up removed it, see [Tutorial forge](Tutorial_Forge_20261008.en.md)). The tabs are `Skills · Combat · Survival` (plus loot, bag, exploration and so on as they open). The tab selected when the window first opens is `Skills` until the level's skills are learned, then the first rules tab. The L4 "Combat style" answer (aggressive/balanced/careful cards, `edict-style-*`) sits at the top of the `Combat` tab.
- **First-skill lesson (L1)**: the highlighted button moved from `hub-menu-3` to `hub-menu-1` (Hunt Edict). The line now reads "Open the Hunt Edict and learn your first power on its Skills tab." (`GameUI.TutorialPit.cs`). `PuzzleHubWindow.SkillButton` was generalised into `MenuButton(int)`.

## 4. The battle gear, content shortcuts and event icon take the skill icon size

At 854×450 landscape the dock icons were about 17 px while skill icons were about 38 px (the dock was fixed at 44 page units). The formula the town already used is now `GlobalHudLayout.ShortcutIcon`, shared with battle. Size = the skill icon's size on screen × 1.15 (the emblem art leaves about 13% of its square clear, so the visible disc matches the skill icon), capped so six fit above the HUD. The minimum is 44 screen pixels in town and the 44 page units the battle shortcuts always had. On a 16:9 landscape screen that is 88.3 page units at any resolution (88 px at 1600×900).

- The settings gear is the same size without its plate, as in town, and the dock folds out beneath it. The observation menu (☰), power saving, escape, the map panel and the header text step left by that column's width (the map panel's `68`, `208` and `120` constants are now derived from the dock width). The event icon on the left is the same size.
- In portrait the skill icons are the landscape composition shrunk to the width, so 1.15 times them is almost the 44 page units the shortcuts had before (44.7). Portrait layout therefore stays nearly as it was (24 px at 440×956 beside 21 px skill icons) and the map panel and boss status keep their width. Using the town's 44-pixel floor in battle would have doubled the column in small test windows and narrowed both, so it was not used.

## 5. Each hero runs their own tutorial

The account's five tutorial fields (`mapComplete`, `mapHero`, `armorId`, `tutorialRun`, `puzzle`) are **the selected hero's share**. The other heroes' shares wait in `guide.tutorialSlots`. Every place that changes the hero (the character screen's `SaveEntryCharacter`, the in-game `SwitchCharacter`, the developer `SelectHero`) goes through `Tutorials.Switch(account, from, to)`, which parks the old hero's share in a slot and takes the new hero's out. A hero who has not begun starts from the beginning. The roughly 150 places that read `guide.puzzle`, `Tutorials.Mandatory` and similar are unchanged; they simply read another hero.

- During the tutorial the `Change character` settings tab is gone (`GameController.CanChangeCharacterFromSettings` refuses too). To play another character, sign out, sign back in and choose them on the character screen: that character's own tutorial continues where it left off. Signing out (`Go to title`) used to live inside the `Change character` tab, so during the tutorial it moved to the top of the `Account` tab. Settings opened from the title and character screens keep their character tab.
- Save migration: a save whose tutorial slot version (`tutorialSlotVersion`) is 0 is tidied once on first load. An account that finished the tutorial counts every hero it already has as finished (the old rule released the whole account). A tutorial under way goes to its hero's slot and the currently selected other hero starts fresh. Exempt accounts stay exempt.
- A parked share passes the same checks as the selected one (it is swapped in, checked and swapped back). A share without a hero is dropped and each hero keeps one. A bad share refuses the load with the usual "original save preserved" exception.
- One hero graduating leaves the others' tutorials untouched. The graduation gold (1,000) and gear are keyed by hero id, so each hero receives them once.

## 6. Sound

- The hub's art buttons are all `Icon` role, so after #76 their click became `ui.click`. `UiButton.SoundCue` now names what the press means (Start = `ui.click_primary`, bottom menu = `ui.tab`).
- Menu slides play `ui.open` when opening and `ui.close` when returning (a click in the same frame yields, as before).
- When a tutorial challenge begins (`ContinueTutorial`) the rift-entry cue `flow.rift_enter` plays. The battle cry stays off because it is a tutorial.
- No new sound files were made: this session had no ElevenLabs access. The candidates and prompt drafts are below.

| Candidate id | Scene | Plays now | Sound to make (prompt draft) |
| --- | --- | --- | --- |
| `puzzle.page` | Hub menu slide | `ui.open`/`ui.close` | A heavy stone door sliding, a low scrape of 0.4 s ending in a short iron ring |
| `puzzle.start` | Level start | `flow.rift_enter` | A pit gate rising on chains with a short gust of sand, 1 s |
| `puzzle.hint` | Hint revealed | none (dialogue advance) | A parchment unfolding with one small bell, 0.5 s |
| `puzzle.clear` | First clear of a level | `flow.victory` | A short metal fanfare with coins pouring, 1.5 s |
| `puzzle.graduate` | Graduation | `flow.level_up` | A heavy great door opening with a choir, 2.5 s |
| `hazard.pull` | L7 gravity pull | none | A low whirl of air drawn into one spot, 2 s (each cycle) |

## UI changes for whoever writes the tutorial lines

Button positions and names moved, which affects guide lines and the pointer ring.

1. Hub bottom menu number 3 changed from Skills to Blacksmith (`hub-menu-3`). Skills are learned in `hub-menu-1` Hunt Edict, tab `edict-tab-skills`.
2. The Hunt Edict window's tabs are `Skills · Combat · Survival`. There is no `Summary` (`edict-tab-overview`). The combat style cards (`edict-style-*`) sit at the top of the `Combat` tab (`edict-tab-combat`) from L4 on. At L1 there is a single `Skills` tab.
3. The hub has no level left/right buttons (`hub-left`, `hub-right`).
4. The HP, resource and experience area moves with the page when menus slide.
5. Battle screen, top right: the settings gear, the dock beneath it (`콘텐츠 메뉴`, `사냥 칙령` …) and the event icon on the left (`field-events`) all grew to the skill icon size. Their positions moved down.
6. In settings during the tutorial there is no `Change character` tab and `Go to title` is on the `Account` tab. Tutorial progress is per hero.
7. One first-skill guide line changed (`PitSkillLessonStep` in `GameUI.TutorialPit.cs`, and `en.txt`).

## Verification

- Edit Mode, once on the final code: 5,643 tests, 5,641 passed, 0 failed, 2 skipped (`MeasureEveryDesignBuild` and `Capture`, which always skip).
- Focused tests: `TutorialHeroSlotsTests` (share swapping, save migration, checks on parked shares, save and load, in-game character switch), `GlobalHudTests` (the icon size formula), `PuzzleHubTests` (the arrow-free layout), the localization tests (source lines and their English) and the shared UI tests.
- `check_ui_contract.py --verify` 15 tests, `check_ui_refresh.py` 14 bindings, the sound bank check (283 cues, 0 problems) and `elevenlabs_audio.py --check` pass.
- macOS development player (real pointer input): the whole tutorial smoke (L1 to graduation) passed to the end for the Warrior in Korean at 1600×900 (including the 10 hub layouts and 4 safe areas), the Ranger in English at 956×440 and the Mage in Korean at 440×956. The new checks cover the blacksmith in the bottom menu (`BlacksmithWindow` opens as the child window beside the navigation band), the Hunt Edict's tab set, the settings' tab set, the status slide, the absence of level arrows, and L4's Summary, Skills, Combat and Survival tabs. The battle layout smoke (including the three battle icon sizes), the map display smoke and the screen settings smoke pass too.
- The real flow of signing out and choosing another hero (hub settings → Account → Go to title → guest → begin the other hero → sign out again → begin the first hero) and the menu and challenge-start sounds also passed. That flow exposed a bug that is now fixed: after signing out of the hub the hub window stayed over the title and covered its buttons (`ShowTitle` now closes the hub first). Signing out from the hub settings existed before, but nobody had pressed the title buttons after it.
- Smokes that already fail on the base build (`f05bbc28`): the content dock smoke (a new account is blocked by the mandatory tutorial and attendance popups, so the town dock is never reached) and the battle escape smoke ("Expected two captions"). They are unrelated to this change and were not fixed.
- Only smoke files changed after the full run, so Edit Mode was not rerun; only the related tutorial smoke was run again.

### Not verified

- Nothing was seen on a physical phone or tablet. Screens were checked only at macOS development player window sizes.
- No new sound files were made and nothing was auditioned.
- The blacksmith's service screens were checked only in their sealed state on a tutorial account.
- If an older client re-saves this save it loses the other heroes' tutorial shares (it does not know the new fields). That does not matter while development builds are used together.
