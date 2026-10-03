# Hunt Edict safe-position evasion

Updated: 2026-10-04 · [한국어](Hunt_Edict_Safe_Dodge.md)

After evading an attack with a visible ground boundary, the hero moves to the nearest reachable safe position and holds it until that attack ends. Leaving the footprint no longer immediately restarts pursuit, approach or orbit movement.

Candidates sit just outside the actual circle, ring, line or sector hit boundary. Existing visibility, walls, landing, enemy occupancy, path, risk and arrival checks remain; safe candidates are ordered by actual path length. This is movement policy, not invulnerability or permission to ignore unreachable positions and attacks that cannot be escaped in time.

- Hold through the originating preparation, attack, projectiles and lingering damage zones; release when they expire or are cancelled.
- Attack eligible enemies within range and sight from the same position. A distant current target does not prevent attacking another reachable enemy. Pursuit, orbit and movement skills cannot leave the safe position.
- Evade again if a new attack threatens the held position. Global emergency survival still operates.
- Count movement and stationary waiting as **Evasion** in the existing activity graphs. Actual attacking intervals count as **Attack**, without double counting.

`CombatSimulation.EdictMovement` selects the destination. `EdictResponseState` stores the originating action and zone IDs, and `CombatSimulation.EdictResponse` checks their real lifetimes. Existing movement, target, aim and class-skill owners honor the hold. No fixed waiting timer or preview-only combat policy is added.

The existing suspended-run snapshot persists this state. Older saves default to empty lists; reopening the same run preserves the hold. Changing or disabling the Edict releases it and reads the new policy. The existing `CombatFeedback`, HUD and result graphs own activity totals.

All 56 focused checks passed: four boundaries, preparation-to-zone continuity, save restoration, cancellation, stationary attacks on another reachable enemy, a new warning and cumulative evasion time.

The final related Edit Mode run against the merged combat code had 1,289 checks: 1,279 passed, 10 pre-existing failures and zero new failures. Failure names and assertions were compared with the prior integrated result. This is not a new full-project test run.

The native macOS development player used the actual Rift, class skills, world and activity HUD to verify arrival at the safe position, two seconds of stationary evasion time, W14 and basic attacks on another in-range enemy instead of the distant current target, the W11 movement restriction and release after the zone expires. This development-only fixture uses fixed combat ticks and an isolated save; it is not physical pointer input, physical-mobile, balance or performance validation. Only native acceptance was repeated after correcting its spawn and capture refresh; unchanged production code reused the related Edit Mode result.

The shared UI ownership check and all 11 contract-tool checks passed. UI layout is unchanged and no text-size matrix was run. Evidence: [validation record](SafeDodgeEvidence20261003/validation.json), [stationary wait](SafeDodgeEvidence20261003/safe-waiting-1600x900-ko.png), [stationary attack](SafeDodgeEvidence20261003/safe-attack-1600x900-en.png).
