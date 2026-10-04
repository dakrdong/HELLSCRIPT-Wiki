# Reset a tutorial test account in Unity

Updated: 2026-10-04

Korean: [Unity tutorial account reset](Tutorial_Account_Reset.md)

Use **HELLSCRIPT → 테스트 → 튜토리얼 계정 초기화 (Tutorial Reset)** in Unity to reset local character progression for testing the first tutorial and staged Hunt Edict disclosure. This Editor-only tool is excluded from game builds.

## Usage

1. Stop Unity Play Mode and any standalone player using the same save.
2. Open the tool and check the selected **account save** and full path. The default save and saves under `accounts/google-*` and `accounts/guest-*` are listed separately, with the most recently saved first.
3. Click **Back up and reset selected account** and confirm. On the next launch, enter that same account and choose a class to start the mandatory first tutorial. The tool does not launch the game or start Play automatically.

The root is `Application.persistentDataPath`, or the directory passed to Unity through `-hellscriptSavePath <directory>`. Choose the save belonging to the login you will test. This does not remotely reset browser or other-device storage.

## Reset and retention scope

Only `hellscript-local-v1.json`, its `.bak` and its `.tmp` in the selected profile are moved into `tutorial-account-backups/<UTC-time>-<unique-id>/` under the device root. Moving the fallback backup prevents previous progress from being recovered automatically. The existing `GameStore` new-account and skill-initialization paths create fresh characters, currency, rift progress, tutorial state and disclosure state. Existing high-level characters and equipment are not combined with rewound tutorial flags.

Other profiles, account ownership, guest authentication, device language/display/audio preferences, original combat archives and credential files remain. Each backup contains the moved files and a `backup.json` with their original directory, time, sizes and SHA-256 hashes. Original account saves are private and are never published to the wiki. Keep backups until you decide recovery is no longer required; there is no automatic deletion.

Play, compilation and imports block execution. Invalid profile paths, symbolic links and directories occupying save-file paths are rejected. Failure to write the backup record leaves originals in place; failure during movement rolls moved files back. Check the shown paths and backup, resolve other-player access or filesystem permissions, then retry.

## Restore previous progress

Stop Play and standalone players, then use **Open all backups** or **Open this backup**. The manifest's `profileDirectory` is the original location. If current test progress is needed, back it up with the tool first, then copy the desired backup's save files to the original location. The next launch reads those characters and progress. Backup files are not consumed or overwritten, and repeated resets create separate folders.

## Implementation and verification

The owner is `Assets/HELLSCRIPT/Editor/TutorialAccountReset.cs`. It preserves saves at the file boundary without changing runtime persistence, tutorial eligibility or sign-in. Standard Editor menus, selections and confirmation dialogs require no new in-game content window or shared-UI exemption. Guidance displays Korean and English together.

`TutorialAccountResetTests` checks real save reset followed by level-one heroes for all three classes, mandatory tutorial v3 and locked later features. It also checks exact recovery bytes, unaffected profiles/ownership/device settings/authentication, restoration, invalid paths and backup failure. Temporary fixtures replace real user accounts. Results are recorded for this task; this does not complete the previous tutorial's outstanding native acceptance.

On 2026-10-04, two of three focused EditMode tests passed initially in Unity 6000.6.0f1. The remaining test incorrectly checked the new-account marker after skill initialization consumed it; its assertion timing was corrected and only that test was rerun, passing. All three classes' v3 eligibility and restoration were verified. The actual macOS Editor showed the confirmation, reset-complete state and backup buttons. SHA-256 equality of both moved saves and preservation of the device setting were checked. No real user account was reset. The Cancel button was not exercised through native input. No full game suite, new player build or previous tutorial acceptance was repeated.

Shared UI ownership, eleven contract regressions and the refresh check passed. The validation Editor uses `Artifacts/Validation/TutorialAccountReset20261004/fixture` as its save root. Keep that input and its backups while testing continues in this Editor; they are not removed automatically. Reopening Unity through the usual launch restores the default device save root.
