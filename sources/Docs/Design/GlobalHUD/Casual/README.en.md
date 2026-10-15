# Battle HUD and live graphs — casual mockup source of truth

Date: 2026-10-14 · [한국어](README.md)

This mockup redraws the battle screen's main HUD (seal, HP, resource, shield, potions, skills, level, XP, status effects, Hunt Edict button, minimap) and the battle graph windows (live DPS, monster damage, boss damage, activity time) for the casual StyleD art concept. It is the final version the user reviewed and adjusted on the design canvas (canvas version 6). This folder holds design material only; no game code changed. The game work is described in the [Codex brief](../../../Implementation/Casual_Battle_HUD_Codex_Prompt_20261014.en.md).

- Design canvas (private): https://claude.ai/artifact/PK5gEQdR1rrbW8Jzx3nDaA. The copy in this folder is the source of truth if the canvas cannot be opened.
- Other casual mockups in the same look: battle position HUD (1Pkt5gL8FtWFDzYTkTaDdJ), training ground (KZpgGkzK1YJ9yNzLex2san), skill tree (JsCKnJUbKkzfBYyJDR3493).

## Files

| Path | Contents |
|---|---|
| `canvas/Main.dc.html` | One component that draws every battle board. Props `locale` (ko, en), `orient` (pc, landscape, portrait), `mode` (normal, boss, danger), `foldDps`, `foldMg`, `foldBg`, `foldAct`, `statusOpen`, `edictOpen`. Its CSS holds the authoritative layout and look numbers |
| `canvas/Seals.dc.html` | Seal sheet for the six heroes (normal, HP under 30%, down) |
| `canvas/*.dc.html`, `canvas/canvas.json` | Board wrappers and the canvas index. `/_blob/<id>` pictures in the sources map to the files below |
| `reference/*.jpg` | 14 reference renders. PC at 1600×900 1x, phones at 2x |
| `assets/seal-*.png` | Six seals, 512×512, faces cropped from the casual full-body delivery (crop boxes in `assets/assets.json`) |
| `assets/potion-*.png` | Three potions, 512×512, squared from the casual item delivery `potion-*-casual-v01.png` by their alpha bounds |
| `tools/smoke.js` | Mockup logic check (`node tools/smoke.js`, 38 cases); the rules to carry into game tests |

| Picture in the source | File |
|---|---|
| `/_blob/ff285505…`, `0cb57a06…`, `e92e20c8…`, `87208fd2…`, `449e3303…`, `017e4e36…` | `assets/seal-warrior-m`, `-warrior-f`, `-mage-m`, `-mage-f`, `-ranger-m`, `-ranger-f` (256 px versions on the canvas) |
| `/_blob/15a094ea…`, `38eb4d3f…`, `4bae48f7…` | `assets/potion-hp`, `-mp`, `-utility` (160 px versions on the canvas) |
| `/_blob/c99fe36e…`, `b9310991…`, `01ae266e…`, `d2682983…`, `eb767351…` | Existing game skill pictures W01 Whirlwind, W02 Leap Slam, W03 Crushing Blow, W05 Iron Wall, W17 War of the Ancestors (ultimate) |
| `/_blob/e1d3bc1c…` | Existing boss picture B01 Graveyard Executioner |

## Reference renders

| File | State |
|---|---|
| `pc-ko-default`, `pc-en-default` | PC default |
| `pc-ko-boss` | PC boss fight (boss band, boss damage window) |
| `pc-ko-edict-open-effects-open` | PC with the Hunt Edict menu open and 8 effect slots |
| `landscape-ko-default`, `landscape-en-default` | Landscape default |
| `landscape-ko-danger` | Landscape danger (HP 22%, potion cooldown, zero count, not enough resource, debuffs) |
| `landscape-en-folded-edict-open` | Landscape with graph windows folded, edict menu and effects open |
| `portrait-ko-default`, `portrait-en-default` | Portrait default (small windows) |
| `portrait-en-boss` | Portrait boss fight |
| `portrait-ko-edict-open` | Portrait with the edict menu open |
| `seals-ko`, `seals-en` | Seal sheet |

