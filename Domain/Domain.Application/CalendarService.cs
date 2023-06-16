using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class CalendarService : ChildApplicationService<ICalendarRepository, Calendar>, ICalendarService
{
    public CalendarService(ICalendarRepository repository) :
        base(repository)
    {
    }

    /// <inheritdoc />
    public async Task<Calendar> GetByNameAsync(IDbContext context, int tenantId, string name) =>
        await Repository.GetByNameAsync(context, tenantId, name);
}