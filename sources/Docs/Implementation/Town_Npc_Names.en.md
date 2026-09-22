# Town NPC names and service labels

작성일: 2026-09-22

[한국어](Town_Npc_Names.md)

## Names and placement

The user's final Korean spellings are authoritative, including revisions to the earlier name candidates.

| Service | Korean name | English name | Placement |
|---|---|---|---|
| Blacksmith | 마르크 쿠스 | Marc Kus | In front of the existing smithy |
| Weapon Merchant | 제이크 보쿤 | Jake Bokun | In front of the existing equipment shop |
| Warehouse | 차도르 사마프 | Chador Samaf | In front of the existing warehouse |
| Rift | 안톤 진다크 | Anton Jindark | Front-left of the central-right portal |
| Rune Master | 인젤 미르 | Injel Mir | In front of the existing rune workshop |
| Resident | 표냐 내르뭰 | Pyonya Nermwen | Left of the campfire, town coordinates (-13, -7) |
| Resident | 쟝 죠린 | Jean Jorin | Left of the well, town coordinates (-11, 7) |
| Resident | 달크 알뷔 | Darc Alvi | Right of the central road, town coordinates (23, 4) |

The rift portal and its interaction destination retain their positions. A new attendant stands beside it; either the attendant or portal can be selected to approach the same service. Residents are stationary figures without service actions or roaming behavior. Existing gambling, training, gem and rune shops remain available.

## Presentation and ownership

- Named services show a bold gold service title at size 22 above a light personal name at size 14.
- Residents show only a personal name at size 16.
- Text retains its dark outline and has no background or input-capturing button. Horizontal positions keep labels inside the safe area.
- Nearby interaction cards also show the personal name below the service title. Existing purchase, sale, smithy, storage, rift and rune workshop actions are reused.
- Korean uses the project's source-string keys; English uses the existing `Localization/en.txt` table. Changing language rebuilds both title and personal-name labels.

## Implementation

- [TownWalk.cs](../../Assets/HELLSCRIPT/Runtime/Core/TownWalk.cs): service names, personal names and positions.
- [WorldView.Town.cs](../../Assets/HELLSCRIPT/Runtime/Presentation/WorldView.Town.cs): live figures and portal selection.
- [GameUI.Plaza.cs](../../Assets/HELLSCRIPT/Runtime/Presentation/GameUI.Plaza.cs): two-line labels and interaction cards.
- [RuntimeTownHudSmoke.cs](../../Assets/HELLSCRIPT/Runtime/Presentation/RuntimeTownHudSmoke.cs): production UI, movement and service-opening validation.

## Validation

Unity 6000.6.0f1 passed all 52 tests in `TownWalkTests` and `LocalizationTests`, with zero failures and zero skipped tests. This was a focused run, not a full project regression. The [Edit Mode result](TownNpcEvidence/editmode.json) is preserved.

A macOS development build succeeded with zero errors. Its isolated-save runtime verified:

- All five named services were reached and opened through production button pointer-click handlers in Korean landscape (1600×900) and English portrait (900×1600). Arrival alone did not open a service.
- All three residents were reached with visible personal names and no service action.
- Live figure positions, Korean/English names, larger titles above smaller personal names, text fit and safe-area bounds.
- The existing HUD acceptance run across six resolutions and 50/100/150% text sizes, totaling 18 combinations, plus safe area, joystick movement/release and the warehouse shortcut.

[Build result](TownNpcEvidence/build.json) · [NPC runtime result](TownNpcEvidence/npc-runtime.txt) · [HUD runtime result](TownNpcEvidence/runtime.txt) · [Layout measurements](TownNpcEvidence/geometry.txt)

The tested player included pre-existing changes in the active checkout. Input was synthesized through EventSystem on macOS; physical iOS/Android touch was not tested. Figures reuse the existing simple village meshes; this change does not produce the detailed character appearances discussed earlier.

## Runtime captures

![Blacksmith and Marc Kus](TownNpcEvidence/npc-Blacksmith-ko.png)

![Rift attendant and town residents](TownNpcEvidence/npc-RiftKeeper-ko.png)

![Weapon merchant in English portrait](TownNpcEvidence/npc-Merchant-en.png)