## Look

These are the same rules as the other casual mockups.

- **Ink outline** `#2a3150`, 2–3 px, with a chunky drop shadow `0 3px 0 ink`. Large panels use `0 5px 0 ink` + `0 12px 20px rgba(42,49,80,.18)`.
- **Panels (DPS, activity):** cream `#fff6e3` with a white top sheen and a dashed stitch 5 px inside (`2px dashed rgba(42,49,80,.13)`). Radius is 18 on PC and 14 on phones; border is 3 px on PC and 2 px on phones.
- **Secondary windows (monster, boss damage):** white `#ffffff`, 2 px border, radius 14, no stitch.
- **Round buttons:** white, 2 px ink border, drop shadow. When on, they use a yellow gradient `#fff6c2→#ffd23f` plus a yellow ring `rgba(255,210,63,.55)`.
- **Colours:** yellow `#ffd23f`, orange `#ff9f1c`, gold ring `#ffc21a`, sky `#35a7ff` (resource), coral `#ff5a5f` (danger), mint `#5ee0d4` (shield; text `#127a72`), green `#4cbf55`, secondary text `#4a5272`/`#6b7393`/`#8a93b5`, empty track `#e6e9f0`.
- **Fonts:** numbers and titles in Jua, body in Noto Sans KR 700. Numbers on bars and slots are white with an ink outline; the RIFT title is yellow with an ink outline.
- The light-green field and the round monsters are mockup only. The game shows the real field.

## Layout per orientation

Numbers are px at the mockup screen sizes (PC 1600×900, landscape 956×440, portrait 440×956), with the reference edge named.

| Element | PC | Landscape | Portrait |
|---|---|---|---|
| Base sizes | slot 76, gap 12, potion↔ultimate group gap 24, seal 112, bar height 20, effect tile 44 (gap 8) | 48, 8, 14, 64, 15, 30 (6) | 42, 5, 10, 60, 14, 32 (6) |
| Header (RIFT, time, kills) | left 24 top 18 | 70, 8 | 18, 58 |
| Menu and settings buttons (44) | right 24 top 18 | 66, 8 | 12, 58 |
| Hunt Edict button | under menu/settings, right 24 top 74, 60 px | right 66 top 58, 44 px | right 12 top 110, 44 px |
| Edict tab menu | drops down below the button, right 22 top 142, icons 48 | right 60 top 106, icons 28 | right 8 top 162, icons 34 |
| Minimap | lower right, right 24 bottom 250, 168×132 | right 66 bottom 96, 104×84 | right 12 bottom 262, 112×86 |
| Graph window stack (DPS → monster → boss) | right 200 top 74, width 320, gap 10 | right 178 top 8, width 236, gap 8 | right 12 top 166, width 196. In a boss fight the boss window opens on the left under activity (x 12, y 356, width 180) |
| Activity window | left 24 top 124, width 320 | 70, 70, width 224 | 12, 110, width 180 (top 160 in a boss fight) |
| Boss band (boss alive) | top centre, top 18, width 460 | left 262 top 8, width 268 | left 12 right 136 top 110 |
| Status effect row | above the left edge of the first potion, left 864 bottom 134 | 444, 88 | above the slot row on the left, left 16 bottom 200 |
| Seal | left 24 bottom 60 | 70, 34 | 16, 58 |
| Level pill | centred over the seal's lower edge, 72×26 | 48×20 | 48×20 |
| HP and resource bars | left 152 bottom 86, width 300, gap 8 | 146, 38, width 200, gap 6 | 88, 62, width 260, gap 5 |
| Potion settings button | right of the bars, 48×48 | 40×38 | 40×40 |
| 3 potions + ultimate + 4 actives | right 32 bottom 27 | right 66 bottom 20 | left 16 right 16, spread, bottom 124 |
| XP bar | left/right 32, bottom 12, height 8 | left 70 right 66, bottom 12, height 6 | left/right 16, bottom 40, height 6 |

