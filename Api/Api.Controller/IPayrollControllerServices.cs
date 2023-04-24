using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PayrollEngine.Domain.Application.Service;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Api.Controller;

internal interface IPayrollControllerServices
{
    IPayrollContextService Context { get; }
    Task<List<CaseValidationIssue>> ValidateCaseAsync(ValidateCaseSettings settings);

    Task<ActionResult<IEnumerable<Model.CaseValue>>> GetPayrollTimeCaseValuesAsync(
        IDbContext context, PayrollQuery query, CaseType caseType, string[] caseFieldNames = null,
        DateTime? valueDate = null);
}