# 스킬 창 흐름 정리 — 착용 스킬 줄·닫기·옛 안내 팝업 제거 (2026-10-24)

> **2026-10-28 A2 정정:** 큰 칙령 창·≡ 메뉴·전용 부품·스모크 설명은 과거 기록이다. [현재 진입 코드](../../Assets/HELLSCRIPT/Runtime/Presentation/GameUI.HuntEdict.cs)와 [대체 화면·소유 코드](Hunt_Edict_Window_Removal_20261028.md)를 따른다. 이전 캡처와 검사 수치는 당시 구현의 근거다.

[English](Skill_Window_Flow_20261024.en.md). 사용자 테스트(main `a045c626`) 보고 8·12·13(Part A)과 항목 17(Part B, [사냥칙령 하위 페이지 계약](Skill_Edict_Pages_Contract_20261024.md))을 한 단위로 묶은 기록이다. 이 문서는 Part A의 결정·규칙·인계를 한곳에 둔다.

## 시안(승인 기준)

디자인 아티팩트 `HELLSCRIPT 스킬 트리 · 캐주얼`(https://claude.ai/artifact/JsCKnJUbKkzfBYyJDR3493)을 확장해 Part A·Part B를 한 라운드로 담았다. **사용자 승인 전에는 화면 코드를 쓰지 않는다**(승인은 사용자 본인의 말만 인정).

- 착용 스킬 줄: `Eq-*` 보드(눌림+'−'+트리 반짝임, 빈 슬롯, 교체 직후, 맞교환 직후, 액티브 아님, 궁극기 잠김 Lv.40, 가로·PC, English).
- 기존 24개 직업별 보드는 같은 컴포넌트라 착용 줄·진입 행이 함께 보인다.

## 코드로 확인한 사실

- 게임에서 스킬 창은 `HuntEdictWindow.OpenD4SkillTree` 한 곳에서만 열리고, 닫힘 콜백이 사냥 칙령 창을 다시 그린다 — 이것이 "X를 누르면 칙령 창으로 간다"의 원인이다. 균열 입장의 **빈 슬롯**은 `GameUI.ShowEdictSkillTree()`로 칙령 창을 거쳐 열고, **채워진 슬롯**은 옛 피커(`RiftEntryWindow.ShowSkills`)를 연다.
- `ShowEdictSkillTree()`는 가이드 튜토리얼도 쓴다(`GameUI.GuideTutorial.cs`, `GameUI.GuideModifierLesson.cs`). 이 호출은 남긴다.
- `SkillTreeD4Window`는 호스트가 `store`를 넘기면 직업 탭을 숨긴다(`showClassTabs=store==null`). 사용자가 표시한 초록 네모는 그 빈 자리다. 사용자가 직접 확인했다(2026-10-24): 스킬 창 최상위의 '전사·궁수·마법사' 직업 전환 버튼은 게임에 표시하지 않는다 — 시안에서는 점선 테두리로 시안용임을 표시했다.

## 규칙 (시안과 동일)

| 입력 | 동작 |
| --- | --- |
| 슬롯 누름 | 그 슬롯이 눌림 상태가 되고 트리가 그 스킬 위치로 스크롤하며 카드가 반짝인다. 같은 슬롯을 다시 누르면 해제(기본). |
| 눌린 슬롯의 '−' | 슬롯을 비운다(빈 슬롯), 눌림 해제. |
| 눌림 상태에서 배운 액티브를 누름 | 그 슬롯에 들어간다. 이미 다른 슬롯에 있으면 두 슬롯이 맞교환. 변경 뒤 눌림 해제. |
| 눌림 상태에서 그 밖의 것(패시브·보조·미습득 액티브·궁극기) | 변경 없음. 하단 상세에 설명만, 한 줄 사유를 알림. |
| 궁극기 칸 | 레벨 40 미만이면 잠금. 누르면 해금 조건(Lv.40)을 알린다. |
| 닫기(X) | 변경을 한 번에 저장하고 **연 화면**으로 돌아간다(균열 입장에서 열었으면 균열 입장). |

저장은 닫을 때 한 번의 트랜잭션이다. 조합 규칙은 기존 빌드 검증(`ClassSkillTree.Equip` 등)을 그대로 쓴다. 저장 실패 시 이전 상태를 유지하고 알린다. 사냥칙령 편집본 저장(Part B)과는 서로 독립이다.

## 시안과 게임 좌표 차이 (Unity가 맞출 곳)

트리·상세 패널의 프레임 좌표는 **바꾸지 않는다**. 착용 줄은 `TabsAt` 자리(세로 `(16,112,408,44)`, 가로 `(150,10)`, PC `(120,12)`)에 들어간다. 사용자 요청(2026-10-24): 사냥칙령 진입(Part B)은 별도 행이 아니라 **상세 패널 머리글의 스킬 이름 오른쪽 44px 아이콘 버튼**이라, 상세 패널에 스크롤을 만들지 않는다.

## 사냥칙령 진입 아이콘

사용자 요청(2026-10-24)으로 상세 패널의 하단 행을 없애고 스킬 이름 오른쪽에 44px 아이콘 버튼을 두었다. 그림은 Codex(내장 이미지 생성, 모델은 표면이 알려 주지 않아 gpt-image-2 사용 여부 미확인)가 만든 남색 금테 메달리온이며 `Docs/Art/SkillEdictIcon20261024/`(512², `manifest.json`에 프롬프트·해시·후처리)에 있다. 시안 아티팩트에는 256²을 올렸다. production 승인 전이다.

## Part A 구현 (2026-10-24, 브랜치 `claude/skill-window-flow-20261024-8afffa`)

사용자가 2026-10-24에 시안을 승인했고 아래 Part A를 구현했다. Part B는 구현하지 **않았다**(메모만).

- **닫기(항목 8)**: `SkillTreeD4Window.Close()`가 슬롯 초안을 한 번 저장하고, `CloseError`를 연 쪽에 알리고, `closed`를 부른다. `HuntEdictWindow`는 오류를 알리고 이전처럼 다시 그리며, 균열 입장 창은 스스로 다시 그린다. 옛 사냥 칙령 창은 그대로이고 스킬 창은 더 이상 그쪽으로 연결하지 않는다.
- **균열 입장(항목 12)**: `RiftEntryWindow.SkillPicker.cs`와 `RiftSkillPickerLayoutTests.cs`를 삭제했다. 슬롯 버튼 세 개는 `RiftEntryWindow.OpenSkillWindow(slot)`을 부르며, 그 슬롯이 눌린 채로 실제 스킬 창이 열린다. 잠긴 궁극기 칸(Lv.40 미만)은 "궁극기는 Lv.40에 해금돼요." 알림만 띄운다. 닫은 뒤 슬롯 줄은 기존 dirty 플래그로 갱신한다(UI_Refresh 규칙, 깜빡임 없음).
- **착용 줄(항목 13)**: `SkillTreeD4.SlotDraft.cs`(순수 규칙), `SkillTreeD4EquipLooks.cs`(코드 정의 룩 — 생성된 looks JSON은 해시 검사 그대로), `SkillTreeD4Window.Equipped.cs`(줄, '−' 배지, 카드 슬롯 번호, 반짝임, 부드러운 스크롤, 알림). 줄은 호스트가 슬롯 호스트이면서 store가 있을 때(게임) 또는 `previewEquipped`일 때(시안·증거)만 나타나고, 일반 디자인 호스트의 직업 탭은 그대로다.
- **저장**: `GameStore.CommitSkillSlots(heroId, opened, desired, catalog, simulation)`은 `ClassSkillTree.SetSlots`와 기존 빌드 검증을 거치는 원자적 한 번의 쓰기다. 낡은 `opened`, 다른 영웅, 중복, 종류가 틀린 스킬은 아무것도 쓰지 않는다. `SetSlots`는 유지되는 스킬의 자동 사용 여부·순서를 보존하고, 새로 착용한 스킬은 바뀐 스킬 자리에서 자동으로 시작한다.
- **창이 열린 동안의 포인트 확정**: 마지막 랭크를 환불해 스킬이 사라지면 초안을 다시 맞춘다. 새 스킬이 사라진 슬롯은 초안이 그 스킬을 다른 곳으로 옮기지 않았다면 저장돼 있던 스킬로 돌아간다. 다시 맞추는 것은 이 창 자신의 포인트 확정뿐이라, 다른 곳에서 바뀐 배치는 닫을 때 낡은 상태 검사에서 여전히 실패한다(오류 표시, 이전 상태 유지).
- **정한 기본값**(아래 질문은 그대로 열려 있다): 같은 슬롯 재탭=해제, 교체·맞교환·빼기 뒤 눌림 해제, 맞교환은 두 스킬의 자동 사용 여부·실행 순서 보존, 균열 입장의 슬롯으로 열면 그 슬롯이 눌린 채 시작(채워진 슬롯도 동일).

### 검증

좁은 EditMode만 실행했고 전체는 돌리지 않았다. 1차(8개 클래스: SkillTreeD4SlotDraftTests, SkillTreeD4SlotWindowTests, RiftEntryTests, RiftEntryCasualWindowTests, SkillTreeD4StoreHostTests, SkillTreeD4WindowTests, SkillTreeD4InputTests, LocalizationTests): 131개 중 124 통과, 7 실패 — 일곱 개 모두 새 창 테스트가 편집 모드에서 `Destroy`를 부른 탓이며 `Close()`가 재생 중이 아닐 때 `DestroyImmediate`를 쓰도록 고쳤다. 2차(새 클래스 둘): 24/24 통과. 나머지 여섯 클래스는 1차에서 통과했고 그 뒤 입력이 바뀐 곳은 새 테스트가 덮는다(`SkillTreeD4Window.cs`의 `Close`, `SlotDraft.Rebase`, `Equipped`의 배지 조건·스크롤 취소). 미실행: 런타임 스모크 `RuntimeRiftEntrySmoke*`(새 `EquipThroughWindow` 흐름으로 고쳤으나 컴파일만 확인), PlayMode, 실기기, 전체 EditMode.

## 사용자 결정이 필요한 것

1. ~~시안 승인~~(2026-10-24 완료).
2. 같은 슬롯을 다시 누르면 눌림 해제(기본) 여부, 교체·맞교환·빼기 뒤 눌림을 해제(기본)할지 유지할지.
3. 맞교환 때 두 스킬의 자동 사용 켜짐/꺼짐과 실행 순서를 그대로 보존(기본)한다.
