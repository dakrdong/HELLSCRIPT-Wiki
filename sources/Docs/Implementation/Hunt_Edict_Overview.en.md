# Hunt Edict overview implementation

Updated: 2026-09-28 · [한국어](Hunt_Edict_Overview.md)

## 2026-10-01: an Overview with combat styles only

The body contains only **Aggressive, Balanced and Careful** cards and their descriptions. Equipped skills/use modes, combat judgment groups, automation summaries, last-hunt advice and extra warnings are removed. Their settings remain reachable in the original category tabs. Wide layouts place the three cards side by side; narrow layouts stack them.

The fixed **Use default settings ON/OFF** control and existing Save/Revert stay. Recommended-mode dimming, edit locking and draft preservation remain unchanged. Actual style recipes and descriptions reuse `HuntEdictQuickPresets.Styles`.

[Current behavior and verification](Hunt_Edict_Simple_Skill_Actions.en.md). Detailed lists, advice, old captures and text-size validation below are historical implementation evidence, not the current UI. Future validation uses the default text size only.

This records how the overview tab, combat styles and advice from the last hunt in the [Hunt Edict UX redesign](../Design/Hunt_Edict_UX_Redesign.en.md) were built into the game window. Rationale and rules live in the design document; this page covers owners, behaviour, a performance fix and verification.

## Use default settings

Updated 2026-09-29: The former automatic-decision switch moved from preset storage to the top of Overview. A compact **Use default settings ON/OFF** button sits beside its status text on wide screens and above it on narrow screens, outside the content scroll.

- On: Recommended defaults are active. Your custom settings are not being used.
- Off: Your custom settings are active.

This does not turn automatic combat off. `GameStore.SetRecommendedEdict` persists the hero's `useRecommendedEdict` flag; the UI and live combat refresh only after a successful write. Failed writes retain the previous mode. Older saves without the new field keep custom-settings mode.

On resolves a detached execution policy from default options and the new-hero Balanced style. Skill ranks and equipped skills remain unchanged; use conditions, attack order and global policies use defaults. Combat, loot, exploration and repeat hunting share this resolved document. Sanctuary potion restocking and equipment recommendations read the same defaults. Default equipment recommendations are disabled, even if the stored custom configuration enables them.

Off restores the stored custom policies. Authored skill options, presets and pending edits are preserved. The mode belongs to the hero and is not included in share codes. Training/comparison copies cannot change the actual hero's mode.

### Editing lock while defaults are active

On dims and disables every category tab except Overview, all Overview content, and the Save/Revert footer. Content scrolling and existing inertia stop as well. The Overview tab, **Use default settings** button, and window close/back actions remain available. The status message stays fully visible.

Switching Off restores category navigation, content and scrolling. Save/Revert regain their availability according to the existing dirty state. Switching modes neither saves nor discards the draft. Closing with pending edits keeps the existing confirmation flow. Reopening the window or using a skill/share deep link while defaults are On keeps Overview visible so the mode switch remains reachable.

`HuntEdictWindow` uses `CanvasGroup` and shared `UiButton` availability for both presentation and input. Disabled actions reject pointer and keyboard submission. The previously implemented mode transaction and effective combat policies are unchanged.

On 2026-09-29, the macOS Development build passed 20 combinations: 440×956, 956×440, 1600×900, 1600×1000 and 2100×900; Korean/English; text at 100%/150%. Actual uGUI raycasts, pointer actions and keyboard submit events verified disabled input, retained layout, restoration after Off and preservation of unsaved edits. A fresh process and the share deep link also retained the On lock. Physical mobile devices were not tested. [Interaction results](HuntEdictDefaultLockEvidence/sections-runtime.txt) · [Restart results](HuntEdictDefaultLockEvidence/sections-restart.txt)

All 98 focused Edit Mode cases covering defaults, Overview, skill presets, shared UI and localization passed, along with nine shared UI contract checks. [Edit Mode results](HuntEdictDefaultLockEvidence/editmode.xml)

- [Off: editing restored](HuntEdictDefaultLockEvidence/overview-custom.png) · [Portrait, English 150%](HuntEdictDefaultLockEvidence/locked-440x956-en-150.png) · [Landscape, Korean 150%](HuntEdictDefaultLockEvidence/locked-956x440-ko-150.png)

![Defaults On: other categories and Overview content disabled](HuntEdictDefaultLockEvidence/overview-defaults.png)

Sources: [default resolution](../../Assets/HELLSCRIPT/Runtime/Core/HuntEdictDefaults.cs), [save transaction](../../Assets/HELLSCRIPT/Runtime/Core/GameStore.Edict.cs), [combat integration](../../Assets/HELLSCRIPT/Runtime/Core/CombatSimulation.EdictDrive.cs).

## Owners

