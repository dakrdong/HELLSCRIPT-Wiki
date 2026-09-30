# Tutorial progression and content guidance

Updated: 2026-09-29

This design implements the approved flow: the *Voice of the Edict* prologue on a dedicated map → learn and equip the first skill and choose its preset through the Hunt Edict sent from the sky → change the automatic potion and avoidance settings while fighting the gatekeeper → 1,000 gold for defeating it → town → rifts → Hunt Edict editing → new skill equipment → guidance at each content unlock.

## Fixed decisions

- Only the dedicated map is mandatory, once per account. Other guidance supports read, defer, hide and reopen independently of practice.
- Both Hunt Edict setups on the mandatory map are forced steps where only the named control can be pressed. The next control is derived from the real saved state.
- Finishing the first map pays a one-time 1,000 gold completion reward. The reward and the account's completion are saved in one transaction, and no equipment is granted. Apart from that reward, tutorial combat grants no XP, gold, random loot, fatigue, rift records, first clears or offline baseline.
- Other characters use per-hero core guidance. Replay uses a separate level 1 account copy and transfers no rewards.
- First-practice support is account-wide, ten kinds, one use each. Support is added only inside the staged execution transaction and committed with the result. Cancelling, invalid selection or failed disk saving retains the entitlement.
- Normal domain prerequisites, prices, probabilities, capacities, protection and NPC proximity remain enforced. No prerequisite legendary, aspect, level, enhancement or sweep attempts are invented.
- Existing accounts are exempt from the mandatory map. Old read/skip acknowledgements migrate as read only; semantically equivalent actual first-play facts are preserved. Existing account guidance is contextual.

## Dedicated map

Step names are the values of the run state `ProloguePhase`.

| Step | Trigger and player action | Completion / recovery |
| --- | --- | --- |
| Preparation | Start the first account character. | The skill allocation is reset (one point at level 1, empty slots), the class's first skill (W01, A01 or M01) gets its starting preset, and the result is saved. Map owner and run are saved before launch. |
| `Surrounded` | The hero kneels in despair, ringed by six monsters. The voice from above commands and sends down the Hunt Edict scroll. | The run is paused. Once the edict emblem lands in its menu slot, the next step begins. |
| `EdictLesson` | Forced steps: open Hunt Edict, spend the one point on the first skill, equip and save it, then tour the preset tabs and activate the chosen preset. | Advances only once the equipped first skill and the chosen preset are really saved and the window is closed. |
| `Encircled` | Fight the ring with actual skills. | Enemy deaths are simulation outcomes; no manufactured victory. Advances once all six are defeated. |
| `Gatekeeper` | After a short dialogue, travel automatically to the boss room and fight the gatekeeper. | The original combat engine spawns the gatekeeper. Before the voice intervenes, its health cannot fall below 35%. Advances when the hero drops below 50% HP. |
| `SurvivalLesson` | Time stops and the voice intervenes. Forced steps: set Automatic potion to *Extra survival supplies* and Avoidance by damage type to *Avoid all warnings*, then save. | The changes are saved to the owned hero's Hunt Edict. Advances once both are saved and the window is closed. |
| `Showdown` | Keep fighting the gatekeeper with the new settings. | Completes on the gatekeeper's actual death. |
| `Cleared` | After the victory staging, press *Take the reward and go to the village*. | 1,000 gold, account completion and run removal are saved in one transaction. A repeated completion request pays the reward only once. |
| Town arrival | Anton Jindark, the rift keeper, tells the village's story, how to walk and where the first rift is. | The conversation plays on the first completion only. |

A defeat in any fight restores the hero on the spot. Living common monsters recover their health too, but the gatekeeper keeps the damage it has taken. Replay uses a level 1 account copy: setting changes apply to that copy only, and it pays no reward. A checkpoint from the old armor road (`tutorial-v1`) means different steps, so it is dropped and the prologue starts over. Implementation and validation: [Prologue — the Voice of the Edict](../Implementation/Prologue_Edict_Voice.en.md).

### Staging

Since 2026-09-29 the mandatory map is a prologue in which the voice from above speaks to the hero. The voice is drawn as a god whose face is hidden behind a halo; Anton Jindark, the rift keeper, first appears on arrival in town.

- `Surrounded`: a fade-in from black, cinematic bars and the *Prologue · The Voice of the Edict* title card. During the hero's lines the camera frames the monsters of the ring. A shaft of light falls and the voice speaks; the scroll descends, the hero raises it with both arms, and the Hunt Edict emblem flies to its menu slot.
- `EdictLesson` and `SurvivalLesson`: a gate that leaves only the named control uncovered and dims the rest of the screen, a breathing border on that control and the voice's instruction.
- `Encircled`: a new-objective notice, the kill count on the battle screen and a short line from the voice at the first kill.
- `Gatekeeper`: an objective-completed notice and the entrance when the gatekeeper comes on screen (pause, camera, roar, name card).
- `SurvivalLesson` intervention: a red edge, cinematic bars and the light shaft falling again.
- `Showdown` and `Cleared`: a line from the voice when the gatekeeper is at or below 30% health, the *Victory · Gatekeeper defeated* card and the 1,000 gold reward notice.
- Defeat: a red edge, the *You have fallen* card and the voice telling the hero to rise.
- Town arrival: after a fade, the *Ashwood · Settlers' village* place card and Anton Jindark's first conversation.

