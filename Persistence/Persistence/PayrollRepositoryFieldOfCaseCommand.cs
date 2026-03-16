using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Persistence.DbSchema;

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
        parameters.Add(ParameterGetDerivedCaseFieldsOfCase.TenantId, query.TenantId, DbType.Int32);
        parameters.Add(ParameterGetDerivedCaseFieldsOfCase.PayrollId, query.PayrollId, DbType.Int32);
        parameters.Add(ParameterGetDerivedCaseFieldsOfCase.IncludeClusters,
            clusterSet?.IncludeClusters?.Any() == true
                ? JsonSerializer.Serialize(clusterSet.IncludeClusters) : null);
        parameters.Add(ParameterGetDerivedCaseFieldsOfCase.ExcludeClusters,
            clusterSet?.ExcludeClusters?.Any() == true
                ? JsonSerializer.Serialize(clusterSet.ExcludeClusters) : null);
        parameters.Add(ParameterGetDerivedCaseFieldsOfCase.CaseNames,
            names?.Any() == true ? JsonSerializer.Serialize(names) : null);
        parameters.Add(ParameterGetDerivedCaseFieldsOfCase.RegulationDate, query.RegulationDate, DbType.DateTime2);
        parameters.Add(ParameterGetDerivedCaseFieldsOfCase.CreatedBefore, query.EvaluationDate, DbType.DateTime2);

        // retrieve derived case fields (stored procedure)
        var caseFields = (await DbContext.QueryAsync<DerivedCaseField>(Procedures.GetDerivedCaseFieldsOfCase,
            parameters, commandType: CommandType.StoredProcedure)).ToList();

        BuildDerivedCaseFields(caseFields, overrideType);
        return caseFields;
    }
}