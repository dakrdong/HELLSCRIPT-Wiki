# Hunt Edict overview implementation

Updated: 2026-09-28 · [한국어](Hunt_Edict_Overview.md)

This records how the overview tab, combat styles and advice from the last hunt in the [Hunt Edict UX redesign](../Design/Hunt_Edict_UX_Redesign.en.md) were built into the game window. Rationale and rules live in the design document; this page covers owners, behaviour, a performance fix and verification.

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
- When the values match none of the three styles the page says so, and it warns when all three dodge policies and low-HP retreat are off, which is how a new hero starts.
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

The repaint time was measured with no other Unity job running; the final build with the latest `main` merged measured 42.7 ms under the same condition. The fix also speeds up the group lists, skill policies and pre-save validation of the existing tabs. Mobile timings were not measured.

## Existing smokes updated

- `RuntimeSkillTreeSmoke` inspects the tree right after opening, so it now selects the Skill tab first.
- `RuntimeRiftVictorySmoke` now expects the result screen's **Edit hunt edict** to open the overview.
- `RuntimeEdictQuickPresetSmoke` sets `mapComplete` so a fresh account is not held in the mandatory tutorial. Running to the end for the first time, it showed that the full-warehouse group has used its own controls instead of a quick-preset picker since `bc71ab3f`; that group is excluded from the picker loop, and its preset values stay covered by the Edit Mode tests.

## Verification

Run on an APFS clone of a work tree created from the latest `main` (`672e4a0c`). `main` moved twice during the work (`40e601c4`, `b418de72`); each time it was merged and the checks were repeated. The user's open Unity Editor and real account save were not used.

### Edit Mode tests

- The new `HuntEdictOverviewTests` (17) pass. They cover style application, recognition, share-code round trips and preservation of other options, the numbers quoted in the style descriptions, agreement between global matching and applying, and the advice rules (no record, death, time out, range, resource, comfortable clear, dropping saved advice).
- 23 related classes with 871 tests pass on the newest merge (`4c85f5e7`): edict, quick presets, localization, tutorial, first play, shared UI, skill tree, skill policies, combat history and the newly merged enemy-attack and telegraph-gauge tests; the skill runtime and policy tests cover the changed projections. [Result XML](HuntEdictOverviewEvidence/editmode-related.xml)
- The full suite passed 4,520 of 4,567 before merging and 4,547 of 4,594 after merging `40e601c4`. Both times the 47 failures are exactly the tests that already failed before this work; nothing new fails. After merging `b418de72`, which only changes enemy attacks, the related classes were rerun instead of the full suite. [Summary](HuntEdictOverviewEvidence/editmode-full.txt)

### macOS development build and runtime smokes

The macOS development build of the newest merge succeeded with no errors. With no other Unity job running, these smokes drove real uGUI raycasts and pointer down/up/click one after another. [Summary](HuntEdictOverviewEvidence/smokes.txt)

| Smoke | Result |
|---|---|
| `RuntimeEdictOverviewSmoke` (new) | Pass: new-hero warning; style apply, revert and save; a judgment preset; Custom opening details; skill, tree and automation links; three pieces of advice from a recorded time-out shown, applied and retired after saving. In 20 combinations (five frame sizes × Korean/English × 100%/150% text) all ten tabs and Save/Revert stay in the frame, are hit by the pointer and have unclipped labels. A separate process reloads the saved style and advice. [Actions](HuntEdictOverviewEvidence/runtime.txt) · [Restart](HuntEdictOverviewEvidence/restart.txt) |
| `RuntimeSkillTreeSmoke` | Pass (initial and restart). |
| `RuntimeEdictQuickPresetSmoke` | Pass (initial and restart). |
| `RuntimeTutorialSmoke` | Pass (phase one and resume), including the tutorial's edict save (F05) and new-skill equip (F06). Once, on an earlier merge and alongside the full Edit Mode suite, the resume ended during the first rift without a result file, exception or failure record; it passed when rerun alone and on the newest merge. |
| `RuntimeRiftVictorySmoke` | Pass; the result screen's edict button opens the overview. |

### Screens

- [New hero: safety warning and three styles](HuntEdictOverviewEvidence/overview-new-hero-1600x900-ko.png) · [Balanced chosen](HuntEdictOverviewEvidence/overview-balanced-1600x900-ko.png) · [Advice from a time-out, one applied](HuntEdictOverviewEvidence/overview-advice-1600x900-ko.png)
- Portrait 440×956: [Korean 100%](HuntEdictOverviewEvidence/overview-440x956-ko-100.png) · [Korean 150%](HuntEdictOverviewEvidence/overview-440x956-ko-150.png) · [English 150%](HuntEdictOverviewEvidence/overview-440x956-en-150.png)
- Landscape 956×440: [Korean 100%](HuntEdictOverviewEvidence/overview-956x440-ko-100.png) · [Korean 150%](HuntEdictOverviewEvidence/overview-956x440-ko-150.png) · [English 150%](HuntEdictOverviewEvidence/overview-956x440-en-150.png)
- PC: [16:9 1600×900](HuntEdictOverviewEvidence/overview-1600x900-ko-100.png) · [16:10 1600×1000](HuntEdictOverviewEvidence/overview-1600x1000-ko-100.png) · [21:9 2100×900](HuntEdictOverviewEvidence/overview-2100x900-ko-100.png)

![Overview for a new hero](HuntEdictOverviewEvidence/overview-new-hero-1600x900-ko.png)

![Advice applied from the last hunt](HuntEdictOverviewEvidence/overview-advice-1600x900-ko.png)

![Portrait, English 150%](HuntEdictOverviewEvidence/overview-440x956-en-150.png)

### Not verified

- No phone or tablet was used; portrait and landscape were checked at macOS window sizes only, and mobile repaint times were not measured.
- No Android or iOS build was made.
- Whether new players actually understand faster was not tested with people.
- The advice thresholds were not tuned against real play records.
