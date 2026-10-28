# Casual UI follow-up handoff

> **2026-10-28 A2 correction:** References to the large edict window, ≡ menu and their dedicated components or smokes are historical. Use the [current entry points](../../Assets/HELLSCRIPT/Runtime/Presentation/GameUI.HuntEdict.cs) and [replacement screens and source owners](Hunt_Edict_Window_Removal_20261028.en.md). Earlier captures and test counts describe the earlier implementation.

2026-10-14 · [Korean](Casual_UI_Parallel_Handoff_20261014.md) · [Canonical style rules](Casual_UI_Skin_Rules_20261014.md)

The user authorized 13 other UI tasks in separate threads. Parent thread `01a10416-f8fa-77cc-b19d-070c6bc290c5` owns assignments, the validation queue and integration. This skill pilot remains the sole editor of common style documentation, theme and rendering code. Each screen starts with its actual design and source investigation. Shared edits and Unity execution require coordination with the parent.

## Recovered shared HUD API freeze — task23

Read-only terminal and file checks confirm recovery on the same Mac. The actual starting HEAD is `b87b581b249c5e682cd7832fa9190b24c0dd9c4e` on `codex/casual-ui-validation-20261014`; all nine uncommitted common files were preserved. The thirteen UI screens and character PR2 with41 PNGs are already integrated. Task23 is now the sole common integration owner of the existing `task-6/skill-skin` checkout. No repository, Library or player build was copied. This current status supersedes pending statements in earlier sections below.

The accompanying commit freezes `SkillIconView.SetCasualHudFrame(ultimate:false,status:false)`, `PotionArt.Bottle(id,casualHud:true)`, `TrainingDpsChart.SetCasualHudAppearance(Color? current=null)` and `RiftActivityPanel.SetCasualHudAppearance()`. Skill/ultimate/status retain frame-skill+circle/frame-seal+circle/frame-status+square through repaint. Potions retain the full512 canvas and separate cache without alpha cropping; frame-active and66% slot placement belong to HUD. Texture budget v11 adds the Casual HUD512/GPU-only/no-mip contract with Standalone BC7, Android ASTC4x4 and WebGL ETC2.

The HUD prompt lines89–101, decision4, also requests shared casual result colors. DPS now uses `#FF9F1C/#35A7FF`; activity shares/areas use `#35A7FF/#FF5A5F/#FFD23F/#4CBF55` and lines use `#2F8FD8/#E8474C/#D9A300/#36A845`. Result layout, mesh, alpha, dashes and endpoint stay unchanged. HUD grid, width and endpoint remain opt-in. Caller colors, skin removal and legacy behavior are preserved; percentages on bright shares use Navy. Four English rows were added: Live DPS, Average, Peak and `{0:0}s`.

One static validation pass compiled all855 Runtime/53 Editor/270 Tests sources with Roslyn:0 errors,90/28/43 warnings,0 diagnostics in changed files. UI contract15/15, refresh16 bindings/199 files and diff checks passed. Combined compile/contract/refresh/diff execution31.69s; failures/retries0. Independent read-only frame/potion and result-chart reviews found no further defects. **Unity/EditMode/game/native build runs0; actual user save access0.** New tests were compiled only; HUD art is not yet integrated. Evidence, input hashes, response files, logs and lifecycle manifest are in `task-23/validation/hud-shared-recovery/` (12,100,357B, retain through final acceptance; review2026-11-14). Same-volume free space225GiB. No full suite or player regeneration was performed.

The HUD owner should locally fetch/FF this commit, compile against the real APIs and deliver a frozen SHA. The initialization hook and both IdleDashboard `Portrait(growth.heroClass,growth.gender)` calls remain HUD-owned. Its `GameUI.EdictSheet` also owns the top-anchor/portrait Position-Target-Order layout adapter, without modifying those protected screen implementations. **Do not integrate the intermediate dirty HUD tree or start Unity.** After frozen HUD handoff, run one final full suite and reuse one native build on the same integration branch; distinguish passed, failed and unexecuted checks. No remote push, main merge, web build or individual build.

## Current shared compatibility and base HUD ownership

Save PR1 `e3809624` and followup `93219e3f` are locally merged over the thirteen-screen integration. The shared change accompanying this document consumes the actual HeroSlots API. The D4 store host prefers the selected created hero, then a created same-class fallback. Optional `ISkillTreeD4HeroAvailability.HasHero(string)` preserves all three tabs for reference hosts. Uncreated tabs reject input and hide point dots. Display keys include selection, creation, identity, gender and all six levels. A held investment/refund click is cancelled after an owner change even across explicit repaint; successful local transactions retain effects. `GameController.SelectHero` now guards creation and uses the real index instead of clamping to0..2.

