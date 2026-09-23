/* Independent coverage check: inspect source descriptions, not the chosen tree map. */
const fs=require('node:fs'),path=require('node:path'),crypto=require('node:crypto');
const exclusions={W16:['W05'],A12:['A04'],A15:['A01'],A17:['A02','A06'],M14:['M05'],M17:['M01','M02','M03'],MP09:['M02'],MP13:['M13']};
// Aliases and runtime-only dependencies verified in the effect owners below.
const implicit={WP03:['W02'],AP04:['A03'],AP14:['A11'],MP07:['M07','M08']};
const core='Assets/HELLSCRIPT/Runtime/Core/';
const evidence={
 WP03:[['CombatSimulation.Passives.cs','action.skill==1']],
 AP04:[['CombatSimulation.Projectiles.cs','trap.snapshot.passives[3]'],['CombatSimulation.Projectiles.cs','definitionId=action.skill==9?"LA03":"A03"']],
 AP14:[['CombatSimulation.ClassSkillPassives.cs','Passive("AP14")']],
 MP07:[['CombatSimulation.ClassSkillPassives.cs','HasDot("M07",enemy)||InFirewall(enemy.position)'],['CombatSimulation.ClassSkillPassives.cs','Passive("MP07")']],
 WP08:[['CombatSimulation.ClassSkillPassives.cs','HasDot("W09",enemy)||HasDot("REF_SW05_B2",enemy)']],
 WP11:[['CombatSimulation.ClassSkillPassives.cs','id=="W02"||id=="W11"']],
 WP17:[['CombatSimulation.ClassSkillPassives.cs','Passive("WP17")']],
 AP11:[['CombatSimulation.ClassSkillPassives.cs','ClassTrapArmed']],
 AP12:[['CombatSimulation.ClassSkillPassives.cs','Passive("AP12")']],
 AP13:[['CombatSimulation.ClassSkillPassives.cs','Passive("AP13")']],
 AP16:[['CombatSimulation.ClassSkillPassives.cs','Passive("AP16")']],
 MP08:[['CombatSimulation.ClassSkillPassives.cs','Passive("MP08")']],
 MP10:[['CombatSimulation.ClassSkillPassives.cs','id!="M06"&&id!="M09"']],
 MP11:[['CombatSimulation.ClassSkillPassives.cs','a.id=="M03"||a.id=="M12"']]
};
function audit(runtime,classes,root) {
 const errors=[],records=[],nodes=classes.flatMap(c=>c.nodes),seen=new Set();
 for(const source of runtime.skills) {
  const n=nodes.find(n=>n.id===source.id);
  if(!n||seen.has(n.id)){errors.push('Missing or duplicate audited skill '+source.id);continue;}seen.add(n.id);
  const referenced=runtime.skills.filter(s=>s.id!==source.id&&(new RegExp('\\b'+s.id+'\\b').test(source.description)||source.description.includes(s.name))).map(s=>s.id);
  const ignored=exclusions[n.id]||[];
  for(const id of ignored)if(!referenced.includes(id))errors.push('Stale exclusion '+n.id+' -> '+id);
  const positive=[...new Set([...referenced.filter(id=>!ignored.includes(id)),...(implicit[n.id]||[])])].sort();
  const actual=[...n.all,...(n.oneOf||[])].map(r=>r.id).sort();
  if(n.kind==='ultimate') {if(actual.length)errors.push('Ultimate must retain shared stage gate '+n.id);}
  else if(JSON.stringify(actual)!==JSON.stringify(positive))errors.push(`Unaccounted effect dependency ${n.id}: source=${positive} tree=${actual}`);
  if([...n.all,...(n.oneOf||[])].some(r=>r.rank!==1))errors.push('Unnecessary prerequisite rank '+n.id);
  // Multi-target modifiers accept an actual target; AP12 also needs the skill whose cooldown is reduced.
  if(n.kind!=='ultimate'&&positive.length>1){
   const required=n.id==='AP12'?['A04']:[];
   if(JSON.stringify(n.all.map(r=>r.id).sort())!==JSON.stringify(required))errors.push('Wrong direct-trigger grouping '+n.id);
  }
  const anchors=(evidence[n.id]||[]).map(([file,cue])=>{
   const filename=core+file,text=fs.readFileSync(path.join(root,filename),'utf8'),at=text.indexOf(cue);
   if(at<0)errors.push('Missing runtime evidence '+n.id+' '+cue);
   return {file:filename,line:at<0?null:text.slice(0,at).split('\n').length,cue};
  });
  records.push({id:n.id,kind:n.kind,classification:n.kind==='ultimate'?'shared_ultimate_gate':actual.length?'direct_effect':'level_only',
   required:n.all,oneDirectSource:n.oneOf||[],sourceReferences:referenced,directEffectReferences:positive,
   exclusions:ignored.map(id=>({id,reason:'Explicitly excluded by the effect description; not a prerequisite.'})),runtimeEvidence:anchors,
   sourceDescription:source.description,sourceSha256:crypto.createHash('sha256').update(source.description+'\n'+source.descriptionEn).digest('hex')});
 }
 if(seen.size!==runtime.skills.length||nodes.length!==runtime.skills.length)errors.push('Incomplete source coverage');
 if(errors.length)throw new Error(errors.join('\n'));
 return {result:'pass',auditedSkills:seen.size,policy:'Direct effect targets/triggers only. Dedicated ultimate support pairs. Shared states remain level-only; ultimates use the previous-stage gate.',records};
}
module.exports={audit};
