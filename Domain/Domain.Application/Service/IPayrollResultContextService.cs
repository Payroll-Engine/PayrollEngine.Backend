using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application.Service;

public interface IPayrollResultContextService
{
    IPayrollResultRepository ResultRepository { get; set; }
    ICollectorResultRepository CollectorResultRepository { get; set; }
    ICollectorCustomResultRepository CollectorCustomResultRepository { get; set; }
    IWageTypeResultRepository WageTypeResultRepository { get; set; }
    IWageTypeCustomResultRepository WageTypeCustomResultRepository { get; set; }
    IPayrunResultRepository PayrunResultRepository { get; set; }
    IPayrollResultSetRepository ResultSetRepository { get; set; }
    IPayrollConsolidatedResultRepository ConsolidatedResultRepository { get; }
}