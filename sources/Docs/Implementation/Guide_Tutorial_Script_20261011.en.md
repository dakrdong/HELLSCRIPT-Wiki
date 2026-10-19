# Guide Tutorial Segments 1, 2 and 3

Updated: 2026-10-12

[한국어](Guide_Tutorial_Script_20261011.md). The first segment of the tutorial-only stage (flow 6) that replaces the puzzle-level tutorial: script, guide profile, implementation and verification.

## Opening revision — 2026-10-17

New starts use flow 7. Existing flow 6 checkpoints retain the original opening; the dated evidence below describes that historical flow.

| Beat | Encounter and actual player action |
| --- | --- |
| Ranged enemy | A skeleton archer appears. Velina explains that Piercing Hook makes distant foes easy to defeat. Invest one point through the shared tree, then equip the hook in the first HUD slot. |
| Frontal swarm | Five tiny, fast baby spiders with 1 HP rush from the front. At 50% hero HP, refund the hook point through the shared tree, invest it in Rend and equip Rend. |
| Level up | The first Rend bleed tick defeats the swarm. Reach level 2, receive one skill point and fully restore HP. |
| Surround | Walk for about two seconds, then five tiny spiders close from all directions. At 50% HP, Velina explains why a frontal attack is insufficient. |
| Modifier | Invest the new point in Spinning Rend. One 360-degree cast defeats the remaining swarm on its first bleed tick. |
| Lesson | Enemy weaknesses and efficient skill choices can decide victory even with identical equipment. Continue into the existing positioning, targeting, treasure, collection and boss lessons. |

Refunding during combat is allowed only for the owner's single Piercing Hook point in the authored frontal-swarm lesson. Normal combat and other skill refunds remain restricted. Reuse the existing tree, transactions, equipment UI, skill damage and save owners. Focus verification on changed tutorial, refund, growth and pointer paths; do not run full Edit Mode automatically.

## The guide

- Name: **Velina**, "Goddess of the Edict". The name comes from *velum*, a veil. The blindfold is only shown in the art; her lines never explain the setting, such as the blindfold or stopped time (user request, 2026-10-11).
- Voice: **polite**, and she speaks **to the player**, not to the hero. The hero is "the hero you are watching".
- Portrait: Codex candidate B (guiding with a light in her palm). `Resources/Art/NpcPortraits/guide-goddess(-mirror).png`; source, prompt and manifest in `Docs/Art/GuideGoddess/`.
- The developer never appears; she speaks only as "the goddess who shows this world's rules", to keep the fiction intact.

## Flow and lines

The map is four rooms (entrance hall, first arena, long gallery, plaza) joined by bent corridors, climbing up and to the right. Segment 1 uses the first two rooms. Segment 2 follows the upper door and winding corridor into the gallery for position-edict lessons. Segment 3 teaches target priorities, treasure pursuit and gathering in the plaza. See the [segment 2 record](Guide_Tutorial_Segment2_Plan_20261012.en.md).

| Beat | Situation | Time | Velina |
| --- | --- | --- | --- |
| Entrance | "The Edict's Road" card; the hero walks in from the left edge of the entrance hall | Running | — |
| Greeting | Stops after a few steps | Stopped | Welcome. I am glad you have come to this world. / I am Velina, the goddess who keeps this world's edicts. So that you do not lose your way here, I will show you this world's rules one at a time. / The road is not short, but I will not rush you. Please follow at your own pace. → [I will follow] |
| Road | The hero follows the road to the first arena and fights the first foe with basic attacks (no skills) | Running | — |
| First talk | Right after the first blows | Stopped | Did you see? The hero met the enemy and swung the sword unbidden. / The hero you are watching fights unbidden whenever an enemy appears. You need not order the sword drawn each time. / But how the hero fights is yours to decide. The hero will do exactly what you write in the Hunt Edict, to the letter. / For now there are only plain attacks. Please watch this fight to the end first. → [Watch the fight] |
| Surround | When the first foe falls, eight appear on all sides | Stopped | Look, every side is blocked. Plain attacks alone can hardly hold off this horde. / Now please teach the hero how to use a new skill. / With the skill you choose, the hero will stand and fight that horde. → [Teach a skill] |
| Lesson | Battle HUD: first skill slot → Whirlwind → Level Up → Equip (the gate leads one step at a time) | Stopped | Press the hero's first skill slot. / This is Whirlwind. The hero spins and cuts down every foe around at once. Press it to take a look. / You have 1 skill point. Press 'Level Up' so the hero learns Whirlwind. / Now press 'Equip' to place it in the first slot. |
| Whirlwind | Time flows again; the hero sweeps the pack with Whirlwind | Running | Watch: this is the skill you taught. (bark) |
| Segment 2 | Clear the surrounding pack → gallery road → first engagement lesson → walking approach → first Whirlwind hit → combat position lesson → remaining fight | Running/Stopped | [Segment 2 integration](Guide_Tutorial_Segment2_Plan_20261012.en.md) |
| Segment 3 | Plaza → grade/type lesson → boss → treasure lesson and chase → gathering lesson and pickup → final boss | Running/Stopped | [Segment 3 integration](Guide_Tutorial_Segment3_Plan_20261012.en.md) |
| End | After the plaza boss falls | Stopped | Did you see? The hero moved exactly as you taught. / This is the Hunt Edict. Whenever the road is blocked, rewrite the edict and open the way. / The road continues up there. I will keep watching over you until you reach the town. → [Head for the town] → graduation, town |

