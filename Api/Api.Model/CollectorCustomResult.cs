using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Api.Model;

/// <summary>
/// The collector custom result API object
/// </summary>
public class CollectorCustomResult : ApiObjectBase
{
    /// <summary>
    /// The wage type result id (immutable)
    /// </summary>
    public int CollectorResultId { get; set; }

    /// <summary>
    /// The collector name (immutable)
    /// </summary>
    [Required]
    public string CollectorName { get; set; }

    /// <summary>
    /// The localized collector names (immutable)
    /// </summary>
    // ReSharper disable once CollectionNeverUpdated.Global
    public Dictionary<string, string> CollectorNameLocalizations { get; set; }

    /// <summary>
    /// The value source (immutable)
    /// </summary>
    [Required]
    public string Source { get; set; }

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
        $"{Source}={Value} [{Start}-{End}] {base.ToString()}";
}