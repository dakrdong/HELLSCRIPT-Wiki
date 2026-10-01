# A simple Hunt Edict Overview and skill actions

Updated: 2026-10-01 · [한국어](Hunt_Edict_Simple_Skill_Actions.md)

## Changed screens

- The Overview body contains only Aggressive, Balanced and Careful cards and their descriptions. It does not list equipment, use policies, combat judgments, automation or last-hunt advice. The default-policy On/Off switch stays above it.
- Equipped actives and ultimates use one thin seal outline without slot-number badges. Actual rank/max rank stays below the seal. Empty equipment sockets have no numerals either.
- A tree active or ultimate opens an adjacent bubble: + if unequipped; − and an edit pictogram if equipped. The edit pictogram is a rolled parchment with a writing brush, using the existing vector renderer and shared colors; no raster asset or new canvas is added.
- Passives always apply by their allocated rank and have no equipment action. An unlearned active has a disabled + and retains unlock information in the inspector.
- Another equipped skill on the detailed preset page navigates directly to its policy without an equipment popup.

## State and input

The existing `HuntEdictWindow` owns its draft. Opening, browsing, language and orientation changes never save. Equipment/removal and style choices edit the draft and retain Save/Revert. Policy activation and immediate custom-option transactions are unchanged. Combat examples reuse the existing isolated runner.

The bubble resolves the current pressed node or socket. When the first portrait selection reveals the inspector, it keeps the selected node inside the tree viewport. Rotation and language changes resolve the rebuilt node again. A transparent outside-click layer closes the bubble without executing the underlying button. Back dismisses the bubble first.

Default settings On retains the existing dimmed/disabled Overview cards, other categories and Save/Revert. Switching Off restores input and the existing draft.

## Verification

The final validation passed the shared UI contract and 83 focused Edit Mode tests (0 failed, 0 skipped). The isolated Unity 6000.6.0f1 macOS development build completed with 0 errors.

One combined native acceptance batch covered default text size at portrait 440×956, landscape 956×440, PC 16:9/16:10/21:9 and Korean/English: ten profiles. It checked the three complete style cards, numeral-free equipment outlines, tree/ultimate bubbles, scroll-and-brush pictogram, direct policy-rail navigation and unchanged draft/saved ownership while browsing. It also passed +/empty-slot/full-slot replacement, active/ultimate removal, Revert/Save, back/outside dismissal, rotation/language/safe-area reflow and default settings On/Off. A fresh Player process restored the removed slot and Careful style.

[Native results](HuntEdictSimpleActionsEvidence/menu-runtime.txt) · [Fresh-process result](HuntEdictSimpleActionsEvidence/menu-restart.txt) · [Scope and hashes](HuntEdictSimpleActionsEvidence/validation.json)

![PC skill bubble](HuntEdictSimpleActionsEvidence/tree-bubble-1600x900-ko.png)
![Portrait Overview](HuntEdictSimpleActionsEvidence/overview-440x956-ko.png)
![Landscape English Overview](HuntEdictSimpleActionsEvidence/overview-956x440-en.png)

Input evidence uses uGUI raycasts and synthetic pointer events in the macOS native Player, not physical mobile devices. No text-size matrix or full combat-example gallery was run. Existing URP postprocessing shader warnings were observed without a C# or interaction failure in this UI. Successful coverage is reused after merges/document updates when the relevant code and configuration are unchanged.
