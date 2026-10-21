/* Proposed progression. Runtime effects, names, ranks and artwork stay source-owned. */
const label = (ko, en) => ({ ko, en });
const dependency = (id, sourceCue, ko, en) => ({ id, sourceCue, reason: label(ko, en) });
// Only actual effect targets/triggers are prerequisites. Shared combat states
// alone do not bind a skill. A skill never offers "this or that" prerequisites: when an effect reads
// several sources, one is required (the one already on its branch) and the others are listed in `optional`.
const dependencies = {
  W10: dependency('W09', '자신의 W09 잔여 출혈만', '찢어낸 상처가 남긴 출혈을 직접 정산하므로 해당 스킬의 활성화가 필요합니다.', 'Consumes the remaining bleed from Raking Wound, so that skill must be unlocked.'),
  WP02: dependency('W01', '회오리를 2초 유지하면', '회오리의 유지 비용을 줄이는 전용 강화입니다.', 'Specifically reduces the sustained cost of Whirlwind.'),
  WP03: dependency('W02', '도약 착지 후', '도약 내려찍기의 착지 직후 방어 효과를 더합니다.', 'Adds damage reduction after Leap Slam lands.'),
  WP10: dependency('W06', '전투 함성 시전 후', '전투 함성 시전 후 회복 효과를 더합니다.', 'Adds recovery after casting Battle Shout.'),
  WP12: dependency('W03', '다음 분쇄 일격의 비용', '기본 공격으로 준비한 효과가 분쇄 일격의 비용을 줄입니다.', 'Basic attacks prepare a cost reduction specifically for Crushing Blow.'),
  WP14: dependency('W04', '장착한 지면 강타의 남은 쿨타임', '지면 강타의 쿨타임을 직접 줄입니다. 제어 상태를 만드는 다른 스킬은 선행 조건으로 강제하지 않습니다.', 'Directly reduces Ground Slam cooldown. No particular source of crowd control is required to unlock it.'),
  WP16: dependency('W01', '회오리를 유지하면', '회오리를 유지하는 동안 적용되고, 보호막이 있으면 더 커집니다. 보호막은 스킬이나 장비 등으로 마련할 수 있어 철벽을 요구하지 않습니다.', 'Applies while sustaining Whirlwind and grows with a barrier. The barrier can come from a skill or equipment, so Iron Wall is not required.'),
  AP02: dependency('A04', '후퇴 도약 착지 후', '후퇴 도약의 착지 후 이동속도를 강화합니다.', 'Improves movement speed after Retreat Leap lands.'),
  AP03: dependency('A01', '관통 사격의 최대 대상', '관통 사격의 최대 적중 대상 수를 늘립니다.', 'Increases the target limit of Piercing Shot.'),
  AP14: dependency('A11', '자신의 살아 있는 미끼', '미끼 투영이 만든 살아 있는 미끼를 기준으로 피해가 증가합니다.', 'The damage bonus depends on the living decoy created by Decoy Projection.'),
  AP17: dependency('A15', '교차 쇠뇌의 교차 사격', '교차 쇠뇌의 교차 사격이 같은 사선의 두 번째 적을 관통하게 합니다.', 'Makes Crossfire Ballista crossfire pierce a second enemy on the same firing line.'),
  M11: dependency('M03', 'M03을 유효 시전할 때마다', '연쇄 번개를 시전해 축전 구체의 추가 폭발을 충전합니다.', 'Chain Lightning casts charge the additional explosion of Capacitor Orb.'),
  M12: dependency('M03', 'M03이 4m', '접지창으로 연쇄 번개의 경로를 중계하고 자원을 회수합니다. 최근 연쇄 번개 적중 대상을 직접 맞혀도 회수합니다.', 'Grounding Spear relays Chain Lightning paths and recovers resource. It also recovers resource on a direct hit against a recent Chain Lightning target.'),
  MP01: dependency('M01', '화염구 폭발', '화염구의 직접 폭발 피해를 강화합니다.', 'Improves the direct explosion damage of Fireball.'),
  MP02: dependency('M02', '눈보라의 둔화', '눈보라가 주는 둔화의 강도를 높입니다.', 'Increases the strength of the slow applied by Blizzard.'),
  MP03: dependency('M03', '연쇄 번개의 총 타격', '연쇄 번개의 총 타격 수를 늘립니다.', 'Increases the total number of Chain Lightning hits.'),
  MP12: dependency('M11', '축전 구체가 자연 만료하면', '축전 구체가 자연 만료될 때만 자원을 회복합니다.', 'Recovers resources specifically when Capacitor Orb expires naturally.')
};
Object.assign(dependencies, {
  WP08: dependency('W09','W09','찢어낸 상처의 출혈에 직접 반응합니다. 장비로 생기는 추가 출혈은 별도 효과이며, 트리는 기본 스킬 관계를 표시합니다.','Directly reads Raking Wound bleeding. Additional equipment bleeding is separate; the tree shows the base skill relationship.'),
  WP11: dependency('W02','도약 내려찍기','도약 내려찍기의 착지에 보호막을 더합니다. 전선 돌파의 착지에도 적용되지만 해금에는 도약 내려찍기만 필요합니다.','Adds a barrier on landing Leap Slam. It also applies to Front Break landings, but only Leap Slam is needed to unlock it.'),
  WP17: dependency('W03','분쇄 일격','분쇄 일격과 지면 강타의 피해를 강화합니다. 해금에는 분쇄 일격만 필요합니다.','Improves Crushing Blow and Ground Slam. Only Crushing Blow is needed to unlock it.'),
  WP18: dependency('W17','선조의 전쟁','선조의 전쟁 전용 패시브입니다. 선조의 실제 적중으로 자원을 회복합니다.','Dedicated to War of the Ancestors; actual ancestor hits restore resources.'),
  WP19: dependency('W18','거인의 심판','거인의 심판 전용 패시브입니다. 직접 맞힌 적 수에 따라 보호막을 얻습니다.',"Dedicated to Titan's Judgment; direct targets grant a barrier."),
  AP04: dependency('A03','직접 설치한 덫','맹독 덫의 속박 시간을 늘립니다. 후퇴 덫은 장비가 만드는 추가 효과이며 가시 덫의 속박에는 적용되지 않습니다.','Extends Venom Trap roots. Retreat traps require equipment; Briar Trap roots do not use this passive.'),
  AP11: dependency('A03','맹독 덫','직접 설치한 세 종류의 덫이 준비되면 발동합니다. 해금에는 맹독 덫만 필요합니다.','Triggers when any of the three directly placed traps arms. Only Venom Trap is needed to unlock it.'),
  AP12: dependency('A04','후퇴 도약','후퇴 도약의 남은 쿨타임을 줄입니다. 쿨타임을 줄이는 계기가 되는 덫은 어떤 종류든 상관없어 해금에는 후퇴 도약만 필요합니다.','Reduces Retreat Leap cooldown when any trap is placed. The trap can be of any kind, so only Retreat Leap is needed to unlock it.'),
  AP13: dependency('A08','관통 사격','관통 사격과 응시 사격의 피해를 강화합니다. 해금에는 응시 사격만 필요합니다.','Improves Piercing Shot and Patient Shot. Only Patient Shot is needed to unlock it.'),
  AP16: dependency('A12','연막 엄폐','연막 엄폐 또는 사냥 준비가 다음 유료 스킬의 비용 감소를 준비합니다. 해금에는 연막 엄폐만 필요합니다.','Smoke Cover or Hunt Preparation prepares the next paid skill cost reduction. Only Smoke Cover is needed to unlock it.'),
  AP18: dependency('A17','일제 소탕','일제 소탕 전용 패시브입니다. 첫 파동의 표식 대상에게 추가 피해를 줍니다.','Dedicated to Killing Rain; the first wave deals extra damage to a marked target.'),
  AP19: dependency('A18','잔상 행군의 지속시간','잔상 행군 전용 패시브입니다. 지속시간과 생성 가능한 총 잔상 수를 늘립니다.','Dedicated to Afterimage March; extends duration and the total number of movement afterimages.'),
  MP07: dependency('M07','연소','잿불창의 연소와 화염 장벽 안의 적에게 직접 반응합니다. 해금에는 잿불창만 필요합니다.','Directly checks Ember Lance burning or an enemy inside Firewall. Only Ember Lance is needed to unlock it.'),
  MP08: dependency('M08','M07','잿불창의 연소 또는 화염 장벽 안에 있는 적에게 받는 피해를 줄입니다. 해금에는 화염 장벽만 필요합니다.','Reduces damage from enemies burning from Ember Lance or standing in Firewall. Only Firewall is needed to unlock it.'),
  MP10: dependency('M09','M06','서리 폭발 또는 빙하창의 빙결·보스 제압 성공에 반응합니다. 해금에는 빙하창만 필요합니다.','Triggers on a successful freeze or boss control from Frost Nova or Glacial Lance. Only Glacial Lance is needed to unlock it.'),
  MP11: dependency('M12','다음 M03·M12의 직접 피해','연쇄 번개와 접지창의 다음 직접 피해를 강화합니다. 해금에는 접지창만 필요합니다.','Improves the next direct damage from Chain Lightning or Grounding Spear. Only Grounding Spear is needed to unlock it.'),
  MP18: dependency('M17','삼원소 붕괴','삼원소 붕괴 전용 패시브입니다. 세 번째 단계의 실제 적중으로 자원을 회복합니다.','Dedicated to Triune Collapse; a hit in the third stage restores resources.'),
  MP19: dependency('M18','현자의 화신','현자의 화신 전용 패시브입니다. 해당 궁극기의 보호막 생성량을 늘립니다.','Dedicated to Sage Incarnate; increases the barrier it creates.')
});
// Lineage: the one skill that precedes each skill in the tree, so every branch reads top to bottom like a
// Diablo skill tree. It is structural, not an effect source: `all` above remains the audited
// requirements, and a lineage parent never opens later than its child, so unlock levels are unchanged.
// A skill whose effect dependency is already its parent needs no entry.
const lineage = {
  // Warrior
  WP01:'W01', WP04:'WP01', WP06:'WP04', W09:'WP06', W13:'W10', WP16:'W13',
  W04:'W03', W08:'W04', WP11:'WP03', WP12:'W08', W11:'WP12', WP15:'W11', W14:'W11', WP14:'W14', WP17:'WP15',
  WP05:'W05', W06:'W05', W07:'W06', WP07:'W07', W12:'W07', WP09:'W12', WP13:'WP09', W16:'W12', W15:'WP13',
  // Ranger
  AP01:'A01', AP03:'AP01', AP06:'A02', A05:'AP06', AP07:'A05', A08:'AP07', AP13:'A08', AP09:'AP03', A07:'AP09', A14:'A07',
  AP08:'A03', AP10:'AP08', A09:'AP08', AP04:'AP10', AP11:'AP04', A10:'A09', AP15:'A10', A13:'AP15', A15:'A13',
  AP05:'A04', A06:'AP05', AP12:'A06', A11:'A06', A12:'A11', AP16:'A12', A16:'A12',
  // Mage
  MP06:'MP01', M07:'MP06', MP07:'M07', M08:'M07', MP08:'M08', MP16:'MP08', M15:'MP16',
  M06:'MP02', MP09:'M06', M09:'MP09', MP10:'M09', M10:'MP10', M11:'MP03', M12:'M11', MP11:'M12', M16:'M12',
  MP05:'M04', M05:'M04', MP04:'M05', MP13:'MP04', MP14:'MP05', M13:'MP14', M14:'MP13', MP15:'M13', MP17:'MP15'
};
const node = (id, level, branch) => {
  const effect = dependencies[id]?.id ? [dependencies[id].id] : [];
  return { id, level, branch,
    all: effect.map(id=>({id,rank:1})),
    oneOf: [],
    lineage: lineage[id] && !effect.includes(lineage[id]) ? lineage[id] : null };
};
// Socket grids: grid[lane][band] is a list of rows; a row names the skill in its left and right column
// ('.' is empty). A band-40 row holds one skill centred on the lane. tree-layout.cjs checks that every
// link runs top to bottom without crossing another skill.
const grids = {
  Warrior: [
    [['W01 .','WP01 WP02'], ['WP04 .','WP06 .'], ['W09 .','WP08 W10'], ['. W13','. WP16'], []],
    [['W02 W03','WP03 .'], ['. W04'], ['WP11 W08','. WP12'], ['. W11','WP15 W14','WP17 WP14'], ['W18','WP19']],
    [[], ['W05 .','. WP05'], ['W06 .','W07 WP10','. WP07'], ['W12 .','WP09 W16','WP13 .','W15 .'], ['W17','WP18']]
  ],
  Ranger: [
    [['A01 A02','AP01 AP06','AP03 .'], ['. A05','. AP07'], ['AP09 A08','A07 AP13'], ['A14 .'], ['A17','AP18']],
    [['A03 .','AP08 .'], [], ['AP10 A09','AP04 A10','AP11 AP15'], ['. A13','. A15','. AP17'], []],
    [['A04 .'], ['AP02 AP05'], ['. A06','AP12 .'], ['. A11','AP14 A12','A16 AP16'], ['A18','AP19']]
  ],
  Mage: [
    [['M01 .','MP01 .'], ['MP06 .'], ['M07 .','M08 MP07','MP08 .'], ['MP16 .','M15 .'], ['M17','MP18']],
    [['M02 M03','MP02 MP03'], [], ['M06 .','MP09 .','M09 .','MP10 .','M10 .'], ['. M11','MP12 M12','M16 MP11'], []],
    [['M04 .'], ['MP05 M05','. MP04','. MP13'], ['MP14 .'], ['M13 M14','MP15 .','MP17 .'], ['M18','MP19']]
  ]
};
module.exports = {
  version: 4,
  dependencies, status: 'html_design_proposal', levelCap: 40, pointsPerLevel: 1,
  ultimatePrerequisitePhase: 3,
  baseRankFree: true, requireEquippedPrerequisites: false,
  phases: [
    { from: 1, to: 9, name: label('전투의 기초', 'Foundations'), note: label('핵심 공격과 첫 연계', 'Core attacks and first synergies') },
    { from: 10, to: 19, name: label('생존과 운용', 'Survival'), note: label('이동 · 방어 · 자원 관리', 'Movement, defence and resource control') },
    { from: 20, to: 29, name: label('전투 방식 확립', 'Specialisation'), note: label('상태 부여 후 연계와 회수', 'Apply conditions, then combine and recover') },
    { from: 30, to: 39, name: label('빌드의 완성', 'Mastery'), note: label('복합 연계와 강화 패시브', 'Advanced combinations and supporting passives') },
    { from: 40, to: 40, name: label('궁극의 선택', 'Ascendancy'), note: label('궁극기 1개와 최종 패시브', 'One equipped ultimate and its final passive') }
  ],
  classes: [
    { id: 'Warrior', name: label('전사', 'Warrior'), subtitle: label('전장을 가르고, 끝까지 버틴다.', 'Break their lines. Hold your ground.'), accent: '#d2ab70', emblem: 'W', grid: grids.Warrior,
      branches: [label('회전과 출혈', 'Whirlwind & blood'), label('돌파와 제압', 'Impact & control'), label('수호와 함성', 'Guard & warcry')],
      nodes: [
        node('W01',1,0), node('W02',1,1), node('W03',1,1),
        node('W04',10,1), node('W05',15,2), node('W06',20,2),
        node('W07',22,2), node('W08',22,1), node('W09',26,0),
        node('W10',28,0), node('W11',30,1), node('W12',30,2),
        node('W13',34,0), node('W14',34,1), node('W15',38,2),
        node('W16',38,2), node('W17',40,2), node('W18',40,1),
        node('WP01',3,0), node('WP02',8,0), node('WP03',6,1),
        node('WP04',12,0), node('WP05',16,2), node('WP06',18,0),
        node('WP07',23,2), node('WP08',27,0), node('WP09',31,2),
        node('WP10',24,2), node('WP11',27,1), node('WP12',28,1),
        node('WP13',32,2), node('WP14',35,1),
        node('WP15',32,1), node('WP16',36,0),
        node('WP17',39,1), node('WP18',40,2), node('WP19',40,1)
      ],
      examples: [
        { name: label('출혈 회수', 'Blood recovery'), level: 30, targets: ['W09','W10','WP08','WP04'], equip: ['W01','W02','W09','W10','WP01','WP04','WP08'] },
        { name: label('거인의 제압', 'Titan control'), level: 40, targets: ['W18','WP14','WP17','WP18'], equip: ['W03','W04','W14','W15','W18','WP14','WP17','WP18'] }
      ]
    },
    { id: 'Ranger', name: label('궁수', 'Ranger'), subtitle: label('거리를 지배하고, 빈틈을 꿰뚫는다.', 'Own the distance. Find the opening.'), accent: '#a5bd82', emblem: 'R', grid: grids.Ranger,
      branches: [label('정밀 사격', 'Precision'), label('덫과 맹독', 'Traps & venom'), label('기동과 그림자', 'Mobility & shadow')],
      nodes: [
        node('A01',1,0), node('A02',1,0), node('A03',1,1),
        node('A04',5,2), node('A05',15,0), node('A06',20,2),
        node('A07',22,0), node('A08',22,0), node('A09',26,1),
        node('A10',28,1), node('A11',30,2), node('A12',32,2),
        node('A13',34,1), node('A14',34,0), node('A15',38,1),
        node('A16',38,2), node('A17',40,0), node('A18',40,2),
        node('AP01',3,0), node('AP02',11,2), node('AP03',6,0),
        node('AP04',27,1), node('AP05',12,2), node('AP06',8,0),
        node('AP07',16,0), node('AP08',9,1), node('AP09',21,0),
        node('AP10',23,1), node('AP11',27,1), node('AP12',29,2),
        node('AP13',24,0), node('AP14',31,2), node('AP15',29,1),
        node('AP16',33,2), node('AP17',39,1), node('AP18',40,0), node('AP19',40,2)
      ],
      examples: [
        { name: label('독과 서리', 'Venom & frost'), level: 30, targets: ['A10','AP15','AP11'], equip: ['A01','A03','A04','A10','AP08','AP11','AP15'] },
        { name: label('그림자 추격자', 'Shadow pursuer'), level: 40, targets: ['A18','A16','AP18'], equip: ['A01','A06','A12','A16','A18','AP01','AP16','AP18'] }
      ]
    },
    { id: 'Mage', name: label('마법사', 'Mage'), subtitle: label('원소를 엮어, 전장의 흐름을 바꾼다.', 'Weave the elements. Rewrite the battle.'), accent: '#a5a8df', emblem: 'M', grid: grids.Mage,
      branches: [label('화염과 원소 순환', 'Flame & convergence'), label('냉기와 번개', 'Frost & lightning'), label('비전과 결계', 'Arcane & wards')],
      nodes: [
        node('M01',1,0), node('M02',1,1), node('M03',1,1),
        node('M04',5,2), node('M05',15,2), node('M06',20,1),
        node('M07',22,0), node('M08',24,0), node('M09',26,1),
        node('M10',28,1), node('M11',30,1), node('M12',32,1),
        node('M13',34,2), node('M14',34,2), node('M15',38,0),
        node('M16',38,1), node('M17',40,0), node('M18',40,2),
        node('MP01',3,0), node('MP02',6,1), node('MP03',8,1),
        node('MP04',16,2), node('MP05',11,2), node('MP06',12,0),
        node('MP07',23,0), node('MP08',25,0), node('MP09',21,1),
        node('MP10',27,1), node('MP11',33,1),
        node('MP12',31,1), node('MP13',18,2), node('MP14',20,2),
        node('MP15',35,2), node('MP16',36,0),
        node('MP17',39,2), node('MP18',40,0), node('MP19',40,2)
      ],
      examples: [
        { name: label('축전 연계', 'Stored lightning'), level: 35, targets: ['M11','M12','MP11','MP12'], equip: ['M03','M05','M11','M12','MP03','MP11','MP12'] },
        { name: label('삼원소 순환', 'Triune cycle'), level: 40, targets: ['M17','MP16','MP18'], equip: ['M01','M02','M03','M15','M17','MP06','MP16','MP18'] }
      ]
    }
  ],
  rationale: {
    WP09: label('일반적인 방어 성공에 발동합니다. 무쇠 자세를 사용해야만 발동하는 효과가 아닙니다.', 'Triggers on a successful block; using Iron Stance is not necessary.'),
    AP15: label('중독과 둔화를 만드는 방법을 특정 스킬로 제한하지 않습니다.', 'The sources of poison and slow are not restricted to particular skills.'),
    MP06: label('서로 다른 두 속성의 피해에 적용됩니다. 사용할 원소 기술을 자유롭게 선택합니다.', 'Applies to damage from two different elements. You can freely choose the elemental skills.'),
    MP15: label('일반 액티브의 준비·집중에 적용됩니다. 마력 회수만을 위한 전용 강화가 아닙니다.', 'Applies while preparing or channelling normal actives; it is not specific to Mana Reclaim.'),
  }
};
