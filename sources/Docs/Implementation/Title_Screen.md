# HELLSCRIPT 살아 있는 타이틀 화면

작성일: 2026-09-20

[English](Title_Screen.en.md)

## 화면과 시작 흐름

높은 절벽 위 영웅들의 뒷모습 너머로 먼 균열이 빛나는 레이어 배경이 움직인다(아래 원경 레이어 참고). 로고는 위쪽 중앙에 두고 로그인·서버 선택과 캐릭터 선택 화면으로 가는 버튼은 중앙 하단에 모았다. 배경은 화면을 가득 채우고, 조작부는 기기의 안전 영역 안에 배치한다. 연출 정지 버튼은 배경의 모든 애니메이션을 멈춘다.

로그인과 서버는 사용자가 요청한 모의 기능이다. 계정 이름과 체험용 비밀번호가 비어 있으면 안내를 표시하고, 입력하면 로그인 상태를 보여 준다. 비밀번호는 마스킹하며 창을 닫으면 입력 필드를 비운다. 네트워크 인증, 비밀번호 저장, 실제 서버 접속은 하지 않는다. 게스트 입장과 로그아웃도 지원한다. 서버 두 개는 선택할 수 있고 점검 중인 서버는 비활성화한다. 어느 서버를 선택해도 기존 저장 계정과 캐릭터를 그대로 사용한다.

로그인 또는 게스트 입장에 성공하면 별도의 [캐릭터 선택 화면](Character_Selection.md)으로 이동한다. 전사·마법사·궁수의 동작을 확인하고 캐릭터를 고른 뒤 시작하기를 누르면 기존 `GameController.EnterPlaza(true)`를 호출한다. 마을 이동은 입장 후 시작한다. 타이틀의 캐릭터 목록 대화상자는 이 독립 화면으로 대체했다.

## 원경 레이어와 로그인 연출 (2026-10-14)

타이틀 배경을 한 장의 그림에서 여러 장의 레이어로 바꿨다. 젤다의 전설 키 아트처럼 영웅 여섯 명이 높은 절벽 위에서 뒷모습을 보이며 먼 계곡 가운데의 균열을 바라본다. 왼쪽 절벽에는 궁수·마법사·전사(남·여·남), 오른쪽 절벽에는 전사·마법사·궁수(여·남·여)가 선다. 두 개의 거대한 바위 사이 틈이 보라색으로 빛나고, 그 위로 폭풍 구름이 소용돌이친다.

| 레이어 | 파일 | 역할 |
|---|---|---|
| 먼 계곡 가로·세로 | `vista_far_landscape` 1672×941, `vista_far_portrait` 852×1846 | 하늘·계곡·균열. 균열 중심을 화면의 같은 점에 고정한다. |
| 균열 근접 | `vista_rift_close` 1672×941 | 비행 끝부분에서 먼 계곡을 대신한다. |
| 떠 있는 바위 | `vista_mid_floaters` 1672×941, 투명 | 비행 중 카메라 옆을 스쳐 지나간다. 대기 화면에서는 로고를 가리지 않도록 숨긴다. |
| 왼쪽·오른쪽 영웅 절벽 | `vista_heroes_left`·`vista_heroes_right` 1254×1254, 투명 | 영웅과 절벽을 한 장에 함께 그렸다. |

