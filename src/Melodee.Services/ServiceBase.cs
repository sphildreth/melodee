using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data;
using Melodee.Common.Models;
using Melodee.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Melodee.Services;

public abstract class ServiceBase(
    ILogger logger,
    ICacheManager cacheManager,
    IDbContextFactory<MelodeeDbContext> contextFactory)
{
    public const string CacheName = "melodee";
    protected static TimeSpan DefaultCacheDuration = TimeSpan.FromDays(1);

    protected ILogger Logger { get; } = logger;
    protected ICacheManager CacheManager { get; } = cacheManager;
    protected IDbContextFactory<MelodeeDbContext> ContextFactory { get; } = contextFactory;

    protected virtual OperationResult<(bool, IEnumerable<ValidationResult>?)> ValidateModel<T>(T? dataToValidate)
    {
        if (dataToValidate != null)
        {
            var ctx = new ValidationContext(dataToValidate);
            var validationResults = new List<ValidationResult>();
            if (!Validator.TryValidateObject(dataToValidate, ctx, validationResults, true))
            {
                return new OperationResult<(bool, IEnumerable<ValidationResult>?)>
                {
                    Data = (false, validationResults),
                    Type = OperationResponseType.ValidationFailure
                };
            }
        }

        return new OperationResult<(bool, IEnumerable<ValidationResult>?)>
        {
            Data = (dataToValidate != null, null)
        };
    }
}
