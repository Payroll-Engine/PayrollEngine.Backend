/* CasePayrollValue */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Serialization;

namespace Ason.Payroll.Client.Scripting;

#region Case Value Collections

/// <summary>Dictionary of case value grouped by date period</summary>
public class PeriodCasePayrollValueDictionary : Dictionary<DatePeriod, CasePayrollValue>, IEnumerable<PeriodValue>
{
    private readonly List<PeriodValue> periodValues = new();

    /// <inheritdoc />
    public PeriodCasePayrollValueDictionary(IDictionary<DatePeriod, CasePayrollValue> values) :
        base(values)
    {
        foreach (var value in values)
        {
            periodValues.Add(new(value.Key, value.Value.Value));
        }
    }

    IEnumerator<PeriodValue> IEnumerable<PeriodValue>.GetEnumerator() =>
        periodValues.GetEnumerator();
}

/// <summary>Dictionary of multiple case values grouped by case field name</summary>
public class CasePayrollValueDictionary : Dictionary<string, CasePayrollValue>, IEnumerable<PeriodValue>
{
    private readonly List<PeriodValue> periodValues = new();

    /// <inheritdoc />
    public CasePayrollValueDictionary(IDictionary<string, CasePayrollValue> values) :
        base(values)
    {
        foreach (var value in values)
        {
            var period = value.Value.PeriodValues.GetPeriod();
            periodValues.Add(new(period, value.Value.Value));
        }
    }

    /// <summary>Return true if any value is available</summary>
    public bool HasAnyValue => periodValues.Any(x => x.HasValue);

    /// <summary>Return true if all values are available</summary>
    public bool HasAllValues => periodValues.Any() && periodValues.All(x => x.HasValue);

    IEnumerator<PeriodValue> IEnumerable<PeriodValue>.GetEnumerator() =>
        periodValues.GetEnumerator();
}

/// <summary>Dictionary of multiple case values grouped by period and case field name</summary>
public class MultiPeriodCasePayrollValueDictionary : Dictionary<DatePeriod, CasePayrollValueDictionary>
{
    /// <inheritdoc />
    public MultiPeriodCasePayrollValueDictionary(IDictionary<DatePeriod, CasePayrollValueDictionary> values) :
        base(values)
    {
    }

    /// <summary>Get period case value by case field</summary>
    /// <param name="caseFieldName">The case field name</param>
    /// <returns>Dictionary of case value grouped by date period</returns>
    public PeriodCasePayrollValueDictionary this[string caseFieldName] =>
        GetCaseValue(caseFieldName);

    /// <summary>Get period case value by case field</summary>
    /// <param name="caseFieldName">The case field name</param>
    /// <returns>Dictionary of case value grouped by date period</returns>
    public PeriodCasePayrollValueDictionary GetCaseValue(string caseFieldName)
    {
        if (string.IsNullOrWhiteSpace(caseFieldName))
        {
            throw new ArgumentException(nameof(caseFieldName));
        }

        var values = new Dictionary<DatePeriod, CasePayrollValue>();
        foreach (var periodValues in this)
        {
            var periodValue = periodValues.Value[caseFieldName];
            if (periodValue == null)
            {
                throw new ScriptException($"Unknown case field {caseFieldName}");
            }
            values.Add(periodValues.Key, periodValue);
        }
        return new(values);
    }
}

#endregion

/// <summary>Payroll value for a date period</summary>
public class CasePayrollValue : PayrollValue, IEnumerable<PeriodValue>
{
    /// <summary>The case field name</summary>
    public string CaseFieldName { get; }

    /// <summary>The period values</summary>
    public ReadOnlyCollection<PeriodValue> PeriodValues { get; }

    /// <summary>Initializes a new instance of the <see cref="CasePayrollValue"/> class without values</summary>
    /// <param name="caseFieldName">Name of the case field</param>
    public CasePayrollValue(string caseFieldName)
    {
        CaseFieldName = caseFieldName;
    }

    /// <summary>Initializes a new instance of the <see cref="CasePayrollValue"/> class</summary>
    /// <param name="caseFieldName">Name of the case field</param>
    /// <param name="periodValues">The period values</param>
    public CasePayrollValue(string caseFieldName, IEnumerable<PeriodValue> periodValues)
    {
        if (string.IsNullOrWhiteSpace(caseFieldName))
        {
            throw new ArgumentException(nameof(caseFieldName));
        }
        if (periodValues == null)
        {
            throw new ArgumentNullException(nameof(periodValues));
        }

        CaseFieldName = caseFieldName;
        PeriodValues = new(periodValues.ToList());
    }