- The spot above the seal is empty now that the edict button sits top right.
- The portrait HUD stacks status effects → slot row → seal, bars and potion settings → XP instead of the one-row fit.
- In landscape the graph stack starts at the top edge (top 8) and the DPS legend is dropped so the effect row above the potions stays clear.

## Parts and states

- **Seal:** round portrait, 4 px yellow ring (3 px on phones), ink outline. Under 30% HP it switches to a coral ring with a soft 1.1 s pulse. When the hero is down it gets a grey ring and a greyscale portrait.
- **Level pill:** yellow, Jua, ink border.
- **HP and resource bars:** empty track `#e6e9f0` with a 1 px ink border, a 1 px ring and a 2 px drop. HP is a coral gradient `#ff9a9d→#ff5a5f`; resource is sky `#5cc8ff→#35a7ff→#2f8fd8`. Both have a white top sheen. Under 30% HP the HP bar gets a coral ring.
- **Shield:** a 4 px mint strip along the top of the HP bar (shield ÷ max HP) plus "Shield +420" above the bar.
- **Potion slot:** rounded square, white to cream, 2 px ink. The bottle fills 66% of the slot and the count "×12" sits below. A cooldown shows a radial cover (`rgba(42,49,80,.6)`, remaining share) and the seconds left (one decimal under 10 s). A zero count shows a dashed grey slot and a greyscale bottle. A running utility effect adds a yellow ring.
- **Skill slot:** round, cream with a yellow glow, 2 px ink. The ultimate has a gold ring and "Ultimate" below. In use shows a yellow halo; cooldown shows a radial cover and seconds; not enough resource shows a blue diamond and a desaturated icon. Locked and empty slots keep the game rules.
- **Status effects:** rounded square tiles. Buffs have a blue icon and a green "+" badge top right; debuffs have a pink tile, a coral icon and a coral "−" badge. Stacks show as an ink pill bottom right, with the seconds left under the tile. Collapsed shows 4.5 tiles with a faded right edge; the "+" button expands to 8.
- **Hunt Edict menu:** the button turns yellow and a cream vertical menu drops down below it. Tabs keep the game order (Combat, Survival, Explore, Repeat, Presets); each tab is a round icon with its name below.
- **XP:** yellow-to-orange fill, 9 ticks at 10–90% (50% darker), "EXP 62%" above the bar.

## Graph windows

Shared rules:
- The four windows (live DPS, monster damage, boss damage, activity time) each move on their own (drag the title) and fold on their own (round button on the left).
- The monster and boss windows are no longer attached to the DPS window.
- Positions are meant to be saved on the device like the existing DPS and activity positions.

Per window:
- **Live DPS:**
  - Header: "Live DPS" + "Last 3 seconds" + a large Jua number.
  - Comparison line: "±n% vs the previous run at this time" (green `#23863a` up, `#c62f35` down, with an arrow).
  - Legend: this run solid orange, previous run dashed sky.
  - Chart: top value and 0, three faint grid lines, this run as a 30% yellow area under a 3 px orange line, the previous run dashed sky (6·5), and an end dot (orange with an ink ring). Skill-use marks are small round skill icons sitting on the line (gold ring for the ultimate); the x axis shows 0 s, middle and end.
  - Bottom row: three cells for average, peak and total.
  - Per orientation: landscape drops the legend; portrait drops the legend and the three cells. Folded keeps the button, title and number.
- **Monster damage · latest 5 kinds:**
  - Row: name | bar | "520/s · 22%".
  - The bar is full at 50% of max HP. It is orange from 20%, coral from 40%, and slate `#8a93b5` below that.
  - Portrait puts name and value on one line and the bar on the next.
