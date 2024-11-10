using System.ComponentModel.DataAnnotations;

namespace Melodee.Common.Data.Validators;

public class RequiredGreaterThanZero : RequiredAttribute
{
    /// <summary>
    ///     Designed for dropdowns to ensure that a selection is valid and not the dummy "SELECT" entry
    /// </summary>
    /// <param name="value">The integer value of the selection</param>
    /// <returns>True if value is greater than zero</returns>
    public override bool IsValid(object? value)
    {
        // return true if value is a non-null number > 0, otherwise return false
        return value != null && int.TryParse(value.ToString(), out var i) && i > 0;
    }
}
