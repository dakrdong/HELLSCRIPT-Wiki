(() => {
  'use strict';
  const D=window.HELLSCRIPT_WIKI, $=s=>document.querySelector(s);
  const esc=s=>String(s??'').replace(/[&<>"']/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
  const PUBLIC=D.audience==='public';
  if(PUBLIC)document.body.classList.add('public-wiki');
  const pageLink=p=>`#/page/${p.id}`;
  function badge(s){return `<span class="badge ${/현재|추출|등록/.test(s)?'current':/대기|후속|임시|검토/.test(s)?'pending':''}">${esc(s)}</span>`}
  function navigation(db=false){
    $('#wiki-tab').classList.toggle('active',!db);$('#db-tab').classList.toggle('active',db);
    const path=location.hash;
    $('#navigation').innerHTML=db?`<p class="nav-group">리소스와 콘텐츠</p>${D.databases.map(d=>`<a class="nav-link ${path.startsWith('#/db/'+d.id)?'active':''}" href="#/db/${d.id}">${esc(d.name)}<span>${d.rows.length}</span></a>`).join('')}`:
      `<a class="nav-link ${path.startsWith('#/tree')?'active':''}" href="#/tree">전체 위키 트리</a><a class="nav-link" href="#/tree?sort=newest">시간순 페이지 목록</a><a class="nav-link" href="#/tags">태그 탐색</a>${PUBLIC?'':'<a class="nav-link" href="public/#/tree">공개용 위키 ↗</a>'}`+[...new Set(D.pages.map(p=>p.category))].map(c=>`<p class="nav-group">${esc(c)}</p>${D.pages.filter(p=>p.category===c).map(p=>`<a class="nav-link ${path.startsWith(pageLink(p))?'active':''}" href="${pageLink(p)}">${esc(p.shortTitle||p.title)}</a>`).join('')}`).join('');
    $('#sidebar-footer').innerHTML=`문서 ${D.pages.length}개 · DB ${D.databases.length}개<br>원본 기준 ${esc(D.generatedAt.slice(0,10))}`;
  }
  const dateText=value=>value?new Date(value).toLocaleString('ko-KR',{timeZone:'Asia/Seoul',year:'numeric',month:'2-digit',day:'2-digit',...(PUBLIC?{}:{hour:'2-digit',minute:'2-digit'})}):'기록 없음';
  const stamp=(p,key='updatedAt')=>p[key]||p.capturedAt||p.date;
  const day=value=>value?new Date(value).toLocaleDateString('sv-SE',{timeZone:'Asia/Seoul'}):'';
  const tagLinks=p=>`<div class="tags">${(p.tags||[]).map(t=>`<a href="#/tags?tag=${encodeURIComponent(t)}">#${esc(t)}</a>`).join('')}</div>`;
  function row(p){return `<article class="tree-row" data-page="${esc(p.id)}"><div class="row-title"><a href="${pageLink(p)}">${esc(p.title)}</a>${badge(p.status||'원문 기록')}</div><p>${esc(p.summary||'')}</p><div class="meta"><span>${esc(p.category)}</span><span>최초 등록 ${esc(dateText(stamp(p,'createdAt')))}</span><span>마지막 변경 ${esc(dateText(stamp(p)))}</span></div>${tagLinks(p)}${`<div class="actions version-links">${(p.versions||[]).slice().reverse().map(v=>`<a href="${pageLink(p)}?version=${v.revision}">v${String(v.revision).padStart(3,'0')}</a>`).join('')}<a href="${pageLink(p)}?view=history">이력 비교</a></div>`}</article>`}
  function tree(params=new URLSearchParams(),mode='tree'){
    let q=params.get('q')||'',tag=params.get('tag')||'',sort=params.get('sort')||'category',basis=params.get('basis')||'updatedAt',from=params.get('from')||'',to=params.get('to')||'';
    if(!['category','newest','oldest'].includes(sort))sort='category';
    if(!['createdAt','updatedAt'].includes(basis))basis='updatedAt';
    $('#search').value=q;
    document.title=(mode==='tags'?'태그 탐색':mode==='search'?'검색 결과':'전체 위키 트리')+' · HELLSCRIPT 위키';
    $('#main').innerHTML=`<div class="page-heading"><div><p class="eyebrow">${PUBLIC?'PUBLIC WIKI':'WIKI MAP'}</p><h1>${mode==='tags'?'태그 탐색':mode==='search'?'검색 결과':'전체 위키 트리'}</h1><p class="lead">${PUBLIC?'전체 문서와 데이터베이스를 읽기 전용으로 탐색합니다.':'기획과 개발 기록을 주제·시간·태그로 탐색합니다.'}</p></div><div class="actions"><button id="expand">모두 펼치기</button><button id="collapse">모두 접기</button></div></div>
    <div class="stats"><div class="stat"><strong>${D.pages.length}</strong><span>문서</span></div><div class="stat"><strong>${new Set(D.pages.flatMap(p=>p.tags||[])).size}</strong><span>태그</span></div><div class="stat"><strong>${new Set(D.pages.map(p=>p.category)).size}</strong><span>카테고리</span></div><div class="stat"><strong>${D.pages.reduce((n,p)=>n+(p.versions?.length||1),0)}</strong><span>보존 문서 버전</span></div></div>
    <div class="toolbar" aria-label="문서 검색과 날짜 필터"><input id="page-search" type="search" aria-label="문서 검색" placeholder="제목·본문 검색" value="${esc(q)}"><label>보기 방식 <select id="page-sort">${[['category','카테고리'],['newest','최신순'],['oldest','오래된순']].map(([v,n])=>`<option value="${v}" ${v===sort?'selected':''}>${n}</option>`).join('')}</select></label><label>날짜 기준 <select id="date-basis"><option value="updatedAt" ${basis==='updatedAt'?'selected':''}>마지막 변경</option><option value="createdAt" ${basis==='createdAt'?'selected':''}>최초 등록</option></select></label><label>시작일 <input id="date-from" type="date" value="${esc(from)}"></label><label>종료일 <input id="date-to" type="date" value="${esc(to)}"></label><button id="page-reset">초기화</button></div>
    <p class="filter-help">날짜는 한국 시간 기준의 위키 등록·변경 시각입니다. 원문에 적힌 기준일과 다를 수 있습니다.</p><div id="tag-browser"></div><p id="page-count" role="status" aria-live="polite"></p><div id="page-results"></div><div id="search-db-results"></div>`;
    function update(){
      const invalid=from&&to&&from>to;
      const dated=D.pages.filter(p=>!invalid&&(!from||day(stamp(p,basis))>=from)&&(!to||day(stamp(p,basis))<=to));
      let found=dated.filter(p=>(!tag||(p.tags||[]).includes(tag))&&(!q||[p.title,p.body,...p.tags||[]].join(' ').toLowerCase().includes(q.toLowerCase())));
      if(sort!=='category')found.sort((a,b)=>(Date.parse(stamp(a,basis))-Date.parse(stamp(b,basis)))*(sort==='newest'?-1:1)||a.id.localeCompare(b.id));
      $('#page-count').textContent=invalid?'시작일은 종료일보다 늦을 수 없습니다.':`${found.length} / ${D.pages.length}개 문서${tag?' · #'+tag:''}`;
      const tags=[...new Set(dated.flatMap(p=>p.tags||[]))].sort((a,b)=>a.localeCompare(b,'ko'));
      const tagUrl=t=>{const p=new URLSearchParams({sort,basis});if(q)p.set('q',q);if(from)p.set('from',from);if(to)p.set('to',to);if(t)p.set('tag',t);return '#/tags?'+p};
      $('#tag-browser').innerHTML=mode==='tags'||tag?`<section class="tag-panel" aria-label="태그 목록"><h2>${tag?'선택한 태그: #'+esc(tag):'태그별 문서'}</h2><div class="tags"><a href="${esc(tagUrl(''))}" ${!tag?'aria-current="true"':''}>전체 태그</a>${tags.map(t=>`<a href="${esc(tagUrl(t))}" ${tag===t?'aria-current="true"':''}>#${esc(t)} <span>${dated.filter(p=>(p.tags||[]).includes(t)).length}</span></a>`).join('')}</div></section>`:'';
      $('#page-results').innerHTML=found.length?(sort==='category'?[...new Set(found.map(p=>p.category))].map(c=>`<details class="tree" open><summary>${esc(c)}<small>${found.filter(p=>p.category===c).length}개 문서</small></summary>${found.filter(p=>p.category===c).map(row).join('')}</details>`).join(''):`<section class="tree timeline" aria-label="시간순 페이지 목록">${found.map(row).join('')}</section>`):'<p class="empty">일치하는 문서가 없습니다. 검색어나 필터를 바꿔 주세요.</p>';
      $('#search-db-results').innerHTML=q&&!tag&&!from&&!to?searchDB(q):'';
      $('#expand').disabled=$('#collapse').disabled=sort!=='category';
    }
    function sync(){const p=new URLSearchParams();for(const [k,v] of Object.entries({q,tag,sort,basis,from,to}))if(v)p.set(k,v);history.replaceState(null,'',`#/${mode}?${p}`);update()}
    $('#page-search').oninput=e=>{q=e.target.value;$('#search').value=q;sync()};
    $('#page-sort').onchange=e=>{sort=e.target.value;sync()};$('#date-basis').onchange=e=>{basis=e.target.value;sync()};$('#date-from').onchange=e=>{from=e.target.value;sync()};$('#date-to').onchange=e=>{to=e.target.value;sync()};
    $('#page-reset').onclick=()=>{q=tag=from=to='';$('#page-search').value=$('#search').value=$('#date-from').value=$('#date-to').value='';sync()};
    $('#expand').onclick=()=>document.querySelectorAll('.tree').forEach(e=>e.open=true);$('#collapse').onclick=()=>document.querySelectorAll('.tree').forEach(e=>e.open=false);update();
  }
  function searchDB(q){return D.databases.map(db=>{const rows=db.rows.filter(r=>JSON.stringify(r).toLowerCase().includes(q.toLowerCase()));return rows.length?`<h2>${esc(db.name)} · ${rows.length}개</h2>${rows.map(r=>`<p><a href="#/db/${db.id}?record=${encodeURIComponent(r.id)}">${esc(r.id)} · ${esc(r.name)}</a></p>`).join('')}`:''}).join('')}
  function page(id,params){const p=D.pages.find(p=>p.id===id);if(!p)return missing();
    if(params.get('view')==='history')return compare(p,params);
    const version=params.get('version');const v=version?(p.versions||[]).find(v=>String(v.revision)===version):null;if(version&&!v)return missing();const content=v||p;
    document.title=p.title+' · HELLSCRIPT 위키';
    $('#main').innerHTML=`<div class="page-heading document-heading"><div><p class="eyebrow">${esc(p.category)}</p><h1>${esc(p.title)}</h1><div class="meta">${badge(p.status||'원문 기록')}<span>최초 등록 ${esc(dateText(stamp(p,'createdAt')))}</span><span>마지막 변경 ${esc(dateText(content.capturedAt||stamp(p)))}</span><span>v${String(content.revision).padStart(3,'0')}</span>${`<a class="button" href="sources/${esc(p.source)}" download>원문 내려받기</a>`}</div></div></div>${tagLinks(p)}${p.notice?`<div class="notice">${esc(p.notice)}</div>`:''}<nav class="toc" aria-label="문서 목차">${(content.headings||[]).map(h=>`<a href="#/page/${p.id}?${version?'version='+version+'&':''}section=${encodeURIComponent(h.id)}">${esc(h.title)}</a>`).join('')}</nav><article class="prose">${content.html||'<pre>'+esc(content.body)+'</pre>'}</article>${`<div class="source">원본: ${esc(p.source)}<br>문서 해시: ${esc(content.hash||'')}</div><h2>보존 이력</h2><div class="actions">${(p.versions||[p]).slice().reverse().map(v=>`<a class="button" href="#/page/${p.id}?version=${v.revision}">v${String(v.revision).padStart(3,'0')} · ${esc(v.date||p.date)}</a>`).join('')}<a class="button" href="${pageLink(p)}?view=history">이력 비교</a></div>`}${connections(p)}`;
    const section=params.get('section');if(section)document.getElementById(section)?.scrollIntoView();
  }
  function connections(p){
    const linked=D.pages.filter(x=>x.id!==p.id&&(p.html.includes('#/page/'+x.id+'"')||p.html.includes('#/page/'+x.id+'?')));
    const backlinks=D.pages.filter(x=>x.id!==p.id&&(x.html.includes('#/page/'+p.id+'"')||x.html.includes('#/page/'+p.id+'?')));
    return [['연결된 문서',linked],['이 문서를 참조하는 문서',backlinks]].map(([title,pages])=>pages.length?`<h2>${title}</h2><div class="actions">${pages.map(x=>`<a class="button" href="${pageLink(x)}">${esc(x.shortTitle||x.title)}</a>`).join('')}</div>`:'').join('');
  }
  function compare(p,params){
    const versions=p.versions||[];if(!versions.length)return missing();
    let before=params.get('from')||String(versions[Math.max(0,versions.length-2)].revision),after=params.get('to')||String(versions[versions.length-1].revision);
    const a=versions.find(v=>String(v.revision)===before),b=versions.find(v=>String(v.revision)===after);if(!a||!b)return missing();
    const options=selected=>versions.map(v=>`<option value="${v.revision}" ${String(v.revision)===selected?'selected':''}>v${String(v.revision).padStart(3,'0')} · ${esc(dateText(v.capturedAt||v.date))}</option>`).join('');
    document.title=p.title+' 이력 비교 · HELLSCRIPT 위키';
    $('#main').innerHTML=`<div class="page-heading"><div><p class="eyebrow">PAGE HISTORY</p><h1>${esc(p.title)}</h1><p class="lead">보존된 두 버전의 본문에서 추가·삭제된 줄을 확인합니다.</p></div><a class="button" href="${pageLink(p)}">현재 문서로 돌아가기</a></div><div class="toolbar"><label>이전 버전 <select id="compare-from">${options(before)}</select></label><label>비교 버전 <select id="compare-to">${options(after)}</select></label></div><p class="notice">${versions.length===1?'아직 보존된 버전이 하나뿐입니다. 다음 본문 변경부터 비교할 수 있습니다.':'초록색 + 줄은 추가, 붉은색 − 줄은 삭제된 내용입니다.'}</p><div class="diff" aria-label="버전 차이">${lineDiff(a.body,b.body)}</div>`;
    const change=()=>{location.hash=`${pageLink(p)}?view=history&from=${$('#compare-from').value}&to=${$('#compare-to').value}`};$('#compare-from').onchange=$('#compare-to').onchange=change;
  }
  function lineDiff(before,after){
    const a=before.split('\n'),b=after.split('\n');
    const line=(kind,text)=>`<div class="diff-${kind}"><span aria-hidden="true">${kind==='add'?'+':kind==='remove'?'−':' '}</span><code>${esc(text)||' '}</code></div>`;
    if(before===after)return '<p class="notice">두 버전의 본문이 같습니다.</p>';
    // Bound memory for unusually large documents; keep all lines visible in the fallback.
    if(a.length*b.length>2000000)return '<p>문서가 길어 이전 본문과 비교 본문을 순서대로 표시합니다.</p>'+a.map(x=>line('remove',x)).join('')+b.map(x=>line('add',x)).join('');
    const dp=Array.from({length:a.length+1},()=>new Uint32Array(b.length+1));
    for(let i=a.length-1;i>=0;i--)for(let j=b.length-1;j>=0;j--)dp[i][j]=a[i]===b[j]?1+dp[i+1][j+1]:Math.max(dp[i+1][j],dp[i][j+1]);
    let i=0,j=0,out='';while(i<a.length||j<b.length){if(i<a.length&&j<b.length&&a[i]===b[j]){out+=line('same',a[i]);i++;j++;}else if(j<b.length&&(i===a.length||dp[i][j+1]>dp[i+1][j]))out+=line('add',b[j++]);else out+=line('remove',a[i++]);}return out;
  }
  function missing(){$('#main').innerHTML='<h1>항목을 찾을 수 없습니다.</h1><p><a href="#/tree">전체 위키 트리로 돌아가기</a></p>'}
  function visual(row,large=false){
    const img=row.image;if(!img)return `<div class="preview"><span class="badge">${esc(row.status==='제작 대기'?'제작 대기':row.category)}</span></div>`;
    const file='media/'+encodeURIComponent(img.file);
    if(img.cell!==undefined){const col=img.cell%6,line=Math.floor(img.cell/6);return `<span role="img" aria-label="${esc(row.name)} 아틀라스 영역" class="${large?'atlas':'thumb'}" style="background-image:url('${file}');background-size:600% 400%;background-position:${col*20}% ${line*100/3}%"></span>`}
    return `<img src="${file}" alt="${esc(row.name)}" loading="lazy">`;
  }
  function value(v){return v===true?'예':v===false?'아니요':v===null||v===undefined?'미기재':Array.isArray(v)?v.join(' · '):String(v)}
  function related(row){return (row.related||[]).map(id=>{const p=D.pages.find(x=>x.id===id);return p?`<a class="button" href="${pageLink(p)}">${esc(p.shortTitle||p.title)}</a>`:''}).join('')}
  function showDetail(db,row){
    const popup=$('#detail');if(!row)return;
    const refs=[row.source,...row.refs||[]];
    $('#detail-body').innerHTML=`<p class="eyebrow">${esc(db.name)} / ${esc(row.id)}</p><h2>${esc(row.name)}</h2>${badge(row.status)}<p>${esc(row.summary)}</p>
      ${row.image?`<div class="preview">${visual(row,true)}</div>`:''}
      <dl>${Object.entries(row.fields).map(([k,v])=>`<dt>${esc(k)}</dt><dd>${esc(value(v))}</dd>`).join('')}</dl>
      <h3>연결된 자료</h3><div class="actions">${row.content?`<a class="button" href="#/db/${row.content.db}?record=${encodeURIComponent(row.content.id)}">콘텐츠 ${esc(row.content.id)}</a>`:''}${row.resource?`<a class="button" href="#/db/resources?record=${encodeURIComponent(row.resource)}">연결 리소스</a>`:''}${related(row)}</div>
      <h3>원본 근거</h3>${refs.map(s=>`<p class="source"><a href="sources/${s.path.split('/').map(encodeURIComponent).join('/')}" target="_blank" rel="noopener">${esc(s.path)}</a><br>${s.line}행부터 · 수집한 원본 사본</p>`).join('')}`;
    popup.setAttribute('aria-label',row.name+' 상세');if(!popup.open)popup.showModal();popup.scrollTop=0;
  }
  function database(id,params){
    const db=D.databases.find(d=>d.id===id);if(!db)return missing();
    document.title=db.name+' · HELLSCRIPT 위키';
    const categories=[...new Set(db.rows.map(r=>r.category))],statuses=[...new Set(db.rows.map(r=>r.status))];
    let query=params.get('q')||'',category=params.get('category')||'',status=params.get('status')||'',view=params.get('view')||(id==='resources'?'gallery':'table'),sort=params.get('sort')||'id';
    if(!['gallery','table'].includes(view))view='table';if(!['id','name'].includes(sort))sort='id';
    $('#main').innerHTML=`<div class="page-heading"><div><p class="eyebrow">${id==='resources'?'RESOURCE LIBRARY':'CONTENT DATABASE'}</p><h1>${esc(db.name)}</h1><p class="lead">${esc(db.description)}</p></div><div class="actions"><button id="download">JSON 내려받기</button></div></div>
      <div class="notice">${id==='resources'?'원본 이미지·아틀라스 영역·코드 생성 표현·제작 과제를 구분합니다. 원본 파일은 4개입니다.':'정의가 존재하거나 당시 검사에 통과한 사실을 출시·밸런스 검증 완료로 해석하지 않습니다.'} <a href="#/page/${id==='resources'?'resource-guide':'current-status'}">관리 기준 보기</a></div>
      <div class="toolbar" aria-label="DB 검색과 필터"><input id="db-search" type="search" aria-label="${esc(db.name)} 검색" placeholder="이름·ID·효과·사용처 검색" value="${esc(query)}">
      <label>분류 <select id="category"><option value="">전체 ${db.rows.length}개</option>${categories.map(c=>`<option value="${esc(c)}" ${c===category?'selected':''}>${esc(c)} · ${db.rows.filter(r=>r.category===c).length}</option>`).join('')}</select></label>
      <label>상태 <select id="status"><option value="">전체 상태</option>${statuses.map(s=>`<option value="${esc(s)}" ${s===status?'selected':''}>${esc(s)}</option>`).join('')}</select></label>
      <label>정렬 <select id="sort"><option value="id" ${sort==='id'?'selected':''}>ID순</option><option value="name" ${sort==='name'?'selected':''}>이름순</option></select></label>
      <button id="view-toggle">${view==='gallery'?'표로 보기':'카드로 보기'}</button><button id="clear-filters">초기화</button><span class="result-count" id="db-count" role="status" aria-live="polite"></span></div><div id="db-results"></div>`;
    let filtered=[];
    function currentParams(){const p=new URLSearchParams();if(query)p.set('q',query);if(category)p.set('category',category);if(status)p.set('status',status);if(view!==(id==='resources'?'gallery':'table'))p.set('view',view);if(sort!=='id')p.set('sort',sort);return p}
    function update(){
      filtered=db.rows.filter(r=>(!category||r.category===category)&&(!status||r.status===status)&&(!query||JSON.stringify(r).toLowerCase().includes(query.toLowerCase()))).sort((a,b)=>a[sort].localeCompare(b[sort],'ko',{numeric:true}));
      $('#db-count').textContent=`${filtered.length} / ${db.rows.length}개 표시`;
      $('#clear-filters').disabled=!(query||category||status);$('#view-toggle').textContent=view==='gallery'?'표로 보기':'카드로 보기';
      if(!filtered.length){$('#db-results').innerHTML='<div class="empty">일치하는 항목이 없습니다. 검색어나 필터를 바꿔 주세요.</div>';return}
      $('#db-results').innerHTML=view==='gallery'?`<div class="db-grid">${filtered.map(r=>`<button class="resource-card" data-record="${esc(r.id)}"><div class="preview">${r.image?visual(r,true):`<span class="badge">${esc(r.status==='제작 대기'?'제작 대기':'코드 생성 / 기록')}</span>`}</div><span class="db-code">${esc(r.id)} · ${esc(r.category)}</span><h3>${esc(r.name)}</h3><p>${esc(r.summary)}</p><span>${badge(r.status)}</span></button>`).join('')}</div>`:
        `<div class="table-scroll"><table class="db-table"><thead><tr><th scope="col">표현</th><th scope="col">이름 · ID</th><th scope="col">분류 · 상태</th><th scope="col">효과 · 용도</th><th scope="col">상세</th></tr></thead><tbody>${filtered.map(r=>`<tr><td>${r.image?visual(r):'<span class="db-code">'+esc(r.category)+'</span>'}</td><td><button class="text-button db-name" data-record="${esc(r.id)}">${esc(r.name)}</button><span class="db-code">${esc(r.id)}</span></td><td>${esc(r.category)}<br>${badge(r.status)}</td><td>${esc(r.summary)}</td><td><button data-record="${esc(r.id)}" aria-label="${esc(r.name)} 상세 보기">보기 ↗</button></td></tr>`).join('')}</tbody></table></div>`;
    }
    function sync(){history.replaceState(null,'',`#/db/${id}${currentParams().size?'?'+currentParams():''}`);update()}
    $('#db-search').oninput=e=>{query=e.target.value;sync()};$('#category').onchange=e=>{category=e.target.value;sync()};$('#status').onchange=e=>{status=e.target.value;sync()};$('#sort').onchange=e=>{sort=e.target.value;sync()};
    $('#view-toggle').onclick=()=>{view=view==='gallery'?'table':'gallery';sync()};
    $('#clear-filters').onclick=()=>{query=category=status='';$('#db-search').value=$('#category').value=$('#status').value='';sync()};
    $('#db-results').onclick=e=>{const button=e.target.closest('[data-record]');if(!button)return;const row=db.rows.find(r=>r.id===button.dataset.record),p=currentParams();p.set('record',row.id);history.pushState(null,'',`#/db/${id}?${p}`);showDetail(db,row)};
    $('#download').onclick=()=>{const blob=new Blob([JSON.stringify({database:db.id,generatedAt:D.generatedAt,rows:filtered},null,2)],{type:'application/json'}),url=URL.createObjectURL(blob),a=document.createElement('a');a.href=url;a.download=`hellscript-${db.id}.json`;a.click();setTimeout(()=>URL.revokeObjectURL(url),1000)};
    update();const record=params.get('record');if(record){const row=db.rows.find(r=>r.id===record);if(row)showDetail(db,row);else $('#db-results').insertAdjacentHTML('afterbegin','<div class="notice">요청한 레코드를 찾을 수 없습니다.</div>')}
  }
  let lastPath='';
  function route(){const [path,query='']=(location.hash||'#/tree').slice(2).split('?'), parts=path.split('/'),params=new URLSearchParams(query);if($('#detail').open)$('#detail').close();document.body.classList.remove('menu-open');$('#menu').setAttribute('aria-expanded','false');navigation(parts[0]==='db');if(path!==lastPath)window.scrollTo(0,0);lastPath=path;document.title='HELLSCRIPT 개발 위키';if(parts[0]==='page')page(parts[1],params);else if(parts[0]==='db')database(parts[1],params);else if(['tree','search','tags'].includes(parts[0]))tree(params,parts[0]);else missing();}
  $('#sync').textContent=(PUBLIC?'읽기 전용 · ':'원본 확인 · ')+D.generatedAt.slice(0,10);
  $('#search-form').onsubmit=e=>{e.preventDefault();location.hash='#/search?q='+encodeURIComponent($('#search').value.trim())};
  $('#menu').onclick=()=>{const open=document.body.classList.toggle('menu-open');$('#menu').setAttribute('aria-expanded',String(open))};
  $('#close-detail').onclick=()=>{const [path,q='']=location.hash.split('?'),params=new URLSearchParams(q);params.delete('record');history.replaceState(null,'',path+(params.size?'?'+params:''));$('#detail').close()};
  $('#detail').addEventListener('cancel',()=>{const [path,q='']=location.hash.split('?'),params=new URLSearchParams(q);params.delete('record');history.replaceState(null,'',path+(params.size?'?'+params:''))});
  document.addEventListener('keydown',e=>{if(e.key==='/'&&!['INPUT','TEXTAREA','SELECT'].includes(document.activeElement.tagName)){e.preventDefault();$('#search').focus()}if(e.key==='Escape'){document.body.classList.remove('menu-open');$('#menu').setAttribute('aria-expanded','false')}});
  window.addEventListener('hashchange',route);route();
})();
