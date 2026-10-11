# Guide Tutorial Segment 1 (2026-10-11)

Updated: 2026-10-11

[한국어](Guide_Tutorial_Script_20261011.md). The first segment of the tutorial-only stage (flow 6) that replaces the puzzle-level tutorial: script, guide profile, implementation and verification.

## The guide

- Name: **Velina**, "Goddess of the Edict". The name comes from *velum*, a veil. The blindfold is only shown in the art; her lines never explain the setting, such as the blindfold or stopped time (user request, 2026-10-11).
- Voice: **polite**, and she speaks **to the player**, not to the hero. The hero is "the hero you are watching".
- Portrait: Codex candidate B (guiding with a light in her palm). `Resources/Art/NpcPortraits/guide-goddess(-mirror).png`; source, prompt and manifest in `Docs/Art/GuideGoddess/`.
- The developer never appears; she speaks only as "the goddess who shows this world's rules", to keep the fiction intact.

## Flow and lines

The map is four rooms (entrance hall, first arena, long gallery, plaza) joined by bent corridors, climbing up and to the right. Segment 1 uses the first two rooms; the rest is the road for later segments.

| Beat | Situation | Time | Velina |
| --- | --- | --- | --- |
| Entrance | "The Edict's Road" card; the hero walks in from the left edge of the entrance hall | Running | — |
| Greeting | Stops after a few steps | Stopped | Welcome. I am glad you have come to this world. / I am Velina, the goddess who keeps this world's edicts. So that you do not lose your way here, I will show you this world's rules one at a time. / The road is not short, but I will not rush you. Please follow at your own pace. → [I will follow] |
| Road | The hero follows the road to the first arena and fights the first foe with basic attacks (no skills) | Running | — |
| First talk | Right after the first blows | Stopped | Did you see? The hero met the enemy and swung the sword unbidden. / The hero you are watching fights unbidden whenever an enemy appears. You need not order the sword drawn each time. / But how the hero fights is yours to decide. The hero will do exactly what you write in the Hunt Edict, to the letter. / For now there are only plain attacks. Please watch this fight to the end first. → [Watch the fight] |
| Surround | When the first foe falls, eight appear on all sides | Stopped | Look, every side is blocked. Plain attacks alone can hardly hold off this horde. / Now please teach the hero how to use a new skill. / With the skill you choose, the hero will stand and fight that horde. → [Teach a skill] |
| Lesson | Battle HUD: first skill slot → Whirlwind → Level Up → Equip (the gate leads one step at a time) | Stopped | Press the hero's first skill slot. / This is Whirlwind. The hero spins and cuts down every foe around at once. Press it to take a look. / You have 1 skill point. Press 'Level Up' so the hero learns Whirlwind. / Now press 'Equip' to place it in the first slot. |
| Whirlwind | Time flows again; the hero sweeps the pack with Whirlwind | Running | Watch: this is the skill you taught. (bark) |
| End | Right after the last foe falls | Stopped | Did you see? The hero moved exactly as you taught. / This is the Hunt Edict. Whenever the road is blocked, rewrite the edict and open the way. / The road continues up there. I will keep watching over you until you reach the town. → [Head for the town] → graduation, town |

Stopped beats tint the screen edges a pale blue. The hero cannot fall in the stage (health refills at zero).

## Rules and implementation

- **First start:** an account where no hero has begun or finished a tutorial (`GuideTutorial.FirstStart`) enters as the warrior right after guest or Google sign-in, without the class choice, also after an interruption.
- **Accounts already in the puzzle levels** finish those; only new accounts take the new stage.
- **Graduation:** finishing segment 1 graduates straight to the town (one `guide-graduate` transaction, the existing prologue gold). The warrior's stage stands for every hero, so the other classes start in the town without a tutorial. Hunt Edict disclosure follows the existing rift-progress rules without the puzzle.
- **Arrival = tutorial finished (added 2026-10-11):** graduation counts as the end of the whole tutorial and opens every feature: all content unlocks (training ground to masterwork), every Hunt Edict option (`edictLegacyAccess`), repeat settings and the rune board (completed without its lesson, so the rift 15 first-clear rune block set lesson reward does not come), and tutorial hints off (the player can turn them back on). Flag `AccountGuide.everythingOpen`, owner `GuideTutorial.OpenEverything`.
- **Restart:** quitting mid-stage restarts it from the start of the road on the next entry (skills are cleared again).
- **HUD skill level-up (general feature):** while a skill point is free, the HUD skill slot list also shows unlearned skills, dimmed; tapping one opens its details, where 'Level Up' spends the point. It is general so the path the tutorial teaches keeps working in rifts.
- Main files: `GuideTutorial.cs` (map, beats, rules), `CombatSimulation.Guide.cs` (walk, spawns, beat changes), `GameStore.Guide.cs` (entry, graduation), `GameUI.GuideTutorial.cs` (lines, HUD lesson), `GameController.Tutorials.cs` (entry routing).

## Verification

- Edit Mode `GuideTutorialTests` (2): a new account starts at the left edge and goes through greeting, first fight, surround, Whirlwind lesson, clear and graduation; the road climbs up-right and is walkable. The related test batch is in the PR record.
- macOS development build `RuntimeGuideTutorialSmoke` (`-hellscriptGuideTutorialSmoke -hellscriptOfflineQa`): passed at 440×956 Korean and 956×440 English. A new guest enters as the warrior without the class choice; Velina's portrait and lines; four real pointer presses through the gate (skill slot, Whirlwind, Level Up, Equip); Whirlwind clears the pack; arrival in town with one graduation.
- Not verified: physical devices, human comprehension, long English lines at other resolutions, the old puzzle tutorial smoke (new accounts no longer go to the puzzle).
- To match the skipped class choice, five smokes that checked the character stage on a fresh save (title, character selection, equipment shop layout, town models, presentation profile) now set the tutorial exemption before guest sign-in.
