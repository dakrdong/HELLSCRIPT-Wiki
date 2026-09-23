# Shared button and tab interaction

Updated: 2026-09-23
Korean: [공통 버튼과 탭의 상호작용](Button_UX.md)

Storage selected-tab skins were being repainted as ordinary buttons by `UiTheme.Button`. Screens now provide persistent selection explicitly; shared controls own input feedback and rendering.

## Visual direction and references

The [Diablo II: Resurrected alpha stash](https://interfaceingame.com/screenshots/diablo-ii-resurrected-technical-alpha-stash/), [Diablo III bank](https://interfaceingame.com/screenshots/diablo-iii-bank/) and [Diablo IV stash imagery](https://mythicdrop.com/guide/diablo-4-shared-stash) were inspected in the in-app browser. These are historical visual references, not statements about current game specifications.

The design uses raised upper edges and recessed lower edges inspired by II, clearly separated tabs and decorated actions inspired by III, and strong selected-surface contrast inspired by IV. HELLSCRIPT keeps its olive, brass and ivory palette. Chosen buttons use their highlighted face; the current vault uses its open lid and visible interior. Redundant selection underlines and lozenges are removed from the shared renderer. Selected list rows and options keep a darker face so rarity colours and secondary labels retain contrast. No reference artwork, raster asset or external package was imported; decoration uses resolution-independent Unity meshes.

## State contract

| State | Presentation and behaviour |
| --- | --- |
| Action | Dark face, raised top, recessed lower ledge and double border. |
| Primary | Brass face and small side ornaments identify the main action. |
| Destructive | Red-brown treatment for dismantling actions; existing confirmation and transaction protection remain authoritative. |
| Chosen | Screen-owned selection drives the bright face. It survives hover exit and focus changes. |
| Hover | Face and border brighten over about 85ms without changing selection or saved state. |
| Pressed | Bevel lighting reverses. Exit/release clears press feedback without moving layout or hit geometry. |
| Keyboard focus | An inner light frame is independent of persistent choice. Submit uses the existing Button action. |
| Disabled | Face, border and text are subdued; hover, press and execution are suppressed. Parent CanvasGroup availability is respected. |
| Sequential lock | A padlock marks later chests. The next purchasable chest displays a plus badge and opens the existing confirmation; later locked tabs retain their explanatory action. |

## Ownership and coverage

`UiTheme` owns role palettes and transition duration; `UiButton` owns Selectable input feedback and the supplied choice; `UiButtonFace` owns the decorative mesh. `UiButton` subclasses the existing Unity Button, preserving click, submit and subscriptions. Use `UiTheme.Choice` for persistent selection and `UiTheme.Skin` for established skin adapters.

Adapters cover storage, inventory, blacksmith, equipment shop, rune master, hunt edict, aspect stones, comparison targets, rune boards, settings, title, character selection and the shared `GameUI` factory. Gems, crafting, training, rewards, collection and history inherit the shared action treatment through that factory. Warehouse navigation uses the chest adapter below; item categories retain their tab labels.

Equipment, skill and minimap controls use the `Item` role, which draws nothing at rest and adds an inset feedback border on hover/press/focus, preserving existing artwork and rarity colours. Transparent modal dismissal targets use `UiTheme.Backdrop`, excluding decoration and keyboard navigation. Dedicated combat HUD skill/potion indicators and map switches retain their semantic displays.

Decoration is non-raycasting and excluded from layout. It never accesses or changes account data. Storage numbering, capacity, costs, equipment protection, drafts and combat rules keep their existing owners.

## Select vaults through chest icons

Shared UI standardizes input and state. It does not require every control to be a rectangular button. Vault navigation uses the `Icon` role and `StorageChestGraphic` because the chest silhouette directly conveys its purpose and state.

- Only the current vault has an open lid and visible interior. Other chests stay closed. Hover and keyboard focus never open a chest.
- Front, side, lid, brass bands and shadow give the chest depth. The graphic reads common hover, press and focus states, with no separate input path, rectangular face or full frame.
- I–V identifies each position. The next purchasable chest shows a plus badge; its hover hint and purchase confirmation show the currency and exact cost. Later chests show a padlock and retain the existing unlock-order explanation when activated.
- The current vault name replaces the redundant panel title, eliminating a separate caption row. Delayed hover or keyboard focus shows a temporary name/unlock hint. Touch users select a vault to update the heading and retain the rename action. Chests and numbers fit within 36 layout units. Prices no longer reserve a permanent row, and compact category buttons return more space to item slots.
- `StorageWindow` supplies selected, owned and purchasable state. The graphic never reads an account. Confirmation, prices, persistence, numbering, capacity and drag-hover switching retain their existing owners.
- Category, grade and sort controls keep text where an icon alone would be ambiguous. This change removes repeated labels where a chest already communicates the action.

### Chest validation

The updated base is merged `main` at `660f0f7`. Evidence for this revision is separate from the earlier shared-button results below.

- Focused Unity Edit Mode: **117 passed, 0 failed, 0 skipped**, covering choice/focus separation, closed unowned chests, shared UI, Graphics, localization, storage transactions and equipment presets. [Test XML](ButtonUxEvidence/Chests/editmode.xml)
- The macOS Development build verified purchase cancellation/exact charging, switching between owned chests, renamed headings and navigation without account writes. Timed drag-hover progress, opening the destination chest and an actual exact-slot transfer also passed. [Runtime](ButtonUxEvidence/Chests/runtime.txt) · [Build](ButtonUxEvidence/Chests/build.txt) · [Matching sources](ButtonUxEvidence/Chests/source-fingerprints.json)
- Rechecked **20 storage combinations** (five ratios × Korean/English × 100%/150%) and six other screen families. This run produced 58 screenshots. [Shared UI rerun](ButtonUxEvidence/Chests/shared-runtime.txt)
- Removing the separate name row and compacting the heading/icon/category rows recovers **51 UI units** relative to the first chest draft: about **92 px at 1440×810** and **55 px at 440×956** at the same canvas scale. Chests keep their previous artwork size; the 35-unit hit region covers each icon. The second compacting pass recovers another 14 units by removing selection ornaments and the permanent cost row. [Before](ButtonUxEvidence/Chests/before-compact.png) · [After](ButtonUxEvidence/Chests/storage-pc.png)
- Phone sizes were simulated in the macOS player. This does not constitute physical-mobile validation or a full Edit Mode rerun.

![Open and closed vault chests](ButtonUxEvidence/Chests/storage-pc.png)

![English portrait storage at 150 percent text](ButtonUxEvidence/Chests/storage-portrait-en-150.png)

## Earlier shared-button validation


- Base: merged `main` at `12505e0`, including the latest equipment-card/drop feedback and rune-window width improvements. Verification used a separate checkout and isolated save files; the open user Editor and real account were untouched.
- Initial focused Unity Edit Mode: **146 passed, 0 failed, 0 skipped**, covering input states, shared UI, Graphic requirements, localization, blacksmith and inventory. [Test XML](ButtonUxEvidence/editmode.xml)
- The pre-integration full Edit Mode run had **3,578 passed and 1 failed out of 3,579**. The new button Graphic lacked its CanvasRenderer declaration; that was fixed. The same test found missing declarations on the newly merged equipment-card/drop Graphics, also fixed. All passed in the 146-test rerun above. This is not a complete 3,579-test rerun of the final revision. [Full original XML](ButtonUxEvidence/preintegration-full-editmode.xml)

- Initial macOS Development build succeeded. **All eight native scenarios exited with code 0**: button states, shared UI, process-restart persistence, inventory, ranges, title, character selection, and the newly merged equipment-card/drop feedback. [Build](ButtonUxEvidence/build.txt) · [Scenario results](ButtonUxEvidence/native-results.json) · [Matching source fingerprints](ButtonUxEvidence/source-fingerprints.json)
- Button acceptance covers hover, press, exit cancellation, persistent choice, keyboard focus/submit, purchase cancellation and disabled-save blocking. Storage was checked in **20 combinations**: 440×956, 956×440, 1440×810, 1440×900 and 1680×720 × Korean/English × 100%/150% text. Six other screen families were checked in both phone orientations, both languages and 150% text, producing 49 screenshots in total. [Button results](ButtonUxEvidence/buttons-runtime.txt)
- Shared acceptance checks all 25 edict groups and 130 global options, draft preservation, actual save and fresh-process reload. Inventory equip/compare/drag/dismantle, shared auto-selection, persisted range preferences and temporary Ctrl overrides passed. [Shared UI](ButtonUxEvidence/shared-runtime.txt) · [Restart](ButtonUxEvidence/shared-resume.txt) · [Inventory](ButtonUxEvidence/inventory-runtime.txt) · [Ranges](ButtonUxEvidence/ranges-runtime.txt)
- Title, character selection, actual town entry, equipment-card styling and drop feedback were revalidated. [Title](ButtonUxEvidence/title-runtime.txt) · [Characters](ButtonUxEvidence/characters-runtime.txt) · [Equipment cards](ButtonUxEvidence/item-polish-runtime.txt)
- Shared UI ownership plus nine regressions, wiki build/check, ten Python tests and JavaScript checks for 307 pages, 28 databases and 2,802 records all passed. Public publication is performed from merged main.

Automation uses actual uGUI handlers, raycasts and services in the macOS Metal player. Phone ratios are simulated in a Mac window; **physical-mobile touch and performance were not tested**.

## Earlier shared-button screens

![Selected warehouse and category](ButtonUxEvidence/storage-pc.png)

![English portrait warehouse at 150 percent text](ButtonUxEvidence/storage-portrait-en-150.png)

![Blacksmith tabs and equipment selection](ButtonUxEvidence/forge-pc.png)

![Destructive dismantling confirmation](ButtonUxEvidence/dismantle-portrait.png)

![Shared buttons on the rune board](ButtonUxEvidence/runes-portrait.png)

![Primary title action](ButtonUxEvidence/title-pc.png)
