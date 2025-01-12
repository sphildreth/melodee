using System.ComponentModel.DataAnnotations;
using Melodee.Common.Data.Constants;

namespace Melodee.Blazor.ViewModels;

public sealed class Register
{
    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    [Required(ErrorMessage = "Username is required")]
    public string? Username { get; set; }

    [MaxLength(MaxLengthDefinitions.MaxGeneralInputLength)]
    [Required(ErrorMessage = "Email is required")]
    [DataType(DataType.EmailAddress)]
    public string? EmailAddress { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "The Password field must be a minimum of 6 characters")]
    [DataType(DataType.Password)]
    public string? Password { get; set; }
}
