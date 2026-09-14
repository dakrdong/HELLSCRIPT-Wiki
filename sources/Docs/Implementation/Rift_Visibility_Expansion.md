# HELLSCRIPT 실시간 시야와 탐색 지도

작성일: 2026-09-14

[English](Rift_Visibility_Expansion.en.md)

## 플레이 규칙

플레이어 주위 12m의 360도 시야를 사용한다. 실제 던전의 벽, 모서리와 시야를 차단하는 장애물에 가린 곳은 보이지 않는다. 방 진입 여부로 방 전체를 공개하지 않으며 문밖에서도 시야가 닿은 내부만 드러낸다.

처음 보는 지형은 검은색, 현재 시야에 있는 지형은 원래 색, 전에 보았으나 현재 가려진 지형은 어두운 회색으로 표시한다. 기억 대상은 바닥과 고정 벽의 형태다. 몬스터, 상자, 성소와 장식 오브젝트는 현재 시야에서만 표시한다. 시야를 확보한 바닥은 직접 밟지 않았어도 기록한다.

## 오버레이 지도와 설정

전투 화면 중앙에 플레이어 표식을 고정하고, 발견한 외벽·통로 윤곽을 반투명 선으로 표시한다. 이동 시 지도는 매 프레임 플레이어 표시 위치를 따라 움직인다. 화면의 사선 방향에 맞추되 HUD와 설정 창은 지도 위에 놓인다. 지도는 클릭과 터치를 받지 않는다.

설정 → 화면의 ‘오버레이 지도 표시’ 토글은 기본 켜짐이다. 변경은 즉시 반영되며 기기의 hellscript-overlay-map-v1.json에 저장한다. 꺼진 동안에도 탐색은 기록된다. 기존 미니맵과 확대 지도도 같은 세밀한 탐색 데이터를 사용한다. 미니맵의 기존 발견 지점 표시는 유지하며 적의 현재 위치는 관측 가능한 경우에만 표시한다.

## 구조와 담당 범위

던전 생성·순환 동선과 이번 시야 기능은 별도로 구현했다. [RiftSurface.cs](../../Assets/HELLSCRIPT/Runtime/Core/RiftSurface.cs)와 [RiftNavigation.cs](../../Assets/HELLSCRIPT/Runtime/Core/RiftNavigation.cs)의 기존 실제 바닥·차폐 판정을 읽으며 지형을 새로 생성하지 않는다.

[RiftVisibility.cs](../../Assets/HELLSCRIPT/Runtime/Core/RiftVisibility.cs)는 0.35m 격자의 현재 시야와 누적 탐색 기록을 관리한다. 원본 위치가 바뀐 시뮬레이션 틱과 관문 상태 변화에 맞춰 갱신한다. 화면 비율, 가시거리 확대와 지도 On/Off는 발견 범위를 바꾸지 않는다. 순간이동은 도착 위치만 계산하며 두 위치 사이를 가상으로 탐색하지 않는다.

기존 방 방문 기록은 자동 이동의 근거로 유지한다. 새로운 discovery 기록은 같은 RunState에 저장되고 런타임 캐시는 직렬화하지 않는다. 새 균열은 빈 기록으로 시작한다. 이전 저장에서 이미 공개됐던 방과 양 끝 방이 방문된 통로는 회색 탐색 기록으로 이관한다. legacy 훈련·고정 필드의 월드 시야 처리는 유지한다.

[RiftFogView.cs](../../Assets/HELLSCRIPT/Runtime/Presentation/RiftFogView.cs)와 지형 셰이더가 실제 지형 표시에, [RiftAutomap.cs](../../Assets/HELLSCRIPT/Runtime/Presentation/RiftAutomap.cs)가 중앙 지도에 같은 마스크를 적용한다. 텍스처·복제 재질은 해당 던전 루트가 소유하고 제거 시 해제한다. 오버레이 윤곽 메시를 매 프레임 다시 만들지 않고 화면상의 이동만 갱신한다.

## 검증

