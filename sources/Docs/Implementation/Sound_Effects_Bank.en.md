# Sound effects replacement

Date: 2026-09-26 · [한국어](Sound_Effects_Bank.md)

Every sound effect in the game was remade and wired in. The bank replaces the 20 temporary synthesized cues on `main` and covers system sounds (buttons, handling equipment, gold, salvage), all 54 class skills, battle cries, pain and death voices, 12 enemy archetypes, elites, 3 bosses and rift events: **283 sounds with 469 variations**. The sanctuary and rift music loops are unchanged.

## Direction and provenance

The target is the texture Diablo IV is known for: heavy, gritty impacts, the feel of iron, leather and stone, short tails, a distinct grain for each element and a low, ringing legendary drop. The feel was the reference. **No Diablo IV audio, recording or sample was extracted or used**; copyrighted game audio cannot go into this project.

All sounds are original, synthesized from seeded recipes in [`tools/generate_sfx.py`](../../tools/generate_sfx.py) and [`tools/sfx`](../../tools/sfx/bank.py). No outside recordings, speech engines or AI audio models were used. Main techniques:

- Metal, stone, glass and coins use modal synthesis with each object's resonance ratios. Bowstrings use a Karplus-Strong string.
- Impacts layer a pitch-dropping sub, a saturated mid crack, wet flesh and bone-crunch grains.
- Swings and dashes are noise through moving filters. Fire, frost, lightning, poison and shadow differ in noise colour and grain density.
- Reverb comes from synthesized impulse responses per room size (small room, hall, crypt, vast space).
- Voices are non-verbal vocalizations from a glottal pulse source (jitter, shimmer, period doubling) and moving vocal-tract resonators. The warrior uses an adult male range; the ranger and mage use adult female ranges. Monsters get lower resonances, subharmonics and distortion.

The author cannot listen to audio. Structure was checked with spectrograms, frequency-band ratios, loudness measurement and truncation checks only; listening evaluation belongs to the user. The synthesized voices in particular may sound more artificial than recorded voice acting. Every sound id can take a recorded replacement.

## Contents

| Category | Sounds | Variations | Covers |
| --- | ---: | ---: | --- |
| Interface | 11 | 25 | Normal, primary and destructive buttons, tabs, unavailable button, window open/close, confirm, reject, slider, notice |
| Item handling | 25 | 48 | Pick up and equip for 9 materials (blade, plate, leather, jewelry, bow, staff, orb, scroll, shield), place, lock, salvage and bulk salvage, discard, sort |
| Gold & materials | 9 | 19 | Gold pickup (with a large variant), spend, receive, material/core/stone/gem pickup, bag full |
| Loot drops | 5 | 10 | Drops by rarity: common, magic, rare, legendary, set/unique |
| Town services | 25 | 35 | Reroll and jackpot, enhancement, forge job start/finish, masterwork, station unlock, core craft/investment, gems, runes, aspects, reward boxes, attendance, gamble beat |
| Progression | 15 | 18 | Enter game, three class picks, begin adventure, rift enter/return/portal, clear, failure, level up, new skill, tutorial, objective, boss gate |
| Class skills | 73 | 94 | Three basic attacks, 54 actives and ultimates, follow-ups such as whirlwind spins, landings, explosions, chain hops, trap triggers and summon strikes |
| Hits | 12 | 39 | Slash, pierce, heavy, fire, cold, lightning, poison, shadow, critical, bone/metal/crystal materials |
| Hero | 7 | 17 | Hurt, block, dodge, barrier absorb, collapse, low-health heartbeat, potion |
| Voices | 18 | 48 | Per class: effort, ultimate shout, battle cry, pain, death, gasping |
| Enemies | 45 | 77 | Wind-up, attack and death for 12 archetypes; poison/fire/ice/blast hazards; elite kill, rage, heal, bell, corpse explosion |
| Bosses | 21 | 21 | Arrival for 3 bosses; wind-up and release for basic, slam, hook, summon, beam, charge and blasts; enrage, stagger, death, minions |
| Rift events | 17 | 18 | Golden goblin appear/flee/escape/death, chests, shrines, cursed chest, seals, offerings, rune found |

Sources are 48 kHz, 16-bit mono WAV: 51.3 MB and 8 min 54 s in total. The folder imports clips up to 2 s as ADPCM and longer ones as Vorbis at quality 0.7, both compressed in memory. The list of sounds, their Korean and English names and playback rules is [bank.json](../../Assets/HELLSCRIPT/Resources/Audio/Sfx/bank.json).

## Wiring

Sound only reads game state. It never touches combat randomness (variations use a separate `System.Random`) and never runs a save or transaction.

