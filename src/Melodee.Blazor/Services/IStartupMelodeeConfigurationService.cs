namespace Melodee.Blazor.Services;

public interface IStartupMelodeeConfigurationService
{
    Task UpdateConfigurationFromEnvironmentAsync(CancellationToken cancellationToken = default);
}
