# Fireball Hunt Edict implementation

Date: 2026-09-12 · [한국어](Edict_Fireball_Expansion.md)

**Follow-up:** [Blizzard’s five M02 core options](Edict_Blizzard_Expansion.en.md) were connected on 2026-09-13. This record preserves the M01 phase and its evidence.

Status: **M01 core policies, full regression, and the new macOS player launch/restart validation are complete.**

## Problem and behavior

Selecting Steady Fireball could still be blocked by an old Blizzard or predicted-target condition. A missing or disabled legacy Fireball row also prevented the core setting from producing a candidate.

When the hero enables Hunt Edict and equips M01, the simulation now derives one Fireball policy from the saved core options. It preserves the legacy rule list, collapses duplicate Fireball candidates, and follows the document's common attack order. Disabling Hunt Edict restores legacy behavior.

| Core option | Connected behavior |
|---|---|
| Automatic use | OFF removes future cast candidates. A cast already preparing and any launched projectile retain their aim, travel, and damage execution. |
| Purpose | STEADY accepts one valid enemy. GROUP requires at least three predicted victims of the primary explosion at the actual first collision. EXPOSURE prefers this hero's own SM4 marks whose remaining duration exceeds preparation plus flight; no qualifying mark or missing SM4 falls back to STEADY without changing the saved selection. |
| Priority | Select the global target, an elite/boss, a support enemy, or the candidate with the most primary victims. Apply this preference inside the qualifying own-mark pool when exposure is preferred. Missing categories fall back to the global target. An unreachable global target does not exclude reachable alternatives. |
| Aim | CURRENT uses the observed position at cast start. PREDICTED leads recent observed velocity through preparation and flight, then predicts first collision and primary victims. Missing, stale, or invalid motion falls back to current position. An action's aim remains fixed after it starts. |

Prediction uses the live 10m range, 14m/s projectile speed, 0.25m collision radius, and 2.5m primary radius. Only perceived enemies enter planning; terrain and body collisions still apply. It does not read unseen positions or enemy intent. The existing impact code independently resolves real damage, so a later turn, death, or newly arriving blocker can change the outcome.

LM02's delayed blast breaks ties only when primary-victim scores match; it never contributes to GROUP's three-victim threshold. MP01 and SM4 consumption remain tied to real primary impacts. Unlock, actual cost, cooldown, action locks, and the preceding global survival response still gate execution. M04 currently costs no resource, so escape reservation does not add an artificial Fireball cost. LC02 is consumed once at an actual paid cast start.

A derived Fireball candidate can approach an out-of-range target even without a legacy row. Turning it off does not reuse that previous approach command. The action captures its derived policy and aim for restoration without an extra release.

## UI and persistence

The Fireball card explains that its four options affect combat. Decision history uses “Fireball hunt edict” instead of a fictitious legacy row number and readable descriptions for waiting and automatic-use-off decisions. The share screen now describes the connected global policies. Language redraws preserve the decision screen's original pause state.

Korean and English strings are maintained together. Native checks follow the [screen layout specification](../Design/HELLSCRIPT_Screen_Layout_Detail.md), including scrolling content, reachable footer buttons, portrait, compact landscape, and 140% text. No save-schema, item weights, damage tuning, scenes, prefabs, or packages changed. `edict:M01` identifies a derived execution policy; it does not rewrite the player's old rule list.

## Validation

The focused run passed 148 tests covering legacy independence, duplicates, GROUP counts, LM02 ties, own SM4 ownership/expiry, observed motion and real hits, unreliable motion, OFF after launch, restore during preparation, and costs/discounts/cooldowns. A later review added the boundary case for turning off a previous approach command.

The final **1,461 Edit Mode tests passed**, with zero failures/skips, September 12, 14:43:16–14:47:12 UTC (235.66 seconds). Existing global-target tests now explicitly switch M01 off in their basic-attack-only fixture; otherwise the still-equipped skill correctly gains a derived candidate ahead of basic attack. The earlier 14 failures are preserved, and two separate M01 integration cases verify exact global target selection on reachable targets.

| Native player check | Result |
|---|---|
| Draft and save | Actual buttons select GROUP, DENSE, and CURRENT. Draft edits preserve the hero; after saving, a newly opened store agrees with memory. |
| Combat and OFF | GROUP waits at one enemy and casts at three. Switching OFF in the editor preserves the in-flight projectile. |
| Independent restart | Restores OFF, travel/direction, cast policy, time, and paid resource. The original blast hits three enemies once each; no new or duplicate cast starts. |
| Layout and pause | Korean/English at 1280×720, 720×1280, and 640×360, including 140% text. Content scrolls and footer buttons stay within the safe area. Language changes preserve both running and manually paused entry states. |

The first native attempt exposed a clipped English 140% subtitle. Both language captions were shortened; **43 localization tests** and **two successful native launches, including restart**, then passed. Successful player processes ran from 14:47:55 to 14:48:16 UTC and produced 11 screenshots. English large-text options/decisions, compact landscape options, and Korean resumed decisions were visually inspected. The failed attempt and its nine screenshots remain preserved.

The app is `Builds/macOS-EdictFireball/HELLSCRIPT.app`. Evidence lives in `Artifacts/Validation/EdictFireball/`: `full-final.xml`, `caption-localization.xml`, and `Native2/`. Full regression covers the code immediately before the caption-only edit; the follow-up localization tests and native runs include that edit. Earlier compiler/test failures are retained, including corrected fixture API names, target geometry, sampled speed, and the incorrect assumption that M04 had a paid cost.

All 168 final runtime C# files and the English table match the native build source. There are no missing C# metas or duplicate GUIDs. `validated-source.tar.gz` preserves 636 source, design, and authored-settings files; SHA-256 is `3069881f696a8eae6c6a6a81be7872f55d37efe0fd0ef77085618c4eec8a97fe`. The isolated Unity import only appends Editor-only URP version metadata to the material. The workspace material and unrelated user settings remain untouched.

## Remaining work

This phase connects M01's four core options. W03, A01, A02, and M03 retain their previous aiming connections; they have not all been migrated away from legacy rule conditions. Remaining fixed policies include M02 placement purpose, preserving existing areas, and the LM01 follow choice. Some other options are already read by survival-response code, so subtracting four from 89 would not be an accurate count of unimplemented behavior.

Mobile-device performance, touch, and natural-play balance remain unverified. The separate [stage-20 affix comparison](Affix_Audit_Expansion.en.md) uses pre-M01 source and legacy behavior, and is not evidence for this new policy's balance.
