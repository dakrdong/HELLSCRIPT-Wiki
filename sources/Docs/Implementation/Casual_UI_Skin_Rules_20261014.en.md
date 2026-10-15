# Shared casual UI skin rules — skill window pilot

2026-10-14 · [Korean canonical document](Casual_UI_Skin_Rules_20261014.md) · [Follow-up ownership and validation queue](Casual_UI_Parallel_Handoff_20261014.en.md)

The Korean document is the source of truth. This pilot applies only to `SkillTreeD4Window`. Apply no automatic changes to other windows, HUD, attendance, currency, or rewards. Read each future screen's separate design and contract first. Unresolved fidelity items are in the [pilot report](Casual_Skill_Window_Skin_20261014.en.md).

## Ownership and source

The runtime basis is `casual/SkillWindow.casual.locked.dc.html`; the live Claude canvas confirms states and appearance but uses some larger typography. Compile appearance from the design through `golden/styles.json`, `build_skill_tree_d4_looks.py`, and `SkillTreeD4Looks.json`. Use the FX data for effect colors and timing.

The global Obsidian & Gilt ramp and `UiFonts.Body/Display/WebBody` remain intact. Only D4 colors and `CasualDisplay/CasualBody` opt into the skin. Existing domain/host code owns IDs, rules, text, save schema, purchase/refund restrictions, selection, close, and scrolling.

## Approved acceptance scope

