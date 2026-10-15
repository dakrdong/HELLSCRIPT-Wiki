# 균열 승리 캐주얼 스킨 — 통합 대기 후보

2026-10-14 · [English](Casual_Rift_Victory_Skin_20261014.en.md)

## 최종 인계 — 전체 화면 통합 후 한 번 검증

사용자 요청에 따라 **개별 Unity 실행·개별 빌드 계획을 종료했다.** 이 작업은 아래 인계 문서 커밋 후 소스 편집을 중지하며 통합 담당의 구체적인 결함 요청이 있을 때만 재개한다. 소스·브랜치·근거·원화를 삭제하지 않는다. 기존 검사/생성/import는 반복하지 않았다.

- 단일 통합 담당: `01a13790-39bc-7062-a516-b83a354000a8`. 대상은 `task-6/skill-skin`의 별도 통합 브랜치와 **기존 Library 그대로**다. 이 화면의 독립 실행 슬롯 요청은 철회하고 전체 화면 통합 검증에 포함한다.
- 최종 **기능/소스** 커밋 `631bcf9fb6117078480beb124444134b4b050ee5`, 브랜치 `codex/casual-rift-victory-skin`, 경로 `/Users/t8g-2410-pn-005/Documents/Codex/2026-10-14/task-19/rift-victory-skin`. 인계 준비 직전 clean, 미커밋 소스 변경0. 이 뒤의 인계 커밋은 한/영 문서와 queue JSON만 바꾸며 실제 최종 HEAD는 전달 메시지/외부 implementation receipt에 기록한다.
- 권장 통합: 이 로컬 저장소에서 **최종 인계 HEAD를 fetch하여 한 번 merge**한다. `0ccba80eef0da4703cb7ffcd1fb8e22848002b3d` 공통 계보와 기존 화면 커밋이 이미 포함돼 있다. 소유 파일을 옮겨야 한다면 공통 `0ccba80e`가 있는 통합 브랜치에서 화면 커밋 `ec9c3a06c562aea0520a0ccf3cc2c59191f5713a` → `d3acacbbda81cffade0c91f0830a22a7de456821` → `631bcf9fb6117078480beb124444134b4b050ee5` → 최종 문서 인계 커밋 순서로 적용한다. 두 방식 중 하나만 사용한다. 공통 merge 커밋 `dd40fdef`/`00affe324`를 별도로 cherry-pick하지 않는다.
- 변경 소유 경계: `RiftVictoryWindow.cs`, `.Loot.cs`, `.CasualSkin.cs`/meta, `RiftVictorySkinTests.cs`/meta, `Docs/Implementation/Casual_Rift_Victory_Skin_20261014*`와 `CasualRiftVictoryEvidence20261014/`. 공통 파일 직접 수정0. 통합/공통 부품의 미완료 경계는 아래에 남겼다.
- 원화 경로: 공통 `Assets/HELLSCRIPT/Resources/Art/Casual/Equipment/{ID}.png`와 `tools/casual_equipment_art_receipt.json`, `CasualEquipmentArt.Provider`. 장비359 PNG/69,753,836 bytes는 공통 공급 한 벌을 사용하고 화면 폴더에 복사하지 않는다. sparse clone에는 PNG0. 비장비52종은 `CasualRiftVictoryEvidence20261014/approved-item-map.json`의 승인 원본 경로·SHA만 인계하며 런타임 매핑은 미완료다. 기존 전역 CurrencyIconView/JewelerArt/RuneItemArt 교체는 하지 않았다.
- 남은 연결: `RiftReviewDetailWindow`/`IncomingDamageRanking`, `ItemDetailPopup` 외곽·도움말·footer, 보상 reveal/box 창 및 비장비52종. 카드·차트·장비 provider 후속 연결은 모두 끝났으며 모델 용량 오류 때문에 남은 코드 작업은 없다.

### 통합 실행자가 사용할 검사와 스모크 진입점

관련 EditMode 필터는 `Hellscript.Tests.RiftResultTests;Hellscript.Tests.RiftVictorySkinTests`다. 전자는 임시 GUID 세이브, 후자는 임시 GameObject를 쓴다. 공통 의존 필터 `Hellscript.Tests.CasualChartSkinTests;Hellscript.Tests.CasualUiSkinTests;Hellscript.Tests.EquipmentArtTests`는 전체 통합 입력에서 중복 없이 묶는다. 기존 정적 컴파일/15개 계약 검사 근거를 유지하며 이 인계 턴에서는 재실행하지 않았다.

