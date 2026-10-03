import {Rejected,fields,integer,sha256,bodyBytes,parseJson,database,canonical,reply} from './common.js';
import {ID} from './accounts.js';
const KINDS=new Set(['session_start','session_end','activity','tutorial_step','rift_enter','currency_spent','progress_snapshot','collection_gap']);
const SOURCES=new Set(['session','town','rift','combat','tutorial','prologue','shop','potions','forge','other']);
export function validatePlayerEvent(v) {
  fields(v,['id','sessionId','kind','clientUtcMs','seconds','stage','step','amount','source','currency']);
  if(!ID.test(v.id)||!ID.test(v.sessionId)||!KINDS.has(v.kind)||!SOURCES.has(v.source)||
    !integer(v.clientUtcMs,0,Number.MAX_SAFE_INTEGER)||typeof v.seconds!=='number'||!Number.isFinite(v.seconds)||v.seconds<0||v.seconds>60||
    !integer(v.stage,0,100000)||!integer(v.step,0,100)||!integer(v.amount,0,2147483647)||!['none','gold','abyssal_coin','enhancement_stone','material'].includes(v.currency))throw new Rejected(422,'player_event');
  if(v.kind==='activity'&&(v.seconds<=0||!['town','rift','combat','tutorial'].includes(v.source))||
    v.kind==='currency_spent'&&(v.amount<1||!['shop','potions','forge','other'].includes(v.source))||
    v.kind==='tutorial_step'&&v.source!=='prologue'||v.kind==='rift_enter'&&v.stage<1||
    v.kind!=='activity'&&v.seconds!==0||v.kind==='currency_spent'&&v.currency==='none'||v.kind!=='currency_spent'&&v.currency!=='none')throw new Rejected(422,'player_event');
}
export async function ingestPlayerEvents(request,env,identity,at) {
  const bytes=await bodyBytes(request,65536),v=parseJson(bytes);fields(v,['version','events']);
  if(v.version!==1||!Array.isArray(v.events)||v.events.length<1||v.events.length>64)throw new Rejected(422,'player_event_batch');
  v.events.forEach(validatePlayerEvent);
  if(new Set(v.events.map(e=>e.id)).size!==v.events.length)throw new Rejected(422,'player_event_duplicate');
  const db=database(env),window=Math.floor(at/60000);
  await db.batch([
    db.prepare('INSERT INTO upload_rates VALUES(?,?,1,?) ON CONFLICT(account_id,window) DO UPDATE SET count=count+1,bytes=bytes+excluded.bytes').bind(identity.accountId,window,bytes.byteLength),
    db.prepare('DELETE FROM upload_rates WHERE window<?').bind(window-2)
  ]);
  const rate=await db.prepare('SELECT count,bytes FROM upload_rates WHERE account_id=? AND window=?').bind(identity.accountId,window).first();
  if(rate.count>120||rate.bytes>64*1024*1024)throw new Rejected(429,'rate_limited');
  const hashes=await Promise.all(v.events.map(e=>sha256(canonical(e))));
  const statements=v.events.map((e,i)=>db.prepare('INSERT INTO player_events VALUES(?,?,?,?,?,?,?,?,?,?,?,?,?,?) ON CONFLICT(account_id,event_id) DO UPDATE SET payload_hash=CASE WHEN payload_hash=excluded.payload_hash THEN payload_hash ELSE NULL END').bind(identity.accountId,e.id,e.sessionId,identity.kind,e.kind,e.clientUtcMs,at,e.seconds,e.stage,e.step,e.amount,e.source,hashes[i],e.currency));
  try{await db.batch(statements);}catch(error){
    for(let i=0;i<v.events.length;i++){
      const row=await db.prepare('SELECT payload_hash FROM player_events WHERE account_id=? AND event_id=?').bind(identity.accountId,v.events[i].id).first();
      if(row&&row.payload_hash!==hashes[i])throw new Rejected(409,'payload_conflict');
    }
    throw error;
  }
  return reply({accepted:true,eventIds:v.events.map(e=>e.id),payloadHash:await sha256(bytes)});
}
