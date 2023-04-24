/* Tools */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Ason.Payroll.Client.Scripting;

#region Key

/// <summary>Represent a key for lookups or results</summary>
public class Key
{
    /// <summary>The key values</summary>
    public object[] Values { get; }

    /// <summary>Initializes a new instance of the <see cref="Key"/> class</summary>
    /// <param name="values">The values</param>
    public Key(params object[] values)
    {
        if (values != null && values.Length > 0)
        {
            var keyValues = new List<object>();
            foreach (var value in values)
            {
                if (value is PayrollValue payrollValue)
                {
                    // extract payroll value
                    keyValues.Add(payrollValue.Value);
                }
                else if (value is string stringValue && !string.IsNullOrWhiteSpace(stringValue))
                {
                    // ignore empty strings
                    keyValues.Add(value);
                }
                else if (value != null)
                {
                    // any other value
                    keyValues.Add(value);
                }
            }
            Values = keyValues.ToArray();
        }
    }

    /// <summary>Convert values to key</summary>
    public static implicit operator string(Key key) =>
        key?.ToString();

    /// <summary>Returns a key as string</summary>
    public override string ToString() =>
        Values != null ? JsonSerializer.Serialize(Values) : null;
}

#endregion

#region CheckDigit

/// <summary>Compute and validate text value digits (ISO 7064 compatible)</summary>
/// <remarks>See https://github.com/gravity00/SimpleISO7064 and https://en.wikipedia.org/wiki/International_Bank_Account_Number </remarks>
public class CheckDigit
{
    private readonly int numCheckDigits;

    /// <summary>Check numeric strings with one check digit or the supplementary check character "X"</summary>
    public static CheckDigit Mod11Radix2 => new(11, 2, false, "0123456789X");

    /// <summary>Check alphanumeric strings with one check digit or letter or the supplementary check character "*"</summary>
    // ReSharper disable once StringLiteralTypo
    public static CheckDigit Mod37Radix2 => new(137, 2, false, "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ*");

    /// <summary>Check numeric strings with two check digits</summary>
    public static CheckDigit Mod97Radix10 => new(97, 10, true, "0123456789");

    /// <summary>Check alphabetic strings with two check letters</summary>
    // ReSharper disable once StringLiteralTypo
    public static CheckDigit Mod661Radix26 => new(661, 26, true, "ABCDEFGHIJKLMNOPQRSTUVWXYZ");

    /// <summary>Check alphabetic strings with two check letters</summary>
    public static CheckDigit Mod1271Radix36 => new(1271, 36, true, "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ");

    /// <summary>Check UPC-A digit</summary>
    public static bool IsValidUpcA(string value) => IsWeightedValid(value, 1);

    /// <summary>Check EAN-13 digit</summary>
    public static bool IsValidEan13(string value) => IsWeightedValid(value, 1);

    /// <summary>Check ITF digit</summary>
    public static bool IsValidItf(string value) => IsWeightedValid(value, 1);

