# Guide tutorial segment 2 integration

Updated: 2026-10-12

[한국어](Guide_Tutorial_Segment2_Plan_20261012.md). Implementation of the supplied plan in the existing guide tutorial (flow 6). The original plan is preserved as `original-segment2-plan.md` in the task evidence directory. See the [guide script](Guide_Tutorial_Script_20261011.en.md) for segment 1.

## Connected flow

Clearing the surrounding pack with Whirlwind now leads through the upper arena door and winding corridor into the gallery. On discovering three ranged enemies, Velina teaches the first engagement position. The hero walks toward them after the saved edit. The first actual Whirlwind hit on a gallery enemy opens the combat-position lesson. Saving Pack Center leads to a closing line; its confirmation resumes the remaining fight. Clearing that pack leads to [segment 3](Guide_Tutorial_Segment3_Plan_20261012.en.md); the closing dialogue and town graduation follow its final boss.

| Phase | Progression | Pause |
| --- | --- | --- |
| Whirlwind (6) | Only the surrounding group counts toward its clear → Road | Running |
| Road (7) | Upper door `(30,14)`, bends `(31,19)` and `(37,23)`, gallery door `(40,24)` and a short turn from `(44,24)` to `(44,21)` to see around the graves. A ranged enemy within 12.5 m with clear line of sight → OpeningTalk | Running |
| OpeningTalk (8) | Saved `position.engage=APPROACH` is required → Approach | Paused |
| Approach (9) | An actual outgoing W01 damage receipt targets the gallery group → CombatTalk | Running |
| CombatTalk (10) | Saved `position.mode=CENTER`, then confirm the closing line → FinalFight | Paused |
| FinalFight (11) | All remaining enemies defeated → Road3 (12) | Running |

The gallery uses field 0 roster slot 2 at `(54,21)`, `(57.5,24)`, `(54,27)`, with 140 HP and 2 attack each. Unseen enemies start at `lastSeenTime=-100`. Groups 0, 1 and 2 separate the first foe, surrounding pack and gallery pack. Tracking updates apply only to the active fight group; the road does not summon the gallery enemies toward the first arena. The existing zero-HP protection remains.

## Existing UI and save ownership

- Reuses `GameUI.EdictSheet`, `GameUI.EdictPosition`, `PrologueGate` and `TutorialAnchorRing` without a new panel or redesign.
- Starts with **Stand and Meet (STAND)** and **Walk (WALK)** preselected. New dialogue and guidance do not mention the other approach method. The panel keeps its two existing radio options.
- `GameStore.UnlockGuideEdict(run)` discloses exactly the three position options during the live first-engagement dialogue. Replays, detached checkpoints and phases without a lesson are rejected. Repeated disclosure is idempotent.
- First engagement and approach method join the shared TaughtOptions direct-edit allowlist. Undisclosed options remain validated; neither Details nor legacy access is granted before graduation.
- Disclosure reattaches the live checkpoint after the staged guide is adopted. `SheetApply` and `CommitHuntEdict(..., simulation, true)` save the owned hero and the running fight together.
- Each frame, the gate selects the next control from saved values: Hunt Edict → combat icon → position group when needed → requested position tab → Rush In or Pack Center. Reflow resolves newly created buttons again.
- The skill-only meaning of `GuideLessonActive` is preserved; lesson state distinguishes skill/opening/combat. Saving combat position opens a closing line whose confirmation resumes combat.
- Only flow 6 uses `GuideTutorial.Last=25` for checkpoint validation. Other tutorial limits, save validation and the existing restart-from-the-start policy remain.
- Velina speaks politely to the player. Segment 1 dialogue and graduation behavior remain; 19 new Korean-key/English translation rows are added.

## Validation

One completed full Edit Mode run on the integrated candidate: **6,036 passed, zero failed, four existing ignored**, 3,962 seconds. Native captures then exposed clipped scroll controls and narration covering a world rank. The affected GuideTutorialTests/PrologueGateTests **20 cases**, **15 UI contract regressions** and UI refresh checks (15 bindings) passed. Combat, save, data and packages match the full-run inputs; the entire suite was not repeated.

The final macOS development player passed **10/10** cases: KO and EN at 440×956, 956×440, 1600×900, 1600×1000 and 2100×900, default text size. Each case validates 44 gated EventSystem presses (skill 4, opening 3, combat position 4, target 16, treasure 9, gathering 8), world ranks during explanations, the paused goblin inside the viewport, manual close, real gold/rare gear pickup, reopening the save and one graduation. The clipped treasure control is scrolled into view and narration avoids rank one. The initial smoke projection mistake and UI-failure evidence are retained in the task validation directory.

[Summary](GuideTutorialEvidence20261012/validation.json), [full XML](GuideTutorialEvidence20261012/full-editmode.xml), [affected regression XML](GuideTutorialEvidence20261012/scroll-regression.xml), [KO report](GuideTutorialEvidence20261012/ko-runtime.txt), [EN report](GuideTutorialEvidence20261012/en-runtime.txt).

![Korean treasure lesson with rank one and the existing panel](GuideTutorialEvidence20261012/ko-treasure.png)

![English target lesson](GuideTutorialEvidence20261012/en-target.png)

Evidence is a Metal macOS development player with accelerated fixed combat ticks and synthetic EventSystem pointers. Ordinary-speed feel, human comprehension, physical mobile, WebGL and performance improvement are not established.
