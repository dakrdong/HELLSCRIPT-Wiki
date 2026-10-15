# 균열 장비 추천 — 단일 통합 검증 인계

2026-10-14 · [English](Casual_Rift_Gear_Skin_20261014.en.md)

**공통 후속 API·승인 장비 provider 연결 완료. 개별 Unity/빌드 계획은 종료하고 소스 편집을 동결한다.** 실제 화면·동작 검증은 모든 UI를 합친 단일 통합 입력에서 수행한다. 통합 소유자는 `01a13790-39bc-7062-a516-b83a354000a8`, 실행 폴더는 부모가 정한 task-6/skill-skin의 별도 통합 브랜치와 기존 Library다. 이후 수정은 구체적인 통합 결함 요청이 있을 때만 재개한다.

## 통합 입력과 소유권

- 전용 브랜치 `codex/casual-rift-gear-skin`, task-10/rift-skin. 기준 `2e193c336c6176d80b9ddbdff7269246bf2590fb`, 검증 게임 코드 `4139a77e2df495cee009c07c6094b2eea9b1aeaa`.
- 화면 커밋 순서: `12880d713ac720e2b177d3f8383bf48584a3aa3e` → `35fd4986c0d4bf0779dd067dbb3c1850a9b20fac` → 이 보고서를 포함하는 최종 provider 소비 커밋. 정확한 최종 SHA/dirty/bundle은 task-10/evidence/FINAL_INTEGRATION_HANDOFF.md에 고정한다.
- 공통 누적 tip `0ccba80eef0da4703cb7ffcd1fb8e22848002b3d`: 선행 `6cdf2df690941b7c9f751c39235ea36ce4ff9ed9`(배치/기호), `f25e67aab62d72f9792b5c0d49e6692bfbca5eed`(원화) 포함. tip만 단독 cherry-pick하면 선행 공급이 빠지므로 전체 ancestry를 사용한다.
- 전용 브랜치에서 공통 첫 merge는 `f02d6f4c4e2fce6d923a13891888b32fc9e6eacf`, 후속 merge는 `b22a82c220884e33c88aeacc6ad649b3b3af3107`. 부모는 전체 브랜치를 merge하거나 공통 tip 위에 화면 소비 커밋을 위 순서로 적용할 수 있다.
- 자체 런타임 수정은 `GameUI.GearRecommendations.cs`, `RiftRecommendationWindow.cs`, `RiftGearSettingsWindow.cs` 세 파일뿐이다. 공통은 직접 수정하지 않았다. 공통774파일이 제공 tip과 동일함을 source/tree audit으로 확인했다.
- 원본 checkout/index/ref 및 다른 작업 파일 변경 없음. sparse clone은 원본 Git 객체를 읽는 alternates를 사용한다. 통합 전 객체를 정리하지 않는다. 이 폴더에는 Resources 전체와 Unity Library가 없어 실행 프로젝트로 사용하면 안 된다.

## 연결 완료 내용과 승인 원화

`RiftGearSkin.Current`는 한 번 생성한 `UiSkin.Casual.WithEquipmentArt(CasualEquipmentArt.Provider)`를 반환한다. HUD 카드, 두 창, 장비 슬롯, 상세/비교 및 그 재생성 경로에 동일 scope가 전달된다. 남은 화면 전용 공통 API 연결은 없다. 차트/칙령 API는 이 화면 소유 범위가 아니다.

공통 provider는 `Art/Casual/Equipment/{ID}` Resources 경로를 사용한다. 공급 receipt는 [tools/casual_equipment_art_receipt.json](../../tools/casual_equipment_art_receipt.json), SHA256 `52b29468e0932e9d40e80ed007b27345a69ad08188ae3eb47b004dd3defa3020`이다. 승인 ID는 `casual-equipment-20261014-359:qa-sha256:619e9fb629ebb03ad0ea78f7bb6f172fb5b0a24c39b59a1798324e33e4d74f4b`.

공통 설치본359 PNG,330 item key,30 emblem,362 GUID와 기존336원화 보존 검증 통과. 총69,753,836bytes이며 B28→B32 alias 및 카탈로그/해금 규칙을 유지한다. 감독/QA/CSV/색인 hash와 원본359개·공통359개 SHA를 다시 대조했다. [화면 측 원화 검증](CasualRiftGearSkinEvidence20261014/approved-art-followup-audit.json). 화면별 PNG 사본·새 생성·기존 원화 삭제0. 새 bundle은 공통 원화 history를 전제조건으로 제외해359 PNG를 다시 담지 않는다. Resources 공급은 연결 화면 외에도 빌드 포함량을 늘리는 공통 변화이며 실제 import/렌더는 미검증이다.

공통에서 해결한 임시 처리도 제거했다: compare 탭의 로컬 ApplyLabel, OnLight(Muted) 중복, `＋`→ASCII `+` 치환. Noto400/700/900은 U+FF0B를 포함한 요청 기호를 지원한다. 원래 번역 키/표시문을 복원하고 공통 LabelFont가 Jua 미지원 시 Noto400을 선택한다. 실제 라벨 폭은 통합 화면에서 확인한다.

## 시안과 변경 전후·남은 차이

시안: https://claude.ai/artifact/JfwkCuft9ZivYvSNvYwceJ . 선언16, DOM/본문/iframe 소스·상태 확인16, 확대 시각 확인3(Main KO portrait, Settings EN landscape, Compare EN landscape). 불완전14/16 PDF는 기준이 아니다. 이후 EN 가로 카드 접근성 확인은 시각 수에 더하지 않았다. 이번 후속 작업은 Chrome을 사용하지 않았고, 재개 시 dakrdong@gmail.com 프로필 확인이 필요하다.

