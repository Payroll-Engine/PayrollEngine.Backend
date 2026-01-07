using System;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Threading.Tasks;
using System.Collections.Generic;
using Task = System.Threading.Tasks.Task;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using PayrollEngine.Api.Map;
using PayrollEngine.Api.Core;
using PayrollEngine.Domain.Model;
using PayrollEngine.Client.Scripting;
using PayrollEngine.Domain.Application.Service;

namespace PayrollEngine.Api.Controller;

internal sealed class PayrollControllerCaseBuilder
{
    private IPayrollControllerServices Services { get; }
    private IPayrollContextService Context => Services.Context;

    internal PayrollControllerCaseBuilder(IPayrollControllerServices services)
    {
        Services = services ?? throw new ArgumentNullException(nameof(services));
    }

    internal async Task<ActionResult<Model.CaseChange>> AddPayrollCaseAsync(IDbContext context,
        int tenantId, int payrollId, Model.CaseChangeSetup caseChangeSetup, PathString path)
    {
        if (caseChangeSetup == null)
        {
            throw new ArgumentNullException(nameof(caseChangeSetup));
        }

        try
        {
            // tenant
            var tenant = await Context.TenantService.GetAsync(context, tenantId);
            if (tenant == null)
            {
                return ActionResultFactory.BadRequest($"Unknown tenant with id {tenantId}");
            }
            var culture = CultureInfo.DefaultThreadCurrentCulture ?? CultureInfo.InvariantCulture;

            // payroll
            var payroll = await Context.PayrollService.GetAsync(context, tenantId, payrollId);
            if (payroll == null)
            {
                return ActionResultFactory.BadRequest($"Unknown payroll with id {payrollId}");
            }

            // division
            var division = await Context.DivisionService.GetAsync(context, tenantId, payroll.DivisionId);
            if (division == null)
            {
                return ActionResultFactory.BadRequest($"Unknown payroll division with id {payroll.DivisionId}");
            }

            // user
            if (caseChangeSetup.UserId <= 0)
            {
                return ActionResultFactory.BadRequest($"Invalid user id {caseChangeSetup.UserId}");
            }

            var user = await Context.UserService.GetAsync(context, tenantId, caseChangeSetup.UserId);
            if (user == null)
            {
                return ActionResultFactory.BadRequest($"Unknown user with id {caseChangeSetup.UserId}");
            }

            // change reason
            if (string.IsNullOrWhiteSpace(caseChangeSetup.Reason))
            {
                caseChangeSetup.Reason = $"Case change from {user.Identifier} at {Date.Now}";
            }

            // root case setup
            if (caseChangeSetup.Case == null)
            {
                return ActionResultFactory.BadRequest("Case change setup without values");
            }

            var domainCaseChangeSetup = await ConvertCaseChangeAsync(context, tenantId, payrollId, caseChangeSetup);

            // validate case change
            CaseType? caseType = null;
            CaseCancellationType? cancellationType = null;
            Employee employee = null;
            var caseSetups = domainCaseChangeSetup.CollectCaseSetups();
            foreach (var caseSetup in caseSetups)
            {
                // case
                var caseName = caseSetup.CaseName;
                if (string.IsNullOrWhiteSpace(caseName))
                {
                    return ActionResultFactory.BadRequest($"Missing case name in case setup {caseSetup}");
                }

                var derivedCases = await GetDerivedCaseAsync(context, tenantId, payrollId, caseName);
                if (!derivedCases.Any())
                {
                    return ActionResultFactory.BadRequest($"Unknown case {caseName}");
                }

                var @case = derivedCases.First();

                // case cancellation
                if (domainCaseChangeSetup.CancellationId.HasValue &&
                    @case.CancellationType == CaseCancellationType.None)
                {
                    return ActionResultFactory.BadRequest($"Case {@case.Name} does not support cancellation");
                }

                // case type from root case
                caseType ??= @case.CaseType;
                cancellationType ??= @case.CancellationType;
                if (caseType != @case.CaseType)
                {
                    return ActionResultFactory.BadRequest(
                        $"Only one case type allowed within the case change ({caseType}/{@case.CaseType})");
                }

                // employee
                if (@case.CaseType == CaseType.Employee)
                {
                    if (!domainCaseChangeSetup.EmployeeId.HasValue)
                    {
                        return ActionResultFactory.BadRequest("Missing employee id on employee cases");
                    }

                    employee = await Context.EmployeeService.GetAsync(context, tenantId, domainCaseChangeSetup.EmployeeId.Value);
                    // case change
                    if (employee == null)
                    {
                        return ActionResultFactory.BadRequest(
                            $"Missing employee with id {domainCaseChangeSetup.EmployeeId}");
                    }
                }

                if (caseSetup.Values != null)
                {
                    foreach (var caseValue in caseSetup.Values)
                    {
                        // start/end test
                        if (!caseValue.Start.HasValue && caseValue.End.HasValue)
                        {
                            return ActionResultFactory.BadRequest($"Case field {caseValue.CaseFieldName} without start date");
                        }
                        if (caseValue.Start.HasValue && caseValue.End.HasValue &&
                            caseValue.End < caseValue.Start)
                        {
                            return ActionResultFactory.BadRequest($"Case field {caseValue.CaseFieldName} period end date before start date");
                        }

                        // case field available test
                        var caseField = (await GetDerivedCaseFieldAsync(context, tenantId, payrollId, caseValue.CaseFieldName)).FirstOrDefault();
                        if (caseField == null)
                        {
                            return ActionResultFactory.BadRequest($"Unknown case field {caseValue.CaseFieldName}");
                        }

                        // case value access test
                        if (!ValueConvert.TryToValue(caseValue.Value, caseField.ValueType, culture, out _))
                        {
                            return ActionResultFactory.BadRequest(
                                $"Invalid value for case field {caseValue.CaseFieldName} ({caseField.ValueType}): {caseValue.Value}");
                        }
                    }
                }
            }

            if (!caseType.HasValue)
            {
                return ActionResultFactory.BadRequest("Unknown case type");
            }

            // case change
            CaseChange domainCaseChange;

            // cancellation case
            if (domainCaseChangeSetup.CancellationId.HasValue)
            {
                if (caseType.Value == CaseType.Employee && !domainCaseChangeSetup.EmployeeId.HasValue)
                {
                    return ActionResultFactory.BadRequest("Missing employee id on employee cases");
                }

                // case change
                var caseChange = await GetCancellationCaseChangeAsync(
                    context, tenantId, caseType.Value, domainCaseChangeSetup);
                if (caseChange == null)
                {
                    return ActionResultFactory.NotFound(
                        $"Missing cancellation case {domainCaseChangeSetup.Case.CaseName}");
                }

                if (!string.Equals(caseChange.ValidationCaseName, domainCaseChangeSetup.Case.CaseName))
                {
                    return ActionResultFactory.NotFound(
                        $"Invalid cancellation case {caseChange.ValidationCaseName}, expected {domainCaseChangeSetup.Case.CaseName}");
                }

                if (caseChange.Values == null || !caseChange.Values.Any())
                {
                    return ActionResultFactory.NotFound(
                        $"Cancellation case {domainCaseChangeSetup.Case.CaseName} without case values");
                }

                if (caseType.Value == CaseType.Employee && !domainCaseChangeSetup.EmployeeId.HasValue)
                {
                    return ActionResultFactory.BadRequest("Missing employee id on employee cases");
                }

                // prevent double case cancellation
                var cancellationQuery =
                    QueryFactory.NewEqualFilterQuery(nameof(CaseChange.CancellationId), caseChange.Id);
                var cancellationCaseChange = await GetCancellationCaseChangeAsync(context, tenantId, domainCaseChangeSetup,
                    caseType.Value, cancellationQuery);
                if (cancellationCaseChange != null)
                {
                    return ActionResultFactory.BadRequest(
                        $"Case change with id {domainCaseChangeSetup.CancellationId.Value} is already cancelled");
                }

                // cancel case values
                caseChange.CancellationDate = Date.Now;
                foreach (var caseValue in caseChange.Values)
                {
                    // cancel case value
                    caseValue.CancellationDate = caseChange.CancellationDate;

                    // case field
                    var caseField = (await GetDerivedCaseFieldAsync(context, tenantId, payrollId, caseValue.CaseFieldName)).FirstOrDefault();
                    if (caseField == null)
                    {
                        return ActionResultFactory.BadRequest($"Unknown case field {caseValue.CaseFieldName}");
                    }

                    // case value
                    var cancellationMode = GetCaseFieldCancellationMode(caseField);
                    caseValue.Value = await GetCancellationCaseValueAsync(tenantId, payrollId,
                        domainCaseChangeSetup, caseValue, cancellationMode, caseType.Value, culture);

                    // ensure newer case value using the next second as creation date
                    caseValue.Created = GetCancellationDate(caseValue.Created);
                    caseValue.Updated = GetCancellationDate(caseValue.Updated);

                    // add cancelled value to the case change setup
                    domainCaseChangeSetup.Case.Values.Add(new(caseValue));
                }

                // validation case
                var validationCase = (await GetDerivedCaseAsync(context, tenantId, payrollId, domainCaseChangeSetup.Case.CaseName))
                    .FirstOrDefault();
                if (validationCase == null)
                {
                    return ActionResultFactory.BadRequest(
                        $"Unknown validation case {domainCaseChangeSetup.Case.CaseName}");
                }

                var issues = await Services.ValidateCaseAsync(
                    new()
                    {
                        Tenant = tenant,
                        Payroll = payroll,
                        Division = division,
                        User = user,
                        Employee = employee,
                        ValidationCase = validationCase,
                        CaseType = caseType.Value,
                        DomainCaseChangeSetup = domainCaseChangeSetup,
                        CancellationDate = caseChange.CancellationDate
                    });

                // issues
                if (issues != null && issues.Any())
                {
                    // don't add case with issues
                    return ActionResultFactory.BadRequest(GetIssuesMessage(issues));
                }

                // cancellation case change
                domainCaseChange = new(caseChange)
                {
                    UserId = domainCaseChangeSetup.UserId,
                    EmployeeId = domainCaseChangeSetup.EmployeeId,
                    DivisionId = domainCaseChangeSetup.DivisionId,
                    Reason = $"Cancel case change from {user.Identifier} created at {caseChange.Created}",
                    Forecast = domainCaseChangeSetup.Forecast,
                    Values = domainCaseChangeSetup.CollectCaseValues(),
                    // cancellation case change is not cancelable
                    CancellationType = CaseCancellationType.None,
                    CancellationId = domainCaseChangeSetup.CancellationId.Value,
                    Created = GetCancellationDate(caseChange.Created),
                    Updated = GetCancellationDate(caseChange.Updated)
                };
            }
            else
            {
                // new case
                // validation case
                var validationCase = (await GetDerivedCaseAsync(context, tenantId, payrollId, domainCaseChangeSetup.Case.CaseName))
                    .FirstOrDefault();
                if (validationCase == null)
                {
                    return ActionResultFactory.BadRequest(
                        $"Unknown validation case {domainCaseChangeSetup.Case.CaseName}");
                }

                var issues = await Services.ValidateCaseAsync(
                    new()
                    {
                        Tenant = tenant,
                        Payroll = payroll,
                        Division = division,
                        User = user,
                        Employee = employee,
                        ValidationCase = validationCase,
                        CaseType = caseType.Value,
                        DomainCaseChangeSetup = domainCaseChangeSetup
                    });

                // issues
                if (issues != null && issues.Any())
                {
                    // don't add case with issues
                    return ActionResultFactory.BadRequest(GetIssuesMessage(issues));
                }

                // new case change
                domainCaseChange = new()
                {
                    UserId = domainCaseChangeSetup.UserId,
                    EmployeeId = domainCaseChangeSetup.EmployeeId,
                    DivisionId = domainCaseChangeSetup.DivisionId,
                    Reason = domainCaseChangeSetup.Reason,
                    ValidationCaseName = domainCaseChangeSetup.Case.CaseName,
                    Forecast = domainCaseChangeSetup.Forecast,
                    Values = domainCaseChangeSetup.CollectCaseValues(),
                    CancellationType = (CaseCancellationType)cancellationType
                };
                if (domainCaseChangeSetup.Created.HasValue)
                {
                    domainCaseChange.Created = domainCaseChangeSetup.Created.Value;
                    domainCaseChange.Updated = domainCaseChangeSetup.Created.Value;
                }
            }

            // create case change
            domainCaseChange = await AddCaseChangeAsync(context, tenantId, domainCaseChange.UserId,
                payrollId, caseType.Value, employee, domainCaseChange);
            var caseChangeMap = new CaseChangeMap<CaseChange, Model.CaseChange>();

            // case change result
            var resultCaseChange = caseChangeMap.ToApi(domainCaseChange);
            if (resultCaseChange == null)
            {
                return ActionResultFactory.BadRequest("Case change not accepted");
            }

            // case change without any change
            if (!resultCaseChange.Values.Any())
            {
                return ActionResultFactory.Ok(resultCaseChange);
            }

            // created resource
            return new CreatedObjectResult(path, resultCaseChange);
        }
        catch (ScriptException exception)
        {
            return new UnprocessableEntityObjectResult(exception.GetBaseException().ToString());
        }
        catch (PersistenceException exception)
        {
            return exception.ErrorType == PersistenceErrorType.UniqueConstraint ?
                new UnprocessableEntityObjectResult(exception.Message) :
                new UnprocessableEntityObjectResult(exception.GetBaseMessage());
        }
        catch (Exception exception)
        {
            return ActionResultFactory.InternalServerError(exception);
        }
    }

