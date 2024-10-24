using ATL;
using FFMpegCore;
using Melodee.Common.Constants;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Utility;
using Melodee.Plugins.MetaData.Song.Extensions;
using Melodee.Plugins.Processor;
using Serilog;
using Serilog.Events;
using SerilogTimings;
using SixLabors.ImageSharp;

using ImageInfo = Melodee.Common.Models.ImageInfo;

namespace Melodee.Plugins.MetaData.Song;

/// <summary>
///     Implementation of Song Plugin using ATL Library
///     <remarks>https://github.com/Zeugma440/atldotnet</remarks>
/// </summary>
public sealed class AtlMetaTag(IMetaTagsProcessorPlugin metaTagsProcessorPlugin, Dictionary<string, object?> configuration) : MetaDataBase(configuration), ISongPlugin
{
    private readonly IMetaTagsProcessorPlugin _metaTagsProcessorPlugin = metaTagsProcessorPlugin;
    public override string Id => "0F622E4B-64CD-4033-8B23-BA2001F045FA";

    public override string DisplayName => nameof(AtlMetaTag);

    public override bool IsEnabled { get; set; } = true;

    public override int SortOrder { get; } = 0;

    public override bool DoesHandleFile(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemInfo)
    {
        if (!IsEnabled || !fileSystemInfo.Exists(directoryInfo))
        {
            return false;
        }

        return FileHelper.IsFileMediaType(fileSystemInfo.Extension(directoryInfo));
    }

