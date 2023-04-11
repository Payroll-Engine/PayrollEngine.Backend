using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Api.Model;

/// <summary>
/// The case field including the slot and values
/// </summary>
public class CaseFieldSet : CaseField
{
    /// <summary>
    /// The case field display name
    /// </summary>
    [StringLength(128)]
    public string DisplayName { get; set; }

    /// <summary>
    /// The case slot
    /// </summary>
    [StringLength(128)]
    public string CaseSlot { get; set; }

    /// <summary>
    /// The localized case slots
    /// </summary>
    public Dictionary<string, string> CaseSlotLocalizations { get; set; }

    /// <summary>
    /// The case value (JSON format)
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// The starting date for the value
    /// </summary>
    public DateTime? Start { get; set; }

    /// <summary>
    /// The ending date for the value
    /// </summary>
    public DateTime? End { get; set; }

    /// <summary>
    /// Cancellation date
    /// </summary>
    public DateTime? CancellationDate { get; set; }

    /// <inheritdoc/>
    public override string ToString() =>
        !string.IsNullOrWhiteSpace(Value) ? $" ({Value}) {base.ToString()}" : base.ToString();
}