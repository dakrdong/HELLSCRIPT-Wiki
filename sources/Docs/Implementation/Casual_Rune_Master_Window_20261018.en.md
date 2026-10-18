# Rune Master window casual skin — 2026-10-18

[한국어](Casual_Rune_Master_Window_20261018.md)

Basis: the design canvas "HELLSCRIPT 룬 장인 · 캐주얼" (AefHsWjXq1UDFzdsNzmdSW), build spec section k (game mapping) and f (component recipes). Started from `origin/main` `3d3e0f4f`. The reshape and ascension rules and transactions (`RuneReshape`, `RuneAscension`, `GameStore.ReshapeRunes/AscendRunes/ClaimRuneAscensions`), drag resolution, animation timings, the `rune-master-*` names and the `PlayerPrefs` key are unchanged.

## What changed

- `RuneMasterWindow.Controls.cs` owns every surface, label and button. `Panel(…, name)` now uses the casual `StorageSurface.SetSkin`: plate `plate` (navy 3px, radius 18/24, drop shadow), cards `base` (white, card edge), selected slot `active` (navy 3px), storage cell `slot`, empty `empty`, scrollbar `track`/`handle`, backdrop `veil` (navy α .45). Text uses `UiSkin.LabelFont` (Jua) and `BodyFont`; colours come from `RuneBoardSkin.Shared` (= `UiSkin.Casual`).
- The window is a rounded plate floating 8–12 units inside the safe area. The header carries the brand, a rune-board gem with the golden title `룬 마스터` (the `InventoryWindow` title recipe), a round coral close button and a 2px divider below.
- Every `UiButton` uses `RuneBoardSkin.Button`/`ApplyChrome`. Fuse and Fuse all are green with a white outlined label and grey with a muted label when disabled (`Enable`). Tabs are yellow when selected with a tail diamond; grade buttons are yellow when selected; Skip animation is a yellow chip when on.
- Material slots get a navy ring with a G chip when filled and a pale-yellow target while dragging (`SlotLook`). The plus is a yellow disc, the result a green-edged card, errors a coral notice box and completion a yellow guide box (`NoticeBox`). In the storage grid the selection check is a yellow disc, NEW a green pill and protected runes a lock icon.

## Differences from the mockup (decisions needed)

- Rune art keeps the game's `RuneBoardGraphic` icon plus the type emblem; the mockup laid a casual gem tile per cell. Whether to unify it with the rune board window is open.
- The empty material slot has a solid ring instead of a dashed one (`StorageSurface` dashes are 1px only and the shared part was not changed).
- The surface flash of the ascension reveal is not visible because the card face is the `UiButton` face. The flip itself (scaleX) is unchanged.
- Dimensions keep the game's units and only adopt the mockup's proportions (plate gap 8/12, header 36/44, radii); the mockup's 1600×900 board pixels were not copied.
- No new text. The missing localisation keys the mockup flagged ("✓ 연출 제거", "NEW") are unchanged.

## Verification

- 60 focused EditMode tests pass: `RuneMasterCasualSkinTests` (8 new), `RuneAscensionTests`, `RuneEconomyTests`, `UiButtonTests`. The new tests check plate, card and tab geometry in landscape and portrait on both tabs, casual fonts, colours and chrome on every text and button, and the pair-selected, error and upgrade-selection states.
- Static UI contract (15) and refresh guard (14 bindings) pass.
- The real window was rendered to PNG in the Editor (PC, landscape, portrait: reshape, pair selected, error, upgrade empty and selected) and checked by eye (`Assets/HELLSCRIPT/Editor/RuneMasterCasualEvidence.cs`, 640×480).
- Not verified: the development-player `RuntimeRuneMasterSmoke` (drag, fusion animation, transactions) and a macOS build, physical devices and touch, long English strings. The fusion and reveal animations were checked only as still frames. The full EditMode suite was not run.
