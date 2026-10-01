# Hunt edict skill tree integration

Updated: 2026-10-01 · [한국어](Hunt_Edict_Skill_Tree.md)

## 2026-10-01: clear equipment marks and compact skill actions

Equipped actives and ultimates have one extra thin outline around the seal, with no slot-number badge. The rank/max-rank label stays below it. Empty equipment sockets no longer show numbers either.

Clicking an active or ultimate in the tree shows its inspector and a small bubble beside the icon: **+** when unequipped, **−** and a **parchment-and-brush edit pictogram** when equipped. The + is disabled for an unlearned skill. Passives apply by their allocated rank and have no equipment action. Outside click and Back dismiss the bubble. Rotation and language changes resolve the rebuilt node again.

The equipped-skill rail on the detailed preset page is navigation: another skill opens its presets directly, without an action popup. Browsing changes neither the draft nor its saved owner. Tree equipment changes retain manual Save/Revert; policy activation retains immediate scoped saving.

[Scope and final verification](Hunt_Edict_Simple_Skill_Actions.en.md)

## Screen and interaction

The Hunt Edict **Skills** tab shows all 37 skills of the current class, arranged by level stage and three branches. Selecting an icon displays effects, unlock requirements and ranks. Development skill IDs in descriptions are replaced by localized skill names.

There are four equipped normal active slots and one separate ultimate slot. At most one ultimate can be selected. Every unlocked passive with an assigned rank applies automatically, so there is no passive equipment area. Existing version 3 free base ranks count as assigned ranks. The combat HUD reads the same four normal slots, separate ultimate slot and actual cooldowns.

Selecting an equipped active in the tree opens icon-only **−** and **Edit hunt edict** actions. Editing opens that skill's preset tabs and combat examples. Custom Settings has detailed controls without an example image; edits save immediately after activation. Returning to the tree preserves the saved policy. The separate skill-edict subtab is removed. Basic attack and attack priority remain available under **Attack settings**.

Landscape puts the tree on the left, the inspector at the top of the right column and the skill bar beneath it. Portrait puts the tree on top, fixes the skill bar at the bottom and opens the inspector between them when a skill is selected. Equipped slots, main navigation and skill-tree save controls stay fixed. The tree, details and policy content scroll separately. The per-skill policy page removes the manual-save footer and moves its skill bar to a right-hand vertical rail in landscape, retaining the bottom dock in portrait. Korean, English and larger text are supported.

## Traditional hack-and-slash tree (2026-09-27 redesign)

The user found the old tree cheap: it listed each skill in a box, and the icons sat against the top of their boxes and looked misaligned. It was rebuilt as a traditional hack-and-slash skill tree, with mobile action RPGs and Diablo III/IV as references. The Diablo IV and III screens were reviewed in the in-app browser; no other game's art or screens were copied.

- **From Diablo IV**: branches that light up along unlocked paths, diamond gates between stages, a rank label under each icon (`2/5`), the inspector beside the tree and a remaining-points display.
- **From Diablo III**: round, ornately framed skill sockets and stage titles with ornamental rules.
- **From mobile games**: a bottom-opening inspector in portrait, the skill bar fixed at the bottom, unlock levels on locked skills and large touch areas.

Each of the three branches is a vertical trunk. Level-only skills hang from the trunk; a skill whose prerequisite is in the same stage hangs directly below it with a link. Trunks and links glow brass up to the hero's level. Stages not yet reached are shaded and each skill there shows its unlock level, such as `Lv.26`. Ultimates sit at the end of their trunk in the last stage. Selecting a skill draws bright links to its prerequisites and to the skills it opens, including ones in other stages or branches.

Actives use a round frame, passives a square frame and ultimates a four-pointed crest. The frames, tree backdrop and point gem were generated with GPT at the user's request. Icons are centered in each frame's measured transparent opening, so frame and icon no longer drift apart. Upgraded and equipped skills glow softly, the selected skill glows brightly, and equipped skills carry their slot number (a diamond for the ultimate). An outline around the selection made the node look like a box again, so state is shown with light only. See the [skill tree screen art record](../Art/SkillTreeUi/Skill_Tree_UI_Art.en.md).