- 배치: 균열 중심은 가로 화면에서 위에서 47%, 세로 화면에서 56%에 둔다. 버튼 패널이 화면 폭의 절반보다 좁으면 절벽이 화면 아래 모서리 밖으로 이어진다. 세로 화면이나 작은 창처럼 버튼이 화면을 가로지르면 영웅이 버튼 바로 위에 서고 절벽 아래쪽 30%는 계곡 안개로 흐려진다.
- 대기 연출: 카메라가 천천히 흔들리며 레이어마다 다른 거리만큼 움직인다. 균열 빛이 맥동하고 작은 불꽃이 위로 오른다. `연출 멈춤`은 이 움직임을 모두 멈춘다.
- 로그인 연출(2.75초): 로그인·게스트 입장·`캐릭터 선택하기`로 타이틀을 떠날 때 재생한다. 0~1.05초 영웅 절벽이 양옆으로 빠지며 커진다. 0~2초 계곡이 균열을 향해 2.6배 확대되고 방사형으로 흐려지며 흔들린다. 0.1~1.5초 떠 있는 바위가 카메라를 스쳐 지나간다. 1.25~1.75초 균열 근접 그림으로 넘어가고, 2.15초에 균열 빛이 화면을 덮은 뒤 2.75초까지 다음 화면이 드러난다.
- 게임 흐름: 로그인 처리와 화면 전환 시점은 바꾸지 않았다. 다음 화면(캐릭터 선택 또는 첫 튜토리얼)은 바로 만들어지고, 연출은 그 위의 별도 캔버스(정렬 2500, 알림 3000 아래)에서 재생된다. 재생 중에는 실제 탭·키 입력이 보이지 않는 다음 화면에 닿지 않도록 UI 입력 모듈을 끈다. 0.25초 뒤부터 탭하거나 키를 누르면 바로 빛 단계로 넘어간다. 연출이 끝나거나 타이틀로 돌아오면 입력을 돌려준다. `연출 멈춤` 상태에서는 연출 없이 바로 넘어간다.
- 효과음: 출발할 때 바람 돌진(`flow.title_flight`)을 울리고, 연출 시간이 1.85초를 지나면 균열 섬광(`flow.title_rift`)을 울린다. 섬광음은 시작 약 0.3초 뒤에 가장 커서 화면이 가장 밝은 2.15초와 겹친다. 탭으로 건너뛸 때도 화면에 맞춰 울린다. 연출이 재생되면 `캐릭터 선택하기`의 기존 돌문 소리(`flow.title_start`)는 내지 않고, `연출 멈춤` 상태에서만 낸다. 두 소리는 ElevenLabs로 만들었다([효과음 출처](ElevenLabs_Game_Audio.md)).
- 성능: 대기 중 움직임은 배경 전용 캔버스 안에서만 일어나 버튼 캔버스를 다시 묶지 않는다. 투명도가 0인 근접·빛 레이어는 그리지 않고, 방사형 흐림(8회 샘플)은 비행 중에만 켠다. 프레임 시간은 측정하지 않았다.
- 소유: [TitleVista.cs](../../Assets/HELLSCRIPT/Runtime/Presentation/TitleVista.cs), [셰이더](../../Assets/HELLSCRIPT/Resources/Art/Title/TitleVista.shader), [가져오기 설정](../../Assets/HELLSCRIPT/Editor/TitleVistaArtImporter.cs). 캔버스 생성은 UI 계약에 따라 `GameUI.Title.cs`가 맡는다. Android 용량 예산에 `Art/Title/Vista/` 2048 ASTC 6×6 행을 추가했다.
- 아트 출처: 그림은 모두 Codex(`codex exec`, 내장 image_gen)로 생성해 바이트 그대로 복사했다. 전체 구도 마스터 시안을 먼저 만들고, 레이어마다 마스터와 영웅 설정화를 참조로 다시 그렸다. 도구가 모델 버전을 알려 주지 않아 `unknown`으로 기록했다. 투명 레이어는 배경 제거 없이 생성 시점의 알파를 쓴다. 프롬프트는 [마스터](../Art/TitleVista/2026-10-14/title_vista_prompt_master.txt)·[먼 배경](../Art/TitleVista/2026-10-14/title_vista_prompt_far.txt)·[영웅 절벽](../Art/TitleVista/2026-10-14/title_vista_prompt_heroes.txt)·[균열 근접과 바위](../Art/TitleVista/2026-10-14/title_vista_prompt_close.txt)이고, 시도별 생성 기록은 [manifest](../Art/TitleVista/2026-10-14/manifest_master.json)와 같은 폴더의 레이어별 manifest에 있다.

![로그인 연출 마스터 시안](../Art/TitleVista/2026-10-14/master_vista_v01.png)

