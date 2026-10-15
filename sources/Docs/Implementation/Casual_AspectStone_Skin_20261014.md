# 위상 각인석 캐주얼 스킨 — 소스 인계

2026-10-14 · [English](Casual_AspectStone_Skin_20261014.en.md)

상태: **전용 소스 구현·정적 검사 완료, 단일 통합 담당에게 인계 후 소스 동결**. 개별 Unity·빌드 계획은 종료했다. 아래 과거 큐 설명보다 [최종 통합 인계](Casual_AspectStone_Integration_Handoff_20261014.md)가 우선한다. 네이티브 화면 일치나 전체 UI 완료 판정이 아니다.

## 기준과 실제 관찰

- 브랜치 `codex/casual-aspect-skin`, 작업 폴더 `/Users/t8g-2410-pn-005/Documents/Codex/2026-10-14/task-16/aspect-skin`.
- 초기 공유 기준 `2e193c336c6176d80b9ddbdff7269246bf2590fb`; 부모 승인으로 공통 API `cdb9c140fbf8b778bd3c0bbc0fbcc68a5d9e3fa8`까지 fast-forward한 뒤 전용 구현 `d0c10513911b6d4e098eb25462c0735aa6f18dbd`를 보존하며 후속 **`0ccba80eef0da4703cb7ffcd1fb8e22848002b3d`**를 충돌 없이 병합했다. 병합 커밋 `15f5baf5c71a54a1cc12fcbc6f1d760409a34659`. 검증된 스킬 게임 코드 `4139a77e2df495cee009c07c6094b2eea9b1aeaa`.
- [Claude 시안](https://claude.ai/artifact/7fmUuS5eVQwGyJ9Kne9h5t): 사용자 제시 32개, iframe 이름으로 확인 32개. 개별 확대·상호작용 검증은 Main PC KO **1개**. 추가 DOM 치수 확인은 Main PC KO와 Imprint-Portrait-EN **2개**. 32개 모두 시각 검증한 것으로 보고하지 않는다.
- 시안에서 각인 확인→완료→중복 비활성, 검색 빈 결과→복구, 세트 장비 선택 시 비적격, 필드 탭을 확인했다. 시안은 실제 계정 저장을 수행하지 않는다.
- PC 카드 약 139.6×157 CSS px, 영문 세로 카드 122×159. 이름 11px/900, 상태 9px/900, 제목 Jua, 본문 Noto400. 코드의 카드 최소폭 122·기본높이 157·11/9 글자를 기준으로 삼고, 글자를 줄이는 fit을 추가하지 않았다.
- 기존 캡처는 `discovery/evidence/claude-pc-ko-field.png`(1440×810), Library **`libfile_fd9fdb26ea908191a21eafb8af7b3cfd`**. 시안 캡처이며 게임 after 증거가 아니다.
- Chrome 프로필 추가 지시 이후 브라우저 재개 없음. 다음 브라우저 작업은 `dakrdong@gmail.com` 프로필을 화면/지원 도구로 확인한 뒤에만 재개한다.

## Before / after — 소스 기준

| 대상 | 이전 | 이번 전용 구현 | 남은 검증/차이 |
|---|---|---|---|
| 도감 | 78높이 단일 행 목록 | 원화·ID·이름·등록 상태 카드, 가변 열 수, 선택 노랑·미등록 회색 점선 | 네이티브 줄바꿈/스크롤 미실행. 공통 셸 배율 때문에 PC 5열과 아직 다름 |
| 패널 | 어두운 inset | 공유 팔레트의 크림색·네이비 둥근 면, 테두리 안쪽 viewport | 공통 창/일반 버튼/장비 슬롯·상세에 승인 장비 공급자를 포함한 화면 전용 Casual 스킨 연결. 네이티브 검증 대기 |
| 타이포 | 기존 Body | 전용 본문 Noto400, 버튼700, 카드 강조900, 섹션 Jua | 공유 셸/장비 부품은 공통 캐주얼 글꼴 선택. 실제 렌더 확인 대기 |
| 각인 | 주 버튼 즉시 거래 | 확인 열기, 취소, 확정, 완료 닫기 | 부모 창 input 차단·저장 실패 실제 실행 대기 |
| 거래 | 기존 `AspectImprintPlan` | 같은 plan을 확인 시 캡처해 확정 때 한 번 실행, 실패 오류 표시 및 재전송 차단 | 게임 규칙·재화·수량·세이브 스키마 변경 없음 |
| 원화 | 기존 위상·장비 경로 | 위상 LW02 승인 문양 + 공통 승인 장비359 공급자 | 대표 네이티브 import/축소 검증 대기; 위상 확장은 별도 판단 |

불변: 장비 선택 자격, 세트/마법 장비 제외, 수집 상태, 필터·정렬·검색 의미, 레벨 임계값 1/2/4/8/16, 수동 성장, 무료 각인의 수량 보존, 기존 저장 거래 함수. 시안이 제공하는 비적격 장비의 선택/미리보기는 현재 게임 선택 계약과 달라 구현하지 않았다.

시안 전용 필드 배치·초기화·분해 서비스 탭과 게임의 기존 각인석 UI는 기능 범위가 다르다. 해당 기능을 임의로 생성하거나 다른 화면 분해 거래를 옮기지 않았다. 현재 게임의 전체/활성/레벨업 3개 필터도 유지했다. 시안의 4개 필터와는 차이가 남는다.

## 소유 파일 및 원화

전용 수정: `AspectStoneWindow.cs`, `AspectRuneGraphic.cs`, `RuntimeAspectStoneSmoke.cs`.
신규: `AspectStoneSkin.cs`, `AspectStoneArt.cs`와 meta, `AspectStoneSkinTests.cs`와 meta, `Resources/Art/CasualAspectStone/LW02.png`와 meta/폴더 meta, 이 보고서와 전용 근거 폴더. 공통 파일은 승인된 0ccba80e까지 그대로 가져왔으며 독자 수정하지 않았다. 도메인·Localization·씬·설정은 수정하지 않았다.

[승인 원화 124개 매핑](CasualAspectStoneEvidence20261014/approved-art-manifest.json)을 모두 SHA 검증했다. 복제는 LW02 1장 1,168,223 bytes만 수행했다. SHA `7eaedc552682d30ece36195a7d7b6cf139aeb250bc577acbd0bbd607a78ff27e`, 1254×1254 RGBA. [연결/보존 기록](CasualAspectStoneEvidence20261014/aspect-art-mapping.md). 기존 PNG/meta/GUID와 공유 경로는 보존했다. 신규 생성 원화·삭제·격리 없음.

## 검사와 큐

- `check_ui_contract.py --verify`: 관련 계약 검사 15개 통과.
- `check_ui_refresh.py`: 16 binding guard 통과. source inventory 192개.
- `git diff --check`: 통과. 신규 meta GUID 중복 없음.
- [글꼴 cmap 검사](CasualAspectStoneEvidence20261014/font-coverage-audit.json): 화면 문구와 123개 위상 이름/설명에서 한글 390자, Jua/Noto400/700/900 누락 0. 실제 렌더·힌팅·영문 줄바꿈 검증을 대신하지 않는다.
- 기존 Unity 참조를 읽는 최신 Roslyn 정적 컴파일: Runtime 835파일 오류0/경고47 → Editor 53파일 오류0/경고1 → Tests 259파일 오류0/경고2. Tests는 이번에 컴파일한 Editor 참조를 사용했고 전체 입력 해시 변경0이다. 테스트 메서드 실행0. 최초 `TextGenerator.Dispose` CS1061 실패를 명시적 `IDisposable` 호출로 수정했으며 이전 실패/성공 로그도 보존했다. [검사 영수증](CasualAspectStoneEvidence20261014/static-checks.json)에 절대 경로·SHA가 있다.
- 신규 EditMode **9사례 작성**, 실행 0. 확인 열기 무거래/host, 비적격·미등록, 열린 모달 언어 전환, stale plan 오류 및 재전송 차단, 같은 레벨/재각인, KO/EN 검색 복구, 기존 필터.
- Runtime smoke에 실제 uGUI synthetic pointer/drag/wheel, 취소 무거래, 확정 단일 receipt, 재화·수량 보존, 완료 닫기, KO/EN·6비율·2868×1320를 추가했다. Unity 실행·플레이어 빌드 모두 0.
- 2868×1320의 가정 안전 영역은 왼쪽/오른쪽102px, 아래54px, 위0px다. Mac 시뮬레이션이며 iOS 실기기 증거가 아니다.
- 부모 큐 필터: `Hellscript.Tests.AspectStoneTests`, `Hellscript.Tests.AspectStoneSkinTests`, `Hellscript.Tests.StoreViewBindingTests`, `ItemArtCoverageTests.EveryReleasedAspectIdentityUsesNativeArtworkAboveItsStone`.
- 네이티브 실행은 부모 지정 프로젝트 한 곳에서 무음, 전용 save path, 첫 실행과 resume 경로를 사용한다. 기존 사용자 세이브·플레이어 출력 덮어쓰기 금지. 산출물 예정: XML/로그/스모크 텍스트/핵심 PNG 약 25–50MB, 검토일 2026-10-21. 실제 출력 크기는 실행자가 기록한다.

독립 리뷰에서 언어 변경 시 장비명 고정, Canvas 배율과 측정 배율 차이, 스크롤 내용의 패널 테두리 침범을 수정했다. Jua에 없는 구분점은 공통 `LabelFont` fallback으로 표시한다. 기존 각진 Item 피드백은 카드 안쪽 6단위로 넣어 바깥 모서리 침범을 막았다. 네이티브에서 아직 확인하지 않았다.

## 연결한 API와 남은 의존성

부모가 전달한 공통 0ccba80e와 실제 인계문·차이를 읽고 병합했다. 메인 창에는 `UiSkin.Casual.WithEquipmentArt(CasualEquipmentArt.Provider)`를, 확인/완료 창·장비 슬롯·상세에는 같은 `view.Skin`을 전달한다. 전역 `UiSkin.Casual.EquipmentArt`는 null로 유지한다. 공통 생성 버튼은 자동 Casual, 화면이 만든 장비/카드 `UiButton`도 `ApplyChrome`을 사용한다. 전용 패널은 공유 `StorageSurface.SetSkin`, 성공 글자는 `UiSkin.Casual.Success`를 사용한다. 공통 상세의 원래 geometry 복원, 탭 대비, Noto 기호 보강도 그대로 포함한다. 차트/전투/칙령 API는 이 화면에서 사용하지 않는다.

승인 장비 원화는 공통 공급자 330 item key/30 emblem key → 359 PNG를 재사용한다. 공통 원본·승인 receipt·destination SHA 검사 359/359가 통과했다. 이 소스 전용 sparse checkout에 359 PNG를 별도 복제하지 않았으며 부모 네이티브 프로젝트에서 공통 커밋의 Resources를 가져와야 한다. 공통 Resources 증가량은 69,753,836 bytes(66.522MiB)다. 같은 LW02라도 위상 문양(1254×1254, SHA `7eaedc55…`)과 장비(512×512, SHA `e922415d…`)는 별도 그림이라 위상 대표 원화를 유지한다. 검사는 실제 장비 슬롯·상세 텍스처와 공급자, 각인 후 원래 장비 그림 보존, 전역 공급자 불변까지 포함한다.

1. **기준 폭/배율 정책 대기:** 일반 셸은 가로 800×450 단위를 기준으로 계산한다. 시안 1440×810과 달라 기본 16:9 중앙 패널에서 122폭 카드가 **1열**이 된다. 2868×1320에서도 시안의 5열 구조와 다르다. 글자를 줄이거나 스킬 2×/별도 Canvas Fit을 복제하지 않았다. 공통 소유자는 화면 기준1440×810과 패널 폭300/742/362로 카드139.6폭의 5열을 제안했다. 그러나 현 배율1.8에서 물리19.8px인 이름이 11px가 되는 효과가 있어 승인되지 않았다. 부모는 글자 축소 예외를 승인하지 않았으며 기준 크기·패널 폭·글자 크기는 그대로 유지했다.
2. **공통 Item feedback 모양:** 0ccba80e에서도 Casual Item은 기존 각진 링을 유지한다. 카드 안쪽6단위에 둬 외곽 둥근 모서리 침범을 막았으며, 동일한 둥근 카드 focus 모양은 공통 소유자 후속 범위다.
3. **캐릭터 원화 및 위상 확대:** 장비359 공급자 연결은 완료됐다. 위상은 승인 LW02 대표만 교체했고 나머지 문양·캐릭터 그림은 기존 상태를 보존한다. 추가 원화 교체는 네이티브 대표 검증과 부모 경로 조율 뒤 판단한다.
4. **통합 검증:** 개별 Unity 슬롯 요청과 빌드 계획은 종료했다. 통합 담당 `01a13790-39bc-7062-a516-b83a354000a8`가 task-6/skill-skin의 별도 통합 브랜치와 기존 Library에서 전체 변경을 모아 검증한다. 실제 Unity 컴파일·PlayMode·화면 캡처·저장/재실행은 기존 정적 C# 증거와 분리한다.

위 의존성은 **대기**, 거절/불확실한 실행은 없다. 원격 push·main 병합·웹 빌드·공개 공유·설치·권한 변경 없음.

공통 규칙 제안: 시안 CSS 크기와 일반 셸 단위의 대응을 화면별로 명시하고, 카드 최소폭/열 수/EN 행높이를 별도 기록한다. 둥근 면의 내용 viewport와 hover/focus mesh도 같은 경계 안에 있어야 한다. 확인 창은 변경 가능한 표시명은 현재 언어로 렌더하되 거래 대상·레벨·fingerprint는 고정한다.