| Moment | Hook |
| --- | --- |
| Buttons | Real pointer or keyboard presses on the shared `UiButton`, by role. If the button's consequence (a purchase, an equip, a window) sounds in the same or next frame, the click is dropped. Unavailable buttons give a dull tick. `onClick.Invoke()` from code is silent. |
| Windows | When `ContentWindowHost` pushes or removes a window |
| Selecting and dragging items | Tapping or starting a drag on an inventory, storage, shop, forge or aspect-stone cell plays the material's pick-up sound. An illegal slot plays the reject; dropping on nothing plays a put-down. |
| Equip, lock, salvage, sort | Chosen from the kind of committed transaction (`GameStore.Committed`): the newly equipped item's material, the new lock state recorded in the operation (inventory and storage), and the number of items that left the bag (3 or more is a bulk salvage). Installing the empty gem is a removal; any other install or replacement is a socket. An account switch re-subscribes to the new store. |
| Discard | The game has no drop-to-floor action. The sound is used for leaving loot behind when the bag is full (`leave-loot`). |
| Gold & materials | Resources actually picked up in a rift (`RiftResourceDrop.claimed`). Gold 2.5× above the running average uses the large sound. Shop buy/sell, gambling and storage purchases are told apart by transaction kind. |
| Loot | A rarity sound when an item becomes visible, the material pick-up when it is collected |
| Forge, gems, runes, aspects, rewards | After the transaction is saved. Auto-reroll finding its target plays the jackpot. The rune board sounds lift, place, rotate, remove and cell unlock while editing. |
| Saves during a rift | Committing a shrine, chest or offering rebuilds the run's record lists with new objects. Reading resumes from the same record found by value; if it cannot be found, earlier history is not replayed. |
| Failures | A refused transaction (`GameStore.Rejected`) plays the reject only if the player gave input within 0.75 s; background settlement failures stay silent. |
| Flow | Title entry, class pick (per class), begin adventure, tutorial prompts, rift entry (hero battle cry 1.5 s later), return, clear, failure, level up (plus a notice when a skill unlocks) |
| Skills | The real skill lifecycle (`ClassSkillChanged`, or `actionEvents` on the legacy path): start, travel and release. Whirlwind sounds per spin tick; fireball explosions, chain hops and venom traps at their moment; summons, fields, arrow rain and shadow echoes through `DAMAGE` events. Heavy skills add an effort voice, ultimates an ultimate shout. |
| Hits | The element of the damage and the attacker's class. Criticals add a sweetener; bone, armored and crystal enemies add a material layer. Damage over time is silent. |
| Hero | Hurt (pain voice at 6% of max health or more), block, dodge, absorb, heartbeat and gasping below 30% health during a fight (the gasp retries if another line holds the voice; it stops when the fight ends or pauses), collapse and death cry, potion |
| Enemies & bosses | Enemy combat records (`enemyEvents`): wind-up, release, hazards, heal, bell, rage, corpse; death records (`effectEvents`): per-archetype death and elite kill. Bosses: arrival per boss, attack wind-ups and releases, enrage, stagger, death. |
| Rift events | Chest, shrine, curse, seal, offering, gate, golden goblin and rune tags in the combat log |

## Mixing rules

- 16 effect voices are shared. Each sound has a cooldown, a simultaneous limit and a priority; rewards, bosses and danger outrank repeated hits.
- The hero says one line at a time; a more important line (death) cuts off a lesser one (pain).
- Variations never repeat back to back. Pitch varies by ±2–6% per sound and never follows combat speed.
- Enemy and loot sounds are attenuated by distance from the hero and panned left/right. When many sounds overlap, each new one is slightly quieter, and a limiter on the listener keeps the final mix under -3 dBFS.
- Default loudness targets per category (short-term LUFS approximation): interface -25, hits -23, items -22, gold and enemies -21, town, hero, voices, loot and rift events -19, skills -17, progression and bosses -15. Legendary drops and ultimates are set louder.
- Existing volume settings (master, music, effects, mute), device-only storage, pausing in the background and not replaying history on resume are unchanged.

## Building and reproducing

```sh
python3 tools/generate_sfx.py            # render WAVs, Unity metas and bank.json
python3 tools/generate_sfx.py --check    # recipes match the files; ids used by code exist
python3 tools/generate_sfx.py --audition Artifacts/Audio   # local listening page
python3 tools/generate_sfx.py --sheets Artifacts/Audio/sheets  # spectrogram sheets
```

Rendering needs NumPy and SciPy; the game does not. `--check` also confirms that every sound id written in code and every active or ultimate in the skill catalog is in the bank.

## Verification

