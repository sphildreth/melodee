## 🚧 *Melodee is under development, currently in beta mode.* 🚧   

<img src="graphics/melodee_logo.png" alt="logo" title="melodee" align="right" height="60px" />   

# Melodee
   
Melodee is a music system designed to manage and stream music libraries with tens of millions of songs with ease and speed.

![GitHub License](https://img.shields.io/github/license/sphildreth/melodee)
[![.NET](https://github.com/sphildreth/melodee/actions/workflows/dotnet.yml/badge.svg)](https://github.com/sphildreth/melodee/actions/workflows/dotnet.yml)
<a href="https://discord.gg/bfMnEUrvbp">
![Discord](https://img.shields.io/discord/1337921126210211943)
</a>
---

## Components

* Melodee.Blazor - Blazor and OpenSubsonic API server
    * Both OpenSubsonic and Subsonic 1.16.1.
    * Blazor administrative front end using [Radzen UI](https://blazor.radzen.com/) and [Blazor Server](https://learn.microsoft.com/en-us/aspnet/core/blazor/hosting-models?view=aspnetcore-9.0#blazor-server).
    * ![Melodee.Blazor](graphics/Snapshot_2025-02-04_23-06-24.png)
* Melodee.Cli - Command line interface
    * Execute jobs manually.
    * Library management.    
    * Manage configuration values.
    * View meta tags for media files.  

## Features

Melodee handles media to library in every step:
  1. Converts, cleans, normalizes and validates inbound media found in inbound library to staging library.
  2. Allows for manual editing of media in staging library before adding to storage libraries. This allows editing of inbound media without serving to API users.
  3. Allows for manual or automated job execution to scan ready media into storage libraries and main database.
  4. Serves Subsonic clients data from main database and streams from storage libraries.

* Process inbound music to prepare for adding to storage library.
    * Convert media to standard format.
    * Apply regex based rules for editing metadata.
    * Does configuration driven magic
        * Validation.
        * Song renumbering.
        * Removes featuring/with artist from song titles.
        * Removes unwanted text from song and album titles.
* Plugin based architecture.
    * Plugins to parse NFO, M3U, SFV metadata files.
    * Plugins to read and edit tags for 22 types of media files, including: AAC, AC3, M4A, Flac, Ogg, Ape, MP3, WAV, WMA.
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
        * [Airsonic (rexfix)](https://github.com/tamland/airsonic-refix)
        * [Dsub](https://github.com/DataBiosphere/dsub)
        * [Feishin](https://github.com/jeffvli/feishin)
        * [Symphonium](https://symfonium.app/)
        * [Sublime Music](https://github.com/sublime-music/sublime-music)
        * [Supersonic](https://github.com/dweymouth/supersonic)
        * [Ultrasonic](https://gitlab.com/ultrasonic/ultrasonic)
