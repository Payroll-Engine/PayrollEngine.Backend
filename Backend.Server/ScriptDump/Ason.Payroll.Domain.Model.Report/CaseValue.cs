/* CaseValue */
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ason.Payroll.Client.Scripting;

/// <summary>National, company or employee case value</summary>
public class CaseValue : IEquatable<CaseValue>
{
    /// <summary>The case field name</summary>
    public string CaseFieldName { get; }

    /// <summary>The created date</summary>
    public DateTime Created { get; set; }

    /// <summary>The period start date</summary>
    public DateTime? Start { get; set; }

    /// <summary>The period end date</summary>
    public DateTime? End { get; set; }

    /// <summary>The case period value as JSON</summary>
    public PayrollValue Value { get; }

    /// <summary>Cancellation date</summary>
    public DateTime? CancellationDate { get; set; }

    /// <summary>The tags</summary>
    public List<string> Tags { get; set; }

    /// <summary>The attributes</summary>
    public Dictionary<string, object> Attributes { get; set; }

    /// <summary>Initializes a new instance</summary>
    public CaseValue()
    {
    }

    /// <summary>Initializes a new instance from a copy</summary>
    /// <param name="copySource">The copy source</param>
    public CaseValue(CaseValue copySource)
    {
        CaseFieldName = copySource.CaseFieldName;
        Created = copySource.Created;
        Start = copySource.Start;
        End = copySource.End;
        Value = copySource.Value;
        CancellationDate = copySource.CancellationDate;
        Tags = copySource.Tags;
        Attributes = copySource.Attributes;
    }

    /// <summary>Initializes a new instance</summary>
    /// <param name="caseFieldName">The case field name</param>
    /// <param name="created">The created date</param>
    /// <param name="start">The start date</param>
    /// <param name="end">The end date</param>
    /// <param name="value">The value</param>
    /// <param name="cancellationDate">Cancellation date</param>
    /// <param name="tags">The tags</param>
    /// <param name="attributes">The attributes</param>
    public CaseValue(string caseFieldName, DateTime created, DateTime? start, DateTime? end,
        PayrollValue value, DateTime? cancellationDate = null, List<string> tags = null,
        Dictionary<string, object> attributes = null)
    {
        if (string.IsNullOrWhiteSpace(caseFieldName))
        {
            throw new ArgumentException(nameof(caseFieldName));
        }

        CaseFieldName = caseFieldName;
        Created = created;
        Start = start;
        End = end;
        Value = value;
        CancellationDate = cancellationDate;
        Tags = tags;
        Attributes = attributes;
    }

    /// <summary>Compare two objects</summary>
    /// <param name="compare">The object to compare with this</param>
    /// <returns>True for objects with the same data</returns>
    public bool Equals(CaseValue compare)
    {
        if (ReferenceEquals(null, compare))
        {
            return false;
        }
        if (ReferenceEquals(this, compare))
        {
            return true;
        }

        return
            string.Equals(CaseFieldName, compare.CaseFieldName) &&
            Created == compare.Created &&
            Start == compare.Start &&
            End == compare.End &&
            Value == compare.Value &&
            CancellationDate == compare.CancellationDate &&
            (Tags?.SequenceEqual(compare.Tags) ?? compare.Tags != null) &&
            (Attributes?.SequenceEqual(compare.Attributes) ?? compare.Attributes != null);
    }

    /// <summary>Returns a <see cref="string" /> that represents this instance</summary>
    /// <returns>A <see cref="string" /> that represents this instance</returns>
    public override string ToString() =>
        $"{CaseFieldName}: {Start?.ToPeriodStartString()} - {End?.ToPeriodEndString()}: {Value}";
}