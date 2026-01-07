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
    /// Gets embedded source code
    /// </summary>
    /// <param name="resourceName">Name of the resource</param>
    /// <returns>The source code</returns>
    internal static string GetEmbeddedCodeFile(string resourceName)
    {
        resourceName = EnsureResourceName(resourceName);

        // cache success
        if (CodeFiles.TryGetValue(resourceName, out var file))
        {
            return file;
        }

        // load embedded resource
        var codeFile = Assembly.GetEmbeddedFile(resourceName);

        // cache update
        CodeFiles.Add(resourceName, codeFile);

        return codeFile;
    }

    /// <summary>
    /// Ensure valid embedded resource name
    /// </summary>
    /// <param name="resourceName">Name of the resource</param>
    private static string EnsureResourceName(string resourceName)
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