Texture budget v10 covers CharacterPlaza backgrounds2048×2, cheer/fight1024×12, crests256×3 and other layers512×24. All41 source PNG hashes/dimensions/GUIDs and GPU-only/mip-off metadata matched. Assets have not been copied into this integration; the new import test awaits PR2. Static Runtime/Editor/Tests compile has0 errors,89/28/43 warnings and no warnings in changed D4/budget files. UI contract15/15, refresh16/199 and diff checks pass. Unity/EditMode/game execution remains0. Evidence: `Builds/Validation/CasualSkillSkin/dotnet/hero-slots-followup/`.

The user added base HUD as screen15, owner `01a139a9-e169-7597-9efe-5ef6e32ce0f6`. **Final Unity/build/visual validation is held until both character plaza and base HUD frozen handoffs are integrated.** Consume `HeroSave.heroClass/gender/created` and `HeroSlots.Index/Created/IsCreated`.

Task22 owns GlobalHudView/Layout/Snapshot, StatusStripView, GameUI.GlobalHud, dedicated HUD data/vector/art/tests and a dedicated GameController.BattleHud partial. Connect its preference helper through existing initialization; do not add Awake/Start. Task22 may supply minimal HUD-only patches to GameUI.DamageGraphs/RiftActivity/BattleLayout/Rift, TrainingGround HUD creation/collapse, EdictSheet.BuildEdictTabIcons and ContentDock layout for integration review. Task6 retains shared charts/activity panel, SkillIconView/PotionArt, theme/fonts, texture budget/importers and shared localization integration. Preserve task8 Position/Target/Order, CasualEdictSkinScope and world distance indicators, plus task11 PotionQuick code/art.

Separate opt-in `Art/GlobalHUD/Casual/frame-*` preserves existing frames; a shared API must retain this choice through SkillIconView.SetSkill/SetKind/SetShape. Do not traverse generated images to replace them. Consume new potion art within the HUD scope; global PotionArt replacement/cropping needs a separate shared patch. Preserve crafted-elixir paths and audit the IdleDashboard consumer of GlobalHudView.Portrait.

The six512×512 seal derivatives (2,170,953B total) are in the HUD worktree's `Docs/Design/GlobalHUD/Casual/assets/seal-{warrior,ranger,mage}-{m,f}.png`. Their original `hero-{class}-{gender}-v01.png` deliveries in `Documents/Codex/2026-10-13/task-4/output/hellscript-main-characters-6/delivery/` match the protected index, QA and DIRECTOR_STATUS pass/hash records. Use the design assets.json crops. Task22 supplies them under Resources/Art/GlobalHUD/Casual; they are distinct from CharacterPlaza's three crests.

The character owner additionally owns created-hero settings lists, the gender-aware HuntEdictWindow.Progression fallback and GameUI.cs's3-column×2-row roster. Preserve GameUI.Items inventory-integrity iteration over uncreated starter equipment. PR2 production account policy/UI/art/localization is still pending. Older fourteen-screen statements below are superseded by the fifteen-screen hold above.

## Fourteenth screen: character creation and selection

Owner: `01a13993-e900-7602-91c1-50fbcdf2d0f2`. The integrated source basis is **`8d356b3c3bc2e7950302c47bf52445777808a85c`**, branch `codex/casual-ui-validation-20261014`, in `task-6/skill-skin`. It contains the thirteen owners and common `0ccba80e`. UI contract15/15, refresh16 bindings/199 files and diff checks passed; Unity execution is unverified. Fetch locally into the new owner's isolated workspace; do not edit or check out the shared project.

The supplied prompt is absent from the original checkout but exists at **`/Users/t8g-2410-pn-005/Documents/MyProject/HELLSCRIPT/wt-casual-art/Docs/Implementation/Casual_Character_Plaza_Codex_Prompt_20261014.md`**. The integration owner read the complete prompt and its subsequent changes. Its tree contains the canonical design/art directories. Updated source **`13a1ce7322ae69f5b518503b52598fbadf7ee5b8` confirms user-approved optionA: six saved heroes**. The earlier pending-decision wording no longer applies. Implement save/migration and UI in two local commit stages.

The character owner owns `GameUI.CharacterSelection.cs`, `CharacterSelectionView.cs`, `CharacterSelectionStage.cs` and dedicated `CharacterPlaza*` source/resources/tests/docs. It also exclusively owns optionA changes to Models, GameStore, Tutorials, GuideTutorial, PuzzleTutorial, selection state/controller, uncreated-hero account iteration filters and their regressions. The integrated13 diff from common0cc to8d356b3c contains no Runtime/Core or Models changes. Supply the exact impact list; task6 will not edit save owners in parallel. Shared theme/fonts/window/equipment views, `CharacterFigure` and `ResourceTextureBudget.cs` remain task6-owned. Request precise optional APIs, budget rows and missing glyphs. Supply additive English localization rows for conflict-aware integration.

