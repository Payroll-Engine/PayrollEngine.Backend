using System;

namespace PayrollEngine.Domain.Scripting;

/// <summary>
/// Assembly information
/// </summary>
public class AssemblyInfo
{
    /// <summary>
    /// Assembly version
    /// </summary>
    public Version Version { get; set; }

    /// <summary>
    /// Assembly title
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Assembly product
    /// </summary>
    public string Product { get; set; }
}