using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Domain.Application.Service;

namespace PayrollEngine.Api.Controller;

internal interface IPayrollControllerServices
{
    IPayrollContextService Context { get; }
    Task<List<Domain.Model.CaseValidationIssue>> ValidateCaseAsync(ValidateCaseSettings settings);

    Task<ActionResult<IEnumerable<Model.CaseValue>>> GetPayrollTimeCaseValuesAsync(
        Domain.Model.PayrollQuery query, CaseType caseType, string[] caseFieldNames = null,
        DateTime? valueDate = null);
}