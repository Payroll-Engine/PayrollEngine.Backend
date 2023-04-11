using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Api.Model;

/// <summary>
/// The retro payrun job API object
/// </summary>
public class RetroPayrunJob : ApiObjectBase
{
    /// <summary>
    /// The schedule date
    /// </summary>
    [Required]
    public DateTime ScheduleDate { get; set; }

    /// <summary>
    /// The result tags
    /// </summary>
    public List<string> ResultTags { get; set; }

    /// <inheritdoc/>
    public override string ToString() =>
        $"{ScheduleDate}] {base.ToString()}";
}