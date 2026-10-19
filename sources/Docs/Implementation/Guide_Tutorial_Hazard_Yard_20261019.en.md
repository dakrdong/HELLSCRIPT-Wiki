# Guide tutorial rework — ground-zone and arrow dodging, level-up fireworks, the boss and Whirlwind (2026-10-19)

[한국어](Guide_Tutorial_Hazard_Yard_20261019.md). The Edict's Road (flow 7) that every new account plays was reworked. Requested: after the Spinning Rend lesson teach lingering-zone dodging and projectile dodging, make the level-up flashy, move the boss further back and teach Whirlwind where he is met, and review all lines and staging.

## The opening (`Awakening`)

A new account used to start walking without a word. The new flow-7 run starts held on `Awakening` (phase 45, a waiting beat) and only the end of the opening hands it to the walk (`Entrance`).

1. A black screen, a heartbeat, the hero on his knees. Three cards of the world's story pass: "The world was torn apart / Monsters crawled out of every crack" → "They called the cracks Rifts / A voice showed the way to every hero who stood before a Rift" → "That voice is you / The edict you write becomes the hero's path" (it casts the player as the voice that writes the edict, which Velina's guidance then continues). A skip button is available.
2. It brightens over 2.4 s while the view leans toward the hero, and his three lines of thought play (my head is ringing / I left the town gate and this place is on no map / the stone is cold and someone is watching). On the last line divine light descends and a golden edge spreads. Velina does not speak yet (she addresses the player politely, never the hero).
3. `Rise to your feet` makes the hero stand, the light lifts with a chime, the `Tutorial · The Edict's Road` card rises and the walk begins, followed by Velina's existing greeting.

Quitting mid-opening replays the opening from its start. Old checkpoints are unaffected.

## New order (from the plaza on)

| Beat | What happens | Saved setting |
| --- | --- | --- |
| `Road3` → `DotFight` | Reaching the plaza entrance brings out one circular-zone mage in a puff of smoke. The hero does not dodge zones yet and fights inside them. | none |
| `DotTalk` | After two pulses of zone damage (7% per second) Velina appears and explains the danger, then has the player set Hunt Edict → Survival → Avoidance by damage type → lingering-zone dodge to **1-7%** (the gate points at each button). | `dodge.ground` 1-7% |
| `DotDodge` | The hero steps aside whenever a zone is announced and defeats the mage. | |
| `ShotFight` / `ShotTalk` | Three archers appear. One arrow = 5% of max HP. After the first hit Velina has the player set projectile dodge to **1-5%**. | `dodge.direct` 1-5% |
| `ShotDodge` → `LevelUp` | When every archer is down the hero reaches level 3 (full HP/MP, +1 skill point) with a **long fireworks show** (5 s, with a confirm button). | |
| `LevelTalk` → `Road4` | After Velina's words the road leads to the new boss room, stopping just before the boss's pack notices the hero (15 m). | |
| `BossTalk` | The camera frames the boss and his title card appears. Velina introduces Whirlwind (the Warrior's main skill: it spins and keeps cutting everything around, the most efficient and fastest way to clear monsters). The player spends the point on it and equips it in the **second slot** (Raking Wound stays in the first). | Whirlwind learned and equipped |
| `MixedFight` → as before | Everything after is the existing flow (cooperation → priorities, treasure goblin, gathering, boss, town). | |

The position lessons (`OpeningTalk`, `CombatTalk`) stay in the gallery before the plaza. The boss's pack (tank, two mages, archer, boss) moved to a new **fifth room** (`RM06`, centre (122,50), joined to the plaza's east door by a corridor). The hero walks the road and meets the boss far later than before.

## Numbers and why