- **Boss damage:**
  - Header: boss seal + name. Each row has a round attack icon, name, bar and value, with the same colour rule.
  - Shown only while a boss is alive.
- **Activity time:**
  - Share bar: movement sky, attacking coral, dodging yellow, farming green. A segment of 12% or more shows its % inside.
  - Under the bar: "Cumulative time · 0.0–48.0s".
  - 2×2 cells: colour square + name, "n s · n%", and a cumulative line (movement `#2f8fd8`, attacking `#e8474c`, dodging `#d9a300`, farming `#36a845`).
  - Folded, the title keeps the same share bar beside it as colour only, with no text (96×14 on PC, 72×12 on phones).

## Copy

Existing keys come from `Resources/Localization/en.txt` unchanged. New keys add Korean and English together.

| Korean | English | Status |
|---|---|---|
| 최근 3초 평균 | Last 3 seconds | existing |
| 직전 판 같은 시점 대비 {0} | {0} vs the previous run at this time | existing |
| 이번 판 / 직전 판 | This run / Previous run | existing |
| 몬스터 피해 · 최근 5종 | Monster damage · latest 5 kinds | existing |
| {0:0.#}/초 · {1:0}% | {0:0.#}/s · {1:0}% | existing |
| 활동 시간 / 이동 / 공격 / 회피 / 파밍 | Activity time / Movement / Attacking / Dodging / Farming | existing |
| 누적 시간 · {0:0.0}~{1:0.0}초 | Cumulative time · {0:0.0}–{1:0.0}s | existing |
| 보호막 +{0:0} | Shield +{0:0} | existing |
| 궁극기 / 총 피해 | Ultimate / Total damage | existing |
| 사냥 칙령 / 전투 / 생존 / 탐색 / 반복 설정 / 프리셋·공유 | Hunt edict / Combat / Survival / Explore / Repeat settings / Presets & Sharing | existing (full tab names) |
| 상자 {0} / {1} | Chests {0} / {1} | existing |
| 그래프 위치를 기기에 저장하지 못했습니다. 다시 끌어 놓아 저장을 재시도해 주세요. | (existing English) | existing |
| 실시간 DPS | Live DPS | new (header; the folded window may reuse the existing "실시간 DPS {0:N0}") |
| 평균 / 최고 | Average / Peak | new (cell titles) |
| 반복 / 프리셋 | Repeat / Presets | short names under the tabs; "프리셋" exists, "반복" is new |
| 실시간 DPS 접기·펼치기, 몬스터 피해 접기·펼치기, 보스 피해 접기·펼치기, 활동 시간 접기·펼치기 | Fold/Expand live DPS, monster damage, boss damage, activity time | new (button labels) |
| 상태 효과 더 보기 / 상태 효과 줄이기 | More effects / Fewer effects | new |
| 물약 설정 | Potion settings | new (button label) |

"Kills 38 · Rift 72/100", the monster and skill names and all numbers in the mockup are demo values.

## Mockup notes

1. The user set these directly in this mockup:
   - The edict button sits top right and its tabs drop down, the same in every orientation.
   - The minimap sits lower right, and status effects sit above the first potion (in portrait, above the slot row on the left).
   - Portrait graphs are small windows, and each graph window moves and folds on its own.
   - A folded activity window keeps a colour bar.
2. Fitting the portrait HUD into one row, as the game does now, shrinks slots to about 27 pt on phones, hence the stacked layout above.
3. The mockup draws only the menu and settings buttons at the top right. How this relates to the game's content dock column is a game-side decision.
4. Skill and boss pictures are the existing game art, as in the other casual mockups. The battle boards use the male warrior seal to match the warrior skill pictures.
5. Mockup sizes are tuned for phone legibility (landscape slots 48, portrait 42). Matching them to the game's HUD scaling rules is part of the game work.
6. The mockup was built as a Design canvas. The reference renders come from the same runtime run locally under headless Chrome. Window dragging was not exercised by an actual drag in a browser.
