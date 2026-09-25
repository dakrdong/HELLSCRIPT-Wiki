"""Versioned rift tuning contract shared by the API and the operations form.

Only released, client-supported knobs are accepted. This is configuration, never
an inventory grant endpoint or executable script transport.
"""
import copy
import hashlib
import json
import math
from pathlib import Path

SCHEMA_VERSION = 1
MAX_BODY = 2 * 1024 * 1024
MAX_OVERRIDES = 100
MAX_REWARDS = 1000
MAX_GRANTS = 20


class InvalidConfig(ValueError):
    def __init__(self, details):
        super().__init__('configuration_invalid')
        self.details = details


def canonical_json(value):
    return json.dumps(value, ensure_ascii=False, sort_keys=True, separators=(',', ':'), allow_nan=False)


def config_hash(value):
    return hashlib.sha256(canonical_json(value).encode('utf-8')).hexdigest()


# id, group, default, min, max, integer, Korean label, English label.
_FIELDS = [
    ('monsterHealth', 'monsters', 1, .1, 5, False, '일반 몬스터 생명력', 'Normal monster health'),
    ('monsterAttack', 'monsters', 1, .1, 5, False, '일반 몬스터 공격력', 'Normal monster attack'),
    ('monsterSpeed', 'monsters', 1, .5, 2, False, '일반 몬스터 이동 속도', 'Normal monster movement speed'),
    ('eliteHealth', 'monsters', 1, .1, 5, False, '정예 생명력', 'Elite health'),
    ('eliteAttack', 'monsters', 1, .1, 5, False, '정예 공격력', 'Elite attack'),
    ('eliteSpeed', 'monsters', 1, .5, 2, False, '정예 이동 속도', 'Elite movement speed'),
    ('bossHealth', 'boss', 1, .1, 5, False, '보스 생명력', 'Boss health'),
    ('bossAttack', 'boss', 1, .1, 5, False, '보스 공격력', 'Boss attack'),
    ('bossSpeed', 'boss', 1, .5, 2, False, '보스 이동 속도', 'Boss movement speed'),
    ('mapScale', 'map', 1, .95, 1.05, False, '맵 크기', 'Map size'),
    ('packSpread', 'map', 1, .8, 1.5, False, '군집 내부 간격', 'Spacing within packs'),
    ('normalDensity', 'map', 1, .75, 1.25, False, '일반 몬스터 밀도', 'Normal monster density'),
    ('timeLimitSeconds', 'map', 300, 60, 1800, False, '균열 제한 시간', 'Rift time limit'),
    ('normalEquipmentChance', 'equipment', .02, 0, 1, False, '일반 장비 드랍 확률', 'Normal equipment drop chance'),
    ('eliteEquipmentChance', 'equipment', 1, 0, 1, False, '정예 장비 드랍 확률', 'Elite equipment drop chance'),
    ('bossEquipmentChance', 'equipment', 1, 0, 1, False, '보스 장비 슬롯별 드랍 확률', 'Boss equipment chance per slot'),
    ('bossEquipmentCount', 'equipment', 3, 0, 20, True, '보스 장비 드랍 수', 'Boss equipment drop count'),
    ('goldMultiplier', 'economy', 1, 0, 10, False, '전투 골드 배율', 'Combat gold multiplier'),
    ('experienceMultiplier', 'economy', 1, 0, 10, False, '전투 경험치 배율', 'Combat experience multiplier'),
    ('clearGoldBase', 'clear', 800, 0, 1000000, True, '클리어 기본 골드', 'Clear base gold'),
    ('clearGoldPerStage', 'clear', 50, 0, 10000, True, '클리어 단계당 골드', 'Clear gold per stage'),
    ('clearMaterialsBase', 'clear', 5, 0, 1000000, True, '클리어 기본 재료', 'Clear base materials'),
    ('clearMaterialsStageDivisor', 'clear', 5, 1, 1000, True, '클리어 재료 증가 간격', 'Clear material stage divisor'),
    ('firstClearGoldBase', 'firstClear', 1000, 0, 1000000, True, '최초 클리어 추가 기본 골드', 'First-clear extra base gold'),
    ('firstClearGoldPerStage', 'firstClear', 100, 0, 10000, True, '최초 클리어 추가 단계당 골드', 'First-clear extra gold per stage'),
    ('firstClearMaterialsBase', 'firstClear', 10, 0, 1000000, True, '최초 클리어 추가 기본 재료', 'First-clear extra base materials'),
    ('firstClearMaterialsStageDivisor', 'firstClear', 5, 1, 1000, True, '최초 클리어 추가 재료 증가 간격', 'First-clear extra material stage divisor'),
    ('bossStoneMultiplier', 'resources', 1, 0, 10, False, '보스 강화석 배율', 'Boss enhancement stone multiplier'),
    ('normalGemChance', 'resources', .015, 0, 1, False, '일반 보석 드랍 확률', 'Normal gem drop chance'),
    ('eliteGemChance', 'resources', .08, 0, 1, False, '정예 보석 드랍 확률', 'Elite gem drop chance'),
    ('bossGemChance', 'resources', 1, 0, 1, False, '보스 보석 드랍 확률', 'Boss gem drop chance'),
    ('bossGemCount', 'resources', 2, 0, 20, True, '보스 보석 드랍 수', 'Boss gem drop count'),
    ('normalRuneChance', 'resources', .02, 0, 1, False, '일반 룬 드랍 확률', 'Normal rune drop chance'),
    ('eliteRuneChance', 'resources', .20, 0, 1, False, '정예 룬 드랍 확률', 'Elite rune drop chance'),
    ('bossRuneChance', 'resources', 1, 0, 1, False, '보스 룬 슬롯별 드랍 확률', 'Boss rune chance per slot'),
    ('bossRuneCount', 'resources', 4, 0, 20, True, '보스 룬 드랍 수', 'Boss rune drop count'),
    ('rarityGrowthHalfSpan', 'rarity', 49, 1, 1000, False, '전설 확률 성장 반감 구간', 'Legendary probability growth half-span'),
]

