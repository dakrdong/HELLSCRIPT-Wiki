# Operations reward validation

Updated: 2026-09-26 · [한국어](Operations_Rewards_Validation.md)

Validated [prepared operations rewards](Operations_Rewards.en.md) on `codex/operations-reward-library`, based on `24694040`, using Unity 6000.6.0f1. The [verification record](OperationsRewardEvidence/verification.json) includes environment, scope and source hashes.

## Results

| Check | Result | Evidence |
| --- | --- | --- |
| Final operations Edit Mode | 12 passed, zero failed/skipped | [XML](OperationsRewardEvidence/operations-final.xml) |
| Expanded related Edit Mode | 275 passed, 4 failed, zero skipped out of 279 | [XML](OperationsRewardEvidence/editmode.xml) |
| Unmodified main save tests | 10 passed, the same 4 failed out of 14 | [Baseline XML](OperationsRewardEvidence/baseline-save.xml), [comparison](OperationsRewardEvidence/baseline-comparison.json) |
| macOS Development build | Succeeded, zero build errors | [Record](OperationsRewardEvidence/verification.json) |
| macOS runtime | 23 checks passed, zero exceptions | [Runtime](OperationsRewardEvidence/runtime.txt) |
| Separate-process restart | Gear/elixirs retained; no duplicate delivery | [Restart](OperationsRewardEvidence/restart.txt) |
| Shared UI contract | Contract passed; 9 Python tests passed | `check_ui_contract.py`, `test_ui_contract.py` |
| Operations tooling | 3 Python tests passed | `test_operations_rewards.py` |
| Catalog and art | Original 96 definitions/PNG bytes and first-clear schedule preserved; 148 RGBA sprites validated | [Comparison](OperationsRewardEvidence/catalog-art.json) |

Expanded coverage includes attendance, core domain, current-build persistence, jeweler, live operations, localization, operations rewards, potions, reward boxes, shared UI and tutorial migration. The final operations run adds opening before potion activation, preserving both starter supplies and the granted quantity. Counts from separate runs are not summed into a larger clean-suite claim.

Two initial new-test failures were fixture mistakes: normalization discards an empty `RunState`, and equipped gear does not consume bag slots. Valid suspended combat and insufficient-capacity fixtures then passed.

The remaining failures are `CurrentBuildSaveTests.RiftFailurePreservesInFlightStateThenDiskMatchesTheExactSuccessfulTransition` cases `(0,1)`, `(1,5)`, `(12,1)` and `(12,15)`. Combat-journal `sequence` differs from 0 to 1. A separate untouched checkout of `24694040` reproduced identical names and messages. This is not a clean full-regression result.

## Native interaction and layout

The smoke grants maintenance supplies, opens the inventory footer, selects potions and credits ten HP potions, then opens currency into the existing wallet for 100 coins. A legendary choice box stays disabled until a slot is selected; choosing weapon grants a legal main hand and opens the shared reward detail. Opening a selected elixir credits five doses to the opening hero. Reserved grants, duplicate delivery, disk reload and a separate application restart are checked.

- Twenty combinations: 440×956, 956×440, 1600×900, 1600×1000 and 2100×900; Korean/English; 100%/150% text scale.
- Checks cover button bounds, fixed navigation/actions, text height and loaded sprites. Three captures were visually inspected.
- [Slot choice](OperationsRewardEvidence/equipment-choice.png), [portrait Korean](OperationsRewardEvidence/operations-440-ko-100.png), [landscape English 150%](OperationsRewardEvidence/operations-956-en-150.png).

Input uses synthetic Unity EventSystem pointers in the macOS Player, verified against actual raycasts and committed before/after state. This does not establish physical mouse or mobile-touch coverage. Isolated guest saves receive a QA tutorial exemption and clear record; the run is not evidence of a natural tutorial or stage-750 playthrough.

## Editor and rollout boundary

Coplay MCP first verified the original project path/readiness and reported zero compile errors. Its connection failed after domain reload, so final evidence comes from batch tests and a Development Player in a separate checkout. No MCP packages or transport settings were changed. The 119 changed source/asset files in the build checkout matched the working branch byte-for-byte. Validation-only ProjectSettings normalization is excluded from the commit.

Wiki build/check, ten Python wiki tests and Node UI checks passed. A logged-out Codex In-app Browser inspected the locally served public mirror: 52 operational records, 30 reserved filter results, the Event Seal detail and no write controls. **This is local preview verification, not a public deployment.**

The implementation is on the work branch. Main integration, public wiki publication, campaign activation and player dispatch are separate steps. The [public wiki](https://dakrdong.github.io/HELLSCRIPT-Wiki/#/tree) must be rebuilt, checked and deployed from merged `main`, followed by live document/DB verification. Thirty reserved concepts remain non-payable. Long-term economic tuning and physical-mobile validation are outside this evidence.
