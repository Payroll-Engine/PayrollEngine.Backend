using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application.Service;

public interface IPayrollResultContextService
{
    IPayrollResultRepository ResultRepository { get; }
    IPayrollConsolidatedResultRepository ConsolidatedResultRepository { get; }

    ICollectorResultRepository CollectorResultRepository { get; init; }
    ICollectorCustomResultRepository CollectorCustomResultRepository { get; init; }
    IWageTypeResultRepository WageTypeResultRepository { get; init; }
    IWageTypeCustomResultRepository WageTypeCustomResultRepository { get; init; }
    IPayrunResultRepository PayrunResultRepository { get; init; }
    IPayrollResultSetRepository ResultSetRepository { get; init; }
}