Saving and rules are unchanged. `ClassSkillTree` and the draft still decide unlocks, ranks and equipment; the new layout (`SkillTreeLayout`) and ornament mesh (`SkillTreeGraphic`) only draw. Existing button names are kept, so tutorial guidance and other smokes find the same buttons.

## Progression and combat

`tools/build_skill_tree.cjs` compiles the levels and prerequisites from `Prototypes/SkillTree/tree-design.cjs` into the native `ClassSkillTree.json` resource. HTML and native progression do not maintain separate handwritten graphs. Existing version 3 allocations retain their free base ranks and upgrade budget. Explicitly resetting the allocation switches that draft to version 4, clears all ranks to zero and refunds a budget equal to hero level, including the starting point. Every rank from 0 to 1 onward costs one point: one point at level 1 and 40 at level 40. Uninvested skills cannot be equipped or grant passive effects. Merely opening or loading an existing build never redistributes its ranks.

Direct effect dependencies remain required. Ultimates require level 40 and any unlocked skill in the preceding level 30–39 stage. Unlocking a prerequisite does not require equipping it in an active slot.

The existing six passives are evaluated by `HeroStats`; expanded passives use the existing combat effect owners. Both check unlock eligibility and assigned ranks. Always active means available once unlocked and invested; their hit, freeze and other trigger conditions remain intact.

## Persistence and compatibility

The detached `HuntEdictEditSession` draft is validated and atomically saved by `GameStore.CommitHuntEdict`. Ranks, four slot positions, ultimate, automatic use, skill policies, attack order and global edict settings are saved together. Reflow and language changes never save. On the skill-policy page, `SaveSkillPolicy` saves only the selected skill policy through the same transaction, preserving other pending rank, equipment and global edits. See [quick presets](Hunt_Edict_Quick_Presets.en.md) for preview tabs, active markers, combat examples and Custom Settings.

Existing player configurations use `ClassSkillLoadout` version 3; explicitly reset allocations and characters created from 2026-09-29 use version 4. Production startup atomically upgrades owned heroes and preserves the original build. Investments below the new unlock level are refunded to free base rank. Existing version 1–2 development fixtures retain their original meaning. A legacy suspended run retains its snapshot; explicitly editing its skills uses the existing live-change transaction to adopt the tree.

HED5 shares the preset name and full skill configuration. Existing HED1–HED3 import and HED4 skill decoding remain available. Received allocations over the receiver's point budget can be stored but cannot be applied.

A live change preserves health ratio, spent cooldowns and the shared ultimate timer. Removing an in-flight skill cancels its preparation/channel and movement intent while retaining validation of already released effects.

New legendary/set drop registration is outside this UI integration.

## Validation

Results and captures are linked in the follow-up verification record. Native macOS uGUI pointer checks and restoration in a new process are separate from physical mobile-device validation.

Verified: 417 Unity Edit Mode regression cases passed with zero failures/skips, followed by 71 persistence, transition and localization cases with zero failures/skips. The runs overlap and must not be summed as unique cases. The macOS development build succeeded. Real uGUI raycasts and pointer down/up/click events verified investment, equipment, removal, policy editing and saving. A separate player process restored the same configuration.

- [Validation summary](HuntEdictSkillTreeEvidence/validation.json)
- [Equipped slot menu](HuntEdictSkillTreeEvidence/slot-menu.png)
- [Mage passive and named references](HuntEdictSkillTreeEvidence/mage-detail-ko.png)
- [Portrait, English, 150% text](HuntEdictSkillTreeEvidence/portrait-large-en.png)
- [Per-skill policy editor](HuntEdictSkillTreeEvidence/policy-large-en.png)
- [Interaction results](HuntEdictSkillTreeEvidence/runtime.txt) · [Fresh-process restoration](HuntEdictSkillTreeEvidence/restart.txt)

Checked 440×956 portrait, 956×440 landscape, and 1600×900, 1600×1000 and 2100×900 desktop. This is not verification in the user's currently open Unity Editor or on physical mobile devices.

### 2026-09-27 redesign verification

Built and tested from a cloned project of a separate work folder created from the latest `main` (`c751d84d`). The user's open Unity Editor and real account saves were not used.

