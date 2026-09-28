# Completed Work Integration

Updated: 2026-09-28 · [한국어](Completed_Native_Work_Integration_2026_09_28.md)

Completed changes from the separate worktrees were integrated into current `main`, preserving the previously merged tutorial staging and hero/enemy/boss model rendering.

- Added models and textures for N08, N09, N10, N13 and N14.
- Integrated [field environments](Field_Environments.en.md) and [hero skill effects](Hero_Skill_Effects.en.md).
- Connected [new enemy and boss attack sounds](New_Content_Sounds.en.md) to existing recordings.
- Added [battle escape and power saving buttons](Battle_Escape.en.md).
- Set the [new-hero Hunt Edict](Hunt_Edict_Overview.en.md) to Balanced. Existing saved heroes keep their settings.

The training-ground UI wiring remains unfinished, and the user stopped the Web build. These changes and unfinished additional model tooling were preserved in full checkout and Git-history backups, without adding them to the game.

## Integration validation

- **157/157 focused Unity 6000.6.0f1 Edit Mode tests** passed. [Fixture results](CompletedIntegrationEvidence/editmode-summary.txt)
- The shared UI contract and its **9/9 tests**, and **25/25 world-art tests**, passed. All 119 installed world resources passed missing-file, hash and budget checks. The art validator now distinguishes a dynamically concatenated path prefix from a complete filename, with a regression test.
- The macOS development build completed without errors. [Build result](CompletedIntegrationEvidence/build-summary.txt)
- Battle controls passed **24 combinations** of portrait, landscape, PC 16:9/16:10/21:9, Korean/English and text sizes. [Battle controls](CompletedIntegrationEvidence/battle-escape.txt)
- The world, enemy and skill-effect gallery passed **59 scenes**. [Gallery](CompletedIntegrationEvidence/art-gallery.txt)
- The Hunt Edict window passed **20 layout combinations**, pointer actions, defaults, editing and saving, followed by reloading in a separate process. [Window checks](CompletedIntegrationEvidence/edict-overview.txt) · [Reload check](CompletedIntegrationEvidence/edict-reload.txt)

The first Hunt Edict run stopped at its resolution assertion because macOS opened a window at twice the requested size. A fresh process with explicit startup dimensions passed all layout and persistence checks. The unsuccessful attempt was not counted as a pass.

The full Edit Mode suite was not rerun for this integration. Earlier full-suite evidence records 47 existing failures, so these results do not imply an entirely passing full suite. Input acceptance used synthetic macOS pointers; physical-mobile testing is not included.
