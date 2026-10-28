# Battle HUD inspection windows casual — 2026-10-18

> **2026-10-28 A2 correction:** References to the large edict window, ≡ menu and their dedicated components or smokes are historical. Use the [current entry points](../../Assets/HELLSCRIPT/Runtime/Presentation/GameUI.HuntEdict.cs) and [replacement screens and source owners](Hunt_Edict_Window_Removal_20261028.en.md). Earlier captures and test counts describe the earlier implementation.

[한국어](Casual_HUD_Inspection_Windows_20261018.md)

Basis: the build spec of the canvas "HELLSCRIPT 전투 관찰 메뉴 · 캐주얼" (FYfemHtqRJFH7wdCg1rsT5). Started from `origin/main` `3d3e0f4f`. All of these windows are drawn by the single `GameUI.GlobalHud.ShowHudPanel`, which already had a casual branch for `skin != null` (used only by the skill popups). This change applies that branch to every window.

## What changed

- `ShowHudPanel` uses `UiSkin.Casual` even when a caller passes no `skin`. The observation menu (rift, training ground, power-saving peek), effect information, slot information (skill and gem elixir), the current effects list, the potion popup and the training pause window share one cream card (`UiWindowFrame.SetSkin`), navy text and white/green buttons.
- The close button was built with the `Item` role, whose face `UiButtonFace` does not draw at rest: the white × was invisible on the cream card. The casual branch now gives it the `Quiet` role so the coral `CasualClose` face shows (this also fixes the skill popups).
- The skill icon row of the information windows uses the cream plate and gold ring of `SkillIconView.SetCasualHudFrame`.
- The casual surface and button handling the training pause window did on its own (`Training pause skin`) duplicated the shared branch and was removed.

## Not changed

- Opening and closing, the pause hand-off, every button action, the names (`hud-details-*`, `관찰 메뉴`) and transactions are as before. The backdrop scrim keeps its existing value, as in the mockup.
- Separate issues the mockup found by reading code are out of scope: nothing calls `ShowHudEffects` (the current-effects list), the 1.5× and 2× speed buttons look enabled but only 1× is allowed, effect windows show raw ids such as `MONSTER_POISON`, and the confirmation dialog sits on a lower canvas (100) so the HUD (105) is not dimmed. They change game rules or copy and need their own decision.
- The phone scale of the mockup (0.72) is not applied. Today it is 0.489 landscape and 0.543 portrait, which makes text small; changing it means changing the shared `ApplyScaler`.

## Verification

- Focused EditMode tests pass: `HudInspectionCasualTests` (3 new), `CasualHudSharedTests`, `CasualBattleHudTests`, `PotionQuickCasualSkinTests`, `CasualUiSkinTests`. The new tests check the card frame, the close button role and the casual fonts, colours and chrome of every text and button, whether or not the caller passes the skin.
- Static UI contract (15) and refresh guard (14 bindings) pass.
- Not verified: the look inside a real battle screen (no Editor capture or macOS build was run), physical devices and touch, long English strings. The full EditMode suite was not run.
