# 장비 아이콘 생성 기록

- 생성일: 2026-09-08
- 사용 도구: 내장 이미지 생성
- 자산: `Assets/HELLSCRIPT/Resources/Art/EquipmentAtlas.png`
- 용도: B01–B24의 목록·상세 아이콘. 왼쪽 위부터 가로 6칸·세로 4칸이다.
- 결과: 1536×1024 PNG. RGBA 원본의 알파를 유지한다. 완전 투명 픽셀이 존재하며 실제 게임의 어두운 패널 위에서 가장자리를 확인했다. 원본 파일을 그대로 복사했고 픽셀 편집은 하지 않았다.
- 모델 표기: 도구가 반환한 PNG의 C2PA 생성 메타데이터에서 `softwareAgent.name = gpt-image`, `softwareAgent.version = 2.0`을 확인했다. 인증서의 신뢰 체인을 별도로 검증했다는 뜻은 아니다.
- 검토: 24개 기물의 순서와 셀 경계를 육안으로 확인했다. 불투명 배경을 요청했지만 생성 결과에는 네이티브 알파가 포함되어 있어 이를 그대로 사용했다. 배경 제거·크로마키 처리는 하지 않았다. 현재 시제품 자산이다.

## 생성 프롬프트

Use case: stylized-concept. Asset type: ONE Unity inventory equipment icon atlas for HELLSCRIPT, a dark gothic auto-battler. Create a landscape PNG 1536x1024, an EXACT 6-column by 4-row grid of 24 square 256x256 icon cells. Each cell has an identical opaque very dark blue-black charcoal background (#11151b), no frame, no text, no letters, no numbers, no grid lines. Painterly high-quality gothic game item art, readable bold silhouette, restrained steel, bone, worn leather and tarnished bronze, warm top-left rim lighting. ONE clearly isolated equipment subject per cell, centered and wholly inside the middle 76% of its cell with generous clean padding, no cropping and no art crossing cells. The equipment is the subject; no people, hands holding items or scenery. Layout in exact left-to-right order: Row 1: (1) rusty narrow iron sword, (2) broad steel greatsword, (3) heavy black iron axe, (4) simple wood hunting bow, (5) reinforced dark war bow, (6) compact heavy crossbow. Row 2: (7) ash wood staff with ember tip, (8) sealed pale rune staff with blue crystal, (9) obsidian staff with violet crystal, (10) worn cloth hood, (11) closed iron helmet, (12) dark leather torso coat. Row 3: (13) iron breastplate, (14) cloth glove pair shown as one compact item silhouette, (15) iron gauntlet pair shown as one compact item silhouette, (16) leather boots pair as one compact item silhouette, (17) iron greaves pair as one compact item silhouette, (18) woven cloth belt. Row 4: (19) dark belt with iron buckle, (20) bone pendant on a cord, (21) silver pendant on a short chain, (22) engraved sealed talisman on a chain, (23) plain iron ring, (24) delicate silver ring. Ensure exact uniform cells and distinct materials. These are UI icons, not an illustrated poster. Keep all 24 subjects similarly large and readable; no glow extending into padding. Return the PNG and any available model provenance.
