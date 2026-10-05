# Hunt Edict Tutorial Summary — Input for the UI Overhaul

Updated: 2026-10-04

[한국어](Tutorial_Hunt_Edict_UI_Brief.md)

## 1. Purpose

- This is the input document for the Hunt Edict UI overhaul. It lists **what the tutorial teaches on the edict screen, which controls it points at, and what it asks of the screen**.
- When the UI changes, the tutorial must be adjusted to match. The UI author should honor the contract in chapter 4 and the requests in chapter 5, and leave the handoff mapping from chapter 7 when finished. Tutorial work (chapter content, edict sentence staging) then continues on top of the new UI.
- The parent design is [Tutorial Chapter Design](Tutorial_Chapters.en.md). Disclosure data and rules follow [Hunt Edict staged disclosure](../Implementation/Hunt_Edict_Progression.en.md). The earlier UX direction is [Hunt Edict UX Redesign](Hunt_Edict_UX_Redesign.en.md).
- This document was written by reading the code. It does not describe anything confirmed by running the game.

## 2. At a glance

1. A new player sees the edict **one thing at a time**. Of the 152 global options only one (the potion HP threshold) is visible at first, and one sentence opens after each rift clear. Access once granted is never revoked.
2. The tutorial does three things on the edict screen.
   - **Forced prologue lesson**: it covers everything except one designated control (`PrologueGate`) and points at it with a gold outline (`TutorialAnchorRing`).
   - **Staged disclosure**: it opens tabs, groups and options by the disclosure rules, and draws what is not open not at all, with no slot or gap.
   - **Town notice card → open window → highlight**: after a rift result it shows one card in town, and `Try configuring` opens and highlights the item.
3. The tutorial finds the screen by **control names (`edict-*`) and window state**. If a name changes or disappears the guidance stalls. 4.1 is that list.
4. The UI must carry three densities: prologue (one control) → early game (3 to 4 tabs, quick presets only) → mid and late game (direct settings, presets, search, sharing). See 3.1.
5. The disclosure order is arranged as an importance ladder (sentences 1 to 15). On this branch (`claude/tutorial-chronicle`) low-HP response moved to Rift 3, position and distance to Rift 5, and the detailed conditions were split into three groups. Chapter 8 separates the current state.

## 3. Flow

### 3.1 Three screen densities

| Level | When | Visible tabs | Editing |
| --- | --- | --- | --- |
| L0 Prologue | Mandatory map | Skills → Survival, one at a time | One first-skill node, one slot, one potion HP threshold |
| L1 Early | Arrival in town to Rift 5 | Overview, Skills, Survival, plus Combat from Rift 4 | Quick presets (official choices) only. Direct editing only for potion HP threshold, auto potion and defense/escape skills |
| L2 Middle | Rift 6 to 15 | Presets (6), Loot (7), Explore (8), Bag (9), Auto equip (12), Repeat (15 + rune guide done) | Direct settings of disclosed groups, skill details, attack order, local presets, training comparison |
| L3 Late | Rift 18 onward | All tabs | Detailed conditions, action order and detailed exploration, settings sharing (20) |

- The tab rule is `HuntEdictProgression.Tab` in code. Overview, Skills and Survival are always open, Presets opens at Rift 6, and any other tab opens once one option of its groups is disclosed.
- Before a hero reaches level 2 only the first skill node is shown. Slots show one more than the number of equipped skills (one at first), up to four. From level 40 an ultimate slot is added.
- Gem, core, elixir and cursed-chest items appear only after the actual acquisition is confirmed, separately from the disclosure stage.
- While recommended mode (`hero.useRecommendedEdict`) is locked, only the Overview, Skills and Repeat tabs are open and the other tabs are disabled.

### 3.2 Forced prologue lesson (edict window)

In the mandatory map (`tutorialFlowVersion==3`) the edict window opens with a `tutorialLesson` (RunState). The window then restricts the following.

