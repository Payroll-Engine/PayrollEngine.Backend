using System;
using PayrollEngine.Domain.Scripting;

namespace PayrollEngine.Domain.Application;

public abstract class FunctionToolBase
{
    protected FunctionToolSettings Settings { get; }
    protected FunctionHost FunctionHost { get; }

    protected FunctionToolBase(FunctionToolSettings settings)
    {
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        FunctionHost = new(settings);
    }
}