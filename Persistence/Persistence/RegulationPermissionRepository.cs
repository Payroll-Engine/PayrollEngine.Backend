using System;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Serialization;

namespace PayrollEngine.Persistence;

public class RegulationPermissionRepository : RootDomainRepository<RegulationPermission>, IRegulationPermissionRepository
{
    public IRegulationRepository RegulationRepository { get; }

    public RegulationPermissionRepository(IRegulationRepository regulationRepository, IDbContext context) :
        base(DbSchema.Tables.RegulationPermission, context)
    {
        RegulationRepository = regulationRepository ?? throw new ArgumentNullException(nameof(regulationRepository));
    }

    protected override void GetObjectData(RegulationPermission permission, DbParameterCollection parameters)
    {
        parameters.Add(nameof(permission.TenantId), permission.TenantId);
        parameters.Add(nameof(permission.RegulationId), permission.RegulationId);
        parameters.Add(nameof(permission.PermissionTenantId), permission.PermissionTenantId);
        parameters.Add(nameof(permission.PermissionDivisionId), permission.PermissionDivisionId);
        parameters.Add(nameof(permission.Attributes), JsonSerializer.SerializeNamedDictionary(permission.Attributes));
        base.GetObjectData(permission, parameters);
    }

    public override async Task<RegulationPermission> CreateAsync(RegulationPermission permission)
    {
        var regulation = await RegulationRepository.GetAsync(permission.TenantId, permission.RegulationId);
        if (regulation == null)
        {
            throw new PayrollException($"Unknown regulation {permission.RegulationId} in tenant {permission.TenantId}");
        }
        if (!regulation.SharedRegulation)
        {
            throw new PayrollException($"Regulation {permission.RegulationId} in tenant {permission.TenantId} is not shared");
        }
        return await base.CreateAsync(permission);
    }

    public async Task<RegulationPermission> GetAsync(int tenantId, int regulationId, int permissionTenantId, int? permissionDivisionId)
    {
        // query
        var dbQuery = DbQueryFactory.NewQuery<RegulationPermission>(Context, TableName)
            .Where(nameof(RegulationPermission.TenantId), tenantId)
            .Where(nameof(RegulationPermission.RegulationId), regulationId)
            .Where(nameof(RegulationPermission.PermissionTenantId), permissionTenantId);

        if (permissionDivisionId.HasValue)
        {
            dbQuery.WhereNullOrValue(nameof(RegulationPermission.PermissionDivisionId), permissionDivisionId);
        }

        // query compilation
        var compileQuery = CompileQuery(dbQuery);

        // SELECT execution
        var permission = (await QueryAsync<RegulationPermission>(compileQuery)).FirstOrDefault();
        return permission;
    }
}