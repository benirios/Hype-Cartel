const heroVideo = document.getElementById("heroVideo");

if (heroVideo) {
  const freezeAt = (timeInSeconds) => {
    heroVideo.pause();
    heroVideo.currentTime = timeInSeconds;
  };

  const startPlayback = () => {
    const midpoint = Math.max(heroVideo.duration / 2, 0.1);

    const stopAtMidpoint = () => {
      if (heroVideo.currentTime >= midpoint) {
        freezeAt(midpoint);
        heroVideo.removeEventListener("timeupdate", stopAtMidpoint);
      }
    };

    heroVideo.currentTime = 0;
    heroVideo.addEventListener("timeupdate", stopAtMidpoint);

    heroVideo.play().catch((error) => {
      console.error("Could not autoplay hero video:", error);
      freezeAt(midpoint);
      heroVideo.removeEventListener("timeupdate", stopAtMidpoint);
    });
  };

  if (heroVideo.readyState >= 1) {
    startPlayback();
  } else {
    heroVideo.addEventListener("loadedmetadata", startPlayback, { once: true });
  }
}
