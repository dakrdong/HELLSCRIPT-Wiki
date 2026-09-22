# 대장간 구현 검증 기록

작성일: 2026-09-22

[English](Blacksmith_Validation.en.md) · [구현·리소스·이전 명세](Blacksmith_Unity_Integration.md)

## 실행 환경과 근거

Unity 6000.6.0f1의 Edit Mode 검사와 macOS 개발 빌드를 사용한다. 구현 브랜치는 `codex/blacksmith-runtime`이며, 커밋된 인벤토리 기반에 `main`을 병합한 별도 작업 디렉터리에서 검증한다. 원래 체크아웃의 실행 중인 에디터, 미커밋 인벤토리 변경과 기존 저장 파일은 수정하지 않는다.

macOS 실행 검사는 별도 저장 경로에 만든 검증용 계정을 사용한다. UGUI의 실제 화면 좌표에서 레이캐스트로 버튼의 노출을 확인한 뒤 포인터 이벤트를 보내고, 키보드 E 입력도 전달한다. 클릭 전후의 실제 `GameStore` 잔액·강화 단계·작업·저장 복구 결과를 확인한다. 사진만 보고 기능 성공으로 판정하지 않는다.

자동 검증 실행에서는 macOS 창의 초점 이동으로 합성 키보드가 비활성화되지 않도록 입력 설정을 일시적으로 조정한다. 이 설정은 검증 실행 인자가 있는 개발 빌드에만 적용한다.

## 검사 결과

- HTML 동작 검사: **80개 통과, 실패 0개**.
- 대장간·자원·품질·능력치·번역 집중 Edit Mode 검사: **178개 통과, 실패·건너뜀 0개**. [원본 XML](BlacksmithEvidence/blacksmith-focused-editmode.xml)과 [요약·해시](BlacksmithEvidence/focused-summary.json)를 보존한다. 이 수는 전체 회귀 검사와 중복되므로 더하지 않는다.
- 최신 인벤토리 `8a5c88d`와 `main` `12e0a88`을 합친 뒤 실행한 통합 Edit Mode 검사: **312개 통과, 실패·건너뜀 0개**, 99.25초. 대장간·공용 비교·분해 설정·인벤토리·상점·룬·그래픽·마을을 포함한다. [원본 XML](BlacksmithEvidence/blacksmith-integration-editmode.xml) · [요약·해시](BlacksmithEvidence/integration-summary.json). 검사 수는 다른 실행과 중복된다.
- macOS 개발 빌드: 성공, 빌드 오류 0개. 실제 실행 검사는 `HELLSCRIPT_BLACKSMITH_SMOKE_OK`로 종료했다. [실행 결과](BlacksmithEvidence/runtime-result.txt)와 [69개 캡처의 해시 목록](BlacksmithEvidence/runtime-manifest.json)을 보존한다.
- 최종 macOS 인벤토리 통합 검증도 `HELLSCRIPT_INVENTORY_SMOKE_OK`로 종료했다. 한·영 가로·세로에서 인벤토리·상점·창고의 공용 비교, 두 반지의 비교 기준, 분해 자동 선택 설정의 공유·저장과 전투 입력 차단을 확인했다. [실행 결과](BlacksmithEvidence/inventory-runtime-result.txt) · [대표 화면 해시](BlacksmithEvidence/inventory-runtime-manifest.json) · [비교 화면](BlacksmithEvidence/inventory-20-shared-inventory-landscape-ko.png).
- 리소스: 투명 PNG **30개**의 알파 채널·투명 픽셀·내용과 해시를 확인했다.

전체 Edit Mode 회귀 검사: **2,880개 통과, 실패·건너뜀 0개**, 1,518.18초. [전체 XML](BlacksmithEvidence/blacksmith-full-editmode.xml)과 [요약](BlacksmithEvidence/full-summary.json)을 보존한다. 이 전체 검사는 최신 인벤토리 병합 전의 대장간 구현 스냅샷에서 실행했다. 이후 추가한 보스 보상 단계 검사와 공용 비교 통합은 최신 312개 검사에 포함한다. 언어 변경 시 화면 유지와 최종 통합 화면은 macOS 실행 검사로 확인한다.

실행본에는 기존 URP 빌드 설정의 Lens Flare·Panini 후처리 셰이더 제외 로그가 남는다. 대장간 UI·입력·저장 검증은 통과했으며 해당 후처리 효과까지 검증한 것으로 보고하지 않는다.

## 화면 기록

| 기준 | 대표 캡처 |
| --- | --- |
| 440×956 | [옵션 목록](BlacksmithEvidence/portrait-ko-tab0-list.png) · [옵션 상세](BlacksmithEvidence/portrait-ko-tab0.png) · [슬롯 성장](BlacksmithEvidence/portrait-ko-tab1.png) · [장비 강화](BlacksmithEvidence/portrait-ko-tab2.png) · [영어 슬롯](BlacksmithEvidence/portrait-en-tab1.png) · [영어 강화](BlacksmithEvidence/portrait-en-tab2.png) |
| 956×440 | [옵션 변경](BlacksmithEvidence/landscape-ko-tab0.png) · [슬롯 성장](BlacksmithEvidence/landscape-ko-tab1.png) · [장비 강화](BlacksmithEvidence/landscape-ko-tab2.png) · [영어 성장 상세](BlacksmithEvidence/landscape-en-growth.png) |
| PC 16:9 | [슬롯 성장](BlacksmithEvidence/pc-16x9-ko-tab1.png) |
| PC 16:10 | [영어 옵션 변경](BlacksmithEvidence/pc-16x10-en-tab0.png) |
| PC 21:9 | [장비 강화](BlacksmithEvidence/pc-21x9-ko-tab2.png) |
| 주요 상태 | [자동 목표 발견](BlacksmithEvidence/interaction-auto-match.png) · [Lv.75 미리보기](BlacksmithEvidence/interaction-preview-75.png) · [해금 안내](BlacksmithEvidence/interaction-unlock-tooltip.png) · [작업 표시 1](BlacksmithEvidence/interaction-working-bright.png) · [작업 표시 2](BlacksmithEvidence/interaction-working-pulse.png) |
| 고정 팝업 | [가능 옵션](BlacksmithEvidence/portrait-ko-candidates.png) · [성장 상세](BlacksmithEvidence/portrait-ko-growth.png) |

