using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using PayrollEngine.Client.Scripting.Function;

namespace PayrollEngine.Domain.Scripting;

/// <summary>
/// Script files loaded from the scripting core assembly
/// </summary>
internal static class CodeFactory
{
    private static Assembly Assembly { get; }
    private static List<string> ResourceNames { get; }
    private static Dictionary<string, string> CodeFiles { get; } = new();

    static CodeFactory()
    {
        Assembly = typeof(Function).Assembly;
        ResourceNames = Assembly.GetManifestResourceNames().ToList();
    }

    /// <summary>
    /// The embedded c# scripting files
    /// </summary>
    /// <remarks>Keep this in sync with the PayrollEngine.Client.Scripting project</remarks>
    internal static readonly string[] EmbeddedScriptNames =
    [
        "ClientScript.cs",
        "Extensions.cs",
        "Date.cs",
        "DatePeriod.cs",
        "HourPeriod.cs",
        "Tools.cs",
        "PayrollValue.cs",
        "PeriodValue.cs",
        "CaseObject.cs",
        "CaseValue.cs",
        "CasePayrollValue.cs",
        "Function\\Function.cs",
        "Function\\PayrollFunction.cs",
        "PayrollResults.cs"
    ];

    /// <summary>
    /// Gets embedded source code
    /// </summary>
    /// <param name="resourceName">Name of the resource</param>
    /// <returns>The source code</returns>
    internal static string GetEmbeddedCodeFile(string resourceName)
    {
        // ensure valid embedded resource name
        resourceName = GetCodeFileResourceName(resourceName);

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

    /// <summary>
    /// Get code file resource name
    /// </summary>
    /// <param name="resourceName">Name of the resource</param>
    private static string GetCodeFileResourceName(string resourceName)
    {
        if (string.IsNullOrWhiteSpace(resourceName))
        {
            throw new ArgumentException(nameof(resourceName));
        }

        // extension
        resourceName = resourceName.EnsureEnd(".cs");

        // name check
        if (ResourceNames.Contains(resourceName))
        {
            return resourceName;
        }

        // subfolder map
        resourceName = resourceName.Replace("\\", "/");
        if (!ResourceNames.Contains(resourceName))
        {
            throw new ArgumentException($"Unknown embedded resource {resourceName}", nameof(resourceName));
        }
        return resourceName;
    }
}