- Only one tab is shown: `Skills` in the first phase and `Survival` in the boss-fight intervention phase.
- The skill tree holds only the class's first-skill node (Warrior W01, Ranger A01, Mage M01), and there is one slot.
- Only the option `survival.potionHpPercent` and the group that holds it are open.
- Only the two comparison choices of the first skill are open as usage presets: stand and edge for the Warrior, steady and pack for the Ranger and Mage.
- `PrologueGate` blocks input outside the designated control, and Back (Esc) is blocked through `ContentWindowHost.BackLocked`. If the designated control cannot be found for 2 seconds the cover is released.

The player follows the order below. A control in the table must really exist on screen (`interactable` and active in the hierarchy) for the guide to point at it. The conditions were read from `ProgressiveSkillLessonStep` and `ProgressiveSurvivalLessonStep` in `GameUI.TutorialComparison.cs`.

| Order | Player action | Designated control | Window state condition |
| --- | --- | --- | --- |
| 1 | Open the edict | `menu-hunt-edict` (content dock) | No window |
| 2 | Skills tab | `edict-tab-skills` | `SelectedTab != "skills"` |
| 3 | Return to the tree if on the usage screen | `edict-policy-back` | `!ManagingSkills` |
| 4 | Select the first skill | `edict-skill-<skillId>` | `SelectedSkill` is not the first skill |
| 5 | Spend 1 skill point | `edict-skill-rank-up` | Rank 0 |
| 6 | Equip to slot 1 | `edict-skill-equip` | Not equipped |
| 7 | Save | `edict-save` | The equipped state is unsaved |
| 8 | Open the equipped skill's slot | `edict-active-slot-0` → `edict-slot-policy` | After saving, depending on whether a dialog is open |
| 9 | Check approach A | `edict-preset-tab-<A>` → `edict-preset-activate` | Lesson step 0 |
| 10 | Observe A and proceed | Area `Preset explanation` (combat example with play, pause, restart) → `edict-starter-next` "I saw the action" | Active once observation completes, step 0→1 |
| 11 | Activate B and confirm the save | `edict-preset-tab-<B>` → `edict-preset-activate` → `edict-starter-next` "Confirm mode B saved" | Step 1→2 |
| 12 | Observe B and proceed | `Preset explanation` → `edict-starter-next` | Step 2→3 |
| 13 | Activate the preferred approach and confirm | Area `Option area` (window body) → `edict-starter-next` "Hunt with this mode" | Step 3→4 |
| 14 | Close and resume combat | `edict-close` | Step 4 or later |
| 15 | (Boss-fight intervention) reopen the edict | `menu-hunt-edict` | Potion HP threshold is not 60 |
| 16 | Survival tab | `edict-tab-survival` | `SelectedTab != "survival"` |
| 17 | Press the 40% potion HP threshold to change it | `edict-option-survival.potionHpPercent` | Value is not 60 |
| 18 | Type a number and apply | `edict-number-input` (type 60) → `edict-number-apply` | `HasDialog` |
| 19 | Save | `edict-save` | Draft value is 60 and unsaved |
| 20 | Close | `edict-close` | Saved value is 60 |

- When the potion is actually used at the 60% threshold in the boss fight the lesson step becomes 5, and defeating the gatekeeper completes it.
- The older flow (v2) uses `edict-group-<optionId>`, `edict-quick-picker-<scope>` and `edict-quick-choice-<choiceId>` in its forced steps. It remains for resuming v2 saves and for legacy accounts, so those names are included in 4.1.
- The two designated areas are named rectangles, not buttons. `Option area` is the window body root, and `Preset explanation` is the row on the usage screen that holds the explanation, the combat example and the progress button. The gate lets input through only inside the area, so **the progress button `edict-starter-next` must sit inside `Preset explanation`.**

### 3.3 After arriving in town: the edict sentence ladder