_RARITY_DEFAULTS = {
    'normalRarity': {'common': .55, 'magic': .25, 'rare': .19, 'legendary': .01, 'legendaryCap': .10},
    'eliteRarity': {'common': 0, 'magic': .60, 'rare': .35, 'legendary': .05, 'legendaryCap': .25},
    'bossRarity': {'common': 0, 'magic': 0, 'rare': .85, 'legendary': .15, 'legendaryCap': .45},
}


def default_config():
    rift = {row[0]: row[2] for row in _FIELDS}
    rift.update(copy.deepcopy(_RARITY_DEFAULTS))
    return {'rift': rift, 'stageOverrides': [], 'firstClearRewards': []}


def load_reward_catalog(path=None):
    if path is None:
        path = Path(__file__).parent / 'data' / 'RewardBoxes.json'
        if not path.is_file():
            path = Path(__file__).parent.parent / 'Assets/HELLSCRIPT/Resources/Data/RewardBoxes.json'
    raw = Path(path).read_bytes()
    catalog = json.loads(raw)
    if catalog.get('version') != 1 or not isinstance(catalog.get('boxes'), list):
        raise ValueError('Unsupported reward box catalog.')
    boxes = catalog['boxes']
    if not boxes or len({b['id'] for b in boxes}) != len(boxes):
        raise ValueError('Invalid reward box catalog.')
    return catalog


def first_clear_defaults(catalog):
    """Match RewardBoxCatalog.FirstClear: matching rules first, then milestone."""
    milestones = {row['stage']: row['grants'] for row in catalog['milestones']}
    result = []
    for stage in range(1, 1001):
        grants = [rule['grant'] for rule in catalog['firstClearRules'] if stage % rule['every'] == 0]
        grants += milestones.get(stage, [])
        if grants:
            result.append({'stage': stage, 'grants': copy.deepcopy(grants)})
    return result