검증(2026-10-14, macOS 개발 빌드): `RuntimeTitleSmoke`가 `HELLSCRIPT_TITLE_RUNTIME_OK checks=24`로 통과했다. PC 16:9·울트라와이드·iPhone 세로에서 로그인 연출이 두 영웅 절벽을 화면 밖으로 보내고, 바람·섬광 효과음을 한 번씩 재생하고, 캐릭터 선택 화면 위에서 재생되는 동안 실제 입력 모듈을 끄고, 끝난 뒤 입력을 돌려주는지 확인했다. 기존 비율·안전 영역·겹침·잘림·한국어·영어·연출 멈춤 검사도 그대로 통과했다. `RuntimeCharacterSelectionSmoke`는 `checks=12`로 통과했고, 연출 중에도 스크립트 UI 클릭이 동작했다. 관련 Edit Mode 검사(`TitleScreenTests` 17개, `CharacterSelectionTests` 10개, `BuildResourceTests` 14개, `SoundBankTests`·`NewContentAudioTests` 24개)는 65/65개를 통과했으며 새 검사 6개를 포함한다. `test_elevenlabs_audio.py`는 7/7개를 통과했다. `python3 tools/check_ui_contract.py --verify`는 15/15개를 통과했다.

미검증: 효과음을 사람이 직접 들어 본 확인, 전체 Edit Mode와 스모크 묶음(최종 `main` 통합 뒤 한 번 실행), iPhone·Android 실기기와 WebGL, 프레임 시간·GPU 비용, 실제 탭·키로 건너뛰기(스모크는 실제 입력을 만들 수 없어 시간 지정으로 장면을 확인했다), 실제 Google 인증 뒤의 연출.

![PC 16:9 타이틀](TitleVistaEvidence/pc-16x9-ko.png)

[돌진 1.0초](TitleVistaEvidence/pc-16x9-flight-1.0.png) · [균열 근접 1.5초](TitleVistaEvidence/pc-16x9-flight-1.5.png) · [iPhone 세로](TitleVistaEvidence/iphone-portrait-ko.png)

## 화면 비율

