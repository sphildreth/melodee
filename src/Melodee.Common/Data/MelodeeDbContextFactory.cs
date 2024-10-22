using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Melodee.Common.Data;

public class MelodeeDbContextFactory : IDesignTimeDbContextFactory<MelodeeDbContext>
{
    public MelodeeDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();
        var builder = new DbContextOptionsBuilder<MelodeeDbContext>();
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        builder.UseNpgsql(connectionString, o => o.UseNodaTime());
        return new MelodeeDbContext(builder.Options);
    }
}
