# Tutorial dim and battle HUD legibility (U3) — 2026-10-21

[한국어](Guide_Dim_And_HUD_Readability_20261021.md). Record of fixing reports 3 and 4 from the user's playtest of main c6354cbf.

## Reports and the user's answer
- 3: During the tutorial the top-left lines (goal, time, targets, ready) and the boss line at top centre disappear into the bright floor. Make them readable without shrinking the text.
- 4: The portrait treasure-goblin priority lesson is not dimmed. The user's answer: keep **only the card to move, its landing place (the drag guidance area) and the explanation panel bright** and dim the rest of the battle. The goblin row must show whole, not cut off at the bottom of the window.

## Confirmed facts
1. **The tutorial uses the old battle HUD.** `GameUI.CasualCombatHud` is true only when `!tutorial`, so the tutorial and the training ground draw the old lines without an outline (`GameUI.BattleLayout.cs`). The tan text in the user's screenshot is that. The ordinary battle HUD already has the navy double outline (`CasualBattleHudStyle.WorldOutline`).
2. **On a bright floor the old lines have 1.2–1.4 : 1 contrast.** Measured in `Docs/Implementation/GuideReadability20261021Evidence/` (WCAG ratio between the ink and the 1–3 px around the glyphs; pale, mid, dark, tiled and the user's screenshot floors): only the dark floor reaches 8.7 : 1. Bright ink inside a navy outline (the widths shipped: 1.0 from size 17, 0.8 below) gives a median of 8.6–8.7 : 1 on the same floors and 2.7–4.0 : 1 for the worst 10% (the measured ring is wider than the thin outline, so the worst 10% is conservative). A backing plate alone gives only 1.4–2.2 : 1; outline + plate reaches 4.3–5.7 : 1 for the worst 10% but darkens a corner of the world. So the outline alone was chosen.
3. **There was a state with no dim.** `PrologueGate` makes no dim pieces when there is no opening (control). While the treasure card appears (0.6 s) `GuideTargetStep` returned a null target, so the screen only blocked input, with no dim and no guidance. The user's screenshot 2 is that state (the voice line is the intermediate "Move 'Treasure monster'…" text) with the treasure row cut by the list. The waiting state also never called `EnsureVisible`.
4. **Explanation steps passed the whole screen as the bright area.** `ShowMessage(..., root, ...)` made `bright` the root, so there was effectively no dim during explanations.
5. **An editor-only trap sat in the treasure reveal.** `GuideTreasureReveal.Awake` and the drag ghost (`HuntEdictOrderCard`) used `GetComponent<CanvasGroup>()??AddComponent`. In the editor a missing component only looks null to Unity's `==`, so `??` does not add it and the next access throws `MissingComponentException`. `CasualEdictSurfaceTests.GuideTreasureReveal…` fails on main with exactly this exception, and the existing `EditorNullPatternTests` flags the same trap. In a player build the value is a real null, which is why the smoke passed.

## Hypothesis (not verified by running)
- If the waiting line stayed up for long in the user's editor run, the `Awake` exception in (5) may have frozen the reveal clock. I did not reproduce it in editor Play mode. Either way the wait can no longer exceed 1.5 s.

## Changes
- `PrologueGate`: an option to keep the dim while the control is missing (`dimWithoutTarget`; input blocking and dim stay and both let go together after `MissingGrace`), a list of areas kept bright (the opening, a band, the drag lane, merged extra areas), a **drag lane** joining the card and its landing place (`Lane`: union of the two rectangles + 6 px), 32 dim quads, a one-line `Describe()` of a frame, and the absence time per frame is capped at 0.25 s so one long frame cannot release the gate.
- `GameUI.GuideTargetLesson`: every target step keeps the dim. During the treasure card's reveal only that card is bright and it is brought whole into its list before waiting; the wait ends with the reveal clock or at 1.5 s, whichever is first. A drag step shows the card and its landing place together when the list has room (`UiVisibility.EnsureVisible(target, also)`), otherwise the card comes first, whole. An explanation lights only its panel and the rank markers over the monsters.
- `GuideTreasureReveal`, `HuntEdictOrderCard`: `TryGetComponent`.
- Old battle HUD (`InkLegacyBattleText`): casual gold / white ink with the navy double outline on each line; sizes under 17 get a thinner ring (0.8). Text size and layout are unchanged. Applied once per built line.

## Checks
- Focused EditMode: `PrologueGateTests` (a portrait five-card list: the fifth card is cut → after scrolling the card and its landing place are whole together; card first when both do not fit; fallback for another list; dim while waiting and release after the grace; drag lane, bright explanation areas and the quad limit; existing input blocking and explanation advance kept), `CasualEdictSurfaceTests`, `EditorNullPatternTests`.
- `RuntimeGuideTutorialSmoke`: in the target lessons it writes `gate-trace.txt` (every 0.25 s: step, reveal clock, card visibility, opening, bright areas, dim quads) and asserts that the dim is present while waiting, that waiting stays under 3 s, that the card is whole and that the drag lane exists.

## Not done · decisions for the user
- Not confirmed in editor Play mode whether the long wait was really the `Awake` exception.
- `RiftGearSettingsWindow.cs:22` and `GameUI.GuideGearLesson.cs:72` still carry the same editor trap (the session that owns auto-select settings and gear lessons was told).
- The ordinary battle HUD's boss line already has the casual outline, so it was left unchanged.

## Results
- macOS development build (`ProjectBuilder.BuildMac`) succeeded. `RuntimeGuideTutorialSmoke` passed (result files `smoke-*.txt`, no failure file) in Korean 942×1510 (the user's screenshot size), Korean 440×956 and English 956×440. Synthetic pointers and accelerated ticks only; no claim about real speed, human comprehension or physical devices.
- Treasure lesson `gate-trace-ko-942x1510.txt`: reveal clock 0.35 → 0.57 s, the wait ended at 0.57 s; while waiting 4 dim quads, the bright band is the treasure card and its visibility is 1.00. The explanation step has 13–15 dim quads (only the panel and the rank markers over the monsters stay bright). The player build has no `Awake` exception, so the stall was not reproduced here.
- Captures (`GuideReadability20261021Evidence/`): `ko-942x1510-treasure-waiting.jpg` (dimmed while waiting, card whole), `ko-440x956-treasure-drag.jpg` (only the drag lane bright, rows 1–5 on one screen), `en-956x440-treasure-waiting.jpg`, `ko-942x1510-treasure-explanation.jpg`, `hud-after-fight-crop.jpg` (boss line with the navy outline over the floor), `hud-current-vs-ring.jpg` and `contrast.txt` (contrast per floor).
- Focused EditMode: first run 131 of 133 passed. Failure 1 is the two gear-owned files flagged by `EditorNullPatternTests` (`GameUI.GuideGearLesson.cs:72`, `RiftGearSettingsWindow.cs:22`), not created by this work. Failure 2 was the waiting-dim test flaking with frame length, so the per-frame absence time is now capped at 0.25 s (`PrologueGate`) and the run repeated. Result: all 49 tests of `PrologueGateTests` and `CasualEdictSurfaceTests` passed (the treasure reveal test included). `EditorNullPatternTests` keeps failing on those two gear-owned lines (outside this bundle's check scope).
- Static checks: `check_ui_contract.py --verify` passed (15 tests) and `check_ui_refresh.py` passed (14 bindings guarded, 235 UI sources).
