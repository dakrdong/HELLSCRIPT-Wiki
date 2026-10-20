# HELLSCRIPT living title screen

작성일: 2026-09-20

[한국어](Title_Screen.md)

## Presentation and entry

Layered paintings show the heroes from behind on high cliffs, looking at a distant glowing rift (see the vista layers below). The logo sits above the central lower login, server and character-stage entry controls. The artwork fills the display while controls remain inside the safe area. The motion button freezes every background animation.

Login and servers are explicitly mocked at the user's request. Empty login fields display guidance; fictional credentials establish a temporary session. The password is masked and input fields are cleared when the dialog closes. There is no network authentication, stored password or server connection. Guest entry and sign-out are supported. Two preview servers are selectable; the maintenance server is disabled. All servers share the existing local account and characters.

Successful login or guest entry opens the separate [character selection stage](Character_Selection.en.md). Players preview the Warrior, Mage, and Ranger, select one, and press Start to call the existing `GameController.EnterPlaza(true)`. The former title character-list dialog is replaced by this dedicated page. Town movement begins after entry.

## Vista layers and login flight (2026-10-14)

The title backdrop is now a set of painted layers instead of one picture. In the spirit of the Zelda: Breath of the Wild key art, the six heroes stand on high cliffs seen from behind and look across a valley at a distant rift. The left cliff holds the archer, mage and warrior (male, female, male); the right cliff the warrior, mage and archer (female, male, female). A crack between two colossal rocks glows violet under a swirling storm.

| Layer | File | Role |
|---|---|---|
| Far valley, landscape and portrait | `vista_far_landscape` 1672×941, `vista_far_portrait` 852×1846 | Sky, valley and rift. The rift core is pinned to the same screen point. |
| Rift close-up | `vista_rift_close` 1672×941 | Replaces the far valley at the end of the flight. |
| Floating rocks | `vista_mid_floaters` 1672×941, alpha | Rush past the camera during the flight; hidden at rest so they do not crowd the logo. |
| Left and right hero cliffs | `vista_heroes_left`, `vista_heroes_right` 1254×1254, alpha | Heroes and cliff painted together. |

- Layout: the rift core sits 47 percent from the top in landscape and 56 percent in portrait. When the button panel is narrower than half the screen, the cliffs run off the bottom corners. When the buttons span the screen (portrait, small windows), the heroes stand just above them and the lowest 30 percent of each cliff fades into the valley mist.
- At rest: the camera drifts slowly and each layer moves by its depth; the rift light breathes and sparks rise from the crack. `Motion paused` stops all of it.
- Login flight (4.4 s, 1.6x slower since 2026-10-20): plays when the title is left by login, guest entry or `Choose character`. 0-3.0 s the hero cliffs grow from 1x to 5x about the rift's vanishing point and leave toward the lower left and lower right, like the camera advancing between them: they gather speed and lose focus as they grow. 0-3.2 s the valley zooms 2.6x toward the rift with a radial smear and shake; 0.16-2.4 s the floating rocks rush past; 2.0-2.8 s the rift close-up takes over; at 3.44 s the rift light fills the screen and the next page is clear by 4.4 s.
- Game flow: login handling and page switching are unchanged. The next page (character selection or the first tutorial) is built at once and the flight plays above it on its own canvas (sorting 2500, below notices at 3000). The UI input module is disabled during the flight so real taps and keys cannot reach the hidden page; after 0.25 s a tap or key jumps to the light. Input returns when the flight ends or the title is shown again. With motion paused the title switches without the flight.
- Sound: the wind rush (`flow.title_flight`) plays at 0.7 s into the flight; when the flight passes 3.12 s the rift burst (`flow.title_rift`) plays, loudest within its first 0.1 s, so it lands as the light blooms toward its peak at 3.44 s. A tap that skips the flight plays the burst alone. When the flight plays, `Choose character` no longer adds the stone door sound (`flow.title_start`); it plays only with motion paused. Both sounds were generated with ElevenLabs ([audio provenance](ElevenLabs_Game_Audio.en.md)).
- Performance: idle motion stays inside the backdrop's own canvas, so the controls are not rebatched. Invisible close-up and light layers are not drawn, and the 8-tap radial smear only runs during the flight. Frame time was not measured.
- Ownership: [TitleVista.cs](../../Assets/HELLSCRIPT/Runtime/Presentation/TitleVista.cs), its [shader](../../Assets/HELLSCRIPT/Resources/Art/Title/TitleVista.shader) and [importer](../../Assets/HELLSCRIPT/Editor/TitleVistaArtImporter.cs). Per the UI contract `GameUI.Title.cs` creates the canvases. The Android budget gains an `Art/Title/Vista/` 2048 ASTC 6x6 row.
- Art provenance: every picture was generated by Codex (`codex exec`, built-in image_gen) and copied byte for byte. A full-composition master came first; each layer was then redrawn with the master and the hero model sheets as references. The tool does not report a model version, so it is recorded as `unknown`. Cut-out layers use their generated alpha without background removal. The prompts are the [master](../Art/TitleVista/2026-10-14/title_vista_prompt_master.txt), [far valley](../Art/TitleVista/2026-10-14/title_vista_prompt_far.txt), [hero cliffs](../Art/TitleVista/2026-10-14/title_vista_prompt_heroes.txt) and [rift close-up with rocks](../Art/TitleVista/2026-10-14/title_vista_prompt_close.txt); per-attempt records are in the [manifest](../Art/TitleVista/2026-10-14/manifest_master.json) and the per-layer manifests beside it.

