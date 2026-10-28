# Skill presentation juice

Updated: 2026-10-18 · Written: 2026-10-18 · [한국어](Skill_Juice_20261018.md)

The combat presentation of all 30 active skills of the D4 skill tree is heavier and more fun. It is a pure presentation change that leaves the simulation and saved data alone. Casts, hits and lasting effects all go through the same path (`WorldView`, `WorldFx`), so the battle screen and the skill window's example fights get it together.

## State before the change

- A skill's presentation was a thin line, a ring and one or two bursts. Nothing showed while a cast wound up, a struck enemy had no per-skill reaction, and only Titan's Judgment shook the screen.
- An arrow was one thin line and a globe a flat ball. Effects that stay on an area (blizzard, arrow rain, rift ward) were one line on the floor.
- The presentation did not tell modifiers apart: modifiers with the same radius or cast kind looked alike. Radius, angle and element colour that a modifier changes are still honoured.

## Layers added

1. **Charge-up**: while a cast winds up, motes gather on the hero, and an area skill lights a rune circle on the ground (skills with a wind-up).
2. **Release**: rings, speed lines and a star, a per-skill camera shake, and a hero flash for buffs. Shakes follow the existing motion setting (reduce motion).
3. **Hit**: every enemy a skill strikes gets a star in the skill's colour (or the hit's element), crits add speed lines, and the enemy is shoved briefly away from the attack (the view moves; the simulation position does not).
4. **Lasting effects**: element auras and light trails on arrows, globes and spectres, weather particles for blizzard, arrow rain, vortex, ward and trap mist, a falling meteor, the three elements circling the hero (M18), giant form and the element aura of a loaded bow.
5. **Simulation notices**: a star or ring when a bleed or poison takes hold, a hook pulls, a bleed is reclaimed, an element reaction fires or the frost orb returns.

## Library additions

- Textures `hit_star` and `speed_lines`, written by `tools/generate_fx_textures.py` and listed in the provenance manifest.
- Presets `star`, `charge`, `lines`, `lines.ground`, `ring`, `pop`: white based, so `Burst`'s tint paints them per element or skill.
- `WorldFx.Streak` (ribbon), `WorldFx.Aura` (element aura), `WorldFx.Zone` (snow, arrow rain, vortex, ward, mist). All are pooled or die with their anchor.
- Timed follow-ups run on the presentation clock, so they pause and slow with the run, and are dropped with the world.

## Presentation per skill

### Warrior

| ID | skill | presentation |
|---|---|---|
| `W09` | Raking Wound | Three rakes (two delayed), a red star and blood when the bleed lands, blood and a heal sparkle when it is reclaimed. |
| `W08` | Impaling Hook | A `flare` and `strike` at the hook (no ground ring); another star and dust when the target is pulled in. |
| `W01` | Whirlwind | Two opposed crescents plus a reach ring and dust on every spin tick. |
| `W03` | Crushing Blow | A gold and a white crescent, a crack, ring and dust where it lands, charge-up motes. |
| `W05` | Iron Wall | A gold ring, star and sparks and a hero flash; a star pops whenever the shield takes a hit. |
| `W06` | Battle Shout | Two rings from orange to white, speed lines, embers, a hero flash and a shake. |
| `W02` | Leap Slam | A streak along the leap, two landing rings, ground speed lines, rubble, a crack and a camera shake. |
| `W04` | Ground Slam | Two rings and ground speed lines, six rim chunks thrown in turn, a cracked floor; a ground rune shows the charge. |
| `W17` | War of the Ancestors | A `flare` and speed lines on the hero (no ground ring or rune); each ancestor gets a spirit aura, a summoning rune and a star, and a star on every swing. |
| `W18` | Titan's Judgment | A ground rune shows the charge, then three big rings, eight rim dust jets, a star, a white flash and a crack; shake 1.0; Giant Form (W18-2) grows the hero. |

### Ranger

| ID | skill | presentation |
|---|---|---|
| `A07` | Successive Shots | A light trail per arrow, a star at the bow and a star on impact. |
| `A14` | Forking Arrow | A light trail on the arrows, a star where they fork and a star at the bow. |
| `A01` | Piercing Shot | A star and sparks at the bow and a straight light ribbon along the arrow line. |
| `A02` | Multishot | A fan crescent (its angle follows the modifier) and sparks at the bow. |
| `A04` | Retreat Leap | A streak, dust and rings between take-off and landing; smoke for Smoke Cover, violet afterimages for Shadow Step. |
| `A03` | Venom Trap | A set-down ring with poison mist (snow for Frost Snare); a poison burst, star and small shake when it springs. |
| `A06` | Elemental Imbuement | A hero flash, ring and star; an aura in the element of the loaded arrows stays on the bow. |
| `A05` | Hunter's Mark | A `flare` and a small star over the head when the mark lands (no ground ring); the mark then pulses. |
| `A17` | Killing Rain | A ground rune shows the charge, a ring and speed lines, streaking arrows falling over the area, dust and a star at each impact. |
| `A18` | Afterimage March | Speed lines, a `flare` and a hero flash (no ground ring); a shadow aura and a summon star on each afterimage. |

