const fs = require('node:fs'), path = require('node:path'), crypto = require('node:crypto');
const rules = require('./tree-design.cjs'), layout = require('./tree-layout.cjs');
const folder = __dirname, root = path.resolve(folder, '../..');
const read = p => JSON.parse(fs.readFileSync(path.join(root, p), 'utf8'));
const runtimePath = 'Assets/HELLSCRIPT/Resources/Data/ClassSkills.json';
const designPath = 'Docs/Design/ClassSkills/catalog.json';
const runtime = read(runtimePath), design = read(designPath);
const hash = p => crypto.createHash('sha256').update(fs.readFileSync(path.join(root, p))).digest('hex');
const data = { version: rules.version, status: rules.status, levelCap: rules.levelCap,
  ultimatePrerequisite: { from: rules.phases[rules.ultimatePrerequisitePhase].from, to: rules.phases[rules.ultimatePrerequisitePhase].to },
  phases: rules.phases, ultimateUnlockLevel: design.loadout.ultimateUnlockLevel, slots: { active: design.loadout.normalActiveSlots, passive: design.loadout.passiveSlots, ultimate: design.loadout.ultimateSlots },
  passiveSlotLevels: read('Assets/HELLSCRIPT/Resources/ContentUnlocks.json').passiveLevels,
  source: { runtime: runtimePath, runtimeSha256: hash(runtimePath), design: designPath, designSha256: hash(designPath), gameEnabled: runtime.playerEnabled },
  classes: [] };