    /// <summary>The value</summary>
    protected override object CurrentValue => TotalValue().Value;

    /// <summary>Test if the payroll value is defined</summary>
    [JsonIgnore]
    public override bool HasValue => PeriodValues != null && PeriodValues.Any();

    /// <summary>Test for period values</summary>
    [JsonIgnore]
    public bool HasPeriods => HasValue;

    /// <summary>Total payroll value, summary of period values</summary>
    public PayrollValue TotalValue()
    {
        // no values
        if (!HasValue)
        {
            return Empty;
        }
        // one value
        if (PeriodValues.Count == 1)
        {
            return PeriodValues[0];
        }
        // multiple values
        var total = PeriodValues.First() as PayrollValue;
        foreach (var value in PeriodValues)
        {
            if (value != PeriodValues.First())
            {
                total += value;
            }
        }
        return total;
    }

    /// <summary>Get the case value key</summary>
    /// <returns>Case value key</returns>
    public Key ToKey() => new(Value);

    #region Casting operators

    /// <summary>Convert case value to string/></summary>
    public static implicit operator string(CasePayrollValue payrollValue) =>
        (PayrollValue)payrollValue;

    /// <summary>Convert case value to int/></summary>
    public static implicit operator int(CasePayrollValue payrollValue) =>
        (PayrollValue)payrollValue;

    /// <summary>Convert case value to nullable int</summary>
    public static implicit operator int?(CasePayrollValue payrollValue) =>
        (PayrollValue)payrollValue;

    /// <summary>Convert case value to decimal</summary>
    public static implicit operator decimal(CasePayrollValue payrollValue) =>
        (PayrollValue)payrollValue;

    /// <summary>Convert case value to nullable decimal</summary>
    public static implicit operator decimal?(CasePayrollValue payrollValue) =>
        (PayrollValue)payrollValue;

    /// <summary>Convert case value to DateTime</summary>
    public static implicit operator DateTime(CasePayrollValue payrollValue) =>
        (PayrollValue)payrollValue;

    /// <summary>Convert case value to nullable DateTime</summary>
    public static implicit operator DateTime?(CasePayrollValue payrollValue) =>
        (PayrollValue)payrollValue;

    #endregion

    #region Binary operators

    /// <summary>Addition of two case values</summary>
    public static PayrollValue operator +(CasePayrollValue left, CasePayrollValue right)
    {
        PayrollValue result = null;
        for (var i = 0; i < left.PeriodValues.Count; i++)
        {
            var periodResult = left.PeriodValues[i] + right.PeriodValues[i];
            result = AddToResult(result, periodResult);
        }
        return result;
    }

    /// <summary>Addition of a case value with a period value</summary>
    public static PayrollValue operator +(CasePayrollValue left, PeriodValue right)
    {

        PayrollValue result = null;
        foreach (var value in left.PeriodValues)
        {
            var periodResult = value + right;
            result = AddToResult(result, periodResult);
        }
        return result;
    }

    /// <summary>Addition of a case value with a payroll value</summary>
    public static PayrollValue operator +(CasePayrollValue left, PayrollValue right)
    {
        PayrollValue result = null;
        foreach (var value in left.PeriodValues)
        {
            var periodResult = value + right;
            result = AddToResult(result, periodResult);
        }
        return result;
    }

    /// <summary>Subtraction of two case values</summary>
    public static PayrollValue operator -(CasePayrollValue left, CasePayrollValue right)
    {
        PayrollValue result = null;
        for (var i = 0; i < left.PeriodValues.Count; i++)
        {
            var periodResult = left.PeriodValues[i] - right.PeriodValues[i];
            result = AddToResult(result, periodResult);
        }
        return result;
    }

    /// <summary>Subtraction of a case value with a period value</summary>
    public static PayrollValue operator -(CasePayrollValue left, PeriodValue right)
    {
        PayrollValue result = null;
        foreach (var value in left.PeriodValues)
        {
            var periodResult = value - right;
            result = AddToResult(result, periodResult);
        }
        return result;
    }

