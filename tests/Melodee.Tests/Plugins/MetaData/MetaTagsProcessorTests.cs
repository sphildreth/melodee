using System.Diagnostics;
using Melodee.Common.Enums;
using Melodee.Common.Models;
using Melodee.Plugins.Processor;

namespace Melodee.Tests.Plugins.MetaData;

public class MetaTagsProcessorTests
{
    [Theory]
    [InlineData(null, null)]
    [InlineData("Something", "Something")]
    [InlineData("Eternally Gifted", "Eternally Gifted")]
    [InlineData("Shift Scene", "Shift Scene")]
    [InlineData("Something With Bob", "Something")]
    [InlineData("Something Ft Bob", "Something")]
    [InlineData("Something ft Bob", "Something")]
    [InlineData("Something Ft. Bob", "Something")]
    [InlineData("Something (Ft. Bob)", "Something")]
    [InlineData("Something Feat. Bob", "Something")]
    [InlineData("Something Featuring Bob", "Something")]
    [InlineData("Something (with Bob)", "Something")]
    [InlineData("Minds Without Fear with Vishal-Shekhar", "Minds Without Fear")]
    [InlineData("Actin Bad Baby (feat. Brookie Salas)", "Actin Bad Baby")]
    public async Task ValidateTrackTitleFeaturingRemoved(string? originalTrackTitle, string? shouldBe)
    {
        var albumArtistShouldBe = "Da Artist";
        var processor = new MetaTagsProcessor(TestsBase.NewConfiguration);
        var processorResult = await processor.ProcessMetaTagAsync(new[]
        {
            new MetaTag<object?> { Identifier = MetaTagIdentifier.Artist, Value = albumArtistShouldBe },
            new MetaTag<object?> { Identifier = MetaTagIdentifier.Title, Value = originalTrackTitle }
        });
        Assert.NotNull(processorResult);
        Assert.True(processorResult.IsSuccess);
        var groupedByIdentifier = processorResult.Data.GroupBy(x => x.Identifier);
        Assert.DoesNotContain(groupedByIdentifier, x => x.Count() > 1);
        var trackTag = processorResult.Data.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.Title);
        Assert.NotNull(trackTag);
        Assert.NotNull(trackTag.Value);
        if (shouldBe != null)
        {
            if (trackTag.Value as string != originalTrackTitle)
            {
                Assert.NotNull(trackTag.OriginalValue);
                Assert.Equal(originalTrackTitle, trackTag.OriginalValue);
            }

            Assert.Equal(shouldBe, trackTag.Value);
            var albumArtist = processorResult.Data.FirstOrDefault(x => x.Identifier == MetaTagIdentifier.AlbumArtist);
            if (albumArtist != null)
            {
                Assert.NotNull(albumArtist.Value);
                Assert.Equal(albumArtistShouldBe, albumArtist.Value);
            }
        }
    }
}