    private async Task<CaseChangeSetup> ConvertCaseChangeAsync(IDbContext context, int tenantId,
        int payrollId, Model.CaseChangeSetup caseChangeSetup)
    {
        var caseChange = new CaseChangeSetupMap().ToDomain(caseChangeSetup);
        var regulations = (await Context.PayrollService.GetDerivedRegulationsAsync(context,
            new()
            {
                TenantId = tenantId,
                PayrollId = payrollId
            })).ToList();

        await ApplyCaseNamespaceAsync(context, tenantId, caseChange.Case, regulations);
        return caseChange;
    }

    private async Task ApplyCaseNamespaceAsync(IDbContext context, int tenantId,
        CaseSetup caseSetup, List<Regulation> regulations)
    {
        // case name
        caseSetup.CaseName = await ApplyCaseNamespaceAsync(context, tenantId, caseSetup.CaseName, regulations);

        // case values
        foreach (var caseValue in caseSetup.Values)
        {
            caseValue.CaseName = caseSetup.CaseName;
            caseValue.CaseFieldName = await ApplyCaseFieldNamespaceAsync(context, tenantId, caseValue.CaseFieldName, regulations);
        }

        // related cases
        foreach (var relatedCase in caseSetup.RelatedCases)
        {
            await ApplyCaseNamespaceAsync(context, tenantId, relatedCase, regulations);
        }
    }

