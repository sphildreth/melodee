using Melodee.Cli.CommandSettings;
using Melodee.Common.Models.Importing;
using Melodee.Common.Services;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;

namespace Melodee.Cli.Command;

public class ImportUserFavoriteCommand : CommandBase<ImportUserFavorite>
{
    public override async Task<int> ExecuteAsync(CommandContext context, ImportUserFavorite settings)
    {
        using (var scope = CreateServiceProvider().CreateScope())
        {
            var userService = scope.ServiceProvider.GetRequiredService<UserService>();
            var result = await userService.ImportUserFavoriteSongs(new UserFavoriteSongConfiguration(
                    settings.CsvFileName,
                    Guid.Parse(settings.UserApiKey),
                    settings.Artist,
                    settings.Album,
                    settings.Song,
                    settings.IsPretend))
                .ConfigureAwait(false);
            return result.IsSuccess ? 1 : 0;
        }
    }
}
