using System;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Serialization;

namespace PayrollEngine.Persistence;

public class CalendarRepository : ChildDomainRepository<Calendar>, ICalendarRepository
{
    public CalendarRepository() :
        base(DbSchema.Tables.Calendar, DbSchema.CalendarColumn.TenantId)
    {
    }

    protected override void GetObjectData(Calendar calendar, DbParameterCollection parameters)
    {
        parameters.Add(nameof(calendar.Name), calendar.Name);
        parameters.Add(nameof(calendar.NameLocalizations), JsonSerializer.SerializeNamedDictionary(calendar.NameLocalizations));
        parameters.Add(nameof(calendar.CycleTimeUnit), calendar.CycleTimeUnit);
        parameters.Add(nameof(calendar.PeriodTimeUnit), calendar.PeriodTimeUnit);
        parameters.Add(nameof(calendar.TimeMap), calendar.TimeMap);
        parameters.Add(nameof(calendar.FirstMonthOfYear), calendar.FirstMonthOfYear);
        parameters.Add(nameof(calendar.MonthDayCount), calendar.MonthDayCount);
        parameters.Add(nameof(calendar.YearWeekRule), calendar.YearWeekRule);
        parameters.Add(nameof(calendar.FirstDayOfWeek), calendar.FirstDayOfWeek);
        parameters.Add(nameof(calendar.WeekMode), calendar.WeekMode);
        parameters.Add(nameof(calendar.WorkMonday), calendar.WorkMonday);
        parameters.Add(nameof(calendar.WorkTuesday), calendar.WorkTuesday);
        parameters.Add(nameof(calendar.WorkWednesday), calendar.WorkWednesday);
        parameters.Add(nameof(calendar.WorkThursday), calendar.WorkThursday);
        parameters.Add(nameof(calendar.WorkFriday), calendar.WorkFriday);
        parameters.Add(nameof(calendar.WorkSaturday), calendar.WorkSaturday);
        parameters.Add(nameof(calendar.WorkSunday), calendar.WorkSunday);
        parameters.Add(nameof(calendar.Attributes), JsonSerializer.SerializeNamedDictionary(calendar.Attributes));
        base.GetObjectData(calendar, parameters);
    }

    public async Task<Calendar> GetByNameAsync(IDbContext context, int tenantId, string name)
    {
        if (tenantId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tenantId));
        }
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(nameof(name));
        }

        // query
        var query = DbQueryFactory.NewQuery(TableName, ParentFieldName, tenantId);

        // filter by calendar ids
        query.WhereIn(DbSchema.CalendarColumn.Name, name);

        // execute query
        var compileQuery = CompileQuery(query);
        return (await QueryAsync(context, compileQuery)).FirstOrDefault();
    }
}