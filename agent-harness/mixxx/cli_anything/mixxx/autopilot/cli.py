#!/usr/bin/env python3
"""Terminal CLI to control the Mixxx autopilot HTTP server.

Usage examples:
  python cli.py status
  python cli.py tracks
  python cli.py play-next --crossfade 6
  python cli.py crossfade-now --crossfade 8
  python cli.py set-crossfade --value 64
  python cli.py load-and-play --slot 1
"""
import argparse
import json
import sys
try:
    import requests
except Exception:
    requests = None
import urllib.request

SERVER = 'http://127.0.0.1:5000'


def http_post(path, data):
    url = SERVER + path
    payload = json.dumps(data).encode('utf-8')
    headers = {'Content-Type': 'application/json'}
    if requests:
        r = requests.post(url, json=data)
        try:
            return r.status_code, r.json()
        except Exception:
            return r.status_code, r.text
    else:
        req = urllib.request.Request(url, data=payload, headers=headers, method='POST')
        with urllib.request.urlopen(req) as r:
            body = r.read().decode('utf-8')
            try:
                return r.getcode(), json.loads(body)
            except Exception:
                return r.getcode(), body


def http_get(path):
    url = SERVER + path
    if requests:
        r = requests.get(url)
        try:
            return r.status_code, r.json()
        except Exception:
            return r.status_code, r.text
    else:
        with urllib.request.urlopen(url) as r:
            body = r.read().decode('utf-8')
            try:
                return r.getcode(), json.loads(body)
            except Exception:
                return r.getcode(), body


def cmd_status(args):
    code, body = http_get('/status')
    print('HTTP', code)
    print(json.dumps(body, indent=2) if isinstance(body, (dict, list)) else body)


def cmd_tracks(args):
    code, body = http_get('/tracks')
    print('HTTP', code)
    if isinstance(body, dict):
        for i,t in enumerate(body.get('tracks', []),1):
            print(f'{i:2d}. {t}')
    else:
        print(body)


def cmd_play_next(args):
    payload = {'type':'play_next','params':{'crossfade': args.crossfade}}
    code, body = http_post('/suggest', payload)
    print('HTTP', code, body)


def cmd_crossfade_now(args):
    payload = {'type':'crossfade_now','params':{'crossfade': args.crossfade}}
    code, body = http_post('/suggest', payload)
    print('HTTP', code, body)


def cmd_set_crossfade(args):
    payload = {'type':'set_crossfade','params':{'value': int(args.value)}}
    code, body = http_post('/suggest', payload)
    print('HTTP', code, body)


def cmd_load_and_play(args):
    payload = {'type':'load_and_play','params':{'slot': int(args.slot)}}
    code, body = http_post('/suggest', payload)
    print('HTTP', code, body)


def main():
    p = argparse.ArgumentParser()
    sub = p.add_subparsers(dest='cmd')
    sub.add_parser('status')
    sub.add_parser('tracks')
    pn = sub.add_parser('play-next'); pn.add_argument('--crossfade', type=float, default=8.0)
    cf = sub.add_parser('crossfade-now'); cf.add_argument('--crossfade', type=float, default=8.0)
    sc = sub.add_parser('set-crossfade'); sc.add_argument('--value', type=int, required=True)
    lap = sub.add_parser('load-and-play'); lap.add_argument('--slot', type=int, default=1)
    args = p.parse_args()
    if not args.cmd:
        p.print_help(); sys.exit(1)
    if args.cmd == 'status': cmd_status(args)
    elif args.cmd == 'tracks': cmd_tracks(args)
    elif args.cmd == 'play-next': cmd_play_next(args)
    elif args.cmd == 'crossfade-now': cmd_crossfade_now(args)
    elif args.cmd == 'set-crossfade': cmd_set_crossfade(args)
    elif args.cmd == 'load-and-play': cmd_load_and_play(args)

if __name__ == '__main__':
    main()
