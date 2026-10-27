# Hunt Edict: Tactical Skill Options

Date: 2026-10-22 · Version: v1.0 draft · **Design only / not implemented**

[한국어](HELLSCRIPT_Hunt_Edict_Tactical_Options.md)

Scope: **30 actives (24 normal, 6 ultimate)** and **84 parent-skill modifiers**. Players choose where, against whom and in what sequence to use skills. This does not add upgrades to damage, range, duration or cooldown.

Each parent card has 4–5 controls including automatic use. Only the selected modifier adds two rows or replaces incompatible parent rows. **Maximum editable controls per card, including its modifier: seven.** Disabled or collapsed numeric controls count separately. Modifiers gain no independent autocast toggle, cooldown or loadout slot.

This is a follow-up proposal for the current D4 inventory. It does not change the implemented version of the [v0.2 catalog](HELLSCRIPT_Hunt_Edict_Skill_Option_Catalog.md), [implementation contract](HELLSCRIPT_Hunt_Edict_Implementation_Contract.md) or [save/preset contract](HELLSCRIPT_Hunt_Edict_Detail.md). The legacy 18 actives, three basics and 89 controls do not describe the full D4 inventory. Implementation must revise those contracts together after review.

## 1. Single ownership of controls

- **Shared globals:** target grade/type/status priorities, chase, engagement distance, gathering, own-area holding, danger/health/emergency-resource/escape reserves and shared execution order.
- **Parent skill:** automatic use, unique tactic, tactical count/location and one selected follow-up. Do not expose native use menus alongside the new tactical menu.
- **Selected modifier only:** two controls reflecting its actual shape. Replace incompatible parent rows; preserve but do not apply the old values in that shape.
- **Retained unique exceptions:** W04/M06 boss use, W06 noncombat refill and M04 strict destination requirements. Do not force distinct permissions into one shared value.
- **Automatic/read-only:** actual hit prediction, effect caps, remaining cooldown/charge/mark/fields and disabled reasons. No tick-count, hit-ratio, prediction-error or custom-formula editor.

`HET-*` are **design row IDs**, not newly implemented save keys, runtime code or share formats. Each row is one choice or number; no hidden sliders beneath a choice.

## 2. Common semantics

| Item | Decision |
|---|---|
| Automatic use | Parent row `-01`: On/Off, default **On** for a valid newly configured equipped skill. Imports preserve Off. Unequipped/locked skills cannot execute. Parent Off outranks emergency designation. |
| Choice | Single selection. Defaults belong to new drafts and never overwrite existing settings. |
| Numeric | Every numeric row specifies bounds, unit, step and default. Apply only within actual shape/rank caps. |
| Ordinary tactics/preferences | Preferences, not gates. Missing candidates fall back to ordinary targeting/current safe position, then another eligible action. A missed count never stalls all hunting. |
| Only/wait | Explicit unmet requirements defer **this skill only**. Other attacks, generators and survival continue. |
| Free | Use existing shared order and global target/position rules; no second priority editor. |
| Skill references | Enable only actual equipped, unlocked, auto-permitted same-class skills in the required form. Missing optional defaults become Free/None; mandatory defaults use the first eligible shared-order item. With no eligible reference, disable the tactic with a reason. |
| Context changes | Gray out unsupported choices with reasons. Stored settings for another modifier never apply until that modifier is actually selected. |
| Changed caps | A requested seven victims with a rank cap of three displays an effective cap of three. Preserve the original request without granting extra effect. |

**Evaluation order:** parent automatic permission → actual action lock/ownership/cost/cooldown/range/line of sight/legal landing → shared emergency/dodge/reserve rules → explicit skill permissions → tactical preference/link → ordinary shared order. Show why an emergency-designated skill is Off and evaluate another valid survival action.

A link reserves **one next action only**. Distinguish actual release, hit, arrival, control, kill and charge events; failed events are not setup. Revalidate and cancel stale reservations. Linked actions cannot recursively reserve new links; return to ordinary shared order. Prefer at most one setup action per reservation, preventing setup loops.

If parent and modifier request different follow-ups on the same event, prefer **one follow-up from the selected modifier**. Modifier layout preferences outrank parent layout preferences for that shape, but never bypass parent Off, boss-use permission or shared survival. Keep a valid reservation until execution/cancellation; do not stack or continually overwrite it with simultaneous preferences.

Positioning tactics choose among **existing permitted movement candidates**. Extra offensive travel is bounded by the smaller of the row's allowance and global limits; absent a local limit, globals apply. If offensive movement is forbidden, use feasible tactics from the current position. No movement ability is granted to stationary spells.

Own fields, marks, bleeds and slows require real owned effects. Expired or replaced shapes are not anchors. Movement paths use observation, never unseen enemies or certain futures. Only actual gear, sets and passives contribute effects; no unowned repeat hits, freezing, charging or following.

**Deadlock prevention:** eligibility for setup/generation cannot require the buff or charge it will create. Channel follow-ups revalidate after normal termination. Release charge holding if its consumer is absent or cannot act within the buff. If holding blocks the sole basic/generator needed to pay the consumer, allow one generation action and re-evaluate.

## 3. Merging existing edicts

These are **design destinations**. Direct mapping describes one field's semantics, not a successful whole-preset conversion. Preserve other originals/versions and show differences in a conversion draft. Never silently relax strict gates into preferences or treat different charge types as one value.

| Group | Existing IDs/menus | New owner | Merge and exception |
|---|---|---|---|
| Auto use | 모든 HEC-*-01 | 모든 HET-*-01 | Preserve ON/OFF exactly. Parent Off also forbids automatic modifier use. |
| Target and chase | target.priorityGrade/Type/Status/Categories, target.switch/chase*, position.* | 기존 전역 / Existing globals | Reuse global grades, types, pursuit, distance, gathering and own-area rules. Skills arrange allowed candidates; focusing primary is not a new grade filter. |
| Emergency protection | dodge.*, survival.* | 기존 전역 / Existing globals | Do not duplicate health, detected danger, emergency resource or escape reserves. Offensive leap/retreat/teleport obey shared entry permission and reserves. |
| Whirlwind | HEC-W01-02/03/04; channel.resourceStop/resourcePercent/emptySeconds; FOLLOW/STAND | HET-W01-02/03/04 | Merge start, route and yield into tactic/victim goal/follow-up. Three-second slicing, custom resource floor and equipment-charge waiting are not equivalent: preserve originals for reviewed conversion. Keep existing empty-area protection. |
| Leap | HEC-W02-02/03/04/05; SPACE | HET-W02-02/03/04/05 | Role and minimum distance (2–8m, 0.5m step) map directly. Matching DENSE/EDGE placement and W01/W03 links are conversion candidates; TARGET does not automatically become back-line entry. Review SPACE separately. |
| Crushing Blow | HEC-W03-02/03/04; aim.direction; READY/CRUSH_CHARGE | HET-W03-02/03/04 | Merge target layout, aim and empowerment into tactic/layout/preparation. Charge-only is stricter than prefer-charge and must not be silently relaxed. |
| Ground Slam | HEC-W04-02/03/04; control.reuse; GROUP/INTERRUPT | HET-W04-02/03/04/05 + 기존 전역 위협 판단 / shared threat rules | Control reuse belongs to mission; group goal and follow-up have single owners. Retain DAMAGE/STAGGER/COMPLETE in row 05. Interrupt-only/retreat-only differ in scope and need review. |
| Iron Wall | HEC-W05-02/03/04; TOGETHER/AFTER | HET-W05-02/03/04 | Timing merges into protection mission. Identical overlap settings map to the new row; mixed shield/leap overlap requires review. |
| Battle Shout | HEC-W06-02/03/04/05; RESOURCE/DAMAGE | HET-W06-02/03/04/05 | Merge purpose and timing into tactic/supported attack/planned casts. Keep noncombat ON/OFF in 05. Per-skill encounter grades and BOTH may differ from global behavior and require review. |
| Pierce Shot | HEC-A01-02/03/04; aim.direction; ONE/TWO | HET-A01-02/03/04 | Line and coverage merge into tactic and victim goal. Strict two-victim gating is not equivalent to preferring two. Review old OWN_POISON as a per-skill status preference. |
| Multishot | HEC-A02-02/03/04; aim.focus/arrows; GROUP/FOCUS | HET-A02-02/03/04; HET-A02-3-01/02 | Parent owns normal tactic; same-victim arrow preference appears only with real modifier/gear support, reusing the same two rows instead of adding another panel. |
| Poison Trap | HEC-A03-02/03/04/05; area.preserve/overlap; KEEP/REPLACE | HET-A03-02/03/04 | Merge placement/opportunity, cap retention and overlap into three parent rows. Turret hides all three; strict new-area gates differ from preferences and require review. |
| Retreat Leap | HEC-A04-02/03/04; SAFE/OWN_TRAP | HET-A04-02/03/04 + 기존 전역 보행 / shared walking | Trap/pierce links merge into mission, landing into anchor and shooting into follow-up. Survival-only/normal-combat OFF and forced post-landing walking need review. |
| Hunter Mark | HEC-A05-02/03/04; mark.transfer/remaining/seconds; KEEP/NEW_TARGET | HET-A05-02/03/04; HET-A05-3-01/02 | Mission owns purpose/timing, retention owns recasting, modifier owns hero behavior after effect transfer. Distinguish recasting from death transfer. Review custom refresh seconds and HIGHER_PRIORITY. |
| Imbuement | HEC-A06-02/03/04; buff.chargeCheck/charges; followup.ready/resourceShots; BASIC 강화 보존 | HET-A06-02/03/04 | One owner for consumer choice, charge assignment and readiness. Old buff.charges gates reapplication; new reserve allocates consumption, so equal numbers do not map automatically. Old resourceShots budgets paid casts, not buff charges. |
| Fireball | HEC-M01-02/03/04; STEADY/GROUP; CURRENT/PREDICTED | HET-M01-02/03/04 | Merge purpose/layout into tactic and anchor; use actual automatic aiming. Fixed CURRENT aiming and set-exposure-only require review. |
| Blizzard | HEC-M02-02/03/04/05; area.preserve; KEEP/REPLACE | HET-M02-02/03/04; M02-1 문맥 / context | Merge retention, replacement and overlap into parent rows. LM01 target-following and M02-1 hero-following differ; display actual ownership and never auto-convert. |
| Chain Lightning | HEC-M03-02/03/04; charge.check/count; SINGLE/LINKED | HET-M03-02/03/04; 실제 강화 상태 / actual empowerment | UNIQUE/TOTAL_HITS maps to objective. Solo permission, strict linked-only and equipment-charge holding require review. Orb charge and chain empowerment are distinct. |
| Teleport | HEC-M04-02/03/04; escape.improvement; SURVIVAL/DISTANCE | HET-M04-02/03/04/05 | Merge role/layout into mission and anchor; retain NO_DAMAGE in 05. Keep actual movement-improvement protection; review stricter old exceptions. |
| Frost Nova | HEC-M06-02/03/04; control.reuse; charge.check/count; HERE/APPROACH | HET-M06-02/03/04/05 | Merge control mission and actual-effect follow-up. Retain boss permission in 05. Do not expand approach permission or turn chain empowerment goals into orb charge. |
| Legacy independent shield | HEC-M04-3-*; survival/explore의 M04-3 참조 | D4 M04-3과 분리 보존 / preserve separately | The same ID represents different owners: old independent active and D4 teleport modifier. Preserve original/version; show old references as unavailable legacy shield, never teleport permission. |
| Three basic attacks | HEC-WARRIOR/RANGER/MAGE-*; basic.keepDiscountTarget/preferEnhancedSkill | 기존 기본 공격 카드 / existing basic cards | Do not count basics again. Keep their role, discount-target and aim controls. Ranger empowerment holding reads A06 allocation; conflicting originals require review. |
| Native options of added actives | W08 ANY/DISTANT; W09 NEW_DOT/ELITE; W17 ELITE/LOW_HEALTH; W18 GROUP/SHOUT/ELITE; A07 ANY/RESERVE; A14 ANY/GROUP; A17 ELITE/CONTROLLED_GROUP; A18 ANY/RESERVE; M07 ANY/GROUP; M08 ANY/CONTROLLED; M11 ANY/CHAIN_READY; M17 ELITE/ELEMENT_BUFF/GROUP; M18 ELITE/CYCLE_READY | 각 HET 부모 전술 / respective parent tactics | Do not expose native use menus alongside new tactics. Only matching roles are conversion candidates. Preserve per-skill resource floors (A07 0.4/A18 0.7), low-health summoning and strict group/cycle/element gates for explicit review. |

