using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Serialization;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Persistence;

public class EmployeeRepository : ChildDomainRepository<Employee>, IEmployeeRepository
{
    public IEmployeeDivisionRepository EmployeeDivisionRepository { get; }

    public EmployeeRepository(IEmployeeDivisionRepository divisionRepository) :
        base(DbSchema.Tables.Employee, DbSchema.EmployeeColumn.TenantId)
    {
        EmployeeDivisionRepository = divisionRepository ?? throw new ArgumentNullException(nameof(divisionRepository));
    }

    protected override void GetObjectCreateData(Employee employee, DbParameterCollection parameters)
    {
        parameters.Add(nameof(employee.Identifier), employee.Identifier);
        base.GetObjectCreateData(employee, parameters);
    }

    protected override void GetObjectData(Employee employee, DbParameterCollection parameters)
    {
        parameters.Add(nameof(employee.FirstName), employee.FirstName);
        parameters.Add(nameof(employee.LastName), employee.LastName);
        parameters.Add(nameof(employee.Culture), employee.Culture);
        parameters.Add(nameof(employee.Calendar), employee.Calendar);
        parameters.Add(nameof(employee.Attributes), JsonSerializer.SerializeNamedDictionary(employee.Attributes));
        base.GetObjectData(employee, parameters);
    }

    public virtual async Task<bool> ExistsAnyAsync(IDbContext context, int tenantId, string identifier) =>
        await ExistsAnyAsync(context, DbSchema.EmployeeColumn.TenantId, tenantId, DbSchema.EmployeeColumn.Identifier, identifier);

    /// <inheritdoc />
    public override async Task<IEnumerable<Employee>> QueryAsync(IDbContext context, int tenantId, Query query = null)
    {
        // division query
        if (query is DivisionQuery divisionQuery && divisionQuery.DivisionId.HasValue)
        {
            // division query
            var dbDivisionQuery = GetDivisionQuery(context, tenantId, query, divisionQuery);

            // SELECT execution
            var employees = (await QueryAsync<Employee>(context, dbDivisionQuery)).ToList();

            // query employee divisions
            if (query.Result == null || query.Result != QueryResultType.Count)
            {
                var divisions = (await EmployeeDivisionRepository.DivisionRepository.QueryAsync(context, tenantId)).ToList();
                foreach (var employee in employees)
                {
                    employee.Divisions = divisions.Select(x => x.Name).ToList();
                }
            }

            // notification
            await OnRetrieved(context, tenantId, employees);
            return employees;
        }

        return await base.QueryAsync(context, tenantId, query);
    }

    /// <inheritdoc />
    public override async Task<long> QueryCountAsync(IDbContext context, int tenantId, Query query = null)
    {
        // division query
        if (query is DivisionQuery divisionQuery && divisionQuery.DivisionId.HasValue)
        {
            // division query
            var dbDivisionQuery = GetDivisionQuery(context, tenantId, query, divisionQuery);
            return await QuerySingleAsync<long>(context, dbDivisionQuery);
        }

        return await base.QueryCountAsync(context, tenantId, query);
    }

    private string GetDivisionQuery(IDbContext context, int tenantId, Query query, DivisionQuery divisionQuery)
    {
        // division query
        var dbQuery = DbQueryFactory.NewQuery<Employee>(context, TableName, ParentFieldName, tenantId, query);
        divisionQuery.ApplyTo(dbQuery, TableName);

        // query compilation
        var compileQuery = CompileQuery(dbQuery);
        return compileQuery;
    }

    #region Divisions (see also Payroll)

    protected override async Task OnRetrieved(IDbContext context, int tenantId, Employee employee)
    {
        // divisions
        var divisions = await GetDivisions(context, tenantId, employee.Id);
        if (divisions.Any())
        {
            // select division names
            employee.Divisions = divisions.Select(x => x.Name).ToList();
        }
    }

    protected override async Task OnCreatedAsync(IDbContext context, int tenantId, Employee employee) =>
        await SaveDivisions(context, tenantId, employee.Id, employee.Divisions);

    protected override async Task OnUpdatedAsync(IDbContext context, int tenantId, Employee employee) =>
        await SaveDivisions(context, tenantId, employee.Id, employee.Divisions);

    protected override async Task<bool> OnDeletingAsync(IDbContext context, int tenantId, int employeeId)
    {
        var divisions = await EmployeeDivisionRepository.QueryAsync(context, employeeId);
        foreach (var division in divisions)
        {
            await EmployeeDivisionRepository.DeleteAsync(context, employeeId, division.Id);
        }
        return await base.OnDeletingAsync(context, tenantId, employeeId);
    }

    private async Task<IList<Division>> GetDivisions(IDbContext context, int tenantId, int employeeId)
    {
        var employeeDivisions = (await EmployeeDivisionRepository.QueryAsync(context, employeeId)).ToList();
        if (!employeeDivisions.Any())
        {
            return new List<Division>();
        }
        var divisionIds = employeeDivisions.Select(x => x.DivisionId);
        return (await EmployeeDivisionRepository.DivisionRepository.GetByIdsAsync(context, tenantId, divisionIds)).ToList();
    }

    private async Task SaveDivisions(IDbContext context, int tenantId, int employeeId, List<string> divisionNames)
    {
        // existing divisions
        var existingDivisions = await GetDivisions(context, tenantId, employeeId);
        if (!existingDivisions.Any() && (divisionNames == null || !divisionNames.Any()))
        {
            // employee without divisions
            return;
        }

        // create new divisions
        if (divisionNames != null)
        {
            foreach (var divisionName in divisionNames)
            {
                var existingDivision = existingDivisions.FirstOrDefault(x => string.Equals(x.Name, divisionName));
                if (existingDivision != null)
                {
                    // ignore division from delete
                    existingDivisions.Remove(existingDivision);
                }
                else
                {
                    // new employee division
                    var division = await EmployeeDivisionRepository.DivisionRepository.GetByNameAsync(context, tenantId, divisionName);
                    if (division == null)
                    {
                        throw new PayrollException($"Unknown division with name {divisionName}");
                    }
                    await EmployeeDivisionRepository.CreateAsync(context, employeeId, new EmployeeDivision
                    {
                        EmployeeId = employeeId,
                        DivisionId = division.Id
                    });
                }
            }
        }

        // delete removed divisions
        while (existingDivisions.Any())
        {
            var division = existingDivisions.First();
            var query = new Query
            {
                Filter = $"{nameof(DbSchema.EmployeeDivisionColumn.DivisionId)} eq {division.Id}"
            };
            var employeeDivision = (await EmployeeDivisionRepository.QueryAsync(context, employeeId, query)).FirstOrDefault();
            if (employeeDivision != null)
            {
                await EmployeeDivisionRepository.DeleteAsync(context, employeeId, employeeDivision.Id);
                existingDivisions.Remove(division);
            }
            else
            {
                throw new PayrollException($"Missing division {division.Id} on employee {employeeId}");
            }
        }
    }

    #endregion
}