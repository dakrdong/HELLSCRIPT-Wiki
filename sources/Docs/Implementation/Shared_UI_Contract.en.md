# Shared UI and new-content contract

Updated: 2026-09-28
Use the latest inventory as the equipment presentation baseline. Compose new content from shared owners rather than copying rendering or gameplay formulas. Keep specialized layouts and each domain's rules.

[Korean version](Shared_UI_Contract.md)

## Presentation owners

| Element | Owner and contract |
| --- | --- |
| Theme | `UiTheme`: dark olive backgrounds, brass selection/actions, ivory text. Preserve semantic danger/gain/loss colors. |
| Fonts | `UiFonts.Body`; `UiFonts.Display` only for title branding. Individual windows must not destroy borrowed fonts. |
| Type | Title 20, heading 15, body 12, caption 10 before reading scale. Existing HUD/legacy adapters convert their coordinate system. |
| Equipment slots | Portrait 52, landscape 54, gap 6, common `UiTheme.Scale`. Reduce columns instead of shrinking slots. Preserve persisted slot indices and capacity. |
| Windows | Header/close, navigation, scroll body, fixed actions. `ContentWindowHost` owns stacking, background input, back and nested pause leases. |
| Details | `ItemTooltip` and `ItemDetailView` share order, values, ranges, effects, sockets, upgrades and protection. Fixed outer bounds, scrolling body. |
| Comparison | `EquipmentComparisonView` and `ItemComparison` share ring/hand replacement and stat deltas. |
| Paperdoll | `CharacterEquipmentView` owns ten placements. Inventory/storage presets show actual items; slot growth shows translucent body-part emblems. |
| Slot states | `EquipmentSlotView` owns grade border, level, lock, worn and selected markers. Forge work uses `ForgeWorkingPulse`. |

Element/class/rune colors, HP/mana, danger telegraphs and map symbols retain their meaning. Forge keeps equal three-column landscape geometry, storage keeps two inventories, and rune boards keep hex cells.

## Domain ownership

Use `HeroStats`, `StatCatalog` and `ItemComparison` for numeric meaning; `EquipmentSlots` and `Storage` for equip positions and movement; existing bulk/salvage/sale policies and `ShopAutoSelect` for protection and selection. Different actions intentionally have different eligibility rules. Do not replace them with one permissive check.

`InventoryQuery`, `ShopAutoSettings` and per-window state preserve filters, selection and scrolling. Existing uGUI drag adapters share pointer lifecycle conventions while movement, hex occupancy and reorder validity remain domain-specific. Existing `GameStore` commands revalidate quotes, ownership and resources atomically.

`GameStore.Committed` fires only after a successful persisted adoption. Duplicate receipts do not publish again. `StoreViewBinding` defers repaint while a modal is being edited. Presentation callbacks cannot turn a completed purchase into a failed purchase. Existing edit models distinguish applied, draft and saved state.

`TownWalk` and `ContentUnlocks` own access. `UiTime` formats durations but never chooses a clock: UTC work and pausable battle time remain separate. `GlobalHudSnapshot`, `SkillIconView` and saved combat/training snapshots preserve consistent labels/icons without recomputing history from live gear. `UiSafeArea`, `Loc` and existing device display/text/audio preferences remain shared.

Pass `EquipmentViewSource.Owned`, `Draft`, `BattleSnapshot`, `Catalog` or `RewardSnapshot`. Detail/comparison views copy their input and never mutate the account. Catalog definitions have no acquired roll; never fabricate one for display. Craft results and reward history use `RewardSnapshot` to preserve committed values. Controllers supply domain commands and drafts.

## Hunt Edict

Keep the original 25 groups and 130 global option IDs, values and ranges. Auto Equip and automatic cleanup share warehouse admission and full-warehouse repeat handling. [Auto Equip](Recommended_Equipment.en.md) adds a tab in the same window with 10 groups, 20 recommendation options and one warehouse-admission option, bringing the totals to 35 and 151. Changes use the existing draft and fixed Save/Revert actions. Landscape uses category, summary list and selected editor with independent scrolling. Portrait slides between summary and detail and restores list position. Search names/help/IDs across categories; optionally show changed groups only.

Show current choices, numbers, sets and priorities compactly; open controls on demand. Disabled conditions remain explained inline. Skill rows show icon/name/rank/equipped state while commands belong to selected details. New skill design remains excluded. Keep dirty state, revert and save in a fixed footer except on the skill-policy page, which uses a fixed activation action and scoped immediate saves through the existing transaction. Pending ranks, equipment and global settings stay detached. Navigation preserves drafts; close/preset replacement asks about unsaved edits.

