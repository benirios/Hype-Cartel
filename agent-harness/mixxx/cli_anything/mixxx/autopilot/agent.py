"""Mixxx Autopilot MIDI agent

Sends MIDI CC messages to a virtual MIDI port to control Mixxx for live sets.

Requirements:
  - python-rtmidi (pip install python-rtmidi)
  - ffprobe/ffmpeg (optional, for duration probing)
  - A virtual MIDI port visible to Mixxx (macOS: create IAC bus; Linux: a2jmidid/jack-midi; Windows: loopMIDI)

Workflow expectations:
  - Map MIDI CCs in Mixxx Preferences -> Controllers to action names (see README.md)
  - Prepare a playlist or directory of audio files and ensure corresponding tracks are in the Mixxx library and selectable

This agent performs a simple beat-agnostic crossfade set:
  - Starts track A on deck 1
  - Waits (durationA - crossfade)
  - Starts track B on deck 2 and ramps crossfader over crossfade seconds

This is a simple demo — for reliable beat-aligned DJing, ensure Mixxx has analyzed BPMs and use tighter integration.
"""

import time
import sys
import os
import argparse

try:
    import rtmidi
except Exception:
    rtmidi = None


def probe_duration(path):
    """Probe duration via ffprobe if available, else return None"""
    try:
        import subprocess, json
        cmd = ["ffprobe", "-v", "error", "-select_streams", "a:0", "-show_entries", "stream=duration", "-of", "default=noprint_wrappers=1:nokey=1", path]
        out = subprocess.check_output(cmd, stderr=subprocess.DEVNULL).decode().strip()
        if out:
            return float(out)
    except Exception:
        pass
    return None


class MidiAgent:
    def __init__(self, port_name=None):
        if rtmidi is None:
            raise RuntimeError('python-rtmidi not installed. Install with: pip install python-rtmidi')
        self.midiout = rtmidi.MidiOut()
        self.port_name = port_name
        self.port = None
        # If port_name provided, try to open matching port; else create virtual port
        self._open_port()

    def _open_port(self):
        ports = self.midiout.get_ports()
        if self.port_name:
            for i, p in enumerate(ports):
                if self.port_name in p:
                    self.midiout.open_port(i)
                    self.port = p
                    print('Opened MIDI out port:', p)
                    return
            # Not found -> create virtual
        try:
            self.midiout.open_virtual_port('mixxx-autopilot')
            self.port = 'virtual:mixxx-autopilot'
            print('Opened virtual MIDI out port: mixxx-autopilot')
        except Exception as e:
            raise RuntimeError('Failed to open MIDI port: ' + str(e))

    def send_cc(self, cc, value, channel=0):
        # channel 0..15 -> status 0xB0 + channel
        status = 0xB0 | (channel & 0x0f)
        msg = [status, cc & 0x7f, int(value) & 0x7f]
        self.midiout.send_message(msg)

    def send_note(self, note, velocity=127, channel=0):
        status = 0x90 | (channel & 0x0f)
        self.midiout.send_message([status, note & 0x7f, velocity & 0x7f])

    def close(self):
        try:
            self.midiout.close_port()
        except Exception:
            pass


def run_sequence(files, crossfade=8.0, port_name=None, autoplay=True):
    if not files:
        print('No input files')
        return
    agent = MidiAgent(port_name=port_name)
    try:
        # Load the first track: user must ensure the selected track corresponds to files[0]
        # The common mapping recommendation (in README) maps CC 20 -> LoadSelectedTrack Deck1
        print('Sending load CC for deck1 (CC 20)')
        agent.send_cc(20, 127)  # load selected into deck1
        time.sleep(0.2)
        if autoplay:
            agent.send_cc(22, 127)  # play deck1 (CC 22)
        # Probe duration
        dur = probe_duration(files[0]) or 180.0
        print('Track duration:', dur)
        wait_time = max(1.0, dur - crossfade)
        print(f'Waiting {wait_time:.1f}s before starting next track')
        time.sleep(wait_time)
        # Prepare track 2
        if len(files) > 1:
            print('Load track 2 into deck2 (CC 21)')
            agent.send_cc(21, 127)
            time.sleep(0.2)
            print('Starting deck2 (CC 23)')
            agent.send_cc(23, 127)
            # Ramp crossfader from left (0) to right (127) over crossfade seconds
            steps = int(max(12, crossfade * 4))
            for i in range(steps + 1):
                v = int((i / steps) * 127)
                agent.send_cc(24, v)
                time.sleep(crossfade / max(1, steps))
            print('Crossfade complete')
        else:
            print('Only one track provided; playback started for deck1')
    finally:
        agent.close()


if __name__ == '__main__':
    p = argparse.ArgumentParser()
    p.add_argument('input', nargs='+', help='Audio files (local paths)')
    p.add_argument('--crossfade', type=float, default=8.0)
    p.add_argument('--port', help='MIDI port name to connect to (optional)')
    p.add_argument('--no-autoplay', dest='autoplay', action='store_false')
    args = p.parse_args()
    run_sequence(args.input, crossfade=args.crossfade, port_name=args.port, autoplay=args.autoplay)
