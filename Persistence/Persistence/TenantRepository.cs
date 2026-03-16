using System;
using System.Data;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Persistence.DbSchema;
using PayrollEngine.Serialization;
using Task = System.Threading.Tasks.Task;

namespace PayrollEngine.Persistence;

/// <summary>Repository for <see cref="Tenant"/> persistence (table: Tenant).</summary>
public class TenantRepository() : RootDomainRepository<Tenant>(Tables.Tenant), ITenantRepository
{
    public async Task<bool> ExistsAsync(IDbContext context, string identifier) =>
        await ExistsAsync(context, TenantColumn.Identifier, identifier);

    protected override void GetObjectCreateData(Tenant tenant, DbParameterCollection parameters)
    {
        parameters.Add(nameof(tenant.Identifier), tenant.Identifier);
        base.GetObjectCreateData(tenant, parameters);
    }

    protected override void GetObjectData(Tenant tenant, DbParameterCollection parameters)
    {
        parameters.Add(nameof(tenant.Culture), tenant.Culture);
        parameters.Add(nameof(tenant.Calendar), tenant.Calendar);
        parameters.Add(nameof(tenant.Attributes), JsonSerializer.SerializeNamedDictionary(tenant.Attributes));
        base.GetObjectData(tenant, parameters);
    }

    /// <inheritdoc />
    public Task UpdateStatisticsAsync(IDbContext context) =>
        context.UpdateStatisticsAsync();

    /// <inheritdoc />
    /// <remarks>Do not call the base class method</remarks>
    public override async Task<bool> DeleteAsync(IDbContext context, int tenantId)
    {
        if (!await ExistsAsync(context, tenantId))
        {
            throw new PayrollException($"Unknown tenant with id {tenantId}.");
        }

        var parameters = new DbParameterCollection();
        parameters.Add(ParameterDeleteTenant.TenantId, tenantId, DbType.Int32);
        if (context.StoredProcedureReturnValue)
        {
            parameters.Add("@sp_return", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);
        }

        try
        {
            // delete tenant (stored procedure)
            await QueryAsync<Tenant>(context, Procedures.DeleteTenant,
                parameters, commandType: CommandType.StoredProcedure);

            // stored procedure return value
            if (context.StoredProcedureReturnValue)
            {
                var result = parameters.Get<int>("@sp_return");
                return result == 1;
            }
            return true;
        }
        catch (Exception exception)
        {
            Log.Error(exception, exception.GetBaseMessage());
            return false;
        }
    }
}