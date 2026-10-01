# Generated content artwork integration

Updated: 2026-10-01 · [한국어](Generated_Game_Art.md)

Code-authored content pictures now use native generated artwork. Small button pictograms remain code, and 3D changes require demonstrated visual improvement under the user's latest constraints.

- Fifty newly generated reward-box designs replace the existing 85 PNG resources used by 148 definitions. The latest main catalog keeps its IDs, names, grants, tiers, quantities and save/opening transactions. Gem tiers share artwork and retain their existing tier/count labels. Inventory, first-clear pages, Rift loot and claim reveals share these resources.
- The large Rift entry chest reuses existing native storage artwork for closed/open states. Locked/ready/claimed decisions and the ready-state shake retain their existing owner and timing.
- `AspectRuneGraphic` connects 123 previously generated power emblems to released aspect IDs in the library and details. Eighteen design-only IDs remain outside runtime. Collection, effects and imprint transactions are unchanged.
- Small functional pictograms, status symbols, empty-slot symbols, live graphs/gauges/maps/rune geometry, window frames, masks and selection effects remain code. All 3D models, textures and the field monument's procedural glyph remain unchanged; no unverified 3D replacement was applied.

Existing shared slot, safe-area and window owners remain in charge. This changes artwork without adding a UI shell or save owner.

[Native requests, hashes and resource paths](../Art/GeneratedGameArt/requests.json), [reward-box registry](../Art/RewardBoxes/manifest.json) and [aspect provenance](../Art/ClassAspectIcons/manifest.json) record byte-identical native PNG adoption. No background removal, chroma key or pixel repainting is used. RGBA and genuinely transparent pixels are checked. A few outputs contain a single 1/255 alpha corner pixel; this is recorded without changing their native bytes. Resampled/composited contact sheets are QA only.

`tools/generate_reward_boxes.py` now only rebuilds the registry and QA sheet from current JSON and native PNGs. It no longer draws runtime art, overwrites native masters or rewrites reward data. Historical SVGs remain archived. Platform textures are capped at 256, preserving existing reward-box GUIDs.

The built-in generator exposes no verifiable actual model name. No claim of `gpt-image-2` use is made. Artwork remains `candidate_model_unknown` with `productionApproved=false`; development runtime adoption is separate from provenance/production approval. Canceled small button-art candidates are not runtime assets.

The shared UI contract and its 11 checks passed. Native registration verified alpha, copied-byte hashes, Unity metas and preserved paths for all 173 records. Eight related Edit Mode cases completed; one aspect import-size check initially failed. Only the 123 new aspect textures were force-reimported through the existing importers, then that single failed check passed. The failed-job MCP serializer omits its aggregate result, so the initial run is not labeled 8/8 passed. See [validation](GeneratedGameArtEvidence/validation.json), [initial tests](GeneratedGameArtEvidence/edit-mode-initial.json) and [targeted retry](GeneratedGameArtEvidence/edit-mode-aspect-reimport.json).

The macOS Development Player built with zero errors. Its single acceptance batch passed ten KO/EN and viewport combinations at 440×956, 956×440, 1600×900, 1600×1000 and 2100×900, with default text size and simulated safe areas. Synthetic pointer input passed real uGUI raycasts for filters and aspect selection; inspection preserved the account snapshot. Actual saved reward claiming and locked/ready/claimed chest states passed. There are 53 captures. After adjusting only the validation capture timing to wait for the fade, three reward/chest scenes were rechecked without repeating the ten combinations. See [runtime](GeneratedGameArtEvidence/runtime.txt), [focused reward capture](GeneratedGameArtEvidence/reveal-runtime.txt), [final build](GeneratedGameArtEvidence/build-final.json) and [capture hashes](GeneratedGameArtEvidence/captures.json). No physical-mobile claim is made.

![Portrait reward boxes, Korean](GeneratedGameArtEvidence/boxes-440x956-ko.png)

![PC gem boxes, Korean](GeneratedGameArtEvidence/gem-boxes-1600x900-ko.png)

![PC aspect library, Korean](GeneratedGameArtEvidence/aspect-library-1600x1000-ko.png)

![Portrait aspect detail, English](GeneratedGameArtEvidence/aspect-detail-440x956-en.png)

![Reward reveal after fade](GeneratedGameArtEvidence/reward-reveal.png)

![Claimed first-clear chest](GeneratedGameArtEvidence/entry-claimed.png)
