using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Persistence.DbSchema;
using PayrollEngine.Serialization;

namespace PayrollEngine.Persistence;

/// <summary>Repository for <see cref="Division"/> persistence (table: Division).</summary>
public class DivisionRepository() : ChildDomainRepository<Division>(Tables.Division,
    DivisionColumn.TenantId), IDivisionRepository
{
    protected override void GetObjectData(Division division, DbParameterCollection parameters)
    {
        parameters.Add(nameof(division.Name), division.Name);
        parameters.Add(nameof(division.NameLocalizations), JsonSerializer.SerializeNamedDictionary(division.NameLocalizations));
        parameters.Add(nameof(division.Culture), division.Culture);
        parameters.Add(nameof(division.Calendar), division.Calendar);
        parameters.Add(nameof(division.Attributes), JsonSerializer.SerializeNamedDictionary(division.Attributes));
        base.GetObjectData(division, parameters);
    }

    public async Task<IEnumerable<Division>> GetByIdsAsync(IDbContext context, int tenantId, IEnumerable<int> divisionIds)
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
        query.WhereIn(ObjectColumn.Id, divisionIds);

        // execute query
        var compileQuery = CompileQuery(query, context);
        return await QueryAsync<Division>(context, compileQuery);
    }

    public async Task<Division> GetByNameAsync(IDbContext context, int tenantId, string name)
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
        query.WhereIn(DivisionColumn.Name, name);

        // execute query
        var compileQuery = CompileQuery(query, context);
        return (await QueryAsync(context, compileQuery)).FirstOrDefault();
    }
}