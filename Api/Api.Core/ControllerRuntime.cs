using System;
using PayrollEngine.Domain.Model;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;

namespace PayrollEngine.Api.Core;

public class ControllerRuntime(IDbContext dbContext, IConfiguration configuration, LinkGenerator linkGenerator,
        IApiDescriptionGroupCollectionProvider apiExplorer, IScriptProvider scriptProvider)
    : IControllerRuntime
{
    public IDbContext DbContext { get; } = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    public IConfiguration Configuration { get; } = configuration ?? throw new ArgumentNullException(nameof(configuration));
    public LinkGenerator LinkGenerator { get; } = linkGenerator ?? throw new ArgumentNullException(nameof(linkGenerator));
    public IApiDescriptionGroupCollectionProvider ApiExplorer { get; } = apiExplorer ?? throw new ArgumentNullException(nameof(apiExplorer));
    public IScriptProvider ScriptProvider { get; } = scriptProvider ?? throw new ArgumentNullException(nameof(scriptProvider));
}