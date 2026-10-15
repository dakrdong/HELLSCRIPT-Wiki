# 보석상 캐주얼 스킨 — 구현과 검증 인계

2026-10-14 · [English](Casual_Jeweler_Window_Skin_20261014.en.md)

화면 전용 소스 구현은 동결했다. 개별 Unity 실행·빌드 계획은 종료했고, Unity import·EditMode·네이티브 before/after는 모든 화면을 합친 단일 통합 검증으로 인계했다. 전체 시각 수용 완료로 보고하지 않는다. 기존 게임 거래·가격·수량·저장 계약과 레이아웃을 보존한다.

## 기준과 소유

- 브랜치 `codex/casual-jeweler-skin`, 작업본 `/Users/t8g-2410-pn-005/Documents/Codex/2026-10-14/task-15/jeweler-skin`.
- 최초 기준 `2e193c336c6176d80b9ddbdff7269246bf2590fb`, 검증 스킬 코드 `4139a77e2df495cee009c07c6094b2eea9b1aeaa`.
- 부모의 후속 지시로 공통 `cdb9c140fbf8b778bd3c0bbc0fbcc68a5d9e3fa8`까지 fast-forward한 뒤, 전용 구현 `74095ce4`를 보존하며 공통 `0ccba80eef0da4703cb7ffcd1fb8e22848002b3d`를 충돌 없이 병합했다. 화면 담당이 공통 파일을 직접 편집한 변경은 0이다.
- 최종 전용 변경: `JewelerWindow.cs`, `RuntimeJewelerSmoke.cs`, 신규 `JewelerWindowArt.cs`/meta, `JewelerWindowSkinTests.cs`/meta, `Resources/Art/JewelerWindow/` 72 PNG/meta 및 폴더 meta, 이 한/영 문서와 전용 근거.
- 소스 전용 sparse checkout이다. Unity Library/Temp/Obj/Builds를 복제하지 않았다. Git object alternates는 읽기 기준 저장소를 참조하므로 부모 통합 전 작업본이나 원본 object 저장소를 정리하지 않는다.
- 원본 `6378320b` 출석 브랜치·미커밋 원화·다른 작업 변경은 그대로 보존했다. 원격 push·main 병합·공개 공유·웹 빌드 없음.

## 실제 시안 확인