    private async Task<string> ApplyCaseNamespaceAsync(IDbContext context, int tenantId,
        string caseName, IEnumerable<Regulation> regulations)
    {
        foreach (var regulation in regulations)
        {
            var regulationCaseName = caseName.EnsureNamespace(regulation.Namespace);
            var @case = await Context.CaseService.GetAsync(context, tenantId, regulation.Id, regulationCaseName);
            if (@case != null)
            {
                return regulationCaseName;
            }
        }
        throw new PayrollException($"Unknown case {caseName}");
    }

    private async Task<string> ApplyCaseFieldNamespaceAsync(IDbContext context, int tenantId,
        string caseFieldName, List<Regulation> regulations)
    {
        foreach (var regulation in regulations)
        {
            var regulationCaseName = caseFieldName.EnsureNamespace(regulation.Namespace);
            var regulationCases = await Context.CaseFieldService.GetRegulationCaseFieldsAsync(context, tenantId, [regulationCaseName]);
            if (regulationCases != null)
            {
                return regulationCaseName;
            }
        }
        throw new PayrollException($"Unknown case field {caseFieldName}");
    }

    private static string GetIssuesMessage(List<CaseValidationIssue> issues)
    {
        var buffer = new StringBuilder();
        foreach (var issue in issues)
        {
            // following lines
            if (buffer.Length > 0)
            {
                buffer.AppendLine();
            }
            buffer.Append(issue.Message);
        }
        return buffer.ToString();
    }

