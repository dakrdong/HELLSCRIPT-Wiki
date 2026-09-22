# HELLSCRIPT HTML 기준 창고 UI 적용

갱신일: 2026-09-22 · [English](Storage_Reference_Parity.en.md)

첨부 HTML과 달랐던 창고 화면을 기존 게임의 uGUI 안에서 다시 구성했다. 가로는 창고·이동 조작·소지품 순서, 세로는 같은 순서의 위아래 배치이며, 양쪽 목록은 모두 8열 정사각 슬롯이다. 실제 장비·보석·룬 데이터와 기존 창고 거래를 연결한 게임 화면이다.

## 기준과 적용 범위

기준 자료는 `HELLSCRIPT-Storage-Reference.html`과 `HELLSCRIPT-Storage-Development-Prompt.md`다. HTML의 CSS·DOM·상태별 표현을 읽고 게임에 필요한 부분을 옮겼다. HTML 파일의 브라우저 미리보기는 도구의 로컬 파일 정책으로 차단되어 수행하지 못했다. 따라서 HTML 렌더 결과와의 자동 픽셀 비교나 완전한 픽셀 일치를 주장하지 않는다. 실제 게임 화면은 macOS 개발 빌드에서 캡처해 확인했다.

| 자료 | SHA-256 |
|---|---|
| HTML | `3c8e734198e77163c9ed541a77e71371e8f6de98b515cdd0572dfacfc48f3a49` |
| 개발 기획 | `97b0bf000ec9b2f20169032aebba884f0eee11afc718831c5d5bde803b6984be` |

HTML의 예시 아이템·예시 이미지·로컬 저장 코드·시연 버튼은 게임 데이터로 사용하지 않았다. 화면 확인에 쓰는 계정은 개발 빌드의 명시적 스모크 옵션으로만 만들며, 별도 임시 저장 경로를 사용했다. 사용자 세이브는 초기화하거나 테스트 재화로 바꾸지 않았다.

## 외형과 배치

| 항목 | 적용 내용 |
|---|---|
| 바탕·경계 | 차콜·올리브 바탕, 세로 그라데이션, 가는 황동 테두리. `StorageSurface`가 uGUI 메시로 그린다 |
| 색상 | 바탕 `#1a1c16`, 황동 `#c0a777`, 본문 `#d9d1be`, 보조 글자 `#9b9689`, 실행 버튼 `#702e28`→`#461b18` |
| 머리말·바닥 | 가운데 창고 제목, 왼쪽 HELLSCRIPT·계정 공용, 오른쪽 닫기. 바닥에는 재화와 기록·안내 |
| 보관함 탭 | I~V, 보관함 이름, 잠금·비용·선택 밑줄. 연필 버튼에서 이름 변경 |
| 분류·필터 | 분류는 얇은 밑줄 탭. 등급은 일반·마법·레어·전설·세트 다중 체크 오버레이. 정렬은 별도 펼침 목록 |
| 슬롯 | 8열 정사각형, 간격 3 논리 단위, 실제 장비 아틀라스, 절제된 등급 테두리. 확장은 마지막 칸 뒤의 점선 슬롯 |
| 이동 조작 | 가로에서는 가운데 세로 막대, 세로에서는 두 목록 사이 한 줄. 방향 문양과 일괄 이동 버튼 |
| 상세·구매 | 큰 실제 장비 아이콘, 이름·능력치·비교, 암적색 실행 버튼. 확장은 전후 정원·비용·보유·승인 후 잔액 표시 |
| 반응형 | 실제 안전 영역에서 배율과 논리 크기를 계산한다. 머리말·탭·필터·이동 조작은 고정하고 두 목록만 각각 스크롤한다 |

장비 이미지는 기존 `EquipmentAtlas`의 6×4 셀을 그대로 쓴다. 창고·가방·화살표·체크·재화 문양은 `StorageGlyph`가 해상도에 맞춰 그린다. 새 래스터 에셋·패키지·씬·프리팹은 추가하지 않았으며, 기존 에셋과 GUID를 유지했다. 제목은 사용 가능한 명조 계열 동적 글꼴을 쓰므로 운영체제별 글꼴 모양은 달라질 수 있다. 개발 빌드에서만 오른쪽 아래 워터마크 공간을 확보한다.

## 실제 게임 연결과 저장