| Area | Owner | Responsibility |
|---|---|---|
| Tab list | `HuntEdictUiCatalog.Tabs`, `AutomationTabs` | Adds the overview as the first tab and names the five tabs reported as automation. |
| Combat styles | `styles` in `Resources/HuntEdictQuickPresets.json`, `HuntEdictQuickPresets.ApplyStyle`/`MatchStyle`/`StyleScopes` | Define styles as bundles of existing quick presets, apply and recognise them. No new option values or save fields. |
| Advice | `HuntEdictCoach`, `EdictCoachTip` | Read the hero's latest `RunRecord`; never re-simulate a fight or edit the hero. |
| Page | `HuntEdictWindow.Overview.cs` | Reuses the window's scrolls, buttons, theme, skill icons and quick-preset picker. `HuntEdictEditSession` owns the draft and the existing `GameStore.CommitHuntEdict` saves it. |
| Entry points | `GameUI.RiftVictory.cs`, `GameUI.Tutorials.cs`, `GameUI.ShowDefeatAnalysis`, `HuntEdictWindow.SelectSkillPage`/`OpenSkillPolicy` | The result screen and the older-record defeat analysis open the overview, tutorials open their area, and opening a skill subpage switches to the Skill tab. |

No new window, canvas, font or save format was added. The overview is one tab of the existing `HuntEdictWindow`, and the Hunt Edict section of the shared UI contract states its rules.

## Behaviour

- The window opens on the overview. Portrait lays out ten tabs as 5×2; landscape keeps one left column.
- A style card applies its seven group presets in turn to a draft. Nothing reaches the hero or the account before Save, and Revert returns to the last saved edict.
- When the values match none of the three styles the page says so, and it warns when all three dodge policies and low-HP retreat are off. New heroes start on Balanced (below), so the warning appears for older heroes that kept the defaults.
- Advice uses the newest record whose `review.heroId` is the hero. It recognises death, running out of time, repeated skill waits and a comfortable clear, and drops advice the saved edict already follows. The thresholds are constants in `HuntEdictCoach`.
- Applied advice shows "applied; save to use it" and leaves the list after saving.
- A judgment row opens the existing quick-preset picker. Custom there opens the group's details in its tab without changing any value.

## Editing performance

The first smoke run measured 454 ms per overview repaint (macOS development build, 1600×900). The cost sat in the shared canonicalization path, not in the page:

- `ClassSkillLoadout.ProjectEdict` and `ClassSkillOptions.ProjectLegacy` wrote each option through `HuntEdictV2.WithOption`, re-canonicalizing the whole edict document every time.
- `EdictOptions.Canonical` looked each of the 151 global options up in a linear scan, O(n²).
- Quick-preset matching applied the preset to a copy and compared the result.

Fixed as follows; results are unchanged:

- New `HuntEdictV2.SetOption`: both projections canonicalize once and write options through it, and `WithOption` uses it too.
- `EdictOptions.Canonical` looks values up in a dictionary and rejects missing, duplicate and unknown IDs as before.
- Global-group matching computes the values a preset writes with the same function the apply path uses (`GlobalValue`) and compares them with the current values. Every caller passes a canonical draft, so the answer is the same; `GlobalMatchingAgreesWithApplyingTheRecipe` checks this for every global preset pair in all three classes.

| Measurement (Editor, mean of 20; `MatchStyle` mean of 5) | Before | After |
|---|---:|---:|
| Draft canonicalization `HuntEdictLoadout.Canonical` | 12.8 ms | 2.0 ms |
| Applying a global quick preset | 26.6 ms | 4.1 ms |
| Applying a skill quick preset | 31.5 ms | 5.4 ms |
| Style recognition `MatchStyle` | 188.6 ms | 0.28 ms |
| Overview repaint (development build, mean of 5) | 454.5 ms | 40.1 ms |

The repaint time was measured with no other Unity job running; later builds measured 42.7 ms (`4c85f5e7`) and 47.3 ms (new heroes on Balanced) under the same condition. The fix also speeds up the group lists, skill policies and pre-save validation of the existing tabs. Mobile timings were not measured.

## Existing smokes updated

- `RuntimeSkillTreeSmoke` inspects the tree right after opening, so it now selects the Skill tab first.
- `RuntimeRiftVictorySmoke` now expects the result screen's **Edit hunt edict** to open the overview.
- `RuntimeEdictQuickPresetSmoke` sets `mapComplete` so a fresh account is not held in the mandatory tutorial. Running to the end for the first time, it showed that the full-warehouse group has used its own controls instead of a quick-preset picker since `bc71ab3f`; that group is excluded from the picker loop, and its preset values stay covered by the Edit Mode tests.

## New hero default: Balanced

