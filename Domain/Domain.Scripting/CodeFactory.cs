using System;
using System.Collections.Generic;
using System.Reflection;
using PayrollEngine.Client.Scripting.Function;

namespace PayrollEngine.Domain.Scripting;

/// <summary>
/// Script files loaded from the scripting core assembly
/// </summary>
internal static class CodeFactory
{
    private static Assembly Assembly { get; }
    private static Dictionary<string, string> CodeFiles { get; } = new();

    static CodeFactory()
    {
        Assembly = typeof(Function).Assembly;
    }

    /// <summary>
    /// The embedded c# scripting files
    /// </summary>
    internal static readonly string[] EmbeddedScriptNames = {
        "ClientScript.cs",
        "Extensions.cs",
        "Date.cs",
        "DatePeriod.cs",
        "Tools.cs",
        "Local\\Locals.cs",
        "PayrollValue.cs",
        "PeriodValue.cs",
        "CaseValue.cs",
        "CasePayrollValue.cs",
        "Function\\Function.cs",
        "Function\\PayrollFunction.cs",
        "PayrollResults.cs"
    };

    /// <summary>
    /// Gets embedded source code
    /// </summary>
    /// <param name="resourceName">Name of the resource</param>
    /// <returns>The source code</returns>
    internal static string GetEmbeddedCodeFile(string resourceName)
    {
        if (string.IsNullOrWhiteSpace(resourceName))
        {
            throw new ArgumentException(nameof(resourceName));
        }

        resourceName = resourceName.EnsureEnd(".cs");

        string codeFile;
        if (CodeFiles.TryGetValue(resourceName, out var file))
        {
            codeFile = file;
        }
        else
        {
            codeFile = Assembly.GetEmbeddedFile(resourceName);
            CodeFiles.Add(resourceName, codeFile);
        }
        return codeFile;
    }
}