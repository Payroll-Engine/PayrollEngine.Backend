using System;
using PayrollEngine.Domain.Scripting;

namespace PayrollEngine.Domain.Application;

public abstract class FunctionToolBase(FunctionToolSettings settings)
{
    protected FunctionToolSettings Settings { get; } = settings ?? throw new ArgumentNullException(nameof(settings));
    protected FunctionHost FunctionHost { get; } = new(settings);
}