| 영역 | 기존 → 현재 소스 | 통합에서 확인할 차이 |
|---|---|---|
| 전투 카드 | 어두운 표면/OS글꼴 → 공통 크림/남색 panel·slot·button, Noto700 이름/점수, Noto400 본문, 승인 PNG | 실제 아이콘·모서리·장착 라벨/말줄임 |
| 점수 배지 | 어두운 바탕 → 크림/초록 테두리·공통 증감색 | 시안의 연두 채움과 다름. 기존 연두 위 공통 색 대비약3.858이라 크림 사용 |
| 설정 | 어두운 창·숫자·선택 → 공통 shell/chrome·금색 선택·실제400본문 | KO `＋` 포함 Jua→Noto fallback 폭, 긴 EN라벨·scroll·disabled |
| 상세/비교 | 기존 원화/테마 → 공통 skin와 승인 provider | 실제 줄바꿈·compare target 전환·새 원화·rounded clip |

표는 소스 before/after다. Unity 게임 캡처0. 기존 시안 핵심 캡처 Library ID `libfile_547bac59b30c81918af6cfff2aa6fff0`.

소비 직접 좌표/fontSize,8개v.Button callback, 비교탭/숫자/slider callback, 카드폭92·slot+98높이(세로150/기존가로152)·가로.75배율·HUD위치·기존best-fit7–10/말줄임은 유지한다. 새 축소·폭확대·Canvas·Update loop 없음. timer/equip/dismiss/transaction/lifecycle tail은 원래 기준과 byte동일하다. 저장 draft/Normalize/원자적 SaveRiftGearOfferSettings/복귀 규칙은 바꾸지 않는다.

공통 상세4개 차이는 **소스상 복원**됐다: score13/16pt·plaque52/64, major divider 원래 높이, greater inset16, socket24/inset28. [독립 검토](CasualRiftGearSkinEvidence20261014/common-followup-review.md). 실제6개geometry 회귀와 폰트에 따른 줄바꿈은 미실행이다. 기존 ItemCardArt Badge/Divider/Star/Socket 리소스가 존재·치수 계산에 여전히 쓰이므로 단순 미사용 장식으로 삭제하면 안 된다.

## 완료한 정적 검사

- 최종 UI 계약15개 통과/22.499초. [receipt](CasualRiftGearSkinEvidence20261014/ui-contract-followup.json), [로그](CasualRiftGearSkinEvidence20261014/ui-contract-followup.log).
- 갱신16 guarded bindings/192 UI sources 통과. [로그](CasualRiftGearSkinEvidence20261014/ui-refresh-followup.log).
- [source/tree/글리프 audit](CasualRiftGearSkinEvidence20261014/shared-followup-audit.json) 통과, 공통774파일 동일성·현재 provider·기존 callback·직접geometry·Noto기호 확인. [재현 스크립트](CasualRiftGearSkinEvidence20261014/audit-shared-followup.py).
- 공급자 `--verify` 통과와 원본/공통359개 SHA 대조 통과. source RGB 대비 수치는 실제 기기 인증이 아니다.
- 독립 검토에서 새로운 소비 코드 결함을 발견하지 않았다. 이전 실패/무효 receipt는 보존하며 최신 pass로 소급하지 않는다.
- 이 화면의 C# 컴파일·Unity·EditMode·Player·빌드·게임 캡처0. Roslyn reference 사전조사만 했고 명령 실행은 하지 않았다. 통합 전환에 따라 별도 컴파일/개별 테스트 계획은 종료했다.

## 단일 통합 검증 대상

관련 필터: `Hellscript.Tests.RecommendedEquipmentTests`, `SharedUiTests`, `ItemComparisonTests`, `StoreViewBindingTests`, `UiButtonTests`, `CasualUiSkinTests`, `EquipmentArtTests`. 공통 검사와 중복 실행하지 않고 통합 담당이 최종 입력에 맞춰 선택한다.

진입점: 전투 HUD의 `rift-gear-icon-{id}`/`rift-gear-inspect-{id}` → 비교/상세, `rift-gear-quick-equip-{id}`, `rift-gear-settings-{id}` → 설정, `rift-gear-dismiss-{id}`. 기존 smoke flags `-hellscriptEdictControlsSmoke -hellscriptEdictGearSmoke`를 통합 실행에서 재사용한다.

KO/EN,440×956·956×440·1600×900·1600×1000·2100×900·2868×1320. 빈/선택/비활성/오류, 저장/취소/잘못된 숫자/옵션 추가/scroll, 비교 target·Repaint·장착/닫기/만료/포탈복귀, 원형 가장자리·둥근 모서리·긴 EN라벨을 확인한다. 일회용 세이브·오디오 무음. Mac 비율 시뮬레이션은 실제 iOS가 아니며 safe-area 가정은 통합 실행자가 기록한다.

외부 Unity PID30116은 부모07:50관측에 실행 중이었으나 이 작업에서 조작하지 않았다. 개별 슬롯 대기는 종료하고 모든 UI 단일 통합 실행으로 이관했다. source/branch/evidence 보존, 원격push·공개공유·main병합·웹빌드/배포 없음. 공통 규칙 제안은 장식 교체와 기하/글꼴 승인 분리, 실제 배경색 기준 대비, 재렌더의 scope 유지 검증이다.
