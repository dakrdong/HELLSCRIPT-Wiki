# Monster Codex development plan

Updated: 2026-10-11 · For: Codex implementation · [한국어](Monster_Codex_Plan_20261011.md)

## 1. Goal

Add a Monster Codex window that records the enemies the player has killed. An enemy with at least one kill shows its portrait, name, stats and skills; one never killed stays hidden as `?` and `???`. On the detail screen a rift tier slider runs from tier 1 to `best clear + 1`; it shows the stats and skill damage that enemy would have if it were placed in a rift of the chosen tier.

## 2. Reference mockup

- Design canvas: https://claude.ai/artifact/MgykAGP9nxCmpwthfRN9P4
  - Row 1: landscape 956×440, Korean (list / enemy detail / boss detail)
  - Row 2: landscape 956×440, English
  - Row 3: portrait 440×956, three Korean then three English screens
  - Every screen works. Tap a card to open the detail; the slider, `−`/`+` and the Normal/Elite switch change the numbers.
  - The link is private. For a Codex account to open it, the user has to share it from the Share menu. If it cannot be opened, sections 3–6 of this document are the reference.
- Numbers in the mockup (kill counts, 18/25 discovered, best clear 22) are sample data. The game uses the save data.
- Do not copy hex colours from the mockup. Use the `UiTheme` tokens and `UiFonts`. The mockup colours already come from the Obsidian & Gilt values in `UiTheme`.

## 3. Entry point: HUD content dock icon

- Position: the content dock under the settings gear at the top right of the battle and plaza screens (`AddContentDock` in `GameUI.ContentDock.cs`). The user marked the slot **directly below the Hunt Edict icon**.
  - Implementation: right after `contentDockEdict=Add("사냥 칙령",...)`, add `Add("몬스터 도감","menu-monster-codex",ShowMonsterCodex)`. The Rune Board and Storage move down one slot.
  - The user's screenshot was a tutorial stage battle (Voice of the Edict, level 3). Show the icon under the same conditions as the Hunt Edict, outside the `prologue` branch, so it appears there too. Confirm this with the user (section 9).
- Icon art: a round `menu-monster-codex` medallion made with Codex/GPT image. Variants A and B, the prompt and the manifest are in `Docs/Art/MonsterCodex/`. **The user picked variant A.** Copy `Docs/Art/MonsterCodex/menu-monster-codex-A.png` unchanged to `Assets/HELLSCRIPT/Resources/Art/GlobalHUD/menu-monster-codex.png` (1254×1254, transparent corners, imported at 512 max). Do not hand-draw it or draw shapes in code.
  - Texture budget: the `Art/GlobalHUD/menu-` row in `ResourceTextureBudget` (512, ASTC 6×6) already covers it, so no new row is needed. Match the import settings of the other `menu-*.png` `.meta` files, and commit the `.meta` Unity rewrites from the build clone.
- Opening behaves like the other dock buttons (Hunt Edict, Storage): opening it in battle pauses the game like those windows, and closing restores it.

## 4. Data

### 4.0 Prerequisite: the monster registry

On 2026-10-11 the user approved a larger roster: 6 fields × 20 monsters plus 18 bosses (N01–N120, B01–B18, 138 in total). A daily routine adds 20 monsters at a time to both the game and the codex. The roster document is `Docs/Design/HELLSCRIPT_Monster_Roster_20261011(.en).md`, currently on the `claude/monster-roster` branch. So the codex follows **the list the registry defines**, not 25 fixed entries.

- Registry contract (branch `claude/monster-registry`, to be merged into `main` before the codex):
  - Data: `Assets/HELLSCRIPT/Resources/Monsters/MonsterRegistry.json`
  - Runtime: the static class `MonsterRegistry` in `Runtime/Core`
    - `All`: `IReadOnlyList<MonsterDefinition>` in codex order (field order, then ID)
    - `ById(string)`, `ForKind(int kind)`, `ForBoss(int pattern)`
    - `IdOf(EnemyState e)`: bosses map to a B id by pattern, everything else to an N id by kind. Elites count as their base kind; goblins return `null`.
  - `MonsterDefinition`:
    - Identity: `id` ("N01".."N120", "B01".."B18"), `boss`, `kind` (N number − 1, −1 for bosses), `pattern` (B number − 1, −1 for regular monsters)
    - Display: `name` (Korean string key, English in en.txt), `field` (`GameCatalog` field index; bosses have a home field too: B02 fortress, B03 cavern), `role`, `family`, `unlockTier`, `portrait` (Resources path, e.g. `Art/Monsters/Portraits/N21`)
    - Skills: a `skills` list of `{name key, description key, tags, damage multiplier or 0}`
  - The existing 25 keep their current kind/pattern numbers; new monsters are appended.
  - New monsters' stats also come from `CombatSimulation.EnemyStats(kind or pattern …)`. Extending the per-kind multipliers (`KindHealth`/`KindAttack`/`KindSpeed`, `BossCombat.Profile`) belongs to the registry/roster work; the codex keeps no formula of its own.
