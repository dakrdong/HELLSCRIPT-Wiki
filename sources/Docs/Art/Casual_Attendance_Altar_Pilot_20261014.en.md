# Casual attendance altar pilot — 2026-10-14

[한국어](Casual_Attendance_Altar_Pilot_20261014.md) · [Validation receipt](Casual_Attendance_Altar_Pilot_20261014.receipt.json)

From baseline `53d6a3e1`, replace only the runtime artwork `Assets/HELLSCRIPT/Resources/Art/Attendance/popup-altar.png` with newly drawn StyleD art. Code, rewards/IDs, saves, importers, and UI coordinates remain unchanged. The 700 approved primary exports did not contain this backdrop. Built-in image_gen produced one new illustration and three focused corrections for flat cel surfaces and actual reward/state/date text contrast. Apply selected PNG bytes without postprocessing.

- Selected SHA-256: `1d8ec19907d2e66acefae53a1b051b304f80328bc1fd5eca8874305776e2d18f`
- Original SHA-256: `05d2b3df863412c16ca55b75dc6f9b4b37c4bd01f891ea569e8456aa409a3f44`
- Preserved GUID: `688eef0212b544a0f9f9f673441bc601`; byte-identical meta SHA-256 `fd891bb4ae492ab2b472262ea9a40b8bd57b41f2afc6fb69d94563dc2b093dc3`.
- Opaque RGB 1254×1254 square; altar upper face near 46% from the top. Actual imported texture: 1024×1024, no mipmaps; unchanged 1024 caps and Standalone BC7 / Android ASTC 6×6 / WebGL ETC2 RGBA8 settings.

[Before](../Implementation/CasualAttendanceAltarPilotEvidence/before/weekly-ko.png) · [After](../Implementation/CasualAttendanceAltarPilotEvidence/after/editor-selected-1893x1637-ko.png) · [Portrait claimable](../Implementation/CasualAttendanceAltarPilotEvidence/native-screens/weekly-ready-440x956.png) · [English landscape monthly](../Implementation/CasualAttendanceAltarPilotEvidence/native-screens/monthly-956x440-en-100.png) · [Portrait claimed](../Implementation/CasualAttendanceAltarPilotEvidence/native-screens/weekly-claimed-440x956-ko-100.png)

## Validation and limits

Final AttendanceArtTests: 7 passed, zero failures/skips. Shared UI contract and its 15 regression tests passed. The art suite ran once for each of two asset inputs before/after contrast corrections; final contract verification reused matching source/environment/log proof.

Exactly one macOS development build succeeded in 34.661 seconds with zero build errors and exit 0. The initial smoke saved 25 captures, then exited 1 with `Collect button obscured: Weekly day 4` (empty raycast hits; Geometry line127, Start line234). Keep this failure as a failure. The same binary and isolated saved state then passed the same collect-button assertions and the remaining cases through the existing CollectOnly entry point, exit 0. Fresh-process Resume also exited 0. **A clean complete single initial-to-final run is not claimed.** Root cause of the first failure is unconfirmed; repaint/input timing is an inference and remains a separate harness follow-up risk before expansion.

Continuation verified 78 default-text viewport/language/safe-area page cases, 1,066 collect-button bounds/raycasts, 1,596 eligibility/seal/fold checks, and 104 raycast-verified clicks. Required 440×956, 956×440, 1440×810, 1440×900, 1680×720 and the existing extra 640×360 cases covered KO/EN. Preserve 69 native captures. Across the initial and continuation phases, verify individual/bulk claims, failed save and retry, UI refresh after saving, swipe/hide/reopen and battle pause handling. Resume restored all 28 monthly claims, 990 coins, 110,000 gold, 110 gems, 55 cores, five unopened chests and one opened reward item. The weekly track correctly rolled to day1 of its later period by the final fixture date.

All players used `-noaudio` and `/Users/t8g-2410-pn-005/Documents/Codex/2026-10-14/task-5/evidence/native-save`; no actual user save was used. The development-console message in the save-fault capture records the deliberately blocked temporary save path. Physical mobile devices, the full 5715 suite, web builds and public deployments were outside this pilot. Preserve the final integration owner's existing full-suite/smoke completion requirement.

## Retention and next boundary

Other-owner transition Markdown/English/index files retain their exact hashes and are excluded from this commit. Restore only the three automatic build settings changes from verified pre-run bytes and preserve their generated state as evidence. Recover original art with `git show 53d6a3e1:Assets/HELLSCRIPT/Resources/Art/Attendance/popup-altar.png` or `/Users/t8g-2410-pn-005/Documents/Codex/2026-10-14/task-5/evidence/original/popup-altar.png`. No permanent deletion of originals, meta, candidates or failure proof.

Next candidate: reuse approved random-core art for **one representative native-alpha Sprite**, `reward-random-core.png`. Validate its alpha edge, small-size readability, green claim glow, GUID and 512 cap before a small same-type batch including the approved legendary-chest sprite. The surrounding UI and rewards still use their existing theme. Shared UiTheme (164 consumers), fixed crops/atlases and 3D rigs have separate impact boundaries. Await parent coordination before expanding.

Prompts, selected source, result JSON, logs, screenshots and artifact lifecycle records live in `/Users/t8g-2410-pn-005/Documents/Codex/2026-10-14/task-5`. Artifact lifecycle records the single player's creation path/hash and recoverable closeout archive path.
