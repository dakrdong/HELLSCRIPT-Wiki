# Guide Tutorial Gear Recommendation Lesson — Implementation Plan (for Codex)

Updated: 2026-10-13

Author: Claude (plan only). Codex implements, verifies and commits. When done, turn this document into the result record together with the Korean version.

[한국어](Guide_Tutorial_Segment4_Plan_20261013.md)

> **Status (2026-10-19): implemented.** Phase numbers, the fixed loot and the card flow differ from this plan in places, so the result is recorded in [the gear lessons of the real-rules rework](Guide_Tutorial_Real_Rules_20261019.en.md). This document stays as the plan of record.

## 0. Prerequisites and baseline

- Baseline code: **`origin/main` with segments 2 and 3** (PR #101, after `a5596519`). Phase constants in `GuideTutorial.cs`: `Entrance=0 … FinalBoss=24, Cleared=25, Last=Cleared`.
- **The gear offer cards (design v2) must reach `main` first.** The design is the Claude artifact "균열 장비 추천 UI" (`https://claude.ai/artifact/1fVkuNDqhdWyHN7ZFmFMmp`). Its plan is `Docs/Implementation/Rift_Gear_Offer_Cards_Plan_20261013.md`, and its implementation is on branches `claude/rift-gear-offer-actions` / `codex/rift-gear-offer-cards`. Start from a `main` that has that work merged. The UI names below come from that branch:
  - card `rift-gear-card-{id}`, icon `rift-gear-icon-{id}`, magnifier `rift-gear-inspect-{id}`, equip `rift-gear-quick-equip-{id}`, close `rift-gear-dismiss-{id}`, cog `rift-gear-settings-{id}`;
  - settings window: recommendation rule `rift-gear-rule-all` / `rift-gear-rule-better`, save `rift-gear-settings-save`, close `rift-gear-settings-close`;
  - detail window: `RiftRecommendationWindow` (`compareFirst`), with `Equip` and `Close` at the bottom.
  - If a name has changed, use the real one.
- The common rules of the segment 1–3 plans still apply:
  - Velina speaks politely to the player and never explains the setting (blindfold, stopped time).
  - The gate (`PrologueGate`) points at one control at a time.
  - Progress is judged from the saved state every frame.
  - Every text goes into `en.txt` in Korean and English.
  - Work in a separate worktree from the latest `origin/main`.

## 1. Goal

Teach the **gear offer cards** inside segment 3's treasure goblin scene and after the boss.

1. Goblin reward: gold (five piles of random amounts) + enhancement stones (three piles: 3, 3, 4) + **a magic helmet (fixed)**. Picking up the helmet shows a card. The player taps the icon to open the details, then equips the helmet.
2. Boss reward: **a rare sword (fixed) + a magic sword (fixed)**. The player taps `Equip` on the rare sword card. The lower-scoring (−) magic sword card follows. The player opens the card settings and sets the recommendation rule to `Gear with a higher score than what is worn`.
3. Then the existing closing lines and the graduation to town.

## 2. Tutorial loot (gear fixed down to the options; only the gold amounts are random)

**Gear** is never left to the random generator (`ItemGenerator` / `DropFrom` rolls); only the gold pile amounts are random. Drop **fixed items built from explicit fields**. Choose values that pass the existing item validation (`ValidateItems`: slot, grade, option ranges), and use only option ids that exist in the game. Start from the defaults below and adjust them to the valid ranges.

| Source | Content | Default (proposal) | Must hold |
| --- | --- | --- | --- |
| Treasure goblin | **Five gold piles** | A **random amount** per pile (proposal: 200–400 each, 1,000–2,000 in total) | Drop each pile separately with `RiftResources.Add(Gold)`. Scatter them within about 1–1.5 m of the drop point (the detour spot behind the hero), all on walkable ground (`Map.CanLand`). Roll the amounts with a seeded run stream (such as `State.goblin.rng`). **Only the gold is random**; the gear is fixed |
| Treasure goblin | **Three enhancement stone piles** | **3 · 3 · 4** (fixed, 10 in total) | Drop each pile separately with `RiftResources.Add(EnhancementStone)`, scattered around the same spot as the gold, all on walkable ground |
| Treasure goblin | **Helmet · magic (rarity 1)** | Item level = hero level; base armor + two magic options (for example max life +, armor %) | The tutorial warrior **must not be wearing a helmet** at this point (the line says "You had no helmet"). Unequip a starting helmet at the start if there is one |
| Boss | **Sword · rare (rarity 2)** | Three options, offence-weighted | Gear score (`EquipmentScore.Value`) **clearly higher than the weapon worn** (the card shows `+gain`) |
| Boss | **Sword · magic (rarity 1)** | Two options | Score **lower than the rare sword** (after the rare sword is worn, the card's gain is negative) |

- **Where it lives:** a fixed table in `GuideTutorial` (or a dedicated static class) and one builder function. It must not depend on random seeds or generator output.
- **Item ids:** unique in the account and identical when rebuilt. For example `{heroId}/guide/helmet`, `{heroId}/guide/rare-sword`, `{heroId}/guide/magic-sword`.
- **No duplicates on restart:** an interrupted tutorial restarts from the beginning (`StartGuideTutorial`). Do not drop an item again if its id is already in the hero's inventory (or reuse the existing one). Otherwise the hero ends up with two copies.
- **The reward is real.** Pickup goes through the same claim-and-save path as rifts; open it for tutorial runs if it is blocked there. The items stay in the inventory after graduation.
- Pin the fixed gear in a **golden test** (slot, grade, option ids, values, score relations) so a generator change is caught.

## 3. Flow and lines

Segment 3 today: `GoblinChase → ReturnToBoss → LootTalk (pickup lesson) → Pickup → LootDoneTalk → FinalBoss → Cleared`. Insert the new phases into it. Names are proposals: number them before `Cleared`/`Last` and add the talk phases to `Waiting()`.

| Phase | Situation | Time | Content |
| --- | --- | --- | --- |
| `Pickup` (existing) | The hero picks up the goblin reward (pickup first) | Running | **When the helmet is picked up and its card first shows** → `HelmetTalk` (the remaining pickups continue after the lesson) |
| `HelmetTalk` | The helmet card just appeared | Stopped | ① New gear dropped from the treasure goblin! When you get gear you can wear, a recommendation pops up like this so you can put it on right away. → [Take a look] |
| (Lesson H) | Ring on the card → tap the **icon** `rift-gear-icon-{helmet}` → the detail window opens | Stopped | Hint: Tap the recommended gear's icon to take a closer look. When the details open: ② Great! You had no helmet until now, so this will shore up your defense! Then point at the detail window's `Equip` (decision 1 in §7): Tap 'Equip' to put on the helmet. |
| (end of H) | Helmet worn | — | `ContinueTutorial()` → back to `Pickup` for the rest (gold, stones) |
| `LootDoneTalk` (existing) | All goblin loot picked up | Stopped | Existing line (shall we go back and defeat the boss?) |
| `FinalBoss` (existing) | The boss dies | Running | **Do not `Finish` on the boss's death.** Drop the rare and the magic sword at the boss and go to `BossLoot` |
| `BossLoot` | The hero picks up both swords (pickup first, combat over) | Running | **When the rare sword card shows** → `RareTalk`. The magic sword card shares the slot and waits behind it (design rule: one card per slot) |
| `RareTalk` | Rare sword card | Stopped | ③ Look at this! You got rare gear! ④ Shall we swap gear quickly with the recommendation again? → [Swap it] |
| (Lesson R) | The card's `Equip` `rift-gear-quick-equip-{rare}` | Stopped | Hint: Tap 'Equip'. Once it is worn, the waiting **magic sword card** appears |
| `MagicTalk` | Magic sword card (negative gain) | Stopped | ⑤ Huh? This time a lower-grade sword is showing. Put a ring on the card's **score pill (negative gain)**. ⑥ There is no need to check items lower than your current gear like this. → [Change the settings] |
| (Lesson S) | Card cog `rift-gear-settings-{magic}` → settings window → rule `rift-gear-rule-better` → `rift-gear-settings-save` | Stopped | Hints: Tap the gear icon on the card. / Choose 'Gear with a higher score than what is worn'. / Tap 'Save'. After saving: ⑦ This way, only gear better than what you have will be recommended. |
| (end of S) | The magic sword card remains | — | Waiting cards stay when settings change (design rule 5). Hint: Tap × to close the card. When it closes → `Cleared` (decision 2 in §7) |
| `Cleared` (existing) | Closing lines → graduation | Stopped | Unchanged. Move `Finish(true, …)` here |

Korean source lines are in the Korean plan; keep both languages in `en.txt`. Choices: 살펴본다 / Take a look · 바꿔 본다 / Swap it · 설정을 바꾼다 / Change the settings.

## 4. Work by file

### Gear offer side (`RiftGearRecommendations.cs`, `GameUI.GearRecommendations.cs`, `GameStore.RecommendedEquipment.cs`)
- `RiftGearRecommendations.Visible` returns nothing when `run.training>=0||run.tutorial`. **Allow guide tutorial runs (`GuideTutorial.Running(run)`, not a replay).** Tutorial runs have `training=3`, so order the conditions carefully.
- Check that the card visibility rules (battle page, hidden on portal/town) hold on the tutorial battle page.
- Check that the pickup-time decision (`drop.gearRecommended = CanOffer(...)`, once at acquisition) is recorded for tutorial pickups too.
- Check that the card's `Equip`, `Close` and settings-save transactions are not refused on a tutorial run (rifts probably key on `suspendedRun`). If they are, allow guide tutorial runs the way `CommitHuntEdict` does with its "prologue" test (`Data.guide.tutorialRun==simulation.State`). After each transaction, `Data.guide.tutorialRun` must still point at the live run (segment 1 trap).
- **Name the score pill if it has no name** (for example `rift-gear-score-{id}`); lesson S highlights it.
- Name the detail window's `Equip` too if needed (for example `rift-gear-detail-equip`).

### Tutorial side (`GuideTutorial.cs`, `CombatSimulation.Guide.cs`, `CombatSimulation.Goblin.cs`, `GameStore.Guide.cs`)
- Add the phase constants and move `Cleared`/`Last` to the end. Add `HelmetTalk`, `RareTalk` and `MagicTalk` to `Waiting()`. Check that `Tutorials.NormalizeShare` checks the range against `GuideTutorial.Last` (segment 3 should already do so).
- **Start settings:** at the tutorial start (near `StartGuideTutorial` / `BarePowers`), set the hero's offer settings `riftGearOffer.allWearable=true` (all equippable gear). With the default `false` (better only), the magic sword fails the acquisition-time check and lesson S cannot happen. Turn on every grade and range (`gradeMask` 31, `rangeMask` 511). This lives in `HeroSave.riftGearOffer`, not the edict; check that it is in `GameStore.Transact`'s per-hero copy list (card plan §5).
- `RewardGoldenGoblin`, `IsGuide` branch: today gold + one random rare item. Change it to **five gold piles (random amounts), three enhancement stone piles (3, 3, 4) and the fixed helmet**. Keep the drop-point logic (behind the hero, requiring a detour).
- `FinalBoss`: on the boss's death do not call `Finish`. Drop the two fixed swords at reachable spots near the boss and go to `BossLoot`. Place the **rare sword nearer the hero so it is picked up first** (pickup order decides which card shows first).
- Phase changes:
  - `Pickup`: helmet card first appears in `Visible` → `HelmetTalk`.
  - `HelmetTalk` continues when the helmet is worn.
  - `BossLoot`: rare sword card visible → `RareTalk`.
  - `RareTalk` continues when the rare sword is worn. Then, when the magic sword card shows, go to `MagicTalk` within the same stop.
  - `MagicTalk` continues when `riftGearOffer.allWearable==false` is saved and decision 2 is done.
  - Many of these are judged from store and UI state (card list, worn gear, settings), not the simulation. Put the predicates in `GuideTutorial` so the UI and the tests share them.
- Keep segment 3's rules (hero protection, the boss holding still from the goblin to the pickup). After the boss dies no enemy may remain (clear summons).

### UI (`GameUI.GuideTutorial.cs`)
- Add the lesson kinds `helmet`, `rare` and `settings`, and pick gate targets in the order above. The card strip, detail window and settings window are different canvases: find targets with the existing `Named(root,name)` under each root (the card strip, `RiftRecommendationWindow`, `RiftGearSettingsWindow`).
- In lesson H, tapping the magnifier instead of the icon opens the same detail window; accept both (design: the icon body opens the details too).
- The score highlight (⑤, ⑥) has nothing to press. Reuse segment 3's "card without a target, tap to continue" and put a `TutorialAnchorRing` on the score pill.
- Check that the card timer stops while the settings window is open (card plan: implemented). The tutorial keeps the popup duration at `Until I press cancel` (the default) so no card expires mid-lesson.
- Add objectives (`GuideObjective`) for the new phases. For example: Goal · Look at the recommended gear / Goal · Swap to the rare gear / Goal · Change the recommendation settings.

## 5. Tests and verification

Focused checks while working; one full Edit Mode and macOS smoke after the last code change and the `main` integration (`CLAUDE.md`).

- Golden test (fixed gear): the three items' slot, grade, option ids and values equal the fixed values. Score relations: rare sword > worn weapon, magic sword < rare sword. The helmet is equippable when no helmet is worn. Building twice for the same hero gives the same ids and values, and nothing drops again if the item is already in the inventory.
- Extend `GuideTutorialTests`:
  - Goblin defeated → five gold piles (each amount within range, at distinct spots), three enhancement stone piles (3, 3, 4) and the helmet on the ground. `LootDoneTalk` waits until every gold and stone pile is picked up.
  - After the pickup-first change, picking up the helmet shows its card and stops at `HelmetTalk`. Continue is refused until it is worn; afterwards the rest is picked up and `LootDoneTalk` follows.
  - Boss defeated → both swords appear without `Finish`. Rare sword card → equip → the magic sword card shows with a negative gain (`RiftGearRecommendations.Offer(...).delta`).
  - Continue is refused until "better only" is saved. Afterwards `Cleared` → graduation. After graduation the three items are in the inventory, the helmet and the rare sword are worn, and gold and stones have grown.
- Gear offer tests (`RecommendedEquipmentTests` etc.): `Visible` returns cards on a guide tutorial run and still returns none for ordinary training and replay runs.
- `python3 tools/check_ui_contract.py --verify`, `python3 tools/check_ui_refresh.py`.
- Extend `RuntimeGuideTutorialSmoke` through these phases with real pointers. Capture and review: the helmet card highlight, the detail window, the rare sword card's `+gain`, the highlighted `−gain` on the magic sword card, and the rule choice in the settings window. At least 440×956 Korean and 956×440 English. In landscape, confirm the cards are at 75% size.

## 6. Do not

- Do not roll tutorial **gear** randomly (the same items with the same options every time). Only the gold pile amounts are random.
- Do not build new card, detail or settings windows; use the design v2 implementation.
- Do not change the offer rules outside the tutorial (acquisition-time check, one card per slot, default "better only"). Change only the tutorial's starting values.
- Do not put setting explanations (blindfold, stopped time) or the 'use skill' approach in the lines.

## 7. Confirmed decisions (user, 2026-10-13)

1. **Lesson H equips the helmet.** After the detail window opens and the line plays, point at the detail window's `Equip`.
2. **The player closes the remaining magic sword card.** After line ⑦, point at the card's × (`rift-gear-dismiss-{magic}`); when it closes, go to `Cleared`.
3. **Loot amounts follow the §2 table.** Gold is five piles of random amounts and the stones three piles of 3, 3 and 4 (both user requests, 2026-10-13). Option values stay within the valid ranges and must keep the score relations (rare sword > worn weapon, magic sword < rare sword).