- Unity Edit Mode related tests: **131 passed, 0 failed, 0 skipped**, including the new tree layout test for each class and the localization, shared UI, button, hunt edict, quick preset and passive tests. The first run failed one localization test. The cause was not this change: two first-play acceptance strings from the existing commit `ce236394` had no English entry. After adding them, the rerun passed. [Test XML](HuntEdictSkillTreeRedesignEvidence/editmode.xml)
- The shared UI ownership check with its 9 regressions and the frame opening measurement (`Docs/Art/SkillTreeUi/measure.py`) passed.
- The macOS development build succeeded. The skill tree smoke used real uGUI raycasts and pointer down/up/click to verify rank investment, equipping, the slot menu, removal, per-skill policy editing, rotation, language switching and saving, and a separate player process reloaded the same configuration. [Interaction results](HuntEdictSkillTreeRedesignEvidence/runtime.txt) · [Fresh-process restoration](HuntEdictSkillTreeRedesignEvidence/restart.txt)
- This smoke had been failing since new accounts start in the mandatory tutorial, which keeps the edict window closed. It now starts from a tutorial-complete account. Captures for landscape English/Korean at 150% text and for a level 18 hero's locked stages were added.
- Checked: portrait 440×956, landscape 956×440, desktop 1600×900 (16:9), 1600×1000 (16:10) and 2100×900 (21:9), Korean and English, 150% text in portrait and landscape.

Captures:
- [Desktop 16:9, passive selected](HuntEdictSkillTreeRedesignEvidence/mage-detail-ko.png) · [Desktop 16:10, ultimate stage](HuntEdictSkillTreeRedesignEvidence/layout-1600x1000-ko.png) · [Desktop 21:9](HuntEdictSkillTreeRedesignEvidence/layout-2100x900-ko.png)
- [Portrait 440×956](HuntEdictSkillTreeRedesignEvidence/layout-440x956-ko.png) · [Landscape 956×440](HuntEdictSkillTreeRedesignEvidence/layout-956x440-ko.png)
- [Portrait 150% Korean](HuntEdictSkillTreeRedesignEvidence/portrait-large-ko.png) · [Portrait 150% English](HuntEdictSkillTreeRedesignEvidence/portrait-large-en.png) · [Landscape 150% Korean](HuntEdictSkillTreeRedesignEvidence/landscape-large-ko.png) · [Landscape 150% English](HuntEdictSkillTreeRedesignEvidence/landscape-large-en.png)
- [Level 18 Warrior: lit branches and locked stages](HuntEdictSkillTreeRedesignEvidence/progress-lv18-440x956.png)
- [Slot menu](HuntEdictSkillTreeRedesignEvidence/slot-menu.png) · [Per-skill policy](HuntEdictSkillTreeRedesignEvidence/selected-skill-policy.png) · [Policy at 150% English](HuntEdictSkillTreeRedesignEvidence/policy-large-en.png)
- Before the redesign: [landscape](HuntEdictSkillTreeEvidence/layout-1600x900-ko.png) · [portrait](HuntEdictSkillTreeEvidence/layout-440x956-ko.png)

Mobile ratios were reproduced in a macOS window. Touch and performance on physical mobile devices were not verified.

## 2026-09-28 allocation reset and inspector cleanup

- Removed the unlocked-skill count under remaining points. Removed the inspector's attack-order, automatic-use and policy summary, along with its duplicate usage-conditions button. The equipped-slot menu still opens the skill policy editor.
- All three first-row actives now become available at level 1: Whirlwind, Leap Slam and Crushing Blow; Piercing Shot, Multishot and Poison Trap; Fireball, Blizzard and Chain Lightning. Passive and later-stage level requirements remain unchanged.
- Reset clears all ranks and equipment in the detached draft. Revert restores it; only Save changes the owned hero. A skill must receive a point before equipping it. Refunding its last point also removes its slot and attack-order entry.
- `ClassSkillTree` and `ClassSkillLoadout` own zero ranks and point accounting. Older saves and presets retain their original rules. Saving a reset allocation, reopening it and restarting the game never reintroduce a free starting rank.
- Legacy combat checks also read `ClassSkillTree.UnlockLevel`, so actual automatic casts do not remain locked behind the old level 3/6 requirements.

