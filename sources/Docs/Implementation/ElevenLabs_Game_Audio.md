# ElevenLabs 게임 음향

갱신일: 2026-10-18 · 작성일: 2026-09-27 · [English](ElevenLabs_Game_Audio.en.md)

사용자가 2026-09-27에 당시 채택된 음원을 최종 버전으로 확정했다(효과음 454개·배경음 4곡, 총 458개 파일). 2026-10-14에 아트 방향이 밝은 캐주얼로 바뀌어 **배경음 4곡을 6곡으로 교체**했으며, 현재 구성은 **효과음 456개(같은 날 병합된 타이틀 비행 효과음 2개 포함)와 배경음 6곡, 총 462개 파일**이다. 모든 효과음 재생 ID와 음악 상황 6종을 연결하며, 효과음 9종은 용도가 맞는 기존 최종 음원을 공유한다. 효과음의 추가 생성은 계획하지 않는다. 아래 458개·배경음 4곡 관련 서술과 검증 근거는 2026-09-27 시점의 기록이다.

**2026-10-18 추가:** D4 스킬 변형 62개에 효과음을 새로 만들어 넣어 현재 효과음은 **518개, 재생 ID는 347종**(카탈로그 기준)이다. 아래 456개·285종과 관련한 서술은 그 직전 시점의 기록이다. 자세한 내용은 [스킬 효과음 프롬프트와 삽입 방식](Skill_Sfx_Prompts_20261018.md)에 있다.

효과음의 최종본 확정은 사용자의 2026-09-27 지시에 따른다. 새로운 사람의 청음 시험이나 모바일 실기기 검증을 수행했다는 뜻은 아니다.

## 2026-10-14 배경음 교체 (밝은 캐주얼 아트 방향)

아트 방향이 어두운 고딕에서 밝고 가벼운 중세 판타지로 바뀌어(화면·전투·저장 구조는 그대로 두고 아트만 변경) 배경음도 새로 만들었다. 이전에는 캐릭터 생성·선택이 타이틀 곡을, 훈련장이 마을 곡을 함께 썼다. 이제 음악 상황 6종이 각자의 곡을 가진다. 곡은 ElevenLabs `eleven_music_v2_5`로 곡마다 한 번씩 생성했고(약 14,400 크레딧), 프롬프트·생성 ID·해시는 [출처 목록](../../AudioSources/ElevenLabs/manifest.json)에 있다. 생성 플로우 ID는 `aKtfRMrdNm1ZoHFt8Y80`이다.

| 상황 | 재생되는 때 | 곡의 방향 | 원본 → 반복용 편집본 | 통합 음량 |
| --- | --- | --- | --- | --- |
| `title` | 타이틀·로그인 | 밝은 영웅 관현악 서곡, D장조, 호른·트럼펫 팡파르 | 150.0초 → 133.4초 | -20 LUFS |
| `characters` | 캐릭터 생성·선택 | 설레는 피치카토·셀레스타·플루트, G장조 | 150.0초 → 136.9초 | -21 LUFS |
| `sanctuary` | 마을·결과 화면 | 햇살 가득한 마을 포크(기타·만돌린·틴 휘슬·아코디언), F장조 | 180.0초 → 143.5초 | -22 LUFS |
| `training` | 훈련장 로비와 훈련 전투 전체(보스 시험·결과 포함) | 경쾌한 연습 행진곡(피치카토·우드블록·바순), C장조 | 150.0초 → 134.0초 | -23 LUFS |
| `rift` | 균열 탐험·전투 | 용감한 모험 전투곡(타이코·호른 훅), E단조에 장조 전환 | 180.0초 → 142.3초 | -23 LUFS |
| `boss` | 균열 보스 | 긴장감 있는 모험풍 보스전(저음 금관 리프·트럼펫), C단조 | 150.0초 → 121.6초 | -21 LUFS |

