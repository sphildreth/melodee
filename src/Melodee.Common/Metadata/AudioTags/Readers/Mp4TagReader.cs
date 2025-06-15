using System.Diagnostics;
using System.Text;
using Melodee.Common.Enums;
using Melodee.Common.Metadata.AudioTags.Interfaces;
using Melodee.Common.Metadata.AudioTags.Models;

namespace Melodee.Common.Metadata.AudioTags.Readers;

public class Mp4TagReader : ITagReader, IMediaAudioReader
{
    // MP4 atom (box) types
    private const string FTYP = "ftyp";
    private const string MOOV = "moov";
    private const string UDTA = "udta";
    private const string META = "meta";
    private const string ILST = "ilst";
    private const string TRAK = "trak";
    private const string MDIA = "mdia";
    private const string MINF = "minf";
    private const string STBL = "stbl";
    private const string STSD = "stsd";
    private const string HDLR = "hdlr";
    private const string SOUN = "soun";
    private const string MP4A = "mp4a";
    private const string ESDS = "esds";

    // Common metadata atoms
    private const string TITLE = "©nam";
    private const string ARTIST1 = "©ART"; // Standard iTunes artist atom
    private const string ARTIST2 = "ART "; // Alternative artist atom (with space)
    private const string ARTIST3 = "aART"; // Album artist that might be used as fallback
    private const string PERFORMER = "perf"; // Performer atom used in some files
    private const string ALBUM = "©alb";
    private const string YEAR = "©day";
    private const string GENRE = "©gen";
    private const string COMMENT = "©cmt";
    private const string TRACK_NUMBER = "trkn";
    private const string DISC_NUMBER = "disk";
    private const string COMPOSER = "©wrt";
    private const string ALBUM_ARTIST = "aART";
    private const string COPYRIGHT = "cprt";
    private const string COVER_ART = "covr";
    private const string LYRICS = "©lyr";
    private const string COMPILATION = "cpil";
    private readonly Dictionary<MediaAudioIdentifier, object> _mediaAudios = new();

    public async Task<IDictionary<MediaAudioIdentifier, object>> ReadMediaAudiosAsync(string filePath, CancellationToken cancellationToken = default)
    {
        await ReadTagsAsync(filePath, cancellationToken).ConfigureAwait(false);
        return _mediaAudios;
    }

    public async Task<IDictionary<MetaTagIdentifier, object>> ReadTagsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var tags = new Dictionary<MetaTagIdentifier, object>();
        var foundAnyMetadata = false;

        try
        {
            await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);

            // Initialize tag dictionary with empty values for common tags
            InitializeDefaultTags(tags);

