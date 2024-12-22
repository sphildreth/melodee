using System.Collections.Concurrent;
using System.Globalization;
using Melodee.Common.Configuration;
using Melodee.Common.Constants;
using Melodee.Common.Enums;
using Melodee.Common.Extensions;
using Melodee.Common.Models;
using Melodee.Common.Models.SearchEngines;
using Melodee.Common.Utility;
using Melodee.Plugins.SearchEngine.MusicBrainz.Data.Models;
using Serilog;
using Serilog.Events;
using SerilogTimings;


namespace Melodee.Plugins.SearchEngine.MusicBrainz.Data;

public abstract class MusicBrainzRepositoryBase(ILogger logger, IMelodeeConfigurationFactory configurationFactory) : IMusicBrainzRepository
{
    public const int MaxIndexSize = 255;
    
    protected readonly ConcurrentBag<Models.Materialized.Artist> LoadedMaterializedArtists = [];
    protected readonly ConcurrentBag<Models.Materialized.ArtistRelation> LoadedMaterializedArtistRelations = [];    
    protected readonly ConcurrentBag<Models.Materialized.Album> LoadedMaterializedAlbums = [];

    protected Models.Artist[] LoadedArtists = [];
    protected ArtistCredit[] LoadedArtistCredits = [];
    protected ArtistCreditName[] LoadedArtistCreditNames = [];
    protected ArtistAlias[] LoadedArtistAliases = [];
    protected LinkType[] LoadedLinkTypes = [];
    protected Link[] LoadedLinks = [];
    protected LinkArtistToArtist[] LoadedLinkArtistToArtists = [];

    protected Release[] LoadedReleases = [];
    protected ReleaseCountry[] LoadedReleasesCountries = [];
    protected Tag[] LoadedTags = [];
    protected ReleaseTag[] LoadedReleaseTags = [];
    protected ReleaseGroup[] LoadedReleaseGroups = [];
    protected ReleaseGroupMeta[] LoadedReleaseGroupMetas = [];

    protected ILogger Logger { get; } = logger;
    protected IMelodeeConfigurationFactory ConfigurationFactory { get; } = configurationFactory;

    protected static async Task<T[]> LoadDataFromFileAsync<T>(string file, Func<string[], T> constructor, CancellationToken cancellationToken = default) where T : notnull
    {
        if (!File.Exists(file))
        {
            return [];
        }

        var result = new List<T>();
        await foreach (var lineFromFile in File.ReadLinesAsync(file, cancellationToken))
        {
            var parts = lineFromFile.Split('\t');
            result.Add(constructor(parts));
        }

        return result.ToArray();
    }

    public abstract Task<Models.Materialized.Album?> GetAlbumByMusicBrainzId(Guid musicBrainzId, CancellationToken cancellationToken = default);
    public abstract Task<PagedResult<ArtistSearchResult>> SearchArtist(ArtistQuery query, int maxResults, CancellationToken cancellationToken = default);
    public abstract Task<OperationResult<bool>> ImportData(CancellationToken cancellationToken = default);

    protected async Task<string> StoragePath(CancellationToken cancellationToken = default)
    {
        var configuration = await ConfigurationFactory.GetConfigurationAsync(cancellationToken).ConfigureAwait(false);
        var storagePath = configuration.GetValue<string>(SettingRegistry.SearchEngineMusicBrainzStoragePath);
        if (storagePath == null || !Directory.Exists(storagePath))
        {
            throw new Exception("MusicBrainz storage path is invalid [{SettingRegistry.SearchEngineMusicBrainzStoragePath}]");
        }

        return storagePath;
    }