Numbers are the sentence numbers of the [Tutorial Chapter Design](Tutorial_Chapters.en.md), ordered by importance (survival → stability → efficiency → advanced). "Where it opens" is the tab and group the guide points at, and "focus option" is the option `FocusGlobalOption` opens on `Try configuring`.

| No. | Feature | Disclosed | Guide ID | Where it opens (tab · group) | Focus option / quick preset scope |
| --- | --- | --- | --- | --- | --- |
| 1 | Potion HP threshold | Prologue | P00 | Survival · Auto potion | `survival.potionHpPercent` |
| 2 | First skill approach comparison | Prologue | P00 | Skills · first skill's usage | `skill/<first skill>` (two approaches per class) |
| 3 | Auto HP potion, default dodge | End of the first rift (win or lose) | F05 | Survival · Auto potion, Dodge by damage type | `survival.potionHpPercent`, `survival.potion`, `dodge.ground.policy` (balanced/all) |
| 4 | Combat style | Rift 2 | E02 | Overview · combat style cards | `edict-style-aggressive/balanced/careful` |
| 5 | Retreat and return at low HP | Rift **3** (was 5) | E05 | Survival · Emergency response and return | `survival.lowHp` (survival first, balanced survival, retreat only in emergency), defense/escape skill `survival.defenseSkill` |
| 6 | Target choice | Rift 4 | E04 | Combat · Target choice | `target.default` (nearest first, dangerous first, clear dense packs), `target.switch` |
| 7 | Position, distance, encirclement | Rift **5** (was 3) | E03 | Combat · Position and distance, Encirclement and herding | `position.engage` (4 kinds), `position.surrounded` (3 kinds) |
| 8 | Direct editing, presets, training comparison | Rift 6 | F07, H09, H10, H17 | Presets tab, direct settings of every disclosed group, skill details | Permissions `details`, `presets` |
| 9 | Loot pickup | Rift 7 | E07 | Loot · Equipment pickup by grade | `loot.NORMAL` (all, rare and above, legendary/set), `loot.class`, `loot.gold`, `loot.distance` |
| 10 | Exploration, cursed chest | Rift 8 (cursed chest after actually finding one) | E08, E08C | Explore · Exploration and rift progress, Cursed chest | `explore.mode`, `explore.chests`, `explore.cursedChest` |
| 11 | Bag space and protection | Rift 9 | E09 | Bag and cleanup · Low bag space | `bag.trigger`, `bag.cleanupAt`, `bag.protectEnhanced`, `bag.warehouseFull` |
| 12 | Auto equip, potion resupply | Rift 12 / 14 (legendary/set comparison once one is acquired after Rift 12) | E12, E12S, E14 | Recommended equipment, Survival · Auto potion | `autoEquip.enabled` (default OFF), `autoEquip.1.mode`, `potion.autoBuy` (default OFF) |
| 13 | Auto repeat | Rift 15 + rune guide done | H11 | Repeat settings | `repeat.enabled`, `repeat.failures`, `repeat.freeSlots` |
| 14 | Detailed conditions (3 groups, announced one group at a time in town) | Rift 18 | E18 / E18B / E18C | Survival · Special hazards and dodge method / Combat · Target keeping and pursuit / Bag and cleanup · Low bag space | `dodge.interrupt` / `target.chaseDistance` / `bag.replacementRank` |
| 15 | Action order, settings sharing | Rift 20 | E20 | Combat · Action priority | `common.order`, permission `sharing` |

- Group sizes: E18 is 20 dodge/survival options, E18B is 13 pursuit/loot options and E18C is 7 bag cleanup options, all opening together at Rift 18.
- Skill-related guides (their destination is the skills tab of the edict screen): F06 equip a new skill (from level 2, an active the hero has never equipped), H07 skill points and passives (from level 2), H08 ultimate (level 40).
- A quick preset is one choice per group. Before Rift 6 only these quick presets are used, and direct settings open at Rift 6. The exceptions are the potion HP threshold, auto potion and the defense/escape skills, which are directly editable from the start.
- Auto buy, auto disposal, auto equip and auto repeat start OFF even when disclosed, and disclosure alone does not turn them on.

