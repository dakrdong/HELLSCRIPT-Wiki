# Live combat examples for skill presets

Updated: 2026-09-30 · [한국어](Skill_Preset_Combat_Previews.md)

All 54 active and ultimate skills offer 122 equipment-independent presets: 41 Warrior, 40 Ranger and 41 Mage. Preset tabs now show an independent battle using the production combat code and existing character/enemy assets, animation and effects. Custom Settings displays detailed controls without an example.

## Playback and saving

- **No duration limit or automatic loop.** Restart, Pause/Play and 0.5×/1×/2× control the example. Victory or defeat retains the last state until Restart.
- Restart restores the complete prepared snapshot and random stream, replacing old projectiles, areas, summons, shields and presentation subscriptions.
- Browsing a different preset starts its example. Resize, language/text-scale changes and activation saves preserve the current example session.
- Browsing and playback never save. Activate Preset and editing Custom Settings use the existing policy transaction, preserving pending allocation/global edits.
- Hidden pages, covering dialogs and background applications suspend the example. Leaving the page or closing the window releases combat, camera, texture and subscriptions.

## Production combat decisions

Fixtures use a level-40 hero, rank-1 equipped skills and fixed normal equipment, without runes, gems, legendary/set items or unnecessary passives. Each scenario fixes HP, resources, cooldowns, enemy types/positions/arrival times and random seed. The hero starts on the left and enemies on the right.

Required initial bleed, poison, slow, shield or trap states are prepared by real casts and simulation ticks. Subsequent casts, movement, hits and recovery use the existing automatic combat policies. No hidden manual casts or scripted enemy choreography fabricate preset behavior. Companion skills and required global settings are shown. The four ordinary-active/one-ultimate slot limit is retained.

When skill-first avoidance cannot use its preferred escape, an explicitly designated preventive defense may answer the detected incoming hit before walking. Walk-first behavior and actual shield timing, chaining, resource and cooldown restrictions remain intact. Mana Reclaim attempts retreat positioning for at most two seconds, then attempts recovery subject to real danger/movement cancellation. Area aiming includes the eligible elite/marked/controlled target; Decoy Projection counts three regular enemies threatening the hero. Damage coefficients and cooldown balance are unchanged.

Wide layouts place description, conditions and live feedback in a left column, with the larger battle viewport on the right. Portrait places the battle first and information below. A compact fixed activation button sits beside the skill heading, removing the separate action row from the content. Restart and Pause/Play use icons; speeds use compact `½×`, `1×` and `2×` labels.

Compact feedback shows actual HP, resources, shield, cooldown and active-effect skill icons. Captions come from observed events or real automatic readiness. Preset descriptions and scenario conditions support Korean and English.

## Ownership and isolation

| Component | Responsibility |
| --- | --- |
| `SkillPresetScenario` | Authored conditions, companions, preparation, arrivals and observations; exact 54-skill/122-preset coverage. |
| `CombatPreviewSession` | Detached in-memory account and battle snapshot, fixed 20Hz simulation, speed, pause and restart. |
| `CombatPresentationContext` | Explicit combat, camera, time, hero and isolated position/layer supplied to `WorldView`. |
| `SkillCombatPreviewPresenter` | One selected example, dedicated camera/RenderTexture, presentation capped at 30fps. |
| `HuntEdictWindow.CombatPreview` | Existing window theme/font/button/scroll adapter for controls and conditions. |

The production example path does not call training entry, training-record persistence or developer-only fixture creation. It cannot reference or mutate the real GameStore, account, rift, training record, ultimate cooldown or pending draft. It grants no rewards, currency, loot or experience. Lighting, post-processing and effects use an isolated position/layer without modifying the main battle's global lighting or shader settings.

This replaces the example area in the existing Hunt Edict window rather than introducing a new content window. Existing owners retain title, category navigation, fixed activation and equipped-skill docks. Basic attacks and common attack order retain their diagram component.

## Saved selections and sharing

Unchanged recipe values retain their ID. Changed recipes use new IDs. The explicit compatibility list contains 47 published retired IDs; these normalize to Custom Settings while preserving executable values. Arbitrary unknown IDs remain rejected. Strict HED4 and enclosing HED5 validation is preserved.

