# Hunt Edict UI Overhaul — Native Port Record

Updated: 2026-10-05 · [한국어](Hunt_Edict_UI_Overhaul_Native.md) · [Design document](../Design/Hunt_Edict_UI_Overhaul.en.md) · [Mockup README](../../Prototypes/HuntEdict/README.md) · [Native port prompt](../../Prototypes/HuntEdict/NATIVE_PORT_PROMPT.md)

Status: the surface and information design of the approved HTML mockup v2 now lives in the Hunt Edict window (`HuntEdictWindow`), milestones M1-M6. The window provides the places and shapes for the new slots (guide caption, three-beat strip, after-save button), but **the tutorial is not connected to them yet** (design document chapter 10, Phase 4). No save field, domain rule or tutorial code changed.

## 1. At a glance

| Item | Content |
| --- | --- |
| Branch and base | `claude/hunt-edict-native-port`, built on `main` (`17dc75e9`, which includes the merged tutorial branch PR #52) with the design-document branch (PR #51) merged in |
| Commits | `bb7f6ba1` window surface, slots and smoke fixtures; `1e718f49` contract smoke and gate visibility checks; `0ebf672d` merge of `main`; then this record and the evidence |
| Size | 34 files under `Assets`, +942 / -129 lines. New files: one Core file, three window partials (`Frame`, `Sentences`, `Guide`), one EditMode test, one smoke |
| Unchanged | The ten tab ids and every `edict-*` name, the area names `Main tabs`, `Option area`, `Fixed save controls`, the order in `HuntEdictUi.json`, the disclosure rules (`HuntEdictProgression`), the save path (`CommitHuntEdict`), the meaning of `HasDialog`, no new save field |
| New names | `edict-teaser`, `edict-beat-strip`, `Guide caption`, `edict-save-next` (as in design document chapter 6) |

## 2. Implementation by milestone

| Step | Implementation | Main files |
| --- | --- | --- |
| M0 baseline | Related EditMode 181/181 before the change. Only 2 of 8 smokes passed; the cause was the staged-account fixture (chapter 5). The name contract is checked by the contract smoke, expanded from game data | `RuntimeEdictContractSmoke.cs` |
| M1 frame and tabs | Header subtitle "class · Lv · style". Tabs show glyph + name + subtitle (landscape, 6 tabs or fewer); portrait stacks the glyph over the name. A gold marker bar on the selected tab. The prologue's single tab is a 48-wide glyph column (landscape) or a title chip (portrait); the `edict-tab-*` objects stay. An unsaved dot in the footer | `HuntEdictWindow.cs`, `HuntEdictWindow.Frame.cs`, `HuntEdictSurface.cs` |
| M2 groups, options, dialogs | Group chips show the current answer (the quick preset's name or "Custom") and a changed dot. Options are one line (label + "?" + value pill) with a gold bar on a changed row. Section titles wrap. The number dialog of percent options gets quick-value chips (they only fill the field; Apply stays separate). The search row is hidden only in the prologue | `HuntEdictWindow.Summary.cs`, `.QuickPresets.cs`, `.Options.cs` |
| M3 overview | Combat style cards carry real facts ("Retreat at n% HP", the dodge style) and "New players". A line appears when no style matches. The teaser card (number badge, next title, condition and progress bar, pages received) is not a button. The old string `HuntEdictProgression.Next` became the structured `HuntEdictSentences.Upcoming/Teaser` | `HuntEdictWindow.Overview.cs`, `Core/HuntEdictSentences.cs` |
| M4 new sentences | New-sentence dots on tabs and group chips (N1), a default group per tab: new group > remembered group > first group (N2), `EnsureVisible`, `FocusGlobalOption` scrolls the option into view and reports success, an extended `DisclosureDisplayKey`. The groups opened this session are kept by GameUI and never saved | `HuntEdictWindow.Sentences.cs`, `.Progression.cs`, `GameUI.HuntEdict.cs` |
| M5 tutorial slots | `SetGuideCaption`/`ClearGuideCaption` (44/58 reserved, `Guide caption` is a sibling of `Option area`), `SetBeats`/`ClearBeats` (folded 28, open 51/117, `edict-beat-strip`), `SetAfterSave` (`edict-save-next`), and the Core models `EdictCaption`, `EdictBeats`, `EdictAfterSave` | `HuntEdictWindow.Guide.cs`, `Core/HuntEdictSentences.cs` |
| M6 skills and policy | Passive icons are rounded squares, the equipped subtitle is green, titles use the body colour (gold when chosen), cards match the list width, and the slot bubble sits above its slot (below when there is no room). In portrait the policy page puts the explanation and progress button first and the example below, plus an observation progress bar and a leaner prologue example (status 2 lines, caption 3 lines, no cooldown or effect rows) | `.Skills.cs`, `.SkillPresets.cs`, `.CombatPreview.cs`, `.Progression.cs`, `.Overview.cs` |
| M7 verification and records | Contract smoke, gate visibility checks in the tutorial smoke, evidence captures, this record, the wiki record | chapter 6 |

## 3. Name contract

- **Kept (checked by the contract smoke at every disclosure stage):** the ten `edict-tab-<id>` buttons (only the disclosed ones, inside `Main tabs`, the centre ray hits the tab itself, no clipped text), `Hunt Edict window`, `Main tabs`, `Option area` (a direct child of the window), `Fixed save controls`, `Selected edict group`, `edict-group-<ids[0]>` (outside the scroll, no overlap), `edict-quick-picker-<scope>`, `edict-option-<id>`, `edict-save` and `edict-revert`.
- **New (4):** `edict-teaser` (not a button), `edict-beat-strip`, `Guide caption` (a RectTransform above `Option area`, its sibling), `edict-save-next`.
- **Decorative (not a contract):** `New sentence dot`, `Tab marker`, `Fact chip`, `Changed mark`, `Teaser row`, `Teaser number`, `Beat dot`, `Observation progress`, `Quick value <n>`. None is a button, or they have `raycastTarget=false`.

The contract smoke (`RuntimeEdictContractSmoke`) walks nine stages from a fresh account right after the mandatory map up to rift 20 with the rune guide, at five sizes (440×956, 956×440, 1600×900, 1600×1000, 2100×900) in Korean and English. At two sizes it also walks every group, checks that focus leaves the control inside its viewport, and drives the guide caption, the beat strip and the after-save button. At every stage it checks that browsing does not change the draft.

## 4. Where it differs from the mockup, and decisions

1. **The 12 seconds of the after-save button.** No timer repaints (the post-save UI refresh rules). `Update` only hides the button with `SetActive(false)`; the "✓ Saved" text stays until the next repaint. The `edict-save` and `edict-revert` instances do not change.
2. **Pages received.** The teaser shows "n/11 pages" only for accounts that already received a page (the chapter engine exists now, but a bare 0/11 would be strange). This is the default of design chapter 15 item 3 and needs a user decision.
3. **Caption name.** `Guide caption` was adopted (the recommendation of design chapter 15 item 2). It is not agreed with the tutorial author yet.
4. **Search row.** Hidden only in the prologue (default of design chapter 15 item 1). Whether to hide it in the early game as well is undecided.
5. **The change-count chip, order ↑↓ and the "Cancel/OK" close dialog were not ported** (design chapter 5). The existing `Session.Dirty`, drag cards and `RequestLeave` are used.
6. **Cleaned existing code.** The unused `QuickGroupSummary` and `VisibleSummary`, and `HuntEdictProgression.Next`, were deleted. A group description paragraph used to count the retired id (`autoEquip.preserveEffects`) as "not disclosed" (auto equip groups showed the hint line instead of their description); it now follows the same rule as search results.
7. **Text height.** In this font `Text.preferredHeight` is a little smaller than the height a `Truncate` line needs. Measured rectangles got +2 (the teaser title line without it disappeared in portrait).
8. **Legacy accounts.** The group editor (chips, option rows, dialogs) is shared with legacy accounts, so they see the new look. The legacy tree screen and the old editor are unchanged.

## 5. Impact on smokes and tests

Most of design chapter 12 held. What was already red before the change (the tutorial-merged baseline) came from fixtures that cannot open the edict for a staged account, and from stale assertions.

| Target | Before | Cause | Action |
| --- | --- | --- | --- |
| `RuntimeEdictOverviewSmoke` | Failed | A new account has no style cards disclosed | Access fixture (`edictLegacyAccess`); the clip message now shows text and heights |
| `RuntimeHuntEdictSaveLayoutSmoke` | Failed | The mandatory tutorial blocked the window; a stale `ranks` edit | Fixture; `SetGlobal` changes the draft |
| `RuntimeSkillTreeSmoke` (Sections, Menu, Presets) | Failed | Styles, details and the repeat tab were not disclosed; the default-lock allow list lacked the repeat tab | New `OpenEdict` fixture, allow list, the window's group description fix |
| `RuntimeSkillTreeSmoke.CheckMenu` | (new design) | The slot bubble moved above its slot | Slots assert above/below, nodes keep beside |
| `RuntimeEdictQuickPresetSmoke` | Failed | Counted the potion group, which has no preset picker | Fixture, group filter |
| `RuntimeSharedUiSmoke` | Failed | Mandatory tutorial state, groups without a picker, a cross-category search chip, a removed `edict-group-back` | Fixture and assertion fixes. Its edict part passes; it then fails on an equipment shop slot size (40×40), unrelated to the edict and left alone |
| `RuntimeTutorialSmoke.Progression` | Failed | On the tutorial branch the combat tab opens at rift 4 | Corrected to rift 4. **It times out at the town notice card step**, which it also does before the change (not fixed) |
| `RuntimeTutorialSmoke` | — | — | Added a check that gate targets and the controls a lesson presses are inside the safe area and any scroll viewport. It exposed skill cards 4 wider than their list, which was fixed |
| `HuntEdictSentenceTests` (6 new) | — | — | Ladder walk, page count and legacy, new-sentence rules, hidden and prologue exclusion, only shown options count, a quick preset does not perform the guide of a closed rule |

## 6. Verification

Verification is for `0ebf672d`; 6.1 is the related checks right after the change and 6.2 the final full checks. Real devices and human comprehension were not checked.

### 6.1 Related checks right after the change

- 187 related EditMode tests pass (including the 6 of `HuntEdictSentenceTests`: the ladder walk, new-sentence rules, protection against a closed rule's guide, page count and legacy). `LocalizationTests` confirmed that the new Korean literals match `en.txt`.
- `python3 tools/check_ui_contract.py`, `python3 tools/test_ui_contract.py` (11 tests) and `python3 tools/check_ui_refresh.py` pass.

### 6.2 Final full checks (`0ebf672d`)

- **Full EditMode 5183/5183 pass** (2236 s). That is 18 more than the last record of `main` before the merge (5165), and 6 of them are this work's tests. It ran on a clone of the work folder, so documentation edits made meanwhile could not leak in.
- **macOS development-build smokes** (same commit; the smokes drive the sizes themselves):

| Smoke | Result | Note |
| --- | --- | --- |
| `EdictContractSmoke` (new) | Pass | 9 stages × 5 sizes × Korean and English, group walk, focus, caption, strip, after-save button |
| `EdictOverviewSmoke`, `EdictSaveLayoutSmoke`, `EdictSectionsSmoke` | Pass | Includes frame, group chips, options, overlap and clipping at 5 sizes × 2 languages |
| `SkillPresetSmoke`, `SkillMenuSmoke`, `QuickPresetSmoke` | Pass | The policy screen, the slot bubble, the quick preset and custom walk |
| `ButtonUxSmoke`, `UiStyleSmoke` | Pass | |
| `SharedUiSmoke` | Fail | Its edict part (group walk, search, choice, save, rotation, language) passes and it then stops at an **equipment shop slot size**. Unrelated to the edict; before the change it stopped earlier, at a fixture |
| `EdictProgression` classes 0, 1, 2 | Fail | The prologue lesson part (with the gate visibility check) passes and it times out at the town notice card step. The pre-change baseline stops at the same point |
| `RecommendedEquipmentSmoke`, `LanguageSmoke`, `RiftEntrySmoke` | Fail | Same place and same message as the pre-change baseline (an undisclosed auto-equip tab fixture, the town menu, a save validation). Unrelated to this change |

- After the final full checks only smoke code changed (added captures, one stale assertion in the progression smoke). `EdictContractSmoke` passed again afterwards and the progression smoke stops at the same town notice point. The window code did not change.
- **Not run:** the whole game's smoke bundle (about a hundred smokes unrelated to the edict window), the v2 tutorial smoke (needs a local LiveOps server) and `EdictControlsSmoke`, the contract smoke for classes other than the Warrior, Android, web and real devices, performance measurement, human comprehension.

### 6.3 Evidence

Reference images are in `HuntEdictOverhaulEvidence/` with SHA-256 in [SHA256SUMS.txt](HuntEdictOverhaulEvidence/SHA256SUMS.txt). Representative screens out of 41:

**Overview (disclosure stage rift 3: new-sentence dot and teaser)**

![overview-rift3-956x440-ko](HuntEdictOverhaulEvidence/overview-rift3-956x440-ko.png) ![overview-rift3-440x956-ko](HuntEdictOverhaulEvidence/overview-rift3-440x956-ko.png) ![overview-rift3-956x440-en](HuntEdictOverhaulEvidence/overview-rift3-956x440-en.png) ![overview-rift3-440x956-en](HuntEdictOverhaulEvidence/overview-rift3-440x956-en.png)

**Group detail + quick preset (combat) / group detail (survival)**

![group-combat-956x440-ko](HuntEdictOverhaulEvidence/group-combat-956x440-ko.png) ![group-combat-440x956-en](HuntEdictOverhaulEvidence/group-combat-440x956-en.png) ![group-survival-956x440-ko](HuntEdictOverhaulEvidence/group-survival-956x440-ko.png) ![group-survival-440x956-en](HuntEdictOverhaulEvidence/group-survival-440x956-en.png)

**Dialogs: number entry (quick-value chips) / quick preset**

![dialog-number-440x956-en](HuntEdictOverhaulEvidence/dialog-number-440x956-en.png) ![dialog-number-956x440-ko](HuntEdictOverhaulEvidence/dialog-number-956x440-ko.png) ![dialog-quick-440x956-ko](HuntEdictOverhaulEvidence/dialog-quick-440x956-ko.png) ![dialog-quick-956x440-en](HuntEdictOverhaulEvidence/dialog-quick-956x440-en.png)

**Skills tab**

![skills-956x440-ko](HuntEdictOverhaulEvidence/skills-956x440-ko.png) ![skills-440x956-en](HuntEdictOverhaulEvidence/skills-440x956-en.png)

**Prologue policy screen (observation bar) and the early town tabs**

![policy-prologue-956x440-en](HuntEdictOverhaulEvidence/policy-prologue-956x440-en.png) ![policy-prologue-440x956-ko](HuntEdictOverhaulEvidence/policy-prologue-440x956-ko.png) ![town-early-956x440-ko](HuntEdictOverhaulEvidence/town-early-956x440-ko.png) ![town-early-440x956-ko](HuntEdictOverhaulEvidence/town-early-440x956-ko.png)

**PC sizes (overview, rift 12)**

![overview-rift12-1600x900-ko](HuntEdictOverhaulEvidence/overview-rift12-1600x900-ko.png) ![overview-rift12-1600x1000-ko](HuntEdictOverhaulEvidence/overview-rift12-1600x1000-ko.png) ![overview-rift12-2100x900-ko](HuntEdictOverhaulEvidence/overview-rift12-2100x900-ko.png)

## 7. What remains on the tutorial side (Phase 4)

The window provides the following and nothing calls it yet. It maps onto requests T1-T7 of design chapter 10.

| Request | Window side | Tutorial side |
| --- | --- | --- |
| T1 off-screen control counts as missing | `EnsureVisible(name)`, called by `FocusGlobalOption` | `PrologueGate` and `LessonTarget` must look at the viewport intersection |
| T2 ring → save | — | Move `edict-save` to the front in `RefreshTutorialAnchor` |
| T3 default-settings lock | `FocusGlobalOption` returns `bool` (`DefaultSettingsLocked` is still private) | Point the caption at `edict-default-settings` when locked |
| T4 caption | The `GuideCaption` RectTransform, `SetGuideCaption`, `ClearGuideCaption` | `PrologueGate` docks into this band instead of a floating card |
| T5 beats and after save | `SetBeats`, `ClearBeats`, `SetAfterSave` | Pass content from `GameUI.EdictNotices` and the save closure |
| T6, T7 | — | Keep the disclosed-rule decision of `EdictSaved`; review the card selection order |

## 8. Open decisions and risks

- The caption's name and ownership and the gate docking must be agreed with the tutorial author. Until it docks, the prologue's floating card still covers the preset tabs as before.
- Not decided: the pages-received condition (chapter 4 item 2), how far the search row is hidden (chapter 4 item 4), and whether "Check in training" after a save really opens the training ground.
- Performance was not measured. Group chips now compute the quick-preset match per chip, so one `Repaint` may cost more. Frame updates were not added.
- Legacy accounts also get the new group editor look.

## 9. Artifacts and cleanup

- Evidence images and hashes are in `HuntEdictOverhaulEvidence/`. The development builds (about 600 MB each) and the smoke outputs live under `Builds/HuntEdictNative/` in the work folder, with owner, path, size and retention in `artifact-lifecycle.json` in that folder.
- Nothing was deleted permanently. Removing the work worktree and the builds is left as an item that needs approval.

## 10. Change log

- 2026-10-05: recorded the port and its verification.
