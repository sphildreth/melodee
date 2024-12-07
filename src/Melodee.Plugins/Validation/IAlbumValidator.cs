using Melodee.Common.Models;
using Melodee.Plugins.Validation.Models;

namespace Melodee.Plugins.Validation;

public interface IAlbumValidator
{
    OperationResult<AlbumValidationResult> ValidateAlbum(Album? album);
}
