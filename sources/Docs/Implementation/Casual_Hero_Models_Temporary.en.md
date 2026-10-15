# Casual hero 3D models (temporary)

Updated: 2026-10-15 · [한국어](Casual_Hero_Models_Temporary.md)

The new casual (StyleD chibi) hero 3D models are shown in the game as temporary stand-ins. User decision (2026-10-15): use the current models as they are for now; they may be replaced later by paid generation.

| Class | Install key | Source | State |
| --- | --- | --- | --- |
| Warrior | `HunyuanReady/Characters/Hero_Warrior` | male warrior HW02 (Hunyuan image-to-3D, rigged with `--hand-capture`) | arms and hands follow the body; a thin blue shard shows above the head in the windup |
| Ranger | `HunyuanReady/Characters/Hero_Ranger` | male archer HW17 (Hunyuan, simplified input) | fairly clean; small shards beside the hands and thighs |
| Mage | `HunyuanReady/Characters/Hero_Mage` | male mage M42 (regenerated 2026-10-15 with short sleeves and bare forearms, T-pose bent to an A-pose, rigged without hand capture) | legs walk, arms stay in the A-pose (no arm swing), nothing tears loose. Replaces HW82, whose hand flew off in the windup |
| Female warrior | `HunyuanReady/Characters/Hero_Warrior_F` | female warrior HW52 (text-to-3D T-pose to A-pose, `--hand-capture`) | arms move; a small hair-tip piece in the windup |
| Female ranger | `HunyuanReady/Characters/Hero_Ranger_F` | female archer HW08 (image-to-3D, `--hand-capture`) | long hair lifts with the raised arm |
| Female mage | `HunyuanReady/Characters/Hero_Mage_F` | female mage F33 (regenerated 2026-10-15, same recipe as M42) | arms fixed, nothing tears loose |

- Code: the existing `HeroBodyArt` keys (`Characters/Hero_*`) are unchanged; `HunyuanReadyArt` already prefers a READY model with the same key over the procedural one. READY models have no weapon sockets, so `WorldView.Actors.cs` attaches props to the hand and back bones when a socket is missing (`Pivot_Weapon` to `Pivot_Hand_L` for the bow and `Pivot_Hand_R` for sword/staff; shield to `Pivot_Hand_L`; quiver parented to `Pivot_Chest` at the READY `backBone` position, keeping the root-aligned rest rotation). The procedural hero files stay; deleting the READY folders restores them.
- Lightening: LOD 6000/3000/1500 triangles and 512 px textures per model, about 5.6–6.0 MB per folder.
- Records: the rigging tool and pre-processing (`tools/hunyuan_monster_ready.py --hand-capture`, `tools/art3d/tpose_to_apose.py`), input art and attempt results are in `Docs/Art/CasualConcept/2026-10-14/Characters/HunyuanInput/README.md` on branch `claude/hero-rig-inputs`.
- The three female heroes (`Hero_*_F`) are installed as READY models at the user's request (2026-10-15: put them in now even at low quality). Integrated main supports six male/female creation and selection slots. Its 3D body lookup still routes by class only, so the installed female model keys are not yet selected by gender.
- Mage attempts (2026-10-15): 6 Hunyuan generations, 5 of them (20 candidates) rigged. With `--hand-capture` the hand tears off the forearm and flies (gate failure or rejected by eye); without hand capture the rig passes with the arms fixed to the torso. A mage with moving arms needs an input with separate hat and sleeves, or paid generation. Sources are in `AssetDeliverables/Hunyuan/2026-10-15/heroes_text/` (outside git).

## Verification

The 86 tests and warrior native capture below are the originating owner's historical `616c88b2` proof. The integrated bow/quiver correction and 13/13 main EditMode results are recorded separately in the [integration note](Casual_Hero_Models_Integration_20261014.en.md). Merging follow-up documentation/captures `8172639e` changes no game source or assets and preserves those integration test inputs.

- Focused EditMode (WorldArt, TownModel, HunyuanMonsterReady): 86 passed, 0 failed, including Hero_Warrior/Ranger/Mage.
- macOS development-build runtime smoke (`-hellscriptMonsterModelsSmoke`): the warrior appears on the rift battle screen ([screenshot](CasualHeroModelsEvidence20261015/warrior-in-rift.jpg)). In-game screens of the ranger and mage were not captured this time. Rig verdict sheets: [warrior](CasualHeroModelsEvidence20261015/warrior-rig-sheet.jpg), [ranger](CasualHeroModelsEvidence20261015/ranger-rig-sheet.jpg), [mage HW82 (replaced)](CasualHeroModelsEvidence20261015/mage-rig-sheet.jpg), [mage M42](CasualHeroModelsEvidence20261015/mage-m42-rig-sheet.jpg), [female warrior](CasualHeroModelsEvidence20261015/warrior-f-rig-sheet.jpg), [female ranger](CasualHeroModelsEvidence20261015/ranger-f-rig-sheet.jpg), [female mage](CasualHeroModelsEvidence20261015/mage-f-rig-sheet.jpg).
- The originating owner reported 37 passing HunyuanMonsterReadyTests after the 2026-10-15 additions. This cleanup could not verify the original XML for that run or its input identity against the current assets. Each of the three existing XML files inspected contains 34 Hunyuan cases, no female cases, and the previous mage; they do not establish 37 passing cases for the later M42/female assets or current main. No in-game smoke was run for the female models or the new mage.
- Real devices, performance and build size were not verified.

The later female/M42 installation was integrated from remote PR133 (`a3f96545`) into local main. At this cleanup baseline, `3c5990e2`, documentation, source and assets were compared read-only; no Unity, build or native run was performed. The earlier 13/13 prop-attachment result retains its original `16c75fa7` inputs and does not validate the later M42/female assets. No generation or rigging is restarted during workspace cleanup.
