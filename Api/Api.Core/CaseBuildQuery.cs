using PayrollEngine.Domain.Model;
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace PayrollEngine.Api.Core;

/// <summary>
/// Query for the payroll case controller
/// </summary>
public class CaseBuildQuery : PayrollQuery
{
    /// <summary>
    /// The user id
    /// </summary>
    public int? UserId { get; set; }

    /// <summary>
    /// The case name
    /// </summary>
    public string CaseName { get; set; }

    /// <summary>
    /// The cluster set name (optional)
    /// </summary>
    public string ClusterSetName { get; set; }
}