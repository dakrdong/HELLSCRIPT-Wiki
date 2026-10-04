# Tutorial freeze investigation and unchanged Hunt Edict save cost

Updated: 2026-10-05 · [한국어](Tutorial_Save_Hitch_20261005.md)

We investigated reports of irregular complete freezes during the tutorial. The supplied warrior boss-fight recording shows pauses of about 0.75 seconds and adjacent pauses totaling about 1.58 seconds. Gaps in a variable-frame-rate recording support the visible observation; they do not identify the CPU operation blocking each frame.

## Confirmed save cost and change

`GameController.Update` synchronously saves tutorial combat state every three seconds. Action transactions can also save. Hunt Edict disclosure checks in `GameStore.Write` projected unchanged heroes into execution policies and repeatedly copied, normalized and compared skill policies. On a private copy of the tutorial checkpoint, complete `GameStore.Save` calls took approximately 600–625ms. This is real game work capable of stalling presentation, but matching Profiler frames for every recorded pause have not been analyzed.

The owning [HuntEdictProgression.cs](../../Assets/HELLSCRIPT/Runtime/Core/HuntEdictProgression.cs) has two small changes.

- Compare the existing deep-copied committed boundary with the hero's `build`, `edict` and passive slots before creating an execution projection for permission-delta validation. Equal values skip that projection; changed values retain the existing check.
- End item-by-item policy work in `ValidateChange` when the editing document equals the original document.

Recommended-mode, preset and sharing checks remain independent. Hero level and owned-skill validation, normalization, file writing/replacement, boundary capture and save-success notification remain in place. Save cadence, failures, combat ticks and reward transactions are unchanged. No asynchronous save or new cache layer was introduced.

## Isolated measurement

With Play off in macOS Unity 6000.6.0f1 Editor, load the same private tutorial checkpoint copy and time five sequential complete `GameStore.Save` calls using `Stopwatch`. Real account and authentication files were neither copied nor changed. UI save subscribers and running combat are excluded.

| State | Five save times, ms | Gen0 collections across five saves |
| --- | --- | --- |
| Before, `53502b35` | 622.2184, 612.8623, 599.5749, 624.9421, 614.7562 | 65 |
| Initial document equality check, `6dda6d6f` | 185.6419, 188.5237, 241.2693, 247.7571, 304.9346 | 4 |
| Committed-boundary check, `1cd8e027` | 17.8545, 13.1911, 16.4521, 16.4783, 12.7055 | 2 |

The final range is approximately 13–18ms. The intervening integration of the latest `main` item-detail changes did not modify the measured Runtime/Core files. Loading settles offline time and normalizes saves; each set is repeated calls in one Editor process. The result is limited to the isolated save call on this input. It does not establish whole-game frame time, CPU utilization, GC bytes per frame, three matched actual before/after player launches, mobile/web performance, or removal of every recorded freeze.

This task was not building or running a full test suite when the symptom was reported. OS samples retained idle CPU and showed no swap traffic during one second, but other-process contribution remains unresolved. The original recording, checkpoint and measurement data are retained privately.

## Validation and remaining confirmation

After the final code was integrated into `main`, all 34 EditMode `HuntEdictProgressionTests` passed, with zero failures or skips. Coverage includes disclosure timing, official simplified-policy validation, retained legacy access, direct-save restrictions and all three classes' v3 comparison resume, potion application and single reward grant.

The new regression verifies all three classes can save unchanged settings, then rejects an invalid hero level and a locked repeat-setting mutation without changing the last committed file. It also checks restoration and a subsequent normal save. Existing deliberate save-failure tests produced three file-access error logs and passed as expected negative cases. There are no compilation errors. No full game suite or new player build was run.

A replay of the same actual tutorial segment is still needed to confirm whether freezes remain. Any remaining pause needs its main-thread, GC, I/O and external-load evidence narrowed further. This fix does not complete outstanding native tutorial acceptance. Use [Unity tutorial account reset](Tutorial_Account_Reset.en.md) to restart local progress.
