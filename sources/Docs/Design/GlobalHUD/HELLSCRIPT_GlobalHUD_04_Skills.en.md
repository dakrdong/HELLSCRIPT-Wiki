# Global HUD 04: equipped skills and cooldowns

Date: 2026-09-14

Display one diamond passive and four square actives. Landscape uses the lowest right row at 1.20 times each baseline dimension, above and clear of XP. Portrait keeps its approved arrangement.

The existing game supports up to three passives. Changing equipment to one requires unlock, preset, recommended-build and save-migration decisions. Hiding two active passives in the HUD is not acceptable. Until that policy is resolved, use development fixtures for the new HUD.

| State | Display |
|---|---|
| Ready | Clear artwork; hide cooldown number. Ready does not promise immediate casting. |
| Cooldown | Dark radial fill plus central remaining time. |
| Automatic casting | Brief frame emphasis, no manual input. |
| Insufficient resource/unmet condition | Distinct real reason, never a fabricated cooldown. |
| Empty/locked | Empty frame or localized locked state. |
| Permanent passive | `상시 / Permanent`; show internal cooldown only when the passive actually has one. |

Resolve equipped IDs through `GameCatalog` and use the definition index for `RunState.cooldowns`, not HUD positions 1–4. Zero-cooldown skills never divide by zero. Radial fraction is remaining time divided by the effective total captured for that cast. Add a source snapshot if the current model does not supply it; do not recalculate the denominator from newly changed stats.

Round remaining time upward: whole seconds at 10 seconds or more, tenths below 10. Never show `0.0` while unavailable. Reuse the 18 active atlas cells and the 18 `passive-WP01`–`passive-MP06` images. Keep frames, text and cooldowns separate. The old atlas already includes frames, so do not double-frame it.

Acceptance: reordered loadouts remain correctly mapped; permanent/conditional/internal-cooldown passives differ; 0/0.01/9.99/10/20 seconds, speed, pause and page changes are correct; clicking icons never triggers combat.

한국어: [스킬](HELLSCRIPT_GlobalHUD_04_Skills.md).