Sources: [presets](../../Assets/HELLSCRIPT/Resources/HuntEdictQuickPresets.json) · [scenario conditions](../../Assets/HELLSCRIPT/Resources/SkillPresetScenarios.json) · [session](../../Assets/HELLSCRIPT/Runtime/Core/CombatPreviewSession.cs) · [creation/restoration](../../Assets/HELLSCRIPT/Runtime/Core/CombatSimulation.SkillPreview.cs) · [presentation owner](../../Assets/HELLSCRIPT/Runtime/Presentation/SkillCombatPreviewPresenter.cs).

## Validation record

One full Edit Mode run on Unity 6000.6.0f1 reported **4,879 total, 4,708 passed, 171 failed, zero skipped**. The empty-passive-slot assertion and UI null-pattern issue accounted for 123 new failures. After repair, **all 140 scoped checks passed**, including automatic main-skill starts/releases for each of the 122 examples, waiting conditions, chained shields, complete snapshot/random restoration, account/draft isolation and old share-code compatibility.

**47 continuity/save/live-ops failures also reproduce on pre-feature `b3876e0c`.** The unchanged audio timing case passed both baseline and focused current-source checks. The full suite was not repeated and is not reported as entirely passing. [Full results, repairs and baseline comparison](SkillPresetCombatPreviewEvidence/validation-summary.json) · [140 scoped checks](SkillPresetCombatPreviewEvidence/focused-tests.json).

The macOS Development build has zero compilation errors. The player exercised real uGUI raycasts/pointer events for **54 skills / 122 rendered examples and activation saves**, Restart, Pause/Play and speeds, browsing/save separation, retained sessions and resource disposal. The complete native smoke ran once. Later viewport/icon/heading-button/shutdown changes received only focused presentation acceptance. The final player log contains no C# exceptions through shutdown.

**20 layouts** cover 440×956 portrait, 956×440 landscape and 1600×900 / 1600×1000 / 2100×900 PC, Korean/English and 100%/150% text. Tabs and activation remain outside content scrolling; compact controls remain accessible. Account comparison excludes only the independent monotonic town-autosave activity timestamp and compares every gameplay field and the exact pending draft. Shared UI ownership and all 11 UI-contract regressions passed.

- [Complete native acceptance](SkillPresetCombatPreviewEvidence/combat-preview-runtime.txt) · [Final scoped layout/input acceptance](SkillPresetCombatPreviewEvidence/combat-preview-frames.txt) · [Build and scope](SkillPresetCombatPreviewEvidence/build.txt) · [Verified source hashes](SkillPresetCombatPreviewEvidence/sources.json)
- [Landscape Venom Trap](SkillPresetCombatPreviewEvidence/combat-preview-A03.png) · [Mana Reclaim](SkillPresetCombatPreviewEvidence/combat-preview-M13.png) · [Portrait English / 150%](SkillPresetCombatPreviewEvidence/combat-preview-440x956-en-150.png) · [Short landscape](SkillPresetCombatPreviewEvidence/combat-preview-956x440-ko-100.png)

Physical mobile touch input and performance remain unverified. macOS synthetic pointer input and responsive frame sizes are not physical-device acceptance.

## All 122 presets and starting conditions

Every skill also offers a final Custom Settings tab. Rows without companions use the main skill and basic attacks.