At the user's decision on 2026-09-28, newly created heroes start on the Balanced style. `HuntEdictStorage.InitializeNewHero` applies `HuntEdictQuickPresets.NewHeroStyle` (Balanced) to the new hero's edict document. Switching to the skill tree copies that document, so the style survives the switch.

- **What changes**: emergency response turns low-HP retreat on (fall back at 30% HP, return at 55%) together with lethal-damage response; all three damage types are dodged when the expected HP loss is 10% or more; special dangers cover stun and freeze plus 25% combined overlapping damage. Engagement position, encirclement, target selection and potions already matched Balanced at their defaults.
- **Unchanged**: option defaults (`EdictOptions`) stay as they are, because the 249 quick presets are written against them. Existing heroes are not changed; an older hero that kept the defaults still sees the overview warning.
- **Tests**: `ANewHeroStartsOnTheBalancedStyleBeforeAndAfterTheSkillTree` checks, for all three classes, that a new hero starts on Balanced, that options outside the style keep their defaults and that the style survives the skill-tree switch. Two `EdictShareTests` cases that assumed retreat starts off now flip the current value.

### Early-balance comparison

`NaturalRiftProgressionAudit.RunEarlyClasses` played fresh accounts twice: the baseline is current `main` (`bfaf2ddc`) with the old defaults, the candidate starts on Balanced. The Warrior played to rift 10 and the Ranger and Mage to rift 5, each class on the same three seeds (20260924–20260926). A new optional `-hellscriptNaturalSeeds` argument runs several seeds in one invocation; without it the tool behaves as before. The repository's ledger validator (`tools/report_natural_rift_progression.py`) verified all 18 paths.

| Class | Seed | Baseline: attempts/deaths/timeouts | Balanced: attempts/deaths/timeouts | Baseline rift time | Balanced rift time | Baseline potions | Balanced potions |
|---|---|---|---|---:|---:|---:|---:|
| Warrior R10 | 20260924 | 11/1/0 | 10/0/0 | 25:10.80 | 22:32.05 | 111 | 102 |
| Warrior R10 | 20260925 | 10/0/0 | 10/0/0 | 23:20.50 | 23:28.25 | 108 | 107 |
| Warrior R10 | 20260926 | 10/0/0 | 11/1/0 | 23:40.00 | 26:30.05 | 99 | 110 |
| Ranger R5 | 20260924 | 5/0/0 | 5/0/0 | 8:40.45 | 8:45.50 | 32 | 30 |
| Ranger R5 | 20260925 | 5/0/0 | 5/0/0 | 9:58.60 | 9:58.50 | 42 | 36 |
| Ranger R5 | 20260926 | 5/0/0 | 5/0/0 | 9:46.35 | 9:21.60 | 37 | 36 |
| Mage R5 | 20260924 | 5/0/0 | 5/0/0 | 9:11.50 | 9:11.50 | 44 | 44 |
| Mage R5 | 20260925 | 5/0/0 | 5/0/0 | 10:04.00 | 9:38.55 | 47 | 42 |
| Mage R5 | 20260926 | 5/0/0 | 5/0/0 | 10:09.10 | 10:09.10 | 45 | 45 |
| Total | 9 paths | 61/1/0 | 61/1/0 | 130:01.30 | 129:35.10 | 565 | 552 |

- In total both played 61 attempts with one death and no timeout; Balanced spent 26 s less in rifts and used 13 fewer potions. Every path ended at the same level (Warrior 13, Ranger and Mage 8).
- The death moved: the baseline fell in rift 7 on the Warrior's first seed, Balanced to the rift 6 boss (the Graveyard Executioner) on its third. Both times a single large hit carried HP past the 30% retreat threshold at once; Balanced does not stop an instant heavy hit.
- Without deaths, rift times differed by −25 s to +8 s. Two Mage paths were identical, meaning no retreat or dodge condition ever fired on them.
- On this sample the Balanced default barely changes early pace or difficulty. Nine paths are too few to judge a difference in death rates.

Evidence: [comparison](HuntEdictOverviewEvidence/balanced-default/compare.txt) · [per-path summary and ledger checks](HuntEdictOverviewEvidence/balanced-default/audit-summary.json) · [deaths](HuntEdictOverviewEvidence/balanced-default/deaths.txt). The earlier measurement is in [Rift 1–10 natural progression](../Design/HELLSCRIPT_Rift_1_10_Playtest.en.md).

## Verification

Run on an APFS clone of a work tree created from the latest `main` (`672e4a0c`). `main` moved twice during the work (`40e601c4`, `b418de72`); each time it was merged and the checks were repeated. The new-hero default followed on the merged `main` (`bfaf2ddc`) with the same checks. The user's open Unity Editor and real account save were not used.

### Edit Mode tests

