# 캐릭터 생성·선택 화면 인계 — 2026-10-14

> **2026-10-28 A2 정정:** 큰 칙령 창·≡ 메뉴·전용 부품·스모크 설명은 과거 기록이다. [현재 진입 코드](../../Assets/HELLSCRIPT/Runtime/Presentation/GameUI.HuntEdict.cs)와 [대체 화면·소유 코드](Hunt_Edict_Window_Removal_20261028.md)를 따른다. 이전 캡처와 검사 수치는 당시 구현의 근거다.

[English](Character_Plaza_Handoff_20261014.en.md) · [저장 영향 조사](Character_Plaza_Save_Audit_20261014.md) · [원화 공급](CharacterPlaza_Art_20261014.md)

## 범위와 의존 순서

사용자가 확정한 남녀 6영웅 저장과 마을 장면 화면을 구현했다. 원본 checkout과 실제 세이브에는 쓰지 않았다. 전용 소스 위치는 `task-21/character-plaza`, UI 분기는 `codex/casual-character-plaza`다. Library/Builds/Temp를 복제하지 않았다.

적용 순서는 저장 `e38096240d1d54f680204212f583340708d798e0` → 저장 보완 `93219e3fef4f7a07f2e3249b9598b1e0e7f014a8` → 공통 통합 `4148241a` → 이 PR2 커밋이다. PR2는 `4148241a`를 직접 선조로 삼는다. 원격 PR 생성·push·main 병합·웹 배포는 하지 않았다. 통합 소유자는 task-6/skill-skin이다. 기본 HUD 소스도 인수한 뒤 한 번의 통합 Unity 검증을 한다.

- `HeroSlots.Index(class, gender)`, `Created(account)`, `IsCreated(account,index)` 사용. 인덱스 0/1/2는 남자 전사/궁수/마법사, 3/4/5는 여자 순서다.
- schema23 이전은 이전 3명의 ID·진행·장비를 유지하고 원본 바이트를 `.schema<N>-before-six-heroes.json`에 보존한다. 같은 이름의 다른 보관본이 있으면 덮지 않고 이전을 중단한다.
- 생산 신규 계정은 남전사만 생성된다. 첫 시작의 `EnterFirstStart`는 유지하고, 다음 접속에서 나머지 다섯을 만든다. 새 레거시 개발자 계정은 기존 남자 3명을 유지하며 schema23 읽기/쓰기는 거부한다.
- 장면에서 여섯 영웅 선택 → 미생성 `+` → 1.8초 화이팅 → 원자적 생성 → Lv.1/장비 → 플레이 → 전투 준비/대사 → 기존 입장으로 연결한다. 실패는 생성 전 상태를 보존하고 재시도할 수 있다.
- 미리보기·카메라 이동·장비 열기는 저장하지 않는다. 생성/입장 중 선택·인장·뒤로 가기를 막고, 균열 소유 ID로 다른 다섯 명을 잠근다. 좌우 키는 전사 남/여 → 궁수 남/여 → 마법사 남/여 순서다.

## 전용 구현과 공통 연결

`CharacterSelectionState`가 순수 상태·1.8초 시간·12개 대사를 소유하고, `CharacterPlazaSlots`가 저장소와 화면을 연결한다. `CharacterPlazaStage`는 동일한 장면 변환으로 배경/영웅/가림 조각을 그리며, 세로 직업 카메라를 0.9초 보간한다. GPU 전용 텍스처는 64×64 비트 마스크로 클릭하고 선택 외곽선·비활성 채도는 셰이더 하나와 영웅별 재질 여섯 개로 처리한다. 프레임마다 저장/JSON 읽기/목록 재생성을 하지 않는다. 성능 향상 수치는 주장하지 않는다.

`CharacterSelectionView(.Equipment)`는 `UiSkin.Casual.WithEquipmentArt(CasualEquipmentArt.Provider)`, 공통 글꼴·버튼·StorageSurface·ContentWindowView·EquipmentSlotView·ItemDetailView를 사용한다. 장비는 실제 장착품 8분류를 표시하며 양손/반지의 두 슬롯은 같은 분류 안에서 모두 보존한다. 장비창을 닫은 Esc가 타이틀 뒤로 가기에 다시 쓰이지 않도록 공통 host 입력 플래그를 확인한다. 크기 또는 Canvas 배율이 바뀌면 배치를 갱신하고 생성/장비 버튼의 두 축을 최소44 화면 단위로 유지한다.

PR2에는 `GameUI.CharacterSelection`, `GameController.CharacterSelection`, `GameUI.ScreenSettings`의 생성된 6영웅 목록, `GameUI.ShowTownMenu`의 최대3열 두 행 목록, `HuntEdictWindow.Progression`의 성별 인덱스 fallback이 포함된다. `GameUI.Items`의 장비 무결성 순회는 여섯 소유 가방 모두를 대상으로 유지했다. 공통 담당의 `4148241a`가 `GameController.SelectHero`, D4 host/탭, 텍스처 예산을 소유하며 여기서 중복 수정하지 않았다.

## 시안·원화와 차이

