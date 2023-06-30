using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PayrollEngine.Domain.Model;

namespace PayrollEngine.Domain.Application.Service;

public interface IPayrollControllerServices
{
    IPayrollContextService Context { get; }
    Task<List<CaseValidationIssue>> ValidateCaseAsync(ValidateCaseSettings settings);

    Task<IEnumerable<CaseValue>> GetPayrollTimeCaseValuesAsync(PayrollQuery query, 
        CaseType caseType, string[] caseFieldNames = null, DateTime? valueDate = null);
}