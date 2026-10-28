# Monster Roster per Field and Hunyuan Model Plan

Updated: 2026-10-28 · [한국어](HELLSCRIPT_Monster_Roster_20261011.md)

This plan gives each of the six rift fields at least 20 regular monsters and at least 3 bosses: 120 regular monsters (20 existing + 100 new) and 18 bosses (5 existing + 13 new), 138 in total. Models are made with the Tencent HY 3D (Hunyuan) free allowance of 20 generations per day, 20 per day in the priority order below. Field rules follow [Six Rift Fields](HELLSCRIPT_Rift_Fields.en.md); existing enemy and boss numbers follow the [regular enemy and elite detail](HELLSCRIPT_Enemy_Combat_Detail.md) and the [boss combat detail](HELLSCRIPT_Boss_Combat_Detail.en.md).

## 1. Design principles

- **Look and feel**: as in Diablo IV, monsters are grouped into families, and one field shares materials and colours. Grim realistic gothic fantasy, weathered materials, bold silhouettes that read from the high top-down camera. The dark painterly portraits in `Resources/Art/TrainingGround/Portraits` are the reference. No gore such as wounds or exposed flesh.
- **One question per monster**: every monster asks the player (that is, the Hunt Edict) one question: whom to hit first, where to step, when to cast, whether to keep a movement skill, area or single-target damage. The `Edict test` column names that question.
- **Always avoidable**: all damage is shown first with the existing telegraph gauge (circle, ring, cone, line). No unavoidable damage, no loss of control longer than 1.5 s, no slow above 60%. As today, a projectile volley hits at most once per cast.
- **Roles inside a field**: each field has 5–6 melee, 2–3 charge or leap, 3 ranged, 3 zone or control, 3 support or summoner, 2 death effects and 1–2 special (ambush, stationary structure, treasure) monsters. A field never repeats a mechanic.
- **Numbers**: the stats and patterns of the existing 20 do not change. Numbers for new monsters are draft values; implementation fits them to the existing `EnemyCombat` shapes and the same damage-per-second budget (melee slot within ±15% of N01).
- **Current spawn table**: the current table (fields other than the graveyard share N02–N05) stays; new monsters shipped in the registry join the same role slot of their own field from their unlock tier (section 11).

## 2. Field summary

| Field | From tier | Families | Existing regular | New regular | Bosses |
| --- | --- | --- | --- | --- | --- |
| Forgotten Graveyard | 1 | Risen corpses, skeleton legion, wraiths, grave cult, grave beasts | N01–N06 | N21–N23 · N25–N26 · N30–N34 (10) | B01 Executioner · B06 Bone Matron · B07 Crypt Lich |
| Dank Cavern | 1 | Cave ghouls, fungus, spiders, the drowned, rift crystal, cave beasts | N15–N16 | N35–N38 · N40–N48 · N50–N52 (16) | B03 Devourer · B08 Brood Queen · B09 Drowned Abyss King |
| Dusk Grassland | 1 | Thornhorn beastmen, werewolves and wolves, briar coven, standing stones and spirits | N17–N18 | N53–N70 (18) | B10 Thornhorn Warchief · B11 Dusk Wolf King · B12 Briar Mother |
| Ruined Fortress | 11 | Penitent order, fanatics, ember cult, cathedral statues, hell minions, skeleton legion, grave cult | N07–N12 | N24 · N27 · N71–N84 (16) | B02 Chorister · B13 Iron Inquisitor · B14 Forge Lord |
| Scorched Desert | 11 | Dune raiders, sandworms and beetles, tomb mummies, serpent cult, fire spirits, corpses and wraiths | N13–N14 | N28–N29 · N85–N102 (20) | B04 Tyrant · B15 Sand Pharaoh · B16 Sun-Eater Djinn |
| Snowbound Highland | 11 | Frost revenants, frost cult, snowfield beasts, ice spirits, cave ghouls, the drowned | N19–N20 | N39 · N49 · N103–N120 (20) | B05 Matriarch · B17 Frost Giant King · B18 Revenant King of the Ice Crown |

B02 and B03 had no fixed field. Here the Chorister belongs to the fortress (cathedral choir) and the Devourer to the cavern (rift crystal beast).

2026-10-28: Six trimmed models were restored and moved: N24/N27 from F0 to F1 fortress, N28/N29 from F0 to F2 desert, and N39/N49 from F3 to F5 highland (unlock tier 11, pack slots preserved). This fills F1/F2/F5 to 8 regular rift kinds each at maximum tier; F0 stays at 10, F3 at 12, and F4 at 8. F3's actual pool is its 6 base kinds plus the modelled N35/N40/N42/N43/N45/N50. The field tables and roster JSON below reflect the current homes; the generation schedule preserves the original plan.

## 3. Forgotten Graveyard (wet flagstones, pale moonlight, teal fog)

| ID | Name | Family | Role | Pattern | Counterplay | Edict test | Gen |
| --- | --- | --- | --- | --- | --- | --- | --- |
| N01 | Chained Corpse | Corpse | Melee | Existing: 0.65 s telegraph, 120° frontal hit | Gather the pack and use area damage | Gathering | D1 |
| N02 | Grave Hound | Beast | Charge | Existing: 0.8 s telegraph, 8 m line charge | Step aside | Keep movement skill | Done |
| N03 | Skeleton Archer | Skeleton | Ranged | Existing: one arrow from 9 m | Leave the line, close in | Ranged first | D1 |
| N04 | Plague Servant | Cult | Zone | Existing: 4 s poison pool | Fight outside the pool | Leave zones | D1 |
| N05 | Funeral Priest | Cult | Support | Existing: heals allies within 3 m by 15% HP | Kill the priest first | Support first | D1 |
| N06 | Bloated Pilgrim | Corpse | Death burst | Existing: 2.5 m blast 1 s after death | Step away after the kill | Leave after kill | D1 |
| N21 | Grave Maggot Swarm | Beast | Swarm | Packs of 5, HP 0.25, short bites | Clear with one area skill | Area skill by enemy count | D2 |
| N22 | Skeleton Shieldbearer | Skeleton | Melee (guard) | Takes 60% less direct damage from the front 120°, 1 m shield shove | Circle behind or stun to drop the shield | Circle behind | D2 |
| N23 | Skeleton Spearman | Skeleton | Melee (reach) | 0.8 s telegraph, 4.5 m × 1 m thrust | Stay beyond the tip or sidestep | Hold melee distance | D2 |
| N25 | Wailing Wraith | Wraith | Ranged (control) | Takes 50% less damage while drifting; materialises, then a 0.9 s scream cone seals skills for 1.2 s | Burst it when it materialises | Skill timing | D2 |
| N26 | Skeletal Mage | Skeleton | Ranged | 1 s telegraph, 12 m piercing bone spear | Leave the line; allies do not block it | Line dodge | D2 |
| N30 | Carrion Crow Flock | Beast | Flying charge | Flies over terrain, 0.6 s telegraph, 6 m dive | Low HP, kill quickly with a single skill | Fast enemies first | D2 |
| N31 | Rotting Brute | Corpse | Melee (heavy) | HP ×2.5, 3 m ring shockwave (1.5 s), 3 maggots on death | Clear the maggots with area damage after the kill | Kill order | D2 |
| N32 | Shroud Maiden | Wraith | Support | Every 8 s shields one ally for 20% of max HP | Kill her first or burst through the shield | Support first | D2 |
| N33 | Bone Bomber | Skeleton | Death burst (runner) | Runs to the hero, 1.2 s fuse, 2.5 m self-destruct | Kill it at range or leave during the fuse | Ranged first / leave | D2 |
| N34 | Tombstone Golem | Corpse | Control | 1.2 s telegraph, a 6 m line of tombstone walls for 4 s (blocks movement) | Do not get boxed in | Positioning | D2 |

