using Melodee.Common.Enums;
using Melodee.Common.Models;
using Melodee.Plugins.Validation.Models;

namespace Melodee.Plugins.Validation;

public interface IImageValidator
{
    Task<OperationResult<ValidationResult>> ValidateImage(FileInfo? fileInfo, PictureIdentifier identifier, CancellationToken cancellationToken = default);
}