Stopped beats tint the screen edges a pale blue. Current flow 7 applies a positive health floor before damage can cause death; it does not refill health after zero. Shared level-up HP/MP recovery remains valid. Legacy flow 6 retains its separate recovery rule.

## Rules and implementation

- **First start:** an account where no hero has begun or finished a tutorial (`GuideTutorial.FirstStart`) enters as the warrior right after guest or Google sign-in, without the class choice, also after an interruption.
- **Accounts already in the puzzle levels** finish those; only new accounts take the new stage.
- **Graduation:** finishing all three segments graduates to the town (one `guide-graduate` transaction, the existing prologue gold). The warrior's stage stands for every hero, so the other classes start in the town without a tutorial. Segment 2 first discloses only the three position options; Segment 3 discloses target and action priorities at their lessons; graduation opens all remaining options.
- **Arrival = tutorial finished (added 2026-10-11):** graduation counts as the end of the whole tutorial and opens every feature: all content unlocks (training ground to masterwork), every Hunt Edict option (`edictLegacyAccess`), repeat settings and the rune board (completed without its lesson, so the rift 15 first-clear rune block set lesson reward does not come), and tutorial hints off (the player can turn them back on). Flag `AccountGuide.everythingOpen`, owner `GuideTutorial.OpenEverything`.
- **Restart:** quitting mid-stage restarts it from the start of the road on the next entry (skills are cleared again).
- **HUD skill level-up (general feature):** while a skill point is free, the HUD skill slot list also shows unlearned skills, dimmed; tapping one opens its details, where 'Level Up' spends the point. It is general so the path the tutorial teaches keeps working in rifts.
- Main files: `GuideTutorial.cs` (map, beats, rules), `CombatSimulation.Guide.cs` (walk, spawns, beat changes), `GameStore.Guide.cs` (entry, graduation), `GameUI.GuideTutorial.cs` (lines, HUD lesson), `GameController.Tutorials.cs` (entry routing).

## Segment 1 verification on 2026-10-11

- Edit Mode `GuideTutorialTests` (2): a new account starts at the left edge and goes through greeting, first fight, surround, Whirlwind lesson, clear and graduation; the road climbs up-right and is walkable. The related test batch is in the PR record.
- macOS development build `RuntimeGuideTutorialSmoke` (`-hellscriptGuideTutorialSmoke -hellscriptOfflineQa`): passed at 440×956 Korean and 956×440 English. A new guest enters as the warrior without the class choice; Velina's portrait and lines; four real pointer presses through the gate (skill slot, Whirlwind, Level Up, Equip); Whirlwind clears the pack; arrival in town with one graduation.
- Not verified: physical devices, human comprehension, long English lines at other resolutions, the old puzzle tutorial smoke (new accounts no longer go to the puzzle).
- To match the skipped class choice, five smokes that checked the character stage on a fresh save (title, character selection, equipment shop layout, town models, presentation profile) now set the tutorial exemption before guest sign-in.

For segments 2 and 3 and final integration on 2026-10-12, see the [segment 2 record](Guide_Tutorial_Segment2_Plan_20261012.en.md).

2026-10-17: 레벨업 HP·MP 완전 회복은 튜토리얼 전용 예외가 아닌 공통 성장 규칙이다. / Level-up fully restores HP and MP through the shared growth owner in all content.

