using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Collections.Concurrent;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Scripting;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

/// <summary>
/// Employee independent data used during the payrun
/// </summary>
internal sealed class PayrunContext : IRegulationProvider
{
    internal bool StoreEmptyResults { get; init; }

    internal User User { get; init; }
    internal Division Division { get; set; }
    internal Payroll Payroll { get; init; }

    internal PayrunJob PayrunJob { get; set; }
    internal PayrunJob ParentPayrunJob { get; set; }
    internal List<RetroPayrunJob> RetroPayrunJobs { get; set; }

    internal IPayrollCalculator Calculator { get; set; }
    internal ICaseFieldProvider CaseFieldProvider { get; set; }

    internal DateTime EvaluationDate { get; set; }

    internal DateTime? RetroDate { get; init; }
    internal DatePeriod EvaluationPeriod { get; set; }

    internal CaseValueCache GlobalCaseValues { get; set; }
    internal CaseValueCache NationalCaseValues { get; set; }
    internal CaseValueCache CompanyCaseValues { get; set; }

    internal List<Regulation> DerivedRegulations { get; set; }
    /// <summary>Loaded once, used as clone source per employee</summary>
    internal ILookup<string, DerivedCollector> SourceDerivedCollectors { get; set; }
    internal ILookup<decimal, DerivedWageType> DerivedWageTypes { get; set; }

    internal IRegulationLookupProvider RegulationLookupProvider { get; set; }
    internal IRuntimeValueProvider RuntimeValueProvider { get; } = new RuntimeValueProvider();

    #region IRegulationProvider

    // DerivedCollectors is not available at payrun level (only per employee via PayrunEmployeeScope)
    ILookup<string, DerivedCollector> IRegulationProvider.DerivedCollectors => null;
    ILookup<decimal, DerivedWageType> IRegulationProvider.DerivedWageTypes => DerivedWageTypes;

    #endregion

    #region Calendar and Culture

    internal string CalendarName { get; set; }
    internal string PayrollCulture { get; private set; }

    internal void SetPayrollCulture(string culture)
    {
        PayrollCulture = culture ?? throw new ArgumentNullException(nameof(culture));
    }

    #endregion

    /// <summary>
    /// Collected errors – ConcurrentDictionary so parallel employee threads can add safely.
    /// </summary>
    internal ConcurrentDictionary<Employee, Exception> Errors { get; } = new();

    internal string GetErrorMessages()
    {
        var buffer = new StringBuilder();
        if (Errors.Any())
        {
            foreach (var error in Errors)
            {
                if (buffer.Length > 0)
                {
                    // line break between error messages
                    buffer.AppendLine();
                }
                buffer.Append($"Employee {error.Key.Identifier}: {error.Value.Message}");
            }
        }
        return buffer.ToString();
    }
}