# HELLSCRIPT living title screen

작성일: 2026-09-20

[한국어](Title_Screen.md)

## Presentation and entry

An original ruined sanctuary and crimson fissure form the backdrop. Moving mist, cloud shadows, breathing fissure light and sparse embers make it feel alive. The logo sits above the central lower login, server and character-stage entry controls. The artwork fills the display while controls remain inside the safe area. The motion button freezes every background animation.

Login and servers are explicitly mocked at the user's request. Empty login fields display guidance; fictional credentials establish a temporary session. The password is masked and input fields are cleared when the dialog closes. There is no network authentication, stored password or server connection. Guest entry and sign-out are supported. Two preview servers are selectable; the maintenance server is disabled. All servers share the existing local account and characters.

Successful login or guest entry opens the separate [character selection stage](Character_Selection.en.md). Players preview the Warrior, Mage, and Ranger, select one, and press Start to call the existing `GameController.EnterPlaza(true)`. The former title character-list dialog is replaced by this dedicated page. Town movement begins after entry.

## Display targets

[Apple's official specification](https://support.apple.com/en-us/125091) confirms a 1320×2868 display for iPhone 17 Pro Max. The title responds to the available dimensions without forcing a hardware model or resolution.

| Target | Validation window | Layout |
|---|---|---|
| iPhone portrait | 440×956, one third of native resolution | Fixed central lower column. |
| iPhone landscape | 956×440, one third of native resolution | Compact login, server and character-stage entry column. |
| PC 16:9 | 1280×720 | Bounded central controls. |
| PC 16:10 | 1440×900 | Extra height extends background and spacing. |
| PC ultrawide | 1720×720, half of 3440×1440 | Full background with centered controls. |

Mobile controls use the existing `Screen.safeArea` path. Validation insets simulate display cutouts and the home indicator; they are not measured iPhone hardware values. Rotating a login dialog preserves its input controls and text. Only dialog content scrolls on short windows, keeping Close fixed.

## Ownership and provenance

[GameUI.Title.cs](../../Assets/HELLSCRIPT/Runtime/Presentation/GameUI.Title.cs) hosts the title. [TitleScreenView](../../Assets/HELLSCRIPT/Runtime/Presentation/TitleScreenView.cs) and its [dialogs](../../Assets/HELLSCRIPT/Runtime/Presentation/TitleScreenView.Dialogs.cs) own presentation. [TitleSession](../../Assets/HELLSCRIPT/Runtime/Core/TitleSession.cs) holds mock session state in memory, separate from game saves. [TitleAtmosphere](../../Assets/HELLSCRIPT/Runtime/Presentation/TitleAtmosphere.cs) and its [shader](../../Assets/HELLSCRIPT/Resources/Art/Title/TitleAtmosphere.shader) composite animation in one background draw and release the dynamic material when the title closes.

The [1672×941 background PNG](../../Assets/HELLSCRIPT/Resources/Art/Title/TitleSanctuary.png) is an unchanged copy from built-in `image_gen`. The tool returned only `image_url` and `output_hint`; its C2PA creation action identifies `softwareAgent.name=ChatGPT`, `version=gpt-image`. No exact model version can be verified, so this is not attributed specifically to `gpt-image-2` or classified as approved release artwork. The [exact prompt](Title_Screen_Prompt.txt) and [provenance](TitleScreenEvidence/provenance.json) are retained.

The logo and frames use uGUI text and geometry. OS font files are neither copied nor redistributed. New text uses the existing Korean source keys with English translations.

## Validation

After adding the character selection stage, the focused tests passed 58/58 and the title runtime checks passed again on the final build. See the [extended validation record](Character_Selection.en.md).

The initial title Edit Mode run passed 48/48 tests: 15 title session, entry gating, aspect preservation and resource checks plus 33 existing localization checks. The [initial test result](TitleScreenEvidence/editmode.json) is retained.

The macOS development player's `RuntimeTitleSmoke` exited with code 0 and `HELLSCRIPT_TITLE_RUNTIME_OK checks=10`. It checked three PC aspect ratios, both iPhone orientations, Korean/English text, safe areas, overlapping controls and clipped labels. Empty login, password masking/clearing, rotation preserving input, server selection/maintenance, real character selection, logout/guest and plaza entry all passed. The [runtime report](TitleScreenEvidence/validation.txt) is retained.

The final title runtime run measured a mean RGB difference of 1.7140 across a 1.4-second interval. Pausing motion froze both the clock and sampled rendered pixels. This is not a frame-rate or mobile performance measurement.

A separate native Mac window was verified with actual pointer and keyboard input: opening login, entering fictional text, Tab navigation, Return login, server selection and logout. See [native input scope](TitleScreenEvidence/native-input.json). Physical iPhone touch, keyboard, rotation and performance, and Windows execution, remain unverified. The initial title build succeeded with zero errors and 39 project shader/import warnings. The final build including character selection succeeded with zero errors and 384 project-wide warnings; see its [build record](CharacterSelectionEvidence/build.json).

![PC 16:9 title](TitleScreenEvidence/pc-16x9-ko.png)

[iPhone portrait](TitleScreenEvidence/iphone-portrait-en.png) · [iPhone landscape](TitleScreenEvidence/iphone-landscape-en.png) · [PC 16:10](TitleScreenEvidence/pc-16x10-ko.png) · [PC ultrawide](TitleScreenEvidence/pc-ultrawide-ko.png) · [Login dialog](TitleScreenEvidence/iphone-login-ko.png) · [English server dialog](TitleScreenEvidence/iphone-servers-en.png)

The person merging this branch into `main` must rebuild, validate and publish the public wiki. This work branch does not overwrite the public deployment.
