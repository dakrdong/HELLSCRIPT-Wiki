# Hunt Edict quick presets

Updated: 2026-09-24

Each configurable item offers named recipes and a final **Custom Settings** choice. Selecting a recipe updates only that item's detached draft; **Save** commits through the existing transaction. Custom Settings preserves current values and exposes every detailed option. The preset dialog describes each recipe before selection.

## Coverage and behavior

- The 25 global groups cover all 130 existing options. There are also 54 active/ultimate skill scopes, three class-specific basic attack scopes, and one common attack-order scope: 83 scopes and 249 recipes across the three classes.
- Only equipped skills appear in the UI. Always-on passives have no configurable edict policy and receive no artificial selector.
- A recipe replaces every field in its scope, filling unspecified fields from the option catalog defaults. It preserves other groups, skill slots, investments and unrelated skill policies. Required survival references resolve to an eligible equipped skill; absent skills remain unassigned.
- Custom Settings is always last. Choosing it alone never changes values or makes the draft dirty. Existing values that match no recipe initially show Current Settings with details collapsed. Selecting the final Custom Settings entry explicitly expands them. Search opens the matching group in Custom Settings so every matched option remains reachable.
- Reopening derives the label from actual values; Custom view state is not a new account field. Saving, reverting, five-slot presets and HED5 sharing use the existing owners. A recipe name is never a second combat policy or a replacement for the stored settings.
- Whirlwind's **Quick center entry** uses a skill-local `CENTER` movement policy, moving toward the nearest legal gap around the centroid of observed enemies within five meters of the selected target. **Survival-first combat** uses the existing edge orbit. Both retain hazard, pursuit and navigation gates; they do not change global positioning or movement speed.

## Ownership

`HuntEdictQuickPresets` reads `Resources/HuntEdictQuickPresets.json` for curated recipes. Native skill policies reuse bilingual choices from `Resources/Data/ClassSkills.json`; Mana Charging recipes bind both its use policy and recovery goal. `HuntEdictWindow.QuickPresets` presents the selector using existing shared buttons, theme, modal and scroll owners. `HuntEdictEditSession` owns the draft; `GameStore.CommitHuntEdict` owns saving. No new save fields, rank mutations, currency operations, textures or window framework are introduced.

Sources: [curated recipes](../../Assets/HELLSCRIPT/Resources/HuntEdictQuickPresets.json), [scope/application owner](../../Assets/HELLSCRIPT/Runtime/Core/HuntEdictQuickPresets.cs), [UI adapter](../../Assets/HELLSCRIPT/Runtime/Presentation/HuntEdictWindow.QuickPresets.cs), [Whirlwind movement](../../Assets/HELLSCRIPT/Runtime/Core/CombatSimulation.EdictWhirlwindMovement.cs).

## Recipe catalog

Every row below also offers Custom Settings as its final entry.

