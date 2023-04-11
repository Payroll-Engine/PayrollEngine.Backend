using System;

namespace PayrollEngine.Api.Core;

[AttributeUsage(AttributeTargets.Method)]
public sealed class ApiOperationIdAttribute : Attribute
{
    public string OperationId { get; }

    public ApiOperationIdAttribute(string operationId)
    {
        if (string.IsNullOrWhiteSpace(operationId))
        {
            throw new ArgumentException(nameof(operationId));
        }
        OperationId = operationId;
    }
}