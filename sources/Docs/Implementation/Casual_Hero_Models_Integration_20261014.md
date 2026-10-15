# 캐주얼 영웅 모델 통합 — 2026-10-14

[English](Casual_Hero_Models_Integration_20261014.en.md)

사용자가 완료를 확인한 `claude/casual-hero-models`의 `616c88b24c0f7cac49cd7e12c7d9408b6f2886ce`를 local main에 통합한다. 직업별 READY 3종(HW02 전사·HW17 궁수·HW82 마법사), LOD별 FBX 9개·512×512 PNG 36개를 그대로 사용한다. 원본 생성·재리깅·추가 결제는 없다. 기존 6슬롯·성별 2D 아트·세이브 계약은 유지하며, 3D는 기존처럼 남녀가 직업별 모델을 공유한다.

기존 무기 소켓이 없는 READY 골격의 연결을 보완한다. 활은 기존 계약대로 왼손에, 검·지팡이는 오른손에, 방패는 왼손에 붙인다. 화살통은 Chest 부모를 유지하며 READY recipe의 backBone 위치에 놓아 몸 뒤에 두고 Cape의 천 흔들림은 상속하지 않는다. 실제 WorldView 모델 구성과 5개 소품 연결을 검증하는 회귀 검사를 기존 WorldArtTests에 추가했다.

자산 144파일은 meta 누락·GUID 충돌·프리팹 참조 누락 0, FBX 9개는 recipe의 해시와 일치한다. provenance의 recipe_sha256는 설치 전 HWxx art 키를 가진 원본 recipe 해시이며, 설치 후 Hero_* 키로 바뀐 recipe 해시와 구분한다. 원본 provenance는 보존했다.

최종 통합 검사 결과와 실행·미실행 범위는 task-23의 validation/cleanup-20261014 및 정리 인계 영수증에서 기록한다. 기존 UI 검증·네이티브 앱은 이전 소스의 근거이며 새 모델의 플레이어 검증 결과로 재분류하지 않는다.

## 통합 결과

모델 병합 `831f3424`와 이미 구현이 반영된 훈련·상점 이력 보존 병합 뒤 검증 기준은 `16c75fa7938700fbd4ec419e5f7755876220287e`다. Runtime·Editor·Tests C# 컴파일 오류 0, 기존 경고 수는 88·28·35다. UI 계약 15개와 갱신 16개 검사도 통과했다. 최종 main 무음 EditMode는 새 소품 연결 5사례·기존 모델 소켓 1사례·제단 원화 7사례를 합쳐 **13/13 통과**, 실패·건너뜀 0이다(검사 5.625초, 전체 프로세스 75.013초). 실행 후 자동 meta 공백 465개와 IDE 설정은 패치를 먼저 보존하고 원래 main 바이트로 복원했다. 새 모델에 대한 전체 스위트·네이티브 플레이어·실기기 검증은 이번 통합에서 실행하지 않았다. 원 담당의 별도 86/86 근거는 `616c88b2` 소스로 구분한다.

사용자가 완료 작업 통합 후 정리를 요청하여 sparse 작업 사본 15개와 통합된 이전 HUD worktree 1개를 macOS 정상 휴지통으로 이동했다. 논리 6,078,038,016바이트이며 `.git`, stash, 미커밋 잔여, ignored 증거를 모두 보존했다. linked HUD의 Git admin/index/log 2,329,721바이트는 별도로 보존한 뒤 worktree 등록만 prune했다. main 조상·미사용 로컬 브랜치 12개와 원격 main에 병합된 `claude/casual-bgm`, `claude/monsters-casual-prompts`만 정리했다. 영구 삭제·stash 삭제·object/reflog purge·main push·CI 실행은 없다.

사용 중 프로세스의 cwd가 남은 캐릭터·튜토리얼 폴더, dirty 원본·living-title, 완료 소유자 확인이 안 된 field/monster, 미완료 hero-rig, 열린 PR130 자료는 보존했다. 원격 `codex/**` 삭제는 telemetry Actions 조건에 걸릴 가능성이 있어 CI 금지 범위에 따라 보류했다. 위키 check는 sparse 환경에 기존 생성 파일 `Wiki/site/data.json`이 없어 실패했으며 위키 생성·게시를 추가하지 않았다. 휴지통 이동은 디스크 회수로 계산하지 않는다. 정확한 경로·SHA·복원 방법·최종 여유 공간은 task-23 `validation/cleanup-20261014/final-handoff.json`에 기록한다.

## 후속 문서·중복 폴더 정리

`8172639e`의 완료 문서 2개와 캡처 4개를 `7272f126`으로 통합했다. 캡처 바이트를 보존하고 활 왼손·남녀 6슬롯/직업별 공유 3D 설명을 현재 main에 맞췄다. 기존 게임 소스·에셋·설정은 동일하므로 13/13 검사 입력을 그대로 유지하며 Unity·빌드·전수 검사는 추가하지 않았다.

담당자의 PR128/main/wiki 완료·idle과 원본 보존을 확인한 `casual-field-kit`, 전체 192경로가 main과 같고 사용이 끝난 `wt-casual-art`의 로컬 폴더를 정상 휴지통으로 추가 이동했다. 논리 9,373,985,022바이트, Git admin/index/log 복구 자료 4,471,862바이트를 보존하고 등록·로컬 브랜치 2개만 정리했다. 이전 단계를 합쳐 폴더 18개·로컬 브랜치 14개·원격 브랜치 2개 정리이며, PR130과 원격 art-concept는 그대로다.

모델·몬스터 담당은 실제 현행 Unity 검사/busy 상태라 폴더를 보존한다. 은퇴 튜토리얼 작업은 idle이지만 Claude·터미널 cwd가 남아 있어 사용 종료 미확정이다. living-title은 미반영 TitleArtImporter/프롬프트와 사용자 변경이 있고 담당 durable 스레드 연결이 unavailable이라 완료·동결을 확인할 수 없다. 취소된 hero-rig 생성은 재개하지 않는다. 모든 원본·미커밋과 원격 main/CI 제한은 유지하며, 정확한 후속 복구·보류 사유는 task-23 `validation/cleanup-20261014/followup/final-handoff.json`에 남긴다.
