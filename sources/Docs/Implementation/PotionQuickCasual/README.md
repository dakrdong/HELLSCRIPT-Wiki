# 물약·전선 이탈 캐주얼 스킨 — 2026-10-14

[English](README.en.md)

상태: **소스 동결·단일 통합 인계 완료.** 사용자의 일괄 검증 지시로 개별 Unity·빌드 계획을 종료했다. 공통 담당의 통합 작업공간에서 Unity 컴파일·EditMode·플레이어 검증을 진행한다. [최종 통합 인계](FINAL_INTEGRATION_HANDOFF.md)의 커밋 순서·테스트 입력·종료 정책이 아래의 이전 큐 설명보다 우선한다. 시각 검증 완료나 병합 준비 완료를 뜻하지 않는다.

## 기준과 소유 범위

- 화면 시안: https://claude.ai/artifact/Pejw6sDmiP48Aa3ewfneYx
- 사용자가 제시한 아트보드 3개, 지원 Chrome에서 실제 읽은 아트보드 **3/3**: KO `자동 설정 창 · 탭 3상태`, EN `Auto action window · English`, 개발 명세. KO/EN 각각 물약·이탈 OFF·이탈 ON 세 상태를 표시한다. 개발 명세는 전체 텍스트/소스 확인이며 별도 스크린샷은 없다.
- 실제 브라우저 상호작용: 탭, OFF↔ON, 이탈/복귀 값의 1% 간격 제한. KO 변경/EN 취소는 시안에 저장 핸들러가 없으므로 저장 성공 증거가 아니다.
- 초기 기준 `2e193c336c6176d80b9ddbdff7269246bf2590fb`; 현재 기준은 부모 승인 공통 후속 `0ccba80eef0da4703cb7ffcd1fb8e22848002b3d`. 초기 화면 `8ad1bc64`, 첫 API 연결 `35f89372`를 보존하고 후속 공급을 merge `1cd07c5d`로 반영했다. [공통 API 인계](../Casual_UI_Parallel_Handoff_20261014.md)를 완독했다.
- 브랜치 `codex/casual-potion-quick-skin`. `GameUI.PotionQuick.cs`, `PotionQuickSkin.cs`, `PotionQuickFill.cs`, 전용 검사·문서·물약 Resources만 소유한다. 새 `Resources/UI.meta`는 이 폴더 최초 생성용이며 다른 화면이 같은 상위 폴더를 만들었다면 통합 담당이 GUID 하나를 선택한다. 승인된 공통 커밋을 가져왔으며 공유 파일을 직접 편집하지 않았다.
- 원본 저장소와 공통 스킬 작업공간의 checkout·스테이징·파일을 수정하지 않았다. 이 작업공간은 소스만 sparse checkout한 로컬 clone이며 공유 Git object 저장소에 의존한다. **Unity를 이 sparse clone에서 실행하지 않는다.** 부모의 완전한 통합 작업공간 한 곳에서 순차 검증한다.
- 시안을 다시 열 때만 `dakrdong@gmail.com` Chrome 프로필을 지원 UI로 확인한 뒤 사용한다. 지시 이후 새 브라우저 탭을 열지 않았다.

## Before / after (소스 확인, 게임 캡처 전)

| 항목 | 이전 | 이번 구현 |
|---|---|---|
| 바탕·행 | 기존 어두운 사냥 칙령 색, 직선 외곽 | 크림 패널, 남색 둥근 외곽·하드 그림자·안쪽 점선, 흰 물약/이탈 카드 |
| 버튼·탭 | 기존 UiButton 외형 | 동일 UiButton 입력에 초록 변경·흰 취소·금색 선택 탭, 비활성/hover/press/키보드 focus 표시 |
| 글꼴 | 기존 공용 글꼴과 합성 Bold | Noto Sans KR 400 본문, 실제 700 강조, Jua 제목·버튼·수치. 글자 크기와 기존 텍스트 Rect 유지 |
| 물약 | 공용 HUD의 이전 PNG | 승인된 potion-hp / potion-mp를 화면 전용 경로에서 로드 |
| 게이지 | 기존 HUD sprite, 사각 임계 음영 | 실제 HP·자원량을 유지하는 Image 파생 채움, 공통 둥근 stencil 안에 채움·음영·줄무늬 포함 |
| 저장·해제·안전 영역 | 기존 분리 draft, 단일 거래, 실패 시 draft 유지 | 동일 메서드 유지. 저장 ID·도메인 범위·1% 간격·일시정지 복원·해금 규칙 변경 없음 |

