# Character create/select screen mockup (town scene)

Written: 2026-10-14 · [한국어](README.md)

After login, **creating a character and choosing one to enter with happen on the same screen.** It is a homage to the Dungeons & Dragons arcade games: the party hanging out in town before setting off. The six heroes (warrior, ranger, mage, each male and female) each spend their time believably in a market street.

- Design canvas: https://claude.ai/artifact/M43Cy6wg7BJUqpKxofu21M (28 boards, private: Codex cannot open it until the user shares it. The copy in this folder is authoritative)
- Art originals, prompts and the lifting tool: [Docs/Art/CasualConcept/2026-10-13/CharacterPlaza](../../Art/CasualConcept/2026-10-13/CharacterPlaza/README.en.md)
- Game implementation brief: [Casual_Character_Plaza_Codex_Prompt_20261014.en.md](../../Implementation/Casual_Character_Plaza_Codex_Prompt_20261014.en.md)

## Folder

| Path | Content |
|---|---|
| `source/` | The files exactly as published to the canvas: one component (`PlazaSelect.dc.html`), 27 thin per-board wrappers and `canvas.json`. Images are referenced as `/_blob/<id>`; `art_map.json` maps them to local files |
| `assets/` | The canvas images (webp): 2 scenes, 12 lifted hero layers, 12 cheer patches, 6 cheer poses, 6 battle-ready poses, 3 crests, 1 equipment icon sheet (the old dark style, mockup only) |
| `reference/` | Renders of the 28 boards (JPEG), the reference for side-by-side comparison with the game screen |
| `scene_data.json` | Picture-pixel data of the two scenes (lifted box per hero, head and feet anchors, cheer box, portrait camera framing), produced by `tools/prep_scene.py` |
| `tools/` | `smoke.js` (state flow and layout checks), `render_reference.js` (reference render), `to_jpg.py` |

## Layout

One component (`source/PlazaSelect.dc.html`) draws every state and size. Props: `locale` (ko, en), `orient` (portrait, landscape, pc), `hero` (none or one of six), `start` (normal, cheer, created, gear, entering, rift), `focus` (portrait camera class). `layout()` is the authority for the layout numbers and `renderVals()` for the state rules.

- **Landscape (956×440) and PC (1152×648 scaled 1.25)**: the whole scene covers the frame. The male and female warrior sit on the tavern terrace in front, the rangers at the fruit stall in the middle, the mages at the magic shop by the fountain in the back.
- **Portrait (440×956)**: a scene redrawn tall. The three crests on the right (mage, ranger, warrior from top to bottom) move the camera to that class's pair and zoom in (0.9 s, `cubic-bezier(.45,.05,.2,1)`). Crests are 65 px, centred on the screen height, 112 px apart; the focused class gets a class-coloured ring and glow. Everything above the focused pair is fogged with cream so two heroes read at a time.
- **Top**: only the back button (‹ Title) and the heading. **There are no motion or language buttons** (user decision: the game always plays the motion and the language is changed in Settings).
- **Bottom**: a hint pill before a choice; the green Play button at the centre once a created hero is chosen.

## States and transitions

| State | What shows | Next |
|---|---|---|
| Browsing | All six inactive (saturation .45, brightness .88). Hint "Tap a hero to choose" | Tap a hero -> selected |
| Selected (never played) | White outline plus class-coloured glow, a class name tag at the feet (class name and gender glyph), a green + button above the head. Hint "Tap + to create this adventurer". No Play | Tap + -> cheer |
| Cheer (create) | The painted hero is hidden with a background patch and the cheer pose stands in its place. 1.8 s hop (1.7 s animation) with a speech bubble line that fits the hero | After 1.8 s the hero returns to its pose, creation is done -> Lv.1 |
| Selected (created) | Lv. pill and a person icon above the head, Play at the bottom centre. Hint "Enter the sanctuary with your selected character." | Person icon -> gear popup, Play -> entering |
| Gear popup | A centred panel with 8 equipped slots (weapon, head, chest, hands, feet, belt, amulet, ring; name, slot, level, grade colour). Close at the top right | Close |
| Entering | The painted hero is hidden and a battle-ready pose with the equipped weapon stands in; a speech bubble line; the Play button turns into a disabled "Entering..."; a light cream veil covers the screen. Hint "Entering sanctuary..." | Enter the game |
| Rift in progress | Only the rift hero can be chosen (the mockup uses the female mage, Lv.31); the rest are greyscale with a padlock. Hint "Continue with the character in your active rift." | Play |

