// Sites Worker: durable observations, never an authority for rewards or saves.
const MAX_BODY = 16 * 1024 * 1024;
const ID = /^[0-9a-f]{32}$/;
const object = v => v !== null && typeof v === 'object' && !Array.isArray(v);
const integer = (v, min, max) => Number.isSafeInteger(v) && v >= min && v <= max;
const text = (v, max) => typeof v === 'string' && Array.from(v).length <= max;
class Rejected extends Error {
  constructor(status, code) { super(code); this.status = status; this.code = code; }
}
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
const SCHEMA = [
  'CREATE TABLE IF NOT EXISTS schema_state (id INTEGER PRIMARY KEY CHECK(id=1),version INTEGER NOT NULL)',
  'INSERT OR IGNORE INTO schema_state VALUES(1,1)',
  'CREATE TABLE IF NOT EXISTS runs (' +
    'account_id TEXT NOT NULL,run_id TEXT NOT NULL,payload_hash TEXT NOT NULL,identity_kind TEXT NOT NULL,' +
    'received_utc_ms INTEGER NOT NULL,local_account_id TEXT NOT NULL,hero_id TEXT NOT NULL,stage INTEGER NOT NULL,' +
    'outcome TEXT NOT NULL,finish TEXT,started_utc_ms INTEGER NOT NULL,completed_utc_ms INTEGER NOT NULL,' +
    'simulation_seconds REAL NOT NULL,attendance_seconds REAL NOT NULL,earned_gold INTEGER NOT NULL,kills INTEGER NOT NULL,' +
    'boss_defeated INTEGER NOT NULL,equipment_collected INTEGER NOT NULL,liveops_version INTEGER,liveops_config_hash TEXT,' +
    'signals_json TEXT NOT NULL,object_key TEXT NOT NULL,payload_bytes INTEGER NOT NULL,PRIMARY KEY(account_id,run_id))',
  'CREATE INDEX IF NOT EXISTS runs_account_time ON runs(account_id,received_utc_ms)',
  'CREATE INDEX IF NOT EXISTS runs_stage_outcome ON runs(stage,outcome)',
  'CREATE TABLE IF NOT EXISTS audit (received_utc_ms INTEGER NOT NULL,account_id TEXT NOT NULL,run_id TEXT NOT NULL,code TEXT NOT NULL,payload_hash TEXT NOT NULL)',
  'CREATE TABLE IF NOT EXISTS upload_rates (account_id TEXT NOT NULL,window INTEGER NOT NULL,count INTEGER NOT NULL,bytes INTEGER NOT NULL,PRIMARY KEY(account_id,window))',
  'CREATE INDEX IF NOT EXISTS upload_rates_window ON upload_rates(window)'
];
function reply(body, status = 200, extra = {}) {
  return Response.json(body, {status, headers: {
    'Cache-Control': 'no-store','X-Content-Type-Options': 'nosniff','Referrer-Policy': 'no-referrer',...extra
  }});
}
function origin(env) {
  try {
    const url = new URL(env.HELLSCRIPT_AUTH_ORIGIN);
    if (url.protocol === 'https:' && url.origin === env.HELLSCRIPT_AUTH_ORIGIN) return url.origin;
  } catch {}
  throw new Rejected(503, 'authentication_service_unavailable');
}
async function boundedBody(request) {
  if ((request.headers.get('Content-Type') || '').split(';')[0].trim().toLowerCase() !== 'application/json') throw new Rejected(415, 'content_type');
  const length = request.headers.get('Content-Length');
  if (length !== null && (!/^[0-9]{1,10}$/.test(length) || Number(length) > MAX_BODY)) throw new Rejected(Number(length) > MAX_BODY ? 413 : 400, 'body_budget');
  if (!request.body) throw new Rejected(400, 'json');
  const reader = request.body.getReader(), chunks = [];
  let total = 0;
  try {
    while (true) {
      const {value, done} = await reader.read();
      if (done) break;
      total += value.byteLength;
      if (total > MAX_BODY) { await reader.cancel(); throw new Rejected(413, 'body_budget'); }
      chunks.push(value);
    }
  } finally { reader.releaseLock(); }
  if (!total || (length !== null && Number(length) !== total)) throw new Rejected(400, 'incomplete_body');
  const bytes = new Uint8Array(total);
  let offset = 0;
  for (const chunk of chunks) { bytes.set(chunk, offset); offset += chunk.byteLength; }
  return bytes;
}
async function sha256(bytes) {
  const hash = await crypto.subtle.digest('SHA-256', bytes);
  return Array.from(new Uint8Array(hash), b => b.toString(16).padStart(2, '0')).join('');
}
export function createHandler({authFetch = (url, init) => globalThis.fetch(url, init), now = Date.now} = {}) {
  const initialized = new WeakMap();
  async function initialize(env) {
    if (!env.DB || !env.LOGS) throw new Rejected(503, 'storage_unavailable');
    if (!initialized.has(env.DB)) {
      const pending = env.DB.batch(SCHEMA.map(sql => env.DB.prepare(sql))).then(async () => {
        const state = await env.DB.prepare('SELECT version FROM schema_state WHERE id=1').first();
        if (state?.version !== 1) throw new Rejected(503, 'storage_schema');
      });
      initialized.set(env.DB, pending);
      pending.catch(() => initialized.delete(env.DB));
    }
    await initialized.get(env.DB);
  }
  async function authenticate(request, env) {
    const authorization = request.headers.get('Authorization') || '';
    if (!/^Bearer [A-Za-z0-9_-]{32,256}$/.test(authorization)) throw new Rejected(401, 'authentication');
    const controller = new AbortController(), timeout = setTimeout(() => controller.abort(), 8000);
    try {
      const upstream = await authFetch(origin(env) + '/v1/telemetry/session', {
        headers: {Authorization: authorization,Accept: 'application/json'},redirect: 'manual',signal: controller.signal
      });
      if ([401,403].includes(upstream.status)) throw new Rejected(401, 'authentication');
      if (upstream.status === 429) throw new Rejected(429, 'rate_limited');
      if (!upstream.ok) throw new Rejected(503, 'authentication_service_unavailable');
      const raw = await upstream.text();
      if (raw.length > 4096) throw new Rejected(503, 'authentication_service_unavailable');
      const identity = JSON.parse(raw);
      if (!object(identity) || !['qa','google'].includes(identity.kind) || typeof identity.accountId !== 'string' ||
          !/^[A-Za-z0-9_.:-]{1,132}$/.test(identity.accountId) || !identity.accountId.startsWith(identity.kind + ':')) {
        throw new Rejected(503, 'authentication_service_unavailable');
      }
      return identity;
    } catch (error) {
      if (error instanceof Rejected) throw error;
      const kind = String(error?.message).startsWith('Illegal invocation') ? 'illegal_invocation' :
        error?.name === 'AbortError' ? 'timeout' :
        /different request|cross.request|I\/O/.test(String(error?.message)) ? 'request_context' :
        /network|Network|disallow|allow|fetch|Fetch|URL/.test(String(error?.message)) ? 'network' : 'transport';
      const errorType = ['TypeError','ReferenceError','SyntaxError','Error'].includes(error?.name) ? error.name : 'unknown';
      const line = String(error?.stack).split('\n').slice(1).join('\n').match(/(?:index|_worker)\.js:\d+:\d+/)?.[0] || '';
      console.error('telemetry_auth_failed', kind, errorType, line);
      throw new Rejected(503, 'authentication_service_unavailable');
    } finally { clearTimeout(timeout); }
  }
  async function ingest(request, env) {
    const identity = await authenticate(request, env), bytes = await boundedBody(request);
    let record;
    try { record = JSON.parse(new TextDecoder('utf-8',{fatal:true}).decode(bytes)); }
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
          origin(env); await initialize(env);
          await env.DB.prepare('SELECT run_id FROM runs LIMIT 1').first();
          await env.LOGS.head('__healthcheck__');
          const response = reply({status:'ok',service:'hellscript-player-logs',schemaVersion:1});
          return request.method === 'HEAD' ? new Response(null,response) : response;
        }
        if (path === '/v1/combat-runs') return request.method === 'POST' ? await ingest(request,env) : reply({error:'method'},405,{Allow:'POST'});
        if (path === '/' && ['GET','HEAD'].includes(request.method)) {
          const html = '<!doctype html><html lang="ko"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">' +
            '<title>HELLSCRIPT 플레이 로그</title><style>body{background:#10151e;color:#e7edf5;font:17px/1.7 system-ui;max-width:700px;margin:12vh auto;padding:24px}a{color:#8ac6ff}h1{font-size:30px}</style>' +
            '<main><h1>HELLSCRIPT 플레이 로그</h1><p>인증된 전투 기록을 보관하는 서버입니다.<br>Authenticated combat log receiver.</p>' +
            '<p><a href="/healthz">서버 상태 확인 · Server health</a></p><p>플레이 기록은 공개 조회할 수 없습니다.<br>Player records are private.</p></main></html>';
          return new Response(request.method === 'HEAD' ? null : html,{headers:{
            'Content-Type':'text/html; charset=utf-8','Cache-Control':'no-store',
            'Content-Security-Policy':"default-src 'none'; style-src 'unsafe-inline'; frame-ancestors 'none'",
            'X-Content-Type-Options':'nosniff','Referrer-Policy':'no-referrer'
          }});
        }
        return reply({error:'route'},404);
      } catch (error) {
        // Fixed diagnostic codes only: no token, request body or identity logs.
        return reply({error:error instanceof Rejected?error.code:'storage_unavailable'},error instanceof Rejected?error.status:503);
      }
    }
  };
}
export default createHandler();
