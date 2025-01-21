![Melodee](graphics/melodee_gh_card.png)

## 🚧 *Melodee is under development, currently in alpha mode.* 🚧

# Melodee

Melodee is a music system designed to handle libraries with tens of millions of songs with ease and speed.

## Components

* Melodee.Blazor - Blazor and OpenSubsonic API server
    * Both OpenSubsonic and Subsonic 1.16.1.
    * Blazor administrative front end.
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
  * Plugins to parse NFO, M3U, SFV metadata files.
  * Plugins to read and edit tags for Flac, Ogg, Ape, MP3 files.
  * Search engines to find album and artist metadata and images and scrobble.
    * iTunes
    * LastFM
    * MusicBrainz
      * Downloads and creates local MusicBrainz SQLite database for faster metadata lookup.
    * Spotify
* Job engine
    * Uses cron like scheduling.
    * Scans inbound, staging and storage libraries for new media and updates.
* Multiple storage library support
    * Storage libraries hold music media files, artist and album images.
    * Allows for configuration of multiple music storage libraries (e.g. across many NAS storage points)
* Web (Blazor Server) UI Editor
    * Edit meta data.
    * Edit album and band photos.
    * Manage users and user permissions.
    * Manage configuration options.
* Robust configuration system
* OpenSubsonic API server
    * Real time transcoding. Including Ogg and Opus formats.
    * Tested with several Subsonic clients
        * Airsonic
        * Feishin
        * Symphonium
        * Sublime Music
        * Supersonic
