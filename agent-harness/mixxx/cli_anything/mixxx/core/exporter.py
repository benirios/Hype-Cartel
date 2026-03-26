import shutil
import subprocess
import os
from typing import List, Tuple, Optional


def _which(cmd):
    return shutil.which(cmd)


def _safe_atempo_chain(factor: float) -> Optional[str]:
    """Return an atempo filter expression. atempo supports 0.5-2.0; chain if needed."""
    if factor <= 0:
        return None
    parts = []
    # Decompose factor into multipliers between 0.5 and 2.0
    remaining = factor
    while remaining > 2.0:
        parts.append('atempo=2.0')
        remaining /= 2.0
    while remaining < 0.5:
        parts.append('atempo=0.5')
        remaining *= 2.0
    parts.append(f'atempo={remaining:.6f}')
    return ','.join(parts)


def build_ffmpeg_mix_command(files: List[str], out: str, crossfade: float = 5.0, bpms: Optional[List[Optional[float]]] = None, align_bpm: bool = False, codec: str = 'libmp3lame') -> Tuple[List[str], str]:
    """Build ffmpeg command and filter_complex for mixing.

    If align_bpm is True and bpms provided, each input will have an atempo applied
    to match the BPM of the first track (target BPM). Returns (cmd_list, filter_complex).
    Does not execute ffmpeg.
    """
    ffmpeg = _which('ffmpeg') or 'ffmpeg'
    if len(files) == 0:
        raise ValueError('no input files')
    # Build base input args
    cmd = [ffmpeg]
    for f in files:
        cmd += ['-i', f]
    labels = [f'[{i}:a]' for i in range(len(files))]

    filter_parts = []
    processed_labels = []

    # Optionally apply atempo per input to align BPM
    if align_bpm and bpms and len(bpms) == len(files) and bpms[0]:
        target = bpms[0]
        for i, b in enumerate(bpms):
            lbl_in = labels[i]
            if b and b > 0:
                factor = target / b
                # atempo supports 0.5-2.0, chain if needed
                atempo_expr = _safe_atempo_chain(factor)
                if atempo_expr:
                    out_lbl = f'[a{i}]'
                    filter_parts.append(f"{lbl_in}{atempo_expr}{out_lbl}")
                    processed_labels.append(out_lbl)
                else:
                    processed_labels.append(lbl_in)
            else:
                processed_labels.append(lbl_in)
    else:
        processed_labels = labels

    # Build acrossfade chaining
    # transform labels to only audio labels (e.g., [a0], [a1], or [0:a])
    cur_label = None
    idx = 0
    while idx < len(processed_labels) - 1:
        left = cur_label if cur_label else processed_labels[idx]
        right = processed_labels[idx + 1]
        out_label = f'[mix{idx+1}]'
        filter_parts.append(f"{left}{right}acrossfade=d={crossfade}:c1=tri:c2=tri{out_label}")
        cur_label = out_label
        idx += 1

    filter_complex = ';'.join(filter_parts)

    # final mapping
    final_label = cur_label if cur_label else processed_labels[0]

    cmd += ['-filter_complex', filter_complex, '-map', final_label, '-c:a', codec, '-y', out]
    return cmd, filter_complex


def create_mix_from_files(files: List[str], out: str, crossfade: float = 5.0, codec: str = 'libmp3lame') -> dict:
    # Backward-compatible wrapper
    try:
        cmd, _ = build_ffmpeg_mix_command(files, out, crossfade, bpms=None, align_bpm=False, codec=codec)
    except ValueError as e:
        return {'success': False, 'message': str(e)}
    try:
        proc = subprocess.run(cmd, check=True, capture_output=True, text=True)
        return {'success': True, 'message': proc.stdout}
    except subprocess.CalledProcessError as e:
        return {'success': False, 'message': e.stderr}


def create_mix_from_track_files(track_files: List[str], out: str, crossfade: float = 5.0, align_bpm: bool = False, bpms: Optional[List[Optional[float]]] = None) -> dict:
    # Try to build command
    try:
        cmd, fc = build_ffmpeg_mix_command(track_files, out, crossfade, bpms=bpms, align_bpm=align_bpm)
    except ValueError as e:
        return {'success': False, 'message': str(e)}
    ffmpeg = _which('ffmpeg')
    if not ffmpeg:
        return {'success': False, 'message': 'ffmpeg not found in PATH'}
    try:
        proc = subprocess.run(cmd, check=True, capture_output=True, text=True)
        return {'success': True, 'message': proc.stdout}
    except subprocess.CalledProcessError as e:
        return {'success': False, 'message': e.stderr}


# Expose helper for testing
__all__ = ['build_ffmpeg_mix_command', 'create_mix_from_track_files']
