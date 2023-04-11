
namespace PayrollEngine.Api.Core;

public class TenantManager : ITenantManager
{
    public bool IsValid(int tenantId)
    {
        // TODO: implement tenant check
        return tenantId > 0;
    }
}