### 2026-10-17 verification

The new opening and restored checkpoint passed focused simulation coverage. Existing related store/growth checks passed 22 cases; the UI contract passed 15 cases. After shared HP/MP restoration changed, 68 growth/new-opening cases passed. No full Edit Mode suite ran. Korean macOS Metal execution reached town through every new and existing lesson; its final stale assertion included uncreated heroes. After correcting that assertion to created heroes, the English 956×440 execution passed all 52 guided EventSystem presses, save reopening and town arrival. Each skill lesson target was checked at 440×956, 956×440, 1920×1080, 1920×1200 and 2520×1080. The later shared HP/MP change used focused growth checks rather than another whole journey. Physical devices, normal-speed feel and human comprehension remain unverified. Task evidence and the temporary app are retained under Builds/Validation/GuideSkillLessons20261017, review 2026-10-24; permanent deletion is not authorized.

2026-10-17 위키 게시 상태 / Wiki delivery: source committed; public generation blocked by the merged monster-art inventory change (`Public originals inventory changed; preserve and pin a new public snapshot`). No partial publication was made. Generated history was retained recoverably in the task evidence directory.


### 2026-10-17 follow-up: Hook, front swarm and Raking Wound

- The first Hook conversation begins about 8m away, outside the actual 6m range. The hero approaches only to that range. Only the base Hook in this lesson can pull across the full range, with 0.4s of visible chain, victim travel and casting/reeling motion. Ordinary 2m pulls and modifier numbers remain unchanged. Target health retains the existing calculation from real attack/skill damage; there is no hit-count death gate.
- Tiny foes then run in from about 8m ahead, one every 0.2s. Basic attacks continue while foes physically flank and surround the hero. Actual HP at or below 75% latches spawning off; actual HP at or below 50% opens the next lesson without setting or restoring HP. The live checkpoint preserves the timer and stop latch. Flow 6 and the later circular wave remain unchanged.
- Raking Wound (W09) reads the damage owner's radius and angle for its ground fill and gold boundary (base 3m/90 degrees), with windup, release slash/impact effects and a 0.3s aftermath. The single-target modifier does not show a false sector; the circular modifier shows a circle.
- Portrait opening masks now cover the title flight outside the selected frame while remaining below notices. Three actual 9:16 captures had zero exposed exterior pixels. An otherwise identical old-order control exposed 1,526,630 pixels; the interior was pixel-identical.

Verification: GuideTutorialTests 35/35, HookPresentationTests 5/5, aspect-mask cases 3/3, RakingWoundPresentationTests 6/6 and final UI contract 15/15 passed. The six W09 cases passed again after the visibility correction. Editor Play used a disposable Sandbox with actual combat, WorldView and the existing 18-bone ActorRig: Hook cast at 6m and pulled 5.144m. The front wave accumulated 19 foes, stopped spawning at 74.22% HP and opened the lesson at 49.21% with the same total of 19. Nearby foes occupied all four quadrants at both thresholds. Actual final PNGs and independent visual review confirmed the gold boundary and release effect.

Local evidence is retained in task-23 `tutorial-hook-presentation-20261017/` and `portrait-start-effects-20261017/`. Visual checks used the actual 1893x1351 Screen with a 9:16 viewport, without the full HUD or full GameController entry. The separate title-mask check and combat-render fixture are distinct. No real save access, new app build, full suite or physical-device validation occurred. The original scene, GameView selection, aspect and Editor mute were restored afterward.


### 2026-10-17 follow-up: guidance, survival, basic attacks and growth

- The earlier 0.2s cadence is superseded by 0.3s. Actual fixed-step observations were 0.30–0.35s: 16 foes accumulated, spawning stopped at 74.22% HP, and the lesson opened at 49.21% with the same count. Nearby foes occupied four quadrants. Common mitigation/dodge/shield calculations remain, while flow 7 records actual HP loss only down to a positive floor. No transition clamps HP back to half and no zero-health refill is used. Valid shared growth and skill healing remain.
- During Rend/Spin lessons, unavailable skills fall through to the existing BASIC planner. General combat already uses this fallback; an explicit BASIC automatic-OFF choice is preserved. Actual Play recorded BASIC damage with 3.2s of Rend cooldown remaining.
- Only the guidance pointer is rendered above the existing gate dim and below narration. Target highlight, clipping, scroll and input cutout stay intact. Actual KO/EN portrait skill windows confirmed the bright pointer and existing target/outside raycast boundary.
- Modifier narration first explains the linked main-skill prerequisite, one modifier choice per main skill, and changes to effects or attack form, before asking for Spinning Rend investment. The shipped English table has the same three new lines.
- Shared WorldView consumes an actual committed GrowthEvent once for a bounded gold ascent/ground effect, reusing existing textures and the existing pool. Restored history is seeded silently rather than replayed.

