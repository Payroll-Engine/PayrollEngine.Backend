using PayrollEngine.Domain.Model;

namespace PayrollEngine.Domain.Scripting.Runtime;

public class RuntimeSettings
{
    /// <summary>The database context</summary>
    public IDbContext DbContext { get; init; }

    /// <summary>The function host</summary>
    public IFunctionHost FunctionHost { get; init; }

    /// <summary>The tenant</summary>
    public Tenant Tenant { get; init; }

    /// <summary>The user</summary>
    public User User { get; init; }

    /// <summary>The user culture</summary>
    public string UserCulture { get; init; }
}