# Proportional bottom HUD and town labels, with English NPC names

Verified: 2026-09-22

[한국어](Hud_Text_Scale.md) · [Original NPC naming record](Town_Npc_Names.en.md)

## Changed behavior

Shrinking the window briefly reduced text before the next HUD refresh enlarged it again. An unscaled minimum font size caused the rebound. Font size and row height now use the same HUD scale. Whole-pixel font rounding may differ by at most half a pixel from the proportional value.

Town service titles and personal names scale as a complete label, including line spacing, outline and visibility bounds. The NPC interaction card uses the same scale and stays above the bottom controls. Labels avoid the actual height of the top title area. Existing interface preferences and the minimum HUD scale of 0.4 remain in effect.

| Window | HUD scale | Bottom caption | After repeated refreshes |
|---|---:|---:|---|
| 1600×900 | 1.0 | 16px | Remains 16px. |
| 800×450 | 0.5 | 8px | Remains 8px. |
| 640×360 | 0.4 | 6px | Remains 6px. |
| 900×1600 | 1.0 | 16px | Remains 16px. |
| 450×800 | 0.5 | 8px | Remains 8px. |
| 360×640 | 0.4 | 6px | Remains 6px. |

At half scale, the 208-unit label becomes 104 pixels wide and the 250-unit interaction card becomes 125 pixels wide. Verification covers Korean and English, landscape and portrait, shrink/restore transitions and interface preferences of 50% and 150%.

## NPC names in English

The existing registered names are preserved. The player visited every named NPC in English mode and verified the rendered name within the screen safe area.

| Korean | English |
|---|---|
| 마르크 쿠스 | Marc Kus |
| 제이크 보쿤 | Jake Bokun |
| 차도르 사마프 | Chador Samaf |
| 안톤 진다크 | Anton Jindark |
| 인젤 미르 | Injel Mir |
| 표냐 내르뭰 | Pyonya Nermwen |
| 쟝 죠린 | Jean Jorin |
| 달크 알뷔 | Darc Alvi |

## Screens and verification

[Korean 800×450: proportional HUD, labels and interaction card](HudTextScaleEvidence/ko-800x450.png)

[English 450×800: English NPC names and a proportional interaction card](HudTextScaleEvidence/en-450x800.png)

- Edit Mode: **63 passed, 0 failed, 0 skipped** across 11 HUD, 33 localization and 19 town movement tests. See the [summary](HudTextScaleEvidence/editmode.json) and [final XML](HudTextScaleEvidence/editmode-final.xml).
- The macOS development build succeeded with zero build errors. See the [build record](HudTextScaleEvidence/build.json).
- The real player checked 30 UI samples, including unchanged font sizes after 1.5 seconds of repeated refreshes. It then visited all eight named NPCs and checked their visible English names. See the [runtime result](HudTextScaleEvidence/runtime.txt) and [geometry and visit log](HudTextScaleEvidence/geometry.txt).
- Final verification ran in a separate filesystem snapshot to preserve the original Editor's active play session. That snapshot includes unrelated working changes; the changed task files match the [recorded hashes](HudTextScaleEvidence/source-snapshot.json). This is neither a full regression run in the original Editor nor physical mobile verification.

## Source changes

- [GlobalHudLayout.cs](../../Assets/HELLSCRIPT/Runtime/Presentation/GlobalHudLayout.cs) calculates proportional fonts and rows.
- [GameUI.Plaza.cs](../../Assets/HELLSCRIPT/Runtime/Presentation/GameUI.Plaza.cs) scales town labels and interaction cards and checks their screen bounds.
- [GlobalHudTests.cs](../../Assets/HELLSCRIPT/Tests/Editor/GlobalHudTests.cs) checks half-size proportions and text row fit.
- [RuntimeHudScaleSmoke.cs](../../Assets/HELLSCRIPT/Runtime/Presentation/RuntimeHudScaleSmoke.cs) verifies actual HUD refreshes, resizing and visits to English NPCs. It runs only with its dedicated development-build argument and requires an isolated save path.