실제 화면 경로는 `GameUI.ShowResult()` → `ShowRiftVictory()` → `RiftVictoryWindow.Open()`이다. 기존 `RuntimeRiftVictorySmoke.cs`의 Development-only fixture는 stage24/seed47021에서 지급·분해·결과 완료 후 실제 경로를 연다. 통합 담당이 만드는 **같은 최종 Development 플레이어**에서 다음 기존 진입점을 사용할 수 있다. 이는 개별 빌드/실행 재개 요청이 아니다.

| 통합 검증 항목 | 기존 시작 인자 | 성공/실패 근거 |
|---|---|---|
| 배치·스크롤·접기·결과 저장/중첩 복귀 | `-hellscriptRiftVictorySmoke` | `runtime.txt`, `HELLSCRIPT_RIFT_VICTORY_RUNTIME_OK`, PNG; 예외는 `failure.txt`/exit1 |
| 다음 단계·장착·첫보상·그래프 tooltip·재도전 취소 | `-hellscriptRiftResultActionsSmoke` | `progress.txt`, `runtime-actions.txt`, `HELLSCRIPT_RIFT_RESULT_ACTIONS_OK` |
| 실패/보조 스킬 분기 | 첫 행에 `-hellscriptRiftDefeatSmoke` 또는 `-hellscriptRiftUtilitySmoke` | 해당 상태 캡처와 runtime 근거 |
| 별도 프로세스 저장 복원 | 첫 행에 `-hellscriptRiftVictoryRestart`, 직전 검증의 **일회용** save만 재사용 | `restart.txt`, `restart-persisted.png` |

모든 진입점에는 `-hellscriptSavePath <새 전용 save 디렉터리> -hellscriptEvidencePath <전용 증거 디렉터리>`가 필요하다. native 무음은 자체 구현되지 않았으므로 통합 담당이 실행 전에 그 일회용 save의 `hellscript-audio-v1.json`을 `{"version":1,"master":0,"music":0,"effects":0,"muted":true}`로 준비하고 확인한다. 사용자 설정/세이브는 사용하지 않는다. EditMode에는 기존 `ValidationAudioSilence`가 있다. 여러 자동 fixture 시작 플래그를 한 프로세스에 합쳐 서로 상태를 덮게 하지 말고, 통합 실행자가 같은 바이너리의 검증 단계 순서를 조율한다.

**기존 스모크 한계/수정 요청:** 기본 크기 목록은 440×956/956×440/1600×900/1600×1000/2100×900 KO/EN이며 **2868×1320이 없다**. 해당 크기·safe area·영문/공통 Fit 결과는 최종 통합 검증에서 추가 확인한다. `CaptureOnly`는 캡처만 하므로 합격 검사 대용이 아니다. `RuntimeRiftVictorySmoke.ItemArt.cs:142`의 `EquipmentArt.SetEmblem(detail.Item)` 구 원화 기대는 provider를 적용한 상세와 충돌할 수 있다. 통합 담당이 승인 provider 기준으로 검사 의미를 보존해 조율해야 하며 실패를 제외하지 않는다. 스모크 파일은 이 인계에서 수정하지 않았다. 현재 실제 게임 캡처0/Unity 실행0이며 시안 캡처 Library ID는 아래와 같다.

화면 전용 후보에 승인된 후속 공통 API `0ccba80e`의 장비 원화 provider·스킬 기여도 카드·전투 그래프를 연결했다. **독립 Roslyn C# 컴파일은 통과했고 Unity 컴파일·EditMode 실행·플레이어·시각 수용 검증은 아직 실행하지 않았다.** 부모의 단일 검증 큐에 제출할 상태이며 화면 전체 완료를 뜻하지 않는다.

## 기준과 실제 확인 범위

