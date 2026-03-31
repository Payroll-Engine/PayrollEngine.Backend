using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Domain.Application;

public class PayrunJobService(PayrunJobServiceSettings settings) :
    ChildApplicationService<IPayrunJobRepository, PayrunJob>(settings.PayrunJobRepository), IPayrunJobService
{
    public PayrunJobServiceSettings Settings { get; } = settings;

    /// <inheritdoc />
    public async Task<IEnumerable<PayrunJob>> QueryEmployeePayrunJobsAsync(IDbContext context, int tenantId, int employeeId, Query query = null) =>
        await Settings.PayrunJobRepository.QueryEmployeePayrunJobsAsync(context, tenantId, employeeId, query);

    /// <inheritdoc />
    public async Task<long> QueryEmployeePayrunJobsCountAsync(IDbContext context, int tenantId, int employeeId, Query query = null) =>
        await Settings.PayrunJobRepository.QueryEmployeePayrunJobsCountAsync(context, tenantId, employeeId, query);

    /// <inheritdoc />
    public async Task<int> ImportPayrunJobSetsAsync(IDbContext context, int tenantId, IEnumerable<PayrunJobSet> jobSets)
    {
        var jobSetList = jobSets.ToList();
        if (jobSetList.Count == 0)
        {
            return 0;
        }

        // phase 1a: resolve all FK ids from denormalized names.
        // collect all missing references across the entire batch before aborting.
        var errors = new List<string>();
        foreach (var jobSet in jobSetList)
        {
            var key = jobSet.Name ?? "(unnamed)";

            // guard: required denormalized fields
            if (string.IsNullOrWhiteSpace(jobSet.Name) ||
                string.IsNullOrWhiteSpace(jobSet.PayrunName) ||
                string.IsNullOrWhiteSpace(jobSet.DivisionName) ||
                string.IsNullOrWhiteSpace(jobSet.UserIdentifier))
            {
                errors.Add($"{key}: missing required field(s) (Name/PayrunName/DivisionName/UserIdentifier)");
                continue;
            }

            // guard: only completed or forecast jobs may be imported
            if (jobSet.JobStatus != PayrunJobStatus.Complete &&
                jobSet.JobStatus != PayrunJobStatus.Forecast)
            {
                errors.Add($"{key}: invalid job status '{jobSet.JobStatus}' — only Complete and Forecast jobs may be imported");
                continue;
            }

            // resolve Payrun
            var payruns = (await Settings.PayrunRepository.QueryAsync(context, tenantId,
                QueryFactory.NewNameQuery(jobSet.PayrunName))).ToList();
            if (payruns.Count != 1)
            {
                errors.Add($"{key}: payrun '{jobSet.PayrunName}' not found");
            }
            else
            {
                jobSet.PayrunId = payruns[0].Id;
                jobSet.PayrollId = payruns[0].PayrollId;
            }

            // resolve Division
            var divisions = (await Settings.DivisionRepository.QueryAsync(context, tenantId,
                QueryFactory.NewNameQuery(jobSet.DivisionName))).ToList();
            if (divisions.Count != 1)
            {
                errors.Add($"{key}: division '{jobSet.DivisionName}' not found");
            }
            else
            {
                jobSet.DivisionId = divisions[0].Id;
            }

            // resolve User
            var users = (await Settings.UserRepository.QueryAsync(context, tenantId,
                QueryFactory.NewIdentifierQuery(jobSet.UserIdentifier))).ToList();
            if (users.Count != 1)
            {
                errors.Add($"{key}: user '{jobSet.UserIdentifier}' not found");
            }
            else
            {
                jobSet.CreatedUserId = users[0].Id;
            }

            // resolve Employee per ResultSet
            if (jobSet.ResultSets != null)
            {
                foreach (var resultSet in jobSet.ResultSets)
                {
                    if (string.IsNullOrWhiteSpace(resultSet.EmployeeIdentifier))
                    {
                        errors.Add($"{key}: result set missing EmployeeIdentifier");
                        continue;
                    }
                    var employees = (await Settings.EmployeeRepository.QueryAsync(context, tenantId,
                        QueryFactory.NewIdentifierQuery(resultSet.EmployeeIdentifier))).ToList();
                    if (employees.Count != 1)
                    {
                        errors.Add($"{key}: employee '{resultSet.EmployeeIdentifier}' not found");
                    }
                    else
                    {
                        resultSet.EmployeeId = employees[0].Id;
                    }
                }
            }
        }
        if (errors.Count > 0)
        {
            throw new PayrollException(
                $"Import aborted: {errors.Count} reference(s) not found: "
                + string.Join(", ", errors));
        }

        // phase 1b: duplicate check — only reached when all references are resolved.
        var duplicates = new List<string>();
        foreach (var jobSet in jobSetList)
        {
            var existing = (await Settings.PayrunJobRepository.QueryAsync(context, tenantId,
                QueryFactory.NewNameQuery(jobSet.Name))).ToList();
            if (existing.Count > 0)
            {
                duplicates.Add(jobSet.Name);
            }
        }
        if (duplicates.Count > 0)
        {
            throw new PayrollException(
                $"Import aborted: {duplicates.Count} duplicate payrun job(s) already exist: "
                + string.Join(", ", duplicates));
        }

        // phase 2: all checks passed — import each job set in a single transaction.
        // if any insert fails, the entire import is rolled back.
        using var transaction = new System.Transactions.TransactionScope(
            System.Transactions.TransactionScopeOption.Required,
            new System.Transactions.TransactionOptions
            {
                IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted
            },
            System.Transactions.TransactionScopeAsyncFlowOption.Enabled);
        var count = 0;
        foreach (var jobSet in jobSetList)
        {
            // reset PK — target DB assigns new id
            jobSet.Id = 0;
            jobSet.ParentJobId = null; // retro parent jobs are not imported
            await Settings.PayrunJobRepository.CreateAsync(context, tenantId, jobSet);
            var newJobId = jobSet.Id;

            // create result sets linked to the new job id
            if (jobSet.ResultSets != null)
            {
                foreach (var resultSet in jobSet.ResultSets)
                {
                    // reset PKs and update all denormalized FK ids to target-DB values
                    resultSet.Id = 0;
                    resultSet.PayrunJobId = newJobId;
                    resultSet.PayrunJobName = jobSet.Name;
                    resultSet.PayrunId = jobSet.PayrunId;
                    resultSet.PayrunName = jobSet.PayrunName;
                    resultSet.PayrollId = jobSet.PayrollId;
                    resultSet.DivisionId = jobSet.DivisionId;

                    if (resultSet.WageTypeResults != null)
                    {
                        foreach (var wt in resultSet.WageTypeResults)
                        {
                            wt.Id = 0;
                            wt.TenantId = tenantId;
                            wt.EmployeeId = resultSet.EmployeeId;
                            wt.DivisionId = resultSet.DivisionId;
                            wt.PayrunJobId = newJobId;
                            wt.ParentJobId = null;
                            if (wt.CustomResults != null)
                            {
                                foreach (var cr in wt.CustomResults)
                                {
                                    cr.Id = 0;
                                    cr.TenantId = tenantId;
                                    cr.EmployeeId = resultSet.EmployeeId;
                                    cr.DivisionId = resultSet.DivisionId;
                                    cr.PayrunJobId = newJobId;
                                    cr.ParentJobId = null;
                                }
                            }
                        }
                    }
                    if (resultSet.CollectorResults != null)
                    {
                        foreach (var col in resultSet.CollectorResults)
                        {
                            col.Id = 0;
                            col.TenantId = tenantId;
                            col.EmployeeId = resultSet.EmployeeId;
                            col.DivisionId = resultSet.DivisionId;
                            col.PayrunJobId = newJobId;
                            col.ParentJobId = null;
                            if (col.CustomResults != null)
                            {
                                foreach (var cr in col.CustomResults)
                                {
                                    cr.Id = 0;
                                    cr.TenantId = tenantId;
                                    cr.EmployeeId = resultSet.EmployeeId;
                                    cr.DivisionId = resultSet.DivisionId;
                                    cr.PayrunJobId = newJobId;
                                    cr.ParentJobId = null;
                                }
                            }
                        }
                    }
                    if (resultSet.PayrunResults != null)
                    {
                        foreach (var pr in resultSet.PayrunResults)
                        {
                            pr.Id = 0;
                            pr.TenantId = tenantId;
                            pr.EmployeeId = resultSet.EmployeeId;
                            pr.DivisionId = resultSet.DivisionId;
                            pr.PayrunJobId = newJobId;
                            pr.ParentJobId = null;
                        }
                    }

                    await Settings.PayrollResultSetRepository.CreateAsync(context, tenantId, resultSet);
                }
            }
            count++;
        }
        transaction.Complete();
        return count;
    }
}