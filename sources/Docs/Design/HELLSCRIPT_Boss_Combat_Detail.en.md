# Five Bosses: Combat Detail Test Plan

Updated: 2026-09-28 · [한국어](HELLSCRIPT_Boss_Combat_Detail.md) · first written 2026-09-09 (D3 implementation), updated 2026-09-28 for five bosses with two-phase kits

This turns the existing values of [content catalog §13](HELLSCRIPT_Content_Catalog.md#13-보스-5종), [implementation plan 16.4](HELLSCRIPT_Unity_Implementation_Plan.md) and [completion checklist 4.3](HELLSCRIPT_Gameplay_Completion_Checklist.md#43-정예-6종과-보스-3종), together with the HP transition and interruption rules, into executable units. Missing preparation times, ranges, priorities and placement values use the test values below. These are not final balance values.

## Common behaviour and phase change

Every boss has six patterns: slots 1–3 are phase-1 patterns and slots 4–6 are phase-2 patterns. The cooldown of the basic attack and of each pattern is stored per slot and counts from the start of preparation. The initial spawn delay applies the generator's existing 1–2 seconds to every cooldown. No other main action starts while a preparation, charge, held beam or blast chain is running. In phase 1 the boss rotates through slots 1–3 and skips any candidate that is out of range, lacks sight, a clear line or space, or is on cooldown. If no pattern is usable it uses the basic attack. The choice depends only on the saved rotation cursor and never draws from the combat random stream. Perception is 14 m with wall checks, and the boss remembers the hero for 4 seconds after losing sight.

When HP drops to 50% or below even once, the transition is booked and applied the next time the boss chooses an action. Healing afterwards neither cancels it nor returns the boss to phase 1. An action already in progress keeps its hit count and shape. At the transition the boss roars: it warns a 4 m radius around itself for 1.5 seconds and then releases one D80% shadow shockwave. From the tick after the roar is announced until it releases, the boss takes no damage and builds no stagger. Damage already resolving in the announcing tick still lands. After the roar the first cooldowns of the phase-2 patterns open at 1, 3 and 5 seconds. In phase 2 the boss tries slots 4–6 first, then slots 1–3, then the basic attack.

The save holds 7 cooldown slots and the rotation cursor. An older save with 3 slots keeps its values and grows to 7; the new slots take the largest existing value. The combat action version is 6. The follow-up flag, fixed direction, target and blast layout are stored when preparation starts. Damage is decided at the actual launch, charge start or effect creation.

Stagger interrupts the action in progress and keeps the remaining cooldowns, which keep running during the stagger. A hook already launched and a beam or follow-up blast already created keep their remaining lifetime. Attacks and follow-ups that were only being prepared are cancelled. The existing control 100 → 3 s stagger → 10 s immunity rule is unchanged. Same-tick deaths and outcomes follow the existing unified settlement.

Boss ground warnings use the same gauge as regular enemies. The footprint shows faintly, fills from the inside over the preparation, and the damage applies on the tick it is full, when the footprint flashes once. Boss warnings are brighter and bolder than regular ones. Circles that land one after another after the release (Grave Toll, Triple Eruption, the blast chain) count one gauge from the moment the boss started warning until each circle lands, so they do not start over at the release. An attack that pulls first and bites later, such as the Devouring Pull, keeps one unbroken gauge up to the bite. A followup warned after the first hit has landed, such as the slam's second hit or the second charge, starts a new gauge.

The basic melee attack has 3 m range, a 120° front arc, 0.6 s preparation, D100% and a 2 s cooldown. The common boss stats are unchanged. Per-boss HP and attack multipliers and movement live in the boss profile table. The Chorister holds its position while the hero is within 8 m and otherwise approaches at 35% of the normal boss speed. The Matriarch holds within 7 m and approaches at 50%. The other bosses approach at normal speed.

The Executioner appears from stage 1, the Chorister from stage 11, the Devourer from stage 21, the Dune Tyrant from stage 31 and the Hoarfrost Matriarch from stage 41. The stages can be changed in ContentUnlocks.json. With three bosses available the boss choice is exactly the same as before, and the choice still uses a single random draw.

## Graveyard Executioner

- Slam: 4 m range and sector radius, 120° front arc, 1.2 s preparation, D200%, 6 s cooldown. The position and direction are fixed when preparation starts.
- A slam started in the late phase re-warns the same position and direction for 0.7 s and then lands one more D200% hit. Both hits keep the same original attack ID but are separate damage events. If the boss is staggered during the second preparation, the follow-up is cancelled.
- Hook: a physical projectile with a 9 s cooldown, 1 s preparation, 10 m range, 1 m width, 12 m/s speed and D100%. It keeps travelling after its caster dies. The projectile and its warning use the same wall end point.
- On a hit the hero is pulled up to 2 m toward the saved launch position, stopping before walkable walls and living bodies. The pull interrupts the hero's action in progress without refunding cost or cooldown. If the hero's current position during a movement skill is not valid walkable or landing ground, only the damage applies and the pull is skipped. The pull never lands the hero inside a wall.
- Reaping Sweep (phase 1): 3.5 m range and radius, 220° front arc, 1 s preparation, D120%, 8 s cooldown.
- Execution Leap (phase 2): 9 m range, 1.2 s preparation, 10 s cooldown. The boss leaps to free walkable ground 1.2 m short of the hero and hits a 3 m radius for D160%. If it cannot land, it does not leap.
- Chain Whirl (phase 2): a 4 m radius around the boss, 1 s preparation, 11 s cooldown. It hits for D50% every 0.5 s over 1.5 s while the boss holds its position.
- Grave Toll (phase 2): 9 m range, 1.2 s preparation, 12 s cooldown. Three 2 m circles at 2.5, 5 and 7.5 m along the aim are warned and deal D100% shadow damage at 0, 0.4 and 0.8 s.

## Chorister of the End

- Summon: 14 s cooldown, 0.8 s preparation, 12 m hero perception. It summons up to 4 adds at a time so that at most 8 of its own adds are alive.
- Fixed candidates on rings of 3, 4.5 and 6 m around the boss are checked in order for walkability, landing, reachability, sight to the boss and spacing from existing bodies. Candidates are fixed when preparation starts and checked again at release; an invalid point is never silently moved. If no point is available, the boss does not prepare and re-checks after 1 second.
- Adds use N01's basic behaviour and store the summoner ID. They give no experience, kill gauge, gold, items or ordinary corpses, but they count in kills and the combat log. While at least one of its own adds lives, the boss takes 20% less damage; adds from other events do not count.
- Focused Beam: 8 s cooldown, 1.5 s preparation, 1.5 m width, 12 m length. The direction is fixed at preparation start and the end point at the wall is stored. After creation it deals D37.5% shadow damage 8 times, every 0.25 s for 2 s, the first 0.25 s after activation. The hero's centre must be inside the beam width. The boss holds while the beam lasts; if it is staggered or dies, the created beam keeps hitting until the unified combat ends.
- Dirge Ring (phase 1): a ring from 2 m to 4.5 m around the boss, 1.2 s preparation, 9 s cooldown, D120% shadow damage. Standing close to the boss or outside the ring avoids it.
- Requiem Choir (phase 2): 12 m range, 1.6 s preparation, 12 s cooldown. Three 1 m wide beams from 2 to 12 m at −30°, 0° and +30° are held for 1.5 s, dealing D30% every 0.25 s while the boss holds.
- Hymn of Silence (phase 2): a 5 m radius around the boss, 1.6 s preparation, 14 s cooldown, D160%. Up to two 1 m refuges 3.2 m from the boss are shown; inside them the hymn deals no damage.
- Lament Orbs (phase 2): 1.2 s preparation, 9 s cooldown. Six orbs are fired in all directions (8 m/s, 10 m, 0.35 m radius); at most one hits per volley.

## Devourer of the Rift

- Charge: 7 s cooldown, 1 s preparation, 10 m/s, up to 8 m, 1 m width, D150%. The movement segment is tested against the hero radius once per charge. The boss body stops at walls with its 1.2 m walking radius.
- A charge started in the late phase moves twice. When the first charge ends, it re-aims at the last seen position and warns for 0.8 s. HP dropping to 50% during the first charge does not insert a second charge.
- Chained Blasts: 12 s cooldown, 1.3 s preparation. The hero's position and the two points 3.5 m to either side are candidates, and a direction that fits the terrain is searched. All three circles are warned from the start and each deals D150% shadow damage once in a 2.5 m radius at 1.3, 1.65 and 2.0 s. After preparation the three effects exist independently, so a later stagger does not remove the remaining blasts.
- Two 0.65 m refuges are at least 1.5 m apart and outside every blast circle. Reachability before the first blast is checked with the hero's real walking speed, current slow and the 0.2 s decision interval, along with walls, unlandable ground, body overlap and hazards already known at warning time. The search is bounded; if no valid layout exists the cooldown is not spent and the boss re-checks after 1 second.
- The refuges are safe only from this blast chain. They are not protection zones against attacks other enemies create later, and the hero is not moved automatically if its rules choose to keep attacking. They are drawn as separate outlines during preparation. A stored layout, path and blast order is never re-rolled on restart.
- Maw Snap (phase 1): 5 m range and radius, 60° front arc, 0.7 s preparation, 7 s cooldown, D140%.
- Rift Tear (phase 2): 12 m range, 1.2 s preparation, 11 s cooldown. A 1.6 m wide straight tear stays for 3 s and deals D35% shadow damage every 0.5 s.
- Shard Storm (phase 2): 1 s preparation, 10 s cooldown. Eight shards are fired in all directions (10 m/s, 10 m); at most one hits.
- Devouring Pull (phase 2): 8 m range, 1 s preparation, 12 s cooldown. A hero in sight is pulled 2.5 m toward the boss, then after a 0.8 s re-warning the boss bites a 3 m radius for D140%.

## Dune Tyrant (new)

A giant scorpion-worm half buried in sand. HP ×1.15, attack ×1.05; appears from stage 31.

- Tail Lash (phase 1): a 6 m × 2.2 m line, 0.9 s preparation, 7 s cooldown, D130%.
- Venom Spray (phase 1): 9 m range, 1 s preparation, 8 s cooldown. Five venom globs are fired over ±24° at 11 m/s; at most one hits (D90% poison).
- Burrow Strike (phase 1): 10 m range, 1.4 s preparation, 11 s cooldown. It burrows and surfaces on walkable ground 1.2 m short of the hero, hitting a 2.8 m radius for D150%.
- Quicksand Maelstrom (phase 2): a ring from 1.5 m to 5 m around the boss stays for 3 s, dealing D30% every 0.5 s and slowing the hero. 1.3 s preparation, 13 s cooldown.
- Scarab Swarm (phase 2): 0.9 s preparation, 15 s cooldown. It summons three Dune Marauder (N13) adds. At most 8 of its own adds live, and adds give no rewards.
- Triple Eruption (phase 2): 12 m range, 1.2 s preparation, 11 s cooldown. Three 2.4 m circles erupt 2.4 m apart along the hero's recent travel at 0, 0.4 and 0.8 s (D120% fire).

## Hoarfrost Matriarch (new)

A frost priestess crowned with ice spikes. HP ×0.9, attack ×1.0; appears from stage 41 and keeps a 7 m distance.

- Ice Lance (phase 1): an 11 m × 1.6 m line, 1.1 s preparation, 7 s cooldown, D130% cold, slows the hero.
- Frost Nova (phase 1): a 4.5 m radius around the boss, 1.4 s preparation, 10 s cooldown, D140% cold, slows the hero.
- Glacial Spikes (phase 1): 2 m circles at one third, two thirds and the full distance to the hero (3–10 m) are warned and deal D130% cold. 1.2 s preparation, 9 s cooldown.
- Blizzard Veil (phase 2): four 1.8 m circles on the diagonals 2.6 m around the hero stay for 3 s, dealing D30% cold every 0.5 s and slowing. 1.3 s preparation, 13 s cooldown.
- Frozen Sentinels (phase 2): 0.9 s preparation, 15 s cooldown. It summons three Frostbitten Revenant (N19) adds.
- Shatter Fan (phase 2): 10 m range, 1.1 s preparation, 9 s cooldown. Seven ice shards are fired over ±30° at 11 m/s; at most one hits (D100% cold).

## Verification

Test each pattern's preparation, cooldown, shape, damage, interruption, lifetime after death and app restore. Cover crossing 50% HP above/below and during preparation; the hook against walls, bodies and movement skills; add caps, ownership and no rewards; the beam's 8 ticks, walls and fixed direction; and the blast chain's three timings, two refuge paths and retry when no layout exists. Also check that no phase-2 pattern is chosen in phase 1, invulnerability and stagger immunity during the roar, the cooldown order right after the transition, leap and burrow landing points, and that the forecast equals the real damage for every pattern. Natively, confirm real app restarts between follow-up hits, during a held beam and between chained blasts. Keep the full rift flow and three game speeds in the checks, and keep Android performance and physical touch separate.