패널은 `StorageSurface.SetSkin(UiSkin.Casual)`, 버튼은 `UiSkin.Casual.ApplyChrome`을 통해 `UiButtonChrome.Casual`, 게이지 마스크는 `UiSkin.Casual.Clip`을 사용한다. 공유 `UiButton.Face`가 계속 활성 상태로 입력·선택·비활성을 처리하며, 이전 전용 `PotionQuickButtonSkin`과 프레임별 관찰자는 제거했다. 버튼 라벨만 생성/탭 선택 시 명시적으로 꾸민다. 표시 글꼴은 공통 `LabelFont(text)`의 문자별 fallback을 따른다. 공통 `UiTheme.D4*`/430개 look 팔레트, `UiFonts.Casual*`, `SkillTreeD4ShapeGraphic`, `SkillTreeD4TextShadow`, `UiMesh`로 화면 전용 행·토글·표식을 구성한다. 스킬 전용 좌표 배율 2를 복사하지 않고 새 범용 renderer/material/shader/Canvas를 만들지 않는다. `PotionQuickFill : Image`와 `fillAmount` 계약을 유지하며 공통 메시로 채움을 그린다. `Vital` 아래 공통 clip은 원래 월드 영역을 유지하면서 채움·음영·줄무늬를 함께 자른다. GC/실기기 성능은 아직 측정하지 않았다.

## 후속 API 연결

후속 연결: 탭 라벨은 공통 규칙에 맞춰 선택 여부와 관계없이 Navy를 유지한다. 보조 글자는 공통 `Muted` 대비 보정을 그대로 받는다. `UiTheme.Choice`만 호출한 뒤에도 탭 글자색이 유지되는지 기존 테스트에 추가했다. Noto 기호 보강을 공유 파일 그대로 받았으며 화면별 폰트·문자 치환은 없다. 이 팝업은 칙령 sheet 숫자 조절기·월드 미리보기·전투 HUD·차트·장비 상세를 생성하지 않으므로 `ConfigureEdictSkin`, `ConfigureCombatSkin`, 차트 `skin:` 및 장비 provider를 호출하지 않는다. 특히 `ConfigureEdictSkin`을 호출하면 팝업 밖 world preview 범위까지 바뀐다. 공통 359장 원화 커밋은 병합 이력에 포함되지만 sparse 작업공간에는 장비 PNG를 추가 복사하지 않았다.

흰 카드의 전선 이탈 제목·백분율에 쓰는 주황색은 공통 `OnLight`로 텍스트만 보정했다. 임계값 마커·게이지 원색·금색 제목·흰 수치에는 일괄 색 변환을 적용하지 않았다. 시안보다 어두운 주황색 텍스트는 공통 대비 규칙에 따른 시각 차이다.

## 승인 원화

원장: 원본 `Docs/Art/Art_Transition_20261014_Asset_Index.json`, `casual-art-brief/DIRECTOR_STATUS.json`의 승인 상태 pass. 새 생성·색상 필터·기존 이미지 변형은 없다. 승인 PNG 바이트 그대로 복사했다.

| ID | SHA-256 | 전용 Resources 키 |
|---|---|---|
| potion-hp | `29086f1948be1c607e75634f6980383fa9ad5e6c1522fef880565bba26269264` | `UI/PotionQuickCasual/potion-hp` |
| potion-mp | `6b6ce69a1b34dc309afe32d3c1d622746a7dec68f2e5b4524c4f8fccb2600fcc` | `UI/PotionQuickCasual/potion-mp` |

