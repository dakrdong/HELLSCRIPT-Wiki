# 전설·세트 장비 이미지 연결과 누락 조사

갱신일: 2026-09-23

[English](Equipment_Art_Integration.en.md)

[세트 장비와 세트 문양 제작 작업](https://chatgpt.com/s/cx_6ab3635913b88191a19efd3dc7f17785)의 장비 105개와 문양 24개를 실제 공통 장비 표시에 연결했다. 원본 제작 커밋 `7417c9f`의 이미지가 이미 기준 `main`인 `4768bb5`에 포함되어 있어 그대로 재사용했다. 함께 저장된 전설 장비 이미지 141개도 같은 조회 경로에 연결했다. 이번 작업에서 새 이미지를 생성하거나 다른 세트의 이미지를 복제하지 않았다.

## 연결 방식

- [EquipmentArt](../../Assets/HELLSCRIPT/Runtime/Presentation/EquipmentArt.cs)는 장비의 원래 고유 정의 ID인 `Item.special`로 전설·세트 이미지 경로를 조회한다. 위상 각인으로 효과가 바뀌어도 원래 장비의 그림은 유지한다. 별도 그림이 없는 장비는 기존 베이스 이미지 또는 벡터 아이콘을 유지한다.
- [EquipmentSlotView](../../Assets/HELLSCRIPT/Runtime/Presentation/EquipmentSlotView.cs)의 공통 표시를 통해 인벤토리, 창고, 상점, 대장간, 장착 배치와 장비 상세가 동일한 이미지를 사용한다. 개별 PNG는 전체 UV 영역으로 표시하고 투명도와 입력 통과를 유지한다.
- [ItemDetailView](../../Assets/HELLSCRIPT/Runtime/Presentation/ItemDetailView.Style.cs)는 세트 효과 영역에 해당 세트 ID의 문양을 표시한다. 기존 효과 문구와 계산을 재사용하며 문양 옆의 본문 높이는 실제 줄바꿈에 맞춰 계산한다.
- 실제 아이템·저장·전투·드롭 데이터와 기존 이미지·메타데이터·GUID는 변경하지 않는다. 신규 직업 장비의 `playerEnabled=false` 상태도 유지한다. 이미지 연결은 신규 세트의 일반 플레이 해금을 의미하지 않는다.

생성 도구의 정확한 모델명은 기존 제작 기록에서 `unknown`이다. 이번 연결은 사용자 요청에 따라 기존 후보 이미지를 사용한 것으로, 모델 근거가 새로 확인되었다는 뜻은 아니다. 과거 제작 manifest의 당시 검증 범위는 보존하고 현재 연결 결과는 이 문서에서 구분한다.

## 파일 재조사

[전체 파일 조사 결과](EquipmentArtEvidence/file-audit.json)는 카탈로그 ID와 실제 PNG·알파를 비교한다. 파일 검사와 Unity 실행 검사는 서로 구분한다.

| 구분 | 정의 수 | 전용 이미지 있음 | 전용 이미지 없음 |
| --- | ---: | ---: | ---: |
| 전설 장비 | 141 | 141 | 0 |
| 신규 24세트 장비 | 105 | 105 | 0 |
| 기존 6세트 장비 | 24 | 0 | 24 |
| 세트 문양 | 30 | 24 | 6 |

전설 141개는 기존 일반 플레이 정의 123개와 추가 직업 정의 18개를 포함한다. 제작된 장비 246개 사이에 동일 파일 해시로 중복된 이미지는 없고, 전달된 장비·문양 270개 중 손상되거나 알파가 없는 PNG는 없다. 이는 파일·알파 검사 결과이며 독립적인 미술 재검수 결과는 아니다.

| 직업 | 전용 이미지가 없는 세트 | 장비 ID | 부위 |
| --- | --- | --- | --- |
| 전사 | 회오리 감시자 | SW1, SW2, SW3, SW4 | 머리, 몸통, 손, 발 |
| 전사 | 낙성의 집행자 | SWB1, SWB2, SWB3, SWB4 | 머리, 몸통, 손, 발 |
| 궁수 | 독무덤지기 | SA1, SA2, SA3, SA4 | 머리, 몸통, 손, 발 |
| 궁수 | 긴 그림자의 추적자 | SAB1, SAB2, SAB3, SAB4 | 머리, 몸통, 손, 발 |
| 마법사 | 겨울의 서약 | SM1, SM2, SM3, SM4 | 머리, 몸통, 손, 발 |
| 마법사 | 공명하는 폭풍 | SMB1, SMB2, SMB3, SMB4 | 머리, 몸통, 손, 발 |

이 6세트의 문양 ID `SW`, `SWB`, `SA`, `SAB`, `SM`, `SMB`도 전용 파일이 없다. 신규 세트 이미지의 ID를 기존 세트에 임의로 대입하지 않았다.

재조사 명령은 `python3 tools/audit_equipment_art.py --output Docs/Implementation/EquipmentArtEvidence/file-audit.json`이다. 알려진 미제작 항목은 보고서에 남기며, 이미 납품된 이미지의 누락·PNG/알파 오류·동일 파일 중복은 실패로 처리한다.

## 검증

- 공통 UI 소유권 검사와 관련 검사 9개가 통과했다.
- Unity 6000.6.0f1 Edit Mode 관련 검사 **63개 통과, 실패 0, 건너뜀 0**이다. 장비 이미지 10개 검사, 공통 UI 23개, 장비 비교 16개, 상세 설정 14개를 포함한다. 장비 이미지 검사는 전체 246개 파일의 Unity 로드, 24세트의 문양 연결, 전체 UV, 입력 통과, 원래 장비 식별자와 대체 표시를 확인한다. [검사 XML](EquipmentArtEvidence/editmode.xml)
- macOS Metal 개발 빌드를 오류 0으로 생성하고, 실제 앱에서 **20개 표시·조작 조합**을 통과했다. 세로 440×956, 가로 956×440, PC 1440×810·1440×900·1680×720에서 한국어·영어와 글자 크기 100%·150%를 확인했다. 합성 포인터가 실제 uGUI 레이캐스트와 클릭 처리기를 거쳐 장비 상세와 비교를 열었으며, 텍스처 ID·문양·본문 높이와 저장된 장비의 불변성을 검사했다. [실행 기록](EquipmentArtEvidence/equipment-art-runtime.txt) · [상세 결과](EquipmentArtEvidence/equipment-art-audit.json)
- 새 작업 폴더의 별도 저장 경로에서 검증했다. 실행 중인 원본 Editor의 Play Mode나 사용자 저장을 바꾸지 않았다. 전체 게임 회귀 검사와 모바일 실기기 검증은 수행하지 않았다.

![세로 장비 상세와 세트 문양](EquipmentArtEvidence/equipment-art-detail-440x956-ko-100.png)

![가로 영어 150퍼센트 장비 비교](EquipmentArtEvidence/equipment-art-comparison-956x440-en-150.png)
