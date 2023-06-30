using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class PayrollResultContextService : IPayrollResultContextService
{
    public IPayrollResultRepository ResultRepository { get; init; }
    public IPayrollConsolidatedResultRepository ConsolidatedResultRepository { get; init; }

    public ICollectorResultRepository CollectorResultRepository { get; init; }
    public ICollectorCustomResultRepository CollectorCustomResultRepository { get; init; }
    public IWageTypeResultRepository WageTypeResultRepository { get; init; }
    public IWageTypeCustomResultRepository WageTypeCustomResultRepository { get; init; }
    public IPayrunResultRepository PayrunResultRepository { get; init; }
    public IPayrollResultSetRepository ResultSetRepository { get; init; }
}