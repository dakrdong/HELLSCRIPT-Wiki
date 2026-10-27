# 인수 후보의 레코딩 전투 포함 확인 (2026-10-24)

[English](Combat_Recording_Integration_20261024.en.md)

사용자가 레코딩 전투도 main 통합 범위에 포함하도록 요청했다. 새 구현을 추정하지 않고 실제 소스·Git·공식 PR·지원되는 작업 목록을 대조했다.

- 소유 작업: Codex **전투 진행 레코딩 방식 도입**, `01a16cab-1ba5-7dd2-b58c-c15b4b1896c9`, 로컬 `/Users/t8g-2410-pn-005/.codex/worktrees/combat-recording/HELLSCRIPT`. 확인 시 idle, 작업 폴더 clean이며 추가 미커밋 변경이 없었다. 소유 작업 대화의 푸시 기록은 이번 인수 후보의 새 전송 승인으로 해석하지 않는다.
- 브랜치 `codex/combat-recording`, head `9bb20f75b0d5ad6acb69c96c4d019b97ff57968f`. [PR #186](https://github.com/dakrdong/HELLSCRIPT/pull/186)이 14:38:17 UTC에 main `1f1887d1148d99e5b5f42f9a899d8945c6209259`로 병합됐다. main의 두 부모는 `ad3bbdff`와 `9bb20f75`이며, 병합 트리와 레코딩 head의 트리는 동일하다.
- 기존 인수 후보 `a33d448a15ad315e28e6a4a0384caed53f3a0752`는 이미 그 main을 포함한다. 중복 cherry-pick·merge나 구형 전투 코드 재적용은 하지 않았다. 실제 기록 계약은 [기존 구현 문서](CombatRecording20261024.md)를 따른다.

## 결합 영향

레코딩 변경과 인수 후보가 함께 바꾼 파일은 번역, `CombatSimulation.ClassSkillAuto.cs`, `CombatSimulation.cs`, `GameUI.cs`, `RiftVisibilityTests.cs`다. 전투 쪽 추가 차이는 승인된 무리 중심 이동·목표 원점이며, 컨트롤러를 과거의 직접 틱 경로로 되돌린 변경은 없다.

기록기 두 파일, 기록 시뮬레이션, 프레임 정산, 컨트롤러, 룬 저장, 저널의 7개 핵심 소스는 레코딩 head와 바이트·SHA가 같다. HUD의 전체 행동 문구는 `Playback.Ready` 전 준비 상태를 유지한다. 표시 코드는 실제 재생된 상태를 읽으며 선계산 보상을 표시·지급하는 새 경로를 만들지 않았다.

E의 새 `tacticsVersion/tactics`와 무리 이동의 `packWalkAt/packWalkDistance`는 기존 필드 복사의 리플렉션 fallback으로 포착된다. 희소 차이와 전체 교체 두 사례를 추가해 Off 값·미지원 유니코드/말미 공백 값 및 두 이동 필드가 실제 기록 차이 적용 후 그대로 남는지 확인했다. E의 플레이어 진입 OFF·전투 미연동 계약은 유지한다. 실제 저장·보상은 기존 재생 시점 거래를 사용하며 미래 테이프를 사용자 세이브로 이식하지 않는다.

## 실제 검증

현재 후보에서 레코딩·상태 복원·E 전술·캐주얼 HUD·결과 팝업·무리 중심 수업 직접 영향 범위를 실행해 **123/123 통과, 실패·건너뜀 0**을 확인했다(19:50:29–19:52:41 UTC, Unity6000.6.0f1). 준비/재생 규칙 일치, 입력 변경 후 재계산, 체크포인트·보상·저장 실패 재시도도 포함한다. 새 사례는 2개이며 제품 런타임 수정은 없다.

로컬 근거: `Builds/Validation/UserReports20261024/takeover/recording-inclusion.json`, `recording-compatibility/results.xml`. 기존 9단계 로컬 CI는 입력이 바뀌지 않아 재사용한다. 이번 Unity 실행에서 모든 어셈블리가 다시 컴파일됐다. 임시 검사 식별자와 자체 import 9파일은 해시와 함께 보관한 뒤 복원했다. 원래 레코딩 작업 폴더·사용자 테스트본·실제 저장은 수정하지 않았다.

전체 EditMode, 새 성능 측정, 실제 렌더/포인터, 모바일·IL2CPP·빌드 근거는 아니다. 기존 문서의 측정값을 현재 후보에서 새로 측정한 값으로 주장하지 않는다. 실제 CLI 리뷰 결과와 이 기록 이후의 고정 head는 로컬 orchestration/CODEX_CHECK_20261024.json에 기록한다. GitHub 전송·J 가져오기 거절 상태는 이 요청으로 해제되지 않았다.
