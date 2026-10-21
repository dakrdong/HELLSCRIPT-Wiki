# 스킬 효과음 프롬프트와 삽입 방식

갱신일: 2026-10-18 · 작성일: 2026-10-18 · [English](Skill_Sfx_Prompts_20261018.en.md)

D4 스킬 트리의 능동 스킬 30개는 모두 기본 효과음을 갖고 있었지만, 변형 84개는 전부 기본 스킬과 같은 소리로 났다. 이번에 변형마다 자기 소리를 갖게 했다. 변형 84개 중 22개는 옛 스킬의 기존 음원을 변형 ID로 이어받았고(전투에서 처음 재생된다), **62개는 2026-10-18에 ElevenLabs로 새로 만들어 삽입**했다. 같은 날 이어받은 소리 중 5개(`W02-1`·`W06-3`·`A03-3`·`M02-3`·`M07-3`)와 기본음 3개(`A06`·`A18`·`M07`, 후속 피해 `A03-3.hit`·`A18.hit`·`M02-3.hit` 포함)는 **다시 만들어 마스터를 교체**했다(14클립). 이 문서는 스킬마다 어떤 소리를 언제 재생하는지, 새 소리를 어떤 프롬프트로 만들었는지를 한곳에 정리한다.

## 만드는 법

- 모델은 `eleven_text_to_sound_v2`, 프롬프트 반영도 0.65, 반복 없음, 길이는 소리마다 지정(0.8–3.2초 요청)한다. 변형 하나에 한 번 생성했다. 처음 결과가 거의 무음이거나 소리가 끝나지 않고 잘린 3개(`A18-1`·`M08-1`·`M11-2`)만 프롬프트를 고쳐 한 번 더 만들었다.
- 프롬프트는 기존 효과음의 문체를 따른다. 소리의 주체 한 가지, 재질·소리 질감, 그리고 마무리 문구 "isolated with a clear attack and controlled natural decay."를 붙인다. 한 번의 생성에는 한 가지 소리만 담는다. 시각 연출 용어는 쓰지 않는다.
- 만든 원본 MP3는 `AudioSources/ElevenLabs/Sources/`에 그대로 보존하고, 마스터(48 kHz 모노 PCM, 소리마다 정한 LUFS 목표, true peak -2 dBTP)는 `tools/elevenlabs_audio.py`가 만든다. 프롬프트·생성 ID·해시는 [출처 목록](../../AudioSources/ElevenLabs/manifest.json)에, 계획과 생성 결과는 [계획 파일](../../AudioSources/ElevenLabs/skill-sfx-d4-plan.json)에 있다.
- 생성 플로우 ID는 `tcHr1hKn6p0at4pzyUOg`, 비용은 354.6크레딧(다시 만든 3개 포함), 새 음원의 총 길이는 89.7초다.
- 교체 14클립은 플로우 `81oyAOAv3bzfGC0fFupM`에서 변형마다 한 번씩(플랜 프롬프트 그대로) 만들었고 64.0크레딧(다시 만든 1개 포함), 총 길이 16.8초다. 클립 파일 이름은 바꾸지 않고 같은 파일을 덮었다(예: `skill_W11_1.wav`가 은행 cue `skill.W02-1`). `skill.M02-3.hit`은 첫 결과가 목표 음량보다 10 LU 낮아(기존 -20.2 LUFS, 새 첫 결과 -25.2) 저음 한 번과 파편 여운을 더한 프롬프트로 한 번 다시 만들었다. 버린 첫 결과는 채택하지도 보존하지도 않았다.

## 재생 방식

- 재생 ID는 `skill.<스킬>-<변형 번호>`다. 시전한 스킬의 변형에 자기 cue가 있으면 **시점 표**에 따라 재생되고, 없으면 기본 cue가 재생된다(`GameAudio.ModifierCue`·`GameAudio.Moments`). 변형 84개의 시점은 아래 표의 "시점" 열에 모두 있고, `SkillModifierAudioTests`가 84개 전부를 코드와 이 문서(한국어·영어)에 고정한다.
- 기준은 **그 소리가 담은 사건이 일어나는 순간**이다. 실제 시점은 시뮬레이션이 남기는 `ClassSkillEvent`·`effectEvents`와 `Runtime/Core/SkillTreeD4`의 효과 코드를 읽어 정했다. 시점은 세 종류다.
  - **시전**(기본, 69개): 변형이 시전 자체의 소리를 바꾼다. 변형 cue가 기본 스킬 cue 대신 같은 순간(시전·도약 출발·회오리 시작)에 재생된다.
  - **슬롯**(6개: `start`·`land`·`hit`): 스킬의 기본 cue 가운데 그 슬롯의 것을 변형 cue가 대신한다. `start`는 준비 시작(`W08-1`·`W18-1`·`M17-2`), `land`는 착지(`W02-2`), `hit`는 후속 피해(`W17-2`·`A18-2`, 기본 후속 피해 cue와 같은 0.25초 간격 제한).
  - **사건**(9개, `종류:출처`): 시전 순간에는 기본 스킬 cue가 그대로 나고, 시뮬레이션이 그 사건을 보고하는 순간에 변형 cue를 더한다. 종류는 `DAMAGE`(피해. 출처 `EXTRA:W03`은 보조 피해, `A05`는 스킬 자신의 피해)·`CASHOUT`·`EFFECT`·`STATUS`다. 기호 `*`는 사건마다 재생(0.3초에 한 번까지), 없으면 시전당 한 번, `+`는 시전 자신의 첫 사건(표식 자체)을 건너뛴다. 사건 시점은 피해·상태가 실제로 생길 때 난다. 맞을 적이 없어 피해가 없는 폭발은 소리도 없다.
- 후속 피해(함정 폭발·쇠뇌 포탑·귀환 서리 구체 등)는 변형에 `.hit` cue가 있을 때만 변형 소리를 재생하며, cue마다 정해 둔 재사용 대기시간이 겹침을 막는다.
- 시점은 소리를 들어 보지 않고 시뮬레이션 사건과 프롬프트로 정했다. 청음 뒤 바꿀 때는 `GameAudio.Moments`의 한 줄과 아래 표, `SkillModifierAudioTests`의 표를 함께 고친다.

