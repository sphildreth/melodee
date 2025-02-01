using Microsoft.EntityFrameworkCore;

namespace Melodee.Common.Models.SearchEngines.ArtistSearchEngineServiceData;

public class ArtistSearchEngineServiceDbContext(DbContextOptions<ArtistSearchEngineServiceDbContext> options) : DbContext(options)
{
    public DbSet<Artist> Artists { get; set; }
    
    public DbSet<Album> Albums { get; set; }
}