    /// <summary>Creates a new instance</summary>
    /// <param name="modulus">The modulus</param>
    /// <param name="radix">The radix</param>
    /// <param name="doubleCheckDigit">Is the computed check digit composed by two characters?</param>
    /// <param name="characterSet">The supported character set</param>
    public CheckDigit(int modulus, int radix, bool doubleCheckDigit, string characterSet)
    {
        if (modulus <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(modulus));
        }
        if (radix <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(radix));
        }
        if (characterSet == null)
        {
            throw new ArgumentNullException(nameof(characterSet));
        }
        if (string.IsNullOrWhiteSpace(characterSet))
        {
            throw new ArgumentException(nameof(characterSet));
        }

        Modulus = modulus;
        Radix = radix;
        DoubleCheckDigit = doubleCheckDigit;
        CharacterSet = characterSet;
        numCheckDigits = DoubleCheckDigit ? 2 : 1;
    }

    /// <summary>The modulus</summary>
    public int Modulus { get; }

    /// <summary>The radix</summary>
    public int Radix { get; }

    /// <summary>Double check digit</summary>
    public bool DoubleCheckDigit { get; }

    /// <summary>The supported character set</summary>
    public string CharacterSet { get; }

    /// <summary>Checks if the given value contains a valid check digit</summary>
    /// <param name="value">The value to check</param>
    /// <returns>True for a valid value</returns>
    public bool IsValid(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(nameof(value));
        }
        if (value.Length <= numCheckDigits)
        {
            throw new ArgumentException($"Value length should be greater than {numCheckDigits}", nameof(value));
        }

        var checkedValue = AddCheckDigit(value.Substring(0, value.Length - numCheckDigits));
        return string.Equals(value, checkedValue);
    }

    /// <summary>Adds the check digit to the given value</summary>
    /// <param name="value">The value from which the check digit will be computed</param>
    /// <returns>The value and appended check digit</returns>
    public string AddCheckDigit(string value) =>
        string.Concat(value, CalculateCheckDigit(value));

    /// <summary>Calculates the check digit from a given value</summary>
    /// <param name="value">The value from which the check digit will be computed</param>
    /// <returns>The check digit</returns>
    public string CalculateCheckDigit(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(nameof(value));
        }

        value = value.ToUpperInvariant();
        var calculation = 0;
        foreach (var valueDigit in value)
        {
            var indexToAdd = CharacterSet.IndexOf(valueDigit);
            if (indexToAdd < 0)
            {
                throw new ArgumentException($"Found illegal character '{valueDigit}'", nameof(value));
            }
            calculation = ((calculation + indexToAdd) * Radix) % Modulus;
        }

        if (DoubleCheckDigit)
        {
            calculation = (calculation * Radix) % Modulus;
        }

        var checksum = (Modulus - calculation + 1) % Modulus;

        // single check digit
        if (!DoubleCheckDigit)
        {
            return checksum >= CharacterSet.Length ? null : CharacterSet[checksum].ToString();
        }

        // double check digits
        var secondPosition = checksum % Radix;
        var firstPosition = (checksum - secondPosition) / Radix;
        return string.Concat(CharacterSet[firstPosition], CharacterSet[secondPosition]);
    }

    #region Weighted Check Digit

    /// <summary>Checks if the given value contains a valid check digit</summary>
    /// <param name="value">The value to check</param>
    /// <param name="numCheckDigits">The number of check digits</param>
    /// <returns>True for a valid value</returns>
    private static bool IsWeightedValid(string value, int numCheckDigits)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException(nameof(value));
        }

        var valueWithoutCheck = value.Substring(0, value.Length - numCheckDigits);
        var checkedValue = string.Concat(valueWithoutCheck, CalculateWeightedCheckDigit(value));
        return string.Equals(value, checkedValue);
    }

    /// <summary>Calculate check digit for UPC-A, EAN-13, ITF-14 (or any ITF)</summary>
    /// <remarks>See https://stackoverflow.com/a/58465929 </remarks>
    /// <param name="value">The value to check</param>
    /// <returns>True for a valid value</returns>
    private static char CalculateWeightedCheckDigit(string value)
    {
        // Don't include check digit in the sum (< code.Length - 1)
        var sum = 0;
        for (var i = 0; i < value.Length - 1; i++)
        {
            sum += (value[i] - '0') * (((i + value.Length % 2) % 2 == 0) ? 3 : 1);

        }
        return (char)((10 - (sum % 10)) % 10 + '0');
    }

    #endregion

}

#endregion

#region Query

/// <summary>Result query base</summary>
public abstract class ResultQueryBase
{
    /// <summary>Initializes a new instance of the <see cref="ResultQueryBase"/> class</summary>
    /// <param name="jobStatus">The status query</param>
    protected ResultQueryBase(PayrunJobStatus? jobStatus = null)
    {
        JobStatus = jobStatus;
    }

    /// <summary>The forecast name</summary>
    public string Forecast { get; set; }