### 시전이 아닌 시점 15개와 근거

| 변형 | 시점 | 시뮬레이션에서 확인한 사건 |
|---|---|---|
| `W08-1` 돌진 갈고리 | 준비 시작 | 영웅이 준비 0.55초 동안 대상까지 날아간다(`MoveD4WarriorCast`). 사슬·바람·착지 소리가 그 이동에 걸쳐 있다. |
| `W18-1` 대지의 심판 | 준비 시작 | 준비가 1.8초로 길다. 소리의 가장 큰 충격이 파일 1.5초 지점에 있다(이전에 정함). |
| `M17-2` 추적 붕괴 | 준비 시작 | 준비 0.5초 뒤 붕괴 3번이 발동·0.5초·1.0초에 일어난다. 소리의 가장 큰 충격이 파일 약 0.6초 지점이라 준비 시작에 재생하면 첫 붕괴와 맞는다. |
| `W02-2` 지진 착지 | 착지 | 착지(발동) 순간에 충격이 나고 진동은 2초 뒤다(이전에 정함). |
| `W17-2` 선조의 기술 | 후속 피해 | 조상이 마지막 능동 스킬을 따라 칠 때마다 `EXTRA:W17` 피해가 난다. 기본 `W17.hit`을 대신한다. |
| `A18-2` 잔상 폭발 | 후속 피해 | 잔상이 사라질 때마다 `EXTRA:A18` 피해가 난다(`FadeD4Afterimage`). 기본 `A18.hit`을 대신한다. |
| `W09-1` 피의 회수 | `CASHOUT` | 3번째 시전마다 출혈을 정산(`CASHOUT`)하고 회복한다. 그 외 시전에는 기본 소리만 난다. |
| `W03-3` 여진 | `EXTRA:W03` 피해 | 시전 1초 뒤에 여진 2개가 같은 순간에 터진다(`D4WarriorBurst`). 시전에는 기본 분쇄 소리가 난다. |
| `M06-1` 이중 파동 | `EXTRA:M06` 피해 | 시전 1초 뒤 두 번째 서리 파동이 터진다(`d4nova`). 시전에는 기본 폭발 소리가 난다. 소리의 첫 큰 소리가 파일 0.6초 지점이라 청음 뒤 앞당길 수 있다. |
| `A14-3` 귀환 화살 | `EXTRA:A14` 피해 | 첫 화살이 맞은 뒤에 갈라진 화살이 날아가 맞고 되돌아온다. 그 보조 피해가 처음 날 때 재생한다(시전당 한 번). |
| `A06-1` 그림자 주입 | `EXTRA:A06` 피해(사건마다) | 주입 화살에 쓰러진 적이 어둠으로 폭발한다(`D4Imbuement`). 처치마다 한 번. |
| `A05-1` 응시 사격 | `A05` 피해 | 표식은 시전에, 화살은 조준 0.9초가 끝난 뒤에 나간다(`d4aim`). 시전 순간에는 기본 표식 소리와 조준 시작 소리(`A05-1.start`)가 난다. 조준이 끊기면(이동) 화살도 소리도 없다. |
| `M01-2` 분열 화염구 | `M01` 피해 | 화염구가 날아가 폭발해 피해를 줄 때 세 갈래가 갈라진다. 시전에는 기본 발사 소리가 난다. |
| `A05-2` 사냥 준비 | `A05:preparation` 효과 | 표식 대상이 쓰러질 때 준비 효과가 생긴다(`D4RangerDeath`). |
| `A05-3` 연쇄 표식 | `A05` 상태(표식 이동) | 표식 대상이 쓰러져 다음 적에게 새 표식이 걸릴 때마다(`STATUS` effectEvent). 시전의 첫 `STATUS`는 표식 자체라 건너뛴다. |

시전 순간에 둔 것 가운데 시뮬레이션과 어긋남이 남은 것(청음 뒤 정한다):

- `M04-1` 화염 잔불: 떠난 자리의 폭발이 이동과 같은 틱에 일어난다(`D4MageHit(..., origin: cast.origin)`이 위치를 옮기기 전에 실행된다). 시전 순간이 맞다.
- `W04-3` 연속 강타: 첫 내려찍기는 시전, 나머지 둘은 0.6초·1.2초 뒤(`EXTRA:W04`)다. 소리 하나에 세 번의 박자가 담겨 시전에 둔다.
- `M11-3` 과충전: 폭발은 구체 수명이 끝날 때인데, 소리는 상승음 뒤에 폭발음이 오는 구조라 시전·폭발 어느 쪽에도 딱 맞지 않는다. 시전에 둔다. 폭발 순간에는 기본 `M11.hit`이 난다.
- `M01-1` 유성: 소리가 낙하(약 1초)와 충격을 함께 담아 시전에 둔다. 충격 순간에 기본 `skill.M01.hit`도 재생되어 충격이 겹친다.
- `A07-3` 마무리 화살: 세 번째 화살은 시전 0.3초 뒤에 나가지만 소리는 그 한 발이다. 시전에 둔다.
- `A04-1` 미끼 투영: 분신은 착지에 생기지만 후퇴 도약의 소리 슬롯(도약 출발)에 둔다. 착지 순간에는 소리가 나지 않는 구조다.

## 검사

- 새 음원 62개와 교체 14클립의 출처 기록(원본·해시·프롬프트)과 신호 규칙(클리핑 0, 양 끝 0, DC, true peak)을 모두 확인했다. 변형 84개가 모두 cue를 가졌는지, 변형 84개의 시점 표(코드와 이 문서의 한국어·영어 표가 같은지 포함), 시점별 cue 선택, 시뮬레이션 사건을 받은 재생과 반복 간격은 Unity Edit Mode 테스트(`SkillModifierAudioTests`)가 확인한다.
- 사람이 직접 들어 본 확인은 하지 않았다. 아래 10개는 소리가 짧은 충격음 위주라 최대 음량 제한 때문에 목표 음량보다 4 LU 이상 낮다: `skill.W04-3`, `skill.M08-1`, `skill.A02-3`, `skill.A06-3`, `skill.A05-3`, `skill.M06-1`, `skill.M06-3`, `skill.M17-2`, `skill.W18-1`, `skill.M02-3.hit`(목표 -15, 실제 -21.5 LUFS; 교체 전 -20.2). 다른 교체 클립은 목표 음량과 1.2 LU 안(`skill.A03-3`만 3.3 LU 낮고 교체 전과 같다)이며 클리핑·양 끝·DC·true peak 규칙을 모두 통과했다.
- 기존 `generate_sfx.py --check`·`elevenlabs_audio.py --check` 실패(D4 스킬 ID 이전 때문에 카탈로그와 뱅크가 다름)는 이번 변경 전부터이며 그대로다. 새 62개의 카탈로그·뱅크 행은 생성기 형식과 같다.

