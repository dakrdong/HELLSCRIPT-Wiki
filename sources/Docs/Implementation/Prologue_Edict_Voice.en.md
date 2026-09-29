# Prologue — the Voice of the Edict

Updated: 2026-09-29

[한국어](Prologue_Edict_Voice.md)

At the user's request, the first mandatory map, "Road to the sanctuary" (an armor award and equip step), has become the "Voice of the Edict" prologue. The player appears as a god whose face is hidden behind a blinding halo, and saves the hero by sending down the Hunt Edict scroll. Both Hunt Edict setups are forced steps where only the named control can be pressed.

## Flow

| Step | Run state (`ProloguePhase`) | Screen |
| --- | --- | --- |
| 1 | `Surrounded` (paused) | The hero kneels in despair, ringed by six monsters. After the hero's lines, a shaft of light falls and the voice from above commands. |
| 2 | 〃 | The scroll descends and the hero raises it to the sky with both arms. It flares, and the Hunt Edict emblem flies to its menu slot at the top right. |
| 3 | `EdictLesson` (paused) | Forced steps: Hunt Edict menu → Skill tab → first skill → `+` (the one skill point) → Equip → Save → slot 1 → Edit hunt edict → the four preset tabs explained in turn → the chosen preset → Activate preset → Close. |
| 4 | `Encircled` | A real fight against the ring. |
| 5 | `Gatekeeper` | A short dialogue, the walk to the boss room and the gatekeeper's entrance. |
| 6 | `SurvivalLesson` (paused) | When the hero drops below 50% HP, time stops and the voice intervenes. Forced steps: Hunt Edict → Survival → Automatic potion → quick preset → Extra survival supplies → Avoidance by damage type → quick preset → Avoid all warnings → Save → Close. |
| 7 | `Showdown` | The fight continues with the new settings. |
| 8 | `Cleared` | Victory staging and a **1,000 gold** reward. "Take the reward and go to the village" stores the reward and the finished tutorial in one transaction. |
| 9 | Town | The arrival card, then Anton Jindark's first conversation (the start of the village event). He welcomes the stranger and says a revelation from the god sent him out to meet them. Demons poured out of the rifts and brought the world into peril, and a prophecy says one who receives the god's revelation shall strike down the demons of the rifts. Many warriors who received it fight here together; their settlement grew into a village of their own, so merchants for warriors came to stay. He ends with how to walk and the first rift. |

### Forced skill per class

Every class already has three level 1 skills (W01–W03 and so on), so the prologue fixes one skill to force (`Tutorials.PrologueSkill`). The steps name that skill by id only.

| Class | Skill | Starting preset | Preset to choose |
| --- | --- | --- | --- |
| Warrior | W01 Whirlwind | Stationary spin | Survival-first combat |
| Ranger | A01 Piercing Shot | Pierce priority enemies | Pierce without waiting |
| Mage | M01 Fireball | Focus on elites | Steady fire |

## Ownership and saving

