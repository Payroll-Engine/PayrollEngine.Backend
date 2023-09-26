using System;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A payroll result
/// </summary>
public class PayrollResult : DomainObjectBase, IEquatable<PayrollResult>
{
    /// <summary>
    /// The payroll id (immutable)
    /// </summary>
    public int PayrollId { get; set; }

    /// <summary>
    /// The payrun id (immutable)
    /// </summary>
    public int PayrunId { get; set; }

    /// <summary>
    /// The payrun job id (immutable)
    /// </summary>
    public int PayrunJobId { get; set; }

    /// <summary>
    /// The employee id (immutable)
    /// </summary>
    public int EmployeeId { get; set; }

    /// <summary>
    /// The division id (immutable)
    /// </summary>
    public int DivisionId { get; set; }

    /// <summary>
    /// The cycle name (immutable)
    /// </summary>
    public string CycleName { get; set; }

    /// <summary>
    /// The cycle start date (immutable)
    /// </summary>
    public DateTime CycleStart { get; set; }

    /// <summary>
    /// The cycle end date (immutable)
    /// </summary>
    public DateTime CycleEnd { get; set; }

    /// <summary>
    /// The period name (immutable)
    /// </summary>
    public string PeriodName { get; set; }

    /// <summary>
    /// The period start date (immutable)
    /// </summary>
    public DateTime PeriodStart { get; set; }

    /// <summary>
    /// The period end date (immutable)
    /// </summary>
    public DateTime PeriodEnd { get; set; }

    /// <inheritdoc/>
    public PayrollResult()
    {
    }

    /// <inheritdoc/>
    protected PayrollResult(PayrollResult copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(PayrollResult compare) =>
        CompareTool.EqualProperties(this, compare);

    /// <inheritdoc/>
    public override string ToString() =>
        $"result: {PeriodName} for employee {EmployeeId} on division {DivisionId} {base.ToString()}";
}