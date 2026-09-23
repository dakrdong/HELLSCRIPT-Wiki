# Town HUD sizing, opacity and visibility

Created: 2026-09-20 · Updated: 2026-09-23

[한국어](Town_Hud_Responsive.md)

## Behavior

The existing town UI and global HUD implement the September 20 requests and the September 22 follow-up.

| Area | Current behavior |
|---|---|
| Warehouse shortcut | Uses the supplied chest PNG and opens the existing account storage. Town and battle share the icon. |
| Movement pad | Its diameter is 20% of the usable screen's shorter side. It stays at the lower center in portrait and above the left HUD in landscape, clear of skills and potions. |
| Pad opacity | Starts at 30% opacity, becomes fully opaque while held, and returns to 30% on release. Another pointer cannot take ownership. Resizing, disabling, focus loss and opening settings release input. |
| Potions | Stay above the highest skill row in both orientations. A translucent dark olive tray with brass decoration and dividers groups the three bottles. |
| Town guide | Hides the class/level/crossing-time text, destination-guide button and menu button. |
| NPC names | Shows names with a dark outline, without a background, speech tail or appended action text. The actual interaction card remains available. |
| Header | Instructions and the full-width divider remain hidden. The title has a compact translucent backing, a maximum size of 18 and a one-line fit for longer English text on narrow screens. |
| Top shortcuts | Enlarged using the skill icons' screen scale. Settings, shortcuts and the folding button have no added rectangular border. Original circular artwork is preserved. The column fits the available height above the potion tray. |
| Bottom baseline | Portrait active skills remain 32 logical units above the safe-area bottom, replacing the offsets of 148/340. Landscape reserves only the spacing needed between skill captions and the XP line. Enlarged portrait vitals stack above the class seal instead of pushing actions upward. |

## Ownership and image provenance

Existing `GameUI.Plaza` owns town presentation, `TownJoystick` owns pointer input and opacity, and `GlobalHudLayout` owns potion placement. Pixel geometry is converted to page-canvas units once. Save formats, packages, scenes and prefabs are unchanged.

`menu-storage.png` is a byte-for-byte copy of the user's 1254×1254 RGBA attachment. No generation, resizing or background removal was performed. Its 611,509 fully transparent pixels and source hash are recorded in the [provenance file](TownHudEvidence/provenance.json). The existing sprite importer preserves alpha and uses uncompressed textures.

![Original warehouse shortcut](../../Assets/HELLSCRIPT/Resources/Art/GlobalHUD/menu-storage.png)

## September 23 validation

The four focused Unity Edit Mode suites (`GlobalHudTests`, `GlobalHudResourceTests`, `TownWalkTests`, `InterfaceScaleTests`) passed **63 tests, with 0 failed or skipped**. This is not a full project regression run. The shared UI contract check and its 9 validator tests also passed. The macOS development build completed with zero build errors.

The player passed **48 combinations** of 1600×900, 1600×1000, 2100×900, 900×1600, 956×440, 440×956, 640×360 and 360×640, with 50/100/150% reading sizes in Korean and English. Additional checks cover asymmetric landscape safe-area insets and portrait top/bottom insets of 44/34.

Checks cover real movement and stopping, pointer ownership and opacity, centered portrait placement, bottom-aligned actions, potion tray, one-line title fit and border removal. Actual UI raycasts verify potion/shortcut/settings access. Folding hides shortcut input; unfolding restores it. Opening real storage, releasing movement for settings and existing named NPC services also passed. The blacksmith probe now checks the current `BlacksmithOpen` window state.

Input uses synthetic EventSystem pointer events in a macOS player. Physical iOS/Android touch input was not tested. Existing URP post-processing shader warnings remain; this change does not modify the render pipeline.

[Test results](TownHudEvidence/2026-09-23/editmode.xml) · [Runtime result](TownHudEvidence/2026-09-23/runtime.txt) · [Measured geometry](TownHudEvidence/2026-09-23/geometry.txt) · [Build result](TownHudEvidence/2026-09-23/build.txt) · [Scope and source hashes](TownHudEvidence/2026-09-23/validation.json)

![Portrait bottom placement](TownHudEvidence/2026-09-23/portrait-ko.png)

![English at 150 percent](TownHudEvidence/2026-09-23/portrait-en-large.png)

[Portrait 440×956](TownHudEvidence/2026-09-23/mobile-portrait-ko.png) · [Landscape 956×440](TownHudEvidence/2026-09-23/mobile-landscape-ko.png) · [PC 16:9](TownHudEvidence/2026-09-23/pc-16-9.png) · [PC 16:10](TownHudEvidence/2026-09-23/pc-16-10.png) · [PC 21:9](TownHudEvidence/2026-09-23/pc-21-9.png) · [Portrait safe area](TownHudEvidence/2026-09-23/portrait-safe-area.png)

## September 20 validation record

All 56 focused Edit Mode tests passed in Unity 6000.6.0f1: `GlobalHudTests`, `GlobalHudResourceTests`, `TownWalkTests` and `InterfaceScaleTests`. This is not a full project regression run. See the [test results](TownHudEvidence/editmode-results.xml).

The macOS development build succeeded with zero build errors. The runtime passed 18 combinations: 1600×900, 900×1600, 844×390, 390×844, 640×360 and 360×640 at 50%, 100% and 150% interface scaling. Safe areas, Korean/English, actual character motion and stopping, held/released opacity, second-pointer ownership, opening storage and opening settings passed. NPC name visibility now follows the usable screen and compact header.

[Runtime result](TownHudEvidence/runtime.txt) · [Measured geometry](TownHudEvidence/geometry.txt) · [Validation summary](TownHudEvidence/validation.json) · [Build result](TownHudEvidence/build.txt)

Input was simulated with EventSystem pointer events in the macOS player. Physical iOS/Android touch input was not tested. The player emitted some URP post-processing shader-stripping warnings; the render pipeline was not changed in this UI task.

## Runtime screenshots

![Landscape and idle movement pad](TownHudEvidence/1600x900-100.png)

![Portrait potions above skills](TownHudEvidence/900x1600-100.png)

![Fully opaque movement pad while held](TownHudEvidence/landscape-held.png)

![Warehouse shortcut using the attachment](TownHudEvidence/storage-shortcut.png)

![NPC names on a small landscape screen](TownHudEvidence/844x390-100.png)
