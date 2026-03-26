import os
import json
import sqlite3
import click

from .core import library as lib
from .core import playlists as pl
from .core import cues as cues_mod
from .utils.repl_skin import ReplSkin


@click.group(invoke_without_command=True)
@click.option('--json', 'as_json', is_flag=True, default=False, help='Output JSON')
@click.pass_context
def main(ctx, as_json):
    """Mixxx CLI harness. Run without args for REPL."""
    ctx.ensure_object(dict)
    ctx.obj['as_json'] = as_json
    if ctx.invoked_subcommand is None:
        skin = ReplSkin("mixxx", version="0.2.0")
        skin.print_banner()
        while True:
            try:
                line = skin.get_input()
            except (EOFError, KeyboardInterrupt):
                skin.print_goodbye()
                break
            if not line.strip():
                continue
            parts = line.strip().split()
            cmd = parts[0]
            if cmd in ('exit', 'quit'):
                skin.print_goodbye(); break
            if cmd == 'library' and len(parts) > 1 and parts[1] == 'list':
                limit = 10
                if len(parts) > 2:
                    try:
                        limit = int(parts[2])
                    except ValueError:
                        pass
                rows = lib.list_tracks(limit=limit)
                if ctx.obj['as_json']:
                    print(json.dumps(rows, default=str))
                else:
                    skin.table(["id","artist","title","duration"], [[r.get('id'), r.get('artist'), r.get('title'), r.get('duration')] for r in rows])
                continue
            if cmd == 'playlists' and len(parts) > 1 and parts[1] == 'list':
                rows = pl.list_playlists()
                if ctx.obj['as_json']:
                    print(json.dumps(rows, default=str))
                else:
                    skin.table(["id","name","count"], [[r.get('id'), r.get('name'), r.get('track_count')] for r in rows])
                continue
            skin.info(f"Unknown command: {line}")
        return


@main.group()
def library():
    """Library commands"""
    pass


@library.command('list')
@click.option('--limit', default=10, help='Limit number of tracks')
@click.pass_context
def list_cmd(ctx, limit):
    """List tracks from the Mixxx library"""
    rows = lib.list_tracks(limit=limit)
    if ctx.obj.get('as_json'):
        print(json.dumps(rows, default=str))
    else:
        skin = ReplSkin("mixxx")
        skin.table(["id","artist","title","duration"], [[r.get('id'), r.get('artist'), r.get('title'), r.get('duration')] for r in rows])


@main.group()
def playlists():
    """Playlist commands"""
    pass


@playlists.command('list')
@click.pass_context
def playlists_list(ctx):
    """List playlists"""
    rows = pl.list_playlists()
    if ctx.obj.get('as_json'):
        print(json.dumps(rows, default=str))
    else:
        skin = ReplSkin("mixxx")
        skin.table(["id","name","track_count"], [[r.get('id'), r.get('name'), r.get('track_count')] for r in rows])


@playlists.command('show')
@click.argument('playlist_id', type=int)
@click.pass_context
def playlists_show(ctx, playlist_id):
    """Show tracks in a playlist"""
    rows = pl.get_playlist_tracks(playlist_id)
    if ctx.obj.get('as_json'):
        print(json.dumps(rows, default=str))
    else:
        skin = ReplSkin("mixxx")
        skin.table(["id","artist","title","duration"], [[r.get('id'), r.get('artist'), r.get('title'), r.get('duration')] for r in rows])


@main.group()
def cues():
    """Cue commands"""
    pass


@cues.command('list')
@click.argument('track_id', type=int)
@click.pass_context
def cues_list(ctx, track_id):
    """List cues for a track"""
    rows = cues_mod.list_cues_for_track(track_id)
    if ctx.obj.get('as_json'):
        print(json.dumps(rows, default=str))
    else:
        skin = ReplSkin("mixxx")
        skin.table(["id","type","position","label"], [[r.get('id'), r.get('type'), r.get('position'), r.get('label')] for r in rows])


@main.command('export')
@click.option('--out', '-o', required=True, help='Output JSON file')
@click.pass_context
def export_library(ctx, out):
    """Export library to JSON"""
    rows = lib.export_library()
    with open(out, 'w', encoding='utf-8') as f:
        json.dump(rows, f, default=str, ensure_ascii=False, indent=2)
    print(f"Wrote {len(rows)} tracks to {out}")


@main.group()
def mix():
    """Mix creation commands"""
    pass


@mix.command('create')
@click.argument('playlist_id', type=int)
@click.option('--out', '-o', required=True, help='Output audio file (mp3)')
@click.option('--crossfade', default=5.0, help='Crossfade duration in seconds')
@click.pass_context
def mix_create(ctx, playlist_id, out, crossfade):
    """Create a mix from a playlist by rendering crossfaded tracks into a single file"""
    rows = pl.get_playlist_tracks(playlist_id)
    if not rows:
        print('No tracks found for playlist', playlist_id)
        return
    files = [r.get('file') for r in rows if r.get('file')]
    bpms = [r.get('bpm') for r in rows]
    from .core import exporter
    res = exporter.create_mix_from_track_files(files, out, crossfade, align_bpm=ctx.obj.get('as_json') and False or True, bpms=bpms)
    if res.get('success'):
        print('Mix created:', out)
    else:
        print('Error creating mix:', res.get('message'))


if __name__ == '__main__':
    main()
