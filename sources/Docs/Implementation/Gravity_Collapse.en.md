# Gravity Collapse elite

Updated: 2026-10-06 · [한국어](Gravity_Collapse.md)

This implements B1–B3 of the puzzle tutorial plan. N10 with explicitly assigned E07 is displayed as the Gravity Caster. Ordinary rift and training-ground elite rolls remain unchanged and do not roll E07. No enemy kind, model, image or audio asset is added. The L7 runner and scout/recap cards belong to later stages.

## Behaviour and decisions

The first cast is after 4s, followed by a 10s cooldown. Existing 14m and line-of-sight checks select the observed hero position for a fixed 2.5m-radius circle. During its 5s delay, each fixed 0.05s tick pulls the hero toward the centre at 8m/s. It then deals one shadow hit at attack ×20. Existing `Map.MoveDirect` handles walls. Travelling leaps ignore the pull; landing outside ends it. Control resistance does not reduce it. Preparation is not repeatedly interrupted and paths are not recalculated each tick.

The serialized `pull` field defaults to zero in old saves. Seven trait timer slots preserve the six existing remaining cooldowns; the added slot starts at 4s according to initialization state. No random numbers are consumed.

The forecast records one area hit and `IncomingControl.Pull`. WALK_FIRST, the WALK order entry and the previous walking-response shortcut all refuse walking while pulled. Escape candidates inside an active pull circle are rejected even before the normal 1.5s damage horizon. Ranger target perception, the Warrior attack-leap cooldown and escape reservation, and existing preparation/travel/recovery checks remain intact.

## Presentation and audio

Existing `WorldFx` pools, meshes, gauges and impact effects are reused. Twelve inward arrows distinguish this warning; the view, mesh and material are reused. `WorldView` reads the actual fixed hazard position and delay. The shadow impact borrows `hazard.blast` at release and N10's windup cue at creation.

The countdown reuses the existing boss-status HUD adapter on the battle canvas. A boss takes priority; otherwise an observed gravity warning is shown. Text updates only when its hazard ID, integer second, language or HUD instance changes. No account writes, save subscriber, new window, canvas or font is added. Existing safe areas, responsive layout and shared fonts remain. Performance improvements have not been measured and are not claimed.

## Validation and delivery

All nine core checks failed before implementation and passed after it. The default survival policy without a designated escape dies in the first explosion; even 6.6m/s walking cannot exit. Designating W02/A04/M04 avoids all hazard damage during three cycles. These isolated trait checks disable attacks and keep the caster alive. Stage 3 tests actual attacks, skills and editing transactions in the Lv5 L7 win/loss scenarios.

The independent branch uses existing unlock levels: Warrior Lv5, Ranger/Mage Lv10. After integration with stage 1 the same checks use Lv5 for all classes; stage 3 records the actual tutorial gate. Unlocks are not bypassed and default edicts are not modified.

All 326 distinct focused Edit Mode cases passed (0 failed, 0 skipped), including restored-state equality, shadow damage forecasts, maximum walking, early escape, previous walking rejection, old cooldown preservation, ordinary rift trait selection, journal/localization/audio/gauge coverage, pooled geometry, the warmed allocation-free FX path and StoreViewBinding. One initial fixture omitted previous walking memory; only that failure and directly affected coverage were re-run.

A macOS Unity 6000.6.0f1 Metal Editor scene verified the inward ring, pull, release flash and 5/3-second countdown. Saved Game View PNGs were inspected in KO/EN at 440×956, 956×440, 1280×720, 1280×800 and 1680×720. Pull moved 1.2m over 0.15s. The visual fixture raised HP to 10,000 to retain the scene after release; lethal balance is proven by the simulation tests. This is not physical input, save UI, player performance, browser or device evidence. All 11 common UI tests and refresh checks (12 bindings, 161 sources) passed.

Evidence: [focused results](../../Artifacts/PuzzleTutorial/20261006/stage2-validation.json), [visual results](../../Artifacts/PuzzleTutorial/20261006/visual/validation.json), [layouts](../../Artifacts/PuzzleTutorial/20261006/visual/gravity-layout-contact.png), [warning](../../Artifacts/PuzzleTutorial/20261006/visual/gravity-ko-1280x720.png), [pull](../../Artifacts/PuzzleTutorial/20261006/visual/gravity-pull-ko.png), [release](../../Artifacts/PuzzleTutorial/20261006/visual/gravity-release-ko.png).

The full Edit Mode suite, runtime smoke batch and physical-device checks are not run at this stage. Merging and public wiki publication belong to the merger. The checkout and evidence are retained for PR review, with cleanup reviewed on 2026-10-13.