    public async Task<OperationResult<Common.Models.Song>> ProcessFileAsync(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemFileInfo, CancellationToken cancellationToken = default)
    {
        using (Operation.At(LogEventLevel.Debug).Time("[{PluginName}] Processing [{FileSystemFileInfo}]", DisplayName, fileSystemFileInfo.Name))
        {
            var tags = new List<MetaTag<object?>>();
            var mediaAudios = new List<MediaAudio<object?>>();
            var images = new List<ImageInfo>();

            try
            {
                if (fileSystemFileInfo.Exists(directoryInfo))
                {
                    var fileAtl = new ATL.Track(fileSystemFileInfo.FullName(directoryInfo));
                    if (!fileAtl.MetadataFormats.Any(x => x.ID < 0) && IsAtlTrackForMp3(fileAtl))
                    {
                        var atlDictionary = fileAtl.ToDictionary();

                        var metaAudioIdentifierDictionary = MediaAudioIdentifier.NotSet.ToDictionary();
                        foreach (var metaTagIdentifier in metaAudioIdentifierDictionary)
                        {
                            if (atlDictionary.TryGetValue(metaTagIdentifier.Value, out var v))
                            {
                                if (v is string s)
                                {
                                    v = s.Nullify();
                                }

                                if (v != null)
                                {
                                    var identifier = SafeParser.ToEnum<MediaAudioIdentifier>(metaTagIdentifier.Key);
                                    mediaAudios.Add(new MediaAudio<object?>
                                    {
                                        Identifier = identifier,
                                        Value = v
                                    });
                                }
                            }
                        }

                        IMediaAnalysis? ffProbeMediaAnalysis = null;
                        try
                        {
                            ffProbeMediaAnalysis = await FFProbe.AnalyseAsync(fileSystemFileInfo.FullName(directoryInfo), cancellationToken: cancellationToken);

                            if (ffProbeMediaAnalysis.PrimaryAudioStream != null)
                            {
                                mediaAudios.Add(new MediaAudio<object?>
                                {
                                    Identifier = MediaAudioIdentifier.CodecLongName,
                                    Value = ffProbeMediaAnalysis.PrimaryAudioStream.CodecLongName
                                });
                                mediaAudios.Add(new MediaAudio<object?>
                                {
                                    Identifier = MediaAudioIdentifier.Channels,
                                    Value = ffProbeMediaAnalysis.PrimaryAudioStream.Channels
                                });
                                mediaAudios.Add(new MediaAudio<object?>
                                {
                                    Identifier = MediaAudioIdentifier.ChannelLayout,
                                    Value = ffProbeMediaAnalysis.PrimaryAudioStream.ChannelLayout
                                });
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Error(e, "FileSystemFileInfo [{FileSystemFileInfo}]", fileSystemFileInfo);
                        }

                        if (fileAtl.IsVBR)
                        {
                            mediaAudios.Add(new MediaAudio<object?>
                            {
                                Identifier = MediaAudioIdentifier.IsVbr,
                                Value = true
                            });
                        }

                        if (fileAtl.ChannelsArrangement != null)
                        {
                            mediaAudios.Add(new MediaAudio<object?>
                            {
                                Identifier = MediaAudioIdentifier.ChannelsArrangementDescription,
                                Value = fileAtl.ChannelsArrangement.Description
                            });
                            mediaAudios.Add(new MediaAudio<object?>
                            {
                                Identifier = MediaAudioIdentifier.ChannelsArrangementNumberChannels,
                                Value = fileAtl.ChannelsArrangement.NbChannels
                            });
                        }

                        if (fileAtl.TechnicalInformation != null)
                        {
                            mediaAudios.Add(new MediaAudio<object?>
                            {
                                Identifier = MediaAudioIdentifier.AudioDataOffset,
                                Value = fileAtl.TechnicalInformation.AudioDataOffset
                            });
                            mediaAudios.Add(new MediaAudio<object?>
                            {
                                Identifier = MediaAudioIdentifier.AudioDataSize,
                                Value = fileAtl.TechnicalInformation.AudioDataSize
                            });
                        }

                        var metaTagIdentifierDictionary = MetaTagIdentifier.NotSet.ToDictionary();
                        foreach (var metaTagIdentifier in metaTagIdentifierDictionary)
                        {
                            if (atlDictionary.TryGetValue(metaTagIdentifier.Value, out var v))
                            {
                                if (v is string s)
                                {
                                    v = s.Nullify();
                                }

                                if (v is DateTime vDt)
                                {
                                    v = vDt > DateTime.MinValue && vDt < DateTime.MaxValue ? v : null;
                                }

                                if (v != null)
                                {
                                    var identifier = SafeParser.ToEnum<MetaTagIdentifier>(metaTagIdentifier.Key);
                                    if (metaTagIdentifier.Key == (int)MetaTagIdentifier.Date ||
                                        metaTagIdentifier.Key == (int)MetaTagIdentifier.OrigAlbumDate ||
                                        metaTagIdentifier.Key == (int)MetaTagIdentifier.RecordingDate)
                                    {
                                        var dt = SafeParser.ToDateTime(v);
                                        if (dt.HasValue)
                                        {
                                            tags.Add(new MetaTag<object?>
                                            {
                                                Identifier = MetaTagIdentifier.OrigAlbumYear,
                                                Value = dt.Value.Year
                                            });
                                        }
                                    }

                                    tags.Add(new MetaTag<object?>
                                    {
                                        Identifier = identifier,
                                        Value = v
                                    });
                                }
                            }
                        }

                        var adData1 = fileAtl.AdditionalFields.ToDictionary();
                        var adData2 = ffProbeMediaAnalysis?.Format.Tags?.ToDictionary() ?? new Dictionary<string, string>();
                        var additionalTags = MetaTagsForTagDictionary(DictionaryExtensions.Merge(new[] { adData1, adData2 }));
                        foreach (var additionalTag in additionalTags)
                        {
                            // Additional tag values override any in place
                            var existing = tags.FirstOrDefault(x => x.Identifier == additionalTag.Identifier);
                            if (existing != null)
                            {
                                tags.Remove(existing);
                            }
                            tags.Add(additionalTag);
                        }

                        if (fileAtl.EmbeddedPictures.Any() && SafeParser.ToBoolean(Configuration[SettingRegistry.ProcessingDoLoadEmbeddedImages]))
                        {
                            var albumId = SafeParser.Hash(
                                tags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.AlbumArtist)?.Value?.ToString() ?? string.Empty,
                                tags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.OrigAlbumYear)?.Value?.ToString() ??
                                tags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.RecordingYear)?.Value?.ToString() ??
                                tags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.RecordingDateOrYear)?.Value?.ToString() ?? string.Empty,
                                tags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.Album)?.Value?.ToString() ?? string.Empty);
                            var pictureIndex = 0;
                            foreach (var embeddedPicture in fileAtl.EmbeddedPictures)
                            {
                                var imageInfo = Image.Load(embeddedPicture.PictureData);
                                var imageCrcHash = Crc32.Calculate(embeddedPicture.PictureData);
                                if (directoryInfo.GetFileForCrcHash("jpg", imageCrcHash) == null)
                                {
                                    var pictureIdentifier = SafeParser.ToEnum<PictureIdentifier>(embeddedPicture.PicType);
                                    var newImageFileName = Path.Combine(directoryInfo.Path, $"{albumId}-{(pictureIndex + 1).ToStringPadLeft(2)}-{embeddedPicture.PicType.ToString()}.jpg");
                                    await File.WriteAllBytesAsync(newImageFileName, embeddedPicture.PictureData, cancellationToken);
                                    images.Add(new ImageInfo
                                    {
                                        CrcHash = imageCrcHash,
                                        PictureIdentifier = pictureIdentifier,
                                        FileInfo = new FileInfo(newImageFileName).ToFileSystemInfo(),
                                        Width = imageInfo.Width,
                                        Height = imageInfo.Height,
                                        SortOrder = embeddedPicture.Position,
                                        WasEmbeddedInSong = true
                                    });
                                }

                                pictureIndex++;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "FileSystemFileInfo [{FileSystemFileInfo}]", fileSystemFileInfo);
            }
            
            // Ensure that OrigAlbumYear exists and if not add with invalid date (will get set later by MetaTagProcessor.)
            if (tags.All(x => x.Identifier != MetaTagIdentifier.OrigAlbumYear))
            {
                tags.Add(new MetaTag<object?>
                {
                    Identifier = MetaTagIdentifier.OrigAlbumYear,
                    Value = 0
                });
            }
            
            // Set SortOrder necessary for processors
            var tagCount = tags.Count;
            tags.ForEach(x => x.SortOrder = tagCount);
            tags.First(x => x.Identifier == MetaTagIdentifier.Album).SortOrder = 1;
            tags.First(x => x.Identifier == MetaTagIdentifier.Artist).SortOrder = 2;
            
            // Ensure that AlbumArtist is set, if has fragments will get cleaned up by MetaTag Processor
            if (tags.All(x => x.Identifier != MetaTagIdentifier.AlbumArtist))
            {
                tags.Add(new MetaTag<object?>
                {
                    Identifier = MetaTagIdentifier.AlbumArtist,
                    Value = tags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.Artist)?.Value,
                    SortOrder = 3
                });
            }

