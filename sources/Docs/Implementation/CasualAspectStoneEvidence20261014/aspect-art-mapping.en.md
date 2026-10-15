# Aspect Runestone representative art mapping — 2026-10-14

[한국어](aspect-art-mapping.md) · [Approved IDs and hashes](approved-art-manifest.json) · [Retention](aspect-art-lifecycle.json)

Only an explicit `AspectRuneGraphic.Create(..., awake, casual: true)` selects `Art/CasualAspectStone/LW02` for `LW02`. The default `casual: false` and the remaining 122 aspect identities retain `Art/ClassAspectIcons/{id}`. The shared stone backdrop, object names, child ordering, geometry, state tint and raycast behavior remain unchanged. This adapter does not change IDs, game rules or saves.

All 124 approved source PNGs (one stone and 123 aspect identities) exist and match the director ledger SHA-256 values. Their total size is 134,929,220 bytes. Only `aspect-LW02` was copied; all other entries remain source-only mappings. Original artwork and existing metadata are preserved.

The pilot is a 1254×1254 RGBA PNG, 1,168,223 bytes, SHA-256 `7eaedc552682d30ece36195a7d7b6cf139aeb250bc577acbd0bbd607a78ff27e`. The copied file has the same hash. Its alpha range is 0–255 with 973,426 fully transparent pixels. Nonzero-alpha bounds `[17, 19, 1236, 1254]` touch the bottom; the approved pixels were not cropped or padded. The existing square identity placement is retained.

The new metadata copies the baseline LW02 TextureImporter policy using a fresh GUID: GPU-only, no mipmaps, sRGB, alpha transparency, Clamp/Bilinear, default maximum 512 and Standalone/Android/WebGL maximum 256. This does not modify the source PNG pixels. Imported quality is not yet verified.

Completed evidence covers source paths and 124 hashes, visual inspection of the representative PNG, dimensions and alpha inspection, and identical copy hash. Unity imports, EditMode, native rendering, save and mobile checks, and expansion to further artwork await parent-coordinated validation. No original art, shared files, public sharing or commits were changed.

The screen now opts into the equipment provider from common followup `0ccba80eef0da4703cb7ffcd1fb8e22848002b3d`. Read-only validation of its common-owner checkout passed for all 359 approved source/destination hashes, 330 item keys, 30 set emblems, 362 GUIDs and 336 preserved legacy artwork entries. No equipment PNGs were copied into this task's sparse checkout.

The aspect sigil and equipment icon share catalogue ID `LW02` but have different display roles. The equipment icon is `Art/Casual/Equipment/LW02`, 512×512, 231,021 bytes, SHA-256 `e922415daabd88aeb3a684b81ed40d1488aa4869cb36f07e534d2d7337605a0f`. Its approved source, pixels and hash differ from `Art/CasualAspectStone/LW02`; none of the 359 equipment hashes duplicates the aspect pilot. Both mappings are retained. [Latest static checks](static-checks.json) and the [font audit](font-coverage-audit.json) record the updated common baseline. Unity import, equipment rendering and native text wrapping remain unverified.
