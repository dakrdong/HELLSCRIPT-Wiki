# Area attack target-cap audit and fixes (2026-10-21)

[한국어](Area_Attack_Targets_20261021.md)

## Boundary

Worktree `.claude/worktrees/aoe-all-targets-20261021`, branch `codex/aoe-all-targets-20261021`. The final candidate starts directly from fetched main `a4b40e4661333611ee4a28251a959873893f3756`. Initial implementation used Claude handoff `907c8bf82bffd970943321aafaeb25d5f779ee95`, but after the user's commit/review/merge authorization, only the area-fix commit was moved onto main. The 55-file unmerged town/forge/UI change is excluded. Include just W09's one-line area-cap removal separately; new area tests cover its base, reclaim, spin and single-target variants. GuideTutorialTests matches main; Claude's original fix/tests remain intact in the author worktree.

The single owner of this area PR is Codex `01a15c22-3696-720e-b18b-9cc46004b021`. A read-only board check found no claims, running commands or pending writes. The existing UI/tutorial integration role and records remain unchanged. Shared main, both Claude worktrees, user saves and the user-test project are not edited. Commit, push, PR creation, official review and a merge commit after all gates pass are now authorized. Actual external outcomes are recorded in this task's Builds/Validation/AreaTargets20261021/integration-status.json and PR.

Additional user authorization extends the original 23 files by one SVG explanation comment in CasualNoticeInfo.cs. Spell out the path coordinates instead of using commands mistaken for retired skill IDs; coordinates, lengths, stroke width and executable code remain unchanged. Reactivate only the required Class skill data and Shared UI contract workflows, without changing their definitions, permissions, authentication or security settings.

The user subsequently requested that Codex run all CI locally by default and avoid paid GitHub features. Record this policy and the separate remote-protection boundary in AGENTS.md, AgentRules/Workflow.md/.en.md and the bilingual AgentRules/Agent_Collaboration.md. After reporting scope and impact, disable the two reactivated automatic workflows to avoid further paid runs. Preserve check definitions, protection, permissions, billing and spending limits.

## Classification and changes

| Path | Result |
| --- | --- |
| W18 normal/expanded impact | Every living enemy in the circle takes damage and stun |
| W02-2/W03-3/W04-3 aftershocks, W04-2 wave | Remove five-victim caps; W02-1 charge-path selection also loses its arbitrary 100-candidate cap |
| W05 stance/reflection, W17 copied areas/counters/reflection | Remove circle/cone/copied-area caps; ordinary ancestors still select one target |
| A04-1 decoy, A14-2 blast, A18-2 afterimage, A06-1 kill explosion | All enemies in each blast; ordinary A14 forks keep their count |
| A17 normal/moving/focused rain | All enemies per pulse; wave counts and focused radius remain unchanged |
| M01 fireball/meteor/initial splitting blast and piercing-fireball trail | All enemies in blast/path areas; retain split projectile count and pierce limits |
| M04 departure blast/arrival stun/shield reactions, M06-1 second nova | Uncap area damage/control/burn/root; no new arrival damage |
| M08 normal/ring/moving/cross | Uncap all ticks, preserving rectangles/circle/cross and line of sight |
| M11 normal/rolling/triple orb ticks and all expiry blasts; M17 collapse | Remove 3/5/8-victim limits; retain orb instance counts and element/stage order |
| WP20, MP20, MP21, M18 elemental bursts, DES_LM42 | Uncap passive and equipment area effects |
| REF_SW06_B5, REF_SM05_B5, REF_SM08_B6, REF_SA08_B6 | Uncapped shared Splash |
| REF_SA06_B5 | Remove Nearby's hidden default cap from damage, stun and recorded victims |
| 18 legendary area damage/control powers | Remove LegendaryTargets' five-victim cap; preserve visibility, distance/ID order and radius-zero single-target behavior |

Nearby and Line default to all matching enemies. Explicit projectile/selection counts still apply. Splash no longer accepts a max parameter. Existing geometry, dead-enemy and wall filters remain in effect.

## Designed limits and forecast alignment

