# 대장간 HTML 기능 시연 / Forge HTML prototype

`HELLSCRIPT-Blacksmith.html`을 브라우저에서 열면 별도 설치 없이 조작할 수 있습니다. CSS, JavaScript, 옵션·슬롯 카탈로그, SVG 아이콘과 기존 프로젝트의 캐릭터 이미지를 파일 안에 모두 포함했습니다.

2026-09-22: 이 시연을 기준으로 실제 장비·계정 재화·저장을 연결한 Unity 대장간을 구현했습니다. 아래의 이전 검증 기록은 당시 HTML 시연의 범위입니다. 현재 Unity 규칙과 저장 이전은 [적용 명세](../../Docs/Implementation/Blacksmith_Unity_Integration.md), 실행 결과는 [검증 기록](../../Docs/Implementation/Blacksmith_Validation.md)을 확인하세요. 시연 장비 이름은 기존 인벤토리 등급 팔레트를 사용하며 별도 고유 등급을 만들지 않습니다.

2026-09-22: The native Unity forge now connects real equipment, account currencies and persistence. Earlier validation entries below describe the HTML prototype at that time. See the [integration specification](../../Docs/Implementation/Blacksmith_Unity_Integration.en.md) and [validation record](../../Docs/Implementation/Blacksmith_Validation.en.md) for current Unity behavior. Prototype item names use the shared inventory grade palette; unique effects do not introduce a new grade.

## 한국어

착용 장비와 인벤토리의 시연 장비 10개를 선택할 수 있습니다. 변경할 옵션 줄을 고르면 해당 장비 부위·접두/접미·다른 줄의 중복 그룹을 반영한 후보와 수치 범위, 출현 확률을 표시합니다. 첫 유료 실행에서 선택한 슬롯 ID를 고정합니다. 이후 장비 전환이나 새로고침으로 다른 줄을 선택할 수 없으며, 같은 종류나 더 낮은 수치가 다시 나올 수도 있습니다.

한 번이라도 옵션을 변경한 장비는 목록 카드에 `옵션 이름 +현재 수치 (범위 내 위치%)`를 한 줄로 표시합니다. 카드에는 막대 게이지나 최소·최대 수치를 표시하지 않습니다. 좁은 화면에서는 긴 옵션 이름만 말줄임 처리하며, 전체 이름은 툴팁으로 확인할 수 있습니다. 위치는 해당 장비 레벨의 범위를 기준으로 `(현재 수치 − 최솟값) / (최댓값 − 최솟값) × 100`이며 소수점 한 자리까지 표시합니다. 예를 들어 공격 속도 3~8% 범위에서 +7.76%는 95.2%입니다. 최솟값은 0%, 최댓값은 100%이며, 다른 장비를 선택해도 각 카드에는 그 장비의 고정 옵션을 표시합니다. 옵션을 변경하지 않은 장비에는 이 요약을 표시하지 않습니다.

자동 돌리기는 1회부터 `floor(보유 골드 / 1회 비용)`까지 정수로 설정합니다. 체크한 옵션 중 하나가 나오면 추가 실행과 차감 없이 멈춥니다. 결과 팝업에서 옵션·수치·최소/최대·사용 골드·남은 횟수를 확인하고 멈추기 또는 계속 돌리기를 선택합니다. 마지막 횟수에 목표가 나와도 팝업을 표시하며 계속 돌리기는 비활성화합니다. 자동 실행 중 페이지를 떠나거나 새로고침하면 중단하며, 이미 확정한 결과는 유지합니다.

프로젝트의 `ItemCatalog` 정의 54종, `ItemGenerator.RerollPool` 후보 규칙, 직업 가중치, 레벨별 티어 분포, 평면 수치 보정과 `Economy.RerollGold`의 `500 × L + 50 × L × (L − 1)` 비용을 시연에 반영했습니다. 변경 횟수는 비용에 영향을 주지 않습니다. 카탈로그의 기준 커밋은 `catalog.json`에 기록했습니다. 장비 인스턴스와 골드만 시연 데이터입니다. 실제 계정·Unity 저장 파일과 연결되지 않습니다. 시연 상태는 `hellscript.blacksmith.prototype.v1`이라는 전용 브라우저 저장 키를 사용합니다. 저장 공간을 사용할 수 없는 환경에서는 화면에 이를 알리고 해당 페이지의 메모리에서만 작동합니다.

