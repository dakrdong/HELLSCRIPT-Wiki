# 아이템 상세 카드 장식 아트

갱신일: 2026-10-05 · [English](Item_Card_Art.en.md) · 적용 기록: [아이템 상세 Diablo IV 재구성](../../Implementation/Item_Detail_D4.md)

공통 아이템 상세 카드(`ItemDetailView`)의 모서리·구분선·점수 명판·소켓 고리·상위 옵션 표식을 Codex(GPT) 이미지 생성으로 새로 만들었다. 사용자의 "더 쥬시하게, 필요하면 코덱스에게 이미지를 요청"이라는 지시에 따른 것이며, 코드로 그린 벡터 장식은 폴백으로만 남겼다.

## 에셋

각 PNG는 흰색 한 가지 색의 윤곽이며 게임이 등급 색으로 입힌다. 원본은 `out/`에 바이트 그대로 보관하고, 게임에는 같은 파일을 `Resources/Art/ItemCard/`에 넣었다.

| 에셋 | 게임 파일 | 원본 크기 | 불투명 영역(px, 좌상단 기준) | 쓰임 |
| --- | --- | --- | --- | --- |
| [card-corner](2026-10-04/out/card-corner.png) | `corner.png` | 1254×1254 | 49,52 – 1202,1171 | 카드 네 모서리. UV를 뒤집어 한 장으로 네 모서리를 만든다 |
| [card-divider](2026-10-04/out/card-divider.png) | `divider.png` | 2172×724 | 35,237 – 2137,480 | 머리 아래·옵션/특수 효과 앞 구분선, 상세 창 제목 장식 |
| [power-badge](2026-10-04/out/power-badge.png) | `badge.png` | 1774×887 | 11,73 – 1764,809 | 장비 점수 명판. 양쪽 날개 30%를 고정하고 가운데만 늘리는 3분할 스프라이트 |
| [socket-ring](2026-10-04/out/socket-ring.png) | `socket.png` | 1254×1254 | 46,30 – 1208,1222 | 소켓 고리. 보석이 있으면 보석 그림을 안에 넣는다 |
| [greater-star](2026-10-04/out/greater-star.png) | `star.png` | 1254×1254 | 135,89 – 1119,1137 | 상위 옵션 표식(불꽃이 오르는 사방 별) |

불투명 영역은 알파 16 초과 기준이며 `ItemCardArt`의 UV 잘라내기 상수와 같다. 원본 파일은 수정하지 않는다.

## 생성 기록

- 도구: `codex exec -m gpt-6-astra`의 내장 `imagegen` 모드. 에셋당 한 번씩 총 5회 호출. 모델은 도구가 알려 주지 않아 `unknown`으로 기록한다.
- 입력: [프롬프트](2026-10-04/item-card-art-prompt.txt), 스타일 참조 [현재 카드 화면](2026-10-04/ref-card.png). 호출별 정확한 프롬프트·원본 경로·SHA-256·알파 통계는 [manifest.json](2026-10-04/out/manifest.json), 보관 정보는 [artifact-lifecycle.json](2026-10-04/out/artifact-lifecycle.json), Codex의 마지막 메시지는 [last-message.txt](2026-10-04/last-message.txt)에 있다.
- 후처리: 없음(복사만). 크로마키·배경 제거·재색칠을 쓰지 않았고 모두 원본 알파 채널이다.
- 생성 도구가 보고한 규격 차이: 크기가 요청한 1024가 아니라 1254(원본 비율 유지), 가장자리 반투명 픽셀의 RGB가 순백이 아님. 불투명 픽셀(알파 200 초과)의 평균 RGB는 모두 248 이상(corner 248.6/248.0/248.2, divider 252.9/252.7/253.0, badge 252.3/252.2/252.3, socket 253.2/252.9/253.2, star 253.1/252.7/253.0)이어서 틴트에 지장이 없다고 판단해 재생성하지 않았다.
- 원본 생성 파일은 Codex 폴더(`~/.codex/generated_images/`)에도 남아 있으며 이 저장소의 사본과 바이트가 같다. 보존 검토일은 2026-11-04이고, 삭제는 승인되지 않았다.

## 가져온 방식

- 임포터: `ItemCardArtImporter`(알파 투명도, 밉맵, Trilinear, NPOT 크기 변경 없음, 최대 1024). 폭이 넓은 구분선·명판은 2의 거듭제곱이 아니므로 크기를 바꾸지 않는다.
- 플랫폼 예산: `ResourceTextureBudget`에 `Art/ItemCard/divider`(Android ASTC 4×4·최대 1024, 데스크톱 BC7)와 `Art/ItemCard/`(최대 512) 행을 추가하고 `GetVersion`을 6으로 올렸다. 얇은 선화라 4×4 블록을 쓴다.
- 폴백: 리소스가 없으면 `ItemCardArt`가 `null`을 돌려주고 카드는 기존 벡터 장식으로 그려진다.
- 틴트: 모든 장식은 장비 등급 색(`EquipmentGradePalette`)을 곱해 쓴다. 입력을 가로채지 않는다(`raycastTarget=false`).
- 그라디언트 두 개(등급 후광, 한 번 지나가는 빛줄기)는 아트가 아니라 코드로 만든 부드러운 알파 곡선이다.
