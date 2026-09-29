# HELLSCRIPT first-play improvements

Updated: 2026-09-29

## Goal and scope

Help new players change equipment and skills, understand the resulting hunt, and choose their next action. Baseline: `246940407ea4ac3914285f4d59b27f2b91ea562c`. Branch: `codex/first-play-improvements`. [Portal recovery](First_Play_Recovery.en.md) is documented separately.

Normal menu/pause fatigue, damage, enemy health, prices, drops and unlock tiers remain unchanged. Loading a save does not reorder manually configured attacks. See the [2026-09-27 integration record](Main_Integration_20260927.en.md) for the subsequent merge with operational rewards, sound and APK optimization. Natural Rift 20 progression for all three classes remains in progress.

## Implementation and ownership

| Work | Behavior | Owners |
| --- | --- | --- |
| D02 skills | Newly equipped skills precede BASIC; replacements inherit the replaced position. Existing order and disabled automatic use are preserved. | `ClassSkillTree.Equip`, `ClassSkillDisplay`, `HuntEdictWindow.Skills`, `GlobalHudSnapshot`, `RiftEntryWindow.Popups` |
| D03 feedback | Record actual blocked-use and interruption reasons, exclusive primary-action time and observed support effects in existing combat records. | `CombatFeedback`, `CombatSimulation`, `CombatStatistics`, `GameUI.History` |
| D04 results | Show level, rewards, unlocks and one next action first. Logs and repeat conditions have dedicated detail screens. | `FirstPlayRecommendation`, `GameUI.FirstPlay`, `GameUI.Repeat` |
| D05 guidance | In-progress/completed/later tabs, one available action, one-condition class A/B practice and an equipment list for forge practice. | `Tutorials`, `ClassPracticeLesson`, `GameUI.Tutorials`, `GameUI.TutorialPractice`, `GameUI.Comparison` |
| D06 Equipment organization | Continuous review was removed on 2026-09-29. Use individual item details, comparison, locking and equipping. | `InventoryWindow.Dialogs`, shared equipment views and existing transactions |
| D07 rewards | Persist actual grants with the transaction and retain names, counts and equipment comparison after opening. | `RewardBoxReceipt`, `RewardBoxes`, `GameStore.RewardBoxes`, `RewardBoxesWindow` |
| D08 visibility | Start the live log collapsed with current action visible. Default the large map overlay off for new devices. Improve hero occlusion silhouette and shapes. | `GameUI.CombatJournal`, `OverlayMapSettings`, `WorldView`, `HeroOcclusion.shader` |
| D09 entry | Present goals, potions and skills first; normal 1× entry is primary. Keep fatigue details and paid-entry confirmation. | `RiftEntryWindow`, `RiftEntryRules` |

## Result screen shared UI adapter

The result screen continues to use the existing `GameUI` result/history adapter. The recommendation is a fixed action with a descriptive label such as Claim Boxes, Review Gear or Review Skills. On short landscape screens the same fixed action area becomes a right-side column, preserving the global HUD, growth summary and enlarged text. Equipment presentation and transaction ownership remain shared.

Entry and first-clear reward screens choose the next unlock from account-wide progression and preserved access. Switching classes no longer presents an already open service as a new goal; per-character admission limits remain unchanged. Results persist actual newly granted IDs at boss defeat in `CombatJournalData.openedContent`. Repeated stages on another class and older records do not invent new unlocks. The focused account-sharing, preserved-access, duplicate-defeat and serialization bundle passed 79 tests. The subsequent early-Ranger lesson and localization bundle passed 42 tests.

Class-practice A guidance and B condition pages use the same right-side fixed actions on short landscape screens. A native verification build reproduced the unreadable body viewport at 956×440 and 150% text. Layout now runs after creating the buttons, keeping the body and fixed actions separate. New practice pages start at the first explanation, and the concise Start B action remains readable with enlarged text.

The landscape entry preparation list retains its scroll offset when a save notification repaints it. Fatigue, potion and skill updates remain subscribed; they are not suppressed to hide the scroll issue. Native acceptance explicitly saves while scrolled down and opens the services button afterward.

## Skill values and combat evidence

Current descriptions reuse `SkillProgressionInfo` and existing class-skill rank calculations. Current ranks, bonus ranks, costs and base-rank descriptions are distinct. A skill after BASIC explains why it may not be evaluated; moving it modifies only the editor draft.

`CombatStatistics.feedback` is optional, version 1. Old records show missing data rather than invented zeros. Limits are 32 reasons per skill, 256 unique targets and 160 effect definitions, with truncation flags. Whirlwind counts one release on the first actual damage tick of each channel and shows channel ticks separately. The persisted action ID prevents counting another first release after restore. Non-skill events do not become casts. Time/support values use the same float serialization format as existing combat state. Skills skipped after a selected higher-priority action are explicitly counted as not evaluated. Instrumentation never re-evaluates conditions or consumes additional random values.

