# HELLSCRIPT play guide

Date: 2026-09-13 · [한국어](Local_Play_Guide.md)

Open `Builds/macOS-Playable-Build2/HELLSCRIPT.app` in Finder. In Unity, open the HELLSCRIPT project and open `Assets/HELLSCRIPT/Scenes/Hellscript.unity` and press Play. Launching without validation flags uses the normal player save.

## First play

1. Choose Warrior, Ranger or Mage in the sanctuary. Review unlocked abilities in Growth and Skills.
2. Edit and save the Hunt Edict v0.2, check which configuration is active, then enter a rift. Movement, attacks and chest interactions run automatically according to the configuration.
3. Pause or inspect settings and decision records during combat. Equipment and socket changes wait until the rift ends.
4. Read the result after a clear or failure. Experience, equipment and resources already collected remain yours. Open equipment, blacksmith and storage to compare, equip, sell, dismantle or enhance gear, then adjust your behavior as needed.
5. After acquiring gems, open Gem storage for effects and fusion requirements. Create a socket on eligible rare-or-better equipment, then install a gem. Costs appear before confirmation.
6. Retry or configure repeat-hunt success, failure and stopping policies. If storage fills, organize it at the portal and return to the rift.
7. After restarting the app, choose Resume current rift. Saved repeat results require review and an explicit resume.

Settings and Help contains language, text size, screen orientation/PC display and audio controls. Training and A/B comparisons do not consume actual equipment. The game can show result statistics; no balance measurements were run for this development phase.

## Scope

This is a playable local macOS development build. The loop covers hero selection, edict configuration, generated rifts, combat and chests, rewards, equipment/gem/rune growth, retry/repetition and save recovery. Presentation uses simple shapes, temporary models and temporary audio.

Server login, cross-device synchronization, real payments and store distribution are not connected. Existing speed locks and planned entitlement policies remain. Physical mobile quality, release assets, detailed balance and long-term farming adjustments are outside this playable milestone. Small unresolved values such as storage capacity use tunable defaults.

[Implementation record](Playable_Completion.en.md) · [Current status](../../Wiki/content/current-status.md)
