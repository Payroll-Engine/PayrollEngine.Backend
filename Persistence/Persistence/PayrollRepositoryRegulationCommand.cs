using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;

namespace PayrollEngine.Persistence;

internal sealed class PayrollRepositoryRegulationCommand : PayrollRepositoryCommandBase
{
    internal PayrollRepositoryRegulationCommand(IDbContext dbContext) :
        base(dbContext)
    {
    }

    internal async Task<IEnumerable<Regulation>> GetDerivedRegulationsAsync(
        IRegulationRepository regulationRepository, IPayrollLayerRepository payrollLayerRepository, PayrollQuery query)
    {
        if (payrollLayerRepository == null)
        {
            throw new ArgumentNullException(nameof(payrollLayerRepository));
        }
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

        // retrieve payroll layers
        var payrollLayers = (await payrollLayerRepository.QueryAsync(DbContext, query.PayrollId, QueryFactory.Active)).ToList();
        if (!payrollLayers.Any())
        {
            // nothing to do
            return new List<Regulation>();
        }

        query.RegulationDate ??= Date.Now;
        query.EvaluationDate ??= Date.Now;
        // retrieve all derived regulations (stored procedure)
        var parameters = new DbParameterCollection();
        parameters.Add(DbSchema.ParameterGetDerivedPayrollRegulations.TenantId, query.TenantId, DbType.Int32);
        parameters.Add(DbSchema.ParameterGetDerivedPayrollRegulations.PayrollId, query.PayrollId, DbType.Int32);
        parameters.Add(DbSchema.ParameterGetDerivedPayrollRegulations.RegulationDate, query.RegulationDate, DbType.DateTime2);
        parameters.Add(DbSchema.ParameterGetDerivedPayrollRegulations.CreatedBefore, query.EvaluationDate, DbType.DateTime2);
        var regulationDefinitions = (await DbContext.QueryAsync<Regulation>(DbSchema.Procedures.GetDerivedPayrollRegulations,
            parameters, commandType: CommandType.StoredProcedure)).ToList();

        // load the regulations
        var regulations = new List<Regulation>();
        foreach (var regulationDefinition in regulationDefinitions)
        {
            var regulationTenant = await regulationRepository.GetParentIdAsync(DbContext, regulationDefinition.Id);
            if (!regulationTenant.HasValue)
            {
                throw new PayrollException($"Unknown tenant of regulation with id {regulationDefinition.Id}.");
            }
            var regulation = await regulationRepository.GetAsync(DbContext, regulationTenant.Value, regulationDefinition.Id);
            if (regulationTenant.Value != query.TenantId && !regulation.SharedRegulation)
            {
                throw new PayrollException($"Invalid regulation with id {regulationDefinition.Id}.");
            }
            regulations.Add(regulation);
        }

        return regulations;
    }
}