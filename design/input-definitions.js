'use strict';
/* Separate read-only source catalog. Does not merge into DATA.parameters or call any API. */
window.InputDefinitions=(()=>{
 const h=x=>String(x??'').replace(/[&<>"']/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
 let request=0;
 const href=pointer=>'#/db/input-definitions/'+encodeURIComponent(pointer);
 const kind=f=>f.valueType==='decimal_string'?'십진 문자열 · 원문 유지':'정수';
 const group=f=>f.group==='training'?'단련 입력':'능력치 상한';
 const value=f=>f.value===null?'미정 · 0/무제한 아님':String(f.value);
 function validate(payload){
  if(!payload||payload.schemaVersion!==1||payload.readOnly!==true||!Array.isArray(payload.fields))throw Error('입력 정의 자료 형식이 올바르지 않습니다.');
  if(!payload.source||payload.source.name!=='CURRENT_RULES.json'||!/^[a-f0-9]{64}$/.test(payload.source.sha256))throw Error('정본 식별 정보가 없습니다.');
  if(payload.fields.length!==0&&payload.fields.length!==10)throw Error('정의10개가 완전하게 로드되지 않았습니다.');
  const pointers=new Set();
  for(const f of payload.fields){
   if(!f||typeof f.sourcePointer!=='string'||!f.sourcePointer.startsWith('/rules/')||pointers.has(f.sourcePointer))throw Error('정의 식별자가 누락되거나 중복됐습니다.');
   pointers.add(f.sourcePointer);
   for(const key of ['sourceNamespace','sourceKey','label','unit','meaning','relation','planId','tableId','consumerStatus'])if(typeof f[key]!=='string'||!f[key])throw Error('정의 설명이 누락됐습니다.');
   if(f.valueType==='integer'){if(!Number.isSafeInteger(f.value))throw Error('정수 정의의 값/형이 다릅니다.');}
   else if(f.valueType==='decimal_string'){if(typeof f.value!=='string'||!/^\d+(?:\.\d+)?$/.test(f.value))throw Error('십진 원문 문자열의 값/형이 다릅니다.');}
   else throw Error('지원하지 않는 정의 자료형입니다.');
   if(!['training','caps'].includes(f.group)||f.registered!==false||f.editable!==false||f.gameApplied!==false||f.runtimeParameterKey!==null)throw Error('읽기 전용/미등록 경계가 올바르지 않습니다.');
   if(f.changeRange?.status!=='UNDECIDED_NOT_UNBOUNDED'||f.changeRange.min!==null||f.changeRange.max!==null||!Array.isArray(f.relatedPointers))throw Error('미정 범위/관계 정의가 올바르지 않습니다.');
  }
  for(const f of payload.fields)if(f.relatedPointers.some(p=>!pointers.has(p)||p===f.sourcePointer))throw Error('연결 정의가 없습니다.');
  return payload;
 }
 function filter(fields,query,selected=''){
  const q=query.toLowerCase();return fields.filter(f=>(!selected||f.group===selected)&&(!q||[f.label,f.sourceNamespace,f.sourceKey,f.sourcePointer,value(f),f.unit,f.meaning,f.relation].join(' ').toLowerCase().includes(q)));
 }
 function header(base,count){return `<div class="eyebrow">DB / 읽기 전용 입력 정의</div><h1>입력 정의 도감</h1><p class="lede">기존 기획에 있는 단련·상한 정의를 읽고 연결을 확인합니다. 운영 파라미터 등록이나 게임 적용은 하지 않습니다.</p><div class="scope-strip"><span>현재 등록 <b>${base.parameters.length}</b> / 로컬 편집 <b>${base.parameterCoverage.editableCount}</b></span><span>추가 정의 <b>${count===null?'로드 전':count}</b> · 별도 정적 자료</span><span>추가 정의 <b>편집·게임 적용 안 됨</b></span></div>`;}
 function tech(payload,f){return `<details><summary>출처 식별 · 형·범위 · 원문 근거</summary><p>source namespace: <code>${h(f.sourceNamespace)}</code><br>source key: <code>${h(f.sourceKey)}</code><br>JSON pointer: <code>${h(f.sourcePointer)}</code></p><p>원문 자료형: ${h(kind(f))} / 단위: ${h(f.unit)}</p><p>변경 허용 범위는 미정입니다. min/max의 null은 무제한·0을 뜻하지 않습니다. 새 runtime key는 발급하지 않았습니다.</p><p>${h(payload.source.name)}<br>SHA256 <code>${h(payload.source.sha256)}</code><br>기존 수치표: <code>${h(f.tableId)}</code></p><p>공개 정본 식별과 pointer만 제공합니다. 비공개 원문이나 로컬 경로를 제공하지 않습니다.</p></details>`;}
 function detail(payload,pointer){
  const f=payload.fields.find(x=>x.sourcePointer===pointer);
  if(!f)return '<div class="notice error" role="status">입력 정의를 찾을 수 없습니다. 이 주소를 운영 API key로 해석하지 않습니다.</div><a class="button" href="#/db/input-definitions">정의 목록으로</a>';
  const others=f.relatedPointers.map(p=>payload.fields.find(x=>x.sourcePointer===p));
  return `<div class="breadcrumbs"><a href="#/db/parameters">파라미터 DB</a> / <a href="#/db/input-definitions">입력 정의</a> / ${h(group(f))}</div><h2>${h(f.label)}</h2><div class="database-row"><span><strong>${h(value(f))}</strong><small>${h(f.unit)} · ${h(kind(f))}</small></span><span class="badge pending">정의만 제공 · 편집 불가</span></div><p>${h(f.meaning)}</p><section><h2>관계와 적용 의미</h2><p>${h(f.relation)}</p></section><div class="notice"><b>운영 등록 안 됨 · 수정 불가 · 게임 적용 안 됨</b><br>${h(f.consumerStatus)}<br>개별 소비 미확인은 제품 전체 기능 부재 판정이 아닙니다.</div><h2>기획·기존 수치표</h2><a class="button" href="#/plan/${h(f.planId)}">장비·성장 기획에서 읽기 →</a><h2>같은 분류의 다른 정의</h2><div class="link-pills">${others.map(x=>`<a href="${h(href(x.sourcePointer))}">${h(x.label)}</a>`).join('')}</div>${tech(payload,f)}`;
 }
 function list(payload){
  const fields=payload.fields;const main=document.querySelector('#definition-body');
  if(!fields.length){main.innerHTML='<div class="notice" role="status">입력 정의 자료가 비어 있습니다. 불러온 추가 정의가 없으며 기존 등록 파라미터와 원장은 변경되지 않았습니다.</div>';return;}
  main.innerHTML='<div class="toolbar"><input id="definition-query" type="search" aria-label="입력 정의 검색" placeholder="이름, source key, 단위, 관련 설명 검색"><select id="definition-group" aria-label="입력 정의 분류"><option value="">모든 정의</option><option value="training">단련 입력</option><option value="caps">능력치 상한</option></select></div><p class="count" id="definition-count" role="status" aria-live="polite"></p><div id="definition-results"></div>';
  const input=document.querySelector('#definition-query'),select=document.querySelector('#definition-group');const view=WikiIA.viewState();input.value=view.query||'';select.value=['training','caps'].includes(view.group)?view.group:'';
  const update=()=>{const rows=filter(fields,input.value,select.value);WikiIA.saveView({query:input.value,group:select.value});document.querySelector('#definition-count').textContent=`${rows.length}개 정의 · 전체 ${fields.length}개 · 운영 등록 아님`;document.querySelector('#definition-results').innerHTML=rows.length?`<div class="table-wrap"><table><thead><tr><th>이름 · 분류</th><th>현재 확정값</th><th>자료형 · 단위</th></tr></thead><tbody>${rows.map(f=>`<tr data-definition-pointer="${h(f.sourcePointer)}"><td><a href="${h(href(f.sourcePointer))}">${h(f.label)}</a><div class="meta">${h(group(f))}</div></td><td><b>${h(value(f))}</b></td><td>${h(kind(f))}<br>${h(f.unit)}</td></tr>`).join('')}</tbody></table></div>`:'<div class="notice" role="status">검색 결과가 없습니다. 검색어나 분류를 바꿔 보세요.</div>';};input.oninput=update;select.onchange=update;update();
 }
 async function show(base,pointer){
  const sequence=++request,hash=location.hash,main=document.querySelector('#main');
  main.innerHTML=header(base,null)+'<div class="notice" role="status">별도 입력 정의 자료를 불러오는 중입니다. 기존 등록 파라미터와 원장은 변경하지 않습니다.</div>';
  try{
   const response=await fetch('input-definitions.json',{cache:'no-store'});if(!response.ok)throw Error('HTTP '+response.status);
   const payload=validate(await response.json());if(sequence!==request||location.hash!==hash)return;
   main.innerHTML=header(base,payload.fields.length)+'<div id="definition-body"></div>';
   if(pointer)document.querySelector('#definition-body').innerHTML=detail(payload,pointer);else list(payload);
  }catch(error){
   if(sequence!==request||location.hash!==hash)return;
   main.innerHTML=header(base,null)+`<div class="notice error" role="alert">입력 정의 자료를 불러오지 못했습니다. 추가 정의가 운영에 등록·적용된 것으로 표시하지 않습니다.</div><div class="operation-actions"><button id="definitions-retry">다시 불러오기</button><a class="button" href="#/db/parameters">기존 파라미터 DB로</a></div><details><summary>로드 오류 상세</summary><p>${h(error.message)}</p></details>`;
   document.querySelector('#definitions-retry').onclick=()=>show(base,pointer);
  }
 }
 return {show,validate,filter,href,value,kind};
})();
