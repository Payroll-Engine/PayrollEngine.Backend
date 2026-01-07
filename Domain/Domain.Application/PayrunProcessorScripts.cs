using System;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Scripting;
using PayrollEngine.Domain.Scripting.Controller;

namespace PayrollEngine.Domain.Application;

internal sealed class PayrunProcessorScripts
{
    private IFunctionHost FunctionHost { get; }
    private PayrunProcessorSettings Settings { get; }
    private IRegulationProvider RegulationProvider { get; }
    private IResultProvider ResultProvider { get; }
    private Tenant Tenant { get; }
    private Payrun Payrun { get; }

    internal PayrunProcessorScripts(IFunctionHost functionHost, PayrunProcessorSettings settings,
        IRegulationProvider regulationProvider, IResultProvider resultProvider, Tenant tenant, Payrun payrun)
    {
        FunctionHost = functionHost ?? throw new ArgumentNullException(nameof(functionHost));
        RegulationProvider = regulationProvider ?? throw new ArgumentNullException(nameof(regulationProvider));
        ResultProvider = resultProvider ?? throw new ArgumentNullException(nameof(resultProvider));
        Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        Tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
        Payrun = payrun ?? throw new ArgumentNullException(nameof(payrun));
    }

    internal bool PayrunStart(PayrunContext context)
    {
        Log.Trace($"payrun start {context.PayrunJob.Name}");

        if (string.IsNullOrWhiteSpace(Payrun.StartExpression))
        {
            return true;
        }

        // case value provider without employee case values
        var caseValueProvider = new CaseValueProvider(
            globalCaseValueRepository: context.GlobalCaseValues,
            nationalCaseValueRepository: context.NationalCaseValues,
            companyCaseValueRepository: context.CompanyCaseValues,
            new()
            {
                DbContext = Settings.DbContext,
                Calculator = context.Calculator,
                CaseFieldProvider = context.CaseFieldProvider,
                EvaluationPeriod = context.EvaluationPeriod,
                EvaluationDate = context.EvaluationDate,
                RetroDate = context.RetroDate
            });

        // execute payrun start script
        var start = new PayrunScriptController().Start(new()
        {
            DbContext = Settings.DbContext,
            PayrollCulture = context.PayrollCulture,
            Namespace = null,
            FunctionHost = FunctionHost,
            Tenant = Tenant,
            User = context.User,
            Payroll = context.Payroll,
            Payrun = Payrun,
            PayrunJob = context.PayrunJob,
            ParentPayrunJob = context.ParentPayrunJob,
            ExecutionPhase = context.ExecutionPhase,
            RegulationProvider = RegulationProvider,
            ResultProvider = ResultProvider,
            CaseValueProvider = caseValueProvider,
            RegulationLookupProvider = context.RegulationLookupProvider,
            RuntimeValueProvider = context.RuntimeValueProvider,
            DivisionRepository = Settings.DivisionRepository,
            EmployeeRepository = Settings.EmployeeRepository,
            CalendarRepository = Settings.CalendarRepository,
            PayrollCalculatorProvider = Settings.PayrollCalculatorProvider,
            WebhookDispatchService = Settings.WebhookDispatchService
        });
        return start ?? true;
    }

