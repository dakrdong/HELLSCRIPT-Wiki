# HELLSCRIPT 균열 바닥 이미지 생성 기록

작성일: 2026-09-08

- 내장 이미지 생성 도구로 새 석재 바닥 이미지를 생성했다. 직접 API·CLI는 사용하지 않았다.
- 생성 PNG: `1254 × 1254`, RGB. 불투명 재질용 이미지이며 투명 배경을 요구한 자산이 아니다.
- PNG 메타데이터에서 `softwareAgent`의 `gpt-image`와 `version`의 `2.0` 표기를 확인했다. 인증서 체인의 독립 검증을 수행한 것은 아니다.
- [프로젝트 원본](../../Assets/HELLSCRIPT/Resources/Art/RiftStone.png)을 복사해 사용하고 원래 생성 파일도 보존했다. Unity에서는 반복·밉맵·최대 1024·텍스처 압축 설정으로 가져온다. 원본 PNG를 별도 이미지 편집 도구로 고치지 않았다.
- 바닥 타일에 적용하고 테마별 재질 색으로 차이를 준다. 이 이미지가 충돌·통행 영역을 결정하지 않는다.
- 크기·SHA-256·출처는 `Artifacts/Validation/rift-stone.json`에 기록했다.

## 최종 생성 프롬프트

```text
Use case: stylized-concept. Asset type: seamless square albedo texture for the walkable floor of HELLSCRIPT, a dark fantasy Unity dungeon crawler. Create a 1024x1024 full-bleed opaque PNG texture, viewed perfectly straight down with orthographic projection. A continuous surface of ancient charcoal-gray and muted blue-gray basalt paving blocks, broad irregular rectangular stones, worn edges, subtle fine cracks, faint pale mineral wear, restrained painterly realistic game material detail. Flat diffuse neutral lighting evenly across the whole image; low contrast so small characters and red danger circles remain readable. The texture must tile seamlessly on all four edges, including matching stone joints and surface colors; uniform visual density, no focal emblem. Fill every pixel with stone and recessed narrow mortar joints. No characters, props, buildings, skulls, coins, treasure, plants, borders, frames, vignette, perspective, cast shadows, specular highlights, letters, runes, text, watermarks, UI, grid lines, or transparent areas. This is a flat material swatch, not a scene or rendered floor plane.
```

