# Rift entry portrait banner: the info plate (U7) — 2026-10-21

[한국어](Rift_Entry_Banner_Plate_20261021.md). Record of fixing report 10 from the user's playtest of main c6354cbf, plus three comments from the mockup review. Mockup: `HELLSCRIPT 균열 입장 · 캐주얼` (Claude design canvas), the five "U7" portrait boards on the bottom row.

## Report and comments
- Report 10: on the rift screen the tier stepper, highest clear and "new challenge" float over the art, the first-reward card floats apart, and the text sinks into the bright, busy portal picture.
- Mockup comments (the user): ① the portrait page has awkwardly empty space ② the "24 tier" number is smaller than its neighbouring buttons ③ drop the "Auto-battle follows your Hunt Edict" line.

## Confirmed facts
1. The game uses a **bright portal painting** (`rift-portal-scene-wide`) as the whole portrait banner's background and puts the stage card, reward card and rule lines on it. The mockup (a plain sky) was never compared with it.
2. **Why the tier number was small:** in the mockup the number inside `<strong>` sits in an inner `span` created by the text hole, and the unit's rule (`.stage-number span`) also caught that span, so the number drew at 0.85×. The game's number font was the same 0.85× (`nf=.85f*B`), copying the mockup as rendered.
3. The portrait page draws the services strip ("Repeat settings · Combat records") in the gap between the skills and the entry footer, but `need` (the required height) did not include the strip, so a small gap lets it cover the skill tiles or the footer. It already overlapped in the user's 942×1510 screenshot and, with the taller banner, would at 440×956 (the smoke's `CheckFit` caught it).
4. The reward card's "ready" state needs a clear record (`riftProgress.best`, see `RewardBoxes.Status`).

## Changes (portrait only; landscape and PC are out of scope)
- Banner: the top strip (7.4 B) keeps only the painting and the title ("Rift", "INTO THE ABYSS" with a white halo); a cream **info plate** is attached under it.
- Plate, left group: the "Challenge tier" name with a hairline → −, the **tier number (3.2 B, gold)**, + spread across the column → a non-pressable inset box with "Highest cleared | New challenge (gold pill) or Best clear". At the plate's right end, a **first-reward card** of the same height (chest, name, state pill "Ready to claim / Locked / Claimed"; locked and claimed keep their grey and cream looks, ready keeps its red dot).
- Under the plate a gold-tinted strip holds the two rule lines. The "Auto-battle follows your Hunt Edict" line is removed in **every layout** and its `en.txt` entry was deleted (the orphan-entry test).
- Portrait page: the gap for the services strip is reserved first and the strip's height is part of the required height in `Render`'s shrink loop, so the unit shrinks instead of overlapping (this also removes the old overlap).
- New colour tokens `UiTheme.MockReadyInk` (#a85d00) and `MockReadyFill` (#fff1b8) for the ready pill (4.5:1). The locked tag's #5a6283 is a local constant.
- `RuntimeRiftEntrySmoke`: with `-hellscriptRiftEntryLayoutOnly` it stops after the tier step, the six sizes and the first-clear list, then captures lowest tier / new challenge / held record / claimed chest in portrait 440×956, Korean and English, reopening the window for each state.

## Checks
- EditMode `RiftEntryCasualWindowTests` (existing control and proportion checks plus new portrait plate checks: tiers 1, 24, 25 in Korean and 1000, 25, 24 in English keep the stage row, record box, reward card and rule strip inside the plate and page without overlap, number box height ≥ buttons, no auto-battle line, reward state tag), `RiftEntryTests`, `LocalizationTests`: 66 passed.
- Smoke (macOS development build, `-hellscriptRiftEntrySmoke -hellscriptRiftEntryLayoutOnly`): 440×956, 956×440, 1600×900, 1600×1000, 2100×900 and 2868×1320 in Korean and English with zero `CheckFit` problems, the tier-1000 first-clear list passed, eight portrait state captures.
- `check_ui_contract.py --verify` passed (15 tests), `check_ui_refresh.py` passed.
- Captures: `RiftBannerPlate20261021Evidence/` (the eight states, full 440×956 in both languages, landscape and PC).

## Not done · notes
- The banner is about 10 B taller, so on shorter portrait screens (e.g. 942×1510, about 54 B of height) the page unit B shrinks somewhat. I did not measure how much on that screen (the smoke uses fixed sizes such as 440×956).
- The tier number in the landscape and PC layouts is still 0.85× (same cause); it is outside this scope and was left alone.
- The rift entry screen was not captured at the user's 942×1510 size. No physical device or performance check.
- One line was deleted from `en.txt` (a mid-file deletion does not conflict with others appending at the end, but can overlap work near that line) and two token lines were added to `UiTheme.cs`.
