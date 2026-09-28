# Hunt edict skill tree integration

Updated: 2026-09-27 · [한국어](Hunt_Edict_Skill_Tree.md)

## Screen and interaction

The Hunt Edict **Skills** tab shows all 37 skills of the current class, arranged by level stage and three branches. Selecting an icon displays effects, unlock requirements and ranks. Development skill IDs in descriptions are replaced by localized skill names.

There are four equipped normal active slots and one separate ultimate slot. At most one ultimate can be selected. Every unlocked passive applies automatically, so there is no passive equipment area. The combat HUD reads the same four normal slots, separate ultimate slot and actual cooldowns.

Selecting an equipped active opens **Remove skill** and **Edit hunt edict**. Editing opens that skill's automatic use and policy settings. Returning to the tree preserves the draft. The separate skill-edict subtab is removed. Basic attack and attack priority remain available under **Attack settings**.

Landscape puts the tree on the left, the inspector at the top of the right column and the skill bar beneath it. Portrait puts the tree on top, fixes the skill bar at the bottom and opens the inspector between them when a skill is selected. Equipped slots, main navigation and save controls stay fixed. The tree, details and policy content scroll separately. The per-skill policy page also keeps the skill bar at the bottom. Korean, English and larger text are supported.

## Traditional hack-and-slash tree (2026-09-27 redesign)

The user found the old tree cheap: it listed each skill in a box, and the icons sat against the top of their boxes and looked misaligned. It was rebuilt as a traditional hack-and-slash skill tree, with mobile action RPGs and Diablo III/IV as references. The Diablo IV and III screens were reviewed in the in-app browser; no other game's art or screens were copied.

- **From Diablo IV**: branches that light up along unlocked paths, diamond gates between stages, a rank label under each icon (`2/5`), the inspector beside the tree and a remaining-points display.
- **From Diablo III**: round, ornately framed skill sockets and stage titles with ornamental rules.
- **From mobile games**: a bottom-opening inspector in portrait, the skill bar fixed at the bottom, unlock levels on locked skills and large touch areas.

Each of the three branches is a vertical trunk. Level-only skills hang from the trunk; a skill whose prerequisite is in the same stage hangs directly below it with a link. Trunks and links glow brass up to the hero's level. Stages not yet reached are shaded and each skill there shows its unlock level, such as `Lv.26`. Ultimates sit at the end of their trunk in the last stage. Selecting a skill draws bright links to its prerequisites and to the skills it opens, including ones in other stages or branches.

Actives use a round frame, passives a square frame and ultimates a four-pointed crest. The frames, tree backdrop and point gem were generated with GPT at the user's request. Icons are centered in each frame's measured transparent opening, so frame and icon no longer drift apart. Upgraded and equipped skills glow softly, the selected skill glows brightly, and equipped skills carry their slot number (a diamond for the ultimate). An outline around the selection made the node look like a box again, so state is shown with light only. See the [skill tree screen art record](../Art/SkillTreeUi/Skill_Tree_UI_Art.en.md).

Saving and rules are unchanged. `ClassSkillTree` and the draft still decide unlocks, ranks and equipment; the new layout (`SkillTreeLayout`) and ornament mesh (`SkillTreeGraphic`) only draw. Existing button names are kept, so tutorial guidance and other smokes find the same buttons.

## Progression and combat

`tools/build_skill_tree.cjs` compiles the levels and prerequisites from `Prototypes/SkillTree/tree-design.cjs` into the native `ClassSkillTree.json` resource. HTML and native progression do not maintain separate handwritten graphs. Base rank one is free once unlocked. Upgrades receive one point per level after level one: 39 points at level 40.

Direct effect dependencies remain required. Ultimates require level 40 and any unlocked skill in the preceding level 30–39 stage. Unlocking a prerequisite does not require equipping it in an active slot.

The existing six passives are evaluated by `HeroStats`; expanded passives use the existing combat effect owners. Always active means available once unlocked; their hit, freeze and other trigger conditions remain intact.

## Persistence and compatibility

The detached `HuntEdictEditSession` draft is validated and atomically saved by `GameStore.CommitHuntEdict`. Ranks, four slot positions, ultimate, automatic use, skill policies, attack order and global edict settings are saved together. Reflow and language changes never save.

Player configurations use `ClassSkillLoadout` version 3. Production startup atomically upgrades owned heroes and preserves the original build. Investments below the new unlock level are refunded to free base rank. Existing version 1–2 development fixtures retain their original meaning. A legacy suspended run retains its snapshot; explicitly editing its skills uses the existing live-change transaction to adopt the tree.

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
