# 스킬·장비 이미지 제작 인계

사용자는 스킬, 전설 장비, 세트 장비, 위상을 종류별 작업으로 나누어 병렬 제작하도록 요청했다. 이 폴더는 각 작업이 참조하는 원본 ID·이름·효과의 고정 사본이다. 사용 옵션은 능력 구현 작업에서 계속 검증한다. 마력 회수(M13)와 도약 내려찍기(W02)는 사용자의 최신 콘셉트를 반영했다. 수치 조정은 아이콘의 동작 콘셉트를 바꾸지 않는다.

| 종류 | 제작 범위 | 파일 |
| --- | --- | --- |
| 스킬 | 108개 ID의 아이콘. 기존 적합한 리소스는 검수 후 재사용 가능 | skills.json |
| 전설 장비 | 기존 확장 123종과 신규 18종의 장비 실물 아이콘 | legendaries.json |
| 세트 장비 | 신규 24세트의 개별 장비 105개와 세트 문양 24개 | sets.json |
| 위상 | 전설 장비와 연결되는 고유 효과 문양 141개 | aspects.json |

위상은 효과를 나타내는 이미지 분류다. 별도 획득·추출·장착 시스템을 이 작업에서 새로 만들지 않는다. 세트 효과 60단계는 해당 세트 문양을 함께 사용하며 단계 숫자는 이미지에 넣지 않는다.

공통 규격은 정사각형 1024px PNG 원본, 중앙의 큰 실루엣, 충분한 여백, 글자·숫자·테두리 없는 다크 고딕 회화풍이다. 기존 `EquipmentAtlas.png`와 `Art/GlobalHUD` 아이콘을 직접 확인해 질감과 명암을 맞춘다. 부위와 동작은 실루엣으로 구분하고, 효과 색만 바꾼 복제품을 만들지 않는다. 투명 원본은 네이티브 알파로 생성하고 실제 알파·가장자리·64px 가독성을 검수한다. 크로마키나 사후 배경 제거로 대체하지 않는다.

이미지 생성은 설치된 imagegen 스킬의 내장 생성 경로를 우선 사용한다. 모델 정보는 실제 반환 근거만 기록한다. gpt-image-2 사용을 확인할 수 없으면 그 한계를 밝히고 제작 후보 상태로 둔다. 게임용 확정 자산으로 허위 표시하지 않는다. 독립된 항목은 개별 생성 요청으로 제작한다.

각 작업은 별도 작업 공간의 자기 이미지 폴더와 제작 기록만 소유한다. 원본 폴더, 전투 데이터, UI 배치·프리팹·씬, 다른 작업의 리소스를 변경하지 않는다. 원본 ID별 파일 경로, 효과 요약, 프롬프트, 생성 근거, 검수 결과, 재사용 여부와 미완료 사유를 manifest에 남긴다. 한국어·영어 기록과 검증 후 커밋·푸시하되 main 병합과 공개 위키 배포는 수행하지 않는다.

## English

The user requested four parallel tasks grouped by asset type. The JSON files freeze IDs, names and effects for handoff. Skill-use policies are still being verified in the ability task. W02 and M13 include the user's latest visual concepts. Balance changes do not change those concepts.

Coverage is 108 skill IDs, 141 physical legendary item icons, 105 pieces and 24 emblems for the new sets, and 141 legendary-power emblems. Verified existing skill art may be reused. Power emblems do not introduce an extraction or equipment system. The 60 set thresholds reuse their set emblem; numbers are supplied by the eventual UI.

Create 1024px square PNG masters with a large central silhouette and clean padding, painterly dark gothic materials, no text, numbers or frame. Inspect the existing EquipmentAtlas and GlobalHUD icons for style. Distinguish actions and equipment shapes beyond color changes. Generate native transparency, verify actual alpha, edges and readability at 64px; do not substitute chroma keys or background removal.

Use the installed imagegen skill's built-in generation path and one request per distinct asset. Record only verifiable model provenance. If gpt-image-2 cannot be verified, report that limitation and mark the art as a candidate, not an approved production asset.

Each task owns only its asset folder and production records in an isolated workspace. Do not edit the source workspace, combat data, UI, scenes, prefabs or another task's files. Deliver an ID-to-file manifest with bilingual effects, prompts, provenance, QA, reuse and outstanding issues. Verify, commit and push the task branch. Do not merge main or publish the public wiki.