While creating or entering, other heroes and the crests ignore taps. Choosing a hero makes the portrait camera follow its class.

## Specification

- Colours: ink #2a3150, cream #fff6e3. Class colours (ring / text / soft fill): warrior #ff9f1c / #e08a00 / #fff1d6, ranger #4cbf55 / #2a8a36 / #e5f7e0, mage #35a7ff / #1f7fd1 / #e3f2ff. Play button #8ee07a -> #4cbf55 with a 3 px ink border and a 4 px hard shadow, white text with a 2 px ink outline. Heading yellow #ffd23f with an ink outline. Fonts are Jua (heading, numbers, buttons) and Noto Sans KR (body); in the game use the fonts `UiTheme` owns.
- Name tag 96×22 (landscape), 108×24 (portrait, PC). Row above the head: + button 32 (landscape) / 38, level pill height 26 / 30, person button 28 / 32, speech bubble max width 200 / 210 with 13 / 15 px text. The row is clamped to stay on screen (centre 112 to W-112 in landscape, 118 and up elsewhere) and below the heading row (landscape 60, portrait 160, PC 108).
- The mockup's + button (32 / 38 px) and person button (28 / 32 px) are smaller than the game's 44 pt touch rule. Keep the visible size and enlarge only the hit area to at least 44 pt.
- Timings: cheer 1.8 s (hop 1.7 s), battle-ready bounce 0.55 s, speech bubble pop 0.5 s, + button bob 1.2 s loop, Play shine 5.6 s loop, portrait camera 0.9 s.
- Mockup example account: male warrior Lv.24, female ranger Lv.17, female mage Lv.31 (rift in progress); the other three are not created. The gear popup's names, levels and icons are placeholders; the game shows the gear the hero actually wears through the shared components.

## Relation to the save

The user settled on **six saved heroes** on 2026-10-14. `AccountSave.heroes` always holds the six heroes (indexes 0-2 male warrior, ranger, mage; 3-5 female warrior, ranger, mage), and each hero has `gender` and `created` (whether it was created). The "+" on this screen turns `created` on. The implementation brief and its stages are in the "Decisions" section of [Casual_Character_Plaza_Codex_Prompt_20261014.en.md](../../Implementation/Casual_Character_Plaza_Codex_Prompt_20261014.en.md).

## Copy

Existing strings (reuse): `‹ 타이틀`, `선택한 캐릭터로 성소에 입장합니다.`, `진행 중인 균열의 캐릭터로 이어갑니다.`, `성소로 향하는 중…`, `닫기`, the class names `전사` `궁수` `마법사` and the equipment slot names. The new strings are below; the Korean source text is the key.

| Korean (key) | English |
|---|---|
| 나만의 모험가를 골라요 | Choose your adventurer |
| 캐릭터를 눌러 선택하세요 | Tap a hero to choose |
| +를 눌러 새 모험가를 만들어요 | Tap + to create this adventurer |
| 새 모험가 만들기 (accessibility) | Create a new adventurer |
| 플레이 | PLAY |
| 입장 중… | Entering… |
| 착용 장비 | Equipped gear |
| 착용 장비 보기 (accessibility) | View equipped gear |
| 남자 / 여자 | Male / Female |
| 균열 진행 중에는 선택할 수 없어요 (padlock hint, not shown in the mockup) | Locked while a rift is in progress |

Hero lines (new copy that fits each personality):