게임 코드는 `GameAudio.Clips.cs`가 상황 6종을 고른다. 캐릭터 생성·선택 화면(`characters`)과 훈련장은 각자의 곡을 쓰고, 훈련장은 로비가 열린 때부터 훈련 전투·보스 시험·결과까지 같은 곡을 유지한다(보스를 골라도 균열 보스곡으로 바뀌지 않는다). 균열과 보스는 기존 규칙(`RunPhase`)을 유지하며 상황 전환은 1.5초 페이드다.

### 반복 편집

생성된 곡은 끝에 마무리 감쇠가 붙거나(타이틀·마을·균열은 마지막 3초 안팎에서 소리가 줄어든다) 박 위치가 처음과 맞지 않았다. 기존 방식(원본 전체 길이에서 끝 2.5초를 처음 2.5초와 교차 연결)을 그대로 쓰면 반복마다 소리가 푹 꺼지거나 박이 밀린다. 생성된 곡의 실제 박자도 프롬프트의 BPM과 다르므로(예: 보스 150 → 약 157 BPM) 고정된 마디 길이로 자를 수 없다.

`tools/elevenlabs_audio.py --loop-end <원본.mp3>`가 끝 지점을 찾는다. 원본 길이의 75% 이후에서 마지막 2.5초가 첫 2.5초와 음색(50ms 로그 대역 프레임)과 리듬(10ms 온셋 포락선)이 가장 비슷한 위치를 고르고, 저역 파형으로 샘플 단위까지 맞춘다. 고른 값은 출처 목록의 `selection.end_seconds`에 남으며 `--rebuild`가 같은 결과를 만든다. 이전 4곡의 `selection`에는 이 값이 없었으므로 원본 전체 길이를 썼다.

[측정 결과](ElevenLabsAudioEvidence/music-casual-master-audit-20261014.json)는 다음과 같다. 모두 신호 측정이며 청음이 아니다.

- 통합 음량은 6곡 모두 목표의 ±0.02 LUFS 안이고, true peak는 -9.7~-6.2 dBTP로 상한(-2.5 dBTP)보다 낮다.
- 반복 경계의 샘플 간 단차는 각 곡 자체의 최대 단차의 10% 이하다(연속).
- 교차 연결되는 마지막·처음 2.5초의 박 위치 차이는 끝 지점을 고르기 전 -310~+500ms에서 고른 뒤 0~±10ms로 줄었다(마을 -310→-10, 훈련장 +500→0, 균열 +80→0, 보스 +90→+10, 타이틀 -20→-10). 캐릭터는 원본 길이가 이미 박에 맞아 있어 0→+10ms로 같은 수준이다.
- 반복 경계 근처에서 가장 깊은 음량 저하는 타이틀 -19.6→-6.8 dB, 마을 -22.9→-12.5 dB, 균열 -34.2→-13.9 dB로 줄었다. 마을의 남은 -12.5 dB는 곡 자체의 조용한 도입부다. 훈련장은 스타카토 곡이라 박 사이 쉼 때문에 경계와 무관하게 어디서나 -27 dB 안팎까지 내려간다.

### 검증 상태

