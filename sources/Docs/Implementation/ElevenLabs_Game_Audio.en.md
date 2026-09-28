# ElevenLabs game audio

Written: 2026-09-27 · [한국어](ElevenLabs_Game_Audio.md)

The user accepted the currently adopted audio as the final version. The retained set contains **454 sound effects and four music tracks: 458 files**. It covers all 283 SFX identifiers and four music contexts. Nine SFX actions share suitable existing final clips. No additional generation is planned.

Final acceptance is based on the user's instruction. It does not claim a new human listening test or physical mobile validation. These changes are on the work branch; merging to `main` and publishing the wiki are separate steps.

## Final catalog and shared clips

274 SFX identifiers have dedicated source generations. The following nine actions reference the same imported `AudioClip` assets as their source cues. They do not copy files or count the same generation as another variation. Action identifiers, cooldowns, priorities and concurrency limits remain intact.

| Action | Final source cue | Reason |
| --- | --- | --- |
| `reward.claim` | `ui.confirm` | Successful reward confirmation |
| `shop.gamble_beat` | `ui.click_heavy` | Short, weighty reveal beat |
| `gem.socket` | `item.equip.orb` | A crystal seating in a metal mount |
| `reward.box_open` | `reward.chest` | A treasure chest with a latch and wooden hinge |
| `rune.fuse` | `gem.fuse` | Crystal tones converging into one |
| `rune.place` | `forge.core_invest` | A stone core inserted into a slot |
| `rune.remove` | `gem.remove` | A release click from a mounted material |
| `rune.rotate` | `loot.stone` | A short mineral contact |
| `rune.unlock` | `aspect.upgrade` | A rising arcane-seal resonance |

The [final catalog](../../AudioSources/ElevenLabs/sfx-catalog.json) owns mastering targets and shared-cue relationships. It produces the [runtime bank](../../Assets/HELLSCRIPT/Resources/Audio/Sfx/bank.json). The [production list](../../AudioSources/ElevenLabs/production-plan.json) distinguishes dedicated and shared audio; the previous generation-limit wait is closed.

## Removal and storage

- Removed all 15 remaining procedural WAVs and their `.meta` files after checking for external Unity GUID references. The bytes, GUIDs and import settings of all 458 final assets are preserved.
- Removed downloaded copies of all 1,087 generation candidates, old preview conversions, duplicated preview masters, previous test builds and temporary test saves. Exactly one retained original per selected generation remains under `AudioSources/ElevenLabs/Sources`.
- Removed retired synthesis recipes and synthesis-only code. The historical `generate_sfx.py` command now validates final files, writes bank metadata and produces previews. Running it without an operation fails instead of synthesizing audio.
- The audition page uses a symbolic link to the actual game masters. Rebuilding it does not duplicate WAV or MP3 files.
- Git history is unchanged. Removed tracked files can be recovered from pre-cleanup commit `949b31af520249e3c7708c7ef96256099f293cf6`. Other worktrees and the general Unity cache are outside this cleanup.

Retired runtime WAVs occupied 1,504,326 bytes. Final runtime WAVs total 176,891,768 bytes. Task production and validation artifacts decreased from approximately 3.00 GB to 13.7 MB, **saving about 2.98 GB**. The new validation build and temporary accounts were also removed. [Cleanup evidence](ElevenLabsAudioEvidence/final-cleanup.json) records the measurements. Source-storage savings are not measurements of mobile build size or runtime memory savings.

## Preserved provenance and audio quality

| Item | SFX | Music |
| --- | --- | --- |
| MCP-verified model | `eleven_text_to_sound_v2` | `eleven_music_v2` |
| Generated original | MP3, 44.1 kHz, stereo, 128 kbps | MP3, 48 kHz, stereo, 192 kbps |
| Final master | WAV, 48 kHz, PCM16, mono | WAV, 48 kHz, PCM16, stereo |
| Unity import | Existing ADPCM or Vorbis, compressed in memory | Streaming, Vorbis quality 0.9, stereo, no preload |

