# Project Overview
- **Game Title**: HELLSCRIPT
- **High-Level Concept**: A dark fantasy rule-based auto-battler ARPG simulation where players author tactical behavior rules ("Hunt Edict") to conquer procedural rifts, optimize gear builds, and master deep combat mechanics.
- **Players**: Single-player simulation with asynchronous build sharing (Base64URL Edict sharing).
- **Inspiration / Reference Games**: Final Fantasy XII (Gambit System), Path of Exile, Grim Dawn, Loop Hero.
- **Tone / Art Direction**: Dark fantasy, gothic ink-and-parchment aesthetic with high-contrast UI accents (ink, gold, pale, muted).
- **Target Platform**: PC (macOS/Windows) & Mobile (Android/iOS).
- **Screen Orientation / Resolution**: Portrait & Landscape adaptive (Reference: 720x1280 portrait baseline, supports Safe Area & flexible scaling).
- **Render Pipeline**: Universal Render Pipeline (URP / PC_RPAsset).

---

# Game Mechanics

## Core Gameplay Loop
1. **Town Preparation & Build Setup**: In the Sanctuary, players configure equipment, allocate skill points, and author their tactical automation ("Hunt Edict v0.2" with 89 discrete options and rule-order matrices).
2. **Rift Infiltration & Simulation**: Enter procedural Rift dungeons. Combat executes via deterministic tick-based simulation where character actions are resolved through the author's Hunt Edict rules, reaction thresholds, aiming criteria, and survival priorities.
3. **Combat Observation & Failure Diagnostics**: Real-time battle visualizer displays HP/Resource changes, monster telegraphs, damage floating text, and failure diagnostics (identifying skill lockouts due to cooldowns, silence, or resource starvation in the 5 seconds preceding lethal damage).
4. **Looting & Progression**: Collect randomized equipment (24 base types, item affixes, 15 legendary uniques, set bonuses) and materials, expand shared storage, upgrade gear at the Blacksmith, and progress through higher Rift tiers.

## Controls and Input Methods
- **Pointer/Touch Interaction**: Full touch & mouse compatibility using Unity's New Input System (`InputSystemUIInputModule`).
- **Touch-Friendly Layouts**: Minimum touch target sizing (48x48 dp), swipeable tabs, scrollable edict rules with accordion foldouts, and quick copy/paste for Base64URL sharing strings.
- **Speed & Pause Controls**: Variable simulation speed toggles (1x, 2x, 4x) and instant pause/resume for tactical adjustments.

---

# UI

## Layout Architecture & Wireframes
```
+-------------------------------------------------------------+
|                      [HEADER BAR]                           |
|  Page Title (e.g. "사냥 칙령 v0.2")       [설정·안내] [공유]  |
+-------------------------------------------------------------+
|  [Tabs: 1.전역 생존/이동 | 2.스킬 행동 순서 | 3.스킬 세부 규칙] |
+-------------------------------------------------------------+
|  [CONTENT VIEW - Scrollable Accordion / 2-Column]           |
|  +-------------------------------------------------------+  |
|  | [▼] 슬롯 1: 스킬명 (자동 실행: ON/OFF)                   |  |
|  |     ├─ 우선순위 / 발동 조건 슬라이더                      |  |
|  |     ├─ 조준 모드: (가장 가까운 적 / 밀집 지역 / 보스)     |  |
|  |     └─ 생존/자원 조건: (HP 40% 이하 시 중단 등)          |  |
|  +-------------------------------------------------------+  |
|  | [▶] 슬롯 2: 스킬명 (접힘)                                |  |
|  +-------------------------------------------------------+  |
|  | [▶] 슬롯 3: 스킬명 (접힘)                                |  |
|  +-------------------------------------------------------+  |
|  | [▶] 슬롯 4: 스킬명 (접힘)                                |  |
|  +-------------------------------------------------------+  |
+-------------------------------------------------------------+
|                      [FOOTER BAR]                           |
|  [프리셋 불러오기]   [코드 내보내기/가져오기]   [저장 및 적용]  |
+-------------------------------------------------------------+
```
- **Mobile / Desktop Adaptive System**:
  - Desktop: 2-column view displaying skill slots on the left and expanded condition editors on the right.
  - Mobile: Accordion stack with collapsible cards, sticky action headers, and Safe Area offset padding.
- **Combat Failure HUD**:
  - Modal overlay on player death highlighting the last 5 seconds of decision history, showing root cause (e.g., "생존 스킬 쿨다운 1.2초 남음", "마나 부족으로 탈출기 미발동") with a direct link to edit the relevant Hunt Edict rule.

---

# Key Asset & Context

