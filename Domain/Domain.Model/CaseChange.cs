using System;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Case change with multiple case values of one case type
/// </summary>
public class CaseChange : DomainObjectBase, IEquatable<CaseChange>
{
    /// <summary>
    /// The change user id
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// The employee id, mandatory for employee case changes (immutable)
    /// </summary>
    public int? EmployeeId { get; set; }

    /// <summary>
    /// The division id (immutable)
    /// If present, this values overrides all case value divisions  <see cref="CaseValue.DivisionId"/>
    /// </summary>
    public int? DivisionId { get; set; }

    /// <summary>
    /// The cancellation type
    /// </summary>
    public CaseCancellationType CancellationType { get; set; }

    /// <summary>
    /// The cancellation case id (immutable)
    /// </summary>
    public int? CancellationId { get; set; }

    /// <summary>
    /// The cancellation date (immutable)
    /// </summary>
    public DateTime? CancellationDate { get; set; }

    /// <summary>
    /// The change reason
    /// </summary>
    public string Reason { get; set; }

    /// <summary>
    /// The validation case name
    /// </summary>
    public string ValidationCaseName { get; set; }

    /// <summary>
    /// The forecast name
    /// </summary>
    public string Forecast { get; set; }

    /// <summary>
    /// The case values
    /// </summary>
    public List<CaseValue> Values { get; set; }

    /// <inheritdoc/>
    public CaseChange()
    {
    }

    /// <inheritdoc/>
    public CaseChange(CaseChange copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(CaseChange compare) =>
        CompareTool.EqualProperties(this, compare);
}