- 기준 브랜치 `codex/casual-skill-skin`, 게임 코드 `4139a77e2df495cee009c07c6094b2eea9b1aeaa`, 문서 포함 기반 `2e193c336c6176d80b9ddbdff7269246bf2590fb`.
- 초기 화면 후보 `ec9c3a06c562aea0520a0ccf3cc2c59191f5713a`를 보존하면서 승인 공통 API `cdb9c140fbf8b778bd3c0bbc0fbcc68a5d9e3fa8`를 merge `dd40fdef57b3b4c2fbd899c1fd7f9b1cf26eef16`로 합쳤다. 이 문서는 그 API 연결 변경을 설명한다.
- 첫 연결 `d3acacbbda81cffade0c91f0830a22a7de456821`을 보존하며 후속 `0ccba80eef0da4703cb7ffcd1fb8e22848002b3d`를 충돌 없이 merge했다. 후속의 상세 카드 기존 크기/간격 복원, 탭 대비, Noto 기호 보강도 함께 수용했다.
- 전용 소스 clone: `/Users/t8g-2410-pn-005/Documents/Codex/2026-10-14/task-19/rift-victory-skin`, 브랜치 `codex/casual-rift-victory-skin`.
- 시안: https://claude.ai/artifact/3bhDep2vbzo24T3qA5Fjo9. 제시32/로드된32/DOM·접근성 내용 확인32. 확대 픽셀 검토1(PC16:9 KO), 성공 동작 확인6. 32개 모두 시각·동작 검증했다고 주장하지 않는다.
- 실제 시안 PNG: `task-19/evidence/claude-rift-victory-pc-ko.png`, 1600×900, SHA256 `bc49e860ee4e3007807bc04a12d122d940d112cfa307a923befb4fdcd8e5d68e`. Library `libfile_970e3974cd548191b654fb0d065628ea`.
- 브라우저 재진입은 사용자 지정 `dakrdong@gmail.com` Chrome 프로필 확인 후에만 한다. 이번 구현 턴에서는 다시 열지 않았다.

## 전후 변화 — 소스 비교, 게임 캡처 비교 아님

| 대상 | 이전 | 구현 후보 |
|---|---|---|
| 결과 배경 | 검은 성소 원화와 검은 셸 | 공통 UiSkin의 하늘·크림 배경, 밝은 기록 패널. 원화 파일은 보존 |
| 패널 | 얇은 금색 사각 테두리 | 둥근 크림색 면·남색 테두리, 주 패널 아래 그림자 |
| 버튼 | 공통 검은 금색 면 | 공통 Casual chrome: 녹색 보상·흰 재도전/일반·코랄 닫기, 정지/귀환의 경고색 글자, 비활성·hover·눌림·키보드 focus |
| 글꼴 | 기존 Display/Body | 제목/숫자/버튼 Jua, 일반 본문 Noto400, 명시적 강조700; 빠진 기호는 Noto |
| 전리품/상세/비교 | 검은 공통 카드·사각 viewport | 공통 skin 인자를 슬롯·상세·비교에 전달, 등급 의미와 Item 입력 유지, 둥근 viewport clip |
| 장비/통계/그래프 | 기존 장비 원화·검은 통계/그래프 | 승인359종 provider 기본 연결, 스킬 기여도·전투 그래프/DPS·활동시간에 같은 skin 전달. 실제 렌더는 대기 |
| 기능 | 다음 단계·그래프·실제 지급·비교·장착·저장 재시도 | 기존 트랜잭션/선택/뒤로가기/재도전 취소 경로 유지 |

화면 자체는 폰트 교체 전 기존 폰트로 `Fit`과 너비·위치를 계산한다. 공통 버튼 생성 직후 baseline 폰트를 복원해 기존 계산을 마친 뒤 새 폰트를 적용하며 추가 축소하지 않는다. 반복 설정 말풍선에도 같은 경계를 사용하고 저장 아이콘의 위치는 기존 폰트로 측정한 상태별 폭을 사용한다. 화면의 기존 `fontSize` 식과 `UiLayout.Place` 호출을 보존했다. 공통 상세·비교·스킬 카드·차트는 해당 API의 Noto 기본 폰트와 기존 내부 fitting 정책을 사용하므로 실제 결과 글자 크기/잘림을 별도 확인해야 한다. 화면 담당이 추가 축소를 넣지 않았다는 사실은 모든 공유 자식의 실제 fontSize 동일성을 뜻하지 않는다. 새 폰트의 실제 잘림·줄 높이·라틴 문구는 실행 검증 대기다.

## 소유 파일과 공유 의존

