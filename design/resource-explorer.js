'use strict';
/* View-only resource explorer. Source records and current balance data are never mutated. */
window.ResourceExplorer=(()=>{
 const h=x=>String(x??'').replace(/[&<>"']/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
 const OLD='https://dakrdong.github.io/HELLSCRIPT-Wiki/';
 const groups={all:'전체 리소스',resources:'공개 원화',items:'장비 베이스',legendaries:'전설',sets:'세트 효과','set-items':'세트 장비','class-ability-gear':'스킬 연계 장비',potions:'물약','reward-boxes':'보상 상자',aspects:'위상 원화',currency:'재화 이미지',models:'모델',audio:'사운드',technical:'기술 자산'};
 const href=(group='all')=>'#/db/catalog/'+encodeURIComponent(group);
 const value=v=>v===undefined?'<span class="rx-missing">원문 필드 없음</span>':v===null?'<span class="rx-missing">null · 미확인/해당 없음</span>':v===''?'<span class="rx-missing">빈 문자열</span>':Array.isArray(v)?v.map(value).join('<br>'):typeof v==='object'?`<details><summary>구조화 속성</summary><pre>${h(JSON.stringify(v,null,2))}</pre></details>`:h(v);
 const safeUrl=u=>{try{const x=new URL(u);return x.protocol==='https:'&&['raw.githubusercontent.com','dakrdong.github.io'].includes(x.hostname)?x.href:null;}catch{return null;}};
 const field=(r,...keys)=>{for(const key of keys)if(Object.hasOwn(r.fields,key))return r.fields[key];return undefined;};
 let returnHash='#/db';
 const entry=r=>'#/db/entry/'+encodeURIComponent(r.db)+'/'+encodeURIComponent(r.id);
 function build(data,reference){
  const records=[];
  for(const db of reference.databases)for(const raw of db.rows){
   const image=safeUrl(reference.imageUrls[raw.image?.file]);
   records.push({key:db.id+'\0'+raw.id,db:db.id,id:raw.id,name:raw.name,category:raw.category,usage:raw.summary,fields:raw.fields||{},image,raw,origin:'이전 공개 위키',status:raw.status});
  }
  const artByHash=new Map();for(const r of records)if(r.db==='resources'&&r.image){const hash=field(r,'SHA-256');if(hash&&!artByHash.has(hash))artByHash.set(hash,r.image);}
  for(const raw of data.resources)records.push({key:'technical\0'+raw.id,db:'technical',id:raw.id,name:raw.name,category:raw.kind,usage:raw.usage,fields:{'형식':raw.format,'바이트':raw.bytes,'GUID':raw.guid,'SHA-256':raw.sha256,'렌더':raw.render,'청음':raw.listening,'라이선스':raw.license},image:artByHash.get(raw.sha256)||null,raw,origin:'현재 위키 기술 목록',status:raw.status});
  return records.map((r,i)=>({...r,ordinal:i}));
 }
 function inGroup(r,group){
  if(group==='all')return true;
  if(group==='aspects')return r.db==='resources'&&/위상/.test(r.category);
  if(group==='currency')return r.db==='resources'&&/currency-|abyssal-coin/.test(r.raw.image?.file||'');
  if(group==='models')return /모델|mesh|prefab|프리팹/.test(r.category);
  if(group==='audio')return /음향|오디오|BGM|사운드/.test(r.category);
  return r.db===group;
 }
 function columns(group){
  const c=(label,get)=>({label,get});
  if(['items','legendaries','set-items','sets','class-ability-gear'].includes(group))return [c('부위',r=>field(r,'부위','장착 부위')),c('직업',r=>field(r,'직업')),c('등급 · 분류',r=>field(r,'등급','품질','분류')),c('세트 ID',r=>field(r,'세트 ID')),c('주 수치 · 효과',r=>field(r,'기본 주 수치','효과','2세트')),c('요구 스킬',r=>field(r,'요구 스킬'))];
  if(group==='potions')return [c('등급',r=>field(r,'등급 / Grade')),c('효과',r=>field(r,'효과 / Effect')),c('계수',r=>field(r,'효과 계수 / Magnitude')),c('재사용(초)',r=>field(r,'재사용 (초) / Cooldown (s)'))];
  if(group==='technical'||group==='audio')return [c('형식',r=>field(r,'형식')),c('바이트',r=>field(r,'바이트')),c('렌더 · 청음',r=>[field(r,'렌더'),field(r,'청음')]),c('GUID',r=>field(r,'GUID'))];
  return [c('해상도 · 형식',r=>field(r,'해상도','형식')),c('연결 콘텐츠',r=>r.raw.content?[r.raw.content.db,r.raw.content.id]:r.raw.resource),c('용도 원문',r=>r.usage)];
 }
 function compare(a,b){if(a===undefined||a===null)return b===undefined||b===null?0:1;if(b===undefined||b===null)return -1;if(typeof a==='number'&&typeof b==='number')return a-b;return String(Array.isArray(a)?a.join(' '):a).localeCompare(String(Array.isArray(b)?b.join(' '):b),'ko',{numeric:false});}
 function query(records,state={},cols=[]){
  const q=String(state.query||'').toLowerCase();const out=records.filter(r=>inGroup(r,state.group||'all')&&(!state.category||r.category===state.category)&&(!q||JSON.stringify([r.id,r.name,r.category,r.usage,r.fields,r.raw.paths]).toLowerCase().includes(q)));
  const get=state.sort==='id'?r=>r.id:state.sort==='category'?r=>r.category:state.sort?.startsWith('field:')?cols[Number(state.sort.slice(6))]?.get:r=>r.name;
  if(get)out.sort((a,b)=>{const x=get(a),y=get(b),missing=x==null||y==null;return (missing?compare(x,y):compare(x,y)*(state.direction==='desc'?-1:1))||a.ordinal-b.ordinal;});
  return out;
 }
 function thumbnail(r,large=false){return r.image?`<img class="rx-thumb ${large?'rx-large':''}" src="${h(r.image)}" alt="${h(r.name)}" loading="lazy" decoding="async" referrerpolicy="no-referrer">`:`<span class="rx-no-image">${/모델|mesh|prefab/.test(r.category)?'3D':/음향|BGM|오디오/.test(r.category)?'음원':'이미지 없음'}</span>`;}
 function wireImages(root){root.querySelectorAll('img.rx-thumb').forEach(img=>img.addEventListener('error',()=>{const s=document.createElement('span');s.className='rx-no-image';s.textContent='이미지 연결 실패';img.replaceWith(s);},{once:true}));}
 function relationships(r,records){
  const keys=[];if(r.raw.resource)keys.push(['resources',r.raw.resource]);if(r.raw.content)keys.push([r.raw.content.db,r.raw.content.id]);const set=field(r,'세트 ID');if(set)keys.push(['sets',set]);
  if(r.db==='legendaries')for(const x of records)if(x.db==='resources'&&/위상/.test(x.category)&&x.name===r.id)keys.push([x.db,x.id]);
  if(r.db==='resources'&&/위상/.test(r.category))keys.push(['legendaries',r.name]);
  return keys.map(([db,id])=>records.find(x=>x.db===db&&x.id===id)).filter(Boolean).filter((x,i,a)=>a.indexOf(x)===i);
 }
 function detail(r,records,group){
  const d=document.createElement('dialog');d.className='rx-dialog';d.setAttribute('aria-label',r.name+' 상세');
  const related=relationships(r,records),source=r.db==='technical'?'#/resource/'+encodeURIComponent(r.id):OLD+'#/db/'+encodeURIComponent(r.db)+'?record='+encodeURIComponent(r.id);
  d.innerHTML=`<div class="rx-dialog-head"><div><small>${h(r.origin)} · ${h(r.db)}</small><h2>${h(r.name)}</h2><code>${h(r.id)}</code></div><button class="rx-close" aria-label="상세 닫기">닫기</button></div><div class="rx-detail-intro">${thumbnail(r,true)}<div><p>${value(r.usage)}</p><p>분류: ${h(r.category)} · 원문 상태: ${h(r.status)}</p><a href="${h(source)}">원본 레코드 ↗</a></div></div><p class="rx-authority">이전 위키의 수치·등급·효과는 참조 원문이며 현행 밸런스 채택값이 아닙니다. <a href="#/plan/equipment-current-rules">현행 장비 규칙</a> · <a href="#/plan/rune-three-colours01">현행 룬 정본</a> · <a href="#/plan/s2-reward-wallet">현행 재화 규칙</a></p><h3>실제 속성</h3><dl class="rx-properties">${Object.entries(r.fields).map(([k,v])=>`<dt>${h(k)}</dt><dd>${value(v)} <small class="rx-type">${v===null?'null':Array.isArray(v)?'array':typeof v}</small></dd>`).join('')}</dl>${related.length?`<h3>연결된 장비 · 세트 · 위상 원화</h3><div class="rx-related">${related.map(x=>`<a href="${h(entry(x))}">${thumbnail(x)}<span>${h(x.name)}<small>${h(x.db)} / ${h(x.id)}</small></span></a>`).join('')}</div>`:''}<details><summary>전체 원본 레코드 · 경로·의존성·출처</summary><pre>${h(JSON.stringify(r.raw,null,2))}</pre></details>`;
  document.body.appendChild(d);const previous=document.activeElement;d.showModal();d.querySelector('.rx-close').focus();wireImages(d);
  d.addEventListener('close',()=>{d.remove();if(location.hash.startsWith('#/db/entry/'))location.hash=returnHash;else previous?.focus();});d.querySelector('.rx-close').onclick=()=>d.close();d.addEventListener('click',e=>{if(e.target===d)d.close();});
 }
 let pending;
 function load(){if(!pending)pending=fetch('resource-records.json').then(r=>{if(!r.ok)throw Error(r.status);return r.json();}).catch(e=>{pending=null;throw e;});return pending;}
 let generation=0;
 async function show(data,requested='all',selected=null){
  const token=++generation,hash=location.hash,main=document.querySelector('#main');document.querySelectorAll('.rx-dialog').forEach(x=>x.remove());main.classList.add('resource-explorer');
  main.innerHTML='<p role="status">리소스 도감을 불러오는 중입니다.</p>';let reference;
  try{reference=await load();}catch{if(token!==generation||location.hash!==hash)return;main.innerHTML='<h1>리소스 도감</h1><p role="alert">공개 리소스 자료를 불러오지 못했습니다. 기존 자료표는 그대로 사용할 수 있습니다.</p><a href="#/db/tables">기존 자료표 열기</a>';return;}
  if(token!==generation||location.hash!==hash)return;WikiIA.shell('db','catalog');const records=build(data,reference),group=Object.hasOwn(groups,requested)?requested:'all',cols=columns(group);
  const saved=WikiIA.viewState();let state={query:'',category:'',sort:'name',direction:'asc',page:0,...saved,group};if(selected)state={query:'',category:'',sort:'name',direction:'asc',page:0,group};
  const categories=[...new Set(records.filter(r=>inGroup(r,group)).map(r=>r.category))].sort();if(!categories.includes(state.category))state.category='';
  main.innerHTML=`<div class="rx-heading"><div class="eyebrow">HELLSCRIPT / RESOURCE ATLAS</div><h1>리소스 도감</h1><p>원화부터 장비와 사운드까지, 실제 이름과 속성으로 찾아보세요.</p><div class="rx-heading-links"><a href="#/db/tables">수치·규칙 자료표</a><a href="#/design/documents">설명·결정 문서</a><a href="${h(reference.referenceUrl)}" target="_blank" rel="noopener">캐주얼 아트 기준 ↗</a></div></div><nav class="rx-tabs" aria-label="자료 종류">${Object.entries(groups).map(([id,name])=>`<a href="${h(href(id))}" ${group===id?'aria-current="page"':''}>${h(name)}</a>`).join('')}</nav><div class="rx-tools"><label>이름 · 고유 ID · 속성 검색<input id="rx-query" type="search" placeholder="B01, 부위, 재질, 파일 이름…" value="${h(state.query)}"></label><label>세부 분류<select id="rx-category"><option value="">모든 분류</option>${categories.map(x=>`<option ${state.category===x?'selected':''}>${h(x)}</option>`).join('')}</select></label><button id="rx-reset">조건 초기화</button></div><p class="rx-authority">같은 원본을 공유하는 콘텐츠와 기술 자산은 서로 다른 레코드입니다. 이전 공개 위키의 속성은 <b>참조 원문</b>이며, <a href="#/plan/rune-three-colours01">현행 룬 정정</a>·기획/운영 원값은 변경하지 않습니다.</p><p id="rx-count" role="status" aria-live="polite"></p><div class="rx-table-wrap" tabindex="0" aria-label="리소스 비교표 · 좁은 화면에서는 표 안에서 가로 이동"><table class="rx-table"><thead><tr><th>미리보기</th>${[['name','이름 · 원본 고유 ID'],['category','분류 · 자료 출처'],...cols.map((c,i)=>['field:'+i,c.label])].map(([id,name])=>`<th scope="col" aria-sort="none"><button data-rx-sort="${h(id)}">${h(name)}</button></th>`).join('')}<th scope="col">관련 정보</th></tr></thead><tbody id="rx-rows"></tbody></table></div><div class="rx-pagination"><button id="rx-prev">이전</button><span id="rx-page"></span><button id="rx-next">다음</button></div>`;
  const search=main.querySelector('#rx-query'),select=main.querySelector('#rx-category');main.onclick=e=>{const a=e.target.closest('a');if(a?.hash.startsWith('#/db/entry/')&&!location.hash.startsWith('#/db/entry/'))returnHash=location.hash;};
  const update=()=>{const out=query(records,state,cols);state.page=Math.max(0,Math.min(Number.isInteger(state.page)?state.page:0,Math.max(0,Math.ceil(out.length/40)-1)));WikiIA.saveView(state);main.querySelector('#rx-count').textContent=groups[group]+' · '+out.length+'개 레코드 / '+(state.page+1)+'페이지';main.querySelector('#rx-page').textContent=(state.page+1)+' / '+Math.max(1,Math.ceil(out.length/40));main.querySelector('#rx-prev').disabled=state.page===0;main.querySelector('#rx-next').disabled=(state.page+1)*40>=out.length;
   main.querySelector('#rx-rows').innerHTML=out.slice(state.page*40,(state.page+1)*40).map(r=>`<tr><td><a class="rx-preview" href="${h(entry(r))}" aria-label="${h(r.name)} 상세">${thumbnail(r)}</a></td><th scope="row"><a href="${h(entry(r))}">${h(r.name)}</a><code>${h(r.id)}</code></th><td>${h(r.category)}<small>${h(r.origin)} / ${h(r.db)}</small></td>${cols.map(c=>`<td>${value(c.get(r))}</td>`).join('')}<td><a class="rx-open" href="${h(entry(r))}">상세 보기</a>${relationships(r,records).slice(0,2).map(x=>`<a class="rx-inline-link" href="${h(entry(x))}">${h(x.name)}</a>`).join('')}</td></tr>`).join('')||`<tr><td colspan="${cols.length+4}">검색 결과가 없습니다. 조건을 바꾸거나 초기화하세요.</td></tr>`;
   main.querySelectorAll('[data-rx-sort]').forEach(b=>{b.closest('th').setAttribute('aria-sort',b.dataset.rxSort===state.sort?(state.direction==='asc'?'ascending':'descending'):'none');});wireImages(main);
  };
  search.oninput=()=>{state.query=search.value;state.page=0;update();};select.onchange=()=>{state.category=select.value;state.page=0;update();};main.querySelector('#rx-reset').onclick=()=>{state={query:'',category:'',sort:'name',direction:'asc',page:0,group};search.value='';select.value='';update();};main.querySelectorAll('[data-rx-sort]').forEach(b=>b.onclick=()=>{state.direction=state.sort===b.dataset.rxSort&&state.direction==='asc'?'desc':'asc';state.sort=b.dataset.rxSort;state.page=0;update();});main.querySelector('#rx-prev').onclick=()=>{state.page--;update();};main.querySelector('#rx-next').onclick=()=>{state.page++;update();};update();if(selected){const r=records.find(r=>r.db===selected.db&&r.id===selected.id);if(r)detail(r,records,group);else main.insertAdjacentHTML('afterbegin','<p role="alert">요청한 원본 ID가 없습니다.</p>');}
 }
 return {build,inGroup,columns,query,compare,safeUrl,relationships,show,href};
})();
