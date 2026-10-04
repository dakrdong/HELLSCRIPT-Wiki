# Hunt Edict progression and starter comparison tutorial

Updated: 2026-10-04

[Korean](Hunt_Edict_Progression.md)

New players first learn that changing a policy changes a hunt. Account-best clears disclose settings permanently. Player understanding and the spacing between disclosures require a separate first-player playtest.

## Single source of disclosure

[HuntEdictProgression.json](../../Assets/HELLSCRIPT/Resources/HuntEdictProgression.json) classifies all 152 global IDs exactly once; [HuntEdictProgression.cs](../../Assets/HELLSCRIPT/Runtime/Core/HuntEdictProgression.cs) owns permissions and save checks. The compatibility field autoEquip.preserveEffects remains stored and hidden.

| Milestone | Disclosure | Global fields |
| --- | --- | --- |
| Mandatory map | First skill, two policies, HP potion threshold | 1 |
| First regular Rift result, win or loss | HP potion toggle, balanced/all dodge, last danger review | 1 |
| Hero level 2 and new points | Currently learnable skills and quick policies | Per skill |
| Rift 2 | Combat styles | Official recipes |
| Rift 3 / 4 / 5 | Position / target selection / low-HP response | 11 / 6 / 5 |
| Rift 6 | Direct editing of disclosed groups, detailed learned-skill policies, basic attacks and order, local presets and training comparison | Capabilities |
| Rift 7 / 8 / 9 | Loot / exploration and interactions / basic bag handling | 12 / 9 / 12 |
| Rift 12 / 14 | Basic automatic equipment / potion supplies | 3 / 11 |
| Rift 15 and mandatory Rune lesson complete | Repeated hunts and limits | 8 |
| Rift 18 | Detailed pursuit, survival/dodging, loot filters, bag protection/admission | 40 |
| Rift 20 | Full action order, detailed exploration, repeat acquisition goals, sharing/import and recommended mode | 15 |
| Legendary or set acquired after Rift 12 | Eight equipment-position criteria and excluded positions | 17 |
| Hero level 40 | Actual ultimate learning, equipment and policy | Per skill |
| Elixir content unlocked and actual ownership | Elixir use and resupply | Related potion fields |

Gems, cores and potion categories also require actual acquisition. Cursed chest options and guidance require a recorded discovery. Hero level, learned/equipped skills and parent-option conditions remain domain checks. New combat style recipes preserve potion thresholds and purchase settings; legacy accounts retain the original style behavior.

## Mandatory map v3

Automatic combat introduction → spend a real point on the first skill, equip slot one and save → observe A → activate/save B and observe → activate the preferred policy and confirm → change HP potion threshold from 40% to 60% during the boss lesson and save → observe actual automatic healing → defeat the boss, commit the existing reward and arrive in town.

Warrior compares W01 stand/edge; Ranger A01 steady/pack; Mage M01 steady/pack. The existing combat preview runner copies the owned early hero, equipment and skill ranks. Both segments reuse A's seed, enemy placement/spawns and starting HP/resources; only the selected skill policy differs. Each segment runs at 1x for 10 seconds, with play/pause/restart. Enemy attacks are at most one. Preview currency, potion stock, records and rewards never reach the real account. Observation needs real casting and edge movement or pack-condition waiting as well as the duration.

The final saved policy is checked against the executing hero. The HP lesson records a successful automatic heal. Mandatory edits do not earn F05/H09 follow-up practice. Training/retry signatures include actual effective policies and recommended-mode state.

## UI and optional guidance

Reuse HuntEdictWindow's existing specialized adapter, ContentWindowHost, UiTheme/UiFonts, StoryDialogueWindow and combat preview owner. The first lesson shows only the current skill, first slot and action. Town starts with overview, skills and survival. Visible tabs determine layout. Search, summaries, change lists, help, journal and shortcuts use the same disclosure rules. There are no locked tab placeholders; only one next feature is teased.

After results and rewards, town shows one optional card with Configure and Later. Configure focuses the relevant setting; Later leaves other content usable. The journal supports rereading and practice. Mandatory Rune guidance has priority; optional dialogs do not open automatically during combat. First-result guidance includes the preserved last five seconds of HP/damage and the largest damage source. Korean and English copy ship together.

StoreViewBinding keys cover the hero, level, disclosures, potion revision, edict/skills/presets and lesson step. Unchanged saves retain controls. Input, presses, dragging and child windows defer refresh. Drafts, named focus paths and owner scroll are preserved. Stop, revert, manual protection and manual restock remain available.

## Saves and compatibility

Schema 21 adds account disclosure version/IDs/legacy access, the separate announced flag, and map flowVersion/lessonStep. Announced, read, deferred and practiced remain distinct. Settings and steps advance only after successful writes. Restart replays the saved comparison segment from its initial state. In-progress v2 saves finish through the old flow. The existing tutorial-map-complete-v2 request ID prevents duplicate rewards.

Unchanged hidden values remain valid in full-document saves. Changed values require disclosure permissions. Store recomputes official recipe IDs and scopes. Share codes contain neither account progress nor recipe exceptions. Presets changing undisclosed values are rejected with the condition. General Save mutations are compared to the last successful edict boundary too.

New heroes execute their edited policies. Automatic buying/disposal/equipment/repeat starts OFF and disclosure never enables it. Existing accounts retain access/settings without newly invented read/practice completion.

## Evidence

[Validation record](EdictProgressionEvidence20261004/validation.json) records final scope, results and gaps. Development checks were limited to disclosure, storage bypass, identical comparison conditions and v3 failure/restart/potion/reward regressions. The final EditMode suite, shared UI/refresh checks and macOS development-player acceptance run once on the integrated final source; failed checks and directly affected coverage alone are rerun after repairs.

macOS synthetic pointer/input evidence is distinct from browser, physical-mobile and first-player evidence. This work makes no measured performance-improvement or physical-mobile pass claim.

[152-field disclosure database](#/db/edict-disclosure)

Current verification: full EditMode ran once (5,147 tests: 5,065 passed, 82 failed). Targeted repair ran 158 tests (156 passed, 2 failed), followed by 32 passing directly affected checks. All 82 original failures are resolved; the later 31 disclosure-boundary and one hero-level check also passed. Warrior native input proved actual A/B observations, checkpoint restart, final policy save/execution, five KO/EN viewport pairs, HP 40→60 save, automatic healing, boss defeat and the existing 1,000-gold completion transaction. Attendance could open before the town arrival dialogue; the shared cinematic handoff now defers automatic popups, and runtime C# compilation passed. No game tests were restarted for the user-requested consolidation. The repaired town arrival, town/entry/restock/refresh interactions and Ranger/Mage native acceptance remain unverified; this is not a complete runtime acceptance claim.

Manual potion restocking remains available from Rift entry before disclosure. It respects the selected supplies, budget and gold reserve, and leaves automatic buying OFF. A new skill can be equipped with its existing policy without falsely treating equipment changes as advanced policy edits.
