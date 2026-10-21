# Casual monsters rebuilt (9) · 2026-10-17

[한국어](MonsterRebuild_20261017.md)

Eight shipped monsters (B01 N03 N07 N08 N09 N10 N19 N25) and the art-pending N23 now use casual StyleD models and portraits. Models and portraits of the same ID were overwritten with `.meta` GUIDs kept; N23 was released with `art-ready`. No code or save-key change.

| ID | Candidate | Note |
|---|---|---|
| N19 | 3 | |
| B01 | 2 | |
| N03 | 3 | |
| N07 | 2 | candidate 3 dropped: pieces torn from the body |
| N08 | 0 | |
| N09 | 0 | |
| N10 | 1 | |
| N23 | 2 | |
| N25 | 0 | |

**Deferred (regenerate next day):** all four candidates of N18, N06 and B02 failed the rig verdict or the windup/walk pose review (long hair, wide hat brim, long hems and the candle ring tore or flew off). The `monster_specs.json` prompts were rewritten to hug the body.

**Checks:** HunyuanMonsterReadyTests 39/39, monster, codex, hunt-edict and rift related EditMode 269/269, `check_ui_contract.py --verify` 15 tests passed, `monster_registry.py validate` passed. The full EditMode was not run, by user instruction.

**Models smoke:** `-hellscriptMonsterModelsSmoke` fails on the N28, N31, N33, N35 lineup ("3 model bodies, expected at least 4"). Those were not changed today, and the same failure happens on main before this change. The five N01–N25 lineup screens before it were captured.

- [N01–N05](MonsterModelsEvidence20261017/lineup-N01-N05.jpg) · [N06–N10](MonsterModelsEvidence20261017/lineup-N06-N10.jpg) · [N11–N15](MonsterModelsEvidence20261017/lineup-N11-N15.jpg) · [N16–N20](MonsterModelsEvidence20261017/lineup-N16-N20.jpg) · [N21–N25](MonsterModelsEvidence20261017/lineup-N21-N25.jpg)
- [Old and new portraits compared](MonsterModelsEvidence20261017/portraits-old-vs-new.jpg)

Not verified: physical devices, performance, build size, wiki publishing.

## Game integration completion · 2026-10-17

N28 is a burrowing ambusher with a 4 m reveal radius; the model smoke paused its lineup at 5 m. Its correctly hidden body was counted as missing. Only lineup fixtures now set `brain.revealed=true`, and the count requires active model bodies. Normal gameplay and combat fixtures retain ambush behavior.

- Focused EditMode: 156/156 passed (HunyuanMonsterReadyTests, MonsterPrimitiveTests, TownModelTests, TownEnvironmentTests); UI contract 15/15 and registry validation passed. No full EditMode run.
- Final macOS development player: READY bodies 34/34, N28/N31/N33/N35 4/4, attacks landed for nine data monsters and B06. B06 correctly retains its primitive pending model installation. [Final result](MonsterModelsEvidence20261017/runtime-monster-models-smoke-final.txt).
- The first incremental player failed before model checks due to missing URP shaders. A clean build of the same source passed the runtime checks; focused EditMode was not repeated.
- Verified all nine replacement model/portrait mappings and artPending flags. No pending downloads or new generation were attempted.
- Player, logs, captures, resource audit and retention manifest: `Builds/Validation/ModelIntegration20261017/`. Wiki publication and existing scratchpad/worktree deletion remain outside this task.

Town PR #141 (`ce06385e`) was fast-forwarded into the shared checkout during the first test run. The final player includes it. Only changed town coverage was repeated: 27/27 passed. Unchanged monster inputs retain the earlier results; final effective coverage is 132 monster cases and 27 town cases.
