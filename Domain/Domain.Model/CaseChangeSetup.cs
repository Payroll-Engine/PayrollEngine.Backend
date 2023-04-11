using System;
using System.Collections.Generic;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// Case change setup
/// </summary>
public class CaseChangeSetup : IEquatable<CaseChangeSetup>
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
    /// If present, this values overrides all case value divisions <see cref="CaseValue.DivisionId"/>
    /// </summary>
    public int? DivisionId { get; set; }

    /// <summary>
    /// The case to cancel, root case name specifies the target case
    /// </summary>
    public int? CancellationId { get; set; }

    /// <summary>
    /// Case change created date
    /// </summary>
    public DateTime? Created { get; set; }

    /// <summary>
    /// The change reason
    /// </summary>
    public string Reason { get; set; }

    /// <summary>
    /// The forecast name
    /// </summary>
    public string Forecast { get; set; }

    /// <summary>
    /// The setup root case
    /// </summary>
    public CaseSetup Case { get; set; }

    /// <summary>
    /// The case validation issues
    /// </summary>
    public List<CaseValidationIssue> Issues { get; set; }

    /// <summary>Initializes a new instance of the <see cref="CaseChangeSetup"/> class</summary>
    public CaseChangeSetup()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="CaseChangeSetup"/> class</summary>
    /// <param name="copySource">The copy source</param>
    public CaseChangeSetup(CaseChangeSetup copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(CaseChangeSetup compare) =>
        CompareTool.EqualProperties(this, compare);
}