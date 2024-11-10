using Ardalis.GuardClauses;
using Dapper;
using Melodee.Common.Constants;
using Melodee.Common.Data;
using Melodee.Common.Data.Models;
using Melodee.Common.Data.Models.Extensions;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models.Extensions;
using Melodee.Common.Utility;
using Melodee.Services.Interfaces;
using Melodee.Services.Scanning;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using Serilog;
using SmartFormat;
using MelodeeModels = Melodee.Common.Models;

namespace Melodee.Services;

public class LibraryService(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory,
    SettingService settingService,
    ArtistService artistService,
    AlbumService albumService)
    : ServiceBase(logger, cacheManager, contextFactory)
{
    private const string CacheKeyDetailByApiKeyTemplate = "urn:library:apikey:{0}";
    private const string CacheKeyDetailLibraryByType = "urn:library_by_type:{0}";
    private const string CacheKeyDetailTemplate = "urn:library:{0}";
    
    public async Task<MelodeeModels.OperationResult<Library>> GetInboundLibraryAsync(CancellationToken cancellationToken = default)
    {
        const int libraryType = (int)LibraryType.Inbound;
        var result = await CacheManager.GetAsync(CacheKeyDetailLibraryByType.FormatSmart(libraryType), async () =>
        {
            var library = await LibraryByType(libraryType, cancellationToken);
            if (library == null)
            {
                throw new Exception("Inbound library not found. A 'Library' record must be setup for a inbound library.");
            }
            return library;
        }, cancellationToken);        
        return new MelodeeModels.OperationResult<Library>
        {
            Data = result
        }; 
    }
    
    public Task<MelodeeModels.OperationResult<Library?>> GetByApiKeyAsync(Guid apiKey, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(_ => apiKey == Guid.Empty, apiKey, nameof(apiKey));

        return CacheManager.GetAsync(CacheKeyDetailByApiKeyTemplate.FormatSmart(apiKey), async () =>
        {
            await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                var libraryId = await scopedContext
                    .Libraries
                    .AsNoTracking()
                    .Where(x => x.ApiKey == apiKey)
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync(cancellationToken)
                    .ConfigureAwait(false);
                return await GetAsync(libraryId, cancellationToken).ConfigureAwait(false);
            }
        }, cancellationToken);
    }
    
    public async Task<MelodeeModels.OperationResult<Library?>> GetAsync(int id, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, id, nameof(id));

        var result = await CacheManager.GetAsync(CacheKeyDetailTemplate.FormatSmart(id), async () =>
        {
            await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
            {
                return await scopedContext
                    .Libraries
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                    .ConfigureAwait(false);
            }
        }, cancellationToken);
        return new MelodeeModels.OperationResult<Library?>
        {
            Data = result
        };
    }    
    
    public async Task<MelodeeModels.OperationResult<Library>> GetLibraryAsync(CancellationToken cancellationToken = default)
    {
        const int libraryType = (int)LibraryType.Library;
        var result = await CacheManager.GetAsync(CacheKeyDetailLibraryByType.FormatSmart(libraryType), async () =>
        {
            var library = await LibraryByType(libraryType, cancellationToken);
            if (library == null)
            {
                throw new Exception("Library not found. A 'Library' record must be setup for a library.");
            }
            return library;
        }, cancellationToken);        
        return new MelodeeModels.OperationResult<Library>
        {
            Data = result
        };         
    }

    private async Task<Library> LibraryByType(int type, CancellationToken cancellationToken = default)
    {
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var dbConn = scopedContext.Database.GetDbConnection();
            var sql = $"SELECT * FROM \"Libraries\" WHERE \"Type\" = {type};";
            return await dbConn
                .QuerySingleAsync<Library>(sql)
                .ConfigureAwait(false);         
        }
    }

    public async Task<MelodeeModels.OperationResult<Library?>> PurgeLibraryAsync(int libraryId, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, libraryId, nameof(libraryId));

        var libraryType = (int)LibraryType.Library;
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var dbLibrary = await scopedContext
                .Libraries
                .FirstOrDefaultAsync(x => x.Id == libraryId, cancellationToken)
                .ConfigureAwait(false);
            if (dbLibrary == null)
            {
                return new MelodeeModels.OperationResult<Library?>("Invalid Library Id")
                {
                    Data = null,
                    Type = MelodeeModels.OperationResponseType.Error,
                };
            }
            libraryType = dbLibrary.Type;
            dbLibrary.PurgePath();
            
            await scopedContext
                .LibraryScanHistories
                .Where(x => x.LibraryId == libraryId)
                .ExecuteDeleteAsync(cancellationToken)
                .ConfigureAwait(false);

            dbLibrary.LastScanAt = null;
            dbLibrary.LastUpdatedAt = Instant.FromDateTimeUtc(DateTime.UtcNow);
            await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            ClearCache();
        }
        return new MelodeeModels.OperationResult<Library?>
        {
            Data = await LibraryByType(libraryType, cancellationToken).ConfigureAwait(false)
        };        
    }
    
    public async Task<MelodeeModels.OperationResult<Library>> GetStagingLibraryAsync(CancellationToken cancellationToken = default)
    {
        const int libraryType = (int)LibraryType.Staging;
        var result = await CacheManager.GetAsync(CacheKeyDetailLibraryByType.FormatSmart(libraryType), async () =>
        {
            var library = await LibraryByType(libraryType, cancellationToken);
            if (library == null)
            {
                throw new Exception("Staging library not found. A 'Library' record must be setup for a staging library.");
            }
            return library;
        }, cancellationToken);        
        return new MelodeeModels.OperationResult<Library>
        {
            Data = result
        };          
    }

    public async Task<MelodeeModels.PagedResult<Library>> ListAsync(MelodeeModels.PagedRequest pagedRequest, CancellationToken cancellationToken = default)
    {
        int librariesCount = 0;
        Library[] libraries = [];
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            try
            {
                var orderBy = pagedRequest.OrderByValue();                
                var dbConn = scopedContext.Database.GetDbConnection();
                var countSqlParts = pagedRequest.FilterByParts("SELECT COUNT(*) FROM \"Libraries\"");
                librariesCount = await dbConn
                    .QuerySingleAsync<int>(countSqlParts.Item1, countSqlParts.Item2)
                    .ConfigureAwait(false);
                if (!pagedRequest.IsTotalCountOnlyRequest)
                {
                    var listSqlParts = pagedRequest.FilterByParts("SELECT * FROM \"Libraries\"");
                    var listSql = $"{listSqlParts.Item1} ORDER BY {orderBy} OFFSET {pagedRequest.SkipValue} ROWS FETCH NEXT {pagedRequest.TakeValue} ROWS ONLY;";
                    if (dbConn is SqliteConnection)
                    {
                        listSql = $"{listSqlParts.Item1 } ORDER BY {orderBy} LIMIT {pagedRequest.TakeValue} OFFSET {pagedRequest.SkipValue};";
                    }
                    libraries = (await dbConn
                        .QueryAsync<Library>(listSql, listSqlParts.Item2)
                        .ConfigureAwait(false)).ToArray();
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to get libraries from database");
            }
        }

        return new MelodeeModels.PagedResult<Library>
        {
            TotalCount = librariesCount,
            TotalPages = pagedRequest.TotalPages(librariesCount),
            Data = libraries
        };
    }

    private void ClearCache()
    {
        CacheManager.Remove(CacheKeyDetailLibraryByType.FormatSmart((int)LibraryType.Inbound));
        CacheManager.Remove(CacheKeyDetailLibraryByType.FormatSmart((int)LibraryType.Library));
        CacheManager.Remove(CacheKeyDetailLibraryByType.FormatSmart((int)LibraryType.Staging));
    }

    public async Task<MelodeeModels.OperationResult<bool>> MoveAlbumToLibrary(Library library, MelodeeModels.Album album, CancellationToken cancellationToken = default)
    {
        // TODO Musicbrainz Db for metadata update job
        
        var configuration = await settingService.GetMelodeeConfigurationAsync(cancellationToken);
        
        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
            var dbArtistResult = await artistService.GetByMediaUniqueId(album.ArtistUniqueId(), cancellationToken).ConfigureAwait(false);
            var dbArtist = dbArtistResult.Data;
            if (!dbArtistResult.IsSuccess)
            {
                dbArtist = new Artist
                {
                    AlternateNames = album.Artist()?.ToAlphanumericName(),
                    AlbumCount = 1,
                    CreatedAt = now,
                    MediaUniqueId = album.ArtistUniqueId(),
                    MetaDataStatus = (int)MetaDataModelStatus.ReadyToProcess,
                    Name = album.Artist() ?? throw new Exception("Album artist is required."),
                    SongCount = album.Songs?.Count() ?? 0,
                    SortName = album.Artist() ?? throw new Exception("Album artist is required.")
                };
                await scopedContext.Artists.AddAsync(dbArtist, cancellationToken).ConfigureAwait(false);
            }
            var dbAlbumResult = await albumService.GetByMediaUniqueId(album.UniqueId, cancellationToken).ConfigureAwait(false);
            if (!dbAlbumResult.IsSuccess)
            {
                var dbAlbum = new Album
                {
                    AlbumStatus = (short)album.Status,
                    AlbumType = (int)AlbumType.Album,
                    ArtistId = dbArtist!.Id,
                    CreatedAt = now,
                    DiscCount = album.MediaCountValue(),
                    Duration = album.TotalDuration(),
                    Genres = album.Genre() == null ? null : string.Join('|', album.Genre()!.Split('/')),
                    IsCompilation = album.IsVariousArtistTypeAlbum(),
                    MediaUniqueId = album.UniqueId,
                    MetaDataStatus = (int)MetaDataModelStatus.ReadyToProcess,
                    Name = album.AlbumTitle() ?? throw new Exception("Album title is required."),
                    OriginalReleaseDate = album.OriginalAlbumYear() == null ? null : new LocalDate(album.OriginalAlbumYear()!.Value, 1, 1),                    
                    ReleaseDate = new LocalDate(album.AlbumYear() ?? throw new Exception("Album year is required."), 1, 1),
                    SongCount = SafeParser.ToNumber<short>(album.Songs?.Count() ?? 0),
                    SortName = album.AlbumTitle() ?? throw new Exception("Album title is required.")
                };
                await scopedContext.Albums.AddAsync(dbAlbum, cancellationToken).ConfigureAwait(false);
                var dbAlbumDiscsToAdd = new List<AlbumDisc>();
                var mediaCountValue = album.MediaCountValue() < 1 ? 1 : album.MediaCountValue();
                for (short i = 0; i < mediaCountValue; i++)
                {
                    dbAlbumDiscsToAdd.Add(new AlbumDisc
                    {
                        AlbumId = dbAlbum.Id,
                        DiscNumber = i,
                        SongCount = SafeParser.ToNumber<short>(album.Songs?.Where(x => x.MediaNumber() == i).Count() ?? 0)
                    });
                }
                await scopedContext.AlbumDiscs.AddRangeAsync(dbAlbumDiscsToAdd, cancellationToken).ConfigureAwait(false);
                var dbSongsToAdd = new List<Song>();
                foreach (var song in album.Songs!)
                {
                    var songFileInfo = song.File.ToFileInfo(album.Directory!);
                    dbSongsToAdd.Add(new Song
                    {
                        AlbumDiscId = dbAlbumDiscsToAdd.First(x => x.DiscNumber == song.MediaNumber()).Id,
                        AlternateNames = song.Title()?.ToAlphanumericName(),
                        BitDepth = SafeParser.ToNumber<int>(song.MediaAudios?.FirstOrDefault(x => x.Identifier == MediaAudioIdentifier.BitDepth)?.Value),
                        BitRate = SafeParser.ToNumber<int>(song.MediaAudios?.FirstOrDefault(x => x.Identifier == MediaAudioIdentifier.BitRate)?.Value),
                        BPM = song.MetaTagValue<int>(MetaTagIdentifier.Bpm),
                        ChannelCount = SafeParser.ToNumber<int?>(song.MediaAudios?.FirstOrDefault(x => x.Identifier == MediaAudioIdentifier.Channels)?.Value),
                        CreatedAt = now,
                        Duration = song.Duration() ?? throw new Exception("Song duration is required."),
                        FileHash = Crc32.Calculate(songFileInfo),
                        FileName = songFileInfo.Name,
                        FilePath = songFileInfo.DirectoryName ?? throw new Exception("Song file path is required."),
                        FileSize = songFileInfo.Length,
                        Lyrics = song.MetaTagValue<string>(MetaTagIdentifier.UnsynchronisedLyrics) ?? song.MetaTagValue<string>(MetaTagIdentifier.SynchronisedLyrics),
                        MediaUniqueId = song.UniqueId,
                        Name = song.Title() ?? throw new Exception("Song title is required."),
                        PartTitles = song.MetaTagValue<string>(MetaTagIdentifier.SubTitle),
                        SamplingRate = SafeParser.ToNumber<int>(song.MediaAudios?.FirstOrDefault(x => x.Identifier == MediaAudioIdentifier.SampleRate)?.Value),
                        SortName = song.MetaTagValue<string>(MetaTagIdentifier.SortTitle) ?? song.Title() ?? throw new Exception("Song title is required."),
                        SortOrder = song.SortOrder,
                        Title = song.Title() ?? throw new Exception("Song title is required."),
                        SongNumber = song.SongNumber()
                    });
                }
                await scopedContext.Songs.AddRangeAsync(dbSongsToAdd, cancellationToken).ConfigureAwait(false);
                
                var dbContributorsToAdd = new List<Contributor>();
                foreach (var song in album.Songs!)
                {
                    var dbSongId = dbSongsToAdd.First(x => x.MediaUniqueId == song.UniqueId).Id;
                    
                    var songArtist = song.SongArtist();
                    if (songArtist.Nullify() != null)
                    {

                        dbContributorsToAdd.Add(new Contributor
                        {
                            CreatedAt = now,
                            Role = "Track Artist",
                            ArtistId = 0,
                            SongId = dbSongId,
                            AlbumId = dbAlbum.Id,
                        });
                    }
                    
                    // Arranger (TIPL:arranger)

                    var lyricist = song.MetaTagValue<string?>(MetaTagIdentifier.Lyricist);
                    if (lyricist.Nullify() != null)
                    {
                        dbContributorsToAdd.Add(new Contributor
                        {
                            CreatedAt = now,
                            Role = "Author/Writer/Lyricist",
                            ArtistId = 0,
                            SongId = dbSongId,
                            AlbumId = dbAlbum.Id,
                        });
                    }                    
                    // Composer (TCOM)
                    var composer = song.MetaTagValue<string?>(MetaTagIdentifier.Lyricist);
                    if (composer.Nullify() != null)
                    {
                        dbContributorsToAdd.Add(new Contributor
                        {
                            CreatedAt = now,
                            Role = "Composer",
                            ArtistId = 0,
                            SongId = dbSongId,
                            AlbumId = dbAlbum.Id,
                        });
                    }   
                    
                    // Conductor (TPE3)
                    // Engineer (TIPL:engineer)
                    // Involved Person (IPL, IPLS, TIPL)
                    // Mix-DJ (TIPL:DJ-mix)
                    // Mix Engineer (TPIL:mix)
                    // Musician Credit (TMCL)
                    // Original Artist (TOPE)
                    // Original Lyricist (TOLY)
                    // Performer (TMCL:<instrument>)
                    // Producer (TIPL:producer)
                    // Publisher (TPUB)
                    // Remixed By (TPE4)                    
                }                
                await scopedContext.Contributors.AddRangeAsync(dbContributorsToAdd, cancellationToken).ConfigureAwait(false);
                
                var saveResult = await scopedContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                if (saveResult < 1)
                {
                    return new MelodeeModels.OperationResult<bool>
                    {
                        Data = false,
                        Errors = [new Exception("Failed to save album to database.")]
                    };
                }
            }

            var albumDirectory = album.AlbumDirectoryName(configuration.Configuration);
            if (!Directory.Exists(albumDirectory))
            {
                Directory.CreateDirectory(albumDirectory);
            }
            else
            {
                // if data album exists for model album if so determine which is better quality
                
            }
            var doMove = SafeParser.ToBoolean(configuration.Configuration[SettingRegistry.ProcessingMoveMelodeeJsonDataFileToLibrary]);
            MediaEditService.MoveDirectory(album.Directory!.FullName(), albumDirectory, doMove ? null : MelodeeModels.Album.JsonFileName);

            return new MelodeeModels.OperationResult<bool>
            {
                Data = true
            };
        }
    }
    
    public async Task<MelodeeModels.OperationResult<LibraryScanHistory?>> CreateLibraryScanHistory(Library library, LibraryScanHistory libraryScanHistory, CancellationToken cancellationToken = default)
    {
        Guard.Against.Expression(x => x < 1, library.Id, nameof(library));

        await using (var scopedContext = await ContextFactory.CreateDbContextAsync(cancellationToken).ConfigureAwait(false))
        {
            var dbLibrary = await scopedContext
                .Libraries
                .FirstOrDefaultAsync(x => x.Id == library.Id, cancellationToken)
                .ConfigureAwait(false);
            if (dbLibrary == null)
            {
                return new MelodeeModels.OperationResult<LibraryScanHistory?>("Invalid Library Id")
                {
                    Data = null,
                    Type = MelodeeModels.OperationResponseType.Error,
                };
            }

            var now = Instant.FromDateTimeUtc(DateTime.UtcNow);
            var newLibraryScanHistory = new LibraryScanHistory
            {
                LibraryId = library.Id,
                CreatedAt = now,
                DurationInMs = libraryScanHistory.DurationInMs,
                ForAlbumId = libraryScanHistory.ForAlbumId,
                ForArtistId = libraryScanHistory.ForArtistId,
                FoundAlbumsCount = libraryScanHistory.FoundAlbumsCount,
                FoundArtistsCount = libraryScanHistory.FoundArtistsCount,
                FoundSongsCount = libraryScanHistory.FoundSongsCount
            };
            scopedContext.LibraryScanHistories.Add(newLibraryScanHistory);
            if (await scopedContext
                    .SaveChangesAsync(cancellationToken)
                    .ConfigureAwait(false) < 1)
            {
                return new MelodeeModels.OperationResult<LibraryScanHistory?>
                {
                    Data = null,
                    Type = MelodeeModels.OperationResponseType.Error
                };
            }
            dbLibrary.LastScanAt = now;
            if (await scopedContext
                    .SaveChangesAsync(cancellationToken)
                    .ConfigureAwait(false) < 1)
            {
                return new MelodeeModels.OperationResult<LibraryScanHistory?>
                {
                    Data = null,
                    Type = MelodeeModels.OperationResponseType.Error
                };
            }
            ClearCache();
            return new MelodeeModels.OperationResult<LibraryScanHistory?>
            {
                Data = newLibraryScanHistory
            };      
        }
    }
}
