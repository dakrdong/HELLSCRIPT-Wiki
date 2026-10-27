# User report integration: actual review corrections (2026-10-24)

[Korean](User_Report_Review_Corrections_20261024.md)

Actual local Codex CLI0.164.0-alpha.11.1 `review --base` reviewed candidate `a58646ff97b0951cfe187c4f5a07408976154b13` against pinned main `1f1887d1148d99e5b5f42f9a899d8945c6209259`, using gpt-6.1-sol/max from16:56:32 to17:08:19UTC. Exit0 did not mean approval: twoP2 findings and oneP3 were read and corrected.

| Finding | Correction and preservation | Direct validation |
| --- | --- | --- |
| A later parent save restores stale slots | Emit existing Committed only after a changed slot layout saves successfully, updating the parent's saved/draft baseline. Failure/unchanged close emits nothing. | Before:2 failures/1pass in new regression cases. After:36/36 slot-window/store-parent/rift-caller cases, including later global save and disk reload. |
| Focus/pause interrupts a graph drag without layout completion | Route both callbacks through existing Drop; settle a moved active pointer once. Keep owner-disable/resize Finish behavior. | Real overlap resolution, displayed position, preserved saved anchor and no duplicate callback; final DpsHudTests13/13. |
| Unsupported preserved auto preference breaks read-only queries | Read unequipped opaque values without boolean interpretation; preserve strict Automatic validation for equipping. | Two ASCII/Unicode/trailing-space inputs cover Value/SavedValue/Dirty/Summary, exactUTF-8 retention and rejected edits/saves/re-equipping. |

The six combined directly affected fixtures passed80/81. The remaining case assumed automatic OnDisable dispatch in EditMode; the fixture now explicitly invokes the actual callback. No runtime source changed for that retest. Final DpsHudTests13/13 plus68 unchanged passing cases establish81/81 focused unit coverage, zero failures/skips. Evidence: local `Builds/Validation/UserReports20261024/takeover/review-fix-unit-proof.json` and its two XMLs. All9 local CI steps passed on the corrected candidate (`review-fix-ci/report.json`). InstalledPython3.9.6/Node26.8.2 differ from workflow3.12/22; exact-version parity is unverified.

Earlier550-unit/C-D/A-G-E-I evidence is reused only for unchanged inputs. Rendered global Canvas raycasts, native pointers and Korean/English game captures remain pending; EditMode filter/event units and HTML prototype screenshots do not establish them. Nine own validation-identity/import files were hash-preserved before restoring HEAD. All8 protected root-checkout files still matched their starting hashes.

At writing, actual CLI re-review of the added delta is the next step. The final head and completed result are recorded separately in local orchestration/CODEX_CHECK_20261024.json, without changing the reviewed head just to record completion. Existing J-import and GitHub-transfer denials have not been retried or bypassed. Preserve main integration/cleanup, then optimization, finalWeb/APK, then video.

## 2026-10-25 Rune derived-preview source-pin renewal

The approved immediate equipment-display refresh changed the complete RuneBoardWindow.cs file hash. Its delta concerns the observed equipped weapon, the StoreViewBinding visible-state key and RefreshCommitted; geometry rendering, palette, coordinates, rotations and textures remain unchanged.

The existing [derived-preview generator](../../tools/wiki_rune_shape_previews.py) ran against candidate 4760e4fe6fc6aca55f1948613b67cb673f1ab1f7 in a separate empty output directory, completing in 12.770 seconds with exit 0. All 34 PNG files were byte-identical, and all 34 record metadata entries were unchanged. Only that file's SHA256 and byte count among 17 source pins changed in the [input manifest](../../Wiki/assets/rune-shapes-20261016/manifest.json). The other 16 pins, original artwork, preview files and validation rules remain unchanged. These are derived 2D images, not runtime screenshots or approval for new artwork.
