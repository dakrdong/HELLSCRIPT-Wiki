# Tutorial Dialogue Script — Chapters 0 to 5

Updated: 2026-10-05

[한국어](Tutorial_Chapter_Scripts.md)

This fixes what is said in chapters 0 to 5 of the [Tutorial Chapter Design](Tutorial_Chapters.en.md). Part E implements it. This document settles the wording; the code and the translation file have not been changed yet.

## The story at a glance

| Ch. | Guide | Hook (why) | Action (what) | Inscribed sentence |
| --- | --- | --- | --- | --- |
| 0 | Voice | Surrounded and about to die, the one who bound demons with writing appears | Inscribe the first skill, compare two modes, change the potion threshold from 40% to 60% | (preface) |
| 1 | Anton | A rift is where the seal tore, and the ledger counts those who return | Walk over, talk, and enter | The door is open, and the edict writes the fight |
| 2 | Ish | Winners and losers alike get their last five seconds read | Change one place: the potion HP threshold, the automatic potion or dodge | The last five seconds of a fallen one are the most honest |
| 3 | Marc | Iron picked up in a rift is chosen by numbers | Compare and equip gear, and do a first enhancement | Forge the tool first |
| 4 | Turk | Testing in the real thing and dying is a fool's game, and every fight has a temperament | Test at the training ground and pick a style card | Temperament is set before the blade |
| 5 | Ish | A fang bites only once it is set | Learn a new skill and equip it in a slot | A power written must be held to bite |

## How to read it

- The columns are the line ID (`C<chapter>-<scene>-<number>`), speaker, line or instruction text, context and control, and status. Status is **Existing** (already in the game, so the English exists), **Reworded** (an existing line was changed), **New**, or **Auto** (an existing line that code appends).
- **Guide** is the card text that points at a designated control. In the prologue it is forced (`PrologueGate`); in other chapters only a gold outline is added and the player may press elsewhere. Control names follow 4.1 of the [Hunt Edict UI brief](Tutorial_Hunt_Edict_UI_Brief.en.md) and are updated after the UI overhaul.
- A **Bark** is a single line that appears briefly over the battle. **Choice**, **Notice** and **Card** are short texts that go into the existing staging parts.
- A chapter runs **hook → one action → confirmation → page fill**. The hook gives the reason first, in at most three lines.
- Keep a line to 90 characters or fewer. The dialogue band shows one line at a time, and a long line pushes the listener on the right out.
- Put fixed text before and after a placeholder such as `{0}`. A line that starts with `{0}` falls into the trap where translations mix with stored combat-record text.
- Every line has an English pair. Text already in `en.txt` uses its existing English.

## Settings to respect

- The old god imprisoned demons not with blades but with **writing**. A rift is where that seal was torn. The god's name is never written in the lines.
- The hero, the Chosen, is a scribe-warrior who carves edicts into their body. Editing the Hunt Edict is writing a sentence into the body.
- The town is "Ashwood · Settlers' village". Existing terms such as sanctuary and rift keeper stay.
- In Ish's **Ember Scroll** one page kindles each time the scribe learns a sentence. Chapter 0 is a preface with no page number.
- No out-of-game speech. Words such as "tutorial" or "button" appear only in guide lines, wrapped in the character's voice.
- Keep it dark. A joke comes from a character's personality, one line, and never more than one per scene.

## Voices

| Character | Speaker ID | Address and register | Personality and humor | Example |
| --- | --- | --- | --- | --- |
| Voice | `prologue-voice` | "you" · archaic ("shall", "mortal") | Majestic and slow, with an occasional dry line | "Lift your head, mortal." |
| Hero | `hero-<class>` | Polite, brief | Barely speaks; one retort gets a laugh | "…Writing? What I need now is a blade." |
| Anton | `anton-jindark` | "you" · gruff elder | Blunt, obsessed with his ledger, deadpan dark jokes | "Those who return always get a longer line." |
| Marc | `mark-kus` | "you" · calm elder | Calm and exact; speaks through iron | "Iron doesn't lie." |
| Turk | `turk-garbi` | "recruit" · casual, exclamations | Hearty; sounds like a drill sergeant but is kind | "Temperament is set before the blade!" |
| Ish | `ish-ashscribe` | "scribe" · formal and polite | Courteous and observant, with tired dry wit | "…Let me correct that." |

## Page fills

When a page fills, show a card (`CinematicCard.Chapter`) and then the notice "Reward · 200 gold". The card frame is "Ember Scroll · Page N · Title · Inscribed sentence".

| Chapter | Page | Title | Inscribed sentence |
| --- | --- | --- | --- |
| C01 | 1 | The First Page | The door is open, and the edict writes the fight |
| C02 | 2 | The Page Read in Death | The last five seconds of a fallen one are the most honest |
| C03 | 3 | The Broken Blade | Forge the tool first |
| C04 | 4 | The Training Ground's Cane | Temperament is set before the blade |
| C05 | 5 | A New Fang | A power written must be held to bite |

## Chapter 0 — The Voice of the Edict (prologue)