    protected async Task LoadDataFromMusicBrainzFiles(CancellationToken cancellationToken = default)
    {
        var storagePath = await StoragePath(cancellationToken).ConfigureAwait(false);

        using (Operation.At(LogEventLevel.Debug).Time("MusicBrainzRepository: Loaded artists"))
        {
            LoadedArtists = await LoadDataFromFileAsync(Path.Combine(storagePath, "staging/mbdump/artist"), parts => new Models.Artist
            {
                Id = SafeParser.ToNumber<long>(parts[0]),
                MusicBrainzId = SafeParser.ToGuid(parts[1]) ?? Guid.Empty,
                Name = parts[2].CleanString().TruncateLongString(MaxIndexSize)!,
                NameNormalized = parts[2].CleanString().TruncateLongString(MaxIndexSize)!.ToNormalizedString() ?? parts[2],
                SortName = parts[3].CleanString(true).TruncateLongString(MaxIndexSize) ?? parts[2]
            }, cancellationToken).ConfigureAwait(false);
        }

        using (Operation.At(LogEventLevel.Debug).Time("MusicBrainzRepository: Loaded artist_credit"))
        {
            LoadedArtistCredits = await LoadDataFromFileAsync(Path.Combine(storagePath, "staging/mbdump/artist_credit"), parts => new ArtistCredit
            {
                Id = SafeParser.ToNumber<long>(parts[0]),
                ArtistCount = SafeParser.ToNumber<int>(parts[2]),
                Name = parts[1],
                RefCount = SafeParser.ToNumber<int>(parts[3]),
                Gid = SafeParser.ToGuid(parts[6]) ?? Guid.Empty
            }, cancellationToken).ConfigureAwait(false);
        }

        using (Operation.At(LogEventLevel.Debug).Time("MusicBrainzRepository: Loaded artist_credit_name"))
        {
            LoadedArtistCreditNames = await LoadDataFromFileAsync(Path.Combine(storagePath, "staging/mbdump/artist_credit_name"), parts => new ArtistCreditName
            {
                ArtistCreditId = SafeParser.ToNumber<long>(parts[0]),
                Position = SafeParser.ToNumber<int>(parts[1]),
                ArtistId = SafeParser.ToNumber<long>(parts[2]),
                Name = parts[3]
            }, cancellationToken).ConfigureAwait(false);
        }

        using (Operation.At(LogEventLevel.Debug).Time("MusicBrainzRepository: Loaded artist_alias"))
        {
            LoadedArtistAliases = await LoadDataFromFileAsync(Path.Combine(storagePath, "staging/mbdump/artist_alias"), parts => new ArtistAlias
            {
                Id = SafeParser.ToNumber<long>(parts[0]),
                ArtistId = SafeParser.ToNumber<long>(parts[1]),
                Name = parts[2],
                Type = SafeParser.ToNumber<int>(parts[6]),
                SortName = parts[7]
            }, cancellationToken).ConfigureAwait(false);
        }


        using (Operation.At(LogEventLevel.Debug).Time("MusicBrainzRepository: Loaded link_type"))
        {
            LoadedLinkTypes = await LoadDataFromFileAsync(Path.Combine(storagePath, "staging/mbdump/link_type"), parts => new LinkType
            {
                Id = SafeParser.ToNumber<long>(parts[0]),
                ParentId = SafeParser.ToNumber<long>(parts[1]),
                ChildOrder = SafeParser.ToNumber<int>(parts[2]),
                MusicBrainzId = SafeParser.ToGuid(parts[3]) ?? Guid.Empty,
                EntityType0 = parts[4],
                EntityType1 = parts[5],
                Name = parts[6],
                Description = parts[7],
                LinkPhrase = parts[8],
                ReverseLinkPhrase = parts[9],
                HasDates = SafeParser.ToBoolean(parts[13]),
                Entity0Cardinality = SafeParser.ToNumber<int>(parts[14]),
                Entity1Cardinality = SafeParser.ToNumber<int>(parts[15])
            }, cancellationToken).ConfigureAwait(false);
        }

        using (Operation.At(LogEventLevel.Debug).Time("MusicBrainzRepository: Loaded link"))
        {
            LoadedLinks = await LoadDataFromFileAsync(Path.Combine(storagePath, "staging/mbdump/link"), parts => new Link
            {
                Id = SafeParser.ToNumber<long>(parts[0]),
                LinkTypeId = SafeParser.ToNumber<long>(parts[1]),
                BeginDateYear = SafeParser.ToNumber<int?>(parts[2]),
                BeginDateMonth = SafeParser.ToNumber<int?>(parts[3]),
                BeginDateDay = SafeParser.ToNumber<int?>(parts[4]),
                EndDateYear = SafeParser.ToNumber<int?>(parts[5]),
                EndDateMonth = SafeParser.ToNumber<int?>(parts[6]),
                EndDateDay = SafeParser.ToNumber<int?>(parts[7]),
                IsEnded = SafeParser.ToBoolean(parts[10])
            }, cancellationToken).ConfigureAwait(false);
        }

        using (Operation.At(LogEventLevel.Debug).Time("MusicBrainzRepository: Loaded artist link"))
        {
            LoadedLinkArtistToArtists = await LoadDataFromFileAsync(Path.Combine(storagePath, "staging/mbdump/l_artist_artist"), parts => new LinkArtistToArtist
            {
                Id = SafeParser.ToNumber<long>(parts[0]),
                LinkId = SafeParser.ToNumber<long>(parts[1]),
                Artist0 = SafeParser.ToNumber<long>(parts[2]),
                Artist1 = SafeParser.ToNumber<long>(parts[3]),
                LinkOrder = SafeParser.ToNumber<int>(parts[6]),
                Artist0Credit = parts[7],
                Artist1Credit = parts[8]
            }, cancellationToken).ConfigureAwait(false);
        }


        using (Operation.At(LogEventLevel.Debug).Time("MusicBrainzRepository: Loaded release"))
        {
            LoadedReleases = await LoadDataFromFileAsync(Path.Combine(storagePath, "staging/mbdump/release"), parts => new Release
            {
                ArtistCreditId = SafeParser.ToNumber<long>(parts[3]),
                Id = SafeParser.ToNumber<long>(parts[0]),
                MusicBrainzId = parts[1],
                Name = parts[2].CleanString()!,
                NameNormalized = parts[2].CleanString().TruncateLongString(MaxIndexSize).ToNormalizedString() ?? parts[2],
                SortName = parts[2].CleanString(true).TruncateLongString(MaxIndexSize) ?? parts[2],
                ReleaseGroupId = SafeParser.ToNumber<long>(parts[4])
            }, cancellationToken).ConfigureAwait(false);
        }

        using (Operation.At(LogEventLevel.Debug).Time("MusicBrainzRepository: Loaded release_country"))
        {
            LoadedReleasesCountries = await LoadDataFromFileAsync(Path.Combine(storagePath, "staging/mbdump/release_country"), parts => new ReleaseCountry
            {
                ReleaseId = SafeParser.ToNumber<long>(parts[0]),
                CountryId = SafeParser.ToNumber<long>(parts[1]),
                DateYear = SafeParser.ToNumber<int>(parts[2]),
                DateMonth = SafeParser.ToNumber<int>(parts[3]),
                DateDay = SafeParser.ToNumber<int>(parts[4])
            }, cancellationToken).ConfigureAwait(false);
        }

        using (Operation.At(LogEventLevel.Debug).Time("MusicBrainzRepository: Loaded tag"))
        {
            LoadedTags = await LoadDataFromFileAsync(Path.Combine(storagePath, "staging/mbdump/tag"), parts => new Tag
            {
                Id = SafeParser.ToNumber<long>(parts[0]),
                Name = parts[1]
            }, cancellationToken).ConfigureAwait(false);
        }

        using (Operation.At(LogEventLevel.Debug).Time("MusicBrainzRepository: Loaded release_tag"))
        {
            LoadedReleaseTags = await LoadDataFromFileAsync(Path.Combine(storagePath, "staging/mbdump/release_tag"), parts => new ReleaseTag
            {
                ReleaseId = SafeParser.ToNumber<long>(parts[0]),
                TagId = SafeParser.ToNumber<long>(parts[1])
            }, cancellationToken).ConfigureAwait(false);
        }

        using (Operation.At(LogEventLevel.Debug).Time("MusicBrainzRepository: Loaded release_group"))
        {
            LoadedReleaseGroups = await LoadDataFromFileAsync(Path.Combine(storagePath, "staging/mbdump/release_group"), parts => new ReleaseGroup
            {
                Id = SafeParser.ToNumber<long>(parts[0]),
                MusicBrainzIdRaw = parts[1],
                Name = parts[2],
                ArtistCreditId = SafeParser.ToNumber<long>(parts[3]),
                ReleaseType = SafeParser.ToNumber<int>(parts[4])
            }, cancellationToken).ConfigureAwait(false);
        }

        using (Operation.At(LogEventLevel.Debug).Time("MusicBrainzRepository: Loaded release_group_meta"))
        {
            LoadedReleaseGroupMetas = await LoadDataFromFileAsync(Path.Combine(storagePath, "staging/mbdump/release_group_meta"), parts => new ReleaseGroupMeta
            {
                ReleaseGroupId = SafeParser.ToNumber<long>(parts[0]),
                DateYear = SafeParser.ToNumber<int>(parts[2]),
                DateMonth = SafeParser.ToNumber<int>(parts[3]),
                DateDay = SafeParser.ToNumber<int>(parts[4])
            }, cancellationToken).ConfigureAwait(false);
        }

        var artistAliasDictionary = LoadedArtistAliases.GroupBy(x => x.ArtistId).ToDictionary(x => x.Key, x => x.ToArray());
        using (Operation.At(LogEventLevel.Debug).Time($"MusicBrainzRepository: LoadedMaterializedArtists"))
        {
            Parallel.ForEach(LoadedArtists, artist =>
            {
                artistAliasDictionary.TryGetValue(artist.Id, out var aArtistAlias);
                LoadedMaterializedArtists.Add(new Models.Materialized.Artist
                {
                    MusicBrainzArtistId = artist.Id,
                    Name = artist.Name,
                    SortName = artist.SortName,
                    NameNormalized = artist.NameNormalized,
                    MusicBrainzIdRaw = artist.MusicBrainzId.ToString(),
                    AlternateNames = "".AddTag(aArtistAlias?.Select(x => x.Name.ToNormalizedString() ?? x.Name), dontLowerCase: true)
                });
            });
        }

        var loadedMaterializedArtistsDictionary = LoadedMaterializedArtists.ToDictionary(x => x.MusicBrainzArtistId, x => x);
        
        using (Operation.At(LogEventLevel.Debug).Time($"MusicBrainzRepository: LoadedMaterializedArtistRelations"))
        {
            var loadedLinkDictionary = LoadedLinks.ToDictionary(x => x.Id, x => x);
            
            var artistLinks = LoadedLinkArtistToArtists.GroupBy(x => x.Artist0).ToDictionary(x => x.Key, x => x.ToArray());
            var associatedArtistRelationType = SafeParser.ToNumber<int>(ArtistRelationType.Associated);
            Parallel.ForEach(artistLinks, artistLink =>
            {
                if (!loadedMaterializedArtistsDictionary.TryGetValue(artistLink.Key, out var dbArtist))
                {
                    return;
                }

                foreach (var artistLinkRelation in artistLink.Value)
                {
                    if (!loadedMaterializedArtistsDictionary.TryGetValue(artistLinkRelation.Artist1, out var dbLinkedArtist))
                    {
                        continue;
                    }

                    loadedLinkDictionary.TryGetValue(artistLink.Key, out var link);
                    if (link != null)
                    {
                        LoadedMaterializedArtistRelations.Add(new Models.Materialized.ArtistRelation
                        {
                            ArtistId = dbArtist.Id,
                            RelatedArtistId = dbLinkedArtist.Id,
                            ArtistRelationType = associatedArtistRelationType,
                            SortOrder = artistLinkRelation.LinkOrder,
                            RelationStart = link.BeginDate,
                            RelationEnd = link.EndDate
                        });
                    }
                }
            });
        }

        using (Operation.At(LogEventLevel.Debug).Time($"MusicBrainzRepository: LoadedMaterializedAlbums"))
        {
            var releaseCountriesDictionary = LoadedReleasesCountries.GroupBy(x => x.ReleaseId).ToDictionary(x => x.Key, x => x.ToList());
            var releaseGroupsDictionary = LoadedReleaseGroups.GroupBy(x => x.Id).ToDictionary(x => x.Key, x => x.ToList());
            var releaseGroupsMetaDictionary = LoadedReleaseGroupMetas.GroupBy(x => x.ReleaseGroupId).ToDictionary(x => x.Key, x => x.ToList());
            var artistCreditsDictionary = LoadedArtistCredits.GroupBy(x => x.Id).ToDictionary(x => x.Key, x => x.ToList());
            var artistCreditsNamesDictionary = LoadedArtistCreditNames.GroupBy(x => x.ArtistCreditId).ToDictionary(x => x.Key, x => x.ToList());

            Parallel.ForEach(LoadedReleases, release =>
            {
                releaseCountriesDictionary.TryGetValue(release.Id, out var releaseCountries);
                releaseGroupsDictionary.TryGetValue(release.ReleaseGroupId, out var releaseReleaseGroups);
                var releaseCountry = releaseCountries?.OrderBy(x => x.ReleaseDate).FirstOrDefault();
                var releaseGroup = releaseReleaseGroups?.FirstOrDefault();

                loadedMaterializedArtistsDictionary.TryGetValue(release.ArtistCreditId, out var releaseArtist);

                if (releaseGroup != null && !(releaseCountry?.IsValid ?? false))
                {
                    releaseGroupsMetaDictionary.TryGetValue(release.ReleaseGroupId, out var releaseGroupsMeta);
                    var releaseGroupMeta = releaseGroupsMeta?.OrderBy(x => x.ReleaseDate).FirstOrDefault();
                    if (releaseGroupMeta?.IsValid ?? false)
                    {
                        releaseCountry = new ReleaseCountry
                        {
                            ReleaseId = release.Id,
                            DateDay = releaseGroupMeta.DateDay,
                            DateMonth = releaseGroupMeta.DateMonth,
                            DateYear = releaseGroupMeta.DateYear
                        };
                    }
                }

                string? contributorIds = null;

                artistCreditsDictionary.TryGetValue(release.ArtistCreditId, out var releaseArtistCredits);
                var artistCredit = releaseArtistCredits?.FirstOrDefault();
                if (artistCredit != null)
                {
                    artistCreditsNamesDictionary.TryGetValue(artistCredit.Id, out var releaseArtistCreditNames);
                    var artistCreditName = releaseArtistCreditNames?.OrderBy(x => x.Position).FirstOrDefault();
                    if (artistCreditName != null)
                    {
                        // Sometimes there are multiple artists on a release (see https://musicbrainz.org/release/519345af-b328-4d88-98cb-29f1a5d1fe2d) and the
                        // ArtistCreditId doesn't point to an ArtistId it points to a ArtistCredit.Id when this is true then get ArtistCredit for
                        // that Id then get the ArtistCreditNames and the first one is used for the artist for Melodee
                        if (releaseArtist == null)
                        {
                          loadedMaterializedArtistsDictionary.TryGetValue(artistCreditName.ArtistId, out releaseArtist);
                        }
                    }

                    var artistCreditNameArtistId = artistCreditName?.ArtistId ?? 0;
                    contributorIds = releaseArtistCreditNames == null
                        ? null
                        : "".AddTag(releaseArtistCreditNames
                            .Where(x => x.ArtistId != artistCreditNameArtistId)
                            .Select(x => x.ArtistId.ToString()));
                }

                if (releaseArtist != null && releaseGroup != null && (releaseCountry?.IsValid ?? false))
                {
                    LoadedMaterializedAlbums.Add(new Models.Materialized.Album
                    {
                        MusicBrainzArtistId = releaseArtist.MusicBrainzArtistId,
                        ContributorIds = contributorIds,
                        MusicBrainzIdRaw = release.MusicBrainzId,
                        Name = release.Name,
                        NameNormalized = (release.NameNormalized ?? release.Name),
                        ReleaseDate = releaseCountry.ReleaseDate,
                        ReleaseGroupMusicBrainzIdRaw = releaseGroup.MusicBrainzIdRaw,
                        ReleaseType = releaseGroup.ReleaseType,
                        SortName = release.SortName ?? release.Name
                    });
                }
            });
        }
    }

    /// <summary>
    /// This is because "1994-02-29" isn't a date.
    /// </summary>
    public static DateTime? ParseJackedUpMusicBrainzDate(string? dateRaw)
    {
        if (dateRaw == null)
        {
            return null;
        }
        if (DateTime.TryParse(dateRaw, CultureInfo.InvariantCulture, out var date))
        {
            return date;
        }
        return null;
    }
}