### Core Simulation & Edict Engine
- `Assets/HELLSCRIPT/Runtime/Core/HuntEdictV2.cs`: Core data structures (`HuntEdictV2Document`, `EdictCoreSkill`, `EdictOption`), canonical validation, and slot manipulation.
- `Assets/HELLSCRIPT/Runtime/Core/HuntEdictV2.Options.cs`: 89 discrete options and rule schemas across Warrior, Archer, and Mage.
- `Assets/HELLSCRIPT/Runtime/Core/HuntEdictV2.Codec.cs`: Base64URL serialization and deserialization for preset sharing.
- `Assets/HELLSCRIPT/Runtime/Core/CombatSimulation.cs` & Partial Classes:
  - `CombatSimulation.EdictAim.cs`: Target evaluation and positioning.
  - `CombatSimulation.EdictSurvival.cs`: Dynamic retreat, health gating, and hazard avoidance.
  - `CombatSimulation.EdictResponse.cs`: Event-driven reaction hooks (interrupts, crowd control breaks).
  - `CombatSimulation.Statistics.cs`: Telemetry gathering and tick metrics.

### Presentation & UI
- `Assets/HELLSCRIPT/Runtime/Presentation/GameUI.cs`: Main UI shell, safe area management, canvas scaler.
- `Assets/HELLSCRIPT/Runtime/Presentation/GameUI.Rules.cs`: Edict rule builder and condition picker.
- `Assets/HELLSCRIPT/Runtime/Presentation/GameUI.Presets.cs`: Preset import/export and preset slot storage.
- `Assets/HELLSCRIPT/Runtime/Presentation/GameController.cs`: Game lifecycle, simulation loop, persistent save hooks.

### Validation & Test Suites
- `Assets/HELLSCRIPT/Tests/Editor/HuntEdictV2Tests.cs`: Unit tests for schema validity, canonical normalization, and codec roundtripping.
- `Assets/HELLSCRIPT/Tests/Editor/CombatStatisticsTests.cs`: Combat verification metrics and damage calculation checks.
- `Assets/HELLSCRIPT/Editor/BuildIntegrationValidation.cs`: Automated build runner across archetypes and difficulty curves.

---

# Next Development Tasks & Priority Backlog

Based on the development history and verification logs, the development tasks are structured into the following prioritized milestones:

### Priority 0 (P0) - Hunt Edict v0.2 Complete UI & Simulation Integration
- **Objective**: Full rollout of the Hunt Edict v0.2 user interface, linking the 89 granular options directly to `CombatSimulation`'s tick loop and replacing legacy rule configurations cleanly.
- **Key Deliverables**:
  1. Complete interactive UI for Global Edicts, Skill Priority Ordering, and Skill-Specific Parameter Options.
  2. Integration of `EdictResponse`, `EdictAim`, and `EdictSurvival` runtime hooks into `CombatSimulation.Tick()`.
  3. Base64URL sharing dialog with validation, clipboard copy/paste, and error handling.

### Priority 1 (P1) - 6 Core Build Automated Balance Simulation Runner
- **Objective**: Execute large-scale simulation tests across 6 primary build archetypes (Warrior: Whirlwind / Shield Bash; Archer: Multishot / Snipe; Mage: Meteor / Blizzard) over 30 seeds each.
- **Key Deliverables**:
  1. Automated batch test runner in Editor measuring clear rates, average clear time, death causes, and resource starvation rates.
  2. Baseline balance tuning for Rift Tiers 1–30.

### Priority 2 (P2) - "Walking Sanctuary" Town Hub & 4 Core NPC Services
- **Objective**: Expand the Sanctuary scene into a functional hub with distinct NPC interaction points.
- **Key Deliverables**:
  1. **Blacksmith (대장장이)**: Item upgrade, prefix/suffix re-rolling, scrap recycling.
  2. **Skill Master / Reset (망각의 사제)**: Build respec and skill unlocking.
  3. **Mystery Merchant (비밀 상인)**: Rotating legendary shop and gamble vendor using rift shards.
  4. **Rift Keeper (균열 관리자)**: Rift tier selection, mutator modifiers, and endless trial mode.
  5. **Shared Storage (공유 보관함)**: Multi-tab stash for cross-character item transfers.

### Priority 3 (P3) - Combat Failure Diagnostics & 5-Second Pre-Death Ring Buffer
- **Objective**: Provide clear, actionable feedback to players upon defeat.
- **Key Deliverables**:
  1. Circular ring buffer in `CombatSimulation` recording the last 300 ticks (5 seconds at 60Hz) of combat events.
  2. Breakdown of incoming burst damage, status effects, and attempted actions blocked by cooldown/mana.
  3. Direct "Fix in Edict" button jumping from death screen to the exact rule causing failure.

### Priority 4 (P4) - Mobile & Responsive UI Optimization
- **Objective**: Enhance ergonomics and visual polish on touch/mobile displays.
- **Key Deliverables**:
  1. Dynamic text scaling support (100% to 140%).
  2. Safe Area compliance with notch/home bar insets.
  3. Touch-friendly hitboxes and virtual input optimizations.

### Priority 5 (P5) - Online Services & Gem Socket System
- **Objective**: Lay groundwork for online progression and item enhancement depth.
- **Key Deliverables**:
  1. Gem socket system (Ruby, Sapphire, Topaz, Emerald socketing in equipment).
  2. Account session tokens and cloud save timestamp synchronization.