## 보류

- 교체로 manifest에서 빠진 옛 원본 MP3 14개(`AudioSources/ElevenLabs/Sources/`)는 사용자 확인 뒤 같은 날 지웠다(git 이력에서 복구할 수 있다). 이번 교체와 무관하게 남아 있던 참조 없는 원본 MP3 27개는 건드리지 않았다.
- `W06-2`(광폭의 함성)는 예시 전투에서 시전 조건이 맞지 않아 연출 시연 영상을 만들지 못했다. 소리와 연출은 모두 연결되어 있다.

## 스킬별 소리와 프롬프트

"출처"가 기존인 줄의 프롬프트는 그 소리를 만들 때 쓴 원래 프롬프트이고, 스킬 이름은 현재 D4 이름이다.

### 전사

| cue | 스킬 · 변형 | 시점 | 출처 | 길이(초) | 프롬프트 (영어) |
|---|---|---|---|---:|---|
| `skill.W09` | 찢어낸 상처 | 시전 | 기존 | 0.68 | Two close raking blade slices, sharp cutting air and a compact wet tearing impact, isolated with a clear attack and controlled natural decay. |
| `skill.W09-1` | 찢어낸 상처 · 피의 회수 | 피의 회수 (`CASHOUT:W09*`) | 기존(옛 스킬 음원 승계) | 1.52 | A dense organic magical pull drawing inward, wet textured suction resolving into a warm low healing pulse, isolated with a clear attack and controlled natural decay. |
| `skill.W09-2` | 찢어낸 상처 · 깊은 상처 | 시전 | 신규 2026-10-18 | 0.88 | One deep heavy blade gouge, a slow weighty steel drag with thick wet tearing resistance and a low dull thud, isolated with a clear attack and controlled natural decay. |
| `skill.W09-3` | 찢어낸 상처 · 회전 베기 | 시전 | 신규 2026-10-18 | 0.79 | A fast full circle of raking blade slices, several rapid layered cutting air passes with wet tearing impacts, isolated with a clear attack and controlled natural decay. |
| `skill.W08` | 꿰뚫는 갈고리 | 시전 | 기존 | 0.81 | An iron hook thrown on a chain, rapid metal links whipping through air then catching with a hard clank and short yank, isolated with a clear attack and controlled natural decay. |
| `skill.W08-1` | 꿰뚫는 갈고리 · 돌진 갈고리 | 준비 시작 (`start`) | 신규 2026-10-18 | 1.20 | An iron hook shot out on a chain with the thrower dragged after it, rapid chain rattle with rushing wind and a hard armored landing thud, isolated with a clear attack and controlled natural decay. |
| `skill.W08-2` | 꿰뚫는 갈고리 · 관통 갈고리 | 시전 | 신규 2026-10-18 | 1.19 | A long iron hook driving straight through several bodies in a row, chain whipping, repeated dense piercing thuds and a final hard yank, isolated with a clear attack and controlled natural decay. |
| `skill.W08-3` | 꿰뚫는 갈고리 · 낙인 갈고리 | 시전 | 기존(옛 스킬 음원 승계) | 0.88 | A burning iron brand stamping once, a sharp metallic impact and brief aggressive sizzling crackle, isolated with a clear attack and controlled natural decay. |
| `skill.W01` | 회오리 | 시작 | 기존 | 0.67 | A heavy sword starting a fast circular sweep, wide forceful air whoosh and restrained steel ring, isolated with a clear attack and controlled natural decay. |
| `skill.W01.tick` | 회오리 | 회전 틱 | 기존 | 0.60 | One continuous fast circular sword pass, tight swishing air with a sharp steel edge and short decay, isolated with a clear attack and controlled natural decay. |
| `skill.W01-1` | 회오리 · 빠른 이동 | 시작 | 신규 2026-10-18 | 1.00 | A heavy sword whirling in fast circles while boots run, quick repeated air swishes over rapid armored footsteps, isolated with a clear attack and controlled natural decay. |
| `skill.W01-2` | 회오리 · 넓은 범위 | 시작 | 신규 2026-10-18 | 1.00 | One huge wide sweeping sword circle, a broad deep air whoosh with a long steel ring and a faint low rumble, isolated with a clear attack and controlled natural decay. |
| `skill.W01-3` | 회오리 · 빠른 회전 | 시작 | 신규 2026-10-18 | 0.80 | A very fast tight blade spin, dense rapid fluttering air cuts layered into a high steel whir, isolated with a clear attack and controlled natural decay. |
| `skill.W03` | 분쇄 일격 | 시전 | 기존 | 0.88 | A powerful crushing mace swing landing in one sharp armored impact, dense thud and short brittle crunch, isolated with a clear attack and controlled natural decay. |
| `skill.W03-1` | 분쇄 일격 · 대지 분쇄 | 시전 | 신규 2026-10-18 | 1.28 | A heavy mace smashing the ground in a circular burst, deep crushing impact with radiating cracks and scattering stone debris, isolated with a clear attack and controlled natural decay. |
| `skill.W03-2` | 분쇄 일격 · 처형 일격 | 시전 | 신규 2026-10-18 | 0.88 | One brutal finishing mace blow, a sharp heavy overhead crack with a dull crunch and a short dry ring, isolated with a clear attack and controlled natural decay. |
| `skill.W03-3` | 분쇄 일격 · 여진 | 여진 (`DAMAGE:EXTRA:W03`) | 신규 2026-10-18 | 1.53 | A heavy ground impact followed by two lingering delayed rumbling thuds, deep stone cracks rolling away, isolated with a clear attack and controlled natural decay. |
| `skill.W05` | 철벽 | 시전 | 기존 | 0.63 | Heavy iron armor plates locking together with a deep shield clang and a restrained sustained metal resonance, isolated with a clear attack and controlled natural decay. |
| `skill.W05-1` | 철벽 · 무쇠 자세 | 시전 | 기존(옛 스킬 음원 승계) | 1.17 | A heavily armored stance planting firmly, low stone thud and iron plates seating with a strong short ring, isolated with a clear attack and controlled natural decay. |
| `skill.W05-2` | 철벽 · 수호의 맹약 | 시전 | 기존(옛 스킬 음원 승계) | 2.68 | A protective iron seal forming, warm low bronze bell with a supportive metal impact and clear restrained overtones, isolated with a clear attack and controlled natural decay. |
| `skill.W05-3` | 철벽 · 가시 갑옷 | 시전 | 신규 2026-10-18 | 0.97 | Heavy iron armor sprouting spikes, grinding metal plates with sharp barbed scraping and a bristling metallic ring, isolated with a clear attack and controlled natural decay. |
| `skill.W06` | 전투 함성 | 시전 | 기존 | 2.00 | One short powerful ancient war horn blast with a dense low chest-like pressure pulse and rough bronze resonance, isolated with a clear attack and controlled natural decay. |
| `skill.W06-1` | 전투 함성 · 결집의 함성 | 시전 | 신규 2026-10-18 | 1.60 | One short rousing ancient war horn blast with a bright bronze overtone and a lifting pressure pulse, isolated with a clear attack and controlled natural decay. |
| `skill.W06-2` | 전투 함성 · 광폭의 함성 | 시전 | 신규 2026-10-18 | 1.60 | A ragged wild war horn blast distorted by rough rattling drums and a fast aggressive rising pulse, isolated with a clear attack and controlled natural decay. |
| `skill.W06-3` | 전투 함성 · 투쟁의 빚 | 시전 | 교체 2026-10-18 | 1.60 | A dark pact of borrowed strength, a warm rising inward energy surge with one faint coin chink and a heavy iron seal thunk, no voice, isolated with a clear attack and controlled natural decay. |
| `skill.W02` | 도약 내려찍기 | 도약 출발 | 기존 | 0.68 | An armored fighter leaping upward, a forceful boot push with short rising air rush and small chain rattle, isolated with a clear attack and controlled natural decay. |
| `skill.W02.land` | 도약 내려찍기 | 착지 | 기존 | 1.58 | One heavy armored leap landing on stone, dense ground slam with cracking rock and short low debris, isolated with a clear attack and controlled natural decay. |
| `skill.W02-1` | 도약 내려찍기 · 전선 돌파 | 도약 출발 | 교체 2026-10-18 | 1.36 | A heavily armored charge shoving through a crowd, fast heavy boot strides into a hard shield shoulder barge, bodies thrown aside with dense armor clanks and a short grounded stance thud, isolated with a clear attack and controlled natural decay. |
| `skill.W02-2` | 도약 내려찍기 · 지진 착지 | 착지 (`land`) | 신규 2026-10-18 | 2.00 | A heavy armored landing followed by a delayed deep ground quake, one big stone crack then a low rolling tremor, isolated with a clear attack and controlled natural decay. |
| `skill.W02-3` | 도약 내려찍기 · 연속 도약 | 도약 출발 | 신규 2026-10-18 | 1.00 | Two fast armored leaps in a row, two sharp boot pushes with rising air rushes and small chain rattles, isolated with a clear attack and controlled natural decay. |
| `skill.W04` | 지면 강타 | 시전 | 기존 | 1.55 | One powerful ground slam releasing a spreading rock crack, deep compact shockwave and falling stone chips, isolated with a clear attack and controlled natural decay. |
| `skill.W04-1` | 지면 강타 · 전장의 고리 | 시전 | 기존(옛 스킬 음원 승계) | 1.94 | A heavy chain spinning in a circle then pulling tight, close metallic links and a compact deep final impact, isolated with a clear attack and controlled natural decay. |
| `skill.W04-2` | 지면 강타 · 전율의 파동 | 시전 | 기존(옛 스킬 음원 승계) | 1.25 | A ground tremor moving outward, one strong rock crack followed by several smaller rolling stone impacts, isolated with a clear attack and controlled natural decay. |
| `skill.W04-3` | 지면 강타 · 연속 강타 | 시전 | 신규 2026-10-18 | 1.48 | Three heavy ground stomps in rhythm, the last one deeper with a sharp stone crack, isolated with a clear attack and controlled natural decay. |
| `skill.W17` | 선조의 전쟁 | 시전 | 기존 | 3.28 | A deep ancient war horn calling with several distant breathy spirit roars and a heavy controlled resonant pulse, isolated with a clear attack and controlled natural decay. |
| `skill.W17.hit` | 선조의 전쟁 | 후속 피해 | 기존 | 1.28 | One ghostly heavy sword strike, solid physical steel contact with a thin airy spectral tail, isolated with a clear attack and controlled natural decay. |
| `skill.W17-1` | 선조의 전쟁 · 선조의 군단 | 시전 | 신규 2026-10-18 | 2.39 | Three ancient spirit warriors summoned together, a deep war horn with layered distant breathy roars and a heavy resonant pulse, isolated with a clear attack and controlled natural decay. |
| `skill.W17-2` | 선조의 전쟁 · 선조의 기술 | 후속 피해 (`hit`) | 신규 2026-10-18 | 0.96 | Ghostly echoes copying a sword technique, thin airy spectral steel whooshes repeating over a dull resonant pulse, isolated with a clear attack and controlled natural decay. |
| `skill.W18` | 거인의 심판 | 시전 | 기존 | 2.27 | One colossal iron hammer impact on stone, forceful metallic crack, deep compact shockwave and heavy stone debris settling, short controlled rumble. |
| `skill.W18.start` | 거인의 심판 | 시작 | 기존 | 1.76 | An enormous weapon being raised, straining iron creaks and low air pressure rising steadily without an impact, isolated with a clear attack and controlled natural decay. |
| `skill.W18-1` | 거인의 심판 · 대지의 심판 | 준비 시작 (`start`) | 신규 2026-10-18 | 2.69 | A very slow enormous weapon rising then slamming a vast area, deep air pressure building into a colossal ground impact with a long rolling rumble, isolated with a clear attack and controlled natural decay. |
| `skill.W18-2` | 거인의 심판 · 거인화 | 시전 | 신규 2026-10-18 | 2.48 | A fighter swelling into a giant, creaking armor stretching with deep bone-like cracks and a heavy low resonant growth pulse, isolated with a clear attack and controlled natural decay. |

