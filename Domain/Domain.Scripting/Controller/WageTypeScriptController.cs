//#define SCRIPT_PERFORMANCE
#if SCRIPT_PERFORMANCE
#define LOG_STOPWATCH
#endif
using System;
using System.Collections.Generic;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Scripting.Runtime;

namespace PayrollEngine.Domain.Scripting.Controller;

/// <summary>
/// Wage type script controller
/// </summary>
public class WageTypeScriptController : ScriptControllerBase<WageType>, IWageTypeScriptController
{

    #region Value

    /// <inheritdoc />
    public Tuple<decimal?, List<RetroPayrunJob>,bool> GetValue(CaseFieldProvider caseFieldProvider,
        WageTypeRuntimeSettings settings, bool autoPeriodResults)
    {
        LogStopwatch.Start(nameof(WageTypeValueRuntime));

        // script runtime
        var runtime = new WageTypeValueRuntime(caseFieldProvider, settings);

        // wage type value
        var wageTypeValue = runtime.EvaluateValue(settings.WageType);

        LogStopwatch.Stop(nameof(WageTypeValueRuntime));

        // payrun results
        settings.CurrentPayrollResult.PayrunResults.AddRange(runtime.PayrunResults);

        // empty result
        if (wageTypeValue == null)
        {
            return null;
        }

        // auto generated custom period results
        if (autoPeriodResults)
        {
            // requested case fields
            foreach (var field in runtime.RequestedFields)
            {
                // item1: created date
                // item2: start date
                // item3: end date
                // item4: case value
                var periodValues = runtime.GetCasePeriodValues(runtime.EvaluationPeriod.Start,
                    runtime.EvaluationPeriod.End, field);
                foreach (var periodValue in periodValues)
                {
                    foreach (var value in periodValue.Value)
                    {
                        if (value.Item4 is decimal decimalValue)
                        {
                            var period = new DatePeriod(value.Item2, value.Item3);
                            runtime.AddCustomResult(periodValue.Key, decimalValue,
                                period.Start, period.End, null, null, null);
                        }
                    }
                }
            }
        }

        return new(wageTypeValue, runtime.RetroJobs, runtime.ExecutionRestartRequest);
    }

    #endregion

    #region Result

    /// <inheritdoc />
    public List<RetroPayrunJob> Result(decimal wageTypeValue, WageTypeRuntimeSettings settings)
    {
        LogStopwatch.Start(nameof(WageTypeResultRuntime));

        // script runtime
        var runtime = new WageTypeResultRuntime(wageTypeValue, settings);

        // wage type result
        runtime.EvaluateResult(settings.WageType);

        LogStopwatch.Stop(nameof(WageTypeResultRuntime));

        // payrun results
        settings.CurrentPayrollResult.PayrunResults.AddRange(runtime.PayrunResults);

        return runtime.RetroJobs;
    }

    #endregion

}