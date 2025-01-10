![Melodee](graphics/melodee_gh_card.png)

# NOTE
Melodee is under development, very much a work in progress.

# Melodee
Melodee is a music system designed to handle libraries with tens of millions of songs with ease and speed.

## Components
* Melodee.Blazor - Blazor and OpenSubsonic API server
  * Both OpenSubsonic and Subsonic 1.16.1.
  * Blazor adminisrative front end.
* Melodee.Cli - Command line interface
  * Library management.
  * Manage configuration values.

## Features
* Process inbound music to prepare for adding to storage library.
  * Convert media to standard format.
  * Apply regex based rules for editing metadata.
  * Does configuration driven magic
    * Validation.
    * Song and Disc renumbering.
    * Removes featuring/with artist from song titles'
    * Removes unwanted text from song and album titles.
* Plugin based architecture.
* Job engine
  * Uses cron like scheduling.
  * Scans inbound, staging and libraries for new media and updates.
* Multiple storage library support
  * Storage libraries hold music media files, artist and album images.
  * Allows for configuration of multiple music storage libraries (e.g. across many NAS storage points)
* Web (Blazor Server) UI Editor
  * Edit meta data.
  * Edit album and band photos.
* Robust configuration system
* OpenSubsonic API server
  * Real time transcoding. Including Ogg and Opus formats.
  * Tested with several Subsonic clients
    * Airsonic
    * Feishin
    * Symphonium
    * Sublime Music
    * Supersonic
