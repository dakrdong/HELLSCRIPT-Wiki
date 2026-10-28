# Character creation and selection handoff — 2026-10-14

> **2026-10-28 A2 correction:** References to the large edict window, ≡ menu and their dedicated components or smokes are historical. Use the [current entry points](../../Assets/HELLSCRIPT/Runtime/Presentation/GameUI.HuntEdict.cs) and [replacement screens and source owners](Hunt_Edict_Window_Removal_20261028.en.md). Earlier captures and test counts describe the earlier implementation.

[Korean details](Character_Plaza_Handoff_20261014.md) · [Save audit](Character_Plaza_Save_Audit_20261014.en.md) · [Art provenance](CharacterPlaza_Art_20261014.en.md)

## Delivery and dependency order

The approved six-hero save model and casual village screen are implemented in the isolated `task-21/character-plaza` source workspace, branch `codex/casual-character-plaza`. The original checkout, real saves and other Chrome profiles were not modified. No Library/Builds/Temp clone, remote PR, push, main merge or web deployment occurred.

Apply `e38096240d1d54f680204212f583340708d798e0` → `93219e3fef4f7a07f2e3249b9598b1e0e7f014a8` → common integration `4148241a` → this PR2 commit. PR2 directly descends from `4148241a`. Integration owner task-6/skill-skin waits for this source and the basic HUD handoff before one final Unity validation.

`HeroSlots.Index(class,gender)`, `Created(account)` and `IsCreated(account,index)` are stable. Slots0–2 are male Warrior/Ranger/Mage;3–5 are female counterparts. Schema23 archives three-hero input byte-for-byte before migration and preserves IDs/progress/equipment. An existing archive with different bytes refuses migration without replacing either file. New production accounts create only the male Warrior, preserving first-start direct entry. Later logins create the other five. Fresh legacy developer accounts keep the original three heroes and reject schema23 reads/writes.

The screen supports browse/select, 1.8-second cheer, atomic creation with rollback/retry, level/equipment, battle-ready speech and existing `CommitEntryCharacter → EnterPlaza(true)`. Preview/camera/equipment never save. Busy input and suspended-rift ownership are enforced. Keyboard order is Warrior male/female, Ranger male/female, Mage male/female.

## Implementation boundary

`CharacterSelectionState` owns pure phases, timing and12 personality lines; `CharacterPlazaSlots` is the screen's store boundary. `CharacterPlazaStage` aligns background/cutouts/patches with one transform, interpolates portrait class focus over0.9 seconds, and uses baked64×64 input masks with GPU-only textures. One shader and six material instances provide inactive/locked/selected appearance. There are no per-frame save/JSON/list rebuilds and no measured performance claim.

`CharacterSelectionView(.Equipment)` explicitly consumes `UiSkin.Casual.WithEquipmentArt(CasualEquipmentArt.Provider)` and shared fonts/buttons/StorageSurface/ContentWindowView/EquipmentSlotView/ItemDetailView. Eight equipment categories retain both hands and both rings inside their existing categories. Shared Escape consumption prevents closing equipment and returning to title with one key. Resize includes Canvas scale; create/equipment hit rectangles retain a minimum44 screen units on both axes.

PR2 owns the character page/controller partials, created-only gendered settings/legacy town lists, and Hunt Edict preview gender fallback. Item ownership integrity continues across all six bags. Common commit4148241a owns `GameController.SelectHero`, D4 host/class availability and the41-texture budget; those files are not duplicated here.

## Reference, assets and differences

The requested MD was absent at the original path, so the authoritative sibling wt-casual-art commit13a1ce7322ae69f5b518503b52598fbadf7ee5b8 was used; MD SHA256 c1ad2137e8f6f81a3e319dbbacea33ac102428e45175ea0012666bb3ff523f6f. Chrome's Hoosung Lee / dakrdong@gmail.com profile was verified in UI before using only that window. The Claude artifact title loaded but its content timed out twice (one retry). The MD explicitly permits the local28-board/source fallback, which was inspected.

The explicit character brief replaces the old three-actor geometry with village coordinates and class cameras; this exception to the general skin geometry rule was reported to integration. Layout adapts440×956 portrait,956×440 short landscape and1152×648 PC design units to safe areas. The equipment grid uses2/4 columns; displaying both real hands/rings and shared detail extends the single-item examples in the mockup.

All17 original PNG hashes and the separately approved six hero portraits/pixels were checked. Two backgrounds and12 poses preserve original PNG bytes. Vision compiled but first extraction failed with ANE Code9, so authored WebP cutouts/patches/crests were decoded to PNG. No new image generation occurred. Runtime assets are under `Resources/Art/CharacterPlaza/`:41 PNGs,31,524,964 bytes. Originals, metadata and old assets remain intact. The old CharacterSelection resource family is about9.23MB and still has shared equipment-atlas consumers; cleanup awaits actual-screen validation.

Remaining visual checks: Unity pixel comparison, WebP cutout seams, enlarged mage softness, shared font wrapping/long equipment names,44-unit targets and popup safe areas. Fixed painted battle weapons do not depict arbitrary equipped appearances; the female Warrior battle pose uses a sword instead of her model-sheet axe. Village/pose art remains **productionApproved:false**, and her exposed midriff requires the brief's **age12 rating review**.

## Verification and integration entry

[Static evidence](CharacterPlazaSourceEvidence20261014/summary.json) records exact scope: Runtime/Editor/Tests Roslyn compilation with zero errors; UI contract15 cases; refresh16 bindings;17 original/41 runtime hashes, new GUIDs,12 layouts and24 masks;22 new Korean keys and8 retired lines. The reference Node smoke passes912 combinations/28 boards; it does not execute the Unity implementation.

**Unity import, shader compilation, EditMode execution, player execution, physical-device checks and performance measurements: zero.** Fifteen save tests, state/asset tests and the new runtime smoke were authored and compiled only. Real saves and network accounts were not used.

After all sources freeze, include `HeroSlotsTests`, `CharacterSelectionTests`, `LocalizationTests`, `BuildResourceTests`, `TutorialHeroSlotsTests`, `SettingsRevisionTests` and D4 host checks in the single full EditMode pass. Run the same development build with `-hellscriptCharacterSelectionSmoke -hellscriptSavePath <fresh disposable save> -hellscriptScreenshots <evidence> -hellscriptServerBase invalid -nosound`. The smoke requires one created male Warrior, creates the other five, enters the female Mage, and captures five ratios, KO/EN, equipment/rotation and rift locks. Google authentication remains in its established account-validation path. Native input and side-by-side reference review remain required; desktop simulated insets are not physical-device validation.

After source freeze the integration owner owns further changes. Local validation logs, compiler inputs, contacts and failed Swift evidence remain under `validation/` with the parent artifact lifecycle record. No permanent deletion occurred.
