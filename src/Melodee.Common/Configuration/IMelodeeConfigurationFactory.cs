namespace Melodee.Common.Configuration;

public interface IMelodeeConfigurationFactory
{
    public Task<IMelodeeConfiguration> GetConfigurationAsync(CancellationToken cancellationToken = default);
}