Attack, evade, move, gather and wait are mutually exclusive per combat tick. Menus and cleanup after combat are excluded. Effect uptimes can overlap and must not be added to primary-action duration. Mark targets and uptime, actual healing, shield absorption and resource restoration are reported without estimating hypothetical extra damage. Decision counts are not counts of available casts or attempts.

## Guidance and practice completion

Results and the guide journal use the same recommendation: capacity/progression blockers, unclaimed rewards, new skills, last failure and unspent points, subject to actual ownership and unlocks. Hidden/deferred optional guides retain their state. Reading does not complete practice. Opening current settings never rewrites an archived combat snapshot.

Class lessons use a copy of the current hero. Warrior changes ground-hazard avoidance, Ranger changes Multishot priority position, and Mage changes Blizzard priority position. Each 60-second A/B pair retains map, level, gear and random seed. Required skills must be unlocked, equipped and automatic. H17 completion requires an observed release or behavior difference; identical outcomes do not pass. Saving and applying a tested setup require a separate explicit choice. Loading an attempt into the editor also restores its class-skill order, ranks and choices. Short tutorial prompts and receipts use the common content window’s fitted height; long content remains scrollable with fixed actions.

The first natural level-4 Ranger pair had no Multishot releases even before BASIC: higher-priority Piercing Shot consumed the resource first. The unchanged pair was preserved without practice completion. The Ranger proposal now moves only Multishot to first priority, or after BASIC if already first, preserving every other relative position. The B screen labels actual A/B conditions in KO/EN rather than concatenating internal skill IDs. Compact/expanded current-action presentation now also lays out correctly in A/B training, which has no full journal.

After integration, the natural level-4 Mage also produced identical outcomes in the old keep/relocate Blizzard lesson: 43 Fireball releases, 32 basic attacks, and no Blizzard releases. Early Mage practice now moves only Blizzard to first priority, or after BASIC if already first. Other relative positions, selected skill options, gear and ranks are retained; existing saved builds are not rewritten. The original unchanged pair is preserved.

The corrected lesson was repeated at normal 1x for 60 seconds per attempt with the same level-4 Mage and identical equipment. A had zero Blizzard releases and 10,719.93 total HP damage; B had eight Blizzard releases and 17,064.92 total HP damage. Fireball changed from 43 to 34 releases and basic attacks from 32 to 37. Practice completion and the comparison were explicitly saved, then B was loaded into the edict draft and saved. Readback confirmed the original order remained before that save and Blizzard moved first only afterward. This multi-target result does not establish the same damage increase in every Rift. The [Mage practice evidence](FirstPlayEvidence/mage-practice.json) preserves both outcomes and source hashes.

## Equipment and reward persistence

Continuous review and its previous/next/storage flow were removed on 2026-09-29 to expand the bag grid. Individual detail, comparison, lock and equip actions retain shared views and GameStore validation. Older continuous-review evidence below records the former behavior; current acceptance uses individual item comparison and locking. See [the current inventory layout](Potion_Slots.en.md).

Receipts originate in the actual grant branches, not wallet differences. They cover equipment, gems, runes, enhancement stones, crafting materials, gold, premium currency, slot cores and operational potion supplies. The existing reward state retains the latest 20 openings, available after restart from Recent openings in the reward-box window. Retrying the same request returns its persisted receipt without granting again. Save failure commits neither rewards, box consumption nor a receipt. Receipts outside the retention limit are not reconstructed through another random roll.

## Visibility and remaining art work

The long white lines were the `RiftAutomap` boundary overlay, not a confirmed navigation debug renderer. Existing explicit visibility preferences are preserved. Collapsed logging is a device display preference; recording continues. Occlusion uses only the existing hero meshes and does not reveal enemies or unexplored areas.

| Remaining asset | Current state | Follow-up requirements |
| --- | --- | --- |
| Three class combat appearances | Primitive meshes with class weapons, colors and silhouette changes | Approved combat models or directional sprites matching the camera |
| Movement, preparation, attacks, hits and death | Existing procedural presentation and effect timing | Frame mapping to actual combat events |
| Early enemy roles and bosses | Existing role colors/shapes and adjusted boss proportions | Distinct combat assets for melee, ranged, support and charging roles |
| Skill effects | Existing release/hit event connections | Approved effects with clear ranges and a measured performance budget |

Character-selection illustrations were not repurposed as combat animations. This is not a full art replacement. No new images were generated.

## Verification status

The [test scopes, counts and source-result hashes](FirstPlayEvidence/verification.json) are retained. Overlapping focused runs must not be summed.

