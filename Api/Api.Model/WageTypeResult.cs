using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Api.Model;

/// <summary>
/// The wage type result API object
/// </summary>
public class WageTypeResult : ApiObjectBase
{
    /// <summary>
    /// The payroll result id (immutable)
    /// </summary>
    public int PayrollResultId { get; set; }

    /// <summary>
    /// The wage type id (immutable)
    /// </summary>
    [Required]
    public int WageTypeId { get; set; }

    /// <summary>
    /// The wage type number (immutable)
    /// </summary>
    [Required]
    public decimal WageTypeNumber { get; set; }

    /// <summary>
    /// The wage type name (immutable)
    /// </summary>
    [Required]
    public string WageTypeName { get; set; }

    /// <summary>
    /// The localized wage type names (immutable)
    /// </summary>
    public Dictionary<string, string> WageTypeNameLocalizations { get; set; }

    /// <summary>
    /// The value type (immutable)
    /// </summary>
    public ValueType ValueType { get; set; }

    /// <summary>
    /// The wage type result value (immutable)
    /// </summary>
    [Required]
    public decimal Value { get; set; }

    /// <summary>
    /// The starting date for the value
    /// </summary>
    [Required]
    public DateTime Start { get; set; }

    /// <summary>
    /// The ending date for the value
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
        $"{WageTypeNumber:##.####} {Value:##.####} {base.ToString()}";
}