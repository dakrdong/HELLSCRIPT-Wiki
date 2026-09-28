# Skill tree screen art record

Created: 2026-09-27 · [한국어](Skill_Tree_UI_Art.md)

This records the five ornament images used by the Hunt Edict skill tree redesign. The user asked for any images needed to decorate the skill tree to be made with GPT. Layout and verification are in [Hunt edict skill tree integration](../../Implementation/Hunt_Edict_Skill_Tree.en.md).

## Production

The images were made through `codex exec` (codex-cli 0.155.0) using the installed imagegen skill in built-in `image_gen` mode, one request per image. Two existing skill icons, the existing round frame and the pre-redesign skill screen were attached as style references only. No other game's art, logos or screens were used.

The outputs were copied byte for byte: no resizing, cropping, color correction or background removal, and the transparent images keep the generator's native alpha. All five PNGs carry a C2PA chunk naming the software agent `ChatGPT` / `gpt-image`. The exact model identifier cannot be verified, so it stays `unknown`; the signature was not validated.

Prompts, source paths, hashes, attempts and review results are in [manifest.json](manifest.json).

| File | Size | Use |
| --- | --- | --- |
| `tree-backdrop.png` | 1024×1536, opaque | Fixed backdrop behind the scrolling tree: a dark slab with an engraved tree |
| `node-frame-active.png` | 1254×1254, transparent | Round frame for active skills in the tree, skill bar and inspector header |
| `node-frame-passive.png` | 1254×1254, transparent | Square frame for passive skills in the tree and inspector header |
| `node-frame-ultimate.png` | 1254×1254, transparent | Four-pointed crest for ultimates in the tree, ultimate socket and inspector header |
| `skill-point-gem.png` | 1254×1254, transparent | Remaining skill point badge in the tree header |

## Review

Codex checked strictly that the inside of each frame is fully transparent and regenerated each frame twice. The kept versions still hold pixels with alpha 1/255 in the opening, so Codex recorded them as failing. That value is invisible and the skill icon lies underneath, so the frames were accepted. Their centers and four corners are fully transparent.

The transparent openings were measured directly: the widest radius for round frames and the distance to the middle of an edge for the square frame, as a fraction of half the frame width. Active 0.657, passive 0.684, ultimate 0.579. The UI constants `ActiveHole`, `PassiveHole` and `UltimateHole` use these values so each icon fills the opening and the frame's inner lip overlaps its edge. This size and centering replace the old screen, where icons sat against the top of their boxes and looked misaligned. After replacing a frame, recheck the values:

```sh
python3 Docs/Art/SkillTreeUi/measure.py
```

## Import settings and size

Default import caps frames and the gem at 512 px and the backdrop at 1024 px, compressed. `AndroidTextureBudget` has matching ASTC 6×6 rows. On a phone the frames display below about 250 px and the backdrop covers a whole panel.

## Status

In use by the skill tree screen. The generator model cannot be verified, so the art is not marked release-approved.
