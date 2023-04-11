//#define SCRIPT_PERFORMANCE
#if SCRIPT_PERFORMANCE
#define LOG_STOPWATCH
#endif
using System.Collections.Generic;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Scripting.Runtime;

namespace PayrollEngine.Domain.Scripting.Controller;

/// <summary>
/// Payrun script controller
/// </summary>
public class PayrunScriptController : ScriptControllerBase<Payrun>, IPayrunScriptController
{

    #region Start

    /// <inheritdoc />
    public bool? Start(PayrunRuntimeSettings settings)
    {
        LogStopwatch.Start(nameof(PayrunStartRuntime));

        // script runtime
        var runtime = new PayrunStartRuntime(settings);

        // script execution
        var start = runtime.ExecuteStartScript(settings.Payrun);

        LogStopwatch.Stop(nameof(PayrunStartRuntime));

        return start;
    }

    #endregion

    #region Employee Available

    /// <inheritdoc />
    public bool? IsEmployeeAvailable(PayrunRuntimeSettings settings)
    {
        LogStopwatch.Start(nameof(PayrunEmployeeAvailableRuntime));

        // script runtime
        var runtime = new PayrunEmployeeAvailableRuntime(settings);

        // script execution
        var isAvailable = runtime.ExecuteIsEmployeeAvailableScript(settings.Payrun);

        LogStopwatch.Stop(nameof(PayrunEmployeeAvailableRuntime));

        return isAvailable;
    }

    /// <inheritdoc />
    public bool? EmployeeStart(PayrunRuntimeSettings settings)
    {
        LogStopwatch.Start(nameof(PayrunEmployeeStartRuntime));

        // script runtime
        var runtime = new PayrunEmployeeStartRuntime(settings);

        // script execution
        var start = runtime.ExecuteEmployeeStartScript(settings.Payrun);

        LogStopwatch.Stop(nameof(PayrunEmployeeStartRuntime));

        return start;
    }

    /// <inheritdoc />
    public void EmployeeEnd(PayrunRuntimeSettings settings)
    {
        LogStopwatch.Start(nameof(PayrunEmployeeEndRuntime));

        // script runtime
        var runtime = new PayrunEmployeeEndRuntime(settings);

        // script execution
        runtime.ExecuteEmployeeEndScript(settings.Payrun);

        LogStopwatch.Stop(nameof(PayrunEmployeeEndRuntime));
    }

    #endregion

    #region Wage Type Available

    /// <inheritdoc />
    public bool? IsWageTypeAvailable(WageType wageType, Dictionary<string, object> wageTypeAttributes, PayrunRuntimeSettings settings)
    {
        LogStopwatch.Start(nameof(PayrunWageTypeAvailableRuntime));

        // script runtime
        var runtime = new PayrunWageTypeAvailableRuntime(wageType, wageTypeAttributes, settings);

        // script execution
        var isAvailable = runtime.ExecuteIsWageTypeAvailableScript(settings.Payrun);

        LogStopwatch.Stop(nameof(PayrunWageTypeAvailableRuntime));

        return isAvailable;
    }

    #endregion

    #region End

    /// <inheritdoc />
    public void End(PayrunRuntimeSettings settings)
    {
        LogStopwatch.Start(nameof(PayrunEndRuntime));

        // script runtime
        var runtime = new PayrunEndRuntime(settings);

        // script execution
        runtime.ExecuteEndScript(settings.Payrun);

        LogStopwatch.Stop(nameof(PayrunEndRuntime));
    }

    #endregion

}