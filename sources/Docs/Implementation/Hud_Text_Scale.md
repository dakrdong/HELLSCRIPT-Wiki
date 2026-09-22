# 하단 HUD·마을 이름표의 비례 축소와 영문 이름 확인

확인일: 2026-09-22

[English](Hud_Text_Scale.en.md) · [NPC 이름 원본 기록](Town_Npc_Names.md)

## 수정한 동작

창을 줄인 직후에는 글자가 작아지지만 다음 HUD 갱신 때 다시 커지는 문제를 수정했습니다. 원인은 글꼴에만 적용되던 고정 픽셀 최솟값이었습니다. 이제 글자와 해당 줄의 높이를 HUD 배율로 함께 계산하여 작은 창에서도 같은 비율을 유지합니다. 정수 글꼴 크기로 인한 반 픽셀 이내의 반올림은 허용합니다.

마을의 시설명·NPC 이름표는 글자, 줄 간격, 외곽선과 표시 범위를 하나의 묶음으로 줄입니다. NPC 대화 카드도 같은 배율을 사용하고 하단 조작 영역 위에 배치합니다. 화면 상단 제목 영역은 실제 높이를 기준으로 피합니다. 기존 화면 크기 설정과 HUD의 최소 배율 0.4는 유지합니다.

| 화면 크기 | HUD 배율 | 하단 설명 글자 | 반복 갱신 후 |
|---|---:|---:|---|
| 1600×900 | 1.0 | 16px | 16px를 유지합니다. |
| 800×450 | 0.5 | 8px | 8px를 유지합니다. |
| 640×360 | 0.4 | 6px | 6px를 유지합니다. |
| 900×1600 | 1.0 | 16px | 16px를 유지합니다. |
| 450×800 | 0.5 | 8px | 8px를 유지합니다. |
| 360×640 | 0.4 | 6px | 6px를 유지합니다. |

이름표 너비는 기준 208에서 0.5배 화면의 104픽셀로, 대화 카드 너비는 250에서 125픽셀로 줄어듭니다. 한국어·영어, 가로·세로 전환, 창을 줄였다가 복원하는 순서와 화면 크기 설정 50%·150%를 확인했습니다.

## 영어로 표시되는 NPC 이름

기존 이름 등록을 유지하고, 영어 모드에서 각 NPC에게 이동하여 이름이 실제로 표시되는지 확인했습니다. 모든 이름표가 화면 안전 영역 안에 나타났습니다.

| 한국어 | 영어 |
|---|---|
| 마르크 쿠스 | Marc Kus |
| 제이크 보쿤 | Jake Bokun |
| 차도르 사마프 | Chador Samaf |
| 안톤 진다크 | Anton Jindark |
| 인젤 미르 | Injel Mir |
| 표냐 내르뭰 | Pyonya Nermwen |
| 쟝 죠린 | Jean Jorin |
| 달크 알뷔 | Darc Alvi |

## 화면과 검사 근거

[한국어 800×450: HUD와 이름표·대화 카드가 함께 축소된 모습](HudTextScaleEvidence/ko-800x450.png)

[영어 450×800: NPC 영문 이름과 비례 축소된 카드](HudTextScaleEvidence/en-450x800.png)

- Edit Mode 검사: HUD 11개, 현지화 33개, 마을 이동 19개로 총 **63개 통과, 실패·건너뜀 0개**입니다. [요약](HudTextScaleEvidence/editmode.json)과 [최종 XML](HudTextScaleEvidence/editmode-final.xml)을 보존합니다.
- macOS 개발 빌드: 오류 0개로 성공했습니다. [빌드 기록](HudTextScaleEvidence/build.json)을 보존합니다.
- 실제 게임 실행: UI 표본 30개를 검사했고, 각 화면에서 1.5초간 반복 갱신한 뒤 글자 크기가 같았습니다. 이어서 NPC 8명의 영문 이름이 표시되는 위치까지 각각 이동했습니다. [실행 결과](HudTextScaleEvidence/runtime.txt)와 [화면별 수치·방문 기록](HudTextScaleEvidence/geometry.txt)을 보존합니다.
- 실행 중인 원본 Unity를 중단하지 않기 위해 마지막 검사는 별도 복사본에서 수행했습니다. 현재 체크아웃의 다른 작업 변경도 포함된 파일 스냅샷이며, 이번 수정 파일은 [기준 해시](HudTextScaleEvidence/source-snapshot.json)와 일치합니다. 원본 에디터 전체 회귀 검사나 모바일 실기기 검증으로 해석하지 않습니다.

## 수정 위치

- [GlobalHudLayout.cs](../../Assets/HELLSCRIPT/Runtime/Presentation/GlobalHudLayout.cs): 글꼴과 줄 높이의 비례 계산을 담당합니다.
- [GameUI.Plaza.cs](../../Assets/HELLSCRIPT/Runtime/Presentation/GameUI.Plaza.cs): 마을 이름표·대화 카드의 배율과 화면 경계를 담당합니다.
- [GlobalHudTests.cs](../../Assets/HELLSCRIPT/Tests/Editor/GlobalHudTests.cs): 화면을 절반으로 줄였을 때의 비율과 줄 높이를 검사합니다.
- [RuntimeHudScaleSmoke.cs](../../Assets/HELLSCRIPT/Runtime/Presentation/RuntimeHudScaleSmoke.cs): 실제 HUD 갱신·창 크기 변경·영문 NPC 방문을 검증합니다. 개발 빌드의 전용 실행 인자가 있을 때만 동작하며 별도 저장 경로를 요구합니다.