def metadata(catalog):
    groups = [
        ('monsters', '몬스터', 'Monsters'), ('boss', '보스', 'Boss'),
        ('map', '맵과 군집', 'Map and packs'), ('equipment', '장비 드랍', 'Equipment drops'),
        ('rarity', '장비 등급', 'Equipment rarity'), ('resources', '보석·룬·강화석', 'Gems, runes and stones'),
        ('economy', '전투 수입', 'Combat earnings'), ('clear', '클리어 보상', 'Clear rewards'),
        ('firstClear', '최초 클리어 추가 보상', 'First-clear extra rewards'),
    ]
    fields = []
    for key, group, default, low, high, integer, ko, en in _FIELDS:
        probability = key.endswith('Chance')
        unit = '%' if probability else 's' if key == 'timeLimitSeconds' else 'stage' if key == 'rarityGrowthHalfSpan' else '×' if not integer else ''
        fields.append({'path': 'rift.' + key, 'group': group, 'type': 'integer' if integer else 'number',
                       'min': low, 'max': high, 'step': 1 if integer else .001 if probability else .01,
                       'default': default, 'unit': unit, 'displayScale': 100 if probability else 1,
                       'labelKo': ko, 'labelEn': en, 'descriptionKo': '', 'descriptionEn': ''})
    rarities = [('common', '일반', 'Common'), ('magic', '마법', 'Magic'), ('rare', '희귀', 'Rare'),
                ('legendary', '전설', 'Legendary'), ('legendaryCap', '전설 상한', 'Legendary cap')]
    for source, ko, en in [('normalRarity', '일반 몬스터', 'Normal'), ('eliteRarity', '정예', 'Elite'), ('bossRarity', '보스', 'Boss')]:
        for key, rarity_ko, rarity_en in rarities:
            fixed_zero = source == 'eliteRarity' and key == 'common' or source == 'bossRarity' and key in ('common', 'magic')
            fields.append({'path': 'rift.' + source + '.' + key, 'group': 'rarity', 'type': 'number',
                           'min': 0, 'max': 0 if fixed_zero else .9999 if key in ('legendary', 'legendaryCap') else 1, 'step': .0001, 'unit': '%', 'displayScale': 100,
                           'default': _RARITY_DEFAULTS[source][key], 'labelKo': ko + ' · ' + rarity_ko,
                           'labelEn': en + ' · ' + rarity_en,
                           'descriptionKo': '네 등급의 합은 100%여야 합니다. 전설 기본 확률과 상한은 일반≤정예≤보스 순서이며 상한은 100% 미만입니다. 전설은 30단계부터 나옵니다.',
                           'descriptionEn': 'The four rarities must total 100%. Legendary bases and caps must follow normal ≤ elite ≤ boss, with caps below 100%. Legendary drops start at stage 30.'})
    descriptions = {
        'mapScale': ('생성된 맵의 기하 구조 전체에 적용합니다. 새 균열 입장부터 반영됩니다.', 'Scales generated map geometry. Applies when entering a new rift.'),
        'packSpread': ('값이 클수록 같은 군집 안의 몬스터 사이 간격이 넓어집니다.', 'Larger values spread monsters farther apart within each pack.'),
        'normalDensity': ('일반 몬스터 배치량의 배율입니다. 1은 기존 밀도이며 정예·보스 수에는 적용하지 않습니다.', 'Multiplies normal monster placement. 1 preserves existing density; elite and boss counts are unaffected.'),
        'timeLimitSeconds': ('전투 시뮬레이션 시간 기준의 제한 시간입니다. 현실 시간이나 운영 설정 게시 시각과 다릅니다.', 'The limit in combat simulation seconds, independent of wall-clock time or publication time.'),
        'normalEquipmentChance': ('일반 몬스터를 처치할 때 장비가 발생할 확률입니다. 발생한 장비의 등급은 별도 등급 확률을 따릅니다.', 'Chance to generate equipment from a normal kill. Its rarity is rolled separately.'),
        'eliteEquipmentChance': ('정예를 처치할 때 장비가 발생할 확률입니다. 100%는 기존의 보장 드랍을 유지합니다.', 'Chance to generate equipment from an elite kill. 100% preserves its guaranteed drop.'),
        'bossEquipmentChance': ('보스 장비 슬롯마다 독립적으로 판정합니다. 실제 드랍 수는 슬롯 수 이하이며 100%이면 모든 슬롯이 지급됩니다.', 'Rolled independently for each boss equipment slot. At 100%, every configured slot drops.'),
        'bossEquipmentCount': ('보스 처치 시 판정할 장비 슬롯 수입니다. 각 슬롯에는 보스 장비 드랍 확률을 따로 적용합니다.', 'Number of equipment slots rolled on a boss kill. Each uses the boss equipment chance.'),
        'goldMultiplier': ('균열 처치·클리어·황금고블린·필드 상자 골드에 곱합니다. 1은 기존 수입이며 마을의 보상 상자 개봉·상점·오프라인 보급에는 적용하지 않습니다.', 'Multiplies rift kill, clear, gold-goblin and field-chest gold. 1 preserves earnings. Reward-box opening, shops and offline supplies are unaffected.'),
        'experienceMultiplier': ('일반·정예 처치와 보스 클리어 경험치에 곱합니다. 1은 기존 경험치이며 훈련·상자 개봉·오프라인 보급에는 적용하지 않습니다.', 'Multiplies normal/elite kill and boss-clear experience. 1 preserves existing XP; training, box opening and offline supplies are unaffected.'),
        'clearGoldBase': ('클리어 골드는 기본 골드 + 균열 단계 × 단계당 골드입니다. 전투 골드 배율도 적용합니다.', 'Clear gold is base gold + stage × gold per stage, then the combat gold multiplier is applied.'),
        'clearGoldPerStage': ('클리어 골드에 균열 단계 × 이 값을 더합니다. 전투 골드 배율도 적용합니다.', 'Adds stage × this value to clear gold before the combat gold multiplier.'),
        'clearMaterialsBase': ('클리어 재료는 기본 재료 + floor(균열 단계 / 재료 증가 간격)입니다.', 'Clear materials equal base materials + floor(stage / material stage divisor).'),
        'clearMaterialsStageDivisor': ('기본 재료 + floor(균열 단계 / 증가 간격)으로 계산합니다.', 'Calculated as base materials + floor(stage / divisor).'),
        'firstClearGoldBase': ('캐릭터가 최고 클리어 단계를 갱신하면 기본 골드 + 단계 × 단계당 골드를 추가합니다. 계정 최초 보상 상자와 별개이며 전투 골드 배율도 적용합니다.', 'Adds base gold + stage × gold per stage when the character improves its highest clear. Separate from account first-clear boxes; combat gold multiplier applies.'),
        'firstClearGoldPerStage': ('캐릭터 최고 단계 갱신의 추가 골드에 단계 × 이 값을 더합니다. 전투 골드 배율도 적용합니다.', 'Adds stage × this value to the character highest-clear gold bonus before its combat gold multiplier.'),
        'firstClearMaterialsBase': ('캐릭터 최고 단계 갱신 시 기본 재료 + floor(단계 / 증가 간격)을 추가합니다. 계정 최초 보상 상자와 별개입니다.', 'Adds base materials + floor(stage / divisor) when the character improves its highest clear, separately from account first-clear boxes.'),
        'firstClearMaterialsStageDivisor': ('추가 기본 재료 + floor(균열 단계 / 증가 간격)으로 계산합니다.', 'Calculated as extra base materials + floor(stage / divisor).'),
        'bossStoneMultiplier': ('기존 단계별 보스 강화석 수에 곱합니다. 1은 기존 보상, 0은 보스 강화석 지급 없음입니다.', 'Multiplies the existing stage-based boss stone reward. 1 preserves it; 0 disables boss stones.'),
        'normalGemChance': ('일반 몬스터 처치 시 보석을 드랍할 확률입니다. 보석 종류와 단계의 기존 규칙은 유지합니다.', 'Gem drop chance on a normal kill. Existing gem type and tier rules remain in effect.'),
        'eliteGemChance': ('정예 처치 시 보석을 드랍할 확률입니다. 보석 종류와 단계의 기존 규칙은 유지합니다.', 'Gem drop chance on an elite kill. Existing gem type and tier rules remain in effect.'),
        'bossGemChance': ('보스 보석 슬롯마다 독립적으로 판정합니다. 100%이면 설정한 보석 슬롯을 모두 지급합니다.', 'Rolled independently per boss gem slot. At 100%, every configured gem slot drops.'),
        'bossGemCount': ('보스 처치 시 판정할 보석 슬롯 수입니다. 각 슬롯에 보스 보석 드랍 확률을 적용합니다.', 'Number of gem slots rolled on a boss kill. Each uses the boss gem chance.'),
        'normalRuneChance': ('일반 몬스터 처치 시 룬을 드랍할 확률입니다. 룬 등급과 모양의 기존 규칙은 유지합니다.', 'Rune drop chance on a normal kill. Existing rune grade and shape rules remain in effect.'),
        'eliteRuneChance': ('정예 처치 시 룬을 드랍할 확률입니다. 룬 등급과 모양의 기존 규칙은 유지합니다.', 'Rune drop chance on an elite kill. Existing rune grade and shape rules remain in effect.'),
        'bossRuneChance': ('보스 룬 슬롯마다 독립적으로 판정합니다. 100%이면 설정한 룬 슬롯을 모두 지급합니다.', 'Rolled independently per boss rune slot. At 100%, every configured rune slot drops.'),
        'bossRuneCount': ('보스 처치 시 판정할 룬 슬롯 수입니다. 각 슬롯에 보스 룬 드랍 확률을 적용합니다.', 'Number of rune slots rolled on a boss kill. Each uses the boss rune chance.'),
        'rarityGrowthHalfSpan': ('1단계에서 이 단계 수만큼 올라가면 전설 확률이 기본값과 상한의 중간에 도달합니다. 값이 클수록 성장이 느리며 30단계 전설 제한은 유지합니다.', 'After this many stages beyond stage 1, legendary chance reaches halfway between its base and cap. Larger values grow more slowly; the stage-30 gate remains.'),
    }
    for field in fields:
        key = field['path'].split('.')[-1]
        if key in descriptions:
            field['descriptionKo'], field['descriptionEn'] = descriptions[key]
        elif key.endswith(('Health', 'Attack', 'Speed')):
            field['descriptionKo'] = '해당 몬스터 종류의 기존 단계별 수치에만 곱합니다. 1은 기존 수치이며 다른 종류의 배율과 중첩하지 않습니다.'
            field['descriptionEn'] = 'Multiplies this enemy category’s existing stage-based stat. 1 preserves it; multipliers from other categories do not stack.'
    return {'schemaVersion': SCHEMA_VERSION,
            'groups': [{'id': i, 'labelKo': ko, 'labelEn': en, 'descriptionKo': '', 'descriptionEn': ''} for i, ko, en in groups],
            'fields': fields, 'templates': {'rift': default_config()['rift']},
            'stage': {'min': 1, 'max': 1000, 'maxOverrides': MAX_OVERRIDES, 'maxFirstClearRewards': MAX_REWARDS},
            'reward': {'minCount': 1, 'maxCount': 100, 'minQuality': 0, 'maxQuality': 10000, 'maxGrants': MAX_GRANTS},
            'firstClearDefaults': first_clear_defaults(catalog),
            'rewardBoxes': [{'id': b['id'], 'nameKo': b['nameKo'], 'nameEn': b['nameEn'], 'kind': b['kind'],
                             'minimumStage': 30 if b['kind'] == 'equipment' and b['rarity'] == 3 else 1,
                             'qualityMin': 0, 'qualityMax': 10000} for b in catalog['boxes']],
            'rewardCatalogVersion': catalog['version'], 'rewardCatalogHash': config_hash(catalog)}


