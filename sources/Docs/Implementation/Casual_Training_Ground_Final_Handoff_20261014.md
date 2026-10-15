# 훈련장 최종 통합 인계 · 2026-10-14

이 문서가 현재 상태의 정본이다. 부모 최신 지시로 개별 Unity 실행·빌드 계획을 종료하고 소스 편집을 동결한다. 이후 변경은 통합 담당의 구체적인 결함 요청에 한정한다. 단일 통합 담당은 `01a13790-39bc-7062-a516-b83a354000a8`, 작업 위치는 `task-6/skill-skin`의 별도 통합 브랜치와 기존 Library다.

## 안전한 상태와 적용 순서

- 전용 소스: `/Users/t8g-2410-pn-005/Documents/Codex/2026-10-14/task-12/training-skin`, 브랜치 `codex/casual-training-skin`.
- 동결 직전 clean HEAD `1cbf79b866de313a738f1195e39067fe3061bbe8`; 구현 커밋 `2dee3038016169b7ad7110cfec524d351c9d02dc`. 이번 인계 변경은 문서/큐만이며 런타임·테스트·아트 소스는 동일하다. 인계 문서 커밋의 전체 SHA는 작업 폴더 밖 `training-evidence/delivery-receipt.json`에 기록한다.
- 적용 완료 공통 API: 원본 `cdb9c140fbf8b778bd3c0bbc0fbcc68a5d9e3fa8`, 로컬 cherry-pick `d5b8c8898cb2bc140c4c736426b961704f57ac09`.
- 후속 `0ccba80eef0da4703cb7ffcd1fb8e22848002b3d`는 인계·실제 소스 읽기만 완료했다. 이 분기에 cherry-pick·폰트/359장 복사·화면 연결을 수행하지 않았다. 조사 보조 에이전트가 모델 용량 오류로 종료됐으며 최신 지시에 따라 재시도하지 않았다. 미완료 상태를 그대로 인계한다.
- 통합 브랜치에 `0ccba80e`가 있다면 중복 공통 커밋을 적용하지 말고, 화면 전용 커밋을 순서대로 검토 적용한다: `828fd7bb2be5b2fe1a2db8305e8742212cc97c4e` → `09ce5e5aefa900f0a17455c41f679e540af4bdf2` → `2dee3038016169b7ad7110cfec524d351c9d02dc` → `1cbf79b866de313a738f1195e39067fe3061bbe8` → 이 문서의 최종 커밋. 첫 커밋은 임시 observer를 포함하지만 `2dee3038`에서 제거하므로 중간 상태를 완성본으로 쓰지 않는다.
- 공통 기준을 아직 준비하지 않은 경우 정확한 소유자 순서는 `cdb9c140` → `6cdf2df690941b7c9f751c39235ea36ce4ff9ed9` → `f25e67aab62d72f9792b5c0d49e6692bfbca5eed` → `0ccba80e`. 본 화면이 소유하지 않는 공유 소스는 수정하지 않았다.

## 완료된 연결과 미완료 연결

완료: 훈련장·적 선택·결과 Open3곳의 `skin:UiSkin.Casual`, 일반 패널 SetSkin, 공통 버튼 chrome, Noto400/명시적700·900, Hero/초상화/드롭다운 Clip, full-UV contain, N23/B06 표시 전용 fallback. Hero/초상화는 shadow0으로 clip 경계를 맞췄다. 게임 규칙·저장·가격·ID와 기존 배치 분기는 유지했다.

통합 담당이 이어서 확인할 정확한 미완료 지점:

