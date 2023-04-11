using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Api.Model;

/// <summary>
/// The wage type custom result API object
/// </summary>
public class WageTypeCustomResult : ApiObjectBase
{
    /// <summary>
    /// The wage type result id (immutable)
    /// </summary>
    public int WageTypeResultId { get; set; }

    /// <summary>
    /// The wage type number (immutable)
    /// </summary>
    [Required]
    public decimal WageTypeNumber { get; set; }
        
    /// <summary>
    /// The wage type name (immutable)
    /// </summary>
    public string WageTypeName { get; set; }

    /// <summary>
    /// The localized wage type names (immutable)
    /// </summary>
    public Dictionary<string, string> WageTypeNameLocalizations { get; set; }

    /// <summary>
    /// The value source (immutable)
    /// </summary>
    [Required]
    public string Source { get; set; }

    /// <summary>
    /// The value type (immutable)
    /// </summary>
    public ValueType ValueType { get; set; }

    /// <summary>
    /// The wage type custom result value (immutable)
    /// </summary>
    [Required]
    public decimal Value { get; set; }

    /// <summary>
    /// The period starting date for the value
    /// </summary>
    [Required]
    public DateTime Start { get; set; }

    /// <summary>
    /// The period ending date for the value
    /// </summary>
    [Required]
    public DateTime End { get; set; }

    /// <summary>
    /// The result tags
    /// </summary>
    public List<string> Tags { get; set; }

    /// <summary>
    /// The result attributes (immutable)
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; }

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Source}={Value} [{Start}-{End}] {base.ToString()}";
}