- The new `HuntEdictOverviewTests` (19) pass. They cover style application, recognition, share-code round trips and preservation of other options, the numbers quoted in the style descriptions, agreement between global matching and applying, the advice rules (no record, death, time out, range, resource, comfortable clear, dropping saved advice) and the new-hero default for all three classes. An earlier version of this page said 17; before the three new-hero cases the class actually held 16.
- 23 related classes with 874 tests pass with new heroes on Balanced: edict, quick presets, localization, tutorial, first play, shared UI, skill tree, skill policies, combat history and the newly merged enemy-attack and telegraph-gauge tests; the skill runtime and policy tests cover the changed projections. [Result XML](HuntEdictOverviewEvidence/editmode-related.xml)
- The full suite passed 4,520 of 4,567 before merging, 4,547 of 4,594 after merging `40e601c4` and 4,573 of 4,620 with new heroes on Balanced. All three times the 47 failures are exactly the tests that already failed before this work; nothing new fails. After merging `b418de72`, which only changes enemy attacks, the related classes were rerun instead of the full suite. [Summary](HuntEdictOverviewEvidence/editmode-full.txt)

### macOS development build and runtime smokes

The macOS development build with new heroes on Balanced succeeded with no errors. With no other Unity job running, these smokes drove real uGUI raycasts and pointer down/up/click. [Summary](HuntEdictOverviewEvidence/smokes.txt)

| Smoke | Result |
|---|---|
| `RuntimeEdictOverviewSmoke` (new) | Pass: a new hero starting on Balanced; the old-defaults safety warning; style apply, revert and save; a judgment preset; Custom opening details; skill, tree and automation links; three pieces of advice from a recorded time-out shown, applied and retired after saving. In 20 combinations (five frame sizes × Korean/English × 100%/150% text) all ten tabs and Save/Revert stay in the frame, are hit by the pointer and have unclipped labels. A separate process reloads the saved style and advice. [Actions](HuntEdictOverviewEvidence/runtime.txt) · [Restart](HuntEdictOverviewEvidence/restart.txt) |
| `RuntimeSkillTreeSmoke` | Pass (initial and restart). |
| `RuntimeEdictQuickPresetSmoke` | Pass (initial and restart). |
| `RuntimeTutorialSmoke` | Pass (phase one and resume): a new hero on Balanced finishes the mandatory tutorial map and the first rift, including the tutorial's edict save (F05) and new-skill equip (F06). Once, on an earlier merge and alongside the full Edit Mode suite, the resume ended during the first rift without a result file, exception or failure record; it passed when rerun alone and on the newest merge. |
| `RuntimeRiftVictorySmoke` | Pass; the result screen's edict button opens the overview. |

### Screens

- [New hero: starts on Balanced](HuntEdictOverviewEvidence/overview-new-hero-1600x900-ko.png) · [Aggressive chosen, unsaved](HuntEdictOverviewEvidence/overview-aggressive-1600x900-ko.png) · [Old defaults: safety warning](HuntEdictOverviewEvidence/overview-safety-warning-1600x900-ko.png) · [Advice from a time-out, one applied](HuntEdictOverviewEvidence/overview-advice-1600x900-ko.png)
- Portrait 440×956: [Korean 100%](HuntEdictOverviewEvidence/overview-440x956-ko-100.png) · [Korean 150%](HuntEdictOverviewEvidence/overview-440x956-ko-150.png) · [English 150%](HuntEdictOverviewEvidence/overview-440x956-en-150.png)
- Landscape 956×440: [Korean 100%](HuntEdictOverviewEvidence/overview-956x440-ko-100.png) · [Korean 150%](HuntEdictOverviewEvidence/overview-956x440-ko-150.png) · [English 150%](HuntEdictOverviewEvidence/overview-956x440-en-150.png)
- PC: [16:9 1600×900](HuntEdictOverviewEvidence/overview-1600x900-ko-100.png) · [16:10 1600×1000](HuntEdictOverviewEvidence/overview-1600x1000-ko-100.png) · [21:9 2100×900](HuntEdictOverviewEvidence/overview-2100x900-ko-100.png)

![Overview for a new hero, starting on Balanced](HuntEdictOverviewEvidence/overview-new-hero-1600x900-ko.png)

![Safety warning for an older hero that kept the defaults](HuntEdictOverviewEvidence/overview-safety-warning-1600x900-ko.png)

![Advice applied from the last hunt](HuntEdictOverviewEvidence/overview-advice-1600x900-ko.png)

![Portrait, English 150%](HuntEdictOverviewEvidence/overview-440x956-en-150.png)

### Not verified

- No phone or tablet was used; portrait and landscape were checked at macOS window sizes only, and mobile repaint times were not measured.
- No Android or iOS build was made.
- Whether new players actually understand faster was not tested with people.
- The advice thresholds were not tuned against real play records.