Valid focused evidence comprises Guide 46 (45 prior passes plus the corrected flow rerun), BASIC resource/cooldown 6 and explicit-OFF 3, PrologueGate 17 (including its typed-overload test rerun), existing WorldFx 12, and final GrowthPresentation 6: 90 relevant cases. Earlier failed assertions and the broad D4 request cancelled after no terminal result at 260s remain preserved and are not counted as passes. Exact inputs and pre/post hashes are retained in task-23 `tutorial-combat-feedback-20261017/`. The final UI contract also passed 15 cases.

Editor Play used Sandbox with actual CombatSimulation/WorldView and no real save access. Minimum natural pre-level HP was 19.13 and remained positive; a committed level 1→2 used valid shared growth recovery. Final level-up visibility was checked separately through an actual RAM XP transaction and 0.2/0.5/0.9s cosmetic samples. The initial weak capture is not final passing proof. Final gold motes rise distinctly from the selection ring without obscuring the hero. Actual Screen was 1893x1351; arrow checks used a logical 9:16 safe frame, while growth captures were world-only. Full HUD, full journey, physical devices, performance and another app build were not tested here. Original scene, GameView, aspect, Editor mute and protected files were preserved/restored. Public Wiki publication remains on hold.


### 2026-10-17 first Hook victim retreat and pursuit

Only the first flow-7 ranged victim, after an actual base-W08 pull and its 0.4s reel, retreats along the existing navigation path toward its original spawn at its existing speed. Arrival, a blocking wall or a step toward the hero returns it to ordinary ranged AI; there is no circular escape loop. It stops when cooldown is ready and holds during the next windup/reel so the unchanged 6m check can land again. Only this actual-pulled first victim gets the close-range automatic exception. Ordinary W08's >3m condition, modifiers, resource/cooldown/range and damage-derived health budget remain. No save field or hit-count death gate was added.

The final focused run passed 14 cases covering natural second Hook/basic damage and death, nine scope cases, saved restoration, two finite-home/crossed-hero cases and existing lesson integration. The four previously passed damage-budget profiles and maximum-pull/reel case retained their relevant method inputs. The initial circular-retreat failure and a test-only internal-constant access error are preserved and excluded from passing evidence.

Disposable actual Play: first pull 5.144m; victim retreat 4.920m and hero pursuit 3.400m; second pull 1.148m after 7.400s. Eight actual basic hits and two successful pulls led to natural death at simulation time 26.250s (first fight began at 11.900s). Maximum victim HP remained the original calculated 232. From that actual final position the following wave accumulated 16 foes in four nearby quadrants, stopped spawning at 73.83% HP and opened the lesson at 48.82%, with no death or transition heal. No new build, full suite or real-save access occurred; original Editor state was restored. Evidence: task-23 `tutorial-combat-feedback-20261017/hook-retreat-proposal/` and `visual/retreat-*-execution.json`.


### 2026-10-17 follow-up: stronger guidance, modifier reading, potion lesson and growth delivery

