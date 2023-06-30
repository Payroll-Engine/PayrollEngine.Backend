// ReSharper disable UnusedAutoPropertyAccessor.Global
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Api.Model;

/// <summary>
/// The regulation script API object
/// </summary>
public class Script : ApiObjectBase
{
    /// <summary>
    /// The script name (immutable)
    /// </summary>
    [Required]
    [StringLength(128)]
    public string Name { get; set; }

    /// <summary>
    /// The scripting function types as bitmask
    /// </summary>
    public List<FunctionType> FunctionTypes { get; set; }

    /// <summary>
    /// The script script
    /// </summary>
    [Required]
    public string Value { get; set; }

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Name} {base.ToString()}";
}