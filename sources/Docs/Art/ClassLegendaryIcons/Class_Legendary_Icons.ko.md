# 전설 장비 이미지 제작 기록

작성일: 2026-09-22 (한국 시간)

전설 장비 141종 중 실제 PNG 원본 141종을 제작했습니다. 기존 확장 123종과 이번 스킬 연동 신규 18종을 [전체 목록](manifest.json)과 항목별 manifest에 기록했습니다. 생성 수에 자리표시자·복제 이미지·프롬프트만 준비한 항목을 포함하지 않습니다.

## 인계 규격과 제작 근거

- 내장 `image_gen`으로 항목마다 개별 요청을 사용했습니다. 모든 생성 원본은 그대로 보존합니다.
- 인계 담당 작업의 규격 조정에 따라 1024px 또는 1254px 정사각형 네이티브 RGBA PNG를 허용합니다. 실제 반환 크기는 각 manifest에 기록합니다. 중앙 76%는 여백·가독성 지침이며 잘림·형태 누락·불투명 배경은 실패로 처리합니다.
- 도구 응답은 `image_url`과 `output_hint`를 제공하며 정확한 모델명을 확인할 수 없습니다. PNG 내장 출처 정보의 `ChatGPT / gpt-image` 표기도 `gpt-image-2`를 입증하지 않습니다. 제작 지침의 기본 모델은 `gpt-image-2`지만 실제 모델은 `unknown`으로 기록하고, 검수를 통과해도 제작 후보로 유지합니다.
- 기존 EquipmentAtlas의 마모된 금속·가죽 질감과 방향성 명암을 참고했습니다. GlobalHUD는 작은 크기에서의 가독성만 참고했으며 UI 바탕이나 테두리를 이미지에 넣지 않았습니다.
- 원본 알파를 재생성하거나 배경을 제거하지 않았습니다. 크로마키·불투명 배경 대체·로컬 크기 변경도 수행하지 않았습니다.

## 파일 구성

- `manifest.json`은 141개 ID의 색인입니다. `manifests/<ID>.json`에 한영 이름·효과, 부위, 연동 스킬, 개별 프롬프트, 파일 경로, 생성 근거, QA와 남은 항목을 기록합니다.
- `../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/<ID>.png`는 게임 프로젝트에 저장한 네이티브 원본입니다. 새 `.meta`에는 고유 GUID가 있으며 기존 GUID는 변경하지 않았습니다.
- `originals/<ID>.png`는 반환 원본의 동일한 보존 사본입니다. 게임 Art 파일과 보존 사본, 도구가 반환한 원본의 SHA-256이 일치해야 합니다.
- [개별 콘셉트](concepts.tsv), [장비 원문](source/legendaries.json), [연동 스킬 원문](source/skills.json), [인계 규격 조정](handoff-revision.json), [원문 보존 근거](source-provenance.json)를 함께 보존합니다. 개별 manifest에 프롬프트 전문이 있습니다. 원본 인계 작업 공간에는 쓰지 않았습니다.
- `qa/`에는 64px 밝은·어두운 바탕 비교와 원본 해상도 가장자리 표본이 있습니다. 이 합성 이미지는 검수 자료이며 게임용 원본을 바꾸지 않습니다.

## 검증 범위

실제 알파를 확인한 원본은 141종이며, 허용 원본 크기를 확인한 항목은 141종입니다. 시각 검수를 기록한 항목은 141종입니다. [검증 결과](validation.json)는 ID·한영 원문·경로·해시·중복 이미지·GUID 검사와 전체 제작 여부를 분리해 기록합니다.

위키의 기존 링크를 보존하기 위해 인계 담당 작업이 지정한 두 작업 공간에서 과거 검증 자료를 읽어 이 작업 공간의 제외 경로에 복원했습니다. [복원 근거](historical-wiki-evidence.json)에 출처와 SHA-256을 기록했으며, 과거 자료를 이번 작업에서 실행한 게임 검사로 집계하지 않았습니다.

