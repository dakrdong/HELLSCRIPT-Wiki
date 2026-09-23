const test=require('node:test'),assert=require('node:assert/strict');
const data=require('./catalog.json'),E=require('./engine.js');
const source=require('../../Assets/HELLSCRIPT/Resources/Data/ClassSkills.json');
function openAll(classId,level=40){let state=E.empty(data,classId,level);for(const n of E.context(data,classId).nodes)if(n.level<=level)state=E.applyPath(data,state,n.id);return state;}
for(const hero of data.classes){
  test(hero.id+': all 37 skills are individually affordable at their unlock level',()=>{
    assert.equal(hero.nodes.length,37);
    for(const n of hero.nodes){const plan=E.pathPlan(data,E.empty(data,hero.id,n.level),n.id);assert.equal(plan.ok,true,n.id);assert.equal(plan.cost,0,n.id+' must not force upgrade spending');assert.equal(E.evaluate(data,plan.state).available[n.id],true,n.id);}
  });
  test(hero.id+': all branches are jointly reachable with 39 points',()=>{const state=openAll(hero.id),v=E.evaluate(data,state);assert.equal(v.unlocked,37);assert.ok(v.spent<=39);});
  test(hero.id+': each level 1–40 retains a valid and reachable progression',()=>{
    for(let level=1;level<=40;level++){
      const state=openAll(hero.id,level),v=E.evaluate(data,state);
      assert.equal(v.unlocked,hero.nodes.filter(n=>n.level<=level).length,'level '+level);
      assert.ok(v.remaining>=0);assert.equal(v.budget,level-1);
      for(const n of hero.nodes)if(v.available[n.id]){assert.ok(n.all.every(r=>v.ranks[r.id]>=r.rank));if(n.kind==='ultimate')assert.ok(v.ultimateReady);}
    }
  });
  test(hero.id+': names and rank-one effects are copied exactly from runtime data',()=>{
    for(const n of hero.nodes){const s=source.skills.find(s=>s.id===n.id);assert.equal(n.name.ko,s.name);assert.equal(n.name.en,s.nameEn);assert.equal(n.description.ko,s.description);assert.equal(n.description.en,s.descriptionEn);assert.equal(n.maxRank,s.maxRank);}
  });
  test(hero.id+': each ultimate support requires its matching ultimate activation at level 40',()=>{
    const n=hero.nodes.find(n=>n.id.endsWith('P18'));assert.deepEqual(n.all,[{id:n.id[0]+'17',rank:1}]);
    assert.equal(E.evaluate(data,E.empty(data,hero.id,39)).available[n.id],false);
    const state=E.applyPath(data,E.empty(data,hero.id,40),n.id),v=E.evaluate(data,state);
    assert.equal(v.available[n.id],true);assert.deepEqual(state.stack,[]);assert.deepEqual(state.equipped,[]);
  });
  test(hero.id+': both ultimates require level 40 and any unlocked skill in the preceding stage',()=>{
    const candidates=hero.nodes.filter(n=>n.level>=30&&n.level<=39),ultimates=hero.nodes.filter(n=>n.kind==='ultimate');
    assert.deepEqual(E.context(data,hero.id).ultimateCandidates.map(n=>n.id),candidates.map(n=>n.id));
    assert.ok(candidates.some(n=>n.kind==='active'));assert.ok(candidates.some(n=>n.kind==='passive'));
    assert.equal(new Set(candidates.map(n=>n.branch)).size,3);
    // Isolate each possible witness: base ranks normally unlock several peers together.
    for(const witness of [null,...candidates]){
      const fixture=E.clone(data),root=hero.nodes[0].id;
      for(const n of fixture.classes.find(c=>c.id===hero.id).nodes)if(candidates.some(c=>c.id===n.id)){n.all=n.id===witness?.id?[]:[{id:root,rank:5}];if(n.id===witness?.id)n.oneOf=[];}
      const state=E.empty(fixture,hero.id,40),v=E.evaluate(fixture,state);
      assert.equal(candidates.filter(n=>v.available[n.id]).length,witness?1:0);
      for(const ultimate of ultimates){
        assert.deepEqual(ultimate.all,[]);assert.equal(v.available[ultimate.id],Boolean(witness),ultimate.id+' / '+witness?.id);
        assert.equal(E.evaluate(fixture,{...state,level:39}).available[ultimate.id],false);
        if(witness){assert.equal(E.pathPlan(fixture,state,ultimate.id).cost,0);assert.deepEqual(E.equip(fixture,state,ultimate.id).equipped,[ultimate.id]);}
        else assert.throws(()=>E.equip(fixture,state,ultimate.id),/LOCKED/);
      }
      if(!witness)assert.equal(v.available[hero.nodes.find(n=>n.id.endsWith('P18')).id],false,'a level-40 passive alone must not count as the previous stage');
    }
  });
  test(hero.id+': suggested loadouts obey points, prerequisites and slot limits',()=>{
    hero.examples.forEach((preset,index)=>{const state=E.example(data,hero.id,index),v=E.evaluate(data,state);assert.equal(state.level,preset.level);assert.deepEqual(state.equipped,preset.equip);assert.ok(v.remaining>=0);for(const id of state.equipped)assert.ok(v.available[id]);});
  });
}
test('only the starting active is available at level one',()=>{for(const hero of data.classes){const v=E.evaluate(data,E.empty(data,hero.id));assert.equal(v.unlocked,1);assert.equal(v.nodes.find(n=>v.available[n.id]).kind,'active');}});
test('a dedicated modifier checks its source activation independently of the player level',()=>{
 const fixture=E.clone(data),hero=fixture.classes.find(c=>c.id==='Warrior');
 // Model an unavailable source independently of normal level ordering.
 hero.nodes.find(n=>n.id==='W01').level=9;
 assert.equal(E.evaluate(fixture,E.empty(fixture,'Warrior',8)).available.WP02,false);
 const state=E.empty(fixture,'Warrior',9),view=E.evaluate(fixture,state);
 assert.equal(view.available.WP02,true);assert.equal(view.ranks.W01,1);assert.deepEqual(state.stack,[]);
});
test('upgrades preserve the point budget and cap',()=>{let state=E.empty(data,'Warrior',2);state=E.invest(data,state,'W01');assert.throws(()=>E.invest(data,state,'W01'),/POINTS/);state=E.empty(data,'Warrior',40);for(let i=0;i<4;i++)state=E.invest(data,state,'W01');assert.throws(()=>E.invest(data,state,'W01'),/MAX_RANK/);state=E.applyPath(data,state,'W17');assert.throws(()=>E.invest(data,state,'W17'),/MAX_RANK/);});
test('refunding a source upgrade preserves its base activation and equipped modifier',()=>{
 let state=E.empty(data,'Warrior',40);state=E.invest(data,state,'W01');state=E.invest(data,state,'WP02');state=E.invest(data,state,'W05');state=E.equip(data,state,'WP02');
 const before=E.clone(state),after=E.refund(data,state,'W01'),view=E.evaluate(data,after);assert.deepEqual(state,before);
 assert.equal(view.ranks.W01,1);assert.equal(view.ranks.WP02,2);assert.equal(view.ranks.W05,2);assert.deepEqual(after.equipped,['WP02']);assert.equal(before.stack.length-after.stack.length,1);
});
test('lowering level removes unavailable allocation and frees dependent loadout slots',()=>{const state=E.example(data,'Mage',1),low=E.normalize(data,{...state,level:3}),v=E.evaluate(data,low);assert.ok(v.spent<=2);assert.ok(low.equipped.every(id=>v.available[id]));assert.ok(!low.equipped.includes('M17'));assert.deepEqual(state,E.example(data,'Mage',1));});
test('passive equipment slots follow levels 3, 6 and 10; actives cap at four',()=>{
 for(const [level,count] of [[1,0],[3,1],[6,2],[10,3]])assert.equal(E.slots(data,E.empty(data,'Warrior',level),'passive'),count);
 let state=openAll('Warrior');for(const id of ['W01','W02','W03','W04'])state=E.equip(data,state,id);assert.throws(()=>E.equip(data,state,'W05'),/SLOTS/);
});
test('selecting a different ultimate replaces just the ultimate',()=>{let state=openAll('Warrior');state=E.equip(data,state,'W01');state=E.equip(data,state,'W17');state=E.equip(data,state,'W18');assert.deepEqual(state.equipped,['W01','W18']);});
test('prerequisite equipment is not required for unlocking a branch',()=>{const state=E.applyPath(data,E.empty(data,'Mage',40),'M17');assert.equal(state.equipped.length,0);assert.equal(E.evaluate(data,state).available.M17,true);});
test('planner export round trips and rejects runtime formats, unknown IDs, overspending and invalid ranks',()=>{
 const state=E.example(data,'Ranger',1);assert.deepEqual(E.parse(data,E.serialize(data,state)),state);
 for(const input of [{...state,format:'CLASS_SKILLS_V1'},{...state,level:0},{...state,stack:['constructor']},{...state,equipped:['constructor']},{...state,stack:Array(45).fill('A01')},{...state,stack:['M01']}])assert.throws(()=>E.parse(data,JSON.stringify(input)));
});
test('every source reference is classified independently of the configured tree links',()=>{
 const audit=require('./audit-dependencies.cjs').audit,root=require('node:path').resolve(__dirname,'../..');
 assert.equal(audit(source,data.classes,root).auditedSkills,111);
 for(const [child,parent] of [['WP18','W17'],['WP19','W18'],['AP18','A17'],['AP19','A18'],['MP18','M17'],['MP19','M18'],['WP11','W11'],['MP07','M08'],['AP12','A10']]){
  const broken=E.clone(data);for(const n of broken.classes.flatMap(c=>c.nodes))if(n.id===child){n.all=n.all.filter(r=>r.id!==parent);n.oneOf=n.oneOf.filter(r=>r.id!==parent);}
  assert.throws(()=>audit(source,broken.classes,root),/Unaccounted effect dependency/,child+' must detect omitted '+parent);
 }
 const extra=E.clone(data);extra.classes[0].nodes.find(n=>n.id==='WP09').all=[{id:'W12',rank:1}];
 assert.throws(()=>audit(source,extra.classes,root),/Unaccounted effect dependency/);
});
test('planner actions never mutate catalog or other class plans',()=>{const before=JSON.stringify(data),ranger=E.empty(data,'Ranger');E.example(data,'Mage',1);E.invest(data,E.empty(data,'Warrior',40),'W01');assert.equal(JSON.stringify(data),before);assert.deepEqual(ranger,E.empty(data,'Ranger'));});

