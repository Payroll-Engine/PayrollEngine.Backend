﻿using System;
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

public class PayrunJobRepository(IPayrunJobEmployeeRepository jobEmployeeRepository) : ChildDomainRepository<PayrunJob>(
    DbSchema.Tables.PayrunJob, DbSchema.PayrunJobColumn.TenantId), IPayrunJobRepository
{
    private IPayrunJobEmployeeRepository JobEmployeeRepository { get; } = jobEmployeeRepository ?? throw new ArgumentNullException(nameof(jobEmployeeRepository));

    protected override void GetObjectCreateData(PayrunJob payrunJob, DbParameterCollection parameters)
    {
        // local fields
        // keep in sync with object properties
        parameters.Add(nameof(payrunJob.Name), payrunJob.Name);
        parameters.Add(nameof(payrunJob.Owner), payrunJob.Owner);
        parameters.Add(nameof(payrunJob.PayrunId), payrunJob.PayrunId);
        parameters.Add(nameof(payrunJob.PayrollId), payrunJob.PayrollId);
        parameters.Add(nameof(payrunJob.DivisionId), payrunJob.DivisionId);
        parameters.Add(nameof(payrunJob.CreatedUserId), payrunJob.CreatedUserId);
        parameters.Add(nameof(payrunJob.ParentJobId), payrunJob.ParentJobId);
        parameters.Add(nameof(payrunJob.Tags), JsonSerializer.SerializeList(payrunJob.Tags));
        parameters.Add(nameof(payrunJob.Forecast), payrunJob.Forecast);
        parameters.Add(nameof(payrunJob.RetroPayMode), payrunJob.RetroPayMode);
        parameters.Add(nameof(payrunJob.JobResult), payrunJob.JobResult);
        parameters.Add(nameof(payrunJob.CycleName), payrunJob.CycleName);
        parameters.Add(nameof(payrunJob.CycleStart), payrunJob.CycleStart);
        parameters.Add(nameof(payrunJob.CycleEnd), payrunJob.CycleEnd);
        parameters.Add(nameof(payrunJob.PeriodName), payrunJob.PeriodName);
        parameters.Add(nameof(payrunJob.PeriodStart), payrunJob.PeriodStart);
        parameters.Add(nameof(payrunJob.PeriodEnd), payrunJob.PeriodEnd);
        parameters.Add(nameof(payrunJob.EvaluationDate), payrunJob.EvaluationDate);
        parameters.Add(nameof(payrunJob.CreatedReason), payrunJob.CreatedReason);

        // base fields
        base.GetObjectCreateData(payrunJob, parameters);
    }

    protected override void GetObjectData(PayrunJob payrunJob, DbParameterCollection parameters)
    {
        parameters.Add(nameof(payrunJob.ReleasedUserId), payrunJob.ReleasedUserId);
        parameters.Add(nameof(payrunJob.ProcessedUserId), payrunJob.ProcessedUserId);
        parameters.Add(nameof(payrunJob.FinishedUserId), payrunJob.FinishedUserId);
        parameters.Add(nameof(payrunJob.Released), payrunJob.Released);
        parameters.Add(nameof(payrunJob.Processed), payrunJob.Processed);
        parameters.Add(nameof(payrunJob.Finished), payrunJob.Finished);
        parameters.Add(nameof(payrunJob.ReleasedReason), payrunJob.ReleasedReason);
        parameters.Add(nameof(payrunJob.ProcessedReason), payrunJob.ProcessedReason);
        parameters.Add(nameof(payrunJob.FinishedReason), payrunJob.FinishedReason);
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

    public async Task<IEnumerable<PayrunJob>> QueryEmployeePayrunJobsAsync(IDbContext context,
        int tenantId, int employeeId, Query query = null)
    {
        // query
        var dbQuery = DbQueryFactory.NewQuery<PayrunJob>(context, TableName, query);

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
        var items = (await QueryAsync<PayrunJob>(context, compileQuery)).ToList();

        // notification
        await OnRetrieved(context, tenantId, items);

        return items;
    }

    public async Task<long> QueryEmployeePayrunJobsCountAsync(IDbContext context,
        int tenantId, int employeeId, Query query = null)
    {
        // query
        var dbQuery = DbQueryFactory.NewQuery<PayrunJob>(context, TableName, query, QueryMode.ItemCount);

        // join payrun job to job employee
        dbQuery
            .Join(DbSchema.Tables.PayrunJobEmployee,
                GetIdColumnName(),
                GetColumnName(DbSchema.Tables.PayrunJobEmployee, DbSchema.PayrunJobEmployeeColumn.PayrunJobId))
            .Where(DbSchema.PayrunJobEmployeeColumn.EmployeeId, employeeId);

        // query compilation
        var compileQuery = CompileQuery(dbQuery);

        // SELECT execution
        var count = await QuerySingleAsync<long>(context, compileQuery);
        return count;
    }

    public async Task<PayrunJob> PatchPayrunJobStatusAsync(IDbContext context,
        int tenantId, int payrunJobId, PayrunJobStatus jobStatus, int userId, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException(nameof(reason));
        }

        var payrunJob = await GetAsync(context, tenantId, payrunJobId);
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
        switch (jobStatus)
        {
            case PayrunJobStatus.Forecast:
                // nothing to do
                break;
            case PayrunJobStatus.Release:
                payrunJob.Released = Date.Now;
                payrunJob.ReleasedUserId = userId;
                payrunJob.ReleasedReason = reason;
                break;
            case PayrunJobStatus.Process:
                payrunJob.Processed = Date.Now;
                payrunJob.ProcessedUserId = userId;
                payrunJob.ProcessedReason = reason;
                break;
            case PayrunJobStatus.Complete:
            case PayrunJobStatus.Abort:
            case PayrunJobStatus.Cancel:
                payrunJob.Finished = Date.Now;
                payrunJob.FinishedUserId = userId;
                payrunJob.FinishedReason = reason;
                break;
            default:
                throw new InvalidOperationException($"Unsupported payrun job status change to {jobStatus}");
        }

        return await UpdateAsync(context, tenantId, payrunJob);
    }

    protected override async Task OnRetrieved(IDbContext context, int tenantId, PayrunJob payrunJob)
    {
        // employees
        payrunJob.Employees = (await JobEmployeeRepository.QueryAsync(context, payrunJob.Id)).ToList();
    }

    protected override async Task OnCreatedAsync(IDbContext context, int caseId, PayrunJob payrunJob)
    {
        // employees
        if (payrunJob.Employees != null)
        {
            await JobEmployeeRepository.CreateAsync(context, payrunJob.Id, payrunJob.Employees);
        }
        await base.OnCreatedAsync(context, caseId, payrunJob);
    }

    protected override async Task OnUpdatedAsync(IDbContext context, int tenantId, PayrunJob payrunJob)
    {
        // employees
        if (payrunJob.Employees != null)
        {
            foreach (var jobEmployee in payrunJob.Employees)
            {
                await JobEmployeeRepository.UpdateAsync(context, payrunJob.Id, jobEmployee);
            }
        }
        await base.OnUpdatedAsync(context, tenantId, payrunJob);
    }

    /// <inheritdoc />
    /// <remarks>Do not call the base class method</remarks>
    public override async Task<bool> DeleteAsync(IDbContext context, int tenantId, int payrunJobId)
    {
        // delete all related objects
        var parameters = new DbParameterCollection();
        parameters.Add(DbSchema.ParameterDeletePayrunJob.TenantId, tenantId);
        parameters.Add(DbSchema.ParameterDeletePayrunJob.PayrunJobId, payrunJobId);
        parameters.Add("@sp_return", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

        // delete the payrun job (stored procedure)
        await QueryAsync(context, DbSchema.Procedures.DeletePayrunJob,
                         parameters, commandType: CommandType.StoredProcedure);

        // stored procedure return value
        var result = parameters.Get<int>("@sp_return");
        return result == 1;
    }
}