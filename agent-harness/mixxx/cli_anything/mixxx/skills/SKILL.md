---
name: "cli-anything-mixxx"
description: "Command-line harness for Mixxx: library, playlists, cues, and autonomous mix rendering via ffmpeg."
---

Usage:

- Install (development):
  python3 -m pip install -e .

- CLI entrypoint: cli-anything-mixxx

Commands:

- library list [--limit N]
  List tracks from the Mixxx database (~/.mixxx/mixxxdb.sqlite). Use MIXXX_DB_PATH to override.

- playlists list
  List playlists and counts

- playlists show <playlist_id>
  Show tracks in a playlist

- cues list <track_id>
  List cue points for a track

- mix create <playlist_id> -o output.mp3 [--crossfade N]
  Render a mix from the given playlist. Uses ffmpeg to acrossfade tracks. If available, attempts BPM alignment using stored BPM values.

Agent guidance:
- Use --json to get machine-readable output (JSON arrays of objects).
- When creating mixes, ensure ffmpeg is in PATH. For accurate beat-aligned mixes, ensure Mixxx has BPM data (analysis) stored in the DB.
- The CLI does not control the Mixxx GUI or audio engine; it reads the Mixxx DB and uses ffmpeg to render mixes from audio files.

Examples:

- List 20 tracks as JSON:
  cli-anything-mixxx --json library list --limit 20

- Create a mix from playlist 3 with 6s crossfades:
  cli-anything-mixxx mix create 3 -o mymix.mp3 --crossfade 6
