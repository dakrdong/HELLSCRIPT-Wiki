# 전역 HUD 이미지 리소스 안내

작성일: 2026-09-14

[전체 기획](HELLSCRIPT_GlobalHUD_Overview.md) · [영문 안내](HELLSCRIPT_GlobalHUD_Resources.en.md) · [ZIP 다운로드](Resources/HELLSCRIPT-GlobalHUD-v1.zip)

## 사용 순서

리소스는 `Docs/Design/GlobalHUD/Resources/v1`에 준비했다. Unity가 자동으로 임포트하지 않도록 아직 Assets 폴더에 넣지 않았다. 구현 승인을 받은 뒤 필요한 PNG를 매니페스트의 대상 경로로 옮겨 Editor에서 임포트한다. SVG는 편집 원본이며 게임에서는 기본 제공 PNG를 사용하면 별도 벡터 패키지가 필요하지 않다.

1. [매니페스트](Resources/v1/resource-manifest.json)와 [배치 프로필](Resources/v1/layout-profile.json)을 확인한다.
2. Sprite (2D and UI), Input Texture Alpha, Alpha Is Transparency, Bilinear, Clamp, Mip Maps Off, Read/Write Off, Full Rect, PPU 100을 시작값으로 사용한다. 초기 시각 검토에서는 압축을 끄고 이후 기기에서 압축을 비교한다.
3. 원화는 Max Size 2048, 벡터 PNG는 실제 크기 이상인 512를 제안한다. 보관 원본은 그대로 두고 모바일용 축소·압축은 Unity 임포트 설정에서 처리한다.
4. SkillAtlas는 1536×1024, 6열×4행, 셀 256×256이다. 매니페스트는 Unity의 왼쪽 아래 원점 좌표를 제공한다. 기존 프로젝트 파일과 바이트가 같으므로 런타임에 사본을 또 로드하지 않는다.
5. 프레임의 border는 왼쪽·아래·오른쪽·위 순서다. `frame-vital`은 14, `fill-vital`은 10을 사용한다. 바를 Sliced로 그릴 때 부모 폭으로 진행 길이를 조절한다. Filled 모드와 Sliced 모드를 한 Image에 동시에 설정하지 않는다.
6. 새로운 공통 그림은 프레임·내용·쿨타임을 나눠 조합한다. 기존 액티브 아틀라스의 테두리는 이미 포함되어 있으므로 새 프레임과 중복하지 않는다. 마법사 초상은 원형 마스크 안에서 얼굴 중심과 확대율을 맞춘다.

## 구성과 사용 범위

PNG 68개는 원화 4개, 공통 요소 21개, 상태 기호 24개, 패시브 18개, 기존 아틀라스 1개로 구성했다. 기존 아틀라스 한 파일에서 액티브 18종과 기존 초상 3종을 사용할 수 있다. 각 상태 기호는 여러 실제 효과가 공유할 수 있는 의미별 그림이다. 패시브 18개는 기존 정의 ID와 1:1로 대응한다.

상태 기호와 패시브는 작은 크기에서의 식별을 우선한 단순 도형이다. 그림별 기호를 구분하고 실제 효과 이름·설명을 함께 연결한다. 색과 기호가 비슷한 두 효과는 설명과 출처로 구분한다. 상태 종류가 더 필요하면 공통 unknown을 사용하고 누락 매핑을 기록한다. 현재 플레이어에게 없는 독·빙결 등을 예시 그림이 있다는 이유로 만들어 표시하지 않는다.

HP·MP·XP 색, 시간·수량·레벨·중첩·설명은 동적으로 표시한다. 텍스트 글꼴은 기존 프로젝트 글꼴을 사용한다. 전체 HUD 배경, 검은 경계 덮개, 숫자가 들어간 스킬 그림은 제공하지 않는다. `mask-edge-smooth`는 흰색의 알파 룩업이며 실제 감쇠 셰이더가 필요하다. 기본 Mask/RectMask2D만으로 그라데이션 구현이 끝난다고 가정하지 않는다.

## 출처와 검사

