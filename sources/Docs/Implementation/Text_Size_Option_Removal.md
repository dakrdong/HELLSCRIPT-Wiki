# 글자 크기 조정 제거

작성일: 2026-09-30 · [English](Text_Size_Option_Removal.en.md)

사용자 결정에 따라 게임의 글자 크기 비율 조정 기능을 제거했다. 설정 화면의 제목·설명·슬라이더·증감 버튼·저장 재시도를 삭제하고, 기기 배율을 읽거나 저장하던 코드도 제거했다. 기존 `hellscript-interface-scale-v1.json`은 삭제하거나 덮어쓰지 않으며 게임에서 읽지 않는다. 타이틀과 공통 UI는 기본 배율 1을 사용한다.

화면 크기·화면 비율·안전 영역에 따른 반응형 배치는 유지한다. 가시거리 50~150% 설정은 전투 카메라의 시야를 조절하는 별도 기능이므로 그대로 제공한다. 언어·화면 비율·소리·캐릭터 변경도 유지한다.

## 이후 검증 규칙

저장소 `AGENTS.md`, `CLAUDE.md`와 한국어·영어 공통 UI 계약에서 글자 크기별 조합 및 확대 글자 전용 스모크 요구를 제외했다. 삭제한 기능의 단위 검사를 제거하고, 실행 가능한 UI 스모크의 배율 변경·반복 단계와 크기 인수를 없앴다. 다섯 화면 크기와 한국어·영어를 검사하는 표준 조합은 기존 20개에서 10개로 줄었다.

기본 크기의 문구 잘림, 고정 행동에 대한 접근, 화면 비율·안전 영역과 실제 조작 검사는 계속 수행한다. 과거 문서와 이미지에 남아 있는 100%·140%·150% 결과는 당시 검증 기록이며, 현재 기능이나 앞으로 실행할 검사 목록이 아니다. [설정 메뉴](Settings_Revision.md)와 [공통 UI 계약](Shared_UI_Contract.md)에 현재 기준을 반영했다.

## 이번 변경의 검증 결과

관련 Edit Mode 검사 81개를 실행해 80개가 통과했다. 삭제한 기능의 번역 문구 하나가 남아 실패한 검사는 해당 문구를 제거한 뒤 1개만 다시 실행해 통과했다. 통과했던 80개는 반복하지 않았다. 공통 UI 계약 검사와 계약 단위 검사 11개도 통과했다.

Unity 6000.6.0f1에서 macOS 개발 플레이어를 컴파일하고, 기본 글자 크기로 세로 440×956·가로 956×440·PC 16:9/16:10/21:9와 한국어·영어의 10개 화면 조합을 확인했다. 설정의 화면·소리·언어·캐릭터 탭과 닫기 조작, 안전 영역·문구 잘림·레이캐스트를 확인했고 조절 항목이 남아 있지 않았다. 기존 150% 설정 파일은 바이트 단위로 보존됐다. 캡처는 총 13개다.

이 실행은 별도 저장 경로를 사용하는 macOS 플레이어와 합성 uGUI 입력의 결과다. 모바일 실기기 검증이나 OS 포인터 조작 결과가 아니다. 이전에 완료한 전체 전투 검사와 122개 프리셋 검증, 관계없는 런타임 스모크는 다시 실행하지 않았다.

[검증 요약](TextSizeOptionRemovalEvidence/verification.json), [첫 검사 결과](TextSizeOptionRemovalEvidence/focused-tests-initial.xml), [실패 항목 재검사](TextSizeOptionRemovalEvidence/localization-final.xml), [설정 조작 결과](TextSizeOptionRemovalEvidence/settings-focused.txt), [기존 파일 보존](TextSizeOptionRemovalEvidence/legacy-file-preserved.json)을 함께 보관한다.

![세로형 설정 — 기본 크기](TextSizeOptionRemovalEvidence/settings-portrait-ko.png)

![가로형 설정 — 기본 크기](TextSizeOptionRemovalEvidence/settings-landscape-en.png)

메인에 새로 병합된 균열 결과 화면(`1bfb7e4d`)도 보존했다. 결과 화면 검증의 추가 배율 반복을 제거하고 통합 코드가 컴파일되는지 확인했다. 설정 화면의 실제 소유 코드는 바뀌지 않아 위 조작 결과를 재사용했으며, 병합으로 달라진 위키의 공개 사본과 이력만 추가 확인했다.
