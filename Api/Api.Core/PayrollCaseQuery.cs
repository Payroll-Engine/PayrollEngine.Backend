using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PayrollEngine.Domain.Model;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace PayrollEngine.Api.Core;

/// <summary>
/// Query for the payroll case controller
/// </summary>
public class PayrollCaseQuery : PayrollQuery
{
    /// <summary>
    /// The user id
    /// </summary>
    [Required]
    public int UserId { get; set; }

    /// <summary>
    /// The case type
    /// </summary>
    [Required]
    public CaseType CaseType { get; set; }

    /// <summary>
    /// The case names (optional)
    /// </summary>
    // ReSharper disable once CollectionNeverUpdated.Global
    public List<string> CaseNames { get; set; }

    /// <summary>
    /// The cluster set name (optional)
    /// </summary>
    public string ClusterSetName { get; set; }

    /// <summary>
    /// The culture (optional)
    /// </summary>
    public string Culture { get; set; }
}