상단에서 한국어·영어, 아이폰 가로·세로, PC 16:9·16:10·21:9와 현재 창 크기를 선택합니다. 모바일 세로에서 `다른 장비 선택`은 뒤로가기 형태의 버튼입니다. 누르면 옵션 변경이 가능한 장비 9개를 보여 주는 목록 화면이 왼쪽에서 오른쪽으로 들어옵니다. 장비를 고르면 옵션 선택 화면이 오른쪽에서 왼쪽으로 돌아옵니다. 장비 목록은 팝업이 아니며, 비활성 화면의 버튼은 조작할 수 없습니다. 가로·PC의 장비 선택, 옵션 선택, 후보 목록은 항상 1:1:1 너비로 표시합니다. 가능 옵션 팝업은 후보 수와 관계없이 게임 화면 전체를 채우며, 긴 목록은 본문 안에서만 스크롤합니다. 팝업 제목과 닫기 버튼, 하단 안내는 고정됩니다. 동작 줄이기 설정에서는 슬라이드를 생략합니다. `안내 및 시연 설정`에서 골드 0원, 1회 비용만 남은 상태, 300만 골드 상태를 시험하거나 처음부터 다시 시작할 수 있습니다.

아이폰 17 프로 맥스의 [Apple 공식 기술 사양](https://support.apple.com/ko-kr/125091)에 있는 1320 × 2868 해상도 비율을 440 × 956과 956 × 440으로 재현합니다. 기기 외곽선과 상단·하단 여백은 시연용입니다. CSS에는 실제 브라우저의 안전 영역도 적용합니다. PC 미리보기 크기는 1440 × 810, 1440 × 900, 1890 × 810입니다. `?live=1`은 미리보기 도구 없이 현재 브라우저 뷰포트를 직접 사용하고, `?view=portrait` 또는 `?view=landscape`는 해당 미리보기로 시작합니다.

2026-09-20 검증: Node 동작 검사 30/30개를 통과했습니다. 내장 브라우저에서 옵션 선택과 취소, 수동 골드 차감, 반복 비용 유지, 슬롯 고정, 저장 후 복구, 착용/인벤토리 전환, 옵션 없는 일반 장비, 잘못된 자동 횟수, 목표 도달 시 정지, Esc로 결과 선택을 건너뛸 수 없는 동작, 남은 횟수 재개, 마지막 회차의 계속 버튼 비활성화, 골드 소진과 시연 초기화를 직접 확인했습니다. 440 × 956 및 956 × 440 뷰포트와 PC 세 가지 비율에서 가로 넘침과 하단 실행 영역을 확인했습니다. 한국어·영어 표시를 확인했고 브라우저 오류·경고는 없었습니다. 물리적인 아이폰의 터치 입력과 Safari, Unity 런타임은 검증 범위에 포함하지 않습니다.

2026-09-21 수정 검증: 기존 Node 검사 30/30개를 다시 통과했습니다. 내장 브라우저에서 PC 세 가지 비율과 모바일 가로의 3열 균등 배치, 세로 장비 목록의 양방향 슬라이드, 변경 불가 장비 제외, 착용/인벤토리 필터, 장비 선택 후 복귀, 목록을 연 상태에서 가로 전환, 한국어·영어 표시를 확인했습니다. 후보 7종과 19종에서 팝업과 닫기 버튼의 위치·크기가 같았으며, 19종 목록을 끝까지 스크롤해도 팝업 영역은 유지됐습니다. 화면 전환은 골드를 차감하지 않습니다.

장비 카드 백분율 표시 검증: 기존 검사 30/30개를 통과했습니다. 내장 브라우저에서 저장된 행운의 적중 확률 +6.3%의 66%, 재사용 대기시간 감소 +5.61%의 87% 표시를 확인했습니다. PC, 모바일 세로 목록과 가로 화면에서 막대와 최소·최대 수치가 제거됐고, 한 줄 표시와 카드 너비를 유지했습니다. 검증 중 저장된 옵션과 보유 골드는 변경하지 않았습니다.

수치와 괄호 안 백분율은 한 묶음으로 표시하고, 일반적인 띄어쓰기 정도의 간격만 둡니다. PC·세로에서는 3px, 모바일 가로에서는 2.25px 간격으로 수치 바로 옆에 백분율이 붙는 것을 확인했습니다.

## English

Open `HELLSCRIPT-Blacksmith.html` directly in a browser. It is a standalone, offline-capable artifact containing its styles, scripts, catalog and vector artwork. Choose from ten demo items in equipped or inventory filters, choose an affix line, and inspect the legal weighted candidates and level-adjusted ranges. The first paid roll permanently binds that item’s reroll slot. Results replace the old affix immediately. Cost depends only on item level, never on previous attempts.

After the first paid reroll, the equipment card shows `Affix name +current value (range position%)` on one line, without a bar or minimum/maximum labels. Long affix names may truncate on narrow screens, with the full name available in a tooltip. Position uses the item's own level-adjusted bounds: `(value − minimum) / (maximum − minimum) × 100`, displayed to one decimal place. Attack Speed +7.76% in a 3–8% range therefore shows 95.2%. Minimum and maximum correspond to 0% and 100%. Selecting another item does not change what each card summarizes. Unmodified items have no reroll summary.

Automatic reroll accepts an integer limit from one through the affordable maximum, with one or more checked targets. Any matching type pauses immediately without further spending. The result dialog includes the value, its full range, spend and remaining budget. Stop keeps the result; continuing uses only the remaining attempts. A match on the final attempt still opens the dialog with Continue disabled. Leaving or reloading stops automation while retaining paid results and locked slots.

The source catalog, item-part restrictions, prefix/suffix and duplicate-group rules, loot-class weights, value/tier distributions and cost formula mirror the project sources identified in `catalog.json`. Inventory instances and currency are demo data, stored under a dedicated browser key with no account or Unity-save connection. This page is a design and behavior prototype, not a game integration. The guide includes reset and gold edge-case controls.

The toolbar provides Korean and English, portrait/landscape iPhone 17 Pro Max ratio previews, desktop 16:9/16:10/21:9, and fluid layout. Landscape and desktop use three equal-width columns. In portrait, the back-style Change equipment button reveals an equipment page sliding in from the left, containing only the nine enchantable demo items. Selecting one returns to the affix page from the right. Inactive pages cannot receive input; reduced-motion preferences disable the slide. The candidate dialog always fills the game screen, regardless of candidate count. Only its body scrolls; the heading, close button and footer remain fixed. Phone previews use 440 × 956 and 956 × 440 based on Apple’s published 1320 × 2868 display ratio; illustrative insets are not physical-device evidence. `?live=1` uses the actual viewport without the preview toolbar. `?view=portrait` and `?view=landscape` choose an initial preview mode.

Validation on 2026-09-20: 30/30 Node tests passed. Direct in-app-browser interaction verified selection/cancellation, spending, constant cost, permanent lock and reload, item filters, ordinary items without affixes, invalid automatic limits, immediate match pause, required result decisions, remaining-budget continuation, final-attempt handling, empty wallet, and reset. Checked both phone-sized viewports and all three desktop ratios, including scrolling and fixed action areas. Korean and English were inspected with no browser warnings or errors. Physical iPhone touch, Safari and Unity runtime validation remain outside this HTML prototype’s scope.

Validation on 2026-09-21: the existing 30/30 Node tests passed again. In-app-browser checks covered equal-width columns at all three desktop ratios and phone landscape; bidirectional portrait navigation; eligible-only equipment filters; selection and return; switching to landscape with the item page open; and Korean/English rendering. Candidate lists of 7 and 19 used identical dialog and close-button bounds. Scrolling the 19-candidate list to its end kept the dialog fixed. Navigation spent no gold.

Equipment-card percentage validation: the existing 30/30 tests passed. In-app-browser checks verified 66% for the saved Lucky Hit Chance +6.3% affix and 87% for Cooldown Reduction +5.61%. Desktop, portrait and landscape lists omitted bars and minimum/maximum labels, while staying on one line within card bounds. Existing affixes and gold were preserved during these checks.

The current value and parenthesized percentage form one unbroken group, separated only by a normal word-space-sized gap. Verified gaps are 3px on desktop/portrait and 2.25px in mobile landscape.

## 장착 슬롯 강화 / Equipment-slot enhancement

상단의 **장착 슬롯 강화**를 선택하거나 `?service=slots`로 시작합니다. [상세 기획서](SLOT-ENHANCEMENT.md)에 10개 부위의 1·25·50·75·100레벨 능력 배정, 레벨당 성장량, 최종 능력치와 재화·시간 계산을 정리했습니다. [99단계 계산 CSV](slot-enhancement-costs.csv)에는 각 강화의 필요 강화석·소요 시간과 누적값을 숫자로 제공합니다.

각 부위는 1레벨에서 시작합니다. 장비를 장착하지 않아도 적용되는 패시브를 선택해서 확인하고, 강화석을 사용해 작업을 시작할 수 있습니다. 동시에 2개 부위를 강화할 수 있으며 3·4·5번째 작업 칸을 다이아 10·50·250개로 차례로 엽니다. 최초 시연 재화는 강화석 10,000개와 다이아 500개입니다. 강화석은 시작 시 차감하고 능력치는 완료 시 적용합니다. 작업 중 같은 부위에 중복 작업을 등록할 수 없으며 100레벨을 넘길 수 없습니다.

**성장 상세**를 누르면 해당 부위의 전체 성장 계획을 봅니다. 메인 화면의 원형 1·25·50·75·100 단계 버튼은 해당 단계의 능력을 ‘모든 원소 저항 해금’처럼 버튼 근처의 작은 말풍선에 3초간 표시합니다. 다른 단계를 누르면 문구와 표시 시간을 새로 갱신합니다. 10부위 모두 해금되는 능력과 해금 레벨부터의 증가량을 표시하며, 표 오른쪽에 선택한 미리보기 레벨의 누적값을 표시합니다. 아직 해금되지 않은 능력은 잠김으로 표시합니다. 상단에는 첫 설명 문단만 남기고, 단계 버튼 아래의 중복 능력치·비용·시간·전체 계산표는 팝업에서 제거했습니다. 부위 선택·레벨 직접 입력·슬라이더·단계 버튼으로 계산을 확인할 수 있으며 실제 강화 상태는 바뀌지 않습니다. 팝업은 화면 전체를 사용하고 본문만 스크롤합니다. PC·가로는 3열을 같은 너비로 표시하고, 세로는 캐릭터와 능력치를 위아래로 배치하며 하단 강화 버튼을 고정합니다.

진행 중인 각 작업과 선택 부위의 하단에 **지금 완료** 버튼을 표시합니다. 비용은 클릭 시점의 `max(0, floor(남은 밀리초 / 60,000))` 다이아이며, 1분은 1개·59초 이하는 0개입니다. 버튼의 비용은 남은 시간에 맞춰 갱신되고, 잔액이 부족하면 비활성화됩니다. 해당 작업만 즉시 완료하고 레벨·패시브와 저장 상태를 갱신하며, 다른 작업은 계속 진행됩니다. 작업 칸 해제와 동일한 다이아 잔액을 사용합니다. 세로 화면 상단의 작업 칸 표시는 진행 중인 칸만 1.5초 주기로 금색 점멸을 반복하며, 작업 완료 시 멈춥니다.

성장 계획 팝업은 가로 화면에서 왼쪽에 단계별 능력 배정, 오른쪽에 레벨 슬라이더와 세로로 나열한 Lv.1·25·50·75·100 버튼을 같은 너비로 배치합니다. 모바일 가로에서는 여백과 행 높이를 줄여 한 화면에서 확인하도록 구성하고, 세로 화면은 기존의 위아래 배치를 유지합니다.

비용·시간의 구간은 도달 레벨 기준입니다. 1→2는 10개·5분, 99→100은 2,620개·2일 19시간 35분입니다. 한 부위 1→100의 합계는 **64,115개·54일 50분**, 10부위는 **641,150개·누적 작업 시간 540일 8시간 20분**입니다. 모든 작업을 쉬지 않고 이어갈 때 전체 완료까지 기본 2칸은 **270일 4시간 10분**, 처음부터 5칸이면 **108일 1시간 40분**이 걸립니다. 모든 스킬 +1은 주무기 100레벨에만 배치했습니다.

슬롯 강화는 `hellscript.blacksmith.slots.prototype.v1`에 별도로 저장합니다. 완료 시각을 저장하므로 새로고침·재접속 시 시간이 지난 작업을 완료 처리합니다. HTML 시연은 기기 시계를 사용하며 계정·서버·실제 결제에 연결되지 않습니다. 안내의 시연 전용 조작으로 현재 부위를 1·24·49·74·99레벨로 설정하거나, 진행 중 작업을 즉시 완료하고 재화 부족 상태를 확인할 수 있습니다. 초기화는 슬롯 강화 데이터만 바꾸며 기존 옵션 변경 아이템과 골드를 유지합니다. 캐릭터는 기존 `Barbarian_Reference_v1.png`를 원본 그대로 포함합니다.

2026-09-21 검증: 기존 옵션 변경 30개와 슬롯 강화 23개, 총 **53/53개**의 Node 검사를 통과했습니다. 비용은 구간별 등차수열로 별도 대조했고, 25·50·75·100레벨 해금과 10부위 최종 능력치, 동시 작업·중복 등록·재화 부족·저장 복구·100레벨 상한을 확인했습니다. 내장 브라우저에서는 첫 5분 작업의 실제 시간 경과 완료, 진행 중 새로고침, 2칸 제한, 10→50→250개 차감과 작업 칸 해제, 24→25와 99→100 전환, 재화 부족 차단, 성장 상세 10부위·5단계, 레벨 입력 검증, 미리보기 상태 분리와 전체 화면 팝업을 직접 확인했습니다. 440×956·956×440 뷰포트 및 PC 16:9·16:10·21:9 미리보기와 한국어·영어 표시를 점검했습니다. 슬롯 시연 초기화 후에도 기존 옵션 변경 골드 1,363,800과 고정 옵션 요약이 유지됐습니다. 모바일 실기기와 Unity 실행 검증은 이 HTML 작업에 포함하지 않습니다.

가로 팝업·지금 완료 추가 검증: 총 **60/60개**의 Node 검사를 통과했습니다. 내장 브라우저에서 모바일 가로 956×440의 10부위 영문 성장표와 PC 16:9·16:10·21:9 팝업에 가로·세로 스크롤이 없고, 좌우 너비가 같은 것을 확인했습니다. 세로 배치와 단계 버튼·숫자 입력·슬라이더도 확인했습니다. 별도 시연 저장 상태에서 4다이아 차감, 다른 작업 유지, 잔액 부족 차단, 카운트다운 비용 갱신을 확인했으며, 실제 남은 시간이 1분 미만이 되면 0다이아 버튼이 활성화되어 잔액 0에서도 완료됐습니다. 완료 후 새로고침에서도 레벨과 잔액이 유지됐습니다. 기존 시연 페이지의 재화·선택 부위·미리보기 레벨은 보존했습니다.

단계 안내·작업 상태 점멸 검증: 기존 **60/60개** 검사를 통과했습니다. 내장 브라우저에서 원형 단계 클릭이 팝업을 열지 않고 부위별 해금 능력을 표시하며, 3초 뒤 말풍선과 접근성 연결이 제거되는 것을 확인했습니다. 보조손 5단계 안내, 연속 클릭 시 문구 교체, 별도 성장 상세 버튼, 키보드 입력, 한국어·영어 및 세로·가로·PC 화면 경계를 확인했습니다. 작업 시작 시 해당 칸만 점멸하고, 지금 완료 후 점멸이 멈추는 것도 확인했습니다. 기존 시연의 선택 부위와 재화는 보존했습니다.

Select **Enhance equipment slots**, or start with `?service=slots`. The [bilingual design](SLOT-ENHANCEMENT.md) specifies all ten slots at levels 1/25/50/75/100, per-level growth, final stats and resource/time calculations. The [99-step CSV](slot-enhancement-costs.csv) contains numeric step and cumulative costs/durations.

Every slot starts at level 1 and grants permanent passives independent of equipped items. Two concurrent workstations are free; unlock the third, fourth and fifth in order for 10, 50 and 250 diamonds. Demo wallets start with 10,000 stones and 500 diamonds. Stones are spent at job start, while levels and passives apply at completion. Duplicate jobs for the same slot and levels above 100 are rejected.

**Growth details** opens all five unlocks and growth rules, with contributions at the selected preview level shown on the right. Future passives show Locked. The circular 1/25/50/75/100 markers in the main view show a small nearby unlock tooltip for three seconds, such as “Unlocks All resistance.” Clicking another marker replaces the text and restarts its duration. Only the first introductory paragraph remains; duplicate stats and cost/time/calculation tables below the milestone buttons are omitted from the popup. Change the previewed slot or level using the dropdown, number field, slider or milestone buttons without changing real progress. The detail overlay fills the screen and scrolls internally. Desktop/landscape use three equal columns; portrait stacks character and stats above a fixed upgrade action.

Every active workstation and the selected slot’s footer display **Finish now**. The click-time cost is `max(0, floor(remaining milliseconds / 60,000))` diamonds: 1 at exactly one minute, 0 below one minute. Prices update with the countdown, and insufficient funds disable the button. Only that job completes, updating its level, passives and saved state while other work continues. It uses the same diamond wallet as workstation unlocks. The portrait work-status indicators pulse gold every 1.5 seconds only while their jobs are active, and stop on completion.

On horizontal screens, the growth popup splits into equal-width columns: the milestone roadmap on the left, and the slider followed by vertical Lv.1/25/50/75/100 buttons on the right. Compact spacing fits phone landscape, while portrait keeps the stacked layout.

Brackets use destination levels. 1→2 costs 10 stones and 5 minutes; 99→100 costs 2,620 stones and 2d 19h 35m. One slot totals **64,115 stones and 54d 50m**; ten total **641,150 stones and 540d 8h 20m of summed work**. With no idle gaps, completing all ten takes **270d 4h 10m with two stations** or **108d 1h 40m with five stations unlocked from the beginning**. Only the main-hand capstone grants all skill levels +1.

Slot progress uses the separate `hellscript.blacksmith.slots.prototype.v1` key. Saved finish times allow reload catch-up using the device clock. This remains an HTML prototype, without account/server/payment integration. Demo controls set selected levels to 1/24/49/74/99, complete jobs immediately or adjust balances. Reset preserves the existing enchanting save. The character image embeds the existing `Barbarian_Reference_v1.png` unchanged.

Validation on 2026-09-21: **53/53 Node tests passed** (30 enchanting, 23 slot enhancement). Independent arithmetic-series checks covered costs; model tests covered all milestone boundaries and final allocations, concurrency, duplication, balances, persistence and the cap. In-app-browser interaction verified a real five-minute completion, active-job reload, the two-station limit, exact 10/50/250 unlock charges, 24→25 and 99→100 transitions, insufficient balances, all ten five-stage roadmaps, numeric validation, separate preview state and full-screen details. Checked 440×956 and 956×440 viewports, three desktop ratios and both languages. Slot reset preserved the existing 1,363,800 gold and locked-affix summaries. Physical mobile and Unity runtime were not part of this HTML validation.

Landscape popup / instant-completion validation: **60/60 Node tests passed**. In-app checks found no horizontal or vertical dialog scrolling for all ten English roadmaps at 956×440 and desktop 16:9, 16:10 and 21:9, with equal column widths. Portrait layout, presets, numeric input and slider remained functional. A separate demo origin verified a 4-diamond charge, independent parallel work, insufficient-funds blocking and countdown price updates. Once real remaining time dropped below a minute, the zero-cost button enabled and completed the job with a zero balance. Reload retained the completed level and balance. The original demo wallet, slot selection and preview level were preserved.

Milestone tooltip / work-indicator validation: the existing **60/60 tests passed**. In-app checks verified a nearby passive-unlock tooltip without opening a dialog, removal of the tooltip and its accessibility association after three seconds, all five off-hand milestones, replacement on successive clicks, the separate Growth details action, keyboard input, both languages and portrait/landscape/desktop bounds. Only the active station pulsed after starting work, and its pulse stopped after Finish now. The original slot selection and wallet were preserved.

## 장비 강화 / Equipment enhancement

상단 **장비 강화** 또는 `?service=gear`에서 착용 중·인벤토리의 장비 10개를 강화합니다. 옵션이 없는 일반 장비도 포함합니다. 장비별로 +0에서 +100까지 골드를 사용해 즉시 확정 강화하며, 아이템 레벨은 변하지 않습니다. 무기는 공격력 +2, 투구는 방어도 +2, 갑옷은 방어도 +3, 장갑은 공격 속도 +0.08%p, 장화는 이동 속도 +0.05%p, 허리띠는 최대 생명력 +8, 목걸이는 모든 원소 저항 +0.04%p, 반지는 치명타 확률 +0.02%p씩 매 단계 동일하게 증가합니다. 강화는 개별 장비에 귀속되고 착용 시 적용됩니다.

아이템 레벨 `L`의 첫 비용은 `100L + 10L²`골드입니다. 도달 강화 단계 `N`의 비용은 첫 비용에 `1 + 0.05(N−1) + 0.0025(N−1)²`를 곱하고 1골드 단위로 올림합니다. 레벨 30 장비는 +1에 12,000골드, +100 단계에 365,430골드, +0→100에 총 14,020,500골드가 듭니다. [상세 기획서와 100단계 비용표](GEAR-ENHANCEMENT.md)에 부위별 성장량, 시연 장비의 최종 능력과 아이템 레벨별 비용을 정리했습니다. 새 부위별 기본 능력과 강화 계수는 HTML 기획값이며 실제 Unity 밸런스와 분리됩니다.

PC·가로는 장비 목록, 현재/다음 능력치, 성장 미리보기를 같은 너비로 표시합니다. 세로는 목록에서 선택한 장비의 상세 화면으로 이동하고, 뒤로가기 버튼으로 목록에 돌아옵니다. 비용표 버튼과 팝업은 제거했습니다. 하단에 **+1레벨 강화·+10레벨 강화·최대 강화**를 표시하며 각 버튼에 도달 단계와 총비용을 함께 보여 줍니다. +10은 필요한 전체 골드가 있어야 실행하고, +100까지 10단계 미만이 남으면 남은 단계만 강화합니다. 최대 강화는 보유 골드로 가능한 연속 단계까지 올리고 잔액을 남깁니다. 모든 묶음 비용은 각 단계의 증가한 가격을 합산하며, 골드와 장비를 한 번에 갱신·저장합니다.

옵션 변경과 같은 장비·골드·저장 키를 사용합니다. 기존 저장은 각 장비의 강화 단계를 +0으로 보완하면서 골드·옵션·고정 위치·변경 기록을 보존합니다. 자동 옵션 변경 중에는 강화를 차단하고, 골드 부족이나 +100 도달 시 버튼을 비활성화합니다. 장비 강화 안내의 시연 설정으로 골드를 30,000,000 또는 0으로 정할 수 있습니다. 상단 초기화는 옵션 변경과 장비 강화의 장비·골드를 함께 초기화하며, 장착 슬롯 강화의 재화·진행도는 별도입니다.

Select **Enhance equipment** or use `?service=gear`. All ten owned items are eligible, including common equipment without affixes. Items enhance instantly and successfully from +0 to +100 using the shared enchanting wallet. Item level stays unchanged. Fixed gains per step are Attack +2 for weapons, Armor +2 for helms, Armor +3 for chest pieces, Attack speed +0.08 pp for gloves, Movement speed +0.05 pp for boots, Maximum life +8 for belts, All resistance +0.04 pp for amulets and Critical chance +0.02 pp for rings. Enhancement belongs to each item and applies while equipped.

The first cost for item level L is `100L + 10L²`. Target enhancement N costs that amount multiplied by `1 + 0.05(N−1) + 0.0025(N−1)²`, rounded up to whole gold. A level-30 item costs 12,000 gold at +1, 365,430 for the +100 step and 14,020,500 in total from +0 to +100. The [design and full cost schedule](GEAR-ENHANCEMENT.md) covers all eight types, final demo stats and item-level examples. New stat assignments and coefficients are HTML design values, separate from Unity balance.

Desktop and landscape show equal-width columns for inventory, current/next stats and growth preview. Portrait moves from an item list into details with back navigation. The cost-table button and dialog are removed. The fixed bottom actions offer +1, +10 and maximum affordable enhancement, with destination and total price shown on each. +10 requires the full batch cost and caps at +100; max buys consecutive affordable steps and retains the remainder. Each quote sums the increasing step prices, then the item and gold are updated and saved together.

Legacy saves migrate to +0 while preserving gold, affixes, reroll locks and history. Auto reroll blocks enhancements; insufficient gold and the cap disable the action. Guide controls set demo gold to 30,000,000 or zero. Reset affects enchanting and item enhancement together, while the separate equipment-slot save remains intact.

2026-09-21 장비 강화 검증: 기존 60개와 새 강화 검사 14개, 총 **74/74개**를 통과했습니다. 전체 1~100 아이템 레벨에서 단계별 비용 증가, 8종 고정 성장, 정확한 골드 차감·부족·상한, 기존 옵션 보존, 자동 변경과의 충돌 차단, 이전 저장의 +0 보완과 새 저장 복구를 확인했습니다. 별도 시연 주소의 내장 브라우저에서 무기를 직접 +100까지 강화했고, 최초 +1 차감 12,000골드와 이후 +2~100 차감 14,008,500골드, +100 능력 276.36 및 버튼 비활성화를 확인했습니다. 장갑의 2.96%→3.04%, 일반 장비 강화, 착용/인벤토리 각 5개 필터, 재화 부족과 새로고침 복구도 확인했습니다. 440×956·956×440 및 PC 세 비율, 한국어·영어와 100행 비용표 끝까지의 스크롤을 점검했습니다. 모바일 가로에서 10개 장비 모두 능력 상세·성장 미리보기에 세로 스크롤이나 가로 넘침이 없었으며, PC 3열의 너비도 동일했습니다. 브라우저 오류·경고는 없었습니다. 물리 기기와 Unity 검증은 이 HTML 시연 범위에 포함하지 않습니다.

Equipment validation on 2026-09-21: **74/74 tests passed** (60 existing, 14 new), covering increasing prices for item levels 1–100, eight linear stat curves, exact/insufficient balances, cap handling, preserved affixes, auto-reroll exclusion, legacy migration and reload. A separate in-app-browser demo origin enhanced the weapon to +100 through actual clicks: 12,000 gold for +1, another 14,008,500 for +2–100, final Attack 276.36 and a disabled cap button. Verified glove speed 2.96%→3.04%, common-item enhancement, two five-item filters, empty-wallet blocking and reload persistence. Inspected 440×956, 956×440, three desktop ratios, both languages and scrolling to cost-table row 100. All ten landscape item detail/preview panels fit without vertical scrolling or horizontal overflow; desktop columns remained equal. No browser warnings/errors. Physical devices and Unity runtime are outside this HTML validation.

묶음 강화 수정 검증: 총 **80/80개**의 검사를 통과했습니다. 10단계 묶음과 개별 10회의 결과 일치, 총액 부족 시 무차감, 최대 강화의 정확한 한도·잔액, +100까지 남은 단계만의 차감, 자동 변경 중 차단과 저장 복구를 확인했습니다. 별도 내장 브라우저 시연에서 +0→10에 155,550골드, 이어서 최대 강화로 +53까지 2,754,150골드를 사용하고 90,300골드를 남겼습니다. +97→100 묶음은 남은 세 단계의 1,076,820골드만 사용했습니다. 비용표 버튼·팝업이 제거됐고, 한국어·영어 및 세로·가로·PC 세 비율에서 세 버튼의 비용과 도달 단계가 넘치지 않는 것을 확인했습니다. 완료 알림은 버튼 위에 표시하며, 기존 시연의 보유 골드 1,296,900과 선택 장비·강화 상태는 유지했습니다.

Bulk-enhancement validation: **80/80 tests passed**, covering equivalence with individual steps, atomic rejection of unaffordable batches, exact affordable limits/remainders, cap truncation, auto-reroll blocking and persistence. Separate in-app-browser checks spent 155,550 gold for +0→10, then 2,754,150 to reach +53 with Max, retaining 90,300 gold. The +97→100 batch cost only the remaining three steps, 1,076,820 gold. The cost-table controls/dialog were removed. All three buttons fit in both languages, portrait, landscape and three desktop ratios, with completion notices above the controls. The original demo retained its 1,296,900 gold, selected item and enhancement progress.

## Build and check

```sh
node Prototypes/Blacksmith/build.cjs
node --test Prototypes/Blacksmith/engine.test.cjs Prototypes/Blacksmith/slot-engine.test.cjs Prototypes/Blacksmith/gear-engine.test.cjs
python3 -m http.server 4186 --bind 127.0.0.1 --directory Prototypes/Blacksmith
```

Open `http://127.0.0.1:4186/HELLSCRIPT-Blacksmith.html`. `page.html` is the build template; the finished downloadable file is `HELLSCRIPT-Blacksmith.html`. Changes to the source files require rebuilding and reloading the page. Building also regenerates both enhancement design reports and the slot cost CSV from their shared models.
