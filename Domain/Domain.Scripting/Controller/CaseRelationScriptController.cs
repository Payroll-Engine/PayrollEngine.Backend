//#define SCRIPT_PERFORMANCE
#if SCRIPT_PERFORMANCE
#define LOG_STOPWATCH
#endif
using System.Collections.Generic;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Scripting.Runtime;

namespace PayrollEngine.Domain.Scripting.Controller;

/// <summary>
/// Case relation script controller
/// </summary>
public class CaseRelationScriptController : ScriptControllerBase<CaseRelation>, ICaseRelationScriptController
{
    /// <inheritdoc />
    public bool? CaseRelationBuild(CaseRelation caseRelation, CaseRelationRuntimeSettings settings)
    {
        LogStopwatch.Start(nameof(CaseRelationBuildRuntime));

        // script runtime
        var runtime = new CaseRelationBuildRuntime(caseRelation, settings);

        // script execution
        var result = runtime.ExecuteCaseRelationBuildScript(caseRelation);

        LogStopwatch.Stop(nameof(CaseRelationBuildRuntime));
        return result;
    }

    /// <inheritdoc />
    public bool? CaseRelationValidate(CaseRelation caseRelation, CaseRelationRuntimeSettings settings,
        ICollection<CaseValidationIssue> issues)
    {
        LogStopwatch.Start(nameof(CaseRelationBuildRuntime));

        // script runtime
        var runtime = new CaseRelationValidateRuntime(caseRelation, settings, issues);

        // script execution
        var result = runtime.ExecuteCaseRelationValidateScript(caseRelation);

        LogStopwatch.Stop(nameof(CaseRelationBuildRuntime));
        return result;
    }
}