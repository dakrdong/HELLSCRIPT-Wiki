# Casual skin for the battle HUD and live graphs — Codex brief

> Historical implementation brief: this code survey describes published main `a3f96545`. The completed local UI/HUD and validation are owned by [the integration record](Casual_UI_Integration_20261014.en.md) and [the HUD handoff](Casual_Battle_HUD_20261014.en.md). Do not treat the old survey as missing implementation or restart generation.

Date: 2026-10-14 · [한국어](Casual_Battle_HUD_Codex_Prompt_20261014.md)

Paste the whole code block below into Codex. The mockup material is on the `claude/casual-battle-hud` branch, which is pushed.

| What to hand over | Where |
|---|---|
| Design canvas (12 boards, private) | https://claude.ai/artifact/PK5gEQdR1rrbW8Jzx3nDaA. Codex can open it only if the user shares it; the repository copy below is enough without it |
| Mockup source of truth (sources, 14 reference renders, layout numbers, copy table, seal and potion art, check tool) | `Docs/Design/GlobalHUD/Casual/` ([README](../Design/GlobalHUD/Casual/README.en.md)) |
| This brief | this document |

```text
Rebuild the HELLSCRIPT battle screen's main HUD and battle graph windows to match the casual StyleD mockup. The mockup source of truth is Docs/Design/GlobalHUD/Casual/ (README, 14 reference renders, mockup sources, seal and potion art, check tool). Combat, saves and graph maths stay as they are; this work changes the look, the layout and splits the graph windows.

## Background
- The game's art is moving from dark gothic to bright casual (StyleD, ages 12+). Swapping skins screen by screen, starting with representative screens, is approved; this is the battle HUD's turn.
- Use the same look as the other casual mockups (skill tree, training ground, battle position HUD and others): ink outline #2a3150, cream panels, yellow and orange accents, sky-blue resource, coral danger, Jua numbers and titles over Noto Sans KR body text, chunky drop shadows.
- The user set these directly in the mockup (README "Mockup notes" 1):
  - The Hunt Edict button sits top right, under menu and settings, in every orientation, and its tabs drop down below it.
  - The minimap moves to the lower right.
  - Status effects start above the left edge of the first potion; in portrait they sit above the slot row on the left.
  - Portrait graphs are small windows, and the portrait HUD is stacked.
  - Live DPS, monster damage, boss damage and activity time each move and fold on their own.
  - A folded activity window shows a text-free colour share bar next to its title.

## First
1. Rules: AGENTS.md, CLAUDE.md, AgentRules/ (Shared_UI, UI_Refresh, Workflow, CPU_Performance, Unity_MCP, Wiki_Publish, Artifact_Cleanup). Use one task and one agent. Conversation and development records are in Korean; code comments, commit messages and logs are in English.
2. Create a new worktree and the branch codex/casual-battle-hud from the latest origin/main (other sessions share the main checkout). Bring the mockup in with:
   git fetch origin claude/casual-battle-hud && git checkout origin/claude/casual-battle-hud -- Docs/Design/GlobalHUD/Casual Docs/Implementation/Casual_Battle_HUD_Codex_Prompt_20261014.md Docs/Implementation/Casual_Battle_HUD_Codex_Prompt_20261014.en.md
3. Read Docs/Design/GlobalHUD/Casual/README.en.md to the end and open all 14 files in reference/. That document and the CSS in canvas/Main.dc.html are authoritative for layout numbers, part states, graph rules and copy; what follows is a summary. node Docs/Design/GlobalHUD/Casual/tools/smoke.js checks the mockup rules and is the source for the rules to port into game tests.

## Current state (main a3f96545, code read directly)
- Main HUD:
  - Presentation/GlobalHudView.cs builds the HUD at runtime (shared by the preview and the game).
  - GlobalHudStyle in GlobalHudLayout.cs holds numbers and colours, read from Resources/Data/GlobalHudLayout.json. Coordinates are bottom-left based on a 1600×900 reference. Portrait fits the landscape composition into one row by width, so slots end up about 27 pt on a portrait phone.
  - GlobalHudSnapshot.cs builds slot states (ready, cooldown, not enough resource ◇, in use, locked, empty) and the effect list.
  - GameUI.GlobalHud.cs handles slot and effect descriptions and the observation menu.
- HUD art: Resources/Art/GlobalHUD/*.png (frame-seal, frame-vital, fill-vital, frame-active, frame-status, badge-*, status-*, mask-*, xp-*, control-*, menu-*, potion-hp/mp/utility).
  - The 63 hand-built vector pictures can be rebuilt with Docs/Design/GlobalHUD/Resources/v1/build-vectors.cjs (uses sharp).
  - GlobalHudView.Portrait(heroClass) picks the seal portrait: portrait-mage for the mage, SkillAtlas 18+class for the others.
  - Potion bottles come from PotionArt.IconId → Art/GlobalHUD/potion-*, with the bottle crop rectangles hard-coded in PotionArt. Crafted elixirs live in Art/Jeweler/Potions.
  - SkillIconView draws skill slots (frame-active, frame-seal).
- Hunt Edict button:
  - GlobalHudLayout.edict is the spot above the seal, and the "Hunt edict" button (menu-hunt-edict) is in GlobalHudView.
  - Tapping it runs GameUI.EdictQuick.ToggleEdictQuick, which opens quickRoot (sorting order 107). GameUI.EdictSheet.BuildEdictTabIcons lays the menu-edict-* icons of HuntEdictHudRows.Tabs (combat, survival, explore, repeat, presets) to the right of the button at 80% of its size; a tab opens OpenEdictSheet.
  - GameUI.EdictPosition.cs uses the same button as its anchor.
- Minimap: GameUI.Rift.ReflowRiftMinimap (riftMinimapPanel) places it at the top (top 88/122/158), left of the right-hand content dock column, following MapDisplay.Corner. GameUI.BattleLayout.ReflowBattleHud does the layout.
- Live DPS window:
  - GameUI.TrainingGround.BuildTrainingHud/PlaceTrainingHud build trainingHud. Its default spot is left of the map at top 124, width min(320, 30%); it folds, and its position is saved on the device through DpsHudDrag and game.DpsPosition.
  - The chart is TrainingChartView + TrainingDpsChart, with Current #b98230 and Previous #4a87bd. These are shared with the result windows TrainingDpsPanel and RiftCombatGraphWindow.
- Damage-taken graphs: in GameUI.DamageGraphs.cs the monster and boss graphs are children of trainingHud, so they hang under the DPS window and fold with it. The bar is full at 50% and changes colour at 20% and 40%. Data comes from DamageGraph.Monsters/Boss, boss seals from Art/Boss/seal-* and attack icons from Art/Boss/skill-*.
- Activity time:
  - GameUI.RiftActivity.cs builds activityHud; it folds and saves its position through DpsHudDrag and game.ActivityPosition.
  - RiftActivityPanel.cs is shared with the result window and uses UiTheme.Resource/Danger/PaidFatigueText/Success.
  - Positions are stored by Core/DpsHudPosition.cs (directory, file); there are two now (DPS, activity).
- Shared owners: UiTheme and UiFonts own colours, fonts and sizes; StorageSurface paints panels. main has no casual colour tokens and no Jua or Noto Sans KR fonts yet; on 2026-10-14 no other casual branch had changed UiTheme or UiFonts either.
- Related records: Docs/Implementation/Global_HUD_Potions, Damage_Graphs_20261010, Edict_Hud_Sheet_20261011, Rift_Activity_Time (each .md/.en.md), Docs/Design/GlobalHUD/*.
- Related tests: GlobalHudTests, GlobalHudResourceTests, BattleHudLayoutTests, DamageGraphTests, DpsHudTests, CombatActivityTests, HuntEdictHudRowsTests, PotionTests, PotionLoadoutTests.
- Related smokes: RuntimeGlobalHudSmoke, RuntimeBattleLayoutSmoke, RuntimeDamageGraphSmoke, RuntimeHudScaleSmoke, RuntimeTrainingGroundSmoke (.RiftDps, .Activity), RuntimeBattleQuickEditSmoke.

## Work (summary; numbers in the README)
1. Look owner: add the casual colours (ink, cream, yellow, orange, gold, sky, coral, mint, green, empty track, secondary text) as named values to the existing owner (UiTheme, or GlobalHudStyle/GlobalHudLayout.json when HUD-only). HUD and graph code use only those values and do not scatter colour literals.
   - Changing an existing shared UiTheme value affects other screens; if you must, report the reach.
   - Build the panel recipe (cream, ink border, inner dashed stitch, drop shadow) as a casual variant of a shared painter such as StorageSurface, or as one 9-slice sprite.
   - Pick one approach for text outlines and record why. Do not stack several uGUI Outline/Shadow components per label.
2. Rebuild the HUD art:
   - With a casual copy of build-vectors.cjs (for example Docs/Design/GlobalHUD/Resources/v2-casual/), regenerate frame-seal, frame-vital, fill-vital, frame-active, frame-status, badge-*, status-*, control-* and xp-* to the README recipes. Keep file names, sizes, 9-slice borders and .meta GUIDs.
   - Make menu-hunt-edict and menu-edict-* casual round buttons (white face, ink outline, drop shadow) with single-colour ink icons.
   - No hand-drawn raster art: use the vector generator or Codex image generation only.
3. Seal and potion art:
   - Seals: bring Docs/Design/GlobalHUD/Casual/assets/seal-{warrior,ranger,mage}-m.png into Resources and make Portrait(heroClass) use the new seal per class. Put the three seal-*-f files in the same folder without wiring them; saving a hero's gender belongs to the character create/select work and is not part of this task.
   - Potions: replace potion-hp/mp/utility.png in Art/GlobalHUD with the casual files, keeping their GUIDs, and re-measure the PotionArt crop rectangles for the new pictures.
   - Leave the SkillAtlas 18+class portraits if other screens use them.
   - Add a row to Editor/ResourceTextureBudget.cs for each new texture and report the size change. The originals stay in Git history.
4. Layout (GlobalHudLayout, GameUI.BattleLayout, ReflowRiftMinimap):
   - Move the Hunt Edict button top right, under the menu and settings buttons, in every orientation. Change BuildEdictTabIcons so the tabs drop down below the button as a vertical menu (cream frame, tab name under each icon). If this collides with the content dock column, put the button at the top of that column (under settings) and report the difference.
   - Leave the spot above the seal empty.
   - Move the minimap to the lower right, above the skill row.
   - Status effects: on PC and landscape they start above the left edge of the first potion; in portrait they sit above the slot row on the left. Eight expanded tiles must not overlap the minimap or graph windows.
   - Stack the portrait HUD from the top as status effects → slot row (spread to both edges) → seal, bars and potion settings → XP, instead of the one-row fit. The bottom layout on landscape and PC stays as it is.
5. Graph windows:
   - Detach the monster and boss graphs from trainingHud into windows of their own. Each window moves, folds and saves its position on the device (two more DpsHudPosition files; the save-failure toast reuses the existing copy).
   - Default positions stack under DPS in order. In a portrait boss fight the boss window opens on the left under the activity window.
   - Folding DPS folds only the DPS window.
   - A folded activity window shows the four-colour share bar beside its title, with no text.
   - Portrait uses small windows (widths 196 and 180, no DPS legend or stat cells); landscape hides the DPS legend. Windows use the casual panel.
   - Chart colours: this run is a solid orange #ff9f1c line over a 30% yellow area; the previous run is dashed sky #35a7ff; draw the end dot and round skill marks. TrainingDpsChart is shared with the result windows, so their charts take the same colours (which match the casual result mockups); report this.
   - Activity colours: movement #35a7ff, attacking #ff5a5f, dodging #ffd23f (line #d9a300), farming #4cbf55. The result window that shares RiftActivityPanel follows.
6. State visuals:
   - Under 30% HP, show a coral ring on the seal and on the HP bar. If the game already has a threshold for this, use it. The pulse is soft, on a 1.1 s cycle.
   - The shield is a mint strip along the top of the HP bar plus text above it (repaint the existing shieldLine and shield).
   - Slot states look as described in README "Parts and states".
7. Copy: reuse existing keys per the README copy table and add new keys in Korean and English together. A missing key falls back to Korean and is logged; never show an empty string or a key name.

## Decisions (go with these defaults and report)
- Decision 1 — Fonts: add no new font in this work. Use the existing UiFonts; if the skill-window casual skin work has landed Jua and Noto Sans KR on main, use those.
- Decision 2 — HUD size: the mockup is tuned for phone legibility (landscape slots 48 pt, portrait 42 pt). Keep GlobalHudLayout's scaling rules (1600×900 reference, 0.4 floor), but make the portrait stack produce slots close to the mockup size, and report the real sizes.
- Decision 3 — Content dock vs edict button: follow the default in step 4 above.
- Decision 4 — Result windows: only the colours of the shared parts (TrainingDpsChart, RiftActivityPanel) may change; result window layouts stay.

## Do not
Change combat, reward or save rules or the graph maths (DamageGraph, CombatFeedback); add hero gender or six-hero saves (the character create/select work); add a class; redraw skill icons, boss art, fields or monsters; bring back the removed text-size setting, its 50–150% checks or the enlarged-text smoke; change colours or layout of other screens; hand-draw art.

## Verification (only related checks while working)
- Related EditMode:
  - Run the related tests listed above.
  - Add layout tests: in all three orientations (portrait 440×956, landscape 956×440, PC 1600×900), status effects (collapsed and expanded), the slot row, the minimap, the open edict menu and the graph windows' default positions must not overlap each other and must stay inside the safe area.
  - Test save and load for the two new DpsHudPosition files, save failure, and restoring an off-screen position.
  - Run the fold states, LocalizationTests (new keys in both languages) and the ResourceTextureBudget check.
  - Port the rules in tools/smoke.js (full at 50%, 20%/40% colours, activity shares sum to 100, tab order, boss window only while a boss is alive) into EditMode.
- Smokes:
  - Update RuntimeGlobalHudSmoke, RuntimeBattleLayoutSmoke, RuntimeDamageGraphSmoke, RuntimeHudScaleSmoke, RuntimeTrainingGroundSmoke (RiftDps, Activity) and RuntimeBattleQuickEditSmoke for the new layout and the split windows.
  - The edict menu must drop down and open its tabs.
  - Move windows with a real mouse drag in a separate Mac player window, and leave proof that the saved positions come back after a restart.
- Screens:
  - Capture portrait 440×956, landscape 956×440 and PC 1600×900, 1280×720 and 1720×720 in Korean and English.
  - States: default, boss fight, danger, and folded with the edict menu and effects open. Compare each side by side with the same state in reference/.
  - Check safe areas, 44 pt touch targets, overlaps and clipped text. Phone aspect ratios come from Mac windows with simulated insets; do not report them as device results.
- Pass python3 tools/check_ui_contract.py --verify and follow the post-save UI refresh rule (AgentRules/UI_Refresh.md).
- Run the full EditMode suite and the macOS development-build runtime smoke once, after the last code change and the latest main merge. Separate failures that already existed from new ones. Documentation-only changes do not need game runs again.
- Performance:
  - Keep the current graph refresh rates (DPS and damage 1 s, activity 0.15 s) and never relayout every frame.
  - Claim performance only with at least three matched before/after runs of the real binaries with CPU and GC data; otherwise write "not measured".
  - Report texture memory changes as size numbers.

## Deliverables
- One PR (codex/casual-battle-hud). Do not merge it; merging waits for the user's approval. Include the mockup package (Docs/Design/GlobalHUD/Casual), and commit related checks and records together.
- Bilingual record Docs/Implementation/Casual_Battle_HUD_20261014(.en).md: what changed, decision results, where you departed from the mockup, check results, and what was not verified. Update the Global_HUD_Potions, Damage_Graphs and Rift_Activity_Time records too. Put evidence screenshots as jpg in Docs/Implementation/CasualBattleHudEvidence/ and link each file.
- If wiki sources change, follow AgentRules/Wiki_Publish.md; follow AgentRules/Artifact_Cleanup.md for cleanup.

## Report (in Korean, short)
Actual changes, the results of decisions 1–4, where and why you departed from the mockup, what you actually checked and what is unverified (devices, performance), size changes, and what is left. Do not report macOS results as device results.
```
