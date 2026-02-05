using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Persistence;

internal sealed class PayrollRepositoryFieldOfCaseCommand : PayrollRepositoryCaseFieldCommandBase
{
    internal PayrollRepositoryFieldOfCaseCommand(IDbContext dbContext) :
        base(dbContext)
    {
    }

    /// <summary>
    /// Get derived case fields of case
    /// </summary>
    /// <param name="query">The query</param>
    /// <param name="caseNames">The case names</param>
    /// <param name="overrideType">The override type</param>
    /// <param name="clusterSet">The cluster set</param>
    /// <returns>The case fields, including the parent case</returns>
    internal async Task<IEnumerable<ChildCaseField>> GetDerivedFieldsOfCaseAsync(PayrollQuery query,
        IEnumerable<string> caseNames, OverrideType? overrideType = null, ClusterSet clusterSet = null)
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
        var names = caseNames?.Distinct().ToList();
        if (names != null)
        {
            foreach (var name in names)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentException(nameof(caseNames));
                }
            }
        }

        // query setup
        query.RegulationDate ??= Date.Now;
        query.EvaluationDate ??= Date.Now;

        // parameters
        var parameters = new DbParameterCollection();
        parameters.Add(DbSchema.ParameterGetDerivedCaseFieldsOfCase.TenantId, query.TenantId, DbType.Int32);
        parameters.Add(DbSchema.ParameterGetDerivedCaseFieldsOfCase.PayrollId, query.PayrollId, DbType.Int32);
        if (clusterSet != null)
        {
            if (clusterSet.IncludeClusters != null && clusterSet.IncludeClusters.Any())
            {
                parameters.Add(DbSchema.ParameterGetDerivedCaseFieldsOfCase.IncludeClusters,
                    System.Text.Json.JsonSerializer.Serialize(clusterSet.IncludeClusters));
            }
            if (clusterSet.ExcludeClusters != null && clusterSet.ExcludeClusters.Any())
            {
                parameters.Add(DbSchema.ParameterGetDerivedCaseFieldsOfCase.ExcludeClusters,
                    System.Text.Json.JsonSerializer.Serialize(clusterSet.ExcludeClusters));
            }
        }
        if (names != null && names.Any())
        {
            parameters.Add(DbSchema.ParameterGetDerivedCaseFieldsOfCase.CaseNames,
                System.Text.Json.JsonSerializer.Serialize(names));
        }
        parameters.Add(DbSchema.ParameterGetDerivedCaseFieldsOfCase.RegulationDate, query.RegulationDate, DbType.DateTime2);
        parameters.Add(DbSchema.ParameterGetDerivedCaseFieldsOfCase.CreatedBefore, query.EvaluationDate, DbType.DateTime2);

        // retrieve derived case fields (stored procedure)
        var caseFields = (await DbContext.QueryAsync<DerivedCaseField>(DbSchema.Procedures.GetDerivedCaseFieldsOfCase,
            parameters, commandType: CommandType.StoredProcedure)).ToList();

        BuildDerivedCaseFields(caseFields, overrideType);
        return caseFields;
    }
}