- **Damage**: a zone pulse is half of the calibrated HP share and it pulses twice a second, so the zone is exactly **7.02%**; an arrow is **5.02%**. The values are found by bisection on `IncomingDamageNumbers`, the function both the forecast and `Hurt` use, so armour and resistance do not change the lesson. The extra 0.02 makes a saved threshold equal to 7% (or 5%) dodge under the `>=` comparison.
- **Starting state**: the hero starts with ground, area and projectile dodging `OFF` (slider 0%, "does not avoid"); the lessons raise it to 1-7% and 1-5%. The edict order is now `DODGE,ATTACK,…` (dodge first). With `ATTACK,DODGE` a melee hero can always attack, so a saved threshold never moves him. The gathering lesson still moves `PRIORITY_LOOT` to the top and dodge stays above attack.
- **Foes fight by the ordinary rules**: the first version of this document pinned the mage and archers in place, shortened their casting rhythm and zone time, and gave them HP floors so they could not die before their lesson. The second pass of 2026-10-19 removed all of it. Combat (enemy movement, casting rhythm and damage; the hero's movement and attacks) follows the real rules in the tutorial too, and the tutorial decides only the order, the guidance and the Hunt Edict settings. [Real-rules rework](Guide_Tutorial_Real_Rules_20261019.en.md).
- **Saves**: new phases are appended (35-44) and `Last` was raised. A checkpoint from the four-room map has a different layout fingerprint (`tutorial-v7r2`), so `Tutorials.Normalize` quietly drops it and the stage restarts instead of walking into rooms that do not exist. Flow 6 (old checkpoints) is untouched.

## Level-up celebration

`LevelUpCelebration` (an overlay layer): white flash, a turning 16-ray sunburst, a ribbon banner popping in with `LEVEL UP!` and `Lv.N` (overshoot), confetti cannons from both lower corners and several fireworks across the upper screen (burst + sparkles + confetti), and for the tutorial's big level a confetti rain until the end. Every level-up lasts **5 seconds**: ordinary ones have 6 bursts and 2 cannon volleys, the tutorial's plaza level-up 11 bursts, 3 volleys and the confetti rain. A **confirm button** below the centre counts the five seconds down (`OK (5)` and a shrinking gold bar): pressing it closes the show within 0.3 s, and when the time is up it closes itself. Every tap outside the button passes through. In the tutorial the level-up words follow the moment the show ends, even when it is closed early. With reduce-motion on, only the banner fades. It is called from `RefreshGrowthHud` (only a level gained within the last 3 s is staged; restored history is just a note). The existing gold ring and light pillar stay in the world.

Five images came from Codex (`gpt-6-astra` requested, model `unknown`): ribbon banner (empty blue plate), sunburst, sparkle 2×2, confetti 4×4 (16 pieces), firework 2×2 (peony, ring, willow, heart). All came back with real alpha on the first try (edge alpha ≤ 7/255) and none was redrawn. Record: [`Docs/Art/LevelUpCelebration20261019`](../Art/LevelUpCelebration20261019/manifest.json). Runtime files live in `Resources/Art/LevelUp/` (`LevelUpArtImporter`, budget 1024 ASTC 6x6); the native 1254² sources are outside the repository in `~/HellscriptBackups/levelup-celebration-20261019/native/`.

## Line and staging review

- The introduction never said that **the hero follows the Hunt Edict word for word**, although the player edited it all the way and only the final `Cleared` beat said "this is the Hunt Edict". Two sentences were added to the introduction.
- The road line "Did you see? The hero moved just as you taught" played right after the skill and potion lessons and made no sense (flow 7). Flow 7 now says "I sense more enemies up ahead…"; the closing scene keeps the original.
- Every new enemy now comes with a **notice** ("New enemy · …") and smoke, a ring and sparks at its feet (swarm, surround, zone mage, archers). Each finished lesson shows a `Completed` notice (e.g. `Impaling Hook learned`, `Ground-zone dodge set`, `Combat position set`).
- Zone and arrow lines read the saved state to say why the hero is being hit right now. The screen edge turns red while a hazard is explained. The boss is introduced with a card and the camera.

## Verification

- Edit Mode (affected areas): guide tutorial, goblin flight, potion, modifier, new `GuideHazardYardTests`, localization, stored localization, growth presentation, edict survival menu, tutorial account reset, hero slots, world FX and town model tests, skill juice and hook presentation tests, **247 passed** (after the opening and the 5-second confirm-button show). The new tests play the real simulation from the start to graduation and check the 7.02% and 5.02% forecasts, that thresholds of 8% and 6% do not dodge, the boss room positions, the one point after the level-up, that an old checkpoint is dropped, that a new run holds on the opening, and the 5-second show with its confirm button and auto-close. Full Edit Mode was not run. The UI contract check (`check_ui_contract.py --verify`) passed 15 tests.
- macOS development player smoke (`RuntimeGuideTutorialSmoke`, Korean 440×956 portrait and English 956×440 landscape): passes through the opening cards, the hero's thoughts, the zone and arrow lessons, pressing the level-up confirm button, the boss and the Whirlwind lesson up to the town arrival. Only the last town HUD probe (pointer blocked at `Hunt Edict`) fails in both languages, and the same failure appears on unchanged `main` (unrelated). The smoke screenshots also showed the opening's story cards hidden under the black fade; `TutorialCinematic` now puts the fade below the cards.
- Not verified: physical device, human comprehension and ordinary-speed feel, the actual sound of the fireworks (existing `loot.drop.rare` and `flow.level_up` are reused); no new sound effects were made.
