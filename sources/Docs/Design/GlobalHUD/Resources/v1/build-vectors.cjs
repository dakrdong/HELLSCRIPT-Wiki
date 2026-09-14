// Rebuild only the original vector UI artwork; never modifies generated raster art.
const fs = require('fs');
const path = require('path');
const sharp = require(process.env.HUD_SHARP_MODULE || 'sharp');
const out = __dirname;
const entries = [];
const defs = `<defs><linearGradient id="gold" x2=".25" y2="1"><stop stop-color="#f5deb0"/><stop offset=".32" stop-color="#bda06d"/><stop offset=".64" stop-color="#655035"/><stop offset="1" stop-color="#dec28c"/></linearGradient><linearGradient id="ink" x2="0" y2="1"><stop stop-color="#1d2630" stop-opacity=".7"/><stop offset="1" stop-color="#03070c" stop-opacity=".45"/></linearGradient></defs>`;
const g = body => `<g fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">${body}</g>`;
const glyphs = {
 shield:g('<path d="M12 2 21 6v6c0 5-6 9-9 10-3-1-9-5-9-10V6Z"/><path d="m12 6-4 7h4l-1 5 5-8h-4Z"/>'),
 power:g('<path d="M5 12 12 3l7 9h-4v9H9v-9Z"/>'),
 poison:g('<path d="M12 2C9 7 5 11 5 15a7 7 0 0 0 14 0c0-4-4-8-7-13Z"/><path d="m9 14 6 4m0-4-6 4"/>'),
 lightning:g('<path d="m14 2-9 12h7l-2 8 9-13h-7Z"/>'),
 leaf:g('<path d="M20 3C7 1 2 8 5 16s16 8 15-13Z"/><path d="m4 22 12-14m-7 8v-5m4 1h5"/>'),
 sword:g('<path d="m6 18 13-15 3-1-1 4L8 20m-5-5 7 7m-8 0 4-4"/>'),
 frost:g('<path d="M12 2v20M3.3 7l17.4 10M3.3 17 20.7 7m-12-2 3.3 3 3.3-3m-6.6 14 3.3-3 3.3 3M3 10l4-1-1-4m15 9-4 1 1 4m-15-5 4 1-1 4m15-9-4-1 1-4"/>'),
 hourglass:g('<path d="M5 2h14M5 22h14M7 2v5l10 10v5M17 2v5L7 17v5m0-3h10"/>'),
 flame:g('<path d="M13 2c1 6 7 7 7 13a8 8 0 0 1-16 0c0-4 3-7 5-9 0 4 2 5 3 5 1-3 1-6 1-9Z"/><path d="M12 14c-5 5-2 8 1 7 3-1 2-4-1-7Z"/>'),
 eye:g('<path d="M2 12s4-7 10-7 10 7 10 7-4 7-10 7S2 12 2 12Z"/><circle cx="12" cy="12" r="3"/>'),
 wind:g('<path d="M2 7h13c6 0 5-6 1-4M2 12h17c5 0 5 6 0 6M2 17h9c5 0 4 6 0 4"/>'),
 heart:g('<path d="M12 21 3 12C-3 2 9-2 12 7c3-9 15-5 9 5Z"/><path d="M4 12h4l2-4 4 8 2-4h4"/>'),
 rune:g('<path d="m12 2 9 10-9 10L3 12Z"/><path d="M15 7c-9-2-10 12 1 10m-7-3 5-5"/>'),
 chain:g('<path d="m9 15 6-6m-4-3 2-2c6-6 15 3 9 9l-3 3m-6 3-2 2C3 27-6 18 0 12l3-3" transform="translate(2 0) scale(.83 1)"/>'),
 broken:g('<path d="M10 3 3 6v6c0 5 6 9 9 10 3-1 9-5 9-10V6l-7-3m-1 2-4 6 5 3-3 5"/>'),
 unknown:g('<path d="M8 7a4 4 0 1 1 7 3c-2 1-3 2-3 5"/><circle cx="12" cy="20" r=".7"/>'),
 whirl:g('<path d="M21 10c-4-12-21-5-16 5 4 10 18 4 14-4-3-7-13-3-10 3 2 3 7 0 4-2"/>'),
 arrow:g('<path d="M2 12h19m-7-7 7 7-7 7M6 8v8"/>'),
 trap:g('<path d="m3 8 3 6 3-6 3 6 3-6 3 6 3-6M4 18h16M8 18v4m8-4v4"/>'),
 mana:g('<path d="M12 2c-3 6-8 11-8 15a8 8 0 0 0 16 0c0-4-5-9-8-15Z"/><path d="m9 15 3-4 3 4-3 5Z"/>'),
 target:g('<circle cx="12" cy="12" r="7"/><circle cx="12" cy="12" r="2"/><path d="M12 1v5m0 12v5M1 12h5m12 0h5"/>'),
 elements:g('<path d="m12 2 4 7H8Z"/><circle cx="6" cy="17" r="4"/><path d="m18 12 4 9h-8Z"/>'),
 focus:g('<path d="M3 9V3h6m6 0h6v6m0 6v6h-6m-6 0H3v-6"/><path d="m12 6 4 6-4 6-4-6Z"/>'),
 people:g('<circle cx="12" cy="6" r="3"/><path d="M7 22v-6a5 5 0 0 1 10 0v6M3 9a2 2 0 1 1 2-3M1 20v-5a3 3 0 0 1 4-3M21 9a2 2 0 1 0-2-3m4 14v-5a3 3 0 0 0-4-3"/>')
};
async function add(id, category, body, width=256, height=256, extra={}) {
 const svg=`<svg xmlns="http://www.w3.org/2000/svg" width="${width}" height="${height}" viewBox="0 0 ${width} ${height}">${defs}${body}</svg>`;
 fs.writeFileSync(path.join(out,'svg',id+'.svg'),svg+'\n');
 await sharp(Buffer.from(svg)).png().toFile(path.join(out,'png',id+'.png'));
 entries.push({id,category,file:'png/'+id+'.png',vector:'svg/'+id+'.svg',width,height,spriteMode:'Single',pivot:[.5,.5],...extra});
}
async function main(){
 await add('frame-seal','common','<circle cx="128" cy="128" r="119" fill="none" stroke="#130f0a" stroke-width="12"/><circle cx="128" cy="128" r="119" fill="none" stroke="url(#gold)" stroke-width="5"/><circle cx="128" cy="128" r="112" fill="none" stroke="#c4a872" stroke-opacity=".55" stroke-width="2"/>');
 await add('mask-circle','mask','<circle cx="128" cy="128" r="110" fill="white"/>');
 await add('mask-square','mask','<rect x="8" y="8" width="240" height="240" rx="7" fill="white"/>');
 await add('mask-diamond','mask','<path d="m128 10 118 118-118 118L10 128Z" fill="white"/>');
 await add('frame-active','common','<rect x="6" y="6" width="244" height="244" rx="8" fill="none" stroke="#171209" stroke-width="9"/><rect x="6" y="6" width="244" height="244" rx="8" fill="none" stroke="url(#gold)" stroke-width="4"/><path d="M11 36V11h25m184 0h25v25M11 220v25h25m184 0h25v-25" stroke="#eed6a5" stroke-width="2" fill="none"/>',256);
 await add('frame-passive','common','<path d="m128 6 122 122-122 122L6 128Z" fill="none" stroke="#171209" stroke-width="8"/><path d="m128 6 122 122-122 122L6 128Z" fill="none" stroke="url(#gold)" stroke-width="4"/>');
 await add('frame-status','common','<rect x="8" y="8" width="240" height="240" rx="12" fill="none" stroke="url(#gold)" stroke-width="5"/>');
 await add('plate-icon','common','<rect x="9" y="9" width="238" height="238" rx="11" fill="url(#ink)"/>');
 await add('frame-vital','common','<rect x="2" y="2" width="508" height="44" rx="12" fill="none" stroke="url(#gold)" stroke-width="3"/>',512,48,{border:[14,14,14,14]});
 await add('fill-white','common','<rect width="16" height="16" fill="white"/>',16,16);
 const fadeStops=Array.from({length:17},(_,i)=>{const t=i/16;return `<stop offset="${t}" stop-color="white" stop-opacity="${t*t*(3-2*t)}"/>`;}).join('');
 await add('mask-edge-smooth','mask',`<defs><linearGradient id="fade">${fadeStops}</linearGradient></defs><rect width="256" height="8" fill="url(#fade)"/>`,256,8,{wrapMode:'Clamp',sRGB:false,usage:'Optional alpha lookup; analytic smoothstep is preferred. Never draw as a dark overlay.'});
 await add('fill-vital','common','<defs><linearGradient id="white" x2="0" y2="1"><stop stop-color="#fff" stop-opacity=".9"/><stop offset=".45" stop-color="#fff" stop-opacity=".55"/><stop offset="1" stop-color="#fff" stop-opacity=".8"/></linearGradient></defs><rect x="2" y="2" width="508" height="28" rx="8" fill="url(#white)"/>',512,32,{border:[10,10,10,10]});
 await add('badge-level','common','<path d="M24 4h144l20 28-20 28H24L4 32Z" fill="#080c12" fill-opacity=".6" stroke="url(#gold)" stroke-width="3"/>',192,64);
 for(const [id,symbol] of [['control-plus','<path d="M20 32h24M32 20v24"/>'],['control-minus','<path d="M20 32h24"/>']]) await add(id,'control','<circle cx="32" cy="32" r="28" fill="#090d12" fill-opacity=".4" stroke="#d5c4a8" stroke-opacity=".6" stroke-width="2"/><g stroke="#f8f3e6" stroke-width="3" stroke-linecap="round">'+symbol+'</g>',64,64);
 for(const [id,color,symbol] of [['badge-buff','#286a40','<path d="M16 9v14M9 16h14"/>'],['badge-debuff','#a02b39','<path d="M9 16h14"/>']]) await add(id,'common',`<rect x="2" y="2" width="28" height="28" rx="5" fill="${color}" stroke="#eee1bd" stroke-width="1.5"/><g stroke="#fff7e6" stroke-width="2.5">${symbol}</g>`,32,32);
 await add('xp-tick','common','<path d="M4 2v12" stroke="#dcc393" stroke-width="2"/>',8,16);
 await add('xp-tick-major','common','<path d="M4 1v22" stroke="#f0d5a0" stroke-width="2"/>',8,24);
 await add('xp-cap','common','<path d="M4 2v18m0-9h9" stroke="#d8bd8a" stroke-width="2"/>',16,20);
 await add('portrait-placeholder','common','<circle cx="128" cy="128" r="110" fill="#10151c" fill-opacity=".72"/><circle cx="128" cy="91" r="35" fill="#8d8169"/><path d="M64 207c-4-93 132-93 128 0Z" fill="#8d8169"/>');
 const statusColors={shield:'#72d6e7',power:'#efc66c',poison:'#c785e9',lightning:'#76bff7',leaf:'#8fce89',sword:'#efb681',frost:'#b7e3f5',hourglass:'#dcb883',flame:'#ec9c65',eye:'#d19feb',wind:'#a5d7bf',heart:'#e98990',rune:'#95bded',chain:'#c6b8d9',broken:'#e79b92',unknown:'#c7bba5',whirl:'#ceb997',arrow:'#c1d298',trap:'#abc393',mana:'#78bce9',target:'#d5c690',elements:'#bcc0ee',focus:'#c0d8bd',people:'#d2ba90'};
 for(const [name,body] of Object.entries(glyphs)) await add('status-'+name,'status',`<g color="${statusColors[name]}" transform="translate(32 32) scale(8)">${body}</g>`);
 const passives=[['WP01','people'],['WP02','whirl'],['WP03','shield'],['WP04','heart'],['WP05','shield'],['WP06','eye'],['AP01','arrow'],['AP02','wind'],['AP03','target'],['AP04','trap'],['AP05','focus'],['AP06','sword'],['MP01','flame'],['MP02','frost'],['MP03','chain'],['MP04','shield'],['MP05','mana'],['MP06','elements']];
 for(const [id,name] of passives){const color=id[0]=='W'?'#e0bb86':id[0]=='A'?'#aed5a5':'#8dc8ef';await add('passive-'+id,'passive',`<path d="m128 21 107 107-107 107L21 128Z" fill="#0c141f" fill-opacity=".75"/><g color="${color}" transform="translate(56 56) scale(6)">${glyphs[name]}</g>${id==='WP05'?'<circle cx="128" cy="128" r="84" fill="none" stroke="#c4a872" stroke-width="3"/>':''}`,256,256,{definitionId:id,semanticGlyph:name});}
 fs.writeFileSync(path.join(out,'vector-manifest.json'),JSON.stringify(entries,null,2)+'\n');
 console.log(JSON.stringify({vectorPngs:entries.length}));
}
main().catch(e=>{console.error(e);process.exit(1)});
