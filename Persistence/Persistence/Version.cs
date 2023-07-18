using System;

namespace PayrollEngine.Persistence;

/// <summary>
/// Database version
/// </summary>
public class Version
{
    /// <summary>
    /// The created date
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    /// The major version
    /// </summary>
    public int MajorVersion { get; set; }

    /// <summary>
    /// The minor version
    /// </summary>
    public int MinorVersion { get; set; }

    /// <summary>
    /// The sub version
    /// </summary>
    public int SubVersion { get; set; }

    /// <summary>
    /// The version owner
    /// </summary>
    public string Owner { get; set; }

    /// <summary>
    /// The version description
    /// </summary>
    public string Description { get; set; }
}