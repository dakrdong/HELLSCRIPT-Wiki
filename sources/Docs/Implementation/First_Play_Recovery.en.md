# First play: inventory return and checkpoint recovery

2026-09-27 · D01 implemented; original blocked checkpoints recovered

The inventory used to reopen after cleanup because `GameUI.PlayInventory` discarded the portal context. Closing the inventory therefore never resumed the suspended battle.

## Behavior

- Portal inventory offers Resume Rift, Storage, and Gem Storage. Looting also permits finishing without remaining loot. Leaving rewards or returning to town requires confirmation that explains the outcome.
- Close and Escape use the same eligibility check. Equipment or pending-gem capacity errors retain the cleanup actions and explanation.
- `PortalRecovery` owns eligibility; `GameStore.ResumePortal` and `ReturnFromPortal` commit before adopting the result. Run identity, RNG, elapsed combat time, and earned rewards are retained.
- The common window pause lease accepts the committed resume state and restores it only after the last nested window closes. Ordinary windows preserve their previous pause state.
- A failed resume or return save enters the existing foreground safety pause. Fatigue does not continue draining before a successful save and explicit resume. The ordinary pause/inventory fatigue policy is unchanged.

Landscape recovery actions sit below the bag column, preserving the equipment, potion, and stat area. The existing inventory adapter and shared equipment components remain the owners.

## Evidence

Baseline: `246940407ea4ac3914285f4d59b27f2b91ea562c`. Original play saves were preserved. Copies were loaded into the native macOS Development Player at normal 1× speed. A uGUI raycast checked button reachability before synthetic pointer events.

| Case | Original blocker | Recovery |
|---|---|---|
| Warrior Lv.12, Rift 6 | Inventory loop after boss defeat | Original run settled at 162.9053 seconds, exactly one completed record. |
| Mage Lv.11, Rift 7 | Inventory loop during combat | Resumed at 105.1018 seconds and cleared at 172.5059 seconds, exactly one completed record. |

[Warrior runtime result](FirstPlayRecoveryEvidence/warrior.txt) · [Mage runtime result](FirstPlayRecoveryEvidence/mage.txt)

No levels, currency, capacity, or simulation time were injected. `RuntimePortalRecoverySmoke` requires explicit copied-save and evidence directories. This is synthetic native uGUI input, not physical mouse or mobile-device evidence.

Ten `PortalRecoveryTests` cover pre/post-boss resume and return, capacity refusal, limited loot, save failure/retry, duplicate completion, and nested pause restoration. The subsequent D01/D02 group passed all 18 tests. Shared UI ownership and its nine checker tests passed. The complete display matrix and three-class Rift 20 playthrough belong to the later D10 acceptance record.

## Ownership

- `Runtime/Core/PortalRecovery.cs`: eligibility and committed transactions.
- `Runtime/Presentation/GameController.cs`: post-save battle/town transitions and safety pause.
- `Runtime/Presentation/InventoryWindow.Portal.cs`, `InventoryWindow.cs`, `GameUI.PlayInventory.cs`: context, confirmation, and input paths.
- `Runtime/Presentation/ContentWindowHost.cs`: nested pause restoration.

This recovery change does not raise the account schema. Existing unfinished saves resume their existing runtime state. The subsequent merge with other development is recorded in the [2026-09-27 integration](Main_Integration_20260927.en.md). Physical-mobile validation remains separate.

The [save integrity comparison](FirstPlayRecoveryEvidence/integrity.json) confirms preserved item and record identities, every pending item collected once, and one completed record per recovered run. Warrior gold changed by −125 for automatic potion replenishment. Mage gold changed by 3,115: 3,245 newly earned minus 130 for replenishment.
