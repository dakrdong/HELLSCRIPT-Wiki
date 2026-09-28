# 전투 탈출과 절전 모드 버튼

작성일: 2026-09-28 · [English](Battle_Escape.en.md)

전투 화면 위쪽에 절전 모드와 탈출 아이콘을 나란히 표시한다. 버튼 아래에는 기능 이름을 한국어·영어로 표시하며, 창 크기에 따라 지도와 상단 상태 표시를 피해서 배치한다. 절전 모드는 기존 자동 사냥 진행을 유지한다.

탈출을 누르면 공통 `ContentWindowView` 확인창이 전투를 일시정지한다. ‘아니요’는 전투를 재개하고, ‘예’는 기존 `GameController.ReturnTown` 경로로 전투를 종료한 뒤 마을로 돌아간다. 처음 진행하는 필수 튜토리얼에서는 탈출 버튼을 표시하지 않는다.

## 검증

통합한 `main` 코드의 macOS 개발 빌드에서 확인했다. 440×956, 956×440, 1600×900, 1600×1000, 2100×900 해상도와 한국어·영어, 글자 크기를 조합한 24개 화면에서 버튼·설명·주변 UI가 겹치지 않고 안전 영역 안에 표시됐다. 실제 EventSystem 포인터 경로로 절전 모드 진입, 탈출 확인창의 일시정지, 취소 후 재개, 마을 복귀를 검사했다. [결과](CompletedIntegrationEvidence/battle-escape.txt)

![가로 전투 화면](CompletedIntegrationEvidence/battle-hud-landscape.png)

![영어·글자 크기 150%의 탈출 확인창](CompletedIntegrationEvidence/escape-question-en-150.png)

## 리소스와 검증 범위

두 아이콘은 원본 PNG와 알파 채널을 유지한다. 호출 도구에서 생성 모델을 확인할 수 없으므로 `candidate_model_unknown`, `productionApproved: false` 상태를 유지한다. [제작 기록](../Art/HudButtons/hud-buttons-manifest.json)

입력 검사는 macOS의 합성 포인터로 수행했다. 모바일 실기기 조작이나 전력 사용량 검증을 의미하지 않는다.
