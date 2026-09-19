# HELLSCRIPT Locked Rune Region Preview

> Historical implementation evidence. The current board rules and elite values are superseded by [v13](../Design/HELLSCRIPT_Rune_Mastery.en.md) on 2026-09-20. Earlier test results remain historical.

Date: 2026-09-17
작성일: 2026-09-17

## The delivered plan

The user handed over `HELLSCRIPT-RuneBoard-Elite-Preview-v13.zip`, a UI revision package that updates the v12 web mock-up. Its instructions are in `CLAUDE-PATCH-v13.md` and `CHANGE-SPEC-v13.md`.

There is one requirement: pressing a locked outer region previews **only the ability of that region's exact centre hex**. The instructions pin down how:

- Do not hard-code six descriptions by region name. Find the real node from the selected weapon and the region's centre coordinate, then read the ability that node points at.
- If the centre data is absent, say so instead of substituting another ability.
- Reading the preview must never unlock a slot or activate an ability.
- Unlock conditions, costs, connection rules and board input policy stay unchanged.
- Do not report the HTML's own automated checks as Unity or real-device verification.

## Where the mock-up and the game differ

The mock-up and the Unity game are two different boards wearing the same name. Comparing them first is what set the scope of this change.

| Item | Web mock-up v13 | Unity game (today) |
|---|---|---|
| Regions | Six named regions: Assault, Precision, Spread, Artistry, Circulation, Guard | Seven colour-grade regions, G0–G6 |
| Region unlock | Fill the 37 central hexes, choose one region, buy its other 36 hexes at 1P each | Opens automatically every 5 account-best rift clears; no points |
| Region centre | A dedicated `color:"elite"` hex, activated by a rune of any colour | An `isStart` hex, activated only by its own region's colour |
| Centre ability | Combat Heat, Exposed Weakness, Chain Collapse, Skill Inheritance, Cycle Core, Unyielding Ward | Attack power on all six weapons at the time of this work; the six were added afterwards in [Elite Abilities](Rune_Elite_Centres.en.md) |
| Padlock | A locked region is drawn as a single padlock marker | A locked region still draws all its hexes; they are dark but pressable |
| Ability icons | Every ability has a `glyph` symbol | The rune board has no ability icons; borders, dots and crosses carry the meaning |

So the mock-up's padlock popup cannot be carried over as drawn. Its `Unlock` button and its `Centre hex free · other 36 × 1P` footer have no matching rule in Unity at all, and the six elite abilities it previews are marked `origin:"new"`, `source:"proposal"` in the mock-up's own data — they do not exist in the Unity ability set.

What does carry over is the **structure**: before a region opens, read its centre hex from real data and show it, and make sure reading changes nothing.

## What was applied

Unity already has the gesture that corresponds to the padlock. Pressing a locked hex runs `InspectRuneCell`, which writes that hex's ability and `This region is locked.` into the selection note below the board. Rather than build a new popup, the centre preview was appended there.

Pressing a hex in a locked region now adds two or three lines after the existing three:

```
Attack power +0.4
Required: G1 · Value tier 0 · Region unlock G1
This region is locked.
The Ivory region opens at account-best rift tier 5.
Centre start hex preview · Attack power +0.4 · Requires Ivory
A preview only. No hex opens and no ability turns on.
```

Pressing the centre hex itself omits the preview line, because it is already the line above.

The lookup order is exactly the one the instructions give:

1. The board of the weapon being edited, `runeEditor.Board`.
2. The pressed hex's region grade, `cell.RegionGrade`.
3. That grade's start coordinate, `Board.TryGetStart(regionGrade, …)`.
4. The real hex at that coordinate, `Board.TryGetCell(…)`.
5. Its name from `Ability.Meaning`, its number from `Value` and `Unit`, its required colour from `RequiredRuneGrade`.

If the hex that comes back does not belong to that region, or is not a start hex, nothing is substituted: the note reads `This region has no centre hex to read.`

The tier in the unlock sentence comes from `RuneMasteryCatalog.ClearForGrade`. The unlock rule `UnlockedGrade` is `account-best clear / 5`, so its inverse sits on the next line as a one-liner. If the two drifted apart the preview would name the wrong tier, so the test checks all seven grades against each other.

## What was left alone

Ability data, board coordinates, the forced start hex, colour requirement checks, region unlock rules, the save schema and its keys, rune placement, removal, rotation and dragging, storage filters, fusion, presets and the placement practice are unchanged. Board drawing (`RuneBoardGraphic`) was not touched either: a locked region already draws all of its hexes, so there is somewhere to press and no padlock marker was needed.

The mock-up's two buttons — `Rune board guide` and `Unlock` — and its cost footer were not carried over. With no matching rule in Unity, carrying them over would advertise a feature that is not there.

Ability icons were not added. Rune board abilities are defined only by a stat meaning (`AttackPower` and so on) or a skill number, and have no artwork. Adding icons would mean drawing one per ability, which is outside this change.

## Verification

Unity Edit Mode, run on a copy of the repository.

| Target | Result |
|---|---|
| `RuneMasteryTests` · `RuneEconomyTests` · `LocalizationTests` | 74 / 74 passed |

The new `EveryLockedRegionCentreIsItsOwnStartSlotOnTheSelectedWeaponBoard` checks, across six weapons × seven regions:

- Each region's centre is its own start hex and requires its own colour.
- No two regions share one centre.
- Every hex of a region lies within radius 4 of that centre, which is what makes it the true centre.
- The centre ability's name is not empty.
- The region opens at the tier `ClearForGrade` names, and does not open one tier early.

The existing `EveryKoreanLiteralInTheRuntimeHasAnEntry` covers the English entries for the four new lines.

Reading a locked region was added to the macOS development build's runtime smoke, `-hellscriptRuneSmoke`. In Korean at 720×1280 portrait and English at 1280×720 landscape it presses (5,-8), a real non-centre hex of region 1, and confirms all six lines appear; it then presses the centre (5,-9) and confirms the preview line is not repeated. Finally it confirms the account save JSON, the unlocked grade and the placement count are byte-identical before and after. The evidence is the [Korean portrait screen](RuneRegionPreviewEvidence/06-locked-region-preview-ko.png), the [English landscape screen](RuneRegionPreviewEvidence/07-locked-region-preview-en-landscape.png), the [run log](RuneRegionPreviewEvidence/runtime.txt) and the [smoke output](RuneRegionPreviewEvidence/smoke-result.txt).

**The smoke exits as a failure, for a reason that predates this change.** Every captured screen flags one label: the global observation HUD's `Shield` value, which is empty while its 9.6-high rect is shorter than its 12-high font line (13.44 against 16 at 140% interface scale). Rebuilding and rerunning the same smoke from a clone with all five files of this change reverted to HEAD reports the identical failure on all nine of its screens; the two screens this change adds report the same one label and nothing else. It belongs to the global HUD work and was left alone.

The checks ran on a macOS development build only. This was not seen on real mobile hardware. The QA results inside the mock-up ZIP describe the browser mock-up and were not used as evidence for this implementation.

## Remaining work

The mock-up's six elite abilities (Combat Heat, Exposed Weakness, Chain Collapse, Skill Inheritance, Cycle Core, Unyielding Ward) were not in the game when this was written. **All six were added the same day in [Elite Abilities at the Rune Region Centres](Rune_Elite_Centres.en.md).** This preview reads the new centre abilities without one line of change. The start hex keeps its colour requirement, so the mock-up's any-colour activation was not carried over; that document says why.

Buying slots with points, per-ability icons and a padlock-popup screen belong to a separate effort that brings the whole rune board in line with the mock-up.