Staging never changes progression or saving. A staged scene can be ended at once by tapping or skipping. The two forced steps are not staging: they advance only when the named control is pressed. The dialogue box and the old armor road's staging are recorded in [tutorial staging](../Implementation/Tutorial_Staging.en.md).

## All 39 guide groups

Guide names and texts are the English values the game shows for `Tutorials.All`, in the same order.

| ID | Guide | Player-facing design | First-practice support |
| --- | --- | --- | --- |
| P00 | The Voice of the Edict | Surrounded by monsters, you receive the Hunt Edict from the sky. Learn and equip your first skill, choose its preset, then change the auto potion and evasion settings while fighting the gatekeeper. Defeat it to earn a reward and reach the sanctuary village. | None |
| F01 | Town movement and residents | Tap a destination or use the movement pad / WASD. Interact near the rift keeper to prepare your run. | None |
| F02 | Prepare your first rift | Review equipment, skills, potions and stage. Normal admission uses 1x speed; fatigue continues during a paused rift. Enter when ready. | None |
| F03 | Explore a rift | Travel, attacks and collection are automatic. Watch the kill meter, the time limit fixed on entry and the map. Complete the objective to face the boss. | None |
| F04 | Results and first-clear rewards | Review success or failure and the final actions. Collected rewards remain yours. Claim first-clear boxes separately in the rift entry screen. | None |
| F05 | Change one Hunt Edict setting | Change the potion HP threshold under Survival in Hunt Edict and save it. Opening an explanation or cancelling does not complete the practice. | None |
| F06 | Equip a new skill | Besides your first skill, equip an active skill you have never equipped in a real slot and save. If it is not learned yet, learn it first with a skill point earned from level 2. Passives apply without occupying a slot. | None |
| F07 | Test and retry | Test your changed edict in training, then enter a new rift with that setup. Training grants no rewards and consumes no fatigue. | None |
| UL01_TRAIN | Training ground | Pick a rift tier and enemies, change a skill's Hunt Edict and fight the same setup again. The result compares clear time and DPS with the previous run. | None |
| UL02_ENHANCE | Equipment enhancement | Choose an owned +0 item, review the cost and enhance it to +1. First practice covers gold for this one step. | Yes |
| UL03_OFFLINE | Offline Supplies | Review supplies settled from actual rift performance. Tutorial combat never creates an offline reward baseline. | None |
| UL04_RARE_CRAFT | Rare equipment crafting | Choose a slot and craft one rare item. First practice covers its gold and material cost. | Yes |
| UL10_SLOT_ENHANCE | Equipment slot upgrade | Upgrade a level 1 equipment slot to level 2. First practice covers stones; the normal five-minute duration still applies. | Yes |
| UL05_GEM | Synthesize gems | Fuse five tier 1 gems of the same kind into one tier 2 gem. First practice supplies the five selected tier 1 gems. | Yes |
| UL11_RUNE | Rune purchase and placement | Buy one G0 single-cell rune, then inspect its placement on the rune board. First practice covers one rune purchase. | Yes |
| UL07_UNIDENTIFIED_SHOP | Unidentified equipment | Buy one unidentified item for your chosen slot. First practice covers gold and uses normal rarity probabilities. | Yes |
| UL06_REROLL | Affix reroll | Choose one affix on an owned item and reroll it. First practice covers one attempt; its result remains random. | Yes |
| UL12_ASPECT | Aspect imprinting | Select an actually owned aspect and eligible equipment. Perform the free imprint; no legendary or aspect is invented for practice. | None |
| UL08_SWEEP | Rift sweep | Review your cleared stage and remaining daily attempts, then sweep. Guidance grants no extra attempts. | None |
| UL13_ELIXIR | Gem elixirs | Craft one elixir using a selected tier 1 gem. First practice supplies one gem. Review usage in your potion slots afterward. | Yes |
| UL14_MASTERWORK | Masterworking | Masterwork an actually owned enhancement +5 or higher item from 0 to 1. First practice covers gold and materials; normal prerequisite enhancement is required. | Yes |
| UL09_CORE_CRAFT | Core crafting | Choose a recipe and craft one item with ten slot cores. First practice covers those cores with premium investment fixed at zero. | Yes |
| H01 | Equipment details and protection | Compare equipped items on the left and candidates on the right. Review option ranges and protection. Comparison alone does not equip items. | None |
| H02 | Shop and buyback | Review protection and prices before selling. Sold equipment can be bought back while retained in the buyback list. | None |
| H03 | Salvage and automatic selection | Check rewards and selected items in the salvage preview. Equipped, protected, socketed and preset-linked equipment follows shared protection rules. | None |
| H04 | Storage and equipment presets | Distinguish account storage from the hero bag. Presets reference owned equipment instead of duplicating it. | None |
| H05 | Potion slots and order | Assign owned potions to slots and review the shared usage order. Configure survival conditions in Hunt Edict. | None |
| H06 | Potion replenishment | Review replenishment costs and stock before departure. If stock or gold is insufficient, adjust the policy or cancel replenishment. | None |
| H07 | Skill points and passives | Invest earned points to improve skills. Unlocked passives apply without being equipped in active slots. | None |
| H08 | Ultimate | At actual level 40, inspect the ultimate and equip it in its dedicated slot. Review its conditions and resource cost. | None |
| H09 | Edicts and decision reasons | Inspect conditions, range and resource shortages in decision records. Edit the relevant setting from Current settings. | None |
| H10 | Presets and sharing | Review changes when saving and loading presets. Imported configurations still validate ownership and unlock requirements. | None |
| H11 | Repeat hunting and idle display | Configure actions after success or failure and when the bag is full. Dimmed display does not provide server-side progression. | None |
| H12 | Failure and resume | Review the cause of failure. Resume an interrupted rift from its saved state; this differs from starting a new run. | None |
| H13 | Bag capacity and pending rewards | When the bag is full, make space while respecting protection. Distinguish pending rewards from collected items and verify the actual claim. | None |
| H14 | Legendary, set and awakened items | Inspect unique effects, set thresholds and awakened properties in shared item details. The first guide for each kind is tracked separately. | None |
| H15 | Rift objectives and encounters | Inspect seal, carrier and offering objectives and chest, shrine, goblin, elite and boss encounters. In-run tips do not block combat; detailed guidance is available in town. | None |
| H16 | Attendance Events | Review attendance days and available rewards, then claim them. Closing or hiding automatic display preserves claim records. | None |
| H17 | One-condition class practice | Run A with the current setup, then change one condition for B while keeping equipment, level, and random seed fixed. Observe the difference before choosing whether to apply it. | None |