Opening the window lands on the **Overview** tab: three combat styles, advice from the last hunt, equipped skills with their use policies, the seven combat-judgment groups and the automation status. A style is a bundle of existing quick presets and advice reads only stored combat records; both edit the draft and go through the existing Save/Revert. See [Hunt Edict overview](Hunt_Edict_Overview.en.md).

## New content workflow

1. Start in a checkout containing the latest merged `main`, then run `python3 tools/new_content_ui.py FeatureName`. Keep unfinished changes in an older checkout intact and use a separate current checkout. Existing files are never overwritten.
2. Supply bilingual title, explicit source, reading scale and render callback. Keep the draft in the controller, outside repaint callbacks.
3. Compose common views under `Navigation`, `Body` and `Actions`; call established transactions for mutations.
4. Document any specialized layout adapter and its shared owners. Do not duplicate font creation, formulas, equipment cards or canvas ownership.
5. Run `python3 tools/check_ui_contract.py`, `python3 tools/test_ui_contract.py`, related Unity tests and native macOS acceptance. Record images plus actual before/after state and persistence.

GitHub Actions runs the ownership checker and its fault-injection tests. The checker catches new window/canvas bypasses, independent font creation, missing shared equipment connections and account access in reusable views. It does not redraw arbitrary UI or prove visual correctness. Repository rules guide subsequent development; they are not an operating-system restriction on external code generators.

## Acceptance

Check identical equipment data, two rings/two hands/offhand/presets, unchanged saved slot indices, fixed actions and scrolling at 440×956, 956×440, 16:9, 16:10 and 21:9. Check Korean, English, larger text, all edict options and preserved drafts across search/navigation/rotation/language. Test failed saves, duplicate/stale requests, insufficient resources and nested pause/input restoration. Distinguish synthetic macOS pointer acceptance from physical-mobile validation.

See [completed-work integration](Completed_Work_Integration.en.md) for the initial merge and excluded skill work, and [shared UI validation](Shared_UI_Validation.en.md) for implementation evidence.

## Forge core crafting

`BlacksmithWindow.Cores` extends the existing forge adapter with a fourth tab, reusing shared slots, rarity colors, details, fonts, scale and window/input ownership. Definitions use `ItemTooltip.CatalogRanges` and `ItemDetailView.AppendCatalog`; results pass saved items. The reasons for retaining portrait page navigation, three equal wide columns and the quality gauge inside the forge, along with validation, are documented in [core crafting](Core_Crafting.en.md).

## Aspect Runestone

`AspectStoneWindow` is generated from the new-content template and opens `ContentWindowView` with `EquipmentViewSource.Owned`. It composes `CharacterEquipmentView`, `EquipmentSlotView` and `ItemDetailView` inside the shared navigation, scrolling body and fixed action region. Landscape has equipment, library and detail columns; portrait uses three steps. The confirmation step contains an inline equipment picker so changing slots preserves the selected aspect. `AspectStoneSession` owns selection and filters; `GameStore` owns collection, level-up and imprint transactions. Rune glyphs and red upgrade dots are content-specific meaning, while fonts, slots, window scale and panel colors use the shared owners. There is no field placement tab.

## Buttons and tabs

Follow the [shared button interaction contract](Button_UX.en.md). Create new buttons with `ContentWindowView.Button` and supply persistent selection with `UiTheme.Choice`. Do not duplicate button palettes or pointer transitions per screen.


Shared UI does not require a rectangular frame for every control. Vaults use `UiButtonRole.Icon` and `StorageChestGraphic` for open, closed, purchasable and locked chests. Shared input state, theme and fonts remain authoritative; `StorageWindow` and `GameStore` own selection and transactions. Ambiguous filters retain text labels. Chest content art uses two native-alpha PNGs in `Art/Storage`; `StorageChestOverlay` draws purchase/lock badges and drag progress. This user-requested content illustration does not replace the common button skin. See [button validation](Button_UX.en.md) for provenance, fixed bounds and input evidence.

## Town and combat HUD placement

`GlobalHudLayout` owns the persistent vitals, skills and potions; `GameUI.Plaza` and `TownJoystick` own the town heading, shortcuts and movement input. This HUD is an adapter using its existing canvas and proportional screen layout, rather than a content window. Exactly three potion slots reuse the existing bottle artwork and square frames, without a separate tray. Equipped legacy passives remain inspectable in the status-effect strip. Title backings reuse `StorageSurface` and `UiTheme` colors without new raster artwork. Presentation does not save accounts or consume potions. See [town HUD validation](Town_Hud_Responsive.en.md) for the portrait bottom baseline, centered movement pad and enlarged-text layout.

