using Melodee.Common.Models;
using Melodee.Plugins.Validation.Models;

namespace Melodee.Plugins.Validation;

public interface IReleaseValidator
{
    OperationResult<ValidationResult> ValidateRelease(Release? release);
}
