// Rasterize the supplied vector icon definitions. No generated or substituted art.
const fs=require('fs'), path=require('path');
const sharp=require(process.env.HELLSCRIPT_SHARP || 'sharp');
const root=path.resolve(__dirname,'../..'), out=path.join(root,'Assets/HELLSCRIPT/Resources/Runes/V13');
const data=JSON.parse(fs.readFileSync(path.join(out,'glyphs.json')));
const definitions=fs.readFileSync(path.join(root,'Docs/Design/RuneV13/glyphs.svg'),'utf8').match(/<defs>([\s\S]*)<\/defs>/)[1];
(async()=>{
 for(const weapon of ['sword','greatsword','axe','bow','crossbow','staff']){
  const mask=Buffer.from('<svg xmlns="http://www.w3.org/2000/svg" width="128" height="128"><defs><radialGradient id="fade"><stop offset="30%" stop-color="white"/><stop offset="100%" stop-color="white" stop-opacity="0"/></radialGradient></defs><ellipse cx="64" cy="64" rx="63" ry="63" fill="url(#fade)"/></svg>');
  await sharp(path.join(root,'Docs/Design/RuneV13/weapons',weapon+'.webp')).modulate({saturation:.5}).ensureAlpha().composite([{input:mask,blend:'dest-in'}]).png().toFile(path.join(out,weapon+'.png'));
 }
 const {tile,columns,ids}=data, rows=Math.ceil((ids.length+1)/columns), layers=[];
 for(let i=0;i<ids.length;i++){
  const svg=`<svg xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink" width="${tile}" height="${tile}" viewBox="0 0 32 32"><defs>${definitions}</defs><g fill="none" stroke="white" color="white" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round"><use xlink:href="#${ids[i]}"/></g></svg>`;
  layers.push({input:await sharp(Buffer.from(svg)).png().toBuffer(),left:(i%columns)*tile,top:Math.floor(i/columns)*tile});
 }
 layers.push({input:await sharp({create:{width:tile,height:tile,channels:4,background:'#ffffff'}}).png().toBuffer(),left:(ids.length%columns)*tile,top:Math.floor(ids.length/columns)*tile});
 await sharp({create:{width:tile*columns,height:tile*rows,channels:4,background:'#00000000'}}).composite(layers).png().toFile(path.join(out,'glyph-atlas.png'));
 console.log(`${ids.length} source SVG glyphs rendered to RGBA atlas.`);
})().catch(e=>{console.error(e);process.exit(1);});