### 3.4 From the notice card to completion

1. Each time the result screen (`ShowResult`) appears, `EdictResultSettled()` is called and one card is allowed. While a run is active (`game.Active`) it is reset to allowed every time, so a card can appear after finishing a rift and returning to town. After one card is shown, no more are shown until the next result screen.
2. In town (`plaza`), with no other window or popup, hints not hidden and the first rift finished, `TryEdictNotice()` shows **one** card.
   - The target is the first guide, among `E*`, F05, F06, F07, H07, H08, H10 and H11, that is open now and not yet announced, deferred, hidden or performed. The order is the `Tutorials.All` order.
   - The F05 card adds the HP change over the last 5 seconds of the previous fight and the largest damage source.
   - The card buttons are `edict-notice-configure` ("Try configuring") and `edict-notice-later` ("Later").
3. `Try configuring` makes `StartTutorialAction` open the edict window and move by guide.
   - Rule guides (E*) and E08C open the group detail with `FocusGlobalOption(<focus option>)`.
   - F05 focuses `survival.potionHpPercent`.
   - Other guides pick a tab: skill guides `skills`, H10 `presets`, H11 `repeat`, the rest `overview`.
4. While the window is open, `RefreshTutorialAnchor()` attaches a gold outline every 0.25 s to the **first active and `interactable` button found** in the name list. The outline disappears when the guide is recorded as performed.
5. Completion is decided by **a change in the saved edict** (`TutorialProgress.EdictSaved`), not by screen actions. A rule guide is recorded as performed when an option of that rule is changed and saved. For F05 it is one of the potion HP threshold, auto potion or default dodge, for E02 the combat style, and for H09 a change of the edict signature.

## 4. Contract the UI must honor

### 4.1 Control names the tutorial uses

The tutorial finds controls by these names. A `Button` counts as found only when it is `interactable` and `activeInHierarchy`.

| Name | Meaning | Used by |
| --- | --- | --- |
| `menu-hunt-edict` | Hunt Edict button in the content dock | Prologue 1, 15 |
| `edict-close` | Close the window | Prologue 14, 20 |
| `edict-save` | Save (active only when there are changes) | Prologue, F05, F06, rule guides, E02 |
| `edict-tab-<tabId>` | Tab button. tabId is `overview`, `skills`, `combat`, `survival`, `loot`, `bag`, `explore`, `repeat`, `autoEquip` or `presets` | Prologue 2, 16, F05, F06 |
| `edict-skill-<skillId>` | Skill tree node | Prologue 4 |
| `edict-skill-rank-up` / `edict-skill-equip` | Spend a point on / equip the selected skill | Prologue 5, 6, F06 |
| `edict-active-slot-0` / `edict-slot-policy` | Equipped slot / usage editing in the slot menu | Prologue 8 |
| `edict-policy-back` | Return from the usage screen to the skill tree | Prologue 3 |
| `edict-preset-tab-<presetId>` / `edict-preset-activate` | Usage tab / activate this approach | Prologue 9, 11 |
| `edict-starter-next` | Comparison-step progress button (its text changes per step) | Prologue 10 to 13 |
| `edict-option-<optionId>` | Global option row. optionId example: `survival.potionHpPercent` | Prologue 17, F05, rule guides |
| `edict-quick-picker-<scope>` | A group's quick preset button. Scope example: `global/position.engage` | Rule guides, older prologue |
| `edict-quick-choice-<presetId>` | A choice in the quick preset dialog | Older prologue |
| `edict-group-<first option id of the group>` | Enter a group | Older prologue |
| `edict-number-input` (InputField) / `edict-number-apply` | Input field / apply in the number-entry dialog | Prologue 18, F05 |
| `edict-style-<styleId>` | Combat style card on the Overview tab. styleId is `aggressive`, `balanced` or `careful` | E02 (outline on `edict-style-balanced`) |
| `edict-notice-configure` / `edict-notice-later` | Town notice card buttons (outside the edict window) | Notice card |
| `Option area` (RectTransform) | Window body root | Designated area of prologue 13 |
| `Preset explanation` (RectTransform) | Row with the usage explanation, combat example and progress button | Designated area of prologue 10, 12 |

