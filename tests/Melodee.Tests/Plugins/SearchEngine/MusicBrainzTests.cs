namespace Melodee.Tests.Plugins.SearchEngine;

public class MusicBrainzTests : TestsBase
{
    // [Fact]
    // public async Task ValidateLoadingOfImportFiles()
    // {
    //     var maxIndexSize = 1000;
    //     var filename = "/melodee_test/search-engine-storage/musicbrainz/staging/mbdump/release";
    //     
    //     var result = new ConcurrentBag<Melodee.Common.Plugins.SearchEngine.MusicBrainz.Data.Models.Release>();
    //     //                    await Parallel.ForEachAsync(models, cancellationToken, async (model, tt) =>
    //     Parallel.ForEach(File.ReadLines(filename), lineFromFile =>
    //     {
    //         var parts = lineFromFile.Split('\t');
    //         result.Add(new Melodee.Common.Plugins.SearchEngine.MusicBrainz.Data.Models.Release
    //         {
    //             ArtistCreditId = SafeParser.ToNumber<long>(parts[3]),
    //             Id = SafeParser.ToNumber<long>(parts[0]),
    //             MusicBrainzId = parts[1],
    //             Name = parts[2].CleanString()!,
    //             NameNormalized = parts[2].CleanString().TruncateLongString(maxIndexSize).ToNormalizedString() ?? parts[2],
    //             SortName = parts[2].CleanString(true).TruncateLongString(maxIndexSize) ?? parts[2],
    //             ReleaseGroupId = SafeParser.ToNumber<long>(parts[4])
    //         });
    //     });
    //     Assert.True(!result.IsEmpty);
    // }
}
