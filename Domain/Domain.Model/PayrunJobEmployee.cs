using System;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A payrun job employee
/// </summary>
// ReSharper disable PropertyCanBeMadeInitOnly.Global
public class PayrunJobEmployee : DomainObjectBase, IEquatable<PayrunJobEmployee>
{
    /// <summary>
    /// The employee id (immutable)
    /// </summary>
    public int EmployeeId { get; set; }

    /// <inheritdoc/>
    public PayrunJobEmployee()
    {
    }

    /// <inheritdoc/>
    public PayrunJobEmployee(PayrunJobEmployee copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(PayrunJobEmployee compare) =>
        CompareTool.EqualProperties(this, compare);
}