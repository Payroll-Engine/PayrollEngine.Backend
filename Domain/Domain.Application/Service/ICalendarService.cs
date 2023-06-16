using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using System.Threading.Tasks;

namespace PayrollEngine.Domain.Application.Service;

public interface ICalendarService : IChildApplicationService<ICalendarRepository, Calendar>
{
    /// <summary>
    /// Get calendar by name
    /// </summary>
    /// <param name="context">The database context</param>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="name">The calendar name</param>
    /// <returns>The calendar matching the name</returns>
    Task<Calendar> GetByNameAsync(IDbContext context, int tenantId, string name);
}