| Boss | Concept | Phase 1 | Phase 2 | Key counterplay | Gen |
| --- | --- | --- | --- | --- | --- |
| B01 Graveyard Executioner | Hook-and-axe executioner | Existing: slam · hook · reaping slash | Existing: execution leap · chain whirl · grave bells | Dodge the hook line, keep movement skills | D1 |
| B06 Bone Matron | Queen on a bone throne who births a skeleton legion | Summon 2 skeletons (cap 6) · bone prison (3 m ring of bone walls around the hero; break one segment or blink out) · triple bone spear | Bone storm (spinning blades around her for 4 s) · grave legion (2 each of N22 and N23) · bone rain (5 sequential impacts) | Movement skill for the prison, clear summons | D2 |
| B07 Crypt Lich | Mage who bound his life to soul lanterns | Places 3 soul lanterns (75% damage reduction while any burns) · frost orb · death ray (10 m line held 1.5 s) | Relights a lantern every 10 s · frost vortex ring · raise dead (2 × N26) | Break the lanterns, then hit the lich | D2 |

## 4. Dank Cavern (wet black rock, puddles, teal fungal light)

| ID | Name | Family | Role | Pattern | Counterplay | Edict test | Gen |
| --- | --- | --- | --- | --- | --- | --- | --- |
| N15 | Cave Ghoul | Ghoul | Melee | Existing: 2.5–4.5 m leap strike | Stay outside leap range | Hold distance | D1 |
| N16 | Sporebloat | Fungus | Death effect | Existing: 4 s poison spore cloud after death | Step away after the kill | Leave after kill | D1 |
| N35 | Cave Spiderling | Spider | Swarm | Packs of 6, HP 0.2, fast | Area skill | Area skill by enemy count | D2 |
| N36 | Webweaver | Spider | Ranged (control) | 0.9 s telegraph, 9 m web shot, roots for 1 s | Kill it first, dodge the line | Ranged first | D2 |
| N37 | Venomfang Lurker | Spider | Ambush | Drops from the ceiling after a 0.8 s circle telegraph; bite adds 3 poison stacks | Leave the drop circle | Leave telegraphed spot | D2 |
| N38 | Spider Egg Sac | Spider | Stationary spawner | Cannot move; hatches 2 spiderlings every 5 s | Destroy it first | Stationary target first | D3 |
| N40 | Tidecaller | Drowned | Zone (knockback) | 1.3 s telegraph, 7 m wave, knocks back 3 m | Step out of the wave's path | Dodge near walls | D3 |
| N41 | Barnacle Brute | Drowned | Melee (guard) | 50% less damage from the front, 30% more from behind, double pincer | Attack from behind | Circle behind | D3 |
| N42 | Cave Bat Swarm | Cave beast | Swarm (leech) | Erratic flight, bites heal it for 50% of damage | Area skill, manage health | Area skill / heal condition | D3 |
| N43 | Blind Cave Troll | Cave beast | Charge | Charges (1 s telegraph) to where the hero last cast a skill; stunned 2 s if it hits a wall | Cast in front of a wall to bait it | Skill position | D3 |
| N44 | Fungal Shaman | Fungus | Support | Plants a mushroom totem (stationary, heals allies within 4 m by 4% per second) | Destroy the totem first | Stationary target first | D3 |
| N45 | Spore Creeper | Fungus | Stationary turret | Cannot move; 1.2 s telegraph, lobbed spore shot (2 m radius) | Go around or kill it at range | Positioning | D3 |
| N46 | Crystal Shardback | Rift crystal | Melee (retaliation) | Every third hit taken, 0.6 s telegraph, 2 m crystal spikes | Big hits instead of many small ones; leave on the telegraph | Hit-count control | D3 |
| N47 | Cave Ooze | Fungus | Splitter | Splits into 2 smaller oozes on death (twice) | Finish with area damage | Kill order | D3 |
| N48 | Blood Leech | Cave beast | Attacher | Latches on and drains 20% of attack every 0.5 s; a movement skill shakes it off | Shake it off with a movement skill | Movement skill condition | D3 |
| N50 | Drowned Hag | Drowned | Curse | 1.4 s cast (stun interrupts): hero takes 20% more damage for 6 s | Stun or kill during the cast | Interrupt casts | D3 |
| N51 | Crystal Sentinel | Rift crystal | Stationary zone | Cannot move; a beam sweeps 90° over 2 s (1 s telegraph) | Move against the sweep | Rotating zone dodge | D3 |
| N52 | Sporebearer Beetle | Fungus | Charge | 1 s telegraph, 8 m charge, leaves a 4 s poison spore trail | Fight off the trail | Leave zones | D3 |

| Boss | Concept | Phase 1 | Phase 2 | Key counterplay | Gen |
| --- | --- | --- | --- | --- | --- |
| B03 Devourer of the Rift | Huge beast overgrown with rift crystal | Existing: charge · chained blasts · maw bite | Existing: rift tear · shard storm · devouring pull | Refuge spots, chained dodges | Done |
| B08 Brood Queen | Giant spider carrying egg sacs | Web spray (cone, 1 s root) · lays 2 egg sacs · venom leap | Hangs from the ceiling (4 s invulnerable, spiderling rain) · poison ring · web cage (lines in 4 directions, blink out) | Clear egg sacs, keep a movement skill for roots | D3 |
| B09 Drowned Abyss King | Crowned drowned giant risen from the deep | Harpoon throw (line, 2 m pull) · tidal slam (ring) · summon drowned | Flood tide (only 3 high rocks are safe, 5 s) · whirlpool (pulls inward) · three tidal waves | Reach a safe rock during the flood | D3 |