- Only these guided steps opt into a larger outlined pointer and brighter description emphasis. Existing targets, input cutouts, outside-click blocking and narration order remain. A 10px optical margin keeps the outline off description ascenders without widening input. Before investing in Spinning Rend, the player selects Blood Reclaim, Deep Wound and Spinning Rend in order, reads each effect description and presses the guidance message to acknowledge it. Reading does not invest or save; the subsequent actual Spinning Rend investment and save use the existing path.
- After Spinning Rend clears the surrounding foes, the potion lesson opens. A fresh flow-7 run starts with ten PH01 HP potions and a 1% automatic HP trigger; the first potion is not automatically consumed before this lesson is saved. The player opens the lower-left HUD potion settings, changes the HP trigger to 50% and saves. The existing real potion-use path consumes one potion, leaving nine, and restores full HP. A detached 50% draft, closing the panel or a failed save cannot consume, heal or advance. Success leaves the fully healed HUD visible for 1.2 seconds before showing the existing skill summary; its next Continue proceeds to segment 2.
- Existing phase IDs 0–33 remain; only PotionTalk is appended as 34. A valid live flow-7 checkpoint for the same hero is resumed. Older early checkpoints that have not passed this lesson receive one preparation: PH01 stock is raised to at least ten without reducing larger stocks. Progress, time, HP and combat state are preserved, and later reentry never repeats the supply grant or policy reset. An old phase-33 checkpoint is not rewound; it aliases the pending potion lesson. Only this compatibility case, which may retain HP above 50% after the retired free heal, uses the existing potion-use path with a 100% demonstration threshold while requiring the saved policy to remain 50%. Checkpoints already beyond the summary, flow 6 and ordinary 35% HP-potion recovery remain unchanged. This checkpoint-resume rule supersedes the earlier restart note above.
- Live GrowthEvent freshness now follows the actual foreground fixed-step budget, `ForegroundCombatClock.TickBudget × CombatSimulation.Step`, so a new event is not dropped after that bounded catch-up batch. The burst starts at the interpolated rendered hero, avoiding an ahead-of-character origin after catch-up. The existing gold ascent/ground presentation is reused and each new event is presented once. New Build, restored-history seeding, silent reveal and duplicate protection continue to suppress historical replays.

Final focused evidence is **167/167 passed**: PrologueGate 26, GrowthPresentation 10, GuideModifierLesson 4, GuidePotionLesson 14, GuideTutorial 59, Potion 19 and HuntEdictProgression 35. After the observed glyph and growth-position fixes, only affected checks were rerun; repeated cases are not counted twice.

Actual Editor Play used Sandbox accounts with the real Battle UI, CombatSimulation and WorldView. KO/EN raycast clicks and read confirmations covered all three modifier descriptions before spending the point, followed by portrait and landscape investment emphasis. An actual Spinning Rend fight cleared five ring enemies and reached phase 34. Real HUD-button, 50% slider and Apply inputs changed HP 1/530, stock ten and uses zero to HP 530/530, stock nine, uses one and saved policy 50%; the summary was not skipped. A final separate display sample showed HP 120 to 530/530 and ten to nine directly in the HUD before opening the summary. Initial clipped text, the ahead-of-actor burst, an immediately obscured recovery HUD and fixture failures are preserved and excluded from final visual acceptance.

The final growth sample used the actual FrontClear outcome, a committed level 1 to 2 event and the remaining nineteen fixed steps. At event age 0.950000 seconds it produced one effect. Gold ascent/ground light at the rendered hero, level 2 and restored HP/resource HUD were inspected. The physical Screen was 1893×1351 with 9:16 and 16:9 logical safe frames. The controller never ran Awake, login or real-save access: this is a RAM fixture, not a complete entry/journey, physical-device, performance or new-app-build claim. Scene, GameView, aspect and Editor mute were restored; four pre-existing dirty files were preserved. Final validation, captures and execution receipts are retained under task-23 `tutorial-guide-salience-20261017/`. Public Wiki publication remains held; the earlier 5fb1989a Wiki proof is distinct from this new source validation.


### 2026-10-18 treasure goblin, boss and action-order feedback

The tutorial treasure goblin now has 110 HP (was 55) and moves at 3m/s (was 1). On chase entry it travels a measured five metres and waits. A new damaging hit while waiting starts another burst of at most five metres; an old wound does not restart it. Walls and immobilization do not consume travelled distance. A corner goal cannot initially turn toward the hero. Remaining distance, observed HP and waiting state survive checkpoint restoration. Ordinary treasure-goblin tuning, escape timing and rewards remain unchanged.

The tutorial boss now has 600 HP (was 1800). During active treasure/loot lesson combat it continues following the hero and stops at its intended spacing. Paused narration and lesson damage protections remain. Old checkpoints containing the former literal values are converted once while preserving remaining HP fractions; dead enemies are not revived. Changed flight positions exposed an existing reward-edge regression: scatter now uses the hero pathfinding radius of .45m, preventing an otherwise unreachable reward at the plaza boundary.

Common action-order rows now match other order lists at 42u height with a 3u gap. Rank, name, description, running badge, arrows and grip share a horizontal line. Long translated descriptions use single-line ellipsis. Existing apply, preview, cancel, drag release and undo transactions are retained.