## Native skill tree adapter

The Skills tab presents the approved 37-skill class tree, four normal active slots and a separate ultimate slot. Unlocked passives with assigned ranks always apply; a slot menu opens that skill’s edict settings. The existing HuntEdictWindow retains draft ownership, shared theme/icons/window host, fixed controls and independent scrolling. See [integration](Hunt_Edict_Skill_Tree.en.md).

Since the 2026-09-27 redesign, the tree is a dedicated layout that draws three branch trunks, level-stage gates and prerequisite links. Trunks, links and glows are a texture-free `SkillTreeGraphic` mesh; node positions come from the presentation-only `SkillTreeLayout`. `ClassSkillTree` still owns unlock, rank and equipment rules. Nodes and skill-bar sockets use the `UiButton` `Icon` role like the storage chests, and their ornament graphic reads the common button's hover, press and focus state. Selection is shown with light rather than an outline. `SkillIconView` draws the icons; the ornate frames, backdrop and point gem are content art generated with GPT at the user's request. This is separate from the rule that common button decoration uses no raster art, and icons are fitted to each frame's measured transparent opening. Production records and checks are in the [skill tree screen art record](../Art/SkillTreeUi/Skill_Tree_UI_Art.en.md).

Equipped-skill actions use a compact menu anchored to the pressed socket within this window. They reuse its buttons, theme, back handling and draft; reflow reads the same socket's rebuilt coordinates. A transparent outside-click layer neither dims the screen nor activates controls underneath. Save confirmations and normal option pickers retain their centered layout.

Global-group quick presets retain the existing draft and selection dialog. Skill policies use fixed preview tabs, a separate activation action and immediate scoped saving. The active check follows the saved owner, while browsing never saves. `SkillPresetExampleView` draws read-only tactical diagrams with shared icons/graphics; Custom Settings shows detailed controls without an image. Narrow layouts wrap the fixed tabs. This remains an adapter within the existing window, preserving unrelated drafts and shared theme/fonts/window ownership. See the [quick preset coverage and verification](Hunt_Edict_Quick_Presets.en.md).

## Rift entry adapter

`RiftEntryWindow` uses the shared `ContentWindowView` owned-data entry point. Landscape fixes the sanctuary illustration and tier selection beside independently scrolling preparation, keeping the tier and entry conditions visible together. Pickers use the shared optional maximum size without a new canvas or lifecycle. It reuses `UiTheme` paid-fatigue colors, `PotionArt`, `SkillIconView` and existing `GameStore` transactions. First-clear rows combine `RewardBoxCatalog` grants and `ContentUnlocks` milestones. The shared detail popup separates automatic service access from box claim state; `GameStore` transactions own the actual grants. See [implementation and validation](Rift_Entry.en.md).
## Inventory potion placement

At the user's request, potion cells use smaller dimensions of 32 in portrait and 28 in landscape. Equipment retains its 52/54 baseline. The adapter reuses `EquipmentSlotView` and sits beside the weapon row owned by `CharacterEquipmentView`, without adding a row. Per-cell assignment is separate from the shared use-order setting, with one gear beside the group. [Potion slot validation](Potion_Slots.en.md) records geometry, input and persistence evidence.

## Jeweler

`JewelerWindow` uses `ContentWindowView`, `EquipmentViewSource.Owned`, `StoreViewBinding` and `GameStore` transactions. Its gem-type/tier count matrix is a specialized adapter rather than equipment-slot geometry. Landscape uses two independent scroll panels; portrait uses navigation tabs and the same fixed craft action. Socket details retain the common equipment owners. See [Jeweler runtime](Jeweler_Runtime.en.md).

## Attendance events

`AttendanceWindow` starts from the new-content template and uses `ContentWindowView`. Day tiles describe reward definitions; account attendance and claim state are supplied by the controller. Landscape places the altar panel with the selected reward beside the day grid; portrait stacks them. At 100% text every day is visible without scrolling; larger text enlarges the tiles and the shared body scrolls. Fixed actions hold the suppression checkbox and the claim button. The altar art and claimed seal come from `AttendanceArt`, compact tile amounts reuse the jeweler's `JewelerSession.Compact`, the pulse reuses `ForgeWorkingPulse` and the round glow reuses `TownCircleGraphic`. `AttendanceSwipe` routes horizontal gestures to pages and vertical gestures to the shared body scroll. `ContentWindowHost` owns combat pause, `GameStore` owns account transactions, and `StoreViewBinding` refreshes after committed saves. See [attendance events](Attendance_Events.en.md) for calendar, reward and verification rules.

