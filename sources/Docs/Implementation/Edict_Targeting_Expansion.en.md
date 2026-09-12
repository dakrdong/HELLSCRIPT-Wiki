# Hunt edict targeting and pursuit

Date: 2026-09-12 · Previous stage: [Claude review and follow-up fixes](Claude_Review_20260912.en.md)

The subsequent [gate passage stage](Gate_Passage_Expansion.en.md) connects physical passage blocking and opening. The remaining-scope list below records the end of this targeting stage.

## Scope

Connect the targeting section of the [global options specification](../Design/HELLSCRIPT_Hunt_Edict_Global_Options.md) under the [implementation contract](../Design/HELLSCRIPT_Hunt_Edict_Implementation_Contract.md). Claude's separate attribute and balance work remains excluded as requested. The only change to `GameCatalog.cs` is one pursuit-state field; attribute, growth and damage formulas are unchanged.

The previous bridge compressed the options into one legacy target mode. High HP became nearest, boss and elite became one preference, and HP competed with distance in a weighted score. The new selector compares priorities in sequence.

## Runtime behavior

| Option | Behavior |
|---|---|
| Nearest | Compare distance among perceived enemies. |
| Low/high HP | Compare remaining HP percentage first, then distance and stable enemy ID. The editor explains the percentage basis. |
| Dense | Compare the number of perceived enemies within the selection radius and mutual line of sight, then distance and ID. |
| Ordered specials | Apply enabled boss, elite, support and ranged categories in saved order; fall through when a category is absent. Bosses do not also count as ordinary elites. |
| Enemy categories | Ranged attackers are N03/N04/N09/N10; heal/support enemies are N05/N11, according to the current content. |
| Until defeated | Keep the valid observed target despite a new boss or elite. Visibility, pursuit limits and emergency survival take precedence. An interruptible dangerous cast can be an urgent target exception. |
| Switch by priority | A better perceived target can replace the old target on the next decision, without the legacy 0.6-second hold. |
| Boss encounter | Prefer the boss, its own summoned adds, or support. Own adds require the current boss's `summonerId`; unrelated field-event enemies are excluded. Fall back to the boss and ordinary targets when the preferred class is absent. |
| Dangerous caster | Prioritize a perceived threatening attack, heal or boss summon only when an enabled Ground Slam (W04) or Frost Nova (M06) meets range, sight, resource, cooldown, rule and remaining preparation-time requirements. For bosses, this cast must finish stagger. This target option does not reorder attack skills. |
| Stop on lost sight | Do not follow the hidden current position or substitute an arbitrary remembered enemy. |
| Check last sighting | Move only to the tracked target's last observed coordinates. End the check on arrival if the enemy is still absent. A hidden enemy's movement cannot update the destination. |
| Pursuit distance | Measure from the engagement's start position and stop before a pursuit movement crosses its boundary. |
| Pursuit time | Accumulate only during approach outside attack reach or checking the last sighting. In-range attacks, waits, ordinary orbiting, pause and cleanup portals do not consume pursuit time. |
| After a limit | Do not immediately restart pursuit of the same visible out-of-range enemy. An enemy entering attack reach or a later new sighting can become eligible again. |

Targeting uses observed enemies and saved observations. An unseen boss directs general exploration toward its known arena, not its hidden current coordinates. Pursuit termination records `EDICT_PURSUIT_END` with a reason.

## Persistence and policy edits

`RunState.edictTarget` stores the tracked ID, engagement origin, last sighting, accumulated pursuit time, search state and current exclusions. Old saves can omit this optional field. Unity can restore absent nested objects as defaults, so policy version 0 retains legacy behavior and version 1 identifies the new policy. Newer versions stop loading while preserving the original save. New actions capture their exact target policy, so a running Whirlwind retains its original policy through document edits and restart. Shared codes contain settings, not personal pursuit state.

The legacy selection path remains when the edict is off. Explicit per-rule target choices retain their meanings; exact edict targeting applies to inherited global choices. Moving to an independent per-skill scheduler remains separate work.