물약은 장비 catalog 항목이 아니므로 `WithEquipmentArt(...)`를 적용하지 않는다. 화면 전용 매핑을 유지했다. 두 PNG 합계 1,261,081 bytes. Sprite single/full rect, 중앙 pivot, 입력 alpha, no mipmaps, max 256 import. 전체 매핑은 [approved-art.json](approved-art.json). 공유 HUD/PotionArt가 이전 아트를 사용하므로 이전 PNG·meta는 삭제하지 않았다.

## 검사 결과

- `python3 tools/check_ui_contract.py --verify --receipt Docs/Implementation/PotionQuickCasual/ui-contract.json`: **15/15 통과**. [영수증](ui-contract.json), [로그](ui-contract.log).
- `python3 tools/check_ui_refresh.py --inventory Docs/Implementation/PotionQuickCasual/ui-refresh-inventory.json`: **16개 바인딩 보호, UI 소스 192개 목록화**. 도달/렌더링 검증은 아니다.
- `python3 tools/check_potion_quick_casual.py --receipt Docs/Implementation/PotionQuickCasual/source-art-preflight.json`: **7개 source/art preflight 통과**. 기존 QuickRect·Label 문구/크기 및 SetBar/Reflow/Commit 본문 보존, 430-look 팔레트, 승인 SHA, 소유 범위 검사.
- [font-coverage.json](font-coverage.json): 갱신된 공통 폰트에 대해 화면 소유 KO 문구·등록 EN 번역으로 재검사했다. 본문 Noto 400/700 각 113개 문자, Jua 제목·탭·버튼·백분율 46개 문자 cmap 누락 0. 입력 폰트 SHA와 문구 목록을 기록했다. 체크 표시는 Noto 700으로 그린다. 동적 자원 이름, Unity 줄바꿈·잘림은 실제 실행 검증이 남아 있다.
- `git diff --check`: 통과.
- [계산 대비](contrast-review.json): 전선 이탈 텍스트/흰 카드 2.5179→4.8368:1, 보조 글자/크림 4.5004:1, 선택 탭 Navy/Gold 8.7943:1. 불투명 단색 기준 계산이며 실제 렌더·접근성 인증은 아니다.
- **Roslyn 소스 컴파일 통과:** 새 공통 파일 3개를 포함한 Runtime 835개 소스 오류 0/경고 47, 전용 테스트 어셈블리 오류 0/경고 0. 설치된 .NET 10.0.401과 기존 Unity 참조 DLL을 읽기만 했고 Unity·플레이어를 실행하지 않았다. Runtime 경고는 화면 변경 외 기존 소스에 있다. [입력 SHA·출력·명령 영수증](static-compilation.json), [Runtime 로그](runtime-compile.log), [테스트 컴파일 로그](tests-compile.log). Unity import/컴파일 및 테스트 실행을 대신하지 않는다.
- **작성·소스 컴파일 완료, 실행 대기:** `Hellscript.Tests.PotionQuickCasualSkinTests` 14개 케이스. 0/1/30/99/100% 채움, 둥근 마스크, 입력/키보드/비활성, 원래 크기, 글꼴 역할, 승인 PNG import를 확인한다.
- **미실행:** Unity 컴파일, 기존 `HuntEdictQuickMenuTests`, `HuntEdictProgressionTests`, `StoreViewBindingTests`, 플레이어 스모크. 실패 검사를 제외하거나 기존 테스트를 약화하지 않았다.

## 남은 차이와 순차 검증 요청

공통 버튼은 반경 최대 10과 hit rect 내부 face/그림자 규칙을 사용하므로 초기 전용 스킨 반경 12/14 및 시안 외형과 차이가 있다. 원래 버튼 좌표·글자 크기는 그대로다.

