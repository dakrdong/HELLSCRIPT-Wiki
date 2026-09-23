# 대장간 코어 제작

갱신일: 2026-09-23 · [English](Core_Crafting.en.md)

## 게임 규칙

대장간의 네 번째 **코어 제작** 탭에서 부위와 제작할 전설·세트 장비를 선택한다. 기존 마을 대장장이의 접근 범위와 콘텐츠 해금 판정을 사용한다. 희귀 제작·옵션 변경·장비 강화·슬롯 강화는 유지한다.

- 전설·세트 장비 1개를 분해하면 같은 부위 코어 1개를 지급한다. 기존 단건·일괄·자동 분해의 `Economy.Dismantle` 경로와 일반 재료·강화석·위상 보상을 유지한다.
- 코어는 기존 `AccountSave.cores[8]`의 계정 공용 재화다. **인벤토리·창고 공용**은 이 실제 합계를 한 번 읽는다는 뜻이며 별도 스택이나 중복 잔액을 만들지 않는다. 두 반지 위치는 반지 코어를 공유한다.
- 제작비는 해당 부위 코어 10개와 선택한 심연 주화 0~8,000개다. 골드·일반 재료는 사용하지 않는다. 결과는 현재 캐릭터의 가방에 저장하며, 공간이 없으면 차감하지 않는다.
- 제작 레벨은 기존 제작 규칙대로 현재 캐릭터의 최고 실제 클리어 단계를 사용하며 Lv.1~60으로 제한한다. HTML의 Lv.30은 시연값이다.
- 후보는 활성 `ItemCatalog.Uniques`를 직접 조회한다. 전설·세트·직업·이름으로 필터링하며 다른 직업의 장비도 제작할 수 있다. 비활성 신규 스킬 기획을 활성화하지 않는다.
- 레시피 ID로 베이스와 접사 종류를 고정한다. 접두 2개·접미 2개, 부위 적합성, 중복 그룹 금지와 기존 직업 가중치를 적용한다. 재진입·언어 변경으로 옵션 종류를 다시 추첨하지 않는다. 필드 드롭의 기존 추첨은 바꾸지 않는다.
- 심연 주화 `D`개를 투입하면 각 접사의 품질을 **D~10,000 정수**에서 독립적이고 균등하게 추첨한다. 0개는 0~100%, 1개는 0.01~100%, 8,000개는 80~100%다. 최대값도 포함한다. 수치는 `AffixDefinition.Value`로 계산한다. 각성·상위 접사·걸작·고유/세트 효과는 주화로 변경하지 않는다.

## 화면과 공통 UI

`BlacksmithWindow`의 기존 어댑터에 탭을 확장한다. 별도 창·캔버스는 추가하지 않는다. 가로·PC는 **코어 목록 / 장비 목록 또는 상세 / 품질 조절**의 1:1:1 배치다. 세로는 코어 목록 → 장비 선택 → 제작 설정을 반대 방향 슬라이드로 왕복한다.

선택한 코어 행 오른쪽에 **장비 제련**과 `보유 수량 / 10`을 표시하고 별도 하단 제련 버튼은 두지 않는다. 제작 설정의 장비 이미지·이름과 옵션 범위는 좌우에 배치한다. 게이지 터치·드래그와 −500·−100·−50·+50·+100·+500 버튼으로 주화를 조절하며 숫자 입력칸은 없다. 보유량과 8,000개 중 작은 값을 넘을 수 없다. 눈금은 0~100%를 유지하고 선택한 최소 품질부터 100%까지 칠한다.

`EquipmentSlotView`, `EquipmentGradePalette`, `ItemDetailView`, `ItemTooltip`, `UiTheme`을 공유한다. 미리보기는 `EquipmentViewSource.Catalog`로 범위만 표시하고 결과에는 `EquipmentViewSource.RewardSnapshot`으로 저장된 아이템을 전달한다. 재화 아이콘은 기존 인벤토리의 `Art/GlobalHUD/currency-abyssal-coin`을 재사용한다. 새 래스터 리소스나 리소스 GUID를 만들지 않는다.

이미지 상세는 읽기 전용이다. 팝업 외곽 크기는 같은 화면 조건에서 고정하고 긴 본문만 스크롤한다. 위·아래 닫기, 바깥 클릭과 Esc를 지원한다. 필터·선택·투입량·목록 위치는 화면 방향과 언어 변경에도 유지한다. 창·입력 차단은 `ContentWindowHost`, 저장 후 갱신은 `StoreViewBinding`을 사용한다.

## 거래와 저장