[Apple 공식 제품 사양](https://support.apple.com/en-us/125091)에서 iPhone 17 Pro Max의 해상도 1320×2868을 확인했다. 타이틀은 이 비율의 세로·가로 화면과 PC 16:9, 16:10, 3440:1440 울트라와이드에 맞춰 배치한다. 기기 모델명을 검사하거나 특정 해상도를 강제로 설정하지 않는다.

| 대상 | 검증 창 크기 | 배치 |
|---|---|---|
| iPhone 17 Pro Max 세로 | 440×956, 원본 해상도의 1/3 | 중앙 하단에 세로로 배치한다. |
| iPhone 17 Pro Max 가로 | 956×440, 원본 해상도의 1/3 | 로그인·서버와 캐릭터 선택 화면 진입을 짧은 열로 배치한다. |
| PC 16:9 | 1280×720 | 중앙 조작부의 최대 폭을 제한한다. |
| PC 16:10 | 1440×900 | 늘어난 높이에 따라 배경과 여백을 확장한다. |
| PC 울트라와이드 | 1720×720, 3440×1440의 1/2 | 배경은 가득 채우고 조작부는 중앙에 유지한다. |

실제 모바일에서는 기존 `Screen.safeArea` 경로를 사용한다. 검증의 상단·하단·좌우 여백은 Dynamic Island와 홈 인디케이터에 대비한 모의 값이며, iPhone 실측치로 주장하지 않는다. 로그인 창은 회전 시 입력 필드를 다시 만들지 않는다. 작은 창에서는 대화상자의 내용만 스크롤하며 닫기 버튼은 고정한다.

## 소유권과 리소스

- [GameUI.Title.cs](../../Assets/HELLSCRIPT/Runtime/Presentation/GameUI.Title.cs)는 기존 UI의 타이틀 진입점이다.
- [TitleScreenView.cs](../../Assets/HELLSCRIPT/Runtime/Presentation/TitleScreenView.cs)와 [대화상자](../../Assets/HELLSCRIPT/Runtime/Presentation/TitleScreenView.Dialogs.cs)가 화면과 입력을 담당한다.
- [TitleSession.cs](../../Assets/HELLSCRIPT/Runtime/Core/TitleSession.cs)는 로그인·서버 선택을 메모리에만 유지한다. 게임 저장 파일과 분리되어 있다.
- 타이틀 배경은 위의 원경 레이어([TitleVista.cs](../../Assets/HELLSCRIPT/Runtime/Presentation/TitleVista.cs))가 그린다. [TitleAtmosphere.cs](../../Assets/HELLSCRIPT/Runtime/Presentation/TitleAtmosphere.cs)와 [셰이더](../../Assets/HELLSCRIPT/Resources/Art/Title/TitleAtmosphere.shader)는 캐릭터 선택과 대기실 배경에 계속 쓴다.
- 이전 타이틀 배경 [TitleSanctuary.png](../../Assets/HELLSCRIPT/Resources/Art/Title/TitleSanctuary.png)(1672×941 원본)는 균열 입장·결과 창에서 계속 쓴다.

배경은 내장 `image_gen`으로 생성했다. 도구 반환값은 `image_url`, `output_hint`이며, 파일의 C2PA 생성 정보는 `softwareAgent.name=ChatGPT`, `version=gpt-image`이다. 정확한 모델 버전은 확인할 수 없으므로 `gpt-image-2` 사용을 주장하지 않는다. 개발용 배경으로 등록하며 출시 아트 승인을 마친 자산으로 분류하지 않는다. [정확한 생성 프롬프트](Title_Screen_Prompt.txt)와 [출처 기록](TitleScreenEvidence/provenance.json)을 보존한다.

로고와 프레임은 uGUI 텍스트·선으로 만든다. 운영체제 글꼴 파일을 복사하거나 재배포하지 않는다. 새 문구는 기존 한국어 원문 키와 영어 문구 표를 함께 사용한다.

## 검증

캐릭터 선택 화면을 연결한 뒤 관련 검사를 58/58개 통과했고, 타이틀 런타임 검증도 최종 빌드로 다시 통과했다. [확장 검증 기록](Character_Selection.md)을 함께 참고한다.

초기 타이틀 관련 Edit Mode 검사는 48/48개를 통과했다. 이 중 타이틀 세션·입장 차단·비율 유지·리소스 검사가 15개이며, 기존 번역 검사가 33개다. [검사 결과](TitleScreenEvidence/editmode.json)를 보존한다.

macOS 개발 빌드의 `RuntimeTitleSmoke`가 `HELLSCRIPT_TITLE_RUNTIME_OK checks=10`으로 종료 코드 0을 반환했다. PC 세 가지 비율, iPhone 비율의 두 방향, 한국어·영어 문구, 안전 영역, 버튼 겹침과 문구 잘림을 확인했다. 로그인 빈 값, 비밀번호 가림·제거, 회전 중 입력 유지, 서버 변경·점검 상태, 실제 캐릭터 선택, 로그아웃·게스트, 마을 입장도 통과했다. [런타임 결과](TitleScreenEvidence/validation.txt)를 보존한다.

최종 타이틀 실행에서 배경은 1.4초 간격의 실제 렌더링으로 평균 RGB 차이 1.7140을 보였다. 연출 정지 후에는 시계와 같은 영역의 픽셀이 모두 고정되었다. 프레임 속도나 모바일 성능을 측정한 결과는 아니다.

별도의 Mac 실행 창에서는 실제 포인터 클릭으로 로그인 창을 열고 문자 입력·Tab 이동·Enter 로그인·서버 선택·로그아웃을 확인했다. [입력 검증 범위](TitleScreenEvidence/native-input.json)에 기록했다. iPhone 실기기에서의 터치·키보드·방향 전환·성능과 Windows 실행은 미검증이다. 초기 타이틀 빌드는 오류 0개로 성공했고 프로젝트의 셰이더·가져오기 경고가 39개였다. 캐릭터 선택을 포함한 최종 빌드도 오류 0개로 성공했으며 전체 프로젝트 경고 384개가 남아 있다. [최종 빌드 기록](CharacterSelectionEvidence/build.json)을 함께 보존한다.

![PC 16:9 타이틀](TitleScreenEvidence/pc-16x9-ko.png)

[iPhone 세로](TitleScreenEvidence/iphone-portrait-ko.png) · [iPhone 가로](TitleScreenEvidence/iphone-landscape-ko.png) · [PC 16:10](TitleScreenEvidence/pc-16x10-ko.png) · [PC 울트라와이드](TitleScreenEvidence/pc-ultrawide-ko.png) · [로그인 창](TitleScreenEvidence/iphone-login-ko.png) · [영어 서버 선택](TitleScreenEvidence/iphone-servers-en.png)

공개 위키 반영은 이 작업 브랜치가 `main`에 병합될 때, 병합 담당자가 생성·검사·배포를 완료한다. 작업 브랜치에서는 공개본을 덮어쓰지 않는다.
