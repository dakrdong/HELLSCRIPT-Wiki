"""Read-only wiki catalog for shipped consumable boxes and rift first-clear packages."""
import json


def amount(box,stage):
    return {'rift-stones':2+stage//50,'ten-materials':20+stage//20,'fifty-stones':50+stage//2}.get(box.get('formula'),box['amount'])


def build(api):
    path='Assets/HELLSCRIPT/Resources/Data/RewardBoxes.json';data=json.loads(api.read(path));definitions={b['id']:b for b in data['boxes']}
    unlock_path='Assets/HELLSCRIPT/Resources/ContentUnlocks.json';unlocks=json.loads(api.read(unlock_path))['features']
    rows=[]
    for b in data['boxes']:
        fields={'영어 이름':b['nameEn'],'종류':b['kind'],'상자당 수량':b['amount'] if not b['formula'] else b['formula'],
                '장비 등급':b['rarity'],'부위':b['slot'],'보석 단계':b['tier'],'보석 종류':b['gemId'] or '선택 / Choice' if b['kind']=='gem' else '',
                '각성':b.get('awakened',False),'직업 세트 한정':b.get('setOnly',False),'룬 등급':b['grade'],'룬 크기':b['size'],
                '소유자':'계정 / Account','개봉':'GameStore.OpenRewardBox; 최대 10개 / Up to 10',
                '아이콘 원본':'Docs/Art/RewardBoxes/'+b['icon']+'.svg','이미지 제작':'코드로 작성한 벡터 원본 / Code-authored vectors'}
        rows.append(api.record(b['id'],b['nameKo'],b['kind'],'균열 최초 보상 등에 쓰는 소비형 지급 상자 / Consumable grant item',fields,path,
                              status='개발 브랜치 구현',image={'file':'RewardBoxes/'+b['icon']+'.png'},related=['reward-boxes','reward-boxes.en'],
                              refs=[api.source_ref('Assets/HELLSCRIPT/Runtime/Core/RewardBoxes.cs'),api.source_ref('Assets/HELLSCRIPT/Runtime/Core/GameStore.RewardBoxes.cs')]))
    stages=[]
    for stage in range(1,1001):
        grants=[r['grant'] for r in data['firstClearRules'] if stage%r['every']==0]+next((m['grants'] for m in data['milestones'] if m['stage']==stage),[])
        names=[definitions[g['boxId']]['nameKo']+' × '+str(g['count']) for g in grants]
        features=[f for f in unlocks if f['stage']==stage]
        fields={'균열 단계':stage,'콘텐츠 개방':' · '.join(f['name'] for f in features) or '추가 개방 없음 / No new service',
                '개방 조건':'계정 클리어 기록 확정 시 자동 개방; 상자 수령 불필요 / Automatic on confirmed account clear; box claim not required','상자':' / '.join(names),'상자 수':sum(g['count'] for g in grants),'수령 조건':'해당 단계의 정상 클리어 최단 기록 / Exact normal-clear record',
                '중복 방지':'계정당 단계별 1회 / Once per account per stage','기존 보상':'자동 골드·재료는 재지급하지 않음 / No duplicate automatic currency',
                '지급 데이터':grants,'상자별 내용물 수':[amount(definitions[g['boxId']],stage) for g in grants]}
        stages.append(api.record('rift-first-'+str(stage),'균열 '+str(stage)+'단계','최초 보상',' / '.join(names),fields,path,status='개발 브랜치 구현',related=['reward-boxes','rift-rewards-1000','rift-content-unlocks'],refs=[api.source_ref(unlock_path)]))
    return [api.db('reward-boxes','지급용 보상 상자 / Reward boxes','실제 소비형 상자 96종의 정의 / Runtime definitions of 96 consumable boxes.',rows),
            api.db('rift-first-boxes','균열 최초 지급 상자 / First-clear boxes','1~1000단계 계정 최초 보상의 실제 상자 묶음 / Account-once runtime packages for tiers 1–1000.',stages)]
