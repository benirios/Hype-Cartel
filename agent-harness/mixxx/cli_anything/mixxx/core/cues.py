import os
import sqlite3

from .library import get_db_path


def list_cues_for_track(track_id):
    db = get_db_path()
    if not os.path.exists(db):
        return []
    conn = sqlite3.connect(db)
    conn.row_factory = sqlite3.Row
    cur = conn.cursor()
    # find cues table
    cur.execute("SELECT name FROM sqlite_master WHERE type='table'")
    tables = [r[0].lower() for r in cur.fetchall()]
    candidates = ["cues", "cue", "track_cues", "cuepoints"]
    table = None
    for c in candidates:
        if c in tables:
            table = c
            break
    if not table:
        return []
    # common columns: id, track_id, type, position, label
    cur.execute(f"PRAGMA table_info('{table}')")
    cols = [r[1].lower() for r in cur.fetchall()]
    id_col = 'id' if 'id' in cols else cols[0]
    track_col = 'track_id' if 'track_id' in cols else (cols[1] if len(cols) > 1 else None)
    pos_col = 'position' if 'position' in cols else ('pos' if 'pos' in cols else None)
    type_col = 'type' if 'type' in cols else None
    label_col = 'label' if 'label' in cols else None
    if not track_col:
        return []
    try:
        cur.execute(f"SELECT {id_col}, {type_col if type_col else 'NULL'} as type, {pos_col if pos_col else 'NULL'} as position, {label_col if label_col else 'NULL'} as label FROM {table} WHERE {track_col} = ?", (track_id,))
        rows = cur.fetchall()
        out = []
        for r in rows:
            out.append({'id': r[0], 'type': r[1], 'position': r[2], 'label': r[3]})
        conn.close()
        return out
    except Exception:
        conn.close()
        return []