| 담당 | 연결 지점과 처리 |
|---|---|
| 진입·일시정지 | `GameUI.Storage` → `StorageWindow`; 기존 마을 창고·콘텐츠 독·가방 진입을 유지하고 닫을 때 이전 정지 상태 복원 |
| 실제 아이템 | `ItemCatalog`, `ItemGenerator`, `Item.id`, `EquipmentAtlas`; 슬롯·상세·드래그·프리셋이 같은 인스턴스 참조 |
| 보석·룬 | `GemInventory`, `GemCatalog`, `RuneMasteryCatalog`, `RuneBoardGraphic`; 실제 종류·효과·점유 모양을 표시 |
| 이동·구매 | 기존 `GameStore.Storage`와 `Storage`의 위치 검증·원자적 거래·정원·재화 규칙 사용 |
| 이름 변경 | `AccountSave.warehouseNames`를 추가하고 `GameStore.RenameStorageTab`으로 저장. 열린 탭에만 1~12글자를 허용하고 제어 문자는 거부 |
| 이전 저장 | 이름 배열이 없거나 짧으면 기본 이름으로 표시하고 변경 시 보완. 다른 거래 후에도 이름이 유지되며 기존 장비·위치·정원은 변경하지 않음 |
| 기록 | 실제 `AccountSave.transactions`에서 최근 창고 거래 30건을 표시. 이름 변경·이동·교환·구매·프리셋 등의 영수증 사용 |

`StorageWindow.cs`는 배치, `.Style.cs`는 표현, `.Dialogs.cs`는 상세·구매·프리셋, `.Tools.cs`는 정렬·이름·기록·안내, `.Drag.cs`는 입력을 담당한다. 언어 변경 뒤 계속 다시 그리던 문제도 수정했다. 기존 데이터·구매·프리셋 정책의 상세는 [창고 UI 구현 기록](Storage_UI.md)을 따른다.

## 드래그 아이콘과 즉시 집기 수정

2026-09-20 후속 요청에 따라 창고와 소지품 목록의 드래그 입력을 수정했다. 아이템 위에서 누른 채 움직이면 첫 움직임에 바로 집히며, 0.24초 길게 누르기와 이동 거리 문턱을 사용하지 않는다. 움직이지 않고 눌렀다 놓으면 상세창이나 일괄 선택이 동작한다. 빈칸에서 쓸기, 스크롤 막대와 마우스 휠로 목록을 스크롤한다.

드래그 중에는 ‘n번 빈칸에 배치’와 같은 빈칸 번호 안내를 표시하지 않는다. 아이템 이름·목적지와 교환·이동 불가·탭 전환 안내는 유지한다. [빈칸 안내를 숨긴 화면](StoragePointerEvidence/drag-portrait-no-slot-hint.png)을 보관한다.

2026-09-22 추가 요청에 따라 아이템을 놓아 이동에 성공했을 때 하단에 표시하던 목적지·칸 번호 알림도 제거했다. [이동 직후 화면](StoragePointerEvidence/drop-without-position-toast.png)을 보관한다.

이전 구현은 안전 영역 프레임의 왼쪽 아래 기준 좌표를 가운데 앵커의 위치로 사용해 드래그 표시가 포인터에서 벗어났다. 입력 ID가 양수이면 터치로 간주한 것도 문제였다. 현재 Input System에서는 마우스 ID도 양수일 수 있다. 아이템을 집는 시간 판정 자체를 없애고, 아이콘 중심을 변환된 포인터 좌표에 직접 놓도록 고쳤다. 아이콘은 실제 슬롯 크기로 표시하고, 이름·목적지·드롭 안내 상자만 화면 안으로 조정한다. 아이콘과 안내 상자는 드롭 대상의 입력을 가로채지 않는다.

새 실행 검사는 마우스의 양수 ID와 터치 모두 첫 1픽셀 이동과 같은 프레임에 집히는지, 이동 중 실제 아이콘 중심과 포인터 좌표가 일치하는지, 세로 배치와 화면 가장자리에서도 따라오는지를 검사한다. 누른 채 정지한 입력의 상세창 열기, 빈칸 스크롤, 이동·교환·취소도 확인한다. 다른 보관함 탭을 여는 1초 머무르기는 별도 동작이다.

관련 Edit Mode 검사 99개가 모두 통과했다. macOS 실행 검사에서는 즉시 집기 6회와 아이콘 중심 74지점을 확인했고, 포인터와의 최대 오차는 0.000픽셀이었다. 입력은 실제 셀 핸들러에 전달한 합성 입력이다.

[가로 드래그 화면](StoragePointerEvidence/drag-landscape.png), [세로 드래그 화면](StoragePointerEvidence/drag-portrait.png), [실행 검사 결과](StoragePointerEvidence/runtime.txt), [관련 Edit Mode 검사](StoragePointerEvidence/editmode.xml)를 보관한다. 이 절의 드래그 화면이 아래 초기 구현 캡처의 드래그 표시를 대체한다. 화면 크기는 데스크톱 창에서 확인했으며 모바일 실기기 검증과 구분한다.

## 검증과 게임 화면

창고 변경만 담은 별도 체크아웃에서 `StorageTests`, `PresetStorageTests`, `InventoryLayoutTests`, `LocalizationTests`를 실행해 **99개 모두 통과**했다. 이름 저장·이전 저장 호환·다른 거래 후 유지·잘못된 이름과 잠긴 탭의 무변경을 새로 검사했다. 전체 Edit Mode 회귀 검사는 이번에 다시 실행하지 않았다.

