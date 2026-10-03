import {Rejected,fields,integer,sha256,challenge,randomToken,equal,reply,database,jsonBody} from './common.js';

export const SESSION_SECONDS=43200;
export const TOKEN=/^[A-Za-z0-9_-]{43}(?![\s\S])/;
export const ID=/^[a-f0-9]{32}(?![\s\S])/;
export const PASSWORD_ITERATIONS=600000;

export function username(value) {
  if(typeof value!=='string'||!/^[A-Za-z0-9_]{3,24}(?![\s\S])/.test(value))throw new Rejected(400,'credential_format');
  return value.toLowerCase();
}
export function validPassword(value) {
  return typeof value==='string'&&Array.from(value).length>=15&&Array.from(value).length<=128&&
    new TextEncoder().encode(value).length<=512&&!/[\u0000-\u001f\u007f-\u009f]/.test(value)&&
    !Array.from(value).some(c=>/^[\uD800-\uDFFF]$/.test(c));
}
export async function passwordHash(password,salt) {
  if(!TOKEN.test(salt))throw new Rejected(503,'password_configuration');
  try {
    const key=await crypto.subtle.importKey('raw',new TextEncoder().encode(password),'PBKDF2',false,['deriveBits']);
    const raw=Uint8Array.from(atob(salt.replaceAll('-','+').replaceAll('_','/')+'='),c=>c.charCodeAt(0));
    const result=await crypto.subtle.deriveBits({name:'PBKDF2',hash:'SHA-256',salt:raw,iterations:PASSWORD_ITERATIONS},key,256);
    return Array.from(new Uint8Array(result),b=>b.toString(16).padStart(2,'0')).join('');
  }catch{throw new Rejected(503,'password_kdf_unavailable');} // Never lower the work factor to fit a host.
}
export async function authRate(db,bucket,at,limit) {
  await db.batch([
    db.prepare('DELETE FROM auth_rates WHERE start<?').bind(at-120),
    db.prepare('INSERT INTO auth_rates VALUES(?,?,1) ON CONFLICT(bucket) DO UPDATE SET count=CASE WHEN start<=excluded.start-60 THEN 1 ELSE count+1 END,start=CASE WHEN start<=excluded.start-60 THEN excluded.start ELSE start END').bind(bucket,at)
  ]);
  const row=await db.prepare('SELECT count FROM auth_rates WHERE bucket=?').bind(bucket).first();
  if(row.count>limit)throw new Rejected(429,'rate_limited');
}
export async function accountSession(request,env,at) {
  const value=request.headers.get('Authorization')||'';
  if(!value.startsWith('Bearer ')||!TOKEN.test(value.slice(7)))throw new Rejected(401,'authentication');
  const hash=await sha256(value.slice(7));
  const row=await database(env).prepare('SELECT s.account_id,s.expires,a.identity_hash,c.username_key FROM auth_sessions s JOIN auth_accounts a ON a.id=s.account_id LEFT JOIN auth_credentials c ON c.account_id=a.id WHERE s.token_hash=? AND s.expires>?').bind(hash,at).first();
  if(!row)throw new Rejected(401,'authentication');
  return {...row,tokenHash:hash};
}
export async function sessionReply(db,accountId,token,expires) {
  const row=await db.prepare('SELECT a.identity_hash,c.username_key FROM auth_accounts a LEFT JOIN auth_credentials c ON c.account_id=a.id WHERE a.id=?').bind(accountId).first();
  if(!row)throw new Rejected(401,'authentication');
  return reply({accountId,displayName:row.username_key||'Google',accessToken:token,expiresAt:expires,
    googleLinked:row.identity_hash.startsWith('local:')?0:1});
}