1. `TrainingGroundResultWindow.cs`의 `DrawDps` → `TrainingDpsPanel.Draw(...,skin:shell.Skin)` 및 `SkillCard` → `RiftSkillShareView.Card(...,skin:shell.Skin)`. 현재 호출에는 skin 인자가 없다. 새 API의 축·시리즈·cast·스크롤·callback 계약을 유지해야 한다.
2. 로비 `TrainingGroundWindow.Hero.cs`의 전투 시작과 결과 `TrainingGroundResultWindow.cs`의 재시작에서, 정상 HUD 생성 전에 `ConfigureCombatSkin`이 적용되도록 통합 담당이 수명을 연결한다. `GameController.BeginTrainingGround`는 검증/저장 후 BeginRun으로 HUD를 만들고 나서 로비를 닫는다. 공통 setter는 GameUI의 다음 생성용 필드를 유지하므로, 로비 Open에 무조건 설정해서 뒤의 다른 균열까지 영향을 주지 않도록 이전 scope 복원/재진입을 검증해야 한다. 여기서는 setter 호출을 추가하지 않았다.
3. 로비 스킬 행/칙령 버튼과 결과 카드의 `game.UI.OpenTrainingEdict(...)` 경로에 `ConfigureEdictSkin` 수명을 연결한다. 후속 setter는 world의 지정 renderer에도 적용하므로 숫자 조절기 생성·world preview·닫기/재진입까지 통합 확인이 필요하다. 공유 GameUI/World 파일은 이 작업의 독립 소유가 아니다.
4. `0ccba80e`의 supplied HUD API는 전체 pause dialog/header의 스킨 완료를 의미하지 않는다. 공유 `GameUI.TrainingGround.cs`의 `trainingPause`, `ShowTrainingPause`, 공용 header/ShowHudPanel을 실제 화면과 대조할 것.
5. 공통 Casual Quiet의 불투명 idle face가 auto-off 스킬 행 부모의 경고 테두리 일부를 덮는 P2는 남아 있다. 경고 문구는 유지된다. Quiet 무면 정책 또는 경고 테두리 우선순위를 공통 소유자가 결정해야 하며 local observer를 다시 만들지 않는다. 후속 commit 범위에 UiButtonFace 변경은 없었다.

훈련장 전용 코드에는 장비 view가 없어 `WithEquipmentArt(CasualEquipmentArt.Provider)` 소비 지점이 없다. 장비 359 PNG나 provider를 별도로 복제하지 않았다. Noto 기호 후속과 변경된 Muted 대비는 통합 공통 기준에서 받아야 한다. 기존 font audit는 이전 입력의 역사적 근거이며 신규 폰트 검증 결과가 아니다.

## 아트 인계

- 전용 배경 `Assets/HELLSCRIPT/Resources/Art/TrainingGround/training-yard.png`: 초기 `828fd7bb`에 포함. SHA256 `d1055549d233e1a451b8b3177b9582372e117d273f1b238017ce46899d4e4a4b`, 2,453,192B. 같은 경로/meta/GUID 유지; meta SHA256 `1cfe71bef209557ac6cd9892a5dc61dfa5468a10df12fffc52ead239e1612221`. 생성/픽셀 수용과 production approval을 구분하며 native crop은 미검증.
- 공유 몬스터35장은 본 분기에 복제하지 않았다. 정확한 원본 `57bba034dad1659e8a36dc9645b1291d2554e497`의 `Assets/HELLSCRIPT/Resources/Art/TrainingGround/Portraits/{ID}.png`/meta를 기존 도감 변경과 함께 통합 담당이 보존한다. 35/35 SHA 일치, 63,338,362B, `pixel-review-accepted`, `productionApproved:false`. N23/B06 파일이 없으면 fallback 그림도 표시되지 않는다. 이전 monsters checkout b732010a를 통째로 복사하지 않는다.
- 원화 상세 영수증은 [기존 보고서](Casual_Training_Ground_Skin_20261014.md)와 evidence의 `training-yard-manifest.json`, `casual-monster-reuse-audit.json`, `monster-source-conflicts.md`에 있다.
- Library 실제 ID: 시안5캡처 시트 `libfile_aa6df6c145ac819195d450952cfb7724`; 배경 원본 `libfile_4854daf17da88191918cb7e001eb3015`. Unity after 캡처는 없다.

## 통합 검사 입력과 스모크 진입점

기존 `2dee3038` 입력에서 UI 계약 회귀15개 및 refresh16 binding/191 소스 통과. `common-api-static-checks.json`과 `common-api-inputs.json`이 정확한 근거다. 이번 동결 인계에서 기존 생성·검사를 다시 실행하지 않았다. Unity/C# compile/native 실행0. 새로운 공통 후속이 합쳐진 입력의 통과로 과거 결과를 재사용하지 않는다.

