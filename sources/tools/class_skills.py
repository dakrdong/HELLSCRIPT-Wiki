#!/usr/bin/env python3
"""Validate and publish design-only skills; never writes Unity Resources or runtime definitions."""
import argparse,collections,hashlib,json,re,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[1]
DATA=ROOT/'Docs/Design/ClassSkills/catalog.json'
SLOTS={'ko':['무기','머리','몸통','손','발','허리','목걸이','반지'],'en':['Weapon','Head','Body','Hands','Feet','Waist','Amulet','Ring']}

def preserved_source_bytes(path, raw):
    # Slot growth deliberately raises the effective-rank ceiling. Keep the design-time
    # fingerprint, accepting only this specific integration without masking other edits.
    if path=='Assets/HELLSCRIPT/Runtime/Core/SkillEffects.cs':
        raw=raw.replace(b'Mathf.Clamp(ranks[index],1,SkillProgression.MaximumEffectiveRank)',
                        b'Mathf.Clamp(ranks[index],1,SkillProgression.MaximumRank)')
    elif path=='Assets/HELLSCRIPT/Runtime/Core/SkillProgression.cs':
        raw=raw.replace(b'MaximumRank=5, MaximumEffectiveRank=6;',b'MaximumRank=5;')
        raw=raw.replace(b'Math.Clamp(rank,1,MaximumEffectiveRank)',b'Math.Clamp(rank,1,MaximumRank)')
    return raw

def loadout_issues(c,hero,level,normal,passives,ultimates):
    by={s['id']:s for s in c['skills']};errors=[]
    for ids,kind,limit in [(normal,'active',4),(passives,'passive',3),(ultimates,'ultimate',1)]:
        if len(ids)>limit or len(set(ids))!=len(ids):errors.append('slot_count_or_duplicate')
        for sid in ids:
            s=by.get(sid)
            if s is None or s['heroClass']!=hero or s['kind']!=kind:errors.append('wrong_skill')
            elif s['unlockLevel'] is not None and s['unlockLevel']>level:errors.append('locked')
    if level>=40 and len(ultimates)!=1:errors.append('choose_exactly_one_ultimate')
    return errors

