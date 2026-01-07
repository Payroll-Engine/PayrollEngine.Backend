using System;

namespace PayrollEngine.Domain.Scripting.Action;

internal sealed class ActionResult
{
    internal ActionResult(string invokeCode, string code)
    {
        if (string.IsNullOrWhiteSpace(invokeCode))
        {
            throw new ArgumentException(nameof(invokeCode));
        }
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException(nameof(code));
        }

        InvokeCode = invokeCode;
        Code = code;
    }

    internal string InvokeCode { get; }
    internal string Code { get; }
}