The priority lists used when attaching the outline follow. The outline attaches to the first control found, from the front.

- F05: `edict-number-apply`, `edict-option-survival.potionHpPercent`, `edict-save`, `edict-tab-survival`
- F06: `edict-skill-equip`, `edict-save`, `edict-tab-skills`
- Rule guides and E08C: `edict-option-<focus option>`, `edict-quick-picker-<that group's scope>`, `edict-save`
- E02: `edict-style-balanced`, `edict-save`

Many other names are used by smokes and tests (search `edict-search*`, changed-only `edict-changed-only`, presets `edict-preset-*`, equipment `edict-auto-*` and `edict-storage-*`, combat example `edict-preview-*`, sharing `edict-share`, dialogs `edict-dialog-*`, and so on). When the screen changes, also check where these names are used, following chapter 6.

### 4.2 Window state the tutorial reads and actions it calls

What `HuntEdictWindow` (the unified window) exposes that the tutorial uses.

- State: `SelectedTab`, `ManagingSkills`, `SelectedSkill`, `PolicySkill`, `SelectedGroup`, `VisibleSkillPreset`, `ActiveSkillPreset`, `HasDialog`, `Session.Draft`, `Session.Dirty`, `PreviewCombat`, `ProgressivePrologue`, `QuickPresetSelection(scope)`
- Actions: `Open(..., tutorialLesson)`, `SelectTab(tabId)`, `FocusGlobalOption(optionId)`, `HandleBack()`
- Opening: `GameUI.ShowEdictEditor()` opens the unified window. The older editor `ShowLegacyEdictEditor` is used only by some smokes and has no tutorial connection.

### 4.3 How disclosure rules are reflected on screen

The disclosure decision lives in code (`HuntEdictProgression.Has/Visible/Direct/Quick/Tab`), and the window follows it as is. Keep the following even if the UI changes.

- Locked tabs, groups and options are **not drawn**. No locked placeholder and no blank. The next feature is announced only by the one-line teaser on the Overview tab (`HuntEdictProgression.Next`).
- `Direct` decides whether direct editing is allowed and `Quick` decides which quick preset choices are shown. The screen cannot bypass them to change a value. The save stage (`ValidateChange`, `ValidateTransaction`) also rejects changes to undisclosed values.
- The redraw key (`DisclosureDisplayKey`) holds hero, level, disclosed list, potion revision, edict, skills, presets and lesson step. A new display state must be added to that key to trigger a redraw.
- Every string is drawn by translating the Korean source through `Loc.T`/`Loc.F`. New strings get an English entry in `en.txt`.

### 4.4 Behavior that must not break

1. Control names must be stable and unique inside the window. If two controls share a name the outline attaches to the first one found.
2. A designated control must be **visible at that moment**. The gate cuts out only the control's screen rectangle. For an item inside a scroll area, `FocusGlobalOption` must open it at a visible position (currently it opens the group detail and puts the scroll at the top).
3. Redrawing the window destroys the controls and detaches the outline. The prologue reattaches by name every frame and the notice card every 0.25 s. Redrawing the window repeatedly through save notifications or per-frame refresh makes the outline flicker. Today `StoreViewBinding` defers refresh during press, input, drag and dialogs, and restores the draft, focus and scroll. Keep this protection.
4. Back order: dialog → from the usage screen to the skill tree → close the window (`HandleBack`). In the prologue Back itself is locked.
5. Saving goes through the existing `CommitHuntEdict` path. Tutorial completion and progress depend on the saved value change, so saving through another path will not complete the guide.
6. Number entry and quick preset choice open as dialogs (`HasDialog`). Tutorial step logic reads `HasDialog`, so a new screen that does the same job must report "input in progress" in the same way.
7. The designated control and the notice card must not be clipped at 440×956 (portrait), 956×440 (landscape) and PC 16:9, 16:10 and 21:9. Validate at the default text size in Korean and English. There is no text-size scale feature.
8. New screens follow the shared UI contract ([Shared_UI_Contract](../Implementation/Shared_UI_Contract.md)) and the new-content UI rules in `AGENTS.md`.

