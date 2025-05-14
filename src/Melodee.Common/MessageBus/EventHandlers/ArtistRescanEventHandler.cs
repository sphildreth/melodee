using Melodee.Common.Data;
using Melodee.Common.Extensions;
using Melodee.Common.MessageBus.Events;
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

        using (Operation.At(LogEventLevel.Debug)
                   .Time("[{Name}] Handle [{id}]", nameof(ArtistRescanEventHandler), message.ToString()))
        {
            await using (var scopedContext =
                         await contextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
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

                    await Parallel.ForEachAsync(dbArtist.Albums.Where(x => !x.IsLocked), cancellationToken,
                        async (album, tt) =>
                        {
                            await albumRescanEventHandler.Handle(new AlbumRescanEvent(album.Id,
                                    Path.Combine(dbArtist.Library.Path, dbArtist.Directory, album.Directory), true))
                                .ConfigureAwait(false);
                        });

                    var artistDirectory = message.ArtistDirectory.ToFileSystemDirectoryInfo();
                    var imageCount = artistDirectory.AllFileImageTypeFileInfos().Count();
                    if (imageCount != dbArtist.ImageCount)
                    {
                        dbArtist.ImageCount = imageCount;
                        dbArtist.LastUpdatedAt = now;
                        await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                    }

                    var artistDirectoryInfos = artistDirectory.AllDirectoryInfos().ToArray();
                    logger.Debug("[{JobName}] Processing artist directories found [{ArtistDirectoryInfosCount}].",
                        nameof(ArtistRescanEventHandler),
                        artistDirectoryInfos.Length
                    );                    
                    await Parallel.ForEachAsync(artistDirectoryInfos, cancellationToken,
                        async (artistDirectoryInfo, tt) =>
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

                    await libraryService.UpdateAggregatesAsync(dbArtist.Library.Id, cancellationToken)
                        .ConfigureAwait(false);
                    await artistService.ClearCacheAsync(dbArtist, cancellationToken);
                }
            }
        }
    }
}
