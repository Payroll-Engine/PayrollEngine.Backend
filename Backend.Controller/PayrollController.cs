using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Api.Core;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;
using ApiObject = PayrollEngine.Api.Model;

namespace PayrollEngine.Backend.Controller;

/// <inheritdoc/>
[ApiControllerName("Payrolls")]
[Route("api/tenants/{tenantId}/payrolls")]
[ApiExplorerSettings(IgnoreApi = ApiServiceIgnore.Payroll)]
public class PayrollController : Api.Controller.PayrollController
{
    /// <inheritdoc/>
    public PayrollController(IPayrollContextService context, IControllerRuntime runtime) :
        base(context, runtime)
    {
    }

    #region Payroll

    /// <summary>
    /// Query payrolls
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>The tenant payrolls</returns>
    [HttpGet]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("QueryPayrolls")]
    public async Task<ActionResult> QueryPayrollsAsync(int tenantId, [FromQuery] Query query)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await QueryItemsAsync(tenantId, query);
    }

    /// <summary>
    /// Get a payroll
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrollId">The payroll id</param>
    /// <returns></returns>
    [HttpGet("{payrollId}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetPayroll")]
    public async Task<ActionResult<ApiObject.Payroll>> GetPayrollAsync(int tenantId, int payrollId)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await GetAsync(tenantId, payrollId);
    }

    /// <summary>
    /// Add a new payroll
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payroll">The payroll to add</param>
    /// <returns>The newly created payroll</returns>
    [HttpPost]
    [CreatedResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("CreatePayroll")]
    public async Task<ActionResult<ApiObject.Payroll>> CreatePayrollAsync(int tenantId, ApiObject.Payroll payroll)
    {
        // cluster sets
        if (payroll.ClusterSets != null)
        {
            foreach (var clusterSet in payroll.ClusterSets)
            {
                if (clusterSet.IncludeClusters != null && clusterSet.ExcludeClusters != null
                                                       && clusterSet.IncludeClusters.Intersect(clusterSet.ExcludeClusters).Any())
                {
                    return BadRequest($"Invalid cluster set {clusterSet.Name}");
                }
            }
        }

        return await CreateAsync(tenantId, payroll);
    }

    /// <summary>
    /// Update a payroll
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payroll">The payroll with updated values</param>
    /// <returns>The modified payroll</returns>
    [HttpPut("{payrollId}")]
    [OkResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("UpdatePayroll")]
    public async Task<ActionResult<ApiObject.Payroll>> UpdatePayrollAsync(int tenantId, ApiObject.Payroll payroll)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await UpdateAsync(tenantId, payroll);
    }

    /// <summary>
    /// Delete a payroll
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrollId">The payroll id</param>
    /// <returns></returns>
    [HttpDelete("{payrollId}")]
    [ApiOperationId("DeletePayroll")]
    public async Task<IActionResult> DeletePayrollAsync(int tenantId, int payrollId)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await DeleteAsync(tenantId, payrollId);
    }

    #endregion

    #region Regulations

    /// <summary>
    /// Get payroll regulations
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrollId">The payroll id</param>
    /// <param name="regulationDate">The regulation date (default: UTC now)</param>
    /// <param name="evaluationDate">Creation date filter (default: UTC now)</param>
    /// <returns>The payroll regulations, including the shared regulations</returns>
    [HttpGet("{payrollId}/regulations")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetPayrollRegulations")]
    public async Task<ActionResult<IEnumerable<ApiObject.Regulation>>> GetPayrollRegulationsAsync(int tenantId,
        int payrollId, [FromQuery] DateTime? regulationDate, [FromQuery] DateTime? evaluationDate)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await base.GetPayrollRegulationsAsync(
            new()
            {
                TenantId = tenantId,
                PayrollId = payrollId,
                RegulationDate = regulationDate,
                EvaluationDate = evaluationDate
            });
    }

    #endregion

    #region Cases

    /// <summary>
    /// Get active and available cases
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrollId">The payroll id</param>
    /// <param name="query">The payroll case query</param>
    /// <returns>All payroll cases</returns>
    [HttpGet("{payrollId}/cases/available")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetPayrollAvailableCases")]
    public async Task<ActionResult<ApiObject.Case[]>> GetPayrollAvailableCasesAsync(int tenantId,
        int payrollId, [FromQuery][Required] PayrollCaseQuery query)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }

        // sync query with mandatory path parameters
        query.TenantId = tenantId;
        query.PayrollId = payrollId;

        return await GetPayrollAvailableCasesAsync(query);
    }

    /// <summary>
    /// Build case with fields and related cases
    /// </summary>
    /// <remarks>
    /// Request body contains array of case values (optional)
    /// Without the request body, this would be a GET method
    /// </remarks>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrollId">The payroll id</param>
    /// <param name="caseName">The case name</param>
    /// <param name="query">The case build query</param>
    /// <param name="caseChangeSetup">The case change setup (optional)</param>
    /// <returns>The created case set</returns>
    [HttpPost("{payrollId}/cases/build/{caseName}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("BuildPayrollCase")]
    [QueryIgnore]
    public override async Task<ActionResult<ApiObject.CaseSet>> BuildPayrollCaseAsync(int tenantId, int payrollId,
        string caseName, [FromQuery][Required] CaseBuildQuery query,
        [FromBody, ModelBinder(BinderType = typeof(OptionalModelBinder<ApiObject.CaseChangeSetup>))] ApiObject.CaseChangeSetup caseChangeSetup = null)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }

        return await base.BuildPayrollCaseAsync(tenantId, payrollId, caseName, query, caseChangeSetup);
    }

    #endregion

    #region Case Changes

    /// <summary>
    /// Query payroll case change values
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrollId">The payroll id</param>
    /// <param name="query">Query parameters</param>
    /// <returns>All payroll case change values</returns>
    [HttpGet("{payrollId}/changes/values")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("QueryPayrollCaseChangeValues")]
    public override async Task<ActionResult> QueryPayrollCaseChangeValuesAsync(int tenantId, int payrollId,
        [FromQuery] PayrollCaseChangeQuery query = null)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }

        return await base.QueryPayrollCaseChangeValuesAsync(tenantId, payrollId, query);
    }

    #endregion

    #region Case Values

    /// <summary>
    /// Get case values
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrollId">The payroll id</param>
    /// <param name="caseFieldNames">The case field names (default: all)</param>
    /// <param name="startDate">The time period start date</param>
    /// <param name="endDate">The time period end date</param>
    /// <param name="employeeId">The employee id, mandatory for employee case</param>
    /// <param name="caseSlot">The case slot</param>
    /// <returns>The payroll case values</returns>
    [HttpGet("{payrollId}/cases/values")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetPayrollCaseValues")]
    public async Task<ActionResult<ApiObject.CaseFieldValue[]>> GetPayrollCaseValuesAsync(int tenantId, int payrollId,
        [FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] string[] caseFieldNames,
        [FromQuery] int? employeeId = null, [FromQuery] string caseSlot = null)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }

        return await base.GetPayrollCaseValuesAsync(
            new()
            {
                TenantId = tenantId,
                PayrollId = payrollId,
                EmployeeId = employeeId
            },
            startDate, endDate, caseFieldNames, caseSlot);
    }

    /// <summary>
    /// Get payroll case values from a specific time moment
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="employeeId">The employee id, mandatory for employee case</param>
    /// <param name="payrollId">The payroll id</param>
    /// <param name="caseType">The case type</param>
    /// <param name="caseFieldNames">The case field names (default: all)</param>
    /// <param name="valueDate">The moment of the value (default: UTC now)</param>
    /// <param name="regulationDate">The regulation date (default: valueDate)</param>
    /// <param name="evaluationDate">The evaluation date (default: valueDate)</param>
    /// <returns>The payroll case value of the case field</returns>
    [HttpGet("{payrollId}/cases/values/time")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetPayrollTimeCaseValues")]
    public async Task<ActionResult<IEnumerable<ApiObject.CaseValue>>> GetPayrollTimeCaseValuesAsync(int tenantId,
        int payrollId, [FromQuery] CaseType caseType, [FromQuery] int? employeeId = null, [FromQuery] string[] caseFieldNames = null,
        [FromQuery] DateTime? valueDate = null,
        [FromQuery] DateTime? regulationDate = null, [FromQuery] DateTime? evaluationDate = null)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }

        return await base.GetPayrollTimeCaseValuesAsync(
            new()
            {
                TenantId = tenantId,
                PayrollId = payrollId,
                EmployeeId = employeeId,
                RegulationDate = regulationDate,
                EvaluationDate = evaluationDate
            },
            caseType, caseFieldNames, valueDate);
    }

    /// <summary>
    /// Get available case period values
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrollId">The payroll id</param>
    /// <param name="userId">The user id</param>
    /// <param name="caseFieldNames">The case field names</param>
    /// <param name="startDate">The time period start date</param>
    /// <param name="endDate">The time period end date</param>
    /// <param name="employeeId">The employee id, mandatory for employee case</param>
    /// <param name="regulationDate">The regulation date (default: UTC now)</param>
    /// <param name="evaluationDate">Creation date filter (default: UTC now)</param>
    /// <returns>Case period values, split by changed values</returns>
    [HttpGet("{payrollId}/cases/values/periods")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetPayrollAvailableCaseFieldValues")]
    public async Task<ActionResult<ApiObject.CaseFieldValue[]>> GetPayrollAvailableCaseFieldValuesAsync(int tenantId, int payrollId,
        [FromQuery][Required] int userId, [FromQuery][Required] string[] caseFieldNames, [FromQuery][Required] DateTime startDate,
        [FromQuery][Required] DateTime endDate, [FromQuery] int? employeeId = null, [FromQuery] DateTime? regulationDate = null,
        [FromQuery] DateTime? evaluationDate = null)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }

        return await base.GetPayrollAvailableCaseFieldValuesAsync(
            new()
            {
                TenantId = tenantId,
                PayrollId = payrollId,
                EmployeeId = employeeId,
                RegulationDate = regulationDate,
                EvaluationDate = evaluationDate
            },
            userId, caseFieldNames, startDate, endDate);
    }

    /// <summary>
    /// Add case change
    /// </summary>
    /// <remarks>
    /// Request body contains the case change
    /// </remarks>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrollId">The payroll id</param>
    /// <param name="caseChangeSetup">The case change setup</param>
    /// <returns>The case change setup with the created case values <see cref="CaseValueCreationMode"/>, including issues</returns>
    [HttpPost("{payrollId}/cases")]
    [CreatedResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("AddPayrollCase")]
    public override async Task<ActionResult<ApiObject.CaseChange>> AddPayrollCaseAsync(int tenantId, int payrollId,
        [FromBody][Required] ApiObject.CaseChangeSetup caseChangeSetup) =>
        await base.AddPayrollCaseAsync(tenantId, payrollId, caseChangeSetup);

    #endregion

    #region Payroll Regulation Items

    /// <summary>
    /// Get payroll cases
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrollId">The payroll id</param>
    /// <param name="caseType">The case type (default: all)</param>
    /// <param name="caseNames">The case names (case-insensitive, default: all)</param>
    /// <param name="overrideType">The override type filter (default: active)</param>
    /// <param name="clusterSetName">The cluster set name</param>
    /// <param name="regulationDate">The regulation date (default: UTC now)</param>
    /// <param name="evaluationDate">The evaluation date (default: UTC now)</param>
    /// <returns>Payroll cases</returns>
    [HttpGet("{payrollId}/cases")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetPayrollCases")]
    public async Task<ActionResult<ApiObject.Case[]>> GetPayrollCasesAsync(int tenantId, int payrollId,
        [FromQuery] CaseType? caseType, [FromQuery] string[] caseNames, [FromQuery] OverrideType? overrideType,
        [FromQuery] string clusterSetName, [FromQuery] DateTime? regulationDate, [FromQuery] DateTime? evaluationDate)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await base.GetPayrollCasesAsync(
            new()
            {
                TenantId = tenantId,
                PayrollId = payrollId,
                RegulationDate = regulationDate,
                EvaluationDate = evaluationDate
            },
            caseType, caseNames,
            overrideType, clusterSetName);
    }

    /// <summary>
    /// Get payroll case fields, sorted by order
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrollId">The payroll id</param>
    /// <param name="caseFieldNames">The case field names (case-insensitive, default: all)</param>
    /// <param name="overrideType">The override type filter (default: active)</param>
    /// <param name="clusterSetName">The cluster set name</param>
    /// <param name="regulationDate">The regulation date (default: UTC now)</param>
    /// <param name="evaluationDate">The evaluation date (default: UTC now)</param>
    /// <returns>Payroll case fields</returns>
    [HttpGet("{payrollId}/casefields")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetPayrollCaseFields")]
    public async Task<ActionResult<ApiObject.CaseField[]>> GetPayrollCaseFieldsAsync(int tenantId, int payrollId,
        [FromQuery] string[] caseFieldNames, [FromQuery] OverrideType? overrideType, [FromQuery] string clusterSetName,
        [FromQuery] DateTime? regulationDate, [FromQuery] DateTime? evaluationDate)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await base.GetPayrollCaseFieldsAsync(
            new()
            {
                TenantId = tenantId,
                PayrollId = payrollId,
                RegulationDate = regulationDate,
                EvaluationDate = evaluationDate
            },
            caseFieldNames, overrideType, clusterSetName);
    }

    /// <summary>
    /// Get payroll case relations, sorted by order
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrollId">The payroll id</param>
    /// <param name="sourceCaseName">The relation source case name (case-insensitive, default: all)</param>
    /// <param name="targetCaseName">The relation target case name (case-insensitive, default: all)</param>
    /// <param name="overrideType">The override type filter (default: active)</param>
    /// <param name="clusterSetName">The cluster set name</param>
    /// <param name="regulationDate">The regulation date (default: UTC now)</param>
    /// <param name="evaluationDate">The evaluation date (default: UTC now)</param>
    /// <returns>Payroll case relations</returns>
    [HttpGet("{payrollId}/caserelations")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetPayrollCaseRelations")]
    public async Task<ActionResult<ApiObject.CaseRelation[]>> GetPayrollCaseRelationsAsync(int tenantId, int payrollId,
        [FromQuery] string sourceCaseName, [FromQuery] string targetCaseName, [FromQuery] OverrideType? overrideType,
        [FromQuery] string clusterSetName, [FromQuery] DateTime? regulationDate, [FromQuery] DateTime? evaluationDate)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await base.GetPayrollCaseRelationsAsync(
            new()
            {
                TenantId = tenantId,
                PayrollId = payrollId,
                RegulationDate = regulationDate,
                EvaluationDate = evaluationDate
            },
            sourceCaseName, targetCaseName, overrideType, clusterSetName);
    }

    /// <summary>
    /// Get payroll wage types
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrollId">The payroll id</param>
    /// <param name="wageTypeNumbers">The wage type numbers (default: all)</param>
    /// <param name="overrideType">The override type filter (default: active)</param>
    /// <param name="clusterSetName">The cluster set name</param>
    /// <param name="regulationDate">The regulation date (default: UTC now)</param>
    /// <param name="evaluationDate">The evaluation date (default: UTC now)</param>
    /// <returns>Payroll wage types</returns>
    [HttpGet("{payrollId}/wagetypes")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetPayrollWageTypes")]
    public async Task<ActionResult<ApiObject.WageType[]>> GetPayrollWageTypesAsync(int tenantId, int payrollId,
        [FromQuery] decimal[] wageTypeNumbers, [FromQuery] OverrideType? overrideType, [FromQuery] string clusterSetName,
        [FromQuery] DateTime? regulationDate, [FromQuery] DateTime? evaluationDate)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await base.GetPayrollWageTypesAsync(
            new()
            {
                TenantId = tenantId,
                PayrollId = payrollId,
                RegulationDate = regulationDate,
                EvaluationDate = evaluationDate
            },
            wageTypeNumbers, overrideType, clusterSetName);
    }

    /// <summary>
    /// Get payroll collectors
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrollId">The payroll id</param>
    /// <param name="collectorNames">The collector names filter (case-insensitive, default is all)</param>
    /// <param name="overrideType">The override type filter (default: active)</param>
    /// <param name="clusterSetName">The cluster set name</param>
    /// <param name="regulationDate">The regulation date (default: UTC now)</param>
    /// <param name="evaluationDate">The evaluation date (default: UTC now)</param>
    /// <returns>Payroll collectors</returns>
    [HttpGet("{payrollId}/collectors")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetPayrollCollectors")]
    public async Task<ActionResult<ApiObject.Collector[]>> GetPayrollCollectorsAsync(int tenantId, int payrollId,
        [FromQuery] string[] collectorNames, [FromQuery] OverrideType? overrideType, [FromQuery] string clusterSetName,
        [FromQuery] DateTime? regulationDate, [FromQuery] DateTime? evaluationDate)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await base.GetPayrollCollectorsAsync(
            new()
            {
                TenantId = tenantId,
                PayrollId = payrollId,
                RegulationDate = regulationDate,
                EvaluationDate = evaluationDate
            },
            collectorNames, overrideType, clusterSetName);
    }

    /// <summary>
    /// Get payroll lookups
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrollId">The payroll id</param>
    /// <param name="lookupNames">The lookup names filter (case-insensitive, default is all)</param>
    /// <param name="overrideType">The override type filter (default: active)</param>
    /// <param name="regulationDate">The regulation date (default: UTC now)</param>
    /// <param name="evaluationDate">The evaluation date (default: UTC now)</param>
    /// <returns>Payroll lookups</returns>
    [HttpGet("{payrollId}/lookups")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetPayrollLookups")]
    public async Task<ActionResult<ApiObject.Lookup[]>> GetPayrollLookupsAsync(int tenantId, int payrollId,
        [FromQuery] string[] lookupNames, [FromQuery] OverrideType? overrideType,
        [FromQuery] DateTime? regulationDate, [FromQuery] DateTime? evaluationDate)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await base.GetPayrollLookupsAsync(
            new()
            {
                TenantId = tenantId,
                PayrollId = payrollId,
                RegulationDate = regulationDate,
                EvaluationDate = evaluationDate
            },
            lookupNames, overrideType);
    }

    /// <summary>
    /// Get payroll lookup values
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrollId">The payroll id</param>
    /// <param name="lookupNames">The lookup names filter (case-insensitive, default is all)</param>
    /// <param name="lookupKeys">The lookup-value keys filter (case-insensitive, default is all)</param>
    /// <param name="regulationDate">The regulation date (default: UTC now)</param>
    /// <param name="evaluationDate">The evaluation date (default: UTC now)</param>
    /// <returns>Payroll lookup values</returns>
    [HttpGet("{payrollId}/lookups/values")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetPayrollLookupValues")]
    public async Task<ActionResult<ApiObject.LookupValue[]>> GetPayrollLookupValuesAsync(int tenantId, int payrollId,
        [FromQuery] string[] lookupNames, [FromQuery] string[] lookupKeys,
        [FromQuery] DateTime? regulationDate, [FromQuery] DateTime? evaluationDate)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await base.GetPayrollLookupValuesAsync(
            new()
            {
                TenantId = tenantId,
                PayrollId = payrollId,
                RegulationDate = regulationDate,
                EvaluationDate = evaluationDate
            },
            lookupNames, lookupKeys);
    }

    /// <summary>
    /// Get payroll lookup data
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrollId">The payroll id</param>
    /// <param name="lookupNames">The lookup names (case-insensitive)</param>
    /// <param name="regulationDate">The regulation date (default: UTC now)</param>
    /// <param name="evaluationDate">The evaluation date (default: UTC now)</param>
    /// <param name="language">The content language</param>
    /// <returns>The lookup data</returns>
    [HttpGet("{payrollId}/lookups/data")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetPayrollLookupData")]
    public async Task<ActionResult<IEnumerable<ApiObject.LookupData>>> GetPayrollLookupDataAsync(int tenantId, int payrollId,
        [FromQuery][Required] string[] lookupNames, [FromQuery] DateTime? regulationDate,
        [FromQuery] DateTime? evaluationDate, [FromQuery] Language? language)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await base.GetPayrollLookupDataAsync(
            new()
            {
                TenantId = tenantId,
                PayrollId = payrollId,
                RegulationDate = regulationDate,
                EvaluationDate = evaluationDate
            },
            lookupNames, language);
    }

    /// <summary>
    /// Get payroll lookup value data
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrollId">The payroll id</param>
    /// <param name="lookupName">The lookup name (case-insensitive)</param>
    /// <param name="lookupKey">The lookup key, optionally with range value</param>
    /// <param name="rangeValue">The lookup range value</param>
    /// <param name="regulationDate">The regulation date (default: UTC now)</param>
    /// <param name="evaluationDate">The evaluation date (default: UTC now)</param>
    /// <param name="language">The language</param>
    /// <returns>The lookup value data</returns>
    [HttpGet("{payrollId}/lookups/values/data")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetPayrollLookupValueData")]
    public async Task<ActionResult<ApiObject.LookupValueData>> GetPayrollLookupValueDataAsync(int tenantId, int payrollId,
        [FromQuery][Required] string lookupName, [FromQuery] string lookupKey, [FromQuery] decimal? rangeValue,
        [FromQuery] DateTime? regulationDate, [FromQuery] DateTime? evaluationDate, [FromQuery] Language? language)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await base.GetPayrollLookupValueDataAsync(
            new()
            {
                TenantId = tenantId,
                PayrollId = payrollId,
                RegulationDate = regulationDate,
                EvaluationDate = evaluationDate
            },
            lookupName, lookupKey, rangeValue, language);
    }

    /// <summary>
    /// Get payroll reports
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrollId">The payroll id</param>
    /// <param name="reportNames">The report names filter (case-insensitive, default is all)</param>
    /// <param name="overrideType">The override type filter (default: active)</param>
    /// <param name="clusterSetName">The cluster set name</param>
    /// <param name="regulationDate">The regulation date (default: UTC now)</param>
    /// <param name="evaluationDate">The evaluation date (default: UTC now)</param>
    /// <returns>Payroll reports</returns>
    [HttpGet("{payrollId}/reports")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetPayrollReports")]
    public async Task<ActionResult<ApiObject.Report[]>> GetPayrollReportsAsync(int tenantId, int payrollId,
        [FromQuery] string[] reportNames, [FromQuery] OverrideType? overrideType, [FromQuery] string clusterSetName,
        [FromQuery] DateTime? regulationDate, [FromQuery] DateTime? evaluationDate)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await base.GetPayrollReportsAsync(
            new()
            {
                TenantId = tenantId,
                PayrollId = payrollId,
                RegulationDate = regulationDate,
                EvaluationDate = evaluationDate
            },
            reportNames, overrideType, clusterSetName);
    }

    /// <summary>
    /// Get payroll report parameters
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrollId">The payroll id</param>
    /// <param name="reportNames">The report names (case-insensitive, default is all)</param>
    /// <param name="regulationDate">The regulation date (default: UTC now)</param>
    /// <param name="evaluationDate">The evaluation date (default: UTC now)</param>
    /// <returns>Payroll report parameters</returns>
    [HttpGet("{payrollId}/reports/parameters")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetPayrollReportParameters")]
    public async Task<ActionResult<ApiObject.ReportParameter[]>> GetPayrollReportParametersAsync(int tenantId, int payrollId,
        [FromQuery] string[] reportNames, [FromQuery] DateTime? regulationDate, [FromQuery] DateTime? evaluationDate)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await base.GetPayrollReportParametersAsync(
            new()
            {
                TenantId = tenantId,
                PayrollId = payrollId,
                RegulationDate = regulationDate,
                EvaluationDate = evaluationDate
            },
            reportNames);
    }

    /// <summary>
    /// Get payroll report templates
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrollId">The payroll id</param>
    /// <param name="reportNames">The report names (case-insensitive, default is all)</param>
    /// <param name="language">The report language (default is all)</param>
    /// <param name="regulationDate">The regulation date (default: UTC now)</param>
    /// <param name="evaluationDate">The evaluation date (default: UTC now)</param>
    /// <returns>Payroll report templates</returns>
    [HttpGet("{payrollId}/reports/templates")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetPayrollReportTemplates")]
    public async Task<ActionResult<ApiObject.ReportTemplate[]>> GetPayrollReportTemplatesAsync(int tenantId, int payrollId,
        [FromQuery] string[] reportNames, [FromQuery] Language? language,
        [FromQuery] DateTime? regulationDate, [FromQuery] DateTime? evaluationDate)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await base.GetPayrollReportTemplatesAsync(
            new()
            {
                TenantId = tenantId,
                PayrollId = payrollId,
                RegulationDate = regulationDate,
                EvaluationDate = evaluationDate
            },
            reportNames, language);
    }

    /// <summary>
    /// Get payroll scripts
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrollId">The payroll id</param>
    /// <param name="scriptNames">The script names filter (case-insensitive, default is all)</param>
    /// <param name="overrideType">The override type filter (default: active)</param>
    /// <param name="regulationDate">The regulation date (default: UTC now)</param>
    /// <param name="evaluationDate">The evaluation date (default: UTC now)</param>
    /// <returns>Payroll scripts</returns>
    [HttpGet("{payrollId}/scripts")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetPayrollScripts")]
    public async Task<ActionResult<ApiObject.Script[]>> GetPayrollScriptsAsync(int tenantId, int payrollId,
        [FromQuery] string[] scriptNames, [FromQuery] OverrideType? overrideType,
        [FromQuery] DateTime? regulationDate, [FromQuery] DateTime? evaluationDate)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await base.GetPayrollScriptAsync(
            new()
            {
                TenantId = tenantId,
                PayrollId = payrollId,
                RegulationDate = regulationDate,
                EvaluationDate = evaluationDate
            },
            scriptNames, overrideType);
    }

    /// <summary>
    /// Get payroll script actions
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrollId">The payroll id</param>
    /// <param name="scriptNames">The script names filter (case-insensitive, default is all)</param>
    /// <param name="overrideType">The override type filter (default: active)</param>
    /// <param name="functionType">The function type (default: all)</param>
    /// <param name="regulationDate">The regulation date (default: UTC now)</param>
    /// <param name="evaluationDate">The evaluation date (default: UTC now)</param>
    /// <returns>Payroll scripts</returns>
    [HttpGet("{payrollId}/actions")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetPayrollScriptActions")]
    public async Task<ActionResult<ApiObject.ActionInfo[]>> GetPayrollScriptActionsAsync(int tenantId, int payrollId,
        [FromQuery] string[] scriptNames, [FromQuery] OverrideType? overrideType, [FromQuery] FunctionType functionType,
        [FromQuery] DateTime? regulationDate, [FromQuery] DateTime? evaluationDate)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await base.GetPayrollScriptActionsAsync(
            new()
            {
                TenantId = tenantId,
                PayrollId = payrollId,
                RegulationDate = regulationDate,
                EvaluationDate = evaluationDate
            },
            scriptNames, overrideType, functionType);
    }

    #endregion

    #region Attributes

    /// <summary>
    /// Get a payroll attribute
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrollId">The payroll id</param>
    /// <param name="attributeName">The attribute name</param>
    /// <returns>The attribute value as JSON</returns>
    [HttpGet("{payrollId}/attributes/{attributeName}")]
    [OkResponse]
    [NotFoundResponse]
    [ApiOperationId("GetPayrollAttribute")]
    public virtual async Task<ActionResult<string>> GetPayrollAttributeAsync(int tenantId, int payrollId, string attributeName)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await GetAttributeAsync(payrollId, attributeName);
    }

    /// <summary>
    /// Set a payroll attribute
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrollId">The payroll id</param>
    /// <param name="attributeName">The attribute name</param>
    /// <param name="value">The attribute value as JSON</param>
    /// <returns>The current attribute value as JSON</returns>
    [HttpPost("{payrollId}/attributes/{attributeName}")]
    [CreatedResponse]
    [NotFoundResponse]
    [UnprocessableEntityResponse]
    [ApiOperationId("SetPayrollAttribute")]
    public virtual async Task<ActionResult<string>> SetPayrollAttributeAsync(int tenantId, int payrollId, string attributeName,
        [FromBody] string value)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await base.SetAttributeAsync(payrollId, attributeName, value);
    }

    /// <summary>
    /// Delete a payroll attribute
    /// </summary>
    /// <param name="tenantId">The tenant id</param>
    /// <param name="payrollId">The payroll id</param>
    /// <param name="attributeName">The attribute name</param>
    /// <returns>True if the attribute was deleted</returns>
    [HttpDelete("{payrollId}/attributes/{attributeName}")]
    [ApiOperationId("DeletePayrollAttribute")]
    public virtual async Task<ActionResult<bool>> DeletePayrollAttributeAsync(int tenantId, int payrollId, string attributeName)
    {
        // tenant check
        var tenantResult = VerifyTenant(tenantId);
        if (tenantResult != null)
        {
            return tenantResult;
        }
        return await base.DeleteAttributeAsync(payrollId, attributeName);
    }

    #endregion

}