(()=>{
const messages={
 ko:{title:'HELLSCRIPT 플레이 로그',intro:'전투 로그 수집 서버의 실제 응답을 확인합니다.',service:'수집 서버 상태',access:'사이트 접근 범위',private:'소유자 전용',accessDetail:'현재는 소유자 계정으로 로그인해야 이 사이트에 접근할 수 있습니다.',game:'일반 게임 연결',pending:'공개 API 승인 대기',gameDetail:'외부 게임의 직접 업로드는 사이트의 공개 접속 전환 후 확인합니다.',refresh:'다시 확인',response:'서버 상태 응답 보기',privacy:'업로드에는 게임 계정 인증이 필요합니다. 플레이 기록을 공개 조회하는 기능은 제공하지 않습니다.',ready:'수집 서버는 전투 원문 보관과 중복 전송 처리를 준비했습니다. 실제 일반 게임 업로드 검증은 공개 API 승인 후 진행합니다.',checking:'확인 중…',checkingDetail:'저장소와 서버 응답을 확인하고 있습니다.',healthy:'정상',healthyDetail:'수집 서비스와 로그 저장소가 정상 응답했습니다.',unavailable:'응답 확인 실패',unavailableDetail:'서버 상태를 확인하지 못했습니다. 잠시 후 다시 확인해 주세요.',login:'사이트 로그인 필요',loginDetail:'사이트 접근 세션을 확인해 주세요. 페이지를 새로고침하거나 다시 로그인한 뒤 확인할 수 있습니다.'},
 en:{title:'HELLSCRIPT player logs',intro:'Check the live response from the combat log collector.',service:'Collector status',access:'Site access',private:'Owner only',accessDetail:'This site currently requires the owner to sign in.',game:'Ordinary game connection',pending:'Public API approval pending',gameDetail:'Direct uploads from external games will be verified after public access is approved.',refresh:'Check again',response:'View health response',privacy:'Uploads require game account authentication. Player records cannot be read through public routes.',ready:'Original payload storage and duplicate handling are ready. Ordinary game uploads will be verified after public API approval.',checking:'Checking…',checkingDetail:'Checking the storage and service response.',healthy:'Healthy',healthyDetail:'The collector and log storage responded successfully.',unavailable:'Response unavailable',unavailableDetail:'Could not confirm server health. Please check again shortly.',login:'Site sign-in required',loginDetail:'Check your site access session. Refresh or sign in again before checking health.'}
};
let locale='ko',state='checking',inflight=false;
try{if(localStorage.getItem('hellscript-status-language')==='en')locale='en';}catch{}
const selector=document.getElementById('language');selector.value=locale;
function render(){
 const copy=messages[locale];document.documentElement.lang=locale;document.title=copy.title;
 document.querySelectorAll('[data-text]').forEach(element=>{element.textContent=copy[element.dataset.text];});
 const status=document.getElementById('status');status.textContent=copy[state];status.className='value '+state;
 document.getElementById('detail').textContent=copy[state+'Detail'];
 document.getElementById('refresh').disabled=inflight;
}
selector.addEventListener('change',()=>{locale=selector.value==='en'?'en':'ko';try{localStorage.setItem('hellscript-status-language',locale);}catch{}render();});
async function check(){
 if(inflight)return;
 inflight=true;state='checking';render();
 const controller=new AbortController(),timeout=setTimeout(()=>controller.abort(),7000);
 try{
  const response=await fetch('/healthz',{headers:{Accept:'application/json'},credentials:'same-origin',cache:'no-store',redirect:'manual',signal:controller.signal});
  if(response.type==='opaqueredirect'||[401,403].includes(response.status))state='login';
  else{
   if(!response.ok)throw new Error('health');
   const result=await response.json();
   state=result.status==='ok'&&result.service==='hellscript-player-logs'&&result.schemaVersion===1?'healthy':'unavailable';
  }
 }catch{state='unavailable';}
 finally{clearTimeout(timeout);inflight=false;render();}
}
document.getElementById('refresh').addEventListener('click',check);check();
})();
