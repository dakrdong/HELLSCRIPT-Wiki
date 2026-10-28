# Casual skill window skin pilot

> **2026-10-28 A2 correction:** References to the large edict window, ≡ menu and their dedicated components or smokes are historical. Use the [current entry points](../../Assets/HELLSCRIPT/Runtime/Presentation/GameUI.HuntEdict.cs) and [replacement screens and source owners](Hunt_Edict_Window_Removal_20261028.en.md). Earlier captures and test counts describe the earlier implementation.

2026-10-14 · [Korean](Casual_Skill_Window_Skin_20261014.md) · [Shared rules](Casual_UI_Skin_Rules_20261014.en.md)

Implemented only the skill window in local branch `codex/casual-skill-skin`, based on `origin/main 53d6a3e1` after the verified `083b03f0` integration. Attendance commit `6378320b` is not included. Attendance, HUD, currency, rewards, equipment effects, CI, other UI and existing skill artwork are unchanged. No push, PR, main integration, web build or public deployment occurred.

## Design and approved decision

Actually inspected the [Claude artifact](https://claude.ai/artifact/JsCKnJUbKkzfBYyJDR3493) in the user's Chrome, including its 24 artboards, class tabs, locked/key passives, portrait scrolling and English landscape refund→enabled invest→reinvestment. No Claude message, upload, payment or sharing change was made.

The required prompt was absent at its original checkout path; it was found and read in `wt-casual-art`, branch `claude/casual-art-concept ab5edd76`. The implementation uses `SkillWindow.casual.locked.dc.html`, retaining the original type sizes rather than the larger live canvas. The dark original is retained under `dark/`.

The actual browser body uses Noto Sans KR 400 (11.5px/16.675px), conflicting with the prompt's 700/900 wording. The user explicitly chose the actual design: default body 400, explicit 700/900 emphasis, original font sizes and layout. This decision is implemented.

The live artifact's solid-color background placed before another gradient layer is invalid CSS. Chrome computes transparent backgrounds, no image and a transparent 3px border. Native rendering uses the cream/navy layers specified by the prompt and local design; this is an explicitly reported difference from the live artifact.

The small top-left UI the user observed was a fixed 956×440 landscape interaction-test preview (roughly 52% of a 1596px-wide game area). Production `HuntEdictWindow` supplies no preview orientation and uses PC Fit. Final captures verify actual responsive fitting.

## Implementation

Applied sky/cream surfaces, navy outlines, downward hard shadows, dotted stitching, round gems, tabs, badges, cards, modifier sockets, passive medals, rank arcs, links, legend, detail panels, buttons and FX colors. Removed blurred detail artwork and retained all skill icon paths/art.

The generated catalog has 430 looks, 7 decorations and 105 palette tokens. Global theme ramps and `UiFonts.Body/Display/WebBody` remain unchanged; fonts are opt-in D4 assets. Titles use one mesh effect with reusable vertex buffers rather than eight Outline components per label.

Native checks exposed clipped `Ground Slam` linked text and `Bonus damage (3 shots)`. Their widths now use the actual Canvas scale. Descriptions use native wrapping. Modifier rows/passive effects reserve original reference flow heights even when the new font needs fewer lines. Actual scale changes cause one repaint. Partial-line measurement strips color tags and balances emphasis tags, fixing spurious English effect wrapping. Disabled controls use inert transparent raycast shields and the footer buttons render last, preventing selection/game click-through. Existing test tolerances and behavior assertions were retained.

## Golden invariants and strict conflict

All 12 viewmodel files, 6 interaction files, rules_golden, rank_values and panel_parts are byte identical to baseline. IDs, text, rules, point values, purchase/refund/selection/unlock and FX event meaning are unchanged. JS and C# each pass 55,803 reference checks.

The literal “only styles.json changes / raw layout diff=0” condition is not met because appearance is serialized inside layout. It was not silently reinterpreted as a pass. See [summary](CasualSkillSkinEvidence20261014/final-golden-diff.json) and [all 3,600 changed paths](CasualSkillSkinEvidence20261014/raw-layout-differences.json).

| File group | Exact difference |
|---|---|
| 9 layouts, 3,510 leaves | 1,436 stroke colors; 1,436 widths (2→3); 620 dashes (4 4→5 5); 18 radii (8→18 / 10→20) |
| 9 layout_scenarios, 90 leaves | plate hashes caused by radius changes |
| 18 picked scenes | only plate.radius; other coordinates/rectangles/text/IDs/state/scroll/bar/arc semantics unchanged |
| CSS geometry, 36 combinations | 49,942 strings; zero geometry and font-size difference groups after restoring inner dimensions |
| styles / fx | intentional new appearance and FX; two selected-key ring look variants added |

Old-color/2px-bar assumptions in extraction were repaired to read source structure/numbers without changing gameplay. JS/C# reference plate-radius appearance values were aligned to 18/20. The original raw-diff conflict is resolved for the raw appearance paths under the approval below.

## Approved criterion and separately reported dimensions

On 2026-10-14 the user approved “retain position, size, font size and behavior; permit only design-driven color, stroke and corner changes,” replying **“ㅇㅇ 그렇게 해줘”**. Body 400 and emphasis 700/900 remain approved. The [acceptance review](CasualSkillSkinEvidence20261014/acceptance-review.json) classifies the existing 3,600 raw paths: 3,510 stroke.color/width/dash and plate.radius leaves, plus 90 radius-derived scenarios[].hashes.plate values. There are zero other raw paths. **These raw appearance differences satisfy the approved criterion.** The literal raw diff=0 result remains false in the historical proof.

Zero position/size difference refers to 18 picked-scene contracts: frame.w/h/scale, header/title/tab/close rect/container/gap/size, plate/panel rect and inner/scrollAxis, primitive rect/hit/face/icon.size, paths/bars/arcs. Font-size comparison has zero differences. Text/rules/behavior are supported by byte-identical 12 viewmodels, 6 interactions, rules/rank_values/panel_parts and the existing 60 real interaction transcripts. This is not pixel equality for every glyph or child RectTransform.

**Measured follow-up:** restored unnecessary CSS inner dimensions; 49,942 strings in 36 combinations now have zero geometry/font-size difference groups. Native text allocation retains the original slots where the approved 400 body and explicitly emphasized 700 text fit. Remaining expansions are quantified below and are not blanket-approved by the raw appearance criterion. The user subsequently authorized the five visual corrections and removal of the bottom legend. Final native validation of the latest corner/legend change also passed.

## Fonts

Official Google Fonts Jua and Noto Sans KR sources are hash-pinned; two SIL OFL licenses are bundled. Derivative family/PostScript names start with Hellscript. Subsets retain 2,350 KS X 1001 syllables, Latin and current design text. Noto covers all current characters. Jua lacks several body symbols, which remain in body/vector rendering.

| File | bytes | cmap characters |
|---|---:|---:|
| Jua-SkillSubset.ttf | 2035040 | 2448 |
| NotoSansKR-Skill400.ttf | 515376 | 2604 |
| NotoSansKR-Skill700.ttf | 515908 | 2604 |
| NotoSansKR-Skill900.ttf | 515760 | 2604 |

Total 3582084 bytes. Noto hhea/typo line metrics are normalized to 880 / -220 / 0 without altering outlines or advances. [Font validation](CasualSkillSkinEvidence20261014/font-validation.json) includes output/source hashes. `build_skill_tree_d4_fonts.py` and pinned codepoints provide the recipe; original downloads are retained in the single task font-sources directory. WebBody is unchanged; no web player was built.

## Validation and evidence

- Initial skin implementation (**7ab84dc6**): **102 distinct related EditMode tests passed**: reused 38 unaffected results from the preceding 99/99 run and reran 64/64 for the final wrap/input changes. Panel 47/47 includes original browser geometry/clipping assertions, 12 linked/stat regressions and 3 rich-emphasis regressions. Footer overlap checks were strengthened.
- Looks check, golden verify (41 files), both 55,803 reference suites, UI contract (15 tests) and UI refresh checks passed.
- Initial implementation’s macOS player passed **60 transcripts / 1,662 clicks / 502 blocked / 2,016 panels / 84 captures**, covering 3 classes × KO/EN × 7 sizes × active/passive. Compared complete state, panel and semantic FX with goldens; checked text height, SafeArea and footer overlap. Six owned purchases were committed to a disposable save, then a separate process confirmed all three allocations and schema 22 survived restart. [Runtime receipt](CasualSkillSkinEvidence20261014/native-receipt.json), [test provenance](CasualSkillSkinEvidence20261014/editmode-provenance.json).
- Retained six preceding native attempts: 1–2 exposed linked/stat clipping; 3–5 reproduced/diagnosed disabled-button click-through; 6 passed behavior/84 captures before visual inspection exposed spurious emphasis wrapping/footer overlap. One player output and one fresh cache were reused.
- Actual before captures were reused from the verified integration. Native source 05973ea3 and baseline 53d6a3e1 have identical Assets, Packages and ProjectSettings trees; no fresh before-player run is claimed.

[Actual before/after](CasualSkillSkinEvidence20261014/before-after.png) · [7 viewport sizes](CasualSkillSkinEvidence20261014/viewport-contact-sheet.png) · [84-capture index](CasualSkillSkinEvidence20261014/screenshots.json)

| Viewport | Observed result | Representative |
|---|---|---|
| 440×956 | Portrait scroll, fixed detail panel, SafeArea | [Warrior KO](CasualSkillSkinEvidence20261014/after/Warrior-ko-440x956.png) |
| 956×440 | Horizontal scroll, right panel; rich English effect fits, no footer overlap | [Ranger EN](CasualSkillSkinEvidence20261014/after/Ranger-en-956x440.png) |
| 1440×810 | PC fits full viewport | [Ranger KO](CasualSkillSkinEvidence20261014/after/Ranger-ko-1440x810.png) |
| 1440×900 | 1440×810 reference frame, 45px top/bottom margins | [Mage KO passive](CasualSkillSkinEvidence20261014/after/Mage-ko-1440x900-passive.png) |
| 1680×720 | 1280×720 frame, 200px left/right margins | [Warrior EN passive](CasualSkillSkinEvidence20261014/after/Warrior-en-1680x720-passive.png) |
| 1600×900 | PC fits full viewport | [Warrior KO](CasualSkillSkinEvidence20261014/after/Warrior-ko-1600x900.png) |
| 900×1600 | Portrait SafeArea contains title/close/fixed panel; scroll retained | [Mage EN passive](CasualSkillSkinEvidence20261014/after/Mage-en-900x1600-passive.png) |

Comparison preserves node geometry, state, KO/EN text and event meaning at locked type sizes. Remaining differences include the larger live-canvas type, its invalid transparent panel background, and uGUI/browser glyph/blur/antialias rendering. The follow-up repaired triangular large-panel corners and rectangular fade overlap; this still does not establish pixel-identical browser rendering. A fresh set of 24 locked casual browser reference renders was not generated; old dark references were not treated as casual references. Actual Claude and before/after native captures were inspected.

## Final visual repairs and 2868×1320 proof

Applied the user’s five corrections: contain the gauge within the rounded padding box and its 0.97 pressed scale; draw all link casings before colored cores; use angular circle fills and circular inset shadows; give large corners contour-aligned vertices and clip tree/fade through one rounded inner viewport mask; hide the unnecessary bottom legend. Model strings, frame/scroll dimensions and node placement remain intact.

[Gauge/link/circle comparison](CasualSkillSkinEvidence20261014/visual-defects-before-after.png) · [Corner/legend comparison](CasualSkillSkinEvidence20261014/corners-legend-before-after.png) · [Final Ranger EN](CasualSkillSkinEvidence20261014/after/Ranger-en-1600x900-visual-fix.png) · [Final Warrior KO 2868×1320](CasualSkillSkinEvidence20261014/after/Warrior-ko-2868x1320.png)

Final affected EditMode **92/92**, final look-data **3/3**, UI-contract **15/15**, UI-refresh (191 sources/16 bindings) and golden verify **41** passed. No duplicate full-project suite was run. The final silent macOS development player checked three heroes × KO/EN × PC1600×900 / portrait900×1600 / landscape2868×1320: **864 panel states, 26 actual captures, 18 gauge/card pressed-scale checks, six pointer drags and six closes**. Captures wait five seconds for the finite glint (1.3s×3 plus at most720ms delay). [Source/binary/execution receipt](CasualSkillSkinEvidence20261014/target-native-receipt.json). Earlier seven-size/60-transcript results remain evidence for the initial implementation, not a claim that all84 images were regenerated for this revision.

2868×1320 is the supplied render-pixel target, not an official device specification. Its 956×440 logical frame uses ConstantPixelSize Canvas scale **1.51074**, or **3.02148px/design point**. DPR **3** and synthetic insets left/right102px, bottom54px, top0px are assumptions. Content occupies **2513.87×1266px**, **87.65%** of screen width and **95.91%** of height. Fixed956×440 preview and real fitting are separately captured; Mac auto-PC uses1152×648. Framebuffer dimensions are asserted. Close/footer targets are44 design points (132.945px, assumed44.315 device points); active cards38 and modifier sockets32 do not all meet44pt. This is Mac simulation, not iOS hardware/DPR/safe-area/touch certification. Selection, horizontal drag and close use EventSystem raycasts/handlers.

Native internal-width audit: **1242/1272** stat slots, **1324/1944** modifier rank slots and **30/36** linked chips retain original widths. Remaining expansions are **0.037–2.818**, **0.061–0.386** and **2.036–4.076** design points respectively. Only English Ground Slam/Raking Wound names expand among linked chips; **0/36 chip row/position changes**. Corrected the early diagnostic’s52pt fixed portion to the original C#54pt. Ordinary body/effects are Noto400; the design explicitly uses Noto700 for chip names/ranks. [Per-state before/after widths and roles](CasualSkillSkinEvidence20261014/geometry-native-audit.json) remain separately reviewable, without blanket approval under the raw appearance criterion. CSS geometry/font-size now has **zero** differences across49,942 strings/36 combinations.

Retained diagnostic failures separately: hidden-panel null handling, sandbox AppKit registration, and a pressed-gauge fixture selecting a rank0 Mage skill. Corrected each cause without weakening game rules or tolerances. One app output/cache was reused; older binaries were not copied.

## Handoff and retention

Local workspace: `task-6/skill-skin`. Code, related tests, bilingual report and shared rules are grouped in a local commit. Original checkout/uncommitted art transition documents are preserved and protected-path diffs are zero. [Lifecycle manifest](CasualSkillSkinEvidence20261014/artifact-lifecycle.json) records paths, measured sizes and retention; no blanket deletion or shared-cache removal occurred.

Unverified: Android/iOS devices, screen readers/touch accessibility, matched three-run before/after CPU/GC performance, WebGL, and the full EditMode suite after main integration. Performance is **unmeasured**. Wiki generation/check/publication remain at the merge stage. Review shared rules and strict conflicts before extending to another UI.