## Unlock authority and variants

The current new-account unlock schedule is below. Migrated accounts retain previously unlocked access.

| Guide ID | Content | Highest account rift clear |
| --- | --- | --- |
| UL01_TRAIN | Training ground | Town arrival |
| UL02_ENHANCE | Equipment enhancement | Stage 1 |
| UL03_OFFLINE | Offline Supplies | Stage 1 |
| UL04_RARE_CRAFT | Slot-based rare crafting | Stage 3 |
| UL10_SLOT_ENHANCE | Slot upgrades | Stage 5 |
| UL05_GEM | Gem socketing, replacement and synthesis | Stage 10 |
| UL11_RUNE | Rune boards, fusion and weapon mastery | Stage 15 |
| UL07_UNIDENTIFIED_SHOP | Unidentified equipment shop | Stage 25 |
| UL06_REROLL | Affix reroll | Stage 35 |
| UL12_ASPECT | Aspect upgrades and imprinting | Stage 40 |
| UL08_SWEEP | Sweep | Stage 60 |
| UL13_ELIXIR | Gem potion crafting | Stage 60 |
| UL14_MASTERWORK | Masterworking | Stage 80 |
| UL09_CORE_CRAFT | Legendary core crafting | Stage 50 |

Unlock stages come directly from `Resources/ContentUnlocks.json`; tutorial code does not duplicate stage thresholds. `H14` independently tracks legendary, set and awakened equipment. `H15` independently tracks seals, carriers, offerings, chests, shrines, goblins, elites and bosses.

The 4–6 minute first-map duration is a usability target, not a validated human measurement. Completion gates use actual actions rather than time spent reading.

Implementation and validation: [record](../Implementation/Tutorial_Progression.en.md).

## Change log

- 2026-09-29: Brought the document in line with the game code. P00 is now the *Voice of the Edict* prologue, and the intro, fixed decisions, dedicated-map flow and staging no longer describe the old *Road to the sanctuary* armor award and equip steps (P01–P08); they describe the prologue steps instead. UL01_TRAIN uses the reworked training ground text, and the missing H17 *One-condition class practice* row brings the total to 39 groups. F06 follows the new-skill rule (the first active equipped after the first skill). Every other row's title, text and first-practice support was checked against `Tutorials.All`. The English titles of UL01_TRAIN, UL03_OFFLINE, UL05_GEM, H08 and H16 now match the game's English text.
