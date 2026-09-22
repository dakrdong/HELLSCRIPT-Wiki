# HELLSCRIPT 스킬 아이콘 제작 기록

작성일: 2026-09-22

[English](Class_Skill_Icons.en.md)

전사·궁수·마법사 스킬 **108개**를 ID별 독립 PNG로 제작하고 검수했습니다. 일반 액티브 48개, 궁극기 6개, 패시브 54개입니다. 기존 아이콘을 재사용하거나 복제한 항목은 없습니다. 모든 이미지는 모델 정보가 확인되지 않은 **제작 후보**입니다.

| 직업 | 일반 액티브 | 궁극기 | 패시브 | 합계 |
| --- | ---: | ---: | ---: | ---: |
| 전사 | 16 | 2 | 18 | 36 |
| 궁수 | 16 | 2 | 18 | 36 |
| 마법사 | 16 | 2 | 18 | 36 |

## 원본 규격과 제작 근거

고정 인계 자료의 ID, 한영 이름, 효과, `visualConcept`, 태그와 장비 연계를 제작 기준으로 사용했습니다. 읽기 전용 원본은 별도 작업 공간의 `Docs/Art/SkillExpansionBrief`이며, [인계 설명 사본](handoff/Class_Skill_Icons_Handoff.md), [스킬 JSON](handoff/skills.json), [사본 해시](handoff/snapshot.json)를 보관했습니다. 기존 장비 아틀라스의 거친 금속·가죽 질감과 강한 명암을 참고했고, GlobalHUD 아이콘을 직접 열어 형태를 확인했습니다.

실제 원본은 모두 **1254×1254px 네이티브 RGBA PNG**입니다. 최초 목표인 1024px 대신 내장 도구가 반환한 1254px을 허용한 [후속 규격](handoff/Class_Skill_Icons_Spec_Revision_2.md)을 적용했습니다. 중앙 76%는 여백 지침으로 검토했습니다. 실제 잘림, 주요 형태 누락, 불투명 배경은 실패 기준입니다.

내장 `image_gen`에 항목별로 별도 요청했습니다. 반환 정보는 `image_url`과 `output_hint`이며 모델명은 제공되지 않았습니다. `gpt-image-2`는 선호 모델일 뿐 실제 사용을 확인할 근거가 없으므로, 모든 manifest는 `model: unknown`, `modelEvidence: null`, `approval: candidate`를 유지합니다. 외부 API/CLI로 전환하지 않았습니다.

PNG를 재조정하거나 배경을 제거하지 않았습니다. 크로마키, 알파 재생성, 색상 교체, SVG 대체, 기존 아이콘 복제를 사용하지 않았으며, 생성 원본과 게임 폴더 사본의 바이트를 동일하게 보존했습니다. 검수용 축소본에만 밝고 어두운 바탕과 ID를 붙였습니다.

## 항목별 인계

- [통합 manifest](manifest.json)에는 108개 ID의 경로, 한영 효과, 실제 프롬프트, 장비 연계, 해시, GUID, 모델 근거와 QA가 있습니다.
- [전체 진행 목록](Class_Skill_Icons_Progress.md)에서 각 ID의 상세 manifest를 열 수 있습니다.
- 게임 폴더는 `Assets/HELLSCRIPT/Resources/Art/ClassSkillIcons/{Warrior,Ranger,Mage}/{ID}.png`입니다. `Resources` 조회 키도 manifest에 기록했습니다.
- 직업별 제작 기록은 [전사](Warrior/Class_Skill_Icons_Warrior.md), [궁수](Ranger/Class_Skill_Icons_Ranger.md), [마법사](Mage/Class_Skill_Icons_Mage.md)에 있습니다.
- 각 직업의 `raw/`에는 내장 도구가 반환한 PNG를 보관했습니다. `manifests/`와 프롬프트 기록에는 실제 요청 근거를 남겼습니다.

W02 도약 내려찍기는 긴 궤적 끝의 착지 충격으로 제작했습니다. M13 마력 회수는 제자리에서 두 손 사이로 푸른 마력이 안쪽으로 모이는 형상입니다. M13의 최신 설명인 무쿨타임, 정지 중 기본 마나 회복 10배, 이동·공격 시 취소, 피격 시 유지를 기준으로 삼았습니다. 인계 JSON의 이전 `cooldownSeconds: 14`는 원본 근거 보존을 위해 수정하지 않았습니다.

## 검수 결과

[통합 검사 결과](qa/validation.json)는 **108개 통과, 누락 0개, 전역 오류 0개**입니다. 다음 내용을 확인했습니다.

- 실제 PNG 형식, 1254px 정사각형, RGBA와 완전 투명 픽셀을 확인했습니다.
- 생성 원본과 게임 사본 108쌍의 SHA-256이 일치합니다. ID 사이의 중복 이미지와 중복 GUID가 없습니다.
- 원본과 64px 축소본을 밝은 배경·어두운 배경에서 직접 검토해 주요 형태, 항목 구별, 잘림과 매트·헤일로 여부를 기록했습니다.
- 모델 미확인 108건, 낮은 알파의 경계 픽셀 56건, 중심 형태의 여백 지침 초과 72건, 옅은 효과의 여백 지침 초과 72건을 경고로 보존했습니다. 경고를 지우기 위해 원본을 가공하지 않았습니다.
- 새 PNG 메타데이터 108개와 폴더 메타데이터 4개에 고유 GUID를 부여했습니다. 단일 Sprite, 입력 알파 사용, Clamp, Mipmap 끔, 기본 최대 크기 2048 설정을 기록했습니다. 기존 에셋 GUID는 변경하지 않았습니다.

[전사 비교표](qa/Warrior-64px-dark.png), [궁수 비교표](qa/Ranger-64px-dark.png), [마법사 비교표](qa/Mage-64px-dark.png)는 실제 64px 검수용입니다. 같은 폴더의 `-light.png` 파일은 밝은 배경 비교표입니다. 이 이미지들은 게임용 아틀라스가 아닙니다.

재검사는 저장소 루트에서 `python3 -B Docs/Art/ClassSkillIcons/validate_assets.py --report Docs/Art/ClassSkillIcons/qa/validation.json`으로 실행합니다. 검사 통과 후 `assemble_manifest.py`가 통합 manifest와 진행 목록을 갱신합니다. 두 도구는 게임 원본 픽셀을 수정하지 않습니다.

## 확인 범위

이번 결과는 이미지 제작과 파일 검수입니다. UI 배치, 씬, 프리팹, 전투 코드, 아이콘 연결 코드는 변경하지 않았습니다. Unity 에디터의 실제 가져오기, 게임 런타임, 모바일 실기기는 검사하지 않았습니다. 표시 크기와 최종 임포트 선택은 후속 UI 통합 작업에서 결정합니다. 이 작업에서는 `main` 병합과 공개 위키 배포를 수행하지 않습니다.