- 사용자 승인: 사용자가 2026-10-14에 곡을 확인하고 "괜찮다"며 진행을 지시했다. 점수표나 별도의 청음 기록은 없으며 출처 목록의 `listening_review`는 `pending` 그대로다.
- 통과: 오프라인 도구 테스트 8개(반복 끝 찾기 포함), 새 음악 6곡의 출처 검사(모델·원본·편집본 해시·경로, 문제 0건)와 원본에서의 바이트 단위 재현, 병합 후보의 전체 Unity Edit Mode 5,752개 중 5,748개 통과·3개 건너뜀(제작용 탐침 시험)·1개 실패(`LocalizationTests.NoEntryIsLeftBehindByARewordedLine`: `en.txt`에 원본 줄이 없는 항목 하나. `main`의 f726d865에서 생긴 기존 실패이며 이번 변경과 무관, 약 90분). 집중 검사 `SoundBankTests`는 23개가 통과했고 새 WAV 6개의 가져오기를 확인했다([결과](ElevenLabsAudioEvidence/music-casual-soundbank-editmode-20261014.json)).
- macOS 개발 빌드(오류 0)의 오디오 스모크 통과: 타이틀·마을·훈련장(로비를 연 상태)·균열·보스 5개 상황에서 실제 스트림이 진행·반복·전환하고, 음량·음소거 설정이 재시작 뒤에도 유지된다. 새 6곡은 개발 빌드에서 합계 31.97 MiB(빌드 964 MiB의 3.3%)다. 교체 전 4곡의 실측값은 없어 증감은 길이(650초 → 812초)로만 추정한다.
- 스모크 수정: 앱 일시정지 확인은 오디오 스레드가 일시정지를 반영하기 전에 위치를 읽어 8회 중 2회 실패했다(음악과 무관, 위치가 버퍼 1개 분량 0.021초 이동). 일시정지 0.1초 뒤에 위치를 읽도록 고쳐 12회 연속 통과했다.
- 아직 하지 않음: 캐릭터 생성·선택 상황의 네이티브 재생(화면 진입에 로그인이 필요해 Edit Mode 매핑 테스트만 있음), 모바일 실기기.
- 이전부터 있던 실패: `elevenlabs_audio.py --check`와 `generate_sfx.py --check`의 전체 실행은 효과음 카탈로그와 뱅크의 불일치(`skill.W08-3`, `skill_M13_1.wav`, `skill_M15_1.wav` 등)로 실패한다. 효과음 관련 파일이 기반인 `origin/main`과 같으므로 이번 변경과 무관하며, 음악 6개 항목은 따로 검사해 문제가 없었다.
- 복구: 교체 전 어두운 고딕 4곡과 원본은 `main`의 `a113f4ff`에서 복구할 수 있다.

## 최종 구성과 공유 규칙

전용 효과음 원본을 가진 ID는 274종이며, 다음 9개 ID는 동일한 `AudioClip` 에셋을 공유한다. 파일을 복사하거나 같은 음원을 별도의 변주로 등록하지 않는다. 각 행동의 ID, 재생 간격, 우선순위와 동시 재생 제한은 유지한다.

| 게임 행동 | 공유하는 최종 음원 | 선택 이유 |
| --- | --- | --- |
| `reward.claim` | `ui.confirm` | 보상 수령의 성공 확인 |
| `shop.gamble_beat` | `ui.click_heavy` | 공개 과정의 짧고 무게감 있는 타격 |
| `gem.socket` | `item.equip.orb` | 수정 구체가 금속 받침에 들어가는 소리 |
| `reward.box_open` | `reward.chest` | 금속 잠금과 나무 경첩이 있는 보물 상자 |
| `rune.fuse` | `gem.fuse` | 여러 수정 음색이 하나로 모이는 합성 |
| `rune.place` | `forge.core_invest` | 돌 재료를 홈에 끼우는 소리 |
| `rune.remove` | `gem.remove` | 장착된 재료를 해제하는 클릭 |
| `rune.rotate` | `loot.stone` | 돌이 맞닿는 짧은 클릭 |
| `rune.unlock` | `aspect.upgrade` | 마법 각인이 위로 울리는 개방 피드백 |

[최종 카탈로그](../../AudioSources/ElevenLabs/sfx-catalog.json)가 음량 기준과 공유 관계의 원본이다. [런타임 뱅크](../../Assets/HELLSCRIPT/Resources/Audio/Sfx/bank.json)는 이 카탈로그로 생성한다. [제작 목록](../../AudioSources/ElevenLabs/production-plan.json)은 전용 음원과 공유 음원을 구분하며, 기존 일일 한도 대기 상태를 종료했다.

## 이전 파일 정리와 용량