            // Look for the moov atom which contains all metadata
            var moovAtom = await FindAtom(stream, MOOV, cancellationToken);
            if (moovAtom != null)
            {
                // Process moov atom to find metadata
                await using var moovStream = new MemoryStream(moovAtom);

                // 1. Look for metadata in udta > meta > ilst
                var udtaAtom = await FindAtom(moovStream, UDTA, cancellationToken);
                if (udtaAtom != null)
                {
                    await using var udtaStream = new MemoryStream(udtaAtom);
                    var metaAtom = await FindAtom(udtaStream, META, cancellationToken);

                    if (metaAtom != null && metaAtom.Length > 4)
                    {
                        // Skip 4 bytes of meta version/flags
                        await using var metaStream = new MemoryStream(metaAtom, 4, metaAtom.Length - 4);
                        var ilstAtom = await FindAtom(metaStream, ILST, cancellationToken);

                        if (ilstAtom != null)
                        {
                            await ReadMetadataFromIlst(ilstAtom, tags, cancellationToken);
                            foundAnyMetadata = true;
                        }
                    }
                }

                // 2. Look for direct meta > ilst in moov (some MP4 files have this structure)
                if (!foundAnyMetadata)
                {
                    moovStream.Position = 0;
                    var metaAtom = await FindAtom(moovStream, META, cancellationToken);

                    if (metaAtom != null && metaAtom.Length > 4)
                    {
                        // Skip 4 bytes of meta version/flags
                        await using var metaStream = new MemoryStream(metaAtom, 4, metaAtom.Length - 4);
                        var ilstAtom = await FindAtom(metaStream, ILST, cancellationToken);

                        if (ilstAtom != null)
                        {
                            await ReadMetadataFromIlst(ilstAtom, tags, cancellationToken);
                            foundAnyMetadata = true;
                        }
                    }
                }

                // 3. Look for track info in trak atoms
                moovStream.Position = 0;
                while (moovStream.Position < moovStream.Length - 8)
                {
                    var atomInfo = await ReadAtomHeader(moovStream, cancellationToken);
                    if (atomInfo.AtomType == TRAK)
                    {
                        var trakData = new byte[atomInfo.AtomSize - 8];
                        await moovStream.ReadExactlyAsync(trakData, 0, trakData.Length, cancellationToken);

                        await using var trakStream = new MemoryStream(trakData);

                        // Extract track number if missing
                        if (string.IsNullOrEmpty(tags[MetaTagIdentifier.TrackNumber]?.ToString()))
                        {
                            var foundTrackInfo = await ReadTrackInfoFromMoov(moovStream, tags, cancellationToken);
                            if (foundTrackInfo)
                            {
                                foundAnyMetadata = true;
                            }
                        }

                        // Extract MPEG audio details from this track
                        var foundAudioDetails = await ExtractMpegAudioDetailsFromTrack(trakStream, tags, _mediaAudios, cancellationToken);
                        if (foundAudioDetails)
                        {
                            foundAnyMetadata = true;
                        }
                    }
                    else
                    {
                        // Skip this atom
                        moovStream.Seek(atomInfo.AtomSize - 8, SeekOrigin.Current);
                    }
                }

                // 4. If still no artist but we have album artist, use that as a fallback
                if (string.IsNullOrEmpty(tags[MetaTagIdentifier.Artist]?.ToString()) &&
                    !string.IsNullOrEmpty(tags[MetaTagIdentifier.AlbumArtist]?.ToString()))
                {
                    tags[MetaTagIdentifier.Artist] = tags[MetaTagIdentifier.AlbumArtist];
                    foundAnyMetadata = true;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error reading MP4 tags: {ex.Message}");
        }

        // If no actual metadata was found, return an empty dictionary
        if (!foundAnyMetadata)
        {
            return new Dictionary<MetaTagIdentifier, object>();
        }

        return tags;
    }

    public async Task<IReadOnlyList<AudioImage>> ReadImagesAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var images = new List<AudioImage>();

        try
        {
            await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);

