using System;
using System.Linq;
using System.Reflection;
using System.Collections.Concurrent;
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
    private static ConcurrentDictionary<string, string> CodeFiles { get; } = new();

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

        // thread-safe cache lookup with atomic insert on miss
        return CodeFiles.GetOrAdd(resourceName, static (key, asm) => asm.GetEmbeddedFile(key), Assembly);
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