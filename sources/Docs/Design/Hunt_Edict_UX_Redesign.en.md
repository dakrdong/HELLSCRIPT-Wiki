# Hunt Edict UX redesign: overview, combat styles and advice from the last hunt

Updated: 2026-09-28 · [한국어](Hunt_Edict_UX_Redesign.md)

## Request and goal

The user defines the Hunt Edict as the core fun of the game. With the same equipment, different settings and skill combinations should produce different ways of fighting, and the goal is a more efficient fight at the same combat power. The problem is the sheer number of settings: a new player can leave before getting a feel for the game.

This document records what the current edict window looks like and the redesigned structure. The redesign keeps every existing setting. It makes the window readable from the first moment ("how does my hero fight right now?"), lets one choice make a meaningful difference, and turns the result of a hunt into the next change.

## The current window

Facts read from code and data on `main` (`672e4a0c`) on 2026-09-28.

| Item | Current state |
|---|---|
| Tabs | Nine: Skill, Combat, Survival, Loot, Bag & Cleanup, Explore, Repeat Hunt, Auto Equip, Presets & Sharing. The window opens on Skill. |
| Volume | 35 global groups with 151 options. The Skill tab holds a 37-node tree per class, four active slots plus an ultimate slot and a use policy per skill. |
| Quick presets | Three per global group (25 groups) and per skill item, but each one changes a single group. |
| New hero defaults | All three dodge policies (ground, area, direct), low-HP retreat and lethal-damage response are off. A new hero does not avoid telegraphed attacks and does not fall back at low HP. |
| Combat records | A record keeps the damage sources of the last five seconds before death, why and how often each skill was held back, and time spent attacking, dodging, moving, collecting and waiting. The edict window never reads it, and the defeat analysis shows numbers without saying which setting to change. |

### Where a new player gets stuck

1. **The first screen is the hardest one.** The window opens on a 37-node skill tree with eight more tabs behind it. Nothing shows at a glance how the hero fights right now.
2. **Core and housekeeping settings carry the same weight.** Ten groups (combat and survival) decide how the hero fights; the other 25 (loot, bag, exploration, repeat, auto equip) work at their defaults. They sit side by side, so there is no hint of what to look at first.
3. **A coherent style cannot be chosen in one step.** Quick presets change one group each. Fighting aggressively means picking position, target, retreat and dodging separately, and mismatched combinations are easy.
4. **Settings and results are disconnected.** The record knows the cause; connecting it to a setting is left to the player. "More efficiency at the same power" is learned by changing a setting and checking the result, and the half of that loop that leads from the result back to a setting is missing.
5. **Missing safety nets stay invisible.** A new hero starts with dodging and retreat off, and no screen summarises that.

## Principles

- **Show current behaviour first.** The first screen should let the player read how the hero fights.
- **One choice, a meaningful difference.** Choosing a combat style should be enough to start.
- **Turn results into the next change.** The last hunt proposes concrete changes that one tap applies to the draft.
- **Open depth on demand.** Keep the nine tabs, the 151 options and Custom, and link to them from the overview.
- **Keep the save rules.** Every choice edits the draft only. Hunts use it after Save, and Revert and the leave confirmation behave as before.

## The new structure

### Three depths

| Depth | Screen | Used by |
|---|---|---|
| 1. Overview | The new **Overview** tab, which the window opens on. | New players choosing a style and applying advice. |
| 2. Quick presets | Judgment rows on the overview and the per-group presets in the tabs. | Players adjusting one part after choosing a style. |
| 3. Custom | The 151 detailed options and per-skill options in the tabs. | Experienced players tuning values. |

### The overview

1. **Combat style**: Aggressive, Balanced and Careful cards. A card sets seven combat and survival groups at once (engagement position, encirclement, target, emergency retreat, potions, dodging by damage type, special hazards). The matching card carries a ✓; when nothing matches, the overview says so, and a warning appears when dodging and emergency retreat are both off. New heroes start on Balanced, so the warning shows for older heroes that kept the defaults.
2. **Advice from your last hunt**: stage, result and time of the hero's last finished rift, and up to three changes drawn from that record. Each piece of advice states what was observed, which setting and preset to use, and the preset's description. Tapping applies it to the draft; after saving it leaves the list. When advice exists it comes before the styles.
3. **Equipped skills and their use**: equipped actives, the ultimate and the basic attack with their use-policy preset names; a tap opens that skill's policy page. A link opens the skill tree.
4. **Combat judgment**: the current preset and description of the seven style groups. The list doubles as the plain-language summary of how the edict fights. A tap opens the quick-preset picker, and Custom there opens the group's detailed options in its tab.
5. **Automation**: loot, bag & cleanup, exploration, repeat hunt and auto equip as one block, with the number of groups changed from defaults per tab and a link to each tab. The text tells a new player these can stay at their defaults.