| Item | Content |
| --- | --- |
| Guide | The Voice of the Edict (`prologue-voice`), the hero (`hero-<class>`) |
| What it teaches | Opening the Hunt Edict → inscribing the first skill → comparing two ways to use it → the potion HP threshold (sentences 1 and 2) |
| Guides | P00 |
| Starts | First launch of a new account (mandatory) |
| Length | About 6 minutes including combat |
| Page | Preface. It has no page number and kindles when the player meets Ish |
| Reward | 1,000 gold (unchanged) |

### C00-A · Opening · surrounded

First launch of a new account. Fade in from black and show the title card. Staging is the existing `IntroBeat`.

| ID | Speaker | Line / instruction | Context · control | Status |
| --- | --- | --- | --- | --- |
| `C00-A0` | Narration/system | **Card** Prologue · The Voice of the Edict · A command descends upon the besieged | Title card | Existing |
| `C00-A1` | Hero | …They're on every side. My hands can barely hold my weapon. |  | Existing |
| `C00-A2` | Hero | Is this the end? Nameless, swallowed by this darkness… |  | Existing |
| `C00-A3` | Narration/system | **Choice** Look up |  | Existing |

### C00-B · The Voice descends

Choosing it makes a pillar of light strike down (`DescentBeat`). The current v3 flow skips this and the scroll staging (`ScrollBeat`); both come back.

| ID | Speaker | Line / instruction | Context · control | Status |
| --- | --- | --- | --- | --- |
| `C00-B1` | Voice | Lift your head, mortal. Your despair has reached the heavens. |  | Existing |
| `C00-B2` | Hero | …Who are you? This blinding light… |  | Existing |
| `C00-B3` | Voice | I am the one who long ago bound demons not with blades but with writing. Now I will write that writing into your body. |  | New |
| `C00-B4` | Hero | …Writing? What I need now is a blade. | One joke. The hero answers briefly. | New |
| `C00-B5` | Voice | You already hold the blade. What you lack is a sentence. Write as I command, and you shall leave this place alive. |  | Reworded |
| `C00-B6` | Voice | Ask not; receive. This is the Hunt Edict, my will to guide your hands and your steps. |  | Existing |
| `C00-B7` | Narration/system | **Choice** Receive the scroll | Choice. Afterwards the scroll staging plays and the seal flies to the menu. | Existing |

### C00-C · Unfolding the edict (forced steps begin)

Only the designated control can be pressed (`PrologueGate`). The Voice speaks every instruction line of this chapter. Control names follow 4.1 of the [Hunt Edict UI brief](Tutorial_Hunt_Edict_UI_Brief.en.md).

| ID | Speaker | Line / instruction | Context · control | Status |
| --- | --- | --- | --- | --- |
| `C00-C1` | Voice | **Guide** Do you see the shining seal? That is the Hunt Edict. Press it and unfold it. | `menu-hunt-edict` | Existing |

### C00-D · The first sentence · inscribing a power

The player learns and equips the first skill, then compares two ways of using it under equal conditions (sentence 2). The tone revives the wording of the older v2 instructions.

| ID | Speaker | Line / instruction | Context · control | Status |
| --- | --- | --- | --- | --- |
| `C00-D1` | Voice | **Guide** First choose the power I grant you. Open 'Skill'. | `edict-tab-skills` | Existing |
| `C00-D2` | Voice | **Guide** First return to the skill tree. | `edict-policy-back` (only on the usage screen) | Existing |
| `C00-D3` | Voice | **Guide** '{0}'. The first power to guard you within the siege. Press it and look. | `edict-skill-<first skill>` · {0}=skill name | Existing |
| `C00-D4` | Voice | **Guide** Inscribe the one skill point I granted into this power. Press '+'. | `edict-skill-rank-up` | Existing |
| `C00-D5` | Voice | **Guide** A power inscribed must be held to be used. Press 'Equip'. | `edict-skill-equip` | Existing |
| `C00-D6` | Voice | **Guide** To seal your will, press 'Save'. | `edict-save` | Existing |
| `C00-D7` | Voice | **Guide** Now press '{0}' in slot 1. | `edict-active-slot-0` · {0}=skill name | Existing |
| `C00-D8` | Voice | **Guide** Decide how that power is wielded. Press 'Edit hunt edict'. | `edict-slot-policy` | Existing |
| `C00-D9` | Voice | **Guide** One power can be wielded many ways. First, '{0}'. {1} Press the tab and look. | `edict-preset-tab-<mode A>` · {0}=mode name {1}=description | Existing |
| `C00-D10` | Voice | **Guide** Watch '{0}' fight. You may pause and watch again. When you have seen enough, confirm. | Area `Preset explanation` · `edict-starter-next` ("I saw the action") turns on once the example finishes | New |
| `C00-D11` | Voice | **Guide** Now choose '{0}' and face the same foe. Change how you wield it, and the fight changes. | `edict-preset-tab-<mode B>` | New |
| `C00-D12` | Voice | **Guide** Chosen, now inscribe it. Press the round box at the right of that tab. | `edict-preset-activate` (the check box at the right of the tab; the older line named 'Activate preset', which is not on screen, so it was reworded) | Reworded |
| `C00-D13` | Voice | **Guide** Once inscribed, confirm. We shall watch once more: same start, different way. | `edict-starter-next` ("Confirm mode B saved") | New |
| `C00-D14` | Voice | **Guide** See how the movement and the waiting differ. Which one suits your hand? | Area `Preset explanation` → `edict-starter-next` | New |
| `C00-D15` | Voice | **Guide** You have seen both sentences. Inscribe the one you prefer and press 'Hunt with this mode'. | Area `Option area` → `edict-starter-next` ("Hunt with this mode") | New |
| `C00-D16` | Voice | **Guide** It is done. Close the edict and prepare to fight. | `edict-close` | Existing |
| `C00-D17` | Voice | The edict is written into your body. Your hands shall no longer falter. | Dialogue after closing | Existing |
| `C00-D18` | Voice | Go. Break the siege and live. |  | Existing |
| `C00-D19` | Narration/system | **Choice** Break the siege |  | Existing |
| `C00-D20` | Narration/system | **Notice** New objective · Defeat the monsters around you |  | Existing |

