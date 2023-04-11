using PayrollEngine.Domain.Model;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;

namespace PayrollEngine.Api.Core;

public interface IControllerRuntime
{
    IConfiguration Configuration { get; }
    LinkGenerator LinkGenerator { get; }
    IApiDescriptionGroupCollectionProvider ApiExplorer { get; }
    ITenantManager Tenant { get; }
    IScriptProvider ScriptProvider { get; }
}