def validate_config(config, catalog):
    errors = []

    def error(path, code, ko, en):
        if len(errors) < 100:
            errors.append({'path': path, 'code': code, 'messageKo': ko, 'messageEn': en})

    def shape(value, keys, path):
        if not isinstance(value, dict):
            error(path, 'object', '설정 객체가 필요합니다.', 'A configuration object is required.')
            return False
        if set(value) != set(keys):
            error(path, 'fields', '필수 항목이 빠졌거나 지원하지 않는 항목이 있습니다.', 'Required fields are missing or unsupported fields are present.')
            return False
        return True

    def number(value, low, high, integer, path):
        if type(value) not in ((int,) if integer else (int, float)) or not low <= value <= high or not math.isfinite(value):
            error(path, 'range', f'{low}~{high} 범위의 ' + ('정수' if integer else '숫자') + '를 입력하세요.',
                  f'Enter an {"integer" if integer else "number"} between {low} and {high}.')
            return False
        return True

    def rift(value, path):
        if not shape(value, [f[0] for f in _FIELDS] + list(_RARITY_DEFAULTS), path):
            return
        for key, _, _, low, high, integer, _, _ in _FIELDS:
            number(value[key], low, high, integer, path + '.' + key)
        valid_profiles = True
        for source, keys in _RARITY_DEFAULTS.items():
            p = path + '.' + source
            profile = value[source]
            if not shape(profile, keys, p):
                valid_profiles = False
                continue
            valid = [number(profile[k], 0, .9999 if k in ('legendary', 'legendaryCap') else 1, False, p + '.' + k) for k in keys]
            if not all(valid):
                valid_profiles = False
                continue
            if abs(sum(profile[k] for k in ('common', 'magic', 'rare', 'legendary')) - 1) > 1e-12:
                error(p, 'probability_sum', '등급 확률의 합은 100%여야 합니다.', 'Rarity probabilities must total 100%.')
            if profile['legendary'] > profile['legendaryCap']:
                error(p + '.legendaryCap', 'legendary_cap', '전설 상한은 기본 전설 확률 이상이어야 합니다.', 'The legendary cap must not be lower than its base probability.')
            if source in ('eliteRarity', 'bossRarity') and profile['common'] != 0 or source == 'bossRarity' and profile['magic'] != 0:
                error(p, 'rarity_floor', '정예는 일반 장비를, 보스는 일반·마법 장비를 드랍할 수 없습니다.', 'Elites cannot drop common equipment; bosses cannot drop common or magic equipment.')
        if valid_profiles:
            normal, elite, boss = [value[source] for source in _RARITY_DEFAULTS]
            if any(not normal[key] <= elite[key] <= boss[key] for key in ('legendary', 'legendaryCap')):
                error(path, 'rarity_order', '전설 기본 확률과 상한은 일반≤정예≤보스 순서여야 합니다.', 'Legendary bases and caps must follow normal ≤ elite ≤ boss.')

    if not shape(config, ('rift', 'stageOverrides', 'firstClearRewards'), ''):
        raise InvalidConfig(errors)
    rift(config['rift'], 'rift')
    overrides = config['stageOverrides']
    if not isinstance(overrides, list) or len(overrides) > MAX_OVERRIDES:
        error('stageOverrides', 'budget', '단계 구간은 최대 100개입니다.', 'At most 100 stage ranges are supported.')
    else:
        spans = []
        for index, override in enumerate(overrides):
            path = f'stageOverrides[{index}]'
            if not shape(override, ('fromStage', 'toStage', 'rift'), path):
                continue
            a = number(override['fromStage'], 1, 1000, True, path + '.fromStage')
            b = number(override['toStage'], 1, 1000, True, path + '.toStage')
            if a and b:
                start, end = override['fromStage'], override['toStage']
                if start > end:
                    error(path, 'stage_order', '시작 단계는 종료 단계 이하여야 합니다.', 'The start stage must not exceed the end stage.')
                elif any(start <= previous_end and end >= previous_start for previous_start, previous_end in spans):
                    error(path, 'stage_overlap', '단계 구간은 서로 겹칠 수 없습니다.', 'Stage ranges must not overlap.')
                spans.append((start, end))
            rift(override['rift'], path + '.rift')
    rewards = config['firstClearRewards']
    boxes = {box['id']: box for box in catalog['boxes']}
    if not isinstance(rewards, list) or len(rewards) > MAX_REWARDS:
        error('firstClearRewards', 'budget', '최초 보상 단계는 최대 1,000개입니다.', 'At most 1,000 first-clear reward stages are supported.')
    else:
        stages = set()
        for index, row in enumerate(rewards):
            path = f'firstClearRewards[{index}]'
            if not shape(row, ('stage', 'grants'), path):
                continue
            valid_stage = number(row['stage'], 1, 1000, True, path + '.stage')
            if valid_stage:
                if row['stage'] in stages:
                    error(path + '.stage', 'duplicate_stage', '같은 단계의 최초 보상을 중복 지정할 수 없습니다.', 'A first-clear reward stage must be unique.')
                stages.add(row['stage'])
            if not isinstance(row['grants'], list) or len(row['grants']) > MAX_GRANTS:
                error(path + '.grants', 'budget', '단계별 보상 항목은 최대 20개입니다.', 'At most 20 grants per stage are supported.')
                continue
            for grant_index, grant in enumerate(row['grants']):
                p = path + f'.grants[{grant_index}]'
                if not shape(grant, ('boxId', 'count', 'minimumQuality'), p):
                    continue
                box_id = grant['boxId']
                if not isinstance(box_id, str) or box_id not in boxes:
                    error(p + '.boxId', 'box_id', '지원하는 보상 상자를 선택하세요.', 'Select a supported reward box.')
                elif valid_stage and row['stage'] < 30 and boxes[box_id]['kind'] == 'equipment' and boxes[box_id]['rarity'] == 3:
                    error(p + '.boxId', 'rarity_gate', '전설 장비 상자는 30단계부터 지급할 수 있습니다.', 'Legendary equipment boxes require stage 30 or higher.')
                number(grant['count'], 1, 100, True, p + '.count')
                number(grant['minimumQuality'], 0, 10000, True, p + '.minimumQuality')
    if errors:
        raise InvalidConfig(errors)
    return json.loads(canonical_json(config))
