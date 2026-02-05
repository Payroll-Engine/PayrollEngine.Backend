using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Persistence;

internal sealed class PayrollRepositoryReportParametersCommand : PayrollRepositoryCommandBase
{
    internal PayrollRepositoryReportParametersCommand(IDbContext dbContext) :
        base(dbContext)
    {
    }

    internal async Task<IEnumerable<DerivedReportParameter>> GetDerivedReportParametersAsync(PayrollQuery query,
        IEnumerable<string> reportNames = null, OverrideType? overrideType = null)
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
        if (query.PayrollId <= 0)
        {
            throw new ArgumentException(nameof(query.PayrollId));
        }
        var names = reportNames?.Distinct().ToList();
        if (names != null)
        {
            foreach (var name in names)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentException(nameof(reportNames));
                }
            }
        }

        // query setup
        query.RegulationDate ??= Date.Now;
        query.EvaluationDate ??= Date.Now;

        // parameters
        var parameters = new DbParameterCollection();
        parameters.Add(DbSchema.ParameterGetDerivedReportParameters.TenantId, query.TenantId, DbType.Int32);
        parameters.Add(DbSchema.ParameterGetDerivedReportParameters.PayrollId, query.PayrollId, DbType.Int32);
        parameters.Add(DbSchema.ParameterGetDerivedReportParameters.RegulationDate, query.RegulationDate, DbType.DateTime2);
        parameters.Add(DbSchema.ParameterGetDerivedReportParameters.CreatedBefore, query.EvaluationDate, DbType.DateTime2);
        if (names != null && names.Any())
        {
            parameters.Add(DbSchema.ParameterGetDerivedReportParameters.ReportNames,
                System.Text.Json.JsonSerializer.Serialize(names));
        }

        // retrieve all derived report parameters (stored procedure)
        var reportParameters = (await DbContext.QueryAsync<DerivedReportParameter>(DbSchema.Procedures.GetDerivedReportParameters,
            parameters, commandType: CommandType.StoredProcedure)).ToList();

        BuildDerivedReportParameters(reportParameters, overrideType);
        return reportParameters;
    }

    private static void BuildDerivedReportParameters(List<DerivedReportParameter> reportParameters, OverrideType? overrideType = null)
    {
        if (reportParameters == null)
        {
            throw new ArgumentNullException(nameof(reportParameters));
        }
        if (!reportParameters.Any())
        {
            return;
        }

        // resulting reports
        var reportParametersByKey = reportParameters.GroupBy(x => x.Name).ToList();

        // override filter
        if (overrideType.HasValue)
        {
            ApplyOverrideFilter(reportParametersByKey, reportParameters, overrideType.Value);
            // update reports
            reportParametersByKey = reportParameters.GroupBy(x => x.Name).ToList();
        }

        // collect derived values
        foreach (var reportKey in reportParametersByKey)
        {
            // order by derived reports
            var derivedParameters = reportKey.OrderByDescending(x => x.Level).ThenByDescending(x => x.Priority).ToList();
            // derived reports
            while (derivedParameters.Count > 1)
            {
                // collect derived values
                var derivedParameter = derivedParameters.First();
                // non-derived fields: name, mandatory and all non-nullable and expressions
                derivedParameter.NameLocalizations = CollectDerivedValue(derivedParameters, x => x.NameLocalizations);
                derivedParameter.Description = CollectDerivedValue(derivedParameters, x => x.Description);
                derivedParameter.DescriptionLocalizations = CollectDerivedValue(derivedParameters, x => x.DescriptionLocalizations);
                derivedParameter.Value = CollectDerivedValue(derivedParameters, x => x.Value);
                derivedParameter.Attributes = CollectDerivedAttributes(derivedParameters);
                // remove the current level for the next iteration
                derivedParameters.Remove(derivedParameter);
            }
        }
    }
}