#!/usr/bin/env python3
"""Export curated source into a Site opened with the installed Sites workflow.

Never pushes, handles credentials, or deletes unknown checkout files.
"""
import argparse
import json
from pathlib import Path
import shutil
import subprocess

def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--mirror',type=Path,required=True)
    args=parser.parse_args()
    source=Path(__file__).resolve().parent;destination=args.mirror.resolve()
    manifest=json.loads((source/'.openai/hosting.json').read_text())
    existing=json.loads((destination/'.openai/hosting.json').read_text())
    if not (destination/'.git').exists() or existing.get('project_id')!=manifest.get('project_id'):
        parser.error('Use the opened source checkout for this same Site.')
    subprocess.run(['python3',str(source/'build.py'),'--generate-only'],check=True)
    paths=['.openai/hosting.json','.gitignore','package.json','package-lock.json','drizzle.config.ts',
           '_worker.js','common.js','auth.js','accounts.js','player_events.js','player_web.js',
           'liveops.js','migration.js','schema.generated.js','ops.generated.js','build.py',
           'analytics_daily.sql','analytics_tutorial.sql','analytics_retention.sql',
           'test_worker.mjs','test_backend.mjs','test_accounts.mjs','test_player_web_bridge.mjs',
           'test_analytics.mjs','test_publish.mjs','test_workerd.mjs','test_support.mjs',
           'account_fixture_server.mjs','verify_cloud.py','publish.py','migrate.py','README.md']
    paths += [str(p.relative_to(source)) for folder in ['public','db','drizzle','fixtures'] for p in (source/folder).rglob('*') if p.is_file()]
    for relative in paths:
        target=destination/relative;target.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(source/relative,target)
    print(json.dumps({'project_id':manifest['project_id'],'checkout_path':str(destination),'files':len(paths)}))

if __name__=='__main__':main()
