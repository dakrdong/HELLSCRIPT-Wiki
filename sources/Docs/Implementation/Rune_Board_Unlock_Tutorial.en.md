# Rune Board Unlock and Playable Guide

Updated: 2026-10-24 · [한국어](Rune_Board_Unlock_Tutorial.md)

After a normal clear of Rift 10 or higher, the first time the hero arrives in the town plaza after leaving the Rift, **Injel Mir, the Rune Master**, begins the lesson (never in battle or on the result window). Until then the Rune Board slot of the upper-right content menu holds a locked icon; tapping it says "Opens when you reach Rift Level 10". After the welcome gift (the Rune Block Set) the padlock breaks, light spreads and the Rune Board emblem lands on that slot. NPC explanations alternate with actual play in the existing five-step guide, followed by free use of the actual weapon boards. It moved from stage 15 to stage 10 on 2026-10-24 ([change record](Rune_Unlock_Rift10_20261024.en.md)).

Rift stages 1–9 grant no rune block drops from normal enemies, elites or bosses. `RuneEconomy.MinimumDropStage` reads the Rune Board unlock stage (10) from `ContentUnlocks`; `RuneGrowth` gates rewards by the actual run stage, including lower-stage repeats on already-unlocked accounts. Existing ownership, mastery XP and duplicate-reward receipts are preserved.

## Player flow

1. Before unlock, the town and battle content menus hold a locked icon (`menu-rune-board-locked`) in the Rune Board slot and a tap shows a toast. After unlock but before the introduction starts it says "The Rune Board opens after its introduction in town." The slot keeps its position, so the dock never jumps when the board opens.
2. Reaching Rift 10 or higher → the Rift result is processed → the **first arrival in the town plaza** starts the lesson once. It waits for other popups or tutorials to close and for any suspended Rift to end, and a pending lesson survives restarts. Automatic repeat and its cleanup wait during the lesson.
3. Injel Mir explains what runes are and where they come from, then gifts the **Rune Block Set**. The claim transaction creates a fixed-ID box and opens it in the same transaction, adding five single-cell G0 runes of different types to the real storage. It is independent of first-clear boxes and grants no mastery XP.
4. After a successful save the padlock breaks off and light spreads (new-feature notice and sound), then the Rune Board emblem flies to the upper right and lands on that slot. The icon turns normal only after the landing checkpoint is saved. Reduce Motion uses a short version.
5. The shown content icon opens the existing native board. The Rune Master **explains each step's rule, controls and goal** before the playable guide. **Next** opens only after the goal is reached by actual play.
6. After all five steps, the Rune Master introduces actual weapon boards and saving, inviting the player to **place runes freely on any weapon**. It ends the tutorial, leaves the board open and releases every control.

| Step | NPC explanation and play | Requirement for Next |
| --- | --- | --- |
| 1 | One-cell gaps between matching colours; C-shaped block | Valid placement of the practice block |
| 2 | Gaps from two existing matching blocks; diamond block | Valid placement of the practice block |
| 3 | Different colours may touch; blue block | Valid placement of the practice block |
| 4 | Select, rotate, place; matching abilities | Activate two red ability cells |
| 5 | Alternate selection, rotation and placement of two colours | Activate two ability cells per colour |

Fast-forwarding dialogue only skips sentences. The mandatory guide blocks page jumps, Previous, Close, background dismissal and Back. Practice uses the existing `RunePracticeModel` pieces and cannot change rewarded runes, actual weapon layouts, mastery or unlocked cells. Reopening the guide from Help after completion retains the existing optional navigation and dismissal. Korean and English dialogue use the existing NPC portrait.

## Ownership and persistence

Account-wide `AccountGuide.runeBoard` stores `Waiting → Reward → Emblem → OpenBoard → Practice → Complete`, the next practice page (0–5) and granted instance IDs. `RuneBoardTutorial` evaluates progress; `GameStore.Transact` atomically commits rewards and checkpoints. Fixed request IDs prevent duplicate grants and repeated page advancement on retry. Failed writes do not adopt successful state.