    private async Task<string> GetCancellationCaseValueAsync(int tenantId, int payrollId,
        CaseChangeSetup caseChangeSetup, Domain.Model.CaseValue caseValue,
        CaseFieldCancellationMode cancellationMode, CaseType caseType, CultureInfo culture)
    {
        var cancellationCaseValue = caseValue.Value;
        switch (cancellationMode)
        {
            case CaseFieldCancellationMode.Previous:
                cancellationCaseValue = await GetPreviousCaseValueAsync(tenantId, payrollId,
                    caseChangeSetup.EmployeeId, caseValue, caseType, caseValue.ValueType);
                break;
            case CaseFieldCancellationMode.Reset:
                cancellationCaseValue = ResetCaseValue(caseValue.Value, caseValue.ValueType);
                break;
            case CaseFieldCancellationMode.Invert:
                cancellationCaseValue = InvertCaseValue(caseValue.Value, caseValue.ValueType, culture);
                break;
            case CaseFieldCancellationMode.Keep:
                break;
        }

        return cancellationCaseValue;
    }

    private async Task<CaseChange> GetCancellationCaseChangeAsync(IDbContext context, int tenantId,
        CaseChangeSetup caseChangeSetup, CaseType caseType, Query cancellationQuery)
    {
        CaseChange cancellationCaseChange = null;
        switch (caseType)
        {
            case CaseType.Global:
                cancellationCaseChange =
                    (await Context.GlobalChangeService.QueryAsync(context, tenantId, tenantId, cancellationQuery))
                    .FirstOrDefault();
                break;
            case CaseType.National:
                cancellationCaseChange =
                    (await Context.NationalChangeService.QueryAsync(context, tenantId, tenantId, cancellationQuery))
                    .FirstOrDefault();
                break;
            case CaseType.Company:
                cancellationCaseChange =
                    (await Context.CompanyChangeService.QueryAsync(context, tenantId, tenantId, cancellationQuery))
                    .FirstOrDefault();
                break;
            case CaseType.Employee:
                if (caseChangeSetup.EmployeeId.HasValue)
                {
                    cancellationCaseChange =
                        (await Context.EmployeeChangeService.QueryAsync(context, tenantId, caseChangeSetup.EmployeeId.Value,
                            cancellationQuery))
                        .FirstOrDefault();
                }

                break;
        }

        return cancellationCaseChange;
    }