    /// <summary>Subtraction of a case value with a payroll value</summary>
    public static PayrollValue operator -(CasePayrollValue left, PayrollValue right)
    {
        PayrollValue result = null;
        foreach (var value in left.PeriodValues)
        {
            var periodResult = value - right;
            result = AddToResult(result, periodResult);
        }
        return result;
    }

    /// <summary>Multiplication of two case values</summary>
    public static PayrollValue operator *(CasePayrollValue left, CasePayrollValue right)
    {
        PayrollValue result = null;
        for (var i = 0; i < left.PeriodValues.Count; i++)
        {
            var periodResult = left.PeriodValues[i] * right.PeriodValues[i];
            result = AddToResult(result, periodResult);
        }
        return result;
    }

    /// <summary>Multiplication of a case value with a period value</summary>
    public static PayrollValue operator *(CasePayrollValue left, PeriodValue right)
    {
        PayrollValue result = null;
        foreach (var value in left.PeriodValues)
        {
            var periodResult = value * right;
            result = AddToResult(result, periodResult);
        }
        return result;
    }

    /// <summary>Multiplication of a case value with a payroll value</summary>
    public static PayrollValue operator *(CasePayrollValue left, PayrollValue right)
    {
        PayrollValue result = null;
        foreach (var value in left.PeriodValues)
        {
            var periodResult = value * right;
            result = AddToResult(result, periodResult);
        }
        return result;
    }

    /// <summary>Division of two case values</summary>
    public static PayrollValue operator /(CasePayrollValue left, CasePayrollValue right)
    {
        PayrollValue result = null;
        for (var i = 0; i < left.PeriodValues.Count; i++)
        {
            var periodResult = left.PeriodValues[i] / right.PeriodValues[i];
            result = AddToResult(result, periodResult);
        }
        return result;
    }

    /// <summary>Division of a case value with a period value</summary>
    public static PayrollValue operator /(CasePayrollValue left, PeriodValue right)
    {

        PayrollValue result = null;
        foreach (var value in left.PeriodValues)
        {
            var periodResult = value / right;
            result = AddToResult(result, periodResult);
        }
        return result;
    }

    /// <summary>Division of a case value with a payroll value</summary>
    public static PayrollValue operator /(CasePayrollValue left, PayrollValue right)
    {
        PayrollValue result = null;
        foreach (var value in left.PeriodValues)
        {
            var periodResult = value / right;
            result = AddToResult(result, periodResult);
        }
        return result;
    }

    /// <summary>Remainder of two case values</summary>
    public static PayrollValue operator %(CasePayrollValue left, CasePayrollValue right)
    {
        PayrollValue result = null;
        for (var i = 0; i < left.PeriodValues.Count; i++)
        {
            var periodResult = left.PeriodValues[i] % right.PeriodValues[i];
            result = AddToResult(result, periodResult);
        }
        return result;
    }

    /// <summary>Remainder of a case value with a period value</summary>
    public static PayrollValue operator %(CasePayrollValue left, PeriodValue right)
    {

        PayrollValue result = null;
        foreach (var value in left.PeriodValues)
        {
            var periodResult = value % right;
            result = AddToResult(result, periodResult);
        }
        return result;
    }

    /// <summary>Remainder of a case value with a payroll value</summary>
    public static PayrollValue operator %(CasePayrollValue left, PayrollValue right)
    {
        PayrollValue result = null;
        foreach (var value in left.PeriodValues)
        {
            var periodResult = value % right;
            result = AddToResult(result, periodResult);
        }
        return result;
    }

    /// <summary>Addition of two payroll values (decimal, int, string)</summary>
    private static PayrollValue AddToResult(PayrollValue result, PayrollValue value)
    {
        if (result == null)
        {
            result = value;
        }
        else
        {
            result += value;
        }
        return result;
    }

    #endregion

    /// <summary>Returns an enumerator that iterates through the collection</summary>
    /// <returns>An enumerator that can be used to iterate through the collection</returns>
    public IEnumerator<PeriodValue> GetEnumerator() =>
        PeriodValues.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() =>
        GetEnumerator();

    /// <summary>Returns a <see cref="string" /> that represents this instance</summary>
    /// <returns>A <see cref="string" /> that represents this instance</returns>
    public override string ToString() =>
        HasValue ? $"{Value} ({PeriodValues.Count} periods)" : base.ToString();
}