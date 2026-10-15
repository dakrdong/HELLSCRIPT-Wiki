# 전투 거리 HUD 캐주얼 스킨 — 공통 후속 API 연결

2026-10-14 · [English](Casual_Combat_Distance_HUD_20261014.en.md)

전용 브랜치 `codex/casual-distance-hud`. 화면 소스 단계와 공통 API 연결을 완료했으며 **Unity 실행·실제 화면 수용은 부모 슬롯 배정 대기**다. 최초 기준은 `2e193c336c6176d80b9ddbdff7269246bf2590fb`(게임 코드 `4139a77e`)이고, 화면 커밋 `fa5110a6`, `3f4eccc0`를 보존했다. 승인된 공통 후속 `0ccba80eef0da4703cb7ffcd1fb8e22848002b3d`를 로컬 merge `4842b667c07a68a30302be7f2ade007ebdb8b477`로 충돌 없이 가져왔다. 공통 파일을 화면 담당이 직접 수정하지 않았다.

## 실제 시안 확인

Claude `https://claude.ai/artifact/1Pkt5gL8FtWFDzYTkTaDdJ`를 Mac 지원 Chrome에서 읽었다. 사용자 제시20개, 실제 열거·내용 확인20개, 확대·상호작용2개다. 내부 이동으로 위치/대상순위/추적/고블린/행동순서5종을 조작했고, 같은 KO 세로 보드의 LURE KNOWN/SCOUT 전장 예보도 확인했다. 모든20개의 모든 상태를 실행했다는 뜻은 아니다. 별도 PC 보드는 없었다.

후속 브라우저 작업은 `dakrdong@gmail.com` 프로필 확인 후에만 수행한다. 이 지시 이후 새 브라우저 작업은 없다. 실제 시안 export의 Library ID는 **`libfile_96eed501147c8191a90434ce71eb2a96`**다. 구현 after 캡처는 아직 없다.

## before / after와 연결 범위

| 영역 | 이전 구현 | 현재 소스 |
|---|---|---|
| compact 외곽 판 | 어두운 Image·직선 border | 기존 공통 `panel.landscape.active`의 크림 면·남색 둥근 테두리 |
| 이름표 | 어두운 판·본문 폰트·best-fit | `card.pill.port.l` 금색 paint, Jua13 디자인 단위, 자동 축소 없음 |
| 순위/행동 카드 | 어두운 면·직선 border | 기존 idle/chosen paint, 실행 상태는 idempotent Apply |
| 본문·강조·기호 | 기존 기본 폰트 및 화면별 기호 fallback | 승인 Noto400/700, FontStyle.Normal, 기호5종도 같은 폰트 |
| 위치 라디오 | 36u행·12/10 글자·장식 PNG2개 | 실제 CSS12.5/11, 최소49u와 설명 줄바꿈 높이, 별도 래스터 없음 |
| 버튼 | 기존 Gilt 얼굴 | UiSkin.Casual·코랄 닫기, 선택/비활성/hover/press/focus는 공통 UiButton 소유 |
| 숫자 조절기 | 기존 track/fill/knob 색 | ConfigureEdictSkin → 공통 StyleSheetNumber의 DisabledEdge/Sky/Gold. 범위·step·preview·release 저장 유지 |
| world preview | 기존 금색/위험/범위색 | 공통 ConfigureEdictSkin의 preview 전용 재질·아이콘 tint. 경로·시간·카메라 계산 유지 |
| 복제본 | 비직렬화 paint/버튼 상태 소실 가능 | 화면 전용 look/scale 및 버튼 상태·라벨 alpha 복원. 실제 버튼에는 최신 상태 유지 |

`BuildCompactEdictSurface`는 Position/Target/Order 세 패널이 숫자 행을 만들기 전에 공통으로 거치는 생성 지점이다. 이 카드의 `CasualEdictSkinScope`가 `ConfigureEdictSkin(UiSkin.Casual)`을 적용하며, 카드 비활성/닫기/재생성 때 이전 범위를 복원한다. 다른 소유자가 범위를 바꿨으면 덮지 않는다. 일반 칙령 화면의 숫자 조절기로 스타일이 넘어가는 것을 방지한다. `ConfigureCombatSkin`은 DPS/그래프의 생성 범위이므로 이 거리 HUD가 설정하지 않는다. 장비·차트가 없는 화면이어서 장비 provider와 차트 skin 인자의 별도 호출도 필요하지 않다.

공통 API는 복제 상태를 직렬화하지 않으므로 기존 `CasualEdictSurface`/`CasualEdictButtonFace`는 유지했다. 별도 렌더러나 원화의 중복 구현은 없다. 보강 Noto에 ●가 포함되어 긴급 생존 안내의 별도 bullet Label/TrimStart 우회도 제거하고 원래 한 문장·10u 시작점·전체 폭을 복원했다. 기존 Loc 키는 같다.

