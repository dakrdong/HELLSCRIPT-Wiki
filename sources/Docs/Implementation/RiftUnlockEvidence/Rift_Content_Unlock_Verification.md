# 균열 단계별 콘텐츠 개방 검증

갱신일: 2026-09-24

개발 브랜치 `codex/balance-1000-analysis`에서 콘텐츠 14개의 개방 조건, 최초 보상 목록·상세 안내, 실제 서비스 제한과 기존 저장 이관을 검증했다. [개방 기획서](../../Design/HELLSCRIPT_Rift_Content_Unlocks.md), [검증 데이터와 소스 해시](verification.json), [Unity 검사 원문](editmode.xml), [실행 검사 원문](runtime.txt)을 함께 본다.

- Unity 6000.6.0f1 Edit Mode: **403 통과, 실패 0, 건너뜀 0**. 해금·보석·대장간·품질·위상·룬·코어 제작·상점·지급 상자·번역 관련 검사다. 전체 테스트 모음의 결과로 해석하지 않는다.
- macOS Development 빌드: 성공, 빌드 오류 0.
- macOS 실행 검사: **24개 항목 통과**. 이 중 화면 크기·언어·글자 크기의 조합은 20개다.
- 화면: 440×956, 956×440, 1600×900, 1600×1000, 2100×900. 한국어·영어, 글자 크기 100%·150%에서 목록 행, 개방 설명, 고정 행동 영역을 확인했다.
- 공통 UI 소유권 검사와 관련 검사 9개, 수급 모델 검사 22개가 통과했다. 1~1000단계 모델의 검증 오류와 평균 재화·작업시간 부족 단계는 0이다. 이 수치는 기획 모델의 결과다.

## 실제 동작 확인

신규 계정은 개방 단계 전 서비스 실행이 차단된다. Edit Mode에서 R4/5 슬롯 강화, R9/10 보석, R14/15 룬, R39/40 위상, R59/60 물약, R79/80 걸작, R119/120 코어 제작 경계를 검사했다. 잠긴 기능으로 거래를 요청해도 재화·장비·룬 배치·난수 상태가 변하지 않는지 확인했다. 최초 보상 안내는 1~1000단계 전체에서 공통 해금 데이터와 일치한다.

macOS Player에서는 R4의 슬롯 강화 안내를 확인하고 R5에서 버튼을 눌러 강화석 소비와 작업 생성을 확인했다. R10 상세의 잠금 상태가 클리어 기록 반영 후 `개방됨`으로 바뀌며, 이 과정에서 최초 보상 상자가 수령되지 않는 것도 확인했다. R60 목록과 상세에는 소탕·보석 물약 제조가 함께 표시된다. R59에서는 제조 버튼이 비활성화되고 R60에서는 버튼 조작으로 보석 1개를 소비해 물약 1개를 지급한다. 영웅 교체와 저장·재로드 후에도 R120 코어 제작 권한이 유지된다.

기존 해금 v1 저장은 schema 13 / 해금 v2로 이관하면서 이전 권한을 보존한다. 구 단계 0·1·5·8·10·12·15, 조기 보석·코어 권한, 반복 이관, 원본 파일 보관과 재로드를 검사했다. 현재 schema에서 해금 버전이 누락되거나 지원되지 않는 경우에는 원본 저장을 덮어쓰지 않는다.

## 화면 증거

[R4 슬롯 강화 잠금](slot-locked-r4.png) · [R10 보석 개방 전 안내](gem-unlock-r10-before.png) · [최초 보상 목록](unlock-list-440-ko.png) · [R60 세로 한국어](unlock-detail-440-ko-100.png) · [R60 가로 영어 150%](unlock-detail-956-en-150.png)

## 검증 범위와 작업 보호

실행 검사는 독립된 macOS Player와 임시 저장 경로에서 수행했다. 포인터 이벤트를 Unity 이벤트 시스템에 전달하고, 실제 레이캐스트 대상과 거래 전후 상태를 확인했다. 초기 검사에서 화면 재생성 뒤의 버튼 참조가 바뀌는 문제를 확인해 검사 스크립트가 레이아웃 반영을 기다리고 맨 위 팝업의 버튼을 다시 찾도록 보완했다. 최종 실행은 종료 코드 0과 `HELLSCRIPT_RIFT_UNLOCK_RUNTIME_OK`를 반환했다.

단계 기록은 검사에서 준비한 조건이다. 물리 마우스·모바일 터치, 모바일 실기기, 자연 전투로 R1000까지 진행한 결과를 포함하지 않는다. 전체 전투·드랍·강화 비용의 수치 조정은 별도 밸런스안이며 이번 변경에서 일괄 적용하지 않았다.

사용자의 원본 작업 폴더와 저장 파일은 수정하지 않았다. Unity 임포트가 바꾼 에셋 메타·설정 506개는 실행 전 보관한 내용으로 복원했다. 테스트를 통과한 게임 소스는 이후 변경하지 않았으며, 최종 검사 스크립트는 macOS 빌드에 다시 반영했다.

공개 위키는 병합된 `main`에서 게시한다. 이 기록은 개발 브랜치의 구현·검증 결과이며, 원본 `main` 병합이나 공개 게시가 완료되었다는 뜻은 아니다.

English: 403 focused Edit Mode tests passed with zero failures or skips. The macOS Development build succeeded, and native acceptance passed 24 checks, including 20 resolution/language/text-scale combinations. The checks cover stage boundaries, service guards, first-clear popup guidance, actual slot and potion transactions, account-wide access, save/reload, and preserved legacy entitlements. Input used raycast-verified Unity EventSystem pointer events in an isolated fixture save. The early harness was corrected to wait for layout and reacquire the foreground popup button after repaint. This does not claim physical-mobile validation, natural progression to Rift 1000, full combat/drop tuning, main integration or public deployment. Import-only changes were restored from the captured baseline.