### C00-E · The siege and the breakthrough

Real combat. Lines cut in briefly.

| ID | Speaker | Line / instruction | Context · control | Status |
| --- | --- | --- | --- | --- |
| `C00-E1` | Voice | **Bark** Do you see? While the edict guides your body, you shall not fall. | At the first kill (bark) | Existing |
| `C00-E2` | Hero | I did it… I actually survived. | After all six are down | Existing |
| `C00-E3` | Voice | Rejoice not yet. A gatekeeper holds the end of the road. |  | Existing |
| `C00-E4` | Voice | The edict is with you. Go forth without fear. |  | Existing |
| `C00-E5` | Narration/system | **Choice** To the gatekeeper |  | Existing |
| `C00-E6` | Voice | **Bark** The gatekeeper. Fight as the edict decrees. I shall not let you fall. | Right after the gatekeeper entrance cut (bark). The name card stays "Boss · Gatekeeper of the road to the sanctuary" | Existing |

### C00-F · The potion sentence (boss-fight intervention)

When the hero drops below 50% HP, time stops. This teaches sentence 1.

| ID | Speaker | Line / instruction | Context · control | Status |
| --- | --- | --- | --- | --- |
| `C00-F1` | Hero | Ngh… At this rate…! |  | Existing |
| `C00-F2` | Voice | Be still. I have seized this very moment. |  | Existing |
| `C00-F3` | Voice | Strength alone will not pass the gatekeeper. Write into the edict that you drink before you fall. |  | Reworded |
| `C00-F4` | Voice | Your edict has you drink only when your health reaches 40%. By then it is too late. Rewrite it to 60%. |  | New |
| `C00-F5` | Narration/system | **Choice** Unfold the Hunt Edict |  | Existing |
| `C00-F6` | Narration/system | **Notice** New objective · Set the potion HP threshold to 60% |  | Existing |
| `C00-F7` | Voice | **Guide** Unfold the Hunt Edict once more. | `menu-hunt-edict` | Existing |
| `C00-F8` | Voice | **Guide** Open 'Survival'. The way to live is written there. | `edict-tab-survival` | Existing |
| `C00-F9` | Voice | **Guide** Press 'HP potion threshold' and rewrite 40 as 60. | `edict-option-survival.potionHpPercent` | New |
| `C00-F10` | Voice | **Guide** Write 60. | `edict-number-input` | New |
| `C00-F11` | Voice | **Guide** Apply 60%. | `edict-number-apply` | New |
| `C00-F12` | Voice | **Guide** Press 'Save' to seal your will. | `edict-save` | Existing |
| `C00-F13` | Voice | **Guide** It is done. Close the edict, and time shall flow again. | `edict-close` | Existing |
| `C00-F14` | Voice | It is done. Time flows once more. | Dialogue after closing | Existing |
| `C00-F15` | Voice | This time you shall not fall. Strike down the gatekeeper. |  | Existing |
| `C00-F16` | Narration/system | **Choice** Fight again |  | Existing |
| `C00-F17` | Voice | **Bark** You drank. One line of writing has saved your life. | The first time the auto potion fires at the 60% threshold (bark). Same moment the lesson step becomes 5 | New |
| `C00-F18` | Voice | **Bark** The gatekeeper falters! Make an end of it! | Gatekeeper at 30% health or less (bark) | Existing |

### C00-G · Victory and arrival in town

The card "Victory · Gatekeeper defeated · One who followed the edict survived" and the notice "Reward · 1,000 gold" stay as they are, and so does the reward transaction (`tutorial-map-complete-v2`).

