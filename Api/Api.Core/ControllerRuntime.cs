using System;
using PayrollEngine.Domain.Model;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;

namespace PayrollEngine.Api.Core;

public class ControllerRuntime : IControllerRuntime
{
    public IDbContext DbContext { get; }
    public IConfiguration Configuration { get; }
    public LinkGenerator LinkGenerator { get; }
    public IApiDescriptionGroupCollectionProvider ApiExplorer { get; }
    public ITenantManager Tenant { get; }
    public IScriptProvider ScriptProvider { get; }

    public ControllerRuntime(IDbContext dbContext, IConfiguration configuration, LinkGenerator linkGenerator,
        IApiDescriptionGroupCollectionProvider apiExplorer, ITenantManager tenant, IScriptProvider scriptProvider)
    {
        DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        LinkGenerator = linkGenerator ?? throw new ArgumentNullException(nameof(linkGenerator));
        ApiExplorer = apiExplorer ?? throw new ArgumentNullException(nameof(apiExplorer));
        Tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
        ScriptProvider = scriptProvider ?? throw new ArgumentNullException(nameof(scriptProvider));
    }
}