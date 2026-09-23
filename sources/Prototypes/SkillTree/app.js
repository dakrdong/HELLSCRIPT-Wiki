(function () {
  'use strict';
  const data = window.SKILL_TREE_DATA, E = window.SkillTreeEngine;
  const $ = id => document.getElementById(id);
  const escape = value => String(value).replace(/[&<>"']/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
  const strings = {
    ko: { eyebrow:'직업별 성장 설계',active:'액티브',passive:'패시브',ultimate:'궁극기',all:'모든 스킬',growth:'성장 미리보기',tree:'스킬 트리',stages:'성장 구간',heroLevel:'플레이어 레벨',remaining:'남은 강화 포인트',undo:'되돌리기',reset:'투자 초기화',export:'빌드 저장',import:'불러오기',fit:'맞춤',allShort:'직접 효과 연계',treeHint:'스킬을 선택하면 선행 경로와 효과를 확인할 수 있습니다.',clearPath:'경로 강조 해제',loadout:'전투 구성 미리보기',loadoutHint:'해금과 장착은 별개입니다. 선행 스킬을 계속 장착할 필요는 없습니다.',examples:'추천 성장 경로',rules:'성장 규칙 보기',scope:'40레벨 스킬 트리 설계안 · 이 페이지의 배분은 게임 저장에 반영되지 않습니다.',search:'이름이나 효과로 찾기',unlocked:'{count} / 37 해금',matches:'{count}개 일치',rank:'등급',available:'해금됨',invested:'강화됨',lockedLevel:'레벨 부족',lockedPath:'선행 조건 필요',baseEffect:'기본 1등급 효과',requirements:'해금 조건',allRequired:'아래 조건을 모두 충족',directSources:'직접 연계 스킬 · 1개 이상 활성화',singleRequired:'효과에 필요한 선행 스킬',levelOnly:'해당 레벨에 도달하면 열립니다. 별도의 선행 스킬은 없습니다.',ultimateGateSummary:'궁극기 공통 · Lv. {level} + Lv. {from}–{to} 스킬 하나 활성화',previousPhaseCondition:'Lv. {from}–{to} 구간의 스킬 중 하나 활성화',gateWitness:'충족한 스킬: {name}',ultimateHint:'직전 구간의 액티브·패시브 중 어느 것이든 가능합니다. 활성화는 해금 기준이며 장착할 필요는 없습니다.',levelCondition:'플레이어 Lv. {level} 이상',rankCondition:'{name} 활성화 · 기본 {rank}등급',root:'처음부터 사용할 수 있는 시작 스킬입니다.',learnPath:'선행 경로 열기 · {cost} P',raiseLevel:'Lv. {level}부터 열 수 있습니다',upgrade:'강화 +1 · 1 P',refund:'1 P 환급',noPoints:'강화 포인트가 부족합니다.',maxRank:'최대 등급',equip:'장착하기',unequip:'장착 해제',ultimateSwap:'이 궁극기 선택',equipped:'장착 중',pointHint:'조건을 달성하면 기본 1등급이 무료로 열립니다.',followup:'이후에 이어지는 스킬',endpoint:'이 스킬을 선행 조건으로 요구하는 후속 스킬은 없습니다.',blank:'비어 있음',slotUnlock:'Lv. {level}',emptyHint:'해금된 스킬을 골라 장착하세요.',resetDone:'투자와 장착 구성을 초기화했습니다. 되돌리기로 복구할 수 있습니다.',undoDone:'이전 배분을 복구했습니다.',changeDone:'성장 배분을 갱신했습니다.',refundDone:'강화 포인트를 환급했습니다. 기본 1등급 활성화는 유지됩니다.',levelDone:'레벨을 낮추면서 사용할 수 없는 투자와 장착을 정리했습니다. 되돌리기로 복구할 수 있습니다.',pathDone:'선행 경로를 열었습니다. {cost} P를 사용했습니다.',exampleDone:'추천 경로를 적용했습니다. 되돌리기로 이전 배분을 복구할 수 있습니다.',importDone:'빌드 파일을 불러왔습니다.',saved:'이 브라우저의 배분을 JSON 파일로 저장했습니다.',loadError:'이 페이지에서 저장한 유효한 빌드 JSON 파일을 선택해 주세요.',storageError:'브라우저 저장소를 사용할 수 없습니다. 빌드 저장으로 파일을 보관해 주세요.',slotError:'해당 종류의 장착 칸이 가득 찼습니다. 먼저 다른 스킬을 해제해 주세요.',lockedError:'레벨과 선행 조건을 먼저 충족해 주세요.',rankHint:'선행 스킬은 기본 1등급 활성화면 충분합니다. 포인트는 원하는 스킬의 강화에 사용합니다.',close:'상세 닫기',classLabel:'직업 선택',treeLabel:'스크롤 가능한 스킬 트리',zoomOut:'축소',zoomIn:'확대',moreInfo:'스킬 상세',refundWarning:'환급하면 후속 스킬 {count}개가 잠깁니다.',loadoutSlots:'{used} / {total}칸',rulesList:['기본 1등급은 무료입니다. 플레이어 레벨과 모든 필수 선행 조건을 충족하면 자동으로 해금됩니다.','강화 포인트는 2레벨부터 레벨당 1개, 40레벨에서 총 39개입니다. 일반 스킬은 최대 5등급, 궁극기는 1등급입니다.','연결선은 특정 스킬의 효과를 직접 사용하거나 강화하는 관계입니다. 선행 스킬의 기본 1등급 활성화만 요구하며 추가 강화나 장착은 필요하지 않습니다. 독립적인 스킬은 플레이어 레벨로 열립니다.','일반 액티브 4개·패시브 3개·궁극기 1개를 장착합니다. 패시브 칸은 3·6·10레벨에 열립니다. 기본 공격은 별도입니다.','궁극기는 40레벨 이상이고 직전 30~39레벨 구간의 스킬 하나가 활성화되어 있으면 열립니다. 레벨을 낮추면 사용할 수 없는 투자와 장착을 정리합니다.','선행 관계와 일부 해금 레벨은 이 HTML의 설계안입니다. 스킬 이름·1등급 효과·아이콘은 프로젝트 데이터를 사용합니다.'] },
    en: { eyebrow:'CLASS PROGRESSION DESIGN',active:'Active',passive:'Passive',ultimate:'Ultimate',all:'All skills',growth:'Progression preview',tree:'Skill tree',stages:'Progression stages',heroLevel:'Player level',remaining:'Upgrade points left',undo:'Undo',reset:'Reset allocation',export:'Save build',import:'Load build',fit:'Fit',allShort:'Direct effect link',treeHint:'Select a skill to trace its prerequisites and inspect its effects.',clearPath:'Clear path',loadout:'Combat loadout preview',loadoutHint:'Unlocking and equipping are separate. Prerequisites do not need to stay equipped.',examples:'Suggested progression',rules:'Progression rules',scope:'Level 40 skill tree proposal · Allocations on this page do not change your game save.',search:'Search by name or effect',unlocked:'{count} / 37 unlocked',matches:'{count} matches',rank:'Rank',available:'Unlocked',invested:'Upgraded',lockedLevel:'Level locked',lockedPath:'Prerequisites needed',baseEffect:'Base rank 1 effect',requirements:'Unlock conditions',allRequired:'Meet all of these',directSources:'Activate at least one direct effect source',singleRequired:'Skill required by this effect',levelOnly:'Unlocks at this level with no prerequisite skill.',ultimateGateSummary:'Ultimates · Lv. {level} + any Lv. {from}–{to} skill unlocked',previousPhaseCondition:'Any one skill in the Lv. {from}–{to} stage is unlocked',gateWitness:'Satisfied by: {name}',ultimateHint:'Any active or passive in the previous stage qualifies. Unlocked is sufficient; equipping it is not required.',levelCondition:'Player level {level} or higher',rankCondition:'{name} unlocked · base rank {rank}',root:'Your starting skill, available from the beginning.',learnPath:'Open prerequisite path · {cost} P',raiseLevel:'Available from Lv. {level}',upgrade:'Upgrade +1 · 1 P',refund:'Refund 1 P',noPoints:'Not enough upgrade points.',maxRank:'Maximum rank',equip:'Equip skill',unequip:'Unequip',ultimateSwap:'Choose this ultimate',equipped:'Equipped',pointHint:'Base rank 1 unlocks for free when its conditions are met.',followup:'Leads to',endpoint:'No later skill requires this skill as a prerequisite.',blank:'Empty',slotUnlock:'Lv. {level}',emptyHint:'Select an unlocked skill to equip it.',resetDone:'Allocation and loadout reset. Undo restores the previous build.',undoDone:'Previous allocation restored.',changeDone:'Progression updated.',refundDone:'Upgrade points refunded. The unlocked base rank is retained.',levelDone:'Unavailable upgrades and equipped skills were removed at this lower level. Undo restores them.',pathDone:'Prerequisite path opened using {cost} P.',exampleDone:'Suggested build applied. Undo restores your previous allocation.',importDone:'Build file loaded.',saved:'This browser build was saved as a JSON file.',loadError:'Choose a valid build JSON saved by this planner.',storageError:'Browser storage is unavailable. Use Save build to keep a copy.',slotError:'These equipment slots are full. Unequip another skill first.',lockedError:'Meet the level and prerequisite conditions first.',rankHint:'Unlocked base rank 1 satisfies a prerequisite. Spend points to upgrade the skills you choose.',close:'Close details',classLabel:'Choose a class',treeLabel:'Scrollable skill tree',zoomOut:'Zoom out',zoomIn:'Zoom in',moreInfo:'Skill details',refundWarning:'Refunding will lock {count} dependent skills.',loadoutSlots:'{used} / {total} slots',rulesList:['Base rank 1 is free. It unlocks automatically once the player level and all required prerequisites are met.','Earn one upgrade point per level after level 1: 39 points at level 40. Normal skills cap at rank 5; ultimates at rank 1.','Lines connect skills that directly use or improve a particular skill. Only unlocked base rank 1 is required, with no further upgrade or equipment requirement. Independent skills unlock by player level.','Equip four normal actives, three passives and one ultimate. Passive slots unlock at levels 3, 6 and 10. Basic attack is separate.','Ultimates require level 40 and any unlocked skill in the preceding level 30–39 stage. Lowering your level removes unavailable upgrades and equipped skills.','Prerequisites and some unlock levels are proposed here. Skill names, rank 1 effects and icons come from the project catalog.'] }
  };
  const STORE = 'HELLSCRIPT_SKILL_TREE_PREVIEW_V1', LANG = STORE + '_LANGUAGE';
  let language = 'ko', profiles = {}, heroId = document.body.dataset.class, selected, trace = false, undoState = null;
  let zoom = null, query = '', kind = 'all', toastTimer;
  try {
    language = localStorage.getItem(LANG) === 'en' ? 'en' : 'ko';
    const saved = JSON.parse(localStorage.getItem(STORE) || '{}');
    for (const c of data.classes) profiles[c.id] = saved.profiles?.[c.id]?.classId === c.id ? E.normalize(data,saved.profiles[c.id]) : E.empty(data,c.id);
  } catch (_) { for (const c of data.classes) profiles[c.id] = E.empty(data,c.id); }
  const requested = new URLSearchParams(location.search).get('class');
  if (data.classes.some(c => c.id === requested)) heroId = requested;
  if (!data.classes.some(c => c.id === heroId)) heroId = 'Warrior';
  for (const c of data.classes) if (!profiles[c.id]) profiles[c.id] = E.empty(data,c.id);
  selected = E.context(data,heroId).nodes[0].id;
  const state = () => profiles[heroId];
  const skillNames = new Map(data.classes.flatMap(c => c.nodes.map(n => [n.id,n.name])));
  skillNames.set('BASIC',{ko:'기본 공격',en:'Basic Attack'});
  function label(value) {
    const text=value?.[language] || value?.ko || '';
    return text.replace(/(?<![A-Za-z0-9_])([WAM]P?\d{2}|BASIC)(?![A-Za-z0-9_])(으로|[을를은는이가과와로])?/g, (match,id,particle='') => {
      const name=skillNames.get(id)?.[language] || skillNames.get(id)?.ko;
      if(!name)return match;
      if(language==='ko'&&particle){
        const last=name.charCodeAt(name.length-1);
        if(last>=0xac00&&last<=0xd7a3){
          const final=(last-0xac00)%28, pair=['을를','은는','이가','과와'].find(p=>p.includes(particle));
          if(pair)particle=pair[final?0:1];
          else if(particle==='으로'||particle==='로')particle=final&&final!==8?'으로':'로';
        }
      }
      return name+particle;
    });
  }
  function t(key,args={}) { return String(strings[language][key] || strings.ko[key] || key).replace(/\{(\w+)\}/g,(_,name)=>String(args[name] ?? '')); }
  function notify(message) { clearTimeout(toastTimer); $('toast').textContent=message; $('toast').classList.add('visible'); toastTimer=setTimeout(()=>$('toast').classList.remove('visible'),4800); }
  function save() { try { localStorage.setItem(STORE,JSON.stringify({profiles})); localStorage.setItem(LANG,language); } catch (_) { notify(t('storageError')); } }
  function commit(next,message) { undoState={heroId,profiles:E.clone(profiles)}; profiles[heroId]=next; save(); render(); if(message)notify(message); }
  function closeDetails() { document.body.classList.remove('details-open'); $('detailsBackdrop').hidden=true; $('inspector').removeAttribute('role'); $('inspector').removeAttribute('aria-modal'); }
  function selectSkill(id,scroll=false,open=true) {
    if(!E.context(data,heroId).byId[id])return;
    selected=id;trace=true;render();
    if(scroll) { const n=E.context(data,heroId).byId[id]; $('treeViewport').scrollTo({top:Math.max(0,n.y*scale()-60),left:Math.max(0,n.x*scale()-$('treeViewport').clientWidth/2+60),behavior:'smooth'}); }
    if(open&&matchMedia('(max-width:850px)').matches) { document.body.classList.add('details-open'); $('detailsBackdrop').hidden=false; $('inspector').setAttribute('role','dialog'); $('inspector').setAttribute('aria-modal','true'); $('closeDetails').focus({preventScroll:true}); }
  }
  function scale() { const width=$('treeViewport').clientWidth; return zoom ?? (width<650?0.9:Math.min(1.08,Math.max(.7,(width-18)/878))); }
  function sizeCanvas() {
    const hero=E.context(data,heroId).hero,s=scale();
    $('treeCanvas').style.transform=`scale(${s})`;
    $('treeSizer').style.width=`${hero.width*s}px`; $('treeSizer').style.height=`${hero.height*s}px`;
    alignBranchHeaders();
  }
  function alignBranchHeaders() {
    const s=scale(),viewport=$('treeViewport'),offset=Math.max(0,(viewport.clientWidth-878*s)/2)-viewport.scrollLeft;
    [...$('branchHeaders').children].forEach((el,i)=>{el.style.left=(offset+(66+i*270)*s)+'px';el.style.width=(236*s)+'px';});
  }
  $('treeViewport').addEventListener('scroll',alignBranchHeaders,{passive:true});
  function buildCanvas() {
    const hero=E.context(data,heroId).hero;
    $('treeCanvas').style.width=hero.width+'px'; $('treeCanvas').style.height=hero.height+'px';
    const bands=hero.bands.map(b=>`<div class="phase-band" style="top:${b.top}px"><span class="phase-level">${b.from===b.to?'40':b.from+'–'+b.to}</span><strong>${escape(label(b.name))}</strong>${b.from===data.ultimateUnlockLevel?`<small class="ultimate-gate"><span class="gate-state" aria-hidden="true">○</span> ${escape(t('ultimateGateSummary',{level:data.ultimateUnlockLevel,...data.ultimatePrerequisite}))}</small>`:`<small>${escape(label(b.note))}</small>`}</div>`).join('');
    const byId=Object.fromEntries(hero.nodes.map(n=>[n.id,n]));
    const edges=hero.nodes.flatMap(n=>[...n.all,...n.oneOf].map(r=>{
      const from=byId[r.id],x1=from.x+56,y1=from.y+120,x2=n.x+56,y2=n.y-8;
      const bend=Math.min(70,Math.max(12,(y2-y1)*.3));
      return `<path class="edge ${n.oneOf.includes(r)?'one-of':''}" data-from="${r.id}" data-to="${n.id}" data-rank="${r.rank}" d="M${x1},${y1} C${x1},${y1+bend} ${x2},${y2-bend} ${x2},${y2}" marker-end="url(#arrow)"></path>`;
    })).join('');
    const nodes=hero.nodes.map(n=>`<button type="button" class="skill-node kind-${n.kind}" id="node-${n.id}" data-node="${n.id}" style="left:${n.x}px;top:${n.y}px"><span class="icon-frame"><img src="${n.icon}" alt="" loading="lazy" decoding="async"><span class="rank-badge"></span></span><span class="node-name">${escape(label(n.name))}</span><span class="node-meta"><strong>Lv. ${n.level}</strong></span></button>`).join('');
    $('treeCanvas').innerHTML=[336,606].map(x=>`<div class="lane-line" style="left:${x}px"></div>`).join('')+bands+`<svg class="edges" width="${hero.width}" height="${hero.height}" aria-hidden="true"><defs><marker id="arrow" viewBox="0 0 8 8" refX="6" refY="4" markerWidth="5" markerHeight="5" orient="auto-start-reverse"><path d="M 0 0 L 8 4 L 0 8" fill="#78815f"></path></marker></defs>${edges}</svg>`+nodes;
    sizeCanvas();
  }
  function translate() {
    document.documentElement.lang=language; document.title=`HELLSCRIPT · ${label(E.context(data,heroId).hero.name)} · ${language==='ko'?'스킬 트리':'Skill tree'}`;
    document.querySelectorAll('[data-i18n]').forEach(el=>el.textContent=t(el.dataset.i18n));
    document.querySelectorAll('[data-i18n-label]').forEach(el=>el.setAttribute('aria-label',t(el.dataset.i18nLabel)));
    $('language').textContent=language==='ko'?'EN':'한국어'; $('search').placeholder=t('search'); $('search').setAttribute('aria-label',t('search'));
    for(const id of ['level','levelNumber'])$(id).setAttribute('aria-label',t('heroLevel'));
    $('kind').setAttribute('aria-label',t('all')); $('classNav').setAttribute('aria-label',t('classLabel'));
    $('closeDetails').setAttribute('aria-label',t('close'));$('inspector').setAttribute('aria-label',t('moreInfo'));
    $('treeViewport').setAttribute('aria-label',t('treeLabel'));$('zoomOut').setAttribute('aria-label',t('zoomOut'));$('zoomIn').setAttribute('aria-label',t('zoomIn'));
    [...$('kind').options].forEach(o=>o.textContent=t(o.value));
    $('ruleList').innerHTML=strings[language].rulesList.map(s=>`<li>${escape(s)}</li>`).join('');
  }
  function render(full=false) {
    const view=E.evaluate(data,state()),hero=view.hero;
    if(full) {
      try {const url=new URL(location.href);url.searchParams.set('class',heroId);history.replaceState(null,'',url);}catch(_){}
      document.documentElement.style.setProperty('--accent',hero.accent);
      $('classNav').setAttribute('role','tablist');
      $('classNav').innerHTML=data.classes.map(c=>`<button id="class-${c.id}" class="class-tab" role="tab" aria-selected="${heroId===c.id}" tabindex="${heroId===c.id?'0':'-1'}" aria-controls="treePanel" type="button" data-class="${c.id}"><img src="${c.nodes[0].icon}" alt=""><span>${escape(label(c.name))}<small>${c.id.toUpperCase()}</small></span></button>`).join('');
      document.querySelector('.tree-panel').id='treePanel'; $('treePanel').setAttribute('role','tabpanel'); $('treePanel').setAttribute('aria-labelledby','class-'+heroId);
      $('classTitle').textContent=language==='ko'?label(hero.name)+' 스킬 트리':label(hero.name)+' skill tree'; $('classSubtitle').textContent=label(hero.subtitle);
      $('branchHeaders').innerHTML=hero.branches.map((n,i)=>`<span>${String(i+1).padStart(2,'0')} · ${escape(label(n))}</span>`).join('');
      $('phaseNav').innerHTML=hero.bands.map((b,i)=>`<button type="button" data-phase="${i}"><b>Lv. ${b.from===b.to?'40':b.from+'–'+b.to}</b><span>${escape(label(b.name))}</span></button>`).join('');
      $('examples').innerHTML=hero.examples.map((e,i)=>`<button type="button" data-example="${i}">${escape(label(e.name))}<small>Lv. ${e.level}</small></button>`).join('');
      translate();buildCanvas();
    }
    $('level').value=state().level;$('levelNumber').value=state().level;$('pointsRemaining').innerHTML=`${view.remaining} <small>/ ${view.budget} P</small>`;
    $('undo').disabled=!undoState;$('unlockCount').textContent=t('unlocked',{count:view.unlocked});
    const gate=document.querySelector('.ultimate-gate'),gateMet=state().level>=data.ultimateUnlockLevel&&view.ultimateReady;
    gate.classList.toggle('met',gateMet);gate.querySelector('.gate-state').textContent=gateMet?'✓':'○';
    const ancestors=trace?E.ancestors(data,heroId,selected):new Set();let matches=0;
    for(const n of hero.nodes) {
      const button=$('node-'+n.id),rank=view.ranks[n.id];
      const match=(kind==='all'||kind===n.kind)&&(!query||[n.id,label(n.name),label(n.description),...(n.tags||[])].join(' ').toLowerCase().includes(query));
      if(match)matches++;
      const status=!view.available[n.id]?(state().level<n.level?'lockedLevel':'lockedPath'):(rank>1?'invested':'available');
      button.className=`skill-node kind-${n.kind} ${view.available[n.id]?'is-unlocked':'is-locked'} ${rank>1?'is-invested':''} ${state().equipped.includes(n.id)?'is-equipped':''} ${n.id===selected&&trace?'is-selected':''} ${ancestors.has(n.id)?'is-ancestor':''} ${match?'':'is-filtered'}`;
      button.setAttribute('aria-label',`${label(n.name)} · ${t(n.kind)} · Lv. ${n.level} · ${t(status)} · ${t('rank')} ${rank}/${n.maxRank}`);
      button.setAttribute('aria-pressed',String(n.id===selected&&trace));button.title=button.getAttribute('aria-label');
      button.querySelector('.rank-badge').textContent=`${rank}/${n.maxRank}`;
    }
    $('matchCount').textContent=(query||kind!=='all')?t('matches',{count:matches}):'';
    document.querySelectorAll('.edge').forEach(edge=>{
      const traced=trace&&(edge.dataset.to===selected||ancestors.has(edge.dataset.to));
      edge.classList.toggle('traced',traced);edge.classList.toggle('unrelated',trace&&!traced);
      edge.classList.toggle('met',view.ranks[edge.dataset.from]>=Number(edge.dataset.rank));
    });
    renderDetails(view);renderLoadout(view);
  }
  function renderDetails(view) {
    const n=view.byId[selected]||view.nodes[0], rank=view.ranks[n.id], available=view.available[n.id],plan=E.pathPlan(data,state(),n.id);
    const status=available?(rank>1?'invested':'available'):(state().level<n.level?'lockedLevel':'lockedPath');
    const row=(condition,met,id)=>`<li class="${met?'met':''}"><span class="require-state" aria-hidden="true">${met?'✓':'○'}</span>${id?`<button type="button" data-jump="${id}">${escape(condition)}</button>`:`<span>${escape(condition)}</span>`}</li>`;
    const req=r=>row(t('rankCondition',{name:label(view.byId[r.id].name),rank:r.rank}),view.ranks[r.id]>=r.rank,r.id);
    const next=view.nodes.filter(k=>[...k.all,...k.oneOf].some(r=>r.id===n.id)||(k.kind==='ultimate'&&view.ultimateCandidates.some(p=>p.id===n.id)));
    const witness=n.kind==='ultimate'?view.ultimateCandidates.find(p=>view.available[p.id]):null;
    let primary;
    if(!available) primary=`<button type="button" class="primary" data-action="path" ${plan.ok?'':'disabled'}>${escape(plan.error==='LEVEL'?t('raiseLevel',{level:plan.level}):t('learnPath',{cost:plan.cost}))}</button>`;
    else primary=`<button type="button" class="primary" data-action="upgrade" ${rank>=n.maxRank||view.remaining===0?'disabled':''}>${escape(rank>=n.maxRank?t('maxRank'):t('upgrade'))}</button>`;
    const afterRefund=E.evaluate(data,E.refund(data,state(),n.id));
    const lost=view.nodes.filter(k=>view.available[k.id]&&!afterRefund.available[k.id]).length;
    const equipped=state().equipped.includes(n.id);
    $('details').innerHTML=`<div class="detail-hero"><img src="${n.icon}" alt=""><div><span class="detail-id">${escape(t(n.kind).toUpperCase())}</span><h2>${escape(label(n.name))}</h2><span class="detail-type">${escape(label(view.hero.branches[n.branch]))}</span></div></div>
      <div class="detail-status"><b>${escape(t(status))}</b><span>${escape(t('rank'))} ${rank} / ${n.maxRank}</span></div>
      <div class="detail-description">${escape(label(n.description))}</div><div class="detail-caption">${escape(t('baseEffect'))}</div>
      <section class="detail-section"><h3>${escape(t('requirements'))}</h3><ul class="requirements">${row(t('levelCondition',{level:n.level}),state().level>=n.level)}</ul>
      ${n.all.length?`<p class="condition-label">${escape(t(n.all.length===1?'singleRequired':'allRequired'))}</p><ul class="requirements">${n.all.map(req).join('')}</ul>`:''}
      ${n.oneOf.length?`<p class="condition-label">${escape(t('directSources'))}</p><ul class="requirements direct-sources">${n.oneOf.map(req).join('')}</ul>`:''}
      ${n.kind==='ultimate'?`<ul class="requirements ultimate-requirement">${row(t('previousPhaseCondition',data.ultimatePrerequisite),view.ultimateReady)}${witness?row(t('gateWitness',{name:label(witness.name)}),true,witness.id):''}</ul><p class="action-note">${escape(t('ultimateHint'))}</p>`:''}
      ${!n.all.length&&!n.oneOf.length&&n.kind!=='ultimate'?`<p class="action-note">${escape(t(n.level===1?'root':'levelOnly'))}</p>`:''}</section>
      ${n.rationale?`<p class="detail-reason">${escape(label(n.rationale))}</p>`:''}
      <div class="detail-actions">${primary}<button type="button" data-action="refund" ${rank<=1?'disabled':''}>${escape(t('refund'))}</button><button type="button" class="equip-button" data-action="equip" ${available?'':'disabled'}>${escape(equipped?t('unequip'):n.kind==='ultimate'?t('ultimateSwap'):t('equip'))}</button></div>
      <p class="action-note">${escape(!available&&plan.error==='POINTS'?t('noPoints'):lost?t('refundWarning',{count:lost}):t('pointHint'))}</p>
      <p class="detail-caption">${escape(t('rankHint'))}</p>
      <section class="detail-section"><h3>${escape(t('followup'))}</h3><div class="next-skills">${next.length?next.map(k=>`<button type="button" data-jump="${k.id}">${escape(label(k.name))} · ${k.level}</button>`).join(''):`<span class="no-next">${escape(t('endpoint'))}</span>`}</div></section>`;
  }
  function renderLoadout(view) {
    $('loadout').innerHTML=['active','passive','ultimate'].map(category=>{
      const capacity=data.slots[category],open=E.slots(data,state(),category),ids=state().equipped.filter(id=>view.byId[id].kind===category);
      return `<div><div class="slot-group-label"><span>${escape(t(category))}</span><span>${t('loadoutSlots',{used:ids.length,total:open})}</span></div><div class="slot-row">${Array.from({length:capacity},(_,i)=>{
        const n=view.byId[ids[i]];
        return n?`<button type="button" class="loadout-slot" data-jump="${n.id}" aria-label="${escape(t('equipped')+' '+label(n.name))}"><img src="${n.icon}" alt=""><span>${escape(label(n.name))}</span></button>`:`<div class="loadout-slot"><div class="empty-slot">${i>=open?'·':'+'}</div><span class="${i>=open?'locked-slot':''}">${escape(i>=open?t('slotUnlock',{level:category==='ultimate'?data.ultimateUnlockLevel:data.passiveSlotLevels[i]}):t('blank'))}</span></div>`;
      }).join('')}</div></div>`;
    }).join('');
  }
  function switchClass(id) { closeDetails();heroId=id;selected=E.context(data,id).nodes[0].id;trace=false;render(true);$('treeViewport').scrollTo(0,0);save(); }
  document.addEventListener('click',event=>{
    const button=event.target.closest('button');if(!button)return;
    const d=button.dataset;
    if(d.class)return switchClass(d.class);
    if(d.node)return selectSkill(d.node);
    if(d.jump)return selectSkill(d.jump,true);
    if(d.phase!==undefined){closeDetails();$('treeViewport').scrollTo({top:E.context(data,heroId).hero.bands[Number(d.phase)].top*scale(),left:0,behavior:'smooth'});return;}
    if(d.example!==undefined){commit(E.example(data,heroId,Number(d.example)),t('exampleDone'));selected=E.context(data,heroId).hero.examples[Number(d.example)].targets[0];render();selectSkill(selected,true,false);window.scrollTo({top:0,behavior:'smooth'});return;}
    if(d.action) {
      try {
        let next,message=t('changeDone');
        if(d.action==='upgrade')next=E.invest(data,state(),selected);
        else if(d.action==='refund'){next=E.refund(data,state(),selected);message=t('refundDone');}
        else if(d.action==='equip')next=E.equip(data,state(),selected);
        else {const plan=E.pathPlan(data,state(),selected);next=E.applyPath(data,state(),selected);message=t('pathDone',{cost:plan.cost});}
        commit(next,message);document.querySelector(`[data-action="${d.action}"]`)?.focus({preventScroll:true});
      }catch(error){notify(t(error.message==='SLOTS'?'slotError':error.message==='POINTS'?'noPoints':'lockedError'));}
    }
  });
  function levelChanged(value) {
    const before=state(),next=E.normalize(data,{...before,level:value});
    if(next.level===before.level)return;
    const removed=next.stack.length<before.stack.length||next.equipped.length<before.equipped.length;
    commit(next,removed?t('levelDone'):null);
  }
  let levelGesture=null;
  $('level').addEventListener('pointerdown',()=>{levelGesture={heroId,profiles:E.clone(profiles)};});
  $('level').addEventListener('input',e=>{levelChanged(e.target.value);if(levelGesture)undoState=levelGesture;});
  $('level').addEventListener('change',()=>{levelGesture=null;});
  $('level').addEventListener('pointercancel',()=>{levelGesture=null;});
  $('levelNumber').addEventListener('change',e=>levelChanged(e.target.value));
  $('levelNumber').addEventListener('keydown',e=>{if(e.key==='Enter'){levelChanged(e.target.value);e.target.blur();}});
  $('search').addEventListener('input',e=>{query=e.target.value.trim().toLowerCase();render();});
  $('kind').addEventListener('change',e=>{kind=e.target.value;render();});
  $('language').addEventListener('click',()=>{language=language==='ko'?'en':'ko';clearTimeout(toastTimer);$('toast').classList.remove('visible');save();render(true);});
  $('zoomIn').addEventListener('click',()=>{zoom=Math.min(1.6,scale()+.1);sizeCanvas();});
  $('zoomOut').addEventListener('click',()=>{zoom=Math.max(.45,scale()-.1);sizeCanvas();});
  $('zoomFit').addEventListener('click',()=>{zoom=Math.max(.35,Math.min(1.08,($('treeViewport').clientWidth-18)/878));sizeCanvas();});
  $('clearSelection').addEventListener('click',()=>{trace=false;render();});
  $('reset').addEventListener('click',()=>commit(E.empty(data,heroId,state().level),t('resetDone')));
  $('undo').addEventListener('click',()=>{if(!undoState)return;const previous={heroId,profiles:E.clone(profiles)};({heroId,profiles}=undoState);undoState=previous;selected=E.context(data,heroId).nodes[0].id;save();render(true);notify(t('undoDone'));});
  $('export').addEventListener('click',()=>{const url=URL.createObjectURL(new Blob([E.serialize(data,state())],{type:'application/json'}));const a=document.createElement('a');a.href=url;a.download=`HELLSCRIPT-${heroId}-Lv${state().level}-build.json`;a.click();setTimeout(()=>URL.revokeObjectURL(url),1000);notify(t('saved'));});
  $('import').addEventListener('click',()=>$('importFile').click());
  $('importFile').addEventListener('change',async event=>{const file=event.target.files[0];if(!file)return;try{if(file.size>100000)throw Error('FORMAT');const next=E.parse(data,await file.text());undoState={heroId,profiles:E.clone(profiles)};heroId=next.classId;profiles[heroId]=next;selected=E.context(data,heroId).nodes[0].id;save();render(true);notify(t('importDone'));}catch(_){notify(t('loadError'));}event.target.value='';});
  $('closeDetails').addEventListener('click',()=>{closeDetails();$('node-'+selected)?.focus({preventScroll:true});});
  $('detailsBackdrop').addEventListener('click',closeDetails);
  document.addEventListener('keydown',event=>{
    if(event.key==='Escape'){const wasOpen=document.body.classList.contains('details-open');closeDetails();if(wasOpen)$('node-'+selected)?.focus({preventScroll:true});return;}
    if(event.key==='/'&&!['INPUT','SELECT','TEXTAREA'].includes(event.target.tagName)){event.preventDefault();$('search').focus();return;}
    if(event.target.dataset.class&&['ArrowLeft','ArrowRight'].includes(event.key)){event.preventDefault();const index=data.classes.findIndex(c=>c.id===heroId);switchClass(data.classes[(index+(event.key==='ArrowRight'?1:2))%3].id);$('class-'+heroId).focus();}
    if(event.key==='Tab'&&document.body.classList.contains('details-open')){
      const els=[...$('inspector').querySelectorAll('button:not(:disabled),a,input,select')];const first=els[0],last=els[els.length-1];
      if(event.shiftKey&&document.activeElement===first){event.preventDefault();last.focus();}else if(!event.shiftKey&&document.activeElement===last){event.preventDefault();first.focus();}
    }
  });
  new ResizeObserver(()=>{sizeCanvas();if(!matchMedia('(max-width:850px)').matches)closeDetails();}).observe($('treeViewport'));
  render(true);
})();
