# 궁수 스킬 아이콘 제작 기록

작성일: 2026-09-22

궁수 스킬 36종을 ID별 독립 PNG로 제작했습니다. 일반 액티브 16종, 궁극기 2종, 패시브 18종이며, 모든 항목은 내장 이미지 생성 도구의 개별 요청으로 생성한 후보 자산입니다. 기존 아이콘을 복제하거나 색만 바꾸어 수량을 채우지 않았습니다.

## 제작 기준과 파일

효과와 이름은 같은 폴더의 `skills.json`을 기준으로 삼았습니다. 원본 프로젝트의 `EquipmentAtlas.png`, `GlobalHUD/status-mana.png`, `GlobalHUD/menu-character.png`, `GlobalHUD/status-heart.png`를 직접 확인해 금속·가죽의 질감과 명암을 참고했습니다. UI 테두리와 배경은 복제하지 않았습니다.

모든 원본은 실제 1254 × 1254px RGBA PNG입니다. 최초 요청에는 1024px가 명시되었으나, 인계 규격 개정에 따라 도구가 반환한 네이티브 1254px 원본을 허용합니다. 요청문은 생성 근거로 그대로 남겼습니다. 이미지 변환, 크로마키, 배경 제거, 알파 재생성은 수행하지 않았습니다. 중앙 76%는 여백 권고 기준이며, 실제 형태가 잘리거나 배경이 남는지 함께 검사했습니다.

- 게임 프로젝트의 PNG는 `Assets/HELLSCRIPT/Resources/Art/ClassSkillIcons/Ranger/{ID}.png`에 있습니다.
- 반환 원본의 보존 사본은 `raw/{ID}.png`에 있습니다. 프로젝트 PNG와 해시가 같습니다.
- `manifests/{ID}.json`에는 한영 이름·효과, 연결 장비 ID, 프롬프트, 파일 경로, 원본 해시, 생성 근거와 검수 기록이 있습니다.
- `prompts.json`에는 항목별 실제 요청문이 있습니다. 생성 모델은 도구가 공개하지 않아 모든 항목을 `model: unknown`, `approval: candidate`로 기록했습니다.

## 검수 결과

`validate_assets.py --class Ranger` 검사에서 PNG 36개와 원본 36개의 형식·알파·원본 일치·GUID 검사가 모두 통과했습니다. 누락 항목과 전역 오류는 없습니다. 실제 64px 축소본을 어두운 바탕과 밝은 바탕에서 확인했습니다. 같은 화살 계열은 관통선, 부채꼴, 연속 사격, 분기점, 낙하 방향으로 구분했습니다. 패시브는 동작 장면보다 지속 효과를 나타내는 정적인 상징을 중심으로 삼았습니다.

검사 경고는 모델 확인 불가 36건, 매우 낮은 알파의 테두리 픽셀 4건, 중심 실루엣 여백 권고 3건, 희미한 부분 여백 권고 3건입니다. 검수에서 불투명 배경이나 눈에 띄는 매트·후광, 캔버스 경계에 잘린 주요 형태는 발견하지 못했습니다. A01·A15·A18의 넓은 구성은 원본 그대로 보존했습니다. 자세한 수치는 `qa/validation.json`에 있습니다.

`qa/Ranger-64px-dark.png`와 `qa/Ranger-64px-light.png`는 비교 검수용 모음 이미지입니다. 모음 이미지의 글자와 바탕은 게임용 PNG에 포함되지 않습니다. Unity 메타데이터는 새 GUID로 생성했고 기존 메타데이터는 덮어쓰지 않았습니다. 이 기록은 이미지 파일 검증이며 Unity 실행, 게임 UI 연결, 실기기 검증을 뜻하지 않습니다.

## 항목 목록

| ID | 한국어 이름 | English name | 종류 |
| --- | --- | --- | --- |
| A01 | 관통 사격 | Piercing Shot | 액티브 |
| A02 | 다중 사격 | Multishot | 액티브 |
| A03 | 맹독 덫 | Venom Trap | 액티브 |
| A04 | 후퇴 도약 | Retreat Leap | 액티브 |
| A05 | 사냥꾼의 표식 | Hunter's Mark | 액티브 |
| A06 | 그림자 화살 | Shadow Arrow | 액티브 |
| A07 | 잇단 사격 | Successive Shots | 액티브 |
| A08 | 응시 사격 | Patient Shot | 액티브 |
| A09 | 가시 덫 | Briar Trap | 액티브 |
| A10 | 서리 올가미 | Frost Snare | 액티브 |
| A11 | 미끼 투영 | Decoy Projection | 액티브 |
| A12 | 연막 엄폐 | Smoke Cover | 액티브 |
| A13 | 독화살 | Venom Arrow | 액티브 |
| A14 | 갈라지는 화살 | Forking Arrow | 액티브 |
| A15 | 경계 쇠뇌 | Watch Ballista | 액티브 |
| A16 | 사냥 준비 | Hunt Preparation | 액티브 |
| A17 | 일제 소탕 | Killing Rain | 궁극기 |
| A18 | 그림자 추격 | Shadow Pursuit | 궁극기 |
| AP01 | 긴 사거리 | Long Reach | 패시브 |
| AP02 | 탈출의 발걸음 | Escaping Step | 패시브 |
| AP03 | 꿰뚫는 시선 | Piercing Gaze | 패시브 |
| AP04 | 덫 사냥꾼 | Trap Hunter | 패시브 |
| AP05 | 절약된 집중 | Saved focus | 패시브 |
| AP06 | 마무리 사격 | Finishing Shot | 패시브 |
| AP07 | 표식의 빈틈 | Marked Opening | 패시브 |
| AP08 | 사냥감의 발자취 | Quarry Tracks | 패시브 |
| AP09 | 다시 당기는 시위 | Drawing Again | 패시브 |
| AP10 | 독의 순환 | Venom Cycle | 패시브 |
| AP11 | 연금 숙련 | Alchemical Practice | 패시브 |
| AP12 | 준비된 도주 | Prepared Escape | 패시브 |
| AP13 | 집중 조준 | Focused Aim | 패시브 |
| AP14 | 유인 전술 | Decoy Tactics | 패시브 |
| AP15 | 독과 서리 | Venom and Frost | 패시브 |
| AP16 | 빈틈 없는 재장전 | Seamless Reload | 패시브 |
| AP17 | 길잡이 | Pathfinder | 패시브 |
| AP18 | 무음의 살수 | Silent Execution | 패시브 |
