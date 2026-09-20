# Town HUD sizing, opacity and visibility

Date: 2026-09-20

[한국어](Town_Hud_Responsive.md)

## Behavior

The six requests in the attached screenshot are implemented in the existing town UI and global HUD.

| Area | Current behavior |
|---|---|
| Warehouse shortcut | Uses the supplied chest PNG and opens the existing account storage. Town and battle share the icon. |
| Movement pad | Its diameter is 20% of the usable screen's shorter side. The circle and knob keep their proportions, above the left HUD. |
| Pad opacity | Starts at 30% opacity, becomes fully opaque while held, and returns to 30% on release. Another pointer cannot take ownership. Resizing, disabling, focus loss and opening settings release input. |
| Potions | Stay above the highest skill row in portrait and landscape, including wrapped skill layouts. |
| Town guide | Hides the class/level/crossing-time text, destination-guide button and menu button. |
| NPC names | Shows names with a dark outline, without a background, speech tail or appended action text. The actual interaction card remains available. |
| Header | Removes the instruction, background and divider. Title size changes from 30 to 18; height changes from 80 to 44. The gear is 20 units inside a 44×44 hit area. |

## Ownership and image provenance

Existing `GameUI.Plaza` owns town presentation, `TownJoystick` owns pointer input and opacity, and `GlobalHudLayout` owns potion placement. Pixel geometry is converted to page-canvas units once. Save formats, packages, scenes and prefabs are unchanged.

`menu-storage.png` is a byte-for-byte copy of the user's 1254×1254 RGBA attachment. No generation, resizing or background removal was performed. Its 611,509 fully transparent pixels and source hash are recorded in the [provenance file](TownHudEvidence/provenance.json). The existing sprite importer preserves alpha and uses uncompressed textures.

![Original warehouse shortcut](../../Assets/HELLSCRIPT/Resources/Art/GlobalHUD/menu-storage.png)

## Validation

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
