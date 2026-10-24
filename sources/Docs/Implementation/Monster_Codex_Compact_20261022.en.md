# Monster codex: tile corners and a compact portrait detail (2026-10-22)

[한국어](Monster_Codex_Compact_20261022.md) · Mockup: canvas "몬스터 도감 · 캐주얼" (Detail board `c.*` values; two "portrait · short screen 704" boards added)

## What the user reported and why

1. **Odd corners on list tiles.** The mockup clips the art to the rounded inner edge (CSS `overflow:hidden`); the game put a square `RawImage` over the rounded rim, so the art's corners covered the border. The mockup was already right and is unchanged.
2. **Portrait detail too big, scrolling.** Identity card 260 + two stat rows of 75.6 + defences + a 200 minimum for the skills panel overflowed a 704-high screen (a real portrait ratio of 0.625). The page content was also made 56 taller than its viewport, so it scrolled even when it fit.
3. **Art cut off at the bottom.** A square image was laid over the card width with `cover` and its lower 62% was washed out with a cream gradient.

## Changes

| Part | Change |
|---|---|
| Discovered list cards | The art sits inside `UiSkin.Clip(frame,12,2)` (inner radius 10). The ID and boss chips are siblings of the clip, so they are not cut. Locked cards get no mask |
| Identity card (both) | The art is shown whole in a rounded (radius 14) square tile, no crop or wash. The name stays on one line and shrinks to fit the card |
| Portrait identity | Height 128: a 112 tile on the left; grade and kill chips, then ID and name, then field and role chips on the right, spread evenly |
| Landscape / PC identity | 180 tile with the grade and kill chips on it; ID, name and field/role below it. Field and role wrap to two rows when they do not fit |
| Portrait metrics | Body padding 8/16/6, gap 6. Stat cards 48 (the number beside the label, the note on the whole line under them), defence cells 36 (a row grows when a long English note wraps), skills header 6/12, rows 7/12, icon 40, tier bar 105 (grade buttons 48×30) |
| Page scrolling | The skills panel takes all remaining height and only its list scrolls. Only on a short screen where the remainder falls below 120 does the page scroll. When it fits, `vertical=false` and the offset is 0 |
| Landscape / PC metrics | Unchanged |

Mockup measurement (Chrome render, 440×704, Korean): header 56 · identity 128 · stats 102 · defences 76 (boss 112) · skills 205 (boss 169) · tier bar 105, with no page scroll.

## Verification scope and limits

- EditMode `MonsterCodexWindowTests`, 34 passing: list tile clip; portrait 440×956, 704, 640 and 540 in Korean and English for normal, elite and boss (the page scrolls only when the skills panel cannot keep its 120 minimum, whole figure, art inside its clip, text widths); the widest English stat note; the landscape identity card. The real board size varies with the screen, so the tests pin a 440-wide board through `MonsterCodexWindow.fitOverride` (private static, null in the game). The full Edit Mode suite was not run.
- A batch-mode render of the real window at 440×704 (Korean, English) and 956×440 was captured and inspected (list tile corners, normal/elite/boss detail, a long English name). The throw-away capture runner is not committed and the PNGs are not kept.
- One suggestion from the planning pass, pinning the portrait `Fit` scale to a 440 width, was not adopted: it is outside the request.
- Each card now adds one stencil level (136 cards). Rendering cost was **not measured**; device and performance checks are separate.
- The existing overlap of the slider's "Best N" label with the end value (accounts with a high best clear) is out of scope here.
