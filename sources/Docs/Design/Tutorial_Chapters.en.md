# Tutorial Chapter Design — The Chronicle of the Written

Updated: 2026-10-04

A design that lets new players learn one feature at a time, with a story attached. Work is split into parts A to I; this document records what part A fixed. Korean original: [Tutorial_Chapters.md](Tutorial_Chapters.md).

## Goals

- 52 guides listed like a manual overload new players. Group them into **story chapters**.
- Do not show the Hunt Edict (152 global options) all at once. Open it **one sentence at a time, most important first**.

## World spine

- An old god sealed demons not with blades but with **writing**. The seal is the edict. The name HELLSCRIPT means the document that holds hell shut.
- The hero, the Chosen, is a **scribe-warrior** who carves edicts into their body. Editing the Hunt Edict is carving a sentence.
- Each edict sentence learned fills a page of the Ember Scroll. The page count is both progress display and reward.
- Tone is Diablo IV-style dark, with light humour from NPC personality.
- The official place name is the already shipped **"Ashwood · Settlers' village"** (잿빛 숲 · 정착민 마을). "Ashen Refuge" in the GDD was a working title, so the GDD now lists the official name beside it.
- The only new character is **Ish, Scribe of Ash** (page rewards and the help shelf). Everything else reuses the 12 existing NPCs and the Voice of the Edict.

## Design principles

1. **Layering**: existing guide IDs (P00, F, UL, H, E) do not change. A chapter layer sits above and reads those ID groups.
2. One chapter = one feature = one NPC = about 90 seconds. One button at a time.
3. Each edict sentence teaches in three beats: **feel the problem → one fix → see the result**.
4. Forced steps stop at the prologue's edict and skill. After that, a `!` over the NPC invites and can be skipped.
5. Content unlock timing (`ContentUnlocks.json`) stays. Only edict disclosure stages follow the ladder below.
6. Every string ships in Korean and English.

## Save compatibility

- Guide IDs, `TutorialRecord` fields, `tutorialVersion` (1), the transaction key `tutorial-map-complete-v2` and fingerprints `tutorial-v2`/`tutorial-v3` do not change. `Tutorials.Version` is not raised (raising it makes saves at schema 17 or later refuse to load).
- Chapter state is stored only as **additional fields** on the account. When empty it is derived from existing records.
- Legacy accounts (`legacyExempt`) skip the prologue, and a chapter whose guides are all done counts as complete.

## Hunt Edict sentence ladder

Ordered by importance (survival, stability, efficiency, advanced). A sentence is announced by one card in town.

| No. | Feature | Disclosed | Hook | Three beats |
| --- | --- | --- | --- | --- |
| 1 | Potion HP threshold | Prologue | Voice: write it so you drink before you fall | Change 40 to 60% in the boss fight |
| 2 | First skill approach comparison | Prologue | Two sentences duel | A/B under equal conditions, then choose |
| 3 | Auto potion, default dodge | First result | Ish: the first page is read in death | Last 5 s of damage → toggle → retry |
| 4 | Combat style | Rift 2 | Turk: temperament is set before the blade | Pick a style card → compare in training |
| 5 | Retreat and return at low HP | Rift 3 (moved from 5) | Anton: running is not a number | Feel danger → retreat HP → confirm survival |
| 6 | Target choice | Rift 4 | Elites chant out of blade range | Quick target-switch setting |
| 7 | Position, distance, encirclement | Rift 5 (moved from 3) | Being surrounded | One of four quick settings |
| 8 | Direct editing, presets, training comparison | Rift 6 | Ish's page-count reward | Open details and save |
| 9 | Loot pickup | Rift 7 | Jake: haul it all and the bag weeps | Choose a pickup grade |
| 10 | Exploration, cursed chest | Rift 8 | Zack's dubious chest | Choose an exploration mode |
| 11 | Bag space and protection | Rift 9 | Marc: never sell what you enhanced | Choose a full-bag response |
| 12 | Auto equip, potion resupply | Rift 12, 14 | A dangerous sentence you must switch on | Switch on and off |
| 13 | Auto repeat | Rift 15 + rune guide | Inzel: let the runes run all night | Set a goal |
| 14 | Fine conditions (dodge detail, pursuit, loot filter) | Rift 18 | Ish's forbidden book | Change one thing in each of three groups |
| 15 | Action order, sharing | Rift 20 | Letting others read your script | Check the order |