- **Start condition**: if `MonsterRegistry` is not on `main`, do not start the codex work; tell the user.

### 4.1 Codex entries (registry-driven)

- Entries: all of `MonsterRegistry.All`, in that order. Do not hard-code a table or a count. Today it holds the existing 25; the routine adds 20 a day (138 at the end).
- Field tabs: split by `definition.field`. The tabs are All, the six fields and Boss; the Boss tab is `boss == true`. Compute each tab's `found/total` from the registry every time.
- Names: the `definition.name` string key (`Loc.T`). Field names: `GameCatalog.FieldNames`. A boss's field chip uses `definition.field` (B02 Ruined Fortress, B03 Dank Cavern). Do not use the mockup's `Rift boss` label.
- Strings: the English for existing monsters, fields and boss skills is already in `Localization/en.txt`.
- Shadow wording: the English for `암흑` is **standardized on Shadow** (the user's decision, matching `암흑 저항`=`Shadow Resistance`). Fix the two en.txt lines that still say Dark:
  - `암흑`=`Dark` → `Shadow`
  - `다음 사격 3회에 암흑 추가 피해.`=`Adds dark damage to the next 3 shots.` → `Adds shadow damage to the next 3 shots.`
- Portraits: `definition.portrait`. The existing 25 point at `Art/TrainingGround/Portraits/{N01..N20,B01..B05}`.
- Skill rows:
  - The existing 25: read from the code definitions in 4.3 (`EnemyCombat.OwnHit`, `BossCombat.Hit`, and so on).
  - New monsters: read from `definition.skills`. The element chip comes from the element in its tags (Physical, Fire, Cold, Lightning, Poison, Shadow); leave the damage cell empty when the multiplier is 0.
- Boss skill icons: for existing bosses, `Resources/Art/Boss/skill-{BossCombat.Definition(attack)}.png`. Show the six attacks of `BossCombat.KitOf(pattern)` in order, with `BOSS_ROAR` (Roar) inserted between phase 1 and phase 2. Skills without an icon (normal enemies, new bosses without art yet) are text only.

### 4.2 Kill record (new save field)

- Add kill counts keyed by ID to `AccountSave`:
  ```csharp
  [Serializable] class MonsterKill { public string id; public int count; }
  [Serializable] class MonsterCodexSave { public List<MonsterKill> kills = new(); }
  ```
  - JsonUtility loads a missing list as empty. Old saves start empty; do not rename existing save keys.
- When to record: in `CombatSimulation.Deal`, at the moment an enemy dies (next to `State.kills++`), get the ID with `MonsterRegistry.IdOf(e)` and increment a per-ID counter on the running `RunState`. Add it to the account in the existing `GameStore` path that commits the run settlement. Do not add a second save path.
  - Ignore a `null` ID and any ID not in the registry.
  - Elites and minions follow the `IdOf` rules.
  - What counts (default): real combat counts, including rifts, tutorial and guide stages, and puzzles. **The training ground does not count.** Any enemy can be spawned there, so counting it would fill the codex at once. Confirm with the user (section 9).
  - Keep the counter in the run snapshot and add it once at settlement, so suspend/resume and restore never count a kill twice.
- An entry is discovered when its ID has `count > 0`. The progress bar's total is `MonsterRegistry.All.Count`, which grows every day.

### 4.3 Estimated stats

- Do not write the formula again. Call `CombatSimulation.EnemyStats(stage, kind, elite, boss, pattern, rift: true, introductory: IntroductoryRift.Applies(stage), tuning)`, where `tuning` is the current live-ops rift setting.
  - For reference, with live-ops multipliers at 1: HP = 70×1.08^(tier−1) × kind multiplier (boss 45×profile) × elite 3; attack = 18×1.055^(tier−1) × kind multiplier (boss 3×profile) × elite 1.5. Tiers 1–5 add the introductory reduction. The mockup numbers follow this formula and match the training ground's tier 10 boss (HP 6,297, attack 87).
- Slider range: 1 to `account.Hero.highestClear + 1`, the same rule as rift entry (`RiftEntry`: `run.stage <= highestClear + 1`). It starts at `highestClear` (1 when that is 0).
- Shown values follow the Diablo IV monster composition (4.4): Life, Attack, Movement Speed, Damage type, and Defenses (Physical Resistance, Non-Physical Resistance, Shield, Crowd control, plus Stagger for bosses).
  - Movement Speed is the same at every tier (`Same at every tier`).
  - Sub lines: `+8% per tier` / `+5.5% per tier`; with Elite `Elite ×3` / `Elite ×1.5`; on tiers 1–5 `Intro rift`.
- Skill damage = attack × the skill multiplier. Each skill gets an element chip (Physical, Fire, Cold, Poison, Shadow).
  - Read multipliers and elements from the code instead of writing them in the UI: `EnemyCombat.OwnHit`, the hazards and death effects in `CombatSimulation.Enemies`, `BossCombat.Hit` and `CombatSimulation.Bosses`. `Element` values: 0 Physical, 1 Fire, 2 Cold, 4 Poison, 5 Shadow.
  - Show damage over time per second. For example, ×0.375 every 0.25 s is ×1.5 per second.
  - Leave the damage cell empty for skills with no multiplier (summons, auras, healing).
  - The mockup's multipliers were checked against the code. Examples: Executioner Hook ×1 Physical, Grave Toll ×1 Shadow, Chorister Focused beam ×1.5 Shadow per second, Devourer Chain explosion ×1.5 Shadow each, Tyrant Triple Eruption ×1.2 Fire each, Roar ×0.8 Shadow.

### 4.4 Diablo IV monster composition

The user asked for the monster composition to follow Diablo IV's actual composition. From research (2026-10, including Maxroll's Season 11 elite affix page), a Diablo IV monster is made of the parts below. Monsters have no numeric armor or per-element resistances; resistances, barriers and immunity come only from elite affixes.

