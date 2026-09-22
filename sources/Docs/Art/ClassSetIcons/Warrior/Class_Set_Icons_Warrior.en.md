# Warrior New Set Icon Production Record

Updated: 2026-09-22

[한국어](Class_Set_Icons_Warrior.md) · [Combined production record](../Class_Set_Icons.en.md)

This batch created 35 equipment icons and 8 emblems for eight new Warrior sets, 43 images total. Each item used its own built-in generation request, with no existing-art reuse. Because the tool did not return a model name, every item remains `generated_candidate` with `model.actual: unknown`.

## Set visual language

| Set | Equipment | Shared form and material |
| --- | ---: | --- |
| Executioner Discipline (`REF_SW01`) | 3 | Blackened iron, muted bronze, and oxblood leather, with clipped corners and a broken crown. |
| Ironwall Oath (`REF_SW02`) | 3 | Blue-gray steel, broad bulwark shapes, and interlocking shields. |
| Vanguard March (`REF_SW03`) | 4 | Bronze, russet leather, red pennants, and forward-swept spear forms. |
| Trifold Armament (`REF_SW04`) | 4 | Pale iron with three fire, cold, and lightning stones arranged in a triangle. |
| Crimson Scars (`REF_SW05`) | 5 | Crimson lacquer, split teardrops, and deep scar grooves. |
| Arsenal Manual (`REF_SW06`) | 5 | Alternating steel and brass, with three interlocked blades representing weapon changes. |
| Ancestral Vanguard (`REF_SW07`) | 5 | Ivory bone, black iron, teal ornament, fur, and paired ancestor profiles. |
| Colossal Fury (`REF_SW08`) | 6 | Large basalt blocks, brass clamps, amber pressure cracks, and a clenched fist. |

Each set also has one emblem. The 20 Warrior bonus thresholds reuse these eight emblems. [Source data](../brief/sets.json) supplies IDs, bilingual names, and effects; the [manifest](manifest.json) records actual prompts and file paths, and the [design record](sets.design.json) preserves form guidance.

## Originals and QA

All 43 tool-returned 1254×1254 RGBA originals are preserved byte-for-byte in `originals/`. The 1024×1024 exports use proportional resizing and transparent padding while retaining native alpha. No background removal or alpha reconstruction was performed.

All 43 items were inspected in the [64px light/dark contact sheet](qa-contact.png). Equipment slots and emblems remain distinguishable, without a conspicuous rectangular backdrop or matte fringe. File checks cover the central 76% bounds, fully transparent pixels, original hashes, and export reproduction. Per-item visual findings are in `qa.visual`. The seven Colossal Fury items also have a [dedicated QA record](REF_SW08-production.json) and [supporting sheet](REF_SW08-qa-contact.png).

The first `REF_SW06_P05` necklace had an incomplete cord and was corrected with the built-in image tool. The rejected original and prompt remain under `rejected/`; the [edit record](REF_SW06_P05.edit.json) preserves the referenced source and actual request. The final candidate was checked for a closed loop and transparent interior.

All 43 candidates are ready with no missing assigned image. Runtime wiring, Unity import/gameplay, and physical-device verification were not performed. Those limits and the absent model identity remain explicit; production approval is not asserted.