The Rune Block Set (`rune-starter` in `RewardBoxes.json`) is paid by the lesson itself, not by a first-clear promise. `GameStore.ClaimRuneBoardTutorial` creates the fixed-ID box `rune-lesson-starter` and opens it in the same `Transact` (retries and save-failure rollback are covered), and the Rift 15 first-clear package no longer carries this set (the server contract `server/testdata/liveops-contract.json` and the generated schema were updated together). Ordinary box openings and operational grants cannot count as the unlock reward. A save stuck in the `Reward` step of the old Rift 15 result-window lesson consumes the set of its saved stage-15 promise and receives no second set.

The UI connects the existing `StoryDialogueWindow`, `PrologueGate`, `TutorialAnchorRing`, `RuneBoardWindow` and five-page `RunePracticeModel`. It adds no separate frame, placement validator or puzzle. Only mandatory practice applies completion gates, restricted navigation and progress saving. Actual weapon editing retains `RuneBoardSession` and `GameStore.CommitRuneBoardState`. It follows the rune adapter in the [shared UI contract](Shared_UI_Contract.en.md).

Restart resumes the saved checkpoint. The wait before the lesson is decided by `ContentUnlockState.introduced` (content whose introduction is done) and `introSeeded`. A save without those fields records every feature that was already open on its first read as introduced, so accounts that already had the board (Rift 15 or higher, everything open, lesson in progress or complete) never replay the lesson and are never locked. A Rift 10–14 account whose board was still closed opens it on that read and receives the lesson once on its first town arrival. Closing after the claim but before landing repeats the lock break and emblem flight; closing during the guide restarts at the unfinished page's NPC explanation and practice. Completed pages never repeat.

## Validation

2026-10-03 drop-stage update: **53 focused Edit Mode cases passed**, covering normal/elite/boss rewards at stages 1–14 and 15–16, already-unlocked accounts, guaranteed operational rates, persistence, duplicate protection and the first-clear set. macOS Unity Editor checks covered the changed labels in Korean/English at five viewport sizes and native Rune Board → Info → Drop Rates button callbacks. The full game suite, a separate development build and physical-mobile validation were not run for this small rule change.

[Test results](RuneDropGateEvidence20261003/focused-editmode.xml) · [UI scope](RuneDropGateEvidence20261003/ui-validation.json) · [Korean portrait](RuneDropGateEvidence20261003/ko-game-view.png) · [English portrait](RuneDropGateEvidence20261003/en-440x956.png)

Nine focused Edit Mode cases passed for actual rewards, failed-write rollback, idempotency, rejected page jumps and unfinished puzzles, per-page reload, unchanged actual ownership, older-save migration and translations. Native macOS input passed in Korean and English at 440×956, 956×440, 1600×900, 1600×1000 and 2100×900 (10 cases). NPC narration changed to begin directly with instructions, followed by two focused checks in Korean portrait and English landscape. The development build succeeded with zero errors; shared UI ownership and all 11 contract checks passed. The full suite belongs to separate integration validation. Physical mobile was not tested.

[Validation record](RuneUnlockEvidence20261001/validation.json) · [Focused Edit Mode results](RuneUnlockEvidence20261001/focused-editmode.xml) · [Native matrix](RuneUnlockEvidence20261001/native-matrix.json)

[Result NPC](RuneUnlockEvidence20261001/result-npc-ko.png) · [Step 4 explanation](RuneUnlockEvidence20261001/step-four-npc-ko.png) · [Step 4 goal](RuneUnlockEvidence20261001/step-four-complete-ko.png) · [English landscape explanation](RuneUnlockEvidence20261001/step-five-npc-en-landscape.png) · [Free-placement invitation](RuneUnlockEvidence20261001/free-placement-en-landscape.png)