    /// <summary>Payrun job status filter</summary>
    public PayrunJobStatus? JobStatus { get; set; }

    /// <summary>The result tag</summary>
    public string Tag
    {
        get => Tags?.FirstOrDefault();
        set => Tags = value == null ? null : new() { value };
    }

    /// <summary>The result tags</summary>
    public List<string> Tags { get; set; }
}

/// <summary>Collector result query</summary>
public class CollectorResultQuery : ResultQueryBase
{
    /// <summary>Initializes a new instance of the <see cref="CollectorResultQuery"/> class</summary>
    /// <param name="collectorName">The collector name</param>
    /// <param name="jobJobStatus">The payrun job status query filter</param>
    public CollectorResultQuery(string collectorName, PayrunJobStatus? jobJobStatus = null) :
        base(jobJobStatus)
    {
        Collectors = new() { collectorName };
    }

    /// <summary>Initializes a new instance of the <see cref="CollectorResultQuery"/> class</summary>
    /// <param name="collectorNames">The collector names</param>
    /// <param name="jobJobStatus">The payrun job status query filter</param>
    public CollectorResultQuery(IEnumerable<string> collectorNames, PayrunJobStatus? jobJobStatus = null) :
        base(jobJobStatus)
    {
        Collectors = collectorNames.ToList();
    }

    /// <summary>The collector names</summary>
    public List<string> Collectors { get; }
}

/// <summary>Wage type result query</summary>
public class WageTypeResultQuery : ResultQueryBase
{
    /// <summary>Initializes a new instance of the <see cref="WageTypeResultQuery"/> class</summary>
    /// <param name="wageTypeNumber">The wage type number</param>
    /// <param name="jobJobStatus">The payrun job status query filter</param>
    public WageTypeResultQuery(decimal wageTypeNumber, PayrunJobStatus? jobJobStatus = null) :
        base(jobJobStatus)
    {
        WageTypes = new() { wageTypeNumber };
    }

    /// <summary>Initializes a new instance of the <see cref="WageTypeResultQuery"/> class</summary>
    /// <param name="wageTypeNumbers">The wage type numbers</param>
    /// <param name="jobJobStatus">The payrun job status query filter</param>
    public WageTypeResultQuery(IEnumerable<decimal> wageTypeNumbers, PayrunJobStatus? jobJobStatus = null) :
        base(jobJobStatus)
    {
        WageTypes = wageTypeNumbers.ToList();
    }

    /// <summary>The wage type numbers</summary>
    public List<decimal> WageTypes { get; }
}

/// <summary>Cycle result query</summary>
public abstract class CycleResultQuery : ResultQueryBase
{
    /// <summary>Initializes a new instance of the <see cref="CycleResultQuery"/> class</summary>
    /// <param name="cycleCount">The cycle count</param>
    /// <param name="jobJobStatus">The payrun job status query filter</param>
    protected CycleResultQuery(int cycleCount = 0, PayrunJobStatus? jobJobStatus = null) :
        base(jobJobStatus)
    {
        CycleCount = cycleCount;
    }

    /// <summary>The cycle count</summary>
    public int CycleCount { get; }
}

/// <summary>Collector cycle result query</summary>
public class CollectorCycleResultQuery : CycleResultQuery
{
    /// <summary>Initializes a new instance of the <see cref="CollectorCycleResultQuery"/> class</summary>
    /// <param name="collectorName">The collector name</param>
    /// <param name="cycleCount">The cycle count</param>
    /// <param name="jobJobStatus">The payrun job status query filter</param>
    public CollectorCycleResultQuery(string collectorName, int cycleCount = 0, PayrunJobStatus? jobJobStatus = null) :
        base(cycleCount, jobJobStatus)
    {
        Collectors = new() { collectorName };
    }

