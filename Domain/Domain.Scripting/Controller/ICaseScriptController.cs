using System.Collections.Generic;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Scripting.Runtime;

namespace PayrollEngine.Domain.Scripting.Controller;

/// <summary>Case script controller</summary>
public interface ICaseScriptController : IScriptController<Case>
{
    /// <summary>Test if a case is available</summary>
    /// <param name="settings">The runtime settings</param>
    /// <returns>True if the case is available</returns>
    bool? CaseAvailable(CaseRuntimeSettings settings);

    /// <summary>Builds a case</summary>
    /// <param name="case">The case to test</param>
    /// <param name="settings">The runtime settings</param>
    /// <returns>True if the case is valid</returns>
    bool? CaseBuild(Case @case, CaseChangeRuntimeSettings settings);

    /// <summary>Validates a case</summary>
    /// <param name="case">The case to test</param>
    /// <param name="settings">The runtime settings</param>
    /// <param name="issues">The resulting validation issues</param>
    /// <returns>True if the case is valid</returns>
    bool? CaseValidate(Case @case, CaseChangeRuntimeSettings settings, ICollection<CaseValidationIssue> issues);
}