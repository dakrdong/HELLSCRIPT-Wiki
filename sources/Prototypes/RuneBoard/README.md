# HELLSCRIPT 룬 보드 HTML 시안 (v14)

2026-10-01 · [English](#english)

[시안 열기](HELLSCRIPT-RuneBoard.html) · [설계·검증 기록](../../Docs/Design/Rune_Board_UX_Redesign.md) · [브라우저 검증 결과](evidence/browser-validation.json) · [이미지 출처](../../Docs/Art/RuneBoard/2026-10-01/art-manifest.json) · [원본 파일 해시](source-manifest.json)

승인된 룬 보드 v13 시안을 게임의 공통 UI(흑요석·금박, 제목 → 무기 탭 → 본문 → 하단 고정 행동)로 다시 구성하고, 블럭을 보석 슬랩으로 새로 디자인한 조작 가능한 시안이다. 규칙·데이터는 v13 그대로이며 실제 계정·전투·인벤토리에는 연결하지 않는다. 경험치와 룬 수량은 시연 데이터다.

## 열기

`HELLSCRIPT-RuneBoard.html` 하나로 실행한다(약 1.1 MiB, 외부 파일 없음). 위쪽 막대에서 화면 크기(현재 창·모바일 세로·가로·16:9·16:10·21:9)와 언어(KO/EN)를 바꾼다. 쿼리로도 지정한다.

| 쿼리 | 값 |
| --- | --- |
| `lang` | `ko`, `en` |
| `view` | `fit`, `portrait`, `landscape`, `pc`, `tall`, `wide` |
| `scenario` | `start`(기본), `empty`, `clarity`(켜짐/꺼짐이 섞인 예), `ready`(중앙 완료), `path`(맹공 확장 중), `full`(전부 개방) |
| `weapon`, `focus`, `mode=view` | 시작 무기, 영역, 보기 모드 |
| `modal` | `guide`, `help`, `effects`, `regions`, `presets`, `catalog`, `lab`, `tutorial`, `region-info:assault` |
| `embed=1` | 미리보기 막대 없이 창 전체 |

## 조작

보관함에서 블럭을 고른 뒤 빈 칸을 누르거나 끌어 놓는다. 고르면 놓을 수 있는 자리에 초록 점이, 모든 칸이 켜지는 자리에는 금색 테두리가 뜬다. 끄는 동안 블럭이 손가락 위로 떠오르고 보드 아래에 켜질 칸 수나 거부 사유가 나온다. 배치한 블럭은 눌러 선택해 회전·회수하고, 0.1초 눌렀다가 끌면 옮긴다. 자세한 규칙은 시안 안의 **이용 안내**(머리글 ?)와 **범례**(보드 오른쪽 아래)에 있다. R 회전 · Delete 회수 · Ctrl+Z 취소 · Ctrl+S 저장 · Esc 선택 해제.

## 만드는 법과 검사

```sh
python3 Prototypes/RuneBoard/tools/make-art.py        # Codex 원본 → 웹용 사본 (Art/web)
node Prototypes/RuneBoard/build.cjs                   # 단일 HTML 생성
node --test Prototypes/RuneBoard/engine.test.cjs      # 규칙·데이터가 v13과 같은지
node --test Prototypes/RuneBoard/locale.test.cjs      # 한/영 문구 누락
node Prototypes/RuneBoard/browser.test.cjs            # 실제 마우스·키·터치 입력 305개, 증거 화면 생성
```

| 파일 | 역할 |
| --- | --- |
| `vendor/engine.js`, `vendor/tutorial-core.js` | v13 원본 스크립트(바이트 동일) |
| `blocks.js` | 블럭·칸 그리기(상태별) |
| `ui/00~99-*.js` | 상태, 거래, 프레임, 보드, 패널, 대화상자, 입력 |
| `tutorial-ui.js` | 5단계 연습 |
| `locale.js` | 한국어·영어 문구와 엔진 거부 사유 번역 |
| `styles.css`, `page.html`, `build.cjs` | 스타일, 틀, 빌드(색은 `UiTheme.cs`, 데이터는 게임 카탈로그와 `en.txt`) |
| `Art/web`, `tools/make-art.py` | 후처리한 이미지와 만드는 도구. 원본은 `Docs/Art/RuneBoard` |

## 확인하지 못한 것

모바일 실기기, Safari·Firefox, 스크린 리더, 색각 이상 시뮬레이션, Unity 반영은 확인하지 않았다. 검사는 macOS 헤드리스 Chrome의 합성 마우스·키·터치 입력이다. 내장 브라우저는 로컬 HTML을 열지 않는다.

## English

[Open the mockup](HELLSCRIPT-RuneBoard.html) · [Design and verification record](../../Docs/Design/Rune_Board_UX_Redesign.en.md) · [Browser results](evidence/browser-validation.json) · [Art provenance](../../Docs/Art/RuneBoard/2026-10-01/art-manifest.json) · [Source hashes](source-manifest.json)

The approved rune board v13 mockup rebuilt on the game's shared UI (obsidian and gilt; title → weapon tabs → body → fixed actions), with a new gem-slab design for the blocks. Rules and data are exactly v13. It is not connected to real accounts, combat or inventory; XP and rune counts are demo values.

**Open it:** `HELLSCRIPT-RuneBoard.html` is one self-contained file (about 1.1 MiB). The toolbar switches the preview size and language; the query parameters above do the same (`lang`, `view`, `scenario`, `weapon`, `focus`, `mode=view`, `modal`, `embed=1`).

**Use it:** pick a block in storage, then tap an empty slot or drag it onto the board. Picking shows green dots where it fits and a gold ring where every cell would switch on. While dragging, the block floats above your finger and the text under the board says how many cells will switch on, or why it is refused. Select a placed block to rotate or recall it; hold it 0.1 s, then drag, to move it. See **How the rune board works** (header ?) and **Legend** (bottom right of the board) inside the mockup. R rotate · Delete recall · Ctrl+Z undo · Ctrl+S save · Esc deselect.

**Build and checks:** the commands above. `engine.test.cjs` proves the rules and data equal v13, `locale.test.cjs` proves nothing lacks English, and `browser.test.cjs` runs 305 real mouse, key, touch and pixel checks in headless Chrome, sweeping six screen shapes in both languages.

**Not verified:** physical phones, Safari and Firefox, screen readers, colour-blindness simulation and the Unity port. Checks use synthetic input in macOS headless Chrome; the in-app browser does not open local HTML.
