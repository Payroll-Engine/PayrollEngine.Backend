using System.Collections.Generic;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Scripting.Runtime;

namespace PayrollEngine.Domain.Scripting.Controller;

/// <summary>
/// Case relation script controller
/// </summary>
public interface ICaseRelationScriptController : IScriptController<CaseRelation>
{
    /// <summary>
    /// Build related case
    /// </summary>
    /// <param name="caseRelation">The case relation to test</param>
    /// <param name="settings">The runtime settings</param>
    /// <returns>True for a valid case relation</returns>
    bool? CaseRelationBuild(CaseRelation caseRelation, CaseRelationRuntimeSettings settings);

    /// <summary>
    /// Validate related case
    /// </summary>
    /// <param name="caseRelation">The case relation to test</param>
    /// <param name="settings">The runtime settings</param>
    /// <param name="issues">The resulting validation issues</param>
    /// <returns>True for a valid case relation</returns>
    bool? CaseRelationValidate(CaseRelation caseRelation, CaseRelationRuntimeSettings settings,
        ICollection<CaseValidationIssue> issues);
}