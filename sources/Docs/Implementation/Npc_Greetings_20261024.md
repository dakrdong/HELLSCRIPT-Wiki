# NPC 고정 인사 재생 준비

기준: `e1c1434a843a552c2c0d72a1100234ac9045c998`. 브랜치: `codex/npc-greetings-20261024`. [English](Npc_Greetings_20261024.en.md)

현재는 선택적 클립 재생 연결만 준비된 상태이며, 실제 NPC 음원은 0개다. 국가 모티브로 모국어를 확정하지 않고, 문구·음성 선택과 유료 생성은 별도 결정을 기다린다. 빈 오디오 파일이나 기존 SFX를 NPC 음성으로 등록하지 않는다.

`GameUI.ShowNpcDialogue`의 기존 거리·페이지·공통 창 검증을 통과해 실제 `ContentWindowView`가 열린 뒤 한 번 시도한다. 일반 인사, 안톤의 소문 이야기, 판매상 선물 이야기가 같은 후처리를 사용한다. 접근 이동, 기존 퀘스트·보상·서비스 동작과 `TalkingNpcId`의 의미는 보존한다.

`Resources/Audio/NpcGreetings/<npc-id>`의 `AudioClip` 하나를 NPC별로 사용한다. 아래 ID는 기존 `NpcProfiles.All`의 12명이며, 리소스 이름에 언어를 붙이지 않는다. 한국어/영어 UI는 같은 승인된 고정 클립을 재사용한다. 대사 전용 화자·주인공과 임의 ID는 재생 대상이 아니다.

| NPC ID | 기존 이름 |
|---|---|
| mark-kus | 마르크 쿠스 |
| jake-bokun | 제이크 보쿤 |
| chador-samaf | 차도르 사마프 |
| anton-jindark | 안톤 진다크 |
| injel-mir | 인젤 미르 |
| pyonya-nermwen | 표냐 내르뭰 |
| jean-jorin | 쟝 죠린 |
| darc-alvi | 달크 알뷔 |
| jacques-chei | 자크 체이 |
| turk-garbi | 터크 가르비 |
| guzel-pan | 구젤 판 |
| mishu-karu | 미슈 카루 |

캐시는 `GameAudio` 인스턴스가 소유하며 실제로 말을 걸 때 지연 조회한다. 없는 클립도 null로 기억해 반복 로드·경고를 만들지 않고 무음으로 끝낸다. 빌드 리소스는 불변으로 취급하며 새 클립을 도입한 빌드/실행에서 새로 조회한다. 아직 재생 중인 UI를 새 파일로 교체하거나 계정에 클립 상태를 저장하지 않는다.

동시에 한 NPC 인사만 시작한다. 같은 대화 창에서는 완료·뮤트·일시정지 뒤에도 다시 시작하지 않고, 새로 말을 걸어 새 창을 열면 같은 캐시 클립을 다시 사용한다. 고정 피치·2D 재생이며 기존 `EffectsLevel`(master×effects, mute 포함)을 따른다. 음량 변경은 즉시 반영되고 0/뮤트·절전·앱 pause·오디오 비활성화/종료에서는 인사를 멈춘다.

오디오 소스는 대화 창 GameObject의 컴포넌트다. 공통 창의 repaint가 자식 UI를 교체해도 소스는 유지되고, 창이 닫히거나 파괴되면 Unity 수명에 따라 즉시 재생이 멎는다. 기존 오디오 볼륨 적용 루프는 활성 인사에 대해서만 페이지·계정·전투 유효성을 확인한다. 페이지 전환의 기존 `CloseNpcDialogue`도 인사만 정리한다. 인사 종료는 다른 효과음이나 예약된 소리를 중단하지 않는다.

새 계정 저장 키·경제 값·전투 RNG·런타임 네트워크 요청·실시간 생성은 추가하지 않았다. UI·지역화·NPC 프로필·원본 음원·serialized asset은 변경하지 않았다.

검증 인계: `Hellscript.Tests.NpcGreetingAudioTests` 및 기존 `SoundBankTests`, `NpcDialogueTests`를 root의 집중 Unity 검사 대상으로 제안한다. 새 테스트는 선택적 무음, 고정 클립 재사용/중복 방지, repaint/닫힘/파괴, 문맥 무효화, 볼륨·뮤트·pause·절전·disable, 다른 효과음 보존과 오디오 소유자 종료를 다룬다. 테스트의 오디오는 메모리 안의 합성 무음 fixture이며 실제 NPC 자산이 아니다.

이 에이전트는 Unity·청음·실제 UI 조작·빌드·CLI 리뷰를 실행하지 않았다. 실제 말걸기 3분기, 닫기/서비스 진입, 페이지·계정 전환, KO/EN·회전 중 재생과 실제 음질은 root의 격리된 개발 환경 검증이 남아 있다. 승인된 음원이 도입되기 전까지 소리가 나지 않는 상태를 완료된 음성 콘텐츠로 표시하지 않는다.

정적 검사: 공통 UI 계약 Python 회귀 15개, UI Refresh(14 bindings/245 UI 파일), 공백 검사는 통과했다. 신규 Unity 테스트는 16개이며 아직 실행하지 않았다. 정적 근거는 Builds/Validation/NpcGreetings20261024/에 보관한다. 이 결과는 C# 컴파일·화면·청음의 통과 근거가 아니다.
