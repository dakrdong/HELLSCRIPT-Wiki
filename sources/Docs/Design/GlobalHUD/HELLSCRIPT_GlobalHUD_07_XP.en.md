# Global HUD 07: experience and 10% ticks

Date: 2026-09-14

Landscape places a thin XP line nearly edge-to-edge along the bottom inside the selected aspect area and safe area. Show only fill, unfilled track, ticks and brief text. Never add an opaque strip or reduce camera height. Portrait retains the approved short left track.

Divide the track into ten equal segments: nine internal ticks at 10–90%, with endpoints representing 0/100%. Make the 50% tick slightly longer; do not label every tick. A 62% fill ends just beyond the sixth tick. Ticks retain normalized positions on resize.

Use `HeroSave.xp / Economy.XpRequired(level)`, not lifetime accumulated XP. After leveling, use the new level's actual remainder and denominator, including multi-level rewards. Only an explicit maximum-level state displays `MAX`; never divide by zero or invent a level cap. Training is separate from saved progression. Floor whole-percent text by default to avoid indicating that the next tick was already crossed. 100% may appear only as a real transient state before growth processing. Decorative effects must not obscure progress.

Tint `fill-white` with XP gold and use `xp-tick`, `xp-tick-major`, `xp-cap`. Proposed thickness is 3 logical units, with pixel alignment calibrated to the display scale. Acceptance: 0/10/50/62/90/100%, leveling/max, notch/20:9/small window, nine fixed internal ticks and bottom gesture clearance.

한국어: [경험치](HELLSCRIPT_GlobalHUD_07_XP.md).