시안은 560×560 창, 탭 46, 하단 버튼 56, 제목 24다. 실제 게임은 최대 폭 420, 양쪽 탭이 열리면 높이 410, 탭 40, 하단 버튼 52, 제목 19이며 이를 그대로 유지했다. HP만 해금 시 높이 232도 유지한다. 시안의 일부 더 큰 본문/수치 칸·탭 간격을 복사하거나 글꼴을 축소하지 않았다. 물약 소켓 장식은 현재 아이콘 28×36을 유지하는 38×46 외곽으로 구성했다. 시안의 46×46 소켓, 선택 탭 아래 돌출 다이아몬드, 반복 shine/blur, 방사형 하이라이트와 정확한 3-stop gradient는 픽셀 동일하지 않다. 돌출 탭 장식은 기존 hint와 간격이 없어 추가하지 않았다.

도메인 충돌은 게임 계약을 유지했다: 시안 HP/자원 5–95 vs 실제 HP 1–100/자원 0–100; 시안 이탈 5–90·복귀 10–100 vs 실제 1–99·2–100. OFF 문구도 별도 치명상 반응과 충돌하는 시안의 “HP와 관계없이 계속 싸움” 대신 기존 “낮은 HP만으로 물러나지 않음”을 유지했다. 마을에는 가상 실시간 수치 없이 HP/자원 이름과 full fill을 표시한다. 해금 전에는 메뉴 입구를 숨기며 빈 설정창을 새로 만들지 않는다. 고정 창이므로 내부 스크롤이 없다.

통합 담당이 모든 화면을 합친 일괄 검증에 다음 범위를 포함한다(이 화면의 별도 실행 계획은 종료):

1. 위 14개 전용 EditMode + 기존 세 관련 테스트 클래스를 실행한다. 실패를 분류·수정하고 모든 결과를 보존한다.
2. 기존 `-hellscriptPotionActionsSmoke`를 **격리 save path, task evidence path, 무음**으로 실행한다. KO/EN 드래그·토글·OFF/ON·취소·저장·실패/재시도·자동저장 중 draft 유지·회전·해금 조합을 검사한다. 튜토리얼의 마을 Fill 계약도 관련 범위에 포함한다.
3. 기존 스모크 해상도 440×956, 956×440, 1600×900, 1600×1000, 2100×900에 더해 **2868×1320** KO/EN 3상태를 별도로 캡처한다. 기존 스모크 크기 목록에는 2868×1320이 없으며 공유 스모크를 임의 변경하지 않았다.
4. 임계값 0/1/99/100%의 마스크 끝, 이탈 줄무늬, 곡선 모서리, 물약 가장자리, 영문 잘림·포인터 입력·저장 오류를 실제 렌더링으로 검토한다. 안전 영역 가정은 기존 시뮬레이션 좌24/하18/우32/상20을 기록한다. Mac 해상도 시뮬레이션을 iOS 실기기 결과로 부르지 않는다.

현재 game before/after 캡처는 없다. 시안 EN 3상태 캡처만 Library에 저장했다: **`libfile_cc29b785e56c819186d44c670a72af7b`**, file `file_00000000ae4881f5ba38b5477dbf6ca8`, version 1, 141,277 bytes. 실제 게임 캡처를 얻은 후 별도 Library ID를 기록해야 한다.

공통 규칙 제안: 표시만 바꾸는 스킨에서도 `Fill : Image` 같은 기존 smoke 계약을 목록으로 공유하고, 게이지의 현재량과 임계값 음영은 동일한 둥근 clip을 사용한다. 새 공통 API의 활성 Face·명시적 라벨·clip 계약을 화면별 검증 목록에 포함한다. 원본 `Shared_UI_Contract`의 “HUD 생명력 부품 공유” 설명은 실제 값/입력 계약과 화면 전용 아트 경계를 구분하도록 갱신을 제안한다. 공유 문서는 여기서 직접 수정하지 않았다.

원격 push·main 병합·웹 빌드·공개 공유·설치·권한 확대 없음. 소스/검사/증거와 Git object 의존 workspace는 부모 통합 및 검증까지 보존하며 2026-10-28에 보존 필요를 재검토한다. 자동 삭제일이 아니다.
