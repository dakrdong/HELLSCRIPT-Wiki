# HELLSCRIPT Editor Null-Placeholder Fixes

Date: 2026-09-15
작성일: 2026-09-15

## Background

In the Unity editor, a `GetComponent`-family call for a missing component does not return a real `null`. It returns a placeholder object carrying an error message. Only Unity's overridden `==` and `!=` treat that object as null; C#'s `??`, `?.` and `is null` see a live reference. So `GetComponent<T>() ?? AddComponent<T>()` never adds in the editor, and the next member access throws `MissingComponentException`. In player builds `GetComponent` returns a real `null`, so the same code works. This is why screens whose development-build smokes pass can still throw only in editor Play Mode.

The first instance was the missing `CanvasRenderer` on custom graphics, recorded in the [play-screen content dock record](Play_Content_Dock.en.md) under "Pre-existing problem exposed in the editor". This document covers the second instance and the regression guard.

## Symptom

Pressing the settings button in the upper right of the play screen raised the following and the settings panel did not open.

```
MissingComponentException: There is no 'CanvasGroup' attached to the "HELLSCRIPT UI" game object, but a script is trying to access it.
  Hellscript.GameUI.ShowCommonPanel (System.Boolean help) (at Assets/HELLSCRIPT/Runtime/Presentation/GameUI.ScreenSettings.cs:33)
```

## Cause and fix

| Location | Before | Fix |
|---|---|---|
| `GameUI.ScreenSettings.cs:33`, opening the settings panel | For every canvas it obtained the input-gate group with `GetComponent<CanvasGroup>() ?? AddComponent<CanvasGroup>()` and read `interactable` at once. In the editor the placeholder came back and the first canvas threw. | `TryGetComponent(out CanvasGroup group)`, then `AddComponent` when it returns false. |
| `GameUI.Presets.cs:73`, preset name input | Filled the input module's `inputOverride` with `GetComponent<HellscriptTextInput>() ?? AddComponent<HellscriptTextInput>()`. In the editor the placeholder was assigned, so the input override never took effect. No exception. | Same `TryGetComponent` then `AddComponent`. |

`TryGetComponent` returns false and a real `null` for a missing component, so the editor and the player take the same path.

## Regression guard

The Edit Mode test `EditorNullPatternTests` scans the C# sources under `Assets/HELLSCRIPT/Runtime` and `Assets/HELLSCRIPT/Editor` for a `GetComponent…(…)` call followed by `??` or `?.` and fails on any hit. Runtime smokes, whose file names contain `Smoke`, are excluded because they run only in the player. `GraphicComponentTests` checks that every `Graphic` subclass brings its own `CanvasRenderer`.

## Verification

No batch run was made on the original project because the owner's editor held it open. Verification used a scratch copy of the project; results are in the [evidence folder](EditorNullObjectEvidence/).

| Item | Result |
|---|---|
| Owner's editor log before the fix | The `CanvasGroup` exception was logged on every press of the settings button. [Sample, 4 occurrences](EditorNullObjectEvidence/editor-exception-sample.txt) |
| `EditorNullPatternTests` on the pre-fix copy | [Failed](EditorNullObjectEvidence/nullpattern-test-before.xml), listing `GameUI.Presets.cs:73` and `GameUI.ScreenSettings.cs:33`. |
| Edit Mode tests on the fixed copy | `EditorNullPatternTests`, `GraphicComponentTests` and the settings revision, preset storage and display settings tests: [64 of 64 passed](EditorNullObjectEvidence/nullpattern-test-after.xml) |
| Editor reproduction on the fixed copy | Entered Play Mode with the owner's Enter Play Mode settings, opened and closed the settings panel twice on the title and once in the town without exceptions, with `CommonPanelOpen` true each time. Opening the preset name dialog gave the input module a real `HellscriptTextInput` override. [Record](EditorNullObjectEvidence/editor-repro-settings-after.txt) |

The pre-fix settings-panel reproduction was not rerun on the copy; the owner's log serves as that evidence. The open editor recompiles on file change, so after compilation the settings button should open the panel in Play Mode. Nothing was checked on a physical device.