            var metaTagsProcessorResult = await _metaTagsProcessorPlugin.ProcessMetaTagAsync(directoryInfo, fileSystemFileInfo, tags, cancellationToken);
            if (!metaTagsProcessorResult.IsSuccess)
            {
                return new OperationResult<Common.Models.Song>(metaTagsProcessorResult.Messages)
                {
                    Errors = metaTagsProcessorResult.Errors,
                    Data = new Common.Models.Song
                    {
                        CrcHash = string.Empty,
                        File = fileSystemFileInfo
                    }
                };
            }

            var song = new Common.Models.Song
            {
                CrcHash = Crc32.Calculate(new FileInfo(fileSystemFileInfo.FullName(directoryInfo))),
                File = fileSystemFileInfo,
                Images = images,
                Tags = metaTagsProcessorResult.Data,
                MediaAudios = mediaAudios,
                SortOrder = tags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.TrackNumber)?.Value as int? ?? 0
            };
            return new OperationResult<Common.Models.Song>
            {
                Data = song
            };
        }
    }

    public async Task<OperationResult<bool>> UpdateSongAsync(FileSystemDirectoryInfo directoryInfo, Common.Models.Song song, CancellationToken cancellationToken = default)
    {
        var result = false;
        if (song.Tags?.Any() ?? false)
        {
            var songFileName = song.File.FullName(directoryInfo);
            using (Operation.At(LogEventLevel.Debug).Time("[{Plugin}] Updating [{FileName}]", DisplayName, songFileName))
            {
                try
                {
                    var fileAtl = new ATL.Track(songFileName)
                    {
                        Album = song.AlbumTitle(),
                        AlbumArtist = song.AlbumArtist(),
                        Artist = song.SongArtist(),
                        DiscNumber = song.MediaNumber(),
                        DiscTotal = song.MediaTotalNumber(),
                        Genre = song.Genre(),
                        OriginalReleaseDate = song.AlbumDateValue(),
                        TrackNumber = song.SongNumber(),
                        TrackTotal = song.SongTotalNumber(),
                        Year = song.AlbumYear()
                    };
                    if (song.Images?.Any() ?? false)
                    {
                        var coverImage = song.Images.FirstOrDefault(x => x.PictureIdentifier is PictureIdentifier.Front or PictureIdentifier.SecondaryFront);
                        if (coverImage != null && (coverImage.FileInfo?.Exists(directoryInfo) ?? false))
                        {
                            fileAtl.EmbeddedPictures.Clear();
                            fileAtl.EmbeddedPictures.Add(PictureInfo.fromBinaryData(
                                await File.ReadAllBytesAsync(coverImage.FileInfo!.FullName(directoryInfo), cancellationToken),
                                PictureInfo.PIC_TYPE.Front));
                        }
                    }
                    else
                    {
                        fileAtl.EmbeddedPictures.Clear();
                    }

                    await fileAtl.SaveAsync();
                    result = true;
                }
                catch (Exception e)
                {
                    Log.Error(e, "FileSystemFileInfo [{FileSystemFileInfo}]", directoryInfo);
                }
            }
        }

        return new OperationResult<bool>
        {
            Data = result
        };
    }

    private IEnumerable<MetaTag<object?>> MetaTagsForTagDictionary(Dictionary<string, string> tagsDictionary)
    {
        var result = new List<MetaTag<object?>>();
        if (tagsDictionary.Count == 0)
        {
            return result;
        }

        foreach (var kp in tagsDictionary)
        {
            var k = kp.Key.CleanString()?.Replace(" ", string.Empty).ToUpperInvariant();
            switch (k)
            {
                case "ARTISTS":
                    if (result.All(x => x.Identifier != MetaTagIdentifier.Artist))
                    {
                        var artists = kp.Value.ToCleanedMultipleArtistsValue();
                        if (artists != null)
                        {
                            result.Add(new MetaTag<object?>
                            {
                                Identifier = MetaTagIdentifier.Artists,
                                Value = artists
                            });
                        }
                    }
                    break;

                case "LENGTH":
                    if (result.All(x => x.Identifier != MetaTagIdentifier.Length))
                    {
                        result.Add(new MetaTag<object?>
                        {
                            Identifier = MetaTagIdentifier.Length,
                            Value = kp.Value
                        });
                    }

                    break;

                case "DATE":
                    if (result.All(x => x.Identifier != MetaTagIdentifier.OrigAlbumDate))
                    {
                        result.Add(new MetaTag<object?>
                        {
                            Identifier = MetaTagIdentifier.OrigAlbumDate,
                            Value = kp.Value
                        });
                    }

                    break;

                case "Song":
                    if (result.All(x => x.Identifier != MetaTagIdentifier.SongNumberTotal))
                    {
                        if (kp.Value.Contains('/'))
                        {
                            result.Add(new MetaTag<object?>
                            {
                                Identifier = MetaTagIdentifier.SongNumberTotal,
                                Value = kp.Value
                            });
                        }
                    }

                    break;

                // ReSharper disable once StringLiteralTypo
                case "WXXX":
                    if (result.All(x => x.Identifier != MetaTagIdentifier.UserDefinedUrlLink))
                    {
                        result.Add(new MetaTag<object?>
                        {
                            Identifier = MetaTagIdentifier.UserDefinedUrlLink,
                            Value = kp.Value
                        });
                    }

                    break;
            }
        }

        return result;
    }


    private static bool IsAtlTrackForMp3(ATL.Track track)
    {
        if (track.AudioFormat?.ShortName == null)
        {
            return false;
        }

        if (string.Equals(track.AudioFormat?.ShortName, "mpeg-4", StringComparison.OrdinalIgnoreCase))
        {
            var ext = track.FileInfo().Extension;
            if (!ext.ToLower().EndsWith("m4a")) // M4A is an audio file using the MP4 encoding
            {
                Console.WriteLine($"Video file found in Scanning. File [{track.FileInfo().FullName}]");
                return false;
            }
        }

        return track is { AudioFormat: { ID: > -1 }, Duration: > 0 };
    }
}
