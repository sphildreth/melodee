using Melodee.Common.Models;
using Melodee.Common.Plugins.Validation.Models;

namespace Melodee.Common.Plugins.Validation;

public interface IAlbumValidator
{
    OperationResult<AlbumValidationResult> ValidateAlbum(Album? album);
}
