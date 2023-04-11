﻿using System;
using System.Data;
using System.Linq;
using System.Text.Json;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Persistence;

internal abstract class WageTypeResultCommandBase : ResultCommandBase
{
    protected WageTypeResultCommandBase(IDbConnection connection) :
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
        parameters.Add(DbSchema.ParameterGetWageTypeResults.TenantId, query.TenantId);
        parameters.Add(DbSchema.ParameterGetWageTypeResults.EmployeeId, query.EmployeeId);

        // division
        if (query.DivisionId.HasValue)
        {
            parameters.Add(DbSchema.ParameterGetWageTypeResults.DivisionId, query.DivisionId.Value);
        }

        // payrun job
        if (payrunJobId.HasValue)
        {
            parameters.Add(DbSchema.ParameterGetWageTypeResults.PayrunJobId, payrunJobId.Value);
        }
        if (parentPayrunJobId.HasValue)
        {
            parameters.Add(DbSchema.ParameterGetWageTypeResults.ParentPayrunJobId, parentPayrunJobId.Value);
        }

        // wage type numbers
        if (numbers != null && numbers.Any())
        {
            parameters.Add(DbSchema.ParameterGetWageTypeResults.WageTypeNumbers,
                JsonSerializer.Serialize(numbers));
        }

        // period
        if (query.Period != null)
        {
            parameters.Add(DbSchema.ParameterGetWageTypeResults.PeriodStart, query.Period.Start);
            parameters.Add(DbSchema.ParameterGetWageTypeResults.PeriodEnd, query.Period.End);
        }

        // forecast
        if (!string.IsNullOrWhiteSpace(query.Forecast))
        {
            parameters.Add(DbSchema.ParameterGetWageTypeResults.Forecast, query.Forecast);
        }

        // job status
        if (query.JobStatus != null)
        {
            parameters.Add(DbSchema.ParameterGetWageTypeResults.JobStatus, query.JobStatus);
        }

        // evaluation
        if (query.EvaluationDate.HasValue)
        {
            parameters.Add(DbSchema.ParameterGetWageTypeResults.EvaluationDate, query.EvaluationDate.Value);
        }

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
        parameters.Add(DbSchema.ParameterGetWageTypeResults.TenantId, query.TenantId);
        parameters.Add(DbSchema.ParameterGetWageTypeResults.EmployeeId, query.EmployeeId);

        // division
        if (query.DivisionId.HasValue)
        {
            parameters.Add(DbSchema.ParameterGetWageTypeResults.DivisionId, query.DivisionId.Value);
        }

        // wage type numbers
        if (numbers != null && numbers.Any())
        {
            parameters.Add(DbSchema.ParameterGetWageTypeResults.WageTypeNumbers,
                JsonSerializer.Serialize(numbers));
        }

        // period
        if (query.PeriodStarts != null && query.PeriodStarts.Any())
        {
            parameters.Add(DbSchema.ParameterGetWageTypeResults.PeriodStartHashes,
                JsonSerializer.Serialize(query.PeriodStarts.Select(x => x.GetPastDaysCount())));
        }

        // forecast
        if (!string.IsNullOrWhiteSpace(query.Forecast))
        {
            parameters.Add(DbSchema.ParameterGetWageTypeResults.Forecast, query.Forecast);
        }

        // job status
        if (query.JobStatus != null)
        {
            parameters.Add(DbSchema.ParameterGetWageTypeResults.JobStatus, query.JobStatus);
        }

        // evaluation
        if (query.EvaluationDate.HasValue)
        {
            parameters.Add(DbSchema.ParameterGetWageTypeResults.EvaluationDate, query.EvaluationDate.Value);
        }

        return parameters;
    }
}