- The only disclosure changes are sentences 5 and 7 (swapping the stages of E05 and E03) and sentence 14 (splitting E18 into three groups). Everything else keeps its current value.
- When a new sentence opens, the edict window highlights only that item as "New sentence" and keeps already open items collapsed.

## Chapters

| Ch. | Title | Guide | Existing guides / sentences |
| --- | --- | --- | --- |
| 0 | The Voice of the Edict | Voice of the Edict | P00 (sentences 1, 2) |
| 1 | The First Page | Anton Jindark | F01, F02, F03 |
| 2 | The Page Read in Death | Ish, Scribe of Ash | F04, F05 (sentence 3) |
| 3 | The Broken Blade | Marc Kus | H01, UL02 |
| 4 | The Training Ground's Cane | Turk Garbi | UL01, E02 (sentence 4) |
| 5 | A New Fang | Ish, Scribe of Ash | F06, H07 |
| 6 | The Art of Fleeing | Anton Jindark | Sentences 5, 6, 7 |
| 7 | The Price of the Useless | Jake Bokun, Chador Samaf | H02, H03, H04 |
| 8 | The Glittering Temptation | Guzel Pan | UL05 |
| 9 | The Silent Stone | Injel Mir | Rune board, 5 steps (unchanged) |
| 10 | A Dubious Deal | Jacques Chei | UL07 |
| 11 onward | Advanced chapters | The NPC in charge | Sentences 8 to 15, aspect, core, sweep, masterwork |

Existing H guides stay and can always be reread from Ish's help shelf (the guide list).

Order rationale: right after the first result, town shows the survival notice (F05) first, and survival ranks first on the ladder, so Ish's chapter (2) comes before Marc's (3). Chapter 0 is a preface with no page number; pages count from chapter 1.

F07 (retry the changed edict) opens together with direct settings at Rift 6 in code (`HuntEdictProgression.GuideVisible`), so it belongs to the advanced chapter for sentence 8. Placing it in chapter 3 or 4 would leave those pages unfilled until Rift 6.

## Chapter structure

1. **Hook**: the NPC gives the reason first (at most three lines).
2. **Action**: press one designated button (`PrologueGate`). Edict chapters follow the three beats.
3. **Check**: show the result (value change, comparison summary).
4. **Page fill**: a scroll page fills and a reward (consumable or effect) is granted once under the transaction key `chapter-reward:{chapterId}`.

## Parts

| Part | Content |
| --- | --- |
| A | Document cleanup, place name, save compatibility (this document) |
| B | Chapter engine: data, save field, recommendation wiring |
| C | Edict ladder: stage reorder, three beats, window highlight |
| D | Presentation and UI parts, Ish NPC |
| E | Chapters 0 to 5 content and smokes |
| F | Chapters 6 to 11 content |
| G | Art (Ish portrait, page illustrations) |
| H | Localization, documents, wiki |
| I | Final validation |

## Code and document differences (confirmed 2026-10-04)

[Tutorial progression and per-content guides](Tutorial_Flow_Design.md) differs from `Tutorials.cs`. Implementation follows the code.

- P00: the document describes the v2 flow, while the code defaults to v3 (compare two approaches, potion 40 to 60%).
- F05: the document says "change one Hunt Edict setting", the code says "review the first danger".
- H10: the document says "presets and sharing", the code says "local presets".
- The code's `Tutorials.All` has 52 entries (39, plus E02, E08C and 11 edict rule guides); the document table has 39.
