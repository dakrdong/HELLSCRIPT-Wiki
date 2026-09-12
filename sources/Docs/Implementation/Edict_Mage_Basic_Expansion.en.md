# Mage Lightning Bolt hunt-edict implementation

Date: 2026-09-13 · [한국어](Edict_Mage_Basic_Expansion.md)

Status: **the four Lightning Bolt policies and actual-hit resource, Restraint and elemental interactions are connected. All 1,765 tests, 17 native processes and the 48-run before/after comparison are complete.**

## Claude integration and scope

Claude's completed attribute, affix and balance revision `b1f3ea5` is included in local and remote main through merge `92d5812`. A fresh remote check at this phase's start found main and the development branch at `214b3de`, including Frost Nova. The 57 attributes, 54 affixes and existing save identifiers remain intact. No damage values or affix weights were tuned. See the [attribute integration record](Attribute_Integration_Expansion.en.md).

HEC-MAGE-01–04 from the [option catalog](../Design/HELLSCRIPT_Hunt_Edict_Skill_Option_Catalog.md) now drive the actual basic attack. An enabled Mage edict derives one Lightning Bolt candidate independently of active slots. Missing, disabled, condition-blocked or duplicate legacy basic rows do not override it. Opting out retains the legacy behavior and source rules.

## Four executable options

| Option | Behavior |
|---|---|
| Automatic use | ON evaluates a candidate; OFF prevents new starts. Committed preparation and in-flight projectiles finish with their original aim and collision rules. |
| Use mode | FILL follows the common action and skill order. RESOURCE requires an equipped, unlocked, automatic paid active with ready cooldown, valid purpose and physical opportunity, blocked only by its actual cost. DISCOUNT requires real LC02 equipment and prioritizes a valid unfinished hit target when both charge and internal cooldown are absent. An existing charge or internal cooldown falls back to FILL behavior. |
| Target preference | GLOBAL follows global targeting; CURRENT retains a valid current target; LOW_HP ranks health fraction before distance. OWN_BLIZZARD prioritizes actual coverage by a live own Blizzard, falling back to the global target. A valid unfinished Restraint target takes precedence during charge preparation. |
| Aim | CURRENT commits the position at preparation start. PREDICTED uses valid observed motion, preparation and flight time. Missing, stale or invalid observations and immobilized enemies fall back to current position. |

RESOURCE has no arbitrary low-resource threshold. An actual LC02 discount changes Fireball's cost from 25 to 12.5: 12.5 resource is funded, while 12.49 may permit a basic if all other conditions hold. A Nova requiring walking is not yet an otherwise-ready paid cast. The affordability probe never consumes resource, charges or cooldowns.

Own-Blizzard preference validates ownership, creation time, lifetime, activation delay, coverage and sight. Empty legacy ownership retains the existing compatibility rule. Foreign, hostile and inactive areas do not qualify. Perception and pursuit exclusions apply to selected targets. Approaches follow global movement and finite pursuit budgets, including after restoration.

## Actual hits, equipment and persistence

Lightning Bolt retains 10 m range, 14 m/s speed, 0.2 m radius, 0.8 lightning coefficient and one collision. Total action time is `0.8 / attackSpeed`, split 40% preparation and 60% recovery. The first actual enemy or wall collision determines the result; no piercing, chaining or homing is added.

Only an actual hit restores five resource and credits the actual victim's LC02 sequence. Recovery respects the real resource maximum and is not amplified by MP05. Three same-target hits earn a five-second discount charge with a four-second internal cooldown; hits during an active charge/cooldown do not pre-bank the next charge. No paid follow-up is forced. MP06 follows actual lightning and other-element damage.

The existing [itemization contract](../Design/HELLSCRIPT_Itemization_Detail.md) remains explicit: starting a basic against another target resets an unfinished LC02 count, and actually hitting another target also changes the sequence. Starting alone grants no hit reward or discount. This phase does not remove the start-time reset.

Edits and restoration preserve a committed action's target, aim and ID and the original in-flight projectile. Completion does not duplicate releases, hits, resource or charges. Korean and English guidance uses the existing editor, scroll content and footer structure from the [screen specification](../Design/HELLSCRIPT_Screen_Layout_Detail.md).

## Observing Nova replacements

The preceding eight configured-survival runs contained 52 M03-to-M06 replacements, but the old record could not distinguish each selected control reason. New records capture the same decision's reason, replacement M06 action ID, actual release/interruption/miss, damage hits and applied control target IDs. Global emergency/dodge requests are distinguished from dangerous enemy casts that can be interrupted. Combat decisions and values are unchanged by this instrumentation.