## 5. Dusk Grassland (trampled grass and menhirs, rose dusk, violet fog)

| ID | Name | Family | Role | Pattern | Counterplay | Edict test | Gen |
| --- | --- | --- | --- | --- | --- | --- | --- |
| N17 | Thornhorn Brute | Thornhorn | Melee | Existing: 0.9 s telegraph, 2.1 m spin | Stay outside the spin | Melee edge circling | D1 |
| N18 | Briar Witch | Coven | Ranged | Existing: 9 m thorn lance, slow | Dodge the line | Ranged first | D1 |
| N53 | Thornhorn Runt | Thornhorn | Swarm | Packs of 4, short spear jabs | Area skill | Area skill by enemy count | D3 |
| N54 | Thornhorn Slinger | Thornhorn | Ranged | 1.1 s telegraph, lobbed stone at the hero (1.5 m) | Leave the circle | Leave telegraphed spot | D3 |
| N55 | Thornhorn Gorer | Thornhorn | Charge | 0.9 s telegraph, 7 m gore, knocks down 0.6 s | Step aside | Keep movement skill | D3 |
| N56 | Thornhorn Drummer | Thornhorn | Support | War drum aura: allies within 6 m gain 25% move and attack speed | Kill the drummer first | Support first | D4 |
| N57 | Thornhorn Shaman | Thornhorn | Zone | Drops 3 lightning pillars 0.4 s apart along the hero's path | Change direction | Change predicted path | D4 |
| N58 | Dusk Wolf | Wolf | Melee (flank) | Packs of 3, bites from behind deal 30% more | Fight with your back to a wall | Positioning | D4 |
| N59 | Werewolf | Wolf | Melee (ramp) | Each consecutive hit adds 10% attack speed (max 5), 5 m leap | Hit and step out in short bursts | Hold distance | D4 |
| N60 | Howling Werewolf | Wolf | Summoner | 1.2 s howl (stun interrupts): summons 2 dusk wolves and frenzies allies | Interrupt the howl | Interrupt casts | D4 |
| N61 | Briar Crawler | Coven | Ambush (control) | Hidden underground; within 4 m, 1 s telegraph then a 1 s vine root | Avoid the marker, kill at range | Ambush response | D4 |
| N62 | Gilded Wisp | Spirit | Treasure | Never attacks, flees and vanishes after 8 s; drops bonus gold and materials | Decide whether to chase it past other enemies | Chase treasure | D4 |
| N63 | Menhir Warden | Menhir | Melee (heavy) | 1.3 s telegraph, rune cracks in a 4-way cross (6 m) | Move diagonally | Cross zone dodge | D4 |
| N64 | Thornback Boar | Beast | Charge | 1 s telegraph, 9 m charge, sheds 1.5 m quills when hit | Dodge the charge, kill at range | Keep movement skill | D4 |
| N65 | Hemlock Witch | Coven | Zone | 1.2 s telegraph, lobbed poison flask (2.5 m, 3 s) | Fight outside the pool | Leave zones | D4 |
| N66 | Pollen Sprite | Spirit | Death effect | 0.5 s after death, 2.5 m drowsy pollen (60% slow for 2 s) | Step away after the kill | Leave after kill | D4 |
| N67 | Field Scarecrow | Coven | Ambush | Poses as a scarecrow; wakes within 3 m, 0.8 s telegraph then a scythe spin | Check suspicious scarecrows from range | Ambush response | D4 |
| N68 | Thornhorn Berserker | Thornhorn | Melee (frenzy) | Below 50% HP: +50% damage, +30% move speed | Finish it in one burst after half HP | Low-HP target first | D4 |
| N69 | Dusk Huntsman | Wolf | Ranged | 1.4 s telegraph, 5 arrow-rain circles (1.2 m) | Move into the gaps | Multi-zone dodge | D4 |
| N70 | Elder Treant | Spirit | Support (guard) | Root link: allies within 6 m take 25% less damage; 2.5 m root slam | Fell the treant first | Support first | D4 |

| Boss | Concept | Phase 1 | Phase 2 | Key counterplay | Gen |
| --- | --- | --- | --- | --- | --- |
| B10 Thornhorn Warchief | Beastman chief with war drums and a great axe | Axe spin (3 m) · earth slam (3 shockwave lines) · runt rush (4 × N53) | War drum totem (frenzies allies, ends when broken) · great charge (12 m twice) · roar of rage (two rings) | Break the totem first, dodge charges | D4 |
| B11 Dusk Wolf King | Old werewolf that grows fiercer as dusk falls | Three chained leaps · claw flurry · call the pack (3 × N58) | Two shadow clones (they telegraph but deal no damage; the real one has a different colour) · moon howl (slowing ring) · hunting charge | Find the real one, avoid leap landings | D4 |
| B12 Briar Mother | Witch tree rooted in the middle of the grassland | Thorn walls grow (block part of the arena for 5 s) · scatters seeds (summons N61) · thorn lance cone | Exposes 3 root hearts (breaking all stuns her for 6 s) · vine whip spin · thorn rain | Break the hearts, burst during the stun | D4 |

## 6. Ruined Fortress (scorched granite and braziers, smoky orange fog)

