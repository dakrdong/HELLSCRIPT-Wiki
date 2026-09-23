# Hunt edict skill tree integration

Updated: 2026-09-23 · [한국어](Hunt_Edict_Skill_Tree.md)

## Screen and interaction

The Hunt Edict **Skills** tab shows all 37 skills of the current class, arranged by level stage and three branches. Selecting an icon displays effects, unlock requirements and ranks. Development skill IDs in descriptions are replaced by localized skill names.

There are four equipped normal active slots and one separate ultimate slot. At most one ultimate can be selected. Every unlocked passive applies automatically, so there is no passive equipment area. The combat HUD reads the same four normal slots, separate ultimate slot and actual cooldowns.

Selecting an equipped active opens **Remove skill** and **Edit hunt edict**. Editing opens that skill's automatic use and policy settings. Returning to the tree preserves the draft. The separate skill-edict subtab is removed. Basic attack and attack priority remain available under **Attack settings**.

Landscape places the tree beside the inspector; portrait puts details below the tree. Equipped slots, main navigation and save controls stay fixed. The tree, details and policy content scroll separately. Korean, English and larger text are supported.

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
