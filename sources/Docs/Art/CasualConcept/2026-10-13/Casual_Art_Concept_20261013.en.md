# Casual art concept switch (2026-10-13)

Updated: 2026-10-14

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

## Character selection mockup

Each class now has two genders, so the selection screen becomes: **pick a class, then pick a gender (male / female)**, then enter the game. There is no nickname field (it is created later, in a tutorial once the player has progressed). Design canvas: https://claude.ai/artifact/K17toUvjigyghBaRN1A8D4 (25 boards: portrait / landscape / PC × KO / EN, class and gender states, preparing, entering, active rift).

- The composition follows the current selection screen (`Docs/Implementation/Character_Selection.en.md`): back to title, motion and language buttons, heading, three heroes (the chosen one steps forward, the others step back), class buttons, details, START and the hint line. New: **two gender buttons** and six hero illustrations. Switching gender switches all three heroes to that gender.
- The details show three signature skills of the class (icons and names from the skill tree data).
- With a rift in progress, only that character can be chosen, as before; class and gender are both fixed and the rest show as locked.
- Copy reuses the existing strings (battle stance, entering the sanctuary, class taglines); the heading, subtitle, gender labels and "Entering…" are new proposals.
- Art: `CharacterSelect/hero_<class>_<gender>.png`, six 1024×1536 images with transparent backgrounds generated by Codex, plus the bright sanctuary courtyard `selection_courtyard_casual.png` (1536×1024). Prompts and manifests sit beside them. The tool does not report a model name (`unknown`); not yet approved for release. One hero (the female warrior) touches the top edge with her hair.
- Open: how existing saved characters (per-class level) should appear, and whether one account may hold several class-gender combinations.
- Superseded by "Character create/select screen: town scene" below.

## Character create/select screen: town scene (2026-10-14)

After login, creating a character and choosing one to enter with happen on the same screen. It is a homage to the Dungeons & Dragons arcade games: the party hanging out in town before setting off. Design canvas: https://claude.ai/artifact/M43Cy6wg7BJUqpKxofu21M (28 boards: 11 portrait, 12 landscape, 5 PC, Korean and English).

- The scene is a market street. On the tavern terrace in front, the male warrior downs a wooden tankard and the female warrior sits on a barrel with a drumstick. At the fruit stall in the middle, the male ranger haggles over an apple and the female ranger holds a melon and a basket. At the magic shop by the fountain in the back, the male mage reads an old book and the female mage inspects a potion. No hero holds a weapon.
- The first attempt (heroes lined up on an empty plaza, each carrying a prop) was rejected as unnatural. The rebuild starts from one painted scene in which every hero does something believable in a believable place.
- Landscape and PC show the whole scene on one screen. Portrait uses the same scene recomposed tall. The crests on the right (mage, ranger, warrior from top to bottom) move the camera to that class's pair and zoom in; farther rows zoom in more, so the move reads as a dolly into the street.
- The portrait crests are 65 px round buttons with the same ink outline as the other buttons, placed at the middle of the screen height 112 px apart; the focused class gets a class-coloured ring and glow. The per-class camera framing was shifted left so the crests never cover a hero.
- Unselected heroes are desaturated so they look inactive. Tapping one restores its colour and adds a white outline with a class-coloured glow, plus a class name tag at its feet.
- A hero that has never been played shows a + above its head. Tapping it plays a 1.8-second cheer pose on the spot with a line that fits the hero's personality, then the hero returns to its pose and Lv.1 plus a person icon appear above the head. The person icon opens the equipped-gear popup (8 slots: weapon, head, chest, hands, feet, belt, amulet, ring).
- Choosing a created hero shows the Play button at the bottom centre. Pressing it switches the hero to a battle-ready pose holding the equipped weapon (warrior sword, ranger bow, mage crystal staff) with a line, then to the entering state. While a rift is in progress only that hero can be chosen and the rest show as locked, as before.

Lines (new copy proposals; they become localization keys in the game):

