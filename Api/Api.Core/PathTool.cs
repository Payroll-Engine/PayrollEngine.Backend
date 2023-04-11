using System;
using System.IO;
using System.Reflection;

namespace PayrollEngine.Api.Core;

public static class PathTool
{
    public static string GetXmlCommentsFileName()
    {
        var assembly = Assembly.GetEntryAssembly();
        if (assembly == null)
        {
            throw new PayrollException("missing entry assembly");
        }

        var xmlCommentsFile = $"{assembly.GetName().Name}{FileExtensions.Xml}";
        return Path.Combine(AppContext.BaseDirectory, xmlCommentsFile);
    }
}