| ID | Name | Family | Role | Pattern | Counterplay | Edict test | Gen |
| --- | --- | --- | --- | --- | --- | --- | --- |
| N07 | Ironclad Wraith | Penitent order | Melee | Existing: rear exposed 0.8 s after attacking | Circle behind after its swing | Edge circling | D1 |
| N08 | Fanatic Charger | Fanatic | Charge | Existing: two charges in a row | Keep moving after the first | Keep moving | D1 |
| N09 | Blackiron Crossbowman | Penitent order | Ranged | Existing: 3-bolt fan | Close in | Line dodge | D1 |
| N10 | Emberfire Sorcerer | Ember cult | Zone | Existing: predicted fire pool on the hero's path | Change path | Change predicted path | D1 |
| N11 | Bellringer | Fanatic | Support | Existing: allies within 4 m +20% attack | Kill it first | Remove support | D1 |
| N12 | Rift Shardling | Hell minion | Death burst | Existing: 3 shards after death | Manage kill position and lines | Kill position | Done |
| N24 | Barrow Knight | Skeleton | Melee (heavy) | 1.4 s telegraph, 2.2 m slam, knocks down for 0.5 s | Step back on the long telegraph | Leave during telegraph | D2 |
| N27 | Corpse Puppeteer | Cult | Summoner | Every 8 s raises a skeleton from a corpse under 10 s old (max 3) | Kill the puppeteer first, clear corpses | Summoner first | D2 |
| N71 | Penitent Flagellant | Fanatic | Melee (support) | 30% of damage it takes heals allies within 4 m | Focus it instead of spreading area damage | Switch to single target | D4 |
| N72 | Bulwark Halberdier | Penitent order | Melee (guard) | Immune to frontal projectiles, 1 s telegraph, 3.5 m halberd sweep | Close in or flank | Circle behind / close in | D4 |
| N73 | Ember Imp | Hell minion | Swarm | Packs of 5, short blinks, leaves 1 s embers | Area skill | Area skill by enemy count | D5 |
| N74 | Cathedral Gargoyle | Cathedral statue | Ambush | A statue (invulnerable) until the hero passes, then dives; turns to stone for 2 s at 50% HP | Switch targets while it is stone | Target switch | D5 |
| N75 | Siege Ballista Crew | Penitent order | Stationary ranged | Cannot move; 2 s telegraph, 16 m piercing bolt (×3 damage) | Stay off the line, kill the crew | Line dodge | D5 |
| N76 | Inquisitor | Ember cult | Control | 1.2 s telegraph, branding chain: 2 s root plus burn | Stun or move during the cast | Interrupt casts | D5 |
| N77 | Iron Maiden Bearer | Fanatic | Melee (grab) | 1.5 s telegraph grab: 1 s loss of control plus damage | Leave on the long telegraph | Leave during telegraph | D5 |
| N78 | Hellfire Hound | Hell minion | Charge | 0.9 s telegraph, 8 m charge, 3 s fire trail | Fight off the trail | Keep movement skill / leave zones | D5 |
| N79 | Fallen Standard-Bearer | Penitent order | Support | Plants a banner: allies within 6 m take 30% less damage (the banner is a breakable object) | Break the banner first | Stationary target first | D5 |
| N80 | Forge Golem | Hell minion | Melee (death zone) | Slow slams; leaves a 3 m lava pool for 3 s on death | Step away after the kill | Leave after kill | D5 |
| N81 | Pyre Priest | Ember cult | Support (revive) | 2 s cast (interruptible) revives one fallen ally | Interrupt, kill the priest first | Interrupt / support first | D5 |
| N82 | Rift Tendril | Hell minion | Stationary zone | Bursts from a floor crack, 1 s telegraph, 6 m sweep | Stay outside its reach | Rotating zone dodge | D5 |
| N83 | Demon Overseer | Hell minion | Melee (support) | 5 m whip; whips allies for +40% move speed | Kill the overseer first | Support first | D5 |
| N84 | Soulreaver Knight | Penitent order | Melee (teleport) | 0.7 s telegraph, teleports behind the hero and slashes | Move when the telegraph appears | Leave during telegraph | D5 |

| Boss | Concept | Phase 1 | Phase 2 | Key counterplay | Gen |
| --- | --- | --- | --- | --- | --- |
| B02 Chorister of the End | Conductor of the cathedral choir | Existing: summon minions · focused beam · dirge ring | Existing: requiem chorus · hymn of silence · orbs of lament | Minions first, safe spots | D1 |
| B13 Iron Inquisitor | Order commander with a tower shield | Shield charge · frontal shield (80% less frontal damage) · branding chains | Drops the shield → twin flail flurry · pyre ring · summons 2 × N76 | Phase 1 circle behind, phase 2 dodge the flurry | D5 |
| B14 Forge Lord | Demon smith of the hell forge | Anvil slam (3.5 m) · three lava streams · chain drag | Furnace burst (half the floor is lava for 4 s) · casts golems (2 × N80) · spark storm | Move to the safe half, manage golem death spots | D5 |

## 7. Scorched Desert (sand-buried sandstone, low amber sunlight)

| ID | Name | Family | Role | Pattern | Counterplay | Edict test | Gen |
| --- | --- | --- | --- | --- | --- | --- | --- |
| N13 | Dune Marauder | Raider | Melee | Existing: double slash | Gather the pack | Gathering | Done |
| N14 | Burrowing Sandworm | Sandworm | Charge (burrow) | Existing: tunnels and erupts | Leave the telegraphed spot | Leave telegraphed spot | D2 |
| N28 | Gravedigger | Corpse | Ambush | Hidden in a dirt mound; within 4 m, 1 s telegraph then a 2.5 m shovel burst | Read the mound and go around | Ambush response | D2 |
| N29 | Chain Specter | Wraith | Control | 1 s telegraph, 10 m chain, pulls 2.5 m on hit | Kill it from outside its line | Keep movement skill | D2 |
| N85 | Scarab Swarm | Beetle | Swarm | Packs of 6, burst out of the sand | Area skill | Area skill by enemy count | D5 |
| N86 | Dune Scorpion | Beetle | Melee | Double pincer plus a 0.8 s telegraphed tail sting (poison stacks) | Sidestep the tail telegraph | Leave during telegraph | D5 |
| N87 | Tomb Mummy | Mummy | Melee (revive) | Rises with 50% HP 3 s after first falling (hitting it meanwhile destroys it) | Finish fallen mummies | Corpse handling | D5 |
| N88 | Raider Fire Archer | Raider | Ranged | 1 s telegraph, fire arrow that leaves 2 s of flame | Dodge the line, close in | Ranged first | D5 |
| N89 | Raider Javelineer | Raider | Ranged (lob) | 1.3 s telegraph, javelin at the hero (1.2 m, ×2 damage) | Leave the circle | Leave telegraphed spot | D5 |
| N90 | Serpent Cult Warrior | Serpent cult | Melee (dash) | 0.6 s telegraph, 3 m side slither plus venom slash | Counter after the slither | Hold distance | D5 |
| N91 | Serpent Cult Seer | Serpent cult | Support | Gives one ally a 2 s sand veil (reflects projectiles) | Attack someone else meanwhile | Target switch | D6 |
| N92 | Sand Viper | Beetle | Ambush swarm | Three of them, 0.6 s telegraph bites from the sand | Leave the circle | Ambush response | D6 |
| N93 | Carrion Vulture | Beast | Flying | Dives at a low-HP hero, eats corpses to heal | Avoid corpses, kill it quickly | Fast enemies first | D6 |
| N94 | Sand Wraith | Wraith | Moving zone | 1 s telegraph, a sand twister (1.5 m) that moves for 3 s | Stay off its path | Moving zone dodge | D6 |
| N95 | Sun Cultist | Serpent cult | Zone | A circle shrinking from 4 m to 1.5 m over 1.6 s ends in a sunbeam | Get outside the circle | Leave zones | D6 |
| N96 | Sandstone Colossus | Spirit | Melee (splitter) | HP ×2, breaks into 3 small sand spirits on death | Clear them with area damage after the kill | Kill order | D6 |
| N97 | Ember Djinn | Spirit | Ranged (blink) | Blinks 4 m when hit, 6-orb fire spiral | Follow its blink | Target tracking | D6 |
| N98 | Jackal Tombguard | Mummy | Melee (elite-grade) | 5 m spear leap plus 3 thrusts; +20% attack near a sarcophagus | Pull it away from the sarcophagus | Positioning | D6 |
| N99 | Cursed Sarcophagus | Mummy | Stationary spawner | Cannot move; opens every 6 s to release a mummy | Destroy it first | Stationary target first | D6 |
| N100 | Raider Chieftain | Raider | Support (summoner) | War cry: allies within 6 m +25% attack, calls 2 dune marauders | Kill the chieftain first | Support first | D6 |
| N101 | Antlion Pit | Beetle | Stationary control | A 4 m sand pit slowly drags inward; mandible bite at the centre | Kill it from outside the pit | Positioning | D6 |
| N102 | Blister Beetle | Beetle | Death effect | 0.6 s after death, a 2.5 m acid pool for 3 s | Step away after the kill | Leave after kill | D6 |