Landscape shows advice and styles in the left column and skills, judgment and automation in the right, each scrolling on its own; portrait shows one column. Portrait now lays out ten tabs as 5×2 instead of 3×3, so the tab area is shorter.

### Combat styles

A style is not a new combat rule. It names a bundle of existing group presets, so the save format and share codes gain no field; the current style is whichever bundle the current values match.

| Judgment group | Aggressive | Balanced | Careful |
|---|---|---|---|
| Combat position & distance | Push into the pack (Warrior) · Balanced engagement (Ranger, Mage) | Balanced engagement | Balanced engagement (Warrior) · Keep your distance (Ranger, Mage) |
| Encirclement & gathering | Fight the current pack | Fight the current pack | Escape encirclement |
| Target selection | Clear dense packs | Nearest enemy first | Dangerous enemies first |
| Emergencies & re-engagement | Emergency retreat (20% HP) | Balanced survival (30% HP) | Survival first (45% HP) |
| Automatic potion | Balanced supplies | Balanced supplies | Extra survival supplies |
| Avoidance by damage type | Maintain attacks | Avoid heavy damage | Avoid all warnings |
| Special dangers & avoidance | Finish the current attack | Walk away first | Avoid control and overlapping damage |

Target retention & pursuit, action priority and survival tools & reserves are left out: the right values depend on the equipped skills, so the player keeps choosing them. Styles never touch loot, bag, exploration, repeat or auto-equip settings.

### Advice from the last hunt

Advice reads the stored record and never re-simulates the fight. Advice the saved edict already follows is not shown.

| Recorded situation | Test | Suggestion |
|---|---|---|
| Death | The run ended with the hero's death. | Survival first, Avoid all warnings, Extra survival supplies; the reason names the largest damage source of the last five seconds and its share. |
| Time out | A failed run whose result is `제한시간 초과` (out of time). | Nearby loot only when collecting took 15% or more of the observed time, Maintain attacks when dodging took 20% or more, and always Rush the boss. |
| Out of range | An equipped skill waited 20 or more times for `RANGE`. | Push into the pack for a Warrior, Balanced engagement for a Ranger or Mage. |
| Out of resource | An equipped skill waited 20 or more times for `RESOURCE`. | The basic attack's Recover resources. |
| Comfortable clear | A clear with 70% or more HP left. | The Aggressive style: the same power could hunt faster. |

The thresholds are starting values chosen from how the recorders count. They need tuning once real records accumulate.

### Entry points

- Opening the edict from town or a result screen lands on the overview. The rift result's **Edit hunt edict** button now opens the overview instead of the skill policy page, so the advice from the hunt that just ended is right there.
- Tutorial steps open their area directly: Equip a newly unlocked skill (F06) the Skill tab, Presets and sharing (H10) the presets tab, Repeat hunting and idle display (H11) the repeat tab. The potion practice (F05) still jumps to the survival option.
- Opening a skill policy or a skill subpage switches to the Skill tab from any tab.
- The defeat analysis shown for older records without review data has a **사냥 칙령 수정하러 가기** (edit the hunt edict) button that opened the legacy action-design page despite its label; it now opens the edict overview.

### Unchanged

The nine existing tabs, the IDs, values and ranges of the 35 groups and 151 options, the 249 quick presets, Save/Revert and the leave confirmation, the five preset slots, HED5 share codes and the account save format. Option defaults are unchanged as well; only newly created heroes start on Balanced (see the decision below).

## Open items

- **New hero defaults (decided 2026-09-28)**: at the user's decision, newly created heroes start on Balanced. Option defaults and existing heroes are unchanged; a hero created earlier that kept the defaults still gets the warning and the Balanced recommendation. The early-balance comparison is in the [implementation record](../Implementation/Hunt_Edict_Overview.en.md).
- **Advice thresholds**: starting values; tune them from recorded hunts.
- **Training comparison**: comparing a new style against the previous one in the training ground under the same conditions is out of scope; the existing A/B comparison could be opened from the overview.
- **Landscape tab height**: with ten tabs, one landscape tab at 956×440 shrinks from about 40 to 36 logical units. The edict editor is measured but not enforced by the 48dp touch rule (`RuntimeTouchLayoutSmoke`); if the tabs prove hard to hit on a phone, a 2×5 landscape grid is the next option.
- **Testing with people**: whether new players actually understand faster needs play tests with people. This work was verified with automated checks and a macOS development build only.

The implementation and verification record is [Hunt Edict overview](../Implementation/Hunt_Edict_Overview.en.md).
