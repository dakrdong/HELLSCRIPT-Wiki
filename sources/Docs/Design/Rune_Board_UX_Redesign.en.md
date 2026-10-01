# Rune board mockup rebuild — shared UI and block design

Updated: 2026-10-01 · [한국어](Rune_Board_UX_Redesign.md) · [Open the mockup](../../Prototypes/RuneBoard/HELLSCRIPT-RuneBoard.html) · [Mockup README](../../Prototypes/RuneBoard/README.md)

## Request and scope

The approved rune board v13 HTML mockup ([source package](RuneV13/reference-v13.zip)) was rebuilt on the game's shared UI, and the blocks were given a new, easy-to-read design for every state. No picture was drawn by hand: the art was requested from Codex. No game code changed; only the screen composition and interaction of the HTML mockup did.

**Rules and data are exactly v13.** The rules engine (`Prototypes/RuneBoard/vendor/engine.js`) and the tutorial model are byte-identical to the scripts of the source package, and the board and ability data are read from the game's `Resources/Runes/V13/catalog.json`, which a test confirms equals the data in that package.

## What changed

| Area | v13 mockup | This mockup | Why |
| --- | --- | --- | --- |
| Window | Mockup header + game header, vertical weapon list on the left | Title and close → weapon tabs → body → fixed bottom actions | [Shared window rule](../Implementation/Shared_UI_Contract.md); same order in landscape and portrait |
| Colour, type, buttons | Mockup-specific colours and shapes | Obsidian-and-gilt tokens read from `UiTheme.cs` at build time; shared button, tab and header rules | No colours copied into the screen |
| Blocks | Flat single-colour hex lumps | Gem slab: a face image per cell, thickness, outline, one joined silhouette | Read the shape and cell count at a glance |
| Ability on / off | Only a glyph brightness difference | On: glowing face + white glyph (dark outline on bright faces) / Off: dimmed block colour + a coloured rim on every cell + engraved glyph | Never colour alone, and an unlit cell must not look empty |
| Slots | Same-strength glyph on every slot | Open slot = bronze-rimmed socket + required-colour ring; locked = slab; sealed region = darker slab | Less noise so blocks stand out |
| Storage | 160 individual runes in 8 columns | Same colour and shape share one tile with ×count (30 tiles), colour tabs (emblem + count) + size chips | Easier to scan |
| Selection | Slot and block details in one long text | Block: name, cell count, on/off count and Rotate/Recall; slot: one “ability +value” line and the place (the required colour is the emblem before it), only an abnormal state is written out; nothing selected: legend. In portrait a fixed-height one-row card | Only the controls needed now, and a bigger board |
| Where it fits | Found out by dragging | Select a block and green dots mark where it fits; a gold ring marks spots where every cell switches on | Answers the most common question |
| Preview | Only while dragging | Mouse hover and dragging both show how many cells will switch on, or the refusal reason, under the board | Tells you before you fail |
| Effects | Small buttons at the bottom | Per-colour active cell counts with the change from the last save (+1/−1) | See what a placement changes at once |
| State guide | Explanatory dialog | Eleven states drawn by the real block and slot renderer, opened by the **Legend** button on the board | Text and screen cannot drift apart |
| Tutorial | Grey and ivory practice blocks | The same red and blue blocks as the board, same on/off drawing | What you practise is what you see |
| Language | Korean only | Korean and English, switchable in the preview and stored on the device | Project language rule |

## Block and slot states

Every state differs from the others by at least two of face brightness, glyph tone and outline in addition to colour.

| State | Look | Non-colour cue |
| --- | --- | --- |
| In storage | Gem slab, storage tile, ×count | Cell count in the tile, colour emblem (sword, drop, shield, claws, star) |
| Selected | Gold outline and glow | Selection card, green anchor dot |
| Placement preview | Dashed outline, translucent, per-cell on/off | “Can place · N/M cells on” text |
| Cannot place | Red dashes, dark face | Reason text and a red tag in the selection card |
| Ability on | Bright face, glowing white glyph; faces that are already bright (yellow, gold, elite, start stone) give the glyph a dark outline | Count rises in the effect chips |
| Ability off · passage | Block colour dimmed (never covered in black), coloured rim on every cell, engraved glyph | “Off: N” tag and explanation |
| Open empty slot | Bronze socket, required-colour ring, glyph | Glyph colour |
| Where it fits | Green dot (+ gold ring) | Only dotted slots are allowed |
| Can open now | Gold dashed ring | Only with slot points |
| Locked slot | Frosted slab, faint glyph | No dashed ring |
| Sealed region | Dark slab, almost hidden glyph | Region padlock |

All of it is drawn by `Prototypes/RuneBoard/blocks.js`; the board, storage, selection card, the block in your hand, the tutorial and the state guide share these functions.

## Fixes after hands-on use (2026-10-01)

1. **The lit glyph on yellow blocks was hard to see.** A white glyph with a bright glow vanished on faces that are already bright (yellow, gold, elite opal, the start stone). On those faces the glow is gone and the glyph gets a dark outline, so it reads on any face. Dark faces (red, blue, purple) keep the white glyph with a glow.
2. **Unlit cells looked empty.** A 50% black cover and black hatch removed the block colour. Now nothing is covered in black: the face only loses saturation and brightness (55% and 58%, with the darkest parts lifted slightly so they do not crush to black), every cell gets a rim in the block colour, and the whole silhouette gets a block-colour band and a heavier dark outline (1.5 → 2). It can no longer be mistaken for an empty socket (black floor + bronze rim).
3. **The slot card took too much of the board.** The value sat under the name, and requirement and origin tags plus an “ability on” sentence sat below that. The value now shares the name line (“Attack Power +0.8”); the requirement tag (“Attack block needed”), the origin tag (“Existing rune effect”) and the normal-state sentences (“Ability on”, “Open and empty”) are gone. The required colour stays as a small emblem before the place line, and only off, locked, sealed and broken-link states are written, at the end of the place line (for example “Off · block Magic ≠ slot Critical”). In portrait the card is a fixed-height single row (art | name and place | buttons) and the “03 Slot info” heading is hidden. Card and board heights (px) at the same screen sizes:

