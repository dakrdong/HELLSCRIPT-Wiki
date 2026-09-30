#!/usr/bin/env python3
"""Verify a Sites receiver with synthetic data and a separately issued QA file.

An optional owner-only Sites dispatch token is read from stdin, never saved.
Evidence contains only synthetic run IDs, hashes, fixed codes and HTTP statuses.
"""
import argparse
import copy
import hashlib
import json
from pathlib import Path
import re
import sys
import termios
import urllib.error
import urllib.request
from urllib.parse import urlsplit
import uuid

sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
from test_combat_telemetry import fixture


class NoRedirect(urllib.request.HTTPRedirectHandler):
    def redirect_request(self, *_):
        return None


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--site-url', required=True)
    parser.add_argument('--qa-session', type=Path, required=True)
    parser.add_argument('--evidence', type=Path, required=True)
    parser.add_argument('--private', action='store_true')
    args = parser.parse_args()
    url = urlsplit(args.site_url)
    if url.scheme != 'https' or url.username or url.password or url.path or url.query or url.fragment:
        parser.error('Use the exact HTTPS Site origin returned by Sites.')
    session = json.loads(args.qa_session.read_text())
    if (session.get('kind') != 'hellscript-qa-telemetry' or session.get('version') != 1
            or not re.fullmatch(r'[A-Za-z0-9_-]{32,256}', session.get('accessToken', ''))):
        parser.error('A dedicated QA session file is required.')
    auth_origin = 'https://hellscript-production.up.railway.app'
    if session.get('baseUrl', '').rstrip('/') not in (auth_origin, args.site_url):
        parser.error('QA credential belongs to another issuer/collector.')
    bypass = ''
    if args.private:
        if sys.stdin.isatty():
            flags = termios.tcgetattr(sys.stdin)
            flags[3] &= ~termios.ECHO
            termios.tcsetattr(sys.stdin, termios.TCSANOW, flags)
        print('Ready for ephemeral private dispatch token', flush=True)
        bypass = json.loads(sys.stdin.readline())['sites_bypass']
    opener = urllib.request.build_opener(NoRedirect())
    checks = []

    def request(origin, path, method='GET', raw=None, token='', expected=200):
        headers = {'Content-Type': 'application/json'}
        if token:
            headers['Authorization'] = 'Bearer ' + token
        if bypass and origin == args.site_url:
            headers['OAI-Sites-Authorization'] = 'Bearer ' + bypass
        req = urllib.request.Request(origin + path, data=raw, method=method, headers=headers)
        try:
            response = opener.open(req, timeout=20)
        except urllib.error.HTTPError as error:
            response = error
        with response:
            status, body = response.code, response.read(8192)
        try:
            body = json.loads(body)
        except ValueError:
            body = None
        checks.append({'path': path, 'method': method, 'status': status, 'expected': expected})
        if status != expected:
            code = body.get('error', '') if isinstance(body, dict) else 'non_json'
            if not re.fullmatch(r'[a-z_]{1,80}', str(code)):
                code = 'unknown'
            raise AssertionError('Unexpected status at ' + path + ': ' + str(status) + ' ' + code)
        return body

    identity = request(auth_origin, '/v1/telemetry/session', token=session['accessToken'])
    assert identity['kind'] == 'qa'
    request(args.site_url, '/healthz')
    value = fixture()
    value['id'] = uuid.uuid4().hex
    raw = (json.dumps(value, ensure_ascii=False) + '\n').encode()
    expected = {'accepted': True, 'runId': value['id'], 'payloadHash': hashlib.sha256(raw).hexdigest()}
    first = request(args.site_url, '/v1/combat-runs', 'POST', raw, session['accessToken'])
    assert first == expected
    assert request(args.site_url, '/v1/combat-runs', 'POST', raw, session['accessToken']) == first
    changed = copy.deepcopy(value)
    changed['journal']['earnedGold'] += 100
    request(args.site_url, '/v1/combat-runs', 'POST', json.dumps(changed).encode(), session['accessToken'], 409)
    invalid = copy.deepcopy(value)
    invalid['journal']['trust'] = 'server_verified'
    request(args.site_url, '/v1/combat-runs', 'POST', json.dumps(invalid).encode(), session['accessToken'], 422)
    request(args.site_url, '/v1/combat-runs', 'POST', b'not json', expected=401)
    request(args.site_url, '/v1/combat-runs', 'POST', raw, 'x' * 43, 401)
    request(args.site_url, '/v1/combat-runs', expected=405)
    request(args.site_url, '/v1/combat-runs/' + value['id'], expected=404)
    evidence = {'site_url': args.site_url, 'private_dispatch': bool(bypass), 'synthetic': True,
                'receipt': first, 'authenticated_kind': identity['kind'], 'checks': checks}
    args.evidence.parent.mkdir(parents=True, exist_ok=True)
    args.evidence.write_text(json.dumps(evidence, indent=2) + '\n')
    print(json.dumps({'passed': len(checks), 'run_id': value['id'], 'payload_hash': expected['payloadHash'],
                      'private_dispatch': bool(bypass), 'evidence': str(args.evidence)}))


if __name__ == '__main__':
    main()