The criterion approved on 2026-10-14 preserves position, size, font size and behavior while permitting only design-driven color, stroke and corner changes. Body 400/emphasis 700–900 remain approved. The 3,600 raw color/stroke/radius/derived-hash changes are accepted under it. Zero golden position/size differences do not establish pixel equality for every text child. CSS ring/padding/line-height dimensions were restored: 36 combinations now have zero geometry and font-size difference groups. The current skill implementation keeps original widths as minima and expands some internal slots, but **these expansions are pilot exceptions awaiting acceptance, not settled common rules**. Exact before/current widths and reversion fit results for linked names, modifier ranks and stat slots are in the [handoff](Casual_UI_Parallel_Handoff_20261014.en.md#separate-acceptance-decision-internal-widths); do not extend them automatically to other screens. The user separately requested gauge containment, continuous link junctions, clean circular/large corners and removal of this window’s bottom legend.

## Colors and state

| Role | Design colors |
|---|---|
| Navy outline / primary text | `#2a3150` |
| Cream / white surfaces | `#fffaf0`, `#fff6e3`, `#fff3d6`, white |
| Body / secondary text | `#4a5272`, `#6b7393` |
| Learned / selected / key passive | `#ffd23f`, `#ffc21a`, `#ff9f1c`; value `#e08a00` |
| Passive / links / information | `#35a7ff`, `#2f8fd8`, `#1f6fb5` |
| Available / inactive | `#8a93b5` |
| Locked | `#e6e9f0`, `#c8cdd9`, dashed outlines |
| Warning / close | `#ff5a5f` |
| Invest | `#8ee07a` → `#4cbf55` |
| FX | gold `(255,194,26)`, blue `(53,167,255)`, ash `(150,156,180)` |

Use generated palette tokens at runtime. Preserve computed state, rank, reasons and selection alongside color and shape. Gold decoration does not introduce a new gameplay rule.

## Typography and dimensions

Use Jua for titles, tabs, numbers, buttons and headings, and Noto Sans KR 400 for ordinary body and explicit 700/900 for emphasis. Bundle OFL licenses and project-specific derivative family names. Preserve the 2,350 KS X 1001 syllables, Latin and current design characters. Body symbols unsupported by Jua use the Noto body font.

Keep existing font sizes and outer rectangles. Values below are CSS design points converted through the established `2×` uGUI boundary.

| Element | Landscape / portrait / PC |
|---|---|
| Title | 24 / 27 / 26 |
| Tabs | 13 / 14 / 13 |
| Detail name | 16 / 19 / 16 |
| Detail heading | 12.5 / 14.5 / 12.5 |
| Refund / invest | 13; existing blocked-reason size 11.5 |
| Panel gap | 4 / 5 / 10 |
| Base panel padding | 12 / 16 / 14 plus existing border boundary |
| Tab gap | 6 / 8 / 6 |
| Close / square actions | existing 44 × 44 target |

Keep original line heights and fitting rules. Reserve the original estimated flow height even when the new font wraps into fewer lines, so detail panels and modifier rows retain their existing rectangles. Measure linked-chip text with the actual Canvas scale. Partial-line measurement excludes color tags and balances emphasis tags so markup does not inflate glyph widths or push modifier rows over the footer. Do not hide differences by shrinking fonts or increasing test tolerances. The Noto derivatives normalize line metrics to 880 / -220 / 0 without changing outlines or advances.

## Panels, nodes and buttons

Use the sky-to-cream background, cloud accents, cream panels, navy outlines and downward hard shadows. Read 3px outlines, 5px hard shadows, 6px inset 2px dashed stitching and 18px gems from appearance data. Plate radii are 18 for landscape/PC and 20 for portrait; those values also live in the old layout contract, conflicting with the original raw diff=0 requirement but permitted as corner changes under the approved criterion.

Use state look IDs for cards, modifier sockets, circle/hex passive medals, rank arcs, tier badges and selection rings. Links use 3px strokes with a 7px navy underlay; locked links are dashed. Titles use one `BaseMeshEffect` with reusable vertex lists, without eight Outline components on every label. Remove the blurred detail art layer. This pilot retained skill artwork and paths; authorized follow-up image replacements use the asset procedure below. A 4pt gauge stays within the rounded padding box and follows the card’s pressed scale. Draw all link casings before colored cores; paint circular fills/shadows with circular geometry. Place contour vertices at large corners and use one rounded viewport mask for tree content and fade, without per-node masks. Hide the bottom legend at the user’s request, retaining model strings, status/reason labels, frame and scroll dimensions. Refund is bright, enabled invest is green, and close is coral; keep disabled-action behavior and input shielding. Inert transparent raycast surfaces absorb disabled-control input without passing it to another modifier or the game. Footer controls render above rows, and overlap is checked.

## Responsive layout, input and accessibility

Preserve vertical tree scrolling with a fixed bottom detail panel in portrait, horizontal scrolling with a right detail panel in landscape, and the fitted full tree on PC. Preserve SafeArea and `SkillTreeD4Units.Fit`.

Decorations do not receive raycasts. Retain existing purchase, refund, selection, tab, blocked-action, scroll and close paths and target sizes. Pair color with existing ranks, reasons and state labels. Existing keyboard behavior is unchanged. Landscape close/footer targets are 44 design points, active cards 38 and modifier sockets 32; do not claim every target meets 44pt. macOS checks do not certify screen readers or real-device touch accessibility. Small orange value text may fall below ordinary body-text contrast requirements; retain fidelity and record the need for contrast/device readability review before extending the system.

## Image reuse, creation, replacement and retirement

The additional user instruction on 2026-10-14 authorizes creating missing new-concept artwork required by assigned UI work and retiring unused old resources after replacement and visual validation. Shared ownership, the single Unity queue and save protection remain. This procedure does not direct blanket regeneration or deletion of old artwork.

1. **Reuse first:** Search the original checkout's [art handoff](</Users/t8g-2410-pn-005/Documents/MyProject/HELLSCRIPT/HELLSCRIPT/Docs/Art/Art_Transition_20261014.md>) and [asset index](</Users/t8g-2410-pn-005/Documents/MyProject/HELLSCRIPT/HELLSCRIPT/Docs/Art/Art_Transition_20261014_Asset_Index.json>) for IDs, current revisions and paths. Match the latest per-ID approvals in the referenced `casual-art-brief/DIRECTOR_STATUS.json` against file SHA256 values in `FINAL_PRIMARY_EXPORTS_QA.json`. The 650-master/700-PNG delivery and held-item list are a search starting point, not proof of current UI integration. Reuse approved current bytes; do not identify approval solely from a historical HEAD in a document.
2. **Create only missing artwork:** Open the actual canonical reference pixels and the assigned UI design. Consult shared `STYLE_GUIDE.md` v05, the StyleD title and relevant character/item originals, giving precedence to the latest user correction and specified UI design. Record the required ID, meaning, dimensions, alpha, consuming screens and why existing candidates do not fit, then create only the needed artwork with supported `image_gen`. Do not report a filter or arbitrary geometric substitute as new original artwork. Paid external services and new account connections are outside authorization. For shared assets, give the parent the required IDs and consuming screens and obtain **one production owner** to prevent duplicate generation or replacement.
3. **Connect and validate:** Check dimensions, transparency, readability at actual small-icon size, and selected/locked/active state meanings. Preserve existing `.meta` and GUID when replacing an image at the same path. New IDs connect through presentation mappings without changing gameplay meanings or save keys. Use the parent's Unity queue for imports and actual screen validation; record input/state/KOEN/relevant-aspect evidence. A generated file alone is not completed integration.
4. **Trace old references:** After replacement and visual validation, inspect project-wide paths, GUID/metadata, scenes/prefabs/data and loading code. Trace `Resources.Load` strings, constructed paths and active asset catalogs in code. Confirm dependencies from other screens and active tasks through the parent. Zero static search hits alone do not establish non-use; preserve anything with remaining or unresolved references.
5. **Recoverable retirement:** Record original path/SHA256/logical bytes/GUID/metadata, replacement ID/path/SHA256, visual proof, full/dynamic/active-task reference checks, recovery path/procedure/verification, owner/disposition/approval in the existing asset/lifecycle record. Retire only unused files within authorization whose normal product state can be restored. Protect unique originals, user saves, dirty source and necessary evidence. Git history alone does not authorize irreversible deletion. **Present exact paths and measured sizes to the parent and obtain separate approval before permanent deletion or emptying Trash.** Reuse verified recovery originals/records without adding large backup copies.

Record actual same-volume `df` free space before/after separately from logical file sizes. Distinguish archive movement, APFS shared blocks and concurrent writes from recovered capacity. Web-build cancellation and remote-push/GitHub-art-sharing limits remain. Follow the [artifact retention rules](../../AgentRules/Artifact_Cleanup.md).

## Validation, performance and retention

2868×1320 is the user-supplied render-pixel target. The Mac probe assumes DPR 3 and synthetic left/right 102px, bottom 54px, top 0px insets. Record its 956×440 logical frame, actual Canvas scale and content coverage; do not identify these as official device specifications or an iOS hardware result.

Run related golden/reference/static checks, affected EditMode tests, then one silent native development player and inspect actual captures. Keep semantic equality and appearance differences separate, preserve failures, and never weaken goldens or tolerances to claim acceptance.

Refresh font/look/mesh caches on meaningful changes, retain shared line meshes and avoid node-specific materials or hot-path string construction. Claim performance changes only from at least three matching before/after CPU/GC runs. Performance was not measured here.

Use disposable saves. Keep builds/caches in one task-owned location with sizes, dependencies and retention in `artifact-lifecycle.json`. No web build, public deployment, remote push or art sharing is included.


The approved follow-up screens can now consume the [first opt-in API contract](Casual_UI_Parallel_Handoff_20261014.en.md#opt-in-shared-api--first-integration-contract) with `UiSkin.Casual`. Legacy defaults and global theme values remain unchanged. Static compilation and queued Unity/native acceptance are separate; this API has not yet completed native acceptance.


## Common implementation follow-up (2026-10-14)

Follow the [common handoff](Casual_UI_Parallel_Handoff_20261014.en.md#common-follow-up--current-contract) for explicit instance skins and approved artwork. Skin changes must preserve score/divider/affix/socket layout slots. Tab labels retain contrast after selection changes; muted ink is checked against its actual surface. Extend shared Noto symbol coverage rather than duplicating fonts per screen. Configure HUD scope before construction and keep world colours in preview-only materials. Reference-resolution/column changes that shrink physical text require an explicit exception decision. Static source evidence is separate from pending Unity/native acceptance.
