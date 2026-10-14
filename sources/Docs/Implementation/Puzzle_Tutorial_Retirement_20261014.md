# 퍼즐 튜토리얼 은퇴

갱신일: 2026-10-14

[English](Puzzle_Tutorial_Retirement_20261014.en.md)

2026-10-14 사용자 결정으로 퍼즐 레벨 튜토리얼(15레벨과 졸업 시험, 대기실, 운영·정비 퍼즐, 복습)을 은퇴시키고 코드·테스트·빌드 포함 에셋을 삭제했다. 새 계정은 이미 [안내 튜토리얼 스테이지](Guide_Tutorial_Script_20261011.md)로만 들어간다. 사용자 요청에 따라 튜토리얼 복습(다시보기) 기능도 흔적 없이 지웠다. 과거 구현 문서는 당시 기록으로 남기고 맨 위에 은퇴 표시를 달았다.

## 삭제한 것

- **런타임 16개 파일:** `PuzzleTutorial*.cs`, `GameStore.Puzzle*.cs`, `CombatSimulation.Puzzle*.cs`, `GameUI.Puzzle*.cs`, `PuzzleHubLayout`, `PuzzleHubWindow`, `PuzzleMaintenanceWindow`, `RuntimeTutorialSmoke.Puzzle.cs`, 그리고 퍼즐 2레벨부터 쓰던 실시간 피격 그래프 `GameUI.LiveIncoming.cs`.
- **테스트:** `Puzzle*Tests` 7개(84개 케이스), 퍼즐 1·2레벨을 검사하던 `TutorialPitTests`(4개), D4 퍼즐 키트 검사 2개.
- **에셋:** `Resources/PuzzleTutorial.json`, `Resources/Art/PuzzleHub/`(13MB), `PuzzleHubTone.shader`, `Editor/PuzzleHubArtImporter.cs`, 텍스처 예산 2줄, 타이틀 분위기 셰이더의 대기실 분기와 톤 조절.
- **복습:** `RunState.tutorialReplay`, 일지 P00의 '실습'(퍼즐 1레벨 복습), 일지 '완료' 탭의 레벨 복습 목록, 피트 다시보기 문구와 스모크.
- **퍼즐만 쓰던 공용 부품:** `UiSafeArea.Reserve`/`Release`/`Unreserved`, `ContentWindowView`의 `fullArea`, `CombatStatistics.TopIncomingLive`, `Economy.AddItem`의 시험 가방 인자, `CombatPreviewSession`의 퍼즐 비교, 성능 프로파일 `hub` 모드(`tools/profile_native_cpu.py` 포함).
- **번역:** `en.txt`에서 삭제 코드와 퍼즐 JSON에서만 쓰던 줄 246개.
- **스모크:** 퍼즐 15레벨 수락 스모크, 튜토리얼 스모크의 퍼즐 분기, 이미 동작하지 않던 피트 복습, 첫 플레이 수락의 복습 프롬프트 캡처. 튜토리얼 스모크는 이제 저장된 피트 체크포인트가 있어야 하며 새 계정 흐름은 `RuntimeGuideTutorialSmoke`가 맡는다.

## 옮기거나 바꾼 것

- 안내 스테이지가 쓰던 `OwnedRun`·`StagedRun`·`AdoptPuzzleRun`은 `GameStore.cs`의 `OwnedRun`·`StagedRun`·`AdoptTutorialRun`으로 옮겼다.
- 전투 HUD 시계는 `ElapsedTime` 대신 `State.time`을 읽는다.
- 일지 P00과 장 C00의 이름·설명을 '칙령의 길'(안내 스테이지)로 바꿨다. P00의 실습 버튼은 비활성이다.
- `SkillTreeD4Loadout.Kit`은 프롤로그 키트만 만드는 `PrologueKit`이 됐다. `ReleaseKit`은 키트 표시를 먼저 지운 뒤 할당을 초기화한다.
- 보존: `Docs/Art/PuzzleHub`, `Prototypes/PuzzleHub`, `Artifacts/PuzzleTutorial` 증거, 퍼즐 기록 문서.

## 저장 호환

- 저장 키와 파일 이름은 바꾸지 않았다. `AccountGuide.puzzle`, `TutorialSlot.puzzle`, `RunState`의 퍼즐 필드와 `tutorialReplay`는 코드에서 지웠다. JsonUtility는 모르는 필드를 무시하므로 옛 세이브는 그대로 읽히고, 다음 저장 때 해당 필드만 빠진다.
- 퍼즐 도중 멈춘 영웅에게 남은 D4 스킬 키트(`kitKind="puzzle"`)는 불러올 때 풀고 스킬 포인트를 돌려준다(`Tutorials.Normalize`). 그대로 두면 키트 소유자 검사가 "원본 보존 후 불러오기 중단"으로 막는다.
- 퍼즐 진행 체크포인트(흐름 5)는 다른 흐름의 체크포인트처럼 버린다. 그 영웅이 전사면 다음 시작에서 안내 스테이지로, 다른 직업이면 기존 면제 경로로 마을에 들어간다.
- 저장된 것(레벨·경험치·장비·골드·칙령 해금·콘텐츠 해금)은 그대로다. 퍼즐 진행에서 계산만 되던 것(13레벨 클리어로 열린 반복 사냥 설정, '시험 두루마리' 쪽수와 레벨 복습)은 사라진다. 사용자가 승인했다.
- 검사: `SkillTreeD4TutorialKitTests.RetiredPuzzleSavesStillLoad`가 보관된 실제 L8 이후 퍼즐 세이브 3종을 실제 읽기 경로로 불러온다. 이어서 퍼즐 키트를 넣은 세이브에서 키트 해제·포인트 반환을 확인하고, 전사는 안내 스테이지 시작, 다른 직업은 면제까지 확인한다.

## 검증

검증 결과는 PR에 기록한다.
