"""Autopilot HTTP server for live suggestions.

Endpoints:
- POST /suggest {"type":"play_next"|"load_and_play"|"crossfade_now"|"set_crossfade","params":{...}}
- GET /status
- GET /tracks -> lists files in /Users/beni/Downloads/mosaic

This server maps high-level suggestions to MIDI actions by importing the existing MidiAgent.
"""

from flask import Flask, request, jsonify
from threading import Thread
import time, os

APP = Flask(__name__)
MOSAIC_DIR = os.path.expanduser('/Users/beni/Downloads/mosaic')

# Import local agent module (same directory)
try:
    from . import agent as _agent_mod
except Exception:
    # fallback to relative import when executed as script
    import agent as _agent_mod


def list_tracks():
    ALLOWED = {'.mp3', '.m4a', '.wav', '.ogg', '.flac'}
    try:
        files = [f for f in sorted(os.listdir(MOSAIC_DIR)) if os.path.splitext(f)[1].lower() in ALLOWED]
        return files
    except Exception:
        return []


@APP.route('/tracks', methods=['GET'])
def http_list_tracks():
    return jsonify({'tracks': list_tracks()})


@APP.route('/status', methods=['GET'])
def status():
    return jsonify({'ok': True, 'midi_port': 'mixxx-autopilot', 'tracks': len(list_tracks())})


def _do_crossfade(duration=8.0, port_name=None):
    agent = _agent_mod.MidiAgent(port_name=port_name)
    try:
        steps = int(max(12, duration * 4))
        for i in range(steps + 1):
            v = int((i / steps) * 127)
            agent.send_cc(24, v)
            time.sleep(duration / max(1, steps))
    finally:
        agent.close()


def _load_and_play(slot=1, autoplay=True, port_name=None):
    agent = _agent_mod.MidiAgent(port_name=port_name)
    try:
        if slot == 1:
            agent.send_cc(20, 127)  # load selected into deck1
            time.sleep(0.1)
            if autoplay:
                agent.send_cc(22, 127)
        else:
            agent.send_cc(21, 127)  # load selected into deck2
            time.sleep(0.1)
            if autoplay:
                agent.send_cc(23, 127)
    finally:
        agent.close()


@APP.route('/suggest', methods=['POST'])
def suggest():
    """Accept suggestions and map them to actions.

    Body: {"type": "play_next"|"load_and_play"|"crossfade_now"|"set_crossfade", "params": {...}}
    """
    data = request.get_json(silent=True) or {}
    t = data.get('type')
    params = data.get('params', {})
    port = params.get('port')

    if t == 'play_next':
        # Load next track into deck2 and crossfade
        # User is expected to select the next track in Mixxx library before calling this,
        # or use a separate endpoint to select by filename via controller mapping.
        thr = Thread(target=_load_and_play, args=(2, True, port), daemon=True)
        thr.start()
        # Start crossfade after a short delay so deck2 has time to start
        cf = float(params.get('crossfade', 8.0))
        Thread(target=lambda: (time.sleep(0.7), _do_crossfade(cf, port)), daemon=True).start()
        return jsonify({'ok': True, 'action': 'play_next'})

    if t == 'load_and_play':
        slot = int(params.get('slot', 1))
        Thread(target=_load_and_play, args=(slot, True, port), daemon=True).start()
        return jsonify({'ok': True, 'action': 'load_and_play', 'slot': slot})

    if t == 'crossfade_now':
        cf = float(params.get('crossfade', 8.0))
        Thread(target=_do_crossfade, args=(cf, port), daemon=True).start()
        return jsonify({'ok': True, 'action': 'crossfade_now', 'crossfade': cf})

    if t == 'set_crossfade':
        # Immediate set crossfader value 0..127
        v = int(params.get('value', 64))
        agent = _agent_mod.MidiAgent(port_name=port)
        try:
            agent.send_cc(24, v)
        finally:
            agent.close()
        return jsonify({'ok': True, 'action': 'set_crossfade', 'value': v})

    return jsonify({'ok': False, 'error': 'unknown_type', 'received': data}), 400


def run_server(host='127.0.0.1', port=5000):
    print(f'Starting autopilot server on http://{host}:{port}')
    APP.run(host=host, port=port)


if __name__ == '__main__':
    run_server()