---

# Implementation Steps

### Phase 1: Hunt Edict v0.2 UI & Simulation Wiring (P0)
1. **Implement Hunt Edict v0.2 Tabbed Interface** (`Assets/HELLSCRIPT/Runtime/Presentation/GameUI.Rules.cs`, `Assets/HELLSCRIPT/Runtime/Presentation/GameUI.Presets.cs`)
   - **Description**: Build 3 main tabs: Global Settings (Survival/Movement thresholds), Skill Order List (Drag/reorder priority), and Skill Details (Granular parameter sliders/toggles).
   - **Assigned role**: developer
   - **Dependencies**: None
   - **Parallelizable**: No

2. **Connect Edict Runtime Hooks in CombatSimulation** (`Assets/HELLSCRIPT/Runtime/Core/CombatSimulation.Rules.cs`, `Assets/HELLSCRIPT/Runtime/Core/CombatSimulation.EdictAim.cs`, `Assets/HELLSCRIPT/Runtime/Core/CombatSimulation.EdictSurvival.cs`)
   - **Description**: Ensure every tick evaluates the active `HuntEdictV2Document`, resolving skill targeting, retreat HP thresholds, hazard dodging, and crowd-control breaking according to v0.2 definitions.
   - **Assigned role**: developer
   - **Dependencies**: Step 1
   - **Parallelizable**: No

3. **Wire Base64URL Preset Sharing UI** (`Assets/HELLSCRIPT/Runtime/Presentation/GameUI.Presets.cs`, `Assets/HELLSCRIPT/Runtime/Presentation/HellscriptTextInput.cs`)
   - **Description**: Add export to clipboard and import text modal with instant validation and descriptive error toast notifications.
   - **Assigned role**: developer
   - **Dependencies**: Step 1
   - **Parallelizable**: Yes

### Phase 2: Combat Failure Diagnostics & Pre-Death Ring Buffer (P3)
4. **Implement Pre-Death Ring Buffer in CombatSimulation** (`Assets/HELLSCRIPT/Runtime/Core/CombatSimulation.Statistics.cs`, `Assets/HELLSCRIPT/Runtime/Core/CombatStatistics.cs`)
   - **Description**: Implement a circular buffer storing tick events (Damage taken, Damage dealt, Skill cast attempts, Block reasons: Cooldown/Resource/CC).
   - **Assigned role**: developer
   - **Dependencies**: Step 2
   - **Parallelizable**: Yes

5. **Create Defeat Analysis Summary UI** (`Assets/HELLSCRIPT/Runtime/Presentation/GameUI.cs`, `Assets/HELLSCRIPT/Runtime/Presentation/BattleHudLayout.cs`)
   - **Description**: Display death recap popup highlighting fatal damage burst and blocked skill actions with quick-navigation to Hunt Edict settings.
   - **Assigned role**: developer
   - **Dependencies**: Step 4
   - **Parallelizable**: No

### Phase 3: Automated Balance & Multi-Build Simulation Runner (P1)
6. **Construct Automated Balance Runner Test Suite** (`Assets/HELLSCRIPT/Tests/Editor/BuildIntegrationValidation.cs`)
   - **Description**: Write batch validation script simulating 30 randomized seeds per archetype across Tier 1, Tier 10, Tier 20, and Tier 30 to aggregate win rates, death ticks, and DPS curves.
   - **Assigned role**: developer
   - **Dependencies**: Step 2
   - **Parallelizable**: Yes

### Phase 4: Town Services & Shared Stash (P2)
7. **Expand Sanctuary UI & Town Interaction System** (`Assets/HELLSCRIPT/Runtime/Presentation/GameUI.cs`, `Assets/HELLSCRIPT/Runtime/Core/Economy.cs`)
   - **Description**: Create dedicated menu views for Blacksmith (upgrade/reroll), Mystery Merchant (gamble/shard shop), Skill Master, and Multi-tab Shared Stash.
   - **Assigned role**: developer
   - **Dependencies**: Step 1
   - **Parallelizable**: Yes

---

# Verification & Testing

### 1. Automated Unit Tests (EditMode)
- Run `HuntEdictV2Tests` to verify all 89 options, default values, canonical sanitization, and Base64URL serialization roundtrips.
- Run `CombatStatisticsTests` and `EdictResponseTests` to verify decision evaluation correctness under varied combat conditions.

### 2. Balance & Stability Verification
- Run `BuildIntegrationValidation` in Unity Editor:
  - Verify 6 archetypes achieve >= 70% clear rate on Tier 10 with matched level gear.
  - Assert zero `NullReferenceException` or out-of-bounds errors across 180 total simulated seeds.

### 3. UI & Ergonomics Manual Checks
- Verify Safe Area insets on mobile resolutions (e.g. 1080x2400 with notch simulation).
- Test accordion foldouts, slider dragging, preset importing, and instant preset application during gameplay.
- Confirm death screen correctly displays the pre-death timeline and blocked skill reasons.