- 게임에서 사용하던 구형 DSP 효과음 15개와 해당 `.meta` 파일을 삭제했다. 제거 전 다른 Unity 에셋의 GUID 참조가 없는지 검사했다. 최종 음원 458개의 바이트, GUID와 가져오기 설정은 보존했다.
- 생성 후보 1,087개의 다운로드 사본, 이전 미리듣기 변환본, 게임 파일의 미리듣기용 복사본, 이전 테스트 빌드와 임시 저장 데이터를 삭제했다. 선택된 원본 458개는 `AudioSources/ElevenLabs/Sources`에 한 벌만 남긴다.
- 구형 합성 제작법과 합성 전용 코드를 제거했다. 기존 명령 이름인 `generate_sfx.py`는 최종 파일의 검사·카탈로그 생성·미리듣기만 수행한다. 인자 없이 실행하면 합성 대신 오류로 종료한다.
- 미리듣기는 실제 게임용 파일을 가리키는 심볼릭 링크를 사용한다. 페이지를 다시 생성해도 WAV·MP3 사본을 만들지 않는다.
- Git 이력은 변경하지 않는다. 삭제된 추적 파일은 정리 전 커밋 `949b31af520249e3c7708c7ef96256099f293cf6`에서 복구할 수 있다. 다른 작업 폴더와 공용 Unity 캐시는 정리 대상에 포함하지 않았다.

구형 게임 WAV는 1,504,326바이트를 차지했다. 최종 게임용 WAV의 합계는 176,891,768바이트다. 작업용 제작·검증 산출물은 약 3.00GB에서 13.7MB로 줄어 **약 2.98GB를 절약**했다. 검증에 사용한 새 빌드와 임시 계정도 제거했다. 자세한 삭제량은 [정리 결과](ElevenLabsAudioEvidence/final-cleanup.json)에 기록한다. 원본 용량의 감소를 모바일 빌드 크기나 실행 중 메모리의 동일한 감소량으로 해석하지 않는다.

## 출처와 편집 품질 유지

| 항목 | 효과음 | 배경음 |
| --- | --- | --- |
| MCP에서 확인한 모델 | `eleven_text_to_sound_v2` | `eleven_music_v2_5` (2026-10-14 교체본, 교체 전 4곡은 `eleven_music_v2`) |
| 생성 원본 | MP3, 44.1 kHz, 스테레오, 128 kbps | MP3, 48 kHz, 스테레오, 192 kbps |
| 최종 편집본 | WAV, 48 kHz, PCM16, 모노 | WAV, 48 kHz, PCM16, 스테레오 |
| Unity 가져오기 | 기존 ADPCM 또는 Vorbis, 메모리에 압축 | Streaming, Vorbis 품질 0.9, 스테레오, 미리 로드하지 않음 |

[출처 목록](../../AudioSources/ElevenLabs/manifest.json)에 모델·생성 ID·프롬프트·매개변수·원본 및 편집본 해시가 있다. 원본은 Unity 빌드 밖에 보관하며, 재편집과 출처 확인에 사용한다. MP3를 WAV로 변환해도 손실된 정보가 복구되지는 않는다. 이번 정리에서 재인코딩이나 음질을 낮추는 처리는 하지 않았다.

효과음은 무음 제거, DC 제거, 짧은 페이드, 선형 음량 조정과 -2 dBTP 상한을 사용한다. 슬라이더는 30ms 한 번의 클릭이며, 탭 전환음 세 변주의 누적 에너지가 5%에 도달하는 시점은 약 5~19ms다. [UI 편집 결과](ElevenLabsAudioEvidence/ui-transient-edit.json)

배경음은 타이틀 133.4초, 캐릭터 136.9초, 마을 143.5초, 훈련장 134.0초, 균열 142.3초, 보스 121.6초다. 끝 지점을 고른 뒤 끝과 처음을 2.5초 교차 연결했고, 게임 상황 전환은 1.5초 페이드로 처리한다. 선택되지 않은 곡은 일시정지한다. 기존 16개 효과음 재생기, 우선순위·동시 재생 제한과 최종 믹스 제한기를 유지한다. 음향은 저장이나 전투 난수를 바꾸지 않는다.

## 관리와 검증 명령

```sh
python3 tools/elevenlabs_audio.py --check
python3 tools/generate_sfx.py --check --audit Artifacts/audio-bank-audit.json
python3 tools/test_elevenlabs_audio.py
python3 tools/elevenlabs_audio.py --rebuild
python3 tools/elevenlabs_audio.py --loop-end <새 음악 원본.mp3>
python3 tools/audio_audition.py
```

