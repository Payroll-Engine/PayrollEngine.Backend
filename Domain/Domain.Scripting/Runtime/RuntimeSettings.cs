using PayrollEngine.Domain.Model;

namespace PayrollEngine.Domain.Scripting.Runtime;

public class RuntimeSettings
{
    public IDbContext DbContext { get; init; }
    public IFunctionHost FunctionHost { get; init; }
    public string Culture { get; init; }
    public Tenant Tenant { get; init; }
    public User User { get; init; }
}