using System.ComponentModel.DataAnnotations;

namespace Melodee.ViewModels;

public class Credential
{
    [Required]
    [DataType(DataType.Password)]
    public string? EmailAddress { get; set; }
    
    [Required]
    [DataType(DataType.Password)]
    public string? Password { get; set; }
    
    public bool RememberMe { get; set; }
}
