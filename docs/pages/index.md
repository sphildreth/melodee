---
layout: page
title: Melodee
permalink: /
---

# Melodee Music System

* This page is a work in progress to document all things around setting up and using Melodee. *

## What is Melodee?

Melodee is a streaming music server that serves to both OpenSubsonic and Subsonic clients. Melodee includes the ability to organize, edit and manage music files and user access.

Melodee handles media to library in every step:
  1. Converts, cleans, normalizes and validates inbound media found in inbound library to staging library.
  2. Allows for manual editing of media in staging library before adding to storage libraries. This allows editing of inbound media without serving to API users.
  3. Allows for manual or automated job execution to scan ready media into storage libraries and main database.
  4. Serves Subsonic clients data from main database and streams from storage libraries.

## Features

Some major features of Melodee include:
 - Process inbound music to prepare for adding to storage library.
    * Convert media to standard format.
    * Apply regex based rules for editing metadata.
    * Does configuration driven magic
        * Validation.
        * Song renumbering.
        * Removes featuring/with artist from song titles.
        * Removes unwanted text from song and album titles.
 - Plugin based architecture.
    * Plugins to parse NFO, M3U, SFV metadata files.
    * Plugins to read and edit tags for 22 types of media files, including: AAC, AC3, M4A, Flac, Ogg, Ape, MP3, WAV, WMA.
    * Search engines to find album and artist metadata and images and scrobble.
        * iTunes
        * LastFM
        * MusicBrainz
            * Downloads and creates local MusicBrainz SQLite database for faster metadata lookup.
        * Spotify
  - Job engine
    * Uses cron like scheduling.
    * Scans inbound, staging and storage libraries for new media and updates.
  - Multiple storage library support
    * Storage libraries hold music media files, artist and album images.
    * Allows for configuration of multiple music storage libraries (e.g. across many NAS storage points)
  - Web (Blazor Server) UI Editor
    * Edit meta data.
    * Edit album and band photos.
    * Manage users and user permissions.
    * Manage configuration options.
  - Robust configuration system
  - OpenSubsonic API server
    * Real time transcoding. Including Ogg and Opus formats.
    * Tested with several Subsonic clients
        * [Airsonic (rexfix)](https://github.com/tamland/airsonic-refix)
        * [Dsub](https://github.com/DataBiosphere/dsub)
        * [Feishin](https://github.com/jeffvli/feishin)
        * [Symphonium](https://symfonium.app/)
        * [Sublime Music](https://github.com/sublime-music/sublime-music)
        * [Supersonic](https://github.com/dweymouth/supersonic)
        * [Ultrasonic](https://gitlab.com/ultrasonic/ultrasonic)



For features, getting started with development, see the {% include doc.html name="Getting Started" path="getting-started" %} page. Would you like to request a feature or contribute?
[Open an issue]({{ site.repo }}/issues)
