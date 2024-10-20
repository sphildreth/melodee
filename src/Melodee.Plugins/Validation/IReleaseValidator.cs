using Melodee.Common.Models;
using Melodee.Plugins.Validation.Models;

namespace Melodee.Plugins.Validation;

public interface IAlbumValidator
{
    OperationResult<ValidationResult> ValidateAlbum(Album? album);
}