| Item | Recipes |
| --- | --- |
| Combat position & distance (6) | Balanced engagement · Push into the pack · Keep your distance |
| Encirclement & gathering (5) | Fight the current pack · Escape encirclement · Gather small packs |
| Target selection (5) | Nearest enemy first · Dangerous enemies first · Clear dense packs |
| Target retention & pursuit (7) | Balanced pursuit · Short pursuit · Finish one target |
| Action priority (1) | Survive, then fight · Fight, then collect · Collect before advancing |
| Emergencies & re-engagement (6) | Survival first · Balanced survival · Emergency retreat |
| Automatic potion (13) | Balanced supplies · Extra survival supplies · Conserve potions |
| Avoidance by damage type (6) | Avoid all warnings · Avoid heavy damage · Maintain attacks |
| Special dangers & avoidance (6) | Avoid control and overlapping damage · Walk away first · Finish the current attack |
| Survival tools & reserves (7) | Retreat on foot · Defend, then escape · Escape skill first |
| Equipment pickup by rarity (5) | Collect all after combat · Rare and better · Focus on legendary and set items |
| Equipment filters & exceptions (6) | Consider all equipment · My class first · Focus on weapons |
| Gold & material pickup (5) | Balanced collection · Prioritize resources · Collect along the way |
| Pickup movement & completion (3) | Safe collection · Nearby loot only · Thorough collection |
| Insufficient bag space (5) | Return when full · Make room early · Replace lower-value items |
| Automatic cleanup (7) | Keep everything · Sell normal and magic items · Salvage normal and magic items |
| Equipment protection (4) | Protect invested equipment · Protect effects and sets too · Use base protections |
| Cleanup failure response (2) | Keep items and stop · Wait for manual cleanup · Continue with limited loot |
| Exploration & rift progress (6) | Follow the right wall · Rush the boss · Explore thoroughly |
| Normal & sealed chests (5) | Open chests on the way · Prioritize chests · Skip chest detours |
| Cursed chests (4) | Avoid cursed chests · Attempt with a safety margin · Attempt after combat |
| Shrines (4) | Use discovered shrines · Use shrines when needed · Skip shrines |
| Repeat & next stage (3) | One run at a time · Repeat the same stage · Climb stages |
| Stop conditions & goals (7) | A short five-run session · A 30-minute session · No time or run limit |
| Next-run preparation (2) | Standard preparation · Require bag space · Quick next run |
| Whirlwind | Quick center entry · Survival-first combat · Stationary spin |
| Leap Slam | Land in the pack · Reserve for escape · Attack and escape |
| Crushing Blow | Crush the pack · Focus elite targets · Crush with a charge |
| Ground Slam | Control packs · Interrupt dangerous casts · Cover a retreat |
| Iron Wall | Prevent damage · Extend protection · Emergency protection |
| Battle Shout | Resource and damage support · Focus on resource recovery · Prepare for the boss |
| Piercing Shot | Pierce without waiting · Pierce multiple enemies · Pierce priority enemies |
| Multishot | Clear packs · Focus one target · Fire from range |
| Venom Trap | Trap the pack · Refresh trap positions · Trap pursuing enemies |
| Retreat Leap | Restore a safe distance · Lure onto your trap · Reserve for survival |
| Hunter's Mark | Maintain a priority mark · Mark each new target · Mark high-health targets |
| Shadow Arrow | Empower everyday shots · Empower elite piercing · Spread poison with volleys |
| Fireball | Steady fire · Maximize explosions · Focus on elites |
| Blizzard | Cover new ground · Maintain Blizzard on target · Cover approach paths |
| Chain Lightning | Attack isolated enemies · Focus linked packs · Conserve charges |
| Teleport | Reserve for survival · Also adjust distance · Escape to Open Space |
| Elemental Shield | Prevent damage · Chain shields · Emergency protection |
| Frost Nova | Freeze in place · Approach and freeze · Set up chain charges |
| Brand of Challenge | Default edict · Below 50% resource · React to a windup |
| Impaling Hook | Default edict · Use when available · Use against distant enemies |
| Raking Wound | Default edict · Spread to new targets · Use against elites and bosses |
| Blood Reclamation | Default edict · Cash bleed out early · Time bleed expiry or healing |
| Resolute Advance | Default edict · Use when available · Use against distant enemies |
| Iron Stance | Default edict · React to a windup · Use at low health |
| Battlefield Ring | Default edict · Use when available · Wait for several targets |
| Tremor Wave | Default edict · Use when available · Wait for several targets |
| Breath Before Battle | Default edict · Below 30% resource · Below 75% resource |
| Guardian's Vow | Default edict · When no shield remains · Reserve for cleansing slow |
| War of the Ancestors | Default edict · Use against elites and bosses · Use at low health |
| Titan's Judgment | Default edict · Wait for several targets · Align with the shout |
| Successive Shots | Default edict · Use when available · Leave 40% resource after casting |
| Patient Shot | Default edict · Use when available · Require a mark and distance |
| Briar Trap | Default edict · Use when available · Prefer new control |
| Frost Snare | Default edict · Use when available · Freeze after poison |
| Decoy Projection | Default edict · React to nearby enemies · Wait for several targets |
| Smoke Cover | Default edict · React to nearby enemies · Use at low health |
| Venom Arrow | Default edict · Spread to new targets · Focus the marked target |
| Forking Arrow | Default edict · Use when available · Wait for several targets |
| Watch Ballista | Default edict · Use when available · Use against elites and bosses |
| Hunt Preparation | Default edict · Below 30% resource · Before a paid skill |
| Killing Rain | Default edict · Use against elites and bosses · Wait for a controlled group |
| Shadow Pursuit | Default edict · Use when available · Leave 70% resource after casting |
| Ember Lance | Default edict · Use when available · Wait for several targets |
| Firewall | Default edict · Use when available · Wait for control |
| Glacial Lance | Default edict · Use when available · Freeze after a slow |
| Frost Globe | Default edict · Use when available · Wait for control |
| Capacitor Orb | Default edict · Use when available · After Chain Lightning is ready |
| Storm Spear | Default edict · Use when available · After Chain Lightning hits |
| Mana Reclaim | Standard recovery · Quick recovery in place · Retreat and recharge |
| Rift Ward | Default edict · When no shield remains · Use at low health |
| Elemental Compass | Default edict · Below 30% resource · Use with three elements |
| Magnetic Vortex | Default edict · Use when available · Wait for several targets |
| Triune Collapse | Default edict · Use against elites and bosses · Wait for elemental empowerment |
| Sage Incarnate | Default edict · Use against elites and bosses · Prepare a three-element cycle |
| Warrior basic attack | Fill between skills · Recover resources · Finish weak enemies |
| Ranger basic attack | Fill between skills · Recover resources · Finish weak enemies |
| Mage basic attack | Fill between skills · Recover resources · Finish weak enemies |
| Common attack order | Equipment slot order · Ultimate first · Basic attack first |

## Validation

- [477 focused Edit Mode cases passed](HuntEdictQuickPresetEvidence/editmode.xml), with zero failures or skips. These cover all recipe values, scope isolation, idempotence, sharing, saving/reverting, real Whirlwind movement and existing class-skill behavior. This is not a full-suite result.
- [macOS development build succeeded](HuntEdictQuickPresetEvidence/build.txt) with zero compile errors.
- [Native pointer acceptance passed](HuntEdictQuickPresetEvidence/runtime.txt): all 25 global groups expose all 130 options in Custom Settings; presets hide details; opening Custom Settings preserves values/dirty state; revert, save/reopen, common order and native skill policies work. Five frame sizes, Korean/English and 100%/150% text passed; an open dialog also survived rotation and language/text-size changes.
- [A fresh native process reloaded the saved Whirlwind and survival presets.](HuntEdictQuickPresetEvidence/restart.txt) No phone/tablet or mobile performance validation is claimed.

![Whirlwind choices in portrait at 150% Korean text](HuntEdictQuickPresetEvidence/whirlwind-choices-440x956-ko.png)

![Whirlwind choices in landscape at 150% English text](HuntEdictQuickPresetEvidence/whirlwind-choices-956x440-en.png)

![Global survival preset](HuntEdictQuickPresetEvidence/global-survival-ko.png)
