/* PeriodValue */
using System;

namespace Ason.Payroll.Client.Scripting;

/// <summary>Payroll value for a date period</summary>
public class PeriodValue : PayrollValue
{
    /// <summary>New period payroll value instance</summary>
    protected PeriodValue()
    {
    }

    /// <summary>New period payroll value instance</summary>
    public PeriodValue(DatePeriod period, object value) :
        base(value)
    {
        Start = period.Start;
        End = period.End;
        Period = new(Start, End.Value.RoundTickToHour());
    }

    /// <summary>New period payroll value instance</summary>
    public PeriodValue(DateTime? start, DateTime? end, object value) :
        base(value)
    {
        Start = start;
        End = end;
        if (end.HasValue)
        {
            Period = new(start, end.Value.RoundTickToHour());
        }
    }

    /// <summary>The period start</summary>
    public DateTime? Start { get; }

    /// <summary>The period end</summary>
    public DateTime? End { get; }

    /// <summary>The period</summary>
    public DatePeriod Period { get; }

    #region Casting operators

    /// <summary>Convert case value to string/></summary>
    public static implicit operator string(PeriodValue value) =>
        (PayrollValue)value;

    /// <summary>Convert case value to int/></summary>
    public static implicit operator int(PeriodValue value) =>
        (PayrollValue)value;

    /// <summary>Convert case value to nullable int</summary>
    public static implicit operator int?(PeriodValue value) =>
        (PayrollValue)value;

    /// <summary>Convert case value to decimal</summary>
    public static implicit operator decimal(PeriodValue value) =>
        (PayrollValue)value;

    /// <summary>Convert case value to nullable decimal</summary>
    public static implicit operator decimal?(PeriodValue value) =>
        (PayrollValue)value;

    /// <summary>Convert case value to DateTime</summary>
    public static implicit operator DateTime(PeriodValue value) =>
        (PayrollValue)value;

    /// <summary>Convert case value to nullable DateTime</summary>
    public static implicit operator DateTime?(PeriodValue value) =>
        (PayrollValue)value;

    #endregion

    /// <summary>Returns a <see cref="string" /> that represents this instance</summary>
    /// <returns>A <see cref="string" /> that represents this instance</returns>
    public override string ToString() =>
        $"{Value} ({Period})";
}