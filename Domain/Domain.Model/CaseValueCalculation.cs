using System;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace PayrollEngine.Domain.Model;

/// <summary>Calculation for the case value</summary>
public class CaseValueCalculation
{
    /// <summary>The evaluation date</summary>
    public DateTime EvaluationDate { get; set; }

    /// <summary>The evaluation period</summary>
    public DatePeriod EvaluationPeriod { get; set; }

    /// <summary>The case value period</summary>
    public DatePeriod CaseValuePeriod { get; set; }

    /// <summary>The case value</summary>
    public decimal CaseValue { get; set; }
}