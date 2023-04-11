namespace PayrollEngine.Domain.Model.Repository;

/// <summary>
/// Repository for collector results
/// </summary>
public interface ICollectorResultSetRepository : IChildDomainRepository<CollectorResultSet>
{
    /// <summary>
    /// The collector custom result repository
    /// </summary>
    ICollectorCustomResultRepository CollectorCustomResultRepository { get; }
}