using System;
using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Api.Model;

/// <summary>A case value from a case change used in national, company and employee case</summary>
public class CaseChangeCaseValue : CaseValue
{
    /// <summary>
    /// The case change id
    /// </summary>
    [Required]
    public int CaseChangeId { get; set; }

    /// <summary>
    /// The case change creation
    /// </summary>
    public DateTime CaseChangeCreated { get; set; }

    /// <summary>
    /// The change user id
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// The user unique identifier
    /// </summary>
    [StringLength(128)]
    public string UserIdentifier { get; set; }

    /// <summary>
    /// The change reason
    /// </summary>
    public string Reason { get; set; }

    /// <summary>
    /// The validation case name
    /// </summary>
    public string ValidationCaseName { get; set; }

    /// <summary>
    /// The cancellation type
    /// </summary>
    public CaseCancellationType CancellationType { get; set; }

    /// <summary>
    /// The canceled case change id
    /// </summary>
    public int? CancellationId { get; set; }

    /// <summary>
    /// The document count
    /// </summary>
    public int Documents { get; set; }
}