    internal void PayrunEnd(PayrunContext context)
    {
        Log.Trace($"payrun end {context.PayrunJob.Name}");

        if (string.IsNullOrWhiteSpace(Payrun.EndExpression))
        {
            return;
        }

        // case value provider without employee case values
        var caseValueProvider = new CaseValueProvider(
            globalCaseValueRepository: context.GlobalCaseValues,
            nationalCaseValueRepository: context.NationalCaseValues,
            companyCaseValueRepository: context.CompanyCaseValues,
            new()
            {
                DbContext = Settings.DbContext,
                Calculator = context.Calculator,
                CaseFieldProvider = context.CaseFieldProvider,
                EvaluationPeriod = context.EvaluationPeriod,
                EvaluationDate = context.EvaluationDate,
                RetroDate = context.RetroDate
            });

        // execute payrun end script
        new PayrunScriptController().End(new()
        {
            DbContext = Settings.DbContext,
            PayrollCulture = context.PayrollCulture,
            Namespace = null,
            FunctionHost = FunctionHost,
            Tenant = Tenant,
            User = context.User,
            Payroll = context.Payroll,
            Payrun = Payrun,
            PayrunJob = context.PayrunJob,
            ParentPayrunJob = context.ParentPayrunJob,
            ExecutionPhase = context.ExecutionPhase,
            RegulationProvider = RegulationProvider,
            ResultProvider = ResultProvider,
            CaseValueProvider = caseValueProvider,
            RegulationLookupProvider = context.RegulationLookupProvider,
            RuntimeValueProvider = context.RuntimeValueProvider,
            DivisionRepository = Settings.DivisionRepository,
            EmployeeRepository = Settings.EmployeeRepository,
            CalendarRepository = Settings.CalendarRepository,
            PayrollCalculatorProvider = Settings.PayrollCalculatorProvider,
            WebhookDispatchService = Settings.WebhookDispatchService
        });
    }

    internal bool EmployeeStart(ICaseValueProvider caseValueProvider, PayrunContext context)
    {
        if (caseValueProvider.Employee == null)
        {
            throw new ArgumentException("Missing employee.");
        }

        Log.Trace($"payrun start {context.PayrunJob.Name} on employee {caseValueProvider.Employee.Identifier}");

        if (string.IsNullOrWhiteSpace(Payrun.EmployeeStartExpression))
        {
            return true;
        }

        // execute payrun employee start script
        var start = new PayrunScriptController().EmployeeStart(new()
        {
            DbContext = Settings.DbContext,
            PayrollCulture = context.PayrollCulture,
            Namespace = null,
            FunctionHost = FunctionHost,
            Tenant = Tenant,
            User = context.User,
            Payroll = context.Payroll,
            Payrun = Payrun,
            PayrunJob = context.PayrunJob,
            ParentPayrunJob = context.ParentPayrunJob,
            ExecutionPhase = context.ExecutionPhase,
            RegulationProvider = RegulationProvider,
            ResultProvider = ResultProvider,
            CaseValueProvider = caseValueProvider,
            RegulationLookupProvider = context.RegulationLookupProvider,
            RuntimeValueProvider = context.RuntimeValueProvider,
            DivisionRepository = Settings.DivisionRepository,
            EmployeeRepository = Settings.EmployeeRepository,
            CalendarRepository = Settings.CalendarRepository,
            PayrollCalculatorProvider = Settings.PayrollCalculatorProvider,
            WebhookDispatchService = Settings.WebhookDispatchService
        });
        return start ?? true;
    }

    internal void EmployeeEnd(ICaseValueProvider caseValueProvider, PayrunContext context)
    {
        if (caseValueProvider.Employee == null)
        {
            throw new ArgumentException("Missing employee.");
        }

        Log.Trace($"payrun end {context.PayrunJob.Name} on employee {caseValueProvider.Employee.Identifier}");

        if (string.IsNullOrWhiteSpace(Payrun.EmployeeEndExpression))
        {
            return;
        }

        // execute payrun employee end script
        new PayrunScriptController().EmployeeEnd(new()
        {
            DbContext = Settings.DbContext,
            PayrollCulture = context.PayrollCulture,
            Namespace = null,
            FunctionHost = FunctionHost,
            Tenant = Tenant,
            User = context.User,
            Payroll = context.Payroll,
            Payrun = Payrun,
            PayrunJob = context.PayrunJob,
            ParentPayrunJob = context.ParentPayrunJob,
            ExecutionPhase = context.ExecutionPhase,
            RegulationProvider = RegulationProvider,
            ResultProvider = ResultProvider,
            CaseValueProvider = caseValueProvider,
            RegulationLookupProvider = context.RegulationLookupProvider,
            RuntimeValueProvider = context.RuntimeValueProvider,
            DivisionRepository = Settings.DivisionRepository,
            EmployeeRepository = Settings.EmployeeRepository,
            CalendarRepository = Settings.CalendarRepository,
            PayrollCalculatorProvider = Settings.PayrollCalculatorProvider,
            WebhookDispatchService = Settings.WebhookDispatchService
        });
    }
}