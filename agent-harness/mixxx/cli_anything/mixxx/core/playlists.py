import os
import sqlite3

from .library import get_db_path


def _find_table(cur, candidates):
    cur.execute("SELECT name FROM sqlite_master WHERE type='table'")
    tables = [r[0].lower() for r in cur.fetchall()]
    for c in candidates:
        if c.lower() in tables:
            return c
    return None


def _get_column(cur, table, candidates):
    cur.execute(f"PRAGMA table_info('{table}')")
    cols = [r[1].lower() for r in cur.fetchall()]
    for c in candidates:
        if c.lower() in cols:
            return c
    return None


def list_playlists():
    db = get_db_path()
    if not os.path.exists(db):
        return []
    conn = sqlite3.connect(db)
    cur = conn.cursor()
    table = _find_table(cur, ['Playlists', 'playlists', 'playlist'])
    if not table:
        return []
    id_col = _get_column(cur, table, ['id', 'playlistid', 'playlist_id']) or 'id'
    name_col = _get_column(cur, table, ['name', 'title']) or 'name'
    # find playlist-tracks table
    pt_table = _find_table(cur, ['PlaylistTracks', 'playlist_tracks', 'playlisttracks', 'playlist_track'])

    cur.execute(f"SELECT {id_col}, {name_col} FROM {table} ORDER BY {id_col} ASC")
    rows = cur.fetchall()
    out = []
    for rid, name in rows:
        track_count = 0
        if pt_table:
            # try common column names
            pt_playlist_col = _get_column(cur, pt_table, ['playlist_id', 'playlistid', 'playlist'])
            if pt_playlist_col:
                try:
                    cur.execute(f"SELECT COUNT(*) FROM {pt_table} WHERE {pt_playlist_col} = ?", (rid,))
                    track_count = cur.fetchone()[0]
                except Exception:
                    track_count = 0
        out.append({'id': rid, 'name': name, 'track_count': track_count})
    conn.close()
    return out


def get_playlist_tracks(playlist_id):
    db = get_db_path()
    if not os.path.exists(db):
        return []
    conn = sqlite3.connect(db)
    conn.row_factory = sqlite3.Row
    cur = conn.cursor()
    pt_table = _find_table(cur, ['PlaylistTracks', 'playlist_tracks', 'playlisttracks', 'playlist_track'])
    track_table = _find_table(cur, ['library', 'track', 'tracks'])
    if not pt_table or not track_table:
        return []
    # detect column names
    pt_playlist_col = _get_column(cur, pt_table, ['playlist_id', 'playlistid', 'playlist'])
    pt_track_col = _get_column(cur, pt_table, ['track_id', 'trackid', 'track'])

    track_id_col = _get_column(cur, track_table, ['id', 'trackid']) or 'id'

    if not pt_playlist_col or not pt_track_col:
        return []
    try:
        cur.execute(f"SELECT {pt_track_col} FROM {pt_table} WHERE {pt_playlist_col} = ? ORDER BY rowid ASC", (playlist_id,))
        tids = [r[0] for r in cur.fetchall()]
    except Exception:
        conn.close()
        return []
    out = []
    for tid in tids:
        try:
            # Try to retrieve common metadata including bpm if present
            col_list = [track_id_col, 'artist', 'title', 'duration', 'filepath', 'FilePath', 'bpm', 'scan', 'analysis']
            # Build select list based on available columns
            cur.execute(f"PRAGMA table_info('{track_table}')")
            available_cols = [r[1] for r in cur.fetchall()]
            select_cols = []
            for c in col_list:
                if c in available_cols and c not in select_cols:
                    select_cols.append(c)
            if not select_cols:
                select_clause = '*'
            else:
                select_clause = ','.join(select_cols)
            cur.execute(f"SELECT {select_clause} FROM {track_table} WHERE {track_id_col} = ?", (tid,))
            row = cur.fetchone()
            if row:
                d = dict(zip([c if c in available_cols else c for c in (select_cols if select_cols != ['*'] else available_cols)], row))
                # extract bpm heuristically
                bpm = None
                for key in ('bpm', 'BPM', 'scan', 'Scan', 'analysis'):
                    if key in d and d.get(key):
                        try:
                            # scan/analysis may be structured; try float conversion
                            bpm = float(d.get('bpm') or d.get('BPM') or d.get('scan') or d.get('Scan'))
                        except Exception:
                            bpm = None
                filepath = d.get('FilePath') or d.get('filepath') or d.get('Filepath') or d.get('filename')
                out.append({'id': d.get(track_id_col), 'artist': d.get('artist'), 'title': d.get('title'), 'duration': d.get('duration'), 'file': filepath, 'bpm': bpm})
        except Exception:
            continue
    conn.close()
    return out
