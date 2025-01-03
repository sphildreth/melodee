namespace Melodee.Blazor.Components.Components;

public record TagsInputOptions
{
    /// <summary>
    /// Gets or inits the name of the class for the tag wrapper
    /// </summary>
    public string WrapperClass { get; init; } = "melodee-tag-wrapper";
    
    /// <summary>
    /// Gets or inits the name of the class for the tag list
    /// </summary>
    public string TagListClass { get; init; } = "melodee-tag-list rz-w-100";
    
    /// <summary>
    /// Gets or inits the name of the class for a tag
    /// </summary>
    public string TagClass { get; init; } = "melodee-tag";
    
    /// <summary>
    /// Gets or inits the name of the class for the input field.
    /// </summary>
    public string InputClass { get; init; } = "melodee-tag-input";
    
    /// <summary>
    /// Gets or inits the name of the class for the label
    /// </summary>
    public string LabelClass { get; init; } = "melodee-tag-label";
    
    /// <summary>
    /// Gets or inits the text for the tooltip from the remove button
    /// </summary>
    public string RemoveButtonTooltip { get; init; } = "Remove";
    
    /// <summary>
    /// Gets or inits the placeholder text for the input field.
    /// </summary>
    public string InputPlaceholder { get; init; } = "Enter tag, add with Enter";
    
    /// <summary>
    /// Gets or inits the maximum length of a tag in order to be able to be added to the list.
    /// <para>0 = No minimum length</para>
    /// </summary>
    public int MinLength { get; init; }
    
    /// <summary>
    /// Gets or inits the maximum length of a tag in order to be able to be added to the list.
    /// <para>0 = No maximum length</para>
    /// </summary>
    public int MaxLength { get; init; }    
}
