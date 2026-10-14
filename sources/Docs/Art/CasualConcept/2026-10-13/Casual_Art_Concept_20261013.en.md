# Casual art concept switch (2026-10-13)

Updated: 2026-10-13

[한국어](Casual_Art_Concept_20261013.md)

## Decisions

- The art concept moves from dark gothic to a **bright, cute, casual medieval fantasy for ages 12+**. UI, combat, saves, screen structure and the game's setting stay; only art changes.
- No player-facing option that keeps both concepts: the switch goes one way, and the current state stays reachable through a tag/branch on `main`.
- Playable heroes become **six**: male and female warrior, mage and archer.
- Of four candidate styles (chunky brawler / angular / comic-ink cel / shonen chibi) the user chose **D "shonen-manga chibi"**: Toriyama (Dragon Quest) plus Cookie Run: Kingdom attitude, 3.5-4 heads tall, rounded bodies with sharp accents (spiky hair, sharp eyes, a small fang).
- Rendering is two-step cel shading (flat colour plus one hard shadow tone), an even outline and matte materials. Shapes stay buildable at about 6,000 triangles and a 512 px texture per character so Hunyuan3D output can be used with minimal processing.

## Files

| File | Content |
|---|---|
| [title_key_art_v05_styleD.png](title_key_art_v05_styleD.png) | Title key art; the top 40% is left open for the logo |
| [battle_encircled_v02_styleD.png](battle_encircled_v02_styleD.png) | Heroes surrounded on a rift battlefield, ready for battle; monsters follow the roster families (skeleton, corpse, thornhorn, spider, drowned, wraith, fanatic, hell minion) |
| [Characters/sheet_1_warrior_m.png](Characters/sheet_1_warrior_m.png) | Male warrior model sheet: front / three-quarter / side / back A-pose, three expressions, weapon and shield |
| [Characters/sheet_2_warrior_f.png](Characters/sheet_2_warrior_f.png) | Female warrior model sheet |
| [Characters/sheet_3_mage_m.png](Characters/sheet_3_mage_m.png) | Male mage model sheet |
| [Characters/sheet_4_mage_f.png](Characters/sheet_4_mage_f.png) | Female mage model sheet |
| [Characters/sheet_5_archer_m.png](Characters/sheet_5_archer_m.png) | Male archer model sheet |
| [Characters/sheet_6_archer_f.png](Characters/sheet_6_archer_f.png) | Female archer model sheet |

Each image sits next to its request prompt (`PROMPT_*.md`) and a manifest (`manifest_*.json`) with the source path, SHA-256 and size. Earlier drafts (v01-v04, the A-D style comparison and the first line-up) were deleted at the user's request.

## How they were made

- Generated with the Codex CLI built-in `image_gen` (`codex exec -m gpt-6-astra`); every PNG is a byte-for-byte copy of the generated original. The tool does not report a model name, so each manifest records `unknown`, and like the other Codex art these are not yet approved for release.
- The six model sheets came from six parallel runs. The identity reference was the style-D line-up from the comparison round, and each sheet prompt describes its hero in words.

## Hunyuan 3D test (male warrior)

The front view of the male warrior sheet ([HunyuanInput/warrior_m_front.png](Characters/HunyuanInput/warrior_m_front.png)) went through Hunyuan3D (hy3d.tencent.ai) Image-to-3D V3.1. The user uploaded it by hand in the in-app browser; the face count was 1.5M (50k was intended, but the denser source only helps the bake). The 80.3 MB textured GLB and the preview live outside git in `AssetDeliverables/Hunyuan/2026-10-13/heroes/`; `tools/hunyuan_monster_ready.py --id HW01 --archetype Biped --height 1.8` baked the game tier (6,000 / 3,000 / 1,500 triangles, 512 px A, C, N, ORM) and auto-rigged it. Every QA gate passed (91 s).

| File | Content |
|---|---|
| [warrior_m_preview.png](Characters/HunyuanTest/warrior_m_preview.png) | Hunyuan render preview (512 px) |
| [warrior_m_HW01_contact_sheet.png](Characters/HunyuanTest/warrior_m_HW01_contact_sheet.png) | Game-tier LOD0 contact sheet: front, side, back, the 49-degree game view, walk, windup and strike poses |
| [warrior_m_hunyuan_test.json](Characters/HunyuanTest/warrior_m_hunyuan_test.json) | Creation id, face counts, source hashes, QA gate values |

Conclusion: the style-D traits (spiky hair, fang grin, blue scarf, star shield) still read at 6,000 triangles and 512 px. The monster pivot rig replaced the right leg bone with a template and the windup pose shows a stretched shard above the head, so heroes need a humanoid rig before they go into the game. Device performance and the licence (`productionApproved: false`) are unverified.

### Second pass: weapon-free input and hand capture

In the first result the low-held sword came back as a floating blade fused to the model, and the sword and shield tore in the walk pose. The game swaps weapons and gear, so hero inputs carry no sword or shield (equipment looks and the plain base outfit come later with the presentation work; the armoured look stays for now). Codex drew an empty-handed front A-pose ([warrior_m_front_noweapon.png](Characters/HunyuanInput/warrior_m_front_noweapon.png)), regenerated at 50k faces ([preview](Characters/HunyuanTest/warrior_m_noweapon_preview.png)).

Baked with the same tool every gate passes ([HW02](Characters/HunyuanTest/warrior_m_noweapon_HW02_contact_sheet.png)) but the fingers come off in the walk pose. Cause: heat weighting gave the finger vertices hanging beside the thighs to the hips, thigh and chest bones (about 35% of the vertices below the right wrist). `tools/hunyuan_monster_ready.py` gained `--hand-capture`, which gives every vertex within 16% of the height past the wrist along the forearm and within 8% of that axis (fingers curled forward included) to the hand bone. The re-bake [HW03](Characters/HunyuanTest/warrior_m_noweapon_HW03_handcapture_contact_sheet.png) keeps the hands on the arms in the walk, windup and strike poses. Without the option existing monster outputs are unchanged. Left over: a scarf shard stretching above the shoulder in the windup. Gear swapping needs one humanoid skeleton shared by every hero, so this rig is a preview only.

## Town concept

The current dark town redrawn bright in the same style. Layout and facilities are unchanged (cross-shaped roads, the rift gateway at the crossroads, warehouse, weapon stall, stone well, blacksmith and the four-dummy training ground in the north, the rune altar by the warehouse, four shops in the south, lanterns, fences and trees).

| File | Content |
|---|---|
| [town_overview_gameview_v01.png](Town/town_overview_gameview_v01.png) | The whole town from the game camera (about 50 degrees down), following the current layout |
| [town_plaza_eyelevel_v01.png](Town/town_plaza_eyelevel_v01.png) | Eye-level key art of the central plaza |

## Open items

- Build the other five heroes the same way; decide the image hand-off first (manual upload, the Chrome extension's upload tool, or base64 injection), because the in-app browser cannot upload files.
- A humanoid hero rig and the player-model swap are separate tasks.
- The in-flight dark-gothic 3D overhaul and the daily monster routine conflict with the new concept and need a continue/stop/keep decision.
- The female warrior's midriff outfit needs a 12+ rating check.
- A mild depth-of-field blur remains on the foreground imps in the battle scene after two regenerations.