| Diablo IV part | In Diablo IV | In our game today | Shown in the codex |
|---|---|---|---|
| Monster level | Damage dealt and taken is adjusted by the level gap with the hero | Rift tier scaling | Rift tier slider |
| Rank | Normal / Elite (more Life and damage, 2–4 affixes) / Minion (spawns with an Elite, more Life) / Boss | Normal / Elite (Life ×3, attack ×1.5, 1–2 traits) / Boss | Rank chip, Normal/Elite switch |
| Life and damage | From rank and level | `EnemyStats` | Life, Attack |
| Damage types | Physical, Fire, Cold, Lightning, Poison, Shadow | Physical, Fire, Cold, Poison, Shadow (no enemy uses Lightning) | Damage type chips, per-skill element chip |
| Physical / Non-Physical Resistance | Only from the `Physical Resistance` / `Non-Physical Resistance` affixes | None. The elite traits Life link (-20% damage taken) and Volley barrier (-50% from frontal projectiles) play a similar role | 0%; for elites, "Only from traits" |
| Barrier | The `Reprisal` affix occasionally grants a Barrier worth 25% of max Life | None | None |
| Immune / Unstoppable | The `Shielded` (immune for a few seconds) and `Unstoppable` affixes | None | (shown as elite trait rows once built) |
| Boss crowd control | CC does not apply; it fills the stagger gauge, and a full gauge staggers the boss for a while | Immune to root, stun and freeze (slow applies); Stagger 100 → incapacitated 3 s → immune 10 s | Crowd control "Immune", Stagger cell |
| Elite affixes | 30+ (fire, cold, lightning, poison, shadow and generic families) | E01–E07 (below) | "Elite traits" group when Elite is selected |

The closest Diablo IV affix to each of our traits (reference only; never show Diablo IV names in the game):

| Our trait | Closest Diablo IV affix |
|---|---|
| E01 Homing flame | Mortar / Fire Enchanted |
| E02 Ice ring | Frozen |
| E03 Life link | Life Link |
| E04 Volley barrier | Suppressor |
| E05 Corpse blast | (none, ours only) |
| E06 Rage buildup | Berserker / Soul Drinker |
| E07 Gravity Collapse | Tempest |

**First codex build**: implement only the "Shown in the codex" column. Every value is one the game already has. Do not add new combat rules.

