using System;

namespace PayrollEngine.Domain.Model;

/// <summary>
/// A case value from a case change used in national, company and employee case
/// </summary>
public class CaseChangeCaseValue : CaseValue, IEquatable<CaseChangeCaseValue>
{
    /// <summary>
    /// The case change id
    /// </summary>
    public int CaseChangeId { get; set; }

    /// <summary>
    /// The case change creation
    /// </summary>
    public DateTime CaseChangeCreated { get; set; }

    /// <summary>
    /// The change user id
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// The user unique identifier
    /// </summary>
    public string UserIdentifier { get; set; }

    /// <summary>
    /// The change reason
    /// </summary>
    public string Reason { get; set; }

    /// <summary>
    /// The validation case name
    /// </summary>
    public string ValidationCaseName { get; set; }

    /// <summary>
    /// The cancellation type
    /// </summary>
    public CaseCancellationType CancellationType { get; set; }

    /// <summary>
    /// The canceled case change id
    /// </summary>
    public int? CancellationId { get; set; }

    /// <summary>
    /// The document count
    /// </summary>
    public int Documents { get; set; }

    /// <summary>
    /// The result kind
    /// </summary>
    public ValueType ResultKind
    {
        get => ValueType;
        set => ValueType = value;
    }

    /// <summary>
    /// The result value
    /// </summary>
    public string ResultValue
    {
        get => Value;
        set => Value = value;
    }

    /// <inheritdoc/>
    public CaseChangeCaseValue()
    {
    }

    /// <inheritdoc/>
    public CaseChangeCaseValue(CaseChangeCaseValue copySource) :
        base(copySource)
    {
        CopyTool.CopyProperties(copySource, this);
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(CaseChangeCaseValue compare) =>
        CompareTool.EqualProperties(this, compare);
}