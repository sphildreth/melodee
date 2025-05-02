let audio;
let dotNetHelper;

export function initializeAudio(helper) {
    dotNetHelper = helper;

    // Create audio element if it doesn't exist
    if (!audio) {
        audio = new Audio();

        // Set up event listeners
        audio.addEventListener('timeupdate', () => {
            dotNetHelper.invokeMethodAsync('OnTimeUpdate', audio.currentTime, audio.duration);
        });

        audio.addEventListener('ended', () => {
            dotNetHelper.invokeMethodAsync('OnSongEnded');
        });
    }

    return true;
}

export function loadSong(src) {
    if (audio) {
        audio.src = src;
        audio.load();
        return true;
    }
    return false;
}

export function playSong() {
    if (audio) {
        audio.play();
        return true;
    }
    return false;
}

export function pauseSong() {
    if (audio) {
        audio.pause();
        return true;
    }
    return false;
}

export function forwardTenSec() {
    if (audio) {
        audio.currentTime = Math.min(audio.duration, audio.currentTime + 10);
        return true;
    }
    return false;
}

export function backwardTenSec() {
    if (audio) {
        audio.currentTime = Math.max(0, audio.currentTime - 10);
        return true;
    }
    return false;
}

export function setProgress(percentage) {
    if (audio) {
        const newTime = audio.duration * percentage;
        if (!isNaN(newTime)) {
            audio.currentTime = newTime;
        }
        return true;
    }
    return false;
}

export function setVolume(volume) {
    if (audio) {
        audio.volume = volume;
        audio.muted = false;
        return true;
    }
    return false;
}

export function setMute(muted) {
    if (audio) {
        audio.muted = muted;
        return true;
    }
    return false;
}

// Helper function to get element's bounding client rect
window.getBoundingClientRect = function (element) {
    return element.getBoundingClientRect();
};
