using Melodee.Common.Enums;
using Melodee.Common.Models;

namespace Melodee.Plugins.Validation;

public interface IReleaseValidator
{
    OperationResult<ReleaseStatus> ValidateRelease(Release? release);
}
