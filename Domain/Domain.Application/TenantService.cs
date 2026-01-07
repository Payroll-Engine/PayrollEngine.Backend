using System.Threading.Tasks;
using System.Collections.Generic;
using PayrollEngine.Action;
using PayrollEngine.Domain.Model;
using PayrollEngine.Client.Scripting;
using PayrollEngine.Domain.Model.Repository;
using PayrollEngine.Domain.Application.Service;

namespace PayrollEngine.Domain.Application;

public class TenantService(ITenantRepository repository) :
    RootApplicationService<ITenantRepository, Tenant>(repository), ITenantService
{
    public async Task<bool> ExistsAsync(IDbContext context, string identifier) =>
        await Repository.ExistsAsync(context, identifier);

    public Task<IEnumerable<ActionInfo>> GetSystemScriptActionsAsync(FunctionType functionType)
    {
        var actions = new List<ActionInfo>();

        // receive system action infos
        var codes = ScriptProvider.GetActionScriptCodes(functionType);

        // parse code
        foreach (var code in codes)
        {
            var scriptActions = Scripting.Action.ActionReflector.GetActionInfo(code);
            actions.AddRange(scriptActions);
        }

        // ensure system action source
        foreach (var action in actions)
        {
            action.Source = ActionSource.System;
        }

        return System.Threading.Tasks.Task.FromResult<IEnumerable<ActionInfo>>(actions);
    }

    public Task<IEnumerable<ActionInfo>> GetSystemScriptActionPropertiesAsync(FunctionType functionType,
        bool readOnly = false)
    {
        var properties = new List<ActionInfo>();

        // receive system action property infos
        var infos = ScriptPropertyProvider.GetProperties(functionType, readOnly);

        // parse code
        foreach (var info in infos)
        {
            var type = info.Type.IsNullable() ? info.Type.GetNullableType().Name : info.Type.Name;
            properties.Add(new()
            {
                Name = info.Name,
                Description = info.Description,
                FunctionType = info.FunctionType,
                Source = ActionSource.System,
                Categories = [type]
            });
        }

        return System.Threading.Tasks.Task.FromResult<IEnumerable<ActionInfo>>(properties);
    }
}