| Boss | Concept | Phase 1 | Phase 2 | Key counterplay | Gen |
| --- | --- | --- | --- | --- | --- |
| B04 Dune Tyrant | Giant scorpion-worm half buried in sand | Existing: tail lash · venom spray · burrow strike | Existing: quicksand vortex · scarab swarm · triple eruption | Dodge lines, clear minions | D1 |
| B15 Sand Pharaoh | Mummy king in a golden mask | Curse brand (explodes on the hero after 4 s; move away from allies) · two sand walls · raise mummies (2 × N87) | Sun-disk beams (2 lines rotating around him for 4 s) · sandstorm (reduced vision, 3 twisters) · sarcophagus seal (4 s invulnerable, ended by breaking 2 sarcophagi) | Move with the rotating beams, break sarcophagi | D6 |
| B16 Sun-Eater Djinn | Giant djinn of flame and mirage | Triple fire orb · 2 mirage clones (explode when killed) · fire pillar line | Meteor rain (6 sequential) · shrinking safe circle (fire outside) · sun absorption (4 s cast; heavy area damage unless interrupted) | Tell clones apart, interrupt, stay in the safe circle | D6 |

## 8. Snowbound Highland (snow-covered stone and ice, cold white sunlight)

| ID | Name | Family | Role | Pattern | Counterplay | Edict test | Gen |
| --- | --- | --- | --- | --- | --- | --- | --- |
| N19 | Frostbitten Revenant | Frost revenant | Melee | Existing: frost slash, slow | Clear the slow | Gathering / clear slows | D1 |
| N20 | Rime Caller | Frost cult | Zone | Existing: frost ring, slow | Ring centre or outside | Ring dodge | D1 |
| N39 | Drowned Thrall | Drowned | Melee | Rises from puddles and grabs (40% slow for 2 s) | Kill before the grab | Clear slows | D3 |
| N49 | Ghoul Butcher | Ghoul | Melee (frenzy) | Each nearby ally death gives +30% attack speed for 5 s (stacks 3) | Kill it first, or last with single-target burst | Kill order | D3 |
| N103 | Frost Wolf | Snow beast | Melee (stacks) | Packs of 3; each bite adds a chill stack, 3 stacks freeze for 1 s | Kill or leave before 3 stacks | Stack management | D6 |
| N104 | Yeti | Snow beast | Ranged (lob) | 1.2 s telegraph, big snowball (2 m); slams when close | Leave the circle | Leave telegraphed spot | D6 |
| N105 | Glacier Golem | Ice spirit | Melee (death effect) | HP ×2, 0.8 s after death fires ice shards in 8 directions | Stand between shards | Kill position | D6 |
| N106 | Frost Wisp | Ice spirit | Swarm | Packs of 6, touch slows by 20% | Area skill | Area skill by enemy count | D6 |
| N107 | Revenant Axe Thrower | Frost revenant | Ranged (returning) | 1 s telegraph, 10 m spinning axe that hits again on its way back | Step off the line | Line dodge | D6 |
| N108 | Frost Cult Zealot | Frost cult | Melee (guard) | Ice armour: ignores the first 3 hits taken | Strip it with fast multi-hits | Hit-count control | D6 |
| N109 | Avalanche Spirit | Ice spirit | Charge | 1.2 s telegraph, a rolling, growing snowball charge of 10 m | Step aside | Keep movement skill | D7 |
| N110 | Frost Mammoth | Snow beast | Melee (heavy) | HP ×3, 0.9 s telegraphed stomp with two rings, 6 m tusk charge | Time the gap between rings | Ring dodge | D7 |
| N111 | Snowfield Huntress | Frost cult | Ranged (trap) | 1 s telegraph arrow; places an ice trap 3 m ahead (1 s root when stepped on) | Path around trap markers | Path choice | D7 |
| N112 | Rime Witch | Frost cult | Support | Blizzard aura: allies within 5 m take 25% less damage, the hero inside is slowed 20% | Kill the witch first | Support first | D7 |
| N113 | Frozen Knight | Frost revenant | Melee (elite-grade) | Encased in ice (invulnerable) until the hero is within 4 m; 1.2 s telegraph, 220° greatsword slash | Gain distance as it wakes | Ambush response | D7 |
| N114 | Frost Harpy | Snow beast | Flying ranged | 3-feather fan from the air, 0.7 s telegraphed dive | Kill it quickly | Fast enemies first | D7 |
| N115 | Revenant Hornblower | Frost revenant | Summoner | 1.5 s horn (interruptible): 2 frostbitten revenants rise from the snow | Interrupt the horn | Interrupt casts | D7 |
| N116 | Mountain Ogre | Snow beast | Melee (heavy) | 1.1 s telegraph, 180° club sweep; roar slows 30% | Circle behind | Circle behind | D7 |
| N117 | Rime Crystal Pylon | Ice spirit | Stationary zone | Cannot move; two pylons are linked by a frost beam (slow and damage when crossed) | Break one pylon to cut the beam | Stationary target first | D7 |
| N118 | Blizzard Wraith | Wraith | Ambush | Invisible in the blizzard until 0.8 s before it attacks | Counter when revealed | Ambush response | D7 |
| N119 | Frost Bloater | Frost revenant | Death effect | 0.8 s after death, a 2.5 m freezing burst (1 s root) | Step away after the kill | Leave after kill | D7 |
| N120 | Icefang Leopard | Snow beast | Leap | From beyond 6 m, 0.7 s telegraph pounce, backs off after hitting | Avoid the landing spot | Hold distance | D7 |