Checked with Unity 6000.6.0f1 and a macOS development build. Another task's Editor was open on the main checkout, so tests and builds ran in batch mode on a clone of the work folder based on `main` 24694040.

| Check | Result |
| --- | --- |
| Audio files | All 469 peak at or below -1.0 dBFS, 0 clipped samples, first and last samples 0, largest DC offset 0.0005. `--check` confirms recipes and files match, and that the 88 ids written in code, the composed ids and the skill catalog ids are all in the bank. [Audit](SfxEvidence/bank-audit.json) |
| New Edit Mode tests | 74/74 pass: bank load with 0 missing clips, sounds for every skill, enemy, boss and voice, material and rarity mapping, cooldown and mute, transaction sounds (lock, unlock, storage lock, gem socket and removal) with the reject only for player-caused failures, only the new store heard after an account switch, no replay of earlier history after the run is rebuilt from JSON, buttons reporting real input only, **all 54 actives and ultimates heard from their real cast lifecycle with identical damage and randomness with or without audio**, plus the 12 existing button tests. [Results](SfxEvidence/editmode-sound.xml) |
| Full Edit Mode | With the final pre-merge code, 3,986 of 4,033 pass. The 47 failures are the same 47 that fail in the baseline full run of `main` 24694040; 0 new failures, and all 62 new tests pass. [Comparison](SfxEvidence/editmode-full-compare.txt) |
| Independent pre-merge review | Reviewers on five dimensions (playback core, combat wiring, transaction wiring, screen changes, generator and tests) raised 21 findings; three refutation attempts per finding left 12 confirmed (10 after duplicates). Fixed: transaction sounds going silent after an account switch, history replaying after a rift save, the heartbeat continuing on the result screen and while paused, the low-health gasp being lost, the first gold drops using the large-pile sound, the gem replace sound, the storage lock sound, silent tap-to-place runes, automatic combat sounds hiding button clicks, and a lock test that never ran. |
| Shared UI contract | `check_ui_contract.py` passes; `test_ui_contract.py` 9/9 |
| Real play smoke | The development build's `-hellscriptSfxSmoke` plays the game through synthetic EventSystem input and records every sound that starts plus the peak level at the listener. Final pre-merge code: `HELLSCRIPT_SFX_SMOKE_OK`, 84 distinct sounds, 1,341 plays, 0 errors logged. Across the seven runs made while fixing the smoke, 92 distinct sounds were heard in real play. [Summary](SfxEvidence/runtime-summary.txt) · [Sound trace](SfxEvidence/runtime-trace.tsv) |
| Mix | Before the limiter, the listener peaked at 1.034 in the mage rift, a momentary clip. With the limiter, every phase stays at or below 0.700. |

The smoke played: title entry, all three class picks and the start of the adventure, the mandatory tutorial fight (with its boss) and equipping from the comparison view, the warrior clearing rift 1 (boss arrival, enrage and death, the clear stinger, chests, level up, new skill, gold, materials, gems, stones, loot), 75-second rifts for the ranger and mage (basic attacks, skills, fire explosions, elemental hits, battle cries, elite kills), selecting, locking, unlocking, dragging to an illegal slot and salvaging in the inventory, a gamble purchase (gold spend, beat, rarity reveal), a forge enhancement and the preview in sound settings.

Not verified:

- The smoke did not complete a drag onto a valid slot (it found no item/slot pair). The equip sound was confirmed through the comparison view's equip button; a drag equip goes through the same saved transaction.
- Tutorial guide windows covered the gamble buy, reveal confirm, shop close, forge enhance and forge close buttons, so those were invoked directly instead of by pointer (their sounds come from the saved transaction, so the coverage is the same). The forge reroll was not run because the smoke could not find the item cell, so its sound was not confirmed in play.
- The heroes are low level, so each class used one skill in the rifts. All 54 are covered by the Edit Mode real-cast tests.
- No human listening evaluation, no physical mobile audio, touch or performance check, and no Windows run.

![Character selection](SfxEvidence/02-character-select.png)

![Mage rift fight](SfxEvidence/05-rift-2.png)

[Ranger rift end](SfxEvidence/06-rift-end-1.png) · [Inventory detail](SfxEvidence/07-inventory-detail.png) · [Sound settings preview](SfxEvidence/11-sound-settings.png)

## Relation to earlier work

- `main`'s 20 `SoundCue` values and their runtime synthesis were removed. `GameAudio.Clips.cs` now only synthesizes the music.
- The unmerged branch `codex/dark-fantasy-audio` (`45e0b468`) from the same day is not the base of this work. Both rewrite `GameAudio`, so do not merge them together.
- Earlier record: [Temporary audio and minimal skill presentation](Audio_Skill_Presentation.en.md)