| ID | Speaker | Line / instruction | Context · control | Status |
| --- | --- | --- | --- | --- |
| `C00-G1` | Voice | Do you see? One who follows the edict does not fall. |  | Existing |
| `C00-G2` | Voice | A sentence is not written all at once. It builds in your body, one line at a time. |  | New |
| `C00-G3` | Voice | For your trial I grant you 1,000 gold. Go to the sanctuary village. There your hunt shall begin. |  | Existing |
| `C00-G4` | Hero | …I will follow your command. |  | Existing |
| `C00-G5` | Narration/system | **Choice** Take the reward and go to the village |  | Existing |

### C00-H · Variants · falling and replay

The existing wording stays.

| ID | Speaker | Line / instruction | Context · control | Status |
| --- | --- | --- | --- | --- |
| `C00-H1` | Voice | Rise. The command I gave you is not yet fulfilled. | When the hero falls in combat | Existing |
| `C00-H2` | Voice | When the red warning fills, leave that ground. The edict shall guide you. |  | Existing |
| `C00-H3` | Narration/system | **Choice** Rise again |  | Existing |
| `C00-H4` | Narration/system | This is a replay. It runs on a level 1 copy, and no settings or rewards are kept on your account. | Replay start notice | Existing |
| `C00-H5` | Voice | A replay grants no reward. Your true journey remains as it was. | Replay victory | Existing |

**Implementation notes (part E)**

- Bring back the descent and scroll staging (`DescentBeat`, `ScrollBeat`) that the v3 flow skips. Today v3 opens with two polite-register lines.
- Replace the instruction lines (`SkillLessonStep`, `SurvivalLessonStep`, `ProgressiveSkillLessonStep`, `ProgressiveSurvivalLessonStep`) with the wording in these tables. Control names are updated after the Hunt Edict UI overhaul.
- Remove from `en.txt` the translation keys of v3 wording that is no longer used (`NoEntryIsLeftBehindByARewordedLine`).
- Anton's arrival lines (`ShowArrivalDialogue`) move to chapter 1.


## Chapter 1 — The First Page

| Item | Content |
| --- | --- |
| Guide | Anton Jindark (`anton-jindark`) |
| What it teaches | Walking to the rift keeper, the pre-entry check (blade, power, potions), and how a rift runs by itself (kill gauge, boss) |
| Guides | F01, F02, F03 |
| Starts | When the player arrives in town after the prologue |
| Length | About 90 seconds (excluding the first rift fight) |
| Page | Page 1 · The First Page · The door is open, and the edict writes the fight |
| Reward | 200 gold (placeholder), collected after meeting Ish |

### C01-A · Arrival in town · Anton's first greeting

Shown once after the prologue, once the "Ashwood · Settlers' village" card has passed (replaces `ShowArrivalDialogue`). Anton speaks and the hero listens on the right.

| ID | Speaker | Line / instruction | Context · control | Status |
| --- | --- | --- | --- | --- |
| `C01-A1` | Anton | So you came. A pillar of light struck down beyond the woods and I wondered who had fallen… It was you. |  | New |
| `C01-A2` | Anton | I am Anton Jindark, the sanctuary's rift keeper. Every time a rift opens, I write in my ledger how many went in and how many came out. |  | New |
| `C01-A3` | Anton | Long ago the god imprisoned the demons not with blades but with writing. A rift is where that seal was torn. |  | New |
| `C01-A4` | Anton | Hence the prophecy: one of the Chosen will carve the edict into their body and slay the rift's demons. …You, who just received the scroll. |  | New |
| `C01-A5` | Anton | Chosen ones like you gathered here, and so smiths, merchants and jewelers settled in too. That is how this village came to be. |  | New |
| `C01-A6` | Anton | I will open your first rift. Come and speak to me. Tap the screen to walk, or use the move pad or WASD. |  | New |
| `C01-A7` | Narration/system | **Choice** Go to Anton | Choice (existing wording) | Existing |
| `C01-A8` | Narration/system | **Choice** Look around | Choice (existing wording). It closes and leaves the `!` over Anton | Existing |

### C01-B · Walking over (F01)

A gold outline marks Anton. It is not forced; the player may go elsewhere.

| ID | Speaker | Line / instruction | Context · control | Status |
| --- | --- | --- | --- | --- |
| `C01-B1` | Anton | **Guide** I'll be standing at the rift. Come close and press 'Talk'. | Outline on Anton · button `town-talk` | New |

### C01-C · At the rift door (F02)

Once, when the rift entry window (`RiftEntryWindow`) opens for the first time.

| ID | Speaker | Line / instruction | Context · control | Status |
| --- | --- | --- | --- | --- |
| `C01-C1` | Anton | Check three things before the door opens: the blade in your hand, the power written on your body, and your potions. |  | New |
| `C01-C2` | Anton | Normal entry runs at 1x, and fatigue keeps flowing even when you pause. Be careful when you step away. |  | New |
| `C01-C3` | Anton | When you are ready, press 'Enter Rift'. Coming back to be written in my ledger is up to you. |  | New |
| `C01-C4` | Anton | **Guide** If you are ready, press 'Enter Rift'. | Outline on `rift-enter-normal` | New |

### C01-D · Inside the rift (F03)

Three short barks (`TutorialCinematic.Bark`) that do not cover the battle. No window opens during combat.