### 궁수

| cue | 스킬 · 변형 | 시점 | 출처 | 길이(초) | 프롬프트 (영어) |
|---|---|---|---|---:|---|
| `skill.A07` | 잇단 사격 | 시전 | 기존 | 0.55 | Three distinct fast bow shots in a tight rhythm, dry string snaps and brief narrow arrow whistles, isolated with a clear attack and controlled natural decay. |
| `skill.A07-1` | 잇단 사격 · 속사 | 시전 | 신규 2026-10-18 | 0.61 | Five quick bow shots in a very fast rhythm, dry string snaps and tight arrow whistles, isolated with a clear attack and controlled natural decay. |
| `skill.A07-2` | 잇단 사격 · 흩뿌림 | 시전 | 신규 2026-10-18 | 1.00 | A bow shot sending arrows in different directions, one string snap followed by several diverging whistles, isolated with a clear attack and controlled natural decay. |
| `skill.A07-3` | 잇단 사격 · 마무리 화살 | 시전 | 신규 2026-10-18 | 1.00 | One final powerful piercing arrow shot, a heavy bowstring crack with a bright sharp whistle and a dense hit, isolated with a clear attack and controlled natural decay. |
| `skill.A14` | 갈라지는 화살 | 시전 | 기존 | 0.42 | One arrow splitting in flight, a sharp bowstring snap followed by two diverging high air whistles, isolated with a clear attack and controlled natural decay. |
| `skill.A14-1` | 갈라지는 화살 · 연쇄 분열 | 시전 | 신규 2026-10-18 | 1.00 | An arrow splitting twice in flight, a bowstring snap then branching air whistles splitting again into four, isolated with a clear attack and controlled natural decay. |
| `skill.A14-2` | 갈라지는 화살 · 폭발 화살 | 시전 | 신규 2026-10-18 | 1.20 | An arrow impact exploding, a sharp bowstring snap then a dense small blast with scattered wooden splinters, isolated with a clear attack and controlled natural decay. |
| `skill.A14-3` | 갈라지는 화살 · 귀환 화살 | 갈라진 화살의 피해 (`DAMAGE:EXTRA:A14`) | 신규 2026-10-18 | 0.91 | Split arrows whistling away then curving back like boomerangs, a returning whoosh and two wooden thunks, isolated with a clear attack and controlled natural decay. |
| `skill.A01` | 관통 사격 | 시전 | 기존 | 1.08 | One powerful longbow shot, taut string crack with a fast penetrating arrow whistle, isolated with a clear attack and controlled natural decay. |
| `skill.A01-1` | 관통 사격 · 도탄 | 시전 | 신규 2026-10-18 | 1.20 | A powerful arrow ricocheting off stone, a string crack then a sharp metallic ping bouncing away, isolated with a clear attack and controlled natural decay. |
| `skill.A01-2` | 관통 사격 · 가속 관통 | 시전 | 신규 2026-10-18 | 0.61 | An arrow piercing several targets while its whistle rises in pitch, repeated dense pierces getting faster, isolated with a clear attack and controlled natural decay. |
| `skill.A01-3` | 관통 사격 · 세 갈래 | 시전 | 신규 2026-10-18 | 1.08 | Three longbow arrows loosed in a tight spread, layered strong string cracks and three parallel penetrating whistles, isolated with a clear attack and controlled natural decay. |
| `skill.A02` | 다중 사격 | 시전 | 기존 | 0.70 | Three arrows released almost simultaneously, closely layered bowstring snaps and a compact fan of air whistles, isolated with a clear attack and controlled natural decay. |
| `skill.A02-1` | 다중 사격 · 탄막 | 시전 | 신규 2026-10-18 | 1.20 | Seven arrows released in a wide fan, a dense layered bowstring snap and a broad sweep of air whistles, isolated with a clear attack and controlled natural decay. |
| `skill.A02-2` | 다중 사격 · 튕기는 화살 | 시전 | 신규 2026-10-18 | 1.20 | Arrows hitting then bouncing to another target, quick string snaps, wooden thunks and glancing pings, isolated with a clear attack and controlled natural decay. |
| `skill.A02-3` | 다중 사격 · 밀집 사격 | 시전 | 신규 2026-10-18 | 1.00 | A tight fan of arrows all landing on one target, one tight bowstring snap and a dense cluster of impact thunks, isolated with a clear attack and controlled natural decay. |
| `skill.A04` | 후퇴 도약 | 도약 출발 | 기존 | 1.00 | One quick backward leap with light leather rustle, rushing air and a soft boot landing, isolated with a clear attack and controlled natural decay. |
| `skill.A04-1` | 후퇴 도약 · 미끼 투영 | 도약 출발 | 기존(옛 스킬 음원 승계) | 1.76 | A magical decoy forming, a short inward airy pull and a soft crystalline pop with a restrained spectral tail, isolated with a clear attack and controlled natural decay. |
| `skill.A04-1.hit` | 후퇴 도약 · 미끼 투영 | 후속 피해 | 기존(옛 스킬 음원 승계) | 1.48 | A shadow decoy bursting, compact hollow pressure thump and a short rushing spectral air release, isolated with a clear attack and controlled natural decay. |
| `skill.A04-2` | 후퇴 도약 · 연막 엄폐 | 도약 출발 | 기존(옛 스킬 음원 승계) | 1.36 | A small smoke vial breaking, one compact pop followed by a thick soft pressurized smoke hiss, isolated with a clear attack and controlled natural decay. |
| `skill.A04-3` | 후퇴 도약 · 그림자 걸음 | 도약 출발 | 신규 2026-10-18 | 1.00 | A quick shadowy teleport behind a target, an air pop with a dark short swish and a sharp dagger-like ping, isolated with a clear attack and controlled natural decay. |
| `skill.A03` | 맹독 덫 | 시전 | 기존 | 0.81 | A small poison trap being set, metal spring catch clicking with a tiny liquid bubble and restrained hiss, isolated with a clear attack and controlled natural decay. |
| `skill.A03.trigger` | 맹독 덫 | 함정 발동 | 기존 | 0.71 | A poison trap snapping shut, sharp metal spring crack and a short pressurized spray of bubbling liquid, isolated with a clear attack and controlled natural decay. |
| `skill.A03-1` | 맹독 덫 · 가시 덫 | 시전 | 기존(옛 스킬 음원 승계) | 0.64 | A thorn trap being armed, dry twig creaks and brittle leaf rustle ending in a small mechanical catch, isolated with a clear attack and controlled natural decay. |
| `skill.A03-1.hit` | 맹독 덫 · 가시 덫 | 후속 피해 | 기존(옛 스킬 음원 승계) | 0.45 | A thorn trap bursting outward, sharp dry wood snaps and a compact spray of brittle thorn fragments, isolated with a clear attack and controlled natural decay. |
| `skill.A03-2` | 맹독 덫 · 서리 올가미 | 시전 | 기존(옛 스킬 음원 승계) | 0.85 | A frost snare being set, a small iron latch click followed by delicate freezing crystal cracks, isolated with a clear attack and controlled natural decay. |
| `skill.A03-2.hit` | 맹독 덫 · 서리 올가미 | 후속 피해 | 기존(옛 스킬 음원 승계) | 0.80 | An icy snare snapping closed, sharp spring crack and a short brittle ring of shattering frost, isolated with a clear attack and controlled natural decay. |
| `skill.A03-3` | 맹독 덫 · 교차 쇠뇌 | 시전 | 교체 2026-10-18 | 1.36 | A compact wooden ballista being planted into dirt, heavy wooden legs thudding down, a short winch ratchet and a firm iron lock, isolated with a clear attack and controlled natural decay. |
| `skill.A03-3.hit` | 맹독 덫 · 교차 쇠뇌 | 후속 피해 | 교체 2026-10-18 | 0.60 | One heavy ballista bolt firing, thick wooden arm snap with a sharp whip crack and a short dense bolt whistle, isolated with a clear attack and controlled natural decay. |
| `skill.A06` | 원소 주입 | 시전 | 교체 2026-10-18 | 1.71 | Arrows being infused with elemental power, a taut bowstring creak with fire crackle, ice shimmer and electric sparks swirling into the arrowheads, isolated with a clear attack and controlled natural decay. |
| `skill.A06-1` | 원소 주입 · 그림자 주입 | 처치 폭발 (`DAMAGE:EXTRA:A06*`) | 신규 2026-10-18 | 1.20 | Dark energy bursting from a fallen enemy, a deep soft implosion with a dark airy burst and a whispery tail, isolated with a clear attack and controlled natural decay. |
| `skill.A06-2` | 원소 주입 · 맹독 주입 | 시전 | 기존(옛 스킬 음원 승계) | 0.37 | One venom-tipped bow shot, taut dry string snap with a brief wet hiss trailing the arrow, isolated with a clear attack and controlled natural decay. |
| `skill.A06-3` | 원소 주입 · 서리 주입 | 시전 | 신규 2026-10-18 | 1.20 | An arrow tip coated in frost, delicate ice crackles with a cold airy shimmer and a brittle freeze click, isolated with a clear attack and controlled natural decay. |
| `skill.A05` | 사냥꾼의 표식 | 시전 | 기존 | 2.48 | A focused hunting mark forming, a subtle inward airy pull snapping into one clear high bronze ping, isolated with a clear attack and controlled natural decay. |
| `skill.A05-1` | 사냥꾼의 표식 · 응시 사격 | 조준 뒤 첫 피해 (`DAMAGE:A05`) | 기존(옛 스킬 음원 승계) | 0.86 | One heavily charged longbow shot, forceful woody string crack and sharp extended arrow whistle, isolated with a clear attack and controlled natural decay. |
| `skill.A05-1.start` | 사냥꾼의 표식 · 응시 사격 | 시작 | 기존(옛 스킬 음원 승계) | 1.20 | A powerful wooden bow drawing slowly to full tension, creaking limbs and taut string fibers, isolated with a clear attack and controlled natural decay. |
| `skill.A05-2` | 사냥꾼의 표식 · 사냥 준비 | 사냥 준비 효과 (`EFFECT:A05:preparation`) | 기존(옛 스킬 음원 승계) | 1.13 | A ranger readying a quiver, leather strap rustle, wooden arrow shafts tapping and one restrained bowstring creak, isolated with a clear attack and controlled natural decay. |
| `skill.A05-3` | 사냥꾼의 표식 · 연쇄 표식 | 표식 이동 (`STATUS:A05*+`) | 신규 2026-10-18 | 1.20 | A hunter's mark jumping from one target to another, a bronze ping then a quick airy whoosh and a second ping, isolated with a clear attack and controlled natural decay. |
| `skill.A17` | 일제 소탕 | 시전 | 기존 | 2.00 | A powerful dense volley of arrows launching together, layered taut strings and a rising sweep of narrow air whistles, isolated with a clear attack and controlled natural decay. |
| `skill.A17.hit` | 일제 소탕 | 후속 피해 | 기존 | 0.58 | A close cluster of arrows striking dense earth, distinct sharp woody thunks in quick irregular succession, isolated with a clear attack and controlled natural decay. |
| `skill.A17-1` | 일제 소탕 · 이동 포화 | 시전 | 신규 2026-10-18 | 1.31 | A rain of arrows that follows the archer, hissing falling arrow showers moving across with rolling whistles and light woody thuds, isolated with a clear attack and controlled natural decay. |
| `skill.A17-2` | 일제 소탕 · 집중 포화 | 시전 | 신규 2026-10-18 | 1.40 | A narrow dense hail of fast heavy arrows, sharp rapid whistles and hard punching woody impacts in a tight cluster, isolated with a clear attack and controlled natural decay. |
| `skill.A18` | 잔상 행군 | 시전 | 교체 2026-10-18 | 2.40 | A swift spectral rush leaving echoes behind, rushing air with soft glassy shimmer ticks as afterimages peel away one by one, isolated with a clear attack and controlled natural decay. |
| `skill.A18.hit` | 잔상 행군 | 후속 피해 | 교체 2026-10-18 | 0.80 | One shadow arrow loosed from an afterimage, a dry dark whoosh with a thin string twang and a soft shadowy thud, isolated with a clear attack and controlled natural decay. |
| `skill.A18-1` | 잔상 행군 · 그림자 분신 | 시전 | 신규 2026-10-18 | 0.67 | A shadow clone tearing away from an archer with a distinct dark pop, a breathy low whoosh and a faint echoing bowstring twang, isolated with a clear attack and controlled natural decay. |
| `skill.A18-2` | 잔상 행군 · 잔상 폭발 | 후속 피해 (`hit`) | 신규 2026-10-18 | 1.48 | Fading afterimages bursting in darkness, several soft dark implosive pops cascading with whispery tails, isolated with a clear attack and controlled natural decay. |

