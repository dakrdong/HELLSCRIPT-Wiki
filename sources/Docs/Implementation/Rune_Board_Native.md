# 룬 보드 네이티브 UI 이식

작성일 2026-10-01 · [English](Rune_Board_Native.en.md) · [시안](../../Prototypes/RuneBoard/HELLSCRIPT-RuneBoard.html) · [시안 설계](../Design/Rune_Board_UX_Redesign.md)

## 범위와 기준

이번 변경은 기존 룬 콘텐츠의 화면 개선이다. 무기 6종, 무기별 259칸, 연결·배치 검증, 34개 블록 모양, 획득 등급 G0–G6, 전투 효과, 몬스터 드롭·합성 경제, 저장 형식은 기존 소유자를 유지한다. 사용자가 지정한 v14 HTML과 개발 계획을 기준으로 실제 uGUI 화면을 재구성했다. 데모의 160개 룬과 숙련도는 자동화 검증용 별도 저장 폴더에만 주입한다.

최신 `origin/main` **7a3f9d3d**에서 `codex/rune-board-native` 작업 폴더를 만들고, 시안 브랜치 `claude/rune-board-ux` **18807bea**를 작업 브랜치에 병합했다(**4afc706e**). 기본 작업 폴더와 실행 중인 Editor는 수정하지 않았다. 시안 파일은 읽기 전용으로 유지한다. 과거의 세로 목록 상자·정보 카드 숨김 구성 대신, 이번에 지정한 시안의 색 탭·크기 칩·고정 정보 카드·두 줄 보관함을 사용한다.

## 소유 경계와 포팅 지도

| 시안 | 게임 소유자 | 경계 |
| --- | --- | --- |
| 편집 상태와 거래 | `RuneBoardSession` | 실제 저장본을 복제한 전체 무기 편집본, 50회 취소, 스택별 임시 회전, 프리셋 5개 |
| 배치·개방·효과 | 기존 `RunePlacementValidator`, `RuneBoardEditor`, `RuneMasteryProgress`, `RuneEffectEvaluator` | 기존 게임 규칙 호출. JS는 교차 검증 자료로만 사용 |
| 저장 | `GameStore.CommitRuneBoardState` | 저장 revision과 전체 배치 재검사, 성공 뒤 표시 갱신 |
| 창·탐색·고정 행동 | `RuneBoardWindow`, `ContentWindowView`, `ContentWindowHost` | 공통 안전 영역·입력 차단·뒤로가기·일시정지 소유권 |
| 보드·블록·견본 | `RuneGemView`, `RuneGemMesh`, `RuneBoardArt` | 육각 좌표, 면·벽·윤곽·문양·상태 표시. 카메라는 부모 변환 |
| 포인터 | `RuneBoardPointer`, `RuneBoardStoragePointer` | uGUI/InputSystem 입력 → 편집 세션. 보관함 위에서도 보이는 독립 드래그 층 |
| 연습 | `RunePracticeModel`, `RunePracticePointer` | 실제 보유 룬·편집본·계정 저장에 접근하지 않는 임시 모델 |
| 번역 | `RuneBoardText`, `Loc`, `en.txt` | 시안 키를 게임 한국어 원문 키로 연결 |
| 기존 진입점 | `GameUI.ShowRunes`, `OpenRuneService` | 해금·비교 중 잠금 유지. 합성·드롭 확률·미수령 결과는 기존 서비스 |

보관함은 `(색, 모양, 획득 등급)`으로 묶고 실제 인스턴스 ID를 유지한다. 한 무기에 놓은 룬은 다른 무기 보관함에서 빠지며 회수하면 다시 사용할 수 있다. 프리셋은 모든 무기의 배치를 한 번에 기록한다. 프리셋 저장·불러오기·이름 변경도 보드의 편집본에만 반영하고, **변경 저장** 전에는 실제 계정을 쓰지 않는다.

## 화면과 입력

세로는 제목 → 무기 탭 → 숙련도·효과 → 보드 → 높이 7U의 정보 카드 → 보관함 → 고정 행동 순서다. 가로·PC는 보드와 보관함/정보를 1.55:1로 나눈다. 선택 상태가 바뀌어도 정보 카드 높이는 고정한다. 보관함은 색별 개수, 복수 크기 필터, 등급 배지, 회전한 실제 모양, 스택 수량을 보여 준다.

같은 렌더러가 보드, 보관함, 정보 카드, 들고 있는 블록, 연습, 범례를 그린다. 꺼진 면은 채도를 줄이되 색과 양감을 남기고 문양을 새김 형태로 표시한다. 밝은 면의 켜진 문양에는 어두운 외곽선을 둔다. 열린 칸·미개방·봉인·지금 개방 가능, 유효/무효 미리보기, 전체 활성 후보를 별도로 표시한다.

