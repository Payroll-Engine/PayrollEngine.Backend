using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Serialization;

namespace PayrollEngine.Persistence;

public class DivisionRepository : ChildDomainRepository<Division>, IDivisionRepository
{
    public DivisionRepository() :
        base(DbSchema.Tables.Division, DbSchema.DivisionColumn.TenantId)
    {
    }

    protected override void GetObjectData(Division division, DbParameterCollection parameters)
    {
        parameters.Add(nameof(division.Name), division.Name);
        parameters.Add(nameof(division.NameLocalizations), JsonSerializer.SerializeNamedDictionary(division.NameLocalizations));
        parameters.Add(nameof(division.Culture), division.Culture);
        parameters.Add(nameof(division.Attributes), JsonSerializer.SerializeNamedDictionary(division.Attributes));
        base.GetObjectData(division, parameters);
    }

    public virtual async Task<IEnumerable<Division>> GetByIdsAsync(IDbContext context, int tenantId, IEnumerable<int> divisionIds)
    {
        if (tenantId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tenantId));
        }
        if (divisionIds == null)
        {
            throw new ArgumentNullException(nameof(divisionIds));
        }

        // query
        var query = DbQueryFactory.NewQuery(TableName, ParentFieldName, tenantId);

        // filter by division ids
        query.WhereIn(DbSchema.ObjectColumn.Id, divisionIds);

        // execute query
        var compileQuery = CompileQuery(query);
        return await QueryAsync<Division>(context, compileQuery);
    }

    public virtual async Task<Division> GetByNameAsync(IDbContext context, int tenantId, string name)
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

        // filter by division ids
        query.WhereIn(DbSchema.DivisionColumn.Name, name);

        // execute query
        var compileQuery = CompileQuery(query);
        return (await QueryAsync(context, compileQuery)).FirstOrDefault();
    }
}