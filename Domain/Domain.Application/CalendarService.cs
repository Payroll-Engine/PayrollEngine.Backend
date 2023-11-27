using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class CalendarService
    (ICalendarRepository repository) : ChildApplicationService<ICalendarRepository, Calendar>(repository),
        ICalendarService
{
    /// <inheritdoc />
    public async Task<Calendar> GetByNameAsync(IDbContext context, int tenantId, string name) =>
        await Repository.GetByNameAsync(context, tenantId, name);
}