| ID | Speaker | Line / instruction | Context · control | Status |
| --- | --- | --- | --- | --- |
| `C01-D1` | Anton | **Bark** The edict handles the moving, the striking and the looting. You just watch, and fix the settings. | Two seconds after entering | New |
| `C01-D2` | Anton | **Bark** See the kill gauge filling? When it is full, the boss appears. | At the first kill | New |
| `C01-D3` | Anton | **Bark** That's the boss. Bring it down within the time limit. Trust the edict. | When the boss appears | New |

### C01-E · After coming back

Once, when the player talks to Anton again after the first rift (win or lose). It leads on to Ish's chapter (chapter 2).

| ID | Speaker | Line / instruction | Context · control | Status |
| --- | --- | --- | --- | --- |
| `C01-E1` | Anton | You're back. I wrote a line in the ledger. Those who return always get a longer line. | After a win | New |
| `C01-E2` | Anton | …I'll write 'returned' in the ledger. Falling and coming back still counts as coming back. | After a fall (instead of E1) | New |
| `C01-E3` | Anton | Go and talk to Ish, the scribe. He will read the fight you just fought. | Turns on the `!` over Ish | New |
| `C01-E4` | Anton | No hurry. The door isn't going anywhere. | When talking to Anton again after "Later" or "Look around" | New |

**Implementation notes (part E)**

- Replace `ShowArrivalDialogue` with scene A. Anton speaks and the hero listens on the right, the two now facing each other.
- F01 is recorded in `OpenStation(RiftKeeper)`, F02 on rift entry and F03 at the first kill (existing code). Once all three are done page 1 can be claimed, but the page-fill staging is shown after meeting Ish (C02-A3).
- The three barks use `TutorialCinematic.Bark`. The existing rule of opening no window during combat is kept.
- After the first rift result, talking to Anton turns on the `!` over Ish (Ish is added as an NPC in part D).


## Chapter 2 — The Page Read in Death

| Item | Content |
| --- | --- |
| Guide | Ish (`ish-ashscribe`) |
| What it teaches | Reading the last five seconds of a result, and fixing one edict line that matches the cause of the danger (sentence 3) |
| Guides | F04, F05 |
| Starts | When the player returns to town after the first rift result (win or lose) |
| Length | About 90 seconds |
| Page | Page 2 · The Page Read in Death · The last five seconds of a fallen one are the most honest |
| Reward | 200 gold (placeholder) |

### C02-A · Meeting Ish (all cases)

When the player returns to town after seeing the first rift result, Ish steps into the place of the current notice card (`TryEdictNotice`). The card speaker changes from Anton to Ish.

| ID | Speaker | Line / instruction | Context · control | Status |
| --- | --- | --- | --- | --- |
| `C02-A1` | Ish | Pleased to meet you, scribe. I am Ish, the Scribe of Ash. |  | New |
| `C02-A2` | Ish | This is the Ember Scroll. Each time you write a line into the edict, one page of it kindles. The preface and the first page are already alight. |  | New |
| `C02-A3` | Ish | This is the ember-fee for the first page. It is nearly the only gift a scribe gives, so please take it. | The page reward of chapter 1 (200 gold) is granted here | New |

### C02-B · If the hero fell (the rift ended in death)

Used when the last record ended in death.

| ID | Speaker | Line / instruction | Context · control | Status |
| --- | --- | --- | --- | --- |
| `C02-B1` | Ish | You fell in the rift just now. My work is to read that last page. |  | New |
| `C02-B2` | Ish | …Let me correct that: the last page of one who fell and came back. The coming back is what matters. | Dry joke: as if scribes only read the dead | New |
| `C02-B3` | Ish | Let me read you the last five seconds. |  | New |
| `C02-B3a` | Narration/system | **Auto** Last 5 seconds: HP {0:0} → {1:0} · damage taken {2:0} | Existing text appended as is (`TryEdictNotice`) | Auto |
| `C02-B3b` | Narration/system | **Auto** Largest damage source: {0} · {1} hits · {2:0} damage | Existing text. Only when a damage source exists | Auto |
| `C02-B4` | Ish | The reason for a fall is usually written in the same place: the potion was drunk too late. |  | New |

### C02-C · If the hero won (the rift was cleared)

Used instead of B when the last record was a clear. The first-clear chest (F04) is also announced here.

| ID | Speaker | Line / instruction | Context · control | Status |
| --- | --- | --- | --- | --- |
| `C02-C1` | Ish | You cleared the rift just now. I read the pages of winners too. Winners are the ones who underestimate danger most. |  | New |
| `C02-C2` | Ish | Let me read you the last five seconds. | Same text as B3 | New |
| `C02-C2a` | Narration/system | **Auto** Last 5 seconds: HP {0:0} → {1:0} · damage taken {2:0} | Existing text appended as is | Auto |
| `C02-C3` | Ish | Even a victory carries its scars. Drink a little earlier and the scars grow fewer. |  | New |
| `C02-C4` | Ish | Oh, and the first rift you clear has a chest riding on it. Open 'First-clear rewards' at the rift entrance. | F04 · `rift-first-rewards` | New |

