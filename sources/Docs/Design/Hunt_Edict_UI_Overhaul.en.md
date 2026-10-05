# Hunt Edict UI Overhaul Design — Tutorial-Compatible

Updated: 2026-10-05 · [한국어](Hunt_Edict_UI_Overhaul.md) · [Open the mockup](../../Prototypes/HuntEdict/HELLSCRIPT-HuntEdict.html) · [Mockup README](../../Prototypes/HuntEdict/README.md) · [Native port prompt](../../Prototypes/HuntEdict/NATIVE_PORT_PROMPT.md)

Status: design settled (HTML mockup v2). **Nothing is implemented in Unity yet.** This is the handoff document that lets the developer who rebuilds the Hunt Edict window and the tutorial author work against one contract. Inputs are `Docs/Design/Tutorial_Hunt_Edict_UI_Brief.md` on the tutorial branch (`claude/tutorial-chronicle`, not merged) and six code investigations (tutorial contract, current window structure, smoke and disclosure impact, two porting maps, test impact). Everything below was read from code and captures; none of it was confirmed by running the game.

Background: [Hunt Edict UX redesign (overview screen)](Hunt_Edict_UX_Redesign.en.md), [Hunt Edict staged disclosure](../Implementation/Hunt_Edict_Progression.en.md), [Shared UI contract](../Implementation/Shared_UI_Contract.en.md).

## 1. Request and decisions

The user found the Hunt Edict screen functional but unstyled, and asked for a design that keeps every current feature, is easy to understand, clean and attractive, and **fits a tutorial that discloses the smallest set of features first and then opens more**. The tutorial points at controls by name and reads window state, so a purely visual rework can stall the guidance. Structure and names are therefore fixed as a contract; only the surface and the information design change.

| Decision | Content | Reason |
| --- | --- | --- |
| D1 | Keep the 10 tab ids and the `edict-tab-<id>` names. The 5-tab merge of mockup v1 is withdrawn | Nine smokes assume all 10 tabs exist at once and tutorial code names tabs directly |
| D2 | Keep the overview from the 2026-10-01 decision (3 combat-style cards + default-settings switch) and add only a **next-sentence teaser** | It was simplified on purpose that day and `CheckSimpleOverview` pins its structure |
| D3 | Keep the quick-preset dialog flow (choosing applies at once). Inline expansion is parked until the tutorial is stable | Avoids breaking resume of v2 saves and 24 smoke references |
| D4 | The guide line (N5) is a **docked caption band** at the top of `Option area`, not a floating bubble | In the current A/B comparison capture the bubble covers the preset tabs |
| D5 | The three-beat strip (N4) is **one line** by default and expands on tap | Keeping 51 of 364 landscape pixels permanently leaves about four option rows |
| D6 | The reference disclosure table is the tutorial branch's `HuntEdictProgression.json`. **Merge that branch into `main` before the Unity port** | Low-HP response at rift 3, position/distance at rift 5 and the 20/13/7 E18 split differ from main |
| D7 | The "new sentence" marker (N1) is computed, not stored | Avoids a save schema change and migration |

## 2. At a glance

| Changes | Does not change |
| --- | --- |
| Tabs: icon + name + subtitle + new-sentence dot. The prologue shows a narrow icon-only strip | 10 tab ids, `edict-tab-<id>` names, all tab buttons inside `Main tabs` |
| Group chips: icon + title + **current answer** (quick-preset name) + changed dot + new-sentence dot | `edict-group-<ids[0]>` names, chips outside any scroll area, `HuntEdictUi.json` groups and id order |
| Option rows: one line (label + ? + value pill) | `edict-option-<id>`, `edict-help-<id>`, the kinds of edit dialogs |
| Dialogs: radio cards, number range and quick-value chips | `edict-quick-picker-*`/`edict-quick-choice-*`, `edict-number-input`/`edict-number-apply`, `HasDialog` |
| Overview: real-number chips on style cards + next-sentence teaser | Overview structure, exactly 3 `edict-style-*` buttons, `edict-default-settings` |
| Policy screen: progress button above the explanation in portrait, observation progress bar | The `Preset explanation` area; `edict-starter-next` stays inside it |
| New slots N1–N6 | Disclosure rules, permissions, the save path (`CommitHuntEdict`), the refresh-after-save rules |
| Guide line: docked caption | The legacy-account tree screen, the legacy editor `ShowLegacyEdictEditor` |

## 3. Design principles

- **P1 Keep names and ids.** Do not change `edict-*` names, the 10 tab ids, area names or the `ids[0]` rule. When one changes, record keep/rename/move/remove/replace in the table of chapter 6.
- **P2 The designated control is on screen at that moment.** The tutorial gate cuts out the control's screen rectangle without clipping it to the viewport or a scroll mask. A control scrolled out of view still counts as present and the cover never releases. The prologue is laid out so nothing needs scrolling; elsewhere `EnsureVisible` guarantees it.
- **P3 Undisclosed content is not drawn.** No locked placeholders or gaps; only the next feature is announced, by the overview teaser.
- **P4 New markers live inside `DisclosureDisplayKey`.** No extra Repaint, timer or per-frame update changes a marker (button-flicker rule).
- **P5 One save path.** Every change goes through the edit draft (`Session.Draft`) and `CommitHuntEdict`. Tutorial completion is decided by a change in the saved value.
- **P6 `HasDialog` keeps its meaning.** True for any modal, including the skill action bubble.
- **P7 N1–N5 never cover or push away save, tabs or options.** A height budget is set and checked by rectangle comparison.
- **P8 Sizes and languages.** Portrait 440×956, landscape 956×440, PC 16:9, 16:10, 21:9; Korean and English; default text size only.
- **P9 New strings ship in Korean and English.** The Korean source text is the `en.txt` key.

## 4. Screen densities and staged disclosure

There are four densities with different tab counts and edit scope. Tab visibility is decided by `HuntEdictProgression.Tab`; this overhaul does not change it.

| Level | When | Visible tabs | Editing | Mockup evidence |
| --- | --- | --- | --- | --- |
| L0 Prologue | Mandatory map | Skills → Survival, one at a time | One first-skill node, one slot, one potion HP threshold | `evidence/lesson-*`, `survival-*` |
| L1 Early | Town arrival to rift 5 | Overview, Skills, Survival, plus Combat from rift 4 | Quick presets only. Direct editing only for potion HP, auto potion, and the defense/escape skills | `evidence/stage-t0-*`, `stage-t1-*`, `stage-r3-*` |
| L2 Middle | Rift 6 to 15 | Presets (6), Loot (7), Explore (8), Bag (9), Auto equip (12), Repeat (15 + rune guide) | Direct settings of disclosed groups, skill details, attack order, local presets, training comparison | `evidence/stage-r6-*`, `stage-r7-*`, `stage-r9-*`, `stage-r12-*` |
| L3 Late | Rift 18 onward | All tabs | Three detailed-condition groups, action order and sharing (20) | `evidence/stage-r18-*`, `stage-r20-*` |

When each sentence opens, and where, follows the tutorial branch's table. One rule can span several groups and tabs (E18B spans Combat and Loot; E20 spans Combat, Explore and Repeat).

| When | Guide | Title | Tab · group | Options | Focus option |
| --- | --- | --- | --- | --- | --- |
| Mandatory map | P00 | Potion HP threshold | Survival · Auto potion | 1 | `survival.potionHpPercent` |
| First regular rift ended | F05 | Auto potion, basic dodge | Survival · Auto potion, Dodge by damage type (permission) | 1 + permission | `survival.potionHpPercent` |
| Rift 2 | E02 | Combat styles | Overview · combat style cards | permission | `edict-style-balanced` |
| Rift 3 | E05 | Low-HP response | Survival · Emergency response and return, Survival means and reserve | 5 | `survival.lowHp` |
| Rift 4 | E04 | Targeting and switching | Combat · Target choice, Target keeping and pursuit | 6 | `target.default` |
| Rift 5 | E03 | Engagement distance and encirclement | Combat · Position and distance, Encirclement and herding | 11 | `position.engage` |
| Rift 6 | F07, H09, H10, H17 | Direct settings, presets, training comparison | Presets and sharing, direct settings of every disclosed group | permission | — |
| Rift 7 | E07 | Loot pickup | Loot · Pickup by grade, Gold and materials, Pickup travel and finish | 12 | `loot.NORMAL` |
| Rift 8 | E08 | Exploration and special encounters | Explore · Exploration, Chests, Shrines (cursed chest after one is actually found) | 9 | `explore.mode` |
| Rift 9 | E09 | Bag space and protection | Bag and cleanup · four groups | 12 | `bag.trigger` |
| Rift 12 | E12 | Auto equip | Recommended equipment · Common settings, Weapon | 3 | `autoEquip.enabled` |
| Rift 12 + legendary or set acquired | E12S | Legendary and set comparison | Recommended equipment · per slot | 17 | `autoEquip.1.mode` |
| Rift 14 | E14 | Potion resupply | Survival · Auto potion | 11 | `potion.autoBuy` |
| Rift 15 + rune guide | H11 | Auto repeat | Repeat · three groups | 8 | `repeat.enabled` |
| Rift 18 | E18 / E18B / E18C | Three detailed-condition guides | Survival, Combat, Loot, Bag | 20 / 13 / 7 | `dodge.interrupt` / `target.chaseDistance` / `bag.replacementRank` |
| Rift 20 | E20 | Action order and sharing | Combat, Explore, Repeat + sharing | 15 | `common.order` |

## 5. Structural changes

**Tabs.** The 10 ids and their order (`overview, skills, combat, survival, loot, bag, explore, repeat, autoEquip, presets`), the visibility rule and the title array (titles are positional against `HuntEdictUiCatalog.Tabs`) all stay. Only the surface changes.

| Item | Today | v2 |
| --- | --- | --- |
| Tab button | Text (+ an 18px icon in landscape) | Icon + name + subtitle (landscape with 6 tabs or fewer) + new-sentence dot |
| Prologue (L0) | One tab in a 172-wide column | About 48 wide icon column in landscape, a thin row in portrait. The button objects (`edict-tab-skills`/`edict-tab-survival`) remain |
| Portrait | `min(5,n)` columns | Unchanged |
| Family dividers | None | Not used. Ten tabs fit one column and read fine without them |