| Boss | Concept | Phase 1 | Phase 2 | Key counterplay | Gen |
| --- | --- | --- | --- | --- | --- |
| B05 Hoarfrost Matriarch | Frost priestess with an ice-spike crown | Existing: ice lance · frost burst · glacial spikes | Existing: blizzard veil · frozen sentinels · shatter fan | Manage slows and distance | D1 |
| B17 Frost Giant King | Frost giant with a crown of rock | Boulder throw (3 lobs) · stomp ring · ice club sweep | Avalanche (3 lines of boulders roll across the arena) · blizzard (reduced vision) · summon yetis (2 × N104) | Move between avalanche lines | D7 |
| B18 Revenant King of the Ice Crown | Revenant king with a frozen army | Ice sword waves (3 lines) · raise the army (3 × N19) · ice tomb (encases the hero 2 s, blink out) | Frost throne (freeze spreads from the arena edge) · soul harvest (absorbs minions to heal; clear them first) · greatsword spin | Clear minions to deny healing, keep movement skills | D7 |

## 9. Draft boss unlock tiers

The three bosses of a field unlock one by one as the tier rises in that field. Existing boss tiers stay (Executioner 1, Chorister 11, Devourer 21, Tyrant 31, Matriarch 41).

| Field | First boss | Second | Third |
| --- | --- | --- | --- |
| Forgotten Graveyard | B01 tier 1 | B06 tier 16 | B07 tier 36 |
| Dank Cavern | B08 tier 1 | B03 tier 21 | B09 tier 46 |
| Dusk Grassland | B10 tier 1 | B11 tier 26 | B12 tier 51 |
| Ruined Fortress | B02 tier 11 | B13 tier 31 | B14 tier 56 |
| Scorched Desert | B04 tier 31 | B15 tier 41 | B16 tier 61 |
| Snowbound Highland | B05 tier 41 | B17 tier 46 | B18 tier 66 |

The tiers in the table are where the new bosses (B06–B18) unlock in their home field. **Actual selection rule (implemented)**: the existing five bosses are picked first by the existing stage rule regardless of field (unlocking at tiers 1/11/21/31/41), and when a shipped data boss has this field as its home and its tier is reached, the two are pooled and the layout's hash picks one. So the cavern's Brood Queen and the grassland's Thornhorn Warchief are drawn alongside the Executioner from tier 1 (they do not replace it), and existing seeds keep their boss in every field and tier without a shipped data boss. Before tiers 31/41, the desert and highland keep showing other fields' existing bosses as today.

## 10. Hunyuan model plan

### 10.1 Daily allocation of 20

| Day | Targets | Why |
| --- | --- | --- |
| D1 (2026-10-11) | N11, N15–N20, B04, B05 with no dedicated model; procedural N01, N03–N10, B01, B02 (20) | Replace enemies that already appear in the game |
| D2 | N14, graveyard N21–N34, B06, B07, cavern N35–N37 (20) | Tier-1 fields; finish the first field, the graveyard |
| D3 | Cavern N38–N52, B08, B09, grassland N53–N55 (20) | Tier-1 field |
| D4 | Grassland N56–N70, B10–B12, fortress N71, N72 (20) | Tier-1 field |
| D5 | Fortress N73–N84, B13, B14, desert N85–N90 (20) | Tier-11 field |
| D6 | Desert N91–N102, B15, B16, highland N103–N108 (20) | Tier-11 field |
| D7 | Highland N109–N120, B17, B18 (14) + 6 spare for regeneration | Tier-11 field, regenerate failures |

N02, N12, N13 and B03 already have Hunyuan models and are not regenerated. 134 models are generated over 7 days. A result that fails the silhouette, pose or material review is regenerated on the next day's spare slots; failures push the later order back by a day.

### 10.2 Generation procedure

- Site: https://hy3d.tencent.ai/ AI Creation → Text-to-3D, `3DGeneration-V3.1`, 50k faces, PBR. One request uses one of the daily generations and makes 4 candidates. At most 3 run at once (a 4th submission is refused with `The current concurrency exceeds the limit` and does not use the allowance).
- The user signs in by hand in the in-app browser. Submissions carry a signature, so they go through the page's text box and `Generate Now` button; results are read with `GET /api/3d/creations/detail?creationsId=…`.
- Prompt form (500 characters at most): `Full-body <appearance>. Neutral stance, limbs slightly apart from the body. Single isolated creature, no ground, base or background. Grim realistic dark gothic ARPG style, bold readable silhouette, weathered PBR materials, complete back.` The appearance follows the portraits and the concepts here, with the field colour as accent (graveyard teal, cavern teal fungus, grassland rose and violet, fortress orange embers, desert amber, highland ice blue).
- Of the 4 candidates, pick the one with the clearest silhouette, limbs apart from the body for rigging, and no ground or base.
- Original GLBs, prompts, hashes and selection reasons live outside the repository in `AssetDeliverables/Hunyuan/2026-10-11/` (and a folder per later day). Downloads happen after the user allows them.

### 10.3 In-game use (follow-up work)

- Reduction: bake with the existing `optimize_all.py --game` pipeline to 6,000 triangles and 512 px for regular monsters, 6,000–12,000 triangles and 1024 px for bosses.
- Rigging: install into `Resources/World/HunyuanReady/` with the existing `HunyuanReady` pivot rig (`Biped`, `Quadruped` and so on; N02, N12 and BOSS03 are the precedent), together with texture budget rows.
- Combat behaviour, spawn tables and localisation keys go in as registry data (section 11, `tools/monster_registry.py add`). Portraits (Codex images) and models are connected with `art-ready` once they exist, and the Monster Codex follows the registry list (25 growing to 138).
- Public distribution approval for Hunyuan output is still open; every record stays `productionApproved: false`.

## 11. Monster registry and data-driven combat (implemented 2026-10-11)

The foundation for adding monsters and bosses as data, without code changes. When the daily routine moves 20 monsters into the game with `tools/monster_registry.py add`, they spawn in rifts the same day.

### 11.1 Registry

