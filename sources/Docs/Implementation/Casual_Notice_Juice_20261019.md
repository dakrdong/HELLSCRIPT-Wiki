# 공통 확인창·토스트 연출 보강 — 2026-10-19

[English](Casual_Notice_Juice_20261019.en.md). [공통 알림 문서](Casual_Common_Notices_20261017.md)의 `.28초` unscaled 상승·fade(`CasualNoticeEntrance`)는 확인창 카드와 토스트에 이미 적용돼 있다. 이번에는 그 위에 빠진 두 가지만 더했다.

- **확인창 배경 dim 페이드**: Navy `.6` scrim이 카드와 같은 시계(`SetBackdrop`)로 alpha 0→`.6`을 올라간다. 최종 색·alpha·입력 차단은 그대로이며 새 색상 리터럴·문구·저장 키는 없다.
- **등장 완료 후 갱신 중지**: `CasualNoticeEntrance`가 끝나면 스스로 `enabled=false`가 되고 `Restart()`가 다시 켠다. 매 프레임 빈 `Update` 호출이 남지 않는다(`AgentRules/CPU_Performance.md`).

## 일부러 하지 않은 것
- **카드·토스트 scale pop**: 문서 정본이 "크기는 바꾸지 않는다"이고 `RuntimeCasualUiFourSmoke`의 "preserves scale rather than zooming" 확인과 충돌한다.
- **토스트·확인창 퇴장 fade**: 토스트 만료(`GameUI.cs` Update)와 확인창 `Close`(즉시 Destroy 후 action) 순서를 바꿔야 해 이 범위를 넘는다.
- **광장·출석·도크 연출**: PR #144(`codex/town-shortcut-stack`)·#146(`claude/dock-medallion-icons`)가 같은 파일(`GameUI.Plaza/ContentDock/Attendance/SettingsFrame`)을 수정 중이라 병합 후 최신 `main`에서 별도로 진행한다.

## 검증
`CasualNoticeLayoutTests`(신규 `BackdropFadesOnTheEntranceClockAndAFinishedEntranceStopsUpdating` 포함)만 EditMode 배치로 실행했다. `python3 tools/check_ui_contract.py --verify`, `python3 tools/check_ui_refresh.py` 통과. 전체 EditMode·Runtime 스모크·macOS 빌드·실기기는 실행하지 않았다.
