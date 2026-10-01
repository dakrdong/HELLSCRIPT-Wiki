# Native rune board UI port

Updated 2026-10-01 · [한국어](Rune_Board_Native.md) · [Approved prototype](../../Prototypes/RuneBoard/HELLSCRIPT-RuneBoard.html)

## Scope and ownership

This is a UI replacement for the existing rune content. Six weapon boards, 259 cells per board, 34 shapes, G0–G6 acquisition grades, placement/connectivity rules, combat bonuses, fusion/drop economy and save schema retain their existing owners. The 160-piece clarity scene is loaded only into explicitly isolated validation saves.

The branch `codex/rune-board-native` starts from main **7a3f9d3d** and merges the prototype branch **18807bea** at **4afc706e**. The primary checkout, its running Editor and the reference prototype remain unchanged. The newly specified prototype supersedes the historical portrait dropdown/hidden-inspector layout: it uses colour tabs, size chips, a fixed inspector and two storage rows.

| Responsibility | Owner |
| --- | --- |
| Account-wide draft, 50-step undo, stack rotations, five global presets | `RuneBoardSession` |
| Placement, ownership, progression and effects | Existing `RuneBoardEditor`, `RunePlacementValidator`, `RuneMasteryProgress`, `RuneEffectEvaluator` |
| Atomic persistence and revision guard | `GameStore.CommitRuneBoardState` |
| Safe area, gameplay/input lease, navigation and close lifecycle | `ContentWindowView`, `ContentWindowHost`, `RuneBoardWindow` |
| Board, thumbnails, inspector, drag layer, legend and practice artwork | `RuneGemView`, `RuneGemMesh`, `RuneBoardArt` |
| Native input | `RuneBoardPointer`, `RuneBoardStoragePointer`, `RunePracticePointer` |
| Isolated practice | Five temporary `RunePracticeModel` instances |
| Localisation | `RuneBoardText`, `Loc`, existing English glossary |
| Access gates and legacy services | `GameUI.ShowRunes`, `OpenRuneService` |

Storage groups by colour, shape and acquisition grade while retaining physical instance IDs. Placement on any weapon removes the piece from shared storage; recall makes it available again. Presets record all weapon layouts. Preset edits are drafts until Save Changes succeeds.

## Layout and rendering

Portrait orders the title, weapon tabs, mastery/effects, board, fixed 7U inspector, storage and fixed actions. Landscape and PC divide the board and side panels at 1.55:1. Changing selection does not resize the inspector. Storage displays counts, multi-size filters, grade badges, rotated shapes and stack quantities.

A shared renderer draws faceted slabs, depth, continuous contours, bright active glyphs with contrasting outlines, engraved inactive glyphs, open/locked/sealed cells, unlock hints and valid/invalid previews. Board pan/zoom changes the parent transform rather than rebuilding meshes on every pointer move.

Mouse and touch use the prototype's 0.1-second board pickup delay; quick motion pans. Storage touch holds for 0.3 seconds before pickup, while quick motion scrolls. Dragged pieces retain the grabbed cell and float above the pointer, including over storage. R rotates; Delete/Backspace recalls; Ctrl/Cmd+Z undoes; Ctrl/Cmd+S saves; Esc cancels/backtracks. Invalid drops and disconnecting recalls preserve the prior layout.

## Assets and decisions

The [import manifest](../Art/RuneBoard/rune-board-unity-import-manifest.json) records source hashes, UV bounds and derivatives. Twenty-seven PNGs are copied unchanged; seven dim faces use the prototype's exact sRGB transform. No new images were generated. Existing provenance remains `unknown` with `productionApproved=false`. The demo-only lab icon is excluded.

Tiles use a 512 limit, mipmaps and Trilinear filtering; icons use 128/Bilinear. Platform rules use Android ASTC 4×4, Standalone BC7 and WebGL ETC2. The common importer version stays unchanged to avoid unrelated mass reimports; the dedicated new-path importer applies the new settings. Glyphs reuse the existing 96px atlas.

| Decision | Final choice |
| --- | --- |
| D1 | Colour/shape/grade stacks, separate G badges |
| D2 | Existing game glossary takes precedence |
| D3 | 0.1-second/5px board gesture distinction for mouse and touch |
| D4 | Preserve candidate art provenance |
| D5 | Replace demo controls with real information/fusion/drop services |
| D6 | One landscape layout for PC |
| D7 | Replace this board renderer; preserve warehouse/rune-master icons |
| D8 | Shared `ContentWindowView` with Draft source; retain runes page ID |
| D9 | Keep editing / discard and close / save and close |
| D10 | Prototype refusal priority/text mapped from existing C# validation |

