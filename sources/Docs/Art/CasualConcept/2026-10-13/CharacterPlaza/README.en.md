# Character create/select screen, town scene: art originals

Written: 2026-10-14 · [한국어](README.md)

The screen mockup and its rules are in [Docs/Design/CharacterPlaza](../../../../Design/CharacterPlaza/README.en.md). This folder holds the original pictures made by Codex, their prompts and manifests, and the tool that lifts the heroes out of the scene. The concept record is the section "Character create/select screen: town scene" in [Casual_Art_Concept_20261013.en.md](../Casual_Art_Concept_20261013.en.md).

## Files

| File | Content |
|---|---|
| `scene_landscape.png` · `scene_portrait.png` | The town scene with the six heroes in it (1536×1024, 1024×1536), each at their own place: tavern terrace (male and female warrior), fruit stall (male and female ranger), magic shop (male and female mage) |
| `clean_landscape.png` · `clean_portrait.png` | The same scene without the heroes (it hides the painted hero during the cheer and battle-ready poses) |
| `cheer_<hero>.png` ×6 | Cheer poses (no weapon, transparent background, 1024×1536) |
| `fight_<hero>.png` ×6 | Battle-ready poses with the equipped weapon (warrior sword, ranger bow, mage crystal staff; transparent background, 1024×1536). The male ranger's "aiming the bow" request was blocked by the Codex safety filter, so he holds the bow with a thumbs-up |
| `crests.png` | The three class crests (warrior, ranger, mage) on one picture; the mockup cuts it into three |
| `PROMPT_*.md` · `manifest_*.json` | The request prompts and records of source path, SHA-256 and size. The tool does not report a model name, so model is `unknown` |
| `tools/` | `lift.swift` (macOS Vision subject lifting), `prep_scene.py` (layer maker), `spec_landscape.json` · `spec_portrait.json` (crop ranges and seed points per hero) |

`PROMPT_cheer_*.md` also describes a prop pose requested in the same run for the first attempt (heroes with props on an empty plaza); those pictures were dropped. `PROMPT_fight_rangers.md` is the run where the male ranger request was blocked and only the female ranger came out; the male ranger was requested again with `PROMPT_fight_archer_m.md`.

## Rebuilding the layers

```bash
swiftc -O tools/lift.swift -o tools/lift
python3 tools/prep_scene.py tools/spec_landscape.json --out /tmp/plaza --format png
python3 tools/prep_scene.py tools/spec_portrait.json  --out /tmp/plaza --format png
```

For each picture this writes `<pic>_bg` (the scene), `<pic>_cut_<hero>` (the hero lifted from the scene), `<pic>_patch_<hero>` (the clean plate piece at the hero's place) and `scn_<pic>.json` (coordinates). It was checked to produce the same boxes and anchors as the already published canvas (all 12 heroes match).

- Screen data: in `scn_*.json`, `cut` is the lifted hero's place in the scene, `head` and `feet` are the anchors for the name tag, speech bubble and pose pictures, `cheer` is the box a pose picture stands in (feet x, feet y, height), `patch` is where the covering piece goes. The portrait camera framing is `focus` (per class: x, y, zoom, screen x).
- The pose pictures (`cheer_*`, `fight_*`) are originals with transparent margins. Trim them to the alpha bounds, centre the feet on `feet` and set the height to `cheer[2]`.

## Processing and rules

- The hero layers are cut from the scene with macOS Vision subject lifting, so they are **not native alpha**. That differs from the repository art rule (no chroma key or background removal, original alpha). When importing into the game, register them as replaceable development art (`productionApproved: false`) and write this processing into the provenance. Replace them with 3D heroes and animation or native-alpha pictures before release.
- The scenes and pose pictures are Codex's original pixels (no post-processing). The model name is unknown (`unknown`); the C2PA chunk records the generation tool.
- The female warrior's outfit (bare midriff) needs a 12+ rating check. The male warrior's tankard stays (user decision).
