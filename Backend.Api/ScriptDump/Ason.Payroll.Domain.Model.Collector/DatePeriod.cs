/* DatePeriod */
using System;
using System.Text.Json.Serialization;

namespace Ason.Payroll.Client.Scripting;

/// <summary>An immutable date period between the start and end date</summary>
public sealed class DatePeriod
{
    /// <summary>Create a full period</summary>
    public DatePeriod()
    {
        Start = Date.MinValue;
        End = Date.MaxValue;
        HasStart = GetHasStart();
        HasEnd = GetHasEnd();
    }

    /// <summary>Create a period from start to end</summary>
    public DatePeriod(DateTime start, DateTime end)
    {
        if (end < start)
        {
            Start = end;
            End = start;
        }
        else
        {
            Start = start;
            End = end;
        }
        HasStart = GetHasStart();
        HasEnd = GetHasEnd();
    }

    /// <summary>Create a period from conditional start to conditional end</summary>
    public DatePeriod(DateTime? start, DateTime? end)
    {
        if (start.HasValue && end.HasValue)
        {
            if (end.Value < start.Value)
            {
                Start = end.Value;
                End = start.Value;
            }
            else
            {
                Start = start.Value;
                End = end.Value;
            }
        }
        else if (start.HasValue)
        {
            Start = start.Value;
            End = Date.MaxValue;
        }
        else if (end.HasValue)
        {
            Start = Date.MinValue;
            End = end.Value;
        }
        HasStart = GetHasStart();
        HasEnd = GetHasEnd();
    }

    // internal
    private bool GetHasStart() => Start != Date.MinValue;
    private bool GetHasEnd() => End != Date.MaxValue;

    /// <summary>The period start</summary>
    public DateTime Start { get; }

    /// <summary>Check for start</summary>
    public bool HasStart { get; }

    /// <summary>The period end</summary>
    public DateTime End { get; }

    /// <summary>Check for end</summary>
    [JsonIgnore]
    public bool HasEnd { get; }

    /// <summary>Test for open period (no start or no end)</summary>
    [JsonIgnore]
    public bool IsOpen => !HasStart || !HasEnd;

    /// <summary>Test for anytime period (without start and end)</summary>
    [JsonIgnore]
    public bool IsAnytime => !HasStart && !HasEnd;

    /// <summary>Test if start and end are equal</summary>
    [JsonIgnore]
    public bool IsMoment => Start.Equals(End);

    /// <summary>Test if start and end are UTC</summary>
    [JsonIgnore]
    public bool IsUtc => Start.IsUtc() && End.IsUtc();

    /// <summary>The period duration, only for closed period</summary>
    [JsonIgnore]
    public TimeSpan Duration => End - Start;

    /// <summary>Test if the period is within a year</summary>
    [JsonIgnore]
    public bool IsWithinYear => Start.IsSameYear(End);

    /// <summary>Test if the period is within a month</summary>
    [JsonIgnore]
    public bool IsWithinMonth => Start.IsSameMonth(End);

    /// <summary>Test if the period is within a day</summary>
    [JsonIgnore]
    public bool IsWithinDay => Start.IsSameDay(End);

    /// <summary>Test if the period is within a hour</summary>
    [JsonIgnore]
    public bool IsWithinHour => Start.IsSameHour(End);

    /// <summary>Gets the value of the <see cref="T:System.TimeSpan" /> between start and end expressed in whole and fractional days</summary>
    /// <returns>The total number of days represented by this instance</returns>
    [JsonIgnore]
    public double TotalDays => Duration.TotalDays;

    /// <summary>Gets the value of the <see cref="T:System.TimeSpan" /> between start and end expressed in whole and fractional hours</summary>
    /// <returns>The total number of hours represented by this instance</returns>
    [JsonIgnore]
    public double TotalHours => Duration.TotalHours;

    /// <summary>Gets the value of the <see cref="T:System.TimeSpan" /> between start and end expressed in whole and fractional minutes</summary>
    /// <returns>The minutes number of hours represented by this instance</returns>
    [JsonIgnore]
    public double TotalMinutes => Duration.TotalMinutes;

    #region Object

    /// <summary>Determines whether the specified <see cref="object" /> is equal to this instance</summary>
    /// <param name="source">The object to compare with the current object</param>
    /// <returns>True if the specified <see cref="object" /> is equal to this instance</returns>
    public override bool Equals(object source)
    {
        var compare = source as DatePeriod;
        if (compare == null)
        {
            return false;
        }
        if (ReferenceEquals(this, compare))
        {
            return true;
        }
        return Start.Equals(compare.Start) &&
               End.Equals(compare.End);
    }

    /// <summary>Returns a hash code for this instance</summary>
    /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table</returns>
    public override int GetHashCode() =>
        (Start, End).GetHashCode();

    /// <summary>Compare two date periods for equal values</summary>
    /// <param name="left">The left period to compare</param>
    /// <param name="right">The right period to compare</param>
    /// <returns>True if the periods are equal</returns>
    public static bool operator ==(DatePeriod left, DatePeriod right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }
        if (left is null || right is null)
        {
            return false;
        }
        return left.Equals(right);
    }

    /// <summary>Compare two date periods for different values</summary>
    /// <param name="left">The left period to compare</param>
    /// <param name="right">The right period to compare</param>
    /// <returns>True if the periods are different</returns>
    public static bool operator !=(DatePeriod left, DatePeriod right) =>
        !(left == right);

    /// <summary>Returns a <see cref="string" /> that represents this instance</summary>
    /// <returns>A <see cref="string" /> that represents this instance</returns>
    public override string ToString() =>
        $"{Start.ToPeriodStartString()} - {End.ToPeriodEndString()}";

    #endregion

}