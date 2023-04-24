using PayrollEngine.Domain.Model;

namespace PayrollEngine.Domain.Scripting.Runtime;

public class RuntimeSettings
{
    public IDbContext DbContext { get; set; }
    public IFunctionHost FunctionHost { get; set; }
    public Tenant Tenant { get; set; }
    public User User { get; set; }
}