## 5. New UI needed (requested by the tutorial, not yet built)

Showing the three beats "feel the problem → one fix → see the result" for each edict sentence needs these places on the edict screen. The tutorial supplies the content and the decisions, and the UI provides the **place and the shape**.

| No. | Request | Description | Priority |
| --- | --- | --- | --- |
| N1 | New-sentence marker | Put a "New sentence" marker only on tabs, groups and options opened since the player last looked, and clear it when opened or changed. The input is the change in the list of opened rules. | Required |
| N2 | Collapse default | Expand only newly opened or focused items and keep groups that were already open collapsed. | Required |
| N3 | Next-sentence teaser on the Overview tab | Show which sentence the player is on, the next sentence's name and unlock condition, and scroll page progress (pages received / total pages, currently 11 chapters). Today it is one line of text. | Required |
| N4 | Three-beat guidance strip | A slot above the group detail screen that shows in order ① the problem met in the last hunt, ② one fix (a quick preset choice or one option), ③ what changed after saving. The tutorial fills the content. | Required |
| N5 | Guide speech bubble place | A place near the highlighted item for a short line (1 to 2 lines) from the guiding NPC. Today guidance is one town card only. | Optional |
| N6 | Entry point for checking the result right after saving | A button place for one next action after saving, such as "Check in the training ground" or "Observe in the next hunt". | Optional |

- N1 to N4 must not cover or push away the edict's main controls (save, tabs, options) at any resolution.
- Keep point 3 of 4.4 so the new markers do not add refreshes that make buttons flicker.

## 6. What to change together when the UI changes

Tutorial side (adjusted afterward, following the UI work):

- `GameUI.TutorialStaging.cs`: `LessonTarget`, `SkillLessonStep`, `SurvivalLessonStep`, `TickPrologueLesson`
- `GameUI.TutorialComparison.cs`: `ProgressiveSkillLessonStep`, `ProgressiveSurvivalLessonStep`
- `GameUI.Tutorials.cs`: `RefreshTutorialAnchor`, `StartTutorialAction`
- `GameUI.EdictNotices.cs`: notice card and target list
- `HuntEdictWindow.Progression.cs`, `HuntEdictProgression.cs`, `HuntEdictProgression.json`: disclosure decision and stages

Smokes and tests (by number of `edict-*` names used, most first):

| File | Name uses |
| --- | --- |
| `RuntimeSkillTreeSmoke` (main, Sections, Presets, CombatPreview, Identity) | 93 / 50 / 30 / 14 / 4 |
| `RuntimeTutorialSmoke.Progression`, `RuntimeTutorialSmoke` | 26, 17 |
| `RuntimeRecommendedEquipmentSmoke` (main, Controls, Storage) | 25 / 23 / 10 |
| `RuntimeEdictQuickPresetSmoke` | 25 |
| `RuntimeSharedUiSmoke` | 24 |
| `RuntimeEdictOverviewSmoke` | 15 |
| `RuntimeRiftEntrySmoke` (Preparation, Repeat, Portal) | 9 / 5 / 3 |
| `RuntimeTrainingGroundSmoke`, `RuntimeLanguageSmoke`, `RuntimeUiStyleSmoke`, `RuntimeHuntEdictSaveLayoutSmoke`, `RuntimeButtonUxSmoke`, `RuntimeTrainingEquipmentSmoke` | 1 to 6 each |
| Edit Mode: `HuntEdictProgressionTests`, `TutorialProgressionTests`, `TutorialChapterTests`, `RepeatHuntTests`, `HuntEdictDefaultsTests` | Disclosure stage and guide ID checks |

