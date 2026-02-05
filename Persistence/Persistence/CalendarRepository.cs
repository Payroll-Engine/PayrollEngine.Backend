using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Serialization;

namespace PayrollEngine.Persistence;

public class CalendarRepository() : ChildDomainRepository<Calendar>(DbSchema.Tables.Calendar,
    DbSchema.CalendarColumn.TenantId), ICalendarRepository
{
    protected override void GetObjectData(Calendar calendar, DbParameterCollection parameters)
    {
        parameters.Add(nameof(calendar.Name), calendar.Name);
        parameters.Add(nameof(calendar.NameLocalizations), JsonSerializer.SerializeNamedDictionary(calendar.NameLocalizations));
        parameters.Add(nameof(calendar.CycleTimeUnit), calendar.CycleTimeUnit, DbType.Int32);
        parameters.Add(nameof(calendar.PeriodTimeUnit), calendar.PeriodTimeUnit, DbType.Int32);
        parameters.Add(nameof(calendar.TimeMap), calendar.TimeMap, DbType.Int32);
        parameters.Add(nameof(calendar.FirstMonthOfYear), calendar.FirstMonthOfYear, DbType.Int32);
        parameters.Add(nameof(calendar.PeriodDayCount), calendar.PeriodDayCount, DbType.Int32);
        parameters.Add(nameof(calendar.YearWeekRule), calendar.YearWeekRule, DbType.Int32);
        parameters.Add(nameof(calendar.FirstDayOfWeek), calendar.FirstDayOfWeek, DbType.Int32);
        parameters.Add(nameof(calendar.WeekMode), calendar.WeekMode, DbType.Int32);
        parameters.Add(nameof(calendar.WorkMonday), calendar.WorkMonday, DbType.Boolean);
        parameters.Add(nameof(calendar.WorkTuesday), calendar.WorkTuesday, DbType.Boolean);
        parameters.Add(nameof(calendar.WorkWednesday), calendar.WorkWednesday, DbType.Boolean);
        parameters.Add(nameof(calendar.WorkThursday), calendar.WorkThursday, DbType.Boolean);
        parameters.Add(nameof(calendar.WorkFriday), calendar.WorkFriday, DbType.Boolean);
        parameters.Add(nameof(calendar.WorkSaturday), calendar.WorkSaturday, DbType.Boolean);
        parameters.Add(nameof(calendar.WorkSunday), calendar.WorkSunday, DbType.Boolean);
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