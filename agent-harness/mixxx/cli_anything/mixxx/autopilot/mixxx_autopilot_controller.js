// Mixxx Autopilot Controller Script
// Map MIDI CCs to Mixxx actions so the autopilot can control playback.

function init() {}
function shutdown() {}

function onMidi(status, data1, data2) {
    var msg = status & 0xF0;
    if (msg == 0xB0) { // CC
        var cc = data1;
        var val = data2;
        if (cc == 20) {
            // Load selected track into Deck 1
            engine.setValue('[Channel1]', 'LoadSelectedTrack', 1);
        } else if (cc == 21) {
            // Load selected track into Deck 2
            engine.setValue('[Channel2]', 'LoadSelectedTrack', 1);
        } else if (cc == 22) {
            // Play toggle Deck 1
            engine.setValue('[Channel1]', 'play', val > 0 ? 1 : 0);
        } else if (cc == 23) {
            // Play toggle Deck 2
            engine.setValue('[Channel2]', 'play', val > 0 ? 1 : 0);
        } else if (cc == 24) {
            // Crossfader: map 0..127 to -1..1
            var f = (val / 127.0) * 2.0 - 1.0;
            engine.setValue('[Master]', 'crossfader', f);
        }
    }
}