### Mage

| ID | skill | presentation |
|---|---|---|
| `M07` | Elemental Lance | A thick light ribbon and bolt in the colour of the element, sparks along it and a star at the end. |
| `M01` | Fireball | A globe with a fire aura and a cast star; a blast ring, speed lines, scorch and shake. The meteor falls from the sky as a ball of fire. |
| `M02` | Blizzard | Snow falls over the zone with frost mist; a ring and speed lines spread out. |
| `M03` | Chain Lightning | A fat coloured bolt under a thin white one, a star where it strikes. |
| `M06` | Frost Nova | A ring and ground speed lines, ten ice spikes erupting in turn; a ground rune shows the charge. |
| `M04` | Teleport | The old spot folds in, the new one pops with a star, ring and speed lines. |
| `M08` | Firewall | An ignition ring and embers, bigger flames. |
| `M11` | Capacitor Orb | An orb with a lightning aura and a placing star, a star on every discharge. |
| `M17` | Triune Collapse | Each stage is a fire blast, ice spikes, then a bolt from the sky, with a star and a shake; a ground rune shows the charge. |
| `M18` | Sage Incarnate | Fire, ice and lightning rings spread in turn; three elements orbit the hero for the whole incarnation. |

## Verification

- The developer-only `-hellscriptSkillShowcaseSmoke` records the 30 skills (all 84 modifiers with `-hellscriptShowcaseModifiers`) frame by frame in the skill window's example fights. Before and after were compared on the same scenes; 113 of 114 recordings exist (`W06-2` does not cast in its example fight). The recordings are not committed.
- Unity Edit Mode: `SkillJuiceTests` (all 30 styles, presets and textures, auras and zones dying with their anchor, ribbon pooling, the shove, timed follow-ups), `WorldFxTests`, `HookPresentationTests` and `RakingWoundPresentationTests`. The full Edit Mode suite and the smoke bundle were not run.
- `generate_fx_textures.py --check` passes for all 36 textures.

## 2026-10-28 follow-up: bigger and clearer in a restrained League-of-Legends style, no floor marks for single-target skills

User report: in battle the skill effects are hard to see and feel weak, and single-target attacks that have no area still show a ground range mark, which is confusing. Then a request for the League of Legends look: cool and flashy, but restrained.

- **Cause confirmed**: the real battle camera frames 10-12 m vertically while the recordings above were a 5 m close-up. The same effect is half as large in play, and additive white on a pale floor has little contrast. The dev recording now takes `-hellscriptShowcaseHalfHeight 10.5` to check at the real distance.
- **Visual language**: a small bright core, a thin ring and fine sparks. Big soft flash blobs wash out on a pale floor, so they are not used. The new presets `ripple` (a thin camera-facing ring), `strike` (a hit: hot core, ring, 10 fine sparks) and `flare` (a cast flash: core, ring, 8 sparks) are white based and painted per skill or element.
- **Deep, vivid colour (League style)**: on a pale floor light added with additive blending washes out to white. Each preset (`ring`, `ripple`, `strike`, `flare`, `star`, `lines`, `pop` and the rune decal) now has a normal-blended colour layer underneath (`WorldFx.Solid`), and beams, slashes and ribbons take a `solid` option (a coloured band with a white additive core). One colour per class or element: warrior crimson and orange, ranger rose pink, mage violet, the shield cobalt, spirits cyan; fire red-orange, cold azure, lightning indigo, poison lime, shadow violet, physical orange. The gold and white wording in the tables above is superseded by this palette.
- **Hierarchy**: an ordinary hit is one `strike`. The comic star is kept for crits and boss hits and the long speed lines for ultimates and big buffs. The struck enemy glows in the skill's colour for 0.16 s (0.11 s for a plain hit) and a crit adds a small camera shake. Each cast gets a small `flare`, release shake x1.15. Charge motes x1.5; the bodies and auras of arrows, globes, lightning and the capacitor orb and the lance/chain-lightning beams are larger (a beam is a coloured band with a white core). The `star` preset has a dark pad behind it so it reads on a pale floor.
- **Single target**: `WorldView.SingleTarget` (Impaling Hook W08, War of the Ancestors W17, Piercing Shot A01, Hunter's Mark A05, Rapid Shots A07, Afterimage March A18, Chain Lightning M03, Elemental Lance M07) draws no charge ground rune, ground ring or ground speed lines. Their weight goes into a `flare` and `strike` over the target. Area skills keep their thin ground ring.
- **Checks**: two more `SkillJuiceTests` (only area skills may wind up with a ground rune; casting a single-target skill leaves no ground preset in the pool, with the area skill W18 as the positive control) and the new presets in the preset test. No physical-device or performance measurement.

## Not done

- Hit-stop: it would desynchronise the view from the simulation.
- Hero body animation and equipment effects are unchanged.
- No physical-device performance measurement. Particle counts halve on the mobile quality as before and follow the per-preset caps, but auras and zones are new particle systems per cast and need a device measurement.