def validate(c):
    assert c['status']=='design_only' and c['runtimeEnabled'] is False
    assert c['quotaPerClass']==dict(normalActives=16,passives=19,ultimates=2,total=37)
    by={s['id']:s for s in c['skills']};eq={e['id']:e for e in c['equipment']};sources={s['id'] for s in c['sources']};links={s['setId']:s for s in c['setLinks']}
    assert len(by)==len(c['skills'])==111 and len(eq)==len(c['equipment'])==141
    for hero in c['classes']:
        assert all(hero[key]==value for key,value in c['quotaPerClass'].items())
        rows=[s for s in c['skills'] if s['heroClass']==hero['id']]
        assert collections.Counter(s['kind'] for s in rows)=={'active':16,'passive':19,'ultimate':2}
        assert sorted(s['displayOrder'] for s in rows)==list(range(1,38))
        ordered=sorted(rows,key=lambda s:s['displayOrder'])
        assert [s['kind'] for s in ordered[-2:]]==['ultimate','ultimate']
        assert [s['activeOrder'] for s in ordered[-2:]]==[17,18]
        assert len({s['exclusiveGroup'] for s in ordered[-2:]})==1
        assert all(s['unlockLevel']==40 and s['maxRank']==1 for s in ordered[-2:])
        gear=[e for e in c['equipment'] if e['heroClass']==hero['id']]
        assert collections.Counter(e['status'] for e in gear)=={'existing_runtime':40,'design_only':6}
        assert len([l for l in links.values() if l['heroClass']==hero['id'] and l['status']=='design_only'])==8
    for s in c['skills']:
        assert all(s[field].get(lang) for field in ['name','description','auto'] for lang in ['ko','en'])
        assert not re.search('[가-힣]',s['name']['en']+s['description']['en'])
        assert set(s['references'])<=sources
        assert all(x in by and by[x]['heroClass']==s['heroClass'] for x in s['anchors'])
        assert s['equipmentIds'] and set(s['equipmentIds'])<=set(eq)
        assert all(eq[x]['heroClass'] in ('Shared',s['heroClass']) for x in s['equipmentIds']), 'Wrong class equipment: '+s['id']
        assert s['exampleBuildIds'], 'No working build example: '+s['id']
        for bid in s['exampleBuildIds']:
            example=next(b for b in c['builds'] if b['id']==bid)
            assert s['id'] in example['normalSkills']+example['passives']+[example['ultimate']], 'Skill missing from build: '+s['id']
    for e in c['equipment']:
        assert 0<=e['slot']<=7
        for sid in e['directSkillIds']+e['supportSkillIds']+e['conditionEnablers']:
            assert sid in by and by[sid]['heroClass']==e['heroClass']
        if e['status']=='design_only':assert e['runtimeEnabled'] is False and e['proposedDropWeight'] is None
    originals=json.loads((ROOT/'Docs/Design/ClassSetReference/catalog.json').read_text())['sets']
    assert len(links)==30 and len(c['builds'])==30
    expected={b['effectId'] for s in originals for b in s['bonuses']}
    assert {b for s in links.values() if s['status']=='design_only' for b in s['bonusEffectIds']}==expected and len(expected)==60
    for b in c['builds']:
        assert not loadout_issues(c,b['heroClass'],40,b['normalSkills'],b['passives'],[b['ultimate']]),b['id']
        link=links[b['setId']]
        assert set(link['requiredSkillIds'])<=set(b['normalSkills'])
        slots=link['slots']+[eq[x]['slot'] for x in b['legendaryIds']]
        assert len(slots)==len(set(slots)) and len(slots)<=8,'Equipment conflict: '+b['id']
        assert len(b['setPieces'])==link['pieceCount']
        for eid in b['legendaryIds']:
            assert eq[eid]['heroClass'] in ('Shared',b['heroClass'])
            enabled=set(b['normalSkills'])|{b['ultimate']}
            required=set(eq[eid]['directSkillIds'])
            assert bool(required&{b['ultimate']}) if any(by[x]['kind']=='ultimate' for x in required) else required<=enabled
            assert not eq[eid]['conditionEnablers'] or set(eq[eid]['conditionEnablers'])&enabled
    # Design contract checks intentionally include forbidden imports and locked/dual ultimate requests.
    assert loadout_issues(c,'Warrior',40,['W01'],[],['W17','W18'])
    assert loadout_issues(c,'Warrior',40,['W01','W18'],[],['W17'])
    assert loadout_issues(c,'Warrior',39,['W01'],[],['W17'])
    assert loadout_issues(c,'Warrior',40,['W01'],[],[])
    assert loadout_issues(c,'Warrior',40,['A01'],[],['W17'])
    assert not loadout_issues(c,'Warrior',39,['W01'],[],[])
    import wiki
    # The original full-file fingerprints remain provenance. Runtime integration adds save fields
    # and a separate item lookup; freeze original constructor arguments instead of unrelated code.
    preservation=json.loads((ROOT/'Docs/Implementation/ClassSkills/legacy_definitions.json').read_text())
    extended={d['path'] for d in preservation['definitions']}
    for f in c['sourceFingerprints']:
        if f['path'] not in extended:
            source=preserved_source_bytes(f['path'],(ROOT/f['path']).read_bytes())
            assert hashlib.sha256(source).hexdigest()==f['sha256'],'Source changed: '+f['path']
    for definition in preservation['definitions']:
        actual=[a for a,line in wiki.constructors(definition['path'],definition['constructor'])]
        expected=definition['arguments']
        # The shared equipment release appended six bases; all original rows remain exact.
        if definition['constructor']=='ItemBaseDefinition':
            assert len({a[0] for a in actual})==len(actual),'Duplicate equipment base ID'
            actual=actual[:len(expected)]
        assert actual==expected,'Legacy definitions changed: '+definition['path']
    for a,line in wiki.constructors('Assets/HELLSCRIPT/Runtime/Core/GameCatalog.cs','SkillDefinition'):
        s=by[a[0]];p=s['parameters']
        assert s['name']['ko']==a[1] and s['description']['ko']==a[12] and s['unlockLevel']==a[4]
        assert [p[x] for x in ['cooldownSeconds','resourceCost','coefficient','rangeMeters','radiusMeters','durationSeconds']]==a[5:11]
    assert len([s for s in by.values() if s['status']=='existing_runtime'])==36
    return {'skills':111,'perClass':37,'ultimatesPerClass':2,'existingSkillsPreserved':36,'linkedLegendaryRecords':141,'newLegendaryConcepts':18,'sets':30,'newSetBonusTiers':60,'legalBuildExamples':30,'status':'design_only','runtimeEnabled':False,'ultimateContractNegativeCases':5}

