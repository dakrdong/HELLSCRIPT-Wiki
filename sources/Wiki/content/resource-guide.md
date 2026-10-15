# 리소스 제작·관리 기준 / Resource management

정리 기준일: 2026-10-15. 리소스 DB는 현재 게임에 등록된 파일과 연결 근거를 수집합니다. 등록·아트 검수·실제 화면 적용·출시 승인은 각각 별도 상태입니다. / Updated 2026-10-15. The resource database collects current registered game files and their evidence. Registration, art review, runtime acceptance and release approval are separate states.

## 현재 수집 범위 / Current collection

| 분류 / Category | 수집과 연결 / Collection and binding |
| --- | --- |
| 승인 장비 / Canonical equipment | 승인 receipt의 359개 그림과 콘텐츠 ID를 대조합니다. 공통 장비 조회는 이 그림을 기본으로 사용하고 B28→B32 공유를 유지합니다. / Match the 359-art receipt to catalog IDs; shared lookup uses canonical art and keeps the B28→B32 alias. |
| 캐주얼 화면 / Casual screens | Resources/Art의 현재 PNG, 기본 HUD Casual 60개, Title/Vista, CharacterPlaza와 각 콘텐츠 창의 실제 등록 그림을 수집합니다. / Collect current PNGs in Resources/Art, including 60 Casual HUD assets, Title/Vista, CharacterPlaza and content-window assets. |
| 보석·제조 영약 / Gems and elixirs | 공통 조회와 DB 미리보기는 JewelerWindow의 새 그림 72개를 사용합니다. / Shared lookup and previews use the 72 new JewelerWindow images. |
| 모델·필드 / Models and fields | HunyuanReady의 실제 prefab/recipe와 6개 캐주얼 필드 manifest를 연결합니다. 설치 상태를 실제 실행 통과로 표현하지 않습니다. / Link installed HunyuanReady prefab/recipes and six casual-field manifests without claiming fresh runtime acceptance. |
| 글꼴·보조 UI / Fonts and auxiliary UI | CasualSkill 번들 글꼴, DailyQuestCasual과 PotionQuickCasual의 등록 파일도 수집합니다. / Include bundled CasualSkill fonts and registered DailyQuestCasual/PotionQuickCasual assets. |
| 런타임 대체 표현 / Runtime fallbacks | 아직 코드나 기존 그림을 사용하는 표현은 해당 소스와 현재 적용 범위를 기록합니다. / Retain source and scoped usage for remaining code or older-art fallbacks. |

파일별 실제 크기·모드·PNG SHA-256·Unity GUID와 원본 경로는 DB에서 확인합니다. 같은 원본을 공유하는 콘텐츠나 별칭을 고유 신규 그림 수로 중복 계산하지 않습니다. `row.image.file`은 미리보기 경로이며 실제 원본은 각 항목의 source/assetPath에 연결됩니다. / Inspect actual dimensions, mode, PNG hash, Unity GUID and source path in each record. Shared art and aliases are not additional unique masters. `row.image.file` identifies the preview; source/assetPath identifies its original.

## 이전 그림과 문서 이력 / Retired art and document history

구 장비335·보석36·제조 영약36의 PNG/meta 407쌍은 새 대체본의 ID·해시·GUID를 확인하고 현재 게임 Resources에서 복구 가능한 범위로 분리했습니다. 현재 DB와 미리보기는 제거한 경로를 사용하지 않습니다. 정확한 목록과 검증은 [retirement manifest](../../tools/casual_resource_retirement_20261015.json), [통합 인계](../../Docs/Implementation/Casual_UI_Integration_20261014.md), [English handoff](../../Docs/Implementation/Casual_UI_Integration_20261014.en.md)에 있습니다. / The 407 old equipment/gem/elixir image/meta pairs are recoverably separated after exact replacement verification; current database rows and previews no longer use them.

과거 문서가 실제로 참조하는 첨부는 [exact Git-object allowlist](../../tools/wiki_retired_art_sources.json)에 따라 보존된 이력에서 읽어 내보냅니다. 이 첨부를 현재 게임 리소스로 다시 등록하거나 게임 폴더에 원화 사본을 만들지 않습니다. Git 객체가 없는 불완전 이력에서는 생성기가 명시적으로 실패하므로 해당 원본 이력을 먼저 복구해야 합니다. / Historical attachments export only from exact preserved Git objects on the allowlist. They are not current resources or new game-source copies. Missing historical objects fail generation explicitly.

## 남은 캐주얼 교체 / Remaining casual replacement

사용자는 새 대체본이 없는 그림도 제작해 이전 컨셉을 모두 교체하도록 승인했습니다. 기존 승인 원화는 먼저 재사용하고, 실제 부족한 스킬·공통 UI 그림만 새로 만듭니다. 스킬, 공통 UI, 인물·아이템 담당은 별도 출력에서 작업하며 게임 소스·Unity·최종 통합은 한 담당이 소유합니다. / The user authorized replacement of the remaining previous-concept artwork, including new art where needed. Reuse approved art first. Separate skill, shared-UI and portrait/item producers deliver outputs to one game/Unity integration owner.

직업·성별·방향·아이템 ID·별칭·저장 키와 기존 화면 기능은 유지합니다. 승인된 인물·위상·상자 원화가 있다는 사실만으로 런타임 연결까지 완료됐다고 표시하지 않습니다. 모델 리깅과 진행 중 몬스터 작업도 별도 소유 범위로 보호합니다. / Preserve class, gender, facing, item IDs, aliases, saves and screen behavior. Existing approved portraits/aspects/boxes are not automatically runtime-integrated. Protect active model-rigging and monster work.

## 추가·교체 검증 / Addition and replacement checks

1. 실제 소비 화면과 콘텐츠 ID·상태·공유 관계를 확인합니다. / Identify the real consumer, ID, states and aliases.
2. 최신 승인 원본·정확한 SHA·출처·프롬프트를 먼저 확인합니다. / Verify the latest approved original, hash, provenance and prompt.
3. 같은 경로의 교체는 meta/GUID를 유지하고 가져오기 예산을 확인합니다. / Preserve meta/GUID for same-path replacement and check import budgets.
4. alpha·잘림·작은 크기·상태 대비와 실제 게임 대표를 검증한 뒤 같은 유형을 확대합니다. / Verify alpha, containment, small-size readability, state contrast and one actual-game representative before expansion.
5. 최종 통합 입력에서 전체 게임 검사와 한 개 네이티브 빌드를 공유하고 위키 build/check·검색·링크·이미지 검사를 한 번 수행합니다. / Share final game tests and one native build, then run integrated wiki build/check, search, link and image validation once.

영구 삭제·공개 게시·원격 push의 승인 범위와 실제 실행 결과는 별도로 기록합니다. 휴지통 이동이나 폴더 표시 용량을 실제 공간 회수로 보고하지 않습니다. / Record permanent deletion, publication and push authorization/results separately. A Trash move or logical folder size is not reclaimed-space proof.
