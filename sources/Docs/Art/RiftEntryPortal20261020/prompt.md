# Task: two opaque illustrations for the HELLSCRIPT "Rift Entry" screen

Use the installed `imagegen` skill in built-in `image_gen` mode, ONE call per asset, then copy each result byte-for-byte into `out/` under the exact file name below. Do not post-process, crop, resize, recolor or remove backgrounds. These are FULL OPAQUE scenes (no transparency wanted). Also write `out/manifest.json` with, per asset: the exact prompt you sent, the source path under ~/.codex/generated_images, pixel size, sha256, and `"model": "unknown"` unless the tool reports one.

Reference images (attached with -i): `guide-tall.png` and `guide-wide.png` are LAYOUT GUIDES (not style): red bands are calm zones, the purple ellipse is where the portal and all detail must sit; do NOT draw the guide lines or text.  `previous-v1-tall.png` is the first attempt: keep its portal design and palette but recompose it per the strict rules below. `current-portal-sprite.png` is the small portal icon the game uses today (keep its violet/gold identity but make the new art far grander); `style-chest.png` shows the casual art style; `screen-layout.png` is the game screen — the LEFT tall panel (sky blue to cream, with a small purple portal) is the area your art replaces. Heading text, a treasure-chest button, the tier selector and short lines of text are drawn ON TOP of that panel by the UI, so the calm zones below matter.

## Shared art direction (style lock)
Bright casual mobile-RPG illustration. Thick dark-navy outlines (#2a3150), cel-shaded flat colours with soft gradients and glossy highlights, chunky rounded shapes, cheerful saturated palette: sky cyan (#8fd3ff) -> pale blue (#bfe7ff) -> warm cream (#fff3d6), violet (#8a4fd8 / #b06cff / #d9b6ff), gold (#ffd23f / #ffc21a), a little magenta and cyan inside the vortex. Same drawing language as the references: navy ink lines, gold accents, violet magic.

## Subject: a spectacular, impressive RIFT PORTAL ("균열 포탈")
A huge vertical-oval gateway torn open in the sky. A thick ornate ring of ancient carved violet stone and gold filigree frames it, with glowing gold runes along the ring and jagged torn-space edges where reality cracks. Inside: a deep swirling vortex of violet, magenta and cyan spiral arms around a blazing white-gold core, crackling energy tendrils reaching out. Around it: floating stone shards and glowing violet crystals orbiting, large golden rune circles hanging in the air, soft light rays, drifting sparkles/embers, wispy white clouds. At the bottom a few floating ancient stone steps lead toward the portal over soft cream mist, with faint glowing cracks. It should look rich, valuable and a little mysterious, yet still bright and friendly.

## Asset 1 — `rift-portal-scene-tall.png` (request 1024x1536, portrait 2:3)  STRICT COMPOSITION
The game draws dark-navy and gold text and buttons over this picture, so the layout rules are hard requirements:
- The portal and ALL detail (gold ring, vortex, orbiting rune circles, crystals, rock shards, floating islands, steps) must lie inside the vertical band 28%–62% of the image height. Portal centre at 45% height, centred horizontally; the portal ring is a tall oval about 34% of the image width and 28% of its height.
- Everything above 28% height: only soft pale sky gradient and a few thin wispy clouds, NO objects, no crystals, no shards, no islands, no rays crossing it (heading text sits there).
- Everything below 62% height: only soft warm-cream clouds/mist, NO stone steps, NO shards, NO islands, NO glow cracks (UI text sits there). A thin stair of three small stones may start exactly at 60% height, nothing lower.
- Floating islands, if any, only tiny and pale, at the far left/right edges inside the 38%–58% band.
- Light, bright overall; no dark vignette.

## Asset 2 — `rift-portal-scene-wide.png` (request 1536x1024, landscape 3:2)
The same portal design re-framed for a wide strip: portal centred, about 70% of the image height, centre at roughly 48% from the top. Calm light sky on the left third and at the far right edge, calm soft mist in the bottom 22%. Slightly lower contrast than asset 1 is fine (it will be shown semi-transparent).

## Hard rules
No text, letters, numbers, UI, buttons, borders, frames, characters, creatures, logos, watermarks or dark vignette. No gore. One portal only. Do not draw a sky-blue rounded rectangle frame around the picture.
