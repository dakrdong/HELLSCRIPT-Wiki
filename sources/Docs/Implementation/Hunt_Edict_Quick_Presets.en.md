# Hunt Edict quick presets

Updated: 2026-09-29 · [한국어](Hunt_Edict_Quick_Presets.md)

Each item offers named recipes and a final **Custom Settings** tab. Skills, basic attacks and common attack order use tabs with descriptions and combat examples. **Activate preset** applies and immediately saves the previewed recipe; editing active Custom Settings also saves immediately. Global groups retain their existing picker and Save/Revert workflow.

## Global section tabs and preset storage

Updated 2026-09-29: Combat, Survival, Loot, Bag, Exploration, Repeat Hunt and Recommended Equipment use fixed horizontal section tabs. All 35 existing groups remain available, including Combat's existing Action Priority group in addition to the four groups requested.

- Each section has a semantic line icon and a bilingual explanation. Tabs wrap on narrow screens or larger text settings; only the selected content scrolls.
- Search results cover all categories and open the corresponding section. The changed-only filter and per-section scroll positions remain available.
- All five preset slots fit on one page without scrolling. A saved slot is a single row with its number, short name field, disk save icon and View Share Code action. The selected row is green; other saved rows are beige. The redundant editing-status text is removed.
- First select a preset name, then select that name again to rename it inline. Confirming the input saves through `RenameHuntEdictPreset`. Long names stay clipped within the field.
- The disk stores the current settings in that slot. Existing slots require overwrite confirmation. Sharing reads that slot's saved snapshot. Import validation, five destinations and overwrite confirmation remain intact.
- The existing footer still applies or reverts the overall draft. Skill-specific auto-saving is described below.
- Use default settings lives in [Overview](Hunt_Edict_Overview.en.md), with no duplicate control in preset storage.

`HuntEdictUi.json` owns section metadata; `HuntEdictWindow.Summary` and `.Presets` own the layout. `.Glyphs` draws the icons with the shared `SkillTreeGraphic` mesh renderer. No new raster assets are used.

## Skill tabs and combat examples

- Three recipe tabs plus Custom Settings stay fixed. Landscape uses one row and portrait two rows; only the description or detailed controls scroll.
- The gold tab is the page being viewed; the green check marks the actually saved, active preset. Browsing tabs never changes or saves values.
- The activation action stays in the same position on every tab. The applied tab says **Active**. A failed write keeps the previous active marker and values and reports the error.
- All 54 active/ultimate skills have three examples each: 162 skill examples. Nine class-specific basic-attack examples and three common-order examples bring the total to 174. Circles represent the hero, diamonds enemies, and arrows movement/attacks. Scenes explain packs, elites, marks/control, shields, resource/HP conditions, movement, retreat, charging and attack order.
- The game draws examples using existing skill icons and `SkillTreeGraphic` lines, rings and shapes. No raster generation model or new bitmap assets are used. Conditions and thresholds read the existing recipe/policy definitions; illustrative placement does not simulate or predict a battle outcome.
- **Custom Settings has no example picture.** Detailed controls are visible before activation, and become editable after activation. Each selected option, confirmed number, or completed common-order drag is then saved immediately.
- The skill-policy page has no Save/Revert footer. Equipped skills form a right-hand rail in landscape and remain in a bottom dock in portrait. Skill-tree investment/equipment and other categories retain their existing explicit-save workflow.

## Coverage and persistence

- Definitions cover the original 25 global groups and 130 options, 54 active/ultimate skills, three class basic-attack scopes and one common-order scope: 83 scopes and 249 recipes. A later change gives full-warehouse handling dedicated controls, leaving 24 global groups with a preset picker.
- Only equipped skills appear. Always-on passives without an edict policy do not receive artificial presets.
- Each recipe fills its entire scope, using catalog defaults for unspecified fields. Survival references resolve to eligible equipped skills and remain unassigned when none exist.
- Skill auto-save uses a separate transaction that copies only the edited policy into the actual owner. It does not commit unrelated skill policies, pending ranks, equipment, global settings or a pending whole-preset slot selection. Those drafts remain pending; Revert cannot undo a policy already saved by this page.
- Existing `GameStore.CommitHuntEdict` updates the currently owned whole-preset slot. A different pending slot stays pending. Live battles preserve health ratio, spent cooldowns and pause state; training/comparison keep their existing dedicated owners.
- Optional `ClassSkillLoadout.presetSelections` stores explicit activation, including Custom. Custom reopens after restart/share even when its values match a named recipe. Older saves and share codes may omit the field. Unmatched old values remain applied while the first recipe is previewed, with the active check on Custom Settings.
- Global-group Custom still only expands existing values and keeps its view state unsaved. Search opens the matching group in Custom Settings.
- Whirlwind's **Quick center entry** uses skill-local `CENTER` movement toward a legal gap near the centroid of observed enemies within five meters of the selected target. **Survival-first combat** keeps the existing edge orbit. Both preserve hazard, pursuit and navigation gates without changing global positioning or movement speed.

## Ownership

`HuntEdictQuickPresets` and `Resources/HuntEdictQuickPresets.json` own recipes. Expanded skills reuse bilingual choices in `Resources/Data/ClassSkills.json`; Mana Recovery binds both use policy and recovery goal. `HuntEdictSkillPresets` validates active tabs and copies one policy. `HuntEdictEditSession.SaveSkillPolicy` invokes the existing atomic save callback and preserves owner/draft on failure.

The page remains an adapter within `HuntEdictWindow`, reusing shared buttons, theme, fonts and scrolling. `SkillPresetExampleView` receives recipe/loadout inputs and draws a read-only diagram without account access. HED4/HED5 preserve explicit tabs while continuing to read old codes and reject unknown fields.

