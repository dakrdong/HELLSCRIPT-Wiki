# Hunyuan monster READY pipeline

Updated: 2026-10-12 · [한국어](Hunyuan_Monster_Ready_Pipeline.md)

One command turns one raw Tencent Hunyuan text-to-3D GLB into a game `HELLSCRIPT_HUNYUAN_READY_V1` folder. The daily routine runs it unattended on about 20 monsters. The same GLB and arguments produce the same FBX/PNG bytes.

```
python3 tools/hunyuan_monster_ready.py --glb raw/N19.glb --id N19 --spec tools/monster_pipeline/monster_specs.json --out <work>
python3 tools/hunyuan_monster_install.py --id N19 --from <work> [--id ... --from ...] --clone <scratch>/proj
```

Exit codes: 0 = QA passed, 1 = QA failed (retry with another candidate or setting), 2 = crash. The installer checks QA and the FBX SHA, copies into `Resources/World/HunyuanReady/<Characters|Bosses>/<ID>/`, runs `HunyuanReadyBuilder.BuildFromCommandLine` and the EditMode `HunyuanMonsterReadyTests` on a Unity clone, and copies the Unity-written `.meta`, `Model.prefab` and `LOD*_World.mat` back.

## Steps

1. **Normalise:** import with merged vertices, cut a ground plate (flat area in the lowest 1.5% wider than the ankle-height box), scale, feet on the floor, hips over the origin. Hunyuan does not keep the front (N01/N16 sideways, N11/B04 backwards), so facing is chosen automatically: upright bodies by the torso band's shoulder axis and the toes (only when two soles are found), quadrupeds by the bulkier end, spiders by putting the highest points (tail, abdomen) behind.
2. **LODs:** 6000/3000/1500 triangles (bosses 12000/6000/3000), each from the source. High-genus sources stop near 3000 under collapse decimation and are re-tessellated with a 0.5%-of-height voxel remesh first (up to +3% over target allowed). Smart UV (1.5 px gutters) and Cycles bakes give A/C/N/ORM at 512 px (bosses 1024).
3. **Rig:** archetype `Pivot_*` bones from slices, heat weights, two smoothing passes, two normalised influences. Several variants (3 shoulder placements × arm/flank split at 0.10/0.16 H or none, plus frozen sleeve arms) are skinned on the same bakes and gated. The pick is a passing variant, preferring moving arms, then the smallest worst stretch.
4. **QA:** contact sheet (front, side, back, the game's 49° view, walk/windup/strike poses) and `qa/qa.json`.

## Fused parts

Even with "limbs slightly apart" in the prompt, Hunyuan fuses hands to thighs, inner arms to flanks and blade tips to feet. Under linear blend skinning those faces stretch like rubber on the 155° windup.

- Faces joining distant bones (three or more links apart, a distal limb and another limb, left and right arms) are torn, and the duplicated corners take their own side's weights.
- Farther than `flank` × height from the shoulder joint, an arm vertex keeps no torso weight and a torso vertex no arm weight, and faces joining them are torn. The two-bone blend near the joint stays. An arm fused to the flank then leaves it on a windup instead of dragging it.
- Torn gaps are gated only at parent-child joints (elbow, near the shoulder). Gaps away from a joint (a hand leaving the thigh, a blade leaving the hip, an arm leaving the flank) are natural separations, like the gaps between rigid parts of the procedural models.
- Arms are frozen into the torso only when sleeves fused to a robe would tear off more than 8% of the body (B02, B05).
- A robe without a leg split follows the hips; thighs and shins only carry the feet.

## Gates and why

| Gate | Limit |
| --- | --- |
| LOD triangles | 95–103% of target |
| UV overlap / coverage / blank texture | < 3% / ≥ 20% / max channel > 0.03 |
| Weights | no empty vertex, ≤ 2 influences, heat fallback ≤ 25% |
| Max edge extension (idle walk at 8 phases, Windup .8, Release .45) | ≤ 0.30 m × max(1, height / 2 m) |
| Torn gap at a joint | same |
| Share torn loose from the body (robe feet excepted) | ≤ 8% |
| Right bone on +X, back bone 5 cm behind −Z | importer axis contract |
| Shoulder/leg symmetry | ≤ 25% of height |
| Ground plate left | flat floor area / ankle box ≤ 1.0 |

**0.30 m gate review (2026-10-12).** The accepted READY models measured at the same poses with the tool's ActorRig port (matches Unity `HunyuanMonsterReadyTests` within ±0.01 m):

| Model | Archetype | Height | Max | 99.9% | Edges > 0.3 m |
| --- | --- | --- | --- | --- | --- |
| N02 | Quadruped | 1.04 m | 0.144 | 0.102 | 0 |
| N12 | Hover | 2.0 m | 0.139 | 0.088 | 0 |
| BOSS03 | Quadruped | 4.3 m | 0.215 | 0.162 | 0 |

No precedent is a skinned biped raising its arm 155°: N12 is a short-armed hover body, and the procedural art3d models are rigid parts with zero stretch and gaps at the joints. The 0.30 m limit is therefore kept, not lowered or turned into a percentile. Two things changed: (1) height scaling, because displacement under the same rotation grows with size: 0.30 m at 2 m (15% of height, close to N02's 0.14) scales for 4.3–5 m bosses, and the Unity test uses the same formula; (2) torn gaps away from a joint are not counted as stretch (see above). N03, N06, N08 and N17, which failed on the first run, pass after this at 0.18–0.26 m, and their windup tiles show the arm rising without dragging the flank.

## What the routine must check

- The contact sheet's front tile really shows the front (QA cannot verify automatic facing); otherwise pass `--yaw`.
- In the windup tile, the weapon and arm stay attached and no piece flies off.
- Props hanging far from a hand (flail, censer, bell) follow the nearest body part.
- The note `arms frozen` marks a model whose arms do not move.
- C/ORM maps are not sampled by the shader but ship in Resources (about twice the texture size).
- Unverified: real devices, APK size, frame time, most Spider/Worm/Serpent shapes.
