using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollEngine.Client.Scripting;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Scripting;

namespace PayrollEngine.Domain.Application;

public class TenantService : RootApplicationService<ITenantRepository, Tenant>, ITenantService
{
    public TenantService(ITenantRepository repository) :
        base(repository)
    {
    }

    public virtual async Task<bool> ExistsAsync(IDbContext context, string identifier) =>
        await Repository.ExistsAsync(context, identifier);

    public Task<IEnumerable<ActionInfo>> GetSystemScriptActionsAsync(FunctionType functionType = FunctionType.All)
    {
        var actions = new List<ActionInfo>();

        // receive system action infos
        var actionScripts = SystemActionProvider.GetSystemActionScripts(functionType);

        // parse code
        foreach (var script in actionScripts)
        {
            var scriptActions = ActionParser.Parse(script);
            actions.AddRange(scriptActions);
        }

        // ensure system action source
        foreach (var action in actions)
        {
            action.Source = ActionSource.System;
        }

        return System.Threading.Tasks.Task.FromResult<IEnumerable<ActionInfo>>(actions);
    }
}