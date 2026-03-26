import re

"""Simple rule-based decision module to map natural-language suggestions
into autopilot actions.
"""

def _extract_number(text):
    m = re.search(r"(\d+(?:\.\d+)?)", text)
    if m:
        try:
            return float(m.group(1))
        except Exception:
            return None
    return None


def decide(text: str):
    t = (text or '').lower()
    if not t.strip():
        return None

    # Energy heuristics
    if 'more energy' in t or 'higher energy' in t or 'raise energy' in t or 'faster' in t:
        return {'type': 'play_next', 'params': {'crossfade': 6.0}}
    if 'less energy' in t or 'calm' in t or 'soft' in t or 'chill' in t:
        return {'type': 'play_next', 'params': {'crossfade': 12.0}}

    # Crossfade commands
    if 'crossfade now' in t or (t.startswith('crossfade') and 'now' in t):
        secs = _extract_number(t) or 8.0
        return {'type': 'crossfade_now', 'params': {'crossfade': float(secs)}}
    if 'set crossfade' in t or 'crossfade to' in t:
        secs = _extract_number(t) or 8.0
        return {'type': 'set_crossfade', 'params': {'value': int(max(0, min(127, (secs/12.0)*127)))}}

    # Load specific deck
    m = re.search(r'deck\s*(\d+)', t)
    if m:
        slot = int(m.group(1))
        return {'type': 'load_and_play', 'params': {'slot': slot}}

    # Keywords for explicit actions
    if 'next' in t or 'play next' in t or 'skip' in t:
        secs = _extract_number(t) or 8.0
        return {'type': 'play_next', 'params': {'crossfade': float(secs)}}

    # Default fallback
    return {'type': 'play_next', 'params': {'crossfade': 8.0}}
