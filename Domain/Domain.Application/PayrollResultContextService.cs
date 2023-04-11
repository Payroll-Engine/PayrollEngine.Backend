using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class PayrollResultContextService : IPayrollResultContextService
{
    public IPayrollResultRepository ResultRepository { get; set; }
    public ICollectorResultRepository CollectorResultRepository { get; set; }
    public ICollectorCustomResultRepository CollectorCustomResultRepository { get; set; }
    public IWageTypeResultRepository WageTypeResultRepository { get; set; }
    public IWageTypeCustomResultRepository WageTypeCustomResultRepository { get; set; }
    public IPayrunResultRepository PayrunResultRepository { get; set; }
    public IPayrollConsolidatedResultRepository ConsolidatedResultRepository { get; set; }
    public IPayrollResultSetRepository ResultSetRepository { get; set; }
}