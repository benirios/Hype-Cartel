import os
import sqlite3


def get_db_path():
    return os.environ.get('MIXXX_DB_PATH') or os.path.expanduser('~/.mixxx/mixxxdb.sqlite')


def list_tracks(limit=10):
    """Return a list of track dicts from Mixxx DB. If DB missing, return []"""
    db = get_db_path()
    if not os.path.exists(db):
        return []
    conn = sqlite3.connect(db)
    conn.row_factory = sqlite3.Row
    cur = conn.cursor()
    # Try a few known table names
    for table in ('library', 'track', 'tracks'):
        try:
            cur.execute(f"SELECT id, artist, title, duration FROM {table} LIMIT ?", (limit,))
            rows = cur.fetchall()
            return [dict(r) for r in rows]
        except sqlite3.OperationalError:
            continue
    return []