원본 작업 폴더에서 먼저 실행한 같은 범위는 99개 중 98개가 통과했다. 실패 1개는 동시에 개발 중인 룬 파일의 미등록 번역 9개였으며, 해당 작업을 고치거나 이번 커밋에 섞지 않았다. 분리한 체크아웃의 99개 통과 결과와 구분한다.

macOS 개발 빌드와 창고 런타임 스모크 결과는 [검증 요약](StorageReferenceEvidence/verification.txt), [Edit Mode 결과](StorageReferenceEvidence/editmode.xml), [런타임 결과](StorageReferenceEvidence/runtime.txt)에 남겼다. 실제 포인터 핸들러에 합성 입력을 전달하여 지정 칸 이동·원자적 교환·길게 눌러 집기·1초 탭 전환·취소·정렬 후 드롭·일괄 이동·중복 구매 무차감·프리셋 복귀·회전 중 상태 유지·전투 정지와 복원을 확인했다. 정렬 목록·이름 변경·기록·안내와 한국어·영어 화면도 확인했다.

| 실제 게임 캡처 | 화면 |
|---|---|
| [가로](StorageReferenceEvidence/storage-reference-01-landscape-town.png) | 1600×900, 마을에서 연 창고 |
| [정렬](StorageReferenceEvidence/storage-reference-02-sort-menu.png) | 정렬 펼침 목록 |
| [안내](StorageReferenceEvidence/storage-reference-03-guide.png), [기록](StorageReferenceEvidence/storage-reference-04-history.png) | 실제 기능이 연결된 보조 화면 |
| [상세](StorageReferenceEvidence/storage-reference-05-detail.png), [비교](StorageReferenceEvidence/storage-reference-06-detail-comparison.png) | 실제 장비와 능력치 |
| [등급](StorageReferenceEvidence/storage-reference-07-grade-menu.png) | 다중 선택 필터 |
| [보석](StorageReferenceEvidence/storage-reference-08-gems.png), [룬](StorageReferenceEvidence/storage-reference-09-runes.png), [룬 상세](StorageReferenceEvidence/storage-reference-10-rune-detail.png) | 실제 계정 재고 |
| [드래그](StorageReferenceEvidence/storage-reference-11-drag-hover.png), [탭 전환](StorageReferenceEvidence/storage-reference-12-drag-tab-opened.png) | 입력과 드롭 미리보기 |
| [일괄 이동](StorageReferenceEvidence/storage-reference-13-bulk-selection.png) | 한쪽 방향 선택 |
| [확장](StorageReferenceEvidence/storage-reference-14-expand-dialog.png), [탭 구매](StorageReferenceEvidence/storage-reference-15-buy-dialog.png), [프리셋](StorageReferenceEvidence/storage-reference-16-presets.png) | 구매와 기존 8부위 장비 프리셋 |
| [세로](StorageReferenceEvidence/storage-reference-17-portrait-town.png) | 900×1600 |
| [작은 세로](StorageReferenceEvidence/storage-reference-18-portrait-phone.png), [작은 가로](StorageReferenceEvidence/storage-reference-19-landscape-phone.png) | 390×844, 844×390 데스크톱 창 크기 |
| [영어 가로](StorageReferenceEvidence/storage-reference-20-english-landscape.png), [영어 세로](StorageReferenceEvidence/storage-reference-21-english-portrait.png), [영어 125%](StorageReferenceEvidence/storage-reference-22-english-large-text.png) | 번역·글자 크기 |
| [균열](StorageReferenceEvidence/storage-reference-23-landscape-rift.png) | 전투 중 창고와 일시정지 |

게임에서는 마을 창고, 플레이 화면 콘텐츠 독의 창고, 가방 하단 창고 버튼으로 연다. 빌드 출력은 `Builds/StorageReference/HELLSCRIPT.app`이다. 자동 검증은 `-hellscriptStorageSmoke`와 별도의 `-hellscriptSavePath`, `-hellscriptScreenshots`를 지정해 실행한다.

## 확인하지 않은 범위와 기존 제약

실제 iOS·Android 기기의 터치·노치·홈 표시줄·성능은 검증하지 않았다. 위의 휴대폰 크기 화면과 터치 시나리오는 macOS 창과 합성 입력 결과다. 전체 게임 회귀 검사나 HTML 렌더 결과와의 픽셀 비교를 통과한 것으로 해석하면 안 된다.

재료·보석·룬은 기존 게임에서 계정 공용 재고이므로 창고 분류에서 조회하며 영웅 가방으로 이동하지 않는다. 장비 프리셋 적용은 기존 정책대로 성소에서만 가능하다. 유료재화 잔액은 기존 필드를 사용하며 지급·결제 경로를 새로 만들지 않았다. 테스트용 잔액은 임시 계정에만 있다.