Subtitles are short phrases in both languages: Overview "How you fight", Skills "Learn and equip", Combat "Position, target", Survival "Potion, retreat", Loot "What to pick up", Bag "When full", Explore "Explore, chests", Repeat "Auto repeat", Auto equip "Auto equip", Presets "Save, compare, share".

**Groups.** `HuntEdictUi.json` does not change (35 groups, id order, tabs, icons). `ids[0]` decides the `edict-group-*` name, the quick-preset scope (`global/<ids[0]>`) and the scroll key, so reordering ids silently breaks all three. Chips only gain display.

| Item | Today | v2 |
| --- | --- | --- |
| Chip | Icon + title + changed dot | + current answer (quick-preset name or "Custom") + new-sentence dot. Height 42 → 44 |
| Columns | `clamp(floor(w/(116·grow)), 1, group count)` | Same formula |
| Body | Description → picker → option rows | Same order. Only option rows become one line |
| Default selection | First group when nothing was selected | N2: new group > remembered group > first group |

**Option rows.** Today a label line sits above a value well (two lines, minimum 62). In v2 the label (+ ?) and the value pill share one line (minimum 36), and the pill is `edict-option-<id>`. A disabled row adds one reason line below ("Detailed settings open at rift 6." or "{parent option}: applies when {value} is chosen"). A changed row gets a gold bar on its left.

**Dialogs.** The frame (title, close `edict-dialog-close`, backdrop `edict-dialog-dismiss`) stays.