## Combat records

`CombatRecordsWindow` uses the new-content starter, `ContentWindowView` and `EquipmentViewSource.BattleSnapshot`. A standard Unity `Dropdown` selects the outcome; the record body scrolls independently. The window owns selection/filter state, while `GameStore` owns post-commit file export. Existing review pages remain shared; live text uses the battle HUD adapter. See [combat journal and server collection](Combat_Journal_Server.en.md).

## Title adapter before sign-in

The Google sign-in dialog in `TitleScreenView` remains part of the existing full-screen title adapter. It preserves safe areas, a scrolling body and fixed close control while sharing `UiFonts`, `UiTheme` and `UiButton`. `GoogleLoginClient` owns authentication; `GameController.Accounts` and `AccountProfiles` own save binding. Presentation code cannot create a successful login or directly change save ownership. Provider branding is an explicit exception: the official Google palette and English Roboto font belong to `UiButtonRole.GoogleSignIn`, `UiTheme` and `UiFonts.GoogleSignIn`. See [Google sign-in](Google_Login.en.md) for scope and validation.

## Tutorial guidance

`TutorialJournalWindow` uses the shared content template. Practice selection distinguishes owned equipment from recipe definitions and reuses `ItemDetailView`. Native inventory and `GameStore` own actual equipment mutations. `TutorialAnchorRing` attaches to logical button IDs and owned inventory cells, does not intercept input and never mutates account state. Brief combat guidance uses the existing HUD adapter. Since 2026-09-29, the top-left Adventure guide HUD button and notifications directing players to it are no longer displayed. First-map explanations and first-use content guides use the story dialogue below. See [implementation](Tutorial_Progression.en.md).

## Rift victory adapter

`RiftVictoryWindow` uses the shared `RewardSnapshot` entry point. Three fixed result regions keep the whole summary visible, while source-grouped loot scrolls independently. Four regular skills and one ultimate read the combat snapshot. Slots, details, icons, safe area and button ownership remain shared, with no private canvas. Decorative numerals reuse `UiFonts.Display` at fixed size. The result page retains the host's input/Back stack while permitting the repeat clock; nested windows retain normal blocking. See [ownership and validation](Rift_Victory.en.md).

Inventory and Rift reward inspection share `ItemDetailPopup` for the item-information header, range controls, fixed action boundary and card viewport. The popup receives explicit snapshots and callbacks; it has no live account access.

Inventory and rift item inspection pass context, notes and actions to `ItemDetailPopup.SetFooter`; the shared owner measures the footer and lays out its controls. Content adapters must not duplicate footer heights or button coordinates. Failed rifts use the same result adapter and shared `UiTheme` failure colors. Skill damage cards are noninteractive readers of target-specific aggregates.

When closing a nested detail window, `ContentWindowHost` restores both the prior selection and its input modality. Pointer selections must not become keyboard focus. Equipment rarity borders are preserved; keyboard focus cues return only for keyboard-originated selection.

Text logs use the `CombatLogWindow` shared `ContentWindowView`/`BattleSnapshot` modal, preserving the underlying result with a Close-only footer. Skill contributions read shared `CombatStatistics` and catalog display roles; presentation must not replay combat events to calculate values.

`IncomingDamageWindow` uses the same `ContentWindowView` / `BattleSnapshot` adapter. `CombatStatistics.TopIncoming` owns cumulative source rankings and their top-only denominators; the view does not duplicate aggregation or mutate saves.
## Proportional scaling and one bottom HUD

Follow [window shrinking and a unified bottom HUD](Responsive_Hud_20260928.en.md). The complete bottom HUD retains one landscape composition and one scale. Do not independently resize or rearrange potions, skills, seal, XP, HP or MP by aspect ratio. Ultimate and ordinary skills always share one row. Ordinary controls and text also shrink together. Landscape power saving uses equal-width columns and a centered vertical divider.

Attendance update, 2026-09-28: claimable uncollected rewards use green `AttendanceClaimGlow` light and a persistent `UiTheme.Claimable` border, independently of selection. Claimed and locked rewards have no green highlight. The original gold glow remains; the older pulse description above does not apply to the new green indicator.

## Power-saving hunt HUD

The existing `GameUI.Idle` canvas is a HUD adapter that keeps real hunting active. Its background does not pause; storage, inventory and the new `IdleEquipmentDetailWindow` use the shared nested pause owner. `EquipmentSlotView`, `ItemDetailView` with `RewardSnapshot`, `UiFonts` and `UiTheme` own equipment rendering, typography, color and scale. Independent scrolling retains every session record while creating only visible rows. Rendering never executes a transaction or substitutes current ownership for captured historical gear. See [power-saving mode](Idle_Display.en.md) for behavior and validation boundaries.

