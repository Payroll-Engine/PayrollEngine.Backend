using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Domain.Scripting;

internal sealed class CaseValueProviderCalculation
{
    private IPayrollCalculator PayrollCalculator { get; }
    private DateTime EvaluationDate { get; }
    private DatePeriod EvaluationPeriod { get; }

    internal CaseValueProviderCalculation(IPayrollCalculator payrollCalculator, DateTime evaluationDate,
        DatePeriod evaluationPeriod)
    {
        PayrollCalculator = payrollCalculator ?? throw new ArgumentNullException(nameof(payrollCalculator));
        EvaluationDate = evaluationDate;
        EvaluationPeriod = evaluationPeriod;
    }

    /// <summary>
    /// Calculate the case value from the evaluation time period
    /// </summary>
    private object CalculateValue(CaseField caseField, CaseValue caseValue) =>
        CalculateValue(caseField, caseValue, EvaluationPeriod);

    /// <summary>
    /// Calculate the case value from a specific time period
    /// </summary>
    internal object CalculateValue(CaseField caseField, CaseValue caseValue, DatePeriod period)
    {
        object value = null;
        if (caseValue != null)
        {
            var culture = CultureInfo.DefaultThreadCurrentCulture ?? CultureInfo.InvariantCulture;

            // value
            value = ValueConvert.ToValue(caseValue.Value, caseValue.ValueType, culture);

            // calendar scaling
            if (caseField.ValueType.IsDecimal() && caseField.TimeType.IsCalendarPeriod() && value != null)
            {
                var context = new CaseValueCalculation
                {
                    EvaluationDate = EvaluationDate,
                    EvaluationPeriod = EvaluationPeriod,
                    CaseValuePeriod = period,
                    // ensure decimal type
                    CaseValue = Convert.ToDecimal(value)
                };
                value = PayrollCalculator.CalculateCasePeriodValue(context);
            }

            // ensure CLR decimal
            if (value != null && caseField.ValueType.IsDecimal())
            {
                value = Convert.ToDecimal(value);
            }
        }
        return value;
    }

    /// <summary>
    /// Calculate the case value time type for periods.
    /// Returns the case value if the start date is within the evaluation period.
    /// </summary>
    internal object CalculatePeriodValue(CaseField caseField, List<CaseValue> caseValues)
    {
        // algorithm needs to evaluate the case values ordered from
        // the newest to the oldest one (creation date)
        caseValues.Sort((x, y) => DateTime.Compare(y.Created, x.Created));

        var evaluationPeriod = EvaluationPeriod;

        // accumulate decimals
        if (caseField.ValueType.IsDecimal())
        {
            decimal total = 0;
            foreach (var caseValue in caseValues)
            {
                var period = new DatePeriod(caseValue.Start, caseValue.End);
                if (evaluationPeriod.IsWithin(period.Start))
                {
                    // return the latest created value
                    var value = CalculateValue(caseField, caseValue);
                    total += value as decimal? ??
                             throw new PayrollException($"Invalid case value type for case field {caseField}.");
                }
            }
            return total;
        }

        // non-decimals: return the latest created value
        foreach (var caseValue in caseValues)
        {
            var period = new DatePeriod(caseValue.Start, caseValue.End);
            if (evaluationPeriod.IsWithin(period.Start))
            {
                return CalculateValue(caseField, caseValue);
            }
        }

        return null;
    }

    /// <summary>
    /// Calculates for any case value the value periods
    /// </summary>
    /// <param name="caseValues">The case values</param>
    /// <returns>Dictionary with date periods per case value</returns>
    internal IDictionary<CaseValue, List<DatePeriod>> SplitCaseValuePeriods(List<CaseValue> caseValues)
    {
        var allValuePeriods = new Dictionary<CaseValue, List<DatePeriod>>();

        // list with the available periods, initialized with the evaluation period
        var availablePeriods = new List<DatePeriod>([EvaluationPeriod]);
        if (availablePeriods[0] == null)
        {
            throw new InvalidOperationException("Avail Period Setup");
        }

        // algorithm needs to evaluate the case values ordered from
        // the newest to the oldest one (creation date)
        caseValues.Sort((x, y) => DateTime.Compare(y.Created, x.Created));

        // determine available periods per case value
        foreach (var caseValue in caseValues)
        {
            var remainingPeriods = new List<DatePeriod>();
            var valuePeriods = new List<DatePeriod>();

            // the case value period, considering open periods
            var valuePeriod = new DatePeriod(caseValue.Start?.ToUtc(), caseValue.End?.LastMomentOfDay().ToUtc());
            foreach (var availablePeriod in availablePeriods)
            {
                SplitCaseValuePeriod(valuePeriod, availablePeriod, valuePeriods, remainingPeriods);
            }

            // collect period results per case value
            if (valuePeriods.Any())
            {
                allValuePeriods.Add(caseValue, valuePeriods);
            }

            // setup next iteration
            if (remainingPeriods.Any())
            {
                // remaining periods available
                availablePeriods = remainingPeriods;
            }
            else
            {
                // no more time available
                break;
            }
        }

        return allValuePeriods;
    }

    /// <summary>
    /// Calculates for a case value the value and remaining periods
    /// </summary>
    /// <param name="valuePeriod">The value period</param>
    /// <param name="availablePeriod">The available period</param>
    /// <param name="valuePeriods">The resulting value periods</param>
    /// <param name="remainingPeriods">The resulting remaining periods</param>
    private static void SplitCaseValuePeriod(DatePeriod valuePeriod, DatePeriod availablePeriod, List<DatePeriod> valuePeriods,
        List<DatePeriod> remainingPeriods)
    {
        var startInside = valuePeriod.Start >= availablePeriod.Start &&
                          valuePeriod.Start < availablePeriod.End;
        var endInside = valuePeriod.End <= availablePeriod.End &&
                        valuePeriod.End > availablePeriod.Start;

        if (startInside)
        {
            // --- start inside ---
            // [G1] gap between the available start und the value start
            AddDatePeriod(remainingPeriods, availablePeriod.Start.RoundLastMoment(), valuePeriod.Start);
            if (endInside)
            {
                // --- start and end inside ---
                // [G2] gap between the value end and the available end
                AddDatePeriod(remainingPeriods, valuePeriod.End.RoundLastMoment(), availablePeriod.End);
                // [V1] value period entirely inside the available period
                valuePeriods.Add(valuePeriod);
            }
            else
            {
                // --- start inside and end outside ---
                // [V2] value period between the value start and available end
                AddDatePeriod(valuePeriods, valuePeriod.Start, availablePeriod.End);
            }
        }
        else if (endInside)
        {
            // --- start outside and end inside ---
            // [G2] gap between the value end and the available end
            AddDatePeriod(remainingPeriods, valuePeriod.End.RoundLastMoment(), availablePeriod.End);
            // [V3] value period between the available start and value end
            AddDatePeriod(valuePeriods, availablePeriod.Start, valuePeriod.End);
        }
        else
        {
            // --- start and end outside ---
            if (valuePeriod.Start < availablePeriod.Start &&
                valuePeriod.End > availablePeriod.End)
            {
                // [V1] value period covers entirely the available period
                valuePeriods.Add(availablePeriod);
            }
            else
            {
                // no period intersections: preserve available period
                remainingPeriods.Add(availablePeriod);
            }
        }
    }

    private static void AddDatePeriod(List<DatePeriod> list, DateTime start, DateTime end)
    {
        start = start.ToUtc();
        end = end.ToUtc();

        // ignore empty periods
        if (Date.IsPeriod(start, end))
        {
            list.Add(new(start, end));
        }
    }
}