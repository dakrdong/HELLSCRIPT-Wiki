# Legendary equipment image production record

Updated on: 2026-09-22 (Korea Standard Time)

Subsequent integration: on 2026-09-23, all 141 legendary equipment images were wired to shared equipment presentation. See [equipment artwork integration](../../Implementation/Equipment_Art_Integration.en.md) for current validation; the original production evidence below is preserved.

Created 141 actual native PNG masters out of 141 requested legendary items. The [complete index](manifest.json) and item manifests track all 123 existing expansion items and 18 new skill-linked items. Generated counts exclude placeholders, duplicates and prompt-only work.

## Handoff format and provenance

- Every distinct item uses its own built-in `image_gen` request. Native outputs are preserved unchanged.
- The originating task revised the accepted format to square native RGBA PNG at 1024px or 1254px. Each manifest records the actual returned size. The central 76% is composition guidance; clipping, missing equipment structure and opaque backgrounds fail review.
- The callable surface returns `image_url` and `output_hint`, with no verifiable exact model identity. Embedded PNG provenance identifies `ChatGPT / gpt-image`, which does not establish `gpt-image-2`. The production instructions default to `gpt-image-2`; the actual model remains `unknown`. Even reviewed art remains a candidate.
- EquipmentAtlas informs worn metal/leather and directional contrast. GlobalHUD informs small-size readability without copying its panels or frames.
- No alpha regeneration, background removal, chroma key, opaque stand-in or local resizing was applied.

## Files

- `manifest.json` indexes all IDs. `manifests/<ID>.json` records bilingual names and effects, slot, linked skills, full prompt, paths, generation evidence, QA and outstanding work.
- `../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/<ID>.png` stores the native game-project master. New `.meta` files have unique GUIDs; existing GUIDs remain unchanged.
- `originals/<ID>.png` is a byte-identical preservation copy. Its SHA-256 must match the Art master and returned native source.
- [Individual concepts](concepts.tsv), [item source](source/legendaries.json), [linked skill source](source/skills.json), [revised handoff format](handoff-revision.json) and [source provenance](source-provenance.json) preserve the design inputs. Each item manifest includes the full prompt. The source workspace was not written to.
- `qa/` contains actual 64px dark/light comparisons and native-resolution edge crops. These are review composites and never replace the native masters.

## Validation scope

Native alpha was verified for 141 items and accepted native dimensions for 141. Visual reviews are recorded for 141 items. The [validation report](validation.json) separates catalog/provenance/file/GUID correctness from complete production coverage.

To preserve existing wiki links, historical validation artifacts were read from the two workspaces authorized by the originating task and restored into this workspace's ignored artifact paths. The [restoration record](historical-wiki-evidence.json) records sources and SHA-256 values. Historical artifacts are not counted as game tests executed by this task.

This task changes only images and production records. Combat code, UI layout, icon bindings, scenes and prefabs were not modified. Live Unity import, Edit Mode, macOS runtime and physical-device validation were not run and belong to UI integration. This task does not merge main or publish the public wiki.

## Item progress