![Login flight master concept](../Art/TitleVista/2026-10-14/master_vista_v01.png)

Validation (2026-10-14, macOS development build): `RuntimeTitleSmoke` passed with `HELLSCRIPT_TITLE_RUNTIME_OK checks=24`. On PC 16:9, ultrawide and iPhone portrait the login flight sent both hero cliffs off screen, played the rush and burst sounds once each, ran above character selection with the real input module disabled and returned input afterwards. The existing viewport, safe area, overlap, clipping, Korean/English and motion pause checks still passed. `RuntimeCharacterSelectionSmoke` passed with `checks=12`, including a scripted UI click during a flight. The focused Edit Mode tests (`TitleScreenTests` 17, `CharacterSelectionTests` 10, `BuildResourceTests` 14, `SoundBankTests` and `NewContentAudioTests` 24) passed 65/65, including six new tests. `test_elevenlabs_audio.py` passed 7/7. `python3 tools/check_ui_contract.py --verify` passed 15/15.

Not verified: a human listening check of the sounds, the full Edit Mode suite and smoke bundle (run once after the final `main` integration), iPhone and Android devices and WebGL, frame time and GPU cost, skipping with a real tap or key (smokes cannot create real input, so frames were checked at set times), and the flight after real Google authentication.

![PC 16:9 title](TitleVistaEvidence/pc-16x9-ko.png)

[Rush at 1.0 s](TitleVistaEvidence/pc-16x9-flight-1.0.png) · [Rift close-up at 1.5 s](TitleVistaEvidence/pc-16x9-flight-1.5.png) · [iPhone portrait](TitleVistaEvidence/iphone-portrait-ko.png)

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

[GameUI.Title.cs](../../Assets/HELLSCRIPT/Runtime/Presentation/GameUI.Title.cs) hosts the title. [TitleScreenView](../../Assets/HELLSCRIPT/Runtime/Presentation/TitleScreenView.cs) and its [dialogs](../../Assets/HELLSCRIPT/Runtime/Presentation/TitleScreenView.Dialogs.cs) own presentation. [TitleSession](../../Assets/HELLSCRIPT/Runtime/Core/TitleSession.cs) holds mock session state in memory, separate from game saves. The vista layers above ([TitleVista](../../Assets/HELLSCRIPT/Runtime/Presentation/TitleVista.cs)) draw the title backdrop; [TitleAtmosphere](../../Assets/HELLSCRIPT/Runtime/Presentation/TitleAtmosphere.cs) and its [shader](../../Assets/HELLSCRIPT/Resources/Art/Title/TitleAtmosphere.shader) still draw the character selection and waiting room backdrops.

