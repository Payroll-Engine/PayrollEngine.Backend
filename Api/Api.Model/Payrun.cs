// ReSharper disable UnusedAutoPropertyAccessor.Global
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PayrollEngine.Api.Model;

/// <summary>
/// The payrun API object
/// </summary>
public class Payrun : ApiObjectBase
{
    /// <summary>
    /// The payroll id (immutable)
    /// </summary>
    public int PayrollId { get; set; }

    /// <summary>
    /// The payrun name (immutable)
    /// </summary>
    [Required]
    [StringLength(128)]
    public string Name { get; set; }

    /// <summary>
    /// The localized payrun name
    /// </summary>
    public Dictionary<string, string> NameLocalizations { get; set; }

    /// <summary>
    /// The default payrun reason
    /// </summary>
    public string DefaultReason { get; set; }

    /// <summary>
    /// The localized default payrun reasons
    /// </summary>
    [Localization(nameof(DefaultReason))]
    public Dictionary<string, string> DefaultReasonLocalizations { get; set; }

    /// <summary>
    /// The payrun start expression
    /// </summary>
    public string StartExpression { get; set; }

    /// <summary>
    /// The employee available expression
    /// </summary>
    public string EmployeeAvailableExpression { get; set; }

    /// <summary>
    /// The expression evaluates the employee start
    /// </summary>
    public string EmployeeStartExpression { get; set; }

    /// <summary>
    /// The expression evaluates the employee end
    /// </summary>
    public string EmployeeEndExpression { get; set; }

    /// <summary>
    /// The wage type available expression
    /// </summary>
    public string WageTypeAvailableExpression { get; set; }

    /// <summary>
    /// The payrun end expression
    /// </summary>
    public string EndExpression { get; set; }

    /// <summary>
    /// The number of previous cycles allowed for retro calculation.
    /// 0  = current cycle only (no back cycles).
    /// n  = retro may reach back n complete cycles before the current one.
    /// -1 = unlimited (no cycle boundary) — default.
    /// </summary>
    public int RetroBackCycles { get; set; } = -1;

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Name} {base.ToString()}";
}