            // Look for the moov > udta > meta > ilst > covr atom which contains cover art
            var moovAtom = await FindAtom(stream, MOOV, cancellationToken);
            if (moovAtom != null)
            {
                await using var moovStream = new MemoryStream(moovAtom);
                var udtaAtom = await FindAtom(moovStream, UDTA, cancellationToken);

                if (udtaAtom != null)
                {
                    await using var udtaStream = new MemoryStream(udtaAtom);
                    var metaAtom = await FindAtom(udtaStream, META, cancellationToken);

                    if (metaAtom != null && metaAtom.Length > 4)
                    {
                        // Skip 4 bytes of meta version/flags
                        await using var metaStream = new MemoryStream(metaAtom, 4, metaAtom.Length - 4);
                        var ilstAtom = await FindAtom(metaStream, ILST, cancellationToken);

                        if (ilstAtom != null)
                        {
                            await using var ilstStream = new MemoryStream(ilstAtom);

                            while (ilstStream.Position < ilstStream.Length - 8)
                            {
                                var atomInfo = await ReadAtomHeader(ilstStream, cancellationToken);

                                if (atomInfo.AtomType == COVER_ART)
                                {
                                    var covrData = new byte[atomInfo.AtomSize - 8];
                                    await ilstStream.ReadExactlyAsync(covrData, 0, covrData.Length, cancellationToken);

                                    // Process cover art data
                                    if (covrData.Length > 16) // Minimum data header plus some image data
                                    {
                                        // Look for data atom inside
                                        if (covrData.Length >= 8)
                                        {
                                            var dataSize = (covrData[0] << 24) | (covrData[1] << 16) | (covrData[2] << 8) | covrData[3];
                                            var dataType = Encoding.ASCII.GetString(covrData, 4, 4);

                                            if (dataType == "data" && dataSize <= covrData.Length)
                                            {
                                                // Get image format from data type value
                                                var formatType = (covrData[8] << 24) | (covrData[9] << 16) | (covrData[10] << 8) | covrData[11];

                                                var mimeType = "image/jpeg"; // Default
                                                if (formatType == 14)
                                                {
                                                    mimeType = "image/png";
                                                }

                                                // Extract image data (skipping 16-byte header)
                                                var imageOffset = 16;
                                                if (imageOffset < covrData.Length)
                                                {
                                                    var imageData = new byte[covrData.Length - imageOffset];
                                                    Array.Copy(covrData, imageOffset, imageData, 0, imageData.Length);

                                                    images.Add(new AudioImage
                                                    {
                                                        Data = imageData,
                                                        MimeType = mimeType,
                                                        Description = null,
                                                        Type = PictureIdentifier.Front
                                                    });
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // Skip this atom
                                    ilstStream.Seek(atomInfo.AtomSize - 8, SeekOrigin.Current);
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error reading MP4 images: {ex.Message}");
        }

        return images;
    }

    public async Task<object?> ReadTagAsync(string filePath, MetaTagIdentifier tagId, CancellationToken cancellationToken = default)
    {
        var tags = await ReadTagsAsync(filePath, cancellationToken);
        return tags.TryGetValue(tagId, out var value) ? value : null;
    }

    private void InitializeDefaultTags(Dictionary<MetaTagIdentifier, object> tags)
    {
        // Initialize common tags with empty values to ensure they're always present
        tags[MetaTagIdentifier.Title] = string.Empty;
        tags[MetaTagIdentifier.Artist] = string.Empty;
        tags[MetaTagIdentifier.Album] = string.Empty;
        tags[MetaTagIdentifier.RecordingYear] = string.Empty;
        tags[MetaTagIdentifier.Genre] = string.Empty;
        tags[MetaTagIdentifier.TrackNumber] = string.Empty;
        tags[MetaTagIdentifier.DiscNumber] = string.Empty;
        tags[MetaTagIdentifier.Comment] = string.Empty;
        tags[MetaTagIdentifier.Composer] = string.Empty;
        tags[MetaTagIdentifier.AlbumArtist] = string.Empty;
        tags[MetaTagIdentifier.Copyright] = string.Empty;

        // // Initialize MPEG audio detail tags
        // tags[MediaAudioIdentifier.CodecLongName] = string.Empty; // For MPEG Audio Version ID
        // tags[MediaAudioIdentifier.Channels] = string.Empty;     // For Channels
        // tags[MediaAudioIdentifier.ChannelLayout] = string.Empty; // For Channel mode
    }

    private async Task<bool> ReadTrackInfoFromMoov(Stream moovStream, Dictionary<MetaTagIdentifier, object> tags, CancellationToken cancellationToken)
    {
        try
        {
            while (moovStream.Position < moovStream.Length - 8)
            {
                var atomInfo = await ReadAtomHeader(moovStream, cancellationToken);
                if (atomInfo.AtomType == TRAK)
                {
                    var trakData = new byte[atomInfo.AtomSize - 8];
                    await moovStream.ReadExactlyAsync(trakData, 0, trakData.Length, cancellationToken);

                    await using var trakStream = new MemoryStream(trakData);
                    var mdiaAtom = await FindAtom(trakStream, MDIA, cancellationToken);

                    if (mdiaAtom != null)
                    {
                        await using var mdiaStream = new MemoryStream(mdiaAtom);
                        var trackNumber = await ExtractTrackNumber(mdiaStream, cancellationToken);

                        if (trackNumber > 0)
                        {
                            tags[MetaTagIdentifier.TrackNumber] = trackNumber.ToString(); // Store as string for consistency
                            return true; // Found track number
                        }
                    }
                }
                else
                {
                    // Skip this atom
                    moovStream.Seek(atomInfo.AtomSize - 8, SeekOrigin.Current);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error reading track info from moov: {ex.Message}");
        }

        return false; // No track information found
    }

    private async Task<int> ExtractTrackNumber(Stream mdiaStream, CancellationToken cancellationToken)
    {
        try
        {
            // Navigate through mdia > minf > stbl > stsd to find track number
            var minfAtom = await FindAtom(mdiaStream, MINF, cancellationToken);
            if (minfAtom != null)
            {
                await using var minfStream = new MemoryStream(minfAtom);
                var stblAtom = await FindAtom(minfStream, STBL, cancellationToken);

                if (stblAtom != null)
                {
                    await using var stblStream = new MemoryStream(stblAtom);
                    var stsdAtom = await FindAtom(stblStream, STSD, cancellationToken);

                    if (stsdAtom != null && stsdAtom.Length > 8)
                    {
                        // First 4 bytes after header are version (1 byte) and flags (3 bytes)
                        // Next 4 bytes are entry count
                        var entryCountBytes = new byte[4];
                        Array.Copy(stsdAtom, 4, entryCountBytes, 0, 4);
                        var entryCount = (entryCountBytes[0] << 24) | (entryCountBytes[1] << 16) |
                                         (entryCountBytes[2] << 8) | entryCountBytes[3];

                        if (entryCount > 0)
                        {
                            return 1; // Default to track 1 if we found valid entries
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error extracting track number: {ex.Message}");
        }

        return 0;
    }

    private async Task ReadMetadataFromIlst(byte[] ilstData, Dictionary<MetaTagIdentifier, object> tags, CancellationToken cancellationToken)
    {
        try
        {
            await using var ilstStream = new MemoryStream(ilstData);

            while (ilstStream.Position < ilstStream.Length - 8)
            {
                var atomInfo = await ReadAtomHeader(ilstStream, cancellationToken);

                // Read the atom data
                var atomData = new byte[atomInfo.AtomSize - 8];
                await ilstStream.ReadExactlyAsync(atomData, 0, atomData.Length, cancellationToken);

                switch (atomInfo.AtomType)
                {
                    case TITLE:
                        tags[MetaTagIdentifier.Title] = ExtractStringValue(atomData);
                        break;
                    case ARTIST1: // Primary artist atom
                    case ARTIST2: // Alternative artist atom
                    case PERFORMER: // Some files use performer instead
                        tags[MetaTagIdentifier.Artist] = ExtractStringValue(atomData);
                        break;
                    case ALBUM:
                        tags[MetaTagIdentifier.Album] = ExtractStringValue(atomData);
                        break;
                    case YEAR:
                        var yearValue = ExtractStringValue(atomData);
                        // Some MP4 files store year as "YYYY-MM-DD" - extract just the year
                        if (yearValue.Length >= 4 && int.TryParse(yearValue.Substring(0, 4), out _))
                        {
                            tags[MetaTagIdentifier.RecordingYear] = yearValue.Substring(0, 4);
                        }
                        else
                        {
                            tags[MetaTagIdentifier.RecordingYear] = yearValue;
                        }

                        break;
                    case GENRE:
                        tags[MetaTagIdentifier.Genre] = ExtractStringValue(atomData);
                        break;
                    case COMMENT:
                        tags[MetaTagIdentifier.Comment] = ExtractStringValue(atomData);
                        break;
                    case TRACK_NUMBER:
                        var trackInfo = ExtractNumberPairValue(atomData);
                        if (trackInfo.Item1 > 0)
                        {
                            tags[MetaTagIdentifier.TrackNumber] = trackInfo.Item1.ToString();
                            if (trackInfo.Item2 > 0)
                            {
                                tags[MetaTagIdentifier.SongTotal] = trackInfo.Item2.ToString();
                            }
                        }

                        break;
                    case DISC_NUMBER:
                        var discInfo = ExtractNumberPairValue(atomData);
                        if (discInfo.Item1 > 0)
                        {
                            tags[MetaTagIdentifier.DiscNumber] = discInfo.Item1.ToString();
                        }

                        break;
                    case COMPOSER:
                        tags[MetaTagIdentifier.Composer] = ExtractStringValue(atomData);
                        break;
                    case ALBUM_ARTIST:
                        tags[MetaTagIdentifier.AlbumArtist] = ExtractStringValue(atomData);
                        break;
                    case COPYRIGHT:
                        tags[MetaTagIdentifier.Copyright] = ExtractStringValue(atomData);
                        break;
                    case LYRICS:
                        tags[MetaTagIdentifier.UnsynchronisedLyrics] = ExtractStringValue(atomData);
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error reading metadata from ilst: {ex.Message}");
        }
    }

    private string ExtractStringValue(byte[] data)
    {
        try
        {
            if (data == null || data.Length < 8)
            {
                return string.Empty;
            }

            // Check for data type atom inside
            var dataSize = (data[0] << 24) | (data[1] << 16) | (data[2] << 8) | data[3];
            var dataType = Encoding.ASCII.GetString(data, 4, 4);

            if (dataType == "data" && dataSize <= data.Length)
            {
                // Data atom structure:
                // Bytes 0-3: Size
                // Bytes 4-7: "data" marker
                // Bytes 8-11: Type indicator (1 = UTF-8, 0 = UTF-8, 3 = UTF-8 no BOM, etc.)
                // Bytes 12-15: Locale/flags
                // Bytes 16+: Actual data

                // Get type indicator
                var typeIndicator = (data[8] << 24) | (data[9] << 16) | (data[10] << 8) | data[11];
                var valueOffset = 16; // Default for most atoms

                // Some MP4 files have different data structures based on type
                if (typeIndicator == 0 || typeIndicator == 1 || typeIndicator == 3) // Text types (UTF-8)
                {
                    if (valueOffset < data.Length)
                    {
                        return Encoding.UTF8.GetString(data, valueOffset, data.Length - valueOffset).TrimEnd('\0');
                    }
                }
                else if (typeIndicator == 2) // UTF-16
                {
                    if (valueOffset < data.Length)
                    {
                        return Encoding.Unicode.GetString(data, valueOffset, data.Length - valueOffset).TrimEnd('\0');
                    }
                }
                else // Try generic handling for other types
                {
                    if (valueOffset < data.Length)
                    {
                        try
                        {
                            // Try UTF-8 first as it's most common
                            return Encoding.UTF8.GetString(data, valueOffset, data.Length - valueOffset).TrimEnd('\0');
                        }
                        catch
                        {
                            // Fall back to ASCII if UTF-8 fails
                            return Encoding.ASCII.GetString(data, valueOffset, data.Length - valueOffset).TrimEnd('\0');
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error extracting string value: {ex.Message}");
        }

        return string.Empty;
    }

    private Tuple<int, int> ExtractNumberPairValue(byte[] data)
    {
        try
        {
            // Check for minimal data size
            if (data == null || data.Length < 16)
            {
                return new Tuple<int, int>(0, 0);
            }

            // Extract the data part
            var dataSize = (data[0] << 24) | (data[1] << 16) | (data[2] << 8) | data[3];
            var dataType = Encoding.ASCII.GetString(data, 4, 4);

            if (dataType == "data" && dataSize <= data.Length)
            {
                // Different MP4 implementations use different formats for track numbers
                // Common format is 16-byte header + 2-byte empty + 2-byte track + 2-byte total
                if (data.Length >= 22)
                {
                    var trackNum = (data[18] << 8) | data[19];
                    var totalTracks = (data[20] << 8) | data[21];
                    return new Tuple<int, int>(trackNum, totalTracks);
                }
                // Alternative format sometimes found is 16-byte header + 4-byte track + 4-byte total

                if (data.Length >= 24)
                {
                    var trackNum = (data[16] << 24) | (data[17] << 16) | (data[18] << 8) | data[19];
                    var totalTracks = (data[20] << 24) | (data[21] << 16) | (data[22] << 8) | data[23];
                    return new Tuple<int, int>(trackNum, totalTracks);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error extracting number pair: {ex.Message}");
        }

        return new Tuple<int, int>(0, 0);
    }

    private async Task<byte[]?> FindAtom(Stream stream, string atomType, CancellationToken cancellationToken)
    {
        var originalPosition = stream.Position;

        try
        {
            // Start from current position instead of rewinding to beginning
            while (stream.Position < stream.Length - 8)
            {
                var atomInfo = await ReadAtomHeader(stream, cancellationToken);

                if (atomInfo.AtomType == atomType)
                {
                    var atomData = new byte[atomInfo.AtomSize - 8]; // Exclude the header
                    var bytesRead = await stream.ReadAsync(atomData, 0, atomData.Length, cancellationToken);

                    if (bytesRead == atomData.Length)
                    {
                        return atomData;
                    }

                    return null;
                }

                // Skip to the next atom
                stream.Seek(atomInfo.AtomSize - 8, SeekOrigin.Current);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error finding atom '{atomType}': {ex.Message}");
        }
        finally
        {
            stream.Position = originalPosition;
        }

        return null;
    }

    private async Task<(long AtomSize, string AtomType)> ReadAtomHeader(Stream stream, CancellationToken cancellationToken)
    {
        // Check if we have enough bytes for a header
        if (stream.Position > stream.Length - 8)
        {
            return (8, "");
        }

        var header = new byte[8];
        await stream.ReadExactlyAsync(header, 0, 8, cancellationToken);

        long atomSize = (header[0] << 24) | (header[1] << 16) | (header[2] << 8) | header[3];
        var atomType = Encoding.ASCII.GetString(header, 4, 4);

        // Handle 64-bit size
        if (atomSize == 1 && stream.Length >= stream.Position + 8)
        {
            var extendedSize = new byte[8];
            await stream.ReadExactlyAsync(extendedSize, 0, 8, cancellationToken);

            atomSize = (long)(((ulong)extendedSize[0] << 56) | ((ulong)extendedSize[1] << 48) |
                              ((ulong)extendedSize[2] << 40) | ((ulong)extendedSize[3] << 32) |
                              ((ulong)extendedSize[4] << 24) | ((ulong)extendedSize[5] << 16) |
                              ((ulong)extendedSize[6] << 8) | extendedSize[7]);
        }

        // Handle special cases
        if (atomSize == 0)
        {
            atomSize = stream.Length - stream.Position + 8; // Rest of the file
        }
        else if (atomSize < 8)
        {
            atomSize = 8; // Minimum valid size
        }

        return (atomSize, atomType);
    }

    private async Task<bool> ExtractMpegAudioDetailsFromTrack(Stream trakStream, Dictionary<MetaTagIdentifier, object> tags, Dictionary<MediaAudioIdentifier, object> mediaAudios, CancellationToken cancellationToken)
    {
        try
        {
            var mdiaAtom = await FindAtom(trakStream, MDIA, cancellationToken);
            if (mdiaAtom == null)
            {
                return false;
            }

            // First check if this is an audio track
            await using var mdiaStream = new MemoryStream(mdiaAtom);
            var hdlrAtom = await FindAtom(mdiaStream, HDLR, cancellationToken);

            var isAudioTrack = false;

            if (hdlrAtom != null && hdlrAtom.Length > 24)
            {
                // The handler type is at offset 8 (4 bytes version/flags + 4 bytes predefined)
                // It's a 4 byte ASCII string like 'soun' for sound
                var handlerType = Encoding.ASCII.GetString(hdlrAtom, 8, 4);
                isAudioTrack = handlerType == SOUN;
            }

            if (!isAudioTrack)
            {
                return false;
            }

            // Reset position and look for minf (media information)
            mdiaStream.Position = 0;
            var minfAtom = await FindAtom(mdiaStream, MINF, cancellationToken);
            if (minfAtom == null)
            {
                return false;
            }

            await using var minfStream = new MemoryStream(minfAtom);
            var stblAtom = await FindAtom(minfStream, STBL, cancellationToken);
            if (stblAtom == null)
            {
                return false;
            }

            await using var stblStream = new MemoryStream(stblAtom);
            var stsdAtom = await FindAtom(stblStream, STSD, cancellationToken);
            if (stsdAtom == null || stsdAtom.Length < 16)
            {
                return false;
            }

            // The stsd atom contains audio format descriptions
            // First 4 bytes are version and flags, next 4 bytes are entry count

            var entryCount = (stsdAtom[4] << 24) | (stsdAtom[5] << 16) | (stsdAtom[6] << 8) | stsdAtom[7];
            if (entryCount == 0)
            {
                return false;
            }

            // Each entry begins at offset 8
            var position = 8;

            for (var i = 0; i < entryCount && position < stsdAtom.Length - 16; i++)
            {
                // Read entry size (first 4 bytes)
                var entrySize = (stsdAtom[position] << 24) | (stsdAtom[position + 1] << 16) |
                                (stsdAtom[position + 2] << 8) | stsdAtom[position + 3];

                // Read format type (next 4 bytes)
                var formatType = Encoding.ASCII.GetString(stsdAtom, position + 4, 4);

                // For MP4 audio, we're looking for "mp4a" atom
                if (formatType == MP4A && entrySize > 28)
                {
                    // Read channel count (bytes at offset 24 from entry start)
                    var channels = (stsdAtom[position + 24] << 8) | stsdAtom[position + 25];
                    mediaAudios[MediaAudioIdentifier.Channels] = channels.ToString();

                    // Set channel mode based on channel count
                    var channelLayout = channels switch
                    {
                        1 => "Mono",
                        2 => "Stereo",
                        _ => $"{channels} channels"
                    };
                    mediaAudios[MediaAudioIdentifier.ChannelLayout] = channelLayout;

                    // Look for the esds atom which has MPEG details
                    var esdsOffset = FindAtomInBytes(stsdAtom, position, entrySize, ESDS);

                    if (esdsOffset > 0)
                    {
                        // esds has the following structure:
                        // 4 bytes size + 4 bytes 'esds' + 4 bytes version/flags + ES descriptor
                        // We need to parse the ES descriptor to get MPEG audio version and layer

                        var esdsStart = esdsOffset + 12; // Skip header and version/flags

                        if (esdsStart < stsdAtom.Length)
                        {
                            // Look for the decoder config descriptor (tag 0x04)
                            for (var j = esdsStart; j < Math.Min(esdsStart + 30, stsdAtom.Length - 4); j++)
                            {
                                if (stsdAtom[j] == 0x04 && j + 2 < stsdAtom.Length)
                                {
                                    // Decoder config descriptor found
                                    // Object type is next byte (0x40 = AAC, 0x69 = MP3, etc.)
                                    int objectType = stsdAtom[j + 1];

                                    // Map object types to MPEG audio versions
                                    var codecName = objectType switch
                                    {
                                        0x69 => "MPEG-1 Audio Layer III",
                                        0x6B => "MPEG-2 Audio Layer III",
                                        0x40 => "AAC (Advanced Audio Coding)",
                                        0x66 => "MPEG-2 Audio Layer II",
                                        0x67 => "MPEG-1 Audio Layer II",
                                        0x68 => "MPEG-1 Audio Layer I",
                                        0x6A => "MPEG-2 Audio Layer I",
                                        _ => $"MPEG Audio (object type 0x{objectType:X2})"
                                    };

                                    mediaAudios[MediaAudioIdentifier.CodecLongName] = codecName;

                                    // Extract layer information from codec name if available
                                    if (codecName.Contains("Layer I"))
                                    {
                                        mediaAudios[MediaAudioIdentifier.Layer] = "Layer I";
                                    }
                                    else if (codecName.Contains("Layer II"))
                                    {
                                        mediaAudios[MediaAudioIdentifier.Layer] = "Layer II";
                                    }
                                    else if (codecName.Contains("Layer III"))
                                    {
                                        mediaAudios[MediaAudioIdentifier.Layer] = "Layer III";
                                    }
                                    else
                                    {
                                        mediaAudios[MediaAudioIdentifier.Layer] = string.Empty;
                                    }

                                    return true;
                                }
                            }
                        }
                    }

                    // If we couldn't find specific MPEG details, at least set a default codec name
                    mediaAudios[MediaAudioIdentifier.CodecLongName] = "MPEG-4 Audio";
                    mediaAudios[MediaAudioIdentifier.Layer] = string.Empty;

                    return true;
                }

                // Move to the next entry
                position += entrySize;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error extracting MPEG audio details: {ex.Message}");
        }

        return false;
    }

    private int FindAtomInBytes(byte[] data, int startPosition, int maxLength, string atomType)
    {
        // Look for the specified atom type within the data array
        var typeBytes = Encoding.ASCII.GetBytes(atomType);

        var endPosition = Math.Min(startPosition + maxLength, data.Length - 8);

        for (var i = startPosition; i < endPosition; i++)
        {
            if (i + 8 <= data.Length &&
                data[i + 4] == typeBytes[0] &&
                data[i + 5] == typeBytes[1] &&
                data[i + 6] == typeBytes[2] &&
                data[i + 7] == typeBytes[3])
            {
                return i;
            }
        }

        return -1;
    }
}