def table(headers,rows):
    return '| '+' | '.join(headers)+' |\n|'+'|'.join('---' for _ in headers)+'|\n'+''.join('| '+' | '.join(str(v).replace('|',' / ').replace('\n',' ') for v in row)+' |\n' for row in rows)+'\n'

def page_name(hero,lang):return 'HELLSCRIPT_'+hero+'_Skills'+('.en' if lang=='en' else '')+'.md'
def equipment_link(e,lang):return e['name'][lang]+' (`'+e['id']+'`)'
def skill_list(ids,by,lang):return ', '.join(by[x]['name'][lang]+' (`'+x+'`)' for x in ids)

def class_page(c,hero,lang):
    ko=lang=='ko';by={s['id']:s for s in c['skills']};eq={e['id']:e for e in c['equipment']};sl={s['setId']:s for s in c['setLinks']};suffix='' if ko else '.en'
    rows=sorted([s for s in c['skills'] if s['heroClass']==hero['id']],key=lambda s:s['displayOrder'])
    out=f"# HELLSCRIPT {hero['name'][lang]} "+('스킬 37종\n\n' if ko else '— 37 skills\n\n')
    out+=('정리 기준일: ' if ko else 'Updated: ')+c['updatedOn']+'\n\n'
    out+=('상태: 확정 기획 · 능력·장비 구현의 검증 상태는 ' if ko else 'Status: approved design. Ability and equipment implementation status is tracked in ')+f"[{'구현 기록' if ko else 'the implementation record'}](../Implementation/Class_Skill_Runtime{suffix}.md). "+('일반 플레이 공개와 UI 연결은 대기 중입니다. 아래 수치는 1등급 기준입니다.\n\n' if ko else 'General release and UI integration are pending. Values below are for rank one.\n\n')
    out+=f"[{'공통 규칙·출처' if ko else 'Rules and sources'}](HELLSCRIPT_Class_Skills{suffix}.md) · [{'장비 대응표' if ko else 'Equipment matrix'}](HELLSCRIPT_Skill_Equipment{suffix}.md) · [{'English' if ko else '한국어'}]({page_name(hero['id'],'en' if ko else 'ko')})\n\n"
    out+=('일반 액티브 16개·패시브 19개·궁극기 2개입니다. 장착은 일반 액티브 최대 4개, 패시브 최대 3개, 궁극기 1개를 제안합니다. 기본 공격은 별도입니다. 전체 목록 36·37번인 궁극기는 같은 최종 단계에 놓고 둘 중 하나만 선택합니다.\n\n' if ko else '16 normal actives, 19 passives and 2 ultimates. Proposed loadout: up to 4 normal actives, up to 3 passives and exactly 1 ultimate after unlock. BASIC is separate. Entries 36 and 37 share the final tier and are mutually exclusive.\n\n')
    out+=('## 장비에서 출발한 빌드 예시\n\n' if ko else '## Equipment-led build examples\n\n')
    out+=('각 행은 해당 세트 전체를 착용하는 예시입니다. 제시한 전설은 세트와 부위가 겹치지 않습니다. 남은 부위에는 희귀 장비를 사용합니다. 각 빌드의 기술 순서를 사냥 칙령으로 설정하며, 자동 사용 조건은 아래 기술별 제안을 따릅니다.\n\n' if ko else 'Each example wears the full named set. Listed legendaries occupy different slots; use rares elsewhere. Configure the sequence in Hunt Edict, using the per-skill automatic-use suggestions below.\n\n')
    for b in [b for b in c['builds'] if b['heroClass']==hero['id']]:
        out+=f"### {sl[b['setId']]['name'][lang]} · `{b['setId']}`\n\n"
        out+=('일반 액티브: ' if ko else 'Normal actives: ')+skill_list(b['normalSkills'],by,lang)+'.\n\n'
        out+=('패시브: ' if ko else 'Passives: ')+skill_list(b['passives'],by,lang)+'. '+('선택 궁극기: ' if ko else 'Selected ultimate: ')+skill_list([b['ultimate']],by,lang)+'.\n\n'
        if b['legendaryIds']:out+=('함께 착용하는 전설: ' if ko else 'Compatible legendaries: ')+', '.join(equipment_link(eq[e],lang) for e in b['legendaryIds'])+'.\n\n'
        out+=b['loop'][lang]+'\n\n'
    for kind,title in [('active','일반 액티브 · 01–16' if ko else 'Normal actives · 01–16'),('passive','패시브 · 17–35' if ko else 'Passives · 17–35'),('ultimate','최종 단계: 궁극기 · 36–37 · 하나만 선택' if ko else 'Final tier: ultimates · 36–37 · choose one')]:
        out+='## '+title+'\n\n'
        for s in [s for s in rows if s['kind']==kind]:
            out+=f"### {s['displayOrder']:02}. {s['name'][lang]} · `{s['id']}`\n\n"
            state=('기존 구현 유지' if ko else 'Existing runtime retained') if s['status']=='existing_runtime' else ('신규 기획' if ko else 'New proposal')
            params=s['parameters'];level=s['unlockLevel'];out+=state
            if level is not None:out+=f" · Lv.{level}"
            if kind!='passive':out+=(f" · {'쿨타임' if ko else 'Cooldown'} {params['cooldownSeconds']}s · {'자원' if ko else 'Resource'} {params['resourceCost']}")
            if s['id']=='W01':out+=(' / 유료 틱 0.25초마다' if ko else ' per paid 0.25s tick')
            out+='\n\n'+s.get('developmentDescription',s['description'])[lang]+'\n\n'
            if kind!='passive':out+=('**자동 사용 제안:** ' if ko else '**Suggested automatic use:** ')+s['auto'][lang]+'\n\n'
            out+=('**장비 연결:** ' if ko else '**Equipment links:** ')+', '.join(equipment_link(eq[e],lang) for e in s['equipmentIds'][:5])
            if len(s['equipmentIds'])>5:out+=(f" 외 {len(s['equipmentIds'])-5}종" if ko else f" and {len(s['equipmentIds'])-5} more")
            out+='.\n\n'
            if s['anchors'] and s['status']=='design_only':out+=('**함께 운용하는 기술:** ' if ko else '**Companion skills:** ')+skill_list(s['anchors'],by,lang)+'.\n\n'
            reference=s['mechanicReference']
            if ko:
                reference={'Existing HELLSCRIPT definition':'기존 HELLSCRIPT 정의','D3 conditional passive structure; HELLSCRIPT equipment requirements':'디아블로 3의 조건부 패시브 구조와 HELLSCRIPT 장비 조건'}.get(reference,reference)
            out+=('**참고 구조:** ' if ko else '**Mechanic reference:** ')+reference+'.\n\n'
    return out

