# 물약·전선 이탈 설정 — 단일 통합 인계

2026-10-14. 사용자의 모든 화면 통합 후 일괄 검증 지시에 따라 **개별 Unity 실행·개별 빌드 계획을 종료하고 소스를 동결한다.** 이후 변경은 통합 담당의 구체적인 결함 요청이 있을 때만 재개한다. 소스·브랜치·원화·이전 근거는 보존한다.

- 단일 통합 소유자: `01a13790-39bc-7062-a516-b83a354000a8` (스킬 트리 시안 구현).
- 통합 대상: `/Users/t8g-2410-pn-005/Documents/Codex/2026-10-14/task-6/skill-skin`의 별도 통합 브랜치와 기존 Library. 이 담당은 그 checkout·파일·캐시를 수정하지 않는다.
- 화면 브랜치: `codex/casual-potion-quick-skin`, 작업공간 `/Users/t8g-2410-pn-005/Documents/Codex/2026-10-14/task-11/potion-skin`.
- 최종 **구현** 커밋: `8ad9f7e464551fc3d1008051e7bb3ab77370da05`. 이후 이 인계 문서 정리만 추가한다. 전체 최종 HEAD·clean 상태는 task-11의 `IMPLEMENTATION_HANDOFF.json`에 기록한다.
- 공통 기준: `0ccba80eef0da4703cb7ffcd1fb8e22848002b3d`. 소스 변경 손실·미완료 공통 연결·모델 용량 장애 없음.

## 통합 순서와 소유 파일

공통 기준을 이미 포함한 통합 브랜치에는 다음 **화면 커밋 세 개를 순서대로** 적용한다. 개별 merge 커밋 `0c4cf039`와 `1cd07c5d`는 공통 기준을 가져오기 위한 것이므로 이미 해당 기준을 포함했다면 다시 cherry-pick할 필요가 없다. 이 인계 문서를 포함한 최종 문서 커밋은 메시지의 전체 HEAD로 확인한다.

1. `8ad1bc64d1d5da0e01050363d2cd39804e0732a0` — 최초 화면·승인 물약 Resources·전용 테스트.
2. `35f89372a47898a6d9b828840d31a34435a6f63c` — 공통 Casual 버튼/패널/clip 연결, 전용 버튼 관찰자 제거.
3. `8ad9f7e464551fc3d1008051e7bb3ab77370da05` — 최신 공통 기준, Navy 탭·전선 이탈 글자 대비 보정과 최신 근거.

마지막 커밋 하나만 적용하면 전체 화면이 되지 않는다. 초기 커밋의 `PotionQuickButtonSkin.cs/.meta`는 두 번째에서 제거되므로 **최종 상태에는 없어야 한다.** 공통·다른 화면 파일의 충돌은 통합 담당이 검토하며 ours/theirs 일괄 덮어쓰기를 하지 않는다.

소유 경로는 `Assets/HELLSCRIPT/Runtime/Presentation/GameUI.PotionQuick.cs`, `PotionQuickSkin.cs/.meta`, `PotionQuickFill.cs/.meta`, `Assets/HELLSCRIPT/Tests/Editor/PotionQuickCasualSkinTests.cs/.meta`, 아래 Resources, 이 문서 폴더와 `tools/check_potion_quick_casual.py`다. 공통 파일 직접 편집은 없다. `Resources/UI.meta`가 다른 화면과 중복 생성되었다면 통합 담당이 기존 상위 폴더 GUID 하나를 유지한다. 두 물약 PNG GUID와 전용 하위 폴더는 보존한다.

## 원화와 공통 연결

승인 PNG는 이미 화면 커밋에 포함되어 있다. 재생성·재복사할 필요가 없다. [상세 승인 원장](approved-art.json)의 SHA 및 `.meta`와 함께 통합한다. 기존 HUD 물약 리소스는 다른 화면에서 사용하므로 유지한다.

| ID | 최종 경로 (`Assets/HELLSCRIPT/Resources/` 아래) | SHA-256 |
|---|---|---|
| potion-hp | `UI/PotionQuickCasual/potion-hp.png` | `29086f1948be1c607e75634f6980383fa9ad5e6c1522fef880565bba26269264` |
| potion-mp | `UI/PotionQuickCasual/potion-mp.png` | `6b6ce69a1b34dc309afe32d3c1d622746a7dec68f2e5b4524c4f8fccb2600fcc` |

PNG 합계 1,261,081 bytes. Sprite single/full rect, alpha, 중앙 pivot, no mipmaps, max size 256. HP GUID `9c9923ee66d54332a10c8a846444b72e`, MP GUID `cdd0e0f6b502456a9b08db1f8e2fcda2`.

`UiSkin.Casual`, `StorageSurface.SetSkin`, `UiButtonChrome.Casual`, `Clip`, `LabelFont`, `Muted`, `OnLight` 연결 완료. 장비·차트·공통 전투 HUD·edict sheet·world preview를 생성하지 않는 팝업이므로 해당 provider/skin/Configure API의 미연결 건은 없다. `ConfigureEdictSkin` 호출을 추가하면 팝업 밖 preview 표시 범위까지 바뀐다. 공통 359장 장비 공급은 공통 기준에서 한 번만 통합하며 이 화면을 위해 복제하지 않는다.

