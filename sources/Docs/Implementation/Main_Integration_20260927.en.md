# Completed development integrated into main — 2026-09-27

Updated: 2026-09-27 · [한국어](Main_Integration_20260927.md)

Integrated first-play improvements, operational rewards and APK size optimization with the sound bank already in main. The newly active ElevenLabs production work is outside this completed-development batch.

## Included changes

| Work | Source commits | Scope |
| --- | --- | --- |
| First-play recovery and improvements | `522ea02a`, `a62a12cb` | D01–D09 implementation and focused validation. Natural Rift 20 progression for all three classes remains in progress. |
| Operational rewards | `a1bc4825` | 148 boxes, 22 operational packages and 30 reserved items. No automatic distribution or server campaign is activated. |
| APK size optimization | `0e8f095b` | Texture imports, unused build resources/packages and a release APK build path. |
| Existing sound bank | `d238fa0e`, main `dc5770b6` | Retains the already merged playback routing for 283 cues. |

The code integration is `138ba4b1`, followed by native receipt/restart coverage in `928dbf28` and count-neutral receipt wording in `d48a08e0`.

## Integration fixes

Preserve the potion category, equipment-slot choice and first-play receipts together. A visible receipt blocks another opening, and required choices remain validated before the transaction. Both sets of English localization additions are retained.

Operational `potion` grants now store their actual potion ID in the receipt and resolve its display name through the existing potion catalog. Existing tests cover all 44 potion definitions, actual stock, receipt identity and persistence. Native UI verified ten HP potions and five Crowned Diamond Elixirs, including reopening their persisted receipts in another process.

## Full regression suite

The full Edit Mode run finished with **4,091 total, 4,044 passed, 47 failed and zero skipped**. All 47 failures exactly match the independently reproduced earlier baseline; zero new failures were introduced by this integration. This is not an all-green result. See the [summary](MainIntegration20260927Evidence/validation.json) and [original XML](MainIntegration20260927Evidence/full-editmode.xml).

The full suite ran at `138ba4b1`. Subsequent changes only extend the native harness and adjust one English receipt sentence. Both final builds and the repeated 20-layout reward/restart acceptance use `d48a08e0`; the [source scope](MainIntegration20260927Evidence/source.json) records this distinction.

## Native and build evidence

- [Native evidence](MainIntegration20260927Evidence/native.json): 20 first-play layout combinations plus a separate-process restart; 20 operational reward combinations plus receipt reopening. Resolutions: 440×956, 956×440, 1600×900, 1600×1000 and 2100×900. Languages: KO/EN. Text scales: 100/150%.
- [Sound run](MainIntegration20260927Evidence/sound.txt): 85 distinct playback events and listener output across title, character selection, tutorial, three classes, services and settings; zero missing clips and zero logged errors. Directly invoked covered service buttons are explicitly identified in the original log.
- [Android release](MainIntegration20260927Evidence/android.json): IL2CPP, ARM64 and LZ4HC; build succeeded with zero errors, 70,585,750 bytes. This integrated APK is distinct from the optimization-only branch's 60,072,342-byte result.

Synthetic macOS input and Android packaging do not establish physical-device touch, thermal/battery performance or human listening approval. Disposable UI fixtures remain separate from the natural-progression account.

## Preservation and resumed progression

Pre-merge branch/worktree inventory and patches are preserved under `Artifacts/Integration20260927/`. The primary workspace received 1,072 previous playtest files with matching hashes. Intermediate validation work is retained as patches, stashes and recoverable archives; active work in other checkouts is preserved.

Natural progression resumes on the same shared guest account at normal 1× speed. Immediately before integration: Warrior level 8/highest clear 5; Ranger level 11/highest clear 5; Mage level 1/highest clear 0, with 14 completed run records. Attempted stages are not counted as clears. No level, currency or time is injected. All-class Rift 20 validation is not yet complete.

[First-play checks](MainIntegration20260927Evidence/firstplay-checks.txt) · [First-play restart](MainIntegration20260927Evidence/firstplay-restart.txt) · [Reward checks](MainIntegration20260927Evidence/operations-checks.txt) · [Reward restart](MainIntegration20260927Evidence/operations-restart.txt)

![Landscape English enlarged result](MainIntegration20260927Evidence/result-956x440-en-150.png)

![Reopened elixir receipt after restart](MainIntegration20260927Evidence/elixir-receipt-restarted.png)
