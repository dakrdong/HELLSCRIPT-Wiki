# Hunt Edict UI Overhaul — Tutorial Wiring Record (Phase 4)

Updated: 2026-10-05 · [한국어](Hunt_Edict_Tutorial_Wiring.md) · [Native port record](Hunt_Edict_UI_Overhaul_Native.en.md) · [Design document](../Design/Hunt_Edict_UI_Overhaul.en.md) · [Tutorial UI brief](../Design/Tutorial_Hunt_Edict_UI_Brief.en.md)

Status: the tutorial-side requests T1-T7 of design document chapter 10 are in. The tutorial code (`GameUI`) is connected to the slots the native port (Phase 3) made (guide caption, three-beat strip, after-save button), and the gate can no longer be left covering a control that is scrolled out of reach. The Ish NPC, the chapter scripts and the scroll window (chapters D-I) are not part of this work, so the captions are system sentences. No save field, disclosure rule, save path or control name changed.

## 1. At a glance

| Item | Content |
| --- | --- |
| Branch and base | `claude/hunt-edict-tutorial-wiring`, stacked on `1ae06fb8`, which is `46523f6a` of the native port branch (`claude/hunt-edict-native-port`, PR #53) with `main` (`a94bb636`) merged in |
| Commits | `a3186981` the tutorial wiring (code, tests, smokes), `1ae06fb8` merge of `main` (PR #55, skill tree lineage) into the port branch, `0637026d` merge of that port branch into this one, then this record and the evidence |
| Size | 18 files under `Assets` (without .meta), +885 / −53 lines. New files: one Core file (`EdictGuidance`), one `GameUI` partial, one display helper (`UiVisibility`), two EditMode tests, one smoke and one smoke partial |
| Unchanged | Save fields, the disclosure rules (`HuntEdictProgression`), the save path (`CommitHuntEdict`), the ten tab ids and every `edict-*` name, the area names such as `Option area` and `Preset explanation`, the meaning of `HasDialog`, the prologue lesson step decisions (`SkillLessonStep`, `SurvivalLessonStep`, `Progressive*Step`) |
| New names | None. It uses `Guide caption`, `edict-beat-strip` and `edict-save-next` from Phase 3. The prologue caption's round badge (`Guide halo`) only got the edict glyph |

## 2. Implementation by request

| Request | Implementation | Main files |
| --- | --- | --- |
| T1 The gate treats a control it cannot reach as missing | The gate's opening is no longer the control's screen rectangle but **the part a pointer can reach** (the rectangle cut by masks, scroll viewports and the safe area). If less than a quarter can be reached the control counts as missing and the cover lets go after 2 seconds. `LessonTarget` scrolls a control into view whenever part of it is hidden (at most four times a second) | `UiVisibility.cs`, `PrologueGate.cs`, `GameUI.EdictGuidance.cs`, `GameUI.TutorialStaging.cs` |
| T2 The ring moves to Save once something changed | The ring order of a rule guide is decided in one place (`EdictGuidance.RingNames`). Once the value of an option the guide teaches differs between the draft and what is saved, `edict-save` leads. The number dialog's `edict-number-apply` always leads because the dialog covers everything else | `Core/EdictGuidance.cs`, `GameUI.Tutorials.cs`, `HuntEdictWindow.Guide.cs` |
| T3 The default-settings lock | While it is on, the ring of a rule guide is `edict-default-settings` alone and the caption says why. If the guide started while locked, the option is remembered and opened right after the switch is turned off | `EdictGuidance.cs`, `GameUI.EdictGuidance.cs`, `HuntEdictWindow.Guide.cs` (`DefaultsLocked`) |
| T4 The caption in the window's band | The prologue lesson's Voice lines go into the `Guide caption` band (`Hard`) and the floating card is hidden. The gate leaves the band undimmed. A rule guide writes one sentence for the control the ring sits on into the same band | `GameUI.TutorialStaging.cs`, `GameUI.EdictGuidance.cs`, `PrologueGate.cs`, `HuntEdictWindow.Guide.cs` |
| T5 Beats and the after-save step | Only the guides that can quote the last fight (F05, E05) fill the strip. After a save the third beat names the option that changed. Right after a save the footer offers one step: "Check in training" when the training ground is open, otherwise "Watch the next hunt", and nothing during a fight | `EdictGuidance.cs`, `GameUI.EdictGuidance.cs`, `GameUI.HuntEdict.cs` |
| T6 "Acted on" counts only disclosed rules | Already true (`TutorialProgress.EdictSaved` checks `GuideVisible`). Not changed; one existing `HuntEdictSentenceTests` case pins it | `HuntEdictSentenceTests` |
| T7 Card order | In list order, position (rift 5) was announced before the low-HP response (rift 3). Cards are now chosen in disclosure-stage order | `EdictGuidance.NoticeRank`, `GameUI.EdictNotices.cs` |

## 3. How the ring and the caption move

When a rule guide ("Set it up") opens, every 0.25 s the ring goes to **the first pressable control** in the order below and the caption band says the sentence that fits it. The band appears once when the guide starts and goes when it ends; when the sentence changes only the text inside the band changes (the window is not repainted).

| Situation | Ring control | Caption |
| --- | --- | --- |
| The default settings are on | `edict-default-settings` | The default settings are on, so this cannot be changed. Turn the default settings off first. |
| The number dialog is open | `edict-number-apply` | Set the value, then press 'Apply'. |
| An option the guide teaches changed (unsaved) | `edict-save` | Save the value you changed. |
| The option or its quick picker is visible | `edict-option-<option>` or `edict-quick-picker-<scope>` | Tap '<label>' to change its value. / Tap the choice under '<group>' to change it. |
| On another group or tab | `edict-group-<group>` then `edict-tab-<tab>` | Open the '<group>' group. / Open the '<tab>' tab. |
| Combat style (E02) | a style card that is not chosen yet, then Save | Pick one combat style card. |
| New skill (F06) | `edict-skill-equip`, Save, `edict-tab-skills` | Equip the new skill in a slot. / Save / tab |

In the prologue `TickPrologueLesson` puts the sentence into the band every frame in the same way. At the first step, where the window does not exist yet (the edict button in the dock), the floating card still speaks. **While a dialog is open** the whole window is dimmed and so is the band, so the floating card speaks too.

## 4. Where it differs from the plan, and decisions

1. **The caption is a system sentence.** The label is "Guide", and only the prologue says "Voice from Above". Ish's lines go into the same place when the chapters (D-I) are implemented; the code only has to change the sentence.
2. **Only F05 and E05 fill the three beats.** They are the only guides that can quote the last 5 seconds' HP and damage taken; the chapter scripts write the others. An empty strip takes no room.
3. **The step after a save.** Opening the training ground is wired to a real walk (the default of design chapter 15 item 6). With the footer button the old toast ("Watch how the setting you just changed behaves in the next hunt") would repeat itself, so it is not shown; a save during a fight has no button, so it keeps the toast.
4. **Card order (T7).** Rule guides go by the stage they open at; the combat style (E02) is 2, the cursed chest (E08C) 8 and auto repeat (H11) 15. The skill guides (F06, H07, H08), F07 and H10 stay first, as now.
5. **The lock applies only to option-rule guides and the style guide.** Skill management stays open under the default settings, so F06 keeps its own path. The switch that sets the lock is visible from rift 20 (or on legacy accounts), so this is in practice a legacy-account case.
6. **The prologue's number-dialog step.** A dialog dims the window and the band becomes unreadable, so the floating card is used alongside it. Changing the band's sentence while a dialog is open leaves the dialog and its input alone (`CaptionKeepsTheDialog`).
7. **The gate's opening.** It used to be the control's unclipped rectangle; now it is the part that can be reached. A control that is only partly visible is pressable exactly where the opening is.
8. **Why the progression smoke used to stall.** The Phase 3 record said "times out at the town notice card step". The cause was the **daily attendance window** that opens there and covers the card (`timeout.png`, the same on the pre-change baseline). The smoke fixture now hides that window for the day.
9. **Saving a combat style also marks other guides as acted on (existing behaviour, not changed).** A style preset writes the low-HP, target and position values too, so `EdictSaved` records the guides of those disclosed rules (E05, E04, E03) as acted on, and a value change that leaves a style no longer matching counts for E02 as well. That is why the smoke runs the style guide last and resets its record first.

## 5. Impact on smokes and tests

| Target | Change | Reason |
| --- | --- | --- |
| `EdictGuidanceTests` (new) | Ring order, lock, captions, card order, beats, result, after-save step | Pure logic, checked without a scene |
| `PrologueGateTests` (new) | The reachable rectangle cut by masks, scroll viewports and the safe area; the dim rectangles | The geometry the gate relies on |
| `RuntimeEdictGuideSmoke` (new) | Through the real notice card: ladder order, caption, ring and Save, an unchanged autosave not rebuilding the window under the ring, lock, dialog preserved, gate lets go | The contract of the tutorial wiring |
| `RuntimeTutorialSmoke` (`CheckLessonCaption`) | When a lesson targets a control in the window: the band holds the sentence, the floating card is hidden, the band is not dimmed and covers no lesson control | The prologue caption move |
| `RuntimeTutorialSmoke.Progression` | Hides the attendance window for the day once the arrival dialogue is closed | Item 8 above |

## 6. Verification

Verification is for the integrated tree (`0637026d`: this branch's code `a3186981` on the port branch with `main` `a94bb636`, that is PR #55's skill tree lineage, merged into it). 6.1 is the related checks during the work and 6.2 the final full checks. Real devices and human comprehension were not checked.

### 6.1 Related checks during the work

- 162 related EditMode tests pass (`EdictGuidanceTests`, `PrologueGateTests`, `HuntEdictSentenceTests`, `LocalizationTests`, `HuntEdictProgressionTests`, `TutorialProgressionTests`, `TutorialChapterTests`, `StoreViewBindingTests`, `HuntEdictOverviewTests`). `LocalizationTests` checks both ways that the new Korean literals and `en.txt` agree. A format string with no Korean (`'{0}' {1} → {2}`) was rejected and no longer goes through `Loc`. One more case was added to `EdictGuidanceTests` afterwards and the full run below includes it.
- `python3 tools/check_ui_contract.py`, `python3 tools/test_ui_contract.py` (11 tests) and `python3 tools/check_ui_refresh.py` pass (12 bindings guarded, 160 UI source files inventoried).

### 6.2 Final full checks (`0637026d`)

- **Full EditMode: 5203 of 5204 pass, 0 fail, 1 skipped** (2119 s). The skipped one is another task's measurement tool (`ClassSkillPassiveMeasurement.MeasureEveryDesignBuild`). It ran on a clone of the work folder, so documentation edits made meanwhile could not leak in. This work's 17 new tests (`EdictGuidanceTests` 9, `PrologueGateTests` 8) all pass.
- **macOS development-build smokes** (same commit; the smokes drive the sizes themselves):

| Smoke | Result | Note |
| --- | --- | --- |
| `EdictGuideSmoke` (new) | Pass | Through the real notice card: ladder order (the put-off style card, then low HP, target, position, then the style card brought back), caption, ring and Save, an unchanged autosave, lock and unlock, the dialog preserved, the gate letting go and holding again |
| `EdictProgression` classes 0, 1, 2 | Pass | From the prologue lesson (with the caption check at 10 size/language profiles) through to the town notice cards. This smoke used to stall on the attendance window; it now passes end to end for the first time |
| `RuneUnlockSmoke` | Pass | The rune introduction, which uses the same gate; 4 runs, landscape and portrait, Korean and English |
| `EdictContractSmoke` | Pass | 9 stages × 5 sizes × Korean and English |
| `EdictOverviewSmoke`, `EdictSaveLayoutSmoke`, `EdictSectionsSmoke` | Pass | |
| `SkillPresetSmoke`, `SkillMenuSmoke`, `QuickPresetSmoke` | Pass | On top of the merged skill tree lineage |
| `ButtonUxSmoke`, `UiStyleSmoke` | Pass | |
| `SharedUiSmoke` | Fail | Stops at `Nonstandard slot on Equipment shop` (40×40) in the equipment shop, outside the edict window. The pre-change player stopped earlier on a stale fixture, so I could not confirm this one is pre-existing |
| `RecommendedEquipmentSmoke`, `LanguageSmoke`, `RiftEntrySmoke` | Fail | The same messages as the pre-change baseline (`edict-tab-autoEquip` missing, a NullReference, `Missing pointer target`). Unrelated to this change |

- After the full run only documentation and evidence images changed; no code.
- **Not run:** the whole game's smoke bundle (about a hundred smokes unrelated to the edict window), the v2 tutorial smoke (needs a local LiveOps server) and `EdictControlsSmoke`, the contract smoke for classes other than the Warrior, **the F06 (new skill) ring path at runtime** (unit tests only), Android, web and real devices, performance and a matched before/after save-refresh measurement, human comprehension. The added per-frame work is a few `GetComponent` calls while the gate is on (the prologue and the rune introduction) and there is no new update interval, but it was not measured.

## 7. Evidence

The reference images are in `HuntEdictTutorialEvidence/` with SHA-256 in [SHA256SUMS.txt](HuntEdictTutorialEvidence/SHA256SUMS.txt). There are 21 in total. They are screens the smokes captured, not real-device screens.

**Prologue policy screen: the Voice's sentence is in the caption band and does not cover the preset tabs** (landscape and portrait, Korean and English)

![prologue-policy-956x440-ko](HuntEdictTutorialEvidence/prologue-policy-956x440-ko.png) ![prologue-policy-440x956-ko](HuntEdictTutorialEvidence/prologue-policy-440x956-ko.png) ![prologue-policy-956x440-en](HuntEdictTutorialEvidence/prologue-policy-956x440-en.png) ![prologue-policy-440x956-en](HuntEdictTutorialEvidence/prologue-policy-440x956-en.png)

**Prologue survival lesson: the band is bright on the option step; on the number-dialog step the window is dimmed, so the floating card speaks as well**

![prologue-survival-option](HuntEdictTutorialEvidence/prologue-survival-option.png) ![prologue-survival-number](HuntEdictTutorialEvidence/prologue-survival-number.png)

**Rule guide E05 (portrait, Korean): start → after the change the ring moves to Save → after the save (third beat and next step)**

![guide-E05-440x956-ko-1-start](HuntEdictTutorialEvidence/guide-E05-440x956-ko-1-start.png) ![guide-E05-440x956-ko-2-save](HuntEdictTutorialEvidence/guide-E05-440x956-ko-2-save.png) ![guide-E05-440x956-ko-3-saved](HuntEdictTutorialEvidence/guide-E05-440x956-ko-3-saved.png)

**Rule guides E04 (landscape, English) and E03 (portrait, English)**

![guide-E04-956x440-en-1-start](HuntEdictTutorialEvidence/guide-E04-956x440-en-1-start.png) ![guide-E04-956x440-en-2-save](HuntEdictTutorialEvidence/guide-E04-956x440-en-2-save.png) ![guide-E04-956x440-en-3-saved](HuntEdictTutorialEvidence/guide-E04-956x440-en-3-saved.png) ![guide-E03-440x956-en-1-start](HuntEdictTutorialEvidence/guide-E03-440x956-en-1-start.png) ![guide-E03-440x956-en-2-save](HuntEdictTutorialEvidence/guide-E03-440x956-en-2-save.png) ![guide-E03-440x956-en-3-saved](HuntEdictTutorialEvidence/guide-E03-440x956-en-3-saved.png)

**Combat style guide E02 (landscape, Korean)**

![guide-E02-956x440-ko-1-start](HuntEdictTutorialEvidence/guide-E02-956x440-ko-1-start.png) ![guide-E02-956x440-ko-2-save](HuntEdictTutorialEvidence/guide-E02-956x440-ko-2-save.png) ![guide-E02-956x440-ko-3-saved](HuntEdictTutorialEvidence/guide-E02-956x440-ko-3-saved.png)

**Default-settings lock: the switch leads, and the option opens once it is off / the gate letting go of a control nobody can reach**

![guide-locked-956x440-ko](HuntEdictTutorialEvidence/guide-locked-956x440-ko.png) ![guide-unlocked-956x440-ko](HuntEdictTutorialEvidence/guide-unlocked-956x440-ko.png) ![gate-letting-go-956x440-ko](HuntEdictTutorialEvidence/gate-letting-go-956x440-ko.png)

## 8. What remains

- **Chapters D-I.** The Ish NPC, the scroll window and the chapter scripts. Replacing the caption and beat sentences with Ish's lines happens in the same place (`EdictGuidance.Caption`, `Beats`).
- Guides other than F05 and E05 have no beat text; the chapter scripts decide.
- Not checked on a real device. The flow where the band changes while the number dialog is open was only exercised with desktop input, not with touch and the soft keyboard.

## 9. Open decisions and risks

- Whether the after-save step should really walk to the training ground needs the user's confirmation. To go back to a toast, only the `Run` of `OfferNextStep` changes.
- The lower bound for the reachable part (a quarter) is new; the release time (2 seconds) is the existing value.
- The rune board introduction uses the same gate, so the same rule applies when its target is only partly visible. The rune introduction smoke (`RuneUnlockSmoke`) passed in all four combinations of landscape and portrait, Korean and English.

## 10. Artifacts and cleanup

- The 21 evidence images and their hashes are in `HuntEdictTutorialEvidence/`. The development builds (about 605 MB each) and the smoke outputs live under `Builds/HuntEdictTutorial/` in the work folder, with path, size and retention in `artifact-lifecycle.json` in that folder.
- Nothing was deleted permanently with approval. Removing the work tree, the builds and the scratch clone (`final-proj-p4`) after the merge is left as an item that needs approval. One exception is not hidden: while iterating on builds I removed one clone of an old build (`app-r1`) before recording it. It is regenerable, but it broke the cleanup rule's order, and every later build was kept.

## 11. Change log

- 2026-10-05: recorded the tutorial wiring and its verification.
