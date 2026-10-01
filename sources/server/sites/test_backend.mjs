import assert from 'node:assert/strict';
import {test} from 'node:test';
import {readFileSync} from 'node:fs';
import {createHash} from 'node:crypto';
import {D1,R2} from './test_support.mjs';
import {createHandler} from './_worker.js';
import {verifiedIdentity,validReturnUri} from './auth.js';
import {randomToken,challenge,base64url,canonical,sha256,parseJson} from './common.js';
import {SCHEMA} from './schema.generated.js';
const ORIGIN='https://logs.example.com',AT=1700000000000,CLIENT='test.apps.googleusercontent.com',OPS='o'.repeat(43),QA='q'.repeat(43);
const env=(ready=true)=>({DB:new D1(ready),LOGS:new R2(),HELLSCRIPT_AUTH_ORIGIN:ORIGIN,HELLSCRIPT_OPS_ORIGIN:ORIGIN,
  HELLSCRIPT_GOOGLE_CLIENT_ID:CLIENT,HELLSCRIPT_GOOGLE_CLIENT_SECRET:'s'.repeat(32),HELLSCRIPT_OPS_TOKENS:JSON.stringify({[OPS]:'operator'}),HELLSCRIPT_TELEMETRY_TOKENS:JSON.stringify({[QA]:'qa'})});
