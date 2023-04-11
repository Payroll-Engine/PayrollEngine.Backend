﻿using PayrollEngine.Domain.Model;

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

    /// <summary>
    /// The language (optional)
    /// </summary>
    public Language? Language { get; set; }
}