- Keep W09-2 single-target, W08-2 pierce 3→7, and A07-2 scatter 3→5, as specified by their modifier rows.
- Keep A01 five targets per arrow plus rune targets, its three independent fan arrows, A02 scatter/barrage arrow counts, normal A14 two forks (three with the poison equipment condition), M03 chain/rebound counts, M07 three pierces, Returning Frost five per leg, small split-fireball five pierces, LA04 two transfers, and projectile.maxHits used by rune hit selection.
- M01-3 previously selected up to five new victims every tick while its forecast allowed only five total. It now keeps five per flight and shares collision footprint/order with forecasting. Its ground burn trail remains uncapped.
- D4 A01 no longer inherits a legacy passive's seven-target forecast while actually hitting five per arrow. Non-D4 passive behavior remains unchanged. M07 policy counting now observes the same three-target limit as damage. W04 forecast and impact share the uncapped wave geometry.

## Data and bilingual text

Remove obsolete area targets parameters from the authored current_runtime_parameters.json and generated ClassSkills.json for W09, W09-1, W05-1, W02-1, W04-1/2, W18, A04-1, A03-1/2, M08, M11, M17 and M18. M03-3 now pulls an already selected chain route, so remove the unused five-target parameter inherited from its retired ground effect. Preserve single-target/projectile/fork values and historical LegacySkillWire.json.

Update Korean/English descriptions for W09 equipment, the frost combination, the retreat afterimage set and 18 legendary powers. Refresh the bilingual legendary catalogue and combat specification. Fix the catalogue loader's missing sys.modules registration so its dynamic wiki import can regenerate/check this catalogue. Actual separate wiki validation/publication outcomes are recorded in the PR and task-owned verification evidence.

## Unresolved and preserved

The residual TickClassEffect kind="shape" uses five pierces for previous A01, otherwise a five-target 60-degree shape. No current source producer was found. Whether the latter is a scatter-shot echo or a pure area cone lacks an active design contract, so preserve this branch pending owner clarification. The currently generated REF_SA08_B6 uses a fixed-position burst instead and is uncapped.

M01-3's five-target value is retained from existing forecast/hit code. The specification describes piercing but does not define a ranked target-count row for it. Confirm separately if a different numeric design was intended; it was not turned into unlimited piercing.

## Actual validation and limits

- New area/boundary/base/modifier/designed-limit cases: **79 passed, 0 failed**. Most fixtures include ten interior enemies plus one boundary enemy, an enemy just outside and a dead enemy; wall exclusion is also tested. One case iterates all 18 area legendary powers.
- Existing directly affected skill cases: **44 passed, 0 failed**, covering W09, hook, counter/reflection, mage area families, chain, scatter, shot forecasts, decoy, rain and afterimages.
- Python current-catalog regression **5/5**, D4 data regression **3/3**; generated ClassSkills/D4 data and legendary catalogue checks pass. Numeric preservation tests exempt only retired area target caps, still comparing all other original defaults and the historical wire domain.
- All 79+44 C# and 5+3 Python cases passed again on the main-based candidate without the town predecessor. The required legacy-ID check's 18 regression cases also passed. Its actual inventory check now **passes** after the user-authorized one-line SVG comment correction (raw=421, actual-use=0, verification-contract=22, retirement-record=399). Neither the checker policy nor UI executable code changed. The follow-up changes only the comment and records, so reuse the unchanged Core validation inputs; verify official review and required remote CI against the new final SHA in the PR.
- C# tests compile the **actual Core sources** with .NET SDK 10.0.401, C# 9 and installed Unity NUnit assertions. Test-only substitutes implement Unity math, Resources, JsonUtility and profiler markers. This is not Unity Editor compilation/Console, Unity EditMode or player proof.
- Actual Unity 6000.6.0f1 EditMode ran only 55 explicitly selected affected methods: **123 passed, 0 failed, 0 skipped** (79 new + 44 existing cases; 133.36s). Use the installed Unity CLI, dedicated development worktree and task-owned save path. The user confirmed no other Unity test/build before the single run; the previously denied ps inventory was never retried or bypassed. No C# compiler errors occurred, but initial import logged one Unity Data Store allocator error, Failed to free block, retained as a limitation. Archive and restore only five tracked changes generated by this run; both ProjectSettings files remained unchanged. No full EditMode, PlayMode, device/performance checks or access to protected saves/user-test files.