| Skill | Preset | Starting condition | Companions |
| --- | --- | --- | --- |
| Whirlwind | Quick central entry | Move into the centre of the pack while spinning. |  |
| Whirlwind | Survival first | Spin while staying at the edge of the pack. |  |
| Whirlwind | Stationary spin | Spin in place in three-second bursts. |  |
| Leap Slam | Quick pack entry | Leap directly into a nearby pack of at least two enemies. |  |
| Leap Slam | Leap after spacing | Try to gain 6m of space for up to two seconds before leaping. Attack from the current distance if spacing fails. |  |
| Leap Slam | Reserve for escape | Reserve it for escaping to safety when the global survival condition is met. |  |
| Crushing Blow | Crush a pack | Aim the crushing strike through several enemies. |  |
| Crushing Blow | Strike strong enemies | Prioritise an elite or boss when choosing the strike direction. |  |
| Ground Slam | Control a pack | Stun at least two nearby enemies. |  |
| Ground Slam | Interrupt a threat | Interrupt an enemy preparing a dangerous cast within range. |  |
| Ground Slam | Cover a retreat | Control pursuers and gain distance in an emergency. |  |
| Iron Wall | Prepare for damage | Use a shield in anticipation of incoming damage. |  |
| Iron Wall | Chain shields | Wait for the other shield and leap defence to end before using it when needed. | Guardian's Vow |
| Iron Wall | Reserve for survival | Use it in a low-health emergency. |  |
| Battle Shout | Restore resources | Restore resources with a shout when paid attacks cannot be afforded. | Crushing Blow |
| Battle Shout | Empower strong-enemy fights | Empower attacks once an elite or boss enters attack range. | Crushing Blow |
| Piercing Shot | Quick single-target pierce | Fire a piercing shot even at a single enemy. |  |
| Piercing Shot | Pierce two or more | Fire once at least two enemies are on the actual line of fire. |  |
| Piercing Shot | Prioritise strong targets | Prioritise an elite or boss when aiming the piercing shot. |  |
| Multishot | Clear a pack | Fire toward several enemies within the fan-shaped attack area. |  |
| Multishot | Long-range fire | Gain distance while using multi-shot. |  |
| Venom Trap | Place in the pack | Place a poison trap in the centre of a group. |  |
| Venom Trap | Place under the target | Place a stationary trap at the target position. The trap does not follow its target. |  |
| Venom Trap | Place along the approach | Place a trap along the observed enemy approach path. |  |
| Retreat Leap | Restore safe distance | Retreat in a safe direction from approaching enemies. |  |
| Retreat Leap | Retreat toward an existing trap | Retreat toward an already placed poison trap. No special equipment is required. | Venom Trap |
| Retreat Leap | Reserve for escape | Reserve it until the global survival condition calls for an escape. |  |
| Hunter's Mark | Maintain a strong-target mark | Maintain a mark on an elite or boss. Reapplication respects the actual cooldown. |  |
| Hunter's Mark | Mark the current target | Mark the current attack target. A target switch still respects the cooldown. |  |
| Hunter's Mark | Mark a high-health target | Mark an enemy with high remaining health. |  |
| Shadow Arrow | Empower regular shots | Empower basic shots with shadow follow-up attacks. | Piercing Shot |
| Shadow Arrow | Elite piercing combination | Empower shots when piercing fire at an elite is ready. | Piercing Shot |
| Fireball | Steady fire | Start with one regular enemy and a separated elite. More regular enemies arrive after 3 seconds. |  |
| Fireball | Prioritise blast coverage | Start with one regular enemy and a separated elite. More regular enemies arrive after 3 seconds. |  |
| Fireball | Prioritise strong targets | Start with one regular enemy and a separated elite. More regular enemies arrive after 3 seconds. |  |
| Blizzard | Cover a new area | Place enemies inside and outside an existing blizzard to compare new coverage and recasting at the target. |  |
| Blizzard | Recast at the target | Place enemies inside and outside an existing blizzard to compare new coverage and recasting at the target. |  |
| Blizzard | Cover the approach | Place a stationary blizzard on the enemy approach path. |  |
| Chain Lightning | Attack isolated enemies | Use chain lightning even on a single isolated enemy. |  |
| Chain Lightning | Wait for linked enemies | Attack when enemies gather within the actual chaining distance. |  |
| Teleport | Reserve for emergencies | Teleport to safety when the global survival condition is met. |  |
| Teleport | Restore combat distance | Adjust combat distance even outside an emergency. |  |
| Teleport | Escape to a sparse area | Choose a low-density destination among candidates without predicted damage. |  |
| Elemental Shield | Prepare for incoming damage | Use the shield in anticipation of an attack. |  |
| Elemental Shield | Chain shields | Use it when needed after the other shield ends. | Rift Ward |
| Elemental Shield | Reserve for survival | Reserve it for a low-health emergency. |  |
| Frost Nova | Control danger in place | Control a nearby dangerous cast from the current position. |  |
| Frost Nova | Approach and freeze a pack | Approach safely and freeze at least two enemies within range. |  |
| Brand of Challenge | Maintain the brand | Refresh the brand as it expires on the same target. |  |
| Brand of Challenge | Brand when resources are low | Apply the brand when resources are at 50% or below. |  |
| Impaling Hook | Pull even nearby enemies | Pull the enemy when ready, even at close range. |  |
| Impaling Hook | Pull distant enemies | Wait for an enemy at least 4m away before pulling. |  |
| Raking Wound | Bleed packs or strong targets | Apply bleeding to a strong target or at least three nearby enemies. |  |
| Raking Wound | Bleed a single enemy | Use it even on one target without your bleeding effect. |  |
| Blood Reclamation | Reap bleeding immediately | Immediately reap your bleeding for extra damage and healing. | Raking Wound |
| Blood Reclamation | Reap near expiry or in danger | Reap when bleeding has at most one second left or health is at 40% or below. | Raking Wound |
| Resolute Advance | Advance even at close range | Pass through nearby enemies to attack and gain defence. |  |
| Resolute Advance | Approach distant enemies | Use it to approach enemies at least 3m away. |  |
| Iron Stance | Defend against a windup | Take the stance against an enemy winding up. A counter requires an actual successful block. |  |
| Iron Stance | Defend at half health | Use it at 50% health or below. A counter requires a successful block. |  |
| Battlefield Ring | Gather even one enemy | Use the ring even on a single nearby enemy. |  |
| Battlefield Ring | Gather three or more | Use it when at least three enemies are within the actual area. |  |
| Tremor Wave | Fire a wave immediately | Fire the wave at the first available enemy. |  |
| Tremor Wave | Wait to pierce three | Fire when three enemies are in the actual line attack. |  |
| Breath Before Battle | Recover low resources | Start restoring resources when they are at 30% or below. |  |
| Breath Before Battle | Top up resources early | Start restoring resources early, at 75% or below. |  |
| Guardian's Vow | Use without a shield | Use it in combat when no shield remains. |  |
| Guardian's Vow | Reserve to remove slowing | After being slowed by an enemy, create a shield and remove the slowing. |  |
| War of the Ancestors | Support strong-enemy fights | Gain ancestral attacks and damage reduction against an elite or boss. Ancestors do not absorb enemy attacks. |  |
| War of the Ancestors | Support in a health crisis | Summon ancestors at 50% health or below to support attacks and survival. |  |
| Titan's Judgment | Judge a strong target | Fight a group containing an elite. |  |
| Titan's Judgment | Judge a group of four | Fight one enemy first. Three more arrive after 3 seconds. |  |
| Titan's Judgment | Judge after a shout | Shout before judgement, then show its empowerment of Crushing Strike and Ground Slam. | Battle Shout, Crushing Blow, Ground Slam |
| Successive Shots | Prioritise repeated shots | Fire repeated shots whenever ready. |  |
| Successive Shots | Keep 40% resources | Use it only when at least 40% resources will remain afterward. |  |
| Patient Shot | Aim when an opportunity appears | Aim as soon as the skill is ready. | Hunter's Mark |
| Patient Shot | Aim at a distant marked target | Aim at a marked target at least 7m away when there is no nearby threat. | Hunter's Mark |
| Briar Trap | Restrict movement immediately | Place a thorn trap whenever ready. | Frost Snare |
| Briar Trap | Prioritise uncontrolled enemies | Prioritise targets without slow, root, stun or freeze. | Frost Snare |
| Frost Snare | Slow enemies first | Use the trap immediately to slow enemies. | Venom Arrow |
| Frost Snare | Freeze after poisoning | Use the trap against poisoned enemies to also freeze them. | Venom Arrow |
| Decoy Projection | Divert nearby threats | Divert nearby normal enemies within 3m toward the decoy. |  |
| Decoy Projection | Divert a threatening pack | Use it when at least three normal enemies threaten the hero within 3m. |  |
| Smoke Cover | Escape nearby threats | Gain damage reduction and movement speed when enemies approach within 3m. |  |
| Smoke Cover | Respond to a health crisis | Use it at 40% health or below. It does not grant invisibility or invulnerability. |  |
| Venom Arrow | Poison a new target | Use it on an enemy without your poison-arrow damage over time. | Hunter's Mark |
| Venom Arrow | Poison the marked target | Wait for a hunter-marked enemy before firing. | Hunter's Mark |
| Forking Arrow | Fire at even one enemy | Fire immediately even at a single enemy. |  |
| Forking Arrow | Split when three gather | Fire when two additional enemies are within 3m of the target. |  |
| Watch Ballista | Deploy in any fight | Deploy the ballista even in fights against normal enemies. |  |
| Watch Ballista | Reserve for strong targets | Reserve deployment until an elite or boss appears. |  |
| Hunt Preparation | Restore low resources | Restore resources at 30% or below and discount the next paid skill. | Multishot |
| Hunt Preparation | Prepare before a paid shot | Prepare when a paid skill is ready. | Multishot |
| Killing Rain | Suppress a strong target | Strike a fixed area containing an elite or boss repeatedly. | Frost Snare |
| Killing Rain | Suppress a controlled pack | Strike a fixed area containing at least three controlled enemies repeatedly. | Frost Snare |
| Shadow Pursuit | Empower pursuit immediately | Immediately gain movement speed and shadow follow-up attacks. |  |
| Shadow Pursuit | Empower with ample resources | Wait for at least 70% resources before empowering shots. |  |
| Ember Lance | Pierce immediately | Fire even at one target, leaving burns after piercing. |  |
| Ember Lance | Wait to pierce three | Fire when three enemies enter the actual line of fire. |  |
| Firewall | Create a fire area immediately | Place the wall immediately. Damage depends on time actually spent in its area. | Blizzard |
| Firewall | Overlap controlled enemies | Place the wall so a controlled target is inside its actual area. | Blizzard |
| Glacial Lance | Slow enemies first | Apply slowing with the ice spear itself. | Blizzard |
| Glacial Lance | Freeze slowed enemies | Use it on an already slowed enemy to freeze it. | Blizzard |
| Frost Globe | Launch immediately | Launch the orb immediately; it explodes after travelling. | Blizzard |
| Frost Globe | Launch at a controlled target | Launch toward a controlled target. The orb does not home. | Blizzard |
| Capacitor Orb | Deploy for lasting damage | Use the orb for periodic damage and an expiry blast. | Chain Lightning |
| Capacitor Orb | Deploy with chain lightning ready | Deploy once chain lightning is ready and affordable, then charge it with actual casts. | Chain Lightning |
| Storm Spear | Pierce immediately | Use the piercing attack whenever ready. | Chain Lightning |
| Storm Spear | Follow chain lightning | Hit a target struck by your chain lightning within the last three seconds to recover resources. | Chain Lightning |
| Mana Reclaim | Recover briefly in safety | Recover to 50% from at most 35% resources when the surroundings are safe. |  |
| Mana Reclaim | Recover while accepting damage | Recover in place to 50% even while taking damage. Global emergency responses can interrupt charging. |  |
| Mana Reclaim | Gain distance and recover fully | Try for up to two seconds to gain 5m of space, then recover to 90%. Stop if danger approaches again. |  |
| Rift Ward | Deploy without a shield | Deploy the ward in combat when no shield remains. |  |
| Rift Ward | Deploy in a health crisis | Deploy at 50% health or below for a shield and damage reduction inside the ward. |  |
| Elemental Compass | Restore low resources | Restore resources at 30% or below. | Ember Lance, Glacial Lance, Storm Spear |
| Elemental Compass | Prepare three elemental discounts | Use it with paid fire, cold and lightning skills equipped. Each element consumes its discount on its first use. | Ember Lance, Glacial Lance, Storm Spear |
| Magnetic Vortex | Gather even one enemy | Use it even on one enemy for an initial pull and lasting damage. |  |
| Magnetic Vortex | Gather three or more | Use it when three or more enemies gather inside the actual area. |  |
| Triune Collapse | Collapse a strong target | Fight a group containing an elite. |  |
| Triune Collapse | Collapse a group of four | Fight one enemy first. Three more arrive after 3 seconds. |  |
| Sage Incarnate | Empower immediately against strong targets | Immediately gain a shield, attack power and reduced costs against a strong target. | Ember Lance, Glacial Lance, Storm Spear |
| Sage Incarnate | Empower when three elements are ready | Empower once all three elemental skills are ready and affordable, then trigger elemental-cycle bursts. | Ember Lance, Glacial Lance, Storm Spear |
