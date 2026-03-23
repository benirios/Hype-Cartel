const heroVideo = document.getElementById("heroVideo");

if (heroVideo) {
  const storageKey = "heroIntroSoundPlayed";
  const hasPlayedIntro = localStorage.getItem(storageKey) === "1";

  const startMutedLoop = () => {
    // Ensure we loop silently from the start of the video
    heroVideo.loop = true;
    heroVideo.muted = true;
    heroVideo.defaultMuted = true;
    try {
      heroVideo.currentTime = 0;
    } catch (e) {
      // Some browsers may throw when modifying currentTime before metadata is loaded
    }

    heroVideo.play().catch(() => {
      /* ignore autoplay rejection */
    });
  };

  const markIntroPlayed = () => {
    localStorage.setItem(storageKey, "1");
  };

  const onIntroEnded = () => {
    // Remove the ended handler and switch to a muted looping playback
    heroVideo.removeEventListener('ended', onIntroEnded);
    startMutedLoop();
  };

  const startIntroWithSound = async () => {
    heroVideo.loop = false;
    heroVideo.currentTime = 0;
    heroVideo.muted = false;
    heroVideo.defaultMuted = false;
    heroVideo.volume = 1;

    await heroVideo.play();
    markIntroPlayed();
  };

  const waitForUserGestureToStartSound = () => {
    const onUserGesture = async () => {
      document.removeEventListener("pointerdown", onUserGesture);
      document.removeEventListener("keydown", onUserGesture);

      try {
        await startIntroWithSound();
      } catch {
        startMutedLoop();
      }
    };

    document.addEventListener("pointerdown", onUserGesture, { once: true });
    document.addEventListener("keydown", onUserGesture, { once: true });
  };

  heroVideo.addEventListener("ended", onIntroEnded);

  const isLocalhost = ['localhost', '127.0.0.1', '::1'].includes(location.hostname);

  if (isLocalhost) {
    // For localhost, always attempt the sound intro on each navigation
    startIntroWithSound().catch(() => {
      startMutedLoop();
      waitForUserGestureToStartSound();
    });
  } else {
    if (hasPlayedIntro) {
      startMutedLoop();
    } else {
      startIntroWithSound().catch(() => {
        startMutedLoop();
        waitForUserGestureToStartSound();
      });
    }
  }
}
