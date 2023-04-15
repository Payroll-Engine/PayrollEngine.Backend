using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Persistence.DbQuery;
using PayrollEngine.Serialization;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Persistence;

public class PayrunJobRepository : ChildDomainRepository<PayrunJob>, IPayrunJobRepository
{
    public IPayrunJobEmployeeRepository JobEmployeeRepository { get; }

    public PayrunJobRepository(IPayrunJobEmployeeRepository jobEmployeeRepository, IDbContext context) :
        base(DbSchema.Tables.PayrunJob, DbSchema.PayrunJobColumn.TenantId, context)
    {
        JobEmployeeRepository = jobEmployeeRepository ?? throw new ArgumentNullException(nameof(jobEmployeeRepository));
    }

    protected override void GetObjectCreateData(PayrunJob payrunJob, DbParameterCollection parameters)
    {
        // local fields
        // keep in sync with object properties
        parameters.Add(nameof(payrunJob.Name), payrunJob.Name);
        parameters.Add(nameof(payrunJob.Owner), payrunJob.Owner);
        parameters.Add(nameof(payrunJob.PayrunId), payrunJob.PayrunId);
        parameters.Add(nameof(payrunJob.PayrollId), payrunJob.PayrollId);
        parameters.Add(nameof(payrunJob.DivisionId), payrunJob.DivisionId);
        parameters.Add(nameof(payrunJob.UserId), payrunJob.UserId);
        parameters.Add(nameof(payrunJob.ParentJobId), payrunJob.ParentJobId);
        parameters.Add(nameof(payrunJob.Tags), JsonSerializer.SerializeList(payrunJob.Tags));
        parameters.Add(nameof(payrunJob.Forecast), payrunJob.Forecast);
        parameters.Add(nameof(payrunJob.RetroPayMode), payrunJob.RetroPayMode);
        parameters.Add(nameof(payrunJob.JobResult), payrunJob.JobResult);
        parameters.Add(nameof(payrunJob.Culture), payrunJob.Culture);
        parameters.Add(nameof(payrunJob.CycleName), payrunJob.CycleName);
        parameters.Add(nameof(payrunJob.CycleStart), payrunJob.CycleStart);
        parameters.Add(nameof(payrunJob.CycleEnd), payrunJob.CycleEnd);
        parameters.Add(nameof(payrunJob.PeriodName), payrunJob.PeriodName);
        parameters.Add(nameof(payrunJob.PeriodStart), payrunJob.PeriodStart);
        parameters.Add(nameof(payrunJob.PeriodEnd), payrunJob.PeriodEnd);
        parameters.Add(nameof(payrunJob.EvaluationDate), payrunJob.EvaluationDate);
        parameters.Add(nameof(payrunJob.Reason), payrunJob.Reason);

        // base fields
        base.GetObjectCreateData(payrunJob, parameters);
    }

    protected override void GetObjectData(PayrunJob payrunJob, DbParameterCollection parameters)
    {
        parameters.Add(nameof(payrunJob.JobStatus), payrunJob.JobStatus);
        parameters.Add(nameof(payrunJob.TotalEmployeeCount), payrunJob.TotalEmployeeCount);
        parameters.Add(nameof(payrunJob.ProcessedEmployeeCount), payrunJob.ProcessedEmployeeCount);
        parameters.Add(nameof(payrunJob.JobStart), payrunJob.JobStart);
        parameters.Add(nameof(payrunJob.JobEnd), payrunJob.JobEnd);
        parameters.Add(nameof(payrunJob.Message), payrunJob.Message);
        parameters.Add(nameof(payrunJob.ErrorMessage), payrunJob.ErrorMessage);
        parameters.Add(nameof(payrunJob.Attributes), JsonSerializer.SerializeNamedDictionary(payrunJob.Attributes));
        base.GetObjectData(payrunJob, parameters);
    }

