namespace PayrollEngine.Domain.Model.Repository;

/// <summary>
/// Repository for payroll results
/// </summary>
public interface IPayrollResultSetRepository : IChildDomainRepository<PayrollResultSet>
{
    /// <summary>
    /// The wage type result repository
    /// </summary>
    IWageTypeResultSetRepository WageTypeResultSetRepository { get; }

    /// <summary>
    /// The collector result repository
    /// </summary>
    ICollectorResultSetRepository CollectorResultSetRepository { get; }
}