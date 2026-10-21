# Town plaza overlay — 20-artboard casual design

2026-10-17 · [한국어](Casual_Town_Plaza_Overlay_20261017.md)

## Evidence and scope

The source is the extracted specification and actual HTML section comments for Claude artifact `W1fPqL5NgY8L2cmofoC8i8`, recorded in `design-1-town-plaza.md`: ten PC states, four landscape states, four portrait states and two specification boards. Work started from main `dbec4dbed084596471675f375c1d1ac517b4b86c` with the shared API patch equivalent to `559abd4f`. The original checkout and Unity were not operated.

`GameUI.Plaza.cs` owns the overlay and `TownJoystick` owns its existing movement pad. New `TownPlazaLayout` and `TownPlazaDecoration` helpers are town-specific. World models, facilities, global HUD, content dock, localization and saved data remain with their existing owners.

## Changes

Explicit `UiSkin.Casual` connects cream surfaces, navy edges, gold names/knob and green primary actions. The card reuses `UiWindowFrame`, title/hidden guide reuse `StorageSurface`, and actions use shared button roles, input and the opt-in `SetCasualAppearance` API. NPC names use shared Noto 700, descriptions Noto 400, and secondary/detail inks use shared contrast correction. Long service titles receive a single-line ellipsis without font shrinking.

Mobile cards use a `.88` presentation floor, world labels `.68`, and the header uses 14-point text with a 28-point plate. Portrait cards clear the centered joystick by 12 screen pixels. The return-portal action retains its previous minimum 44-screen-pixel height. Stitches, card gem and knob ring/drop/shine reuse procedural primitives with non-raycasting decoration. The round settings face occupies 68% of its unchanged hit square; its gear occupies 56% of the face.

Only town callers opt into the new bubble skin. The shared helper's default font, color, outline and scale remain unchanged for edict previews. Town bubble visibility also reserves the bottom of the enlarged title plate.

## Preserved behavior and unapplied proposals

Pointer ownership, deadzone, release/focus/window resets, keyboard movement, existing tap routing, arrival without automatically opening a facility, talk/service/buy/sell/portal callbacks and first-visit hints are preserved. The guide, status and menu remain hidden by default. Existing common-panel and guide input gates remain in place.

Resident/portal cards remain 168 high; facility/aspect-stone cards remain 232, including the aspect stone's empty talk row. Guide deletion, sibling reordering and the optional talk icon were not applied. Font manifests and glyphs are untouched, so the middle dot in the title follows the shared Noto fallback. The normal talk button is approximately 38.7 screen pixels high at the design scale; this does not certify every touch target as at least 44 pixels.

The event badge's green/navy/white treatment was requested from the integration owner because `GameUI.Attendance.cs` is outside this screen commit.

## Verification and remaining acceptance

UI contract regression checks passed 15 cases and the refresh checker confirmed 14 guarded bindings. Roslyn compilation retained the real Unity 6000.6.0f1 references and seven analyzers: 853 Runtime sources and 269 test sources compiled with zero errors and zero changed-source warnings. The 225 existing Runtime warnings remain in the logs.

A .NET harness invoked the 14 focused `TownPlazaLayoutTests` cases using candidate DLLs and actual Unity math assemblies. It covered 28 card variants, landscape/portrait and PC 16:9/16:10/21:9, HUD/joystick clearance, portal minimum height and Canvas-unit conversion. This is not Unity Test Runner, rendered UI or actual pointer acceptance.

Actual KO/EN text fit, all service/resident/merchant/aspect/portal states, dock/settings/facility/nested restoration and physical-device touch remain unverified. No Unity suite, game build or main merge ran here. The integration owner performs final grouped acceptance after all sources freeze. Response files, logs, input hashes and small candidate DLLs are retained under the external task's `validation/` with an artifact lifecycle record. Wiki generation/publication is deferred to its single merge owner.

## Follow-up against the complete original specification

All 269 lines of the newly supplied `design-1-spec-ko-original.txt` were compared with the summary and frozen `67552d4c`. Explicit choices remain unchanged; the following mandatory omissions were corrected in town-only files.

- The retained hidden guide now uses PC 300×546, rows46/gaps7/font18; landscape width220/rows30/gaps5/font13; portrait width220/rows40/font15. Mobile placement is left10/top44, while its opener stays hidden.
- Card stitches now use inset4/radius12/width2/alpha.13. The gem is a 16-pixel circle with ring2.5/drop2 and a white/gold/orange gradient using existing `GoldF08A00A1000`. Shared vector primitives add the top radial highlight and offset12/spread20/alpha.24 soft shadow. Pixel equivalence to CSS blur remains unverified.
- Description lines are measured to 21 local units (18.48 at .88). The header never wraps and reserves the prescribed screen−icon−36 maximum width. Knob rings are PC3/mobile2, drops4/2 and shine placement left22%/top18%.
- Title and gear stroke2/drop3 account for presentation/Canvas conversion. The card consumes the integration owner's `UiWindowFrame.SetSkin(skin,borderWidth:3,shadow:4)` to avoid double stroke compensation under nested scaling. Gear hit bounds remain unchanged.
- Town buttons opt into the owner's immutable `UiSkin.Casual.WithInteractionStates()` for the specified Action/Primary hover, pressed offset3/shadow0 and disabled shadow0. The event badge diameter×.26/font×.2 was also reported as mandatory for the shared owner to apply.

No full Runtime/test-assembly compilation was repeated for this follow-up. Roslyn parsed four changed runtime files, and a small helper/test-only .NET harness passed 17 cases (the existing28 card variants plus three guide orientations). Its ten CS0436 warnings are preserved: newly compiled helpers intentionally take precedence over older Runtime metadata. Errors were zero. Final semantic validation including the new shared APIs remains with the integration owner's single candidate pass. Earlier 853/269-source compilation evidence applies to the first frozen commit `67552d4c`.
