# Global HUD 04: equipped skills and cooldowns

Date: 2026-09-14

Display three diamond passives and four square actives. Landscape uses the lowest right row at 1.20 times each baseline dimension, above and clear of XP. Portrait keeps its approved arrangement.

Keep the existing three-passive equipment, unlocks and presets. Show all three individually. Landscape separates the passive and active groups by 24 logical units; portrait gives passives their own row. Empty and locked slots remain visible.

| State | Display |
|---|---|
| Ready | Clear artwork; hide cooldown number. Ready does not promise immediate casting. |
| Cooldown | Dark radial fill plus central remaining time. |
| Automatic casting | Brief frame emphasis, no manual input. |
| Insufficient resource/unmet condition | Distinct real reason, never a fabricated cooldown. |
| Empty/locked | Empty frame or localized locked state. |
| Permanent passive | `상시 / Permanent`; show internal cooldown only when the passive actually has one. |

Resolve equipped IDs through `GameCatalog` and use the definition index for `RunState.cooldowns`, not HUD positions 1–4. Zero-cooldown skills never divide by zero. Radial fraction is remaining time divided by the effective total captured for that cast. Store this effective total in `RunState.cooldownTotals`; do not recalculate the denominator from newly changed stats.

Round remaining time upward: whole seconds at 10 seconds or more, tenths below 10. Never show `0.0` while unavailable. Reuse the 18 active atlas cells and the 18 `passive-WP01`–`passive-MP06` images. Keep frames, text and cooldowns separate. Permanent captions use “Always”; when that would exceed a very small slot, use ∞ and retain the full description in inspection. The old atlas already includes frames, so do not double-frame it.

Acceptance: reordered loadouts remain correctly mapped; permanent/conditional/internal-cooldown passives differ; 0/0.01/9.99/10/20 seconds, speed, pause and page changes are correct; clicking icons never triggers combat.

한국어: [스킬](HELLSCRIPT_GlobalHUD_04_Skills.md).
