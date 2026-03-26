#!/usr/bin/env python3
"""Simple terminal helper to send natural-language suggestions to autopilot server."""
import sys, json
import urllib.request

SERVER = 'http://127.0.0.1:5000'

def suggest(text):
    url = SERVER + '/suggest_nl'
    payload = json.dumps({'text': text}).encode('utf-8')
    req = urllib.request.Request(url, data=payload, headers={'Content-Type':'application/json'}, method='POST')
    with urllib.request.urlopen(req) as r:
        print('HTTP', r.getcode())
        print(r.read().decode('utf-8'))

if __name__ == '__main__':
    if len(sys.argv) < 2:
        print('Usage: cli_suggest.py "more energy"')
        sys.exit(1)
    text = ' '.join(sys.argv[1:])
    suggest(text)
