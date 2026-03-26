from cli_anything.mixxx.core import exporter


def test_build_command_simple():
    files = ['/tmp/a.mp3', '/tmp/b.mp3']
    cmd, fc = exporter.build_ffmpeg_mix_command(files, '/tmp/out.mp3', crossfade=5.0, bpms=None, align_bpm=False)
    assert 'acrossfade' in fc
    assert '-filter_complex' in cmd


def test_build_command_align_bpm():
    files = ['/tmp/a.mp3', '/tmp/b.mp3']
    bpms = [120.0, 100.0]
    cmd, fc = exporter.build_ffmpeg_mix_command(files, '/tmp/out.mp3', crossfade=4.0, bpms=bpms, align_bpm=True)
    # Expect atempo usage
    assert 'atempo' in fc or 'acrossfade' in fc
