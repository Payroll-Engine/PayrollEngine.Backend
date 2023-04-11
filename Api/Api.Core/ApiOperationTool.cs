using System;

namespace PayrollEngine.Api.Core;

internal static class ApiOperationTool
{
    /// <summary>Get operation base name</summary>
    /// <param name="operation">The operation name</param>
    /// <remarks>see als PayrollEngine.Client.Scripting.Function.GetOperationBaseName()</remarks>
    /// <returns>Operation base name</returns>
    internal static string GetOperationBaseName(string operation)
    {
        if (string.IsNullOrWhiteSpace(operation))
        {
            throw new ArgumentException(nameof(operation));
        }
        if (operation.StartsWith("Query"))
        {
            return operation.RemoveFromStart("Query");
        }
        return operation.StartsWith("Get") ?
            operation.RemoveFromStart("Get") :
            operation;
    }
}
