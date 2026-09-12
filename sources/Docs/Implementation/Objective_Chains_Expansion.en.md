# Essence carriers and offerings

Date: September 12, 2026 · Previous stage: [gate passage](Gate_Passage_Expansion.en.md)

## Implemented flow

Follow the objective table in the [dungeon composition specification](../Design/HELLSCRIPT_Dungeon_Composition_Detail.md): two seals, three essence carriers or three offerings. Newly generated rifts at tier 5 or above select one of these kinds. Tiers 1–4 retain the meter flow. Older suspended maps keep their saved configuration.

| Objective | Generation and automatic play | Completion |
|---|---|---|
| Essence carriers | Mark three existing elites in distinct passage/crossroad rooms. Approach visited objective rooms or the last observed positions, then follow the player's combat rules. | Settle all three assigned deaths to open the gate. |
| Offerings | Place guarded objects in three fighting rooms. Collect immediately after safely approaching the cleared guarding pack. Carry all three to an altar in the antechamber. | Deliver for one combat second to open the gate. |
| Meter alternative | Every objective retains the 100-meter alternative. | Opening by meter ends unfinished objective interactions and records progress at that moment. |

The objective draw is fixed by the entry seed and survives candidate retries and fixed fallback selection. New chain objectives limit wings to retain three passage/crossroad rooms. Their objective rooms plus the antechamber contain at least 100 meter in existing encounters; the accepted seal threshold remains 60.

The enemy budget remains 110 normal enemies and eight elites. Assigning carriers adds no enemies or attributes. Altar placement preserves doors, enemy spawns, chest and shrine access, and cursed-chest event spawn reservations.

## Persistence and presentation

- Settle carrier progress after death rewards, using one saved request per run and target.
- Save each pickup and altar delivery through idempotent requests. On write failure, stop in the same rift and support a retry.
- Danger interrupts delivery without clearing accumulated progress. Pause and map views do not advance it.
- Freeze objective type, counts, delivery state, opening cause and combat time when the gate opens. Results and run history retain that snapshot through the later boss fight.
- If a run ends before opening, record its progress at termination. Older maps show that opening time was not recorded rather than inventing it.
- Use discovered information and last observations for field/map markers, without reading hidden current carrier positions for presentation or pursuit.

Connect Korean and English progress to the battle HUD, map, result screen and previous run log. Primitive Unity models provide carrier markers, offering vessels, three altar lights and a progress ring driven by saved state.

## Concurrent work and validation ownership

Claude's separate attribute/balance worktree remains excluded. During this phase, another Codex task, “균열 단계별 등급 확률 추가”, began changing reward sources and UI in the shared project.

Subsequent validation uses an isolated copy whose 207 C# sources were reconciled with the previous source hashes. Only in that copy, exclude five new rarity-task sources and its hunks in shared files. Do not revert the live project's reward code, screens, documents or settings. The application in this report is an objective validation build; it does not represent a combined build with the stage-dependent rarity and reroll-cost changes. The separate rarity task is complete at closeout; validation of the combined changes is the next integration step.

Record the copy and file hashes in `Artifacts/Validation/ObjectiveChains/isolation.json` and `tested-source-manifest.json`. Distinguish this frozen validation snapshot from the mutable shared project.

## Validation

| Check | Result |
|---|---|
| Final isolated Editor suite | 1,287 passed; zero failures or skips |
| New objective coverage | Ten generation cases and 16 progress/persistence/history cases |
| Geometry | Both themes, 6/7/8 rooms, six fixed fallbacks × three objective kinds |
| Autonomous travel | Collect all three known offerings and deliver without teleporting the hero |
| Persistence exceptions | Failed pickup, delivery and carrier writes; retry, reload, replay and simultaneous hero death |
| 10,000 generated maps | All passed: 3,361 seals, 3,401 carriers and 3,238 offerings; 3,157 retries and zero fallback selections |
| Final native launch/restart scenarios | Five passed; 13 screenshots; Korean/English at 1920×1080, 1280×720, 720×1280 and 640×360 |
| Source and asset audit | All 207 sources unchanged after the full suite; 300 protected baseline files preserved; zero duplicate GUIDs or missing C# metas |

The sample consists of actual generated maps at tiers 5–30. Validate objective and combat access with gates closed, then navigation with gates open. This is distinct from the separate pure objective-draw test.

Use the [objective validation macOS application](../../Builds/macOS-ObjectiveChains/HELLSCRIPT.app). Preserve the [final validation summary](../../Artifacts/Validation/ObjectiveChains/validation-summary.json), [source manifest](../../Artifacts/Validation/ObjectiveChains/final-source-manifest.json), [archived Unity project](../../Artifacts/Validation/ObjectiveChains/validated-project.tar.gz) and [archive hash and verification](../../Artifacts/Validation/ObjectiveChains/validated-project.json). The archive contains the tested Assets, Packages, ProjectSettings and design documents; it excludes Library, temporary files and other worktrees.

Fixes include insufficient altar space on a fallback map, compact objective counters and the long English portrait-map subtitle. Existing shrine fixtures now force a seal objective so clearing every enemy does not also complete a carrier goal. Add protection for reserved event spawns during altar placement.

Preserve intermediate failed tests and earlier native attempts alongside final evidence. Physical mobile touch, sustained device performance/thermals and long-term difficulty/economy tuning are outside this audit.

## Main implementation files

- Objective data, generation and records: `Assets/HELLSCRIPT/Runtime/Core/RiftObjectives.cs`
- Automatic progress and idempotent persistence: `Assets/HELLSCRIPT/Runtime/Core/CombatSimulation.ObjectiveChains.cs`
- UI and temporary models: `Assets/HELLSCRIPT/Runtime/Presentation/GameUI.Objectives.cs`, `WorldView.ObjectiveChains.cs`
- Tests: `ObjectiveChainTests.cs`, `ObjectiveProgressTests.cs`, `RuntimeObjectiveChainSmoke.cs`

The next independent candidate is the remaining loot-goblin behavior, rewards and hunt-edict integration. Coordinate attribute screens and balance integration with their owning work after their current work is ready.

Visual review also found that the existing return-to-town result still offers death analysis. The subsequent [objectives and rewards integration](Rift_Integration_Expansion.en.md) restricts it to actual hero-death results and corrects English presentation of stored logs. That later app also includes stage-dependent rarity. The validation counts above remain the historical results of this objective-only snapshot.
