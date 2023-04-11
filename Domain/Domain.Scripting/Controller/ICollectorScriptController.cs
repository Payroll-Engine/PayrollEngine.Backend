using System;
using System.Collections.Generic;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Scripting.Runtime;

namespace PayrollEngine.Domain.Scripting.Controller;

/// <summary>
/// Collector script controller
/// </summary>
public interface ICollectorScriptController : IScriptController<Collector>
{
    /// <summary>
    /// Start the collector
    /// </summary>
    /// <param name="settings">The runtime settings</param>
    /// <returns>The scheduled retro jobs</returns>
    List<RetroPayrunJob> Start(CollectorRuntimeSettings settings);

    /// <summary>
    /// Apply the collector value
    /// </summary>
    /// <param name="wageTypeResult">The wage type result results</param>
    /// <param name="settings">The runtime settings</param>
    /// <returns>The collector value to apply and the scheduled retro jobs</returns>
    Tuple<decimal?, List<RetroPayrunJob>> ApplyValue(WageTypeResult wageTypeResult,
        CollectorRuntimeSettings settings);

    /// <summary>
    /// End the collector
    /// </summary>
    /// <param name="settings">The runtime settings</param>
    /// <returns>The scheduled retro jobs</returns>
    List<RetroPayrunJob> End(CollectorRuntimeSettings settings);
}