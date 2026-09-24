# 지급 상자 검증 기록

갱신일: 2026-09-25

이 문서는 전설 보상 단계를 조정하기 전의 검증 기록이다. 현재는 10단계에서 전설 없이 상자 5개를 지급하고, 전설 무기 상자는 30단계에서 지급한다. 최신 규칙의 재검증은 [2026-09-25 통합 기록](../Main_Integration_20260925.md)을 따른다. 아래 과거 수치와 캡처는 당시 근거로 보존한다.

English: This historical report predates the legendary reward delay. R10 now grants five boxes without legendary gear; the legendary weapon box is granted at R30. See the [2026-09-25 integration report](../Main_Integration_20260925.en.md) for current verification. Earlier results and captures are retained as historical evidence.

개발 브랜치 `codex/balance-1000-analysis`에서 지급 상자 96종, 최초 보상 수령, 개봉·저장 거래, UI와 투명 아이콘을 검증했다. [기획과 구현 범위](../../Design/HELLSCRIPT_Reward_Boxes.md), [검증 데이터](verification.json), [Unity 결과 원문](editmode.xml)을 함께 본다.

- Unity 6000.6.0f1 Edit Mode: **158 통과, 실패 0, 건너뜀 0**. 상자·보석·균열 입장·품질·코어 제작·번역 검사다.
- macOS Development 빌드: 성공, 빌드 오류 0.
- macOS 실행 검사: **25개 항목 통과**. 그중 화면 크기·언어·글자 크기 조합 20개를 검사했다. [원문](runtime.txt)
- 화면: 440×956, 956×440, 1600×900, 1600×1000, 2100×900. 한국어·영어, 글자 크기 100%·150%.
- 입력: macOS Player의 Unity 이벤트 시스템에 포인터 이벤트를 보내고, 실제 레이캐스트 대상과 수령·개봉 전후 저장 상태를 확인했다. 물리 마우스·모바일 터치 실기기 검증과 구분한다.
- 수급 모델: 22개 검사 통과. 1~1000단계의 실제 상자 지급과 기획 예산을 비교했다. 180일 기본 경로의 평균 재화·작업시간 부족 단계는 0이다.
- 리소스: 96개 PNG의 RGBA·알파 0/255·투명한 네 모서리와 Unity Sprite 로드를 확인했다. SVG 원본과 PNG는 같은 작성 도형을 사용한다. [리소스 목록](../../Art/RewardBoxes/manifest.json)

## 직접 확인한 동작

균열 10단계 목록 → 정상 완료 기록에 따른 상자 6개 수령 → 중복 수령 차단 → T1 취옥 선택 후 10개 지급 → 전설 무기 개봉 → 공통 장비 상세 표시 → 심연 주화 100개 지급을 확인했다. 기존 자동 골드·재료는 재지급하지 않았다.

가방 하단에서 보상 상자를 여는 경로, 1000단계 목록 접근과 수령, T6 선택 상자 2개 및 품질 하한 90% 각성 무기 상자의 저장·재로드도 확인했다. 저장 실패·용량 부족·정수 초과·요청 중복·낯선 저장 버전은 Edit Mode에서 검사했다.

## 화면 증거

[10단계 최초 보상](first-clear-10.png) · [전설 무기 결과](legendary-weapon-result.png) · [세로 한국어](boxes-440-ko-100.png) · [가로 영어 150%](boxes-956-en-150.png) · [1000단계 최초 보상](first-clear-1000.png)

## 검증의 경계

검사에는 별도 작업 폴더와 임시 저장 경로를 사용했다. 사용자의 저장 파일과 원래 열려 있던 Unity 프로젝트는 수정하지 않았다. 정상 완료 기록은 실행 검사에서 준비한 조건이며, 1000단계까지 자연 플레이한 결과가 아니다. 적 난이도·일반 드랍·강화 지출의 전체 적용과 장기 실전 성장 검증은 남아 있다.

원본 저장소의 작업 브랜치와 공개 위키 배포를 구분한다. 공개본은 이 변경을 `main`에 병합한 뒤 게시한다. 현재 기록은 공개 게시 완료를 주장하지 않는다.

English: 158 focused Edit Mode tests passed; macOS build succeeded. Native acceptance passed 25 checks including 20 resolution/language/scale combinations. Pointer events were injected through Unity's EventSystem with raycast and persisted before/after assertions. This does not claim physical-mobile testing, measured combat win rates, a natural 1,000-tier playthrough or public deployment. Isolated fixture saves were used. Full combat/drop tuning remains a proposal.