관련 EditMode 필터: `Hellscript.Tests.TrainingGroundSkinTests`(준비5cases·실행0), `TrainingGroundUiTests`, `TrainingGroundTests`, `StoreViewBindingTests`; 공통 영향은 `UiButtonTests`, `CasualUiSkinTests`, 후속 `CasualChartSkinTests`를 통합 계획에 포함한다. 전체 회귀는 통합 담당이 모든 화면을 합친 입력에서 조율한다.

기존 개발 플레이어 진입점은 `RuntimeTrainingGroundSmoke.Boot`의 `-hellscriptTrainingGroundSmoke`, 필수 인자는 `-hellscriptSavePath <일회용 경로>`와 `-hellscriptEvidence <통합 근거 경로>`다. 스모크는 fixture 저장 데이터를 바꾸므로 사용자 세이브를 절대 쓰지 않는다. 무음 옵션/설정을 통합 실행자가 적용한다. 시작·재시작·비교/실패·picker/dropdown·칙령/scroll·KO/EN을 포함하며 성공 표식은 `HELLSCRIPT_TRAINING_GROUND_SMOKE_OK`, 결과 `runtime.txt`; 예외는 `failure.txt`/exit1이다. 다른 DPS/활동 스모크 flag는 Start의 별도 분기이므로 여러 flag를 한 프로세스에 나열해 모두 실행됐다고 해석하지 않는다.

실제 KO/EN·2868×1320·PC16:9/16:10/21:9·440×956, 빈/선택/비활성/오류, click/touch/scroll/save failure, gauge/원형/둥근 모서리/긴 EN·시안 글자 크기는 통합 실행에서 확인해야 한다. 기존 스모크의 해상도 목록이 모든 요구를 자동 충족한다고 가정하지 않는다. Mac DPR3/좌우102px/하단54px는 채택 시 기록할 가정이며 실제 iOS 인증이 아니다.

## 남은 시안 차이와 동결 조건

사용자 제시32개 중 AX·치수32개 확인, 개별 조작은 PC KO/Portrait EN 2프로토타입, 빈 상태 캡처1장은 실패로 보존했다. 게임의 PC16:9 영웅 판 숨김, tier/count dropdown, 공용 칙령, 결과2열은 시안과 다르며 그대로 보존했다. Quiet 경고 테두리, 아직 연결하지 않은 chart/HUD/edict, 실제 긴 영문과 native after는 미완료다. 임의 글자 축소나 위상석5열 예외를 적용하지 않았다.

개별 검증 큐는 폐쇄하고 [validation-queue.json](TrainingGroundSkinEvidence20261014/validation-queue.json)을 통합 큐 입력으로 전환한다. 실행 대기는 거절이나 불확실한 실행이 아니다. 분기/원화/근거를 삭제하지 않고 2026-10-21 보존 검토까지 유지한다. 새 Unity/빌드/Library 복제/원격 push/공개 배포/main 병합0. 통합 담당의 구체적인 결함 요청 전에는 소스 편집을 재개하지 않는다.

## English handoff

Source is frozen at implementation `2dee3038` / previous clean HEAD `1cbf79b8`; only handoff documents change now. Common cdb9 is integrated, while 0ccba80e was inspected but NOT imported or wired. Apply the screen-only commits above onto the common owner's 0cc baseline without replaying d5/cdb. Preserve the background and audited shared35 portraits; no equipment assets were duplicated. The follow-up audit agent hit model capacity and was not retried after the freeze instruction.

Still unwired: result DPS and skill-card skin arguments; creation-time combat and edict scope with safe lifetime/restoration; remaining shared pause/header surfaces. Casual Quiet still covers part of auto-off row warning borders. Existing static tests passed only for the previous exact input; all native/Unity tests remain unexecuted. The shared owner now owns a single integrated validation plan using the existing Library. Do not resume this screen's edits without a specific integration defect request.
