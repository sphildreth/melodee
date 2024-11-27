using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Utility;
using ServiceStack.OrmLite;
using dbModels = Melodee.Services.MusicBrainz.Data.Models;

namespace Melodee.Services.MusicBrainz.Data;

public class MusicBrainzRepository
{
    public async Task<OperationResult<bool>> ImportData(CancellationToken cancellationToken = default)
    {
        var batchSize = 10000;
        var maxToProcess = int.MaxValue;
        var storagePath = "/mnt/incoming/melodee_test/search-engine-storage/musicbrainz/";

        var artistCountInserted = 0;
        var releaseCountInserted = 0;
        var artistCreditInserted = 0;
        var artistCreditNameInserted = 0;
        var artistAliasesInserted = 0;

        var dbFactory = new OrmLiteConnectionFactory(Path.Combine(storagePath, "musicbrainz.db"), SqliteDialect.Provider);

        using (var db = await dbFactory.OpenDbConnectionAsync(cancellationToken))
        {
            db.CreateTable<dbModels.Artist>();
            var artistsToAdd = new List<dbModels.Artist>();

            await foreach (var lineFromFile in File.ReadLinesAsync(Path.Combine(storagePath, "staging/artist"), cancellationToken))
            {
                var parts = lineFromFile.Split('\t');
                var artist = new dbModels.Artist
                {
                    Id = SafeParser.ToNumber<long>(parts[0]),
                    MusicBrainzId = SafeParser.ToGuid(parts[1]) ?? Guid.Empty,
                    Name = parts[2],
                    NameNormalized = parts[2].ToNormalizedString() ?? parts[2],
                    SortName = parts[3].CleanString(doPutTheAtEnd: true) ?? parts[2]
                };
                if (artist.IsValid)
                {
                    artistsToAdd.Add(artist);
                    if (artistsToAdd.Count >= batchSize)
                    {
                        await db.InsertAllAsync(artistsToAdd, cancellationToken);
                        artistCountInserted += artistsToAdd.Count;
                        Console.WriteLine($"Inserted {artistCountInserted} artists");
                        artistsToAdd.Clear();
                    }
                }
                
                if (artistCountInserted > maxToProcess)
                {
                    break;
                }                
            }


            db.CreateTable<dbModels.ArtistCredit>();
             var artistCreditsToAdd = new List<dbModels.ArtistCredit>();
            await foreach (var lineFromFile in File.ReadLinesAsync(Path.Combine(storagePath, "staging/artist_credit"), cancellationToken))
            {
                var parts = lineFromFile.Split('\t');
                
                var artistCredit = new dbModels.ArtistCredit
                {
                    Id = SafeParser.ToNumber<long>(parts[0]),
                    ArtistCount = SafeParser.ToNumber<int>(parts[2]),
                    Name = parts[1],
                    RefCount = SafeParser.ToNumber<int>(parts[3]),
                    Gid = SafeParser.ToGuid(parts[6]) ?? Guid.Empty,                             
                };
                
                if (artistCredit.IsValid)
                {
                    artistCreditsToAdd.Add(artistCredit);
                    if (artistCreditsToAdd.Count >= batchSize)
                    {
                        await db.InsertAllAsync(artistCreditsToAdd, cancellationToken);
                        artistCreditInserted += artistCreditsToAdd.Count;
                        Console.WriteLine($"Inserted {artistCreditInserted} artist credits");
                        artistCreditsToAdd.Clear();
                    }
                }
                
                if (artistCreditInserted > maxToProcess)
                {
                    break;
                }
            }

            db.CreateTable<dbModels.ArtistCreditName>();
            var artistCreditNamesToAdd = new List<dbModels.ArtistCreditName>();
            await foreach (var lineFromFile in File.ReadLinesAsync(Path.Combine(storagePath, "staging/artist_credit_name"), cancellationToken))
            {
                var parts = lineFromFile.Split('\t');
                
                var artistCreditName = new dbModels.ArtistCreditName
                {
                    ArtistCreditId = SafeParser.ToNumber<long>(parts[0]),
                    Position = SafeParser.ToNumber<int>(parts[1]),
                    ArtistId = SafeParser.ToNumber<long>(parts[2]),
                    Name = parts[3]
                };
                if (artistCreditName.IsValid)
                {
                    artistCreditNamesToAdd.Add(artistCreditName);
                    if (artistCreditNamesToAdd.Count >= batchSize)
                    {
                        await db.InsertAllAsync(artistCreditNamesToAdd, cancellationToken);
                        artistCreditNameInserted += artistCreditNamesToAdd.Count;
                        Console.WriteLine($"Inserted {artistCreditNameInserted} artist credit names");
                        artistCreditNamesToAdd.Clear();
                    }
                }
                if (artistCreditNameInserted > maxToProcess)
                {
                    break;
                }  
            }

            
            db.CreateTable<dbModels.ArtistAlias>();
            var artistAliasToAdd = new List<dbModels.ArtistAlias>();
            await foreach (var lineFromFile in File.ReadLinesAsync(Path.Combine(storagePath, "staging/artist_alias"), cancellationToken))
            {
                var parts = lineFromFile.Split('\t');

                var artistAlias = new dbModels.ArtistAlias
                {
                    Id = SafeParser.ToNumber<long>(parts[0]),
                    ArtistId = SafeParser.ToNumber<long>(parts[1]),
                    Name = parts[2],
                    Type = SafeParser.ToNumber<int>(parts[6]),
                    SortName = parts[7]
                };
                
                if (artistAlias.IsValid)
                {
                    artistAliasToAdd.Add(artistAlias);
                    if (artistAliasToAdd.Count >= batchSize)
                    {
                        await db.InsertAllAsync(artistAliasToAdd, cancellationToken);
                        artistAliasesInserted += artistAliasToAdd.Count;
                        Console.WriteLine($"Inserted {artistAliasesInserted} artist aliases");
                        artistAliasToAdd.Clear();
                    }
                }
                
                if (artistAliasesInserted > maxToProcess)
                {
                    break;
                }                
            }

            db.CreateTable<dbModels.Release>();
            var releasesToAdd = new List<dbModels.Release>();
            await foreach (var lineFromFile in File.ReadLinesAsync(Path.Combine(storagePath, "staging/release"), cancellationToken))
            {
                var parts = lineFromFile.Split('\t');

                var release = new dbModels.Release
                {
                    ArtistCreditId = SafeParser.ToNumber<long>(parts[3]),
                    Id = SafeParser.ToNumber<long>(parts[0]),
                    MusicBrainzId = SafeParser.ToGuid(parts[1]) ?? Guid.Empty,
                    Name = parts[2],
                    NameNormalized = parts[2].ToNormalizedString() ?? parts[2],
                    SortName = parts[2].CleanString(doPutTheAtEnd: true)
                };
                
                if (release.IsValid)
                {
                    releasesToAdd.Add(release);
                    if (releasesToAdd.Count >= batchSize)
                    {
                        await db.InsertAllAsync(releasesToAdd, cancellationToken);
                        releaseCountInserted += releasesToAdd.Count;
                        Console.WriteLine($"Inserted {releaseCountInserted} releases");
                        releasesToAdd.Clear();
                    }
                }
                
                if (releaseCountInserted > maxToProcess)
                {
                    break;
                }                
            }
          
            // select r.Name as "AlbumName", r.NameNormalized as "AlbumNameNormalized", 
            // r.MusicBrainzId as "AlbumMusicBrainzId", a.Name as "ArtistName", 
            // a.NameNormalized as "ArtistNameNormalized", a.MusicBrainzId as "ArtistMusicBrainzId"
            // from "Release" r
            //     left join "ArtistCreditName" acn on r.ArtistCreditId = acn.ArtistCreditId
            // left join "Artist" a on acn.ArtistId = a.Id
            // where r.MusicBrainzId = 'D208BEA3-F292-4C00-BF29-FE1E2361A28A'            
          
           //TODO record last time ran, create a job to run import
           
            
        }

        return new OperationResult<bool>
        {
            Data = artistCountInserted > 0 &&
                   releaseCountInserted > 0 &&
                   artistCreditInserted > 0 &&
                   artistCreditNameInserted > 0
        };
    }
}
