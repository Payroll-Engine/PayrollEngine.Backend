﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PayrollEngine.Api.Core;
using PayrollEngine.Api.Map;
using PayrollEngine.Client.Scripting;
using PayrollEngine.Domain.Model.Repository;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Domain.Application;
using PayrollEngine.Domain.Application.Service;
using DomainObject = PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Api.Controller;

/// <summary>
/// API controller for payrolls
/// </summary>
[ApiControllerName("Payrolls")]
[Route("api/tenants/{tenantId}/payrolls")]
[ApiExplorerSettings(IgnoreApi = ApiServiceIgnore.Payroll)]
public abstract class PayrollController : RepositoryChildObjectController<ITenantService, IPayrollService,
    ITenantRepository, IPayrollRepository,
    DomainObject.Tenant, DomainObject.Payroll, ApiObject.Payroll>, IPayrollControllerServices
{
    public IPayrollContextService Context { get; }
    public DomainObject.IScriptProvider ScriptProvider => Runtime.ScriptProvider;

    internal IRegulationService RegulationService { get; }
    internal ILookupSetService RegulationLookupSetService { get; }
    internal IUserService UserService { get; }
    internal ITaskService TaskService { get; }
    internal ILogService LogService { get; }
    internal IPayrollService PayrollService => Service;
    internal ICaseService CaseService { get; }
    internal ICaseFieldService CaseFieldService { get; }
    internal ICaseRelationService CaseRelationService { get; }
    internal IGlobalCaseChangeService GlobalChangeService { get; }
    internal IGlobalCaseValueService GlobalCaseValueService { get; }
    internal INationalCaseChangeService NationalChangeService { get; }
    internal INationalCaseValueService NationalCaseValueService { get; }
    internal ICompanyCaseChangeService CompanyChangeService { get; }
    internal ICompanyCaseValueService CompanyCaseValueService { get; }
    internal IEmployeeService EmployeeService { get; }
    internal IEmployeeCaseChangeService EmployeeChangeService { get; }
    internal IEmployeeCaseValueService EmployeeCaseValueService { get; }
    internal DomainObject.IWebhookDispatchService WebhookDispatchService { get; }

    protected PayrollController(IPayrollContextService context, IControllerRuntime runtime) :
        base(context.TenantService, context.PayrollService, runtime, new PayrollMap())
    {
        Context = context;
        RegulationService = context.RegulationService ?? throw new ArgumentNullException(nameof(context.RegulationService));
        RegulationLookupSetService = context.RegulationLookupSetService ?? throw new ArgumentNullException(nameof(context.RegulationLookupSetService));
        CaseService = context.CaseService ?? throw new ArgumentNullException(nameof(context.CaseService));
        CaseFieldService = context.CaseFieldService ?? throw new ArgumentNullException(nameof(context.CaseFieldService));
        UserService = context.UserService ?? throw new ArgumentNullException(nameof(context.UserService));
        TaskService = context.TaskService ?? throw new ArgumentNullException(nameof(context.TaskService));
        LogService = context.LogService ?? throw new ArgumentNullException(nameof(context.LogService));
        CaseRelationService = context.CaseRelationService ?? throw new ArgumentNullException(nameof(context.CaseRelationService));
        GlobalChangeService = context.GlobalChangeService ?? throw new ArgumentNullException(nameof(context.GlobalChangeService));
        GlobalCaseValueService = context.GlobalCaseValueService ?? throw new ArgumentNullException(nameof(context.GlobalCaseValueService));
        NationalChangeService = context.NationalChangeService ?? throw new ArgumentNullException(nameof(context.NationalChangeService));
        NationalCaseValueService = context.NationalCaseValueService ?? throw new ArgumentNullException(nameof(context.NationalCaseValueService));
        CompanyChangeService = context.CompanyChangeService ?? throw new ArgumentNullException(nameof(context.CompanyChangeService));
        CompanyCaseValueService = context.CompanyCaseValueService ?? throw new ArgumentNullException(nameof(context.CompanyCaseValueService));
        EmployeeService = context.EmployeeService ?? throw new ArgumentNullException(nameof(context.EmployeeService));
        EmployeeChangeService = context.EmployeeChangeService ?? throw new ArgumentNullException(nameof(context.EmployeeChangeService));
        EmployeeCaseValueService = context.EmployeeCaseValueService ?? throw new ArgumentNullException(nameof(context.EmployeeCaseValueService));
        WebhookDispatchService = context.WebhookDispatchService ?? throw new ArgumentNullException(nameof(context.WebhookDispatchService));
    }

    #region Payroll Regulations

    protected virtual async Task<ActionResult<IEnumerable<ApiObject.Regulation>>> GetPayrollRegulationsAsync(DomainObject.PayrollQuery query)
    {
        var collectMoment = CurrentEvaluationDate;
        query.RegulationDate ??= collectMoment;
        query.EvaluationDate ??= collectMoment;
        var derivedRegulations = await
            Service.GetDerivedRegulationsAsync(Runtime.DbContext,
                new()
                {
                    TenantId = query.TenantId,
                    PayrollId = query.PayrollId,
                    RegulationDate = query.RegulationDate.Value,
                    EvaluationDate = query.EvaluationDate.Value
                });
        var result = new RegulationMap().ToApi(derivedRegulations);
        return result;
    }

    #endregion

    #region Cases

    protected async Task<ActionResult<ApiObject.Case[]>> GetPayrollAvailableCasesAsync(PayrollCaseQuery query)
    {
        try
        {
            // tenant
            query.TenantId = query.TenantId;
            var tenantResult = VerifyTenant(query.TenantId);
            if (tenantResult != null)
            {
                return tenantResult;
            }
            var tenant = await ParentService.GetAsync(Runtime.DbContext, query.TenantId);
            if (tenant == null)
            {
                return BadRequest($"Unknown tenant with id {query.TenantId}");
            }

            // user
            var user = await UserService.GetAsync(Runtime.DbContext, query.TenantId, query.UserId);
            if (user == null)
            {
                return BadRequest($"Unknown user with id {query.UserId}");
            }

            // payroll
            query.PayrollId = query.PayrollId;
            var payroll = await Service.GetAsync(Runtime.DbContext, query.TenantId, query.PayrollId);
            if (payroll == null)
            {
                return BadRequest($"Unknown payroll with id {query.PayrollId}");
            }

            // cluster
            if (!string.IsNullOrWhiteSpace(query.ClusterSetName) && !payroll.ClusterSetExists(query.ClusterSetName))
            {
                return BadRequest($"Unknown cluster set {query.ClusterSetName}");
            }

            // employee
            DomainObject.Employee employee = null;
            if (query.CaseType == CaseType.Employee)
            {
                if (!query.EmployeeId.HasValue)
                {
                    return BadRequest("Missing employee id on employee cases");
                }
                employee = await EmployeeService.GetAsync(Runtime.DbContext, query.TenantId, query.EmployeeId.Value);
            }

            // lookups
            var lookupProvider = await this.NewRegulationLookupProviderAsync(Runtime.DbContext, tenant, payroll);

            // case collector
            var collectMoment = CurrentEvaluationDate;
            query.RegulationDate ??= collectMoment;
            query.EvaluationDate ??= collectMoment;
            using var collector = this.NewCaseCollector(new()
            {
                DbContext = Runtime.DbContext,
                CaseType = query.CaseType,
                Tenant = tenant,
                Payroll = payroll,
                User = user,
                RegulationLookupProvider = lookupProvider,
                RegulationDate = query.RegulationDate.Value,
                EvaluationDate = query.EvaluationDate.Value,
                ClusterSetName = query.ClusterSetName,
                Employee = employee
            });

            // cases
            query.Language ??= default;
            var cases = query.CaseType switch
            {
                CaseType.Global => await collector.GetAvailableGlobalCasesAsync(query.Language.Value, query.CaseNames),
                CaseType.National => await collector.GetAvailableNationalCasesAsync(query.Language.Value, query.CaseNames),
                CaseType.Company => await collector.GetAvailableCompanyCasesAsync(query.Language.Value, query.CaseNames),
                CaseType.Employee => await collector.GetAvailableEmployeeCasesAsync(query.Language.Value, query.CaseNames),
                _ => throw new ArgumentOutOfRangeException(nameof(query.CaseType), query.CaseType, null)
            };
            return new CaseMap().ToApi(cases);
        }
        catch (ScriptException exception)
        {
            return UnprocessableEntity(exception.GetBaseException().ToString());
        }
        catch (Exception exception)
        {
            return InternalServerError(exception);
        }
    }

    public virtual async Task<ActionResult<ApiObject.CaseSet>> BuildPayrollCaseAsync(
        int tenantId, int payrollId, string caseName, CaseBuildQuery query, ApiObject.CaseChangeSetup caseChangeSetup = null)
    {
        // apply path ids to the query
        query.TenantId = tenantId;
        query.PayrollId = payrollId;
        query.CaseName = caseName;

        try
        {
            // query setup
            var setupQuery = await SetupQuery(query, query.UserId, query.ClusterSetName);
            if (setupQuery.Item2 != null)
            {
                // invalid setup response
                return setupQuery.Item2;
            }
            var querySetup = setupQuery.Item1;

            // case
            if (string.IsNullOrWhiteSpace(query.CaseName))
            {
                return BadRequest("Missing case name");
            }
            var regulationCases = (await PayrollService.GetDerivedCasesAsync(Runtime.DbContext, query,
                caseNames: new[] { query.CaseName })).ToList();
            if (!regulationCases.Any())
            {
                return BadRequest($"Unknown case {query.CaseName}");
            }
            var @case = regulationCases.First();

            // employee
            DomainObject.Employee employee = null;
            if (@case.CaseType == CaseType.Employee)
            {
                if (!query.EmployeeId.HasValue)
                {
                    return BadRequest("Missing employee id on employee cases");
                }
                employee = await EmployeeService.GetAsync(Runtime.DbContext, query.TenantId, query.EmployeeId.Value);
            }

            // case change
            DomainObject.CaseChangeSetup domainCaseSetup = new();
            if (caseChangeSetup != null)
            {
                domainCaseSetup = new CaseChangeSetupMap().ToDomain(caseChangeSetup);
                foreach (var caseValue in DomainObject.CaseChangeSetupExtensions.CollectCaseValues(domainCaseSetup))
                {
                    if (caseValue.Start.HasValue && !caseValue.Start.Value.IsUtc())
                    {
                        return BadRequest("Case value start must be UTC");
                    }
                    if (caseValue.End.HasValue && !caseValue.End.Value.IsUtc())
                    {
                        return BadRequest("Case value end must be UTC");
                    }
                }
            }

            // lookups
            var lookupProvider = await this.NewRegulationLookupProviderAsync(Runtime.DbContext, querySetup.Tenant, querySetup.Payroll);

            // resolver
            var collectMoment = CurrentEvaluationDate;
            query.RegulationDate ??= collectMoment;
            query.EvaluationDate ??= collectMoment;
            using var builder = this.NewCaseBuilder(new()
            {
                DbContext = Runtime.DbContext,
                CaseType = @case.CaseType,
                Tenant = querySetup.Tenant,
                Payroll = querySetup.Payroll,
                User = querySetup.User,
                RegulationLookupProvider = lookupProvider,
                RegulationDate = query.RegulationDate.Value,
                EvaluationDate = query.EvaluationDate.Value,
                ClusterSetName = query.ClusterSetName,
                Employee = employee
            });

            // case set
            query.Language ??= default;
            DomainObject.CaseSet caseSet = @case.CaseType switch
            {
                CaseType.Global => await builder.BuildGlobalCaseAsync(query.CaseName, domainCaseSetup, query.Language.Value),
                CaseType.National =>
                    await builder.BuildNationalCaseAsync(query.CaseName, domainCaseSetup, query.Language.Value),
                CaseType.Company => await builder.BuildCompanyCaseAsync(query.CaseName, domainCaseSetup, query.Language.Value),
                CaseType.Employee =>
                    await builder.BuildEmployeeCaseAsync(query.CaseName, domainCaseSetup, query.Language.Value),
                _ => throw new ArgumentOutOfRangeException()
            };
            return new CaseSetMap().ToApi(caseSet);
        }
        catch (ScriptException exception)
        {
            return UnprocessableEntity(exception.GetBaseException().ToString());
        }
        catch (Exception exception)
        {
            return InternalServerError(exception);
        }
    }

    #endregion

    #region Case Changes

    public virtual async Task<ActionResult> QueryPayrollCaseChangeValuesAsync(
        int tenantId, int payrollId, DomainObject.PayrollCaseChangeQuery query = null)
    {
        query ??= new();
        if (query.CaseType == CaseType.Employee && !query.EmployeeId.HasValue)
        {
            return BadRequest("Missing employee id");
        }

        try
        {
            IEnumerable<ApiObject.CaseChangeCaseValue> caseChangeValues = null;
            long? caseChangeValuesCount = null;

            // cases
            var cases = (await GetPayrollCasesAsync(
                new()
                {
                    TenantId = tenantId,
                    PayrollId = payrollId,
                    RegulationDate = query.RegulationDate,
                    EvaluationDate = query.EvaluationDate
                },
                caseType: query.CaseType,
                caseNames: null,
                overrideType: null,
                clusterSetName: query.ClusterSetName)).Value;
            var caseNames = cases?.Select(x => x.Name).ToList();
            if (caseNames != null && caseNames.Any())
            {
                // case fields
                var now = Date.Now;
                var caseFieldNames = (await Service.GetDerivedCaseFieldsOfCaseAsync(Runtime.DbContext,
                    new()
                    {
                        TenantId = tenantId,
                        PayrollId = payrollId,
                        RegulationDate = query.RegulationDate ?? now,
                        EvaluationDate = query.EvaluationDate ?? now
                    },
                    caseNames)).Select(x => x.Name);
                if (caseFieldNames.Any())
                {
                    // case values
                    query.Result ??= QueryResultType.Items;

                    // values
                    if (query.Result is QueryResultType.Items or QueryResultType.ItemsWithCount)
                    {
                        var domainValues = query.CaseType switch
                        {
                            CaseType.Global => await GlobalChangeService.QueryValuesAsync(Runtime.DbContext, tenantId, tenantId, query),
                            CaseType.National => await NationalChangeService.QueryValuesAsync(Runtime.DbContext, tenantId, tenantId, query),
                            CaseType.Company => await CompanyChangeService.QueryValuesAsync(Runtime.DbContext, tenantId, tenantId, query),
                            // ReSharper disable once PossibleInvalidOperationException
                            CaseType.Employee => await EmployeeChangeService.QueryValuesAsync(Runtime.DbContext, tenantId, query.EmployeeId.Value, query),
                            _ => throw new ArgumentOutOfRangeException(nameof(query.CaseType), query.CaseType, null)
                        };
                        caseChangeValues = new CaseChangeCaseValueMap().ToApi(domainValues);
                    }

                    // count
                    if (query.Result is QueryResultType.Count ||
                        query.Result is QueryResultType.ItemsWithCount && caseChangeValues != null)
                    {
                        caseChangeValuesCount = query.CaseType switch
                        {
                            CaseType.Global => await GlobalChangeService.QueryValuesCountAsync(Runtime.DbContext, tenantId, tenantId, query),
                            CaseType.National => await NationalChangeService.QueryValuesCountAsync(Runtime.DbContext, tenantId, tenantId, query),
                            CaseType.Company => await CompanyChangeService.QueryValuesCountAsync(Runtime.DbContext, tenantId, tenantId, query),
                            // ReSharper disable once PossibleInvalidOperationException
                            CaseType.Employee => await EmployeeChangeService.QueryValuesCountAsync(Runtime.DbContext, tenantId, query.EmployeeId.Value, query),
                            _ => throw new ArgumentOutOfRangeException(nameof(query.CaseType), query.CaseType, null)
                        };
                    }
                }
            }

            // response
            switch (query.Result)
            {
                case QueryResultType.Items:
                    var values = caseChangeValues != null ? caseChangeValues.ToArray() : Array.Empty<ApiObject.CaseChangeCaseValue>();
                    return Ok(values);
                case QueryResultType.Count:
                    var count = caseChangeValuesCount ?? 0;
                    return Ok(count);
                case QueryResultType.ItemsWithCount:
                    values = caseChangeValues != null ? caseChangeValues.ToArray() : Array.Empty<ApiObject.CaseChangeCaseValue>();
                    count = caseChangeValuesCount ?? 0;
                    return Ok(new QueryResult<ApiObject.CaseChangeCaseValue>(values, count));
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        catch (ScriptException exception)
        {
            return UnprocessableEntity(exception.GetBaseException().ToString());
        }
        catch (Exception exception)
        {
            return InternalServerError(exception);
        }
    }

    #endregion

    #region Case Values

    async Task<IEnumerable<DomainObject.CaseValue>> IPayrollControllerServices.GetPayrollTimeCaseValuesAsync(
        DomainObject.IDbContext context, DomainObject.PayrollQuery query, CaseType caseType, string[] caseFieldNames, DateTime? valueDate)
    {
        var result = await GetPayrollTimeCaseValuesAsync(query, caseType, caseFieldNames, valueDate);
        return new CaseValueMap().ToDomain(result.Value);
    }

    protected async Task<ActionResult<IEnumerable<ApiObject.CaseValue>>> GetPayrollTimeCaseValuesAsync(
        DomainObject.PayrollQuery query, CaseType caseType, string[] caseFieldNames = null, DateTime? valueDate = null) =>
        await GetPayrollTimeCaseValuesAsync(Runtime.DbContext, query, caseType, caseFieldNames, valueDate);

    protected async Task<ActionResult<IEnumerable<ApiObject.CaseValue>>> GetPayrollTimeCaseValuesAsync(DomainObject.IDbContext context,
        DomainObject.PayrollQuery query, CaseType caseType, string[] caseFieldNames = null, DateTime? valueDate = null)
    {
        try
        {
            // query setup
            var setupQuery = await SetupQuery(query);
            if (setupQuery.Item2 != null)
            {
                // invalid setup response
                return setupQuery.Item2;
            }
            var querySetup = setupQuery.Item1;

            // employee
            DomainObject.Employee employee = null;
            if (query.EmployeeId.HasValue)
            {
                employee = await EmployeeService.GetAsync(Runtime.DbContext, query.TenantId, query.EmployeeId.Value);
                if (employee == null)
                {
                    return BadRequest($"Unknown employee with id {query.EmployeeId}");
                }
            }

            // dates
            var caseValueDate = (valueDate ?? CurrentEvaluationDate).ToUtc();
            var caseRegulationDate = (query.RegulationDate ?? caseValueDate).ToUtc();
            var caseEvaluationDate = (query.EvaluationDate ?? caseValueDate).ToUtc();

            // lookups
            var lookupProvider = await this.NewRegulationLookupProviderAsync(Runtime.DbContext, querySetup.Tenant,
                querySetup.Payroll, caseRegulationDate, caseEvaluationDate);

            // server configuration
            var serverConfiguration = Configuration.GetConfiguration<PayrollServerConfiguration>();

            // settings
            var settings = new CaseValueToolSettings
            {
                DbContext = Runtime.DbContext,
                Tenant = querySetup.Tenant,
                Payroll = querySetup.Payroll,
                TaskRepository = TaskService.Repository,
                LogRepository = LogService.Repository,
                PayrollRepository = Service.Repository,
                CaseRepository = CaseService.Repository,
                RegulationLookupProvider = lookupProvider,
                ValueDate = caseValueDate,
                EvaluationDate = caseEvaluationDate,
                ScriptProvider = Runtime.ScriptProvider,
                AssemblyCacheTimeout = serverConfiguration.AssemblyCacheTimeout
            };

            // get case value
            using var caseValueTool = employee != null
                ? new(employee,
                    globalCaseValueRepository: GlobalCaseValueService.Repository,
                    nationalCaseValueRepository: NationalCaseValueService.Repository,
                    companyCaseValueRepository: CompanyCaseValueService.Repository,
                    employeeCaseValueRepository: EmployeeCaseValueService.Repository,
                    settings: settings) :
                new CaseValueTool(
                    globalCaseValueRepository: GlobalCaseValueService.Repository,
                    nationalCaseValueRepository: NationalCaseValueService.Repository,
                    companyCaseValueRepository: CompanyCaseValueService.Repository,
                    settings: settings);

            var domainCaseValues = caseFieldNames != null && caseFieldNames.Length > 0 ?
                // get case values of specific case fields
                await caseValueTool.GetTimeCaseValuesAsync(caseValueDate, caseType, caseFieldNames) :
                // get case values by case case type
                await caseValueTool.GetTimeCaseValuesAsync(caseValueDate, caseType);
            return domainCaseValues.Select(domainCaseValue => new CaseValueMap().ToApi(domainCaseValue)).ToList();
        }
        catch (Exception exception)
        {
            return InternalServerError(exception);
        }
    }

    protected virtual async Task<ActionResult<ApiObject.CaseFieldValue[]>> GetPayrollAvailableCaseFieldValuesAsync(
        DomainObject.PayrollQuery query,
        int userId, string[] caseFieldNames, DateTime startDate, DateTime endDate)
    {
        if (caseFieldNames == null || !caseFieldNames.Any())
        {
            throw new ArgumentException(nameof(caseFieldNames));
        }
        if (startDate >= endDate)
        {
            return BadRequest($"Start date {startDate.ToPeriodStartString()} must be before end date {endDate.ToPeriodEndString()}");
        }
        if (startDate.Year != endDate.Year || startDate.Month != endDate.Month)
        {
            return BadRequest("Start and end end date must be within a period");
        }

        try
        {
            // query setup
            var setupQuery = await SetupQuery(query, userId: userId);
            if (setupQuery.Item2 != null)
            {
                // invalid setup response
                return setupQuery.Item2;
            }
            var querySetup = setupQuery.Item1;

            // case fields
            var caseType = CaseType.Global;
            foreach (var caseFieldName in caseFieldNames)
            {
                // case field
                var caseValueReference = new CaseValueReference(caseFieldName);
                var caseFields = await Service.GetDerivedCaseFieldsAsync(Runtime.DbContext, query, new[] { caseValueReference.CaseFieldName });
                //var caseFields = await CaseFieldService.GetRegulationCaseFieldsAsync(query.TenantId, new[] { caseValueReference.CaseFieldName });
                var caseFieldId = caseFields.FirstOrDefault()?.Id;
                if (!caseFieldId.HasValue)
                {
                    return BadRequest($"Unknown case field {caseFieldName}");
                }

                // case
                var caseId = await CaseFieldService.GetParentIdAsync(Runtime.DbContext, caseFieldId.Value);
                if (!caseId.HasValue)
                {
                    return BadRequest($"Unknown case field with id {caseFieldId.Value}");
                }

                // regulation
                var regulationId = await CaseService.GetParentIdAsync(Runtime.DbContext, caseId.Value);
                if (!regulationId.HasValue)
                {
                    return BadRequest($"Unknown regulation for case field {caseValueReference.CaseFieldName}");
                }

                // case
                DomainObject.Case @case = await CaseService.GetAsync(Runtime.DbContext, regulationId.Value, caseId.Value);
                if (@case == null)
                {
                    return BadRequest($"Unknown regulation case with id {caseId.Value}");
                }
                if (@case.CaseType > caseType)
                {
                    caseType = @case.CaseType;
                }
            }

            // employee
            DomainObject.Employee employee = null;
            if (caseType == CaseType.Employee)
            {
                if (!query.EmployeeId.HasValue)
                {
                    return BadRequest("Missing employee id on employee cases");
                }
                employee = await EmployeeService.GetAsync(Runtime.DbContext, query.TenantId, query.EmployeeId.Value);
            }

            // lookups
            var lookupProvider = await this.NewRegulationLookupProviderAsync(Runtime.DbContext, querySetup.Tenant, querySetup.Payroll);

            // resolver
            var collectMoment = CurrentEvaluationDate;
            query.RegulationDate ??= collectMoment;
            query.EvaluationDate ??= collectMoment;
            using var collector = this.NewCaseCollector(new()
            {
                DbContext = Runtime.DbContext,
                CaseType = caseType,
                Tenant = querySetup.Tenant,
                Payroll = querySetup.Payroll,
                User = querySetup.User,
                RegulationLookupProvider = lookupProvider,
                RegulationDate = query.RegulationDate.Value,
                EvaluationDate = query.EvaluationDate.Value,
                Employee = employee
            });

            // period values
            if (endDate.IsMidnight())
            {
                // end period without daytime: expect the hole day
                endDate = endDate.LastMomentOfDay();
            }
            var period = new DatePeriod(startDate.ToUtc(), endDate.ToUtc());
            var domainPeriodValues = await collector.GetCasePeriodValuesAsync(period, caseFieldNames);
            var result = new CaseFieldValueMap().ToApi(domainPeriodValues);
            return result;
        }
        catch (ScriptException exception)
        {
            return UnprocessableEntity(exception.GetBaseException().ToString());
        }
        catch (Exception exception)
        {
            return InternalServerError(exception);
        }
    }

    protected virtual async Task<ActionResult<ApiObject.CaseFieldValue[]>> GetPayrollCaseValuesAsync(
        DomainObject.PayrollQuery query, DateTime startDate, DateTime endDate, string[] caseFieldNames, string caseSlot = null)
    {
        if (startDate >= endDate)
        {
            return BadRequest($"Start date {startDate.ToPeriodStartString()} must be before end date {endDate.ToPeriodEndString()}");
        }
        if (caseFieldNames == null || caseFieldNames.Length == 0)
        {
            return BadRequest("Missing case field names");
        }

        // tenant check
        var tenantResult = VerifyTenant(query.TenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }

        try
        {
            // query setup
            var setupQuery = await SetupQuery(query);
            if (setupQuery.Item2 != null)
            {
                // invalid setup response
                return setupQuery.Item2;
            }
            var querySetup = setupQuery.Item1;

            var valueDate = Date.Now;
            var evaluationDate = query.EvaluationDate ?? valueDate;
            var regulationDate = query.RegulationDate ?? valueDate;

            var caseFields = (await Service.GetDerivedCaseFieldsAsync(Runtime.DbContext,
                new()
                {
                    TenantId = query.TenantId,
                    PayrollId = query.PayrollId,
                    RegulationDate = regulationDate,
                    EvaluationDate = evaluationDate
                },
                caseFieldNames)).ToList();
            var caseValues = new List<ApiObject.CaseFieldValue>();
            foreach (var caseFieldName in caseFieldNames)
            {
                var caseField = caseFields.FirstOrDefault(x => string.Equals(x.Name, caseFieldName));
                if (caseField == null)
                {
                    return BadRequest($"Unknown case field {caseFieldName}");
                }

                // case
                var caseId = await CaseFieldService.GetParentIdAsync(Runtime.DbContext, caseField.Id);
                if (!caseId.HasValue)
                {
                    return BadRequest($"Unknown case field {caseField.Name}");
                }

                // regulation
                var regulationId = await CaseService.GetParentIdAsync(Runtime.DbContext, caseId.Value);
                if (!regulationId.HasValue)
                {
                    return BadRequest($"Unknown regulation for case field {caseField.Name}");
                }

                // case
                DomainObject.Case @case = await CaseService.GetAsync(Runtime.DbContext, regulationId.Value, caseId.Value);
                if (@case == null)
                {
                    return BadRequest($"Unknown regulation case with id {caseId.Value}");
                }

                // lookups
                var lookupProvider = await this.NewRegulationLookupProviderAsync(Runtime.DbContext, querySetup.Tenant, querySetup.Payroll);

                // server configuration
                var serverConfiguration = Configuration.GetConfiguration<PayrollServerConfiguration>();

                // settings
                var settings = new CaseValueToolSettings
                {
                    DbContext = Runtime.DbContext,
                    Tenant = querySetup.Tenant,
                    Payroll = querySetup.Payroll,
                    TaskRepository = TaskService.Repository,
                    LogRepository = LogService.Repository,
                    PayrollRepository = Service.Repository,
                    CaseRepository = CaseService.Repository,
                    RegulationLookupProvider = lookupProvider,
                    ValueDate = valueDate,
                    EvaluationDate = evaluationDate,
                    ScriptProvider = Runtime.ScriptProvider,
                    AssemblyCacheTimeout = serverConfiguration.AssemblyCacheTimeout
                };

                // case value periods
                var period = new DatePeriod(startDate, endDate);
                List<DomainObject.CaseFieldValue> periodValues;
                switch (@case.CaseType)
                {
                    case CaseType.Global:
                        {
                            // global case value periods
                            using var caseValueTool = new CaseValueTool(
                                globalCaseValueRepository: Service.GlobalCaseValueRepository,
                                settings: settings);
                            periodValues = await caseValueTool.GetCaseValuesAsync(caseField.Name, period, caseSlot);
                        }
                        break;
                    case CaseType.National:
                        {
                            // national case value periods
                            using var caseValueTool = new CaseValueTool(
                                globalCaseValueRepository: Service.GlobalCaseValueRepository,
                                nationalCaseValueRepository: Service.NationalCaseValueRepository,
                                settings: settings);
                            periodValues = await caseValueTool.GetCaseValuesAsync(caseField.Name, period, caseSlot);
                        }
                        break;
                    case CaseType.Company:
                        {
                            // company case value periods
                            using var caseValueTool = new CaseValueTool(
                                globalCaseValueRepository: Service.GlobalCaseValueRepository,
                                nationalCaseValueRepository: Service.NationalCaseValueRepository,
                                companyCaseValueRepository: Service.CompanyCaseValueRepository,
                                settings: settings);
                            periodValues = await caseValueTool.GetCaseValuesAsync(caseFieldName, period, caseSlot);
                        }
                        break;
                    case CaseType.Employee:
                        {
                            if (!query.EmployeeId.HasValue)
                            {
                                return BadRequest("Missing employee id to access employee data");
                            }
                            // employee case value periods
                            var employee = await EmployeeService.GetAsync(Runtime.DbContext, query.TenantId, query.EmployeeId.Value);
                            using var caseValueTool = new CaseValueTool(employee,
                                globalCaseValueRepository: Service.GlobalCaseValueRepository,
                                nationalCaseValueRepository: Service.NationalCaseValueRepository,
                                companyCaseValueRepository: Service.CompanyCaseValueRepository,
                                employeeCaseValueRepository: Service.EmployeeCaseValueRepository,
                                settings: settings);
                            periodValues = await caseValueTool.GetCaseValuesAsync(caseFieldName, period, caseSlot);
                            break;
                        }
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                caseValues.AddRange(new CaseFieldValueMap().ToApi(periodValues));
            }

            return caseValues.ToArray();
        }
        catch (Exception exception)
        {
            return InternalServerError(exception);
        }
    }

    public virtual async Task<ActionResult<ApiObject.CaseChange>> AddPayrollCaseAsync(int tenantId, int payrollId,
        ApiObject.CaseChangeSetup caseChangeSetup)
    {
        try
        {
            // tenant
            var tenantResult = VerifyTenant(tenantId);
            if (tenantResult != null)
            {
                return tenantResult;
            }

            var builder = new PayrollControllerCaseBuilder(this);
            var caseChange = await builder.AddPayrollCaseAsync(Runtime.DbContext, tenantId, payrollId,
                caseChangeSetup, Request.Path);
            return caseChange;
        }
        catch (ScriptException exception)
        {
            return UnprocessableEntity(exception.GetBaseException().ToString());
        }
        catch (Exception exception)
        {
            return InternalServerError(exception);
        }
    }

    Task<List<DomainObject.CaseValidationIssue>> IPayrollControllerServices.ValidateCaseAsync(ValidateCaseSettings settings) =>
        ValidateCaseAsync(settings);

    private async Task<List<DomainObject.CaseValidationIssue>> ValidateCaseAsync(ValidateCaseSettings settings)
    {
        // lookups
        var regulationLookupProvider = await this.NewRegulationLookupProviderAsync(Runtime.DbContext, settings.Tenant, settings.Payroll);

        // setup validator
        var collectMoment = CurrentEvaluationDate;
        settings.RegulationDate ??= collectMoment;
        settings.EvaluationDate ??= collectMoment;
        using var validator = this.NewCaseValidator(new()
        {
            DbContext = Runtime.DbContext,
            CaseType = settings.CaseType,
            Tenant = settings.Tenant,
            Payroll = settings.Payroll,
            User = settings.User,
            RegulationLookupProvider = regulationLookupProvider,
            RegulationDate = settings.RegulationDate.Value,
            EvaluationDate = settings.EvaluationDate.Value,
            Employee = settings.Employee
        });

        // case values maybe changed during the validation
        var issues = settings.CaseType switch
        {
            CaseType.Global => await validator.ValidateGlobalCaseAsync(settings.ValidationCase.Name, null,
                settings.DomainCaseChangeSetup, settings.CancellationDate),
            CaseType.National => await validator.ValidateNationalCaseAsync(settings.ValidationCase.Name, null,
                settings.DomainCaseChangeSetup, settings.CancellationDate),
            CaseType.Company => await validator.ValidateCompanyCaseAsync(settings.ValidationCase.Name, null,
                settings.DomainCaseChangeSetup, settings.CancellationDate),
            CaseType.Employee => await validator.ValidateEmployeeCaseAsync(settings.ValidationCase.Name, null,
                settings.DomainCaseChangeSetup, settings.CancellationDate),
            _ => throw new ArgumentOutOfRangeException()
        };

        return issues;
    }

    #endregion

    #region Payroll Regulation Items

    protected virtual async Task<ActionResult<ApiObject.Case[]>> GetPayrollCasesAsync(
        DomainObject.PayrollQuery query,
        CaseType? caseType, string[] caseNames, OverrideType? overrideType, string clusterSetName)
    {
        try
        {
            // tenant
            var tenantResult = VerifyTenant(query.TenantId);
            if (tenantResult != null)
            {
                return tenantResult;
            }

            // query setup
            var setupQuery = await SetupQuery(query, clusterSetName: clusterSetName);
            if (setupQuery.Item2 != null)
            {
                // invalid setup response
                return setupQuery.Item2;
            }
            var querySetup = setupQuery.Item1;

            // collectors
            var collectMoment = CurrentEvaluationDate;
            query.RegulationDate ??= collectMoment;
            query.EvaluationDate ??= collectMoment;
            overrideType ??= OverrideType.Active;
            var cases = await Service.Repository.GetDerivedCasesAsync(Runtime.DbContext,
                new()
                {
                    TenantId = query.TenantId,
                    PayrollId = query.PayrollId,
                    RegulationDate = query.RegulationDate.Value.ToUtc(),
                    EvaluationDate = query.EvaluationDate.Value.ToUtc()
                },
                caseType: caseType,
                caseNames: caseNames,
                overrideType: overrideType,
                clusterSet: querySetup.ClusterSet);

            // select the most derived collector by name
            var casesByNames = cases.ToLookup(c => c.Name, c => c);
            var caseMap = new CaseMap();
            var derivedCases = new List<ApiObject.Case>();
            foreach (var caseByName in casesByNames)
            {
                derivedCases.Add(caseMap.ToApi(caseByName.First()));
            }

            return derivedCases.ToArray();
        }
        catch (Exception exception)
        {
            return InternalServerError(exception);
        }
    }

    protected virtual async Task<ActionResult<ApiObject.CaseField[]>> GetPayrollCaseFieldsAsync(
        DomainObject.PayrollQuery query,
        string[] caseFieldNames, OverrideType? overrideType, string clusterSetName)
    {
        try
        {
            // tenant
            var tenantResult = VerifyTenant(query.TenantId);
            if (tenantResult != null)
            {
                return tenantResult;
            }

            // query setup
            var setupQuery = await SetupQuery(query, clusterSetName: clusterSetName);
            if (setupQuery.Item2 != null)
            {
                // invalid setup response
                return setupQuery.Item2;
            }
            var querySetup = setupQuery.Item1;

            // collectors
            var collectMoment = CurrentEvaluationDate;
            query.RegulationDate ??= collectMoment;
            query.EvaluationDate ??= collectMoment;
            overrideType ??= OverrideType.Active;
            var caseFields = await Service.GetDerivedCaseFieldsAsync(Runtime.DbContext,
                new()
                {
                    TenantId = query.TenantId,
                    PayrollId = query.PayrollId,
                    RegulationDate = query.RegulationDate.Value.ToUtc(),
                    EvaluationDate = query.EvaluationDate.Value.ToUtc()
                },
                caseFieldNames: caseFieldNames,
                overrideType: overrideType,
                clusterSet: querySetup.ClusterSet);

            // select the most derived collector by name
            var caseFieldsByNames = caseFields.ToLookup(c => c.Name, c => c);
            var caseFieldMap = new CaseFieldMap();
            var derivedCaseFields = new List<ApiObject.CaseField>();
            foreach (var caseFieldByName in caseFieldsByNames)
            {
                derivedCaseFields.Add(caseFieldMap.ToApi(caseFieldByName.First()));
            }

            // sort case fields by order and then by id
            derivedCaseFields = derivedCaseFields.OrderBy(x => x.Order).ThenBy(x => x.Id).ToList();

            return derivedCaseFields.ToArray();
        }
        catch (Exception exception)
        {
            return InternalServerError(exception);
        }
    }

    protected virtual async Task<ActionResult<ApiObject.CaseRelation[]>> GetPayrollCaseRelationsAsync(
        DomainObject.PayrollQuery query,
        string sourceCaseName, string targetCaseName, OverrideType? overrideType, string clusterSetName)
    {
        try
        {
            // tenant
            var tenantResult = VerifyTenant(query.TenantId);
            if (tenantResult != null)
            {
                return tenantResult;
            }

            // query setup
            var setupQuery = await SetupQuery(query, clusterSetName: clusterSetName);
            if (setupQuery.Item2 != null)
            {
                // invalid setup response
                return setupQuery.Item2;
            }
            var querySetup = setupQuery.Item1;

            // collectors
            var collectMoment = CurrentEvaluationDate;
            query.RegulationDate ??= collectMoment;
            query.EvaluationDate ??= collectMoment;
            overrideType ??= OverrideType.Active;
            var caseRelations = await Service.GetDerivedCaseRelationsAsync(Runtime.DbContext,
                new()
                {
                    TenantId = query.TenantId,
                    PayrollId = query.PayrollId,
                    RegulationDate = query.RegulationDate.Value.ToUtc(),
                    EvaluationDate = query.EvaluationDate.Value.ToUtc()
                },
                sourceCaseName: sourceCaseName,
                targetCaseName: targetCaseName,
                overrideType: overrideType,
                clusterSet: querySetup.ClusterSet);

            // select the most derived collector by name
            var caseRelationsByNames = caseRelations.ToLookup(x => new { x.SourceCaseName, x.SourceCaseSlot, x.TargetCaseName, x.TargetCaseSlot }, c => c);
            var caseRelationMap = new CaseRelationMap();
            var derivedCaseRelations = new List<ApiObject.CaseRelation>();
            foreach (var caseRelationByName in caseRelationsByNames)
            {
                derivedCaseRelations.Add(caseRelationMap.ToApi(caseRelationByName.First()));
            }

            // sort case relations by order and then by id
            derivedCaseRelations = derivedCaseRelations.OrderBy(x => x.Order).ThenBy(x => x.Id).ToList();

            return derivedCaseRelations.ToArray();
        }
        catch (Exception exception)
        {
            return InternalServerError(exception);
        }
    }

    protected virtual async Task<ActionResult<ApiObject.WageType[]>> GetPayrollWageTypesAsync(
        DomainObject.PayrollQuery query,
        decimal[] wageTypeNumbers, OverrideType? overrideType, string clusterSetName)
    {
        try
        {
            // tenant
            var tenantResult = VerifyTenant(query.TenantId);
            if (tenantResult != null)
            {
                return tenantResult;
            }

            // query setup
            var setupQuery = await SetupQuery(query, clusterSetName: clusterSetName);
            if (setupQuery.Item2 != null)
            {
                // invalid setup response
                return setupQuery.Item2;
            }
            var querySetup = setupQuery.Item1;

            // wage types
            var collectMoment = CurrentEvaluationDate;
            query.RegulationDate ??= collectMoment;
            query.EvaluationDate ??= collectMoment;
            overrideType ??= OverrideType.Active;
            var wageTypes = (await Service.GetDerivedWageTypesAsync(Runtime.DbContext,
                new()
                {
                    TenantId = query.TenantId,
                    PayrollId = query.PayrollId,
                    RegulationDate = query.RegulationDate.Value.ToUtc(),
                    EvaluationDate = query.EvaluationDate.Value.ToUtc()
                },
                wageTypeNumbers: wageTypeNumbers,
                overrideType: overrideType,
                clusterSet: querySetup.ClusterSet)).ToList();

            // select the most derived wage type by wage type number
            var wageTypeByNumbers = wageTypes.ToLookup(wt => wt.WageTypeNumber, wt => wt);
            var wageTypeMap = new WageTypeMap();
            var derivedWageTypes = new List<ApiObject.WageType>();
            foreach (var wageTypeByNumber in wageTypeByNumbers)
            {
                derivedWageTypes.Add(wageTypeMap.ToApi(wageTypeByNumber.First()));
            }

            return derivedWageTypes.ToArray();
        }
        catch (Exception exception)
        {
            return InternalServerError(exception);
        }
    }

    protected virtual async Task<ActionResult<ApiObject.Collector[]>> GetPayrollCollectorsAsync(
        DomainObject.PayrollQuery query,
        string[] collectorNames, OverrideType? overrideType, string clusterSetName)
    {
        try
        {
            // tenant
            var tenantResult = VerifyTenant(query.TenantId);
            if (tenantResult != null)
            {
                return tenantResult;
            }

            // query setup
            var setupQuery = await SetupQuery(query, clusterSetName: clusterSetName);
            if (setupQuery.Item2 != null)
            {
                // invalid setup response
                return setupQuery.Item2;
            }
            var querySetup = setupQuery.Item1;

            // collectors
            var collectMoment = CurrentEvaluationDate;
            query.RegulationDate ??= collectMoment;
            query.EvaluationDate ??= collectMoment;
            overrideType ??= OverrideType.Active;
            var collectors = await Service.GetDerivedCollectorsAsync(Runtime.DbContext,
                new()
                {
                    TenantId = query.TenantId,
                    PayrollId = query.PayrollId,
                    RegulationDate = query.RegulationDate.Value.ToUtc(),
                    EvaluationDate = query.EvaluationDate.Value.ToUtc()
                },
                collectorNames: collectorNames,
                overrideType: overrideType,
                clusterSet: querySetup.ClusterSet);

            // select the most derived collector by name
            var collectorByNames = collectors.ToLookup(col => col.Name, wt => wt);
            var collectorMap = new CollectorMap();
            var derivedCollectors = new List<ApiObject.Collector>();
            foreach (var collectorByName in collectorByNames)
            {
                derivedCollectors.Add(collectorMap.ToApi(collectorByName.First()));
            }

            return derivedCollectors.ToArray();
        }
        catch (Exception exception)
        {
            return InternalServerError(exception);
        }
    }

    protected virtual async Task<ActionResult<ApiObject.Lookup[]>> GetPayrollLookupsAsync(
        DomainObject.PayrollQuery query,
        string[] lookupNames, OverrideType? overrideType)
    {
        try
        {
            // tenant
            var tenantResult = VerifyTenant(query.TenantId);
            if (tenantResult != null)
            {
                return tenantResult;
            }

            // query setup
            var setupQuery = await SetupQuery(query);
            if (setupQuery.Item2 != null)
            {
                // invalid setup response
                return setupQuery.Item2;
            }

            // wage types
            var collectMoment = CurrentEvaluationDate;
            query.RegulationDate ??= collectMoment;
            query.EvaluationDate ??= collectMoment;
            overrideType ??= OverrideType.Active;
            var lookups = (await Service.GetDerivedLookupsAsync(Runtime.DbContext,
                new()
                {
                    TenantId = query.TenantId,
                    PayrollId = query.PayrollId,
                    RegulationDate = query.RegulationDate.Value.ToUtc(),
                    EvaluationDate = query.EvaluationDate.Value.ToUtc()
                },
                lookupNames: lookupNames,
                overrideType: overrideType)).ToList();

            // select the most derived wage type by wage type number
            var lookupByNumbers = lookups.ToLookup(l => l.Name, l => l);
            var lookupMap = new LookupMap();
            var derivedLookups = new List<ApiObject.Lookup>();
            foreach (var lookupByNumber in lookupByNumbers)
            {
                derivedLookups.Add(lookupMap.ToApi(lookupByNumber.First()));
            }

            return derivedLookups.ToArray();
        }
        catch (Exception exception)
        {
            return InternalServerError(exception);
        }
    }

    protected virtual async Task<ActionResult<ApiObject.LookupValue[]>> GetPayrollLookupValuesAsync(
        DomainObject.PayrollQuery query, string[] lookupNames, string[] lookupKeys)
    {
        try
        {
            // tenant
            var tenantResult = VerifyTenant(query.TenantId);
            if (tenantResult != null)
            {
                return tenantResult;
            }

            // query setup
            var setupQuery = await SetupQuery(query);
            if (setupQuery.Item2 != null)
            {
                // invalid setup response
                return setupQuery.Item2;
            }

            // reports
            var collectMoment = CurrentEvaluationDate;
            query.RegulationDate ??= collectMoment;
            query.EvaluationDate ??= collectMoment;
            var lookupValues = await Service.GetDerivedLookupValuesAsync(Runtime.DbContext,
                new()
                {
                    TenantId = query.TenantId,
                    PayrollId = query.PayrollId,
                    RegulationDate = query.RegulationDate.Value.ToUtc(),
                    EvaluationDate = query.EvaluationDate.Value.ToUtc()
                },
                lookupNames: lookupNames,
                lookupKeys: lookupKeys);

            return new LookupValueMap().ToApi(lookupValues);
        }
        catch (Exception exception)
        {
            return InternalServerError(exception);
        }
    }

    protected virtual async Task<ActionResult<IEnumerable<ApiObject.LookupData>>> GetPayrollLookupDataAsync(
        DomainObject.PayrollQuery query, string[] lookupNames, Language? language)
    {
        if (lookupNames == null || !lookupNames.Any())
        {
            return BadRequest("Missing lookup names");
        }

        try
        {
            // tenant
            var tenantResult = VerifyTenant(query.TenantId);
            if (tenantResult != null)
            {
                return tenantResult;
            }

            // query setup
            var setupQuery = await SetupQuery(query);
            if (setupQuery.Item2 != null)
            {
                // invalid setup response
                return setupQuery.Item2;
            }

            query.RegulationDate ??= CurrentEvaluationDate;
            query.EvaluationDate ??= CurrentEvaluationDate;

            // lookup values
            var lookupValues = new List<ApiObject.LookupData>();
            var lookupDataMap = new LookupDataMap();
            foreach (var lookupName in lookupNames)
            {
                // lookups
                var lookups = (await Service.GetDerivedLookupsAsync(Runtime.DbContext,
                    new()
                    {
                        TenantId = query.TenantId,
                        PayrollId = query.PayrollId,
                        RegulationDate = query.RegulationDate.Value.ToUtc(),
                        EvaluationDate = query.EvaluationDate.Value.ToUtc()
                    },
                    new[] { lookupName }, OverrideType.Active)).ToList();
                if (!lookups.Any())
                {
                    return BadRequest($"Unknown lookup {lookupName}");
                }

                // compose override lookup, setup lookup values by base lookup until the topmost lookup
                DomainObject.LookupData lookupData = null;
                for (var i = lookups.Count - 1; i >= 0; i--)
                {
                    var lookup = lookups[i];
                    var regulationId = await RegulationLookupSetService.Repository.GetParentIdAsync(Runtime.DbContext, lookup.Id);
                    if (!regulationId.HasValue)
                    {
                        return BadRequest($"Unknown regulation for lookup {lookupName}");
                    }

                    var currentLookupData = await RegulationLookupSetService.Repository.GetLookupDataAsync(
                        Runtime.DbContext, query.TenantId, regulationId.Value, lookup.Id, language);

                    // first/base regulation
                    if (lookupData == null)
                    {
                        lookupData = currentLookupData;
                        continue;
                    }

                    // override regulation lookup values
                    foreach (var lookupValue in currentLookupData.Values)
                    {
                        // remove for replacement
                        var existingValue = lookupData.Values.FirstOrDefault(x => string.Equals(x.Key, lookupValue.Key));
                        if (existingValue != null)
                        {
                            lookupData.Values.Remove(existingValue);
                        }

                        // add lookup value
                        lookupData.Values.Add(lookupValue);
                    }
                }

                lookupValues.Add(lookupDataMap.ToApi(lookupData));
            }
            return lookupValues;
        }
        catch (Exception exception)
        {
            return InternalServerError(exception);
        }
    }

    protected virtual async Task<ActionResult<ApiObject.LookupValueData>> GetPayrollLookupValueDataAsync(
        DomainObject.PayrollQuery query,
        string lookupName, string lookupKey, decimal? rangeValue, Language? language)
    {
        // verify tenant
        var tenantResult = VerifyTenant(query.TenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        // verify lookup key, optional with range value
        if (!rangeValue.HasValue && string.IsNullOrWhiteSpace(lookupKey))
        {
            return BadRequest("Missing lookup key");
        }

        // query setup
        var setupQuery = await SetupQuery(query);
        if (setupQuery.Item2 != null)
        {
            // invalid setup response
            return setupQuery.Item2;
        }
        var querySetup = setupQuery.Item1;

        // lookups
        var lookupProvider = await this.NewRegulationLookupProviderAsync(Runtime.DbContext, querySetup.Tenant,
            querySetup.Payroll, query.RegulationDate, query.EvaluationDate);

        // range value request
        if (rangeValue.HasValue)
        {
            var rangeValueData = await lookupProvider.
                GetRangeLookupValueDataAsync(Runtime.DbContext, lookupName, rangeValue.Value, lookupKey, language);
            return new LookupValueDataMap().ToApi(rangeValueData);
        }

        // simple value request
        var valueData = await lookupProvider.GetLookupValueDataAsync(Runtime.DbContext, lookupName, lookupKey, language);
        return new LookupValueDataMap().ToApi(valueData);
    }

    protected virtual async Task<ActionResult<ApiObject.Report[]>> GetPayrollReportsAsync(
        DomainObject.PayrollQuery query,
        string[] reportNames, OverrideType? overrideType, string clusterSetName)
    {
        try
        {
            // tenant
            var tenantResult = VerifyTenant(query.TenantId);
            if (tenantResult != null)
            {
                return tenantResult;
            }

            // query setup
            var setupQuery = await SetupQuery(query, clusterSetName: clusterSetName);
            if (setupQuery.Item2 != null)
            {
                // invalid setup response
                return setupQuery.Item2;
            }
            var querySetup = setupQuery.Item1;

            // reports
            var collectMoment = CurrentEvaluationDate;
            query.RegulationDate ??= collectMoment;
            query.EvaluationDate ??= collectMoment;
            overrideType ??= OverrideType.Active;
            var reports = await Service.GetDerivedReportsAsync(Runtime.DbContext,
                new()
                {
                    TenantId = query.TenantId,
                    PayrollId = query.PayrollId,
                    RegulationDate = query.RegulationDate.Value.ToUtc(),
                    EvaluationDate = query.EvaluationDate.Value.ToUtc()
                },
                reportNames: reportNames,
                overrideType: overrideType,
                clusterSet: querySetup.ClusterSet);

            return new ReportMap().ToApi(reports);
        }
        catch (Exception exception)
        {
            return InternalServerError(exception);
        }
    }

    protected virtual async Task<ActionResult<ApiObject.ReportParameter[]>> GetPayrollReportParametersAsync(
        DomainObject.PayrollQuery query, string[] reportNames)
    {
        try
        {
            // tenant
            var tenantResult = VerifyTenant(query.TenantId);
            if (tenantResult != null)
            {
                return tenantResult;
            }

            // query setup
            var setupQuery = await SetupQuery(query);
            if (setupQuery.Item2 != null)
            {
                // invalid setup response
                return setupQuery.Item2;
            }

            // reports
            var collectMoment = CurrentEvaluationDate;
            query.RegulationDate ??= collectMoment;
            query.EvaluationDate ??= collectMoment;
            var reportParameters = await Service.GetDerivedReportParametersAsync(Runtime.DbContext,
                new()
                {
                    TenantId = query.TenantId,
                    PayrollId = query.PayrollId,
                    RegulationDate = query.RegulationDate.Value.ToUtc(),
                    EvaluationDate = query.EvaluationDate.Value.ToUtc()
                },
                reportNames: reportNames);

            return new ReportParameterMap().ToApi(reportParameters);
        }
        catch (Exception exception)
        {
            return InternalServerError(exception);
        }
    }

    protected virtual async Task<ActionResult<ApiObject.ReportTemplate[]>> GetPayrollReportTemplatesAsync(
        DomainObject.PayrollQuery query, string[] reportNames = null, Language? language = null)
    {
        try
        {
            // tenant
            var tenantResult = VerifyTenant(query.TenantId);
            if (tenantResult != null)
            {
                return tenantResult;
            }

            // query setup
            var setupQuery = await SetupQuery(query);
            if (setupQuery.Item2 != null)
            {
                // invalid setup response
                return setupQuery.Item2;
            }

            // reports
            var collectMoment = CurrentEvaluationDate;
            query.RegulationDate ??= collectMoment;
            query.EvaluationDate ??= collectMoment;
            var reportTemplate = await Service.GetDerivedReportTemplateAsync(Runtime.DbContext,
                new()
                {
                    TenantId = query.TenantId,
                    PayrollId = query.PayrollId,
                    RegulationDate = query.RegulationDate.Value.ToUtc(),
                    EvaluationDate = query.EvaluationDate.Value.ToUtc()
                },
                reportNames: reportNames,
                language: language);

            return new ReportTemplateMap().ToApi(reportTemplate);
        }
        catch (Exception exception)
        {
            return InternalServerError(exception);
        }
    }

    protected virtual async Task<ActionResult<ApiObject.Script[]>> GetPayrollScriptAsync(
        DomainObject.PayrollQuery query,
        string[] scriptNames, OverrideType? overrideType)
    {
        try
        {
            // tenant
            var tenantResult = VerifyTenant(query.TenantId);
            if (tenantResult != null)
            {
                return tenantResult;
            }

            // query setup
            var setupQuery = await SetupQuery(query);
            if (setupQuery.Item2 != null)
            {
                // invalid setup response
                return setupQuery.Item2;
            }

            // scripts
            var collectMoment = CurrentEvaluationDate;
            query.RegulationDate ??= collectMoment;
            query.EvaluationDate ??= collectMoment;
            overrideType ??= OverrideType.Active;
            var scripts = await Service.GetDerivedScriptsAsync(Runtime.DbContext,
                new()
                {
                    TenantId = query.TenantId,
                    PayrollId = query.PayrollId,
                    RegulationDate = query.RegulationDate.Value.ToUtc(),
                    EvaluationDate = query.EvaluationDate.Value.ToUtc()
                },
                scriptNames: scriptNames,
                overrideType: overrideType);

            return new ScriptMap().ToApi(scripts);
        }
        catch (Exception exception)
        {
            return InternalServerError(exception);
        }
    }

    protected virtual async Task<ActionResult<ApiObject.ActionInfo[]>> GetPayrollScriptActionsAsync(
        DomainObject.PayrollQuery query,
        string[] scriptNames, OverrideType? overrideType, FunctionType functionType = FunctionType.All)
    {
        try
        {
            // tenant
            var tenantResult = VerifyTenant(query.TenantId);
            if (tenantResult != null)
            {
                return tenantResult;
            }

            // query setup
            var setupQuery = await SetupQuery(query);
            if (setupQuery.Item2 != null)
            {
                // invalid setup response
                return setupQuery.Item2;
            }

            var apiActionInfos = new List<ApiObject.ActionInfo>();

            // scripts
            var collectMoment = CurrentEvaluationDate;
            query.RegulationDate ??= collectMoment;
            query.EvaluationDate ??= collectMoment;
            overrideType ??= OverrideType.Active;
            var actions = await Service.GetDerivedScriptActionsAsync(Runtime.DbContext,
                new()
                {
                    TenantId = query.TenantId,
                    PayrollId = query.PayrollId,
                    RegulationDate = query.RegulationDate.Value.ToUtc(),
                    EvaluationDate = query.EvaluationDate.Value.ToUtc()
                },
                scriptNames: scriptNames,
                overrideType: overrideType);

            foreach (var action in actions)
            {
                apiActionInfos.Add(new ActionInfoMap().ToApi(action));
            }

            return apiActionInfos.ToArray();
        }
        catch (Exception exception)
        {
            return InternalServerError(exception);
        }
    }

    #endregion

    #region Query

    private sealed class QuerySetup
    {
        internal DomainObject.Tenant Tenant { get; set; }
        internal DomainObject.Payroll Payroll { get; set; }
        internal DomainObject.User User { get; set; }
        internal DomainObject.ClusterSet ClusterSet { get; set; }
    }

    private async Task<Tuple<QuerySetup, ActionResult>> SetupQuery(DomainObject.PayrollQuery query, int? userId = null, string clusterSetName = null)
    {
        var querySetup = new QuerySetup();
        try
        {
            // tenant (mandatory)
            if (query.TenantId <= 0)
            {
                return new(querySetup, BadRequest($"Invalid query tenant id {query.TenantId}"));
            }
            var tenantResult = VerifyTenant(query.TenantId);
            if (tenantResult != null)
            {
                return new(querySetup, tenantResult);
            }
            var tenant = await ParentService.GetAsync(Runtime.DbContext, query.TenantId);
            if (tenant == null)
            {
                return new(querySetup, BadRequest($"Unknown tenant with id {query.TenantId}"));
            }
            querySetup.Tenant = tenant;

            // payroll (mandatory)
            if (query.PayrollId <= 0)
            {
                return new(querySetup, BadRequest($"Invalid query payroll id: {query.PayrollId}"));
            }
            var payroll = await Service.GetAsync(Runtime.DbContext, query.TenantId, query.PayrollId);
            if (payroll == null)
            {
                return new(querySetup, BadRequest($"Unknown payroll with id {query.PayrollId}"));
            }
            querySetup.Payroll = payroll;

            // user (optional)
            if (userId.HasValue)
            {
                if (userId.Value <= 0)
                {
                    return new(querySetup, BadRequest($"Invalid query user id {userId.Value}"));
                }
                var user = await UserService.GetAsync(Runtime.DbContext, query.TenantId, userId.Value);
                if (user == null)
                {
                    return new(querySetup, BadRequest($"Unknown user with id {userId}"));
                }
                querySetup.User = user;
            }

            // cluster set (optional)
            if (!string.IsNullOrWhiteSpace(clusterSetName))
            {
                var clusterSet = payroll.GetClusterSet(clusterSetName);
                if (clusterSet == null)
                {
                    return new(querySetup, BadRequest($"Unknown cluster set {clusterSetName}"));
                }
                querySetup.ClusterSet = clusterSet;
            }

            return new(querySetup, null);
        }
        catch (ScriptException exception)
        {
            return new(querySetup, UnprocessableEntity(exception.GetBaseException().ToString()));
        }
        catch (Exception exception)
        {
            return new(querySetup, InternalServerError(exception));
        }
    }

    #endregion

}