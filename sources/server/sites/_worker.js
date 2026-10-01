// One Site owns player authentication, operations and durable observations.
import {Rejected,object,integer,reply,sha256,bodyBytes,parseJson,database,trustedOrigin} from './common.js';
import {createAuth,telemetryIdentity} from './auth.js';
import {createLiveOps} from './liveops.js';
import {createMigration} from './migration.js';
import {OPS_ASSETS} from './ops.generated.js';
const MAX_BODY = 16 * 1024 * 1024;
const ID = /^[0-9a-f]{32}$/;
const text = (v, max) => typeof v === 'string' && Array.from(v).length <= max;
const require = (condition, code) => { if (!condition) throw new Rejected(422, code); };
function tree(v, depth = 0) {
  require(depth <= 64, 'nesting');
  if (typeof v === 'number') require(Number.isFinite(v), 'nonfinite');
  if (v && typeof v === 'object') for (const child of Object.values(v)) tree(child, depth + 1);
}
export function validate(record) {
  tree(record);
  require(object(record) && typeof record.id === 'string' && ID.test(record.id), 'run_id');
  const j = record.journal;
  require(object(j) && j.version === 1, 'schema');
  require(j.trust === 'client_observed' && j.archived === false, 'provenance');
  require(['victory', 'defeat'].includes(j.outcome), 'outcome');
  for (const key of ['heroId', 'localAccountId']) require(typeof j[key] === 'string' && ID.test(j[key]), 'identity');
  for (const [key, min, max] of [
    ['stage', 1, 100000], ['sequence', 0, 10000000], ['omittedEvents', 0, 10000000],
    ['attempt', 1, Number.MAX_SAFE_INTEGER], ['earnedGold', 0, Number.MAX_SAFE_INTEGER], ['kills', 0, 10000000]
  ]) require(integer(j[key], min, max), 'counter_' + key);
  require(record.stage === j.stage && record.kills === j.kills, 'summary_mismatch');
  for (const key of ['equipmentCollected', 'runesAwarded', 'bossesKilled', 'resumes', 'priorUnexportedLossCount']) {
    if (key in j) require(integer(j[key], 0, Number.MAX_SAFE_INTEGER), 'counter_' + key);
  }
  if ('bossDefeated' in j) require(typeof j.bossDefeated === 'boolean', 'boss_result');
  if ('finish' in j) require(text(j.finish, 128), 'finish');
  const tuning = j.liveOps;
  if (tuning !== undefined && tuning !== null) {
    require(object(tuning) && tuning.schemaVersion === 1 && integer(tuning.version, 0, 2147483647) &&
      ['builtin', 'published'].includes(tuning.source) && typeof tuning.configHash === 'string' &&
      /^[0-9a-f]{64}$/.test(tuning.configHash) && tuning.stage === j.stage &&
      j.liveOpsVersion === tuning.version && j.liveOpsConfigHash === tuning.configHash, 'liveops_reference');
  }
  for (const key of ['simulationSeconds', 'attendanceSeconds', 'observedFrom', 'walkingDistance', 'maxHealth']) {
    require(typeof j[key] === 'number' && Number.isFinite(j[key]) && j[key] >= 0, 'number_' + key);
  }
  for (const key of ['startedUtcMs', 'completedUtcMs']) require(integer(j[key], 0, Number.MAX_SAFE_INTEGER), 'clock');
  require(Array.isArray(j.events) && j.events.length <= 20000, 'event_budget');
  let sequence = 0, seconds = -1;
  for (const e of j.events) {
    require(object(e) && Number.isSafeInteger(e.sequence) && e.sequence > sequence, 'event_sequence');
    require(typeof e.seconds === 'number' && Number.isFinite(e.seconds) &&
      e.seconds >= Math.max(0, seconds) && e.seconds <= j.simulationSeconds + 0.1, 'event_time');
    for (const key of ['kind', 'message']) require(text(e[key], 8192), 'event_text');
    for (const key of ['source', 'trigger']) if (key in e) require(text(e[key], 512), 'event_identifier');
    for (const key of ['hp', 'resource']) if (key in e) require(typeof e[key] === 'number' && Number.isFinite(e[key]), 'event_number');
    for (const key of ['target', 'actionId']) if (key in e) require(integer(e[key], -1, 2147483647), 'event_reference');
    for (const key of ['damageEvents', 'rules', 'builds', 'edicts']) if (key in e) {
      require(Array.isArray(e[key]) && e[key].length <= 1 && e[key].every(object), 'event_snapshot');
    }
    sequence = e.sequence; seconds = e.seconds;
  }
  require((!j.events.length || sequence === j.sequence) && j.sequence - j.events.length === j.omittedEvents, 'event_gap');
  for (const key of ['equipmentDrops', 'resourceDrops']) require(Array.isArray(j[key]) && j[key].length <= 20000 && j[key].every(object), 'reward_budget');
  const signals = ['CLIENT_OBSERVATION_UNVERIFIED'];
  if (j.startedUtcMs === 0 || j.observedFrom > 0) signals.push('PARTIAL_RUN');
  if (j.completedUtcMs < j.startedUtcMs) signals.push('CLIENT_CLOCK_ROLLBACK');
  if (j.simulationSeconds > j.attendanceSeconds * 1.5 + 1) signals.push('REPORTED_SPEED_REQUIRES_AUTHORITY_CHECK');
  if (j.outcome === 'victory' && !j.bossDefeated) signals.push('VICTORY_WITHOUT_REPORTED_BOSS');
  if (j.omittedEvents) signals.push('EVENT_GAP');
  if (j.priorUnexportedLossCount) signals.push('LOCAL_EXPORT_LOSS');
  return signals;
}
export function createHandler({fetcher,now=Date.now,authenticate:overrideIdentity}={}) {
  const auth=createAuth({fetcher,now}),liveops=createLiveOps({now}),migration=createMigration({now});
  async function initialize(env) {
    if(!env.DB||!env.LOGS)throw new Rejected(503,'storage_unavailable');
    const db=database(env);
    const version=await db.prepare('SELECT version FROM schema_state WHERE id=1').first();
    if(version?.version!==1)throw new Rejected(503,'storage_schema');
    const state=await db.prepare('SELECT import_id FROM backend_state WHERE id=1').first();
    if(!state)throw new Rejected(503,'backend_initializing');
    return db;
  }
  const authenticate=(request,env)=>overrideIdentity?overrideIdentity(request,env):telemetryIdentity(request,env,now());
  async function ingest(request, env) {
    const identity = await authenticate(request, env), bytes = await bodyBytes(request,MAX_BODY);
    let record;
    try { record = parseJson(bytes); }
    catch { throw new Rejected(400, 'json'); }
    const signals = validate(record), j = record.journal;
    const key = request.headers.get('Idempotency-Key');
    if (key !== null && key !== record.id) throw new Rejected(400, 'idempotency_key');
    const hash = await sha256(bytes), at = now(), window = Math.floor(at / 60000);
    await initialize(env);
    // Primary-key constraints resolve concurrent duplicates. The immutable,
    // hash-keyed raw object is durable before receipt metadata can commit.
    const db = env.DB.withSession ? env.DB.withSession('first-primary') : env.DB;
    await db.batch([
      db.prepare('INSERT INTO upload_rates VALUES(?,?,1,?) ON CONFLICT(account_id,window) DO UPDATE SET count=count+1,bytes=bytes+excluded.bytes').bind(identity.accountId,window,bytes.byteLength),
      db.prepare('DELETE FROM upload_rates WHERE window<?').bind(window-2)
    ]);
    const rate = await db.prepare('SELECT count,bytes FROM upload_rates WHERE account_id=? AND window=?').bind(identity.accountId,window).first();
    if (rate.count > 120 || rate.bytes > 64*1024*1024) throw new Rejected(429, 'rate_limited');
    const find = () => db.prepare('SELECT payload_hash,object_key FROM runs WHERE account_id=? AND run_id=?').bind(identity.accountId,record.id).first();
    async function conflict() {
      await db.prepare('INSERT INTO audit VALUES(?,?,?,?,?)').bind(at,identity.accountId,record.id,'PAYLOAD_CONFLICT',hash).run();
      throw new Rejected(409,'payload_conflict');
    }
    const previous = await find();
    if (previous && previous.payload_hash !== hash) return conflict();
    const objectKey = identity.accountId + '/' + record.id + '/' + hash + '.json';
    await env.LOGS.put(objectKey,bytes,{httpMetadata:{contentType:'application/json; charset=utf-8'},customMetadata:{sha256:hash,runId:record.id}});
    await db.prepare('INSERT INTO runs VALUES (' + Array(23).fill('?').join(',') + ') ON CONFLICT(account_id,run_id) DO NOTHING').bind(
      identity.accountId,record.id,hash,identity.kind,at,j.localAccountId,j.heroId,j.stage,j.outcome,j.finish??null,
      j.startedUtcMs,j.completedUtcMs,j.simulationSeconds,j.attendanceSeconds,j.earnedGold,j.kills,
      j.bossDefeated?1:0,j.equipmentCollected??0,j.liveOps?.version??null,j.liveOps?.configHash??null,
      JSON.stringify(signals),objectKey,bytes.byteLength
    ).run();
    const committed = await find();
    if (!committed) throw new Rejected(503,'storage_unavailable');
    if (committed.payload_hash !== hash) {
      await env.LOGS.delete(objectKey); // Never touches the committed object's key.
      return conflict();
    }
    const stored = await env.LOGS.head(committed.object_key);
    if (!stored || stored.size !== bytes.byteLength || stored.customMetadata?.sha256 !== hash) throw new Rejected(503,'storage_unavailable');
    return reply({accepted:true,runId:record.id,payloadHash:hash});
  }
  return {
    async fetch(request, env) {
      try {
        const path = new URL(request.url).pathname;
        if (path === '/healthz' && ['GET','HEAD'].includes(request.method)) {
          trustedOrigin(env); await initialize(env);
          await env.DB.prepare('SELECT run_id FROM runs LIMIT 1').first();
          await env.LOGS.head('__healthcheck__');
          await env.DB.prepare('SELECT id FROM auth_accounts LIMIT 1').first();
          const active=await env.DB.prepare('SELECT version FROM liveops_active WHERE singleton=1').first();
          if(!active)throw new Rejected(503,'operations_unavailable');
          const response = reply({status:'ok',service:'hellscript-player-logs',schemaVersion:1,backend:'sites',authentication:'local',operations:true});
          return request.method === 'HEAD' ? new Response(null,response) : response;
        }
        if(path.startsWith('/internal/migration/'))return await migration(request,env);
        if(path.startsWith('/v1/')||path.startsWith('/auth/')||path.startsWith('/ops/api/'))await initialize(env);
        if(path.startsWith('/v1/auth/')||path.startsWith('/auth/google/'))return await auth(request,env);
        if(path==='/v1/telemetry/session')return request.method==='GET'?reply(await authenticate(request,env)):reply({error:'method'},405,{Allow:'GET'});
        if(path.startsWith('/v1/liveops/')||path.startsWith('/ops/api/'))return await liveops(request,env);
        if(path==='/ops'||path==='/ops/'||Object.hasOwn(OPS_ASSETS,path)){
          if(!['GET','HEAD'].includes(request.method))return reply({error:'method'},405,{Allow:'GET, HEAD'});
          const name=path==='/ops'||path==='/ops/'?'/ops/index.html':path;
          const asset=OPS_ASSETS[name];if(!asset)throw new Rejected(404,'route');
          return new Response(request.method==='HEAD'?null:asset.body,{headers:{'Content-Type':asset.type,'Cache-Control':'no-store',
            'X-Content-Type-Options':'nosniff','Referrer-Policy':'no-referrer','Content-Security-Policy':"default-src 'none'; script-src 'self'; style-src 'self'; connect-src 'self'; img-src 'self' data:; frame-ancestors 'none'"}});
        }
        if (path === '/v1/combat-runs') return request.method === 'POST' ? await ingest(request,env) : reply({error:'method'},405,{Allow:'POST'});
        if (path === '/' && ['GET','HEAD'].includes(request.method)) {
          const html = '<!doctype html><html lang="ko"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">' +
            '<title>HELLSCRIPT 플레이 로그</title><style>body{background:#10151e;color:#e7edf5;font:17px/1.7 system-ui;max-width:700px;margin:12vh auto;padding:24px}a{color:#8ac6ff}h1{font-size:30px}</style>' +
            '<main><h1>HELLSCRIPT 플레이 로그</h1><p>인증된 전투 기록을 보관하는 서버입니다.<br>Authenticated combat log receiver.</p>' +
            '<p><a href="/ops">운영 도구 · Operations</a></p><p><a href="/healthz">서버 상태 확인 · Server health</a></p><p>플레이 기록은 공개 조회할 수 없습니다.<br>Player records are private.</p></main></html>';
          return new Response(request.method === 'HEAD' ? null : html,{headers:{
            'Content-Type':'text/html; charset=utf-8','Cache-Control':'no-store',
            'Content-Security-Policy':"default-src 'none'; style-src 'unsafe-inline'; frame-ancestors 'none'",
            'X-Content-Type-Options':'nosniff','Referrer-Policy':'no-referrer'
          }});
        }
        return reply({error:'route'},404);
      } catch (error) {
        // Fixed diagnostic codes only: no token, request body or identity logs.
        return reply({error:error instanceof Rejected?error.code:'storage_unavailable',...(error instanceof Rejected&&error.details?{details:error.details}:{})},error instanceof Rejected?error.status:503);
      }
    }
  };
}
export default createHandler();