### 마법사

| cue | 스킬 · 변형 | 시점 | 출처 | 길이(초) | 프롬프트 (영어) |
|---|---|---|---|---:|---|
| `skill.M07` | 원소창 | 시전 | 교체 2026-10-18 | 1.36 | A lance of raw arcane energy launching, a narrow rushing jet with a crisp snap, faint fire flicker, frost shimmer and a spark tail, isolated with a clear attack and controlled natural decay. |
| `skill.M07-1` | 원소창 · 잿불창 | 시전 | 신규 2026-10-18 | 1.28 | One concentrated lance of fire launching, a hot narrow rushing jet with a crisp ignition and a short dry ember tail, isolated with a clear attack and controlled natural decay. |
| `skill.M07-2` | 원소창 · 빙하창 | 시전 | 기존(옛 스킬 음원 승계) | 1.36 | One glacial spear forming and launching, tense crystalline crack with a narrow fast icy air whistle, isolated with a clear attack and controlled natural decay. |
| `skill.M07-3` | 원소창 · 접지창 | 시전 | 교체 2026-10-18 | 1.23 | One lightning spear thrown and planted into the ground, a tight violent electrical snap ending in a metallic stake thunk with a lingering low electrical hum, isolated with a clear attack and controlled natural decay. |
| `skill.M01` | 화염구 | 시전 | 기존 | 0.83 | A fireball igniting and launching, crisp combustion crack followed by a focused rushing flame whoosh, isolated with a clear attack and controlled natural decay. |
| `skill.M01.hit` | 화염구 | 후속 피해 | 기존 | 2.00 | One large fireball explosion, sharp ignition attack with a weighty pressure thump and textured rushing flame dying naturally, no music. |
| `skill.M01-1` | 화염구 · 유성 | 시전 | 신규 2026-10-18 | 2.48 | A meteor falling from the sky then exploding, a long descending roar into a huge fiery impact with heavy rolling debris, isolated with a clear attack and controlled natural decay. |
| `skill.M01-2` | 화염구 · 분열 화염구 | 폭발 피해 (`DAMAGE:M01`) | 신규 2026-10-18 | 1.21 | A fireball exploding and throwing three small fireballs outward, a crack then several quick small whooshing ignitions, isolated with a clear attack and controlled natural decay. |
| `skill.M01-3` | 화염구 · 관통 화염구 | 시전 | 신규 2026-10-18 | 1.22 | A fireball tearing through enemies without exploding, a long searing flame jet with a crackling burning trail, isolated with a clear attack and controlled natural decay. |
| `skill.M02` | 눈보라 | 시전 | 기존 | 2.50 | A short fierce magical blizzard gust with textured cold wind and scattered brittle ice cracks, isolated with a clear attack and controlled natural decay. |
| `skill.M02-1` | 눈보라 · 따라오는 눈보라 | 시전 | 신규 2026-10-18 | 1.95 | A blizzard gust following along, textured cold wind swirling with ice crackles moving past, isolated with a clear attack and controlled natural decay. |
| `skill.M02-2` | 눈보라 · 혹한 | 시전 | 신규 2026-10-18 | 2.56 | A violent deep freezing storm, heavy howling cold wind with dense ice formation cracks and a low frozen rumble, isolated with a clear attack and controlled natural decay. |
| `skill.M02-3` | 눈보라 · 귀환 서리 | 시전 | 교체 2026-10-18 | 1.36 | A frost orb hurled outward, a crisp icy launch whoosh with delicate ice crackles and a soft glass hum, isolated with a clear attack and controlled natural decay. |
| `skill.M02-3.hit` | 눈보라 · 귀환 서리 | 후속 피해 | 교체 2026-10-18 | 1.06 | One frost orb striking and shattering, a dense glassy crack followed by a rattling cascade of brittle ice shards and a low frosty thump, isolated with a clear attack and controlled natural decay. |
| `skill.M03` | 연쇄 번개 | 시전 | 기존 | 1.20 | A forceful branching lightning discharge, multiple closely spaced electrical cracks over a compact low pressure pulse, isolated with a clear attack and controlled natural decay. |
| `skill.M03.hop` | 연쇄 번개 | 연쇄 도약 | 기존 | 0.28 | One tiny sharp lightning arc jumping between targets, immediate electrical snap with a very short spark tail, isolated with a clear attack and controlled natural decay. |
| `skill.M03-1` | 연쇄 번개 · 과부하 연쇄 | 시전 | 신규 2026-10-18 | 1.60 | An overcharged chain of many lightning arcs, intense layered electrical cracks hopping rapidly over a heavy buzzing surge, isolated with a clear attack and controlled natural decay. |
| `skill.M03-2` | 연쇄 번개 · 되튀김 | 시전 | 신규 2026-10-18 | 0.76 | One lightning bolt bouncing back and forth between two points, two sharp electrical snaps with a quick reverse zap, isolated with a clear attack and controlled natural decay. |
| `skill.M03-3` | 연쇄 번개 · 자력 연쇄 | 시전 | 기존(옛 스킬 음원 승계) | 2.28 | A magnetic vortex spinning up, dense circular metallic air rush with several small iron fragments rattling into orbit, isolated with a clear attack and controlled natural decay. |
| `skill.M06` | 서리 폭발 | 시전 | 기존 | 1.02 | A circular frost explosion, strong brittle ice crack radiating into a short cold wind and falling crystal shards, isolated with a clear attack and controlled natural decay. |
| `skill.M06-1` | 서리 폭발 · 이중 파동 | 두 번째 파동 (`DAMAGE:EXTRA:M06`) | 신규 2026-10-18 | 2.20 | Two frost explosions one second apart, a strong brittle ice crack then a short pause and a second smaller crack with cold wind, isolated with a clear attack and controlled natural decay. |
| `skill.M06-2` | 서리 폭발 · 균열 결계 | 시전 | 기존(옛 스킬 음원 승계) | 2.56 | A magical rift ward settling into place, deep stone-like resonance with a restrained shimmering pressure ring, isolated with a clear attack and controlled natural decay. |
| `skill.M06-3` | 서리 폭발 · 얼음 갑옷 | 시전 | 신규 2026-10-18 | 1.07 | Ice forming into a protective armor shell, quick crystalline crackling growth ending in a solid glassy lock, isolated with a clear attack and controlled natural decay. |
| `skill.M04` | 순간이동 | 시전 | 기존 | 0.71 | One magical teleport, a short inward air collapse followed by a clean resonant glass pop, isolated with a clear attack and controlled natural decay. |
| `skill.M04-1` | 순간이동 · 화염 잔불 | 시전 | 신규 2026-10-18 | 0.85 | A teleport leaving a burst of flame behind, an air collapse pop then a fiery whoomph at the departure point, isolated with a clear attack and controlled natural decay. |
| `skill.M04-2` | 순간이동 · 번개 도약 | 시전 | 신규 2026-10-18 | 1.36 | A teleport arriving with a lightning strike, a sharp air pop then a violent thunder crack with an electric sizzle, isolated with a clear attack and controlled natural decay. |
| `skill.M04-3` | 순간이동 · 원소 보호막 | 시전 | 기존(옛 스킬 음원 승계) | 2.00 | An elemental shield forming, dense airy pulse with layered restrained flame crackles, ice ticks and electrical sparks, isolated with a clear attack and controlled natural decay. |
| `skill.M08` | 화염 장벽 | 시전 | 기존 | 2.40 | A wall of flame erupting, strong low combustion punch opening into a broad textured roaring fire, isolated with a clear attack and controlled natural decay. |
| `skill.M08-1` | 화염 장벽 · 화염 고리 | 시전 | 신규 2026-10-18 | 2.20 | A ring of fire igniting around a caster with a sudden whoomph, crackling flames circling outward and dying down into embers, isolated with a clear attack and controlled natural decay. |
| `skill.M08-2` | 화염 장벽 · 전진 불길 | 시전 | 신규 2026-10-18 | 2.19 | A wall of fire marching forward, a deep rolling roar with advancing crackling flames and heat shimmer, isolated with a clear attack and controlled natural decay. |
| `skill.M08-3` | 화염 장벽 · 십자 장벽 | 시전 | 신규 2026-10-18 | 2.20 | Two crossed walls of flame erupting one after another, two strong combustion punches into a broad roaring fire, isolated with a clear attack and controlled natural decay. |
| `skill.M11` | 축전 구체 | 시전 | 기존 | 2.00 | An electrical orb gathering charge, a restrained deep buzzing pulse with irregular close electrical crackles, isolated with a clear attack and controlled natural decay. |
| `skill.M11.hit` | 축전 구체 | 후속 피해 | 기존 | 1.36 | A charged electrical orb discharging once, explosive crisp lightning crack and short dense rolling spark texture, isolated with a clear attack and controlled natural decay. |
| `skill.M11-1` | 축전 구체 · 구르는 구체 | 시전 | 신규 2026-10-18 | 2.00 | A slowly rolling electrical orb, a deep humming buzz with irregular crackles drifting steadily, isolated with a clear attack and controlled natural decay. |
| `skill.M11-2` | 축전 구체 · 3분할 | 시전 | 신규 2026-10-18 | 1.76 | Three electrical orbs placed in quick succession, three short crackling charge-ups over low hums, the last hum fading out to silence, isolated with a clear attack and controlled natural decay. |
| `skill.M11-3` | 축전 구체 · 과충전 | 시전 | 신규 2026-10-18 | 2.20 | An electrical orb overcharging then bursting, a rising whine building to one huge crack of lightning, isolated with a clear attack and controlled natural decay. |
| `skill.M17` | 삼원소 붕괴 | 시전 | 기존 | 2.77 | Three elemental energies collapsing together, fire ignition, brittle ice shatter and lightning crack over one controlled deep impact, isolated with a clear attack and controlled natural decay. |
| `skill.M17-1` | 삼원소 붕괴 · 단일 원소 집중 | 시전 | 신규 2026-10-18 | 3.00 | One element collapsing three times in escalating deep impacts, a single focused arcane convergence with dense layered thuds, isolated with a clear attack and controlled natural decay. |
| `skill.M17-2` | 삼원소 붕괴 · 추적 붕괴 | 준비 시작 (`start`) | 신규 2026-10-18 | 2.40 | Three collapses each snapping to a new spot, a quick arcane swish then a deep impact repeated three times, isolated with a clear attack and controlled natural decay. |
| `skill.M18` | 현자의 화신 | 시전 | 기존 | 3.41 | A powerful arcane transformation, a rising organic wind pulse blooming into clear glass and bronze resonance with restrained elemental crackles, isolated with a clear attack and controlled natural decay. |
| `skill.M18-1` | 현자의 화신 · 원소 폭주 | 시전 | 신규 2026-10-18 | 1.61 | A second elemental spell cast at once, a wave of fire, ice and lightning crackles layered into a rising arcane surge, isolated with a clear attack and controlled natural decay. |
| `skill.M18-2` | 현자의 화신 · 원소 화신 | 시전 | 신규 2026-10-18 | 2.37 | A mage becoming an elemental avatar, a deep resonant bloom with swirling fire and frost shimmer and electrical crackle, isolated with a clear attack and controlled natural decay. |

