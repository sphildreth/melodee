namespace Melodee.Blazor.Controllers.Melodee.Models;

public record SearchRequest(string Query, string? Type, short? Page, short? PageSize, string? SortBy, string? SortOrder);
