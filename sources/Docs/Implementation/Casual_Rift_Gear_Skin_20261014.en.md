# Rift gear recommendations — unified integration handoff

2026-10-14 · [한국어](Casual_Rift_Gear_Skin_20261014.md)

**Shared follow-up and approved equipment provider are connected. Individual Unity/build plans are closed; source editing is frozen after this handoff.** All screens will be validated together by common owner `01a13790-39bc-7062-a516-b83a354000a8` on a separate integration branch in task-6/skill-skin using the existing Library. Resume edits only for a specific integration defect request.

## Input and integration order

Branch `codex/casual-rift-gear-skin`, task-10/rift-skin. Original baseline2e193c336c6176d80b9ddbdff7269246bf2590fb contains validated game code4139a77e2df495cee009c07c6094b2eea9b1aeaa. Apply screen consumer commits in order:12880d713ac720e2b177d3f8383bf48584a3aa3e,35fd4986c0d4bf0779dd067dbb3c1850a9b20fac, then the final provider consumer commit containing this document. Exact final SHA/dirty/bundle are recorded in task-10/evidence/FINAL_INTEGRATION_HANDOFF.md.

Common cumulative tip0ccba80eef0da4703cb7ffcd1fb8e22848002b3d includes6cdf2df690941b7c9f751c39235ea36ce4ff9ed9 (geometry/symbols) and f25e67aab62d72f9792b5c0d49e6692bfbca5eed (art). Do not cherry-pick only the last tip commit: its ancestors contain those dependencies. Common merges in this screen branch are f02d6f4c4e2fce6d923a13891888b32fc9e6eacf and b22a82c220884e33c88aeacc6ad649b3b3af3107. Integration may merge the full branch or apply the ordered screen commits over the cumulative common tip.

Only assigned runtime files were authored here: GameUI.GearRecommendations.cs, RiftRecommendationWindow.cs and RiftGearSettingsWindow.cs. All774common files match the supplied tip. Original checkout/index/refs and other work remain unchanged. Sparse checkout relies on original Git object alternates; preserve these objects until integration. This folder lacks complete Resources and Library and is not a Unity execution project.

## Completed connection and approved art

RiftGearSkin.Current caches one `UiSkin.Casual.WithEquipmentArt(CasualEquipmentArt.Provider)` instance. HUD cards, both windows, slots, details/comparison and their replacement paths receive this scope. No screen-owned shared API connection remains. Chart/edict APIs are outside this screen's ownership.

Resources root: `Art/Casual/Equipment/{ID}`. [Shared receipt](../../tools/casual_equipment_art_receipt.json) SHA256:52b29468e0932e9d40e80ed007b27345a69ad08188ae3eb47b004dd3defa3020. Approval ID:`casual-equipment-20261014-359:qa-sha256:619e9fb629ebb03ad0ea78f7bb6f172fb5b0a24c39b59a1798324e33e4d74f4b`.

Shared verify passed359PNGs,330item keys,30emblems,362GUIDs and preservation of336legacy assets. Total69,753,836bytes. IDs/B28-to-B32 alias/catalogue/unlock rules remain. Independently matched source359/shared359SHA and CSV/director/QA/index hashes: [art evidence](CasualRiftGearSkinEvidence20261014/approved-art-followup-audit.json). Local image generation/copies/deletions:0. New bundle excludes common art ancestry as a prerequisite, avoiding another359PNG archive. Resources increases shared build inclusion even outside opt-in use; import/loading/rendering remains unverified.

Removed superseded local tab ApplyLabel, repeated OnLight(Muted), and fullwidth-plus replacement. Noto400/700/900 now covers U+FF0B and requested symbols, so the original label/key is restored and common LabelFont falls back from Jua to Noto400. Actual width needs integrated render validation.

## Design, before/after and remaining differences

