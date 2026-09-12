# Blizzard hunt-edict implementation record

Date: 2026-09-13 · [한국어](Edict_Blizzard_Expansion.md)

Status: **all five M02 core options are connected; the full suite and a new macOS app launch/restart passed.**

## Problem and resulting behavior

Blizzard's card saved its choices while combat still depended on legacy conditions and area movement settings. An opted-in hero with M02 equipped now derives an executable policy from the saved document. Missing, disabled or condition-blocked legacy Blizzard rows cannot override that policy. Duplicate rows produce one candidate without modifying the saved rule list. Opting out restores legacy behavior.

Fireball and Blizzard follow the common attack order and use their own names in decision history. Unlock level, actual resource cost, cooldown and action locks still gate execution. A global survival response that takes the main action prevents installation during that decision cycle.

## Five connected options

| Option | Runtime behavior |
|---|---|
| Automatic use | OFF prevents new installation decisions. Paid preparation and the remaining lifetime of existing areas continue. |
| Purpose | NEW_AREA must cover at least one enemy outside existing own areas. GROUP requires three expected targets, including one uncovered enemy. KEEP_TARGET requires the candidate to cover the current target and the **longest remaining lifetime** among existing covering areas to be at most one second. An uncovered target has zero remaining coverage. |
| Placement | TARGET uses the exact global target's current position; SELF uses the hero's current position. DENSE maximizes perceived coverage among legal candidates that satisfy the purpose. PATH projects observed velocity by 0.75 seconds and clips movement against terrain. Missing, older-than-0.4-second, future-dated or invalid motion, and immobilization, fall back to the current position. |
| Two areas remaining | KEEP holds installation while two valid own areas remain. REPLACE removes only the oldest own area when a qualified, authorized cast finishes. Foreign areas are preserved. |
| Movement | FIXED stays at the created position. FOLLOW requires LM01 and follows the currently perceived attack target at 1 m/s, up to 4 m total. Without LM01 the area is fixed while the original FOLLOW preference remains saved. The mode is captured at preparation start, so later edits, OFF and save restoration do not rewrite an ongoing cast or existing area. |

Only currently active own areas count as existing coverage. Hostile, foreign, expired and not-yet-active areas do not cover targets now. Created own areas awaiting activation still occupy the two-area KEEP limit. Empty owners in legacy saves retain their existing interpretation as this hero's areas.

DENSE considers enemy centers, pair midpoints, radius-three circle intersections, range boundaries and near-side alternatives. This handles triangular groups and targets whose centers are beyond the installation range. Every candidate passes the actual 10 m range, landing and sight checks. Hidden, dead or globally excluded enemies do not inflate the count. Forecasts use the observed formation and do not guarantee hits after enemies move.

The existing runtime still applies only the stronger overlapping Blizzard damage. MP02 slow, SM two-piece cost, SM four-piece exposure and LC02 discounts retain their real effects. Blizzard does not wait for Fireball; integration checks cover Fireball consuming Blizzard's own exposure on a real hit. Damage values and affix weights are unchanged.

## UI and persistence

The card now explains that all five settings affect combat and movement edits apply to new areas. Korean and English text are supplied together. Decision history resolves Blizzard's name instead of labeling every derived policy as Fireball. Validation follows the [screen layout specification](../Design/HELLSCRIPT_Screen_Layout_Detail.md): scrollable content, accessible footer actions, enlarged text and compact landscape layouts.

The save schema, legacy rule list, area ownership and identities, scenes, prefabs and packages are preserved. `edict:M02` identifies a runtime-derived policy. Validation uses isolated Unity projects and dedicated saves.

## Validation

The initial 245-test focused run passed. Additional terrain, follow-limit and ordering checks brought the final focused run to **250 passing tests**, including 53 new M02 cases. Coverage includes ownership and active windows, the one-second boundary, population thresholds, legal placement, exact global targeting, observed motion, approach/OFF, cost and discounts, deterministic save replay and SM4 Fireball integration.

The final **1,514-test Edit Mode suite passed**, with zero failures or skips, from 2026-09-12 15:18:44 to 15:23:32 UTC (287.75 seconds). Existing basic-only global-target fixtures explicitly disable M02. The unsupported-aiming fixture now uses M05, since M02 has an aiming stage.

| Native macOS check | Result |
|---|---|
| Editing and persistence | Actual buttons selected GROUP, SELF, KEEP and FOLLOW. The saved hero remained unchanged while editing; memory and a newly read save agreed after applying. |
| Installation decisions | GROUP waited at one target. With three targets and two existing areas, KEEP held installation; saving REPLACE started preparation. |
| Independent process restart | Saving FIXED/OFF during preparation preserved existing fields and the paid FOLLOW cast, time and resource. Exactly one release replaced only the oldest field; the new area followed and dealt damage. Areas expired normally, with no extra cast or duplicate payment. |
| Layout and pause | Korean/English and 140% text were checked at 1280×720, 720×1280 and 640×360. Text-height, missing-translation and footer safe-area checks passed. Language redraw preserved both running and manually paused entry states. |

Both native processes passed between 2026-09-12 15:18:59 and 15:19:19 UTC. Ten screenshots were retained. Enlarged English options, compact landscape options, Korean restored decisions and the following-area combat screen were visually inspected. These fixtures control enemy placement and simulation time while exercising real editing, persistence and restoration.

The app is `Builds/macOS-EdictBlizzard/HELLSCRIPT.app`. Evidence is under `Artifacts/Validation/EdictBlizzard/`: `focused-final.xml`, `full.xml`, `Native1/` and `validation-summary.json`. All 170 Runtime C# files and the English table match the build source. No C# metadata is missing and no asset GUID is duplicated. The source/design/settings archive contains 642 files, SHA-256 `faf2336cc02a1dba6c250e94cdf0fd7448fe0b529450e585e60bb96c677370a4`. Unity's imported URP Editor material-version metadata is recorded separately; the authored material remains intact.

## Remaining work

This phase covers the five M02 options. Next: remove legacy-condition dependencies from M03's purpose, first target and chain selection, and investigate the Chain Control build's low clear rate in the completed affix comparison using timing, linking and damage evidence. This phase does not establish multi-seed progression balance or mobile-device performance and touch quality.
