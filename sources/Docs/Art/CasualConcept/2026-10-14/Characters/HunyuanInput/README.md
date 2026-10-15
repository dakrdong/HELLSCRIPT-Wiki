> 보존 기록 / Archived attempts — 2026-10-15
>
> 아래는 당시 입력·실패·대기 이력이다. 정리 과정에서 이미 생성된 원화·프롬프트·도구·결과를 main에 보존했으며, 취소된 생성·리깅이나 문서의 재시도 대기를 재개하지 않는다. 현재 모델 상태는 `Docs/Implementation/Casual_Hero_Models_Temporary.md`를 따른다.
>
> Historical inputs, failures and queued attempts are preserved here. Cleanup archives existing art, prompts, the opt-in tool and records in main; it does not resume canceled generation/rigging or old retry queues. See `Docs/Implementation/Casual_Hero_Models_Temporary.en.md` for installed model status.

# Rig-friendly hero inputs (2026-10-14)
Re-run of `hunyuan_monster_ready.py --hand-capture` on the 2026-10-13 hero GLBs (height 1.8, Biped):
- PASS with sound poses: HW02 male warrior (no weapon), HW07/HW17 male archer, HW08 female archer v2.
- FAIL or poor: HW04 female warrior (spiky hair tears, arms frozen into the torso), HW05 male mage and HW06 female mage (robe share torn loose).
New inputs for those three: wide A-pose with clear gaps between arms/torso and between legs, no robe/dress over the legs, mitten hands, no cape/scarf, compact smooth hair (see PROMPT_RIG_*.md).

2026-10-14 addendum: hy3d.tencent.ai rejected all four RIG inputs at `resource/review` (code 500 "Detection failed") while older inputs still passed. Tripo Studio accepted both mages as-is; the female warrior was re-drawn fully covered (`RIG_HW04_female_warrior_covered.png`, uploaded to Tripo as JPEG because the PNG upload failed twice).

2026-10-14 Hunyuan text-to-3D attempt for the remaining heroes (female warrior, male mage, female mage):
- Image review (`resource/review` 500 "Detection failed") rejects the RIG A-pose inputs; Tripo generated them well but GLB export is paid-plan only, so no Tripo files were used.
- Hunyuan text prompts are moderated too: wording about body/legs/arm angles is rejected (审核不通过); style wording passes. "Standing in a T-pose with both arms stretched out horizontally to the sides, feet apart" passes.
- Without a pose phrase the models stand with arms at the sides: rig results HW24 female warrior PASS (arms frozen into the torso), HW25 male mage FAIL (pivot symmetry), HW26 female mage FAIL (robe torn). T-pose re-runs (ids HW04P/HW05P/HW06P) are queued.

2026-10-14 evening: T-pose preprocessing (`tools/art3d/tpose_to_apose.py`, arms bent down 38 degrees around a shoulder pivot) lets `hunyuan_monster_ready.py --hand-capture` rig T-posed Hunyuan text results.
- Female warrior: HW04P candidate 1 -> `--angle 38 --xs 0.18 --blend 0.1` -> HW52 PASS with no notes, arms swing in walk/windup/strike, no loose pieces.
- Male mage (HW05P_2 -> HW61/62, HW05P_1 -> HW72 PASS with template arm/leg notes, HW05P_3 -> HW73 FAIL LOD2): gloves detach from the sleeves in the windup pose; female mage (HW06P_3 -> HW63/64, HW06P_0 -> HW71) FAIL. Retry queued with bare hands and small hat brims (ids HW05Q/HW06Q).
- Earlier A-pose works: male warrior HW02 and male archer HW17, female archer HW08 (all with `--hand-capture`, height 1.8).
Source GLBs stay under AssetDeliverables/Hunyuan/2026-10-14/heroes_text/ (not in git).