Design:https://claude.ai/artifact/JfwkCuft9ZivYvSNvYwceJ . Declared16boards; DOM/content/iframe source/state16; enlarged visual views3. Incomplete14/16PDF is not the design basis. A subsequent EN landscape accessibility inspection is not an additional visual capture. No Chrome used this follow-up; future browser work requires dakrdong@gmail.com profile verification. Original key capture Library:`libfile_547bac59b30c81918af6cfff2aa6fff0`. Game captures:0.

Dark cards/settings become cream/navy shared surfaces, common buttons, Noto700names/scores, Noto400body and approved art. Detail/comparison shares the same skin. Score pills remain cream with green border rather than reference pale-green fill: common semantic text would be about3.858:1 against the green. Final reference acceptance remains pending along with long EN labels, original fullwidth-plus Noto fallback width, scroll/input/error states, rounded corners and new art rendering.

Direct coordinates/font sizes/eight v.Button callbacks/comparison/numeric/slider callbacks remain. Card width92, slot+98height (portrait150/existing landscape152), landscape.75scale, HUD position, legacy best-fit7-10 and ellipsis are unchanged. No new shrink/widen/Canvas/update loop. Timer/equip/dismiss/transaction/lifecycle tail is byte-identical to original baseline. Draft/Normalize/atomic save/portal-return contracts are preserved.

Four prior common geometry differences are restored in source: score13/16pt and plaque52/64; original major-divider height; greater-affix inset16; socket24/inset28. [Independent review](CasualRiftGearSkinEvidence20261014/common-followup-review.md). Six runtime geometry cases and font wrapping are unexecuted. Legacy ItemCardArt Badge/Divider/Star/Socket resources still provide presence/dimensions; do not retire them as unused decoration.

## Completed static checks

Latest contract15passed/22.499s: [receipt](CasualRiftGearSkinEvidence20261014/ui-contract-followup.json), [log](CasualRiftGearSkinEvidence20261014/ui-contract-followup.log). Refresh16bindings/192sources passed. [Source/tree/glyph audit](CasualRiftGearSkinEvidence20261014/shared-followup-audit.json) passed774shared identities, provider scope, callbacks/direct geometry and Noto glyphs. [Script](CasualRiftGearSkinEvidence20261014/audit-shared-followup.py). Shared installed-art verify and source/shared359SHA comparison passed. Source RGB is not device certification. Independent review found no new consumer defect. Earlier failed/invalidated receipts are retained as historical evidence.

This screen ran no C# compiler, Unity, EditMode, player, build or game capture. Roslyn reference preflight was read-only; separate compile/test proposals are superseded by the unified integration instruction. Completed checks were not rerun.

## Unified validation targets

Related Hellscript.Tests filters: RecommendedEquipmentTests, SharedUiTests, ItemComparisonTests, StoreViewBindingTests, UiButtonTests, CasualUiSkinTests, EquipmentArtTests. Integration owner selects against final inputs without duplicating common checks.

Entry controls: rift-gear-icon-{id}/rift-gear-inspect-{id} open detail/comparison; rift-gear-quick-equip-{id}; rift-gear-settings-{id}; rift-gear-dismiss-{id}. Existing smoke flags:`-hellscriptEdictControlsSmoke -hellscriptEdictGearSmoke`.

KO/EN440x956,956x440,1600x900,1600x1000,2100x900,2868x1320. Empty/selected/disabled/error, save/cancel/invalid numbers/add-option/scroll, comparison target/repaint, equip/dismiss/expiry/portal return, round icon edges/panel corners/long labels. Isolated save, muted audio. Mac resolution simulation is not physical iOS; integration runner records safe-area assumptions.

Parent observed external Unity PID30116 running at07:50; this task did not manipulate it. Individual queue is closed in favor of the unified run. Preserve source/branch/evidence. No remote push/public sharing/main merge/web build/deployment. Proposed shared rules: separate decoration from geometry/font approval, contrast against actual backgrounds, and scoped-art persistence through redraw.