Unity 6000.6.0f1의 별도 검증 프로젝트에서 시야·탐색 이동·한영 문구·가시거리 관련 Edit Mode 검사 64개가 모두 통과했다. 문밖 시야, 벽 뒤 차폐, 회색 지형 기억, 저장 복원, 이전 저장 이관, 순간이동, 닫힌 관문의 개방, 실제 이동 시야와 격자 판정의 일치, 설정 저장 실패·재시도를 확인한다. [검사 결과](RiftVisibilityEvidence/editmode-results.xml)에 세부 결과를 보존한다.

macOS 개발 빌드는 빌드 오류 0개로 성공했다. 실제 게임 실행에서 오브젝트 차폐, 플레이어 중심 지도, 지도 영역의 입력 통과, 포인터로 조작한 토글, 한영 문구와 가로·세로 배치, 실제 생성 던전의 전투 진행을 확인했다. 별도 프로세스로 다시 실행한 뒤 탐색 기록과 지도 끄기 설정이 복원됐다. 기존 화면 설정 검사도 통과했다. 화면 비율 10개, 글자 크기·가시거리의 모든 21단계, 슬라이더·휠·버튼 입력, 설정 중 일시정지와 복귀, 재실행 후 설정 복원을 포함한다.

0.35m 격자와 12m 시야에서 시야 갱신은 생성 지도 1개, 60회 표본 기준 평균 2.85ms였다. 해당 지도의 전체 격자는 200,604칸이다. 짧은 실제 전투 실행은 화면 캡처 비용을 포함해 약 51.90 FPS를 기록했다. 이는 로컬 macOS 개발 빌드의 관찰값이며 목표 기기의 성능 보장이 아니다. 모바일 실기기 성능과 여러 시드에서의 장시간 메모리·프레임 시간은 추가 검증 대상이다.

검증용 두 방·통로·기둥 지형의 [문밖 시야](RiftVisibilityEvidence/01-doorway.png), [방 내부](RiftVisibilityEvidence/02-room.png), [돌아온 뒤 회색 지형](RiftVisibilityEvidence/03-remembered.png), [한국어 설정](RiftVisibilityEvidence/04-settings-ko.png), [지도 끄기](RiftVisibilityEvidence/05-overlay-off.png), [영어 세로 설정](RiftVisibilityEvidence/06-settings-en-portrait.png), [세로 지도](RiftVisibilityEvidence/07-overlay-portrait.png), [재실행 복원](RiftVisibilityEvidence/09-restart.png)을 보존한다. [생성 던전 화면](RiftVisibilityEvidence/08-generated.png)은 실제 생성 규칙으로 만든 균열에서 촬영했다.

근거: [실행 결과](RiftVisibilityEvidence/runtime.txt), [재실행 결과](RiftVisibilityEvidence/restart.txt), [기존 화면 설정 검사](RiftVisibilityEvidence/settings-runtime.txt), [화면 설정 재실행 검사](RiftVisibilityEvidence/settings-restart.txt), [검증 조건·수치·소스 해시](RiftVisibilityEvidence/validation.json), [시야 검사 코드](../../Assets/HELLSCRIPT/Tests/Editor/RiftVisibilityTests.cs), [실제 실행 검사 코드](../../Assets/HELLSCRIPT/Runtime/Presentation/RuntimeVisibilitySmoke.cs).

## 이전 규칙과의 관계

이 문서는 [자동 생성 균열 상세 기획](../Design/HELLSCRIPT_Rift_Exploration_Detail.md)과 [균열 개발 기록](Rift_Expansion.md)의 방 단위 공개 규칙을 세밀한 현재 시야·누적 탐색 규칙으로 대체한다. 첨부 화면은 플레이어 중심 반투명 윤곽 지도의 시각적 참고로 사용했다. 화면 속 게임 정보나 문구를 별도 요구사항으로 해석하지 않았다.

[유기적인 균열 지형 개발 기록](Organic_Rift_Expansion.md)의 생성·순환 동선·보스 소환 규칙은 그대로 사용한다. 다른 작업이 담당한 생성기·바닥 판정·이동 판정 코드는 변경하지 않았고, 넘겨받은 월드 표시·미니맵·시뮬레이션 연결 지점에 이번 기능을 추가했다. Unity 패키지, 씬, 프리팹과 기존 이미지 에셋은 변경하지 않는다.