Verification: actual Unity compilation had zero errors/warnings; 69/69 focused tests passed (15 goblin/boss/edge cases, two existing graduation/reward-save-failure cases, 36 action-order, ten rule-order, one ordinary boss and five ordinary goblin cases). UI contract passed 15/15 and refresh passed 14 bindings/198 sources. The initially observed 4.95m early stop and unreachable-reward regression remain recorded as failures; their causes were corrected and only affected checks rerun. Repeated cases are not counted twice.

Actual Editor Play used the existing Library and a disposable RAM Sandbox with CombatSimulation, WorldView and Battle UI. First travel was 5.000m, waiting travel 0m, a fresh hit produced 1.500m travel in 0.5s, and boss spacing fell from 4.472 to 2.500m. KO/EN each covered five logical aspect ratios, seven compact rows and real pointer-event arrows, undo, preview/cancel and drag-release save. The physical Screen was 1893×1351; this is not a complete entry journey, physical-device, performance or new native-build claim. The owner controller never ran Awake/login or accessed real saves. Original scene, GameView and mute were restored, with pre-existing protected changes preserved. Evidence: task-23 `goblin-feedback-20261018/`.

Landscape logical safe-frame captures also showed upper title/nameplate clipping in the shared shell. Its geometry code is unchanged by this row patch; the cause and reproduction in a normal GameView remain unconfirmed. This is recorded separately from row-layout and input acceptance.

### 2026-10-18 modifier explanations, outside-tap continuation and 30% potion threshold

This section supersedes the generic highlighted-description narration, card-only continuation and exact 50% potion requirement recorded above on October 17.

Velina explains each modifier directly. Blood Reclaim detonates remaining bleed and heals on every third **cast of Rend**, not every third basic attack. Deep Wound trades multiple targets for one enemy and up to three increasingly strong bleed stacks. Spinning Rend trades bleed duration for a full-circle attack suited to the current encirclement. Combat tuning and skill definitions are unchanged.

Messages displaying “Tap to continue” accept both the card and any screen location. The existing PrologueGate blocker invokes the narration callback once and swallows the input, preserving underlying selections and unspent points. Mandatory selection, investment and save steps retain their specific input openings.

The potion instruction asks for at least 30% HP for safer play. Guidance, save validation and detached combat preview accept 30–100% and retain the chosen value. Saving the lesson still demonstrates one real potion through the existing use path and restores full HP. The one-time paused demonstration neither lowers current HP nor changes the saved threshold to 100%; ordinary automatic potion rules remain unchanged.

Actual Unity compilation had zero errors/warnings. All **92 focused Edit Mode cases passed**: PrologueGate 28, GuideModifierLesson 4, GuidePotionLesson 27, ordinary Potion 19, StoreViewBinding 13 and one existing full tutorial progression case. These include the 29/30% boundary, 30/31/40/50/75/100% acceptance, detached drafts, real save failure/retry, duplicate acknowledgement, old-checkpoint compatibility and ordinary potion recovery. UI-contract regressions passed 15/15; refresh passed 14 bindings/198 sources.

The real RAM Sandbox UI covered 30 explanations across KO/EN and five logical aspect ratios, checking wrapping, safe bounds, visible effect descriptions, real EventSystem selection, outside and duplicate taps, and no point spending. A portrait representative then read all three explanations, cleared the ring with Spinning Rend, blocked Apply at 29%, dragged the handle to 30% and saved through the real Apply control. HP 1/530, stock ten and uses zero became HP 530/530, stock nine, uses one and saved policy 30%.

No full test suite, new native build, physical mobile or performance measurement ran. Physical Screen was 1893×1351; aspect tests used logical safe frames. No real user save was accessed. Original scene, GameView, aspect and mute were restored; the four pre-existing dirty files retain their original SHA. Evidence and classified fixture-input failures are under task-23 `tutorial-reading-20261018/`. Public Wiki publication remains held for the existing cost check.

## 2026-10-19 rework link

The flow from the plaza on (ground-zone and arrow dodge lessons, level-up fireworks, the new boss room and the Whirlwind lesson) and the corrected introduction and road lines are recorded in the [guide tutorial rework](Guide_Tutorial_Hazard_Yard_20261019.en.md). The line tables above predate that rework.