- 변경: `RiftVictoryWindow.cs`, `RiftVictoryWindow.Loot.cs`.
- 추가: `RiftVictoryWindow.CasualSkin.cs`와 meta, `RiftVictorySkinTests.cs`와 meta, 이 한/영 문서·증거 JSON.
- 이 작업의 공통 파일 직접 수정0. 승인 merge로 공통 소유자의 변경을 받아 사용한다. 화면 전용 `RiftVictoryCasualButton`과 상태 polling을 제거하고 `UiButtonChrome.Casual/CasualClose`에 시각 상태를 맡겼다. 입력·콜백·버튼 hit rect는 유지한다.
- `ContentWindowView.Open(...,skin:...)`, `StorageSurface.SetSkin`, `UiSkin.Clip`, `EquipmentSlotView`, `ItemDetailView`, `EquipmentComparisonView`를 연결했다. 슬롯의 Item chrome도 적용했다. ScrollRect는 기존 raycast frame에 두고 Clip 자식을 viewport로 지정해 드래그 전달과 기존 좌표계를 보존한다. 투명 링/의미색 띠만 공유 ShapeGraphic으로 화면 장식을 그린다.
- `RiftVictoryWindow.Open`의 선택 인자 UiSkin을 root→중첩 창→슬롯/상세/비교 및 카드/그래프로 전달한다. 기본은 승인된 `UiSkin.Casual.WithEquipmentArt(CasualEquipmentArt.Provider)`를 한 번 생성해 재사용한다. 화면별 Resources 경로/ID 맵을 복제하지 않았다. 새 Muted의 대비 계산은 화면에서 정적 캐시하여 매 프레임 반복하지 않는다. 고정430개 look/캐시 객체는 수정하지 않는다.
- `RiftSkillShareView.Card` 두 호출과 `RiftCombatGraphWindow.Open`에 skin을 전달했다. 그래프 내부 DPS/활동시간/tooltip은 공통 API가 연결한다. `ConfigureCombatSkin`·`ConfigureEdictSkin`은 다음 HUD/칙령 생성 범위를 설정하므로 이 결과 창에서 호출하지 않는다. 기존 editEdict callback과 HUD 소유권은 유지한다.
- 남은 공통 경계: `RiftReviewDetailWindow`/`IncomingDamageRanking`, `ItemDetailPopup` 외곽/범위 도움말/footer, 보상 상자/reveal 및 다른 중첩 공유 창. 해당 화면/공통 API와 소유 범위가 아직 제공되지 않아 기존 표현을 유지한다.

## 원화와 시안 차이

공통 소유자의 장비359종 provider를 기본 스킨에 연결했다. [공통 공급 영수증](../../tools/casual_equipment_art_receipt.json)의 실제359 PNG SHA/GUID·원장4개 SHA를 읽기 대조해 모두 일치했고 69,753,836 bytes다. item key330/emblem30, B28→B32 경로는 공통 소유자 계약을 그대로 사용한다. 검증 요약은 `CasualRiftVictoryEvidence20261014/equipment-provider.json`. 소스 전용 sparse clone에는 PNG를 materialize하지 않았고 Git 트리와 공통 공급 경로로만 확인했다. Unity import/대표 렌더 대기이며 사용 여부와 무관한 빌드 Resources 증가량을 숨기지 않는다.

장비 이외 관련 아이템52 PNG의 ID·경로·SHA는 `CasualRiftVictoryEvidence20261014/approved-item-map.json`, 합48,069,265 bytes다. **이 화면의 새 원화 import0·중복 복사0·imagegen0·삭제0**. CurrencyIconView/JewelerArt/RuneItemArt의 승인 런타임 매핑은 여전히 부모/단일 소유자 의존이다. 금색/재료/보석 등을 임의 전역 교체하지 않았고 룬의 실제 polyhex 의미는 유지한다.

시안은 하단3동작이나 게임은 ‘다음 레벨’을 포함한4동작이다. 시안의 표 비교/읽기 전용 아이템/보상→스킬 이동/8초 표시와 달리 실제 게임은 그래프/보유 장비 비교·장착/첫 보상 상자 지급/5초 취소 가능한 재도전이다. 이 차이는 기능 삭제로 맞추지 않았다. 재도전은 공통 Action 역할의 흰 버튼으로 표시되며 시안/초기 후보의 노란 면과 다르다. 노란 선택색을 얻으려고 실제 선택 상태를 조작하지 않았다. 기존 슬롯 크기와 전체4열/세로배치는 유지하므로 시안의 구조·크기와 완전 동일하다고 주장하지 않는다. 단계 배지의 별빛 장식도 현재 기존 두 링의 둥근 선으로 남아 있어 시각 충실도 검토가 필요하다.

## 검사와 남은 큐