The [source manifest](../../AudioSources/ElevenLabs/manifest.json) records model, generation ID, prompt, parameters and source/master hashes. Originals remain outside Unity builds for reproduction and provenance. Converting an MP3 to WAV does not recover discarded information. Cleanup does not re-encode or reduce the quality of any retained master.

SFX processing uses silence trimming, DC removal, short fades, linear gain and a -2 dBTP ceiling. Slider effects contain one 30 ms tick; the three tab variants reach 5% cumulative energy about 5–19 ms after onset. [UI edit evidence](ElevenLabsAudioEvidence/ui-transient-edit.json)

Music lengths are 147.5 seconds for title and boss, and 177.5 seconds for sanctuary and rift. The loop splice spans 2.5 seconds; context changes use a 1.5-second fade. Unselected music pauses. The existing 16 SFX voices, priorities, concurrency controls and final-mix limiter remain. Audio does not mutate saves or combat random streams.

## Maintenance and validation

```sh
python3 tools/elevenlabs_audio.py --check
python3 tools/generate_sfx.py --check --audit Artifacts/audio-bank-audit.json
python3 tools/test_elevenlabs_audio.py
python3 tools/elevenlabs_audio.py --rebuild
python3 tools/audio_audition.py
```

The full provenance check requires all 458 assets; a partial-completion option is no longer necessary. Retired or unreferenced runtime WAVs and modified originals or masters fail validation. `--rebuild` uses retained sources and skips shared actions, so it creates no duplicate audio. These tools make no API calls and spend no credits.

## Evidence

Current cleanup validation is recorded in the [cleanup report](ElevenLabsAudioEvidence/final-cleanup.json) and [Unity result](ElevenLabsAudioEvidence/editmode-final-summary.json). Checks compare all 458 master hashes and metadata with the baseline, retain every playback identifier, and verify that shared actions play the same imported assets.

31 Unity Edit Mode tests, seven offline tool tests and nine shared UI contract tests passed. The [full provenance check](ElevenLabsAudioEvidence/final-source-validation.json) passed for all 458 files with none missing. The macOS development build succeeded with zero errors. Native focused checks passed for all nine shared effects using existing imported assets and listener output, four music transitions/loops, short developer-training runs for three classes, volume/mute and preferences restored after process restart. [Shared effects](ElevenLabsAudioEvidence/native-final-shared-effects.txt) · [Music](ElevenLabsAudioEvidence/native-final-music.txt) · [Controls/classes](ElevenLabsAudioEvidence/native-final-audio.txt) · [Restart](ElevenLabsAudioEvidence/native-final-restart.txt)

The full gameplay smoke recorded 1,074 events across 76 cues with no missing clips or logged errors. However, after an optional inventory interaction failed, the ranger timed out approaching the rift keeper: **the full smoke failed**. Forge interaction was also skipped because its pointer target was blocked. Focused audio success is not substituted for full gameplay success. [Full summary](ElevenLabsAudioEvidence/native-cleanup-full-summary.txt) · [Trace](ElevenLabsAudioEvidence/native-cleanup-full-trace.tsv)

Earlier signal measurements remain applicable to byte-identical masters: all 454 SFX passed with zero clipped samples, maximum absolute DC 0.000260 and approximately -2 dBTP peak. [SFX](ElevenLabsAudioEvidence/runtime-master-audit.json) · [Music](ElevenLabsAudioEvidence/music-master-audit.json)

Earlier music/settings runtime tests covered four streaming tracks, loop progress, transitions, volume, mute and settings restoration after process restart. [Music](ElevenLabsAudioEvidence/native-audio-music.txt) · [Settings](ElevenLabsAudioEvidence/native-audio-initial.txt) · [Restart](ElevenLabsAudioEvidence/native-audio-restart.txt)

The macOS gameplay smoke uses synthetic EventSystem input and disposable accounts. Covered town controls may use direct callbacks; optional forge interaction is skipped if pointer access is blocked. This is not physical mobile input or performance evidence. The public wiki is published only from merged `main`.