The browser demo tools, device frames, localStorage, JSON import/export and language toggles are excluded. Game settings own language. Native fonts, rasterisation and mesh shadows differ from browser SVG/CSS; CSS breathing and placement animations are not reproduced exactly. Anchored popovers use the shared modal frame. The game retains 34 shapes, acquisition grades, existing numeric units and two-decimal formatting. Actual accounts and combat do not use demo economy values. [Translation differences](RuneBoardNativeEvidence/translation-differences.json) lists 29 keys affected by retaining the existing English glossary.

## Validation and delivery

The [combined evidence report](RuneBoardNativeEvidence/validation.json) records scope and reasons for retries. The initial focused run passed **8/8**, including **468 placement cases** exported independently from the unchanged JavaScript engine, geometry, draft ownership and global presets. On the final repaired code, the relevant rune, localisation and resource checks passed **108/108**. One initial failure was a missing unchanged catalog JSON in the isolated validation clone; copying the fixture and rerunning only that case resolved it. Full regression results are recorded separately below.

The [full Edit Mode run](RuneBoardNativeEvidence/editmode-full.json) executed once: **4,958 total, 4,911 passed, 47 failed, zero skipped**. All 47 failure names exactly match the previous integration baseline; there are no new failures. Six final presentation repairs, made after freezing that run's source, were validated in an isolated clone by the 108 focused checks and affected native sections. [Source equivalence](RuneBoardNativeEvidence/source-equivalence.json) confirms **909 identical source, package and locale files** between the tested/built clone and the pre-main-refresh delivery checkout, so the full suite was not repeated. The macOS development build completed with zero errors. The shared-UI ownership check and 11 checker tests passed; the unchanged checker-test result was reused.

Before submission, latest main **c4e03b09** was integrated into the feature branch. Only the four shared-UI document/history files conflicted; both texts and existing history were preserved. Rune implementation files were unchanged. One English title missing from the incoming rift graph was added, followed by a retry of only the failed localisation check. Integration checks passed **126/126**, the macOS build had zero errors, and the save/preset/service-return/gameplay pause-resume section passed. Separate [integration source hashes](RuneBoardNativeEvidence/integration-source-hashes.json) and [native results](RuneBoardNativeEvidence/integration-native/result.json) record this state. The full suite and unchanged layout matrix were not repeated.

Native pixel checks use the same clarity placements, first cell of each state and central crop as the browser test. The report explicitly allows one 8-bit code value (1/255) of quantisation tolerance and retains raw values. Empty-cell luminance was 0.120263 against 0.12; inactive skill-face saturation was 0.399638 against 0.40. Source/derived image transforms and renderer tint were not changed to force the numbers across the thresholds.

Native acceptance covers 440×956, 956×440, 1440×810, 1440×900 and 1890×810 in Korean and English, with empty/block/cell selection states. It checks fixed inspector height, at least two portrait storage rows, top/bottom safe insets, drag layering, rotated footprints, touch scroll/hold, pinch zoom, seven dialogs and all five practice steps. It also verifies five presets after disk reload, cross-weapon ownership transfer, slot unlock undo, unsaved close, selected-weapon preservation across fusion/drop service visits and gameplay pause/resume. Some dialog actions invoke uGUI callbacks; piece movement and practice use InputSystem pointer events.

Passing sections of the initial native batch are reused. Later repairs address touch test-event spacing, service return weapon selection, the neutral start stone, minimap English wrapping and practice field height; only affected sections are repeated. Earlier failure records under `native-initial` and `touch` remain as historical context, separate from the final results.

| Comparison | Native game | Prototype reference |
| --- | --- | --- |
| Korean portrait | [440×956](RuneBoardNativeEvidence/native-final/02-ko-440x956-block.png) | [Portrait](../../Prototypes/RuneBoard/evidence/layout-portrait-ko.jpg) |
| English landscape | [956×440](RuneBoardNativeEvidence/native-final/32-en-956x440-block.png) | [Landscape](../../Prototypes/RuneBoard/evidence/layout-landscape-en.jpg) |
| Practice step five | [Korean landscape](RuneBoardNativeEvidence/practice-landscape/05-ko-practice-5-956.png) | [Landscape](../../Prototypes/RuneBoard/evidence/tutorial-page5-landscape-ko.jpg) |
| Colour/state rendering | [49 swatches](RuneBoardNativeEvidence/gallery/render-gallery.png) | [Pixel metrics](RuneBoardNativeEvidence/gallery/pixel-legibility.json) |

Physical Android/iOS devices, screen readers and colour-vision tools are outside the recorded verification. macOS-generated mouse/touch events are not physical mobile evidence. Only the default text size is tested.

A feature branch/PR is separate from main integration and public release. Wiki generation/validation occurs on the feature branch; public publication and game deployment follow a main merge.
