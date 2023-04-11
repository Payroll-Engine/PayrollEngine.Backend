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

    public EmployeeRepository(IEmployeeDivisionRepository divisionRepository, IDbContext context) :
        base(DbSchema.Tables.Employee, DbSchema.EmployeeColumn.TenantId, context)
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
        parameters.Add(nameof(employee.Language), employee.Language);
        parameters.Add(nameof(employee.Culture), employee.Culture);
        parameters.Add(nameof(employee.Attributes), JsonSerializer.SerializeNamedDictionary(employee.Attributes));
        base.GetObjectData(employee, parameters);
    }

    public virtual async Task<bool> ExistsAnyAsync(int tenantId, string identifier) =>
        await ExistsAnyAsync(DbSchema.EmployeeColumn.TenantId, tenantId, DbSchema.EmployeeColumn.Identifier, identifier);

    /// <inheritdoc />
    public override async Task<IEnumerable<Employee>> QueryAsync(int tenantId, Query query = null)
    {
        // division query
        if (query is DivisionQuery divisionQuery && divisionQuery.DivisionId.HasValue)
        {
            // division query
            var dbDivisionQuery = GetDivisionQuery(tenantId, query, divisionQuery);

            // SELECT execution
            var employees = (await QueryAsync<Employee>(dbDivisionQuery)).ToList();

            // query employee divisions
            if (query.Result == null || query.Result != QueryResultType.Count)
            {
                var divisions = (await EmployeeDivisionRepository.DivisionRepository.QueryAsync(tenantId)).ToList();
                foreach (var employee in employees)
                {
                    employee.Divisions = divisions.Select(x => x.Name).ToList();
                }
            }

            // notification
            await OnRetrieved(tenantId, employees);
            return employees;
        }

        return await base.QueryAsync(tenantId, query);
    }

    /// <inheritdoc />
    public override async Task<long> QueryCountAsync(int tenantId, Query query = null)
    {
        // division query
        if (query is DivisionQuery divisionQuery && divisionQuery.DivisionId.HasValue)
        {
            // division query
            var dbDivisionQuery = GetDivisionQuery(tenantId, query, divisionQuery);
            return await QuerySingleAsync<long>(dbDivisionQuery);
        }

        return await base.QueryCountAsync(tenantId, query);
    }

    private string GetDivisionQuery(int tenantId, Query query, DivisionQuery divisionQuery)
    {
        // division query
        var dbQuery = DbQueryFactory.NewQuery<Employee>(Context, TableName, ParentFieldName, tenantId, query);
        divisionQuery.ApplyTo(dbQuery, TableName);

        // query compilation
        var compileQuery = CompileQuery(dbQuery);
        return compileQuery;
    }

    #region Divisions (see also Payroll)

    protected override async Task OnRetrieved(int tenantId, Employee employee)
    {
        // divisions
        var divisions = await GetDivisions(tenantId, employee.Id);
        if (divisions.Any())
        {
            // select division names
            employee.Divisions = divisions.Select(x => x.Name).ToList();
        }
    }

    protected override async Task OnCreatedAsync(int tenantId, Employee employee)
    {
        await SaveDivisions(tenantId, employee.Id, employee.Divisions);
    }

    protected override async Task OnUpdatedAsync(int tenantId, Employee employee)
    {
        await SaveDivisions(tenantId, employee.Id, employee.Divisions);
    }

    protected override async Task<bool> OnDeletingAsync(int tenantId, int employeeId)
    {
        var divisions = await EmployeeDivisionRepository.QueryAsync(employeeId);
        foreach (var division in divisions)
        {
            await EmployeeDivisionRepository.DeleteAsync(employeeId, division.Id);
        }
        return await base.OnDeletingAsync(tenantId, employeeId);
    }

    private async Task<IList<Division>> GetDivisions(int tenantId, int employeeId)
    {
        var employeeDivisions = (await EmployeeDivisionRepository.QueryAsync(employeeId)).ToList();
        if (!employeeDivisions.Any())
        {
            return new List<Division>();
        }
        var divisionIds = employeeDivisions.Select(x => x.DivisionId);
        return (await EmployeeDivisionRepository.DivisionRepository.GetByIdsAsync(tenantId, divisionIds)).ToList();
    }

    private async Task SaveDivisions(int tenantId, int employeeId, List<string> divisionNames)
    {
        // existing divisions
        var existingDivisions = await GetDivisions(tenantId, employeeId);
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
                    var division = await EmployeeDivisionRepository.DivisionRepository.GetByNameAsync(tenantId, divisionName);
                    if (division == null)
                    {
                        throw new PayrollException($"Unknown division with name {divisionName}");
                    }
                    await EmployeeDivisionRepository.CreateAsync(employeeId, new EmployeeDivision
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
            var employeeDivision = (await EmployeeDivisionRepository.QueryAsync(employeeId, query)).FirstOrDefault();
            if (employeeDivision != null)
            {
                await EmployeeDivisionRepository.DeleteAsync(employeeId, employeeDivision.Id);
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