| ID | English name | Slot | Native output | Visual QA |
| --- | --- | --- | --- | --- |
| [LW01](manifests/LW01.json) | [Fang of the Maelstrom](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW01.png) | weapon | Generated candidate | Passed |
| [LW02](manifests/LW02.json) | [Stride of the Falling Star](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW02.png) | boots | Generated candidate | Passed |
| [LW03](manifests/LW03.json) | [Solitary Execution](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW03.png) | gloves or gauntlets | Generated candidate | Passed |
| [LW04](manifests/LW04.json) | [Final Order](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW04.png) | necklace | Generated candidate | Passed |
| [LA01](manifests/LA01.json) | [Endless Trajectory](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA01.png) | weapon | Generated candidate | Passed |
| [LA02](manifests/LA02.json) | [Narrowed Line](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA02.png) | gloves or gauntlets | Generated candidate | Passed |
| [LA03](manifests/LA03.json) | [Viper's Molt](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA03.png) | boots | Generated candidate | Passed |
| [LA04](manifests/LA04.json) | [Black Contagion](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA04.png) | necklace | Generated candidate | Passed |
| [LM01](manifests/LM01.json) | [Winter's Trail](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM01.png) | weapon | Generated candidate | Passed |
| [LM02](manifests/LM02.json) | [Echoing Ember](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM02.png) | gloves or gauntlets | Generated candidate | Passed |
| [LM03](manifests/LM03.json) | [Knot of Feedback](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM03.png) | belt | Generated candidate | Passed |
| [LM04](manifests/LM04.json) | [Circuit of the End](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM04.png) | necklace | Generated candidate | Passed |
| [LC01](manifests/LC01.json) | [Watchman's Ring](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LC01.png) | finger ring | Generated candidate | Passed |
| [LC02](manifests/LC02.json) | [Oath of Restraint](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LC02.png) | necklace | Generated candidate | Passed |
| [LC03](manifests/LC03.json) | [Unquenched Heart](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LC03.png) | belt | Generated candidate | Passed |
| [LW05](manifests/LW05.json) | [Turning Rack](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW05.png) | weapon | Generated candidate | Passed |
| [LW06](manifests/LW06.json) | [Rising Ire](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW06.png) | gloves or gauntlets | Generated candidate | Passed |
| [LW07](manifests/LW07.json) | [Flesh-Cutting Wind](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW07.png) | weapon | Generated candidate | Passed |
| [LW08](manifests/LW08.json) | [Red Rampart](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW08.png) | torso armor | Generated candidate | Passed |
| [LW09](manifests/LW09.json) | [Hastening Grasp](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW09.png) | gloves or gauntlets | Generated candidate | Passed |
| [LW10](manifests/LW10.json) | [Parched Fury](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW10.png) | finger ring | Generated candidate | Passed |
| [LW11](manifests/LW11.json) | [Scorched Landing](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW11.png) | boots | Generated candidate | Passed |
| [LW12](manifests/LW12.json) | [Returning Leap](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW12.png) | finger ring | Generated candidate | Passed |
| [LW13](manifests/LW13.json) | [Execution Foretold](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW13.png) | necklace | Generated candidate | Passed |
| [LW14](manifests/LW14.json) | [Impact Cuirass](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW14.png) | torso armor | Generated candidate | Passed |
| [LW15](manifests/LW15.json) | [Devouring Heel](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW15.png) | boots | Generated candidate | Passed |
| [LW16](manifests/LW16.json) | [Silencing Descent](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW16.png) | head armor | Generated candidate | Passed |
| [LW17](manifests/LW17.json) | [Crushed Crown](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW17.png) | weapon | Generated candidate | Passed |
| [LW18](manifests/LW18.json) | [Skull Echo](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW18.png) | gloves or gauntlets | Generated candidate | Passed |
| [LW19](manifests/LW19.json) | [Deep-Carved Scar](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW19.png) | weapon | Generated candidate | Passed |
| [LW20](manifests/LW20.json) | [Bloodied Writ](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW20.png) | belt | Generated candidate | Passed |
| [LW21](manifests/LW21.json) | [Battlefield Recall](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW21.png) | finger ring | Generated candidate | Passed |
| [LW22](manifests/LW22.json) | [Brandbreaker](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW22.png) | gloves or gauntlets | Generated candidate | Passed |
| [LW23](manifests/LW23.json) | [Twice-Ringing Earth](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW23.png) | weapon | Generated candidate | Passed |
| [LW24](manifests/LW24.json) | [Binder's Cord](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW24.png) | belt | Generated candidate | Passed |
| [LW25](manifests/LW25.json) | [Stormgate](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW25.png) | necklace | Generated candidate | Passed |
| [LW26](manifests/LW26.json) | [Bastion of Remains](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW26.png) | torso armor | Generated candidate | Passed |
| [LW27](manifests/LW27.json) | [Ankle-Binding Rift](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW27.png) | boots | Generated candidate | Passed |
| [LW28](manifests/LW28.json) | [Cry-Bound Strike](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW28.png) | head armor | Generated candidate | Passed |
| [LW29](manifests/LW29.json) | [Ironwall Shards](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW29.png) | torso armor | Generated candidate | Passed |
| [LW30](manifests/LW30.json) | [Mending Ironcore](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW30.png) | belt | Generated candidate | Passed |
| [LW31](manifests/LW31.json) | [Unblinking Watch](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW31.png) | head armor | Generated candidate | Passed |
| [LW32](manifests/LW32.json) | [Unbending Vow](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW32.png) | finger ring | Generated candidate | Passed |
| [LW33](manifests/LW33.json) | [Answer of the Defender](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW33.png) | gloves or gauntlets | Generated candidate | Passed |
| [LW34](manifests/LW34.json) | [Horn of Resentment](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW34.png) | necklace | Generated candidate | Passed |
| [LW35](manifests/LW35.json) | [Comrade's Last Wish](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW35.png) | torso armor | Generated candidate | Passed |
| [LW36](manifests/LW36.json) | [Bloodbeat](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW36.png) | gloves or gauntlets | Generated candidate | Passed |
| [LW37](manifests/LW37.json) | [Edict of Assault](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW37.png) | boots | Generated candidate | Passed |
| [LW38](manifests/LW38.json) | [Unquenched Resolve](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW38.png) | finger ring | Generated candidate | Passed |
| [LW39](manifests/LW39.json) | [Third Sentence](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW39.png) | necklace | Generated candidate | Passed |
| [LW40](manifests/LW40.json) | [Bitter Draught Resolve](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW40.png) | belt | Generated candidate | Passed |
| [LA05](manifests/LA05.json) | [Trailing Arrowhead](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA05.png) | weapon | Generated candidate | Passed |
| [LA06](manifests/LA06.json) | [Oath of the Far Hunt](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA06.png) | necklace | Generated candidate | Passed |
| [LA07](manifests/LA07.json) | [Brand-Reading Eye](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA07.png) | head armor | Generated candidate | Passed |
| [LA08](manifests/LA08.json) | [Shadow Rupture](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA08.png) | gloves or gauntlets | Generated candidate | Passed |
| [LA09](manifests/LA09.json) | [Mirestring](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA09.png) | weapon | Generated candidate | Passed |
| [LA10](manifests/LA10.json) | [Escape-Opening Arrow](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA10.png) | boots | Generated candidate | Passed |
| [LA11](manifests/LA11.json) | [Seed of Arrow Rain](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA11.png) | weapon | Generated candidate | Passed |
| [LA12](manifests/LA12.json) | [Death Draws Near](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA12.png) | gloves or gauntlets | Generated candidate | Passed |
| [LA13](manifests/LA13.json) | [Relentless Loading](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA13.png) | gloves or gauntlets | Generated candidate | Passed |
| [LA14](manifests/LA14.json) | [Venom-Lacquered Fletching](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA14.png) | weapon | Generated candidate | Passed |
| [LA15](manifests/LA15.json) | [Branded Concussion](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA15.png) | head armor | Generated candidate | Passed |
| [LA16](manifests/LA16.json) | [Unhurried Archer](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA16.png) | finger ring | Generated candidate | Passed |
| [LA17](manifests/LA17.json) | [Trapper's Aim](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA17.png) | necklace | Generated candidate | Passed |
| [LA18](manifests/LA18.json) | [Venom-Harvesting Knot](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA18.png) | belt | Generated candidate | Passed |
| [LA19](manifests/LA19.json) | [Trap-Leaving Step](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA19.png) | boots | Generated candidate | Passed |
| [LA20](manifests/LA20.json) | [Black Root](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA20.png) | torso armor | Generated candidate | Passed |
| [LA21](manifests/LA21.json) | [Brand-Seeping Venom](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA21.png) | weapon | Generated candidate | Passed |
| [LA22](manifests/LA22.json) | [Last Venom Haze](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA22.png) | finger ring | Generated candidate | Passed |
| [LA23](manifests/LA23.json) | [Venomcloud Landing](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA23.png) | boots | Generated candidate | Passed |
| [LA24](manifests/LA24.json) | [Nimble Reload](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA24.png) | gloves or gauntlets | Generated candidate | Passed |
| [LA25](manifests/LA25.json) | [Fading Wound](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA25.png) | torso armor | Generated candidate | Passed |
| [LA26](manifests/LA26.json) | [Resetting Noose](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA26.png) | belt | Generated candidate | Passed |
| [LA27](manifests/LA27.json) | [Retreater's Riposte](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA27.png) | necklace | Generated candidate | Passed |
| [LA28](manifests/LA28.json) | [Shed Husk](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA28.png) | torso armor | Generated candidate | Passed |
| [LA29](manifests/LA29.json) | [Quarry's Pledge](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA29.png) | finger ring | Generated candidate | Passed |
| [LA30](manifests/LA30.json) | [Foreseen End](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA30.png) | weapon | Generated candidate | Passed |
| [LA31](manifests/LA31.json) | [Veintracker](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA31.png) | belt | Generated candidate | Passed |
| [LA32](manifests/LA32.json) | [Rewritten Hunt Ledger](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA32.png) | head armor | Generated candidate | Passed |
| [LA33](manifests/LA33.json) | [Binding Gaze](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA33.png) | necklace | Generated candidate | Passed |
| [LA34](manifests/LA34.json) | [Gloom Bloom](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA34.png) | weapon | Generated candidate | Passed |
| [LA35](manifests/LA35.json) | [Venomshade Erosion](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA35.png) | gloves or gauntlets | Generated candidate | Passed |
| [LA36](manifests/LA36.json) | [Shadow Shackles](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA36.png) | torso armor | Generated candidate | Passed |
| [LA37](manifests/LA37.json) | [Alchemist's Pulse](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA37.png) | finger ring | Generated candidate | Passed |
| [LA38](manifests/LA38.json) | [Thrice-Drawn String](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA38.png) | necklace | Generated candidate | Passed |
| [LA39](manifests/LA39.json) | [Venom-Weathered Hide](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA39.png) | torso armor | Generated candidate | Passed |
| [LA40](manifests/LA40.json) | [Mist-Drinking Boots](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA40.png) | boots | Generated candidate | Passed |
| [LM05](manifests/LM05.json) | [Lingering Ember](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM05.png) | weapon | Generated candidate | Passed |
| [LM06](manifests/LM06.json) | [Unblemished Furnace](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM06.png) | necklace | Generated candidate | Passed |
| [LM07](manifests/LM07.json) | [Ember Succession](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM07.png) | gloves or gauntlets | Generated candidate | Passed |
| [LM08](manifests/LM08.json) | [Flameroot](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM08.png) | head armor | Generated candidate | Passed |
| [LM09](manifests/LM09.json) | [Mana Drawn from Ash](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM09.png) | finger ring | Generated candidate | Passed |
| [LM10](manifests/LM10.json) | [Rift-Opening Spark](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM10.png) | boots | Generated candidate | Passed |
| [LM11](manifests/LM11.json) | [Splintered Snowflake](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM11.png) | weapon | Generated candidate | Passed |
| [LM12](manifests/LM12.json) | [Judgment of Distant Winter](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM12.png) | necklace | Generated candidate | Passed |
| [LM13](manifests/LM13.json) | [Frost Husk](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM13.png) | torso armor | Generated candidate | Passed |
| [LM14](manifests/LM14.json) | [Cold Circulation](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM14.png) | finger ring | Generated candidate | Passed |
| [LM15](manifests/LM15.json) | [Freezing Sigil](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM15.png) | gloves or gauntlets | Generated candidate | Passed |
| [LM16](manifests/LM16.json) | [Icebound Memory](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM16.png) | head armor | Generated candidate | Passed |
| [LM17](manifests/LM17.json) | [Overflowing Charge](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM17.png) | weapon | Generated candidate | Passed |
| [LM18](manifests/LM18.json) | [Close-Quarters Judgment](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM18.png) | necklace | Generated candidate | Passed |
| [LM19](manifests/LM19.json) | [Storm Continuum](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM19.png) | gloves or gauntlets | Generated candidate | Passed |
| [LM20](manifests/LM20.json) | [Reclaimed Current](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM20.png) | finger ring | Generated candidate | Passed |
| [LM21](manifests/LM21.json) | [Clinging Lightning](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM21.png) | head armor | Generated candidate | Passed |
| [LM22](manifests/LM22.json) | [Wintercalling Thunder](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM22.png) | belt | Generated candidate | Passed |
| [LM23](manifests/LM23.json) | [Spatial Discharge](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM23.png) | boots | Generated candidate | Passed |
| [LM24](manifests/LM24.json) | [Thunderbolt Arrival](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM24.png) | weapon | Generated candidate | Passed |
| [LM25](manifests/LM25.json) | [Weightless Boundary](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM25.png) | boots | Generated candidate | Passed |
| [LM26](manifests/LM26.json) | [Leaping Flame](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM26.png) | necklace | Generated candidate | Passed |
| [LM27](manifests/LM27.json) | [Riftwoven Mantle](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM27.png) | torso armor | Generated candidate | Passed |
| [LM28](manifests/LM28.json) | [Mana from the Between](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM28.png) | finger ring | Generated candidate | Passed |
| [LM29](manifests/LM29.json) | [Winter Refuge](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM29.png) | torso armor | Generated candidate | Passed |
| [LM30](manifests/LM30.json) | [Mending Crystal Veil](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM30.png) | belt | Generated candidate | Passed |
| [LM31](manifests/LM31.json) | [Charged Shell](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM31.png) | gloves or gauntlets | Generated candidate | Passed |
| [LM32](manifests/LM32.json) | [Unbroken Escape Route](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM32.png) | boots | Generated candidate | Passed |
| [LM33](manifests/LM33.json) | [Chill-Wrapped Knot](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM33.png) | belt | Generated candidate | Passed |
| [LM34](manifests/LM34.json) | [Second Winter](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM34.png) | weapon | Generated candidate | Passed |
| [LM35](manifests/LM35.json) | [Bound Mana](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM35.png) | finger ring | Generated candidate | Passed |
| [LM36](manifests/LM36.json) | [Deepening Blizzard](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM36.png) | necklace | Generated candidate | Passed |
| [LM37](manifests/LM37.json) | [Icebreaking Oath](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM37.png) | gloves or gauntlets | Generated candidate | Passed |
| [LM38](manifests/LM38.json) | [Wellspring of Fundamentals](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM38.png) | finger ring | Generated candidate | Passed |
| [LM39](manifests/LM39.json) | [Reserved Spark](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM39.png) | head armor | Generated candidate | Passed |
| [LM40](manifests/LM40.json) | [Crystal in the Vial](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM40.png) | belt | Generated candidate | Passed |
| [DES_LW41](manifests/DES_LW41.json) | [Pursuer's Chain](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/DES_LW41.png) | necklace | Generated candidate | Passed |
| [DES_LW42](manifests/DES_LW42.json) | [Wound Reaper's Grasp](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/DES_LW42.png) | gloves or gauntlets | Generated candidate | Passed |
| [DES_LW43](manifests/DES_LW43.json) | [Moving Rampart](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/DES_LW43.png) | torso armor | Generated candidate | Passed |
| [DES_LW44](manifests/DES_LW44.json) | [Mountain Tremor](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/DES_LW44.png) | belt | Generated candidate | Passed |
| [DES_LW45](manifests/DES_LW45.json) | [Unspent Resolve](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/DES_LW45.png) | finger ring | Generated candidate | Passed |
| [DES_LW46](manifests/DES_LW46.json) | [Ancestral King's Crown](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/DES_LW46.png) | head armor | Generated candidate | Passed |
| [DES_LA41](manifests/DES_LA41.json) | [Patient Hunter's Bow](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/DES_LA41.png) | weapon | Generated candidate | Passed |
| [DES_LA42](manifests/DES_LA42.json) | [Winterthorn Grasp](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/DES_LA42.png) | gloves or gauntlets | Generated candidate | Passed |
| [DES_LA43](manifests/DES_LA43.json) | [Decoy Mantle](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/DES_LA43.png) | torso armor | Generated candidate | Passed |
| [DES_LA44](manifests/DES_LA44.json) | [Forked Fang](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/DES_LA44.png) | necklace | Generated candidate | Passed |
| [DES_LA45](manifests/DES_LA45.json) | [Prepared Outpost](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/DES_LA45.png) | belt | Generated candidate | Passed |
| [DES_LA46](manifests/DES_LA46.json) | [Oath of Two Hunts](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/DES_LA46.png) | finger ring | Generated candidate | Passed |
| [DES_LM41](manifests/DES_LM41.json) | [Kindling Grasp](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/DES_LM41.png) | gloves or gauntlets | Generated candidate | Passed |
| [DES_LM42](manifests/DES_LM42.json) | [Glassglacier Staff](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/DES_LM42.png) | weapon | Generated candidate | Passed |
| [DES_LM43](manifests/DES_LM43.json) | [Thunder Collector](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/DES_LM43.png) | necklace | Generated candidate | Passed |
| [DES_LM44](manifests/DES_LM44.json) | [Quiet Rift](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/DES_LM44.png) | belt | Generated candidate | Passed |
| [DES_LM45](manifests/DES_LM45.json) | [Compass Center](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/DES_LM45.png) | torso armor | Generated candidate | Passed |
| [DES_LM46](manifests/DES_LM46.json) | [Threefold Crownstone](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/DES_LM46.png) | finger ring | Generated candidate | Passed |

## Visual review evidence

Each group was reviewed using dark/light 64px comparisons and native-resolution edge samples.

| Reviewed IDs | Evidence |
| --- | --- |
| LW01, LW02, LW03, LW04, LA01, LA02, LA03, LA04, LM01, LM02 | [first-ten.png](qa/first-ten.png) / [first-ten-native-edges.png](qa/first-ten-native-edges.png) |
| LM03, LM04, LC01, LC02, LC03, LW05, LW06, LW07 | [batch-003.png](qa/batch-003.png) / [batch-003-edges.png](qa/batch-003-edges.png) |
| LW08, LW09, LW10, LW11, LW12, LW13, LW14, LW15, LW16 | [batch-006.png](qa/batch-006.png) / [batch-006-edges.png](qa/batch-006-edges.png) |
| LW17, LW18, LW19, LW20, LW21, LW22, LW23, LW24, LW25, LW26, LW27, LW28 | [batch-007.png](qa/batch-007.png) / [batch-007-edges.png](qa/batch-007-edges.png) |
| LW29, LW30, LW31, LW32, LW33, LW34, LW35, LW36, LW37 | [batch-008.png](qa/batch-008.png) / [batch-008-edges.png](qa/batch-008-edges.png) |
| LW38, LW39, LW40, LA05, LA06, LA07, LA08, LA09, LA10, LA11, LA12, LA13 | [batch-009.png](qa/batch-009.png) / [batch-009-edges.png](qa/batch-009-edges.png) |
| LA14, LA15, LA16, LA17, LA18, LA19, LA20, LA21, LA22, LA23 | [batch-010.png](qa/batch-010.png) / [batch-010-edges.png](qa/batch-010-edges.png) |
| LA24, LA25, LA26, LA27, LA28, LA29, LA32 | [batch-011.png](qa/batch-011.png) / [batch-011-edges.png](qa/batch-011-edges.png) |
| LA30, LA31, LA33, LA34, LA35, LA36, LA37, LA38, LA39 | [batch-012.png](qa/batch-012.png) / [batch-012-edges.png](qa/batch-012-edges.png) |
| LA40, LM05, LM06, LM07, LM08, LM09, LM10, LM11, LM12, LM13, LM14, LM15 | [batch-013.png](qa/batch-013.png) / [batch-013-edges.png](qa/batch-013-edges.png) |
| LM16, LM17, LM18, LM19, LM20, LM21, LM22, LM23, LM24, LM25, LM26, LM27 | [batch-014.png](qa/batch-014.png) / [batch-014-edges.png](qa/batch-014-edges.png) |
| LM28, LM29, LM30, LM31, LM32, LM33, LM34, LM35, LM38, LM39 | [batch-015.png](qa/batch-015.png) / [batch-015-edges.png](qa/batch-015-edges.png) |
| LM36, LM40 | [batch-016.png](qa/batch-016.png) / [batch-016-edges.png](qa/batch-016-edges.png) |
| LM37 | [batch-017.png](qa/batch-017.png) / [batch-017-edges.png](qa/batch-017-edges.png) |
| DES_LW41, DES_LW42, DES_LW43, DES_LW44, DES_LW45, DES_LA41, DES_LA42, DES_LA43 | [batch-004.png](qa/batch-004.png) / [batch-004-edges.png](qa/batch-004-edges.png) |
| DES_LW46 | [batch-004.png](qa/batch-004.png) / [batch-004-edges.png](qa/batch-004-edges.png) / [DES_LW46-top-edge.png](qa/DES_LW46-top-edge.png) |
| DES_LA44, DES_LA45, DES_LA46, DES_LM41, DES_LM42, DES_LM43, DES_LM44, DES_LM45, DES_LM46 | [batch-005.png](qa/batch-005.png) / [batch-005-edges.png](qa/batch-005-edges.png) |