    /// <summary>Initializes a new instance of the <see cref="CollectorCycleResultQuery"/> class</summary>
    /// <param name="collectorNames">The collector names</param>
    /// <param name="cycleCount">The cycle count</param>
    /// <param name="jobJobStatus">The payrun job status query filter</param>
    public CollectorCycleResultQuery(IEnumerable<string> collectorNames, int cycleCount = 0, PayrunJobStatus? jobJobStatus = null) :
        base(cycleCount, jobJobStatus)
    {
        Collectors = collectorNames.ToList();
    }

    /// <summary>The collector names</summary>
    public List<string> Collectors { get; }
}

/// <summary>Wage type cycle result query</summary>
public class WageTypeCycleResultQuery : CycleResultQuery
{
    /// <summary>Initializes a new instance of the <see cref="WageTypeCycleResultQuery"/> class</summary>
    /// <param name="wageTypeNumber">The wage type number</param>
    /// <param name="cycleCount">The cycle count</param>
    /// <param name="jobJobStatus">The payrun job status query filter</param>
    public WageTypeCycleResultQuery(decimal wageTypeNumber, int cycleCount = 0, PayrunJobStatus? jobJobStatus = null) :
        base(cycleCount, jobJobStatus)
    {
        WageTypes = new() { wageTypeNumber };
    }

    /// <summary>Initializes a new instance of the <see cref="WageTypeCycleResultQuery"/> class</summary>
    /// <param name="wageTypeNumbers">The wage type numbers</param>
    /// <param name="cycleCount">The cycle count</param>
    /// <param name="jobJobStatus">The payrun job status query filter</param>
    public WageTypeCycleResultQuery(IEnumerable<decimal> wageTypeNumbers, int cycleCount = 0, PayrunJobStatus? jobJobStatus = null) :
        base(cycleCount, jobJobStatus)
    {
        WageTypes = wageTypeNumbers.ToList();
    }

    /// <summary>The wage type numbers</summary>
    public List<decimal> WageTypes { get; }
}

/// <summary>Period result query</summary>
public abstract class PeriodResultQuery : ResultQueryBase
{
    /// <summary>Initializes a new instance of the <see cref="PeriodResultQuery"/> class</summary>
    /// <param name="periodCount">The period count</param>
    /// <param name="jobStatus">The payrun job status query filter</param>
    protected PeriodResultQuery(int periodCount = 0, PayrunJobStatus? jobStatus = null) :
        base(jobStatus)
    {
        PeriodCount = periodCount;
    }

    /// <summary>The period count</summary>
    public int PeriodCount { get; }
}

/// <summary>Collector period result query</summary>
public class CollectorPeriodResultQuery : PeriodResultQuery
{
    /// <summary>Initializes a new instance of the <see cref="CollectorPeriodResultQuery"/> class</summary>
    /// <param name="collectorName">The collector name</param>
    /// <param name="periodCount">The period count</param>
    /// <param name="jobJobStatus">The payrun job status query filter</param>
    public CollectorPeriodResultQuery(string collectorName, int periodCount = 0, PayrunJobStatus? jobJobStatus = null) :
        base(periodCount, jobJobStatus)
    {
        Collectors = new() { collectorName };
    }

    /// <summary>Initializes a new instance of the <see cref="CollectorPeriodResultQuery"/> class</summary>
    /// <param name="collectorNames">The collector names</param>
    /// <param name="periodCount">The period count</param>
    /// <param name="jobJobStatus">The payrun job status query filter</param>
    public CollectorPeriodResultQuery(IEnumerable<string> collectorNames,
        int periodCount = 0, PayrunJobStatus? jobJobStatus = null) :
        base(periodCount, jobJobStatus)
    {
        Collectors = collectorNames.ToList();
    }

    /// <summary>The collector names</summary>
    public List<string> Collectors { get; }
}

