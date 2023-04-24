using System;
using System.Collections.Generic;
using Ason.Payroll.Client.Scripting;
using Ason.Payroll.Client.Scripting.Cache;
using Ason.Payroll.Client.Scripting.Function;

namespace Ason.Regulation.Swissdec5;

/// <summary>Wage type tools</summary>
public abstract class WageTypeToolBase<TNational, TCompany, TEmployee>
    where TNational : PayrollNational
    where TCompany : PayrollCompany<TNational>
    where TEmployee : PayrunEmployee<TNational, TCompany>
{
    /// <summary>The or tag</summary>
    protected static string OrTag => "*";

    /// <summary>Wage type Oasi constructor</summary>
    protected WageTypeToolBase(WageTypeFunction function, TEmployee employee)
    {
        Function = function ?? throw new ArgumentNullException(nameof(function));
        Employee = employee ?? throw new ArgumentNullException(nameof(employee));

        // lookups
        WageType = new(wageTypeNumber => Function.WageType[wageTypeNumber]);
        Collector = new(collectorName => Function.Collector[collectorName]);
    }

    /// <summary>The function</summary>
    protected WageTypeFunction Function { get; }

    /// <summary>The employee</summary>
    protected TEmployee Employee { get; }

    #region Collector

    /// <summary>Get a collector value by collector name</summary>
    protected ScriptDictionary<string, decimal> Collector { get; }

    /// <summary>Create new collector result cycle query</summary>
    protected CollectorConsolidatedResultQuery NewCollectorCycleQuery(string collectorName, List<string> tags = null) =>
        NewCollectorQuery(collectorName, Function.CycleStart, tags);

    /// <summary>Create new collector result query</summary>
    protected static CollectorConsolidatedResultQuery NewCollectorQuery(string collectorName, DateTime periodMoment,
        List<string> tags = null) =>
        new(collectorName, periodMoment, PayrunJobStatus.Legal) { Tags = tags };

    private CollectorConsolidatedResultCache collectorConsolidatedResultCache;
    /// <summary>Get the collector results</summary>
    protected IList<CollectorResult> GetConsolidatedCollectorResults(CollectorConsolidatedResultQuery query)
    {
        collectorConsolidatedResultCache ??= new(Function.CycleStart, PayrunJobStatus.Legal);
        return collectorConsolidatedResultCache.GetConsolidatedResults(Function, query,
            nameof(GetConsolidatedCollectorResults).ToNamespace());
    }

    #endregion

    #region Wage Type

    /// <summary>Get a wage type value by wage type number</summary>
    protected ScriptDictionary<decimal, decimal> WageType { get; }

    /// <summary>Create wage type result cycle query</summary>
    protected WageTypeConsolidatedResultQuery NewWageTypeCycleQuery(decimal wageTypeNumber, List<string> tags = null) =>
        NewWageTypeQuery(wageTypeNumber, Function.CycleStart, tags);

    /// <summary>Create wage type result cycle query</summary>
    protected WageTypeConsolidatedResultQuery NewWageTypeCycleQuery(IEnumerable<decimal> wageTypeNumbers, List<string> tags = null) =>
        NewWageTypeQuery(wageTypeNumbers, Function.CycleStart, tags);

    /// <summary>Create new wage type result query</summary>
    protected static WageTypeConsolidatedResultQuery NewWageTypeQuery(decimal wageTypeNumber, DateTime periodMoment,
        List<string> tags = null) =>
        new(wageTypeNumber, periodMoment, PayrunJobStatus.Legal) { Tags = tags };

    /// <summary>Create new wage type result query</summary>
    protected static WageTypeConsolidatedResultQuery NewWageTypeQuery(IEnumerable<decimal> wageTypeNumbers,
        DateTime periodMoment, List<string> tags = null) =>
        new(wageTypeNumbers, periodMoment, PayrunJobStatus.Legal) { Tags = tags };

    private WageTypeConsolidatedResultCache wageTypeConsolidatedResultCache;
    /// <summary>Get the collector results</summary>
    protected IList<Payroll.Client.Scripting.WageTypeResult> GetConsolidatedWageTypeResults(WageTypeConsolidatedResultQuery query)
    {
        wageTypeConsolidatedResultCache ??= new(Function.CycleStart, PayrunJobStatus.Legal);
        return wageTypeConsolidatedResultCache.GetConsolidatedResults(Function, query,
            nameof(GetConsolidatedWageTypeResults).ToNamespace());
    }

    #endregion

    /// <summary>Test for valid employee insurance slot</summary>
    protected static bool IsValidEmployeeInsuranceSlot(int slot) =>
        slot >= 1 && slot <= Specification.MaxEmployeeInsuranceCodes;

    /// <summary>Calculate wage value for insurance wage</summary>
    /// <param name="allowNegative">Allow result to go below 0</param>
    /// <param name="yearMaximumWage">The maximum yearly insurance wage value</param>
    /// <param name="yearMinimumWage">The minimum yearly insurance wage value</param>
    /// <param name="collector">The total Basis (collector) - current + accumulated</param>
    /// <param name="previousPeriodsWage">Total wage of previous periods (accumulated)</param>
    /// <param name="accumulatedSvDays">Total SV days from Cycle.Start to Period.End</param>
    /// <returns>The insurance wage</returns>
    protected static decimal CalculateInsuranceWage(decimal yearMinimumWage, decimal yearMaximumWage, decimal collector,
        decimal previousPeriodsWage, decimal accumulatedSvDays, bool allowNegative = true)
    {
        if (collector == 0)
        {
            throw new InvalidOperationException("Undefined insurance wage");
        }

        decimal result = 0;
        var yearMinWage = yearMinimumWage / Specification.DaysInYear * accumulatedSvDays;
        var yearMaxWage = yearMaximumWage / Specification.DaysInYear * accumulatedSvDays;

        if (collector >= yearMaxWage)
        {
            // upper limit
            result = yearMaxWage - yearMinWage - previousPeriodsWage;
        }
        else if (collector >= yearMinWage)
        {
            // lower limit
            result = collector - yearMinWage - previousPeriodsWage;
        }
        else if (collector >= 0)
        {
            // between lower and upper limit
            result = 0 - previousPeriodsWage;
        }
        else if (allowNegative)
        {
            // negative wage
            result = collector - previousPeriodsWage;
        }
        return result;
    }

    /// <summary>Get SV month days (only for wage type result function)</summary>
    protected int GetSvMonthDaysFromResult() =>
        int.TryParse(Function.Attribute["SvMonthDays"].ToString(), out var svDays)
            ? svDays
            : 0;

    /// <summary>Set SV month days (only for wage type result function)</summary>
    protected void SetSvMonthDaysToResult() =>
        Function.Attribute["SvMonthDays"] = Employee.GetSvMonthDays();
}
