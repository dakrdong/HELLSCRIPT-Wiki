// Project-authored vector weapon icons; preserve the imported v13 reference art separately.
const fs=require('fs'), path=require('path');
const sharp=require(process.env.HELLSCRIPT_SHARP || 'sharp');
const root=path.resolve(__dirname,'../..');
const source=path.join(root,'Docs/Design/RuneV13/weapons-clear');
const output=path.join(root,'Assets/HELLSCRIPT/Resources/Runes/V13/Weapons');
(async()=>{
 fs.mkdirSync(output,{recursive:true});
 const report=[];
 for(const weapon of ['sword','greatsword','axe','bow','crossbow','staff']){
  const file=path.join(output,weapon+'.png');
  await sharp(path.join(source,weapon+'.svg')).ensureAlpha().png().toFile(file);
  const {data,info}=await sharp(file).raw().toBuffer({resolveWithObject:true});
  let clear=0,solid=0;
  for(let p=3;p<data.length;p+=4){if(data[p]===0)clear++;if(data[p]===255)solid++;}
  if(info.width!==512||info.height!==512||info.channels!==4||clear===0||solid===0)throw Error('Invalid RGBA icon '+weapon);
  report.push({weapon,width:info.width,height:info.height,channels:info.channels,transparentPixels:clear,opaquePixels:solid});
 }
 fs.writeFileSync(path.join(source,'validation.json'),JSON.stringify({provenance:'Original project-authored SVG geometry, rendered with Sharp; no AI-generated raster assets.',icons:report},null,2)+'\n');
 console.log('Rendered and validated six 512px transparent weapon icons.');
})().catch(e=>{console.error(e);process.exit(1);});
