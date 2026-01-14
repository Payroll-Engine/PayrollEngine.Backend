using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application.Service;

public class PayrunJobServiceSettings
{
    public ICalendarRepository CalendarRepository { get; init; }
    public IUserRepository UserRepository { get; init; }
    public IDivisionRepository DivisionRepository { get; init; }
    public IPayrunRepository PayrunRepository { get; init; }
    public IPayrunJobRepository PayrunJobRepository { get; init; }
    public IPayrollRepository PayrollRepository { get; init; }
    public IPayrollCalculatorProvider PayrollCalculatorProvider { get; init; }
}