The former title backdrop, the [1672×941 TitleSanctuary PNG](../../Assets/HELLSCRIPT/Resources/Art/Title/TitleSanctuary.png), remains in the rift entry and result windows. It is an unchanged copy from built-in `image_gen`. The tool returned only `image_url` and `output_hint`; its C2PA creation action identifies `softwareAgent.name=ChatGPT`, `version=gpt-image`. No exact model version can be verified, so this is not attributed specifically to `gpt-image-2` or classified as approved release artwork. The [exact prompt](Title_Screen_Prompt.txt) and [provenance](TitleScreenEvidence/provenance.json) are retained.

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

## Foreground approach and defocus (2026-10-18)

During the first 1.2 seconds of the login flight, the hero cliffs grow from 1× to 2.1× and leave diagonally through the lower left and right corners. Downward travel compensates for growth so the feet move down rather than rise. A foreground-only, alpha-weighted 5×5 Gaussian blur ramps between 0.08 and 0.85 seconds to a maximum radius of 0.4% of the view height, avoiding dark cutout fringes. Existing central rift effects, overall timing, input, authentication and saves retain their current owners.

The 29 focused title Edit Mode cases and 15 UI contract regressions passed. Five defocus cases initially failed because the test viewport had no parent bounds; after fixing the fixture, only those cases were rerun and passed. The shader/asset case passed again after refining the blur kernel. An isolated Editor camera rendered five times at each of 440×956, 956×440, 1440×810, 2560×1600 and 3440×1440, with simulated control bounds. This is render evidence, not actual player input proof. Player builds, physical mobile devices and GPU performance remain unverified; older runtime results above describe their original versions.

Native macOS follow-up: built successfully (0 errors, 227 existing build warnings). The title smoke completed three flight sequences, checked off-screen exits, sounds, input blocking/restoration and character-selection handoff. Its final gameplay assertion failed because it skips the current character-creation phase and waits only 1.2 seconds for the 2.45-second entry. A subsequent native run and the user confirmed normal gameplay entry. The user explicitly accepted the transition on 2026-10-18. Physical-mobile/GPU performance remain unverified.

## Camera advance and sound replacement (2026-10-20)

The heroes no longer slide sideways; the camera now advances between them. Each hero cliff grows about the rift's vanishing point (`TitleVista.Cliff`, lens 1 to 5, accelerating) and leans outward and down, so the middle opens, and the whole flight plays `Pace` 1.6 times slower, from 2.75 s to 4.4 s. The hero blur reuses the existing shader with a peak radius of 3 percent of the screen height; each tap reads the mip level that matches its spacing so a wide blur does not show the 5x5 grid as ghost copies. Only the two hero textures are therefore imported with trilinear mips (`TitleVistaArtImporter`).

The wind rush plays 0.7 s into the flight (`RushAt`) and the rift burst at 3.12 s (`ImpactAt`). Both sounds were regenerated with ElevenLabs ([audio provenance](ElevenLabs_Game_Audio.en.md)).

Checked: 0 compile errors. The related Edit Mode cases (hero motion, blur ramp, import contract, backdrop and shader loading, painting cover, audio bank resolution) passed. Portrait 788x1256 and wide 1600x900 frames rendered over 0-2.4 s show the heroes growing, losing focus and leaving through both lower corners. Not checked: a human listening review, the macOS runtime smoke (`-hellscriptTitleSmoke`), the full Edit Mode suite and device performance.
