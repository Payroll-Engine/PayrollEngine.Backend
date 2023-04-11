using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Api.Model;

/// <summary>
/// The collector result API object
/// </summary>
public class CollectorResult : ApiObjectBase
{
    /// <summary>
    /// The payroll result id (immutable)
    /// </summary>
    public int PayrollResultId { get; set; }

    /// <summary>
    /// The collector id (immutable)
    /// </summary>
    [Required]
    public int CollectorId { get; set; }

    /// <summary>
    /// The collector name (immutable)
    /// </summary>
    [Required]
    public string CollectorName { get; set; }

    /// <summary>
    /// The localized collector names (immutable)
    /// </summary>
    public Dictionary<string, string> CollectorNameLocalizations { get; set; }

    /// <summary>
    /// The collection type (immutable)
    /// </summary>
    [Required]
    public CollectType CollectType { get; set; }

    /// <summary>
    /// The value type (immutable)
    /// </summary>
    [Required]
    public ValueType ValueType { get; set; }

    /// <summary>
    /// The collector result value (immutable)
    /// </summary>
    [Required]
    public decimal Value { get; set; }

    /// <summary>
    /// The starting date for the value (immutable)
    /// </summary>
    [Required]
    public DateTime Start { get; set; }

    /// <summary>
    /// The ending date for the value (immutable)
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
        $"{CollectorName}={Value:##.####} {base.ToString()}";
}