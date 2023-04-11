namespace PayrollEngine.Domain.Model.Repository;

/// <summary>
/// Repository for wage type results
/// </summary>
public interface IWageTypeResultSetRepository : IChildDomainRepository<WageTypeResultSet>
{
    /// <summary>
    /// The wage type custom result repository
    /// </summary>
    IWageTypeCustomResultRepository WageTypeCustomResultRepository { get; }
}