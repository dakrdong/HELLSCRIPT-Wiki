# Item detail card ornament art

Updated: 2026-10-05 · [한국어](Item_Card_Art.md) · Applied in: [Shared item detail rebuilt after Diablo IV](../../Implementation/Item_Detail_D4.en.md)

The corners, dividers, score plaque, socket ring and greater-affix mark of the shared item card (`ItemDetailView`) were generated with Codex (GPT) image generation, following the user's request for a juicier look with Codex images where needed. The code-drawn vector ornaments remain only as a fallback.

## Assets

Every PNG is a single-colour white silhouette that the game tints with the item's rarity colour. Originals are stored byte-for-byte under `out/`; the same files ship in `Resources/Art/ItemCard/`.

| Asset | Game file | Source size | Opaque bounds (px, top-left origin) | Use |
| --- | --- | --- | --- | --- |
| [card-corner](2026-10-04/out/card-corner.png) | `corner.png` | 1254×1254 | 49,52 – 1202,1171 | Four card corners; the UV rect is mirrored so one PNG makes all four |
| [card-divider](2026-10-04/out/card-divider.png) | `divider.png` | 2172×724 | 35,237 – 2137,480 | Dividers under the header and before options/special power; detail-window title flourish |
| [power-badge](2026-10-04/out/power-badge.png) | `badge.png` | 1774×887 | 11,73 – 1764,809 | Gear-score plaque, a 3-slice sprite: 30% wing caps fixed, only the middle stretches |
| [socket-ring](2026-10-04/out/socket-ring.png) | `socket.png` | 1254×1254 | 46,30 – 1208,1222 | Socket ring; a socketed gem's art sits inside |
| [greater-star](2026-10-04/out/greater-star.png) | `star.png` | 1254×1254 | 135,89 – 1119,1137 | Greater-affix mark (four-point star with a rising flame) |

Bounds use alpha > 16 and match the UV crop constants in `ItemCardArt`. The source files are never edited.

## Generation record

- Tool: `codex exec -m gpt-6-astra`, built-in `imagegen` mode, one call per asset (five calls). The tool does not report its model, so it is recorded as `unknown`.
- Inputs: [prompt](2026-10-04/item-card-art-prompt.txt) and the style reference [current card screen](2026-10-04/ref-card.png). Exact per-call prompts, source paths, SHA-256 and alpha statistics are in [manifest.json](2026-10-04/out/manifest.json); retention data is in [artifact-lifecycle.json](2026-10-04/out/artifact-lifecycle.json); Codex's final message is [last-message.txt](2026-10-04/last-message.txt).
- Post-processing: none (copy only). No chroma key, background removal or recolouring; all files keep their native alpha.
- Spec deviations reported by the tool: 1254 px instead of the requested 1024 (aspect kept), and RGB of anti-aliased edge pixels is not pure white. The mean RGB of opaque pixels (alpha > 200) is at least 248 for every file (corner 248.6/248.0/248.2, divider 252.9/252.7/253.0, badge 252.3/252.2/252.3, socket 253.2/252.9/253.2, star 253.1/252.7/253.0), so tinting is unaffected and nothing was regenerated.
- The generated originals also remain in Codex's folder (`~/.codex/generated_images/`), byte-identical to the repository copies. Review date 2026-11-04; deletion is not approved.

## Import

- Importer: `ItemCardArtImporter` (alpha transparency, mipmaps, Trilinear, no NPOT rescale, max 1024). The wide divider and plaque are not powers of two, so their size is not altered.
- Platform budget: `ResourceTextureBudget` gained `Art/ItemCard/divider` (Android ASTC 4×4, max 1024, desktop BC7) and `Art/ItemCard/` (max 512) rows and `GetVersion` is now 6. Thin line art uses 4×4 blocks.
- Fallback: a missing resource makes `ItemCardArt` return `null` and the card draws its previous vector ornaments.
- Tint: every ornament is multiplied by the item's grade colour (`EquipmentGradePalette`) and never intercepts input (`raycastTarget=false`).
- Two gradients (the rarity halo and the one-shot light streak) are code-generated soft alpha curves, not artwork.
