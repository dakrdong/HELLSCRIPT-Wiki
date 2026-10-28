# Skill window flow — equipped-skill row, close, old info popup removed (2026-10-24)

> **2026-10-28 A2 correction:** References to the large edict window, ≡ menu and their dedicated components or smokes are historical. Use the [current entry points](../../Assets/HELLSCRIPT/Runtime/Presentation/GameUI.HuntEdict.cs) and [replacement screens and source owners](Hunt_Edict_Window_Removal_20261028.en.md). Earlier captures and test counts describe the earlier implementation.

[한국어](Skill_Window_Flow_20261024.md). One record for user-test reports 8, 12, 13 (Part A, main `a045c626`) and item 17 (Part B, the [per-skill hunt edict page contract](Skill_Edict_Pages_Contract_20261024.en.md)). This document holds Part A's decisions, rules and hand-over.

## Mock-up (approval baseline)

The design artifact `HELLSCRIPT 스킬 트리 · 캐주얼` (https://claude.ai/artifact/JsCKnJUbKkzfBYyJDR3493) was extended to carry Part A and Part B in one approval round. **No screen code is written before the user approves** (only the user's own words count as approval).

- Equipped-skill row: `Eq-*` boards (pressed + '−' + tree glint, empty slot, just replaced, just swapped, not an active, ultimate locked at Lv.40, landscape/PC, English).
- The 24 existing per-class boards share the component, so they show the row and the entry row too.

## Facts confirmed in code

- In game the skill window opens from exactly one place, `HuntEdictWindow.OpenD4SkillTree`, and its closed callback repaints the hunt edict window — that is why "X goes to the edict window". In rift entry an **empty slot** goes through `GameUI.ShowEdictSkillTree()` (edict window first) and a **filled slot** opens the old picker (`RiftEntryWindow.ShowSkills`).
- `ShowEdictSkillTree()` is also used by the guide tutorials (`GameUI.GuideTutorial.cs`, `GameUI.GuideModifierLesson.cs`). That call stays.
- `SkillTreeD4Window` hides the class tabs when the host passes `store` (`showClassTabs=store==null`). The user's green box is that empty spot. The user confirmed directly (2026-10-24) that the "Warrior / Ranger / Mage" class-switch buttons at the top of the skill window are never shown in the game — the mock-up marks them as mock-only with a dashed outline.

## Rules (same as the mock-up)

| Input | Result |
| --- | --- |
| Tap a slot | The slot becomes pressed, the tree scrolls to that skill and its card glints. Tapping the pressed slot again releases it (default). |
| '−' on the pressed slot | Empties the slot, releases the press. |
| Tap a learned active while pressed | It goes into that slot; if another slot holds it the two slots swap. The press is released after a change. |
| Tap anything else while pressed (passive, modifier, unlearned active, ultimate) | No change. The detail area explains it and a one-line reason is shown. |
| Ultimate slot | Locked below level 40; tapping it shows the unlock condition (Lv.40). |
| Close (X) | Saves all changes once and returns to the **screen it was opened from** (rift entry when opened there). |

Saving is one transaction on close. Combination rules reuse the existing build validation (`ClassSkillTree.Equip` and friends). A failed save keeps the previous state and notifies. It is independent of saving the hunt edict draft (Part B).

## Mock-up vs game geometry (what Unity must match)

The tree/detail panel frame coordinates are **unchanged**. The equipped row sits at `TabsAt` (portrait `(16,112,408,44)`, landscape `(150,10)`, PC `(120,12)`). User request (2026-10-24): the hunt-edict entry (Part B) is not a row but a **44 px icon button in the detail panel header, right of the skill name**, so the detail panel gets no scroll.

## Hunt-edict entry icon

At the user's request (2026-10-24) the detail panel's bottom row is gone and a 44 px icon button sits right of the skill name. The art is a navy/gold medallion made by Codex (built-in image generation; the surface does not report its model, so gpt-image-2 use is unverified), stored at `Docs/Art/SkillEdictIcon20261024/` (512², `manifest.json` has prompt, hashes and post-processing). A 256² copy is in the design artifact. Not production-approved.

## Part A implementation (2026-10-24, branch `claude/skill-window-flow-20261024-8afffa`)

The user approved the mock-up on 2026-10-24; Part A below is implemented, Part B is **not** (memo only).

- **Close (item 8)**: `SkillTreeD4Window.Close()` saves the slot draft once, reports `CloseError` to the opener and calls `closed`. `HuntEdictWindow` reports the error and repaints as before; the rift entry window repaints itself. The old hunt edict window is untouched and the skill window no longer links to it.
- **Rift entry (item 12)**: `RiftEntryWindow.SkillPicker.cs` and `RiftSkillPickerLayoutTests.cs` are deleted. The three slot buttons call `RiftEntryWindow.OpenSkillWindow(slot)`, which opens the real skill window with that slot pressed; the locked ultimate slot (below Lv.40) only toasts "궁극기는 Lv.40에 해금돼요." The slot row refreshes through the existing dirty flag after close (UI_Refresh rules, no flicker).
- **Equipped row (item 13)**: `SkillTreeD4.SlotDraft.cs` (pure rules), `SkillTreeD4EquipLooks.cs` (code-defined looks, the generated looks JSON stays hash-checked), `SkillTreeD4Window.Equipped.cs` (row, '−' badge, slot numbers on cards, glint, smooth scroll, toast). The row only exists when the host is a slot host with a store (game) or `previewEquipped` (mock/evidence); the class tabs are unchanged in the plain design host.
- **Save**: `GameStore.CommitSkillSlots(heroId, opened, desired, catalog, simulation)` is one atomic write through `ClassSkillTree.SetSlots` and the existing build validation; a stale `opened`, another hero, a duplicate or a wrong-kind skill write nothing. `SetSlots` keeps each kept skill's automatic-use flag and order; a newly equipped skill starts automatic at the replaced skill's place.
- **Point commits while open**: a refund that drops a skill's last rank rebases the draft; a slot whose new skill is gone falls back to its saved skill unless the draft moved that skill. Only this window's own point commit rebases, so a layout saved elsewhere still fails the stale check on close (error shown, previous state kept).
- **Chosen defaults** (the open questions below stay open): same-slot tap releases; the press is released after put/swap/remove; swap keeps both automatic-use flags and the action order; a slot opened from an empty rift slot starts pressed (a filled one too).

### Validation

Focused EditMode only, no full run. Run 1 (8 classes: SkillTreeD4SlotDraftTests, SkillTreeD4SlotWindowTests, RiftEntryTests, RiftEntryCasualWindowTests, SkillTreeD4StoreHostTests, SkillTreeD4WindowTests, SkillTreeD4InputTests, LocalizationTests): 131 tests, 124 passed, 7 failed — all seven were the new window tests hitting `Destroy` in edit mode; `Close()` now uses `DestroyImmediate` when not playing. Run 2 (the two new classes): 24/24 passed. The other six classes passed in run 1 and their inputs did not change afterwards (only `SkillTreeD4Window.cs` `Close`, `SlotDraft.Rebase`, `Equipped` badge condition and scroll cancel changed, covered by the new tests). Not run: the runtime smokes `RuntimeRiftEntrySmoke*` (edited to the new `EquipThroughWindow` flow, compile-checked only), PlayMode, real device, full EditMode.

## Decisions needed from the user

1. ~~Mock-up approval~~ (done 2026-10-24).
2. Whether tapping the same slot again releases the press (default) and whether the press is released after replace/swap/remove (default) or kept.
3. On swap, both skills keep their automatic-use on/off state and execution order (default).