`CoreCrafting`이 정의·비용·품질 추첨을 소유하고 `GameStore.CraftCoreEquipment`가 `Transact`에서 실행한다. UI는 저장 데이터를 직접 변경하지 않는다. 실행 시 캐릭터·레시피·레벨·옵션 구성·재화·가방 공간·해금·균열 진행 여부를 다시 검사한다.

코어·주화 차감, 가방 장비, 제작 기록과 미확인 결과 ID를 한 번에 저장한다. 저장 실패는 원래 계정을 유지하고 성공 알림을 보내지 않는다. 같은 요청 ID를 다시 실행해도 추가 차감하지 않는다. 결과 확인 전 추가 제작은 막으며, 재접속 후 코어 탭에서 동일 결과를 다시 연다. 확인 저장에 실패하면 결과창을 유지한다. 창 닫기·게임 종료는 재추첨을 실행하지 않는다.

통합 스키마 11의 `AccountSave.coreCraft`에 제작 결과의 복사본과 미확인 결과 ID를 보관한다. 기록의 수치는 실제 장비의 후속 강화·분해와 무관하게 보존한다. 기존 저장에는 빈 기록만 추가하며 잔액·장비·성장·작업과 다른 콘텐츠 상태를 유지한다. 이전 게임 버전이 새 기록을 없애지 않도록 기존 미래 스키마 보호를 적용한다.

## 검증 경로

`CoreCraftingTests`와 기존 `BlacksmithTests`로 정의·범위·독립 추첨·경계값·거래·이전 저장·분해 지급을 검사한다. macOS 개발 빌드는 별도 저장 경로에서 `RuntimeCoreCraftingSmoke`를 실행한다.

```text
-hellscriptCoreCraftingSmoke -hellscriptSavePath <isolated-directory> -hellscriptScreenshots <evidence-directory>
```

같은 저장 경로에 `-hellscriptCoreCraftingVerify`를 추가한 별도 프로세스로 미확인 결과 복원과 확인을 검사한다. `main`의 인벤토리 개선분 `ff25bae`까지 합친 뒤 [통합 검사 130/130](BlacksmithEvidence/core-crafting-main-editmode.xml)도 통과했다. 아래 검사와 범위가 겹치므로 개수를 합산하지 않는다.

2026-09-23 실행 결과: 관련 Unity Edit Mode **354/354**, 공통 UI 검사 **9/9**, HTML 시연 검사 **103/103**을 통과했다. macOS 개발 빌드가 성공했고, 별도 저장 파일을 사용하는 실제 실행본에서 코어 20개·심연 주화 8,000개 차감과 전설·세트 장비 2개 지급을 확인했다. 별도 프로세스에서 미확인 결과 복원과 확인도 통과했다.

세로 440×956·가로 956×440·PC 1600×900 / 1440×900 / 1680×720에서 한국어·영어, 100% 글자와 세로·가로·PC의 150% 글자를 검사했다. 합성 포인터·키보드 입력으로 검색·직업 필터, 바깥 클릭·Esc·닫기, +/− 버튼·게이지, 동일 너비 열과 선택값 유지를 확인했다. 모바일 실기기·노치 안전 영역·모바일 성능은 미검증이다. 개발 실행본 시작 시 기존 URP 후처리 셰이더 경고는 남아 있으며 코어 제작 실행 예외는 없었다.

근거: [검사 XML](BlacksmithEvidence/core-crafting-editmode.xml), [검증 요약](CoreCraftingEvidence/validation.json), [실행 결과](CoreCraftingEvidence/result.txt), [재시작 결과](CoreCraftingEvidence/restart-result.txt), [세로 제작](CoreCraftingEvidence/portrait-ko-forge.png), [가로 영어](CoreCraftingEvidence/landscape-en-forge.png), [PC 장비 목록](CoreCraftingEvidence/pc-16x9-ko-catalogue.png), [확대 글자](CoreCraftingEvidence/large-text-portrait.png).

구현: [규칙](../../Assets/HELLSCRIPT/Runtime/Core/CoreCrafting.cs), [거래](../../Assets/HELLSCRIPT/Runtime/Core/GameStore.CoreCrafting.cs), [탭](../../Assets/HELLSCRIPT/Runtime/Presentation/BlacksmithWindow.Cores.cs), [팝업](../../Assets/HELLSCRIPT/Runtime/Presentation/BlacksmithWindow.Cores.Dialogs.cs), [자동 검사](../../Assets/HELLSCRIPT/Tests/Editor/CoreCraftingTests.cs), [실행 검증](../../Assets/HELLSCRIPT/Runtime/Presentation/RuntimeCoreCraftingSmoke.cs).

공개 위키는 이 브랜치를 `main`에 병합할 때 병합된 원본에서 재생성·검증한 뒤 게시한다.