외곽416/380·높이356/396, 안전영역/quickScale, target/order 행 치수·컨트롤 좌표·문구를 유지한다. 라디오 최소49u는 시안 실측48.8438px에 근거한다. 기존 대상 설명의 의도적 말줄임도 유지한다. Undo/Play·내 영역·진행 잠금·7개 행동·긴급 생존 고정·드래그 예보/드롭 저장·실패 처리·focus/scroll/pause 복원은 기존 소유 코드로 연결한다. 사용자 세이브·Core·스키마는 변경하지 않았다. 새 프레임 갱신 루프는 없다.

## 폰트·자산 근거

`0ccba80e`의 Git blob에서 Jua1종/Noto3종을 직접 읽어 확인했다. Noto cmap2619, 본문 누락0, `▲▼≡●○` 누락0이며 모두 실제 윤곽/advance가 있다. KO288·EN매칭288(고유284) 문구 집합은 이전 화면 커밋과 같다. Jua 제목3종 KO/EN 누락0이며,13pt EN 정적 advance109.733/96.694/87.672는 명목122폭 안이다. 이는 Canvas 줄바꿈·hinting·클리핑 합격 근거가 아니다. 최신 증거는 작업 루트 `evidence/common-followup/font-coverage-audit.json`, 이전 감사는 별도 보존한다. 폰트 크기를 줄이지 않았다.

공통 승인 장비359PNG/provider는 공통 커밋 이력에 포함되어 있다. 이 화면은 장비를 표시하지 않으므로 새 화면 매핑은0개다. sparse 소스 작업 폴더에359PNG를 펼치거나 복제하지 않았다. 승인650종/700PNG 색인에 없던 기존 Position4종(`icon-expected-position`, `icon-chokepoint`, `icon-scout`, `icon-enemy-pack`)은 WorldView의 Resources 참조 때문에 PNG/meta/GUID를 보존한다. 기존 manifest의 productionApproved=false/model=unknown도 유지한다. 전장 원화 교체는 단일 제작 소유 배정이 필요하다. 생성·삭제·교체0, 회수0bytes. 전체 SHA·참조는 작업 루트 `Combat_Distance_HUD_Asset_Audit_20261014.md`에 있다.

## 검증과 남은 차이

- UI 계약 `tools/check_ui_contract.py --verify`: 관련15개 회귀. 최신 receipt는 `evidence/common-followup/ui-contract-receipt.json`.
- UI 갱신 `tools/check_ui_refresh.py`:16 guarded bindings /192 UI source files. `git diff --check`와 공통 파일 무수정도 확인한다.
- 독립 Roslyn: 현재 런타임837소스와 화면 전용 테스트10사례 어셈블리 컴파일 통과. 기존 경고217개, 변경 화면 파일 경고0, 컴파일 중 입력 변경0. 읽기 전용 기존 참조를 사용하고 모든 출력은 작업 루트에 보관한다. `evidence/common-followup/receipt.json`이 입력 SHA 정본이다.
- 테스트10사례: paint 복제2, 루트/중첩 버튼 복제4, live 비활성 보존1, compact 범위 복원/다른 소유자 보존3. **컴파일만 완료했으며 Unity 실행0**이다.
- 다음 큐: 위10사례와 HuntEdictHudRowsTests/HuntEdictQuickMenuTests/StoreViewBindingTests, 공통 CasualChartSkinTests의 관련 숫자/preview 계약. PositionOnly/TargetOnly/OrderOnly 무음 스모크와 격리 세이브, KO/EN,440×956/956×440/PC16:9·16:10·21:9/2868×1320, 입력·저장 실패·재진입을 확인한다. Mac 시뮬레이션은 iOS 실기기 검증이 아니며 안전영역은 해당 실행의 가정으로 기록한다.

소스 기준 잔여 차이는 둥근 사각형 뒤로 버튼(시안 원형), target/order 닫기32×36u(시안36×36), slider의 기존 사각 track/fill/knob(공통 API는 색만 제공), 기존 world PNG4종과 overlay badge/label의 세부 모양이다. 새 world palette의 안개·범위 상태·연결선·아이콘 가장자리도 실제 캡처가 필요하다. 관련 공통 모양 API나 원화 소유를 조율하며 임의 렌더러를 추가하지 않는다.

기존 Position smoke는 무스크롤을 요구한다. 세로 LURE 최소콘텐츠207u/viewport208u이므로 EN 설명 줄바꿈이 특히 위험하다. 실패를 제외하거나 폰트를 줄이지 않고 보고한다. EN 고블린 안내208×32 및 target 제목21u 높이도 우선 확인한다.

부모가 보고한07:50의 외부 Unity PID30116은 scratchpad/proj-final에서 실행 중이다. 새 Unity/import/build/player는 시작하지 않았고, 이 화면 슬롯은 미배정이다. 실제 after 캡처는 배정 후 Library에 저장한다. 원본 checkout/staging, 다른 브랜치·미커밋 원화, 세이브는 보존했다. 원격 push/PR/main병합/웹빌드/공개 배포 없음.

공통 규칙 제안: 임시 compact UI는 생성 범위와 닫기 복원을 함께 검증한다. drag ghost는 비직렬화 renderer 상태/alpha를 확인하되 live 상태를 snapshot으로 덮지 않는다. 공통 기호가 보강되면 임시 화면별 폰트 우회와 분리 Label을 제거하면서 원래 문구·크기·좌표를 복원한다.