## Local CI correspondence

| Existing workflow | Local equivalent | Evidence |
| --- | --- | --- |
| Class skill data | D4/ClassSkills generated checks, 3+5 Python cases, 18 legacy-ID regression cases and real inventory check, git diff --exit-code | Commands, exit codes, logs and target SHA in task-owned CI records |
| Shared UI contract | 15 checks through tools/check_ui_contract.py --verify --receipt ... | Task-owned receipt and execution log |

The initial eight local steps passed on macOS/Python 3.9.6/Node 26.8.2. Validation matching the defined runtime versions uses existing Python 3.12.13 and task-local Node 22.23.3. Verify Node against the official SHA256 manifest without changing global installation/settings. macOS execution does not prove the Ubuntu runner environment. Record the final SHA, actual execution/reuse outcomes and logs under Builds/Validation/AreaTargets20261021/ and in the PR. GitHub runs prevented from starting by billing/spending limits remain failed; report any actual remote protection gate separately.

Input SHA256 hashes and exact validation boundaries: worktree Builds/Validation/AreaTargets20261021/verification.json. Main-based C# reports: headless/isolated-area.json and headless/isolated-existing.json. Initial Claude-based area-final.json and existing-regressions.json remain as history. Merge only after the final candidate's affected Unity checks, official review and required CI pass. Full EditMode still needs explicit separate authorization.

## Changed files

- `Assets/HELLSCRIPT/Resources/Data/ClassSkills.json`
- `Assets/HELLSCRIPT/Resources/Data/LegendaryPowers.json`
- `Assets/HELLSCRIPT/Runtime/Core/CombatSimulation.ClassSkillEffects.cs`
- `Assets/HELLSCRIPT/Runtime/Core/CombatSimulation.ClassSkillPassives.cs`
- `Assets/HELLSCRIPT/Runtime/Core/CombatSimulation.ClassSkillPolicies.cs`
- `Assets/HELLSCRIPT/Runtime/Core/CombatSimulation.ClassSkills.cs`
- `Assets/HELLSCRIPT/Runtime/Core/CombatSimulation.Forecast.cs`
- `Assets/HELLSCRIPT/Runtime/Core/CombatSimulation.Legendaries.cs`
- `Assets/HELLSCRIPT/Runtime/Core/SkillTreeD4/CombatSimulation.D4Mage.cs`
- `Assets/HELLSCRIPT/Runtime/Core/SkillTreeD4/CombatSimulation.D4Ranger.cs`
- `Assets/HELLSCRIPT/Runtime/Core/SkillTreeD4/CombatSimulation.D4Warrior.cs`
- `Assets/HELLSCRIPT/Runtime/Presentation/CasualNoticeInfo.cs` (one authorized comment line)
- `AGENTS.md`
- `AgentRules/Workflow.md`
- `AgentRules/Workflow.en.md`
- `AgentRules/Agent_Collaboration.md`
- `Docs/Design/ClassSkills/current_runtime_parameters.json`
- `Docs/Design/D4SkillTree/skill_tree_d4.json`
- `Docs/Design/HELLSCRIPT_Legendary_Class_Expansion.en.md`
- `Docs/Design/HELLSCRIPT_Legendary_Class_Expansion.md`
- `Docs/Implementation/D4_Skill_Tree_Combat_Spec_20261012.en.md`
- `Docs/Implementation/D4_Skill_Tree_Combat_Spec_20261012.md`
- `tools/legendary_catalog.py`
- `tools/test_class_skill_runtime.py`
- `Assets/HELLSCRIPT/Tests/Editor/D4/SkillTreeD4CombatCoreTests.AreaTargets.cs`
- `Assets/HELLSCRIPT/Tests/Editor/D4/SkillTreeD4CombatCoreTests.AreaTargets.cs.meta`

This record and its Korean counterpart are also new. No package, scene, prefab or project setting changed.
