# Global HUD 01: character seal

Date: 2026-09-14

Show the selected class portrait in a small circle at the lower left. Keep portrait, circular mask, metal rim and level badge separate. The seal is not a movement, attack or character-switch button.

| State | Display |
|---|---|
| Warrior/Ranger | Existing SkillAtlas cells 18/19, clipped with a circular mask. |
| Mage | New silver-haired mage portrait. |
| No selection/loading | `portrait-placeholder` and unavailable values. |
| Dead | Keep the same face and communicate the real death state separately. |

Bind character ID, class and combat session as one read snapshot. Replace portrait, vitals, skills, potions, level, XP and effects atomically when selection changes; never show old cooldowns beside a new portrait. Use `portrait-mage`, `portrait-placeholder`, `mask-circle` and `frame-seal`. Adjust portrait framing non-destructively inside the Unity mask. Reuse the project's existing atlas for the other classes without replacing existing artwork.

Acceptance: correct portraits for all three classes and no-selection state; persistence across orientation and town/combat/bag/settings; no artwork escaping the circle; no mixed character data during rapid selection changes.

한국어: [캐릭터 인장](HELLSCRIPT_GlobalHUD_01_Seal.md).