| Hero | Created (cheer) | Entering (battle-ready) |
|---|---|---|
| Male warrior | 좋아, 한판 붙어 보자고! / Alright, let's brawl! | 다 덤벼라! 앞장은 내가 선다! / Come at me! I'll lead the charge! |
| Female warrior | 배도 채웠겠다, 덤벼! / Belly's full. Bring it on! | 몸 다 풀렸어. 전부 날려 줄게! / All warmed up. I'll send them flying! |
| Male ranger | 흥정 끝! 이제 사냥 시간이야. / Deal's done. Hunting time! | 한 발이면 충분하지! / One shot's all I need! |
| Female ranger | 조용히, 확실하게. 따라와. / Quiet and sure. Follow me. | 숨 고르고… 놓치지 않아. / Breathe… I won't miss. |
| Male mage | 책에 다 나와 있었다고! / It was all in the book! | 계산 끝. 불꽃 맛 좀 봐라! / Calculations done. Taste the flames! |
| Female mage | 반짝반짝, 마법 시작이야! / Sparkle sparkle, magic time! | 자, 마법 쇼를 시작해 볼까? / Shall we start the magic show? |

## Boards (28 reference renders)

| Reference | Size | orient | locale | hero | start | focus | Shows |
|---|---|---|---|---|---|---|---|
| [Main](reference/Main.jpg) | 1440×810 | pc | ko | none | normal | auto | Browsing, nothing selected |
| [Portrait-Browse-Warrior-KO](reference/Portrait-Browse-Warrior-KO.jpg) | 440×956 | portrait | ko | none | normal | Warrior | Browsing, nothing selected · camera on the warrior pair |
| [Portrait-Browse-Ranger-EN](reference/Portrait-Browse-Ranger-EN.jpg) | 440×956 | portrait | en | none | normal | Ranger | Browsing, nothing selected · camera on the ranger pair |
| [Portrait-Browse-Mage-KO](reference/Portrait-Browse-Mage-KO.jpg) | 440×956 | portrait | ko | none | normal | Mage | Browsing, nothing selected · camera on the mage pair |
| [Portrait-Played-KO](reference/Portrait-Played-KO.jpg) | 440×956 | portrait | ko | warrior_m | normal | auto | Created hero selected: level, person icon, Play · male warrior |
| [Portrait-New-KO](reference/Portrait-New-KO.jpg) | 440×956 | portrait | ko | archer_m | normal | auto | Never-played hero selected: + above the head · male ranger |
| [Portrait-Cheer-KO](reference/Portrait-Cheer-KO.jpg) | 440×956 | portrait | ko | archer_m | cheer | auto | Creating: cheer pose and line · male ranger |
| [Portrait-Created-EN](reference/Portrait-Created-EN.jpg) | 440×956 | portrait | en | archer_m | created | auto | Just created, Lv.1 · male ranger |
| [Portrait-Gear-KO](reference/Portrait-Gear-KO.jpg) | 440×956 | portrait | ko | warrior_m | gear | auto | Equipped gear popup · male warrior |
| [Portrait-Gear-Mage-EN](reference/Portrait-Gear-Mage-EN.jpg) | 440×956 | portrait | en | mage_f | gear | auto | Equipped gear popup · female mage |
| [Portrait-Rift-KO](reference/Portrait-Rift-KO.jpg) | 440×956 | portrait | ko | none | rift | auto | Active rift: only the rift hero (female mage) selectable, the rest locked |
| [Portrait-Entering-EN](reference/Portrait-Entering-EN.jpg) | 440×956 | portrait | en | warrior_m | entering | auto | Play pressed: battle-ready pose and line, entering · male warrior |
| [PlazaSelect](reference/PlazaSelect.jpg) | 956×440 | landscape | ko | none | normal | auto | Browsing, nothing selected |
| [Landscape-Browse-EN](reference/Landscape-Browse-EN.jpg) | 956×440 | landscape | en | none | normal | auto | Browsing, nothing selected |
| [Landscape-Played-KO](reference/Landscape-Played-KO.jpg) | 956×440 | landscape | ko | warrior_m | normal | auto | Created hero selected: level, person icon, Play · male warrior |
| [Landscape-Played-Mage-EN](reference/Landscape-Played-Mage-EN.jpg) | 956×440 | landscape | en | mage_f | normal | auto | Created hero selected: level, person icon, Play · female mage |
| [Landscape-New-KO](reference/Landscape-New-KO.jpg) | 956×440 | landscape | ko | mage_m | normal | auto | Never-played hero selected: + above the head · male mage |
| [Landscape-New-Warrior-EN](reference/Landscape-New-Warrior-EN.jpg) | 956×440 | landscape | en | warrior_f | normal | auto | Never-played hero selected: + above the head · female warrior |
| [Landscape-Cheer-KO](reference/Landscape-Cheer-KO.jpg) | 956×440 | landscape | ko | archer_m | cheer | auto | Creating: cheer pose and line · male ranger |
| [Landscape-Created-EN](reference/Landscape-Created-EN.jpg) | 956×440 | landscape | en | archer_m | created | auto | Just created, Lv.1 · male ranger |
| [Landscape-Gear-KO](reference/Landscape-Gear-KO.jpg) | 956×440 | landscape | ko | warrior_m | gear | auto | Equipped gear popup · male warrior |
| [Landscape-Gear-Ranger-EN](reference/Landscape-Gear-Ranger-EN.jpg) | 956×440 | landscape | en | archer_f | gear | auto | Equipped gear popup · female ranger |
| [Landscape-Rift-KO](reference/Landscape-Rift-KO.jpg) | 956×440 | landscape | ko | none | rift | auto | Active rift: only the rift hero (female mage) selectable, the rest locked |
| [Landscape-Entering-EN](reference/Landscape-Entering-EN.jpg) | 956×440 | landscape | en | mage_f | entering | auto | Play pressed: battle-ready pose and line, entering · female mage |
| [PC-Played-EN](reference/PC-Played-EN.jpg) | 1440×810 | pc | en | warrior_m | normal | auto | Created hero selected: level, person icon, Play · male warrior |
| [PC-New-KO](reference/PC-New-KO.jpg) | 1440×810 | pc | ko | mage_m | normal | auto | Never-played hero selected: + above the head · male mage |
| [PC-Gear-KO](reference/PC-Gear-KO.jpg) | 1440×810 | pc | ko | archer_f | gear | auto | Equipped gear popup · female ranger |
| [PC-Rift-EN](reference/PC-Rift-EN.jpg) | 1440×810 | pc | en | none | rift | auto | Active rift: only the rift hero (female mage) selectable, the rest locked |

## Rebuild and check

```bash
node Docs/Design/CharacterPlaza/tools/smoke.js            # state flow, UI layout (overlaps, off-screen), crest camera, speech bubble
node Docs/Design/CharacterPlaza/tools/render_reference.js /tmp/plaza-ref [board name ...]   # reference render (PNG) with local Chrome
python3 Docs/Design/CharacterPlaza/tools/to_jpg.py /tmp/plaza-ref Docs/Design/CharacterPlaza/reference
```

The game-side image layers (PNG) are re-extracted from the original scenes with `Docs/Art/CasualConcept/2026-10-13/CharacterPlaza/tools/prep_scene.py` (see the README in that folder).

## Limits

- The mockup was checked only as static renders in local Chrome plus state checks. Live behaviour inside the canvas and real devices were not checked.
- The hero layers are segmented from the scene, which is mockup processing and not native alpha. That differs from the game asset rule, so register them as replaceable development art and replace them with 3D heroes and animation or native-alpha pictures before release.
- The portrait mage framing is zoomed 1.2× and slightly soft. The equipment icons are still the old dark style.
- The NPCs in the scene (fruit-seller, magic shop keeper, passer-by) are not selectable.