### Verification for this change

Unity 6000.6.0f1 Edit Mode passed 112 allocation/transaction/persistence/sharing/localization cases and 502 combat regression cases, with zero failures or skips in either run. The HTML tree's 39 tests and the shared UI ownership check with 9 regressions also passed.

In a macOS development player, uGUI pointer events verified reset, revert, investment and equipment of the second/third first-row active, slot-based policy navigation, save/reopen and last-point refunds for all three level-one classes. A separate player process restored all-zero allocations with one remaining point and empty slots. Existing version 3 investment, equipment, policy editing and fresh-process restoration also passed.

The 20-layout matrix combined 440×956 portrait, 956×440 landscape and 1600×900/1600×1000/2100×900 desktop with Korean/English and 100%/150% text. No Unity Editor instance was connected, so this task used the established batch test/build path. Production account saves were untouched. Physical mobile input and performance were not verified.

- [Validation summary](HuntEdictSkillResetEvidence/validation.json) · [Allocation/persistence tests](HuntEdictSkillResetEvidence/editmode.xml) · [Combat regressions](HuntEdictSkillResetEvidence/combat-editmode.xml) · [Build result](HuntEdictSkillResetEvidence/build.txt)
- [Pointer interactions](HuntEdictSkillResetEvidence/runtime.txt) · [Zero-rank restart](HuntEdictSkillResetEvidence/restart.txt) · [Existing allocation interactions](HuntEdictSkillResetEvidence/legacy-runtime.txt) · [Existing allocation restart](HuntEdictSkillResetEvidence/legacy-restart.txt)
- [Desktop Korean](HuntEdictSkillResetEvidence/desktop-ko.png) · [Portrait English 150%](HuntEdictSkillResetEvidence/portrait-en-150.png) · [Landscape Korean](HuntEdictSkillResetEvidence/landscape-ko.png) · [Landscape English 150%](HuntEdictSkillResetEvidence/landscape-en-150.png) · [Empty slots and refunded point](HuntEdictSkillResetEvidence/reset-empty-ko.png)

## 2026-09-29 compact menu beside equipped skills

Clicking an equipped normal active or ultimate opens a compact menu immediately above that socket. It falls below the socket if there is insufficient room above and stays inside the safe area at either horizontal edge. The screen remains undimmed. The menu contains only the skill name, close control, **Remove skill** and **Edit Hunt Edict**.

Clicking outside, closing or going back dismisses only this menu. An outside click is consumed rather than activating the underlying tab or button. Rotation, safe-area, language and text-size changes rebuild the menu against the same newly rendered socket. Opening or dismissing the menu never changes the draft or save; removal and policy editing retain their existing transaction paths.

### Compact menu verification

All 99 related Unity Edit Mode tests and 9 shared UI regressions passed, and the macOS development build completed without errors. The 20-layout matrix combined 440×956 portrait, 956×440 landscape, 1600×900/1600×1000/2100×900 desktop, Korean/English and 100%/150% text. Each of the five equipped slots was clicked in every layout, checking 100 menus for placement, size, text clipping and outside-click handling.

Verification also covered rotation, language and text-size changes while a menu remained open, simulated safe-area insets, policy navigation, active/ultimate removal, revert and save. A separate player process restored the removed slot as empty while retaining the other skills. Checks used synthetic uGUI pointer input in a native macOS development player with isolated saves. No connected Unity Editor or physical mobile device was used.

- [Validation summary](HuntEdictSkillMenuEvidence/validation.json) · [Edit Mode tests](HuntEdictSkillMenuEvidence/editmode.xml) · [Build result](HuntEdictSkillMenuEvidence/build.txt)
- [Pointer interactions](HuntEdictSkillMenuEvidence/runtime.txt) · [Fresh-process restoration](HuntEdictSkillMenuEvidence/restart.txt)
- [Desktop Korean](HuntEdictSkillMenuEvidence/desktop-ko.png) · [Portrait English 150%](HuntEdictSkillMenuEvidence/portrait-en-150.png) · [Landscape Korean](HuntEdictSkillMenuEvidence/landscape-ko.png) · [Right-edge ultimate, English 150%](HuntEdictSkillMenuEvidence/ultimate-edge-en.png) · [Simulated safe area](HuntEdictSkillMenuEvidence/safe-area.png)

