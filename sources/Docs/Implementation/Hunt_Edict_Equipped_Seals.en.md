# Emerald equipped skill seals in Hunt Edict

Updated: 2026-10-01 · [한국어](Hunt_Edict_Equipped_Seals.md)

## Appearance

Equipped normal actives and ultimates use generated emerald-lit variants of their existing metal seals. The eight-point circular active seal and four-point ultimate crest were each supplied as an image reference. Slot-number badges and the procedural equipped circle are absent.

The tree, equipment sockets and inspector header share the same frame art and registration. Equipping switches to emerald; removing returns to bronze. Empty sockets are bronze. Passives retain their always-applied allocated-rank behavior and do not receive an equipment marker. Unequipped selection highlighting remains; equipped skills no longer have an additional gold backdrop glow.

The sprite changes within the original centered frame rectangle. Icon position, size, measured frame opening and hit target remain unchanged. No separately positioned geometric outline is drawn. Actual rank remains below the seal. Equipment changes still edit the draft, with the existing Save/Revert transaction.

## Images and provenance

The built-in image-generation surface edited each original frame reference. Generated PNGs were copied byte-for-byte with native alpha intact, without drawing, cropping, recoloring or background removal.

| Asset | Reference | Original |
| --- | --- | --- |
| [Emerald active seal](../../Assets/HELLSCRIPT/Resources/Art/SkillTree/node-frame-active-equipped.png) | Existing active seal | 1254×1254 RGBA |
| [Emerald ultimate seal](../../Assets/HELLSCRIPT/Resources/Art/SkillTree/node-frame-ultimate-equipped.png) | Existing ultimate crest | 1254×1254 RGBA |

[Full prompts, hashes and provenance](../Art/SkillTreeUi/equipped-frame-provenance.json) record the original references. The callable surface exposes no verifiable exact model identifier; status remains `candidate_model_unknown`, without a specific model claim or final art approval.

Corners and center are alpha zero. The active/ultimate files contain 1,022,465 and 1,180,976 fully transparent pixels. Like the original frames, a few sub-visible alpha 1/255 pixels remain in the clear opening. Native alpha is unchanged.

`measure.py` checks that opening measurements differ from the existing registration constants by at most 0.015. Unity uses input alpha, a full-rect centered sprite, Clamp, Bilinear, no mipmaps and a 512 maximum; existing `ResourceTextureBudget` platform settings are reused.

## Validation

Focused sources and assets correspond to integrated code `e26da6b4`. [Scope and hashes](HuntEdictEquippedSealsEvidence/validation.json) · [Resource Edit Mode report](HuntEdictEquippedSealsEvidence/editmode.xml) · [Native input results](HuntEdictEquippedSealsEvidence/menu-runtime.txt) · [Fresh-process restoration](HuntEdictEquippedSealsEvidence/menu-restart.txt).

- Shared UI ownership and 11 Python checks passed. Three focused resource Edit Mode cases passed with zero failures/skips. No unrelated full combat suite was run.
- Unity 6000.6.0f1 macOS development build completed with zero errors. One final interaction batch covered the five portrait/landscape/PC profiles and Korean/English at default text size. No text-size matrix was run.
- Every tree node was checked for its equipped/bronze/passive frame, unchanged center and registration size, and non-intercepting images. Equipment/replacement/removal, Revert/Save and a fresh native process restored the correct frame states.
- Default policy On/Off, browse-only draft/owner immutability, safe-area and rotation were included in the same batch. Thirty-nine captures are preserved. This is native uGUI pointer/raycast evidence, not physical mobile or OS mouse evidence.
- CoplayDev confirmed the primary Editor path/readiness and both sprite imports. No C# errors were observed; five Unity AI NoSubscription callback exceptions are separate from this UI.

Actual game views: [all original captures](HuntEdictEquippedSealsEvidence/native-captures.zip).

![PC equipped emerald seals](HuntEdictEquippedSealsEvidence/tree-bubble-1600x900-ko.png)

![Portrait equipped emerald seals](HuntEdictEquippedSealsEvidence/tree-bubble-440x956-ko.png)

Additional views: [landscape English](HuntEdictEquippedSealsEvidence/tree-bubble-956x440-en.png) · [bronze after removal](HuntEdictEquippedSealsEvidence/equipped-after-remove.png) · [fresh process](HuntEdictEquippedSealsEvidence/menu-restart.png).