    public virtual async Task<IEnumerable<PayrunJob>> QueryEmployeePayrunJobsAsync(int tenantId, int employeeId, Query query = null)
    {
        // query
        var dbQuery = DbQueryFactory.NewQuery<PayrunJob>(Context, TableName, query);

        // join payrun job to job employee
        dbQuery
            .Select($"{TableName}.*")
            .Join(DbSchema.Tables.PayrunJobEmployee,
                GetIdColumnName(),
                GetColumnName(DbSchema.Tables.PayrunJobEmployee, DbSchema.PayrunJobEmployeeColumn.PayrunJobId))
            .Where(DbSchema.PayrunJobEmployeeColumn.EmployeeId, employeeId);

        // query compilation
        var compileQuery = CompileQuery(dbQuery);

        // SELECT execution
        var items = (await QueryAsync<PayrunJob>(compileQuery)).ToList();

        // notification
        await OnRetrieved(tenantId, items);

        return items;
    }

    public virtual async Task<long> QueryEmployeePayrunJobsCountAsync(int tenantId, int employeeId, Query query = null)
    {
        // query
        var dbQuery = DbQueryFactory.NewQuery<PayrunJob>(Context, TableName, query, QueryMode.ItemCount);

        // join payrun job to job employee
        dbQuery
            .Join(DbSchema.Tables.PayrunJobEmployee,
                GetIdColumnName(),
                GetColumnName(DbSchema.Tables.PayrunJobEmployee, DbSchema.PayrunJobEmployeeColumn.PayrunJobId))
            .Where(DbSchema.PayrunJobEmployeeColumn.EmployeeId, employeeId);

        // query compilation
        var compileQuery = CompileQuery(dbQuery);

        // SELECT execution
        var count = await QuerySingleAsync<long>(compileQuery);
        return count;
    }

    public virtual async Task<PayrunJob> PatchPayrunJobStatusAsync(int tenantId, int payrunJobId, PayrunJobStatus jobStatus)
    {
        var payrunJob = await GetAsync(tenantId, payrunJobId);
        if (payrunJob == null)
        {
            return null;
        }

        // already the requested status
        if (payrunJob.JobStatus == jobStatus)
        {
            return payrunJob;
        }

        // update status
        payrunJob.JobStatus = jobStatus;
        return await UpdateAsync(tenantId, payrunJob);
    }

    protected override async Task OnRetrieved(int tenantId, PayrunJob payrunJob)
    {
        // employees
        payrunJob.Employees = (await JobEmployeeRepository.QueryAsync(payrunJob.Id)).ToList();
    }

    protected override async Task OnCreatedAsync(int caseId, PayrunJob payrunJob)
    {
        // employees
        if (payrunJob.Employees != null)
        {
            await JobEmployeeRepository.CreateAsync(payrunJob.Id, payrunJob.Employees);
        }
        await base.OnCreatedAsync(caseId, payrunJob);
    }

    protected override async Task OnUpdatedAsync(int tenantId, PayrunJob payrunJob)
    {
        // employees
        if (payrunJob.Employees != null)
        {
            foreach (var jobEmployee in payrunJob.Employees)
            {
                await JobEmployeeRepository.UpdateAsync(payrunJob.Id, jobEmployee);
            }
        }
        await base.OnUpdatedAsync(tenantId, payrunJob);
    }

    public override async Task<bool> DeleteAsync(int tenantId, int payrunJobId)
    {
        // delete all related objects
        var parameters = new DbParameterCollection();
        parameters.Add(DbSchema.ParameterDeletePayrunJob.TenantId, tenantId);
        parameters.Add(DbSchema.ParameterDeletePayrunJob.PayrunJobId, payrunJobId);

        // retrieve derived case fields (stored procedure)
        await QueryAsync(DbSchema.Procedures.DeletePayrunJob,
                         parameters, commandType: CommandType.StoredProcedure);
        return true;
    }
}