﻿using System;
using System.Runtime.CompilerServices;
using PayrollEngine.Client.Scripting;
using PayrollEngine.Client.Scripting.Function;
using PayrollEngine.Client.Scripting.Runtime;
using PayrollEngine.Domain.Model;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Domain.Scripting.Runtime;

/// <summary>
/// Runtime for the case relation build function
/// </summary>
public class CaseRelationBuildRuntime : CaseRelationRuntimeBase, ICaseRelationBuildRuntime
{
    /// <inheritdoc />
    internal CaseRelationBuildRuntime(CaseRelation caseRelation, CaseRelationRuntimeSettings settings) :
        base(caseRelation, settings)
    {
    }

    /// <summary>The log owner type</summary>
    protected override string LogOwnerType => nameof(CaseRelationBuildFunction);

    /// <inheritdoc />
    public string[] GetBuildActions() =>
        CaseRelation.BuildActions == null ? Array.Empty<string>() :
            CaseRelation.BuildActions.ToArray();

    /// <summary>
    /// Execute the case relation build script
    /// </summary>
    /// <param name="caseRelation">The case relation</param>
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal bool? ExecuteCaseRelationBuildScript(CaseRelation caseRelation)
    {
        try
        {
            var task = Task.Factory.StartNew<bool?>(() =>
            {
                // create script
                using var script = CreateScript(typeof(CaseRelationBuildFunction), caseRelation);
                // call the script function
                return script.Build();
            });
            return task.WaitScriptResult(typeof(CaseRelationBuildFunction), Timeout);
        }
        catch (ScriptException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new ScriptException($"Build script error in case relation {caseRelation}: {exception.GetBaseMessage()}", exception);
        }
    }
}