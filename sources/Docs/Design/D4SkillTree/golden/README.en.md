# Skill window golden files and reference implementation

Updated: 2026-10-12 · For: Codex implementation work · [한국어](README.md)

This folder lets a machine, not only a human eye, check that the in-game skill window has **the same look and the same behaviour** as the design.

- The design (`../SkillWindow.dc.html`) is an executable specification. The golden files here were produced by running it.
- The reference implementation (`reference/`) is a readable JavaScript rewrite of the design's calculations. **All 55,803 checks match the golden files** (`node reference/test_reference.js`), and deliberately breaking the code makes the checks fail.
- The game (C#) ports the reference implementation almost line by line and runs **the same checks against the same golden files** as EditMode tests. When they pass, rules, layout, wording and interaction match the design.

## Files

| Location | Contents | Used for |
|---|---|---|
| `reference/` | reference implementation (`model` `rules` `layout` `viewmodel` `session`) + `test_reference.js` | the source of the C# port; the comment at the top of each file is that chapter's rule |
| `rules_golden.json` | rules engine answers: **every invest/refund on every node** of 288 allocations (16 hero levels × 3 classes × 6), plus 48 random 70-step replays | the rules engine, `SkillTreeD4RulesTests` |
| `layout/<class>-<landscape\|portrait\|pc>.json` | **every element's rectangle, state and text** for 9 screens × 2 languages, in draw order | the layout function, `SkillTreeD4LayoutTests`; look names, `SkillTreeD4LookTests` |
| `layout_scenarios/<class>-<landscape\|portrait\|pc>.json` | per-element hashes of the scene in **10 states beyond the design sample** (levels 1-60, available modifier sockets, next-rank ghost bars, lit passive medals ...) | dynamic states of the layout function, `SkillTreeD4LayoutTests` |
| `viewmodel/<class>-<ko\|en>-<landscape\|portrait>.json` | the text and enabled flags of the panel, tabs and header after selecting each of the 48 nodes | the view-model builder, `SkillTreeD4ViewModelTests` |
| `interaction/<class>-<ko\|en>.json` | **60 recorded sessions** (select, invest, refund, tab) made by really driving the design; screen state and allocation after every step, plus the **effect signals** each step started (`events`: the glint lines a select starts, the node effect and ignite lines an invest/refund starts) | interaction tests and the smoke |
| `styles.json` | the **look** (CSS declarations) of 428 element kinds, found by state name; the layout files point at these names. **One name determines the look** (the generator fails if a name does not capture a state). Bar widths (`bars`, `bar`) and the lit count of rank arcs (`arc`) differ per element, so they stay outside the look | the drawing code (chapter 9) |
| `panel_parts.json` | the look names of the parts **inside** the detail panel (buttons, blocked-reason line, modifier rows, passive block) for each selected node (48 per class), per class and layout. The design sample (level 24) has no investable state, so **two more states at level 60** (empty allocation, lower tiers full) are added (landscape and portrait only). An identical part object is listed once in `parts[]` and nodes point at it by index | look-name test `SkillTreeD4LookTests` |
| `anatomy/<layout>-<ko\|en>.json` | **measured browser layout** of widget internals: six detail-panel states and the inner rectangles and font sizes of cards, badges, passives, tabs, legend ... | the detail panel and widget internals |
| `rank_values.json` | **per-rank values** of skills, modifiers and passives (computed by a rule) | combat numbers and generator tests |
| `fx.json` | the 31 effect keyframes and each class's duration and curve | effects (9.6) |
| `anatomy/OUTLINE.md` | the measured dimensions above as an indented list (generated) | reading |
| `anatomy/TEXT_STYLES.md` | the 129 measured text styles (generated) | text implementation |
| `tools/` | `css_features.js` (CSS features and colours the design uses), `print_anatomy.js` (measured outline and text styles), `measure_anatomy.mjs` (measuring script, for the design owner), `scene_dump.js` (prints a scenario scene), **`compare_screens.py`** (compares a game screenshot with a reference render, L5) | planning, debugging and verification |
| `build_golden.js`, `verify_golden.js`, `lib/` | the generator and the up-to-date check | when the design changes |

Only `anatomy/` is a measured value (headless Chrome) and is not made by the generator; everything else is deterministic.

## The fidelity contract

What **must match** the golden files:
- every verdict of the rules engine (including the blocked reason) and the resulting allocation, down to the hash;
- layout: every rectangle's x, y, width and height (within 0.01pt), the element count, the draw order, state names and card font sizes (`fitFontSize`);
- wording: panel, tabs, header numbers, node names and accessibility text, blocked reasons, in Korean and English;
- interaction: whether each step was accepted and the resulting state.
- look names: which name (`styles.json`) each element and each button, row and block of the detail panel uses (`style` and `panel.parts` in `layout/*.json`, and `panel_parts.json`). The reference implementation does not compute looks, so the game's `SkillTreeD4LookTests` checks them.

What **may differ** (engine differences):
- the font shape (section 17 D6); where text wraps can vary a little with the font. The line limits (description slot: 2 lines, 3 on PC) hold;
- how gradients, glows and inner shadows are approximated. Use the colours in `styles.json`;
- effects follow the durations and curves in `fx.json` without frame-exact matching.

## How to use

```bash
node Docs/Design/D4SkillTree/golden/reference/test_reference.js   # does the reference match the goldens (under a second)
node Docs/Design/D4SkillTree/golden/verify_golden.js              # do the goldens match the design
node Docs/Design/D4SkillTree/golden/build_golden.js               # regenerate after the design changed (design owner)
```

Read the files from C# tests with `Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Docs", "Design", "D4SkillTree", "golden", "<file>"))`. Do not copy the golden files.

### Allocation string and hash

An allocation is one string. For each active: `rank,chosenModifier,modifierRank` joined by `;`, then `|` and the passive ranks joined by `,`. A modifier that is not chosen is `-1`.

```
2,-1,0;0,-1,0;5,2,3;0,-1,0;3,1,1;2,-1,0;1,-1,0;0,-1,0;0,-1,0;0,-1,0|3,2,0,0,0,0,0,0,0,0
```

The hash is FNV-1a 32-bit over the UTF-8 bytes of that string, written as 8 lowercase hex digits.

```csharp
static string Hash(string text){ uint h=2166136261; foreach(var b in System.Text.Encoding.UTF8.GetBytes(text)){ h^=b; h*=16777619; } return h.ToString("x8"); }
```

### Reading the rules golden

`heroes.<class>.snapshots[]` in `rules_golden.json` is one allocation and every action possible on it.

- `alloc`: the allocation string; `level`: hero level (points = level − spent)
- `spent`, `left`, `tierPoints[5]`, `pointsBefore[5]`, `tierOpen[5]`: derived values
- `inv`, `ref`: a result per node key (`a3` = active 3, `a3m1` = modifier 1 of active 3, `p6` = passive 6)
  - invest: `ok:<hash of the new allocation>` or the blocked reason `tier:3` `max` `pts` `base` `swap` `cap` `key` `lv:25`
  - refund: `ok:<hash of the new allocation>` or `no`

`sequences[]` replays `op` (`inv`|`ref`) and `node` from an empty allocation. Every step's `result` and the final `finalHash` must match.

### Reading the layout golden

`locales.ko|en` in `layout/<class>-<layout>.json` is one screen.

- `frame`: stage size and scale (PC is 1152×648 at 1.25×)
- `header`, `plate`, `panel`: the header (title, tabs, numbers, close), the tree plate (rectangle, inner size, PC zoom and offset) and the detail panel rectangle
  - `panel.parts`: the look names **inside** the panel. `buttons` (`layout` = `header` square buttons | `bottom` wide buttons, `refund`, `invest`, `shine`), `reason` (the blocked-reason line of the portrait layout, `null` when absent), `modifiers[]` (`focused`, `row`, `name`, `effect`, `effectTo`, `right`, `socket`), `passive` (`unlock`, `unlockBar`, `effect`, `effectTo`, `linkedHeading`, `links[]` = `chip`, `level`, `icon`). For an active, `passive` is `null`; for a passive, `modifiers` is `null`.
- `primitives[]`: in draw order (later = on top). Kinds: `band` `tray` `line` `arrowhead` `badge` `card` `modifier` `passive` `tag`
  - every rectangle is `{x,y,w,h,minW}`; `null` means "not fixed" (for example the width of a chip sized by its text);
  - card, modifier and passive `id`, and icon `iconId` (source file name) and `id` (game id);
  - `state`: card `l` (invested) `o` (available) `k` (locked); modifier `chosen` `available` `idle` `unlearned` `tier-locked`; passive `l` `o` `k`; badge `open` `next-locked` `locked`; line `g` `o` `k` `s1` `s2` `s3`;
  - `style`, `glow`, `boxStyle` ... are names in `styles.json`.
- Positions are the same in both languages; only text and card font sizes differ (the generator checks this).
- `views.passives` (portrait): the selection and initial scroll position of the "passive view".

### Using the reference implementation

```js
const R = require("./reference");
const model = R.loadModel(require("../skill_tree_d4.json"));
const s = new R.Session(model, { hero: "Warrior", locale: "ko", orient: "landscape", level: 24 });
s.select({ kind: "modifier", i: 2, j: 2 }); s.invest(); s.tab(1);
s.scene();      // every element of the screen (same shape as the layout golden)
s.viewModel();  // text and enabled flags of panel, tabs, numbers
s.snapshot();   // same shape as one step of an interaction golden
```

Mapping to C#: `model.js` → data loading (`SkillTreeD4.json`), `rules.js` → rules engine, `layout.js` → pure layout function, `viewmodel.js` → view-model builder, `session.js` → the state the window keeps.

## C# port (verified)

`csharp/` holds a C# port of the reference implementation. **It passes the same 55,803 checks against the same golden files as the JavaScript reference** (rules 32,208 · layout 2,975 · layout scenarios 15,034 · view model 600 · interaction 4,986). Deliberately breaking constants and conditions (about 20 in each of JavaScript and C#) makes the checks fail every time.

```bash
cd Docs/Design/D4SkillTree/golden/csharp && dotnet run     # no Unity, about 0.5 s. exit code 0 = everything matches
```

| File | Contents | In the game |
|---|---|---|
| `SkillTreeD4.Model.cs` | data model, allocation string and hash, selection | copy to `Runtime/Core` |
| `SkillTreeD4.Rules.cs` | rules engine `Apply` (with blocked reasons) | copy to `Runtime/Core` |
| `SkillTreeD4.Layout.cs` | pure layout function `SkillTreeD4Layout.Build` -> `SkillTreeD4Scene` | `Runtime/Presentation` (no UnityEngine) |
| `SkillTreeD4.ViewModel.cs` | panel, tabs, numbers, accessibility text `SkillTreeD4ViewBuilder.Build` | same place |
| `SkillTreeD4.Session.cs` | select / invest / refund / tab state machine | same place |
| `SkillTreeD4.Fx.cs` | effect signal rules (`FxOf`, `LineOverlays`): which node gets an effect, maxed rank, newly opened tier, ignite and glint lines with delays | same place |
| `Tests/GoldenJson.cs` `Tests/GoldenShapes.cs` `Tests/Program.cs` | test-only JSON reader, golden-shape adapters, check runner | move into EditMode tests |
| `Tests/SpecLoader.cs` | fills the model from the design export (**test only**) | the game reads `SkillTreeD4.json` through JsonUtility DTOs into the same classes |

- C# 9, `System` only. Coordinates are `double` (the tolerance is 0.01pt, so `float` is fine in the game).
- The port is a **starting point**. Names and folders may follow the game's conventions, but behaviour must keep passing these checks.
- Sorting must be stable (`OrderBy`; `List.Sort` is not). Rounding is JavaScript's round-half-up (`Math.Floor(x + 0.5)`); `Math.Round` rounds half to even.
- No Korean text appears in these files. All text comes from the strings table; only `KeyPrefix` ("Key · " / the Korean equivalent) is missing from the design export and must be added to the game's string table.

## When the design changes

1. The design owner runs `node extract.js SkillWindow.dc.html skill_tree_d4.json behaviour.json`.
2. Rebuild the goldens with `node golden/build_golden.js`; if `node golden/reference/test_reference.js` fails, fix the reference.
3. Confirm `verify_golden.js` passes and commit them together.
4. The game-side tests that fail show what changed. Once implementation has started, **change numbers in the game-side JSON first** and tell the design owner (plan section 5.3).
