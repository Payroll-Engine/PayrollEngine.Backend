using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Api.Model;

/// <summary>
/// The payroll layer API object.
/// Payroll layer evaluated order in payrolls:
/// 1. From th highest level to the lowest level
/// 2. Within a level: from th highest priority to the lowest priority
/// </summary>
public class PayrollLayer : ApiObjectBase
{
    /// <summary>
    /// The layer level, used as primary sorting key (descending)
    /// </summary>
    [Required]
    public int Level { get; set; }

    /// <summary>
    /// The layer priority, used as secondary sorting key (descending)
    /// </summary>
    [Required]
    public int Priority { get; set; }

    /// <summary>
    /// The regulation name
    /// </summary>
    [Required]
    public string RegulationName { get; set; }

    /// <summary>
    /// Custom attributes
    /// </summary>
    public Dictionary<string, object> Attributes { get; set; }

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Level}.{Priority} -> {RegulationName} {base.ToString()}";
}