Consume `UiSkin.Casual.WithEquipmentArt(CasualEquipmentArt.Provider)` and the existing optional `skin:` window/slot/detail APIs. The dedicated popup owns its eight-slot grid and passes each hero's actual `EquipmentSlots.At` item to shared slots. `CharacterEquipmentView.Create` still renders the old `CharacterFigure`; do not use it for new portrait artwork or replace its generated image afterward. Keep equipment359, monster portraits35 and new character gender/pose art as distinct domains. Own one dedicated character scene/pose provider; request a task6-owned shared hero portrait API only for explicit cross-screen consumers. Reuse shared Jua/Noto weights, contrast tokens, chrome and clipping; verify added glyphs such as male/female signs against bundled cmaps.

Final Unity tests/builds are **held until the fourteenth screen is handed off and integrated**. The thirteen-screen CLI attempt exited before Unity started because its editor path needed an `.app` bundle. The path is corrected without retry: import/compile/test/build execution0. Preserve the45-filter plan as13-screen preparation. OptionA now spans save schema, tutorials and account iteration, so combine the final full regression once after the new impact list and source are integrated. Native visual coverage retains the agreed two viewport sizes in KO/EN. No individual-owner Unity/build, main merge, remote push or web deployment.

Six-slot followup: **task6 owns** replacing `StoreSkillTreeD4Host`'s `Single(class)` and gating D4 class tabs on created heroes. After the owner supplies actual `HeroGender`, `HeroSave.created`, `HeroSlots` and schema23 source, prefer the currently selected same-class hero; otherwise choose a created same-class hero. Disable unavailable-class tabs and preserve preview/live-run/save ownership. Cover both genders, missing created class, live-run restrictions and the actual saved hero in focused tests. Do not guess an unavailable API into shared code.

The character owner may exclusively edit the **created filters and class-to-hero index assumptions** in `GameUI.Items.cs`, `GameUI.ScreenSettings.cs` and `HuntEdictWindow.Progression.cs`. Those three files have zero diff from common0cc to integration8d. This does not authorize style or shared-API changes. Supply exact HeroSlots signatures and the prerequisite save commit.

## Pinned basis and completion

- Verified game-code basis: **`4139a77e2df495cee009c07c6094b2eea9b1aeaa`**, local branch `codex/casual-skill-skin`.
- Worktree: `/Users/t8g-2410-pn-005/Documents/Codex/2026-10-14/task-6/skill-skin`. This handoff changes documentation only.
- Skill implementation, all five requested visual repairs, focused validation, captures and common rules are complete. **Full acceptance of every internal size is not complete.** The widths below still require a separate decision.
- Reuse the [pilot report](Casual_Skill_Window_Skin_20261014.en.md) and [final runtime receipt](CasualSkillSkinEvidence20261014/target-native-receipt.json) as skill evidence, never as validation of new screens.
- Do not merge `main` or other branches independently. The parent coordinates basis adoption and source-only worktrees. No new remote push, PR, GitHub art sharing, web build or public deployment authorization is implied.

## Shared files and edit boundaries

Paths are relative to the repository. `P` means `Assets/HELLSCRIPT/Runtime/Presentation`; `R` means `Assets/HELLSCRIPT/Resources`. Screen owners may call shared components but may not edit them.

