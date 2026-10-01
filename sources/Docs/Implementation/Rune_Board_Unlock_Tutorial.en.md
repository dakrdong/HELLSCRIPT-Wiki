# Rune Board Unlock and Playable Guide

Updated: 2026-10-01 · [한국어](Rune_Board_Unlock_Tutorial.md)

After a normal Rift 15 clear, **Injel Mir, the Rune Master**, begins the lesson on the result window. Claiming the first-clear reward sends the Rune Board emblem to the upper-right content menu. NPC explanations alternate with actual play in the existing five-step guide, followed by free use of the actual weapon boards.

## Player flow

1. Before unlock, the town and battle content menus omit the Rune Board shortcut.
2. An exact normal Rift 15 clear starts the Rune Master's result dialogue. A highest-clear number alone, training or the prologue cannot trigger it. Automatic repeat and its cleanup wait during this lesson.
3. Only the highlighted **Claim Reward** control is reachable. The transaction grants the first-clear boxes and opens the included **Rune Block Set**, adding five types of G0 single-cell rune to actual storage. Other reward boxes remain unopened. It grants no mastery XP.
4. After successful saving, the emblem flies to the upper right. Its landing checkpoint is saved before the shortcut becomes normally visible. Reduce Motion uses a short landing.
5. The highlighted shortcut opens the existing native board. The Rune Master **explains each step's rule, controls and goal before practice**. **Next** becomes available only after the player achieves that step's goal.
6. After all five steps, the Rune Master introduces actual weapon boards and saving, inviting the player to **choose any weapon and arrange runes freely**. The lesson ends with the board open and all controls available.

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

The existing `rune-starter` definition in `RewardBoxes.json` is named **Rune Block Set / 룬 블럭 세트**. The Rift 15 first-clear promise retains this required gift even with an operational reward configuration. Ordinary box openings and operational grants cannot count as the unlock reward.

The UI connects the existing `StoryDialogueWindow`, `PrologueGate`, `TutorialAnchorRing`, `RuneBoardWindow` and five-page `RunePracticeModel`. It adds no separate frame, placement validator or puzzle. Only mandatory practice applies completion gates, restricted navigation and progress saving. Actual weapon editing retains `RuneBoardSession` and `GameStore.CommitRuneBoardState`. It follows the rune adapter in the [shared UI contract](Shared_UI_Contract.en.md).

Restart resumes the saved checkpoint. Closing after claim but before landing repeats the emblem flight; closing during practice restarts the unfinished page's NPC explanation and puzzle. Completed pages do not repeat. Existing accounts without this state keep their access if Rune content is already unlocked or their highest clear is at least 15; they do not repeat the mandatory lesson. Earlier locked accounts wait for the new introduction.

## Validation

Nine focused Edit Mode cases passed for actual rewards, failed-write rollback, idempotency, rejected page jumps and unfinished puzzles, per-page reload, unchanged actual ownership, older-save migration and translations. Native macOS input passed in Korean and English at 440×956, 956×440, 1600×900, 1600×1000 and 2100×900 (10 cases). NPC narration changed to begin directly with instructions, followed by two focused checks in Korean portrait and English landscape. The development build succeeded with zero errors; shared UI ownership and all 11 contract checks passed. The full suite belongs to separate integration validation. Physical mobile was not tested.

[Validation record](RuneUnlockEvidence20261001/validation.json) · [Focused Edit Mode results](RuneUnlockEvidence20261001/focused-editmode.xml) · [Native matrix](RuneUnlockEvidence20261001/native-matrix.json)

[Result NPC](RuneUnlockEvidence20261001/result-npc-ko.png) · [Step 4 explanation](RuneUnlockEvidence20261001/step-four-npc-ko.png) · [Step 4 goal](RuneUnlockEvidence20261001/step-four-complete-ko.png) · [English landscape explanation](RuneUnlockEvidence20261001/step-five-npc-en-landscape.png) · [Free-placement invitation](RuneUnlockEvidence20261001/free-placement-en-landscape.png)