## Screen layout

Following the [screen layout specification](../Design/HELLSCRIPT_Screen_Layout_Detail.md), the editor retains its draft and saved state through portrait and landscape layouts. Order buttons have more width and row height. Long notes expand their rows to fit the text. Width changes reflow around the current reading position; rebuilt pages calculate the new content's height as well.

Native checks exposed clipped order buttons and long warnings with English text at 140% in compact windows. Both were corrected. The body scrolls while save and revert remain in the footer; the layout does not require every option to fit above the fold.

## Remaining scope

- Golden goblin content does not yet exist. Enabling its preference shows an unavailable notice while retaining the saved choice. Its dedicated distance/time options await the content.
- The full set of skill options and an independent decision scheduler remain in progress. This stage does not create hidden skill rules.
- Conditional legendary/set pulls and chained control procs are not forecast as guaranteed interruption tools. The direct control checks cover W04 and M06.
- Physical gate passage, essence/offering objectives, remaining cleanup/repeat choices and online accounts remain on the prior backlog.

## Validation

Validated with Unity 6000.6.0f1. Evidence is under `Artifacts/Validation/EdictTargets`.

- The old implementation failed 13 of the 15 initial target-selection cases: `before.xml`.
- Initial targeting/edict/localization checks passed 70/70; follow-up caster checks passed 61/61: `first-fixed.xml`, `target-complete.xml`.
- The full run exposed 7 legacy-save continuity failures: an empty restored policy object was interpreted as active. Existing continuity expectations were retained; explicit policy versions fixed the source. All 105 focused checks then passed: `full-final.xml` and `migration-fixed.xml`.
- The final full run passed **1,243/1,243**, with no failures or skips. September 12, 09:12:40–09:16:05 UTC. This includes 33 new targeting/pursuit cases and the existing action-continuity suite: `full-verified.xml`.

The final macOS development player is `Builds/macOS-EdictTargets/HELLSCRIPT.app`. `build-verified.log` records a successful build, zero build errors and normal exit.

- Target selection and a separate restart process both passed. The actual editor saved High HP, the simulation selected it over a nearer low-HP enemy, and restart restored a partially consumed pursuit timer. The remaining time ended pursuit without immediately restarting it; an enemy entering attack reach became eligible again. Post-restart state matched a control simulation receiving the same inputs. See `native-results.json`.
- Two additional native processes passed the broader edict editing, saving, sharing and restart checks after the shared row-layout change: draft edits, order moves, slots, skill options, revert, code import and persisted values. See `editor-results.json`.
- The final build produced 22 screenshot artifacts. Checks covered Korean and English; 640×360, 720×1280, 1280×720 and 1920×1080 windows; and English text at 140%. This is not every combination of those settings. Examples: [compact order buttons at 140%](../../Artifacts/Validation/EdictTargets/Evidence/06-target-order-english-140-compact.png), [Korean target settings](../../Artifacts/Validation/EdictTargets/Evidence/01-target-editor-korean.png), [pursuit stopped after restart](../../Artifacts/Validation/EdictTargets/Evidence/01-resumed-pursuit-stopped.png).
- Only three presentation files changed after the full suite: `GameUI.EdictEditor.cs`, `GameUI.cs` and `RuntimeTargetSmoke.cs`, for layout and its validation. Combat, persistence, test sources and localization still match the full-suite snapshot. The final build and four native processes validate those presentation edits. Both snapshots are retained in `full-test-source-manifest.json` and `final-source-manifest.json`.
- Audited 196 C# files and preserved all 287 baseline metadata, serialized assets, scenes, settings, package and design files. Four new C# meta files belong to this stage; no duplicate GUIDs or missing C# metas were found. See `validation-summary.json`.

This evidence covers automated native development-player scenarios with isolated save directories and visual inspection. Physical mobile touch, sustained performance and full-game balance are outside these checks.
