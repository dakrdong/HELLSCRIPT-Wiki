# ElevenLabs 게임 음향

작성일: 2026-09-27 · [English](ElevenLabs_Game_Audio.en.md)

사용자가 현재 채택된 음원을 최종 버전으로 확정했다. **효과음 454개와 배경음 4곡, 총 458개 파일**을 유지한다. 효과음 재생 ID 283종과 음악 상황 4종을 모두 연결하며, 효과음 9종은 용도가 맞는 기존 최종 음원을 공유한다. 추가 음원 생성은 계획하지 않는다.

최종본 확정은 사용자의 이번 지시에 따른다. 새로운 사람의 청음 시험이나 모바일 실기기 검증을 수행했다는 뜻은 아니다. 이 결과는 작업 브랜치에 있으며 `main` 병합과 공개 위키 배포는 별도다.

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
| MCP에서 확인한 모델 | `eleven_text_to_sound_v2` | `eleven_music_v2` |
| 생성 원본 | MP3, 44.1 kHz, 스테레오, 128 kbps | MP3, 48 kHz, 스테레오, 192 kbps |
| 최종 편집본 | WAV, 48 kHz, PCM16, 모노 | WAV, 48 kHz, PCM16, 스테레오 |
| Unity 가져오기 | 기존 ADPCM 또는 Vorbis, 메모리에 압축 | Streaming, Vorbis 품질 0.9, 스테레오, 미리 로드하지 않음 |

[출처 목록](../../AudioSources/ElevenLabs/manifest.json)에 모델·생성 ID·프롬프트·매개변수·원본 및 편집본 해시가 있다. 원본은 Unity 빌드 밖에 보관하며, 재편집과 출처 확인에 사용한다. MP3를 WAV로 변환해도 손실된 정보가 복구되지는 않는다. 이번 정리에서 재인코딩이나 음질을 낮추는 처리는 하지 않았다.

효과음은 무음 제거, DC 제거, 짧은 페이드, 선형 음량 조정과 -2 dBTP 상한을 사용한다. 슬라이더는 30ms 한 번의 클릭이며, 탭 전환음 세 변주의 누적 에너지가 5%에 도달하는 시점은 약 5~19ms다. [UI 편집 결과](ElevenLabsAudioEvidence/ui-transient-edit.json)

배경음은 타이틀 147.5초, 성역 177.5초, 균열 177.5초, 보스 147.5초다. 끝과 처음을 2.5초 교차 연결했고, 게임 상황 전환은 1.5초 페이드로 처리한다. 선택되지 않은 곡은 일시정지한다. 기존 16개 효과음 재생기, 우선순위·동시 재생 제한과 최종 믹스 제한기를 유지한다. 음향은 저장이나 전투 난수를 바꾸지 않는다.

## 관리와 검증 명령

```sh
python3 tools/elevenlabs_audio.py --check
python3 tools/generate_sfx.py --check --audit Artifacts/audio-bank-audit.json
python3 tools/test_elevenlabs_audio.py
python3 tools/elevenlabs_audio.py --rebuild
python3 tools/audio_audition.py
```

전체 출처 검사는 458개 파일 모두를 요구하며, 미완료 허용 옵션은 필요하지 않다. 남아 있는 구형 WAV, 참조되지 않는 파일, 변조된 원본이나 편집본은 검사에서 실패한다. `--rebuild`는 보존한 최종 원본을 사용하고 공유 ID는 건너뛰므로 중복 파일을 만들지 않는다. API 호출이나 크레딧 사용은 없다.

## 검증 근거

이번 정리 후 검증은 [정리 결과](ElevenLabsAudioEvidence/final-cleanup.json)와 [Unity 검사](ElevenLabsAudioEvidence/editmode-final-summary.json)에 기록한다. 최종 음원 458개의 해시와 `.meta`가 정리 전과 같고, 모든 재생 ID가 남아 있으며, 공유 ID가 실제 동일한 가져오기 에셋을 재생하는지 확인한다.

Unity Edit Mode 31개, 오프라인 도구 7개와 공통 UI 계약 검사 9개가 통과했다. [전체 출처 검사](ElevenLabsAudioEvidence/final-source-validation.json)는 458개 모두 통과하고 누락이 0개다. macOS 개발 빌드는 오류 0으로 성공했다. 공유 효과음 9종은 실제 가져온 에셋과 청취 지점의 출력으로 검증했고, 배경음 4곡의 전환·반복, 세 직업의 짧은 개발용 훈련, 음량·음소거 및 재시작 후 설정 복원도 통과했다. [공유 효과음](ElevenLabsAudioEvidence/native-final-shared-effects.txt) · [음악](ElevenLabsAudioEvidence/native-final-music.txt) · [조작·직업](ElevenLabsAudioEvidence/native-final-audio.txt) · [재시작](ElevenLabsAudioEvidence/native-final-restart.txt)

전체 게임 흐름 자동 검사는 1,074회·76종의 재생을 기록했고 음원 누락과 오류 로그는 0개였다. 다만 선택 검사인 가방 조작이 실패한 뒤 궁수의 균열 관리인 접근이 시간 초과되어 **전체 검사 결과는 실패**다. 마을 대장간도 포인터 차단으로 건너뛰었다. 앞의 음향 집중 검사 통과를 전체 게임 검사 통과로 대체하지 않는다. [전체 실행 요약](ElevenLabsAudioEvidence/native-cleanup-full-summary.txt) · [재생 기록](ElevenLabsAudioEvidence/native-cleanup-full-trace.tsv)

이전 제작 단계의 신호 측정은 바이트가 보존된 파일에 한해 계속 유효하다. 효과음 454개는 클리핑 0, 최대 DC 절댓값 0.000260, true peak 약 -2 dBTP였다. [효과음 검사](ElevenLabsAudioEvidence/runtime-master-audit.json) · [음악 검사](ElevenLabsAudioEvidence/music-master-audit.json)

이전 음악·설정 실행에서는 네 음악의 스트림 진행·반복·전환, 음량·음소거와 재시작 후 설정 복원을 확인했다. [음악](ElevenLabsAudioEvidence/native-audio-music.txt) · [설정](ElevenLabsAudioEvidence/native-audio-initial.txt) · [재시작](ElevenLabsAudioEvidence/native-audio-restart.txt)

macOS 게임 흐름 검사는 합성 EventSystem 입력과 임시 계정을 사용한다. 일부 마을 버튼은 가려져 콜백을 직접 호출하며, 선택 검사인 대장간 조작은 포인터 차단 시 건너뛴다. 이 결과를 실제 모바일 터치나 성능 검사로 보고하지 않는다. 공개 위키는 병합된 `main`에서만 게시한다.
