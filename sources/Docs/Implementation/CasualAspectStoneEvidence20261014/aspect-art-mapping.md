# 위상 각인석 대표 원화 연결 — 2026-10-14

[English](aspect-art-mapping.en.md) · [승인 ID/SHA 색인](approved-art-manifest.json) · [보존 기록](aspect-art-lifecycle.json)

`AspectRuneGraphic.Create(..., awake, casual: true)`를 선택한 화면에서만 `LW02`를 `Art/CasualAspectStone/LW02`로 읽는다. 기본값 `casual: false`와 나머지 122개 위상은 기존 `Art/ClassAspectIcons/{id}`를 사용한다. 공통 각인석 배경 `Art/ItemIcons/aspect-stone`, 오브젝트 이름·자식 순서·크기·상태색·raycast 정책은 유지한다. 이 원화 연결은 ID·게임 규칙·저장을 변경하지 않는다.

감독 승인 목록의 각인석 1개와 위상 문양 123개, 총 124개의 실제 경로와 SHA를 확인했다. 전체 원본은 134,929,220 bytes이며 이 작업에는 `aspect-LW02` 한 장만 복사했다. 나머지는 승인 원본 위치만 색인에 기록한다. 원본 아트와 기존 `.meta`는 보존한다.

대표 PNG는 1254×1254 RGBA, 1,168,223 bytes, SHA-256 `7eaedc552682d30ece36195a7d7b6cf139aeb250bc577acbd0bbd607a78ff27e`다. 복사 결과의 SHA도 같다. 알파 범위는 0–255, 완전 투명 픽셀은 973,426개다. 비영 알파 경계는 `[17, 19, 1236, 1254]`로 하단에 닿는 픽셀이 있어 원본을 임의로 자르거나 여백을 추가하지 않았다. 정사각형 아트 슬롯의 기존 배치를 유지한다.

새 PNG 메타는 기준 HEAD의 기존 LW02 TextureImporter 정책을 새 GUID로 복제했다. GPU 전용, mipmap 없음, sRGB, 알파 투명도, Clamp/Bilinear, 기본 최대 512, Standalone/Android/WebGL 최대 256 정책을 유지했다. 이는 원본 PNG의 1254×1254 픽셀을 바꾸지 않으며 실제 가져오기 품질은 아직 확인하지 않았다.

이 단계에서 완료한 증거는 승인 경로/124개 SHA 읽기, 대표 원화 시각 확인, PNG 크기·알파 읽기, 대표 복사 SHA 일치다. Unity·EditMode·실제 화면·저장·모바일 검증과 전체 원화 확대는 부모의 순차 검증을 기다린다. 새 원화 생성·원본 아트 수정·공통 파일 수정·공개 공유·커밋은 실행하지 않았다.

공통 후속 `0ccba80eef0da4703cb7ffcd1fb8e22848002b3d`의 장비 provider를 화면 skin에 연결했다. 장비 359종은 공통 소유 경로에서 승인 원본·대상 SHA, 330개 아이템 매핑, 30개 세트 문양, 362개 GUID와 기존 원화 336개의 보존을 정적으로 확인했다. 이 작업의 sparse checkout에는 장비 PNG 359개를 복제하지 않았다.

위상 대표 `LW02`와 장비 `LW02`는 같은 ID를 다른 표시 역할에 사용한다. 장비 원화는 `Art/Casual/Equipment/LW02`, 512×512, 231,021 bytes, SHA-256 `e922415daabd88aeb3a684b81ed40d1488aa4869cb36f07e534d2d7337605a0f`다. 위상 문양의 `Art/CasualAspectStone/LW02`와 원본·픽셀·SHA가 다르며, 장비 359개 전체 SHA와 비교해 위상 대표 PNG 중복은 0개다. 두 매핑을 유지한다. [최신 정적 검증](static-checks.json)과 [글꼴 감사](font-coverage-audit.json)는 새 공통 기준을 기록한다. 실제 Unity 가져오기·장비 표시·폰트 줄바꿈은 아직 검증하지 않았다.
