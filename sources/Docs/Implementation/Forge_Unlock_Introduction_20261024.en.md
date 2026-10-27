# Forge unlock introductions · 2026-10-24

The first forge visit after a service becomes usable shows its unlock effect and an optional introduction. Previously usable services and the first enhancement already taught by the town errand remain quiet.

## Behavior and persistence

The current four services are reroll (Rift35/tab0), slot enhancement (Rift5/tab1), equipment enhancement (Rift1/tab2), and core crafting (Rift50/tab3). No nonexistent fifth tab is invented.

Only unintroduced services that satisfy the actual `ForgeHas` eligibility enter the ordered forge queue. Town rune introductions use their own location filter, so a waiting forge service cannot block them.

The window selects the service and reuses U-J's shared seal/light effect and `flow.skill_unlock` sound. Next becomes available after about two seconds; Skip remains usable immediately. Reduced motion uses 0.15 seconds. Three shared `PrologueGate`/`TutorialAnchorRing` pages show the purpose, a visible primary control, and a caution. Core crafting initially highlights the existing `core-refine` control before a recipe is selected.

Underlying forge input stays blocked. Closing, leaving town, changing the owning window/account, or otherwise interrupting the introduction does not acknowledge it; the next visit starts it again. Final acknowledgement or Skip atomically saves the existing introduction record and read/deferred tutorial state through `GameStore.Transact`. A failed save retains the gate and allows retry. It never performs a purchase, crafting action, reward claim, or practice completion.

The one-time `forgeIntroSeeded` migration quietly acknowledges services already usable by existing players. It removes only premature acknowledgements written for future locked forge services by the old GrantAll path. Completed town enhancement or an actual practiced enhancement is evidence of learning; receiving the subsidy alone is not. Existing save keys and unrelated progress are preserved.

## Localized copy

The opening is: “{0} is now open. Let me break the seal and show you around. You can skip this introduction at any time.” The purpose page reuses the current unlock guide and tutorial definition.

- Enhancement: Check the chosen equipment and cost before enhancing. Nothing changes until you press the action button.
- Slot enhancement: This upgrade applies to the selected equipment slot. It takes crafting time after you start, so collect the result when it finishes.
- Reroll: Choose one affix to change. Check the locked line and cost, and remember that reroll results are random.
- Core crafting: You need ten cores for the equipment's slot. Check the recipe and final cost before crafting.

All new text has Korean and English entries. The shared gate and target refresh after language or viewport changes.

## Validation

The scoped `j-k-i-validation/run2/results.xml` contains330 passed of332, with two fixture expectation failures. The expectations were corrected to inspect the actual location queue and highlighted region rather than the narration target. A new visible-core-button regression brought the focused rerun to19 cases; all19 passed. Reusing314 unaffected cases gives333 unique passed cases. The initial sandbox UPM socket failure occurred before tests and is retained without counting as success. All8 local data/UI CI steps passed using Python3.12 and Node22. The Git clean gate and native UI acceptance remain separate.

Coverage includes eligibility/order/migration/learning, repeat visits, save failure/retry and blocked underlying controls. Headless Editor unit evidence is not native macOS pointer, multilingual viewport, device-performance or final-build evidence. No full EditMode suite was run.

한국어: [구현 기록](Forge_Unlock_Introduction_20261024.md).

## Additional actual-review corrections

The Sol/Max review found unrelated prologue cleanup hiding the active forge tour every frame, and stale rune-show state surviving a profile switch until after the first dock render. The tour now retains ownership during cleanup; profile reset and pre-dock account synchronization clear the old rune presentation. Two regressions cover ordinary frame ordering with stable scroll/focus, and profile reset without changing saved account progress. All11 forge UI cases, including both regressions, passed and the new development acceptance driver compiled. Native acceptance, final combined validation and re-review remain pending.

The explicit development-only `-hellscriptForgeIntroductionProof` accepts only its disposable /tmp save, checks first R5 forge entry and Korean/English rendered viewport/raycast behavior, and waits for actual macOS pointer input for final acknowledgement. It is excluded from release builds.
