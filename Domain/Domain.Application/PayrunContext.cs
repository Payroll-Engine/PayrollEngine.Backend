﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Scripting;

namespace PayrollEngine.Domain.Application;

/// <summary>
/// Employee independent data used during the payrun
/// </summary>
internal sealed class PayrunContext
{
    internal User User { get; set; }
    internal Division Division { get; set; }
    internal Payroll Payroll { get; set; }

    internal PayrunJob PayrunJob { get; set; }
    internal PayrunJob ParentPayrunJob { get; set; }
    internal List<RetroPayrunJob> RetroPayrunJobs { get; set; }
    internal PayrunExecutionPhase ExecutionPhase { get; set; }

    internal CultureInfo Culture { get; set; }

    internal IPayrollCalculator Calculator { get; set; }
    internal CaseFieldProvider CaseFieldProvider { get; set; }

    internal DateTime EvaluationDate { get; set; }
    internal DateTime? RetroDate { get; set; }
    internal DatePeriod EvaluationPeriod { get; set; }

    internal CaseValueCache GlobalCaseValues { get; set; }
    internal CaseValueCache NationalCaseValues { get; set; }
    internal CaseValueCache CompanyCaseValues { get; set; }

    internal ILookup<string, Collector> DerivedCollectors { get; set; }
    internal ILookup<decimal, WageType> DerivedWageTypes { get; set; }

    internal RegulationLookupProvider RegulationLookupProvider { get; set; }
    internal RuntimeValueProvider RuntimeValueProvider { get; } = new();

    /// <summary>
    /// Collected errors
    /// </summary>
    internal Dictionary<Employee, Exception> Errors { get; } = new();

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