- Baseline related tests: 159 passed.
- Portal recovery: 10 focused tests passed; both preserved blocked Warrior/Mage saves resumed and settled through macOS UI copies.
- Skill equip/order: 8 focused tests passed.
- Feedback, reward, training, history and inventory bundle: 127 passed.
- The full Edit Mode run had 4,009 tests: 3,957 passed and 52 failed. Running the 334 related tests in a separate exact-baseline checkout confirmed 47 pre-existing failures. Four introduced floating-point serialization round-trip failures and one obsolete localization key were fixed. The final focused 89 tests and 33 result-localization/shared-UI tests passed. This is not a claim that the full suite is green.
- An isolated macOS fixture verified five continuous comparisons, protection/storage, batch receipt and process-restart persistence. Captured 20 combinations of five resolutions, KO/EN and 100/150% text. The compact log inset bug was fixed and current-action rendering was recaptured. All 20 final-build combinations passed rendered bounds, fixed-action text fit and first-screen growth visibility checks. The recommended action opened its destination through a raycast pointer. A restarted process reopened the persisted receipt through UI. See [native checks](FirstPlayEvidence/checks.txt) and [restart checks](FirstPlayEvidence/restart.txt).
- Natural progression rotates Warrior, Ranger and Mage on a new shared account. This is in progress, not a claim of three Rift 20 completions.
- The post-integration Mage follow-up passed all 92 related Edit Mode tests. Native macOS acceptance verified A/B guidance across 20 screen combinations, 40 captured pages and an actual start-button pointer action. Preserved [layout checks](FirstPlayEvidence/mage-practice-layout-checks.txt), [landscape English at 150%](FirstPlayEvidence/mage-practice-landscape-en-150.png) and [portrait Korean at 150%](FirstPlayEvidence/mage-practice-portrait-ko-150.png). Disposable layout fixtures are separate from natural-progression evidence.

macOS synthetic input is distinct from physical mobile, physical input, sound and human comprehension acceptance. No physical-mobile or release-build performance pass is claimed.

## Representative screens

![Landscape result at 150 percent English text](FirstPlayEvidence/result-956x440-en-150.png)

![Portrait result at 150 percent Korean text](FirstPlayEvidence/result-440x956-ko-150.png)

![Persisted receipt reopened after restart](FirstPlayEvidence/receipt-reopened-after-restart.png)

## Measured cost on one device

Three 20-second captures per build used an Apple M3 Pro, macOS Development Mono, 1280×720, normal 1x, and identical stage-30 map, equipment, seed and live-operations configuration. Values are medians of per-trial metrics. The [measurement summary](FirstPlayEvidence/performance.json) is retained. These level-30 fixtures are not natural-progression evidence.

| Metric | Before | After |
| --- | ---: | ---: |
| Median frame time | 16.82ms | 16.70ms |
| Frame time p95 | 24.75ms | 23.99ms |
| UI mean per frame | 0.189ms | 0.050ms |
| Combat mean per frame | 1.031ms | 1.206ms |
| World presentation mean per frame | 0.241ms | 0.260ms |
| Mean GC allocation per frame | 229.4KiB | 221.6KiB |
| Peak process RSS | 630.0MiB | 614.9MiB |

Every trial observed 401 actual combat ticks. Increased combat work is reported separately from frame-time changes. This short desktop sample does not prove mobile, thermal or battery performance, or absence of memory leaks. Draw Calls and Batches returned only zeros and were excluded from conclusions. Both harnesses received identical asynchronous-admission waiting and valid main-weapon fixture fixes. Baseline gameplay code was preserved; the two profiling-harness source hashes match.

## Natural-play follow-up: readable death sources

The actual Mage Rift 6 defeat exposed `BOSS_BASIC`, `BOSS01_SLAM`, and `BOSS01_HOOK` as attack labels. The journal's existing attack-name mapping is now shared through `CombatJournal.AttackName` by death summaries and tick details. Known raw identifiers in old records are resolved only for display; stored names, damage, HP loss, and grouping keys remain unchanged. Unknown identifiers and custom stored labels are preserved. Korean and English reuse the existing translations.

Native verification also reproduced a collapsed content viewport at 956×440 with enlarged text. Death summaries, final windows, and tick details now use the result screen's fixed side actions on short landscape screens and reflow after constructing those actions. Newly opened pages start at the beginning.

All 65 focused Edit Mode tests passed. A copy of the actual defeat record was displayed in 20 macOS combinations, with 40 summary/detail captures and raycast back-action checks. Visual inspection caught a tutorial overlay missed by the first fixture assertions; that fixture was corrected and rerun. Final evidence: [verification](FirstPlayEvidence/death-review-verification.json), [layout checks](FirstPlayEvidence/death-review-checks.txt), [landscape English 150%](FirstPlayEvidence/death-review-landscape-en-150.png), and [portrait Korean 150%](FirstPlayEvidence/death-review-portrait-ko-150.png). This does not complete the three-class natural Rift 20 run or establish physical-mobile acceptance.
