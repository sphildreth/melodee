using ATL;
using FFMpegCore;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Plugins.Conversion.Image;
using Melodee.Common.Plugins.MetaData.Song.Extensions;
using Melodee.Common.Plugins.Processor;
using Melodee.Common.Plugins.Validation;
using Melodee.Common.Utility;
using Serilog;
using Serilog.Events;
using SerilogTimings;
using SixLabors.ImageSharp;
using ImageInfo = Melodee.Common.Models.ImageInfo;

namespace Melodee.Common.Plugins.MetaData.Song;

/// <summary>
///     Implementation of Song Plugin using ATL Library
///     <remarks>https://github.com/Zeugma440/atldotnet</remarks>
/// </summary>
public sealed class AtlMetaTag(
    IMetaTagsProcessorPlugin metaTagsProcessorPlugin,
    ImageConvertor imageConverter,
    IImageValidator imageValidator,
    IMelodeeConfiguration configuration) : MetaDataBase(configuration),
    ISongPlugin
{
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

    public async Task<OperationResult<Models.Song>> ProcessFileAsync(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemFileInfo, CancellationToken cancellationToken = default)
    {
        var tags = new List<MetaTag<object?>>();
        var mediaAudios = new List<MediaAudio<object?>>();
        var images = new List<ImageInfo>();

        try
        {
            if (fileSystemFileInfo.Exists(directoryInfo))
            {
                var fileAtl = new Track(fileSystemFileInfo.FullName(directoryInfo));
                if (!fileAtl.MetadataFormats.Any(x => x.ID < 0) && IsAtlTrackForAudioFile(fileAtl))
                {
                    var atlDictionary = fileAtl.ToDictionary().ToDictionary(x => x.Key.ToNormalizedString() ?? x.Key, x => x.Value);

                    var metaAudioIdentifierDictionary = MediaAudioIdentifier.NotSet.ToNormalizedDictionary();
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

                    var metaTagIdentifierDictionary = MetaTagIdentifier.NotSet.ToNormalizedDictionary();
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
                                    metaTagIdentifier.Key == (int)MetaTagIdentifier.AlbumDate ||
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

                                if (identifier == MetaTagIdentifier.DiscNumber ||
                                    identifier == MetaTagIdentifier.DiscTotal)
                                {
                                    // Every album with songs has at least one disc
                                    if (SafeParser.ToNumber<short>(v) < 1)
                                    {
                                        v = 1;
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

                    if (fileAtl.EmbeddedPictures.Any() && SafeParser.ToBoolean(Configuration[SettingRegistry.ImagingDoLoadEmbeddedImages]))
                    {
                        try
                        {
                            var pictureIndex = 0;
                            foreach (var embeddedPicture in fileAtl.EmbeddedPictures)
                            {
                                var imageInfo = Image.Load(embeddedPicture.PictureData);
                                var imageCrcHash = Crc32.Calculate(embeddedPicture.PictureData);
                                if (directoryInfo.GetFileForCrcHash("jpg", imageCrcHash) == null)
                                {
                                    var pictureIdentifier = SafeParser.ToEnum<PictureIdentifier>(embeddedPicture.PicType);
                                    var newImageFileName = Path.Combine(directoryInfo.Path, $"{(pictureIndex + 1).ToStringPadLeft(2)}-{embeddedPicture.PicType.ToString()}.jpg");
                                    await File.WriteAllBytesAsync(newImageFileName, embeddedPicture.PictureData, cancellationToken);
                                    var newImageFileInfo = new FileInfo(newImageFileName).ToFileSystemInfo();
                                    newImageFileInfo = (await imageConverter.ProcessFileAsync(directoryInfo, newImageFileInfo, cancellationToken).ConfigureAwait(false)).Data;
                                    if ((await imageValidator.ValidateImage(newImageFileInfo.ToFileInfo(directoryInfo), pictureIdentifier, cancellationToken)).Data.IsValid)
                                    {
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
                                    else
                                    {
                                        Log.Warning("[{PluginName}] embedded image did not pass validation.", nameof(AtlMetaTag));
                                    }
                                }

                                pictureIndex++;
                            }
                        }
                        catch (Exception e)
                        {
                            Log.Warning(e, "Could not load embedded pictures [{FileInfo}]", fileSystemFileInfo);
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

        // If the album title isn't set try to parse it from the directory name
        if (tags.All(x => x.Identifier != MetaTagIdentifier.Album))
        {
            tags.Add(new MetaTag<object?>
            {
                Identifier = MetaTagIdentifier.Album,
                Value = directoryInfo.Name.TryToGetAlbumTitle()
            });
        }           
        
        var albumTag = tags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.Album);
        var artistTag = tags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.Artist);
        if (albumTag?.Value?.ToString().Nullify() == null || artistTag?.Value?.ToString().Nullify() == null)
        {
            return new OperationResult<Models.Song>($"Song [{fileSystemFileInfo.Name}] is invalid, missing Album and/or Artist tags.")
            {
                Data = new Models.Song
                {
                    CrcHash = string.Empty,
                    File = fileSystemFileInfo
                },
                Type = OperationResponseType.ValidationFailure
            };
        }         
        
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
        
        var metaTagsProcessorResult = await metaTagsProcessorPlugin.ProcessMetaTagAsync(directoryInfo, fileSystemFileInfo, tags, cancellationToken);
        if (!metaTagsProcessorResult.IsSuccess)
        {
            return new OperationResult<Models.Song>(metaTagsProcessorResult.Messages)
            {
                Errors = metaTagsProcessorResult.Errors,
                Data = new Models.Song
                {
                    CrcHash = string.Empty,
                    File = fileSystemFileInfo
                }
            };
        }

        var song = new Models.Song
        {
            CrcHash = Crc32.Calculate(new FileInfo(fileSystemFileInfo.FullName(directoryInfo))),
            File = fileSystemFileInfo,
            Images = images,
            Tags = metaTagsProcessorResult.Data.DistinctBy(x => x.Identifier).ToArray(),
            MediaAudios = mediaAudios.DistinctBy(x => x.Identifier).ToArray(),
            SortOrder = tags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.TrackNumber)?.Value as int? ?? 0
        };
        return new OperationResult<Models.Song>
        {
            Data = song
        };
    }

    public async Task<OperationResult<bool>> UpdateSongAsync(FileSystemDirectoryInfo directoryInfo, Models.Song song, CancellationToken cancellationToken = default)
    {
        var result = false;
        if (song.Tags?.Any() ?? false)
        {
            var songFileName = song.File.FullName(directoryInfo);
            if (!File.Exists(songFileName))
            {
                Log.Error(new Exception($"File not found [{songFileName}]"), "[{PlugInName}] UpdateSongAsync called File [{FileName}] does not exist", nameof(AtlMetaTag), songFileName);
                return new OperationResult<bool>
                {
                    Data = false
                };
            }

            try
            {
                var doDeleteComment = MelodeeConfiguration.GetValue<bool?>(SettingRegistry.ProcessingDoDeleteComments) ?? true;
                var fileAtl = new Track(songFileName)
                {
                    Album = song.AlbumTitle(),
                    AlbumArtist = song.AlbumArtist(),
                    Artist = song.SongArtist(),
                    Comment = doDeleteComment ? null : song.Comment(),
                    DiscNumber = song.MediaNumber(),
                    DiscTotal = song.MediaTotalNumber(),
                    Genre = song.Genre(),
                    OriginalReleaseDate = song.AlbumDateValue(),
                    Title = song.Title(),
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

                result = fileAtl.Save();
            }
            catch (Exception e)
            {
                Log.Error(e, "FileSystemFileInfo [{FileSystemFileInfo}]", directoryInfo);
            }
        }

        return new OperationResult<bool>
        {
            Data = result
        };
    }

    public async Task<OperationResult<bool>> RemoveImagesAsync(FileSystemDirectoryInfo directoryInfo, Models.Song song, CancellationToken cancellationToken = default)
    {
        var result = false;
        if (song.Tags?.Any() ?? false)
        {
            var songFileName = song.File.FullName(directoryInfo);
            using (Operation.At(LogEventLevel.Debug).Time("[{Plugin}] Removing images from [{FileName}]", DisplayName, songFileName))
            {
                try
                {
                    var fileAtl = new Track(songFileName);
                    fileAtl.EmbeddedPictures.Clear();
                    result = await fileAtl.SaveAsync();
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
                    if (result.All(x => x.Identifier != MetaTagIdentifier.AlbumDate))
                    {
                        result.Add(new MetaTag<object?>
                        {
                            Identifier = MetaTagIdentifier.AlbumDate,
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


    private static bool IsAtlTrackForAudioFile(Track track)
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
