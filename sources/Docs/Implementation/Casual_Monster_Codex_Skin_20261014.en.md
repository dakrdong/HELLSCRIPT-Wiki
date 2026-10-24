# Casual Monster Codex skin — implementation, validation pending

2026-10-14 · [한국어](Casual_Monster_Codex_Skin_20261014.md)

**Final handoff:** per-screen Unity/build plans are closed and source is frozen. The [single integration handoff](Casual_Monster_Codex_Final_Handoff_20261014.md) supersedes earlier per-screen slot references below. Only a concrete defect request from the integration owner reopens editing.

The screen-owned implementation and static checks are ready. **Unity import, EditMode execution, native interaction and visual acceptance remain pending the parent’s single validation slot.** No player after screenshot or iOS certification is claimed.

Base: `2e193c336c6176d80b9ddbdff7269246bf2590fb`, validated game code `4139a77e2df495cee009c07c6094b2eea9b1aeaa`. Branch: `codex/casual-monster-codex-skin`. The isolated source-only sparse checkout shares Git objects, not Unity caches; the original checkout/index is untouched. Commit `27d41bc1` first preserves the three existing portrait-fit/fallback/test file changes from `57bba034dad1659e8a36dc9645b1291d2554e497`. If integration already contains that art commit, review only the following skin commit instead of applying the preservation commit twice.

Shared API commit `cdb9c140fbf8b778bd3c0bbc0fbcc68a5d9e3fa8` and its handoff were imported exactly via merge `1dbf3458e1c932ba84022d8b20258eed45631742`, as authorized by the parent.

Followup `0ccba80eef0da4703cb7ffcd1fb8e22848002b3d` was imported without conflicts via merge `4a3c414a7758628a5fb1a48583af73985868f285`. Shared detail geometry, fonts, equipment provider and chart/combat APIs remain exactly owner-supplied. Sparse checkout excludes the359 equipment PNGs and font binaries, so no duplicate working copies were created.

Owned changes: `MonsterCodexWindow.cs`, `.Detail.cs`, new `.Skin.cs` and meta, `RuntimeMonsterCodexSmoke.cs`, `MonsterCodexWindowTests.cs`, plus these reports. Shared theme, renderer, shell and font updates come only from the owner’s imported commit; no direct shared-file edits were made here. Localization, registry, balance and save files remain untouched.

## Before and after in source

Dark surfaces become shared cream panels, navy outlines, rounded white cards and shared gold selection. Controls use white idle, yellow selected, grey disabled and coral close looks. Body text uses Noto400; explicit values/skill names use Noto700; headings use Jua. Button labels use the shared LabelFont glyph fallback from Jua to Noto400. Existing font sizes and hit/layout rectangles are retained; tab widths use the existing content measurement rule with the actual font selected by LabelFont.

Progress and tier fills reuse the shared D4 gauge at the original 3/4-unit track heights. The slider remains SettingsStepSlider. The 35 portrait path mappings and existing full-UV/aspect-fit correction remain intact. No PNGs are copied into this source-only checkout.

The window now opts into `ContentWindowView.Open(..., skin: UiSkin.Casual)`. Close uses CasualClose; other controls use Casual. Shared UiButton/UiButtonFace own input, focus, pressed, disabled and reactivation states. The screen-specific MonsterCodexButtonSkin and its callbacks/alpha overrides were removed. Item chrome intentionally omits backgrounds, so discovered-white/locked-grey D4 card decorations remain as first children while the shared Face stays visible. Body colors now call shared TextColor(Color) directly, removing duplicate screen conversion. Semantic colors retain OnLight via that API; secondary text receives the corrected Muted. Navy tab labels and Noto symbol coverage apply through existing shared calls. Panels, chips and gauges continue using the existing shared D4 renderer. Equipment-only WithEquipmentArt is not applied to monster portrait paths.

## Actual design evidence

