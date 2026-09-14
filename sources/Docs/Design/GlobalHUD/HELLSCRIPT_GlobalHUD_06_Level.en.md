# Global HUD 06: level

Date: 2026-09-14

Show current level as `Lv.24` below the seal using `badge-level` and live localized text, never baked digits. Both orientations share the same notation. Read `HeroSave.level`; distinguish training-copy levels and never persist them back to the real hero. Update level and XP from the same snapshot. No-selection/loading uses `Lv.—` or the localized unavailable state.

On level-up, update the number immediately. A brief badge emphasis remains an optional visual follow-up. Do not add a blocking full-screen level-up dialog to this HUD scope. A maximum level exists only when explicitly supplied by growth rules.

Acceptance: one- and three-digit levels and maximum state fit; Korean/English and text scaling do not overlap seal/vitals/XP; rapid hero changes and training exit restore the correct level.

한국어: [레벨](HELLSCRIPT_GlobalHUD_06_Level.md).