| Hero | Created (cheer) | Entering (battle-ready) |
|---|---|---|
| Male warrior | 좋아, 한판 붙어 보자고! / Alright, let's brawl! | 다 덤벼라! 앞장은 내가 선다! / Come at me! I'll lead the charge! |
| Female warrior | 배도 채웠겠다, 덤벼! / Belly's full. Bring it on! | 몸 다 풀렸어. 전부 날려 줄게! / All warmed up. I'll send them flying! |
| Male ranger | 흥정 끝! 이제 사냥 시간이야. / Deal's done. Hunting time! | 한 발이면 충분하지! / One shot's all I need! |
| Female ranger | 조용히, 확실하게. 따라와. / Quiet and sure. Follow me. | 숨 고르고… 놓치지 않아. / Breathe… I won't miss. |
| Male mage | 책에 다 나와 있었다고! / It was all in the book! | 계산 끝. 불꽃 맛 좀 봐라! / Calculations done. Taste the flames! |
| Female mage | 반짝반짝, 마법 시작이야! / Sparkle sparkle, magic time! | 자, 마법 쇼를 시작해 볼까? / Shall we start the magic show? |
- Example account in the mockup: male warrior Lv.24, female ranger Lv.17, female mage Lv.31 (rift in progress); the other three are not created yet.
- This screen has no motion or language buttons. In the game the motion is always on and the language is changed in Settings (user decision, 2026-10-14); the two buttons on the current selection screen go away when the new screen is built.

How it was made:

- Codex painted the scene with the heroes in it as one picture. For landscape, the better of two candidates (larger heroes, more depth) was chosen; portrait was recomposed with the landscape scene as reference. Codex edits also produced clean plates without the heroes (`clean_*.png`).
- The hero layers are the scene's own pixels, cut out with macOS Vision subject lifting (`VNGenerateForegroundInstanceMaskRequest`, [tools/lift.swift](CharacterPlaza/tools/lift.swift)). They sit exactly over the painted heroes, so the selected and inactive looks match the painting. During the cheer, a piece of the clean plate hides the painted hero and a weapon-free full-body cheer pose (Codex) stands in its place. Crop boxes and seed points are in [tools/spec_*.json](CharacterPlaza/tools/spec_landscape.json); the processing script is [tools/prep_scene.py](CharacterPlaza/tools/prep_scene.py). The first pass clipped the ranger hoods and the portrait female mage's hat tip, which cut the outline. The script now grows a crop side the cut-out touches and lifts again, keeps only the part connected to the seed point, and gives pixels claimed by two heroes to the nearer one; a passer-by touching the portrait female mage is removed with an exclusion box.
- The lifting and patching are mockup processing. They differ from the game asset rule (native alpha only), so the game will rebuild this screen with the 3D heroes and animations.

| File | Content |
|---|---|
| [scene_landscape.png](CharacterPlaza/scene_landscape.png) | Town scene for landscape and PC (1536×1024) |
| [scene_portrait.png](CharacterPlaza/scene_portrait.png) | Town scene for portrait (1024×1536) |
| [clean_landscape.png](CharacterPlaza/clean_landscape.png) · [clean_portrait.png](CharacterPlaza/clean_portrait.png) | Clean plates without the heroes (for the cheer) |
| [cheer_warrior_m.png](CharacterPlaza/cheer_warrior_m.png) and 5 more | Each hero's cheer pose (transparent background) |
| [fight_warrior_m.png](CharacterPlaza/fight_warrior_m.png) and 5 more | Each hero's battle-ready pose with the equipped weapon (transparent background). The first male ranger request ("aiming the bow") was blocked by the Codex safety filter, so he was redrawn holding the bow with a thumbs-up |
| [crests.png](CharacterPlaza/crests.png) | The three class crests (warrior, ranger, mage) |

Each picture sits beside its prompt (`PROMPT_*.md`) and manifest. `PROMPT_cheer_*.md` also describes a prop pose requested in the same run; those pictures were dropped with the first attempt. The tool does not report a model name (`unknown`); not yet approved for release.

Open:

- The male warrior's tankard stays (user decision, 2026-10-14).
- The gear popup icons are still the old dark style.
- The portrait mage framing is zoomed 1.2× and slightly soft.
- The NPCs in the scene (fruit-seller, magic shop keeper, one passer-by) are not selectable.
- Checked only as static renders in local Chrome; the live behaviour inside the canvas and real devices were not checked.

## Open items

- Build the other five heroes the same way; decide the image hand-off first (manual upload, the Chrome extension's upload tool, or base64 injection), because the in-app browser cannot upload files.
- A humanoid hero rig and the player-model swap are separate tasks.
- The in-flight dark-gothic 3D overhaul and the daily monster routine conflict with the new concept and need a continue/stop/keep decision.
- The female warrior's midriff outfit needs a 12+ rating check.
- A mild depth-of-field blur remains on the foreground imps in the battle scene after two regenerations.
