using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace PayrollEngine.Api.Model;

/// <summary>
/// The payroll case object, including case fields and related cases
/// </summary>
public class CaseSet : Case
{
    /// <summary>
    /// The case display name
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
    /// Cancellation date
    /// </summary>
    public DateTime? CancellationDate { get; set; }

    /// <summary>
    /// The change reason
    /// </summary>
    public string Reason { get; set; }
    
    /// <summary>
    /// The change forecast
    /// </summary>
    public string Forecast { get; set; }

    /// <summary>
    /// Derived case fields
    /// </summary>
    public CaseFieldSet[] Fields { get; set; }

    /// <summary>
    /// Related cases
    /// </summary>
    public CaseSet[] RelatedCases { get; set; }
        
    /// <inheritdoc/>
    public override string ToString() =>
        $"{Fields?.Length} fields, {RelatedCases?.Length} related {base.ToString()}";

}