Status applications include refreshes, so they are not counts of enemy casts prevented or damage saved. Missing causes are not retroactively assigned to old records. Last-global-decision observations remain separate from the actual same-frame selection reason.

## Matched before/after sample

The preceding final M06 sample and this sample each contain 24 runs: level 30, stage 20, Chain Control, eight legal items and seed indices 0, 1, 2, 3, 4, 5, 9 and 14. Paired maps, encounters, objectives, full equipment, stat sheets, maximum HP, base damage and complete edict documents matched. The reused baseline was verified against 256 C#, localization and package inputs from its archive and commit `214b3de`.

| Eight runs per mode | Death / timeout, before→after | Boss spawns | Basic hits | M03 interruptions |
|---|---|---|---|---|
| Legacy rules | 4 / 4 → 4 / 4 | 4 → 4 | 989 → 989 | 28 → 28 |
| Default edict | 7 / 1 → 7 / 1 | 1 → 2 | 627 → 658 | 2 → 2 |
| Explicit survival configuration | 0 / 8 → 0 / 8 | 3 → 5 | 1,033 → 1,050 | 68 → 64 |

All modes had zero clears before and after. All eight complete legacy summaries were identical. Fresh defaults disable global survival; the explicit survival document is a separate laboratory configuration. Its 13 global differences leave skill options, slots and order identical. Both documents use FILL/GLOBAL/PREDICTED for BASIC. No player settings were replaced.

Configured M03-to-M06 replacements increased from 52 to 56. All 56 new records selected an interruptible dangerous cast in the same decision. All replacement Novas released, recording 222 damage hits and 183 per-cast control-target applications. Five had one damage hit each but no freeze/stagger application record. Deaths that made control unnecessary have not been separated, so these are not labeled five failed controls.

Mean pre-boss occupancy was 271.21→266.26 combat seconds in the configured sample; runs without a boss contribute their entire combat time. These outcome-selected cases do not estimate population win rates, farming efficiency or the isolated effect of predictive basic aim. Increased boss progression or damage does not establish improved clears, growth or long-term balance. The [comparison](../../Artifacts/Validation/EdictMageBasic/comparison.json) retains full policies, paired results and limitations.

## Validation

Validated with Unity **6000.6.0f1**. The [native player](../../Builds/macOS-EdictMageBasic/HELLSCRIPT.app) and evidence under `Artifacts/Validation/EdictMageBasic` are retained.

| Check | Result |
|---|---|
| Full Editor suite | **1,765 passed**, zero failures or skips. September 12, 2026, 19:35:37–19:40:17 UTC; 280.23 seconds. |
| Focused suite | 58 Mage basic cases and four combat-observer cases passed, including non-interference checks. |
| New native scenario | Three independent processes passed actual UI edits, file persistence, preparation restart, projectile restart, collision, resource, LC02 and MP06. |
| Existing active regression | M01–M06 scenarios passed in 14 further processes using the same new player. Skill-isolation fixtures explicitly disable BASIC. |
| Screens | All 13 new basic screenshots passed automated text/footer checks and visual review. They include Korean/English, 1280×720, 720×1280, 640×360 and 140% text. The 66 previous-active screenshots passed their smoke's automatic checks. This is not every language/size combination. |
| Rift comparison | 24 preserved baseline runs and 24 changed runs, with three modes and eight seeds per mode. Policy and initial-input equality and event/observer totals were checked. |

The first full run exposed one test expecting the old per-rule basic target while an edict was active. It now checks two explicit boundaries: the new global target with edict ON and the preserved legacy target with edict OFF. The original rule is asserted unchanged. Both cases passed in the final full suite; earlier failures remain archived.

Isolated Unity projects and dedicated saves preserve the original workspace. All 257 C#/English-text inputs matched the final full-test source. Native equivalence covers 181 runtime C# files and the English table; the target-selection test revised after the build is Editor-only. No C# metas are missing and no asset GUIDs are duplicated. Scenes, prefabs, packages and 17 unrelated settings/wiki changes were preserved.

The [validation summary](../../Artifacts/Validation/EdictMageBasic/validation-summary.json), [source/player manifest](../../Artifacts/Validation/EdictMageBasic/validated-source.json) and [source archive](../../Artifacts/Validation/EdictMageBasic/validated-source.tar.gz) retain scope and provenance. Controlled native scenarios do not replace natural-play balance assessment or physical Android-device validation.

## Remaining work

The Mage's M01–M06 actives and basic attack now have independent core policies. Other classes' remaining policies, growth and farming utility, control opportunity cost, long-term balance and physical mobile-device input/performance remain. Completing this phase does not complete the game or establish its balance.