    private static CaseFieldCancellationMode GetCaseFieldCancellationMode(CaseField caseField)
    {
        var cancellationMode = caseField.CancellationMode;
        if (cancellationMode == CaseFieldCancellationMode.TimeType)
        {
            switch (caseField.TimeType)
            {
                case CaseFieldTimeType.Timeless:
                case CaseFieldTimeType.Period:
                case CaseFieldTimeType.CalendarPeriod:
                    cancellationMode = CaseFieldCancellationMode.Previous;
                    break;
                case CaseFieldTimeType.Moment:
                    cancellationMode = CaseFieldCancellationMode.Reset;
                    break;
            }
        }

        return cancellationMode;
    }

    private async Task<CaseChange> GetCancellationCaseChangeAsync(IDbContext context,
        int tenantId, CaseType caseType, CaseChangeSetup caseChangeSetup)
    {
        if (!caseChangeSetup.CancellationId.HasValue)
        {
            throw new ArgumentException(nameof(caseChangeSetup));
        }

        CaseChange caseChange = null;
        var query = QueryFactory.NewIdQuery(caseChangeSetup.CancellationId.Value);
        switch (caseType)
        {
            case CaseType.Global:
                caseChange = (await Context.GlobalChangeService.QueryAsync(context, tenantId, tenantId, query))
                    .FirstOrDefault();
                break;
            case CaseType.National:
                caseChange = (await Context.NationalChangeService.QueryAsync(context, tenantId, tenantId, query))
                    .FirstOrDefault();
                break;
            case CaseType.Company:
                caseChange = (await Context.CompanyChangeService.QueryAsync(context, tenantId, tenantId, query))
                    .FirstOrDefault();
                break;
            case CaseType.Employee:
                if (!caseChangeSetup.EmployeeId.HasValue)
                {
                    throw new ArgumentException(nameof(caseChangeSetup));
                }

                caseChange =
                    (await Context.EmployeeChangeService.QueryAsync(context, tenantId, caseChangeSetup.EmployeeId.Value,
                        query))
                    .FirstOrDefault();
                break;
        }

        return caseChange;
    }