| Shared owner files | Reuse and limits |
|---|---|
| `P/UiTheme.cs` | Also contains `UiFonts`. Reuse `CasualDisplay`, `CasualBody(bool)`, `CasualWeight(int)` and existing `D4*` colors. Do not replace global `Body/Display/WebBody` or the legacy color ramp. |
| `R/Fonts/CasualSkill/`, metadata and OFL licenses | Reuse bundled Jua and Noto400/700/900. No per-screen generation, subsetting, destruction or weight duplication. |
| `P/SkillTreeD4/SkillTreeD4Looks.cs`, `SkillTreeD4PanelLooks.cs`, `R/Data/SkillTreeD4Looks.json` | Read existing looks/tokens. The catalog validates exactly **430** entries; `SkillTreeD4Look` maps skill states. Request new roles/keys/call sites from the common owner instead of adding keys or weakening the count check. |
| `P/SkillTreeD4/SkillTreeD4Graphics.cs`, `SkillTreeD4PaintGraphic.cs`, `SkillTreeD4TextShadow.cs` | Reuse rounded geometry, rings/arcs, circular paints, gauges and a single text effect. Preserve casing-before-core lines, inset gauges and rounded clipping. `UiMesh` is also partial in `P/UiButtonFace.cs`. |
| `P/ContentWindowView.cs`, `ContentWindowHost.cs`, `UiWindowFrame.cs`, `UiSafeArea.cs`, `UiButton.cs`, `UiButtonFace.cs`, `UiIconButton.cs`, `StorageSurface.cs` | Reuse shell, safe area, modal/back behavior, input shielding and button lifecycle. Do not clone the skill Canvas/Fit into ordinary windows. Investigate and coordinate a casual opt-in for the current shell; do not assume it already skins every window. |
| `P/ItemDetailView*.cs`, `ItemDetailPopup.cs`, `EquipmentComparisonView.cs`, `EquipmentSlotView.cs`, `CharacterEquipmentView.cs`, `SkillIconView.cs`, `CurrencyIconView.cs` | Reuse equipment/skill/currency semantics and explicit data sources. Shared appearance changes remain owned centrally. |
| `P/StoreViewBinding.cs`, `Assets/HELLSCRIPT/Runtime/Core/Localization.cs` | Share successful-save refresh, visible keys and KO/EN. Batch shared refresh/text additions through their owner. |
| `P/SkillTreeD4/SkillTreeD4Wrap.cs`, `SkillTreeD4Units.cs`, `P/SkillTreeD4Window*.cs`, `RuntimeSkillTreeD4Smoke*.cs` | Skill-specific ownership. Its 2× design boundary, Fit and reserved wrap height are not defaults for other screens. Available measurement functions do not authorize automatic width expansion. |
| `Docs/Design/D4SkillTree/`, `tools/build_skill_tree_d4*.py`, `R/Data/SkillTreeD4*.json`, `R/SkillTreeD4Tone.shader`, `Assets/HELLSCRIPT/Tests/Editor/D4/` | Skill design/golden/generation/validation bundle. Do not overwrite generated data, semantic goldens or tests for another screen. |
| Common rules, `AgentRules/`, shared contracts, UI contract/refresh tools, `Packages/`, `ProjectSettings/`, shared asmdefs/scenes | Read-only to screen owners. Do not relax owner lists/checks or modify packages/settings to make a screen pass. |

Public API visibility does not imply edit ownership. Respect each screen's unit boundary: skill design points use 2 uGUI units; ordinary windows use `UiTheme.Scale`. A new common adapter is one small centrally reviewed, implemented and validated change; the parent distributes its exact SHA. Broad refactoring is not a prerequisite.

## Local screen files

The parent owns the exact 13-screen/design/thread assignment list. This is a current-source lookup, not approval of every row or an invented 13-screen list. Edit only registered files matching the assigned screen's prefix. Do not assign the same partial class to competing owners.

| Screen family | Registrable local files under `P/` |
|---|---|
| Inventory / storage | `InventoryWindow*.cs` / `StorageWindow*.cs` |
| Equipment shop / blacksmith | `EquipmentShopWindow*.cs` / `BlacksmithWindow*.cs` |
| Rune board / rune master / jeweler / aspect | `RuneBoardWindow*.cs` / `RuneMasterWindow*.cs` / `JewelerWindow.cs` / `AspectStoneWindow.cs` |
| Rift entry / results / records | `RiftEntryWindow*.cs`, `RiftVictoryWindow*.cs`, `RiftRewardRevealWindow.cs`, `RiftReviewDetailWindow.cs`, `RiftGearSettingsWindow.cs`, `CombatRecordsWindow.cs`, each with its own assignment |
| Hunt edict | Assigned presentation files in `HuntEdictWindow*.cs`; coordinate overlapping skill files/shared entry points first. |
| Training / codex | `TrainingGroundWindow*.cs` and `TrainingGroundResultWindow.cs` / `MonsterCodexWindow*.cs` |
| Attendance / daily quests / rewards / offline supplies | `AttendanceWindow.cs` / `DailyQuestWindow.cs` / `RewardBoxesWindow.cs` / `OfflineSuppliesWindow.cs`. Parent checks the attendance pilot first; never overwrite its work. |
| Puzzles / dialogue / other | Confirmed owners such as `PuzzleHubWindow.cs`, `PuzzleMaintenanceWindow.cs`, `NpcDialogueWindow.cs`, `StoryDialogueWindow.cs`, `TutorialJournalWindow.cs`, only when assigned. |
| HUD / battle quick popups | `GameUI` partials, `GlobalHudView` and layouts span screens. Parent must assign exact files/method boundaries; do not claim the entire family locally. |

Screen-specific adapters, designs, focused tests and bilingual records may be added locally. Preserve existing rules, prices, calculations, transactions, saves and schema. `Runtime/Core/GameStore*.cs` and model/save owners are outside the skin edit allowlist. Local tests do not authorize changing shared criteria or other screens' tests.

## Lightweight edits and one Unity queue

