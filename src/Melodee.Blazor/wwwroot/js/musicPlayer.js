let audio = null;
let dotNetHelper = null;

export function initializeAudio(helper) {
    if (!helper) {
        console.error("Helper object is required");
        return false;
    }

    dotNetHelper = helper;

    try {
        if (!audio) {
            audio = new Audio();

            // Set up event listeners
            audio.addEventListener('timeupdate', handleTimeUpdate);
            audio.addEventListener('ended', handleSongEnded);
            audio.addEventListener('error', handleAudioError);
        }
        return true;
    } catch (error) {
        console.error("Failed to initialize audio:", error);
        return false;
    }
}

function handleTimeUpdate() {
    if (dotNetHelper) {
        try {
            // Ensure values are valid numbers before sending to .NET
            const currentTime = isNaN(audio.currentTime) ? 0 : audio.currentTime;
            const duration = isNaN(audio.duration) ? 0 : audio.duration;

            dotNetHelper.invokeMethodAsync('OnTimeUpdate', currentTime, duration);
        } catch (error) {
            console.error("Error in timeupdate handler:", error);
        }
    }
}

function handleSongEnded() {
    if (dotNetHelper) {
        try {
            dotNetHelper.invokeMethodAsync('OnSongEnded');
        } catch (error) {
            console.error("Error in ended handler:", error);
        }
    }
}

function handleAudioError(event) {
    if (dotNetHelper) {
        try {
            dotNetHelper.invokeMethodAsync('OnAudioError', audio.error?.code || -1);
        } catch (error) {
            console.error("Error in audio error handler:", error);
        }
    }
}

// Function to clean up resources when no longer needed
export function cleanupAudio() {
    if (audio) {
        audio.removeEventListener('timeupdate', handleTimeUpdate);
        audio.removeEventListener('ended', handleSongEnded);
        audio.removeEventListener('error', handleAudioError);
        audio.pause();
        audio = null;
    }
    dotNetHelper = null;
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