전체 69개 화면은 리소스 묶음의 `Artifacts/Blacksmith/RuntimeEvidence/`에 포함한다. 위 대표 화면은 Git에도 보존한다.

## 검증표

| 대상 | 확인 항목 | 근거 |
| --- | --- | --- |
| 강화 견적 | 1~100단계 가격, +10 합계, 최대치와 잔액, +97→100, 부족·변경된 견적의 무차감 | `BlacksmithTests` |
| 거래 보호 | 같은 요청 재실행, 저장 실패, 재화 넘침, 상태 보존 | `BlacksmithTests` |
| 품질·이전 | 이전 +5 보존, 각성·걸작 뒤 고정 증가량, 접사 고정·투자 기록 보존, 걸작 +5 이상 | `BlacksmithTests`, 품질 검사 |
| 옵션 변경 | 최초 결제 후 위치 고정, 반복 비용 유지, 실제 후보·범위, 자동 목표 대기·계속·중단·창 닫기 | 코어 검사와 macOS 실행 검사 |
| 작업 칸 | 캐릭터별 성장, 계정 공용 두 칸, 10·50·250다이아 순차 개방, 중복 작업 차단 | `BlacksmithTests` |
| 시간 | 도달 레벨별 비용·시간, 종료 후 한 번 정산, 60초 1다이아·59초/1초 무료 | `BlacksmithTests`, macOS 실행 검사 |
| 전투 | 진행 중 슬롯 스냅샷 유지, 다음 전투 반영, 원소 저항 70% 상한, 배운 액티브·장착 패시브의 유효 레벨 | 코어·스킬·전투 회귀 검사 |
| 분해 | 전설 10·고유 효과 15·세트 10을 단건/일괄/자동 경로 각각 검사, 코어와 저장 보존 | 9개 경로 조합 검사 |
| 균열·소탕 | 보상 단계별 강화석, 한 번만 획득, 소탕 반복 요청 차단 | `BlacksmithTests`와 자원 검사 |
| 접근·화면 | NPC 범위 밖 차단, 대장간 이용·E키, 같은 폭의 세 열, 목록 왕복, 전체 화면 팝업 | macOS 실행 검사 |
| 상세 연출 | 성장 미리보기 값 갱신, 3초 해금 안내, 작업 중 점멸 | macOS 상태 검사와 캡처 |
| 언어·비율 | 한국어·영어, 440×956·956×440·PC 16:9/16:10/21:9, 인벤토리 공용 등급 색상 | 번역 검사와 macOS 캡처 |

## 재현 방법

HTML: `node --test Prototypes/Blacksmith/*.test.cjs`.

Unity: `-batchmode -nographics -runTests -testPlatform EditMode -testResults <xml>`로 검사한다. 대장간 범위만 실행할 때는 `-testFilter Hellscript.Tests.BlacksmithTests`를 추가한다.

macOS 개발 빌드: 기존 `Hellscript.Editor.ProjectBuilder.BuildMac`을 실행하고 `-hellscriptBuildOutput <app 경로>`를 전달한다. 실행본에 `-hellscriptBlacksmithSmoke -hellscriptSavePath <새 검증 저장 폴더> -hellscriptScreenshots <증거 폴더>`를 전달하면 자동 검증 후 종료한다. 성공 표식은 `HELLSCRIPT_BLACKSMITH_SMOKE_OK`와 증거 폴더의 `result.txt`다. 일반 게임 실행에는 해당 인자를 넣지 않는다.

현재 작업 폴더의 `Artifacts/Blacksmith/Play-Blacksmith.command`는 별도 검증 계정으로 수동 플레이를 시작한다. 원래 계정과 저장 경로를 공유하지 않는다.

리소스 묶음: `python3 tools/blacksmith/package.py`. 결과는 `Artifacts/Blacksmith/HELLSCRIPT-Blacksmith-Resources.zip`이며 파일별 SHA-256 목록을 포함한다. 이 묶음은 리소스와 적용 근거를 전달한다. 실행 가능한 전체 변경은 Git 브랜치로 통합한다.

## 확인 범위와 공개 상태

모바일 해상도는 macOS 창에서 재현했다. 실제 아이폰·안드로이드의 터치, 키보드, 회전, 노치와 성능은 별도 기기 검증이 필요하다. 로컬 저장 어댑터를 사용하며 서버 시각·서버 재화 거래를 구현했다고 주장하지 않는다.

이 작업 브랜치에서는 공개 위키를 배포하지 않는다. `main` 병합 담당자가 최신 `main`에서 위키를 다시 생성·검사하고 공개 저장소와 GitHub Pages에 게시한 뒤 확인한다. 공개 주소는 [HELLSCRIPT Wiki](https://dakrdong.github.io/HELLSCRIPT-Wiki/#/tree)다.
