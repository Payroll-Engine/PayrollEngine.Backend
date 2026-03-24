using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Persistence.DbSchema;
using PayrollEngine.Serialization;

namespace PayrollEngine.Persistence;

public class RegulationShareRepository(IRegulationRepository regulationRepository) :
    RootDomainRepository<RegulationShare>(Tables.RegulationShare), IRegulationShareRepository
{
    private IRegulationRepository RegulationRepository { get; } = regulationRepository ?? throw new ArgumentNullException(nameof(regulationRepository));

    protected override void GetObjectData(RegulationShare share, DbParameterCollection parameters)
    {
        parameters.Add(nameof(share.ProviderTenantId), share.ProviderTenantId, DbType.Int32);
        parameters.Add(nameof(share.ProviderRegulationId), share.ProviderRegulationId, DbType.Int32);
        parameters.Add(nameof(share.ConsumerTenantId), share.ConsumerTenantId, DbType.Int32);
        parameters.Add(nameof(share.ConsumerDivisionId), share.ConsumerDivisionId, DbType.Int32);
        parameters.Add(nameof(share.IsolationLevel), (int)share.IsolationLevel, DbType.Int32);
        parameters.Add(nameof(share.Attributes), JsonSerializer.SerializeNamedDictionary(share.Attributes));
        base.GetObjectData(share, parameters);
    }

    public override async Task<RegulationShare> CreateAsync(IDbContext context, RegulationShare share)
    {
        var regulation = await RegulationRepository.GetAsync(context, share.ProviderTenantId, share.ProviderRegulationId);
        if (regulation == null)
        {
            throw new PayrollException($"Unknown regulation {share.ProviderRegulationId} in tenant {share.ProviderTenantId}.");
        }
        if (!regulation.SharedRegulation)
        {
            throw new PayrollException($"Regulation {share.ProviderRegulationId} in tenant {share.ProviderTenantId} is not shared.");
        }
        return await base.CreateAsync(context, share);
    }

    public async Task<RegulationShare> GetAsync(IDbContext context, int providerTenantId, int providerRegulationId,
        int consumerTenantId, int? consumerDivisionId)
    {
        var dbQuery = DbQueryFactory.NewQuery<RegulationShare>(context, TableName)
            .Where(nameof(RegulationShare.ProviderTenantId), providerTenantId)
            .Where(nameof(RegulationShare.ProviderRegulationId), providerRegulationId)
            .Where(nameof(RegulationShare.ConsumerTenantId), consumerTenantId);

        if (consumerDivisionId.HasValue)
        {
            dbQuery.WhereNullOrValue(nameof(RegulationShare.ConsumerDivisionId), consumerDivisionId);
        }

        var compileQuery = CompileQuery(dbQuery, context);
        var shares = (await QueryAsync<RegulationShare>(context, compileQuery)).FirstOrDefault();
        return shares;
    }

    /// <summary>Query regulation shares for a consumer tenant, filtered by minimum permission level</summary>
    public async Task<System.Collections.Generic.IEnumerable<RegulationShare>> GetConsumerSharesAsync(
        IDbContext context, int consumerTenantId, TenantIsolationLevel minLevel)
    {
        var dbQuery = DbQueryFactory.NewQuery<RegulationShare>(context, TableName)
            .Where(nameof(RegulationShare.ConsumerTenantId), consumerTenantId)
            .WhereRaw($"{nameof(RegulationShare.IsolationLevel)} >= {(int)minLevel}");

        var compileQuery = CompileQuery(dbQuery, context);
        return await QueryAsync<RegulationShare>(context, compileQuery);
    }

    /// <summary>Query regulation shares for a consumer tenant and division, filtered by minimum isolation level</summary>
    public async Task<System.Collections.Generic.IEnumerable<RegulationShare>> GetConsumerDivisionSharesAsync(
        IDbContext context, int consumerTenantId, int divisionId, TenantIsolationLevel minLevel)
    {
        var dbQuery = DbQueryFactory.NewQuery<RegulationShare>(context, TableName)
            .Where(nameof(RegulationShare.ConsumerTenantId), consumerTenantId)
            .Where(nameof(RegulationShare.ConsumerDivisionId), divisionId)
            .WhereRaw($"{nameof(RegulationShare.IsolationLevel)} >= {(int)minLevel}");

        var compileQuery = CompileQuery(dbQuery, context);
        return await QueryAsync<RegulationShare>(context, compileQuery);
    }
}
