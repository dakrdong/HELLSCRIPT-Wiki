import assert from 'node:assert/strict';
import {test} from 'node:test';
import {D1,R2} from './test_support.mjs';
import {readFileSync} from 'node:fs';
import {fileURLToPath} from 'node:url';
import {createHash} from 'node:crypto';
import {createHandler,validate} from './_worker.js';

// Use the existing Python receiver's fixture, including its Korean event text.
const generated=readFileSync(new URL('./fixtures/combat.json',import.meta.url),'utf8');
const fixture=()=>JSON.parse(generated);
const TOKEN='t'.repeat(43);
function setup(identity='google:'+'d'.repeat(32)){
  const env={DB:new D1(),LOGS:new R2(),HELLSCRIPT_AUTH_ORIGIN:'https://logs.example.com'};
  let valid=true,at=1700000000000;
  const dependencies={
    authenticate:async(request)=>{
      if(!valid||request.headers.get('Authorization')!=='Bearer '+TOKEN){
        const {Rejected}=await import('./common.js');throw new Rejected(401,'authentication');
      }
      return {accountId:identity,kind:identity.split(':')[0]};
    },now:()=>at
  };
  const handler=createHandler(dependencies);
  const send=(value=fixture(),options={})=>{
    const raw=options.raw??JSON.stringify(value);
    return handler.fetch(new Request('https://logs.example.com/v1/combat-runs',{
      method:'POST',headers:{Authorization:'Bearer '+TOKEN,'Content-Type':'application/json',...options.headers},body:raw
    }),env);
  };
  return {env,handler,dependencies,send,revoke:()=>{valid=false;},advance:()=>{at+=60000;}};
}
test('original UTF-8 bytes have a durable receipt across Worker restart',async()=>{
  const s=setup(),raw='\n'+generated.trim()+'\n';
  const first=await s.send(null,{raw}),receipt=await first.json();
  assert.equal(first.status,200);
  assert.deepEqual(receipt,{accepted:true,runId:fixture().id,payloadHash:createHash('sha256').update(raw).digest('hex')});
  const restarted=createHandler(s.dependencies);
  const again=await restarted.fetch(new Request('https://logs.example.com/v1/combat-runs',{
    method:'POST',headers:{Authorization:'Bearer '+TOKEN,'Content-Type':'application/json'},body:raw}),s.env);
  assert.deepEqual(await again.json(),receipt);
  assert.equal(s.env.DB.rows().length,1);
  assert.equal(s.env.LOGS.objects.size,1);
  assert.equal([...s.env.LOGS.objects.values()][0].bytes.toString(),raw);
});
test('concurrent equal and conflicting payloads preserve exactly one accepted run',async()=>{
  const s=setup(),original=fixture(),changed=fixture();changed.journal.earnedGold=999;
  const replies=await Promise.all([s.send(original),s.send(original),s.send(changed)]);
  assert.equal(replies.filter(r=>r.status===200).length,2);
  assert.equal(replies.filter(r=>r.status===409).length,1);
  assert.equal(s.env.DB.rows()[0].earned_gold,23);
  assert.equal(s.env.DB.rows().length,1);
  assert.equal(s.env.LOGS.objects.size,1);
});
test('R2/D1 failures never acknowledge missing data and retries repair interrupted writes',async()=>{
  const s=setup();
  s.env.LOGS.failPut=true;assert.equal((await s.send()).status,503);assert.equal(s.env.DB.rows().length,0);
  s.env.LOGS.failPut=false;s.env.DB.failInsert=true;assert.equal((await s.send()).status,503);
  assert.equal(s.env.DB.rows().length,0);assert.equal(s.env.LOGS.objects.size,1);
  s.env.DB.failInsert=false;assert.equal((await s.send()).status,200);
  s.env.LOGS.objects.clear();assert.equal((await s.send()).status,200);
  assert.equal(s.env.LOGS.objects.size,1);assert.equal(s.env.DB.rows().length,1);
  s.env.LOGS.failHead=true;assert.equal((await s.send()).status,503);
});
test('revoked, anonymous and unknown credentials cannot parse or persist player data',async()=>{
  const s=setup();
  assert.equal((await s.send(null,{raw:'bad json',headers:{Authorization:''}})).status,401);
  assert.equal((await s.send(null,{raw:'bad json',headers:{Authorization:'Bearer '+'x'.repeat(43)}})).status,401);
  s.revoke();assert.equal((await s.send()).status,401);
  assert.equal(s.env.LOGS.objects.size,0);
});
test('authenticated server account owns the row independently of client identity',async()=>{
  const s=setup('qa:synthetic-test');
  const value=fixture();value.accountId='forged-other-player';value.journal.localAccountId='f'.repeat(32);
  assert.equal((await s.send(value)).status,200);
  assert.equal(s.env.DB.rows()[0].account_id,'qa:synthetic-test');
  assert.equal(s.env.DB.rows()[0].identity_kind,'qa');
  assert.equal(s.env.DB.rows()[0].local_account_id,'f'.repeat(32));
});
test('size, provenance, event sequence and idempotency boundaries reject unsafe payloads',async()=>{
  const s=setup();
  assert.equal((await s.send(null,{raw:'{}',headers:{'Content-Length':String(16*1024*1024+1)}})).status,413);
  assert.equal((await s.send(null,{raw:'not json'})).status,400);
  assert.equal((await s.send(fixture(),{headers:{'Idempotency-Key':'b'.repeat(32)}})).status,400);
  for(const mutate of [
    v=>{v.journal.trust='server_verified';},v=>{v.journal.events[0].sequence=2;},
    v=>{v.journal.events[0].seconds=2;},v=>{v.journal.kills=2;},
    v=>{v.journal.earnedGold=2**53;},v=>{v.journal.archived=true;}
  ]){
    const value=fixture();mutate(value);assert.equal((await s.send(value)).status,422);
  }
  assert.equal(s.env.LOGS.objects.size,0);
  assert.deepEqual(validate(fixture()),['CLIENT_OBSERVATION_UNVERIFIED']);
});
test('health and public routes expose no player record or reward mutation',async()=>{
  const s=setup();
  const get=path=>s.handler.fetch(new Request('https://logs.example.com'+path),s.env);
  assert.equal((await get('/healthz')).status,200);
  assert.equal((await get('/v1/combat-runs')).status,405);
  for(const path of ['/v1/combat-runs/example','/admin','/authority'])assert.equal((await get(path)).status,404);
  assert.equal((await (await get('/')).text()).includes('Player records are private.'),true);
});
test('per-account rate budget recovers next minute',async()=>{
  const s=setup();
  for(let i=0;i<120;i++)assert.equal((await s.send()).status,200);
  assert.equal((await s.send()).status,429);
  s.advance();assert.equal((await s.send()).status,200);
});
test('default handler validates QA credentials locally with no upstream server',async()=>{
  const s=setup(),original=globalThis.fetch;
  s.env.HELLSCRIPT_TELEMETRY_TOKENS=JSON.stringify({[TOKEN]:'local-qa'});
  try{
    globalThis.fetch=()=>{throw new Error('No upstream request is allowed');};
    const handler=createHandler();
    const send=()=>handler.fetch(new Request('https://logs.example.com/v1/combat-runs',{
      method:'POST',headers:{Authorization:'Bearer '+TOKEN,'Content-Type':'application/json'},body:JSON.stringify(fixture())}),s.env);
    assert.equal((await send()).status,200);
    s.env.HELLSCRIPT_TELEMETRY_TOKENS='{}';assert.equal((await send()).status,401);
  }finally{globalThis.fetch=original;}
});
test('ordinary game sessions are validated locally on every upload and revoke immediately',async()=>{
  const s=setup(),hash=createHash('sha256').update(TOKEN).digest('hex');
  s.env.DB.database.prepare('INSERT INTO auth_sessions VALUES(?,?,?)').run(hash,'e'.repeat(32),1700000040);
  const handler=createHandler({now:()=>1700000000000});
  const send=()=>handler.fetch(new Request('https://logs.example.com/v1/combat-runs',{
    method:'POST',headers:{Authorization:'Bearer '+TOKEN,'Content-Type':'application/json'},body:JSON.stringify(fixture())}),s.env);
  assert.equal((await send()).status,200);assert.equal(s.env.DB.rows()[0].account_id,'google:'+'e'.repeat(32));
  s.env.DB.database.prepare('DELETE FROM auth_sessions').run();assert.equal((await send()).status,401);
});