병 3종과 마법사 초상은 내장 image_gen으로 생성했고 원본 PNG 바이트를 보존했다. 메타데이터의 `softwareAgent.name=gpt-image`, `version=2.0`을 확인했다. 도구는 모델 지정 인자를 노출하지 않으며 서명 검증을 수행한 것은 아니다. 기존 아틀라스의 과거 생성 모델은 이번 기록에서 추정하지 않는다. 기존 사용 기록은 [Asset Provenance](../../Implementation/Asset_Provenance.md)를 따른다. 이번 원화는 프로젝트용 신규 생성물이며 독점권이나 별도 법적 검토가 완료되었다고 주장하지 않는다.

[프롬프트](Resources/v1/generation-prompts.json), [원본 파일 식별자](Resources/v1/native-sources.json), [벡터 재생성 도구](Resources/v1/build-vectors.cjs), [검사 도구](Resources/v1/validate-resources.py), [검사 결과](Resources/v1/resource-qa.json), [벡터 목록](Resources/v1/vector-manifest.json)을 함께 보관한다. 픽셀 검사에서는 실제 알파·투명 픽셀·크기·좌표·해시를 확인했다. Unity 및 모바일 시각 검증은 구현 단계에 남아 있다.

아래 목록은 각각의 원본 파일을 제공한다. [리소스 미리보기](Resources/v1/gallery.html)는 로컬에서 파일을 열거나 폴더를 정적 서버로 열어 확인할 수 있다. 공개 위키에서는 HTML이 텍스트로 제공될 수 있으므로 PNG 링크 또는 ZIP을 사용한다.

## 파일 목록

