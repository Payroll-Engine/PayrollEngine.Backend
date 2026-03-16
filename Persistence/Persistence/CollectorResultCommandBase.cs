using System;
using System.Data;
using System.Linq;
using System.Text.Json;
using PayrollEngine.Domain.Model;
using PayrollEngine.Persistence.DbSchema;

namespace PayrollEngine.Persistence;

internal abstract class CollectorResultCommandBase(IDbContext context) : ResultCommandBase(context)
{
    /// <summary>
    /// Get query parameters for wage type results
    /// </summary>
    /// <param name="query">The query</param>
    /// <param name="payrunJobId">The payrun job id</param>
    /// <param name="parentPayrunJobId">The parent payrun job id</param>
    /// <returns>The wage type results</returns>
    protected DbParameterCollection GetQueryParameters(CollectorResultQuery query,
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
        if (payrunJobId <= 0)
        {
            throw new ArgumentException(nameof(payrunJobId));
        }
        if (parentPayrunJobId is <= 0)
        {
            throw new ArgumentException(nameof(parentPayrunJobId));
        }
        // collector names
        var names = query.CollectorNames?.Distinct().ToList();
        if (names != null)
        {
            foreach (var name in names)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentException(nameof(query.CollectorNames));
                }
            }
        }
        if (query.Period != null && !query.Period.IsUtc)
        {
            throw new ArgumentException(nameof(query.Period));
        }

        // parameters
        var parameters = new DbParameterCollection();
        parameters.Add(ParameterGetCollectorResults.TenantId, query.TenantId, DbType.Int32);
        parameters.Add(ParameterGetCollectorResults.EmployeeId, query.EmployeeId, DbType.Int32);

        parameters.Add(ParameterGetCollectorResults.DivisionId,
            query.DivisionId, DbType.Int32);
        parameters.Add(ParameterGetCollectorResults.PayrunJobId,
            payrunJobId, DbType.Int32);
        parameters.Add(ParameterGetCollectorResults.ParentPayrunJobId,
            parentPayrunJobId, DbType.Int32);
        parameters.Add(ParameterGetCollectorResults.CollectorNameHashes,
            names?.Any() == true ? JsonSerializer.Serialize(names.Select(x => x.ToPayrollHash())) : null);
        parameters.Add(ParameterGetCollectorResults.PeriodStart,
            query.Period != null ? query.Period.Start : null, DbType.DateTime2);
        parameters.Add(ParameterGetCollectorResults.PeriodEnd,
            query.Period != null ? query.Period.End : null, DbType.DateTime2);
        parameters.Add(ParameterGetCollectorResults.Forecast,
            string.IsNullOrWhiteSpace(query.Forecast) ? null : query.Forecast);
        parameters.Add(ParameterGetCollectorResults.JobStatus,
            query.JobStatus != null ? (object)query.JobStatus : null, DbType.Int32);
        parameters.Add(ParameterGetCollectorResults.EvaluationDate,
            query.EvaluationDate, DbType.DateTime2);

        return parameters;
    }

    /// <summary>
    /// Get query parameters for consolidated wage type results
    /// </summary>
    /// <param name="query">The query</param>
    /// <returns>The wage type results</returns>
    protected DbParameterCollection GetQueryParameters(ConsolidatedCollectorResultQuery query)
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
        // collector names
        var names = query.CollectorNames?.Distinct().ToList();
        if (names != null)
        {
            foreach (var name in names)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentException(nameof(query.CollectorNames));
                }
            }
        }
        if (query.Period != null && !query.Period.IsUtc)
        {
            throw new ArgumentException(nameof(query.Period));
        }

        // parameters
        var parameters = new DbParameterCollection();
        parameters.Add(ParameterGetCollectorResults.TenantId, query.TenantId, DbType.Int32);
        parameters.Add(ParameterGetCollectorResults.EmployeeId, query.EmployeeId, DbType.Int32);

        parameters.Add(ParameterGetCollectorResults.DivisionId,
            query.DivisionId, DbType.Int32);
        parameters.Add(ParameterGetCollectorResults.CollectorNameHashes,
            names?.Any() == true ? JsonSerializer.Serialize(names.Select(x => x.ToPayrollHash())) : null);
        parameters.Add(ParameterGetCollectorResults.PeriodStartHashes,
            query.PeriodStarts?.Any() == true
                ? JsonSerializer.Serialize(query.PeriodStarts.Select(x => x.GetPastDaysCount())) : null);
        parameters.Add(ParameterGetCollectorResults.Forecast,
            string.IsNullOrWhiteSpace(query.Forecast) ? null : query.Forecast);
        parameters.Add(ParameterGetCollectorResults.JobStatus,
            query.JobStatus != null ? (object)query.JobStatus : null, DbType.Int32);
        parameters.Add(ParameterGetCollectorResults.EvaluationDate,
            query.EvaluationDate, DbType.DateTime2);
        parameters.Add(ParameterGetCollectorResults.NoRetro, query.NoRetro, DbType.Boolean);
        parameters.Add(ParameterGetCollectorResults.ExcludeParentJobId,
            query.ExcludeParentJobId, DbType.Int32);

        return parameters;
    }
}