test('ultimate equipment slot opens exactly at level 40',()=>{assert.equal(E.slots(data,E.empty(data,'Mage',39),'ultimate'),0);assert.equal(E.slots(data,E.empty(data,'Mage',40),'ultimate'),1);});

test('generic combat states do not impose invented skill sources',()=>{
 const find=id=>data.classes.flatMap(c=>c.nodes).find(n=>n.id===id);
 for(const id of ['W05','W12','W16','WP05','WP09','WP13','WP15','A08','A10','A15','AP09','AP15','M04','M13','M14','MP04','MP05','MP06','MP13','MP14','MP15','MP16','MP17']){
  assert.deepEqual(find(id).all,[],id);assert.deepEqual(find(id).oneOf,[],id);
 }
});
test('ultimates preserve the stage condition without imposing additional upgrade spending',()=>{
 let state=E.empty(data,'Warrior',40);assert.equal(E.evaluate(data,state).ultimateReady,true);
 const plan=E.pathPlan(data,state,'W17');assert.equal(plan.ok,true);assert.equal(plan.cost,0);
 state=E.invest(data,state,'W11');state=E.equip(data,state,'W17');
 assert.equal(E.evaluate(data,state).available.W18,true);assert.equal(state.equipped.length,1);
 const refunded=E.refund(data,state,'W11');assert.equal(E.evaluate(data,refunded).available.W17,true);assert.deepEqual(refunded.equipped,['W17']);
 const low=E.normalize(data,{...state,level:39});assert.equal(E.evaluate(data,low).available.W17,false);assert.deepEqual(low.equipped,[]);
});