- Game data lives in `Assets/HELLSCRIPT/Resources/Monsters/MonsterRegistry.json`; the runtime is `Runtime/Core/MonsterRegistry.cs`. The contract the Monster Codex reads: `MonsterRegistry.All` (codex order: field index, regular before boss, then number), `ById`, `ForKind`, `ForBoss`, `IdOf(EnemyState)` (bosses map to the B id of their pattern, everything else to the N id of its kind, elites to their base kind, the golden goblin to null).
- Entry fields: `id` (N01–N120, B01–B18), `boss`, `kind` (N number − 1, −1 for bosses), `pattern` (B number − 1, −1 for regular), `name`/`role`/`family` (Korean localisation keys, English in `en.txt`), `field` (GameCatalog field index; bosses have a home field: B01 graveyard, B02 fortress, B03 cavern, B04 desert, B05 highland), `slot` (pack role 0 melee, 1 charger, 2 ranged, 3 zone, 4 support, 5 exploder), `unlockTier`, `portrait` (`Art/TrainingGround/Portraits/<ID>`), `art` (HunyuanReady key), `artPending`, `skills` (name and description keys, at least one element tag physical/fire/cold/lightning/poison/shadow, the damage multiplier of one hit or 0, optional `icon`), `hp`/`attack`/`speed`, for bosses `keepDistance`/`approach`/`body`, `attacks`, `traits`, `legacy`.
- The existing 25 are `legacy: true`: the entry only describes their current kind, pattern, stats and skills; their combat stays the existing code. Existing boss skill icons are `Art/Boss/skill-<definition>`. From the 21st kind and the 6th boss on, names, stats (`CombatSimulation.EnemyStats`, `BossCombat.ProfileOf`) and combat come from the registry.
- A monster whose art is not ready ships with `artPending: true`: `portrait` and `art` are empty strings, the world shows the existing primitive figure and the training ground an empty portrait frame. Another monster's portrait is never used as a stand-in.
- The design source is `Docs/Design/MonsterRoster/roster.json`: all 138 in the registry schema plus Korean and English text (`nameEn`, `roleEn`, `familyEn`, skill `nameEn`/`descriptionEn`), `shipped`, `day` and `deferred` (mechanics not implemented yet).

### 11.2 Data-driven combat primitives

`Runtime/Core/MonsterBehavior.cs` owns the geometry; `CombatSimulation.Monsters.cs` and `CombatSimulation.MonsterTraits.cs` own the execution. The simulation, the warning gauges (`EnemyCombat.Threats`) and the damage forecast (`ForecastIncoming`) read the same shape function. The existing 25 never enter this path, so existing seeds keep their results.

