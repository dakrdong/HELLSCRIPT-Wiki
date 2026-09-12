# Connecting Warrior Ground Slam hunt edicts

Date: September 13, 2026 · [한국어](Edict_Slam_Expansion.md)

Status: **All four W04 core options are connected. All 1,903 tests and three final macOS processes passed.**

## Scope

Continue from Warrior attack revision `f6f80ab` and independently connect all four W04 Ground Slam core options. Follow the ordinary-enemy, existing-control and boss policies in the [skill catalog](../Design/HELLSCRIPT_Hunt_Edict_Skill_Option_Catalog.md), and the [screen layout specification](../Design/HELLSCRIPT_Screen_Layout_Detail.md).

Claude's attributes, affixes and balance records remain on main. The [integration record](Attribute_Integration_Expansion.en.md) preserves the merge and subsequent review. At the user's request, the fully merged development branch was deleted locally and remotely. Further work proceeds on `main`.

## Combat decisions

| Setting | Connected behavior |
|---|---|
| Automatic use | OFF prevents ordinary and designated survival use. Missing, disabled or duplicated legacy rows cannot suppress or duplicate the independent core policy. Original rows remain unchanged. |
| Ordinary-enemy purpose | GROUP requires two ordinary/elite enemies within the real 3 m area. INTERRUPT requires an observed dangerous cast in progress. RETREAT requires a global emergency/dodge request and an opportunity for new control. |
| Existing control | DAMAGE imposes no control restriction. NEW_CONTROL requires an enemy with neither stun nor freeze. EXPIRING requires the longer remaining stun/freeze duration to be at most 0.35 s. This option does not restrict bosses. |
| Boss use | DAMAGE allows use during stagger or immunity. STAGGER requires neither state to be active. COMPLETE requires the existing 15-point credit to fill the current meter to 100. Ordinary-enemy opportunities are evaluated independently. |

These policies decide whether to cast. An actual cast still affects the full existing 3 m area; a boss excluded from the decision can still take real area damage. Visibility, walls, perception and pursuit exclusions are respected. A distant priority target does not hide a valid nearby group.

An observed cast is not immediately reported as successfully interrupted. A short enemy cast can release its hazard during Ground Slam's preparation; a later stun does not erase that hazard. A sufficiently long cast is interrupted by the actual hit and stun. Enemies leaving the area during preparation produce an actual miss.

## Survival order, action lifetime and saves

Designated global defense use respects the same ordinary-enemy and boss settings. A survival candidate must provide new control or complete boss stagger on impact. A previously selected emergency Leap Slam or walk is never delayed to prepare Ground Slam.

Ordinary group damage or boss damage alone cannot cancel an active attack. Actual ordinary-enemy interrupt/retreat opportunities obey the global cancellation permission, and travel/recovery locks remain in force. A lone boss cannot borrow the ordinary-enemy interrupt setting to cancel an unrelated attack.

Preserve base cost **0**, preparation **0.35 s**, cooldown **10 s**, damage coefficient **1.2** and ordinary-enemy stun **1.5 s**. Zero resource remains usable, and the existing LC02 discount is not consumed. Bosses receive the existing stagger credit, three-second stagger and immunity instead of direct stun. A committed preparation survives later setting edits and process restarts, completing only once without replaying damage, control or discounts.

## Validation and remaining work

| Check | Result |
|---|---|
| Full Unity Editor suite | **1,903 passed**, zero failures/skips. September 12, 2026, 21:09:41–21:15:02 UTC |
| Ground Slam coverage | **56 passed**, covering real radius, overlapping control, boss stagger, emergency priority, cancellation, misses and persistence. |
| Final native player | **Three processes passed**: save ordinary preparation, restore and stun two targets, save boss preparation, restore and complete actual stagger. |
| Visual review | **13 screenshots**, spanning KO/EN, 1280×720, 720×1280, 640×360 and 140% text. Reviewed the explanatory note, scroll body, fixed footer and action text; not every language/resolution combination was tested. |

The first test build referenced the wrong preparation-time field and failed compilation; the test was corrected. The next focused run revealed a test assumption that a short enemy cast produced a projectile. It actually releases a persistent hazard and replaces its action record, so the test now checks the actual RELEASE event and hazard. All 266 focused checks then passed. Two additional boss-only cancellation cases were included in the final 1,903-test result above. Earlier failures and intermediate player results are retained.

Keep the [validation summary](../../Artifacts/Validation/EdictSlam/validation-summary.json), [visual review](../../Artifacts/Validation/EdictSlam/visual-review.json), [source/player manifest](../../Artifacts/Validation/EdictSlam/validated-source.json) and [final macOS app](../../Builds/macOS-EdictSlam/HELLSCRIPT.app) together. Tests used isolated project copies and dedicated save directories.

This phase establishes functional integration, persistence and UI regression coverage, not new long-term balance results. W05, W06, Warrior BASIC, other class policies, generated-rift survival/interruption/progression/farming outcomes and mobile-device validation remain. No scene, prefab or package was replaced, and all 17 unrelated pre-existing working files are preserved.