## 2026-09-29 first-row active unlock level and a skill-less start

- The second and third first-row actives of every class (Warrior Leap Slam and Crushing Blow, Ranger Multishot and Venom Trap, Mage Blizzard and Chain Lightning) now also unlock at level 1 in the skill catalogs. The tree has opened them at level 1 since 2026-09-28, but the growth screen, training comparison, edict imports, recommended builds and some combat checks read the catalog's levels 3 and 6 and still locked or labelled them. The design catalog `Docs/Design/ClassSkills/catalog.json`, the legacy definitions in `GameCatalog.cs`, their frozen copy `Docs/Implementation/ClassSkills/legacy_definitions.json` and `Assets/HELLSCRIPT/Resources/GameCatalog.asset`, which the game scene actually reads, changed together. The asset was saved through the Unity Editor and only those six values differ. The generators rebuilt the class skill pages and the HTML prototype catalog, and the runtime `ClassSkills.json` matches the generator output byte for byte.
- All three characters of a player's brand-new account start on a zero-rank version-4 loadout: no learned or equipped skill, basic attacks only, and one point at level 1 to learn one first-row active. The game applies this through `GameStore.StartNewHeroesWithoutSkills` only when it created the account because no save existed; existing saves and accounts built directly by tests are unchanged. The prologue then teaches the first skill (Warrior Whirlwind, Ranger Piercing Shot, Mage Fireball) from this state.
- When the basic attack uses the resource-refill role, it still attacks if no paid active can run now (no skill, cooldown, turned off, locked or out of reach). Before, the hero stood still in these cases even at zero mana. It still refills the first eligible paid active that is short of resource and still holds back while that active can run now. The warrior shout keeps its existing refill check.
- The mage recommendation for level 1 before Blizzard and two growth-screen notes that assumed level-3 and level-6 unlocks were removed with their translations. Tests that used a locked skill as an example now use the level-10 actives that still unlock by growth. The early-rift fight fingerprints were regenerated because the level-1 warrior now also uses Leap Slam. The skill tree and quick preset smokes explicitly prepare an existing (version-3) save so they keep testing existing players' loadouts.

### Verification for this change

Unity 6000.6.0f1 ran the full Edit Mode suite on this branch merged with `main` (`857556da`) and on `main` at the same point (`20169eaf`). In the last full run the merge passed 4,687 of 4,735 tests and `main` passed 4,686 of 4,733. The one new failure was the blacksmith bonus test, whose locked example, Leap Slam, now opens at level 1; after switching the example to a level-10 active, its 43-test class passed again. The other 47 failures are shared, pre-existing failures. The class skill design, option and report generator checks, the 39 HTML prototype engine tests, the shared UI check and its 9 tool tests also passed. The runtime `ClassSkills.json` equals the output of `tools/class_skill_runtime.py build`.

A scratch probe ran a 60-second fight for each class. A level-1 hero with no skill killed 18–20 enemies with basic attacks alone even with its resource held at zero. With the basic attack in the resource-refill role, all three classes killed nothing before the fix and 18–20 enemies after it.

The macOS development build passed the tutorial smoke (prologue included, two processes), the five skill tree flows, the quick preset, class skill and attendance smokes. After the tutorial the Warrior had Whirlwind from the prologue and Ground Slam from the level-10 fixture equipped, while the Ranger and Mage still had no skill. The edict overview, character selection, title, preset, training, growth and equipment art smokes failed the same way on the pre-change build, so they are not attributed to this change. Physical mobile devices were not tested.

- [Validation summary](FirstRowSkillStartEvidence/validation.json) · [Edit Mode comparison](FirstRowSkillStartEvidence/editmode-comparison.json) · [Combat probe](FirstRowSkillStartEvidence/basic-attack-probe.txt) · [Early-rift fingerprints](FirstRowSkillStartEvidence/early-rift-fingerprints.txt)
- [Runtime smokes](FirstRowSkillStartEvidence/runtime-smokes.txt) · [New account skill tree](FirstRowSkillStartEvidence/new-player-skill-tree.png)
