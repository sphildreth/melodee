namespace Melodee.Models;

public partial record KeyPair
{
    public required string StyleClass { get; init; }
    
    public required string Key { get; init; }
    
    public required object? Value { get; init; }

}
