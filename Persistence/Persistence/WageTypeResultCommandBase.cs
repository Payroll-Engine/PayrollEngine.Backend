using System;
using System.Data;
using System.Linq;
using System.Text.Json;
using PayrollEngine.Domain.Model;
using PayrollEngine.Persistence.DbSchema;

namespace PayrollEngine.Persistence;

internal abstract class WageTypeResultCommandBase(IDbContext context) : ResultCommandBase(context)
{
    /// <summary>
    /// Get query parameters for wage type results
    /// </summary>
    /// <param name="query">The query</param>
    /// <param name="payrunJobId">The payrun job id</param>
    /// <param name="parentPayrunJobId">The parent payrun job id</param>
    /// <returns>The wage type results</returns>
    protected DbParameterCollection GetQueryParameters(WageTypeResultQuery query,
        int? payrunJobId = null, int? parentPayrunJobId = null)
    {
        // query check
        if (query == null)
        {
            throw new ArgumentNullException(nameof(query));
        }
        if (query.TenantId <= 0)
        {
            throw new ArgumentException(nameof(query.TenantId));
        }
        if (query.EmployeeId <= 0)
        {
            throw new ArgumentException(nameof(query.EmployeeId));
        }
        if (query.DivisionId <= 0)
        {
            throw new ArgumentException(nameof(query.DivisionId));
        }
        if (parentPayrunJobId is <= 0)
        {
            throw new ArgumentException(nameof(payrunJobId));
        }
        if (parentPayrunJobId is <= 0)
        {
            throw new ArgumentException(nameof(parentPayrunJobId));
        }
        // wage type numbers
        var numbers = query.WageTypeNumbers?.Distinct().ToList();
        if (numbers != null)
        {
            foreach (var number in numbers)
            {
                if (number <= 0)
                {
                    throw new ArgumentException(nameof(query.WageTypeNumbers));
                }
            }
        }
        if (query.Period != null && !query.Period.IsUtc)
        {
            throw new ArgumentException(nameof(query.Period));
        }

        // parameters
        var parameters = new DbParameterCollection();
        parameters.Add(ParameterGetWageTypeResults.TenantId, query.TenantId, DbType.Int32);
        parameters.Add(ParameterGetWageTypeResults.EmployeeId, query.EmployeeId, DbType.Int32);

        parameters.Add(ParameterGetWageTypeResults.DivisionId,
            query.DivisionId, DbType.Int32);
        parameters.Add(ParameterGetWageTypeResults.PayrunJobId,
            payrunJobId, DbType.Int32);
        parameters.Add(ParameterGetWageTypeResults.ParentPayrunJobId,
            parentPayrunJobId, DbType.Int32);
        parameters.Add(ParameterGetWageTypeResults.WageTypeNumbers,
            numbers?.Any() == true ? JsonSerializer.Serialize(numbers) : null);
        parameters.Add(ParameterGetWageTypeResults.PeriodStart,
            query.Period != null ? query.Period.Start : null, DbType.DateTime2);
        parameters.Add(ParameterGetWageTypeResults.PeriodEnd,
            query.Period != null ? query.Period.End : null, DbType.DateTime2);
        parameters.Add(ParameterGetWageTypeResults.Forecast,
            string.IsNullOrWhiteSpace(query.Forecast) ? null : query.Forecast);
        parameters.Add(ParameterGetWageTypeResults.JobStatus,
            query.JobStatus != null ? (object)query.JobStatus : null, DbType.Int32);
        parameters.Add(ParameterGetWageTypeResults.EvaluationDate,
            query.EvaluationDate, DbType.DateTime2);

        return parameters;
    }

    /// <summary>
    /// Get query parameters for consolidated wage type results
    /// </summary>
    /// <param name="query">The query</param>
    /// <returns>The wage type results</returns>
    protected DbParameterCollection GetQueryParameters(ConsolidatedWageTypeResultQuery query)
    {
        // query check
        if (query == null)
        {
            throw new ArgumentNullException(nameof(query));
        }

        if (query.TenantId <= 0)
        {
            throw new ArgumentException(nameof(query.TenantId));
        }

        if (query.EmployeeId <= 0)
        {
            throw new ArgumentException(nameof(query.EmployeeId));
        }

        if (query.DivisionId <= 0)
        {
            throw new ArgumentException(nameof(query.DivisionId));
        }

        // wage type numbers
        var numbers = query.WageTypeNumbers?.Distinct().ToList();
        if (numbers != null)
        {
            foreach (var number in numbers)
            {
                if (number <= 0)
                {
                    throw new ArgumentException(nameof(query.WageTypeNumbers));
                }
            }
        }

        if (query.Period != null && !query.Period.IsUtc)
        {
            throw new ArgumentException(nameof(query.Period));
        }

        // parameters
        var parameters = new DbParameterCollection();
        parameters.Add(ParameterGetWageTypeResults.TenantId, query.TenantId, DbType.Int32);
        parameters.Add(ParameterGetWageTypeResults.EmployeeId, query.EmployeeId, DbType.Int32);

        parameters.Add(ParameterGetWageTypeResults.DivisionId,
            query.DivisionId, DbType.Int32);
        parameters.Add(ParameterGetWageTypeResults.WageTypeNumbers,
            numbers?.Any() == true ? JsonSerializer.Serialize(numbers) : null);
        parameters.Add(ParameterGetWageTypeResults.PeriodStartHashes,
            query.PeriodStarts?.Any() == true
                ? JsonSerializer.Serialize(query.PeriodStarts.Select(x => x.GetPastDaysCount())) : null);
        parameters.Add(ParameterGetWageTypeResults.Forecast,
            string.IsNullOrWhiteSpace(query.Forecast) ? null : query.Forecast);
        parameters.Add(ParameterGetWageTypeResults.JobStatus,
            query.JobStatus != null ? (object)query.JobStatus : null, DbType.Int32);
        parameters.Add(ParameterGetWageTypeResults.EvaluationDate,
            query.EvaluationDate, DbType.DateTime2);
        parameters.Add(ParameterGetWageTypeResults.NoRetro, query.NoRetro, DbType.Boolean);
        parameters.Add(ParameterGetWageTypeResults.ExcludeParentJobId,
            query.ExcludeParentJobId, DbType.Int32);

        return parameters;
    }
}