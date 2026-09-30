#!/usr/bin/env python3
"""Export only this Sites service, push its exact source, and package runtime output.

Supply the short-lived Sites repository credential as one JSON line on stdin.
The credential is held in memory and scoped to the returned repository URL.
"""
import argparse
import json
import os
from pathlib import Path
import shutil
import subprocess
import sys
import tarfile
import tempfile


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--mirror', type=Path, required=True)
    parser.add_argument('--archive', type=Path, required=True)
    args = parser.parse_args()
    source = Path(__file__).resolve().parent
    hosting = json.loads((source / '.openai/hosting.json').read_text())
    if not hosting.get('project_id'):
        parser.error('Provision and record the Sites project_id first.')
    if sys.stdin.isatty():
        import termios
        flags = termios.tcgetattr(sys.stdin)
        flags[3] &= ~termios.ECHO
        termios.tcsetattr(sys.stdin, termios.TCSANOW, flags)
    print('Ready for ephemeral repository credential', flush=True)
    credential = json.loads(sys.stdin.readline())
    if credential['auth_mode'] != 'http_extra_header':
        parser.error('Unsupported repository credential mode.')
    remote, branch = credential['remote_url'], credential['branch']
    env = os.environ.copy()
    env.update(GIT_CONFIG_COUNT='1',
               GIT_CONFIG_KEY_0='http.' + remote + '.extraHeader',
               GIT_CONFIG_VALUE_0='Authorization: Bearer ' + credential['token'],
               GIT_TERMINAL_PROMPT='0')

    def git(*command):
        run = subprocess.run(['git', '-C', str(args.mirror), *command],
                             env=env, text=True, capture_output=True)
        if run.returncode:
            raise RuntimeError(run.stderr.replace(credential['token'], '[redacted]'))
        return run.stdout.strip()

    if not args.mirror.exists():
        args.mirror.mkdir(parents=True)
        git('init', '-b', branch)
        git('remote', 'add', 'origin', remote)
    elif git('remote', 'get-url', 'origin') != remote:
        raise RuntimeError('Source mirror belongs to another Site.')
    if git('status', '--porcelain'):
        raise RuntimeError('Source mirror has uncommitted changes.')
    git('check-ref-format', 'refs/heads/' + branch)
    # This mirror is agent-owned. Preserve only its .git; export tracked source
    # without .env, build outputs, dependencies, or any part of the Unity game.
    for path in args.mirror.iterdir():
        if path.name == '.git':
            continue
        if path.is_dir():
            shutil.rmtree(path)
        else:
            path.unlink()
    paths = ['.openai/hosting.json', '_worker.js', 'public/index.html', 'publish.py', 'test_worker.mjs', 'verify_cloud.py']
    for relative in paths:
        target = args.mirror / relative
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(source / relative, target)
    git('add', '--all')
    if git('diff', '--cached', '--name-only'):
        git('commit', '-m', 'Update HELLSCRIPT Sites log receiver')
    sha = git('rev-parse', 'HEAD')
    git('push', 'origin', 'HEAD:refs/heads/' + branch)
    remote_head = git('ls-remote', 'origin', 'refs/heads/' + branch).split()[0]
    if remote_head != sha:
        raise RuntimeError('Published source branch does not match the local build.')
    args.archive.parent.mkdir(parents=True, exist_ok=True)
    # Package precisely the committed runtime state, never a dirty source tree.
    with tempfile.TemporaryDirectory(prefix='hellscript-sites-build-') as build:
        temporary = Path(build) / 'runtime.tar'
        with tarfile.open(temporary, 'w') as archive:
            for relative, output in (
                ('.openai/hosting.json', '.openai/hosting.json'),
                ('_worker.js', 'dist/index.js'),
                ('public/index.html', 'dist/client/index.html'),
            ):
                archive.add(args.mirror / relative, arcname=output, recursive=False)
        os.replace(temporary, args.archive)
    print(json.dumps({'project_id': hosting['project_id'], 'commit_sha': sha,
                      'archive': str(args.archive.resolve()), 'files': paths}))


if __name__ == '__main__':
    main()