[Claude 원본](https://claude.ai/artifact/XEsCcRCirkCoXsQ2T5yNUH): 제시 26, 실제 목록/프레임 DOM 26, 전체 캔버스 썸네일 26. 확대 픽셀 확인은 PC KO 1개와 그 화면의 등급 수치 모달이다. [26개 목록](CasualJewelerSkinEvidence20261014/source-artboards.json)에 수량과 범위를 구분했다. 모든 26개 확대 수용 검사가 아니다.

PC에서 가족 탭 변경, 보유0 왕관 다이아몬드 선택과 disabled 상태, 1개 제조(보석24→23, 물약0→1), 사용 시연(물약1→0, 30초 타이머), 등급 모달 열기/닫기를 실제 수행했다. 이는 시안 세션 동작이다. 필요시 브라우저를 다시 열기 전에 `dakrdong@gmail.com` Chrome 프로필을 지원 UI로 확인하며 다른 프로필/사용자 탭은 변경하지 않는다.

[원본 캡처](CasualJewelerSkinEvidence20261014/claude-jeweler-pc-ko.png)는 실제 Claude Export PNG, 1440×810/335651 bytes, 제조·사용 후 G03 등급3/보석23/물약0이다. **게임 before/after 캡처가 아니다.** Library 저장 ID는 `libfile_da3b38b038d08191802793953d39194f` ([메타데이터](CasualJewelerSkinEvidence20261014/library-capture.json)). 게임 전후 캡처는 미실행이다.

## 소스 기준 before / after와 남은 차이

| 항목 | 이전 게임 | 이번 구현 |
|---|---|---|
| 셸·버튼 | 기본 어두운 테마 | `ContentWindowView.Open(..., skin:UiSkin.Casual)`; 공통 Casual/CasualClose 상태·입력 수명 재사용 |
| 폰트 | 기본 UiFonts | 본문 Noto400, 명시적 가족/개수/물약명 강조700, 제목·버튼 공통 LabelFont. Jua에 없는 `·`, `→` 등은 공통 Noto400 fallback. fontsize·hit rect·폰트 파일 변경 없음 |
| 패널 | 사각 어두운 열 배경 | 공통 `StorageSurface.SetSkin` + `Skin.Clip`, radius14/inset2. 원래 content 좌표·폭·ScrollRect 범위 유지 |
| 가족 의미색 | 어두운 화면용 원색 | 공통 `OnLight`로 밝은 바탕 대비 보정. 도메인 색상 데이터는 유지 |
| 보석/물약 그림 | 공유 구 Resources 경로 | 승인 PNG 72종을 `Art/JewelerWindow`에 전용 연결. 공용 JewelerArt/PotionArt는 보존 |
| 가로 내부 스크롤 | 저장/repaint 뒤 각 열 위치 소실 | 두 열의 위치를 이벤트로 기록하고 레이아웃 후 범위 내 복원. 새 Update·저장 subscriber 없음 |
| 정상 smoke fixture | 최고 균열12 | 실제 Elixir 해금 카탈로그 stage(현재60) 사용. 별도 잠김 상태 검사는 유지 |

시안은 G03 등급3·3열 물약 카드·별도 +/-/Max·등급표 모달·사용/초기화 시연을 갖는다. 게임의 G03 등급1·48/52% 두 열과 단일 물약 목록·1/5/All·구매350골드·소켓 관리·인벤토리/전투 사용 경로를 유지했다. 시안의 기능을 계정에 임의 연결하지 않았다. 시안 큰 글자(제목26 등)와 게임 기존 크기 차이, source Noto700 본문→승인400, 공통 API의 남색 제목·금색 선택 버튼은 명시적 차이다. 구조/치수 동일 또는 픽셀 동일이라고 주장하지 않는다.

공통 API 도착 전 작성한 임시 셸/버튼 adapter는 채택하지 않았다. 해당 초안과 근거는 task15 `discovery/retired-scaffolding/pre-shared-api`에 복구 가능하게 보존했다. 최종 구현은 별도 버튼 이벤트·메시·팔레트 복제를 추가하지 않는다.

## 승인 원화와 보존

[전체 매핑](CasualJewelerSkinEvidence20261014/approved-art-mapping.json): `gem-G01..G06-1..6` 36종과 `elixir-G01..G06-1..6` 36종, 원본 합계 **58,539,336 bytes**. 승인 ledger/index의 ID와 SHA256를 다시 대조했고 복사 PNG도 72/72 동일하다. 새 생성 0, 원본 PNG 수정 0. 화면 전용 GUID는 baseline 전체 tracked meta와 충돌 없이 새로 배정했다.

Sprite importer: single sprite, full rect, center pivot, alpha transparency, no mipmap, clamp, 기본512 max/uncompressed. 이는 import 설정 초안이며 Unity 실제 import·크기·알파 가장자리 검증은 아직 없다. 대표 gem/elixir부터 부모 큐에서 확인하고 전체72 lookup 검사를 실행한다.

기존 shared art 참조는 baseline18행이 있으며 보석상 외 화면/재화/상세에도 남아 있다([참조 목록](CasualJewelerSkinEvidence20261014/retained-shared-art-references.txt)). 삭제·구 GUID 교체·전역 재화 치환 0. 정리 조건이 성립하지 않아 이전 원화는 정리하지 않는다. 신규 공용 `Art/Casual.meta` 충돌을 피하기 위해 전용 `Art/JewelerWindow` 경로를 사용한다.

## 관련 검증과 실패 보존

- UI 계약: `python3 tools/check_ui_contract.py --verify --receipt Docs/Implementation/CasualJewelerSkinEvidence20261014/ui-contract-receipt.json`. 최종 [영수증](CasualJewelerSkinEvidence20261014/ui-contract-receipt.json)/[로그](CasualJewelerSkinEvidence20261014/ui-contract-receipt.log).
- UI refresh: [로그](CasualJewelerSkinEvidence20261014/ui-refresh.log). 공유 subscriber/key 규칙을 변경하지 않았다.
- [런타임 소스 정적 컴파일](CasualJewelerSkinEvidence20261014/runtime-static-compile-receipt.json)과 [전용 테스트 소스 정적 컴파일](CasualJewelerSkinEvidence20261014/jeweler-tests-static-compile-receipt.json): 설치된 Roslyn C#9, Unity6000.6 참조 및 기존 package assembly를 읽기 사용. Unity Editor 실행·NUnit 실행·프로젝트 import·플레이어 빌드가 아니다. 입력 hash 전후 일치 확인 포함.
- [원화 SHA/GUID/import metadata 검사](CasualJewelerSkinEvidence20261014/asset-verification.json), [폰트 cmap 검사](CasualJewelerSkinEvidence20261014/font-coverage.json), `git diff --check`.
- 독립 소스 리뷰에서 발견한 이전 Backdrop 이름 조회/키보드 눌림 전달 문제는 임시 adapter 제거와 공통 API 사용으로 사라졌다. 마지막 상태 문구 semantic 색 인자 누락도 수정했다.
- 실패를 제외하지 않았다. 첫 정적 컴파일은 harness의 Unity module 참조 경로 누락, 첫 테스트 컴파일은 mscorlib shim 누락으로 실패했으며 로그 보존 후 참조를 바로잡았다. UI 계약 실행 중 소스 변경으로 무효 처리된 시도도 `stale-input`/`pre-final-color` 근거로 보존했다. 이전 입력의 pass를 최종 pass로 재사용하지 않는다.
- 성능 개선, native 픽셀 수용, 물리 터치·iOS 검증은 주장하지 않는다.

## 공통 0ccba80e 후속 통합

실제 공통 인계와 `cdb9c140..0ccba80e` diff를 읽고 기존 전용 구현을 유지한 merge로 통합했다. 기존 `UiSkin.Casual` 호출이 새 Noto 기호와 대비 보정된 `Muted`를 소비한다. 보석상 선택 버튼은 기존 `Button → UiTheme.Choice`의 Navy 라벨을 사용하며 추가 우회 코드는 필요 없다. 상세 치수 복원·차트·전투/명령 Configure API는 이 화면의 연결 대상이 아니다. 장비359 공급자는 item/emblem 전용이므로 보석·물약72 전용 매핑을 그대로 둔다. 신규 importer/budget의 `Art/Casual/Equipment/` 조건은 `Art/JewelerWindow/`와 일치하지 않는다.

통합 후 [별도 근거](CasualJewelerSkinEvidence20261014/shared-followup-0ccba80e/integration-verification.json): 전용 코드4개는 `74095ce4`와 byte 동일, 승인 PNG72/72 source/destination SHA 동일, 신규 GUID75개는 새 공통 기준 meta4648개와 충돌0이다. [폰트 검사](CasualJewelerSkinEvidence20261014/shared-followup-0ccba80e/font-coverage.json)는 공통 커밋의 정확한 font blob을 읽었다. Noto400/700/900 cmap은 각2619자이며 보석상·GemElixirs·전체 Localization 문자열 누락0, Jua의 `·→`는 기존 공통 fallback 대상이다. 폰트·글자 크기·게임 규칙·레이아웃에 전용 후속 수정은 없다.

이번 입력의 [Runtime 컴파일](CasualJewelerSkinEvidence20261014/shared-followup-0ccba80e/runtime-static-compile-receipt.json)은 오류0/경고91(전용 파일 경고0), [전용 테스트 소스 컴파일](CasualJewelerSkinEvidence20261014/shared-followup-0ccba80e/jeweler-tests-static-compile-receipt.json)은 오류0/경고0, [UI 계약](CasualJewelerSkinEvidence20261014/shared-followup-0ccba80e/ui-contract-receipt.json)은15/15, [refresh](CasualJewelerSkinEvidence20261014/shared-followup-0ccba80e/ui-refresh.log)는16 binding/193소스 통과다. 컴파일 입력 hash 전후 일치, Unity/NUnit 실행0. 이전 영수증은 과거 입력의 근거로 보존한다. 첫 GUID 검사 harness는 서로 다른 branch의 공통 추가 metadata까지 파일로 읽으려다 sparse 경로 부재로 실패했다. [실패와 수정 범위](CasualJewelerSkinEvidence20261014/shared-followup-0ccba80e/asset-verification-initial-failure.json)를 남기고 전용 신규75개를 새 공통 전체 GUID와 다시 비교해 통과했다.

공통 merge는 장비359 원화도 history에 포함하지만 sparse 작업 폴더에 이를 복제하지 않았다. 보석상에 추가 art 원본/캐시 복제0, shared 공급자의 게임 빌드 포함량69,753,836 bytes는 공통 통합 부담이며 화면별 중복 추가분이 아니다. 아래 시안 차이와 단일 Unity 검증 큐는 계속 남는다.

## 부모 검증 큐와 공통 제안

[검증 큐 요청](CasualJewelerSkinEvidence20261014/validation-queue.json). 전용 EditMode 2건과 기존 JewelerTests(85 expanded cases)·해금·StoreViewBinding 및 공통 API 관련 테스트를 지정했다. 확장 smoke는 KO/EN × 6해상도(2868×1320 포함), 빈/선택/비활성/최저·최고등급/물약·보석 한도/잠김/저장 오류·재시도, raycast 클릭·pointer drag·저장 후 각 열 위치, 제조→장착→실제 전투 사용→새 프로세스 재열기/30초 종료를 검사한다. 예상 캡처48개; 실제 실행0.

2868×1320 safe area는 좌우102/하단54/상단0px 가정(DPR3 참조)이다. Mac 해상도 시뮬레이션이며 실제 iOS가 아니다. 무음·일회용 세이브 강제·실사용 세이브 경로 거부를 유지했다. 크기나 글자를 줄여 잘림 검사를 통과시키지 않았다.

**현재 상태:** 사용자의 전체 화면 일괄 검증 지시에 따라 개별 슬롯 요청·빌드 계획은 종료했다. 통합 소유자 `01a13790-39bc-7062-a516-b83a354000a8`가 `task-6/skill-skin`의 별도 통합 브랜치와 기존 Library를 사용한다. 실제 import, 관련 EditMode, native before/after와 Library 업로드, 수용 차이 판정은 **미실행·통합 검증으로 이관**이다. sparse 작업본을 Unity로 열거나 개별 빌드를 만들지 않는다.

공통 규칙 제안: 기호 포함 라벨은 `LabelFont`를 통일 사용하고, 별도 Resources 폴더의 meta 소유자도 배정표에 기록한다. 중첩 ScrollRect의 각 위치는 화면 소유자가 기록하고 unchanged/changed save를 둘 다 검증한다. 새 gem/elixir 공통 공급 계약이 정해지면 이 전용 mapping만 옮기며 현재 EquipmentArtProvider를 잘못 적용하지 않는다.

새 산출물 수명·실측 크기·검사 횟수와 최종 커밋은 task15 `discovery/final-handoff.json`/`artifact-lifecycle.json`에 기록한다. 통합·native 확인까지 원본/근거/미병합 소스를 보존하고 2026-10-21 재검토한다.

## 최종 통합 인계와 편집 동결

- 마지막 구현/정적 근거 커밋: `6befc80c9373d08835e5e903959810b2a4413390`. 동결 인계 시작 시 tracked/untracked 변경0. 이 후속 커밋은 인계 문서·실행 계획 metadata만 바꾸며 게임 코드·PNG·meta를 바꾸지 않는다. 인계 커밋의 전체 SHA/최종 clean 여부는 `task-15/discovery/final-handoff.json`에 기록한다.
- 통합 소유자: `01a13790-39bc-7062-a516-b83a354000a8`. 대상은 `task-6/skill-skin`의 **별도 통합 브랜치 + 기존 Library**다. 원본 체크아웃·캐시·출석 파일럿은 이 작업에서 변경하지 않는다.
- 소스 브랜치: `codex/casual-jeweler-skin`, 위치 `task-15/jeweler-skin`. 공통 `cdb9c140` 위 전용 구현 `74095ce42185e2ce2e444f2e5bffdad4ed2d669d`, 공통 `0ccba80eef0da4703cb7ffcd1fb8e22848002b3d`, 두 이력을 합친 `6befc80c9373d08835e5e903959810b2a4413390`, 마지막 문서 동결 커밋 순이다. 통합 브랜치에 공통0ccba80e가 있으면 **이 화면의 최종 브랜치 tip을 merge**해 구현/근거를 함께 받는다. 동일 공통 변경을 다시 cherry-pick하지 않는다. 선택 이관이 필요하면 부모2(0ccba80e) 대비6befc80c가 보석상 전용 전체 diff이며, merge commit을 mainline 지정 없이 cherry-pick하면 안 된다. 실제 통합 명령은 소유자가 수행한다.
- 원화는 `Assets/HELLSCRIPT/Resources/Art/JewelerWindow/{gem,elixir}-G01..G06-1..6.png`72개와 기존 전용 `.meta`를 함께 가져간다. Resources root는 `Art/JewelerWindow/`, 승인 SHA 매핑은 [approved-art-mapping.json](CasualJewelerSkinEvidence20261014/approved-art-mapping.json), 총58,539,336 bytes다. GUID를 재생성하지 않는다. 공유 JewelerArt/PotionArt와 옛 원화는 참조가 남아 보존한다. 공통 장비359는 이 화면용 중복 복사나 provider 연결 대상이 아니다.
- 남은 공통 API 연결 **0**: `ContentWindowView.Open(..., skin:UiSkin.Casual)`와 view.Skin 기반 라벨·패널·clip을 이미 사용한다. 컴파일 완료이며 모델 용량 때문에 미완료된 보석상 연결은 없다. 네이티브 렌더/기능 수용은 미실행이다.
- 전용 EditMode 필터: `Hellscript.Tests.JewelerWindowSkinTests`(2건), `Hellscript.Tests.JewelerTests`(기존85 expanded cases). 관련 공통 범위는 `Hellscript.Tests.StoreViewBindingTests`, `Hellscript.Tests.RiftContentUnlockTests`, `Hellscript.Tests.CasualUiSkinTests`, `Hellscript.Tests.UiButtonTests`; 통합 실행에서 중복 필터를 합친다.
- 동일 **통합 Development 플레이어**의 보석상 진입점은 `-hellscriptJewelerSmoke -hellscriptSavePath <통합 검증 전용 격리 경로> -hellscriptScreenshots <통합 근거 경로>`다. 새 프로세스 저장 검증은 첫 실행의 동일한 격리 save/인자를 유지하고 `-hellscriptJewelerResume`을 추가한다. **Resume에도 Smoke 플래그가 필요**하다. 다른 화면 자동 종료 smoke 플래그를 동일 프로세스에 동시에 넣지 않는다. 이 인계는 화면별 추가 빌드 계획이 아니다. 실제 실행 순서는 통합 소유자가 조율한다.
- 성공 표식은 `HELLSCRIPT_JEWELER_RUNTIME_OK`/`initial.txt`, 이어서 `HELLSCRIPT_JEWELER_RESUME_OK`/`resume.txt`. Exception은 exit1, 성공은 exit0. 자동 무음·실사용 save 경로 거부·KO/EN×6비율·예상48캡처를 유지한다. [validation-queue.json](CasualJewelerSkinEvidence20261014/validation-queue.json)은 개별 큐가 종료된 통합 검증 입력 목록이다.
- 이미 완료된 원화 생성/SHA/컴파일/UI 계약 검사를 이번 문서 정리에서 반복하지 않았다. 소스·브랜치·근거·복구 초안은 보존한다. 인계 커밋 후 쓰기를 멈추며 통합 소유자의 **구체적인 결함 요청**이 있을 때만 재개한다.