/// <summary>Wage type period result query</summary>
public class WageTypePeriodResultQuery : PeriodResultQuery
{
    /// <summary>Initializes a new instance of the <see cref="WageTypePeriodResultQuery"/> class</summary>
    /// <param name="wageTypeNumber">The wage type number</param>
    /// <param name="periodCount">The period count</param>
    /// <param name="jobJobStatus">The payrun job status query filter</param>
    public WageTypePeriodResultQuery(decimal wageTypeNumber, int periodCount = 0, PayrunJobStatus? jobJobStatus = null) :
        base(periodCount, jobJobStatus)
    {
        WageTypes = new() { wageTypeNumber };
    }

    /// <summary>Initializes a new instance of the <see cref="WageTypeCycleResultQuery"/> class</summary>
    /// <param name="wageTypeNumbers">The wage type numbers</param>
    /// <param name="periodCount">The period count</param>
    /// <param name="jobJobStatus">The payrun job status query filter</param>
    public WageTypePeriodResultQuery(IEnumerable<decimal> wageTypeNumbers,
        int periodCount = 0, PayrunJobStatus? jobJobStatus = null) :
        base(periodCount, jobJobStatus)
    {
        WageTypes = wageTypeNumbers.ToList();
    }

    /// <summary>The wage type numbers</summary>
    public List<decimal> WageTypes { get; }
}

/// <summary>Custom date range result query</summary>
public abstract class RangeResultQuery : ResultQueryBase
{
    /// <summary>Initializes a new instance of the <see cref="RangeResultQuery"/> class</summary>
    /// <param name="start">The range start date</param>
    /// <param name="end">The range end date</param>
    /// <param name="jobJobStatus">The payrun job status query filter</param>
    protected RangeResultQuery(DateTime start, DateTime end, PayrunJobStatus? jobJobStatus = null) :
        base(jobJobStatus)
    {
        Start = start;
        End = end;
    }

    /// <summary>The range start date</summary>
    public DateTime Start { get; }

    /// <summary>The range end date</summary>
    public DateTime End { get; }
}

/// <summary>Collector cycle result query</summary>
public class CollectorRangeResultQuery : RangeResultQuery
{
    /// <summary>Initializes a new instance of the <see cref="CollectorRangeResultQuery"/> class</summary>
    /// <param name="collectorName">The collector name</param>
    /// <param name="start">The range start date</param>
    /// <param name="end">The range end date</param>
    /// <param name="jobJobStatus">The payrun job status query filter</param>
    public CollectorRangeResultQuery(string collectorName, DateTime start, DateTime end, PayrunJobStatus? jobJobStatus = null) :
        base(start, end, jobJobStatus)
    {
        Collectors = new() { collectorName };
    }

    /// <summary>Initializes a new instance of the <see cref="CollectorRangeResultQuery"/> class</summary>
    /// <param name="collectorNames">The collector names</param>
    /// <param name="start">The range start date</param>
    /// <param name="end">The range end date</param>
    /// <param name="jobJobStatus">The payrun job status query filter</param>
    public CollectorRangeResultQuery(IEnumerable<string> collectorNames,
        DateTime start, DateTime end, PayrunJobStatus? jobJobStatus = null) :
        base(start, end, jobJobStatus)
    {
        Collectors = collectorNames.ToList();
    }

    /// <summary>The collector names</summary>
    public List<string> Collectors { get; }
}

/// <summary>Wage type cycle result query</summary>
public class WageTypeRangeResultQuery : RangeResultQuery
{
    /// <summary>Initializes a new instance of the <see cref="WageTypeRangeResultQuery"/> class</summary>
    /// <param name="wageTypeNumber">The wage type number</param>
    /// <param name="start">The range start date</param>
    /// <param name="end">The range end date</param>
    /// <param name="jobJobStatus">The payrun job status query filter</param>
    public WageTypeRangeResultQuery(decimal wageTypeNumber, DateTime start, DateTime end, PayrunJobStatus? jobJobStatus = null) :
        base(start, end, jobJobStatus)
    {
        WageTypes = new() { wageTypeNumber };
    }