const errors = [], seen = new Set();
if (rules.phases[rules.ultimatePrerequisitePhase + 1]?.from !== data.ultimateUnlockLevel || data.ultimatePrerequisite.to >= data.ultimateUnlockLevel) throw new Error('Ultimate prerequisite must be the immediately previous phase');
for (const hero of rules.classes) {
  const src = runtime.skills.filter(n => n.heroClass === ['Warrior','Ranger','Mage'].indexOf(hero.id));
  if (hero.nodes.length !== 37 || src.length !== 37) errors.push(hero.id + ': expected 37 nodes');
  const byId = {};
  for (const proposed of hero.nodes) {
    const current = src.find(n => n.id === proposed.id), origin = design.skills.find(n => n.id === proposed.id);
    if (!current || !origin || seen.has(proposed.id)) { errors.push('Invalid or duplicate ID ' + proposed.id); continue; }
    seen.add(proposed.id);
    if ('any' in proposed) errors.push('Per-skill alternative prerequisites are not supported: ' + proposed.id);
    if (proposed.lineage && current.category === 'ultimate') errors.push('Ultimates have no lineage parent: ' + proposed.id);
    const dependency = rules.dependencies[proposed.id];
    if ([...proposed.all,...proposed.oneOf].some(r=>r.rank!==1)) errors.push('Only base-rank direct dependencies may be required: ' + proposed.id);
    if (dependency && (!current.description.includes(dependency.sourceCue) || !dependency.reason.ko || !dependency.reason.en)) errors.push('Direct dependency needs current source evidence and a bilingual explanation: ' + proposed.id);
    if (current.category === 'ultimate' && (proposed.all.length || proposed.oneOf.length || proposed.level !== data.ultimateUnlockLevel)) errors.push('Ultimates require only their shared phase gate and level: ' + proposed.id);
    const iconPath = `Assets/HELLSCRIPT/Resources/Art/ClassSkillIcons/${hero.id}/${proposed.id}.png`;
    if (!fs.existsSync(path.join(root, iconPath))) errors.push('Missing art ' + proposed.id);
    byId[proposed.id] = { ...proposed, kind: current.category, maxRank: current.maxRank,
      name: { ko: current.name, en: current.nameEn }, description: { ko: current.description, en: current.descriptionEn },
      sourceUnlock: current.unlock, icon: '../../' + iconPath, tags: origin.tags,
      equipmentIds: origin.equipmentIds, setIds: origin.setIds || [], rationale: dependency?.reason || rules.rationale[proposed.id] || null };
  }
  const sorted = [], done = new Set(), active = new Set();
  function visit(n) {
    if (done.has(n.id)) return;
    if (active.has(n.id)) throw new Error('Cyclic prerequisite: ' + n.id);
    active.add(n.id);
    if (n.lineage) {
      const parent = byId[n.lineage];
      if (!parent || parent.id === n.id || parent.kind === 'ultimate') throw new Error('Invalid lineage on ' + n.id);
      if (parent.level > n.level) throw new Error('Lineage parent opens later than ' + n.id);
      visit(parent);
    }
    for (const r of [...n.all,...n.oneOf]) {
      const parent = byId[r.id];
      if (!parent || parent.id === n.id || r.rank < 1 || r.rank > parent.maxRank) throw new Error('Invalid prerequisite on ' + n.id);
      if (n.all.includes(r) && parent.level > n.level) throw new Error('Prerequisite opens later than ' + n.id);
      visit(parent);
    }
    if(n.oneOf.length&&!n.oneOf.some(r=>byId[r.id].level<=n.level))throw new Error('Every direct alternative opens later than '+n.id);
    active.delete(n.id); done.add(n.id); sorted.push(n);
  }
  Object.values(byId).sort((a,b) => a.level-b.level || a.id.localeCompare(b.id)).forEach(visit);
  const placed = layout.place({ ...hero, nodes: sorted });
  let phaseTop = 0;
  const bands = [];
  for (let phase = 0; phase < data.phases.length; phase++) {
    const interval = data.phases[phase];
    let rows = 1;
    for (const n of sorted) {
      const s = placed[n.id];
      if (s.band !== phase) continue;
      rows = Math.max(rows, s.row + 1);
      Object.assign(n, { phase, row: s.row, col: s.col, links: s.links, via: s.via,
        x: 66 + n.branch * 270 + (s.col < 0 ? 62 : s.col * 124), y: phaseTop + 64 + s.row * 140 });
    }
    const height = 64 + rows * 140 + 26;
    bands.push({ ...interval, top: phaseTop, height }); phaseTop += height;
  }
  data.classes.push({ ...hero, grid: undefined, nodes: sorted, bands, height: phaseTop + 8, width: 878 });
}
if (seen.size !== 111) errors.push('Expected exactly 111 unique skills');
for (const id of Object.keys(rules.dependencies)) if (!seen.has(id)) errors.push('Dependency audit refers to an unknown skill: ' + id);
if (errors.length) throw new Error(errors.join('\n'));
const dependencyAudit=require('./audit-dependencies.cjs').audit(runtime,data.classes,root);
const engine = require('./engine.js'), paths = [];
for (const hero of data.classes) {
  for (const n of hero.nodes) {
    const plan = engine.pathPlan(data, engine.empty(data, hero.id, n.level), n.id);
    if (!plan.ok || !engine.evaluate(data,plan.state).available[n.id]) throw new Error(`Unreachable ${n.id} at level ${n.level}: ${plan.error}`);
    paths.push({ id: n.id, level: n.level, requiredPoints: plan.cost, budget: n.level-1 });
  }
  hero.examples.forEach((_,i) => engine.example(data,hero.id,i));
}
const encoded = JSON.stringify(data).replace(/</g,'\\u003c');
fs.writeFileSync(path.join(folder,'catalog.json'),JSON.stringify(data,null,2)+'\n');
fs.writeFileSync(path.join(folder,'catalog.js'),'window.SKILL_TREE_DATA = '+encoded+';\n');
const template = fs.readFileSync(path.join(folder,'page.html'),'utf8');
for (const cls of ['index','Warrior','Ranger','Mage']) {
  fs.writeFileSync(path.join(folder,cls+'.html'),template.replaceAll('__CLASS__',cls==='index'?'Warrior':cls));
}
const allNodes=data.classes.flatMap(c=>c.nodes);
fs.writeFileSync(path.join(folder,'evidence/graph-validation.json'),JSON.stringify({ result:'pass', nodes:seen.size, classes:3, cyclicEdges:0, unreachableNodes:0,
  directSourceGroups:allNodes.filter(n=>n.oneOf.length).length, effectEdges:allNodes.reduce((sum,n)=>sum+n.all.length,0),
  detailOnlyEdges:allNodes.reduce((sum,n)=>sum+n.oneOf.length,0),
  lineageEdges:allNodes.filter(n=>n.lineage).length, drawnLinks:allNodes.reduce((sum,n)=>sum+n.links.length,0),
  rootNodes:allNodes.filter(n=>n.kind!=='ultimate'&&!n.all.length&&!n.oneOf.length&&!n.lineage).length,
  layout:{ crossingLinks:0, upwardLinks:0, unplacedNodes:0, sharedSockets:0, note:'tree-layout.cjs rejects any grid that breaks these rules.' },
  extraPrerequisiteRanks:0,
  ultimateGate:{ level:data.ultimateUnlockLevel, ...data.ultimatePrerequisite, requiredUnlockedSkills:1, appliesTo:allNodes.filter(n=>n.kind==='ultimate').map(n=>n.id) },
  source:data.source, paths },null,2)+'\n');
fs.writeFileSync(path.join(folder,'evidence/dependency-audit.json'),JSON.stringify({source:data.source,...dependencyAudit},null,2)+'\n');
console.log(JSON.stringify({ result:'pass', nodes:seen.size, classes:data.classes.map(c=>({id:c.id,nodes:c.nodes.length,height:c.height})), pathsReachable:paths.length }));
