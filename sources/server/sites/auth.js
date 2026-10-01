import {Rejected,fields,integer,sha256,challenge,randomToken,base64url,equal,reply,database,trustedOrigin,jsonBody,cookie,changed,tokenMap,parseJson} from './common.js';
const TOKEN=/^[A-Za-z0-9_-]{43}$/;
const COOKIE='__Host-hellscript_login';
const SESSION_SECONDS=43200;
export function validReturnUri(value) {
  if(value==='hellscript://auth/google')return true;
  const m=typeof value==='string'&&value.match(/^http:\/\/127\.0\.0\.1:([0-9]+)\/auth\/google\/$/);
  return !!m&&String(Number(m[1]))===m[1]&&integer(Number(m[1]),1024,65535);
}
export function configured(env) {
  return /^[A-Za-z0-9_-]+\.apps\.googleusercontent\.com$/.test(env.HELLSCRIPT_GOOGLE_CLIENT_ID||'')&&
    typeof env.HELLSCRIPT_GOOGLE_CLIENT_SECRET==='string'&&env.HELLSCRIPT_GOOGLE_CLIENT_SECRET.length>=16&&env.HELLSCRIPT_GOOGLE_CLIENT_SECRET.length<=512&&/^[\x21-\x7e]+$/.test(env.HELLSCRIPT_GOOGLE_CLIENT_SECRET);
}
function query(url) {
  if(url.search.length>8192||[...url.searchParams].length>24)throw new Rejected(400,'query_budget');
  const q=Object.create(null);for(const [k,v] of url.searchParams){if(Object.hasOwn(q,k))throw new Rejected(400,'duplicate_query');q[k]=v;}return q;
}
function bearer(request) {
  const value=request.headers.get('Authorization')||'';
  if(!value.startsWith('Bearer ')||!TOKEN.test(value.slice(7)))throw new Rejected(401,'authentication');return value.slice(7);
}
export async function telemetryIdentity(request,env,at=Date.now()) {
  const value=request.headers.get('Authorization')||'';
  if(!/^Bearer [A-Za-z0-9_-]{32,256}$/.test(value))throw new Rejected(401,'authentication');
  const supplied=value.slice(7),qa=tokenMap(env.HELLSCRIPT_TELEMETRY_TOKENS);
  for(const [token,account] of Object.entries(qa))if(equal(token,supplied))return {accountId:'qa:'+account,kind:'qa'};
  if(!TOKEN.test(supplied))throw new Rejected(401,'authentication');
  const row=await database(env).prepare('SELECT account_id,expires FROM auth_sessions WHERE token_hash=? AND expires>?').bind(await sha256(supplied),Math.floor(at/1000)).first();
  if(!row)throw new Rejected(401,'authentication');
  return {accountId:'google:'+row.account_id,kind:'google'};
}
function jwtPart(raw) {
  if(!/^[A-Za-z0-9_-]+$/.test(raw))throw new Rejected(401,'google_identity');
  return Uint8Array.from(atob(raw.replaceAll('-','+').replaceAll('_','/')+'='.repeat((4-raw.length%4)%4)),c=>c.charCodeAt(0));
}
export async function verifiedIdentity(idToken,jwks,clientId,nonce,now) {
  try {
    if(typeof idToken!=='string'||idToken.length>16384)throw new Error();
    const parts=idToken.split('.');if(parts.length!==3)throw new Error();
    const header=parseJson(jwtPart(parts[0])),claims=parseJson(jwtPart(parts[1]));
    if(header.alg!=='RS256'||typeof header.kid!=='string'||header.kid.length>256)throw new Error();
    const jwk=jwks.keys?.find(k=>k.kid===header.kid&&k.kty==='RSA'&&(!k.alg||k.alg==='RS256')&&(!k.use||k.use==='sig'));
    if(!jwk)throw new Error();
    const key=await crypto.subtle.importKey('jwk',jwk,{name:'RSASSA-PKCS1-v1_5',hash:'SHA-256'},false,['verify']);
    if(!await crypto.subtle.verify('RSASSA-PKCS1-v1_5',key,jwtPart(parts[2]),new TextEncoder().encode(parts[0]+'.'+parts[1])))throw new Error();
    if(!['accounts.google.com','https://accounts.google.com'].includes(claims.iss)||claims.aud!==clientId||
      (claims.azp!==undefined&&claims.azp!==clientId)||!integer(claims.exp,now+1,Number.MAX_SAFE_INTEGER)||
      !integer(claims.iat,0,now+60)||!equal(claims.nonce,nonce)||typeof claims.sub!=='string'||
      !/^[\x21-\x7e]{1,255}$/.test(claims.sub))throw new Error();
    let name=claims.email;
    if(claims.email_verified!==true||typeof name!=='string'||Array.from(name).length<3||Array.from(name).length>254||/[\x00-\x1f]/.test(name))name='Google';
    return {identity:await sha256('https://accounts.google.com\0'+claims.sub),name};
  }catch{throw new Rejected(401,'google_identity');}
}
export function createAuth({fetcher=(url,init)=>globalThis.fetch(url,init),now=Date.now}={}) {
  let certificateCache=null;
  async function external(url,init={}) {
    const controller=new AbortController(),timeout=setTimeout(()=>controller.abort(),10000);
    try {
      const response=await fetcher(url,{...init,redirect:'manual',signal:controller.signal});
      if(!response.ok)throw new Error();const raw=await response.text();if(raw.length>65536)throw new Error();return parseJson(raw);
    }catch{throw new Rejected(502,'google_verification_failed');}finally{clearTimeout(timeout);}
  }
  async function identity(code,attempt,env) {
    const token=await external('https://oauth2.googleapis.com/token',{method:'POST',
      headers:{'Content-Type':'application/x-www-form-urlencoded'},body:new URLSearchParams({
        code,client_id:env.HELLSCRIPT_GOOGLE_CLIENT_ID,client_secret:env.HELLSCRIPT_GOOGLE_CLIENT_SECRET,
        redirect_uri:trustedOrigin(env)+'/auth/google/callback',grant_type:'authorization_code',code_verifier:attempt.verifier}).toString()});
    const scopes=typeof token.scope==='string'?new Set(token.scope.split(/\s+/).filter(Boolean).map(s=>s==='email'?'https://www.googleapis.com/auth/userinfo.email':s)):null;
    if(!scopes||scopes.size!==2||!scopes.has('openid')||!scopes.has('https://www.googleapis.com/auth/userinfo.email'))throw new Rejected(502,'google_verification_failed');
    if(!certificateCache||certificateCache.expires<now()){
      const keys=await external('https://www.googleapis.com/oauth2/v3/certs');
      if(!Array.isArray(keys.keys)||keys.keys.length>32)throw new Rejected(502,'google_verification_failed');
      certificateCache={keys,expires:now()+300000};
    }
    return verifiedIdentity(token.id_token,certificateCache.keys,env.HELLSCRIPT_GOOGLE_CLIENT_ID,attempt.nonce,Math.floor(now()/1000));
  }
  async function rate(db,bucket,at,limit) {
    await db.batch([
      db.prepare('DELETE FROM auth_rates WHERE start<?').bind(at-120),
      db.prepare('INSERT INTO auth_rates VALUES(?,?,1) ON CONFLICT(bucket) DO UPDATE SET count=CASE WHEN start<=excluded.start-60 THEN 1 ELSE count+1 END,start=CASE WHEN start<=excluded.start-60 THEN excluded.start ELSE start END').bind(bucket,at)
    ]);
    const row=await db.prepare('SELECT count FROM auth_rates WHERE bucket=?').bind(bucket).first();
    if(row.count>limit)throw new Rejected(429,'rate_limited');
  }
  return async function auth(request,env) {
    const url=new URL(request.url),path=url.pathname,method=request.method,at=Math.floor(now()/1000),db=database(env);
    if(path==='/v1/auth/config'&&method==='GET')return reply({googleEnabled:configured(env)});
    if(!configured(env))throw new Rejected(503,'google_not_configured');
    await rate(db,'all',at,600);
    if(path==='/v1/auth/google/start'&&method==='POST') {
      const source=request.headers.get('CF-Connecting-IP')||'shared';
      await rate(db,'start:'+await sha256(source.length<=64?source:'shared'),at,20);
      const v=await jsonBody(request);fields(v,['challenge','state','returnUri']);
      if(Object.values(v).some(x=>typeof x!=='string')||!TOKEN.test(v.challenge)||!TOKEN.test(v.state)||!validReturnUri(v.returnUri))throw new Rejected(400,'login_request');
      const token=randomToken();
      const result=await db.batch([
        db.prepare('DELETE FROM auth_attempts WHERE expires<=?').bind(at),
        db.prepare('DELETE FROM auth_sessions WHERE expires<=?').bind(at),
        db.prepare('INSERT INTO auth_attempts(request_hash,challenge,app_state,return_uri,expires,status) SELECT ?,?,?,?,?,0 WHERE (SELECT COUNT(*) FROM auth_attempts)<1000').bind(await sha256(token),v.challenge,v.state,v.returnUri,at+300)
      ]);
      if(changed(result[2])!==1)throw new Rejected(429,'login_capacity');
      return reply({authorizationUrl:trustedOrigin(env)+'/auth/google/begin?request='+token});
    }
    if(path==='/auth/google/begin'&&method==='GET') {
      const token=query(url).request;if(!TOKEN.test(token))throw new Rejected(400,'login_request');
      const state=randomToken(),nonce=randomToken(),verifier=randomToken(),browserCookie=randomToken();
      const result=await db.prepare('UPDATE auth_attempts SET status=1,google_state_hash=?,nonce=?,verifier=?,cookie_hash=? WHERE request_hash=? AND status=0 AND expires>?').bind(await sha256(state),nonce,verifier,await sha256(browserCookie),await sha256(token),at).run();
      if(changed(result)!==1)throw new Rejected(400,'login_expired');
      const target=new URL('https://accounts.google.com/o/oauth2/auth');
      target.search=new URLSearchParams({client_id:env.HELLSCRIPT_GOOGLE_CLIENT_ID,redirect_uri:trustedOrigin(env)+'/auth/google/callback',
        response_type:'code',scope:'openid https://www.googleapis.com/auth/userinfo.email',state,nonce,code_challenge:await challenge(verifier),
        code_challenge_method:'S256',access_type:'online',prompt:'select_account'}).toString();
      return new Response(null,{status:302,headers:{Location:target.href,'Cache-Control':'no-store','Referrer-Policy':'no-referrer',
        'Set-Cookie':COOKIE+'='+browserCookie+'; Path=/; Secure; HttpOnly; SameSite=Lax; Max-Age=300'}});
    }
    if(path==='/auth/google/callback'&&method==='GET') {
      const q=query(url),browserCookie=cookie(request,COOKIE);
      if(!TOKEN.test(q.state)||!TOKEN.test(browserCookie))throw new Rejected(400,'login_state');
      const stateHash=await sha256(q.state),cookieHash=await sha256(browserCookie);
      const attempt=await db.prepare('UPDATE auth_attempts SET status=2 WHERE google_state_hash=? AND cookie_hash=? AND status=1 AND expires>? RETURNING *').bind(stateHash,cookieHash,at).first();
      if(!attempt)throw new Rejected(400,'login_state');
      const result={state:attempt.app_state};
      if(q.error)result.error='cancelled';
      else {
        if(typeof q.code!=='string'||q.code.length<1||q.code.length>4096)throw new Rejected(400,'google_code');
        try {
          const person=await identity(q.code,attempt,env),handoff=randomToken(),handoffHash=await sha256(handoff);
          const written=await db.batch([
            db.prepare('INSERT OR IGNORE INTO auth_accounts VALUES(?,?,?)').bind(crypto.randomUUID().replaceAll('-',''),person.identity,at),
            db.prepare('UPDATE auth_attempts SET status=3,handoff_hash=?,account_id=(SELECT id FROM auth_accounts WHERE identity_hash=?),display_name=?,expires=?,verifier=NULL,nonce=NULL,cookie_hash=NULL WHERE request_hash=? AND status=2 AND expires>?').bind(handoffHash,person.identity,person.name,at+60,attempt.request_hash,at)
          ]);
          if(changed(written[1])!==1)throw new Rejected(400,'login_expired');result.code=handoff;
        }catch(error){if(!(error instanceof Rejected))throw error;result.error='authentication_failed';}
      }
      return new Response(null,{status:302,headers:{Location:attempt.return_uri+'?'+new URLSearchParams(result).toString(),
        'Cache-Control':'no-store','Referrer-Policy':'no-referrer','Set-Cookie':COOKIE+'=; Path=/; Secure; HttpOnly; SameSite=Lax; Max-Age=0'}});
    }
    if(path==='/v1/auth/google/exchange'&&method==='POST') {
      const v=await jsonBody(request);fields(v,['code','verifier','state']);
      if(Object.values(v).some(x=>typeof x!=='string'||!TOKEN.test(x)))throw new Rejected(400,'exchange_request');
      const hash=await sha256(v.code),proof=await challenge(v.verifier);
      const attempt=await db.prepare('SELECT * FROM auth_attempts WHERE handoff_hash=? AND status=3 AND expires>?').bind(hash,at).first();
      if(!attempt||!equal(attempt.challenge,proof)||!equal(attempt.app_state,v.state))throw new Rejected(401,'exchange_proof');
      const token=randomToken(),expires=at+SESSION_SECONDS;
      const written=await db.batch([
        db.prepare('UPDATE auth_attempts SET status=4 WHERE handoff_hash=? AND status=3 AND expires>? AND challenge=? AND app_state=?').bind(hash,at,proof,v.state),
        db.prepare('DELETE FROM auth_sessions WHERE account_id IN (SELECT account_id FROM auth_attempts WHERE handoff_hash=? AND status=4)').bind(hash),
        db.prepare('INSERT INTO auth_sessions SELECT ?,account_id,? FROM auth_attempts WHERE handoff_hash=? AND status=4').bind(await sha256(token),expires,hash),
        db.prepare('DELETE FROM auth_attempts WHERE handoff_hash=? AND status=4').bind(hash)
      ]);
      if(changed(written[0])!==1||changed(written[2])!==1)throw new Rejected(401,'exchange_proof');
      return reply({accountId:attempt.account_id,displayName:attempt.display_name,accessToken:token,expiresAt:expires});
    }
    if(path==='/v1/auth/session'&&['GET','DELETE'].includes(method)) {
      const hash=await sha256(bearer(request));
      const row=await db.prepare('SELECT * FROM auth_sessions WHERE token_hash=? AND expires>?').bind(hash,at).first();
      if(!row)throw new Rejected(401,'authentication');
      if(method==='DELETE')await db.prepare('DELETE FROM auth_sessions WHERE token_hash=?').bind(hash).run();
      return reply({accountId:row.account_id,expiresAt:row.expires});
    }
    throw new Rejected(404,'route');
  };
}