    /// <summary>Initializes a new instance of the <see cref="WageTypeCycleResultQuery"/> class</summary>
    /// <param name="wageTypeNumbers">The wage type numbers</param>
    /// <param name="start">The range start date</param>
    /// <param name="end">The range end date</param>
    /// <param name="jobJobStatus">The payrun job status query filter</param>
    public WageTypeRangeResultQuery(IEnumerable<decimal> wageTypeNumbers,
        DateTime start, DateTime end, PayrunJobStatus? jobJobStatus = null) :
        base(start, end, jobJobStatus)
    {
        WageTypes = wageTypeNumbers.ToList();
    }

    /// <summary>The wage type numbers</summary>
    public List<decimal> WageTypes { get; }
}

/// <summary>Consolidated result query</summary>
public abstract class ConsolidatedResultQuery : ResultQueryBase
{
    /// <summary>Initializes a new instance of the <see cref="ConsolidatedResultQuery"/> class</summary>
    /// <param name="periodMoment">Moment within the period</param>
    /// <param name="jobStatus">The payrun job status query filter</param>
    protected ConsolidatedResultQuery(DateTime periodMoment, PayrunJobStatus? jobStatus = null) :
        base(jobStatus)
    {
        PeriodMoment = periodMoment;
    }

    /// <summary>Moment within the period</summary>
    public DateTime PeriodMoment { get; }
}

/// <summary>Collector consolidated result query</summary>
public class CollectorConsolidatedResultQuery : ConsolidatedResultQuery
{
    /// <summary>Initializes a new instance of the <see cref="CollectorConsolidatedResultQuery"/> class</summary>
    /// <param name="collectorName">The collector name</param>
    /// <param name="periodMoment">Moment within the period</param>
    /// <param name="jobStatus">The payrun job status query filter</param>
    public CollectorConsolidatedResultQuery(string collectorName, DateTime periodMoment, PayrunJobStatus? jobStatus = null) :
        base(periodMoment, jobStatus)
    {
        Collectors = new() { collectorName };
    }

    /// <summary>Initializes a new instance of the <see cref="CollectorConsolidatedResultQuery"/> class</summary>
    /// <param name="collectorNames">The collector names</param>
    /// <param name="periodMoment">Moment within the period</param>
    /// <param name="jobStatus">The payrun job status query filter</param>
    public CollectorConsolidatedResultQuery(IEnumerable<string> collectorNames, DateTime periodMoment, PayrunJobStatus? jobStatus = null) :
        base(periodMoment, jobStatus)
    {
        Collectors = collectorNames.ToList();
    }

    /// <summary>The collector names</summary>
    public List<string> Collectors { get; }
}

/// <summary>Wage type consolidated result query</summary>
public class WageTypeConsolidatedResultQuery : ConsolidatedResultQuery
{
    /// <summary>Initializes a new instance of the <see cref="WageTypeConsolidatedResultQuery"/> class</summary>
    /// <param name="wageTypeNumber">The wage type number</param>
    /// <param name="periodMoment">Moment within the period</param>
    /// <param name="jobStatus">The payrun job status query filter</param>
    public WageTypeConsolidatedResultQuery(decimal wageTypeNumber, DateTime periodMoment, PayrunJobStatus? jobStatus = null) :
        base(periodMoment, jobStatus)
    {
        WageTypes = new() { wageTypeNumber };
    }

    /// <summary>Initializes a new instance of the <see cref="WageTypeConsolidatedResultQuery"/> class</summary>
    /// <param name="wageTypeNumbers">The wage type numbers</param>
    /// <param name="periodMoment">Moment within the period</param>
    /// <param name="jobStatus">The payrun job status query filter</param>
    public WageTypeConsolidatedResultQuery(IEnumerable<decimal> wageTypeNumbers, DateTime periodMoment, PayrunJobStatus? jobStatus = null) :
        base(periodMoment, jobStatus)
    {
        WageTypes = wageTypeNumbers.ToList();
    }

    /// <summary>The wage type numbers</summary>
    public List<decimal> WageTypes { get; }
}

#endregion