`IdleHuntStatus` reads actual `RunState` and `RepeatHuntSession` for location/activity. It reuses `RiftObjectives` counts; the footer reuses `GlobalHudView.CreateSeal`/`Portrait` and `GlobalHudSnapshot.ReadGrowth`. Refreshing the power-saving view does not rebuild the normal HUD's skill, potion or effect lists.
## NPC dialogue

`NpcDialogueWindow` uses the shared starter and `ContentWindowView` with `EquipmentViewSource.Catalog`. Portraits and greetings are catalog definitions; the view does not read or mutate an account. To keep the field visible, the shared bottomDock option anchors the window to the safe-area bottom, caps its height at 30% of the safe area and removes full-screen dimming. The portrait remains fixed on the left, the name sits above the independently scrolling greeting on the right, and actions remain fixed below. Since 2026-09-28 it is drawn as the story scene below (`StoryDialogueWindow.Scene`): the figure rises out of the band but stays inside the safe area and clear of the greeting and actions, only the greeting scrolls, and the service and goodbye actions stay fixed at the bottom of the band. Existing centered windows retain their default layout and backdrop. `GameController.NpcDialogue` revalidates distance and delegates to existing service access checks. [NPC portraits and dialogue](Npc_Dialogue_Portraits.en.md) records identities, art provenance and validation.

## Story dialogue and tutorial staging

`StoryDialogueWindow` is the shared dialogue box for scenes in which a character speaks. It uses the `ContentWindowView` `bottomDock` option, so the window host keeps owning the safe area, input blocking, back and pause leases. It provides a full-width band (a clear gradient at the top, opaque behind text), a cartouche name plate on a brass rule, large character art with transparent surroundings rising out of the band (in landscape with the hero dimmed on the right), centred glyph-by-glyph lines, next and skip, and choices on the last line, using `ContentWindowView.Button`, `UiTheme` colours and `UiFonts`. The reveal never changes `Text.text`; it only fades glyph quads in. `NpcDialogueWindow` passes `StoryDialogueWindow.Scene` to `ContentWindowView.Open` to draw the same scene. Every text is passed as its Korean source and translated when drawn.

The tutorial staging layer (`TutorialCinematic`) is a display-only canvas above the play HUD for bars, fades, title cards, objective notices, companion lines and the item reveal. Its canvas is created by the existing owner `GameUI.cs` at sorting order 130, below every content window (400+). The item reveal reuses `EquipmentSlotView` and `EquipmentGradePalette`, and ornaments reuse `EquipmentRevealGraphic`. It neither reads nor writes an account, and progression stays with the existing `GameController` commands. Combat dialogues open with `blocksGameplay:false` so the world keeps presenting, while the pause lease still applies. [Tutorial staging](Tutorial_Staging.en.md) records the flow and validation.

2026-09-29: The 35 global groups use fixed top icon tabs instead of a split list/detail layout. Tabs wrap and only the selected content scrolls. Preset storage uses five fixed single rows, inline names, disk save icons and per-slot sharing, without page scrolling. The default-policy switch lives at the top of Overview and saves only the hero's execution mode through `GameStore.SetRecommendedEdict`. `HuntEdictDefaults` resolves detached policies; presentation components never overwrite authored settings. See [presets](Hunt_Edict_Quick_Presets.en.md) and [Overview](Hunt_Edict_Overview.en.md).

## Training ground

`TrainingGroundWindow` (the lobby) and `TrainingGroundResultWindow` (the result) use the new-content template's `ContentWindowView`. The lobby reads owned data (`EquipmentViewSource.Owned`). In landscape it places the skills and the enemies in two independently scrolling columns, so the skill checks and the enemy setup are visible together; portrait stacks them in the shared body scroll. The enemy picker is a maximum-size window (`Catalog`) from the same window host. The lobby controller owns the setup draft and keeps it in the hero save through `GameStore.SaveTrainingGroundSetup`; per-skill edict edits reuse `HuntEdictWindow` and `GameStore.CommitHuntEdict` unchanged. The result opens as a battle-snapshot (`BattleSnapshot`) page like the rift result and never blocks the repeat clock. The live DPS panel and the pause dialog are adapters on the existing battle HUD canvas (`GameUI`), and the chart is the texture-free `TrainingDpsChart` mesh. Fonts, colours, buttons and skill icons come from `UiFonts`, `UiTheme`, `ContentWindowView.Button` and `SkillIconView`. See [implementation and verification](Training_Ground.en.md).
