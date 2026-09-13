# Detailed history for completed rifts

Date: 2026-09-13 · [한국어](Combat_History_Expansion.md)

Status: **the latest-ten detailed archive and existing result navigation are implemented; all 2,156 editor tests and three native launches/restarts passed.**

## Player flow

Open **Combat records** in town and select one of the last ten results, or select **Detailed combat record** immediately after a rift. The summary links to skill/effect totals, the final combat window, decision entries, final settings/equipment and the text log. The existing death-analysis action uses the same archived record.

Per-effect totals distinguish starts, releases, completions, interruptions, hits, damage, actual HP damage and insufficient-resource checks. Actual HP damage excludes overkill. Resource checks count decisions rather than resource spent or interrupted actions. Partial totals and configuration changes are identified.

The final window shows its actual duration, HP endpoints, incoming sources and their shares, and blocked attempts. Select a moment to inspect position, target, action state, incoming damage and the rule that was blocked. When a core hunt edict was active, the attempt links to a copy of the document actually used at that time. These are observed reasons, not a claim that an alternative skill would certainly have prevented death.

Final settings are copies of the equipment, active skills, passives, rules and active edict at completion; they do not describe every earlier configuration. Viewing history does not change current equipment, rules, currency or rewards. Copies remain after selling an item or changing characters, without granting or locking the original equipment.

## Persistence boundaries

The optional version-1 `RunRecord.review` field is additive. Legacy results retain their summary and text log. An unsupported newer detailed schema also leaves those older fields readable. Version zero is treated as legacy because Unity may deserialize an absent nested object as an empty instance.

| Data | Retention |
|---|---|
| Results | Latest ten, removing the oldest when a new result arrives. |
| Totals | All observed aggregate and per-skill/effect counters are copied. |
| Final window | Last five combat seconds, at most 101 samples. |
| Detailed events | Up to 512 incoming hits and blocked attempts together; newest first. |
| Decision entries | Latest 80, preserving grouped counts and first/last timestamps. |
| Historical edicts | Up to eight actual documents referenced by retained blocked attempts. |

Trimming details does not reduce observed window totals; the UI identifies the retention limit. The live ring is not serialized mid-run, so a fight ending shortly after resuming may have less than five seconds of detailed observations. Actual coverage is shown and missing time is not reconstructed. Training never creates or changes owned result history.

Completion captures telemetry after the fatal tick. Incoming-source HP-loss shares use actual HP loss for both numerator and denominator, avoiding percentages above 100% when absorption or overkill is present. Generic attacks from different enemy kinds no longer merge under one enemy name.

The layout follows the [screen specification](../Design/HELLSCRIPT_Screen_Layout_Detail.md). Long descriptions grow with content; window size, interface scale and safe-area changes trigger reflow. History pages pause the automatic-repeat countdown, and returning to results continues the remaining delay.

## Validation and limits

The final pinned input passed **2,156 full Unity editor tests**, **120 focused tests**, a macOS development build and **three independent launches/restarts**. Preserve the [validation summary](../../Artifacts/Validation/CombatHistory/validation-summary.json), [actual test report](../../Artifacts/Validation/CombatHistory/Full3.xml) and [source/player hashes](../../Artifacts/Validation/CombatHistory/validated-source.json). Inputs are `9f0489d`, including Rune drops/fusion, plus this phase's files. No scene, prefab or package edits were made; all 19 unrelated working changes were preserved.

A fresh starting Warrior entered stage one with seed 551. Real fixed ticks produced death at 67.1s, level three and 35 kills without injected HP, enemies, terrain, rewards or terminal states. The archive retained 100 samples across five seconds, 94 blocked skill attempts and one edict context. The next rift started through automatic repeat; after returning, changing character and relaunching, the original record remained identical. Normal controller updates were disabled during accelerated fixed-step combat, so its real-time counter is 0.0s. Countdown checks separately used normal updates.

A separate result fixture explicitly injected alive return, timeout, hero death, training death and boss/hero tie victory. It verified the death-analysis action and return path. These boundary fixtures are not counted as natural combat.

There are 33 captured screens with automated layout/translation checks and 13 directly reviewed representative screens, including KO/EN, portrait/landscape and 140% interface scale. See the [visual review](../../Artifacts/Validation/CombatHistory/visual-review.json), [Korean final window](../../Artifacts/Validation/CombatHistory/Native4/initial-03-window-ko.png), [enlarged English configuration](../../Artifacts/Validation/CombatHistory/Native4/initial-07-configuration-en-large.png) and [corrected action names](../../Artifacts/Validation/CombatHistory/Native4/initial-09-decisions-en.png).

Validation corrected legacy empty-object restoration, translations of joined skill names and stored condition text, the paragraph retained during resize, and utility decisions mislabeled as basic attacks. A natural set of 125 logs and 80 decision descriptions is retained as a translation regression fixture. The existing boundary excluding utility actions from blocked-skill totals is also tested.

The equipment view lists name, item level and enhancement. Complete item data is copied, but an archived affix-detail viewer and rune-board placement snapshots remain follow-up work. This advances CHK-F03 and CHK-A10 in the [completion checklist](../Design/HELLSCRIPT_Gameplay_Completion_Checklist.md). Counterfactual replay of every unchosen action, restoring pre-restart live samples, and physical-device touch/performance testing remain outside this phase.
