using System;
using System.Collections.Generic;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Scripting.Runtime;

namespace PayrollEngine.Domain.Scripting.Controller;

/// <summary>
/// Wage type script controller
/// </summary>
public interface IWageTypeScriptController : IScriptController<WageType>
{
    /// <summary>
    /// Get wage type value
    /// </summary>
    /// <param name="caseFieldProvider">The case field provider</param>
    /// <param name="settings">The runtime settings</param>
    /// <param name="autoPeriodResults">Create custom period results of used wage type case values</param>
    /// <returns>The wage type value, scheduled retro jobs and the execution restart request</returns>
    Tuple<decimal?, List<RetroPayrunJob>, bool> GetValue(ICaseFieldProvider caseFieldProvider,
        WageTypeRuntimeSettings settings, bool autoPeriodResults);

    /// <summary>
    /// Get wage type value
    /// </summary>
    /// <param name="wageTypeValue">The wage type result</param>
    /// <param name="settings">The runtime settings</param>
    /// <returns>The scheduled retro jobs</returns>
    List<RetroPayrunJob> Result(decimal wageTypeValue, WageTypeRuntimeSettings settings);
}