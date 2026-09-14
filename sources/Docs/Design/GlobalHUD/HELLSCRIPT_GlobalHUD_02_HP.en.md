# Global HUD 02: health

Date: 2026-09-14

Show a thin red HP fill and current/maximum text beside the portrait. Use only an individual shallow track and fine outline, never a large panel. Text is ivory with a dark outline. When shields exist, show a separate thin cyan line and shield amount; do not add shields to HP and imply overhealing.

Read `RunState.health`, actual `HeroStats.hp` and `RunState.shield`. Fill is `clamp(current / maximum, 0, 1)`; invalid maximums display `—`. Death follows simulation state, not a UI threshold. A gentle outline emphasis at or below 25% HP is proposed; this warning threshold is separate from automatic potion logic. This implementation updates both the number and fill from the same real snapshot. Low-health emphasis and fill interpolation remain optional visual follow-ups.

If town has no current-health source, show the maximum with a pre-combat label. Do not carry stale low combat HP into town or invent a refill. Compose the 9-sliced `frame-vital`, tinted `fill-vital` and separate text; shields reuse `fill-white`.

Acceptance: 0/25/100% health, shield creation/exhaustion, changing maximum, death/restart and training copies. Text scaling must remain readable without overlap.

한국어: [체력](HELLSCRIPT_GlobalHUD_02_HP.md).