- `check_ui_contract.py --verify`: 후속 API 연결 최종 소스 입력으로 15 tests PASS, 28.332초. 증거 `ui-contract.json/.log` 참조.
- `check_ui_refresh.py`: PASS,16 bindings guarded/193 source files inventoried.
- 소스 보존41항목 해결: 관련28개 메서드 본문 byte 동일, Open/Graph/SkillGrid/Inspect/CompareLoot 5개는 raw 비교 실패를 보존하고 승인 API 인자·기본 font 전달만 정규화하면 동일함을 검증했다. 배치 호출·fontSize 식·action ID·look 카탈로그 등8항목도 PASS. 실패 항목/정확한 정규화는 `source-contract.json`에 남겼다.
- 독립 Roslyn 정적 컴파일: Runtime834소스/296참조, Tests259소스/308참조, 모두 오류0. 경고 Runtime47/Tests2건을 log에 그대로 보존했다. 공유 Unity6000.6.0f1 참조를 읽기 전용으로 사용하고 Tests는 이번 Runtime.ref를 참조했다. 기존 Editor.ref 재사용이므로 Editor 소스 컴파일은 아니다. `static-compile.json` 및 task-19/evidence/roslyn-common-followup의 RSP/전체1093소스·440참조 SHA/log 참조. Unity나 테스트를 실행한 결과가 아니다.
- 독립 검토에서 기호 누락, 슬롯 대비, 측정 시점, 구분선 rect, 비활성 잉크, 슬롯 focus 링 순서를 발견하고 수정했다.
- `RiftVictorySkinTests`5 cases는 컴파일 통과: 버튼 rect/입력·클릭 차단, 공통 Casual face의 hover/focus, Jua/Noto 기호와 글자 크기/rect 보존. **실행0**.
- 부모 큐 최소 필터: `Hellscript.Tests.RiftResultTests;Hellscript.Tests.RiftVictorySkinTests`. 후속 공통 의존은 `CasualChartSkinTests;CasualUiSkinTests;EquipmentArtTests`의 동일 통합 입력 근거를 조율한다. 전체 회귀를 개별 화면마다 반복하지 않는다.
- 실행 입력은 이 브랜치 전체 SHA와 공유 통합 SHA를 부모가 고정한다. 사용자 세이브 대신 일회용 세이브, 무음 macOS 플레이어 한 개. 예상 핵심 캡처1–5MB; 별도 Library/Unity 캐시/앱 복제 없음.
- KO/EN × 세로440×956·가로956×440·PC1600×900/1600×1000/2100×900 및2868×1320. 비활성/선택/빈 목록/기록 없음/분해/버림/저장실패/첫보상/중첩 복귀/스크롤·접기/재도전취소/다음단계 확인.
- 2868×1320의 DPR3·safe area 좌우102px/상0/하54px는 가정. Mac 시뮬레이션을 iOS 실기기 검증으로 부르지 않는다. 게이지·카드·원형 경계·모서리·영문 잘림도 실행 대기다.
- 실제 게임 before/after 캡처0. 현재 Library 파일은 시안 근거1개다. 실행 후 게임 캡처를 별도로 올리고 실제 ID를 추가해야 한다.

## 공통 규칙 제안과 자원 수명

제공된 공통 API로 셸/버튼/슬롯의 중복 어댑터를 제거했다. 다음 공통 규칙 후보는 일반 UI 단위1과 스킬 전용2× 단위 구분, 기존 측정 결과/히트 영역 보존 정책, 실제 선택과 무관한 강조 버튼 색 정책이다. 전투 그래프 API는 연결 완료했고 남은 전투 상세/피해 순위 및 아이템 팝업 외곽/도움말 API를 공통 소유자에게 요청한다. 이번 작업이 공통 파일을 선점하지 않는다.

소스 전용 공유 object clone을 유지한다. 중단됐던 최초 sparse checkout은 전용 clone에서만 복구해 clean 기반으로 시작했으며 원본/타 작업의 파일·index·브랜치는 바꾸지 않았다. 디스크 절감 성과는 주장하지 않는다. 초기 `Builds/Validation/UIContract`는 소형 static JSON/log이며 Unity 빌드가 아니다. 독립 Roslyn 산출물은 task-19/evidence/roslyn-common-api에 약13.3MB로 제한하고 공유 Library/Builds를 복제하지 않았다. 산출물은 부모 통합과 검증 완료까지 보존한다. 외부 Unity를 실행/중단/조작하지 않았다. 원격 push/main 병합/웹 빌드/공개 배포 없음.

적용한 공통 API 기준: `0ccba80eef0da4703cb7ffcd1fb8e22848002b3d`. 기존 약13.3MB Roslyn 근거와 후속 약13.3MB 근거는 각 디렉터리에 보존한다. 이 화면은 Unity를 실행/중단하지 않았다. 개별 큐는 종료됐고 모든 실행은 단일 통합 담당에게 이관한다. 이후 편집은 통합 담당의 구체적인 결함 요청이 있을 때만 재개한다.
