# Ranger skill icon production record

[Relentless Pursuit AP19](manifests/AP19.json) was added on 2026-09-23, bringing the class total to 37. The record below describes the initial 36 icons. New Sprite import and HTML verification are recorded in the [combined report](../Class_Skill_Icons.en.md).

Updated on: 2026-09-22

Thirty-six Ranger skills now have independent PNG files: 16 normal actives, 2 ultimates and 18 passives. Every item is candidate art made through one distinct built-in image-generation request. No existing icons were duplicated or palette-swapped to fill coverage.

## Source and deliverables

Names and effects follow the frozen `skills.json` in this directory. The original project's `EquipmentAtlas.png`, `GlobalHUD/status-mana.png`, `GlobalHUD/menu-character.png` and `GlobalHUD/status-heart.png` were visually inspected for worn metal, leather, texture and lighting. UI borders and backgrounds were not copied.

Every returned master is a native 1254 × 1254 RGBA PNG. Initial prompts requested 1024px; the revised handoff accepts the tool's native 1254px square output. The original request text remains preserved as provenance. No resizing, chroma key, background removal or alpha reconstruction was performed. The central 76% is a padding guideline; actual clipping and residual backgrounds were inspected separately.

- Project images: `Assets/HELLSCRIPT/Resources/Art/ClassSkillIcons/Ranger/{ID}.png`.
- Verbatim raw copies: `raw/{ID}.png`, with hashes equal to the project PNGs.
- Per-ID manifests: `manifests/{ID}.json`, containing bilingual names and effects, linked equipment IDs, prompts, paths, source hashes, generation evidence and QA.
- Prompt collection: `prompts.json`. The tool exposes no model provenance, so every item remains `model: unknown` and `approval: candidate`.

## Validation

The class-scoped validator passed all 36 project PNGs and 36 raw files for format, alpha, source equality and GUID integrity, with no missing items or global errors. Actual 64px thumbnails were inspected against dark and light backgrounds. Arrow skills use different trajectories: straight penetration, outward fan, consecutive parallel shots, a split junction and downward rain. Passives use steady symbolic forms rather than attack scenes.

Warnings consist of 36 unknown-model notices, 4 faint-border notices, 3 core-padding notices and 3 faint-padding notices. Visual inspection found no opaque background, visible matte or halo, or primary form clipped by the canvas. Wide compositions A01, A15 and A18 are retained verbatim. Measurements are recorded in `qa/validation.json`.

`qa/Ranger-64px-dark.png` and `qa/Ranger-64px-light.png` are QA-only contact sheets. Their labels and backgrounds are absent from the game PNGs. Missing Unity metadata was created with fresh GUIDs without rewriting existing metadata. This is file-level asset verification; live Unity importing, game UI integration and physical-device validation are outside this record.

## Coverage

| ID | Korean name | English name | Kind |
| --- | --- | --- | --- |
| A01 | 관통 사격 | Piercing Shot | active |
| A02 | 다중 사격 | Multishot | active |
| A03 | 맹독 덫 | Venom Trap | active |
| A04 | 후퇴 도약 | Retreat Leap | active |
| A05 | 사냥꾼의 표식 | Hunter's Mark | active |
| A06 | 그림자 화살 | Shadow Arrow | active |
| A07 | 잇단 사격 | Successive Shots | active |
| A08 | 응시 사격 | Patient Shot | active |
| A09 | 가시 덫 | Briar Trap | active |
| A10 | 서리 올가미 | Frost Snare | active |
| A11 | 미끼 투영 | Decoy Projection | active |
| A12 | 연막 엄폐 | Smoke Cover | active |
| A13 | 독화살 | Venom Arrow | active |
| A14 | 갈라지는 화살 | Forking Arrow | active |
| A15 | 경계 쇠뇌 | Watch Ballista | active |
| A16 | 사냥 준비 | Hunt Preparation | active |
| A17 | 일제 소탕 | Killing Rain | ultimate |
| A18 | 그림자 추격 | Shadow Pursuit | ultimate |
| AP01 | 긴 사거리 | Long Reach | passive |
| AP02 | 탈출의 발걸음 | Escaping Step | passive |
| AP03 | 꿰뚫는 시선 | Piercing Gaze | passive |
| AP04 | 덫 사냥꾼 | Trap Hunter | passive |
| AP05 | 절약된 집중 | Saved focus | passive |
| AP06 | 마무리 사격 | Finishing Shot | passive |
| AP07 | 표식의 빈틈 | Marked Opening | passive |
| AP08 | 사냥감의 발자취 | Quarry Tracks | passive |
| AP09 | 다시 당기는 시위 | Drawing Again | passive |
| AP10 | 독의 순환 | Venom Cycle | passive |
| AP11 | 연금 숙련 | Alchemical Practice | passive |
| AP12 | 준비된 도주 | Prepared Escape | passive |
| AP13 | 집중 조준 | Focused Aim | passive |
| AP14 | 유인 전술 | Decoy Tactics | passive |
| AP15 | 독과 서리 | Venom and Frost | passive |
| AP16 | 빈틈 없는 재장전 | Seamless Reload | passive |
| AP17 | 길잡이 | Pathfinder | passive |
| AP18 | 무음의 살수 | Silent Execution | passive |