| Attack `type` | Behaviour | Main fields |
| --- | --- | --- |
| `strike` | Circle, ring, sector or line, telegraphed on the caster (`at: self`), the hero (`hero`), the predicted spot (`predicted`) or where the hero last started a skill (`lastSkill`), moved `offset` along the aim. `hits` 2+ adds strikes `hitDelay` apart; `pattern` `row` (along the aim), `around` (centre plus a ring), `cross` (`count` directions, 4 by default) or `lanes` (side by side) | `shape`, `radius`, `inner`, `angle`, `length`, `count`, `spacing`, `spread` |
| `volley` | A fan of projectiles; the hero is hit at most once per volley. `returning` adds a return pass (still one hit), `impactZone` leaves a zone where each shot stops | `count`, `spread`, `speed`, `projectile`, `impactRadius`, `impactPower` |
| `charge` | Line charge; the body hits once as it passes. `growth` widens the contact per metre, `trail` leaves a zone along the path, `wallStun` stuns the charger for 2 s against a wall | `travel`, `speed`, `trail`, `trailPower` |
| `leap` / `burrow` / `blink` | Leap to 1 m short of the hero, tunnel to the hero and erupt, or blink behind (`blinkTo: behind`) or beside (`side`) the hero and strike. `retreat` springs back that far after landing | `travel`, `speed` |
| `zone` | Ground zone: every `interval` for `duration`, after `zoneDelay`; `moveSpeed` drifts, `endRadius` shrinks, `endInner` moves a ring's inner edge (freeze spreading from the edge, a closing safe circle), `spin` sweeps a beam, `follow` rides on the caster, `pull` pulls while waiting, `refuges` adds safe spots (`refugeRadius`), `trap` springs once when the hero steps in | `duration`, `interval`, `spin`, `endInner`, `refuges` |
| `attach` | A melee latch: on a hit it rides on the hero and drains `trailPower` every `interval`; a movement skill, a stun or `duration` ends it | `duration`, `interval`, `trailPower`, `leech` |
| `heal` / `shield` | Heal allies in range / give another ally a ward (`ward`) or projectile reflection (`reflect` seconds) | `heal`, `ward`, `reflect` |
| `summon` / `raise` / `harvest` | Summon (alternating with `summonAlt`, which falls back to `summon` until it ships) / raise from a corpse / absorb its own minions, healing `heal` per minion | `summon`, `count`, `cap` |
| `object` | Breakable objects (`obj`): `layout` `ring` (around the hero), `line`, `wall` (across the aim) or `around` (around the caster). `blocks` stops the hero walking (movement skills pass; at most 2 s around the hero, 5 s as walls), auras (`auraRadius`: attack, speed, damage reduction, healing, hero slow), `ownerReduction` (the owner takes less while it stands), `ownerInvulnerable`, `stunOwner` (breaking all staggers or stuns the owner), `link` (a beam from the owner to the object), `mimic` (a darkened decoy that only telegraphs the owner's phase-1 attacks), `burstPower` (explodes when broken) | `obj.*` |

- On-hit effects: `slow` (seconds) with `slowStrength` (35% by default, at most 60%), `root`, `knockdown`, `push`, `pull`, `chill`/`chillLimit`, `seal` (skills cannot start; basic attacks can), `curse`/`curseTime` (more damage taken), `poison`/`poisonPower`/`poisonCap` (stacking poison every 0.5 s), `leech` (heals by a share of damage dealt), `blind` (vision capped at 7 m), `rearPower` (bonus when biting from behind the hero, decided at announce). Loss of control and seals last at most 1.5 s and are reduced by control resistance. Hero statuses show as global HUD status icons.
- Other attack fields: `consume` (self-destruct), `trigger: hits` (retaliation only), `invulnerable` (the boss is invulnerable and holds still that long), `frenzy` (hastes nearby allies), `blind` (applied on release).
- Traits: frontal guard (`frontReduction`, dropped while stunned or frozen, phase 1 only with `shieldPhase1`), `frontProjectiles` (frontal projectiles do nothing), `rearBonus`, `idleReduction` (less damage while not acting), `guardHits`, `enrageBelow`, ally auras and `auraHeroSlow`, `stationary`, `flying` (moves straight over walls, drawn in the air), `ambushRadius` (hidden under a visible mound, untouchable and unseen), `fleeSeconds`/`bonusGold` (treasure: gold and materials), `pack`, `retaliateHits`/`retaliateAttack` (counterattack every N hits taken), `allyDeathHaste`, `hitHaste`, `hasteCap`, `hasteTime` (faster cooldowns), `shareHeal`, `blinkOnHit`/`blinkCooldown`, `blinkStep`/`blinkTrail` (moves in blinks leaving embers), `petrifyBelow`/`petrifySeconds`, `reviveHealth`/`reviveDelay` (rises after falling unless hit in between), `nearKind`/`nearRadius`/`nearAttack`, `corpseHeal`, death effects (`deathDelay`, `deathRadius`, `deathPower`, `deathDuration`, `deathSlow`, `deathSlowStrength`, `deathRoot`, `deathShards`), splitting.
- Data bosses: the 6 `attacks` are the kit (0–2 phase 1, 3–5 phase 2). They use BossCombat's slot order, cooldowns, the roar at 50% HP, movement, the minion cap (8) and stagger unchanged; the basic attack is the existing `BOSS_BASIC`.
- Attack definition ids: a regular monster's first attack is `N21`, later ones `N21_2`; bosses `B06_1`–`B06_6`; death effects `N33_DEATH`. Objects have no registry id, so `IdOf` returns null for them (not in the codex).

### 11.3 Spawn tables and bosses

- A pack slot's candidates are the slot's original kind plus every shipped monster of the same field and `slot` that has a 3D model and whose `unlockTier` is at most the current tier (in kind order). The pick is a hash of the layout's `encounterSeed` and the spawn index (`RiftGenerator.SpawnPick`), never a run or encounter stream value. A slot with one candidate spawns exactly as before, and positions, roles, elites and rewards never change. Tiers 1–5 still use slot 0 only.
- A non-empty registry `art` key means the monster has a model. Art-pending entries and entries whose models were trimmed are excluded from the additional rift candidates; they stay in the registry and codex. Their codex cards and details use the shared "Coming soon." message instead of kill counts, and they do not contribute to discovery progress.
- New regular monsters unlock at tier 6 in the tier-1 fields (graveyard, cavern, grassland) and tier 11 in the tier-11 fields (fortress, desert, highland), so tier 1–5 natural progression and the recorded results (FieldTests goldens, tier-1 fights) stay unchanged.
- A swarm's placed spawn leads `pack` − 1 followers. Followers count as adds (no reward, kill-gauge meter or corpse), so rift rewards stay as they were.
- Bosses: follow the "actual selection rule" in section 9. The boss the stage rule picks (unlocks at tiers 1/11/21/31/41, unchanged) is pooled with the shipped data bosses that have a 3D model, whose home is that field and whose tier is reached, and the layout's hash (`RiftGenerator.FieldBoss`) picks one. Fields and tiers without additional candidates keep their boss.
- The training ground picker and the skill preview scenario check accept every registered kind.

### 11.4 Tool

- `python3 tools/monster_registry.py next 20`: the next unshipped ids in day order (`tools/monster_pipeline/day_order.json` when present, otherwise the roster's `day`).
- `python3 tools/monster_registry.py add <ID...> [--art-pending] [--allow-deferred]`: copies roster entries into the registry, writes English lines for missing Korean keys into `en.txt`, marks them `shipped: true` and validates; nothing is written when validation fails. Entries with `deferred` mechanics are refused without `--allow-deferred`.
- `python3 tools/monster_registry.py refresh <ID...>`: re-copies a shipped entry after its roster entry changed (keeps the art-pending state).
- `python3 tools/monster_registry.py art-ready <ID...>`: once the portrait and model of an art-pending entry land, sets the roster's `portrait`/`art` and validates.
- `python3 tools/monster_registry.py validate`: schema, unique ids, kind/pattern agreement, text completeness (missing English, object names included), element tags, slow strength 60%, seals 1.5 s, curse/vision/poison caps, object layout and blocking time (2 s around the hero, 5 s as walls), retaliation setup, damage-per-second budget (melee slot within ±15% of N01, other slots at most N01 + 15%, boss hits at most 2.0×), telegraphs of 0.4 s or more, loss of control at most 1.5 s, slows at most 3 s, registered summon/split kinds, and portrait/model files for entries that are not art-pending.
- Regression checks: `python3 tools/test_monster_registry.py` and Edit Mode `MonsterRegistryTests`, `MonsterPrimitiveTests`, `MonsterMechanicTests`, `MonsterSpawnTableTests` (the same rules again in C#).

### 11.5 Shipped monsters (2026-10-11)

N21 Grave Maggot Swarm, N22 Skeleton Shieldbearer, N23 Skeleton Spearman, N33 Bone Bomber and B06 Bone Matron shipped art-pending. Every skill description matches what it does (B06's bone prison is a breakable 2 s ring of bone walls, grave legion summons 2 each of N22 and N23).

### 11.5.1 Art-pending batch (2026-10-20)

88 of the 90 unshipped roster entries (79 regular N30, N32, N34, N36–N38, N41, N46, N47, N50–N57 and N59–N120, plus 9 bosses B09 and B11–B18) shipped together with `add --art-pending`. They have no model or portrait yet, so the world shows the primitive figure and the codex an empty portrait frame. `art-ready` clears the flag once art lands.

- Unlock tiers are the roster's own (regular monsters tier 6 in graveyard, cavern and grassland, tier 11 in fortress, desert and highland; bosses tiers 26–66). B08 and B10 unlock at tier 1 and would change the boss choice of tiers 1–5, so they were left out. Seeds and fixed results of tiers 1–5 therefore stay as they were.
- From tier 6 on, the slot candidate lists grow, so the same seed draws other monster kinds and the original monsters lose share (for example the desert melee slot goes from one kind to one of seven). Placement, roles, elites and rewards do not change, as `MonsterSpawnTableTests` checks.
- The codex holds 136 monsters (120 regular + 16 bosses). Kills are recorded per id, so existing saves carry on unchanged.

### 11.6 Deferred and limits

No roster entry has deferred mechanics; the daily routine ships without `--allow-deferred`. Known limits:

- Walls and cages only stop the hero walking. Enemies pass, and the hero's pathfinding does not know them, so it stops at a wall and attacks it or waits for it to expire.
- Vision loss shows as the 7 m perception cap and a HUD status; there is no screen fog.
- The Hunt Edict does not seek refuge spots (the flood tide's rocks), as with the existing Hymn of Silence refuges.
- Flying changes movement and drawing height only; the hero's ground effects still hit flyers.
- Decoys mimic only the owner's non-projectile phase-1 attacks.
