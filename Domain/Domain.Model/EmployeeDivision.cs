using System;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// An employee division
/// </summary>
public class EmployeeDivision : DomainObjectBase, IEquatable<EmployeeDivision>
{
    /// <summary>
    /// The employee id
    /// </summary>
    public int EmployeeId { get; set; }

    /// <summary>
    /// The division id
    /// </summary>
    public int DivisionId { get; set; }

    /// <inheritdoc/>
    public EmployeeDivision()
    {
    }

    /// <inheritdoc/>
    public EmployeeDivision(EmployeeDivision copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(EmployeeDivision compare) =>
        CompareTool.EqualProperties(this, compare);

    /// <inheritdoc/>
    public override string ToString() =>
        $"{EmployeeId} > {DivisionId} {base.ToString()}";
}