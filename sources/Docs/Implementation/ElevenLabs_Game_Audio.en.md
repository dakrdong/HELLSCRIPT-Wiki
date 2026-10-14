# ElevenLabs game audio

Updated: 2026-10-14 · Written: 2026-09-27 · [한국어](ElevenLabs_Game_Audio.md)

On 2026-09-27 the user accepted the then-adopted audio as the final version (454 sound effects and four music tracks: 458 files). On 2026-10-14 the art direction changed to bright casual fantasy, so **the four music tracks were replaced by six**. The current set is **456 sound effects (including the two title-flight effects merged the same day) and six music tracks: 462 files**. It covers all SFX identifiers and six music contexts. Nine SFX actions share suitable existing final clips. No additional SFX generation is planned. The 458-file and four-track statements and the evidence below describe the 2026-09-27 state.

Final acceptance of the SFX is based on the user's instruction of 2026-09-27. It does not claim a new human listening test or physical mobile validation.

## 2026-10-14 music replacement (bright casual art direction)

The art direction moved from dark gothic to bright, light medieval fantasy (screens, combat and saves unchanged; only the art changed), so the music was recreated. Previously character creation/selection shared the title track and the training ground shared the town track. Now each of the six music contexts has its own track. The tracks were generated once each with ElevenLabs `eleven_music_v2_5` (about 14,400 credits). Prompts, generation IDs and hashes are in the [source manifest](../../AudioSources/ElevenLabs/manifest.json). The generation flow ID is `aKtfRMrdNm1ZoHFt8Y80`.

| Context | Plays during | Direction | Source → loop master | Integrated loudness |
| --- | --- | --- | --- | --- |
| `title` | Title and sign-in | Bright heroic orchestral overture, D major, horn and trumpet fanfare | 150.0 s → 133.4 s | -20 LUFS |
| `characters` | Character creation and selection | Anticipatory pizzicato, celesta and flute, G major | 150.0 s → 136.9 s | -21 LUFS |
| `sanctuary` | Town and result screens | Sunny village folk (guitar, mandolin, tin whistle, accordion), F major | 180.0 s → 143.5 s | -22 LUFS |
| `training` | Training lobby and every training fight (boss tests and results included) | Light, brisk drill march (pizzicato, woodblock, bassoon), C major | 150.0 s → 134.0 s | -23 LUFS |
| `rift` | Rift exploration and combat | Brave adventure battle music (taiko, horn hook), E minor | 180.0 s → 142.3 s | -23 LUFS |
| `boss` | Rift boss | Tense adventure boss fight (low brass riff, trumpet), C minor | 150.0 s → 121.6 s | -21 LUFS |

In game code, `GameAudio.Clips.cs` picks among the six contexts. The character creation/selection screen (`characters`) and the training ground use their own tracks. The training ground keeps one track from the moment the lobby opens through fights, boss tests and results, so choosing a boss there never switches to the rift boss track. Rift and boss keep the existing `RunPhase` rules, and context changes use a 1.5-second fade.

### Loop editing

Generated tracks either end with a fade-out (title, town and rift lose level over roughly their last three seconds) or have a beat grid that does not line up with the start. The old method (blend the last 2.5 seconds of the full-length source into the first 2.5 seconds) would make every loop sag in level or stumble in rhythm. The real tempo also differs from the prompted BPM (for example boss 150 → about 157 BPM), so a fixed bar length cannot be cut.

`tools/elevenlabs_audio.py --loop-end <source.mp3>` finds the end. Starting at 75% of the source length, it picks the point where the last 2.5 seconds best match the first 2.5 seconds in timbre (50 ms log-band frames) and rhythm (10 ms onset envelope), then aligns to the sample on the low-passed waveform. The result is stored as `selection.end_seconds` in the source manifest and `--rebuild` reproduces it. The previous four tracks had no such value and used the full source length.

The [measurements](ElevenLabsAudioEvidence/music-casual-master-audit-20261014.json) follow. They are signal measurements, not listening.