test('level reduction refunds unavailable upgrades and restores capacity without changing independent investment',()=>{
 let state=E.empty(data,'Mage',40);for(const id of ['M13','M13','MP17','M01'])state=E.invest(data,state,id);for(const id of ['M13','MP17','M01'])state=E.equip(data,state,id);
 const low=E.normalize(data,{...state,level:30});assert.deepEqual(low.stack,['M01']);assert.deepEqual(low.equipped,['M01']);assert.equal(E.evaluate(data,low).remaining,28);
});

test('each split passive stays locked when only the other ultimate is active',()=>{
 for(const hero of data.classes)for(const [passive,ultimate,other] of [['P18','17','18'],['P19','18','17']]){
  const fixture=E.clone(data),nodes=fixture.classes.find(c=>c.id===hero.id).nodes,prefix=nodes[0].id[0];
  nodes.find(n=>n.id===prefix+ultimate).level=41;
  const state=E.empty(fixture,hero.id,40),view=E.evaluate(fixture,state);
  assert.equal(view.available[prefix+other],true);assert.equal(view.available[prefix+passive],false);
  assert.equal(E.pathPlan(fixture,state,prefix+passive).ok,false);
 }
});
test('direct alternatives accept each actual source and reject an unrelated unlocked skill',()=>{
 for(const hero of data.classes)for(const node of hero.nodes.filter(n=>n.oneOf.length)){
  for(const witness of [null,...node.oneOf]){
   const fixture=E.clone(data),nodes=fixture.classes.find(c=>c.id===hero.id).nodes;
   for(const r of node.oneOf)if(r.id!==witness?.id)nodes.find(n=>n.id===r.id).level=41;
   // Isolate the source from its own prerequisites so every alternative is exercised.
   if(witness){const n=nodes.find(n=>n.id===witness.id);n.all=[];n.oneOf=[];}
   const v=E.evaluate(fixture,E.empty(fixture,hero.id,40));
   assert.equal(v.available[node.id],Boolean(witness),node.id+' / '+witness?.id);
  }
 }
});
test('old planner exports move ultimate support ranks to the selected branch once',()=>{
 for(const [classId,prefix] of [['Warrior','W'],['Ranger','A'],['Mage','M']]){
  const legacy={format:E.FORMAT,classId,level:40,stack:[prefix+'P18',prefix+'P18'],equipped:[prefix+'18',prefix+'P18']};
  const migrated=E.parse(data,JSON.stringify(legacy));
  assert.deepEqual(migrated.stack,[prefix+'P19',prefix+'P19']);assert.deepEqual(migrated.equipped,[prefix+'18',prefix+'P19']);
  assert.deepEqual(E.parse(data,E.serialize(data,migrated)),migrated);
 }
});