이 작업은 이미지와 제작 기록만 다룹니다. 전투 코드, UI 배치, 아이콘 연결 코드, 씬, 프리팹은 수정하지 않았습니다. Unity의 실제 임포트, Edit Mode 검사, macOS 런타임과 실기기 검증은 수행하지 않았으며 UI 통합 작업에서 확인해야 합니다. main 병합과 공개 위키 배포는 이번 작업 범위에 포함하지 않습니다.

## 항목별 진행 목록

| ID | 한국어 이름 | 부위 | 원본 상태 | 시각 검수 |
| --- | --- | --- | --- | --- |
| [LW01](manifests/LW01.json) | [소용돌이의 송곳니](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW01.png) | 무기 | 생성 후보 | 통과 |
| [LW02](manifests/LW02.json) | [낙성의 발걸음](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW02.png) | 발 | 생성 후보 | 통과 |
| [LW03](manifests/LW03.json) | [고독한 처형](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW03.png) | 손 | 생성 후보 | 통과 |
| [LW04](manifests/LW04.json) | [마지막 명령](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW04.png) | 목걸이 | 생성 후보 | 통과 |
| [LA01](manifests/LA01.json) | [끝없는 궤적](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA01.png) | 무기 | 생성 후보 | 통과 |
| [LA02](manifests/LA02.json) | [좁혀진 사선](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA02.png) | 손 | 생성 후보 | 통과 |
| [LA03](manifests/LA03.json) | [독사의 탈피](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA03.png) | 발 | 생성 후보 | 통과 |
| [LA04](manifests/LA04.json) | [검은 전염](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA04.png) | 목걸이 | 생성 후보 | 통과 |
| [LM01](manifests/LM01.json) | [겨울의 발자취](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM01.png) | 무기 | 생성 후보 | 통과 |
| [LM02](manifests/LM02.json) | [되울림의 잿불](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM02.png) | 손 | 생성 후보 | 통과 |
| [LM03](manifests/LM03.json) | [환류의 매듭](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM03.png) | 허리 | 생성 후보 | 통과 |
| [LM04](manifests/LM04.json) | [종말의 회로](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM04.png) | 목걸이 | 생성 후보 | 통과 |
| [LC01](manifests/LC01.json) | [파수꾼의 고리](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LC01.png) | 반지 | 생성 후보 | 통과 |
| [LC02](manifests/LC02.json) | [절제의 서약](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LC02.png) | 목걸이 | 생성 후보 | 통과 |
| [LC03](manifests/LC03.json) | [꺼지지 않는 심장](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LC03.png) | 허리 | 생성 후보 | 통과 |
| [LW05](manifests/LW05.json) | [회전하는 형틀](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW05.png) | 무기 | 생성 후보 | 통과 |
| [LW06](manifests/LW06.json) | [차오르는 격노](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW06.png) | 손 | 생성 후보 | 통과 |
| [LW07](manifests/LW07.json) | [살점 가르는 바람](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW07.png) | 무기 | 생성 후보 | 통과 |
| [LW08](manifests/LW08.json) | [붉은 성벽](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW08.png) | 몸통 | 생성 후보 | 통과 |
| [LW09](manifests/LW09.json) | [재촉하는 손아귀](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW09.png) | 손 | 생성 후보 | 통과 |
| [LW10](manifests/LW10.json) | [메마른 분노](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW10.png) | 반지 | 생성 후보 | 통과 |
| [LW11](manifests/LW11.json) | [불타는 착지](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW11.png) | 발 | 생성 후보 | 통과 |
| [LW12](manifests/LW12.json) | [돌아오는 도약](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW12.png) | 반지 | 생성 후보 | 통과 |
| [LW13](manifests/LW13.json) | [처형의 예고](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW13.png) | 목걸이 | 생성 후보 | 통과 |
| [LW14](manifests/LW14.json) | [충돌의 갑주](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW14.png) | 몸통 | 생성 후보 | 통과 |
| [LW15](manifests/LW15.json) | [포식하는 발뒤꿈치](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW15.png) | 발 | 생성 후보 | 통과 |
| [LW16](manifests/LW16.json) | [침묵시키는 낙하](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW16.png) | 머리 | 생성 후보 | 통과 |
| [LW17](manifests/LW17.json) | [짓눌린 왕관](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW17.png) | 무기 | 생성 후보 | 통과 |
| [LW18](manifests/LW18.json) | [두개골의 메아리](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW18.png) | 손 | 생성 후보 | 통과 |
| [LW19](manifests/LW19.json) | [깊이 패인 흉터](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW19.png) | 무기 | 생성 후보 | 통과 |
| [LW20](manifests/LW20.json) | [피 묻은 집행장](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW20.png) | 허리 | 생성 후보 | 통과 |
| [LW21](manifests/LW21.json) | [전장의 재소집](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW21.png) | 반지 | 생성 후보 | 통과 |
| [LW22](manifests/LW22.json) | [표식 파쇄자](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW22.png) | 손 | 생성 후보 | 통과 |
| [LW23](manifests/LW23.json) | [두 번 울리는 대지](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW23.png) | 무기 | 생성 후보 | 통과 |
| [LW24](manifests/LW24.json) | [포박자의 허리끈](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW24.png) | 허리 | 생성 후보 | 통과 |
| [LW25](manifests/LW25.json) | [폭풍의 개문](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW25.png) | 목걸이 | 생성 후보 | 통과 |
| [LW26](manifests/LW26.json) | [잔해의 보루](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW26.png) | 몸통 | 생성 후보 | 통과 |
| [LW27](manifests/LW27.json) | [발목을 묶는 균열](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW27.png) | 발 | 생성 후보 | 통과 |
| [LW28](manifests/LW28.json) | [함성에 응하는 일격](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW28.png) | 머리 | 생성 후보 | 통과 |
| [LW29](manifests/LW29.json) | [철벽의 파편](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW29.png) | 몸통 | 생성 후보 | 통과 |
| [LW30](manifests/LW30.json) | [회복의 철심](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW30.png) | 허리 | 생성 후보 | 통과 |
| [LW31](manifests/LW31.json) | [눈감지 않는 파수](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW31.png) | 머리 | 생성 후보 | 통과 |
| [LW32](manifests/LW32.json) | [굽히지 않는 맹세](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW32.png) | 반지 | 생성 후보 | 통과 |
| [LW33](manifests/LW33.json) | [막아낸 자의 응답](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW33.png) | 손 | 생성 후보 | 통과 |
| [LW34](manifests/LW34.json) | [울분의 나팔](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW34.png) | 목걸이 | 생성 후보 | 통과 |
| [LW35](manifests/LW35.json) | [전우의 유언](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW35.png) | 몸통 | 생성 후보 | 통과 |
| [LW36](manifests/LW36.json) | [핏빛 박자](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW36.png) | 손 | 생성 후보 | 통과 |
| [LW37](manifests/LW37.json) | [돌격의 칙령](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW37.png) | 발 | 생성 후보 | 통과 |
| [LW38](manifests/LW38.json) | [꺼지지 않는 투지](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW38.png) | 반지 | 생성 후보 | 통과 |
| [LW39](manifests/LW39.json) | [세 번째 선고](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW39.png) | 목걸이 | 생성 후보 | 통과 |
| [LW40](manifests/LW40.json) | [쓴 약의 결의](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LW40.png) | 허리 | 생성 후보 | 통과 |
| [LA05](manifests/LA05.json) | [뒤따르는 살촉](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA05.png) | 무기 | 생성 후보 | 통과 |
| [LA06](manifests/LA06.json) | [먼 사냥의 맹세](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA06.png) | 목걸이 | 생성 후보 | 통과 |
| [LA07](manifests/LA07.json) | [낙인을 읽는 눈](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA07.png) | 머리 | 생성 후보 | 통과 |
| [LA08](manifests/LA08.json) | [그림자의 파열](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA08.png) | 손 | 생성 후보 | 통과 |
| [LA09](manifests/LA09.json) | [늪의 시위](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA09.png) | 무기 | 생성 후보 | 통과 |
| [LA10](manifests/LA10.json) | [퇴로를 여는 화살](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA10.png) | 발 | 생성 후보 | 통과 |
| [LA11](manifests/LA11.json) | [화살비의 씨앗](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA11.png) | 무기 | 생성 후보 | 통과 |
| [LA12](manifests/LA12.json) | [가까워진 죽음](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA12.png) | 손 | 생성 후보 | 통과 |
| [LA13](manifests/LA13.json) | [쏟아지는 장전](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA13.png) | 손 | 생성 후보 | 통과 |
| [LA14](manifests/LA14.json) | [독을 덧칠한 깃](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA14.png) | 무기 | 생성 후보 | 통과 |
| [LA15](manifests/LA15.json) | [낙인의 뇌진탕](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA15.png) | 머리 | 생성 후보 | 통과 |
| [LA16](manifests/LA16.json) | [여유로운 사수](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA16.png) | 반지 | 생성 후보 | 통과 |
| [LA17](manifests/LA17.json) | [덫지기의 조준](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA17.png) | 목걸이 | 생성 후보 | 통과 |
| [LA18](manifests/LA18.json) | [독을 거두는 매듭](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA18.png) | 허리 | 생성 후보 | 통과 |
| [LA19](manifests/LA19.json) | [덫을 떠나는 발](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA19.png) | 발 | 생성 후보 | 통과 |
| [LA20](manifests/LA20.json) | [검은 뿌리](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA20.png) | 몸통 | 생성 후보 | 통과 |
| [LA21](manifests/LA21.json) | [낙인에 스미는 독](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA21.png) | 무기 | 생성 후보 | 통과 |
| [LA22](manifests/LA22.json) | [마지막 독안개](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA22.png) | 반지 | 생성 후보 | 통과 |
| [LA23](manifests/LA23.json) | [착지하는 독구름](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA23.png) | 발 | 생성 후보 | 통과 |
| [LA24](manifests/LA24.json) | [민첩한 재장전](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA24.png) | 손 | 생성 후보 | 통과 |
| [LA25](manifests/LA25.json) | [희미해지는 상처](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA25.png) | 몸통 | 생성 후보 | 통과 |
| [LA26](manifests/LA26.json) | [다시 놓는 올가미](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA26.png) | 허리 | 생성 후보 | 통과 |
| [LA27](manifests/LA27.json) | [물러선 자의 반격](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA27.png) | 목걸이 | 생성 후보 | 통과 |
| [LA28](manifests/LA28.json) | [벗어낸 허물](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA28.png) | 몸통 | 생성 후보 | 통과 |
| [LA29](manifests/LA29.json) | [먹잇감의 보증](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA29.png) | 반지 | 생성 후보 | 통과 |
| [LA30](manifests/LA30.json) | [꿰뚫어 본 최후](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA30.png) | 무기 | 생성 후보 | 통과 |
| [LA31](manifests/LA31.json) | [핏줄 추적자](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA31.png) | 허리 | 생성 후보 | 통과 |
| [LA32](manifests/LA32.json) | [옮겨 적는 사냥명부](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA32.png) | 머리 | 생성 후보 | 통과 |
| [LA33](manifests/LA33.json) | [묶어 두는 시선](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA33.png) | 목걸이 | 생성 후보 | 통과 |
| [LA34](manifests/LA34.json) | [그늘의 개화](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA34.png) | 무기 | 생성 후보 | 통과 |
| [LA35](manifests/LA35.json) | [독그늘의 침식](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA35.png) | 손 | 생성 후보 | 통과 |
| [LA36](manifests/LA36.json) | [그림자 족쇄](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA36.png) | 몸통 | 생성 후보 | 통과 |
| [LA37](manifests/LA37.json) | [연금술사의 박동](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA37.png) | 반지 | 생성 후보 | 통과 |
| [LA38](manifests/LA38.json) | [세 번 당긴 시위](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA38.png) | 목걸이 | 생성 후보 | 통과 |
| [LA39](manifests/LA39.json) | [독에 익숙한 가죽](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA39.png) | 몸통 | 생성 후보 | 통과 |
| [LA40](manifests/LA40.json) | [안개를 마신 장화](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LA40.png) | 발 | 생성 후보 | 통과 |
| [LM05](manifests/LM05.json) | [오래 남는 잿불](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM05.png) | 무기 | 생성 후보 | 통과 |
| [LM06](manifests/LM06.json) | [흠 없는 화로](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM06.png) | 목걸이 | 생성 후보 | 통과 |
| [LM07](manifests/LM07.json) | [잿불의 승계](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM07.png) | 손 | 생성 후보 | 통과 |
| [LM08](manifests/LM08.json) | [불꽃 뿌리](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM08.png) | 머리 | 생성 후보 | 통과 |
| [LM09](manifests/LM09.json) | [재에서 길어 올린 마력](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM09.png) | 반지 | 생성 후보 | 통과 |
| [LM10](manifests/LM10.json) | [틈을 여는 불씨](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM10.png) | 발 | 생성 후보 | 통과 |
| [LM11](manifests/LM11.json) | [눈송이의 파편](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM11.png) | 무기 | 생성 후보 | 통과 |
| [LM12](manifests/LM12.json) | [먼 겨울의 심판](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM12.png) | 목걸이 | 생성 후보 | 통과 |
| [LM13](manifests/LM13.json) | [서리 껍질](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM13.png) | 몸통 | 생성 후보 | 통과 |
| [LM14](manifests/LM14.json) | [차가운 순환](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM14.png) | 반지 | 생성 후보 | 통과 |
| [LM15](manifests/LM15.json) | [얼어붙는 문장](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM15.png) | 손 | 생성 후보 | 통과 |
| [LM16](manifests/LM16.json) | [빙결된 기억](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM16.png) | 머리 | 생성 후보 | 통과 |
| [LM17](manifests/LM17.json) | [넘쳐 흐르는 전하](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM17.png) | 무기 | 생성 후보 | 통과 |
| [LM18](manifests/LM18.json) | [근접한 천벌](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM18.png) | 목걸이 | 생성 후보 | 통과 |
| [LM19](manifests/LM19.json) | [폭풍의 연속음](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM19.png) | 손 | 생성 후보 | 통과 |
| [LM20](manifests/LM20.json) | [되찾은 전류](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM20.png) | 반지 | 생성 후보 | 통과 |
| [LM21](manifests/LM21.json) | [끈적이는 번개](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM21.png) | 머리 | 생성 후보 | 통과 |
| [LM22](manifests/LM22.json) | [겨울을 부르는 뇌성](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM22.png) | 허리 | 생성 후보 | 통과 |
| [LM23](manifests/LM23.json) | [공간의 방전](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM23.png) | 발 | 생성 후보 | 통과 |
| [LM24](manifests/LM24.json) | [낙뢰의 도착](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM24.png) | 무기 | 생성 후보 | 통과 |
| [LM25](manifests/LM25.json) | [가벼워진 경계](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM25.png) | 발 | 생성 후보 | 통과 |
| [LM26](manifests/LM26.json) | [도약하는 화염](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM26.png) | 목걸이 | 생성 후보 | 통과 |
| [LM27](manifests/LM27.json) | [틈새의 외투](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM27.png) | 몸통 | 생성 후보 | 통과 |
| [LM28](manifests/LM28.json) | [공간에서 건진 마력](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM28.png) | 반지 | 생성 후보 | 통과 |
| [LM29](manifests/LM29.json) | [겨울의 피난처](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM29.png) | 몸통 | 생성 후보 | 통과 |
| [LM30](manifests/LM30.json) | [회복의 결정막](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM30.png) | 허리 | 생성 후보 | 통과 |
| [LM31](manifests/LM31.json) | [충전된 외피](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM31.png) | 손 | 생성 후보 | 통과 |
| [LM32](manifests/LM32.json) | [끊어지지 않는 탈출로](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM32.png) | 발 | 생성 후보 | 통과 |
| [LM33](manifests/LM33.json) | [한기를 두른 매듭](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM33.png) | 허리 | 생성 후보 | 통과 |
| [LM34](manifests/LM34.json) | [두 번째 겨울](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM34.png) | 무기 | 생성 후보 | 통과 |
| [LM35](manifests/LM35.json) | [결박된 마력](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM35.png) | 반지 | 생성 후보 | 통과 |
| [LM36](manifests/LM36.json) | [깊어지는 눈보라](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM36.png) | 목걸이 | 생성 후보 | 통과 |
| [LM37](manifests/LM37.json) | [얼음을 깨는 서약](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM37.png) | 손 | 생성 후보 | 통과 |
| [LM38](manifests/LM38.json) | [기초의 샘](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM38.png) | 반지 | 생성 후보 | 통과 |
| [LM39](manifests/LM39.json) | [아껴 둔 불씨](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM39.png) | 머리 | 생성 후보 | 통과 |
| [LM40](manifests/LM40.json) | [약병 속 결정](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/LM40.png) | 허리 | 생성 후보 | 통과 |
| [DES_LW41](manifests/DES_LW41.json) | [추격자의 쇠사슬](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/DES_LW41.png) | 목걸이 | 생성 후보 | 통과 |
| [DES_LW42](manifests/DES_LW42.json) | [상처를 거두는 손](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/DES_LW42.png) | 손 | 생성 후보 | 통과 |
| [DES_LW43](manifests/DES_LW43.json) | [움직이는 성벽](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/DES_LW43.png) | 몸통 | 생성 후보 | 통과 |
| [DES_LW44](manifests/DES_LW44.json) | [산맥의 진동](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/DES_LW44.png) | 허리 | 생성 후보 | 통과 |
| [DES_LW45](manifests/DES_LW45.json) | [마르지 않는 결의](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/DES_LW45.png) | 반지 | 생성 후보 | 통과 |
| [DES_LW46](manifests/DES_LW46.json) | [선조왕의 관](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/DES_LW46.png) | 머리 | 생성 후보 | 통과 |
| [DES_LA41](manifests/DES_LA41.json) | [인내하는 사냥활](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/DES_LA41.png) | 무기 | 생성 후보 | 통과 |
| [DES_LA42](manifests/DES_LA42.json) | [겨울 가시 손아귀](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/DES_LA42.png) | 손 | 생성 후보 | 통과 |
| [DES_LA43](manifests/DES_LA43.json) | [미끼를 두른 외투](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/DES_LA43.png) | 몸통 | 생성 후보 | 통과 |
| [DES_LA44](manifests/DES_LA44.json) | [갈라진 독니](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/DES_LA44.png) | 목걸이 | 생성 후보 | 통과 |
| [DES_LA45](manifests/DES_LA45.json) | [준비된 초소](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/DES_LA45.png) | 허리 | 생성 후보 | 통과 |
| [DES_LA46](manifests/DES_LA46.json) | [이중 사냥의 맹세](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/DES_LA46.png) | 반지 | 생성 후보 | 통과 |
| [DES_LM41](manifests/DES_LM41.json) | [서로 먹이는 불씨](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/DES_LM41.png) | 손 | 생성 후보 | 통과 |
| [DES_LM42](manifests/DES_LM42.json) | [유리빙하 지팡이](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/DES_LM42.png) | 무기 | 생성 후보 | 통과 |
| [DES_LM43](manifests/DES_LM43.json) | [벼락 수집기](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/DES_LM43.png) | 목걸이 | 생성 후보 | 통과 |
| [DES_LM44](manifests/DES_LM44.json) | [고요한 균열](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/DES_LM44.png) | 허리 | 생성 후보 | 통과 |
| [DES_LM45](manifests/DES_LM45.json) | [나침반의 중심](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/DES_LM45.png) | 몸통 | 생성 후보 | 통과 |
| [DES_LM46](manifests/DES_LM46.json) | [세 끝의 왕관석](../../../Assets/HELLSCRIPT/Resources/Art/ClassLegendaryIcons/DES_LM46.png) | 반지 | 생성 후보 | 통과 |

