# Tutorial staging and the game dialogue box

Updated: 2026-09-28

[한국어](Tutorial_Staging.md)

The first tutorial used to advance through explanation windows and a "Continue" button, with no staging at all. The mandatory map, *Road to the sanctuary*, is now told through staged scenes and a speaking character. The plain dialogue window became a game dialogue box, and NPC conversations and first-use content guides use the same box.

Progression conditions, saving, the armor award, reward isolation and restoration still follow [the tutorial implementation](Tutorial_Progression.en.md). This work only changes what is shown and in which order; every choice calls the existing progression command.

## The guide

**Anton Jindark**, the sanctuary's rift keeper, guides the first journey. He speaks to the distant hero through the amber compass he holds in his portrait, and when the hero reaches town he is the one who prepares the first rift. His portrait and name are the existing definitions from [NPC portraits and dialogue](Npc_Dialogue_Portraits.en.md); no new art was created for this work.

## Flow of the first map

| Step | Staging | Lines and choice |
| --- | --- | --- |
| Opening | The screen fades in from black and cinematic bars slide in. The camera leans in on the hero and slowly pulls back while the *Prologue · Road to the sanctuary* title card appears. | Anton introduces himself; on his second line the camera frames the monsters blocking the road. He explains automatic movement and attacks and that skills follow the hunt edict. **Begin the fight** |
| First fight | The bars lift and a *New objective* notice appears. Under the battle title, `Objective · Defeat the monsters blocking the road 0/3` counts the kills. | At the first kill Anton calls out a short line at the top of the screen. The fight never stops. |
| Armor | An *Objective completed* notice is followed by the item reveal: a halo and flash in the item's grade colour turn behind the shared equipment slot, with the grade's pickup sound. | Anton tells the player to compare and equip the armor. **Open inventory** |
| Equipping | The compare and equip targets carry a breathing gold border, a spreading ripple and a bobbing pointer. | The real inventory performs the comparison and equipment. |
| Equipped | An *Objective completed* notice appears. | Anton describes the gatekeeper and the red warning gauge: the attack lands the moment it is full, which is the actual combat rule. **To the gatekeeper** |
| Gatekeeper entrance | When the boss comes within the distance at which its model is drawn, the fight pauses. The bars return and the camera frames the hero and the boss together. A roar, a screen shake and a red edge wash lead into the boss name card. | The fight resumes and Anton shouts to dodge the warnings. At half health he urges the player on once more. |
| Falling | A red edge wash and a *You have fallen* card. | Anton announces the section restart. **Rise again** |
| Victory | Bars, a gold edge wash and a *Victory · Gatekeeper defeated* card. | Anton says he will wait in town. **Head for the sanctuary** fades to black and travels to town. |
| Arrival | The screen fades back in with the *Ashwood · Settlers' village* place card. | Anton explains how to walk and where he is. **Go to Anton** requests the existing auto-walk; **Look around** only closes the dialogue. The equivalent "New guide" toast is not shown at this moment. |

Replay uses the same staging. Its first line states that it is a level 1 copy that leaves nothing on the account, and the armor step still continues into the detail window that equips the copy only. The arrival lines appear only on the first real completion.

During a staged scene, a tap anywhere or the **Skip** button at the top right ends it at once. Combat staging only skips its cards and fades; progression and saving are untouched.

## The game dialogue box

Like the reference screen the user shared, it is a story scene: large character art with transparent surroundings stands out of the text area.

- **Band**: a full-width band docked to the bottom of the safe area, at most 30% of its height. Its top fades in from clear and the lower part where the text sits is an opaque lacquer body; under linear blending even a 0.96-alpha dark body lets the HUD behind show through visibly, so it is opaque behind text. A brass double rule whose ends fade out runs across the top.
- **Name plate**: a pointed cartouche sits in the middle of the rule with the name and the role (or the guide title). If space runs out, the role gives way first.
- **Speaker**: in landscape the bust stands on the left, rising from the bottom of the safe area to about 80% of the screen height, drawn in front of the band. On a narrow portrait screen it stands on the band, which covers where the picture is cut. When the speaker changes, the figure fades in where it stands.
- **Listener**: in landscape the hero stands dimmed on the right, using the bust for their class (Warrior, Ranger, Mage), also in replays. A narrow screen shows the speaker only, because two figures side by side would hide the whole field.
- **Line**: centred in the space between the figures, never overlapping them, written at 38 glyphs per second. The sentence itself is never shortened; only the glyph quads fade in, so wrapping, height measurement and every text check see the finished sentence from the first frame. A line that does not fit between the two figures takes the listener's place. If large text still makes it longer than the band, a thin scroll rail appears beside it and the view follows the writing down until the reader scrolls it themselves.
- **Advancing**: the first press finishes the line being written and the next one moves on. Tapping anywhere, Space/Enter or the ▶ button at the bottom right all work; ▶ blinks once a line is finished. **Skip** and Escape/Back jump to the last line.
- **Choices**: they appear centred under the line once the last line is written, and keep the existing tutorial button names (`tutorial-continue`, `tutorial-practice` and so on).
- Every text is passed as its Korean source and translated when drawn. Changing language, text size or orientation redraws the same line at the same progress.

