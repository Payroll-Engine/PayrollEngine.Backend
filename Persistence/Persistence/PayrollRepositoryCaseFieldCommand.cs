using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using PayrollEngine.Domain.Model;

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
        parameters.Add(DbSchema.ParameterGetDerivedCaseFields.TenantId, query.TenantId, DbType.Int32);
        parameters.Add(DbSchema.ParameterGetDerivedCaseFields.PayrollId, query.PayrollId, DbType.Int32);
        parameters.Add(DbSchema.ParameterGetDerivedCaseFields.RegulationDate, query.RegulationDate, DbType.DateTime2);
        parameters.Add(DbSchema.ParameterGetDerivedCaseFields.CreatedBefore, query.EvaluationDate, DbType.DateTime2);
        if (clusterSet != null)
        {
            if (clusterSet.IncludeClusters != null && clusterSet.IncludeClusters.Any())
            {
                parameters.Add(DbSchema.ParameterGetDerivedCaseFields.IncludeClusters,
                    System.Text.Json.JsonSerializer.Serialize(clusterSet.IncludeClusters));
            }
            if (clusterSet.ExcludeClusters != null && clusterSet.ExcludeClusters.Any())
            {
                parameters.Add(DbSchema.ParameterGetDerivedCaseFields.ExcludeClusters,
                    System.Text.Json.JsonSerializer.Serialize(clusterSet.ExcludeClusters));
            }
        }
        if (names != null && names.Any())
        {
            parameters.Add(DbSchema.ParameterGetDerivedCaseFields.CaseFieldNames,
                System.Text.Json.JsonSerializer.Serialize(names));
        }

        // retrieve derived case fields (stored procedure)
        var caseFields = (await DbContext.QueryAsync<DerivedCaseField>(DbSchema.Procedures.GetDerivedCaseFields,
            parameters, commandType: CommandType.StoredProcedure)).ToList();

        BuildDerivedCaseFields(caseFields, overrideType);
        return caseFields;
    }
}