    private async Task<CaseChange> AddCaseChangeAsync(IDbContext context, int tenantId, int userId, int payrollId,
        CaseType caseType, Employee employee, CaseChange caseChange)
    {
        switch (caseType)
        {
            case CaseType.Global:
                caseChange = await Context.GlobalChangeService.AddCaseChangeAsync(context, tenantId, userId, payrollId,
                    tenantId, caseChange);
                break;
            case CaseType.National:
                caseChange = await Context.NationalChangeService.AddCaseChangeAsync(context, tenantId, userId, payrollId,
                    tenantId, caseChange);
                break;
            case CaseType.Company:
                caseChange = await Context.CompanyChangeService.AddCaseChangeAsync(context, tenantId, userId, payrollId,
                    tenantId, caseChange);
                break;
            case CaseType.Employee:
                if (employee != null)
                {
                    caseChange = await Context.EmployeeChangeService.AddCaseChangeAsync(context, tenantId,
                        userId, payrollId, employee.Id, caseChange);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        return caseChange;
    }

    private static DateTime GetCancellationDate(DateTime dateTime) =>
        dateTime.AddSeconds(1);

    private static string ResetCaseValue(string inputValue, ValueType valueType)
    {
        if (valueType.IsInteger())
        {
            return ValueConvert.ToJson(0);
        }
        if (valueType.IsDecimal())
        {
            return ValueConvert.ToJson(0M);
        }
        if (valueType.IsBoolean())
        {
            return ValueConvert.ToJson(false);
        }
        return inputValue;
    }

    private static string InvertCaseValue(string inputValue, ValueType valueType, CultureInfo culture) =>
        ValueConvert.InvertValue(inputValue, valueType, culture);

    private async Task<List<Case>> GetDerivedCaseAsync(IDbContext context, int tenantId, int payrollId, string caseName)
    {
        var cases = (await Context.PayrollService.GetDerivedCasesAsync(context,
            new() { TenantId = tenantId, PayrollId = payrollId },
            caseNames: [caseName])).ToList();
        return cases;
    }

    private async Task<List<ChildCaseField>> GetDerivedCaseFieldAsync(IDbContext context, int tenantId, int payrollId, string caseFieldName)
    {
        var caseFields = (await Context.PayrollService.GetDerivedCaseFieldsAsync(context,
            new() { TenantId = tenantId, PayrollId = payrollId },
            [caseFieldName])).ToList();
        return caseFields;
    }

    private async Task<string> GetPreviousCaseValueAsync(int tenantId, int payrollId, int? employeeId,
        Domain.Model.CaseValue caseValue, CaseType caseType, ValueType valueType)
    {
        // get case value before the case value was created
        var caseValues = await Services.GetPayrollTimeCaseValuesAsync(
            new()
            {
                TenantId = tenantId,
                PayrollId = payrollId,
                EmployeeId = employeeId
            },
            caseType,
            [caseValue.CaseFieldName],
            caseValue.Created);
        if (caseValues == null)
        {
            return null;
        }

        var previousCaseValue =
            caseValues.FirstOrDefault(x => string.Equals(x.CaseFieldName, caseValue.CaseFieldName));
        return previousCaseValue != null
            ? previousCaseValue.Value
            :
            // no previous value available
            ResetCaseValue(caseValue.Value, valueType);
    }
}