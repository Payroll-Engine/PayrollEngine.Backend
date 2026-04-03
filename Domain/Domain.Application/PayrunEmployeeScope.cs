using System;
using System.Linq;
using System.Collections.Generic;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Scripting;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

/// <summary>
/// Per-employee execution scope, derived from a shared <see cref="PayrunContext"/>.
/// Holds all mutable state that varies per employee so that multiple employees can
/// be processed concurrently without sharing mutable fields on the context.
/// </summary>
internal sealed class PayrunEmployeeScope : IRegulationProvider
{
    // -------------------------------------------------------------------------
    // Shared (read-only) references from the parent context
    // -------------------------------------------------------------------------

    internal PayrunContext SharedContext { get; }

    // Convenience pass-through to avoid repeating SharedContext.X everywhere
    internal bool StoreEmptyResults => SharedContext.StoreEmptyResults;
    internal User User => SharedContext.User;
    internal Division Division => SharedContext.Division;
    internal Payroll Payroll => SharedContext.Payroll;
    internal PayrunJob PayrunJob => SharedContext.PayrunJob;
    internal PayrunJob ParentPayrunJob => SharedContext.ParentPayrunJob;
    internal List<RetroPayrunJob> RetroPayrunJobs => SharedContext.RetroPayrunJobs;
    internal DateTime EvaluationDate => SharedContext.EvaluationDate;
    internal DateTime? RetroDate => SharedContext.RetroDate;
    internal DatePeriod EvaluationPeriod => SharedContext.EvaluationPeriod;
    internal CaseValueCache GlobalCaseValues => SharedContext.GlobalCaseValues;
    internal CaseValueCache NationalCaseValues => SharedContext.NationalCaseValues;
    internal CaseValueCache CompanyCaseValues => SharedContext.CompanyCaseValues;
    internal List<Regulation> DerivedRegulations => SharedContext.DerivedRegulations;
    internal ILookup<decimal, DerivedWageType> DerivedWageTypes => SharedContext.DerivedWageTypes;
    internal IRegulationLookupProvider RegulationLookupProvider => SharedContext.RegulationLookupProvider;
    internal ICaseFieldProvider CaseFieldProvider => SharedContext.CaseFieldProvider;

    // -------------------------------------------------------------------------
    // Per-employee mutable state
    // -------------------------------------------------------------------------

    /// <summary>Collector instances cloned fresh for this employee.</summary>
    internal ILookup<string, DerivedCollector> DerivedCollectors { get; set; }

    /// <summary>Active calculator for the current employee (may differ by calendar/culture).</summary>
    internal IPayrollCalculator Calculator { get; set; }

    /// <summary>Calendar name resolved for the current employee.</summary>
    internal string CalendarName { get; set; }

    /// <summary>Execution phase (Setup / Reevaluation).</summary>
    internal PayrunExecutionPhase ExecutionPhase { get; set; }

    /// <summary>Runtime values scoped to this employee.</summary>
    internal IRuntimeValueProvider RuntimeValueProvider { get; } = new RuntimeValueProvider();

    /// <summary>
    /// Pre-loaded consolidated WageType results (with retro-merge) for all WageTypes
    /// tagged via <c>Payroll.ClusterSet.ClusterSetWageTypeCons</c>.
    /// <c>null</c> when no Cons-clustered WageTypes exist, or in the first period of a cycle.
    /// Populated at PayrunEmployeeStart; reset and reloaded before retro reevaluation.
    /// </summary>
    internal WageTypeConsCache WageTypeConsCache { get; set; }

    /// <summary>
    /// Pre-loaded WageType results for all WageTypes tagged via
    /// <c>Payroll.ClusterSet.ClusterSetWageTypeCycle</c>.
    /// Covers the range <c>(CycleStart, PreviousPeriodEnd)</c> for any calendar cycle type.
    /// <c>null</c> when no cycle-clustered WageTypes exist, or in the first period of a cycle.
    /// Populated at PayrunEmployeeStart; reset and reloaded before retro reevaluation.
    /// </summary>
    internal WageTypeCycleCache WageTypeCycleCache { get; set; }

    // -------------------------------------------------------------------------
    // Culture stack (per-employee, not shared)
    // -------------------------------------------------------------------------

    private readonly Stack<string> payrollCultures = new();

    internal string PayrollCulture => payrollCultures.Peek();

    internal void PushPayrollCulture(string culture) =>
        payrollCultures.Push(culture);

    // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Global
    internal void PopPayrollCulture(string culture)
    {
        if (!payrollCultures.Any() || !string.Equals(PayrollCulture, culture))
        {
            throw new InvalidOperationException();
        }
        payrollCultures.Pop();
    }

    // -------------------------------------------------------------------------
    // IRegulationProvider (scripts receive the scope as IRegulationProvider)
    // -------------------------------------------------------------------------

    ILookup<string, DerivedCollector> IRegulationProvider.DerivedCollectors => DerivedCollectors;
    ILookup<decimal, DerivedWageType> IRegulationProvider.DerivedWageTypes => DerivedWageTypes;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    internal PayrunEmployeeScope(PayrunContext sharedContext)
    {
        SharedContext = sharedContext ?? throw new ArgumentNullException(nameof(sharedContext));

        // Seed mutable state from the shared context at the time the scope is created.
        Calculator = sharedContext.Calculator;
        CalendarName = sharedContext.CalendarName;
        ExecutionPhase = PayrunExecutionPhase.Setup;

        // Seed the culture stack so PayrollCulture is always valid.
        PushPayrollCulture(sharedContext.PayrollCulture);
    }
}