def equipment_page(c,lang):
    ko=lang=='ko';suffix='' if ko else '.en';by={s['id']:s for s in c['skills']}
    out=('# HELLSCRIPT 스킬·장비 대응표\n\n' if ko else '# HELLSCRIPT skill and equipment matrix\n\n')
    out+=('정리 기준일: ' if ko else 'Updated: ')+c['updatedOn']+'\n\n'
    out+=f"[{'설계 원칙' if ko else 'Design rules'}](HELLSCRIPT_Class_Skills{suffix}.md) · [JSON](ClassSkills/catalog.json)\n\n"
    out+=('기존 직업 전설 120종·공용 3종, 신규 전설 기획 18종, 기존 세트 6종·신규 세트 기획 24종을 연결합니다. **직접 연결**은 원래 지정한 스킬 ID입니다. **조건 제공**은 표식·보호막 등을 마련하는 보조 관계이며 기존 장비의 지정 기술을 교체하지 않습니다. 데이터의 supportSkillIds도 같은 의미입니다.\n\n' if ko else 'Links 120 existing class legendaries, 3 shared items, 18 new legendary concepts, 6 existing sets and 24 set concepts. **Direct** retains exact bound skill IDs. **Condition enablers** provide a mark, barrier or similar state without replacing an item’s bound skill. The data’s supportSkillIds has the same support-only meaning.\n\n')
    out+=('## 추가 전설 장비 18종\n\n새로운 두 기술을 실제로 연결하는 효과를 직업마다 6종씩 제안합니다. 현재 드롭·저장·전투 데이터에는 등록하지 않았고 가중치는 미정입니다. 궁극기 장비는 이미 선택한 한쪽 분기만 강화합니다.\n\n' if ko else '## 18 additional legendary concepts\n\nSix items per class link new skill pairs. These are not registered for drops, saves or combat; weights are undecided. Ultimate items enhance only the already-selected branch.\n\n')
    for hero in c['classes']:
        out+='### '+hero['name'][lang]+'\n\n'
        rows=[e for e in c['equipment'] if e['status']=='design_only' and e['heroClass']==hero['id']]
        out+=table(['ID','이름' if ko else 'Name','부위' if ko else 'Slot','연결' if ko else 'Skills','고유 효과' if ko else 'Unique power'],[(e['id'],e['name'][lang],SLOTS[lang][e['slot']],', '.join(e['directSkillIds']),e['description'][lang]) for e in rows])
    out+=('## 기존 전설 123종의 연결\n\n지정 스킬이 없는 항목은 원래의 기본 공격·물약·전역 조건을 유지합니다. 표의 조건 기술은 대안 목록이며 전부 장착할 필요는 없습니다. 개별 장비 효과 전문은 기존 전설 문서와 JSON에 보존합니다.\n\n' if ko else '## Existing 123 legendary links\n\nItems without bound skills retain their original BASIC, potion or global conditions. Enabler lists are alternatives, not requirements to equip every listed skill. Full effects remain in the existing legendary document and JSON.\n\n')
    out+=f"[{'기존 전설 효과 전문' if ko else 'Existing full legendary effects'}](HELLSCRIPT_Legendary_Class_Expansion{suffix}.md)\n\n"
    for hero in c['classes']+[{'id':'Shared','name':{'ko':'공용','en':'Shared'}}]:
        out+='### '+hero['name'][lang]+'\n\n'
        out+=table(['ID','이름' if ko else 'Name','부위' if ko else 'Slot','직접 연결' if ko else 'Direct','조건 제공' if ko else 'Enablers'],[(e['id'],e['name'][lang],SLOTS[lang][e['slot']],', '.join(e['directSkillIds']) or e.get('triggerSkill','BASIC / potion / global'),', '.join(e['conditionEnablers']) or '—') for e in c['equipment'] if e['status']=='existing_runtime' and e['heroClass']==hero['id']])
    out+=('## 세트 30종과 발동 단계\n\n신규 세트의 60단계 모두를 연결했습니다. 새 기술에 같은 원소·모양이 있어도 세트가 지정한 W01–W06·A01–A06·M01–M06을 자동으로 대신하지 않습니다.\n\n' if ko else '## 30 sets and bonus tiers\n\nAll 60 new-set bonus tiers are linked. Sharing an element or shape never automatically substitutes for the original skill IDs bound by a set.\n\n')
    out+=table(['세트' if ko else 'Set','구성 수' if ko else 'Pieces','필수 원 기술' if ko else 'Required original skills','효과 단계 ID' if ko else 'Effect IDs'],[(s['name'][lang]+' / '+s['setId'],s['pieceCount'],', '.join(s['requiredSkillIds']) or '—',', '.join(s['bonusEffectIds'])) for s in c['setLinks']])
    return out

def main():
    parser=argparse.ArgumentParser();parser.add_argument('command',choices=['build','check']);args=parser.parse_args();c=json.loads(DATA.read_text());report=validate(c)
    generated={}
    for lang in ['ko','en']:
        for hero in c['classes']:generated[ROOT/'Docs/Design'/page_name(hero['id'],lang)]=class_page(c,hero,lang)
        generated[ROOT/'Docs/Design'/('HELLSCRIPT_Skill_Equipment'+('.en' if lang=='en' else '')+'.md')]=equipment_page(c,lang)
    generated[DATA.parent/'validation.json']=json.dumps(report,ensure_ascii=False,indent=2)+'\n'
    for path,text in generated.items():
        text=text.rstrip()+'\n'
        if args.command=='build':path.write_text(text)
        else:assert path.read_text()==text,'Stale generated output: '+str(path)
    print(json.dumps(report,ensure_ascii=False))
if __name__=='__main__':main()