**Follow-up candidates (separate steps, after the user approves)**: moving the Diablo IV composition into combat itself needs the items below. Each changes combat balance, so each needs a before/after rift simulation on the same seeds.
1. Raise elite traits from 1–2 to 2–4.
2. A Minion rank: more Life for normal enemies that spawn with an elite. Diablo IV publishes no multiplier, so a design value is needed.
3. New elite traits: Physical Resistance, Non-Physical Resistance, Reprisal (Barrier worth 25% of max Life), Shielded (immune for a few seconds), Unstoppable, and other Diablo IV affixes.
   - Resistance percentages and immunity times are not published for Diablo IV, so design values are needed.
   - Enemy barrier and immunity need new HUD feedback.
4. An enemy that deals Lightning damage. None does today.
5. Boss stagger length: Diablo IV gives at least about 12 s; ours incapacitates for 3 s.
6. Level-gap damage adjustment between hero level and rift tier.

Sources:
- [Maxroll: Elite and Affixes Overview](https://maxroll.gg/d4/resources/elites-affixes) (updated 2025-12-09, Season 11): elites carry 2–4 affixes; minions have more Life; the Physical/Non-Physical Resistance, Reprisal barrier, Shielded and Unstoppable affixes.
- [Fextralife: Stats and Attributes](https://diablo4.wiki.fextralife.com/Stats_and_Attributes): level-gap damage adjustment.
- [PureDiablo: Stagger Meter](https://www.purediablo.com/diablo4/Stagger_Meter), [WePC: How to Stagger Bosses](https://www.wepc.com/gaming/how-to-stagger-bosses-in-diablo-4/): boss CC immunity and the stagger gauge.
- Only normal enemies have the Normal/Elite switch. Bosses show a `Boss grade` chip instead.

## 5. Screens

Common: create `MonsterCodexWindow` (`ContentWindowView`) with `python3 tools/new_content_ui.py MonsterCodex`, and use the shared title, back button, scrolling and safe area. The list and the detail switch inside one window, as in the mockup. Back returns from the detail to the list, and from the list closes the window.

### 5.1 List

- Header: back, the `HELLSCRIPT` wordmark (landscape only), the title `Monster Codex`, and on the right a `Discovered` progress bar with `18 / 25`. In portrait, hide the wordmark and shorten the bar.
- Field tabs: All, the six fields, Boss. Each tab shows `found/total`. Tabs scroll horizontally when they overflow.
- Grid: 8 columns in landscape, 4 in portrait. The entry count grows daily up to 138, so the grid scrolls vertically and must cope with that many cards (reuse the shared scroll parts, e.g. a recycled list or skipping off-screen cards). Each card has a square portrait with the codex ID at the bottom right. Bosses get a `Boss` band at the top left and a gold border. Below the portrait: name and `Killed N`.
- Undiscovered card: a disabled button showing `?` instead of the portrait, `???` as the name and `Undiscovered`. Tapping it does nothing.
- At the user's request, leave out the "Any enemy you have killed is recorded" hint.

### 5.2 Detail

- Landscape: on the left, a 230 px portrait panel with a rank chip (Normal/Elite/Boss), a kill chip, ID, name, and field and role chips. On the right:
  - one row of four tiles: Life, Attack, Movement Speed, Damage type chips
  - one Defenses row: Physical Resistance, Non-Physical Resistance, Shield, Crowd control, plus a Stagger cell for bosses
  - a scrolling skill list. Switching to Elite adds an `Elite traits · 1–2 at random` group (E01–E06) at the top.
  - Fixed bottom bar: the `Rift tier` label with `Best clear: tier N`, `−`, the slider (with a tick at the best clear), `+`, the large tier number with a status line (`Cleared` / `Next challenge` / `Intro rift reduction`), and the Normal/Elite switch.
- Portrait: the portrait panel spans the full width at the top (about 260 high), the stats and the defenses each form a 2×2 grid (three rows of defenses for bosses), and the skill list scrolls in the remaining height. The bottom bar has two rows: label, tier number and grade on the first; `−`, slider and `+` on the second.
- Skill row:
  - Bosses only: a 44 px icon with a gold border for phase 1 and a red border for phase 2 and Roar.
  - Name and a tag chip (Phase 1, Phase 2, Phase shift, On death, Weak spot, Healing, Aura, Poison, Fire, Cold, Slowed, 2 hits).
  - Description.
  - On the right: damage (`Damage`, `Damage per sec`, `Damage each`, `Damage every 0.5 s`) and `Attack ×mult`.
- Touch targets are at least 44 px. The slider supports both dragging and `−`/`+`.

## 6. Text and localization

- Put every string through the string table keys (`Loc.T`/`Loc.F`) and fill the English in `en.txt`, following the mockup's English.
  - New keys, for example:
    - 몬스터 도감=Monster Codex, 발견한 적=Discovered, 미발견=Undiscovered, 처치 {0}=Killed {0}
    - 생명력=Life, 피해 유형=Damage type, 방어 구성=Defenses, 물리 저항=Physical Resistance, 군중 제어=Crowd control, 모두 적용=All apply, 면역=Immune, 제압=Stagger, 정예 속성=Elite traits
    - 피해 감소 없음=No damage reduction, 단계와 무관=Same at every tier, 사용 기술=Skills
    - 최고 클리어 {0}단계=Best clear: tier {0}, 다음 도전 단계=Next challenge, 클리어한 단계=Cleared, 입문 균열 보정 적용=Intro rift reduction
    - 보스 등급=Boss grade, 1페이즈=Phase 1, 2페이즈=Phase 2, 페이즈 전환=Phase shift, 사망 시=On death
    - 근접 추적=Melee chaser, 사망 폭발=Death burst
  - Element keys exist too: 물리=Physical, 화염=Fire, 냉기=Cold, 독=Poison, 비물리 저항=Non-Physical Resistance, 보스 제압=Boss staggered. The English for `암흑` is **standardized on Shadow** (the user's decision, matching `암흑 저항`=`Shadow Resistance`). Fix the two en.txt lines that still say Dark: `암흑`=`Dark`→`Shadow`, and `다음 사격 3회에 암흑 추가 피해.`=`Adds dark damage to the next 3 shots.`→`Adds shadow damage to the next 3 shots.`
  - Existing keys: 균열 단계=Rift tier, 정예=Elite, 일반=Normal, 보스=Boss, 공격력=Attack power, 이동 속도=Movement Speed, 원거리=Ranged, 장판=Zone, 지원=Support, and all monster, field and boss skill names.
  - The skill names and descriptions of normal enemies and the boss skill descriptions are new text. Use the mockup's Korean and English as written.
- Key template trap: keys shaped like `{0}/{1}` have swallowed saved combat lines before. Give new keys names that do not collide with existing ones.

## 7. Verification

- EditMode:
  - Kills are recorded per kind: elites under their base kind, bosses by pattern, minions under their own kind.
  - The training ground is excluded, and suspend/restore never counts a kill twice.
  - Loading an old save gives an empty kill list.
  - Detail values equal `EnemyStats` at tiers 1, 5, 6 and best+1, and for elites.
  - Each skill's element and multiplier match the code (`EnemyCombat.OwnHit`, `BossCombat.Hit`, hazards and death effects).
  - Boss defenses match the real rules: immune to root, stun and freeze, slow applies, Stagger 100 → 3 s → immune 10 s.
  - The slider range is 1 to highestClear+1, and undiscovered entries do not open.
  - Iterate the registry (never a hard-coded count): every entry has Korean and English names, a portrait and `EnemyStats` values. Existing bosses' skill icons load too.
  - Kills of an ID not in the registry, or where `IdOf` is `null`, are not saved.
  - With a test registry that has one extra entry, the list, tab counts and progress-bar total follow automatically.
  - No Korean or English string shows as empty or as a key name.
- UI contract: `python3 tools/check_ui_contract.py --verify`.
- Native check (macOS development build smoke): capture these at portrait 440×956, landscape 956×440 and PC 16:9/16:10/21:9, in Korean and English. Check for clipping, overlap and touch target size. Do not run text-size tests.
  - The dock icon position (below the Hunt Edict)
  - The list
  - A normal enemy detail
  - A boss detail
- Run the full EditMode suite and the runtime smokes once, after the last code change and the `main` integration. Follow the wiki publishing procedure at merge.

## 8. Suggested steps

0. Check that `MonsterRegistry` is on `main` (4.0). If not, stop and tell the user.
1. Save field, kill recording and tests.
2. `MonsterCodexWindow` list and detail (landscape) and the string keys.
3. Portrait layout, boss skill icons and the Roar row.
4. Dock icon resource and the `ContentDock` entry point.
5. Native captures, UI contract, documentation and wiki.

## 9. Open questions for the user

1. Whether to take on the follow-up candidates in 4.4 (moving the Diablo IV composition into combat rules), and if so which first. The first codex build goes ahead either way.
2. Should training ground kills count (default: no)?
3. Should the codex icon also show in the prologue and tutorial screens (default: yes, like the Hunt Edict, per the screenshot)?

Decided: the dock icon is variant A; the English for `암흑` is Shadow; the codex list is driven by `MonsterRegistry` (requested by the roster session, 2026-10-11).
