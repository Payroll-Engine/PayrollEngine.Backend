﻿using PayrollEngine.Domain.Model;

namespace PayrollEngine.Domain.Scripting.Runtime;

public class PayrollRuntimeSettings : RuntimeSettings
{
    /// <summary>
    /// The payroll
    /// </summary>
    public Model.Payroll Payroll { get; set; }

    /// <summary>
    /// The case value provider
    /// </summary>
    public CaseValueProvider CaseValueProvider { get; set; }

    /// <summary>
    /// Provider for regulation lookups
    /// </summary>
    public IRegulationLookupProvider RegulationLookupProvider { get; set; }
}