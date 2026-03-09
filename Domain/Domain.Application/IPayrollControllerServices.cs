using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;
using PayrollEngine.Domain.Application.Service;

namespace PayrollEngine.Domain.Application;

public interface IPayrollControllerServices
{
    IPayrollContextService Context { get; }
    Task<List<CaseValidationIssue>> ValidateCaseAsync(ValidateCaseSettings settings);

    Task<IEnumerable<CaseValue>> GetPayrollTimeCaseValuesAsync(PayrollQuery query, 
        CaseType caseType, string[] caseFieldNames = null, DateTime? valueDate = null);
}