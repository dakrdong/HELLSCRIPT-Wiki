(()=>{
const messages={
 ko:{title:'HELLSCRIPT 플레이 로그',intro:'전투 로그 수집 서버의 실제 응답을 확인합니다.',service:'수집 서버 상태',access:'사이트 접근 범위',accessValue:'공개 API',accessDetail:'사이트 로그인 없이 수집 주소에 접속할 수 있습니다. 로그 업로드에는 게임 계정 인증이 필요합니다.',game:'일반 게임 연결',gameValue:'게임 계정 인증 필요',gameDetail:'게임의 수집 주소가 연결되어 있습니다. Google 로그인 후 전투가 끝나면 기록을 전송합니다.',refresh:'다시 확인',response:'서버 상태 응답 보기',privacy:'업로드에는 게임 계정 인증이 필요합니다. 플레이 기록을 공개 조회하는 기능은 제공하지 않습니다.',ready:'수집 서버는 전투 원문을 보관하고 중복 전송을 처리합니다. 업로드가 완료되어도 게임의 최근 전투 기록은 유지됩니다.',checking:'확인 중…',checkingDetail:'저장소와 서버 응답을 확인하고 있습니다.',healthy:'정상',healthyDetail:'수집 서비스와 로그 저장소가 정상 응답했습니다.',unavailable:'응답 확인 실패',unavailableDetail:'서버 상태를 확인하지 못했습니다. 잠시 후 다시 확인해 주세요.',login:'사이트 로그인 필요',loginDetail:'사이트 접근 세션을 확인해 주세요. 페이지를 새로고침하거나 다시 로그인한 뒤 확인할 수 있습니다.'},
 en:{title:'HELLSCRIPT player logs',intro:'Check the live response from the combat log collector.',service:'Collector status',access:'Site access',accessValue:'Public API',accessDetail:'The collector is accessible without site sign-in. Uploads require game account authentication.',game:'Ordinary game connection',gameValue:'Game authentication required',gameDetail:'The game is configured to use this collector. Completed combat records are sent after Google sign-in.',refresh:'Check again',response:'View health response',privacy:'Uploads require game account authentication. Player records cannot be read through public routes.',ready:'The collector stores original payloads and handles duplicate uploads. Recent combat records remain in the game after delivery.',checking:'Checking…',checkingDetail:'Checking the storage and service response.',healthy:'Healthy',healthyDetail:'The collector and log storage responded successfully.',unavailable:'Response unavailable',unavailableDetail:'Could not confirm server health. Please check again shortly.',login:'Site sign-in required',loginDetail:'Check your site access session. Refresh or sign in again before checking health.'}
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