- Integrated loudness is within ±0.02 LUFS of target for all six, and true peak is -9.7 to -6.2 dBTP, below the -2.5 dBTP ceiling.
- The sample step across the loop boundary is at most 10% of each file's own largest step (continuous).
- The beat offset between the blended last and first 2.5 seconds improved from -310…+500 ms before choosing the end to 0…±10 ms after (town -310→-10, training +500→0, rift +80→0, boss +90→+10, title -20→-10). Characters was already aligned at full length, so it stays at the same level (0→+10 ms).
- The deepest level dip near the loop boundary improved from -19.6 to -6.8 dB (title), -22.9 to -12.5 dB (town) and -34.2 to -13.9 dB (rift). The remaining -12.5 dB in town is the track's own quiet introduction. Training is staccato, so it drops to about -27 dB between hits everywhere, independent of the boundary.

### Validation status

- User approval: on 2026-10-14 the user reviewed the tracks, said they are fine and told us to proceed. There is no scoring sheet or separate listening record, and `listening_review` in the source manifest stays `pending`.
- Passed: eight offline tool tests (including loop-end finding), the provenance check of the six new music tracks (model, original and master hashes, paths; zero problems) with byte-for-byte reproduction from the originals, and on the merge candidate the full Unity Edit Mode run (5,752 tests: 5,748 passed, 3 skipped authoring probes and 1 failed, `LocalizationTests.NoEntryIsLeftBehindByARewordedLine`, an `en.txt` entry with no source line that `main` commit f726d865 left behind; unrelated to this change; about 90 minutes). The focused `SoundBankTests` run passed 23 tests and confirmed the import of the six new WAVs ([result](ElevenLabsAudioEvidence/music-casual-soundbank-editmode-20261014.json)).
- The audio smoke on the macOS development player (zero build errors) passed: in five contexts (title, town, training with the lobby open, rift and boss) the real stream advances, wraps and transitions, and volume and mute settings survive a restart. In the development player the six tracks total 31.97 MiB (3.3% of the 964 MiB build). There is no measurement of the previous four tracks, so the change is only estimated from length (650 s → 812 s).
- Smoke fix: the app-pause check read the position before the audio thread applied the pause and failed 2 of 8 runs (unrelated to music: the position moved one buffer, 0.021 s). It now reads the position 0.1 s after the pause and passed 12 consecutive runs.
- Not done: native playback of the character creation/selection context (reaching that screen needs a sign-in, so only the Edit Mode mapping tests cover it) and physical mobile devices.
- Pre-existing failure: the full `elevenlabs_audio.py --check` and `generate_sfx.py --check` runs fail on a mismatch between the SFX catalog and bank (`skill.W08-3`, `skill_M13_1.wav`, `skill_M15_1.wav` and others). The SFX files match the base `origin/main`, so this is unrelated to this change; the six music entries were checked separately with zero problems.
- Recovery: the previous dark-gothic tracks and originals are recoverable from `a113f4ff` on `main`.

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
| MCP-verified model | `eleven_text_to_sound_v2` | `eleven_music_v2_5` (2026-10-14 replacement; the previous four used `eleven_music_v2`) |
| Generated original | MP3, 44.1 kHz, stereo, 128 kbps | MP3, 48 kHz, stereo, 192 kbps |
| Final master | WAV, 48 kHz, PCM16, mono | WAV, 48 kHz, PCM16, stereo |
| Unity import | Existing ADPCM or Vorbis, compressed in memory | Streaming, Vorbis quality 0.9, stereo, no preload |

The [source manifest](../../AudioSources/ElevenLabs/manifest.json) records model, generation ID, prompt, parameters and source/master hashes. Originals remain outside Unity builds for reproduction and provenance. Converting an MP3 to WAV does not recover discarded information. Cleanup does not re-encode or reduce the quality of any retained master.

SFX processing uses silence trimming, DC removal, short fades, linear gain and a -2 dBTP ceiling. Slider effects contain one 30 ms tick; the three tab variants reach 5% cumulative energy about 5–19 ms after onset. [UI edit evidence](ElevenLabsAudioEvidence/ui-transient-edit.json)

Music lengths are 133.4 seconds for title, 136.9 for characters, 143.5 for town, 134.0 for training, 142.3 for rift and 121.6 for boss. After the end point is chosen, the loop splice spans 2.5 seconds; context changes use a 1.5-second fade. Unselected music pauses. The existing 16 SFX voices, priorities, concurrency controls and final-mix limiter remain. Audio does not mutate saves or combat random streams.

## Maintenance and validation

```sh
python3 tools/elevenlabs_audio.py --check
python3 tools/generate_sfx.py --check --audit Artifacts/audio-bank-audit.json
python3 tools/test_elevenlabs_audio.py
python3 tools/elevenlabs_audio.py --rebuild
python3 tools/elevenlabs_audio.py --loop-end <new-music-source.mp3>
python3 tools/audio_audition.py
```

