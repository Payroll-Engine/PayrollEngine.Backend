using System;

namespace PayrollEngine.Api.Core;

[AttributeUsage(AttributeTargets.Class)]
public sealed class ApiControllerNameAttribute : Attribute
{
    public string ControllerName { get; }

    public ApiControllerNameAttribute(string controllerName)
    {
        if (string.IsNullOrWhiteSpace(controllerName))
        {
            throw new ArgumentException(nameof(controllerName));
        }
        ControllerName = controllerName;
    }
}