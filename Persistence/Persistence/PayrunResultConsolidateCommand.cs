using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Persistence.DbSchema;

namespace PayrollEngine.Persistence;

internal sealed class PayrunResultConsolidateCommand : ResultCommandBase
{
    internal PayrunResultConsolidateCommand(IDbContext context) :
        base(context)
    {
    }

    internal async Task<IEnumerable<PayrunResult>> GetResultsAsync(ConsolidatedPayrunResultQuery query)
    {
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
        var names = query.ResultNames?.Distinct().ToList();
        if (names != null)
        {
            foreach (var name in names)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentException(nameof(query.ResultNames));
                }
            }
        }
        if (query.Period != null && !query.Period.IsUtc)
        {
            throw new ArgumentException(nameof(query.Period));
        }

        // parameters
        var parameters = new DbParameterCollection();
        parameters.Add(ParameterGetPayrunResults.TenantId, query.TenantId, DbType.Int32);
        parameters.Add(ParameterGetPayrunResults.EmployeeId, query.EmployeeId, DbType.Int32);
        if (query.DivisionId.HasValue)
        {
            parameters.Add(ParameterGetPayrunResults.DivisionId, query.DivisionId.Value, DbType.Int32);
        }
        if (names != null && names.Any())
        {
            parameters.Add(ParameterGetPayrunResults.Names,
                JsonSerializer.Serialize(names));
        }
        if (query.PeriodStarts != null && query.PeriodStarts.Any())
        {
            parameters.Add(ParameterGetPayrunResults.PeriodStartHashes,
                JsonSerializer.Serialize(query.PeriodStarts.Select(x => x.GetPastDaysCount())));
        }
        if (!string.IsNullOrWhiteSpace(query.Forecast))
        {
            parameters.Add(ParameterGetPayrunResults.Forecast, query.Forecast);
        }
        if (query.JobStatus != null)
        {
            parameters.Add(ParameterGetPayrunResults.JobStatus, query.JobStatus, DbType.Int32);
        }
        if (query.EvaluationDate.HasValue)
        {
            parameters.Add(ParameterGetPayrunResults.EvaluationDate, query.EvaluationDate.Value, DbType.DateTime2);
        }

        // query pre action
        QueryBegin();

        // retrieve consolidated payrun results (stored procedure)
        var values = await DbContext.QueryAsync<PayrunResult>(Procedures.GetConsolidatedPayrunResults,
            parameters, commandType: CommandType.StoredProcedure);

        // query post action
        QueryEnd(() => $"{{Result query}}Result cons payrun {GetItemsString(query.ResultNames?.Distinct())}");

        // tags filter
        values = ApplyTagFilter(values, query.Tags);

        return values;
    }
}