NPC conversations are drawn as the same scene. The service, sell and goodbye choices are offered at once, even while the greeting is being written, and close and Back end the conversation. The hero stands dimmed on the right.

### Hero busts

The Warrior, Ranger and Mage busts that stand on the listening side were requested from GPT (Codex built-in image_gen), as the user has asked for new screen art. Two existing NPC portraits were the style references and each class's character-selection art was the identity reference. The 1024×1536 PNGs and their alpha are used as generated, without chroma key or background removal. The tool did not report the actual model, so the model is unverified and the art is recorded as a prototype candidate. They live in `Resources/Art/NpcPortraits/hero-*.png` so they receive the NPC portrait import settings. [Generation record](../Art/HeroPortraits/hero-portraits-manifest.json) · [Request](../Art/HeroPortraits/hero-portraits-prompt.txt)

## First-use content guides

The first time a blacksmith, gambler or jeweler content opens, the resident who runs it explains it in the dialogue box instead of the full-screen journal: the blacksmith Mark Kus explains enhancement, Guzel Pan explains gem fusion, and so on. A remaining first-practice support is announced on a second line. **Practice, Read and Later** record the same facts as the journal buttons. The *Adventure guide* journal remains the place to browse every guide.

## Ownership

| File | Responsibility |
| --- | --- |
| `StoryDialogueWindow.cs` | Line and choice definitions, progression (`StoryDialogueState`), the story-scene layout (`Scene`), input (`StoryDialogueDriver`), glyph reveal (`DialogueReveal`), the figure entrance, the band (`DialogueBandGraphic`) and the name plate (`CartoucheGraphic`) |
| `TutorialCinematic.cs` | The staging layer: bars, fades, title cards with their dark band, objective notices, companion lines, the item reveal, edge washes and skip |
| `GameUI.TutorialStaging.cs` | Per-step lines, choices and scene order; the gatekeeper entrance, the arrival and the resident guides |
| `GameUI.cs` | Creates the staging canvas at sorting order 130: above the play HUD (105) and below every content window (400+). |
| `WorldView.cs` | `Frame(focus, zoom)` moves only the camera. No gameplay rule reads it, and clearing the dungeon resets it. |
| `TutorialAnchorRing.cs` | Target highlight. Inside a scrolling list it points from the right side, because the space above a first-row cell is masked. |
| `NpcDialogueWindow.cs` | NPC conversation: opens `ContentWindowView` and draws it with `StoryDialogueWindow.Scene`; services keep the existing `GameController.NpcDialogue` path. |

The staging layer neither reads nor writes an account. Combat dialogues open with `blocksGameplay:false` so the world keeps presenting and the camera can move, while the window host's pause lease still applies. Town dialogues keep the default and block movement and background input. The gatekeeper entrance releases its pause through `ContentWindowHost.AcceptPauseState` when it ends. If the app closes during a scene, `CombatSimulation` restores each step's pause rule on reopening, so the fight is never left frozen.

The reduce-motion setting (`hellscript.reduce-motion`) turns off the glyph reveal, the figure entrance, camera zoom, the pointer's bobbing and the ripple. Screen shake follows the existing lighting rule.

## Translation

Forty-eight Korean and English lines for dialogue, cards, objectives and buttons were added to the checked section of the table, and ten translations of guide lines the new flow no longer uses were removed. In English, long titles and companion lines shrink to fit their boxes.

## Validation record

The baseline is Unity 6000.6.0f1 and a work branch from source main `672e4a0c` with main `40e601c4` (moving bosses and build optimization) and then `b418de72` (new enemy attacks) merged in. The full Edit Mode run used the code merged with `40e601c4`; the focused Edit Mode run, the build and the tutorial and NPC runtime evidence used the code merged up to `b418de72`. Just before the PR, the code merged up to main `bfaf2ddc` (hunt edict overview) passed focused Edit Mode 85/85, the build, both tutorial processes and the NPC smoke again. No Editor was running, so instead of the CoplayDev Unity MCP the established path was used: batch-mode tests and builds on a cloned verification project. The source repository's ProjectSettings were not touched.