### C02-D · One prescription

Follows B or C. The point of this chapter is to change only one line at a time.

| ID | Speaker | Line / instruction | Context · control | Status |
| --- | --- | --- | --- | --- |
| `C02-D1` | Ish | One line is all you need to fix. The most common fix is raising the HP threshold for drinking a potion, so you drink earlier. |  | New |
| `C02-D2` | Ish | If the automatic potion was off, you may switch it on, and if it was a fight of nothing but being hit, you may change 'Avoidance by damage type' to 'Avoid all warnings'. |  | New |
| `C02-D3` | Ish | Save a change to any one of these three and this page fills. Still, change only one at a time; that is the only way to read what changed. | The code counts a save of any of the three as F05 (`TutorialProgress.EdictSaved`). One at a time is a recommendation, not enforced | New |
| `C02-D4` | Narration/system | **Choice** Try configuring | Choice (existing wording). Opens the edict and focuses `survival.potionHpPercent` | Existing |
| `C02-D5` | Narration/system | **Choice** Later | Choice (existing wording) | Existing |

### C02-E · In the edict window (gold-outline guidance)

Not forced. The outline moves step by step and one line from Ish appears beside it. The outline leads to the most common fix, the potion HP threshold (the F05 guide order in code). Saving a change to the automatic potion or to dodge also completes F05. After the Hunt Edict UI overhaul the control names are updated to the new screen.

| ID | Speaker | Line / instruction | Context · control | Status |
| --- | --- | --- | --- | --- |
| `C02-E1` | Ish | **Guide** Open 'Survival'. | `edict-tab-survival` | New |
| `C02-E2` | Ish | **Guide** Press 'HP potion threshold'. The higher the number, the earlier you drink. | `edict-option-survival.potionHpPercent` | New |
| `C02-E3` | Ish | **Guide** Type a number and press 'Apply'. | `edict-number-input` → `edict-number-apply` | New |
| `C02-E4` | Ish | **Guide** Press 'Save'. The body does not obey a sentence that was never written. | `edict-save` | New |

### C02-F · Confirmation and the page fills

Saving records F05 as performed. Show the page card "Ember Scroll · Page 2 · The Page Read in Death · The last five seconds of a fallen one are the most honest" and the notice "Reward · 200 gold".

| ID | Speaker | Line / instruction | Context · control | Status |
| --- | --- | --- | --- | --- |
| `C02-F1` | Ish | It is written. The next hunt will confirm it. Watch for what changes. |  | New |
| `C02-F2` | Ish | The fewer places you change, the easier the result is to read. It is the fastest way to learn a sentence. |  | New |
| `C02-F3` | Ish | There is no rush. I will keep the page open. Come when you are ready. | When the player chooses "Later" | New |
| `C02-F4` | Ish | The next page will be written by the next hunt. I only read. | When talking to Ish again after completion | New |

**Implementation notes (part E)**

- Change the speaker of the `TryEdictNotice` card from fixed `TutorialGuide` (Anton) to a per-guide guide: Ish for F05, F06, H07 and H08, Turk for E02, and Ish by default for the other edict guides.
- Choose B or C by whether the last record in `a.records` ended in the hero's death. The five-second danger lines reuse the existing templates, so no new translations are needed.
- In code F05 is recorded when a save changes any of the potion HP threshold (`survival.potionHpPercent`), the automatic potion (`survival.potion`) or the ground-attack dodge policy (`dodge.ground.policy`) (`TutorialProgress.EdictSaved`). How many places were changed is not limited. The lines name all three and recommend changing one at a time without enforcing it.
- When Ish is first met, grant the page reward of chapter 1 with `ClaimChapter("C01")`, and call `ClaimChapter("C02")` at the end.
- F04 is recorded automatically by code when the result screen opens. The first-clear chest line (C02-C4) is spoken only after a win.


## Chapter 3 — The Broken Blade

| Item | Content |
| --- | --- |
| Guide | Marc Kus (`mark-kus`) |
| What it teaches | Comparing new gear with what is worn, equipping it if it is better, and enhancing gear at the forge |
| Guides | H01, UL02 |
| Starts | When the player talks to Marc after gear enters the bag. Enhancing (B) comes after the first clear |
| Length | About 90 seconds |
| Page | Page 3 · The Broken Blade · Forge the tool first |
| Reward | 200 gold (placeholder) |

### C03-A · Comparing gear (H01)

After gear enters the bag in the first rift, a `!` appears over Marc. Talking to him starts it. H01 is recorded when a comparison or equip transaction happens.

