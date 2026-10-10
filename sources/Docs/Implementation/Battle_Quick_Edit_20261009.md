# 전투 HUD 빠른 편집 (2026-10-09)

[English](Battle_Quick_Edit_20261009.en.md)

전투 중 메뉴를 거치지 않고 HUD에서 사냥 칙령·물약·스킬을 바꾼다. 열려 있는 동안 전투는 일시정지되고, 닫으면 원래 상태로 돌아간다. 일반 균열 전투에서만 동작한다(튜토리얼·훈련·퍼즐·비교 전투는 기존 경로).

- **사냥 칙령 버튼**(인장 위): 전투 위치·거리 항목(첫 교전 위치, 기본 전투 위치, 희망 거리, 선회 방향). 선택지를 한 번 누르면 예상 이동(3초, 분리된 시뮬레이션 `CombatSimulation.ForecastHeroPath`)을 바닥에 그리고, 한 번 더 누르면 `CommitHuntEdict`로 저장한다. 공개 여부는 `HuntEdictProgression.Direct`를 따른다.
- **물약 설정 버튼**(HP·자원 바 오른쪽): HP·자원 기준을 드래그 바로 지정하고 `변경`으로 저장(`survival.potionHpPercent`, `potion.resourcePercent`, 0~100%).
- **스킬·궁극기·물약 칸**: 짧게 누르면 후보 목록(없으면 열리지 않음), 후보를 누르면 장착(`ClassSkillTree.Equip` + `CommitHuntEdict`, 물약은 `SelectPotionSlot`). 0.3초 뒤 게이지가 0.7초간 차면 중앙 상세 팝업(`HoldPress`). 스킬 팝업에서는 사냥 칙령 빠른 프리셋도 바꾼다.
- **결정**: 물약 칸은 원래 마을·포탈 체크포인트에서만 바꿀 수 있었으나, 전투 중 교체를 허용했다. 보조 물약은 효과가 진행 중이면 교체할 수 없다.
- **전투 로그 제거**: 전투 화면의 실시간 로그 줄·버튼을 뺐다(기록 화면은 그대로). 해당 스모크 두 개를 "로그 없음" 확인으로 바꿨다. `GameUI.CombatJournal.cs`의 패널 코드는 호출되지 않는 상태로 남아 있다.
- **검증**: `HuntEdictQuickMenuTests` 8개, HUD·지역화·공통 UI 테스트, `check_ui_contract.py --verify` 통과. 예상 경로는 같은 선택을 저장한 뒤의 실제 경로와 일치함을 테스트로 확인했다.
- **macOS 개발 빌드 스모크**(`RuntimeBattleQuickEditSmoke`, 정리된 전사 계정 복사본, 440×956·956×440·1600×900): 칙령 저장, 물약 기준 저장, 스킬 장착, 보조 물약 장착, 길게 누르기 게이지·팝업, 전투 로그 없음을 통과. `RuntimeCombatJournalSmoke`(HUD 전용)도 통과.
- **미검증**: `RuntimeFirstPlayAcceptance`, 실제 손가락 입력·실기기(macOS 합성 입력만 확인). 항목 타일은 아이콘 그림 없이 글자만 쓴다.

## 후속 조정 (2026-10-09)

- 물약 기준 패널을 실제 HUD의 HP·자원 바 부품(트랙·채움·테두리·같은 색·같은 라벨)으로 다시 그렸다. 채움은 지금 상태, 금빛 눈금이 기준, 눈금 이하는 밝게 칠한다.
- 세로 화면의 하단 HUD를 키웠다: 세로에서만 구성 폭을 줄여 HP 바·설정 버튼 영역과 물약 영역이 맞닿는다(`GlobalHudLayout`). 가로 구성은 그대로이고, "회전해도 구성 불변" 테스트는 세로의 물약 묶음 이동만 허용하도록 바꿨다.
- 물약 설정 버튼을 버튼 모양의 새 그림(`menu-potion-settings`, Codex 생성, `Docs/Art/PotionSettingsButton/`)으로 바꿨다.

전체 Edit Mode 통합 결과와 실패 항목 보완은 [전투 피해 그래프](Damage_Graphs_20261010.md#통합-검사-2026-10-10)에 기록했다. 미승인 칙령 아이콘 후보 5장은 `Docs/Art/EdictHudIcons/Candidates/`에 보관하며 게임에서 불러오지 않는다.
