import {SCHEMA,CATALOG} from './schema.generated.js';
import {Rejected,object,integer,fields,canonical,sha256,randomToken,equal,reply,database,trustedOrigin,jsonBody,cookie,rows,tokenMap} from './common.js';
const SESSION_COOKIE='__Host-hellscript_ops',SESSION_SECONDS=28800,MAX_BODY=2*1024*1024;
const list=result=>rows(result);
function reason(value) {
  if(typeof value!=='string'||Array.from(value.trim()).length<2||Array.from(value.trim()).length>512||/[\x00-\x08\x0b\x0c\x0e-\x1f]/.test(value))
    throw new Rejected(422,'reason',[{path:'reason',code:'reason',messageKo:'변경 사유를 2~512자로 입력하세요.',messageEn:'Enter a change reason of 2–512 characters.'}]);
  return value.trim();
}
function revision(value) {if(!integer(value))throw new Rejected(400,'revision');return value;}
export function validateConfig(config) {
  const errors=[];
  const error=(path,code,ko,en)=>{if(errors.length<100)errors.push({path,code,messageKo:ko,messageEn:en});};
  function shape(value,keys,path) {
    if(!object(value)){error(path,'object','설정 객체가 필요합니다.','A configuration object is required.');return false;}
    if(Object.keys(value).length!==keys.length||keys.some(k=>!Object.hasOwn(value,k))){
      error(path,'fields','필수 항목이 빠졌거나 지원하지 않는 항목이 있습니다.','Required fields are missing or unsupported fields are present.');return false;}return true;
  }
  function number(value,low,high,whole,path) {
    if(typeof value!=='number'||!Number.isFinite(value)||value<low||value>high||(whole&&!Number.isSafeInteger(value))){
      error(path,'range',low+'~'+high+' 범위의 '+(whole?'정수':'숫자')+'를 입력하세요.','Enter a '+(whole?'whole number':'number')+' between '+low+' and '+high+'.');return false;}return true;
  }
  function rift(value,path) {
    if(!shape(value,Object.keys(SCHEMA.templates.rift),path))return;
    for(const field of SCHEMA.fields.filter(f=>f.path.split('.').length===2))number(value[field.path.slice(5)],field.min,field.max,field.type==='integer',path+'.'+field.path.slice(5));
    let valid=true;
    for(const source of ['normalRarity','eliteRarity','bossRarity']) {
      const p=value[source],location=path+'.'+source;
      if(!shape(p,['common','magic','rare','legendary','legendaryCap'],location)){valid=false;continue;}
      if(Object.keys(p).some(k=>!number(p[k],0,['legendary','legendaryCap'].includes(k)? .9999 : 1,false,location+'.'+k))){valid=false;continue;}
      if(Math.abs(p.common+p.magic+p.rare+p.legendary-1)>1e-12)error(location,'probability_sum','등급 확률의 합은 100%여야 합니다.','Rarity probabilities must total 100%.');
      if(p.legendary>p.legendaryCap)error(location+'.legendaryCap','legendary_cap','전설 상한은 기본 확률 이상이어야 합니다.','The legendary cap must not be lower than its base probability.');
      if((source!=='normalRarity'&&p.common!==0)||(source==='bossRarity'&&p.magic!==0))error(location,'rarity_floor','정예·보스의 최소 장비 등급을 확인하세요.','Check the minimum rarity for elites and bosses.');
    }
    if(valid)for(const k of ['legendary','legendaryCap'])if(!(value.normalRarity[k]<=value.eliteRarity[k]&&value.eliteRarity[k]<=value.bossRarity[k]))
      error(path,'rarity_order','전설 기본 확률과 상한은 일반≤정예≤보스 순서여야 합니다.','Legendary bases and caps must follow normal ≤ elite ≤ boss.');
  }
  if(shape(config,['rift','stageOverrides','firstClearRewards'],'')) {
    rift(config.rift,'rift');
    const spans=[];
    if(!Array.isArray(config.stageOverrides)||config.stageOverrides.length>100)error('stageOverrides','budget','단계 구간은 최대 100개입니다.','At most 100 stage ranges are supported.');
    else config.stageOverrides.forEach((v,i)=>{
      const p='stageOverrides['+i+']';if(!shape(v,['fromStage','toStage','rift'],p))return;
      const a=number(v.fromStage,1,1000,true,p+'.fromStage'),b=number(v.toStage,1,1000,true,p+'.toStage');
      if(a&&b){if(v.fromStage>v.toStage)error(p,'stage_order','시작 단계는 종료 단계 이하여야 합니다.','The start stage must not exceed the end stage.');
        else if(spans.some(s=>v.fromStage<=s[1]&&v.toStage>=s[0]))error(p,'stage_overlap','단계 구간은 서로 겹칠 수 없습니다.','Stage ranges must not overlap.');
        spans.push([v.fromStage,v.toStage]);}rift(v.rift,p+'.rift');
    });
    const seen=new Set(),boxes=new Map(CATALOG.boxes.map(b=>[b.id,b]));
    if(!Array.isArray(config.firstClearRewards)||config.firstClearRewards.length>1000)error('firstClearRewards','budget','최초 보상 단계는 최대 1,000개입니다.','At most 1,000 first-clear reward stages are supported.');
    else config.firstClearRewards.forEach((v,i)=>{
      const p='firstClearRewards['+i+']';if(!shape(v,['stage','grants'],p))return;
      const a=number(v.stage,1,1000,true,p+'.stage');
      if(a){if(seen.has(v.stage))error(p+'.stage','duplicate_stage','같은 단계의 최초 보상을 중복 지정할 수 없습니다.','A first-clear reward stage must be unique.');seen.add(v.stage);}
      if(!Array.isArray(v.grants)||v.grants.length>20){error(p+'.grants','budget','단계별 보상 항목은 최대 20개입니다.','At most 20 grants per stage are supported.');return;}
      v.grants.forEach((g,j)=>{
        const q=p+'.grants['+j+']';if(!shape(g,['boxId','count','minimumQuality'],q))return;
        const box=typeof g.boxId==='string'&&boxes.get(g.boxId);
        if(!box)error(q+'.boxId','box_id','지원하는 보상 상자를 선택하세요.','Select a supported reward box.');
        else if(a&&v.stage<30&&box.kind==='equipment'&&box.rarity===3)error(q+'.boxId','rarity_gate','전설 장비 상자는 30단계부터 지급할 수 있습니다.','Legendary equipment boxes require stage 30 or higher.');
        number(g.count,1,100,true,q+'.count');number(g.minimumQuality,0,10000,true,q+'.minimumQuality');
      });
    });
  }
  if(errors.length)throw new Rejected(422,'configuration_invalid',errors);return config;
}
function release(row,privateView=false) {
  if(!row)throw new Rejected(404,'release');
  const v={schemaVersion:row.schema_version,version:row.version,publishedUtcMs:row.published_utc_ms,
    config:JSON.parse(row.config_json),configJson:row.config_json,configHash:row.config_hash};
  if(privateView)Object.assign(v,{operator:row.operator,reason:row.reason,rollbackOf:row.rollback_of});return v;
}
function draft(row) {
  if(!row)throw new Rejected(503,'storage_unavailable');
  return {revision:row.revision,baseVersion:row.base_version,config:JSON.parse(row.config_json),configHash:row.config_hash,
    reason:row.reason,updatedUtcMs:row.updated_utc_ms,operator:row.operator};
}
async function state(db,actor) {
  const r=await db.batch([db.prepare('SELECT r.* FROM liveops_releases r JOIN liveops_active a ON a.version=r.version WHERE a.singleton=1'),
    db.prepare('SELECT * FROM liveops_draft WHERE singleton=1'),db.prepare('SELECT * FROM liveops_releases ORDER BY version DESC LIMIT 100')]);
  if(!list(r[0])[0])throw new Rejected(503,'storage_unavailable');
  return {current:release(list(r[0])[0],true),draft:draft(list(r[1])[0]),
    history:list(r[2]).map(v=>({version:v.version,publishedUtcMs:v.published_utc_ms,operator:v.operator,reason:v.reason,rollbackOf:v.rollback_of,configHash:v.config_hash})),schema:SCHEMA,operator:actor};
}
function changes(a,b,path='') {
  if(object(a)&&object(b))return [...new Set([...Object.keys(a),...Object.keys(b)])].sort().flatMap(k=>changes(a[k]??null,b[k]??null,path?path+'.'+k:k));
  return canonical(a)===canonical(b)?[]:[{path,before:a,after:b}];
}
export function createLiveOps({now=Date.now}={}) {
  function sameOrigin(request,env) {
    if(request.headers.get('Origin')!==trustedOrigin(env)||!['same-origin','none'].includes(request.headers.get('Sec-Fetch-Site')||'same-origin'))throw new Rejected(403,'origin');
  }
  function credentials(env) {
    const ops=tokenMap(env.HELLSCRIPT_OPS_TOKENS,32),qa=tokenMap(env.HELLSCRIPT_TELEMETRY_TOKENS);
    if(Object.keys(ops).some(t=>Object.hasOwn(qa,t)))throw new Rejected(503,'credentials_configuration');return ops;
  }
  async function session(request,env,write=false) {
    const raw=cookie(request,SESSION_COOKIE);if(!/^[A-Za-z0-9_-]{43}$/.test(raw))throw new Rejected(401,'authentication');
    const row=await database(env).prepare('SELECT * FROM ops_sessions WHERE session_hash=? AND expires_utc_ms>?').bind(await sha256(raw),now()).first();
    if(!row)throw new Rejected(401,'authentication');
    const active=credentials(env);let valid=false;
    for(const [t,a] of Object.entries(active))if(a===row.operator&&equal(await sha256(t),row.credential_hash))valid=true;
    if(!valid)throw new Rejected(401,'authentication');
    if(write){sameOrigin(request,env);if(!equal(request.headers.get('X-CSRF-Token'),row.csrf_token))throw new Rejected(403,'csrf');}
    return row;
  }
  async function mutate(db,actor,v,path) {
    const why=reason(v.reason),version=revision(path==='/ops/api/draft'?v.baseVersion:v.expectedVersion),
      rev=revision(v.expectedDraftRevision),utc=now();
    const current=await db.prepare('SELECT r.* FROM liveops_releases r JOIN liveops_active a ON a.version=r.version WHERE a.singleton=1').first();
    const saved=await db.prepare('SELECT * FROM liveops_draft WHERE singleton=1').first();
    if(!current||!saved)throw new Rejected(503,'storage_unavailable');
    const conflict=path==='/ops/api/draft'?'draft_conflict':'publish_conflict';
    if(current.version!==version||saved.revision!==rev||(path==='/ops/api/publish'&&saved.base_version!==version))throw new Rejected(409,conflict);
    if(rev+1>=2147483647||(path!=='/ops/api/draft'&&version+1>=2147483647))throw new Rejected(409,'version_budget');
    let raw,rollback=null;
    if(path==='/ops/api/draft')raw=canonical(validateConfig(v.config));
    else if(path==='/ops/api/publish')raw=canonical(validateConfig(JSON.parse(saved.config_json)));
    else{
      rollback=revision(v.targetVersion);
      const target=await db.prepare('SELECT * FROM liveops_releases WHERE version=?').bind(rollback).first();
      if(!target)throw new Rejected(404,'release');
      if(rollback>=version)throw new Rejected(409,'no_changes');
      validateConfig(JSON.parse(target.config_json));raw=target.config_json;
    }
    if(path!=='/ops/api/draft'&&canonical(JSON.parse(raw))===canonical(JSON.parse(current.config_json)))throw new Rejected(409,'no_changes');
    const hash=await sha256(raw),next=path==='/ops/api/draft'?version:version+1,previous=path==='/ops/api/draft'?saved.config_json:current.config_json;
    const commands=[db.prepare('INSERT INTO ops_write_guard SELECT CASE WHEN EXISTS(SELECT 1 FROM liveops_active a,liveops_draft d WHERE a.singleton=1 AND d.singleton=1 AND a.version=? AND d.revision=? AND d.config_hash=?) THEN 1 ELSE 0 END').bind(version,rev,saved.config_hash)];
    if(path!=='/ops/api/draft'){
      commands.push(db.prepare('INSERT INTO liveops_releases VALUES(?,?,?,?,?,?,?,?)').bind(next,1,raw,hash,utc,actor,why,rollback));
      commands.push(db.prepare('UPDATE liveops_active SET version=? WHERE singleton=1').bind(next));
    }
    commands.push(db.prepare('UPDATE liveops_draft SET revision=?,base_version=?,config_json=?,config_hash=?,reason=?,updated_utc_ms=?,operator=? WHERE singleton=1').bind(rev+1,next,raw,hash,why,utc,actor));
    commands.push(db.prepare('INSERT INTO liveops_audit(utc_ms,operator,action,reason,from_version,to_version,draft_revision,previous_hash,config_hash,changes_json) VALUES(?,?,?,?,?,?,?,?,?,?)').bind(
      utc,actor,path==='/ops/api/draft'?'draft_saved':rollback===null?'published':'rollback',why,version,next,rev+1,await sha256(previous),hash,canonical(changes(JSON.parse(previous),JSON.parse(raw)))));
    commands.push(db.prepare('DELETE FROM ops_write_guard WHERE id=1'));
    try{await db.batch(commands);}catch(error){
      const fresh=await db.prepare('SELECT a.version,d.revision FROM liveops_active a,liveops_draft d WHERE a.singleton=1 AND d.singleton=1').first();
      if(fresh&&(fresh.version!==version||fresh.revision!==rev))throw new Rejected(409,conflict);throw error;
    }
    return state(db,actor);
  }
  return async function liveops(request,env) {
    const url=new URL(request.url),path=url.pathname,method=request.method,db=database(env);
    if(url.search)throw new Rejected(400,'query');
    const publicVersion=path.match(/^\/v1\/liveops\/releases\/([0-9]{1,10})$/);
    if(path.startsWith('/v1/liveops/')){
      if(path!=='/v1/liveops/current'&&!publicVersion)throw new Rejected(404,'route');
      if(!['GET','HEAD'].includes(method))return reply({error:'method'},405,{Allow:'GET, HEAD'});
      const row=publicVersion?await db.prepare('SELECT * FROM liveops_releases WHERE version=?').bind(revision(Number(publicVersion[1]))).first():
        await db.prepare('SELECT r.* FROM liveops_releases r JOIN liveops_active a ON a.version=r.version WHERE a.singleton=1').first();
      const v=release(row),response=reply(v,200,{ETag:'"liveops-'+v.version+'-'+v.configHash+'"'});
      return method==='HEAD'?new Response(null,response):response;
    }
    const ops=credentials(env);if(!Object.keys(ops).length)throw new Rejected(503,'operations_disabled');
    if(path==='/ops/api/session'&&method==='POST'){
      sameOrigin(request,env);const utc=now(),cutoff=utc-300000;
      await db.batch([db.prepare('DELETE FROM login_windows WHERE start_utc_ms<?').bind(cutoff),
        db.prepare('INSERT INTO login_windows VALUES(?,?,1) ON CONFLICT(remote_hash) DO UPDATE SET attempts=attempts+1').bind(await sha256((request.headers.get('CF-Connecting-IP')||'shared').slice(0,64)),utc)]);
      const budget=await db.prepare('SELECT SUM(attempts) AS total FROM login_windows').first();
      const local=await db.prepare('SELECT attempts FROM login_windows WHERE remote_hash=?').bind(await sha256((request.headers.get('CF-Connecting-IP')||'shared').slice(0,64))).first();
      if(local.attempts>20||budget.total>100)throw new Rejected(429,'login_rate');
      const v=await jsonBody(request);fields(v,['token']);let actor=null;
      for(const [t,a] of Object.entries(ops))if(equal(t,v.token))actor=a;
      if(!actor)throw new Rejected(401,'authentication');
      const token=randomToken(),csrf=randomToken(),expires=utc+SESSION_SECONDS*1000;
      await db.batch([db.prepare('DELETE FROM ops_sessions WHERE expires_utc_ms<=?').bind(utc),
        db.prepare('DELETE FROM ops_sessions WHERE session_hash IN (SELECT session_hash FROM ops_sessions WHERE operator=? ORDER BY expires_utc_ms DESC LIMIT -1 OFFSET 7)').bind(actor),
        db.prepare('INSERT INTO ops_sessions VALUES(?,?,?,?,?)').bind(await sha256(token),actor,await sha256(v.token),csrf,expires)]);
      return reply({operator:actor,csrfToken:csrf,expiresUtcMs:expires},200,{'Set-Cookie':SESSION_COOKIE+'='+token+'; Path=/; Max-Age='+SESSION_SECONDS+'; HttpOnly; Secure; SameSite=Strict'});
    }
    const s=await session(request,env,!['GET','HEAD'].includes(method)),actor=s.operator;
    if(path==='/ops/api/session'){
      if(method==='GET')return reply({operator:actor,csrfToken:s.csrf_token,expiresUtcMs:s.expires_utc_ms});
      if(method==='DELETE'){await db.prepare('DELETE FROM ops_sessions WHERE session_hash=?').bind(s.session_hash).run();return reply({loggedOut:true},200,{'Set-Cookie':SESSION_COOKIE+'=; Path=/; Max-Age=0; HttpOnly; Secure; SameSite=Strict'});}
      return reply({error:'method'},405,{Allow:'GET, POST, DELETE'});
    }
    if(path==='/ops/api/state'&&method==='GET')return reply(await state(db,actor));
    if(path==='/ops/api/audit'&&method==='GET'){
      const r=list(await db.prepare('SELECT * FROM liveops_audit ORDER BY id DESC LIMIT 100').all());
      return reply({entries:r.map(v=>({id:v.id,utcMs:v.utc_ms,operator:v.operator,action:v.action,reason:v.reason,
        fromVersion:v.from_version,toVersion:v.to_version,draftRevision:v.draft_revision,previousHash:v.previous_hash,
        configHash:v.config_hash,changes:JSON.parse(v.changes_json)}))});
    }
    const version=path.match(/^\/ops\/api\/releases\/([0-9]{1,10})$/);
    if(version&&method==='GET')return reply(release(await db.prepare('SELECT * FROM liveops_releases WHERE version=?').bind(revision(Number(version[1]))).first(),true));
    if(!['/ops/api/validate','/ops/api/draft','/ops/api/publish','/ops/api/rollback'].includes(path))throw new Rejected(404,'route');
    if(method!=='POST')return reply({error:'method'},405,{Allow:'POST'});
    const v=await jsonBody(request,MAX_BODY);
    if(path==='/ops/api/validate'){fields(v,['config']);validateConfig(v.config);return reply({valid:true});}
    fields(v,path==='/ops/api/draft'?['expectedDraftRevision','baseVersion','config','reason']:
      path==='/ops/api/publish'?['expectedVersion','expectedDraftRevision','reason']:['expectedVersion','expectedDraftRevision','targetVersion','reason']);
    return reply(await mutate(db,actor,v,path));
  };
}
