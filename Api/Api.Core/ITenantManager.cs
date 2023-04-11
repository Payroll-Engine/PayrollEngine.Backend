
namespace PayrollEngine.Api.Core;

public interface ITenantManager
{
    bool IsValid(int tenantId);
}