using System;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// The retro payrun job
/// </summary>
// ReSharper disable PropertyCanBeMadeInitOnly.Global
public class RetroPayrunJob : IEquatable<RetroPayrunJob>
{
    /// <summary>
    /// The schedule date
    /// </summary>
    public DateTime ScheduleDate { get; set; }

    /// <summary>
    /// The result tags
    /// </summary>
    public List<string> ResultTags { get; set; }

    /// <summary>
    /// Default constructor
    /// </summary>
    public RetroPayrunJob()
    {
    }

    /// <summary>
    /// Copy constructor
    /// </summary>
    public RetroPayrunJob(RetroPayrunJob copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(RetroPayrunJob compare) =>
        CompareTool.EqualProperties(this, compare);

    /// <inheritdoc/>
    public override string ToString() =>
        $"{ScheduleDate}] {base.ToString()}";
}