| Screen | Card before → after | Board before → after |
| --- | --- | --- |
| 416×881 | 152 → 92 | 232 → 291 (+25%) |
| 440×956 | 161 → 97 | 270 → 332 (+23%) |
| 360×780 | 132 → 79 | 217 → 268 (+24%) |

4. **Hatching hid the glyph.** The hatch drawn over unlit cells crossed the engraved glyph and made it harder to read. The hatch is gone and the face is simply darker. An unlit cell is now a dark face, a block-colour rim on every cell and an engraved glyph with a light lip; nothing is drawn over the glyph. The rim keeps it reading as a filled cell.

All four fixes are locked in by regression checks. They measure screen pixels (an unlit cell must not be mostly black like an empty socket: dark-pixel share at most 30%, saturation at least 0.4; the glyph on a yellow or gold face must have both white strokes and a dark outline), check that no hatch is drawn over an unlit cell, and check that the slot card has no tag row or status sentence and keeps one height whatever is selected. Each check fails on the build from before its fix.

## Input rules (unchanged)

A placed block pans the board if dragged within 0.1 s and moves itself if held for 0.1 s first. In storage, touch needs a 0.3 s hold to drag. A 5 px wobble is not a drag. Rotation has six 60° steps, undo keeps 50 steps, and R, Delete, Ctrl+Z, Ctrl+S and Esc are supported. New: mouse hover preview, anchor dots, storage stacks, and automatic switch to edit mode when a storage block is picked in view mode.

## Art (Codex)

Each asset was generated with one built-in `image_gen` call through `codex exec` (`gpt-6-astra` requested; the tool does not report a model, so it is recorded as `unknown`). Everything is candidate art with `productionApproved=false`. The original PNGs are kept byte for byte in [`Docs/Art/RuneBoard/2026-10-01/source`](../Art/RuneBoard/2026-10-01/art-manifest.json); the mockup embeds only the small copies made by `tools/make-art.py`.

| Set | Contents | Chosen |
| --- | --- | --- |
| 7 block faces | Attack ruby, magic sapphire, support citrine, critical amethyst, skill-bonus gold, elite opal, start stone | Style A (bold graphic) for all seven. Style B (painterly) lost facet contrast at small sizes and was not used |
| Slot beds | Open socket, locked slab (frost), sealed slab | Socket A, slab B (locked), slab A (sealed region) |
| 5 colour emblems | Sword, drop, shield, claws, star | Attack A, magic A, support A, critical B (three claws), skill A |
| 13 action icons | Rotate, recall, undo, revert, save, presets, view, fit, lock, unlock, help, demo tools, overview | All used, together with the game's existing pencil, book and close icons |

In post-processing the block faces and beds are cropped to their opaque bounds, fitted to the exact grid hexagon (2R wide, √3R high) and masked with the same hexagon so neighbouring cells meet along straight edges. No background removal or recolouring is applied. Emblems and icons stay single white shapes tinted on screen. Rejected variants are recorded by hash and reason only.

## Relation to the shared UI contract

Window structure, colours, type hierarchy and the button, tab and header rules follow the shared contract. The hex board is the contract's recognised specialised layout (“rune hex board”), so only the board area uses a dedicated renderer. There is no text-size scaling and checks use the default size. The adapter reason to record when this reaches Unity is: hex occupancy and connection rules need a dedicated board renderer.

## Verification

- `node --test Prototypes/RuneBoard/engine.test.cjs` — 7 pass. Engine and tutorial model are the original bytes, the game's catalog equals the package data, and placing, touching, gap and unlocking rules match the documents.
- `node --test Prototypes/RuneBoard/locale.test.cjs` — 5 pass. Every key the code uses, every key built from data and every Korean refusal reason of the engine has English.
- `node Prototypes/RuneBoard/browser.test.cjs` — **305 pass**, 0 page errors. Headless Chrome with real mouse, key and touch input: placing, preview, refusal reasons, rotate, recall, undo, save, revert, close confirmation, presets, view mode, pan, zoom, hold-to-lift, region and slot opening, seven dialogs, the five tutorial pages solved by real drags, dragging inside the scaled preview, and touch (swipe-scrolling storage, a 0.3 s hold to drag, tap to place, two-finger zoom). Portrait 440×956, narrow 360×780, landscape 956×440 and PC 16:9, 16:10, 21:9 are swept in Korean and English for clipped, overlapping or off-screen parts. 49 evidence screenshots and the result are in [`Prototypes/RuneBoard/evidence`](../../Prototypes/RuneBoard/evidence/browser-validation.json).
- `python3 tools/check_ui_contract.py` and `python3 tools/test_ui_contract.py` pass.

## Not verified

- How touch feels, and performance, on a physical phone. Touch and pinch were checked only with synthetic touch events in macOS headless Chrome.
- Safari and Firefox. The in-app browser refuses local HTML, so it was not used.
- Screen readers and colour-blindness simulation. Non-colour cues (brightness, outline, glyph) are in place but no person checked them.
- Unity. This is a mockup; whether storage stacks fit the game's rune inventory (34 shapes, G0–G6) must be decided when porting. The mockup keeps the v13 package's six shapes and 160 runes.
- English names. The game's `en.txt` maps the rune colour “공격” to “Attacking”; this mockup writes only the colour name as “Attack”. Decide on one when porting.