| ID | 분류 | 크기 | PNG | 편집 원본 |
|---|---|---|---|---|
| frame-seal | common | 256×256 | [PNG](Resources/v1/png/frame-seal.png) | [SVG](Resources/v1/svg/frame-seal.svg) |
| mask-circle | mask | 256×256 | [PNG](Resources/v1/png/mask-circle.png) | [SVG](Resources/v1/svg/mask-circle.svg) |
| mask-square | mask | 256×256 | [PNG](Resources/v1/png/mask-square.png) | [SVG](Resources/v1/svg/mask-square.svg) |
| mask-diamond | mask | 256×256 | [PNG](Resources/v1/png/mask-diamond.png) | [SVG](Resources/v1/svg/mask-diamond.svg) |
| frame-active | common | 256×256 | [PNG](Resources/v1/png/frame-active.png) | [SVG](Resources/v1/svg/frame-active.svg) |
| frame-passive | common | 256×256 | [PNG](Resources/v1/png/frame-passive.png) | [SVG](Resources/v1/svg/frame-passive.svg) |
| frame-status | common | 256×256 | [PNG](Resources/v1/png/frame-status.png) | [SVG](Resources/v1/svg/frame-status.svg) |
| plate-icon | common | 256×256 | [PNG](Resources/v1/png/plate-icon.png) | [SVG](Resources/v1/svg/plate-icon.svg) |
| frame-vital | common | 512×48 | [PNG](Resources/v1/png/frame-vital.png) | [SVG](Resources/v1/svg/frame-vital.svg) |
| fill-white | common | 16×16 | [PNG](Resources/v1/png/fill-white.png) | [SVG](Resources/v1/svg/fill-white.svg) |
| mask-edge-smooth | mask | 256×8 | [PNG](Resources/v1/png/mask-edge-smooth.png) | [SVG](Resources/v1/svg/mask-edge-smooth.svg) |
| fill-vital | common | 512×32 | [PNG](Resources/v1/png/fill-vital.png) | [SVG](Resources/v1/svg/fill-vital.svg) |
| badge-level | common | 192×64 | [PNG](Resources/v1/png/badge-level.png) | [SVG](Resources/v1/svg/badge-level.svg) |
| control-plus | control | 64×64 | [PNG](Resources/v1/png/control-plus.png) | [SVG](Resources/v1/svg/control-plus.svg) |
| control-minus | control | 64×64 | [PNG](Resources/v1/png/control-minus.png) | [SVG](Resources/v1/svg/control-minus.svg) |
| badge-buff | common | 32×32 | [PNG](Resources/v1/png/badge-buff.png) | [SVG](Resources/v1/svg/badge-buff.svg) |
| badge-debuff | common | 32×32 | [PNG](Resources/v1/png/badge-debuff.png) | [SVG](Resources/v1/svg/badge-debuff.svg) |
| xp-tick | common | 8×16 | [PNG](Resources/v1/png/xp-tick.png) | [SVG](Resources/v1/svg/xp-tick.svg) |
| xp-tick-major | common | 8×24 | [PNG](Resources/v1/png/xp-tick-major.png) | [SVG](Resources/v1/svg/xp-tick-major.svg) |
| xp-cap | common | 16×20 | [PNG](Resources/v1/png/xp-cap.png) | [SVG](Resources/v1/svg/xp-cap.svg) |
| portrait-placeholder | common | 256×256 | [PNG](Resources/v1/png/portrait-placeholder.png) | [SVG](Resources/v1/svg/portrait-placeholder.svg) |
| status-shield | status | 256×256 | [PNG](Resources/v1/png/status-shield.png) | [SVG](Resources/v1/svg/status-shield.svg) |
| status-power | status | 256×256 | [PNG](Resources/v1/png/status-power.png) | [SVG](Resources/v1/svg/status-power.svg) |
| status-poison | status | 256×256 | [PNG](Resources/v1/png/status-poison.png) | [SVG](Resources/v1/svg/status-poison.svg) |
| status-lightning | status | 256×256 | [PNG](Resources/v1/png/status-lightning.png) | [SVG](Resources/v1/svg/status-lightning.svg) |
| status-leaf | status | 256×256 | [PNG](Resources/v1/png/status-leaf.png) | [SVG](Resources/v1/svg/status-leaf.svg) |
| status-sword | status | 256×256 | [PNG](Resources/v1/png/status-sword.png) | [SVG](Resources/v1/svg/status-sword.svg) |
| status-frost | status | 256×256 | [PNG](Resources/v1/png/status-frost.png) | [SVG](Resources/v1/svg/status-frost.svg) |
| status-hourglass | status | 256×256 | [PNG](Resources/v1/png/status-hourglass.png) | [SVG](Resources/v1/svg/status-hourglass.svg) |
| status-flame | status | 256×256 | [PNG](Resources/v1/png/status-flame.png) | [SVG](Resources/v1/svg/status-flame.svg) |
| status-eye | status | 256×256 | [PNG](Resources/v1/png/status-eye.png) | [SVG](Resources/v1/svg/status-eye.svg) |
| status-wind | status | 256×256 | [PNG](Resources/v1/png/status-wind.png) | [SVG](Resources/v1/svg/status-wind.svg) |
| status-heart | status | 256×256 | [PNG](Resources/v1/png/status-heart.png) | [SVG](Resources/v1/svg/status-heart.svg) |
| status-rune | status | 256×256 | [PNG](Resources/v1/png/status-rune.png) | [SVG](Resources/v1/svg/status-rune.svg) |
| status-chain | status | 256×256 | [PNG](Resources/v1/png/status-chain.png) | [SVG](Resources/v1/svg/status-chain.svg) |
| status-broken | status | 256×256 | [PNG](Resources/v1/png/status-broken.png) | [SVG](Resources/v1/svg/status-broken.svg) |
| status-unknown | status | 256×256 | [PNG](Resources/v1/png/status-unknown.png) | [SVG](Resources/v1/svg/status-unknown.svg) |
| status-whirl | status | 256×256 | [PNG](Resources/v1/png/status-whirl.png) | [SVG](Resources/v1/svg/status-whirl.svg) |
| status-arrow | status | 256×256 | [PNG](Resources/v1/png/status-arrow.png) | [SVG](Resources/v1/svg/status-arrow.svg) |
| status-trap | status | 256×256 | [PNG](Resources/v1/png/status-trap.png) | [SVG](Resources/v1/svg/status-trap.svg) |
| status-mana | status | 256×256 | [PNG](Resources/v1/png/status-mana.png) | [SVG](Resources/v1/svg/status-mana.svg) |
| status-target | status | 256×256 | [PNG](Resources/v1/png/status-target.png) | [SVG](Resources/v1/svg/status-target.svg) |
| status-elements | status | 256×256 | [PNG](Resources/v1/png/status-elements.png) | [SVG](Resources/v1/svg/status-elements.svg) |
| status-focus | status | 256×256 | [PNG](Resources/v1/png/status-focus.png) | [SVG](Resources/v1/svg/status-focus.svg) |
| status-people | status | 256×256 | [PNG](Resources/v1/png/status-people.png) | [SVG](Resources/v1/svg/status-people.svg) |
| passive-WP01 | passive | 256×256 | [PNG](Resources/v1/png/passive-WP01.png) | [SVG](Resources/v1/svg/passive-WP01.svg) |
| passive-WP02 | passive | 256×256 | [PNG](Resources/v1/png/passive-WP02.png) | [SVG](Resources/v1/svg/passive-WP02.svg) |
| passive-WP03 | passive | 256×256 | [PNG](Resources/v1/png/passive-WP03.png) | [SVG](Resources/v1/svg/passive-WP03.svg) |
| passive-WP04 | passive | 256×256 | [PNG](Resources/v1/png/passive-WP04.png) | [SVG](Resources/v1/svg/passive-WP04.svg) |
| passive-WP05 | passive | 256×256 | [PNG](Resources/v1/png/passive-WP05.png) | [SVG](Resources/v1/svg/passive-WP05.svg) |
| passive-WP06 | passive | 256×256 | [PNG](Resources/v1/png/passive-WP06.png) | [SVG](Resources/v1/svg/passive-WP06.svg) |
| passive-AP01 | passive | 256×256 | [PNG](Resources/v1/png/passive-AP01.png) | [SVG](Resources/v1/svg/passive-AP01.svg) |
| passive-AP02 | passive | 256×256 | [PNG](Resources/v1/png/passive-AP02.png) | [SVG](Resources/v1/svg/passive-AP02.svg) |
| passive-AP03 | passive | 256×256 | [PNG](Resources/v1/png/passive-AP03.png) | [SVG](Resources/v1/svg/passive-AP03.svg) |
| passive-AP04 | passive | 256×256 | [PNG](Resources/v1/png/passive-AP04.png) | [SVG](Resources/v1/svg/passive-AP04.svg) |
| passive-AP05 | passive | 256×256 | [PNG](Resources/v1/png/passive-AP05.png) | [SVG](Resources/v1/svg/passive-AP05.svg) |
| passive-AP06 | passive | 256×256 | [PNG](Resources/v1/png/passive-AP06.png) | [SVG](Resources/v1/svg/passive-AP06.svg) |
| passive-MP01 | passive | 256×256 | [PNG](Resources/v1/png/passive-MP01.png) | [SVG](Resources/v1/svg/passive-MP01.svg) |
| passive-MP02 | passive | 256×256 | [PNG](Resources/v1/png/passive-MP02.png) | [SVG](Resources/v1/svg/passive-MP02.svg) |
| passive-MP03 | passive | 256×256 | [PNG](Resources/v1/png/passive-MP03.png) | [SVG](Resources/v1/svg/passive-MP03.svg) |
| passive-MP04 | passive | 256×256 | [PNG](Resources/v1/png/passive-MP04.png) | [SVG](Resources/v1/svg/passive-MP04.svg) |
| passive-MP05 | passive | 256×256 | [PNG](Resources/v1/png/passive-MP05.png) | [SVG](Resources/v1/svg/passive-MP05.svg) |
| passive-MP06 | passive | 256×256 | [PNG](Resources/v1/png/passive-MP06.png) | [SVG](Resources/v1/svg/passive-MP06.svg) |
| potion-hp | potion | 1254×1254 | [PNG](Resources/v1/png/potion-hp.png) | — |
| potion-mp | potion | 1254×1254 | [PNG](Resources/v1/png/potion-mp.png) | — |
| potion-utility | potion | 1254×1254 | [PNG](Resources/v1/png/potion-utility.png) | — |
| portrait-mage | portrait | 1254×1254 | [PNG](Resources/v1/png/portrait-mage.png) | — |
| skill-atlas-existing | reused | 1536×1024 | [PNG](Resources/v1/png/skill-atlas-existing.png) | — |