| ID | Speaker | Line / instruction | Context · control | Status |
| --- | --- | --- | --- | --- |
| `C03-A1` | Marc | Welcome. I hear metal ringing in your bag. Iron picked up in a rift keeps crying until it meets its owner. |  | New |
| `C03-A2` | Marc | Whether new gear is truly better is told by numbers, not eyes. Start by laying it beside what you wear. |  | New |
| `C03-A3` | Marc | Open your bag and tap a piece of gear. The comparison will appear side by side. |  | New |
| `C03-A4` | Marc | **Guide** Open your bag. | Bag button in the content dock | New |
| `C03-A5` | Marc | **Guide** Tap the new gear. The numbers against what you wear will show. | A gear cell in the bag | New |
| `C03-A6` | Marc | **Guide** If it's better, press 'Equip'. If not, leave it. Iron doesn't run away. | Equip button in the item detail | New |
| `C03-A7` | Marc | Good eye. Iron doesn't lie. Just do as the numbers say. | Right after H01 is performed | New |

### C03-B · How to forge (UL02)

Opens only at account best clear of Rift 1 or more (`ContentUnlocks`). Uses the first-time practice support (one cost-covered try per account).

| ID | Speaker | Line / instruction | Context · control | Status |
| --- | --- | --- | --- | --- |
| `C03-B1` | Marc | Now I'll teach you to forge. Even good iron only hardens by being struck. |  | New |
| `C03-B2` | Marc | Open 'Equipment enhancement' at the forge and pick a +0 piece. |  | New |
| `C03-B3` | Marc | I'll cover the fire cost of your first quench. Just once per account. | Introduces the first-time practice support (`tutorial-support-execute`) | New |
| `C03-B4` | Marc | **Guide** Pick the gear to enhance. | Forge gear selection | New |
| `C03-B5` | Marc | **Guide** Press 'Enhance +1'. | The first enhance button on the forge equipment-enhancement screen | New |
| `C03-B6` | Marc | Look. The same iron, struck once, has changed this much. Even a broken blade starts like this. | Right after UL02 is performed. Page 3 fills | New |
| `C03-B7` | Marc | I'll teach enhancing after you clear a rift once. I'll light the fire then. | When there is no first clear yet (the first rift ended in a fall) | New |
| `C03-B8` | Marc | No need to rush. Iron can be heated again after it cools. | When the player chooses "Later" | New |

**Implementation notes (part E)**

- Turn on Marc's `!` when H01 is open and there is gear in the bag (the H01 discovery condition in `ObserveTutorialContext`).
- UL02 opens at account best clear of Rift 1 or more. If the first rift ended in a fall and there is no clear, do only A and postpone B with B7.
- The enhancing practice uses the first-time practice support (`TutorialPractice`, UL02 is `supported`).
- Page 3 fills only when both H01 and UL02 are done.


## Chapter 4 — The Training Ground's Cane

| Item | Content |
| --- | --- |
| Guide | Turk Garbi (`turk-garbi`) |
| What it teaches | Testing safely at the training ground (UL01), and picking one combat style card (E02, sentence 4) |
| Guides | UL01, E02 |
| Starts | A any time after arriving in town. B after clearing Rift 2 |
| Length | A about 60 seconds, B about 60 seconds |
| Page | Page 4 · The Training Ground's Cane · Temperament is set before the blade |
| Reward | 200 gold (placeholder), once both UL01 and E02 are done |

### C04-A · Meet the training ground (UL01)

The training ground is open from arrival. A `!` appears over Turk. UL01 is recorded when a training run ends.

| ID | Speaker | Line / instruction | Context · control | Status |
| --- | --- | --- | --- | --- |
| `C04-A1` | Turk | You made it, recruit! I'm Turk Garbi, instructor of this training ground! |  | New |
| `C04-A2` | Turk | Here, dying costs you nothing. No rewards, no fatigue. In return, you can test anything to your heart's content! |  | New |
| `C04-A3` | Turk | If you changed the edict, try it here first. Testing it in the real thing and dying is a fool's game! |  | New |
| `C04-A4` | Turk | **Guide** Press 'Train'! | `npc-service` in the NPC dialogue | New |
| `C04-A5` | Turk | **Guide** Press 'Begin the fight'! | `training-start` in the training window | New |
| `C04-A6` | Turk | Look at how long the dummy took to fall. Remember the record. You'll compare against it later! | Right after the training result screen. UL01 is performed | New |
| `C04-A7` | Turk | No hurry, rest! Resting is training too! | When the player chooses "Later" | New |

### C04-B · A fighter's temperament (E02 · sentence 4)

After the player clears Rift 2, Turk is the speaker of the notice card. The player picks one of the three style cards on the summary tab and saves. Page 4 fills only when both UL01 and E02 are done.

| ID | Speaker | Line / instruction | Context · control | Status |
| --- | --- | --- | --- | --- |
| `C04-B1` | Turk | You've won two rifts! Now I'll tell you: every fight has a temperament! |  | New |
| `C04-B2` | Turk | The one who pushes, the one who balances, the one who measures distance. Temperament is set before the blade. |  | New |
| `C04-B3` | Turk | On the Hunt Edict 'Overview' there are three cards. Pick just one and save. That is your temperament! |  | New |
| `C04-B4` | Narration/system | **Choice** Try configuring | Choice (existing wording). Opens the summary tab | Existing |
| `C04-B5` | Turk | **Guide** Pick a card! | Outline on `edict-style-balanced` (any of the three works) | New |
| `C04-B6` | Turk | **Guide** Save it! A temperament never written is no temperament at all! | `edict-save` | New |
| `C04-B7` | Turk | Good! Now check it on the dummy. Aggressive finishes fast and takes more hits. Careful is the opposite! | Right after E02 is performed. Page 4 fills | New |

