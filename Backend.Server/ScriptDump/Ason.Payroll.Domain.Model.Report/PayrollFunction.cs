/* PayrollFunction */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Ason.Payroll.Client.Scripting.Function;

/// <summary>Base class for any script function</summary>
// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class PayrollFunction : Function
{
    /// <summary>New function instance</summary>
    /// <param name="runtime">The function runtime</param>
    protected PayrollFunction(object runtime) :
        base(runtime)
    {
        // payroll
        PayrollId = Runtime.PayrollId;
        PayrollCountry = Runtime.PayrollCountry;

        // employee
        EmployeeId = Runtime.EmployeeId;
        EmployeeIdentifier = Runtime.EmployeeIdentifier;

        // evaluation
        EvaluationDate = Runtime.EvaluationDate;
        var (start, end) = (Tuple<DateTime, DateTime>)Runtime.GetEvaluationPeriod();
        EvaluationPeriod = new(start, end);

        // cycle
        Cycle = GetCycle();
        PreviousCycle = GetCycle(-1);
        NextCycle = GetCycle(1);

        // date/period
        Period = GetPeriod();
        PreviousPeriod = GetPeriod(-1);
        NextPeriod = GetPeriod(1);
        CycleStartOffset = GetPeriodOffset(CycleStart);
        CycleEndOffset = GetPeriodOffset(CycleEnd);

        Periods = new(GetPeriod);
        CaseValue = new(x => GetCaseValue(x));
        CaseValueTags = new(GetCaseValueTags);
    }

    #region Scripting Development

    /// <summary>New function instance without runtime (scripting development)</summary>
    /// <remarks>Use <see cref="Function.GetSourceFileName"/> in your constructor for the source file name</remarks>
    /// <param name="sourceFileName">The name of the source file</param>
    protected PayrollFunction(string sourceFileName) :
        base(sourceFileName)
    {
    }

    #endregion

    #region Payrol

    /// <summary>The payroll id</summary>
    public int PayrollId { get; }

    /// <summary>The ISO 3166-1 numeric country code</summary>
    public int PayrollCountry { get; }

    #endregion

    #region Employee

    /// <summary>The employee id</summary>
    public int? EmployeeId { get; }

    /// <summary>The employee identifier</summary>
    public string EmployeeIdentifier { get; }

    /// <summary>Get employee attribute value</summary>
    public object GetEmployeeAttribute(string attributeName) =>
        Runtime.GetEmployeeAttribute(attributeName);

    /// <summary>Get employee attribute typed value</summary>
    public T GetEmployeeAttribute<T>(string attributeName, T defaultValue = default)
    {
        var value = GetEmployeeAttribute(attributeName);
        return value == null ? defaultValue : (T)Convert.ChangeType(value, typeof(T));
    }

    #endregion

    #region Cycle

    /// <summary>The current cycle start date</summary>
    public DateTime CycleStart => Cycle.Start;

    /// <summary>The current cycle end date</summary>
    public DateTime CycleEnd => Cycle.End;

    /// <summary>The current cycle</summary>
    public DatePeriod Cycle { get; }

    /// <summary>The day count of the current cycle</summary>
    public double CycleDays => Cycle.TotalDays;

    /// <summary>The previous cycle</summary>
    public DatePeriod PreviousCycle { get; }

    /// <summary>The next cycle</summary>
    public DatePeriod NextCycle { get; }

    /// <summary>Get cycle by offset to the current cycle</summary>
    /// <param name="offset">The cycle offset: 0=current, -1=previous, 1=next</param>
    /// <returns>The offset cycle</returns>
    public DatePeriod GetCycle(int offset = 0) =>
        GetCycle(EvaluationPeriod.Start, offset);

    /// <summary>Get cycle by moment</summary>
    /// <param name="moment">The cycle moment</param>
    /// <param name="offset">The cycle offset: 0=current, -1=previous, 1=next</param>
    /// <returns>The cycle including the moment</returns>
    public DatePeriod GetCycle(DateTime moment, int offset = 0)
    {
        var (start, end) = (Tuple<DateTime, DateTime>)Runtime.GetCycle(moment, offset);
        return new(start, end);
    }

    /// <summary>Get periods by offset range</summary>
    /// <param name="startOffset">The offset to the first period</param>
    /// <param name="endOffset">The offset to the last period</param>
    /// <returns>The periods ordered by date</returns>
    public List<DatePeriod> GetPeriods(int startOffset, int endOffset)
    {
        if (startOffset > endOffset)
        {
            throw new ArgumentOutOfRangeException(nameof(startOffset));
        }
        var periods = new List<DatePeriod>();
        for (var offset = startOffset; offset <= endOffset; offset++)
        {
            periods.Add(GetPeriod(offset));
        }
        return periods;
    }

    /// <summary>Get cycle periods</summary>
    /// <returns>The cycle periods ordered by date</returns>
    public List<DatePeriod> GetCyclePeriods() =>
        GetPeriods(CycleStartOffset, CycleEndOffset);

    /// <summary>Get past cycle periods from the first cycle period to the current period</summary>
    /// <param name="includeCurrent">Include the current period</param>
    /// <returns>The past cycle periods ordered by date</returns>
    public List<DatePeriod> GetPastCyclePeriods(bool includeCurrent = true) =>
        GetPeriods(CycleStartOffset, includeCurrent ? 0 : -1);

    /// <summary>Get future cycle periods from the current period to the last cycle period</summary>
    /// <param name="includeCurrent">Include the current period</param>
    /// <returns>The future cycle periods ordered by date</returns>
    public List<DatePeriod> GetFutureCyclePeriods(bool includeCurrent = true) =>
        GetPeriods(includeCurrent ? 0 : 1, CycleEndOffset);

    /// <summary>Test if a date is the first cycle day</summary>
    /// <param name="moment">The cycle moment</param>
    /// <returns>True for the first cycle day</returns>
    public bool IsFirstCycleDay(DateTime moment) =>
        moment.IsSameDay(CycleStart);

    /// <summary>Test if a date is the last cycle day</summary>
    /// <param name="moment">The cycle moment</param>
    /// <returns>Tru for the last cycle day</returns>
    public bool IsLastCycleDay(DateTime moment) =>
        moment.IsSameDay(CycleEnd);

    #endregion

    #region Date/Period

    /// <summary>The evaluation date</summary>
    public DateTime EvaluationDate { get; }

    /// <summary>The evaluation period</summary>
    public DatePeriod EvaluationPeriod { get; }

    /// <summary>Periods by offset</summary>
    public ScriptDictionary<int, DatePeriod> Periods { get; }

    /// <summary>The current period start date</summary>
    public DateTime PeriodStart => Period.Start;

    /// <summary>The current period end date</summary>
    public DateTime PeriodEnd => Period.End;

    /// <summary>The current period</summary>
    public DatePeriod Period { get; }

    /// <summary>The day count of the current period</summary>
    public double PeriodDays => Period.TotalDays;

    /// <summary>The previous period</summary>
    public DatePeriod PreviousPeriod { get; }

    /// <summary>The next period</summary>
    public DatePeriod NextPeriod { get; }

    /// <summary>True for the first cycle period</summary>
    public bool FirstCyclePeriod => CycleStart == PeriodStart;

    /// <summary>True for the last cycle period</summary>
    public bool LastCyclePeriod => PeriodEnd == CycleEnd;

    /// <summary>Offset of the current period to the start of the current cycle,<br />
    /// 0 for the first cycle period, -1 for the previous period, and so on</summary>
    public int CycleStartOffset { get; }

    /// <summary>Offset of the current period to the end of the current cycle,<br />
    /// 0 for the ultimate cycle period, 1 for the penultimate cycle period, and so on</summary>
    public int CycleEndOffset { get; }

    /// <summary>The number of periods from the cycle start of the current period</summary>
    public int PastCyclePeriods => Math.Abs(CycleStartOffset);

    /// <summary>The number of periods from the current period to the cycle end</summary>
    public int FutureCyclePeriods => CycleEndOffset;

    /// <summary>The number of periods within a cycle</summary>
    public int PeriodsInCycle => CycleStartOffset + CycleEndOffset + 1;

    /// <summary>Get period before the evaluation date</summary>
    /// <returns>Period from the minimal date until the evaluation date</returns>
    public DatePeriod PastPeriod() => new(Date.MinValue, EvaluationDate.PreviousTick());

    /// <summary>Get period after the evaluation date</summary>
    /// <returns>Period from the evaluation date until the maximal date</returns>
    public DatePeriod FuturePeriod() => new(EvaluationDate.NextTick(), Date.MaxValue);

    /// <summary>Get period by offset to the current period</summary>
    /// <param name="offset">The period offset: 0=current, -1=previous, 1=next</param>
    /// <returns>The offset period</returns>
    public DatePeriod GetPeriod(int offset = 0) =>
        GetPeriod(EvaluationPeriod.Start, offset);

    /// <summary>Get period by moment</summary>
    /// <param name="moment">The period moment</param>
    /// <param name="offset">The period offset: 0=current, -1=previous, 1=next</param>
    /// <returns>The period including the moment</returns>
    public DatePeriod GetPeriod(DateTime moment, int offset = 0)
    {
        var (start, end) = (Tuple<DateTime, DateTime>)Runtime.GetPeriod(moment, offset);
        return new(start, end);
    }

    /// <summary>Get offset to period</summary>
    /// <param name="moment">The period moment</param>
    /// <returns>The period offset</returns>
    public int GetPeriodOffset(DateTime moment)
    {
        // current period
        if (Period.IsWithin(moment))
        {
            return 0;
        }

        var offset = 0;
        if (Period.IsBefore(moment))
        {
            // past period
            var periodStart = PeriodStart;
            while (periodStart > moment)
            {
                offset--;
                // previous period
                periodStart = GetPeriod(offset).Start;
            }
        }
        else
        {
            // future period
            var periodEnd = PeriodEnd;
            while (periodEnd < moment)
            {
                offset++;
                // next period
                periodEnd = GetPeriod(offset).End;
            }
        }
        return offset;
    }

    /// <summary>Test if a date is the first period day</summary>
    /// <param name="moment">The period moment</param>
    /// <returns>True for the first period day</returns>
    public bool IsFirstPeriodDay(DateTime moment) =>
        moment.IsSameDay(PeriodStart);

    /// <summary>Test if a date is the last period day</summary>
    /// <param name="moment">The period moment</param>
    /// <returns>Tru for the last period day</returns>
    public bool IsLastPeriodDay(DateTime moment) =>
        moment.IsSameDay(PeriodEnd);

    #endregion

    #region Case Value

    /// <summary>Test for available cases from the current period</summary>
    /// <param name="caseFieldNames">The name of the case fields to test</param>
    /// <returns>True if all case values are available</returns>
    public bool TestAvailableCaseValues(IEnumerable<string> caseFieldNames) =>
        TestAvailableCaseValues(Period, caseFieldNames);

    /// <summary>Test for available cases</summary>
    /// <param name="period">The value period</param>
    /// <param name="caseFieldNames">The name of the case fields to test</param>
    /// <returns>True if all case values are available</returns>
    public bool TestAvailableCaseValues(DatePeriod period, IEnumerable<string> caseFieldNames) =>
        GetFirstUnavailableCaseValue(Period, caseFieldNames) == null;

    /// <summary>Get the first available case value from the current period</summary>
    /// <param name="caseFieldNames">The name of the case fields to test</param>
    /// <returns>The first available case value, otherwise null</returns>
    public CasePayrollValue GetFirstAvailableCaseValue(IEnumerable<string> caseFieldNames) =>
        GetFirstAvailableCaseValue(Period, caseFieldNames);

    /// <summary>Get the first available case value within a time period</summary>
    /// <param name="period">The value period</param>
    /// <param name="caseFieldNames">The name of the case fields to test</param>
    /// <returns>The first available case value, otherwise null</returns>
    public CasePayrollValue GetFirstAvailableCaseValue(DatePeriod period, IEnumerable<string> caseFieldNames)
    {
        foreach (var caseFieldName in caseFieldNames)
        {
            var value = GetPeriodCaseValue(period, caseFieldName);
            if (value != null && value.Any())
            {
                return value;
            }
        }
        return null;
    }

    /// <summary>Get the first unavailable case value from the current period</summary>
    /// <param name="caseFieldNames">The name of the case fields to test</param>
    /// <returns>The case field name of the first unavailable case value, otherwise null</returns>
    public string GetFirstUnavailableCaseValue(IEnumerable<string> caseFieldNames) =>
        GetFirstUnavailableCaseValue(Period, caseFieldNames);

    /// <summary>Get the first unavailable case value within a time period</summary>
    /// <param name="period">The value period</param>
    /// <param name="caseFieldNames">The name of the case fields to test</param>
    /// <returns>The case field name of the first unavailable case value, otherwise null</returns>
    public string GetFirstUnavailableCaseValue(DatePeriod period, IEnumerable<string> caseFieldNames)
    {
        foreach (var caseFieldName in caseFieldNames)
        {
            var value = GetPeriodCaseValue(period, caseFieldName);
            if (value == null || !value.Any())
            {
                return caseFieldName;
            }
        }
        return null;
    }

    /// <summary>Get the case field name including the case slot</summary>
    /// <param name="caseFieldName">The case field name</param>
    /// <param name="caseSlot">The case slot</param>
    /// <returns>The case field slot name</returns>
    public string CaseFieldSlot(string caseFieldName, string caseSlot)
    {
        if (string.IsNullOrWhiteSpace(caseFieldName) || caseFieldName.Contains(StringExtensions.CaseFieldSlotSeparator))
        {
            throw new ArgumentException(nameof(caseFieldName));
        }
        if (string.IsNullOrWhiteSpace(caseSlot) || caseSlot.Contains(StringExtensions.CaseFieldSlotSeparator))
        {
            throw new ArgumentException(nameof(caseSlot));
        }
        return $"{caseFieldName}{StringExtensions.CaseFieldSlotSeparator}{caseSlot}";
    }

    /// <summary>Get the case payroll value by case field name</summary>
    public ScriptDictionary<string, CasePayrollValue> CaseValue { get; }

    /// <summary>Get the case value tags by case field name</summary>
    public ScriptDictionary<string, List<string>> CaseValueTags { get; }

    /// <summary>Get the case payroll typed value by case field name</summary>
    /// <param name="period">The value period</param>
    /// <param name="caseFieldName">The case field name</param>
    /// <param name="caseSlot">The case slot</param>
    /// <returns>A case value from the period</returns>
    public T GetPeriodCaseValue<T>(DatePeriod period, string caseFieldName, string caseSlot = null) =>
        GetPeriodCaseValue(period, caseFieldName, caseSlot).ValueAs<T>();

    /// <summary>Get the case payroll value by case field name</summary>
    /// <param name="period">The value period</param>
    /// <param name="caseFieldName">The case field name</param>
    /// <param name="caseSlot">The case slot</param>
    /// <returns>A case value from the period</returns>
    public CasePayrollValue GetPeriodCaseValue(DatePeriod period, string caseFieldName, string caseSlot = null)
    {
        if (!string.IsNullOrWhiteSpace(caseSlot))
        {
            caseFieldName = CaseFieldSlot(caseFieldName, caseSlot);
        }
        var periodValues = GetPeriodCaseValues(period, caseFieldName);
        return periodValues.Count == 1 ? periodValues[caseFieldName] : new(caseFieldName);
    }

    /// <summary>Get multiple case values of a date period</summary>
    /// <param name="period">The date period</param>
    /// <param name="caseFieldNames">The case field names</param>
    /// <returns>Dictionary of case values grouped by case field name</returns>
    public CasePayrollValueDictionary GetPeriodCaseValues(DatePeriod period, params string[] caseFieldNames) =>
        TupleExtensions.TupleToCaseValuesDictionary(Runtime.GetCasePeriodValues(period.Start, period.End, caseFieldNames));

    /// <summary>Get multiple case values of an offset period</summary>
    /// <param name="periodOffset">The offset period</param>
    /// <param name="caseFieldNames">The case field names</param>
    /// <returns>Dictionary of case values grouped by case field name</returns>
    public CasePayrollValueDictionary GetPeriodCaseValues(int periodOffset, params string[] caseFieldNames) =>
        GetPeriodCaseValues(GetPeriod(periodOffset), caseFieldNames);

    /// <summary>Get case payroll typed value of an offset period</summary>
    /// <param name="periodOffset">The offset period</param>
    /// <param name="caseFieldName">The case field name</param>
    /// <param name="caseSlot">The case slot</param>
    /// <returns>A case value from the offset period</returns>
    public T GetPeriodCaseValue<T>(int periodOffset, string caseFieldName, string caseSlot = null) =>
        GetPeriodCaseValue(periodOffset, caseFieldName, caseSlot).ValueAs<T>();

    /// <summary>Get case payroll value of an offset period</summary>
    /// <param name="periodOffset">The offset period</param>
    /// <param name="caseFieldName">The case field name</param>
    /// <param name="caseSlot">The case slot</param>
    /// <returns>A case value from the offset period</returns>
    public CasePayrollValue GetPeriodCaseValue(int periodOffset, string caseFieldName, string caseSlot = null) =>
        GetPeriodCaseValue(GetPeriod(periodOffset), caseFieldName, caseSlot);

    /// <summary>Get case payroll typed value of the current period</summary>
    /// <param name="caseFieldName">The case field name</param>
    /// <param name="caseSlot">The case slot</param>
    /// <returns>A case value from the current period</returns>
    public T GetCaseValue<T>(string caseFieldName, string caseSlot = null) =>
        GetCaseValue(caseFieldName, caseSlot).ValueAs<T>();

    /// <summary>Get case payroll value of the current period</summary>
    /// <param name="caseFieldName">The case field name</param>
    /// <param name="caseSlot">The case slot</param>
    /// <returns>A case value from the current period</returns>
    public CasePayrollValue GetCaseValue(string caseFieldName, string caseSlot = null) =>
        GetPeriodCaseValue(Period, caseFieldName, caseSlot);

    /// <summary>Get case payroll value of multiple periods</summary>
    /// <param name="periodStartOffset">Offset of start period</param>
    /// <param name="periodEndOffset">Offset of end period</param>
    /// <param name="caseFieldName">The case field name</param>
    /// <param name="caseSlot">The case slot</param>
    /// <returns>Dictionary of case values grouped by periods</returns>
    public PeriodCasePayrollValueDictionary GetPeriodCaseValue(int periodStartOffset,
        int periodEndOffset, string caseFieldName, string caseSlot = null)
    {
        if (periodEndOffset < periodStartOffset)
        {
            throw new ArgumentOutOfRangeException(nameof(periodEndOffset));
        }
        var values = new Dictionary<DatePeriod, CasePayrollValue>();
        for (var offset = periodStartOffset; offset <= periodEndOffset; offset++)
        {
            var period = GetPeriod(offset);
            values.Add(period, GetPeriodCaseValue(period, caseFieldName, caseSlot));
        }
        return new(values);
    }

    /// <summary>Get multiple case values of multiple periods</summary>
    /// <param name="periodStartOffset">Offset of start period</param>
    /// <param name="periodEndOffset">Offset of end period</param>
    /// <param name="caseFieldNames">The case field names</param>
    /// <returns>Dictionary of multiple case values grouped by period</returns>
    public MultiPeriodCasePayrollValueDictionary GetMultiPeriodCaseValues(int periodStartOffset,
        int periodEndOffset, params string[] caseFieldNames)
    {
        if (periodEndOffset < periodStartOffset)
        {
            throw new ArgumentOutOfRangeException(nameof(periodEndOffset));
        }
        var values = new Dictionary<DatePeriod, CasePayrollValueDictionary>();
        for (var offset = periodStartOffset; offset <= periodEndOffset; offset++)
        {
            var period = GetPeriod(offset);
            values.Add(period, GetPeriodCaseValues(period, caseFieldNames));
        }
        return new(values);
    }

    /// <summary>Get raw case value from a specific date</summary>
    /// <param name="caseFieldName">The case field name</param>
    /// <param name="valueDate">The value date</param>
    /// <returns>Raw case value from a specific date</returns>
    public CaseValue GetRawCaseValue(string caseFieldName, DateTime valueDate) =>
        TupleExtensions.TupleToCaseValue(Runtime.GetCaseValue(caseFieldName, valueDate.ToUtc()));

    /// <summary>Get raw case values created within a date period</summary>
    /// <param name="caseFieldName">The case field name</param>
    /// <param name="period">The case value creation period</param>
    /// <returns>Raw case values from a date period</returns>
    public List<CaseValue> GetRawCaseValues(string caseFieldName, DatePeriod period) =>
        GetRawCaseValues(caseFieldName, period.Start, period.End);
    
    /// <summary>Get raw case values created within a date period</summary>
    /// <param name="caseFieldName">The case field name</param>
    /// <remarks>Case value tags and attributes are not supported</remarks>
    /// <returns>Raw case values from a date period</returns>
    public List<CaseValue> GetPeriodRawCaseValues(string caseFieldName) =>
        GetRawCaseValues(caseFieldName, Period);

    /// <summary>Get raw case values created within an offset period</summary>
    /// <param name="caseFieldName">The case field name</param>
    /// <param name="periodOffset">The offset period</param>
    /// <returns>Raw case values from an offset period</returns>
    public List<CaseValue> GetRawCaseValues(string caseFieldName, int periodOffset) =>
        GetRawCaseValues(caseFieldName, GetPeriod(periodOffset));

    /// <summary>Get raw case values created within the current period</summary>
    /// <param name="caseFieldName">The case field name</param>
    /// <param name="startDate">The date after the case value was created</param>
    /// <param name="endDate">The date before the case value was created</param>
    /// <returns>Raw case values from the current period</returns>
    public List<CaseValue> GetRawCaseValues(string caseFieldName, DateTime? startDate = null, DateTime? endDate = null) =>
        TupleExtensions.TupleToCaseValues(Runtime.GetCaseValues(caseFieldName, startDate, endDate));

    /// <summary>Get multiple case values of the current period</summary>
    /// <param name="caseFieldNames">The case field names</param>
    /// <returns>Dictionary of case values grouped by case field name</returns>
    public CasePayrollValueDictionary GetCaseValues(params string[] caseFieldNames) =>
        GetPeriodCaseValues(Period, caseFieldNames);

    /// <summary>Get the case value tags from the current period end</summary>
    /// <param name="caseFieldName">Name of the case field</param>
    /// <returns>The case value tags</returns>
    public List<string> GetCaseValueTags(string caseFieldName) =>
        GetCaseValueTags(caseFieldName, PeriodEnd);

    /// <summary>Get the case value tags from specific date</summary>
    /// <param name="caseFieldName">Name of the case field</param>
    /// <param name="valueDate">The value date</param>
    /// <returns>The case value tags</returns>
    public List<string> GetCaseValueTags(string caseFieldName, DateTime valueDate) =>
        Runtime.GetCaseValueTags(caseFieldName, valueDate);

    /// <summary>Get the case value slots</summary>
    /// <param name="caseFieldName">Name of the case field</param>
    /// <returns>The case value slot names</returns>
    public List<string> GetCaseValueSlots(string caseFieldName) =>
        Runtime.GetCaseValueSlots(caseFieldName);

    /// <summary>Get the case slot values, grouped by slot name</summary>
    /// <param name="caseFieldName">The name of the case field</param>
    /// <returns>The case values in a dictionary grouped by slot name</returns>
    public Dictionary<string, CasePayrollValue> GetSlotValues(string caseFieldName)
    {
        var slotValues = new Dictionary<string, CasePayrollValue>();
        var caseValueSlots = GetCaseValueSlots(caseFieldName);
        if (caseValueSlots != null && caseValueSlots.Any())
        {
            foreach (var caseValueSlot in caseValueSlots)
            {
                var caseValue = GetCaseValue(CaseFieldSlot(caseFieldName, caseValueSlot));
                if (caseValue.HasValue)
                {
                    slotValues.Add(caseValueSlot, caseValue);
                }
            }
        }
        return slotValues;
    }

    /// <summary>Get the typed case slot values, grouped by slot name</summary>
    /// <param name="caseFieldName">The name of the case field</param>
    /// <returns>The case values in a dictionary grouped by slot name</returns>
    public Dictionary<string, T> GetSlotValues<T>(string caseFieldName)
    {
        var slotValues = new Dictionary<string, T>();
        var caseValueSlots = GetCaseValueSlots(caseFieldName);
        if (caseValueSlots != null && caseValueSlots.Any())
        {
            foreach (var caseValueSlot in caseValueSlots)
            {
                var caseValue = GetCaseValue(CaseFieldSlot(caseFieldName, caseValueSlot));
                if (caseValue.HasValue)
                {
                    var value = (T)Convert.ChangeType(caseValue.Value, typeof(T));
                    if (value != null)
                    {
                        slotValues.Add(caseValueSlot, value);
                    }
                }
            }
        }
        return slotValues;
    }

    /// <summary>Get slot name by value</summary>
    /// <param name="caseFieldName">The name of the case field</param>
    /// <param name="value">The slot value</param>
    /// <param name="prefix">The slot prefix</param>
    /// <returns>The slot name matching the value and prefix, otherwise null</returns>
    public string GetSlotByValue(string caseFieldName, string value, string prefix = null)
    {
        var slotValues = GetSlotValues(caseFieldName);
        foreach (var slotValue in slotValues)
        {
            if (prefix != null && !slotValue.Key.StartsWith(prefix))
            {
                continue;
            }
            if (string.Equals(slotValue.Value, value))
            {
                return slotValue.Key;
            }
        }
        return null;
    }

    #endregion

    #region Lookups

    /// <summary>Get lookup value</summary>
    /// <param name="lookupName">The lookup name</param>
    /// <param name="lookupKey">The lookup key</param>
    /// <param name="language">The lookup language, null for the original value (optional)</param>
    public T GetLookup<T>(string lookupName, string lookupKey, Language? language = null)
    {
        var languageCode = language.HasValue ? (int)language.Value : (int?)null;
        var value = Runtime.GetLookup(lookupName, lookupKey, languageCode) as string;
        return !string.IsNullOrWhiteSpace(value) ? value.ConvertJson<T>() : default;
    }

    /// <summary>Get lookup value with multiple keys</summary>
    /// <param name="lookupName">The lookup name</param>
    /// <param name="lookupKeyValues">The lookup key values (serialized to JSON string)</param>
    /// <param name="language">The lookup language, null for the original value (optional)</param>
    public T GetLookup<T>(string lookupName, object[] lookupKeyValues, Language? language = null)
    {
        if (lookupKeyValues == null || lookupKeyValues.Length == 0)
        {
            throw new ArgumentException(nameof(lookupKeyValues));
        }
        return GetLookup<T>(lookupName, JsonSerializer.Serialize(lookupKeyValues), language);
    }

    /// <summary>Get lookup by range value</summary>
    /// <param name="lookupName">The lookup name</param>
    /// <param name="rangeValue">The range value</param>
    /// <param name="lookupKey">The lookup key (optional)</param>
    /// <param name="language">The lookup language, null for the original value (optional)</param>
    public T GetRangeLookup<T>(string lookupName, decimal rangeValue, string lookupKey = null, Language? language = null)
    {
        var languageCode = language.HasValue ? (int)language.Value : (int?)null;
        var value = Runtime.GetRangeLookup(lookupName, rangeValue, lookupKey, languageCode) as string;
        return !string.IsNullOrWhiteSpace(value) ? value.ConvertJson<T>() : default;
    }

    /// <summary>Get lookup by range value with multiple keys</summary>
    /// <param name="lookupName">The lookup name</param>
    /// <param name="rangeValue">The range value</param>
    /// <param name="lookupKeyValues">The lookup key values (serialized to JSON string)</param>
    /// <param name="language">The lookup language, null for the original value (optional)</param>
    public T GetRangeLookup<T>(string lookupName, decimal rangeValue, object[] lookupKeyValues, Language? language = null)
    {
        if (lookupKeyValues == null || lookupKeyValues.Length == 0)
        {
            throw new ArgumentException(nameof(lookupKeyValues));
        }
        return GetRangeLookup<T>(lookupName, rangeValue, JsonSerializer.Serialize(lookupKeyValues), language);
    }

    #endregion

}