The full provenance check requires all 462 assets; a partial-completion option is no longer necessary. Retired or unreferenced runtime WAVs and modified originals or masters fail validation. `--rebuild` uses retained sources and skips shared actions, so it creates no duplicate audio. These tools make no API calls and spend no credits.

## Evidence

The following records the 2026-09-27 cleanup state (458 files). Validation of the 2026-10-14 music replacement is listed separately in the section above.

Current cleanup validation is recorded in the [cleanup report](ElevenLabsAudioEvidence/final-cleanup.json) and [Unity result](ElevenLabsAudioEvidence/editmode-final-summary.json). Checks compare all 458 master hashes and metadata with the baseline, retain every playback identifier, and verify that shared actions play the same imported assets.

31 Unity Edit Mode tests, seven offline tool tests and nine shared UI contract tests passed. The [full provenance check](ElevenLabsAudioEvidence/final-source-validation.json) passed for all 458 files with none missing. The macOS development build succeeded with zero errors. Native focused checks passed for all nine shared effects using existing imported assets and listener output, four music transitions/loops, short developer-training runs for three classes, volume/mute and preferences restored after process restart. [Shared effects](ElevenLabsAudioEvidence/native-final-shared-effects.txt) · [Music](ElevenLabsAudioEvidence/native-final-music.txt) · [Controls/classes](ElevenLabsAudioEvidence/native-final-audio.txt) · [Restart](ElevenLabsAudioEvidence/native-final-restart.txt)

The full gameplay smoke recorded 1,074 events across 76 cues with no missing clips or logged errors. However, after an optional inventory interaction failed, the ranger timed out approaching the rift keeper: **the full smoke failed**. Forge interaction was also skipped because its pointer target was blocked. Focused audio success is not substituted for full gameplay success. [Full summary](ElevenLabsAudioEvidence/native-cleanup-full-summary.txt) · [Trace](ElevenLabsAudioEvidence/native-cleanup-full-trace.tsv)

Earlier signal measurements remain applicable to byte-identical masters: all 454 SFX passed with zero clipped samples, maximum absolute DC 0.000260 and approximately -2 dBTP peak. [SFX](ElevenLabsAudioEvidence/runtime-master-audit.json) · [Music, previous four tracks](ElevenLabsAudioEvidence/music-master-audit.json)

Earlier music/settings runtime tests covered the previous four streaming tracks, loop progress, transitions, volume, mute and settings restoration after process restart. [Music](ElevenLabsAudioEvidence/native-audio-music.txt) · [Settings](ElevenLabsAudioEvidence/native-audio-initial.txt) · [Restart](ElevenLabsAudioEvidence/native-audio-restart.txt)

The macOS gameplay smoke uses synthetic EventSystem input and disposable accounts. Covered town controls may use direct callbacks; optional forge interaction is skipped if pointer access is blocked. This is not physical mobile input or performance evidence. The public wiki is published only from merged `main`.

## Addition: title login flight sounds (2026-10-14)

Two new sound effects for the [title login flight](Title_Screen.en.md) were generated with ElevenLabs `eleven_text_to_sound_v2` (MCP, prompt influence 0.65). Four candidates were generated per cue and one was chosen from its loudness envelope.

| Cue | Use | Adopted generation ID | Length |
|---|---|---|---|
| `flow.title_flight` | Wind rush as the flight starts | `PmH0ZtJAEtC86aHk2V7O` | 1.70 s |
| `flow.title_rift` | Rift light burst | `e9C3rrKPb3aFTcuHuWXP` | 1.50 s |

The original MP3s are kept in `AudioSources/ElevenLabs/Sources/` and were mastered with `elevenlabs_audio.py`. The set is now 456 sound effect files plus the four music tracks of that moment (the same day's music replacement above made it six), with 285 SFX identifiers. The eight generations cost 54.7 credits (the estimate said 180). No human listening check was done.

Note: on the same day `main`'s `sfx-catalog.json` and `bank.json` disagree because of the D4 skill identifier migration, so `generate_sfx.py --check` and `elevenlabs_audio.py --check` fail (they already did before this change). The two new bank rows match the generator's format, and `test_elevenlabs_audio.py` passes.