| Check | Result |
| --- | --- |
| Focused Edit Mode | 85 / 85 passed, 0 skipped. Includes six new `StoryDialogueTests` (a press finishes the line before moving on, skip, an empty dialogue is refused, only unwritten glyphs are hidden, guide and resident portraits, a hero bust for every class) plus the localization, NPC, tutorial and first-play suites. [Results](TutorialStagingEvidence/editmode-focused.xml) |
| Full Edit Mode | 4,537 of 4,584 passed, 47 failed, 0 skipped (code merged with main `40e601c4`). The failures are ActionContinuityTests 36, CurrentBuildSaveTests 4, RestoreFidelityTests 3, EnemyTests 2, EdictTargetIntegrationTests 1 and RuleTests 1; running those six fixtures on the latest main `b418de72` fails the same 47. No test fails only with this work. [Summary](TutorialStagingEvidence/editmode-full-summary.json) |
| Shared UI ownership | Passed; validator tests 9 / 9 |
| macOS development build | Succeeded, 0 build errors. [Result](TutorialStagingEvidence/build.txt) |
| Tutorial runtime (two processes) | Passed. The first process covers the opening staging and dialogue, real combat and the armor checkpoint. The second covers restoration, real comparison and equipment, the gatekeeper entrance (pause and resume checked), the real boss kill, victory, town arrival, the journal, the first rift, edict and new-skill saves, training and retry, the resident guide, supported practice and the rewardless replay. [First process](TutorialStagingEvidence/phase-one.txt) · [Full result](TutorialStagingEvidence/runtime-tutorial-smoke.txt) |
| Layout matrix | The opening dialogue, the arrival dialogue and the resident guide each cover 20 combinations (440×956, 956×440, 1600×900, 1600×1000, 2100×900 × Korean/English × 100%/150% text): bottom 30% docking, portrait aspect and safe area, clipped button and name-plate text, and Korean left in English screens. |
| NPC dialogue | The existing NPC smoke passed: 12 NPCs, 242 layout/language/text-size checks, 508 raycast-verified clicks, 73 greeting scroll-to-end checks and 11 service dispatches. It also confirms that a tap on the band completes the greeting before each capture. For the new layout, in which the figure stands out of the band, the portrait rule changed from "inside the frame" to "inside the safe area and clear of the greeting and actions". [Result](TutorialStagingEvidence/npc-runtime.txt) |
| Baseline comparison | On a main `672e4a0c` build the same smoke's second process timed out waiting 400 s for the first rift result, after every tutorial stage had passed. With this change the first rift ended within about two minutes in both runs. The wait is now 1500 s and a timeout records combat time, pause, portal, navigation error and health. What caused the baseline timeout was not investigated. |
| Physical Android/iOS | Not verified. macOS synthetic pointer and window-size checks are not physical-device results. |
| Human feel and comprehension | Not verified |

The SFX smoke (`RuntimeSfxSmoke`) and first-play acceptance (`RuntimeFirstPlayAcceptance`) only had their tutorial button flow adapted to the new dialogue. Both were already on the list of failing smokes before this work and were not rerun here.

### Scenes

![Opening title card](TutorialStagingEvidence/staging-intro-card.png)
![Anton's first line](TutorialStagingEvidence/staging-intro-dialogue.png)
![New objective notice](TutorialStagingEvidence/staging-objective.png)
![Companion line in combat](TutorialStagingEvidence/staging-companion-line.png)
![Item reveal](TutorialStagingEvidence/staging-armor-reveal.png)
![Gatekeeper entrance](TutorialStagingEvidence/staging-boss-entrance.png)
![Victory card](TutorialStagingEvidence/staging-victory.png)
![Arrival place card](TutorialStagingEvidence/staging-arrival-card.png)
![Arrival dialogue](TutorialStagingEvidence/staging-arrival-dialogue.png)
![The blacksmith explains enhancement](TutorialStagingEvidence/staging-briefing-typing.png)

More: [inventory highlight](TutorialStagingEvidence/staging-inventory-pointer.png) · [dialogue after the gatekeeper](TutorialStagingEvidence/real-boss-cleared.png) · [replay victory](TutorialStagingEvidence/rewardless-replay.png)

### Layout samples

The smoke checked all 20 combinations automatically; representative captures are kept here.

![PC 16:9 dialogue: Anton with the hero dimmed](TutorialStagingEvidence/intro-1600x900-ko-100.png)

| Screen | 440×956 Korean 150% | 956×440 English 100% |
| --- | --- | --- |
| Opening dialogue | [PNG](TutorialStagingEvidence/intro-440x956-ko-150.png) | [PNG](TutorialStagingEvidence/intro-956x440-en-100.png) |
| Arrival | [PNG](TutorialStagingEvidence/arrival-440x956-ko-150.png) | [PNG](TutorialStagingEvidence/arrival-956x440-en-100.png) |
| Resident guide | [PNG](TutorialStagingEvidence/briefing-440x956-ko-150.png) | [PNG](TutorialStagingEvidence/briefing-956x440-en-100.png) |

NPC dialogue: [Anton Jindark 1440×810 Korean](TutorialStagingEvidence/npc-anton-jindark-1440x810-ko-100.png) · [Mark Kus 1440×810 Korean](TutorialStagingEvidence/npc-mark-kus-1440x810-ko-100.png) · [Turk Garbi 440×956 English 150%](TutorialStagingEvidence/npc-turk-garbi-440x956-en-150.png)

[Screenshot hashes](TutorialStagingEvidence/screenshots-manifest.json)
