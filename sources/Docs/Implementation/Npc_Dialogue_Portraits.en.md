# NPC portraits and food-inspired dialogue

Updated on: 2026-09-27

[한국어](Npc_Dialogue_Portraits.md)

All twelve human NPCs currently placed in the field have individual upper-body portraits and greetings: nine attendants and three residents. The Aspect Stone is a facility, not a person. **Turk Garbi (tteokgalbi)**, selected from the existing name list, is the training instructor.

## Authored identities

The nationality motifs, genders, heritage, skin tones and appearances come from the [original conversation supplied by the user](https://chatgpt.com/s/cx_6ab9076cbfdc8191b39dd57b8bd611bd). Nationalities are fictional design motifs. Food colors and textures inform clothing and accessories, not edible bodies or food costumes.

| Role | Canonical name | Nationality motif | Gender | Dish |
| --- | --- | --- | --- | --- |
| Blacksmith | Mark Kus / 마르크 쿠스 | Germany | Male | Makguksu |
| Weapon merchant | Jake Bokun / 제이크 보쿤 | United States | Male | Jeyuk bokkeum |
| Warehouse | Chador Samaf / 차도르 사마프 | Iran | Female | Chadol samhap |
| Rift keeper | Anton Jindark / 안톤 진다크 | Poland | Male | Andong jjimdak |
| Rune master | Injel Mir / 인젤 미르 | Austria | Female | Injeolmi |
| Resident | Pyonya Nermwen / 표냐 내르뭰 | Kazakhstan | Female | Pyongyang naengmyeon |
| Resident | Jean Jorin / 쟝 죠린 | China | Female | Jangjorim |
| Resident | Darc Alvi / 달크 알뷔 | Hungary | Female | Dakgalbi |
| Gambling merchant | Jacques Chei / 자크 체이 | France | Male | Japchae |
| Training instructor | Turk Garbi / 터크 가르비 | Türkiye | Male | Tteokgalbi |
| Jeweler | Guzel Pan / 구젤 판 | Azerbaijan | Female | Gujeolpan |
| Rune merchant | Mishu Karu / 미슈 카루 | Romania | Female | Misutgaru |

The eight previously placed NPCs retain their final spellings. Earlier candidate spellings are the identity sources for Jake Bokun, Anton Jindark, Jean Jorin and Darc Alvi; their nationalities and genders are not inferred again from their revised names. Four existing candidates fill the previously unnamed gambling, training, jewelry and rune services.

## Interaction and ownership

Selecting an NPC requests movement to a point 1.4 m in front of them, avoiding overlapping bodies. The interaction card keeps the expanded content shortcuts clear. Arrival does not automatically converse or transact. Use **Talk** nearby or press E to see the portrait, name, role and greeting. Existing service shortcuts remain available. Attendants offer their existing services in the dialogue footer; weapon and gambling merchants expose separate Buy/Sell actions, while residents only offer conversation dismissal.

Conversation reach uses the NPC's actual position. Services revalidate reach and the existing service access checks when invoked. The rift attendant and existing portal both retain access. The shared window host blocks movement/background input and handles close/back. Reading a greeting does not change the account, rewards or trades. Language, text scale and orientation changes retain the dialogue identity.

[NpcProfiles.json](../../Assets/HELLSCRIPT/Resources/NpcProfiles.json) owns identities, food concepts, appearances, greetings and portrait resource paths. [NpcProfiles.cs](../../Assets/HELLSCRIPT/Runtime/Core/NpcProfiles.cs) connects these definitions to field placement and range. Town labels use the same names; English uses the existing `Localization/en.txt` table.

`NpcDialogueWindow` was created with `tools/new_content_ui.py NpcDialogue` and uses `ContentWindowView` with `EquipmentViewSource.Catalog`. It never reads or mutates an account. `GameController.NpcDialogue` dispatches existing services. The window reuses `ContentWindowHost`, `UiTheme`, `UiFonts`, scrolling content and fixed actions. The shared bottomDock option anchors the panel to the safe-area bottom and caps its height at 30% of the safe area (about 25% on a normal portrait screen). Including the bottom inset, it remains within the bottom 33%. The full-screen backdrop is transparent, leaving the field undimmed. A small portrait stays on the left; the name sits above the independently scrolling greeting on the right. The portrait, name and actions remain fixed while text scrolls. Landscape keeps the role beside the name; portrait gives actions the full panel width. Art retains its aspect ratio and text scale settings remain active. Shared body sizing and compact spacing leave room for at least two greeting lines even at 150% text. The shared host still blocks background input and movement.

## Art provenance

The user approved using the available built-in image generation surface. Its response does not disclose the actual model, so these files are not claimed as verified `gpt-image-2` outputs. Provenance remains `candidate_model_unknown`, with user approval to use these built-in results recorded separately. Native PNG alpha is preserved without chroma key or background-removal postprocessing. The scoped importer preserves alpha, caps the imported long side at 1024, disables mipmaps and uses uncompressed textures; original PNG files remain intact.

[Prompts](../Art/NpcPortraits/prompts.json) · [Generation results](../Art/NpcPortraits/generation-results.json) · [Alpha checks](../Art/NpcPortraits/alpha-validation.json) · [Portrait gallery](Npc_Dialogue_Portraits.md#초상화)

## Compact bottom layout validation

After the layout revision on 2026-09-27, **48/48 NPC, town movement and shared UI Edit Mode tests** passed. The macOS development build completed with zero errors. [Tests](NpcDialogueCompactEvidence/editmode.xml) · [Build](NpcDialogueCompactEvidence/build.txt)

The native player passed **242 layout checks**, covering all twelve NPCs, five resolutions, Korean/English and 100%/150% text. Assertions verify bottom anchoring, the 30% height cap, transparent field backdrop and fixed portrait/actions. All 76 overflowing greeting cases passed uGUI scroll-to-end, return and pointer-drag checks. The 508 synthetic clicks and 11 service dispatches also passed, including distance gates, dismissal/back and unchanged account state. [Runtime](NpcDialogueCompactEvidence/runtime.txt)

[PC Korean](NpcDialogueCompactEvidence/turk-garbi-1440x810-ko-100.png) · [PC English](NpcDialogueCompactEvidence/turk-garbi-1440x810-en-100.png) · [Portrait Korean 150%](NpcDialogueCompactEvidence/turk-garbi-440x956-ko-150.png) · [Portrait English 150%](NpcDialogueCompactEvidence/turk-garbi-440x956-en-150.png) · [Landscape English 150%](NpcDialogueCompactEvidence/turk-garbi-956x440-en-150.png) · [16:10](NpcDialogueCompactEvidence/turk-garbi-1440x900-en-100.png) · [21:9 at 150%](NpcDialogueCompactEvidence/turk-garbi-1680x720-en-150.png) · [Simulated notch](NpcDialogueCompactEvidence/training-notched-portrait-en-150.png)

Shared UI ownership and nine contract tests, wiki build/check, ten Python tests and the JavaScript checks passed. This is branch validation; physical mobile was not tested.

## Initial feature validation

Unity 6000.6.0f1 passed **25/25 NPC and town movement Edit Mode tests**, covering distinct imported portraits, canonical identities, reachable approach points, distance guards and English dialogue. [Focused report](NpcDialogueEvidence/npc-town-editmode.xml)

The broader localization run passed 57/58 tests. Its single failure reports two existing missing translation keys in `RuntimeFirstPlayAcceptance.cs`: `눈보라 검사 순서` and `눈보라를 공격 순서`. Both exact strings and missing English entries were verified in the unmodified base commit. All new NPC lines passed. These overlapping test runs are not additive. [Combined report](NpcDialogueEvidence/editmode-final.xml) · [Baseline comparison](NpcDialogueEvidence/baseline-localization.json)

The native macOS development build completed with zero build errors. The actual player passed **242 portrait/layout checks, 508 raycast-verified synthetic clicks and 11 service dispatches**: nine facilities plus the two merchants' Sell tabs. Coverage combines all twelve NPCs with 440×956, 956×440, 1440×810, 1440×900 and 1680×720; Korean/English; and 100%/150% text. Further checks cover changing language/scale/orientation while open, a simulated notch, portrait aspect, text fit, safe-area bounds, scrolling/fixed-action separation, unobscured expanded shortcuts, world picking, remote and stale-distance rejection, movement blocking, close/back and unchanged account data while conversing. Dynamic-font wrapping at 150% has an extra line of clearance. The training instructor's field object, nameplate, portrait dialogue and training entry were verified together.

[Build result](NpcDialogueEvidence/build.txt) · [Runtime result](NpcDialogueEvidence/runtime.txt) · [Training field](NpcDialogueEvidence/training-npc-world.png) · [Training entry](NpcDialogueEvidence/training-service.png)

The existing town HUD regression also passed 48 resolution/scale/language combinations, safe areas, joystick input and expanded shortcuts. All nine services and three residents passed the existing nameplate/placement path in Korean landscape and English portrait. [HUD runtime](NpcDialogueEvidence/town-hud-runtime.txt) · [NPC placement](NpcDialogueEvidence/town-hud-npc-runtime.txt)

No MCP Editor instance was connected, so validation used the existing Unity batch tooling and native macOS development player. Phone dimensions are simulated desktop windows; physical mobile touch and performance remain unverified. These are branch validation results, not evidence of a main merge or public wiki deployment.