| Dialog | v2 |
| --- | --- |
| Quick preset | Radio cards (name + description). Choosing applies to the draft and closes. The last "Custom" entry appears from rift 6 |
| Number | −/＋, range-and-step line, quick-value chips for percent options (a chip only fills the input; Apply is separate), error line |
| Choice, set, order | Choice applies on tap; set and order use `edict-choice-apply`. **Order keeps the existing drag cards** (the mockup's ↑↓ buttons are a simplification) |
| Help, confirm | Unchanged |

**Search row.** The mockup hid `edict-search` and `edict-changed-only` when fewer than 8 options were disclosed. A smoke may use these two controls at low disclosure stages, so **the port's default is to hide them only in the prologue (`ProgressivePrologue`)**. Whether to hide them for other low-disclosure accounts is an open decision (chapter 15).

**Overview.** The 2026-10-01 layout (3 combat-style cards + the default-settings switch from rift 20) gains:

- Two real-number chips per style card: "Retreat at {n}% HP" (from that style's `survival.lowHp` preset) and the dodge preset name. Invented meters are not used.
- A next-sentence teaser card (N3). It is not a button and uses a sentence-number badge instead of an icon, so `CheckSimpleOverview` ("exactly 3 style buttons, one ScrollRect, no SkillIconView") still holds.

**Default settings (lock).** From rift 20 the switch sits at the top of the overview. When on, only Overview, Skills (manage) and Repeat stay open; the other tabs and the footer are dimmed (`DisableForDefaults`). In that state `FocusGlobalOption` silently does nothing, so the guide caption switches to a sentence pointing at `edict-default-settings` (chapter 10).

**Skills tab (new, non-legacy accounts).** A card list of learnable skills, not a tree. Before level 2 only the first skill is listed. It has remaining points, the card list, an inspector (−, ＋, equip) and the equipped-skill row. Tapping a card opens an action bubble (a modal) with `edict-slot-equip`/`edict-slot-remove` and `edict-slot-policy`. The prologue shows one slot; later "equipped + 1" (up to 4), and an ultimate slot is added at level 40.

**Policy screen.** `edict-policy-back`, preset tabs (each with a radio), `Preset explanation`, the equipped row below in portrait and as a right-hand vertical strip in landscape. Activation saves at once, so there is no footer (no `edict-save`).

**What the port does differently from the mockup.** The mockup is HTML and simplifies in places. In native code follow this:

- Do not port the change-count chip ("Unsaved changes n"); `Session.Dirty` is only a boolean.
- Keep the order dialog's existing drag cards (no ↑↓ buttons).
- Keep the existing `RequestLeave` close flow (revert, or save and leave). Do not use the mockup's "Cancel/OK".
- The gold bar on the left of a changed option row has no native equivalent. Build it as a new 3-wide Panel or reuse the "•" dot of the group chips.
- Keep the dedicated native screens for the `bag.warehouseFull` group and the auto-equip tab (`DrawWarehouseHandling`, `DrawRecommendedEquipment`). The mockup's plain option rows are a simplification.
- Do not copy the mockup's pixel values as constants. The mockup uses the same `UiTheme.Scale` formula as the game (450 tall in landscape, 405 wide in portrait), but existing height formulas multiply by `lastScale`/`Grow`, which the mockup lacks.

## 6. Name mapping table

Status: **Keep** = name, meaning and placement concept unchanged; **Move** = same name in a different position; **New** = added now. "In mockup" means the name appears in `Prototypes/HuntEdict/evidence/ui-names.json` (the names the browser tests visited). The mockup attaches names as `data-ctl` attributes.

### 6.1 Frame, tabs, footer

| Name | Meaning | Status | In mockup | Note |
| --- | --- | --- | --- | --- |
| `menu-hunt-edict` | Hunt Edict button in the dock | Keep | ✓ | In the game this is the `contentDockEdict` field, not an object name (the object is named "사냥 칙령") |
| `Hunt Edict window` | Window root | Keep | ✓ | Used as a path by smokes |
| `Main tabs` | Tab panel | Keep | ✓ | All 10 tab buttons must be inside it (nine smokes) |
| `edict-tab-<id>` | Tab button | Keep | ✓ | ids: `overview skills combat survival loot bag explore repeat autoEquip presets` |
| `Option area` | Window body root (RectMask2D) | Keep | ✓ | A caption band may be reserved at its top (chapter 8) |
| `Fixed save controls` | Footer | Keep | ✓ | Height 0 on the policy screen |
| `edict-save` / `edict-revert` | Save / Revert | Keep | ✓ | Active only with changes |
| `edict-close` | Close | Keep | ✓ | |
| `edict-dialog-dismiss` / `edict-dialog-close` | Dialog backdrop / close | Keep | ✓ | |
| `Dialog` | Dialog panel | Keep | ✓ | |

### 6.2 Overview

| Name | Meaning | Status | In mockup | Note |
| --- | --- | --- | --- | --- |
| `Overview custom settings` | Overview body root | Keep | ✓ | Used as a path by `CheckSimpleOverview` |
| `Overview combat styles` | Overview scroll | Keep | ✓ | |
| `edict-style-aggressive` / `balanced` / `careful` | Combat-style cards | Keep | ✓ | Exactly 3 buttons |
| `edict-default-settings` / `edict-default-status` | Default-settings switch / status text | Keep | ✓ | From rift 20 |
| `edict-teaser` | Next-sentence teaser (N3) | **New** | ✓ | Not a button |

### 6.3 Group screens

| Name | Meaning | Status | In mockup | Note |
| --- | --- | --- | --- | --- |
| `edict-search` / `edict-changed-only` | Search input / changed-only | Keep | ✓ | Display rule in chapter 5 |
| `edict-group-<ids[0]>` | Group chip | Keep | ✓ | Outside scroll; group icon required |
| `edict-search-result-<key>` | Search result card | Keep | ✓ | |
| `Selected edict group` | Selected-group scroll | Keep | ✓ | |
| `edict-quick-picker-<scope>` | Quick-preset picker | Keep | ✓ | Example scope: `global/position.engage` |
| `edict-quick-choice-<preset id>` | Dialog choice (`custom` for direct setup) | Keep | ✓ | |
| `edict-option-<id>` | Option value button | Keep | ✓ | The value pill carries this name |
| `edict-help-<id>` | Help `?` | Keep | ✓ | |
| `edict-choice-apply` | Apply for set and order | Keep | ✓ | |
| `edict-number-input` / `edict-number-apply` | Number input / apply | Keep | ✓ | The input is an `InputField` found by name |
| `edict-beat-strip` | Three-beat strip (N4) | **New** | ✓ | Between chips and scroll, outside the scroll |

### 6.4 Skills and policy screens

| Name | Meaning | Status | In mockup | Note |
| --- | --- | --- | --- | --- |
| `Available skill tree` | Skill card list scroll | Keep | ✓ | |
| `edict-skill-<skillId>` | Skill card (a tree node for legacy accounts) | Keep | ✓ | The `edict-skill-` prefix is shared with `rank-up/rank-down/equip/reset` |
| `edict-skill-rank-up` / `rank-down` / `equip` / `reset` | Inspector buttons | Keep | ✓ (`reset` is out of the mockup) | |
| `Skill action bubble` / `edict-skill-menu-dismiss` | Action bubble (modal) / outside-click consumer | Keep | ✓ | `HasDialog` is true |
| `edict-slot-equip` / `edict-slot-remove` | Bubble equip / unequip | Keep | ✓ | |
| `edict-slot-policy` | Bubble: edit hunt edict | Keep | ✓ | |
| `Equipped active skills` | Equipped skill row | Keep | ✓ | |
| `edict-active-slot-<0..3>` / `edict-ultimate-slot` | Equip slots | Keep | ✓ | |
| `edict-common-policy` | Common attack settings | Keep | ✓ | From rift 6 |
| `edict-policy-back` | Back to the skill list | Keep | ✓ | |
| `Skill preset panel` | Policy screen panel | Keep | ✓ | |
| `edict-preset-tab-<id>` | Use-policy tab | Keep | ✓ | The prologue has two approaches only |
| `edict-preset-activate` | Radio of the tab currently shown | Keep | ✓ | Activation saves at once |
| `edict-preset-radio-<id>` | Radio of a tab not shown | Keep | ✓ | |
| `Preset explanation` | Row with explanation, preview, progress button | Keep | ✓ | Chapter 8 |
| `edict-starter-next` | Comparison progress button | **Move** | ✓ | Same area. Above the explanation in portrait, below it in landscape |
| `edict-preview-restart` / `pause` / `speed-<rate>` | Combat example controls | Keep | ✓ | |
| `edict-policy-basic` / `edict-policy-order` / `edict-policy-choice-<id>` | Basic attack, attack order, direct use-policy choice | Keep | — | Out of the mockup |

### 6.5 Presets and sharing

| Name | Meaning | Status | In mockup |
| --- | --- | --- | --- |
| `edict-preset-name-<slot>` / `store-<slot>` / `share-<slot>` / `import` | Slot name / store / share / import | Keep | ✓ |
| `edict-preset-name-input` | Rename input | Keep | — |

### 6.6 Outside the window, new slots, out of the mockup

| Name | Meaning | Status | In mockup |
| --- | --- | --- | --- |
| `edict-notice-configure` / `edict-notice-later` | Town notice card buttons (outside the window) | Keep | — |
| `Guide caption` (mockup name `edict-caption`) | Guide caption band (N5), a RectTransform **sibling** of `Option area` | **New** | ✓ |
| `edict-save-next` | After-save action button (N6) | **New** | ✓ |
| `edict-auto-*`, `edict-storage-*` | Recommended-equipment and admission screens | Keep (surface rework later) | — |

Mockup-only name: `Skills page` (a mockup wrapper). It does not exist in game code.

## 7. Window state and actions

Every public member the tutorial reads from outside the window stays.

| Member | Status |
| --- | --- |
| `SelectedTab`, `ManagingSkills`, `SelectedSkill`, `PolicySkill`, `SelectedGroup`, `VisibleSkillPreset`, `ActiveSkillPreset`, `QuickPresetSelection(scope)`, `ProgressivePrologue`, `PreviewCombat`, `Session.Draft`, `Session.Dirty` | Keep (meaning unchanged) |
| `HasDialog` | Keep. True for any modal (quick preset, number, choice, set, order, help, confirm, skill action bubble) |
| `Open(…, tutorialLesson)`, `SelectTab(tabId)`, `SelectSkillPage`, `OpenSkillPolicy`, `HandleBack()` | Keep. Back order: dialog → policy screen to the skill list → close the window. The prologue uses `ContentWindowHost.BackLocked` |
| `FocusGlobalOption(optionId)` | Keep, but it now **scrolls the option into view** (`EnsureVisible`). In default-settings mode it tells the caller it refused |

Additions (names and signatures are proposals).

| Member | Purpose |
| --- | --- |
| `EnsureVisible(string controlName)` | Moves the scroll area holding the control so its rectangle is inside the viewport. Called by `FocusGlobalOption`, default group selection and the tutorial's `LessonTarget` |
| `SetGuideCaption(EdictCaption)` / `ClearGuideCaption()` | Caption band (N5) |
| `SetBeats(EdictBeats)` / `ClearBeats()` | Three-beat strip (N4) |
| `SetAfterSave(EdictAfterSave)` | After-save action (N6) |
| `MarkGroupOpened(string groupKey)` | Records a group opened this session (N1). Not saved |

## 8. Designated areas

- **`Option area`** stays. It is the window body root with a RectMask2D. While a guide is active 44 (landscape) or 58 (portrait) logical pixels are reserved. **Implement it by shrinking `Option area` itself and placing `Guide caption` above it as a sibling.** A child wrapper would break the `Option area/Overview custom settings` path (smokes) and the assumption that the name is unique. With this, the caption band is not inside the gate's `Option area` hole. The reservation changes only when a guide starts or ends and stays while a dialog is open.
- **`Preset explanation`** stays too. It is one row of the policy screen holding the explanation, the combat example and the progress button. It is not drawn for the Custom tab.
- **`edict-starter-next`** must stay **inside** `Preset explanation`, because the gate lets input through only inside the designated area. The mockup puts it at the bottom of the explanation block in landscape and at the top (above the combat example) in portrait. Both are visible without scrolling.
- **No-scroll layout (P2).** Every designated control of the whole prologue is on screen at all five sizes in both languages with auto-scroll off (mockup verification).
- The tutorial's `starter-selection-area` maps to `Option area` and `starter-observation-area` to `Preset explanation`; the tutorial side (`LessonTarget`) owns that mapping.

## 9. New slots N1–N6

Heights are logical units, measured in the mockup in the rift-5 guide state. The mockup sizes the window with the same formula as the game's `UiTheme.Scale` (landscape 956×440 → 978×450, portrait 440×956 → 405×880), so the landscape area is 364 and the portrait area about 746.

| Slot | Position | Height (landscape / portrait) | Data | Refresh |
| --- | --- | --- | --- | --- |
| **N1** New-sentence marker | Dot on tab buttons and group chips | 0 (overlap) | Computed: `rule disclosed ∧ guide not performed ∧ group not opened this session` | Put the marker list into `DisclosureDisplayKey` |
| **N2** Default expansion | Selected group on tab entry | 0 | New group > remembered group > first group | Existing `SelectGroup` |
| **N3** Next-sentence teaser | Overview, below the style cards | 76 / 76 | `EdictTeaser` | The overview already repaints |
| **N4** Three-beat strip | Between group chips and scroll (outside the scroll) | one line 28, open 51 / one line 28, open 117 | `EdictBeats` | Only when the tutorial sets or clears it |
| **N5** Guide caption | Reserved band at the top of `Option area` | 44 / 58 | `EdictCaption` | Only when the tutorial sets or clears it |
| **N6** After-save action | Footer status line | inside the footer | `EdictAfterSave` | Once on save success, 12 seconds or until the next change |

**N1 computation.** A rule is an entry of `HuntEdictProgression.json`; its guide is the rule's `guide` (F05 for the first-result rule, H11 for the repeat rule when none is set). Only rules with at least one disclosed (`Disclosed`) option are candidates. A quick preset also writes values of rules that are not yet open (for example the low-HP preset sets the lethal-damage and attached-damage values of E18), so without a disclosure check a guide would be recorded as performed too early. The game's `TutorialProgress.EdictSaved` already checks `GuideVisible`. A marker clears through `TutorialRecord.practiced`, and a group opened this session is remembered for the session. There is no new save field.

**Data shapes** (proposals, placed in `Core` without a `UnityEngine` dependency).

```csharp
public struct EdictTeaser   // structured result of HuntEdictProgression.Next(...) + the chapter engine
{ public int Sentence, Total, Stage, Now, PagesReceived, PagesTotal; public string Key, Title, Condition; }
public sealed class EdictBeats      // filled per guide by the tutorial
{ public string Guide, Focus, Problem, Fix, ResultBefore, ResultAfter; public bool Done; }
public struct EdictCaption          // text is already translated
{ public string Text, Target; public bool Hard; }
public struct EdictAfterSave        // label + action
{ public string Label; public System.Action Run; }
```

- `EdictTeaser.Condition` is one of "After the first rift ends", "At rift N clear" or "After the rune guide"; `Now/Stage` draws the progress bar. Hide the page count until the chapter engine is connected.
- `EdictBeats.Problem` reuses the last-5-seconds HP and largest damage source text of the first-result guide (F05); other guides are written by the tutorial. The mockup copy is illustrative.
- `EdictCaption.Hard` true means a gate guide (prologue), false a ring guide. Prologue text is the game's own v3 text.
- The N6 label is "Check in training" when the training ground is open, otherwise "Watch next hunt".

## 10. Requests to the tutorial side

Outside the UI scope; the tutorial author applies them in Phase 4.

| # | Request | Reason |
| --- | --- | --- |
| T1 | Make the gate treat a designated control whose rectangle is outside the viewport or mask as missing and release after 2 seconds (`PrologueGate`, `LessonTarget`) | Today a scrolled-out control counts as present and the cover never releases (from reading code) |
| T2 | In rule guides, hand the ring to `edict-save` once the draft has a change (`RefreshTutorialAnchor`) | The ring sticks to the first control found in the name list, so it never reaches Save while an option row or picker is visible |
| T3 | In default-settings mode `FocusGlobalOption` is refused, so the caption points at `edict-default-settings` (`StartTutorialAction`) | Today no ring appears and the guidance stalls |
| T4 | Draw the caption into the window's caption band (`edict-caption`) instead of its own panel | The own panel covers controls such as the preset tabs |
| T5 | Hand over N4 and N6 content per guide (`GameUI.EdictNotices`) | The UI provides only the place and shape |
| T6 | Keep the "performed" decision of `EdictSaved` limited to disclosed rules | Same rule as N1 |
| T7 | Review the card selection order (`Tutorials.All` JSON order) against the ladder order | A new-sentence dot and the card may point at different sentences |

## 11. Porting map

Read the mockup numbers through the conversion rules of chapters 5 and 8 (`UiTheme.Scale`, portrait 405×880, and the `lastScale`/`Grow` factors that multiply existing heights). "Estimated" below means computed from that formula, not measured.

As of 2026-10-05. The window UI code is the same on the tutorial branch and on `main` (only `Resources/HuntEdictProgression.json` and `Core/TutorialChapters.cs` differ). Everything below was read from code; no build, test or smoke was run. Line numbers refer to the source of that date and drift when the code changes.

Abbreviations: `W` = `Presentation/HuntEdictWindow.cs`; `Su`, `Ov`, `Qp`, `Op`, `Pr`, `Pz`, `Sk`, `Eq` = `HuntEdictWindow.{Summary, Overview, QuickPresets, Options, Progression, Presets, Skills, Equipment}.cs`; `Prog` = `Core/HuntEdictProgression.cs`. Smokes are `S:` + name (Ovw = RuntimeEdictOverviewSmoke, Sec = RuntimeSkillTreeSmoke.Sections, Qp = RuntimeEdictQuickPresetSmoke, Shared = RuntimeSharedUiSmoke, Save = RuntimeHuntEdictSaveLayoutSmoke, Tut = RuntimeTutorialSmoke).

### 11.1 Window frame

| Owner | Change | Trap |
| --- | --- | --- |
| Header W:85-92 | Subtitle (:90-91, class · preset) becomes class · Lv · style | Reuse `CurrentStyle` Ov:10 |
| `Option area` W:112-113 | Reserve height only while a guide is active. Cheapest: shrink this `Place` and add a sibling `Guide caption` above it (every drawing function already follows `bodyHeight`) | The name must stay a unique direct child (S:Shared 69; `GameUI.TutorialStaging.cs:250` takes the first "Option area"). The path `Option area/Overview custom settings` (S:Sec 20) forbids a wrapper child. Shrinking removes the band from the gate hole, so the floating card of `PrologueGate.cs:85-103` must be aimed at the band. Toggle on guide-active, not on `HasDialog` (`Repaint` kills dialogs, W:138). Handle it from `Update` (W:52-56) |
| Footer W:126-133 | After `Save()` (:166-170) show "Saved" and `edict-save-next`. The label comes from GameUI (today a toast, `GameUI.HuntEdict.cs:41-42`). Clear on change, revert or navigation, plus a 12 s timer | Build it before `DisableForDefaults(floating)` (:132); S:Sec 17-19 pins exactly 4 enabled buttons under the lock. The status Text is `width-210` wide (:129). There is no change-count API (only the `Session.Dirty` boolean, `HuntEdictLoadout.cs:102`), so drop the count chip. S:Save 45-56, S:Tut 134-136 |

### 11.2 Tabs (W:94-111)

The 172-wide landscape column and the `min(5,n)` portrait columns already match the mockup (:96-97).

| Part | Change | Trap |
| --- | --- | --- |
| Subtitle | A second `Text` only in landscape with `visibleTabs.Length<=6`. New strings go to `en.txt` | Tab Texts must not clip (S:Ovw 72; S:Tut 132 runs 5 sizes × 2 languages). Cap `cellHeight` (:100) at 52 like the mockup; today it stretches, so 3 tabs come out about 119 tall |
| Portrait icon | Portrait has no `Glyph` (:107); add one above the label | `Glyph` |
| New dot | A gold circle child, `raycastTarget=false`, hidden on the selected tab | Skin `unequip` (GoldHex) or add a `new-dot` case in `HuntEdictSurface.cs:20-58` |
| L0 single tab | When `visibleTabs.Length==1`: `navWidth` 48 (:97), portrait a thin row about 38 tall, `cellHeight` ≤ 48. Hide the label and centre the glyph | `IconButton` pattern (W:258-262). Keep the buttons `edict-tab-skills` and `edict-tab-survival`. S:Tut.Progression 41 allows only the one visible tab and :102 requires exactly overview, skills and survival in town. `Option area` x/y derive from `navWidth` (:112) |

### 11.3 Group chips (Su:44-59)

| Part | Change | Trap |
| --- | --- | --- |
| Second line | The name from `QuickPresetSelection(scope)` (Qp:15-21), or "Custom". Empty when any id is not `Disclosed`, the tab is `autoEquip`, the group has `bag.warehouseFull`, or it is `survival.potion` | `QuickGroupSummary` (Qp:91-97, currently unused) is close (drop its `VisibleSummary` text). `HuntEdictQuickPresets.For(scope)` throws for scopes without presets (all 10 auto-equip groups) |
| Height | The `tabHeight` measurement (:46-50) must include the second line | S:Sec 67: no clipped chip Text |
| Changed dot | Keep the "•" (:58). Hoist `Session.Baseline` out of the :54 lambda | `Baseline` deep-copies on every access (`HuntEdictLoadout.cs:99`) |
| New dot | Same dot as on tabs, hidden on the selected chip | |
| Keep | Button `edict-group-<ids[0]>`, `GroupGlyph` (`Glyphs.cs:8`) | S:Sec 64-69: inside `Option area`, no ScrollRect ancestor (:65), `SkillTreeGraphic` present (:66), no overlaps |
| N2 default group | New group > remembered group > first group | Su:42 uses one `selectedGroup` regardless of tab. A per-tab dictionary is needed |

### 11.4 Group body

| Part | Owner | Change | Reuse and trap |
| --- | --- | --- | --- |
| Picker | Qp:47-63 | Already matches the mockup | S:Qp 82: no clipped Text |
| Option rows | Su:100-108 (two lines, ≥62) | One row: `Row(list,"Option "+id,max(36,34*lastScale),"option-card")` holding the label, `?` and the value well on the right | Op:42-52 is a ready one-line pattern (35 tall, "option-card" + "input-well"). `?` is `Button(..,"tab-idle")` named `edict-help-<id>`. Every `edict-option-<id>` must exist in custom mode (S:Shared 92, S:Qp 136) and be absent on a first visit or in simple mode (S:Qp 134, 137). Disabled rows stay as `interactable=false`. The changed bar has no native equivalent |
| Reason line | Su:93-97 | Keep | |
| Section titles | Op:11-12 `SectionTitle` (24 tall, truncates) | Use the wrapping `Heading` (Ov:43) at 12pt | Called at Su:89-90 |
| Quick-preset dialog | Qp:64-90 | Unchanged (choosing applies, Qp:22-46) | "Custom" is last and the count is `For+1` (S:Qp 65) |
| Number dialog | Su:146-155 | The range line (:152) and the error Text (:149) already exist. Only the chips are new: percent options (`d.id.ToLowerInvariant().Contains("percent")`, the same test as `HuntEdictSummary.cs:33`) get 10 to 90 filtered by min, max and step, at most 6 | A chip only fills `input.text` (the tutorial tests `=="60"`, `GameUI.TutorialComparison.cs:42`). Never `Repaint`. Push the current y slots (range 98, error 125, apply 178, height 230) down. Keep `edict-number-input` and `edict-number-apply` |
| Choice, set, order | Su:121-145, Op:56-66 | Frame only | Order uses drag cards (Op:79-92; `HuntEdictOrderCard.Dragging` blocks refresh, W:50), so do not port the mockup's ↑↓. The apply button is `edict-choice-apply` |

The `bag.warehouseFull` group and the `autoEquip` tab return early to `Eq:11,141` (Su:79-83). Keep those native screens.

### 11.5 N4 strip

- **Owner:** Su:60-61. **Change:** at `top` insert a fixed, outside-the-scroll `Button(body,..,"chip-on")` named `edict-beat-strip`, then `top += strip height + 4`. Heights are 28 collapsed (same in both orientations) and 51 (landscape) / 117 (portrait) open, times `Grow`. `stripOpen` is a window field and changing it calls `Repaint`.
- **Data:** the fix is the rule body from `Prog.Guide(id)` (Prog:95). The problem and result texts live in GameUI (`tutorialFocus`, `GameUI.Tutorials.cs:17`; the first-result guide's danger text, `GameUI.EdictNotices.cs:24-29`).
- **Traps:** keep it outside the ScrollRect. The `Selected edict group` viewport must stay at least 90 (S:Sec 70); the worst case is landscape auto-equip: two chip rows + the caption band + an open strip. State owned by GameUI needs an explicit setter and `Repaint` (`StoreViewBinding` reacts only to store commits). Lock the button under the default-settings lock (the repeat tab is wrapped at W:121).

### 11.6 Overview

| Part | Owner | Change | Trap |
| --- | --- | --- | --- |
| Style cards | Ov:30-65 | Add two fact chips as **non-Button** Panels (`chip-off` + Text). Raise the LayoutElement before the row-height equalisation (:59-63). Facts: the picks of `HuntEdictQuickPresets.Style(id).For(class)`, then the preset's `survival.lowHpPercent` and the dodge preset's `.Name` | S:Ovw 83-100: Buttons under `Option area` must be exactly the 3 styles + `edict-default-settings`, with 1 ScrollRect and unclipped style Texts. The `balanced` preset has no `lowHpPercent`, so fall back to the option's `initial`. `GlobalValue` is private |
| Default row | Pz:69-80 | Keep (already height 0 without Sharing) | Names `edict-default-settings` and `edict-default-status` (S:Sec 141) |
| Teaser | Ov:27 | A new **non-Button** Panel `edict-teaser` in the same list. The bar is two Panels | Inside `DisableForDefaults(custom)`. Skin `skill-card` or a new one |
| `Next` | Prog:179-188 | Returns a string. Its only consumer is Ov:27 and there are no tests | The old `en.txt` lines 6564-6571 sit below the marker (line 3492), so they are harmless |

The structured replacement of `Next` is `Prog.Upcoming(a)` returning `readonly struct EdictNext { kind(first|rift|rune), guide, title, stage, now }`, with the same branch order as `Next()`: legacy returns null; no `Dodge` returns first/F05; no `Styles` returns stage 2; best clear ≥ 5 without `Details` returns stage 6; otherwise the lowest-stage rule with ids not yet granted (skipping `special-equipment`); rune once best ≥ 15. `now` is `ContentUnlocks.AccountClear` (`ContentUnlocks.cs:37`). The 15-step ladder behind "n/15" does not exist natively (mockup `engine.js` 30-35) and must be added. The page count exists on the tutorial branch as `TutorialChapters.Pages(a)` over `All.Length` (11).

### 11.7 Dialogs

`HasDialog` is `modal!=null` (W:37).

- **Frame:** `Dialog()` at W:195-202 builds quick preset, number, set, order, choice, help, `Error`, `Confirm` and `RequestLeave`. It sets `modal` (:197), adds `edict-dialog-dismiss` (:203-212), a 450-wide `dialog-panel` and `edict-dialog-close`.
- **Skill bubble:** Sk:285-324 sets `modal` (:300) and creates `edict-skill-menu-dismiss`, a panel with skin `skill-action-bubble`, and buttons `edict-slot-remove|equip|policy`. The tutorial dismisses it through `HasDialog`/`HandleBack` (`GameUI.TutorialStaging.cs:237-238`).
- **No footer zone:** apply buttons sit at `h-48`. Keep that.
- **Close-confirm differs:** the mockup's "Cancel/OK" is not the native close flow `RequestLeave` (W:157-165: revert, or save and leave). `Confirm` (:223) is used only for skill reset.
- **Traps:** `Reflow` restores dialogs through `restoreDialog` (W:61-77). An outside click must still reach the close behaviour over the tabs (S:RecommendedEquipment.Controls 67). Keep the caption band reserved while a dialog is open.

### 11.8 `DisclosureDisplayKey` (Pr:62-63)

Add derived scalars only:

- `string.Join(",", new guide ids)`.
- `AccountClear`, `contentUnlocks.firstRunEnded` and `guide.runeBoard?.step` for the teaser.
- `TutorialChapters.Pages` for the page count.
- The latest record id or `Count` for the N4 problem text.

Do not add the `guide.tutorials` JSON. `Tutorials.Discover` (`GameUI.HuntEdict.cs:13`) and `Tutorials.Record` (called every frame, `GameUI.EdictNotices.cs:16`) mutate that data, so adding it would rebuild the window on an unchanged save and break S:Tut.Progression 104-105. Do not add draft state, time or the GameUI focus either (they need an explicit `Repaint`).

### 11.9 What the "new sentence" computation (N1) reads

- **Where:** `store.Data.guide.tutorials` (`FirstPlayGuide.cs:20`), a list of `TutorialRecord` (`Tutorials.cs:8-12`).
- **Not performed:** read `practiced`. `TutorialProgress.EdictSaved` fills it through `Tutorials.Performed`, gated by `GuideVisible` (`Tutorials.Progress.cs:31-60`).
- **Lookup:** like `TutorialChapters.Finished`: `tutorials.Find(x => x.id==g && x.hero==(Tutorials.Definition(g)?.perHero==true ? hero.id : "") && x.variant=="")` (:30-34). Do not use `Tutorials.Record` (:105-110), which appends a record. Do not use `Finished` itself either (it counts `read` as finished for E* guides).
- **Disclosed:** `Prog.GuideVisible(a, g)` (Prog:96-104).
- **Guards:** skip when `Legacy`, `guide.hintsHidden` or `Tutorials.Mandatory` is true; otherwise old saves light every dot. `Tutorials.Available` handles `legacyExempt`.
- **Rule to guide:** there is no `GuideOf(rule)` helper. The mockup's version (`engine.js:45`) maps `repeat.enabled` to H11 and the first-result rule to F05; `Prog.Guide` (:95) misses both.
- **Rule to groups:** every group that contains any id of the rule.

### 11.10 Groups opened this session

- **Existing patterns:** `GameUI.shownTutorialContexts` (`GameUI.Tutorials.cs:16`, added at :142, read at :150) and `edictNoticePresented` (`GameUI.EdictNotices.cs:7`). Window-lifetime sets (`collapsed` W:29, `customQuickScopes` Qp:13) die at `Destroy` (W:156).
- **Plan:** keep an `opened` set on GameUI and pass it into `HuntEdictWindow.Open` (`GameUI.HuntEdict.cs:44`). Add to it in `SelectTab` (W:140-148) and `SelectGroup` (Su:25-31); both already `Repaint`.

### 11.11 Skills tab (non-legacy path)

Skills-tab line numbers refer to the `main` checkout. On the tutorial branch the same code sits 17 lines earlier after line 196 of `Skills.cs` (only the legacy tree part differs). "Estimated" values are computed from the formulas at reading scale 1 (fixed, `GameUI.cs:71`), not measured.

- **Drawn at.**
  - Layout: `Skills.cs:34-64` (policy rail 76 at :42, manage landscape :52-55, manage portrait list → inspector → bar :57-63).
  - Cards: `Skills.cs:70-75` → `Progression.cs:15-25` → `Card()` (`Overview.cs:30-41`).
  - Inspector: `Skills.cs:376-421` (−/＋/equip at :418-420). Bar: `Skills.cs:333-371`.
  - Bubble: `Skills.cs:285-324` (a modal, so `HasDialog`; placed beside its anchor :316-323).
- **What the mockup really changes.** One slot in the prologue, slot 4 at level 40 (`Progression.cs:13`), the portrait order and the −/＋/equip trio are **already native**. The real deltas are only:
  - A rounded-square passive icon (`Card` hard-codes `Create(..,false)`, `Overview.cs:33`).
  - A green subtitle when equipped (`Overview.cs:38`) and the title colour from gold to text (`Progression.cs:21`).
  - A minimum list height of 120 (`Skills.cs:60`, mockup `styles.css:222`).
  - The slot bubble **above** the slot (mockup `ui.dialogs.js:29-32`; native is beside it).
  - The branch name instead of the element in the inspector meta (`Skills.cs:398-399`, mockup `ui.skills.js:36`).
- **Keep.** `SkillBarHeight` (:336) is the only source of the bar height budget (:46, 52, 59-62). `BarCell` (:333) is also the slot hit width, so the one-slot tutorial hole is as wide as the whole bar. `ActiveHole/PassiveHole/UltimateHole/BarGap` (:29) feed `FrameSize` (:236), the icon (:355) and the inspector (:387-395). The mockup's CSS rings are stand-ins; keep the `NodeFrame` art. Also keep `VisibleSlots` (`Progression.cs:13-14`) and `Grow/Lines` (:30-32).
- **Traps.** Cards must stay Buttons named `edict-skill-<id>` registered in `skillNodeAnchors` (`Progression.cs:22`); in the prologue the smoke allows at most one (`RuntimeTutorialSmoke.Progression.cs:42`). Native rank-down is `rank > MinimumRank` (:418), not the mockup's `r > 0`.

### 11.12 Policy screen (`SkillPresets.cs:53-156`)

Map: back :58, tabs :66-92, plate :93-94, `mainScroll` :96 (`actions=0` at :95 is a spare fixed-strip offset), `Preset explanation` row :115 (info width .28 at :114), live branch :129-149 (`DrawCombatPreview` :140 → `DrawStarterControls` :141, `Progression.cs:26-38`), row height :151.

- **(a) Progress button placement.**
  - Portrait is art-first today (:144-148), so the button ends last. Replace that block with `artY = descriptionHeight + 4; Place(art, artX, artY, artW, artH)` so info comes first (mockup `styles.css:256-257`).
  - Landscape: before :141 add `Row(information, "Starter spacer", 0)` with `flexibleHeight=1`, and at :143 place the info block with `Max(descriptionHeight, artH)` (mockup `styles.css:246`). Widen .28 to about .34.
- **(b) Fit without scrolling.**
  - The prologue viewport is about h−79 (estimated): ≈329 landscape, ≈579 portrait, before the caption band. This screen has no footer (`HuntEdictWindow.cs:93`).
  - Keep content at or below the viewport − 9 (list spacing + `EndList` :155).
  - Today the landscape info column is ≈355 (estimated) and its fixed rows alone are ≈250 (`CombatPreview.cs:37-41`). The portrait row is ≈590.
  - For `ProgressivePrologue`: reduce the stats to one line, drop the cooldown and effects rows (null-guard in `CombatPreview.cs:50-61`), and keep the caption to two lines.
- **(c) Observation progress bar.**
  - The state lives in `Core/CombatPreviewSession.cs`: `Seconds` :29, `StarterObservationComplete` :21-22 (Seconds ≥ 10, a CAST_START, and the private `waited`/`moved` flags :18), `Ended` :28 (the clock stops at 10 s). Reach it through `PreviewCombat` (`CombatPreview.cs:11`).
  - It is readable every frame: `SkillCombatPreviewPresenter.cs:72-75` advances each frame and calls `RefreshLabels` at no more than 30 Hz while visible. That closure (`CombatPreview.cs:44-65`) already prints `Seconds` (:49) and calls `UpdateStarterControls` (:47).
  - Edit: add a fill row before :141 with width `Clamp01(Seconds/10)`, capped at .99 until `Complete` (otherwise you get a full bar with a disabled button). Replace the `10-.001f` literals (`CombatPreviewSession.cs:21, 28`) with a constant.
- **Traps.**
  - The offsets key is `skills-edict` (`HuntEdictWindow.cs:39`). Only `mainScroll` is remembered (:186-193) and it is zeroed at `SkillPresets.cs:32, 86`. Do not create a second `Scroll()`; use `actions`.
  - `previewSkillPresets` is per scope (:11). `ActivateSkillPreset` saves `VisibleSkillPreset` (:36), so radios must set it first (:86).
  - `UpdateStarterControls` (`Progression.cs:55-61`) does nothing while `starterContinue==null`. Keep the order combat preview → progress button and keep the name, because `LessonTarget` returns interactable buttons only (`GameUI.TutorialStaging.cs:253`).
  - There is no per-preset "observed" memory (the session is rebuilt when the preset or signature changes, `SkillCombatPreviewPresenter.cs:27`), so the mockup's "Observed" chip has no native source. The clock also freezes while any dialog is open (`CombatPreview.cs:42`).

### 11.13 Tutorial layer

- **Today.**
  - `PrologueGate` builds "Voice narration" (`PrologueGate.cs:29-32`). The 58·s face crop is the stand-in for the mockup's halo; the name comes from `NpcProfiles.cs:27`.
  - It lives on the canvas "Prologue gate": overlay, sorting order 2000, pixel units (`GameUI.cs:143-147`, created at `TutorialStaging.cs:32-36`). The window canvas is 310.
  - The card is at least 74·s tall (:87-91). Placement (:93-103) is the far edge from the hole, or above/below a hole at least two cards tall, so it floats over `Option area` for big holes.
  - The hole (:70-84) is the world corners of `target` in screen pixels, **with no mask clip**. `blocker.raycastTarget = missing < 2 s` (:80).
- **Docking at the caption band.**
  1. `HuntEdictWindow.cs:112-113`: if `ProgressivePrologue` (or a soft guide line), add a public RectTransform "Guide caption" at the top of "Option area". Move the content into a child host using the repeat tab's body swap (:119-121, `bodyHeight -= band`). Keep the name "Option area" (`TutorialStaging.cs:250`). With today's card the band is at least 74; the mockup's 44/58 needs a smaller face, padding and font (about 51 with a 34·s face). The "sibling" approach of 11.1 is simpler than this one.
  2. `PrologueGate.cs:85-103`: add `public RectTransform Dock`. When set, the narration rectangle is its world corners. Leave it null for the rune board (`GameUI.RuneBoardTutorial.cs:41, 67, 91`) and keep the name "Voice narration" (smoke `RuntimeTutorialSmoke.cs:128`).
  3. `TutorialStaging.cs:239`: `gate.Dock = huntEdictWindow?.GuideCaption`.
  - Reserve the band when the guide starts, never per frame, because `Repaint()` dismisses dialogs (`HuntEdictWindow.cs:138`).
- **(i) A control outside the view counts as missing.**
  - Change `LessonTarget` (`TutorialStaging.cs:246-255`) and the button search (`GameUI.Tutorials.cs:164`).
  - Add a helper that intersects `GetWorldCorners` with every ancestor `RectMask2D`, starting at `parent` (`Option area` itself has one, :112).
  - A control must be fully contained; "Preset explanation" only needs to overlap. On failure return null so the gate's 2 s grace applies.
  - There is no EnsureVisible. The scroll snippet is inline at `Skills.cs:289-298`, and `FocusGlobalOption` never scrolls (`Summary.cs:30`).
- **(ii) Ring → Save.** In `GameUI.Tutorials.cs:155-164` move `edict-save` to the front of `names`. It is found only while interactable, which equals `Session.Dirty` (`HuntEdictWindow.cs:131`). Skip it while `HasDialog`. The poll runs every 0.25 s (:117).
- **(iii) Focus refused by the lock.**
  - `HuntEdictWindow.Summary.cs:19-21` and :27 return silently. Make `FocusGlobalOption` return a `bool` (all current callers are statements) and expose the private lock (`HuntEdictWindow.cs:38`).
  - In `StartTutorialAction` (`GameUI.Tutorials.cs:98-101`) set a guide line and ring `edict-default-settings` (`HuntEdictWindow.Presets.cs:75`, which exists only once Sharing is disclosed, :71).
  - New strings need `en.txt` (the English gate text is checked by a smoke, `RuntimeTutorialSmoke.cs:125`). `en.txt:6254` reads "Voice from Above"; the mockup says VOICE OF THE EDICT.

### 11.14 Town notice card path

- **Today.** `GameUI.EdictNotices.cs:15-16` takes the first entry of `Tutorials.All` (not ladder order). :21 calls `StartTutorialAction` (`GameUI.Tutorials.cs:80`), which sets `tutorialFocus` (:82, GameUI-private, cleared once practiced :137), then `ShowEdictEditor` (:96) and `FocusGlobalOption(id)` (:98-101). The window receives only an option id.
- **Add.**
  - After :96 call `window.ShowGuide(guideId, focusId, title, problem, fix)`. Take the title and the fix from `EdictDisclosureRule.title/body` (`Core/HuntEdictProgression.cs:8`) or `TutorialDefinition.bodyKo`.
  - The window keeps its own copy; the third beat reads `Tutorials.Record(a, id).practiced`. Draw the strip in `Summary.cs:60-61`, between the chips and `editorScroll` (mockup `ui.tutorial.js:94-108`).
  - For the problem text, lift `EdictNotices.cs:24-30` into a helper shared by the card and the strip (the F05 text is `startingHp5s → finalHp`, `burstDamage5s` and the top `fatalSources`). E03 and E07 can reuse the `HuntEdictCoach.Tips` reasons (`Core/HuntEdictCoach.cs:26-70`, no runtime caller today). E04/E05 problem text does not exist (mockup copy only).
- **N6.** The `Save` closure at `GameUI.HuntEdict.cs:41-42` already knows success and `tutorialFocus != ""` (it shows a toast). Call `window.ShowAfterSave(label, action)` there; it runs before `Repaint()` (`HuntEdictWindow.cs:166-170`). Draw it in the footer (:129). The status width is width−210, about 195 at 405, so it is tight. The action is `game.RequestStation(TownStation.Training)` or a toast. Any new display state must enter `DisclosureDisplayKey` (`Progression.cs:62-63`), or `StoreViewBinding` skips the refresh.

### 11.15 Public members the tutorial reads: confirmation

(Owner | reader.) A pure restyle keeps all of them as long as the wiring stays. **Bold** marks the ones that can change silently.

- `SelectedTab` `HuntEdictWindow.cs:35` | `TutorialComparison.cs:15`. It is rewritten at :81, 83, 95.
- **`ManagingSkills`** :36 | :16, 24. False means the policy screen, and it also drives the footer (:93), `ScrollKey` (:39), the preview `Visible` (`CombatPreview.cs:42`) and `HandleBack` (:58).
- `SelectedSkill` `Skills.cs:12` | :18. Only `SelectSkill` (:22) sets it, from a card tap.
- `PolicySkill` :13 | :25. It defaults to "BASIC" and is never null.
- **`VisibleSkillPreset`** `SkillPresets.cs:16` | :28. The per-scope `previewSkillPresets` persists; the mockup resets it on open.
- `ActiveSkillPreset` :15 | :29. It reads the saved hero and allocates a session per call (four per refresh, `Progression.cs:59-60`).
- **`HasDialog`** `HuntEdictWindow.cs:37` | `TutorialComparison.cs` :24, 39 and `TutorialStaging.cs` :237. The skill bubble must stay modal (`Skills.cs:300`).
- `Session.Draft/Dirty` :31 | `TutorialComparison.cs` :17, 44 and `TutorialStaging.cs` :268, 306. Draft is replaced on every `Change` (:175).
- **`PreviewCombat`** `CombatPreview.cs:11` | `Progression.cs:35, 41, 60`. It is null unless `DrawCombatPreview` ran in this paint (released at `HuntEdictWindow.cs:84`).
- `ProgressivePrologue` `Progression.cs:11` is window-internal; the tutorial reads `RunState.tutorialFlowVersion` (`TutorialStaging.cs:258`). A new code path that tests `Legacy()` or level < 2 could break one tab, one card, one slot and no Custom.
- `QuickPresetSelection` `QuickPresets.cs:15` and `SelectedGroup` `Summary.cs:13` (the key is `ids[0]`, `Core/HuntEdictSummary.cs:9`) are read only by the v2 flow (`TutorialStaging.cs:267-316`). N2's default opening changes the group chosen at `Summary.cs:42`.
- GameUI also calls: `HandleBack`, `SelectTab`, `SelectSkillPage`, `FocusGlobalOption`, `SelectSkill` and `OpenSkillPolicy` (`GameUI.FirstPlay.cs:22`, `GameUI.TrainingGround.cs:41`), `Session.ReplaceDraft` + `Repaint` (`GameUI.Comparison.cs:213`).

### 11.16 Cross-cutting traps

1. `Repaint` rebuilds everything and closes dialogs. The tutorial ring re-attaches every 0.25 s (`GameUI.Tutorials.cs:117`). Never `Repaint` per frame.
2. `Button()` names the GameObject by its label unless you set `.name`. Smokes find "확인" and "취소" by label. A guide target must be `interactable && activeInHierarchy` (`GameUI.TutorialStaging.cs:253`).
3. Put every new Button inside a `DisableForDefaults` region.
4. `Text()` runs `Loc.T` on its input. New Korean literals need `en.txt` entries and composed lines must use `Loc.F` (`LocalizationTests.cs:239`). The mockup's `OWN_EN` (`ui.core.js`) is the starting set.
5. A Text rectangle must be at least its `preferredHeight` (S:Tut 132-133 checks 10 profiles).
6. Mockup simplifications not to port: plain auto-equip rows, the ↑↓ order buttons, the toast-only code share, and the "Cancel/OK" close dialog.

## 12. Affected smokes and tests, and new checks

As of 2026-10-05, read from code, not run. `P/` = `Assets/HELLSCRIPT/Runtime/Presentation/`, `T/` = `Assets/HELLSCRIPT/Tests/Editor/`. Change codes: (a) one-line option row (36) (b) second line on group chips (c) narrow L0 tab column (d) reserved caption band (e) overview teaser (f) N4 strip (g) footer `edict-save-next` (h) chips and range line in the number and choice dialogs.

### 12.1 Read this first

- **The baseline may already be red.** Since 93d59959 (2026-10-04) a new account is a progressive-disclosure account (`Core/GameStore.cs:265`). Only `RuntimeTutorialSmoke.cs:146` sets `edictLegacyAccess`. A `highestClear=0` fixture shows only the overview, skills and survival tabs, so the "10 tabs / 35 groups" assertions below fail unless the fixture grants access (`guide.edictLegacyAccess=true; HuntEdictProgression.Reconcile(a)`).
- **SharedUi is stale in a different way.** `RuntimeSharedUiSmoke.cs:87-92` clicks `edict-quick-picker-*` even on groups with no quick picker (`survival.potion`, `bag.warehouseFull`, `autoEquip.*`). Before blaming the surface rework, run each smoke once on an untouched branch to fix the baseline.
- **Branch differences.** The smokes are identical in both checkouts. The tutorial branch differs only in Core, data and tests.

### 12.2 Smokes and tests to fix or confirm

| File:line | Assertion | Breaks? | Fix |
| --- | --- | --- | --- |
| `P/RuntimeEdictOverviewSmoke.cs:88-91` | `Option area` buttons are exactly 3 `edict-style-*` plus `edict-default-settings`, one ScrollRect, no `SkillIconView` | Only if the teaser, tendency chips or caption are Buttons, a second ScrollRect or icons (e) | Keep. Teaser and chips are Image + Text inside the existing list |
| `:92-98` | Style cards inside the viewport, no clipped Text | No (by calculation, card + teaser + 44/58 caption fit all five sizes). Chip Text gets its rectangle from `preferredHeight` | Keep |
| `:59-74` CheckFrame | 10 tabs and save/revert inside the frame, tab centre raycast, no clipped tab or scroll Text | Not for (c). Shrinking while the subtitle Text stays on breaks :72 | Keep. Hide the subtitle with `SetActive(false)` |
| `P/RuntimeSharedUiSmoke.cs:66-73` | `Main tabs`, `Fixed save controls`, `Option area` are children of `Hunt Edict window`, body above the footer, 10 tabs inside the nav | No while names and parents stay and the caption is a sibling | Keep |
| `:97`, `:115-119` | Choices found by `GetComponentInChildren<Text>().text==Loc.T(..)`, `Single(InputField "edict-number-input")` | :97 breaks if a one-line value pill shows the same text without " ›" (a). :119 breaks if the chip is an InputField (h) | Find by button name. Chips are Buttons |
| `P/RuntimeHuntEdictSaveLayoutSmoke.cs:25-56,72-77` | `Node()` is the first of that name, save and revert are `Single`, save is right of revert on one line | No. `edict-save-next` does not hit a `Single` (g) | Keep. Keep the name unique |
| `P/RuntimeSkillTreeSmoke.Sections.cs:14-49` CheckDefaultLock | Every Button except 4 disabled ones (:19), `Option area/Overview custom settings` + CanvasGroup (:20), ScrollRect lock (:31-38) | If `edict-save-next` sits outside the dimmed footer, or the caption/teaser is a Button (g) | Keep. Keep the paths |
| `:57-73` CheckSectionFrame | Chips inside `Option area` (:64), outside ScrollRect (:65), `SkillTreeGraphic` (:66), no clipped Text (:67), viewport ≥ 90 (:70), one Text equals `Description` (:72) | :67 if the second line (preset name) is not measured (b). :72 if the description is clipped or moved. :70 holds (worst auto-equip 10 groups + caption + N4 leaves about 120+ logical px) | Put the subtitle in the measurement loop (`P/HuntEdictWindow.Summary.cs:47-50`). Keep an exact description Text inside the scroll |
| `:50-56`, `:74-91` | All ScrollRects vertical, the preset page has 0 ScrollRects | Only if the caption shrinks `Option area` on the preset page (d) | Keep (the fixture sets `hintsHidden`) |
| `P/RuntimeSkillTreeSmoke.Presets.cs:24,33` | 10 tabs + activate, back and common policy inside the frame, no clipping, `Skill preset content` viewport > 40 | No | Keep |
| `P/RuntimeSkillTreeSmoke.cs:351` | `edict-tab-combat` centre click hits `edict-skill-menu-dismiss` | No (the backdrop covers the whole window, `P/HuntEdictWindow.Skills.cs:302`) | Keep |
| `P/RuntimeEdictQuickPresetSmoke.cs:77,82,134-137` | Tabs and save/revert inside the frame, no clipped picker Text, no `edict-option-*` before direct setup and all present after | No. (a) must also build the disabled option rows. Search can be hidden in the prologue only | Keep |
| `P/RuntimeRecommendedEquipmentSmoke.cs:57-63`, `.Controls.cs:58,62,67` | Same frame checks + `CheckText`, an outside click at the centre of `edict-tab-overview` closes the dialog, `Dialog` 3px corners are empty | No: dialog ≤ 450 wide, landscape tab column x < 172, portrait tab row at the top. The chips of (h) must not reach those corners | Keep |
| `P/RuntimeTutorialSmoke.cs:111-139` | :119 `Single("Hunt Edict window")`, :125 no Korean in the English gate Text, :128 `Single("Voice narration")` vs pause/resume, :132 button Text not clipped, :134 footer buttons inside the frame | :128 breaks if the caption replaces the gate card (the card is inactive so `Single` throws). :125 loses coverage. :132 breaks if 48px tab labels stay on (c) | :128: take the active card with `FirstOrDefault`, else `Guide caption`. Add a no-Korean check on the caption Text. Disable the label on narrow tabs |
| `:74-83` FollowGate | The centre of the only tab hits "Input blocker" | No (the hole is exactly the target rectangle, `P/PrologueGate.cs:76`) | Keep |
| `P/RuntimeTutorialSmoke.Progression.cs:41,102,105` | During the lesson only `edict-tab-skills`, town tabs are overview, skills and survival, an unchanged save keeps the `edict-save` instance | No if hidden tabs are inactive and N1 reads only inputs of `DisclosureDisplayKey` | Keep |
| `:108-111` | After `highestClear=3` `edict-tab-combat` exists | Breaks on the tutorial branch (the combat tab opens at rift 4, main 3) | Change to 4 after the merge |
| `P/RuntimeButtonUxSmoke.cs:184-185` | Every `UiButton` has a non-raycast Face and matching Chosen/Available, `Single(edict-save)` | No if new buttons are built with `Button()` | Keep |
| `P/RuntimeRiftEntrySmoke.Repeat.cs:58-71` | `edict-tab-repeat` is locked and matches its own centre ray, :63 `Loc.MissingCount==0` | Not the tab. :63 breaks on any new Korean string drawn on the skills page without an `en.txt` key (including a pre-built hidden subtitle) | Add the keys. Build the subtitle only when visible |
| `P/RuntimeLanguageSmoke.cs:183-187`, `P/RuntimeUiStyleSmoke.cs:69-70` | The legacy page of `ShowLegacyEdictEditor`, capture only | No | Keep. Refresh the screenshots |
| `T/HuntEdictOverviewTests.cs:24-166`, `HuntEdictDefaultsTests.cs:70-74`, `SharedUiTests.cs:91-104`, `StoreViewBindingTests.cs` | Tab and tendency ids, coach tips, group fields, 35 groups, the generic harness | No | Keep. No test covers `DisplayKey` (`P/HuntEdictWindow.Progression.cs:62`), so add new test (2) |
| `T/HuntEdictProgressionTests.cs:65-74` | 152 ids, `Single(stage==18)` is 40, TestCase | Not from the surface rework. The tutorial branch breaks at :69 and :73-74 | Adopt the branch file (`Where(stage==18).Sum==40`, `Count==3`, low HP@3, position@5, `target.chaseDistance` and `bag.replacementRank` in 18). Do not put the 15-step ladder in `HuntEdictProgression.json`. The rule getter allows each option id exactly once and asserts the stage-18 count |
| `T/TutorialProgressionTests.cs:20`, `TutorialChapterTests` | 52 definitions (branch 54), chapters | No | None. After the merge, derive the teaser page count by dividing `TutorialChapters.Pages(a)` by `All.Length` (11) |

Other smokes that only find and press by name (`RuntimeRiftEntrySmoke.Portal` and `Preparation`, `RuntimeTrainingEquipmentSmoke:121-158`, `RuntimeTrainingGroundSmoke:176-246`, `RuntimeRiftVictorySmoke:232`, `RuntimeFirstPlayAcceptance:200`) stay as they are. `RuntimeSkillTreeSmoke.CombatPreview.cs:51` checks the preview control Text for clipping.

### 12.3 `LocalizationTests` and `en.txt` (`T/LocalizationTests.cs:177-256`)

- Every non-interpolated Korean literal in `Runtime/**/*.cs` needs an exactly matching key in `en.txt`. Files whose name contains "Smoke" and `Localization.cs` are excluded. `$"…Korean…"` is rejected, so use `Loc.F`.
- `en.txt` format: one tab per line, no duplicate keys, Korean in keys and none in values, the same `{n}` set and `|` count.
- Keys above `# --- composed at runtime` (`en.txt:3492`) must still be source literals. When you reword or remove a line, delete the key or move it below the marker. Composed sentences and JSON text go below the marker.
- 38 of the mockup's 49 explicit Korean/English pairs are not yet in `en.txt` (for example the next sentence, pages received, "Edict saved", the problem you hit, the one thing to fix, what changed). The tutorial branch edits the same area of `en.txt`.

### 12.4 Repository checks

- **`tools/check_ui_contract.py`**: the concatenation of `HuntEdictWindow*.cs` must contain `ContentWindowHost.Attach`, `UiTheme.Scale`, `HuntEdictSummary` and `Session.Draft` (`ADAPTERS` :15). `Font.CreateDynamic` is forbidden (:55). Quoted 6- or 8-digit hex colour literals are forbidden, use `UiTheme` tokens (:71). `class …Window` is allowed only in files named `HuntEdictWindow.*` (:57). `typeof(Canvas)` is allowed because the owner is in `ADAPTERS` (:62). `Runtime*Smoke*.cs` is excluded (:46). **A new partial must be named `HuntEdictWindow.<X>.cs`**; a `partial class HuntEdictWindow` in a file with any other name, or a new `…Window` class, fails.
- **`tools/check_ui_refresh.py`**: scans top-level `Presentation/*.cs` only and skips `Runtime*` (:50). Every `StoreViewBinding.Attach(` needs 5 or more arguments or `visibleState:` (:56). `HuntEdictWindow.cs:50` has 5. A second `Attach` for N6 needs a key. A new partial is only added to the list.
- **`tools/test_ui_contract.py`**: `test_current_source` copies `Presentation/` and requires `check()==[]`, so the same rules apply. Add a `.meta` for every new file.

### 12.5 New tests

1. **Contract smoke.** `P/RuntimeEdictContractSmoke.cs`, flags `-hellscriptEdictContractSmoke -hellscriptUiNames <ui-names.json>`. Its `Runtime…Smoke` name keeps both check tools away from it.
   - **Parsing.** Read the top-level keys with a regex. `JsonUtility` cannot read dictionaries and Newtonsoft is not installed.
   - **Pattern expansion.** `<tab>` = `HuntEdictUiCatalog.Tabs` filtered by `HuntEdictProgression.Tab`. `<ids[0]>` = `HuntEdictSummary.Key` of the groups that pass `GroupDisclosed`. `<option id>` = for each group after `FocusGlobalOption`, the ids that pass `Visible` (with help: `HuntEdictUiCatalog.Help(id)!=""`). `<scope>` = `HuntEdictQuickPresets.GlobalScope(g)` for groups with a picker. `<preset id>` = `For(scope)` plus `custom` (absent in the prologue). `<slot>` = 0..4, `<n>` = 0..3 (+ `edict-ultimate-slot`), `<skill id>` = `ClassSkills.For(hero.heroClass)`. `<speed>` = 1 only in the prologue (`P/HuntEdictWindow.CombatPreview.cs:35`). `<key>` = a `Window.Search` result.
   - **Names to handle separately.** `Skills page` is a mockup wrapper and does not exist in Unity. `edict-caption` is the same element as `Guide caption`; the final name is an open decision (chapter 15). `menu-hunt-edict` is a dock button, found through `game.UI` (chapter 6.1: the object name is "사냥 칙령"). The JSON carries no component kind, so keep a separate name → kind table.
   - **Stages.** The prologue skills and survival pages (new account, `game.EnterPlaza(true)`, `Until(game.TutorialActive)`, as in `Progression.cs:19`), the three town tabs, then `hero.highestClear=N; HuntEdictProgression.Reconcile(a)` for rifts 3, 4, 6, 9, 12, 15 + rune and 20. Each stage runs at the five sizes × Korean and English.
   - **For every visible name.** It is found and active, `IsInteractable()` matches the expectation and `Face.Available`. It is inside `UiSafeArea.Current`, and a first-screen control is inside `scroll.viewport` before it is scrolled to the centre. The centre ray hits it, Buttons in the same container do not overlap, and Text is not clipped. Names that must not exist do not.
   - **Invariants.** Chips sit outside the ScrollRect. `Guide caption` is above `Option area`, its height is 44/58 × `CanvasScaler.scaleFactor` and `Option area` is shorter by exactly that. `edict-beat-strip` sits between the chips and `Selected edict group`. `edict-save-next` is inside `Fixed save controls`, does not overlap save or revert, disappears on the next change, and the `edict-save`/`edict-revert` instances survive the 12-second expiry. Browsing changes neither the `Session.Draft` JSON nor `Store.Revision`.
   - **Helpers to reuse.** `P/RuntimeEdictOverviewSmoke.cs`: `Boot` and `Error` :19-28, `Find(name)` :29 (the same one in the QuickPreset and RecommendedEquipment smokes; the full `game.UI` search variants are `RuntimeTutorialSmoke:34` and `RuntimeRiftEntrySmoke:26`), `Bounds` :31, `Trail` :30, `Tap` (raycast proof) :33-47 (it scrolls to the centre first, so run the "inside the viewport" check before it), `TabsHit` and `Settle` :51-57, `Resize(w,h,lang)` :75-80, `Capture` :81, the size array :125. Also `Within(a,b)` (±1px) `RuntimeSharedUiSmoke:26`, the viewport check in `Click` `RuntimeSharedUiSmoke:32`, pairwise overlap and text overflow `RuntimeRiftEntrySmoke:49-57`, `UiSafeArea.Simulate` `RuntimeSkillTreeSmoke.Sections:146`, fixtures `RuntimeSkillTreeSmoke.ExistingTrees` (:22), `ContentUnlocks.RecordRunEnd` (`Progression:115`) and `runeBoard.step=RuneBoardLesson.Complete` (`ButtonUx:122`). Window-state API: `SelectTab`, `SelectGroup`, `FocusGlobalOption`, `OpenSkillPolicy`, `SelectSkillPage`, `Search`, `HasDialog`, `PreviewCombat`.
2. **Edit Mode `T/HuntEdictSentenceTests.cs`.** N1 and the teaser logic must live in Core without `UnityEngine` (`EdictTeaser`, chapter 9). Port the four cases of `Prototypes/HuntEdict/engine.test.cjs` (earlier guides count as performed, a change + save marks the guide performed, new-sentence display, the teaser walks the schedule).
   - **Unperformed guides.** `NewRules` = "disclosed ∧ `GuideVisible` ∧ not `practiced`", and earlier guides are set through `Tutorials.Performed`. Table: end of rift 1 → F05, main at r3 → E03 and r5 → E05, tutorial branch at r3 → E05 and r5 → E03.
   - **Undisclosed rules.** On the branch at rift 5, after `HuntEdictQuickPresets.Apply(d,"global/survival.lowHp","careful")`, `TutorialProgress.EdictSaved` performs E05 and does not perform E18.
   - **Group and tab marks.** Opening a group clears only its own dot. A tab mark is the union of its group marks. Undisclosed rules are never marked. Do not add an `AccountGuide` field.
   - **Display key.** The key changes when `Performed` or `edictUnlocks` change, and does not change on an unrelated gold save.
   - **Teaser walk.** Before the first result: First, key F05, step 1. Best clear 1→2 (E02), 2→3 … 5→6 (`details`), 19→20. Twenty without a rune: Rune H11, step 15. With a rune, or on a legacy account: none. `Now==min(best,Stage)`, `Total==15`, `Sentence` never decreases. The five `Next()` strings match while both coexist.
   - **Pages.** On main `PagesTotal==0`, so the page count is hidden. After the merge, `PagesReceived==TutorialChapters.Pages(a)` (of 11), and receiving `C00` gives 1.
3. **Lesson-replay extension (`P/RuntimeTutorialSmoke*.cs`).**
   - **Hook.** Add an `Action each` parameter to `LayoutProfiles` (:111). In the loops at `Progression:37`, `:81` and `cs:72`, run `yield return LayoutProfiles("gate-"+name, null, () => AssertInView(game.UI.Gate.Target))` once per distinct `gate.Target.name`. The last one is `Resize(440,956,"ko")` (:138).
   - **Per profile.** The target rectangle is inside `UiSafeArea.Current`; if it is under a ScrollRect, `Within(Bounds(scroll.viewport),…)` holds before `Tap` scrolls to the centre. For area targets (`Option area`, `Preset explanation`) the child controls (`edict-starter-next`, pause/resume, `edict-preset-activate`) are on screen. The centre ray hits the target or its children, not "Input blocker". The caption does not overlap the target, and every v3 caption string (`P/GameUI.TutorialComparison.cs:8-46`) fits 44/58 in Korean and English, with no Korean in English.
   - **Designated list.** `menu-hunt-edict`, `edict-skill-<first skill>`, `edict-skill-rank-up`, `edict-skill-equip`, `edict-save`, `edict-active-slot-0`, `edict-slot-policy`, `edict-preset-tab-<B>`, `edict-preset-activate`, `edict-starter-next`, `starter-observation-area`, `starter-selection-area`, `edict-close`, `edict-option-survival.potionHpPercent`, `edict-number-input`, `edict-number-apply`.

### 12.6 Running only what is affected

`AGENTS.md` has no smoke-run recipe (it only describes the MCP `run_tests`). The procedure used on this computer:

- **Edit Mode.** Clone the project (`cp -Rc Assets Packages ProjectSettings Library <work folder>/proj; rm -f proj/Temp/UnityLockfile`). Run `Unity -batchmode -projectPath <proj> -runTests -testPlatform EditMode -testFilter "Hellscript.Tests.X;…"`. Build the filter in a bash array; a broken filter silently runs 0 tests.
- **Development build.** `-executeMethod Hellscript.Editor.ProjectBuilder.BuildMac -hellscriptBuildOutput <x>.app -quit` (about 2 minutes).
- **Running a smoke.** `<x>.app/Contents/MacOS/HELLSCRIPT <flags> -hellscriptSavePath <empty folder> -hellscriptEvidencePath <folder> -screen-fullscreen 0 -screen-width W -screen-height H`, from a bash script with `ARGS=(…)` arrays (zsh does not split words). The local runner (`Artifacts/Validation/FinalMain20260926/Runner/run_smokes.py`, which may not be in the repository) handles resumes and the `HELLSCRIPT_*_OK` markers, and its plan covers only ButtonUx, QuickPreset, SaveLayout, SharedUi, SkillTree, Language, RiftEntry and Tutorial.
- **Flags.** `-hellscriptEdictOverviewSmoke`, `-hellscriptSharedUiSmoke` (each with a `…Resume` variant), `-hellscriptEdictSaveLayoutSmoke`, `-hellscriptEdictSectionsSmoke`, `-hellscriptSkillPresetSmoke` and `-hellscriptSkillMenuSmoke` (resume with `-hellscriptSkillTreeResume`), `-hellscriptQuickPresetSmoke` (+`…Resume`), `-hellscriptEdictControlsSmoke`, `-hellscriptRecommendedEquipmentSmoke`, `-hellscriptEdictProgressionSmoke -hellscriptEdictClass 0|1|2` (+`-hellscriptTutorialResume`; the v2 flow is `-hellscriptTutorialSmoke`; both take `-hellscriptEvidencePath`), `-hellscriptButtonUxSmoke`, `-hellscriptUiStyleSmoke`, `-hellscriptRepeatSettingsSmoke`, `-hellscriptLanguageSmoke`.

## 13. Verification plan and capture list

While iterating, run only the checks directly related to the change; run the full Edit Mode suite and the runtime smoke batch once, after the last code change and the `main` merge (global rule).

| Item | Content |
| --- | --- |
| Static checks | `python3 tools/check_ui_contract.py`, `python3 tools/test_ui_contract.py`, `python3 tools/check_ui_refresh.py` |
| Edit Mode | The updated and new tests of chapter 12, `LocalizationTests`, `StoreViewBindingTests` |
| Runtime smokes | The updated and new smokes of chapter 12 (development build, one set at a time) |
| Refresh scenarios | Unchanged autosave, changed save, save during press, child-window close, repeated tab switches and re-entry, rapid clicks, Korean and English, save failure, all in a real runtime |
| Sizes | 440×956, 956×440, PC 16:9, 16:10, 21:9 × Korean and English (default text size only) |
| Docs and wiki | Korean and English docs plus `Wiki/history`, `tools/wiki.py build` and `check`; publish only from the merged `main` |
| Artifacts | Record owner, path, size and retention in `artifact-lifecycle.json` and include approved cleanup in the completion criteria |

**Capture list (brief chapter 7, item 6).** Keep the four screens below at portrait 440×956 and landscape 956×440, in Korean and English (16 images). For the three PC sizes, one overview and one guide-state image each.

| Screen | Mockup reference images (`Prototypes/HuntEdict/evidence`) |
| --- | --- |
| Overview | `stage-r3-overview-N1-N3-*`, `stage-r20-overview-*` |
| Group detail + quick-preset choice | `focus-r5-before-*`, `focus-r5-dialog-*`, `stage-r9-bag-custom-*` |
| Number-entry dialog | `survival-02-input-*` |
| Skills tab + use-policy screen | `skills-r12-*`, `lesson-07-observe-A-*`, `lesson-11-select-*` |

Checks that were not run must be reported as "not run". Real-device and human-comprehension results are recorded separately.

## 14. Mockup verification results and limits

- `node Prototypes/HuntEdict/engine.test.cjs`: 32 checks pass: the disclosure table of the game's `HuntEdictProgressionTests`, tabs, groups and `Next`, quick-preset apply/match round trips, new-hero Balanced, markers and completion, skills, the prologue lesson.
- `node Prototypes/HuntEdict/browser.test.cjs`: 655 checks pass (headless Chrome, real mouse and keyboard): the prologue lesson (five sizes × two languages, auto-scroll off), tabs and groups per stage, N1–N6 and overlap, dialogs, search, styles, lock, skills, no horizontal overflow, no missing English.
- Not verified: real devices and touch, other browsers, screen readers, new-player comprehension, and agreement with the game code (it is HTML; verify again in Unity).
- Illustrative: the combat preview, the last-hunt problem and three-beat copy, preset slot names, the hero level curve, the page count. Simplified: the auto-equip tab, per-skill detail editing, the share-code dialog, the close-confirm dialog, the ↑↓ of the order dialog, Warrior only, legacy accounts and the legacy editor excluded (chapter 5, "What the port does differently from the mockup").

## 15. Open decisions

1. **Search-row display rule.** The default hides it only in the prologue. Whether to hide it at other low-disclosure stages (the mockup's behavior) is decided after the smoke impact is known.
2. **Caption band name and ownership.** Recommended: the window exposes a `Guide caption` RectTransform (a sibling of `Option area`) and the tutorial draws into it. It is the mockup's `edict-caption`. Confirm the name with the tutorial author.
3. **Page count.** Connect the teaser to `TutorialChapters` once the chapter engine is in `main`; hide the count until then.
4. **Auto-equip and per-skill direct-settings screens.** Not covered by the mockup. Decide the scope of a follow-up using the same tokens.
5. **Legacy-account tree screen.** Out of scope; decide whether to align surface tokens only.
6. **N6 action wiring.** Decide whether "Check in training" really opens the training ground or stays a hint toast.

## 16. Change log

- 2026-10-05: Written from mockup v2. Covers the name mapping table, structural changes, window state and actions, designated areas, the N1–N6 data shapes, requests to the tutorial side, the porting map, affected smokes and tests, and the verification and capture plan.