보드에서 0.1초 이내 이동은 보드 이동, 그 이후의 이동은 블록 이동이다. 보관함 터치는 0.3초 길게 눌러 끌고 빠른 세로 이동은 목록 스크롤이다. 들고 있는 블록은 집은 칸을 유지하며 포인터 위로 올라온다. R 회전, Delete/Backspace 회수, Ctrl/Cmd+Z 취소, Ctrl/Cmd+S 저장, Esc 취소/뒤로가기를 지원한다. 회수로 다른 블록의 연결이 끊어지는 경우 기존 배치를 보존한다.

## 리소스

[Unity 이식 매니페스트](../Art/RuneBoard/rune-board-unity-import-manifest.json)에 원본·파생물 해시와 UV 영역을 기록했다. 시안 원본 PNG 27장은 바이트 그대로 복사했고, 7개의 꺼진 면만 시안의 sRGB 변환식으로 재현했다. 시안 전용 `icon-lab`은 제외했다. 새 AI 이미지 생성은 하지 않았다. 기존 원화의 모델 정보 `unknown`, `productionApproved=false`를 유지한다.

타일은 최대 512와 밉맵/Trilinear, 문장·아이콘은 최대 128/Bilinear를 사용한다. Android ASTC 4×4, Standalone BC7, WebGL ETC2 예산 규칙을 연결했다. 기존 에셋의 대량 재임포트를 피하기 위해 공통 임포터 버전은 유지하고 새 경로의 전용 임포터로 적용한다. 문양은 기존 96px 아틀라스를 재사용한다.

## 결정 D1–D10과 시안 차이

| 결정 | 적용 |
| --- | --- |
| D1 스택 | 색·모양·등급. 같은 색·모양이라도 등급이 다르면 별도 칸과 G 배지 |
| D2 영어 | 기존 게임 용어집 우선. 다른 콘텐츠의 번역을 변경하지 않음 |
| D3 잡기 지연 | 마우스·터치 모두 보드 0.1초/5px 기준 |
| D4 원화 승인 | 기존 후보 상태 및 모델 unknown 유지 |
| D5 시연 도구 | 제외. 정보에서 실제 합성·드롭 확률 서비스 연결 |
| D6 PC | 공통 UI 단위의 가로 배치 사용 |
| D7 기존 렌더러 | 새 보드만 교체. 창고·룬 마스터의 기존 아이콘 유지 |
| D8 공통 창 | 새 창 템플릿의 `ContentWindowView` + Draft 출처. 페이지 ID runes 유지 |
| D9 닫기 | 계속 편집 / 버리고 닫기 / 저장 후 닫기 |
| D10 거부 사유 | 시안의 순서와 문구, 실제 C# 검사 결과에서 결정 |

HTML의 데모 도구·로컬 저장·JSON 가져오기·기기 프레임·언어 토글은 게임에 이식하지 않았다. 언어는 게임 설정을 사용한다. 브라우저 SVG/CSS와 Unity 아틀라스/uGUI의 글꼴·래스터화·그림자 처리는 다르며 CSS의 호흡·배치 애니메이션은 그대로 복제하지 않았다. 말풍선은 공통 모달 틀을 사용한 위치 고정 팝업으로 표시한다. 획득 등급 배지와 34개 게임 모양을 유지하며, 능력 수치 표시는 기존 게임의 단위와 소수 두 자리 형식을 유지한다. 실제 계정과 전투 효과는 시안의 데모 수치로 바꾸지 않는다. 기존 영어 용어 유지로 생기는 차이 29개 키는 [번역 비교 목록](RuneBoardNativeEvidence/translation-differences.json)에 기록했다.

## 검증 기록

[통합 검증 기록](RuneBoardNativeEvidence/validation.json)에 실행 범위와 재검사 사유를 기록했다. 시안의 JavaScript에서 추출한 **468개 배치 사례**를 C#과 비교하며, 기하와 편집본·전체 무기 프리셋을 포함한 새 Edit Mode 검사 **8/8**이 통과했다. 마지막 보정 코드의 룬·번역·리소스 관련 검사 **108/108**도 통과했다. 분리 검증 폴더에 기존 도감 JSON이 빠져 최초 1건이 실패했으며, 파일을 복사한 뒤 그 항목만 다시 확인했다. 전체 회귀 결과는 아래에 별도로 기록한다.