정본 MD는 요청된 원본 경로에 없어서 형제 `wt-casual-art`의 커밋 `13a1ce7322ae69f5b518503b52598fbadf7ee5b8`에서 가져왔다. MD SHA256은 `c1ad2137e8f6f81a3e319dbbacea33ac102428e45175ea0012666bb3ff523f6f`이다. Chrome에서 Hoosung Lee / dakrdong@gmail.com 프로필을 실제 UI로 확인하고 그 창에만 새 탭을 열었다. Claude artifact는 제목이 표시됐으나 로드 시간초과였고 한 번 재시도 후 동일했다. 정본이 허용한 저장소 사본의 28개 화면과 소스를 사용했다. 다른 계정 탭은 변경하지 않았다.

공통 스킨의 기본 위치/크기 보존 규칙과 달리 이번 MD는 기존 3명 stage를 마을 장면·6명 좌표·직업 카메라로 교체하도록 명시한다. 이 예외를 통합 담당에게 전달하고 적용했다. 배치는 440×956 세로, 956×440 짧은 가로, 1152×648 PC 설계 단위를 안전영역에 맞춘다. 장비창은 공통 입력/창 host 위에 정본의 2열/4열 8분류를 배치한다. 두 실제 무기/반지 표시와 공통 장비 상세는 시안의 단일 예시 아이템보다 확장된 부분이다.

원본 17 PNG 및 승인된 기존 6명 원화 해시/실제 픽셀을 확인했다. 마을 배경2·자세12는 원본 PNG 바이트를 재사용한다. Vision 도구는 컴파일됐지만 첫 추출이 ANE model Code9로 실패해 기존 저작 WebP의 cut/patch/crest를 PNG로 디코딩했다. 새 이미지를 생성하지 않았다. 런타임 경로는 `Resources/Art/CharacterPlaza/`, PNG41개 **31,524,964 bytes**이다. 모든 원본/메타/구 리소스는 보존했다. 기존 `CharacterSelection` 약9.23MB는 장비창의 공유 아틀라스 소비자가 남고 실제 화면 대조 전이므로 정리하지 않는다.

남은 시각 차이: 실제 Unity 픽셀 대조, WebP cutout 가장자리 접합, 작은 마법사 확대 선명도, 공통 폰트 줄바꿈/장비 긴 이름, 터치 영역·팝업 안전영역은 통합 실행에서 확인한다. 전투 자세 그림의 무기는 고정 그림이며 임의의 실제 장착 외형으로 바뀌지 않는다. 원본의 여전사 전투 그림은 도끼 대신 검이다. 마을/자세 원화는 **productionApproved:false**이며 여전사 배꼽 노출은 MD가 요구한 **12세 등급 검토 대상**이다.

## 검증 증거와 통합 진입점

[정적 근거](CharacterPlazaSourceEvidence20261014/summary.json)에 정확한 범위를 기록했다. Runtime/Editor/Tests Roslyn 컴파일 오류0, 공통 UI 계약15개, refresh16바인딩, 원본17·런타임41 해시/새GUID·장면12좌표·마스크24, 신규 한국어22키 및 구문구8개 정리를 확인했다. Node 정본 smoke는912조합/28화면을 통과했으며 Unity 구현 실행 결과가 아니다.

**Unity import/셰이더 컴파일/EditMode/플레이어/실기기/성능 측정은 모두 실행0이다.** 저장 테스트15개 및 상태/자산 검사, 새 개발 스모크는 작성·C# 컴파일만 했다. 실제 세이브·네트워크 계정은 사용하지 않았다.

통합 담당은 모든 소스 고정 후 다음을 한 번 묶는다.

1. 전체 EditMode 안에 `HeroSlotsTests`, `CharacterSelectionTests`, `LocalizationTests`, `BuildResourceTests`, `TutorialHeroSlotsTests`, `SettingsRevisionTests`, D4 host 테스트를 포함한다. 생성된/미생성 계정 집계와 이전 실패 보존도 확인한다.
2. 동일 개발 빌드의 `-hellscriptCharacterSelectionSmoke -hellscriptSavePath <새 일회용 저장 경로> -hellscriptScreenshots <근거 경로> -hellscriptServerBase invalid -nosound` 진입점. 새 남전사1명 fixture만 허용한다. 5명 생성과 여마법사 입장, 다섯 창 비율, KO/EN, 장비/회전, 균열 잠금과 무음 캡처를 포함한다. Google 인증은 기존 계정 검증 흐름에서 별도로 확인한다.
3. 최종 실제 입력은 생성·장비 Esc·플레이·뒤로·급속 선택·세로 인장 카메라를 확인한다. Mac 모의 안전영역을 실기기 검증으로 표현하지 않는다. 원본 reference와 나란히 픽셀/줄바꿈을 확인한 후에만 구 리소스 정리를 판단한다.

소스 동결 후 통합 담당만 변경한다. 검증 스크립트/로그/Swift 실패 근거는 작업폴더 `validation/`과 작업 상위 `artifact-lifecycle.json`으로 보존한다. 영구 삭제나 복구 불가능 정리는 실행하지 않았다.
