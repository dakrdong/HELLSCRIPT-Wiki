# Global HUD 03: mana and class resources

Date: 2026-09-14

Show a blue resource bar below HP with current/maximum values. Mage mana uses MP; other classes use their project-defined resource names rather than being renamed mana. Color, thickness and text conventions are shared across orientations.

Read `RunState.resource` and current `HeroStats.maxResource`; clamp fill to 0–1 and display `—` for invalid maximums. Resource spending, regeneration and potion restoration are simulation results, never HUD mutations. Distinguish zero resource from insufficient cost for an individual skill. Skills may show an insufficient-resource indicator, but must not invent a cooldown. Missing town/loading/finished-combat data follows HP rules. The UI never regenerates resource during a simulation pause.

Use `frame-vital`, MP-tinted `fill-vital` and independent text. Acceptance covers empty/partial/full resources, maximum changes, repeated consumption/restoration, speed changes and pauses. Register class-specific resource names in Korean and English.

한국어: [마나와 자원](HELLSCRIPT_GlobalHUD_03_Resource.md).