- The numbers count occurrences of `"edict-…"` strings in the source and are only for estimating size. Which smokes are really affected depends on how far names change.
- Note: some skill smokes use the older editor (not the unified window). Decide first whether the overhaul targets the unified `HuntEdictWindow` or the older one.
- When edict stages change, the guide text, the error messages in `ValidateChange` and the `TestCase` numbers in `HuntEdictProgressionTests` change together.

## 7. Handoff requested from the UI author

Leaving the following when the UI work ends lets the tutorial work continue on the new screen right away.

1. **Name mapping table**: for every name in 4.1, one line on what it became in the new UI (kept, renamed, moved, removed or replaced).
2. **Structure changes**: if the tab and group layout changed, how the group placement in `HuntEdictUi.json` and the "where it opens" column of 3.3 changed.
3. **Window state and actions**: items in 4.2 that were removed or renamed, and new ones.
4. **Designated areas**: whether `Option area` and `Preset explanation` are kept, what replaces them if not, and where the progress button (`edict-starter-next`) is.
5. Whether **N1 to N4 of chapter 5** are implemented, and the shape of the data they take in (what the tutorial must hand over).
6. Captures of the main screens: Overview tab, group detail (with the quick preset choice), the number-entry dialog, and the Skills tab with the usage screen. At 440×956 and 956×440, in Korean and English.
7. The list of fixed smokes and tests, the checks that were run and their results, and the checks that were not run.

## 8. Current implementation state

| Item | `main` | `claude/tutorial-chronicle` (not merged) |
| --- | --- | --- |
| Staged disclosure (Rift 2 to 20, first result) | Present | Present |
| Low-HP response at Rift 3, position and distance at Rift 5 | Reversed (low HP 5, position 3) | Swapped |
| Detailed conditions | E18 as one group of 40 | Three groups E18 / E18B / E18C (20, 13, 7) |
| Chapter engine (page rewards, current-chapter recommendation) | None | Present (no screen) |
| New-sentence marker, collapse, next-sentence teaser, three-beat strip | None | None |
| Chapter dialogue and scroll window, guide NPC | None | None |

- The changes on this branch were checked up to the related Edit Mode tests. The full suite and the runtime smokes are planned once at the end and have not been run yet.
- The screen behavior described here comes from reading the code, and was not re-confirmed by running the game in this work.

## 9. Source files

- Forced prologue lesson: `Assets/HELLSCRIPT/Runtime/Presentation/GameUI.TutorialStaging.cs`, `GameUI.TutorialComparison.cs`
- Guide wiring and outline: `GameUI.Tutorials.cs`, `GameUI.EdictNotices.cs`, `TutorialAnchorRing.cs`, `PrologueGate.cs`
- Edict window: `HuntEdictWindow.cs`, `HuntEdictWindow.Progression.cs`, `HuntEdictWindow.Overview.cs`, `HuntEdictWindow.Summary.cs`, `HuntEdictWindow.QuickPresets.cs`, `HuntEdictWindow.SkillPresets.cs`, `HuntEdictWindow.Skills.cs`
- Disclosure rules and data: `Assets/HELLSCRIPT/Runtime/Core/HuntEdictProgression.cs`, `Assets/HELLSCRIPT/Resources/HuntEdictProgression.json`, `HuntEdictUi.json`, `HuntEdictQuickPresets.json`
- Guide definitions and decisions: `Assets/HELLSCRIPT/Runtime/Core/Tutorials.cs`, `Tutorials.Progress.cs`
