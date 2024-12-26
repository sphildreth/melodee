using Melodee.Common.Enums;

namespace Melodee.Plugins.Validation.Models;

public sealed record AlbumValidationResult(AlbumStatus AlbumStatus, AlbumNeedsAttentionReasons AlbumStatusReasons) : ValidationResult;
