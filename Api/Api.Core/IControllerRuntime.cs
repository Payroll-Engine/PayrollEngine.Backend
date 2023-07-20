using PayrollEngine.Domain.Model;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;

namespace PayrollEngine.Api.Core;

public interface IControllerRuntime
{
    IDbContext DbContext { get; }
    IConfiguration Configuration { get; }
    LinkGenerator LinkGenerator { get; }
    IApiDescriptionGroupCollectionProvider ApiExplorer { get; }
    IScriptProvider ScriptProvider { get; }
}