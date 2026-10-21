# Casual town NPCs (7) and crate stack rebuilt · 2026-10-17

[한국어](TownNpcRebake_20261017.md)

The seven casual NPCs on quality hold (Jacques Chei, Guzel Pan, Darc Alvi, Jean Jorin, Chador Samaf, Injel Mir, Mishu Karu) and the crate stack were re-optimised and put in the game. The seven replace the existing `Npc_*` files with their `.meta` GUIDs kept. The crate stack is a new `Prop_CrateStack`, and it replaces the F1 crate (scale 2.5) at both crate spots at scale 1. Out of scope: the eight town models already applied, monsters N05/N14/N21/N22/N24, the garland, and the male and female mage.

## Cause and new method

The 10-16 candidates kept the Hunyuan source UVs, reduced the faces and re-baked only the normal map (`decimate_normal_only.py`). The source UVs are a fragmented atlas of tiny islands. After decimation, single triangles span island borders, which produces sawtooth gold trim, black seam triangles and face colour bleeding.

This rebuild uses the method of the approved buildings and props (`blender_bake.py`). The decimated copy gets a fresh Smart-UV atlas, and colour, ORM and normal are all re-baked from the high-resolution source. The island margin went from 0.004 to 0.002, which raised UV coverage from 17% to 25–46%.

| Model | Tris | Texture | UV angle | 8-view SSIM | Local defect (blob) | UV overlap |
|---|---|---|---|---|---|---|
| Jacques Chei | 6,000 | 1024 | 85° | 0.966 | 0 | 0.34% |
| Guzel Pan | 6,000 | 1024 | 66° | 0.969 | 0 | 0.09% |
| Darc Alvi | 6,000 | 1024 | 85° | 0.947 | 0 | 0.43% |
| Jean Jorin | 6,000 | 1024 | 85° | 0.970 | 0 | 0.74% |
| Chador Samaf | 6,000 | 1024 | 85° | 0.940 | 0 | 1.23% |
| Injel Mir | 6,000 | 1024 | 45° | 0.961 | 0 | 0.61% |
| Mishu Karu | 6,000 | 1024 | 66° | 0.930 | 0.33% | 1.98% |
| Crate stack | 2,500 | 512 | 66° | 0.939 | 0 | 0 |

- **Injel:** at 85° the inside of the cape took the outer pattern (4.9% UV overlap, 4.45% blob), so 45° is used.
- **Mishu:** the 0.33% blob is a brightness difference at the front shirt edge. The Unity close shots show no vertical lines or triangle marks that the source does not have.
- **Why NPC textures went from 512 to 1024:** at 512 the card diamonds and coat pattern blurred again even with the new atlas (Jacques SSIM 0.945 against 0.966 at 1024). The town folder import cap was already 1024; only the npc budget in `generate_world_art.py` changed to 1024. The cost is 7 NPCs × 2 maps, about +6–7 MB at Android ASTC 6x6.

**Crate stack:** as requested, one new crate was generated with Hunyuan text-to-3D (four candidates). All four were open-topped and strongly metallic, far from the concept's closed lids and flat colours (SSIM 0.861 after the same bake). The adopted version is the existing source (`f58965a5…glb`) re-baked with the new method. The black marks at the plank and diagonal-brace joints are gone in both the Blender eight views and the Unity close shots.

Tried and dropped: vertex-weighted decimation that kept more faces on the head broke the body (SSIM 0.80). Raising the face count to 10,000 left the dark mouth line almost unchanged. Neither is used.

## Evidence

Source, Codex candidate and the new version were compared with the same camera and lights (Blender, face and torso crops from 1024 px renders).

- [Jacques Chei](TownNpcRebakeEvidence20261017/compare-jacques-chei.jpg)
- [Guzel Pan](TownNpcRebakeEvidence20261017/compare-guzel-pan.jpg)
- [Darc Alvi](TownNpcRebakeEvidence20261017/compare-darc-alvi.jpg)
- [Jean Jorin](TownNpcRebakeEvidence20261017/compare-jean-jorin.jpg)
- [Chador Samaf](TownNpcRebakeEvidence20261017/compare-chador-samaf.jpg)
- [Injel Mir](TownNpcRebakeEvidence20261017/compare-injel-mir.jpg)
- [Mishu Karu](TownNpcRebakeEvidence20261017/compare-mishu-karu.jpg)
- [Crate stack eight views](TownNpcRebakeEvidence20261017/compare-casual-crate-stack.jpg)

The macOS development build's `-hellscriptTownModelsSmoke` gained a close-shot step. It renders front, side, back and game-pitch views with the real game material and lights, and hides every other object during these shots.

- [NPCs 1–4](TownNpcRebakeEvidence20261017/unity-closeups-npc-1.jpg)
- [NPCs 5–7 and the crate](TownNpcRebakeEvidence20261017/unity-closeups-npc-2-crate.jpg)
- [In game, near the gambler](TownNpcRebakeEvidence20261017/game-near-gambler.jpg)

Checks:
- Focused EditMode during the work: TownModelTests and TownEnvironmentTests, 24/24 passed.
- The town models smoke passed (`HELLSCRIPT_TOWN_MODELS_SMOKE_OK`).
- `generate_world_art.py --check`: the problem count is unchanged at 2,419 (no new problems).

Reproduction material:
- Bake tool copies, the eight final GLBs and their SHA256: `AssetDeliverables/Hunyuan/2026-10-17/town-rebake/` (outside git)
- The newly generated crate candidates: `AssetDeliverables/Hunyuan/2026-10-17/town-crate/`
- Source RAWs: the existing files in Downloads, read only.

## Remaining

- **Mouth edge:** the source mouths are recessed, so at 6,000 tris a thin dark line remains at the upper teeth and lip edge (Darc, Mishu). It is smaller than the black triangle fragments of the Codex candidates but not identical to the source.
- **Not verified:** physical devices, real Android ASTC output, performance comparison and repeated English screens. In game an NPC is about 70 px tall at 1080p.

## Final game integration check · 2026-10-17

The same final macOS development player passed the town model smoke: NPC/resident, seven building and portal bindings; four close views of all eight rebuilt assets; ten services and merchant occlusion. [Final result](TownNpcRebakeEvidence20261017/runtime-town-models-smoke-final.txt). The 24 town tests passed within the single 156-case focused model integration run.

The run used isolated saves and `-hellscriptOfflineQa`. The first town invocation omitted that flag and failed before character selection; only the town smoke was rerun with the flag. Evidence and player are retained in `Builds/Validation/ModelIntegration20261017/`. Physical devices, ASTC visuals, performance, repeated English views and wiki publication remain unverified.

Final baseline update: town PR #141 landed in the shared checkout and is included in the final player. Changed town coverage was rerun and passed 27/27; the earlier 24 cases are not reused as proof for the newer town source.
