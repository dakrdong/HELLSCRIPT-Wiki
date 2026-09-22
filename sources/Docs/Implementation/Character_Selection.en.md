# HELLSCRIPT character selection

Created: 2026-09-20

[한국어](Character_Selection.md)

## Login to gameplay

Successful mock login or guest entry opens a dedicated three-character stage. The Warrior checks his sword, the Mage studies magic in her hand, and the Ranger checks her bowstring. Clicking a figure or its class button brings that character to the center foreground while the others recede and dim. **Start** appears after the combat-ready pose settles. It commits the selected saved hero and opens the existing town gameplay screen.

Class and level come from the real save. Previewing does not save a character or change equipment; returning to the title preserves the prior saved selection. Authentication and servers remain mock interactions. An existing rift restricts selection to its owner. Character switching from the title settings also respects the entry flow.

A dedicated [sanctuary courtyard](../../Assets/HELLSCRIPT/Resources/Art/CharacterSelection/SelectionCourtyard.png) grounds the figures and their contact shadows. The presentation uses 2D painted frames, movement, breathing, and a ground sigil. It does not replace gameplay's 3D models or animations. Each class has six frames of one continuous preparation sequence, with separate idle and combat-ready playback ranges. Motion pause freezes idle animation and the background while keeping selection usable.

## Layout and input

The screen retains the title's iPhone 17 Pro Max portrait/landscape ratios and PC 16:9, 16:10, and ultrawide support. It uses `UiSafeArea`. Rotation, window resizing, and language changes preserve the previewed character. Compact landscape places details beside Start; portrait stacks them below the central character.

Transparent figure margins reject pointer hits using small masks baked from the native PNG alpha. Textures remain GPU-only at runtime. Separate class buttons are also available. Left/right arrow keys select a character, Escape returns to the title, and Enter starts when no other selectable control has focus.

## Ownership and assets

- [CharacterSelectionState.cs](../../Assets/HELLSCRIPT/Runtime/Core/CharacterSelectionState.cs) owns the preview and rift-owner restrictions.
- [GameUI.CharacterSelection.cs](../../Assets/HELLSCRIPT/Runtime/Presentation/GameUI.CharacterSelection.cs) creates the page. [CharacterSelectionView.cs](../../Assets/HELLSCRIPT/Runtime/Presentation/CharacterSelectionView.cs) handles UI and input; [CharacterSelectionStage.cs](../../Assets/HELLSCRIPT/Runtime/Presentation/CharacterSelectionStage.cs) handles movement and pose timing.
- [GameController.CharacterSelection.cs](../../Assets/HELLSCRIPT/Runtime/Presentation/GameController.CharacterSelection.cs) commits only at Start, restores the old selection on save failure, and uses the existing `GameController.EnterPlaza(true)` path.
- [CharacterFigure.cs](../../Assets/HELLSCRIPT/Runtime/Presentation/CharacterFigure.cs) displays frames and performs alpha-mask hit testing.
- Original atlases: [Warrior](../../Assets/HELLSCRIPT/Resources/Art/CharacterSelection/Warrior.png), [Mage](../../Assets/HELLSCRIPT/Resources/Art/CharacterSelection/Mage.png), [Ranger](../../Assets/HELLSCRIPT/Resources/Art/CharacterSelection/Ranger.png).
- [Prompts](Character_Selection_Prompts.txt), [provenance](CharacterSelectionEvidence/provenance.json), [alpha/scale checks](CharacterSelectionEvidence/art-qc.json), and the [inspection tool](../../tools/inspect_character_atlases.py) are preserved.

Built-in `image_gen` created the art using the existing warrior reference's style. Each original is a 1024×1536 RGBA PNG copied without pixel edits. No chroma key or background removal was used. Rendering applies one shared scale with measured foot anchors instead of independently fitting each frame. The Ranger's generated grid differs from the requested grid; measured UV rectangles follow the actual figures. Full silhouettes, including sword tips that cross nominal cell boundaries, are preserved.

C2PA identifies `ChatGPT / gpt-image`, without an exact model version. Use of `gpt-image-2` cannot be verified. The three atlases and courtyard backdrop are registered as replaceable development art.

## Validation

The focused Edit Mode run passed **58/58 tests**: 10 character-selection state/ownership/atlas cases, 15 title cases, and 33 localization cases. [Results](CharacterSelectionEvidence/editmode.json) are preserved.

The final macOS development player completed `HELLSCRIPT_CHARACTER_SELECTION_OK checks=11` and `HELLSCRIPT_TITLE_RUNTIME_OK checks=10`, both with exit code 0. Checks covered direct silhouette clicks on all three classes, distinct idle-frame advancement, central movement, combat poses, delayed Start, rapid reselection, no save mutation during preview, login/guest/logout/back, rift ownership, real Mage town entry, and stage disposal. See [selection results](CharacterSelectionEvidence/validation.txt) and [title regression](TitleScreenEvidence/validation.txt).

Layout checks covered 440×956 and 956×440 phone-ratio windows plus 1280×720, 1440×900, and 1720×720 PC windows. Phone insets were simulated on Mac. Controls stayed inside the safe area without overlap or clipped text. Korean/English and rotation preserved the preview; motion pause froze idle clocks and poses.

A separate native Mac player was exercised with real mouse/keyboard input: fictional login, Tab and Return, a Warrior silhouette click, Right-arrow selection of Ranger, Start click, and visible arrival in town. The [native-input record](CharacterSelectionEvidence/native-input.json) distinguishes this from scripted handlers. Physical iPhone touch, keyboard, performance and Windows execution remain unverified.

The [final build](CharacterSelectionEvidence/build.json) succeeded with zero errors. It reported 384 project-wide warnings; the character-selection Console filter returned no warnings or errors. This was focused validation, not full-game regression or a mobile performance measurement.

![Character selection](CharacterSelectionEvidence/selected-mage-pc-ko.png)

[Lineup](CharacterSelectionEvidence/lineup-pc-ko.png) · [Warrior](CharacterSelectionEvidence/selected-warrior-pc-ko.png) · [Ranger](CharacterSelectionEvidence/selected-ranger-pc-ko.png) · [Phone portrait](CharacterSelectionEvidence/selected-iphone-portrait-ko.png) · [Phone landscape](CharacterSelectionEvidence/selected-iphone-landscape-ko.png) · [English](CharacterSelectionEvidence/selected-iphone-portrait-en.png) · [Gameplay entry](CharacterSelectionEvidence/plaza-entry-mage-en.png)

Public wiki publication is performed by the merger after this branch reaches `main`.