1. Investigate actual design states/tabs/sizes/input and current HEAD/dirty state, owners, behavior and shared dependencies. Opening a page alone is not inspection. Do not execute Unity or edit shared files during discovery.
2. Make small changes only in independent registered files. Creating 13 threads does not imply 13 concurrent implementation jobs. Unless separately directed, parent schedules at most **three** independent tasks per `AGENTS/Workflow`. Source-only worktrees do not copy or cross-symlink `Library/Temp/Obj/Builds` or existing caches.
3. Submit a pending validation request with screen/thread, branch/full SHA, dirty diff/asset hashes, common basis SHA, test filter, design states/KOEN/viewports, disposable save, output path and retention. Do not start Unity before parent assigns the slot.
4. Parent chooses the validation project. Keep the current skill `Library` and `Builds/Validation/CasualSkillSkin/HELLSCRIPT.app` as documented existing candidates. Parent decides whether to reuse them or select a source-only validation project; do not checkout/reset an active skill worktree or overwrite its app independently. Do not create 13 caches/apps. Unity import/compilation/EditMode/build/player jobs on this Mac run **one at a time**.
5. Record start/end, PID/job ID, actual compiled inputs, XML/JSON/logs, captures, binary hashes and artifact lifetime. Release the slot only after confirmed completion. Do not duplicate a stalled job or evade it with a fresh cache; record failure causes and changed inputs before focused retries.
6. Parent reviews owners and determines integration order. Reuse proof only for matching inputs. Coordinate full EditMode and final Mac smoke once on final integrated inputs, instead of repeating full suites per screen.

Use disposable saves and silent audio. Validate KO/EN, portrait/landscape, PC 16:9/16:10/21:9 and relevant existing input, save failure, modal return and scroll paths. Mac simulation is not physical-device certification. Preserve original art/metadata, attendance `6378320b`, and all three uncommitted art handoff documents. Web build cancellation and remote push limits remain. Queue waiting, implementation completion and validation completion are separate states.

## Additional asset authorization and ownership