Sources: [recipes](../../Assets/HELLSCRIPT/Resources/HuntEdictQuickPresets.json), [scope/application](../../Assets/HELLSCRIPT/Runtime/Core/HuntEdictQuickPresets.cs), [activation/policy copy](../../Assets/HELLSCRIPT/Runtime/Core/HuntEdictSkillPresets.cs), [preset page](../../Assets/HELLSCRIPT/Runtime/Presentation/HuntEdictWindow.SkillPresets.cs), [examples](../../Assets/HELLSCRIPT/Runtime/Presentation/SkillPresetExampleView.cs), [Whirlwind movement](../../Assets/HELLSCRIPT/Runtime/Core/CombatSimulation.EdictWhirlwindMovement.cs).

## Tab and auto-save verification, 2026-09-29

**146 focused Unity Edit Mode tests** and **9 shared UI regression tests** passed, together with the UI ownership check. The macOS Development build has zero compilation errors. This covers the changed UI, presets, transactions, sharing, class skills and localization, not the complete game suite.

The native game used real uGUI raycasts and pointer down/up/click events to open, activate and save **all 174 examples**. The **20 layout combinations** cover 440×956 portrait, 956×440 landscape, 1600×900, 1600×1000 and 2100×900, Korean/English, and 100%/150% text. All four recipe tabs, the fixed activation action and main navigation stay visible without scrolling. The action does not move between tabs. Custom shows detailed controls without a picture.

An intentionally failed disk write kept the previous active tab and values and allowed retry. Pending ranks, equipment and global settings were not captured by a skill auto-save. Read-only matching also resolves saved policies for a skill equipped only in the draft, without learning or equipping it on the owner. A common attack-order card drag saved on drop; a fresh player process restored the last Custom option, explicit tab and dragged order. Regression runs also covered the 24 global picker groups, pending-equipment policies, socket actions and the existing skill-tree allocation/save/restart flow. The injected failure remains in the logs; only the development-console overlay was hidden for captures.

No live Unity Editor instance was connected, so verification used the existing project batch/build tools and a macOS player with isolated saves. The user's account save was untouched. Physical-phone/tablet touch input and performance remain unverified.

- [Edit Mode results](HuntEdictSkillPresetEvidence/editmode.xml) · [Build](HuntEdictSkillPresetEvidence/build.txt) · [Native interaction](HuntEdictSkillPresetEvidence/runtime.txt) · [Restart](HuntEdictSkillPresetEvidence/restart.txt)
- [174-example catalog and source hashes](HuntEdictSkillPresetEvidence/examples.json)

![Landscape: Whirlwind center-entry preset](HuntEdictSkillPresetEvidence/preset-final-center.png)

![Custom: detailed controls without an example image](HuntEdictSkillPresetEvidence/preset-restart.png)

![Portrait: preview and active tab remain distinct](HuntEdictSkillPresetEvidence/preset-440x956-ko-100.png)

![Short landscape: complete example at English 150%](HuntEdictSkillPresetEvidence/preset-956x440-en-150.png)

![Frost Snare: follow poison with freeze](HuntEdictSkillPresetEvidence/example-A10-setup.png)

![Mana Recovery: retreat and recover more mana](HuntEdictSkillPresetEvidence/example-M13-retreat.png)

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

## 2026-09-29 section and preset-storage validation

- 223 distinct related Unity 6000.6.0f1 Edit Mode tests passed: 129 focused cases plus 101 mode/equipment/training cases, with seven duplicates. No failures or skips. Shared UI ownership checks and nine validator regression cases passed.
- The macOS Development build reported zero errors. No Unity MCP editor instance was connected, so the existing project batch test runner and builder were used.
- Native macOS acceptance covered 440×956, 956×440, 1600×900, 1600×1000 and 2100×900, Korean/English, and 100%/150% text: 20 combinations. All 35 section tabs were exercised through real uGUI pointer events and raycasts, with icon, explanation, bounds and content-height checks. Landscape safe-area insets were also checked.
- With all five slots populated, the page has no ScrollRect and each slot stays on one row. Select-then-rename, disk saves, exact-slot sharing, import overwrite confirmation/cancel, stored-slot overwrite and fresh-process persistence passed.
- Default/custom mode transitions, source preservation, failed-write rollback, effective combat skill policy, potion/equipment policy consistency passed. The 24 global groups with quick presets retained access to every detailed option, save/reopen and process-restart behavior.
- Mobile dimensions were simulated in the native macOS player. No physical mobile device was tested.

[Edit Mode](HuntEdictSectionEvidence/editmode.xml) · [Mode regression](HuntEdictSectionEvidence/mode-regression.xml) · [Build](HuntEdictSectionEvidence/build.txt) · [Runtime](HuntEdictSectionEvidence/runtime.txt) · [Restart](HuntEdictSectionEvidence/restart.txt) · [Quick-preset regression](HuntEdictSectionEvidence/quick-regression.txt) · [Regression restart](HuntEdictSectionEvidence/quick-restart.txt)

![Combat section tabs](HuntEdictSectionEvidence/combat-final.png)

![Five single-row preset slots](HuntEdictSectionEvidence/presets-final.png)

![Default mode in Overview](HuntEdictSectionEvidence/overview-defaults.png)

![Portrait, English, 150% text](HuntEdictSectionEvidence/slots-440x956-en-150.png)

![Landscape, Korean, 150% text](HuntEdictSectionEvidence/slots-956x440-ko-150.png)

![Equipment section icons](HuntEdictSectionEvidence/autoEquip-956x440-en-150.png)
