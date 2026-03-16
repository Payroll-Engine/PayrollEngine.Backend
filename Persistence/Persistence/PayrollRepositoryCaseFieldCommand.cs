using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Persistence.DbSchema;

namespace PayrollEngine.Persistence;

internal sealed class PayrollRepositoryCaseFieldCommand : PayrollRepositoryCaseFieldCommandBase
{
    internal PayrollRepositoryCaseFieldCommand(IDbContext dbContext) :
        base(dbContext)
    {
    }

    internal async Task<IEnumerable<ChildCaseField>> GetDerivedCaseFieldsAsync(PayrollQuery query,
        IEnumerable<string> caseFieldNames = null, OverrideType? overrideType = null, ClusterSet clusterSet = null)
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
        var names = caseFieldNames?.Distinct().ToList();
        if (names != null)
        {
            foreach (var name in names)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentException(nameof(caseFieldNames));
                }
            }
        }

        // query setup
        query.RegulationDate ??= Date.Now;
        query.EvaluationDate ??= Date.Now;

        // parameters
        var parameters = new DbParameterCollection();
        parameters.Add(ParameterGetDerivedCaseFields.TenantId, query.TenantId, DbType.Int32);
        parameters.Add(ParameterGetDerivedCaseFields.PayrollId, query.PayrollId, DbType.Int32);
        parameters.Add(ParameterGetDerivedCaseFields.RegulationDate, query.RegulationDate, DbType.DateTime2);
        parameters.Add(ParameterGetDerivedCaseFields.CreatedBefore, query.EvaluationDate, DbType.DateTime2);
        parameters.Add(ParameterGetDerivedCaseFields.IncludeClusters,
            clusterSet?.IncludeClusters?.Any() == true
                ? JsonSerializer.Serialize(clusterSet.IncludeClusters) : null);
        parameters.Add(ParameterGetDerivedCaseFields.ExcludeClusters,
            clusterSet?.ExcludeClusters?.Any() == true
                ? JsonSerializer.Serialize(clusterSet.ExcludeClusters) : null);
        parameters.Add(ParameterGetDerivedCaseFields.CaseFieldNames,
            names?.Any() == true ? JsonSerializer.Serialize(names) : null);

        // retrieve derived case fields (stored procedure)
        var caseFields = (await DbContext.QueryAsync<DerivedCaseField>(Procedures.GetDerivedCaseFields,
            parameters, commandType: CommandType.StoredProcedure)).ToList();

        BuildDerivedCaseFields(caseFields, overrideType);
        return caseFields;
    }
}