[Claude reference](https://claude.ai/artifact/9SNavng98YhUUzUYgxEGYM): 12 supplied, **12 DOM boards confirmed**, **5 individually enlarged/viewed**, **6 valid captured states**. Boards span landscape/portrait, KO/EN, list/normal/boss. Executed interactions: KO landscape undiscovered field0/2, fortress list, normal→elite, tier22→23 ceiling; EN portrait boss Home→tier1, scroll to the final skill. Other boards and town-back were not individually exercised. Twelve srcdoc wrappers, typography readings, captures and two failed blank clips are retained in task-9.

Library review sheet: **`libfile_38a63ef1ddfc819184f30d8517c23195`**, `MonsterCodex_Claude_Review_20261014.jpg`. This is reference evidence, not a game after capture. No tabs were reopened after the new browser instruction; any future browser work must verify the `dakrdong@gmail.com` Chrome profile first.

## Art dependency

[35 IDs with PNG/meta SHA256 and bytes](CasualMonsterCodexEvidence20261014/approved-portrait-map.json): 63,338,362 bytes under `Assets/HELLSCRIPT/Resources/Art/TrainingGround/Portraits/{ID}.png`, including N23/B06. Source art commit `af95064ab794a4007f1ac4c53770e4aeef26413e`; main commit `57bba034dad1659e8a36dc9645b1291d2554e497`. Existing33 GUIDs remain. Training Ground shares these paths. The validation project must include these35 pinned images before running. This skin commit does not contain PNG replacements, generation, deletion or a guessed generation-model claim.

## Validation

- Diff whitespace and shared UI ownership: pass.
- Refresh guard after followup:16 bindings and193 UI source files pass.
- Roslyn source compile using existing Unity6000.6 dependency DLLs: runtime0 errors/91 warnings (none in changed codex files); codex test source0 errors/0 warnings. This is not a Unity import or test run. Initial reference-path/facade setup failures remain recorded.
- The shared font checker passed with original-source/license SHA verification, including all requested Noto400/700/900 symbols. Jua’s five known absent design symbols retain shared LabelFont fallback. All four checked font SHA values match the0ccba80e Git blobs. No fonts or PNGs were generated or copied.
- Registry Python tests fail in the sparse workspace because art fixtures are intentionally absent (initially roster was absent too). With original unchanged data read-only and throwaway test directories, all10 pass. No assertions or fixtures were weakened.
- Added 2868×1320 KO/EN cases and tests for shared shell/chrome retention, discovered/locked card backgrounds, stable hit rectangles, culling/reactivation, shared font fallback/glyph support, tier object identity and save immutability. Existing UV, aspect, badge overlap, locale, lock and stat checks remain. EditMode execution is pending.
- Native smoke adds2868×1320 and shared skin assertions. It retains sandbox saves and muted audio. Run and after captures are pending.

Requested focused filters: MonsterCodexWindowTests, MonsterCodexTests, MonsterCodexPreviewTests plus StoreViewBindingTests.CommitDuringPressWaitsForReleaseWithoutCancellingTheClick, MeaningfulRefreshRestoresTheSameNamedFocusAndModality and RepeatingButtonConfigurationDoesNotRestartAnExistingTransition. Ratios:440×956,956×440,1280×720,1280×800,1680×720,2868×1320, both languages. Safe-area behavior assumes existing UiSafeArea/desktop Screen.safeArea; macOS simulation is not physical mobile validation.

## Remaining differences and coordination

Actual35 entries and combat semantics remain authoritative, including boss slow→stagger. The reference uses a25-entry fixture. Header progress/close, portrait/badge placement and separate defense panels remain different from the reference layout.

The shared rule explicitly preserves existing size and font size. Live Claude detail names22px/section16px/skill names13px differ from baseline15/10-or12/12. Trying22 in the unchanged214×28 identity-name area overflows Graveyard Executioner and Blackiron Crossbowman by font-metric inspection. The unapproved enlargement was removed; no smaller-than-baseline font was introduced to pass checks. Exact reference geometry/font-size acceptance needs parent coordination.

The proposed shared casual opt-in is now supplied by cdb9c140 and wired. Suggested shared rules: keep Item Face visible while providing screen-owned card backgrounds; measure tab width with the actual LabelFont after glyph fallback. The shared Item focus ring retains its legacy chamfer shape; its fit against rounded cards needs native visual review. The430-look catalog is unchanged by this screen. Native review must still check rounded panel/chip corners, circular close edges, full-UV portrait corners, gauge limits and English wrapping. Any layout or shared clipping-renderer change requires coordination.

Integration: with art57bba034 and shared0ccba80e already present, apply screen330d30ff,549939ee and this TextColor wiring commit in order; do not repeat the art preservation27d41bc1. Equipment B01 and monster B01 are separate domains, so the equipment provider is intentionally not attached to codex portraits.

No remote push, public sharing, main merge, web build or Unity process was started. Pending action: parent assigns one validation project and slot. The parent reports external Unity PID30116 in scratchpad/proj-final was still running at07:50 UTC, so Unity was not attempted during this migration. No action was denied or has an uncertain execution outcome.


## 2026-10-23 short-board integration correction

Long English defense titles/values and multi-element captions reserve separate space. Only when a short, narrow board cannot retain the skills minimum does the detail body scroll; the title and rift-tier controls remain fixed. Default font sizes and the existing 44px acceptance minimum remain. Actual native validation is recorded by candidate SHA in the integration evidence.
