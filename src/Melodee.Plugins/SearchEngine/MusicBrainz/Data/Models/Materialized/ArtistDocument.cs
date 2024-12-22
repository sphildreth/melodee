using Lucene.Net.Documents;

namespace Melodee.Plugins.SearchEngine.MusicBrainz.Data.Models.Materialized;

public sealed record ArtistDocument(StringField MusicBrainzId, StringField NameNormalized, TextField AlternateNames);