The additional user instruction on 2026-10-14 authorizes producing genuinely missing new-concept images for assigned screens and recoverably retiring unused old resources after replacement, visual validation and full reference checks. The [canonical asset procedure](Casual_UI_Skin_Rules_20261014.en.md#image-reuse-creation-replacement-and-retirement) owns the details. Search the new-art index and latest approved per-ID SHA first. Report shared-asset needs as `ID/consuming screens/current candidates/reason insufficient` to the parent for a single production owner. Preserve `.meta/GUID` for same-path replacement. Record generation, integration, validation, retirement and recovery separately. Preserve resources referenced by other screens/active tasks, dynamic Resources loading or GUIDs. Permanent deletion/emptying Trash requires separate approval from the parent for exact paths and sizes. Artwork authorization does not expand shared-code ownership, Unity execution slots, paid services, account connections or remote publication rights.

## Separate acceptance decision: internal widths

Approved body400/emphasis700–900 and original font sizes remain. These **design-point** values compare baseline `53d6a3e1` widths against actual Canvas-scale measurement in `4139a77e`. Observations include repeated tested states, not independent widgets. The changes are not approved new fixed design dimensions. This handoff reads the [existing audit](CasualSkillSkinEvidence20261014/geometry-native-audit.json), with no new execution. Reversion fit is inferred from the recorded measured glyph widths and original slots, not a newly executed reverted player. Current stat-label widths are calculated by subtracting the wider value slot from the unchanged total row width.

| Target | Exact before → current | Targets and reverting |
|---|---|---|
| Portrait Multishot/Killing Rain stat values, KO/EN | `91.500→91.537` (+0.037); label `290.500→290.463` | Four unique records; previous slots still report fits=true. |
| Portrait Chain Lightning value, KO/EN | `95.500→98.318` (+2.818); label `286.500→283.682` | Two unique records; reverting reports fits=false. |
| Landscape Chain Lightning value, KO/EN | `84.500→86.382` (+1.882); label `157.500→155.618` | Two unique records; reverting reports fits=false. Stats total **30/1272 observations**, eight unique changed records. |
| English modifier ranks, portrait | `16.325→16.386` (+0.061); each name/effect slot `304.675→304.614` | 77 unique states; current Noto700 exceeds the old slot, fits=false. |
| English modifier ranks, landscape unselected | `15.500→15.886` (+0.386); each name/effect slot `173.500→173.114` | 77 unique states; reverting reports fits=false. |
| English modifier ranks, landscape selected | `29.500→29.886` (+0.386); each name/effect slot `157.500→157.114` | Seven unique states; the original padded slot still fits=true. Ranks total **620/1944 observations**, 161 unique states across 84 names. |

| English linked name | PC: name / entire chip | Portrait: name / entire chip | Landscape2868: name / entire chip |
|---|---|---|---|
| Ground Slam | `67.540→69.840` / `138.865→141.165` | `73.680→75.716` / `145.005→147.041` | `67.540→70.164` / `138.865→141.489` |
| Raking Wound | `73.700→77.760` / `145.025→149.085` | `80.400→83.627` / `151.725→154.952` | `73.700→77.776` / `145.025→149.101` |

Linked chips change in **6/36 observations**, across two names; all six original name slots report fits=false with current Noto700. Height28, level-slot17.325, rows and positions remain unchanged (0/36 row/position changes). Fixed non-text width is54; the earlier diagnostic52 was incorrect and is not used here.

**The strict size-preservation criterion remains unmet for these three types of expanded internal slots and their width allocation, including the six wider English chip rectangles.** Source CSS dimensions/font sizes compare equal across36 combinations/49,942 strings; outer windows/nodes/hit/icon/layout/behavior contracts are preserved. That does not establish every child's size equality. Separate actual clipping on reversion from still-fitting micro-adjustments when deciding acceptance. Do not propagate width expansion as a settled rule to the other13 UI. Browser pixel equality and physical-device accessibility remain unverified, not evidence authorizing size changes.


## Opt-in shared API — first integration contract

This is the common owner's minimum implementation, subject to queued Unity acceptance. Every new `skin` argument defaults to `null`, retaining legacy visuals. Global theme values, gameplay, saves, transactions and comparison calculations are unchanged. Screen owners consume these APIs without editing the common files.

```csharp
var skin = UiSkin.Casual;
ContentWindowView.Open(parent, title, source, reading, Render, skin: skin);
EquipmentSlotView.Create(parent, item, x, y, size, skin: skin);
ItemDetailView.Append(parent, item, width, source: source, skin: skin);
EquipmentComparisonView.Create(host, hero, item, runes, null, null,
    source: source, sideBySide: true, skin: skin);
```

- Trailing `UiSkin skin=null`: `ContentWindowView.Open`, `EquipmentSlotView.Create/Paint/Caption/Icon`, `ItemDetailView.Create/Append/AppendCatalog/CreateDefinition`, `EquipmentComparisonView.Create`. Pass `view.Skin` explicitly to children. Read-only view `Skin` survives repaint and reaches comparison cards/tabs. Existing equipped-left/candidate-right columns, `sideBySide`, and mobile panes retain their behavior.
- Buttons: `button.SetChrome(UiButtonChrome.Casual)` or `skin.ApplyChrome(button)`. Chrome survives `Configure`, `UiTheme.Button/Choice`, hover, press and disable. It does not alter labels. Explicitly use `skin.ApplyLabel(text, display:true, semantic: optionalColor)` for one label without resizing it. Never recursively replace every semantic text color. `UiButtonChrome.CasualClose` / `ApplyChrome(button, close:true)` provides coral rounded close chrome while preserving the existing `Quiet` role, input and audio. Google branding and Item/Icon art/rarity surfaces remain separate.
- Panels: `StorageSurface.SetSkin(skin, border: optionalColor, radius:12, borderWidth:2, shadow:3)` and `UiWindowFrame.SetSkin(skin)`. Subsequent legacy `Paint` calls do not clear the scope. `SetSkin(null)` restores the original paint/options. Values are the current Canvas units, not the skill pilot's special 2× units. Face, border and lower shadow stay within the hit rectangle without drawing an opaque shadow beneath the entire translucent face.
- Graphics alone do not clip children. Create edge-reaching content beneath `skin.Clip(panel, radius:12, inset:2)`. It retains content coordinates/bounds and clips the inner rounded silhouette. Shared item details already do this; do not add redundant per-node masks.
- Public tokens: `Cream`, `CreamShade`, `Sky`, `Navy`, `Gold`, `GoldShade`, `Green`, `GreenShade`, `Muted`, `Edge`, `Disabled`, `DisabledEdge`, `Coral`, `White`. They read the generated pilot palette without adding non-skill IDs to its 430-look catalogue.
- On-light semantic ink: `Success`, `Danger`, `Rarity(item)`, `OnLight(color)` darken semantic colors toward Navy, targeting calculated 4.5:1 for opaque text on `CreamShade`. This is not a claim of full accessibility or contrast for every decoration, white close glyph, fade or physical device. Greater-affix/rarity/delta distinctions, arrows and wording remain. `RichText` converts only known gain/loss tags; `ItemTooltip` source values remain unchanged.
- Fonts: `BodyFont`, `DisplayFont`, `LabelFont(text)`. Labels containing a glyph absent from Jua use Noto400. Explicit `font` arguments win; pass `null` to choose the skin default. Generated shell Button/Tab labels follow this policy. `×` exists in Noto400/700/900 but not Jua. U+FF0B `＋` is absent from all four subsets; U+002B `+` exists in all. A screen may replace only the translated display result `＋`→`+`, keeping its localization lookup key. Do not independently patch the shared fonts.

### Artwork supply and outstanding input

`new EquipmentArtProvider(approvalReceipt, itemResources, setEmblemResources:null)` snapshots catalogue/base IDs to extensionless Resources paths. Pass it only through `UiSkin.Casual.WithEquipmentArt(provider)`. Shared `EquipmentArt` retains catalogue lookup, B28→B32 and pre-imprint identity. Resolution is new unique → existing unique → new base → existing base/atlas; partial maps must not replace a valid unique illustration with a generic base. Set emblems use mapped art then existing fallback.

The receipt string itself does not verify approval or SHA. One import owner must supply the reviewed per-ID approval, SHA256, actual Resources path and GUID mapping. **This API change imports no new equipment PNGs and grants no asset approval.** `UiSkin.Casual` without a provider retains original artwork. Screens must not traverse generated RawImages to replace art or duplicate ID lookup logic.

Known delivery outside the 650-entry index: tree `57bba034dad1659e8a36dc9645b1291d2554e497` has 35 `Resources/Art/TrainingGround/Portraits/{ID}.png` portraits, 29 normal and 6 bosses. Read-only checks matched **35/35 SHA256 values, 63,338,362 logical bytes**, against `Docs/Art/Monsters/Casual20261014/validation/boss-background-assets.json`; the latest boss revision has lavender backgrounds. Distinguish `pixel-review-accepted` from manifest `productionApproved:false`; do not infer model provenance/release approval. Inspect these before regenerating portraits. Preserve the later `MonsterCodexWindow.cs`, `MonsterCodexWindowTests.cs`, and `MonsterCodexPortraitEvidence.cs` changes that are absent from this pilot basis; the parent controls adoption. They were neither overwritten nor automatically merged here.

### Ownership and proposed Unity queue

- `EquipmentShopWindow*` is one shared family for normal/gamble shops. `.Settings.cs/OpenAutoSettings` is also consumed by inventory salvage. One writer needs common-owner coordination; it is not an independent screen file. A concrete settings diff still needs review.
- Combat shared files: `GameUI.EdictSheet.cs/.EdictPosition.cs/.EdictQuick.cs`, `WorldView.EdictForecast.cs/.EdictTarget.cs/.EdictOrder.cs`. This minimum API covers buttons/panels only. Prediction, timers, camera and drag commit behavior are unchanged. World appearance hooks remain unimplemented pending verified state colors/art IDs.
- Training/rift-result shared ownership: `RiftSkillShareView`, `TrainingDpsChart`, `TrainingChartView`, `TrainingDpsPanel`, `RiftActivityPanel`, `RiftCombatGraphWindow`, `GameUI.TrainingGround.cs/.DamageGraphs.cs/.RiftActivity.cs`. Chart/combat-card skin APIs are not included in this first commit.
- Proposed queue: parent integrates the common commit → related `UiButtonTests`, `CasualUiSkinTests`, `SharedUiTests`, `EquipmentArtTests`, `ItemComparisonTests`, `ItemTooltipSettingsTests`, `StoreViewBindingTests` → **rift entry `118371c7` first slot** (latest parent assignment) → daily quest (task-20 queue request) → shop/inventory → rift recommendations → remaining parent-assigned screens. Repeat affected checks after new shared imports/code, not the whole project by default. Each screen still needs KO/EN, portrait/landscape/PC ratios and actual interaction acceptance.
- No Unity slot is assigned. Do not duplicate caches or use another owner's Editor. Static Roslyn compilation reads existing Unity references and writes only `Builds/Validation/CasualSkillSkin/dotnet/common-api`; it launches neither Editor nor Player. The pilot's earlier 92 passing tests are not evidence for this API.
- Reopen design URLs only in a Chrome window verified by supported tools/UI as profile **dakrdong@gmail.com**. Do not modify other profiles' tabs. If verification fails, pause browser work only. This code task opened no new browser tabs.


[Static API receipt](Casual_UI_Common_API_Validation_20261014.json): Runtime/Tests compilation has 0 errors (47/2 warnings), 15 UI-contract regression checks pass, refresh/format/local links pass. The six new Unity cases were compiled, **not executed**. External Unity PID 30116 was observed running full EditMode tests in `scratchpad/proj-final`; recheck completion before claiming a slot and do not interrupt it. This owner's Editor/player/compiler processes are stopped.

The parent may take over `task-6/skill-skin` and its existing `Library` (Unity 6000.6.0f1) as the single validation project, after pinning this local commit and clean status. The parent reviews/integrates only the target screen commit and approved art on a validation branch while this common owner stops source edits. **No checkout, cherry-pick or cache move has been performed.** Final integration with later main changes, including the codex, remains parent-owned; an existing cache is not proof for newer sources.


## Common follow-up — current contract

The latest follow-up supersedes the initial API limitations above. `CasualEquipmentArt.Provider` supplies 359 approved PNGs (69,753,836 bytes; 330 item keys and 30 emblem keys; B28 shares B32). Use `UiSkin.Casual.WithEquipmentArt(CasualEquipmentArt.Provider)`. The [receipt](../../tools/casual_equipment_art_receipt.json) verifies delivery CSV, current director approval, final QA, all destination hashes and GUID preservation. [Import/verify](../../tools/import_casual_equipment_art.py) is idempotent. No gameplay catalogue unlock changes. Resources increase build inclusion regardless of the skin opt-in. Unity import and representative native artwork validation remain pending.

Noto400/700/900 now cover `≠①②ⓘ□◆＋×`: 15 symbols added, all previous 2989 glyph outlines/advances/hints and line metrics preserved, Jua and GUIDs unchanged, +4,484 bytes total. The pinned source lacks `↳ ↶ ⌖ ▴ ▸ ▾ ☐ ☑ ☰ ✔`; these remain explicit gaps, without fabricated glyphs or localization-key changes.

Detail geometry was restored: score 13/16pt and 52/64-high slot, original major-divider space, greater-affix inset16 and socket24/inset28. Casual primitives replace ornaments inside those same slots. Six same-font layout comparisons were added, execution pending. Common tab labels always use Navy so later Choice calls remain readable; Muted now targets4.5:1 on CreamShade. `TextColor(Color)` maps neutral ink; use `OnLight` for semantic metrics.

Trailing `UiSkin skin=null` is now supported by `TrainingChartView.Create`, `TrainingDpsPanel.Draw`, `RiftActivityPanel.Create`, `RiftCombatGraphWindow.Open` and `RiftSkillShareView.Card`. `TrainingDpsChart.SetSkin` preserves samples and caller hues; instance `CurrentColor`/`PreviousColor` are used by plot, legend and tooltip. Series timing, axes, solid/dashed distinction, input/scroll handoff, skill metrics/gauges and edit callbacks remain owned by existing code. Existing skill/boss art is retained because replacement IDs were not found in this delivery.

Call `GameUI.ConfigureCombatSkin(skin)` **before shared HUD construction**. Existing HUDs keep their captured scope until their normal rebuild. Call `GameUI.ConfigureEdictSkin(skin)` before sheet construction/rebuild; it also forwards to `WorldView.ConfigureEdictSkin(skin)`. The world API updates only known preview renderers and caches dedicated preview materials under the existing WorldView lifecycle. Slider ranges/steps, release-save, targets, path/ring geometry, widths, timer and camera remain unchanged. No recursive artwork replacement, global theme mutation or mid-drag rebuild. No native visual or performance claim yet.

### Aspect-stone size exception proposal — not applied

Read task16 commit `d0c10513911b6d4e098eb25462c0735aa6f18dbd` source and its recorded design measurements; no new browser/native inspection. At1440×810, reading1 and no safe inset, the800×450 shell yields Body776 and library236.32 (one122-min-width column). Changing only the reference to1440×810 gives library441.12 (three columns). Five122 cards with8 gaps require642 units.

Propose an **aspect-main-window-only** reference1440×810 plus workbench left300/right362/gaps12/centre remainder. Its centre742 gives library730 and five139.6-wide cards, matching the owner's recorded CSS width. Keep122 minimum and11/9pt, reduce columns for smaller screens/reading enlargement. This changes physical11pt from19.8px to11px at1440px and therefore needs an explicit size exception decision; it is not approved by the preservation rule. No global scale/shell change was made. KO/EN and native readability remain unverified.

External Unity PID30116 remained running after the app update, on Claude scratch `c0f5663b-4a35-4341-9c80-4957c89bc86b/scratchpad/proj-final`, full EditMode. Parent processes30114(bash)/30113(zsh); log updated07:38:38 UTC and no resultsXML existed. Never terminated or declared hung. Rift entry is first planned slot but unassigned. After external completion, parent still assigns slot and freezes integrated inputs. Parent can take over the existing task6 worktree/Library in place after clean handoff; no cache copy, checkout or merge has been performed here. The retained player binary is the previous skill pilot and does not validate these APIs.


Final follow-up [static receipt](Casual_UI_Common_Followup_20261014.json): Runtime/Editor/Tests errors0, warnings47/1/2; contract15 and refresh16 bindings passed. The21 new Unity cases compiled but executed0. At07:50:38 UTC externalPID30116 was stillR (1:05:21), log updated07:50:30 and no resultXML; the first slot is still unavailable. Latest rift-entry candidate: `118371c77908ddfa1cf57097efa7c6c0c9391b0a`.