전체 출처 검사는 462개 파일 모두를 요구하며, 미완료 허용 옵션은 필요하지 않다. 남아 있는 구형 WAV, 참조되지 않는 파일, 변조된 원본이나 편집본은 검사에서 실패한다. `--rebuild`는 보존한 최종 원본을 사용하고 공유 ID는 건너뛰므로 중복 파일을 만들지 않는다. API 호출이나 크레딧 사용은 없다.

## 검증 근거

아래는 2026-09-27 정리 시점(458개)의 기록이다. 2026-10-14 배경음 교체의 검증은 위 절에 따로 적었다.

이번 정리 후 검증은 [정리 결과](ElevenLabsAudioEvidence/final-cleanup.json)와 [Unity 검사](ElevenLabsAudioEvidence/editmode-final-summary.json)에 기록한다. 최종 음원 458개의 해시와 `.meta`가 정리 전과 같고, 모든 재생 ID가 남아 있으며, 공유 ID가 실제 동일한 가져오기 에셋을 재생하는지 확인한다.

Unity Edit Mode 31개, 오프라인 도구 7개와 공통 UI 계약 검사 9개가 통과했다. [전체 출처 검사](ElevenLabsAudioEvidence/final-source-validation.json)는 458개 모두 통과하고 누락이 0개다. macOS 개발 빌드는 오류 0으로 성공했다. 공유 효과음 9종은 실제 가져온 에셋과 청취 지점의 출력으로 검증했고, 배경음 4곡의 전환·반복, 세 직업의 짧은 개발용 훈련, 음량·음소거 및 재시작 후 설정 복원도 통과했다. [공유 효과음](ElevenLabsAudioEvidence/native-final-shared-effects.txt) · [음악](ElevenLabsAudioEvidence/native-final-music.txt) · [조작·직업](ElevenLabsAudioEvidence/native-final-audio.txt) · [재시작](ElevenLabsAudioEvidence/native-final-restart.txt)

전체 게임 흐름 자동 검사는 1,074회·76종의 재생을 기록했고 음원 누락과 오류 로그는 0개였다. 다만 선택 검사인 가방 조작이 실패한 뒤 궁수의 균열 관리인 접근이 시간 초과되어 **전체 검사 결과는 실패**다. 마을 대장간도 포인터 차단으로 건너뛰었다. 앞의 음향 집중 검사 통과를 전체 게임 검사 통과로 대체하지 않는다. [전체 실행 요약](ElevenLabsAudioEvidence/native-cleanup-full-summary.txt) · [재생 기록](ElevenLabsAudioEvidence/native-cleanup-full-trace.tsv)

이전 제작 단계의 신호 측정은 바이트가 보존된 파일에 한해 계속 유효하다. 효과음 454개는 클리핑 0, 최대 DC 절댓값 0.000260, true peak 약 -2 dBTP였다. [효과음 검사](ElevenLabsAudioEvidence/runtime-master-audit.json) · [교체 전 배경음 4곡 검사](ElevenLabsAudioEvidence/music-master-audit.json)

이전 음악·설정 실행에서는 교체 전 네 음악의 스트림 진행·반복·전환, 음량·음소거와 재시작 후 설정 복원을 확인했다. [음악](ElevenLabsAudioEvidence/native-audio-music.txt) · [설정](ElevenLabsAudioEvidence/native-audio-initial.txt) · [재시작](ElevenLabsAudioEvidence/native-audio-restart.txt)

macOS 게임 흐름 검사는 합성 EventSystem 입력과 임시 계정을 사용한다. 일부 마을 버튼은 가려져 콜백을 직접 호출하며, 선택 검사인 대장간 조작은 포인터 차단 시 건너뛴다. 이 결과를 실제 모바일 터치나 성능 검사로 보고하지 않는다. 공개 위키는 병합된 `main`에서만 게시한다.

## 추가: 타이틀 로그인 연출 효과음 (2026-10-14)

[타이틀 로그인 연출](Title_Screen.md)에 쓸 효과음 2종을 ElevenLabs `eleven_text_to_sound_v2`(MCP, 프롬프트 반영도 0.65)로 새로 만들었다. 신호마다 4개씩 생성하고 음량 곡선으로 1개를 골랐다.

