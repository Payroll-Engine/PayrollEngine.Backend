using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Api.Model;

/// <summary>
/// The payrun result API object
/// </summary>
public class PayrunResult : ApiObjectBase
{
    /// <summary>
    /// The payroll result id (immutable)
    /// </summary>
    public int PayrollResultId { get; set; }

    /// <summary>
    /// The result source (immutable)
    /// </summary>
    [Required]
    public string Source { get; set; }

    /// <summary>
    /// The result name (immutable)
    /// </summary>
    [Required]
    public string Name { get; set; }

    /// <summary>
    /// The localized result names
    /// </summary>
    // ReSharper disable once CollectionNeverUpdated.Global
    public Dictionary<string, string> NameLocalizations { get; set; }

    /// <summary>
    /// The result slot (immutable)
    /// </summary>
    public string Slot { get; set; }

    /// <summary>
    /// The value type (immutable)
    /// </summary>
    [Required]
    public ValueType ValueType { get; set; }

    /// <summary>
    /// The result value (immutable)
    /// </summary>
    [Required]
    public string Value { get; set; }

    /// <summary>
    /// The numeric result value (immutable)
    /// </summary>
    public decimal? NumericValue { get; set; }

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
    // ReSharper disable once CollectionNeverUpdated.Global
    public List<string> Tags { get; set; }

    /// <summary>
    /// The result attributes (immutable)
    /// </summary>
    // ReSharper disable once CollectionNeverUpdated.Global
    public Dictionary<string, object> Attributes { get; set; }

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Name}={Value} [{Start}-{End}] {base.ToString()}";
}