### Inventory and control counts

Each class has 10 actives and 28 modifiers. There are **293 design rows** (125 parent definitions, 168 modifier definitions), never shown all at once. Basics, passives and globals are not counted again. Counts below include contextual replacement.

| Skill | Parent card | With selected modifier |
|---|---:|---:|
| [W09 · Raking Wound](#w09) | 4 | 4–6 |
| [W08 · Impaling Hook](#w08) | 4 | 3–6 |
| [W01 · Whirlwind](#w01) | 4 | 6–6 |
| [W03 · Crushing Blow](#w03) | 4 | 6–6 |
| [W05 · Iron Wall](#w05) | 4 | 6–6 |
| [W06 · Battle Shout](#w06) | 5 | 4–7 |
| [W02 · Leap Slam](#w02) | 5 | 7–7 |
| [W04 · Ground Slam](#w04) | 5 | 6–7 |
| [W17 · War of the Ancestors](#w17) | 4 | 4–6 |
| [W18 · Titan's Judgment](#w18) | 4 | 3–6 |
| [A07 · Successive Shots](#a07) | 4 | 4–6 |
| [A14 · Forking Arrow](#a14) | 4 | 4–6 |
| [A01 · Piercing Shot](#a01) | 4 | 4–6 |
| [A02 · Multishot](#a02) | 4 | 6–6 |
| [A04 · Retreat Leap](#a04) | 4 | 3–6 |
| [A03 · Venom Trap](#a03) | 4 | 3–6 |
| [A06 · Elemental Imbuement](#a06) | 4 | 6–6 |
| [A05 · Hunter's Mark](#a05) | 4 | 6–6 |
| [A17 · Killing Rain](#a17) | 4 | 5–6 |
| [A18 · Afterimage March](#a18) | 4 | 3–6 |
| [M07 · Elemental Lance](#m07) | 4 | 6–6 |
| [M01 · Fireball](#m01) | 4 | 3–6 |
| [M02 · Blizzard](#m02) | 4 | 3–6 |
| [M03 · Chain Lightning](#m03) | 4 | 5–6 |
| [M06 · Frost Nova](#m06) | 5 | 4–7 |
| [M04 · Teleport](#m04) | 5 | 7–7 |
| [M08 · Firewall](#m08) | 4 | 4–4 |
| [M11 · Capacitor Orb](#m11) | 4 | 6–6 |
| [M17 · Triune Collapse](#m17) | 4 | 6–6 |
| [M18 · Sage Incarnate](#m18) | 4 | 6–6 |

## 4. Warrior

<a id="w09"></a>

### W09 · Raking Wound

Choose between spreading bleeds and repeatedly cutting one target.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-W09-01` | Automatic use | On / Off | **On** | Sole automatic-use owner; Off applies to modifiers, links and emergency designation. |
| `HET-W09-02` | Raking tactic | Spread bleed / Hold primary / Prepare spender | **Spread bleed** | Spread favors enemies without your bleed; hold keeps the current target; prepare favors victims inside the follow-up footprint. |
| `HET-W09-03` | Spread goal | 1–5 enemies · step 1 enemies | **3 enemies** | In spread mode, yield to normal order once this many living enemies carry your bleed; resource generation may still use the skill. |
| `HET-W09-04` | Prepared attack | Free / W01 / W03 | **Free** | Prepare makes one preceding rake only when the chosen attack is eligible; otherwise use hold primary. |

No bleed, resource or damage tuning; stack sustainability needs later combat validation.

<a id="w09-1"></a>

#### W09-1 · Blood Reclaim

Adds these two controls to the parent. Card with this modifier: **6 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-W09-1-01` | Reclaim tactic | Favor healing / Cash out bleed / Normal raking | **Favor healing** | On the actual third cast, healing favors more bleeding victims; cash out favors greater remaining own bleed. |
| `HET-W09-1-02` | Healing victims | 1–5 enemies · step 1 enemies | **2 enemies** | Preferred victim count for healing; insufficient victims do not stall combat. |

Cast cadence, healing and bleed consumption remain skill-defined.

<a id="w09-2"></a>

#### W09-2 · Deep Wound

Replaces parent rows `HET-W09-02`, `HET-W09-03`. Card with this modifier: **4 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-W09-2-01` | Wound upkeep | Stack one enemy / Move to new primary | **Stack one enemy** | Stack keeps the existing living victim; move attacks the new global primary without transferring existing bleed stacks. |
| `HET-W09-2-02` | Desired stacks | 1–3 stacks · step 1 stacks | **3 stacks** | Prefer another rake toward actual stacks; never assume stacks that cooldown or bleed duration cannot sustain. |

This single-target variant replaces spread tactic/goal with wound upkeep/desired stacks.

<a id="w09-3"></a>

#### W09-3 · Spinning Rend

Adds these two controls to the parent. Card with this modifier: **6 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-W09-3-01` | Spin position | Between enemies / Receive approaching enemies / Beside primary | **Between enemies** | Favor positions covered by the real circular hitbox; entry obeys global approach and safety. |
| `HET-W09-3-02` | Surround goal | 1–6 enemies · step 1 enemies | **3 enemies** | Use actual nearby enemy count as a preference; otherwise cast from the current safe position. |

Spinning Rend grants no movement ability.

<a id="w08"></a>

### W08 · Impaling Hook

Choose where to retrieve an enemy and what to do afterward.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-W08-01` | Automatic use | On / Off | **On** | Sole automatic-use owner; Off applies to modifiers, links and emergency designation. |
| `HET-W08-02` | Hook mission | Isolate one / Retrieve into own delayed hit/control / Catch departing enemy | **Isolate one** | Isolate chooses one victim; retrieve favors enemies before an actual own aftershock/quake/control point; catch favors observed departure. |
| `HET-W08-03` | Retrieval anchor | Current position / Own W04 control point / Own W03 aftershock / Own W02 quake | **Current position** | The pull still ends at the hero; use an active chosen effect only when the hero can stand safely beside it. |
| `HET-W08-04` | After pull | Keep primary / Attack pulled victim once / Follow with W03 | **Keep primary** | Apply one follow-up preference after a successful pull; failure returns to normal order. |

Absent effects fall back to hero position; base W02/W03 gain no modifier-only delayed zone.

<a id="w08-1"></a>

#### W08-1 · Lunging Hook

Replaces parent rows `HET-W08-02`, `HET-W08-03`, `HET-W08-04`. Card with this modifier: **3 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-W08-1-01` | Dash purpose | Reach back line / Return to primary / Enter beyond own field | **Reach back line** | The hero moves instead of pulling, so retrieval anchor and after-pull rows are inactive. |
| `HET-W08-1-02` | Entry follow-up | Free / W01 / W03 / W04 | **Free** | One follow-up after a safe arrival; offensive-entry bans and emergency reserves veto the dash. |

Replaces all three parent pulling mission, anchor and after-pull rows.

<a id="w08-2"></a>

#### W08-2 · Piercing Hook

Adds these two controls to the parent. Card with this modifier: **6 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-W08-2-01` | Piercing route | Cross a line / Retrieve last victim / Include primary | **Cross a line** | Select a legal path within the real pierce cap; only the last victim is pulled. |
| `HET-W08-2-02` | Path victims | 1–7 enemies · step 1 enemies | **3 enemies** | Effective maximum is the current rank cap; otherwise fall back to a one-victim path. |

Cannot freely replace the final victim or pull every pierced enemy.

<a id="w08-3"></a>

#### W08-3 · Branding Hook

Adds these two controls to the parent. Card with this modifier: **6 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-W08-3-01` | Brand use | Suppress threat / Collect kill resource / Keep current brand | **Suppress threat** | Suppress follows global threat candidates; collect favors observed low-health victims. |
| `HET-W08-3-02` | After branding | Existing primary / Focus branded victim / W03 | **Existing primary** | One preference after actual application; no guaranteed kill or advance resource. |

Damage reduction and kill resource remain skill-defined.

<a id="w01"></a>

### W01 · Whirlwind

Choose sweeping, receiving enemies or a short spin before another attack.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-W01-01` | Automatic use | On / Off | **On** | Sole automatic-use owner; Off applies to modifiers, links and emergency designation. |
| `HET-W01-02` | Whirlwind tactic | Sweep pack / Receive approach / Spin then link | **Sweep pack** | Sweep follows a safe pack; receive spins at the current front; link checks a follow-up after the victim goal. |
| `HET-W01-03` | Sweep victims | 1–6 enemies · step 1 enemies | **3 enemies** | Distinct enemies actually hit during this channel, not tick count. |
| `HET-W01-04` | After spinning | Free / W02 / W03 / W04 | **Free** | In link mode, finish normally at the goal and prefer one follow-up; otherwise retain existing channel-end rules. |

Emergency resource and empty-area exits outrank links; ending grants no assumed refund.

<a id="w01-1"></a>

#### W01-1 · Swift Whirl

Adds these two controls to the parent. Card with this modifier: **6 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-W01-1-01` | Fast route | Circle pack edge / Move between packs / Follow primary | **Circle pack edge** | Choose routes at actual movement speed within global chase limits. |
| `HET-W01-1-02` | Next-pack reach | 0–4 m · step 0.5 m | **2 m** | Maximum extra travel beyond the current pack edge; 0 stays within it. |

The modifier owns speed; the edict only chooses routes.

<a id="w01-2"></a>

#### W01-2 · Wide Whirl

Adds these two controls to the parent. Card with this modifier: **6 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-W01-2-01` | Wide-area position | Cover from edge / Stand among enemies / Primary plus neighbors | **Cover from edge** | Evaluate only candidates inside the actual enlarged radius. |
| `HET-W01-2-02` | Desired coverage | 1–6 enemies · step 1 enemies | **3 enemies** | Preferred positioning coverage; no extra range. |

Sweep victims controls yielding; desired coverage controls position.

<a id="w01-3"></a>

#### W01-3 · Rapid Whirl

Adds these two controls to the parent. Card with this modifier: **6 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-W01-3-01` | Rapid-spin use | Pressure primary / Receive pack / Burst before follow-up | **Pressure primary** | Pressure stays by primary; pack holds the front; burst favors channeling until the parent follow-up becomes available. |
| `HET-W01-3-02` | Yield point | Follow-up eligible / New enemy approaches / Primary leaves | **Follow-up eligible** | On the chosen event, check the parent follow-up and end normally; without an eligible follow-up use common end rules. |

Yield point applies only to parent spin-then-link mode. Check the follow-up at the earlier of the parent victim goal or selected actual event. No tick-interval, tick-count or bonus-damage controls.

<a id="w03"></a>

### W03 · Crushing Blow

Choose immediate pressure or a finish after entry or control.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-W03-01` | Automatic use | On / Off | **On** | Sole automatic-use owner; Off applies to modifiers, links and emergency designation. |
| `HET-W03-02` | Strike tactic | Immediate pressure / Finish after entry / Finish after control | **Immediate pressure** | Entry favors actual W02/W08-1 arrival; control favors existing control; otherwise evaluate ordinary strikes. |
| `HET-W03-03` | Strike layout | Center on primary / Primary plus nearby / Focus isolated victim | **Center on primary** | Merges old aim and target choices; no isolated victim falls back to primary. |
| `HET-W03-04` | Preparation | None / Favor W06 shout / Favor actual empowered charge | **None** | Prefer preparation only when present or a valid setup exists; never invent equipment or charges. |

This table owns layout and preparation; global target grades remain shared.

<a id="w03-1"></a>

#### W03-1 · Earth Shatter

Adds these two controls to the parent. Card with this modifier: **6 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-W03-1-01` | Blast center | Beside durable victim / Pack center / Front contact | **Beside durable victim** | Evaluate victims using the actual Ground Shatter radius. |
| `HET-W03-1-02` | Blast victims | 1–6 enemies · step 1 enemies | **3 enemies** | Insufficient coverage returns to the parent strike layout. |

The player cannot enlarge the blast.

<a id="w03-2"></a>

#### W03-2 · Execution Strike

Adds these two controls to the parent. Card with this modifier: **6 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-W03-2-01` | Execution tactic | Favor executable victim / Wait to execute primary / Use normally | **Favor executable victim** | Wait only permits this skill inside the actual execution window; other attacks continue. |
| `HET-W03-2-02` | After refund | Free / W03 if ready / W01 | **Free** | Prefer once after an actual kill and refund; do not assume cooldown reset. |

Execution health threshold is rank-defined, with no extra HP slider.

<a id="w03-3"></a>

#### W03-3 · Aftershock

Adds these two controls to the parent. Card with this modifier: **6 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-W03-3-01` | Aftershock placement | Current engagement / Inside own control / Approach lane | **Inside own control** | Favor placement that keeps enemies around the later explosions. |
| `HET-W03-3-02` | After first strike | Keep attacking primary / Attack outside victims / Follow with W04 | **Keep attacking primary** | Prefer the next action without creating extra aftershock damage. |

Use actual burst timing; no invented second delay.

<a id="w05"></a>

### W05 · Iron Wall

Assign Iron Wall to a protected combat window.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-W05-01` | Automatic use | On / Off | **On** | Sole automatic-use owner; Off applies to modifiers, links and emergency designation. |
| `HET-W05-02` | Protection mission | Protect under danger / Protect entry / Protect main attack | **Protect under danger** | Danger reads global survival rules; entry/main favor one protection cast before the selected action. |
| `HET-W05-03` | Protected action | Free / W02 / W01 / W03 / W08-1 | **Free** | Only equipped, auto-enabled actions are selectable; unavailable setup falls back to survival protection. |
| `HET-W05-04` | Other protection | Relay after expiry / Allow overlap | **Relay after expiry** | Merges shield and defensive-leap overlap; emergencies use only globally permitted overlap. |

No duplicate emergency-health threshold.

<a id="w05-1"></a>

#### W05-1 · Iron Stance

Adds these two controls to the parent. Card with this modifier: **6 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-W05-1-01` | Counter position | In front of primary / Receive approach / Hold after entry | **In front of primary** | Favor a position for real blocks without forcing incoming hits. |
| `HET-W05-1-02` | After counter | Hold position / W03 / W04 | **Hold position** | One follow-up after an actual successful block and counter. |

Counter limits and internal cooldown remain fixed.

<a id="w05-2"></a>

#### W05-2 · Guardian Oath

Adds these two controls to the parent. Card with this modifier: **6 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-W05-2-01` | Cleanse use | Favor escape from root / Prevent entry control / Prevent spender control | **Favor escape from root** | Escape checks removable control; prevention follows the protected action. |
| `HET-W05-2-02` | During immunity | Free / Move with W02 / Maintain W01 / Attack with W03 | **Free** | Prefer one action during actual immunity; no invulnerability or universal cleanse. |

Emergency cleanse follows global survival priority.

<a id="w05-3"></a>

#### W05-3 · Thorn Armor

Adds these two controls to the parent. Card with this modifier: **6 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-W05-3-01` | Thorn tactic | Fight nearby pack / Fight strong melee / Protect retreat | **Fight nearby pack** | Favor protective windows with enemies inside real reflection range. |
| `HET-W05-3-02` | Enemies in thorn range | 1–6 enemies · step 1 enemies | **2 enemies** | Preferred coverage only; never ignore survival to fill it. |

Use real reflection amount and radius; no new ranged reflect.

<a id="w06"></a>

### W06 · Battle Shout

Assign the shout to a spending window.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-W06-01` | Automatic use | On / Off | **On** | Sole automatic-use owner; Off applies to modifiers, links and emergency designation. |
| `HET-W06-02` | Shout tactic | Refill resource / Prepare repeated spenders / Prepare ultimate pressure | **Refill resource** | Refill uses existing shortage checks; repeated follows the chosen spender; pressure follows actual equipped ultimate readiness. |
| `HET-W06-03` | Supported attack | Free / W01 / W03 / W17 / W18 | **Free** | One post-shout preference; selecting an ultimate does not authorize an otherwise disabled cast. |
| `HET-W06-04` | Planned attacks | 1–3 casts · step 1 casts | **1 casts** | Budget only attacks feasible at real cost/cooldown; W01 counts one channel start. |
| `HET-W06-05` | Noncombat refill | Off / Allow | **Off** | Retain the existing unique permission; only applies with actual resource gain and current shortage. |

This row solely owns noncombat permission; incompatible old per-skill encounter restrictions require conversion review.

<a id="w06-1"></a>

#### W06-1 · Rallying Cry

Adds these two controls to the parent. Card with this modifier: **7 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-W06-1-01` | Rally purpose | Enable entry / Close pursuit / Enable retreat | **Enable entry** | Favor the shout before permitted movement needing its speed or control reduction. |
| `HET-W06-1-02` | After moving | Free / W01 / W03 / W04 | **Free** | One preference after movement actually completes. |

Retreat uses the global safe destination.

<a id="w06-2"></a>

#### W06-2 · Frenzy Cry

Replaces parent rows `HET-W06-02`, `HET-W06-04`, `HET-W06-05`. Card with this modifier: **4 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-W06-2-01` | Frenzy window | Spender pressure / Basic and generator pressure / Pressure during ultimate | **Spender pressure** | Reflect the actual attack-speed replacement of resource gain. |
| `HET-W06-2-02` | Pressure focus | Free / W09 / W01 / W03 | **Free** | Only changes preference; does not promise haste on unsupported actions. |

Replaces resource tactic, planned attacks and noncombat refill with frenzy window/pressure focus.

<a id="w06-3"></a>

#### W06-3 · Debt of Battle

Adds these two controls to the parent. Card with this modifier: **7 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-W06-3-01` | Debt timing | Prepare short burst / Refill chosen spender / Use freely | **Prepare short burst** | Favor actual spending of extra resource and account for the regeneration lock. |
| `HET-W06-3-02` | During regen lock | Favor W09 generation / Favor basic attacks / Keep spending | **Favor W09 generation** | Count actual generators only; never assume natural regeneration. |

Resource and lock duration stay fixed.

<a id="w02"></a>

### W02 · Leap Slam

Choose entry placement and the attack after landing.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-W02-01` | Automatic use | On / Off | **On** | Sole automatic-use owner; Off applies to modifiers, links and emergency designation. |
| `HET-W02-02` | Leap role | Offensive entry / Escape / Both | **Both** | Reuse the existing role; global survival owns escape purpose. |
| `HET-W02-03` | Offensive landing | Reach back line / Center of gathered enemies / Pack edge | **Pack edge** | Choose only legal positions under offensive-entry, safety and range constraints. |
| `HET-W02-04` | Minimum offensive distance | 2–8 m · step 0.5 m | **4 m** | Merges the old distance row; does not gate escape. |
| `HET-W02-05` | Landing follow-up | Free / W01 / W03 / W04 | **Free** | Prefer once after a real successful landing. |

Missing follow-up uses free order; no legal landing defers only this leap.

<a id="w02-1"></a>

#### W02-1 · Breach Charge

Adds these two controls to the parent. Card with this modifier: **7 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-W02-1-01` | Breakthrough route | Cross enemy front / Direct to primary / Open side lane | **Cross enemy front** | Evaluate the real shove path; do not relocate enemies arbitrarily. |
| `HET-W02-1-02` | Enemies crossed | 1–5 enemies · step 1 enemies | **2 enemies** | Preferred count; insufficient victims falls back to an edge route. |

Cannot exceed the current rank travel cap.

<a id="w02-2"></a>

#### W02-2 · Quake Landing

Adds these two controls to the parent. Card with this modifier: **7 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-W02-2-01` | Quake placement | Hold landing victims / Prepare approach lane / Overlap own control | **Hold landing victims** | Favor observed enemies likely to remain at the actual delayed-quake position. |
| `HET-W02-2-02` | Before quake | Continue landing attack / Hold with W04 / Move to next pack | **Continue landing attack** | Unavailable W04 continues attacking; do not count stun before the quake occurs. |

Quake delay and stun duration stay fixed.

<a id="w02-3"></a>

#### W02-3 · Double Leap

Adds these two controls to the parent. Card with this modifier: **7 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-W02-3-01` | Two-charge tactic | Keep one for escape / Consecutive offensive entries / Enter then return | **Keep one for escape** | Merge escape reserve with the global reserve using the stricter value. |
| `HET-W02-3-02` | Offensive reserve | 0–1 charges · step 1 charges | **1 charges** | Minimum charges left after offense; cannot lower the global emergency reserve. |

Returning consumes another real leap, not a free return.

<a id="w04"></a>

### W04 · Ground Slam

Choose opening control, receiving new enemies or relaying control.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-W04-01` | Automatic use | On / Off | **On** | Sole automatic-use owner; Off applies to modifiers, links and emergency designation. |
| `HET-W04-02` | Control mission | Open pressure / Stop new approach / Relay control | **Open pressure** | Merges old opportunity and reuse choices; relay favors an actual control effect approaching expiry. |
| `HET-W04-03` | Control group | 1–6 enemies · step 1 enemies | **3 enemies** | Preferred normal group size; globally permitted survival may act on one enemy. |
| `HET-W04-04` | After control | Free / W01 / W03 / W18 | **Free** | Link once after actual control; immune victims return to normal order after damage. |
| `HET-W04-05` | Boss use | Damage / Build stagger / Complete stagger | **Build stagger** | Retain existing boss-use choices; read actual stagger availability, separately from global boss targeting. |

This row owns boss-use intent; global boss targeting remains separate.

<a id="w04-1"></a>

#### W04-1 · Battle Ring

Replaces parent rows `HET-W04-03`. Card with this modifier: **6 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-W04-1-01` | Gathering purpose | Into own W01 / Into own W03 / Gather around hero | **Into own W01** | The pull remains hero-centered; missing attack reference falls back to gathering around the hero. |
| `HET-W04-1-02` | Gathering goal | 1–6 enemies · step 1 enemies | **3 enemies** | Count only enemies inside the rank-defined pull radius. |

Replaces the parent control-group row rather than adding another count.

<a id="w04-2"></a>

#### W04-2 · Tremor Wave

Replaces parent rows `HET-W04-03`. Card with this modifier: **6 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-W04-2-01` | Wave route | Along approach line / Along retreat line / Past primary | **Along approach line** | Use the real linear slow footprint instead of circular stun. |
| `HET-W04-2-02` | Wave victims | 1–6 enemies · step 1 enemies | **3 enemies** | Insufficient coverage falls back toward primary. |

Follow-ups consume an actual slow event, never a fictitious stun.

<a id="w04-3"></a>

#### W04-3 · Repeated Stomp

Adds these two controls to the parent. Card with this modifier: **7 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-W04-3-01` | Repeated-slam placement | Hold one pack / Receive approaching pack / Pressure primary | **Hold one pack** | Favor placement that stays useful through the final strike. |
| `HET-W04-3-02` | Follow-up event | After final stun / After sequence ends | **After final stun** | The first hit is not a stun; movement and follow-up obey the actual action lock. |

Follow-up skill remains solely in the parent row.

<a id="w17"></a>

### W17 · War of the Ancestors

Choose how ancestors and the hero divide targets.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-W17-01` | Automatic use | On / Off | **On** | Sole automatic-use owner; Off applies to modifiers, links and emergency designation. |
| `HET-W17-02` | Ancestor mission | Focus primary / Ancestors on adds / Hero on adds | **Focus primary** | Focus prefers primary for ancestors; the others split initial target preferences between ancestors and hero. |
| `HET-W17-03` | Ancestor target spread | 1–3 enemies · step 1 enemies | **1 enemies** | Effective cap equals actual summons; 2 and 3 are disabled with one ancestor. |
| `HET-W17-04` | Summon window | On engagement / Favor after W06 / Favor after W04 control | **On engagement** | Absent setup falls back to engagement summoning. |

Target allocation is proposed ancestor AI behavior, not an implementation claim.

<a id="w17-1"></a>

#### W17-1 · Ancestral Legion

Replaces parent rows `HET-W17-02`, `HET-W17-03`. Card with this modifier: **4 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-W17-1-01` | Army allocation | Surround primary / Different adds / Primary plus adds | **Surround primary** | Distribute preferences for the actual three ancestors among reachable victims. |
| `HET-W17-1-02` | Hero follow-up | Free / W01 / W03 / W04 | **Free** | Prefer one hero action after successful summoning. |

Replaces parent mission/target spread with army allocation; use only available current victims for the three summons.

<a id="w17-2"></a>

#### W17-2 · Ancestral Technique

Adds these two controls to the parent. Card with this modifier: **6 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-W17-2-01` | Copy preparation | Use actual last skill / Prepare attack skill / Prepare protection skill | **Use actual last skill** | Preparation requires one real setup cast; failure falls back to the actual last skill. |
| `HET-W17-2-02` | Prepared copy | W09 / W08 / W01 / W03 / W05 / W06 / W02 / W04 | **W03** | Enable equipped, auto-permitted normal Warrior skills; attack setup uses attacks, protection setup uses W05/W06. Intervening casts update actual history. |

Protection copies keep their real scope; the edict never rewrites cast history.

<a id="w18"></a>

### W18 · Titan's Judgment

Assign the ultimate to opening control or finishing a gathered pack.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-W18-01` | Automatic use | On / Off | **On** | Sole automatic-use owner; Off applies to modifiers, links and emergency designation. |
| `HET-W18-02` | Judgment mission | Open engagement / Finish gathered enemies / Primary and reinforcements | **Finish gathered enemies** | Finish favors actual gathered enemies; reinforcements favors observed adds around primary. |
| `HET-W18-03` | Judgment victims | 1–8 enemies · step 1 enemies | **4 enemies** | Preferred pack size; an allowed solo boss is not excluded by it. |
| `HET-W18-04` | Link order | Independent / Favor W04 then judgment / Favor judgment then W03 | **Independent** | One link step on actual cast/control events. |

Normal judgment uses real impact/control; giant form uses the modifier's transformation-pressure controls.

<a id="w18-1"></a>

#### W18-1 · Judgment of the Earth

Adds these two controls to the parent. Card with this modifier: **6 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-W18-1-01` | Delayed judgment placement | Currently controlled pack / Ahead of approaching pack / Primary current position | **Currently controlled pack** | Evaluate impact placement from observed motion across the real 1.8-second windup. |
| `HET-W18-1-02` | Controlled-victim goal | 1–8 enemies · step 1 enemies | **3 enemies** | A missed preference does not discard a valid primary-target opportunity. |

No certainty about future motion and no windup tuning.

<a id="w18-2"></a>

#### W18-2 · Giant Form

Replaces parent rows `HET-W18-02`, `HET-W18-03`, `HET-W18-04`. Card with this modifier: **3 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-W18-2-01` | Giant-form window | Enter multiple enemies / Melee primary pressure / Pressure through knockback | **Melee primary pressure** | Treat it as the actual transformation window, not a one-shot judgment. |
| `HET-W18-2-02` | Main attack in form | Free / W01 / W03 / W04 | **Free** | Prefer the selected attack with its actual enlarged area; no unowned attack granted. |

Replaces all three one-shot judgment rows; never wait for a nonexistent slam or stun.

### Warrior configuration examples

#### Enter, gather and finish with aftershocks

Prefer W04 once after landing; ordinary order then selects W03 against actually gathered enemies. Quake/aftershock retain their real timings; W04 creates no recursive link reservation.

- **W02 + W02-2 (Quake Landing)**: Leap role=Both; Offensive landing=Center of gathered enemies; Minimum offensive distance=4 m; Landing follow-up=W04; Quake placement=Hold landing victims; Before quake=Hold with W04.
- **W04 + W04-1 (Battle Ring)**: Control mission=Open pressure; After control=W03; Gathering purpose=Into own W03; Gathering goal=3 enemies.
- **W03 + W03-3 (Aftershock)**: Strike tactic=Finish after control; Strike layout=Primary plus nearby; Preparation=None; Aftershock placement=Inside own control; After first strike=Keep attacking primary.

#### Spread bleed and sweep with Whirlwind

Bleed new victims, sweep safe packs, then favor W03 when a real executable victim appears. Allow generation despite the bleed goal when resource is needed.

- **W09 + W09-1 (Blood Reclaim)**: Raking tactic=Spread bleed; Spread goal=3 enemies; Prepared attack=W01; Reclaim tactic=Favor healing; Healing victims=2 enemies.
- **W01 + W01-1 (Swift Whirl)**: Whirlwind tactic=Spin then link; Sweep victims=4 enemies; After spinning=W03; Fast route=Move between packs; Next-pack reach=2 m.
- **W03 + W03-2 (Execution Strike)**: Strike tactic=Immediate pressure; Strike layout=Center on primary; Preparation=None; Execution tactic=Favor executable victim; After refund=Free.

## 5. Ranger

<a id="a07"></a>

### A07 · Successive Shots

Focus a burst on one enemy or generate resource between spenders.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-A07-01` | Automatic use | On / Off | **On** | Sole automatic-use owner; Off applies to modifiers, links and emergency designation. |
| `HET-A07-02` | Burst tactic | Focus primary / Generate between spenders / Spread when lethal in flight | **Focus primary** | Spread reduces overkill from observed own arrows in flight; it never guarantees a kill. |
| `HET-A07-03` | Focused bursts | 1–3 casts · step 1 casts | **1 casts** | Preferred consecutive actual casts; arrows and resource events per cast do not change. |
| `HET-A07-04` | Shooting anchor | Global primary / Favor own marked victim / Prepared spender target | **Global primary** | No mark falls back to primary; unavailable spender falls back to normal generation. |

Retargeting stops at the real boundaries supported by the firing implementation.

<a id="a07-1"></a>

#### A07-1 · Rapid Volley

Adds these two controls to the parent. Card with this modifier: **6 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-A07-1-01` | Rapid-fire assignment | One victim through burst / Reduce overkill / Generate before spender | **Reduce overkill** | Keep actual launch order and adjust only legally retargetable future casts; never redirect launched arrows. |
| `HET-A07-1-02` | After rapid fire | Free / A01 / A02 | **Free** | Prefer once when the spender is actually eligible. |

Five-arrow count, damage and hit-resource rules stay fixed.

<a id="a07-2"></a>

#### A07-2 · Scatter Shot

Replaces parent rows `HET-A07-02`, `HET-A07-03`. Card with this modifier: **4 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-A07-2-01` | Spread assignment | New victims first / Include marked victim / Victims inside own trap first | **New victims first** | Assign only distinct victims currently eligible for spread. |
| `HET-A07-2-02` | Spread targets | 1–5 enemies · step 1 enemies | **3 enemies** | Clamp to the rank target cap; use available victims without creating arrows. |

Replaces parent focused tactic/burst count with spread assignment/target count.

<a id="a07-3"></a>

#### A07-3 · Finishing Arrow

Adds these two controls to the parent. Card with this modifier: **6 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-A07-3-01` | Final-arrow line | Include enemies behind primary / Primary behind adds / Secure primary hit | **Include enemies behind primary** | Choose cast direction from the actual third-arrow pierce path. |
| `HET-A07-3-02` | Final-line victims | 1–5 enemies · step 1 enemies | **2 enemies** | Prefer within the actual third-arrow pierce cap; otherwise secure primary. |

Cannot fire the third arrow independently or add another critical effect.

<a id="a14"></a>

### A14 · Forking Arrow

Choose the first victim that seeds the split.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-A14-01` | Automatic use | On / Off | **On** | Sole automatic-use owner; Off applies to modifiers, links and emergency designation. |
| `HET-A14-02` | Split seed | Central enemy / Primary first / Front-line enemy | **Central enemy** | Central favors extra victims within actual split reach; primary favors first-hit reliability; front favors the nearest legal first collision. |
| `HET-A14-03` | Spread goal | 1–6 enemies · step 1 enemies | **3 enemies** | Preferred distinct victims across the first hit and real split paths. |
| `HET-A14-04` | Without spread | Use on primary / Yield to single-target shooting | **Use on primary** | Yield only when another eligible basic/generator exists; otherwise use A14 on primary. |

No manual secondary-arrow aiming beyond the skill's capabilities.

<a id="a14-1"></a>

#### A14-1 · Chain Split

Adds these two controls to the parent. Card with this modifier: **6 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-A14-1-01` | Resplit placement | Spread toward second pack / Spread inside one pack / Spread around primary | **Spread toward second pack** | Favor collisions that can produce the real second split. |
| `HET-A14-1-02` | Extra-victim goal | 1–6 enemies · step 1 enemies | **2 enemies** | Preferred extra victims beyond those covered by the first split. |

Resplit count and angles remain effect-defined.

<a id="a14-2"></a>

#### A14-2 · Explosive Arrow

Replaces parent rows `HET-A14-02`, `HET-A14-03`. Card with this modifier: **4 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-A14-2-01` | Explosion seed | Beside durable victim / Clustered adds center / Inside own trap | **Beside durable victim** | Select the initial collision by the actual explosion footprint. |
| `HET-A14-2-02` | Explosion victims | 1–6 enemies · step 1 enemies | **3 enemies** | Insufficient coverage falls back to a primary first hit. |

Replaces parent split-seed and spread-goal rows.

<a id="a14-3"></a>

#### A14-3 · Returning Arrow

Adds these two controls to the parent. Card with this modifier: **6 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-A14-3-01` | Return path | Around fork origin / Between two packs / Outbound and return on primary | **Around fork origin** | Return anchor is the fork origin, not the hero. |
| `HET-A14-3-02` | Return victims | 1–6 enemies · step 1 enemies | **2 enemies** | Preferred observed-path victims, not guaranteed repeat hits. |

Return trajectory and repeat-hit rules stay effect-defined.

<a id="a01"></a>

### A01 · Piercing Shot

Use enemy ordering to shape piercing value.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-A01-01` | Automatic use | On / Off | **On** | Sole automatic-use owner; Off applies to modifiers, links and emergency designation. |
| `HET-A01-02` | Pierce tactic | Adds then primary / Primary then adds / Chokepoint line | **Adds then primary** | Evaluate actual first collision and subsequent pierces among globally eligible candidates. |
| `HET-A01-03` | Line victims | 1–6 enemies · step 1 enemies | **2 enemies** | Merges existing line-count gates; an allowed solo primary is retained. |
| `HET-A01-04` | Lateral adjustment | 0–2 m · step 0.5 m | **0 m** | Maximum extra positioning travel; global movement bans and chase caps still prevail. |

Target grades and distance discipline remain global.

<a id="a01-1"></a>

#### A01-1 · Ricochet

Adds these two controls to the parent. Card with this modifier: **6 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-A01-1-01` | Ricochet route | Direct line first / Favor wall-assisted spread / Favor revisiting primary | **Direct line first** | Evaluate real collidable walls and the actual bounce allowance. |
| `HET-A01-1-02` | Desired bounces | 1–3 bounces · step 1 bounces | **1 bounces** | Preference clamped to current rank; no valid bounce falls back to direct fire. |

No invented wall or unseen victim.

<a id="a01-2"></a>

#### A01-2 · Accelerating Pierce

Replaces parent rows `HET-A01-02`, `HET-A01-03`. Card with this modifier: **4 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-A01-2-01` | Accelerating order | Adds before primary / Pierce many / Primary immediately | **Adds before primary** | Count only actual eligible pierces before primary. |
| `HET-A01-2-02` | Victims before primary | 0–4 enemies · step 1 enemies | **1 enemies** | If the requested order is unavailable, hit primary immediately. |

Replaces parent tactic/total coverage with order/victims before primary; damage gain stays fixed.

<a id="a01-3"></a>

#### A01-3 · Triple Pierce

Adds these two controls to the parent. Card with this modifier: **6 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-A01-3-01` | Three-way layout | Primary on center ray / Cover side packs / Cover broad front | **Primary on center ray** | Evaluate the actual three fixed rays. |
| `HET-A01-3-02` | Desired rays | 1–3 rays · step 1 rays | **2 rays** | Preferred number of occupied rays; never assume three hits on a distant solo victim. |

Angles stay fixed; no spread-angle slider.

<a id="a02"></a>

### A02 · Multishot

Choose front clearing, own-field cooperation or primary focus.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-A02-01` | Automatic use | On / Off | **On** | Sole automatic-use owner; Off applies to modifiers, links and emergency designation. |
| `HET-A02-02` | Multishot tactic | Clear front / Clear own trap/rain / Focus primary | **Clear front** | Focus respects the real per-victim projectile cap. |
| `HET-A02-03` | Volley victims | 1–7 enemies · step 1 enemies | **3 enemies** | Prefer distinct victims in the real fan; focus does not wait for a group. |
| `HET-A02-04` | Empowerment setup | Use immediately / Favor mark / Favor imbuement | **Use immediately** | Prefer existing or eligible setup only; unavailable setup falls back to immediate use. |

Base A02 never assumes multiple hits on one victim.

<a id="a02-1"></a>

#### A02-1 · Barrage

Adds these two controls to the parent. Card with this modifier: **6 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-A02-1-01` | Barrage center | Broad front / Sweep from one edge / Cover own rain | **Broad front** | Evaluate the real seven-arrow fan. |
| `HET-A02-1-02` | Occupied rays | 1–7 rays · step 1 rays | **4 rays** | Preferred rays containing a victim; otherwise use parent tactic. |

Per-victim hit restrictions remain.

<a id="a02-2"></a>

#### A02-2 · Bouncing Arrows

Adds these two controls to the parent. Card with this modifier: **6 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-A02-2-01` | Bounce seed | Front victim with neighbor / Primary first / Own-trap victim first | **Front victim with neighbor** | Favor candidates with a distinct second victim in real bounce range and line of sight. |
| `HET-A02-2-02` | Additional victims | 1–5 enemies · step 1 enemies | **2 enemies** | Preferred distinct extra victims along legal bounce paths. |

No extra bounce count or same-victim bounce.

<a id="a02-3"></a>

#### A02-3 · Focused Spread

Adds these two controls to the parent. Card with this modifier: **6 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-A02-3-01` | Focused-fan stance | Close primary / Controlled primary / Current position only | **Close primary** | Favor legal geometry for actual triple hits; entry bans limit it to the current position. |
| `HET-A02-3-02` | Desired overlapping arrows | 1–3 arrows · step 1 arrows | **2 arrows** | Hit-layout preference only; no change to angles, damage or cap. |

Overlap/shotgun choices require this modifier or corresponding equipment.

<a id="a04"></a>

### A04 · Retreat Leap

Use retreat to lure enemies or prepare a firing line.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-A04-01` | Automatic use | On / Off | **On** | Sole automatic-use owner; Off applies to modifiers, links and emergency designation. |
| `HET-A04-02` | Movement mission | Restore distance / Lure into own trap / Set up pierce line / Prepare imbued shot | **Restore distance** | Merges old retreat/trap/pierce choices; tactical movement requires global offensive-movement permission. |
| `HET-A04-03` | Destination anchor | Global safe point / Far side of own trap / Line with primary | **Global safe point** | Use only legal landings consistent with actual retreat direction and range. |
| `HET-A04-04` | After moving | Free / A01 / A02 / A07 | **Free** | One follow-up after actual arrival; imbuement setup only recognizes actual A01/A02/basic consumers. |

Missing safe anchor/trap falls back to restoring distance.

<a id="a04-1"></a>

#### A04-1 · Decoy Projection

Adds these two controls to the parent. Card with this modifier: **6 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-A04-1-01` | Decoy position | Leave on own trap / Leave in approach lane / Leave beside primary | **Leave on own trap** | The decoy appears at departure, so favor feasible departure positions. |
| `HET-A04-1-02` | After decoy | Victims gathering at decoy / Existing primary / A01 line | **Victims gathering at decoy** | Use observed lure results without guaranteeing attraction. |

Do not enter danger merely to place a decoy.

<a id="a04-2"></a>

#### A04-2 · Smoke Cover

Adds these two controls to the parent. Card with this modifier: **6 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-A04-2-01` | Cover use | Escape danger / Hold firing position / Move behind own field | **Escape danger** | Assign actions within the actual three-second damage reduction. |
| `HET-A04-2-02` | During cover | Parent follow-up / Reposition A03 / Prepare A06 | **Parent follow-up** | Unavailable actions fall back to parent follow-up. |

No stealth, persistent smoke field or added invulnerability.

<a id="a04-3"></a>

#### A04-3 · Shadow Step

Replaces parent rows `HET-A04-02`, `HET-A04-03`, `HET-A04-04`. Card with this modifier: **3 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-A04-3-01` | Shadow arrival | Behind primary / Behind safe candidate / Behind victim facing own trap | **Behind primary** | Require legal rear landing, range and line of sight. |
| `HET-A04-3-02` | Buffed critical shot | Free / A01 / A02 / A07 | **Free** | Prefer only attacks supported by the actual next-critical effect. |

Replaces all three retreat mission/anchor/follow-up rows; no wall bypass.

<a id="a03"></a>

### A03 · Venom Trap

Place traps ahead of enemies, on the retreat route or across two engagement sites.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-A03-01` | Automatic use | On / Off | **On** | Sole automatic-use owner; Off applies to modifiers, links and emergency designation. |
| `HET-A03-02` | Placement tactic | Ambush approach / Leave on retreat route / Cover two engagements | **Ambush approach** | Choose from observed enemy motion and actual trigger reach. |
| `HET-A03-03` | At placement cap | Keep existing / Replace for new front / Keep one retreat slot | **Keep existing** | Respect the actual cap; retreat reserve prefers one fewer normal placement. |
| `HET-A03-04` | Relation to existing trap | Cover new victims / Overlap primary / Split retreat and front | **Cover new victims** | Merges overlap/preservation choices using actual remaining traps. |

Turret assignment is proposed AI behavior, not proof of current targeting.

<a id="a03-1"></a>

#### A03-1 · Spike Trap

Adds these two controls to the parent. Card with this modifier: **6 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-A03-1-01` | Root-trap purpose | Stop pursuer / Hold inside rain / Root primary then shoot | **Stop pursuer** | Prefer a link only after actual root application. |
| `HET-A03-1-02` | After root | Free / A01 / A02 / A17 | **Free** | Immunity or no trigger retains parent placement logic. |

Root duration and trigger mechanics stay fixed.

<a id="a03-2"></a>

#### A03-2 · Frost Snare

Adds these two controls to the parent. Card with this modifier: **6 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-A03-2-01` | Frost lane | Retreat lane / Enemy merge point / Overlap own rain | **Retreat lane** | Favor real routes covered by the slow field. |
| `HET-A03-2-02` | Desired trap separation | 0–6 m · step 0.5 m | **3 m** | Choose the closest legal placement to this spacing; 0 favors overlap. |

No path-blocking wall or guaranteed freeze.

<a id="a03-3"></a>

#### A03-3 · Crossbow Turret

Replaces parent rows `HET-A03-02`, `HET-A03-03`, `HET-A03-04`. Card with this modifier: **3 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-A03-3-01` | Turret position | Current fight edge / Next approach lane / Line to primary | **Current fight edge** | Evaluate actual turret range and line of sight instead of trap triggers. |
| `HET-A03-3-02` | Turret fire assignment | Favor primary / Other than hero target / Nearest legal victim | **Favor primary** | Prefer an eligible victim at the next actual shot; shot count stays fixed. |

Replaces all three parent trap rows; no two-trap reserve or trigger controls on the turret.

<a id="a06"></a>

### A06 · Elemental Imbuement

Assign actual empowerment charges to attacks.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-A06-01` | Automatic use | On / Off | **On** | Sole automatic-use owner; Off applies to modifiers, links and emergency designation. |
| `HET-A06-02` | Imbuement tactic | Assign to A01 / Assign to A02 / Allow basic attacks | **Assign to A01** | Only verified consumers qualify; A07/A14 stay excluded until their consumption path is verified. |
| `HET-A06-03` | Charges reserved for spender | 0–3 charges · step 1 charges | **1 charges** | Minimum actual charges kept for the selected spender; the edict creates or extends none. |
| `HET-A06-04` | Preparation window | When consumer ready / While engaging primary / Any legal consumer | **When consumer ready** | Setup never requires its future charge; eligibility uses real cost, cooldown and line of sight. |

Basic empowerment holding reads this sole owner; absent consumers or buff expiry releases the hold.

<a id="a06-1"></a>

#### A06-1 · Shadow Imbuement

Adds these two controls to the parent. Card with this modifier: **6 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-A06-1-01` | Explosion seed | Add beside primary / Victim near death / Primary directly | **Add beside primary** | Favor the actual first kill explosion footprint. |
| `HET-A06-1-02` | Explosion neighbors | 1–6 enemies · step 1 enemies | **2 enemies** | Preferred observed neighbors inside actual explosion range. |

No assumed recursive explosion chains or new consumers.

<a id="a06-2"></a>

#### A06-2 · Poison Imbuement

Adds these two controls to the parent. Card with this modifier: **6 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-A06-2-01` | Poison assignment | Maintain on primary / Spread to new victims / Durable victims first | **Maintain on primary** | Spread favors victims without your current poison. |
| `HET-A06-2-02` | Poisoned-victim goal | 1–5 enemies · step 1 enemies | **3 enemies** | Preference for future real consumers; no promised applications beyond actual charges. |

Poison damage, duration and stacking stay unchanged.

<a id="a06-3"></a>

#### A06-3 · Cold Imbuement

Adds these two controls to the parent. Card with this modifier: **6 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-A06-3-01` | Frost assignment | Focus freezing one / Spread slows on approach / Freeze rain victims | **Focus freezing one** | Focus uses actual frost stacks; spread favors unslowed victims. |
| `HET-A06-3-02` | After freezing | Spread remaining charges / Continue same victim / Favor A17 | **Spread remaining charges** | One preference after actual freeze; A17 gains no imbuement consumption. |

Respect actual three-stack logic and cast-level, not per-projectile, charge consumption.

<a id="a05"></a>

### A05 · Hunter's Mark

Use marking for burst, divided attention or post-kill transfer.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-A05-01` | Automatic use | On / Off | **On** | Sole automatic-use owner; Off applies to modifiers, links and emergency designation. |
| `HET-A05-02` | Mark mission | Prepare focused fire / Mark primary and clear adds / Kill then move on | **Prepare focused fire** | Prioritize current candidates by mission without duplicating global grades. |
| `HET-A05-03` | Mark retention | Keep living mark / Replace for new primary / Let transfer operate | **Keep living mark** | Use actual mark duration and transfer availability; transfer choice requires its modifier. |
| `HET-A05-04` | After marking | Free / A01 / A02 / A07 | **Free** | One preference after actual mark application. |

Other skills read this actual mark; they create no separate mark.

<a id="a05-1"></a>

#### A05-1 · Gaze Shot

Adds these two controls to the parent. Card with this modifier: **6 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-A05-1-01` | Aimed-shot setup | Secure marked victim / Favor controlled victim / Cast from safe cover | **Secure marked victim** | Favor candidates suitable for the real 0.9-second aim window. |
| `HET-A05-1-02` | After aimed shot | Continue marked victim / Clear nearby enemies / Parent follow-up | **Continue marked victim** | Link after actual release/action end; never assume a pre-release hit or kill. |

Danger cancellation follows existing action rules.

<a id="a05-2"></a>

#### A05-2 · Hunter's Preparation

Adds these two controls to the parent. Card with this modifier: **6 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-A05-2-01` | Kill-reward use | Spender after kill / Refill then spread / Keep current primary | **Spender after kill** | Only favor spending after an actual marked kill grants resource/discount. |
| `HET-A05-2-02` | Discounted attack | Free / A01 / A02 | **Free** | Enable only equipped attacks supported by the next-paid-skill discount. |

No advance or double discount.

<a id="a05-3"></a>

#### A05-3 · Chain Mark

Adds these two controls to the parent. Card with this modifier: **6 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-A05-3-01` | Hero after transfer | Follow new mark / Keep existing primary / Attack another victim | **Follow new mark** | The effect selects the nearby transfer victim; this controls hero behavior only. |
| `HET-A05-3-02` | Transfers to follow | 0–3 transfers · step 1 transfers | **1 transfers** | Maximum transfers the hero follows in this chain, capped by actual rank allowance. |

No changes to transfer radius, target order or effect transfer count.

<a id="a17"></a>

### A17 · Killing Rain

Assign rain to controlled packs, movement routes or reinforcements.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-A17-01` | Automatic use | On / Off | **On** | Sole automatic-use owner; Off applies to modifiers, links and emergency designation. |
| `HET-A17-02` | Rain mission | Clear controlled pack / Bombard approach / Primary plus adds | **Clear controlled pack** | Choose the real rain footprint from current victims and observed motion. |
| `HET-A17-03` | Rain victims | 1–8 enemies · step 1 enemies | **3 enemies** | Preferred pack size; allowed solo bosses remain eligible. |
| `HET-A17-04` | Setup control | Independent / Favor own trap root / Favor own frost freeze | **Independent** | Favor actual root/freeze without stalling all combat for absent effects. |

Post-cast movement of the effect requires actual modifier support.

<a id="a17-1"></a>

#### A17-1 · Moving Barrage

Replaces parent rows `HET-A17-02`. Card with this modifier: **5 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-A17-1-01` | Moving-rain route | Stay near primary / Follow approaching pack / Carry rain along retreat | **Stay near primary** | Rain follows the hero, not the target. |
| `HET-A17-1-02` | Additional offensive travel | 0–4 m · step 0.5 m | **1 m** | Maximum extra offensive travel from the start; retreat uses global safety movement. |

Replaces parent fixed-rain mission with moving route; if movement is forbidden, keep rain at the current position.

<a id="a17-2"></a>

#### A17-2 · Focused Barrage

Adds these two controls to the parent. Card with this modifier: **6 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-A17-2-01` | Focused-rain placement | Held primary / Narrow choke / Gathered adds | **Held primary** | Evaluate the real half-radius, rapid-wave variant. |
| `HET-A17-2-02` | When control ends | Use current rain / Next attack on departing victim / Favor renewed control | **Use current rain** | Do not relocate existing rain; only change the next hero action. |

Wave count and interval stay fixed.

<a id="a18"></a>

### A18 · Afterimage March

Choose how many echoes to leave before shooting.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-A18-01` | Automatic use | On / Off | **On** | Sole automatic-use owner; Off applies to modifiers, links and emergency designation. |
| `HET-A18-02` | Echo tactic | Move then shoot / Shoot after two echoes / Move while spender readies | **Move then shoot** | Use actual echo count and movement-spawn events. |
| `HET-A18-03` | Echoes before shooting | 1–2 echoes · step 1 echoes | **1 echoes** | Used by move-then-shoot; two-echo mode displays fixed 2; no spawn-distance tuning. |
| `HET-A18-04` | Echo shot | Free / A01 / A02 / A07 | **Free** | Enable only actual echo-consuming shots; if further generation is unavailable, shoot with existing echoes. |

When offensive movement is forbidden, keep shooting instead of waiting for echoes.

<a id="a18-1"></a>

#### A18-1 · Shadow Clone

Replaces parent rows `HET-A18-02`, `HET-A18-03`, `HET-A18-04`. Card with this modifier: **3 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-A18-1-01` | Clone pressure | Stationary shooting / Move along firing line / Focus primary | **Stationary shooting** | Use actual direct-hit copying without requiring movement echoes. |
| `HET-A18-1-02` | Clone attack | Free / A01 / A02 / A07 | **Free** | Prefer supported direct hits; no assumed copying of imbuement or every on-hit effect. |

Replaces all parent echo-tactic/count/shot rows.

<a id="a18-2"></a>

#### A18-2 · Afterimage Burst

Adds these two controls to the parent. Card with this modifier: **6 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-A18-2-01` | Explosive-echo position | Enemy approach lane / Beside own trap / Beside primary | **Enemy approach lane** | Route using the real prior hero position where the echo spawns. |
| `HET-A18-2-02` | Echo consumption | Consume by shooting / Use natural expiry / Use replacement | **Consume by shooting** | Use actual consume, expiry or replacement events; no manual detonation command. |

If no echo can be created, return to valid shooting.

### Ranger configuration examples

#### Lure into a trap, then focus rain

Favor leaving the decoy on an own trap, then rain on actually rooted enemies. Failed attraction/root keeps ordinary shooting active.

- **A03 + A03-1 (Spike Trap)**: Placement tactic=Ambush approach; At placement cap=Keep existing; Relation to existing trap=Cover new victims; Root-trap purpose=Hold inside rain; After root=A17.
- **A04 + A04-1 (Decoy Projection)**: Movement mission=Lure into own trap; Destination anchor=Far side of own trap; After moving=A01; Decoy position=Leave on own trap; After decoy=Victims gathering at decoy.
- **A17 + A17-2 (Focused Barrage)**: Rain mission=Clear controlled pack; Rain victims=3 enemies; Setup control=Favor own trap root; Focused-rain placement=Held primary; When control ends=Use current rain.

#### Empowered focused fan on a marked victim

Keep one actual empowerment charge for A02 and favor real overlapping-arrow geometry on the marked victim. Frost stacks require actual consumption/hits; base Multishot gains no shotgun effect.

- **A06 + A06-3 (Cold Imbuement)**: Imbuement tactic=Assign to A02; Charges reserved for spender=1 charges; Preparation window=When consumer ready; Frost assignment=Focus freezing one; After freezing=Spread remaining charges.
- **A02 + A02-3 (Focused Spread)**: Multishot tactic=Focus primary; Volley victims=1 enemies; Empowerment setup=Favor mark; Focused-fan stance=Controlled primary; Desired overlapping arrows=2 arrows.
- **A05 + A05-2 (Hunter's Preparation)**: Mark mission=Prepare focused fire; Mark retention=Keep living mark; After marking=A02; Kill-reward use=Spender after kill; Discounted attack=A02.

## 6. Mage

<a id="m07"></a>

### M07 · Elemental Lance

Use generator casts between core spells or to prepare modifier effects.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-M07-01` | Automatic use | On / Off | **On** | Sole automatic-use owner; Off applies to modifiers, links and emergency designation. |
| `HET-M07-02` | Lance tactic | Pressure primary / Generate between cores / Prepare modifier effect | **Generate between cores** | Preparation uses only the selected modifier's actual burn, slow or conduit. |
| `HET-M07-03` | Consecutive lances | 1–3 casts · step 1 casts | **1 casts** | Preferred actual consecutive casts; yield under shared order when a core becomes ready. |
| `HET-M07-04` | Prepared core | Free / M01 / M02 / M03 | **Free** | One link when eligible; otherwise normal generation. |

No direct element-state assignment; only real casts change last-element history.

<a id="m07-1"></a>

#### M07-1 · Ember Lance

Adds these two controls to the parent. Card with this modifier: **6 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-M07-1-01` | Ember assignment | Maintain burn on primary / Burn new victims / Pressure outside fields | **Maintain burn on primary** | Use actual own burns and field footprints. |
| `HET-M07-1-02` | Burn coverage goal | 1–5 enemies · step 1 enemies | **2 enemies** | Preferred living burned victims, not multi-target burn granted to one projectile. |

No tuning of burn damage, duration or stacking.

<a id="m07-2"></a>

#### M07-2 · Glacier Lance

Adds these two controls to the parent. Card with this modifier: **6 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-M07-2-01` | Glacial tactic | Freeze slowed victim / Spread slow / Hold core target | **Freeze slowed victim** | Freeze counts only on an actual hit against a slowed victim. |
| `HET-M07-2-02` | After freeze | Parent prepared core / Slow another victim / Continue same victim | **Parent prepared core** | No actual freeze retains ordinary lance logic. |

No bypass of slow/freeze immunity.

<a id="m07-3"></a>

#### M07-3 · Grounding Lance

Adds these two controls to the parent. Card with this modifier: **6 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-M07-3-01` | Conduit mission | Bridge distant pack / Fight beside own orb / Bridge around primary | **Bridge distant pack** | Favor conduit placement using observed enemies and actual radius. |
| `HET-M07-3-02` | Conduit replacement | Keep current bridge / Move to new pack / Move on primary change | **Keep current bridge** | Respect the single-conduit cap and replacement of the old node. |

Keep yields lance to another generator when available; otherwise allow replacement to avoid blocking resource generation.

<a id="m01"></a>

### M01 · Fireball

Use first collision, own fields and front placement to aim Fireball.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-M01-01` | Automatic use | On / Off | **On** | Sole automatic-use owner; Off applies to modifiers, links and emergency designation. |
| `HET-M01-02` | Fireball tactic | Explode on front / Barrage own field / Clear line blocker | **Explode on front** | Front uses the actual first-impact blast; field uses own active areas; blocker uses a real line obstruction. |
| `HET-M01-03` | Explosion anchor | Primary / First collision / Own M02/M08 field | **Primary** | Piercing Fireball replaces this blast anchor with path evaluation. |
| `HET-M01-04` | Desired victims | 1–6 enemies · step 1 enemies | **2 enemies** | Preferred coverage inside the actual shape; solo primary remains valid. |

Missing fields use first collision; no legal line yields to another action.

<a id="m01-1"></a>

#### M01-1 · Meteor

Replaces parent rows `HET-M01-03`. Card with this modifier: **5 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-M01-1-01` | Meteor placement | Controlled pack center / Ahead of approach / Primary current position | **Controlled pack center** | Evaluate observed motion over the real one-second delay and legal ground positions. |
| `HET-M01-1-02` | Hero before impact | Keep casting / Favor M06 control / Hold safe position | **Keep casting** | Unavailable M06 continues casting; no pre-impact damage or control. |

Replaces the parent first-collision anchor.

<a id="m01-2"></a>

#### M01-2 · Splitting Fireball

Adds these two controls to the parent. Card with this modifier: **6 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-M01-2-01` | Split center | Beside primary / Between side packs / One pack center | **One pack center** | Choose first impact by the actual three fixed world-direction paths. |
| `HET-M01-2-02` | Occupied split paths | 1–3 paths · step 1 paths | **2 paths** | Preference for occupied branches; no rotation control. |

Actual 0/120/240-degree split directions stay fixed.

<a id="m01-3"></a>

#### M01-3 · Piercing Fireball

Replaces parent rows `HET-M01-02`, `HET-M01-03`, `HET-M01-04`. Card with this modifier: **3 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-M01-3-01` | Piercing-fire path | Cross pack / Leave fire on retreat line / Pierce beyond primary | **Cross pack** | Use the actual pierce path and fire trails instead of an explosion. |
| `HET-M01-3-02` | Pierce victims | 1–5 enemies · step 1 enemies | **3 enemies** | Prefer within the actual pierce cap. |

Replaces all three parent blast controls; no leftover explosion link.

<a id="m02"></a>

### M02 · Blizzard

Place fields at the fight center, retreat route or next approach.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-M02-01` | Automatic use | On / Off | **On** | Sole automatic-use owner; Off applies to modifiers, links and emergency designation. |
| `HET-M02-02` | Blizzard placement | Hold fight center / Cover retreat route / Ambush next choke | **Hold fight center** | Choose where the actual persistent field remains. |
| `HET-M02-03` | At field cap | Keep existing / Replace for new front / Keep one retreat slot | **Keep existing** | Use actual cap and remaining fields; reserve prefers one free normal-placement slot. |
| `HET-M02-04` | Existing-field relation | Cover new victims / Overlap primary / Same front as M08/M11 | **Cover new victims** | Fixed/follow behavior is read-only from actual gear/modifier effects. |

Editing the edict neither deletes nor instantly repositions existing effects.

<a id="m02-1"></a>

#### M02-1 · Trailing Blizzard

Replaces parent rows `HET-M02-02`, `HET-M02-03`, `HET-M02-04`. Card with this modifier: **3 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-M02-1-01` | Following-blizzard route | Stay by primary / Receive approaching pack / Lure along retreat | **Receive approaching pack** | The modifier follows the hero, distinct from target-following gear. |
| `HET-M02-1-02` | Extra offensive travel | 0–4 m · step 0.5 m | **1 m** | Maximum offensive travel from cast position; shared safety/movement rules prevail. |

Replaces fixed-field placement, two-field reserve and overlap controls.

<a id="m02-2"></a>

#### M02-2 · Deep Freeze Storm

Adds these two controls to the parent. Card with this modifier: **6 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-M02-2-01` | Deep-frost goal | Freeze one pack / Maintain primary exposure / Slow approaching enemies | **Freeze one pack** | Use actual field exposure and freeze state. |
| `HET-M02-2-02` | Hero after freeze | Attack outside victims / Focus frozen victims / Favor M01 | **Attack outside victims** | Prefer one next action after actual freeze. |

No exposure-time tuning or instant freeze.

<a id="m02-3"></a>

#### M02-3 · Returning Frost

Replaces parent rows `HET-M02-02`, `HET-M02-03`, `HET-M02-04`. Card with this modifier: **3 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-M02-3-01` | Returning-frost purpose | Slow then freeze / Collect returning resource / Pressure a line | **Slow then freeze** | Use the real round trip; return destination stays at cast origin. |
| `HET-M02-3-02` | Position on return | Return to origin if safe / Keep firing position / Move to next field position | **Return to origin if safe** | Resource collection requires being within the actual one-meter origin radius on return. |

Replaces all parent field rows; no ground-field reservation on a returning projectile.

<a id="m03"></a>

### M03 · Chain Lightning

Choose the first victim to shape the chain.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-M03-01` | Automatic use | On / Off | **On** | Sole automatic-use owner; Off applies to modifiers, links and emergency designation. |
| `HET-M03-02` | Chain start | Well-connected victim / Pack edge / Via own conduit | **Well-connected victim** | Evaluate observed links within real range, line of sight and link radius. |
| `HET-M03-03` | Chain objective | Distinct victims / Total hits / Include primary | **Distinct victims** | Merges existing unique/total scoring and primary inclusion. |
| `HET-M03-04` | Desired connected victims | 1–8 enemies · step 1 enemies | **3 enemies** | Preferred distinct connected candidates; solo primary permission remains. |

Absent conduit falls back to well-connected victim; charge checks read actual effects.

<a id="m03-1"></a>

#### M03-1 · Overload Chain

Replaces parent rows `HET-M03-04`. Card with this modifier: **5 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-M03-1-01` | Overload route | Deep inside pack / Bridge next pack / Spread including primary | **Deep inside pack** | Use the actual link cap without inventing victims. |
| `HET-M03-1-02` | Desired links | 1–8 links · step 1 links | **6 links** | Clamp to current rank; repeated hits count only when an actual effect allows them. |

Replaces the connected-victim goal.

<a id="m03-2"></a>

#### M03-2 · Rebound

Adds these two controls to the parent. Card with this modifier: **6 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-M03-2-01` | Rebound situation | Favor isolated primary / Isolated victims only / Normal chain | **Favor isolated primary** | Isolation requires no visible link neighbor within four meters; only-mode defers this skill when unmet. |
| `HET-M03-2-02` | When neighbors arrive | Allow normal chain / Favor another single-target attack | **Allow normal chain** | Never ignore valid neighbors to force rebounds; without an alternative attack allow normal chain. |

In isolated-only mode, disable the neighbors-arrive row and defer this skill when neighbors exist. Repeat count and damage stay modifier-defined.

<a id="m03-3"></a>

#### M03-3 · Magnetic Chain

Replaces parent rows `HET-M03-02`. Card with this modifier: **5 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-M03-3-01` | Magnetic center | Primary / First victim in own field / First victim in pack center | **Primary** | Use the real pull of later victims toward the first one. |
| `HET-M03-3-02` | After gathering | Free / M01 / M02 / M08 / M11 | **Free** | One preference after actual pull results; unavailable follow-up returns to normal order. |

Replaces parent chain start with magnetic center; no arbitrary destination point.

<a id="m06"></a>

### M06 · Frost Nova

Use close control to prepare a core, stop approach or relay control.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-M06-01` | Automatic use | On / Off | **On** | Sole automatic-use owner; Off applies to modifiers, links and emergency designation. |
| `HET-M06-02` | Frost mission | Prepare core / Stop approach / Relay existing control | **Prepare core** | Merges HERE/APPROACH and control-reuse intent. |
| `HET-M06-03` | After frost | Free / M01 / M02 / M03 / M17 | **Free** | Freeze variants link after actual freeze; protection variants link after protection creation. |
| `HET-M06-04` | Nearby victims | 1–6 enemies · step 1 enemies | **2 enemies** | Preferred count inside the real self-centered radius; emergencies use existing global rules. |
| `HET-M06-05` | Use on boss | Selected mission / Only if stagger contributes / Hold against solo boss | **Selected mission** | Retain existing permission; protection variants show stagger-contribution requirements as unmet. |

Only actual empowerment/charges count; immune targets yield no invented freeze link.

<a id="m06-1"></a>

#### M06-1 · Double Nova

Adds these two controls to the parent. Card with this modifier: **7 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-M06-1-01` | Double-wave position | Stay on first pack / Receive new approach / Stay beside primary | **Stay on first pack** | Favor victims at the actual second damage-wave location. |
| `HET-M06-1-02` | After second wave | Parent follow-up / Attack another victim / Hold position | **Parent follow-up** | The second wave is a damage event, never a second freeze. |

Waiting for the second wave never locks all actions.

<a id="m06-2"></a>

#### M06-2 · Rift Ward

Replaces parent rows `HET-M06-02`, `HET-M06-03`, `HET-M06-04`. Card with this modifier: **4 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-M06-2-01` | Ward position | Beside own field / Current firing position / Next fight edge | **Current firing position** | Treat the ward as stationary protection at cast origin. |
| `HET-M06-2-02` | Pressure in ward | Free / M01 / M03 / M18 | **Free** | Prefer selected pressure while actually inside the protective ward. |

Replaces all three parent freeze-mission/follow-up/count rows; ward grants no freeze.

<a id="m06-3"></a>

#### M06-3 · Ice Armor

Replaces parent rows `HET-M06-04`. Card with this modifier: **6 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-M06-3-01` | Armor setup | Hit many then pressure / Protect before entry / Emergency protection | **Hit many then pressure** | Evaluate shield from actual hits only. |
| `HET-M06-3-02` | Armor-hit goal | 1–6 enemies · step 1 enemies | **3 enemies** | Replaces nearby count; hits beyond shield cap grant no assumed extra shield. |

Respect the actual 20% HP cap and rank-defined shield per hit.

<a id="m04"></a>

### M04 · Teleport

Reposition relative to own fields or restore a firing line.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-M04-01` | Automatic use | On / Off | **On** | Sole automatic-use owner; Off applies to modifiers, links and emergency designation. |
| `HET-M04-02` | Teleport mission | Survival escape / Leave enemies on own field / Lure across own field / Restore firing line | **Survival escape** | Merges old survival/distance roles with tactical positioning; offense requires global permission. |
| `HET-M04-03` | Field anchor | None / Own M02 / Own M08 / Own M11 | **None** | Only active own effects qualify; absent anchors use a global safe point. |
| `HET-M04-04` | After arrival | Free / M01 / M03 / M06 / M08 | **Free** | Prefer once after actual teleport success. |
| `HET-M04-05` | Destination requirement | Global danger rule / No detected damage risk | **Global danger rule** | Retain the existing NO_DAMAGE exception; absence of detected risk does not grant invulnerability or unseen safety. |

No wall bypass, illegal landing or extra range; reaction reads actual history at teleport cast.

<a id="m04-1"></a>

#### M04-1 · Flame Afterburn

Adds these two controls to the parent. Card with this modifier: **7 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-M04-1-01` | Ember departure | Beside approaching pack / Beside own field / Escape current position | **Escape current position** | Favor victims in the real instantaneous departure blast. |
| `HET-M04-1-02` | Departure-blast victims | 1–6 enemies · step 1 enemies | **2 enemies** | Offense preference only; survival escapes never wait for this count. |

No persistent fire field at departure.

<a id="m04-2"></a>

#### M04-2 · Lightning Blink

Adds these two controls to the parent. Card with this modifier: **7 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-M04-2-01` | Lightning arrival | Control primary / Stop approaching pack / Safe edge | **Safe edge** | Evaluate actual arrival stun radius and legal safe landing. |
| `HET-M04-2-02` | After stun | Parent arrival action / M01 / M03 / M17 | **Parent arrival action** | One link after successful stun; immune victims use normal arrival action. |

No invented direct lightning damage.

<a id="m04-3"></a>

#### M04-3 · Elemental Shield

Adds these two controls to the parent. Card with this modifier: **7 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-M04-3-01` | Shielded movement | Protected reposition / Protect offensive entry / Protect escape | **Protected reposition** | This is a real teleport-attached protection effect, not an independent cast. |
| `HET-M04-3-02` | Reactive-element setup | Use actual last element / Prepare fire cast / Prepare cold cast / Prepare lightning cast | **Use actual last element** | History changes only after one real eligible equipped cast; emergency teleport never waits for setup. |

Never automatically convert legacy independent M04-3 shield rules.

<a id="m08"></a>

### M08 · Firewall

Place fire along or across enemy movement.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-M08-01` | Automatic use | On / Off | **On** | Sole automatic-use owner; Off applies to modifiers, links and emergency designation. |
| `HET-M08-02` | Firewall tactic | Along approach / Across approach / Overlap own blizzard | **Along approach** | Evaluate the axis produced by actual caster and aim positions; the wall does not block navigation. |
| `HET-M08-03` | Partner field | None / Own M02 / Own M11 | **None** | Use only currently valid own field footprints. |
| `HET-M08-04` | Crossing victims | 1–6 enemies · step 1 enemies | **3 enemies** | Preferred observed paths crossing the actual shape. |

If no legal caster position produces the desired axis, use a valid direction from the safe current position.

<a id="m08-1"></a>

#### M08-1 · Ring of Fire

Replaces parent rows `HET-M08-02`, `HET-M08-04`. Card with this modifier: **4 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-M08-1-01` | Fire-ring position | Receive approach / Hold own field / Pressure near primary | **Receive approach** | The ring stays at the hero's cast origin. |
| `HET-M08-1-02` | After creating ring | Pressure inside ring / Retreat across ring / Attack primary | **Pressure inside ring** | Prefer legal movement or attacks; the ring does not follow the hero. |

Replaces parent linear placement and crossing-victim goal.

<a id="m08-2"></a>

#### M08-2 · Advancing Flames

Replaces parent rows `HET-M08-02`, `HET-M08-04`. Card with this modifier: **4 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-M08-2-01` | Advancing-fire direction | Along enemy motion / Across pack / Into approach | **Along enemy motion** | Choose a fixed straight direction from observed movement. |
| `HET-M08-2-02` | Path victims | 1–6 enemies · step 1 enemies | **3 enemies** | Count only victims on a path reachable at actual speed and duration. |

No steering or homing after launch.

<a id="m08-3"></a>

#### M08-3 · Cross Wall

Replaces parent rows `HET-M08-02`, `HET-M08-04`. Card with this modifier: **4 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-M08-3-01` | Cross center | Enemy merge point / Primary center / Beside own orb | **Enemy merge point** | Evaluate the union of the real two axes. |
| `HET-M08-3-02` | Occupied axes | 1–2 axes · step 1 axes | **2 axes** | Preferred occupied axes; one intersection victim never counts as two hits. |

Replaces linear placement/count; no arbitrary rotation control.

<a id="m11"></a>

### M11 · Capacitor Orb

Use the orb as an engagement anchor or prepare actual lightning charge.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-M11-01` | Automatic use | On / Off | **On** | Sole automatic-use owner; Off applies to modifiers, links and emergency designation. |
| `HET-M11-02` | Orb mission | Sustained engagement / Prepare M03 charging / Ambush next approach | **Sustained engagement** | Place using current victims, actual expiry and M03 readiness. |
| `HET-M11-03` | Desired charge | 0–3 stacks · step 1 stacks | **1 stacks** | Count only the actual charge each M03 cast gives the oldest current orb; no per-hit/bounce charge or arbitrary distribution to other orbs. |
| `HET-M11-04` | New-orb placement | Keep current orb / Replace for new front / Install after expiry | **Keep current orb** | Respect actual orb cap and removal of replaced orbs. |

Without M03, disable charging preparation and use sustained engagement.

<a id="m11-1"></a>

#### M11-1 · Rolling Orb

Adds these two controls to the parent. Card with this modifier: **6 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-M11-1-01` | Rolling route | Straight through pack / Toward primary / Against approach | **Straight through pack** | Favor legal paths at actual straight-line speed. |
| `HET-M11-1-02` | Hero after roll | Keep firing position / Move alongside / Prepare next field | **Keep firing position** | Moving alongside requires global movement permission. |

No homing or early detonation.

<a id="m11-2"></a>

#### M11-2 · Triple Orb

Adds these two controls to the parent. Card with this modifier: **6 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-M11-2-01` | Three-orb placement | Cover one pack / Cover front width / Primary and side victims | **Cover one pack** | Evaluate the actual three fixed positions around aim. |
| `HET-M11-2-02` | Occupied orbs | 1–3 orbs · step 1 orbs | **2 orbs** | Preferred occupied orbs; no triangle or arbitrary spacing. |

Retention/replacement follows the real three-orb group rules.

<a id="m11-3"></a>

#### M11-3 · Overcharge

Adds these two controls to the parent. Card with this modifier: **6 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-M11-3-01` | Expiry-explosion placement | Inside controlled pack / Primary engagement / Approach choke | **Inside controlled pack** | Evaluate the actual expiry explosion with no periodic damage. |
| `HET-M11-3-02` | Hero before explosion | Favor M03 charging / Favor M06 control / Attack next victim | **Favor M03 charging** | Prefer one action feasible before actual expiry; otherwise attack normally. |

Replacement removal is not an explosion; no manual detonation.

<a id="m17"></a>

### M17 · Triune Collapse

Commit phases to one engagement or coordinate with control and orb expiry.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-M17-01` | Automatic use | On / Off | **On** | Sole automatic-use owner; Off applies to modifiers, links and emergency designation. |
| `HET-M17-02` | Collapse mission | Commit to one fight / Finish after control / Pressure near orb expiry | **Commit to one fight** | Use actual phases and observed control/orb expiry; no guaranteed simultaneous hits. |
| `HET-M17-03` | Collapse victims | 1–8 enemies · step 1 enemies | **4 enemies** | Preferred victims inside actual phase area; allowed solo bosses remain. |
| `HET-M17-04` | Setup effect | None / Favor current slow / Favor current freeze / Own M11 near expiry | **None** | Absent actual setup falls back to committing to one fight. |

The edict changes no phase count, element multiplier or effect range.

<a id="m17-1"></a>

#### M17-1 · Single Element Focus

Adds these two controls to the parent. Card with this modifier: **6 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-M17-1-01` | Focused-element setup | Use actual last element / Prepare fire cast / Prepare cold cast / Prepare lightning cast | **Use actual last element** | Prefer the ultimate after one real equipped, auto-permitted cast of the selected element. |
| `HET-M17-1-02` | If setup unavailable | Use actual last element / Prefer another attack | **Use actual last element** | Prefer one valid alternative then re-evaluate; no history follows the current default-fire rule. |

No direct history edits or free chosen-element effects.

<a id="m17-2"></a>

#### M17-2 · Tracking Collapse

Adds these two controls to the parent. Card with this modifier: **6 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-M17-2-01` | Phase retargeting | Keep same pack / Favor new dense pack / Favor primary inclusion | **Keep same pack** | Retarget only within actual observation range; this is proposed tactical restriction of existing density aiming. |
| `HET-M17-2-02` | Minimum new-pack size | 1–6 enemies · step 1 enemies | **3 enemies** | Preference used only for new-pack mode; otherwise keep a valid current pack/primary. |

Keep actual rank retarget range and three phases.

<a id="m18"></a>

### M18 · Sage Incarnate

Assign the avatar window to cores, fields or protected casting.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-M18-01` | Automatic use | On / Off | **On** | Sole automatic-use owner; Off applies to modifiers, links and emergency designation. |
| `HET-M18-02` | Avatar pressure | Focus ready cores / Fields then cores / Cast under protection | **Focus ready cores** | Use actual equipped cores, fields and protection. |
| `HET-M18-03` | Primary core | M01 / M02 / M03 | **M01** | Enable equipped, auto-permitted cores only; no second global-order editor. |
| `HET-M18-04` | Partner spell | None / M08 / M11 / M06 / M04 | **None** | One setup/follow-up preference appropriate to the mode; unavailable partner falls back to cores. |

Show why unavailable cores are disabled and build drafts only from eligible cores.

<a id="m18-1"></a>

#### M18-1 · Elemental Surge

Adds these two controls to the parent. Card with this modifier: **6 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-M18-1-01` | Surge tactic | Use another-element core / Use ready combination / Favor resource efficiency | **Use another-element core** | The real extra cast is the equipped different-element core selected by shared order. |
| `HET-M18-1-02` | Preferred initiating cast | Parent primary core / Lowest current-cost core / Currently eligible core | **Parent primary core** | Prefer an eligible initiating cast; no arbitrary extra spell, unowned spell or recursive cast. |

Without another-element core, continue normal avatar pressure.

<a id="m18-2"></a>

#### M18-2 · Elemental Avatar

Adds these two controls to the parent. Card with this modifier: **6 controls**.

| Design ID | Control | Choices / range | Default | Behavior |
|---|---|---|---|---|
| `HET-M18-2-01` | Elemental-avatar tactic | Pressure with current element / Prepare next element / Prepare defensive element under danger | **Pressure with current element** | Effects change only through actual last-element casts; danger reads global rules. |
| `HET-M18-2-02` | Element to prepare | Keep current / Fire / Cold / Lightning | **Keep current** | Prefer one real equipped cast of that element; unavailable setup keeps the current element. |

No full-window element lock, button-only element switch or invented defense.

### Mage configuration examples

#### Crossfire and meteors over deep frost

Hold the current fight with Blizzard, overlap a cross and favor meteors on actually controlled victims. Cross intersections do not double-count; meteor damage follows its real delay.

- **M02 + M02-2 (Deep Freeze Storm)**: Blizzard placement=Hold fight center; At field cap=Keep existing; Existing-field relation=Same front as M08/M11; Deep-frost goal=Freeze one pack; Hero after freeze=Favor M01.
- **M08 + M08-3 (Cross Wall)**: Partner field=Own M02; Cross center=Primary center; Occupied axes=2 axes.
- **M01 + M01-1 (Meteor)**: Fireball tactic=Barrage own field; Desired victims=3 enemies; Meteor placement=Controlled pack center; Hero before impact=Favor M06 control.

#### Charge an orb and use its expiry explosion

Seek charge through actual M03 casts and use feasible M06 control/protection before expiry. Replacement removal and multiple bounces count as neither an explosion nor extra charge.

- **M11 + M11-3 (Overcharge)**: Orb mission=Prepare M03 charging; Desired charge=3 stacks; New-orb placement=Keep current orb; Expiry-explosion placement=Inside controlled pack; Hero before explosion=Favor M03 charging.
- **M03 + M03-1 (Overload Chain)**: Chain start=Well-connected victim; Chain objective=Distinct victims; Overload route=Deep inside pack; Desired links=6 links.
- **M06 + M06-3 (Ice Armor)**: Frost mission=Prepare core; After frost=M02; Armor setup=Hit many then pressure; Armor-hit goal=3 enemies.

## 7. Saves and presets

Reuse the [existing save contract](HELLSCRIPT_Hunt_Edict_Detail.md) for the editor, five presets, unsaved drafts, undo and atomic application; create no duplicate saving UI. Restore loadout and options together. Missing skills leave their original slot empty. Presets never replenish or reset health, resource, cooldown, active actions, charges, projectiles or fields.

Preserve modifier settings under both parent and actual modifier IDs. Inactive modifier settings never trigger. Keep unsupported originals and show conversion differences. Current execution changes only after explicitly saving the conversion draft. Never interpret legacy independent `M04-3` as the D4 modifier without identifying version and shape.

## 8. Open implementation checks and acceptance

**Specified here:** actual choices for every active/modifier, numeric bounds/steps/defaults, ownership of retained existing features, contextual replacement, missing-reference/immunity/illegal-position/resource fallback and bounded link reservation.

**Validate before implementation completion:** sustainable W09-2 stacks under actual cooldown/bleed duration; A07/A14 imbuement and echo consumption support; legal mid-burst retargeting boundaries; ancestor/turret target assignment support; modifier/gear conflicts and effective caps. Unverified consumers never become selectable automatically. Values are initial design values, not combat-balanced results.

Keep later acceptance checks scoped to affected systems.

1. At most seven controls per actual equipped card; no incompatible trap/field/explosion/freeze controls remain.
2. The examples reproduce different tactics with the same skill/equipment, with visible fallback/defer reasons.
3. Links never bypass Off, shared survival, resource, cooldown, range, line of sight, landing safety or effect ownership.
4. Generation, charge, transfer and hits react only to real events; canceled reservations, missing references and immunity do not loop.
5. Preserve original saves/shares and apply semantically different legacy values only through explicit conversion drafts.

This task validates document inventory, choices, defaults, numbers, counts and links. No game code, data or save format was implemented; no Unity, Play Mode, balance or device validation was performed.

## 9. Source snapshot checked

- [Current D4 inventory](../../Assets/HELLSCRIPT/Resources/Data/SkillTreeD4.json)
- [Current base metadata](../../Assets/HELLSCRIPT/Resources/Data/ClassSkills.json)
- [Existing core edict options](../../Assets/HELLSCRIPT/Runtime/Core/HuntEdictV2.Options.cs)
- [Existing global/detail options](../../Assets/HELLSCRIPT/Runtime/Core/EdictOptions.cs)
- [Actual Warrior effects](../../Assets/HELLSCRIPT/Runtime/Core/SkillTreeD4/CombatSimulation.D4Warrior.cs)
- [Actual Ranger effects](../../Assets/HELLSCRIPT/Runtime/Core/SkillTreeD4/CombatSimulation.D4Ranger.cs)
- [Actual Mage effects](../../Assets/HELLSCRIPT/Runtime/Core/SkillTreeD4/CombatSimulation.D4Mage.cs)

Snapshot: `ce4202fe3c286750409da89a6e7622f9d3bef38e`. Compared skill/edict sources with local `origin/main`; this does not verify the latest remote state or live-game feature enablement.

Document checks: all 30 actives and 84 modifiers covered; matching 293 design-row IDs in both languages; valid choice/numeric defaults, steps, row replacements and example references; maximum seven controls per card; valid relative document/source links. Other tasks' uncommitted files were preserved.