- **Starting state**: a new hero has every skill at a free rank 1 (version 3) and its first skill already in slot 1, so "spend one point and equip" would not exist. On the first entry `GameStore.PreparePrologue` resets the allocation (version 4, one point at level 1, empty slots), sets the first skill's starting preset and saves.
- **Forced steps**: `GameUI.TickPrologueLesson` reads the real saved state (the owned hero's skills, presets and edict values) and the window every frame and names the one control to press next. Closing the window or changing tabs just recomputes the next step.
- **Gate**: `PrologueGate` is its own canvas above every content window (order 2000). Only the named control's area lets input through; the rest is dimmed and blocked, and Escape is held. If the control cannot be found for two seconds the block lifts, so the player is never trapped. Per the shared UI contract, `GameUI.cs` creates the canvas.
- **Edict saves**: the tutorial run keeps its checkpoint in `guide.tutorialRun`, so `CommitHuntEdict` gained a branch that applies the change to that run. A replay applies it to its copied hero only.
- **Reward**: `CompleteTutorialRun` (transaction `tutorial-map-complete-v2`) stores 1,000 gold and completion together. A replay pays nothing. On arrival the just-chosen Extra survival supplies restocks potions, so gold drops at once; that is expected (175 in the smoke).
- **Old saves**: a checkpoint from the armor road (`tutorial-v1`) means different phases and is dropped; the prologue starts over.
- Before the intervention the gatekeeper cannot fall below 35% health. After a defeat only the hero is restored; the gatekeeper keeps its wounds.

## Staging assets

| Asset | Location | Notes |
| --- | --- | --- |
| Voice from above portrait | `Resources/Art/NpcPortraits/prologue-divine-voice.png` | A faceless god behind a halo. Codex built-in image generation, model unknown |
| Edict scroll | `Resources/World/Fx/edict_scroll.png` | A world card facing the camera. It renders after the light shaft, which otherwise washes it out to white |
| Despair and offer poses | `ActorRig` `Despair`, `Offer` | Two poses added to the procedural rig |
| Light shaft | `WorldFx.DivineLight` | Reuses the existing beam, mote and rune circle sheets |

Request and generation records: [prompt](../Art/Prologue/prologue-art-prompt.txt), [manifest](../Art/Prologue/prologue-art-manifest.json).

Beside a speaker's name the role reads "The Voice of the Edict" for the voice and "The Chosen" for the hero. On a line the hero speaks, the dialogue no longer draws the same hero again as the listener (`StoryDialogueWindow`). The intervention's gold wash is cleared when its dialogue ends, and the wash now covers the corners of a portrait screen too.

## Verification

- Edit Mode (batch mode, project copy): `TutorialProgressionTests` 30/30. For all three classes it checks the prepared state (one point, empty slots, starting preset), real combat through the ring → gatekeeper → intervention below 50% HP → gatekeeper death, 1,000 gold paid once, the restart checkpoint, the old checkpoint restarting, and defeat recovery. The related suites (localization, shared UI, combat journal, stored text, Hunt Edict, fields, dialogue, world FX, content unlocks) passed 314 tests; one earlier failure (the data no longer has a level 3 active skill) was fixed along the way.
- `tools/check_ui_contract.py` and `tools/test_ui_contract.py` pass.
- macOS development build tutorial smoke (two processes, 20 resolution/language/text-size profiles such as 440×956): intro dialogue layout, 15 forced skill steps and 10 survival steps pressed with real pointers, the gate swallowing another tab, resume after restart, the boss intervention, the reward, town arrival, and a replay that leaves the account unchanged.
- Not checked on a physical mobile device.

## Screen evidence

- [Surrounded](PrologueEvidence/prologue-02-despair.png) · [Light](PrologueEvidence/prologue-03-light.png) · [Voice](PrologueEvidence/prologue-04-voice.png)
- [Scroll descends](PrologueEvidence/prologue-05-scroll-descends.png) · [Raised](PrologueEvidence/prologue-06-scroll-raised.png) · [Emblem flight](PrologueEvidence/prologue-07-emblem-flight.png) · [First forced step](PrologueEvidence/prologue-08-edict-forced.png)
- [First skill](PrologueEvidence/lesson-skill-03.png) · [Point](PrologueEvidence/lesson-skill-04.png) · [Slot 1](PrologueEvidence/lesson-skill-07.png) · [Preset tour](PrologueEvidence/lesson-skill-09.png) · [Preset choice](PrologueEvidence/lesson-skill-13.png)
- [Gatekeeper](PrologueEvidence/prologue-12-boss-entrance.png) · [Intervention](PrologueEvidence/prologue-13-intervention.png) · [Automatic potion](PrologueEvidence/lesson-survival-05.png) · [Avoid all warnings](PrologueEvidence/lesson-survival-08.png)
- [Victory](PrologueEvidence/prologue-14-victory.png) · [Reward](PrologueEvidence/prologue-15-reward.png) · [Arrival dialogue](PrologueEvidence/prologue-17-arrival-dialogue.png)
- [Smoke result](PrologueEvidence/runtime-tutorial-smoke.txt)
