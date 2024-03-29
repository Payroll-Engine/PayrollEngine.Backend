﻿using PayrollEngine.Client.Scripting.Runtime;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Domain.Scripting.Runtime;

/// <summary>Runtime for a case change function</summary>
public abstract class CaseRuntimeBase : PayrollRuntimeBase, ICaseRuntime
{
    /// <summary>
    /// The runtime settings
    /// </summary>
    protected new CaseRuntimeSettings Settings => base.Settings as CaseRuntimeSettings;

    /// <summary>The case</summary>
    protected Case Case => Settings.Case;

    /// <inheritdoc />
    protected CaseRuntimeBase(CaseRuntimeSettings settings) :
        base(settings)
    {
    }

    /// <inheritdoc />
    public string CaseName => Case.Name;

    /// <inheritdoc />
    public int CaseType => (int)Case.CaseType;

    /// <summary>The log owner, the source identifier</summary>
    protected override string LogOwner => CaseName;

    /// <inheritdoc />
    public object GetCaseAttribute(string attributeName) =>
        Case.Attributes?.GetValue<object>(attributeName);

}