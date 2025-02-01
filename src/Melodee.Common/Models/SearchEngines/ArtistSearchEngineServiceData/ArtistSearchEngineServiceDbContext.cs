using Microsoft.EntityFrameworkCore;

namespace Melodee.Common.Models.SearchEngines.ArtistSearchEngineServiceData;

public class ArtistSearchEngineServiceDbContext(DbContextOptions<ArtistSearchEngineServiceDbContext> options) : DbContext(options)
{
    public DbSet<Artist> Artists { get; set; }
}
