using System.Diagnostics;
using ATL;
using IdSharp.Tagging.ID3v1;
using IdSharp.Tagging.ID3v2;
using Melodee.Common.Configuration;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Plugins.Processor;
using Melodee.Common.Utility;
using Serilog;
using Serilog.Events;
using SerilogTimings;

namespace Melodee.Common.Plugins.MetaData.Song;

public sealed class IdSharpMetaTag(
    IMetaTagsProcessorPlugin metaTagsProcessorPlugin,
    IMelodeeConfiguration configuration) : MetaDataBase(configuration), ISongPlugin
{
    private readonly IMetaTagsProcessorPlugin _metaTagsProcessorPlugin = metaTagsProcessorPlugin;

    public override string Id => "0AE16462-6924-496B-AC5E-C9CD70EA078D";

    public override string DisplayName => nameof(IdSharpMetaTag);

    public override bool IsEnabled { get; set; } = true;

    public override int SortOrder { get; } = 1;

    public override bool DoesHandleFile(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemInfo)
    {
        if (!IsEnabled || !fileSystemInfo.Exists(directoryInfo))
        {
            return false;
        }

        return FileHelper.IsFileMediaType(fileSystemInfo.Extension(directoryInfo));
    }

    public async Task<OperationResult<Models.Song>> ProcessFileAsync(FileSystemDirectoryInfo directoryInfo,
        FileSystemFileInfo fileSystemInfo, CancellationToken cancellationToken = default)
    {
        using (Operation.At(LogEventLevel.Debug)
                   .Time("[{PluginName}] Processing [{fileSystemInfo}]", DisplayName, fileSystemInfo.Name))
        {
            var tags = new List<MetaTag<object?>>();
            var mediaAudios = new List<MediaAudio<object?>>();
            var images = new List<ImageInfo>();

            try
            {
                if (fileSystemInfo.Exists(directoryInfo))
                {
                    var fullname = fileSystemInfo.FullName(directoryInfo);
                    if (ID3v1Tag.DoesTagExist(fullname))
                    {
                        var id3V1 = new ID3v1Tag(fullname);
                        if (id3V1.Album.Nullify() != null)
                        {
                            tags.Add(new MetaTag<object?>
                            {
                                Identifier = MetaTagIdentifier.Album,
                                Value = id3V1.Album
                            });
                        }

                        if (id3V1.Artist.Nullify() != null)
                        {
                            tags.Add(new MetaTag<object?>
                            {
                                Identifier = MetaTagIdentifier.Artist,
                                Value = id3V1.Artist
                            });
                        }

                        if (id3V1.Title.Nullify() != null)
                        {
                            tags.Add(new MetaTag<object?>
                            {
                                Identifier = MetaTagIdentifier.Title,
                                Value = id3V1.Title.ToTitleCase(false)
                            });
                        }

                        if (id3V1.TrackNumber > 0)
                        {
                            tags.Add(new MetaTag<object?>
                            {
                                Identifier = MetaTagIdentifier.TrackNumber,
                                Value = SafeParser.ToNumber<short?>(id3V1.TrackNumber)
                            });
                        }

                        var date = SafeParser.ToDateTime(id3V1.Year);
                        if (date != null)
                        {
                            tags.Add(new MetaTag<object?>
                            {
                                Identifier = MetaTagIdentifier.OrigAlbumYear,
                                Value = date?.Year
                            });
                        }
                    }

                    if (ID3v2Tag.DoesTagExist(fullname))
                    {
                        IID3v2Tag id3V2 = new ID3v2Tag(fullname);
                        if (tags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.Album) == null)
                        {
                            tags.Add(new MetaTag<object?>
                            {
                                Identifier = MetaTagIdentifier.Album,
                                Value = id3V2.Album
                            });
                        }

                        if (tags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.Artist) == null)
                        {
                            tags.Add(new MetaTag<object?>
                            {
                                Identifier = MetaTagIdentifier.Artist,
                                Value = id3V2.AlbumArtist ?? id3V2.Artist
                            });
                        }

                        if (tags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.Title) == null)
                        {
                            tags.Add(new MetaTag<object?>
                            {
                                Identifier = MetaTagIdentifier.Title,
                                Value = id3V2.Title.ToTitleCase(false)
                            });
                        }

                        if (tags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.TrackNumber) == null)
                        {
                            tags.Add(new MetaTag<object?>
                            {
                                Identifier = MetaTagIdentifier.TrackNumber,
                                Value = SafeParser.ToNumber<short?>(id3V2.TrackNumber)
                            });
                        }

                        if (tags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.OrigAlbumYear) == null)
                        {
                            var date = SafeParser.ToDateTime(id3V2.Year);
                            tags.Add(new MetaTag<object?>
                            {
                                Identifier = MetaTagIdentifier.OrigAlbumYear,
                                Value = date?.Year
                            });
                        }

                        if (mediaAudios.FirstOrDefault(x => x.Identifier == MediaAudioIdentifier.DurationMs) == null)
                        {
                            var duration = SafeParser.ToNumber<double?>(id3V2.LengthMilliseconds);
                            if ((duration ?? 0) < 1)
                            {
                                try
                                {
                                    var atlTag = new Track(fullname);
                                    duration = atlTag.DurationMs;
                                }
                                catch
                                {
                                    // Dont do anything at this point with exception.
                                }
                            }

                            mediaAudios.Add(new MediaAudio<object?>
                            {
                                Identifier = MediaAudioIdentifier.DurationMs,
                                Value = duration
                            });
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "fileSystemInfo [{fileSystemInfo}]", fileSystemInfo);
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

            var metaTagsProcessorResult =
                await _metaTagsProcessorPlugin.ProcessMetaTagAsync(directoryInfo, fileSystemInfo, tags,
                    cancellationToken);
            if (!metaTagsProcessorResult.IsSuccess)
            {
                return new OperationResult<Models.Song>(metaTagsProcessorResult.Messages)
                {
                    Errors = metaTagsProcessorResult.Errors,
                    Data = new Models.Song
                    {
                        CrcHash = string.Empty,
                        File = fileSystemInfo
                    }
                };
            }

            var song = new Models.Song
            {
                CrcHash = Crc32.Calculate(new FileInfo(fileSystemInfo.FullName(directoryInfo))),
                File = fileSystemInfo,
                Images = images,
                Tags = metaTagsProcessorResult.Data,
                MediaAudios = mediaAudios,
                SortOrder = tags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.TrackNumber)?.Value as int? ?? 0
            };
            if (!song.IsValid(Configuration))
            {
                Trace.WriteLine("Song is invalid");
            }

            return new OperationResult<Models.Song>
            {
                Data = song
            };
        }
    }

    public Task<OperationResult<bool>> UpdateSongAsync(FileSystemDirectoryInfo directoryInfo, Models.Song song,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
