﻿using System;
using System.Data;
using System.Linq;
using System.Text.Json;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Persistence;

internal abstract class CollectorResultCommandBase : ResultCommandBase
{
    protected CollectorResultCommandBase(IDbConnection connection) :
        base(connection)
    {
    }

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
        parameters.Add(DbSchema.ParameterGetCollectorResults.TenantId, query.TenantId);
        parameters.Add(DbSchema.ParameterGetCollectorResults.EmployeeId, query.EmployeeId);

        // division
        if (query.DivisionId.HasValue)
        {
            parameters.Add(DbSchema.ParameterGetCollectorResults.DivisionId, query.DivisionId.Value);
        }

        // payrun job
        if (payrunJobId.HasValue)
        {
            parameters.Add(DbSchema.ParameterGetCollectorResults.PayrunJobId, payrunJobId.Value);
        }
        if (parentPayrunJobId.HasValue)
        {
            parameters.Add(DbSchema.ParameterGetCollectorResults.ParentPayrunJobId, parentPayrunJobId.Value);
        }

        // collector names
        if (names != null && names.Any())
        {
            parameters.Add(DbSchema.ParameterGetCollectorResults.CollectorNameHashes,
                JsonSerializer.Serialize(names.Select(x => x.ToPayrollHash())));
        }

        // period
        if (query.Period != null)
        {
            parameters.Add(DbSchema.ParameterGetCollectorResults.PeriodStart, query.Period.Start);
            parameters.Add(DbSchema.ParameterGetCollectorResults.PeriodEnd, query.Period.End);
        }

        // forecast
        if (!string.IsNullOrWhiteSpace(query.Forecast))
        {
            parameters.Add(DbSchema.ParameterGetCollectorResults.Forecast, query.Forecast);
        }

        // job status
        if (query.JobStatus != null)
        {
            parameters.Add(DbSchema.ParameterGetCollectorResults.JobStatus, query.JobStatus);
        }

        // evaluation
        if (query.EvaluationDate.HasValue)
        {
            parameters.Add(DbSchema.ParameterGetCollectorResults.EvaluationDate, query.EvaluationDate.Value);
        }

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
        parameters.Add(DbSchema.ParameterGetCollectorResults.TenantId, query.TenantId);
        parameters.Add(DbSchema.ParameterGetCollectorResults.EmployeeId, query.EmployeeId);

        // division
        if (query.DivisionId.HasValue)
        {
            parameters.Add(DbSchema.ParameterGetCollectorResults.DivisionId, query.DivisionId.Value);
        }

        // collector names
        if (names != null && names.Any())
        {
            parameters.Add(DbSchema.ParameterGetCollectorResults.CollectorNameHashes,
                JsonSerializer.Serialize(names.Select(x => x.ToPayrollHash())));
        }

        // period
        if (query.PeriodStarts != null && query.PeriodStarts.Any())
        {
            parameters.Add(DbSchema.ParameterGetCollectorResults.PeriodStartHashes,
                JsonSerializer.Serialize(query.PeriodStarts.Select(x => x.GetPastDaysCount())));
        }

        // forecast
        if (!string.IsNullOrWhiteSpace(query.Forecast))
        {
            parameters.Add(DbSchema.ParameterGetCollectorResults.Forecast, query.Forecast);
        }

        // job status
        if (query.JobStatus != null)
        {
            parameters.Add(DbSchema.ParameterGetCollectorResults.JobStatus, query.JobStatus);
        }

        // evaluation
        if (query.EvaluationDate.HasValue)
        {
            parameters.Add(DbSchema.ParameterGetCollectorResults.EvaluationDate, query.EvaluationDate.Value);
        }

        return parameters;
    }
}