## 시각 검수 자료

같은 행에 묶인 ID를 밝은·어두운 바탕의 64px 비교와 원본 해상도 가장자리 표본으로 확인했습니다.

| 검수 ID | 비교 자료 |
| --- | --- |
| LW01, LW02, LW03, LW04, LA01, LA02, LA03, LA04, LM01, LM02 | [first-ten.png](qa/first-ten.png) / [first-ten-native-edges.png](qa/first-ten-native-edges.png) |
| LM03, LM04, LC01, LC02, LC03, LW05, LW06, LW07 | [batch-003.png](qa/batch-003.png) / [batch-003-edges.png](qa/batch-003-edges.png) |
| LW08, LW09, LW10, LW11, LW12, LW13, LW14, LW15, LW16 | [batch-006.png](qa/batch-006.png) / [batch-006-edges.png](qa/batch-006-edges.png) |
| LW17, LW18, LW19, LW20, LW21, LW22, LW23, LW24, LW25, LW26, LW27, LW28 | [batch-007.png](qa/batch-007.png) / [batch-007-edges.png](qa/batch-007-edges.png) |
| LW29, LW30, LW31, LW32, LW33, LW34, LW35, LW36, LW37 | [batch-008.png](qa/batch-008.png) / [batch-008-edges.png](qa/batch-008-edges.png) |
| LW38, LW39, LW40, LA05, LA06, LA07, LA08, LA09, LA10, LA11, LA12, LA13 | [batch-009.png](qa/batch-009.png) / [batch-009-edges.png](qa/batch-009-edges.png) |
| LA14, LA15, LA16, LA17, LA18, LA19, LA20, LA21, LA22, LA23 | [batch-010.png](qa/batch-010.png) / [batch-010-edges.png](qa/batch-010-edges.png) |
| LA24, LA25, LA26, LA27, LA28, LA29, LA32 | [batch-011.png](qa/batch-011.png) / [batch-011-edges.png](qa/batch-011-edges.png) |
| LA30, LA31, LA33, LA34, LA35, LA36, LA37, LA38, LA39 | [batch-012.png](qa/batch-012.png) / [batch-012-edges.png](qa/batch-012-edges.png) |
| LA40, LM05, LM06, LM07, LM08, LM09, LM10, LM11, LM12, LM13, LM14, LM15 | [batch-013.png](qa/batch-013.png) / [batch-013-edges.png](qa/batch-013-edges.png) |
| LM16, LM17, LM18, LM19, LM20, LM21, LM22, LM23, LM24, LM25, LM26, LM27 | [batch-014.png](qa/batch-014.png) / [batch-014-edges.png](qa/batch-014-edges.png) |
| LM28, LM29, LM30, LM31, LM32, LM33, LM34, LM35, LM38, LM39 | [batch-015.png](qa/batch-015.png) / [batch-015-edges.png](qa/batch-015-edges.png) |
| LM36, LM40 | [batch-016.png](qa/batch-016.png) / [batch-016-edges.png](qa/batch-016-edges.png) |
| LM37 | [batch-017.png](qa/batch-017.png) / [batch-017-edges.png](qa/batch-017-edges.png) |
| DES_LW41, DES_LW42, DES_LW43, DES_LW44, DES_LW45, DES_LA41, DES_LA42, DES_LA43 | [batch-004.png](qa/batch-004.png) / [batch-004-edges.png](qa/batch-004-edges.png) |
| DES_LW46 | [batch-004.png](qa/batch-004.png) / [batch-004-edges.png](qa/batch-004-edges.png) / [DES_LW46-top-edge.png](qa/DES_LW46-top-edge.png) |
| DES_LA44, DES_LA45, DES_LA46, DES_LM41, DES_LM42, DES_LM43, DES_LM44, DES_LM45, DES_LM46 | [batch-005.png](qa/batch-005.png) / [batch-005-edges.png](qa/batch-005-edges.png) |
