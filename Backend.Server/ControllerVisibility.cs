using System;
using System.Linq;
using System.Collections.Generic;
using PayrollEngine.Api.Core;
using PayrollEngine.Backend.Controller;

namespace PayrollEngine.Backend.Server;

/// <inheritdoc />
public class ControllerVisibility : IControllerVisibility
{
    private static readonly Type[] ScriptAuditControllers =
    [
        typeof(ScriptAuditController)
    ];
    private static readonly Type[] LookupAuditControllers =
    [
        typeof(LookupAuditController),
        typeof(LookupValueAuditController)
    ];
    private static readonly Type[] InputAuditControllers =
    [
        typeof(CaseAuditController),
        typeof(CaseFieldAuditController),
        typeof(CaseRelationAuditController)
    ];
    private static readonly Type[] PayrunAuditControllers =
    [
        typeof(CollectorAuditController),
        typeof(WageTypeAuditController)
    ];
    private static readonly Type[] ReportAuditControllers =
    [
        typeof(ReportAuditController),
        typeof(ReportTemplateAuditController),
        typeof(ReportParameterAuditController)
    ];

    /// <inheritdoc />
    public string[] GetVisibleControllers(PayrollServerConfiguration config)
    {
        return config.VisibleControllers;
    }

    /// <inheritdoc />
    public string[] GetHiddenControllers(PayrollServerConfiguration config)
    {
        var controllers = new HashSet<string>();

        // server configurations
        if (config.HiddenControllers != null)
        {
            foreach (var controller in config.HiddenControllers)
            {
                controllers.Add(controller);
            }
        }

        // audit trails
        AddControllers(!config.AuditTrail.Script, controllers, ScriptAuditControllers);
        AddControllers(!config.AuditTrail.Lookup, controllers, LookupAuditControllers);
        AddControllers(!config.AuditTrail.Input, controllers, InputAuditControllers);
        AddControllers(!config.AuditTrail.Payrun, controllers, PayrunAuditControllers);
        AddControllers(!config.AuditTrail.Report, controllers, ReportAuditControllers);

        return controllers.ToArray();
    }

    private static void AddControllers(bool condition, HashSet<string> names, IEnumerable<Type> types)
    {
        if (!condition)
        {
            return;
        }
        foreach (var type in types)
        {
            // extract controller name
            names.Add(type.Name.RemoveFromEnd(nameof(Controller)));
        }
    }
}