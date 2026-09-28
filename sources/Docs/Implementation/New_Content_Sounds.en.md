# Sounds for the New Enemies and the Moving Boss Attacks

Updated: 2026-09-28 · [한국어](New_Content_Sounds.md)

The eight new regular enemies (N13–N20) and some of the bosses' new moving attacks attacked in silence. They now borrow the closest recorded sound. No new sound was made or generated through an outside service, and no game rule or value changed. This is a side item of stage 2 of the [dark gothic overhaul](Dark_Gothic_Overhaul_Stage1.en.md).

## Cause

Enemy windup, attack and death sounds are recorded per enemy (N01–N12), and boss attacks are tied to a cue per attack type (swing, slam, hook, summon, beam, charge, blast). N13–N20 had no recordings, so their sounds were not found. Of the bosses' new attacks, Reaping Sweep, Chain Whirl, Maw Snap and Tail Lash were missing from the cue list and fell back to the basic attack sound or none that fit.

## Borrowed sounds

| Enemy | Sound |
| --- | --- |
| N13 Dune Marauder | N01 Chained Corpse (blades) |
| N14 Burrowing Sandworm, N15 Cave Ghoul | N02 Grave Hound (beasts) |
| N16 Sporebloat | N06 Bloated Pilgrim; its spore cloud on death uses the poison pool sound |
| N17 Thornhorn Brute, N19 Frostbitten Revenant | N07 Ironclad Wraith (heavy undead) |
| N18 Briar Witch, N20 Rime Caller | N05 Funeral Priest (casters) |

For the bosses, Reaping Sweep uses the heavy swing, Chain Whirl and Maw Snap the charge, and Tail Lash the slam cue.

## Verification

On 2026-09-28, all 157 focused Edit Mode tests passed on the integrated project, including both `NewContentAudioTests`. The checks cover enemy windup, attack and death cues and sound routing for every boss attack. [Test summary](CompletedIntegrationEvidence/editmode-summary.txt)

## Not verified

- The sounds were not listened to in the running game.
- The borrowed sounds are stand-ins; dedicated recordings per enemy and attack are still to come.
