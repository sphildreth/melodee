using Melodee.Common.Models;
using Melodee.Plugins.Validation.Models;

namespace Melodee.Plugins.Validation;

public interface IImageValidator
{
    Task<OperationResult<ValidationResult>> ValidateImage(FileInfo? fileInfo, CancellationToken cancellationToken = default);
}