const req=(path,method='GET',body,headers={})=>new Request(ORIGIN+path,{method,headers:{'Content-Type':'application/json',...headers},...(body!==undefined?{body:typeof body==='string'?body:JSON.stringify(body)}:{})});
function requests(e,handler=createHandler({now:()=>AT})) {return (path,method='GET',body,headers={})=>handler.fetch(req(path,method,body,headers),e);}
async function operator(e){
  const send=requests(e),r=await send('/ops/api/session','POST',{token:OPS},{Origin:ORIGIN});assert.equal(r.status,200);
  const body=await r.json(),cookie=r.headers.get('Set-Cookie').split(';')[0];
  assert.match(r.headers.get('Set-Cookie'),/HttpOnly; Secure; SameSite=Strict/);
  return {send,body,headers:{Cookie:cookie,Origin:ORIGIN,'X-CSRF-Token':body.csrfToken}};
}
const keys=await crypto.subtle.generateKey({name:'RSASSA-PKCS1-v1_5',modulusLength:2048,publicExponent:new Uint8Array([1,0,1]),hash:'SHA-256'},true,['sign','verify']);
const jwk={...await crypto.subtle.exportKey('jwk',keys.publicKey),kid:'test-key',alg:'RS256',use:'sig'};
async function signed(claims,header={alg:'RS256',kid:jwk.kid}){
  const encode=v=>base64url(new TextEncoder().encode(JSON.stringify(v))),raw=encode(header)+'.'+encode(claims);
  return raw+'.'+base64url(new Uint8Array(await crypto.subtle.sign('RSASSA-PKCS1-v1_5',keys.privateKey,new TextEncoder().encode(raw))));
}
test('Google identity verifies real RSA signatures, issuer, audience, expiry and nonce',async()=>{
  const claims={iss:'https://accounts.google.com',aud:CLIENT,iat:AT/1000-1,exp:AT/1000+300,nonce:'nonce',sub:'stable-subject',email:'synthetic@example.com',email_verified:true};
  const expected=createHash('sha256').update('https://accounts.google.com\0stable-subject').digest('hex');
  assert.deepEqual(await verifiedIdentity(await signed(claims),{keys:[jwk]},CLIENT,'nonce',AT/1000),{identity:expected,name:claims.email});
  for(const changed of [{iss:'https://attacker.example'},{aud:'other-client'},{exp:AT/1000},{nonce:'wrong'},{azp:'other-client'},{sub:''}])
    await assert.rejects(()=>signed({...claims,...changed}).then(t=>verifiedIdentity(t,{keys:[jwk]},CLIENT,'nonce',AT/1000)),/google_identity/);
  await assert.rejects(()=>signed(claims,{alg:'none',kid:jwk.kid}).then(t=>verifiedIdentity(t,{keys:[jwk]},CLIENT,'nonce',AT/1000)),/google_identity/);
  const token=await signed(claims);await assert.rejects(()=>verifiedIdentity(token.slice(0,-2)+'xx',{keys:[jwk]},CLIENT,'nonce',AT/1000),/google_identity/);
});
test('existing Google account ID survives OAuth, cookie/PKCE proofs, replay and revocation',async()=>{
  const e=env(),id='a'.repeat(32),identity=await sha256('https://accounts.google.com\0stable-subject');
  e.DB.database.prepare('INSERT INTO auth_accounts VALUES(?,?,?)').run(id,identity,AT/1000-100);
  let upstreamNonce,providerCalls=0;
  const handler=createHandler({now:()=>AT,fetcher:async(url,init)=>{
    assert.equal(init.redirect,'manual');providerCalls++;
    if(url==='https://www.googleapis.com/oauth2/v3/certs')return Response.json({keys:[jwk]});
    assert.equal(url,'https://oauth2.googleapis.com/token');const form=new URLSearchParams(init.body);
    assert.equal(form.get('redirect_uri'),ORIGIN+'/auth/google/callback');assert.equal(form.get('client_id'),CLIENT);
    return Response.json({scope:'openid https://www.googleapis.com/auth/userinfo.email',id_token:await signed({iss:'accounts.google.com',aud:CLIENT,iat:AT/1000,exp:AT/1000+300,nonce:upstreamNonce,sub:'stable-subject'})});
  }}),send=requests(e,handler),verifier=randomToken(),state=randomToken();
  const start=await send('/v1/auth/google/start','POST',{challenge:await challenge(verifier),state,returnUri:'http://127.0.0.1:43210/auth/google/'});
  assert.equal(start.status,200);const beginUrl=new URL((await start.json()).authorizationUrl);
  const begin=await send(beginUrl.pathname+beginUrl.search),target=new URL(begin.headers.get('Location'));
  upstreamNonce=target.searchParams.get('nonce');assert.equal(target.searchParams.get('code_challenge_method'),'S256');
  assert.equal((await send(beginUrl.pathname+beginUrl.search)).status,400);
  const callback='/auth/google/callback?state='+target.searchParams.get('state')+'&code=synthetic-provider-code';
  assert.equal((await send(callback)).status,400);assert.equal(providerCalls,0);
  const cookie=begin.headers.get('Set-Cookie').split(';')[0],returned=await send(callback,'GET',undefined,{Cookie:cookie});
  assert.equal(returned.status,302);const code=new URL(returned.headers.get('Location')).searchParams.get('code');assert.ok(code);
  assert.equal((await send(callback,'GET',undefined,{Cookie:cookie})).status,400);
  assert.equal((await send('/v1/auth/google/exchange','POST',{code,verifier:randomToken(),state})).status,401);
  const exchanges=await Promise.all([send('/v1/auth/google/exchange','POST',{code,verifier,state}),send('/v1/auth/google/exchange','POST',{code,verifier,state})]);
  assert.deepEqual(exchanges.map(r=>r.status).sort(),[200,401]);const session=await exchanges.find(r=>r.status===200).json();assert.equal(session.accountId,id);
  assert.equal((await send('/v1/telemetry/session','GET',undefined,{Authorization:'Bearer '+session.accessToken})).status,200);
  assert.equal((await send('/v1/auth/session','DELETE',undefined,{Authorization:'Bearer '+session.accessToken})).status,200);
  assert.equal((await send('/v1/telemetry/session','GET',undefined,{Authorization:'Bearer '+session.accessToken})).status,401);
  assert.equal(e.DB.database.prepare('SELECT COUNT(*) AS count FROM auth_accounts').get().count,1);
});
test('strict JSON, redirect and account/session separation reject unsafe requests',async()=>{
  for(const raw of ['{"x":1,"x":2}','{"x":1e999}','[1,]','{"x":"bad\u0001"}'])assert.throws(()=>parseJson(raw),/json/);
  assert.equal(parseJson('{"__proto__":{"x":1}}').__proto__.x,1);assert.equal(Object.getPrototypeOf(parseJson('{}')),null);
  for(const uri of ['http://localhost:43210/auth/google/','https://attacker.example/','http://127.0.0.1:80/auth/google/','hellscript://auth/google?next=x'])assert.equal(validReturnUri(uri),false);
  const e=env(),send=requests(e);assert.equal((await send('/v1/auth/session','GET',undefined,{Authorization:'Bearer '+QA})).status,401);
  assert.equal((await send('/ops/api/session','POST',{token:QA},{Origin:ORIGIN})).status,401);
  assert.equal((await send('/v1/auth/google/start','POST',{challenge:randomToken(),state:randomToken(),returnUri:'https://attacker.example'})).status,400);
});
test('operator cookie, CSRF, same-origin and credential revocation protect all writes',async()=>{
  const e=env(),o=await operator(e);
  assert.equal((await o.send('/ops/api/state')).status,401);
  const value={expectedDraftRevision:0,baseVersion:0,config:{rift:SCHEMA.templates.rift,stageOverrides:[],firstClearRewards:[]},reason:'Synthetic change'};
  assert.equal((await o.send('/ops/api/draft','POST',value,{...o.headers,Origin:'https://attacker.example'})).status,403);
  assert.equal((await o.send('/ops/api/draft','POST',value,{...o.headers,'X-CSRF-Token':'wrong'})).status,403);
  assert.equal((await o.send('/ops/api/state','GET',undefined,o.headers)).status,200);
  e.HELLSCRIPT_OPS_TOKENS='{}';assert.equal((await o.send('/ops/api/state','GET',undefined,o.headers)).status,503);
  assert.equal(e.DB.database.prepare('SELECT revision FROM liveops_draft').get().revision,0);
});
test('D1 CAS commits one concurrent draft, publishes immutable history and rolls back',async()=>{
  const e=env(),o=await operator(e),config={rift:{...SCHEMA.templates.rift,goldMultiplier:2},stageOverrides:[],firstClearRewards:[]};
  const value={expectedDraftRevision:0,baseVersion:0,config,reason:'Synthetic draft'};
  const results=await Promise.all([o.send('/ops/api/draft','POST',value,o.headers),o.send('/ops/api/draft','POST',value,o.headers)]);
  assert.deepEqual(results.map(r=>r.status).sort(),[200,409]);
  assert.equal(e.DB.database.prepare('SELECT COUNT(*) AS count FROM liveops_audit').get().count,1);
  const publish=await o.send('/ops/api/publish','POST',{expectedVersion:0,expectedDraftRevision:1,reason:'Synthetic publish'},o.headers);assert.equal(publish.status,200);
  const current=await (await o.send('/v1/liveops/current')).json();assert.equal(current.version,1);assert.equal(current.configHash,await sha256(current.configJson));
  assert.equal((await o.send('/ops/api/publish','POST',{expectedVersion:0,expectedDraftRevision:1,reason:'Stale publish'},o.headers)).status,409);
  assert.throws(()=>e.DB.database.prepare('DELETE FROM liveops_releases WHERE version=1').run(),/release_immutable/);
  assert.throws(()=>e.DB.database.prepare('UPDATE liveops_audit SET reason=?').run('tamper'),/audit_immutable/);
  assert.equal((await o.send('/ops/api/rollback','POST',{expectedVersion:1,expectedDraftRevision:2,targetVersion:0,reason:'Synthetic rollback'},o.headers)).status,200);
  const restored=await (await o.send('/v1/liveops/current')).json();assert.equal(restored.version,2);assert.equal(restored.config.rift.goldMultiplier,1);
});
test('operations validator shares the Python default contract and integer bounds',async()=>{
  const fixture=JSON.parse(readFileSync(new URL('./fixtures/liveops-contract.json',import.meta.url),'utf8'));
  const e=env(),o=await operator(e);
  assert.equal((await o.send('/ops/api/validate','POST',{config:fixture.defaultConfig},o.headers)).status,200);
  const wrong=structuredClone(fixture.defaultConfig);wrong.rift.bossEquipmentCount=2.5;
  assert.equal((await o.send('/ops/api/validate','POST',{config:wrong},o.headers)).status,422);
});
test('migration requires the one-time secret and seals all write routes on completion',async()=>{
  const e=env(false),send=requests(e),secret=randomToken(),id='c'.repeat(32),headers={Authorization:'Bearer '+secret,'X-Migration-ID':id};e.HELLSCRIPT_MIGRATION_TOKEN=secret;
  assert.equal((await send('/v1/auth/config')).status,503);
  assert.equal((await send('/internal/migration/part','POST',{})).status,404);
  const raw=canonical({rift:SCHEMA.templates.rift,stageOverrides:[],firstClearRewards:[]}),hash=await sha256(raw);
  const data={liveops_releases:[{version:0,schema_version:1,config_json:raw,config_hash:hash,published_utc_ms:0,operator:'system',reason:'Initial config',rollback_of:null}],liveops_active:[{singleton:1,version:0}],liveops_draft:[{singleton:1,revision:0,base_version:0,config_json:raw,config_hash:hash,reason:'Initial config',updated_utc_ms:0,operator:'system'}]};
  const parts=[];for(const [table,rows] of Object.entries(data)){
    const part=table+'-0000',body=JSON.stringify({part,table,rows}),response=await send('/internal/migration/part','POST',body,headers);assert.equal(response.status,200);
    const accepted=await response.json();parts.push({part,hash:accepted.hash,rows:rows.length});
    assert.equal((await send('/internal/migration/part','POST',body,headers)).status,200);
    assert.equal((await send('/internal/migration/part','POST',body+' ',headers)).status,409);
  }
  const backups=[];for(const name of ['accounts.sqlite','liveops.sqlite','telemetry.sqlite']){
    const bytes='synthetic-private-backup-'+name,hash=await sha256(bytes),response=await send('/internal/migration/backup/'+name,'POST',bytes,{...headers,'Content-Type':'application/octet-stream','X-Content-SHA256':hash});assert.equal(response.status,200);backups.push({name,hash,bytes:Buffer.byteLength(bytes)});
  }
  const counts={auth_accounts:0,auth_sessions:0,liveops_releases:1,liveops_active:1,liveops_draft:1,liveops_audit:0,ops_sessions:0,legacy_telemetry_rows:0,runs:0};
  assert.equal((await send('/internal/migration/complete','POST',{parts,backups,counts:{...counts,runs:1}},headers)).status,409);
  assert.equal((await send('/internal/migration/complete','POST',{parts,backups,counts},headers)).status,200);
  assert.equal((await send('/internal/migration/part','POST',{},headers)).status,404);
  assert.equal((await send('/healthz')).status,200);assert.equal((await send('/v1/auth/config')).status,200);
});
test('adopting existing Site schema preserves its stored run and needs no runtime DDL',()=>{
  const d=new D1();d.database.prepare('INSERT INTO audit VALUES(?,?,?,?,?)').run(1,'qa:old','a'.repeat(32),'PAYLOAD_CONFLICT','b'.repeat(64));
  d.database.exec(readFileSync(new URL('./drizzle/0000_chubby_wrecker.sql',import.meta.url),'utf8'));
  assert.equal(d.database.prepare('SELECT COUNT(*) AS count FROM audit').get().count,1);
  assert.doesNotMatch(readFileSync(new URL('./_worker.js',import.meta.url),'utf8'),/CREATE TABLE|railway\.app|authFetch/);
});
