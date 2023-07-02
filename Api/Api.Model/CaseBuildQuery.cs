// ReSharper disable UnusedAutoPropertyAccessor.Global
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Api.Model;

/// <summary>
/// Query for the payroll case controller
/// </summary>
public class CaseBuildQuery : PayrollQuery, ICultureQuery
{
    /// <summary>
    /// The user id
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// The culture name based on RFC 4646
    /// </summary>
    public string Culture { get; set; }

    /// <summary>
    /// The case name
    /// </summary>
    public string CaseName { get; set; }

    /// <summary>
    /// The cluster set name (optional)
    /// </summary>
    public string ClusterSetName { get; set; }
}