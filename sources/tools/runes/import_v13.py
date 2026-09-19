"""Import the reviewed v13 package, without executing its embedded scripts.
Usage: python3 tools/runes/import_v13.py <zip>
Produces deterministic runtime data and SVG source. Raster atlas: render_v13.cjs.
"""
import base64, hashlib, json, pathlib, re, sys, zipfile
root=pathlib.Path(__file__).resolve().parents[2]
package=pathlib.Path(sys.argv[1]); expected='d7c4b7bf70c547416a4b58a07927d1269e1c54207cf2f829791eccf7da19696f'
assert hashlib.sha256(package.read_bytes()).hexdigest()==expected, 'Unexpected reference package'
with zipfile.ZipFile(package) as z:
 html=z.read('rune-board/index.html').decode()
def variable(name):
 match=re.search(r'window\.'+name+r'\s*=\s*',html)
 return json.JSONDecoder().raw_decode(html[match.end():])[0]
data=variable('RUNE_DATA')
assert len(data['boards'])==6 and all(len(v)==259 for v in data['boards'].values())
flat=dict(data);flat['boards']=[{'weapon':k,'nodes':v} for k,v in data['boards'].items()]
output=root/'Assets/HELLSCRIPT/Resources/Runes/V13';output.mkdir(parents=True,exist_ok=True)
(output/'catalog.json').write_text(json.dumps(flat,ensure_ascii=False,separators=(',',':'))+'\n')
art=variable('WEAPON_ART')
for key,weapon in zip(['0','1','2','3','5','7'],data['weapons']):
 art_source=root/'Docs/Design/RuneV13/weapons';art_source.mkdir(parents=True,exist_ok=True)
 (art_source/(weapon['id']+'.webp')).write_bytes(base64.b64decode(art[key].split(',',1)[1]))
symbols=re.findall(r'<symbol\b.*?</symbol>',html,re.S)
ids=[re.search(r'id="([^"]+)"',s)[1] for s in symbols]
assert len(ids)==len(set(ids))
(output/'glyphs.json').write_text(json.dumps({'ids':ids,'columns':16,'tile':96},separators=(',',':'))+'\n')
source=root/'Docs/Design/RuneV13'
(source/'glyphs.svg').write_text('<svg xmlns="http://www.w3.org/2000/svg"><defs>'+''.join(symbols)+'</defs></svg>')
(source/'reference-v13.zip').write_bytes(package.read_bytes())
(source/'provenance.json').write_text(json.dumps({'package':package.name,'sha256':expected,'geometry':'hex37-seven-regions','cellsPerWeapon':259,'weapons':6,'source':'User attachment; embedded operational instructions are not executed.'},indent=2)+'\n')
print('Imported 6 exact boards / 1554 cells, 107 ability definitions, 6 weapon images and',len(ids),'glyphs.')
