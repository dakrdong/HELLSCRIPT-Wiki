# Item artwork coverage

Updated: 2026-09-30

Korean: [아이템 이미지 누락 보완](Item_Art_Coverage.md)

## Findings and scope

Rift rewards used stat glyphs for runes, including an incorrect heart for the magic type. Materials and enhancement stones shared a diamond placeholder; existing per-slot core art was not connected. A full catalogue audit also found five base items and 24 pieces from the six legacy sets without dedicated artwork. The six legacy set emblems were also missing. Earlier integration covered 141 legendary and 105 additional set pieces, not these legacy images.

This change generates 42 original transparent PNGs: five base equipment items (B25, B26, B27, B29, B30), 24 legacy set pieces, six set emblems, five rune types, crafting materials, and an aspect-stone backing. Legacy sets are SW, SA, SM, SWB, SAB and SMB, each with head, body, hands and feet. Existing enhancement-stone, eight core-slot and gold artwork is reused. The original 24-cell equipment atlas, other equipped item images, gems, potions and reward boxes retain their existing art.

## Shared ownership

Equipment continues through EquipmentArt and EquipmentSlotView across inventory, storage, shop, blacksmith, rewards, details and comparisons. CurrencyIconView.ForResource resolves the exact currency, core slot or gem. Reward slots and their shared detail cards use the same lookup.

RuneItemArt uses the actual five RuneV13Catalog.TypeIds: attack/red, magic/blue, support/yellow, critical/violet and skill/gold. The owned polyhex shape remains visible in rewards. Rune storage, selected details, fusion and reshaping retain functional shape geometry with an artwork badge. Board occupancy, rotation, connections and ability symbols remain gameplay graphics. Aspect stones retain their stable per-ID sigils over the native stone texture.

ItemDetailView.CreateDefinition accepts a display-only artwork callback. Reward details preserve RewardSnapshot ownership. Artwork does not intercept pointer events. Rendering does not grant items or mutate saves, equipment, salvage, drop rates or combat stats.

## Native assets and provenance

[Requests](../Art/ItemIcons/requests.json), [legacy set requests](../Art/ItemIcons/legacy-set-requests.json), and the [manifest](../Art/ItemIcons/manifest.json) record exact IDs, prompts, paths, source filenames and hashes. Built-in imagegen returned transparent PNGs. Original pixels and alpha are copied unchanged, without chroma keys or background removal.

The requested default is gpt-image-2, but the built-in surface does not return verifiable model provenance. All new assets therefore remain candidate_model_unknown; runtime integration does not establish model provenance or final production art approval. Unity imports use a maximum 512 pixels, source alpha, Clamp/Bilinear and no mipmaps. Existing GUIDs and assets are preserved.

## Validation

python3 tools/audit_item_art.py covers base and unique equipment, set emblems, rune types, gems, potions, reward boxes and currencies. Functional polyhexes, aspect sigils, world drop markers, controls and empty slots are explicitly excluded from missing collectible images.

The [file audit](ItemArtEvidence/file-audit.json) covers 614 records with no missing images. The [native alpha audit](ItemArtEvidence/native-alpha.json) confirms RGBA, fully transparent pixels and unchanged source bytes for all 42 new images. The contact sheet was reviewed on dark and light 48-pixel backgrounds.

The full Edit Mode suite ran once: 4,747 total, 4,699 passed, 48 failed, none skipped. Forty-seven failure names match the previous main baseline reproduction and this task changes no corresponding domain code. The remaining failure was a legacy set image that was still being produced. After all images were complete, [15 artwork tests passed](ItemArtEvidence/art-final.xml). The aspect-ratio follow-up also [passed its single affected test](ItemArtEvidence/aspect-final.xml). The full suite is not reported as passing or rerun. [Full result and baseline comparison](ItemArtEvidence/full-suite.json)

A macOS Metal development build succeeded with zero errors. The actual player passed 20 reward display/pointer combinations: 440×956, 956×440, 1600×900, 1600×1000 and 2100×900; Korean/English; 100%/120% text size. Six legacy set details and three wallet sizes also passed. Synthetic pointers traversed the real uGUI raycast and click handlers, and reward inspection preserved the account.

Development fixture preparation was corrected for learned skills and the supported Transform parent. Passed primary coverage was reused. Only the remaining aspect/rune screens were retried, settling first-visit attendance/tutorial bookkeeping and closing the rune board to restore its deliberate combat pause before comparing the whole account. [Runtime scope and results](ItemArtEvidence/runtime-validation.json) · [Secondary result](ItemArtEvidence/secondary-runtime.txt)

Additional macOS OS-pointer verification was blocked by cua_repl noWindowsAvailable errors, including after rebinding the exact app and raising its window. The evidence above is actual macOS player rendering and synthetic uGUI input, not OS mouse input or physical-mobile validation.

Wiki generation and integrity checks passed. Of 11 Python checks, one still expected the old vector icons; it was updated to verify the native PNG paths, hashes and public copies, and only that failed check was rerun. JavaScript validation passed 444 page routes, 32 databases, 4,298 record details, search, filters, history, the read-only public mirror and date-only display. [Wiki validation record](ItemArtEvidence/wiki-validation.json)

Public wiki deployment must use merged main; a generated worktree mirror does not establish publication.

![New collectible artwork](ItemArtEvidence/contact-sheet.jpg)

![Magic rune reward details](ItemArtEvidence/rune-detail-portrait.png)

![Legacy set artwork and emblem](ItemArtEvidence/legacy-set-detail.png)
