Mixxx Autopilot (MIDI-based)

Overview

This module provides a simple MIDI-based autopilot that can drive Mixxx to perform live-style DJ sets.

What it does

- Sends MIDI CC messages to a virtual MIDI port to control Mixxx decks and crossfader.
- Provides a demo agent that starts track A, waits, starts track B and performs a crossfade.

Requirements

- Mixxx installed with a virtual MIDI input visible (macOS: IAC Driver; Windows: loopMIDI; Linux: a2jmidid / virtual ports).
- Map MIDI CCs in Mixxx Controller Mapping UI to the following actions (example mapping):
  - CC 20 -> "LoadSelectedTrack" (Deck 1)
  - CC 21 -> "LoadSelectedTrack" (Deck 2)
  - CC 22 -> "play" toggle (Deck 1)
  - CC 23 -> "play" toggle (Deck 2)
  - CC 24 -> "crossfader" (Master fader) — 0..127 mapped to -1..1 or 0..1 depending on your mapping

Installation

1. Create a virtual MIDI port and ensure Mixxx sees it.
2. Install python-rtmidi: pip install python-rtmidi
3. (Optional) Ensure ffmpeg/ffprobe are installed for accurate duration probing.

Usage

- Manual (prepare Mixxx):
  1. In Mixxx, select the first track in the library and map CC20 to LoadSelectedTrack (Deck1).
  2. Start the autopilot agent with the local files as arguments: python autopilot/agent.py /path/to/track1.mp3 /path/to/track2.mp3

- The agent will:
  - Send CC20 (load selected track into deck1)
  - Send CC22 (play Deck1)
  - Wait for (duration - crossfade)
  - Send CC21 (load next into deck2), CC23 (play Deck2)
  - Ramp CC24 over crossfade seconds to perform crossfade

Limitations and Next Steps

- This demo is beat-agnostic; for professional results, integrate BPM analysis and use Mixxx's beatgrid/cuepoints.
- Advanced version: agent queries Mixxx database or uses OSC/HTTP bridging to select and load tracks automatically.
- For full autonomy, implement a Mixxx controller script that accepts SysEx filenames and calls LoadTrack directly.

Security and safety

- The agent sends MIDI messages only. It does not execute audio engines or modify Mixxx internals.