// An existing application owns its auth. No recovery by UID, email collection or cloud-save writes.
export async function localAccountRoute(request,env,at) {
  const path=new URL(request.url).pathname,db=database(env);
  if(path==='/v1/accounts/config'&&request.method==='GET')return reply({localEnabled:env.HELLSCRIPT_LOCAL_ACCOUNTS_ENABLED==='true',passwordMinimum:15,sessionSeconds:SESSION_SECONDS});
  if(path==='/v1/accounts/session'&&['GET','DELETE'].includes(request.method)) {
    const session=await accountSession(request,env,at);
    if(request.method==='DELETE'){
      await db.prepare('DELETE FROM auth_sessions WHERE token_hash=?').bind(session.tokenHash).run();return reply({signedOut:true});
    }
    return sessionReply(db,session.account_id,request.headers.get('Authorization').slice(7),session.expires);
  }
  if(!['/v1/accounts/register','/v1/accounts/login'].includes(path)||request.method!=='POST')throw new Rejected(404,'route');
  if(path.endsWith('/register')&&env.HELLSCRIPT_LOCAL_ACCOUNTS_ENABLED!=='true')throw new Rejected(503,'local_accounts_disabled');
  const source=request.headers.get('CF-Connecting-IP')||'shared';
  await authRate(db,'local:all',at,60);
  await authRate(db,'local:ip:'+await sha256(source.length<=64?source:'shared'),at,10);
  const v=await jsonBody(request);fields(v,['username','password','requestId','proof']);
  const name=username(v.username);
  if(!validPassword(v.password)||!ID.test(v.requestId)||!TOKEN.test(v.proof))throw new Rejected(400,'credential_format');
  await authRate(db,'local:name:'+await sha256(name),at,5);
  const requestHash=await sha256(v.requestId),payloadHash=await sha256(name+'\0'+v.password+'\0'+v.proof+'\0'+path);
  const token=await challenge('hellscript-local-session-v1\0'+v.requestId+'\0'+v.proof),tokenHash=await sha256(token);
  async function replay() {
    const row=await db.prepare('SELECT r.*,s.expires AS session_expires FROM auth_local_requests r LEFT JOIN auth_sessions s ON s.token_hash=r.session_hash WHERE r.request_hash=?').bind(requestHash).first();
    if(!row)return null;
    if(!equal(row.payload_hash,payloadHash)||!equal(row.session_hash,tokenHash)||row.expires<=at||!(row.session_expires>at))throw new Rejected(401,'credentials_rejected');
    return sessionReply(db,row.account_id,token,row.session_expires);
  }
  const prior=await replay();if(prior)return prior;
  const existing=await db.prepare('SELECT * FROM auth_credentials WHERE username_key=?').bind(name).first();
  const salt=existing?.salt||randomToken(),hash=await passwordHash(v.password,salt);
  const register=path.endsWith('/register');
  if((register&&existing)||(!register&&(!existing||existing.kdf!=='pbkdf2-sha256-600000'||!equal(hash,existing.password_hash))))throw new Rejected(401,'credentials_rejected');
  const accountId=register?crypto.randomUUID().replaceAll('-',''):existing.account_id,expires=at+SESSION_SECONDS;
  const inserts=register?[
    db.prepare('INSERT INTO auth_accounts(id,identity_hash,created) VALUES(?,?,?)').bind(accountId,'local:'+accountId,at),
    db.prepare('INSERT INTO auth_credentials(account_id,username_key,salt,password_hash,kdf) VALUES(?,?,?,?,?)').bind(accountId,name,salt,hash,'pbkdf2-sha256-600000')
  ]:[];
  try {
    await db.batch([
      db.prepare('DELETE FROM auth_local_requests WHERE expires<=?').bind(at),
      db.prepare('DELETE FROM auth_sessions WHERE expires<=?').bind(at),
      ...inserts,
      db.prepare('INSERT INTO auth_local_requests VALUES(?,?,?,?,?)').bind(requestHash,payloadHash,accountId,tokenHash,at+300),
      db.prepare('DELETE FROM auth_sessions WHERE account_id=?').bind(accountId),
      db.prepare('INSERT INTO auth_sessions VALUES(?,?,?)').bind(tokenHash,accountId,expires)
    ]);
  }catch(error) {
    const repeated=await replay();if(repeated)return repeated;
    if(register&&await db.prepare('SELECT account_id FROM auth_credentials WHERE username_key=?').bind(name).first())throw new Rejected(401,'credentials_rejected');
    throw error;
  }
  return sessionReply(db,accountId,token,expires);
}
