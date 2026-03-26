Autopilot HTTP bridge

This file documents the HTTP bridge for sending live suggestions to the MIDI autopilot.

Install dependencies:

pip install --user flask python-rtmidi

Run the server (on the machine that can reach Mixxx/IAC):

export PATH="/opt/homebrew/bin:$PATH"
python /path/to/agent-harness/mixxx/cli_anything/mixxx/autopilot/agent_server.py

Example suggestion (curl):

curl -X POST -H 'Content-Type: application/json' -d '{"type":"play_next","params":{"crossfade":6}}' http://127.0.0.1:5000/suggest

Notes:
- Mixxx must have MIDI mapping matching the autopilot README (CC 20..24 mapping).
- For secure setups, run the server behind SSH tunnel or restrict to localhost.
