using JetBrains.Annotations;
using Melodee.Common.Data;
using Melodee.Common.Extensions;
using Melodee.Common.MessageBus.Events;
using Melodee.Common.Models;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Services;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Rebus.Handlers;
using Rebus.Pipeline;
using Serilog;
using Serilog.Events;
using SerilogTimings;

namespace Melodee.Common.MessageBus.EventHandlers;

[UsedImplicitly]
public class ArtistRescanEventHandler(
    ILogger logger,
    IMessageContext messageContext,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    ArtistService artistService,
    LibraryService libraryService,
    AlbumRescanEventHandler albumRescanEventHandler,
    AlbumAddEventHandler albumAddEventHandler
) : IHandleMessages<ArtistRescanEvent>
{
    public async Task Handle(ArtistRescanEvent message)
    {
        var cancellationToken = messageContext.IncomingStepContext.Load<CancellationToken>();

        using (Operation
                   .At(LogEventLevel.Debug)
                   .Time("[{Name}] Handle [{id}]",
                       nameof(ArtistRescanEventHandler),
                       message.ToString()))
        {
            await using (var scopedContext = await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                var now = Instant.FromDateTimeUtc(DateTime.UtcNow);

                var dbArtist = await scopedContext
                    .Artists
                    .Include(x => x.Library)
                    .Include(x => x.Contributors)
                    .Include(x => x.Albums)
                    .FirstOrDefaultAsync(x => x.Id == message.ArtistId, cancellationToken)
                    .ConfigureAwait(false);

                if (dbArtist == null)
                {
                    logger.Warning("[{Name}] Unable to find artist with id [{ArtistId}] in database.",
                        nameof(ArtistRescanEventHandler), message.ArtistId);
                }
                else
                {
                    if (dbArtist.IsLocked || dbArtist.Library.IsLocked)
                    {
                        logger.Warning("[{Name}] Library or artist is locked. Skipping rescan request [{AlbumDir}].",
                            nameof(ArtistRescanEventHandler),
                            message.ArtistDirectory);
                        return;
                    }
                    
                    // Add log entry with number of albums found for artist in database
                    logger.Information("[{JobName}] Rescanning artist [{ArtistName}] with id [{ArtistId}] found [{AlbumCount}] albums in database.",
                        nameof(ArtistRescanEventHandler),
                        dbArtist.Name,
                        dbArtist.Id,
                        dbArtist.Albums.Count
                    );

                    // Get all albums from database for artist that are not locked and rescan them
                    await Parallel.ForEachAsync(dbArtist.Albums.Where(x => !x.IsLocked), cancellationToken,
                        async (album, _) =>
                        {
                            await albumRescanEventHandler.Handle(new AlbumRescanEvent(album.Id,
                                    Path.Combine(dbArtist.Library.Path, dbArtist.Directory, album.Directory), true))
                                .ConfigureAwait(false);
                        });

                    FileSystemDirectoryInfo? artistDirectory = null;
                    try
                    {
                        // Get all images from artist directory and update artist image count in database
                        artistDirectory = message.ArtistDirectory.ToFileSystemDirectoryInfo();
                        var imageCount = artistDirectory.AllFileImageTypeFileInfos().Count();
                        if (imageCount != dbArtist.ImageCount)
                        {
                            dbArtist.ImageCount = imageCount;
                            dbArtist.LastUpdatedAt = now;
                            await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error(e, "Error attempting to update artist image count.");
                    }

                    if (artistDirectory != null)
                    {
                        try
                        {
                            var artistDirectoryInfos = artistDirectory.AllDirectoryInfos().ToArray();
                            logger.Debug("[{JobName}] Processing artist directories found [{ArtistDirectoryInfosCount}].",
                                nameof(ArtistRescanEventHandler),
                                artistDirectoryInfos.Length
                            );
                            await Parallel.ForEachAsync(artistDirectoryInfos, cancellationToken,
                                async (artistDirectoryInfo, _) =>
                                {
                                    var dbArtistAlbum = dbArtist.Albums.FirstOrDefault(x => artistDirectoryInfo.IsSameDirectory(x.Directory));
                                    logger.Debug("[{JobName}] Processing artistDirectoryInfo [{ArtistDirectoryInfo}] dbArtistAlbum found [{IsbArtistAlbum}].",
                                        nameof(ArtistRescanEventHandler),
                                        artistDirectoryInfo,
                                        dbArtistAlbum != null
                                    );
                                    if (dbArtistAlbum == null)
                                    {
                                        await albumAddEventHandler
                                            .Handle(new AlbumAddEvent(dbArtist.Id, artistDirectoryInfo.FullName, true))
                                            .ConfigureAwait(false);
                                    }
                                });
                        }
                        catch (Exception e)
                        {
                            logger.Error(e, "Error processing artist directories for [{ArtistName}].", dbArtist.Name);
                        }
                    }
                    else
                    {
                        // Log warning if artist directory is null
                        logger.Warning("[{Name}] Artist directory is null for artist [{ArtistName}].",
                            nameof(ArtistRescanEventHandler), dbArtist.Name);
                    }

                    await libraryService.UpdateAggregatesAsync(dbArtist.Library.Id, cancellationToken)
                        .ConfigureAwait(false);
                    await artistService.ClearCacheAsync(dbArtist, cancellationToken);
                }
            }
        }
    }
}