**Implementation notes (part E)**

- UL01 is recorded by `TutorialProgress.Result` when a training run ends (existing code).
- Turk speaks the E02 notice card. E02 opens after Rift 2 (`HuntEdictProgression.GuideVisible`).
- Page 4 fills only when both UL01 and E02 are done, so after clearing Rift 2. The page 5 guide (F06) may come first in the meantime.
- The new chapter engine's `Current` skips finished guides, so a locked E02 does not hold back the next chapter (part B fix).


## Chapter 5 — A New Fang

| Item | Content |
| --- | --- |
| Guide | Ish (`ish-ashscribe`) |
| What it teaches | Learning a new skill with skill points, equipping it in a slot, and that passives apply without being equipped |
| Guides | F06, H07 |
| Starts | When the hero is level 2 or higher and has an active skill never equipped |
| Length | About 90 seconds |
| Page | Page 5 · A New Fang · A power written must be held to bite |
| Reward | 200 gold (placeholder) |

### C05-A · A new fang (F06 · H07)

A notice card appears when the hero is level 2 or higher and has an active skill never equipped (`TutorialProgress.NewSkills`). Ish is the speaker.

| ID | Speaker | Line / instruction | Context · control | Status |
| --- | --- | --- | --- | --- |
| `C05-A1` | Ish | Scribe, you have gained a level. A corner of the scroll says 'a new fang has grown'. |  | New |
| `C05-A2` | Ish | You have skill points. Spend them to learn a skill, and equip the learned skill in a slot so the hunt uses it. |  | New |
| `C05-A3` | Ish | A fang that was grown but never set in the jaw has never bitten anything. |  | New |
| `C05-A4` | Narration/system | **Choice** Try configuring | Choice (existing wording). Opens the skills tab | Existing |
| `C05-A5` | Narration/system | **Choice** Later | Choice (existing wording) | Existing |

### C05-B · In the edict window (gold-outline guidance)

Not forced. The skill to learn is decided by `TutorialProgress.NewSkills`.

| ID | Speaker | Line / instruction | Context · control | Status |
| --- | --- | --- | --- | --- |
| `C05-B1` | Ish | **Guide** Open 'Skills'. | `edict-tab-skills` | New |
| `C05-B2` | Ish | **Guide** Tap the skill you can newly learn. | `edict-skill-<new skill>` | New |
| `C05-B3` | Ish | **Guide** Spend a point with '+'. | `edict-skill-rank-up` | New |
| `C05-B4` | Ish | **Guide** Set it in a slot with 'Equip'. | `edict-skill-equip` | New |
| `C05-B5` | Ish | **Guide** Press 'Save'. Only what is written becomes a fang. | `edict-save` | New |

### C05-C · Confirmation and the page fills

Equipping and saving the new skill records F06 as performed. Show the page card "Ember Scroll · Page 5 · A New Fang · A power written must be held to bite" and the notice "Reward · 200 gold". H07 is recorded when points are spent.

| ID | Speaker | Line / instruction | Context · control | Status |
| --- | --- | --- | --- | --- |
| `C05-C1` | Ish | It is set. You can also decide when and how to use this skill… but we will read that slowly on the later pages. |  | New |
| `C05-C2` | Ish | By the way, passives are already part of your body even when not equipped, so do not worry about them. |  | New |
| `C05-C3` | Ish | If you have points left, growing a skill you already know is a fine sentence too. | H07. Only when skill points remain | New |
| `C05-C4` | Ish | There is no rush. A fang takes time to grow. | When the player chooses "Later" | New |

**Implementation notes (part E)**

- F06 opens when `TutorialProgress.NewSkills` is not empty. H07, which spends points, opens at level 2 and is recorded when `Spent` grows.
- Page 5 fills only when both F06 and H07 are done. Learning a new skill spends points, so they usually finish in the same save.
- F06 and H07 are recorded per hero (`perHero`). Each other hero is guided again.

## Review status

- 156 lines in total: 60 existing, 3 reworded, 93 new.
- 95 entries must be added to `en.txt`. The English in this document goes in as written.
- Line length, a leading placeholder and placeholder pairing were checked automatically. Result: no problems.
- The 21 quoted on-screen labels were checked against the strings in the source (2026-10-05). Quotes that are not screen labels, or older guide lines whose string does not appear verbatim: `돌아옴`, `새 이빨이 돋았다`.


## Open questions

1. Whether Ish calling the hero "scribe" suits the Warrior, Ranger and Mage alike.
2. The page reward of 200 gold is a placeholder. Its economic effect has not been reviewed.
3. Ish's NPC position and look are decided in part D. The portrait is settled (option B) and not yet placed in town.
4. When the Hunt Edict UI overhaul is done, recheck the control names and positions in the guide lines against the new screen.
