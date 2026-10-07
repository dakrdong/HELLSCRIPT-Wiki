# 이동기 5레벨 해금 구현

갱신일: 2026-10-06 · [English](Escape_Skill_Unlock_Level.en.md)

퍼즐 튜토리얼 계획의 A1 단계다. 궁수 후퇴 도약(A04)과 마법사 순간이동(M04)은 4레벨에 잠겨 있고 5레벨에 열린다. 전사 도약 내려찍기(W02)는 1레벨을 유지한다. 이 단계만으로 15레벨 퍼즐 튜토리얼이 구현된 것은 아니다.

## 소유 코드와 생성물

`Docs/Design/ClassSkills/catalog.json`·`GameCatalog.cs`·`legacy_definitions.json`·`tree-design.cjs`의 해금을 함께 맞춘다. 직업별 문서·`ClassSkills.json`·HTML 트리 카탈로그·`ClassSkillTree.json`은 기존 생성기로 다시 만든다. 두 트리 루트를 첫 밴드로 옮기며 후속 스킬의 해금과 선행 관계는 유지한다. Unity 에셋은 `ProjectBuilder.SyncSkillUnlocks`로 실제 에디터 직렬화를 거쳐 저장한다. `.meta`와 에셋 참조를 유지한다.

저장된 포인트·장착·칙령은 변경하지 않는다. F06의 5레벨 후보 마지막에 이동기가 붙으며 추천 순서는 A02/M02를 유지한다. 튜토리얼 중 장착 이력이 F06을 소진하므로 퍼즐 이동기 학습은 후속 튜토리얼 단계가 직접 안내한다. 과거 기록의 10레벨 검증 결과는 당시 근거로 남긴다.

## 검증

추가 경계 검사 5개는 변경 전 1 통과·4 실패·0 건너뜀으로 Lv5 미해금을 확인했다. Unity 6000.6.0f1 Edit Mode의 `GrowthTests`·`PlayerTrainingTests`·`SkillTreeIntegrationTests`·`TutorialProgressionTests`는 **187 통과·0 실패·0 건너뜀**이다. HTML 트리 엔진 **42 통과**, 공통 UI 검사 도구 테스트 **11 통과**이며 스킬 데이터·문서·옵션·보고서 생성기와 네이티브 트리 검사가 통과했다.

생성기 검사는 `class_skill_runtime.py`·`class_skills.py`·`class_skill_options.py`·`class_skill_reports.py`의 `check`, `Prototypes/SkillTree/build.cjs`, `build_skill_tree.cjs --check`, `engine.test.cjs`다. 생성기를 재실행해 추가 변경이 없는지 확인한다. 공통 UI 계약 검사도 실행한다.

전체 Edit Mode·macOS 게임 빌드·런타임 스모크·브라우저 게임·모바일 실기기는 이 단계에서 실행하지 않는다. 데이터·해금 변경의 집중 검사이며 실제 초반 균열 밸런스를 전수 검증한 결과는 아니다.

## 전달과 산출물

브랜치 `codex/escape-skill-lv5`에서 PR까지만 전달한다. PR 병합과 공개 위키 배포는 이 실행에서 하지 않는다. 병합 담당자가 통합된 main에서 공개 배포를 마친다.

작업 근거는 `Artifacts/PuzzleTutorial/20261006/`에 모으고 `artifact-lifecycle.json`에 정확한 경로·크기·보존·정리 계획을 기록한다. 기존 위키 근거는 필요한 파일만 읽기 전용으로 복사하며 원본을 보존한다. 열려 있는 PR 검토에 필요한 작업 폴더와 검사 결과는 보존하고 2026-10-13에 재검토한다. 영구 삭제는 승인되지 않았다.

[집중 검사 XML](../../Artifacts/PuzzleTutorial/20261006/stage1-focused.xml) · [변경 전 경계 검사](../../Artifacts/PuzzleTutorial/20261006/stage1-red.xml) · [검증 요약](../../Artifacts/PuzzleTutorial/20261006/stage1-validation.json)
