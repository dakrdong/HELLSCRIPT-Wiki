#!/usr/bin/env python3
"""Import private SQLite online backups once; publish only aggregate evidence."""
import argparse
import hashlib
import json
from pathlib import Path
import sqlite3
import urllib.error
import urllib.request
from urllib.parse import urlsplit
import uuid

def raw(value):return json.dumps(value,ensure_ascii=False,sort_keys=True,separators=(',',':'),allow_nan=False).encode()
def digest(data):return hashlib.sha256(data).hexdigest()
def read(db,table):return [dict(r) for r in db.execute('SELECT * FROM '+table+' ORDER BY rowid')]
def export(directory,existing_runs):
    dbs={name:sqlite3.connect(directory/(name+'.sqlite')) for name in ('accounts','liveops','telemetry')}
    for db in dbs.values():
        db.row_factory=sqlite3.Row
        if db.execute('PRAGMA quick_check').fetchone()[0]!='ok':raise ValueError('Backup integrity failed')
    data={}
    for table in ('auth_accounts','auth_sessions'):data[table]=read(dbs['accounts'],table)
    for source,target in [('releases','liveops_releases'),('active','liveops_active'),('draft','liveops_draft'),('ops_audit','liveops_audit'),('ops_sessions','ops_sessions')]:
        data[target]=read(dbs['liveops'],source)
    data['legacy_telemetry_rows']=[]
    for table in ('runs','events','drops','authority_evidence','audit','run_configuration'):
        for index,row in enumerate(read(dbs['telemetry'],table)):
            data['legacy_telemetry_rows'].append({'table_name':table,'row_key':digest(raw([table,index])),'row_json':raw(row).decode()})
    data['runs']=[]
    for r in read(dbs['telemetry'],'runs'):
        value=json.loads(r['payload']);j=value['journal'];payload=r['payload'];encoded=payload.encode()
        if digest(encoded)!=r['payload_hash']:raise ValueError('Original combat payload hash mismatch')
        # The legacy receiver accepted QA credentials only, without this prefix.
        account='qa:'+r['account_id'];key=account+'/'+r['run_id']+'/'+r['payload_hash']+'.json'
        tuning=j.get('liveOps') or {}
        data['runs'].append({'account_id':account,'run_id':r['run_id'],'payload_hash':r['payload_hash'],'identity_kind':'qa',
          'received_utc_ms':r['received_utc_ms'],'local_account_id':j['localAccountId'],'hero_id':r['hero_id'],'stage':r['stage'],
          'outcome':r['outcome'],'finish':r['finish'],'started_utc_ms':r['started_utc_ms'],'completed_utc_ms':r['completed_utc_ms'],
          'simulation_seconds':r['simulation_seconds'],'attendance_seconds':r['attendance_seconds'],'earned_gold':r['earned_gold'],
          'kills':r['kills'],'boss_defeated':r['boss_defeated'],'equipment_collected':r['equipment_collected'],
          'liveops_version':tuning.get('version'),'liveops_config_hash':tuning.get('configHash'),
          'signals_json':r['signals_json'],'object_key':key,'payload_bytes':len(encoded),'payload':payload})
    for db in dbs.values():db.close()
    counts={table:len(rows) for table,rows in data.items()};counts['runs']+=existing_runs
    return data,counts

class NoRedirect(urllib.request.HTTPRedirectHandler):
    def redirect_request(self,*_):return None

def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--backups',type=Path,required=True)
    parser.add_argument('--site-url',required=True)
    parser.add_argument('--token-file',type=Path,required=True)
    parser.add_argument('--existing-runs',type=int,required=True)
    parser.add_argument('--evidence',type=Path,required=True)
    parser.add_argument('--export-only',action='store_true')
    args=parser.parse_args();url=urlsplit(args.site_url)
    if url.scheme!='https' or url.path or url.query or url.fragment or url.username or url.password:parser.error('Use the exact HTTPS Site origin.')
    data,counts=export(args.backups,args.existing_runs)
    import_id_file=args.backups/'import-id.txt'
    if not import_id_file.exists():import_id_file.write_text(uuid.uuid4().hex);import_id_file.chmod(0o600)
    import_id=import_id_file.read_text().strip()
    token=args.token_file.read_text().strip();opener=urllib.request.build_opener(NoRedirect())
    def post(path,body,binary=False):
        headers={'Authorization':'Bearer '+token,'X-Migration-ID':import_id,'User-Agent':'HELLSCRIPT-Backend-Migration/1.0',
                 'Content-Type':'application/octet-stream' if binary else 'application/json'}
        if binary:headers['X-Content-SHA256']=digest(body)
        try:
            with opener.open(urllib.request.Request(args.site_url+'/internal/migration/'+path,data=body,method='POST',headers=headers),timeout=40) as response:
                return json.loads(response.read(65536))
        except urllib.error.HTTPError as error:
            value=json.loads(error.read(8192));raise RuntimeError('Migration rejected: '+str(error.code)+' '+str(value.get('error','unknown'))) from None
    manifest={'parts':[],'backups':[],'counts':counts}
    for name in ('accounts.sqlite','liveops.sqlite','telemetry.sqlite'):
        body=(args.backups/name).read_bytes();record={'name':name,'hash':digest(body),'bytes':len(body)}
        if not args.export_only:
            result=post('backup/'+name,body,True)
            if result.get('hash')!=record['hash'] or result.get('bytes')!=record['bytes']:raise ValueError('Stored backup mismatch')
        manifest['backups'].append(record)
    for table,rows in data.items():
        for n,start in enumerate(range(0,max(1,len(rows)),40)):
            part=table+'-'+str(n).zfill(4);body=raw({'part':part,'table':table,'rows':rows[start:start+40]})
            record={'part':part,'hash':digest(body),'rows':len(rows[start:start+40])}
            if not args.export_only:
                result=post('part',body)
                if result.get('hash')!=record['hash'] or result.get('rows')!=record['rows']:raise ValueError('Imported rows mismatch')
            manifest['parts'].append(record)
    manifest_body=raw(manifest)
    if not args.export_only:
        result=post('complete',manifest_body)
        if result.get('complete') is not True or result.get('manifestHash')!=digest(manifest_body):raise ValueError('Completion mismatch')
    evidence={'site_url':args.site_url,'import_id':import_id,'complete':not args.export_only,'manifest_sha256':digest(manifest_body),
              'counts':counts,'backups':manifest['backups'],'parts':manifest['parts']}
    args.evidence.parent.mkdir(parents=True,exist_ok=True);args.evidence.write_text(json.dumps(evidence,indent=2)+'\n')
    print(json.dumps({'complete':not args.export_only,'counts':counts,'manifest_sha256':digest(manifest_body)}))

if __name__=='__main__':main()
