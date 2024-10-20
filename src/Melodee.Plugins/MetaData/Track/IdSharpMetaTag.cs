using IdSharp.Tagging.ID3v1;
using IdSharp.Tagging.ID3v2;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.Configuration;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Utility;
using Melodee.Plugins.Processor;
using Serilog;
using Serilog.Events;
using SerilogTimings;

namespace Melodee.Plugins.MetaData.Track;

public sealed class IdSharpMetaTag(IMetaTagsProcessorPlugin metaTagsProcessorPlugin, Configuration configuration) : MetaDataBase(configuration), ITrackPlugin
{
    private readonly IMetaTagsProcessorPlugin _metaTagsProcessorPlugin = metaTagsProcessorPlugin;

    public override string Id => "0AE16462-6924-496B-AC5E-C9CD70EA078D";

    public override string DisplayName => nameof(IdSharpMetaTag);

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

    public async Task<OperationResult<Common.Models.Track>> ProcessFileAsync(FileSystemDirectoryInfo directoryInfo, FileSystemFileInfo fileSystemInfo, CancellationToken cancellationToken = default)
    {
        using (Operation.At(LogEventLevel.Debug).Time("[{PluginName}] Processing [{fileSystemInfo}]", DisplayName, fileSystemInfo.Name))
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
                        var id3v1 = new ID3v1Tag(fullname);
                        tags.Add(new MetaTag<object?>
                        {
                            Identifier = MetaTagIdentifier.Album,
                            Value = id3v1.Album
                        });
                        tags.Add(new MetaTag<object?>
                        {
                            Identifier = MetaTagIdentifier.Artist,
                            Value = id3v1.Artist
                        });
                        tags.Add(new MetaTag<object?>
                        {
                            Identifier = MetaTagIdentifier.Artist,
                            Value = id3v1.Title.ToTitleCase(false)
                        });
                        tags.Add(new MetaTag<object?>
                        {
                            Identifier = MetaTagIdentifier.TrackNumber,
                            Value = SafeParser.ToNumber<short?>(id3v1.TrackNumber)
                        });
                        var date = SafeParser.ToDateTime(id3v1.Year);
                        tags.Add(new MetaTag<object?>
                        {
                            Identifier = MetaTagIdentifier.OrigReleaseYear,
                            Value = date?.Year
                        });
                    }

                    if (ID3v2Tag.DoesTagExist(fullname))
                    {
                        IID3v2Tag id3v2 = new ID3v2Tag(fullname);
                        tags.Add(new MetaTag<object?>
                        {
                            Identifier = MetaTagIdentifier.Album,
                            Value = id3v2.Album
                        });
                        tags.Add(new MetaTag<object?>
                        {
                            Identifier = MetaTagIdentifier.Artist,
                            Value = id3v2.Artist
                        });
                        tags.Add(new MetaTag<object?>
                        {
                            Identifier = MetaTagIdentifier.Artist,
                            Value = id3v2.Title.ToTitleCase(false)
                        });
                        tags.Add(new MetaTag<object?>
                        {
                            Identifier = MetaTagIdentifier.TrackNumber,
                            Value = SafeParser.ToNumber<short?>(id3v2.TrackNumber)
                        });
                        var date = SafeParser.ToDateTime(id3v2.Year);
                        tags.Add(new MetaTag<object?>
                        {
                            Identifier = MetaTagIdentifier.OrigReleaseYear,
                            Value = date?.Year
                        });
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e, "fileSystemInfo [{fileSystemInfo}]", fileSystemInfo);
            }

            // Ensure that OrigReleaseYear exists and if not add with invalid date (will get set later by MetaTagProcessor.)
            if (tags.All(x => x.Identifier != MetaTagIdentifier.OrigReleaseYear))
            {
                tags.Add(new MetaTag<object?>
                {
                    Identifier = MetaTagIdentifier.OrigReleaseYear,
                    Value = 0
                });
            }

            var metaTagsProcessorResult = await _metaTagsProcessorPlugin.ProcessMetaTagAsync(directoryInfo, fileSystemInfo, tags, cancellationToken);
            if (!metaTagsProcessorResult.IsSuccess)
            {
                return new OperationResult<Common.Models.Track>(metaTagsProcessorResult.Messages)
                {
                    Errors = metaTagsProcessorResult.Errors,
                    Data = new Common.Models.Track
                    {
                        CrcHash = string.Empty,
                        File = fileSystemInfo
                    }
                };
            }

            var track = new Common.Models.Track
            {
                CrcHash = CRC32.Calculate(new FileInfo(fileSystemInfo.FullName(directoryInfo))),
                File = fileSystemInfo,
                Images = images,
                Tags = metaTagsProcessorResult.Data,
                MediaAudios = mediaAudios,
                SortOrder = tags.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.TrackNumber)?.Value as int? ?? 0
            };
            return new OperationResult<Common.Models.Track>
            {
                Data = track
            };
        }
    }

    public Task<OperationResult<bool>> UpdateTrackAsync(FileSystemDirectoryInfo directoryInfo, Common.Models.Track track, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