| 신호 | 용도 | 채택 생성 ID | 길이 |
|---|---|---|---|
| `flow.title_flight` | 출발할 때 바람 돌진 | `PmH0ZtJAEtC86aHk2V7O` | 1.70초 |
| `flow.title_rift` | 균열 빛이 터질 때 | `e9C3rrKPb3aFTcuHuWXP` | 1.50초 |

원본 MP3는 `AudioSources/ElevenLabs/Sources/`에 보존했고 `elevenlabs_audio.py`로 마스터했다. 파일은 효과음 456개와 당시 배경음 4곡(같은 날 위 '2026-10-14 배경음 교체'로 6곡이 됐다), 재생 ID는 285종이 됐다. 생성 비용은 8개에 54.7크레딧이었다(사전 견적은 180크레딧). 사람이 직접 들어 본 확인은 하지 않았다.

참고: 같은 날 `main`에서는 D4 스킬 ID 이전 때문에 `sfx-catalog.json`과 `bank.json`이 서로 달라 `generate_sfx.py --check`와 `elevenlabs_audio.py --check`가 실패한다(이번 변경 전부터 그렇다). 새 두 신호의 뱅크 행은 생성기가 쓰는 형식과 같고, `test_elevenlabs_audio.py`는 통과한다.

## 추가: 스킬 변형 효과음 62개 (2026-10-18)

D4 스킬 변형 84개 중 22개만 소리가 있었고 나머지는 기본 스킬과 같은 소리로 났다. 소리가 없던 62개를 ElevenLabs `eleven_text_to_sound_v2`(MCP, 프롬프트 반영도 0.65, 변형마다 1회 생성)로 만들었다. 3개(`A18-1`·`M08-1`·`M11-2`)는 처음 결과가 거의 무음이거나 끝나지 않고 잘려 프롬프트를 고쳐 한 번 더 만들었다. 프롬프트·생성 ID·해시는 [출처 목록](../../AudioSources/ElevenLabs/manifest.json)에 있고 비용은 354.6크레딧(생성 플로우 `tcHr1hKn6p0at4pzyUOg`)이다. 원본은 `AudioSources/ElevenLabs/Sources/`에 보존했다.

`GameAudio`는 시전한 변형에 자기 cue(`skill.<스킬>-<번호>`)가 있으면 시점 표에 따라 재생한다(변형 84개 중 69개는 기본 스킬 cue 대신 같은 시전 순간에, 15개는 준비 시작·착지·후속 피해 슬롯이나 시뮬레이션 사건의 순간에). 프롬프트와 시점, 보류 항목은 [스킬 효과음 프롬프트와 삽입 방식](Skill_Sfx_Prompts_20261018.md)에 정리했다. 새 62개는 출처 기록과 신호 규칙을 모두 통과했고(클리핑 0, 양 끝 0), Unity Edit Mode `SkillModifierAudioTests` 14개(시점 표를 더한 뒤 `SoundBankTests`와 함께 다시 실행)·`SoundBankTests`·`NewContentAudioTests`와 오프라인 도구 시험 8개가 통과했다. 사람이 직접 들어 본 확인은 하지 않았다.

같은 날 보류했던 기존 마스터 교체 11행(14클립)도 새로 만들어 같은 클립 파일 이름으로 교체했다(플로우 `81oyAOAv3bzfGC0fFupM`, 64.0크레딧). 새 음원은 출처 기록과 신호 규칙을 통과했고 `SoundBankTests`·`SkillModifierAudioTests` 29개와 오프라인 도구 시험 8개가 통과했다. 교체로 manifest에서 빠진 옛 원본 MP3 14개는 사용자 확인 뒤 지웠다(git 이력에서 복구 가능).

참고: 위 2026-10-14 절의 `--check` 실패(카탈로그와 뱅크의 D4 ID 불일치)는 그대로 남아 있고, 새 62개의 카탈로그·뱅크 행은 생성기 형식과 같다.
