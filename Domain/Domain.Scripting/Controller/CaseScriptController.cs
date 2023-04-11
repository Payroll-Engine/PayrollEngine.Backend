//#define SCRIPT_PERFORMANCE
#if SCRIPT_PERFORMANCE
#define LOG_STOPWATCH
#endif
using System.Collections.Generic;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Scripting.Runtime;

namespace PayrollEngine.Domain.Scripting.Controller;

/// <summary>Case script controller</summary>
public class CaseScriptController : ScriptControllerBase<Case>, ICaseScriptController
{

    /// <inheritdoc />
    public bool? CaseAvailable(CaseRuntimeSettings settings)
    {
        LogStopwatch.Start(nameof(CaseAvailableRuntime));

        // script runtime
        var runtime = new CaseAvailableRuntime(settings);

        // script execution
        var available = runtime.ExecuteCaseAvailableScript();

        LogStopwatch.Stop(nameof(CaseAvailableRuntime));

        return available;
    }

    /// <inheritdoc />
    public bool? CaseBuild(Case @case, CaseChangeRuntimeSettings settings)
    {
        LogStopwatch.Start(nameof(CaseBuildRuntime));

        // script runtime
        var runtime = new CaseBuildRuntime(settings);

        // script execution
        var build = runtime.ExecuteCaseBuildScript(@case);

        LogStopwatch.Stop(nameof(CaseBuildRuntime));

        return build;
    }

    /// <inheritdoc />
    public bool? CaseValidate(Case @case, CaseChangeRuntimeSettings settings, ICollection<CaseValidationIssue> issues)
    {
        LogStopwatch.Start(nameof(CaseValidateRuntime));

        // script runtime
        var runtime = new CaseValidateRuntime(settings, issues);

        // script execution
        var valid = runtime.ExecuteCaseValidateScript(@case);

        LogStopwatch.Stop(nameof(CaseValidateRuntime));

        return valid;
    }
}