[전체 Edit Mode](RuneBoardNativeEvidence/editmode-full.json)는 한 번 실행했다. **4,958개 중 4,911개 통과·47개 실패·건너뜀 0개**이며, 47개는 기존 통합 검증의 실패 이름과 정확히 같고 새 실패는 없다. 실행 중 동결한 통합 코드 이후의 여섯 화면 보정 파일은 별도 복제 폴더에서 위 108개 검사와 관련 네이티브 범위로 검증했다. 이 코드와 당시 작업 폴더의 소스·패키지·번역 **909개 파일이 동일**함을 [해시 목록](RuneBoardNativeEvidence/source-equivalence.json)으로 확인하고 전체 검사를 반복하지 않았다. macOS 개발 빌드는 오류 0개로 성공했다. 공통 UI 소유권 검사와 도구 자체 검사 11개도 통과했으며, 도구 자체 검사의 성공 결과는 재사용했다.

제출 중 추가된 최신 main **c4e03b09**도 작업 브랜치에 통합했다. 충돌은 공통 UI 문서·이력 네 파일뿐이었으며 양쪽 본문과 기존 이력을 보존했다. 룬 구현 코드는 바뀌지 않았다. 새 main의 균열 그래프 제목에 빠진 영어 문구 1개를 채운 뒤 실패한 번역 검사만 다시 확인했고, 통합 후 관련 검사 **126/126**, macOS 빌드 오류 0개, 저장·프리셋·합성/드롭 복귀·전투 일시정지/복귀 범위가 통과했다. [통합 소스 해시](RuneBoardNativeEvidence/integration-source-hashes.json)와 [실행 결과](RuneBoardNativeEvidence/integration-native/result.json)를 별도로 기록했다. 전체 검사와 변하지 않은 화면 행렬은 반복하지 않았다.

네이티브 픽셀 비교는 시안과 같은 clarity 배치·첫 상태별 칸·중앙 사각형을 사용한다. PNG/아틀라스의 8비트 반올림을 고려하여 1/255 허용오차를 명시하고 원시 수치를 함께 보관한다. 실제 차이는 빈 칸 밝기 0.120263(기준 0.12), 꺼진 스킬 면 채도 0.399638(기준 0.40)였다. 원본·파생 이미지의 색 변환식을 바꾸거나 색을 덧칠해 검사를 맞추지는 않았다.

네이티브 검증은 440×956, 956×440, 1440×810, 1440×900, 1890×810에서 한국어·영어와 선택 없음/블럭/칸을 확인했다. 정보 카드 높이 고정, 세로 보관함 두 줄 이상, 상하 안전 영역, 드래그 층, 회전한 실제 모양, 스크롤과 길게 누르기, 핀치 확대, 7개 대화상자, 5단계 연습을 검사했다. 프리셋 5칸의 디스크 복원, 무기 간 룬 이동, 슬롯 개방 취소, 미저장 닫기, 합성·드롭 화면 왕복 시 선택 무기 유지, 전투 일시정지·복귀도 확인했다. 대화상자의 버튼 일부는 uGUI 콜백으로 실행했고 블럭 이동·연습은 InputSystem 포인터로 수행했다.

최초 네이티브 묶음에서 성공한 범위는 재사용했다. 뒤이어 발견한 터치 스크롤 검증 입력의 이벤트 간격, 서비스 복귀 시 선택 무기, 시작점 돌·미니맵 영어 줄바꿈, 연습판 높이를 수정하고 해당 범위만 다시 검사했다. `native-initial`과 `touch`의 실패 기록은 후속 수정 맥락을 위해 남겼으며 최종 결과와 구분한다.

| 비교 | 실제 게임 | 시안 참고 |
| --- | --- | --- |
| 세로 한국어 | [440×956](RuneBoardNativeEvidence/native-final/02-ko-440x956-block.png) | [세로](../../Prototypes/RuneBoard/evidence/layout-portrait-ko.jpg) |
| 가로 영어 | [956×440](RuneBoardNativeEvidence/native-final/32-en-956x440-block.png) | [가로](../../Prototypes/RuneBoard/evidence/layout-landscape-en.jpg) |
| 연습 5단계 | [가로 한국어](RuneBoardNativeEvidence/practice-landscape/05-ko-practice-5-956.png) | [가로](../../Prototypes/RuneBoard/evidence/tutorial-page5-landscape-ko.jpg) |
| 색·상태 렌더링 | [49개 견본](RuneBoardNativeEvidence/gallery/render-gallery.png) | [픽셀 수치](RuneBoardNativeEvidence/gallery/pixel-legibility.json) |

물리 Android/iOS 기기, 스크린리더, 색각 보조 도구 검사는 포함하지 않는다. macOS에서 생성한 포인터·터치 입력 증거와 모바일 실기기 검증을 구분한다. 기본 글자 크기만 검사한다.

작업 브랜치의 PR은 main 병합과 공개 배포를 뜻하지 않는다. 위키는 작업 브랜치에서 생성·검사하며 공개 위키 게시와 게임 배포는 main 병합 후 수행한다.