## 통합 검증에 포함할 항목

아래는 통합 소유자가 일괄 검증에 합칠 입력이며, 이 화면 담당의 별도 실행 계획이 아니다. 기존 완료 검사·이미지 생성·컴파일은 이번 인계에서 반복하지 않았다.

NUnit `-testFilter`에 합칠 관련 클래스:

```text
Hellscript.Tests.PotionQuickCasualSkinTests;Hellscript.Tests.HuntEdictQuickMenuTests;Hellscript.Tests.HuntEdictProgressionTests;Hellscript.Tests.StoreViewBindingTests
```

전용 `PotionQuickCasualSkinTests` 14사례는 소스 컴파일만 완료했으며 Unity에서 실행되지 않았다. 공통 Casual/API 테스트는 통합 담당이 전체 입력에서 중복 없이 포함한다.

기존 플레이어 진입점은 `RuntimeBattleQuickEditSmoke.Install()` → `PotionActionsAcceptance()`다. Development/debug player의 `-hellscriptBattleQuickEditSmoke` 단독 전체 경로가 물약 검증을 포함한다. `-hellscriptPotionActionsSmoke`는 같은 setup 뒤 물약만 수행하는 기존 분기이며 별도 빌드를 요구하지 않는다. 두 flag를 함께 주면 물약 전용 분기가 우선한다. 다른 smoke 초기화 flag를 무작정 함께 주면 서로 다른 runner가 동시에 시작할 수 있으므로 통합 실행 계획에서 조율한다.

필수 입력은 `-hellscriptSavePath <통합 작업용 일회용 fixture 폴더>`와 `-hellscriptEvidencePath <통합 작업의 물약 증거 폴더>`다. 개발 빌드·튜토리얼 졸업 영웅 fixture가 필요하며 실제 사용자 세이브를 넘기지 않는다. 스모크는 실제 저장·실패·재시도를 수행한다. 물약 runner는 자체 무음 처리가 없으므로 실행 전 격리 save 루트의 `hellscript-audio-v1.json`을 `{"version":1,"master":0,"music":0,"effects":0,"muted":true}`로 준비한다. 통합 실행 측에서 추가로 `-nosound` 또는 공통 무음 초기화를 적용·확인할 수 있다. 결과는 `result.txt`, `NN-이름.png`, `save-failure-blocker-ko/en`이며 성공은 `HELLSCRIPT_BATTLEQUICK_SMOKE_OK`와 종료 코드 0으로 확인한다. 예외 `HELLSCRIPT_BATTLEQUICK_SMOKE_FAIL` 및 실패 로그를 보존한다. NUnit XML 경로는 통합 실행의 `-testResults`로 지정한다.

현재 runner의 KO/EN 크기 목록은 440×956, 956×440, 1600×900, 1600×1000, 2100×900이다. **2868×1320은 기존 목록에 없으므로 통합 캡처 계획에 별도로 포함해야 한다.** Potion/OFF/ON, 클릭·드래그·취소·저장·실패/재시도·draft 유지·회전·해금 상태를 검증한다. 게이지 0/1/99/100%, 줄무늬와 clip 끝, 모서리, 물약 가장자리, 영문 잘림 및 튜토리얼 마을 `Fill : Image` 계약을 확인한다. 고정 팝업에 내부 스크롤은 없다. 안전 영역 시뮬레이션은 좌24/하18/우32/상20이며 Mac 시뮬레이션을 실제 iOS 결과로 부르지 않는다.

## 기존 근거와 미검증

- 정적 UI 계약 15/15, refresh 바인딩 16개, 화면 source/art preflight 7개 통과.
- 최신 공통 기준의 Runtime 835개 소스 Roslyn 컴파일 오류 0/기존 경고 47. 전용 테스트 소스 오류 0/경고 0. [컴파일 영수증](static-compilation.json).
- 현 Noto400/700 각 113문자·Jua 46문자 cmap 누락 0. [글꼴](font-coverage.json), [계산 대비](contrast-review.json).
- 시안 사용자 제시 3개 / 실제 확인 3개. EN 시안 Library `libfile_cc29b785e56c819186d44c670a72af7b`는 **참고 시안**이다. 실제 게임 before/after 캡처는 아직 없다.
- 남은 시안 차이는 기존 게임 창 폭420/탭40/버튼52/제목19 유지, 공통 모서리·그림자, Navy 탭, 대비 보정된 주황 글자, 일부 시안 장식 생략이다. 게임 범위·OFF 문구·마을 표시 차이는 기존 기능 계약을 지켰다. [전체 before/after 기록](README.md).
- Unity import/compile·EditMode 실행·플레이어 검증은 **단일 통합 담당에게 이관된 대기 상태**다. 이 화면의 독립 큐 요청은 종료한다. 거부된 실행이나 불명확한 실행 결과는 없다.

이후 이 담당은 통합 담당의 구체적인 결함 요청 전까지 소스 편집·새 테스트·빌드를 재개하지 않는다. 원격 push·main 병합·웹 빌드·공개 공유를 수행하지 않는다. 작업공간은 공유 Git object에 의존하므로 통합·검증 완료까지 보존하며, 2026-10-28은 보존 재검토일이지 자동 삭제일이 아니다.
